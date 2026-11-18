using UnityEngine;

// Fons de pixel art darrere del terra, sense collisio ni efectes de joc.
public class DungeonBackdrop : MonoBehaviour
{
    DungeonGenerator dungeon;
    SpriteRenderer backdrop;
    public static void Attach(DungeonGenerator owner)
    {
        if(owner.Theme == null || owner.Theme.voidSprite == null) return;
        var go = new GameObject("Fons de l'ambient"); go.transform.SetParent(owner.transform,false);
        var b = go.AddComponent<DungeonBackdrop>(); b.dungeon = owner;
        b.backdrop = go.AddComponent<SpriteRenderer>(); b.backdrop.sprite = owner.Theme.voidSprite;
        b.backdrop.drawMode = SpriteDrawMode.Tiled;
        var ground = owner.ground.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        b.backdrop.sortingLayerID = ground.sortingLayerID; b.backdrop.sortingOrder = ground.sortingOrder-30;
    }
    void LateUpdate()
    {
        Camera cam = Camera.main; if(cam == null) return;
        transform.position = new Vector3(Mathf.Floor(cam.transform.position.x),Mathf.Floor(cam.transform.position.y),0);
        backdrop.size = new Vector2(cam.orthographicSize*2*cam.aspect+4,cam.orthographicSize*2+4);
        var room = dungeon.CurrentRoom;
        float light = room != null && room.type == DungeonGenerator.RoomType.Treasure ? .85f : .6f;
        backdrop.color = new Color(light,light,light,1);
    }
}
