using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Attack = EnemyBoss.Attack;
using Arena = EnemyBoss.ArenaKind;

public static partial class AlphaContentBuilder
{
    const string BossArt = "Assets/Resources/Pixel/Bosses/";

    [MenuItem("Canica/Bosses v1.5")]
    public static void BuildBosses()
    {
        foreach (string id in new[] { "CorCristall", "CavallerCorrupte", "CucSorra", "Fenix", "HidraLlac",
                                      "ReiClavegueres", "CuinerBoig", "ProfessorInfernal" })
            ImportBossSprite("ArtNou_v1.4/Bosses/Sprites/" + id + ".png", id);
        ImportBossSprite("ArtNou_v1.4/Blocs/Laboratori/Valvula_centre.png", "Valvula_centre");
        ImportBossSprite("ArtNou_v1.4/Blocs/Cuina/Paella_centre.png", "Paella_centre");
        ImportBossSprite("ArtNou_v1.4/Blocs/Cuina/Fogons_apagats_centre.png", "Fogons_apagats_centre");
        ImportBossSprite("ArtNou_v1.4/Blocs/Cuina/Fogons_encesos_centre.png", "Fogons_encesos_centre");
        ImportBossSprite("ArtNou_v1.4/Blocs/Escola/Pupitre_verd_centre.png", "Pupitre_verd_centre");
        ImportBossSprite("ArtNou_v1.4/Blocs/Escola/Pissarra.png", "Pissarra_escola");

        var orb = BossPart("OrbitaCristall", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Pixel/Blocs/Mineral_ametista.png"),
            6, new Color(.85f, .78f, 1f));
        var valve = BossPart("ValvulaClavegueres", BossSprite("Valvula_centre"), 5, Color.white);
        var head = BossPart("CapHidra", BossSprite("HidraLlac"), 20, new Color(.62f, 1f, .8f), .62f);

        var crystal = Boss150("CorCristall", "Cor de Cristall", 120, 0f, Arena.Crystal, new Color(.86f, .62f, 1f),
            new[] { Attack.Beam, Attack.Fan, Attack.Shockwave }, orb);
        var knight = Boss150("CavallerCorrupte", "Cavaller Corrupte", 100, 2f, Arena.Knight, new Color(.8f, .42f, .45f),
            new[] { Attack.Sweep, Attack.Charge, Attack.Shockwave });
        var worm = Boss150("CucSorra", "Cuc de Sorra", 110, 1.4f, Arena.Worm, new Color(.93f, .72f, .41f),
            new[] { Attack.Burrow, Attack.Sweep, Attack.Shockwave });
        var phoenix = Boss150("Fenix", "F\u00e8nix", 105, 2.3f, Arena.Phoenix, new Color(1f, .48f, .12f),
            new[] { Attack.Hazard, Attack.Fan, Attack.Bomb, Attack.Shockwave });
        var hydra = Boss150("HidraLlac", "Hidra del Llac", 100, 0f, Arena.Hydra, new Color(.45f, .8f, .72f),
            new[] { Attack.Sweep, Attack.Fan, Attack.Shockwave }, head);
        var sewer = Boss150("ReiClavegueres", "Rei de les Clavegueres", 95, 1.55f, Arena.Sewer, new Color(.67f, .55f, .55f),
            new[] { Attack.Shockwave, Attack.Fan, Attack.Sweep }, valve);
        var cook = Boss150("CuinerBoig", "Cuiner Boig", 80, 1.5f, Arena.Cook, new Color(1f, .63f, .28f),
            new[] { Attack.Sweep, Attack.Hazard, Attack.Bomb }, prop: BossSprite("Fogons_apagats_centre"));
        var professor = Boss150("ProfessorInfernal", "Professor Infernal", 85, 1.65f, Arena.Professor, new Color(.94f, .67f, .35f),
            new[] { Attack.Fan, Attack.Hazard, Attack.Spiral }, prop: BossSprite("Pupitre_verd_centre"));

        var variants = new Dictionary<string, GameObject[]>
        {
            { "Cova", new[] { hydra } }, { "Bosc", new[] { hydra } }, { "Desert", new[] { worm } },
            { "Claveguera", new[] { sewer } }, { "Neu", new[] { professor } }, { "Cripta", new[] { knight } },
            { "Volca", new[] { phoenix, cook } }, { "Cristall", new[] { crystal } }
        };
        foreach (var path in Directory.GetFiles("Assets/Dungeon/Temes", "*.asset"))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            int separator = name.LastIndexOf('_');
            if (!variants.TryGetValue(name.Substring(separator + 1), out var pool)) continue;
            var theme = AssetDatabase.LoadAssetAtPath<DungeonTheme>(path);
            if (theme == null) continue;
            theme.bossVariants = pool;
            EditorUtility.SetDirty(theme);
        }
        AssetDatabase.SaveAssets();
    }

    static Sprite BossSprite(string id) => AssetDatabase.LoadAssetAtPath<Sprite>(BossArt + id + ".png");

    static void ImportBossSprite(string source, string id)
    {
        Directory.CreateDirectory(BossArt);
        string target = BossArt + id + ".png";
        if (!File.Exists(target) || !System.Linq.Enumerable.SequenceEqual(File.ReadAllBytes(source), File.ReadAllBytes(target)))
            File.Copy(source, target, true);
        AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
        if (BossSprite(id) == null) throw new InvalidOperationException("Falta l'sprite de " + id);
    }

    static GameObject BossPart(string id, Sprite sprite, int hp, Color tint, float scale = 1f)
    {
        var go = new GameObject(id);
        try
        {
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingLayerName = "Player";
            renderer.sortingOrder = 10;
            go.transform.localScale = Vector3.one * scale;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = .45f;
            collider.isTrigger = true;
            var health = go.AddComponent<EnemyHealth>();
            health.maxHealth = health.currentHealth = hp;
            health.arenaPart = true;
            health.knockbackResist = 1f;
            health.healDropChance = 0f;
            return PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Enemics/" + id + ".prefab");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    static GameObject Boss150(string id, string name, int hp, float speed, Arena arena, Color tint, Attack[] attacks,
        GameObject part = null, Sprite prop = null)
    {
        var go = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemics/MagFosc.prefab");
        go.name = id;
        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = BossSprite(id);
        renderer.color = Color.white;
        var health = go.GetComponent<EnemyHealth>();
        health.maxHealth = health.currentHealth = hp;
        go.GetComponent<EnemyPathfindingAI>().moveSpeed = speed;
        var boss = go.GetComponent<EnemyBoss>();
        boss.bossName = name;
        boss.arenaKind = arena;
        boss.arenaPartPrefab = part;
        boss.arenaProp = prop;
        boss.attacks = attacks;
        boss.calmAttacks = 2;
        boss.useBulletColor = true;
        boss.bulletColor = tint;
        boss.dustColor = tint;
        SaveEnemy(go, id);
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemics/" + id + ".prefab");
    }
}

