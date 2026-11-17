using UnityEngine;

// Quin so fa l'arma:
// - Tret: un "bang" cada vegada que dispara (AK...)
// - Llancament: un "fiuu" en llançar-la i un "toc" quan impacta (pedra...)
public enum WeaponSound { Tret, Llancament }

// Una arma de la hotbar. Es crea des de Unity amb:
// clic dret a Project -> Create -> Canica -> Arma
[CreateAssetMenu(fileName = "NovaArma", menuName = "Canica/Arma")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string weaponName = "Arma";
    public Sprite icon;                 // Icona de la hotbar, sprite a la mà i al terra
    public ChestRarity rarity = ChestRarity.Comu; // Color de la vora quan està al terra
    public float handScale = 0.75f;     // Mida de l'arma a la mà del jugador

    [Header("Atac")]
    public int damage = 1;
    public float cooldown = 0.5f;       // Temps entre trets (segons)
    public float knockbackForce = 4f;

    [Header("Projectil")]
    public GameObject projectilePrefab; // Opcional: prefab propi del projectil
    public Sprite projectileSprite;     // Opcional: sprite del projectil (s'orienta cap on va)
    public float projectileSpriteScale = 1f;
    public float projectileSpeed = 12f;
    public float range = 6f;            // Distància màxima del projectil
    public float projectileSize = 0.3f; // Mida de la hitbox (i de la boleta si no hi ha sprite)
    public Color projectileColor = Color.white;

    [Header("Dispersió (0 = tret recte; l'aim es fa en V)")]
    [Range(0f, 45f)]
    public float spreadAngle = 0f;      // Angle màxim (cap a cada costat) que es desvia cada bala
    [Min(1)]
    public int pellets = 1;             // Bales per tret (escopeta: 5-8)
    [Range(0f, 0.5f)]
    public float rangeRandom = 0f;      // Cada bala pot quedar-se fins a aquest % més curta

    [Header("Especials")]
    public float explosionRadius = 0f;  // > 0: el projectil explota en acabar (granades)
    public int explosionDamage = 3;
    [Min(0)]
    public int pierce = 0;              // Enemics que travessa abans de parar (fletxes)
    public bool returns = false;        // Bumerang: en acabar (o xocar) torna cap a tu, pegant de nou
    public float homing = 0f;           // Teledirigit: com de ràpid gira cap a l'enemic (radians/s, 0 = no)
    public float spinSpeed = 0f;        // El dibuix gira (graus/s): shurikens, bumerangs...

    [Header("So (si els clips estan buits, es generen per codi)")]
    public WeaponSound sound = WeaponSound.Tret;
    public AudioClip shootSound;
    public AudioClip impactSound;
}
