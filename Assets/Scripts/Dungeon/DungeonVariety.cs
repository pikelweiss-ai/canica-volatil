using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public partial class DungeonGenerator
{
    // Les formes retallen nomes els quadrants: les tres cel·les de cada eix sempre connecten les portes.
    public static bool InsideShape(Room room, Vector2Int cell)
    {
        if (!room.rect.Contains(cell)) return false;
        int x = cell.x - room.Center.x, y = cell.y - room.Center.y;
        int ax = Mathf.Abs(x), ay = Mathf.Abs(y);
        if (ax <= 1 || ay <= 1) return true;
        float nx = ax / (room.rect.width * .5f), ny = ay / (room.rect.height * .5f);
        switch (room.shape)
        {
            case RoomShape.L: return x <= 1 || y <= 1;
            case RoomShape.T: return y >= -1;
            case RoomShape.Cross: return ax <= room.rect.width / 4 || ay <= room.rect.height / 4;
            case RoomShape.Circle: return nx * nx + ny * ny <= 1f;
            case RoomShape.Octagon: return nx + ny <= 1.5f;
            case RoomShape.Pillars: return !(ax == room.rect.width / 4 && ay == room.rect.height / 4);
            default: return true;
        }
    }

    static bool CombatRoom(Room r) => r.type == RoomType.Combat || r.type == RoomType.Boss ||
        r.type == RoomType.Arena || (r.type == RoomType.Emboscada && r.ambushTriggered);

    public bool OpenAmbush(Chest chest)
    {
        Room r = RoomAt(ground.WorldToCell(chest.transform.position), 0);
        if (r == null || r.type != RoomType.Emboscada || r.cleared) return false;
        if (!NetCoop.IsClient && !r.ambushTriggered)
        {
            r.ambushTriggered = true;
            NetCoop.RoomChanged(Rooms.IndexOf(r), 7);
            ui.ShowBanner("EMBOSCADA!", 2f);
            MobSounds.Play("sala_emboscada", chest.transform.position, .6f);
            // En cooperatiu es conserva l'entrada oberta fins que entren tots.
            if (!NetCoop.Online) TryStartRoom(ToCell(r.Center));
        }
        return true; // El boti arriba despres de les dos onades.
    }

    void CreateSecretWalls()
    {
        foreach (Room r in Rooms)
        {
            if (r.type != RoomType.Secreta) continue;
            foreach (Vector3Int cell in r.doorCells)
            {
                var go = new GameObject("Paret esquerdada");
                go.transform.SetParent(transform);
                go.transform.position = ground.GetCellCenterWorld(cell);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = walls.GetSprite(cell + Vector3Int.right) ?? walls.GetSprite(cell + Vector3Int.up);
                if (sr.sprite == null && Theme != null && Theme.wallTile is UnityEngine.Tilemaps.Tile tile) sr.sprite = tile.sprite;
                sr.color = new Color(.8f, .75f, .65f);
                var wr = walls.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
                sr.sortingLayerID = wr.sortingLayerID; sr.sortingOrder = wr.sortingOrder + 1;
                go.AddComponent<BoxCollider2D>();
                var b = go.AddComponent<Breakable>(); b.health = 1; b.coinChance = b.healChance = 0; b.meleeOrMineOnly = true;
                var crack = new GameObject("Esquerda", typeof(TextMeshPro)); crack.transform.SetParent(go.transform, false);
                var text = crack.GetComponent<TextMeshPro>(); text.text = "ϟ"; text.fontSize = 5; text.alignment = TextAlignmentOptions.Center;
                text.color = new Color(.15f, .12f, .1f); text.rectTransform.sizeDelta = Vector2.one;
                text.GetComponent<MeshRenderer>().sortingLayerID = sr.sortingLayerID;
                text.GetComponent<MeshRenderer>().sortingOrder = sr.sortingOrder + 2;
            }
            Room neighbour = r.links[0];
            Vector2Int direction = r.slot - neighbour.slot;
            Vector2Int hint = neighbour.Center + direction * 2;
            var sign = new GameObject("Pista sala secreta", typeof(TextMeshPro)); sign.transform.SetParent(transform);
            sign.transform.position = ground.GetCellCenterWorld(ToCell(hint));
            var label = sign.GetComponent<TextMeshPro>(); label.text = "Una paret sona buida..."; label.fontSize = 2;
            label.alignment = TextAlignmentOptions.Center; label.rectTransform.sizeDelta = new Vector2(6, 1);
            label.GetComponent<MeshRenderer>().sortingLayerName = "Player";
        }
    }

    public bool IsIceAt(Vector3 position)
    {
        Room r = RoomAt(ground.WorldToCell(position), 0);
        return r != null && r.modifier == RoomModifier.Ice;
    }

    void DecorateLargeCombat(Room room)
    {
        bool corners = rng.Next(2) == 0;
        foreach (int x in new[] { -1, 1 })
            foreach (int y in new[] { -1, 1 })
            {
                var sign = new Vector2Int(x, y);
                var corner = room.Center + new Vector2Int(x * (room.rect.width / 4), y * (room.rect.height / 4));
                for (int length = 0; length < 3; length++)
                {
                    Vector2Int cell = corner - new Vector2Int(length * sign.x, 0);
                    if (FreeForDecor(room, cell)) solid.Add(cell);
                    if (corners)
                    {
                        cell = corner - new Vector2Int(0, length * sign.y);
                        if (FreeForDecor(room, cell)) solid.Add(cell);
                    }
                }
            }

        PlaceProps(room, cratePrefab, rng.Next(6, 9), cratesToSpawn);
        PlaceProps(room, barrelPrefab, rng.Next(1, 3), barrelsToSpawn);
    }

    const int ArenaWaves = 3;

    IEnumerator RunArenaWaves(Room room)
    {
        for (int wave = 1; wave <= ArenaWaves; wave++)
        {
            if (wave > 1)
            {
                yield return new WaitForSeconds(1.5f);
                room.arenaWave = wave;
                NetCoop.RoomChanged(Rooms.IndexOf(room), 1, wave);
            }

            var cells = new List<Vector2Int>();
            foreach (Vector2Int cell in room.rect.allPositionsWithin)
                if (InsideShape(room, cell) && !reserved.Contains(cell) && !TileNav.IsBlocked(ToCell(cell)))
                    cells.Add(cell);

            var players = new List<Vector2>(RemotePlayerPositions()) { (Vector2)player.position };
            int count = 1 + RunState.Floor + wave;
            for (int i = 0; i < count && cells.Count > 0; i++)
            {
                Vector2Int direction = wave == 1 ? Dirs[i % 2] : wave == 2 ? Dirs[2 + i % 2] : Dirs[i % 4];
                var flank = cells.FindAll(cell =>
                {
                    Vector2Int delta = cell - room.Center;
                    if (delta.x * direction.x + delta.y * direction.y < 3) return false;
                    Vector2 position = ground.GetCellCenterWorld(ToCell(cell));
                    return players.TrueForAll(at => (at - position).sqrMagnitude >= 3.5f * 3.5f);
                });
                var positions = flank.Count > 0 ? flank : cells;
                Vector2Int spawn = positions[rng.Next(positions.Count)];
                cells.Remove(spawn);
                StartCoroutine(SpawnEnemy(PickEnemy(), spawn, room, 0f));
            }

            yield return WaitClear(room);
        }
    }

    Light2D[] roomLights;
    TextMeshProUGUI arenaProgress;
    void UpdateRoomEffects()
    {
        if (roomLights == null) roomLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        var r = player != null ? RoomAt(ground.WorldToCell(player.position), 0) : null;
        foreach (var light in roomLights)
            if (light != null && light.lightType == Light2D.LightType.Global)
                light.intensity = (Theme != null ? Theme.lightIntensity : 1f) * (r != null && r.modifier == RoomModifier.Darkness ? .4f : 1f);
        bool inArena = r != null && r.type == RoomType.Arena && r.active && !r.cleared;
        if (arenaProgress == null && inArena && ui != null)
        {
            var go = new GameObject("Progrés de l'arena", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(ui.transform, false);
            arenaProgress = go.GetComponent<TextMeshProUGUI>();
            arenaProgress.fontSize = 24;
            arenaProgress.alignment = TextAlignmentOptions.Center;
            arenaProgress.raycastTarget = false;
            var rect = arenaProgress.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -85);
            rect.sizeDelta = new Vector2(600, 45);
        }
        if (arenaProgress != null)
            arenaProgress.text = inArena ? $"ARENA · ONADA {r.arenaWave}/{ArenaWaves}" : "";
    }

    IEnumerator RoomFlames(Room r)
    {
        int next = 0;
        while (r.active && !r.cleared && !gameOver)
        {
            yield return new WaitForSeconds(4.8f);
            if (!r.active || r.cleared || gameOver) yield break;
            var cell = r.Center + Dirs[next++ % 4] * 3;
            Vector2 pos = ground.GetCellCenterWorld(ToCell(cell));
            NetShared.Telegraph(pos, 2f, 1.2f);
            yield return NetShared.ShowTelegraph(pos, 2f, 1.2f);
            if (!r.active || r.cleared || gameOver) yield break;
            Fx.ExplodeVisual(pos, 1f); NetCoop.Exploded(pos, 1f, 1);
            if (player != null && player.gameObject.activeInHierarchy && Vector2.Distance(player.position, pos) <= 1.3f)
                NetCoop.DamagePlayer(player, 1);
        }
    }
}
