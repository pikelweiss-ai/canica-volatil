using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

// Atac estil Brawl Stars:
// - Apuntes amb el ratolí (es veu una línia cap on apuntes)
// - Clic esquerre (o Espai) per a atacar
// - Tecles 1-4 o la roda del ratolí per a triar casella de la hotbar
// - Si la casella està buida, ataques amb el PUNY (curta distància)
public class PlayerAttack : MonoBehaviour
{
    [Header("Puny (quan la casella està buida)")]
    public int fistDamage = 1;
    public float fistRange = 0.7f;       // Distància del centre del jugador al colp
    public float fistRadius = 0.45f;     // Mida de la zona del colp
    public float fistCooldown = 0.35f;
    public float punchKnockback = 2.5f;

    [Header("Hotbar (tecles 1-4)")]
    public WeaponData[] hotbarSlots = new WeaponData[4];
    public TMP_FontAsset hotbarFont;     // Si està buit, agafa la font del nom del jugador

    [Header("Apuntat")]
    public bool showAimLine = true;
    public Color aimColor = new Color(1f, 1f, 1f, 0.45f);
    [Range(0f, 1f)]
    public float aimChargingOpacity = 0.35f; // Opacitat de la part encara no carregada (respecte d'aimColor)

    [Header("Arma a la mà")]
    public float handDistance = 0.45f;   // Distància del centre del jugador a l'arma
    public KeyCode dropKey = KeyCode.Z;  // Deixa anar al terra l'arma de la casella triada

    [Header("Efectes")]
    public CameraFollow cameraFollow;

    private SpriteRenderer handSprite;   // L'arma que porta a la mà
    private float recoil;                // Retrocés de l'arma en disparar

    private WeaponData spinningWeapon;
    private float spinCharge;
    private bool reverseSwing;
    private int selectedSlot = 0;
    private bool mouseBlocked;          // El clic ha començat damunt de la UI
    private readonly float[] slotCooldown = new float[4];
    private LineRenderer aimFill; // Part carregada del cooldown, damunt de l'aim
    private Vector2 aimDirection = Vector2.right;
    public Vector2 AimDirection => aimDirection;
    private Vector3 originalScale;

    private Camera cam;
    private LineRenderer aimLine;
    private HotbarUI hotbarUI;
    private SpriteRenderer playerSprite;

    public WeaponData CurrentWeapon
    {
        get
        {
            if (hotbarSlots == null || selectedSlot >= hotbarSlots.Length) return null;
            return hotbarSlots[selectedSlot];
        }
    }

    void Start()
    {
        originalScale = transform.localScale;
        cam = Camera.main;
        playerSprite = GetComponent<SpriteRenderer>();

        if (hotbarSlots == null || hotbarSlots.Length == 0)
            hotbarSlots = new WeaponData[4];

        // A la masmorra, les armes es conserven entre pisos
        if (RunState.Active && RunState.Weapons != null)
            for (int i = 0; i < hotbarSlots.Length && i < RunState.Weapons.Length; i++)
                hotbarSlots[i] = RunState.Weapons[i];
        if (RunState.Active && RunState.WeaponCooldowns != null)
            System.Array.Copy(RunState.WeaponCooldowns, slotCooldown, slotCooldown.Length);

        CreateAimLine();
        CreateHand();

        // Mateixa tipografia que el nom de damunt del jugador
        TMP_FontAsset font = hotbarFont;
        if (font == null)
        {
            PlayerNameTag nameTag = GetComponent<PlayerNameTag>();
            if (nameTag != null && nameTag.playerNameText != null)
                font = nameTag.playerNameText.font;
        }

        hotbarUI = HotbarUI.Create(hotbarSlots.Length, font);
        hotbarUI.SetCallbacks(SelectSlot, DropSlot, SwapSlots,
            i => hotbarSlots[i] != null ? hotbarSlots[i].icon : null);
        hotbarUI.Refresh(hotbarSlots, selectedSlot);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        // Tab: mostra / amaga la línia d'apuntat
        if (GameControls.Down(7))
            showAimLine = !showAimLine;

        HandleSlotSelection();
        slotCooldown[selectedSlot] = Mathf.Max(0f, slotCooldown[selectedSlot] - Time.deltaTime);
        UpdateAim();
        UpdateHand();

        if (GameControls.Down(6))
            DropCurrentWeapon();

        bool attackPressed = MobileInput.Active
            ? !GameControls.Blocked && MobileInput.Firing
            : ReadDesktopAttack(Input.GetMouseButtonDown(0), Input.GetMouseButton(0), Input.mousePosition,
                GameControls.Key(4) == KeyCode.Mouse0 || GameControls.Key(4, 1) == KeyCode.Mouse0, GameControls.AttackHeld(true));
        TickAttack(attackPressed);
    }

    bool ReadDesktopAttack(bool pressed, bool held, Vector2 pointer, bool mouseBound, bool otherBindingHeld)
    {
        // Només l'inici del clic decideix si la UI se'l queda.
        if (pressed) mouseBlocked = ClickTargetsUI(pointer);
        if (!held) mouseBlocked = false;
        if (CreativeMode.Placing) return false; // el clic col·loca un enemic o cofre del mode Creatiu
        return !GameControls.Blocked && (otherBindingHeld || (mouseBound && held && !mouseBlocked));
    }

    void TickAttack(bool attackPressed)
    {
        if(!attackPressed || spinningWeapon!=CurrentWeapon){spinCharge=0;spinningWeapon=CurrentWeapon;}
        if(attackPressed && CurrentWeapon!=null && CurrentWeapon.spinUpTime>0)
        {
            if(spinCharge==0) Fx.Text(transform.position,"Carregant...",new Color(1f,0.8f,0.2f));
            spinCharge+=Time.deltaTime;
            if(spinCharge<CurrentWeapon.spinUpTime) return;
        }
        if (attackPressed && slotCooldown[selectedSlot] <= 0f)
        {
            WeaponData weapon = CurrentWeapon;

            if (weapon == null)
            {
                Punch();
                slotCooldown[selectedSlot] = fistCooldown;
            }
            else
            {
                Shoot(weapon);
                slotCooldown[selectedSlot] = weapon.cooldown;
            }
        }
    }

    readonly System.Collections.Generic.List<RaycastResult> clickHits = new();

    bool ClickTargetsUI(Vector2 pointer)
    {
        EventSystem events = EventSystem.current;
        if (events == null) return false;
        events.RaycastAll(new PointerEventData(events) { position = pointer }, clickHits);
        foreach (var hit in clickHits)
        {
            if (!(hit.module is UnityEngine.UI.GraphicRaycaster)) continue;
            GameObject target = hit.gameObject;
            return ExecuteEvents.GetEventHandler<IPointerDownHandler>(target) != null
                || ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) != null;
        }
        return false;
    }

    // ---------- Apuntar ----------

    void UpdateAim()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = MobileInput.Active ? MobileInput.Aim : (Vector2)mouseWorld - (Vector2)transform.position;

        if (dir.sqrMagnitude > 0.001f)
            aimDirection = dir.normalized;

        if (aimLine == null || aimFill == null)
            return;

        aimLine.enabled = showAimLine;

        // Rectangle: per al puny, tan ample i llarg com la zona del colp;
        // per a les armes, tan ample com el projectil i llarg com l'abast
        WeaponData weapon = CurrentWeapon;
        float length = Mathf.Max(weapon != null ? weapon.range : fistRange + fistRadius, 0.5f);
        float width = weapon != null ? Mathf.Max(weapon.projectileSize, 0.15f) : fistRadius * 2f;

        // Amb dispersió, l'aim és una V: estret al jugador i ample al final.
        // Amplària final = 2 · llargària · tan(angle) -> les bales cauen just dins de la V
        float endWidth = width;
        if (weapon != null && weapon.spreadAngle > 0f)
            endWidth = Mathf.Max(width, 2f * length * Mathf.Tan(weapon.spreadAngle * Mathf.Deg2Rad));

        // Progrés del cooldown: 0 = acabes d'atacar, 1 = llest
        float remaining = slotCooldown[selectedSlot];
        float progress = remaining <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / (weapon != null ? weapon.cooldown : fistCooldown));
        bool charging = progress < 1f;

        Vector2 start = transform.position;
        Vector2 end = start + aimDirection * length;

        // Fons: mentre carrega és més fluix; quan està llest, color normal
        Color baseColor = aimColor;
        if (charging)
            baseColor.a *= aimChargingOpacity;

        bool radial = weapon != null && weapon.melee && weapon.meleeRadial;
        if (radial) SetAimCircle(aimLine, start, length, 1f, baseColor);
        else SetLine(aimLine, start, end, width, endWidth, baseColor);

        // Part carregada: creix des del jugador cap a fora amb el color normal
        // (l'opacitat es calcula perquè, sumada al fons, done just aimColor i no faça salt en acabar)
        aimFill.enabled = showAimLine && charging;
        if (aimFill.enabled)
        {
            Color fillColor = aimColor;
            fillColor.a = baseColor.a >= 1f ? 1f : (aimColor.a - baseColor.a) / (1f - baseColor.a);
            // La part carregada segueix la forma de la V (a mig camí, mitja amplària)
            if (radial) SetAimCircle(aimFill, start, length, progress, fillColor);
            else SetLine(aimFill, start, start + aimDirection * (length * progress), width, Mathf.Lerp(width, endWidth, progress), fillColor);
        }
    }

    void SetAimCircle(LineRenderer line, Vector2 center, float radius, float progress, Color color)
    {
        const int segments = 48;
        line.positionCount = segments + 1;
        line.startWidth = line.endWidth = 0.045f;
        line.startColor = line.endColor = color;
        for (int i = 0; i <= segments; i++)
        {
            float angle = 2f * Mathf.PI * progress * i / segments;
            line.SetPosition(i, center + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius);
        }
    }

    void SetLine(LineRenderer line, Vector2 start, Vector2 end, float startWidth, float endWidth, Color color)
    {
        line.positionCount = 2;
        line.startWidth = startWidth;
        line.endWidth = endWidth;
        line.startColor = color;
        line.endColor = color;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    void CreateAimLine()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        Material material = new Material(shader);

        // Les dues línies es dibuixen davall del jugador; la de càrrega, damunt del fons
        aimLine = CreateLine("AimLine", material, -2);
        aimFill = CreateLine("AimCooldown", material, -1);
        aimFill.enabled = false;
    }

    LineRenderer CreateLine(string name, Material material, int orderOffset)
    {
        // L'aim és NOMÉS visual: un LineRenderer sense collider (no té hitbox, no bloqueja
        // res i els atacs no el detecten). La capa Ignore Raycast és per a assegurar-ho.
        GameObject lineGO = new GameObject(name);
        lineGO.transform.SetParent(transform, false);
        lineGO.layer = LayerMask.NameToLayer("Ignore Raycast");

        LineRenderer line = lineGO.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.numCapVertices = 0; // Puntes rectes = rectangle
        line.material = material;

        if (playerSprite != null)
        {
            line.sortingLayerID = playerSprite.sortingLayerID;
            line.sortingOrder = playerSprite.sortingOrder + orderOffset;
        }
        return line;
    }

    // ---------- Hotbar ----------

    void HandleSlotSelection()
    {
        int newSlot = selectedSlot;

        for (int i = 0; i < hotbarSlots.Length && i < 9; i++)
        {
            if (GameControls.Down(11 + i))
                newSlot = i;
        }

        float scroll = GameControls.Wheel == 3 || GameControls.WheelHeals ? 0f : Input.mouseScrollDelta.y;
        if (scroll < 0f) newSlot = (selectedSlot + 1) % hotbarSlots.Length;
        if (scroll > 0f) newSlot = (selectedSlot - 1 + hotbarSlots.Length) % hotbarSlots.Length;

        if (newSlot != selectedSlot)
        {
            selectedSlot = newSlot;
            hotbarUI.Refresh(hotbarSlots, selectedSlot);
        }
    }

    // Per si en el futur vols recollir armes del terra
    public bool HasFreeSlot { get { if (hotbarSlots == null) return false; foreach (var w in hotbarSlots) if (w == null) return true; return false; } }

    public bool AddWeapon(WeaponData weapon, float cooldownRemaining = 0f)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = weapon;
                slotCooldown[i] = Mathf.Clamp(cooldownRemaining, 0f, weapon.cooldown);
                hotbarUI.Refresh(hotbarSlots, selectedSlot);
                return true;
            }
        }
        return false; // Hotbar plena
    }

    // ---------- Atacs ----------

    void Punch()
    {
        Vector2 hitPoint = (Vector2)transform.position + aimDirection * fistRange;

        StartCoroutine(AttackSquash());
        StartCoroutine(PunchFlash(hitPoint));
        NetCoop.LocalPunch(hitPoint);

        bool hitSomething = false;

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(hitPoint, fistRadius))
        {
            Breakable breakable = hit.GetComponentInParent<Breakable>();
            if (breakable != null)
            {
                breakable.Hit(fistDamage);
                hitSomething = true;
                continue;
            }

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null)
                continue;

            enemy.TakeHit(fistDamage, aimDirection * punchKnockback);
            hitSomething = true;
        }

        if (hitSomething && cameraFollow != null)
            cameraFollow.ShakeCamera(0.1f, 0.1f);
    }

    void Shoot(WeaponData weapon)
    {
        if (weapon.melee) { Swing(weapon); return; }
        Vector2 center = transform.position;
        Vector2 muzzle = center + aimDirection * Mathf.Max(0f, MuzzleDistance(weapon) - 0.2f) + Vector2.down * 0.08f;
        Vector2 spawnPos = Projectile.SafeMuzzle(center, muzzle, weapon.projectileSize * 0.5f, transform);
        int layer = playerSprite != null ? playerSprite.sortingLayerID : 0;
        int order = playerSprite != null ? playerSprite.sortingOrder + 1 : 0;

        float range = Mathf.Max(weapon.range, 0.5f);
        int pellets = Mathf.Max(1, weapon.pellets);

        for (int i = 0; i < pellets; i++)
        {
            float angle = weapon.fixedSpread && pellets > 1
                ? Mathf.Lerp(-weapon.spreadAngle, weapon.spreadAngle, i / (float)(pellets - 1))
                : Random.Range(-weapon.spreadAngle, weapon.spreadAngle);
            Vector2 dir = Rotate(aimDirection, angle);

            // Acaba a la vora del final de l'aim: com més desviada, un poc més lluny
            // (range / cos) perquè toque la línia del final de la V i no s'acurte.
            // rangeRandom fa que algunes es queden més curtes (perdigons).
            float dist = range / Mathf.Cos(angle * Mathf.Deg2Rad);
            if (!weapon.fixedSpread) dist *= 1f - Random.Range(0f, weapon.rangeRandom);

            Vector2 end = (Vector2)transform.position + dir * dist;
            Projectile.Spawn(spawnPos, dir, weapon, transform, layer, order, end);
            NetCoop.LocalPellet(weapon, spawnPos, dir, end); // Online: els companys tambe la veuen
        }

        WeaponSounds.Play(WeaponSounds.ShootClip(weapon), 0.5f);
        recoil = pellets > 1 ? 0.14f : 0.08f; // L'arma recula (més si és escopeta)
    }

    // Un colp per objectiu dins de l'arc, amb visibilitat per evitar pegar a traves de murs.
    void Swing(WeaponData weapon)
    {
        if(weapon.meleeDashDistance>0) GetComponent<PlayerMovement>()?.Dash(aimDirection,weapon.meleeDashDistance);
        reverseSwing=weapon.alternateSwing && !reverseSwing;
        NetCoop.LocalSwing(weapon, aimDirection, reverseSwing);
        var enemies = new System.Collections.Generic.HashSet<EnemyHealth>();
        var objects = new System.Collections.Generic.HashSet<Breakable>();
        Vector2 origin = transform.position;
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(origin, weapon.range))
        {
            Vector2 delta = (Vector2)hit.ClosestPoint(origin) - origin;
            if (delta.sqrMagnitude > 0.01f && !weapon.meleeRadial && Vector2.Angle(aimDirection, delta) > weapon.meleeArc * 0.5f) continue;
            if (TileNav.Ready() && !TileNav.LineOfSight(origin, hit.transform.position)) continue;
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null && enemies.Add(enemy)) enemy.TakeHit(weapon.damage, aimDirection * weapon.knockbackForce);
            Breakable box = hit.GetComponentInParent<Breakable>();
            if (box != null && objects.Add(box)) box.Hit(weapon.damage);
        }
        StartCoroutine(AttackSquash());
        StartCoroutine(SwingArc(weapon));
        WeaponSounds.Play(WeaponSounds.ShootClip(weapon), 0.35f);
    }

    IEnumerator SwingArc(WeaponData weapon)
    {
        GameObject go = new GameObject("Arc de tall");
        LineRenderer line = go.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 17; line.startWidth = 0.13f; line.endWidth = 0.03f;
        line.sortingLayerID = playerSprite.sortingLayerID; line.sortingOrder = playerSprite.sortingOrder + 3;
        Color color = weapon.projectileColor;
        for (int i = 0; i < 17; i++) line.SetPosition(i, transform.position + (Vector3)(Rotate(aimDirection, (reverseSwing?-1:1) * ((weapon.meleeRadial?360:weapon.meleeArc) * (i / 16f - 0.5f))) * weapon.range));
        for (float t = 0; t < 0.16f; t += Time.deltaTime) { color.a = 1f - t / 0.16f; line.startColor = line.endColor = color; yield return null; }
        Destroy(line.material); Destroy(go);
    }
    // Gira un vector 'degrees' graus (positiu = sentit antihorari)
    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float r = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    // Distància del centre del jugador a la punta de l'arma
    float MuzzleDistance(WeaponData weapon)
    {
        if (weapon.icon == null) return handDistance;
        float length = weapon.icon.rect.width / weapon.icon.pixelsPerUnit * weapon.handScale;
        return handDistance + length * 0.5f;
    }

    // ---------- Arma a la mà ----------

    void CreateHand()
    {
        // No és fill del jugador perquè la seua escala no deforme l'arma
        GameObject go = new GameObject("Arma a la mà");
        go.layer = LayerMask.NameToLayer("Ignore Raycast"); // Només visual
        handSprite = go.AddComponent<SpriteRenderer>();
        handSprite.enabled = false;
    }

    // L'arma segueix el jugador i apunta cap al ratolí.
    // Quan apuntes cap a l'esquerra es gira en espill (si no, es veuria cap per avall).
    void UpdateHand()
    {
        if (handSprite == null) return;

        WeaponData weapon = CurrentWeapon;
        if (weapon == null || weapon.icon == null)
        {
            handSprite.enabled = false; // Puny: no porta res
            return;
        }

        handSprite.enabled = true;
        handSprite.sprite = weapon.icon;
        handSprite.flipY = aimDirection.x < 0f;

        // Si apuntes cap amunt, l'arma queda darrere del jugador; si no, davant
        if (playerSprite != null)
        {
            handSprite.sortingLayerID = playerSprite.sortingLayerID;
            handSprite.sortingOrder = aimDirection.y > 0.35f ? playerSprite.sortingOrder - 1 : playerSprite.sortingOrder + 1;
        }

        recoil = Mathf.MoveTowards(recoil, 0f, Time.deltaTime * 0.6f);
        Vector2 pos = (Vector2)transform.position + aimDirection * (handDistance - recoil) + Vector2.down * 0.08f;

        Transform hand = handSprite.transform;
        hand.position = pos;
        hand.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg);
        hand.localScale = Vector3.one * weapon.handScale;
    }

    // Z: deixa l'arma de la casella triada al terra, davant del jugador
    void DropCurrentWeapon() => DropSlot(selectedSlot);

    // Clic a una casella de la hotbar: la tria
    public void SelectSlot(int i)
    {
        if (i < 0 || i >= hotbarSlots.Length) return;
        selectedSlot = i;
        hotbarUI.Refresh(hotbarSlots, selectedSlot);
    }

    // Arrossegar una arma a una altra casella: les intercanvia
    public void SwapSlots(int a, int b)
    {
        (hotbarSlots[a], hotbarSlots[b]) = (hotbarSlots[b], hotbarSlots[a]);
        (slotCooldown[a], slotCooldown[b]) = (slotCooldown[b], slotCooldown[a]);
        if (selectedSlot == a) selectedSlot = b;
        else if (selectedSlot == b) selectedSlot = a;
        hotbarUI.Refresh(hotbarSlots, selectedSlot);
    }

    // Online: en caure, totes les armes queden al terra al voltant (els companys les poden agafar)
    public void DropAllOnDeath()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            WeaponData weapon = hotbarSlots[i];
            float remaining = slotCooldown[i];
            slotCooldown[i] = 0f;
            if (weapon == null) continue;
            hotbarSlots[i] = null;
            float a = (i / (float)hotbarSlots.Length) * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
            Vector3 from = transform.position;
            Vector3 to = from + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * 1.1f;
            ItemPickup.Spawn(weapon, from, TileNav.DropDestination(from, to), cooldownRemaining: remaining);
        }
        if (hotbarUI != null) hotbarUI.Refresh(hotbarSlots, selectedSlot);
        SaveRunInventory();
    }

    public void SaveRunInventory()
    {
        RunState.Weapons = (WeaponData[])hotbarSlots.Clone();
        RunState.WeaponCooldowns = (float[])slotCooldown.Clone();
    }

    // Tira al terra l'arma de la casella 'i' (Z o arrossegar-la fora de la hotbar)
    public void DropSlot(int i)
    {
        if (i < 0 || i >= hotbarSlots.Length) return;
        WeaponData weapon = hotbarSlots[i];
        if (weapon == null) return;

        hotbarSlots[i] = null;
        hotbarUI.Refresh(hotbarSlots, selectedSlot);

        Vector3 from = transform.position;
        Vector3 to = transform.position + (Vector3)(aimDirection * 1f);
        ItemPickup.Spawn(weapon, from, TileNav.DropDestination(from, to), true, slotCooldown[i]);
        slotCooldown[i] = 0f;
    }

    void OnDisable()
    {
        if (handSprite == null) return;
        handSprite.enabled = false;
        transform.localScale = originalScale;
    }

    void OnDestroy()
    {
        if (handSprite != null) Destroy(handSprite.gameObject);
    }

    // ---------- Efectes visuals ----------

    // Xicoteta "empenta" del jugador en atacar
    IEnumerator AttackSquash()
    {
        transform.localScale = originalScale * 1.15f;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = originalScale;
    }

    // Cercle blanc que apareix on pega el puny i s'esvaeix
    IEnumerator PunchFlash(Vector2 position)
    {
        GameObject flash = new GameObject("PunchFlash");
        flash.transform.position = position;
        flash.layer = LayerMask.NameToLayer("Ignore Raycast"); // També només visual

        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = Projectile.CircleSprite;
        if (playerSprite != null)
        {
            sr.sortingLayerID = playerSprite.sortingLayerID;
            sr.sortingOrder = playerSprite.sortingOrder + 1;
        }

        const float duration = 0.12f;
        float t = 0f;
        while (t < duration)
        {
            float k = t / duration;
            flash.transform.localScale = Vector3.one * Mathf.Lerp(fistRadius * 2f, fistRadius * 3f, k);
            sr.color = new Color(1f, 1f, 1f, 0.6f * (1f - k));
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(flash);
    }

    // Dibuixa la zona del puny a la vista Scene (només a l'editor)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + aimDirection * fistRange, fistRadius);
    }
}
