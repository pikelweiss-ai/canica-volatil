using System.Collections.Generic;
using UnityEngine;

public partial class DungeonGenerator
{
    const int KeyOnGround = -1;
    const int KeySpent = -2;
    Room lockedBossRoom;
    public Vector3 BossKeyPosition { get; private set; }
    public int BossKeyOwner { get; private set; } = KeySpent;
    public bool BossKeyLocked => lockedBossRoom != null && BossKeyOwner != KeySpent;
    GameObject keyObject;
    static Sprite keySprite;

    public bool IsKeyDoorCell(Vector3Int cell) => BossKeyLocked && lockedBossRoom.doorCells.Contains(cell);
    public Vector3Int[] KeyDoorCenters
    {
        get
        {
            if (!BossKeyLocked) return System.Array.Empty<Vector3Int>();
            var cells = lockedBossRoom.doorCells;
            var centers = new Vector3Int[cells.Count / 3];
            for (int i = 0; i < centers.Length; i++) centers[i] = cells[i * 3 + 1];
            return centers;
        }
    }

    void PlaceBossKey()
    {
        lockedBossRoom = Rooms.Find(r => r.type == RoomType.Boss);
        if (lockedBossRoom == null || lockedBossRoom.doorCells.Count == 0)
        {
            lockedBossRoom = null;
            return;
        }

        // Tria només sales accessibles des de l'inici amb la porta del boss tancada.
        var reached = new HashSet<Room> { Rooms[0], lockedBossRoom };
        var queue = new Queue<Room>();
        var candidates = new List<Vector2Int>();
        queue.Enqueue(Rooms[0]);
        while (queue.Count > 0)
        {
            var room = queue.Dequeue();
            foreach (var next in room.links)
                if (next.type != RoomType.Secreta && reached.Add(next)) queue.Enqueue(next);
            if (room == Rooms[0]) continue;
            foreach (var cell in room.rect.allPositionsWithin)
            {
                if (!FreeForDecor(room, cell, 2) || TileNav.IsBlocked(ToCell(cell))) continue;
                candidates.Add(cell);
                break;
            }
        }

        Vector2Int position;
        if (candidates.Count == 0)
        {
            position = Rooms[0].Center + Vector2Int.right * 2;
        }
        else
        {
            int seed = NetCoop.Online ? NetSession.Instance.Seed : RunState.Seed;
            position = candidates[PosHash(new Vector2Int(seed, RunState.Floor), 71) % candidates.Count];
        }
        BossKeyPosition = ground.GetCellCenterWorld(ToCell(position));
        keyObject = Fx.MakeSprite("Clau del boss", BossKeyPosition, BossKeySprite, Color.white, "Player", 18).gameObject;
        keyObject.transform.SetParent(transform, true);
        SetBossKeyState(KeyOnGround);
    }

    static Sprite BossKeySprite
    {
        get
        {
            if (keySprite != null) return keySprite;
            string[] pixels =
            {
                "..###.......",
                ".#ggg#......",
                "#gg#gg#.....",
                "#g#.#g#.....",
                "#gg#gg####..",
                ".#ggggggg#..",
                "..####g#gg#.",
                ".....#g####.",
                "......#....."
            };
            var tex = new Texture2D(pixels[0].Length, pixels.Length) { filterMode = FilterMode.Point };
            for (int y = 0; y < pixels.Length; y++)
                for (int x = 0; x < pixels[y].Length; x++)
                    tex.SetPixel(x, pixels.Length - 1 - y, pixels[y][x] == '#' ? new Color(.32f, .19f, .08f) :
                        pixels[y][x] == 'g' ? new Color(1f, .81f, .27f) : Color.clear);
            tex.Apply();
            keySprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * .5f, 16);
            return keySprite;
        }
    }

    void TickBossKey()
    {
        if (lockedBossRoom == null || NetCoop.IsClient || !BossKeyLocked) return;
        int localId = NetCoop.Online ? NetSession.Instance.LocalId : 0;
        if (BossKeyOwner == KeyOnGround)
        {
            if (player != null && player.gameObject.activeInHierarchy && NearKey(player.position))
            {
                ChangeBossKeyOwner(localId);
                return;
            }
            if (NetCoop.IsHost)
                foreach (var peer in NetSession.Instance.Players.Values)
                    if (peer.id != localId && IsKeyPeerPresent(peer) && NearKey(new Vector2(peer.x, peer.y)))
                    { ChangeBossKeyOwner(peer.id); return; }
            return;
        }

        if (BossKeyOwner == localId && player != null && player.gameObject.activeInHierarchy && !NetCoop.IsDown(localId))
        {
            if (NearBossLock(player.position)) ChangeBossKeyOwner(KeySpent);
            return;
        }
        if (NetCoop.IsHost && BossKeyOwner != localId &&
            NetSession.Instance.Players.TryGetValue(BossKeyOwner, out var carrier) && IsKeyPeerPresent(carrier))
        {
            if (NearBossLock(new Vector2(carrier.x, carrier.y))) ChangeBossKeyOwner(KeySpent);
            return;
        }
        // Si qui la porta cau o ix, la clau torna a la sala on estava.
        ChangeBossKeyOwner(KeyOnGround);
    }

    bool NearKey(Vector2 position) => Vector2.Distance(position, BossKeyPosition) < .8f;

    bool NearBossLock(Vector2 position)
    {
        for (int i = 1; i < lockedBossRoom.doorCells.Count; i += 3)
            if (Vector2.Distance(position, ground.GetCellCenterWorld(lockedBossRoom.doorCells[i])) < 1.65f)
                return true;
        return false;
    }

    static bool IsKeyPeerPresent(NetPlayer peer) =>
        peer.scene == NetSession.SceneToken() && !NetCoop.IsDown(peer.id) &&
        Time.realtimeSinceStartupAsDouble - peer.lastPos < 2d;

    void ChangeBossKeyOwner(int owner)
    {
        NetBossKeyState(owner);
        NetCoop.BossKeyChanged(owner);
    }

    public void NetBossKeyState(int owner)
    {
        if (lockedBossRoom == null || owner == BossKeyOwner) return;
        SetBossKeyState(owner);
        if (owner == KeySpent) ui?.ShowBanner("PORTA DEL BOSS OBERTA", 1.7f);
        else if (owner >= 0) ui?.ShowBanner("CLAU TROBADA", 1.4f);
    }

    void SetBossKeyState(int owner)
    {
        BossKeyOwner = owner;
        if (keyObject != null) keyObject.SetActive(owner == KeyOnGround);
        if (lockedBossRoom == null || lockedBossRoom.active || lockedBossRoom.cleared) return;
        foreach (var cell in lockedBossRoom.doorCells) doors.SetTile(cell, BossKeyLocked ? doorTile : null);
    }

    public string BossKeyLabel
    {
        get
        {
            if (lockedBossRoom == null) return "";
            if (BossKeyOwner == KeySpent) return "Porta del boss oberta";
            if (BossKeyOwner == KeyOnGround) return "Busca la clau del boss";
            var session = NetSession.Instance;
            if (!NetCoop.Online || BossKeyOwner == session.LocalId) return "Portes la clau del boss";
            return session.Players.TryGetValue(BossKeyOwner, out var peer) ? "Clau: " + NetSession.Safe(peer.name) : "Clau trobada";
        }
    }
}

