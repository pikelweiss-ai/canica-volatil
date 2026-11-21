using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cooperatiu online: l'AMFITRIO mana i els CLIENTS veuen copies.
//
//  Enemics : nomes l'amfitrio els crea i els mou (IA). Els clients en tenen copies ("proxies") que segueixen
//            les posicions que envia l'amfitrio. Si un client pega a una copia, envia el colp a l'amfitrio.
//  Dany    : cada jugador rep el dany al seu propi PC. Les bales enemigues es dupliquen a cada PC i fan mal
//            al jugador d'eixe PC; els atacs directes (embestida, boss) els envia l'amfitrio ("Hurt").
//  Sales   : l'amfitrio decideix quan comença i s'acaba cada sala (portes) per a tots.
//  Cofres  : el primer que l'obri se l'emporta; per a la resta queda obert.
//  Bales   : cada tret es reprodueix als altres PCs com a bala "cosmetica" (no fa mal, nomes es veu).
//  Caure   : si caus, esperes; quan netegen la sala tornes. Si cauen tots, es torna a començar.
public static class NetCoop
{
    static NetSession S => NetSession.Instance;
    public static bool Online => NetSession.Active && S.LocalId > 0;
    public static bool IsHost => Online && NetSession.IsHost;
    public static bool IsClient => Online && !NetSession.IsHost;

    // ---------- Estat ----------
    static int nextEnemyId = 1;
    static readonly Dictionary<int, EnemyHealth> enemies = new Dictionary<int, EnemyHealth>();
    static readonly Dictionary<string, GameObject> templates = new Dictionary<string, GameObject>(); // enemics de l'escena (client)
    static readonly Dictionary<int, int> roomStates = new Dictionary<int, int>();          // amfitrio: per a qui arriba tard
    static readonly HashSet<int> openedChests = new HashSet<int>();
    static readonly HashSet<int> pendingChests = new HashSet<int>();
    static readonly List<(int rarity, Vector2Int cell)> extraChests = new List<(int, Vector2Int)>();
    static Vector2Int? portalCell;
    static readonly HashSet<int> downed = new HashSet<int>();
    static PlayerHealth downedLocal;
    class PendingSpawn { public int id; public string key; public Vector2 pos; public float scale; public int hp, maxHp; public float until; }
    static readonly List<PendingSpawn> pendingSpawns = new List<PendingSpawn>();
    static Dictionary<string, WeaponData> weapons;
    static float weaponsRefreshed = -10;

    static int Token => NetSession.SceneToken();

    public static void OnSceneLoaded()
    {
        enemies.Clear(); templates.Clear(); roomStates.Clear(); openedChests.Clear(); pendingChests.Clear();
        extraChests.Clear(); portalCell = null; pendingSpawns.Clear(); downedLocal = null;
        NetShared.Reset();
        if (IsHost) downed.Clear();
        // El client avisa que ja ha carregat: l'amfitrio li torna a enviar l'estat de l'escena
        if (IsClient) NetSession.Instance.StartCoroutine(SendReadyLater());
    }

    static IEnumerator SendReadyLater()
    {
        yield return null; yield return null;
        if (IsClient) S.SendToHost(NetProtocol.Frame(Msg.EnemyMark, w => { w.Write(-1); w.Write(Token); })); // -1 = "estic llest"
    }

    static string Key(GameObject go)
    {
        string n = go.name;
        int i = n.IndexOf("(Clone)");
        return (i >= 0 ? n.Substring(0, i) : n).Trim();
    }

    // =====================================================================
    //  ENEMICS
    // =====================================================================
    public static void EnemyStarted(EnemyHealth h)
    {
        if (!Online || h.netProxy) return;
        if (IsHost)
        {
            h.netId = nextEnemyId++;
            enemies[h.netId] = h;
            S.SendAll(SpawnFrame(h));
            return;
        }
        // Client: els enemics nomes els crea l'amfitrio. Els de l'escena es guarden com a motle.
        string key = Key(h.gameObject);
        if (!templates.ContainsKey(key)) { templates[key] = h.gameObject; h.gameObject.SetActive(false); }
        else Object.Destroy(h.gameObject);
    }

    static byte[] SpawnFrame(EnemyHealth h)
    {
        var p = h.transform.position;
        return NetProtocol.Frame(Msg.EnemySpawn, w =>
        {
            w.Write(Token); w.Write(h.netId); w.Write(Key(h.gameObject)); w.Write(p.x); w.Write(p.y);
            w.Write(h.transform.localScale.x); w.Write(h.currentHealth); w.Write(h.maxHealth);
        });
    }

    public static void EnemyDied(EnemyHealth h)
    {
        if (!IsHost || h.netId == 0) return;
        enemies.Remove(h.netId);
        int id = h.netId;
        S.SendAll(NetProtocol.Frame(Msg.EnemyDie, w => w.Write(id)));
    }

    // Client: ha pegat a una copia -> el colp el decideix l'amfitrio
    public static void ProxyHit(EnemyHealth h, int damage, Vector2 knockback, ProjectileEffect effect = ProjectileEffect.None)
    {
        if (!IsClient) return;
        int id = h.netId;
        S.SendToHost(NetProtocol.Frame(Msg.EnemyHitReq, w => { w.Write(id); w.Write(damage); w.Write(knockback.x); w.Write(knockback.y); w.Write((byte)effect); }));
    }

    static GameObject Lookup(string key)
    {
        if (templates.TryGetValue(key, out var t) && t != null) return t;
        var g = DungeonGenerator.Instance;
        if (g == null)
        {
            // Mapa lliure: els enemics del mode Creatiu venen del cataleg
            var cat = Resources.Load<NetContentCatalog>("NetContent");
            if (cat != null && cat.enemies != null) foreach (var p in cat.enemies) if (p != null && Key(p) == key) return p;
            return null;
        }
        bool Match(GameObject p) => p != null && Key(p) == key;
        if (g.enemyPrefabs != null) foreach (var p in g.enemyPrefabs) if (Match(p)) return p;
        if (Match(g.bossPrefab)) return g.bossPrefab;
        if (g.themes != null)
            foreach (var th in g.themes)
            {
                if (th == null) continue;
                if (th.enemies != null) foreach (var p in th.enemies) if (Match(p)) return p;
                if (Match(th.boss)) return th.boss;
                var b = th.boss != null ? th.boss.GetComponent<EnemyBoss>() : null;
                if (b != null && Match(b.minionPrefab)) return b.minionPrefab;
            }
        var gb = g.bossPrefab != null ? g.bossPrefab.GetComponent<EnemyBoss>() : null;
        if (gb != null && Match(gb.minionPrefab)) return gb.minionPrefab;
        return null;
    }

    static bool SpawnProxy(PendingSpawn s)
    {
        if (enemies.ContainsKey(s.id)) return true;
        var source = Lookup(s.key);
        if (source == null) return false;
        var go = Object.Instantiate(source, new Vector3(s.pos.x, s.pos.y, 0), Quaternion.identity);
        go.name = s.key + "(Clone)";
        var h = go.GetComponent<EnemyHealth>();
        if (h == null) { Object.Destroy(go); return true; }
        h.netProxy = true; h.netId = s.id;
        h.maxHealth = s.maxHp; h.currentHealth = s.hp;
        go.transform.localScale = new Vector3(s.scale, s.scale, go.transform.localScale.z);
        // Fora la IA: es mou amb les posicions de l'amfitrio. El dany de contacte es queda (fa mal al jugador d'aquest PC)
        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            if (mb != null && !(mb is EnemyHealth) && !(mb is EnemyDamage) && !(mb is EnemyBoss) && mb.GetType().Name.StartsWith("Enemy")) mb.enabled = false;
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.bodyType = RigidbodyType2D.Kinematic; rb.linearVelocity = Vector2.zero; }
        var proxy = go.AddComponent<NetEnemyProxy>(); proxy.target = s.pos;
        if (!go.activeSelf) go.SetActive(true);
        enemies[s.id] = h;
        Fx.Burst(s.pos, new Color(1f, 0.4f, 0.45f), 6, 2f);
        return true;
    }

    // =====================================================================
    //  JUGADORS: objectius i dany
    // =====================================================================
    public static Transform NearestTarget(Vector2 from)
    {
        Transform best = null; float bestD = float.MaxValue;
        var local = Object.FindFirstObjectByType<PlayerMovement>();
        if (local != null) { best = local.transform; bestD = ((Vector2)local.transform.position - from).sqrMagnitude; }
        foreach (var av in Object.FindObjectsByType<RemoteAvatar>(FindObjectsSortMode.None))
        {
            if (av.Down) continue;
            float d = ((Vector2)av.transform.position - from).sqrMagnitude;
            if (d < bestD) { bestD = d; best = av.transform; }
        }
        return best;
    }

    // Dany directe (embestides, boss...): al jugador local o, si es un company, l'enviem al seu PC
    public static void DamagePlayer(Transform target, int damage)
    {
        if (target == null) return;
        var ph = target.GetComponent<PlayerHealth>();
        if (ph != null) { ph.TakeDamage(damage); return; }
        var av = target.GetComponent<RemoteAvatar>();
        if (av != null && IsHost) S.SendTo(av.playerId, NetProtocol.Frame(Msg.Hurt, w => w.Write(damage)));
    }

    // Explosions amb dany al jugador (kamikazes...): tambe als companys, i tots la veuen
    public static void Exploded(Vector2 pos, float radius, int playerDamage)
    {
        // Les explosions dels jugadors (sense dany al jugador) ja es veuen amb la bala cosmetica
        if (!IsHost || playerDamage <= 0) return;
        S.SendAll(NetProtocol.Frame(Msg.ExplodeFx, w => { w.Write(Token); w.Write(pos.x); w.Write(pos.y); w.Write(radius); }));
        foreach (var av in Object.FindObjectsByType<RemoteAvatar>(FindObjectsSortMode.None))
            if (!av.Down && Vector2.Distance(av.transform.position, pos) <= radius + 0.3f)
            {
                int id = av.playerId;
                S.SendTo(id, NetProtocol.Frame(Msg.Hurt, w => w.Write(playerDamage)));
            }
    }

    public static void EnemyBulletFired(Vector2 pos, Vector2 velocity, int damage, float scale, Color? tint)
    {
        if (!IsHost) return;
        Color c = tint ?? new Color(0, 0, 0, 0);
        S.SendAll(NetProtocol.Frame(Msg.BulletFx, w =>
        {
            w.Write(Token); w.Write(pos.x); w.Write(pos.y); w.Write(velocity.x); w.Write(velocity.y); w.Write(damage); w.Write(scale);
            w.Write(tint.HasValue); w.Write(c.r); w.Write(c.g); w.Write(c.b); w.Write(c.a);
        }));
    }

    public static void SpawnMark(Vector2 pos)
    {
        if (!IsHost) return;
        S.SendAll(NetProtocol.Frame(Msg.EnemyMark, w => { w.Write(Token); w.Write(pos.x); w.Write(pos.y); }));
    }

    // =====================================================================
    //  SALES, COFRES I PORTAL (masmorra)
    // =====================================================================
    public static void EnemySound(string sound, Vector2 position, float volume)
    {
        if (!IsHost) return;
        S.SendAll(NetProtocol.Frame(Msg.MobSound, w => { w.Write(Token); w.Write(sound); w.Write(position.x); w.Write(position.y); w.Write(volume); }));
    }

    public static void RoomChanged(int index, int state)
    {
        if (!IsHost) return;
        roomStates[index] = state;
        S.SendAll(NetProtocol.Frame(Msg.RoomState, w => { w.Write(Token); w.Write(index); w.Write(state); }));
        if (state == 2 && downed.Count > 0) ReviveEveryone();
    }

    public static void ChestSpawned(int rarity, Vector2Int cell)
    {
        if (!IsHost) return;
        extraChests.Add((rarity, cell));
        S.SendAll(NetProtocol.Frame(Msg.ChestSpawn, w => { w.Write(Token); w.Write(rarity); w.Write(cell.x); w.Write(cell.y); }));
    }

    public static void PortalSpawned(Vector2Int cell)
    {
        if (!IsHost) return;
        portalCell = cell;
        S.SendAll(NetProtocol.Frame(Msg.PortalSpawn, w => { w.Write(Token); w.Write(cell.x); w.Write(cell.y); }));
    }

    public static int ChestKey(Chest c)
    {
        var p = c.transform.position;
        return Mathf.RoundToInt(p.x * 4) * 100003 + Mathf.RoundToInt(p.y * 4);
    }

    static Chest FindChest(int key)
    {
        foreach (var c in Object.FindObjectsByType<Chest>(FindObjectsSortMode.None)) if (ChestKey(c) == key) return c;
        return null;
    }

    // El jugador acaba d'obrir un cofre. Retorna true si NO s'ha d'obrir ara (cal esperar l'amfitrio o ja estava agafat)
    public static bool InterceptChestOpen(Chest c)
    {
        if (c == null || !DungeonGenerator.CanInteractAt(c.transform.position)) return true;
        if (!Online) return false;
        int key = ChestKey(c);
        if (IsHost)
        {
            if (openedChests.Contains(key)) { c.NetMarkOpened(); return true; }
            openedChests.Add(key);
            S.SendAll(NetProtocol.Frame(Msg.ChestOpened, w => { w.Write(Token); w.Write(key); }));
            return false;
        }
        pendingChests.Add(key); S.SendToHost(NetProtocol.Frame(Msg.ChestReq, w => { w.Write(Token); w.Write(key); }));
        return true;
    }

    // Portal: tots passen de pis alhora
    public static void RestoreOpenedChest(Chest chest)
    {
        chest.NetMarkOpened();
        if (!IsHost) return;
        int key = ChestKey(chest);
        openedChests.Add(key);
        S.SendAll(NetProtocol.Frame(Msg.ChestOpened, w => { w.Write(Token); w.Write(key); }));
    }

    public static bool InterceptFloorChange(int direction)
    {
        if (!Online) return false;
        if (IsClient)
        {
            S.SendToHost(NetProtocol.Frame(Msg.PortalReq, w => { w.Write(Token); w.Write(direction); }));
            return true;
        }
        int next = RunState.Floor + direction;
        S.SendAll(NetProtocol.Frame(Msg.NextFloorAll, w => { w.Write(Token); w.Write(next); }));
        downed.Clear();
        return false;
    }

    // El jugador local ha caigut
    public static bool InterceptDeath(PlayerHealth local)
    {
        if (!Online || DungeonGenerator.Instance == null) return false;
        downedLocal = local;
        // Les armes i cures queden al terra (les monedes no): tornaràs sense res, al pis següent o en netejar la sala
        local.GetComponent<PlayerAttack>()?.DropAllOnDeath();
        local.GetComponent<HealInventory>()?.DropAllOnDeath();
        Spectate.Begin(local.transform);
        NetToast.Show("Has caigut! Les armes i cures han quedat al terra. Tornaràs quan netegen la sala o al pis següent.", 5f);
        if (IsHost) { downed.Add(S.LocalId); CheckAllDown(); }
        else S.SendToHost(NetProtocol.Frame(Msg.PlayerDown));
        return true;
    }

    // Jugadors que no han caigut (nomes ho sap l'amfitrio)
    public static int PlayerCount => Online ? Mathf.Max(1, S.Players.Count) : 1;
    public static int AliveCount => S == null ? 1 : Mathf.Max(1, S.Players.Count - downed.Count);

    static void CheckAllDown()
    {
        foreach (var p in S.Players.Values) if (!downed.Contains(p.id)) return;
        ForceGameOverAll();
    }

    // Tots han caigut (o una prova ho força): fi de partida i resum per a tots
    public static void ForceGameOverAll()
    {
        if (!IsHost) return;
        downed.Clear();
        S.SendAll(NetProtocol.Frame(Msg.GameOverAll));
        DungeonGenerator.Instance?.NetGameOver();
    }

    // Resum de partida: l'amfitrio tria "tornar a jugar" i tots tornen al pis 1
    public static void RestartRunAll()
    {
        if (!IsHost) { DungeonGenerator.Instance?.RestartRun(); return; }
        downed.Clear();
        S.SendAll(NetProtocol.Frame(Msg.RestartRun));
        DungeonGenerator.Instance?.RestartRun();
    }

    static void ReviveEveryone()
    {
        downed.Clear();
        S.SendAll(NetProtocol.Frame(Msg.ReviveAll));
        ReviveLocal();
    }

    static void ReviveLocal()
    {
        if (downedLocal == null) return;
        Spectate.Stop();
        downedLocal.Revive();
        downedLocal = null;
        NetToast.Show("Has tornat! Ànims!", 2f);
    }

    // =====================================================================
    //  BALES I ATACS DELS JUGADORS (visuals per als altres)
    // =====================================================================
    public static void LocalPellet(WeaponData weapon, Vector2 spawn, Vector2 dir, Vector2 end)
    {
        if (!Online || weapon == null) return;
        int from = S.LocalId;
        S.SendGame(NetProtocol.Frame(Msg.ShotFx, w =>
        {
            w.Write(from); w.Write(Token); w.Write(weapon.name); w.Write(spawn.x); w.Write(spawn.y); w.Write(dir.x); w.Write(dir.y); w.Write(end.x); w.Write(end.y);
        }));
    }

    public static void LocalSwing(WeaponData weapon, Vector2 aim, bool reverse)
    {
        if (!Online || weapon == null) return;
        int from = S.LocalId;
        S.SendGame(NetProtocol.Frame(Msg.SwingFx, w => { w.Write(from); w.Write(Token); w.Write(weapon.name); w.Write(aim.x); w.Write(aim.y); w.Write(reverse); }));
    }

    public static void LocalPunch(Vector2 point)
    {
        if (!Online) return;
        int from = S.LocalId;
        S.SendGame(NetProtocol.Frame(Msg.PunchFx, w => { w.Write(from); w.Write(Token); w.Write(point.x); w.Write(point.y); }));
    }

    public static WeaponData Weapon(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (weapons == null || (!weapons.ContainsKey(name) && Time.unscaledTime - weaponsRefreshed > 2f))
        {
            weapons = new Dictionary<string, WeaponData>();
            var catalog = Resources.Load<NetContentCatalog>("NetContent");
            if (catalog != null && catalog.weapons != null)
                foreach (var wd in catalog.weapons) if (wd != null) weapons[wd.name] = wd;
            foreach (var wd in Resources.FindObjectsOfTypeAll<WeaponData>()) if (wd != null && !weapons.ContainsKey(wd.name)) weapons[wd.name] = wd;
            weaponsRefreshed = Time.unscaledTime;
        }
        return weapons.TryGetValue(name, out var found) ? found : null;
    }

    static RemoteAvatar Avatar(int playerId)
    {
        foreach (var av in Object.FindObjectsByType<RemoteAvatar>(FindObjectsSortMode.None)) if (av.playerId == playerId) return av;
        return null;
    }

    static void PlayShot(int from, string weaponName, Vector2 spawn, Vector2 dir, Vector2 end)
    {
        var weapon = Weapon(weaponName); var av = Avatar(from);
        if (weapon == null || av == null) return;
        var body = av.GetComponent<SpriteRenderer>();
        var p = Projectile.Spawn(spawn, dir, weapon, av.transform, body != null ? body.sortingLayerID : 0, body != null ? body.sortingOrder + 1 : 0, end);
        if (p != null) p.cosmetic = true;
        WeaponSounds.Play(WeaponSounds.ShootClip(weapon), 0.25f);
    }

    static void PlaySwing(int from, string weaponName, Vector2 aim, bool reverse)
    {
        var weapon = Weapon(weaponName); var av = Avatar(from);
        if (weapon == null || av == null) return;
        NetSession.Instance.StartCoroutine(SwingArc(av, weapon, aim, reverse));
    }

    static IEnumerator SwingArc(RemoteAvatar av, WeaponData weapon, Vector2 aim, bool reverse)
    {
        var go = new GameObject("Arc de tall (company)");
        var line = go.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 17; line.startWidth = 0.13f; line.endWidth = 0.03f;
        var body = av.GetComponent<SpriteRenderer>();
        if (body != null) { line.sortingLayerID = body.sortingLayerID; line.sortingOrder = body.sortingOrder + 3; }
        Color color = weapon.projectileColor;
        float arc = weapon.meleeRadial ? 360 : weapon.meleeArc;
        for (float t = 0; t < 0.16f; t += Time.deltaTime)
        {
            if (av == null) break;
            for (int i = 0; i < 17; i++)
            {
                float a = (reverse ? -1 : 1) * arc * (i / 16f - 0.5f) * Mathf.Deg2Rad;
                Vector2 d = new Vector2(aim.x * Mathf.Cos(a) - aim.y * Mathf.Sin(a), aim.x * Mathf.Sin(a) + aim.y * Mathf.Cos(a));
                line.SetPosition(i, av.transform.position + (Vector3)(d * weapon.range));
            }
            color.a = 1f - t / 0.16f; line.startColor = line.endColor = color;
            yield return null;
        }
        Object.Destroy(line.material); Object.Destroy(go);
    }

    public static void SampleLocal(PlayerMovement pm, NetPlayer me)
    {
        if (pm == null) return;
        var health = pm.GetComponent<PlayerHealth>();
        me.health = health != null ? health.currentHealth : 0;
        me.maxHealth = health != null ? health.maxHealth : 0;
        var attack = pm.GetComponent<PlayerAttack>();
        if (attack == null) return;
        var a = attack.AimDirection;
        me.aim = (short)Mathf.RoundToInt(Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg);
        var wd = attack.CurrentWeapon;
        me.weapon = wd != null ? wd.name : "";
        me.down = false;
    }

    // =====================================================================
    //  MISSATGES
    // =====================================================================
    // A l'amfitrio des d'un client
    public static void OnHost(NetPlayer from, Msg type, BinaryReader r)
    {
        if (from == null) return;
        switch (type)
        {
            case Msg.EnemyHitReq:
            {
                int id = r.ReadInt32(); int dmg = r.ReadInt32(); var kb = new Vector2(r.ReadSingle(), r.ReadSingle());
                var effect = (ProjectileEffect)r.ReadByte();
                if (effect > ProjectileEffect.Chain) break;
                if (enemies.TryGetValue(id, out var h) && h != null && !h.IsDead)
                {
                    RunStats.HitBy = ""; // colp d'un company: compta com a mort, pero no per a l'arma de l'amfitrio
                    h.TakeProjectileHit(Mathf.Clamp(dmg, 0, 10000), Vector2.ClampMagnitude(kb, 40f), effect);
                    RunStats.HitBy = null;
                }
                break;
            }
            case Msg.HealReq:
            {
                int token = r.ReadInt32(); int target = r.ReadInt32();
                if (token != Token) break;
                if (target == S.LocalId) HealLocal();
                else S.SendTo(target, NetProtocol.Frame(Msg.HealAlly, w => w.Write(token)));
                break;
            }
            case Msg.ChestReq:
            {
                int token = r.ReadInt32(); int key = r.ReadInt32();
                if (token != Token) break;
                int fid = from.id;
                var c = FindChest(key);
                if (c == null || !DungeonGenerator.CanInteractAt(c.transform.position)) break;
                if (openedChests.Contains(key)) { S.SendTo(fid, NetProtocol.Frame(Msg.ChestOpened, w => { w.Write(token); w.Write(key); })); break; }
                openedChests.Add(key);
                Vector3? opener = from != null && S.Players.TryGetValue(fid, out var opening) ? new Vector3(opening.x, opening.y, 0f) : (Vector3?)null;
                c.OpenAsAuthority(opener);
                S.SendTo(fid, NetProtocol.Frame(Msg.ChestGrant, w => { w.Write(token); w.Write(key); }));
                S.SendAllExcept(fid, NetProtocol.Frame(Msg.ChestOpened, w => { w.Write(token); w.Write(key); }));
                break;
            }
            case Msg.ShotFx:
            {
                r.ReadInt32(); int token = r.ReadInt32(); string wname = r.ReadString();
                var sp = new Vector2(r.ReadSingle(), r.ReadSingle()); var d = new Vector2(r.ReadSingle(), r.ReadSingle()); var e = new Vector2(r.ReadSingle(), r.ReadSingle());
                int fid = from.id;
                S.SendAllExcept(fid, NetProtocol.Frame(Msg.ShotFx, w => { w.Write(fid); w.Write(token); w.Write(wname); w.Write(sp.x); w.Write(sp.y); w.Write(d.x); w.Write(d.y); w.Write(e.x); w.Write(e.y); }));
                if (token == Token) PlayShot(fid, wname, sp, d, e);
                break;
            }
            case Msg.SwingFx:
            {
                r.ReadInt32(); int token = r.ReadInt32(); string wname = r.ReadString(); var a = new Vector2(r.ReadSingle(), r.ReadSingle()); bool rev = r.ReadBoolean();
                int fid = from.id;
                S.SendAllExcept(fid, NetProtocol.Frame(Msg.SwingFx, w => { w.Write(fid); w.Write(token); w.Write(wname); w.Write(a.x); w.Write(a.y); w.Write(rev); }));
                if (token == Token) PlaySwing(fid, wname, a, rev);
                break;
            }
            case Msg.PunchFx:
            {
                r.ReadInt32(); int token = r.ReadInt32(); var pt = new Vector2(r.ReadSingle(), r.ReadSingle());
                int fid = from.id;
                S.SendAllExcept(fid, NetProtocol.Frame(Msg.PunchFx, w => { w.Write(fid); w.Write(token); w.Write(pt.x); w.Write(pt.y); }));
                if (token == Token) Fx.Burst(pt, Color.white, 5, 2f);
                break;
            }
            case Msg.PlayerDown:
                downed.Add(from.id);
                if (DungeonGenerator.Instance != null) CheckAllDown();
                break;
            default: NetShared.OnHost(from, type, r); break;
            case Msg.PortalReq:
            {
                int token = r.ReadInt32();
                int direction = r.ReadInt32();
                if (Token == token && !downed.Contains(from.id) && S.Players.TryGetValue(from.id, out var peer) && peer.scene == token)
                    DungeonGenerator.Instance?.TryTravel(direction, new Vector2(peer.x, peer.y), 1.4f);
                break;
            }
            case Msg.EnemyMark:
            {
                // Un client acaba de carregar l'escena: li enviem tot el que ja ha passat
                int marker = r.ReadInt32(); int token = r.ReadInt32();
                if (marker != -1 || token != Token) break;
                int fid = from.id;
                foreach (var kv in roomStates) { int i = kv.Key, st = kv.Value; S.SendTo(fid, NetProtocol.Frame(Msg.RoomState, w => { w.Write(token); w.Write(i); w.Write(st); })); }
                foreach (var ch in extraChests) { var c = ch; S.SendTo(fid, NetProtocol.Frame(Msg.ChestSpawn, w => { w.Write(token); w.Write(c.rarity); w.Write(c.cell.x); w.Write(c.cell.y); })); }
                foreach (var key in openedChests) { int k = key; S.SendTo(fid, NetProtocol.Frame(Msg.ChestOpened, w => { w.Write(token); w.Write(k); })); }
                if (portalCell.HasValue) { var pc = portalCell.Value; S.SendTo(fid, NetProtocol.Frame(Msg.PortalSpawn, w => { w.Write(token); w.Write(pc.x); w.Write(pc.y); })); }
                foreach (var h in enemies.Values) if (h != null && !h.IsDead) S.SendTo(fid, SpawnFrame(h));
                NetShared.Resync(fid);
                break;
            }
        }
    }


    // ---------- Vareta curativa (v1.3.1) ----------
    public static void HealAlly(int targetId)
    {
        if (S == null || !Online) return;
        if (IsHost)
        {
            if (targetId == S.LocalId) HealLocal();
            else S.SendTo(targetId, NetProtocol.Frame(Msg.HealAlly, w => w.Write(Token)));
        }
        else S.SendToHost(NetProtocol.Frame(Msg.HealReq, w => { w.Write(Token); w.Write(targetId); }));
    }

    static void HealLocal()
    {
        var ph = Object.FindFirstObjectByType<PlayerHealth>();
        if (ph == null || ph.IsFull) return;
        ph.Heal(1);
        Fx.Text(ph.transform.position, "+1", new Color(0.45f, 1f, 0.45f));
        Fx.Burst(ph.transform.position, new Color(0.4f, 1f, 0.5f), 8, 2.5f);
    }
    // Al client des de l'amfitrio
    public static void ChainJump(Vector2 from, Vector2 to)
    {
        DrawChainJump(from, to);
        if (IsHost) S.SendAll(NetProtocol.Frame(Msg.ChainFx, w =>
        { w.Write(Token); w.Write(from.x); w.Write(from.y); w.Write(to.x); w.Write(to.y); }));
    }

    static void DrawChainJump(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        var beam = Fx.MakeSprite("Raig de polsos", (from + to) * 0.5f, Fx.Disc, new Color(0.3f, 0.72f, 1f), "Enemy", 2);
        beam.transform.localScale = new Vector3(delta.magnitude, 0.09f, 1f);
        beam.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        Object.Destroy(beam.gameObject, 0.15f);
    }

    public static void OnClient(Msg type, BinaryReader r)
    {
        var gen = DungeonGenerator.Instance;
        switch (type)
        {
            case Msg.MobSound:
            {
                int token = r.ReadInt32(); string sound = r.ReadString();
                var position = new Vector2(r.ReadSingle(), r.ReadSingle()); float volume = r.ReadSingle();
                if (token == Token) MobSounds.PlayLocal(sound, position, volume);
                break;
            }
            case Msg.EnemySpawn:
            {
                int token = r.ReadInt32();
                var s = new PendingSpawn { id = r.ReadInt32(), key = r.ReadString(), pos = new Vector2(r.ReadSingle(), r.ReadSingle()), scale = r.ReadSingle(), hp = r.ReadInt32(), maxHp = r.ReadInt32(), until = Time.unscaledTime + 4f };
                if (token != Token) break;
                if (!SpawnProxy(s)) pendingSpawns.Add(s); // l'escena encara s'esta preparant: ho tornem a provar
                break;
            }
            case Msg.EnemySnap:
            {
                int token = r.ReadInt32(); int n = r.ReadInt16();
                for (int i = 0; i < n; i++)
                {
                    int id = r.ReadInt32(); float x = r.ReadSingle(), y = r.ReadSingle(); int hp = r.ReadInt32(); bool flip = r.ReadBoolean();
                    var col = new Color32(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()); float scale = r.ReadSingle();
                    if (token != Token || !enemies.TryGetValue(id, out var h) || h == null) continue;
                    var proxy = h.GetComponent<NetEnemyProxy>();
                    if (proxy != null) { proxy.target = new Vector2(x, y); proxy.flip = flip; proxy.scale = scale; if (!proxy.color.Equals(col)) { proxy.color = col; var psr = h.GetComponent<SpriteRenderer>(); if (psr != null) psr.color = col; } }
                    h.currentHealth = hp;
                }
                break;
            }
            case Msg.EnemyDie:
            {
                int id = r.ReadInt32();
                if (enemies.TryGetValue(id, out var h)) { enemies.Remove(id); if (h != null) h.NetDie(); }
                break;
            }
            case Msg.EnemyMark:
            {
                int token = r.ReadInt32(); var p = new Vector2(r.ReadSingle(), r.ReadSingle());
                if (token == Token) NetSession.Instance.StartCoroutine(Mark(p));
                break;
            }
            case Msg.RoomState:
            {
                int token = r.ReadInt32(); int index = r.ReadInt32(); int state = r.ReadInt32();
                if (token == Token && gen != null) { gen.NetRoomState(index, state); RoomEvents++; }
                break;
            }
            case Msg.ChestSpawn:
            {
                int token = r.ReadInt32(); int rarity = r.ReadInt32(); var cell = new Vector2Int(r.ReadInt32(), r.ReadInt32());
                if (token == Token && gen != null) gen.NetSpawnChest(rarity, cell);
                else if (token == Token) CreativeMode.CreateChest(rarity, cell); // mapa lliure: cofre del Creatiu (cel·la = posicio x4)
                break;
            }
            case Msg.ChestOpened:
            {
                int token = r.ReadInt32(); int key = r.ReadInt32();
                if (token != Token) break;
                pendingChests.Remove(key);
                openedChests.Add(key);
                var c = FindChest(key); if (c != null) c.NetMarkOpened();
                break;
            }
            case Msg.ChestGrant:
            {
                int token = r.ReadInt32(); int key = r.ReadInt32();
                if (token != Token) break;
                pendingChests.Remove(key);
                if (!openedChests.Add(key)) break;
                var c = FindChest(key); if (c != null) c.NetGrantOpen(dropLoot: false);
                break;
            }
            case Msg.HealAlly:
            {
                int token = r.ReadInt32();
                if (token == Token) HealLocal();
                break;
            }
            case Msg.ChainFx:
            {
                int token = r.ReadInt32();
                Vector2 start = new Vector2(r.ReadSingle(), r.ReadSingle());
                Vector2 end = new Vector2(r.ReadSingle(), r.ReadSingle());
                if (token == Token) DrawChainJump(start, end);
                break;
            }
            case Msg.PortalSpawn:
            {
                int token = r.ReadInt32(); var cell = new Vector2Int(r.ReadInt32(), r.ReadInt32());
                if (token == Token && gen != null) gen.NetSpawnPortal(cell);
                break;
            }
            case Msg.Hurt:
            {
                int dmg = r.ReadInt32();
                var ph = Object.FindFirstObjectByType<PlayerHealth>();
                if (ph != null) ph.TakeDamage(dmg);
                HurtsReceived++;
                break;
            }
            case Msg.NextFloorAll:
            {
                int token = r.ReadInt32();
                int floor = r.ReadInt32();
                if (token == Token && gen != null) gen.NetNextFloor(floor);
                break;
            }
            case Msg.GameOverAll: if (gen != null) gen.NetGameOver(); break;
            case Msg.RestartRun: if (gen != null) gen.RestartRun(); break;
            case Msg.ReviveAll: ReviveLocal(); break;
            case Msg.BulletFx:
            {
                int token = r.ReadInt32(); var p = new Vector2(r.ReadSingle(), r.ReadSingle()); var v = new Vector2(r.ReadSingle(), r.ReadSingle());
                int dmg = r.ReadInt32(); float scale = r.ReadSingle(); bool hasTint = r.ReadBoolean(); var c = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                if (token == Token) EnemyBullet.Fire(p, v, v.magnitude, dmg, null, scale, hasTint ? c : (Color?)null);
                break;
            }
            case Msg.ExplodeFx:
            {
                int token = r.ReadInt32(); var p = new Vector2(r.ReadSingle(), r.ReadSingle()); float radius = r.ReadSingle();
                if (token == Token) Fx.ExplodeVisual(p, radius);
                break;
            }
            case Msg.ShotFx:
            {
                int from = r.ReadInt32(); int token = r.ReadInt32(); string wname = r.ReadString();
                var sp = new Vector2(r.ReadSingle(), r.ReadSingle()); var d = new Vector2(r.ReadSingle(), r.ReadSingle()); var e = new Vector2(r.ReadSingle(), r.ReadSingle());
                if (token == Token) PlayShot(from, wname, sp, d, e);
                break;
            }
            case Msg.SwingFx:
            {
                int from = r.ReadInt32(); int token = r.ReadInt32(); string wname = r.ReadString(); var a = new Vector2(r.ReadSingle(), r.ReadSingle()); bool rev = r.ReadBoolean();
                if (token == Token) PlaySwing(from, wname, a, rev);
                break;
            }
            default: NetShared.OnClient(type, r); break;
            case Msg.PunchFx:
            {
                r.ReadInt32(); int token = r.ReadInt32(); var pt = new Vector2(r.ReadSingle(), r.ReadSingle());
                if (token == Token) Fx.Burst(pt, Color.white, 5, 2f);
                break;
            }
        }
    }

    static IEnumerator Mark(Vector2 pos)
    {
        SpriteRenderer mark = Fx.MakeSprite("Avis enemic", pos, Fx.Disc, new Color(1f, 0.25f, 0.3f, 0.5f), "Shadows", 5);
        float t = 0f;
        while (t < 0.7f && mark != null)
        {
            mark.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 0.9f, t / 0.7f);
            t += Time.deltaTime;
            yield return null;
        }
        if (mark != null) Object.Destroy(mark.gameObject);
    }

    // =====================================================================
    //  CADA FOTOGRAMA (el crida NetCoopRunner)
    // =====================================================================
    static float nextSnap;
    public static void Tick()
    {
        NetShared.Tick();
        TickCoop();
    }

    static float downSince = -1;
    static void TickCoop()
    {
        // Amfitrio: si algu ha caigut fora de combat (TNT, punxes...), torna als 4 segons
        if (IsHost && downed.Count > 0)
        {
            var g = DungeonGenerator.Instance;
            if (downSince < 0) downSince = Time.unscaledTime;
            if (g != null && !g.AnyRoomFighting && Time.unscaledTime - downSince > 4f) ReviveEveryone();
        }
        else downSince = -1;

        if (!Online) return;
        if (IsClient && pendingSpawns.Count > 0)
            for (int i = pendingSpawns.Count - 1; i >= 0; i--)
                if (SpawnProxy(pendingSpawns[i]) || Time.unscaledTime > pendingSpawns[i].until) pendingSpawns.RemoveAt(i);

        if (IsHost && Time.unscaledTime >= nextSnap)
        {
            nextSnap = Time.unscaledTime + 1f / 15f;
            var alive = new List<EnemyHealth>();
            var dead = new List<int>();
            foreach (var kv in enemies) { if (kv.Value == null) dead.Add(kv.Key); else if (!kv.Value.IsDead) alive.Add(kv.Value); }
            foreach (var id in dead) enemies.Remove(id);
            if (alive.Count == 0) return;
            int token = Token;
            S.SendAll(NetProtocol.Frame(Msg.EnemySnap, w =>
            {
                w.Write(token); w.Write((short)alive.Count);
                foreach (var h in alive)
                {
                    var p = h.transform.position; var sr = h.GetComponent<SpriteRenderer>();
                    w.Write(h.netId); w.Write(p.x); w.Write(p.y); w.Write(h.currentHealth); w.Write(sr != null && sr.flipX);
                    Color32 col = sr != null ? (Color32)sr.color : new Color32(255, 255, 255, 255);
                    w.Write(col.r); w.Write(col.g); w.Write(col.b); w.Write(col.a); w.Write(h.transform.localScale.x);
                }
            }));
        }
    }

    // Per a les proves automatiques
    public static int EnemyCount { get { int n = 0; foreach (var h in enemies.Values) if (h != null && !h.IsDead) n++; return n; } }
    public static IEnumerable<EnemyHealth> Enemies => enemies.Values;
    public static EnemyHealth FindEnemy(int id) => enemies.TryGetValue(id, out var enemy) ? enemy : null;
    public static bool IsChestOpenedForAll(int key) => openedChests.Contains(key);
    public static int RoomEvents, HurtsReceived;
    public static int DownedCount => downed.Count;
    public static bool IsDown(int playerId) => downed.Contains(playerId);
}

public class NetCoopRunner : MonoBehaviour
{
    void Awake() { SceneManager.sceneLoaded += (s, m) => NetCoop.OnSceneLoaded(); }
    void Update() { NetCoop.Tick(); }
}

// Copia d'un enemic al PC d'un client: llisca cap a la posicio que diu l'amfitrio.
public class NetEnemyProxy : MonoBehaviour
{
    public Vector2 target;
    public bool flip;
    public Color32 color = new Color32(255, 255, 255, 255);
    public float scale = -1;
    SpriteRenderer sr;
    Rigidbody2D rb;
    void Start() { sr = GetComponent<SpriteRenderer>(); rb = GetComponent<Rigidbody2D>(); }
    void Update()
    {
        Vector2 p = transform.position;
        Vector2 next = (target - p).sqrMagnitude > 9f ? target : Vector2.Lerp(p, target, 1f - Mathf.Exp(-15f * Time.deltaTime));
        if (rb != null) rb.position = next;
        transform.position = new Vector3(next.x, next.y, transform.position.z);
        if (sr != null) sr.flipX = flip;
        if (scale > 0) transform.localScale = new Vector3(scale, Mathf.Abs(scale) * Mathf.Sign(transform.localScale.y), transform.localScale.z);
    }
}
