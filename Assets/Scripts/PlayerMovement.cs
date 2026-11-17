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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector3Int groundPosition = groundTilemap.WorldToCell(transform.position);
        Vector3Int liquidPosition = liquidTilemap.WorldToCell(transform.position);

        bool onGround = groundTilemap.HasTile(groundPosition);
        bool onLiquid = liquidTilemap.HasTile(liquidPosition);

        if (onLiquid)
        {
            currentSpeed = waterSpeed;
        }
        else if (onGround)
        {
            currentSpeed = normalSpeed;
        }

        movement = GameControls.Move;
    }

    void FixedUpdate()
    {
        if(dashLeft>0f)
        {
            float step=Mathf.Min(dashLeft,12f*Time.fixedDeltaTime);
            var filter=new ContactFilter2D();filter.useTriggers=false;
            int n=rb.Cast(dashDirection,filter,dashHits,step+0.05f);
            for(int i=0;i<n;i++) step=Mathf.Min(step,Mathf.Max(0,dashHits[i].distance-0.05f));
            rb.MovePosition(rb.position+dashDirection*step);dashLeft-=step;
            if(step<0.001f)dashLeft=0;
        }
        else rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
}