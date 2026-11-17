using UnityEngine;

// Llista de coses per a personalitzar la canica. N'hi ha una a Assets/Resources/Cosmetics.asset
// (la crea el menú Canica > Masmorra > Crear-ho tot). Per a afegir un barret: dibuixa'l a
// Tools/PixelArt/masmorra.ps1 (secció Barrets), torna a executar draw.ps1 i afig-lo ací.
[CreateAssetMenu(fileName = "Cosmetics", menuName = "Canica/Base de dades de personalització")]
public class CosmeticsDatabase : ScriptableObject
{
    [System.Serializable]
    public class Hat { public string name; public Sprite sprite; }

    [System.Serializable]
    public class Tint { public string name; public Color color = Color.white; }

    public Sprite playerSprite;        // Per a la previsualització del menú
    public Tint[] colors =
    {
        new Tint { name = "Original", color = Color.white },
        new Tint { name = "Foc", color = new Color(1f, 0.55f, 0.45f) },
        new Tint { name = "Gel", color = new Color(0.6f, 0.85f, 1f) },
        new Tint { name = "Gespa", color = new Color(0.6f, 1f, 0.55f) },
        new Tint { name = "Raïm", color = new Color(0.8f, 0.6f, 1f) },
        new Tint { name = "Or", color = new Color(1f, 0.88f, 0.4f) },
        new Tint { name = "Ombra", color = new Color(0.45f, 0.45f, 0.55f) },
    };
    public Hat[] hats;                 // El primer pot ser "Cap" (sprite buit)
    public Tint[] trails =
    {
        new Tint { name = "Cap", color = Color.clear },
        new Tint { name = "Blanc", color = Color.white },
        new Tint { name = "Foc", color = new Color(1f, 0.5f, 0.15f) },
        new Tint { name = "Elèctric", color = new Color(0.3f, 0.9f, 1f) },
        new Tint { name = "Toxic", color = new Color(0.55f, 1f, 0.3f) },
        new Tint { name = "Arc de Sant Martí", color = new Color(1f, 0f, 1f, 0.99f) }, // Especial: canvia de color
    };

    public const string KeyColor = "Cos_Color", KeyHat = "Cos_Hat", KeyTrail = "Cos_Trail";

    static CosmeticsDatabase instance;
    public static CosmeticsDatabase Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<CosmeticsDatabase>("Cosmetics");
            return instance;
        }
    }

    public static int Get(string key, int count) => count <= 0 ? 0 : Mathf.Clamp(PlayerPrefs.GetInt(key, 0), 0, count - 1);

    public static bool IsRainbow(Color c) => c.a > 0.98f && c.a < 1f;
}
