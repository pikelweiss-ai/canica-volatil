using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class NetBot
{
    const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    IEnumerator CheckHeldAttack()
    {
        var player = DungeonGenerator.Instance.player;
        var attack = player.GetComponent<PlayerAttack>();
        var slot = FindFirstObjectByType<SlotInput>();
        if (slot == null) { Fail("no hi ha cap casella interactiva per provar el HUD"); yield break; }

        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)slot.transform;
        var canvas = slot.GetComponentInParent<Canvas>().rootCanvas;
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 hud = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        Vector2 world = new Vector2(Screen.width * .57f, Screen.height * .52f);
        var read = typeof(PlayerAttack).GetMethod("ReadDesktopAttack", PrivateInstance);
        var tick = typeof(PlayerAttack).GetMethod("TickAttack", PrivateInstance);
        var cooldown = typeof(PlayerAttack).GetField("nextAttackTime", PrivateInstance);
        bool Read(bool start, bool held, Vector2 pointer) =>
            (bool)read.Invoke(attack, new object[] { start, held, pointer, true, false });

        var savedWeapon = attack.hotbarSlots[0];
        attack.enabled = false;
        try
        {
            foreach (string name in new[] { "AK47", "Pistola" })
            {
                var weapon = NetCoop.Weapon(name);
                attack.hotbarSlots[0] = weapon;
                attack.SelectSlot(0);
                cooldown.SetValue(attack, 0f);
                if (!Read(true, true, world)) { Fail("el clic al món no arma el tret"); yield break; }
                tick.Invoke(attack, new object[] { true });
                float first = (float)cooldown.GetValue(attack);
                if (first <= Time.time || !Read(false, true, hud))
                {
                    Fail("el tret s'atura en arrossegar el punter sobre el HUD");
                    yield break;
                }
                yield return new WaitForSeconds(weapon.cooldown + .05f);
                tick.Invoke(attack, new object[] { Read(false, true, hud) });
                if ((float)cooldown.GetValue(attack) <= first)
                {
                    Fail("el tret s'atura amb el punter quiet al HUD: " + name);
                    yield break;
                }
                if (Read(false, false, hud) || Read(true, true, hud) ||
                    Read(false, true, world) || Read(false, false, world) || !Read(true, true, world))
                {
                    Fail("la casella del HUD no conserva el clic que hi comença");
                    yield break;
                }
                Read(false, false, world);
                Ok(name + " dispara en arrossegar i mantindre el clic sobre una casella del HUD");
            }
        }
        finally
        {
            attack.hotbarSlots[0] = savedWeapon;
            cooldown.SetValue(attack, 0f);
            attack.SelectSlot(0);
            attack.enabled = true;
        }
    }

    static void SetBotPosition(DungeonGenerator dungeon, Vector3 position)
    {
        dungeon.player.GetComponent<Rigidbody2D>().position = position;
        dungeon.player.position = position;
    }

    static Vector3 RecoveryPosition(DungeonGenerator dungeon, int offset)
    {
        var center = dungeon.Rooms[0].Center;
        return dungeon.ground.GetCellCenterWorld(new Vector3Int(center.x + offset, center.y, 0));
    }

    static NetItem[] InventoryDrops(int ownerId)
    {
        return Array.FindAll(FindObjectsByType<NetItem>(FindObjectsSortMode.None),
            item => item.id / 1000000 == ownerId && (item.kind == NetShared.Kind.Weapon || item.kind == NetShared.Kind.Heal));
    }

    static bool HasBothDropKinds(NetItem[] drops)
    {
        return Array.Exists(drops, drop => drop != null && drop.kind == NetShared.Kind.Weapon) &&
               Array.Exists(drops, drop => drop != null && drop.kind == NetShared.Kind.Heal);
    }

    IEnumerator HostDeathRecovery(int clientId)
    {
        var dungeon = DungeonGenerator.Instance;
        var clientPosition = RecoveryPosition(dungeon, 3);
        var session = NetSession.Instance;
        yield return WaitFor(() => session.Players.TryGetValue(clientId, out var ready) && ready.scene == NetSession.SceneToken() &&
            Vector2.Distance(new Vector2(ready.x, ready.y), clientPosition) < .4f, 12, "client apartat al pis 2");
        if (failed) yield break;

        // Dues armes damunt de la mateixa casella amb només una plaça disponible al client.
        SetBotPosition(dungeon, RecoveryPosition(dungeon, -2));
        var first = ItemPickup.Spawn(NetCoop.Weapon("AK47"), clientPosition, clientPosition);
        var second = ItemPickup.Spawn(NetCoop.Weapon("Pistola"), clientPosition, clientPosition);
        yield return WaitFor(() => first == null || second == null, 12, "el host veu que el client recull una de les dues armes");
        if (failed) yield break;
        yield return WaitFor(() => first == null && second == null, 12, "les dues recollides es repliquen quan torna a haver-hi lloc");
        if (failed) yield break;

        var attack = dungeon.player.GetComponent<PlayerAttack>();
        var bag = dungeon.player.GetComponent<HealInventory>();
        var health = dungeon.player.GetComponent<PlayerHealth>();
        var weapon = NetCoop.Weapon("Escopeta");
        HealItem heal = HealItem.All[0];
        Array.Clear(attack.hotbarSlots, 0, attack.hotbarSlots.Length);
        Array.Clear(bag.items, 0, bag.items.Length);
        Array.Clear(bag.counts, 0, bag.counts.Length);
        if (!attack.AddWeapon(weapon) || !bag.Add(heal)) { Fail("l'amfitrió no ha pogut preparar l'inventari de prova"); yield break; }

        var room = dungeon.Rooms[0].rect;
        var corner = new Vector3Int(room.xMax - 1, room.yMax - 1, 0);
        Vector3 deathPosition = dungeon.ground.GetCellCenterWorld(corner);
        if (!TileNav.CircleFree(deathPosition, .12f) ||
            !TileNav.IsBlocked(corner + Vector3Int.right) || !TileNav.IsBlocked(corner + Vector3Int.up))
        {
            Fail("la sala inicial no té la cantonada lliure necessària per a la prova");
            yield break;
        }
        SetBotPosition(dungeon, deathPosition);
        health.TakeDamage(999);
        yield return WaitFor(() => Spectate.Active && HasBothDropKinds(InventoryDrops(session.LocalId)),
            4, "l'amfitrió cau amb arma i cura visibles al terra");
        if (failed) yield break;
        var drops = InventoryDrops(session.LocalId);
        if (Array.Exists(drops, drop => !TileNav.CircleFree(drop.destination, .12f) ||
            !TileNav.LineOfSight(deathPosition, drop.destination, .12f)))
        {
            Fail("una arma o una cura ha travessat la cantonada");
            yield break;
        }
        Ok("l'arma i la cura llançades en morir queden dins de la cantonada");
        yield return WaitFor(() => health.gameObject.activeInHierarchy && health.IsFull && !Spectate.Active,
            10, "l'amfitrió reapareix amb vida plena");
        if (failed) yield break;

        foreach (var drop in drops)
        {
            if (drop == null) continue;
            SetBotPosition(dungeon, drop.destination);
            yield return WaitFor(() => drop == null, 4, "l'amfitrió recupera un objecte propi");
            if (failed) yield break;
        }
        if (!Array.Exists(attack.hotbarSlots, w => w == weapon) || !Array.Exists(bag.items, item => item == heal))
        {
            Fail("el botí propi de l'amfitrió no ha tornat a l'inventari");
            yield break;
        }

        // El company confirma la retirada dels objectes abans que l'escena canvie.
        float afterRecovery = Time.realtimeSinceStartup;
        yield return WaitFor(() => Time.realtimeSinceStartup >= afterRecovery + .5f &&
            session.Players.TryGetValue(clientId, out var peer) &&
            Vector2.Distance(new Vector2(peer.x, peer.y), clientPosition + Vector3.up) < .4f,
            10, "el company ha confirmat que ja no veu el botí de l'amfitrió");
    }

    IEnumerator ObserveHostRecovery()
    {
        var dungeon = DungeonGenerator.Instance;
        var attack = dungeon.player.GetComponent<PlayerAttack>();
        var referenceWeapon = NetCoop.Weapon("Escopeta");
        var clientPosition = RecoveryPosition(dungeon, 3);
        for (int i = 0; i < attack.hotbarSlots.Length - 1; i++) attack.hotbarSlots[i] = referenceWeapon;
        attack.SelectSlot(0);
        SetBotPosition(dungeon, clientPosition);

        yield return WaitFor(() => !attack.HasFreeSlot, 16, "el client ompli l'última casella amb una arma");
        if (failed) yield break;
        yield return new WaitForSecondsRealtime(.4f);
        int ownerId = 1; // El protocol assigna 1 a l'amfitrió.
        var spare = InventoryDrops(ownerId);
        if (spare.Length != 1 || spare[0].kind != NetShared.Kind.Weapon)
        {
            Fail("amb la hotbar plena ha de quedar exactament una arma compartida al terra");
            yield break;
        }

        Ok("el segon objecte queda disponible quan s'ompli la hotbar");
        attack.hotbarSlots[attack.hotbarSlots.Length - 1] = null;
        yield return WaitFor(() => !attack.HasFreeSlot && spare[0] == null, 5, "la segona arma es recull en alliberar una casella");
        if (failed) yield break;
        Array.Clear(attack.hotbarSlots, 0, attack.hotbarSlots.Length);
        attack.SelectSlot(0);

        yield return WaitFor(() => HasBothDropKinds(InventoryDrops(ownerId)), 10, "el client veu l'arma i la cura de l'amfitrió caigut");
        if (failed) yield break;
        var drops = InventoryDrops(ownerId);
        yield return WaitFor(() => Array.TrueForAll(drops, drop => drop == null), 16, "el botí recuperat per l'amfitrió desapareix al client");
        if (failed) yield break;
        SetBotPosition(dungeon, clientPosition + Vector3.up);
    }

    IEnumerator TrailGallery()
    {
        if (shots == null) { Fail("falta -botShots per guardar la galeria"); yield break; }
        var previous = SceneManager.GetActiveScene();
        var gallery = SceneManager.CreateScene("Proves de rastre");
        SceneManager.SetActiveScene(gallery);
        yield return SceneManager.UnloadSceneAsync(previous);

        var camera = new GameObject("Camera de la galeria", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 6.15f;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.05f, .075f, .095f);

        var db = CosmeticsDatabase.Instance;
        for (int remotePass = 0; remotePass < 2; remotePass++)
        {
            bool showRemote = remotePass == 1;
            string prefix = showRemote ? "client" : "local";
            var objects = new GameObject("Mostres " + prefix);
            var reference = new GameObject("Referència", typeof(SpriteRenderer));
            reference.transform.SetParent(objects.transform);
            var sampleBody = reference.GetComponent<SpriteRenderer>();
            sampleBody.sprite = db.playerSprite;
            sampleBody.sortingLayerName = "Player";
            sampleBody.enabled = false;

            var title = GalleryLabel(objects.transform, showRemote ? "ESTELES · VISTA DEL CLIENT" : "ESTELES · JUGADOR LOCAL",
                new Vector3(0, 5.5f), 4.1f);
            title.alignment = TextAlignmentOptions.Center;
            var avatars = new Transform[db.trails.Length];
            var remotePlayers = new NetPlayer[db.trails.Length];
            var origins = new Vector3[db.trails.Length];
            int maxRows = (db.trails.Length + 1) / 2;
            for (int i = 0; i < db.trails.Length; i++)
            {
                int column = i / maxRows;
                int row = i % maxRows;
                float x = column == 0 ? -9.7f : .75f;
                float y = 4.5f - row * 1.20f;
                var start = new Vector3(x + 4.25f, y, 0);
                origins[i] = start;
                GalleryLabel(objects.transform, db.trails[i].name, new Vector3(x, y, 0), 2.75f);

                if (showRemote)
                {
                    var pl = new NetPlayer { id = i + 100, name = "", trail = i, x = start.x, y = start.y, lastPos = Time.realtimeSinceStartupAsDouble };
                    var avatar = RemoteAvatar.Create(pl, reference);
                    avatars[i] = avatar.transform;
                    remotePlayers[i] = pl;
                }
                else
                {
                    var avatar = new GameObject("Mostra local", typeof(SpriteRenderer));
                    avatar.transform.position = start;
                    var sprite = avatar.GetComponent<SpriteRenderer>();
                    sprite.sprite = db.playerSprite;
                    sprite.sortingLayerName = "Player";
                    var cosmetics = avatar.AddComponent<PlayerCosmetics>();
                    cosmetics.skinOverride = 0;
                    cosmetics.hatOverride = 0;
                    cosmetics.trailOverride = i;
                    avatars[i] = avatar.transform;
                }
                avatars[i].SetParent(objects.transform);
            }

            yield return null;
            IEnumerator Move(Vector3 from, Vector3 to)
            {
                const float duration = .63f;
                float started = Time.time;
                float progress = 0f;
                while (progress < 1f)
                {
                    progress = Mathf.Clamp01((Time.time - started) / duration);
                    PlaceAvatars(Vector3.Lerp(from, to, progress));
                    yield return null;
                }
            }

            void PlaceAvatars(Vector3 motion)
            {
                for (int i = 0; i < avatars.Length; i++)
                {
                    Vector3 target = origins[i] + motion;
                    if (showRemote)
                    {
                        var pl = remotePlayers[i];
                        pl.lastPos = Time.realtimeSinceStartupAsDouble;
                        pl.x = target.x;
                        pl.y = target.y;
                        avatars[i].GetComponent<RemoteAvatar>().Refresh(pl, false);
                    }
                    else avatars[i].position = target;
                }
            }

            Vector3 diagonalEnd = new Vector3(3.25f, .27f);
            yield return Move(Vector3.zero, diagonalEnd);
            for (int i = 0; i < avatars.Length; i++)
            {
                bool expected = db.trails[i].color.a > 0f;
                if (TrailVisible(avatars[i]) != expected)
                {
                    Fail(prefix + ": estela absent o incorrecta: " + db.trails[i].name);
                    yield break;
                }
            }
            yield return Shot(prefix + "-moviment");
            yield return new WaitForSeconds(.14f);
            yield return Shot(prefix + "-parada");
            yield return new WaitForSeconds(.8f);
            for (int i = 0; i < avatars.Length; i++)
                if (TrailVisible(avatars[i])) { Fail(prefix + ": estela congelada en parar: " + db.trails[i].name); yield break; }
            yield return Move(diagonalEnd, new Vector3(.1f, diagonalEnd.y));
            for (int i = 0; i < avatars.Length; i++)
            {
                bool expected = db.trails[i].color.a > 0f;
                if (TrailVisible(avatars[i]) != expected)
                {
                    Fail(prefix + ": estela absent o incorrecta en tornar a moure's en recta: " + db.trails[i].name);
                    yield break;
                }
            }
            yield return Shot(prefix + "-recta");
            Ok(prefix + ": " + db.trails.Length + " presets en diagonal, parada i arrancada en recta");
            Destroy(objects);
            yield return null;
        }
        Pass();
    }

    static bool TrailVisible(Transform avatar)
    {
        var ribbon = avatar.GetComponent<TrailRenderer>();
        if (ribbon != null) return ribbon.positionCount >= 2;
        var emitter = avatar.GetComponent<TrailParticleEmitter>();
        if (emitter == null) return false;
        var root = (GameObject)typeof(TrailParticleEmitter).GetField("root", PrivateInstance).GetValue(emitter);
        return root != null && root.GetComponentsInChildren<SpriteRenderer>().Length > 0;
    }

    static TextMeshPro GalleryLabel(Transform parent, string value, Vector3 position, float fontSize)
    {
        var item = new GameObject(value).AddComponent<TextMeshPro>();
        item.transform.SetParent(parent, false);
        item.transform.position = position;
        if (GameTypography.Font != null) item.font = GameTypography.Font;
        item.text = value;
        item.color = new Color(.92f, .96f, 1f);
        item.fontSize = fontSize;
        item.alignment = TextAlignmentOptions.MidlineLeft;
        item.rectTransform.pivot = new Vector2(0f, .5f);
        item.rectTransform.sizeDelta = new Vector2(3.9f, .6f);
        item.textWrappingMode = TextWrappingModes.NoWrap;
        item.GetComponent<MeshRenderer>().sortingOrder = 50;
        return item;
    }
}
