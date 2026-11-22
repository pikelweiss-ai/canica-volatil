using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockShadow : MonoBehaviour
{
    public Tilemap wallsTilemap;
    public Tilemap groundTilemap;
    public Tilemap shadowTilemap;
    public RuleTile shadowTile;
    readonly List<Tile> generatedShadowTiles = new List<Tile>();

    void Start() => Rebuild();

    public void Rebuild()
    {
        if (wallsTilemap == null || groundTilemap == null || shadowTilemap == null || shadowTile == null) return;
        shadowTilemap.ClearAllTiles();
        foreach (var tile in generatedShadowTiles) if (tile != null) Destroy(tile);
        generatedShadowTiles.Clear();
        foreach (Vector3Int pos in wallsTilemap.cellBounds.allPositionsWithin)
        {
            if (!wallsTilemap.HasTile(pos)) continue;
            Vector3Int below = pos + new Vector3Int(0, -1, 0);
            if (!groundTilemap.HasTile(below) || wallsTilemap.HasTile(below)) continue;

            var sourceData = new TileData();
            shadowTile.GetTileData(below, shadowTilemap, ref sourceData);
            var rotatedShadow = ScriptableObject.CreateInstance<Tile>();
            rotatedShadow.sprite = sourceData.sprite;
            rotatedShadow.color = sourceData.color;
            rotatedShadow.transform = wallsTilemap.GetTransformMatrix(pos);
            rotatedShadow.colliderType = Tile.ColliderType.None;
            rotatedShadow.flags = TileFlags.None;
            shadowTilemap.SetTile(below, rotatedShadow);
            generatedShadowTiles.Add(rotatedShadow);
        }
    }
}
