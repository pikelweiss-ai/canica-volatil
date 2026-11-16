using System.IO;
using UnityEditor;
using UnityEngine;

// Menú: Canica > Crear prefabs dels cofres
// Crea Assets/Prefabs/Cofres/Cofre_<Raresa>.prefab per a cada raresa,
// amb els sprites tancat/obert, collider i el script Chest ja configurats.
// Després només cal arrossegar-los a l'escena (i clonar-los amb Ctrl+D).
public static class ChestPrefabCreator
{
    const string SpriteFolder = "Assets/Sprites/Pixel/Cofres";
    const string PrefabFolder = "Assets/Prefabs/Cofres";

    [MenuItem("Canica/Crear prefabs dels cofres")]
    public static void CreateAll()
    {
        Directory.CreateDirectory(PrefabFolder);
        int created = 0;

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

            // Arrel: script + collider (el cofre no es mou quan vibra)
            GameObject root = new GameObject("Cofre_" + name);
            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.75f, 0.5f); // El dibuix fa 12x10 píxels (0.75 x 0.625)

            AudioSource audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            // Fill: l'sprite (és el que vibra i bota)
            GameObject visual = new GameObject("Sprite");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = closed;
            sr.sortingLayerName = "Player"; // Mateixa capa que el jugador...
            sr.sortingOrder = 1;            // ...però per davall d'ell (ordre 2)

            Chest chest = root.AddComponent<Chest>();
            chest.rarity = rarity;
            chest.spriteRenderer = sr;
            chest.closedSprite = closed;
            chest.openSprite = open;

            string path = PrefabFolder + "/Cofre_" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            created++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Cofres", created + " prefabs creats a " + PrefabFolder, "OK");
    }
}
