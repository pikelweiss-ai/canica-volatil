using UnityEngine;
using UnityEngine.Tilemaps;

// Tema d'un pis de la masmorra: quins tiles fa servir i de quin color és la llum.
// Es crea amb: clic dret a Project -> Create -> Canica -> Tema de masmorra
[CreateAssetMenu(fileName = "NouTema", menuName = "Canica/Tema de masmorra")]
public class DungeonTheme : ScriptableObject
{
    public string themeName = "Cova";
    public TileBase[] floorTiles;          // Es trien a l'atzar (el primer surt més)
    public TileBase wallTile;
    public TileBase liquidTile;            // Basses dins de les sales (aigua, lava...)
    [Range(0f, 1f)] public float liquidChance = 0.3f;
    public Color lightColor = Color.white; // Color de la llum global 2D
    public float lightIntensity = 1f;
    public Color backgroundColor = new Color(0.07f, 0.07f, 0.09f);
    public Sprite voidSprite;
    public TileBase refugeFloor;
    public GameObject[] decorPrefabs;

    [Header("Enemics d'este tema (si està buit, els de la masmorra)")]
    public GameObject[] enemies;
    public string doorStyle;               // v1.4.0: estil de les portes (pedra, reixa_castell, preso, lab_metall, perill, vidre)
    public GameObject mimic;               // v1.4.0: cofre fals que apareix a les sales de tresor des del pis 2
    public GameObject boss;                // Si està buit, el boss per defecte
}
