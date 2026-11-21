using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// v1.4.1: crea els 6 enemics i els 4 microbosses que faltaven (el comportament és a EnemyV140) i els reparteix pels temes.
// Es pot tornar a executar sense risc: reutilitza els prefabs que ja existeixen.
public static class V141ContentBuilder
{
    static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pixel/" + name + ".png");

    [MenuItem("Canica/Contingut v1.4.1")]
    public static void Build()
    {
        GameObject Make(string id, EnemyV140.Kind kind, int hp, float speed, int contact, float scale = 1f, float knockbackResist = -1f)
        {
            const string folder = "Assets/Prefabs/Enemics/";
            var root = PrefabUtility.LoadPrefabContents(folder + "Malo.prefab");
            root.name = id;
            var sprite = root.GetComponent<SpriteRenderer>(); sprite.sprite = S("Enemics/" + id); sprite.color = Color.white;
            var health = root.GetComponent<EnemyHealth>(); health.maxHealth = hp; health.currentHealth = hp;
            if (knockbackResist >= 0f) health.knockbackResist = knockbackResist;
            var ai = root.GetComponent<EnemyPathfindingAI>(); if (ai != null) ai.moveSpeed = speed;
            var damage = root.GetComponent<EnemyDamage>(); if (damage != null) damage.damage = contact;
            var behaviour = root.GetComponent<EnemyV140>(); if (behaviour == null) behaviour = root.AddComponent<EnemyV140>();
            behaviour.kind = kind;
            root.transform.localScale = Vector3.one * scale;
            if (kind >= EnemyV140.Kind.GoldenBeetle) { health.coinsMin = 4; health.coinsMax = 7; }
            if (kind == EnemyV140.Kind.FloatingEye) behaviour.minion = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "Ratpenat.prefab");
            if (kind == EnemyV140.Kind.Slug)
            {
                var split = root.GetComponent<EnemySplitter>(); if (split == null) split = root.AddComponent<EnemySplitter>();
                split.pieces = 3; split.maxGenerations = 1; split.childScale = .6f; split.childHealth = 2;
            }
            PrefabUtility.SaveAsPrefabAsset(root, folder + id + ".prefab");
            PrefabUtility.UnloadPrefabContents(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(folder + id + ".prefab");
        }

        var salamander = Make("Salamandra", EnemyV140.Kind.Salamander, 7, 2.6f, 1);
        var slug = Make("LlimacAcid", EnemyV140.Kind.Slug, 8, 1.0f, 1);
        var scorpion = Make("Escorpi", EnemyV140.Kind.Scorpion, 8, 1.7f, 1);
        var wolf = Make("LlopNeu", EnemyV140.Kind.SnowWolf, 4, 2.2f, 1);
        var bomber = Make("Bombardier", EnemyV140.Kind.Bomber, 5, 1.6f, 0);
        var turret = Make("TorretaAntiga", EnemyV140.Kind.AncientTurret, 12, 0f, 0, 1f, 1f);
        var beetle = Make("EscarabatDaurat", EnemyV140.Kind.GoldenBeetle, 28, 2.2f, 1, 1.4f, .6f);
        var eye = Make("UllFlotant", EnemyV140.Kind.FloatingEye, 26, 1.6f, 0, 1.4f, .6f);
        var frog = Make("GranotaGegant", EnemyV140.Kind.GiantFrog, 34, 1.5f, 1, 1.4f, .6f);
        var mage = Make("MagCendra", EnemyV140.Kind.AshMage, 32, 1.5f, 0, 1.4f, .6f);

        // Reina Aranya: sense el dash (no té sentit en una aranya) i amb teranyines que frenen (nota de Ferran, v1.3.3)
        {
            var queenPath = "Assets/Prefabs/Enemics/ReinaAranya.prefab";
            var queen = PrefabUtility.LoadPrefabContents(queenPath);
            var queenBoss = queen.GetComponent<EnemyBoss>();
            queenBoss.attacks = new[] { EnemyBoss.Attack.Summon, EnemyBoss.Attack.Fan, EnemyBoss.Attack.Jump, EnemyBoss.Attack.Web };
            PrefabUtility.SaveAsPrefabAsset(queen, queenPath); PrefabUtility.UnloadPrefabContents(queen);
        }

        foreach (string path in System.IO.Directory.GetFiles("Assets/Dungeon/Temes", "*.asset"))
        {
            var theme = AssetDatabase.LoadAssetAtPath<DungeonTheme>(path); if (theme == null) continue;
            var enemies = (theme.enemies ?? new GameObject[0]).ToList();
            void Add(GameObject p) { if (p != null && !enemies.Contains(p)) enemies.Add(p); }
            bool Is(params string[] names) => names.Any(n => path.Contains(n));
            if (Is("Volca")) Add(salamander);
            if (Is("Claveguera")) Add(slug);
            if (Is("Desert")) Add(scorpion);
            if (Is("Neu")) Add(wolf);
            if (Is("Volca", "Cristall")) Add(bomber);
            if (!Is("Cova")) Add(turret); // «totes» menys la primera planta, perquè no puje la dificultat inicial
            theme.enemies = enemies.ToArray();

            var micro = new List<GameObject>();
            if (Is("Desert", "Cova")) micro.Add(beetle);
            if (Is("Cripta", "Cristall", "Neu")) micro.Add(eye);
            if (Is("Bosc", "Claveguera")) micro.Add(frog);
            if (Is("Volca", "Cripta")) micro.Add(mage);
            theme.microbosses = micro.ToArray();
            EditorUtility.SetDirty(theme);
        }
        NetContentBuilder.Build();
        AssetDatabase.SaveAssets();
        Debug.Log("V141_CONTENT: 6 enemics i 4 microbosses preparats");
    }
}
