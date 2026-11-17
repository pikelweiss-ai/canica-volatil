using UnityEngine;

// Llim: quan mor es divideix en llims més xicotets (fins a 'maxGenerations' vegades).
[RequireComponent(typeof(EnemyHealth))]
public class EnemySplitter : MonoBehaviour
{
    public int pieces = 2;
    public int maxGenerations = 1;
    public float childScale = 0.65f;
    [HideInInspector] public int generation = 0;

    void Start()
    {
        GetComponent<EnemyHealth>().Died += OnDied;
    }

    void OnDied(EnemyHealth h)
    {
        if (generation >= maxGenerations) return;

        for (int i = 0; i < pieces; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * 0.35f;
            Vector2 pos = (Vector2)transform.position + offset;
            if (TileNav.Ground != null && TileNav.IsBlockedWorld(pos)) pos = transform.position;

            GameObject child = Instantiate(gameObject, pos, Quaternion.identity);
            child.name = gameObject.name;
            child.transform.localScale = transform.localScale * childScale;
            SpriteRenderer csr = child.GetComponent<SpriteRenderer>();
            if (csr != null) csr.color = Color.white; // Podia estar en mig del flash roig

            EnemySplitter s = child.GetComponent<EnemySplitter>();
            s.generation = generation + 1;

            EnemyHealth ch = child.GetComponent<EnemyHealth>();
            ch.maxHealth = Mathf.Max(1, h.maxHealth / 2);
            ch.currentHealth = ch.maxHealth;
            ch.coinsMax = 1;

            EnemyPathfindingAI ai = child.GetComponent<EnemyPathfindingAI>();
            if (ai != null) { ai.moveSpeed *= 1.25f; ai.bodyRadius *= childScale; }

            // La sala de la masmorra ha de saber que hi ha més enemics
            if (DungeonGenerator.Instance != null) DungeonGenerator.Instance.RegisterSpawned(ch);
            if (ai != null) ai.Invoke(nameof(EnemyPathfindingAI.Alert), 0.05f);
        }
    }
}
