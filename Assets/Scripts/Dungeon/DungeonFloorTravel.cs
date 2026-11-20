using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DungeonGenerator
{
    internal sealed class FloorState
    {
        internal readonly List<(bool cleared, bool visited)> Rooms = new();
        internal readonly List<(Vector2Int cell, ChestRarity rarity, bool opened)> Chests = new();
        internal readonly Dictionary<int, int> BreakableHealth = new();
        internal readonly List<(Vector3 position, WeaponData weapon, HealItem heal, float cooldown)> Drops = new();
        internal bool ExitOpen;
    }

    readonly Dictionary<Vector2Int, Chest> floorChests = new();
    bool changingFloor;

    Vector3 PortalPosition(int direction)
    {
        Vector2Int cell = direction < 0
            ? Rooms[0].Center
            : Rooms.Find(room => room.type == RoomType.Boss).Center + NextPortalOffset;
        return ground.GetCellCenterWorld(ToCell(cell));
    }

    public bool TryTravel(int direction, Vector2 position, float tolerance = DungeonPortal.TriggerRadius)
    {
        if (changingFloor || gameOver || (direction != -1 && direction != 1)) return false;
        if (direction < 0 && (RunState.Floor <= 1 || (!NetCoop.IsClient && !RunState.Floors.ContainsKey(RunState.Floor - 1)))) return false;
        if (direction > 0 && !portalsSpawned) return false;
        if (waitingRoom != null || Rooms.Exists(room => room.active && !room.cleared)) return false;
        if (Vector2.Distance(position, PortalPosition(direction)) >= tolerance) return false;
        ChangeFloor(direction);
        return true;
    }

    void ChangeFloor(int direction)
    {
        if (changingFloor) return;
        if (NetCoop.InterceptFloorChange(direction)) return;
        LoadFloor(RunState.Floor + direction);
    }

    FloorState CaptureFloor()
    {
        var state = new FloorState { ExitOpen = portalsSpawned };
        foreach (var room in Rooms) state.Rooms.Add((room.cleared, room.visited));
        foreach (var entry in floorChests)
        {
            var chest = entry.Value;
            if (chest == null) continue;
            state.Chests.Add((entry.Key, chest.rarity, chest.IsOpen ||
                NetCoop.IsChestOpenedForAll(NetCoop.ChestKey(chest))));
        }

        foreach (var prop in FindObjectsByType<Breakable>(FindObjectsSortMode.None))
            if (prop.health > 0) state.BreakableHealth[NetShared.BreakableKey(prop)] = prop.health;

        Vector3 RestingPosition(Component pickup)
        {
            var networkItem = pickup.GetComponent<NetItem>();
            return networkItem != null ? networkItem.destination : pickup.transform.position;
        }

        foreach (var pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
            if (NetShared.IsUnclaimed(pickup)) state.Drops.Add((RestingPosition(pickup), pickup.weapon, null, pickup.CooldownRemaining));
        foreach (var pickup in FindObjectsByType<HealPickup>(FindObjectsSortMode.None))
            if (NetShared.IsUnclaimed(pickup)) state.Drops.Add((RestingPosition(pickup), null, pickup.item, 0f));
        foreach (var pickup in FindObjectsByType<CoinDrop>(FindObjectsSortMode.None))
            if (NetShared.IsUnclaimed(pickup)) state.Drops.Add((RestingPosition(pickup), null, null, 0f));

        return state;
    }

    void Start()
    {
        if (!NetCoop.IsClient && RunState.Floors.TryGetValue(RunState.Floor, out var saved))
            StartCoroutine(RestoreFloor(saved));
    }

    IEnumerator RestoreFloor(FloorState saved)
    {
        for (int i = 0; i < Rooms.Count; i++)
        {
            var room = Rooms[i];
            room.visited = saved.Rooms[i].visited || room == CurrentRoom;
            room.cleared = saved.Rooms[i].cleared;
            if (room.cleared) NetCoop.RoomChanged(i, 2);
        }
        if (saved.ExitOpen)
        {
            var boss = Rooms.Find(room => room.type == RoomType.Boss);
            Vector2Int exit = boss.Center + NextPortalOffset;
            SpawnPortal(exit);
            NetCoop.PortalSpawned(exit);
        }

        foreach (var entry in saved.Chests)
        {
            if (!floorChests.ContainsKey(entry.cell))
            {
                SpawnChest(entry.rarity, entry.cell);
                NetCoop.ChestSpawned((int)entry.rarity, entry.cell);
            }
            if (entry.opened && floorChests.TryGetValue(entry.cell, out var chest))
                NetCoop.RestoreOpenedChest(chest);
        }
        ui.RefreshMap();

        // Els accessoris registren la seua casella en Start.
        yield return null;
        foreach (var prop in FindObjectsByType<Breakable>(FindObjectsSortMode.None))
        {
            if (saved.BreakableHealth.TryGetValue(NetShared.BreakableKey(prop), out int health)) prop.health = health;
            else NetShared.RestoreBroken(prop);
        }

        foreach (var drop in saved.Drops)
        {
            if (drop.weapon != null)
                ItemPickup.Spawn(drop.weapon, drop.position, drop.position, cooldownRemaining: drop.cooldown);
            else if (drop.heal != null) HealPickup.Spawn(drop.heal, drop.position, drop.position);
            else CoinDrop.Spawn(drop.position, drop.position);
        }
    }
}
