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

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
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
            cameraFollow.ShakeCamera(0.50f, 0.50f);
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        if (poofEffectPrefab != null)
        {
            Instantiate(poofEffectPrefab, transform.position, Quaternion.identity);
        }

        transform.position = spawnPoint.position;

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = cor_ple;
            }
            else
            {
                hearts[i].sprite = cor_buit;
            }
        }
    }
}