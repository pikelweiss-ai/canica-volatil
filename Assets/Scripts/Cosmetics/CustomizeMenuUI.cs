using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pantalla "TALLER DE CANIQUES" del menú principal (feta per codi, com el selector de mapes).
// Tries el color, el barret i el rastre amb les fletxes. Es guarda a PlayerPrefs al moment.
public class CustomizeMenuUI : MonoBehaviour
{
    private TMP_InputField nameInput;
    private TMP_FontAsset font;
    private CosmeticsDatabase db;
    private Image preview, hatPreview, trailPreview;
    private TextMeshProUGUI colorLabel, hatLabel, trailLabel, coinsLabel;

    public static CustomizeMenuUI Create(TMP_FontAsset font)
    {
        GameObject canvasGO = new GameObject("CustomizeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;

        CustomizeMenuUI ui = canvasGO.AddComponent<CustomizeMenuUI>();
        ui.font = font;
        ui.db = CosmeticsDatabase.Instance;
        ui.Build(canvasGO.transform);
        canvasGO.SetActive(false);
        return ui;
    }

    public void Show() { gameObject.SetActive(true); if(nameInput!=null)nameInput.text=PlayerPrefs.GetString("PlayerName","Usuario"); Refresh(); }
    public void Hide() { gameObject.SetActive(false); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (nameInput==null || !nameInput.isFocused)) Hide();
        // Previsualització del rastre de l'arc de Sant Martí
        if (db != null && db.trails != null && db.trails.Length > 0)
        {
            Color c = db.trails[CosmeticsDatabase.Get(CosmeticsDatabase.KeyTrail, db.trails.Length)].color;
            if (CosmeticsDatabase.IsRainbow(c)) trailPreview.color = Color.HSVToRGB(Time.time * 0.5f % 1f, 0.8f, 1f);
        }
    }

    void Build(Transform root)
    {
        Image dim = NewImage("Fons", root, new Color(0.06f, 0.06f, 0.09f, 0.97f));
        Stretch(dim.rectTransform);

        Place(NewText("Titol", root, "TALLER DE CANIQUES", 44).rectTransform, new Vector2(0f, 330f), new Vector2(1200f, 80f));
        coinsLabel = NewText("Monedes", root, "", 20);
        coinsLabel.color = new Color(1f, 0.82f, 0.25f);
        Place(coinsLabel.rectTransform, new Vector2(0f, 260f), new Vector2(1350f, 40f));

        if (db == null)
        {
            Place(NewText("Error", root, "Falta Resources/Cosmetics.asset\n(menú Canica > Masmorra > Crear-ho tot)", 30).rectTransform, Vector2.zero, new Vector2(1200f, 200f));
            AddBackButton(root);
            return;
        }

        Place(NewText("Nom",root,"Nom del jugador",22).rectTransform,new Vector2(-480,205),new Vector2(350,45));
        Image nameBox=NewImage("Nom del jugador",root,new Color(0.12f,0.22f,0.3f));
        Place(nameBox.rectTransform,new Vector2(-480,145),new Vector2(400,48));
        nameInput=nameBox.gameObject.AddComponent<TMP_InputField>();nameInput.targetGraphic=nameBox;
        var nameText=NewText("Text",nameBox.transform,"",24);Stretch(nameText.rectTransform);nameText.rectTransform.offsetMin=new Vector2(12,0);nameText.rectTransform.offsetMax=new Vector2(-12,0);
        nameInput.textViewport=nameBox.rectTransform;nameInput.textComponent=nameText;nameInput.characterLimit=24;nameInput.lineType=TMP_InputField.LineType.SingleLine;
        nameInput.text=PlayerPrefs.GetString("PlayerName","Usuario");
        nameInput.onEndEdit.AddListener(value=>{
            string clean=System.Text.RegularExpressions.Regex.Replace(value.Trim(),"<[^>]*>","");
            if(string.IsNullOrWhiteSpace(clean))clean="Usuario";
            PlayerPrefs.SetString("PlayerName",clean);PlayerPrefs.Save();nameInput.SetTextWithoutNotify(clean);
        });        // Previsualització: rastre (darrere), canica i barret
        trailPreview = NewImage("Rastre", root, Color.clear);
        Place(trailPreview.rectTransform, new Vector2(-130f, 70f), new Vector2(200f, 60f));

        preview = NewImage("Canica", root, Color.white);
        preview.sprite = db.playerSprite;
        preview.preserveAspect = true;
        Place(preview.rectTransform, new Vector2(0f, 70f), new Vector2(170f, 170f));

        hatPreview = NewImage("Barret", preview.transform, Color.white);
        hatPreview.preserveAspect = true;
        Place(hatPreview.rectTransform, new Vector2(0f, 125f), new Vector2(150f, 150f));

        colorLabel = Row(root, "Skin", -60f, d => Cycle(CosmeticsDatabase.KeySkin, db.skins != null ? db.skins.Length : 0, d));
        hatLabel = Row(root, "Barret", -140f, d => Cycle(CosmeticsDatabase.KeyHat, db.hats != null ? db.hats.Length : 0, d));
        trailLabel = Row(root, "Rastre", -220f, d => Cycle(CosmeticsDatabase.KeyTrail, db.trails.Length, d));

        Button random = NewButton("Sorpren", root, new Color(0.25f, 0.8f, 0.85f));
        Place((RectTransform)random.transform, new Vector2(480f, 70f), new Vector2(330f, 70f));
        TextMeshProUGUI randomText = NewText("Text", random.transform, "Sorprén-me!", 20);
        randomText.color = Color.black; Stretch(randomText.rectTransform);
        random.onClick.AddListener(() => {
            if (db.skins != null && db.skins.Length > 0) PlayerPrefs.SetInt(CosmeticsDatabase.KeySkin, Random.Range(0, db.skins.Length));
            if (db.hats != null && db.hats.Length > 0) PlayerPrefs.SetInt(CosmeticsDatabase.KeyHat, Random.Range(0, db.hats.Length));
            if (db.trails != null && db.trails.Length > 0) PlayerPrefs.SetInt(CosmeticsDatabase.KeyTrail, Random.Range(0, db.trails.Length));
            PlayerPrefs.Save(); Refresh();
        });        AddBackButton(root);
        Refresh();
    }

    void AddBackButton(Transform root)
    {
        Button back = NewButton("Fet", root, new Color(1f, 1f, 1f, 0.9f));
        Place((RectTransform)back.transform, new Vector2(0f, -330f), new Vector2(240f, 60f));
        TextMeshProUGUI t = NewText("Text", back.transform, "Fet", 30);
        t.color = Color.black;
        Stretch(t.rectTransform);
        back.onClick.AddListener(Hide);
    }

    TextMeshProUGUI Row(Transform root, string title, float y, System.Action<int> change)
    {
        Place(NewText("Nom " + title, root, title, 32).rectTransform, new Vector2(-330f, y), new Vector2(220f, 60f));

        Button left = NewButton("<", root, new Color(1f, 1f, 1f, 0.9f));
        Place((RectTransform)left.transform, new Vector2(-170f, y), new Vector2(60f, 60f));
        TextMeshProUGUI lt = NewText("T", left.transform, "<", 34); lt.color = Color.black; Stretch(lt.rectTransform);
        left.onClick.AddListener(() => change(-1));

        TextMeshProUGUI value = NewText("Valor", root, "", 32);
        Place(value.rectTransform, new Vector2(30f, y), new Vector2(320f, 60f));

        Button right = NewButton(">", root, new Color(1f, 1f, 1f, 0.9f));
        Place((RectTransform)right.transform, new Vector2(230f, y), new Vector2(60f, 60f));
        TextMeshProUGUI rt = NewText("T", right.transform, ">", 34); rt.color = Color.black; Stretch(rt.rectTransform);
        right.onClick.AddListener(() => change(1));

        return value;
    }

    void Cycle(string key, int count, int dir)
    {
        if (count <= 0) return;
        int v = (CosmeticsDatabase.Get(key, count) + dir + count) % count;
        PlayerPrefs.SetInt(key, v);
        PlayerPrefs.Save();
        Refresh();
    }

    void Refresh()
    {
        if (db == null || colorLabel == null) return;

        if (db.skins != null && db.skins.Length > 0)
        {
            var skin = db.skins[CosmeticsDatabase.Get(CosmeticsDatabase.KeySkin, db.skins.Length)];
            preview.sprite = skin.sprite != null ? skin.sprite : db.playerSprite;
            colorLabel.text = skin.name;
        }
        else colorLabel.text = "Original";

        // Monedes guardades de totes les partides
        if (coinsLabel != null) coinsLabel.text = "Monedes: " + CoinBank.Total + " · " + db.skins.Length + " skins · " + db.hats.Length + " barrets";

        if (db.hats != null && db.hats.Length > 0)
        {
            var h = db.hats[CosmeticsDatabase.Get(CosmeticsDatabase.KeyHat, db.hats.Length)];
            hatLabel.text = h.name;
            hatPreview.sprite = h.sprite;
            hatPreview.enabled = h.sprite != null;
            if(h.sprite!=null)
            {
                float bodySize=preview.rectTransform.sizeDelta.y;
                float size=bodySize*(h.fullHelmet?1.18f:1.05f);
                hatPreview.rectTransform.sizeDelta=Vector2.one*size;
                float lift=h.sprite.name.Contains("Aureola")?34f:0f;
                float top=(HatFit.Top(preview.sprite)-.5f)*bodySize;
                hatPreview.rectTransform.anchoredPosition=h.fullHelmet?Vector2.zero:new Vector2(0,top-4+lift-(HatFit.Bottom(h.sprite)-0.5f)*size);
            }
        }
        else { hatLabel.text = "Cap"; hatPreview.enabled = false; }

        var tr = db.trails[CosmeticsDatabase.Get(CosmeticsDatabase.KeyTrail, db.trails.Length)];
        trailLabel.text = tr.name;
        foreach(Transform child in trailPreview.transform)Destroy(child.gameObject);
        trailPreview.enabled=tr.shape==TrailShape.Ribbon;
        if(tr.shape!=TrailShape.Ribbon && tr.color.a>0)
            for(int i=0;i<7;i++)
            {
                var sample=NewImage("Mostra d'estela",trailPreview.transform,tr.color);
                sample.sprite=TrailParticleEmitter.SpriteFor(tr.shape);sample.preserveAspect=true;
                Place(sample.rectTransform,new Vector2(-80+i*25,Mathf.Sin(i*.9f)*12),Vector2.one*(tr.shape==TrailShape.Dots?12:23));
                sample.rectTransform.localRotation=Quaternion.Euler(0,0,tr.shape==TrailShape.Footsteps?90:i*12);
            }
        Color tc = tr.color; if (tc.a > 0f) tc.a = 0.6f;
        trailPreview.color = tc;
    }

    // ---------- Ajudes ----------

    Image NewImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    Button NewButton(string name, Transform parent, Color color)
    {
        Image img = NewImage(name, parent, color);
        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1f, 0.85f, 0.1f, 1f);
        cb.pressedColor = new Color(1f, 0.7f, 0f, 1f);
        b.colors = cb;
        return b;
    }

    TextMeshProUGUI NewText(string name, Transform parent, string text, float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
