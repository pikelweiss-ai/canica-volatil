using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public partial class NetBot
{
    IEnumerator Phase0()
    {
        foreach (int floor in new[] { 2, 5 })
        {
            RunState.Reset(); RunState.Active = true; RunState.Seed = 150; RunState.Floor = floor;
            var previous = DungeonGenerator.Instance;
            SceneManager.LoadScene("Dungeon");
            yield return WaitFor(() => DungeonGenerator.Instance != null && DungeonGenerator.Instance != previous, 15, "pis " + floor);
            if (failed) yield break;
            var dungeon = DungeonGenerator.Instance;
            if (dungeon.MicrobossRoom == null || dungeon.Theme.microbosses.Length == 0)
            { Fail("falta sala de microboss al pis " + floor); yield break; }

            if (floor == 2)
            {
                var start = dungeon.Rooms.First(r => r.type == DungeonGenerator.RoomType.Start);
                Vector3 center = dungeon.ground.GetCellCenterWorld((Vector3Int)start.Center);
                var merchant = Shop.Spawn(center + new Vector3(9, 10), Shop.MerchantKind, 150);
                var vending = Shop.Spawn(center + new Vector3(11, 10), Shop.VendingKind, 150);
                foreach (var shop in new[] { merchant, vending })
                    foreach (var slot in shop.slots.Where(s => s.heal != null))
                    {
                        string name = slot.heal.itemName.ToLowerInvariant();
                        int basePrice = name.StartsWith("poma") ? 10 : name.StartsWith("bena") ? 15 :
                            name.StartsWith("poci") ? 26 : name.StartsWith("farma") ? 45 : 12 + 8 * (int)slot.heal.rarity;
                        int expected = Mathf.RoundToInt(basePrice * 1.2f);
                        if (slot.offer) expected = Mathf.RoundToInt(expected * .7f);
                        if (slot.price != expected) { Fail("preu incoherent: " + slot.title); yield break; }
                    }
                Ok("cures al mateix preu als dos estocs, descomptes a banda");

                ShopSlot surprise = null;
                for (int seed = 1; seed <= 100 && surprise == null; seed++)
                {
                    var shop = Shop.Spawn(center + new Vector3(9, 10), Shop.MerchantKind, seed);
                    surprise = shop.slots.FirstOrDefault(slot => slot.surprise);
                    Destroy(shop.gameObject);
                }
                if (surprise == null || surprise.weapon == null || surprise.icon != null ||
                    surprise.title == surprise.weapon.weaponName || surprise.rarity != ChestRarity.Comu)
                { Fail("la sorpresa no amaga l'arma"); yield break; }
                Ok("sorpresa amb nom generic, interrogant i vora neutra");

                var trunks = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                    .Where(sprite => sprite.gameObject.name.StartsWith("Objecte Tronc")).ToArray();
                if (trunks.Length == 0 || trunks.Any(sprite => !sprite.name.StartsWith("Objecte Tronc_caigut") ||
                        !sprite.TryGetComponent<BoxCollider2D>(out var box) || box.size != Vector2.one ||
                        sprite.GetComponent<Breakable>() != null || !TileNav.IsBlockedWorld(sprite.transform.position)))
                { Fail("falta el tronc tombat solid d'una casella"); yield break; }
                Ok("troncs tombats, hitbox sencera i indestructibles");
            }

            MovePhase0Player(dungeon.ground.GetCellCenterWorld((Vector3Int)dungeon.MicrobossRoom.Center));
            yield return WaitFor(() => dungeon.MicrobossRoom.alive.Any(IsPhase0Microboss), 8, "microboss real del pis " + floor);
            if (failed) yield break;
            if (dungeon.MicrobossRoom.alive.Count(IsPhase0Microboss) != 1)
            { Fail("hi ha mes d'un microboss a la primera onada"); yield break; }
            yield return Shot("fase0-microboss-pis-" + floor);
        }

        yield return Phase0Secret();
        if (failed) yield break;

        SceneManager.LoadScene(CreativeMode.Scene);
        yield return WaitFor(() => CreativeMode.Instance != null, 15, "mapa lliure i mode creatiu");
        if (failed) yield break;
        var slug = SpawnPhase0Enemy("LlimacAcid", new Vector2(2, 2));
        yield return null;
        slug.GetComponent<EnemyHealth>().TakeDamage(999);
        yield return WaitFor(() => FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)
            .Count(enemy => !enemy.IsDead && enemy.name.StartsWith("LlimacAcid")) == 3, 4, "el llimac crea tres fills");
        if (failed) yield break;
        typeof(CreativeMode).GetMethod("ClearEnemies", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(CreativeMode.Instance, null);
        yield return null;
        if (FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Any(enemy => !enemy.IsDead))
        { Fail("el Creatiu encara deixa enemics vius"); yield break; }
        Ok("TREURE ENEMICS lleva fills que no venen del menu");
        Pass();
    }

    IEnumerator Phase0Secret()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            RunState.Reset(); RunState.Active = true; RunState.Seed = seed; RunState.Floor = 3;
            var previous = DungeonGenerator.Instance;
            SceneManager.LoadScene("Dungeon");
            yield return WaitFor(() => DungeonGenerator.Instance != null && DungeonGenerator.Instance != previous, 12, "mapa per provar la sala secreta");
            if (failed) yield break;
            var dungeon = DungeonGenerator.Instance;
            var room = dungeon.Rooms.FirstOrDefault(r => r.type == DungeonGenerator.RoomType.Secreta);
            if (room == null) continue;
            Vector2 direction = room.slot - room.links[0].slot;
            var wall = FindObjectsByType<Breakable>(FindObjectsSortMode.None).FirstOrDefault(b => b.name == "Paret esquerdada");
            if (wall == null || GameObject.Find("Pista sala secreta") != null)
            { Fail("la sala secreta ha de tindre paret i cap cartell"); yield break; }

            Vector2 from = (Vector2)wall.transform.position - direction * 1.25f;
            MovePhase0Player(from);
            FirePhase0Shot(from, direction);
            yield return WaitFor(() => wall == null, 3, "una bala de pistola trenca la paret secreta");
            if (failed) yield break;
            yield return Shot("fase0-sala-secreta");
            yield break;
        }
        Fail("cap sala secreta trobada per a la prova");
    }

    static void FirePhase0Shot(Vector2 from, Vector2 direction)
    {
        var player = GameObject.FindWithTag("Player").transform;
        Projectile.Spawn(from, direction, NetCoop.Weapon("Pistola"), player, 0, 0);
    }

    static Breakable Phase0OnlineWall(Vector3 origin)
    {
        var wall = new GameObject("Paret esquerdada (prova)");
        wall.transform.position = origin + Vector3.down * 3;
        wall.AddComponent<BoxCollider2D>();
        var prop = wall.AddComponent<Breakable>();
        prop.health = 1;
        prop.coinChance = prop.healChance = 0f;
        return prop;
    }

    static bool IsPhase0Microboss(EnemyHealth enemy) =>
        enemy != null && enemy.TryGetComponent<EnemyV140>(out var behavior) && behavior.IsMicroboss;

    static void MovePhase0Player(Vector3 at)
    {
        var player = GameObject.FindWithTag("Player");
        player.transform.position = at;
        if (player.TryGetComponent<Rigidbody2D>(out var body)) body.position = at;
    }

    static GameObject SpawnPhase0Enemy(string id, Vector2 at)
    {
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        return Instantiate(catalog.enemies.First(enemy => enemy != null && enemy.name == id), at, Quaternion.identity);
    }

    static Vector3 Phase0Origin()
    {
        var dungeon = DungeonGenerator.Instance;
        var start = dungeon.Rooms.First(room => room.type == DungeonGenerator.RoomType.Start);
        return dungeon.ground.GetCellCenterWorld((Vector3Int)start.Center);
    }

    static DamageTile Phase0Liquid(Vector3 origin)
    {
        var map = DungeonGenerator.Instance.liquid;
        var tile = ScriptableObject.CreateInstance<DamageTile>();
        tile.colliderType = Tile.ColliderType.Grid;
        map.SetTile(map.WorldToCell(origin + Vector3.up * 3), tile);
        return tile;
    }

    IEnumerator HostPhase0()
    {
        Vector3 origin = Phase0Origin();
        Phase0Liquid(origin);
        var wall = Phase0OnlineWall(origin);
        MovePhase0Player(origin + Vector3.down * 2);
        var mimic = SpawnPhase0Enemy("Mimic", origin + Vector3.left * 3);
        var mole = SpawnPhase0Enemy("Talp", origin + Vector3.right * 3);
        var host = NetSession.Instance;
        bool ready = false, done = false;
        void ReadBotLine(NetSession.ChatLine line)
        {
            if (line.system) return;
            if (line.text == "fase0 bot llest") ready = true;
            if (line.text == "fase0 bot fi") done = true;
        }
        host.ChatReceived += ReadBotLine;
        yield return WaitFor(() => ready, 25, "client ha vist el Mímic disfressat i l'ombra del Talp");
        if (failed) yield break;

        mimic.GetComponent<EnemyV140>().Reveal();
        var turret = SpawnPhase0Enemy("TorretaAntiga", origin + new Vector3(-2, 3));
        yield return WaitFor(() => done, 25, "client veu el Mímic despert i la bala de la Torreta travessa el liquid");
        host.ChatReceived -= ReadBotLine;
        if (failed) yield break;
        if (wall != null) { Fail("el colp del client no ha trencat la paret a l\u0027amfitrio"); yield break; }
        foreach (var go in new[] { mimic, mole, turret })
        {
            NetCoop.EnemyDied(go.GetComponent<EnemyHealth>());
            Destroy(go);
        }
    }

    IEnumerator JoinPhase0()
    {
        Vector3 origin = Phase0Origin();
        Phase0Liquid(origin);
        var wall = Phase0OnlineWall(origin);
        MovePhase0Player(origin + Vector3.down * 2);
        EnemyV140 mimic = null, mole = null;
        yield return WaitFor(() =>
        {
            var enemies = NetCoop.Enemies.Where(h => h != null).Select(h => h.GetComponent<EnemyV140>());
            mimic = enemies.FirstOrDefault(e => e != null && e.kind == EnemyV140.Kind.Mimic);
            mole = enemies.FirstOrDefault(e => e != null && e.kind == EnemyV140.Kind.Mole);
            return mimic != null && mole != null;
        }, 12, "repliques online del Mímic i del Talp");
        if (failed) yield break;
        if (mimic.GetComponent<SpriteRenderer>().sprite != mimic.disguise)
        { Fail("el Mímic es veu com una criatura abans de despertar"); yield break; }
        var shadow = mole.transform.Find("Ombra del talp").GetComponent<SpriteRenderer>();
        yield return WaitFor(() => shadow.enabled, 12, "el client mostra l'ombra del Talp");
        if (failed) yield break;
        if (!mole.MoleUnderground || mole.GetComponent<SpriteRenderer>().color.a != 0f ||
            Vector2.Distance(shadow.transform.position, mole.Aim) > .01f ||
            mole.GetComponentsInChildren<Collider2D>().Any(collider => !collider.enabled))
        { Fail("estat enterrat, ombra, punteria o colliders de la copia del Talp"); yield break; }

        var session = NetSession.Instance;
        bool chat = false;
        void ReadBotLine(NetSession.ChatLine line) { if (line.text.Contains("bafarada fase0")) chat = true; }
        session.ChatReceived += ReadBotLine;
        var shop = Shop.Spawn(origin + Vector3.one * 8, Shop.MerchantKind, 150);
        shop.Say("bafarada fase0", true);
        if (shop.GetComponentInChildren<ChatBubble>() == null || chat)
        { Fail("el mercader no parla exclusivament amb bafarada"); yield break; }
        session.SendChat("fase0 bot llest");
        yield return WaitFor(() => mimic.GetComponent<SpriteRenderer>().sprite != mimic.disguise, 10, "el client veu el Mímic revelat");
        if (failed) yield break;
        yield return WaitFor(() => FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None).Any(bullet =>
            Mathf.Abs(bullet.transform.position.y - (origin.y + 3)) < .1f &&
            bullet.transform.position.x > origin.x + .65f), 10, "la bala replicada creua el liquid que fa mal");
        if (failed) yield break;
        FirePhase0Shot(origin + Vector3.down * 1.75f, Vector2.down);
        yield return WaitFor(() => wall == null, 5, "el client trenca la paret amb una bala i rep la confirmacio");
        if (failed) yield break;
        session.ChatReceived -= ReadBotLine;
        if (chat) { Fail("la bafarada del mercader ha entrat al xat"); yield break; }
        Ok("mercader amb bafarada local i sense missatge al xat");
        session.SendChat("fase0 bot fi");
    }
}

