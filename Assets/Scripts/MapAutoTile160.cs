using System.Linq;
using UnityEngine;

public static class MapAutoTile160
{
    public static bool IsPath(string id) => id != null && id.StartsWith("Natura/Gespa_cami_");
    public static MapCell Resolve(MapCell cell, MapCell[] cells)
    {
        if (!IsPath(cell.blockId) || cells == null) return cell;
        bool U = Has(cells, cell.x, cell.y + 1), R = Has(cells, cell.x + 1, cell.y), D = Has(cells, cell.x, cell.y - 1), L = Has(cells, cell.x - 1, cell.y);
        int count = (U?1:0)+(R?1:0)+(D?1:0)+(L?1:0);
        string part = "final"; int rot = U ? 0 : R ? 1 : D ? 2 : L ? 3 : cell.rot;
        if (count == 2)
        {
            if (U && D || L && R) { part = "recte"; rot = U && D ? 0 : 1; }
            else { part = "canto"; rot = U && R ? 0 : R && D ? 1 : D && L ? 2 : 3; }
        }
        else if (count == 3) { part = "T"; rot = !D ? 0 : !L ? 1 : !U ? 2 : 3; }
        else if (count == 4) { part = "creu"; rot = 0; }
        string id = "Natura/Gespa_cami_" + part;
        var resolved = BlockCatalog.Instance.blocks.FirstOrDefault(b => b.id == id);
        if (resolved != null) { cell.blockId = id; cell.rot = rot; }
        return cell;
    }
    static bool Has(MapCell[] cells, int x, int y) => cells.Any(c => c.x == x && c.y == y && IsPath(c.blockId));
    public static Rect RotateHitbox(Rect rect, int rotation)
    {
        rotation = ((rotation % 4) + 4) % 4;
        return rotation % 2 == 0 ? rect : new Rect(-rect.yMax, rect.xMin, rect.height, rect.width);
    }
}
