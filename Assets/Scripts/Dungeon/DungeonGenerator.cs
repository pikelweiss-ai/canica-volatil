using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Genera un pis de masmorra cada vegada que es carrega l'escena.
//
// 1) DISTRIBUCIÓ: sales en una graella (com un mapa de quadrets). Es comença a la sala inicial
//    i es van afegint sales veïnes a l'atzar, en forma d'arbre (sense cercles).
// 2) TIPUS: la sala més llunyana (i sense eixida) és la del BOSS; una altra sala sense eixida és
//    la del TRESOR (cofre bo); la resta són sales de COMBAT.
// 3) PINTAT: terra a les sales i als passadissos, i parets al voltant de tot el que és terra.
//    Dins de les sales de combat: columnes, basses (aigua/lava), caixes, barrils i punxes.
// 4) EN JUGAR: quan entres a una sala de combat, les portes es tanquen i ixen enemics per onades.
//    Quan els mates tots, les portes s'obrin (i a voltes apareix un cofre).
//    Quan mates el boss, apareix un portal al pis següent (un altre tema i més difícil).
public partial class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance { get; private set; }

    public enum RoomType { Start, Combat, Treasure, Boss, Emboscada, Arena, Secreta }
    public enum RoomShape { Rectangle, L, T, Cross, Circle, Octagon, Pillars }
    public enum RoomModifier { None, Darkness, Ice, Flames }

    public class Room
    {
        public Vector2Int slot;
        public RoomType type;
        public RoomShape shape;
        public RoomModifier modifier;
        public bool ambushTriggered;
        public ChestRarity ambushRarity;
        public float combatStarted;
        public RectInt rect;                          // Cel·les de terra de dins de la sala
        public readonly List<Room> links = new List<Room>();
        public readonly List<Vector3Int> doorCells = new List<Vector3Int>();
        public readonly List<EnemyHealth> alive = new List<EnemyHealth>();
        public bool visited, active, cleared;
        public int waitingEntrance = -1; // costat obert mentre s'esperen els companys
        public Vector2Int Center => new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);
    }

    [Header("Tilemaps de l'escena")]
    public Tilemap ground;
    public Tilemap walls;
    public Tilemap liquid;
    public Tilemap doors;                 // Si està buit, es crea sol (amb col·lisió)

    [Header("Temes (un per pis, en ordre; després torna a començar)")]
    public DungeonTheme[] themes;
    public TileBase doorTile;

    [Header("Jugador")]
    public Transform player;
    public Transform spawnPoint;

    [Header("Enemics (0 cos a cos, 1 llim, 2 tirador, 3 kamikaze)")]
    public GameObject[] enemyPrefabs;
    public GameObject bossPrefab;
    public Sprite enemyBulletSprite;

    [Header("Objectes")]
    public GameObject[] chestPrefabs;     // Per raresa: Comú, Rar, Èpic, Llegendari
    public GameObject cratePrefab;
    public GameObject barrelPrefab;
    public GameObject spikePrefab;
    public GameObject portalPrefab;       // Opcional: si està buit, es fa per codi

    [Header("Mida")]
    public int minRooms = 7;
    public int maxRooms = 10;
    public int slotSize = 24;
    public Vector2Int roomSizeMin = new Vector2Int(11, 9);
    public Vector2Int roomSizeMax = new Vector2Int(17, 13);
    public int seed = 0;                  // 0 = a l'atzar cada vegada

    public List<Room> Rooms { get; } = new List<Room>();
    public Room CurrentRoom { get; private set; }
    public Room ProgressionRoom { get; private set; }
    public DungeonTheme Theme { get; private set; }

    static readonly Vector2Int[] Dirs = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    private System.Random rng;
    private readonly HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> solid = new HashSet<Vector2Int>();   // Columnes i parets de dins
    private readonly HashSet<Vector2Int> reserved = new HashSet<Vector2Int>(); // Ocupades per objectes
    private DungeonUI ui;
    private bool gameOver;

    // ------------------------------------------------------------------ Generació

    void Awake()
    {
        Instance = this;

        if (!RunState.Active)
        {
            RunState.Reset();
            RunState.Active = true;
        }

        // En una sala online tots generen la mateixa masmorra: llavor de la sala + pis
        int netSeed = NetSession.Active && NetSession.Instance.Seed != 0 ? NetSession.Instance.Seed + RunState.Floor * 7919 : 0;
        if (RunState.Seed == 0) RunState.Seed = seed != 0 ? seed : System.Environment.TickCount | 1;
        rng = new System.Random(netSeed != 0 ? netSeed : unchecked(RunState.Seed + (RunState.Floor - 1) * 7919));
        Theme = themes != null && themes.Length > 0 ? themes[(RunState.Floor - 1) % themes.Length] : null;
        if (enemyBulletSprite != null) EnemyBullet.sprite = enemyBulletSprite;

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        PrepareTilemaps();
        BuildLayout();
        Carve();
        Paint();
        Populate();
        ApplyLook();
        DungeonBackdrop.Attach(this);
        DungeonDoors.Attach(this);

        // El jugador comença al mig de la sala inicial
        Room start = Rooms[0];
        Vector3 startPos = ground.GetCellCenterWorld(ToCell(start.Center));
        if (RunState.Floor > 1) DungeonPortal.Create(startPos, returning: true);
        if (RunState.ArrivalDirection < 0) startPos = PortalPosition(1);
        if (player != null) player.position = new Vector3(startPos.x, startPos.y, player.position.z);
        if (spawnPoint != null) spawnPoint.position = startPos;
        Camera cam = Camera.main;
        if (cam != null) cam.transform.position = new Vector3(startPos.x, startPos.y, cam.transform.position.z);
        start.visited = true;
        CurrentRoom = RunState.ArrivalDirection < 0 ? Rooms.Find(r => r.type == RoomType.Boss) : start;

        ui = DungeonUI.Create(this);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void PrepareTilemaps()
    {
        if (doors == null)
        {
            GameObject go = new GameObject("Tilemap Portes", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
            go.transform.SetParent(ground.transform.parent, false);
            doors = go.GetComponent<Tilemap>();
            TilemapRenderer wr = walls.GetComponent<TilemapRenderer>();
            TilemapRenderer dr = go.GetComponent<TilemapRenderer>();
            if (wr != null) { dr.sortingLayerID = wr.sortingLayerID; dr.sortingOrder = wr.sortingOrder + 1; }
        }

        ground.ClearAllTiles();
        walls.ClearAllTiles();
        if (liquid != null) liquid.ClearAllTiles();
        doors.ClearAllTiles();

        TileNav.SetGround(ground);
        TileNav.Register(walls);
        TileNav.Register(liquid);
        TileNav.Register(doors);
    }

    void BuildLayout()
    {
        var slots = new Dictionary<Vector2Int, Room>();
        Room start = new Room { slot = Vector2Int.zero, type = RoomType.Start };
        slots[start.slot] = start;
        Rooms.Add(start);

        int target = rng.Next(minRooms, maxRooms + 1) + Mathf.Min(RunState.Floor - 1, 3);
        int guard = 0;
        while (Rooms.Count < target && guard++ < 2000)
        {
            Room from = Rooms[rng.Next(Rooms.Count)];
            if (from.type == RoomType.Start && from.links.Count >= 2) continue; // Que la inicial no siga un nus

            Vector2Int s = from.slot + Dirs[rng.Next(4)];
            if (slots.ContainsKey(s)) continue;

            int neighbours = 0;
            foreach (Vector2Int d in Dirs) if (slots.ContainsKey(s + d)) neighbours++;
            if (neighbours > 2 || (neighbours == 2 && (RunState.Floor < 3 || rng.NextDouble() >= .25))) continue;

            Room r = new Room { slot = s, type = RoomType.Combat };
            foreach (Vector2Int d in Dirs)
                if (slots.TryGetValue(s + d, out Room neighbour))
                { r.links.Add(neighbour); neighbour.links.Add(r); }
            slots[s] = r;
            Rooms.Add(r);
        }

        // Distàncies (en sales) des de l'inici
        var dist = new Dictionary<Room, int> { [start] = 0 };
        var queue = new Queue<Room>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Room c = queue.Dequeue();
            foreach (Room n in c.links)
                if (!dist.ContainsKey(n)) { dist[n] = dist[c] + 1; queue.Enqueue(n); }
        }

        // Boss: la sala sense eixida més llunyana
        Room boss = null;
        foreach (Room r in Rooms)
        {
            if (r == start) continue;
            if (boss == null || dist[r] > dist[boss])
                boss = r;
        }
        if (boss != null) boss.type = RoomType.Boss;

        // Tresor: una altra sala sense eixida (si n'hi ha)
        var leaves = Rooms.FindAll(r => r.type == RoomType.Combat && r.links.Count == 1);
        if (leaves.Count > 0) leaves[rng.Next(leaves.Count)].type = RoomType.Treasure;

        if (RunState.Floor >= 3)
        {
            foreach (Room r in Rooms)
                if (r.type == RoomType.Combat && (ProgressionRoom == null || dist[r] > dist[ProgressionRoom]))
                    ProgressionRoom = r;
        }

        var special = Rooms.FindAll(r => r.type == RoomType.Combat && r != ProgressionRoom);
        if (special.Count > 0) { var r = special[rng.Next(special.Count)]; r.type = RoomType.Emboscada; special.Remove(r); }
        if (special.Count > 0) { var r = special[rng.Next(special.Count)]; r.type = RoomType.Arena; special.Remove(r); }
        var secret = special.FindAll(r => r.links.Count == 1);
        if (secret.Count > 0) secret[rng.Next(secret.Count)].type = RoomType.Secreta;

        // Mida de cada sala (senars, perquè tinguen un centre exacte)
        foreach (Room r in Rooms)
        {
            Vector2Int size;
            switch (r.type)
            {
                case RoomType.Start: size = new Vector2Int(11, 11); break;
                case RoomType.Treasure: size = new Vector2Int(9, 9); break;
                case RoomType.Boss: size = new Vector2Int(19, 15); break;
                case RoomType.Emboscada: size = new Vector2Int(23, 7); break;
                case RoomType.Arena: size = new Vector2Int(21, 17); break;
                default:
                    size = new Vector2Int(Odd(rng.Next(roomSizeMin.x, roomSizeMax.x + 1)), Odd(rng.Next(roomSizeMin.y, roomSizeMax.y + 1)));
                    break;
            }
            Vector2Int c = r.slot * slotSize;
            r.rect = new RectInt(c.x - size.x / 2, c.y - size.y / 2, size.x, size.y);
            if (r.type == RoomType.Combat || r.type == RoomType.Arena || r.type == RoomType.Emboscada)
            {
                r.shape = (RoomShape)rng.Next(7);
                if (r != ProgressionRoom) r.modifier = (RoomModifier)rng.Next(4);
            }
        }
    }

    static int Odd(int n) => n % 2 == 0 ? n + 1 : n;

    void Carve()
    {
        foreach (Room r in Rooms)
            foreach (Vector2Int p in r.rect.allPositionsWithin) if (InsideShape(r, p)) floor.Add(p);

        // Passadissos de 3 d'ample entre sales veïnes
        var done = new HashSet<(Room, Room)>();
        foreach (Room a in Rooms)
            foreach (Room b in a.links)
            {
                if (done.Contains((b, a))) continue;
                done.Add((a, b));

                Vector2Int d = b.slot - a.slot;
                Room first = (d.x > 0 || d.y > 0) ? a : b;   // La de l'esquerra / de baix
                Room second = first == a ? b : a;
                Vector2Int c = first.slot * slotSize;

                if (d.y == 0)
                {
                    int x0 = first.rect.xMax, x1 = second.rect.xMin - 1;
                    for (int x = x0; x <= x1; x++)
                        for (int y = c.y - 1; y <= c.y + 1; y++) floor.Add(new Vector2Int(x, y));
                    for (int y = c.y - 1; y <= c.y + 1; y++)
                    {
                        first.doorCells.Add(new Vector3Int(x0, y, 0));
                        second.doorCells.Add(new Vector3Int(x1, y, 0));
                    }
                }
                else
                {
                    int y0 = first.rect.yMax, y1 = second.rect.yMin - 1;
                    for (int y = y0; y <= y1; y++)
                        for (int x = c.x - 1; x <= c.x + 1; x++) floor.Add(new Vector2Int(x, y));
                    for (int x = c.x - 1; x <= c.x + 1; x++)
                    {
                        first.doorCells.Add(new Vector3Int(x, y0, 0));
                        second.doorCells.Add(new Vector3Int(x, y1, 0));
                    }
                }
            }

        // Decoració de dins de les sales de combat i del boss
        foreach (Room r in Rooms)
        {
            if (r.type == RoomType.Combat) DecorateCombat(r);
            else if (r.type == RoomType.Boss) DecorateBoss(r);
        }
    }

    // Una cel·la de dins de la sala on es pot posar alguna cosa sense tancar camins:
    // ni a la vora (deixem un anell lliure) ni a la creu central (camí de porta a porta)
    bool FreeForDecor(Room r, Vector2Int p, int margin = 2)
    {
        if (!InsideShape(r, p) || !floor.Contains(p)) return false;
        Vector2Int c = r.Center;
        if (p.x < r.rect.xMin + margin || p.x >= r.rect.xMax - margin) return false;
        if (p.y < r.rect.yMin + margin || p.y >= r.rect.yMax - margin) return false;
        if (Mathf.Abs(p.x - c.x) <= 1 || Mathf.Abs(p.y - c.y) <= 1) return false;
        return !solid.Contains(p) && !reserved.Contains(p);
    }

    void DecorateCombat(Room r)
    {
        Vector2Int c = r.Center;
        int qx = r.rect.width / 4, qy = r.rect.height / 4;

        if (r == ProgressionRoom)
        {
            // Columnes als quadrants i corredors oberts en creu.
            foreach (int x in new[] { -1, 1 })
                foreach (int y in new[] { -1, 1 })
                {
                    Vector2Int pillar = c + new Vector2Int(x * qx, y * qy);
                    if (FreeForDecor(r, pillar)) solid.Add(pillar);
                }
            return;
        }

        switch (rng.Next(7))
        {
            case 4: // Anell de columnes al voltant del centre
                for (int i = 0; i < 8; i++)
                {
                    float a = i * Mathf.PI / 4f;
                    Vector2Int p = c + new Vector2Int(Mathf.RoundToInt(Mathf.Cos(a) * qx * 1.3f), Mathf.RoundToInt(Mathf.Sin(a) * qy * 1.3f));
                    if (FreeForDecor(r, p)) solid.Add(p);
                }
                break;

            case 5: // Franges de líquid a dalt i a baix (passadís pel mig)
                if (Theme != null && Theme.liquidTile != null)
                    for (int x = r.rect.xMin + 2; x < r.rect.xMax - 2; x++)
                        foreach (int y in new[] { r.rect.yMin + 2, r.rect.yMax - 3 })
                        {
                            Vector2Int p = new Vector2Int(x, y);
                            if (FreeForDecor(r, p)) { reserved.Add(p); poolCells.Add(p); }
                        }
                break;

            case 6: // Parets en L als racons (cobertura)
                foreach (Vector2Int s in new[] { new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) })
                {
                    Vector2Int o = c + new Vector2Int(s.x * qx, s.y * qy);
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2Int h = o + new Vector2Int(k * s.x, 0), v = o + new Vector2Int(0, k * s.y);
                        if (FreeForDecor(r, h)) solid.Add(h);
                        if (FreeForDecor(r, v)) solid.Add(v);
                    }
                }
                break;

            case 0: // Quatre columnes de 2x2
                foreach (Vector2Int s in new[] { new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) })
                {
                    Vector2Int o = c + new Vector2Int(s.x * qx, s.y * qy);
                    for (int x = 0; x < 2; x++)
                        for (int y = 0; y < 2; y++)
                        {
                            Vector2Int p = o + new Vector2Int(x * s.x, y * s.y);
                            if (FreeForDecor(r, p)) solid.Add(p);
                        }
                }
                break;

            case 1: // Basses als quatre racons (aigua o lava, segons el tema)
                if (Theme != null && Theme.liquidTile != null && rng.NextDouble() < Theme.liquidChance + 0.4)
                    foreach (Vector2Int s in new[] { new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) })
                    {
                        Vector2Int o = c + new Vector2Int(s.x * qx, s.y * qy);
                        for (int x = -1; x <= 0; x++)
                            for (int y = -1; y <= 0; y++)
                            {
                                Vector2Int p = o + new Vector2Int(x, y);
                                if (FreeForDecor(r, p)) { reserved.Add(p); poolCells.Add(p); }
                            }
                    }
                break;

            case 2: // Columnes soltes
                for (int i = 0; i < 6; i++)
                {
                    Vector2Int p = RandomCell(r);
                    if (FreeForDecor(r, p) && FreeForDecor(r, p + Vector2Int.right)) solid.Add(p);
                }
                break;

            default: // Sala oberta
                break;
        }

        // Objectes: caixes, barrils i punxes
        PlaceProps(r, cratePrefab, rng.Next(2, 5), cratesToSpawn);
        PlaceProps(r, barrelPrefab, rng.Next(0, 3), barrelsToSpawn);
        if (rng.NextDouble() < 0.5) PlaceProps(r, spikePrefab, rng.Next(2, 6), spikesToSpawn);
    }

    void DecorateBoss(Room r)
    {
        // Quatre columnes lluny del centre per a poder amagar-se de les bales
        Vector2Int c = r.Center;
        foreach (Vector2Int s in new[] { new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(1, 1) })
        {
            Vector2Int o = c + new Vector2Int(s.x * (r.rect.width / 3), s.y * (r.rect.height / 3));
            solid.Add(o);
            solid.Add(o + new Vector2Int(s.x, 0));
        }
    }

    readonly List<Vector2Int> poolCells = new List<Vector2Int>();
    readonly List<Vector2Int> cratesToSpawn = new List<Vector2Int>();
    readonly List<Vector2Int> barrelsToSpawn = new List<Vector2Int>();
    readonly List<Vector2Int> spikesToSpawn = new List<Vector2Int>();

    void PlaceProps(Room r, GameObject prefab, int count, List<Vector2Int> list)
    {
        if (prefab == null) return;
        for (int i = 0, tries = 0; i < count && tries < 40; tries++)
        {
            Vector2Int p = RandomCell(r);
            if (!FreeForDecor(r, p, 1)) continue;
            reserved.Add(p);
            list.Add(p);
            i++;
        }
    }

    Vector2Int RandomCell(Room r)
    {
        for (int i = 0; i < 80; i++)
        {
            var p = new Vector2Int(rng.Next(r.rect.xMin, r.rect.xMax), rng.Next(r.rect.yMin, r.rect.yMax));
            if (InsideShape(r, p)) return p;
        }
        return r.Center;
    }

    void Paint()
    {
        TileBase[] floors = Theme != null ? Theme.floorTiles : null;
        foreach (Vector2Int p in floor)
        {
            TileBase t = floors != null && floors.Length > 0
                ? (floors.Length == 1 || rng.NextDouble() < 0.75 ? floors[0] : floors[rng.Next(1, floors.Length)])
                : null;
            ground.SetTile(ToCell(p), t);
        }
        if(Theme != null && Theme.refugeFloor != null)
            foreach(Room room in Rooms)
                if(room.type == RoomType.Start || room.type == RoomType.Treasure)
                    foreach(Vector2Int p in room.rect.allPositionsWithin)
                        if(floor.Contains(p)) ground.SetTile(ToCell(p),Theme.refugeFloor);

        foreach (Room room in Rooms)
            if (room.modifier == RoomModifier.Ice)
                foreach (Vector2Int p in room.rect.allPositionsWithin)
                    if (floor.Contains(p))
                    { ground.SetTileFlags(ToCell(p), TileFlags.None); ground.SetColor(ToCell(p), new Color(.65f, .85f, 1f)); }

        // Parets: tot el que toca terra (també en diagonal) i no és terra
        TileBase wall = Theme != null ? Theme.wallTile : null;
        foreach (Vector2Int p in floor)
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int n = p + new Vector2Int(x, y);
                    if (!floor.Contains(n)) walls.SetTile(ToCell(n), wall);
                }

        foreach (Vector2Int p in solid) walls.SetTile(ToCell(p), wall);

        if (liquid != null && Theme != null && Theme.liquidTile != null)
            foreach (Vector2Int p in poolCells) liquid.SetTile(ToCell(p), Theme.liquidTile);
    }

    void Populate()
    {
        if(Theme!=null && Theme.decorPrefabs!=null && Theme.decorPrefabs.Length>0)
            foreach(Room room in Rooms)
                if(room.type==RoomType.Combat)
                    for(int tries=0;tries<30;tries++)
                    {
                        var cell=RandomCell(room);if(!FreeForDecor(room,cell,2))continue;
                        reserved.Add(cell);Spawn(Theme.decorPrefabs[rng.Next(Theme.decorPrefabs.Length)],cell);break;
                    }
        // v1.2.1 (BUG-026): mai dos objectes a la mateixa casella
        var occupied = new HashSet<Vector2Int>();
        foreach (Vector2Int p in cratesToSpawn) if (occupied.Add(p)) Spawn(cratePrefab, p);
        foreach (Vector2Int p in barrelsToSpawn) if (occupied.Add(p)) Spawn(barrelPrefab, p);
        foreach (Vector2Int p in spikesToSpawn) if (occupied.Add(p)) Spawn(spikePrefab, p);

        foreach (Room r in Rooms)
        {
            if (r.type == RoomType.Treasure || r.type == RoomType.Secreta || r.type == RoomType.Emboscada)
            {
                ChestRarity rarity = TreasureRarity();
                r.ambushRarity = rarity;
                SpawnChest(rarity, r.Center);
                // v1.4.0: a vegades, un cofre fals (Mimic) a la sala de tresor, des del pis 2
                if (r.type == RoomType.Treasure && Theme != null && Theme.mimic != null && RunState.Floor >= 2 && rng.NextDouble() < 0.3)
                    for (int tries = 0; tries < 30; tries++)
                    {
                        var mimicCell = RandomCell(r); if (!FreeForDecor(r, mimicCell, 2)) continue;
                        reserved.Add(mimicCell); Spawn(Theme.mimic, mimicCell); break;
                    }
            }
            else if (r.type == RoomType.Start)
            {
                // Cofre a la sala inicial de cada pis (al pis 1 és la primera arma)
                SpawnChest(RunState.Floor >= 3 && rng.NextDouble() < 0.4 ? ChestRarity.Rar : ChestRarity.Comu, r.Center + StartChestOffsets[0]);
                for (int extra = 1; extra < StartChestCount(); extra++) SpawnChest(ChestRarity.Comu, r.Center + StartChestOffsets[extra]); // un cofre comú per jugador
            }
            else if (r.type == RoomType.Combat && r != ProgressionRoom && rng.NextDouble() < 0.45)
            {
                // Cofres escampats per les sales de combat
                for (int tries = 0; tries < 30; tries++)
                {
                    Vector2Int p = RandomCell(r);
                    if (!FreeForDecor(r, p, 2)) continue;
                    reserved.Add(p);
                    SpawnChest(RandomRarity(), p);
                    break;
                }
            }
        }
        CreateSecretWalls();
    }

    // v1.3.1: els cofres milloren amb els pisos. Al pis 1 no hi ha èpics ni llegendaris.
    // Pesos (comú, rar, èpic, llegendari) per pis; del pis 6 en endavant es queda al darrer.
    static readonly int[][] ChestWeights =
    {
        new[] { 70, 30, 0, 0 },    // pis 1
        new[] { 55, 35, 10, 0 },   // pis 2
        new[] { 45, 35, 17, 3 },   // pis 3
        new[] { 35, 35, 23, 7 },   // pis 4
        new[] { 28, 32, 28, 12 },  // pis 5
        new[] { 20, 30, 32, 18 },  // pis 6+
    };

    ChestRarity RandomRarity()
    {
        int[] w = ChestWeights[Mathf.Clamp(RunState.Floor, 1, ChestWeights.Length) - 1];
        int total = w[0] + w[1] + w[2] + w[3];
        int roll = rng.Next(0, total);
        for (int i = 0; i < 4; i++) { if (roll < w[i]) return (ChestRarity)i; roll -= w[i]; }
        return ChestRarity.Comu;
    }

    // Sala del tresor: sempre per damunt d'un cofre normal, però tampoc és llegendari al pis 1-2.
    ChestRarity TreasureRarity()
    {
        double x = rng.NextDouble();
        switch (RunState.Floor)
        {
            case 1: return ChestRarity.Rar;
            case 2: return x < 0.7 ? ChestRarity.Rar : ChestRarity.Epic;
            case 3: return x < 0.3 ? ChestRarity.Rar : x < 0.85 ? ChestRarity.Epic : ChestRarity.Llegendari;
            default: return x < 0.15 ? ChestRarity.Rar : x < 0.7 ? ChestRarity.Epic : ChestRarity.Llegendari;
        }
    }

    // Recompensa del boss: Rar al pis 1, Èpic al 2 i Llegendari a partir del 3.
    ChestRarity BossRarity() => RunState.Floor <= 1 ? ChestRarity.Rar : RunState.Floor == 2 ? ChestRarity.Epic : ChestRarity.Llegendari;

    // Online: un cofre comú per jugador a la sala inicial (cada cofre només l'obri una persona)
    static readonly Vector2Int[] StartChestOffsets = { new Vector2Int(3, 3), new Vector2Int(-3, 3), new Vector2Int(3, -3), new Vector2Int(-3, -3) };
    int StartChestCount()
    {
        var s = NetSession.Instance;
        return NetCoop.Online && s != null ? Mathf.Clamp(s.Players.Count, 1, StartChestOffsets.Length) : 1;
    }

    GameObject Spawn(GameObject prefab, Vector2Int cell)
    {
        if (prefab == null) return null;
        return Instantiate(prefab, ground.GetCellCenterWorld(ToCell(cell)), Quaternion.identity);
    }

    void SpawnChest(ChestRarity rarity, Vector2Int cell)
    {
        if (chestPrefabs == null || chestPrefabs.Length <= (int)rarity) return;
        if (floorChests.TryGetValue(cell, out var existing) && existing != null) return;
        var spawned = Spawn(chestPrefabs[(int)rarity], cell);
        if (spawned != null && spawned.TryGetComponent(out Chest floorChest)) floorChests[cell] = floorChest;
        if (spawned == null || ProgressionRoom == null || RunState.Floor < 4 ||
            cell != ProgressionRoom.Center + new Vector2Int(2, 0)) return;

        var chest = spawned.GetComponent<Chest>();
        var weapon = NetCoop.Weapon(RunState.Floor >= 5 ? "BallestaCristall" : "ProjectorPolsos");
        if (chest == null || weapon == null) return;

        if (RunState.Floor == 4 || RunState.Floor == 5)
        {
            chest.lootTable = new[] { weapon };
            return;
        }

        var options = new List<WeaponData>(chest.lootTable ?? System.Array.Empty<WeaponData>());
        if (!options.Contains(weapon)) options.Add(weapon);
        chest.lootTable = options.ToArray();
    }

    void ApplyLook()
    {
        // El fons de núvols del mapa de proves no pinta res ací: fora de les sales, tot fosc
        GameObject sky = GameObject.Find("Background");
        if (sky != null) sky.SetActive(false);

        if (Theme == null) return;
        foreach (Light2D l in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
            if (l.lightType == Light2D.LightType.Global)
            {
                l.color = Theme.lightColor;
                l.intensity = Theme.lightIntensity;
            }
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Theme.backgroundColor;
        }
    }

    static Vector3Int ToCell(Vector2Int p) => new Vector3Int(p.x, p.y, 0);

    // ------------------------------------------------------------------ En jugar

    void Update()
    {
        // Online (amfitrio): els companys tambe obrin sales, encara que l'amfitrio haja caigut
        if (!gameOver && NetCoop.IsHost) TryStartRoomTogether();
        if (!gameOver) RunStats.Tick(Time.deltaTime);
        UpdateRoomEffects();

        if (gameOver || player == null || !player.gameObject.activeInHierarchy) return;

        Vector3Int pc = ground.WorldToCell(player.position);
        Room r = RoomAt(pc, 0);
        if (r != null && r != CurrentRoom)
        {
            CurrentRoom = r;
            if (!r.visited) { r.visited = true; ui.RefreshMap(); }
            ui.RefreshMap();
        }

        // Entres de deveres (una casella endins): comença el combat
        // Online: nomes l'amfitrio comença les sales (tambe quan hi entra un company); els clients ho reben
        if (!NetCoop.Online) TryStartRoom(pc);
        else WaitingHint(pc);

        if (r != null && (r.type == RoomType.Start || r.type == RoomType.Treasure)) r.cleared = true;
    }

    Room waitingRoom;
    readonly Dictionary<int, Vector3Int> previousPlayerCells = new Dictionary<int, Vector3Int>();

    // Online: nomes l'entrada queda oberta fins que hi son tots els jugadors vius.
    void TryStartRoomTogether()
    {
        var cells = new List<Vector3Int>();
        var ids = new List<int>();
        var session = NetSession.Instance;
        if (player != null && player.gameObject.activeInHierarchy) ids.Add(session.LocalId);
        if (player != null && player.gameObject.activeInHierarchy) cells.Add(ground.WorldToCell(player.position));
        foreach (var p in session.Players.Values)
            if (p.id != session.LocalId && !NetCoop.IsDown(p.id) && p.scene == NetSession.SceneToken() && Time.realtimeSinceStartupAsDouble - p.lastPos < 2)
            { ids.Add(p.id); cells.Add(ground.WorldToCell(new Vector2(p.x, p.y))); }
        try
        {
            if (waitingRoom != null && !cells.Exists(c => RoomAt(c, 0) == waitingRoom))
            {
                SetDoors(waitingRoom, false);
                NetCoop.RoomChanged(Rooms.IndexOf(waitingRoom), 0);
                waitingRoom = null;
            }
            if (waitingRoom == null)
                for (int i = 0; i < cells.Count; i++)
                {
                    Room candidate = RoomAt(cells[i], 1);
                    if (candidate == null || candidate.active || candidate.cleared || !CombatRoom(candidate)) continue;
                    if (candidate.doorCells.Count == 0) continue;
                    var from = previousPlayerCells.TryGetValue(ids[i], out var previous) ? previous : cells[i];
                    var nearest = candidate.doorCells[0];
                    foreach (var door in candidate.doorCells)
                        if ((door - from).sqrMagnitude < (nearest - from).sqrMagnitude) nearest = door;
                    waitingRoom = candidate;
                    SetWaitingDoors(candidate, DoorSide(candidate, nearest));
                    NetCoop.RoomChanged(Rooms.IndexOf(candidate), 3 + candidate.waitingEntrance);
                    break;
                }
            if (waitingRoom == null || cells.Count < NetCoop.AliveCount) return;
            // v1.2.1 (FIX-027): els raconets i les vores també compten (abans calia estar una casella endins)
            foreach (var c in cells) if (RoomAt(c, 0) != waitingRoom || waitingRoom.doorCells.Contains(new Vector3Int(c.x, c.y, 0))) return;
            Room ready = waitingRoom;
            waitingRoom = null;
            TryStartRoom(new Vector3Int(ready.Center.x, ready.Center.y, 0));
        }
        finally
        {
            for (int i = 0; i < cells.Count; i++) previousPlayerCells[ids[i]] = cells[i];
        }
    }

    Room hintRoom;
    void WaitingHint(Vector3Int pc)
    {
        Room r = RoomAt(pc, 1);
        bool waiting = r != null && !r.cleared && !r.active && CombatRoom(r);
        if (waiting && hintRoom != r) NetToast.Show("Esperant que entren tots els companys a la sala...", 2.5f);
        hintRoom = waiting ? r : null;
    }

    void TryStartRoom(Vector3Int cell)
    {
        Room r = RoomAt(cell, 1);
        if (r != null && !r.cleared && !r.active && CombatRoom(r))
            StartCoroutine(RunRoom(r));
    }

    static IEnumerable<Vector2> RemotePlayerPositions()
    {
        var s = NetSession.Instance;
        if (s == null) yield break;
        int token = NetSession.SceneToken();
        double now = Time.realtimeSinceStartupAsDouble;
        foreach (var p in s.Players.Values)
            if (p.id != s.LocalId && p.scene == token && now - p.lastPos < 2) yield return new Vector2(p.x, p.y);
    }

    // ------------------------------------------------------------------ Online (ho crida NetCoop als clients)

    public void NetRoomState(int index, int state)
    {
        if (index < 0 || index >= Rooms.Count) return;
        Room r = Rooms[index];
        if (state == 7) { r.ambushTriggered = true; return; }
        if (state >= 3 && state <= 6 && !r.active && !r.cleared) SetWaitingDoors(r, state - 3);
        else if (state == 0 && !r.active && !r.cleared) SetDoors(r, false);
        else if (state == 1 && !r.active && !r.cleared)
        {
            r.active = true;
            r.ambushTriggered = true;
            r.combatStarted = Time.time;
            SetDoors(r, true);
            ShowProgressionBanner(r);
            if (r.type == RoomType.Boss)
            {
                GameObject boss = Theme != null && Theme.boss != null ? Theme.boss : bossPrefab;
                EnemyBoss info = boss != null ? boss.GetComponent<EnemyBoss>() : null;
                ui.ShowBanner(info != null ? info.bossName.ToUpper() : "BOSS", 1.5f);
            }
        }
        else if (state == 2 && !r.cleared)
        {
            if (r.type != RoomType.Start && r.type != RoomType.Treasure) RunStats.RoomCleared();
            r.cleared = true; r.active = false;
            SetDoors(r, false);
            ui.RefreshMap();
        }
    }

    public void NetSpawnChest(int rarity, Vector2Int cell) => SpawnChest((ChestRarity)rarity, cell);
    public void NetSpawnPortal(Vector2Int cell) => SpawnPortal(cell);

    public void NetNextFloor(int floor)
    {
        if (Mathf.Abs(floor - RunState.Floor) == 1 && floor > 0) LoadFloor(floor);
    }

    public void NetGameOver() => PlayerDied();

    // Hi ha alguna sala en combat ara mateix?
    public bool AnyRoomFighting => Rooms.Exists(r => r.active && !r.cleared);

    // El boti i la decoracio d'una sala de combat es desbloquegen en netejar-la.
    // Tambe protegeix una sala encara no activada (no es pot saquejar des de la porta).
    public static bool CanInteractAt(Vector3 position)
    {
        var g = Instance;
        if (g == null) return true;
        Room r = g.RoomAt(g.ground.WorldToCell(position), 0);
        return r == null || r.cleared || !CombatRoom(r);
    }

    public static bool CanDamageBreakableAt(Vector3 position)
    {
        if (CanInteractAt(position)) return true;
        var g = Instance;
        Room r = g.RoomAt(g.ground.WorldToCell(position), 0);
        // Les caixes i TNT es poden trencar quan comença el combat.
        return r != null && r.active;
    }

    Room RoomAt(Vector3Int cell, int inset)
    {
        foreach (Room r in Rooms)
        {
            RectInt q = r.rect;
            if (cell.x >= q.xMin + inset && cell.x < q.xMax - inset && cell.y >= q.yMin + inset && cell.y < q.yMax - inset && InsideShape(r, new Vector2Int(cell.x, cell.y)))
                return r;
        }
        return null;
    }

    IEnumerator RunRoom(Room r)
    {
        r.active = true;
        r.combatStarted = Time.time;
        SetDoors(r, true);
        NetCoop.RoomChanged(Rooms.IndexOf(r), 1);
        if (r.modifier == RoomModifier.Flames) StartCoroutine(RoomFlames(r));
        yield return new WaitForSeconds(0.4f);

        if (r.type == RoomType.Boss)
        {
            GameObject boss = Theme != null && Theme.boss != null ? Theme.boss : bossPrefab;
            EnemyBoss info = boss != null ? boss.GetComponent<EnemyBoss>() : null;
            ui.ShowBanner(info != null ? info.bossName.ToUpper() : "BOSS", 1.5f);
            yield return SpawnEnemy(boss, r.Center + new Vector2Int(0, r.rect.height / 4), r, 1.2f);
            yield return WaitClear(r);
            // Recompensa: cofre llegendari i portal
            Vector2Int bossChest = BossChestCell(r); // BUG-022: lluny de qualsevol jugador
            ChestRarity bossRarity = BossRarity();
            SpawnChest(bossRarity, bossChest);
            NetCoop.ChestSpawned((int)bossRarity, bossChest);
            SpawnPortal(r.Center + NextPortalOffset);
            NetCoop.PortalSpawned(r.Center + NextPortalOffset);
        }
        else
        {
            ShowProgressionBanner(r);
            if (r == ProgressionRoom && RunState.Floor >= 4) StartCoroutine(ResonancePulses(r));
            int waves = 1 + (rng.NextDouble() < 0.5 ? 1 : 0) + (RunState.Floor >= 3 ? 1 : 0);
            if (r.type == RoomType.Emboscada) waves = 2;
            if (r.type == RoomType.Arena) waves = 3;
            for (int w = 0; w < waves; w++)
            {
                // Pis 1: 3-4 enemics per onada; cada pis en suma un
                int count = 2 + RunState.Floor + rng.Next(0, 2);
                for (int i = 0; i < count; i++)
                {
                    bool elite = r == ProgressionRoom && w == 0 && i == 0;
                    StartCoroutine(SpawnEnemy(elite ? PickElite() : PickEnemy(), RandomSpawnCell(r), r, elite ? 1.25f : 0f));
                }
                yield return new WaitForSeconds(0.9f);
                yield return WaitClear(r);
                if (w < waves - 1) yield return new WaitForSeconds(0.6f);
            }
            // Recompensa per netejar la sala
            if (r == ProgressionRoom || r.type == RoomType.Arena || r.type == RoomType.Emboscada || rng.NextDouble() < 0.5)
            {
                ChestRarity reward = r == ProgressionRoom
                    ? (RunState.Floor >= 5 ? ChestRarity.Epic : ChestRarity.Rar)
                    : RandomRarity();
                if (r.type == RoomType.Emboscada) reward = (ChestRarity)Mathf.Min(3, (int)r.ambushRarity + 1);
                if (r.type == RoomType.Arena) for (int i = 0; i < 5; i++) CoinDrop.Spawn(ground.GetCellCenterWorld(ToCell(r.Center)));
                Vector2Int offset = new Vector2Int(2, 0);
                SpawnChest(reward, r.Center + offset);
                NetCoop.ChestSpawned((int)reward, r.Center + offset);
            }
        }

        if (!r.cleared) RunStats.RoomCleared();
        r.cleared = true;
        r.active = false;
        SetDoors(r, false);
        ui.RefreshMap();
        NetCoop.RoomChanged(Rooms.IndexOf(r), 2);
    }

    void ShowProgressionBanner(Room r)
    {
        if (r == ProgressionRoom)
            ui.ShowBanner(RunState.Floor >= 4 ? "CAMBRA DE RESSONÀNCIA" : "GUÀRDIA D'ELIT", 2f);
    }

    IEnumerator ResonancePulses(Room r)
    {
        const float radius = 1.45f;
        const float warning = 1.15f;
        int next = 0;
        while (r.active && !r.cleared && !gameOver)
        {
            yield return new WaitForSeconds(4.5f);
            if (r.cleared || gameOver) yield break;
            if (!r.alive.Exists(enemy => enemy != null && !enemy.IsDead && enemy.enabled)) continue;

            Vector2Int cell = r.Center + Dirs[next] * 2;
            next = (next + 1) % Dirs.Length;
            Vector2 pos = ground.GetCellCenterWorld(ToCell(cell));
            if (!TileNav.CircleFree(pos, radius)) continue;

            NetShared.Telegraph(pos, radius * 2f, warning);
            yield return NetShared.ShowTelegraph(pos, radius * 2f, warning);
            if (r.cleared || gameOver || !r.alive.Exists(enemy => enemy != null && !enemy.IsDead && enemy.enabled)) continue;

            Fx.ExplodeVisual(pos, radius);
            NetCoop.Exploded(pos, radius, 1);
            if (player != null && player.gameObject.activeInHierarchy && Vector2.Distance(player.position, pos) <= radius + .3f)
                NetCoop.DamagePlayer(player, 1);
        }
    }

    GameObject PickElite()
    {
        if (themes != null && themes.Length > 1)
        {
            var previous = themes[(RunState.Floor - 2) % themes.Length];
            var candidates = previous != null ? previous.enemies : null;
            if (candidates != null && candidates.Length > 0)
            {
                var candidate = candidates[rng.Next(candidates.Length)];
                if (candidate != null) return candidate;
            }
        }
        return PickEnemy();
    }

    IEnumerator WaitClear(Room r)
    {
        while (true)
        {
            r.alive.RemoveAll(e => e == null || e.IsDead);
            if (r.alive.Count == 0) yield break;
            yield return new WaitForSeconds(0.2f);
        }
    }

    GameObject PickEnemy()
    {
        // Cada tema té els seus enemics (tots igual de probables)
        if (Theme != null && Theme.enemies != null && Theme.enemies.Length > 0)
        {
            GameObject e = Theme.enemies[rng.Next(Theme.enemies.Length)];
            if (e != null) return e;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;
        int f = RunState.Floor;
        float[] w = { 3f, 3f, f >= 1 ? 2f : 0f, f >= 2 ? 2f : 0.6f };
        float total = 0f;
        for (int i = 0; i < enemyPrefabs.Length && i < w.Length; i++) if (enemyPrefabs[i] != null) total += w[i];
        float x = (float)rng.NextDouble() * total;
        for (int i = 0; i < enemyPrefabs.Length && i < w.Length; i++)
        {
            if (enemyPrefabs[i] == null) continue;
            x -= w[i];
            if (x <= 0f) return enemyPrefabs[i];
        }
        return enemyPrefabs[0];
    }

    Vector2Int RandomSpawnCell(Room r)
    {
        for (int i = 0; i < 40; i++)
        {
            Vector2Int p = new Vector2Int(rng.Next(r.rect.xMin + 1, r.rect.xMax - 1), rng.Next(r.rect.yMin + 1, r.rect.yMax - 1));
            Vector3Int cell = ToCell(p);
            if (!InsideShape(r, p) || !floor.Contains(p) || TileNav.IsBlocked(cell) || reserved.Contains(p)) continue;
            if (player != null && Vector2.Distance(ground.GetCellCenterWorld(cell), player.position) < 3.5f) continue;
            return p;
        }
        return r.Center;
    }

    // Avís (cercle roig que creix) i després apareix l'enemic
    IEnumerator SpawnEnemy(GameObject prefab, Vector2Int cell, Room r, float scaleBonus)
    {
        if (prefab == null) yield break;
        Vector3 pos = ground.GetCellCenterWorld(ToCell(cell));

        // Compta ja com a viu, perquè la sala no es done per acabada abans d'hora
        var placeholder = new GameObject("Spawn").AddComponent<EnemyHealth>();
        placeholder.enabled = false;
        r.alive.Add(placeholder);

        SpriteRenderer mark = Fx.MakeSprite("Avis enemic", pos, Fx.Disc, new Color(1f, 0.25f, 0.3f, 0.5f), "Shadows", 5);
        NetCoop.SpawnMark(pos);
        float t = 0f;
        while (t < 0.7f)
        {
            mark.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 0.9f, t / 0.7f);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(mark.gameObject);

        GameObject e = Instantiate(prefab, pos, Quaternion.identity);
        if (scaleBonus > 0f) e.transform.localScale *= scaleBonus;
        EnemyHealth h = e.GetComponent<EnemyHealth>();
        if (h != null)
        {
            float mult = 1f + 0.3f * (RunState.Floor - 1);
            if (r.type == RoomType.Boss)
            {
                float partyMultiplier = 1f + 0.55f * (NetCoop.PlayerCount - 1);
                h.maxHealth = Mathf.RoundToInt((h.maxHealth + 25 * (RunState.Floor - 1)) * partyMultiplier);
            }
            else
            {
                if (scaleBonus > 0f) mult *= 1.5f;
                h.maxHealth = Mathf.RoundToInt(h.maxHealth * mult);
            }
            h.currentHealth = h.maxHealth;
            r.alive.Add(h);
        }
        e.GetComponent<EnemyPathfindingAI>()?.Alert();
        Fx.Burst(pos, new Color(1f, 0.4f, 0.45f), 6, 2f);

        r.alive.Remove(placeholder);
        Destroy(placeholder.gameObject);
    }

    // Els llims que es divideixen i els fills del boss també compten
    public bool TryChildSpawn(EnemyHealth parent, Vector2 desired, out Vector2 position)
    {
        position = desired;
        Room room = Rooms.Find(r => r.alive.Contains(parent));
        if (room == null) return true;

        float bestDistance = float.PositiveInfinity;
        foreach (Vector2Int cell in room.rect.allPositionsWithin)
        {
            Vector3Int tile = ToCell(cell);
            if (TileNav.IsBlocked(tile)) continue;
            Vector2 center = TileNav.Center(tile);
            float distance = (center - desired).sqrMagnitude;
            if (distance >= bestDistance) continue;
            bestDistance = distance;
            position = center;
        }
        return !float.IsPositiveInfinity(bestDistance);
    }

    public void RegisterSpawned(EnemyHealth h)
    {
        if (h == null) return;
        foreach (Room r in Rooms)
            if (r.active && !r.cleared) { r.alive.Add(h); return; }
    }

    void SetDoors(Room r, bool closed)
    {
        r.waitingEntrance = -1;
        foreach (Vector3Int c in r.doorCells)
            doors.SetTile(c, closed ? doorTile : null);
        Fx.Play(Fx.Door, 0.6f);
        if (closed) Fx.Shake(0.15f, 0.1f);
    }

    // Les tres caselles d'una porta han de romandre juntes.
    public static int DoorSide(Room r, Vector3Int c)
    {
        if (c.x < r.rect.xMin) return 0;
        if (c.x >= r.rect.xMax) return 1;
        return c.y < r.rect.yMin ? 2 : 3;
    }

    void SetWaitingDoors(Room r, int entrance)
    {
        r.waitingEntrance = entrance;
        foreach (var c in r.doorCells) doors.SetTile(c, DoorSide(r, c) == entrance ? null : doorTile);
    }

    // El cofre del boss apareix a l'esquerra del centre, però si hi ha algú just allí (pot quedar-se encallat
    // dins del col·lisionador) tria una altra casella lliure de la sala.
    Vector2Int BossChestCell(Room r)
    {
        var options = new[] { new Vector2Int(-2, 0), new Vector2Int(-2, 2), new Vector2Int(-2, -2), new Vector2Int(-3, 0), new Vector2Int(0, -3), new Vector2Int(0, 3) };
        var people = new List<Vector2>();
        if (player != null && player.gameObject.activeInHierarchy) people.Add(player.position);
        if (NetCoop.Online) foreach (var p in RemotePlayerPositions()) people.Add(p);
        foreach (var o in options)
        {
            Vector2Int cell = r.Center + o;
            Vector3 world = ground.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
            bool free = !TileNav.IsBlocked(new Vector3Int(cell.x, cell.y, 0));
            foreach (var p in people) if (Vector2.Distance(p, world) < 1.6f) free = false;
            if (free) return cell;
        }
        return r.Center + options[0];
    }

    static readonly Vector2Int NextPortalOffset = new Vector2Int(2, 0);

    public Vector3 KeepDropClearOfPortals(Vector3 position)
    {
        Room boss = Rooms.Find(room => room.type == RoomType.Boss);
        if (boss == null) return position;
        Vector2 next = TileNav.Center(ToCell(boss.Center + NextPortalOffset));
        Vector2 back = PortalPosition(-1);
        bool Near(Vector2 point, Vector2 portal) => (point - portal).sqrMagnitude < 1.44f;
        bool Clear(Vector2 point) => !Near(point, next) && (RunState.Floor == 1 || !Near(point, back));
        if (Clear(position)) return position;
        Room room = Near(position, next) ? boss : Rooms[0];
        float best = float.PositiveInfinity;
        Vector3 result = position;
        foreach (Vector2Int cell in room.rect.allPositionsWithin)
        {
            Vector3Int tile = ToCell(cell);
            if (TileNav.IsBlocked(tile)) continue;
            Vector2 center = TileNav.Center(tile);
            float distance = (center - (Vector2)position).sqrMagnitude;
            if (!Clear(center) || distance >= best) continue;
            best = distance;
            result = new Vector3(center.x, center.y, position.z);
        }
        return result;
    }

    bool portalsSpawned;

    void SpawnPortal(Vector2Int cell)
    {
        if (portalsSpawned) return;
        portalsSpawned = true;
        Vector3 pos = ground.GetCellCenterWorld(ToCell(cell));
        if (portalPrefab != null) Instantiate(portalPrefab, pos, Quaternion.identity);
        else DungeonPortal.Create(pos);
    }

    // ------------------------------------------------------------------ Canvi de pis i mort

    public void NextFloor()
    {
        ChangeFloor(1);
    }

    void LoadFloor(int floor)
    {
        if (changingFloor) return;
        changingFloor = true;
        if (!NetCoop.IsClient) RunState.Floors[RunState.Floor] = CaptureFloor();
        PlayerAttack attack = player != null ? player.GetComponent<PlayerAttack>() : null;
        attack?.SaveRunInventory();
        PlayerHealth health = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (health != null) RunState.Health = health.currentHealth;
        RunState.ArrivalDirection = floor - RunState.Floor;
        RunState.Floor = floor;
        RunStats.Floor(floor);
        RunState.FloorVisit++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PlayerDied()
    {
        if (gameOver) return;
        gameOver = true;
        StartCoroutine(GameOver());
    }

    // Resum de partida. Sol: tornar a jugar o menu. Online: decideix l'amfitrio
    // (tornar a jugar tots o tornar al lobby); el client espera o ix de la sala.
    IEnumerator GameOver()
    {
        bool coop = NetCoop.Online;
        var records = RunStats.CommitRecords(coop);
        if (!coop) ui.ShowSummary(records, false, false, RestartRun, ToMainMenu, "MENÚ PRINCIPAL");
        else if (NetCoop.IsHost) ui.ShowSummary(records, true, false, NetCoop.RestartRunAll, () => NetSession.Instance.BackToLobby(), "TORNAR AL LOBBY");
        else ui.ShowSummary(records, true, true, null, LeaveRoom, null);
        yield break;
    }

    // Comença una partida nova des del pis 1 (sol, o quan l'amfitrio ho diu)
    public void RestartRun()
    {
        RunState.Reset();
        RunState.Active = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    static void ToMainMenu()
    {
        RunState.Reset();
        SceneManager.LoadScene("MainMenu");
    }

    static void LeaveRoom()
    {
        if (NetSession.Active) NetSession.Instance.Leave(null);
        RunState.Reset();
        SceneManager.LoadScene("MainMenu");
    }
}
