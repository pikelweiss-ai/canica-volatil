using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Dibuixa la hotbar a baix de la pantalla. La crea PlayerAttack per codi,
// així no cal muntar-la a mà al Canvas.
public class HotbarUI : MonoBehaviour
{
    private const float SlotSize = 90f;
    private const float Spacing = 10f;

    private static readonly Color SlotColor = new Color(0f, 0f, 0f, 0.45f);
    private static readonly Color BorderColor = new Color(1f, 1f, 1f, 0.6f);
    private static readonly Color SelectedBorderColor = new Color(1f, 0.85f, 0.1f, 1f);

    private const float BorderWidth = 4f;

    private Image[][] borders;
    private Image[] icons;
    private TextMeshProUGUI[] names;
    private TextMeshProUGUI currentWeaponText;
    private TMP_FontAsset font;

    public static HotbarUI Create(int slotCount, TMP_FontAsset font = null)
    {
        // Canvas propi per a la hotbar
        GameObject canvasGO = new GameObject("HotbarCanvas", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        HotbarUI ui = canvasGO.AddComponent<HotbarUI>();
        ui.font = font;
        ui.Build(canvasGO.transform, slotCount);
        return ui;
    }

    void Build(Transform parent, int slotCount)
    {
        borders = new Image[slotCount][];
        icons = new Image[slotCount];
        names = new TextMeshProUGUI[slotCount];

        float totalWidth = slotCount * SlotSize + (slotCount - 1) * Spacing;

        for (int i = 0; i < slotCount; i++)
        {
            // Fons de la casella
            GameObject slot = new GameObject("Slot " + (i + 1), typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(parent, false);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(SlotSize, SlotSize);
            rt.anchoredPosition = new Vector2(-totalWidth / 2f + i * (SlotSize + Spacing), 20f);
            slot.GetComponent<Image>().color = SlotColor;

            // Vora: 4 barretes (dalt, baix, esquerra, dreta)
            borders[i] = new Image[]
            {
                CreateBorderBar(slot.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -BorderWidth), Vector2.zero),
                CreateBorderBar(slot.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, BorderWidth)),
                CreateBorderBar(slot.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(BorderWidth, 0f)),
                CreateBorderBar(slot.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-BorderWidth, 0f), Vector2.zero),
            };

            // Icona de l'arma
            GameObject iconGO = new GameObject("Icona", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(slot.transform, false);
            RectTransform irt = iconGO.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.15f, 0.25f);
            irt.anchorMax = new Vector2(0.85f, 0.9f);
            irt.offsetMin = irt.offsetMax = Vector2.zero;
            Image icon = iconGO.GetComponent<Image>();
            icon.preserveAspect = true;
            icons[i] = icon;

            // Número de la tecla
            TextMeshProUGUI number = CreateText(slot.transform, (i + 1).ToString(), 22, TextAlignmentOptions.TopLeft);
            number.rectTransform.offsetMin = new Vector2(6f, 0f);
            number.rectTransform.offsetMax = new Vector2(0f, -4f);

            // Nom de l'arma
            TextMeshProUGUI weaponName = CreateText(slot.transform, "", 16, TextAlignmentOptions.Bottom);
            weaponName.rectTransform.offsetMin = new Vector2(2f, 4f);
            weaponName.rectTransform.offsetMax = new Vector2(-2f, 0f);
            names[i] = weaponName;
        }

        // Text damunt de la hotbar amb l'arma seleccionada
        currentWeaponText = CreateText(parent, "", 26, TextAlignmentOptions.Bottom);
        RectTransform crt = currentWeaponText.rectTransform;
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0f);
        crt.pivot = new Vector2(0.5f, 0f);
        crt.sizeDelta = new Vector2(400f, 40f);
        crt.anchoredPosition = new Vector2(0f, 20f + SlotSize + 10f);
    }

    Image CreateBorderBar(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject("Vora", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        Image img = go.GetComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    TextMeshProUGUI CreateText(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null)
            tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    // Actualitza la hotbar: quina casella està triada i què hi ha a cada una
    public void Refresh(WeaponData[] slots, int selected)
    {
        for (int i = 0; i < borders.Length; i++)
        {
            WeaponData weapon = (slots != null && i < slots.Length) ? slots[i] : null;

            Color borderColor = (i == selected) ? SelectedBorderColor : BorderColor;
            foreach (Image bar in borders[i])
                bar.color = borderColor;

            icons[i].enabled = weapon != null && weapon.icon != null;
            if (icons[i].enabled)
                icons[i].sprite = weapon.icon;

            names[i].text = weapon != null ? weapon.weaponName : "";
        }

        WeaponData current = (slots != null && selected < slots.Length) ? slots[selected] : null;
        currentWeaponText.text = current != null ? current.weaponName : "Puny";
    }
}
