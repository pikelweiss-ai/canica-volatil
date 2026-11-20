using UnityEngine;

// Aplica la personalització al jugador en jugar: color de la canica, barret i rastre.
// S'afig sol al jugador (veure PlayerHazards.AttachToPlayer), no cal posar-lo a mà.
public class PlayerCosmetics : MonoBehaviour
{
    private SpriteRenderer body;
    private SpriteRenderer hat;
    internal const string TrailSortingLayer = "Shadows";
    private TrailRenderer trail;
    private bool rainbow;
    private RemoteAvatar remote;
    private Material trailMaterial;
    private Vector3 previousTrailPosition;

    // Online: per a un company, les seues tries en lloc de les d'aquest PC (-1 = les d'aquest PC)
    [System.NonSerialized] public int skinOverride = -1, hatOverride = -1, trailOverride = -1;
    int Pick(int over, string key, int count) => over >= 0 && over < count ? over : CosmeticsDatabase.Get(key, count);

    void Start()
    {
        CosmeticsDatabase db = CosmeticsDatabase.Instance;
        body = GetComponent<SpriteRenderer>();
        remote = GetComponent<RemoteAvatar>();
        if (db == null || body == null) return;

        // Skin (si és l'original, el sprite està buit i no es canvia res)
        if (db.skins != null && db.skins.Length > 0)
        {
            Sprite skin = db.skins[Pick(skinOverride, CosmeticsDatabase.KeySkin, db.skins.Length)].sprite;
            if (skin != null) body.sprite = skin;
        }

        // Barret
        if (db.hats != null && db.hats.Length > 0)
        {
            var definition = db.hats[Pick(hatOverride, CosmeticsDatabase.KeyHat, db.hats.Length)];
            if (definition.sprite != null) CreateHat(definition);
        }

        // Rastre
        if (db.trails != null && db.trails.Length > 0)
        {
            var selected = db.trails[Pick(trailOverride, CosmeticsDatabase.KeyTrail, db.trails.Length)];
            if (selected.color.a > 0f)
            {
                if(selected.shape==TrailShape.Ribbon)CreateTrail(selected.color);
                else gameObject.AddComponent<TrailParticleEmitter>().Init(selected.shape,selected.color);
            }
        }
    }

    void CreateHat(CosmeticsDatabase.Hat definition)
    {
        Sprite s=definition.sprite;
        GameObject go = new GameObject("Barret");
        go.transform.SetParent(transform, false);
        hat = go.AddComponent<SpriteRenderer>();
        hat.sprite = s;
        hat.sortingLayerID = body.sortingLayerID;
        hat.sortingOrder = body.sortingOrder + 1;

        float parentScale=Mathf.Max(0.01f,transform.lossyScale.y);
        float bodyWidth=body.sprite.bounds.size.x;
        float localScale=bodyWidth*(definition.fullHelmet?1.18f:1.05f)/s.bounds.size.x;
        go.transform.localScale=Vector3.one*localScale;
        float bottom=(HatFit.Bottom(s)-0.5f)*s.bounds.size.y;
        float lift=s.name.Contains("Aureola")?0.2f:0f;
        float bodyTop=(HatFit.Top(body.sprite)-0.5f)*body.sprite.bounds.size.y;
        go.transform.localPosition=definition.fullHelmet ? new Vector3(0,body.sprite.bounds.center.y,0) : new Vector3(0f,bodyTop-0.025f+lift-bottom*localScale,0f);
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
        if (sh != null)
        {
            trailMaterial = new Material(sh);
            trail.sharedMaterial = trailMaterial;
        }
        previousTrailPosition = transform.position;
        trail.sortingLayerName = TrailSortingLayer;
        trail.sortingOrder = 1;
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
        if (body != null)
        {
            body.sortingOrder = Mathf.Clamp(Mathf.RoundToInt(-transform.position.y * 8f), -30000, 30000);
            if (hat != null)
            {
                hat.sortingOrder = body.sortingOrder + 1;
                hat.enabled = body.enabled;
            }
        }
    }
    void LateUpdate()
    {
        if (trail == null) return;
        Vector3 position = transform.position;
        bool down = remote != null && remote.Down;
        if (down || (position - previousTrailPosition).sqrMagnitude >= 9f) trail.Clear();
        trail.emitting = !down;
        previousTrailPosition = position;
    }

    void OnDisable()
    {
        if (trail != null) trail.Clear();
        previousTrailPosition = transform.position;
    }

    void OnDestroy()
    {
        if (trailMaterial != null) Destroy(trailMaterial);
    }
}

// Mesura la base opaca; els marges transparents no compten com a barret.
public static class HatFit
{
    static readonly System.Collections.Generic.Dictionary<Sprite,float> tops=new System.Collections.Generic.Dictionary<Sprite,float>();
    public static float Top(Sprite sprite)
    {
        if (sprite == null || !sprite.texture.isReadable) return 1f;
        if(tops.TryGetValue(sprite,out float top))return top;Rect r=sprite.rect;
        for(int y=(int)r.height-1;y>=0;y--)for(int x=0;x<(int)r.width;x++)
            if(sprite.texture.GetPixel((int)r.x+x,(int)r.y+y).a>0.1f){top=(y+1)/r.height;tops[sprite]=top;return top;}
        return 1;
    }
    static readonly System.Collections.Generic.Dictionary<Sprite,float> cache=new System.Collections.Generic.Dictionary<Sprite,float>();
    public static float Bottom(Sprite sprite)
    {
        if (sprite == null || !sprite.texture.isReadable) return 0f;
        if(cache.TryGetValue(sprite,out float bottom))return bottom;
        Rect rect=sprite.rect;
        for(int y=0;y<(int)rect.height;y++) for(int x=0;x<(int)rect.width;x++)
            if(sprite.texture.GetPixel((int)rect.x+x,(int)rect.y+y).a>0.1f) {bottom=y/rect.height;cache[sprite]=bottom;return bottom;}
        return 0f;
    }
}
