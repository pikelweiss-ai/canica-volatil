using System.Collections;
using UnityEngine;
using TMPro;

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

    [Header("Efectes")]
    public CameraFollow cameraFollow;

    private int selectedSlot = 0;
    private float nextAttackTime = 0f;
    private float lastCooldownDuration = 1f;
    private LineRenderer aimFill; // Part carregada del cooldown, damunt de l'aim
    private Vector2 aimDirection = Vector2.right;
    private Vector3 originalScale;

    private Camera cam;
    private LineRenderer aimLine;
    private HotbarUI hotbarUI;
    private SpriteRenderer playerSprite;

    WeaponData CurrentWeapon
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

        CreateAimLine();

        // Mateixa tipografia que el nom de damunt del jugador
        TMP_FontAsset font = hotbarFont;
        if (font == null)
        {
            PlayerNameTag nameTag = GetComponent<PlayerNameTag>();
            if (nameTag != null && nameTag.playerNameText != null)
                font = nameTag.playerNameText.font;
        }

        hotbarUI = HotbarUI.Create(hotbarSlots.Length, font);
        hotbarUI.Refresh(hotbarSlots, selectedSlot);
    }

    void Update()
    {
        // Tab: mostra / amaga la línia d'apuntat
        if (Input.GetKeyDown(KeyCode.Tab))
            showAimLine = !showAimLine;

        UpdateAim();
        HandleSlotSelection();

        bool attackPressed = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        if (attackPressed && Time.time >= nextAttackTime)
        {
            WeaponData weapon = CurrentWeapon;

            if (weapon == null)
            {
                Punch();
                lastCooldownDuration = fistCooldown;
            }
            else
            {
                Shoot(weapon);
                lastCooldownDuration = weapon.cooldown;
            }
            nextAttackTime = Time.time + lastCooldownDuration;
        }
    }

    // ---------- Apuntar ----------

    void UpdateAim()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)mouseWorld - (Vector2)transform.position;

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

        // Progrés del cooldown: 0 = acabes d'atacar, 1 = llest
        float remaining = nextAttackTime - Time.time;
        float progress = remaining <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / lastCooldownDuration);
        bool charging = progress < 1f;

        Vector2 start = transform.position;
        Vector2 end = start + aimDirection * length;

        // Fons: mentre carrega és més fluix; quan està llest, color normal
        Color baseColor = aimColor;
        if (charging)
            baseColor.a *= aimChargingOpacity;

        SetLine(aimLine, start, end, width, baseColor);

        // Part carregada: creix des del jugador cap a fora amb el color normal
        // (l'opacitat es calcula perquè, sumada al fons, done just aimColor i no faça salt en acabar)
        aimFill.enabled = showAimLine && charging;
        if (aimFill.enabled)
        {
            Color fillColor = aimColor;
            fillColor.a = baseColor.a >= 1f ? 1f : (aimColor.a - baseColor.a) / (1f - baseColor.a);
            SetLine(aimFill, start, start + aimDirection * (length * progress), width, fillColor);
        }
    }

    void SetLine(LineRenderer line, Vector2 start, Vector2 end, float width, Color color)
    {
        line.startWidth = width;
        line.endWidth = width;
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
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                newSlot = i;
        }

        float scroll = Input.mouseScrollDelta.y;
        if (scroll < 0f) newSlot = (selectedSlot + 1) % hotbarSlots.Length;
        if (scroll > 0f) newSlot = (selectedSlot - 1 + hotbarSlots.Length) % hotbarSlots.Length;

        if (newSlot != selectedSlot)
        {
            selectedSlot = newSlot;
            hotbarUI.Refresh(hotbarSlots, selectedSlot);
        }
    }

    // Per si en el futur vols recollir armes del terra
    public bool AddWeapon(WeaponData weapon)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = weapon;
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

        bool hitSomething = false;

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(hitPoint, fistRadius))
        {
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
        Vector2 spawnPos = (Vector2)transform.position + aimDirection * 0.5f;
        int layer = playerSprite != null ? playerSprite.sortingLayerID : 0;
        int order = playerSprite != null ? playerSprite.sortingOrder + 1 : 0;

        Projectile.Spawn(spawnPos, aimDirection, weapon, transform, layer, order);
        StartCoroutine(AttackSquash());
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
