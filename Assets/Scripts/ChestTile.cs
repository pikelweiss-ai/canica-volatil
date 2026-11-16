using UnityEngine;
using UnityEngine.Tilemaps;

// Tile per a pintar cofres amb la Tile Palette.
// - A l'editor es veu el dibuix del cofre (previewSprite) per a dissenyar el mapa.
// - En jugar, cada tile crea un cofre de deveres (chestPrefab) al centre de la cel·la,
//   amb la seua hitbox i el script Chest ja configurats. El dibuix del tile s'amaga sol.
[CreateAssetMenu(fileName = "TileCofre", menuName = "Canica/Tile de cofre")]
public class ChestTile : TileBase
{
    public Sprite previewSprite;   // El que es veu a la paleta i a l'escena mentre edites
    public GameObject chestPrefab; // El cofre real que apareix en jugar

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = previewSprite;
        tileData.color = Color.white;
        tileData.transform = Matrix4x4.identity;
        tileData.gameObject = chestPrefab;
        tileData.colliderType = Tile.ColliderType.None; // La hitbox la porta el prefab
        tileData.flags = TileFlags.LockTransform | TileFlags.InstantiateGameObjectRuntimeOnly;
    }
}
