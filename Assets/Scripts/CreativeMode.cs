using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// MODE CREATIU del mapa lliure (escena GameScene).
// Botó davall del menú (☰) o tecla C: obri un panell per a agafar armes, crear enemics i posar cofres.
// No es guarda RES: tot són objectes de l'escena i, en eixir de la partida, Unity els destruïx.
// Online només el té l'amfitrió; els enemics i cofres arriben als altres pels missatges de NetCoop que ja hi havia.
public class CreativeMode : MonoBehaviour
{
    public const string Scene = "GameScene";
    public const KeyCode ToggleKey = KeyCode.C;
    public const KeyCode CancelKey = KeyCode.Q;

    public static CreativeMode Instance { get; private set; }
    // PlayerAttack i HealInventory no fan res mentre col·loques (si no, el clic també dispararia)
    public static bool Placing => Instance != null && (Instance.placing != null || Time.frameCount <= Instance.placedFrame + 1);
    // v1.4.1: amb el ratolí damunt del panell Creatiu, la roda sols fa scroll de la llista (no canvia d'arma ni de cura)
    public static bool PointerOverPanel => Instance != null && Instance.panel != null && Instance.panel.activeInHierarchy &&
        RectTransformUtility.RectangleContainsScreenPoint((RectTransform)Instance.panel.transform, Input.mousePosition, null);

    enum Tab { Armes, Menjar, Enemics, Cofres }
    class Entry { public string name, info; public Sprite icon; public Color tint = Color.white; public WeaponData weapon; public HealItem heal; public GameObject prefab; public bool chest; }

    Tab tab = Tab.Armes;
    GameObject canvas, panel, ghost;
    RectTransform toggleButton, list;
    UIKit.UIButton[] tabButtons;
    TextMeshProUGUI hint;
    Entry placing;
    int placedFrame = -10;
    SpriteRenderer ghostRenderer;
    NetContentCatalog catalog;

    // ---------- Arrancada: s'afig sol en carregar el mapa lliure ----------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Hook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreate();
    }

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene s, LoadSceneMode m) => TryCreate();

    static void TryCreate()
    {
        if (Instance != null || SceneManager.GetActiveScene().name != Scene) return;
        if (NetSession.Active && !NetSession.IsHost) return; // online: només l'amfitrió crea coses
        if (GameObject.FindWithTag("Player") == null) return;
        new GameObject("Mode creatiu").AddComponent<CreativeMode>();
    }

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; if (canvas != null) Destroy(canvas); if (ghost != null) Destroy(ghost); }

    void Start()
    {
        catalog = Resources.Load<NetContentCatalog>("NetContent");
        canvas = UIKit.Canvas("Mode creatiu UI", 89); // just davall del menú de pausa (90): la pausa el tapa
        var sc = canvas.GetComponent<CanvasScaler>();
        sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; sc.matchWidthOrHeight = 0.5f; // igual que el botó ☰

        // Botó quadrat davall del ☰ amb una creu daurada de píxels: "afegir coses"
        Vector2 size = MobileInput.Active ? new Vector2(85, 70) : new Vector2(54, 48);
        var b = UIKit.Box(canvas.transform, Vector2.zero, size, new Color(0.04f, 0.09f, 0.14f, 0.8f), "Boto creatiu");
        b.anchorMin = b.anchorMax = b.pivot = Vector2.one; toggleButton = b;
        b.gameObject.AddComponent<Button>().onClick.AddListener(Toggle);
        float k = size.y / 48f; // al mòbil el botó és més gran
        // Ombra fosca desplaçada + creu daurada (estil píxel)
        IconPiece(b, new Vector2(2, -2) * k, new Vector2(8, 26) * k, UIKit.Shadow);
        IconPiece(b, new Vector2(2, -2) * k, new Vector2(26, 8) * k, UIKit.Shadow);
        IconPiece(b, Vector2.zero, new Vector2(8, 26) * k, UIKit.Gold);
        IconPiece(b, Vector2.zero, new Vector2(26, 8) * k, UIKit.Gold);
        PlaceButton();
        BuildPanel();
        panel.SetActive(false);
    }

    static void IconPiece(Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        var d = UIKit.Box(parent, pos, size, color, "Icona");
        d.GetComponent<Image>().raycastTarget = false;
    }

    void BuildPanel()
    {
        var p = UIKit.Box(canvas.transform, Vector2.zero, new Vector2(450, 620), new Color(.03f, .065f, .095f, .96f), "Panell creatiu");
        p.anchorMin = p.anchorMax = p.pivot = Vector2.one; p.anchoredPosition = new Vector2(-(toggleButton.sizeDelta.x + 32), -20);
        p.gameObject.AddComponent<Outline>().effectColor = new Color(.25f, .42f, .48f);
        panel = p.gameObject;
        UIKit.Text(p, "MODE CREATIU", new Vector2(0, 280), new Vector2(420, 40), 28, TextAlignmentOptions.Center, UIKit.Gold);
        UIKit.Text(p, "Res es guarda: en eixir de la partida s'oblida tot.", new Vector2(0, 250), new Vector2(420, 26), 15, TextAlignmentOptions.Center, UIKit.Muted);
        UIKit.Button(p, "X", new Vector2(195, 282), new Vector2(36, 34), Toggle, UIKit.Style.Ghost, 18, false);
        string[] names = { "ARMES", "MENJAR", "ENEMICS", "COFRES" };
        tabButtons = new UIKit.UIButton[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            int t = i;
            tabButtons[i] = UIKit.Button(p, names[i], new Vector2(-159 + i * 106, 205), new Vector2(100, 40), () => { tab = (Tab)t; Fill(); }, UIKit.Style.Normal, 16);
        }
        list = UIKit.ScrollList(p, new Vector2(0, -35), new Vector2(420, 420), out _, 4);
        hint = UIKit.Text(p, "", new Vector2(0, -268), new Vector2(420, 40), 15, TextAlignmentOptions.Center, UIKit.Muted);
        // v1.4.1: quatre botons (afegit TREURE COFRES), amb la mateixa separació que les pestanyes
        UIKit.Button(p, "TREURE ENEMICS", new Vector2(-159, -287), new Vector2(100, 34), ClearEnemies, UIKit.Style.Danger, 11, false);
        UIKit.Button(p, "TREURE OBJECTES", new Vector2(-53, -287), new Vector2(100, 34), ClearItems, UIKit.Style.Danger, 11, false);
        UIKit.Button(p, "TREURE COFRES", new Vector2(53, -287), new Vector2(100, 34), ClearChests, UIKit.Style.Danger, 11, false);
        UIKit.Button(p, "TANCAR (C)", new Vector2(159, -287), new Vector2(100, 34), Toggle, UIKit.Style.Normal, 11, false);
        Fill();
    }

    // ---------- Llistes ----------
    List<Entry> Entries()
    {
        var e = new List<Entry>();
        if (catalog == null) return e;
        if (tab == Tab.Armes && catalog.weapons != null)
            foreach (var w in catalog.weapons)
                if (w != null) e.Add(new Entry { name = w.weaponName, info = RarityName(w.rarity) + " · " + w.damage + " de dany", icon = w.icon, tint = ItemGraphics.RarityColor(w.rarity), weapon = w });
        if (tab == Tab.Menjar)
            foreach (var hi in HealItem.All)
                if (hi != null) e.Add(new Entry { name = hi.itemName, info = "+" + hi.healAmount + (hi.healAmount == 1 ? " cor" : " cors"), icon = hi.icon, tint = ItemGraphics.RarityColor(hi.rarity), heal = hi });
        if (tab == Tab.Enemics && catalog.enemies != null)
            foreach (var go in catalog.enemies)
            {
                if (go != null && go.TryGetComponent<EnemyHealth>(out var part) && part.arenaPart) continue;
                if (go == null) continue;
                var h = go.GetComponent<EnemyHealth>();
                bool boss = go.GetComponent<EnemyBoss>() != null;
                e.Add(new Entry { name = go.name, info = (boss ? "BOSS · " : "") + (h != null ? h.maxHealth + " de vida" : ""), icon = SpriteOf(go), tint = boss ? UIKit.Red : UIKit.TextColor, prefab = go });
            }
        if (tab == Tab.Cofres && catalog.chests != null)
            foreach (var go in catalog.chests)
            {
                if (go == null) continue;
                var c = go.GetComponent<Chest>();
                e.Add(new Entry { name = go.name.Replace("Cofre_", "Cofre "), info = c != null ? RarityName(c.rarity) + " · arma aleatòria" : "", icon = c != null && c.closedSprite != null ? c.closedSprite : SpriteOf(go), tint = c != null ? ItemGraphics.RarityColor(c.rarity) : UIKit.TextColor, prefab = go, chest = true });
            }
        return e;
    }

    void Fill()
    {
        if (list == null) return;
        UIKit.Clear(list);
        for (int i = 0; i < tabButtons.Length; i++) tabButtons[i].SetStyle(i == (int)tab ? UIKit.Style.Primary : UIKit.Style.Normal);
        int n = 0;
        foreach (var entry in Entries())
        {
            var en = entry;
            var row = UIKit.ListRow(list, 54, n++ % 2 == 0 ? UIKit.Row : UIKit.RowAlt);
            var btn = row.gameObject.AddComponent<Button>();
            var colors = btn.colors; colors.highlightedColor = new Color(1.5f, 1.5f, 1.3f); colors.pressedColor = new Color(.7f, .8f, .8f); btn.colors = colors;
            btn.onClick.AddListener(() => Pick(en));
            var pic = UIKit.Picture(row, en.icon, Vector2.zero, new Vector2(42, 42)); UIKit.AnchorLeft(pic.rectTransform, 32);
            UIKit.Cell(row, en.name, 64, 200, 19, TextAlignmentOptions.MidlineLeft, en.tint);
            UIKit.Cell(row, en.info, 210, 190, 14, TextAlignmentOptions.MidlineRight, UIKit.Muted);
        }
        if (n == 0) UIKit.Text(list, "No hi ha res al catàleg", Vector2.zero, new Vector2(380, 40), 18, TextAlignmentOptions.Center, UIKit.Dim);
        UpdateHint();
    }

    void UpdateHint()
    {
        if (hint == null) return;
        if (placing != null) hint.text = "<color=#EBB347>" + placing.name + "</color>: clic al mapa per a posar-ne\nClic dret o Q per a parar";
        else hint.text = tab == Tab.Armes ? "Clica una arma i va a la teua hotbar" : tab == Tab.Menjar ? "Clica'l i va a la bossa de cures (tecles 5-8)" :MobileInput.Active ? "Toca'l i apareix davant teu" : "Clica'l i després clica on el vols al mapa";
    }

    // ---------- Accions ----------
    void Pick(Entry e)
    {
        if (e.weapon != null) { GiveWeapon(e.weapon); return; }
        if (e.heal != null) { GiveHeal(e.heal); return; }
        if (MobileInput.Active)
        {
            // En el mòbil no hi ha ratolí: apareix dos blocs davant de la canica
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;
            var aim = player.GetComponent<PlayerAttack>();
            Vector2 dir = aim != null && aim.AimDirection.sqrMagnitude > 0.01f ? aim.AimDirection : Vector2.right;
            Spawn(e, Snap((Vector2)player.transform.position + dir.normalized * 2f));
            return;
        }
        placing = e;
        ShowGhost(e);
        UpdateHint();
    }

    void GiveWeapon(WeaponData w)
    {
        var player = GameObject.FindWithTag("Player");
        var attack = player != null ? player.GetComponent<PlayerAttack>() : null;
        if (attack == null) return;
        if (attack.AddWeapon(w)) Fx.Text(player.transform.position, w.weaponName, ItemGraphics.RarityColor(w.rarity));
        else
        {
            // Hotbar plena: la deixem al terra, als teus peus (Z per a deixar-ne una altra)
            Vector3 from = player.transform.position;
            ItemPickup.Spawn(w, from, TileNav.DropDestination(from, from + Vector3.down * 0.9f));
            Fx.Text(from, "Hotbar plena: al terra", UIKit.Muted);
        }
    }

    void GiveHeal(HealItem item)
    {
        var player = GameObject.FindWithTag("Player");
        var bag = player != null ? player.GetComponent<HealInventory>() : null;
        if (bag == null) return;
        if (bag.Add(item)) Fx.Text(player.transform.position, item.itemName, ItemGraphics.RarityColor(item.rarity));
        else
        {
            // Bossa plena (4 caselles de 5): cau al terra com quan la deixes tu
            Vector3 from = player.transform.position;
            HealPickup.Spawn(item, from, TileNav.DropDestination(from, from + Vector3.down * 0.9f));
            Fx.Text(from, "Bossa plena: al terra", UIKit.Muted);
        }
    }

    void Spawn(Entry e, Vector2 pos)
    {
        if (TileNav.Ready() && TileNav.IsBlockedWorld(pos)) { Fx.Text(pos, "Ací no", UIKit.Red); return; }
        if (e.chest)
        {
            var c = e.prefab.GetComponent<Chest>();
            int rarity = c != null ? (int)c.rarity : 0;
            var cell = new Vector2Int(Mathf.RoundToInt(pos.x * 4), Mathf.RoundToInt(pos.y * 4));
            if (CreateChest(rarity, cell) != null) NetCoop.ChestSpawned(rarity, cell); // online: també als altres
        }
        else
        {
            Instantiate(e.prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            Fx.Burst(pos, new Color(1f, 0.4f, 0.45f), 8, 2f);
        }
    }

    // Comú per a l'amfitrió i per als clients (NetCoop ho crida en rebre ChestSpawn al mapa lliure)
    public static GameObject CreateChest(int rarity, Vector2Int cell)
    {
        var cat = Resources.Load<NetContentCatalog>("NetContent");
        if (cat == null || cat.chests == null) return null;
        Vector3 pos = new Vector3(cell.x / 4f, cell.y / 4f, 0);
        foreach (var go in cat.chests)
        {
            var c = go != null ? go.GetComponent<Chest>() : null;
            if (c == null || (int)c.rarity != rarity) continue;
            var chest = Instantiate(go, pos, Quaternion.identity);
            Fx.Burst(pos, ItemGraphics.RarityColor(c.rarity), 8, 2f);
            return chest;
        }
        return null;
    }

    // Lleva tot el que hi ha solt pel terra: armes, menjar i monedes caigudes.
    // Les monedes animades del mapa (MonedaMapa) no, que són part del mapa.
    void ClearItems()
    {
        int n = 0;
        foreach (var it in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None)) { Fx.Burst(it.transform.position, new Color(.7f, .9f, 1f), 4, 1.5f); NetShared.HostRemove(it); n++; }
        foreach (var it in FindObjectsByType<HealPickup>(FindObjectsSortMode.None)) { Fx.Burst(it.transform.position, new Color(.7f, .9f, 1f), 4, 1.5f); NetShared.HostRemove(it); n++; }
        foreach (var it in FindObjectsByType<CoinDrop>(FindObjectsSortMode.None)) { NetShared.HostRemove(it); n++; }
        var player = GameObject.FindWithTag("Player");
        if (player != null) Fx.Text(player.transform.position, n > 0 ? n + " objectes fora" : "No n'hi ha", UIKit.Muted);
    }

    // v1.4.1: lleva tots els cofres del mapa (els de l'escena i els posats amb el Creatiu)
    void ClearChests()
    {
        int n = DestroyAllChests();
        NetCoop.ChestsCleared(); // online: també als altres i als que entren tard
        var player = GameObject.FindWithTag("Player");
        if (player != null) Fx.Text(player.transform.position, n > 0 ? n + " cofres fora" : "No n'hi ha", UIKit.Muted);
    }

    public static int DestroyAllChests()
    {
        int n = 0;
        foreach (var c in FindObjectsByType<Chest>(FindObjectsSortMode.None))
        {
            Fx.Burst(c.transform.position, new Color(.7f, .9f, 1f), 4, 1.5f);
            Destroy(c.gameObject); n++;
        }
        return n;
    }

    void ClearEnemies()
    {
        int n = 0;
        foreach (var h in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
        {
            if (h == null || h.IsDead) continue;
            Fx.Burst(h.transform.position, new Color(.7f, .9f, 1f), 6, 2f);
            NetCoop.EnemyDied(h); // online: el fa desaparéixer també als clients
            Destroy(h.gameObject); n++;
        }
        var player = GameObject.FindWithTag("Player");
        if (player != null) Fx.Text(player.transform.position, n > 0 ? n + " enemics fora" : "No n'hi ha", UIKit.Muted);
    }

    void Toggle()
    {
        if (panel == null) return;
        bool show = !panel.activeSelf;
        panel.SetActive(show);
        if (!show) StopPlacing();
        else Fill();
    }

    void StopPlacing()
    {
        placing = null;
        if (ghost != null) ghost.SetActive(false);
        UpdateHint();
    }

    // ---------- Fantasma per a col·locar ----------
    void ShowGhost(Entry e)
    {
        if (ghost == null)
        {
            ghost = new GameObject("Fantasma creatiu");
            ghostRenderer = ghost.AddComponent<SpriteRenderer>();
            ghostRenderer.sortingLayerName = "Player"; ghostRenderer.sortingOrder = 500; // per damunt del terra i dels arbres
        }
        Sprite s = e.icon;
        ghostRenderer.sprite = s;
        // Mateixa mida que l'objecte de veritat
        var src = e.prefab != null ? e.prefab.GetComponentInChildren<SpriteRenderer>() : null;
        ghost.transform.localScale = src != null ? Vector3.Scale(e.prefab.transform.localScale, src.transform.localScale) : Vector3.one;
        ghost.SetActive(true);
    }

    static Vector2 Snap(Vector2 world)
    {
        if (TileNav.Ready()) return TileNav.Center(TileNav.Cell(world));
        return new Vector2(Mathf.Floor(world.x) + .5f, Mathf.Floor(world.y) + .5f);
    }

    static bool PointerOnUI()
    {
        var es = EventSystem.current;
        return es != null && es.IsPointerOverGameObject();
    }

    void PlaceButton()
    {
        if (toggleButton == null) return;
        // Just davall del ☰ (que està a -20,-20 i fa 48 o 70 d'alt). Al mòbil respecta la zona segura.
        float scale = canvas.GetComponent<Canvas>().scaleFactor;
        Vector2 safe = MobileInput.Active && scale > 0 ? new Vector2((Screen.width - Screen.safeArea.xMax) / scale, (Screen.height - Screen.safeArea.yMax) / scale) : Vector2.zero;
        toggleButton.anchoredPosition = new Vector2(-20 - safe.x, -20 - toggleButton.sizeDelta.y - 10 - safe.y);
        if (panel != null) ((RectTransform)panel.transform).anchoredPosition = new Vector2(-(toggleButton.sizeDelta.x + 32) - safe.x, -20 - safe.y);
    }

    void Update()
    {
        if (MobileInput.Active) PlaceButton();
        if (Time.timeScale == 0f) return; // pausa: el menú ☰ mana
        if (!GameControls.Blocked && Input.GetKeyDown(ToggleKey)) Toggle();
        if (placing == null) return;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(CancelKey)) { StopPlacing(); placedFrame = Time.frameCount; return; }
        var cam = Camera.main;
        if (cam == null) return;
        Vector2 world = Snap(cam.ScreenToWorldPoint(Input.mousePosition));
        bool onUI = PointerOnUI();
        if (ghost != null)
        {
            ghost.SetActive(!onUI);
            ghost.transform.position = new Vector3(world.x, world.y, 0);
            bool blocked = TileNav.Ready() && TileNav.IsBlockedWorld(world);
            ghostRenderer.color = blocked ? new Color(1f, .35f, .35f, .55f) : new Color(1f, 1f, 1f, .6f);
        }
        if (Input.GetMouseButtonDown(0) && !onUI)
        {
            Spawn(placing, world); // es queda en mode col·locar: pots posar-ne uns quants seguits
            placedFrame = Time.frameCount;
        }
    }

    static Sprite SpriteOf(GameObject go)
    {
        var sr = go.GetComponentInChildren<SpriteRenderer>(true);
        return sr != null ? sr.sprite : null;
    }

    static string RarityName(ChestRarity r) =>
        r == ChestRarity.Rar ? "Rara" : r == ChestRarity.Epic ? "Èpica" : r == ChestRarity.Llegendari ? "Llegendària" : r == ChestRarity.PocComu ? "Poc comuna" : "Comuna";
}
