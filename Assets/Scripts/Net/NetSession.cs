using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sessio de sala online. Viu entre escenes (DontDestroyOnLoad).
//
// Com funciona (topologia en estrella):
//  - L'AMFITRIO crea la sala: obri un port TCP (7777) i, si la sala es de LAN, respon al descobriment UDP.
//    Si la sala es d'Internet, a mes es registra al servidor de sales, que li reenvia els jugadors.
//  - Cada JUGADOR parla nomes amb l'amfitrio. L'amfitrio decideix (qui entra, qui esta llest, quan es comença)
//    i reenvia a tots l'estat de la sala, el xat i les posicions.
public class NetSession : MonoBehaviour
{
    public enum Role { None, Host, Client }

    public class HostSettings
    {
        public string name = "Sala";
        public RoomMode mode = RoomMode.Dungeon;
        public string customMapName, customMapCode;
        public MapData customMapData;
        public int maxPlayers = 4;
        public bool isPublic = true;
        public string password = "";
        public bool internet;
    }

    public class JoinResult
    {
        public bool ok, needPassword;
        public RoomInfo target; // on hem intentat entrar (amb el codi real), per a tornar-ho a provar
        public string error, code;
    }

    public struct ChatLine { public int from; public string name; public string text; public bool system, localOnly; public float time; }

    static NetSession instance;
    public static NetSession Instance => instance;
    public static bool Active => instance != null && instance.role != Role.None;
    public static bool IsHost => Active && instance.role == Role.Host;

    // ---------- Estat public ----------
    public Role role;
    public RoomInfo Room = new RoomInfo();
    public readonly SortedDictionary<int, NetPlayer> Players = new SortedDictionary<int, NetPlayer>();
    public int LocalId;
    public int Seed;
    public float CountdownLeft => Room.phase == RoomPhase.Countdown ? Mathf.Max(0, countdownEnd - Time.realtimeSinceStartup) : 0;
    public readonly List<ChatLine> Chat = new List<ChatLine>();
    public string NetworkLabel = "";
    // Adreça real (IP local:port TCP obert) per a la connexió directa. Només a l'amfitrió i si el port s'ha obert.
    public string DirectAddress => role == Role.Host && listener != null ? NetProtocol.LocalIPv4() + ":" + gamePort : null;
    public int DirectPort => role == Role.Host && listener != null ? gamePort : 0;
    // Sala d'Internet sense IP (túnel + directori). null si no ho és.
    public bool IsCloudRoom => role == Role.Host ? cloudTunnel != null : role == Role.Client && cloudClient;
    public string CloudStatus { get; private set; }
    public bool HasCustomMap => !string.IsNullOrEmpty(customMapCode) && customMapData != null;
    public event Action Changed;
    public event Action<ChatLine> ChatReceived;
    public NetPlayer Local => Players.TryGetValue(LocalId, out var p) ? p : null;

    // Missatge a mostrar quan tornem al menu (p. ex. "L'amfitrio ha tancat la sala")
    public static string PendingMenuMessage;
    public static bool ReopenBrowser;

    // Els bots de prova no toquen les preferencies reals del jugador
    public static string ServerOverride, NameOverride;
    public static string ServerAddress
    {
        get => ServerOverride ?? (NetProtocol.LoopbackOnly
            ? "127.0.0.1:" + NetProtocol.RelayPort
            : PlayerPrefs.GetString("Net_Server", ""));
        set { if (ServerOverride != null || NetProtocol.LoopbackOnly) { ServerOverride = (value ?? "").Trim(); return; } PlayerPrefs.SetString("Net_Server", (value ?? "").Trim()); PlayerPrefs.Save(); }
    }
    public static string PlayerName
    {
        get { if (NameOverride != null) return NameOverride; var n = PlayerPrefs.GetString("PlayerName", "Usuario"); return string.IsNullOrWhiteSpace(n) ? "Usuario" : n; }
        set { if (NameOverride != null) { NameOverride = CleanName(value); return; } PlayerPrefs.SetString("PlayerName", CleanName(value)); PlayerPrefs.Save(); }
    }

    // ---------- Intern ----------
    readonly ConcurrentQueue<Action> mainThread = new ConcurrentQueue<Action>();
    class Peer { public INetLink link; public NetPlayer player; public float since, lastSeen; }
    readonly List<Peer> peers = new List<Peer>();
    TcpListener listener;
    Thread acceptThread;
    readonly ConcurrentQueue<TcpClient> accepted = new ConcurrentQueue<TcpClient>();
    volatile bool listening;
    LanDiscovery.Responder responder;
    TcpLink relay;
    readonly Dictionary<int, RelayPeerLink> relayPeers = new Dictionary<int, RelayPeerLink>();
    string passwordHash = "";
    string customMapName, customMapCode;
    MapData customMapData;
    int nextId = 2;
    float countdownEnd, nextPing, nextSnap, nextInfo, nextPos, lastFromHost;
    INetLink hostLink;
    bool roomDirty;
    int gamePort;
    readonly System.Random rng = new System.Random();
    Action<JoinResult> pendingJoin;
    string pendingCode, pendingPassword;
    // Internet sense IP
    BoreTunnel cloudTunnel;
    UPnPMapper upnpMapper;
    volatile string directPublicEndpoint;
    string upnpStatus;
    bool cloudClient, cloudPublishing, cloudOk;
    float nextCloud, lastCloud;
    string cloudSig;
    int cloudGen;

    static NetSession Ensure()
    {
        if (instance != null) return instance;
        var go = new GameObject("Sessio online");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<NetSession>();
        go.AddComponent<NetAvatars>();
        go.AddComponent<NetCoopRunner>();
        // Amb el joc en segon pla (alt-tab, dues finestres) la sala ha de seguir viva
        Application.runInBackground = true;
        return instance;
    }

    public static void RunOnMain(Action a) => Ensure().mainThread.Enqueue(a);

    void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    void OnApplicationQuit() { Leave(null); }

    public static string CleanName(string n, int max = 16)
    {
        if (n == null) return "Jugador";
        var sb = new System.Text.StringBuilder();
        foreach (char c in n.Trim()) if (!char.IsControl(c) && c != '<' && c != '>') sb.Append(c);
        var s = sb.ToString();
        if (s.Length > max) s = s.Substring(0, max);
        return s.Length == 0 ? "Jugador" : s;
    }
    public static string Safe(string s) => "<noparse>" + (s ?? "").Replace("</noparse>", "") + "</noparse>";

    // =====================================================================
    //  LLISTATS
    // =====================================================================
    public static void ListLan(Action<List<RoomInfo>> done)
    {
        Ensure();
        if (NetCloud.NoLan) { done(new List<RoomInfo>()); return; }
        new Thread(() =>
        {
            List<RoomInfo> rooms;
            try { rooms = LanDiscovery.Query(null, 700); } catch { rooms = new List<RoomInfo>(); }
            RunOnMain(() => done(rooms));
        }) { IsBackground = true }.Start();
    }

    public static void ListInternet(Action<List<RoomInfo>, string> done)
    {
        var self = Ensure();
        if (NetCloud.InUse) { self.StartCoroutine(CloudList(done)); return; } // sense servidor propi: directori públic
        string server = ServerAddress;
        if (!NetProtocol.TryParseEndpoint(server, NetProtocol.RelayPort, out var host, out var port))
        {
            done(new List<RoomInfo>(), "Encara no has configurat cap servidor de sales.");
            return;
        }
        new Thread(() =>
        {
            var list = new List<RoomInfo>(); string error = null;
            TcpLink link = null;
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                link = TcpLink.Connect(host, port, 3000);
                link.Send(NetProtocol.Frame(Msg.RelayList));
                var frame = Wait(link, 3000);
                int ping = (int)sw.ElapsedMilliseconds / 2;
                if (frame == null) throw new IOException("El servidor no ha respost.");
                var r = NetProtocol.Reader(frame, out var type);
                if (type != Msg.RelayRooms) throw new IOException("Resposta inesperada del servidor.");
                int n = r.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    var info = RoomInfo.Read(r);
                    info.viaRelay = true; info.address = host; info.port = port; info.pingMs = ping;
                    list.Add(info);
                }
            }
            catch (Exception e) { error = "No es pot connectar amb el servidor " + server + " (" + e.Message + ")"; }
            finally { link?.Close(); }
            RunOnMain(() => done(list, error));
        }) { IsBackground = true }.Start();
    }

    // Llista de sales públiques d'Internet (directori + ping estimat: jo -> túnel -> amfitrió)
    static IEnumerator CloudList(Action<List<RoomInfo>, string> done)
    {
        var rtt = new int[] { -2 };
        new Thread(() => rtt[0] = NetCloud.MeasureTunnelRtt()) { IsBackground = true }.Start();
        List<RoomInfo> rooms = null; string error = null;
        yield return NetCloud.Fetch(NetCloud.PublicTopic, (r, e) => { rooms = r; error = e; });
        float until = Time.realtimeSinceStartup + 3f;
        while (rtt[0] == -2 && Time.realtimeSinceStartup < until) yield return null;
        int mine = rtt[0];
        rooms = rooms.FindAll(r => r.isPublic);
        foreach (var r in rooms) r.pingMs = mine < 0 ? -1 : r.pingMs >= 0 ? r.pingMs + mine : mine * 2;
        if (error == null && mine < 0) error = "No es pot arribar a cap servei de connexió (" + string.Join(", ", NetCloud.TunnelHosts) + "). Prova XARXA LOCAL (LAN) o unir-te per IP. Si falla la llista, comprova també ntfy.sh.";
        done(rooms, error);
    }

    // Busca una sala d'Internet pel codi: primer entre les públiques i després en el canal del codi (privades)
    static IEnumerator CloudFind(string code, Action<RoomInfo, string> done)
    {
        RoomInfo found = null; string error = null;
        // El directori tarda uns segons a servir un anunci acabat de publicar: ho tornem a provar abans de rendir-nos
        for (int attempt = 0; attempt < 5 && found == null; attempt++)
        {
            if (attempt > 0) yield return new WaitForSecondsRealtime(2f);
            error = null;
            yield return NetCloud.Fetch(NetCloud.PublicTopic, (r, e) => { error = e; found = r.Find(x => x.code == code); });
            if (found == null)
                yield return NetCloud.Fetch(NetCloud.CodeTopic(code), (r, e) => { error = error ?? e; found = r.Find(x => x.code == code); });
        }
        done(found, found == null ? error : null);
    }

    static byte[] Wait(INetLink link, int ms)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < ms)
        {
            if (link.Incoming.TryDequeue(out var f)) return f;
            if (!link.Connected) return null;
            Thread.Sleep(5);
        }
        return null;
    }

    // =====================================================================
    //  CREAR SALA (amfitrio)
    // =====================================================================
    public static void HostRoom(HostSettings s, Action<string> done)
    {
        var self = Ensure();
        if (Active) self.Leave(null);
        s.name = CleanName(string.IsNullOrWhiteSpace(s.name) ? "Sala de " + PlayerName : s.name, 24);
        if (s.mode == RoomMode.CustomMap)
        {
            string mapError;
            if (s.customMapData == null)
            {
                if (string.IsNullOrWhiteSpace(s.customMapName)) { done("Tria un mapa creat abans de crear la sala."); return; }
                if (!MapStore.Load(s.customMapName, out s.customMapData, out mapError))
                { done("No s'ha pogut carregar el mapa creat: " + mapError); return; }
            }
            if (!MapStore.Validate(s.customMapData, out mapError))
            { done("El mapa creat no és vàlid: " + mapError); return; }
            if (!MapShare160.Encode(s.customMapData, out s.customMapCode, out mapError))
            { done("Este mapa no es pot enviar als companys: " + mapError); return; }
        }
        s.maxPlayers = Mathf.Clamp(s.maxPlayers, 2, NetProtocol.MaxPlayers);
        var info = new RoomInfo { name = s.name, hostName = PlayerName, mode = s.mode, phase = RoomPhase.Lobby, players = 1, maxPlayers = s.maxPlayers, hasPassword = !string.IsNullOrEmpty(s.password), isPublic = s.isPublic };

        if (!s.internet)
        {
            info.code = NetProtocol.NewCode(self.rng);
            self.StartHost(s, info, null, done);
            return;
        }
        if (NetCloud.InUse)
        {
            // Internet sense IP: obrim el túnel i, quan tinga port públic, creem la sala com sempre
            info.code = NetProtocol.NewCode(self.rng);
            BoreTunnel tunnel = null;
            tunnel = new BoreTunnel(c => self.accepted.Enqueue(c));
            tunnel.Start();
            new Thread(() =>
            {
                bool ok = tunnel.WaitReady(9000);
                RunOnMain(() =>
                {
                    if (!ok)
                    {
                        string why = tunnel.Error;
                        tunnel.Stop();
                        done("No es pot arribar a cap servei de connexió (túnels: " + string.Join(", ", NetCloud.TunnelHosts) + "). Prova XARXA LOCAL (LAN) o unir-te per IP.");
                        return;
                    }
                    self.StartHost(s, info, null, done, tunnel);
                });
            }) { IsBackground = true }.Start();
            return;
        }
        if (!NetProtocol.TryParseEndpoint(ServerAddress, NetProtocol.RelayPort, out var host, out var port))
        {
            done("Per a crear sales d'Internet cal configurar el servidor de sales.");
            return;
        }
        new Thread(() =>
        {
            TcpLink link = null; string error = null; string code = null;
            try
            {
                link = TcpLink.Connect(host, port, 4000);
                link.Send(NetProtocol.Frame(Msg.RelayHost, w => info.Write(w)));
                var frame = Wait(link, 4000);
                if (frame == null) throw new IOException("el servidor no respon");
                var r = NetProtocol.Reader(frame, out var type);
                if (type == Msg.RelayHostOk) code = r.ReadString();
                else if (type == Msg.RelayFail) error = r.ReadString();
                else error = "Resposta inesperada del servidor.";
            }
            catch (Exception e) { error = "No s'ha pogut registrar la sala al servidor (" + e.Message + ")."; }
            if (error != null) link?.Close();
            RunOnMain(() =>
            {
                if (error != null) { done(error); return; }
                info.code = code;
                self.StartHost(s, info, link, done);
            });
        }) { IsBackground = true }.Start();
    }

    void StartHost(HostSettings s, RoomInfo info, TcpLink relayLink, Action<string> done, BoreTunnel tunnel = null)
    {
        ResetState();
        customMapName = s.customMapName; customMapCode = s.customMapCode; customMapData = s.customMapData;
        cloudTunnel = tunnel; cloudSig = null; cloudOk = false; cloudPublishing = false; nextCloud = 0; lastCloud = -99;
        // Port TCP per a la LAN / IP directa
        for (int p = NetProtocol.GamePort; p <= NetProtocol.GamePort + 10 && listener == null; p++)
        {
            try { var l = new TcpListener(NetProtocol.ListenAddress, p); l.Start(); listener = l; gamePort = p; }
            catch { }
        }
        if (listener == null && relayLink == null && tunnel == null) { done("No s'ha pogut obrir cap port (7777-7787). Tens una altra sala oberta?"); return; }
        if (listener != null)
        {
            listening = true;
            acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "Host accept" };
            acceptThread.Start();
            if (tunnel != null && !NetCloud.IsBot)
            {
                new Thread(() =>
                {
                    string why;
                    var mapping = UPnPMapper.TryOpen(gamePort, out why);
                    RunOnMain(() =>
                    {
                        if (role != Role.Host || listener == null) { mapping?.Close(); return; }
                        upnpMapper = mapping;
                        directPublicEndpoint = mapping?.Endpoint;
                        upnpStatus = mapping == null ? "Connexió directa no disponible: " + why : "Connexió directa preparada";
                        cloudSig = null;
                        nextCloud = 0;
                    });
                }) { IsBackground = true, Name = "Canica UPnP" }.Start();
            }
        }
        relay = relayLink;
        role = Role.Host;
        Room = info;
        passwordHash = NetProtocol.HashPassword(info.code, s.password);
        LocalId = 1;
        Players[1] = MakeLocalPlayer(1, true);
        Players[1].ready = true;
        responder = new LanDiscovery.Responder { gamePort = gamePort };
        responder.Update(Clone(info));
        if (listener != null && !NetCloud.NoLan) responder.Start();
        NetworkLabel = cloudTunnel != null ? "INTERNET · sala pública, sense IP" : relay != null ? "INTERNET · servidor " + ServerAddress : "XARXA LOCAL · " + NetProtocol.LocalIPv4() + ":" + gamePort;
        CloudStatus = cloudTunnel != null ? "Publicant la sala a Internet..." : null;
        AddSystem(cloudTunnel != null
            ? "Sala oberta a Internet. Els teus amics la veuran a INTERNET · PÚBLIQUES o hi entraran amb el codi " + info.code + ". Si UPnP funciona, primer provaran connexió directa; si no, usaran el túnel."
            : "Has creat la sala. Comparteix el codi " + info.code + " amb els teus amics.");
        done(null);
        SceneManager.LoadScene("Lobby");
    }

    void AcceptLoop()
    {
        while (listening)
        {
            try { accepted.Enqueue(listener.AcceptTcpClient()); }
            catch { if (!listening) break; }
        }
    }

    static RoomInfo Clone(RoomInfo i) => new RoomInfo { code = i.code, name = i.name, hostName = i.hostName, mode = i.mode, phase = i.phase, players = i.players, maxPlayers = i.maxPlayers, hasPassword = i.hasPassword, isPublic = i.isPublic, version = i.version };

    NetPlayer MakeLocalPlayer(int id, bool host)
    {
        var db = CosmeticsDatabase.Instance;
        return new NetPlayer
        {
            id = id, name = CleanName(PlayerName), isHost = host,
            skin = db != null && db.skins != null ? CosmeticsDatabase.Get(CosmeticsDatabase.KeySkin, db.skins.Length) : 0,
            hat = db != null && db.hats != null ? CosmeticsDatabase.Get(CosmeticsDatabase.KeyHat, db.hats.Length) : 0,
            trail = db != null && db.trails != null ? CosmeticsDatabase.Get(CosmeticsDatabase.KeyTrail, db.trails.Length) : 0,
        };
    }

    // =====================================================================
    //  ENTRAR A UNA SALA (client)
    // =====================================================================
    public static void Join(RoomInfo target, string password, Action<JoinResult> done)
    {
        var self = Ensure();
        if (Active) self.Leave(null);
        self.pendingJoin = done; self.pendingCode = target.code; self.pendingPassword = password ?? "";
        int gen = ++self.joinGen;
        new Thread(() =>
        {
            INetLink link = null; string error = null;
            try
            {
                if (target.viaRelay)
                {
                    if (!NetProtocol.TryParseEndpoint(ServerAddress, NetProtocol.RelayPort, out var h, out var p)) throw new IOException("servidor de sales no configurat");
                    var tcp = TcpLink.Connect(h, p, 4000);
                    tcp.Send(NetProtocol.Frame(Msg.RelayJoin, w => w.Write(target.code)));
                    var frame = Wait(tcp, 4000);
                    if (frame == null) { tcp.Close(); throw new IOException("el servidor no respon"); }
                    var r = NetProtocol.Reader(frame, out var type);
                    if (type == Msg.RelayFail) { error = r.ReadString(); tcp.Close(); }
                    else if (type != Msg.RelayJoinOk) { error = "Resposta inesperada del servidor."; tcp.Close(); }
                    else link = tcp;
                }
                else if (!string.IsNullOrEmpty(target.directAddress))
                {
                    try { link = TcpLink.Connect(target.directAddress, target.directPort, 4000); }
                    catch
                    {
                        if (target.viaCloud && !string.IsNullOrEmpty(target.address)) link = TcpLink.Connect(target.address, target.port, 4000);
                        else throw;
                    }
                }
                else link = TcpLink.Connect(target.address, target.port, target.viaCloud ? 4000 : 4000);
            }
            catch (Exception e) { error = "No s'ha pogut connectar amb la sala (" + e.Message + ")."; }
            RunOnMain(() =>
            {
                if (gen != self.joinGen) { link?.Close(); return; } // cancel·lat
                if (error != null) { self.FinishJoin(new JoinResult { error = error }); return; }
                self.BeginClient(link, target);
            });
        }) { IsBackground = true }.Start();
    }

    // Codi de 6 lletres (LAN o Internet) o adreça IP:port
    public static void JoinByCode(string input, string password, Action<JoinResult> done)
    {
        Ensure();
        input = (input ?? "").Trim();
        if (NetProtocol.LooksLikeAddress(input))
        {
            if (!NetProtocol.TryParseEndpoint(input, NetProtocol.GamePort, out var h, out var p)) { done(new JoinResult { error = "L'adreça no és vàlida. Exemple: 192.168.1.20:7777" }); return; }
            Join(new RoomInfo { address = h, port = p, code = "" }, password, done);
            return;
        }
        string code = NetProtocol.NormalizeCode(input);
        if (code.Length != 6) { done(new JoinResult { error = "Els codis de sala tenen 6 caràcters (p. ex. K7QX2M)." }); return; }
        int gen = instance.joinGen;
        new Thread(() =>
        {
            List<RoomInfo> found;
            try { found = NetCloud.NoLan ? new List<RoomInfo>() : LanDiscovery.Query(code, 600); } catch { found = new List<RoomInfo>(); }
            RunOnMain(() =>
            {
                if (gen != instance.joinGen) return; // cancel·lat
                if (found.Count > 0) { Join(found[0], password, done); return; }
                if (NetProtocol.TryParseEndpoint(ServerAddress, NetProtocol.RelayPort, out _, out _)) { Join(new RoomInfo { code = code, viaRelay = true }, password, done); return; }
                // Sense servidor propi: busquem el codi al directori d'Internet
                instance.StartCoroutine(CloudFind(code, (room, error) =>
                {
                    if (gen != instance.joinGen) return;
                    if (room != null) { Join(room, password, done); return; }
                    done(new JoinResult { error = error ?? "No hi ha cap sala amb el codi " + code + " ni a la teua xarxa ni a Internet. Comprova el codi (o que la sala continue oberta)." });
                }));
            });
        }) { IsBackground = true }.Start();
    }

    void BeginClient(INetLink link, RoomInfo target)
    {
        ResetState();
        hostLink = link;
        role = Role.Client;
        Room = Clone(target);
        lastTarget = target;
        lastFromHost = Time.realtimeSinceStartup;
        cloudClient = target.viaCloud;
        NetworkLabel = target.viaCloud ? "INTERNET · sala pública" : target.viaRelay ? "INTERNET · servidor " + ServerAddress : "XARXA LOCAL · " + target.address + ":" + target.port;
        var me = MakeLocalPlayer(0, false);
        string hash = NetProtocol.HashPassword(target.code ?? "", pendingPassword);
        link.Send(NetProtocol.Frame(Msg.Hello, w =>
        {
            w.Write(NetProtocol.Version); w.Write(GameVersion.Package); w.Write(me.name);
            me.WriteCosmetics(w); w.Write(hash);
        }));
        welcomed = false;
    }
    bool welcomed;
    int joinGen;
    RoomInfo lastTarget;

    // El jugador ha premut "Cancel·lar" mentre connectava
    public static void CancelJoin()
    {
        if (instance == null) return;
        instance.joinGen++;
        instance.pendingJoin = null;
        if (instance.role == Role.Client && !instance.welcomed) instance.Leave(null);
    }

    void FinishJoin(JoinResult r)
    {
        var cb = pendingJoin; pendingJoin = null;
        cb?.Invoke(r);
    }

    // =====================================================================
    //  ACCIONS
    // =====================================================================
    public void SetReady(bool ready)
    {
        if (role == Role.Client) hostLink?.Send(NetProtocol.Frame(Msg.Ready, w => w.Write(ready)));
        var me = Local; if (me != null && role == Role.Client) { me.ready = ready; Changed?.Invoke(); }
    }

    public void SendChat(string text)
    {
        text = (text ?? "").Trim();
        if (text.Length == 0) return;
        if (text.Length > 120) text = text.Substring(0, 120);
        if (role == Role.Host) HostChat(LocalId, text);
        else if (role == Role.Client) hostLink?.Send(NetProtocol.Frame(Msg.Chat, w => w.Write(text)));
    }

    public void SetMode(RoomMode mode)
    {
        if (role != Role.Host || Room.phase != RoomPhase.Lobby || Room.mode == mode) return;
        if (mode == RoomMode.CustomMap && !HasCustomMap) return;
        Room.mode = mode;
        AddSystem("Mode de joc: " + RoomModes.Short(mode) + ".", true);
        roomDirty = true;
    }

    public void Kick(int id)
    {
        if (role != Role.Host) return;
        var peer = peers.Find(p => p.player != null && p.player.id == id);
        if (peer == null) return;
        peer.link.Send(NetProtocol.Frame(Msg.Kicked, w => w.Write("L'amfitrió t'ha expulsat de la sala.")));
        AddSystem(peer.player.name + " ha sigut expulsat.", true);
        RemovePeer(peer, false);
    }

    public bool EveryoneReady
    {
        get { foreach (var p in Players.Values) if (!p.isHost && !p.ready) return false; return true; }
    }
    public int ReadyCount { get { int n = 0; foreach (var p in Players.Values) if (p.isHost || p.ready) n++; return n; } }

    public void StartCountdown()
    {
        if (role != Role.Host || Room.phase != RoomPhase.Lobby || !EveryoneReady) return;
        Room.phase = RoomPhase.Countdown;
        countdownEnd = Time.realtimeSinceStartup + 3f;
        AddSystem("La partida comença en 3 segons!", true);
        roomDirty = true;
    }

    public void CancelCountdown(string why)
    {
        if (role != Role.Host || Room.phase != RoomPhase.Countdown) return;
        Room.phase = RoomPhase.Lobby;
        AddSystem(why, true);
        roomDirty = true;
    }

    public void Leave(string messageForMenu)
    {
        if (role == Role.None) return;
        if (role == Role.Host)
        {
            var bye = NetProtocol.Frame(Msg.Closed, w => w.Write("L'amfitrió ha tancat la sala."));
            foreach (var p in peers) { p.link.Send(bye); }
            Thread.Sleep(30);
            foreach (var p in peers) p.link.Close();
        }
        else if (hostLink != null)
        {
            hostLink.Send(NetProtocol.Frame(Msg.Leave));
            Thread.Sleep(20);
            hostLink.Close();
        }
        ResetState();
        if (messageForMenu != null) PendingMenuMessage = messageForMenu;
        Changed?.Invoke();
    }

    void ResetState()
    {
        MapPlaytest.ClearNetworkMap();
        customMapName = customMapCode = null; customMapData = null;
        RunStats.DontSave = false;
        upnpMapper?.Close(); upnpMapper = null; directPublicEndpoint = null; upnpStatus = null;
        if (cloudTunnel != null)
        {
            // Avisa el directori que la sala tanca (així desapareix de la llista al moment)
            if (cloudSig != null && role == Role.Host && Room != null && !string.IsNullOrEmpty(Room.code))
            {
                var closed = NetCloud.FromRoom(Clone(Room), Mathf.Max(1024, cloudTunnel.PublicPort), -1, true);
                StartCoroutine(NetCloud.Publish(Room.isPublic ? NetCloud.PublicTopic : NetCloud.CodeTopic(Room.code), closed));
            }
            cloudTunnel.Stop(); cloudTunnel = null;
        }
        cloudClient = false; CloudStatus = null; cloudSig = null;
        listening = false;
        try { listener?.Stop(); } catch { }
        listener = null;
        while (accepted.TryDequeue(out var c)) try { c.Close(); } catch { }
        responder?.Stop(); responder = null;
        relay?.Close(); relay = null; relayPeers.Clear();
        peers.Clear();
        hostLink = null;
        role = Role.None;
        Players.Clear();
        Chat.Clear();
        LocalId = 0; Seed = 0; nextId = 2;
        Room = new RoomInfo();
        welcomed = false;
    }

    // Es crida quan l'amfitrio tanca, ens expulsen o es perd la connexio
    void Disconnected(string reason)
    {
        bool wasInRoom = role != Role.None && welcomedOrHost;
        Leave(null);
        if (pendingJoin != null) { FinishJoin(new JoinResult { error = reason }); return; }
        if (!wasInRoom) return;
        PendingMenuMessage = reason;
        ReopenBrowser = true;
        if (SceneManager.GetActiveScene().name != "MainMenu") SceneManager.LoadScene("MainMenu");
        else NetToast.Show(reason);
    }
    bool welcomedOrHost => role == Role.Host || welcomed;

    // v1.4.1: línia de xat només per a mi (el mercader parla a cada jugador per separat)
    public void LocalNotice(string text) => AddSystem(text, false);

    void AddSystem(string text, bool broadcast = false)
    {
        var line = new ChatLine { from = 0, name = "", text = text, system = true, localOnly = !broadcast, time = Time.realtimeSinceStartup };
        PushChat(line);
        if (broadcast && role == Role.Host)
            SendAll(NetProtocol.Frame(Msg.ChatOut, w => { w.Write(0); w.Write(""); w.Write(text); }));
    }

    void PushChat(ChatLine line)
    {
        Chat.Add(line);
        if (Chat.Count > 60) Chat.RemoveAt(0);
        ChatReceived?.Invoke(line);
    }

    // =====================================================================
    //  BUCLE
    // =====================================================================
    void Update()
    {
        while (mainThread.TryDequeue(out var a))
        {
            try { a(); } catch (Exception e) { Debug.LogException(e); }
        }
        if (role == Role.Host) HostUpdate();
        else if (role == Role.Client) ClientUpdate();
    }

    // ---------- Amfitrio ----------
    void HostUpdate()
    {
        float now = Time.realtimeSinceStartup;
        while (accepted.TryDequeue(out var c))
        {
            if (peers.Count >= 32) { try { c.Close(); } catch { } continue; }
            peers.Add(new Peer { link = new TcpLink(c), since = now, lastSeen = now });
        }

        if (relay != null)
        {
            while (relay.Incoming.TryDequeue(out var frame)) HandleRelay(frame, now);
            if (!relay.Connected)
            {
                relay = null;
                foreach (var rp in relayPeers.Values) rp.open = false;
                relayPeers.Clear();
                AddSystem("S'ha perdut la connexió amb el servidor de sales. La sala continua per a la xarxa local.", true);
                NetworkLabel = listener != null ? "XARXA LOCAL · " + NetProtocol.LocalIPv4() + ":" + gamePort : "SENSE CONNEXIÓ";
                roomDirty = true;
            }
        }

        for (int i = peers.Count - 1; i >= 0; i--)
        {
            var p = peers[i];
            int guard = 0;
            while (guard++ < 200 && p.link.Incoming.TryDequeue(out var frame))
            {
                p.lastSeen = now;
                try { HandleFromPeer(p, frame); }
                catch (Exception e) { Debug.LogWarning("Missatge invalid de " + p.link.Describe + ": " + e.Message); RemovePeer(p, true); break; }
            }
            if (!peers.Contains(p)) continue;
            if (!p.link.Connected) { RemovePeer(p, true); continue; }
            if (p.player == null && now - p.since > 10) { p.link.Close(); peers.Remove(p); continue; }
            if (p.player != null && now - p.lastSeen > 12) { AddSystem(p.player.name + " ha perdut la connexió.", true); RemovePeer(p, false); }
        }

        if (Room.phase == RoomPhase.Countdown)
        {
            if (!EveryoneReady) CancelCountdown("Compte enrere cancel·lat: algú ja no està llest.");
            else if (now >= countdownEnd) LaunchGame();
        }

        if (now >= nextPing)
        {
            nextPing = now + 1f;
            long t = DateTime.UtcNow.Ticks;
            var ping = NetProtocol.Frame(Msg.Ping, w => w.Write(t));
            foreach (var p in peers) if (p.player != null) p.link.Send(ping);
            roomDirty = true; // envia pings actualitzats
        }

        SampleLocal(LocalId);

        if (now >= nextSnap)
        {
            nextSnap = now + 0.05f;
            var snap = NetProtocol.Frame(Msg.Snap, w =>
            {
                double tnow = Time.realtimeSinceStartupAsDouble; bool Fresh(NetPlayer q) => q.scene >= 0 && tnow - q.lastPos < 1.0; // sense posicions recents (caigut, carregant) no s'envia
                int count = 0; foreach (var pl in Players.Values) if (Fresh(pl)) count++;
                w.Write((byte)count);
                foreach (var pl in Players.Values) if (Fresh(pl)) { w.Write(pl.id); pl.WritePosition(w); }
            });
            SendAll(snap);
        }

        Room.players = Players.Count;
        if (roomDirty)
        {
            roomDirty = false;
            var state = RoomFrame();
            SendAll(state);
            Changed?.Invoke();
        }
        if (now >= nextInfo)
        {
            nextInfo = now + 2f;
            var copy = Clone(Room);
            responder?.Update(copy);
            relay?.Send(NetProtocol.Frame(Msg.RelayUpdate, w => copy.Write(w)));
        }
        if (cloudTunnel != null) CloudHostTick(now);
    }

    // Anuncia la sala al directori: cada 20 s per a seguir a la llista, i abans si canvia alguna cosa
    // (jugadors, fase, port nou del túnel). Mai més d'un anunci cada 4 s, per respectar el servei gratuït.
    void CloudHostTick(float now)
    {
        int port = cloudTunnel.PublicPort;
        if (port <= 0)
        {
            CloudStatus = "Reconnectant amb Internet...";
            return;
        }
        string sig = port + "|" + Room.players + "|" + (int)Room.mode + "|" + (int)Room.phase + "|" + Room.maxPlayers + "|" + Room.isPublic + "|" + Room.name;
        bool changed = sig != cloudSig;
        if (!cloudPublishing && (now >= nextCloud || (changed && now - lastCloud >= 4f)))
        {
            cloudSig = sig; lastCloud = now; nextCloud = now + 20f; cloudPublishing = true;
            int gen = ++cloudGen;
            var a = NetCloud.FromRoom(Clone(Room), port, cloudTunnel.Rtt, false, cloudTunnel.Host, directPublicEndpoint);
            string topic = Room.isPublic ? NetCloud.PublicTopic : NetCloud.CodeTopic(Room.code);
            StartCoroutine(NetCloud.Publish(topic, a, ok =>
            {
                if (gen != cloudGen || cloudTunnel == null) return;
                cloudPublishing = false; cloudOk = ok;
                if (!ok) { nextCloud = Time.realtimeSinceStartup + 6f; cloudSig = null; }
            }));
        }
        CloudStatus = cloudOk
            ? (Room.isPublic ? "A Internet: surt a la llista de sales públiques" : "A Internet: privada, només amb el codi")
            : "Publicant la sala a Internet...";
    }

    void HandleRelay(byte[] frame, float now)
    {
        var r = NetProtocol.Reader(frame, out var type);
        switch (type)
        {
            case Msg.RelayPeerJoin:
            {
                int id = r.ReadInt32();
                var link = new RelayPeerLink(relay, id);
                relayPeers[id] = link;
                peers.Add(new Peer { link = link, since = now, lastSeen = now });
                break;
            }
            case Msg.RelayPeerData:
            {
                int id = r.ReadInt32(); int len = r.ReadInt32(); var data = r.ReadBytes(len);
                if (relayPeers.TryGetValue(id, out var link)) link.Incoming.Enqueue(data);
                break;
            }
            case Msg.RelayPeerLeave:
            {
                int id = r.ReadInt32();
                if (relayPeers.TryGetValue(id, out var link)) { link.open = false; relayPeers.Remove(id); }
                break;
            }
        }
    }

    void HandleFromPeer(Peer p, byte[] frame)
    {
        var r = NetProtocol.Reader(frame, out var type);
        if (p.player == null)
        {
            if (type != Msg.Hello) return;
            int version = r.ReadInt32(); string package = r.ReadString(); string name = CleanName(r.ReadString());
            int skin = 0, hat = 0, trail = 0;
            string hash = "";
            if (version == NetProtocol.Version)
            {
                NetPlayer.ReadCosmetics(r, out skin, out hat, out trail);
                hash = r.ReadString();
            }
            string reject = null; int kind = 0;
            if (version != NetProtocol.Version) { reject = "Versió diferent: la sala és " + GameVersion.Label + " i tu tens " + package + " (protocols " + NetProtocol.Version + " i " + version + ")."; kind = 3; }
            else if (Room.phase != RoomPhase.Lobby) { reject = "La partida ja ha començat. Espera que tornen al lobby."; kind = 4; }
            else if (Players.Count >= Room.maxPlayers) { reject = "La sala està plena (" + Room.maxPlayers + "/" + Room.maxPlayers + ")."; kind = 2; }
            else if (passwordHash.Length > 0 && hash != passwordHash) { reject = hash.Length == 0 ? "Aquesta sala té contrasenya." : "Contrasenya incorrecta."; kind = 1; }
            if (reject != null)
            {
                p.link.Send(NetProtocol.Frame(Msg.Reject, w => { w.Write(reject); w.Write((byte)kind); w.Write(Room.code); }));
                var dead = p; peers.Remove(p);
                StartCoroutine(CloseLater(dead.link));
                return;
            }
            name = UniqueName(name);
            var pl = new NetPlayer { id = nextId++, name = name, skin = skin, hat = hat, trail = trail };
            p.player = pl; Players[pl.id] = pl;
            p.link.Send(NetProtocol.Frame(Msg.Welcome, w => w.Write(pl.id)));
            // L'historial recent del xat, perque no entre "a cegues"
            foreach (var line in Chat) { if (line.localOnly) continue; var l = line; p.link.Send(NetProtocol.Frame(Msg.ChatOut, w => { w.Write(l.from); w.Write(l.name); w.Write(l.text); })); }
            AddSystem(name + " ha entrat a la sala.", true);
            roomDirty = true;
            return;
        }

        switch (type)
        {
            case Msg.Ready: p.player.ready = r.ReadBoolean(); roomDirty = true; break;
            case Msg.Chat: HostChat(p.player.id, r.ReadString()); break;
            case Msg.Pos: p.player.ReadPosition(r, Time.realtimeSinceStartupAsDouble); break;
            case Msg.Pong: { long t = r.ReadInt64(); p.player.ping = (int)Math.Max(0, (DateTime.UtcNow.Ticks - t) / TimeSpan.TicksPerMillisecond); break; }
            case Msg.Leave: AddSystem(p.player.name + " ha eixit de la sala.", true); RemovePeer(p, false); break;
            default: NetCoop.OnHost(p.player, type, r); break;
        }
    }

    System.Collections.IEnumerator CloseLater(INetLink link) { yield return new WaitForSecondsRealtime(0.3f); link.Close(); }

    string UniqueName(string name)
    {
        string candidate = name; int n = 2;
        bool Taken(string c) { foreach (var pl in Players.Values) if (string.Equals(pl.name, c, StringComparison.OrdinalIgnoreCase)) return true; return false; }
        while (Taken(candidate)) candidate = (name.Length > 13 ? name.Substring(0, 13) : name) + " " + n++;
        return candidate;
    }

    void HostChat(int from, string text)
    {
        text = (text ?? "").Trim();
        if (text.Length == 0) return;
        if (text.Length > 120) text = text.Substring(0, 120);
        string name = Players.TryGetValue(from, out var pl) ? pl.name : "?";
        var line = new ChatLine { from = from, name = name, text = text, time = Time.realtimeSinceStartup };
        PushChat(line);
        SendAll(NetProtocol.Frame(Msg.ChatOut, w => { w.Write(from); w.Write(name); w.Write(text); }));
    }

    void RemovePeer(Peer p, bool announce)
    {
        peers.Remove(p);
        if (p.player != null)
        {
            Players.Remove(p.player.id);
            if (announce) AddSystem(p.player.name + " ha eixit de la sala.", true);
            roomDirty = true;
        }
        if (p.link is RelayPeerLink rp) relayPeers.Remove(rp.peerId);
        StartCoroutine(CloseLater(p.link));
    }

    // ---------- Enviaments per al cooperatiu ----------
    public void SendTo(int playerId, byte[] frame)
    {
        foreach (var p in peers) if (p.player != null && p.player.id == playerId) { p.link.Send(frame); return; }
    }
    public void SendAllExcept(int playerId, byte[] frame)
    {
        foreach (var p in peers) if (p.player != null && p.player.id != playerId) p.link.Send(frame);
    }
    public void SendToHost(byte[] frame) { if (role == Role.Client && welcomed) hostLink?.Send(frame); }
    // L'amfitrio ho envia a tots; un client ho envia a l'amfitrio (que ho reenvia)
    public void SendGame(byte[] frame) { if (role == Role.Host) SendAll(frame); else SendToHost(frame); }

    public void SendAll(byte[] frame)
    {
        foreach (var p in peers) if (p.player != null) p.link.Send(frame);
    }

    byte[] RoomFrame()
    {
        return NetProtocol.Frame(Msg.Room, w =>
        {
            Room.Write(w);
            w.Write(CountdownLeft);
            w.Write(NetworkLabel);
            w.Write((byte)Players.Count);
            foreach (var pl in Players.Values) pl.Write(w);
        });
    }

    void LaunchGame()
    {
        Seed = rng.Next(1, int.MaxValue);
        Room.phase = RoomPhase.InGame;
        var start = NetProtocol.Frame(Msg.Start, w =>
        {
            w.Write((byte)Room.mode); w.Write(Seed);
            if (Room.mode == RoomMode.CustomMap) w.Write(customMapCode);
        });
        SendAll(start);
        roomDirty = true;
        HostUpdateRoomNow();
        EnterGame();
    }

    void HostUpdateRoomNow() { roomDirty = false; SendAll(RoomFrame()); Changed?.Invoke(); }

    void EnterGame()
    {
        RunState.Reset();
        RunStats.DontSave = Room.mode == RoomMode.CustomMap;
        if (Room.mode == RoomMode.CustomMap) MapPlaytest.SetNetworkMap(customMapData);
        foreach (var pl in Players.Values) pl.scene = -1;
        SceneManager.LoadScene(RoomModes.Scene(Room.mode));
    }

    // Torna tota la sala al lobby (nomes l'amfitrio)
    public void BackToLobby()
    {
        if (role != Role.Host) return;
        RunStats.DontSave = false;
        MapPlaytest.ClearNetworkMap();
        Room.phase = RoomPhase.Lobby;
        foreach (var pl in Players.Values) if (!pl.isHost) pl.ready = false;
        SendAll(NetProtocol.Frame(Msg.Start, w => { w.Write((byte)255); w.Write(0); }));
        AddSystem("Tornem al lobby!", true);
        roomDirty = true;
        RunState.Reset();
        SceneManager.LoadScene("Lobby");
    }

    // ---------- Client ----------
    void ClientUpdate()
    {
        float now = Time.realtimeSinceStartup;
        if (hostLink == null) return;
        int guard = 0;
        while (guard++ < 400 && role == Role.Client && hostLink != null && hostLink.Incoming.TryDequeue(out var frame))
        {
            lastFromHost = now;
            try { HandleFromHost(frame); }
            catch (Exception e) { Debug.LogWarning("Missatge invalid de l'amfitrio: " + e.Message); }
        }
        if (role != Role.Client || hostLink == null) return;
        if (!hostLink.Connected) { Disconnected(welcomed ? "S'ha perdut la connexió amb l'amfitrió." : "L'amfitrió ha tancat la connexió."); return; }
        if (now - lastFromHost > 12) { Disconnected("L'amfitrió no respon. S'ha tancat la connexió."); return; }

        if (welcomed && now >= nextPos)
        {
            nextPos = now + 0.05f;
            var me = Local;
            if (SampleLocal(LocalId) && me != null)
                hostLink.Send(NetProtocol.Frame(Msg.Pos, me.WritePosition));
        }
    }

    void HandleFromHost(byte[] frame)
    {
        var r = NetProtocol.Reader(frame, out var type);
        switch (type)
        {
            case Msg.Welcome:
                LocalId = r.ReadInt32();
                welcomed = true;
                FinishJoin(new JoinResult { ok = true, code = Room.code });
                SceneManager.LoadScene("Lobby");
                break;
            case Msg.Reject:
            {
                string reason = r.ReadString(); int kind = r.ReadByte(); string code = r.ReadString();
                var tried = lastTarget ?? new RoomInfo();
                tried.code = code;
                var res = new JoinResult { error = reason, needPassword = kind == 1, code = code, target = tried };
                hostLink.Close();
                ResetState();
                FinishJoin(res);
                break;
            }
            case Msg.Room:
            {
                var info = RoomInfo.Read(r);
                float countdown = r.ReadSingle();
                NetworkLabel = r.ReadString();
                RoomPhase before = Room.phase;
                Room = info;
                if (info.phase == RoomPhase.Countdown) countdownEnd = Time.realtimeSinceStartup + countdown;
                int n = r.ReadByte();
                var seen = new HashSet<int>();
                for (int i = 0; i < n; i++)
                {
                    var pl = NetPlayer.Read(r); seen.Add(pl.id);
                    if (Players.TryGetValue(pl.id, out var old)) { pl.scene = old.scene; pl.x = old.x; pl.y = old.y; pl.aim = old.aim; pl.weapon = old.weapon; pl.shieldRaised = old.shieldRaised; pl.down = old.down; pl.health = old.health; pl.maxHealth = old.maxHealth; pl.lastPos = old.lastPos; }
                    Players[pl.id] = pl;
                }
                var gone = new List<int>(); foreach (var id in Players.Keys) if (!seen.Contains(id)) gone.Add(id);
                foreach (var id in gone) Players.Remove(id);
                Changed?.Invoke();
                break;
            }
            case Msg.ChatOut:
            {
                int from = r.ReadInt32(); string name = r.ReadString(); string text = r.ReadString();
                PushChat(new ChatLine { from = from, name = name, text = text, system = from == 0, time = Time.realtimeSinceStartup });
                break;
            }
            case Msg.Snap:
            {
                int n = r.ReadByte();
                double t = Time.realtimeSinceStartupAsDouble;
                NetPlayer skipped = null;
                for (int i = 0; i < n; i++)
                {
                    int id = r.ReadInt32();
                    if (id != LocalId && Players.TryGetValue(id, out var pl)) pl.ReadPosition(r, t);
                    else (skipped ??= new NetPlayer()).ReadPosition(r, t);
                }
                break;
            }
            case Msg.Ping:
            {
                long t = r.ReadInt64();
                hostLink.Send(NetProtocol.Frame(Msg.Pong, w => w.Write(t)));
                break;
            }
            case Msg.Start:
            {
                int mode = r.ReadByte(); int seed = r.ReadInt32();
                if (mode == 255)
                {
                    RunStats.DontSave = false; MapPlaytest.ClearNetworkMap();
                    customMapName = customMapCode = null; customMapData = null;
                    RunState.Reset(); SceneManager.LoadScene("Lobby"); break;
                }
                Room.mode = (RoomMode)mode; Seed = seed; Room.phase = RoomPhase.InGame;
                customMapData = null; customMapCode = null;
                if (Room.mode == RoomMode.CustomMap)
                {
                    string mapCode = r.ReadString();
                    if (!MapShare160.Decode(mapCode, out customMapData, out var mapError))
                    { Disconnected("No s'ha pogut rebre el mapa de l'amfitrió: " + mapError); break; }
                    customMapCode = mapCode;
                }
                var me = Local; if (me != null) me.ready = false;
                EnterGame();
                break;
            }
            case Msg.Kicked: Disconnected(r.ReadString()); break;
            case Msg.Closed: Disconnected(r.ReadString()); break;
            default: NetCoop.OnClient(type, r); break;
        }
    }

    // ---------- Posicio local ----------
    PlayerMovement cachedPlayer;
    public static int SceneToken()
    {
        var s = SceneManager.GetActiveScene();
        return s.buildIndex * 1000 + (s.name == "Dungeon" ? unchecked(RunState.FloorVisit * 100000 + RunState.Floor) : 0);
    }

    bool SampleLocal(int id)
    {
        if (!Players.TryGetValue(id, out var me)) return false;
        if (cachedPlayer == null) cachedPlayer = FindFirstObjectByType<PlayerMovement>();
        if (cachedPlayer == null || !cachedPlayer.gameObject.activeInHierarchy) { me.scene = -1; return false; } // caigut: no compta
        var pos = cachedPlayer.transform.position;
        me.scene = SceneToken(); me.x = pos.x; me.y = pos.y; me.lastPos = Time.realtimeSinceStartupAsDouble;
        NetCoop.SampleLocal(cachedPlayer, me);
        return true;
    }

    // Tots apareixen al mateix punt: separem cada jugador un poc (sense ficar-lo dins d'una paret)
    System.Collections.IEnumerator SpreadSpawn()
    {
        yield return null;
        int index = 0;
        foreach (var id in Players.Keys) { if (id == LocalId) break; index++; }
        if (index == 0) yield break;
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm == null) yield break;
        var rb = pm.GetComponent<Rigidbody2D>();
        Vector2 origin = pm.transform.position;
        float angle = index * 2.39996f; // angle d'or: punts ben repartits
        foreach (float radius in new[] { 1.4f, 0.9f, 0.5f })
        {
            var p = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            bool blocked = false;
            foreach (var hit in Physics2D.OverlapCircleAll(p, 0.35f))
                if (!hit.isTrigger && hit.attachedRigidbody != rb) { blocked = true; break; }
            if (blocked) continue;
            pm.transform.position = new Vector3(p.x, p.y, pm.transform.position.z);
            if (rb != null) rb.position = p;
            yield break;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cachedPlayer = null;
        if (scene.name == "MainMenu")
        {
            if (Active) Leave(null);
            if (!string.IsNullOrEmpty(PendingMenuMessage)) { NetToast.Show(PendingMenuMessage); PendingMenuMessage = null; }
            return;
        }
        if (!Active) return;
        StartCoroutine(SpreadSpawn());
        if (scene.name == "Lobby") LobbyRoomUI.Create();
        else InGameNetUI.Create();
        if (role == Role.Host && scene.name == "Lobby" && Room.phase != RoomPhase.Lobby) { Room.phase = RoomPhase.Lobby; roomDirty = true; }
    }
}
