using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

// Objectes compartits de la partida online:
//  - Monedes, cures i armes del terra: les veu tothom i se les queda el PRIMER que les agafa
//    (l'amfitrio fa d'arbitre: un client demana "me la quede?" i l'amfitrio diu si o no).
//  - Caixes i barrils (TNT): si algu en trenca un, es trenca per a tots. El dany de l'explosio
//    el rep cada jugador al seu PC; el botí el genera nomes qui l'ha trencat (i es comparteix).
//  - Avisos d'atac del boss (cercles rojos) tambe als clients.
//  - Espectar: si caus, la camera segueix un company (A/D o clic per canviar).
public static class NetShared
{
    static NetSession S => NetSession.Instance;
    static int Token => NetSession.SceneToken();

    public enum Kind : byte { Coin = 0, Heal = 1, Weapon = 2 }

    static int nextItem = 1;
    static readonly Dictionary<int, NetItem> items = new Dictionary<int, NetItem>();
    static readonly HashSet<int> taken = new HashSet<int>();   // amfitrio: objectes ja agafats
    static bool spawningRemote, breakingRemote;
    static Dictionary<string, HealItem> heals;

    // Per a les proves automatiques
    public static int RemoteItemsSeen, ItemsTakenByOthers, RemoteBreaks;
    public static int RemoteHealsSeen;

    // Companys dibuixats en aquesta escena (es busca un cop per fotograma)
    static readonly List<RemoteAvatar> avatarCache = new List<RemoteAvatar>();
    static int avatarFrame = -1;
    public static List<RemoteAvatar> Avatars()
    {
        if (avatarFrame == Time.frameCount) return avatarCache;
        avatarFrame = Time.frameCount;
        avatarCache.Clear();
        if (NetCoop.Online) avatarCache.AddRange(Object.FindObjectsByType<RemoteAvatar>(FindObjectsSortMode.None));
        return avatarCache;
    }

    public static void Reset()
    {
        items.Clear(); taken.Clear(); mapTaken.Clear(); mapPending.Clear(); Spectate.Stop();
    }

    // =====================================================================
    //  OBJECTES DEL TERRA
    // =====================================================================
    // El criden CoinDrop / HealPickup / ItemPickup en apareixer
    public static void Spawned(Component pickup, Kind kind, string data, Vector3 from, Vector3 to)
    {
        if (!NetCoop.Online || spawningRemote || pickup == null) return;
        var ni = pickup.gameObject.AddComponent<NetItem>();
        ni.id = S.LocalId * 1000000 + nextItem++;
        ni.kind = kind; ni.data = data ?? ""; ni.destination = to;
        items[ni.id] = ni;
        int from_ = S.LocalId, id = ni.id;
        S.SendGame(NetProtocol.Frame(Msg.PickupSpawn, w =>
        {
            w.Write(from_); w.Write(Token); w.Write(id); w.Write((byte)kind); w.Write(data ?? "");
            w.Write(from.x); w.Write(from.y); w.Write(to.x); w.Write(to.y);
            w.Write(pickup is ItemPickup weapon ? weapon.CooldownRemaining : 0f);
        }));
    }

    // El jugador local toca un objecte. Retorna true si NO l'ha d'agafar encara (ho decideix l'amfitrio).
    // 'collect' es el codi normal d'agafar-lo (premi + Destroy).
    public static bool Claim(Component pickup, System.Action collect)
    {
        if (!NetCoop.Online || pickup == null) return false;
        var ni = pickup.GetComponent<NetItem>();
        if (ni == null) return false;
        if (!NetCoop.IsHost && taken.Contains(ni.id)) return true;
        if (NetCoop.IsHost)
        {
            if (!taken.Add(ni.id)) { Object.Destroy(pickup.gameObject); return true; }
            int id = ni.id;
            S.SendAll(NetProtocol.Frame(Msg.PickupTaken, w => w.Write(id)));
            items.Remove(id);
            return false;
        }
        if (ni.pending) return true;
        // Amb latència, dos objectes poden vore la mateixa casella buida en el mateix frame.
        // La segona arma o cura torna a comprovar l'espai després de rebre la concessió.
        if (ni.kind != Kind.Coin)
            foreach (var request in items.Values)
                if (request != null && request.kind == ni.kind && request.pending) return true;
        ni.pending = true; ni.until = Time.unscaledTime + 3f; ni.collect = collect;
        foreach (var r in pickup.GetComponentsInChildren<SpriteRenderer>()) r.enabled = false; // s'amaga mentre esperem
        int cid = ni.id;
        S.SendToHost(NetProtocol.Frame(Msg.PickupClaim, w => w.Write(cid)));
        return true;
    }

    // Mode Creatiu: l'amfitrio lleva un objecte del terra. Als altres els arriba com si algu l'haguera agafat.
    public static void HostRemove(Component pickup)
    {
        if (pickup == null) return;
        var ni = pickup.GetComponent<NetItem>();
        if (NetCoop.IsHost && ni != null && taken.Add(ni.id))
        {
            int id = ni.id;
            S.SendAll(NetProtocol.Frame(Msg.PickupTaken, w => w.Write(id)));
            items.Remove(id);
        }
        Object.Destroy(pickup.gameObject);
    }

    static void SpawnRemote(int id, Kind kind, string data, Vector3 from, Vector3 to, float cooldownRemaining)
    {
        if (taken.Contains(id) || items.ContainsKey(id)) return;
        Component c = null;
        spawningRemote = true;
        try
        {
            switch (kind)
            {
                case Kind.Coin: c = CoinDrop.SpawnAt(from, to); break;
                case Kind.Heal: { var h = Heal(data); if (h != null) c = HealPickup.Spawn(h, from, to); break; }
                case Kind.Weapon: { var wd = NetCoop.Weapon(data); if (wd != null) c = ItemPickup.Spawn(wd, from, to, cooldownRemaining: cooldownRemaining); break; }
            }
        }
        finally { spawningRemote = false; }
        if (c == null) return;
        var ni = c.gameObject.AddComponent<NetItem>(); ni.id = id;
        ni.kind = kind; ni.data = data ?? ""; ni.destination = to;
        items[id] = ni;
        RemoteItemsSeen++;
        if (kind == Kind.Heal) RemoteHealsSeen++;
    }

    static HealItem Heal(string name)
    {
        if (heals == null || !heals.ContainsKey(name))
        {
            heals = new Dictionary<string, HealItem>();
            foreach (var h in HealItem.All) if (h != null && !heals.ContainsKey(h.name)) heals[h.name] = h;
        }
        return heals.TryGetValue(name, out var found) ? found : null;
    }

    static void RemoveItem(int id)
    {
        if (items.TryGetValue(id, out var ni)) { items.Remove(id); if (ni != null) { Fx.Burst(ni.transform.position, new Color(1f, 0.9f, 0.5f), 3, 1f); Object.Destroy(ni.gameObject); } }
        ItemsTakenByOthers++;
    }

    // =====================================================================
    //  CAIXES I BARRILS
    // =====================================================================
    public static int BreakableKey(Breakable b)
    {
        var p = b.transform.position;
        return Mathf.RoundToInt(p.x * 4) * 100003 + Mathf.RoundToInt(p.y * 4);
    }

    public static bool RequestBreakableHit(Breakable b, int damage)
    {
        if (!NetCoop.IsClient) return false;
        S.SendToHost(NetProtocol.Frame(Msg.BreakableHit, w =>
        { w.Write(Token); w.Write(BreakableKey(b)); w.Write(Mathf.Clamp(damage, 0, 10000)); }));
        return true;
    }

    // El crida Breakable.Break quan l'ha trencat algu d'aquest PC
    public static void Broken(Breakable b)
    {
        if (!NetCoop.Online || breakingRemote) return;
        int key = BreakableKey(b), from = S.LocalId; bool host = NetCoop.IsHost;
        S.SendGame(NetProtocol.Frame(Msg.BreakableBroken, w => { w.Write(from); w.Write(Token); w.Write(key); w.Write(host); }));
    }

    static void BreakRemote(int key, bool explosionHandled)
    {
        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsSortMode.None))
            if (BreakableKey(b) == key)
            {
                breakingRemote = true;
                try { b.RemoteBreak(explosionHandled); } finally { breakingRemote = false; }
                RemoteBreaks++;
                return;
            }
    }

    // =====================================================================
    //  MONEDES DEL MAPA (animades, posades a l'escena)
    // =====================================================================
    static readonly HashSet<int> mapTaken = new HashSet<int>();
    static readonly Dictionary<int, (Coin coin, System.Action collect, float until)> mapPending = new Dictionary<int, (Coin, System.Action, float)>();
    public static int MapCoinsGranted, MapCoinsLost;
    public static int MapTakenCount => mapTaken.Count;

    public static int MapCoinKey(Component c)
    {
        var p = c.transform.position;
        return Mathf.RoundToInt(p.x * 4) * 100003 + Mathf.RoundToInt(p.y * 4);
    }

    static Coin FindMapCoin(int key)
    {
        foreach (var c in Object.FindObjectsByType<Coin>(FindObjectsSortMode.None)) if (MapCoinKey(c) == key) return c;
        return null;
    }

    // Retorna true si NO s'ha d'agafar ara (esperem l'amfitrio o ja l'han agafada)
    public static bool ClaimMapCoin(Coin c, System.Action collect)
    {
        if (!NetCoop.Online || c == null) return false;
        int key = MapCoinKey(c), token = Token;
        if (NetCoop.IsHost)
        {
            if (!mapTaken.Add(key)) { c.NetTakenByOther(); return true; }
            S.SendAll(NetProtocol.Frame(Msg.MapCoinTaken, w => { w.Write(token); w.Write(key); }));
            return false;
        }
        mapPending[key] = (c, collect, Time.unscaledTime + 3f);
        S.SendToHost(NetProtocol.Frame(Msg.MapCoinReq, w => { w.Write(token); w.Write(key); }));
        return true;
    }

    // Per a qui arriba tard a l'escena
    public static void Resync(int playerId)
    {
        int token = Token;
        // Els avisos inicials poden arribar mentre el client encara carrega el pis.
        // Reenviar nomes els objectes que continuen al terra; SpawnRemote evita duplicats.
        foreach (var ni in items.Values)
        {
            if (ni == null || taken.Contains(ni.id)) continue;
            int id = ni.id; var kind = ni.kind; string data = ni.data;
            Vector3 from = ni.transform.position, to = ni.destination;
            S.SendTo(playerId, NetProtocol.Frame(Msg.PickupSpawn, w => {
                w.Write(id / 1000000); w.Write(token); w.Write(id); w.Write((byte)kind); w.Write(data);
                w.Write(from.x); w.Write(from.y); w.Write(to.x); w.Write(to.y);
                w.Write(ni.GetComponent<ItemPickup>()?.CooldownRemaining ?? 0f);
            }));
        }
        foreach (var key in mapTaken) { int k = key; S.SendTo(playerId, NetProtocol.Frame(Msg.MapCoinTaken, w => { w.Write(token); w.Write(k); })); }
    }

    // =====================================================================
    //  AVISOS D'ATAC (boss)
    // =====================================================================
    public static void Telegraph(Vector2 pos, float scale, float seconds, int enemyId = 0)
    {
        if (!NetCoop.IsHost) return;
        S.SendAll(NetProtocol.Frame(Msg.Telegraph, w => { w.Write(Token); w.Write(pos.x); w.Write(pos.y); w.Write(scale); w.Write(seconds); w.Write(enemyId); }));
    }

    public static IEnumerator ShowTelegraph(Vector2 pos, float scale, float seconds, EnemyHealth owner = null)
    {
        var m = Fx.MakeSprite("Avis atac", pos, Fx.Disc, new Color(1f, 0.2f, 0.2f, 0.35f), "Shadows", 5);
        m.transform.localScale = Vector3.one * scale;
        bool hasOwner = owner != null;
        float expires = Time.time + seconds;
        while (m != null && Time.time < expires && (!hasOwner || (owner != null && !owner.IsDead)))
            yield return null;
        if (m != null) Object.Destroy(m.gameObject);
    }

    // =====================================================================
    //  MISSATGES
    // =====================================================================
    public static void OnHost(NetPlayer from, Msg type, BinaryReader r)
    {
        switch (type)
        {
            case Msg.PickupSpawn:
            {
                r.ReadInt32(); int token = r.ReadInt32(); int id = r.ReadInt32(); var kind = (Kind)r.ReadByte(); string data = r.ReadString();
                var a = new Vector3(r.ReadSingle(), r.ReadSingle()); var b = new Vector3(r.ReadSingle(), r.ReadSingle());
                float cooldownRemaining = r.ReadSingle();
                int fid = from.id;
                if (token != Token || id / 1000000 != fid || taken.Contains(id) || items.ContainsKey(id)) break;
                S.SendAllExcept(fid, NetProtocol.Frame(Msg.PickupSpawn, w => { w.Write(fid); w.Write(token); w.Write(id); w.Write((byte)kind); w.Write(data); w.Write(a.x); w.Write(a.y); w.Write(b.x); w.Write(b.y); w.Write(cooldownRemaining); }));
                if (token == Token) SpawnRemote(id, kind, data, a, b, cooldownRemaining);
                break;
            }
            case Msg.PickupClaim:
            {
                int id = r.ReadInt32(); int fid = from.id;
                if (!items.TryGetValue(id, out var item) || item == null || !taken.Add(id))
                {
                    S.SendTo(fid, NetProtocol.Frame(Msg.PickupTaken, w => w.Write(id)));
                    break;
                }
                S.SendTo(fid, NetProtocol.Frame(Msg.PickupGrant, w => w.Write(id)));
                S.SendAllExcept(fid, NetProtocol.Frame(Msg.PickupTaken, w => w.Write(id)));
                RemoveItem(id);
                break;
            }
            case Msg.MapCoinReq:
            {
                int token = r.ReadInt32(); int key = r.ReadInt32(); int fid = from.id;
                if (token != Token) break;
                if (!mapTaken.Add(key)) { S.SendTo(fid, NetProtocol.Frame(Msg.MapCoinTaken, w => { w.Write(token); w.Write(key); })); break; }
                S.SendTo(fid, NetProtocol.Frame(Msg.MapCoinGrant, w => { w.Write(token); w.Write(key); }));
                S.SendAllExcept(fid, NetProtocol.Frame(Msg.MapCoinTaken, w => { w.Write(token); w.Write(key); }));
                var c = FindMapCoin(key); if (c != null) c.NetTakenByOther();
                break;
            }
            case Msg.BreakableHit:
            {
                int token = r.ReadInt32(), key = r.ReadInt32(), damage = r.ReadInt32();
                if (token != Token) break;
                foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsSortMode.None))
                    if (BreakableKey(b) == key)
                    {
                        if (DungeonGenerator.CanDamageBreakableAt(b.transform.position)) { b.Hit(Mathf.Clamp(damage, 0, 10000)); if(b.health <= 0) RemoteBreaks++; }
                        break;
                    }
                break;
            }
        }
    }

    public static void OnClient(Msg type, BinaryReader r)
    {
        switch (type)
        {
            case Msg.PickupSpawn:
            {
                r.ReadInt32(); int token = r.ReadInt32(); int id = r.ReadInt32(); var kind = (Kind)r.ReadByte(); string data = r.ReadString();
                var a = new Vector3(r.ReadSingle(), r.ReadSingle()); var b = new Vector3(r.ReadSingle(), r.ReadSingle());
                float cooldownRemaining = r.ReadSingle();
                if (token == Token) SpawnRemote(id, kind, data, a, b, cooldownRemaining);
                break;
            }
            case Msg.PickupGrant:
            {
                int id = r.ReadInt32();
                if (taken.Add(id) && items.TryGetValue(id, out var ni) && ni != null)
                {
                    items.Remove(id);
                    var go = ni.gameObject;
                    ni.pending = false;
                    ni.collect?.Invoke();
                    // Si no s'ha pogut quedar (bossa plena...), torna a vore's
                    if (go != null) foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = true;
                }
                break;
            }
            case Msg.PickupTaken:
            {
                int id = r.ReadInt32();
                taken.Add(id);
                RemoveItem(id);
                break;
            }
            case Msg.MapCoinGrant:
            {
                int token = r.ReadInt32(); int key = r.ReadInt32();
                if (token == Token && mapPending.TryGetValue(key, out var p)) { mapPending.Remove(key); if (p.coin != null) { p.collect?.Invoke(); MapCoinsGranted++; } }
                break;
            }
            case Msg.MapCoinTaken:
            {
                int token = r.ReadInt32(); int key = r.ReadInt32();
                if (token != Token) break;
                mapPending.Remove(key);
                var c = FindMapCoin(key); if (c != null) { c.NetTakenByOther(); MapCoinsLost++; }
                break;
            }
            case Msg.BreakableBroken:
            {
                r.ReadInt32(); int token = r.ReadInt32(); int key = r.ReadInt32(); bool byHost = r.ReadBoolean();
                // Si l'ha trencat l'amfitrio, l'explosio (efecte i dany) ja ha arribat per una altra via
                if (token == Token) BreakRemote(key, byHost);
                break;
            }
            case Msg.Telegraph:
            {
                int token = r.ReadInt32(); var p = new Vector2(r.ReadSingle(), r.ReadSingle()); float scale = r.ReadSingle(), secs = r.ReadSingle();
                int enemyId = r.ReadInt32();
                if (token != Token) break;
                var owner = enemyId != 0 ? NetCoop.FindEnemy(enemyId) : null;
                if (enemyId == 0 || (owner != null && !owner.IsDead))
                    S.StartCoroutine(ShowTelegraph(p, scale, secs, owner));
                break;
            }
        }
    }

    public static void Tick()
    {
        if (!NetCoop.IsClient) return;
        // Monedes del mapa sense resposta: es poden tornar a provar
        if (mapPending.Count > 0)
            foreach (var key in new List<int>(mapPending.Keys))
                if (Time.unscaledTime > mapPending[key].until) { var p = mapPending[key]; mapPending.Remove(key); if (p.coin != null) p.coin.NetRelease(); }
        // Demandes sense resposta: l'objecte torna a vore's
        foreach (var ni in items.Values)
            if (ni != null && ni.pending && Time.unscaledTime > ni.until)
            {
                ni.pending = false;
                foreach (var sr in ni.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = true;
            }
    }
}

// Marca d'un objecte compartit
public class NetItem : MonoBehaviour
{
    public int id;
    [System.NonSerialized] public NetShared.Kind kind;
    [System.NonSerialized] public string data;
    [System.NonSerialized] public Vector3 destination;
    [System.NonSerialized] public bool pending;
    [System.NonSerialized] public float until;
    [System.NonSerialized] public System.Action collect;
}

// Mode espectador: la camera segueix un company viu mentre estas caigut
public class Spectate : MonoBehaviour
{
    static Spectate instance;
    CameraFollow cam;
    Transform original;
    int index;
    TextMeshProUGUI label;
    GameObject canvas;

    public static bool Active => instance != null;

    public static void Begin(Transform localPlayer)
    {
        if (instance != null) return;
        var go = new GameObject("Espectador");
        instance = go.AddComponent<Spectate>();
        instance.cam = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        instance.original = instance.cam != null ? instance.cam.target : localPlayer;
        instance.canvas = UIKit.Canvas("Espectant", 95);
        instance.canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>().enabled = MobileInput.Active;
        var bar = UIKit.Box(instance.canvas.transform, Vector2.zero, new Vector2(760, 78), new Color(.02f, .04f, .06f, .82f), "Espectant");
        bar.anchorMin = bar.anchorMax = new Vector2(.5f, 0); bar.pivot = new Vector2(.5f, 0); bar.anchoredPosition = new Vector2(0, 205);
        instance.label = UIKit.Text(bar, "", Vector2.zero, new Vector2(740, 74), 22, TextAlignmentOptions.Center);
        if(MobileInput.Active)
        {
            UIKit.Button(bar,"<",new Vector2(-430,0),new Vector2(75,75),()=>{if(instance!=null)instance.index--;});
            UIKit.Button(bar,">",new Vector2(430,0),new Vector2(75,75),()=>{if(instance!=null)instance.index++;});
        }
    }

    public static void Stop()
    {
        if (instance == null) return;
        if (instance.cam != null && instance.original != null) instance.cam.target = instance.original;
        if (instance.canvas != null) Destroy(instance.canvas);
        Destroy(instance.gameObject);
        instance = null;
    }

    void Update()
    {
        var alive = new List<RemoteAvatar>();
        foreach (var av in FindObjectsByType<RemoteAvatar>(FindObjectsSortMode.None)) if (!av.Down) alive.Add(av);
        alive.Sort((a, b) => a.playerId.CompareTo(b.playerId));
        if (alive.Count == 0)
        {
            label.text = "<color=#D9544D>HAS CAIGUT</color>\n<size=80%>Esperant els companys...</size>";
            return;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0)) index++;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1)) index--;
        index = ((index % alive.Count) + alive.Count) % alive.Count;
        var target = alive[index];
        if (cam != null) cam.target = target.transform;
        string name = NetSession.Instance != null && NetSession.Instance.Players.TryGetValue(target.playerId, out var p) ? p.name : "company";
        label.text = "<color=#D9544D>HAS CAIGUT</color> · Espectant a <color=#EBB347>" + NetSession.Safe(name) + "</color>" +
                     "\n<size=78%>Tornaràs quan netegen la sala o al pis següent" + (alive.Count > 1 ? " · A / D o clic per canviar de company" : "") + "</size>";
    }

    void OnDestroy() { if (instance == this) { if (canvas != null) Destroy(canvas); instance = null; } }
}
