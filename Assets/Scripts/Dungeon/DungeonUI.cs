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




    private DungeonGenerator gen;
    private TMP_FontAsset font;

    private TextMeshProUGUI banner, keyText;
    private CanvasGroup bannerGroup;


    public static DungeonUI Create(DungeonGenerator gen)
    {
        GameObject go = new GameObject("UI Masmorra", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
        Image frame = NewImage("Radar", transform, Color.clear);
        RectTransform frameRt=frame.rectTransform;
        frameRt.anchorMin=frameRt.anchorMax=frameRt.pivot=Vector2.one;
        frameRt.anchoredPosition=new Vector2(-20,-86);
        frameRt.sizeDelta=new Vector2(240,190);
        frame.gameObject.AddComponent<DungeonRadar>().gen=gen;
        var floorText=NewText("Pis",frameRt,"Pis " + RunState.Floor + " · N ↑",20);
        var fr=floorText.rectTransform;fr.anchorMin=fr.anchorMax=new Vector2(0.5f,0);fr.anchoredPosition=new Vector2(0,-22);fr.sizeDelta=new Vector2(240,40);
        keyText = NewText("Clau del boss", frameRt, gen.BossKeyLabel, 18);
        keyText.color = UIKit.Gold;
        var keyRect = keyText.rectTransform;
        keyRect.anchorMin = keyRect.anchorMax = new Vector2(.5f, 0f);
        keyRect.anchoredPosition = new Vector2(0f, -46f);
        keyRect.sizeDelta = new Vector2(240, 28);
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

    void LateUpdate()
    {
        if (keyText != null && keyText.text != gen.BossKeyLabel) keyText.text = gen.BossKeyLabel;
    }

    public void RefreshMap() { } // El radar actualitza el terreny en temps real.

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

    // Resum de partida en morir: dades de la partida, records i botons.
    // waitForHost: client online, nomes pot esperar o eixir (l'amfitrio decideix).
    public void ShowSummary(List<RunStats.Record> records, bool coop, bool waitForHost,
        UnityEngine.Events.UnityAction again, UnityEngine.Events.UnityAction menu, string menuLabel)
    {
        Time.timeScale = 1f;
        StopAllCoroutines();
        Transform root = UIKit.Canvas("Resum de partida", 60).transform;
        Image dim = NewImage("Fosc", root, new Color(0f, 0f, 0f, 0f));
        RectTransform r = dim.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        dim.raycastTarget = true; // Que no es puga clicar el que hi ha davall
        StartCoroutine(FadeIn(dim));

        UIKit.Text(root, "HAS MORT", new Vector2(0, 330), new Vector2(1200, 110), 84, TextAlignmentOptions.Center, new Color(1f, 0.35f, 0.35f));
        UIKit.Text(root, coop ? "Partida cooperativa" : "Partida en solitari", new Vector2(0, 268), new Vector2(1200, 40), 26, TextAlignmentOptions.Center, UIKit.Muted);

        // Resum
        RectTransform left = UIKit.Box(root, new Vector2(-270, 10), new Vector2(500, 430), new Color(0.04f, 0.07f, 0.1f, 0.92f), "Resum");
        UIKit.Text(left, "RESUM", new Vector2(0, 180), new Vector2(460, 44), 30, TextAlignmentOptions.Center, UIKit.Gold);
        string fav = RunStats.FavoriteWeapon(out int favKills);
        var rows = new (string, string)[]
        {
            ("Pis màxim", RunStats.MaxFloor.ToString()),
            ("Temps", RunStats.TimeText()),
            ("Monedes guanyades", RunStats.CoinsEarned.ToString()),
            ("Enemics", RunStats.Kills.ToString()),
            ("Bosses", RunStats.BossKills.ToString()),
            ("Arma favorita", fav != null ? $"{fav} ({favKills})" : "—"),
            ("Sales netejades", RunStats.RoomsCleared.ToString()),
            ("Cofres oberts", RunStats.ChestsOpened.ToString()),
        };
        for (int i = 0; i < rows.Length; i++)
        {
            float y = 125 - i * 42;
            UIKit.Text(left, rows[i].Item1, new Vector2(-100, y), new Vector2(260, 38), 24, TextAlignmentOptions.MidlineLeft);
            UIKit.Text(left, rows[i].Item2, new Vector2(115, y), new Vector2(220, 38), 24, TextAlignmentOptions.MidlineRight, Color.white);
        }

        // Records
        RectTransform right = UIKit.Box(root, new Vector2(270, 10), new Vector2(500, 430), new Color(0.04f, 0.07f, 0.1f, 0.92f), "Records");
        UIKit.Text(right, coop ? "RÈCORDS · COOPERATIU" : "RÈCORDS · SOL", new Vector2(0, 180), new Vector2(460, 44), 30, TextAlignmentOptions.Center, UIKit.Gold);
        UIKit.Text(right, "Ara", new Vector2(10, 128), new Vector2(100, 30), 20, TextAlignmentOptions.Center, UIKit.Muted);
        UIKit.Text(right, "Millor", new Vector2(150, 128), new Vector2(160, 30), 20, TextAlignmentOptions.Center, UIKit.Muted);
        for (int i = 0; i < records.Count; i++)
        {
            float y = 80 - i * 58;
            var rec = records[i];
            UIKit.Text(right, rec.label, new Vector2(-150, y), new Vector2(180, 40), 24, TextAlignmentOptions.MidlineLeft);
            UIKit.Text(right, rec.value.ToString(), new Vector2(10, y), new Vector2(100, 40), 26, TextAlignmentOptions.Center, Color.white);
            UIKit.Text(right, rec.isNew ? "NOU RÈCORD!" : rec.best.ToString(), new Vector2(150, y), new Vector2(180, 40), rec.isNew ? 22 : 26,
                TextAlignmentOptions.Center, rec.isNew ? UIKit.Gold : UIKit.TextColor);
        }
        UIKit.Text(right, "El Mode Creatiu no compta.", new Vector2(0, -185), new Vector2(460, 30), 18, TextAlignmentOptions.Center, UIKit.Dim);

        // Botons
        if (waitForHost)
        {
            UIKit.Text(root, "Esperant que l'amfitrió trie...", new Vector2(0, -268), new Vector2(900, 40), 26, TextAlignmentOptions.Center, UIKit.Muted);
            UIKit.Button(root, "EIXIR DE LA SALA", new Vector2(0, -330), new Vector2(300, 56), menu, UIKit.Style.Ghost, 22);
        }
        else
        {
            UIKit.Button(root, "TORNAR A JUGAR", new Vector2(-170, -300), new Vector2(300, 64), again, UIKit.Style.Primary, 26);
            UIKit.Button(root, menuLabel, new Vector2(170, -300), new Vector2(300, 64), menu, UIKit.Style.Normal, 26);
        }
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
