using UnityEngine;

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

    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private EnemyPathfindingAI pathfinding;

    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        pathfinding = GetComponent<EnemyPathfindingAI>();
    }

    // El crida PlayerAttack: fa mal, flash roig i empenta l'enemic
    public void TakeHit(int damage, Vector2 knockback)
    {
        if (isDead) return;

        if (pathfinding != null)
            pathfinding.ApplyKnockback(knockback, 0.15f);

        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(HitFlash));
            StartCoroutine(nameof(HitFlash));
        }

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

        int randomIndex = Random.Range(0, damageSounds.Length);
        damageAudioSource.PlayOneShot(damageSounds[randomIndex]);
    }

    void Die()
    {
        isDead = true;

        if (poofEffectPrefab != null)
        {
            Instantiate(poofEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}