using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MapPlaytest : MonoBehaviour
{
    public static string MapName;
    public static bool ReturnToEditor;
    MapData map;
    Vector2Int player;
    TMP_Text status;
    void Start()
    {
        if (!MapStore.Load(MapName, out map, out var error)) { SceneManager.LoadScene("Creador"); return; }
        var spawn = map.spawns.FirstOrDefault(s => s.kind == "player"); player = map.spawns.Any(s => s.kind == "player") ? new Vector2Int(spawn.x, spawn.y) : new Vector2Int(0, 0);
        var canvas = UIKit.Canvas("Prova mapa", 20);
        var bg = UIKit.Box(canvas.transform, Vector2.zero, Vector2.zero, UIKit.Ink, "Fons"); UIKit.Stretch(bg);
        UIKit.Text(canvas.transform, "PROVAR: " + map.name, new Vector2(0, 410), new Vector2(800, 50), 30, TextAlignmentOptions.Center, UIKit.Gold);
        status = UIKit.Text(canvas.transform, "Fletxes o WASD per moure't", new Vector2(0, 355), new Vector2(800, 32), 17, TextAlignmentOptions.Center, UIKit.TextColor);
        UIKit.Button(canvas.transform, "TORNAR A L'EDITOR", new Vector2(0, -405), new Vector2(260, 48), Return, UIKit.Style.Primary, 17);
        Draw(canvas.transform);
        if (map.goal != null && player.x == map.goal.x && player.y == map.goal.y) status.text = "MAPA COMPLETAT!";
    }
    void Update()
    {
        Vector2Int d = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) d = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) d = Vector2Int.right;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) d = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) d = Vector2Int.down;
        if (d == Vector2Int.zero) return;
        player.x = Mathf.Clamp(player.x + d.x, 0, map.width - 1); player.y = Mathf.Clamp(player.y + d.y, 0, map.height - 1);
        if (map.goal != null && player.x == map.goal.x && player.y == map.goal.y) status.text = "MAPA COMPLETAT!";
        Draw(GameObject.Find("Prova mapa").transform);
    }
    void Draw(Transform canvas)
    {
        var old = canvas.Find("Graella"); if (old != null) Destroy(old.gameObject);
        var grid = UIKit.Empty(canvas, "Graella");
        float size = Mathf.Min(680f / map.width, 600f / map.height);
        for (int y = 0; y < map.height; y++) for (int x = 0; x < map.width; x++)
        {
            var cell = UIKit.Box(grid, new Vector2(-((map.width - 1) * size) / 2 + x * size, 250 - y * size), new Vector2(size - 1, size - 1), new Color(.12f,.17f,.2f), "Cel·la");
            var all = map.layers.ground.Concat(map.layers.objects).Concat(map.layers.overlay).ToArray();
            var block = MapAutoTile160.Resolve(all.FirstOrDefault(c => c.x == x && c.y == y), all);
            if (!string.IsNullOrEmpty(block.blockId))
            {
                var data = BlockCatalog.Instance.blocks.FirstOrDefault(b => b.id == block.blockId);
                if (data != null) { var image = UIKit.Picture(cell, data.sprite, Vector2.zero, new Vector2(size - 2, size - 2)); image.rectTransform.localEulerAngles = new Vector3(0, 0, -90 * block.rot); image.rectTransform.localScale = new Vector3(block.flip ? -1 : 1, 1, 1); }
            }
            if (map.goal != null && map.goal.x == x && map.goal.y == y) cell.GetComponent<UnityEngine.UI.Image>().color = Color.green;
            if (map.spawns.Any(s => s.kind == "enemy" && s.x == x && s.y == y)) cell.GetComponent<UnityEngine.UI.Image>().color = Color.red;
            if (map.chests.Any(c => c.x == x && c.y == y)) cell.GetComponent<UnityEngine.UI.Image>().color = UIKit.Gold;
            if (player.x == x && player.y == y) cell.GetComponent<UnityEngine.UI.Image>().color = Color.cyan;
        }
    }
    void Return() { ReturnToEditor = true; SceneManager.LoadScene("Creador"); }
}
