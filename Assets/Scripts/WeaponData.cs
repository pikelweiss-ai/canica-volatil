using UnityEngine;

// Quin so fa l'arma:
// - Tret: un "bang" cada vegada que dispara (AK...)
// - Llancament: un "fiuu" en llançar-la i un "toc" quan impacta (pedra...)
public enum ProjectileEffect : byte { None, Slow, Poison, Chain }
// v1.3.1: armes amb comportament propi (Torreta, Mina, Arc de càrrega, Escut, Vareta curativa i Ganxo)
public enum SpecialKind : byte { None, Turret, Mine, Charge, Shield, HealBolt, Grapple }

public enum WeaponSound { Tret, Llancament, Escopeta, Sniper, Cristall, Electricitat }

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
    [Min(0f)] public float closeDamageRange; // Dins d'aquest radi, suma un punt per impacte.
    public float cooldown = 0.5f;       // Temps entre trets (segons)
    public float knockbackForce = 4f;

    [Header("Projectil")]
    public GameObject projectilePrefab; // Opcional: prefab propi del projectil
    public Sprite projectileSprite;     // Opcional: sprite del projectil (s'orienta cap on va)
    public float projectileSpriteScale = 1f;
    public float projectileSpeed = 12f;
    public float range = 6f;            // Distància màxima del projectil
    public float zoomRange = -1f;       // Abast que compta per a l'allunyament de la càmera: -1 = l'abast (màx. 10), 0 = no allunya
    public float projectileSize = 0.3f; // Mida de la hitbox (i de la boleta si no hi ha sprite)
    public Color projectileColor = Color.white;
    public bool tintProjectileSprite;

    [Header("Dispersió (0 = tret recte; l'aim es fa en V)")]
    public bool fixedSpread;
    [Range(0f, 45f)]
    public float spreadAngle = 0f;      // Angle màxim (cap a cada costat) que es desvia cada bala
    [Min(1)]
    public int pellets = 1;             // Bales per tret (escopeta: 5-8)
    [Range(0f, 0.5f)]
    public float rangeRandom = 0f;      // Cada bala pot quedar-se fins a aquest % més curta

    [Header("Cos a cos")]
    public float spinUpTime;
    public float meleeDashDistance;
    public bool alternateSwing;
    public bool meleeRadial;
    [Min(0)] public int meleeMaxTargets;    // Enemics màxims per colp (0 = tots). Llança: 3
    [Min(0)] public int bounceDamage;       // Dany que suma el projectil a cada rebot de paret (Disc)
    [Min(0f)] public float bounceRangeBonus;   // Cada rebot allarga el recorregut en aquesta fracció de l'abast (Disc: 0,15)
    [Min(0f)] public float hitSpeedBoost;      // Colpejar amb esta arma dona +% de velocitat (0,15 = +15 %)...
    [Min(0f)] public float hitSpeedBoostTime;  // ...durant tants segons (es renova, no s'acumula)
    [Min(0f)] public float meleeLineWidth;     // > 0: el colp cos a cos és una línia recta d'esta amplada (Llança), no un arc
    public SpecialKind special;             // Comportament propi de l'arma (v1.3.1)
    public float specialValue;              // Torreta: segons vius; Mina: radi; Arc: segons de càrrega; Escut: graus; Vareta curativa: abast; Ganxo: distància d'estirada
    public Sprite specialSprite, specialSprite2; // Torreta: cos i canó; Mina: cos i llum; Ganxo: harpó (v1.3.2)
    public bool melee = false;
    [Range(20f, 180f)] public float meleeArc = 110f;

    [Header("Especials")]
    public ProjectileEffect projectileEffect;
    public float explosionRadius = 0f;  // > 0: el projectil explota en acabar (granades)
    public int explosionDamage = 3;
    [Min(0)]
    public int pierce = 0;              // Enemics que travessa abans de parar (fletxes)
    [Min(0)] public int wallBounces;
    public bool returns = false;        // Bumerang: en acabar (o xocar) torna cap a tu, pegant de nou
    public float homing = 0f;           // Teledirigit: com de ràpid gira cap a l'enemic (radians/s, 0 = no)
    public float spinSpeed = 0f;        // El dibuix gira (graus/s): shurikens, bumerangs...

    [Header("Estat per instància")]
    [Min(0)] public int maxShots;
    [Min(0)] public int heatLimit;
    [Min(0f)] public float heatCoolingRate;
    [Min(0f)] public float overheatCooldown;

    [Header("So (si els clips estan buits, es generen per codi)")]
    public WeaponSound sound = WeaponSound.Tret;
    public AudioClip shootSound;
    public AudioClip impactSound;
}

public struct WeaponCondition
{
    public int ShotsUsed;
    public float Heat;

    public WeaponCondition(int shotsUsed, float heat)
    {
        ShotsUsed = shotsUsed;
        Heat = heat;
    }
}
