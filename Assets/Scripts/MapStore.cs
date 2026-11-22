using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class MapStore
{
    public const int MaxDimension = 64;
    public const int MaxFileBytes = 200 * 1024;
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
            (s.kind != "player" && s.kind != "enemy") || s.kind == "enemy" && string.IsNullOrWhiteSpace(s.id)) ||
            map.chests.Any(c => c.x < 0 || c.y < 0 || c.x >= map.width || c.y >= map.height ||
            string.IsNullOrWhiteSpace(c.rarity)) ||
            map.goal != null && (map.goal.x < 0 || map.goal.y < 0 || map.goal.x >= map.width || map.goal.y >= map.height))
        { error = "Objectes del mapa fora dels límits."; return false; }
        return true;
    }

    public static bool Save(MapData map, string fileName, out string error)
    {
        if (!Validate(map, out error)) return false;
        string path;
        try { path = SafePath(fileName); }
        catch (Exception e) { error = e.Message; return false; }
        string json = JsonUtility.ToJson(map);
        if (System.Text.Encoding.UTF8.GetByteCount(json) > MaxFileBytes) { error = "El mapa és massa gran."; return false; }
        string temporary = path + ".tmp";
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(temporary, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temporary, path);
            error = null; return true;
        }
        catch (Exception e) { error = e.Message; return false; }
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
            map = JsonUtility.FromJson<MapData>(File.ReadAllText(path));
            if (!Validate(map, out error)) { map = null; return false; }
            error = null; return true;
        }
        catch (Exception e) { error = e.Message; map = null; return false; }
    }

    public static bool Delete(string fileName)
    {
        try { string path = SafePath(fileName); if (!File.Exists(path)) return false; File.Delete(path); return true; }
        catch { return false; }
    }

    static string SafePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileNameWithoutExtension(fileName))
            throw new InvalidDataException("Nom de mapa no vàlid.");
        return Path.Combine(DirectoryPath, fileName + ".canicamap");
    }
}
