using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

// Atac estil Brawl Stars:
// - Apuntes amb el ratolí (es veu una línia cap on apuntes)
// - Clic esquerre (o Espai) per a atacar
// - Tecles 1-4 o la roda del ratolí per a triar casella de la hotbar
// - Si la casella està buida, ataques amb el PUNY (curta distància)
public partial class PlayerAttack : MonoBehaviour
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
    private readonly bool[] overheated = new bool[4]; // Minigun: bloquejada fins que l'escalfament torna a 0
    private readonly WeaponCondition[] conditions = new WeaponCondition[4];
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
        if (RunState.Active && RunState.WeaponConditions != null)
            System.Array.Copy(RunState.WeaponConditions, conditions, conditions.Length);

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
        // v1.3: com abans de la v1.2.1, nomes baixa el cooldown de l'arma que tens a la ma;
        // les armes guardades es queden congelades, llevat de les de cos a cos (i l'escalfament, mes avall).
        // v1.4.1: el Bàcul de torreta també es carrega guardat, però al 80 % del ritme (10 s -> 12,5 s)
        for (int s = 0; s < slotCooldown.Length; s++)
        {
            bool turretStowed = s != selectedSlot && hotbarSlots[s] != null && hotbarSlots[s].special == SpecialKind.Turret;
            if (s == selectedSlot || (hotbarSlots[s] != null && hotbarSlots[s].melee) || turretStowed)
                slotCooldown[s] = Mathf.Max(0f, slotCooldown[s] - Time.deltaTime * (turretStowed ? StowedTurretRate : 1f));
            // v1.4.1: Llançamines amb càrregues. conditions[s].Heat = càrregues que falten (0 = totes plenes).
            // Cada una es recupera en 1,5 s a la mà i en ~1,9 s guardat (80 %), una darrere de l'altra.
            if (hotbarSlots[s] != null && hotbarSlots[s].special == SpecialKind.Mine)
                conditions[s].Heat = Mathf.Max(0f, conditions[s].Heat - Time.deltaTime * (s == selectedSlot ? 1f : StowedTurretRate) / MineRecharge);
        }
        UpdateAim();
        UpdateHand();

        if (GameControls.Down(6))
            DropCurrentWeapon();

        bool attackPressed = MobileInput.Active
            ? !GameControls.Blocked && MobileInput.Firing
            : ReadDesktopAttack(Input.GetMouseButtonDown(0), Input.GetMouseButton(0), Input.mousePosition,
                GameControls.Key(4) == KeyCode.Mouse0 || GameControls.Key(4, 1) == KeyCode.Mouse0, GameControls.AttackHeld(true));
        TickAttack(attackPressed);
        TickShieldEnergy(); // v1.4.1: recàrrega de l escut
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            var weapon = hotbarSlots[i];
            float progress = weapon == null ? -1f : weapon.maxShots > 0 ?
                1f - conditions[i].ShotsUsed / (float)weapon.maxShots : weapon.heatLimit > 0 ?
                conditions[i].Heat / weapon.heatLimit : -1f;
            // v1.4.1: el Bàcul ensenya a la casella com va la recàrrega (roig -> verd; desapareix quan està llest)
            if (weapon != null && weapon.special == SpecialKind.Turret && weapon.cooldown > 0f)
                progress = slotCooldown[i] > 0f ? 1f - Mathf.Clamp01(slotCooldown[i] / weapon.cooldown) : -1f;
            // Llançamines: la barra és la càrrega que s'està recuperant i el text, les que tens («2/3»)
            bool mines = weapon != null && weapon.special == SpecialKind.Mine;
            if (weapon != null && weapon.special == SpecialKind.Shield) progress = ShieldEnergyFraction; // barra d energia de l escut
            if (mines) { float missing = conditions[i].Heat; progress = missing > 0f ? 1f - (missing - Mathf.Floor(missing - 0.0001f)) : -1f; }
            hotbarUI.SetCharges(i, mines ? MinesReady(conditions[i]) + "/" + MineCharges : null);
            hotbarUI.SetWeaponCondition(i, progress, weapon != null && weapon.heatLimit > 0);
        }
    }

    // v1.4.1: servei «Recarregar combustible» del mercader. Fracció gastada (0-1) de l'arma de trets limitats que més n'ha gastat; -1 si no en portes cap.
    public float FuelUsedFraction()
    {
        float worst = -1f;
        for (int i = 0; i < hotbarSlots.Length; i++)
            if (hotbarSlots[i] != null && hotbarSlots[i].maxShots > 0)
                worst = Mathf.Max(worst, conditions[i].ShotsUsed / (float)hotbarSlots[i].maxShots);
        return worst;
    }

    public void RefuelAll()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
            if (hotbarSlots[i] != null && hotbarSlots[i].maxShots > 0) conditions[i].ShotsUsed = 0;
        hotbarUI.Refresh(hotbarSlots, selectedSlot);
    }

    const float StowedTurretRate = 0.8f; // també per a les càrregues del Llançamines
    const int MineCharges = 3;
    const float MineRecharge = 1.5f;
    public static int MinesReady(WeaponCondition c) => Mathf.Clamp(MineCharges - Mathf.CeilToInt(c.Heat - 0.0001f), 0, MineCharges);
    float noFuelAt = -10f;
    void NoFuel()
    {
        if (Time.time - noFuelAt >= 1f) { noFuelAt = Time.time; Fx.Text(transform.position, "Sense combustible!", new Color(1f, .45f, .2f)); }
        slotCooldown[selectedSlot] = Mathf.Max(slotCooldown[selectedSlot], .2f);
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
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            var gun = hotbarSlots[i];
            if (gun == null || gun.heatLimit <= 0) { overheated[i] = false; continue; }
            if (i != selectedSlot || !attackPressed || slotCooldown[i] > gun.cooldown || overheated[i])
                conditions[i].Heat = Mathf.Max(0f, conditions[i].Heat - gun.heatCoolingRate * Time.deltaTime);
            if (overheated[i] && conditions[i].Heat <= 0f) overheated[i] = false; // v1.3: no pots tornar a disparar fins que la barra torna al principi
        }
        if(!attackPressed || spinningWeapon!=CurrentWeapon){spinCharge=0;spinningWeapon=CurrentWeapon;}
        if(attackPressed && CurrentWeapon!=null && CurrentWeapon.spinUpTime>0)
        {
            if(spinCharge==0) Fx.Text(transform.position,"Carregant...",new Color(1f,0.8f,0.2f));
            spinCharge+=Time.deltaTime;
            if(spinCharge<CurrentWeapon.spinUpTime) return;
        }
        if (TickSpecialHeld(attackPressed)) return; // v1.3.1: Arc de llum (càrrega) i Escut
        if (attackPressed && slotCooldown[selectedSlot] <= 0f && !overheated[selectedSlot])
        {
            WeaponData weapon = CurrentWeapon;

            if (weapon == null)
            {
                Punch();
                slotCooldown[selectedSlot] = fistCooldown;
            }
            else if (weapon.maxShots > 0 && RunState.Active && conditions[selectedSlot].ShotsUsed >= weapon.maxShots)
            {
                NoFuel(); // v1.4.0: l'arma no desapareix; es queda buida
            }
            else if (weapon.special == SpecialKind.Mine && MinesReady(conditions[selectedSlot]) <= 0)
            {
                slotCooldown[selectedSlot] = 0.1f; // sense càrregues: espera que se'n recupere una
            }
            else
            {
                Shoot(weapon);
                slotCooldown[selectedSlot] = weapon.cooldown;
                ref WeaponCondition condition = ref conditions[selectedSlot];
                if (weapon.special == SpecialKind.Mine) condition.Heat = Mathf.Min(MineCharges, condition.Heat + 1f); // gasta una càrrega
                if (weapon.heatLimit > 0 && ++condition.Heat >= weapon.heatLimit)
                {
                    condition.Heat = weapon.heatLimit;
                    slotCooldown[selectedSlot] = weapon.overheatCooldown;
                    overheated[selectedSlot] = true;
                    Fx.Text(transform.position, "Sobreescalfada", new Color(1f, .45f, .2f));
                }
                // v1.2.1 (FIX-019): només en masmorra es trenquen; al mapa lliure i al mode Creatiu no
                if (weapon.maxShots > 0 && RunState.Active && ++condition.ShotsUsed >= weapon.maxShots)
                {
                    condition.ShotsUsed = weapon.maxShots; // v1.4.0: sense combustible, l'arma es queda a la hotbar
                    NoFuel();
                    hotbarUI.Refresh(hotbarSlots, selectedSlot);
                }
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
        SyncAimSorting();

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
        // Cos a cos: l'aim dibuixa la V real del colp (amplada de la corda de l'arc)
        if (weapon != null && weapon.melee && weapon.meleeLineWidth > 0f) { width = weapon.meleeLineWidth; endWidth = width; }
        else if (weapon != null && weapon.melee && !weapon.meleeRadial)
            endWidth = Mathf.Max(width, 2f * length * Mathf.Sin(weapon.meleeArc * 0.5f * Mathf.Deg2Rad));

        // Progrés del cooldown: 0 = acabes d'atacar, 1 = llest
        float remaining = slotCooldown[selectedSlot];
        float progress = remaining <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / (weapon != null ? (remaining > weapon.cooldown ? weapon.overheatCooldown : weapon.cooldown) : fistCooldown));
        bool charging = progress < 1f;
        if (ChargeProgress(weapon, out float chargeProgress)) { progress = chargeProgress; charging = true; }

        Vector2 start = transform.position;
        Vector2 end = start + aimDirection * length;
        // v1.4.1: armes de col·locar (Torreta, Mines): l'aim acaba on cau l'objecte i el marca amb un cercle
        bool placer = PlaceDistance(weapon) > 0f;
        if (placer)
        {
            end = SpotInFront(PlaceDistance(weapon));
            length = Mathf.Max(0.05f, Vector2.Distance(start, end));
            width = endWidth = 0.12f;
        }

        // Fons: mentre carrega és més fluix; quan està llest, color normal
        Color baseColor = aimColor;
        if (charging)
            baseColor.a *= aimChargingOpacity;

        bool radial = weapon != null && weapon.melee && weapon.meleeRadial;
        if (radial) SetAimCircle(aimLine, start, length, 1f, baseColor);
        else if (weapon != null && !weapon.melee && weapon.wallBounces > 0 && GameOptions.BounceAim)
        {
            aimLine.enabled = false; // el recorregut complet amb rebots el dibuixa DrawBouncePath
            DrawBouncePath(weapon, start, baseColor, 0.14f);
        }
        else SetLine(aimLine, start, end, width, endWidth, baseColor);
        if (aimBounce != null && !(weapon != null && !weapon.melee && weapon.wallBounces > 0 && GameOptions.BounceAim)) aimBounce.enabled = false;
        if (placer && aimSpot == null) aimSpot = CreateLine("AimSpot", aimLine.sharedMaterial, -3);
        if (aimSpot != null)
        {
            aimSpot.enabled = placer && showAimLine;
            if (aimSpot.enabled) { SortLine(aimSpot, playerSprite != null ? playerSprite.sortingOrder - 3 : 0); SetAimCircle(aimSpot, end, 0.35f, 1f, baseColor); }
        }

        // Part carregada: creix des del jugador cap a fora amb el color normal
        // (l'opacitat es calcula perquè, sumada al fons, done just aimColor i no faça salt en acabar)
        aimFill.enabled = showAimLine && charging && !(weapon != null && !weapon.melee && weapon.wallBounces > 0 && GameOptions.BounceAim);
        if (aimFill.enabled)
        {
            Color fillColor = aimColor;
            fillColor.a = baseColor.a >= 1f ? 1f : (aimColor.a - baseColor.a) / (1f - baseColor.a);
            // La part carregada segueix la forma de la V (a mig camí, mitja amplària)
            if (radial) SetAimCircle(aimFill, start, length, progress, fillColor);
            else SetLine(aimFill, start, start + aimDirection * (length * progress), width, Mathf.Lerp(width, endWidth, progress), fillColor);
        }
    }

    // Disc (v1.3): l'aim ensenya tots els rebots que farà, com el Rico del Brawl Stars.
    // Cada rebot allarga el recorregut un 15 % de l'abast (bounceRangeBonus).
    LineRenderer aimBounce, aimSpot;
    void DrawBouncePath(WeaponData w, Vector2 start, Color color, float width)
    {
        if (aimBounce == null) aimBounce = CreateLine("AimBounce", aimLine.sharedMaterial, -2);
        var points = new System.Collections.Generic.List<Vector2> { start };
        Vector2 pos = start, dir = aimDirection;
        float remaining = w.range;
        for (int segment = 0; segment <= w.wallBounces && remaining > 0.01f; segment++)
        {
            RaycastHit2D wall = default; bool found = false;
            foreach (var hit in Physics2D.RaycastAll(pos, dir, remaining))
            {
                var c = hit.collider;
                if (c == null || c.isTrigger || c.transform.IsChildOf(transform) || c.GetComponentInParent<EnemyHealth>() != null) continue;
                wall = hit; found = true; break;
            }
            if (!found) { points.Add(pos + dir * remaining); break; }
            points.Add(wall.point);
            if (segment == w.wallBounces) break;
            remaining = remaining - wall.distance + w.range * w.bounceRangeBonus;
            dir = Vector2.Reflect(dir, wall.normal);
            pos = wall.point + wall.normal * 0.02f;
        }
        aimBounce.enabled = showAimLine;
        aimBounce.positionCount = points.Count;
        aimBounce.startWidth = aimBounce.endWidth = width;
        aimBounce.startColor = aimBounce.endColor = color;
        for (int i = 0; i < points.Count; i++) aimBounce.SetPosition(i, points[i]);
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

    // v1.4.1: el cos canvia d'ordre cada frame segons la Y (PlayerCosmetics), aixi que l'aim
    // l'ha de seguir: sempre davall de canica, barret i arma, pero damunt de l'ombra (capa Shadows).
    void SyncAimSorting()
    {
        if (playerSprite == null) return;
        int body = playerSprite.sortingOrder;
        SortLine(aimLine, body - 3); SortLine(aimBounce, body - 3); SortLine(aimFill, body - 2);
    }

    void SortLine(LineRenderer line, int order)
    {
        if (line == null) return;
        line.sortingLayerID = playerSprite.sortingLayerID;
        line.sortingOrder = order;
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

        float scroll = GameControls.Wheel == 3 || GameControls.WheelHeals || CreativeMode.PointerOverPanel ? 0f : Input.mouseScrollDelta.y;
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

    public bool AddWeapon(WeaponData weapon, float cooldownRemaining = 0f, WeaponCondition condition = default)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = weapon; Almanac.PickedWeapon(weapon); // v1.4.1: l Almanac guarda l arma
                slotCooldown[i] = Mathf.Clamp(cooldownRemaining, 0f, Mathf.Max(weapon.cooldown, weapon.overheatCooldown));
                conditions[i] = new WeaponCondition(Mathf.Clamp(condition.ShotsUsed, 0, Mathf.Max(0, weapon.maxShots)),
                    float.IsNaN(condition.Heat) ? 0f : Mathf.Clamp(condition.Heat, 0f, weapon.special == SpecialKind.Mine ? MineCharges : weapon.heatLimit)); // Mines: càrregues que falten
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
                breakable.Hit(fistDamage, true);
                hitSomething = true;
                continue;
            }

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null)
                continue;

            RunStats.HitBy = "Puny";
            enemy.TakeHit(fistDamage, aimDirection * punchKnockback);
            RunStats.HitBy = null;
            hitSomething = true;
        }

        if (hitSomething && cameraFollow != null)
            cameraFollow.ShakeCamera(0.1f, 0.1f);
    }

    void Shoot(WeaponData weapon)
    {
        if (weapon.melee) { Swing(weapon); return; }
        if (weapon.special == SpecialKind.Turret || weapon.special == SpecialKind.Mine || weapon.special == SpecialKind.Grapple) { UseInstant(weapon); return; }
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
        if (weapon.special == SpecialKind.HealBolt) TryHealAlly(weapon);
    }

    // Un colp per objectiu dins de l'arc, amb visibilitat per evitar pegar a traves de murs.
    void Swing(WeaponData weapon)
    {
        if(weapon.meleeDashDistance>0) GetComponent<PlayerMovement>()?.Dash(aimDirection,weapon.meleeDashDistance);
        reverseSwing=weapon.alternateSwing && !reverseSwing;
        NetCoop.LocalSwing(weapon, aimDirection, reverseSwing);
        var enemies = new System.Collections.Generic.HashSet<EnemyHealth>();
        var objects = new System.Collections.Generic.HashSet<Breakable>();
        var targets = new System.Collections.Generic.List<(float dist, EnemyHealth enemy)>();
        Vector2 origin = transform.position;
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(origin, weapon.range))
        {
            Vector2 delta = (Vector2)hit.ClosestPoint(origin) - origin;
            if (weapon.meleeLineWidth > 0f)
            {
                // Llança (v1.3): colp en línia recta, no en V
                Vector2 centre = hit.bounds.center;
                Vector2 onLine = origin + aimDirection * Mathf.Clamp(Vector2.Dot(centre - origin, aimDirection), 0f, weapon.range);
                if (Vector2.Dot(centre - origin, aimDirection) < -0.3f || ((Vector2)hit.ClosestPoint(onLine) - onLine).magnitude > weapon.meleeLineWidth * 0.5f) continue;
            }
            else if (delta.sqrMagnitude > 0.01f && !weapon.meleeRadial && Vector2.Angle(aimDirection, delta) > weapon.meleeArc * 0.5f) continue;
            Breakable box = hit.GetComponentInParent<Breakable>();
            // El martell (radial) pega per damunt de les parets. La resta necessita visió.
            if (!weapon.meleeRadial && TileNav.Ready())
            {
                Vector2 to = hit.transform.position;
                if (box != null)
                {
                    // La casella de la caixa és un bloqueig: mira fins a mig tros abans del centre,
                    // però una paret de pel mig continua tallant el colp (BUG-021).
                    Vector2 toBox = to - origin; float len = toBox.magnitude;
                    to = len > 0.7f ? origin + toBox * ((len - 0.7f) / len) : origin;
                }
                if (!TileNav.LineOfSight(origin, to)) continue;
            }
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null && enemies.Add(enemy)) targets.Add((((Vector2)enemy.transform.position - origin).sqrMagnitude, enemy));
            if (box != null && objects.Add(box)) box.Hit(weapon.damage, true);
        }
        targets.Sort((a, b) => a.dist.CompareTo(b.dist));
        if (targets.Count > 0 && weapon.hitSpeedBoost > 0f) GetComponent<PlayerMovement>()?.BoostSpeed(1f + weapon.hitSpeedBoost, weapon.hitSpeedBoostTime);
        int hits = 0;
        foreach (var t in targets)
        {
            if (weapon.meleeMaxTargets > 0 && hits++ >= weapon.meleeMaxTargets) break;
            RunStats.HitBy = weapon.weaponName; t.enemy.TakeHit(weapon.damage, aimDirection * weapon.knockbackForce); RunStats.HitBy = null;
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
        if (weapon.meleeLineWidth > 0f)
        {
            line.positionCount = 2; line.startWidth = line.endWidth = weapon.meleeLineWidth * 0.5f;
            line.SetPosition(0, transform.position); line.SetPosition(1, transform.position + (Vector3)(aimDirection * weapon.range));
        }
        else
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

        handSprite.enabled = !(weapon.special == SpecialKind.Shield && ShieldIsUp); // amb l'escut alçat sols es veu l'escut (no dos)
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
        (conditions[a], conditions[b]) = (conditions[b], conditions[a]);
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
            ItemPickup.Spawn(weapon, from, TileNav.DropDestination(from, to), cooldownRemaining: remaining, condition: conditions[i]);
            conditions[i] = default;
        }
        if (hotbarUI != null) hotbarUI.Refresh(hotbarSlots, selectedSlot);
        SaveRunInventory();
    }

    public void SaveRunInventory()
    {
        RunState.Weapons = (WeaponData[])hotbarSlots.Clone();
        RunState.WeaponCooldowns = (float[])slotCooldown.Clone();
        RunState.WeaponConditions = (WeaponCondition[])conditions.Clone();
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
        ItemPickup.Spawn(weapon, from, TileNav.DropDestination(from, to), true, slotCooldown[i], conditions[i]);
        conditions[i] = default;
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
