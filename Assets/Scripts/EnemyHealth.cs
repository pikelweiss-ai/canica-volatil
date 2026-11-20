using System;
using UnityEngine;

// Vida de qualsevol enemic.
// - TakeHit: dany + flash roig + empenta + número de dany
// - En morir: partícules, monedes, a voltes un item de cura, i avisa (Died) la sala de la masmorra
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public AudioSource damageAudioSource;
    public AudioClip[] damageSounds;

    public GameObject poofEffectPrefab;

    [Header("Feedback del colp")]
    public Color hitColor = Color.red;   // Color del flash en rebre un colp
    public float flashDuration = 0.1f;
    [Range(0f, 1f)] public float knockbackResist = 0f; // 1 = no el mou ningú (boss)

    [Header("Botí en morir")]
    public int coinsMin = 1;
    public int coinsMax = 2;
    [Range(0f, 1f)] public float healDropChance = 0.08f;
    public Color deathColor = new Color(0.6f, 0.9f, 0.35f);

    public bool IsDead => isDead;
    public event Action<EnemyHealth> Died;
    public static event Action<EnemyHealth> AnyDied;

    private bool isDead = false;

    // Online: identificador comu i si aquest enemic es una copia d'un enemic de l'amfitrio
    [System.NonSerialized] public int netId;
    [System.NonSerialized] public bool netProxy;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private EnemyPathfindingAI pathfinding;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        GameAudio.Track(damageAudioSource,SoundCategory.Enemies);
        if (currentHealth <= 0) currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        pathfinding = GetComponent<EnemyPathfindingAI>();
        NetCoop.EnemyStarted(this);
    }

    // El crida PlayerAttack / Projectile / explosions: fa mal, flash roig i empenta l'enemic
    float slowedUntil;
    int poisonDoses;
    public float MovementMultiplier => Time.time < slowedUntil ? 0.6f : 1f;

    public void TakeProjectileHit(int damage, Vector2 knockback, ProjectileEffect effect)
    {
        if (isDead) return;
        if (netProxy && effect != ProjectileEffect.None)
        {
            NetCoop.ProxyHit(this, damage, knockback, effect);
            return;
        }
        TakeHit(damage, knockback);
        if (effect == ProjectileEffect.Chain) ChainLightning();
        if (isDead) return;
        if (effect == ProjectileEffect.Slow) slowedUntil = Time.time + 0.5f;
        if (effect == ProjectileEffect.Poison && poisonDoses < 3) StartCoroutine(PoisonDose());
    }

    System.Collections.IEnumerator PoisonDose()
    {
        poisonDoses++;
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(1f);
            if (isDead) yield break;
            TakeHit(1, Vector2.zero);
        }
        poisonDoses--;
    }

    void ChainLightning()
    {
        var hit = new System.Collections.Generic.HashSet<EnemyHealth> { this };
        Vector2 origin = transform.position;
        for (int jump = 0; jump < 3; jump++)
        {
            EnemyHealth nearest = null;
            float distance = 2.5f * 2.5f;
            foreach (var collider in Physics2D.OverlapCircleAll(origin, 2.5f))
            {
                var target = collider.GetComponentInParent<EnemyHealth>();
                if (target == null || target.IsDead || hit.Contains(target)) continue;
                float squared = ((Vector2)target.transform.position - origin).sqrMagnitude;
                if (squared > distance) continue;
                distance = squared;
                nearest = target;
            }
            if (nearest == null) break;
            hit.Add(nearest);
            Vector2 next = nearest.transform.position;
            NetCoop.ChainJump(origin, next);
            nearest.TakeHit(1, Vector2.zero);
            origin = next;
        }
    }

    public void TakeHit(int damage, Vector2 knockback)
    {
        if (isDead) return;

        // Copia online: es veu el colp ací, pero el dany el decideix l'amfitrio
        if (netProxy)
        {
            if (spriteRenderer != null) { StopCoroutine(nameof(HitFlash)); StartCoroutine(nameof(HitFlash)); }
            Fx.DamageNumber(transform.position, damage, Color.white);
            PlayRandomDamageSound();
            NetCoop.ProxyHit(this, damage, knockback * (1f - knockbackResist));
            return;
        }

        if (pathfinding != null)
            pathfinding.ApplyKnockback(knockback * (1f - knockbackResist), 0.15f);

        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(HitFlash));
            StartCoroutine(nameof(HitFlash));
        }

        Fx.DamageNumber(transform.position, damage, Color.white);
        TakeDamage(damage);
    }

    System.Collections.IEnumerator HitFlash()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (netProxy) { NetCoop.ProxyHit(this, damage, Vector2.zero); return; }

        currentHealth -= damage;

        PlayRandomDamageSound();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void PlayRandomDamageSound()
    {
        if (damageAudioSource == null) return;
        if (damageSounds == null || damageSounds.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, damageSounds.Length);
        damageAudioSource.PlayOneShot(damageSounds[randomIndex]);
    }

    void Die()
    {
        isDead = true;

        if (poofEffectPrefab != null)
            Instantiate(poofEffectPrefab, transform.position, Quaternion.identity);

        Fx.Burst(transform.position, deathColor, 10, 3.5f);
        DropLoot();

        Died?.Invoke(this);
        AnyDied?.Invoke(this);
        NetCoop.EnemyDied(this);

        Destroy(gameObject);
    }

    // Copia online: l'amfitrio diu que ha mort. Efectes i boti per al jugador d'aquest PC
    public void NetDie()
    {
        if (isDead) return;
        isDead = true;
        if (poofEffectPrefab != null) Instantiate(poofEffectPrefab, transform.position, Quaternion.identity);
        Fx.Burst(transform.position, deathColor, 10, 3.5f);
        // El boti el crea l'amfitrio i es comparteix (el primer que l'agafa)
        Destroy(gameObject);
    }

    void DropLoot()
    {
        int coins = UnityEngine.Random.Range(coinsMin, coinsMax + 1);
        for (int i = 0; i < coins; i++)
            CoinDrop.Spawn(transform.position);

        if (UnityEngine.Random.value < healDropChance)
        {
            HealItem item = HealItem.RandomCommon();
            if (item != null) HealPickup.Spawn(item, transform.position, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.6f);
        }
    }
}
