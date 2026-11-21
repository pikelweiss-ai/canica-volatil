using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class NetBot
{
    static IEnumerator TestBossAttack(EnemyBoss boss, EnemyBoss.Attack attack)
    {
        var method = typeof(EnemyBoss).GetMethod("PerformAttack", BindingFlags.Instance | BindingFlags.NonPublic);
        return (IEnumerator)method.Invoke(boss, new object[] { attack });
    }

    IEnumerator Bosses150()
    {
        RunState.Reset(); RunState.Active = true; RunState.Seed = 150;
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => DungeonGenerator.Instance != null, 15, "masmorra per a provar bosses");
        if (failed) yield break;
        var player = DungeonGenerator.Instance.player;
        var hp = player.GetComponent<PlayerHealth>();
        keepAlive = true;
        StartCoroutine(KeepAlive(hp));
        Vector3 center = Phase0Origin();
        var specs = new (string id, int hp)[]
        {
            ("CorCristall", 120), ("CavallerCorrupte", 100), ("CucSorra", 110), ("Fenix", 105),
            ("HidraLlac", 100), ("ReiClavegueres", 95), ("CuinerBoig", 80), ("ProfessorInfernal", 85)
        };
        foreach (var (id, baseHp) in specs)
        {
            MovePhase0Player(center + Vector3.down * 2.5f);
            var go = SpawnPhase0Enemy(id, center + Vector3.up);
            var boss = go.GetComponent<EnemyBoss>();
            var health = go.GetComponent<EnemyHealth>();
            if (health.maxHealth != baseHp || !DungeonGenerator.Instance.themes.Any(theme =>
                theme.bossVariants != null && theme.bossVariants.Any(prefab => prefab.name == id)))
            { Fail("vida o tema incorrectes: " + id); yield break; }
            health.maxHealth += EnemyBoss.BonusHealth;
            health.currentHealth = health.maxHealth;
            yield return null;
            // La prova dispara cada atac per la mateixa ruta que el cicle del boss.
            boss.enabled = false;
            go.GetComponent<EnemyPathfindingAI>().enabled = false;
            var pieces = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Where(e => e.arenaPart).ToArray();
            int expected = boss.arenaKind == EnemyBoss.ArenaKind.Crystal ? 6 : boss.arenaKind == EnemyBoss.ArenaKind.Hydra ? 3 :
                boss.arenaKind == EnemyBoss.ArenaKind.Sewer ? 4 : 0;
            if (pieces.Length != expected)
            { Fail("nombre de peces incorrecte: " + id); yield break; }
            if (boss.arenaKind == EnemyBoss.ArenaKind.Crystal || boss.arenaKind == EnemyBoss.ArenaKind.Sewer)
            {
                health.TakeDamage(7);
                if (health.currentHealth != health.maxHealth) { Fail("guarda oberta abans d'hora: " + id); yield break; }
            }
            yield return Shot("boss-" + id);
            foreach (var attack in boss.attacks)
            {
                yield return TestBossAttack(boss, attack);
                if (boss == null) { Fail("boss destruit durant " + attack); yield break; }
                Ok(id + " completa " + attack);
            }

            if (boss.arenaKind == EnemyBoss.ArenaKind.Crystal || boss.arenaKind == EnemyBoss.ArenaKind.Sewer)
            {
                int count = boss.arenaKind == EnemyBoss.ArenaKind.Crystal ? 1 : 4;
                foreach (var piece in pieces.Take(count)) piece.TakeDamage(999);
                health.TakeDamage(7);
                if (health.currentHealth != health.maxHealth - 7) { Fail("el boss no queda vulnerable: " + id); yield break; }
            }
            if (boss.arenaKind == EnemyBoss.ArenaKind.Phoenix)
            {
                health.TakeDamage(health.maxHealth);
                if (health.currentHealth != health.maxHealth / 2 || health.IsDead)
                { Fail("el Fenix no reneix a mitja vida"); yield break; }
                health.TakeDamage(health.maxHealth);
                if (!health.IsDead) { Fail("el Fenix ha renascut dos vegades"); yield break; }
            }
            else
            {
                health.currentHealth = health.maxHealth / 2;
                boss.enabled = true;
                var ai = go.GetComponent<EnemyPathfindingAI>();
                ai.enabled = true;
                ai.Alert();
                float before = boss.timeBetweenAttacks;
                yield return WaitFor(() => boss.timeBetweenAttacks < before, 4, "fase 2 a mitja vida: " + id);
                if (failed) yield break;
            }

            Destroy(go);
            foreach (var bullet in FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None)) Destroy(bullet.gameObject);
            foreach (var hazard in FindObjectsByType<HazardZone>(FindObjectsSortMode.None)) Destroy(hazard.gameObject);
            yield return null;
            yield return null;
        }
        keepAlive = false;
        Pass();
    }

    IEnumerator HostBosses150()
    {
        Vector3 center = Phase0Origin();
        MovePhase0Player(center + Vector3.down * 3);
        var go = SpawnPhase0Enemy("CucSorra", center + Vector3.up * 2);
        var health = go.GetComponent<EnemyHealth>();
        var boss = go.GetComponent<EnemyBoss>();
        go.GetComponent<EnemyPathfindingAI>().Alert();
        bool seen = false;
        var host = NetSession.Instance;
        void MarkSeen(NetSession.ChatLine line) { if (!line.system && line.text == "boss 150 vist") seen = true; }
        host.ChatReceived += MarkSeen;
        yield return WaitFor(() => seen, 25, "el client confirma la barra i els avisos del boss");
        host.ChatReceived -= MarkSeen;
        if (failed) yield break;
        NetCoop.EnemyDied(health);
        Destroy(boss.gameObject);
    }

    IEnumerator JoinBosses150()
    {
        MovePhase0Player(Phase0Origin() + Vector3.down * 3);
        yield return WaitFor(() => NetCoop.Enemies.Any(h => h != null && h.IsNetProxy && h.name.StartsWith("CucSorra")) &&
            GameObject.Find("Barra Boss") != null, 12, "copia del boss amb barra de vida");
        if (failed) yield break;
        yield return WaitFor(() => GameObject.Find("Avis atac") != null, 15, "els avisos del boss arriben al client");
        if (failed) yield break;
        NetSession.Instance.SendChat("boss 150 vist");
    }
}

