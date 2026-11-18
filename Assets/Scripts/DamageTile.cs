using UnityEngine;
using UnityEngine.Tilemaps;

// Tile de terra que fa mal si el trepitges (lava...). El jugador ho comprova amb PlayerHazards.
[CreateAssetMenu(fileName = "TileDany", menuName = "Canica/Tile que fa mal")]
public class DamageTile : Tile
{
    public int damage = 1;
    [Header("Connexions: N, E, S, W i cantonades")]
    public Sprite[] connections;
    static readonly Vector3Int[] Offsets = { Vector3Int.up,Vector3Int.right,Vector3Int.down,Vector3Int.left,
        new Vector3Int(1,1,0),new Vector3Int(1,-1,0),new Vector3Int(-1,-1,0),new Vector3Int(-1,1,0) };
    public static int CanonicalMask(int mask)
    {
        int sides = mask & 15;
        return sides | ((sides & 3) == 3 ? mask & 16 : 0) | ((sides & 6) == 6 ? mask & 32 : 0)
            | ((sides & 12) == 12 ? mask & 64 : 0) | ((sides & 9) == 9 ? mask & 128 : 0);
    }
    public override void GetTileData(Vector3Int position, ITilemap map, ref TileData data)
    {
        base.GetTileData(position,map,ref data);
        if(connections == null || connections.Length != 256) return;
        int mask = 0;
        for(int i=0;i<Offsets.Length;i++) if(map.GetTile(position+Offsets[i]) == this) mask |= 1 << i;
        var connected = connections[CanonicalMask(mask)];
        if(connected != null) data.sprite = connected;
    }
    public override void RefreshTile(Vector3Int position, ITilemap map)
    {
        for(int y=-1;y<=1;y++) for(int x=-1;x<=1;x++) map.RefreshTile(position+new Vector3Int(x,y,0));
    }
}
