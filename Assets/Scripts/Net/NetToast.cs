using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Avisos curts a dalt de la pantalla ("Codi copiat", "L'amfitrio ha tancat la sala"...).
public class NetToast : MonoBehaviour
{
    static NetToast instance;
    class Item { public RectTransform rect; public CanvasGroup group; public float born, life; }
    readonly List<Item> items = new List<Item>();
    Transform root;

    public static void Show(string message, float seconds = 4f)
    {
        if (instance == null)
        {
            var go = UIKit.Canvas("Avisos online", 400);
            DontDestroyOnLoad(go);
            go.GetComponent<GraphicRaycaster>().enabled = false;
            instance = go.AddComponent<NetToast>();
            instance.root = go.transform;
        }
        instance.Add(message, seconds);
    }

    void Add(string message, float seconds)
    {
        var r = UIKit.Box(root, Vector2.zero, new Vector2(900, 64), new Color(.04f, .09f, .12f, .96f), "Avis");
        r.anchorMin = r.anchorMax = new Vector2(.5f, 1); r.pivot = new Vector2(.5f, 1);
        r.GetComponent<Image>().raycastTarget = false;
        var bar = UIKit.Box(r, Vector2.zero, new Vector2(8, 64), UIKit.Gold); bar.anchorMin = bar.anchorMax = new Vector2(0, .5f); bar.anchoredPosition = new Vector2(4, 0);
        var t = UIKit.Text(r, message, Vector2.zero, new Vector2(860, 60), 23, TextAlignmentOptions.Center);
        var g = r.gameObject.AddComponent<CanvasGroup>(); g.blocksRaycasts = false;
        items.Insert(0, new Item { rect = r, group = g, born = Time.unscaledTime, life = seconds });
        if (items.Count > 4) { Destroy(items[4].rect.gameObject); items.RemoveAt(4); }
    }

    void Update()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var it = items[i];
            float age = Time.unscaledTime - it.born;
            if (age > it.life) { Destroy(it.rect.gameObject); items.RemoveAt(i); continue; }
            float target = -24 - i * 74;
            var p = it.rect.anchoredPosition;
            it.rect.anchoredPosition = new Vector2(0, Mathf.Lerp(p.y == 0 ? target + 40 : p.y, target, 1 - Mathf.Exp(-14 * Time.unscaledDeltaTime)));
            it.group.alpha = Mathf.Clamp01(Mathf.Min(age / 0.15f, (it.life - age) / 0.4f));
        }
    }
}
