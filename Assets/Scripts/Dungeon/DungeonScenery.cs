using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// v1.4.1: mès varietat de blocs a les masmorres. Cada tema té el seu joc de rajoles de terra, parets amb mineral/roca,
// objectes (troncs, roques, bolets, cactus...) i decoració de terra (flors, pedretes). Tot ix de les textures de
// Assets/Resources/Pixel/Blocs i es decideix per posició o amb el mateix rng de la generació: igual a tots els equips.
public partial class DungeonGenerator
{
    class SceneryKit
    {
        public string[] floors = new string[0], walls = new string[0], props = new string[0], decals = new string[0];
        public Color tint = Color.white;
        public float floorShare = .16f, wallShare = .09f;
    }

    static SceneryKit Kit(DungeonTheme theme)
    {
        string n = theme != null ? theme.themeName.ToLowerInvariant() : "";
        if (n.Contains("cristall")) return new SceneryKit
        {
            walls = new[] { "Mineral_ametista", "Mineral_diamant", "Mineral_maragda" }, wallShare = .05f,
            props = new[] { "Bolet_lluminos", "Bolet_lila", "Roca_petita", "Pedres" }, decals = new[] { "Pedres", "Bolet_lluminos", "Roca_petita" }
        };
        if (n.Contains("cova")) return new SceneryKit
        {
            floors = new[] { "Roca_cova_1", "Roca_cova_marro", "Grava", "Roca_cova_molsa" }, walls = new[] { "Mineral_carbo", "Mineral_ferro", "Mineral_coure", "Roca_cova_molsa" },
            props = new[] { "Roca_gran", "Roca_petita", "Roca_musgo", "Pedres", "Bolet_marro", "Bolets_grup", "Bolet_lluminos", "Soca" }, decals = new[] { "Pedres", "Roca_petita", "Bolet_marro", "Mata_herba" }
        };
        if (n.Contains("bosc")) return new SceneryKit
        {
            floors = new[] { "Gespa_alta", "Gespa_trevols", "Gespa_flors_mixtes", "Fulles_verd", "Molsa", "Gespa_fosca" }, floorShare = .3f, walls = new[] { "Molsa", "Fulles_verd", "Roca_cova_molsa" },
            props = new[] { "Tronc_pi", "Tronc_betula", "Tronc_caigut", "Soca", "Seto_verd", "Seto_baies", "Seto_dens", "Falguera", "Bolet_vermell", "Bolets_grup" },
            decals = new[] { "Flor_margarida", "Flor_lavanda", "Flor_rosa", "Flor_tulipa_groga", "Flor_lliri_blau", "Mata_herba", "Falguera", "Flor_rosa_blanca" }
        };
        if (n.Contains("desert")) return new SceneryKit
        {
            floors = new[] { "Sorra", "Grava", "Terra", "Gespa_seca" }, walls = new[] { "Mineral_or", "Mineral_coure", "Roca_cova_marro" },
            props = new[] { "Cactus", "Cactus_petit", "Roca_gran", "Roca_petita", "Soca", "Pedres" }, decals = new[] { "Mata_herba", "Pedres", "Cactus_petit", "Roca_petita" }
        };
        if (n.Contains("claveguera")) return new SceneryKit
        {
            floors = new[] { "Formigo", "Formigo_fosc", "Reixa_metall", "Grava", "Antilliscant_rovellat" }, walls = new[] { "Roca_cova_molsa", "Antilliscant_rovellat", "Metall_placa" },
            props = new[] { "Canyes", "Roca_musgo", "Bolet_marro", "Paperera", "Carro_compra", "Armari_metal" }, decals = new[] { "Nenufar", "Canyes", "Mata_herba", "Pedres", "Bolet_marro" }
        };
        if (n.Contains("neu")) return new SceneryKit
        {
            floors = new[] { "Neu_1", "Neu_2", "Neu_pisada" }, walls = new[] { "Gel_glacera", "Gel_esquerdat" },
            props = new[] { "Seto_neu", "Roca_gran", "Roca_petita", "Pedres", "Tronc_pi" }, decals = new[] { "Pedres", "Mata_herba", "Roca_petita" }
        };
        if (n.Contains("cripta")) return new SceneryKit
        {
            floors = new[] { "Marbre", "Escacs", "Pissarra", "Rajola_negra" }, walls = new[] { "Castell_fosc", "Castell_blocs" },
            props = new[] { "Roca_musgo", "Pedres", "Bolet_lila", "Soca", "Armari_metal", "Llum_peu" }, decals = new[] { "Pedres", "Bolet_lila", "Roca_petita" }
        };
        if (n.Contains("volc")) return new SceneryKit
        {
            floors = new[] { "Roca_cova_magma", "Terra", "Pissarra", "Grava" }, floorShare = .2f, walls = new[] { "Mineral_robi", "Roca_cova_magma", "Mineral_carbo" },
            props = new[] { "Roca_gran", "Roca_petita", "Pedres", "Soca" }, decals = new[] { "Pedres", "Roca_petita" }, tint = new Color(1f, .78f, .7f)
        };
        return new SceneryKit();
    }

    static readonly Dictionary<string, Sprite> blockSprites = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Tile> blockTiles = new Dictionary<string, Tile>();

    static Sprite Block(string name)
    {
        if (blockSprites.TryGetValue(name, out var s) && s != null) return s;
        s = Resources.Load<Sprite>("Pixel/Blocs/" + name); blockSprites[name] = s; return s;
    }

    static Tile BlockTile(string name, Tile.ColliderType collider)
    {
        string key = name + (int)collider;
        if (blockTiles.TryGetValue(key, out var t) && t != null) return t;
        var sprite = Block(name); if (sprite == null) return null;
        t = ScriptableObject.CreateInstance<Tile>(); t.sprite = sprite; t.name = key; t.colliderType = collider;
        blockTiles[key] = t; return t;
    }

    static int PosHash(Vector2Int p, int salt)
    {
        int h = (p.x * 73856093) ^ (p.y * 19349663) ^ (salt * 83492791);
        return (h ^ (h >> 13)) & 0x7fffffff;
    }

    // Després de pintar el terra i les parets: algunes rajoles es canvien per variants del tema (sense gastar rng).
    // Variants d'una rajola del MATEIX tema (més clara, més fosca, amb esquerdes o amb detalls), perquè el terra i les
    // parets varien sense que cap bloc xoque amb el to dels altres. Es creen a mà a partir del sprite original.
    static readonly Dictionary<string, Tile> variantTiles = new Dictionary<string, Tile>();

    static Tile VariantOf(Tile source, int kind, Tile.ColliderType collider)
    {
        if (source == null || source.sprite == null) return null;
        var sprite = source.sprite; string key = sprite.GetInstanceID() + "_" + kind + "_" + (int)collider;
        if (variantTiles.TryGetValue(key, out var cached) && cached != null) return cached;
        var texture = sprite.texture; if (texture == null || !texture.isReadable) return null;
        var rect = sprite.rect; int w = (int)rect.width, h = (int)rect.height;
        var pixels = texture.GetPixels((int)rect.x, (int)rect.y, w, h);
        var random = new System.Random(key.GetHashCode());
        void Scale(int index, float factor) { var c = pixels[index]; if (c.a <= 0f) return; pixels[index] = new Color(Mathf.Clamp01(c.r * factor), Mathf.Clamp01(c.g * factor), Mathf.Clamp01(c.b * factor), c.a); }
        switch (kind)
        {
            case 0: for (int i = 0; i < pixels.Length; i++) Scale(i, 1.09f); break;            // més clara
            case 1: for (int i = 0; i < pixels.Length; i++) Scale(i, .88f); break;             // més fosca
            case 2:                                                                             // esquerdes: un camí en ziga-zaga fosc
            {
                int x = random.Next(3, w - 3), y = h - 1;
                for (int step = 0; step < h - 1; step++)
                {
                    Scale(y * w + x, .62f);
                    if (random.Next(3) == 0 && x + 1 < w) Scale(y * w + x + 1, .8f);
                    y--; x = Mathf.Clamp(x + random.Next(-1, 2), 1, w - 2);
                    if (y < 0) break;
                }
                break;
            }
            default:                                                                            // detalls: uns quants píxels més clars i més foscos
                for (int i = 0; i < 5; i++) Scale(random.Next(pixels.Length), 1.2f);
                for (int i = 0; i < 4; i++) Scale(random.Next(pixels.Length), .84f);
                break;
        }
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        tex.SetPixels(pixels); tex.Apply(false, false);
        var variant = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height), sprite.pixelsPerUnit);
        var tile = ScriptableObject.CreateInstance<Tile>(); tile.sprite = variant; tile.name = key; tile.colliderType = collider; tile.color = source.color;
        variantTiles[key] = tile; return tile;
    }

    void ApplyScenery()
    {
        var kit = Kit(Theme);
        // Terra: variants de la rajola del tema, repartides per posició (igual a tots els equips)
        foreach (Room room in Rooms)
        {
            if (room.modifier == RoomModifier.Ice || room.type == RoomType.Start || room.type == RoomType.Treasure) continue; // refugi i gel tenen el seu terra
            foreach (Vector2Int p in room.rect.allPositionsWithin)
            {
                if (!floor.Contains(p) || !InsideShape(room, p)) continue;
                int h = PosHash(p, 1);
                if (h % 1000 >= 240) continue; // 24 % de les rajoles
                var cell = ToCell(p);
                var variant = VariantOf(ground.GetTile(cell) as Tile, (h / 1000) % 4, Tile.ColliderType.None);
                if (variant != null) ground.SetTile(cell, variant);
            }
        }
        // Parets: variants de la paret del tema; i, només a la cova de cristall, algun mineral encastat (ametista, diamant...)
        bool crystal = Theme != null && Theme.themeName.ToLowerInvariant().Contains("cristall");
        if (walls != null)
            foreach (Vector3Int cell in walls.cellBounds.allPositionsWithin)
            {
                if (!walls.HasTile(cell)) continue;
                var tile = walls.GetTile(cell) as Tile; if (tile == null) continue;
                int h = PosHash(new Vector2Int(cell.x, cell.y), 2);
                Tile replacement = null;
                if (crystal && kit.walls.Length > 0 && h % 1000 < kit.wallShare * 1000f) replacement = BlockTile(kit.walls[(h / 1000) % kit.walls.Length], tile.colliderType);
                else if (h % 1000 < 200) replacement = VariantOf(tile, (h / 1000) % 4, tile.colliderType);
                if (replacement != null) walls.SetTile(cell, replacement);
            }
    }

    // Objectes i decoració de les sales (rng compartit: mateix resultat a tots els equips).
    void ScatterScenery()
    {
        var kit = Kit(Theme);
        if (kit.props.Length == 0 && kit.decals.Length == 0) return;
        foreach (Room room in Rooms)
        {
            if (room.type == RoomType.Boss) continue;
            int area = room.rect.width * room.rect.height;
            bool fighting = room.type == RoomType.Combat || room.type == RoomType.Arena || room.type == RoomType.Emboscada;
            int props = fighting ? Mathf.Clamp(area / 22, 2, 6) : 0;
            int decals = Mathf.Clamp(area / 9, 3, 14);
            for (int i = 0, tries = 0; i < props && tries < 60; tries++)
            {
                var cell = RandomCell(room);
                if (!FreeForDecor(room, cell, 2)) continue;
                reserved.Add(cell); MakeScenery(kit.props[rng.Next(kit.props.Length)], cell, kit, false); i++;
            }
            for (int i = 0, tries = 0; i < decals && tries < 80; tries++)
            {
                var cell = RandomCell(room);
                if (!floor.Contains(cell) || !InsideShape(room, cell) || solid.Contains(cell) || reserved.Contains(cell)) continue;
                if (i == 0 && kit.decals.Length == 0) break;
                MakeScenery(kit.decals[rng.Next(kit.decals.Length)], cell, kit, true); i++;
            }
        }
    }

    void MakeScenery(string name, Vector2Int cell, SceneryKit kit, bool decal)
    {
        var sprite = Block(name); if (sprite == null) return;
        Vector3 pos = ground.GetCellCenterWorld(ToCell(cell));
        var go = new GameObject((decal ? "Decoracio " : "Objecte ") + name);
        go.transform.position = pos; go.transform.SetParent(transform);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = kit.tint;
        if (decal) { sr.sortingLayerName = "Shadows"; sr.sortingOrder = 2; if (rng.Next(2) == 0) sr.flipX = true; return; }
        sr.sortingLayerName = "Player"; sr.sortingOrder = Mathf.Clamp(Mathf.RoundToInt(-pos.y * 8f), -30000, 30000);
        // Ombra semitransparent al peu (bolets, roques, troncs...), igual que la resta d objectes
        var shadow = Fx.MakeSprite("Ombra", pos + new Vector3(0f, -.24f, 0f), Fx.Disc, new Color(0f, 0f, 0f, .32f), "Shadows", 1);
        shadow.transform.SetParent(go.transform, true); shadow.transform.localScale = new Vector3(name.StartsWith("Tronc") || name.StartsWith("Roca_gran") ? .85f : .6f, .24f, 1f);
        var box = go.AddComponent<BoxCollider2D>(); box.size = new Vector2(.65f, .55f); box.offset = new Vector2(0, -.1f);
        var b = go.AddComponent<Breakable>();
        bool hard = name.StartsWith("Roca") || name == "Pedres" || name.StartsWith("Soca") || name.StartsWith("Tronc") || name == "Armari_metal" || name == "Llum_peu" || name == "Carro_compra";
        bool mushroom = name.StartsWith("Bolet");
        b.health = hard ? 4 : mushroom || name.StartsWith("Cactus") ? 2 : 1;
        b.coinChance = hard ? .08f : .15f; b.healChance = mushroom ? .3f : hard ? .04f : .12f;
        b.debrisColor = name.StartsWith("Roca") || name == "Pedres" || name == "Armari_metal" || name == "Paperera" ? new Color(.55f, .55f, .6f)
            : name.StartsWith("Tronc") || name == "Soca" || name == "Carro_compra" ? new Color(.45f, .3f, .15f)
            : mushroom ? new Color(.8f, .4f, .5f) : new Color(.3f, .65f, .3f);
    }
}
