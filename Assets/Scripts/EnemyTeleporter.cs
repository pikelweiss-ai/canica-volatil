using UnityEngine;

// Mag: es manté lluny, i de tant en tant desapareix i reapareix en un altre lloc
// (a una distància de tu) i dispara un ventall de bales màgiques.
[RequireComponent(typeof(EnemyPathfindingAI))]
public class EnemyTeleporter : MonoBehaviour
{
    public float cooldown = 3.2f;
    public float minDistance = 3.5f;
    public float maxDistance = 6f;
    public int bullets = 5;
    public float spreadAngle = 50f;
    public float bulletSpeed = 5f;
    public Color bulletColor = new Color(0.75f, 0.55f, 1f);

    private EnemyPathfindingAI ai;
    private SpriteRenderer sr;
    private float timer;
    private bool busy;

    void Start()
    {
        ai = GetComponent<EnemyPathfindingAI>();
        ai.mode = EnemyPathfindingAI.Mode.KeepDistance;
        ai.preferredDistance = 5f;
        sr = GetComponent<SpriteRenderer>();
        timer = Random.Range(1f, cooldown);
    }

    void Update()
    {
        if (busy || ai.Player == null || !ai.HasAggro) return;
        timer -= Time.deltaTime;
        if (timer <= 0f) StartCoroutine(Blink());
    }

    System.Collections.IEnumerator Blink()
    {
        busy = true;
        ai.speedMultiplier = 0f;

        // Desapareix
        for (float t = 0f; t < 0.3f; t += Time.deltaTime)
        {
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f - t / 0.3f);
            yield return null;
        }
        Fx.Burst(transform.position, bulletColor, 8, 2f);

        // Busca un lloc lliure, a distància del jugador i des d'on el veja
        if (ai.Player != null && TileNav.Ground != null)
        {
            for (int i = 0; i < 25; i++)
            {
                Vector2 p = (Vector2)ai.Player.position + Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);
                if (!TileNav.CircleFree(p, ai.bodyRadius) || !TileNav.LineOfSight(p, ai.Player.position)) continue;
                Vector3Int cell = TileNav.Cell(p);
                DungeonGenerator.Room room = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.CurrentRoom : null;
                if (room != null && !room.rect.Contains(new Vector2Int(cell.x, cell.y)))
                    continue; // No eixir de la sala
                transform.position = p;
                break;
            }
        }

        // Reapareix i dispara
        for (float t = 0f; t < 0.3f; t += Time.deltaTime)
        {
            if (sr != null) sr.color = new Color(1f, 1f, 1f, t / 0.3f);
            yield return null;
        }
        if (sr != null) sr.color = Color.white;
        yield return new WaitForSeconds(0.25f);

        if (ai.Player != null)
        {
            Vector2 dir = (ai.Player.position - transform.position).normalized;
            for (int i = 0; i < bullets; i++)
            {
                float a = bullets == 1 ? 0f : Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, i / (float)(bullets - 1));
                Vector2 d = EnemyShooter.Rotate(dir, a);
                EnemyBullet.Fire((Vector2)transform.position + d * 0.4f, d, bulletSpeed, 1, transform, 1f, bulletColor);
            }
            Fx.Play(Fx.Pew, 0.4f);
        }

        ai.speedMultiplier = 1f;
        timer = cooldown;
        busy = false;
    }
}
