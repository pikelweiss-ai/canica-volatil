using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// v1.4.1: Almanac fase 1 (llistes d'armes i d'enemics). Guarda al dispositiu què has trobat (PlayerPrefs, per nom).
// Armes: quan les reculls. Enemics: quan els veus. Bosses i microbosses: se't mostren com a silueta fins que els derrotes.
public static class Almanac
{
    const string WeaponsKey = "Almanac_Weapons", SeenKey = "Almanac_Seen", DefeatedKey = "Almanac_Defeated";
    static HashSet<string> weapons, seen, defeated, blocks;
    const string BlocksKey = "Almanac_Blocks";

    public static bool HasBlock(string id) { blocks ??= Load(BlocksKey); return blocks.Contains(id); }

    public static bool SawBlock(string id)
    {
        if (!RunState.Active || string.IsNullOrEmpty(id)) return false;
        blocks ??= Load(BlocksKey);
        return blocks.Add(id);
    }

    public static void SaveBlocks()
    {
        if (!RunStats.DontSave && blocks != null) Save(BlocksKey, blocks);
    }

    static HashSet<string> Load(string key)
    {
        string raw = ""; try { raw = PlayerPrefs.GetString(key, ""); } catch { }
        return new HashSet<string>(raw.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
    }
    static void Save(string key, HashSet<string> set) { try { PlayerPrefs.SetString(key, string.Join(",", set)); PlayerPrefs.Save(); } catch { } }

    static string Clean(string name) => name.Replace("(Clone)", "").Trim();

    public static bool HasWeapon(string id) { weapons ??= Load(WeaponsKey); return weapons.Contains(id); }
    public static bool HasSeen(string id) { seen ??= Load(SeenKey); return seen.Contains(Clean(id)); }
    public static bool HasDefeated(string id) { defeated ??= Load(DefeatedKey); return defeated.Contains(Clean(id)); }

    public static void PickedWeapon(WeaponData weapon)
    {
        if (weapon == null || !RunState.Active) return; weapons ??= Load(WeaponsKey); // només compta en partides de masmorra: ni Creatiu ni mapa lliure
        if (weapons.Add(weapon.name)) Save(WeaponsKey, weapons);
    }

    public static void SawEnemy(string prefabName)
    {
        if (!RunState.Active) return; seen ??= Load(SeenKey); string id = Clean(prefabName);
        if (seen.Add(id)) Save(SeenKey, seen);
    }

    public static void DefeatedEnemy(string prefabName)
    {
        if (!RunState.Active) return; defeated ??= Load(DefeatedKey); string id = Clean(prefabName);
        if (defeated.Add(id)) Save(DefeatedKey, defeated);
    }

    // Frase de personalitat de cada enemic (els que no hi són, porten el text automàtic)
    public static readonly Dictionary<string, string> Notes = new Dictionary<string, string>
    {
        ["Mimic"] = "El cofre que mossega. Si el veus massa tranquil, dispara-li abans d'acostar-te.",
        ["Sanador"] = "Cura els seus amics amb un feix verd. Mata'l primer, que fuig.",
        ["EscutGuerrer"] = "El seu escut para les bales. Cal obrir-lo a colps o voltar-lo.",
        ["Talp"] = "Ataca des de sota terra. Si no et mous, et trobarà.",
        ["FranctiradorCristall"] = "Apunta amb un làser i dispara un sol tret fort. Pega'l i l'interromps.",
        ["Banshee"] = "Fuig de tu i crida: t'empeny i et fa mal. No et quedes quiet.",
        ["Salamandra"] = "Colp i fugida: s'atura, encén un cercle de foc i fuig. No el persegueixes: eviteu el foc.",
        ["LlimacAcid"] = "Lent i enganxós. Deixa tolls d'àcid que et frenen i, en morir, es parteix en tres.",
        ["Escorpi"] = "L'escut del davant para colps i bales. Dóna-li la volta; la cua avisa abans de colpejar.",
        ["LlopNeu"] = "Van en manada de tres. El líder udola i carreguen tots alhora: mata'l primer.",
        ["Bombardier"] = "Llança bombes amb un cercle roig. Les bombes també fan mal als altres enemics.",
        ["TorretaAntiga"] = "Fixa, dispara només en línies de fila i columna i canvia d'eix cada 3 segons.",
        ["EscarabatDaurat"] = "Es fa bola i rebota, invulnerable. Quan s'atordeix, colpeja'l: deixa monedes.",
        ["UllFlotant"] = "Tancat és immune. Quan obri l'ull dispara un feix giratori... i és vulnerable.",
        ["GranotaGegant"] = "La llengua t'estira cap a ella. Si la colpeges mentre la prepara, la talles.",
        ["MagCendra"] = "Tres orbes de foc paren els colps. Sense orbes és vulnerable 4 segons.",
        ["ReinaAranya"] = "Reina dels boscos i criptes. Invoca aranyes i carrega contra tu.",
        ["ReiGel"] = "Rei de les neus. Dispara anells i espirals de gel.",
    };
}

// Llibre de descobriments. La vista prèvia és una animació de la fitxa i no executa combat.
public class AlmanacUI : MonoBehaviour
{
    enum Tab { Weapons, Enemies, Bosses, Amulets, Blocks }
    class Entry
    {
        public string id, name, stats, text, family;
        public string[] themes = System.Array.Empty<string>();
        public Sprite icon, projectile;
        public ChestRarity rarity;
        public bool unlocked, silhouette;
        public AlmanacPreview.Pattern pattern;
    }

    const int PageSize = 30;
    static AlmanacUI instance;
    GameObject canvas;
    RectTransform grid, filters, filterMenu;
    ScrollRect scroll;
    TextMeshProUGUI title, stats, text, counter, pageLabel;
    Image big;
    AlmanacPreview preview;
    readonly UIKit.UIButton[] tabs = new UIKit.UIButton[5];
    UIKit.UIButton left, right, familyButton, themeButton, rarityButton;
    TMP_InputField search;
    Tab tab;
    List<Entry> all = new(), entries = new();
    int page, selected;
    Image selectedBorder;
    string family, theme;
    ChestRarity? rarity;

    public static void Open()
    {
        if (instance != null) { instance.Close(); return; }
        var go = new GameObject("Almanac UI");
        instance = go.AddComponent<AlmanacUI>();
        instance.Build();
    }

    void Build()
    {
        canvas = UIKit.Canvas("Almanac", 60);
        canvas.transform.SetParent(transform, false);
        UIKit.Box(canvas.transform, Vector2.zero, new Vector2(4000, 3000), new Color(.035f, .065f, .10f, 1f), "Fons");
        UIKit.Text(canvas.transform, "ALMANAC", new Vector2(0, 400), new Vector2(700, 70), 48, TextAlignmentOptions.Center, UIKit.Gold);
        UIKit.Button(canvas.transform, "TANCAR", new Vector2(-640, 400), new Vector2(240, 62), Close, UIKit.Style.Danger, 28);
        counter = UIKit.Text(canvas.transform, "", new Vector2(540, 400), new Vector2(420, 40), 24, TextAlignmentOptions.MidlineRight, UIKit.Muted);
        string[] names = { "ARMES", "ENEMICS", "BOSSES", "AMULETS", "BLOCS" };
        for (int i = 0; i < tabs.Length; i++)
        {
            var current = (Tab)i;
            tabs[i] = UIKit.Button(canvas.transform, names[i], new Vector2(-568 + i * 173, 326),
                new Vector2(165, 47), () => SwitchTab(current), UIKit.Style.Normal, 22);
        }

        search = UIKit.Input(canvas.transform, new Vector2(-532, 266), new Vector2(365, 43), "Busca pel nom", 50, fontSize: 20);
        search.onValueChanged.AddListener(_ => ApplyFilters());
        filters = UIKit.Empty(canvas.transform, "Filtres");
        familyButton = UIKit.Button(filters, "", new Vector2(-259, 266), new Vector2(165, 43),
            () => ShowFilter("Família", all.Select(e => e.family), choice => { family = choice; ApplyFilters(); }), UIKit.Style.Normal, 17);
        themeButton = UIKit.Button(filters, "", new Vector2(-79, 266), new Vector2(165, 43),
            () => ShowFilter("Tema", all.SelectMany(e => e.themes), choice => { theme = choice; ApplyFilters(); }), UIKit.Style.Normal, 17);
        rarityButton = UIKit.Button(filters, "", new Vector2(87, 266), new Vector2(166, 43), ShowRarityFilter, UIKit.Style.Normal, 17);

        var frame = UIKit.Box(canvas.transform, new Vector2(-320, -42), new Vector2(760, 543), UIKit.Ink, "Llista");
        scroll = frame.gameObject.AddComponent<ScrollRect>();
        var viewport = UIKit.Box(frame, Vector2.zero, Vector2.zero, new Color(0, 0, 0, .01f), "Viewport");
        UIKit.Stretch(viewport, 8, 8, 8, 8);
        viewport.gameObject.AddComponent<RectMask2D>();
        var content = new GameObject("Contingut", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport, false);
        grid = (RectTransform)content.transform;
        grid.anchorMin = new Vector2(0, 1);
        grid.anchorMax = new Vector2(1, 1);
        grid.pivot = new Vector2(.5f, 1);
        grid.offsetMin = grid.offsetMax = Vector2.zero;
        var layout = content.GetComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(112, 97);
        layout.spacing = new Vector2(8, 8);
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 6;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport;
        scroll.content = grid;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 35;

        left = UIKit.Button(canvas.transform, "‹", new Vector2(-452, -363), new Vector2(70, 48), () => ChangePage(-1), UIKit.Style.Normal, 35);
        right = UIKit.Button(canvas.transform, "›", new Vector2(-188, -363), new Vector2(70, 48), () => ChangePage(1), UIKit.Style.Normal, 35);
        pageLabel = UIKit.Text(canvas.transform, "", new Vector2(-320, -363), new Vector2(172, 38), 22,
            TextAlignmentOptions.Center, UIKit.Muted);

        var side = UIKit.Box(canvas.transform, new Vector2(448, -58), new Vector2(542, 630), UIKit.Panel, "Fitxa");
        var stage = UIKit.Box(side, new Vector2(0, 155), new Vector2(488, 245), UIKit.Ink, "Previsualització");
        stage.gameObject.AddComponent<RectMask2D>();
        big = UIKit.Picture(stage, null, Vector2.zero, new Vector2(175, 175));
        preview = stage.gameObject.AddComponent<AlmanacPreview>();
        preview.Build(big);
        title = UIKit.Text(side, "", new Vector2(0, 9), new Vector2(504, 48), 32, TextAlignmentOptions.Center, UIKit.Gold);
        title.enableAutoSizing = true;
        title.fontSizeMin = 19;
        title.fontSizeMax = 32;
        stats = UIKit.Text(side, "", new Vector2(0, -51), new Vector2(502, 66), 21, TextAlignmentOptions.Center, UIKit.TextColor);
        var paper = UIKit.Box(side, new Vector2(0, -189), new Vector2(496, 190), new Color(.78f, .70f, .52f), "Pergamí");
        text = UIKit.Text(paper, "", Vector2.zero, new Vector2(457, 166), 22, TextAlignmentOptions.Center, new Color(.18f, .12f, .06f));
        SwitchTab(Tab.Weapons);
    }

    void SwitchTab(Tab current)
    {
        if (filterMenu != null) Destroy(filterMenu.gameObject);
        tab = current;
        family = theme = null;
        rarity = null;
        search.SetTextWithoutNotify("");
        all = Collect();
        for (int i = 0; i < tabs.Length; i++)
            tabs[i].SetStyle(i == (int)tab ? UIKit.Style.Primary : UIKit.Style.Normal);
        familyButton.rect.gameObject.SetActive(tab == Tab.Blocks);
        rarityButton.rect.gameObject.SetActive(tab != Tab.Blocks);
        themeButton.rect.gameObject.SetActive(tab == Tab.Blocks || tab == Tab.Enemies || tab == Tab.Bosses);
        ApplyFilters();
    }

    List<Entry> Collect()
    {
        if (tab == Tab.Blocks)
            return (BlockCatalog.Instance?.blocks ?? System.Array.Empty<BlockData>()).Where(block => block != null)
                .OrderBy(block => block.family).ThenBy(block => block.displayName)
                .Select(block => new Entry
                {
                    id = block.id, name = block.displayName, icon = block.sprite, family = block.family,
                    themes = new[] { block.theme }, unlocked = Almanac.HasBlock(block.id),
                    stats = block.family + " · " + block.theme,
                    text = Properties(block)
                }).ToList();

        if (tab == Tab.Amulets)
            return RunAmulets.Catalog.Select(info => new Entry
            {
                id = info.id.ToString(), name = info.name, icon = info.Icon, rarity = info.rarity,
                unlocked = RunAmulets.Discovered(info.id), stats = CosmeticStore.QualityName(info.rarity), text = info.description,
                pattern = AlmanacPreview.Pattern.Amulet
            }).ToList();

        var list = new List<Entry>();
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        if (catalog == null) return list;
        if (tab == Tab.Weapons)
        {
            foreach (var w in catalog.weapons.Where(w => w != null).OrderBy(w => w.rarity == ChestRarity.PocComu ? .5f : (float)w.rarity).ThenBy(w => w.weaponName))
                list.Add(new Entry
                {
                    id = w.name, name = w.weaponName, icon = w.icon, projectile = w.projectileSprite, rarity = w.rarity,
                    unlocked = Almanac.HasWeapon(w.name), pattern = w.melee ? AlmanacPreview.Pattern.Sweep :
                        w.pellets > 1 ? AlmanacPreview.Pattern.Fan : w.explosionRadius > 0 ? AlmanacPreview.Pattern.Hazard : AlmanacPreview.Pattern.Bolt,
                    stats = "Dany " + w.damage + " · " + (w.cooldown > 0 ? (1f / w.cooldown).ToString("0.0") : "—") +
                        " trets/s · abast " + w.range.ToString("0.#"),
                    text = (w.melee ? "Arma cos a cos." : w.special != SpecialKind.None ? "Arma especial." : "Arma de distància.") +
                        (w.pierce > 0 ? " Travessa enemics." : "") + (w.explosionRadius > 0 ? " Explota en impactar." : "") +
                        (w.maxShots > 0 ? " Gasta combustible (" + w.maxShots + " trets)." : "") +
                        "\n" + CosmeticStore.QualityName(w.rarity)
                });
            return list;
        }

        foreach (var prefab in catalog.enemies.Where(p => p != null))
        {
            var boss = prefab.GetComponent<EnemyBoss>();
            var enemy = prefab.GetComponent<EnemyV140>();
            var hp = prefab.GetComponent<EnemyHealth>();
            bool micro = enemy != null && enemy.IsMicroboss;
            if ((tab == Tab.Bosses) != (boss != null || micro) || hp == null || hp.arenaPart) continue;
            bool saw = Almanac.HasSeen(prefab.name), won = Almanac.HasDefeated(prefab.name);
            Almanac.Notes.TryGetValue(prefab.name, out var note);
            var themes = ThemesFor(prefab);
            float speed = prefab.GetComponent<EnemyPathfindingAI>()?.moveSpeed ?? 0f;
            list.Add(new Entry
            {
                id = prefab.name, name = boss != null ? boss.bossName : prefab.name,
                icon = prefab.GetComponent<SpriteRenderer>()?.sprite, themes = themes,
                rarity = boss != null ? ChestRarity.Epic : micro ? ChestRarity.Rar : ChestRarity.Comu,
                unlocked = tab == Tab.Bosses ? won : saw, silhouette = tab == Tab.Bosses && saw && !won,
                stats = "Vida " + (hp.maxHealth + (boss != null ? EnemyBoss.BonusHealth : 0)) + " · Velocitat " +
                    speed.ToString("0.#") + "\n" + string.Join(", ", themes),
                text = note ?? (boss != null ? "Boss de la masmorra. Canvia d'atacs a mitja vida." : "Enemic de la masmorra."),
                pattern = AlmanacPreview.ForEnemy(boss, enemy, prefab.GetComponent<EnemyShooter>() != null)
            });
        }
        return list.OrderBy(e => e.name).ToList();
    }

    static string[] ThemesFor(GameObject prefab)
    {
        return (BlockCatalog.Instance?.themes ?? System.Array.Empty<DungeonTheme>())
            .Where(t => t != null && (t.boss == prefab || t.mimic == prefab ||
                (t.bossVariants != null && t.bossVariants.Contains(prefab)) ||
                (t.enemies != null && t.enemies.Contains(prefab)) ||
                (t.microbosses != null && t.microbosses.Contains(prefab))))
            .Select(t => t.themeName).Distinct().ToArray();
    }

    static string Properties(BlockData block)
    {
        var details = new List<string>
        {
            block.passable ? "Es pot travessar" : "Sòlid",
            block.blocksBullets ? "Para les bales" : "Deixa passar les bales",
            block.breakable ? "Es pot trencar" : "Indestructible"
        };
        if (block.damage > 0) details.Add(block.damage.ToString("0.#") + " de mal per segon");
        if (block.hideInside) details.Add("T'hi pots amagar");
        if (block.emitsLight) details.Add("Fa llum");
        if (block.multiPart) details.Add("Peça d'un objecte de diversos blocs");
        if (!block.passable) details.Add("Hitbox " + block.hitbox.width.ToString("0.##") + " × " + block.hitbox.height.ToString("0.##"));
        return string.Join("\n", details);
    }

    void ShowFilter(string label, IEnumerable<string> values, System.Action<string> choose)
    {
        if (filterMenu != null) Destroy(filterMenu.gameObject);
        filterMenu = UIKit.Box(canvas.transform, new Vector2(-246, 47), new Vector2(925, 339), UIKit.PanelLight, label);
        UIKit.Text(filterMenu, label, new Vector2(0, 142), new Vector2(840, 44), 28, TextAlignmentOptions.Center, UIKit.Gold);
        var choices = new[] { "" }.Concat(values.Where(v => !string.IsNullOrEmpty(v)).Distinct().OrderBy(v => v)).ToArray();
        for (int i = 0; i < choices.Length; i++)
        {
            string choice = choices[i];
            UIKit.Button(filterMenu, string.IsNullOrEmpty(choice) ? "Tots" : choice,
                new Vector2(-355 + i % 5 * 178, 76 - i / 5 * 58), new Vector2(166, 46),
                () => { Destroy(filterMenu.gameObject); filterMenu = null; choose(choice); }, UIKit.Style.Normal, 19);
        }
    }

    void ShowRarityFilter()
    {
        var values = System.Enum.GetValues(typeof(ChestRarity)).Cast<ChestRarity>()
            .Where(value => value != ChestRarity.PocComu || tab == Tab.Weapons).ToArray();
        string Name(ChestRarity value) => value == ChestRarity.PocComu ? "Poc comú" : CosmeticStore.QualityName(value);
        ShowFilter("Qualitat", values.Select(Name), choice =>
        {
            rarity = string.IsNullOrEmpty(choice) ? null : (ChestRarity?)values.First(value => Name(value) == choice);
            ApplyFilters();
        });
    }

    void ApplyFilters()
    {
        var compare = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
        const System.Globalization.CompareOptions ignore = System.Globalization.CompareOptions.IgnoreCase |
            System.Globalization.CompareOptions.IgnoreNonSpace;
        string query = search.text.Trim();
        entries = all.Where(e =>
            (string.IsNullOrEmpty(family) || e.family == family) &&
            (string.IsNullOrEmpty(theme) || e.themes.Contains(theme)) &&
            (!rarity.HasValue || e.rarity == rarity.Value) &&
            (query.Length == 0 || compare.IndexOf(e.name, query, ignore) >= 0)).ToList();
        page = selected = 0;
        familyButton.SetLabel(string.IsNullOrEmpty(family) ? "Família" : family);
        themeButton.SetLabel(string.IsNullOrEmpty(theme) ? "Tema" : theme);
        rarityButton.SetLabel(rarity.HasValue ? (rarity == ChestRarity.PocComu ? "Poc comú" : CosmeticStore.QualityName(rarity.Value)) : "Qualitat");
        RenderPage();
    }

    void ChangePage(int direction)
    {
        page += direction;
        selected = page * PageSize;
        RenderPage();
    }

    void RenderPage()
    {
        UIKit.Clear(grid);
        selectedBorder = null;
        scroll.verticalNormalizedPosition = 1f;
        int end = Mathf.Min((page + 1) * PageSize, entries.Count);
        for (int i = page * PageSize; i < end; i++)
        {
            int index = i;
            var e = entries[i];
            var go = new GameObject("Carta " + e.id, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(grid, false);
            var border = go.GetComponent<Image>();
            border.color = BorderFor(index, e);
            if (selected == index) selectedBorder = border;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selected == index) return;
                int previous = selected;
                selected = index;
                if (selectedBorder != null) selectedBorder.color = BorderFor(previous, entries[previous]);
                selectedBorder = border;
                border.color = UIKit.Gold;
                ShowDetail();
            });
            var inner = UIKit.Box(go.transform, Vector2.zero, new Vector2(104, 89), UIKit.Row, "Fons");
            inner.GetComponent<Image>().raycastTarget = false;
            var pic = UIKit.Picture(inner, e.icon, new Vector2(0, 8), new Vector2(62, 62));
            pic.color = e.unlocked ? Color.white : e.icon != null ? Color.black : Color.clear;
            var label = UIKit.Text(inner, e.unlocked || e.silhouette ? e.name : "???", new Vector2(0, -33),
                new Vector2(99, 22), 14, TextAlignmentOptions.Center, e.unlocked || e.silhouette ? UIKit.TextColor : UIKit.Muted);
            label.enableAutoSizing = true;
            label.fontSizeMin = 8;
            label.fontSizeMax = 14;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }
        int pages = Mathf.Max(1, Mathf.CeilToInt(entries.Count / (float)PageSize));
        pageLabel.text = (page + 1) + " / " + pages;
        left.SetInteractable(page > 0);
        right.SetInteractable(page + 1 < pages);
        counter.text = entries.Count(e => e.unlocked) + " / " + entries.Count +
            (entries.Count != all.Count ? " · Total " + all.Count(e => e.unlocked) + " / " + all.Count : "");
        ShowDetail();
    }

    Color BorderFor(int index, Entry e)
    {
        if (index == selected) return UIKit.Gold;
        if (tab != Tab.Blocks) return e.unlocked ? ItemGraphics.RarityColor(e.rarity) : UIKit.Dim;
        int hash = 17;
        foreach (char c in e.family) hash = unchecked(hash * 31 + c);
        return Color.HSVToRGB((hash & 0xffff) / 65536f, .48f, .74f);
    }

    void ShowDetail()
    {
        if (entries.Count == 0)
        {
            title.text = stats.text = "";
            text.text = "Cap resultat.";
            preview.Show(null, null, AlmanacPreview.Pattern.None, false);
            return;
        }
        var e = entries[selected];
        preview.Show(e.icon, e.projectile, e.pattern, e.unlocked);
        if (e.unlocked) { title.text = e.name; stats.text = e.stats; text.text = e.text; }
        else if (e.silhouette) { title.text = e.name; stats.text = "???"; text.text = "L'has vist, però encara no l'has derrotat."; }
        else
        {
            title.text = "???";
            stats.text = "";
            text.text = tab switch
            {
                Tab.Weapons => "Recull esta arma per a descobrir-la.",
                Tab.Enemies => "Troba'l a la masmorra.",
                Tab.Blocks => "Descobrix este bloc a la masmorra.",
                Tab.Amulets => "Aconseguix l'amulet durant una partida de masmorra.",
                _ => "Derrota'l per a desbloquejar-lo."
            };
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (filterMenu == null) Close();
        else { Destroy(filterMenu.gameObject); filterMenu = null; }
    }

    void Close() { instance = null; Destroy(gameObject); }
    void OnDestroy() { if (instance == this) instance = null; }
}
