using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Radar de terreny orientat al nord: el jugador sempre queda al centre.
public class DungeonRadar : MonoBehaviour
{
    public DungeonGenerator gen;
    Texture2D texture; RawImage image; RectTransform frame; bool expanded; float refresh;
    readonly System.Collections.Generic.HashSet<Vector3Int> explored = new System.Collections.Generic.HashSet<Vector3Int>();
    const int Width=193, Height=153;
    void Start()
    {
        frame=(RectTransform)transform;
        var go=new GameObject("Terreny",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(transform,false);
        image=go.GetComponent<RawImage>();image.raycastTarget=false;
        var r=image.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(8,8);r.offsetMax=new Vector2(-8,-8);
        texture=new Texture2D(Width,Height,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;image.texture=texture;
        var marker=new GameObject("Jugador",typeof(RectTransform),typeof(Image));marker.transform.SetParent(transform,false);
        marker.GetComponent<Image>().color=new Color(1f,0.9f,0.2f);marker.GetComponent<Image>().raycastTarget=false;
        ((RectTransform)marker.transform).sizeDelta=new Vector2(7,7);
        SetSize();
    }
    void SetSize(){frame.sizeDelta=expanded?new Vector2(560,444):new Vector2(GameSettings.MapSize,GameSettings.MapSize*190f/240f);}
    void Update()
    {
        if(gen==null || gen.player==null || image==null)return;
        SetSize();
        if(GameControls.Down(9)){expanded=!expanded;SetSize();refresh=0;}
        refresh-=Time.unscaledDeltaTime;if(refresh>0)return;refresh=0.18f;
        var center=gen.ground.WorldToCell(gen.player.position);
        // Ajustar el pis complet al voltant del jugador; evita veure nomes una sala.
        float reachX=12, reachY=12;
        foreach(var room in gen.Rooms)
        {
            reachX=Mathf.Max(reachX,Mathf.Abs(room.rect.xMin-center.x)+4,Mathf.Abs(room.rect.xMax-center.x)+4);
            reachY=Mathf.Max(reachY,Mathf.Abs(room.rect.yMin-center.y)+4,Mathf.Abs(room.rect.yMax-center.y)+4);
        }
        float scale=Mathf.Max(reachX/(Width/2-2),reachY/(Height/2-2))*GameSettings.MapRange;
        var pixels=new Color32[Width*Height];
        for(int y=0;y<Height;y++)for(int x=0;x<Width;x++)
        {
            var cell=center+new Vector3Int(Mathf.RoundToInt((x-Width/2)*scale),Mathf.RoundToInt((y-Height/2)*scale),0);
            bool known=(cell-center).sqrMagnitude<100;
            DungeonGenerator.Room room=null;
            foreach(var r in gen.Rooms) if(r.rect.Contains(new Vector2Int(cell.x,cell.y))) {room=r;known|=r.visited;break;}
            if(known) explored.Add(cell); else known=explored.Contains(cell);
            Color c=Color.clear;
            {
                if(gen.walls.HasTile(cell))c=new Color(0.45f,0.5f,0.6f);
                else if(gen.ground.HasTile(cell))
                {
                    c=new Color(0.16f,0.23f,0.28f);
                    if(room!=null && room.type==DungeonGenerator.RoomType.Treasure)c=new Color(0.5f,0.39f,0.15f);
                    if(room!=null && room.type==DungeonGenerator.RoomType.Boss)c=new Color(0.48f,0.15f,0.22f);
                    if(gen.liquid!=null && gen.liquid.HasTile(cell))c=new Color(0.1f,0.5f,0.65f);
                    if(gen.doors!=null && gen.doors.HasTile(cell))c=new Color(0.95f,0.5f,0.15f);
                }
            }
            if(c.a>0f) c.a=known?0.78f:0.32f;
            pixels[y*Width+x]=c;
        }
        texture.SetPixels32(pixels);texture.Apply(false);
    }
    void OnDestroy(){if(texture!=null)Destroy(texture);}
}