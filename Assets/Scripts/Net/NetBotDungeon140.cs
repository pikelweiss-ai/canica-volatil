using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class NetBot
{
    IEnumerator Dungeon140()
    {
        RunState.Reset(); RunState.Active = true; RunState.Seed = 140; RunState.Floor = 3;
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => Scene == "Dungeon" && DungeonGenerator.Instance != null, 15, "pis v1.4 carregat");
        if (failed) yield break;
        yield return null;
        var g = DungeonGenerator.Instance;
        var player = g.player; var body = player.GetComponent<Rigidbody2D>();
        var health = player.GetComponent<PlayerHealth>();
        var seen = new HashSet<DungeonGenerator.RoomShape>();
        foreach (var r in g.Rooms)
        {
            player.position = g.ground.GetCellCenterWorld(new Vector3Int(r.Center.x, r.Center.y, 0));
            body.position = player.position; health.currentHealth = health.maxHealth;
            yield return null;
            if (seen.Add(r.shape)) { yield return new WaitForSeconds(.2f); yield return Shot("forma-" + r.shape); }
            if (r.type == DungeonGenerator.RoomType.Secreta)
            {
                foreach (var b in FindObjectsByType<Breakable>(FindObjectsSortMode.None))
                {
                    if (!b.meleeOrMineOnly) continue;
                    int before = b.health; b.Hit(100);
                    if (b.health != before) { Fail("una bala trenca la paret secreta"); yield break; }
                    b.Hit(1, true);
                    if (b.health > 0) { Fail("cos a cos no obri la paret secreta"); yield break; }
                }
                Ok("paret secreta rebutja bales i accepta cos a cos");
            }
            if (r.type == DungeonGenerator.RoomType.Emboscada)
            {
                if (r.active) { Fail("emboscada abans d'obrir cofre"); yield break; }
                Chest chest = null;
                foreach (var c in FindObjectsByType<Chest>(FindObjectsSortMode.None))
                    if (r.rect.Contains((Vector2Int)g.ground.WorldToCell(c.transform.position))) { chest = c; break; }
                if (chest == null) { Fail("falta cofre d'emboscada"); yield break; }
                chest.OpenAsAuthority(player.position);
                if (!r.ambushTriggered || !r.active) { Fail("el cofre no activa l'emboscada"); yield break; }
            }
            if (r.type == DungeonGenerator.RoomType.Start || r.type == DungeonGenerator.RoomType.Treasure || r.type == DungeonGenerator.RoomType.Secreta) continue;
            float until = Time.time + 25f; int waves = 0; bool hadEnemies = false;
            while (!r.cleared && Time.time < until)
            {
                health.currentHealth = health.maxHealth;
                bool enemies = r.alive.Exists(e => e != null && e.enabled && !e.IsDead);
                if (enemies && !hadEnemies) waves++;
                hadEnemies = enemies;
                foreach (var e in r.alive.ToArray()) if (e != null && e.enabled && !e.IsDead) e.TakeDamage(99999);
                yield return new WaitForSeconds(.1f);
            }
            if (!r.cleared) { Fail("sala no acabada: " + r.type); yield break; }
            if ((r.type == DungeonGenerator.RoomType.Emboscada && waves != 2) || (r.type == DungeonGenerator.RoomType.Arena && waves != 3))
            { Fail("nombre d'onades incorrecte: " + r.type + "=" + waves); yield break; }
            foreach (var cell in r.doorCells) if (g.doors.HasTile(cell)) { Fail("porta tancada despres del combat"); yield break; }
            Ok("sala acabada: " + r.type + ", onades=" + waves);
        }
        if (FindFloorPortal(1) == null) { Fail("falta portal final"); yield break; }
        Ok("planta individual v1.4 completada");
        if (shots != null)
        {
            seen.Clear();
            for (int seed = 1; seed <= 50 && seen.Count < 7; seed++)
            {
                RunState.Reset(); RunState.Active = true; RunState.Seed = seed; RunState.Floor = 3;
                var old = DungeonGenerator.Instance; SceneManager.LoadScene("Dungeon");
                yield return WaitFor(() => DungeonGenerator.Instance != null && DungeonGenerator.Instance != old, 12, "galeria de formes");
                if (failed) yield break;
                g = DungeonGenerator.Instance; g.enabled = false;
                yield return null;
                var camera = Camera.main; var follow = camera.GetComponent<CameraFollow>(); if (follow != null) follow.enabled = false;
                camera.orthographicSize = 12f;
                foreach (var r in g.Rooms)
                {
                    if (!seen.Add(r.shape)) continue;
                    var center = g.ground.GetCellCenterWorld(new Vector3Int(r.Center.x, r.Center.y, 0));
                    g.player.position = center; g.player.GetComponent<Rigidbody2D>().position = center;
                    camera.transform.position = new Vector3(center.x, center.y, -10f);
                    yield return Shot("forma-" + r.shape);
                }
            }
            if (seen.Count != 7) { Fail("falten captures de formes"); yield break; }
            Ok("captures de les set formes");
        }
        Pass();
    }
}
