using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MapEditor : MonoBehaviour
{
    enum EditTool { Paint, Erase, Fill, Rectangle, Line, Eyedropper, Copy, Paste, RotatePlaced, FlipPlaced }
    enum EditLayer { Ground, Objects, Overlay }
    enum PlaceMode { Blocks, Player, Enemy, Chest, Goal, Merchant }
    MapData map;
    BlockData selected;
    Transform grid, palette, uiRoot, mapListPanel;
    RectTransform palettePanel, paletteMoveHandle;
    TMP_InputField nameField, mapWidthField, mapHeightField, searchField, familyField;
    TMP_InputField sharePathField;
    TMP_Text status, enemyChoiceLabel, chestChoiceLabel, layerLabel;
    RectTransform helpPanel;
    GameObject[] enemyChoices = System.Array.Empty<GameObject>();
    int enemyChoiceIndex;
    ChestRarity[] chestChoices = { ChestRarity.Comu, ChestRarity.Rar, ChestRarity.Epic, ChestRarity.Llegendari };
    int chestChoiceIndex;
    int rotation;
    bool flip, eraseMode;
    bool filledRectangle;
    EditTool tool;
    EditLayer layer;
    PlaceMode placeMode;
    Vector2Int? dragStart;
    MapLayers clipboard;
    bool dirty;
    bool gridVisible = true, hitboxesVisible;
    int paletteMode;
    bool[] layerVisible = { true, true, true };
    float autosaveAt;
    string currentSavedName;
    bool mousePainting;
    Vector2Int? lastDragCell, suppressGridClick;
    Vector2 middlePanPosition;
    RectTransform promptPanel;
    TMP_Text promptText;
    System.Action pendingConfirm;
    sealed class Snapshot { public MapCell[] ground, objects, overlay; public MapSpawn[] spawns; public MapChest[] chests; public MapGoal goal; }
    struct MultipartPart { public BlockData block; public Vector2Int offset; }
    readonly Stack<Snapshot> undo = new Stack<Snapshot>();
    readonly Stack<Snapshot> redo = new Stack<Snapshot>();
    internal bool GridVisibleForTest => gridVisible;
    internal bool HitboxesVisibleForTest => hitboxesVisible;
    internal Vector2Int MapSizeForTest => new Vector2Int(map.width, map.height);
    internal RectTransform PaletteMoveHandleForTest => paletteMoveHandle;
    internal Vector2 PalettePositionForTest => palettePanel != null ? palettePanel.anchoredPosition : Vector2.zero;
    internal void MovePaletteForTest(Vector2 position) => SetPalettePosition(position);
    internal bool IsPointerOverMapGridForTest(Vector2 screenPosition) => IsPointOverMapGrid(screenPosition);
    internal Vector2 GridCenterScreenPositionForTest => RectTransformUtility.WorldToScreenPoint(null,
        grid.TransformPoint(new Vector3(-150 + (map.width - 1) * Mathf.Min(650f / map.width, 540f / map.height) * .5f,
            130 - (map.height - 1) * Mathf.Min(650f / map.width, 540f / map.height) * .5f, 0)));

    void Update()
    {
        bool typing = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
        if (!typing)
        {
            if (Input.GetKeyDown(KeyCode.R)) rotation = (rotation + 1) % 4;
            if (Input.GetKeyDown(KeyCode.F)) flip = !flip;
            if (Input.GetKeyDown(KeyCode.E)) eraseMode = !eraseMode;
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl)) Undo();
            if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftControl)) Redo();
            if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl)) Save();
            if (Input.GetKeyDown(KeyCode.F1)) ToggleHelp();
        }
        if (layerLabel != null)
        {
            string layerText = "CAPA: " + LayerName(EffectiveLayer()) + (eraseMode ? "  ·  ESBORRANT (E)" : "");
            if (layerLabel.text != layerText) layerLabel.text = layerText;
        }
        if (Time.unscaledTime >= autosaveAt)
        {
            autosaveAt = Time.unscaledTime + 60f;
            if (dirty) { MapStore.Save(map, "__autosave", out _); status.text = "Còpia automàtica guardada."; }
        }
        if (!typing && grid != null && EventSystem.current != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                bool overPaletteHandle = paletteMoveHandle != null && RectTransformUtility.RectangleContainsScreenPoint(paletteMoveHandle, Input.mousePosition, null);
                mousePainting = !overPaletteHandle;
                lastDragCell = null;
            }
            if (mousePainting && Input.GetMouseButton(0) && placeMode == PlaceMode.Blocks && (tool == EditTool.Paint || tool == EditTool.Erase))
            {
                var pointer = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                var hit = hits.FirstOrDefault(h => h.gameObject.name.StartsWith("Cel·la "));
                if (hit.gameObject != null)
                {
                    string[] coords = hit.gameObject.name.Substring("Cel·la ".Length).Split(',');
                    if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                    {
                        var next = new Vector2Int(x, y);
                        if (!lastDragCell.HasValue || lastDragCell.Value != next)
                        {
                            suppressGridClick = next;
                            foreach (var point in lastDragCell.HasValue ? TraceLine(lastDragCell.Value, next).Skip(1) : new[] { next }) Paint(point.x, point.y);
                            lastDragCell = next;
                        }
                    }
                }
            }
            if (Input.GetMouseButtonUp(0)) { mousePainting = false; lastDragCell = null; }
        }
        if (!typing && grid != null)
        {
            Vector3 pan = Vector3.zero;
            if (Input.GetKey(KeyCode.A)) pan.x += 5; if (Input.GetKey(KeyCode.D)) pan.x -= 5;
            if (Input.GetKey(KeyCode.W)) pan.y -= 5; if (Input.GetKey(KeyCode.S)) pan.y += 5;
            grid.localPosition += pan;
            float zoom = IsPointOverMapGrid(Input.mousePosition) ? Input.mouseScrollDelta.y : 0f;
            if (Mathf.Abs(zoom) > .01f) grid.localScale = Vector3.one * Mathf.Clamp(grid.localScale.x + zoom * .08f, .5f, 2f);
            if (Input.GetMouseButtonDown(2)) middlePanPosition = Input.mousePosition;
            if (Input.GetMouseButton(2))
            {
                Vector2 delta = (Vector2)Input.mousePosition - middlePanPosition;
                grid.localPosition += new Vector3(delta.x, delta.y, 0);
                middlePanPosition = Input.mousePosition;
            }
        }
    }

    void Start()
    {
        map = new MapData();
        if (MapPlaytest.ReturnToEditor && MapStore.Load(MapPlaytest.MapName, out var restored, out _)) { map = restored; currentSavedName = map.name; }
        MapPlaytest.ReturnToEditor = false;
        RunStats.DontSave = false;
        autosaveAt = Time.unscaledTime + 60f;
        var canvas = UIKit.Canvas("Creador UI", 20);
        uiRoot = canvas.transform;
        var contentCatalog = Resources.Load<NetContentCatalog>("NetContent");
        enemyChoices = contentCatalog != null && contentCatalog.enemies != null
            ? contentCatalog.enemies.Where(e => e != null && !(e.TryGetComponent<EnemyHealth>(out var h) && h.arenaPart)).ToArray()
            : System.Array.Empty<GameObject>();
        enemyChoiceIndex = System.Array.FindIndex(enemyChoices, e => e.name == "Llim");
        if (enemyChoiceIndex < 0) enemyChoiceIndex = 0;
        var background = UIKit.Box(canvas.transform, Vector2.zero, Vector2.zero, UIKit.Ink, "Fons");
        UIKit.Stretch(background);
        UIKit.Text(canvas.transform, "CREADOR DE MAPES", new Vector2(0, 420), new Vector2(900, 58), 34,
            TextAlignmentOptions.Center, UIKit.Gold);
        nameField = UIKit.Input(canvas.transform, new Vector2(-550, 360), new Vector2(260, 42), "Nom del mapa", 40, fontSize: 18);
        mapWidthField = UIKit.Input(canvas.transform, new Vector2(-386, 360), new Vector2(52, 42), "An.", 2, fontSize: 16);
        mapWidthField.text = map.width.ToString();
        UIKit.Text(canvas.transform, "×", new Vector2(-350, 360), new Vector2(18, 36), 20, TextAlignmentOptions.Center, UIKit.TextColor);
        mapHeightField = UIKit.Input(canvas.transform, new Vector2(-314, 360), new Vector2(52, 42), "Alt.", 2, fontSize: 16);
        mapHeightField.text = map.height.ToString();
        UIKit.Button(canvas.transform, "NOU MAPA", new Vector2(-220, 360), new Vector2(120, 42), ConfirmNewMap, UIKit.Style.Normal, 15);
        UIKit.Button(canvas.transform, "GUARDAR", new Vector2(-90, 360), new Vector2(130, 42), Save, UIKit.Style.Primary, 16);
        UIKit.Button(canvas.transform, "OBRIR", new Vector2(55, 360), new Vector2(110, 42), Load, UIKit.Style.Normal, 16);
        UIKit.Button(canvas.transform, "ROTAR", new Vector2(185, 360), new Vector2(110, 42), () => rotation = (rotation + 1) % 4, UIKit.Style.Normal, 16);
        UIKit.Button(canvas.transform, "PROVAR", new Vector2(315, 360), new Vector2(115, 42), Playtest, UIKit.Style.Primary, 16);
        UIKit.Button(canvas.transform, "COPIAR CODI", new Vector2(445, 360), new Vector2(135, 42), ExportCode, UIKit.Style.Normal, 14);
        UIKit.Button(canvas.transform, "IMPORTAR CODI", new Vector2(585, 360), new Vector2(140, 42), ImportCode, UIKit.Style.Normal, 13);
        UIKit.Button(canvas.transform, "EIXIR", new Vector2(735, 360), new Vector2(100, 42),
            ConfirmExit, UIKit.Style.Ghost, 16);
        var filePanel = UIKit.Box(canvas.transform, new Vector2(650, -80), new Vector2(270, 180), UIKit.Panel, "Panell fitxers");
        UIKit.Text(filePanel, "COMPARTIR FITXER", new Vector2(0, 65), new Vector2(240, 28), 16, TextAlignmentOptions.Center, UIKit.Gold);
        sharePathField = UIKit.Input(filePanel, new Vector2(0, 30), new Vector2(245, 34), "Ruta .canicamap", 240, fontSize: 12);
        UIKit.Button(filePanel, "EXPORTAR", new Vector2(-60, -18), new Vector2(110, 36), ExportFile, UIKit.Style.Normal, 13);
        UIKit.Button(filePanel, "IMPORTAR", new Vector2(60, -18), new Vector2(110, 36), ImportFile, UIKit.Style.Normal, 13);
        UIKit.Text(filePanel, "El fitxer és només dada; es valida en importar.", new Vector2(0, -60), new Vector2(250, 35), 11, TextAlignmentOptions.Center, UIKit.Muted);
        status = UIKit.Text(canvas.transform, "", new Vector2(0, -430), new Vector2(800, 30), 16, TextAlignmentOptions.Center, UIKit.Muted);
        UIKit.Button(canvas.transform, "ENEMIC −", new Vector2(-285, 230), new Vector2(105, 32), () => CycleEnemy(-1), UIKit.Style.Normal, 12);
        enemyChoiceLabel = UIKit.Text(canvas.transform, "", new Vector2(-155, 230), new Vector2(155, 32), 13, TextAlignmentOptions.Center, UIKit.TextColor);
        UIKit.Button(canvas.transform, "ENEMIC +", new Vector2(-35, 230), new Vector2(105, 32), () => CycleEnemy(1), UIKit.Style.Normal, 12);
        UIKit.Button(canvas.transform, "COFRE −", new Vector2(90, 230), new Vector2(95, 32), () => CycleChest(-1), UIKit.Style.Normal, 12);
        chestChoiceLabel = UIKit.Text(canvas.transform, "", new Vector2(245, 230), new Vector2(150, 32), 13, TextAlignmentOptions.Center, UIKit.TextColor);
        UIKit.Button(canvas.transform, "COFRE +", new Vector2(385, 230), new Vector2(95, 32), () => CycleChest(1), UIKit.Style.Normal, 12);
        UpdateObjectChoices();
        UIKit.Button(canvas.transform, "KIT CASTELL", new Vector2(-360, 315), new Vector2(140, 34), () => PlaceKit("Parets/Castell_blocs", "Parets/Castell_fosc", "Vegetacio/Seto_verd", "Natura/Gespa_flors_grogues"), UIKit.Style.Normal, 13);
        UIKit.Button(canvas.transform, "KIT CASA", new Vector2(-205, 315), new Vector2(140, 34), () => PlaceKit("Parets/Paret_casa", "Terres/Terra", "Objectes/Planta_test"), UIKit.Style.Normal, 13);
        UIKit.Button(canvas.transform, "KIT SUPERMERCAT", new Vector2(-35, 315), new Vector2(165, 34), () => PlaceKit("Moderns/Paret_supermercat", "Terres/Rajola_supermercat", "Objectes/Carro_compra", "Objectes/Estanteria_fruita", "Natura/Gespa_flors_mixtes"), UIKit.Style.Normal, 12);
        UIKit.Button(canvas.transform, "AJUDA", new Vector2(140, 315), new Vector2(110, 34), ToggleHelp, UIKit.Style.Primary, 13);
        string[] toolNames = { "PINTAR", "ESBORRAR", "OMPLIR", "RECTANGLE", "R. PLE/BUIT", "LÍNIA", "COMPTAGOTES", "DESFER", "REFER", "CAPA", "JUGADOR", "ENEMIC", "COFRE", "FINAL", "MERCADER", "COPIAR", "ENGANXAR", "V. CAPA" };
        float toolX = -(toolNames.Length - 1) * 88f * .5f; // fila centrada: a 16:9 (1600 de ample) abans se n'eixia per la dreta i V. CAPA no es veia
        foreach (string toolName in toolNames)
        {
            string action = toolName;
            UIKit.Button(canvas.transform, action, new Vector2(toolX, 270), new Vector2(82, 34), () => SelectTool(action), UIKit.Style.Normal, 11);
            toolX += 88;
        }
        UIKit.Button(canvas.transform, "ROTAR POSAT", new Vector2(-125, 190), new Vector2(150, 32), () => SelectTool("ROTAR POSAT"), UIKit.Style.Normal, 12);
        UIKit.Button(canvas.transform, "VOLTEJAR POSAT", new Vector2(45, 190), new Vector2(165, 32), () => SelectTool("VOLTEJAR POSAT"), UIKit.Style.Normal, 12);
        UIKit.Button(canvas.transform, "GRAELLA: SÍ", new Vector2(225, 190), new Vector2(145, 32), ToggleGrid, UIKit.Style.Normal, 12);
        UIKit.Button(canvas.transform, "HITBOXES: NO", new Vector2(390, 190), new Vector2(160, 32), ToggleHitboxes, UIKit.Style.Normal, 12);
        layerLabel = UIKit.Text(canvas.transform, "", new Vector2(-560, 190), new Vector2(380, 32), 17, TextAlignmentOptions.Left, UIKit.Gold);
        layerLabel.overflowMode = TextOverflowModes.Overflow;
        var panel = UIKit.Box(canvas.transform, new Vector2(-470, -124), new Vector2(245, 580), UIKit.Panel, "Panell paleta");
        palettePanel = panel.GetComponent<RectTransform>();
        var moveHandle = UIKit.Box(panel.transform, new Vector2(0, 280), new Vector2(225, 22), UIKit.ButtonColor, "Mou la paleta");
        paletteMoveHandle = moveHandle.GetComponent<RectTransform>();
        moveHandle.gameObject.AddComponent<PaletteDragHandle>().Initialize(this);
        UIKit.Text(moveHandle.transform, "ARROSSEGA PER A MOURE", Vector2.zero, new Vector2(235, 20), 11, TextAlignmentOptions.Center, UIKit.TextColor);
        searchField = UIKit.Input(panel, new Vector2(0, 230), new Vector2(220, 34), "Busca bloc", 40, fontSize: 14);
        familyField = UIKit.Input(panel, new Vector2(0, 188), new Vector2(220, 34), "Família (opcional)", 30, fontSize: 13);
        searchField.onValueChanged.AddListener(_ => BuildPalette());
        familyField.onValueChanged.AddListener(_ => BuildPalette());
        palette = UIKit.ScrollList(panel, new Vector2(0, -68), new Vector2(225, 375), out _, 2);
        UIKit.Button(panel, "VISTA: TOTS", new Vector2(0, 147), new Vector2(220, 32), CyclePaletteMode, UIKit.Style.Normal, 12);
        // La graella viu dins d'un marc que la retalla per dalt (RectMask2D) i va DARRERE de tota la UI: en fer zoom o moure-la ja no tapa els botons.
        // El marc té el seu origen al centre del Canvas, així les coordenades de la graella no canvien.
        const float gridClipTop = 172f, gridClipDepth = 6000f;
        var gridViewport = new GameObject("Graella marc", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
        gridViewport.SetParent(canvas.transform, false);
        gridViewport.anchorMin = gridViewport.anchorMax = Vector2.one * .5f;
        gridViewport.sizeDelta = new Vector2(8000f, gridClipDepth + gridClipTop);
        gridViewport.pivot = new Vector2(.5f, gridClipDepth / (gridClipDepth + gridClipTop));
        gridViewport.anchoredPosition = Vector2.zero;
        gridViewport.SetSiblingIndex(background.GetSiblingIndex() + 1);
        var gridRect = UIKit.Empty(gridViewport, "Graella");
        gridRect.anchorMin = gridRect.anchorMax = gridViewport.pivot;
        gridRect.anchoredPosition = Vector2.zero; gridRect.sizeDelta = Vector2.zero;
        grid = gridRect;
        promptPanel = UIKit.Box(canvas.transform, Vector2.zero, new Vector2(420, 200), UIKit.Panel, "Confirmació");
        promptText = UIKit.Text(promptPanel, "", new Vector2(0, 45), new Vector2(380, 60), 20, TextAlignmentOptions.Center, UIKit.TextColor);
        UIKit.Button(promptPanel, "CONFIRMAR", new Vector2(-95, -55), new Vector2(160, 46), () => { var action = pendingConfirm; promptPanel.gameObject.SetActive(false); pendingConfirm = null; action?.Invoke(); }, UIKit.Style.Danger, 16);
        UIKit.Button(promptPanel, "CANCEL·LAR", new Vector2(95, -55), new Vector2(160, 46), () => { promptPanel.gameObject.SetActive(false); pendingConfirm = null; }, UIKit.Style.Normal, 16);
        promptPanel.gameObject.SetActive(false);
        helpPanel = UIKit.Box(canvas.transform, Vector2.zero, new Vector2(920, 660), UIKit.Panel, "Guia");
        UIKit.Text(helpPanel, "GUIA DEL CREADOR  (F1 per obrir i tancar)", new Vector2(0, 300), new Vector2(880, 40), 24, TextAlignmentOptions.Center, UIKit.Gold);
        var guideText = UIKit.Text(helpPanel, GuideText, new Vector2(0, 12), new Vector2(870, 540), 15, TextAlignmentOptions.TopLeft, UIKit.TextColor);
        guideText.overflowMode = TextOverflowModes.Overflow;
        UIKit.Button(helpPanel, "TANCAR", new Vector2(0, -300), new Vector2(170, 42), ToggleHelp, UIKit.Style.Primary, 16);
        helpPanel.gameObject.SetActive(false);
        BuildPalette();
        DrawGrid();
        if (MapStore.Load("__autosave", out _, out _)) ShowPrompt("Hi ha una còpia automàtica. La vols recuperar?", RecoverAutosave);
    }

    sealed class PaletteDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        MapEditor owner;
        Vector2 offset;
        public void Initialize(MapEditor editor) => owner = editor;
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (owner != null && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)owner.uiRoot, eventData.position, eventData.pressEventCamera, out var pointer))
                offset = owner.palettePanel.anchoredPosition - pointer;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (owner != null && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)owner.uiRoot, eventData.position, eventData.pressEventCamera, out var pointer))
                owner.SetPalettePosition(pointer + offset);
        }
    }

    bool IsPointOverMapGrid(Vector2 screenPosition)
    {
        if (grid == null || map == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)grid, screenPosition, null, out var local)) return false;
        float cell = Mathf.Min(650f / map.width, 540f / map.height);
        float left = -150f - cell * .5f, right = -150f + (map.width - .5f) * cell;
        float top = 130f + cell * .5f, bottom = 130f - (map.height - .5f) * cell;
        return local.x >= left && local.x <= right && local.y >= bottom && local.y <= top;
    }

    void SetPalettePosition(Vector2 position)
    {
        if (palettePanel == null || uiRoot == null) return;
        Rect bounds = ((RectTransform)uiRoot).rect;
        Vector2 half = palettePanel.rect.size * .5f;
        position.x = Mathf.Clamp(position.x, bounds.xMin + half.x, bounds.xMax - half.x);
        position.y = Mathf.Clamp(position.y, bounds.yMin + half.y, bounds.yMax - half.y);
        palettePanel.anchoredPosition = position;
    }

    void NewMap(int width, int height)
    {
        currentSavedName = null; dirty = false;
        undo.Clear(); redo.Clear();
        map = new MapData { width = width, height = height, layers = new MapLayers() };
        mapWidthField.text = width.ToString();
        mapHeightField.text = height.ToString();
        DrawGrid();
        status.text = "Mapa nou " + width + "×" + height;
    }

    void BuildPalette()
    {
        UIKit.Clear(palette);
        string search = searchField.text.Trim();
        string family = familyField.text.Trim();
        var favorites = ReadPaletteIds("Canica.MapEditor.Favorites");
        var recent = ReadPaletteIds("Canica.MapEditor.Recent");
        var blocks = BlockCatalog.Instance.blocks.Where(b => b != null)
            .Where(b => string.IsNullOrEmpty(search) || b.displayName.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(b => string.IsNullOrEmpty(family) || b.family.IndexOf(family, System.StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(b => paletteMode != 1 || favorites.Contains(b.id))
            .Where(b => paletteMode != 2 || recent.Contains(b.id))
            .OrderBy(b => b.family).ThenBy(b => b.displayName);
        int rowIndex = 0;
        foreach (var block in blocks)
        {
            var row = UIKit.ListRow(palette, 38, rowIndex++ % 2 == 0 ? UIKit.Row : UIKit.RowAlt);
            row.name = "Bloc " + block.id;
            var button = row.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => { selected = block; RememberRecent(block.id); });
            UIKit.Picture(row, block.sprite, new Vector2(-84, 0), new Vector2(26, 26));
            bool favorite = favorites.Contains(block.id);
            var star = UIKit.Button(row, favorite ? "★" : "☆", new Vector2(95, 0), new Vector2(30, 30), () => ToggleFavorite(block.id), UIKit.Style.Ghost, 17, false);
            star.rect.name = "Favorit " + block.id;
            star.button.navigation = new Navigation { mode = Navigation.Mode.None };
            var label = UIKit.Text(row, block.displayName, new Vector2(24, 0), new Vector2(126, 32), 13, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }

    void CyclePaletteMode()
    {
        paletteMode = (paletteMode + 1) % 3;
        string label = paletteMode == 1 ? "VISTA: FAVORITS" : paletteMode == 2 ? "VISTA: RECENTS" : "VISTA: TOTS";
        var button = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name.StartsWith("Boto VISTA:"));
        if (button != null) button.GetComponentInChildren<TMP_Text>().text = label;
        BuildPalette();
    }

    void ToggleFavorite(string id)
    {
        var ids = ReadPaletteIds("Canica.MapEditor.Favorites");
        if (!ids.Add(id)) ids.Remove(id);
        PlayerPrefs.SetString("Canica.MapEditor.Favorites", string.Join("|", ids)); PlayerPrefs.Save();
        BuildPalette();
    }

    void RememberRecent(string id)
    {
        var ids = ReadPaletteIds("Canica.MapEditor.Recent");
        ids.Remove(id);
        var ordered = new List<string> { id };
        ordered.AddRange(ids.Take(11));
        PlayerPrefs.SetString("Canica.MapEditor.Recent", string.Join("|", ordered)); PlayerPrefs.Save();
        if (paletteMode == 2) BuildPalette();
    }

    static HashSet<string> ReadPaletteIds(string key) => new HashSet<string>(PlayerPrefs.GetString(key, "")
        .Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries), System.StringComparer.Ordinal);

    // Dibuixa només l'sprite (girat/voltejat) d'una cel·la d'una capa de sota, sense hitbox ni «?».
    void DrawLayerSprite(RectTransform parent, MapCell item, float cell)
    {
        var block = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && b.id == item.blockId);
        if (block == null) return;
        var image = UIKit.Picture(parent, item.resolvedSprite != null ? item.resolvedSprite : block.sprite, Vector2.zero, new Vector2(cell - 3, cell - 3));
        image.rectTransform.localEulerAngles = new Vector3(0, 0, -90 * item.rot);
        image.rectTransform.localScale = new Vector3(item.flip ? -1 : 1, 1, 1);
        image.raycastTarget = false;
    }

    void DrawGrid()
    {
        UIKit.Clear(grid);
        float cell = Mathf.Min(650f / map.width, 540f / map.height);
        var cells = VisibleCells();
        var cellsByPosition = MapAutoTile160.Index(cells);
        // Un índex per capa: la graella ensenya la capa de dalt de tot, però ara també dibuixa les de sota (p. ex. el terra sota d'un carro).
        var layerArrays = new[] { map.layers.ground, map.layers.objects, map.layers.overlay };
        var layerIndexes = new MapAutoTile160.CellIndex[3];
        for (int i = 0; i < 3; i++) layerIndexes[i] = layerVisible[i] ? MapAutoTile160.Index(layerArrays[i]) : null;
        var markers = GameObjectMarkers();
        for (int y = 0; y < map.height; y++)
        for (int x = 0; x < map.width; x++)
        {
            int cx = x, cy = y;
            var button = UIKit.Button(grid, "", new Vector2(-150 + x * cell, 130 - y * cell), new Vector2(cell - 2, cell - 2),
                () => { if (suppressGridClick == new Vector2Int(cx, cy)) { suppressGridClick = null; return; } suppressGridClick = null; Paint(cx, cy); }, UIKit.Style.Normal, 10);
            button.rect.name = "Cel·la " + cx + "," + cy;
            button.image.color = gridVisible ? UIKit.ButtonColor : new Color(0f, 0f, 0f, 0f);
            cellsByPosition.TryGetValue(new Vector2Int(cx, cy), out var source);
            var item = MapAutoTile160.Resolve(source, cellsByPosition);
            var cellPosition = new Vector2Int(cx, cy);
            int topLayer = -1;
            for (int l = 2; l >= 0 && topLayer < 0; l--) if (layerIndexes[l] != null && layerIndexes[l].TryGetValue(cellPosition, out _)) topLayer = l;
            for (int l = 0; l < topLayer; l++)
                if (layerIndexes[l] != null && layerIndexes[l].TryGetValue(cellPosition, out var lower))
                    DrawLayerSprite(button.rect, MapAutoTile160.Resolve(lower, layerIndexes[l]), cell);
            if (!string.IsNullOrEmpty(item.blockId))
            {
                var block = BlockCatalog.Instance.blocks.FirstOrDefault(b => b.id == item.blockId);
                if (block != null)
                {
                    var image = UIKit.Picture(button.rect, item.resolvedSprite != null ? item.resolvedSprite : block.sprite, Vector2.zero, new Vector2(cell - 3, cell - 3));
                    image.rectTransform.localEulerAngles = new Vector3(0, 0, -90 * item.rot);
                    image.rectTransform.localScale = new Vector3(item.flip ? -1 : 1, 1, 1);
                    image.raycastTarget = false;
                    if (hitboxesVisible && !block.passable && block.hitbox.width > 0f && block.hitbox.height > 0f)
                    {
                        Rect hitbox = MapAutoTile160.TransformHitbox(block.hitbox, item.rot, item.flip);
                        var visual = UIKit.Box(button.rect, new Vector2(hitbox.center.x * cell, -hitbox.center.y * cell),
                            new Vector2(hitbox.width * cell, hitbox.height * cell), new Color(.95f, .15f, .12f, .32f), "Hitbox depuració");
                        visual.GetComponent<Image>().raycastTarget = false;
                    }
                }
                else
                {
                    button.image.color = new Color(.55f, .18f, .55f);
                    UIKit.Text(button.rect, "?", Vector2.zero, new Vector2(cell - 3, cell - 3), Mathf.Max(10, cell * .55f), TextAlignmentOptions.Center, Color.white);
                }
            }
            // Marcadors dels objectes de joc (spawn, enemics, cofres, final, mercader): abans es posaven però no es veien enlloc.
            if (markers.TryGetValue(cellPosition, out var marker))
            {
                float size = Mathf.Max(8f, cell * .62f);
                var tag = UIKit.Box(button.rect, Vector2.zero, new Vector2(size, size), marker.color, "Marcador " + marker.text);
                tag.GetComponent<Image>().raycastTarget = false;
                UIKit.Text(tag, marker.text, Vector2.zero, new Vector2(size, size), Mathf.Max(8f, size * .72f), TextAlignmentOptions.Center, Color.white);
            }
        }
    }

    // Una lletra per tipus d'objecte de joc; si n'hi ha més d'un a la mateixa cel·la, se sumen les lletres.
    Dictionary<Vector2Int, (string text, Color color)> GameObjectMarkers()
    {
        var markers = new Dictionary<Vector2Int, (string text, Color color)>();
        void Mark(int mx, int my, string letter, Color color)
        {
            var key = new Vector2Int(mx, my);
            markers[key] = markers.TryGetValue(key, out var old) ? (old.text + letter, color) : (letter, color);
        }
        foreach (var s in map.spawns)
        {
            if (s.kind == "player") Mark(s.x, s.y, "J", new Color(.16f, .62f, .26f));
            else if (s.kind == "merchant") Mark(s.x, s.y, "M", new Color(.50f, .30f, .72f));
            else Mark(s.x, s.y, "E", new Color(.75f, .18f, .16f));
        }
        foreach (var c in map.chests) Mark(c.x, c.y, "C", new Color(.78f, .58f, .10f));
        if (map.goal != null) Mark(map.goal.x, map.goal.y, "F", new Color(.10f, .55f, .70f));
        return markers;
    }

    void ToggleGrid()
    {
        gridVisible = !gridVisible;
        var button = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Boto GRAELLA: SÍ" || b.gameObject.name == "Boto GRAELLA: NO");
        if (button != null) button.GetComponentInChildren<TMP_Text>().text = "GRAELLA: " + (gridVisible ? "SÍ" : "NO");
        DrawGrid();
    }

    void ToggleHitboxes()
    {
        hitboxesVisible = !hitboxesVisible;
        var button = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Boto HITBOXES: SÍ" || b.gameObject.name == "Boto HITBOXES: NO");
        if (button != null) button.GetComponentInChildren<TMP_Text>().text = "HITBOXES: " + (hitboxesVisible ? "SÍ" : "NO");
        DrawGrid();
    }

    void Paint(int x, int y)
    {
        if (placeMode != PlaceMode.Blocks) { PlaceObject(x, y); return; }
        if (!eraseMode && selected != null && (tool == EditTool.Paint || tool == EditTool.Fill || tool == EditTool.Rectangle || tool == EditTool.Line))
            layer = LayerFor(selected);
        if (tool == EditTool.Copy) { CopyRegion(x, y); return; }
        if (tool == EditTool.Paste) { PasteRegion(x, y); return; }
        if (tool == EditTool.RotatePlaced || tool == EditTool.FlipPlaced) { TransformPlaced(x, y); return; }
        if (tool == EditTool.Fill) { Fill(x, y); return; }
        if (tool == EditTool.Eyedropper) { selected = ActiveCells().FirstOrDefault(c => c.x == x && c.y == y).blockId == null ? null :
            BlockCatalog.Instance.blocks.FirstOrDefault(b => b.id == ActiveCells().First(c => c.x == x && c.y == y).blockId); return; }
        if ((tool == EditTool.Rectangle || tool == EditTool.Line) && dragStart.HasValue)
        {
            DrawShape(dragStart.Value, new Vector2Int(x, y), tool == EditTool.Rectangle);
            dragStart = null;
            return;
        }
        if (tool == EditTool.Rectangle || tool == EditTool.Line) { dragStart = new Vector2Int(x, y); status.text = "Tria el segon cantó."; return; }
        if ((tool == EditTool.Fill || tool == EditTool.Rectangle || tool == EditTool.Line) && selected != null && selected.multiPart)
        { status.text = "Les peces multipart es pinten com un objecte sencer."; return; }
        var cells = new List<MapCell>(ActiveCells());
        Vector2Int anchorOffset = Vector2Int.zero;
        List<MultipartPart> multipart = selected != null && selected.multiPart && tool != EditTool.Erase && !eraseMode
            ? MultipartParts(selected, out anchorOffset) : null;
        var placed = new List<MapCell>();
        if (multipart != null && multipart.Count > 1)
        {
            foreach (var part in multipart)
            {
                var offset = TransformOffset(part.offset - anchorOffset, rotation, flip);
                int px = x + offset.x, py = y + offset.y;
                if (px < 0 || py < 0 || px >= map.width || py >= map.height)
                { status.text = "No cap l'objecte sencer dins del mapa."; return; }
                placed.Add(new MapCell { x = px, y = py, blockId = part.block.id, rot = rotation, flip = flip });
            }
        }
        PushUndo();
        RemovePlacedAt(cells, x, y);
        if (placed.Count == 0 && tool != EditTool.Erase && !eraseMode && selected != null)
            placed.Add(new MapCell { x = x, y = y, blockId = selected.id, rot = rotation, flip = flip });
        foreach (var part in placed)
        {
            RemovePlacedAt(cells, part.x, part.y);
            cells.RemoveAll(c => c.x == part.x && c.y == part.y);
            cells.Add(part);
        }
        SetActiveCells(cells.ToArray());
        DrawGrid();
    }

    List<MultipartPart> MultipartParts(BlockData block, out Vector2Int selectedOffset)
    {
        selectedOffset = Vector2Int.zero;
        var result = new List<MultipartPart>();
        if (block == null || !block.multiPart) return result;
        int split = block.id.LastIndexOf('_');
        if (split < 0) { result.Add(new MultipartPart { block = block }); return result; }
        string stem = block.id.Substring(0, split);
        var byPart = BlockCatalog.Instance.blocks.Where(b => b != null && b.multiPart && b.id.StartsWith(stem + "_", System.StringComparison.Ordinal))
            .ToDictionary(b => b.id.Substring(stem.Length + 1), b => b, System.StringComparer.OrdinalIgnoreCase);
        string[] layout;
        Vector2Int[] offsets;
        if (byPart.ContainsKey("esq") && byPart.ContainsKey("mig") && byPart.ContainsKey("dre"))
        { layout = new[] { "esq", "mig", "dre" }; offsets = new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2 }; }
        else if (byPart.ContainsKey("esq") && byPart.ContainsKey("dre"))
        { layout = new[] { "esq", "dre" }; offsets = new[] { Vector2Int.zero, Vector2Int.right }; }
        else if (byPart.ContainsKey("cap") && byPart.ContainsKey("peus"))
        { layout = new[] { "peus", "cap" }; offsets = new[] { Vector2Int.zero, Vector2Int.up }; }
        else if (byPart.ContainsKey("cap") && byPart.ContainsKey("popa"))
        { layout = new[] { "cap", "popa" }; offsets = new[] { Vector2Int.zero, Vector2Int.right }; }
        else { result.Add(new MultipartPart { block = block }); return result; }
        for (int i = 0; i < layout.Length; i++)
        {
            result.Add(new MultipartPart { block = byPart[layout[i]], offset = offsets[i] });
            if (byPart[layout[i]].id == block.id) selectedOffset = offsets[i];
        }
        return result;
    }

    static Vector2Int TransformOffset(Vector2Int offset, int rotation, bool flip)
    {
        if (flip) offset.x = -offset.x;
        for (int i = 0; i < ((rotation % 4) + 4) % 4; i++) offset = new Vector2Int(offset.y, -offset.x);
        return offset;
    }

    void RemovePlacedAt(List<MapCell> cells, int x, int y)
    {
        var clicked = cells.FirstOrDefault(c => c.x == x && c.y == y);
        if (clicked.blockId == null) return;
        var block = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && b.id == clicked.blockId);
        var group = block != null && block.multiPart ? FindPlacedMultipartGroup(cells, clicked, block) : new List<MapCell> { clicked };
        foreach (var member in group) cells.RemoveAll(c => c.x == member.x && c.y == member.y);
    }

    List<MapCell> FindPlacedMultipartGroup(List<MapCell> cells, MapCell clicked, BlockData block)
    {
        var parts = MultipartParts(block, out var clickedOffset);
        if (parts.Count <= 1) return new List<MapCell> { clicked };
        var group = new List<MapCell>();
        foreach (var part in parts)
        {
            var offset = TransformOffset(part.offset - clickedOffset, clicked.rot, clicked.flip);
            var placed = cells.FirstOrDefault(c => c.x == clicked.x + offset.x && c.y == clicked.y + offset.y && c.blockId == part.block.id);
            if (placed.blockId != null) group.Add(placed);
        }
        if (group.Count == parts.Count) return group;

        var expectedIds = new HashSet<string>(parts.Select(p => p.block.id), System.StringComparer.Ordinal);
        var foundIds = new HashSet<string>(group.Select(c => c.blockId), System.StringComparer.Ordinal);
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(clicked.x, clicked.y));
        while (queue.Count > 0 && foundIds.Count < expectedIds.Count)
        {
            var position = queue.Dequeue();
            if (!visited.Add(position)) continue;
            var candidate = cells.FirstOrDefault(c => c.x == position.x && c.y == position.y && expectedIds.Contains(c.blockId));
            if (candidate.blockId == null) continue;
            if (foundIds.Add(candidate.blockId) && !group.Any(c => c.x == candidate.x && c.y == candidate.y)) group.Add(candidate);
            queue.Enqueue(position + Vector2Int.up); queue.Enqueue(position + Vector2Int.down);
            queue.Enqueue(position + Vector2Int.left); queue.Enqueue(position + Vector2Int.right);
        }
        return group.Count > 0 ? group : new List<MapCell> { clicked };
    }

    static EditLayer LayerFor(BlockData block)
    {
        if (block.layer == BlockData.DrawLayer.OverPlayer && !block.fullBlock) return EditLayer.Overlay;
        if (!block.passable) return EditLayer.Objects;
        return EditLayer.Ground;
    }

    MapCell[] ActiveCells() => layer == EditLayer.Ground ? map.layers.ground :
        layer == EditLayer.Objects ? map.layers.objects : map.layers.overlay;
    MapCell[] VisibleCells()
    {
        var visible = new List<MapCell>();
        if (layerVisible[0]) visible.AddRange(map.layers.ground);
        if (layerVisible[1]) visible.AddRange(map.layers.objects);
        if (layerVisible[2]) visible.AddRange(map.layers.overlay);
        return visible.GroupBy(c => c.x + "," + c.y).Select(g => g.Last()).ToArray();
    }
    void SetActiveCells(MapCell[] cells)
    {
        if (layer == EditLayer.Ground) map.layers.ground = cells;
        else if (layer == EditLayer.Objects) map.layers.objects = cells;
        else map.layers.overlay = cells;
    }
    const string GuideText =
        "<b>PINTAR</b>: tria un bloc a la paleta (esquerra) i clica o arrossega per la graella.\n" +
        "<b>ESBORRAR</b> (o la tecla E): lleva el bloc de la capa activa.   <b>OMPLIR</b>: cubell sobre blocs iguals connectats.\n" +
        "<b>RECTANGLE</b> i <b>LÍNIA</b>: clica dos punts.   <b>R. PLE/BUIT</b>: canvia entre rectangle ple i buit.   <b>COMPTAGOTES</b>: agafa el bloc de sota el ratolí.\n" +
        "<b>COPIAR</b> / <b>ENGANXAR</b>: copia una regió i posa-la on vulgues.\n" +
        "<b>ROTAR</b> (R) i <b>VOLTEJAR</b> (F) giren el bloc que vas a posar; <b>ROTAR POSAT</b> i <b>VOLTEJAR POSAT</b>, un bloc que ja és al mapa.\n" +
        "<b>DESFER</b> (Ctrl+Z) i <b>REFER</b> (Ctrl+Y).   <b>GUARDAR</b> (Ctrl+S): posa-li un nom teu, no «Mapa».\n\n" +
        "<b>LES CAPES</b> (cada cel·la en té 3, i tu veus quina és dalt a l'esquerra, a CAPA):\n" +
        "   · <b>TERRA</b>: el que es pot travessar: terres, herba, flors i aigua.\n" +
        "   · <b>OBJECTES</b>: parets i blocs que ocupen la cel·la sencera.\n" +
        "   · <b>DAMUNT</b>: objectes que no omplin la cel·la i van sobre el jugador (carros, llums, copes d'arbre...).\n" +
        "En pintar, el bloc va sol a la seua capa. Pots combinar capes en una cel·la (un carro sobre terra),\n" +
        "però dos blocs de la mateixa capa es substitueixen entre ells (p. ex. una flor lleva el terra).\n" +
        "El botó <b>CAPA</b> canvia la capa activa (per a esborrar, copiar...) i <b>V. CAPA</b> amaga o mostra la capa.\n\n" +
        "<b>OBJECTES DE JOC</b>: JUGADOR (punt d'inici), ENEMIC, COFRE, FINAL (portal) i MERCADER. ENEMIC −/+ i COFRE −/+ trien quin. <b>PROVAR</b> juga el mapa i torna a l'editor.\n" +
        "<b>MARCADORS</b> a la graella: <b>J</b> punt d'inici (només n'hi ha un; en tornar a clicar es mou), <b>E</b> enemic, <b>C</b> cofre, <b>F</b> final, <b>M</b> mercader. Ctrl+Z desfà l'últim que has posat.\n" +
        "<b>MOURE LA VISTA</b>: WASD o botó del mig del ratolí; la roda fa zoom. GRAELLA i HITBOXES ajuden a depurar. La paleta es mou arrossegant la barra de dalt.\n" +
        "<b>KITS</b>: Castell, Casa i Supermercat posen una base feta.\n" +
        "<b>COMPARTIR</b>: COPIAR CODI i IMPORTAR CODI, o el panell de fitxers. Importa només mapes de gent que conegues.";

    static string LayerName(EditLayer value) => value == EditLayer.Ground ? "TERRA" : value == EditLayer.Objects ? "OBJECTES" : "DAMUNT";

    // La capa on anirà el bloc: en pintar, es tria sola segons el bloc triat; si no, és la capa activa.
    EditLayer EffectiveLayer() =>
        placeMode == PlaceMode.Blocks && !eraseMode && selected != null &&
        (tool == EditTool.Paint || tool == EditTool.Fill || tool == EditTool.Rectangle || tool == EditTool.Line)
            ? LayerFor(selected) : layer;

    void ToggleHelp()
    {
        if (helpPanel != null) helpPanel.gameObject.SetActive(!helpPanel.gameObject.activeSelf);
    }

    void SelectTool(string name)
    {
        if (name == "R. PLE/BUIT") { filledRectangle = !filledRectangle; status.text = filledRectangle ? "Rectangle ple" : "Rectangle buit"; return; }
        if (name == "JUGADOR") { placeMode = PlaceMode.Player; status.text = "Posa el punt inicial"; return; }
        if (name == "ENEMIC") { placeMode = PlaceMode.Enemy; status.text = "Posa un Llim"; return; }
        if (name == "COFRE") { placeMode = PlaceMode.Chest; status.text = "Posa un cofre comú"; return; }
        if (name == "FINAL") { placeMode = PlaceMode.Goal; status.text = "Posa o mou el portal final"; return; }
        if (name == "MERCADER") { placeMode = PlaceMode.Merchant; status.text = "Posa un mercader"; return; }
        if (name == "V. CAPA") { placeMode = PlaceMode.Blocks; layerVisible[(int)layer] = !layerVisible[(int)layer]; DrawGrid(); status.text = "Visibilitat " + layer + ": " + (layerVisible[(int)layer] ? "activa" : "oculta"); return; }
        placeMode = PlaceMode.Blocks;
        tool = name switch
        {
            "ESBORRAR" => EditTool.Erase, "OMPLIR" => EditTool.Fill, "RECTANGLE" => EditTool.Rectangle,
            "LÍNIA" => EditTool.Line, "COMPTAGOTES" => EditTool.Eyedropper,
            "COPIAR" => EditTool.Copy, "ENGANXAR" => EditTool.Paste,
            "ROTAR POSAT" => EditTool.RotatePlaced, "VOLTEJAR POSAT" => EditTool.FlipPlaced,
            "DESFER" => EditTool.Paint, "REFER" => EditTool.Paint, _ => EditTool.Paint
        };
        if (name == "DESFER") Undo();
        if (name == "REFER") Redo();
        if (name == "CAPA") layer = (EditLayer)(((int)layer + 1) % 3);
        status.text = "Eina: " + name + (name == "CAPA" ? " · " + layer : "");
    }

    void TransformPlaced(int x, int y)
    {
        var cells = ActiveCells().ToArray();
        int index = System.Array.FindIndex(cells, c => c.x == x && c.y == y);
        if (index < 0) { status.text = "No hi ha cap bloc en esta cel·la de la capa."; return; }
        var clicked = cells[index];
        var block = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && b.id == clicked.blockId);
        if (block != null && block.multiPart)
        {
            var list = new List<MapCell>(cells);
            var group = FindPlacedMultipartGroup(list, clicked, block);
            if (group.Count > 1)
            {
                bool rotate = tool == EditTool.RotatePlaced;
                var groupCoordinates = new HashSet<Vector2Int>(group.Select(c => new Vector2Int(c.x, c.y)));
                var transformed = new List<MapCell>();
                foreach (var member in group)
                {
                    var offset = TransformOffset(new Vector2Int(member.x - clicked.x, member.y - clicked.y), rotate ? 1 : 0, !rotate);
                    int nx = clicked.x + offset.x, ny = clicked.y + offset.y;
                    if (nx < 0 || ny < 0 || nx >= map.width || ny >= map.height ||
                        list.Any(c => c.x == nx && c.y == ny && !groupCoordinates.Contains(new Vector2Int(c.x, c.y))))
                    { status.text = "No hi ha espai per transformar l'objecte multipart."; return; }
                    var moved = member;
                    moved.x = nx; moved.y = ny;
                    if (rotate) moved.rot = (moved.rot + 1) % 4; else moved.flip = !moved.flip;
                    transformed.Add(moved);
                }
                PushUndo();
                list.RemoveAll(c => groupCoordinates.Contains(new Vector2Int(c.x, c.y)));
                list.AddRange(transformed);
                SetActiveCells(list.ToArray()); DrawGrid(); return;
            }
        }
        PushUndo();
        if (tool == EditTool.RotatePlaced) cells[index].rot = (cells[index].rot + 1) % 4;
        else cells[index].flip = !cells[index].flip;
        SetActiveCells(cells);
        DrawGrid();
    }
    void PlaceObject(int x, int y)
    {
        PushUndo();
        if (placeMode == PlaceMode.Player)
        {
            map.spawns = map.spawns.Where(s => s.kind != "player").Concat(new[] { new MapSpawn { x = x, y = y, kind = "player" } }).ToArray();
            status.text = "Punt d'inici (J) a " + x + "," + y + ". Torna a clicar en una altra cel·la per moure'l.";
        }
        else if (placeMode == PlaceMode.Enemy)
            map.spawns = map.spawns.Concat(new[] { new MapSpawn { x = x, y = y, kind = "enemy", id = SelectedEnemyId() } }).ToArray();
        else if (placeMode == PlaceMode.Chest)
            map.chests = map.chests.Concat(new[] { new MapChest { x = x, y = y, rarity = chestChoices[chestChoiceIndex].ToString() } }).ToArray();
        else if (placeMode == PlaceMode.Goal) map.goal = new MapGoal { x = x, y = y };
        else if (placeMode == PlaceMode.Merchant)
            map.spawns = map.spawns.Concat(new[] { new MapSpawn { x = x, y = y, kind = "merchant", id = "Mercader" } }).ToArray();
        DrawGrid();
    }

    string SelectedEnemyId() => enemyChoices.Length == 0 ? "Llim" : enemyChoices[Mathf.Clamp(enemyChoiceIndex, 0, enemyChoices.Length - 1)].name;
    void CycleEnemy(int delta)
    {
        if (enemyChoices.Length == 0) { status.text = "No hi ha enemics al catàleg."; return; }
        enemyChoiceIndex = (enemyChoiceIndex + delta + enemyChoices.Length) % enemyChoices.Length;
        UpdateObjectChoices();
    }
    void CycleChest(int delta)
    {
        chestChoiceIndex = (chestChoiceIndex + delta + chestChoices.Length) % chestChoices.Length;
        UpdateObjectChoices();
    }
    void UpdateObjectChoices()
    {
        if (enemyChoiceLabel != null) enemyChoiceLabel.text = "Enemic: " + SelectedEnemyId();
        if (chestChoiceLabel != null) chestChoiceLabel.text = "Cofre: " + chestChoices[chestChoiceIndex];
    }
    void Playtest()
    {
        string target = string.IsNullOrWhiteSpace(nameField.text) ? "Mapa" : nameField.text.Trim();
        if (MapStore.List().Contains(target) && target != currentSavedName)
        { ShowPrompt("Ja existeix un mapa amb este nom. Vols sobreescriure'l abans de provar-lo?", () => { if (SaveAs(target)) BeginPlaytest(); }); return; }
        if (SaveAs(target)) BeginPlaytest();
    }

    void BeginPlaytest()
    {
        if (!MapStore.Load(map.name, out _, out var error)) { status.text = error; return; }
        MapPlaytest.MapName = map.name;
        MapPlaytest.StartInDungeon = true;
        MapPlaytest.DeleteMapAfterConsume = false;
        RunStats.DontSave = true;
        SceneManager.LoadScene("Dungeon");
    }
    void CopyRegion(int x, int y)
    {
        if (!dragStart.HasValue) { dragStart = new Vector2Int(x,y); status.text = "Tria l'altre cantó de la selecció."; return; }
        int minX=Mathf.Min(x,dragStart.Value.x), maxX=Mathf.Max(x,dragStart.Value.x), minY=Mathf.Min(y,dragStart.Value.y), maxY=Mathf.Max(y,dragStart.Value.y);
        bool expanded;
        do
        {
            expanded = false;
            foreach (var layerCells in new[] { map.layers.ground, map.layers.objects, map.layers.overlay })
            foreach (var cell in layerCells.Where(c => c.x >= minX && c.x <= maxX && c.y >= minY && c.y <= maxY).ToArray())
            {
                var block = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && b.id == cell.blockId);
                if (block == null || !block.multiPart) continue;
                foreach (var part in FindPlacedMultipartGroup(layerCells.ToList(), cell, block))
                {
                    if (part.x < minX) { minX = part.x; expanded = true; }
                    if (part.x > maxX) { maxX = part.x; expanded = true; }
                    if (part.y < minY) { minY = part.y; expanded = true; }
                    if (part.y > maxY) { maxY = part.y; expanded = true; }
                }
            }
        } while (expanded);
        clipboard = new MapLayers { ground=Crop(map.layers.ground,minX,minY,maxX,maxY), objects=Crop(map.layers.objects,minX,minY,maxX,maxY), overlay=Crop(map.layers.overlay,minX,minY,maxX,maxY) };
        dragStart = null; status.text = "Regió copiada.";
    }
    static MapCell[] Crop(MapCell[] cells,int minX,int minY,int maxX,int maxY) => cells.Where(c=>c.x>=minX&&c.x<=maxX&&c.y>=minY&&c.y<=maxY)
        .Select(c=> { c.x-=minX; c.y-=minY; return c; }).ToArray();
    void PasteRegion(int x,int y)
    {
        if (clipboard == null) { status.text="Copia una regió primer."; return; }
        PushUndo();
        map.layers.ground=PasteCells(map.layers.ground,clipboard.ground,x,y); map.layers.objects=PasteCells(map.layers.objects,clipboard.objects,x,y); map.layers.overlay=PasteCells(map.layers.overlay,clipboard.overlay,x,y);
        DrawGrid(); status.text="Regió enganxada.";
    }
    MapCell[] PasteCells(MapCell[] target,MapCell[] source,int x,int y)
    {
        var result = new List<MapCell>(target);
        var pasted = new List<MapCell>();
        foreach (var sourceCell in source)
        {
            var cell = sourceCell; cell.x += x; cell.y += y;
            if (cell.x < 0 || cell.y < 0 || cell.x >= map.width || cell.y >= map.height) continue;
            pasted.Add(cell);
        }
        foreach (var cell in pasted) RemovePlacedAt(result, cell.x, cell.y);
        foreach (var cell in pasted) { result.RemoveAll(v => v.x == cell.x && v.y == cell.y); result.Add(cell); }
        return result.ToArray();
    }
    void PlaceKit(string wallId,string floorId,params string[] decorIds)
    {
        var wall=BlockCatalog.Instance.blocks.FirstOrDefault(b=>b.id==wallId); var floor=BlockCatalog.Instance.blocks.FirstOrDefault(b=>b.id==floorId);
        if(wall==null||floor==null){ status.text="Falta una peça del kit."; return; }
        PushUndo(); int w=Mathf.Min(9,map.width), h=Mathf.Min(9,map.height), sx=(map.width-w)/2, sy=(map.height-h)/2;
        var ground=map.layers.ground.Where(c=>c.x<sx||c.x>=sx+w||c.y<sy||c.y>=sy+h).ToList();
        var objects=map.layers.objects.Where(c=>c.x<sx||c.x>=sx+w||c.y<sy||c.y>=sy+h).ToList();
        for(int y=0;y<h;y++) for(int x=0;x<w;x++) { int px=sx+x,py=sy+y; bool edge=x==0||y==0||x==w-1||y==h-1; var cell=new MapCell{x=px,y=py,blockId=edge?wall.id:floor.id}; (edge?objects:ground).Add(cell); }
        Vector2Int[] decorSpots = { new Vector2Int(sx + 2, sy + 2), new Vector2Int(sx + w - 3, sy + 2), new Vector2Int(sx + 2, sy + h - 3), new Vector2Int(sx + w - 3, sy + h - 3) };
        for (int i = 0; i < decorIds.Length && i < decorSpots.Length; i++)
        {
            var decor = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && b.id == decorIds[i]);
            if (decor == null) continue;
            var p = decorSpots[i];
            objects.RemoveAll(c => c.x == p.x && c.y == p.y);
            objects.Add(new MapCell { x = p.x, y = p.y, blockId = decor.id });
        }
        map.layers.ground=ground.ToArray(); map.layers.objects=objects.ToArray(); DrawGrid(); status.text="Kit col·locat: "+wallId;
    }
    void Fill(int x, int y)
    {
        if (selected != null && selected.multiPart) { status.text = "No es pot omplir amb un bloc multipart."; return; }
        if (selected == null) return;
        var cells = new List<MapCell>(ActiveCells());
        string original = cells.FirstOrDefault(c => c.x == x && c.y == y).blockId;
        if (original == selected.id) return;
        PushUndo();
        var sourceCells = cells.ToArray();
        var visited = new HashSet<Vector2Int>(); var queue = new Queue<Vector2Int>(); queue.Enqueue(new Vector2Int(x, y));
        while (queue.Count > 0)
        {
            var p = queue.Dequeue(); if (!visited.Add(p) || p.x < 0 || p.y < 0 || p.x >= map.width || p.y >= map.height) continue;
            string id = sourceCells.FirstOrDefault(c => c.x == p.x && c.y == p.y).blockId;
            if (id != original) continue;
            cells.RemoveAll(c => c.x == p.x && c.y == p.y);
            if (selected != null) cells.Add(new MapCell { x = p.x, y = p.y, blockId = selected.id, rot = rotation, flip = flip });
            queue.Enqueue(p + Vector2Int.up); queue.Enqueue(p + Vector2Int.down); queue.Enqueue(p + Vector2Int.left); queue.Enqueue(p + Vector2Int.right);
        }
        SetActiveCells(cells.ToArray()); DrawGrid();
    }
    void DrawShape(Vector2Int start, Vector2Int end, bool rectangle)
    {
        if (selected != null && selected.multiPart) { status.text = "No es pot dibuixar una forma amb un bloc multipart."; return; }
        if (selected == null) return;
        var cells = new List<MapCell>(ActiveCells());
        PushUndo();
        int minX = Mathf.Min(start.x, end.x), maxX = Mathf.Max(start.x, end.x);
        int minY = Mathf.Min(start.y, end.y), maxY = Mathf.Max(start.y, end.y);
        var points = new List<Vector2Int>();
        if (rectangle)
        {
            for (int y = minY; y <= maxY; y++) for (int x = minX; x <= maxX; x++)
                if (filledRectangle || x == minX || x == maxX || y == minY || y == maxY) points.Add(new Vector2Int(x,y));
        }
        else
            points.AddRange(TraceLine(start, end));
        foreach (var p in points) { int x=p.x,y=p.y; cells.RemoveAll(c => c.x == x && c.y == y); cells.Add(new MapCell { x = x, y = y, blockId = selected.id, rot = rotation, flip = flip }); }
        SetActiveCells(cells.ToArray()); DrawGrid();
    }

    internal static Vector2Int[] TraceLine(Vector2Int start, Vector2Int end)
    {
        var points = new List<Vector2Int>();
        int x = start.x, y = start.y, dx = Mathf.Abs(end.x - x), sx = x < end.x ? 1 : -1;
        int dy = -Mathf.Abs(end.y - y), sy = y < end.y ? 1 : -1, error = dx + dy;
        while (true)
        {
            points.Add(new Vector2Int(x, y));
            if (x == end.x && y == end.y) break;
            int twice = 2 * error;
            if (twice >= dy) { error += dy; x += sx; }
            if (twice <= dx) { error += dx; y += sy; }
        }
        return points.ToArray();
    }

    void Undo()
    {
        if (undo.Count == 0) return;
        redo.Push(Capture()); Restore(undo.Pop());
        DrawGrid();
    }

    void Redo()
    {
        if (redo.Count == 0) return;
        undo.Push(Capture()); Restore(redo.Pop());
        DrawGrid();
    }

    Snapshot Capture() => new Snapshot { ground = map.layers.ground.ToArray(), objects = map.layers.objects.ToArray(), overlay = map.layers.overlay.ToArray(), spawns=map.spawns.ToArray(), chests=map.chests.ToArray(), goal=map.goal == null ? null : new MapGoal { x=map.goal.x,y=map.goal.y } };
    void Restore(Snapshot s) { map.layers.ground = s.ground; map.layers.objects = s.objects; map.layers.overlay = s.overlay; map.spawns=s.spawns; map.chests=s.chests; map.goal=s.goal; }
    void PushUndo() { undo.Push(Capture()); if (undo.Count > 50) { var keep = undo.Reverse().Take(50).Reverse().ToArray(); undo.Clear(); foreach (var s in keep) undo.Push(s); } redo.Clear(); dirty = true; }

    void Save()
    {
        string target = string.IsNullOrWhiteSpace(nameField.text) ? "Mapa" : nameField.text.Trim();
        if (MapStore.List().Contains(target) && target != currentSavedName)
        { ShowPrompt("Ja existeix un mapa amb este nom. Vols sobreescriure'l?", () => SaveAs(target)); return; }
        SaveAs(target);
    }
    // En guardar de debò, la còpia automàtica ja no fa falta: si es quedara, el Creador preguntaria «recuperar-la?» cada vegada que s'obri.
    bool SaveAs(string target) { map.name = target; if (MapStore.Save(map, map.name, out var error)) { currentSavedName=map.name; dirty=false; MapStore.Delete("__autosave"); status.text = "Guardat: " + map.name; return true; } status.text = error; return false; }
    void ShowPrompt(string message, System.Action action) { pendingConfirm=action; promptText.text=message; promptPanel.gameObject.SetActive(true); }
    void ConfirmNewMap()
    {
        if (!int.TryParse(mapWidthField.text, out int width) || !int.TryParse(mapHeightField.text, out int height) ||
            width < 1 || height < 1 || width > MapStore.MaxDimension || height > MapStore.MaxDimension)
        { status.text = "La mida ha d'estar entre 1 i 64 blocs."; return; }
        if (dirty) ShowPrompt("Hi ha canvis sense guardar. Vols començar un mapa nou?", () => NewMap(width, height));
        else NewMap(width, height);
    }
    void RecoverAutosave() { if (MapStore.Load("__autosave",out var recovered,out var error)) { map=recovered; nameField.text=map.name; mapWidthField.text=map.width.ToString(); mapHeightField.text=map.height.ToString(); currentSavedName=null; dirty=true; DrawGrid(); status.text="Còpia automàtica recuperada; guarda-la amb un nom."; } else status.text=error; }
    void ConfirmExit() { if (dirty) ShowPrompt("Hi ha canvis sense guardar. Vols eixir igualment?", LeaveEditor); else LeaveEditor(); }
    void LeaveEditor() { MapStore.Delete("__autosave"); SceneManager.LoadScene("MainMenu"); } // eixir a consciència: la còpia automàtica és per a quan el joc es tanca de cop
    void ExportCode()
    {
        if (!MapShare160.Encode(map, out var code, out var error)) { status.text = error; return; }
        GUIUtility.systemCopyBuffer = code; status.text = "Codi CVM1 copiat al portapapers.";
    }
    void ImportCode()
    {
        if (!MapShare160.Decode(GUIUtility.systemCopyBuffer, out var imported, out var error)) { status.text = error; return; }
        map = imported; nameField.text = map.name; mapWidthField.text=map.width.ToString(); mapHeightField.text=map.height.ToString(); currentSavedName=null; dirty=true; undo.Clear(); redo.Clear(); DrawGrid(); status.text = "Codi importat. Guarda'l per conservar-lo.";
    }
    string SharePath()
    {
        string path = sharePathField.text.Trim();
        if (string.IsNullOrEmpty(path)) path = System.IO.Path.Combine(Application.persistentDataPath, "Mapes", map.name + ".canicamap");
        if (System.IO.Path.GetExtension(path).Length == 0) path += ".canicamap";
        return path;
    }
    void ExportFile()
    {
        string path = SharePath();
        if (MapShare160.ExportFile(map, path, out var error)) status.text = "Exportat: " + path; else status.text = error;
    }
    void ImportFile()
    {
        if (!MapShare160.ImportFile(SharePath(), out var imported, out var error)) { status.text = error; return; }
        map = imported; nameField.text = map.name; mapWidthField.text=map.width.ToString(); mapHeightField.text=map.height.ToString(); currentSavedName=null; dirty=true; undo.Clear(); redo.Clear(); DrawGrid(); status.text = "Fitxer importat: " + map.name;
    }

    void Load()
    {
        var names = MapStore.List();
        if (names.Length == 0) { status.text = "Encara no hi ha mapes guardats."; return; }
        if (mapListPanel != null) Destroy(mapListPanel.gameObject);
        mapListPanel = UIKit.Box(uiRoot, Vector2.zero, new Vector2(720, 620), new Color(.04f, .06f, .09f, .98f), "Llista mapes guardats");
        UIKit.Text(mapListPanel, "OBRIR MAPA", new Vector2(0, 270), new Vector2(620, 38), 24, TextAlignmentOptions.Center, UIKit.Gold);
        var content = UIKit.ScrollList(mapListPanel, new Vector2(0, -5), new Vector2(660, 500), out _, 4);
        int i = 0;
        foreach (string savedName in names)
        {
            string captured = savedName;
            if (!MapStore.Load(captured, out var savedMap, out _)) continue;
            var row = UIKit.ListRow(content, 52, i++ % 2 == 0 ? UIKit.Row : UIKit.RowAlt);
            row.name = "Mapa guardat " + captured;
            var button = row.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => LoadSavedMap(captured));
            var modified = MapStore.LastModifiedUtc(captured);
            string date = modified == System.DateTime.MinValue ? "data desconeguda" : modified.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            var label = UIKit.Text(row, captured + "  ·  " + savedMap.width + "×" + savedMap.height + "  ·  " + date,
                Vector2.zero, new Vector2(620, 46), 16, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }
        UIKit.Button(mapListPanel, "TANCAR", new Vector2(0, -275), new Vector2(150, 42), () => { Destroy(mapListPanel.gameObject); mapListPanel = null; }, UIKit.Style.Normal, 15);
    }

    void LoadSavedMap(string savedName)
    {
        if (mapListPanel != null) { Destroy(mapListPanel.gameObject); mapListPanel = null; }
        if (MapStore.Load(savedName, out map, out var error)) { nameField.text = savedName; mapWidthField.text=map.width.ToString(); mapHeightField.text=map.height.ToString(); currentSavedName = savedName; dirty = false; DrawGrid(); status.text = "Obert: " + savedName; }
        else status.text = error;
    }
}
