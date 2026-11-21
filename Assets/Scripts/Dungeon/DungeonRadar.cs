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
                    if(room!=null && IsChestRoom(room.type))c=new Color(0.5f,0.39f,0.15f); // v1.4.1: tota sala amb cofre ix marro
                    if(room!=null && room.type==DungeonGenerator.RoomType.Shop)c=new Color(.15f,.52f,.42f);
                    if(room!=null && room.type==DungeonGenerator.RoomType.Boss)c=new Color(0.48f,0.15f,0.22f);
                    if(gen.liquid!=null && gen.liquid.HasTile(cell))c=new Color(0.1f,0.5f,0.65f);
                    if(gen.doors!=null && gen.doors.HasTile(cell))c=new Color(0.95f,0.5f,0.15f);
                }
            }
            if(c.a>0f) c.a=known?0.78f:0.32f;
            pixels[y*Width+x]=c;
        }
        foreach (var cell in gen.KeyDoorCenters)
        {
            var delta = cell - center;
            int cx = Width / 2 + Mathf.RoundToInt(delta.x / scale);
            int cy = Height / 2 + Mathf.RoundToInt(delta.y / scale);
            for (int y = -3; y <= 3; y++)
                for (int x = -3; x <= 3; x++)
                {
                    bool body = y <= 0 && Mathf.Abs(x) <= 2 && y >= -2;
                    bool shackle = y >= 1 && (y == 3 ? Mathf.Abs(x) <= 1 : Mathf.Abs(x) == 2);
                    int px = cx + x, py = cy + y;
                    if ((body || shackle) && px >= 0 && px < Width && py >= 0 && py < Height)
                        pixels[py * Width + px] = new Color32(255, 212, 88, 255);
                }
        }
        // Online: els companys com a punts blau cel
        foreach(var av in NetShared.Avatars())
        {
            if(av==null || av.Down) continue;
            var d=gen.ground.WorldToCell(av.transform.position)-center;
            int px=Width/2+Mathf.RoundToInt(d.x/scale), py=Height/2+Mathf.RoundToInt(d.y/scale);
            for(int oy=-2;oy<=2;oy++)for(int ox=-2;ox<=2;ox++)
            {
                int x=Mathf.Clamp(px+ox,0,Width-1), y=Mathf.Clamp(py+oy,0,Height-1);
                bool edge=Mathf.Abs(ox)==2 || Mathf.Abs(oy)==2;
                pixels[y*Width+x]=edge?new Color32(10,20,30,255):new Color32(110,210,255,255);
            }
        }
        texture.SetPixels32(pixels);texture.Apply(false);
    }
    void OnDestroy(){if(texture!=null)Destroy(texture);}
    // Sales que generen un cofre al centre (DungeonGenerator): tresor, secreta i emboscada
    static bool IsChestRoom(DungeonGenerator.RoomType t)=>t==DungeonGenerator.RoomType.Treasure||t==DungeonGenerator.RoomType.Secreta||t==DungeonGenerator.RoomType.Emboscada;
}