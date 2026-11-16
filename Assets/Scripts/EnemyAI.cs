using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    public Transform player;

    public Tilemap liquidTilemap;
    public Tilemap wallTilemap;

    public float detectionRange = 8f;
    public float enemySpeed = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            return;
        }

        Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;

        Vector2 bestDirection = FindBestDirection(directionToPlayer);

        if (bestDirection != Vector2.zero)
        {
            Vector2 newPosition = rb.position + bestDirection * enemySpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    Vector2 FindBestDirection(Vector2 mainDirection)
    {
        Vector2[] possibleDirections =
        {
            mainDirection,
            new Vector2(mainDirection.x, 0).normalized,
            new Vector2(0, mainDirection.y).normalized,
            new Vector2(-mainDirection.y, mainDirection.x).normalized,
            new Vector2(mainDirection.y, -mainDirection.x).normalized
        };

        Vector2 bestDirection = Vector2.zero;
        float bestDistance = Mathf.Infinity;

        foreach (Vector2 dir in possibleDirections)
        {
            if (dir == Vector2.zero)
            {
                continue;
            }

            Vector2 testPosition = rb.position + dir * enemySpeed * Time.fixedDeltaTime;

            if (CanMoveTo(testPosition))
            {
                float distance = Vector2.Distance(testPosition, player.position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDirection = dir;
                }
            }
        }

        return bestDirection;
    }

    bool CanMoveTo(Vector2 position)
    {
        Vector3Int liquidCell = liquidTilemap.WorldToCell(position);
        Vector3Int wallCell = wallTilemap.WorldToCell(position);

        if (liquidTilemap.HasTile(liquidCell))
        {
            return false;
        }

        if (wallTilemap.HasTile(wallCell))
        {
            return false;
        }

        return true;
    }
}