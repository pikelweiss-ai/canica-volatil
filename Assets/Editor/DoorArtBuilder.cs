using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// v1.4.0: copia els sprites de ArtNou_v1.4/Portes_dungeon i crea Resources/DoorArt.asset
public static class DoorArtBuilder
{
    public static readonly string[] Styles = { "pedra", "reixa_castell", "preso", "lab_metall", "perill", "vidre" };
    static readonly string[] States = { "tancada", "mitja", "oberta" };
    static readonly string[] Parts = { "esq", "mig", "dre" };

    [MenuItem("Canica/Masmorra/Portes (v1.4)")]
    public static void Build()
    {
        const string folder = "Assets/Sprites/Pixel/Portes";
        foreach (var orientation in new[] { "H", "V" })
        {
            Directory.CreateDirectory(folder + "/" + orientation);
            foreach (var source in Directory.GetFiles("ArtNou_v1.4/Portes_dungeon/" + orientation, "*.png"))
            {
                var dest = folder + "/" + orientation + "/" + Path.GetFileName(source);
                if (!File.Exists(dest)) File.Copy(source, dest);
            }
        }
        AssetDatabase.Refresh();
        var art = AssetDatabase.LoadAssetAtPath<DoorArt>("Assets/Resources/DoorArt.asset");
        if (art == null) { art = ScriptableObject.CreateInstance<DoorArt>(); AssetDatabase.CreateAsset(art, "Assets/Resources/DoorArt.asset"); }
        art.styles = Styles.Select(style =>
        {
            var list = new System.Collections.Generic.List<Sprite>();
            foreach (var orientation in new[] { "H", "V" })
                foreach (var state in States)
                    foreach (var part in Parts)
                    {
                        string path = folder + "/" + orientation + "/Porta_" + style + "_" + state + "_" + part + ".png";
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sprite == null) throw new Exception("Falta el sprite de porta " + path);
                        list.Add(sprite);
                    }
            return new DoorArt.Style { id = style, sprites = list.ToArray() };
        }).ToArray();
        EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
        Debug.Log("DOOR_ART: " + art.styles.Length + " estils de porta");
    }
}
