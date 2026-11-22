using System.Linq;
using UnityEngine;

public static class MapAutoTile160
{
    public static bool IsPath(string id) => id != null && id.StartsWith("Natura/Gespa_cami_");
    public static MapCell Resolve(MapCell cell, MapCell[] cells)
    {
        string prefix = StylePrefix(cell.blockId);
        if (prefix == null || cells == null) return cell;
        bool path = IsPath(cell.blockId);
        bool U = Has(cells, cell.x, cell.y + 1, prefix), R = Has(cells, cell.x + 1, cell.y, prefix), D = Has(cells, cell.x, cell.y - 1, prefix), L = Has(cells, cell.x - 1, cell.y, prefix);
        int count = (U?1:0)+(R?1:0)+(D?1:0)+(L?1:0);
        if (count == 0) return cell;
        string part; int rot = 0;
        if (count == 2)
        {
            if (U && D || L && R)
            {
                if (path) { part = "recte"; rot = U && D ? 0 : 1; }
                else part = "recte_" + (U && D ? "V" : "H");
            }
            else if (path) { part = "canto"; rot = U && R ? 0 : R && D ? 1 : D && L ? 2 : 3; }
            else part = "canto_" + (U && R ? "NE" : U && L ? "NO" : D && R ? "SE" : "SO");
        }
        else if (count == 1)
        {
            if (path) { part = "final"; rot = U ? 0 : R ? 1 : D ? 2 : 3; }
            else part = "recte_" + (L || R ? "H" : "V");
        }
        else if (count == 3) { part = "T"; rot = !D ? 0 : !L ? 1 : !U ? 2 : 3; }
        else { part = "creu"; rot = 0; }
        string id = prefix + part;
        var resolved = BlockCatalog.Instance.blocks.FirstOrDefault(b => b.id == id);
        if (resolved != null) { cell.blockId = id; cell.rot = rot; }
        return cell;
    }
    static string StylePrefix(string id)
    {
        if (id == null) return null;
        if (id.StartsWith("Natura/Gespa_cami_")) return "Natura/Gespa_cami_";
        if (id.StartsWith("Oficina/Panell_") && id != "Oficina/Panell_tela") return "Oficina/Panell_";
        if (id.StartsWith("Laboratori/Tuberia_verda_")) return "Laboratori/Tuberia_verda_";
        if (id.StartsWith("Laboratori/Tuberia_")) return "Laboratori/Tuberia_";
        if (id.StartsWith("Port/Barana_")) return "Port/Barana_";
        return null;
    }
    static bool Has(MapCell[] cells, int x, int y, string prefix) => cells.Any(c => c.x == x && c.y == y && StylePrefix(c.blockId) == prefix);
    public static Rect RotateHitbox(Rect rect, int rotation)
    {
        rotation = ((rotation % 4) + 4) % 4;
        if (rotation % 2 == 0) return rect;
        return new Rect(rect.yMin, -rect.xMax, rect.height, rect.width);
    }

    public static Rect TransformHitbox(Rect rect, int rotation, bool flip)
    {
        if (flip) rect = new Rect(-rect.xMax, rect.yMin, rect.width, rect.height);
        return RotateHitbox(rect, rotation);
    }
}
