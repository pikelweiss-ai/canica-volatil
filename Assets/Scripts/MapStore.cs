using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MapStore
{
    public const int MaxDimension = 64;
    public const int MaxFileBytes = 200 * 1024;
    public const int MaxUncompressedBytes = 1024 * 1024;
    static string DirectoryPath => Path.Combine(Application.persistentDataPath, "Mapes");

    public static bool Validate(MapData map, out string error)
    {
        error = null;
        if (map == null || map.format < 1 || map.format > 1) { error = "Versió de mapa incompatible."; return false; }
        if (map.width < 1 || map.height < 1 || map.width > MaxDimension || map.height > MaxDimension)
        { error = "Mida de mapa no vàlida."; return false; }
        if (map.layers == null || map.layers.ground == null || map.layers.objects == null || map.layers.overlay == null)
        { error = "Capes del mapa no vàlides."; return false; }
        var cells = map.layers.ground.Concat(map.layers.objects).Concat(map.layers.overlay).ToArray();
        if (cells.Length > map.width * map.height * 3 || map.spawns == null || map.chests == null)
        { error = "Nombre de blocs no vàlid."; return false; }
        if (cells.Any(c => string.IsNullOrWhiteSpace(c.blockId) || c.x < 0 || c.y < 0 ||
            c.x >= map.width || c.y >= map.height || c.rot < 0 || c.rot > 3))
        { error = "Hi ha una cel·la no vàlida."; return false; }
        foreach (var layer in new[] { map.layers.ground, map.layers.objects, map.layers.overlay })
            if (layer.Select(c => c.x + c.y * map.width).Distinct().Count() != layer.Length)
            { error = "Hi ha cel·les duplicades dins d'una capa."; return false; }
        if (map.spawns.Any(s => s.x < 0 || s.y < 0 || s.x >= map.width || s.y >= map.height ||
            (s.kind != "player" && s.kind != "enemy" && s.kind != "merchant") || (s.kind == "enemy" || s.kind == "merchant") && string.IsNullOrWhiteSpace(s.id)) ||
            map.chests.Any(c => c.x < 0 || c.y < 0 || c.x >= map.width || c.y >= map.height ||
            string.IsNullOrWhiteSpace(c.rarity)) ||
            map.goal != null && (map.goal.x < 0 || map.goal.y < 0 || map.goal.x >= map.width || map.goal.y >= map.height))
        { error = "Objectes del mapa fora dels límits."; return false; }
        return true;
    }

    public static bool Save(MapData map, string fileName, out string error)
    {
        if (!TryEncodeFile(map, out var bytes, out error)) return false;
        string path;
        try { path = SafePath(fileName); }
        catch (Exception e) { error = e.Message; return false; }
        string temporary = path + ".tmp";
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllBytes(temporary, bytes);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temporary, path);
            error = null; return true;
        }
        catch (Exception e) { error = e.Message; return false; }
    }

    public static bool TryEncodeFile(MapData map, out byte[] bytes, out string error)
    {
        bytes = null;
        if (!Validate(map, out error)) return false;
        try
        {
            byte[] json = Encoding.UTF8.GetBytes(JsonUtility.ToJson(map));
            if (json.Length > MaxUncompressedBytes) { error = "El mapa és massa gran."; return false; }
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress, true)) gzip.Write(json, 0, json.Length);
            bytes = output.ToArray();
            if (bytes.Length > MaxFileBytes) { bytes = null; error = "El mapa és massa gran."; return false; }
            error = null; return true;
        }
        catch (Exception e) { bytes = null; error = e.Message; return false; }
    }

    public static bool TryDecodeFile(byte[] bytes, out MapData map, out string error)
    {
        map = null;
        try
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > MaxFileBytes)
                throw new InvalidDataException("Fitxer absent o massa gran.");
            byte[] json;
            if (bytes.Length >= 2 && bytes[0] == 0x1f && bytes[1] == 0x8b)
            {
                using var input = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
                using var output = new MemoryStream();
                byte[] buffer = new byte[4096]; int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (output.Length + read > MaxUncompressedBytes) throw new InvalidDataException("El mapa descomprimit és massa gran.");
                    output.Write(buffer, 0, read);
                }
                json = output.ToArray();
            }
            else json = bytes;
            if (json.Length > MaxUncompressedBytes) throw new InvalidDataException("El mapa descomprimit és massa gran.");
            map = JsonUtility.FromJson<MapData>(Encoding.UTF8.GetString(json));
            if (!Validate(map, out error)) { map = null; return false; }
            error = null; return true;
        }
        catch (Exception e) { error = e.Message; map = null; return false; }
    }

    public static string[] List()
    {
        try { return Directory.Exists(DirectoryPath) ? Directory.GetFiles(DirectoryPath, "*.canicamap")
            .Select(Path.GetFileNameWithoutExtension).Where(n => n != "__autosave").OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray() : Array.Empty<string>(); }
        catch { return Array.Empty<string>(); }
    }

    public static bool Load(string fileName, out MapData map, out string error)
    {
        map = null;
        try
        {
            string path = SafePath(fileName);
            var info = new FileInfo(path);
            if (!info.Exists || info.Length > MaxFileBytes) throw new InvalidDataException("Fitxer absent o massa gran.");
            return TryDecodeFile(File.ReadAllBytes(path), out map, out error);
        }
        catch (Exception e) { error = e.Message; map = null; return false; }
    }

    public static bool Delete(string fileName)
    {
        try { string path = SafePath(fileName); if (!File.Exists(path)) return false; File.Delete(path); return true; }
        catch { return false; }
    }

    public static DateTime LastModifiedUtc(string fileName)
    {
        try { return File.GetLastWriteTimeUtc(SafePath(fileName)); }
        catch { return DateTime.MinValue; }
    }

    static string SafePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileNameWithoutExtension(fileName))
            throw new InvalidDataException("Nom de mapa no vàlid.");
        return Path.Combine(DirectoryPath, fileName + ".canicamap");
    }
}
