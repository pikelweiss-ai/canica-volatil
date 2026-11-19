using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MobileHUD : MonoBehaviour
{
    GameObject canvas;
    RectTransform safe;
    bool suspended;
    void Start()
    {
        canvas = new GameObject("Controls tactils", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = canvas.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 20;
        var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
        safe = new GameObject("Zona segura", typeof(RectTransform)).GetComponent<RectTransform>(); safe.SetParent(canvas.transform, false);
        Stick(false, new Vector2(170, 170));
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Lobby")
        {
            Stick(true, new Vector2(-170, 170));
            Action("OBRIR\n(mantín)", 5, new Vector2(-350, 350), true);
            Action("TIRAR\nARMA", 6, new Vector2(350, 90), false);
            Action("MAPA", 9, new Vector2(-350, 90), true);
        }
        RefreshSafeArea();
    }
    void RefreshSafeArea()
    {
        var r = Screen.safeArea;
        safe.anchorMin = new Vector2(r.xMin / Screen.width, r.yMin / Screen.height);
        safe.anchorMax = new Vector2(r.xMax / Screen.width, r.yMax / Screen.height);
        safe.offsetMin = safe.offsetMax = Vector2.zero;
    }
    void Update()
    {
        RefreshSafeArea();
        bool blocked = Time.timeScale == 0 || GameControls.Blocked;
        if (blocked && !suspended) CancelTouches();
        suspended = blocked;
    }
    void CancelTouches() { MobileInput.Clear(); if(canvas == null) return; foreach(var s in canvas.GetComponentsInChildren<MobileStick>()) s.Cancel(); foreach(var b in canvas.GetComponentsInChildren<MobileAction>()) b.Cancel(); }
    void OnApplicationFocus(bool focus) { if (!focus) { CancelTouches(); GetComponent<GameplayMenu>()?.PauseGame(); } }
    void OnApplicationPause(bool pause) { if(pause) OnApplicationFocus(false); }
    void OnDestroy() { MobileInput.Clear(); if (canvas != null) Destroy(canvas); }
    void OnDisable() { CancelTouches(); if(canvas != null) canvas.SetActive(false); }
    void OnEnable() { if(canvas != null) canvas.SetActive(true); }
    RectTransform Box(string name, Vector2 pos, Vector2 size, bool right, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image)); var r = (RectTransform)go.transform; r.SetParent(safe, false);
        r.anchorMin = r.anchorMax = new Vector2(right ? 1 : 0, 0); r.sizeDelta = size; r.anchoredPosition = pos;
        go.GetComponent<Image>().color = color; return r;
    }
    void Label(Transform parent, string text, float size)
    {
        var t = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>(); t.transform.SetParent(parent, false);
        t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one; t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
        var other = Object.FindFirstObjectByType<TextMeshProUGUI>(); if(other != null && other != t) t.font = other.font;
        t.text = text; t.fontSize = size; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
    }
    void Stick(bool aim, Vector2 pos)
    {
        var r = Box(aim ? "Apuntar i atacar" : "Moure", pos, Vector2.one * 230, aim, new Color(.08f,.16f,.22f,.38f));
        var image = r.GetComponent<Image>(); image.sprite = Disc();
        var knob = new GameObject("Palanca", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); knob.SetParent(r, false); knob.sizeDelta = Vector2.one * 82;
        knob.GetComponent<Image>().sprite = image.sprite; knob.GetComponent<Image>().color = new Color(.55f,.85f,1,.65f); knob.GetComponent<Image>().raycastTarget = false;
        var stick = r.gameObject.AddComponent<MobileStick>(); stick.aim = aim; stick.knob = knob;
        Label(r, aim ? "APUNTAR\nATACAR" : "MOURE", 19);
    }
    void Action(string label, int action, Vector2 pos, bool right)
    {
        var r = Box(label, pos, new Vector2(120,85), right, new Color(.06f,.14f,.22f,.75f));
        var b = r.gameObject.AddComponent<MobileAction>(); b.action = action; Label(r,label,21);
    }
    static Sprite disc;
    static Sprite Disc()
    {
        if(disc != null) return disc;
        var tex = new Texture2D(64,64,TextureFormat.RGBA32,false);
        for(int y=0;y<64;y++) for(int x=0;x<64;x++) tex.SetPixel(x,y, Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f)) < 31 ? Color.white : Color.clear);
        tex.Apply(); disc = Sprite.Create(tex,new Rect(0,0,64,64),Vector2.one*.5f); return disc;
    }
}

public class MobileStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool aim;
    public RectTransform knob;
    int pointer = int.MinValue;
    public void OnPointerDown(PointerEventData e) { if(pointer != int.MinValue || Time.timeScale == 0 || GameControls.Blocked) return; pointer = e.pointerId; OnDrag(e); }
    public void OnDrag(PointerEventData e)
    {
        if(e.pointerId != pointer) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var p);
        Vector2 v = Vector2.ClampMagnitude(p / 82,1); knob.anchoredPosition = v * 82;
        float magnitude = v.magnitude; v = magnitude < .16f ? Vector2.zero : v.normalized * Mathf.InverseLerp(.16f,1,magnitude);
        if(aim) { MobileInput.Firing = v.sqrMagnitude > 0; if(MobileInput.Firing) MobileInput.Aim = v.normalized; } else MobileInput.Movement = v;
    }
    public void OnPointerUp(PointerEventData e) { if(e.pointerId == pointer) Cancel(); }
    public void Cancel() { pointer = int.MinValue; if(knob != null) knob.anchoredPosition = Vector2.zero; if(aim) MobileInput.Firing = false; else MobileInput.Movement = Vector2.zero; }
    void OnDisable() { Cancel(); }
}

public class MobileAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int action;
    int pointer = int.MinValue;
    public void OnPointerDown(PointerEventData e) { if(pointer != int.MinValue || Time.timeScale == 0 || GameControls.Blocked) return; pointer = e.pointerId; MobileInput.Press(action); }
    public void OnPointerUp(PointerEventData e) { if(pointer == e.pointerId) { pointer = int.MinValue; MobileInput.Release(action); } }
    public void Cancel() { pointer = int.MinValue; MobileInput.Release(action); }
    void OnDisable() { Cancel(); }
}
