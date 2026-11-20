using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Bot de proves de les sales online. No fa res si no s'arranca amb -canicaBot.
// Exemples (cada un en una finestra/proces diferent, sense grafics):
//   Joc.exe -batchmode -nographics -canicaBot host -botNet lan -botPassword abc -botExpect 2 -botCodeFile codi.txt
//   Joc.exe -batchmode -nographics -canicaBot join -botJoin lan-list -botPassword abc
//   Joc.exe -batchmode -nographics -canicaBot join -botJoin code-file:codi.txt -botPassword abc
//   (Internet) afegir -botServer 127.0.0.1:7780 i -botNet internet / -botJoin internet-list
// Escriu "NETBOT OK: ..." per cada pas i "NETBOT PASS" o "NETBOT FAIL: ..." al final (codi d'eixida 0/1).
public partial class NetBot : MonoBehaviour
{
    string role, net, join, password, codeFile, mode, botName, shots, roomName;
    int maxPlayers = 4;
    bool coop;
    int expect;
    bool failed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void FromCommandLine()
    {
        var a = Environment.GetCommandLineArgs();
        string Arg(string key, string def) { int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : def; }
        string role = Arg("-canicaBot", null);
        if (role == null) return;
        RunStats.DontSave = true; // Les proves no guarden records al PC
        var go = new GameObject("NetBot");
        DontDestroyOnLoad(go);
        var bot = go.AddComponent<NetBot>();
        bot.role = role;
        bot.net = Arg("-botNet", "lan");
        bot.join = Arg("-botJoin", "lan-list");
        bot.password = Arg("-botPassword", "");
        bot.codeFile = Arg("-botCodeFile", null);
        bot.mode = Arg("-botMode", "free");
        bot.expect = int.Parse(Arg("-botExpect", "2"));
        bot.shots = Arg("-botShots", null);
        bot.coop = Array.IndexOf(a, "-botCoop") >= 0;
        bot.roomName = Arg("-botRoom", "BotRoom");
        bot.maxPlayers = int.Parse(Arg("-botMax", "4"));
        bot.botName = Arg("-botName", role == "host" ? "BotAmfitrio" : "BotJugador");
        NetSession.NameOverride = bot.botName;
        NetSession.ServerOverride = Arg("-botServer", "");
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        AudioListener.volume = 0;
    }

    void Start()
    {
        StartCoroutine(role == "solo" ? SoloCombat() : role == "weaponshots" ? WeaponShots() : role == "trails" ? TrailGallery() : role == "host" ? Host() : Join());
        StartCoroutine(Watchdog());
    }

    static void Log(string s) => Debug.Log("NETBOT " + s);
    void Ok(string s) => Log("OK: " + s);
    void Fail(string s) { if (failed) return; failed = true; Log("FAIL: " + s); Application.Quit(1); }
    void Pass() { Log("PASS"); Application.Quit(0); }

    IEnumerator Watchdog() { yield return new WaitForSecondsRealtime(240); Fail("temps esgotat (240 s)"); }

    IEnumerator WaitFor(Func<bool> cond, float seconds, string what)
    {
        float until = Time.realtimeSinceStartup + seconds;
        while (!cond())
        {
            if (Time.realtimeSinceStartup > until) { Fail("esperant: " + what); yield break; }
            yield return null;
        }
        Ok(what);
    }

    static string Scene => SceneManager.GetActiveScene().name;
    static int Avatars => FindObjectsByType<RemoteAvatar>(FindObjectsSortMode.None).Length;

    // ------------------------------------------------------------------
    IEnumerator Host()
    {
        yield return WaitFor(() => Scene == "MainMenu", 20, "menu principal carregat");
        string error = "pending";
        var settings = new NetSession.HostSettings { name = roomName, mode = mode == "dungeon" ? RoomMode.Dungeon : RoomMode.FreeMap, maxPlayers = maxPlayers, isPublic = true, password = password, internet = net == "internet" };
        NetSession.HostRoom(settings, e => error = e);
        yield return WaitFor(() => error != "pending", 15, "sala creada");
        if (failed) yield break;
        if (error != null) { Fail("no s'ha pogut crear: " + error); yield break; }
        var s = NetSession.Instance;
        Ok("codi " + s.Room.code + " · " + s.NetworkLabel);
        if (s.IsCloudRoom) { yield return WaitFor(() => s.CloudStatus != null && s.CloudStatus.StartsWith("A Internet"), 20, "sala anunciada a Internet sense IP"); if (failed) yield break; }
        if (codeFile != null) File.WriteAllText(codeFile, s.Room.code);
        yield return WaitFor(() => Scene == "Lobby", 10, "lobby carregat");

        bool gotChat = false;
        s.ChatReceived += l => { if (!l.system && l.text.Contains("hola")) gotChat = true; };
        yield return WaitFor(() => s.Players.Count >= expect, 60, expect + " jugadors a la sala");
        if (coop) yield return WaitFor(() => NetShared.MapTakenCount > 0, 30, "moneda del lobby agafada pel company: ja no es pot agafar ací");
        yield return WaitFor(() => gotChat, 20, "xat rebut del jugador");
        yield return WaitFor(() => s.EveryoneReady, 20, "tots llestos");
        yield return WaitFor(() => Avatars >= expect - 1, 15, "avatar remot visible al lobby");
        if (failed) yield break;
        yield return Shot("4-lobby-amfitrio");
        if (failed) yield break;
        s.SetMode(settings.mode);
        s.StartCountdown();
        yield return WaitFor(() => s.Room.phase == RoomPhase.Countdown, 2, "compte enrere");
        if (shots != null) { yield return new WaitForSecondsRealtime(1.2f); yield return Shot("5-compte-enrere"); }
        string target = RoomModes.Scene(settings.mode);
        yield return WaitFor(() => Scene == target, 10, "partida carregada (" + target + ")");
        yield return null; Ok("llavor " + s.Seed + " · " + NetBotLayout.Fingerprint());
        yield return WaitFor(() => Avatars >= expect - 1, 20, "avatar remot visible en partida");
        if (coop && Scene == "Dungeon") { yield return HostCoop(); if (failed) yield break; }
        if (Scene == "GameScene") { yield return CheckFreeSky(); if (failed) yield break; }
        yield return new WaitForSecondsRealtime(0.5f);
        yield return Shot("6-partida-amfitrio");
        if (failed) yield break;
        yield return new WaitForSecondsRealtime(2);
        s.BackToLobby();
        yield return WaitFor(() => Scene == "Lobby" && s.Room.phase == RoomPhase.Lobby, 10, "tornada al lobby");
        yield return new WaitForSecondsRealtime(3);
        if (failed) yield break;
        s.Leave(null);
        Ok("sala tancada");
        yield return new WaitForSecondsRealtime(1);
        Pass();
    }

    // ------------------------------------------------------------------
    IEnumerator Join()
    {
        yield return WaitFor(() => Scene == "MainMenu", 20, "menu principal carregat");
        RoomInfo target = null;

        if (join == "lan-list" || join == "internet-list")
        {
            float until = Time.realtimeSinceStartup + 30;
            while (target == null && Time.realtimeSinceStartup < until)
            {
                bool done = false; string err = null;
                if (join == "lan-list") NetSession.ListLan(list => { foreach (var r in list) if (r.name == "BotRoom") target = r; done = true; });
                else NetSession.ListInternet((list, e) => { err = e; foreach (var r in list) if (r.name == "BotRoom") target = r; done = true; });
                while (!done) yield return null;
                if (err != null) Log("llista: " + err);
                if (target == null) yield return new WaitForSecondsRealtime(1);
            }
            if (target == null) { Fail("no apareix BotRoom a la llista " + join); yield break; }
            if (shots != null)
            {
                RoomBrowserUI.Open(); yield return new WaitForSecondsRealtime(3f);
                RoomBrowserUI.DebugShow("select", null); yield return Shot("1-navegador");
                RoomBrowserUI.DebugShow("password", target); yield return Shot("2b-contrasenya");
                RoomBrowserUI.DebugShow("create", null); yield return Shot("2-crear-sala");
                RoomBrowserUI.DebugShow("close", null);
            }
            Ok("sala trobada a la llista: " + target.code + " (" + target.players + "/" + target.maxPlayers + ", contrasenya=" + target.hasPassword + ", ping " + target.pingMs + " ms)");
        }

        NetSession.JoinResult res = null;
        if (target == null)
        {
            // Per codi (o IP), sense contrasenya primer: ha de demanar-la
            string code = join;
            if (join.StartsWith("code-file:"))
            {
                string path = join.Substring(10);
                yield return WaitFor(() => File.Exists(path) && File.ReadAllText(path).Trim().Length == 6, 30, "codi publicat per l'amfitrio");
                if (failed) yield break;
                code = File.ReadAllText(path).Trim();
            }
            NetSession.JoinByCode(code, "", r => res = r);
        }
        else NetSession.Join(target, "", r => res = r);
        yield return WaitFor(() => res != null, 15, "resposta de la sala");
        if (failed) yield break;

        if (!string.IsNullOrEmpty(password))
        {
            if (!res.needPassword) { Fail("la sala hauria de demanar contrasenya: " + (res.ok ? "ha deixat entrar" : res.error)); yield break; }
            Ok("demana contrasenya: " + res.error);
            var again = res.target;
            res = null;
            NetSession.Join(again, "dolenta", r => res = r);
            yield return WaitFor(() => res != null, 15, "resposta amb contrasenya dolenta");
            if (failed) yield break;
            if (res.ok || !res.needPassword) { Fail("contrasenya dolenta acceptada o error inesperat: " + res.error); yield break; }
            Ok("rebutja contrasenya dolenta: " + res.error);
            res = null;
            NetSession.Join(again, password, r => res = r);
            yield return WaitFor(() => res != null, 15, "resposta amb contrasenya bona");
            if (failed) yield break;
        }
        if (!res.ok) { Fail("no ha pogut entrar: " + res.error); yield break; }
        var s = NetSession.Instance;
        yield return WaitFor(() => Scene == "Lobby" && s.Players.Count >= 2, 10, "dins del lobby amb l'amfitrio");
        yield return WaitFor(() => Avatars >= 1, 10, "avatar de l'amfitrio visible al lobby");
        if (failed) yield break;
        if (coop)
        {
            Coin mapCoin = null; float bestc = float.MaxValue; var me = FindFirstObjectByType<PlayerMovement>();
            foreach (var mc in FindObjectsByType<Coin>(FindObjectsSortMode.None)) { float d = Vector2.Distance(mc.transform.position, me.transform.position); if (d < bestc) { bestc = d; mapCoin = mc; } }
            if (mapCoin != null) { var mrb = me.GetComponent<Rigidbody2D>(); me.transform.position = mapCoin.transform.position; if (mrb != null) mrb.position = mapCoin.transform.position; }
            yield return WaitFor(() => NetShared.MapCoinsGranted > 0, 10, "moneda del lobby agafada (concedida per l'amfitrio)");
            if (failed) yield break;
        }
        s.SendChat("hola des del bot");
        yield return new WaitForSecondsRealtime(0.5f);
        s.SetReady(true);
        if (shots != null) { yield return new WaitForSecondsRealtime(0.4f); yield return Shot("3-lobby-jugador"); }
        bool closed = false;
        yield return WaitFor(() => Scene == "GameScene" || Scene == "Dungeon", 30, "partida començada per l'amfitrio");
        yield return null; Ok("llavor " + s.Seed + " · " + NetBotLayout.Fingerprint());
        if (coop && Scene == "Dungeon") { yield return JoinCoop(); if (failed) yield break; }
        yield return WaitFor(() => Avatars >= 1, 20, "avatar de l'amfitrio visible en partida");
        yield return WaitFor(() => Scene == "Lobby", 20, "tornada al lobby");
        yield return WaitFor(() => { closed = !NetSession.Active; return closed && Scene == "MainMenu"; }, 20, "sala tancada per l'amfitrio i tornada al menu");
        if (failed) yield break;
        Pass();
    }

    // ---------- Proves del cooperatiu a la masmorra (-botCoop) ----------
    static DungeonGenerator.Room FirstCombat(DungeonGenerator g) => g.Rooms.Find(r => r.type == DungeonGenerator.RoomType.Combat && r.links.Count > 1);

    bool CheckWaitingDoors(DungeonGenerator g, DungeonGenerator.Room room)
    {
        int open = 0, closed = 0;
        foreach (var cell in room.doorCells)
        {
            bool expectedClosed = DungeonGenerator.DoorSide(room, cell) != room.waitingEntrance;
            if (g.doors.HasTile(cell) != expectedClosed) { Fail("porta en espera incorrecta: " + cell); return false; }
            if (expectedClosed) closed++; else open++;
        }
        if (room.waitingEntrance < 0 || open != 3 || closed < 3) { Fail("cal una entrada oberta i les altres portes tancades"); return false; }
        Ok("portes en espera: nomes les 3 caselles de l'entrada obertes");
        return true;
    }

    static void Teleport(DungeonGenerator g, DungeonGenerator.Room room)
    {
        Vector3 c = g.ground.GetCellCenterWorld(new Vector3Int(room.Center.x, room.Center.y, 0));
        var rb = g.player.GetComponent<Rigidbody2D>();
        g.player.position = new Vector3(c.x, c.y, g.player.position.z); if (rb != null) rb.position = c;
    }

    bool keepAlive;

    void SpawnTestCrate(DungeonGenerator g)
    {
        var combat = FirstCombat(g);
        if (combat == null) return;
        var center = new Vector3Int(combat.Center.x, combat.Center.y, 0);
        Vector3 position = g.ground.GetCellCenterWorld(center + Vector3Int.up * 2);
        var fixture = Instantiate(g.cratePrefab, position, Quaternion.identity);
        fixture.name = "Caixa de prova en combat";
        var crate = fixture.GetComponent<Breakable>();
        crate.health = 1000;
        crate.coinChance = 1f;
        crate.healChance = 0f;
    }

    IEnumerator CheckProjectiles(DungeonGenerator g)
    {
        yield return WaitFor(()=>FindFirstObjectByType<RemoteAvatar>()!=null,15,"company carregat abans de provar projectils");
        if(failed)yield break;
        var weapon = ScriptableObject.CreateInstance<WeaponData>();
        weapon.weaponName="Prova de pas"; weapon.projectileSpeed=4; weapon.range=5; weapon.projectileSize=.12f;
        var center = g.player.position;
        var avatar = FindFirstObjectByType<RemoteAvatar>();
        foreach(bool cosmetic in new[] { false, true })
        {
            var shot = Projectile.Spawn(center-Vector3.right,Vector2.right,weapon,avatar.transform,0,0,center+Vector3.right*4);
            shot.cosmetic=cosmetic;
            yield return WaitFor(()=>shot==null || shot.transform.position.x>center.x+.8f,3,"tret travessant jugador local");
            if(shot==null) { Fail("el tret s'ha aturat al company (cosmetic="+cosmetic+")"); Destroy(weapon); yield break; }
            Destroy(shot.gameObject);
        }
        Destroy(weapon);
        Ok("tret real i cosmetic travessen el company");
    }

    bool CheckRoomLocked(DungeonGenerator g, DungeonGenerator.Room room)
    {
        var pos=g.ground.GetCellCenterWorld(new Vector3Int(room.Center.x,room.Center.y,0));
        if(DungeonGenerator.CanInteractAt(pos)) { Fail("la sala no netejada permet interactuar"); return false; }
        var chest=FindFirstObjectByType<Chest>(); var old=chest.transform.position; chest.transform.position=pos;
        bool stopped=NetCoop.InterceptChestOpen(chest); chest.transform.position=old;
        if(!stopped || chest.IsOpen) { Fail("s'ha pogut obrir cofre en sala bloquejada"); return false; }
        if(DungeonGenerator.CanDamageBreakableAt(pos)!=room.active) { Fail("les caixes han d'estar protegides en espera i disponibles en combat"); return false; }
        if(NetCoop.IsHost)
        {
            // Una caixa de la sala podria estar trencada i pendent de Destroy al final del frame.
            var probe = new GameObject("Prova de dany a caixes");
            probe.transform.position = pos;
            var crate = probe.AddComponent<Breakable>();
            foreach (bool usesExplosive in new[] { false, true })
            {
                crate.explodes = usesExplosive;
                crate.health = 10;
                crate.Hit(1);
                if (crate.health != (room.active ? 9 : 10))
                {
                    Destroy(probe);
                    Fail("colp a caixa o TNT incorrecte en espera/combat");
                    return false;
                }
            }
            Destroy(probe);
        }
        Ok(room.active ? "caixa i TNT reben colps durant el combat" : "caixa i TNT protegides mentre falten companys");
        Ok("cofre bloquejat en sala pendent/combat"); return true;
    }
    IEnumerator KeepAlive(PlayerHealth h) { while (keepAlive) { if (h != null && h.gameObject.activeInHierarchy) h.Heal(5); yield return new WaitForSecondsRealtime(0.3f); } }

    // Arranca la mateixa rutina de generació que usa una sala de boss, lluny dels jugadors.
    IEnumerator SpawnTestBoss(DungeonGenerator dungeon, GameObject prefab, Action<EnemyHealth> spawned)
    {
        var method = typeof(DungeonGenerator).GetMethod("SpawnEnemy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var room = new DungeonGenerator.Room { type = DungeonGenerator.RoomType.Boss };
        yield return (IEnumerator)method.Invoke(dungeon, new object[] { prefab, new Vector2Int(1100, 1100), room, 1.2f });
        spawned(room.alive.Count == 1 ? room.alive[0] : null);
    }

    static SpriteRenderer MakeBossWarning(EnemyHealth health, Vector2 position)
    {
        var boss = health.GetComponent<EnemyBoss>();
        var markAttack = typeof(EnemyBoss).GetMethod("MarkAttack",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return (SpriteRenderer)markAttack.Invoke(boss, new object[] { "Avís de prova del boss", position, 2.4f, 10f });
    }

    IEnumerator CheckTrailReset()
    {
        var fixture = new GameObject("Prova d'estela", typeof(SpriteRenderer));
        fixture.transform.position = new Vector3(600, 600);
        var emitter = fixture.AddComponent<TrailParticleEmitter>();
        emitter.Init(TrailShape.Zigzag, Color.cyan);
        yield return null;

        fixture.transform.position += Vector3.right * .15f;
        yield return null;
        yield return null;

        var particle = System.Array.Find(FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None),
            sprite => sprite.name == "Particula" && sprite.transform.position.x > 600.1f && sprite.transform.position.x < 600.2f);
        var root = particle != null ? particle.transform.parent : null;
        bool drawn = particle != null;
        bool behindSkin = drawn && SortingLayer.GetLayerValueFromID(particle.sortingLayerID) <
            SortingLayer.GetLayerValueFromName("Player");
        fixture.transform.position += Vector3.right * 10f;
        yield return null;
        yield return null;
        bool cleared = root != null && root.GetComponentsInChildren<SpriteRenderer>().Length == 0;
        Destroy(fixture);
        if (!drawn || !cleared) Fail("l'estela deixa una marca fora del punt o no es neteja al teletransport");
        else if (!behindSkin) Fail("l'estela es dibuixa davant de la skin");
        else Ok("estela mostrejada per distància i neta després d'un teletransport");
    }

    // v1.3.1: amb dos jugadors, la sala inicial té dos cofres comuns (un per jugador)
    bool CheckStartChests(DungeonGenerator g, int players)
    {
        var start = g.Rooms.Find(r => r.type == DungeonGenerator.RoomType.Start);
        if (start == null) { Fail("no hi ha sala inicial"); return false; }
        int common = 0;
        foreach (var chest in FindObjectsByType<Chest>(FindObjectsSortMode.None))
        {
            var cell = g.ground.WorldToCell(chest.transform.position);
            if (start.rect.Contains(new Vector2Int(cell.x, cell.y)) && chest.rarity == ChestRarity.Comu) common++;
        }
        if (common < players) { Fail("la sala inicial té " + common + " cofres comuns i hi ha " + players + " jugadors"); return false; }
        Ok("sala inicial amb " + common + " cofres comuns per a " + players + " jugadors");
        return true;
    }

    IEnumerator HostCoop()
    {
        var g = DungeonGenerator.Instance;
        if (g == null) { Fail("no hi ha masmorra"); yield break; }
        keepAlive = true; StartCoroutine(KeepAlive(g.player.GetComponent<PlayerHealth>()));
        if (!CheckStartChests(g, 2)) yield break;
        SpawnTestCrate(g);
        yield return CheckProjectiles(g); if(failed)yield break;
        int remoteKills = 0;
        System.Action<EnemyHealth> onDied = h => { if (h.netId != 0) remoteKills++; };
        EnemyHealth.AnyDied += onDied;
        var combat = FirstCombat(g);
        if (combat == null) { Fail("cap sala de combat"); yield break; }
        // Receptor en fred: totes les cures, lluny dels jugadors per no recollir-les.
        int n = 0;
        foreach (var heal in HealItem.All)
        {
            var p = new Vector3(1000 + n++ * 2, 1000);
            HealPickup.Spawn(heal, p, p);
        }
        // 1) L'amfitrio entra sol: la sala NO s'ha de tancar fins que entre el company
        Teleport(g, combat);
        yield return new WaitForSecondsRealtime(3f);
        if (combat.active || NetCoop.EnemyCount > 0) { Fail("la sala s'ha tancat sense el company"); yield break; }
        if (!CheckWaitingDoors(g, combat)) yield break;
        if (!CheckRoomLocked(g, combat)) yield break;
        Ok("la sala espera que entre el company");
        yield return WaitFor(() => NetCoop.EnemyCount > 0, 25, "sala començada quan han entrat tots dos");
        if (failed) yield break;
        Ok("enemics vius: " + NetCoop.EnemyCount);
        if (!CheckRoomLocked(g, combat)) yield break;
        if (shots != null) yield return new WaitForSecondsRealtime(1f);
        yield return Shot("7-masmorra-amfitrio");
        foreach (var chest in FindObjectsByType<Chest>(FindObjectsSortMode.None)) chest.healChance = 1f;

        // 2) El client en mata un (via colp a la seua copia)
        yield return WaitFor(() => remoteKills > 0, 25, "un enemic ha mort pel colp del company");
        // 3) Bales del company visibles
        yield return WaitFor(() => { foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) if (p.cosmetic) return true; return false; }, 25, "bala del company visible (cosmetica, sense dany)");
        // 4) Cofre: l'obri el company i per a l'amfitrio ja queda obert
        Chest opened = null;
        yield return WaitFor(() => { foreach (var ch in FindObjectsByType<Chest>(FindObjectsSortMode.None)) if (ch.IsOpen && NetCoop.IsChestOpenedForAll(NetCoop.ChestKey(ch))) { opened = ch; return true; } return false; }, 25, "cofre obert pel company, obert tambe ací");
        if (failed) yield break;
        if (!NetCoop.InterceptChestOpen(opened)) { Fail("l'amfitrio ha pogut tornar a obrir el cofre"); yield break; }
        Ok("el cofre no es pot obrir dues vegades");
        yield return WaitFor(() => System.Array.Exists(FindObjectsByType<HealPickup>(FindObjectsSortMode.None),
            heal => Vector2.Distance(heal.transform.position, opened.transform.position) < 3f),
            10, "l'amfitrio genera la cura del cofre obert pel client");
        // Bot real de boss: forcem la probabilitat, no substituim la ruta de botí.
        var bossPrefab = g.Theme != null && g.Theme.boss != null ? g.Theme.boss : g.bossPrefab;
        EnemyHealth bossHealth = null;
        yield return SpawnTestBoss(g, bossPrefab, h => bossHealth = h);
        int baseBossHealth = bossPrefab.GetComponent<EnemyHealth>().maxHealth;
        if (bossHealth == null || bossHealth.maxHealth != Mathf.RoundToInt(baseBossHealth * 1.55f))
        {
            Fail("vida del boss amb dos jugadors incorrecta");
            yield break;
        }
        Ok("boss amb dos jugadors: " + bossHealth.currentHealth + "/" + bossHealth.maxHealth);
        bossHealth.healDropChance = 1;
        yield return null;
        var warning = MakeBossWarning(bossHealth, bossHealth.transform.position);
        yield return new WaitForSecondsRealtime(.8f);
        bossHealth.TakeHit(99999, Vector2.zero);
        yield return null;
        if (warning != null) { Fail("queda un avís del boss després de morir al host"); yield break; }
        Ok("avís del boss retirat al host");
        Ok("boss mort amb cura garantida per provar el boti compartit");
        // 5) Caixa trencada pel company i moneda compartida
        yield return WaitFor(() => NetShared.RemoteBreaks > 0, 20, "caixa trencada pel company, trencada tambe ací");
        yield return WaitFor(() => NetShared.RemoteItemsSeen > 0, 20, "moneda del company visible ací");
        yield return WaitFor(() => NetShared.ItemsTakenByOthers > 0, 20, "moneda agafada pel company, desapareix ací");
        var avc = FindFirstObjectByType<RemoteAvatar>();
        Teleport(g, g.Rooms[0]); // l'amfitrio s'aparta: el company queda clarament mes a prop
        yield return new WaitForSecondsRealtime(0.3f);
        if (avc != null) { CoinDrop.Spawn(avc.transform.position); Ok("moneda de l'amfitrio als peus del company"); }
        // 6) Dany al company
        var av = FindFirstObjectByType<RemoteAvatar>();
        if (av == null) { Fail("no hi ha avatar del company"); yield break; }
        int clientId = av.playerId;
        var existingClientDrops = new System.Collections.Generic.HashSet<int>(
            Array.ConvertAll(InventoryDrops(clientId), drop => drop.id));
        NetCoop.DamagePlayer(av.transform, 1);
        Ok("dany enviat al company");
        // 7) El company cau i espera
        yield return WaitFor(() => NetCoop.DownedCount > 0, 25, "el company ha caigut i espera");
        NetItem[] clientDeathDrops = null;
        yield return WaitFor(() =>
        {
            clientDeathDrops = Array.FindAll(InventoryDrops(clientId), drop => !existingClientDrops.Contains(drop.id));
            return HasBothDropKinds(clientDeathDrops);
        }, 10, "l'amfitrió veu les armes i cures del client caigut");
        if (failed) yield break;
        // 8) Acabem la sala: tots els enemics morts -> portes obertes i el company torna
        float until = Time.realtimeSinceStartup + 40;
        while (!combat.cleared && Time.realtimeSinceStartup < until)
        {
            foreach (var h in new System.Collections.Generic.List<EnemyHealth>(NetCoop.Enemies)) if (h != null && !h.IsDead) h.TakeHit(9999, Vector2.zero);
            yield return new WaitForSecondsRealtime(0.4f);
        }
        if (!combat.cleared) { Fail("la sala no s'ha acabat"); yield break; }
        Ok("sala netejada");
        if(!DungeonGenerator.CanInteractAt(g.ground.GetCellCenterWorld(new Vector3Int(combat.Center.x,combat.Center.y,0)))) { Fail("la sala netejada no desbloqueja objectes"); yield break; }
        Ok("sala netejada desbloqueja objectes");
        yield return WaitFor(() => NetCoop.DownedCount == 0, 5, "el company ha tornat");
        yield return WaitFor(() => NetSession.Instance.Players.TryGetValue(av.playerId, out var revived) &&
            revived.maxHealth > 0 && revived.health == revived.maxHealth && revived.lastPos > Time.realtimeSinceStartupAsDouble - 1,
            5, "l'amfitrio veu la vida plena del company");
        yield return WaitFor(() => Array.TrueForAll(clientDeathDrops, drop => drop == null),
            12, "els objectes recuperats pel client desapareixen també a l'amfitrió");
        if (failed) yield break;
        // El company torna a caure i nosaltres passem de pis: ell ha de reapareixer al pis seguent
        yield return WaitFor(() => NetCoop.DownedCount > 0, 20, "el company ha caigut altra vegada");
        if (failed) yield break;
        g.NextFloor();
        yield return WaitFor(() => Scene == "Dungeon" && RunState.Floor == 2 && DungeonGenerator.Instance != null && DungeonGenerator.Instance != g, 15, "passem al pis 2 amb el company caigut");
        yield return WaitFor(() => Avatars >= 1, 20, "el company ha reaparegut al pis 2");
        EnemyHealth.AnyDied -= onDied;
        keepAlive = false;
        yield return HostDeathRecovery(clientId);
        if (failed) yield break;
        yield return HostProgression(clientId);
    }

    IEnumerator JoinCoop()
    {
        var g = DungeonGenerator.Instance;
        if (g == null) { Fail("no hi ha masmorra"); yield break; }
        var health = FindFirstObjectByType<PlayerHealth>();
        keepAlive = true; StartCoroutine(KeepAlive(health));
        if (!CheckStartChests(g, 2)) yield break;
        SpawnTestCrate(g);
        yield return CheckProjectiles(g); if(failed)yield break;
        // L'amfitrio entra primer i comprova que la sala l'espera
        yield return WaitFor(() => NetShared.RemoteHealsSeen >= 4, 15, "totes les cures rebudes en fred de l'amfitrio");
        // Simulem un objecte que el client ha perdut en carregar i repetim l'estat.
        HealPickup missing = null;
        foreach(var hp in FindObjectsByType<HealPickup>(FindObjectsSortMode.None))
            if(hp.transform.position.x >= 1000 && hp.GetComponent<NetItem>() != null) { missing=hp;break; }
        if(missing == null) { Fail("falta cura per provar resincronitzacio");yield break; }
        int missingId=missing.GetComponent<NetItem>().id;
        Destroy(missing.gameObject);
        yield return null;
        NetSession.Instance.SendToHost(NetProtocol.Frame(Msg.EnemyMark,w=>{w.Write(-1);w.Write(NetSession.SceneToken());}));
        yield return WaitFor(()=>{
            foreach(var ni in FindObjectsByType<NetItem>(FindObjectsSortMode.None))if(ni.id==missingId)return true;
            return false;
        },10,"cura perduda recuperada en resincronitzar");
        NetSession.Instance.SendToHost(NetProtocol.Frame(Msg.EnemyMark,w=>{w.Write(-1);w.Write(NetSession.SceneToken());}));
        yield return new WaitForSecondsRealtime(.4f);
        foreach (var heal in HealItem.All)
        {
            bool visible = false;
            int copies = 0;
            foreach (var pickup in FindObjectsByType<HealPickup>(FindObjectsSortMode.None))
                if (pickup.item == heal && pickup.GetComponentInChildren<SpriteRenderer>() != null) { visible = true; if(pickup.transform.position.x>=1000 && pickup.transform.position.x<1010)copies++; }
            if (!visible) { Fail("cura no visible: " + heal.name); yield break; }
            if(copies!=1) { Fail("resincronitzacio duplica la cura: "+heal.name);yield break; }
        }
        Ok("resincronitzacio repetida sense duplicar cures");
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        if (catalog == null) { Fail("falta cataleg d'armes"); yield break; }
        foreach (var weapon in catalog.weapons)
            if (NetCoop.Weapon(weapon.name) != weapon) { Fail("arma no resolta: " + weapon.name); yield break; }
        Ok("totes les armes resoltes pel nom");
        var waiting = FirstCombat(g);
        yield return WaitFor(() => waiting != null && waiting.waitingEntrance >= 0, 15, "estat de portes en espera rebut del host");
        if (!CheckWaitingDoors(g, waiting)) yield break;
        if (!CheckRoomLocked(g, waiting)) yield break;
        yield return new WaitForSecondsRealtime(4f);
        Teleport(g, FirstCombat(g));
        Ok("el client entra a la sala");
        yield return WaitFor(() => NetCoop.EnemyCount > 0, 25, "copies dels enemics de l'amfitrio");
        if (failed) yield break;
        Ok("copies d'enemics: " + NetCoop.EnemyCount);
        if (!CheckRoomLocked(g, waiting)) yield break;
        // El client trenca una caixa dins de la sala encara activa i recull la moneda.
        var fixture = GameObject.Find("Caixa de prova en combat");
        var crate = fixture != null ? fixture.GetComponent<Breakable>() : null;
        if (crate == null || DungeonGenerator.CanInteractAt(crate.transform.position) ||
            !DungeonGenerator.CanDamageBreakableAt(crate.transform.position))
        {
            Fail("falta la caixa de la sala activa");
            yield break;
        }
        var cratePosition = crate.transform.position;
        int coinsBeforeCrate = RunState.Coins;
        crate.Hit(1000);
        yield return WaitFor(() => crate == null, 5, "caixa trencada pel client en combat");
        if (failed) yield break;
        g.player.GetComponent<Rigidbody2D>().position = cratePosition;
        g.player.position = cratePosition;
        yield return WaitFor(() => RunState.Coins > coinsBeforeCrate, 8, "moneda de la caixa recollida en combat");
        if (failed) yield break;
        EnemyHealth victim = null;
        foreach (var h in NetCoop.Enemies) if (h != null && !h.IsDead) { victim = h; break; }
        if (victim == null || !victim.netProxy) { Fail("la copia no esta marcada com a copia"); yield break; }
        if (victim.GetComponent<EnemyPathfindingAI>() is EnemyPathfindingAI ai && ai.enabled) { Fail("la copia te la IA activa"); yield break; }
        if (victim.transform.Find("Ombra") != null) Ok("la copia de l'enemic te la seua ombra"); else Log("avis: la copia no te fill Ombra");
        victim.TakeHit(9999, Vector2.zero);
        yield return WaitFor(() => victim == null, 10, "l'amfitrio ha confirmat la mort de l'enemic");
        if (failed) yield break;
        // Disparem unes quantes bales perque l'amfitrio les veja
        WeaponData gun = null;
        foreach (var wd in Resources.FindObjectsOfTypeAll<WeaponData>()) if (wd != null && !wd.melee) { gun = wd; break; }
        if (gun == null) { Fail("no trobe cap arma"); yield break; }
        for (int i = 0; i < 8; i++)
        {
            Vector2 p = g.player.position;
            NetCoop.LocalPellet(gun, p, Vector2.right, p + Vector2.right * 4f);
            yield return new WaitForSecondsRealtime(0.25f);
        }
        Ok("bales enviades (" + gun.name + ")");
        yield return Shot("8-masmorra-jugador");
        // Cofre de la sala inicial
        Chest nearest = null; float best = float.MaxValue;
        foreach (var ch in FindObjectsByType<Chest>(FindObjectsSortMode.None))
        {
            float d = Vector2.Distance(ch.transform.position, g.player.position);
            if (!ch.IsOpen && DungeonGenerator.CanInteractAt(ch.transform.position) && d < best) { best = d; nearest = ch; }
        }
        if (nearest == null) { Fail("cap cofre tancat"); yield break; }
        if (!NetCoop.InterceptChestOpen(nearest)) { Fail("el client ha obert el cofre sense preguntar"); yield break; }
        yield return WaitFor(() => nearest.IsOpen, 10, "l'amfitrio ha donat el cofre al client");
        GameObject bossWarning = null;
        yield return WaitFor(() => (bossWarning = GameObject.Find("Avis atac")) != null,
            12, "avís del boss rebut pel client");
        if (failed) yield break;
        yield return WaitFor(() => bossWarning == null, 3, "avís del boss retirat al client quan mor");
        if (failed) yield break;
        yield return WaitFor(() =>
        {
            foreach (var hp in FindObjectsByType<HealPickup>(FindObjectsSortMode.None))
                if (hp.transform.position.x > 1090 && hp.GetComponent<NetItem>() != null) return true;
            return false;
        }, 15, "cura del boss de l'amfitrio visible al client");
        // Moneda: apareix als peus, l'agafem i l'amfitrio ens la dona
        int coins = RunState.Coins;
        CoinDrop.Spawn(g.player.position);
        yield return WaitFor(() => RunState.Coins > coins, 15, "moneda agafada (concedida per l'amfitrio)");
        yield return WaitFor(() => RunState.Coins > coins + 1, 15, "moneda de l'amfitrio agafada pel client (iman cap al mes proxim)");
        yield return WaitFor(() => NetCoop.HurtsReceived > 0, 20, "dany rebut des de l'amfitrio");
        // Caiem: mode espectador
        keepAlive = false;
        yield return new WaitForSecondsRealtime(1.3f); // fora la invulnerabilitat del colp
        var att = g.player.GetComponent<PlayerAttack>();
        var bag = g.player.GetComponent<HealInventory>();
        if (att != null) att.AddWeapon(gun);
        if (bag != null) bag.Add(HealItem.All[0]);
        var preexistingDrops = new System.Collections.Generic.HashSet<int>();
        foreach (var item in FindObjectsByType<NetItem>(FindObjectsSortMode.None)) preexistingDrops.Add(item.id);
        yield return null;
        var hand = GameObject.Find("Arma a la mà")?.GetComponent<SpriteRenderer>();
        if (hand == null || !hand.enabled) { Fail("falta l'arma visible de prova"); yield break; }
        health.TakeDamage(999);
        yield return WaitFor(() => Spectate.Active, 5, "caigut: espectant el company");
        if (hand.enabled) { Fail("l'arma de la mà queda flotant quan cau el jugador"); yield break; }
        Ok("es retira l'arma de la mà quan cau el jugador");
        var myAttack = g.player.GetComponent<PlayerAttack>();
        if (myAttack != null && myAttack.CurrentWeapon == null && !System.Array.Exists(myAttack.hotbarSlots, w => w != null)) Ok("en caure, les armes han quedat al terra"); else Fail("encara te armes despres de caure");
        var cam = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        yield return null;
        if (cam != null && cam.target != null && cam.target.GetComponent<RemoteAvatar>() != null) Ok("la camera segueix el company");
        else Log("avis: la camera no segueix cap avatar");
        yield return Shot("9-espectant");
        yield return WaitFor(() => NetCoop.RoomEvents >= 2, 45, "sala començada i acabada per l'amfitrio");
        yield return WaitFor(() => health != null && health.gameObject.activeInHierarchy && !Spectate.Active, 10, "tornat a la partida en netejar la sala");
        if (failed) yield break;
        if (!health.IsFull || System.Array.Exists(health.hearts, heart => heart != null && heart.sprite != health.cor_ple))
        {
            Fail("vida incompleta al tornar a la sala");
            yield break;
        }
        Ok("vida plena al tornar a la sala");
        if (cam != null && cam.target == health.transform) Ok("la camera torna al jugador");
        int localOwner = NetSession.Instance.LocalId;
        yield return new WaitForSecondsRealtime(0.8f);
        var drops = new System.Collections.Generic.List<NetItem>();
        foreach (var item in FindObjectsByType<NetItem>(FindObjectsSortMode.None))
        {
            if (item.id / 1000000 != localOwner || preexistingDrops.Contains(item.id)) continue;
            if (item.GetComponent<ItemPickup>() != null || item.GetComponent<HealPickup>() != null)
                drops.Add(item);
        }
        foreach (var drop in drops)
        {
            if (drop == null) continue;
            var rb = g.player.GetComponent<Rigidbody2D>();
            rb.position = drop.transform.position;
            g.player.position = drop.transform.position;
            string description = drop.kind + " " + drop.data;
            yield return WaitFor(() => drop == null, 5, "objecte propi recuperat: " + description);
            if (drop != null)
                Log("drop=" + drop.transform.position + " jugador=" + g.player.position + " pendent=" + drop.pending);
            if (failed) yield break;
        }
        if (att == null || !System.Array.Exists(att.hotbarSlots, w => w != null) ||
            bag == null || !System.Array.Exists(bag.items, item => item != null))
        {
            Fail("el jugador reviscut no ha recuperat armes i cures");
            yield break;
        }
        Ok("armes i cures recuperades pel jugador que les havia soltat");
        // Tornem a caure i l'amfitrio passa de pis: hem de reapareixer al pis 2 sense armes i amb tota la vida
        var att2 = g.player.GetComponent<PlayerAttack>(); if (att2 != null) att2.AddWeapon(gun);
        yield return new WaitForSecondsRealtime(2.4f); // fora la invulnerabilitat de tornar
        health.TakeDamage(999);
        yield return WaitFor(() => Spectate.Active, 5, "caigut altra vegada");
        yield return WaitFor(() => RunState.Floor == 2 && Scene == "Dungeon" && DungeonGenerator.Instance != null && DungeonGenerator.Instance != g, 30, "els companys han passat de pis");
        if (failed) yield break;
        yield return null; yield return null;
        var p2 = FindFirstObjectByType<PlayerHealth>();
        var a2 = p2 != null ? p2.GetComponent<PlayerAttack>() : null;
        if (p2 == null || !p2.gameObject.activeInHierarchy) { Fail("no ha reaparegut al pis 2"); yield break; }
        if (a2 != null && System.Array.Exists(a2.hotbarSlots, w => w != null)) { Fail("ha reaparegut amb armes"); yield break; }
        if (!p2.IsFull) { Fail("vida incompleta al reapareixer al pis 2"); yield break; }
        Ok("reaparegut al pis 2 sense armes, vida " + p2.currentHealth + "/" + p2.maxHealth);
        yield return ObserveHostRecovery();
        if (failed) yield break;
        yield return JoinProgression();
    }


    // Captures de les armes, enemics i bosses nous (cal obrir el joc amb gràfics i -botShots <carpeta>)
    IEnumerator WeaponShots()
    {
        RunState.Reset();
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => Scene == "Dungeon" && DungeonGenerator.Instance != null, 12, "masmorra individual carregada");
        if (failed) yield break;
        yield return new WaitForSeconds(1f);
        var player = DungeonGenerator.Instance.player;
        var attack = player.GetComponent<PlayerAttack>();
        yield return CheckNewWeapons(attack, player.position);
        if (failed) yield break;
        yield return CheckNewEnemies();
        if (!failed) { Log("PASS"); Application.Quit(0); }
    }
    // Prova de regressió en una masmorra individual real.
    IEnumerator SoloCombat()
    {
        RunState.Reset();
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => Scene == "Dungeon" && DungeonGenerator.Instance != null, 12, "masmorra individual carregada");
        if (failed) yield break;
        yield return null;

        var player = DungeonGenerator.Instance.player;
        var attack = player.GetComponent<PlayerAttack>();
        var shoot = typeof(PlayerAttack).GetMethod("Shoot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        foreach (string id in new[] { "Escopeta", "VaretaEspores", "Shuriken", "AK47", "Minigun", "SMG" })
        {
            WeaponData weapon = NetCoop.Weapon(id);
            if (weapon == null) { Fail("arma absent: " + id); yield break; }
            var samples = new System.Collections.Generic.HashSet<string>();
            for (int trial = 0; trial < 5; trial++)
            {
                shoot.Invoke(attack, new object[] { weapon });
                var shots = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
                var angles = System.Array.ConvertAll(shots, shot => shot.transform.eulerAngles.z);
                System.Array.Sort(angles);
                if (angles.Length != weapon.pellets) { Fail("nombre de bales incorrecte: " + id); yield break; }
                samples.Add(string.Join(",", angles));
                foreach (var shot in shots) Destroy(shot.gameObject);
                yield return null;
            }
            bool fixedPattern = id == "Escopeta" || id == "VaretaEspores" || id == "Shuriken";
            if ((samples.Count == 1) != fixedPattern) { Fail("dispersio incorrecta: " + id); yield break; }
            Ok("dispersio " + (fixedPattern ? "fixa: " : "aleatoria: ") + id);
        }

        Vector2 origin = new Vector2(600, 600);
        var wall = new GameObject("Paret de prova");
        wall.transform.position = origin + Vector2.right * .45f;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(.06f, 1f);
        Physics2D.SyncTransforms();
        if (Projectile.SafeMuzzle(origin, origin + Vector2.right, .05f, player) != origin)
        {
            Fail("la boca de l'arma salta la paret");
            yield break;
        }
        var fast = ScriptableObject.CreateInstance<WeaponData>();
        fast.projectileSpeed = 120;
        fast.projectileSize = .10f;
        fast.range = 6f;
        var shotThroughWall = Projectile.Spawn(origin, Vector2.right, fast, player, 0, 0);
        yield return new WaitForFixedUpdate();
        yield return null;
        if (shotThroughWall != null) { Fail("el projectil rapid travessa la paret"); yield break; }
        Destroy(wall);
        Destroy(fast);
        Ok("el cano i el projectil rapid respecten la paret");
        yield return CheckCometDisc(player, origin + Vector2.up * 10f);
        if (failed) yield break;
        yield return CheckArsenal(attack, origin + Vector2.up * 20f);
        if (failed) yield break;
        yield return CheckNewWeapons(attack, origin + Vector2.up * 30f);
        if (failed) yield break;

        var camera = Camera.main;
        float normalSize = camera.orthographicSize;
        foreach (string id in new[] { "Sniper", "Laser" })
        {
            var weapon = NetCoop.Weapon(id);
            attack.hotbarSlots[0] = weapon;
            attack.SelectSlot(0);
            yield return WaitFor(() => camera.orthographicSize * Mathf.Min(1f, camera.aspect) >= CameraFollow.ZoomRange(weapon), 2, "el zoom de " + id);
            if (failed) yield break;
        }
        foreach (string id in new[] { "Ballesta", "BallestaCristall", "Vareta", "DiscCometa", "ProjectorPolsos" })
        {
            attack.hotbarSlots[0] = NetCoop.Weapon(id);
            yield return WaitFor(() => Mathf.Approximately(camera.orthographicSize, normalSize), 2, "zoom normal amb " + id);
            if (failed) yield break;
        }
        attack.hotbarSlots[0] = null;
        yield return WaitFor(() => Mathf.Approximately(camera.orthographicSize, normalSize), 2, "zoom normal amb el puny");
        if (failed) yield break;

        // Una caiguda voluntària contra una paret de la masmorra.
        var dungeon = DungeonGenerator.Instance;
        bool wallFound = false;
        Vector3 dropFrom = Vector3.zero, desired = Vector3.zero;
        Vector2 wallDirection = Vector2.zero;
        foreach (var cell in dungeon.ground.cellBounds.allPositionsWithin)
        {
            if (TileNav.IsBlocked(cell)) continue;
            foreach (var direction in new[] { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down })
            {
                if (!dungeon.walls.HasTile(cell + direction) || TileNav.IsBlocked(cell - direction)) continue;
                dropFrom = dungeon.ground.GetCellCenterWorld(cell);
                wallDirection = (Vector3)direction;
                desired = dungeon.ground.GetCellCenterWorld(cell + direction);
                wallFound = true;
                break;
            }
            if (wallFound) break;
        }
        if (!wallFound || !TileNav.CircleFree(TileNav.DropDestination(dropFrom, desired), 0.12f))
        {
            Fail("un objecte pot caure dins d'una paret");
            yield break;
        }
        var sniper = NetCoop.Weapon("Sniper");
        var pistol = NetCoop.Weapon("Pistola");
        const float remaining = 0.6f;
        attack.AddWeapon(sniper, remaining);
        attack.AddWeapon(pistol);
        var cooldowns = (float[])typeof(PlayerAttack).GetField("slotCooldown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(attack);
        attack.SelectSlot(1);
        yield return new WaitForSeconds(0.1f);
        if (cooldowns[0] != remaining || cooldowns[1] != 0f)
        {
            Fail("el cooldown baixa amb l'arma guardada o afecta una altra casella");
            yield break;
        }
        attack.SwapSlots(0, 1);
        if (cooldowns[1] != remaining || attack.hotbarSlots[1] != sniper)
        {
            Fail("el cooldown no acompanya l'arma quan canvia de casella");
            yield break;
        }
        attack.hotbarSlots[0] = null;
        attack.hotbarSlots[1] = null;
        cooldowns[1] = 0f;
        attack.AddWeapon(sniper, remaining);
        var dropGun = sniper;
        attack.SelectSlot(0);
        player.GetComponent<Rigidbody2D>().position = dropFrom;
        player.position = dropFrom;
        typeof(PlayerAttack).GetField("aimDirection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(attack, wallDirection);
        attack.DropSlot(0);
        yield return new WaitForSeconds(0.9f);
        var dropped = FindFirstObjectByType<ItemPickup>();
        if (dropped == null || dropped.weapon != dropGun || dropped.CooldownRemaining != remaining || !TileNav.CircleFree(dropped.transform.position, 0.12f))
        {
            Fail("l'arma solta no ha quedat al terra davant la paret");
            yield break;
        }
        Vector2 resting = dropped.transform.position;
        var body = player.GetComponent<Rigidbody2D>();
        var away = (Vector2)dropFrom - wallDirection * 1.2f;
        body.position = away;
        player.position = away;
        yield return null;
        body.position = resting;
        player.position = resting;
        yield return WaitFor(() => dropped == null, 2, "arma solta al costat de la paret recuperada");
        if (failed) yield break;
        if (cooldowns[0] <= 0f || attack.CurrentWeapon != sniper)
        {
            Fail("l'arma recuperada ha perdut el cooldown pendent");
            yield break;
        }
        Ok("cooldown independent i conservat en canviar, soltar i recollir");
        attack.hotbarSlots[0] = null;
        cooldowns[0] = 0f;

        var movementController = player.GetComponent<PlayerMovement>();
        var moveInput = typeof(PlayerMovement).GetField("movement", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var movementStep = typeof(PlayerMovement).GetMethod("FixedUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Vector2 slideDirection = Vector2.Perpendicular(wallDirection);
        Vector3Int stepCell = new Vector3Int(Mathf.RoundToInt(slideDirection.x), Mathf.RoundToInt(slideDirection.y), 0);
        if (TileNav.IsBlocked(TileNav.Cell(dropFrom) + stepCell)) slideDirection = -slideDirection;
        foreach (bool dash in new[] { false, true })
        {
            movementController.enabled = false;
            body.position = dropFrom;
            player.position = dropFrom;
            if (dash) movementController.Dash(wallDirection, 1.8f);
            Vector2 direction = (wallDirection + slideDirection).normalized;
            for (int i = 0; i < 13; i++)
            {
                moveInput.SetValue(movementController, direction);
                movementStep.Invoke(movementController, null);
                yield return new WaitForFixedUpdate();
            }
            float travel = Vector2.Dot(body.position - (Vector2)dropFrom, slideDirection);
            movementController.enabled = true;
            if (travel < .3f || dungeon.walls.HasTile(TileNav.Cell(body.position)))
            {
                Fail("la canica no llisca al mur " + (dash ? "després de l'impuls" : "caminant"));
                yield break;
            }
            Ok(dash ? "l'impuls bloquejat permet continuar movent-se" : "el jugador llisca al mur");
        }

        var arena = dungeon.Rooms[0];
        var emptyRoomType = arena.type;
        arena.type = DungeonGenerator.RoomType.Combat;
        arena.active = true;
        Vector3 cratePos = dungeon.ground.GetCellCenterWorld(new Vector3Int(arena.Center.x, arena.Center.y, 0));
        player.position = cratePos + Vector3.right;
        body.position = player.position;
        var crate = Instantiate(dungeon.cratePrefab, cratePos, Quaternion.identity).GetComponent<Breakable>();
        crate.coinChance = 1f;
        crate.healChance = 0f;
        yield return null;
        int coins = RunState.Coins;
        crate.Hit(999);
        yield return WaitFor(() => crate == null && RunState.Coins > coins && arena.active, 5, "caixa trencada i moneda recollida en combat individual");
        if (failed) yield break;
        arena.active = false;
        arena.type = emptyRoomType;

        // v1.2.1 (BUG-021): una arma cos a cos (Espasa de vidre) trenca una caixa a una casella i mitja
        {
            var swingCrate = Instantiate(dungeon.cratePrefab, cratePos, Quaternion.identity).GetComponent<Breakable>();
            swingCrate.coinChance = 0f; swingCrate.healChance = 0f;
            player.position = cratePos + Vector3.right * 1.5f; body.position = player.position;
            Physics2D.SyncTransforms();
            yield return null;
            var instance = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(PlayerAttack).GetField("aimDirection", instance).SetValue(attack, Vector2.left);
            var swing = typeof(PlayerAttack).GetMethod("Swing", instance);
            var sword = NetCoop.Weapon("EspasaVidre");
            for (int i = 0; i < 4 && swingCrate != null; i++) { swing.Invoke(attack, new object[] { sword }); yield return null; }
            if (swingCrate != null) { Fail("l'Espasa de vidre no trenca una caixa a una casella i mitja (BUG-021)"); yield break; }
            Ok("el cos a cos trenca caixes");
        }

        var db = CosmeticsDatabase.Instance;
        for (int skin = 0; skin < db.skins.Length; skin++)
        {
            var fixtures = new System.Collections.Generic.List<GameObject>();
            for (int hat = 1; hat < db.hats.Length; hat++)
            {
                var fixture = new GameObject("Prova de barret");
                fixture.SetActive(false);
                fixture.AddComponent<SpriteRenderer>().sprite = db.playerSprite;
                var cosmetics = fixture.AddComponent<PlayerCosmetics>();
                cosmetics.skinOverride = skin;
                cosmetics.hatOverride = hat;
                cosmetics.trailOverride = 1;
                fixture.SetActive(true);
                fixtures.Add(fixture);
            }
            yield return null;
            for (int i = 0; i < fixtures.Count; i++)
            {
                var fixture = fixtures[i];
                var hat = db.hats[i + 1];
                Transform fitted = fixture.transform.Find("Barret");
                if (fitted == null || (!hat.fullHelmet && fitted.localPosition.y <= 0f) ||
                    (fixture.GetComponent<TrailRenderer>() == null && fixture.GetComponent<TrailParticleEmitter>() == null))
                {
                    Fail("barret o rastre mal aplicat: " + db.skins[skin].name + " / " + hat.name);
                    yield break;
                }
                Destroy(fixture);
            }
        }
        Ok("totes les combinacions de skins i barrets s'apliquen amb rastre");
        for (int i = 0; i < dungeon.themes.Length; i++)
        {
            RunState.Floor = i + 1;
            var prefab = dungeon.themes[i].boss;
            EnemyHealth bossHealth = null;
            yield return SpawnTestBoss(dungeon, prefab, h => bossHealth = h);
            int expected = prefab.GetComponent<EnemyHealth>().maxHealth + 25 * i;
            if (bossHealth == null || bossHealth.currentHealth != expected)
            {
                Fail("vida de boss individual incorrecta al pis " + RunState.Floor);
                yield break;
            }
            if (i == 0)
            {
                yield return null;
                var warning = MakeBossWarning(bossHealth, bossHealth.transform.position);
                bossHealth.TakeDamage(99999);
                yield return null;
                if (warning != null) { Fail("queda un avís del boss després de morir en solitari"); yield break; }
                Ok("avís del boss retirat en solitari");
            }
            else Destroy(bossHealth.gameObject);
            Ok("vida del boss individual al pis " + RunState.Floor + ": " + expected);
        }
        yield return CheckTrailReset();
        if (failed) yield break;
        yield return CheckHeldAttack();
        if (failed) yield break;
        yield return CheckChestLootAtWall();
        if (failed) yield break;
        yield return CheckProgression();
        if (failed) yield break;
        yield return CheckFloorReturn();
        if (failed) yield break;
        yield return CheckSoloSummary();
        if (failed) yield break;
        yield return CheckNewEnemies();
        if (!failed) Pass();
    }

    static void ClickButton(string label)
    {
        var go = GameObject.Find("Boto " + label);
        go?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();
    }

    // Resum de partida en solitari: compta enemics, bosses i pis; l'arma favorita surt del context del colp;
    // "Tornar a jugar" comença al pis 1 amb el resum a zero.
    IEnumerator CheckSoloSummary()
    {
        var dungeon = DungeonGenerator.Instance;
        if (RunStats.Seconds <= 0f) { Fail("el temps de partida no avança"); yield break; }
        EnemyHealth target = null;
        yield return SpawnTestBoss(dungeon, dungeon.themes[0].boss, h => target = h);
        if (target == null) { Fail("no s'ha pogut crear l'enemic del resum"); yield break; }
        int kills = RunStats.Kills, bosses = RunStats.BossKills;
        RunStats.HitBy = "Sniper"; target.TakeHit(99999, Vector2.zero); RunStats.HitBy = null;
        string fav = RunStats.FavoriteWeapon(out int favKills);
        if (RunStats.Kills != kills + 1 || RunStats.BossKills != bosses + 1) { Fail($"el resum no compta el boss ({RunStats.Kills - kills} enemics, {RunStats.BossKills - bosses} bosses)"); yield break; }
        if (fav != "Sniper" || favKills < 1) { Fail("arma favorita del resum incorrecta: " + fav); yield break; }
        Ok($"resum: {RunStats.Kills} enemics, {RunStats.BossKills} bosses, pis {RunStats.MaxFloor}, {RunStats.TimeText()}, favorita {fav}");

        dungeon.PlayerDied();
        yield return WaitFor(() => GameObject.Find("Resum de partida") != null && GameObject.Find("Boto TORNAR A JUGAR") != null &&
            GameObject.Find("Boto MENÚ PRINCIPAL") != null, 5, "resum de partida en solitari amb els dos botons");
        if (failed) yield break;
        yield return new WaitForSecondsRealtime(0.5f);
        if (DungeonGenerator.Instance != dungeon) { Fail("el resum en solitari no ha esperat el botó"); yield break; }
        ClickButton("TORNAR A JUGAR");
        yield return WaitFor(() => Scene == "Dungeon" && RunState.Floor == 1 && DungeonGenerator.Instance != null &&
            DungeonGenerator.Instance != dungeon && RunStats.Kills == 0 && RunStats.MaxFloor == 1, 12, "tornar a jugar comença al pis 1 amb el resum a zero");
    }

    // Cooperatiu: l'amfitrio veu el resum amb els seus botons i "tornar a jugar" porta tota la colla al pis 1
    IEnumerator HostRunRestart()
    {
        yield return new WaitForSecondsRealtime(10f); // Que el client acabe les seues comprovacions del pis
        var dungeon = DungeonGenerator.Instance;
        NetCoop.ForceGameOverAll();
        yield return WaitFor(() => GameObject.Find("Resum de partida") != null && GameObject.Find("Boto TORNAR A JUGAR") != null &&
            GameObject.Find("Boto TORNAR AL LOBBY") != null, 5, "l'amfitrió veu el resum cooperatiu amb els seus botons");
        if (failed) yield break;
        yield return new WaitForSecondsRealtime(2f);
        ClickButton("TORNAR A JUGAR");
        yield return WaitFor(() => Scene == "Dungeon" && RunState.Floor == 1 && DungeonGenerator.Instance != null &&
            DungeonGenerator.Instance != dungeon && Avatars >= 1, 20, "tota la colla torna a jugar al pis 1");
    }

    IEnumerator JoinRunRestart()
    {
        DungeonGenerator dungeon = null;
        yield return WaitFor(() => GameObject.Find("Resum de partida") != null && (dungeon = DungeonGenerator.Instance) != null, 30,
            "el client veu el resum de partida");
        if (failed) yield break;
        if (GameObject.Find("Boto TORNAR A JUGAR") != null || GameObject.Find("Boto EIXIR DE LA SALA") == null)
        { Fail("el client no hauria de poder decidir tornar a jugar"); yield break; }
        Ok("el client espera l'amfitrió");
        yield return WaitFor(() => Scene == "Dungeon" && RunState.Floor == 1 && DungeonGenerator.Instance != null &&
            DungeonGenerator.Instance != dungeon, 25, "el client torna a jugar quan ho diu l'amfitrió");
    }

    // v1.3.1: enemics i bosses nous, al final perquè deixen la sala feta un nyap
    IEnumerator CheckNewEnemies()
    {
        var dungeonRoom = DungeonGenerator.Instance.Rooms[0];
        var origin = (Vector2)DungeonGenerator.Instance.ground.GetCellCenterWorld(new Vector3Int(dungeonRoom.Center.x, dungeonRoom.Center.y, 0)) + Vector2.left * 3f;
        var player = DungeonGenerator.Instance.player;
        var body = player.GetComponent<Rigidbody2D>();
        // Enemics i bosses nous: es creen, es mouen i ataquen sense errors
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        foreach (string id in new[] { "Gargola", "Cranc", "FantasmaGel", "ReinaAranya", "ReiGel" })
        {
            GameObject prefab = null;
            foreach (var e in catalog.enemies) if (e != null && e.name == id) prefab = e;
            if (prefab == null) { Fail("falta " + id + " al catàleg d'enemics"); yield break; }
            player.position = origin + new Vector2(-1.5f, 0f); if (body != null) body.position = player.position;
            var spawned = Instantiate(prefab, origin + new Vector2(5f, 0f), Quaternion.identity);
            var guard = player.GetComponent<PlayerHealth>();
            for (float waited = 0f; waited < (id.StartsWith("Rei") ? 6f : 2.5f); waited += 0.25f) { if (guard != null) guard.currentHealth = guard.maxHealth; yield return new WaitForSeconds(0.25f); }
            if (spawned == null || spawned.GetComponent<EnemyHealth>() == null) { Fail(id + " ha desaparegut o no té vida"); yield break; }
            yield return Shot("e-" + id);
            Destroy(spawned);
            var hurt = player.GetComponent<PlayerHealth>(); if (hurt != null) hurt.currentHealth = hurt.maxHealth;
            Ok("enemic nou: " + id);
        }
    }

    // v1.3.1: les sis armes noves fan el que diuen
    IEnumerator CheckNewWeapons(PlayerAttack attack, Vector2 origin)
    {
        const System.Reflection.BindingFlags instance = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        // Dins de la sala inicial (fora del mapa la línia de visió no existeix i la torreta no veuria res)
        var dungeonRoom = DungeonGenerator.Instance.Rooms[0];
        origin = (Vector2)DungeonGenerator.Instance.ground.GetCellCenterWorld(new Vector3Int(dungeonRoom.Center.x, dungeonRoom.Center.y, 0)) + Vector2.left * 3f;
        var aimField = typeof(PlayerAttack).GetField("aimDirection", instance);
        var player = attack.transform;
        var body = player.GetComponent<Rigidbody2D>();
        EnemyHealth Dummy(Vector2 at)
        {
            var go = new GameObject("Objectiu de prova nou", typeof(BoxCollider2D), typeof(EnemyHealth));
            go.transform.position = at;
            var health = go.GetComponent<EnemyHealth>(); health.currentHealth = 30;
            return health;
        }
        void Equip(string id) { attack.hotbarSlots[0] = NetCoop.Weapon(id); attack.SelectSlot(0); }
        player.position = origin; if (body != null) body.position = origin;
        aimField.SetValue(attack, Vector2.right);

        // Torreta: dispara sola contra l'enemic proper
        var target = Dummy(origin + new Vector2(4.5f, 0f)); Physics2D.SyncTransforms();
        Equip("BaculTorreta");
        typeof(PlayerAttack).GetMethod("Shoot", instance).Invoke(attack, new object[] { NetCoop.Weapon("BaculTorreta") });
        yield return WaitFor(() => target.currentHealth < 30, 3, "la torreta dispara a un enemic");
        if (failed) yield break;
        yield return Shot("w1-torreta");
        foreach (var t in FindObjectsByType<TurretUnit>(FindObjectsSortMode.None)) t.Expire();
        Ok("Bàcul de torreta");

        // Mina: explota quan s'hi acosta un enemic
        target.currentHealth = 30;
        player.position = origin + new Vector2(-1f, 0f); if (body != null) body.position = player.position;
        aimField.SetValue(attack, Vector2.right);
        var mineWeapon = NetCoop.Weapon("Llancamines");
        var mineGo = new GameObject("Mina de prova"); mineGo.transform.position = target.transform.position + Vector3.left * 0.3f;
        mineGo.AddComponent<MineUnit>().Setup(mineWeapon, player, 1.5f);
        yield return WaitFor(() => target.currentHealth <= 30 - mineWeapon.damage, 4, "la mina explota i fa mal");
        yield return Shot("w2-mina");
        if (failed) yield break;
        Ok("Llançamines");

        // Arc de llum: més càrrega, més mal
        foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) Destroy(p.gameObject);
        player.position = origin; if (body != null) body.position = origin;
        Equip("ArcLlum");
        var fire = typeof(PlayerAttack).GetMethod("FireCharged", instance);
        fire.Invoke(attack, new object[] { NetCoop.Weapon("ArcLlum"), 1f });
        yield return null;
        bool arrow = false; foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) arrow = true;
        if (!arrow) { Fail("l'Arc de llum no dispara la fletxa carregada"); yield break; }
        yield return Shot("w3-arc");
        Ok("Arc de llum");

        // Escut: reflecteix una bala enemiga que ve de cara
        foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) Destroy(p.gameObject);
        Equip("EscutMirall"); aimField.SetValue(attack, Vector2.right);
        var bullet = EnemyBullet.Fire(origin + new Vector2(1.1f, 0f), Vector2.left, 1f, 1, null);
        var tick = typeof(PlayerAttack).GetMethod("TickAttack", instance);
        tick.Invoke(attack, new object[] { true });
        yield return null;
        if (bullet != null) { Fail("l'Escut de mirall no reflecteix la bala"); yield break; }
        attack.enabled = false; tick.Invoke(attack, new object[] { true }); yield return Shot("w4-escut"); attack.enabled = true;
        Ok("Escut de mirall");

        // Ganxo: fa mal i estira l'enemic cap a tu
        var pulled = Dummy(origin + new Vector2(4.5f, 0f)); Physics2D.SyncTransforms();
        Equip("Ganxo"); aimField.SetValue(attack, Vector2.right);
        float before = pulled.transform.position.x;
        typeof(PlayerAttack).GetMethod("Shoot", instance).Invoke(attack, new object[] { NetCoop.Weapon("Ganxo") });
        yield return new WaitForSeconds(0.5f);
        if (pulled.currentHealth >= 30 || pulled.transform.position.x >= before) { Fail("el Ganxo no estira l'enemic ni li fa mal"); yield break; }
        yield return Shot("w6-ganxo");
        Ok("Ganxo d'abordatge");

        // Cofres: al pis 1 no hi ha èpics ni llegendaris; a partir del 3 sí que poden eixir
        var generator = DungeonGenerator.Instance;
        int savedFloor = RunState.Floor;
        var pick = typeof(DungeonGenerator).GetMethod("RandomRarity", instance);
        RunState.Floor = 1;
        for (int i = 0; i < 400; i++)
        {
            var rarity = (ChestRarity)pick.Invoke(generator, null);
            if (rarity == ChestRarity.Epic || rarity == ChestRarity.Llegendari) { RunState.Floor = savedFloor; Fail("al pis 1 ha eixit un cofre " + rarity); yield break; }
        }
        RunState.Floor = 6; bool legendary = false;
        for (int i = 0; i < 600 && !legendary; i++) legendary = (ChestRarity)pick.Invoke(generator, null) == ChestRarity.Llegendari;
        RunState.Floor = savedFloor;
        if (!legendary) { Fail("al pis 6 mai ix un cofre llegendari"); yield break; }
        Ok("cofres progressius");

        Destroy(target.gameObject); Destroy(pulled.gameObject);
    }

    IEnumerator CheckArsenal(PlayerAttack attack, Vector2 origin)
    {
        var flame = NetCoop.Weapon("Llancaflames");
        var minigun = NetCoop.Weapon("Minigun");
        const System.Reflection.BindingFlags instance = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var cooldowns = (float[])typeof(PlayerAttack).GetField("slotCooldown", instance).GetValue(attack);
        var conditions = (WeaponCondition[])typeof(PlayerAttack).GetField("conditions", instance).GetValue(attack);
        var tick = typeof(PlayerAttack).GetMethod("TickAttack", instance);
        attack.SelectSlot(0);
        attack.AddWeapon(flame, condition: new WeaponCondition(299, 0f));
        tick.Invoke(attack, new object[] { true });
        if (attack.CurrentWeapon != null || flame.maxShots != 300)
        { Fail("el Llancaflames no es trenca al tret 300"); yield break; }
        attack.AddWeapon(minigun, condition: new WeaponCondition(0, 59f));
        typeof(PlayerAttack).GetField("spinningWeapon", instance).SetValue(attack, minigun);
        typeof(PlayerAttack).GetField("spinCharge", instance).SetValue(attack, minigun.spinUpTime);
        tick.Invoke(attack, new object[] { true });
        if (conditions[0].Heat != 60f || cooldowns[0] != 2f)
        { Fail("la Minigun no es bloqueja durant dos segons"); yield break; }
        attack.hotbarSlots[0] = null;
        cooldowns[0] = 0f;
        conditions[0] = default;
        attack.SelectSlot(0);
        foreach (var projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None)) Destroy(projectile.gameObject);
        Ok("durabilitat del Llancaflames i bloqueig de la Minigun");

        var targets = new EnemyHealth[5];
        for (int i = 0; i < targets.Length; i++)
        {
            var target = new GameObject("Objectiu de prova", typeof(BoxCollider2D), typeof(EnemyHealth));
            target.transform.position = origin + Vector2.right * (i * 2);
            targets[i] = target.GetComponent<EnemyHealth>();
            targets[i].currentHealth = 20;
        }
        Physics2D.SyncTransforms();
        targets[0].TakeProjectileHit(2, Vector2.zero, ProjectileEffect.Chain);
        if (targets[0].currentHealth != 18 || targets[1].currentHealth != 20 - EnemyHealth.ChainJumpDamage || targets[2].currentHealth != 20 - EnemyHealth.ChainJumpDamage ||
            targets[3].currentHealth != 20 - EnemyHealth.ChainJumpDamage || targets[4].currentHealth != 20)
        { Fail("el Projector no respecta el limit de tres salts"); yield break; }
        targets[4].TakeProjectileHit(2, Vector2.zero, ProjectileEffect.Slow);
        if (!Mathf.Approximately(targets[4].MovementMultiplier, .8f))
        { Fail("la Ballesta de cristall no aplica l alentiment"); yield break; }
        for (int i = 0; i < 4; i++) targets[4].TakeProjectileHit(1, Vector2.zero, ProjectileEffect.Poison);
        yield return new WaitForSeconds(2.2f);
        if (targets[4].currentHealth != 8 || targets[4].MovementMultiplier != 1f)
        { Fail("el veri supera tres dosis o l alentiment no caduca"); yield break; }
        foreach (var target in targets) Destroy(target.gameObject);
        Ok("cadena de tres salts, alentiment temporal i veri amb tres dosis");
    }

    IEnumerator CheckCometDisc(Transform player, Vector2 origin)
    {
        var disc = NetCoop.Weapon("DiscCometa");
        var boomerang = NetCoop.Weapon("Bumerang");
        if (disc == null || disc.returns || disc.wallBounces != 3 || boomerang == null || !boomerang.returns)
        {
            Fail("el Disc i el Bumerang no tenen el comportament esperat");
            yield break;
        }

        var walls = new GameObject("Parets de prova del Disc");
        foreach (float side in new[] { -1f, 1f })
        {
            var wall = new GameObject("Paret de prova");
            wall.transform.SetParent(walls.transform);
            wall.transform.position = origin + Vector2.right * side * .65f;
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.1f, 2f);
        }
        Physics2D.SyncTransforms();
        var shot = Projectile.Spawn(origin, Vector2.right, disc, player, 0, 0);
        var remaining = typeof(Projectile).GetField("wallBouncesLeft", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        try
        {
            for (int bounce = 1; bounce <= 3; bounce++)
            {
                int expected = 3 - bounce;
                yield return WaitFor(() => shot != null && (int)remaining.GetValue(shot) == expected,
                    2, "rebot " + bounce + " del Disc de cometa");
                if (failed) yield break;
            }
            yield return WaitFor(() => shot == null, 2, "el Disc desapareix al quart impacte contra la paret");
        }
        finally
        {
            if (shot != null) Destroy(shot.gameObject);
            Destroy(walls);
        }
    }

    void EnterProgressionRoom(DungeonGenerator dungeon)
    {
        var center = dungeon.ProgressionRoom.Center;
        var cell = new Vector3Int(center.x - 3, center.y, 0);
        Vector3 safe = dungeon.ground.GetCellCenterWorld(cell);
        dungeon.player.GetComponent<Rigidbody2D>().position = safe;
        dungeon.player.position = safe;
    }

    static void SilenceRoomEnemies(DungeonGenerator.Room room)
    {
        foreach (var enemy in room.alive)
            if (enemy != null && enemy.enabled)
                foreach (var script in enemy.GetComponents<MonoBehaviour>())
                    if (script != enemy) script.enabled = false;
    }

    static Chest ProgressionReward(DungeonGenerator dungeon)
    {
        var center = dungeon.ProgressionRoom.Center;
        Vector3 reward = dungeon.ground.GetCellCenterWorld(new Vector3Int(center.x + 2, center.y, 0));
        return System.Array.Find(FindObjectsByType<Chest>(FindObjectsSortMode.None),
            item => Vector2.Distance(item.transform.position, reward) < .2f);
    }

    IEnumerator HostProgression(int clientId)
    {
        for (int floor = 3; floor <= 4; floor++)
        {
            var previous = DungeonGenerator.Instance;
            previous.NextFloor();
            yield return WaitFor(() => DungeonGenerator.Instance != null && DungeonGenerator.Instance != previous &&
                RunState.Floor == floor && Avatars >= 1, 15, "sala cooperativa al pis " + floor);
            if (failed) yield break;
        }

        var dungeon = DungeonGenerator.Instance;
        if (dungeon.ProgressionRoom == null) { Fail("falta la cambra al host"); yield break; }
        EnterProgressionRoom(dungeon);
        var room = dungeon.ProgressionRoom;
        yield return WaitFor(() => room.active, 8, "els dos jugadors activen la cambra");
        if (failed) yield break;
        float end = Time.realtimeSinceStartup + 17f;
        var session = NetSession.Instance;
        while (Time.realtimeSinceStartup < end)
        {
            SilenceRoomEnemies(room);
            if (session.Players.TryGetValue(clientId, out var client) && client.scene == NetSession.SceneToken() &&
                client.maxHealth > 0 && client.health == client.maxHealth - 1) break;
            yield return null;
        }
        if (Time.realtimeSinceStartup >= end) { Fail("el host no veu el dany del pols al client"); yield break; }
        Ok("el host veu el cor perdut pel client dins del pols");

        // v1.3.1: la Vareta curativa de l'amfitrió cura el cor que el client ha perdut
        {
            const System.Reflection.BindingFlags nonPublic = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            yield return new WaitForSecondsRealtime(1.5f); // perquè el client comprove abans el cor perdut
            var hostAttack = dungeon.player.GetComponent<PlayerAttack>();
            var wand = NetCoop.Weapon("VaretaCurativa");
            hostAttack.hotbarSlots[0] = wand; hostAttack.SelectSlot(0);
            Vector2 clientPosition = new Vector2(session.Players[clientId].x, session.Players[clientId].y);
            Vector2 hostSpot = clientPosition + Vector2.left * 2f;
            dungeon.player.GetComponent<Rigidbody2D>().position = hostSpot; dungeon.player.position = hostSpot;
            typeof(PlayerAttack).GetField("aimDirection", nonPublic).SetValue(hostAttack, Vector2.right);
            float healUntil = Time.realtimeSinceStartup + 6f; bool healed = false;
            while (Time.realtimeSinceStartup < healUntil && !healed)
            {
                SilenceRoomEnemies(room);
                typeof(PlayerAttack).GetMethod("TryHealAlly", nonPublic).Invoke(hostAttack, new object[] { wand });
                yield return new WaitForSecondsRealtime(0.3f);
                healed = session.Players.TryGetValue(clientId, out var after) && after.health == after.maxHealth;
            }
            if (!healed) { Fail("la Vareta curativa no cura el client online"); yield break; }
            Ok("la Vareta curativa cura el client online");
        }

        end = Time.realtimeSinceStartup + 22f;
        while (!room.cleared && Time.realtimeSinceStartup < end)
        {
            SilenceRoomEnemies(room);
            foreach (var enemy in room.alive.ToArray())
                if (enemy != null && enemy.enabled && !enemy.IsDead) enemy.TakeHit(9999, Vector2.zero);
            yield return new WaitForSeconds(.2f);
        }
        if (!room.cleared) { Fail("cambra cooperativa sense acabar"); yield break; }
        yield return new WaitForSecondsRealtime(1.5f);
        yield return CheckFloorReturn();
        if (failed) yield break;
        yield return HostRunRestart();
    }

    IEnumerator JoinProgression()
    {
        yield return WaitFor(() =>
        {
            var current = DungeonGenerator.Instance;
            return RunState.Floor == 4 && current != null && current.themes.Length >= 4 && current.Theme == current.themes[3];
        }, 25, "client amb el pis de ressonància carregat");
        if (failed) yield break;
        yield return null;
        yield return null;
        var dungeon = DungeonGenerator.Instance;
        if (dungeon.ProgressionRoom == null) { Fail("falta la cambra al client"); yield break; }
        EnterProgressionRoom(dungeon);
        GameObject mark = null;
        yield return WaitFor(() => (mark = GameObject.Find("Avis atac")) != null, 14, "el client rep el cercle del pols");
        if (failed) yield break;
        var health = dungeon.player.GetComponent<PlayerHealth>();
        var received = NetCoop.HurtsReceived;
        dungeon.player.GetComponent<Rigidbody2D>().position = mark.transform.position;
        dungeon.player.position = mark.transform.position;
        yield return WaitFor(() => NetCoop.HurtsReceived > received && health.currentHealth == health.maxHealth - 1,
            4, "el client rep un cor de dany del pols");
        if (failed) yield break;

        yield return WaitFor(() => dungeon.ProgressionRoom.cleared && ProgressionReward(dungeon) != null,
            24, "el client rep el cofre de la cambra");
        if (failed) yield break;
        var reward = ProgressionReward(dungeon);
        if (reward.lootTable.Length != 1 || reward.lootTable[0] != NetCoop.Weapon("ProjectorPolsos"))
            Fail("la recompensa del client difereix de la del host");
        if (failed) yield break;

        yield return WaitFor(() => RunState.Floor == 5 && DungeonGenerator.Instance != dungeon, 25, "el client ix pel portal blau");
        if (failed) yield break;
        var next = DungeonGenerator.Instance;
        var portal = FindFloorPortal(-1);
        if (portal == null || Vector2.Distance(next.player.position, portal.transform.position) > .1f)
        { Fail("el client no ha aparegut dins del portal blau"); yield break; }
        yield return WalkBackThroughPortal(next, portal);
        if (failed) yield break;
        yield return WaitFor(() => RunState.Floor == 4 && ProgressionReward(DungeonGenerator.Instance)?.IsOpen == true,
            15, "la colla torna al pis anterior amb el cofre obert");
        if (!failed)
            yield return WaitFor(() => FindReturnedWeapon(), 6, "el client recupera el boti compartit del pis anterior");
        if (!failed)
            yield return JoinRunRestart();
    }

    static DungeonPortal FindFloorPortal(int direction) => System.Array.Find(
        FindObjectsByType<DungeonPortal>(FindObjectsSortMode.None), p => p.Direction == direction);

    static bool FindReturnedWeapon() => System.Array.Exists(FindObjectsByType<ItemPickup>(FindObjectsSortMode.None),
        item => item.weapon == NetCoop.Weapon("Sniper") && Mathf.Approximately(item.CooldownRemaining, .55f));

    IEnumerator WalkBackThroughPortal(DungeonGenerator dungeon, DungeonPortal portal)
    {
        var position = portal.transform.position;
        yield return new WaitForSeconds(.65f);
        if (DungeonGenerator.Instance != dungeon) { Fail("el portal ha tornat abans d'eixir del cercle"); yield break; }
        SetBotPosition(dungeon, position + Vector3.right * 1.25f);
        yield return new WaitForSeconds(.4f);
        SetBotPosition(dungeon, position);
    }

    IEnumerator CheckFloorReturn()
    {
        var dungeon = DungeonGenerator.Instance;
        int floor = RunState.Floor;
        var boss = dungeon.Rooms.Find(room => room.type == DungeonGenerator.RoomType.Boss);
        dungeon.NetRoomState(dungeon.Rooms.IndexOf(boss), 2);
        var center = boss.Center;
        var exitCell = center + new Vector2Int(2, 0);
        dungeon.NetSpawnPortal(exitCell);
        NetCoop.PortalSpawned(exitCell);
        NetCoop.RestoreOpenedChest(ProgressionReward(dungeon));
        Vector3 drop = TileNav.Center(new Vector3Int(center.x - 2, center.y, 0));
        ItemPickup.Spawn(NetCoop.Weapon("Sniper"), drop, drop, cooldownRemaining: .55f);
        var exit = FindFloorPortal(1);
        Vector3 exitPosition = exit.transform.position;
        yield return null;
        SetBotPosition(dungeon, exitPosition);
        yield return WaitFor(() => RunState.Floor == floor + 1 && DungeonGenerator.Instance != dungeon,
            12, "la colla passa pel portal del boss");
        if (failed) yield break;
        var next = DungeonGenerator.Instance;
        var back = FindFloorPortal(-1);
        if (back == null || Vector2.Distance(next.player.position, back.transform.position) > .1f)
        { Fail("no s'ha aparegut dins del portal blau"); yield break; }
        if (!NetCoop.Online) yield return WalkBackThroughPortal(next, back);
        if (failed) yield break;
        yield return WaitFor(() => RunState.Floor == floor && DungeonGenerator.Instance != next && FindReturnedWeapon(),
            18, "tornada amb el boti del pis anterior");
        if (failed) yield break;
        var restored = DungeonGenerator.Instance;
        if (!restored.Rooms.Find(room => room.type == DungeonGenerator.RoomType.Boss).cleared ||
            ProgressionReward(restored)?.IsOpen != true || FindFloorPortal(1) == null ||
            Vector2.Distance(restored.player.position, exitPosition) > .1f)
            Fail("estat o punt d'arribada incorrecte al pis restaurat");
    }

    IEnumerator CheckProgression()
    {
        for (int floor = 2; floor <= 5; floor++)
        {
            var previous = DungeonGenerator.Instance;
            RunState.Reset();
            RunState.Active = true;
            RunState.Floor = floor;
            SceneManager.LoadScene("Dungeon");
            yield return WaitFor(() => DungeonGenerator.Instance != null && DungeonGenerator.Instance != previous, 10, "pis de prova " + floor);
            if (failed) yield break;
            yield return null;

            var dungeon = DungeonGenerator.Instance;
            var room = dungeon.ProgressionRoom;
            if ((room != null) != (floor >= 3))
            {
                Fail("la sala especial apareix al pis incorrecte");
                yield break;
            }
            if (room == null || floor == 3) continue;

            var health = dungeon.player.GetComponent<PlayerHealth>();
            var body = dungeon.player.GetComponent<Rigidbody2D>();
            EnterProgressionRoom(dungeon);
            Vector3 safe = dungeon.player.position;
            yield return WaitFor(() => room.active && room.alive.Exists(enemy => enemy != null && enemy.enabled), 8, "cambra amb enemics activa");
            if (failed) yield break;

            var previousTheme = dungeon.themes[floor - 2];
            bool elite = room.alive.Exists(enemy => enemy != null && System.Array.Exists(previousTheme.enemies, prefab =>
                prefab != null && enemy.name.StartsWith(prefab.name + "(") &&
                Mathf.Abs(enemy.transform.localScale.x) >= Mathf.Abs(prefab.transform.localScale.x) * 1.24f));
            if (!elite) { Fail("falta el reforç d'elit del tema anterior"); yield break; }

            // Aïllem el pols del dany de les IA en esta prova.
            float end = Time.realtimeSinceStartup + 14f;
            GameObject mark = null;
            while (mark == null && Time.realtimeSinceStartup < end)
            {
                SilenceRoomEnemies(room);
                mark = GameObject.Find("Avis atac");
                yield return null;
            }
            if (mark == null) { Fail("falta l'avís de ressonància"); yield break; }
            Vector3 pulse = mark.transform.position;
            if (Vector2.Distance(pulse, safe) <= 1.75f) { Fail("el refugi de prova està massa prop del pols"); yield break; }

            health.Heal(health.maxHealth);
            yield return new WaitForSeconds(1.3f);
            if (!health.IsFull) { Fail("un pols ha fet mal fora de l'àrea marcada"); yield break; }

            yield return WaitFor(() => (mark = GameObject.Find("Avis atac")) != null, 9, "segon cercle d'avís");
            if (failed) yield break;
            body.position = mark.transform.position;
            dungeon.player.position = mark.transform.position;
            yield return WaitFor(() => health.currentHealth == health.maxHealth - 1, 3, "un cor de dany dins del pols");
            if (failed) yield break;

            end = Time.realtimeSinceStartup + 24f;
            while (!room.cleared && Time.realtimeSinceStartup < end)
            {
                health.Heal(health.maxHealth);
                foreach (var enemy in room.alive.ToArray())
                    if (enemy != null && enemy.enabled && !enemy.IsDead) enemy.TakeHit(9999, Vector2.zero);
                yield return new WaitForSeconds(.2f);
            }
            var chest = ProgressionReward(dungeon);
            var weapon = NetCoop.Weapon(floor == 4 ? "ProjectorPolsos" : "BallestaCristall");
            if (!room.cleared || chest == null || weapon == null || chest.lootTable.Length != 1 || chest.lootTable[0] != weapon)
            {
                Fail("falta la recompensa de la cambra al pis " + floor);
                yield break;
            }
            Ok("elit, senyal, dany i recompensa comprovats al pis " + floor);
        }
    }

    // ---------- Captures (nomes amb -botShots <carpeta>, sense -nographics) ----------
    IEnumerator Shot(string label)
    {
        if (shots == null) yield break;
        yield return null; yield return null;
        Directory.CreateDirectory(shots);
        var cam = Camera.main; bool temp = false;
        if (cam == null) { cam = new GameObject("Camera captura").AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black; temp = true; }
        var rt = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
        var saved = new System.Collections.Generic.List<(Canvas c, RenderMode m, Camera w, float d, int layer, int order)>();
        var layers = SortingLayer.layers;
        int top = layers.Length > 0 ? layers[layers.Length - 1].id : 0;
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!c.isRootCanvas || c.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            saved.Add((c, c.renderMode, c.worldCamera, c.planeDistance, c.sortingLayerID, c.sortingOrder));
            // Com en Overlay: per damunt del mon i ordenats pel seu sortingOrder
            c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam;
            c.planeDistance = 1f + (1000 - Mathf.Clamp(c.sortingOrder, 0, 999)) * 0.001f;
            c.sortingLayerID = top; c.sortingOrder = 20000 + c.sortingOrder;
        }
        var prev = cam.targetTexture; cam.targetTexture = rt;
        yield return null; yield return null; yield return null;
        // En -batchmode les cameres no es dibuixen soles: ho demanem explicitament
        Canvas.ForceUpdateCanvases();
        var request = new UnityEngine.Rendering.RenderPipeline.StandardRequest { destination = rt };
        if (UnityEngine.Rendering.RenderPipeline.SupportsRenderRequest(cam, request)) UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam, request);
        else cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply();
        RenderTexture.active = null;
        string path = Path.Combine(shots, label + ".png");
        File.WriteAllBytes(path, tex.EncodeToPNG());
        cam.targetTexture = prev;
        foreach (var s in saved) if (s.c != null) { s.c.renderMode = s.m; s.c.worldCamera = s.w; s.c.planeDistance = s.d; s.c.sortingLayerID = s.layer; s.c.sortingOrder = s.order; }
        if (temp) Destroy(cam.gameObject);
        Destroy(rt); Destroy(tex);
        Ok("captura " + path);
    }
}


public static class NetBotLayout
{
    // Empremta de la masmorra generada: si coincideix entre amfitrio i jugador, tenen la mateixa masmorra
    public static string Fingerprint()
    {
        var g = DungeonGenerator.Instance;
        if (g == null) return "sense masmorra";
        unchecked
        {
            int h = 17;
            foreach (var r in g.Rooms) h = h * 31 + r.slot.x * 7 + r.slot.y * 13 + (int)r.type * 101 + r.rect.x * 3 + r.rect.y * 5 + r.rect.width * 11 + r.rect.height * 19;
            return g.Rooms.Count + " sales, empremta " + h.ToString("X8");
        }
    }
}
