using UnityEngine;

// Portal al pis següent: un remolí morat que gira. Quan hi entres, passes de pis
// (amb les teues armes i la vida que tingues).
public class DungeonPortal : MonoBehaviour
{
    private Transform player;
    private SpriteRenderer outer, inner;
    private bool used;
    private float appear;

    public static DungeonPortal Create(Vector3 pos)
    {
        GameObject go = new GameObject("Portal");
        go.transform.position = pos;
        DungeonPortal p = go.AddComponent<DungeonPortal>();
        p.outer = Child(go.transform, new Color(0.64f, 0.35f, 0.88f, 0.9f), 1.6f, 3);
        p.inner = Child(go.transform, new Color(0.15f, 0.05f, 0.25f, 1f), 1.0f, 4);
        Fx.Burst(pos, new Color(0.64f, 0.35f, 0.88f), 16, 4f);
        return p;
    }

    static SpriteRenderer Child(Transform parent, Color color, float size, int order)
    {
        SpriteRenderer sr = Fx.MakeSprite("Anell", parent.position, Fx.Disc, color, "Player", order);
        sr.transform.SetParent(parent, true);
        sr.transform.localScale = Vector3.one * size;
        return sr;
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        appear = Mathf.MoveTowards(appear, 1f, Time.deltaTime * 2f);
        float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.06f;
        transform.localScale = Vector3.one * appear * pulse;
        if (inner != null) inner.transform.localScale = Vector3.one * (0.9f + Mathf.Sin(Time.time * 6f) * 0.1f);

        // Espurnes que giren cap al centre
        if (Random.value < 0.3f)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * 0.9f;
            Fx.Burst((Vector2)transform.position + offset, new Color(0.85f, 0.6f, 1f), 1, 0.5f);
        }

        if (used || player == null || appear < 1f) return;
        if (Vector2.Distance(player.position, transform.position) < 0.6f)
        {
            used = true;
            Fx.Play(Fx.Heal, 0.7f);
            if (DungeonGenerator.Instance != null) DungeonGenerator.Instance.NextFloor();
        }
    }
}
