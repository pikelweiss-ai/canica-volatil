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
    int baseMaxHealth;
    private SpriteRenderer sprite;

    void Start()
    {
        GameAudio.Track(damageAudioSource,SoundCategory.Effects);
        sprite = GetComponent<SpriteRenderer>();
        baseMaxHealth = maxHealth;
        maxHealth = Mathf.Max(1, baseMaxHealth + RunAmulets.ExtraHealth);
        currentHealth = maxHealth;

        // A la masmorra, la vida es conserva entre pisos
        if (RunState.Active && RunState.Health > 0)
            currentHealth = Mathf.Min(RunState.Health, maxHealth);

        UpdateHealthUI();
    }

    public void TakeDamage(int damage, EnemyHealth contact = null)
    {
        if (damage <= 0 || IsInvulnerable || currentHealth <= 0) return;

        if (RunAmulets.Defend(RunAmulets.Action.Glass))
        {
            Fx.Text(transform.position, "Escut de vidre", UIKit.Gold);
            return;
        }
        currentHealth -= damage;
        if (contact != null && RunAmulets.Has(Amulet.Thorns)) contact.TakeDamage(1);

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
            if (RunAmulets.Defend(RunAmulets.Action.Phoenix))
            {
                currentHealth = Mathf.Max(1, Mathf.CeilToInt(maxHealth * .5f));
                invulnerableUntil = Time.time + 2f;
                Fx.Burst(transform.position, new Color(1f, .58f, .22f), 20, 3f);
                UpdateHealthUI();
                StartCoroutine(nameof(Blink));
                return;
            }
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
            // Online: caus i esperes els companys (si cauen tots, s'acaba per a tots)
            if (!NetCoop.InterceptDeath(this)) DungeonGenerator.Instance.PlayerDied();
            gameObject.SetActive(false);
            return;
        }

        Respawn();
    }

    // Online: torna a la partida (quan els companys netegen la sala)
    public void Revive()
    {
        RestoreFullHealth(2f);
        Fx.Burst(transform.position, new Color(0.5f, 1f, 0.6f), 12, 3f);
    }

    void Respawn()
    {
        if (spawnPoint != null)
            transform.position = spawnPoint.position;

        RestoreFullHealth(1.5f);
    }

    void RestoreFullHealth(float protection)
    {
        currentHealth = maxHealth;
        invulnerableUntil = Time.time + protection;
        UpdateHealthUI();
        gameObject.SetActive(true);
        StopCoroutine(nameof(Blink));
        StartCoroutine(nameof(Blink));
    }

    public void RefreshAmuletHealth()
    {
        if (baseMaxHealth <= 0) return;
        int previous = maxHealth;
        maxHealth = Mathf.Max(1, baseMaxHealth + RunAmulets.ExtraHealth);
        if (currentHealth > 0) currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, maxHealth - previous));
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        RunState.Health = currentHealth;
        if (hearts == null || hearts.Length == 0) return;
        while (maxHealth > hearts.Length && hearts[hearts.Length - 1] != null)
        {
            int last = hearts.Length - 1;
            var template = hearts[last];
            var copy = Instantiate(template, template.transform.parent);
            var stride = last > 0 && hearts[last - 1] != null
                ? template.rectTransform.anchoredPosition - hearts[last - 1].rectTransform.anchoredPosition
                : new Vector2(template.rectTransform.rect.width, 0);
            copy.rectTransform.anchoredPosition = template.rectTransform.anchoredPosition + stride;
            System.Array.Resize(ref hearts, hearts.Length + 1);
            hearts[last + 1] = copy;
        }
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].enabled = i < maxHealth;
            hearts[i].sprite = i < currentHealth ? cor_ple : cor_buit;
        }
    }
}
