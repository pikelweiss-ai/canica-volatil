using System.Linq;
using UnityEditor;
using UnityEngine;

public static class NetContentBuilder
{
    public static void Build()
    {
        const string path = "Assets/Resources/NetContent.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<NetContentCatalog>(path);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<NetContentCatalog>();
            AssetDatabase.CreateAsset(catalog, path);
        }
        catalog.weapons = AssetDatabase.FindAssets("t:WeaponData")
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p)
            .Select(AssetDatabase.LoadAssetAtPath<WeaponData>).ToArray();
        if (catalog.weapons.GroupBy(w => w.name).Any(g => g.Count() > 1))
            throw new System.Exception("Noms d'armes duplicats al cataleg online");
        // Mode Creatiu: tots els enemics (bosses inclosos) i els quatre cofres
        catalog.enemies = Prefabs("Assets/Prefabs/Enemics").Where(g => g.GetComponent<EnemyHealth>() != null).ToArray();
        catalog.chests = Prefabs("Assets/Prefabs/Cofres").Where(g => g.GetComponent<Chest>() != null)
            .OrderBy(g => (int)g.GetComponent<Chest>().rarity).ToArray();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }

    static GameObject[] Prefabs(string folder) => AssetDatabase.FindAssets("t:Prefab", new[] { folder })
        .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p)
        .Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(g => g != null).ToArray();
}
