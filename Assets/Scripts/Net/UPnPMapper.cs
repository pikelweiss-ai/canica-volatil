using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

// Mapeig IGD d'IPv4 sense paquets externs. Cridar-lo sempre des d'un fil de fons.
public sealed class UPnPMapper
{
    const int LeaseSeconds = 3600;
    readonly string controlUrl, serviceType, localAddress;
    readonly int port;
    public string ExternalAddress { get; private set; }
    int renewGeneration;

    UPnPMapper(string url, string service, string local, int port)
    { controlUrl = url; serviceType = service; localAddress = local; this.port = port; }

    public static UPnPMapper TryOpen(int port, out string reason)
    {
        reason = null;
        try
        {
            IPAddress gateway;
            var location = Discover(out gateway);
            string local = LocalIPv4(gateway);
            string xml;
            using (var wc = new WebClient()) { wc.Headers[HttpRequestHeader.Connection] = "close"; xml = wc.DownloadString(location); }
            var match = Regex.Match(xml, @"<serviceType>([^<]*(?:WANIPConnection|WANPPPConnection)[^<]*)</serviceType>[\s\S]{0,2000}?<controlURL>([^<]+)</controlURL>", RegexOptions.IgnoreCase);
            if (!match.Success) throw new IOException("el router no anuncia WANIPConnection");
            var baseUri = new Uri(location);
            string control = new Uri(baseUri, WebUtility.HtmlDecode(match.Groups[2].Value)).ToString();
            string service = match.Groups[1].Value;
            var mapper = new UPnPMapper(control, service, local, port);
            mapper.ExternalAddress = mapper.Call("GetExternalIPAddress", "");
            if (!PublicIPv4.IsPublic(mapper.ExternalAddress)) throw new IOException("el router té IP privada o CGNAT (" + mapper.ExternalAddress + ")");
            string args = "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + port + "</NewExternalPort><NewProtocol>TCP</NewProtocol><NewInternalPort>" + port + "</NewInternalPort><NewInternalClient>" + local + "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>Canica Volatil</NewPortMappingDescription><NewLeaseDuration>" + LeaseSeconds + "</NewLeaseDuration>";
            mapper.Call("AddPortMapping", args);
            mapper.StartRenewal();
            return mapper;
        }
        catch (Exception e) { reason = e.Message; return null; }
    }

    public string Endpoint => ExternalAddress + ":" + port;

    public void Close()
    {
        Interlocked.Increment(ref renewGeneration);
        try { Call("DeletePortMapping", "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + port + "</NewExternalPort><NewProtocol>TCP</NewProtocol>"); } catch { }
    }

    void StartRenewal()
    {
        int generation = Interlocked.Increment(ref renewGeneration);
        new Thread(() =>
        {
            while (generation == Volatile.Read(ref renewGeneration))
            {
                Thread.Sleep(TimeSpan.FromMinutes(30));
                if (generation != Volatile.Read(ref renewGeneration)) break;
                try
                {
                    string args = "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + port + "</NewExternalPort><NewProtocol>TCP</NewProtocol><NewInternalPort>" + port + "</NewInternalPort><NewInternalClient>" + localAddress + "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>Canica Volatil</NewPortMappingDescription><NewLeaseDuration>" + LeaseSeconds + "</NewLeaseDuration>";
                    Call("AddPortMapping", args);
                }
                catch { }
            }
        }) { IsBackground = true, Name = "UPnP lease Canica" }.Start();
    }

    string Call(string action, string args)
    {
        string envelope = "<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body><u:" + action + " xmlns:u=\"" + serviceType + "\">" + args + "</u:" + action + "></s:Body></s:Envelope>";
        var req = (HttpWebRequest)WebRequest.Create(controlUrl);
        req.Method = "POST"; req.Timeout = 3000; req.ReadWriteTimeout = 3000;
        req.ContentType = "text/xml; charset=\"utf-8\""; req.Headers.Add("SOAPACTION", "\"" + serviceType + "#" + action + "\"");
        byte[] bytes = Encoding.UTF8.GetBytes(envelope); req.ContentLength = bytes.Length;
        using (var stream = req.GetRequestStream()) stream.Write(bytes, 0, bytes.Length);
        using (var response = (HttpWebResponse)req.GetResponse())
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
            string xml = reader.ReadToEnd();
            if (action == "GetExternalIPAddress")
            {
                var m = Regex.Match(xml, @"<NewExternalIPAddress>([^<]+)</NewExternalIPAddress>");
                if (!m.Success) throw new IOException("el router no dona IP externa");
                return m.Groups[1].Value;
            }
            return null;
        }
    }

    static string Discover(out IPAddress gateway)
    {
        using (var udp = new UdpClient(0))
        {
            udp.Client.ReceiveTimeout = 1800;
            string query = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:device:InternetGatewayDevice:1\r\n\r\n";
            byte[] data = Encoding.ASCII.GetBytes(query);
            udp.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900));
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            var result = udp.Receive(ref sender);
            string headers = Encoding.ASCII.GetString(result);
            var match = Regex.Match(headers, @"(?im)^LOCATION:\s*(\S+)");
            if (!match.Success) throw new IOException("cap router UPnP trobat");
            gateway = sender.Address;
            return match.Groups[1].Value.Trim();
        }
    }

    static string LocalIPv4(IPAddress gateway)
    {
        using (var route = new UdpClient())
        {
            route.Connect(gateway, 1900);
            var local = ((IPEndPoint)route.Client.LocalEndPoint).Address;
            if (local.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(local)) return local.ToString();
        }
        throw new IOException("no s'ha trobat una IPv4 local cap al router");
    }
}
