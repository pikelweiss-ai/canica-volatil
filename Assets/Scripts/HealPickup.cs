using System.Collections;
using UnityEngine;

// Item de cura al terra: flota amb ombra (com les armes). Quan hi passes per damunt, va a la bossa
// de cures (HealInventory) i el fas servir quan vols amb les tecles 5-8.
// Si la bossa està plena, es queda al terra.
public class HealPickup : MonoBehaviour
{
    public HealItem item;
    public float pickupRange = 0.6f;

    private Transform visual;
    private Transform player;
    private HealInventory bag;
    private bool ready;
    private bool waitForPlayerToLeave; // Si l'has tirat tu, no es torna a agafar fins que t'allunyes
    private float bob;

    public static HealPickup Spawn(HealItem item, Vector3 from, Vector3 to, bool droppedByPlayer = false)
    {
        GameObject go = new GameObject("Cura " + item.itemName);
        go.transform.position = from;
        HealPickup p = go.AddComponent<HealPickup>();
        p.item = item;
        p.waitForPlayerToLeave = droppedByPlayer;
        p.Build();
        p.StartCoroutine(p.PopOut(from, to));
        NetShared.Spawned(p, NetShared.Kind.Heal, item.name, from, to); // Online: tots el veuen
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
        if (p != null) player = p.transform;
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

        if (player == null) return;
        float dist = Vector2.Distance(player.position, transform.position);
        if (waitForPlayerToLeave) { if (dist > pickupRange + 0.3f) waitForPlayerToLeave = false; return; }
        if (dist > pickupRange) return;

        // Va a la bossa de cures (tecles 5-8). Si la bossa està plena, es queda al terra.
        if (bag == null) bag = player.GetComponent<HealInventory>();
        if (bag == null || !bag.CanAdd(item)) return;
        // Online: el primer que l'agafa se la queda
        if (NetShared.Claim(this, Collect)) return;
        Collect();
    }

    void Collect()
    {
        if (bag == null || !bag.Add(item)) return;
        ItemGraphics.PlayPickupSound(transform.position);
        Fx.Text(transform.position, item.itemName, Color.white, 3f);
        Destroy(gameObject);
    }
}
