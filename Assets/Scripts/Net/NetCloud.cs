using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

// =====================================================================
//  INTERNET SENSE IP (Alpha v1.1.1)
// =====================================================================
// Perquè qualsevol puga crear una sala pública i que els amics l'escullen de la llista
// (o hi entren amb el codi de 6 lletres) sense obrir ports, sense VPN i sense escriure cap IP.
//
// Dues peces gratuïtes i sense compte:
//  1. TÚNEL (bore.pub): l'amfitrió obri UNA connexió d'eixida cap a bore.pub i este li dona un port
//     públic (p. ex. bore.pub:24901). Qui es connecta a eixe port arriba a l'amfitrió pel túnel.
//     Com que tot són connexions d'EIXIDA, el router i el tallafocs no han de fer res,
//     i la IP de casa de l'amfitrió no la veu ningú.
//  2. DIRECTORI (ntfy.sh): l'amfitrió anuncia la sala cada pocs segons en un "tema" públic
//     (nom, mode, jugadors, codi i el port del túnel). El navegador llig els anuncis recents.
//     Si una sala deixa d'anunciar-se (o avisa que tanca), desapareix de la llista.
//
// Després tot va pel protocol de sempre (8): el túnel només porta els bytes.
// Si algú configura un servidor de sales propi (camp avançat), es fa servir eixe com abans.
public static class NetCloud
{
    static string[] configuredTunnelHosts;
    static string Arg(string key, string def)
    {
        var a = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(a, key);
        return i >= 0 && i + 1 < a.Length ? a[i + 1] : def;
    }

    // Candidate hosts may be overridden for testing or advanced troubleshooting.
    // Keep bore.pub as the only built-in public instance: the official project documents no others.
    static string[] LoadTunnelHosts()
    {
        string configured = Arg("-canicaTunnel", PlayerPrefs.GetString("Net_Tunnels", "bore.pub"));
        var hosts = new List<string>();
        foreach (var raw in (configured ?? "").Split(','))
        {
            string host = raw.Trim();
            if (host.Length > 0 && !hosts.Contains(host)) hosts.Add(host);
        }
        if (hosts.Count == 0) hosts.Add("bore.pub");
        return hosts.ToArray();
    }
    public static string[] TunnelHosts => configuredTunnelHosts ?? (configuredTunnelHosts = LoadTunnelHosts());
    public static string TunnelHost => TunnelHosts[0];
    public const int TunnelControlPort = 7835;
    public static readonly string Directory = Arg("-canicaDirectory", "https://ntfy.sh").TrimEnd('/');
    // Les proves automàtiques usen un tema propi perquè les sales dels bots no surten a ningú
    static readonly string Tag = Arg("-canicaCloudTag", "");
    // Sense LAN (proves): obliga a passar pel túnel encara que amfitrió i client estiguen al mateix PC
    public static readonly bool NoLan = Array.IndexOf(Environment.GetCommandLineArgs(), "-canicaNoLan") >= 0;
    public static readonly bool IsBot = Array.IndexOf(Environment.GetCommandLineArgs(), "-canicaBot") >= 0;

    // Es fa servir quan no hi ha cap servidor de sales propi configurat
    public static bool InUse => string.IsNullOrEmpty(NetSession.ServerAddress);

    static string Prefix => "canica-volatil-v" + NetProtocol.Version + (Tag.Length > 0 ? "-" + Tag : "");
    public static string PublicTopic => Prefix + "-sales";
    public static string CodeTopic(string code)
    {
        using (var sha = SHA256.Create())
        {
            var h = sha.ComputeHash(Encoding.UTF8.GetBytes("canica-codi:" + code));
            return Prefix + "-codi-" + BitConverter.ToString(h, 0, 10).Replace("-", "").ToLowerInvariant();
        }
    }

    // ---------- Anunci d'una sala ----------
    [Serializable]
    public class Announce
    {
        public int v;           // protocol
        public string ver;      // versió del joc
        public string code, name, host, ep, directEp;
        public int mode, phase, players, max, rtt;
        public bool pw, pub, closed, direct;
    }

    [Serializable] class NtfyLine { public string @event; public long time; public string message; }

    public static Announce FromRoom(RoomInfo r, int tunnelPort, int rtt, bool closed, string tunnelHost = null, string directEp = null) => new Announce
    {
        v = NetProtocol.Version, ver = GameVersion.Package, code = r.code, name = r.name, host = r.hostName,
        ep = (tunnelHost ?? TunnelHost) + ":" + tunnelPort, directEp = directEp, direct = !string.IsNullOrEmpty(directEp), mode = (int)r.mode, phase = (int)r.phase, players = r.players, max = r.maxPlayers,
        rtt = rtt, pw = r.hasPassword, pub = r.isPublic, closed = closed
    };

    // Publica (corrutina: UnityWebRequest ha d'anar pel fil principal)
    public static IEnumerator Publish(string topic, Announce a, Action<bool> done = null)
    {
        var body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(a));
        using (var req = new UnityWebRequest(Directory + "/" + topic, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 8;
            yield return req.SendWebRequest();
            bool ok = req.result == UnityWebRequest.Result.Success;
            if (!ok) Debug.LogWarning("[Online] No s'ha pogut anunciar la sala: " + req.error);
            done?.Invoke(ok);
        }
    }

    // Llig els anuncis dels últims segons d'un tema i torna l'últim estat de cada sala viva
    public static IEnumerator Fetch(string topic, Action<List<RoomInfo>, string> done)
    {
        using (var req = UnityWebRequest.Get(Directory + "/" + topic + "/json?poll=1&since=75s"))
        {
            req.timeout = 8;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { done(new List<RoomInfo>(), "No es pot llegir la llista de sales d'Internet (" + req.error + ")."); yield break; }
            done(Parse(req.downloadHandler.text), null);
        }
    }

    public static List<RoomInfo> Parse(string text)
    {
        var latest = new Dictionary<string, (long time, Announce a)>();
        long newest = 0;
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            NtfyLine n;
            try { n = JsonUtility.FromJson<NtfyLine>(line); } catch { continue; }
            if (n == null || n.@event != "message" || string.IsNullOrEmpty(n.message)) continue;
            Announce a;
            try { a = JsonUtility.FromJson<Announce>(n.message); } catch { continue; }
            if (a == null || string.IsNullOrEmpty(a.code)) continue;
            newest = Math.Max(newest, n.time);
            if (!latest.TryGetValue(a.code, out var prev) || n.time >= prev.time) latest[a.code] = (n.time, a);
        }
        var rooms = new List<RoomInfo>();
        foreach (var kv in latest.Values)
        {
            var a = kv.a;
            if (a.closed || newest - kv.time > 50) continue;            // tancada o sense anunciar-se
            var room = ToRoom(a);
            if (room != null) rooms.Add(room);
        }
        return rooms;
    }

    // Només s'accepten endpoints d'un candidat configurat o una IP directa vàlida.
    static RoomInfo ToRoom(Announce a)
    {
        if (string.IsNullOrEmpty(a.ep)) return null;
        string host = null; int port = 0;
        int colon = a.ep.LastIndexOf(':');
        if (colon > 0 && Array.IndexOf(TunnelHosts, a.ep.Substring(0, colon)) >= 0 && int.TryParse(a.ep.Substring(colon + 1), out port) && port >= 1024 && port <= 65535) host = a.ep.Substring(0, colon);
        string directHost = null; int directPort = 0;
        if (a.direct && NetProtocol.TryParseEndpoint(a.directEp, NetProtocol.GamePort, out directHost, out directPort) && directPort >= NetProtocol.GamePort && directPort <= NetProtocol.GamePort + 10 && PublicIPv4.IsPublic(directHost)) { }
        else { directHost = null; directPort = 0; }
        if (host == null && directHost == null) return null;
        string code = NetProtocol.NormalizeCode(a.code);
        if (code.Length != 6) return null;
        return new RoomInfo
        {
            code = code, name = NetSession.CleanName(a.name, 24), hostName = NetSession.CleanName(a.host),
            mode = a.mode == 1 ? RoomMode.FreeMap : RoomMode.Dungeon, phase = (RoomPhase)Mathf.Clamp(a.phase, 0, 2),
            players = Mathf.Clamp(a.players, 1, NetProtocol.MaxPlayers), maxPlayers = Mathf.Clamp(a.max, 2, NetProtocol.MaxPlayers),
            hasPassword = a.pw, isPublic = a.pub, version = a.v,
            address = host ?? directHost, port = host != null ? port : directPort, directAddress = directHost, directPort = directPort, viaCloud = true, pingMs = a.rtt
        };
    }

    // Temps d'anada i tornada fins al túnel (per a estimar el ping de les sales)
    public static int MeasureTunnelRtt()
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            foreach (var host in TunnelHosts)
            {
                using (var c = new TcpClient())
                {
                    var ar = c.BeginConnect(host, TunnelControlPort, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(4000)) continue;
                    c.EndConnect(ar);
                    return (int)sw.ElapsedMilliseconds;
                }
            }
            return -1;
        }
        catch { return -1; }
    }
}

// =====================================================================
//  Túnel bore (protocol obert de github.com/ekzhang/bore)
// =====================================================================
// Missatges JSON acabats en \0 pel port de control 7835:
//   nosaltres -> {"Hello":0}             servidor -> {"Hello":24901}  (port públic assignat)
//   servidor  -> "Heartbeat" (cada 0,5 s) o {"Connection":"uuid"} quan entra algú
//   per cada Connection obrim una connexió nova, enviem {"Accept":"uuid"} i, des d'eixe moment,
//   eixa connexió ÉS el jugador que acaba d'entrar (ja la tracta NetSession com qualsevol altra).
public class BoreTunnel
{
    public volatile int PublicPort;     // 0 mentre no està connectat
    public volatile int Rtt = -1;
    public volatile string Error;
    public string Host { get; private set; }
    public int Generation;              // puja cada vegada que canvia el port (cal tornar a anunciar)
    readonly Action<TcpClient> onClient;
    readonly Thread thread;
    volatile bool stopped;
    TcpClient control;

    public BoreTunnel(Action<TcpClient> onClient)
    {
        this.onClient = onClient;
        thread = new Thread(Loop) { IsBackground = true, Name = "Canica tunel" };
    }

    public void Start() => thread.Start();

    public void Stop()
    {
        stopped = true;
        try { control?.Close(); } catch { }
    }

    // Espera fins que el túnel tinga port (o falle). Per a HostRoom.
    public bool WaitReady(int ms)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < ms && !stopped)
        {
            if (PublicPort > 0) return true;
            Thread.Sleep(20);
        }
        return PublicPort > 0;
    }

    void Loop()
    {
        int failures = 0;
        while (!stopped)
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                Exception last = null;
                foreach (var candidate in NetCloud.TunnelHosts)
                {
                    try { control = Connect(candidate, NetCloud.TunnelControlPort, 4000); Host = candidate; break; }
                    catch (Exception e) { last = e; }
                }
                if (control == null) throw last ?? new IOException("cap servei de túnel disponible");
                Rtt = (int)sw.ElapsedMilliseconds;
                control.ReceiveTimeout = 8000; // el servidor envia batecs cada 0,5 s
                var stream = control.GetStream();
                Send(stream, "{\"Hello\":0}");
                string hello = Read(stream);
                int port = ParseInt(hello, "Hello");
                if (port <= 0) throw new IOException(hello != null && hello.Contains("Error") ? hello : "resposta inesperada del túnel");
                PublicPort = port; Error = null; failures = 0;
                Interlocked.Increment(ref Generation);
                while (!stopped)
                {
                    string msg = Read(stream);
                    if (msg == null) break;
                    if (msg.Contains("\"Connection\""))
                    {
                        string id = ParseString(msg, "Connection");
                        if (id != null) new Thread(() => AcceptOne(id)) { IsBackground = true, Name = "Canica tunel accept" }.Start();
                    }
                    else if (msg.Contains("\"Error\"")) Error = ParseString(msg, "Error");
                }
            }
            catch (Exception e) { if (!stopped) Error = e.Message; }
            try { control?.Close(); } catch { }
            PublicPort = 0;
            if (stopped) break;
            failures++;
            Thread.Sleep(Math.Min(10000, 1000 * failures)); // torna a provar (amb un port nou)
        }
    }

    void AcceptOne(string id)
    {
        try
        {
            var c = Connect(Host ?? NetCloud.TunnelHost, NetCloud.TunnelControlPort, 4000);
            Send(c.GetStream(), "{\"Accept\":\"" + id + "\"}");
            if (stopped) { c.Close(); return; }
            onClient(c);
        }
        catch (Exception e) { Debug.LogWarning("[Online] No s'ha pogut acceptar un jugador pel túnel: " + e.Message); }
    }

    static TcpClient Connect(string host, int port, int timeoutMs)
    {
        var c = new TcpClient();
        var ar = c.BeginConnect(host, port, null, null);
        if (!ar.AsyncWaitHandle.WaitOne(timeoutMs)) { try { c.Close(); } catch { } throw new TimeoutException("no respon " + host); }
        c.EndConnect(ar);
        c.NoDelay = true;
        return c;
    }

    static void Send(NetworkStream s, string json)
    {
        var b = Encoding.UTF8.GetBytes(json + "\0");
        s.Write(b, 0, b.Length);
        s.Flush();
    }

    static string Read(NetworkStream s)
    {
        var buf = new List<byte>(64);
        while (true)
        {
            int b = s.ReadByte();
            if (b < 0) return null;
            if (b == 0) return Encoding.UTF8.GetString(buf.ToArray());
            buf.Add((byte)b);
            if (buf.Count > 1024) throw new IOException("missatge del túnel massa llarg");
        }
    }

    static int ParseInt(string json, string key)
    {
        if (json == null) return 0;
        int i = json.IndexOf("\"" + key + "\":", StringComparison.Ordinal);
        if (i < 0) return 0;
        i += key.Length + 3;
        int j = i; while (j < json.Length && char.IsDigit(json[j])) j++;
        return int.TryParse(json.Substring(i, j - i), out int v) ? v : 0;
    }

    static string ParseString(string json, string key)
    {
        int i = json.IndexOf("\"" + key + "\":\"", StringComparison.Ordinal);
        if (i < 0) return null;
        i += key.Length + 4;
        int j = json.IndexOf('"', i);
        return j > i ? json.Substring(i, j - i) : null;
    }
}
