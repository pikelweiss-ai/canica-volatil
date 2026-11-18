using UnityEngine;

// Fons de pixel art darrere del terra, sense collisio ni efectes de joc.
[DefaultExecutionOrder(100)]
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
        b.backdrop.drawMode = SpriteDrawMode.Simple;
        var ground = owner.ground.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        b.backdrop.sortingLayerID = ground.sortingLayerID; b.backdrop.sortingOrder = ground.sortingOrder-30;
    }
    void LateUpdate()
    {
        Camera cam = Camera.main; if(cam == null) return;
        transform.position = new Vector3(cam.transform.position.x,cam.transform.position.y,0);
        float cover=Mathf.Max(cam.orthographicSize*2*cam.aspect/backdrop.sprite.bounds.size.x,cam.orthographicSize*2/backdrop.sprite.bounds.size.y)*1.03f;
        transform.localScale=Vector3.one*cover;
        var room = dungeon.CurrentRoom;
        float light = room != null && room.type == DungeonGenerator.RoomType.Treasure ? .9f : .75f;
        backdrop.color = Color.Lerp(Color.white,dungeon.Theme.lightColor,.3f)*light;
    }
}
