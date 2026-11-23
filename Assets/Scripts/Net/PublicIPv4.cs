using System.Net;

// Classificació IPv4 per a decidir si un anunci directe és usable a Internet.
public static class PublicIPv4
{
    public static bool IsPublic(string text)
    {
        if (!IPAddress.TryParse(text, out var ip) || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
        var b = ip.GetAddressBytes();
        return !(b[0] == 0 || b[0] == 10 || b[0] == 127 ||
                 (b[0] == 100 && b[1] >= 64 && b[1] <= 127) ||
                 (b[0] == 169 && b[1] == 254) ||
                 (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
                 (b[0] == 192 && b[1] == 168) ||
                 (b[0] == 192 && b[1] == 0) ||
                 (b[0] == 192 && b[1] == 0 && b[2] == 2) ||
                 (b[0] == 198 && (b[1] == 18 || b[1] == 19)) ||
                 (b[0] == 198 && b[1] == 51 && b[2] == 100) ||
                 (b[0] == 203 && b[1] == 0 && b[2] == 113) ||
                 b[0] >= 224);
    }
}
