using UnityEngine;
using UnityEngine.Tilemaps;

// Tile genèric que, en jugar, es converteix en un objecte de deveres (caixa, barril, punxes...).
// - A l'editor es veu 'previewSprite' per a pintar-lo amb la Tile Palette.
// - En jugar, crea 'prefab' al centre de la cel·la i el dibuix del tile s'amaga.
[CreateAssetMenu(fileName = "TileObjecte", menuName = "Canica/Tile d'objecte")]
public class PrefabTile : TileBase
{
    public Sprite previewSprite;
    public GameObject prefab;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = Application.isPlaying ? null : previewSprite;
        tileData.color = Color.white;
        tileData.transform = Matrix4x4.identity;
        tileData.gameObject = prefab;
        tileData.colliderType = Tile.ColliderType.None; // La hitbox la porta el prefab
        tileData.flags = TileFlags.LockTransform | TileFlags.InstantiateGameObjectRuntimeOnly;
    }
}
