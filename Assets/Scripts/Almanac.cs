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
    static HashSet<string> weapons, seen, defeated;

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
        if (weapon == null) return; weapons ??= Load(WeaponsKey);
        if (weapons.Add(weapon.name)) Save(WeaponsKey, weapons);
    }

    public static void SawEnemy(string prefabName)
    {
        seen ??= Load(SeenKey); string id = Clean(prefabName);
        if (seen.Add(id)) Save(SeenKey, seen);
    }

    public static void DefeatedEnemy(string prefabName)
    {
        defeated ??= Load(DefeatedKey); string id = Clean(prefabName);
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

// Pantalla de l'Almanac: pestanyes ARMES / ENEMICS / BOSSES, quadrícula a l'esquerra i fitxa a la dreta.
public class AlmanacUI : MonoBehaviour
{
    enum Tab { Weapons, Enemies, Bosses }
    class Entry { public string id, name; public Sprite icon; public ChestRarity rarity; public bool unlocked, silhouette; public string stats, text; }

    static AlmanacUI instance;
    GameObject canvas;
    RectTransform grid;
    TextMeshProUGUI title, stats, text, counter;
    Image big;
    UIKit.UIButton[] tabs = new UIKit.UIButton[3];
    Tab tab = Tab.Weapons;
    List<Entry> entries = new List<Entry>();
    int selected;

    public static void Open()
    {
        if (instance != null) { instance.Close(); return; }
        var go = new GameObject("Almanac UI"); instance = go.AddComponent<AlmanacUI>(); instance.Build();
    }

    void Build()
    {
        canvas = UIKit.Canvas("Almanac", 60); canvas.transform.SetParent(transform, false);
        UIKit.Box(canvas.transform, Vector2.zero, new Vector2(4000, 3000), new Color(.035f, .065f, .10f, 1f), "Fons");
        UIKit.Text(canvas.transform, "ALMANAC", new Vector2(0, 400), new Vector2(700, 70), 48, TextAlignmentOptions.Center, UIKit.Gold);
        UIKit.Button(canvas.transform, "TANCAR", new Vector2(-640, 400), new Vector2(240, 62), Close, UIKit.Style.Danger, 28);
        counter = UIKit.Text(canvas.transform, "", new Vector2(540, 400), new Vector2(420, 40), 24, TextAlignmentOptions.MidlineRight, UIKit.Muted);
        string[] names = { "ARMES", "ENEMICS", "BOSSES" };
        for (int i = 0; i < 3; i++) { int t = i; tabs[i] = UIKit.Button(canvas.transform, names[i], new Vector2(-400 + i * 210, 320), new Vector2(200, 56), () => { tab = (Tab)t; selected = 0; Fill(); }, UIKit.Style.Normal, 24); }

        var frame = UIKit.Box(canvas.transform, new Vector2(-320, -30), new Vector2(760, 600), new Color(.03f, .06f, .09f), "Llista");
        var scroll = frame.gameObject.AddComponent<ScrollRect>();
        var viewport = UIKit.Box(frame, Vector2.zero, Vector2.zero, new Color(0, 0, 0, .01f), "Viewport"); UIKit.Stretch(viewport, 8, 8, 8, 8); viewport.gameObject.AddComponent<RectMask2D>();
        var content = new GameObject("Contingut", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter)); content.transform.SetParent(viewport, false);
        grid = (RectTransform)content.transform; grid.anchorMin = new Vector2(0, 1); grid.anchorMax = new Vector2(1, 1); grid.pivot = new Vector2(.5f, 1); grid.offsetMin = grid.offsetMax = Vector2.zero;
        var layout = content.GetComponent<GridLayoutGroup>(); layout.cellSize = new Vector2(112, 112); layout.spacing = new Vector2(8, 8); layout.padding = new RectOffset(4, 4, 4, 4);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 6;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = grid; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 35;

        var side = UIKit.Box(canvas.transform, new Vector2(400, -30), new Vector2(580, 600), UIKit.Panel, "Fitxa");
        big = UIKit.Picture(side, null, new Vector2(0, 190), new Vector2(190, 190));
        title = UIKit.Text(side, "", new Vector2(0, 70), new Vector2(540, 50), 34, TextAlignmentOptions.Center, UIKit.Gold);
        stats = UIKit.Text(side, "", new Vector2(0, 0), new Vector2(540, 60), 22, TextAlignmentOptions.Center, UIKit.TextColor);
        var scrollBox = UIKit.Box(side, new Vector2(0, -170), new Vector2(530, 190), new Color(.78f, .70f, .52f), "Pergami");
        text = UIKit.Text(scrollBox, "", Vector2.zero, new Vector2(500, 170), 22, TextAlignmentOptions.Center, new Color(.18f, .12f, .06f));
        Fill();
    }

    List<Entry> Collect()
    {
        var list = new List<Entry>();
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        if (catalog == null) return list;
        if (tab == Tab.Weapons)
        {
            foreach (var w in catalog.weapons.Where(w => w != null).OrderBy(w => w.rarity).ThenBy(w => w.weaponName))
            {
                bool got = Almanac.HasWeapon(w.name);
                list.Add(new Entry { id = w.name, name = w.weaponName, icon = w.icon, rarity = w.rarity, unlocked = got,
                    stats = "Dany " + w.damage + " · " + (w.cooldown > 0 ? (1f / w.cooldown).ToString("0.0") : "-") + " trets/s · abast " + w.range.ToString("0.#"),
                    text = (w.melee ? "Arma cos a cos. " : w.special != SpecialKind.None ? "Arma especial. " : "Arma de distància. ") + (w.pierce > 0 ? "Travessa enemics. " : "") + (w.explosionRadius > 0 ? "Explota en impactar. " : "") + (w.maxShots > 0 ? "Gasta combustible (" + w.maxShots + " trets). " : "") + "Rareza " + w.rarity + "." });
            }
        }
        else
        {
            foreach (var prefab in catalog.enemies.Where(p => p != null))
            {
                bool boss = prefab.GetComponent<EnemyBoss>() != null, micro = prefab.TryGetComponent<EnemyV140>(out var v) && v.IsMicroboss;
                if ((tab == Tab.Bosses) != (boss || micro)) continue;
                var health = prefab.GetComponent<EnemyHealth>(); var sr = prefab.GetComponent<SpriteRenderer>();
                bool saw = Almanac.HasSeen(prefab.name), won = Almanac.HasDefeated(prefab.name);
                bool open = tab == Tab.Bosses ? won : saw;
                Almanac.Notes.TryGetValue(prefab.name, out var note);
                list.Add(new Entry { id = prefab.name, name = boss ? prefab.GetComponent<EnemyBoss>().bossName : prefab.name, icon = sr != null ? sr.sprite : null, unlocked = open, silhouette = tab == Tab.Bosses && saw && !won,
                    stats = "Vida " + (health != null ? health.maxHealth : 0),
                    text = note ?? "Enemic de la masmorra. Vida " + (health != null ? health.maxHealth : 0) + "." });
            }
            list = list.OrderBy(e => e.name).ToList();
        }
        return list;
    }

    void Fill()
    {
        entries = Collect();
        UIKit.Clear(grid);
        for (int i = 0; i < 3; i++) tabs[i].SetStyle(i == (int)tab ? UIKit.Style.Primary : UIKit.Style.Normal);
        selected = Mathf.Clamp(selected, 0, Mathf.Max(0, entries.Count - 1));
        for (int i = 0; i < entries.Count; i++)
        {
            int index = i; var e = entries[i];
            var go = new GameObject("Carta", typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(grid, false);
            go.GetComponent<Image>().color = i == selected ? UIKit.Gold : e.unlocked ? ItemGraphics.RarityColor(tab == Tab.Weapons ? e.rarity : ChestRarity.Comu) : UIKit.Dim;
            go.GetComponent<Button>().onClick.AddListener(() => { selected = index; Fill(); });
            var inner = UIKit.Box(go.transform, Vector2.zero, new Vector2(104, 104), UIKit.Row, "Fons"); inner.GetComponent<Image>().raycastTarget = false;
            var pic = UIKit.Picture(inner, e.icon, new Vector2(0, 8), new Vector2(64, 64));
            pic.color = e.unlocked ? Color.white : Color.black;
            if (!e.unlocked && !e.silhouette) UIKit.Text(inner, "???", new Vector2(0, -38), new Vector2(96, 24), 16, TextAlignmentOptions.Center, UIKit.Dim);
            else UIKit.Text(inner, e.silhouette ? e.name : e.name, new Vector2(0, -38), new Vector2(98, 24), 11, TextAlignmentOptions.Center, UIKit.TextColor).textWrappingMode = TextWrappingModes.NoWrap;
        }
        int got = entries.Count(e => e.unlocked);
        counter.text = got + " / " + entries.Count;
        ShowDetail();
    }

    void ShowDetail()
    {
        if (entries.Count == 0) { title.text = ""; stats.text = ""; text.text = "Encara no hi ha res."; big.color = Color.clear; return; }
        var e = entries[selected];
        big.sprite = e.icon; big.color = e.unlocked ? Color.white : Color.black;
        if (e.unlocked) { title.text = e.name; stats.text = e.stats; text.text = e.text; }
        else if (e.silhouette) { title.text = e.name; stats.text = "???"; text.text = "L'has vist, però encara no l'has derrotat."; }
        else { title.text = "???"; stats.text = ""; text.text = tab == Tab.Weapons ? "Recull esta arma per a descobrir-la." : tab == Tab.Enemies ? "Troba'l a la masmorra." : "Derrota'l per a desbloquejar-lo."; }
    }

    void Update() { if (Input.GetKeyDown(KeyCode.Escape)) Close(); }
    void Close() { instance = null; Destroy(gameObject); }
    void OnDestroy() { if (instance == this) instance = null; }
}
