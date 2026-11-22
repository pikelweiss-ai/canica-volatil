using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MapEditor : MonoBehaviour
{
    enum EditTool { Paint, Erase, Fill, Rectangle, Line, Eyedropper }
    enum EditLayer { Ground, Objects, Overlay }
    enum PlaceMode { Blocks, Player, Enemy, Chest, Goal }
    MapData map;
    BlockData selected;
    Transform grid, palette;
    TMP_InputField nameField, searchField, familyField;
    TMP_Text status;
    int rotation;
    bool flip, eraseMode;
    EditTool tool;
    EditLayer layer;
    PlaceMode placeMode;
    Vector2Int? dragStart;
    sealed class Snapshot { public MapCell[] ground, objects, overlay; }
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
    }

    void Start()
    {
        map = new MapData();
        if (MapPlaytest.ReturnToEditor && MapStore.Load(MapPlaytest.MapName, out var restored, out _)) map = restored;
        MapPlaytest.ReturnToEditor = false;
        var canvas = UIKit.Canvas("Creador UI", 20);
        var background = UIKit.Box(canvas.transform, Vector2.zero, Vector2.zero, UIKit.Ink, "Fons");
        UIKit.Stretch(background);
        UIKit.Text(canvas.transform, "CREADOR DE MAPES", new Vector2(0, 420), new Vector2(900, 58), 34,
            TextAlignmentOptions.Center, UIKit.Gold);
        nameField = UIKit.Input(canvas.transform, new Vector2(-550, 360), new Vector2(260, 42), "Nom del mapa", 40, fontSize: 18);
        UIKit.Button(canvas.transform, "NOU 16×16", new Vector2(-250, 360), new Vector2(140, 42), () => NewMap(16, 16), UIKit.Style.Normal, 16);
        UIKit.Button(canvas.transform, "GUARDAR", new Vector2(-90, 360), new Vector2(130, 42), Save, UIKit.Style.Primary, 16);
        UIKit.Button(canvas.transform, "OBRIR", new Vector2(55, 360), new Vector2(110, 42), Load, UIKit.Style.Normal, 16);
        UIKit.Button(canvas.transform, "ROTAR", new Vector2(185, 360), new Vector2(110, 42), () => rotation = (rotation + 1) % 4, UIKit.Style.Normal, 16);
        UIKit.Button(canvas.transform, "PROVAR", new Vector2(315, 360), new Vector2(115, 42), Playtest, UIKit.Style.Primary, 16);
        UIKit.Button(canvas.transform, "EIXIR", new Vector2(550, 360), new Vector2(120, 42),
            () => SceneManager.LoadScene("MainMenu"), UIKit.Style.Ghost, 16);
        status = UIKit.Text(canvas.transform, "", new Vector2(0, 315), new Vector2(800, 30), 16, TextAlignmentOptions.Center, UIKit.Muted);
        string[] toolNames = { "PINTAR", "ESBORRAR", "OMPLIR", "RECTANGLE", "LÍNIA", "COMPTAGOTES", "DESFER", "REFER", "CAPA", "JUGADOR", "ENEMIC", "COFRE", "FINAL" };
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
        BuildPalette();
        DrawGrid();
    }

    void NewMap(int width, int height)
    {
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
                () => Paint(cx, cy), UIKit.Style.Normal, 10);
            button.rect.name = "Cel·la " + cx + "," + cy;
            var cells = ActiveCells();
            var item = MapAutoTile160.Resolve(cells.FirstOrDefault(c => c.x == cx && c.y == cy), cells);
            if (!string.IsNullOrEmpty(item.blockId))
            {
                var block = BlockCatalog.Instance.blocks.FirstOrDefault(b => b.id == item.blockId);
                if (block != null) { var image = UIKit.Picture(button.rect, block.sprite, Vector2.zero, new Vector2(cell - 3, cell - 3)); image.rectTransform.localEulerAngles = new Vector3(0, 0, -90 * item.rot); image.rectTransform.localScale = new Vector3(item.flip ? -1 : 1, 1, 1); }
            }
        }
    }

    void Paint(int x, int y)
    {
        if (placeMode != PlaceMode.Blocks) { PlaceObject(x, y); return; }
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
    void SetActiveCells(MapCell[] cells)
    {
        if (layer == EditLayer.Ground) map.layers.ground = cells;
        else if (layer == EditLayer.Objects) map.layers.objects = cells;
        else map.layers.overlay = cells;
    }
    void SelectTool(string name)
    {
        if (name == "JUGADOR") { placeMode = PlaceMode.Player; status.text = "Posa el punt inicial"; return; }
        if (name == "ENEMIC") { placeMode = PlaceMode.Enemy; status.text = "Posa un Llim"; return; }
        if (name == "COFRE") { placeMode = PlaceMode.Chest; status.text = "Posa un cofre comú"; return; }
        if (name == "FINAL") { placeMode = PlaceMode.Goal; status.text = "Posa o mou el portal final"; return; }
        placeMode = PlaceMode.Blocks;
        tool = name switch
        {
            "ESBORRAR" => EditTool.Erase, "OMPLIR" => EditTool.Fill, "RECTANGLE" => EditTool.Rectangle,
            "LÍNIA" => EditTool.Line, "COMPTAGOTES" => EditTool.Eyedropper,
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
            map.spawns = map.spawns.Concat(new[] { new MapSpawn { x = x, y = y, kind = "enemy", id = "Llim" } }).ToArray();
        else if (placeMode == PlaceMode.Chest)
            map.chests = map.chests.Concat(new[] { new MapChest { x = x, y = y, rarity = "Comu" } }).ToArray();
        else if (placeMode == PlaceMode.Goal) map.goal = new MapGoal { x = x, y = y };
        DrawGrid();
    }
    void Playtest()
    {
        Save();
        if (MapStore.Load(map.name, out var saved, out _)) { MapPlaytest.MapName = map.name; SceneManager.LoadScene("MapaProva"); }
    }
    void Fill(int x, int y)
    {
        if (selected == null) return;
        var cells = new List<MapCell>(ActiveCells());
        string original = cells.FirstOrDefault(c => c.x == x && c.y == y).blockId;
        if (original == selected.id) return;
        PushUndo();
        var visited = new HashSet<Vector2Int>(); var queue = new Queue<Vector2Int>(); queue.Enqueue(new Vector2Int(x, y));
        while (queue.Count > 0)
        {
            var p = queue.Dequeue(); if (!visited.Add(p) || p.x < 0 || p.y < 0 || p.x >= map.width || p.y >= map.height) continue;
            string id = cells.FirstOrDefault(c => c.x == p.x && c.y == p.y).blockId;
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
        for (int y = minY; y <= maxY; y++) for (int x = minX; x <= maxX; x++)
        {
            if (rectangle && x != minX && x != maxX && y != minY && y != maxY) continue;
            cells.RemoveAll(c => c.x == x && c.y == y);
            cells.Add(new MapCell { x = x, y = y, blockId = selected.id, rot = rotation, flip = flip });
        }
        SetActiveCells(cells.ToArray()); DrawGrid();
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

    Snapshot Capture() => new Snapshot { ground = map.layers.ground.ToArray(), objects = map.layers.objects.ToArray(), overlay = map.layers.overlay.ToArray() };
    void Restore(Snapshot s) { map.layers.ground = s.ground; map.layers.objects = s.objects; map.layers.overlay = s.overlay; }
    void PushUndo() { undo.Push(Capture()); if (undo.Count > 50) { var keep = undo.Reverse().Take(50).Reverse().ToArray(); undo.Clear(); foreach (var s in keep) undo.Push(s); } redo.Clear(); }

    void Save()
    {
        map.name = string.IsNullOrWhiteSpace(nameField.text) ? "Mapa" : nameField.text.Trim();
        if (MapStore.Save(map, map.name, out var error)) status.text = "Guardat: " + map.name;
        else status.text = error;
    }

    void Load()
    {
        string first = MapStore.List().FirstOrDefault();
        if (first == null) { status.text = "Encara no hi ha mapes guardats."; return; }
        if (MapStore.Load(first, out map, out var error)) { nameField.text = first; DrawGrid(); status.text = "Obert: " + first; }
        else status.text = error;
    }
}
