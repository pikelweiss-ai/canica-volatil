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
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }
}
