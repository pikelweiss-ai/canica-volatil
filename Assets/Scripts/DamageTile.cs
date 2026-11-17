using UnityEngine;
using UnityEngine.Tilemaps;

// Tile de terra que fa mal si el trepitges (lava...). El jugador ho comprova amb PlayerHazards.
[CreateAssetMenu(fileName = "TileDany", menuName = "Canica/Tile que fa mal")]
public class DamageTile : Tile
{
    public int damage = 1;
}
