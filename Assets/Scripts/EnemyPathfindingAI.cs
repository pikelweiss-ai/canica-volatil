using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyPathfindingAI : MonoBehaviour
{
    public Transform player;
    public Tilemap referenceTilemap;
    public Tilemap[] obstacleTilemaps;

    public float detectionRange = 8f;
    public float moveSpeed = 2.5f;
    public float recalculateTime = 0.25f;

    private Rigidbody2D rb;
    private List<Vector3Int> path = new List<Vector3Int>();
    private int pathIndex = 0;
    private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null || referenceTilemap == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectionRange)
        {
            path.Clear();
            return;
        }

        timer -= Time.fixedDeltaTime;

        if (timer <= 0f)
        {
            CalculatePath();
            timer = recalculateTime;
        }

        MoveAlongPath();
    }

    void CalculatePath()
    {
        Vector3Int startCell = referenceTilemap.WorldToCell(transform.position);
        Vector3Int targetCell = referenceTilemap.WorldToCell(player.position);

        targetCell = FindNearestFreeCell(targetCell);

        path = FindPath(startCell, targetCell);
        pathIndex = 0;
    }

    void MoveAlongPath()
    {
        if (path == null || path.Count == 0)
        {
            MoveDirectlyToPlayer();
            return;
        }

        if (pathIndex >= path.Count)
        {
            MoveDirectlyToPlayer();
            return;
        }

        Vector3 targetWorld = referenceTilemap.GetCellCenterWorld(path[pathIndex]);

        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            targetWorld,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        if (Vector2.Distance(rb.position, targetWorld) < 0.08f)
        {
            pathIndex++;
        }
    }

    void MoveDirectlyToPlayer()
    {
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;

        Vector2 newPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;

        Vector3Int enemyNextCell = referenceTilemap.WorldToCell(newPosition);
        Vector3Int playerCell = referenceTilemap.WorldToCell(player.position);

        // Si la següent posició és obstacle, no es mou
        if (IsBlocked(enemyNextCell))
        {
            return;
        }

        // Si el jugador està damunt d'aigua/paret/arbre, no intentes entrar al seu tile
        if (IsBlocked(playerCell))
        {
            return;
        }

        rb.MovePosition(newPosition);
    }

    List<Vector3Int> FindPath(Vector3Int start, Vector3Int target)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector3Int[] directions =
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        int maxSearch = 300;
        int searched = 0;

        while (queue.Count > 0 && searched < maxSearch)
        {
            searched++;

            Vector3Int current = queue.Dequeue();

            if (current == target)
                break;

            foreach (Vector3Int dir in directions)
            {
                Vector3Int next = current + dir;

                if (cameFrom.ContainsKey(next))
                    continue;

                if (IsBlocked(next))
                    continue;

                queue.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        if (!cameFrom.ContainsKey(target))
            return new List<Vector3Int>();

        List<Vector3Int> newPath = new List<Vector3Int>();
        Vector3Int currentCell = target;

        while (currentCell != start)
        {
            newPath.Add(currentCell);
            currentCell = cameFrom[currentCell];
        }

        newPath.Reverse();
        return newPath;
    }

    Vector3Int FindNearestFreeCell(Vector3Int cell)
    {
        if (!IsBlocked(cell))
            return cell;

        for (int radius = 1; radius <= 15; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int testCell = cell + new Vector3Int(x, y, 0);

                    if (!IsBlocked(testCell))
                    {
                        return testCell;
                    }
                }
            }
        }

        return cell;
    }

    bool IsBlocked(Vector3Int cell)
    {
        foreach (Tilemap tilemap in obstacleTilemaps)
        {
            if (tilemap != null && tilemap.HasTile(cell))
                return true;
        }

        return false;
    }
}