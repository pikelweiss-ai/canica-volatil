using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Menú Canica > Cofres:
//  - "Crear prefabs i tiles": per a cada raresa crea
//      Assets/Prefabs/Cofres/Cofre_<Raresa>.prefab  (cofre real: sprite, hitbox, script)
//      Assets/Tiles/Cofres/TileCofre_<Raresa>.asset (per a pintar-lo amb la Tile Palette)
//  - "Omplir el mapa de cofres": crea 'Tilemap Cofres' a GameScene i hi pinta
//      11 cofres per tot el mapa (cada raresa dona les seues armes) i mou els enemics a la vora.
public static class ChestPrefabCreator
{
    const string SpriteFolder = "Assets/Sprites/Pixel/Cofres";
    const string PrefabFolder = "Assets/Prefabs/Cofres";
    const string TileFolder = "Assets/Tiles/Cofres";
    const string TestScene = "Assets/Scenes/GameScene.unity";
    const string ChestTilemapName = "Tilemap Cofres";

    [MenuItem("Canica/Cofres/Crear prefabs i tiles")]
    public static void CreateAllMenu()
    {
        int n = CreateAll().Count;
        EditorUtility.DisplayDialog("Cofres",
            n + " cofres creats.\n\nPrefabs: " + PrefabFolder + "\nTiles: " + TileFolder +
            "\n\nArrossega els tiles a la Tile Palette per a pintar-los al mapa.", "OK");
    }

    public static Dictionary<ChestRarity, ChestTile> CreateAll()
    {
        Directory.CreateDirectory(PrefabFolder);
        Directory.CreateDirectory(TileFolder);
        var tiles = new Dictionary<ChestRarity, ChestTile>();

        foreach (ChestRarity rarity in new[] { ChestRarity.Comu, ChestRarity.Rar, ChestRarity.Epic, ChestRarity.Llegendari })
        {
            string name = rarity.ToString();
            Sprite closed = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/Cofre_" + name + ".png");
            Sprite open = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/Cofre_" + name + "_Obert.png");

            if (closed == null || open == null)
            {
                Debug.LogWarning("[Cofres] Falten els sprites de " + name + " a " + SpriteFolder);
                continue;
            }

            GameObject prefab = CreatePrefab(rarity, closed, open);
            tiles[rarity] = CreateTile(rarity, closed, prefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return tiles;
    }

    static GameObject CreatePrefab(ChestRarity rarity, Sprite closed, Sprite open)
    {
        string name = rarity.ToString();

        // Arrel: script + hitbox (no es mou quan el cofre vibra)
        // El dibuix del cofre va de y=-0.3125 (base) a y=0.25 (tapa).
        // La hitbox només ocupa la meitat de baix: així pots passar per darrere.
        GameObject root = new GameObject("Cofre_" + name);
        BoxCollider2D col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.75f, 0.28f);
        col.offset = new Vector2(0f, -0.17f);

        AudioSource audio = root.AddComponent<AudioSource>();
        audio.playOnAwake = false;

        // Ombra: a l'arrel, perquè es quede al terra quan el cofre vibra o bota
        Sprite shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + "/Ombra.png");
        if (shadowSprite != null)
        {
            GameObject shadow = new GameObject("Ombra");
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            SpriteRenderer ssr = shadow.AddComponent<SpriteRenderer>();
            ssr.sprite = shadowSprite;
            ssr.color = new Color(0f, 0f, 0f, 130f / 255f); // Igual de fosca que l'ombra del jugador (sombra 1.png)
            ssr.sortingLayerName = "Shadows"; // Per davall del jugador i del cofre
        }

        // Fill: l'sprite (és el que vibra i bota)
        GameObject visual = new GameObject("Sprite");
        visual.transform.SetParent(root.transform, false);
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = closed;
        sr.sortingLayerName = "Player"; // Mateixa capa que el jugador. En jugar, Chest posa l'ordre
        sr.sortingOrder = 1;            // davant o darrere de la canica segons l'altura

        Chest chest = root.AddComponent<Chest>();
        chest.rarity = rarity;
        chest.spriteRenderer = sr;
        chest.closedSprite = closed;
        chest.openSprite = open;
        chest.lootTable = LoadWeapons(rarity);
        chest.healChance = 0.75f;

        string path = PrefabFolder + "/Cofre_" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // Cada cofre dona les armes de la SEUA raresa (cofre Rar -> armes Rar...).
    // Si encara no hi ha cap arma d'eixa raresa, pot donar qualsevol arma del projecte.
    static WeaponData[] LoadWeapons(ChestRarity rarity)
    {
        var all = new List<WeaponData>();
        var same = new List<WeaponData>();
        foreach (string guid in AssetDatabase.FindAssets("t:WeaponData"))
        {
            WeaponData w = AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(guid));
            if (w == null) continue;
            all.Add(w);
            if (w.rarity == rarity || (rarity == ChestRarity.Comu && w.rarity == ChestRarity.PocComu)) same.Add(w);
        }
        return (same.Count > 0 ? same : all).ToArray();
    }

    static ChestTile CreateTile(ChestRarity rarity, Sprite preview, GameObject prefab)
    {
        string path = TileFolder + "/TileCofre_" + rarity + ".asset";
        ChestTile tile = AssetDatabase.LoadAssetAtPath<ChestTile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<ChestTile>();
            AssetDatabase.CreateAsset(tile, path);
        }
        tile.previewSprite = preview;
        tile.chestPrefab = prefab;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    [MenuItem("Canica/Cofres/Omplir el mapa de cofres (i enemics a la vora)")]
    public static void PlaceTestChests()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Dictionary<ChestRarity, ChestTile> tiles = CreateAll();
        if (tiles.Count == 0)
        {
            EditorUtility.DisplayDialog("Cofres", "No s'han trobat els sprites dels cofres.", "OK");
            return;
        }

        string originalScene = EditorSceneManager.GetActiveScene().path;
        var scene = EditorSceneManager.OpenScene(TestScene, OpenSceneMode.Single);

        // En obrir una escena, Unity descarrega els assets que no s'usen: tornem a carregar els tiles
        tiles.Clear();
        foreach (ChestRarity rarity in new[] { ChestRarity.Comu, ChestRarity.Rar, ChestRarity.Epic, ChestRarity.Llegendari })
        {
            ChestTile t = AssetDatabase.LoadAssetAtPath<ChestTile>(TileFolder + "/TileCofre_" + rarity + ".asset");
            if (t != null) tiles[rarity] = t;
        }

        Tilemap ground = FindTilemap("Tilemap Ground");
        GameObject grid = GameObject.Find("Grid");
        GameObject spawn = GameObject.Find("SpawnPoint");
        if (ground == null || grid == null || spawn == null)
        {
            EditorUtility.DisplayDialog("Cofres", "No trobe 'Grid', 'Tilemap Ground' o 'SpawnPoint' a GameScene.", "OK");
            return;
        }

        Tilemap chests = FindTilemap(ChestTilemapName);
        if (chests == null)
        {
            GameObject go = new GameObject(ChestTilemapName, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(grid.transform, false);
            TilemapRenderer tr = go.GetComponent<TilemapRenderer>();
            tr.sortingLayerName = "Player";
            tr.sortingOrder = 2;
            chests = go.GetComponent<Tilemap>();
        }
        chests.ClearAllTiles();

        Tilemap[] obstacles =
        {
            FindTilemap("Tilemap Walls"), FindTilemap("Tilemap Liquid"), FindTilemap("Tilemap Trees")
        };

        // Totes les cel·les lliures del mapa (amb terra, sense obstacles)
        Vector3Int center = ground.WorldToCell(spawn.transform.position);
        var free = new List<Vector3Int>();
        float maxDist = 0f;
        BoundsInt bounds = ground.cellBounds;
        foreach (Vector3Int c in bounds.allPositionsWithin)
        {
            if (!ground.HasTile(c) || IsBlocked(c, obstacles)) continue;
            free.Add(c);
            maxDist = Mathf.Max(maxDist, Dist(c, center));
        }

        // Cofres: per tot el mapa, entre 3 cel·les del SpawnPoint i el 70 % del radi
        // (la vora queda per als enemics). Ordre a l'atzar, però sempre el mateix (llavor fixa).
        var candidates = free.FindAll(c => Dist(c, center) >= 3f && Dist(c, center) <= maxDist * 0.7f);
        var rng = new System.Random(1234);
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        var placed = new List<Vector3Int>();
        foreach (ChestRarity rarity in new[] { ChestRarity.Comu, ChestRarity.Rar, ChestRarity.Epic, ChestRarity.Llegendari })
        {
            if (!tiles.ContainsKey(rarity)) continue;
            int count = ChestsPerRarity[(int)rarity];
            foreach (Vector3Int c in candidates)
            {
                if (count == 0) break;
                bool farEnough = true;
                foreach (Vector3Int p in placed)
                    if ((p - c).sqrMagnitude < 16) { farEnough = false; break; } // 4 cel·les
                if (!farEnough) continue;

                chests.SetTile(c, tiles[rarity]);
                placed.Add(c);
                count--;
            }
        }

        int enemies = MoveEnemiesToEdge(ground, free, center, maxDist);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Cofres] " + placed.Count + " cofres posats a GameScene: " + string.Join(", ", placed) +
                  " (tiles al tilemap: " + chests.GetUsedTilesCount() + ")");

        if (!string.IsNullOrEmpty(originalScene) && originalScene != TestScene)
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("Cofres", placed.Count + " cofres posats a GameScene (a '" + ChestTilemapName + "').\n" +
            enemies + " enemic(s) moguts a la vora del mapa.", "OK");
    }

    // Quants cofres de cada raresa: Comú, Rar, Èpic, Llegendari
    static readonly int[] ChestsPerRarity = { 4, 3, 2, 2 };

    static float Dist(Vector3Int a, Vector3Int b)
    {
        Vector3Int d = a - b;
        return Mathf.Sqrt(d.x * d.x + d.y * d.y);
    }

    // Reparteix els enemics al voltant del cercle, a la vora del mapa (lluny del SpawnPoint):
    // l'enemic i va a l'angle i·360/n, a la cel·la lliure més allunyada en eixa direcció.
    static int MoveEnemiesToEdge(Tilemap ground, List<Vector3Int> free, Vector3Int center, float maxDist)
    {
        EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.InstanceID);
        var edge = free.FindAll(c => Dist(c, center) >= maxDist * 0.85f);
        if (edge.Count == 0) return 0;

        for (int i = 0; i < enemies.Length; i++)
        {
            float angle = 45f + i * 360f / enemies.Length; // 45° = cantó de dalt a la dreta
            Vector2 want = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            Vector3Int best = edge[0];
            float bestScore = float.MinValue;
            foreach (Vector3Int c in edge)
            {
                Vector2 dir = new Vector2(c.x - center.x, c.y - center.y).normalized;
                float score = Vector2.Dot(dir, want) * 10f + Dist(c, center) * 0.1f; // direcció primer, després lluny
                if (score > bestScore) { bestScore = score; best = c; }
            }

            Transform t = enemies[i].transform;
            Undo.RecordObject(t, "Moure enemic a la vora");
            Vector3 pos = ground.GetCellCenterWorld(best);
            t.position = new Vector3(pos.x, pos.y, t.position.z);
            Debug.Log("[Cofres] Enemic '" + t.name + "' mogut a " + best);
        }
        return enemies.Length;
    }

    static Tilemap FindTilemap(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<Tilemap>() : null;
    }

    static bool IsBlocked(Vector3Int cell, Tilemap[] obstacles)
    {
        foreach (Tilemap t in obstacles)
            if (t != null && t.HasTile(cell)) return true;
        return false;
    }
}
