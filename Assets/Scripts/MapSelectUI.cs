using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Informació d'un mapa que es pot triar al menú
[System.Serializable]
public class MapInfo
{
    public string displayName = "Mapa";  // Text de davall de la miniatura
    public string sceneName = "";        // Nom de l'escena (ha d'estar al Build Settings)
    public Sprite preview;               // Miniatura. Si està buida, es busca a Resources/MapPreviews/<sceneName>
}

// Pantalla "TRIA LA TEUA EXPEDICIÓ": targetes amb la miniatura del mapa i el nom a baix.
// Es crea per codi des de MainMenuController.
public class MapSelectUI : MonoBehaviour
{
    private const float CardWidth = 400f;
    private const float ImageHeight = 225f; // 16:9
    private const float LabelHeight = 145f;
    private const float Border = 6f;
    private const float Spacing = 40f;

    private TMP_FontAsset font;

    public static MapSelectUI Create(MapInfo[] maps, TMP_FontAsset font)
    {
        GameObject canvasGO = new GameObject("MapSelectCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Damunt del menú

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        MapSelectUI ui = canvasGO.AddComponent<MapSelectUI>();
        ui.font = font;
        ui.Build(canvasGO.transform, maps);
        canvasGO.SetActive(false);
        return ui;
    }

    public void Show() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

    void Build(Transform root, MapInfo[] maps)
    {
        // Fons fosc que tapa el menú (i bloqueja els clics de darrere)
        Image dim = CreateImage("Fons", root, new Color(0.025f, 0.04f, 0.075f, 0.99f));
        Stretch(dim.rectTransform);

        TextMeshProUGUI title = CreateText("Titol", root, "TRIA LA TEUA EXPEDICIÓ", 38);
        Place(title.rectTransform, new Vector2(0f, 340f), new Vector2(1450f, 80f));

        var version = CreateText("Versio", root, "CANICA VOLÀTIL · ALPHA · COMPILACIÓ DE TREBALL", 22);
        Place(version.rectTransform, new Vector2(0, 280), new Vector2(1400, 45));
        int count = maps != null ? maps.Length : 0;
        float cardHeight = ImageHeight + LabelHeight + Border * 2f;
        float totalWidth = count * (CardWidth + Border * 2f) + Mathf.Max(0, count - 1) * Spacing;

        for (int i = 0; i < count; i++)
        {
            float x = -totalWidth / 2f + (CardWidth + Border * 2f) / 2f + i * (CardWidth + Border * 2f + Spacing);
            CreateCard(root, maps[i], new Vector2(x, 0f), cardHeight);
        }

                TextMeshProUGUI help = CreateText("Controls", root, "Configura les tecles des de Controls al menú\nRoda configurable · Dues tecles per acció · Esc per tornar", 23);
        Place(help.rectTransform, new Vector2(0f, -260f), new Vector2(1300f, 80f));
        // Botó per a tornar al menú
        Button back = CreateButton("Tornar", root, new Color(1f, 1f, 1f, 0.9f));
        Place((RectTransform)back.transform, new Vector2(0f, -370f), new Vector2(240f, 60f));
        TextMeshProUGUI backText = CreateText("Text", back.transform, "Tornar", 30);
        backText.color = Color.black;
        Stretch(backText.rectTransform);
        back.onClick.AddListener(Hide);
    }

    void CreateCard(Transform root, MapInfo map, Vector2 position, float cardHeight)
    {
        // Marc de la targeta (és el botó: canvia de color en passar el ratolí)
        Button card = CreateButton("Mapa " + map.displayName, root, Color.white);
        Place((RectTransform)card.transform, position, new Vector2(CardWidth + Border * 2f, cardHeight));

        ColorBlock colors = card.colors;
        colors.normalColor = new Color(0.12f, 0.19f, 0.27f, 1f);
        colors.highlightedColor = new Color(1f, 0.85f, 0.1f, 1f);
        colors.pressedColor = new Color(1f, 0.7f, 0f, 1f);
        colors.selectedColor = colors.normalColor;
        card.colors = colors;

        string sceneName = map.sceneName;
                bool available = sceneName != "Lobby" && Application.CanStreamedLevelBeLoaded(sceneName);
        card.interactable = available;
        if (available) card.onClick.AddListener(() => { RunState.Reset(); SceneManager.LoadScene(sceneName); });
        string description = sceneName == "Dungeon" ? "SOLO · ROGUELITE\nTresors, armes i bosses" :
            sceneName == "Lobby" ? "ONLINE · PRÒXIMAMENT\nSales amb codi" : "PRÀCTICA OFFLINE\nMoviment i punteria";
        TextMeshProUGUI detail = CreateText("Descripcio", card.transform, description, 18);
        Place(detail.rectTransform, new Vector2(0f, -82f), new Vector2(380f, 74f));
        // Miniatura
        Sprite preview = map.preview;
        if (preview == null && !string.IsNullOrEmpty(map.sceneName))
            preview = Resources.Load<Sprite>("MapPreviews/" + map.sceneName);

        Image image = CreateImage("Miniatura", card.transform, preview != null ? Color.white : new Color(0.15f, 0.2f, 0.15f, 1f));
        image.sprite = preview;
        image.raycastTarget = false;
        RectTransform irt = image.rectTransform;
        irt.anchorMin = new Vector2(0f, 1f);
        irt.anchorMax = new Vector2(1f, 1f);
        irt.pivot = new Vector2(0.5f, 1f);
        irt.offsetMin = new Vector2(Border, -Border - ImageHeight);
        irt.offsetMax = new Vector2(-Border, -Border);

        if (preview == null)
        {
            TextMeshProUGUI noPreview = CreateText("SenseMiniatura", image.transform, map.sceneName == "Dungeon" ? "8 TEMES · 3 BOSSES" : "CANICA VOLÀTIL", 22);
            noPreview.color = new Color(1f, 1f, 1f, 0.5f);
            Stretch(noPreview.rectTransform);
        }

        // Nom del mapa, a baix
        TextMeshProUGUI label = CreateText("Nom", card.transform, map.displayName, 25);
        label.color = Color.white;
        RectTransform lrt = label.rectTransform;
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 0f);
        lrt.pivot = new Vector2(0.5f, 0f);
        lrt.offsetMin = new Vector2(Border, Border);
        lrt.offsetMax = new Vector2(-Border, Border + 54f);
    }

    // ---------- Ajudes per a crear UI ----------

    Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    Button CreateButton(string name, Transform parent, Color color)
    {
        Image img = CreateImage(name, parent, color);
        Button button = img.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        return button;
    }

    TextMeshProUGUI CreateText(string name, Transform parent, string text, float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform rt, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }
}
