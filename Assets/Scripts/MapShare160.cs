using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public static class MapShare160
{
    const string Prefix = "CVM1:";
    public const int MaxCodeChars = 20000;
    public static bool Encode(MapData map, out string code, out string error)
    {
        code = null;
        if (!MapStore.Validate(map, out error)) return false;
        try
        {
            byte[] raw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(map));
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress, true)) gzip.Write(raw, 0, raw.Length);
            code = Prefix + Convert.ToBase64String(output.ToArray());
            if (code.Length > MaxCodeChars) { code = null; error = "El mapa no cap en un codi de text; comparteix el fitxer."; return false; }
            error = null; return true;
        }
        catch (Exception e) { error = e.Message; return false; }
    }
    public static bool Decode(string code, out MapData map, out string error)
    {
        map = null;
        try
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length > MaxCodeChars || !code.StartsWith(Prefix, StringComparison.Ordinal))
                throw new InvalidDataException("Codi CVM1 no vàlid o massa gran.");
            byte[] compressed = Convert.FromBase64String(code.Substring(Prefix.Length));
            using var input = new GZipStream(new MemoryStream(compressed), CompressionMode.Decompress);
            using var output = new MemoryStream(); byte[] buffer = new byte[4096]; int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            { if (output.Length + read > MapStore.MaxFileBytes) throw new InvalidDataException("El mapa descomprimit és massa gran."); output.Write(buffer, 0, read); }
            map = JsonUtility.FromJson<MapData>(Encoding.UTF8.GetString(output.ToArray()));
            if (!MapStore.Validate(map, out error)) { map = null; return false; }
            error = null; return true;
        }
        catch (Exception e) { map = null; error = e.Message; return false; }
    }
    public static bool ExportFile(MapData map, string path, out string error)
    {
        try { if (!MapStore.Validate(map, out error)) return false; File.WriteAllText(path, JsonUtility.ToJson(map)); error = null; return true; }
        catch (Exception e) { error = e.Message; return false; }
    }
    public static bool ImportFile(string path, out MapData map, out string error)
    {
        map = null;
        try
        {
            var info = new FileInfo(path); if (!info.Exists || info.Length > MapStore.MaxFileBytes) throw new InvalidDataException("Fitxer absent o massa gran.");
            map = JsonUtility.FromJson<MapData>(File.ReadAllText(path));
            if (!MapStore.Validate(map, out error)) { map = null; return false; }
            error = null; return true;
        }
        catch (Exception e) { map = null; error = e.Message; return false; }
    }
}
