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
public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance { get; private set; }

    public enum RoomType { Start, Combat, Treasure, Boss }

    public class Room
    {
        public Vector2Int slot;
        public RoomType type;
        public RectInt rect;                          // Cel·les de terra de dins de la sala
        public readonly List<Room> links = new List<Room>();
        public readonly List<Vector3Int> doorCells = new List<Vector3Int>();
        public readonly List<EnemyHealth> alive = new List<EnemyHealth>();
        public bool visited, active, cleared;
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

        rng = new System.Random(seed != 0 ? seed : System.Environment.TickCount);
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

        // El jugador comença al mig de la sala inicial
        Room start = Rooms[0];
        Vector3 startPos = ground.GetCellCenterWorld(ToCell(start.Center));
        if (player != null) player.position = new Vector3(startPos.x, startPos.y, player.position.z);
        if (spawnPoint != null) spawnPoint.position = startPos;
        Camera cam = Camera.main;
        if (cam != null) cam.transform.position = new Vector3(startPos.x, startPos.y, cam.transform.position.z);
        start.visited = true;
        CurrentRoom = start;

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
            if (neighbours > 1) continue; // Arbre: cada sala nova toca només la seua mare

            Room r = new Room { slot = s, type = RoomType.Combat };
            r.links.Add(from);
            from.links.Add(r);
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
            bool leaf = r.links.Count == 1;
            if (boss == null || (leaf && (boss.links.Count != 1 || dist[r] > dist[boss])) || (!leaf && boss.links.Count != 1 && dist[r] > dist[boss]))
                boss = r;
        }
        if (boss != null) boss.type = RoomType.Boss;

        // Tresor: una altra sala sense eixida (si n'hi ha)
        var leaves = Rooms.FindAll(r => r.type == RoomType.Combat && r.links.Count == 1);
        if (leaves.Count > 0) leaves[rng.Next(leaves.Count)].type = RoomType.Treasure;

        // Mida de cada sala (senars, perquè tinguen un centre exacte)
        foreach (Room r in Rooms)
        {
            Vector2Int size;
            switch (r.type)
            {
                case RoomType.Start: size = new Vector2Int(11, 11); break;
                case RoomType.Treasure: size = new Vector2Int(9, 9); break;
                case RoomType.Boss: size = new Vector2Int(19, 15); break;
                default:
                    size = new Vector2Int(Odd(rng.Next(roomSizeMin.x, roomSizeMax.x + 1)), Odd(rng.Next(roomSizeMin.y, roomSizeMax.y + 1)));
                    break;
            }
            Vector2Int c = r.slot * slotSize;
            r.rect = new RectInt(c.x - size.x / 2, c.y - size.y / 2, size.x, size.y);
        }
    }

    static int Odd(int n) => n % 2 == 0 ? n + 1 : n;

    void Carve()
    {
        foreach (Room r in Rooms)
            foreach (Vector2Int p in r.rect.allPositionsWithin) floor.Add(p);

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

        switch (rng.Next(4))
        {
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

    Vector2Int RandomCell(Room r) => new Vector2Int(rng.Next(r.rect.xMin, r.rect.xMax), rng.Next(r.rect.yMin, r.rect.yMax));

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
        foreach (Vector2Int p in cratesToSpawn) Spawn(cratePrefab, p);
        foreach (Vector2Int p in barrelsToSpawn) Spawn(barrelPrefab, p);
        foreach (Vector2Int p in spikesToSpawn) Spawn(spikePrefab, p);

        foreach (Room r in Rooms)
        {
            if (r.type == RoomType.Treasure)
            {
                ChestRarity rarity = RunState.Floor >= 3 && rng.NextDouble() < 0.4 ? ChestRarity.Llegendari
                                   : rng.NextDouble() < 0.5 ? ChestRarity.Epic : ChestRarity.Rar;
                SpawnChest(rarity, r.Center);
            }
            else if (r.type == RoomType.Start)
            {
                // Cofre a la sala inicial de cada pis (al pis 1 és la primera arma)
                SpawnChest(RunState.Floor >= 3 && rng.NextDouble() < 0.4 ? ChestRarity.Rar : ChestRarity.Comu, r.Center + new Vector2Int(3, 3));
            }
            else if (r.type == RoomType.Combat && rng.NextDouble() < 0.45)
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
    }

    // Comú 55 %, Rar 30 %, Èpic 12 %, Llegendari 3 % (més bons als pisos de dalt)
    ChestRarity RandomRarity()
    {
        double x = rng.NextDouble() - 0.04 * (RunState.Floor - 1);
        if (x < 0.03) return ChestRarity.Llegendari;
        if (x < 0.15) return ChestRarity.Epic;
        if (x < 0.45) return ChestRarity.Rar;
        return ChestRarity.Comu;
    }

    GameObject Spawn(GameObject prefab, Vector2Int cell)
    {
        if (prefab == null) return null;
        return Instantiate(prefab, ground.GetCellCenterWorld(ToCell(cell)), Quaternion.identity);
    }

    void SpawnChest(ChestRarity rarity, Vector2Int cell)
    {
        if (chestPrefabs == null || chestPrefabs.Length <= (int)rarity) return;
        Spawn(chestPrefabs[(int)rarity], cell);
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
        if (r != null && !r.cleared && !r.active && (r.type == RoomType.Combat || r.type == RoomType.Boss) && RoomAt(pc, 1) == r)
            StartCoroutine(RunRoom(r));

        if (r != null && (r.type == RoomType.Start || r.type == RoomType.Treasure)) r.cleared = true;
    }

    Room RoomAt(Vector3Int cell, int inset)
    {
        foreach (Room r in Rooms)
        {
            RectInt q = r.rect;
            if (cell.x >= q.xMin + inset && cell.x < q.xMax - inset && cell.y >= q.yMin + inset && cell.y < q.yMax - inset)
                return r;
        }
        return null;
    }

    IEnumerator RunRoom(Room r)
    {
        r.active = true;
        SetDoors(r, true);
        yield return new WaitForSeconds(0.4f);

        if (r.type == RoomType.Boss)
        {
            ui.ShowBanner("BOSS", 1.5f);
            yield return SpawnEnemy(bossPrefab, r.Center + new Vector2Int(0, r.rect.height / 4), r, 1.2f);
            yield return WaitClear(r);
            // Recompensa: cofre llegendari i portal
            SpawnChest(ChestRarity.Llegendari, r.Center + new Vector2Int(-2, 0));
            SpawnPortal(r.Center + new Vector2Int(2, 0));
        }
        else
        {
            int waves = 1 + (rng.NextDouble() < 0.5 ? 1 : 0) + (RunState.Floor >= 3 ? 1 : 0);
            for (int w = 0; w < waves; w++)
            {
                // Pis 1: 3-4 enemics per onada; cada pis en suma un
                int count = 2 + RunState.Floor + rng.Next(0, 2);
                for (int i = 0; i < count; i++)
                    StartCoroutine(SpawnEnemy(PickEnemy(), RandomSpawnCell(r), r, 0f));
                yield return new WaitForSeconds(0.9f);
                yield return WaitClear(r);
                if (w < waves - 1) yield return new WaitForSeconds(0.6f);
            }
            // Recompensa per netejar la sala
            if (rng.NextDouble() < 0.5)
                SpawnChest(RandomRarity(), r.Center + new Vector2Int(2, 2));
        }

        r.cleared = true;
        SetDoors(r, false);
        ui.RefreshMap();
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
            if (TileNav.IsBlocked(cell) || reserved.Contains(p)) continue;
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
            h.maxHealth = Mathf.RoundToInt(h.maxHealth * mult);
            h.currentHealth = h.maxHealth;
            r.alive.Add(h);
        }
        e.GetComponent<EnemyPathfindingAI>()?.Alert();
        Fx.Burst(pos, new Color(1f, 0.4f, 0.45f), 6, 2f);

        r.alive.Remove(placeholder);
        Destroy(placeholder.gameObject);
    }

    // Els llims que es divideixen i els fills del boss també compten
    public void RegisterSpawned(EnemyHealth h)
    {
        if (h == null) return;
        foreach (Room r in Rooms)
            if (r.active && !r.cleared) { r.alive.Add(h); return; }
    }

    void SetDoors(Room r, bool closed)
    {
        foreach (Vector3Int c in r.doorCells)
            doors.SetTile(c, closed ? doorTile : null);
        Fx.Play(Fx.Door, 0.6f);
        if (closed) Fx.Shake(0.15f, 0.1f);
    }

    void SpawnPortal(Vector2Int cell)
    {
        Vector3 pos = ground.GetCellCenterWorld(ToCell(cell));
        if (portalPrefab != null) Instantiate(portalPrefab, pos, Quaternion.identity);
        else DungeonPortal.Create(pos);
    }

    // ------------------------------------------------------------------ Canvi de pis i mort

    public void NextFloor()
    {
        PlayerAttack attack = player != null ? player.GetComponent<PlayerAttack>() : null;
        if (attack != null) RunState.Weapons = (WeaponData[])attack.hotbarSlots.Clone();
        PlayerHealth health = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (health != null) RunState.Health = health.currentHealth;
        RunState.Floor++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PlayerDied()
    {
        if (gameOver) return;
        gameOver = true;
        StartCoroutine(GameOver());
    }

    IEnumerator GameOver()
    {
        ui.ShowDeath(RunState.Floor, RunState.Coins);
        yield return new WaitForSeconds(3.5f);
        RunState.Reset();
        RunState.Active = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
