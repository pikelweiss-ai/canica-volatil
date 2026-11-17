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

        float parentScale=Mathf.Max(0.01f,transform.lossyScale.y);
        float bodyWidth=body.sprite.bounds.size.x;
        float localScale=bodyWidth*1.05f/s.bounds.size.x;
        go.transform.localScale=Vector3.one*localScale;
        float bottom=(HatFit.Bottom(s)-0.5f)*s.bounds.size.y;
        float lift=s.name.Contains("Aureola")?0.2f:0f;
        go.transform.localPosition=new Vector3(0f,body.sprite.bounds.max.y-0.07f+lift-bottom*localScale,0f);
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

// Mesura la base opaca; els marges transparents no compten com a barret.
public static class HatFit
{
    static readonly System.Collections.Generic.Dictionary<Sprite,float> cache=new System.Collections.Generic.Dictionary<Sprite,float>();
    public static float Bottom(Sprite sprite)
    {
        if(cache.TryGetValue(sprite,out float bottom))return bottom;
        Rect rect=sprite.rect;
        for(int y=0;y<(int)rect.height;y++) for(int x=0;x<(int)rect.width;x++)
            if(sprite.texture.GetPixel((int)rect.x+x,(int)rect.y+y).a>0.1f) {bottom=y/rect.height;cache[sprite]=bottom;return bottom;}
        return 0f;
    }
}