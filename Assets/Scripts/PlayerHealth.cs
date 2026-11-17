using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public CameraFollow cameraFollow;

    public Image[] hearts;

    public Sprite cor_ple;
    public Sprite cor_buit;

    public AudioSource damageAudioSource;

    public Transform spawnPoint;

    public GameObject poofEffectPrefab;

    [Header("Invulnerabilitat després d'un colp")]
    public float invulnerableTime = 1f;   // Mentre parpelleja, no et poden fer mal

    public bool IsFull => currentHealth >= maxHealth;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    private float invulnerableUntil;
    private SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        // A la masmorra, la vida es conserva entre pisos
        if (RunState.Active && RunState.Health > 0)
            currentHealth = Mathf.Min(RunState.Health, maxHealth);

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsInvulnerable || currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        if (damageAudioSource != null)
        {
            damageAudioSource.Play();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ShakeCamera(0.3f, 0.3f);
        }

        Fx.DamageNumber(transform.position, damage, new Color(1f, 0.35f, 0.35f));
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        invulnerableUntil = Time.time + invulnerableTime;
        StopCoroutine(nameof(Blink));
        StartCoroutine(nameof(Blink));
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
        RunState.Health = currentHealth;
    }

    // Parpelleja mentre és invulnerable
    IEnumerator Blink()
    {
        if (sprite == null) yield break;
        while (IsInvulnerable)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        sprite.enabled = true;
    }

    void Die()
    {
        if (poofEffectPrefab != null)
            Instantiate(poofEffectPrefab, transform.position, Quaternion.identity);

        // A la masmorra: s'acaba la partida i tornes a començar
        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.PlayerDied();
            gameObject.SetActive(false);
            return;
        }

        Respawn();
    }

    void Respawn()
    {
        if (spawnPoint != null)
            transform.position = spawnPoint.position;

        currentHealth = maxHealth;
        invulnerableUntil = Time.time + 1.5f;
        StartCoroutine(nameof(Blink));
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        RunState.Health = currentHealth;
        if (hearts == null) return;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].sprite = i < currentHealth ? cor_ple : cor_buit;
        }
    }
}
