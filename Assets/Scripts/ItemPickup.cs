using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Item tirat al terra (per exemple, el loot d'un cofre):
// - Flota pujant i baixant, amb una ombra redona que es fa xicoteta quan puja
// - Té una vora d'1 píxel del color de la seua raresa
// - Si hi passes per damunt, va a la primera casella buida de la hotbar
public class ItemPickup : MonoBehaviour
{
    public WeaponData weapon;
    public float CooldownRemaining { get; private set; }
    public float pickupRange = 0.6f;
    public float floatHeight = 0.3f;   // Altura a la qual flota
    public float bobHeight = 0.06f;    // Quant puja i baixa
    public float bobSpeed = 3f;
    public float dropScale = 0.6f;     // Al terra es veu més xicotet que a la mà

    private bool waitForPlayerToLeave; // Si l'has deixat anar tu, no el tornes a agafar fins que t'allunyes

    private Transform itemVisual;
    private Transform shadow;
    private Vector3 shadowScale;
    private Transform player;
    private PlayerAttack playerAttack;
    private bool canPickup;
    private float bobOffset;

    // Crea l'item a 'from' i el fa saltar fins a 'to'
    public static ItemPickup Spawn(WeaponData weapon, Vector3 from, Vector3 to, bool droppedByPlayer = false, float cooldownRemaining = 0f)
    {
        if (DungeonGenerator.Instance != null) to = DungeonGenerator.Instance.KeepDropClearOfPortals(to);
        GameObject go = new GameObject("Item " + weapon.weaponName);
        go.transform.position = from;
        ItemPickup item = go.AddComponent<ItemPickup>();
        item.weapon = weapon;
        item.CooldownRemaining = cooldownRemaining;
        item.waitForPlayerToLeave = droppedByPlayer;
        item.Build();
        item.StartCoroutine(item.PopOut(from, to));
        NetShared.Spawned(item, NetShared.Kind.Weapon, weapon.name, from, to); // Online: tots el veuen
        return item;
    }

    void Build()
    {
        bobOffset = Random.Range(0f, 10f);

        // Ombra redona (al terra)
        GameObject shadowGo = new GameObject("Ombra");
        shadowGo.transform.SetParent(transform, false);
        SpriteRenderer ssr = shadowGo.AddComponent<SpriteRenderer>();
        ssr.sprite = ItemGraphics.RoundShadow;
        ssr.color = new Color(0f, 0f, 0f, 130f / 255f); // Igual que les altres ombres
        ssr.sortingLayerName = "Shadows";
        shadow = shadowGo.transform;
        shadowScale = shadow.localScale;

        // L'item, amb la vora del color de la raresa
        GameObject visualGo = new GameObject("Sprite");
        visualGo.transform.SetParent(transform, false);
        visualGo.transform.localPosition = Vector3.up * floatHeight;
        SpriteRenderer sr = visualGo.AddComponent<SpriteRenderer>();
        if (weapon.icon != null)
        {
            sr.sprite = ItemGraphics.Outlined(weapon.icon, ItemGraphics.RarityColor(weapon.rarity));
        }
        else
        {
            // Arma sense dibuix: una boleta del color del seu projectil (perquè no siga invisible)
            sr.sprite = Projectile.CircleSprite;
            sr.color = weapon.projectileColor;
            visualGo.transform.localScale = Vector3.one * 0.4f;
        }
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 3;
        itemVisual = visualGo.transform;
        if (weapon.icon != null)
            itemVisual.localScale = Vector3.one * dropScale;
    }

    bool FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerAttack = p.GetComponent<PlayerAttack>();
        }
        return player != null && player.gameObject.activeInHierarchy && playerAttack != null;
    }

    // Surt del cofre fent un arc
    IEnumerator PopOut(Vector3 from, Vector3 to)
    {
        const float duration = 0.45f;
        float t = 0f;
        while (t < duration)
        {
            float k = t / duration;
            transform.position = Vector3.Lerp(from, to, k);
            itemVisual.localPosition = Vector3.up * (floatHeight + Mathf.Sin(k * Mathf.PI) * 0.7f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = to;

        // Un moment abans de poder-lo agafar (que no se t'enganxe de colp)
        yield return new WaitForSeconds(0.25f);
        canPickup = true;
    }

    void Update()
    {
        if (!canPickup) return;

        // Flota: puja i baixa; l'ombra es fa xicoteta quan l'item puja
        float s = Mathf.Sin((Time.time + bobOffset) * bobSpeed);
        itemVisual.localPosition = Vector3.up * (floatHeight + s * bobHeight);
        shadow.localScale = shadowScale * (1f - 0.12f * (s + 1f) * 0.5f);

        if (player == null || !player.gameObject.activeInHierarchy || playerAttack == null)
            if (!FindPlayer()) return;
        float distance = Vector2.Distance(player.position, transform.position);

        if (waitForPlayerToLeave)
        {
            if (distance > pickupRange + 0.3f) waitForPlayerToLeave = false;
            return;
        }

        if (distance > pickupRange) return;

        // Online: el primer que l'agafa se'l queda (si tens lloc a la hotbar)
        if (!playerAttack.HasFreeSlot) return;
        if (NetShared.Claim(this, Collect)) return;
        Collect();
    }

    void Collect()
    {
        if (playerAttack != null && playerAttack.AddWeapon(weapon, CooldownRemaining))
        {
            ItemGraphics.PlayPickupSound(transform.position);
            Destroy(gameObject);
        }
    }
}

// Gràfics i so generats per codi per als items del terra
public static class ItemGraphics
{
    private static Sprite roundShadow;
    private static AudioClip pickupSound;
    private static readonly Dictionary<(Sprite, Color), Sprite> outlineCache = new Dictionary<(Sprite, Color), Sprite>();

    // Colors de raresa (de moment tot el que hi ha és comú = gris)
    public static Color RarityColor(ChestRarity rarity)
    {
        switch (rarity)
        {
            case ChestRarity.PocComu:    return new Color(0.32f, 0.78f, 0.36f);
            case ChestRarity.Rar:        return new Color(0.29f, 0.50f, 0.88f);
            case ChestRarity.Epic:       return new Color(0.64f, 0.35f, 0.88f);
            case ChestRarity.Llegendari: return new Color(0.96f, 0.77f, 0.26f);
            default:                     return new Color(0.75f, 0.75f, 0.78f); // Comú: gris
        }
    }

    // Còpia de l'sprite amb una vora d'1 píxel del color donat
    // (la textura ha de ser llegible: les de les carpetes Pixel/ ho són)
    public static Sprite Outlined(Sprite source, Color color)
    {
        if (outlineCache.TryGetValue((source, color), out Sprite cached) && cached != null)
            return cached;

        Texture2D tex = source.texture;
        if (!tex.isReadable)
        {
            Debug.LogWarning("[Items] La textura de " + source.name + " no és llegible: no puc fer-li la vora.");
            return source;
        }

        Rect r = source.textureRect;
        int w = (int)r.width, h = (int)r.height;
        Color[] src = tex.GetPixels((int)r.x, (int)r.y, w, h);

        int W = w + 2, H = h + 2;
        Color[] copy = new Color[W * H];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                copy[(y + 1) * W + (x + 1)] = src[y * w + x];

        Color[] result = (Color[])copy.Clone();
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                if (copy[y * W + x].a >= 0.5f) continue;
                bool nextToItem =
                    (x > 0 && copy[y * W + x - 1].a >= 0.5f) ||
                    (x < W - 1 && copy[y * W + x + 1].a >= 0.5f) ||
                    (y > 0 && copy[(y - 1) * W + x].a >= 0.5f) ||
                    (y < H - 1 && copy[(y + 1) * W + x].a >= 0.5f);
                if (nextToItem) result[y * W + x] = color;
            }
        }

        Texture2D outTex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        outTex.filterMode = FilterMode.Point;
        outTex.wrapMode = TextureWrapMode.Clamp;
        outTex.SetPixels(result);
        outTex.Apply();

        Sprite sprite = Sprite.Create(outTex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
        outlineCache[(source, color)] = sprite;
        return sprite;
    }

    // Ombra redona de 14x6 píxels (a 16 píxels per unitat)
    public static Sprite RoundShadow
    {
        get
        {
            if (roundShadow == null)
            {
                const int w = 14, h = 6;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                float cx = w / 2f, cy = h / 2f;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float dx = (x + 0.5f - cx) / cx, dy = (y + 0.5f - cy) / cy;
                        tex.SetPixel(x, y, dx * dx + dy * dy <= 1f ? Color.white : Color.clear);
                    }
                }
                tex.Apply();
                roundShadow = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
            }
            return roundShadow;
        }
    }

    // "Bip" curt pujant en agafar un item
    public static void PlayPickupSound(Vector3 position)
    {
        if (pickupSound == null)
        {
            const int sampleRate = 44100;
            int n = (int)(sampleRate * 0.12f);
            float[] data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(600f, 1400f, t / 0.12f);
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.3f + Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;
                data[i] = wave * Mathf.Exp(-t * 20f) * 0.5f;
            }
            pickupSound = AudioClip.Create("ItemPickup", n, 1, sampleRate, false);
            pickupSound.SetData(data, 0);
        }

        Vector3 listenerPos = Camera.main != null ? Camera.main.transform.position : position;
        GameAudio.Play(pickupSound, 0.7f);
    }
}
