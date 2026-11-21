using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum Amulet : byte
{
    None, GlassShield, Whetstone, Hourglass, Horseshoe, Magnet, StoneHeart,
    HawkEye, PoisonDrop, VampireTooth, LuckyCoin, Thorns, PhoenixFeather,
    DragonHeart, ShatteredMirror, ForbiddenBook, FallenStar
}

public sealed class AmuletInfo
{
    public readonly Amulet id;
    public readonly string name, description, art;
    public readonly ChestRarity rarity;
    public Sprite Icon => Resources.Load<Sprite>("Pixel/Amulets/" + art);

    public AmuletInfo(Amulet id, string name, string description, string art, ChestRarity rarity)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.art = art;
        this.rarity = rarity;
    }
}

public static class RunAmulets
{
    public const int Slots = 3;
    public enum Action : byte { Choose, Discard, Swap, Glass, Phoenix }
    const ChestRarity Common = ChestRarity.Comu, Rare = ChestRarity.Rar, Epic = ChestRarity.Epic, Legendary = ChestRarity.Llegendari;

    public static readonly AmuletInfo[] Catalog =
    {
        new(Amulet.GlassShield, "Escut de vidre", "Para un colp cada 15 s.", "Escut_de_vidre", Common),
        new(Amulet.Whetstone, "Pedra afiladora", "+1 de mal cada tercer impacte.", "Pedra_afiladora", Common),
        new(Amulet.Hourglass, "Rellotge de sorra", "−12 % de temps entre atacs, amb un mínim de 0,08 s.", "Rellotge_de_sorra", Common),
        new(Amulet.Horseshoe, "Ferradura", "+10 % de velocitat.", "Ferradura", Common),
        new(Amulet.Magnet, "Imant", "Atrau monedes i cures a quatre caselles.", "Imant", Common),
        new(Amulet.StoneHeart, "Cor de pedra", "+1 de vida màxima.", "Cor_de_pedra", Common),
        new(Amulet.HawkEye, "Ull de falcó", "+20 % d’abast i +15 % de camp de visió.", "Ull_de_falco", Rare),
        new(Amulet.PoisonDrop, "Gota de verí", "25 % de probabilitat d’enverinar durant 3 s.", "Gota_de_veri", Rare),
        new(Amulet.VampireTooth, "Dent de vampir", "Cura 1 cor cada 12 enemics derrotats.", "Dent_de_vampir", Rare),
        new(Amulet.LuckyCoin, "Moneda de la sort", "+25 % en monedes i 15 % de pujar la qualitat del botí d’un cofre.", "Moneda_de_la_sort", Rare),
        new(Amulet.Thorns, "Espines", "Torna 1 de mal a qui et pega cos a cos.", "Espines", Rare),
        new(Amulet.PhoenixFeather, "Ploma de fènix", "Reneixes una vegada amb el 50 % de vida.", "Ploma_de_fenix", Epic),
        new(Amulet.DragonHeart, "Cor de dragó", "+30 % de mal amb menys del 30 % de vida.", "Cor_de_drago", Epic),
        new(Amulet.ShatteredMirror, "Mirall trencat", "Els projectils guanyen un rebot.", "Mirall_trencat", Epic),
        new(Amulet.ForbiddenBook, "Llibre prohibit", "+50 % de mal i −1 de vida màxima.", "Llibre_prohibit", Epic),
        new(Amulet.FallenStar, "Estrella caiguda", "Cada 10 s fa 6 de mal en àrea al voltant de l’enemic més fort.", "Estrella_caiguda", Legendary)
    };

    public sealed class Loadout
    {
        public readonly Amulet[] Equipped = new Amulet[Slots];
        public Amulet[] Offer = Array.Empty<Amulet>();
        public int LastOfferFloor;
        public bool PhoenixUsed;
        public float GlassReady, StarReady;
        internal int Hits, Kills, Coins;

        public bool Has(Amulet id) => id != Amulet.None && Array.IndexOf(Equipped, id) >= 0;
    }

    static readonly Dictionary<int, Loadout> loadouts = new();
    static HashSet<string> seen;
    public static event System.Action Changed;
    public static int? RemoteAttacker { get; set; }
    public static bool Enabled => DungeonGenerator.Instance != null && RunState.Active;
    public static int LocalId => NetCoop.Online ? NetSession.Instance.LocalId : 0;
    public static Loadout Local => Get(LocalId);
    public static IEnumerable<KeyValuePair<int, Loadout>> Party => loadouts;

    public static Loadout Get(int id)
    {
        if (!loadouts.TryGetValue(id, out var kit)) loadouts[id] = kit = new Loadout();
        return kit;
    }

    public static void Reset()
    {
        loadouts.Clear();
        RemoteAttacker = null;
    }

    public static AmuletInfo Info(Amulet id) => id > Amulet.None && (int)id <= Catalog.Length ? Catalog[(int)id - 1] : null;

    static HashSet<string> Seen
    {
        get
        {
            if (seen == null) seen = new HashSet<string>(PlayerPrefs.GetString("Almanac_Amulets", "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            return seen;
        }
    }

    public static bool Discovered(Amulet id) => Seen.Contains(id.ToString());

    static void Discover(Loadout kit)
    {
        if (RunStats.DontSave) return;
        bool changed = false;
        foreach (var id in kit.Equipped)
            if (id != Amulet.None && Seen.Add(id.ToString())) changed = true;
        if (changed) PlayerPrefs.SetString("Almanac_Amulets", string.Join(",", Seen));
    }

    public static Amulet PickStock(System.Random rng, int floor)
    {
        var pool = Catalog.Where(info => (int)info.rarity <= (int)MaxRarity(floor)).ToArray();
        return pool[rng.Next(pool.Length)].id;
    }

    static ChestRarity MaxRarity(int floor) => floor < 3 ? Rare : floor < 5 ? Epic : Legendary;

    public static bool CanBuy(int id, Amulet amulet)
    {
        var kit = Get(id);
        return Enabled && Info(amulet) != null && !kit.Has(amulet) && Array.IndexOf(kit.Equipped, Amulet.None) >= 0;
    }

    public static void Buy(int id, Amulet amulet)
    {
        if (NetCoop.IsClient || !CanBuy(id, amulet)) return;
        var kit = Get(id);
        Equip(kit, Array.IndexOf(kit.Equipped, Amulet.None), amulet);
        UpdateState(id);
    }

    public static void OfferFloor()
    {
        if (!Enabled || NetCoop.IsClient) return;
        foreach (int id in PlayerIds())
        {
            var kit = Get(id);
            if (kit.LastOfferFloor >= RunState.Floor) continue;
            kit.LastOfferFloor = RunState.Floor;
            var rng = new System.Random(unchecked(RunState.Seed * 17 + RunState.Floor * 7919 + id));
            var pool = Catalog.Where(info => !kit.Has(info.id) && (int)info.rarity <= (int)MaxRarity(RunState.Floor)).ToList();
            var offer = new List<Amulet>();
            while (offer.Count < Slots && pool.Count > 0)
            {
                int RollWeight(AmuletInfo info) => info.rarity == Common ? 7 : info.rarity == Rare ? 4 : info.rarity == Epic ? 2 : 1;
                int roll = rng.Next(pool.Sum(RollWeight));
                int index = 0;
                while ((roll -= RollWeight(pool[index])) >= 0) index++;
                offer.Add(pool[index].id);
                pool.RemoveAt(index);
            }
            kit.Offer = offer.ToArray();
            UpdateState(id);
        }
    }

    public static bool WaitingForChoices => PlayerIds().Any(id => Get(id).Offer.Length > 0);

    static IEnumerable<int> PlayerIds()
    {
        if (!NetCoop.Online) { yield return LocalId; yield break; }
        foreach (int id in NetSession.Instance.Players.Keys) yield return id;
    }

    public static void Request(Action action, int first, int second = 0)
    {
        if (!Enabled) return;
        if (NetCoop.IsClient) NetCoop.AmuletRequest(action, first, second);
        else Apply(LocalId, action, first, second);
    }

    public static bool Apply(int id, Action action, int first, int second)
    {
        if (!Enabled || NetCoop.IsClient) return false;
        var kit = Get(id);
        switch (action)
        {
            case Action.Choose:
                if (first < 0 || first >= kit.Offer.Length || second < 0 || second >= Slots ||
                    kit.Has(kit.Offer[first])) return false;
                Equip(kit, second, kit.Offer[first]);
                kit.Offer = Array.Empty<Amulet>();
                break;
            case Action.Discard:
                if (first < 0 || first >= Slots || kit.Equipped[first] == Amulet.None) return false;
                Equip(kit, first, Amulet.None);
                break;
            case Action.Swap:
                if (first < 0 || first >= Slots || second < 0 || second >= Slots) return false;
                (kit.Equipped[first], kit.Equipped[second]) = (kit.Equipped[second], kit.Equipped[first]);
                break;
            case Action.Glass:
                if (!kit.Has(Amulet.GlassShield) || Time.time < kit.GlassReady) return false;
                kit.GlassReady = Time.time + 15f;
                break;
            case Action.Phoenix:
                if (!kit.Has(Amulet.PhoenixFeather) || kit.PhoenixUsed) return false;
                kit.PhoenixUsed = true;
                break;
            default:
                return false;
        }
        UpdateState(id);
        return true;
    }

    static void Equip(Loadout kit, int index, Amulet id)
    {
        kit.Equipped[index] = id;
        if (id == Amulet.FallenStar) kit.StarReady = Time.time + 10f;
    }

    static void UpdateState(int id)
    {
        if (id == LocalId) RefreshLocal();
        Changed?.Invoke();
        NetCoop.AmuletChanged(id);
    }

    static void RefreshLocal()
    {
        Discover(Local);
        var player = DungeonGenerator.Instance?.player;
        if (player != null) player.GetComponent<PlayerHealth>()?.RefreshAmuletHealth();
    }

    public static void Write(BinaryWriter writer, int id)
    {
        var kit = Get(id);
        writer.Write(id);
        foreach (var slot in kit.Equipped) writer.Write((byte)slot);
        writer.Write((byte)kit.Offer.Length);
        foreach (var offer in kit.Offer) writer.Write((byte)offer);
        writer.Write(kit.PhoenixUsed);
        writer.Write(Mathf.Max(0f, kit.GlassReady - Time.time));
        writer.Write(Mathf.Max(0f, kit.StarReady - Time.time));
    }

    public static void Read(BinaryReader reader)
    {
        int id = reader.ReadInt32();
        var kit = Get(id);
        for (int i = 0; i < Slots; i++) kit.Equipped[i] = (Amulet)reader.ReadByte();
        kit.Offer = new Amulet[reader.ReadByte()];
        for (int i = 0; i < kit.Offer.Length; i++) kit.Offer[i] = (Amulet)reader.ReadByte();
        kit.PhoenixUsed = reader.ReadBoolean();
        kit.GlassReady = Time.time + reader.ReadSingle();
        kit.StarReady = Time.time + reader.ReadSingle();
        if (id == LocalId) RefreshLocal();
        Changed?.Invoke();
    }

    public static bool Has(Amulet id) => Enabled && Local.Has(id);

    public static bool Defend(Action action)
    {
        if (!Enabled) return false;
        if (!NetCoop.IsClient) return Apply(LocalId, action, 0, 0);
        if (action == Action.Glass && Local.Has(Amulet.GlassShield) && Time.time >= Local.GlassReady)
            Local.GlassReady = Time.time + 15f;
        else if (action == Action.Phoenix && Local.Has(Amulet.PhoenixFeather) && !Local.PhoenixUsed)
            Local.PhoenixUsed = true;
        else return false;
        NetCoop.AmuletRequest(action, 0, 0);
        Changed?.Invoke();
        return true;
    }

    public static int ExtraHealth => (Has(Amulet.StoneHeart) ? 1 : 0) - (Has(Amulet.ForbiddenBook) ? 1 : 0);
    public static float MoveMultiplier => Has(Amulet.Horseshoe) ? 1.1f : 1f;
    public static float ViewMultiplier => Has(Amulet.HawkEye) ? 1.15f : 1f;
    public static float RangeMultiplier => Has(Amulet.HawkEye) ? 1.2f : 1f;
    public static float Cooldown(float time) => Has(Amulet.Hourglass) && time > .08f ? Mathf.Max(.08f, time * .88f) : time;
    public static float MagnetRange(float normal) => Has(Amulet.Magnet) ? 4f : normal;

    public static int CoinValue()
    {
        if (!Has(Amulet.LuckyCoin)) return 1;
        return ++Local.Coins % 4 == 0 ? 2 : 1;
    }

    public static ChestRarity ChestQuality(ChestRarity rarity, int playerId, float roll)
    {
        if (!Enabled || rarity >= Legendary || !Get(playerId).Has(Amulet.LuckyCoin) || roll >= .15f) return rarity;
        return rarity == ChestRarity.PocComu ? Common : rarity + 1;
    }

    public static int Hit(EnemyHealth target, int damage, out int attacker)
    {
        attacker = RemoteAttacker ?? LocalId;
        if (!Enabled || NetCoop.IsClient || damage <= 0 || damage >= 9999 || RunStats.HitBy == null) return damage;
        var kit = Get(attacker);
        float power = kit.Has(Amulet.ForbiddenBook) ? 1.5f : 1f;
        if (kit.Has(Amulet.DragonHeart) && PlayerHealthFraction(attacker) < .3f) power *= 1.3f;
        int extra = kit.Has(Amulet.Whetstone) && ++kit.Hits % 3 == 0 ? 1 : 0;
        if (kit.Has(Amulet.PoisonDrop) && UnityEngine.Random.value < .25f) target.PoisonFromAmulet(attacker);
        return Mathf.Max(1, Mathf.RoundToInt(damage * power)) + extra;
    }

    static float PlayerHealthFraction(int id)
    {
        if (id == LocalId)
        {
            var hp = DungeonGenerator.Instance?.player?.GetComponent<PlayerHealth>();
            if (hp != null) return hp.currentHealth / (float)hp.maxHealth;
        }
        else if (NetSession.Instance != null && NetSession.Instance.Players.TryGetValue(id, out var peer) && peer.maxHealth > 0)
            return peer.health / (float)peer.maxHealth;
        return 1f;
    }

    public static void Killed(int attacker)
    {
        if (!Enabled || NetCoop.IsClient || attacker < 0) return;
        var kit = Get(attacker);
        if (!kit.Has(Amulet.VampireTooth) || ++kit.Kills % 12 != 0) return;
        if (NetCoop.Online) NetCoop.HealAlly(attacker);
        else DungeonGenerator.Instance.player?.GetComponent<PlayerHealth>()?.Heal(1);
    }

    public static void Tick()
    {
        if (!Enabled || NetCoop.IsClient) return;
        foreach (int id in PlayerIds())
        {
            var kit = Get(id);
            if (!kit.Has(Amulet.FallenStar) || Time.time < kit.StarReady || NetCoop.IsDown(id)) continue;
            var player = DungeonGenerator.Instance.player;
            Vector2 origin;
            if (id == LocalId && player != null && player.gameObject.activeInHierarchy) origin = player.position;
            else if (NetSession.Instance != null && NetSession.Instance.Players.TryGetValue(id, out var peer) &&
                peer.scene == NetSession.SceneToken()) origin = new Vector2(peer.x, peer.y);
            else continue;
            var enemies = UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            var target = enemies.Where(enemy => !enemy.IsDead && enemy.enabled && !enemy.arenaPart &&
                Vector2.Distance(origin, enemy.transform.position) <= 10f).OrderByDescending(enemy => enemy.currentHealth).FirstOrDefault();
            if (target == null) continue;
            kit.StarReady = Time.time + 10f;
            Vector2 at = target.transform.position;
            Fx.ExplodeVisual(at, 2.1f);
            NetCoop.AmuletExplosion(at, 2.1f);
            foreach (var enemy in enemies)
                if (!enemy.IsDead && enemy.enabled && Vector2.Distance(at, enemy.transform.position) <= 2.1f)
                    enemy.TakeDamage(6);
            UpdateState(id);
        }
    }
}

