using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

// Servidor de sales d'Internet. És el mateix joc arrancat sense finestra:
//   CanicaAlphaReview.exe -batchmode -nographics -canicaServer [port]
//   ./CanicaAlphaReview.x86_64 -batchmode -nographics -canicaServer 7780   (Linux, p. ex. AWS)
// Fa dues coses: guarda la llista de sales publiques i reenvia els missatges entre
// l'amfitrio i els jugadors (relay), aixi ningu ha d'obrir ports al router.
public class RelayServer
{
    class Room { public RoomInfo info; public TcpLink host; public readonly Dictionary<int, TcpLink> peers = new Dictionary<int, TcpLink>(); public double created; }
    class Pending { public TcpLink link; public double since; public bool listing; }

    readonly int port;
    TcpListener listener;
    Thread acceptThread, loopThread;
    volatile bool running;
    readonly object newLock = new object();
    readonly List<TcpLink> fresh = new List<TcpLink>();
    readonly List<Pending> pending = new List<Pending>();
    readonly Dictionary<string, Room> rooms = new Dictionary<string, Room>();
    readonly Dictionary<TcpLink, Room> hostOf = new Dictionary<TcpLink, Room>();
    readonly Dictionary<TcpLink, (Room room, int id)> peerOf = new Dictionary<TcpLink, (Room, int)>();
    readonly System.Random rng = new System.Random();
    readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    int nextPeer = 1;
    const int MaxConnections = 512, MaxRooms = 128;
    public int RoomCount { get { lock (newLock) return rooms.Count; } }

    public RelayServer(int port) { this.port = port; }

    public void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        running = true;
        acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "Relay accept" };
        loopThread = new Thread(Loop) { IsBackground = true, Name = "Relay loop" };
        acceptThread.Start(); loopThread.Start();
        Log("Servidor de sales escoltant al port TCP " + port);
    }

    public void Stop()
    {
        running = false;
        try { listener.Stop(); } catch { }
    }

    static void Log(string s) => Debug.Log("[SERVIDOR SALES] " + s);

    void AcceptLoop()
    {
        while (running)
        {
            try
            {
                var c = listener.AcceptTcpClient();
                lock (newLock)
                {
                    if (pending.Count + hostOf.Count + peerOf.Count >= MaxConnections) { c.Close(); continue; }
                    fresh.Add(new TcpLink(c));
                }
            }
            catch (Exception e) { if (running) Log("accept: " + e.Message); }
        }
    }

    double Now => clock.Elapsed.TotalSeconds;

    void Loop()
    {
        while (running)
        {
            try { Tick(); } catch (Exception e) { Log("error: " + e); }
            Thread.Sleep(2);
        }
    }

    void Tick()
    {
        lock (newLock)
        {
            foreach (var l in fresh) pending.Add(new Pending { link = l, since = Now });
            fresh.Clear();

            // Connexions que encara no han dit qui son
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var p = pending[i];
                if (!p.link.Connected || Now - p.since > (p.listing ? 10 : 15)) { p.link.Close(); pending.RemoveAt(i); continue; }
                if (p.listing || !p.link.Incoming.TryDequeue(out var first)) continue;
                if (!HandleFirst(p, first)) { pending.RemoveAt(i); }
            }

            // Amfitrions
            var deadHosts = new List<TcpLink>();
            foreach (var kv in hostOf)
            {
                var host = kv.Key; var room = kv.Value;
                while (host.Incoming.TryDequeue(out var frame)) HandleHost(room, frame);
                if (!host.Connected) deadHosts.Add(host);
            }
            foreach (var h in deadHosts) CloseRoom(hostOf[h]);

            // Jugadors
            var deadPeers = new List<TcpLink>();
            foreach (var kv in peerOf)
            {
                var link = kv.Key; var (room, id) = kv.Value;
                while (link.Incoming.TryDequeue(out var frame))
                    room.host.Send(NetProtocol.Frame(Msg.RelayPeerData, w => { w.Write(id); w.Write(frame.Length); w.Write(frame); }));
                if (!link.Connected) deadPeers.Add(link);
            }
            foreach (var l in deadPeers) DropPeer(l, true);
        }
    }

    // Retorna true si la connexio es queda a "pending" (llistat esperant que el client tanque)
    bool HandleFirst(Pending p, byte[] first)
    {
        var link = p.link;
        var r = NetProtocol.Reader(first, out var type);
        switch (type)
        {
            case Msg.RelayList:
            {
                var list = new List<RoomInfo>();
                foreach (var room in rooms.Values) if (room.info.isPublic) list.Add(room.info);
                link.Send(NetProtocol.Frame(Msg.RelayRooms, w => { w.Write(list.Count); foreach (var info in list) info.Write(w); }));
                p.listing = true; // el client tanca quan ho ha llegit
                return true;
            }
            case Msg.RelayHost:
            {
                if (rooms.Count >= MaxRooms) { link.Send(NetProtocol.Frame(Msg.RelayFail, w => w.Write("El servidor esta ple de sales. Prova-ho d'aci a un moment."))); p.listing = true; return true; }
                var info = RoomInfo.Read(r);
                string code; do code = NetProtocol.NewCode(rng); while (rooms.ContainsKey(code));
                info.code = code;
                var room = new Room { info = info, host = link, created = Now };
                rooms[code] = room; hostOf[link] = room;
                link.Send(NetProtocol.Frame(Msg.RelayHostOk, w => w.Write(code)));
                Log("Sala nova " + code + " \"" + info.name + "\" (" + rooms.Count + " sales)");
                return false;
            }
            case Msg.RelayJoin:
            {
                string code = NetProtocol.NormalizeCode(r.ReadString());
                if (!rooms.TryGetValue(code, out var room))
                {
                    link.Send(NetProtocol.Frame(Msg.RelayFail, w => w.Write("No hi ha cap sala amb el codi " + code + ".")));
                    p.listing = true;
                    return true;
                }
                int id = nextPeer++;
                room.peers[id] = link; peerOf[link] = (room, id);
                link.Send(NetProtocol.Frame(Msg.RelayJoinOk, w => w.Write(room.info.code)));
                room.host.Send(NetProtocol.Frame(Msg.RelayPeerJoin, w => w.Write(id)));
                return false;
            }
            default:
                link.Close();
                return false;
        }
    }

    void HandleHost(Room room, byte[] frame)
    {
        var r = NetProtocol.Reader(frame, out var type);
        switch (type)
        {
            case Msg.RelayToPeer:
            {
                int id = r.ReadInt32(); int len = r.ReadInt32(); var data = r.ReadBytes(len);
                if (room.peers.TryGetValue(id, out var peer)) peer.Send(data);
                break;
            }
            case Msg.RelayKick:
            {
                int id = r.ReadInt32();
                if (room.peers.TryGetValue(id, out var peer)) DropPeer(peer, false);
                break;
            }
            case Msg.RelayUpdate:
            {
                var info = RoomInfo.Read(r); info.code = room.info.code; room.info = info;
                break;
            }
        }
    }

    void DropPeer(TcpLink link, bool tellHost)
    {
        if (!peerOf.TryGetValue(link, out var entry)) return;
        peerOf.Remove(link); entry.room.peers.Remove(entry.id);
        if (tellHost) entry.room.host.Send(NetProtocol.Frame(Msg.RelayPeerLeave, w => w.Write(entry.id)));
        link.Close();
    }

    void CloseRoom(Room room)
    {
        foreach (var peer in new List<TcpLink>(room.peers.Values))
        {
            peer.Send(NetProtocol.Frame(Msg.Closed, w => w.Write("L'amfitrio ha tancat la sala.")));
            peerOf.Remove(peer); peer.Close();
        }
        room.peers.Clear();
        hostOf.Remove(room.host); rooms.Remove(room.info.code); room.host.Close();
        Log("Sala tancada " + room.info.code + " (" + rooms.Count + " sales)");
    }

    // ---------- Arrancada sense finestra ----------
    static RelayServer headless;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void FromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, "-canicaServer");
        if (i < 0) return;
        int p = NetProtocol.RelayPort;
        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int custom)) p = custom;
        Application.targetFrameRate = 10;
        AudioListener.volume = 0;
        headless = new RelayServer(p);
        try { headless.Start(); }
        catch (Exception e) { Debug.LogError("[SERVIDOR SALES] No s'ha pogut obrir el port " + p + ": " + e.Message); Application.Quit(2); }
        Application.quitting += () => headless?.Stop();
    }
    public static bool HeadlessRunning => headless != null;
}
