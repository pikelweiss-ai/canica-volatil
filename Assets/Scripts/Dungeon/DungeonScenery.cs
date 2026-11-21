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
        public bool webs;                    // teranyines als racons de les sales
        public bool lilies;                  // nenúfars (només on hi ha aigua)
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
            props = new[] { "Roca_gran", "Roca_petita", "Roca_musgo", "Pedres", "Bolet_marro", "Bolets_grup", "Bolet_lluminos", "Soca" }, decals = new[] { "Pedres", "Roca_petita", "Bolet_marro" }, webs = true
        };
        if (n.Contains("bosc")) return new SceneryKit
        {
            floors = new[] { "Gespa_alta", "Gespa_trevols", "Gespa_flors_mixtes", "Fulles_verd", "Molsa", "Gespa_fosca" }, floorShare = .3f, walls = new[] { "Molsa", "Fulles_verd", "Roca_cova_molsa" },
            props = new[] { "Tronc_caigut", "Tronc_caigut_cap", "Tronc_caigut", "Soca", "Seto_verd", "Seto_baies", "Seto_dens", "Falguera", "Bolet_vermell", "Bolets_grup" },
            decals = new[] { "Flor_margarida", "Flor_lavanda", "Flor_rosa", "Flor_tulipa_groga", "Flor_lliri_blau", "Mata_herba", "Falguera", "Flor_rosa_blanca" }, webs = true, lilies = true
        };
        if (n.Contains("desert")) return new SceneryKit
        {
            floors = new[] { "Sorra", "Grava", "Terra", "Gespa_seca" }, walls = new[] { "Mineral_or", "Mineral_coure", "Roca_cova_marro" },
            props = new[] { "Cactus", "Cactus_petit", "Roca_gran", "Roca_petita", "Soca", "Pedres" }, decals = new[] { "Mata_herba", "Pedres", "Cactus_petit", "Roca_petita" }
        };
        if (n.Contains("claveguera")) return new SceneryKit
        {
            floors = new[] { "Formigo", "Formigo_fosc", "Reixa_metall", "Grava", "Antilliscant_rovellat" }, walls = new[] { "Roca_cova_molsa", "Antilliscant_rovellat", "Metall_placa" },
            props = new[] { "Canyes", "Roca_musgo", "Bolet_marro", "Paperera", "Carro_compra", "Armari_metal" }, decals = new[] { "Pedres", "Bolet_marro", "Roca_petita" }, webs = true, lilies = true
        };
        if (n.Contains("neu")) return new SceneryKit
        {
            floors = new[] { "Neu_1", "Neu_2", "Neu_pisada" }, walls = new[] { "Gel_glacera", "Gel_esquerdat" },
            props = new[] { "Seto_neu", "Roca_gran", "Roca_petita", "Pedres", "Tronc_caigut" }, decals = new[] { "Pedres", "Roca_petita" }
        };
        if (n.Contains("cripta")) return new SceneryKit
        {
            floors = new[] { "Marbre", "Escacs", "Pissarra", "Rajola_negra" }, walls = new[] { "Castell_fosc", "Castell_blocs" },
            props = new[] { "Roca_musgo", "Pedres", "Bolet_lila", "Taula_fusta", "Armari_metal", "Llum_peu" }, decals = new[] { "Pedres", "Bolet_lila", "Roca_petita" }, webs = true
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

    // Estil alternatiu de cada tema: base diferent per al terra i les parets (textures de Ferran o d'altres blocs, tenyides).
    // Cada partida i pis tria a l'atzar (per llavor, igual a tots els equips) entre l'estil de sempre i l'alternatiu.
    class AltStyle { public string[] floors; public string wall; public Color floorTint = Color.white, wallTint = Color.white; }

    static AltStyle AltFor(DungeonTheme theme)
    {
        string n = theme != null ? theme.themeName.ToLowerInvariant() : "";
        if (n.Contains("cristall")) return new AltStyle { floors = new[] { "Ferran_Bolas_1" }, floorTint = new Color(.5f, .55f, .85f), wall = "Roca_cova_blava" };
        if (n.Contains("cova")) return new AltStyle { floors = new[] { "Ferran_Cobble_1", "Ferran_Cobble_2" }, floorTint = new Color(.62f, .58f, .55f), wall = "Ferran_Bricks_2", wallTint = new Color(.42f, .4f, .42f) };
        if (n.Contains("bosc")) return new AltStyle { floors = new[] { "Gespa_alta", "Gespa_trevols" }, wall = "Fulles_verd" };
        if (n.Contains("desert")) return new AltStyle { floors = new[] { "Ferran_Cobble_2", "Sorra" }, floorTint = new Color(.88f, .74f, .52f), wall = "Ferran_Bricks_3", wallTint = new Color(.8f, .62f, .4f) };
        if (n.Contains("claveguera")) return new AltStyle { floors = new[] { "Ferran_Mosaico_1" }, floorTint = new Color(.45f, .55f, .5f), wall = "Ferran_Bricks_1", wallTint = new Color(.4f, .5f, .45f) };
        if (n.Contains("neu")) return new AltStyle { floors = new[] { "Neu_1", "Neu_2" }, wall = "Ferran_Bricks_4", wallTint = new Color(.75f, .85f, .95f) };
        if (n.Contains("cripta")) return new AltStyle { floors = new[] { "Ferran_Parches_1" }, floorTint = new Color(.55f, .5f, .6f), wall = "Ferran_Bricks_3", wallTint = new Color(.35f, .32f, .4f) };
        if (n.Contains("volc")) return new AltStyle { floors = new[] { "Ferran_Cobble_1" }, floorTint = new Color(.5f, .33f, .3f), wall = "Ferran_Bricks_2", wallTint = new Color(.35f, .22f, .22f) };
        return null;
    }

    static Tile TintedTile(string name, Color tint, Tile.ColliderType collider)
    {
        string key = "alt_" + name + "_" + ColorUtility.ToHtmlStringRGB(tint) + (int)collider;
        if (blockTiles.TryGetValue(key, out var t) && t != null) return t;
        var sprite = Block(name); if (sprite == null) return null;
        t = ScriptableObject.CreateInstance<Tile>(); t.sprite = sprite; t.name = key; t.colliderType = collider; t.color = tint;
        blockTiles[key] = t; return t;
    }

    void ApplyAltStyle(AltStyle alt)
    {
        var excluded = new HashSet<Vector2Int>();
        foreach (Room room in Rooms)
            if (room.modifier == RoomModifier.Ice || room.type == RoomType.Start || room.type == RoomType.Treasure)
                foreach (Vector2Int p in room.rect.allPositionsWithin) excluded.Add(p);
        foreach (Vector2Int p in floor)
        {
            if (excluded.Contains(p)) continue;
            int h = PosHash(p, 5);
            var tile = TintedTile(alt.floors[h % 4 == 0 && alt.floors.Length > 1 ? 1 + (h / 4) % (alt.floors.Length - 1) : 0], alt.floorTint, Tile.ColliderType.None);
            if (tile != null) ground.SetTile(ToCell(p), tile);
        }
        if (walls != null)
            foreach (Vector3Int cell in walls.cellBounds.allPositionsWithin)
            {
                if (!walls.HasTile(cell)) continue;
                var collider = (walls.GetTile(cell) as Tile)?.colliderType ?? Tile.ColliderType.Grid;
                var tile = TintedTile(alt.wall, alt.wallTint, collider);
                if (tile != null) walls.SetTile(cell, tile);
            }
    }

    void ApplyScenery()
    {
        var kit = Kit(Theme);
        var alt = AltFor(Theme);
        int styleSeed = (NetSession.Active && NetSession.Instance.Seed != 0 ? NetSession.Instance.Seed : RunState.Seed) ^ (RunState.Floor * 7919);
        if (alt != null && (styleSeed & 0x7fffffff) % 2 == 1) ApplyAltStyle(alt);
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
        ScatterCornerWebsAndLilies(kit);
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

    // Teranyina de racó (dibuixada per codi): llegeix-se com un racó dalt a l'esquerra i es gira per als altres tres.
    static Sprite cobweb;
    static Sprite Cobweb
    {
        get
        {
            if (cobweb != null) return cobweb;
            const int size = 16; var tex = new Texture2D(size, size) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            tex.SetPixels(new Color[size * size]);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x, dy = size - 1 - y, d = Mathf.Sqrt(dx * dx + dy * dy);       // distància al racó dalt-esquerra
                    if (d > 15f) continue;
                    float a = Mathf.Atan2(dy, dx) * 4f / (Mathf.PI * .5f);                     // 4 radis dins del quart de cercle
                    bool spoke = Mathf.Abs(a - Mathf.Round(a)) * d < .6f;
                    bool arc = (Mathf.Abs(d - 5f) < .6f || Mathf.Abs(d - 9.5f) < .6f || Mathf.Abs(d - 14f) < .6f) && d > 1f;
                    if (spoke || arc) tex.SetPixel(x, y, new Color(.92f, .92f, .96f, d > 11f ? .5f : .75f));
                }
            tex.Apply(); cobweb = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size); return cobweb;
        }
    }

    // Racons de sala (terra amb dues parets contigües): hi van les teranyines; i nenúfars només sobre l'aigua.
    void ScatterCornerWebsAndLilies(SceneryKit kit)
    {
        foreach (Room room in Rooms)
        {
            if (room.type == RoomType.Boss) continue;
            foreach (Vector2Int p in room.rect.allPositionsWithin)
            {
                if (!floor.Contains(p) || !InsideShape(room, p)) continue;
                var cell = ToCell(p);
                if (kit.lilies && liquid != null && liquid.HasTile(cell) && PosHash(p, 7) % 1000 < 110)
                {   // nenúfar sobre l'aigua
                    var lily = Block("Nenufar"); if (lily == null) continue;
                    var li = new GameObject("Nenufar"); li.transform.position = ground.GetCellCenterWorld(cell); li.transform.SetParent(transform);
                    var lr = li.AddComponent<SpriteRenderer>(); lr.sprite = lily; lr.sortingLayerName = "Shadows"; lr.sortingOrder = 2;
                    continue;
                }
                if (!kit.webs || (liquid != null && liquid.HasTile(cell)) || solid.Contains(p) || reserved.Contains(p)) continue;
                bool up = !floor.Contains(p + Vector2Int.up), down = !floor.Contains(p + Vector2Int.down);
                bool left = !floor.Contains(p + Vector2Int.left), right = !floor.Contains(p + Vector2Int.right);
                if ((up == down) || (left == right)) continue;           // cal exactament una paret vertical i una d'horitzontal
                if (PosHash(p, 9) % 100 >= 45) continue;                   // no tots els racons
                var go = new GameObject("Teranyina"); go.transform.position = ground.GetCellCenterWorld(cell); go.transform.SetParent(transform);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Cobweb; sr.sortingLayerName = "Shadows"; sr.sortingOrder = 3;
                // v1.4.2: la teranyina de racó frena (x0,35) a qui la xafa i es pot trencar a colps o amb trets (2 impactes)
                var hitArea = go.AddComponent<CircleCollider2D>(); hitArea.isTrigger = true; hitArea.radius = .5f;
                var webBreakable = go.AddComponent<Breakable>(); webBreakable.health = 2; webBreakable.coinChance = 0f; webBreakable.healChance = 0f; webBreakable.debrisColor = new Color(.92f, .92f, .98f);
                go.AddComponent<CobwebSlow>();
                sr.flipX = right; sr.flipY = down;
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
        var box = go.AddComponent<BoxCollider2D>();
        bool trunk = name.StartsWith("Tronc");
        box.size = trunk ? Vector2.one : new Vector2(.65f, .55f);
        box.offset = trunk ? Vector2.zero : new Vector2(0, -.1f);
        if (trunk)
        {
            TileNav.AddBlocker(pos);
            return;
        }
        var b = go.AddComponent<Breakable>();
        bool hard = name.StartsWith("Roca") || name == "Pedres" || name.StartsWith("Soca") || name == "Armari_metal" || name == "Llum_peu" || name == "Carro_compra" || name == "Taula_fusta";
        bool mushroom = name.StartsWith("Bolet");
        b.health = hard ? 4 : mushroom || name.StartsWith("Cactus") ? 2 : 1;
        b.coinChance = hard ? .08f : .15f; b.healChance = mushroom ? .3f : hard ? .04f : .12f;
        b.debrisColor = name.StartsWith("Roca") || name == "Pedres" || name == "Armari_metal" || name == "Paperera" ? new Color(.55f, .55f, .6f)
            : name == "Soca" || name == "Carro_compra" || name == "Taula_fusta" ? new Color(.45f, .3f, .15f)
            : mushroom ? new Color(.8f, .4f, .5f) : new Color(.3f, .65f, .3f);
    }
}

// Teranyina de racó: ralentitza el jugador que hi passa per damunt (cada equip, el seu). Es trenca amb Breakable.
public class CobwebSlow : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D other)
    {
        var movement = other.GetComponentInParent<PlayerMovement>();
        if (movement != null) movement.BoostSpeed(0.35f, 0.15f);
    }
}
