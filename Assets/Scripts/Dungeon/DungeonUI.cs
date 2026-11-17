using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Interfície de la masmorra, feta per codi:
// - Minimapa (dalt a la dreta): sales visitades i les veïnes; groc = on estàs,
//   roig = boss, or = tresor. Les sales sense netejar tenen un punt roig.
// - Rètol en començar el pis ("Pis 1 · Cova") i en entrar a la sala del boss.
// - Pantalla de "Has mort".
public class DungeonUI : MonoBehaviour
{
    const float Cell = 22f, RoomSize = 16f;

    private DungeonGenerator gen;
    private TMP_FontAsset font;
    private RectTransform mapRoot;
    private TextMeshProUGUI banner;
    private CanvasGroup bannerGroup;
    private readonly List<GameObject> mapItems = new List<GameObject>();

    public static DungeonUI Create(DungeonGenerator gen)
    {
        GameObject go = new GameObject("UI Masmorra", typeof(Canvas), typeof(CanvasScaler));
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 30;
        CanvasScaler sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1600, 900);
        sc.matchWidthOrHeight = 0.5f;

        DungeonUI ui = go.AddComponent<DungeonUI>();
        ui.gen = gen;
        TextMeshProUGUI any = FindAnyObjectByType<TextMeshProUGUI>();
        if (any != null) ui.font = any.font;
        ui.Build();
        return ui;
    }

    void Build()
    {
        // Minimapa
        Image frame = NewImage("Minimapa", transform, new Color(0.05f, 0.05f, 0.07f, 0.75f));
        RectTransform frameRt = frame.rectTransform;
        frameRt.anchorMin = frameRt.anchorMax = frameRt.pivot = new Vector2(1f, 1f);
        frameRt.anchoredPosition = new Vector2(-20f, -20f);
        frameRt.sizeDelta = new Vector2(240f, 190f);

        // Contingut centrat (les sales es posen respecte del centre del requadre)
        GameObject content = new GameObject("Contingut", typeof(RectTransform), typeof(RectMask2D));
        content.transform.SetParent(frameRt, false);
        mapRoot = (RectTransform)content.transform;
        mapRoot.anchorMin = Vector2.zero;
        mapRoot.anchorMax = Vector2.one;
        mapRoot.offsetMin = mapRoot.offsetMax = Vector2.zero;

        TextMeshProUGUI floorText = NewText("Pis", frameRt, $"Pis {RunState.Floor}", 22);
        RectTransform fr = floorText.rectTransform;
        fr.anchorMin = new Vector2(0f, 0f); fr.anchorMax = new Vector2(1f, 0f); fr.pivot = new Vector2(0.5f, 1f);
        fr.anchoredPosition = new Vector2(0f, -4f); fr.sizeDelta = new Vector2(0f, 30f);

        // Rètol central
        banner = NewText("Retol", transform, "", 64);
        banner.rectTransform.anchoredPosition = new Vector2(0f, 180f);
        banner.rectTransform.sizeDelta = new Vector2(1200f, 100f);
        bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;

        RefreshMap();
        string theme = gen.Theme != null ? gen.Theme.themeName : "";
        ShowBanner($"Pis {RunState.Floor}" + (theme != "" ? " · " + theme : ""), 2.5f);
    }

    public void RefreshMap()
    {
        foreach (GameObject g in mapItems) Destroy(g);
        mapItems.Clear();

        // Què es veu: visitades + veïnes de les visitades
        var shown = new HashSet<DungeonGenerator.Room>();
        foreach (var r in gen.Rooms)
            if (r.visited) { shown.Add(r); foreach (var n in r.links) shown.Add(n); }

        Vector2 center = gen.CurrentRoom != null ? (Vector2)gen.CurrentRoom.slot : Vector2.zero;

        // Passadissos
        foreach (var r in shown)
            foreach (var n in r.links)
            {
                if (!shown.Contains(n) || !(r.visited || n.visited)) continue;
                Vector2 a = ((Vector2)r.slot - center) * Cell, b = ((Vector2)n.slot - center) * Cell;
                Image line = NewImage("Pas", mapRoot, new Color(1f, 1f, 1f, 0.35f));
                line.rectTransform.anchoredPosition = (a + b) / 2f;
                line.rectTransform.sizeDelta = a.x != b.x ? new Vector2(Cell, 4f) : new Vector2(4f, Cell);
                Keep(line.rectTransform);
            }

        foreach (var r in shown)
        {
            Color c = r.visited ? new Color(0.85f, 0.85f, 0.9f, 1f) : new Color(0.45f, 0.47f, 0.55f, 1f);
            if (r.type == DungeonGenerator.RoomType.Boss) c = r.visited ? new Color(0.95f, 0.3f, 0.3f) : new Color(0.7f, 0.2f, 0.2f, 0.8f);
            if (r.type == DungeonGenerator.RoomType.Treasure) c = new Color(1f, 0.8f, 0.25f, r.visited ? 1f : 0.7f);
            if (r == gen.CurrentRoom) c = new Color(1f, 0.95f, 0.4f);

            Image img = NewImage("Sala", mapRoot, c);
            img.rectTransform.anchoredPosition = ((Vector2)r.slot - center) * Cell;
            img.rectTransform.sizeDelta = Vector2.one * (r.type == DungeonGenerator.RoomType.Boss ? RoomSize + 4f : RoomSize);
            Keep(img.rectTransform);

            if (r.visited && !r.cleared && r.type == DungeonGenerator.RoomType.Combat)
            {
                Image dot = NewImage("Pendent", img.transform, new Color(0.9f, 0.2f, 0.25f));
                dot.rectTransform.sizeDelta = Vector2.one * 6f;
            }
        }
    }

    void Keep(RectTransform rt)
    {
        // El RectMask2D del contingut retalla el que ix del requadre
        Vector2 p = rt.anchoredPosition;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = p;
        mapItems.Add(rt.gameObject);
    }

    public void ShowBanner(string text, float time)
    {
        StopAllCoroutines();
        StartCoroutine(Banner(text, time));
    }

    IEnumerator Banner(string text, float time)
    {
        banner.text = text;
        for (float t = 0f; t < 0.3f; t += Time.deltaTime) { bannerGroup.alpha = t / 0.3f; yield return null; }
        bannerGroup.alpha = 1f;
        yield return new WaitForSeconds(time);
        for (float t = 0f; t < 0.6f; t += Time.deltaTime) { bannerGroup.alpha = 1f - t / 0.6f; yield return null; }
        bannerGroup.alpha = 0f;
    }

    public void ShowDeath(int floor, int coins)
    {
        Image dim = NewImage("Mort", transform, new Color(0f, 0f, 0f, 0f));
        RectTransform r = dim.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;

        TextMeshProUGUI t = NewText("Titol", dim.transform, "HAS MORT", 90);
        t.color = new Color(1f, 0.35f, 0.35f);
        t.rectTransform.anchoredPosition = new Vector2(0f, 40f);
        TextMeshProUGUI s = NewText("Info", dim.transform, $"Has arribat al pis {floor}  ·  {coins} monedes\nTornes a començar...", 32);
        s.rectTransform.anchoredPosition = new Vector2(0f, -70f);
        s.rectTransform.sizeDelta = new Vector2(1200f, 120f);
        StartCoroutine(FadeIn(dim));
    }

    IEnumerator FadeIn(Image dim)
    {
        for (float t = 0f; t < 0.8f; t += Time.deltaTime) { dim.color = new Color(0f, 0f, 0f, 0.8f * t / 0.8f); yield return null; }
    }

    Image NewImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image i = go.GetComponent<Image>();
        i.color = color;
        i.raycastTarget = false;
        return i;
    }

    TextMeshProUGUI NewText(string name, Transform parent, string text, float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.rectTransform.sizeDelta = new Vector2(1000f, size * 1.5f);
        return t;
    }
}
