using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Comprova si el jugador trepitja un tile que fa mal (DamageTile, p. ex. la lava).
// S'afig sol al jugador en carregar qualsevol escena (no cal posar-lo a mà).
public class PlayerHazards : MonoBehaviour
{
    public float checkInterval = 0.25f;

    private Tilemap[] tilemaps;
    private float timer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Hook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachToPlayer();
    }

    static void OnSceneLoaded(Scene s, LoadSceneMode m) => AttachToPlayer();

    static void AttachToPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) return;
        if (p.GetComponent<PlayerHazards>() == null) p.AddComponent<PlayerHazards>();
        if (p.GetComponent<PlayerCosmetics>() == null) p.AddComponent<PlayerCosmetics>();
        if (p.GetComponent<HealInventory>() == null) p.AddComponent<HealInventory>();
    }

    void Start()
    {
        tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = checkInterval;

        foreach (Tilemap t in tilemaps)
        {
            if (t == null) continue;
            if (t.GetTile(t.WorldToCell(transform.position)) is DamageTile d)
            {
                GetComponent<PlayerHealth>()?.TakeDamage(d.damage);
                return;
            }
        }
    }
}
