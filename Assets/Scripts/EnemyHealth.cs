using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public AudioSource damageAudioSource;
    public AudioClip[] damageSounds;

    public GameObject poofEffectPrefab;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(1);
        }
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