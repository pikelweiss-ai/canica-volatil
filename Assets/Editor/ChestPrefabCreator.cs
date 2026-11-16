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
//  - "Posar-ne de prova al mapa de proves": crea 'Tilemap Cofres' a GameScene i hi pinta
//      un cofre de cada raresa en cel·les lliures prop del SpawnPoint.
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

        foreach (ChestRarity rarity in System.Enum.GetValues(typeof(ChestRarity)))
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
        chest.lootTable = LoadAllWeapons();

        string path = PrefabFolder + "/Cofre_" + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // De moment, tots els cofres poden donar qualsevol arma del projecte (Assets/Weapons)
    static WeaponData[] LoadAllWeapons()
    {
        var list = new List<WeaponData>();
        foreach (string guid in AssetDatabase.FindAssets("t:WeaponData"))
        {
            WeaponData w = AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(guid));
            if (w != null) list.Add(w);
        }
        return list.ToArray();
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

    [MenuItem("Canica/Cofres/Posar-ne de prova al mapa de proves")]
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
        foreach (ChestRarity rarity in System.Enum.GetValues(typeof(ChestRarity)))
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

        // Cel·les lliures (amb terra, sense obstacles) a 3-7 cel·les del SpawnPoint,
        // separades entre elles i ordenades per proximitat
        Vector3Int center = ground.WorldToCell(spawn.transform.position);
        var candidates = new List<Vector3Int>();
        for (int x = -7; x <= 7; x++)
        {
            for (int y = -7; y <= 7; y++)
            {
                Vector3Int c = center + new Vector3Int(x, y, 0);
                float d = Mathf.Sqrt(x * x + y * y);
                if (d < 3f || d > 7f) continue;
                if (!ground.HasTile(c)) continue;
                if (IsBlocked(c, obstacles)) continue;
                candidates.Add(c);
            }
        }
        candidates.Sort((a, b) => (a - center).sqrMagnitude.CompareTo((b - center).sqrMagnitude));

        var placed = new List<Vector3Int>();
        foreach (ChestRarity rarity in System.Enum.GetValues(typeof(ChestRarity)))
        {
            if (!tiles.ContainsKey(rarity)) continue;
            foreach (Vector3Int c in candidates)
            {
                bool farEnough = true;
                foreach (Vector3Int p in placed)
                    if ((p - c).sqrMagnitude < 9) { farEnough = false; break; } // 3 cel·les
                if (!farEnough) continue;

                chests.SetTile(c, tiles[rarity]);
                placed.Add(c);
                break;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Cofres] " + placed.Count + " cofres posats a GameScene: " + string.Join(", ", placed) +
                  " (tiles al tilemap: " + chests.GetUsedTilesCount() + ")");

        if (!string.IsNullOrEmpty(originalScene) && originalScene != TestScene)
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("Cofres", placed.Count + " cofres posats a GameScene (a '" + ChestTilemapName + "').", "OK");
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
