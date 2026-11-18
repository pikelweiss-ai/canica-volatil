using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

// Un canal de missatges cap a un altre jugador.
// TcpLink: connexio TCP directa (LAN, IP o servidor de sales).
// RelayPeerLink: un jugador que arriba a l'amfitrio a traves del servidor de sales.
public interface INetLink
{
    void Send(byte[] payload);
    bool Connected { get; }
    ConcurrentQueue<byte[]> Incoming { get; }
    void Close();
    string Describe { get; }
}

public class TcpLink : INetLink
{
    readonly TcpClient client;
    readonly NetworkStream stream;
    readonly object sendLock = new object();
    readonly Thread reader;
    volatile bool open = true;
    public ConcurrentQueue<byte[]> Incoming { get; } = new ConcurrentQueue<byte[]>();
    public bool Connected => open;
    public string Describe { get; }
    public string CloseReason;

    public TcpLink(TcpClient client)
    {
        this.client = client;
        client.NoDelay = true;
        stream = client.GetStream();
        Describe = client.Client.RemoteEndPoint != null ? client.Client.RemoteEndPoint.ToString() : "?";
        reader = new Thread(ReadLoop) { IsBackground = true, Name = "CanicaNet " + Describe };
        reader.Start();
    }

    // Connecta amb un temps maxim; llança excepcio si no pot.
    public static TcpLink Connect(string host, int port, int timeoutMs = 4000)
    {
        var c = new TcpClient();
        var ar = c.BeginConnect(host, port, null, null);
        if (!ar.AsyncWaitHandle.WaitOne(timeoutMs))
        {
            try { c.Close(); } catch { }
            throw new TimeoutException("No respon " + host + ":" + port);
        }
        c.EndConnect(ar);
        return new TcpLink(c);
    }

    void ReadLoop()
    {
        try
        {
            while (open)
            {
                var frame = NetProtocol.ReadFrame(stream);
                if (frame == null) break;
                Incoming.Enqueue(frame);
            }
        }
        catch (Exception e) { CloseReason = e.Message; }
        open = false;
    }

    public void Send(byte[] payload)
    {
        if (!open) return;
        try { lock (sendLock) NetProtocol.WriteFrame(stream, payload); }
        catch (Exception e) { CloseReason = e.Message; Close(); }
    }

    public void Close()
    {
        open = false;
        try { stream.Close(); } catch { }
        try { client.Close(); } catch { }
    }
}

// Jugador remot que passa pel servidor de sales: els seus missatges arriben embolicats.
public class RelayPeerLink : INetLink
{
    readonly TcpLink relay;
    public readonly int peerId;
    public bool open = true;
    public ConcurrentQueue<byte[]> Incoming { get; } = new ConcurrentQueue<byte[]>();
    public bool Connected => open && relay.Connected;
    public string Describe => "relay#" + peerId;

    public RelayPeerLink(TcpLink relay, int peerId) { this.relay = relay; this.peerId = peerId; }

    public void Send(byte[] payload)
    {
        if (!Connected) return;
        relay.Send(NetProtocol.Frame(Msg.RelayToPeer, w => { w.Write(peerId); w.Write(payload.Length); w.Write(payload); }));
    }

    public void Close()
    {
        if (!open) return;
        open = false;
        relay.Send(NetProtocol.Frame(Msg.RelayKick, w => w.Write(peerId)));
    }
}
