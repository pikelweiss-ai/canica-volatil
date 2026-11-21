using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class MapInfo
{
    public string displayName = "Mapa";
    public string sceneName = "";
    public Sprite preview;
}

public class MapSelectUI : MonoBehaviour
{
    private const float CardWidth = 432f;
    private const float CardHeight = 382f;
    private const float CardSpacing = 32f;
    private const float Frame = 4f;
    private const float CoverWidth = CardWidth - Frame * 2;
    private const float CoverHeight = CoverWidth * 9f / 16f;
    private const string Covers = "Pixel/Menu140/Covers/";

    private TMP_FontAsset font;

    public static MapSelectUI Create(MapInfo[] maps, TMP_FontAsset font)
    {
        var canvas = UIKit.Canvas("MapSelectCanvas", 50);
        var selector = canvas.AddComponent<MapSelectUI>();
        selector.font = font;
        selector.Build(maps);
        canvas.SetActive(false);
        return selector;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Hide();
    }

    void Build(MapInfo[] maps)
    {
        var shade = UIKit.Box(transform, Vector2.zero, Vector2.zero, new Color(.016f, .035f, .055f, .985f), "Fons");
        UIKit.Stretch(shade);
        Label(transform, "TRIA LA TEUA EXPEDICIÓ", new Vector2(0, 280), new Vector2(1360, 84),
            49, TextAlignmentOptions.Center, new Color(1f, .89f, .65f));

        if (maps != null)
        {
            float step = CardWidth + CardSpacing;
            float start = -(maps.Length - 1) * step * .5f;
            for (int i = 0; i < maps.Length; i++)
                if (maps[i] != null) CreateCard(maps[i], new Vector2(start + i * step, 12));
        }

        UIKit.Button(transform, "TORNAR", new Vector2(0, -272), new Vector2(224, 58), Hide,
            UIKit.Style.Normal, 22);
    }

    void CreateCard(MapInfo map, Vector2 position)
    {
        string scene = map.sceneName;
        var (title, details, action, accent) = scene switch
        {
            "Dungeon" => ("Masmorra", "Tresors, trampes i bosses.", "JUGAR EN SOLITARI", new Color(1f, .72f, .35f)),
            "GameScene" => ("Mapa de proves", "Practica el moviment i la punteria.", "PRACTICAR", new Color(.73f, .88f, .48f)),
            "Lobby" => ("Online", "Per codi, LAN o Internet.", "VEURE SALES", new Color(.43f, .89f, .87f)),
            _ => (map.displayName, "", "JUGAR", UIKit.Gold)
        };

        var frame = UIKit.Box(transform, position, new Vector2(CardWidth, CardHeight), Color.white, "Mapa " + scene);
        var card = frame.gameObject.AddComponent<Button>();
        card.targetGraphic = frame.GetComponent<Image>();
        var colors = card.colors;
        colors.normalColor = new Color(.22f, .31f, .38f);
        colors.highlightedColor = colors.selectedColor = accent;
        colors.pressedColor = Color.Lerp(accent, Color.black, .2f);
        card.colors = colors;
        card.interactable = Application.CanStreamedLevelBeLoaded(scene);
        if (scene == "Lobby") card.onClick.AddListener(() => { Hide(); RoomBrowserUI.Open(); });
        else card.onClick.AddListener(() => { RunState.Reset(); SceneManager.LoadScene(scene); });

        var panel = UIKit.Box(frame, Vector2.zero, Vector2.zero, new Color(.05f, .105f, .15f), "Cos de la targeta");
        UIKit.Stretch(panel, Frame, Frame, Frame, Frame);
        panel.GetComponent<Image>().raycastTarget = false;

        var cover = map.preview != null ? map.preview : Resources.Load<Sprite>(Covers + scene);
        if (cover == null) cover = Resources.Load<Sprite>("MapPreviews/" + scene);
        float coverY = (CardHeight - CoverHeight) * .5f - Frame;
        var window = UIKit.Box(frame, new Vector2(0, coverY), new Vector2(CoverWidth, CoverHeight),
            UIKit.Ink, "Portada");
        window.GetComponent<Image>().raycastTarget = false;
        window.gameObject.AddComponent<RectMask2D>();
        if (cover != null)
        {
            var picture = UIKit.Picture(window, cover, Vector2.zero, Vector2.zero);
            var fitter = picture.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectRatio = cover.rect.width / cover.rect.height;
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }

        float coverBottom = coverY - CoverHeight * .5f;
        var divider = UIKit.Box(frame, new Vector2(0, coverBottom - 2), new Vector2(CoverWidth, 4), accent);
        divider.GetComponent<Image>().raycastTarget = false;
        const float textWidth = CardWidth - 52;
        var alignment = TextAlignmentOptions.MidlineLeft;
        Label(frame, title, new Vector2(0, -79), new Vector2(textWidth, 42), 33, alignment, Color.white);
        Label(frame, details, new Vector2(0, -122), new Vector2(textWidth, 46), 21, alignment, UIKit.TextColor);
        Label(frame, card.interactable ? action : "NO DISPONIBLE", new Vector2(0, -164), new Vector2(textWidth, 26), 18, alignment, accent);
    }

    TextMeshProUGUI Label(Transform parent, string text, Vector2 position, Vector2 size, float fontSize,
        TextAlignmentOptions alignment, Color color)
    {
        var label = UIKit.Text(parent, text, position, size, fontSize, alignment, color);
        if (font != null) label.font = font;
        return label;
    }
}
