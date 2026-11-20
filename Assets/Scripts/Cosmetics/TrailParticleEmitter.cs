using System.Collections.Generic;
using UnityEngine;

// Partícules cosmètiques per distància recorreguda.
public class TrailParticleEmitter : MonoBehaviour
{
    sealed class Particle
    {
        public SpriteRenderer renderer;
        public Vector3 position, velocity;
        public float age, life, scale, angle;
    }

    readonly List<Particle> active = new();
    readonly Queue<Particle> pool = new();
    static readonly Dictionary<TrailShape, Sprite> sprites = new();
    TrailShape shape;
    Color color;
    RemoteAvatar remote;
    Vector3 previous;
    float carry;
    int side;
    GameObject root;

    public void Init(TrailShape kind, Color tint)
    {
        shape = kind;
        color = tint;
        remote = GetComponent<RemoteAvatar>();
        previous = transform.position;
        root = new GameObject("Particules de l'estela");
    }

    void OnEnable() => previous = transform.position;

    void OnDisable() => ResetTrail();

    void ResetTrail()
    {
        foreach (var particle in active) Recycle(particle);
        active.Clear();
        carry = 0f;
        side = 0;
    }

    void LateUpdate()
    {
        if (root == null) return;
        Vector3 position = transform.position;
        Vector3 delta = position - previous;
        previous = position;
        float distance = delta.magnitude;
        if (distance >= 3f || (remote != null && remote.Down))
        {
            ResetTrail();
            return;
        }

        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var particle = active[i];
            particle.age += dt;
            if (particle.age >= particle.life)
            {
                Recycle(particle);
                active.RemoveAt(i);
                continue;
            }
            particle.position += particle.velocity * dt;
            Draw(particle);
        }

        if (distance <= 0.0001f) return;
        float spacing = shape switch
        {
            TrailShape.Footsteps => .22f,
            TrailShape.Zigzag => .12f,
            TrailShape.Sparks => .16f,
            _ => .14f
        };
        Vector3 direction = delta / distance;
        float next = spacing - carry;
        for (int emitted = 0; next <= distance && emitted < 20; emitted++, next += spacing)
            Emit(position - direction * (distance - next), direction);
        carry = Mathf.Repeat(carry + distance, spacing);
    }

    void Recycle(Particle particle)
    {
        if (particle.renderer == null) return;
        particle.renderer.gameObject.SetActive(false);
        pool.Enqueue(particle);
    }

    void Emit(Vector3 position, Vector3 direction)
    {
        if (active.Count >= 64) return;
        Particle particle;
        if (pool.Count > 0) particle = pool.Dequeue();
        else
        {
            var go = new GameObject("Particula", typeof(SpriteRenderer));
            go.transform.SetParent(root.transform, false);
            particle = new Particle { renderer = go.GetComponent<SpriteRenderer>() };
        }

        side = 1 - side;
        float sign = side == 0 ? -1f : 1f;
        var cross = new Vector3(-direction.y, direction.x, 0f);
        particle.position = position;
        particle.velocity = Vector3.zero;
        particle.age = 0f;
        particle.life = shape == TrailShape.Footsteps ? .6f : .36f;
        particle.scale = shape switch
        {
            TrailShape.Dots => .24f,
            TrailShape.Sparks => .32f,
            TrailShape.Zigzag => .43f,
            _ => .38f
        };
        float heading = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        particle.angle = shape switch
        {
            TrailShape.Footsteps or TrailShape.Zigzag => heading - 90f,
            TrailShape.Sparks => heading - 45f + sign * 18f,
            TrailShape.Stars => sign * 15f,
            _ => 0f
        };

        if (shape == TrailShape.Footsteps) particle.position += cross * sign * .1f;
        if (shape == TrailShape.Sparks) particle.velocity = -direction * .55f + cross * sign * .22f;
        particle.renderer.sprite = SpriteFor(shape);
        particle.renderer.sortingLayerName = PlayerCosmetics.TrailSortingLayer;
        particle.renderer.sortingOrder = 1;
        Draw(particle);
        particle.renderer.gameObject.SetActive(true);
        active.Add(particle);
    }

    void Draw(Particle particle)
    {
        float progress = particle.age / particle.life;
        var visual = particle.renderer.transform;
        visual.position = particle.position;
        float growth = shape == TrailShape.Rings ? Mathf.Lerp(.75f, 1.6f, progress) : 1f - progress * .35f;
        visual.localScale = Vector3.one * (particle.scale * growth);
        visual.rotation = Quaternion.Euler(0f, 0f, particle.angle + (shape == TrailShape.Stars ? progress * 100f : 0f));
        Color tint = CosmeticsDatabase.IsRainbow(color)
            ? Color.HSVToRGB(Mathf.Repeat(Time.time * .5f + progress * .2f, 1f), .8f, 1f)
            : color;
        tint.a = (1f - progress) * Mathf.Min(.75f, color.a);
        particle.renderer.color = tint;
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
                case TrailShape.Zigzag:on=(y>=5&&x==4)||(y==4&&x>=2&&x<=4)||(y<=3&&x==2);break;
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
