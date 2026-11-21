using System.Collections.Generic;
using UnityEngine;

public partial class DungeonGenerator
{
    readonly Dictionary<Vector3Int, BlockData> placedCatalogBlocks = new();
    Vector3Int lastBlockCell = new(int.MaxValue, int.MaxValue, 0);

    void TrackBlock(Vector2Int cell, Sprite sprite)
    {
        var catalog = BlockCatalog.Instance;
        if (catalog == null) return;
        var block = catalog.Find(sprite);
        if (block != null) placedCatalogBlocks[ToCell(cell)] = block;
    }

    void DiscoverBlocks(Vector3Int cell)
    {
        if (cell == lastBlockCell) return;
        lastBlockCell = cell;
        const int radius = 6;
        var catalog = BlockCatalog.Instance;
        if (catalog == null) return;
        var changed = false;
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius) continue;
                var at = cell + new Vector3Int(x, y, 0);
                var point = ground.GetCellCenterWorld(at);
                var midpoint = Vector3.Lerp(player.position, point, .82f);
                if (!TileNav.LineOfSight(player.position, midpoint, ignoreWater: true)) continue;
                if (placedCatalogBlocks.TryGetValue(at, out var obj)) changed |= Almanac.SawBlock(obj.id);
                var terrain = catalog.Find(ground.GetSprite(at));
                var wall = catalog.Find(walls.GetSprite(at));
                var pool = liquid != null ? catalog.Find(liquid.GetSprite(at)) : null;
                if (terrain != null) changed |= Almanac.SawBlock(terrain.id);
                if (wall != null) changed |= Almanac.SawBlock(wall.id);
                if (pool != null) changed |= Almanac.SawBlock(pool.id);
            }
        if (changed) Almanac.SaveBlocks();
    }
}
