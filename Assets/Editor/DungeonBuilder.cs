using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Menú Canica > Masmorra > Crear-ho tot
// Crea (o actualitza) tot el que necessita el mode masmorra a partir dels PNG de draw.ps1:
//   - Tiles dels 4 temes, porta i lava          -> Assets/Tiles/Masmorra
//   - Prefabs: caixa, barril, punxes            -> Assets/Prefabs/Masmorra (+ tiles per a pintar-los)
//   - Prefabs dels enemics i del boss           -> Assets/Prefabs/Enemics
//   - Items de cura                             -> Assets/Resources/Items
//   - Armes noves (granades, ballesta, flames)  -> Assets/Weapons
//   - Temes de la masmorra                      -> Assets/Dungeon/Temes
//   - Personalització (barrets, colors...)      -> Assets/Resources/Cosmetics.asset
//   - Escena Assets/Scenes/Dungeon.unity (còpia de GameScene sense mapa, amb el generador)
// Es pot tornar a executar sense por: actualitza el que ja existeix.
public static class DungeonBuilder
{
    const string Px = "Assets/Sprites/Pixel/";
    const string TileDir = "Assets/Tiles/Masmorra";
    const string PropDir = "Assets/Prefabs/Masmorra";
    const string EnemyDir = "Assets/Prefabs/Enemics";
    const string ItemDir = "Assets/Resources/Items";
    const string ThemeDir = "Assets/Dungeon/Temes";
    const string DungeonScene = "Assets/Scenes/Dungeon.unity";
    const string BaseScene = "Assets/Scenes/GameScene.unity";

    [MenuItem("Canica/Masmorra/Crear-ho tot (tiles, enemics, items, escena)")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        foreach (string d in new[] { TileDir, PropDir, EnemyDir, ItemDir, ThemeDir })
            Directory.CreateDirectory(d);
        AssetDatabase.Refresh();

        var tiles = BuildTiles();
        var props = BuildProps();
        var enemies = BuildEnemies();
        BuildHealItems();
        BuildWeapons();
        ChestPrefabCreator.CreateAll();   // Torna a omplir els cofres amb les armes noves
        BuildCosmetics();
        var themes = BuildThemes(tiles);
        AssetDatabase.SaveAssets();

        bool sceneOk = BuildScene(tiles, props, enemies, themes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Masmorra",
            "Fet!\n\n- Tiles, objectes i enemics creats\n- Items de cura i 3 armes noves\n- Personalització al menú\n" +
            (sceneOk ? "- Escena Dungeon creada i afegida al Build Settings\n\nObri MainMenu, Jugar -> Masmorra." : "- No s'ha pogut crear l'escena (mira la Console)"),
            "OK");
    }

    // ------------------------------------------------------------------ Tiles

    static Sprite S(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(Px + path + ".png");

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null)
        {
            a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, path);
        }
        return a;
    }

    static Tile MakeTile(string name, Sprite sprite, Tile.ColliderType collider)
    {
        Tile t = LoadOrCreate<Tile>(TileDir + "/" + name + ".asset");
        t.sprite = sprite;
        t.colliderType = collider;
        EditorUtility.SetDirty(t);
        return t;
    }

    static Dictionary<string, TileBase> BuildTiles()
    {
        var d = new Dictionary<string, TileBase>();
        foreach (string theme in new[] { "Cova", "Desert", "Neu", "Volca" })
        {
            d[theme + "_Terra1"] = MakeTile(theme + "_Terra1", S("Masmorra/" + theme + "_Terra1"), Tile.ColliderType.None);
            d[theme + "_Terra2"] = MakeTile(theme + "_Terra2", S("Masmorra/" + theme + "_Terra2"), Tile.ColliderType.None);
            d[theme + "_Paret"] = MakeTile(theme + "_Paret", S("Masmorra/" + theme + "_Paret"), Tile.ColliderType.Grid);
        }
        d["Porta"] = MakeTile("Porta", S("Masmorra/Porta"), Tile.ColliderType.Grid);

        DamageTile lava = LoadOrCreate<DamageTile>(TileDir + "/Lava.asset");
        lava.sprite = S("Masmorra/Lava");
        lava.colliderType = Tile.ColliderType.None;
        lava.damage = 1;
        EditorUtility.SetDirty(lava);
        d["Lava"] = lava;
        return d;
    }

    // ------------------------------------------------------------------ Objectes

    static Dictionary<string, GameObject> BuildProps()
    {
        var d = new Dictionary<string, GameObject>();
        Sprite shadow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pixel/Cofres/Ombra.png");

        // Caixa
        GameObject crate = PropRoot("Caixa", S("Masmorra/Caixa"), shadow, true);
        Breakable cb = crate.AddComponent<Breakable>();
        cb.health = 2;
        d["Caixa"] = SavePrefab(crate, PropDir + "/Caixa.prefab");

        // Barril explosiu
        GameObject barrel = PropRoot("Barril", S("Masmorra/Barril"), shadow, true);
        Breakable bb = barrel.AddComponent<Breakable>();
        bb.health = 1;
        bb.explodes = true;
        bb.debrisColor = new Color(0.9f, 0.25f, 0.3f);
        d["Barril"] = SavePrefab(barrel, PropDir + "/Barril.prefab");

        // Punxes (sense col·lisió, al terra)
        GameObject spikes = PropRoot("Punxes", S("Masmorra/Punxes_Baix"), null, false);
        SpriteRenderer ssr = spikes.GetComponentInChildren<SpriteRenderer>();
        ssr.sortingLayerName = "Shadows";
        ssr.sortingOrder = 10;
        SpikeTrap trap = spikes.AddComponent<SpikeTrap>();
        trap.downSprite = S("Masmorra/Punxes_Baix");
        trap.upSprite = S("Masmorra/Punxes_Dalt");
        d["Punxes"] = SavePrefab(spikes, PropDir + "/Punxes.prefab");

        // Tiles per a pintar-los a mà amb la Tile Palette
        foreach (var kv in d)
        {
            PrefabTile t = LoadOrCreate<PrefabTile>(TileDir + "/Objecte_" + kv.Key + ".asset");
            t.prefab = kv.Value;
            t.previewSprite = kv.Key == "Punxes" ? S("Masmorra/Punxes_Dalt") : S("Masmorra/" + kv.Key);
            EditorUtility.SetDirty(t);
        }
        return d;
    }

    static GameObject PropRoot(string name, Sprite sprite, Sprite shadow, bool solid)
    {
        GameObject root = new GameObject(name);
        if (solid)
        {
            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.7f);
            col.offset = new Vector2(0f, -0.05f);
        }
        if (shadow != null)
        {
            GameObject sh = new GameObject("Ombra");
            sh.transform.SetParent(root.transform, false);
            sh.transform.localPosition = new Vector3(0f, -0.38f, 0f);
            SpriteRenderer s = sh.AddComponent<SpriteRenderer>();
            s.sprite = shadow;
            s.color = new Color(0f, 0f, 0f, 130f / 255f);
            s.sortingLayerName = "Shadows";
        }
        GameObject v = new GameObject("Sprite");
        v.transform.SetParent(root.transform, false);
        SpriteRenderer sr = v.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 1;
        return root;
    }

    static GameObject SavePrefab(GameObject go, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ------------------------------------------------------------------ Enemics

    static Dictionary<string, GameObject> BuildEnemies()
    {
        var d = new Dictionary<string, GameObject>();

        // So de dany i "poof" dels enemics vells
        AudioClip[] hurt = new[] { "0b910f8fc1e77254aa5e7809e54eb107", "b7fed903a7873134a8eb3b068862d783", "255fa4568be0f774dbb333f7fab679fd", "42437720e9f95e84ab0c3b8efc96226d" }
            .Select(g => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g))).Where(c => c != null).ToArray();
        GameObject poof = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("3a857fc53770baa4d93d6c2a02ecfbd2"));
        Sprite oldEnemy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pixil-malo-enemic.png");

        GameObject Make(string name, Sprite sprite, float scale, int hp, float speed, float radius, float colliderSize, Color death, bool contactDamage)
        {
            GameObject root = new GameObject(name);
            root.transform.localScale = Vector3.one * scale;

            SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Enemy";
            sr.sortingOrder = 2;

            Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one * colliderSize;

            AudioSource audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            EnemyHealth h = root.AddComponent<EnemyHealth>();
            h.maxHealth = hp;
            h.damageAudioSource = audio;
            h.damageSounds = hurt;
            h.poofEffectPrefab = poof;
            h.deathColor = death;

            if (contactDamage) root.AddComponent<EnemyDamage>().damage = 1;

            EnemyPathfindingAI ai = root.AddComponent<EnemyPathfindingAI>();
            ai.moveSpeed = speed;
            ai.bodyRadius = radius;

            // Ombra
            Sprite shadow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pixel/Cofres/Ombra.png");
            if (shadow != null)
            {
                GameObject sh = new GameObject("Ombra");
                sh.transform.SetParent(root.transform, false);
                sh.transform.localPosition = new Vector3(0f, -0.42f, 0f);
                SpriteRenderer s = sh.AddComponent<SpriteRenderer>();
                s.sprite = shadow;
                s.color = new Color(0f, 0f, 0f, 110f / 255f);
                s.sortingLayerName = "Shadows";
            }
            return root;
        }

        // Cos a cos (el dibuix de l'enemic de sempre)
        GameObject malo = Make("Malo", oldEnemy, 0.75f, 4, 2.6f, 0.28f, 0.8f, new Color(0.9f, 0.3f, 0.35f), true);
        d["Malo"] = SavePrefab(malo, EnemyDir + "/Malo.prefab");

        GameObject llim = Make("Llim", S("Enemics/Llim"), 1f, 5, 2.2f, 0.3f, 0.7f, new Color(0.6f, 0.9f, 0.35f), true);
        llim.AddComponent<EnemySplitter>();
        d["Llim"] = SavePrefab(llim, EnemyDir + "/Llim.prefab");

        GameObject tirador = Make("Tirador", S("Enemics/Tirador"), 1f, 3, 2.2f, 0.28f, 0.7f, new Color(0.64f, 0.35f, 0.88f), false);
        tirador.AddComponent<EnemyShooter>();
        d["Tirador"] = SavePrefab(tirador, EnemyDir + "/Tirador.prefab");

        GameObject kami = Make("Kamikaze", S("Enemics/Kamikaze"), 1f, 2, 2.6f, 0.28f, 0.7f, new Color(0.4f, 0.4f, 0.45f), false);
        kami.GetComponent<EnemyHealth>().coinsMax = 1;
        kami.AddComponent<EnemyKamikaze>();
        d["Kamikaze"] = SavePrefab(kami, EnemyDir + "/Kamikaze.prefab");

        GameObject boss = Make("ReiLlim", S("Enemics/ReiLlim"), 1f, 70, 1.6f, 0.75f, 1.5f, new Color(0.6f, 0.9f, 0.35f), true);
        EnemyHealth bh = boss.GetComponent<EnemyHealth>();
        bh.knockbackResist = 1f;
        bh.coinsMin = 0; bh.coinsMax = 0;
        boss.transform.Find("Ombra").localScale = new Vector3(2f, 1.5f, 1f);
        boss.transform.Find("Ombra").localPosition = new Vector3(0f, -0.8f, 0f);
        boss.AddComponent<EnemyBoss>().minionPrefab = d["Llim"];
        d["ReiLlim"] = SavePrefab(boss, EnemyDir + "/ReiLlim.prefab");

        return d;
    }

    // ------------------------------------------------------------------ Items i armes

    static void BuildHealItems()
    {
        void Item(string file, string name, int heal, ChestRarity rarity)
        {
            HealItem i = LoadOrCreate<HealItem>(ItemDir + "/" + file + ".asset");
            i.itemName = name;
            i.icon = S("Items/" + file);
            i.healAmount = heal;
            i.rarity = rarity;
            EditorUtility.SetDirty(i);
        }
        Item("Poma", "Poma", 1, ChestRarity.Comu);
        Item("Bena", "Bena", 2, ChestRarity.Rar);
        Item("PocioVida", "Poció de vida", 3, ChestRarity.Epic);
        Item("Farmaciola", "Farmaciola", 5, ChestRarity.Llegendari);
    }

    static void BuildWeapons()
    {
        WeaponData W(string file) => LoadOrCreate<WeaponData>("Assets/Weapons/" + file + ".asset");

        WeaponData g = W("Llancagranades");
        if (string.IsNullOrEmpty(g.weaponName) || g.weaponName == "Arma")
        {
            g.weaponName = "Llançagranades"; g.rarity = ChestRarity.Epic; g.handScale = 0.75f;
            g.damage = 1; g.cooldown = 1.0f; g.knockbackForce = 3f;
            g.projectileSpeed = 9f; g.range = 6f; g.projectileSize = 0.25f;
            g.explosionRadius = 1.6f; g.explosionDamage = 4;
            g.sound = WeaponSound.Llancament;
        }
        g.icon = S("Armes/Llancagranades"); g.projectileSprite = S("Armes/Granada");
        EditorUtility.SetDirty(g);

        WeaponData b = W("Ballesta");
        if (string.IsNullOrEmpty(b.weaponName) || b.weaponName == "Arma")
        {
            b.weaponName = "Ballesta"; b.rarity = ChestRarity.Rar; b.handScale = 0.75f;
            b.damage = 3; b.cooldown = 0.6f; b.knockbackForce = 2.5f;
            b.projectileSpeed = 20f; b.range = 9f; b.projectileSize = 0.15f;
            b.pierce = 2;
            b.sound = WeaponSound.Llancament;
        }
        b.icon = S("Armes/Ballesta"); b.projectileSprite = S("Armes/Fletxa");
        EditorUtility.SetDirty(b);

        WeaponData f = W("Llancaflames");
        if (string.IsNullOrEmpty(f.weaponName) || f.weaponName == "Arma")
        {
            f.weaponName = "Llançaflames"; f.rarity = ChestRarity.Llegendari; f.handScale = 0.75f;
            f.damage = 1; f.cooldown = 0.06f; f.knockbackForce = 0.5f;
            f.projectileSpeed = 9f; f.range = 3.2f; f.projectileSize = 0.28f;
            f.projectileColor = new Color(1f, 0.55f, 0.15f, 0.85f);
            f.spreadAngle = 14f; f.pellets = 2; f.rangeRandom = 0.35f;
            f.sound = WeaponSound.Llancament;
        }
        f.icon = S("Armes/Llancaflames"); f.projectileSprite = null;
        EditorUtility.SetDirty(f);
    }

    static void BuildCosmetics()
    {
        CosmeticsDatabase db = LoadOrCreate<CosmeticsDatabase>("Assets/Resources/Cosmetics.asset");
        db.playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pixil-frame-0.png");
        db.hats = new[]
        {
            new CosmeticsDatabase.Hat { name = "Cap", sprite = null },
            new CosmeticsDatabase.Hat { name = "Corona", sprite = S("Barrets/Corona") },
            new CosmeticsDatabase.Hat { name = "Gorra", sprite = S("Barrets/Gorra") },
            new CosmeticsDatabase.Hat { name = "Banyes", sprite = S("Barrets/Banyes") },
            new CosmeticsDatabase.Hat { name = "Aureola", sprite = S("Barrets/Aureola") },
            new CosmeticsDatabase.Hat { name = "Bolet", sprite = S("Barrets/Bolet") },
        };
        EditorUtility.SetDirty(db);
    }

    static DungeonTheme[] BuildThemes(Dictionary<string, TileBase> tiles)
    {
        TileBase water = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/WaterTile.asset");

        DungeonTheme T(string file, string name, string key, TileBase liquid, float liquidChance, Color light, float intensity, Color bg)
        {
            DungeonTheme t = LoadOrCreate<DungeonTheme>(ThemeDir + "/" + file + ".asset");
            t.themeName = name;
            t.floorTiles = new[] { tiles[key + "_Terra1"], tiles[key + "_Terra2"] };
            t.wallTile = tiles[key + "_Paret"];
            t.liquidTile = liquid;
            t.liquidChance = liquidChance;
            t.lightColor = light;
            t.lightIntensity = intensity;
            t.backgroundColor = bg;
            EditorUtility.SetDirty(t);
            return t;
        }

        return new[]
        {
            T("1_Cova", "Cova", "Cova", water, 0.3f, new Color(0.8f, 0.85f, 1f), 0.8f, new Color(0.05f, 0.05f, 0.08f)),
            T("2_Desert", "Desert", "Desert", water, 0.1f, new Color(1f, 0.95f, 0.85f), 1.05f, new Color(0.2f, 0.14f, 0.08f)),
            T("3_Neu", "Neu i gel", "Neu", water, 0.25f, new Color(0.9f, 0.95f, 1f), 1f, new Color(0.12f, 0.16f, 0.22f)),
            T("4_Volca", "Volcà", "Volca", tiles["Lava"], 0.6f, new Color(1f, 0.75f, 0.6f), 0.85f, new Color(0.12f, 0.03f, 0.02f)),
        };
    }

    // ------------------------------------------------------------------ Escena

    static bool BuildScene(Dictionary<string, TileBase> tiles, Dictionary<string, GameObject> props,
                           Dictionary<string, GameObject> enemies, DungeonTheme[] themes)
    {
        string previous = EditorSceneManager.GetActiveScene().path;

        if (!File.Exists(DungeonScene) && !AssetDatabase.CopyAsset(BaseScene, DungeonScene))
        {
            Debug.LogError("[Masmorra] No s'ha pogut copiar " + BaseScene);
            return false;
        }

        var scene = EditorSceneManager.OpenScene(DungeonScene, OpenSceneMode.Single);

        // Fora el que era del mapa de proves: monedes, enemic, tiles...
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name.StartsWith("CoinPivot") || root.name == "Enemy")
                Object.DestroyImmediate(root);
        foreach (EnemyHealth e in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
            Object.DestroyImmediate(e.gameObject);
        foreach (Tilemap t in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            t.ClearAllTiles();

        Tilemap Find(string n) { GameObject g = GameObject.Find(n); return g != null ? g.GetComponent<Tilemap>() : null; }

        GameObject genGO = GameObject.Find("Masmorra");
        if (genGO == null) genGO = new GameObject("Masmorra");
        DungeonGenerator gen = genGO.GetComponent<DungeonGenerator>();
        if (gen == null) gen = genGO.AddComponent<DungeonGenerator>();

        gen.ground = Find("Tilemap Ground");
        gen.walls = Find("Tilemap Walls");
        gen.liquid = Find("Tilemap Liquid");
        gen.themes = themes;
        gen.doorTile = tiles["Porta"];
        GameObject player = GameObject.FindWithTag("Player");
        gen.player = player != null ? player.transform : null;
        GameObject sp = GameObject.Find("SpawnPoint");
        gen.spawnPoint = sp != null ? sp.transform : null;
        gen.enemyPrefabs = new[] { enemies["Malo"], enemies["Llim"], enemies["Tirador"], enemies["Kamikaze"] };
        gen.bossPrefab = enemies["ReiLlim"];
        gen.enemyBulletSprite = S("Enemics/BalaEnemiga");
        gen.chestPrefabs = new[] { "Comu", "Rar", "Epic", "Llegendari" }
            .Select(r => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cofres/Cofre_" + r + ".prefab")).ToArray();
        gen.cratePrefab = props["Caixa"];
        gen.barrelPrefab = props["Barril"];
        gen.spikePrefab = props["Punxes"];

        if (gen.ground == null || gen.walls == null)
        {
            Debug.LogError("[Masmorra] No trobe 'Tilemap Ground' o 'Tilemap Walls' a l'escena.");
            return false;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Build Settings
        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DungeonScene))
        {
            list.Add(new EditorBuildSettingsScene(DungeonScene, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        if (!string.IsNullOrEmpty(previous) && previous != DungeonScene)
            EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
        return true;
    }
}
