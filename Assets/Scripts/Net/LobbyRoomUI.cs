using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Pantalla de sala mentre s'espera: els jugadors es mouen pel lobby, xategen i es posen LLEST.
// L'amfitrio tria el mode i comença quan tots estan llestos (compte enrere de 3 segons).
public class LobbyRoomUI : MonoBehaviour
{
    NetSession s;
    TextMeshProUGUI roomTitle, roomSub, codeText, playersTitle, countdown, hint;
    RectTransform playerList, hostBox;
    UIKit.UIButton mainButton, modeDungeon, modeFree;
    float nextRefresh;
    int lastCountdown = -1;

    public static void Create()
    {
        var go = UIKit.Canvas("Sala online", 80);
        go.AddComponent<LobbyRoomUI>();
    }

    void Start()
    {
        s = NetSession.Instance;
        if (s == null || !NetSession.Active) { Destroy(gameObject); return; }
        Build();
        s.Changed += Refresh;
        Refresh();
    }

    void OnDestroy() { if (s != null) s.Changed -= Refresh; }

    void Build()
    {
        var root = transform;
        // Targeta de la sala (dalt al centre)
        var card = UIKit.Box(root, new Vector2(-40, 368), new Vector2(760, 120), new Color(.04f, .08f, .11f, .9f), "Sala");
        UIKit.Box(card, new Vector2(0, 57), new Vector2(760, 6), UIKit.Gold);
        roomTitle = UIKit.Text(card, "", new Vector2(-165, 24), new Vector2(400, 44), 28, TextAlignmentOptions.MidlineLeft, UIKit.Gold);
        roomSub = UIKit.Text(card, "", new Vector2(-165, -22), new Vector2(400, 50), 15, TextAlignmentOptions.TopLeft, UIKit.Muted);
        UIKit.Text(card, "CODI", new Vector2(150, 34), new Vector2(120, 26), 16, TextAlignmentOptions.MidlineLeft, UIKit.Muted);
        codeText = UIKit.Text(card, "", new Vector2(190, -8), new Vector2(180, 56), 36, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
        codeText.characterSpacing = 3;
        UIKit.Button(card, "COPIAR", new Vector2(327, -8), new Vector2(90, 44), CopyCode, UIKit.Style.Normal, 17);

        countdown = UIKit.Text(root, "", new Vector2(-40, 150), new Vector2(700, 220), 170, TextAlignmentOptions.Center, UIKit.Gold);
        countdown.outlineWidth = 0.22f; countdown.outlineColor = new Color32(10, 18, 26, 255);

        // Panell de jugadors (dreta)
        var panel = UIKit.Box(root, new Vector2(600, 20), new Vector2(380, 680), new Color(.04f, .08f, .11f, .92f), "Jugadors");
        playersTitle = UIKit.Text(panel, "", new Vector2(0, 305), new Vector2(340, 40), 23, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
        var listHolder = UIKit.Empty(panel, "Llista"); listHolder.anchorMin = new Vector2(0, 1); listHolder.anchorMax = new Vector2(1, 1); listHolder.pivot = new Vector2(.5f, 1); listHolder.sizeDelta = new Vector2(-20, 420); listHolder.anchoredPosition = new Vector2(0, -58);
        var vl = listHolder.gameObject.AddComponent<VerticalLayoutGroup>(); vl.spacing = 6; vl.childControlHeight = true; vl.childControlWidth = true; vl.childForceExpandHeight = false;
        playerList = listHolder;

        hostBox = UIKit.Empty(panel, "Amfitrio");
        UIKit.Text(hostBox, "MODE DE JOC", new Vector2(0, -158), new Vector2(340, 28), 17, TextAlignmentOptions.MidlineLeft, UIKit.Muted);
        modeDungeon = UIKit.Button(hostBox, "MASMORRA", new Vector2(-88, -192), new Vector2(166, 42), () => s.SetMode(RoomMode.Dungeon), UIKit.Style.Normal, 19);
        modeFree = UIKit.Button(hostBox, "MAPA LLIURE", new Vector2(88, -192), new Vector2(166, 42), () => s.SetMode(RoomMode.FreeMap), UIKit.Style.Normal, 19);

        mainButton = UIKit.Button(panel, "", new Vector2(0, -250), new Vector2(340, 60), MainAction, UIKit.Style.Primary, 23);
        UIKit.Button(panel, "EIXIR DE LA SALA", new Vector2(0, -308), new Vector2(340, 40), LeaveRoom, UIKit.Style.Danger, 17);

        UIKit.Box(root, new Vector2(-40, 290), new Vector2(760, 30), new Color(.02f, .04f, .06f, .7f), "Ajuda").GetComponent<Image>().raycastTarget = false;
        hint = UIKit.Text(root, MobileInput.Active ? "Joystick per moure't · Toca el xat i ESTIC LLEST" : "WASD per moure't · Intro per xatejar · R per estar llest", new Vector2(-40, 290), new Vector2(740, 26), 16, TextAlignmentOptions.Center, UIKit.Muted);

        ChatBox.Create(root, MobileInput.Active?new Vector2(-560,65):new Vector2(-560,-235), MobileInput.Active?new Vector2(460,240):new Vector2(460,300), false);
    }

    void CopyCode()
    {
        GUIUtility.systemCopyBuffer = s.Room.code;
        NetToast.Show("Codi " + s.Room.code + " copiat. Passa'l als teus amics!", 2.5f);
    }

    void MainAction()
    {
        if (NetSession.IsHost)
        {
            if (s.Room.phase == RoomPhase.Countdown) s.CancelCountdown("L'amfitrió ha aturat el compte enrere.");
            else s.StartCountdown();
        }
        else
        {
            var me = s.Local;
            if (me != null) s.SetReady(!me.ready);
        }
        Refresh();
    }

    void LeaveRoom()
    {
        s.Leave(null);
        NetSession.ReopenBrowser = true;
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        if (s == null || !NetSession.Active) return;
        if (!TypingGuard.Typing && Input.GetKeyDown(KeyCode.R)) MainAction();
        if (Time.unscaledTime >= nextRefresh) { nextRefresh = Time.unscaledTime + 1f; Refresh(); }
        int c = s.Room.phase == RoomPhase.Countdown ? Mathf.CeilToInt(s.CountdownLeft) : -1;
        if (c != lastCountdown)
        {
            lastCountdown = c;
            countdown.text = c > 0 ? c.ToString() : c == 0 ? "JA!" : "";
            if (c >= 0) countdown.transform.localScale = Vector3.one * 1.35f;
            Refresh();
        }
        countdown.transform.localScale = Vector3.Lerp(countdown.transform.localScale, Vector3.one, 1 - Mathf.Exp(-10 * Time.unscaledDeltaTime));
    }

    void Refresh()
    {
        if (this == null || s == null || !NetSession.Active || roomTitle == null) return;
        var room = s.Room;
        roomTitle.text = NetSession.Safe(room.name);
        roomSub.text = RoomModes.Label(room.mode) + (room.isPublic ? " · PÚBLICA" : " · PRIVADA") + (room.hasPassword ? " · CONTRASENYA" : "") + "\n<size=90%>" + NetSession.Safe(s.NetworkLabel) + "</size>";
        codeText.text = room.code;
        playersTitle.text = "JUGADORS  <color=#8AA4A8>" + s.Players.Count + " / " + room.maxPlayers + "</color>";

        UIKit.Clear(playerList);
        foreach (var pl in s.Players.Values)
        {
            var row = UIKit.ListRow(playerList, 46, pl.id == s.LocalId ? new Color(.12f, .22f, .27f) : UIKit.Row);
            UIKit.Box(row, Vector2.zero, new Vector2(38, 38), UIKit.Ink).anchoredPosition = Vector2.zero;
            var icon = UIKit.Picture(row, UIKit.SkinSprite(pl.skin), Vector2.zero, new Vector2(32, 32));
            UIKit.AnchorLeft((RectTransform)icon.transform, 26);
            UIKit.AnchorLeft((RectTransform)row.GetChild(0), 26);
            string name = NetSession.Safe(pl.name) + (pl.id == s.LocalId ? " <color=#8AA4A8>(tu)</color>" : "");
            var nameCell = UIKit.Cell(row, name, 54, 190, 18);
            nameCell.rectTransform.sizeDelta = new Vector2(190, -22); nameCell.rectTransform.anchoredPosition = new Vector2(54, 8);
            string state = pl.isHost ? "<color=#EBB347>AMFITRIÓ</color>" : pl.ready ? "<color=#5CC772>LLEST</color>" : "<color=#8AA4A8>esperant...</color>";
            var stateCell = UIKit.Cell(row, state, 54, 190, 14);
            stateCell.rectTransform.sizeDelta = new Vector2(190, -26); stateCell.rectTransform.anchoredPosition = new Vector2(54, -11);
            if (pl.id != s.LocalId && !pl.isHost)
                UIKit.Cell(row, pl.ping + " ms", 246, 66, 14, TextAlignmentOptions.MidlineRight, UIKit.Dim);
            if (NetSession.IsHost && pl.id != s.LocalId)
            {
                int id = pl.id;
                var kick = UIKit.Button(row, "X", Vector2.zero, new Vector2(32, 32), () => s.Kick(id), UIKit.Style.Danger, 18, false);
                UIKit.AnchorRight(kick.rect, 26);
            }
        }

        bool host = NetSession.IsHost;
        hostBox.gameObject.SetActive(host);
        if (host)
        {
            modeDungeon.SetStyle(room.mode == RoomMode.Dungeon ? UIKit.Style.Primary : UIKit.Style.Normal);
            modeFree.SetStyle(room.mode == RoomMode.FreeMap ? UIKit.Style.Primary : UIKit.Style.Normal);
            bool lobby = room.phase == RoomPhase.Lobby;
            modeDungeon.SetInteractable(lobby); modeFree.SetInteractable(lobby);
            if (room.phase == RoomPhase.Countdown) { mainButton.SetStyle(UIKit.Style.Normal); mainButton.SetLabel("ATURAR"); mainButton.SetInteractable(true); }
            else if (s.EveryoneReady) { mainButton.SetStyle(UIKit.Style.Primary); mainButton.SetLabel(s.Players.Count == 1 ? "COMENÇAR (SOLO)" : "COMENÇAR PARTIDA"); mainButton.SetInteractable(true); }
            else { mainButton.SetStyle(UIKit.Style.Normal); mainButton.SetLabel("ESPERANT " + s.ReadyCount + "/" + s.Players.Count + " LLESTOS"); mainButton.SetInteractable(false); }
            hint.text = MobileInput.Active ? "Joystick per moure't · Toca COMENÇAR · Codi " + room.code : "WASD per moure't · Intro per xatejar · R per començar · Comparteix el codi " + room.code;
        }
        else
        {
            var me = s.Local;
            bool ready = me != null && me.ready;
            mainButton.SetStyle(ready ? UIKit.Style.Normal : UIKit.Style.Success);
            mainButton.SetLabel(room.phase == RoomPhase.Countdown ? "COMENÇANT..." : ready ? "JA NO ESTIC LLEST" : "ESTIC LLEST");
            mainButton.SetInteractable(true);
            hint.text = MobileInput.Active ? "Joystick per moure't · Toca ESTIC LLEST · Espera l'amfitrió" : "WASD per moure't · Intro per xatejar · R per estar llest · Espera que l'amfitrió comence";
        }
    }
}

// Xat reutilitzable (lobby i partida). Intro obri, Intro envia, Esc tanca.
public class ChatBox : MonoBehaviour
{
    TextMeshProUGUI log;
    TMP_InputField input;
    CanvasGroup group;
    bool fade;
    float lastActivity;
    int submitFrame = -1;
    NetSession s;

    public static ChatBox Create(Transform parent, Vector2 pos, Vector2 size, bool fadeWhenIdle)
    {
        var root = UIKit.Box(parent, pos, size, new Color(.02f, .045f, .065f, fadeWhenIdle ? .55f : .78f), "Xat");
        root.GetComponent<Image>().raycastTarget = false;
        var box = root.gameObject.AddComponent<ChatBox>();
        box.fade = fadeWhenIdle;
        box.group = root.gameObject.AddComponent<CanvasGroup>();
        box.log = UIKit.Text(root, "", Vector2.zero, Vector2.zero, 18, TextAlignmentOptions.BottomLeft);
        UIKit.Stretch(box.log.rectTransform, 14, 58, 14, 10);
        box.log.overflowMode = TextOverflowModes.Masking;
        box.input = UIKit.Input(root, Vector2.zero, new Vector2(size.x - 16, 42), MobileInput.Active?"Toca per xatejar":"Prem Intro per xatejar", 120, false, 19);
        var ir = (RectTransform)box.input.transform; ir.anchorMin = ir.anchorMax = new Vector2(.5f, 0); ir.anchoredPosition = new Vector2(0, 29);
        if(MobileInput.Active)
        {
            ir.anchorMin=ir.anchorMax=new Vector2(0,0);ir.pivot=new Vector2(0,.5f);ir.anchoredPosition=new Vector2(8,29);ir.sizeDelta=new Vector2(size.x-108,42);
            var send=UIKit.Button(root,"ENVIA",Vector2.zero,new Vector2(84,42),()=>box.Submit(box.input.text),UIKit.Style.Normal,17,false);
            send.rect.anchorMin=send.rect.anchorMax=new Vector2(1,0);send.rect.pivot=new Vector2(1,.5f);send.rect.anchoredPosition=new Vector2(-8,29);
        }
        box.input.onSubmit.AddListener(box.Submit);
        box.lastActivity = Time.unscaledTime;
        return box;
    }

    void Start()
    {
        s = NetSession.Instance;
        if (s != null) { s.ChatReceived += OnLine; Redraw(); }
    }

    void OnDestroy() { if (s != null) s.ChatReceived -= OnLine; }

    void OnLine(NetSession.ChatLine line) { lastActivity = Time.unscaledTime; Redraw(); }

    void Redraw()
    {
        if (log == null || s == null) return;
        var sb = new System.Text.StringBuilder();
        int start = Mathf.Max(0, s.Chat.Count - 9);
        for (int i = start; i < s.Chat.Count; i++)
        {
            var l = s.Chat[i];
            if (l.system) sb.Append("<color=#8AA4A8><i>").Append(NetSession.Safe(l.text)).Append("</i></color>");
            else
            {
                bool host = s.Players.TryGetValue(l.from, out var p) && p.isHost;
                sb.Append(host ? "<color=#EBB347>" : "<color=#9FD8E6>").Append(NetSession.Safe(l.name)).Append(":</color> ").Append(NetSession.Safe(l.text));
            }
            if (i < s.Chat.Count - 1) sb.Append('\n');
        }
        log.text = sb.ToString();
    }

    void Submit(string text)
    {
        if (s != null) s.SendChat(text);
        input.text = "";
        input.DeactivateInputField();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        TypingGuard.End(input);
        submitFrame = Time.frameCount;
        lastActivity = Time.unscaledTime;
    }

    void Update()
    {
        if (Time.frameCount != submitFrame && !TypingGuard.Typing && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !DisplaySettings.Pending)
        {
            input.Select(); input.ActivateInputField();
            lastActivity = Time.unscaledTime;
        }
        if (input.isFocused) lastActivity = Time.unscaledTime;
        if (fade)
        {
            float idle = Time.unscaledTime - lastActivity;
            group.alpha = Mathf.Lerp(group.alpha, idle > 8 ? 0.15f : 1f, 1 - Mathf.Exp(-6 * Time.unscaledDeltaTime));
        }
    }
}

// Durant la partida online: llista compacta de jugadors, xat i (amfitrio) tornar al lobby.
public class InGameNetUI : MonoBehaviour
{
    NetSession s;
    TextMeshProUGUI list;
    RectTransform panelRect, backButton;
    float next;

    public static void Create()
    {
        var go = UIKit.Canvas("Partida online", 85);
        go.AddComponent<InGameNetUI>();
    }

    void Start()
    {
        s = NetSession.Instance;
        if (s == null || !NetSession.Active) { Destroy(gameObject); return; }
        var panel = UIKit.Box(transform, Vector2.zero, new Vector2(260, 210), new Color(.02f, .045f, .065f, .6f), "Jugadors online");
        panel.anchorMin = panel.anchorMax = new Vector2(1, .5f); panel.pivot = new Vector2(1, 1); panel.anchoredPosition = new Vector2(-14, 70); panelRect = panel;
        panel.GetComponent<Image>().raycastTarget = false;
        list = UIKit.Text(panel, "", Vector2.zero, Vector2.zero, 17, TextAlignmentOptions.TopLeft);
        UIKit.Stretch(list.rectTransform, 14, 10, 14, 10);
        if (NetSession.IsHost)
        {
            var b = UIKit.Button(transform, "TORNAR TOTS AL LOBBY", Vector2.zero, new Vector2(260, 40), () => s.BackToLobby(), UIKit.Style.Normal, 16, false);
            b.rect.anchorMin = b.rect.anchorMax = new Vector2(1, .5f); b.rect.pivot = new Vector2(1, 1); backButton = b.rect;
        }
        var chat = ChatBox.Create(transform, Vector2.zero, new Vector2(440, 220), true);
        var cr = (RectTransform)chat.transform; cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0, 0); cr.anchoredPosition = new Vector2(14, MobileInput.Active?340:150);
        Refresh();
    }

    void Update()
    {
        if (Time.unscaledTime >= next) { next = Time.unscaledTime + 0.5f; Refresh(); }
    }

    void Refresh()
    {
        if (s == null || list == null || !NetSession.Active) return;
        int token = NetSession.SceneToken();
        var sb = new System.Text.StringBuilder();
        sb.Append("<color=#EBB347>SALA ").Append(s.Room.code).Append("</color>  <color=#8AA4A8>").Append(s.Players.Count).Append("/").Append(s.Room.maxPlayers).Append("</color>\n");
        foreach (var p in s.Players.Values)
        {
            bool here = p.id == s.LocalId || p.scene == token;
            sb.Append(here ? "" : "<color=#5C7478>").Append(NetSession.Safe(p.name));
            if (p.id == s.LocalId) sb.Append(" (tu)");
            if (!here) sb.Append(p.scene >= 0 && p.scene % 1000 > 0 ? " · pis " + (p.scene % 1000) : " · lluny");
            if (!here) sb.Append("</color>");
            sb.Append('\n');
        }
        list.text = sb.ToString();
        float h = 34 + 23 * (s.Players.Count + 1);
        panelRect.sizeDelta = new Vector2(260, h);
        if (backButton != null) backButton.anchoredPosition = new Vector2(-14, 70 - h - 10);
    }
}
