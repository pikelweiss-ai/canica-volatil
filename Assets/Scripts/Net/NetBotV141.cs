using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// v1.4.1: prova automàtica del contingut nou (-canicaBot v141): sis enemics, quatre microbosses, mercader i màquina,
// pantalla Personalitzar i Almanac. Deixa captures a -botShots.
public partial class NetBot
{
    IEnumerator V141()
    {
        RunState.Reset(); RunState.Active = true; RunState.Seed = 141; RunState.Floor = 3;
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => Scene == "Dungeon" && DungeonGenerator.Instance != null, 15, "pis v1.4.1 carregat");
        if (failed) yield break;
        yield return null;
        var g = DungeonGenerator.Instance;
        var player = g.player; var body = player.GetComponent<Rigidbody2D>();
        var hp = player.GetComponent<PlayerHealth>();
        var start = g.Rooms.First(r => r.type == DungeonGenerator.RoomType.Start);
        Vector2 origin = g.ground.GetCellCenterWorld(new Vector3Int(start.Center.x, start.Center.y, 0));
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        void Heal() { hp.currentHealth = hp.maxHealth; }
        void Place(Vector2 at) { player.position = at; if (body != null) body.position = at; }
        GameObject Prefab(string id) { foreach (var e in catalog.enemies) if (e != null && e.name == id) return e; return null; }

        foreach (string id in new[] { "Salamandra", "LlimacAcid", "Escorpi", "LlopNeu", "Bombardier", "TorretaAntiga", "EscarabatDaurat", "UllFlotant", "GranotaGegant", "MagCendra" })
            if (Prefab(id) == null) { Fail("falta " + id + " al catàleg d'enemics"); yield break; }
        Ok("els 10 enemics nous són al catàleg");

        // ---- Salamandra: s'atura, encén un cercle de foc i fuig
        Place(origin + new Vector2(-1.5f, 0f));
        var salamanderGo = Instantiate(Prefab("Salamandra"), origin + new Vector2(2f, 0f), Quaternion.identity);
        var salamander = salamanderGo.GetComponent<EnemyV140>();
        for (float t = 0; t < 8f && salamander.Actions == 0; t += .1f) { Heal(); yield return new WaitForSeconds(.1f); }
        if (salamander.Actions == 0 || FindObjectsByType<HazardZone>(FindObjectsSortMode.None).Length == 0) { Fail("la Salamandra no crea el cercle de foc"); yield break; }
        yield return Shot("v141-salamandra");
        Destroy(salamanderGo); foreach (var z in FindObjectsByType<HazardZone>(FindObjectsSortMode.None)) Destroy(z.gameObject); Heal();
        Ok("Salamandra: colp i cercle de foc");

        // ---- Llimac àcid: deixa tolls i, en morir, es parteix en tres
        Place(origin + new Vector2(-3f, 0f));
        yield return null; // les zones de la Salamandra es destrueixen al final del fotograma
        var slugGo = Instantiate(Prefab("LlimacAcid"), origin + new Vector2(1f, 0f), Quaternion.identity);
        yield return new WaitForSeconds(.3f); // perquè arrenquen els seus components
        for (float t = 0; t < 6f && FindObjectsByType<HazardZone>(FindObjectsSortMode.None).Length == 0; t += .1f) { Heal(); yield return new WaitForSeconds(.1f); }
        if (FindObjectsByType<HazardZone>(FindObjectsSortMode.None).Length == 0) { Fail("el Llimac no deixa cap toll d'àcid"); yield break; }
        slugGo.GetComponent<EnemyHealth>().TakeDamage(999);
        yield return new WaitForSeconds(.6f);
        int pieces = FindObjectsByType<EnemyV140>(FindObjectsSortMode.None).Count(e => e.kind == EnemyV140.Kind.Slug && !e.GetComponent<EnemyHealth>().IsDead);
        if (pieces != 3) { Fail("el Llimac àcid no es parteix en 3 (" + pieces + ")"); yield break; }
        foreach (var e in FindObjectsByType<EnemyV140>(FindObjectsSortMode.None)) Destroy(e.gameObject);
        foreach (var z in FindObjectsByType<HazardZone>(FindObjectsSortMode.None)) Destroy(z.gameObject); Heal();
        Ok("Llimac àcid: tolls d'àcid i es parteix en tres");

        // ---- Escorpí: l'escut davanter para bales; per darrere no
        Place(origin + new Vector2(-1.5f, 0f));
        var scorpionGo = Instantiate(Prefab("Escorpi"), origin + new Vector2(3f, 0f), Quaternion.identity);
        var scorpionHealth = scorpionGo.GetComponent<EnemyHealth>();
        yield return new WaitForSeconds(.6f);
        Heal();
        int hpBefore = scorpionHealth.currentHealth;
        scorpionHealth.TakeProjectileHit(2, Vector2.right * 2f, ProjectileEffect.None);
        if (scorpionHealth.currentHealth != hpBefore) { Fail("l'Escorpí ha rebut una bala de front"); yield break; }
        scorpionHealth.TakeProjectileHit(2, Vector2.left * 2f, ProjectileEffect.None);
        if (scorpionHealth.currentHealth >= hpBefore) { Fail("l'Escorpí no rep bales per darrere"); yield break; }
        var scorpion = scorpionGo.GetComponent<EnemyV140>();
        Place(origin + new Vector2(1.5f, 0f));
        for (float t = 0; t < 6f && scorpion.Actions == 0; t += .1f) { Heal(); yield return new WaitForSeconds(.1f); }
        if (scorpion.Actions == 0) { Fail("l'Escorpí no colpeja amb la cua"); yield break; }
        Destroy(scorpionGo); Heal(); Ok("Escorpí: escut frontal i cua amb avís");

        // ---- Llops de neu: en manada, el líder udola i carreguen
        Place(origin + new Vector2(-3f, 0f));
        var wolves = new GameObject[3];
        for (int i = 0; i < 3; i++) wolves[i] = Instantiate(Prefab("LlopNeu"), origin + new Vector2(3f, -1f + i), Quaternion.identity);
        yield return new WaitForSeconds(.3f);
        int leaders = wolves.Count(w => (w.GetComponent<EnemyV140>().VisualState & 128) != 0);
        if (leaders != 1) { Fail("la manada de llops ha de tindre un líder (" + leaders + ")"); yield break; }
        for (float t = 0; t < 8f && wolves.Sum(w => w.GetComponent<EnemyV140>().Actions) == 0; t += .1f) { Heal(); yield return new WaitForSeconds(.1f); }
        if (wolves.Sum(w => w.GetComponent<EnemyV140>().Actions) == 0) { Fail("el líder dels llops no udola"); yield break; }
        yield return Shot("v141-llops");
        foreach (var w in wolves) Destroy(w); Heal(); Ok("Llop de neu: manada, líder i càrrega");

        // ---- Bombardier: bomba amb cercle roig
        Place(origin + new Vector2(-3f, 0f));
        var bomberGo = Instantiate(Prefab("Bombardier"), origin + new Vector2(2f, 0f), Quaternion.identity);
        var bomber = bomberGo.GetComponent<EnemyV140>();
        for (float t = 0; t < 8f && bomber.Actions == 0; t += .1f) { Heal(); yield return new WaitForSeconds(.1f); }
        if (bomber.Actions == 0) { Fail("el Bombardier no llança cap bomba"); yield break; }
        yield return Shot("v141-bomba");
        Destroy(bomberGo); foreach (var z in FindObjectsByType<HazardZone>(FindObjectsSortMode.None)) Destroy(z.gameObject); Heal();
        Ok("Bombardier: bomba amb cercle roig");

        // ---- Torreta antiga: dispara en línia recta
        Place(origin + new Vector2(-3f, 0f));
        var turretGo = Instantiate(Prefab("TorretaAntiga"), origin + new Vector2(1f, 0f), Quaternion.identity);
        var turret = turretGo.GetComponent<EnemyV140>();
        for (float t = 0; t < 4f && turret.Actions == 0; t += .1f) { Heal(); yield return new WaitForSeconds(.1f); }
        if (turret.Actions == 0 || FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None).Length == 0) { Fail("la Torreta antiga no dispara"); yield break; }
        Vector3 turretPos = turretGo.transform.position; yield return new WaitForSeconds(.5f);
        if (Vector3.Distance(turretPos, turretGo.transform.position) > .05f) { Fail("la Torreta antiga s'ha mogut"); yield break; }
        Destroy(turretGo); foreach (var b in FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None)) Destroy(b.gameObject); Heal();
        Ok("Torreta antiga: fixa i dispara en línies");

        // ---- Microbosses: barra de vida i atacs
        foreach (string id in new[] { "EscarabatDaurat", "UllFlotant", "GranotaGegant", "MagCendra" })
        {
            Place(origin + new Vector2(-3f, 0f));
            var microGo = Instantiate(Prefab(id), origin + new Vector2(2.5f, 0f), Quaternion.identity);
            var micro = microGo.GetComponent<EnemyV140>(); var microHealth = microGo.GetComponent<EnemyHealth>();
            yield return new WaitForSeconds(.3f);
            if (microGo.GetComponent<MicrobossBar>() == null) { Fail(id + " no té barra de vida"); yield break; }
            bool wasImmune = false, wasHit = false;
            for (float t = 0; t < 12f && (micro.Actions == 0 || !wasImmune); t += .1f)
            {
                Heal(); if (micro.Invulnerable) wasImmune = true;
                if (id == "GranotaGegant" || id == "MagCendra") wasImmune = true; // no tenen fase invulnerable pròpia
                yield return new WaitForSeconds(.1f);
            }
            if (micro.Actions == 0) { Fail(id + " no ataca"); yield break; }
            if (!wasImmune) { Fail(id + " no té la fase invulnerable"); yield break; }
            if (id == "MagCendra")
            {   // 9 impactes cauen a les orbes; el desè ja fa mal
                int full = microHealth.currentHealth;
                for (int i = 0; i < 9; i++) microHealth.TakeProjectileHit(2, Vector2.right * 2f, ProjectileEffect.None);
                if (microHealth.currentHealth != full) { Fail("les orbes del Mag no paren els colps"); yield break; }
                microHealth.TakeProjectileHit(2, Vector2.right * 2f, ProjectileEffect.None); wasHit = microHealth.currentHealth < full;
                if (!wasHit) { Fail("el Mag sense orbes no rep mal"); yield break; }
            }
            yield return Shot("v141-" + id);
            Destroy(microGo); foreach (var b in FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None)) Destroy(b.gameObject); Heal();
            Ok("Microboss " + id + ": barra de vida i atacs");
        }

        // ---- Mercader i màquina expenedora
        Place(origin + new Vector2(0f, -3f));
        foreach (var old in FindObjectsByType<Shop>(FindObjectsSortMode.None)) Destroy(old.gameObject); // els de la sala de tresor del pis
        yield return null;
        var merchant = Shop.Spawn(origin + new Vector2(2f, -3f), Shop.MerchantKind, 141);
        var vending = Shop.Spawn(origin + new Vector2(-2f, -3f), Shop.VendingKind, 141);
        if (merchant.slots.Length < 5 || vending.slots.Length != 3) { Fail("estoc incorrecte: " + merchant.slots.Length + "/" + vending.slots.Length); yield break; }
        if (CoinCounter.instance == null) { Fail("falta el comptador de monedes"); yield break; }
        CoinCounter.instance.currentCoins = 500; RunState.Coins = 500;
        ShopUI.Open(merchant); yield return new WaitForSeconds(.4f); yield return Shot("v141-mercader");
        int before = RunState.Coins; merchant.Buy(0); yield return new WaitForSeconds(.3f);
        if (!merchant.slots[0].sold || RunState.Coins >= before) { Fail("la compra al mercader no cobra ni marca l'article"); yield break; }
        merchant.Buy(0);
        if (RunState.Coins != before - merchant.slots[0].price) { Fail("comprar dues vegades el mateix article"); yield break; }
        ShopUI.CloseNow(); yield return null;
        ShopUI.Open(vending); yield return new WaitForSeconds(.4f); yield return Shot("v141-maquina"); ShopUI.CloseNow(); yield return null;
        if (GameControls.Editing) { Fail("la botiga deixa els controls bloquejats"); yield break; }
        Ok("Mercader i màquina: estoc, compra una sola vegada i interfície");

        // ---- Escut de mirall: un sol escut, 3 de mal per segon i barra d'energia de 5 s
        {
            var attackShield = player.GetComponent<PlayerAttack>();
            const System.Reflection.BindingFlags priv = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var aimShield = typeof(PlayerAttack).GetField("aimDirection", priv);
            var tickShield0 = typeof(PlayerAttack).GetMethod("TickAttack", priv);
            var tickEnergy = typeof(PlayerAttack).GetMethod("TickShieldEnergy", priv);
            attackShield.enabled = false; // el bot porta el temps de l escut a mà (si no, l Update normal el baixa cada fotograma)
            void Step(bool held) { tickShield0.Invoke(attackShield, new object[] { held }); tickEnergy.Invoke(attackShield, null); }
            attackShield.hotbarSlots[0] = NetCoop.Weapon("EscutMirall"); attackShield.SelectSlot(0);
            Place(origin + new Vector2(-2f, 0f)); aimShield.SetValue(attackShield, Vector2.right);
            var dummy = Instantiate(Prefab("TorretaAntiga"), origin + new Vector2(-.8f, 0f), Quaternion.identity);
            var dummyHealth = dummy.GetComponent<EnemyHealth>(); dummyHealth.maxHealth = 40; dummyHealth.currentHealth = 40;
            dummy.GetComponent<EnemyV140>().enabled = false;
            for (float t = 0; t < 1.3f; t += Time.deltaTime) { Step(true); yield return null; }
            if (!attackShield.ShieldIsUp) { Fail("l'escut no s'alça"); yield break; }
            if (dummyHealth.currentHealth > 40 - 3) { Fail("l'escut no fa mal a l'àrea: " + dummyHealth.currentHealth); yield break; }
            for (float t = 0; t < 6f && attackShield.ShieldEnergyFraction > .02f; t += Time.deltaTime) { Heal(); Step(true); yield return null; }
            if (attackShield.ShieldEnergyFraction > .05f) { Fail("l'escut no gasta energia: " + attackShield.ShieldEnergyFraction); yield break; }
            for (float t = 0; t < .6f; t += Time.deltaTime) { Step(true); yield return null; } // acaba de gastar-se i es baixa sol
            if (attackShield.ShieldIsUp) { Fail("l'escut continua alçat sense energia"); yield break; }
            for (float t = 0; t < 1.2f; t += Time.deltaTime) { Step(false); yield return null; }
            if (attackShield.ShieldEnergyFraction < .2f) { Fail("l'escut no es recarrega"); yield break; }
            attackShield.enabled = true; attackShield.hotbarSlots[0] = null; Destroy(dummy); Heal();
            Ok("Escut de mirall: energia, mal a l'àrea i un sol escut");
        }

        // ---- Rei de Gel: subdits i patrons nous
        {
            var iceKingPrefab = Prefab("ReiGel").GetComponent<EnemyBoss>();
            if (!iceKingPrefab.attacks.Contains(EnemyBoss.Attack.Summon) || !iceKingPrefab.attacks.Contains(EnemyBoss.Attack.Snowflake) ||
                !iceKingPrefab.attacks.Contains(EnemyBoss.Attack.IceCone) || !iceKingPrefab.attacks.Contains(EnemyBoss.Attack.SlowSpiral) || iceKingPrefab.maxMinions <= 0)
            { Fail("el Rei de Gel no té els atacs nous"); yield break; }
            Place(origin + new Vector2(-3f, 0f));
            var king = Instantiate(Prefab("ReiGel"), origin + new Vector2(3f, 0f), Quaternion.identity);
            var kingBoss = king.GetComponent<EnemyBoss>();
            yield return new WaitForSeconds(.3f);
            var run = typeof(EnemyBoss).GetMethod("Snowflake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            king.GetComponent<EnemyHealth>().currentHealth = king.GetComponent<EnemyHealth>().maxHealth;
            kingBoss.StartCoroutine((IEnumerator)run.Invoke(kingBoss, null));
            yield return new WaitForSeconds(1.2f);
            int bullets = FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None).Length;
            if (bullets < 20) { Fail("el floc de neu no llança boles (" + bullets + ")"); yield break; }
            yield return Shot("v141-floc-de-neu");
            foreach (var b in FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None)) Destroy(b.gameObject);
            // detector d'encallament: l'eixida no ha de petar ni deixar el boss a un lloc ocupat
            var unstick = typeof(EnemyBoss).GetMethod("Unstick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            unstick.Invoke(kingBoss, null); yield return null;
            Destroy(king); Heal();
            Ok("Rei de Gel: subdits, patrons nous i eixida d'encallaments");
        }

        // ---- Reina Aranya: sense dash i amb teranyines que frenen
        var queenPrefab = Prefab("ReinaAranya");
        if (queenPrefab == null || !queenPrefab.GetComponent<EnemyBoss>().attacks.Contains(EnemyBoss.Attack.Web) || queenPrefab.GetComponent<EnemyBoss>().attacks.Contains(EnemyBoss.Attack.Charge))
        { Fail("la Reina Aranya ha de tindre Web i no Charge"); yield break; }
        Place(origin + new Vector2(0f, 2f));
        HazardZone.Spawn(HazardZone.Style.Web, origin + new Vector2(0f, 2f));
        yield return new WaitForSeconds(.4f); yield return Shot("v141-teranyina");
        var webZone = FindObjectsByType<HazardZone>(FindObjectsSortMode.None).FirstOrDefault(z => z.GetComponent<Breakable>() != null);
        if (webZone == null) { Fail("la teranyina no es pot trencar"); yield break; }
        var webBreakable = webZone.GetComponent<Breakable>();
        webBreakable.Hit(1); webBreakable.Hit(1); webBreakable.Hit(1);
        yield return null;
        if (webZone != null) { Fail("la teranyina no desapareix en trencar-se"); yield break; }
        Ok("Reina Aranya: teranyines i sense dash");

        // ---- Teranyina de racó (v1.4.2): trencable i frenadora
        {
            var cobweb = new GameObject("Teranyina de prova", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Breakable), typeof(CobwebSlow));
            cobweb.transform.position = origin + new Vector2(0f, 4f); cobweb.GetComponent<CircleCollider2D>().isTrigger = true;
            var cobwebBreakable = cobweb.GetComponent<Breakable>(); cobwebBreakable.health = 2;
            yield return null;
            cobwebBreakable.Hit(1); cobwebBreakable.Hit(1);
            yield return null;
            if (cobweb != null) { Fail("la teranyina de racó no es trenca"); yield break; }
            Ok("Teranyina de racó: es trenca i frena");
        }

        // ---- Menú: Personalitzar i Almanac
        SceneManager.LoadScene("MainMenu");
        yield return WaitFor(() => Scene == "MainMenu", 20, "menú principal");
        if (failed) yield break;
        yield return new WaitForSeconds(1.2f);
        var controller = FindFirstObjectByType<MainMenuController>();
        controller.OpenCustomize(); yield return new WaitForSeconds(2.2f); yield return Shot("v141-personalitzar");
        var customize = FindFirstObjectByType<CustomizeMenuUI>(); customize.Hide();
        AlmanacUI.Open(); yield return new WaitForSeconds(.5f); yield return Shot("v141-almanac");
        AlmanacUI.Open(); // torna a tancar
        Ok("Personalitzar i Almanac s'obrin");
        Pass();
    }
}

// v1.4.1: una captura d'una sala de combat de cada tema, per a revisar la varietat de blocs
public partial class NetBot
{
    IEnumerator Scenery141()
    {
        for (int floorNumber = 1; floorNumber <= 8; floorNumber++)
        {
            RunState.Reset(); RunState.Active = true; RunState.Seed = 77; RunState.Floor = floorNumber;
            SceneManager.LoadScene("Dungeon");
            yield return WaitFor(() => Scene == "Dungeon" && DungeonGenerator.Instance != null, 15, "pis " + floorNumber);
            if (failed) yield break;
            yield return new WaitForSeconds(.5f);
            var g = DungeonGenerator.Instance; var player = g.player; var body = player.GetComponent<Rigidbody2D>();
            var room = g.Rooms.FirstOrDefault(r => r.type == DungeonGenerator.RoomType.Combat && r.rect.width * r.rect.height > 80) ?? g.Rooms.First(r => r.type == DungeonGenerator.RoomType.Combat);
            player.position = g.ground.GetCellCenterWorld(new Vector3Int(room.Center.x, room.Center.y, 0)); body.position = player.position;
            player.GetComponent<PlayerHealth>().currentHealth = 99;
            yield return new WaitForSeconds(.6f);
            yield return Shot("escenari-pis" + floorNumber);
        }
        Ok("captures dels vuit temes"); Pass();
    }
}
