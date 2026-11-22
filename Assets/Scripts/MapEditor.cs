using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MapEditor : MonoBehaviour
{
    enum EditTool { Paint, Erase, Fill, Rectangle, Line, Eyedropper, Copy, Paste }
    enum EditLayer { Ground, Objects, Overlay }
    enum PlaceMode { Blocks, Player, Enemy, Chest, Goal, Merchant }
    MapData map;
    BlockData selected;
    Transform grid, palette, uiRoot, mapListPanel;
    TMP_InputField nameField, searchField, familyField;
    TMP_InputField sharePathField;
    TMP_Text status, enemyChoiceLabel, chestChoiceLabel;
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
    bool[] layerVisible = { true, true, true };
    float autosaveAt;
    string currentSavedName;
    bool mousePainting;
    Vector2Int? lastDragCell, suppressGridClick;
    RectTransform promptPanel;
    TMP_Text promptText;
    System.Action pendingConfirm;
    sealed class Snapshot { public MapCell[] ground, objects, overlay; public MapSpawn[] spawns; public MapChest[] chests; public MapGoal goal; }
    readonly Stack<Snapshot> undo = new Stack<Snapshot>();
    readonly Stack<Snapshot> redo = new Stack<Snapshot>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) rotation = (rotation + 1) % 4;
        if (Input.GetKeyDown(KeyCode.F)) flip = !flip;
        if (Input.GetKeyDown(KeyCode.E)) eraseMode = !eraseMode;
        if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl)) Undo();
        if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftControl)) Redo();
        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl)) Save();
        if (Time.unscaledTime >= autosaveAt)
        {
            autosaveAt = Time.unscaledTime + 60f;
            if (dirty) { MapStore.Save(map, "__autosave", out _); status.text = "Còpia automàtica guardada."; }
        }
        bool typing = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
        if (!typing && grid != null && EventSystem.current != null)
        {
            if (Input.GetMouseButtonDown(0)) { mousePainting = true; lastDragCell = null; }
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
            float zoom = Input.mouseScrollDelta.y;
            if (Mathf.Abs(zoom) > .01f) grid.localScale = Vector3.one * Mathf.Clamp(grid.localScale.x + zoom * .08f, .5f, 2f);
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
        UIKit.Button(canvas.transform, "NOU 16×16", new Vector2(-250, 360), new Vector2(140, 42), ConfirmNewMap, UIKit.Style.Normal, 16);
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
        status = UIKit.Text(canvas.transform, "", new Vector2(0, 315), new Vector2(800, 30), 16, TextAlignmentOptions.Center, UIKit.Muted);
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
        string[] toolNames = { "PINTAR", "ESBORRAR", "OMPLIR", "RECTANGLE", "R. PLE/BUIT", "LÍNIA", "COMPTAGOTES", "DESFER", "REFER", "CAPA", "JUGADOR", "ENEMIC", "COFRE", "FINAL", "MERCADER", "COPIAR", "ENGANXAR", "V. CAPA" };
        float toolX = -575;
        foreach (string toolName in toolNames)
        {
            string action = toolName;
            UIKit.Button(canvas.transform, action, new Vector2(toolX, 270), new Vector2(82, 34), () => SelectTool(action), UIKit.Style.Normal, 11);
            toolX += 88;
        }
        var panel = UIKit.Box(canvas.transform, new Vector2(-470, -20), new Vector2(245, 580), UIKit.Panel, "Panell paleta");
        searchField = UIKit.Input(panel, new Vector2(0, 258), new Vector2(220, 34), "Busca bloc", 40, fontSize: 14);
        familyField = UIKit.Input(panel, new Vector2(0, 216), new Vector2(220, 34), "Família (opcional)", 30, fontSize: 13);
        searchField.onValueChanged.AddListener(_ => BuildPalette());
        familyField.onValueChanged.AddListener(_ => BuildPalette());
        palette = UIKit.ScrollList(panel, new Vector2(0, -35), new Vector2(225, 455), out _, 2);
        grid = UIKit.Empty(canvas.transform, "Graella");
        promptPanel = UIKit.Box(canvas.transform, Vector2.zero, new Vector2(420, 200), UIKit.Panel, "Confirmació");
        promptText = UIKit.Text(promptPanel, "", new Vector2(0, 45), new Vector2(380, 60), 20, TextAlignmentOptions.Center, UIKit.TextColor);
        UIKit.Button(promptPanel, "CONFIRMAR", new Vector2(-95, -55), new Vector2(160, 46), () => { var action = pendingConfirm; promptPanel.gameObject.SetActive(false); pendingConfirm = null; action?.Invoke(); }, UIKit.Style.Danger, 16);
        UIKit.Button(promptPanel, "CANCEL·LAR", new Vector2(95, -55), new Vector2(160, 46), () => { promptPanel.gameObject.SetActive(false); pendingConfirm = null; }, UIKit.Style.Normal, 16);
        promptPanel.gameObject.SetActive(false);
        BuildPalette();
        DrawGrid();
        if (MapStore.Load("__autosave", out _, out _)) ShowPrompt("Hi ha una còpia automàtica. La vols recuperar?", RecoverAutosave);
    }

    void NewMap(int width, int height)
    {
        currentSavedName = null; dirty = false;
        undo.Clear(); redo.Clear();
        map = new MapData { width = width, height = height, layers = new MapLayers() };
        DrawGrid();
        status.text = "Mapa nou " + width + "×" + height;
    }

    void BuildPalette()
    {
        UIKit.Clear(palette);
        string search = searchField.text.Trim();
        string family = familyField.text.Trim();
        var blocks = BlockCatalog.Instance.blocks.Where(b => b != null)
            .Where(b => string.IsNullOrEmpty(search) || b.displayName.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(b => string.IsNullOrEmpty(family) || b.family.IndexOf(family, System.StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(b => b.family).ThenBy(b => b.displayName);
        int rowIndex = 0;
        foreach (var block in blocks)
        {
            var row = UIKit.ListRow(palette, 38, rowIndex++ % 2 == 0 ? UIKit.Row : UIKit.RowAlt);
            row.name = "Bloc " + block.id;
            var button = row.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => selected = block);
            UIKit.Picture(row, block.sprite, new Vector2(-84, 0), new Vector2(26, 26));
            var label = UIKit.Text(row, block.displayName, new Vector2(35, 0), new Vector2(158, 32), 13, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }

    void DrawGrid()
    {
        UIKit.Clear(grid);
        float cell = Mathf.Min(650f / map.width, 540f / map.height);
        for (int y = 0; y < map.height; y++)
        for (int x = 0; x < map.width; x++)
        {
            int cx = x, cy = y;
            var button = UIKit.Button(grid, "", new Vector2(-150 + x * cell, 240 - y * cell), new Vector2(cell - 2, cell - 2),
                () => { if (suppressGridClick == new Vector2Int(cx, cy)) { suppressGridClick = null; return; } suppressGridClick = null; Paint(cx, cy); }, UIKit.Style.Normal, 10);
            button.rect.name = "Cel·la " + cx + "," + cy;
            var cells = VisibleCells();
            var item = MapAutoTile160.Resolve(cells.FirstOrDefault(c => c.x == cx && c.y == cy), cells);
            if (!string.IsNullOrEmpty(item.blockId))
            {
                var block = BlockCatalog.Instance.blocks.FirstOrDefault(b => b.id == item.blockId);
                if (block != null) { var image = UIKit.Picture(button.rect, block.sprite, Vector2.zero, new Vector2(cell - 3, cell - 3)); image.rectTransform.localEulerAngles = new Vector3(0, 0, -90 * item.rot); image.rectTransform.localScale = new Vector3(item.flip ? -1 : 1, 1, 1); }
                else
                {
                    button.image.color = new Color(.55f, .18f, .55f);
                    UIKit.Text(button.rect, "?", Vector2.zero, new Vector2(cell - 3, cell - 3), Mathf.Max(10, cell * .55f), TextAlignmentOptions.Center, Color.white);
                }
            }
        }
    }

    void Paint(int x, int y)
    {
        if (placeMode != PlaceMode.Blocks) { PlaceObject(x, y); return; }
        if (tool == EditTool.Copy) { CopyRegion(x, y); return; }
        if (tool == EditTool.Paste) { PasteRegion(x, y); return; }
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
        var active = ActiveCells();
        var cells = new List<MapCell>(active);
        PushUndo();
        cells.RemoveAll(c => c.x == x && c.y == y);
        if (tool != EditTool.Erase && !eraseMode && selected != null) cells.Add(new MapCell { x = x, y = y, blockId = selected.id, rot = rotation, flip = flip });
        SetActiveCells(cells.ToArray());
        DrawGrid();
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
            "DESFER" => EditTool.Paint, "REFER" => EditTool.Paint, _ => EditTool.Paint
        };
        if (name == "DESFER") Undo();
        if (name == "REFER") Redo();
        if (name == "CAPA") layer = (EditLayer)(((int)layer + 1) % 3);
        status.text = "Eina: " + name + (name == "CAPA" ? " · " + layer : "");
    }
    void PlaceObject(int x, int y)
    {
        PushUndo();
        if (placeMode == PlaceMode.Player)
        {
            map.spawns = map.spawns.Where(s => s.kind != "player").Concat(new[] { new MapSpawn { x = x, y = y, kind = "player" } }).ToArray();
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
        RunStats.DontSave = true;
        SceneManager.LoadScene("Dungeon");
    }
    void CopyRegion(int x, int y)
    {
        if (!dragStart.HasValue) { dragStart = new Vector2Int(x,y); status.text = "Tria l'altre cantó de la selecció."; return; }
        int minX=Mathf.Min(x,dragStart.Value.x), maxX=Mathf.Max(x,dragStart.Value.x), minY=Mathf.Min(y,dragStart.Value.y), maxY=Mathf.Max(y,dragStart.Value.y);
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
        var result=new List<MapCell>(target); foreach(var c0 in source) { var c=c0; c.x+=x; c.y+=y; if(c.x<0||c.y<0||c.x>=map.width||c.y>=map.height) continue; result.RemoveAll(v=>v.x==c.x&&v.y==c.y); result.Add(c); } return result.ToArray();
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
    bool SaveAs(string target) { map.name = target; if (MapStore.Save(map, map.name, out var error)) { currentSavedName=map.name; dirty=false; status.text = "Guardat: " + map.name; return true; } status.text = error; return false; }
    void ShowPrompt(string message, System.Action action) { pendingConfirm=action; promptText.text=message; promptPanel.gameObject.SetActive(true); }
    void ConfirmNewMap() { if (dirty) ShowPrompt("Hi ha canvis sense guardar. Vols començar un mapa nou?", () => NewMap(16,16)); else NewMap(16,16); }
    void RecoverAutosave() { if (MapStore.Load("__autosave",out var recovered,out var error)) { map=recovered; nameField.text=map.name; currentSavedName=null; dirty=true; DrawGrid(); status.text="Còpia automàtica recuperada; guarda-la amb un nom."; } else status.text=error; }
    void ConfirmExit() { if (dirty) ShowPrompt("Hi ha canvis sense guardar. Vols eixir igualment?", () => SceneManager.LoadScene("MainMenu")); else SceneManager.LoadScene("MainMenu"); }
    void ExportCode()
    {
        if (!MapShare160.Encode(map, out var code, out var error)) { status.text = error; return; }
        GUIUtility.systemCopyBuffer = code; status.text = "Codi CVM1 copiat al portapapers.";
    }
    void ImportCode()
    {
        if (!MapShare160.Decode(GUIUtility.systemCopyBuffer, out var imported, out var error)) { status.text = error; return; }
        map = imported; nameField.text = map.name; currentSavedName=null; dirty=true; undo.Clear(); redo.Clear(); DrawGrid(); status.text = "Codi importat. Guarda'l per conservar-lo.";
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
        map = imported; nameField.text = map.name; currentSavedName=null; dirty=true; undo.Clear(); redo.Clear(); DrawGrid(); status.text = "Fitxer importat: " + map.name;
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
        if (MapStore.Load(savedName, out map, out var error)) { nameField.text = savedName; currentSavedName = savedName; dirty = false; DrawGrid(); status.text = "Obert: " + savedName; }
        else status.text = error;
    }
}
