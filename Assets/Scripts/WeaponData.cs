using UnityEngine;

// Una arma de la hotbar. Es crea des de Unity amb:
// clic dret a Project -> Create -> Canica -> Arma
[CreateAssetMenu(fileName = "NovaArma", menuName = "Canica/Arma")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string weaponName = "Arma";
    public Sprite icon;                 // Icona de la hotbar i sprite quan està al terra
    public ChestRarity rarity = ChestRarity.Comu; // Color de la vora quan està al terra

    [Header("Atac")]
    public int damage = 1;
    public float cooldown = 0.5f;       // Temps entre trets (segons)
    public float knockbackForce = 4f;

    [Header("Projectil")]
    public GameObject projectilePrefab; // Opcional: si està buit, es fa una boleta
    public float projectileSpeed = 12f;
    public float range = 6f;            // Distància màxima del projectil
    public float projectileSize = 0.3f;
    public Color projectileColor = Color.white;
}
