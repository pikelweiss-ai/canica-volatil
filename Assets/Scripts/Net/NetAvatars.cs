using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Dibuixa els altres jugadors de la sala a l'escena actual (lobby, mapa lliure o masmorra).
// Nomes es veuen si estan a la mateixa escena (i al mateix pis de la masmorra).
public class NetAvatars : MonoBehaviour
{
    readonly Dictionary<int, RemoteAvatar> avatars = new Dictionary<int, RemoteAvatar>();
    PlayerMovement local;
    bool subscribed;

    void Update()
    {
        var s = NetSession.Instance;
        if (s != null && !subscribed) { s.ChatReceived += OnChat; subscribed = true; }
        if (!NetSession.Active) { ClearAll(); return; }
        if (local == null) local = FindFirstObjectByType<PlayerMovement>();
        if (local == null) { ClearAll(); return; }

        int token = NetSession.SceneToken();
        double now = Time.realtimeSinceStartupAsDouble;
        var keep = new HashSet<int>();
        foreach (var pl in s.Players.Values)
        {
            if (pl.id == s.LocalId) continue;
            if (pl.scene != token || now - pl.lastPos > 3) continue;
            keep.Add(pl.id);
            if (!avatars.TryGetValue(pl.id, out var av) || av == null)
            {
                av = RemoteAvatar.Create(pl, local.gameObject);
                avatars[pl.id] = av;
            }
            av.Refresh(pl, s.Room.phase != RoomPhase.InGame);
        }
        var drop = new List<int>();
        foreach (var kv in avatars) if (kv.Value == null || !keep.Contains(kv.Key)) drop.Add(kv.Key);
        foreach (var id in drop) { if (avatars[id] != null) Destroy(avatars[id].gameObject); avatars.Remove(id); }
    }

    void OnChat(NetSession.ChatLine line)
    {
        if (line.system || !NetSession.Active) return;
        if (line.from == NetSession.Instance.LocalId)
        {
            if (local != null) ChatBubble.Show(local.gameObject, line.text);
        }
        else if (avatars.TryGetValue(line.from, out var av) && av != null) ChatBubble.Show(av.gameObject, line.text);
    }

    void ClearAll()
    {
        foreach (var av in avatars.Values) if (av != null) Destroy(av.gameObject);
        avatars.Clear();
    }
}

// Un company dibuixat al nostre PC: cos amb la seua skin, barret, estela, ombra, arma a la ma i nom.
public class RemoteAvatar : MonoBehaviour
{
    public int playerId;
    public bool Down { get; private set; }
    SpriteRenderer body, hand;
    SpriteRenderer shield;
    TMP_Text nameText;
    Vector3 target;
    float aim;
    string weaponName;
    bool shieldRaised;
    string lastName;
    bool lastReady, lastShowReady;

    public static RemoteAvatar Create(NetPlayer pl, GameObject reference)
    {
        var go = new GameObject("Jugador online " + pl.name);
        var av = go.AddComponent<RemoteAvatar>();
        av.playerId = pl.id;
        go.transform.localScale = reference.transform.lossyScale;
        go.transform.position = new Vector3(pl.x, pl.y, reference.transform.position.z);
        av.target = go.transform.position;
        av.body = go.AddComponent<SpriteRenderer>();
        var refBody = reference.GetComponent<SpriteRenderer>();
        if (refBody != null) { av.body.sortingLayerID = refBody.sortingLayerID; av.body.sortingOrder = refBody.sortingOrder; av.body.sharedMaterial = refBody.sharedMaterial; }
        av.body.sprite = UIKit.SkinSprite(pl.skin);

        // Skin, barret i estela del company (el mateix codi que per al jugador local)
        var cos = go.AddComponent<PlayerCosmetics>();
        cos.skinOverride = pl.skin; cos.hatOverride = pl.hat; cos.trailOverride = pl.trail;

        // Ombra: copia de la del jugador local
        foreach (Transform child in reference.transform)
        {
            if (!child.name.ToLowerInvariant().Contains("ombra") && !child.name.ToLowerInvariant().Contains("shadow")) continue;
            var shadow = Instantiate(child.gameObject, go.transform, false);
            shadow.name = child.name;
            shadow.transform.localPosition = child.localPosition; shadow.transform.localRotation = child.localRotation; shadow.transform.localScale = child.localScale;
            foreach (var mb in shadow.GetComponentsInChildren<MonoBehaviour>()) Destroy(mb);
            break;
        }

        // Arma a la ma (com PlayerAttack.UpdateHand)
        var handGo = new GameObject("Arma del company");
        av.hand = handGo.AddComponent<SpriteRenderer>();
        av.hand.enabled = false;
        handGo.transform.SetParent(go.transform, true);

        // Copiem l'etiqueta de nom del jugador local (mateixa font, mida i capa)
        var tag = reference.GetComponent<PlayerNameTag>();
        if (tag != null && tag.playerNameText != null)
        {
            var copy = Instantiate(tag.playerNameText.gameObject, go.transform, false);
            copy.transform.localPosition = tag.playerNameText.transform.localPosition;
            copy.transform.localRotation = tag.playerNameText.transform.localRotation;
            copy.transform.localScale = tag.playerNameText.transform.localScale;
            av.nameText = copy.GetComponent<TMP_Text>();
            foreach (var extra in copy.GetComponentsInChildren<MonoBehaviour>()) if (!(extra is TMP_Text) && !(extra is TMP_SubMesh)) Destroy(extra);
        }
        else
        {
            var t = new GameObject("Nom").AddComponent<TextMeshPro>();
            t.transform.SetParent(go.transform, false);
            t.transform.localPosition = new Vector3(0, 0.75f, 0);
            if (GameTypography.Font != null) t.font = GameTypography.Font;
            t.fontSize = 3; t.alignment = TextAlignmentOptions.Center; t.textWrappingMode = TextWrappingModes.NoWrap;
            t.GetComponent<MeshRenderer>().sortingOrder = 50;
            av.nameText = t;
        }
        av.Refresh(pl, true);
        return av;
    }

    public void Refresh(NetPlayer pl, bool showReady)
    {
        target = new Vector3(pl.x, pl.y, transform.position.z);
        aim = pl.aim;
        weaponName = pl.weapon;
        shieldRaised = pl.shieldRaised;
        Down = pl.down || Time.realtimeSinceStartupAsDouble - pl.lastPos > 0.6;
        if (nameText != null && (pl.name != lastName || pl.ready != lastReady || showReady != lastShowReady))
        {
            lastName = pl.name; lastReady = pl.ready; lastShowReady = showReady;
            // Una sola linia (no xoca amb el barret): or = amfitrio, verd = llest al lobby
            string n = NetSession.Safe(pl.name);
            if (pl.isHost) n = "<color=#EBB347>" + n + "</color>";
            else if (showReady && pl.ready) n = "<color=#5CC772>" + n + "</color>";
            nameText.text = n;
        }
    }

    void Update()
    {
        var p = transform.position;
        // Si ha saltat molt (portal, teletransport), apareix directament; si no, llisca suau
        if ((target - p).sqrMagnitude > 16f) transform.position = target;
        else transform.position = Vector3.Lerp(p, target, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
        UpdateHand();
    }

    void UpdateHand()
    {
        if (hand == null) return;
        var weapon = NetCoop.Weapon(weaponName);
        if (weapon == null || weapon.icon == null) { hand.enabled = false; if (shield != null) shield.enabled = false; return; }
        Vector2 dir = new Vector2(Mathf.Cos(aim * Mathf.Deg2Rad), Mathf.Sin(aim * Mathf.Deg2Rad));
        if (weapon.special == SpecialKind.Shield && shieldRaised)
        {
            hand.enabled = false;
            if (shield == null)
            {
                var go = new GameObject("Escut del company");
                shield = go.AddComponent<SpriteRenderer>(); go.transform.SetParent(transform, true);
                shield.transform.localScale = Vector3.one * 1.1f;
            }
            shield.enabled = body.enabled; shield.sprite = weapon.icon; shield.sortingLayerID = body.sortingLayerID;
            shield.sortingOrder = dir.y > 0.35f ? body.sortingOrder - 1 : body.sortingOrder + 1;
            shield.flipY = dir.x < 0f; shield.transform.position = transform.position + (Vector3)(dir * 0.85f);
            shield.transform.rotation = Quaternion.Euler(0f, 0f, aim);
            return;
        }
        if (shield != null) shield.enabled = false;
        hand.enabled = body.enabled;
        hand.sprite = weapon.icon;
        hand.flipY = dir.x < 0f;
        hand.sortingLayerID = body.sortingLayerID;
        hand.sortingOrder = dir.y > 0.35f ? body.sortingOrder - 1 : body.sortingOrder + 1;
        hand.transform.position = transform.position + (Vector3)(dir * 0.45f + Vector2.down * 0.08f);
        hand.transform.rotation = Quaternion.Euler(0f, 0f, aim);
        float parent = Mathf.Max(0.01f, transform.lossyScale.x);
        hand.transform.localScale = Vector3.one * (weapon.handScale / parent);
    }
}


// Bafarada de xat damunt d'un jugador (local o remot), com als lobbies de jocs de navegador.
public class ChatBubble : MonoBehaviour
{
    TextMeshPro text;
    float until;

    public static void Show(GameObject owner, string message)
    {
        if (owner == null) return;
        var b = owner.GetComponentInChildren<ChatBubble>();
        if (b == null)
        {
            var go = new GameObject("Bafarada");
            go.transform.SetParent(owner.transform, false);
            float scale = Mathf.Max(0.01f, owner.transform.lossyScale.y);
            go.transform.localScale = Vector3.one / scale;
            go.transform.localPosition = new Vector3(0, 1.45f / scale, 0);
            b = go.AddComponent<ChatBubble>();
            b.text = go.AddComponent<TextMeshPro>();
            if (GameTypography.Font != null) b.text.font = GameTypography.Font;
            b.text.fontSize = 3.2f; b.text.alignment = TextAlignmentOptions.Bottom;
            b.text.textWrappingMode = TextWrappingModes.Normal;
            b.text.rectTransform.sizeDelta = new Vector2(7f, 2f);
            b.text.rectTransform.pivot = new Vector2(.5f, 0);
            var mr = go.GetComponent<MeshRenderer>(); mr.sortingOrder = 400;
            mr.sortingLayerName = "PlayerName";
        }
        if (message.Length > 70) message = message.Substring(0, 67) + "...";
        b.text.text = "<mark=#0B1620E6 padding=\"18,18,8,8\">" + NetSession.Safe(message) + "</mark>";
        b.text.alpha = 1;
        b.until = Time.unscaledTime + 4.5f;
    }

    void Update()
    {
        float left = until - Time.unscaledTime;
        if (left <= 0) { Destroy(gameObject); return; }
        text.alpha = Mathf.Clamp01(left / 0.5f);
    }
}
