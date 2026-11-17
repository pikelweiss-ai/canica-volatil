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

    private TextMeshProUGUI banner;
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

    public void ShowDeath(int floor, int coins)
    {

        Time.timeScale=1f;
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
