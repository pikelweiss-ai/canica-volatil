using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pantalla PERSONALITZAR (v1.4.1, estil Stumble Guys): categories a l'esquerra, pista de previsualització al centre
// i quadrícula de col·leccionables a la dreta. Les tries es guarden a PlayerPrefs amb les mateixes claus de sempre
// (Cos_Skin, Cos_Hat, Cos_Trail); res no canvia a CosmeticsDatabase.
public class CustomizeMenuUI : MonoBehaviour
{
    enum Category { Skins, Hats, Trails }

    TMP_FontAsset font;
    CosmeticsDatabase db;
    TMP_InputField nameInput;
    TextMeshProUGUI coinsLabel, gridCount, nameCard, qualityLabel;
    RectTransform track, grid;
    Image marble, hat;
    readonly Image[] tabIcons = new Image[3];
    UIKit.UIButton[] tabs = new UIKit.UIButton[3];
    UIKit.UIButton useButton;
    Category category = Category.Skins;
    int pSkin, pHat, pTrail; // previsualització (encara no aplicada)
    readonly List<Sprite> cropped = new List<Sprite>();

    public static CustomizeMenuUI Create(TMP_FontAsset font)
    {
        var canvasGO = UIKit.Canvas("CustomizeCanvas", 50);
        canvasGO.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        var ui = canvasGO.AddComponent<CustomizeMenuUI>();
        ui.font = font; ui.db = CosmeticsDatabase.Instance;
        ui.Build(canvasGO.transform);
        canvasGO.SetActive(false);
        return ui;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (nameInput != null) nameInput.text = PlayerPrefs.GetString("PlayerName", "Usuario");
        if (db == null) return;
        pSkin = CosmeticsDatabase.Get(CosmeticsDatabase.KeySkin, db.skins != null ? db.skins.Length : 0);
        pHat = CosmeticsDatabase.Get(CosmeticsDatabase.KeyHat, db.hats != null ? db.hats.Length : 0);
        pTrail = CosmeticsDatabase.Get(CosmeticsDatabase.KeyTrail, db.trails != null ? db.trails.Length : 0);
        category = Category.Skins; Refresh();
    }

    public void Hide() { gameObject.SetActive(false); }

    // ------------------------------------------------------------------ construcció

    void Build(Transform root)
    {
        var back = UIKit.Box(root, Vector2.zero, new Vector2(4000, 3000), new Color(.035f, .065f, .10f, 1f), "Fons");
        if (db == null)
        {
            UIKit.Text(root, "Falta Resources/Cosmetics.asset", Vector2.zero, new Vector2(1200, 200), 34);
            UIKit.Button(root, "TORNAR", new Vector2(0, -300), new Vector2(260, 64), Hide, UIKit.Style.Danger, 28);
            return;
        }

        // Dalt: TORNAR, títol i monedes
        UIKit.Button(root, "TORNAR", new Vector2(-640, 400), new Vector2(240, 62), Hide, UIKit.Style.Danger, 28);
        UIKit.Text(root, "PERSONALITZAR", new Vector2(0, 400), new Vector2(800, 70), 46, TextAlignmentOptions.Center, UIKit.Gold);
        coinsLabel = UIKit.Text(root, "", new Vector2(540, 400), new Vector2(380, 50), 28, TextAlignmentOptions.MidlineRight, UIKit.Gold);

        // Esquerra: tres categories senzilles (icona a dalt, nom a baix)
        string[] names = { "CANIQUES", "BARRETS", "ESTELES" };
        for (int i = 0; i < 3; i++)
        {
            int c = i;
            tabs[i] = UIKit.Button(root, names[i], new Vector2(-640, 250 - i * 160), new Vector2(170, 140), () => { category = (Category)c; Refresh(); }, UIKit.Style.Normal, 20, false);
            tabs[i].label.rectTransform.anchoredPosition = new Vector2(0, -48);
            tabIcons[i] = UIKit.Picture(tabs[i].rect, null, new Vector2(0, 22), new Vector2(72, 72));
        }
        UIKit.Text(root, "més aviat: colors, expressions", new Vector2(-640, -240), new Vector2(230, 70), 16, TextAlignmentOptions.Center, UIKit.Dim);
        UIKit.Text(root, "Nom del jugador", new Vector2(-640, -305), new Vector2(250, 30), 16, TextAlignmentOptions.Center, UIKit.Muted);
        BuildNameField(root);

        // Centre: pista amb la canica
        var trackFrame = UIKit.Box(root, new Vector2(-170, 70), new Vector2(560, 440), UIKit.PanelLight, "Vora pista");
        track = UIKit.Box(trackFrame, Vector2.zero, new Vector2(548, 428), new Color(.03f, .06f, .09f), "Pista");
        track.gameObject.AddComponent<RectMask2D>();
        for (int g = -5; g <= 5; g++)
        {   // quadrícula suau
            UIKit.Box(track, new Vector2(g * 50, 0), new Vector2(1, 428), new Color(1, 1, 1, .04f), "Linia");
            if (Mathf.Abs(g) <= 4) UIKit.Box(track, new Vector2(0, g * 50), new Vector2(548, 1), new Color(1, 1, 1, .04f), "Linia");
        }
        marble = UIKit.Picture(track, db.playerSprite, Vector2.zero, new Vector2(120, 120));
        hat = UIKit.Picture(marble.transform, null, Vector2.zero, new Vector2(120, 120));
        UIKit.Text(root, "WASD o arrossega el ratolí per moure la canica", new Vector2(-170, -165), new Vector2(560, 30), 16, TextAlignmentOptions.Center, UIKit.Dim);
        var card = UIKit.Box(root, new Vector2(-170, -250), new Vector2(480, 110), UIKit.Panel, "Targeta de nom");
        nameCard = UIKit.Text(card, "", new Vector2(0, 18), new Vector2(460, 50), 32, TextAlignmentOptions.Center, UIKit.TextColor);
        var band = UIKit.Box(card, new Vector2(0, -30), new Vector2(300, 32), new Color(.25f, .30f, .33f), "Banda de qualitat");
        qualityLabel = UIKit.Text(band, "Sense qualitat", Vector2.zero, new Vector2(290, 30), 18, TextAlignmentOptions.Center, UIKit.TextColor);

        // Dreta: quadrícula amb scroll i botó UTILITZAR
        gridCount = UIKit.Text(root, "", new Vector2(500, 330), new Vector2(500, 40), 22, TextAlignmentOptions.MidlineLeft, UIKit.Muted);
        BuildGrid(root);
        useButton = UIKit.Button(root, "UTILITZAR", new Vector2(500, -355), new Vector2(380, 64), UseSelected, UIKit.Style.Success, 30);
        var random = UIKit.Button(root, "SORPREN-ME!", new Vector2(-170, -355), new Vector2(260, 52), Surprise, UIKit.Style.Normal, 22);
        random.rect.name = "Boto sorpresa";
    }

    void BuildNameField(Transform root)
    {
        var box = UIKit.Box(root, new Vector2(-640, -350), new Vector2(250, 44), new Color(.12f, .22f, .30f), "Nom del jugador");
        nameInput = box.gameObject.AddComponent<TMP_InputField>();
        nameInput.targetGraphic = box.GetComponent<Image>();
        var text = UIKit.Text(box, "", Vector2.zero, new Vector2(230, 40), 22, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
        nameInput.textViewport = box; nameInput.textComponent = text; nameInput.characterLimit = 24; nameInput.lineType = TMP_InputField.LineType.SingleLine;
        nameInput.text = PlayerPrefs.GetString("PlayerName", "Usuario");
        nameInput.onEndEdit.AddListener(value =>
        {
            string clean = System.Text.RegularExpressions.Regex.Replace(value.Trim(), "<[^>]*>", "");
            if (string.IsNullOrWhiteSpace(clean)) clean = "Usuario";
            PlayerPrefs.SetString("PlayerName", clean); PlayerPrefs.Save(); nameInput.SetTextWithoutNotify(clean);
        });
    }

    void BuildGrid(Transform root)
    {
        var frame = UIKit.Box(root, new Vector2(500, 10), new Vector2(580, 590), new Color(.03f, .06f, .09f), "Quadricula");
        var scroll = frame.gameObject.AddComponent<ScrollRect>();
        var viewport = UIKit.Box(frame, Vector2.zero, Vector2.zero, new Color(0, 0, 0, .01f), "Viewport");
        UIKit.Stretch(viewport, 8, 8, 8, 8); viewport.gameObject.AddComponent<RectMask2D>();
        var content = new GameObject("Contingut", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport, false);
        grid = (RectTransform)content.transform;
        grid.anchorMin = new Vector2(0, 1); grid.anchorMax = new Vector2(1, 1); grid.pivot = new Vector2(.5f, 1); grid.offsetMin = grid.offsetMax = Vector2.zero;
        var layout = content.GetComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(130, 128); layout.spacing = new Vector2(8, 8); layout.padding = new RectOffset(4, 4, 4, 4);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 4;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = grid; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 35;
    }

    // ------------------------------------------------------------------ dades per categoria

    int Count => category == Category.Skins ? (db.skins != null ? db.skins.Length : 0) : category == Category.Hats ? (db.hats != null ? db.hats.Length : 0) : db.trails.Length;
    int Selected { get => category == Category.Skins ? pSkin : category == Category.Hats ? pHat : pTrail; set { if (category == Category.Skins) pSkin = value; else if (category == Category.Hats) pHat = value; else pTrail = value; } }
    string KeyOf(Category c) => c == Category.Skins ? CosmeticsDatabase.KeySkin : c == Category.Hats ? CosmeticsDatabase.KeyHat : CosmeticsDatabase.KeyTrail;
    int Applied(Category c) => CosmeticsDatabase.Get(KeyOf(c), c == Category.Skins ? db.skins.Length : c == Category.Hats ? db.hats.Length : db.trails.Length);
    string NameOf(int i) => category == Category.Skins ? (i == 0 ? "Original" : db.skins[i].name) : category == Category.Hats ? (string.IsNullOrEmpty(db.hats[i].name) ? "Cap" : db.hats[i].name) : db.trails[i].name;

    void UseSelected()
    {
        PlayerPrefs.SetInt(KeyOf(category), Selected); PlayerPrefs.Save(); Refresh();
    }

    void Surprise()
    {
        if (db.skins.Length > 0) pSkin = Random.Range(0, db.skins.Length);
        if (db.hats.Length > 0) pHat = Random.Range(0, db.hats.Length);
        if (db.trails.Length > 0) pTrail = Random.Range(0, db.trails.Length);
        PlayerPrefs.SetInt(CosmeticsDatabase.KeySkin, pSkin); PlayerPrefs.SetInt(CosmeticsDatabase.KeyHat, pHat); PlayerPrefs.SetInt(CosmeticsDatabase.KeyTrail, pTrail);
        PlayerPrefs.Save(); Refresh();
    }

    // ------------------------------------------------------------------ pintar

    // Retalla el sprite al rectangle visible (per als barrets: sense la canica, centrat)
    Sprite Crop(Sprite sprite)
    {
        if (sprite == null || !sprite.texture.isReadable) return sprite;
        var r = sprite.rect; int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
        for (int y = 0; y < (int)r.height; y++) for (int x = 0; x < (int)r.width; x++)
            if (sprite.texture.GetPixel((int)r.x + x, (int)r.y + y).a > .1f) { minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
        if (maxX < 0) return sprite;
        var made = Sprite.Create(sprite.texture, new Rect(r.x + minX, r.y + minY, maxX - minX + 1, maxY - minY + 1), new Vector2(.5f, .5f), sprite.pixelsPerUnit);
        cropped.Add(made); return made;
    }

    void Refresh()
    {
        if (db == null || grid == null) return;
        coinsLabel.text = "● " + CoinBank.Total;
        for (int i = 0; i < 3; i++) tabs[i].SetStyle(i == (int)category ? UIKit.Style.Primary : UIKit.Style.Normal);
        UIKit.Clear(grid); foreach (var s in cropped) if (s != null) Destroy(s); cropped.Clear();

        int n = Count;
        gridCount.text = "Tots " + n + "/" + n;
        for (int i = 0; i < n; i++) BuildCard(i);
        RefreshTabIcons();
        useButton.SetLabel(Selected == Applied(category) ? "EQUIPAT" : "UTILITZAR");
        useButton.SetInteractable(Selected != Applied(category));
        RefreshPreview();
    }

    // Icona de cada categoria: la peça que portes ara (canica, barret retallat, forma de l'estela)
    void RefreshTabIcons()
    {
        var skin = db.skins != null && db.skins.Length > 0 ? db.skins[Applied(Category.Skins)] : null;
        tabIcons[0].sprite = skin != null && skin.sprite != null ? skin.sprite : db.playerSprite; tabIcons[0].color = Color.white;
        var hatPiece = db.hats != null && db.hats.Length > 0 ? db.hats[Applied(Category.Hats)].sprite : null;
        tabIcons[1].sprite = hatPiece != null ? Crop(hatPiece) : null; tabIcons[1].color = hatPiece != null ? Color.white : new Color(1, 1, 1, 0);
        var trail = db.trails[Applied(Category.Trails)];
        tabIcons[2].sprite = trail.color.a > 0 ? TrailParticleEmitter.SpriteFor(trail.shape == TrailShape.Ribbon ? TrailShape.Dots : trail.shape) : null;
        tabIcons[2].color = trail.color.a > 0 ? (CosmeticsDatabase.IsRainbow(trail.color) ? Color.HSVToRGB(.6f, .8f, 1f) : trail.color) : new Color(1, 1, 1, 0);
    }

    void BuildCard(int i)
    {
        bool chosen = i == Selected;
        var go = new GameObject("Carta " + i, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(grid, false);
        go.GetComponent<Image>().color = chosen ? UIKit.Gold : new Color(.20f, .27f, .31f);
        int index = i;
        go.GetComponent<Button>().onClick.AddListener(() => { Selected = index; Refresh(); });
        var inner = UIKit.Box(go.transform, Vector2.zero, new Vector2(chosen ? 120 : 124, chosen ? 118 : 122), chosen ? UIKit.RowSelected : UIKit.Row, "Fons");
        inner.GetComponent<Image>().raycastTarget = false;

        if (category == Category.Skins)
            UIKit.Picture(inner, i == 0 ? db.playerSprite : db.skins[i].sprite, new Vector2(0, 14), new Vector2(78, 78));
        else if (category == Category.Hats)
        {
            var sprite = db.hats[i].sprite;
            if (sprite != null) UIKit.Picture(inner, Crop(sprite), new Vector2(0, 14), new Vector2(78, 78));
            else UIKit.Text(inner, "—", new Vector2(0, 14), new Vector2(78, 78), 44, TextAlignmentOptions.Center, UIKit.Dim);
        }
        else
        {
            var trail = db.trails[i];
            if (trail.color.a <= 0) UIKit.Text(inner, "—", new Vector2(0, 14), new Vector2(78, 78), 44, TextAlignmentOptions.Center, UIKit.Dim);
            else for (int k = 0; k < 5; k++)
            {   // ratlla xicoteta amb la forma REAL de l'estela
                var piece = UIKit.Picture(inner, trail.shape == TrailShape.Ribbon || trail.shape == TrailShape.Dots ? TrailParticleEmitter.SpriteFor(TrailShape.Dots) : TrailParticleEmitter.SpriteFor(trail.shape),
                    new Vector2(-40 + k * 20, 14 + Mathf.Sin(k * 1.1f) * 8), Vector2.one * (trail.shape == TrailShape.Ribbon ? 12 + k * 2 : 20));
                piece.color = CosmeticsDatabase.IsRainbow(trail.color) ? Color.HSVToRGB(k / 5f, .8f, 1f) : trail.color;
            }
        }
        var label = UIKit.Text(inner, NameOf(i), new Vector2(0, -46), new Vector2(116, 26), 15, TextAlignmentOptions.Center, UIKit.TextColor);
        label.enableAutoSizing = true; label.fontSizeMin = 9; label.fontSizeMax = 15; label.textWrappingMode = TextWrappingModes.NoWrap;
        if (i == Applied(category)) UIKit.Text(inner, "●", new Vector2(46, 44), new Vector2(26, 26), 20, TextAlignmentOptions.Center, UIKit.Green);
    }

    void RefreshPreview()
    {
        var skin = db.skins != null && db.skins.Length > 0 ? db.skins[pSkin] : null;
        marble.sprite = skin != null && skin.sprite != null ? skin.sprite : db.playerSprite;
        var h = db.hats != null && db.hats.Length > 0 ? db.hats[pHat] : null;
        hat.enabled = h != null && h.sprite != null;
        if (hat.enabled)
        {
            float bodySize = marble.rectTransform.sizeDelta.y, size = bodySize * (h.fullHelmet ? 1.18f : 1.05f);
            hat.sprite = h.sprite; hat.rectTransform.sizeDelta = Vector2.one * size;
            float lift = h.sprite.name.Contains("Aureola") ? 34f * bodySize / 170f : 0f;
            float top = (HatFit.Top(marble.sprite) - .5f) * bodySize;
            hat.rectTransform.anchoredPosition = h.fullHelmet ? Vector2.zero : new Vector2(0, top - 3 + lift - (HatFit.Bottom(h.sprite) - .5f) * size);
        }
        string current = category == Category.Skins ? NameOf(pSkin) : category == Category.Hats ? NameOf(pHat) : NameOf(pTrail);
        nameCard.text = current;
        qualityLabel.text = "Sense qualitat";
    }

    // ------------------------------------------------------------------ pista: canica i estela

    Vector2 pos, driftOrigin; float idleTime, emitClock, lastMove = -9f;
    Vector2 lastPos;

    void Update()
    {
        if (db == null) return;
        if (Input.GetKeyDown(KeyCode.Escape) && (nameInput == null || !nameInput.isFocused)) { Hide(); return; }
        if (track == null) return;

        // Moviment: WASD/fletxes o arrossegar dins de la pista; si no, un bucle suau perquè sempre es veja l'estela
        Vector2 input = Vector2.zero;
        if (nameInput == null || !nameInput.isFocused)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1; if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1; if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1;
        }
        bool dragging = false;
        if (Input.GetMouseButton(0) && RectTransformUtility.ScreenPointToLocalPointInRectangle(track, Input.mousePosition, null, out var local) && track.rect.Contains(local))
        { pos = Vector2.MoveTowards(pos, local, 520f * Time.unscaledDeltaTime); dragging = true; }
        if (input != Vector2.zero) pos += input.normalized * 300f * Time.unscaledDeltaTime;
        if (dragging || input != Vector2.zero) idleTime = 0f;
        else
        {
            idleTime += Time.unscaledDeltaTime;
            if (idleTime > 1.2f)
            {
                float t = (idleTime - 1.2f) * .9f;
                Vector2 target = new Vector2(Mathf.Sin(t) * 170f, Mathf.Sin(t * 2f) * 80f);
                pos = Vector2.MoveTowards(pos, target, 260f * Time.unscaledDeltaTime);
            }
        }
        var half = track.rect.size * .5f - Vector2.one * 70f;
        pos = new Vector2(Mathf.Clamp(pos.x, -half.x, half.x), Mathf.Clamp(pos.y, -half.y, half.y));
        marble.rectTransform.anchoredPosition = pos;

        // Estela de la previsualització (la triada, encara sense aplicar)
        Vector2 moved = pos - lastPos; lastPos = pos;
        var trail = db.trails[Mathf.Clamp(pTrail, 0, db.trails.Length - 1)];
        emitClock += Time.unscaledDeltaTime;
        if (trail.color.a > 0f && moved.sqrMagnitude > .05f && emitClock >= .045f)
        {
            emitClock = 0f;
            var piece = new GameObject("Estela", typeof(RectTransform), typeof(Image), typeof(TrackPiece));
            piece.transform.SetParent(track, false);
            var img = piece.GetComponent<Image>(); img.raycastTarget = false;
            img.sprite = TrailParticleEmitter.SpriteFor(trail.shape == TrailShape.Ribbon ? TrailShape.Dots : trail.shape);
            Color c = CosmeticsDatabase.IsRainbow(trail.color) ? Color.HSVToRGB(Time.unscaledTime * .5f % 1f, .8f, 1f) : trail.color; c.a = 1f;
            img.color = c;
            var rt = (RectTransform)piece.transform; rt.anchoredPosition = pos - moved.normalized * 16f; rt.sizeDelta = Vector2.one * (trail.shape == TrailShape.Ribbon ? 22 : 28);
            if (trail.shape == TrailShape.Footsteps) rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(moved.y, moved.x) * Mathf.Rad2Deg);
            piece.GetComponent<TrackPiece>().life = trail.shape == TrailShape.Ribbon ? .45f : .8f;
            marble.transform.SetAsLastSibling(); // l'estela queda darrere de la canica
        }
    }

    void OnDestroy() { foreach (var s in cropped) if (s != null) Destroy(s); }
}

// Un trosset d'estela de la pista: es va apagant i desapareix
public class TrackPiece : MonoBehaviour
{
    public float life = .6f; float age; Image image;
    void Awake() { image = GetComponent<Image>(); }
    void Update()
    {
        age += Time.unscaledDeltaTime;
        if (age >= life) { Destroy(gameObject); return; }
        var c = image.color; c.a = 1f - age / life; image.color = c;
        transform.localScale = Vector3.one * Mathf.Lerp(1f, .5f, age / life);
    }
}
