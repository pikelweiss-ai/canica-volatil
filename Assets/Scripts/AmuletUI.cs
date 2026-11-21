using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AmuletUI : MonoBehaviour
{
    readonly Image[] borders = new Image[RunAmulets.Slots];
    readonly Image[] rings = new Image[RunAmulets.Slots];
    static Sprite ringSprite;
    readonly Image[] icons = new Image[RunAmulets.Slots];
    readonly TextMeshProUGUI[] marks = new TextMeshProUGUI[RunAmulets.Slots];
    readonly TextMeshProUGUI[] timers = new TextMeshProUGUI[RunAmulets.Slots];
    TextMeshProUGUI tooltip;
    GameObject modal, party;
    RectTransform modalBody;
    Amulet[] displayedOffer;
    int replaceOffer = -1;
    int confirmDiscard = -1;
    bool ownsInput;

    public static void Attach(Transform canvas, float width)
    {
        var go = new GameObject("Amulets", typeof(RectTransform));
        go.transform.SetParent(canvas, false);
        var root = (RectTransform)go.transform;
        root.anchorMin = root.anchorMax = root.pivot = new Vector2(.5f, 0f);
        root.anchoredPosition = new Vector2(-width / 2f - 16f, 20f);
        go.AddComponent<AmuletUI>().Build(root);
    }

    void Build(RectTransform root)
    {
        for (int i = 0; i < RunAmulets.Slots; i++)
        {
            int index = i;
            bool mobile = MobileInput.Active;
            Vector2 at = mobile ? new Vector2(-32, 33 + i * 50) : new Vector2(-35 - (2 - i) * 71, 37);
            var slot = UIKit.Box(root, at, mobile ? new Vector2(47, 47) : new Vector2(65, 65), UIKit.Dim, "Ranura d'amulet");
            borders[i] = slot.GetComponent<Image>();
            var face = UIKit.Box(slot, Vector2.zero, mobile ? new Vector2(41, 41) : new Vector2(57, 57), UIKit.Panel, "Fons");
            var icon = UIKit.Box(face, Vector2.zero, mobile ? new Vector2(34, 34) : new Vector2(48, 48), Color.white, "Icona");
            icons[i] = icon.GetComponent<Image>();
            icons[i].preserveAspect = true;
            icons[i].raycastTarget = false;
            marks[i] = UIKit.Text(face, "+", Vector2.zero, new Vector2(35, 35), 24, TextAlignmentOptions.Center, UIKit.Muted);
            rings[i] = UIKit.Picture(face, CooldownRing(), Vector2.zero,
                mobile ? new Vector2(39, 39) : new Vector2(53, 53));
            rings[i].color = UIKit.Gold;
            rings[i].type = Image.Type.Filled;
            rings[i].fillMethod = Image.FillMethod.Radial360;
            rings[i].raycastTarget = false;
            timers[i] = UIKit.Text(face, "", new Vector2(0, -19), new Vector2(44, 22), 16, TextAlignmentOptions.Center, UIKit.Gold);
            var input = slot.gameObject.AddComponent<AmuletSlotInput>();
            input.ui = this;
            input.index = index;
        }
        tooltip = UIKit.Text(root, "", new Vector2(-135, MobileInput.Active ? 180 : 101), new Vector2(300, 58),
            20, TextAlignmentOptions.Center, UIKit.TextColor);
        RunAmulets.Changed += Refresh;
        Refresh();
    }

    static Sprite CooldownRing()
    {
        if (ringSprite != null) return ringSprite;
        const int size = 32;
        var texture = new Texture2D(size, size) { filterMode = FilterMode.Point };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float radius = new Vector2(x - 15.5f, y - 15.5f).magnitude;
                texture.SetPixel(x, y, radius >= 13f && radius < 15.5f ? Color.white : Color.clear);
            }
        texture.Apply();
        ringSprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f, size);
        return ringSprite;
    }

    void OnDestroy()
    {
        RunAmulets.Changed -= Refresh;
        if (ownsInput) GameControls.Editing = false;
        if (modal != null) Destroy(modal);
        if (party != null) Destroy(party);
    }

    void Update()
    {
        for (int i = 0; i < RunAmulets.Slots; i++)
        {
            var id = RunAmulets.Local.Equipped[i];
            float until = id == Amulet.GlassShield ? RunAmulets.Local.GlassReady : id == Amulet.FallenStar ? RunAmulets.Local.StarReady : 0f;
            float remaining = Mathf.Max(0f, until - Time.time);
            timers[i].text = remaining > 0f ? Mathf.CeilToInt(remaining).ToString() : "";
            rings[i].enabled = remaining > 0f;
            rings[i].fillAmount = remaining / (id == Amulet.GlassShield ? 15f : 10f);
        }
        if (NetCoop.Online && GameControls.Down(7))
        {
            if (party != null) { Destroy(party); party = null; }
            else ShowParty();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && confirmDiscard >= 0) CloseModal();
    }

    void Refresh()
    {
        var kit = RunAmulets.Local;
        for (int i = 0; i < RunAmulets.Slots; i++)
        {
            var info = RunAmulets.Info(kit.Equipped[i]);
            borders[i].color = info != null ? ItemGraphics.RarityColor(info.rarity) : UIKit.Dim;
            icons[i].sprite = info?.Icon;
            icons[i].enabled = icons[i].sprite != null;
            marks[i].enabled = info == null;
        }
        if (party != null) ShowParty();
        if (kit.Offer == displayedOffer) return;
        displayedOffer = kit.Offer;
        replaceOffer = -1;
        if (kit.Offer.Length > 0) ShowOffer();
        else if (confirmDiscard < 0) CloseModal();
    }

    void ShowParty()
    {
        if (party != null) Destroy(party);
        party = UIKit.Canvas("Amulets de l'equip", 42);
        var panel = UIKit.Box(party.transform, new Vector2(515, 105), new Vector2(505, 420), UIKit.Panel, "Amulets de l'equip");
        UIKit.Text(panel, "AMULETS DE L’EQUIP", new Vector2(0, 177), new Vector2(450, 36), 27,
            TextAlignmentOptions.Center, UIKit.Gold);
        int row = 0;
        foreach (var peer in NetSession.Instance.Players.Values)
        {
            var kit = RunAmulets.Get(peer.id);
            string title = NetSession.Safe(peer.name);
            float y = 135 - row++ * 40;
            UIKit.Text(panel, title, new Vector2(-102, y), new Vector2(210, 34), 19, TextAlignmentOptions.Left, UIKit.TextColor);
            for (int i = 0; i < RunAmulets.Slots; i++)
            {
                var info = RunAmulets.Info(kit.Equipped[i]);
                var backing = UIKit.Box(panel, new Vector2(45 + i * 46, y), new Vector2(34, 34),
                    info != null ? ItemGraphics.RarityColor(info.rarity) : UIKit.Dim, "Ranura");
                var sprite = info?.Icon;
                if (sprite != null) UIKit.Picture(backing, sprite, Vector2.zero, new Vector2(29, 29));
            }
        }
    }

    public void Hover(int slot)
    {
        var info = RunAmulets.Info(RunAmulets.Local.Equipped[slot]);
        tooltip.text = info != null ? info.name + "\n" + info.description : "";
    }

    public void HintOff() => tooltip.text = "";

    public void Swap(int from, int to)
    {
        if (from != to) RunAmulets.Request(RunAmulets.Action.Swap, from, to);
        Hover(to);
    }

    public void AskDiscard(int index)
    {
        if (RunAmulets.Local.Offer.Length > 0 || ShopUI.IsOpen) return;
        var info = RunAmulets.Info(RunAmulets.Local.Equipped[index]);
        if (info == null) return;
        confirmDiscard = index;
        StartModal("Descartar " + info.name + "?", new Vector2(540, 230));
        UIKit.Button(modalBody, "DESCARTAR", new Vector2(-118, -65), new Vector2(207, 57), () =>
        {
            RunAmulets.Request(RunAmulets.Action.Discard, confirmDiscard);
            CloseModal();
        }, UIKit.Style.Danger, 23);
        UIKit.Button(modalBody, "CANCEL·LAR", new Vector2(118, -65), new Vector2(207, 57), CloseModal, UIKit.Style.Normal, 23);
    }

    void ShowOffer()
    {
        ShopUI.CloseNow();
        StartModal(replaceOffer < 0 ? "Tria un amulet" : "Quin amulet vols substituir?", new Vector2(1080, 445));
        for (int i = 0; i < RunAmulets.Slots; i++)
        {
            int choice = i;
            var kit = RunAmulets.Local;
            if (replaceOffer < 0 && i >= kit.Offer.Length) break;
            var info = RunAmulets.Info(replaceOffer < 0 ? kit.Offer[i] : kit.Equipped[i]);
            float x = (i - 1) * 315;
            var card = UIKit.Box(modalBody, new Vector2(x, -7), new Vector2(290, 275),
                info != null ? ItemGraphics.RarityColor(info.rarity) : UIKit.Dim, "Amulet");
            var face = UIKit.Box(card, Vector2.zero, new Vector2(282, 267), UIKit.PanelLight, "Fitxa");
            if (info?.Icon != null) UIKit.Picture(face, info.Icon, new Vector2(0, 72), new Vector2(64, 64));
            UIKit.Text(face, info?.name ?? "Ranura buida", new Vector2(0, 19), new Vector2(268, 44), 24,
                TextAlignmentOptions.Center, UIKit.Gold);
            UIKit.Text(face, info?.description ?? "", new Vector2(0, -39), new Vector2(257, 64), 21,
                TextAlignmentOptions.Center, UIKit.TextColor);
            UIKit.Button(face, replaceOffer < 0 ? "TRIAR" : "SUBSTITUIR", new Vector2(0, -101), new Vector2(200, 46), () =>
            {
                if (replaceOffer >= 0) { RunAmulets.Request(RunAmulets.Action.Choose, replaceOffer, choice); return; }
                int free = System.Array.IndexOf(RunAmulets.Local.Equipped, Amulet.None);
                if (free >= 0) RunAmulets.Request(RunAmulets.Action.Choose, choice, free);
                else { replaceOffer = choice; ShowOffer(); }
            }, UIKit.Style.Success, 21);
        }
        if (replaceOffer >= 0)
            UIKit.Button(modalBody, "TORNAR", new Vector2(0, -184), new Vector2(166, 45),
                () => { replaceOffer = -1; ShowOffer(); }, UIKit.Style.Normal, 20);
    }

    void StartModal(string title, Vector2 size)
    {
        if (modal != null) Destroy(modal);
        modal = UIKit.Canvas(title, 93);
        UIKit.Box(modal.transform, Vector2.zero, new Vector2(4000, 3000), new Color(0, 0, 0, .65f), "Fons");
        modalBody = UIKit.Box(modal.transform, Vector2.zero, size, UIKit.Panel, title);
        UIKit.Text(modalBody, title, new Vector2(0, size.y / 2f - 43), new Vector2(size.x - 26, 70), 33,
            TextAlignmentOptions.Center, UIKit.Gold);
        if (!GameControls.Editing) { ownsInput = true; GameControls.Editing = true; }
    }

    void CloseModal()
    {
        if (modal != null) Destroy(modal);
        modal = null;
        confirmDiscard = -1;
        if (ownsInput) { GameControls.Editing = false; ownsInput = false; }
    }
}

public class AmuletSlotInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IDropHandler
{
    public AmuletUI ui;
    public int index;
    float pressedAt;
    bool held;

    public void OnPointerEnter(PointerEventData e) { if (!MobileInput.Active) ui.Hover(index); }
    public void OnPointerExit(PointerEventData e) { ui.HintOff(); held = false; }
    public void OnPointerDown(PointerEventData e) { held = true; pressedAt = Time.unscaledTime; }
    public void OnPointerUp(PointerEventData e) { held = false; }
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Right || (MobileInput.Active && Time.unscaledTime - pressedAt < .33f))
            ui.AskDiscard(index);
    }
    void Update() { if (held && Time.unscaledTime - pressedAt >= .33f) ui.Hover(index); }
    public void OnBeginDrag(PointerEventData e) { held = false; }
    public void OnDrag(PointerEventData e) { }
    public void OnDrop(PointerEventData e)
    {
        if (e.pointerDrag != null && e.pointerDrag.TryGetComponent<AmuletSlotInput>(out var source))
            ui.Swap(source.index, index);
    }
}

