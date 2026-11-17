using System.Collections;
using UnityEngine;

// Moneda que deixen els enemics i les caixes: salta, i quan estàs a prop vola cap a tu (imant).
public class CoinDrop : MonoBehaviour
{
    const float MagnetRange = 2.2f;

    private Transform player;
    private bool ready;
    private float speed;

    public static void Spawn(Vector3 pos)
    {
        SpriteRenderer sr = Fx.MakeSprite("Moneda", pos, Fx.PixelCircle(5), new Color(1f, 0.82f, 0.25f), "Player", 4);
        CoinDrop c = sr.gameObject.AddComponent<CoinDrop>();
        c.StartCoroutine(c.Pop(pos + (Vector3)(Random.insideUnitCircle * 0.8f)));
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
        float d = Vector2.Distance(transform.position, player.position);
        if (d > MagnetRange) return;

        speed = Mathf.Min(speed + 25f * Time.deltaTime, 14f);
        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        if (d < 0.25f)
        {
            if (CoinCounter.instance != null) CoinCounter.instance.AddCoin();
            else { Fx.Play(Fx.Coin, 0.4f); RunState.Coins++; CoinBank.Add(1); }
            Destroy(gameObject);
        }
    }
}
