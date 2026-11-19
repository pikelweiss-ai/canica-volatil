using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Navegador de sales online, inspirat en les llistes de sales de jocs de navegador (HaxBall, skribbl, krunker):
//  - Pestanyes INTERNET (sales publiques del servidor) i XARXA LOCAL (descobriment LAN).
//  - Taula amb nom, amfitrio, mode, jugadors, estat, ping i cadenat si te contrasenya.
//  - Cerca i filtres, doble clic per entrar, unir-se amb codi (o IP:port), crear sala.
public class RoomBrowserUI : MonoBehaviour
{
    enum Tab { Internet, Lan }
    static RoomBrowserUI open;

    Tab tab = Tab.Lan;
    UIKit.UIButton tabInternet, tabLan, hideFull, hideLocked, joinSelected;
    TMP_InputField search, codeInput, nameInput, serverInput;
    TextMeshProUGUI status, detailTitle, detailBody, footerHint;
    RectTransform listContent, modalRoot, serverRow;
    ScrollRect scroll;
    readonly List<RoomInfo> lanRooms = new List<RoomInfo>(), netRooms = new List<RoomInfo>();
    string netError;
    bool loadingLan, loadingNet, filterFull, filterLocked;
    float nextLan, nextNet, lastLanTime = -1, lastNetTime = -1;
    RoomInfo selected;
    float lastClickTime; string lastClickCode;
    bool busy;

    public static void Open()
    {
        if (open != null) { open.gameObject.SetActive(true); return; }
        var go = UIKit.Canvas("Sales online", 60);
        open = go.AddComponent<RoomBrowserUI>();
        open.Build();
    }

    public static bool IsOpen => open != null && open.gameObject.activeSelf;

    // Nomes per al bot de captures (NetBot -botShots)
    public static void DebugShow(string what, RoomInfo room)
    {
        if (open == null) return;
        switch (what)
        {
            case "select": { var v = open.Visible(); if (v.Count > 0) { open.selected = v[0]; open.Rebuild(); } break; }
            case "password": open.AskPassword(room); break;
            case "create": open.OpenCreate(); break;
            case "close": open.CloseModal(); Destroy(open.gameObject); open = null; break;
        }
    }

    void Close()
    {
        NetSession.CancelJoin();
        Destroy(gameObject);
    }

    void OnDestroy() { if (open == this) open = null; }

    // =====================================================================
    void Build()
    {
        var root = transform;
        var bg = UIKit.Box(root, Vector2.zero, Vector2.zero, new Color(.025f, .045f, .07f, 1f), "Fons"); UIKit.Stretch(bg);
        // Bandes decoratives
        UIKit.Box(root, new Vector2(0, 345), new Vector2(1600, 2), new Color(.14f, .26f, .3f));
        UIKit.Box(root, new Vector2(580, -40), new Vector2(430, 760), new Color(.045f, .09f, .12f), "Columna dreta");

        UIKit.Button(root, "< TORNAR", new Vector2(-695, 397), new Vector2(170, 50), Close, UIKit.Style.Ghost, 22);
        UIKit.Text(root, "SALES ONLINE", new Vector2(-330, 403), new Vector2(520, 60), 44, TextAlignmentOptions.MidlineLeft, UIKit.Gold);
        UIKit.Text(root, "Crea una sala o tria'n una: per Internet, sense IPs ni ports", new Vector2(-280, 362), new Vector2(620, 30), 19, TextAlignmentOptions.MidlineLeft, UIKit.Muted);

        // Nom del jugador i la seua canica
        UIKit.Text(root, "EL TEU NOM", new Vector2(445, 420), new Vector2(300, 26), 16, TextAlignmentOptions.MidlineLeft, UIKit.Muted);
        nameInput = UIKit.Input(root, new Vector2(445, 378), new Vector2(300, 46), "Nom del jugador", 16, false, 22);
        nameInput.text = NetSession.PlayerName;
        nameInput.onEndEdit.AddListener(v => { NetSession.PlayerName = v; nameInput.text = NetSession.PlayerName; });
        var db = CosmeticsDatabase.Instance;
        int skin = db != null && db.skins != null ? CosmeticsDatabase.Get(CosmeticsDatabase.KeySkin, db.skins.Length) : 0;
        UIKit.Box(root, new Vector2(660, 385), new Vector2(76, 76), UIKit.PanelLight);
        UIKit.Picture(root, UIKit.SkinSprite(skin), new Vector2(660, 385), new Vector2(64, 64));

        // Pestanyes
        tabInternet = UIKit.Button(root, "INTERNET · PÚBLIQUES", new Vector2(-628, 298), new Vector2(300, 56), () => SetTab(Tab.Internet), UIKit.Style.Normal, 21);
        tabLan = UIKit.Button(root, "XARXA LOCAL · LAN", new Vector2(-318, 298), new Vector2(300, 56), () => SetTab(Tab.Lan), UIKit.Style.Normal, 21);
        search = UIKit.Input(root, new Vector2(12, 298), new Vector2(330, 50), "Cerca sala o amfitrió...", 24, false, 21);
        search.onValueChanged.AddListener(_ => Rebuild());
        UIKit.Button(root, "ACTUALITZA", new Vector2(265, 298), new Vector2(150, 50), Refresh, UIKit.Style.Normal, 19);

        hideFull = UIKit.Button(root, "NOMÉS AMB LLOC", new Vector2(-680, 240), new Vector2(200, 40), () => { filterFull = !filterFull; StyleFilters(); Rebuild(); }, UIKit.Style.Ghost, 16);
        hideLocked = UIKit.Button(root, "NOMÉS SENSE CONTRASENYA", new Vector2(-420, 240), new Vector2(300, 40), () => { filterLocked = !filterLocked; StyleFilters(); Rebuild(); }, UIKit.Style.Ghost, 16);
        status = UIKit.Text(root, "", new Vector2(110, 240), new Vector2(430, 36), 17, TextAlignmentOptions.MidlineRight, UIKit.Muted);

        // Capçalera de la taula
        var head = UIKit.Box(root, new Vector2(-225, 195), new Vector2(1110, 40), new Color(.09f, .17f, .21f), "Capçalera");
        string[] cols = { "NOM DE LA SALA", "AMFITRIÓ", "MODE", "JUGADORS", "ESTAT", "PING" };
        float[] xs = { 22, 386, 590, 762, 885, 1000 };
        float[] ws = { 360, 200, 170, 120, 115, 80 };
        for (int i = 0; i < cols.Length; i++) UIKit.Cell(head, cols[i], xs[i], ws[i], 16, TextAlignmentOptions.MidlineLeft, UIKit.Muted);

        listContent = UIKit.ScrollList(root, new Vector2(-225, -70), new Vector2(1110, 485), out scroll, 3);

        // Peu: ajuda o servidor de sales
        footerHint = UIKit.Text(root, "", new Vector2(-225, -345), new Vector2(1110, 40), 18, TextAlignmentOptions.MidlineLeft, UIKit.Dim);
        serverRow = UIKit.Empty(root, "Servidor");
        UIKit.Text(serverRow, "SERVIDOR PROPI (opcional)", new Vector2(-660, -395), new Vector2(240, 40), 15, TextAlignmentOptions.MidlineLeft, UIKit.Dim);
        serverInput = UIKit.Input(serverRow, new Vector2(-355, -395), new Vector2(380, 46), "buit = automàtic (recomanat)", 64, false, 20);
        serverInput.text = NetSession.ServerAddress;
        UIKit.Button(serverRow, "DESA", new Vector2(-90, -395), new Vector2(120, 46), SaveServer, UIKit.Style.Normal, 18);
        UIKit.Text(serverRow, "Deixa-ho buit i el joc troba les sales sol. Només per a servidors propis (CanicaVolatil -batchmode -nographics -canicaServer 7780).", new Vector2(250, -395), new Vector2(560, 44), 14, TextAlignmentOptions.MidlineLeft, UIKit.Dim);

        // Columna dreta: codi, crear, detall
        UIKit.Text(root, "UNIR-SE AMB CODI", new Vector2(580, 290), new Vector2(380, 34), 22, TextAlignmentOptions.MidlineLeft, UIKit.TextColor);
        codeInput = UIKit.Input(root, new Vector2(580, 238), new Vector2(380, 62), "K7QX2M", 32, false, 32);
        codeInput.onSubmit.AddListener(_ => JoinCode());
        codeInput.onValueChanged.AddListener(v => { var up = v.ToUpperInvariant(); if (up != v) codeInput.SetTextWithoutNotify(up); });
        UIKit.Button(root, "ENTRAR", new Vector2(580, 172), new Vector2(380, 54), JoinCode, UIKit.Style.Normal, 24);
        UIKit.Text(root, "Codi de 6 lletres · Internet o xarxa local", new Vector2(580, 132), new Vector2(380, 26), 15, TextAlignmentOptions.MidlineLeft, UIKit.Dim);

        UIKit.Button(root, "CREAR SALA", new Vector2(580, 62), new Vector2(380, 78), OpenCreate, UIKit.Style.Primary, 30);

        UIKit.Box(root, new Vector2(580, -165), new Vector2(380, 300), UIKit.Panel, "Detall");
        detailTitle = UIKit.Text(root, "", new Vector2(580, -45), new Vector2(350, 44), 25, TextAlignmentOptions.MidlineLeft, UIKit.Gold);
        detailBody = UIKit.Text(root, "", new Vector2(580, -150), new Vector2(350, 160), 18, TextAlignmentOptions.TopLeft, UIKit.TextColor);
        detailBody.lineSpacing = 12;
        joinSelected = UIKit.Button(root, "ENTRAR A LA SALA", new Vector2(580, -270), new Vector2(350, 58), () => { if (selected != null) JoinRoom(selected, null); }, UIKit.Style.Success, 23);

        modalRoot = UIKit.Empty(root, "Finestres");

        SetTab(Tab.Internet); // Internet funciona sense configurar res
        StyleFilters();
        ShowDetail();
    }

    void StyleFilters()
    {
        hideFull.SetStyle(filterFull ? UIKit.Style.Success : UIKit.Style.Ghost);
        hideFull.SetLabel(filterFull ? "NOMÉS AMB LLOC" : "NOMÉS AMB LLOC");
        hideLocked.SetStyle(filterLocked ? UIKit.Style.Success : UIKit.Style.Ghost);
        hideLocked.SetLabel(filterLocked ? "NOMÉS SENSE CONTRASENYA" : "NOMÉS SENSE CONTRASENYA");
    }

    void SetTab(Tab t)
    {
        tab = t;
        tabInternet.SetStyle(t == Tab.Internet ? UIKit.Style.Primary : UIKit.Style.Normal);
        tabLan.SetStyle(t == Tab.Lan ? UIKit.Style.Primary : UIKit.Style.Normal);
        serverRow.gameObject.SetActive(t == Tab.Internet);
        selected = null;
        Refresh();
        Rebuild();
        ShowDetail();
    }

    void SaveServer()
    {
        NetSession.ServerAddress = serverInput.text;
        serverInput.text = NetSession.ServerAddress;
        NetToast.Show(string.IsNullOrEmpty(NetSession.ServerAddress) ? "Servidor de sales esborrat." : "Servidor de sales desat: " + NetSession.ServerAddress);
        netRooms.Clear(); netError = null;
        Refresh();
    }

    void Refresh()
    {
        if (tab == Tab.Lan && !loadingLan)
        {
            loadingLan = true; nextLan = Time.unscaledTime + 3f;
            NetSession.ListLan(rooms => { if (this == null) return; loadingLan = false; lanRooms.Clear(); lanRooms.AddRange(rooms); lastLanTime = Time.unscaledTime; Rebuild(); });
        }
        if (tab == Tab.Internet && !loadingNet)
        {
            loadingNet = true; nextNet = Time.unscaledTime + 8f;
            NetSession.ListInternet((rooms, error) => { if (this == null) return; loadingNet = false; netRooms.Clear(); netRooms.AddRange(rooms); netError = error; lastNetTime = Time.unscaledTime; Rebuild(); });
        }
        UpdateStatus();
    }

    void Update()
    {
        if (busy) return;
        if (tab == Tab.Lan && Time.unscaledTime >= nextLan) Refresh();
        if (tab == Tab.Internet && Time.unscaledTime >= nextNet) Refresh();
        UpdateStatus();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (modalRoot.childCount > 0) CloseModal();
            else if (!TypingGuard.Typing) Close();
        }
    }

    void UpdateStatus()
    {
        if (status == null) return;
        bool loading = tab == Tab.Lan ? loadingLan : loadingNet;
        float last = tab == Tab.Lan ? lastLanTime : lastNetTime;
        int count = Visible().Count;
        string age = last < 0 ? "" : " · actualitzat fa " + Mathf.Max(0, Mathf.FloorToInt(Time.unscaledTime - last)) + " s";
        status.text = (loading && last < 0 ? "Buscant sales..." : count + (count == 1 ? " sala" : " sales") + age);
    }

    List<RoomInfo> Visible()
    {
        var src = tab == Tab.Lan ? lanRooms : netRooms;
        string q = search != null ? search.text.Trim().ToLowerInvariant() : "";
        var list = new List<RoomInfo>();
        foreach (var r in src)
        {
            if (filterFull && r.Full) continue;
            if (filterLocked && r.hasPassword) continue;
            if (q.Length > 0 && !r.name.ToLowerInvariant().Contains(q) && !r.hostName.ToLowerInvariant().Contains(q) && !r.code.ToLowerInvariant().Contains(q)) continue;
            list.Add(r);
        }
        // Primer les que estan al lobby i no estan plenes; despres per jugadors
        list.Sort((a, b) =>
        {
            int sa = (a.phase == RoomPhase.Lobby && !a.Full) ? 0 : 1, sb = (b.phase == RoomPhase.Lobby && !b.Full) ? 0 : 1;
            if (sa != sb) return sa.CompareTo(sb);
            if (a.players != b.players) return b.players.CompareTo(a.players);
            return a.pingMs.CompareTo(b.pingMs);
        });
        return list;
    }

    void Rebuild()
    {
        if (listContent == null) return;
        UIKit.Clear(listContent);
        var rooms = Visible();
        footerHint.text = tab == Tab.Lan
            ? "Sales de la teua xarxa (wifi o cable). Les privades no surten: entra amb el seu codi."
            : NetCloud.InUse ? "Sales públiques de tot arreu. Crea'n una i els teus amics la veuran ací, sense IPs ni ports. Les privades: amb el codi." : "Sales públiques del servidor " + NetSession.ServerAddress + ". Les privades només s'hi entra amb codi.";

        if (rooms.Count == 0)
        {
            var row = UIKit.ListRow(listContent, 200, new Color(0, 0, 0, 0));
            string msg;
            if (tab == Tab.Internet && netError != null)
                msg = "<color=#D9544D>" + NetSession.Safe(netError) + "</color>\n" + (NetCloud.InUse ? "Comprova la connexió a Internet. Mentrestant pots jugar en XARXA LOCAL." : "Comprova l'adreça del servidor i que estiga arrancat.");
            else if ((tab == Tab.Lan ? loadingLan && lastLanTime < 0 : loadingNet && lastNetTime < 0)) msg = "Buscant sales...";
            else if ((tab == Tab.Lan ? lanRooms.Count : netRooms.Count) > 0) msg = "Cap sala coincideix amb la cerca o els filtres.";
            else msg = tab == Tab.Lan ? "No hi ha cap sala a la teua xarxa local.\nCrea'n una amb <color=#EBB347>CREAR SALA</color> o demana el codi a qui la tinga." : "No hi ha cap sala pública ara mateix.\nSigues el primer: <color=#EBB347>CREAR SALA</color> i els teus amics la veuran ací.\n<size=80%><color=#5C7478>La llista s'actualitza sola cada pocs segons.</color></size>";
            var t = UIKit.Text(row, msg, Vector2.zero, Vector2.zero, 21, TextAlignmentOptions.Center, UIKit.Muted);
            UIKit.Stretch(t.rectTransform, 20, 10, 20, 10);
            ShowDetail();
            return;
        }

        if (selected != null && rooms.Find(r => r.code == selected.code) == null) selected = null;
        for (int i = 0; i < rooms.Count; i++)
        {
            var info = rooms[i];
            bool isSel = selected != null && selected.code == info.code;
            var row = UIKit.ListRow(listContent, 52, isSel ? UIKit.RowSelected : (i % 2 == 0 ? UIKit.Row : UIKit.RowAlt));
            var b = row.gameObject.AddComponent<Button>();
            var c = b.colors; c.highlightedColor = new Color(1.25f, 1.25f, 1.15f); c.pressedColor = new Color(.8f, .9f, .9f); c.selectedColor = Color.white; b.colors = c;
            var captured = info;
            b.onClick.AddListener(() => ClickRoom(captured));
            bool joinable = info.phase == RoomPhase.Lobby && !info.Full && info.version == NetProtocol.Version;
            Color main = joinable ? UIKit.TextColor : UIKit.Dim;
            UIKit.Cell(row, NetSession.Safe(info.name), 22, 350, 21, TextAlignmentOptions.MidlineLeft, main).fontStyle = FontStyles.Bold;
            UIKit.Cell(row, NetSession.Safe(info.hostName), 386, 195, 19, TextAlignmentOptions.MidlineLeft, joinable ? UIKit.Muted : UIKit.Dim);
            UIKit.Cell(row, RoomModes.Short(info.mode), 590, 165, 19, TextAlignmentOptions.MidlineLeft, info.mode == RoomMode.Dungeon ? new Color(.95f, .62f, .45f) : new Color(.55f, .85f, .6f));
            UIKit.Cell(row, info.players + " / " + info.maxPlayers, 762, 115, 20, TextAlignmentOptions.MidlineLeft, info.Full ? UIKit.Red : main);
            string phase = info.version != NetProtocol.Version ? "<color=#D9544D>ALTRA VERSIÓ</color>" : info.phase == RoomPhase.Lobby ? (info.Full ? "<color=#D9544D>PLENA</color>" : "<color=#5CC772>AL LOBBY</color>") : "<color=#EBB347>EN PARTIDA</color>";
            UIKit.Cell(row, phase, 885, 115, 16, TextAlignmentOptions.MidlineLeft);
            UIKit.Cell(row, info.pingMs >= 0 ? info.pingMs + " ms" : "-", 1000, 80, 18, TextAlignmentOptions.MidlineLeft, PingColor(info.pingMs));
            if (info.hasPassword) { var holder = UIKit.Empty(row, "Cadenat"); holder.anchorMin = holder.anchorMax = new Vector2(1, .5f); holder.sizeDelta = Vector2.zero; holder.anchoredPosition = new Vector2(-24, -2); UIKit.Lock(holder, Vector2.zero, 1f, UIKit.Gold); }
        }
        ShowDetail();
    }

    static Color PingColor(int ms) => ms < 0 ? UIKit.Dim : ms < 60 ? UIKit.Green : ms < 140 ? UIKit.Gold : UIKit.Red;

    void ClickRoom(RoomInfo info)
    {
        bool dbl = lastClickCode == info.code && Time.unscaledTime - lastClickTime < 0.35f;
        lastClickCode = info.code; lastClickTime = Time.unscaledTime;
        selected = info;
        Rebuild();
        if (dbl) JoinRoom(info, null);
    }

    void ShowDetail()
    {
        if (detailTitle == null) return;
        if (selected == null)
        {
            detailTitle.text = "Tria una sala";
            detailBody.text = "<color=#8AA4A8>Clica una sala de la llista per veure'n els detalls.\nDoble clic per entrar directament.</color>";
            joinSelected.SetInteractable(false);
            return;
        }
        var s = selected;
        detailTitle.text = NetSession.Safe(s.name);
        detailBody.text =
            "<color=#8AA4A8>Amfitrió</color>  " + NetSession.Safe(s.hostName) + "\n" +
            "<color=#8AA4A8>Mode</color>  " + RoomModes.Short(s.mode) + "\n" +
            "<color=#8AA4A8>Jugadors</color>  " + s.players + " de " + s.maxPlayers + "\n" +
            "<color=#8AA4A8>Accés</color>  " + (s.hasPassword ? "<color=#EBB347>amb contrasenya</color>" : "obert") + "\n" +
            "<color=#8AA4A8>Xarxa</color>  " + (s.viaCloud ? "Internet · sense IP" : s.viaRelay ? "Internet · servidor" : "LAN · " + s.address);
        bool ok = s.phase == RoomPhase.Lobby && !s.Full && s.version == NetProtocol.Version;
        joinSelected.SetInteractable(ok);
        joinSelected.SetLabel(ok ? "ENTRAR A LA SALA" : s.Full ? "SALA PLENA" : s.version != NetProtocol.Version ? "ALTRA VERSIÓ" : "JA HAN COMENÇAT");
    }

    // =====================================================================
    //  ENTRAR
    // =====================================================================
    void JoinCode()
    {
        string code = codeInput.text.Trim();
        if (code.Length == 0) { NetToast.Show("Escriu el codi de la sala (o una IP)."); return; }
        CommitName();
        ShowBusy("Buscant la sala " + code.ToUpperInvariant() + "...");
        NetSession.JoinByCode(code, "", r => OnJoinResult(r, null, code));
    }

    void JoinRoom(RoomInfo info, string password)
    {
        CommitName();
        if (info.hasPassword && password == null) { AskPassword(info); return; }
        ShowBusy("Entrant a " + info.name + "...");
        NetSession.Join(info, password ?? "", r => OnJoinResult(r, info, null));
    }

    void CommitName()
    {
        if (nameInput != null) { NetSession.PlayerName = nameInput.text; nameInput.text = NetSession.PlayerName; }
    }

    void OnJoinResult(NetSession.JoinResult r, RoomInfo info, string typedCode)
    {
        if (this == null) return;
        CloseModal();
        if (r.ok) { Destroy(gameObject); return; } // el lobby es carrega sol
        if (r.needPassword)
        {
            // Pot ser que hagem entrat per codi o IP: el resultat porta on hem connectat i el codi real
            AskPassword(r.target ?? info, r.error);
            return;
        }
        ShowMessage("NO S'HA POGUT ENTRAR", r.error ?? "Error desconegut.");
    }

    // Demana la contrasenya i torna a entrar al mateix lloc.
    void AskPassword(RoomInfo info, string error = null)
    {
        var panel = Modal(640, 360);
        UIKit.Text(panel, "SALA AMB CONTRASENYA", new Vector2(0, 125), new Vector2(560, 50), 30, TextAlignmentOptions.Center, UIKit.Gold);
        UIKit.Lock(panel, new Vector2(0, 80), 1.6f, UIKit.Gold);
        var field = UIKit.Input(panel, new Vector2(0, 10), new Vector2(480, 58), "Contrasenya", 32, true, 26);
        var err = UIKit.Text(panel, error != null && error != "Aquesta sala té contrasenya." ? NetSession.Safe(error) : "Demana-la a l'amfitrió de la sala.", new Vector2(0, -50), new Vector2(560, 34), 18, TextAlignmentOptions.Center, error != null && error != "Aquesta sala té contrasenya." ? UIKit.Red : UIKit.Muted);
        UnityEngine.Events.UnityAction go = () =>
        {
            string pass = field.text;
            if (pass.Length == 0) { err.text = "Escriu la contrasenya."; err.color = UIKit.Red; return; }
            CloseModal();
            ShowBusy("Comprovant la contrasenya...");
            NetSession.Join(info, pass, r => OnJoinResult(r, info, null));
        };
        field.onSubmit.AddListener(_ => go());
        UIKit.Button(panel, "CANCEL·LAR", new Vector2(-125, -125), new Vector2(220, 58), CloseModal, UIKit.Style.Ghost, 22);
        UIKit.Button(panel, "ENTRAR", new Vector2(125, -125), new Vector2(220, 58), go, UIKit.Style.Primary, 24);
        field.Select(); field.ActivateInputField();
    }

    // =====================================================================
    //  CREAR SALA
    // =====================================================================
    void OpenCreate()
    {
        CommitName();
        var s = new NetSession.HostSettings { name = "Sala de " + NetSession.PlayerName, internet = tab == Tab.Internet };
        var panel = Modal(820, 770);
        UIKit.Text(panel, "CREAR SALA", new Vector2(0, 335), new Vector2(700, 56), 38, TextAlignmentOptions.Center, UIKit.Gold);

        float labelX = -250, y = 255;
        TextMeshProUGUI Label(string t, float yy) => UIKit.Text(panel, t, new Vector2(labelX, yy), new Vector2(240, 40), 19, TextAlignmentOptions.MidlineLeft, UIKit.Muted);

        Label("NOM DE LA SALA", y);
        var nameField = UIKit.Input(panel, new Vector2(110, y), new Vector2(480, 52), "Nom de la sala", 24, false, 23);
        nameField.text = NetSession.CleanName(s.name, 24);

        y -= 80; Label("MODE DE JOC", y);
        UIKit.UIButton modeA = null, modeB = null; TextMeshProUGUI modeHelp = null;
        System.Action paintMode = () =>
        {
            modeA.SetStyle(s.mode == RoomMode.Dungeon ? UIKit.Style.Primary : UIKit.Style.Normal);
            modeB.SetStyle(s.mode == RoomMode.FreeMap ? UIKit.Style.Primary : UIKit.Style.Normal);
            modeHelp.text = s.mode == RoomMode.Dungeon ? "Masmorra procedural: la mateixa per a tots (mateixa llavor)." : "Mapa obert per a fer el cabra, provar armes i quedar.";
        };
        modeA = UIKit.Button(panel, "MASMORRA", new Vector2(-10, y), new Vector2(235, 52), () => { s.mode = RoomMode.Dungeon; paintMode(); }, UIKit.Style.Normal, 21);
        modeB = UIKit.Button(panel, "MAPA LLIURE", new Vector2(235, y), new Vector2(235, 52), () => { s.mode = RoomMode.FreeMap; paintMode(); }, UIKit.Style.Normal, 21);
        modeHelp = UIKit.Text(panel, "", new Vector2(110, y - 42), new Vector2(480, 28), 15, TextAlignmentOptions.MidlineLeft, UIKit.Dim);

        y -= 110; Label("JUGADORS MÀXIMS", y);
        TextMeshProUGUI countText = null;
        UIKit.Button(panel, "-", new Vector2(-90, y), new Vector2(60, 52), () => { s.maxPlayers = Mathf.Max(2, s.maxPlayers - 1); countText.text = s.maxPlayers.ToString(); }, UIKit.Style.Normal, 30);
        UIKit.Box(panel, new Vector2(-5, y), new Vector2(90, 52), new Color(.025f, .05f, .075f));
        countText = UIKit.Text(panel, s.maxPlayers.ToString(), new Vector2(-5, y), new Vector2(90, 52), 30, TextAlignmentOptions.Center, UIKit.TextColor);
        UIKit.Button(panel, "+", new Vector2(80, y), new Vector2(60, 52), () => { s.maxPlayers = Mathf.Min(NetProtocol.MaxPlayers, s.maxPlayers + 1); countText.text = s.maxPlayers.ToString(); }, UIKit.Style.Normal, 30);
        UIKit.Text(panel, "de 2 a " + NetProtocol.MaxPlayers, new Vector2(250, y), new Vector2(200, 40), 16, TextAlignmentOptions.MidlineLeft, UIKit.Dim);

        y -= 80; Label("VISIBILITAT", y);
        UIKit.UIButton visA = null, visB = null; TextMeshProUGUI visHelp = null;
        System.Action paintVis = () =>
        {
            visA.SetStyle(s.isPublic ? UIKit.Style.Primary : UIKit.Style.Normal);
            visB.SetStyle(!s.isPublic ? UIKit.Style.Primary : UIKit.Style.Normal);
            visHelp.text = s.isPublic ? "Surt a la llista de sales." : "No surt a la llista: només s'hi entra amb el codi.";
        };
        visA = UIKit.Button(panel, "PÚBLICA", new Vector2(-10, y), new Vector2(235, 52), () => { s.isPublic = true; paintVis(); }, UIKit.Style.Normal, 21);
        visB = UIKit.Button(panel, "PRIVADA", new Vector2(235, y), new Vector2(235, 52), () => { s.isPublic = false; paintVis(); }, UIKit.Style.Normal, 21);
        visHelp = UIKit.Text(panel, "", new Vector2(110, y - 42), new Vector2(480, 28), 15, TextAlignmentOptions.MidlineLeft, UIKit.Dim);

        y -= 105; Label("CONTRASENYA", y);
        var passField = UIKit.Input(panel, new Vector2(110, y), new Vector2(480, 52), "Opcional · sense contrasenya", 32, true, 23);

        y -= 80; Label("XARXA", y);
        UIKit.UIButton netA = null, netB = null; TextMeshProUGUI netHelp = null;
        bool serverOk = true; // sense servidor propi, Internet va pel túnel automàtic
        System.Action paintNet = () =>
        {
            netA.SetStyle(!s.internet ? UIKit.Style.Primary : UIKit.Style.Normal);
            netB.SetStyle(s.internet ? UIKit.Style.Primary : UIKit.Style.Normal);
            netHelp.text = s.internet ? (NetCloud.InUse ? "Per a tot el món: sense IPs, sense obrir ports, sense VPN." : "Via servidor " + NetSession.ServerAddress + ": no cal obrir ports.") : "Només la teua wifi o cable (casa, classe). Més ràpid.";
        };
        netA = UIKit.Button(panel, "XARXA LOCAL", new Vector2(-10, y), new Vector2(235, 52), () => { s.internet = false; paintNet(); }, UIKit.Style.Normal, 21);
        netB = UIKit.Button(panel, "INTERNET", new Vector2(235, y), new Vector2(235, 52), () =>
        {
            if (!serverOk) { NetToast.Show("Primer configura el servidor de sales a la pestanya INTERNET."); return; }
            s.internet = true; paintNet();
        }, UIKit.Style.Normal, 21);
        netHelp = UIKit.Text(panel, "", new Vector2(110, y - 42), new Vector2(480, 28), 15, TextAlignmentOptions.MidlineLeft, UIKit.Dim);

        paintMode(); paintVis(); paintNet();

        UIKit.Button(panel, "CANCEL·LAR", new Vector2(-160, -335), new Vector2(260, 64), CloseModal, UIKit.Style.Ghost, 23);
        UIKit.Button(panel, "CREAR SALA", new Vector2(160, -335), new Vector2(260, 64), () =>
        {
            s.name = nameField.text; s.password = passField.text;
            CloseModal();
            ShowBusy("Creant la sala...");
            NetSession.HostRoom(s, error =>
            {
                if (this == null) return;
                CloseModal();
                if (error == null) { Destroy(gameObject); return; }
                ShowMessage("NO S'HA POGUT CREAR", error);
            });
        }, UIKit.Style.Primary, 25);
    }

    // =====================================================================
    //  FINESTRES
    // =====================================================================
    RectTransform Modal(float w, float h)
    {
        CloseModal();
        var dim = UIKit.Box(modalRoot, Vector2.zero, Vector2.zero, new Color(0, .01f, .02f, .82f), "Enfosquir"); UIKit.Stretch(dim);
        UIKit.Box(dim, new Vector2(0, -6), new Vector2(w, h), UIKit.Shadow);
        var panel = UIKit.Box(dim, Vector2.zero, new Vector2(w, h), new Color(.06f, .115f, .15f), "Finestra");
        UIKit.Box(panel, new Vector2(0, h / 2 - 3), new Vector2(w, 6), UIKit.Gold);
        return panel;
    }

    void CloseModal()
    {
        busy = false;
        if (modalRoot != null) UIKit.Clear(modalRoot);
    }

    void ShowBusy(string text)
    {
        var panel = Modal(620, 250);
        busy = true;
        UIKit.Text(panel, text, new Vector2(0, 50), new Vector2(560, 60), 26, TextAlignmentOptions.Center);
        var spinner = UIKit.Box(panel, new Vector2(0, -10), new Vector2(22, 22), UIKit.Gold, "Carrega");
        spinner.gameObject.AddComponent<Spinner>();
        UIKit.Button(panel, "CANCEL·LAR", new Vector2(0, -80), new Vector2(240, 56), () => { NetSession.CancelJoin(); CloseModal(); }, UIKit.Style.Ghost, 22);
    }

    void ShowMessage(string title, string text)
    {
        var panel = Modal(680, 300);
        UIKit.Text(panel, title, new Vector2(0, 95), new Vector2(600, 50), 30, TextAlignmentOptions.Center, UIKit.Red);
        UIKit.Text(panel, NetSession.Safe(text), new Vector2(0, 10), new Vector2(600, 100), 21, TextAlignmentOptions.Center);
        UIKit.Button(panel, "D'ACORD", new Vector2(0, -100), new Vector2(240, 58), CloseModal, UIKit.Style.Primary, 24);
    }
}

// Quadradet que gira mentre connecta
public class Spinner : MonoBehaviour
{
    void Update() { transform.Rotate(0, 0, -360 * Time.unscaledDeltaTime); }
}
