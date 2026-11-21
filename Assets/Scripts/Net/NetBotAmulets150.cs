using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class NetBot
{
    IEnumerator Amulets150()
    {
        RunState.Reset(); RunState.Active = true; RunState.Seed = 153; RunState.Floor = 5;
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => DungeonGenerator.Instance != null, 12, "masmorra i amulets");
        if (failed) yield break;
        yield return null;
        var dungeon = DungeonGenerator.Instance;
        var player = dungeon.player;
        var hp = player.GetComponent<PlayerHealth>();
        int baseHp = hp.maxHealth;
        Vector3 center = Phase0Origin();
        MovePhase0Player(center + Vector3.down);
        var kit = RunAmulets.Local;
        void Clear()
        {
            for (int i = 0; i < RunAmulets.Slots; i++)
                RunAmulets.Apply(0, RunAmulets.Action.Discard, i, 0);
        }

        bool Equip(Amulet id)
        {
            Clear();
            RunAmulets.Buy(0, id);
            if (kit.Has(id) && RunAmulets.Info(id).Icon != null) return true;
            Fail("no es pot equipar " + id + " o falta la icona");
            return false;
        }

        bool Check(bool condition, string label)
        {
            if (!condition) { Fail(label); return false; }
            Ok(label);
            return true;
        }

        RunAmulets.OfferFloor();
        if (!Check(kit.Offer.Length == 3 && kit.Offer.Distinct().Count() == 3, "tria de tres amulets diferents")) yield break;
        yield return Shot("fase3-tria");
        var chosen = kit.Offer[1];
        RunAmulets.Request(RunAmulets.Action.Choose, 1, 0);
        if (!Check(kit.Has(chosen) && !RunAmulets.CanBuy(0, chosen), "tria i rebuig de duplicats")) yield break;
        var dummy = SpawnPhase0Enemy("Malo", center + Vector3.right * 3).GetComponent<EnemyHealth>();
        dummy.maxHealth = dummy.currentHealth = 250;
        dummy.GetComponent<EnemyPathfindingAI>().enabled = false;
        yield return null;
        void Hit(int damage)
        {
            RunStats.HitBy = "bot";
            dummy.TakeProjectileHit(damage, Vector2.zero, ProjectileEffect.None);
            RunStats.HitBy = null;
        }

        if (!Equip(Amulet.GlassShield)) yield break;
        hp.TakeDamage(1);
        if (!Check(hp.currentHealth == baseHp && kit.GlassReady - Time.time > 14f, "Escut de vidre: colp parat i 15 s")) yield break;
        if (!Equip(Amulet.Whetstone)) yield break;
        int before = dummy.currentHealth;
        Hit(2); Hit(2); Hit(2);
        if (!Check(dummy.currentHealth == before - 7, "Pedra afiladora: +1 al tercer impacte")) yield break;
        if (!Equip(Amulet.Hourglass) ||
            !Check(Mathf.Approximately(RunAmulets.Cooldown(1f), .88f) && Mathf.Approximately(RunAmulets.Cooldown(.081f), .08f), "Rellotge: −12 % amb mínim")) yield break;
        if (!Equip(Amulet.Horseshoe) || !Check(Mathf.Approximately(RunAmulets.MoveMultiplier, 1.1f), "Ferradura: +10 %")) yield break;
        if (!Equip(Amulet.Magnet) || !Check(RunAmulets.MagnetRange(2.2f) == 4f, "Imant: quatre caselles")) yield break;
        if (!Equip(Amulet.StoneHeart) || !Check(hp.maxHealth == baseHp + 1, "Cor de pedra: un cor més")) yield break;
        if (!Equip(Amulet.HawkEye) ||
            !Check(Mathf.Approximately(RunAmulets.RangeMultiplier, 1.2f) && Mathf.Approximately(RunAmulets.ViewMultiplier, 1.15f), "Ull: +20 % d’abast i +15 % de visió")) yield break;

        if (!Equip(Amulet.PoisonDrop)) yield break;
        // Busca una tirada guanyadora i rejuga-la amb l'impacte real.
        for (int seed = 1; seed < 100; seed++)
        {
            Random.InitState(seed);
            if (Random.value >= .25f) continue;
            Random.InitState(seed);
            Hit(2);
            break;
        }
        before = dummy.currentHealth;
        yield return new WaitForSeconds(3.2f);
        if (!Check(dummy.currentHealth == before - 3, "Gota de verí: tres aplicacions de mal")) yield break;
        if (!Equip(Amulet.VampireTooth)) yield break;
        hp.currentHealth = baseHp - 2;
        for (int i = 0; i < 12; i++) RunAmulets.Killed(0);
        if (!Check(hp.currentHealth == baseHp - 1, "Dent: cura un cor cada dotze baixes")) yield break;
        if (!Equip(Amulet.LuckyCoin)) yield break;
        int coins = RunAmulets.CoinValue() + RunAmulets.CoinValue() + RunAmulets.CoinValue() + RunAmulets.CoinValue();
        if (!Check(coins == 5 && RunAmulets.ChestQuality(ChestRarity.Comu, 0, .149f) == ChestRarity.Rar &&
            RunAmulets.ChestQuality(ChestRarity.Comu, 0, .15f) == ChestRarity.Comu, "Sort: +25 % monedes i llindar del 15 % en cofres")) yield break;

        if (!Equip(Amulet.Thorns)) yield break;
        before = dummy.currentHealth;
        hp.TakeDamage(1, dummy);
        if (!Check(dummy.currentHealth == before - 1, "Espines: torna un de mal de contacte")) yield break;
        yield return new WaitForSecondsRealtime(hp.invulnerableTime + .05f);
        if (!Equip(Amulet.PhoenixFeather)) yield break;
        hp.TakeDamage(99);
        if (!Check(kit.PhoenixUsed && hp.currentHealth == Mathf.CeilToInt(baseHp * .5f) &&
            !RunAmulets.Apply(0, RunAmulets.Action.Phoenix, 0, 0), "Ploma: una sola resurrecció a mitja vida")) yield break;

        if (!Equip(Amulet.DragonHeart)) yield break;
        hp.currentHealth = 1;
        before = dummy.currentHealth;
        Hit(10);
        if (!Check(dummy.currentHealth == before - 13, "Cor de dragó: +30 % per davall del llindar de vida")) yield break;

        if (!Equip(Amulet.ShatteredMirror)) yield break;
        var gun = NetCoop.Weapon("Pistola");
        var bullet = Projectile.Spawn(player.position, Vector2.right, gun, player, 0, 0);
        int bounces = (int)typeof(Projectile).GetField("wallBouncesLeft", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bullet);
        Destroy(bullet.gameObject);
        if (!Check(bounces == gun.wallBounces + 1, "Mirall: un rebot afegit")) yield break;

        if (!Equip(Amulet.ForbiddenBook)) yield break;
        before = dummy.currentHealth;
        Hit(10);
        if (!Check(dummy.currentHealth == before - 15 && hp.maxHealth == baseHp - 1, "Llibre: +50 % de mal i −1 cor")) yield break;

        if (!Equip(Amulet.FallenStar)) yield break;
        kit.StarReady = Time.time;
        before = dummy.currentHealth;
        RunAmulets.Tick();
        if (!Check(dummy.currentHealth == before - 6 && kit.StarReady - Time.time > 9f, "Estrella: sis de mal i deu segons")) yield break;

        RunAmulets.Buy(0, Amulet.GlassShield);
        RunAmulets.Buy(0, Amulet.StoneHeart);
        if (!Check(!RunAmulets.CanBuy(0, Amulet.Horseshoe), "màxim de tres amulets")) yield break;
        yield return Shot("fase3-amulets-hotbar");
        Pass();
    }

    IEnumerator HostAmulets150()
    {
        var peer = NetSession.Instance.Players.Values.First(player => !player.isHost);
        RunAmulets.OfferFloor();
        RunAmulets.Request(RunAmulets.Action.Choose, 0, 0);
        yield return WaitFor(() => RunAmulets.Get(peer.id).Equipped[1] != Amulet.None, 15,
            "l’amfitrió valida la tria del client");
        if (failed) yield break;
        if (RunAmulets.Local.Equipped[0] == Amulet.None || RunAmulets.Local.Equipped[1] != Amulet.None)
        { Fail("les ranures de l'amfitrió s’han barrejat amb les del client"); yield break; }
        yield return WaitFor(() => RunAmulets.Get(peer.id).Equipped[1] == Amulet.None, 15,
            "l’amfitrió valida el descart del client");
    }

    IEnumerator JoinAmulets150()
    {
        yield return WaitFor(() => RunAmulets.Local.Offer.Length == 3, 12, "el client rep els tres amulets");
        if (failed) yield break;
        RunAmulets.Request(RunAmulets.Action.Choose, 0, 1);
        yield return WaitFor(() => RunAmulets.Local.Equipped[1] != Amulet.None &&
            RunAmulets.Party.Any(entry => entry.Key != RunAmulets.LocalId && entry.Value.Equipped[0] != Amulet.None),
            12, "inventaris sincronitzats i independents");
        if (failed) yield break;
        RunAmulets.Request(RunAmulets.Action.Discard, 1);
        yield return WaitFor(() => RunAmulets.Local.Equipped[1] == Amulet.None, 12, "el client rep el descart validat");
    }
}

