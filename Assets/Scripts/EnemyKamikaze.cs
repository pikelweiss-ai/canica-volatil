using UnityEngine;

// Kamikaze: corre cap a tu i, quan està a prop, s'encén (parpelleja i s'infla) i explota.
// Si el mates abans, també explota (i pot fer mal als altres enemics!).
[RequireComponent(typeof(EnemyPathfindingAI))]
public class EnemyKamikaze : MonoBehaviour
{
    public float triggerDistance = 1.3f;
    public float fuseTime = 0.7f;
    public float radius = 1.6f;
    public int playerDamage = 2;
    public int enemyDamage = 4;
    public float chaseSpeedMultiplier = 1.6f;

    private EnemyPathfindingAI ai;
    private EnemyHealth health;
    private SpriteRenderer sr;
    private bool lit;
    private bool exploded;
    private Vector3 baseScale;

    void Start()
    {
        ai = GetComponent<EnemyPathfindingAI>();
        health = GetComponent<EnemyHealth>();
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        ai.mode = EnemyPathfindingAI.Mode.Chase;
        if (health != null) health.Died += OnDied;
    }

    void OnDied(EnemyHealth h) => Explode();

    void Update()
    {
        if (lit || ai.Player == null) return;
        ai.speedMultiplier = ai.HasAggro ? chaseSpeedMultiplier : 1f;
        if (ai.HasAggro && Vector2.Distance(transform.position, ai.Player.position) < triggerDistance)
            StartCoroutine(Fuse());
    }

    System.Collections.IEnumerator Fuse()
    {
        lit = true;
        ai.speedMultiplier = 0.2f;
        float t = 0f;
        while (t < fuseTime)
        {
            float k = t / fuseTime;
            if (sr != null) sr.color = (Mathf.FloorToInt(t * Mathf.Lerp(8f, 24f, k)) % 2 == 0) ? Color.white : new Color(1f, 0.3f, 0.2f);
            transform.localScale = baseScale * (1f + 0.3f * k);
            t += Time.deltaTime;
            yield return null;
        }
        if (health != null && !health.IsDead) health.TakeDamage(9999); // Mor -> Died -> Explode
        else Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;
        Fx.Explode(transform.position, radius, playerDamage, enemyDamage, transform);
    }
}
