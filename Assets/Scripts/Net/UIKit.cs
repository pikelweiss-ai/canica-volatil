using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Peces d'interficie per a les pantalles online, amb la paleta del menu Alpha.
// Tot es crea per codi (com la resta de menus del joc), en un Canvas de referencia 1600x900.
public static class UIKit
{
    public static readonly Color Ink = new Color(.035f, .065f, .10f);
    public static readonly Color Panel = new Color(.06f, .12f, .16f);
    public static readonly Color PanelLight = new Color(.09f, .17f, .22f);
    public static readonly Color Row = new Color(.08f, .15f, .20f);
    public static readonly Color RowAlt = new Color(.07f, .135f, .18f);
    public static readonly Color RowSelected = new Color(.20f, .34f, .40f);
    public static readonly Color ButtonColor = new Color(.11f, .21f, .27f);
    public static readonly Color Gold = new Color(.92f, .70f, .28f);
    public static readonly Color Green = new Color(.36f, .78f, .45f);
    public static readonly Color Red = new Color(.85f, .33f, .30f);
    public static readonly Color TextColor = new Color(.88f, .94f, .95f);
    public static readonly Color Muted = new Color(.55f, .72f, .74f);
    public static readonly Color Dim = new Color(.36f, .50f, .54f);
    public static readonly Color Shadow = new Color(.015f, .025f, .04f);

    public enum Style { Normal, Primary, Danger, Ghost, Success }

    public static GameObject Canvas(string name, int order)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = order;
        var sc = go.GetComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1600, 900); sc.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        EnsureEventSystem();
        return go;
    }

    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null || Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public static RectTransform Box(Transform parent, Vector2 pos, Vector2 size, Color color, string name = "Panell")
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform;
        r.anchorMin = r.anchorMax = Vector2.one * .5f; r.anchoredPosition = pos; r.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return r;
    }

    public static RectTransform Empty(Transform parent, string name = "Grup")
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform; Stretch(r);
        return r;
    }

    public static void Stretch(RectTransform r, float left = 0, float bottom = 0, float right = 0, float top = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.pivot = Vector2.one * .5f;
        r.offsetMin = new Vector2(left, bottom); r.offsetMax = new Vector2(-right, -top);
    }

    public static TextMeshProUGUI Text(Transform parent, string value, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions align = TextAlignmentOptions.Center, Color? color = null)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (GameTypography.Font != null) t.font = GameTypography.Font;
        t.text = value; t.fontSize = fontSize; t.alignment = align; t.color = color ?? TextColor; t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal; t.overflowMode = TextOverflowModes.Ellipsis;
        t.rectTransform.anchorMin = t.rectTransform.anchorMax = Vector2.one * .5f; t.rectTransform.anchoredPosition = pos; t.rectTransform.sizeDelta = size;
        return t;
    }

    // Text dins d'una fila: x des de l'esquerra, ample fix
    public static TextMeshProUGUI Cell(Transform row, string value, float x, float width, float fontSize, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, Color? color = null)
    {
        var t = Text(row, value, Vector2.zero, Vector2.zero, fontSize, align, color);
        var r = t.rectTransform; r.anchorMin = new Vector2(0, 0); r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, .5f);
        r.anchoredPosition = new Vector2(x, 0); r.sizeDelta = new Vector2(width, 0);
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    public class UIButton
    {
        public Button button; public Image image; public TextMeshProUGUI label; public RectTransform rect; public Style style;
        public void SetLabel(string s) { label.text = s; }
        public void SetInteractable(bool v) { button.interactable = v; label.color = v ? LabelColor(style) : Dim; }
        public void SetStyle(Style s) { style = s; image.color = Fill(s); label.color = button.interactable ? LabelColor(s) : Dim; }
    }

    static Color Fill(Style s) => s == Style.Primary ? Gold : s == Style.Danger ? new Color(.45f, .16f, .16f) : s == Style.Ghost ? new Color(.07f, .13f, .17f) : s == Style.Success ? new Color(.20f, .52f, .30f) : ButtonColor;
    static Color LabelColor(Style s) => s == Style.Primary ? Ink : TextColor;

    public static UIButton Button(Transform parent, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action, Style style = Style.Normal, float fontSize = 24, bool shadowed = true)
    {
        RectTransform shadow = null;
        if (shadowed) { shadow = Box(parent, pos + new Vector2(0, -4), size, Shadow, "Ombra"); shadow.GetComponent<Image>().raycastTarget = false; }
        var r = Box(parent, pos, size, Fill(style), "Boto " + label);
        var b = r.gameObject.AddComponent<Button>();
        var c = b.colors; c.highlightedColor = new Color(1, 1, .86f); c.pressedColor = new Color(.65f, .8f, .8f); c.selectedColor = Color.white; c.disabledColor = new Color(.55f, .6f, .62f, .55f); c.fadeDuration = .06f; b.colors = c;
        if (action != null) b.onClick.AddListener(action);
        var t = Text(r, label, Vector2.zero, size - new Vector2(16, 6), fontSize, TextAlignmentOptions.Center, LabelColor(style));
        t.textWrappingMode = TextWrappingModes.NoWrap;
        // L'ombra segueix el boto si s'amaga
        if (shadow != null) { var follow = r.gameObject.AddComponent<ShadowFollower>(); follow.shadow = shadow.gameObject; }
        return new UIButton { button = b, image = r.GetComponent<Image>(), label = t, rect = r, style = style };
    }


    public static TMP_InputField Input(Transform parent, Vector2 pos, Vector2 size, string placeholder, int limit, bool password = false, float fontSize = 24)
    {
        var r = Box(parent, pos, size, new Color(.025f, .05f, .075f), "Camp " + placeholder);
        var outline = r.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(.25f, .42f, .48f); outline.effectDistance = new Vector2(2, -2);
        var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(r, false);
        var ar = (RectTransform)area.transform; Stretch(ar, 14, 4, 14, 4);
        var ph = Text(ar, placeholder, Vector2.zero, Vector2.zero, fontSize, TextAlignmentOptions.MidlineLeft, Dim);
        Stretch(ph.rectTransform); ph.fontStyle = FontStyles.Italic; ph.textWrappingMode = TextWrappingModes.NoWrap;
        var txt = Text(ar, "", Vector2.zero, Vector2.zero, fontSize, TextAlignmentOptions.MidlineLeft, TextColor);
        Stretch(txt.rectTransform); txt.textWrappingMode = TextWrappingModes.NoWrap; txt.overflowMode = TextOverflowModes.Overflow; txt.richText = false;
        var field = r.gameObject.AddComponent<TMP_InputField>();
        field.textViewport = ar; field.textComponent = txt; field.placeholder = ph; field.targetGraphic = r.GetComponent<Image>();
        field.characterLimit = limit; field.richText = false;
        field.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        field.lineType = TMP_InputField.LineType.SingleLine;
        if (GameTypography.Font != null) field.fontAsset = GameTypography.Font;
        field.pointSize = fontSize;
        field.caretColor = Gold; field.customCaretColor = true; field.caretWidth = 3;
        field.selectionColor = new Color(.92f, .70f, .28f, .35f);
        // Mentre s'escriu, el joc no ha de moure el jugador ni obrir menus
        field.onSelect.AddListener(_ => TypingGuard.Begin(field));
        field.onDeselect.AddListener(_ => TypingGuard.End(field));
        return field;
    }

    public static RectTransform ScrollList(Transform parent, Vector2 pos, Vector2 size, out ScrollRect scroll, float spacing = 4)
    {
        var root = Box(parent, pos, size, new Color(.03f, .06f, .09f), "Llista");
        scroll = root.gameObject.AddComponent<ScrollRect>();
        var viewport = Box(root, Vector2.zero, Vector2.zero, new Color(0, 0, 0, .01f), "Viewport");
        Stretch(viewport, 6, 6, 6, 6); viewport.gameObject.AddComponent<RectMask2D>();
        var content = new GameObject("Contingut", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport, false);
        var cr = (RectTransform)content.transform;
        cr.anchorMin = new Vector2(0, 1); cr.anchorMax = new Vector2(1, 1); cr.pivot = new Vector2(.5f, 1); cr.offsetMin = cr.offsetMax = Vector2.zero;
        var layout = content.GetComponent<VerticalLayoutGroup>(); layout.spacing = spacing; layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = cr; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 35;
        return cr;
    }

    public static RectTransform ListRow(Transform content, float height, Color color)
    {
        var go = new GameObject("Fila", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        go.transform.SetParent(content, false);
        go.GetComponent<Image>().color = color;
        var le = go.GetComponent<LayoutElement>(); le.preferredHeight = height; le.minHeight = height;
        return (RectTransform)go.transform;
    }

    public static void Clear(Transform t) { for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject); }

    // Cadenat dibuixat amb dos rectangles (no depen de cap simbol de la font)
    public static void Lock(Transform parent, Vector2 pos, float scale, Color color)
    {
        // Arc: dos pals i un travesser
        Dot3(parent, pos + new Vector2(-4.5f * scale, 6 * scale), new Vector2(3 * scale, 10 * scale), color);
        Dot3(parent, pos + new Vector2(4.5f * scale, 6 * scale), new Vector2(3 * scale, 10 * scale), color);
        Dot3(parent, pos + new Vector2(0, 11 * scale), new Vector2(12 * scale, 3 * scale), color);
        var body = Box(parent, pos - new Vector2(0, 3 * scale), new Vector2(18 * scale, 13 * scale), color, "Cadenat cos"); body.GetComponent<Image>().raycastTarget = false;
        var hole = Box(body, Vector2.zero, new Vector2(3 * scale, 5 * scale), Ink); hole.GetComponent<Image>().raycastTarget = false;
    }

    static void Dot3(Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        var d = Box(parent, pos, size, color, "Cadenat arc"); d.GetComponent<Image>().raycastTarget = false;
    }

    public static void Dot(Transform parent, Vector2 pos, float size, Color color)
    {
        var d = Box(parent, pos, new Vector2(size, size), color, "Punt"); d.GetComponent<Image>().raycastTarget = false;
    }

    public static Sprite SkinSprite(int skin)
    {
        var db = CosmeticsDatabase.Instance;
        if (db == null) return null;
        if (db.skins != null && skin >= 0 && skin < db.skins.Length && db.skins[skin].sprite != null) return db.skins[skin].sprite;
        return db.playerSprite;
    }

    public static Image Picture(Transform parent, Sprite sprite, Vector2 pos, Vector2 size)
    {
        var r = Box(parent, pos, size, sprite != null ? Color.white : new Color(1, 1, 1, 0), "Imatge");
        var im = r.GetComponent<Image>(); im.sprite = sprite; im.preserveAspect = true; im.raycastTarget = false;
        return im;
    }

    // Posa l'element a l'esquerra/dreta d'una fila
    public static void AnchorLeft(RectTransform r, float x)
    {
        r.anchorMin = r.anchorMax = new Vector2(0, .5f); r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = new Vector2(x, 0);
    }
    public static void AnchorRight(RectTransform r, float x)
    {
        r.anchorMin = r.anchorMax = new Vector2(1, .5f); r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = new Vector2(-x, 0);
    }
}

// L'ombra dels botons es crea com a germana: la mostrem/amaguem i movem amb el boto.
public class ShadowFollower : MonoBehaviour
{
    public GameObject shadow;
    void OnEnable() { if (shadow != null) shadow.SetActive(true); }
    void OnDisable() { if (shadow != null) shadow.SetActive(false); }
    void OnDestroy() { if (shadow != null) Destroy(shadow); }
}

// Bloqueja els controls del joc mentre hi ha un camp de text seleccionat.
public static class TypingGuard
{
    static TMP_InputField active;
    static int endedFrame = -1;
    public static bool Typing => active != null && active.isActiveAndEnabled && active.isFocused;
    public static bool CapturedInput => Typing || endedFrame == Time.frameCount;
    public static void Begin(TMP_InputField f)
    {
        active = f;
    }
    public static void End(TMP_InputField f)
    {
        if (active != f) return;
        active = null;
        endedFrame = Time.frameCount;
    }
}
