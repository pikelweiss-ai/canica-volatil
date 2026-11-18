using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MobileHelpUI : MonoBehaviour
{
    float previousScale;
    bool previousEditing;
    public static void Open()
    {
        if(Object.FindFirstObjectByType<MobileHelpUI>() != null) return;
        var go = new GameObject("Ajuda tactil",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 120;
        var s = go.GetComponent<CanvasScaler>(); s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; s.referenceResolution = new Vector2(1600,900); s.matchWidthOrHeight = .5f;
        var ui = go.AddComponent<MobileHelpUI>(); ui.previousScale = Time.timeScale; ui.previousEditing = GameControls.Editing;
        Time.timeScale = 0; GameControls.Editing = true; MobileInput.Clear();
        var bg = new GameObject("Fons",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>(); bg.SetParent(go.transform,false); bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one; bg.offsetMin = bg.offsetMax = Vector2.zero; bg.GetComponent<Image>().color = new Color(.025f,.055f,.09f,.98f);
        ui.Text(go.transform,"CANICA VOLÀTIL · MÒBIL",new Vector2(0,310),42);
        ui.Text(go.transform,"Joystick esquerre: mou-te.\nJoystick dret: apunta i ataca. Solta'l per parar.\nPots moure't i disparar alhora amb dos dits.\n\nToca una arma per equipar-la o una cura per usar-la.\nOBRIR: mantín el botó prop del cofre sense moure't.\nTIRAR ARMA: deixa l'arma seleccionada.\nMAPA: amplia el radar. Les tres barres obrin la pausa.",new Vector2(0,20),29);
        var b = new GameObject("Fet",typeof(RectTransform),typeof(Image),typeof(Button)).GetComponent<RectTransform>(); b.SetParent(go.transform,false); b.sizeDelta = new Vector2(500,80); b.anchoredPosition = new Vector2(0,-330); b.GetComponent<Image>().color = new Color(.12f,.35f,.45f); b.GetComponent<Button>().onClick.AddListener(()=>Destroy(go)); ui.Text(b,"Fet",Vector2.zero,30);
    }
    void Text(Transform parent,string value,Vector2 pos,float size)
    {
        var t = new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>(); t.transform.SetParent(parent,false); t.rectTransform.sizeDelta = new Vector2(1450,560); t.rectTransform.anchoredPosition = pos;
        var any = Object.FindFirstObjectByType<TextMeshProUGUI>(); if(any != null && any != t) t.font = any.font;
        t.text = value; t.fontSize = size; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
    }
    void Update() { if(Input.GetKeyDown(KeyCode.Escape)) Destroy(gameObject); }
    void OnDestroy() { Time.timeScale = previousScale; GameControls.Editing = previousEditing; MobileInput.Clear(); }
}
