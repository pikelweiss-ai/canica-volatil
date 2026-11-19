using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

// Protocol de sales de Canica Volatil.
// Cada missatge viaja en un "frame": [int32 llargaria][byte tipus][dades].
// El mateix format serveix per a parlar amb l'amfitrio (LAN/IP) i amb el servidor de sales (Internet).
public static class NetProtocol
{
    public const int Version = 7; // 7: noves sales de progressió a partir del pis 3
    public const int GamePort = 7777;      // TCP: l'amfitrio escolta ací (prova 7777..7787)
    public const int DiscoveryPort = 7778; // UDP: descobriment de sales en LAN
    public const int RelayPort = 7780;     // TCP: servidor de sales d'Internet
    public const int MaxFrame = 64 * 1024;
    public const int MaxPlayers = 8;
    const string CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"; // sense O/0, I/1/L

    public static string NewCode(Random rng)
    {
        var sb = new StringBuilder(6);
        for (int i = 0; i < 6; i++) sb.Append(CodeAlphabet[rng.Next(CodeAlphabet.Length)]);
        return sb.ToString();
    }

    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return "";
        var sb = new StringBuilder();
        foreach (char c in code.Trim().ToUpperInvariant())
            if (CodeAlphabet.IndexOf(c) >= 0) sb.Append(c); // espais, guions i simbols fora
        return sb.ToString();
    }

    public static bool LooksLikeAddress(string text) => !string.IsNullOrEmpty(text) && (text.Contains(":") || text.Contains("."));

    // La contrasenya no viatja mai en clar: s'envia el hash amb el codi de la sala com a sal.
    public static string HashPassword(string code, string password)
    {
        if (string.IsNullOrEmpty(password)) return "";
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("canica:" + code + ":" + password));
            return Convert.ToBase64String(bytes);
        }
    }

    // ---------- Frames ----------
    public static byte[] Frame(Msg type, Action<BinaryWriter> body = null)
    {
        using (var ms = new MemoryStream())
        using (var w = new BinaryWriter(ms, Encoding.UTF8))
        {
            w.Write((byte)type);
            body?.Invoke(w);
            w.Flush();
            return ms.ToArray();
        }
    }

    public static void WriteFrame(Stream s, byte[] payload)
    {
        var len = BitConverter.GetBytes(payload.Length);
        var all = new byte[4 + payload.Length];
        Buffer.BlockCopy(len, 0, all, 0, 4);
        Buffer.BlockCopy(payload, 0, all, 4, payload.Length);
        s.Write(all, 0, all.Length);
    }

    // Llig un frame sencer; retorna null si la connexio s'ha tancat.
    public static byte[] ReadFrame(Stream s)
    {
        var head = ReadExact(s, 4);
        if (head == null) return null;
        int len = BitConverter.ToInt32(head, 0);
        if (len <= 0 || len > MaxFrame) throw new IOException("Frame invalid: " + len);
        return ReadExact(s, len);
    }

    static byte[] ReadExact(Stream s, int count)
    {
        var buf = new byte[count];
        int got = 0;
        while (got < count)
        {
            int n = s.Read(buf, got, count - got);
            if (n <= 0) return null;
            got += n;
        }
        return buf;
    }

    public static BinaryReader Reader(byte[] payload, out Msg type)
    {
        var r = new BinaryReader(new MemoryStream(payload), Encoding.UTF8);
        type = (Msg)r.ReadByte();
        return r;
    }

    public static List<IPAddress> BroadcastTargets()
    {
        var list = new List<IPAddress> { IPAddress.Broadcast, IPAddress.Loopback };
        try
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork || ua.IPv4Mask == null) continue;
                    var ip = ua.Address.GetAddressBytes(); var mask = ua.IPv4Mask.GetAddressBytes();
                    var b = new byte[4];
                    for (int i = 0; i < 4; i++) b[i] = (byte)(ip[i] | ~mask[i]);
                    var addr = new IPAddress(b);
                    if (!list.Contains(addr)) list.Add(addr);
                }
            }
        }
        catch { }
        return list;
    }

    // IP de la xarxa real (wifi/cable amb porta d'enllaç), no la d'adaptadors virtuals (VirtualBox, Hyper-V...)
    public static string LocalIPv4()
    {
        string best = "127.0.0.1"; int bestScore = int.MinValue;
        try
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;
                var props = ni.GetIPProperties();
                int score = 0;
                foreach (var g in props.GatewayAddresses) if (g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any)) { score += 100; break; }
                string d = (ni.Description + " " + ni.Name).ToLowerInvariant();
                if (d.Contains("virtual") || d.Contains("vmware") || d.Contains("hyper-v") || d.Contains("vethernet") || d.Contains("vbox") || d.Contains("wsl")) score -= 200;
                if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211) score += 10;
                foreach (var ua in props.UnicastAddresses)
                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork && score > bestScore) { bestScore = score; best = ua.Address.ToString(); }
            }
        }
        catch { }
        return best;
    }

    public static bool TryParseEndpoint(string text, int defaultPort, out string host, out int port)
    {
        host = null; port = defaultPort;
        if (string.IsNullOrWhiteSpace(text)) return false;
        text = text.Trim();
        int colon = text.LastIndexOf(':');
        if (colon > 0)
        {
            if (!int.TryParse(text.Substring(colon + 1), out port) || port <= 0 || port > 65535) return false;
            host = text.Substring(0, colon);
        }
        else host = text;
        return host.Length > 0;
    }
}

public enum Msg : byte
{
    // Client -> amfitrio
    Hello = 1, Ready = 2, Chat = 3, Pos = 4, Pong = 5, Leave = 6,
    // Amfitrio -> client
    Welcome = 20, Reject = 21, Room = 22, ChatOut = 23, Snap = 24, Ping = 25, Start = 26, Kicked = 27, Closed = 28,
    // Servidor de sales (Internet)
    RelayList = 40, RelayRooms = 41, RelayHost = 42, RelayHostOk = 43, RelayJoin = 44, RelayJoinOk = 45, RelayFail = 46,
    RelayPeerJoin = 47, RelayPeerData = 48, RelayPeerLeave = 49, RelayToPeer = 50, RelayKick = 51, RelayUpdate = 52,
    // Cooperatiu: client -> amfitrio
    EnemyHitReq = 60, ChestReq = 61, ShotFx = 62, SwingFx = 63, PlayerDown = 64, PortalReq = 65, PunchFx = 66,
    // Cooperatiu: amfitrio -> clients
    EnemySpawn = 70, EnemySnap = 71, EnemyDie = 72, EnemyMark = 73, RoomState = 74, ChestOpened = 75, ChestSpawn = 76, PortalSpawn = 77,
    Hurt = 78, NextFloorAll = 79, GameOverAll = 80, BulletFx = 81, ExplodeFx = 82, ReviveAll = 83, ChestGrant = 84, Telegraph = 85,
    // Objectes compartits (qualsevol -> amfitrio -> tots)
    PickupSpawn = 86, PickupClaim = 87, PickupGrant = 88, PickupTaken = 89, BreakableBroken = 90,
    MapCoinReq = 91, MapCoinGrant = 92, MapCoinTaken = 93,
    BreakableHit = 94,
}

public enum RoomMode : byte { Dungeon = 0, FreeMap = 1 }
public enum RoomPhase : byte { Lobby = 0, Countdown = 1, InGame = 2 }

public static class RoomModes
{
    public static string Scene(RoomMode m) => m == RoomMode.Dungeon ? "Dungeon" : "GameScene";
    public static string Label(RoomMode m) => m == RoomMode.Dungeon ? "MASMORRA" : "MAPA LLIURE";
    public static string Short(RoomMode m) => m == RoomMode.Dungeon ? "Masmorra" : "Mapa lliure";
}

// El que es veu d'una sala al navegador (LAN o Internet).
[Serializable]
public class RoomInfo
{
    public string code = "";
    public string name = "Sala";
    public string hostName = "";
    public RoomMode mode;
    public RoomPhase phase;
    public int players = 1;
    public int maxPlayers = 4;
    public bool hasPassword;
    public bool isPublic = true;
    public int version = NetProtocol.Version;
    // Nomes al client: com connectar-hi
    [NonSerialized] public string address;
    [NonSerialized] public int port;
    [NonSerialized] public bool viaRelay;
    [NonSerialized] public int pingMs = -1;

    public void Write(BinaryWriter w)
    {
        w.Write(code); w.Write(name); w.Write(hostName); w.Write((byte)mode); w.Write((byte)phase);
        w.Write((byte)players); w.Write((byte)maxPlayers); w.Write(hasPassword); w.Write(isPublic); w.Write(version);
    }

    public static RoomInfo Read(BinaryReader r)
    {
        return new RoomInfo
        {
            code = r.ReadString(), name = r.ReadString(), hostName = r.ReadString(), mode = (RoomMode)r.ReadByte(), phase = (RoomPhase)r.ReadByte(),
            players = r.ReadByte(), maxPlayers = r.ReadByte(), hasPassword = r.ReadBoolean(), isPublic = r.ReadBoolean(), version = r.ReadInt32()
        };
    }

    public bool Full => players >= maxPlayers;
}

// Un jugador dins d'una sala.
public class NetPlayer
{
    public int id;
    public string name = "Jugador";
    public int skin, hat, trail;
    public bool ready, isHost;
    public int ping;
    // Posicio (es rep pels snapshots)
    public int scene = -1;
    public float x, y;
    public short aim;       // graus d'apuntat
    public string weapon;   // nom de l'arma a la ma ("" = puny)
    public bool down;       // caigut a la masmorra
    public int health, maxHealth;
    public double lastPos;

    public void WritePosition(BinaryWriter w)
    {
        w.Write(scene); w.Write(x); w.Write(y); w.Write(aim); w.Write(weapon ?? "");
        w.Write(down); w.Write(health); w.Write(maxHealth);
    }

    public void ReadPosition(BinaryReader r, double receivedAt)
    {
        scene = r.ReadInt32(); x = r.ReadSingle(); y = r.ReadSingle();
        aim = r.ReadInt16(); weapon = r.ReadString(); down = r.ReadBoolean();
        health = r.ReadInt32(); maxHealth = r.ReadInt32();
        lastPos = receivedAt;
    }

    public void Write(BinaryWriter w)
    {
        w.Write(id); w.Write(name); w.Write((short)skin); w.Write((short)hat); w.Write((short)trail);
        w.Write(ready); w.Write(isHost); w.Write((short)Math.Min(ping, 9999));
    }

    public static NetPlayer Read(BinaryReader r)
    {
        return new NetPlayer { id = r.ReadInt32(), name = r.ReadString(), skin = r.ReadInt16(), hat = r.ReadInt16(), trail = r.ReadInt16(), ready = r.ReadBoolean(), isHost = r.ReadBoolean(), ping = r.ReadInt16() };
    }
}
