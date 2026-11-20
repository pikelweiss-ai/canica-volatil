using UnityEngine;

// Portal al pis següent: un remolí morat que gira. Quan hi entres, passes de pis
// (amb les teues armes i la vida que tingues).
public class DungeonPortal : MonoBehaviour
{
    private Transform player;
    private SpriteRenderer outer, inner;
    private bool used;
    public Vector3? ReturnDestination { get; set; }
    private float appear;

    public static DungeonPortal Create(Vector3 pos, bool returning = false)
    {
        GameObject go = new GameObject(returning ? "Portal de tornada" : "Portal");
        go.transform.position = pos;
        DungeonPortal p = go.AddComponent<DungeonPortal>();
        Color outerColor = returning ? new Color(0.25f, 0.78f, 0.9f, 0.9f) : new Color(0.64f, 0.35f, 0.88f, 0.9f);
        Color innerColor = returning ? new Color(0.04f, 0.16f, 0.24f) : new Color(0.15f, 0.05f, 0.25f);
        p.outer = Child(go.transform, outerColor, 1.6f, 3);
        p.inner = Child(go.transform, innerColor, 1.0f, 4);
        Fx.Burst(pos, outerColor, 16, 4f);
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
            Fx.Burst((Vector2)transform.position + offset, outer != null ? outer.color : new Color(0.85f, 0.6f, 1f), 1, 0.5f);
        }

        if (used || player == null || !player.gameObject.activeInHierarchy || appear < 1f) return;
        if (Vector2.Distance(player.position, transform.position) < 0.6f)
        {
            Fx.Play(Fx.Heal, 0.7f);
            if (ReturnDestination.HasValue)
            {
                Vector3 destination = ReturnDestination.Value;
                var body = player.GetComponent<Rigidbody2D>();
                if (body != null) { body.position = destination; body.linearVelocity = Vector2.zero; }
                player.position = destination;
                return;
            }
            used = true;
            DungeonGenerator.Instance?.NextFloor();
        }
    }
}
