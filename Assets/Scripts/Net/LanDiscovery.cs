using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// Descobriment de sales en la xarxa local per UDP (com els jocs de LAN de tota la vida).
// El client crida "hi ha alguna sala?" a tota la xarxa; cada amfitrio respon amb la seua info.
public static class LanDiscovery
{
    static readonly byte[] AskList = Encoding.ASCII.GetBytes("CANICA?LIST");
    const string AskCodePrefix = "CANICA?CODE:";
    static readonly byte[] Answer = Encoding.ASCII.GetBytes("CANICA!");

    // ---------- Amfitrio ----------
    public class Responder
    {
        UdpClient udp;
        Thread thread;
        volatile bool running;
        volatile RoomInfo snapshot;
        public volatile int gamePort;

        public void Update(RoomInfo info) { snapshot = info; }

        public bool Start()
        {
            try
            {
                udp = new UdpClient();
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.Client.Bind(new IPEndPoint(IPAddress.Any, NetProtocol.DiscoveryPort));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("Descobriment LAN no disponible: " + e.Message);
                return false;
            }
            running = true;
            thread = new Thread(Loop) { IsBackground = true, Name = "LAN responder" };
            thread.Start();
            return true;
        }

        void Loop()
        {
            while (running)
            {
                try
                {
                    var from = new IPEndPoint(IPAddress.Any, 0);
                    var data = udp.Receive(ref from);
                    var info = snapshot;
                    if (info == null) continue;
                    string text = data.Length < 64 ? Encoding.ASCII.GetString(data) : "";
                    bool reply = false;
                    if (text == "CANICA?LIST") reply = info.isPublic;
                    else if (text.StartsWith(AskCodePrefix)) reply = NetProtocol.NormalizeCode(text.Substring(AskCodePrefix.Length)) == info.code;
                    if (!reply) continue;
                    byte[] body;
                    using (var ms = new MemoryStream())
                    using (var w = new BinaryWriter(ms, Encoding.UTF8))
                    {
                        w.Write(Answer); info.Write(w); w.Write(gamePort); w.Flush(); body = ms.ToArray();
                    }
                    udp.Send(body, body.Length, from);
                }
                catch (Exception) { if (!running) break; }
            }
        }

        public void Stop()
        {
            running = false;
            try { udp?.Close(); } catch { }
        }
    }

    // ---------- Client ----------
    // Bloqueja durant "waitMs": cal cridar-ho des d'un fil, no des del fil principal.
    public static List<RoomInfo> Query(string code, int waitMs)
    {
        var found = new Dictionary<string, RoomInfo>();
        var udp = new UdpClient(0) { EnableBroadcast = true };
        try
        {
            byte[] ask = code == null ? AskList : Encoding.ASCII.GetBytes(AskCodePrefix + code);
            var sent = System.Diagnostics.Stopwatch.StartNew();
            foreach (var target in NetProtocol.BroadcastTargets())
                try { udp.Send(ask, ask.Length, new IPEndPoint(target, NetProtocol.DiscoveryPort)); } catch { }
            udp.Client.ReceiveTimeout = 100;
            while (sent.ElapsedMilliseconds < waitMs)
            {
                try
                {
                    var from = new IPEndPoint(IPAddress.Any, 0);
                    var data = udp.Receive(ref from);
                    if (data.Length < Answer.Length + 4) continue;
                    bool ok = true;
                    for (int i = 0; i < Answer.Length; i++) if (data[i] != Answer[i]) { ok = false; break; }
                    if (!ok) continue;
                    using (var r = new BinaryReader(new MemoryStream(data, Answer.Length, data.Length - Answer.Length), Encoding.UTF8))
                    {
                        var info = RoomInfo.Read(r);
                        info.port = r.ReadInt32();
                        info.address = from.Address.ToString();
                        info.pingMs = (int)sent.ElapsedMilliseconds;
                        // La mateixa sala pot respondre per loopback i per la IP real: ens quedem amb la IP real
                        if (found.TryGetValue(info.code, out var old) && !IPAddress.IsLoopback(IPAddress.Parse(old.address))) continue;
                        found[info.code] = info;
                    }
                    if (code != null) break;
                }
                catch (SocketException) { }
                catch (Exception) { }
            }
        }
        finally { udp.Close(); }
        return new List<RoomInfo>(found.Values);
    }
}
