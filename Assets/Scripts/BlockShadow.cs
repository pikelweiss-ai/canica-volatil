using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockShadow : MonoBehaviour
{
    public Tilemap wallsTilemap;
    public Tilemap groundTilemap;
    public Tilemap shadowTilemap;
    public RuleTile shadowTile; // Ara és RuleTile, com ja has fet tu

    void Start()
    {
        foreach (Vector3Int pos in wallsTilemap.cellBounds.allPositionsWithin)
        {
            if (!wallsTilemap.HasTile(pos))
                continue;

            Vector3Int below = pos + new Vector3Int(0, -1, 0);

            if (groundTilemap.HasTile(below) && !wallsTilemap.HasTile(below))
            {
                shadowTilemap.SetTile(below, shadowTile);
            }
        }
    }
}