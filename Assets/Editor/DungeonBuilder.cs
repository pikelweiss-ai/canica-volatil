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
        var themes = BuildThemes(tiles, enemies);
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
        foreach (string theme in new[] { "Cova", "Desert", "Neu", "Volca", "Bosc", "Cripta", "Cristall", "Claveguera" })
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

        DamageTile acid = LoadOrCreate<DamageTile>(TileDir + "/Acid.asset");
        acid.sprite = S("Masmorra/Acid");
        acid.colliderType = Tile.ColliderType.None;
        acid.damage = 1;
        EditorUtility.SetDirty(acid);
        d["Acid"] = acid;
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

        // ---- Enemics nous ----
        EnemyCharger Charger(GameObject g, float range, float tele, float speed, float dist, float cd, float stun, int dmg)
        {
            EnemyCharger c = g.AddComponent<EnemyCharger>();
            c.chargeRange = range; c.telegraphTime = tele; c.dashSpeed = speed; c.dashDistance = dist;
            c.cooldown = cd; c.stunTime = stun; c.dashDamage = dmg;
            return c;
        }

        GameObject bat = Make("Ratpenat", S("Enemics/Ratpenat"), 1f, 2, 3.4f, 0.25f, 0.6f, new Color(0.45f, 0.2f, 0.6f), true);
        bat.GetComponent<EnemyHealth>().coinsMax = 1;
        Charger(bat, 4f, 0.25f, 9f, 3f, 1.3f, 0.3f, 1);
        d["Ratpenat"] = SavePrefab(bat, EnemyDir + "/Ratpenat.prefab");

        GameObject spider = Make("Aranya", S("Enemics/Aranya"), 1f, 3, 3f, 0.27f, 0.65f, new Color(0.3f, 0.3f, 0.35f), true);
        Charger(spider, 5f, 0.35f, 12f, 4.5f, 2f, 0.5f, 1);
        d["Aranya"] = SavePrefab(spider, EnemyDir + "/Aranya.prefab");

        GameObject golem = Make("Golem", S("Enemics/Golem"), 1f, 12, 1.3f, 0.32f, 0.8f, new Color(0.45f, 0.45f, 0.5f), true);
        golem.GetComponent<EnemyHealth>().knockbackResist = 0.6f;
        golem.GetComponent<EnemyHealth>().coinsMax = 3;
        Charger(golem, 6f, 0.7f, 9f, 6f, 3f, 1.2f, 2);
        d["Golem"] = SavePrefab(golem, EnemyDir + "/Golem.prefab");

        GameObject skel = Make("Esquelet", S("Enemics/Esquelet"), 1f, 4, 2f, 0.27f, 0.65f, new Color(0.9f, 0.9f, 0.85f), false);
        EnemyShooter ss = skel.AddComponent<EnemyShooter>();
        ss.pattern = EnemyShooter.Pattern.Burst3; ss.fireCooldown = 2.2f; ss.bulletSpeed = 7f;
        d["Esquelet"] = SavePrefab(skel, EnemyDir + "/Esquelet.prefab");

        GameObject mage = Make("Mag", S("Enemics/Mag"), 1f, 4, 2f, 0.27f, 0.65f, new Color(0.3f, 0.5f, 1f), false);
        mage.AddComponent<EnemyTeleporter>();
        d["Mag"] = SavePrefab(mage, EnemyDir + "/Mag.prefab");

        GameObject shroom = Make("BoletVeri", S("Enemics/BoletVeri"), 1f, 5, 0f, 0.3f, 0.7f, new Color(0.64f, 0.35f, 0.88f), true);
        shroom.GetComponent<EnemyPathfindingAI>().wanderWhenIdle = false;
        EnemyShooter bs = shroom.AddComponent<EnemyShooter>();
        bs.pattern = EnemyShooter.Pattern.Ring8; bs.fireCooldown = 2.4f; bs.bulletSpeed = 4f;
        bs.useColor = true; bs.bulletColor = new Color(0.6f, 1f, 0.4f);
        d["BoletVeri"] = SavePrefab(shroom, EnemyDir + "/BoletVeri.prefab");

        // ---- Bosses nous ----
        GameObject Boss(string name, string sprite, int hp, float speed, EnemyBoss.Attack[] attacks, GameObject minion, Color bullets, Color dust, string title)
        {
            GameObject b = Make(name, S("Enemics/" + sprite), 1f, hp, speed, 0.75f, 1.5f, dust, true);
            EnemyHealth h = b.GetComponent<EnemyHealth>();
            h.knockbackResist = 1f; h.coinsMin = 0; h.coinsMax = 0;
            b.transform.Find("Ombra").localScale = new Vector3(2f, 1.5f, 1f);
            b.transform.Find("Ombra").localPosition = new Vector3(0f, -0.8f, 0f);
            EnemyBoss eb = b.AddComponent<EnemyBoss>();
            eb.bossName = title; eb.attacks = attacks; eb.calmAttacks = 2; eb.minionPrefab = minion;
            eb.useBulletColor = true; eb.bulletColor = bullets; eb.dustColor = dust;
            return SavePrefab(b, EnemyDir + "/" + name + ".prefab");
        }

        d["GolemGegant"] = Boss("GolemGegant", "GolemGegant", 90, 1.3f,
            new[] { EnemyBoss.Attack.Charge, EnemyBoss.Attack.RockRain, EnemyBoss.Attack.Ring, EnemyBoss.Attack.Jump },
            d["Golem"], new Color(0.75f, 0.72f, 0.7f), new Color(0.55f, 0.55f, 0.6f), "Golem Gegant");
        d["MagFosc"] = Boss("MagFosc", "MagFosc", 65, 2f,
            new[] { EnemyBoss.Attack.Teleport, EnemyBoss.Attack.Fan, EnemyBoss.Attack.Summon, EnemyBoss.Attack.Spiral },
            d["Ratpenat"], new Color(0.8f, 0.5f, 1f), new Color(0.6f, 0.35f, 0.9f), "Mag Fosc");

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
            b.projectileSpeed = 20f; b.range = 8.5f; b.projectileSize = 0.15f;
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
            f.projectileSpeed = 9f; f.range = 2.8f; f.maxShots = 300; f.projectileSize = 0.28f;
            f.projectileColor = new Color(1f, 0.55f, 0.15f, 0.85f);
            f.spreadAngle = 14f; f.pellets = 2; f.rangeRandom = 0.35f;
            f.sound = WeaponSound.Llancament;
        }
        f.icon = S("Armes/Llancaflames"); f.projectileSprite = null;
        EditorUtility.SetDirty(f);

        void Melee(string file, string name, ChestRarity rarity, int damage, float cooldown, float reach, float arc, Color color)
        {
            WeaponData w = W(file);
            w.weaponName = name; w.rarity = rarity; w.melee = true;
            w.damage = damage; w.cooldown = cooldown; w.range = reach; w.meleeArc = arc;
            w.knockbackForce = 5f; w.icon = S("Armes/" + file); w.projectileColor = color;
            w.meleeDashDistance = file == "SabreCorall" ? 0.85f : 0f;
            w.alternateSwing = file == "EspasaVidre";
            w.meleeRadial = file == "MartellOrbital";
            w.sound = WeaponSound.Llancament; EditorUtility.SetDirty(w);
        }
        Melee("EspasaVidre", "Espasa de vidre", ChestRarity.Comu, 3, 0.55f, 2f, 120f, new Color(0.6f, 0.9f, 1f));
        Melee("SabreCorall", "Sabre de corall", ChestRarity.Rar, 4, 0.65f, 2f, 130f, new Color(1f, 0.45f, 0.6f));
        Melee("MartellOrbital", "Martell orbital", ChestRarity.Epic, 6, 1.1f, 2.2f, 160f, new Color(0.8f, 0.6f, 1f));

        // ---- Armes noves (els valors només es posen la primera vegada; després els canvies a l'Inspector) ----
        void New(string file, string name, ChestRarity rarity, string icon, string proj, float projScale,
                 int dmg, float cd, float speed, float range, float size, WeaponSound sound,
                 System.Action<WeaponData> extra = null)
        {
            WeaponData w = W(file);
            if (string.IsNullOrEmpty(w.weaponName) || w.weaponName == "Arma")
            {
                w.weaponName = name; w.rarity = rarity; w.handScale = 0.75f;
                w.damage = dmg; w.cooldown = cd; w.knockbackForce = 2f;
                w.projectileSpeed = speed; w.range = range; w.projectileSize = size;
                w.projectileSpriteScale = projScale; w.sound = sound;
                extra?.Invoke(w);
            }
            w.icon = S("Armes/" + icon);
            w.projectileSprite = proj != null ? S("Armes/" + proj) : null;
            EditorUtility.SetDirty(w);
        }

        New("Revolver", "Revòlver", ChestRarity.Comu, "Revolver", "Bala", 1f, 2, 0.45f, 18f, 7.5f, 0.15f, WeaponSound.Tret);
        New("Bumerang", "Bumerang", ChestRarity.Rar, "Bumerang", "Bumerang", 0.8f, 2, 0.7f, 11f, 5.5f, 0.35f, WeaponSound.Llancament,
            w => { w.returns = true; w.spinSpeed = 900f; });
        New("Shuriken", "Shurikens", ChestRarity.Comu, "Shuriken", "Shuriken", 0.7f, 1, 0.45f, 15f, 6f, 0.2f, WeaponSound.Llancament,
            w => { w.pellets = 3; w.spreadAngle = 12f; w.pierce = 1; w.spinSpeed = 1080f; });
        New("Vareta", "Vareta màgica", ChestRarity.Epic, "Vareta", "BolaMagica", 1f, 2, 0.35f, 9f, 9f, 0.25f, WeaponSound.Llancament,
            w => { w.homing = 6f; });
        New("Minigun", "Minigun", ChestRarity.Llegendari, "Minigun", "Bala", 1f, 1, 0.045f, 19f, 7.5f, 0.15f, WeaponSound.Tret,
            w => { w.spreadAngle = 12f; w.knockbackForce = 0.8f; w.heatLimit = 60; w.heatCoolingRate = 15f; w.overheatCooldown = 2f; });
        W("Minigun").spinUpTime = 0.6f;
        EditorUtility.SetDirty(W("Minigun"));
        New("Laser", "Làser", ChestRarity.Llegendari, "Laser", "Raig", 1f, 4, 0.5f, 35f, 12f, 0.18f, WeaponSound.Tret,
            w => { w.pierce = 5; });
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
            new CosmeticsDatabase.Hat { name = "Astronauta", sprite = S("Barrets/Astronauta") },
            new CosmeticsDatabase.Hat { name = "Samurai", sprite = S("Barrets/Samurai") },
            new CosmeticsDatabase.Hat { name = "Cristall", sprite = S("Barrets/Cristall") },
            new CosmeticsDatabase.Hat { name = "Pirata", sprite = S("Barrets/Pirata") },
            new CosmeticsDatabase.Hat { name = "Viking", sprite = S("Barrets/Viking") },
            new CosmeticsDatabase.Hat { name = "Flor", sprite = S("Barrets/Flor") },
            new CosmeticsDatabase.Hat { name = "Copalta", sprite = S("Barrets/Copalta") },
        };
        var skins = new List<CosmeticsDatabase.Hat> { new CosmeticsDatabase.Hat { name = "Clàssica", sprite = null } };
        foreach (var s in new[] { ("Vidre", "Canica de vidre"), ("Foc", "Bola de foc"), ("Gel", "Glaçó"), ("Or", "Canica d'or"),
                                  ("Robot", "Robot"), ("Zombi", "Zombi"), ("Ninja", "Ninja"), ("Sindria", "Síndria"), ("Fantasma", "Fantasma"), ("Cosmos", "Cosmos"), ("Corall", "Corall"), ("Menta", "Menta"), ("Obsidiana", "Obsidiana"), ("Bombolla", "Bombolla"), ("Capvespre", "Capvespre"), ("Aurora", "Aurora"), ("Coure", "Coure"), ("Caramel", "Caramel"), ("Llima", "Llima"), ("Safir", "Safir"), ("Perla", "Perla") })
            skins.Add(new CosmeticsDatabase.Hat { name = s.Item2, sprite = S("Skins/" + s.Item1) });
        db.skins = skins.ToArray();
        var trails = new List<CosmeticsDatabase.Tint>(db.trails ?? new CosmeticsDatabase.Tint[0]);
        foreach(var entry in new[] {
            ("Corall",new Color(1f,0.3f,0.5f)), ("Cosmos",new Color(0.55f,0.25f,1f)),
            ("Or",new Color(1f,0.8f,0.2f)), ("Gel",new Color(0.65f,0.9f,1f)),
            ("Menta",new Color(0.2f,1f,0.7f)), ("Obsidiana",new Color(0.3f,0.3f,0.45f)) })
            if(!trails.Any(t=>t.name==entry.Item1)) trails.Add(new CosmeticsDatabase.Tint {name=entry.Item1,color=entry.Item2});
        db.trails=trails.ToArray();
        EditorUtility.SetDirty(db);
    }

    static DungeonTheme[] BuildThemes(Dictionary<string, TileBase> tiles, Dictionary<string, GameObject> enemies)
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

        // Els temes vells (1_Cova, 2_Desert...) ja no es fan servir
        foreach (string old in new[] { "1_Cova", "2_Desert", "3_Neu", "4_Volca" })
            AssetDatabase.DeleteAsset(ThemeDir + "/" + old + ".asset");

        DungeonTheme With(DungeonTheme t, string boss, params string[] list)
        {
            t.enemies = list.Select(n => enemies[n]).ToArray();
            t.boss = enemies[boss];
            EditorUtility.SetDirty(t);
            return t;
        }

        // Ordre dels pisos: després del 8 torna a començar (més difícil)
        return new[]
        {
            With(T("Tema_1_Cova", "Cova", "Cova", water, 0.3f, new Color(0.8f, 0.85f, 1f), 0.8f, new Color(0.05f, 0.05f, 0.08f)),
                 "ReiLlim", "Malo", "Llim", "Ratpenat", "Kamikaze"),
            With(T("Tema_2_Bosc", "Bosc perdut", "Bosc", water, 0.3f, new Color(0.85f, 1f, 0.85f), 0.9f, new Color(0.03f, 0.08f, 0.03f)),
                 "GolemGegant", "Llim", "Aranya", "BoletVeri", "Tirador"),
            With(T("Tema_3_Desert", "Desert", "Desert", water, 0.1f, new Color(1f, 0.95f, 0.85f), 1.05f, new Color(0.2f, 0.14f, 0.08f)),
                 "MagFosc", "Malo", "Golem", "Esquelet", "Kamikaze"),
            With(T("Tema_4_Claveguera", "Claveguera", "Claveguera", tiles["Acid"], 0.7f, new Color(0.8f, 1f, 0.75f), 0.75f, new Color(0.04f, 0.06f, 0.03f)),
                 "ReiLlim", "Llim", "BoletVeri", "Ratpenat", "Aranya"),
            With(T("Tema_5_Neu", "Neu i gel", "Neu", water, 0.25f, new Color(0.9f, 0.95f, 1f), 1f, new Color(0.12f, 0.16f, 0.22f)),
                 "GolemGegant", "Golem", "Mag", "Tirador", "Ratpenat"),
            With(T("Tema_6_Cripta", "Cripta", "Cripta", water, 0.1f, new Color(0.8f, 0.75f, 1f), 0.7f, new Color(0.06f, 0.03f, 0.08f)),
                 "MagFosc", "Esquelet", "Mag", "Ratpenat", "Tirador"),
            With(T("Tema_7_Volca", "Volcà", "Volca", tiles["Lava"], 0.6f, new Color(1f, 0.75f, 0.6f), 0.85f, new Color(0.12f, 0.03f, 0.02f)),
                 "GolemGegant", "Golem", "Kamikaze", "Mag", "Esquelet"),
            With(T("Tema_8_Cristall", "Cova de cristall", "Cristall", water, 0.2f, new Color(0.75f, 0.85f, 1f), 0.85f, new Color(0.02f, 0.03f, 0.1f)),
                 "MagFosc", "Mag", "Aranya", "Golem", "Tirador"),
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
