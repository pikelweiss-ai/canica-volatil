using UnityEngine;

// Enemic que CARREGA: s'acosta, es para, tremola (avís) i es llança en línia recta cap a on estaves.
// Si xoca contra una paret, es queda marejat un moment (moment bo per a pegar-li).
// Golem: lent i fort. Aranya: ràpida. Ratpenat: carregues curtes i seguides.
[RequireComponent(typeof(EnemyPathfindingAI))]
public class EnemyCharger : MonoBehaviour
{
    public float chargeRange = 6f;      // Des d'on comença a carregar
    public float telegraphTime = 0.5f;
    public float dashSpeed = 10f;
    public float dashDistance = 5f;
    public float cooldown = 2.5f;
    public float stunTime = 0.8f;       // Marejat si xoca amb una paret
    public int dashDamage = 1;

    private EnemyPathfindingAI ai;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float timer;
    private bool busy;

    void Start()
    {
        ai = GetComponent<EnemyPathfindingAI>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        timer = Random.Range(0.5f, cooldown);
    }

    void Update()
    {
        if (busy || ai.Player == null || !ai.HasAggro) return;
        timer -= Time.deltaTime;
        if (timer > 0f || !ai.SeesPlayer) return;
        if (Vector2.Distance(transform.position, ai.Player.position) > chargeRange) return;
        StartCoroutine(Charge());
    }

    System.Collections.IEnumerator Charge()
    {
        busy = true;
        ai.mode = EnemyPathfindingAI.Mode.Hold;

        // Avís: tremola i s'il·lumina
        Vector3 basePos = transform.position;
        for (float t = 0f; t < telegraphTime; t += Time.deltaTime)
        {
            if (sr != null) sr.color = Mathf.FloorToInt(t * 16f) % 2 == 0 ? Color.white : new Color(1f, 0.6f, 0.6f);
            yield return null;
        }
        if (sr != null) sr.color = Color.white;

        if (ai.Player != null)
        {
            Vector2 dir = ((Vector2)ai.Player.position - rb.position).normalized;
            ai.enabled = false; // Durant la carrega el moviment el porta este script
            float moved = 0f;
            bool hitWall = false;
            bool hitPlayer = false;
            while (moved < dashDistance)
            {
                yield return new WaitForFixedUpdate();
                float step = dashSpeed * Time.fixedDeltaTime;
                Vector2 next = rb.position + dir * step;
                if (!TileNav.CircleFree(next, ai.bodyRadius)) { hitWall = true; break; }
                rb.MovePosition(next);
                moved += step;

                if (!hitPlayer && Vector2.Distance(next, ai.Player.position) < ai.bodyRadius + 0.35f)
                {
                    hitPlayer = true;
                    NetCoop.DamagePlayer(ai.Player, dashDamage);
                }
            }
            ai.enabled = true;

            if (hitWall)
            {
                Fx.Shake(0.12f, 0.1f);
                Fx.Burst(transform.position, new Color(0.7f, 0.7f, 0.75f), 6, 2f);
                if (sr != null) sr.color = new Color(0.7f, 0.7f, 1f);
                yield return new WaitForSeconds(stunTime);
                if (sr != null) sr.color = Color.white;
            }
        }

        ai.mode = EnemyPathfindingAI.Mode.Chase;
        timer = cooldown;
        busy = false;
    }
}
