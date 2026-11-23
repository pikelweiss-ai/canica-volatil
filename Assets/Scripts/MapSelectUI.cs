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
    private const float CardWidth = 326f;
    private const float CardHeight = 334f;
    private const float CardSpacing = 22f;
    private const float Frame = 4f;
    private const float CoverWidth = CardWidth - Frame * 2;
    private const float CoverHeight = CoverWidth * 9f / 16f;
    private const string Covers = "Pixel/Menu140/Covers/";

    private TMP_FontAsset font;
    TextMeshProUGUI creatorNotice;
    Transform importPanel;
    TMP_InputField importCodeField;
    TMP_Text importStatus;
    float noticeUntil;

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
        if (creatorNotice != null && Time.unscaledTime >= noticeUntil) creatorNotice.gameObject.SetActive(false);
        if (pendingDelete != null && Time.unscaledTime >= pendingDeleteUntil)
        {   // si no confirmes en 4 s, l'esborrat es desarma i la paperera torna al roig normal
            pendingDelete = null; savedMessage = "";
            if (savedPanel != null) OpenSavedMaps();
        }
    }

    void Build(MapInfo[] maps)
    {
        var shade = UIKit.Box(transform, Vector2.zero, Vector2.zero, new Color(.016f, .035f, .055f, .985f), "Fons");
        UIKit.Stretch(shade);
        Label(transform, "TRIA LA TEUA EXPEDICIÓ", new Vector2(0, 280), new Vector2(1360, 84),
            49, TextAlignmentOptions.Center, new Color(1f, .89f, .65f));

        var cards = new System.Collections.Generic.List<MapInfo>();
        if (maps != null)
            foreach (var map in maps)
                if (map != null) cards.Add(map);
        cards.Add(new MapInfo { displayName = "Creador", sceneName = "Creador" });
        float step = CardWidth + CardSpacing;
        float start = -(cards.Count - 1) * step * .5f;
        for (int i = 0; i < cards.Count; i++) CreateCard(cards[i], new Vector2(start + i * step, 12));

        creatorNotice = Label(transform, "El mode Creador arriba a la v1.6", new Vector2(0, -203),
            new Vector2(720, 44), 25, TextAlignmentOptions.Center, UIKit.Muted);
        creatorNotice.gameObject.name = "Avís Creador";
        creatorNotice.gameObject.SetActive(false);

        UIKit.Button(transform, "JUGAR MAPA GUARDAT", new Vector2(-185, -350), new Vector2(350, 48), OpenSavedMaps,
            UIKit.Style.Primary, 17);
        UIKit.Button(transform, "IMPORTAR CODI I JUGAR", new Vector2(185, -350), new Vector2(350, 48), OpenCodeImport,
            UIKit.Style.Success, 17);
        UIKit.Button(transform, "TORNAR", new Vector2(0, -272), new Vector2(224, 58), Hide,
            UIKit.Style.Normal, 22);
    }

    void CreateCard(MapInfo map, Vector2 position)
    {
        string scene = map.sceneName;
        bool coming = string.IsNullOrEmpty(scene);
        bool creator = scene == "Creador";
        var (title, details, action, accent) = scene switch
        {
            "Dungeon" => ("Masmorra", "Tresors, trampes i bosses.", "JUGAR EN SOLITARI", new Color(1f, .72f, .35f)),
            "GameScene" => ("Mapa de proves", "Practica el moviment i la punteria.", "PRACTICAR", new Color(.73f, .88f, .48f)),
            "Lobby" => ("Online", "Per codi, LAN o Internet.", "VEURE SALES", new Color(.43f, .89f, .87f)),
            "Creador" => ("CREADOR", "Dibuixa i guarda els teus mapes.", "OBRIR EDITOR", new Color(.62f, .82f, .56f)),
            _ => ("CREADOR", "", "PRÒXIMAMENT", new Color(.62f, .65f, .68f))
        };
        bool creatorMobile = creator && Application.isMobilePlatform;
        if (creatorMobile) details = "L'editor és només per a PC. Pots jugar codis de mapa ací.";

        var frame = UIKit.Box(transform, position, new Vector2(CardWidth, CardHeight), Color.white, "Mapa " + (coming ? "Creador" : scene));
        var card = frame.gameObject.AddComponent<Button>();
        card.targetGraphic = frame.GetComponent<Image>();
        var colors = card.colors;
        colors.normalColor = new Color(.22f, .31f, .38f);
        colors.highlightedColor = colors.selectedColor = accent;
        colors.pressedColor = Color.Lerp(accent, Color.black, .2f);
        card.colors = colors;
        card.interactable = coming || creator && !creatorMobile && Application.CanStreamedLevelBeLoaded(scene) || !creator && Application.CanStreamedLevelBeLoaded(scene);
        if (coming && !creator) card.onClick.AddListener(() =>
        {
            noticeUntil = Time.unscaledTime + 2.5f;
            creatorNotice.gameObject.SetActive(true);
        });
        else if (creator) card.onClick.AddListener(() => SceneManager.LoadScene("Creador"));
        else if (scene == "Lobby") card.onClick.AddListener(() => { Hide(); RoomBrowserUI.Open(); });
        else card.onClick.AddListener(() => { RunState.Reset(); SceneManager.LoadScene(scene); });

        var panel = UIKit.Box(frame, Vector2.zero, Vector2.zero, new Color(.05f, .105f, .15f), "Cos de la targeta");
        UIKit.Stretch(panel, Frame, Frame, Frame, Frame);
        panel.GetComponent<Image>().raycastTarget = false;

        var cover = map.preview != null ? map.preview : Resources.Load<Sprite>(Covers + scene);
        if (!coming && cover == null) cover = Resources.Load<Sprite>("MapPreviews/" + scene);
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

        if (coming || creator)
        {
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 7; x++)
                {
                    float tone = .12f + ((x * 7 + y * 11) % 4) * .033f;
                    var cell = UIKit.Box(window, new Vector2(-129 + x * 43, -63 + y * 42), new Vector2(40, 39),
                        new Color(tone, tone + .012f, tone + .025f), "Bloc");
                    cell.GetComponent<Image>().raycastTarget = false;
                }
            if (coming)
            {
                var ribbon = UIKit.Box(window, Vector2.zero, new Vector2(263, 41), new Color(.16f, .18f, .22f), "Cinta");
                ribbon.GetComponent<Image>().raycastTarget = false;
                Label(ribbon, action, Vector2.zero, new Vector2(255, 38), 25, TextAlignmentOptions.Center, accent);
            }
        }

        float coverBottom = coverY - CoverHeight * .5f;
        var divider = UIKit.Box(frame, new Vector2(0, coverBottom - 2), new Vector2(CoverWidth, 4), accent);
        divider.GetComponent<Image>().raycastTarget = false;
        const float textWidth = CardWidth - 52;
        var alignment = TextAlignmentOptions.MidlineLeft;
        Label(frame, title, new Vector2(0, coming ? -100 : -55), new Vector2(textWidth, 41), 28,
            coming ? TextAlignmentOptions.Center : alignment, coming ? accent : Color.white);
        if (!coming || creator)
        {
            Label(frame, details, new Vector2(0, -98), new Vector2(textWidth, 48), 18, alignment, UIKit.TextColor);
            Label(frame, creatorMobile ? "CREADOR NOMÉS A PC" : card.interactable ? action : "NO DISPONIBLE", new Vector2(0, -141), new Vector2(textWidth, 26), 17, alignment, accent);
        }
    }

    Transform savedPanel;
    string pendingDelete, savedMessage = "";
    float pendingDeleteUntil;
    static Sprite trashIcon;

    // Paperera: el primer clic només «arma» l'esborrat (4 s) i el segon l'esborra de debò. Un mapa esborrat no es pot recuperar.
    void TrashClicked(string savedName)
    {
        if (pendingDelete == savedName && Time.unscaledTime < pendingDeleteUntil)
        {
            bool deleted = MapStore.Delete(savedName);
            pendingDelete = null;
            savedMessage = deleted ? "Mapa «" + savedName + "» esborrat." : "No s'ha pogut esborrar «" + savedName + "».";
        }
        else
        {
            pendingDelete = savedName; pendingDeleteUntil = Time.unscaledTime + 4f;
            savedMessage = "Torna a clicar la paperera de «" + savedName + "» per esborrar-lo. No es pot desfer.";
        }
        OpenSavedMaps();
    }

    // Icona de paperera en píxel art (16×16), feta per codi.
    static Sprite TrashIcon()
    {
        if (trashIcon != null) return trashIcon;
        var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color32[16 * 16];
        void Put(int x, int y) { pixels[y * 16 + x] = new Color32(245, 245, 245, 255); }
        for (int x = 6; x <= 9; x++) Put(x, 14);                       // l'agafador
        Put(6, 13); Put(9, 13);
        for (int x = 2; x <= 13; x++) Put(x, 12);                      // la tapa
        for (int y = 2; y <= 11; y++) { Put(4, y); Put(11, y); }       // els costats
        for (int x = 4; x <= 11; x++) Put(x, 1);                       // el fons
        for (int y = 3; y <= 10; y++) { Put(6, y); Put(9, y); }        // les ratlles
        texture.SetPixels32(pixels);
        texture.Apply();
        trashIcon = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(.5f, .5f), 16f);
        return trashIcon;
    }

    // Llista els mapes guardats (els del Creador i els importats) perquè es puguen jugar sense passar per l'editor.
    void OpenSavedMaps()
    {
        if (savedPanel != null) Destroy(savedPanel.gameObject);
        savedPanel = UIKit.Box(transform, Vector2.zero, new Vector2(720, 620), new Color(.04f, .06f, .09f, .99f), "Llista mapes per jugar");
        UIKit.Text(savedPanel, "JUGAR UN MAPA GUARDAT", new Vector2(0, 270), new Vector2(620, 38), 24, TextAlignmentOptions.Center, new Color(1f, .89f, .65f));
        var names = MapStore.List();
        if (names.Length == 0)
            UIKit.Text(savedPanel, "Encara no hi ha mapes guardats. Crea'n un al Creador.", new Vector2(0, 0), new Vector2(620, 60), 18, TextAlignmentOptions.Center, UIKit.Muted);
        else
        {
            var content = UIKit.ScrollList(savedPanel, new Vector2(0, 10), new Vector2(660, 450), out _, 4);
            int i = 0;
            foreach (string savedName in names)
            {
                string captured = savedName;
                if (!MapStore.Load(captured, out var savedMap, out _)) continue;
                var row = UIKit.ListRow(content, 52, i++ % 2 == 0 ? UIKit.Row : UIKit.RowAlt);
                row.name = "Mapa per jugar " + captured;
                row.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaySavedMap(captured));
                var modified = MapStore.LastModifiedUtc(captured);
                string date = modified == System.DateTime.MinValue ? "data desconeguda" : modified.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                var label = UIKit.Text(row, captured + "  ·  " + savedMap.width + "×" + savedMap.height + "  ·  " + date,
                    new Vector2(-60, 0), new Vector2(450, 46), 15, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
                label.textWrappingMode = TextWrappingModes.NoWrap;
                UIKit.Button(row, "ONLINE", new Vector2(218, 0), new Vector2(94, 38), () => RoomBrowserUI.OpenCreateForMap(captured), UIKit.Style.Success, 14);
                bool armed = pendingDelete == captured && Time.unscaledTime < pendingDeleteUntil;
                var trash = UIKit.Box(row, new Vector2(307, 0), new Vector2(40, 38), armed ? new Color(.95f, .12f, .12f) : new Color(.50f, .12f, .12f), "Paperera " + captured);
                trash.gameObject.AddComponent<Button>().onClick.AddListener(() => TrashClicked(captured));
                UIKit.Picture(trash, TrashIcon(), Vector2.zero, new Vector2(28, 28)).raycastTarget = false;
            }
        }
        UIKit.Text(savedPanel, savedMessage, new Vector2(0, -238), new Vector2(680, 28), 15, TextAlignmentOptions.Center,
            pendingDelete != null ? new Color(1f, .89f, .65f) : UIKit.Muted);
        UIKit.Button(savedPanel, "TANCAR", new Vector2(0, -275), new Vector2(150, 42), () => { Destroy(savedPanel.gameObject); savedPanel = null; }, UIKit.Style.Normal, 15);
    }

    void PlaySavedMap(string savedName)
    {
        MapPlaytest.MapName = savedName;
        MapPlaytest.ReturnToEditor = false;
        MapPlaytest.DeleteMapAfterConsume = false; // és un mapa guardat de debò: NO s'esborra en jugar-lo
        MapPlaytest.StartInDungeon = true;
        RunStats.DontSave = true;
        RunState.Reset();
        SceneManager.LoadScene("Dungeon");
    }

    void OpenCodeImport()
    {
        if (importPanel != null) { importPanel.gameObject.SetActive(true); return; }
        importPanel = UIKit.Box(transform, Vector2.zero, new Vector2(900, 390), new Color(.035f, .065f, .10f, .99f), "Importar codi mapa");
        UIKit.Text(importPanel, "IMPORTAR UN MAPA PER CODI", new Vector2(0, 135), new Vector2(820, 42), 26,
            TextAlignmentOptions.Center, new Color(1f, .89f, .65f));
        UIKit.Text(importPanel, "Enganxa un codi CVM1 per jugar este mapa en este dispositiu.", new Vector2(0, 92), new Vector2(820, 36), 16,
            TextAlignmentOptions.Center, UIKit.TextColor);
        importCodeField = UIKit.Input(importPanel, new Vector2(0, 30), new Vector2(820, 48), "Codi CVM1", MapShare160.MaxCodeChars, fontSize: 13);
        importStatus = UIKit.Text(importPanel, "", new Vector2(0, -24), new Vector2(820, 35), 15, TextAlignmentOptions.Center, UIKit.Muted);
        UIKit.Button(importPanel, "JUGAR MAPA", new Vector2(-190, -90), new Vector2(220, 48), ImportAndPlayCode, UIKit.Style.Primary, 17);
        UIKit.Button(importPanel, "CANCEL·LAR", new Vector2(190, -90), new Vector2(220, 48), () => importPanel.gameObject.SetActive(false), UIKit.Style.Normal, 17);
    }

    void ImportAndPlayCode()
    {
        if (!MapImport160.SaveCodeForPlay(importCodeField.text, out var storedName, out var error))
        { importStatus.text = error; return; }
        MapPlaytest.MapName = storedName;
        MapPlaytest.ReturnToEditor = false;
        MapPlaytest.DeleteMapAfterConsume = true;
        MapPlaytest.StartInDungeon = true;
        RunStats.DontSave = true;
        RunState.Reset();
        SceneManager.LoadScene("Dungeon");
    }

    TextMeshProUGUI Label(Transform parent, string text, Vector2 position, Vector2 size, float fontSize,
        TextAlignmentOptions alignment, Color color)
    {
        var label = UIKit.Text(parent, text, position, size, fontSize, alignment, color);
        if (font != null) label.font = font;
        return label;
    }
}
