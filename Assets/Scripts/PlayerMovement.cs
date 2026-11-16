using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap liquidTilemap;
    public float normalSpeed = 5f;
    public float waterSpeed = 2f;

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

        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
}