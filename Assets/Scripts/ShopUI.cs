using TMPro;
using UnityEngine;
using UnityEngine.UI;

// v1.4.1: interfície del mercader i de la màquina expenedora (panell 1600×900 de referència, estil UIKit).
// Esq: retrat i frase. Centre: targetes de l'estoc. Baix: COMPRAR, monedes i TANCAR (Esc).
public class ShopUI : MonoBehaviour
{
    public static bool IsOpen => instance != null;
    static ShopUI instance;

    Shop shop;
    GameObject canvas;
    RectTransform cards;
    TextMeshProUGUI coins, detail;
    UIKit.UIButton buy;
    int selected = -1;

    public static void Open(Shop s)
    {
        if (instance != null || s == null) return;
        var go = new GameObject("Botiga UI"); instance = go.AddComponent<ShopUI>(); instance.shop = s; instance.Build();
    }

    public static void Refresh() { if (instance != null) instance.Fill(); }

    public static void CloseNow() { if (instance != null) instance.Close(); }

    void Build()
    {
        GameControls.Editing = true; // el joc no rep moviment ni atac mentre la botiga està oberta
        canvas = UIKit.Canvas("Botiga", 95);
        canvas.transform.SetParent(transform, false);
        var dim = UIKit.Box(canvas.transform, Vector2.zero, new Vector2(4000, 3000), new Color(0, 0, 0, .6f), "Fosc");
        var panel = UIKit.Box(canvas.transform, Vector2.zero, new Vector2(1320, 720), UIKit.Panel, "Panell botiga");
        UIKit.Box(panel, new Vector2(0, 320), new Vector2(1320, 80), UIKit.Ink, "Capcalera");
        UIKit.Text(panel, shop.Title, new Vector2(0, 322), new Vector2(900, 70), 42, TextAlignmentOptions.Center, UIKit.Gold);

        // Esquerra: retrat i globus amb la frase
        var portrait = UIKit.Box(panel, new Vector2(-500, 90), new Vector2(260, 260), UIKit.PanelLight, "Retrat");
        var picture = UIKit.Picture(portrait, shop.Portrait, Vector2.zero, new Vector2(240, 240));
        if (picture != null) picture.preserveAspect = true;
        detail = UIKit.Text(panel, shop.IsMerchant ? "Mira, mira... i compra." : "Només articles de cura.", new Vector2(-500, -150), new Vector2(280, 180), 22, TextAlignmentOptions.Top, UIKit.Muted);

        cards = UIKit.Empty(panel, "Targetes");
        coins = UIKit.Text(panel, "", new Vector2(-200, -315), new Vector2(380, 50), 30, TextAlignmentOptions.MidlineLeft, UIKit.Gold);
        buy = UIKit.Button(panel, "COMPRAR", new Vector2(200, -315), new Vector2(300, 64), () => { if (selected >= 0) shop.Buy(selected); }, UIKit.Style.Success, 30);
        UIKit.Button(panel, "TANCAR (Esc)", new Vector2(520, -315), new Vector2(240, 64), Close, UIKit.Style.Normal, 24);
        selected = -1;
        Fill();
    }

    void Fill()
    {
        UIKit.Clear(cards);
        int n = shop.slots.Length, columns = n <= 3 ? n : 3;
        for (int i = 0; i < n; i++)
        {
            int index = i; var s = shop.slots[i];
            int col = i % 3, row = i / 3;
            float x = 150f + (col - (columns - 1) / 2f) * 300f, y = 150f - row * 240f;
            UIKit.Box(cards, new Vector2(x, y), new Vector2(i == selected ? 292 : 284, i == selected ? 232 : 224), i == selected ? UIKit.Gold : s.sold ? UIKit.Dim : ItemGraphics.RarityColor(s.rarity), "Vora");
            var card = UIKit.Box(cards, new Vector2(x, y), new Vector2(280, 220), i == selected ? UIKit.RowSelected : UIKit.Row, "Targeta");
            var button = card.gameObject.AddComponent<Button>(); button.onClick.AddListener(() => { selected = index; Fill(); });
            if (s.icon != null) { var pic = UIKit.Picture(card, s.icon, new Vector2(0, 40), new Vector2(110, 110)); if (pic != null) { pic.preserveAspect = true; pic.color = s.sold ? new Color(1, 1, 1, .25f) : Color.white; } }
            else UIKit.Text(card, s.repair ? "+" : "?", new Vector2(0, 40), new Vector2(110, 110), 80, TextAlignmentOptions.Center, UIKit.Gold);
            UIKit.Text(card, s.title, new Vector2(0, -42), new Vector2(262, 40), 22, TextAlignmentOptions.Center, s.sold ? UIKit.Dim : UIKit.TextColor);
            int price = shop.PriceOf(s);
            bool afford = RunState.Coins >= price;
            string label = s.sold ? "VENUT" : s.repair && price <= 0 ? "—" : price + " ●";
            UIKit.Text(card, label, new Vector2(0, -85), new Vector2(262, 40), 28, TextAlignmentOptions.Center, s.sold ? UIKit.Dim : afford ? UIKit.Gold : UIKit.Red);
            if (s.offer && !s.sold) UIKit.Text(card, "OFERTA", new Vector2(85, 90), new Vector2(110, 30), 18, TextAlignmentOptions.Center, UIKit.Green);
        }
        if (selected >= 0 && selected < n) detail.text = shop.slots[selected].info;
        coins.text = "Monedes: " + RunState.Coins;
        var chosen = selected >= 0 && selected < n ? shop.slots[selected] : null;
        bool canBuy = chosen != null && !chosen.sold && RunState.Coins >= shop.PriceOf(chosen) && !(chosen.repair && shop.PriceOf(chosen) <= 0);
        buy.button.interactable = canBuy;
    }

    float lastCoins = -1;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || (shop != null && ClosePlayer())) { Close(); return; }
        if (lastCoins != RunState.Coins) { lastCoins = RunState.Coins; Fill(); } // les monedes poden canviar mentre està oberta
    }

    // Si t'allunyes del mercader (o et peguen i et dus lluny), la botiga es tanca
    bool ClosePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        return player == null || Vector2.Distance(player.transform.position, shop.transform.position) > 4f;
    }

    void Close()
    {
        if (shop != null) shop.Say(shop.IsMerchant ? "Cuida't! I si veus un Mímic, no el toques." : null);
        GameControls.Editing = false;
        instance = null;
        Destroy(gameObject);
    }

    void OnDestroy() { if (instance == this) { instance = null; GameControls.Editing = false; } }
}
