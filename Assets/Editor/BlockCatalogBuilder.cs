using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BlockCatalogBuilder
{
    const string Source = "ArtNou_v1.4/Blocs";
    const string Textures = "Assets/Resources/Pixel/Blocs/Cataleg";
    const string Data = "Assets/Blocks";
    const string CatalogPath = "Assets/Resources/BlockCatalog.asset";

    [MenuItem("Canica/Almanac de blocs v1.5")]
    public static void Build()
    {
        var sources = Directory.GetDirectories(Source)
            .OrderBy(path => path, StringComparer.Ordinal)
            .SelectMany(folder => Directory.GetFiles(folder, "*.png")
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => (family: Path.GetFileName(folder), name: Path.GetFileNameWithoutExtension(path), path)))
            .ToArray();
        var imports = new List<(string family, string name, string sprite, string legacy)>();
        foreach (var entry in sources)
        {
            string legacy = "Assets/Resources/Pixel/Blocs/" + entry.name + ".png";
            string sprite = Textures + "/" + entry.family + "/" + entry.name + ".png";
            byte[] bytes = File.ReadAllBytes(entry.path);
            if (File.Exists(legacy) && File.ReadAllBytes(legacy).SequenceEqual(bytes))
                sprite = legacy;
            else
            {
                Directory.CreateDirectory(Textures + "/" + entry.family);
                if (!File.Exists(sprite) || !File.ReadAllBytes(sprite).SequenceEqual(bytes)) File.WriteAllBytes(sprite, bytes);
            }
            Directory.CreateDirectory(Data + "/" + entry.family);
            imports.Add((entry.family, entry.name, sprite, sprite == legacy ? legacy : null));
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var entries = new List<BlockData>(imports.Count);
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var entry in imports)
            {
                string id = entry.family + "/" + entry.name;
                string path = Data + "/" + id + ".asset";
                var block = AssetDatabase.LoadAssetAtPath<BlockData>(path);
                if (block == null)
                {
                    block = ScriptableObject.CreateInstance<BlockData>();
                    AssetDatabase.CreateAsset(block, path);
                }

                block.id = id;
                block.displayName = entry.name.Replace('_', ' ');
                block.family = entry.family == "Vegetacio" ? "Vegetació" : entry.family;
                block.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(entry.sprite);
                block.scenerySprite = entry.legacy != null ? AssetDatabase.LoadAssetAtPath<Sprite>(entry.legacy) : null;
                if (block.sprite == null) throw new InvalidOperationException("Falta la textura de " + id);
                ApplyProperties(block, entry.name);
                EditorUtility.SetDirty(block);
                entries.Add(block);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        var catalog = AssetDatabase.LoadAssetAtPath<BlockCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<BlockCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        catalog.blocks = entries.ToArray();
        catalog.themes = Directory.GetFiles("Assets/Dungeon/Temes", "*.asset")
            .OrderBy(path => path, StringComparer.Ordinal).Select(path => AssetDatabase.LoadAssetAtPath<DungeonTheme>(path.Replace('\\', '/')))
            .Where(theme => theme != null).ToArray();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log("BLOCK_CATALOG: " + entries.Count + " blocs");
    }

    static void ApplyProperties(BlockData block, string name)
    {
        string lower = name.ToLowerInvariant();
        bool trunk = lower.StartsWith("tronc") || lower.StartsWith("soca");
        bool weed = lower.StartsWith("flor") || lower.StartsWith("mata_herba") || lower.Contains("nenufar") ||
                    lower.Contains("molsa") || lower.Contains("herba") || lower.Contains("canyes");
        bool liquid = lower.StartsWith("aigua") || lower.StartsWith("lava") || lower.StartsWith("acid");
        bool floor = block.family == "Terres" || (block.family == "Natura" && !trunk &&
            (weed || lower.StartsWith("gespa") || lower.StartsWith("sorra") || lower.StartsWith("terra")));
        bool wall = block.family == "Parets";
        bool grass = block.family == "Vegetació" && !trunk &&
            !lower.StartsWith("roca") && !lower.StartsWith("cactus") && !lower.StartsWith("bolet");

        block.passable = floor || grass || liquid;
        block.blocksBullets = !block.passable;
        block.breakable = !block.passable && !wall && !trunk;
        block.hideInside = lower.StartsWith("seto");
        block.damage = lower.StartsWith("lava") || lower.StartsWith("acid") ? 1f : 0f;
        block.multiPart = lower.EndsWith("_esq") || lower.EndsWith("_mig") || lower.EndsWith("_dre");
        block.emitsLight = lower.Contains("lluminos") || lower.Contains("llum_peu") || lower.Contains("ences");
        block.layer = block.passable ? BlockData.DrawLayer.UnderPlayer : BlockData.DrawLayer.OverPlayer;
        block.fullBlock = wall || trunk || floor;
        block.hitbox = block.passable ? default : block.fullBlock ? new Rect(-.5f, -.5f, 1f, 1f) : SpriteBounds(block.sprite);

        block.theme = lower.Contains("neu") || lower.StartsWith("gel") ? "Neu" :
            lower.StartsWith("lava") || lower.Contains("magma") ? "Volcà" :
            lower.Contains("sorra") || lower.StartsWith("cactus") ? "Desert" :
            lower.Contains("ametista") || lower.Contains("cristall") ? "Cristall" :
            block.family == "Troncs" || block.family == "Vegetació" ? "Bosc" :
            block.family == "Metall" || block.family == "Port" ? "Claveguera" :
            block.family == "Escola" || block.family == "Laboratori" || block.family == "Cuina" ||
                block.family == "Oficina" || block.family == "Hospital" || block.family == "Bany" ? block.family :
            lower.StartsWith("castell") ? "Cripta" : "General";
        block.rarity = lower.Contains("diamant") ? ChestRarity.Llegendari :
            lower.StartsWith("mineral") ? ChestRarity.Epic :
            block.emitsLight || block.family == "Metall" ? ChestRarity.Rar : ChestRarity.Comu;
    }

    static Rect SpriteBounds(Sprite sprite)
    {
        var tex = sprite.texture;
        if (!tex.isReadable) return new Rect(-.5f, -.5f, 1f, 1f);
        var rect = sprite.rect;
        int w = (int)rect.width, h = (int)rect.height;
        int minX = w, minY = h, maxX = -1, maxY = -1;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (tex.GetPixel(x + (int)rect.x, y + (int)rect.y).a < .1f) continue;
                minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y);
            }
        if (maxX < 0) return new Rect(-.5f, -.5f, 1f, 1f);
        return new Rect(minX / (float)w - .5f, minY / (float)h - .5f,
            (maxX - minX + 1f) / w, (maxY - minY + 1f) / h);
    }
}
