using System.Collections.Generic;
using UnityEngine;

// Particules cosmetiques per distancia recorreguda, amb reutilitzacio i sense collisio.
public class TrailParticleEmitter : MonoBehaviour
{
    class Particle { public GameObject go;public SpriteRenderer renderer;public Vector3 position,velocity;public float age,life,scale,angle; }
    readonly List<Particle> active=new List<Particle>();
    readonly Queue<Particle> pool=new Queue<Particle>();
    static readonly Dictionary<TrailShape,Sprite> sprites=new Dictionary<TrailShape,Sprite>();
    TrailShape shape;Color color;SpriteRenderer owner;
    Vector3 previous;float carry;int side;
    GameObject root;
    public void Init(TrailShape kind,Color tint,SpriteRenderer body){shape=kind;color=tint;owner=body;previous=transform.position;root=new GameObject("Particules de l'estela");}
    void LateUpdate()
    {
        float dt=Time.deltaTime;if(root==null || dt<=0)return;
        Vector3 delta=transform.position-previous;float distance=delta.magnitude;
        if(distance<3)
        {
            carry+=distance;float spacing=shape==TrailShape.Footsteps?.22f:.14f;
            int count=Mathf.Min(8,Mathf.FloorToInt(carry/spacing));carry-=count*spacing;
            for(int i=0;i<count;i++)Emit(Vector3.Lerp(previous,transform.position,(i+1f)/(count+1)),delta.normalized);
        }
        else carry=0;
        previous=transform.position;
        for(int i=active.Count-1;i>=0;i--)
        {
            var p=active[i];p.age+=dt;
            if(p.age>=p.life){p.go.SetActive(false);pool.Enqueue(p);active.RemoveAt(i);continue;}
            float t=p.age/p.life;p.position+=p.velocity*dt;p.go.transform.position=p.position;
            p.go.transform.localScale=Vector3.one*p.scale*(shape==TrailShape.Rings?Mathf.Lerp(.5f,2,t):1-t*.5f);
            p.go.transform.rotation=Quaternion.Euler(0,0,p.angle+(shape==TrailShape.Stars?t*120:0));
            Color c=CosmeticsDatabase.IsRainbow(color)?Color.HSVToRGB((Time.time*.5f+t*.2f)%1, .8f,1):color;c.a=(1-t)*.75f;p.renderer.color=c;
        }
    }
    void Emit(Vector3 position,Vector3 direction)
    {
        if(active.Count>=64)return;Particle p;
        if(pool.Count>0)p=pool.Dequeue();
        else {p=new Particle();p.go=new GameObject("Particula",typeof(SpriteRenderer));p.go.transform.SetParent(root.transform,false);p.renderer=p.go.GetComponent<SpriteRenderer>();}
        side=1-side;Vector3 cross=new Vector3(-direction.y,direction.x,0);
        p.position=position+cross*(side==0?-.10f:.10f);p.velocity=Vector3.zero;
        if(shape==TrailShape.Sparks)p.velocity=-direction*.65f+cross*(side==0?-.5f:.5f);
        p.age=0;p.life=shape==TrailShape.Footsteps?.65f:.4f;p.scale=shape==TrailShape.Dots?.22f:.4f;
        p.angle=shape==TrailShape.Footsteps?Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90:side*35;
        p.renderer.sprite=SpriteFor(shape);p.renderer.sortingLayerID=owner.sortingLayerID;p.renderer.sortingOrder=owner.sortingOrder-1;
        p.go.SetActive(true);active.Add(p);
    }
    public static Sprite SpriteFor(TrailShape shape)
    {
        if(sprites.TryGetValue(shape,out var sprite))return sprite;
        var tex=new Texture2D(8,8,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Point;
        for(int y=0;y<8;y++)for(int x=0;x<8;x++)
        {
            float d=Vector2.Distance(new Vector2(x,y),new Vector2(3.5f,3.5f));bool on;
            switch(shape)
            {
                case TrailShape.Rings:on=d>2.4f&&d<3.8f;break;
                case TrailShape.Stars:on=(x>=3&&x<=4)||(y>=3&&y<=4)||(Mathf.Abs(x-3.5f)+Mathf.Abs(y-3.5f)<3);break;
                case TrailShape.Zigzag:on=(y>4&&x==4)||(y==4&&x>=2&&x<=4)||(y<4&&x==2);break;
                case TrailShape.Footsteps:on=(y>=3&&x>=2&&x<=4)||(y==1&&x>=2&&x<=3);break;
                case TrailShape.Sparks:on=Mathf.Abs(x-y)<=1&&x>0&&x<7;break;
                default:on=d<2.5f;break;
            }
            tex.SetPixel(x,y,on?Color.white:Color.clear);
        }
        tex.Apply();sprite=Sprite.Create(tex,new Rect(0,0,8,8),Vector2.one*.5f,16);sprites[shape]=sprite;return sprite;
    }
    void OnDestroy(){if(root!=null)Destroy(root);}
}
