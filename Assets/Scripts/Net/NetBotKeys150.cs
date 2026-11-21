using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class NetBot
{
    static Vector3 LockAddress(DungeonGenerator dungeon)
    {
        Vector3Int cell = dungeon.KeyDoorCenters[0];
        Vector3 at = dungeon.ground.GetCellCenterWorld(cell);
        var boss = dungeon.Rooms.First(r => r.type == DungeonGenerator.RoomType.Boss);
        int side = DungeonGenerator.DoorSide(boss, cell);
        Vector3 outwards = side == 0 ? Vector3.left : side == 1 ? Vector3.right : side == 2 ? Vector3.down : Vector3.up;
        return at + outwards;
    }

    IEnumerator Keys150()
    {
        RunState.Reset(); RunState.Active = true; RunState.Seed = 152;
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => DungeonGenerator.Instance != null, 15, "masmorra amb porta i clau");
        if (failed) yield break;
        var dungeon = DungeonGenerator.Instance;
        var boss = dungeon.Rooms.First(room => room.type == DungeonGenerator.RoomType.Boss);
        var keyAt = (Vector2Int)dungeon.ground.WorldToCell(dungeon.BossKeyPosition);
        var keyRoom = dungeon.Rooms.First(room => room.rect.Contains(keyAt));
        if (keyRoom == boss || keyRoom.type == DungeonGenerator.RoomType.Start || !dungeon.BossKeyLocked ||
            dungeon.KeyDoorCenters.Any(cell => !dungeon.doors.HasTile(cell) || !TileNav.IsBlocked(cell)))
        { Fail("la clau ha d'estar en una altra sala i la porta ha de parar el pas"); yield break; }
        MovePhase0Player(LockAddress(dungeon));
        yield return null;
        if (!dungeon.BossKeyLocked || boss.active)
        { Fail("la porta o el combat han obert sense clau"); yield break; }
        yield return Shot("fase2-cadenat");

        yield return OpenBossWithKey(dungeon);
        if (failed) yield break;
        yield return Shot("fase2-porta-oberta");
        if (dungeon.BossKeyOwner != -2)
        { Fail("la clau continua a l'inventari"); yield break; }

        MovePhase0Player(dungeon.ground.GetCellCenterWorld((Vector3Int)boss.Center));
        yield return WaitFor(() => boss.active, 4, "la sala del boss arranca quan s'entra per la porta oberta");
        if (!failed) Pass();
    }

    IEnumerator OpenBossWithKey(DungeonGenerator dungeon)
    {
        Vector3 address = LockAddress(dungeon);
        MovePhase0Player(dungeon.BossKeyPosition);
        yield return WaitFor(() => dungeon.BossKeyOwner >= 0, 6, "clau recollida a l'altra sala");
        if (failed) yield break;
        yield return Shot("fase2-clau-hud");
        MovePhase0Player(address);
        yield return WaitFor(() => !dungeon.BossKeyLocked, 8, "la clau obri la porta i es gasta");
    }

    IEnumerator HostKeys150()
    {
        var dungeon = DungeonGenerator.Instance;
        var cells = dungeon.KeyDoorCenters;
        yield return WaitFor(() => dungeon.BossKeyOwner > 0 && dungeon.BossKeyOwner != NetSession.Instance.LocalId,
            16, "la clau la porta el client");
        if (failed) yield break;
        yield return WaitFor(() => !dungeon.BossKeyLocked && cells.All(cell => !dungeon.doors.HasTile(cell)),
            14, "la clau del client obri la porta a l'amfitrio");
    }

    IEnumerator JoinKeys150()
    {
        var dungeon = DungeonGenerator.Instance;
        if (!dungeon.BossKeyLocked)
        { Fail("el client ha carregat amb el cadenat obert"); yield break; }
        var cells = dungeon.KeyDoorCenters;
        yield return OpenBossWithKey(dungeon);
        if (failed) yield break;
        if (cells.Any(cell => dungeon.doors.HasTile(cell)))
            Fail("el client encara te les caselles de la porta tancades");
    }
}

