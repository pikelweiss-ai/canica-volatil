using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bossa de cures del jugador (a la dreta de la hotbar).
// - Quan passes per damunt d'un item de cura, el guardes (s'apilen: fins a 'maxStack' iguals).
// - Tecles 5, 6, 7, 8: uses la cura d'eixa casella. Cada item cura una quantitat diferent
//   (poma +1, bena +2, poció +3, farmaciola +5).
// - Si tens la vida plena, no la gasta ("Vida plena!").
// S'afig sol al jugador (veure PlayerHazards.AttachToPlayer).
public class HealInventory : MonoBehaviour
{
    public const int Slots = 4;
    public int maxStack = 5;

    public HealItem[] items = new HealItem[Slots];
    public int[] counts = new int[Slots];

    private int selectedHeal;
    private PlayerHealth health;
    private HealBarUI ui;

    void Start()
    {
        health = GetComponent<PlayerHealth>();

        // A la masmorra, la bossa es conserva entre pisos
        if (RunState.Active && RunState.HealItems != null)
            for (int i = 0; i < Slots && i < RunState.HealItems.Length; i++)
            {
                items[i] = RunState.HealItems[i];
                counts[i] = items[i] != null ? RunState.HealCounts[i] : 0;
            }
    }

    // L'item del terra crida açò. Torna false si no hi cap.
    public bool Add(HealItem item)
    {
        for (int i = 0; i < Slots; i++)
            if (items[i] == item && counts[i] < maxStack) { counts[i]++; Changed(); return true; }
        for (int i = 0; i < Slots; i++)
            if (items[i] == null) { items[i] = item; counts[i] = 1; Changed(); return true; }
        return false;
    }

    // Arrossegar una cura a una altra casella: les intercanvia
    public void Swap(int a, int b)
    {
        (items[a], items[b]) = (items[b], items[a]);
        (counts[a], counts[b]) = (counts[b], counts[a]);
        Changed();
    }

    // Arrossegar-la fora de la bossa: tira tota la pila al terra, davant teu
    public void Drop(int slot)
    {
        HealItem item = items[slot];
        if (item == null) return;
        PlayerAttack attack = GetComponent<PlayerAttack>();
        Vector3 dir = attack != null ? (Vector3)attack.AimDirection : Vector3.right;
        for (int k = 0; k < counts[slot]; k++)
        {
            Vector3 to = transform.position + dir * 1f + (Vector3)(Random.insideUnitCircle * 0.25f);
            HealPickup.Spawn(item, transform.position, to, true);
        }
        items[slot] = null;
        counts[slot] = 0;
        Changed();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (ui == null)
        {
            ui = HealBarUI.Create(FindFont());
            ui.SetCallbacks(Use, Drop, Swap, i => items[i] != null ? items[i].icon : null);
        }

        for (int i = 0; i < Slots; i++)
            if (GameControls.Down(15 + i))
                { selectedHeal = i; Use(i); }
        if (GameControls.WheelHeals && Input.mouseScrollDelta.y != 0f) selectedHeal = (selectedHeal + (Input.mouseScrollDelta.y < 0 ? 1 : 3)) % Slots;
        if (GameControls.Down(10)) Use(selectedHeal);
        ui.Select(selectedHeal);
    }

    public void Use(int slot)
    {
        HealItem item = items[slot];
        if (item == null || health == null) return;

        if (health.IsFull)
        {
            Fx.Text(transform.position, "Vida plena!", new Color(1f, 1f, 1f, 0.9f));
            ui.Shake(slot);
            return;
        }

        int before = health.currentHealth;
        health.Heal(item.healAmount);
        int healed = health.currentHealth - before;
        Fx.Text(transform.position, "+" + healed, new Color(0.45f, 1f, 0.45f));
        Fx.Play(Fx.Heal, 0.6f);
        Fx.Burst(transform.position, new Color(1f, 0.35f, 0.45f), 8, 2.5f);

        counts[slot]--;
        if (counts[slot] <= 0) { items[slot] = null; counts[slot] = 0; }
        Changed();
    }

    void Changed()
    {
        RunState.HealItems = (HealItem[])items.Clone();
        RunState.HealCounts = (int[])counts.Clone();
        if (ui != null) ui.Refresh(items, counts);
    }

    void LateUpdate()
    {
        if (ui != null && ui.NeedsRefresh) ui.Refresh(items, counts);
    }

    TMP_FontAsset FindFont()
    {
        HotbarUI hotbar = FindAnyObjectByType<HotbarUI>();
        TextMeshProUGUI t = hotbar != null ? hotbar.GetComponentInChildren<TextMeshProUGUI>() : null;
        return t != null ? t.font : null;
    }
}

// Les 4 caselles de cura, a la dreta de la hotbar d'armes (mateix estil)
public class HealBarUI : MonoBehaviour
{
    const float SlotSize = 70f, Spacing = 8f;

    private Image[] icons = new Image[HealInventory.Slots];
    private TextMeshProUGUI[] countTexts = new TextMeshProUGUI[HealInventory.Slots];
    private TextMeshProUGUI[] healTexts = new TextMeshProUGUI[HealInventory.Slots];
    private RectTransform[] slots = new RectTransform[HealInventory.Slots];
    private Vector2[] basePos = new Vector2[HealInventory.Slots];       // On va cada casella (no es mou mai)
    private Coroutine[] shaking = new Coroutine[HealInventory.Slots];
    private SlotInput[] inputs = new SlotInput[HealInventory.Slots];
    private TextMeshProUGUI usageLabel;
    private TMP_FontAsset font;
    public bool NeedsRefresh { get; private set; } = true;

    public static HealBarUI Create(TMP_FontAsset font)
    {
        GameObject go = new GameObject("HealBarCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10;
        CanvasScaler s = go.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.matchWidthOrHeight = 0.5f;

        HealBarUI ui = go.AddComponent<HealBarUI>();
        ui.font = font;
        ui.Build();
        return ui;
    }

    void Build()
    {
        // La hotbar d'armes fa 4 x 90 + 3 x 10 = 390 d'ample i està centrada: ens posem a la dreta
        float startX = MobileInput.Active ? -(HealInventory.Slots * SlotSize + (HealInventory.Slots - 1) * Spacing) / 2 : 390f / 2f + 30f;
        usageLabel=Text(transform,"",16,TextAlignmentOptions.Center,Vector2.zero,Vector2.zero);
        var hint=usageLabel.rectTransform;hint.anchorMin=hint.anchorMax=new Vector2(0.5f,0);hint.sizeDelta=new Vector2(370,28);hint.anchoredPosition=new Vector2(390,105);
        for (int i = 0; i < HealInventory.Slots; i++)
        {
            GameObject slot = new GameObject("Cura " + (i + 5), typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)slot.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = Vector2.zero;
            rt.sizeDelta = Vector2.one * SlotSize;
            basePos[i] = new Vector2(startX + i * (SlotSize + Spacing), MobileInput.Active ? 145f : 20f);
            rt.anchoredPosition = basePos[i];
            slot.GetComponent<Image>().color = new Color(0.35f, 0f, 0.05f, 0.45f);
            slots[i] = rt;
            inputs[i] = slot.AddComponent<SlotInput>();
            inputs[i].index = i;
            inputs[i].group = this;

            Image icon = new GameObject("Icona", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            icon.transform.SetParent(slot.transform, false);
            icon.rectTransform.anchorMin = new Vector2(0.15f, 0.15f);
            icon.rectTransform.anchorMax = new Vector2(0.85f, 0.85f);
            icon.rectTransform.offsetMin = icon.rectTransform.offsetMax = Vector2.zero;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icons[i] = icon;

            Text(slot.transform, GameControls.Label(15 + i), 18, TextAlignmentOptions.TopLeft, new Vector2(5f, 0f), new Vector2(0f, -3f)).gameObject.AddComponent<ControlKeyLabel>().action = 15 + i;
            countTexts[i] = Text(slot.transform, "", 20, TextAlignmentOptions.BottomRight, Vector2.zero, new Vector2(-5f, 2f));
            healTexts[i] = Text(slot.transform, "", 16, TextAlignmentOptions.TopRight, Vector2.zero, new Vector2(-4f, -3f));
            healTexts[i].color = new Color(0.5f, 1f, 0.5f);
        }
    }

    TextMeshProUGUI Text(Transform parent, string s, float size, TextAlignmentOptions align, Vector2 offMin, Vector2 offMax)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = offMin; rt.offsetMax = offMax;
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = s;
        t.fontSize = size;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    public void Refresh(HealItem[] items, int[] counts)
    {
        NeedsRefresh = false;
        for (int i = 0; i < HealInventory.Slots; i++)
        {
            HealItem it = items[i];
            icons[i].enabled = it != null && it.icon != null;
            if (icons[i].enabled) icons[i].sprite = it.icon;
            countTexts[i].text = it != null && counts[i] > 1 ? "x" + counts[i] : "";
            healTexts[i].text = it != null ? "+" + it.healAmount : "";
        }
    }

    public void SetCallbacks(System.Action<int> click, System.Action<int> dropOutside,
                             System.Action<int, int> swap, System.Func<int, Sprite> icon)
    {
        foreach (SlotInput s in inputs)
        {
            s.onClick = click;
            s.onDropOutside = dropOutside;
            s.onSwap = swap;
            s.getIcon = icon;
        }
    }

    public void Select(int index)
    {
        if(usageLabel!=null) usageLabel.text=MobileInput.Active ? "" : GameControls.Label(10)+" · usar cura seleccionada";
        for (int i=0;i<slots.Length;i++) slots[i].GetComponent<Image>().color = i==index ? new Color(0.6f,0.2f,0.3f,0.9f) : new Color(0.35f,0f,0.05f,0.45f);
    }
    // Tremola la casella (no es pot usar). Sempre torna a la seua posició original,
    // encara que prems la tecla moltes vegades seguides.
    public void Shake(int i)
    {
        if (shaking[i] != null) StopCoroutine(shaking[i]);
        shaking[i] = StartCoroutine(ShakeSlot(i));
    }

    System.Collections.IEnumerator ShakeSlot(int i)
    {
        RectTransform rt = slots[i];
        for (int k = 0; k < 6; k++)
        {
            rt.anchoredPosition = basePos[i] + new Vector2(k % 2 == 0 ? 5f : -5f, 0f);
            yield return new WaitForSeconds(0.03f);
        }
        rt.anchoredPosition = basePos[i];
        shaking[i] = null;
    }
}
