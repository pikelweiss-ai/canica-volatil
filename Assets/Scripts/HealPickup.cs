using System.Collections;
using UnityEngine;

// Item de cura al terra: flota amb ombra (com les armes) i et cura quan hi passes per damunt.
// Si tens la vida plena, no l'agafes (es queda per a després).
public class HealPickup : MonoBehaviour
{
    public HealItem item;
    public float pickupRange = 0.6f;

    private Transform visual;
    private Transform player;
    private PlayerHealth health;
    private bool ready;
    private float bob;

    public static HealPickup Spawn(HealItem item, Vector3 from, Vector3 to)
    {
        GameObject go = new GameObject("Cura " + item.itemName);
        go.transform.position = from;
        HealPickup p = go.AddComponent<HealPickup>();
        p.item = item;
        p.Build();
        p.StartCoroutine(p.PopOut(from, to));
        return p;
    }

    void Build()
    {
        bob = Random.Range(0f, 10f);

        GameObject shadow = new GameObject("Ombra");
        shadow.transform.SetParent(transform, false);
        SpriteRenderer ssr = shadow.AddComponent<SpriteRenderer>();
        ssr.sprite = ItemGraphics.RoundShadow;
        ssr.color = new Color(0f, 0f, 0f, 130f / 255f);
        ssr.sortingLayerName = "Shadows";

        GameObject v = new GameObject("Sprite");
        v.transform.SetParent(transform, false);
        SpriteRenderer sr = v.AddComponent<SpriteRenderer>();
        if (item.icon != null) sr.sprite = ItemGraphics.Outlined(item.icon, ItemGraphics.RarityColor(item.rarity));
        else { sr.sprite = Fx.PixelCircle(6); sr.color = new Color(1f, 0.3f, 0.4f); }
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 3;
        visual = v.transform;
        visual.localScale = Vector3.one * 0.75f;
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; health = p.GetComponent<PlayerHealth>(); }
    }

    IEnumerator PopOut(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (t < 0.4f)
        {
            float k = t / 0.4f;
            transform.position = Vector3.Lerp(from, to, k);
            visual.localPosition = Vector3.up * (0.3f + Mathf.Sin(k * Mathf.PI) * 0.6f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
        yield return new WaitForSeconds(0.2f);
        ready = true;
    }

    void Update()
    {
        if (!ready) return;
        visual.localPosition = Vector3.up * (0.3f + Mathf.Sin((Time.time + bob) * 3f) * 0.06f);

        if (player == null || health == null) return;
        if (Vector2.Distance(player.position, transform.position) > pickupRange) return;
        if (health.IsFull) return;

        health.Heal(item.healAmount);
        Fx.Play(Fx.Heal, 0.6f);
        Fx.Burst(transform.position, new Color(1f, 0.35f, 0.45f), 8, 2.5f);
        Destroy(gameObject);
    }
}
