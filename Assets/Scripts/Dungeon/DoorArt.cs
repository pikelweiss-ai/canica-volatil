using UnityEngine;

// Sprites de les portes de la masmorra (v1.4.0). Cada estil té els tres estats (tancada, mitjana, oberta)
// en les dues orientacions i en les tres peces d'una porta de 3 caselles (esquerra, mig, dreta).
// Es crea sol amb el menú Canica > Masmorra > Portes (DoorArtBuilder).
[CreateAssetMenu(fileName = "DoorArt", menuName = "Canica/Art de portes")]
public class DoorArt : ScriptableObject
{
    [System.Serializable]
    public class Style
    {
        public string id;
        public Sprite[] sprites; // índex = ((orientació * 3 + estat) * 3 + peça); orientació 0 = fulla horitzontal, 1 = vertical
    }

    public Style[] styles;

    static DoorArt instance;
    public static DoorArt Load()
    {
        if (instance == null) instance = Resources.Load<DoorArt>("DoorArt");
        return instance;
    }

    public Sprite Get(string style, bool vertical, int state, int part)
    {
        if (styles == null) return null;
        foreach (var s in styles)
        {
            if (s.id != style || s.sprites == null) continue;
            int index = ((vertical ? 1 : 0) * 3 + Mathf.Clamp(state, 0, 2)) * 3 + Mathf.Clamp(part, 0, 2);
            return index < s.sprites.Length ? s.sprites[index] : null;
        }
        return null;
    }
}
