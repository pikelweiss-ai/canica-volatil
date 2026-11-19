using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap liquidTilemap;
    public float normalSpeed = 5f;
    public float waterSpeed = 2f;

    private Vector2 dashDirection;
    private float dashLeft;
    readonly RaycastHit2D[] dashHits=new RaycastHit2D[16];
    public void Dash(Vector2 direction,float distance){dashDirection=direction.normalized;dashLeft=distance;}
    private float currentSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        foreach (Collider2D body in GetComponents<Collider2D>())
        {
            if (body.isTrigger) continue;
            body.sharedMaterial = SmoothContact;
            if (body is BoxCollider2D box && box.edgeRadius == 0f)
            {
                float radius = Mathf.Min(box.size.x, box.size.y) * 0.2f;
                box.size -= Vector2.one * (radius * 2f);
                box.edgeRadius = radius;
            }
        }
    }

    static PhysicsMaterial2D smoothContact;
    static PhysicsMaterial2D SmoothContact => smoothContact != null ? smoothContact :
        smoothContact = new PhysicsMaterial2D("Canica - moviment") { friction = 0f, bounciness = 0f };

    void OnDisable()
    {
        movement = Vector2.zero;
        dashLeft = 0f;
    }

    void Update()
    {
        bool onLiquid = liquidTilemap != null && liquidTilemap.HasTile(liquidTilemap.WorldToCell(transform.position));
        currentSpeed = onLiquid ? waterSpeed : normalSpeed;

        movement = GameControls.Move;
    }

    void FixedUpdate()
    {
        if(dashLeft>0f)
        {
            float step=Mathf.Min(dashLeft,12f*Time.fixedDeltaTime);
            var filter=new ContactFilter2D();filter.useTriggers=false;
            int n=rb.Cast(dashDirection,filter,dashHits,step+0.05f);
            for (int i = 0; i < n; i++)
                if (Vector2.Dot(dashDirection, dashHits[i].normal) < -0.01f)
                    step = Mathf.Min(step, Mathf.Max(0f, dashHits[i].distance - 0.05f));
            if (step < 0.001f)
            {
                dashLeft = 0f;
                rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
                return;
            }
            rb.MovePosition(rb.position + dashDirection * step);
            dashLeft -= step;
        }
        else rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
}
