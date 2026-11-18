using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Moviment i "cervell" bàsic de tots els enemics.
//
// Com pensa:
// 1) Tranquil: passeja a poc a poc per la zona.
// 2) Et VEU (estàs a 'detectionRange' i no hi ha cap paret pel mig) o li pegues -> s'enfada.
// 3) Enfadat: et persegueix amb A* (camí per caselles en 8 direccions, sense tallar cantons).
//    Si et veu en línia recta, va directe (moviment suau, sense zig-zag de casella en casella).
// 4) Si et perd de vista prou temps i estàs lluny, s'oblida de tu.
// Els enemics s'aparten entre ells perquè no es facen una pilota.
//
// Altres scripts (Tirador, Kamikaze, Boss...) canvien 'mode', 'preferredDistance' i 'speedMultiplier'.
public class EnemyPathfindingAI : MonoBehaviour
{
    public enum Mode { Chase, KeepDistance, Hold }

    [Header("Referències (si estan buides, es busquen soles)")]
    public Transform player;
    float nextRetarget;
    public Tilemap referenceTilemap;
    public Tilemap[] obstacleTilemaps;

    [Header("Moviment")]
    public float detectionRange = 8f;
    public float moveSpeed = 2.5f;
    public float recalculateTime = 0.25f;
    public float bodyRadius = 0.28f;          // Mida del cos (per a no ficar-se a les parets)

    [Header("Comportament")]
    public Mode mode = Mode.Chase;
    public float preferredDistance = 4.5f;    // Per a KeepDistance (tiradors)
    public float forgetTime = 3f;             // Segons sense veure't abans d'oblidar-se'n
    public bool wanderWhenIdle = true;
    [HideInInspector] public float speedMultiplier = 1f;

    public bool HasAggro { get; private set; }
    public Transform Player => player;
    public bool SeesPlayer { get; private set; }
    public Vector2 Velocity { get; private set; }

    static readonly List<EnemyPathfindingAI> All = new List<EnemyPathfindingAI>();

    private Rigidbody2D rb;
    private List<Vector2> path = new List<Vector2>();
    private int pathIndex;
    private float repathTimer;
    private float lastSeenTime = -999f;
    private Vector2 knockbackVelocity;
    private float knockbackTimer;
    private Vector2 wanderTarget;
    private float wanderTimer;
    private float strafeSign = 1f;
    private float strafeTimer;
    private Vector2 home;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if(rb!=null) rb.interpolation=RigidbodyInterpolation2D.Interpolate;
        home = transform.position;
        wanderTarget = home;

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Si l'enemic ve configurat a mà (escena vella), registrem els seus tilemaps
        if (referenceTilemap != null && TileNav.Ground == null)
        {
            TileNav.SetGround(referenceTilemap);
            if (obstacleTilemaps != null)
                foreach (Tilemap t in obstacleTilemaps) TileNav.Register(t);
        }
        TileNav.Ready();
    }

    // Li han pegat o la sala s'ha activat: s'enfada encara que no et veja
    public void Alert()
    {
        HasAggro = true;
        lastSeenTime = Time.time;
    }

    public void ApplyKnockback(Vector2 velocity, float duration)
    {
        knockbackVelocity = velocity;
        knockbackTimer = duration;
        Alert();
    }

    void FixedUpdate()
    {
        if (rb == null || !TileNav.Ready()) return;
        float dt = Time.fixedDeltaTime;

        // Retrocés: l'empenta mana (sense ficar-lo dins de parets)
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= dt;
            TryMove(rb.position + knockbackVelocity * dt);
            repathTimer = 0f;
            return;
        }

        // Online (amfitrio): persegueix el jugador mes proxim, siga qui siga
        if (NetCoop.IsHost && Time.time >= nextRetarget) { nextRetarget = Time.time + 0.4f; var t = NetCoop.NearestTarget(rb.position); if (t != null) player = t; }

        if (player == null) { Wander(dt); return; }

        Vector2 pos = rb.position;
        Vector2 target = player.position;
        float dist = Vector2.Distance(pos, target);

        SeesPlayer = dist <= detectionRange * 1.75f && TileNav.LineOfSight(pos, target);
        if (SeesPlayer && (HasAggro || dist <= detectionRange))
        {
            HasAggro = true;
            lastSeenTime = Time.time;
        }
        if (HasAggro && Time.time - lastSeenTime > forgetTime && dist > detectionRange)
            HasAggro = false;

        if (!HasAggro) { Wander(dt); return; }

        Vector2 dir = Vector2.zero;
        switch (mode)
        {
            case Mode.Hold:
                break;

            case Mode.Chase:
                dir = DirectionTo(pos, target);
                break;

            case Mode.KeepDistance:
                if (!SeesPlayer || dist > preferredDistance + 1f)
                    dir = DirectionTo(pos, target);                         // S'acosta
                else if (dist < preferredDistance - 1f)
                    dir = (pos - target).normalized;                         // Recula
                else
                {
                    // A bona distància: es mou de costat (més difícil d'encertar)
                    strafeTimer -= dt;
                    if (strafeTimer <= 0f) { strafeSign = Random.value < 0.5f ? -1f : 1f; strafeTimer = Random.Range(0.8f, 1.8f); }
                    Vector2 toP = (target - pos).normalized;
                    dir = new Vector2(-toP.y, toP.x) * strafeSign * 0.6f;
                }
                break;
        }

        dir += Separation(pos);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector2 step = dir * moveSpeed * speedMultiplier * dt;
        Velocity = step / dt;
        if (step.sqrMagnitude > 0f)
        {
            // Si no pot anar en diagonal contra una paret, llisca per l'eix que puga
            if (!TryMove(pos + step) && !TryMove(pos + new Vector2(step.x, 0f)))
                TryMove(pos + new Vector2(0f, step.y));
        }
    }

    // Cap on caminar per a arribar a 'target': directe si hi ha via lliure, si no, pel camí A*
    Vector2 DirectionTo(Vector2 pos, Vector2 target)
    {
        if (TileNav.LineOfSight(pos, target, bodyRadius))
        {
            path.Clear();
            return (target - pos).normalized;
        }

        repathTimer -= Time.fixedDeltaTime;
        if (repathTimer <= 0f || path.Count == 0)
        {
            path = TileNav.FindPath(pos, target);
            pathIndex = 0;
            repathTimer = recalculateTime;
        }
        if (path.Count == 0) return Vector2.zero;

        // Allisa el camí: salta punts si ja veu el següent
        while (pathIndex + 1 < path.Count && TileNav.LineOfSight(pos, path[pathIndex + 1], bodyRadius))
            pathIndex++;

        if (Vector2.Distance(pos, path[pathIndex]) < 0.1f && pathIndex + 1 < path.Count)
            pathIndex++;

        return (path[pathIndex] - pos).normalized;
    }

    // Empenta suau per a separar-se dels enemics que té enganxats
    Vector2 Separation(Vector2 pos)
    {
        Vector2 push = Vector2.zero;
        for (int i = 0; i < All.Count; i++)
        {
            EnemyPathfindingAI o = All[i];
            if (o == this || o == null) continue;
            Vector2 d = pos - (Vector2)o.transform.position;
            float min = bodyRadius + o.bodyRadius + 0.15f;
            float m = d.magnitude;
            if (m < min && m > 0.001f) push += d / m * (1f - m / min) * 1.5f;
        }
        return push;
    }

    void Wander(float dt)
    {
        Velocity = Vector2.zero;
        if (!wanderWhenIdle) return;

        wanderTimer -= dt;
        Vector2 pos = rb.position;
        if (wanderTimer <= 0f)
        {
            wanderTimer = Random.Range(1.5f, 3.5f);
            Vector2 tryTarget = home + Random.insideUnitCircle * 3f;
            wanderTarget = TileNav.LineOfSight(pos, tryTarget, bodyRadius) ? tryTarget : pos;
        }

        Vector2 to = wanderTarget - pos;
        if (to.magnitude < 0.1f) return;
        Vector2 step = to.normalized * moveSpeed * 0.4f * dt;
        if (!TryMove(pos + step)) wanderTimer = 0f;
        Velocity = step / dt;
    }

    bool TryMove(Vector2 newPos)
    {
        if (!TileNav.CircleFree(newPos, bodyRadius)) return false;
        rb.MovePosition(newPos);
        return true;
    }
}
