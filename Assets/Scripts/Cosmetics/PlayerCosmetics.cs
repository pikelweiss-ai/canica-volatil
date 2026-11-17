using UnityEngine;

// Aplica la personalització al jugador en jugar: color de la canica, barret i rastre.
// S'afig sol al jugador (veure PlayerHazards.AttachToPlayer), no cal posar-lo a mà.
public class PlayerCosmetics : MonoBehaviour
{
    private SpriteRenderer body;
    private SpriteRenderer hat;
    private TrailRenderer trail;
    private bool rainbow;

    void Start()
    {
        CosmeticsDatabase db = CosmeticsDatabase.Instance;
        body = GetComponent<SpriteRenderer>();
        if (db == null || body == null) return;

        // Skin (si és l'original, el sprite està buit i no es canvia res)
        if (db.skins != null && db.skins.Length > 0)
        {
            Sprite skin = db.skins[CosmeticsDatabase.Get(CosmeticsDatabase.KeySkin, db.skins.Length)].sprite;
            if (skin != null) body.sprite = skin;
        }

        // Barret
        if (db.hats != null && db.hats.Length > 0)
        {
            Sprite s = db.hats[CosmeticsDatabase.Get(CosmeticsDatabase.KeyHat, db.hats.Length)].sprite;
            if (s != null) CreateHat(s);
        }

        // Rastre
        if (db.trails != null && db.trails.Length > 0)
        {
            Color c = db.trails[CosmeticsDatabase.Get(CosmeticsDatabase.KeyTrail, db.trails.Length)].color;
            if (c.a > 0f) CreateTrail(c);
        }
    }

    void CreateHat(Sprite s)
    {
        GameObject go = new GameObject("Barret");
        go.transform.SetParent(transform, false);
        hat = go.AddComponent<SpriteRenderer>();
        hat.sprite = s;
        hat.sortingLayerID = body.sortingLayerID;
        hat.sortingOrder = body.sortingOrder + 1;

        // El dibuix del barret està a baix del seu requadre: el posem perquè la vora de baix
        // quede un poc per davall de la part de dalt de la canica
        float scaleWorld = 0.85f;
        float parentScale = Mathf.Max(0.01f, transform.lossyScale.y);
        go.transform.localScale = Vector3.one * (scaleWorld / parentScale);
        float bodyTop = body.sprite != null ? body.sprite.bounds.max.y : 0.5f;          // Local
        float hatHalf = s.bounds.extents.y * scaleWorld / parentScale;                 // Local
        go.transform.localPosition = new Vector3(0f, bodyTop - 0.15f / parentScale + hatHalf, 0f);

        // El nom puja un poc perquè no tape el barret
        PlayerNameTag tag = GetComponent<PlayerNameTag>();
        if (tag != null && tag.playerNameText != null)
            tag.playerNameText.transform.localPosition += Vector3.up * (0.3f / parentScale);
    }

    void CreateTrail(Color c)
    {
        rainbow = CosmeticsDatabase.IsRainbow(c);
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.25f;
        trail.startWidth = 0.35f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        Shader sh = Shader.Find("Sprites/Default");
        if (sh != null) trail.material = new Material(sh);
        trail.sortingLayerID = body.sortingLayerID;
        trail.sortingOrder = body.sortingOrder - 1;
        SetTrailColor(c);
    }

    void SetTrailColor(Color c)
    {
        Color start = c; start.a = 0.7f;
        Color end = c; end.a = 0f;
        trail.startColor = start;
        trail.endColor = end;
    }

    void Update()
    {
        if (trail != null && rainbow)
            SetTrailColor(Color.HSVToRGB(Time.time * 0.5f % 1f, 0.8f, 1f));

        // Mentre el jugador parpelleja (invulnerable), el barret també
        if (hat != null && body != null) hat.enabled = body.enabled;
    }
}
