using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemyBoss
{
    public enum ArenaKind { Classic, Crystal, Knight, Worm, Phoenix, Hydra, Sewer, Cook, Professor }
    public ArenaKind arenaKind;
    public GameObject arenaPartPrefab;
    public Sprite arenaProp;

    EnemyHealth[] arenaParts;
    Vector2 arenaCenter;
    float vulnerableUntil, reflectUntil, nextLava, nextWater;
    bool reborn;
    bool Stationary => arenaKind == ArenaKind.Crystal || arenaKind == ArenaKind.Hydra;

    void PrepareArena()
    {
        var dungeon = DungeonGenerator.Instance;
        var room = dungeon?.Rooms.Find(r => r.type == DungeonGenerator.RoomType.Boss && r.alive.Contains(health));
        arenaCenter = room == null ? (Vector2)transform.position : dungeon.ground.GetCellCenterWorld((Vector3Int)room.Center);
        if (Stationary)
        {
            ai.mode = EnemyPathfindingAI.Mode.Hold;
            ai.speedMultiplier = 0f;
        }
        int count = arenaKind == ArenaKind.Crystal ? 6 : arenaKind == ArenaKind.Sewer ? 4 : arenaKind == ArenaKind.Hydra ? 3 : 0;
        if (count > 0 && arenaPartPrefab != null)
        {
            arenaParts = new EnemyHealth[count];
            for (int i = 0; i < count; i++) SpawnArenaPart(i);
        }
        nextLava = Time.time + 20f;
    }

    Vector2 PartPosition(int index)
    {
        if (arenaKind == ArenaKind.Sewer)
            return arenaCenter + new Vector2(index % 2 == 0 ? -5 : 5, index < 2 ? -3 : 3);
        if (arenaKind == ArenaKind.Hydra)
            return (Vector2)transform.position + new Vector2((index - 1) * 1.8f, 1.25f);
        return (Vector2)transform.position + Dir(index * 60f + Time.time * 35f) * 2.2f;
    }

    void SpawnArenaPart(int index)
    {
        Vector2 position = PartPosition(index);
        if (arenaKind == ArenaKind.Sewer && TileNav.Ready())
            position = TileNav.Center(TileNav.NearestFree(TileNav.Cell(position)));
        var part = Instantiate(arenaPartPrefab, position, Quaternion.identity).GetComponent<EnemyHealth>();
        arenaParts[index] = part;
        if (arenaKind == ArenaKind.Hydra) return;
        part.Died += _ =>
        {
            if (arenaKind == ArenaKind.Crystal || LivingParts() == 0)
                vulnerableUntil = Time.time + (arenaKind == ArenaKind.Crystal ? 8f : 6f);
        };
    }

    int LivingParts()
    {
        int count = 0;
        if (arenaParts != null)
            foreach (var part in arenaParts)
                if (part != null && !part.IsDead) count++;
        return count;
    }

    void TickArena()
    {
        if (arenaParts != null)
        {
            if (vulnerableUntil > 0f && Time.time >= vulnerableUntil)
            {
                vulnerableUntil = 0f;
                for (int i = 0; i < arenaParts.Length; i++)
                    if (arenaParts[i] == null || arenaParts[i].IsDead) SpawnArenaPart(i);
            }
            if (arenaKind == ArenaKind.Crystal)
                for (int i = 0; i < arenaParts.Length; i++)
                    if (arenaParts[i] != null) arenaParts[i].transform.position = PartPosition(i);
            if (arenaKind == ArenaKind.Sewer && vulnerableUntil <= 0f && Time.time >= nextWater)
            {
                nextWater = Time.time + 1f;
                HazardZone.Spawn(HazardZone.Style.DirtyWater, arenaCenter);
            }
        }
        if (arenaKind == ArenaKind.Phoenix && Time.time >= nextLava)
        {
            nextLava = Time.time + 20f;
            for (int i = 0; i < 8; i++)
                HazardZone.Spawn(HazardZone.Style.RisingLava, arenaCenter + Dir(i * 45f) * 3.8f);
        }
    }

    internal bool GuardDamage(int damage)
    {
        if (arenaParts != null && (arenaKind == ArenaKind.Crystal || arenaKind == ArenaKind.Sewer) &&
            vulnerableUntil <= Time.time)
            return true;
        if (arenaKind != ArenaKind.Phoenix || reborn || health.currentHealth > damage) return false;
        reborn = true;
        health.currentHealth = Mathf.Max(1, health.maxHealth / 2);
        HazardZone.Spawn(HazardZone.Style.RisingLava, transform.position);
        MobSounds.Play("boss_rugit", transform.position, .55f);
        return true;
    }

    internal bool ReflectProjectile(Vector2 direction)
    {
        if (arenaKind != ArenaKind.Knight || Time.time >= reflectUntil || direction.sqrMagnitude < .01f) return false;
        Fire(-direction.normalized);
        return true;
    }

    void ClearArenaParts()
    {
        if (arenaParts == null) return;
        foreach (var part in arenaParts)
        {
            if (part == null || part.IsDead) continue;
            NetCoop.EnemyDied(part);
            Destroy(part.gameObject);
        }
        arenaParts = null;
    }

    Vector2 AimDirection()
    {
        var target = NetCoop.NearestTarget(transform.position);
        return target == null ? Vector2.down : ((Vector2)target.position - (Vector2)transform.position).normalized;
    }

    static IEnumerable<Transform> ArenaPlayers()
    {
        var local = GameObject.FindWithTag("Player");
        if (local != null && local.TryGetComponent<PlayerHealth>(out var health) && health.currentHealth > 0)
            yield return local.transform;
        foreach (var avatar in NetShared.Avatars())
            if (avatar != null && !avatar.Down) yield return avatar.transform;
    }

    static float DistanceToLine(Vector2 position, Vector2 start, Vector2 end)
    {
        Vector2 line = end - start;
        float t = line.sqrMagnitude < .001f ? 0f : Mathf.Clamp01(Vector2.Dot(position - start, line) / line.sqrMagnitude);
        return Vector2.Distance(position, start + t * line);
    }

    IEnumerator Burrow()
    {
        float row = Mathf.Clamp(ai.Player.position.y - arenaCenter.y, -2.5f, 2.5f);
        Vector2 center = arenaCenter + Vector2.up * row;
        Vector2 side = transform.position.x < center.x ? Vector2.right : Vector2.left;
        Vector2 start = center - side * 6f, end = center + side * 6f;
        var markers = new List<SpriteRenderer>();
        for (int i = 0; i < 9; i++)
            markers.Add(MarkAttack("Rastre del cuc", Vector2.Lerp(start, end, i / 8f), 1.5f, 1f));
        yield return Telegraph(.65f);
        var hit = new HashSet<Transform>();
        for (float t = 0; t < .38f; t += Time.deltaTime)
        {
            transform.position = Vector2.Lerp(start, end, t / .38f);
            foreach (var player in ArenaPlayers())
                if (!hit.Contains(player) && Vector2.Distance(player.position, transform.position) < 1.15f)
                { hit.Add(player); NetCoop.DamagePlayer(player, 2); }
            yield return null;
        }
        transform.position = end;
        if (TileNav.Ready() && !TileNav.CircleFree(end, ai.bodyRadius))
            transform.position = TileNav.Center(TileNav.NearestFree(TileNav.Cell(end)));
        var body = GetComponent<Rigidbody2D>();
        if (body != null) body.position = transform.position;
        foreach (var marker in markers) RemoveMark(marker);
    }

    IEnumerator Sweep()
    {
        if (arenaKind == ArenaKind.Cook)
        {
            Vector2 start = transform.position;
            Vector2 direction = AimDirection();
            Vector2 end = start + direction * 4.5f;
            var mark = MarkAttack("Paella", end, 1.4f, .65f);
            yield return Telegraph(.6f);
            RemoveMark(mark);
            EnemyBullet.Fire(start + direction * .7f, direction, 8f, 1, transform, 1.25f, flightTime: .5f, appearance: EnemyBullet.Appearance.Pan);
            yield return new WaitForSeconds(.5f);
            EnemyBullet.Fire(end, -direction, 8f, 1, transform, 1.25f, flightTime: .55f, appearance: EnemyBullet.Appearance.Pan);
            yield break;
        }

        if (arenaKind == ArenaKind.Knight)
        {
            reflectUntil = Time.time + 2f;
            yield return Telegraph(2f);
        }
        int swings = arenaKind == ArenaKind.Knight ? 3 : arenaKind == ArenaKind.Hydra ? Mathf.Max(1, LivingParts()) : 1;
        for (int i = 0; i < swings; i++)
        {
            Vector2 origin = transform.position;
            Vector2 direction = AimDirection();
            float reach = arenaKind == ArenaKind.Worm ? 6f : 3f;
            var mark = MarkAttack("Avis de colp", origin + direction * (reach * .5f), reach, .48f);
            float delay = arenaKind == ArenaKind.Hydra ? .22f + .09f * LivingParts() : .45f;
            yield return Telegraph(delay);
            foreach (var player in ArenaPlayers())
            {
                Vector2 offset = (Vector2)player.position - origin;
                if (offset.magnitude <= reach && Vector2.Dot(offset.normalized, direction) > 0f)
                    NetCoop.DamagePlayer(player, arenaKind == ArenaKind.Hydra ? 2 : 1);
            }
            RemoveMark(mark);
            yield return new WaitForSeconds(.15f);
        }
    }

    IEnumerator Beam()
    {
        Vector2 origin = transform.position;
        var hit = new HashSet<Transform>();
        float angle = Time.time * 35f;
        int steps = enraged ? 9 : 6;
        for (int step = 0; step < steps; step++)
        {
            var markers = new List<SpriteRenderer>();
            var rays = new List<(Vector2 start, Vector2 end)>();
            for (int i = 0; i < arenaParts.Length; i++)
            {
                var part = arenaParts[i];
                if (part == null || part.IsDead) continue;
                Vector2 start = part.transform.position;
                Vector2 end = start;
                Vector2 ray = Dir(angle + step * 12f + i * 60f);
                for (int distance = 1; distance <= 5; distance++)
                {
                    Vector2 at = start + ray * distance;
                    if (!TileNav.LineOfSight(start, at, ignoreWater: true)) break;
                    end = at;
                    markers.Add(MarkAttack("Cristall", at, .75f, .26f));
                }
                rays.Add((start, end));
            }
            yield return new WaitForSeconds(.22f);
            foreach (var player in ArenaPlayers())
                foreach (var ray in rays)
                    if (!hit.Contains(player) && DistanceToLine(player.position, ray.start, ray.end) < .45f)
                    { hit.Add(player); NetCoop.DamagePlayer(player, 2); }
            foreach (var marker in markers) RemoveMark(marker);
        }
        Fx.Burst(origin, dustColor, 5, 2f);
    }

    IEnumerator Shockwave()
    {
        Vector2 center = arenaCenter;
        if (arenaKind == ArenaKind.Sewer)
        {
            int rows = enraged ? 5 : 3;
            for (int row = 0; row < rows; row++)
            {
                Vector2 start = center + new Vector2(row % 2 == 0 ? -6f : 6f, (row - (rows - 1) * .5f) * 1.65f);
                Vector2 dir = row % 2 == 0 ? Vector2.right : Vector2.left;
                var pointers = new List<SpriteRenderer>();
                for (int j = 1; j <= 6; j++) pointers.Add(MarkAttack("Rastre de rates", start + dir * j * 1.65f, .9f, .6f));
                yield return Telegraph(.55f);
                foreach (var pointer in pointers) RemoveMark(pointer);
                for (int rat = 0; rat < 3; rat++)
                {
                    EnemyBullet.Fire(start, dir, 7f, 1, transform, 1.4f, overHazardousLiquid: true, flightTime: 1.8f, appearance: EnemyBullet.Appearance.Rat);
                    yield return new WaitForSeconds(.17f);
                }
            }
            yield break;
        }

        yield return Telegraph(.55f);
        for (int burst = 0; burst < (enraged ? 3 : 2); burst++)
        {
            for (int i = 0; i < 20; i++) Fire(Dir(i * 18f + burst * 8f), 1.1f);
            yield return new WaitForSeconds(.28f);
        }
    }

    IEnumerator Bomb()
    {
        var target = NetCoop.NearestTarget(transform.position);
        if (target == null) yield break;
        Vector2 at = target.position;
        HazardZone.Spawn(HazardZone.Style.BossBomb, at);
        yield return Telegraph(.75f);
    }

    IEnumerator ArenaHazard()
    {
        if (arenaKind == ArenaKind.Professor)
        {
            Fx.Text(arenaCenter + Vector2.up * 4f, "Examen sorpresa!", UIKit.Gold);
            Vector2[] desks = { arenaCenter + new Vector2(-4, -2), arenaCenter + new Vector2(4, -2),
                               arenaCenter + new Vector2(-4, 2), arenaCenter + new Vector2(4, 2) };
            int safe = Random.Range(0, desks.Length);
            var marks = new List<SpriteRenderer>();
            for (int i = 0; i < desks.Length; i++)
                if (i != safe) marks.Add(MarkAttack("Pupitre de l'examen", desks[i], 2.4f, 2.6f));
            yield return Telegraph(2.5f);
            foreach (var player in ArenaPlayers())
                if (Vector2.Distance(player.position, desks[safe]) > 1.3f) NetCoop.DamagePlayer(player, 2);
            foreach (var mark in marks) RemoveMark(mark);
            yield break;
        }

        int count = arenaKind == ArenaKind.Cook ? 4 : (enraged ? 6 : 4);
        Vector2 target = NetCoop.NearestTarget(transform.position)?.position ?? transform.position;
        for (int i = 0; i < count; i++)
        {
            Vector2 at = arenaKind == ArenaKind.Cook
                ? arenaCenter + new Vector2(-4.5f + i * 3f, 2f)
                : target + Dir(i * (360f / count)) * 2.4f;
            if (TileNav.Ready() && !TileNav.CircleFree(at, .15f)) continue;
            var mark = MarkAttack("Avis de foc", at, 3f, .48f);
            yield return new WaitForSeconds(.45f);
            RemoveMark(mark);
            HazardZone.Spawn(arenaKind == ArenaKind.Cook ? HazardZone.Style.Stove : HazardZone.Style.Fire, at);
        }
    }
}

