using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Navegació per caselles compartida per tots els enemics.
// - Ground: el tilemap de terra (marca la graella)
// - Obstacles: parets, aigua, arbres, portes... (no s'hi pot passar)
// Es configura sol buscant els tilemaps pel nom, o la masmorra els registra a mà.
public static class TileNav
{
    public static Tilemap Ground;
    public static readonly List<Tilemap> Obstacles = new List<Tilemap>();

    static readonly string[] ObstacleNames = { "Tilemap Walls", "Tilemap Liquid", "Tilemap Trees", "Tilemap Portes" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        Ground = null;
        Obstacles.Clear();
        Blockers.Clear();
    }

    // La masmorra ho configura a mà. En canviar d'escena, el Ground vell es destrueix
    // (Ground == null) i tot es torna a configurar.
    public static void SetGround(Tilemap ground)
    {
        Ground = ground;
        Obstacles.Clear();
        Blockers.Clear();
    }

    // Busca els tilemaps per nom si encara no estan
    public static bool Ready()
    {
        if (Ground != null) return true;

        GameObject g = GameObject.Find("Tilemap Ground");
        if (g == null) return false;
        SetGround(g.GetComponent<Tilemap>());

        foreach (string n in ObstacleNames)
        {
            GameObject o = GameObject.Find(n);
            if (o != null) Register(o.GetComponent<Tilemap>());
        }
        return Ground != null;
    }

    public static void Register(Tilemap t)
    {
        if (t != null && !Obstacles.Contains(t)) Obstacles.Add(t);
    }

    public static Vector3Int Cell(Vector2 world) => Ground.WorldToCell(world);
    public static Vector2 Center(Vector3Int cell) => Ground.GetCellCenterWorld(cell);

    // Objectes que ocupen una casella sense ser tiles (cofres, caixes, barrils...)
    static readonly Dictionary<Vector3Int, int> Blockers = new Dictionary<Vector3Int, int>();

    public static void AddBlocker(Vector2 world)
    {
        if (!Ready()) return;
        Vector3Int c = Cell(world);
        Blockers[c] = Blockers.TryGetValue(c, out int n) ? n + 1 : 1;
    }

    public static void RemoveBlocker(Vector2 world)
    {
        if (Ground == null) return;
        Vector3Int c = Cell(world);
        if (!Blockers.TryGetValue(c, out int n)) return;
        if (n <= 1) Blockers.Remove(c); else Blockers[c] = n - 1;
    }

    // Casella on no es pot estar: sense terra, o amb un obstacle damunt
    public static bool IsBlocked(Vector3Int cell, bool ignoreWater = false)
    {
        if (Ground == null) return false;
        if (!Ground.HasTile(cell)) return true;
        if (Blockers.ContainsKey(cell)) return true;
        for (int i = 0; i < Obstacles.Count; i++)
            if (Obstacles[i] != null && Obstacles[i].HasTile(cell) &&
                !(ignoreWater && IsWaterTilemap(Obstacles[i]) && !(Obstacles[i].GetTile(cell) is DamageTile))) return true;
        return false;
    }

    public static bool IsWaterTilemap(UnityEngine.Tilemaps.Tilemap map) =>
        map != null && (map == DungeonGenerator.Instance?.liquid || map.name == "Tilemap Liquid");

    public static bool IsBlockedWorld(Vector2 pos) => IsBlocked(Cell(pos));

    // Un cercle de radi 'r' cap en 'pos' sense tocar caselles bloquejades
    public static bool CircleFree(Vector2 pos, float r)
    {
        return !IsBlockedWorld(pos)
            && !IsBlockedWorld(pos + new Vector2(r, 0)) && !IsBlockedWorld(pos + new Vector2(-r, 0))
            && !IsBlockedWorld(pos + new Vector2(0, r)) && !IsBlockedWorld(pos + new Vector2(0, -r));
    }

    // Línia de visió: cap casella bloquejada entre a i b (passos de mitja casella)
    public static bool LineOfSight(Vector2 a, Vector2 b, float radius = 0f)
    {
        if (Ground == null) return true;
        float dist = Vector2.Distance(a, b);
        int steps = Mathf.CeilToInt(dist / 0.25f);
        for (int i = 1; i < steps; i++)
        {
            Vector2 p = Vector2.Lerp(a, b, i / (float)steps);
            if (radius > 0f ? !CircleFree(p, radius) : IsBlockedWorld(p)) return false;
        }
        return true;
    }

    public static Vector3 DropDestination(Vector3 from, Vector3 to)
    {
        if (!Ready()) return to;
        if (IsBlockedWorld(from))
        {
            Vector2 free = Center(NearestFree(Cell(from)));
            return new Vector3(free.x, free.y, from.z);
        }

        int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / 0.06f));
        Vector3 safe = from;
        for (int i = 1; i <= steps; i++)
        {
            Vector3 candidate = Vector3.Lerp(from, to, i / (float)steps);
            if (!CircleFree(candidate, 0.12f)) break;
            safe = candidate;
        }
        return safe;
    }

    // Cerca la casella lliure més pròxima (en espiral)
    public static Vector3Int NearestFree(Vector3Int cell, int maxRadius = 6)
    {
        if (!IsBlocked(cell)) return cell;
        for (int r = 1; r <= maxRadius; r++)
            for (int x = -r; x <= r; x++)
                for (int y = -r; y <= r; y++)
                {
                    if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue;
                    Vector3Int c = cell + new Vector3Int(x, y, 0);
                    if (!IsBlocked(c)) return c;
                }
        return cell;
    }

    // ---------- A* en 8 direccions (sense tallar cantons) ----------

    static readonly Vector3Int[] Dirs =
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(1, 1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(-1, -1, 0)
    };

    public static List<Vector2> FindPath(Vector2 fromWorld, Vector2 toWorld, int maxNodes = 2500)
    {
        var result = new List<Vector2>();
        if (Ground == null) return result;

        Vector3Int start = Cell(fromWorld);
        Vector3Int goal = NearestFree(Cell(toWorld));
        if (start == goal) { result.Add(toWorld); return result; }

        var open = new List<Vector3Int> { start };
        var came = new Dictionary<Vector3Int, Vector3Int>();
        var g = new Dictionary<Vector3Int, float> { [start] = 0f };
        var f = new Dictionary<Vector3Int, float> { [start] = H(start, goal) };
        var closed = new HashSet<Vector3Int>();

        Vector3Int best = start;
        float bestH = H(start, goal);
        int expanded = 0;

        while (open.Count > 0 && expanded < maxNodes)
        {
            // Node amb menor f (llista xicoteta: busca lineal és prou ràpida)
            int bi = 0;
            for (int i = 1; i < open.Count; i++) if (f[open[i]] < f[open[bi]]) bi = i;
            Vector3Int cur = open[bi];
            open.RemoveAt(bi);
            if (cur == goal) { best = goal; break; }
            closed.Add(cur);
            expanded++;

            float h = H(cur, goal);
            if (h < bestH) { bestH = h; best = cur; }

            foreach (Vector3Int d in Dirs)
            {
                Vector3Int n = cur + d;
                if (closed.Contains(n) || IsBlocked(n)) continue;
                // Diagonal: no tallar cantons de parets
                if (d.x != 0 && d.y != 0 && (IsBlocked(cur + new Vector3Int(d.x, 0, 0)) || IsBlocked(cur + new Vector3Int(0, d.y, 0))))
                    continue;

                float cost = g[cur] + ((d.x != 0 && d.y != 0) ? 1.414f : 1f);
                if (g.TryGetValue(n, out float old) && cost >= old) continue;
                came[n] = cur;
                g[n] = cost;
                f[n] = cost + H(n, goal);
                if (!open.Contains(n)) open.Add(n);
            }
        }

        // Si no arriba, va fins a la casella més pròxima que ha trobat
        Vector3Int end = best;
        var cells = new List<Vector3Int>();
        while (end != start && came.ContainsKey(end))
        {
            cells.Add(end);
            end = came[end];
        }
        cells.Reverse();
        foreach (Vector3Int c in cells) result.Add(Center(c));
        return result;
    }

    static float H(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x), dy = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dy) + 0.414f * Mathf.Min(dx, dy);
    }
}
