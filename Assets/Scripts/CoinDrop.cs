using System.Collections;
using UnityEngine;

// Moneda que deixen els enemics i les caixes: salta, i quan estàs a prop vola cap a tu (imant).
public class CoinDrop : MonoBehaviour
{
    const float MagnetRange = 2.2f;

    private Transform player;
    private bool ready;
    private float speed;
    private float hoverOnOther; // online: temps esperant damunt d'un company

    public static CoinDrop Spawn(Vector3 pos) => Spawn(pos, pos + (Vector3)(Random.insideUnitCircle * 0.8f));

    public static CoinDrop Spawn(Vector3 pos, Vector3 to)
    {
        if (DungeonGenerator.Instance != null) to = DungeonGenerator.Instance.KeepDropClearOfPortals(to);
        CoinDrop c = SpawnAt(pos, to);
        NetShared.Spawned(c, NetShared.Kind.Coin, "", pos, to); // Online: tots la veuen
        return c;
    }

    public static CoinDrop SpawnAt(Vector3 pos, Vector3 to)
    {
        SpriteRenderer sr = Fx.MakeSprite("Moneda", pos, Fx.PixelCircle(5), new Color(1f, 0.82f, 0.25f), "Player", 4);
        CoinDrop c = sr.gameObject.AddComponent<CoinDrop>();
        c.StartCoroutine(c.Pop(to));
        return c;
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    IEnumerator Pop(Vector3 to)
    {
        Vector3 from = transform.position;
        float t = 0f;
        while (t < 0.35f)
        {
            float k = t / 0.35f;
            transform.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.4f;
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
        ready = true;
    }

    void Update()
    {
        if (!ready || player == null) return;
        // Online: la moneda va cap al jugador MES PROXIM (tambe els companys); nomes l'agafa ell
        Transform target = player.gameObject.activeInHierarchy ? player : null;
        float d = target != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
        float mine = d;
        if (hoverOnOther < 1.2f) // si fa estona que espera damunt d'un company, ja la pot agafar qualsevol
            foreach (var av in NetShared.Avatars())
            {
                if (av == null || av.Down) continue;
                float da = Vector2.Distance(transform.position, av.transform.position);
                if (da + 0.5f < mine && da < d) { d = da; target = av.transform; } // el company ha d'estar clarament mes a prop
            }
        if (target != player && d < 0.45f) hoverOnOther += Time.deltaTime;
        if (target == null || d > RunAmulets.MagnetRange(MagnetRange)) return;

        speed = Mathf.Min(speed + 25f * Time.deltaTime, 14f);
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (target == player && d < 0.25f)
        {
            // Online: la moneda es del primer que l'agafa
            if (NetShared.Claim(this, Collect)) return;
            Collect();
        }
    }

    void Collect()
    {
        if (CoinCounter.instance != null) CoinCounter.instance.AddCoin();
        else { Fx.Play(Fx.Coin, 0.4f); RunState.Coins++; CoinBank.Add(1); RunStats.Coin(); }
        Destroy(gameObject);
    }
}
