using UnityEngine;

// Tirador: es queda a distància, es mou de costat i et dispara.
// Abans de disparar s'il·lumina (avís) perquè pugues esquivar.
[RequireComponent(typeof(EnemyPathfindingAI))]
public class EnemyShooter : MonoBehaviour
{
    public enum Pattern { Single, Spread3, Burst3, Ring8 }

    public Pattern pattern = Pattern.Single;
    public float fireCooldown = 1.8f;
    public float bulletSpeed = 6f;
    public int bulletDamage = 1;
    public float preferredDistance = 4.5f;
    public float telegraphTime = 0.35f;
    public bool useColor = false;              // Bales d'un color (p. ex. espores verdes)
    public Color bulletColor = Color.white;

    private EnemyPathfindingAI ai;
    private SpriteRenderer sr;
    private float timer;

    void Start()
    {
        ai = GetComponent<EnemyPathfindingAI>();
        ai.mode = EnemyPathfindingAI.Mode.KeepDistance;
        ai.preferredDistance = preferredDistance;
        sr = GetComponent<SpriteRenderer>();
        timer = Random.Range(0.8f, fireCooldown);
    }

    void Update()
    {
        if (ai.Player == null || !ai.HasAggro || !ai.SeesPlayer) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = fireCooldown;
        StartCoroutine(Shoot());
    }

    System.Collections.IEnumerator Shoot()
    {
        // Avís: s'il·lumina i es para
        ai.speedMultiplier = 0f;
        if (sr != null) sr.color = new Color(1f, 1f, 0.6f);
        yield return new WaitForSeconds(telegraphTime);
        if (sr != null) sr.color = Color.white;
        ai.speedMultiplier = 1f;
        if (ai.Player == null) yield break;

        Vector2 dir = (ai.Player.position - transform.position).normalized;
        switch (pattern)
        {
            case Pattern.Single:
                Fire(dir);
                break;
            case Pattern.Spread3:
                Fire(Rotate(dir, -15f)); Fire(dir); Fire(Rotate(dir, 15f));
                break;
            case Pattern.Burst3:
                for (int i = 0; i < 3; i++)
                {
                    if (ai.Player == null) yield break;
                    Fire((ai.Player.position - transform.position).normalized);
                    yield return new WaitForSeconds(0.15f);
                }
                break;
            case Pattern.Ring8: // Bolet: espores en totes direccions
                for (int i = 0; i < 8; i++)
                {
                    float a = (i * 45f + Random.Range(0f, 20f)) * Mathf.Deg2Rad;
                    Fire(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                }
                break;
        }
    }

    void Fire(Vector2 dir)
    {
        EnemyBullet.Fire((Vector2)transform.position + dir * 0.4f, dir, bulletSpeed, bulletDamage, transform, 1f,
                         useColor ? bulletColor : (Color?)null);
        GameAudio.Enemy(Fx.Pew, 0.35f);
    }

    public static Vector2 Rotate(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
