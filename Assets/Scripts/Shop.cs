using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// v1.4.1: mercader i màquina expenedora de la sala de tresor (disseny v1.4-mercader.md).
// L'estoc ix d'una llavor compartida, així que tots els equips veuen el mateix sense missatges d'estoc;
// l'amfitrió decideix qui compra primer (ShopReq/ShopSold) i cada jugador paga amb les SEUES monedes de la partida.

public class ShopSlot
{
    public string title = "", info = "";
    public Sprite icon;
    public int price;
    public bool offer, sold, repair, surprise;
    public ChestRarity rarity = ChestRarity.Comu;
    public WeaponData weapon;
    public HealItem heal;
}

public class Shop : MonoBehaviour
{
    public const int MerchantKind = 0, VendingKind = 1;
    public int kind;
    public ShopSlot[] slots = new ShopSlot[0];
    public bool jammed;

    static readonly List<Shop> all = new List<Shop>();
    public static Shop Find(int k) { all.RemoveAll(s => s == null); return all.FirstOrDefault(s => s.kind == k); }

    SpriteRenderer body;
    Sprite quiet, speaking;
    float speakUntil, lastLine = -99f;
    bool greeted;
    TextMeshPro prompt;
    int pendingSlot = -1; float pendingUntil;
    System.Random lines;

    public bool IsMerchant => kind == MerchantKind;
    public string Title => IsMerchant ? "MERCADER" : "MÀQUINA EXPENEDORA";
    public Sprite Portrait => Resources.Load<Sprite>(IsMerchant ? "Pixel/Botiga/Mercader_retrat" : "Pixel/Botiga/Maquina_vending");

    // ----------------------------------------------------------------- creació al món

    public static Shop Spawn(Vector3 pos, int kind, int seed)
    {
        var go = new GameObject(kind == MerchantKind ? "Mercader" : "Maquina expenedora");
        go.transform.position = pos;
        var shop = go.AddComponent<Shop>();
        shop.kind = kind;
        shop.body = go.AddComponent<SpriteRenderer>();
        shop.body.sortingLayerName = "Player"; shop.body.sortingOrder = 2;
        shop.quiet = Resources.Load<Sprite>(kind == MerchantKind ? "Pixel/Botiga/Mercader_quiet" : "Pixel/Botiga/Maquina_vending");
        shop.speaking = kind == MerchantKind ? Resources.Load<Sprite>("Pixel/Botiga/Mercader_parla") : shop.quiet;
        shop.body.sprite = shop.quiet;
        var box = go.AddComponent<BoxCollider2D>(); box.size = new Vector2(.9f, .8f);
        if (kind == MerchantKind)
        {   // taulell de fusta davant del mercader
            var table = new GameObject("Taulell"); table.transform.SetParent(go.transform, false); table.transform.localPosition = new Vector3(0f, -1f, 0f);
            var sr = table.AddComponent<SpriteRenderer>(); sr.sprite = Resources.Load<Sprite>("Pixel/Botiga/Taulell_fusta");
            sr.sortingLayerName = "Player"; sr.sortingOrder = 3;
            table.AddComponent<BoxCollider2D>().size = new Vector2(1f, .9f);
        }
        shop.lines = new System.Random(seed ^ 0x5A17);
        shop.Stock(new System.Random(seed + kind * 977), RunState.Floor);
        all.Add(shop);
        return shop;
    }

    void OnDestroy() { all.Remove(this); }

    // ----------------------------------------------------------------- estoc

    static readonly string[] EntryLines =
    {
        "Bones, aventurer! Tinc de tot, i el que no tinc, ho aconseguisc... cobrant.",
        "Passe, passe! Que no mossegue. El Mímic sí.",
        "Monedes? Perfecte. Em falten uns quants viatges.",
        "Ací tens la millor botiga de la masmorra. La única, però la millor.",
        "Mira, mira, però no toques la màquina. Embussa i es queda la moneda.",
        "Xe, quina sort! Justet vaig cobrir l'estoc de hui.",
        "Hui tinc ofertes. Demà, no sé. Depén de com em lleve.",
        "Una cura a temps val més que una arma rara. Ho dic per experiència.",
        "Si ets amic dels Llops de neu, ja m'ho diràs. Jo, de moment, no.",
        "Pague amb monedes i et dic un secret: les parets també tenen oïdes.",
    };

    static int WeaponBase(ChestRarity r) => r == ChestRarity.PocComu ? 30 : r == ChestRarity.Comu ? 40 : r == ChestRarity.Rar ? 90 : r == ChestRarity.Epic ? 200 : 400;

    static int HealBase(HealItem item)
    {
        string n = item.itemName.ToLowerInvariant();
        return n.StartsWith("poma") ? 10 : n.StartsWith("bena") ? 15 : n.StartsWith("poci") ? 26 : n.StartsWith("farma") ? 45 : 12 + 8 * (int)item.rarity;
    }

    static int Final(int basePrice, int floor, System.Random rng) =>
        Mathf.Max(1, Mathf.RoundToInt(basePrice * (1f + .1f * floor) * Mathf.Lerp(.85f, 1.15f, (float)rng.NextDouble())));

    static ChestRarity PickRarity(System.Random rng, int floor)
    {
        // Com els cofres: al pis 1 no hi ha èpics ni llegendaris
        float comu = Mathf.Max(1f, 6f - floor), rar = 3f + floor, epic = floor >= 3 ? floor - 2f : 0f, leg = floor >= 6 ? 1f : 0f;
        float x = (float)rng.NextDouble() * (comu + rar + epic + leg);
        ChestRarity r = (x -= comu) < 0 ? ChestRarity.Comu : (x -= rar) < 0 ? ChestRarity.Rar : (x -= epic) < 0 ? ChestRarity.Epic : ChestRarity.Llegendari;
        if (rng.NextDouble() < .15 && r < ChestRarity.Llegendari) r++; // 15 % d'una rareza més
        return r;
    }

    static WeaponData PickWeapon(System.Random rng, ChestRarity rarity, IEnumerable<WeaponData> all)
    {
        var pool = all.Where(w => w != null && w.rarity == rarity && w.special != SpecialKind.HealBolt).ToList();
        if (pool.Count == 0) pool = all.Where(w => w != null && w.rarity == ChestRarity.Comu).ToList();
        return pool.Count == 0 ? null : pool[rng.Next(pool.Count)];
    }

    static HealItem PickHeal(System.Random rng, IList<HealItem> items)
    {
        if (items.Count == 0) return null;
        float total = items.Sum(i => i.rarity == ChestRarity.Comu ? 6f : i.rarity == ChestRarity.Rar ? 3f : 1.5f), x = (float)rng.NextDouble() * total;
        foreach (var i in items) { x -= i.rarity == ChestRarity.Comu ? 6f : i.rarity == ChestRarity.Rar ? 3f : 1.5f; if (x <= 0) return i; }
        return items[0];
    }

    void Stock(System.Random rng, int floor)
    {
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        var weapons = catalog != null && catalog.weapons != null ? catalog.weapons.Where(w => w != null).OrderBy(w => w.name).ToList() : new List<WeaponData>();
        var heals = HealItem.All.Where(h => h != null).OrderBy(h => h.name).ToList();
        var list = new List<ShopSlot>();

        ShopSlot Heal(float multiplier = 1f)
        {
            var item = PickHeal(rng, heals); if (item == null) return null;
            return new ShopSlot { heal = item, icon = item.icon, title = item.itemName, info = "Cura " + item.healAmount + (item.healAmount == 1 ? " cor" : " cors"), rarity = item.rarity,
                price = IsMerchant ? Final(HealBase(item), floor, rng) : Mathf.Max(3, Mathf.RoundToInt(HealBase(item) * .5f * Mathf.Lerp(.8f, 1.2f, (float)rng.NextDouble()))) };
        }
        ShopSlot Weapon(ChestRarity r, bool surprise = false)
        {
            var w = PickWeapon(rng, r, weapons); if (w == null) return null;
            return new ShopSlot { weapon = w, icon = w.icon, title = surprise ? "Sorpresa!" : w.weaponName, rarity = w.rarity, surprise = surprise,
                info = surprise ? "Una arma de rareza desconeguda" : "Dany " + w.damage + " · " + (w.cooldown > 0 ? (1f / w.cooldown).ToString("0.0") : "-") + " trets/s · abast " + w.range.ToString("0.#"),
                price = surprise ? Final(60, floor, rng) : Final(WeaponBase(w.rarity), floor, rng) };
        }

        if (!IsMerchant)
        {   // Màquina: tres articles barats i limitats
            for (int i = 0; i < 3; i++) { ShopSlot s = null; for (int tries = 0; tries < 8; tries++) { s = Heal(); if (s == null || !list.Any(o => o.title == s.title)) break; } if (s != null) list.Add(s); } // sense repetits
        }
        else
        {
            for (int i = 0; i < 2; i++) { ShopSlot s = null; for (int tries = 0; tries < 8; tries++) { s = Heal(); if (s == null || !list.Any(o => o.title == s.title)) break; } if (s != null) list.Add(s); }
            var main = PickRarity(rng, floor);
            var a = Weapon(main); if (a != null) list.Add(a);
            var b = Weapon(main < ChestRarity.Llegendari ? main + 1 : main); if (b != null) list.Add(b); // «amulet»: fins que n'hi haja, una arma extra
            list.Add(new ShopSlot { repair = true, title = "Recarregar combustible", info = "Torna a omplir les armes de trets limitats", rarity = ChestRarity.Comu });
            if (rng.NextDouble() < .20) { var s = Weapon((ChestRarity)rng.Next(0, 4), true); if (s != null) list.Add(s); }
            // Una ranura a l'atzar, amb -30 % (OFERTA)
            var candidates = list.Where(s => !s.repair).ToList();
            if (candidates.Count > 0) { var o = candidates[rng.Next(candidates.Count)]; o.offer = true; o.price = Mathf.Max(1, Mathf.RoundToInt(o.price * .7f)); }
        }
        slots = list.ToArray();
    }

    // Preu del servei: 1 moneda per cada 3 % gastat (mínim 15)
    public int RepairPrice()
    {
        var attack = FindFirstObjectByType<PlayerAttack>();
        float used = attack != null ? attack.FuelUsedFraction() : -1f;
        return used <= 0f ? 0 : Mathf.Max(15, Mathf.CeilToInt(used * 100f / 3f));
    }

    public int PriceOf(ShopSlot s) => s.repair ? RepairPrice() : s.price;

    // ----------------------------------------------------------------- paraules

    public void Say(string text, bool force = false)
    {
        if (!IsMerchant || string.IsNullOrEmpty(text)) return;
        if (!force && Time.unscaledTime - lastLine < 4f) return; // màxim 1 missatge cada 4 s
        lastLine = Time.unscaledTime; speakUntil = Time.time + 2.5f;
        ChatBubble.Show(gameObject, text);
        if (NetSession.Active && NetSession.Instance != null) NetSession.Instance.LocalNotice("<color=#F2C94C>Mercader:</color> " + text);
    }

    // ----------------------------------------------------------------- interacció

    void Update()
    {
        if (body != null && IsMerchant) body.sprite = Time.time < speakUntil && ((int)(Time.time * 5f) & 1) == 0 ? speaking : quiet;
        if (pendingSlot >= 0 && Time.unscaledTime > pendingUntil) pendingSlot = -1;

        var player = GameObject.FindWithTag("Player");
        if (player == null) { SetPrompt(false); return; }
        float distance = Vector2.Distance(player.transform.position, transform.position);
        if (IsMerchant && !greeted && distance < 4.5f) { greeted = true; Say(EntryLines[lines.Next(EntryLines.Length)], true); }
        bool near = distance < 2.0f && !ShopUI.IsOpen;
        SetPrompt(near);
        if (near && GameControls.Down(5))
        {
            if (!IsMerchant && jammed) { jammed = false; CoinDrop.Spawn(transform.position, transform.position + Vector3.down); Fx.Text(transform.position + Vector3.up, "Un colp i torna 1 moneda", UIKit.Gold); return; }
            ShopUI.Open(this);
        }
    }

    void SetPrompt(bool show)
    {
        if (prompt == null)
        {
            if (!show) return;
            var go = new GameObject("Avis botiga", typeof(TextMeshPro)); go.transform.SetParent(transform, false); go.transform.localPosition = new Vector3(0f, IsMerchant ? .95f : .9f, 0f);
            prompt = go.GetComponent<TextMeshPro>(); prompt.fontSize = 2.2f; prompt.alignment = TextAlignmentOptions.Center; prompt.rectTransform.sizeDelta = new Vector2(5f, 1f);
            var mr = go.GetComponent<MeshRenderer>(); mr.sortingLayerName = "PlayerName"; mr.sortingOrder = 50;
            prompt.text = "E: " + (IsMerchant ? "Botiga" : "Màquina");
        }
        if (prompt.gameObject.activeSelf != show) prompt.gameObject.SetActive(show);
    }

    // ----------------------------------------------------------------- compra

    // Comprovacions locals del comprador (monedes i article); després, a l'amfitrió.
    public void Buy(int index)
    {
        if (index < 0 || index >= slots.Length || pendingSlot >= 0) return;
        var s = slots[index];
        if (s.sold) { Say("Això ja ho han comprat."); return; }
        int price = PriceOf(s);
        if (s.repair && price <= 0) { Fx.Text(transform.position + Vector3.up, "Cap arma per recarregar", UIKit.Muted); return; }
        if (RunState.Coins < price) { Say("Sense monedes, sense tracte. Torna quan la butxaca pese."); return; }
        if (NetCoop.IsClient) { pendingSlot = index; pendingUntil = Time.unscaledTime + 3f; NetCoop.ShopBuyRequest(kind, index); return; }
        HostResolve(kind, index, NetSession.Instance != null && NetCoop.Online ? NetSession.Instance.LocalId : 0);
    }

    // L'amfitrió (o la partida sola) decideix qui s'ho queda
    public static void HostResolve(int kind, int index, int buyer)
    {
        var shop = Find(kind);
        if (shop == null || index < 0 || index >= shop.slots.Length || shop.slots[index].sold) return;
        shop.slots[index].sold = true;
        NetCoop.ShopSoldBroadcast(kind, index, buyer);
        if (buyer == (NetCoop.Online ? NetSession.Instance.LocalId : 0)) shop.Complete(index);
    }

    // Un company (o jo mateix) ha comprat: marca l'estoc i, si sóc el comprador, cobra i entrega
    public static void OnSold(int kind, int index, int buyer)
    {
        var shop = Find(kind);
        if (shop == null || index < 0 || index >= shop.slots.Length) return;
        bool already = shop.slots[index].sold; shop.slots[index].sold = true;
        shop.pendingSlot = -1;
        if (NetCoop.Online && buyer == NetSession.Instance.LocalId && !NetCoop.IsHost) shop.Complete(index);
        else if (!already) ShopUI.Refresh();
    }

    void Complete(int index)
    {
        var s = slots[index];
        var player = GameObject.FindWithTag("Player"); if (player == null) return;
        int price = PriceOf(s);
        if (!CoinCounter.Spend(price)) { slots[index].sold = false; return; }
        Vector3 at = player.transform.position;
        Vector3 to = at + (Vector3)(Random.insideUnitCircle.normalized * 1.1f);
        if (!IsMerchant && Random.value < .03f)
        {   // 3 % de probabilitat d'embussar-se i quedar-se la moneda
            jammed = true; Fx.Text(transform.position + Vector3.up, "MÀQUINA EMBUSSADA", UIKit.Red);
        }
        else if (s.repair) { var attack = player.GetComponent<PlayerAttack>(); if (attack != null) attack.RefuelAll(); Say("Això es pot arreglar. Per un preu, clar.", true); }
        else
        {
            if (s.weapon != null) ItemPickup.Spawn(s.weapon, transform.position, to);
            else if (s.heal != null) HealPickup.Spawn(s.heal, transform.position, to);
            MobSounds.PlayLocal("sala_monedes", transform.position, .6f);
            Say(Random.value < .5f ? "Un plaer fer negocis!" : "Això donarà vida a la teua aventura... o a la meua butxaca.", true);
        }
        if (slots.All(x => x.sold)) Say("M'has deixat net. Torna a la planta següent.");
        ShopUI.Refresh();
    }
}
