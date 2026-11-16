using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using TMPro;

public enum ChestRarity { Comu, Rar, Epic, Llegendari }

// Cofre que s'obri amb la E:
// 1) Vibra/es sacseja durant 'openTime' segons (amb soroll de colps de fusta)
// 2) Canvia a l'sprite obert, fa un botet i sona el so d'obrir
// 3) Crida 'onOpened' -> ací connectarem el loot
public class Chest : MonoBehaviour
{
    [Header("Cofre")]
    public ChestRarity rarity = ChestRarity.Comu;
    public SpriteRenderer spriteRenderer;   // Si està buit, el busca als fills
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Obrir")]
    public KeyCode openKey = KeyCode.E;
    public float interactRange = 1.2f;      // Distància màxima per a poder obrir-lo
    public float openTime = 0.75f;          // Temps que tarda a obrir-se (vibrant)

    [Header("Animació")]
    public float shakeStrength = 0.06f;     // Com de fort vibra (al final de la càrrega)
    public float jumpHeight = 0.25f;        // Altura del bot en obrir-se
    public float jumpTime = 0.25f;

    [Header("So (si està buit, es genera un so per codi)")]
    public AudioClip shakeSound;
    public AudioClip openSound;
    [Range(0f, 1f)] public float volume = 0.6f;

    [Header("Loot (n'ix un a l'atzar en obrir-lo)")]
    public WeaponData[] lootTable;
    public Vector2 lootDropOffset = new Vector2(0f, -0.8f); // On cau l'item (respecte del cofre)

    [Header("Quan s'obri")]
    public UnityEvent onOpened;

    public bool IsOpen { get; private set; }

    // Tots els cofres de l'escena: amb la E només s'obri el més pròxim
    private static readonly List<Chest> allChests = new List<Chest>();

    private Transform player;
    private SpriteRenderer playerSprite;
    private Transform visual;
    private Vector3 visualStartPos;
    private Vector3 visualStartScale;
    private Quaternion visualStartRot;
    private AudioSource audioSource;
    // Indicador: cercle blanc amb la E negra + anell que es descompta
    private Transform promptRoot;
    private LineRenderer promptRing;
    private Vector3 promptStartPos;
    private const float RingRadius = 0.27f;
    private const int RingSegments = 48;

    private bool opening;        // Fent l'animació d'obrir (bot)
    private float holdTime;      // Quant de temps portes mantenint la E
    private float nextKnock;
    private Vector3 lastPlayerPos;

    void OnEnable() { allChests.Add(this); }
    void OnDisable() { allChests.Remove(this); }

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        visual = spriteRenderer != null ? spriteRenderer.transform : transform;
        visualStartPos = visual.localPosition;
        visualStartScale = visual.localScale;
        visualStartRot = visual.localRotation;

        if (spriteRenderer != null && closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (shakeSound == null) shakeSound = ChestSounds.Knock;
        if (openSound == null) openSound = ChestSounds.Open;

        CreatePrompt();
        HideTilePreview();
    }

    // Si aquest cofre l'ha creat un ChestTile, amaguem el dibuix del tile
    // (si no, es veurien dos cofres: el del tile i aquest)
    void HideTilePreview()
    {
        Tilemap tilemap = GetComponentInParent<Tilemap>();
        if (tilemap == null) return;

        Vector3Int cell = tilemap.WorldToCell(transform.position);
        if (tilemap.GetTile(cell) is ChestTile)
            tilemap.SetColor(cell, Color.clear);
    }

    void Update()
    {
        UpdateSortingWithPlayer();

        if (IsOpen || opening || player == null)
        {
            SetPromptVisible(false);
            return;
        }

        bool canOpen = IsInRange() && IsClosestChest();
        // Estàs movent-te si prems alguna tecla de moviment o si la canica s'ha desplaçat.
        // (Només amb la posició no basta: el moviment va per passos de física i hi ha
        //  fotogrames en què no canvia, i això feia sonar els colps com una metralleta.)
        bool movingInput = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
                           Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
        bool playerMoved = movingInput || (player.position - lastPlayerPos).sqrMagnitude > 0.0004f;
        lastPlayerPos = player.position;

        SetPromptVisible(canOpen);
        if (!canOpen)
        {
            CancelHold();
            return;
        }

        // L'indicador puja i baixa suaument
        promptRoot.localPosition = promptStartPos + Vector3.up * Mathf.Sin(Time.time * 4f) * 0.04f;

        // Cal MANTINDRE la E sense moure's: mentrestant el cofre vibra
        if (Input.GetKey(openKey) && !playerMoved)
        {
            holdTime += Time.deltaTime;
            ShakeStep();

            if (holdTime >= openTime)
            {
                ResetVisual();
                StartCoroutine(OpenFinishRoutine());
                return;
            }
        }
        else
        {
            CancelHold(); // Has deixat anar la E o t'has mogut: torna a començar
        }

        SetRing(1f - holdTime / openTime);
    }

    // Vibració que creix mentre mantens la E, amb colps de fusta cada vegada més seguits
    void ShakeStep()
    {
        float k = Mathf.Clamp01(holdTime / openTime);
        float amp = Mathf.Lerp(shakeStrength * 0.35f, shakeStrength, k);

        float x = Mathf.Sin(holdTime * 60f) * amp;
        float angle = Mathf.Sin(holdTime * 47f) * amp * 45f;
        visual.localPosition = visualStartPos + new Vector3(x, 0f, 0f);
        visual.localRotation = visualStartRot * Quaternion.Euler(0f, 0f, angle);

        if (holdTime >= nextKnock)
        {
            audioSource.pitch = Random.Range(0.85f, 1.25f);
            audioSource.PlayOneShot(shakeSound, volume * Random.Range(0.6f, 1f));
            nextKnock = holdTime + Mathf.Lerp(0.12f, 0.06f, k);
        }
    }

    void CancelHold()
    {
        if (holdTime <= 0f) return;
        holdTime = 0f;
        nextKnock = 0f;
        ResetVisual();
    }

    void ResetVisual()
    {
        visual.localPosition = visualStartPos;
        visual.localRotation = visualStartRot;
        visual.localScale = visualStartScale;
    }

    // Si la canica està més amunt que el cofre (darrere), el cofre es dibuixa davant;
    // si està més avall (davant), el cofre es dibuixa darrere d'ella.
    void UpdateSortingWithPlayer()
    {
        if (player == null || spriteRenderer == null) return;
        if (playerSprite == null) playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite == null) return;

        spriteRenderer.sortingLayerID = playerSprite.sortingLayerID;
        spriteRenderer.sortingOrder = player.position.y > transform.position.y
            ? playerSprite.sortingOrder + 1
            : playerSprite.sortingOrder - 1;
    }

    bool IsInRange()
    {
        return Vector2.Distance(player.position, transform.position) <= interactRange;
    }

    bool IsClosestChest()
    {
        float myDistance = Vector2.Distance(player.position, transform.position);
        foreach (Chest other in allChests)
        {
            if (other == this || other.IsOpen || other.opening) continue;
            if (Vector2.Distance(player.position, other.transform.position) < myDistance)
                return false;
        }
        return true;
    }

    // Ja has mantingut la E prou temps: s'obri, sona i bota
    IEnumerator OpenFinishRoutine()
    {
        opening = true;
        SetPromptVisible(false);
        float t;

        // S'obri: sprite obert + so
        if (spriteRenderer != null && openSprite != null)
            spriteRenderer.sprite = openSprite;

        IsOpen = true;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(openSound, volume);

        DropLoot();

        if (onOpened != null)
            onOpened.Invoke();

        // 3) Botet: s'estira en pujar...
        t = 0f;
        while (t < jumpTime)
        {
            float k = t / jumpTime;
            float s = Mathf.Sin(k * Mathf.PI);
            visual.localPosition = visualStartPos + Vector3.up * s * jumpHeight;
            visual.localScale = Vector3.Scale(visualStartScale, new Vector3(1f - 0.08f * s, 1f + 0.12f * s, 1f));
            t += Time.deltaTime;
            yield return null;
        }

        // ...i s'aixafa un poc en caure
        visual.localPosition = visualStartPos;
        const float squashTime = 0.1f;
        t = 0f;
        while (t < squashTime)
        {
            float s = 1f - t / squashTime;
            visual.localScale = Vector3.Scale(visualStartScale, new Vector3(1f + 0.15f * s, 1f - 0.15f * s, 1f));
            t += Time.deltaTime;
            yield return null;
        }
        visual.localScale = visualStartScale;
        opening = false;
    }

    // Trau un item a l'atzar de la taula de loot i el llança fora del cofre
    void DropLoot()
    {
        if (lootTable == null || lootTable.Length == 0) return;

        WeaponData weapon = lootTable[Random.Range(0, lootTable.Length)];
        if (weapon == null) return;

        Vector3 from = transform.position + Vector3.up * 0.1f;
        Vector3 to = transform.position + (Vector3)lootDropOffset;
        ItemPickup.Spawn(weapon, from, to);
    }

    // ---------- Indicador: cercle blanc + E negra + anell de temps ----------

    void CreatePrompt()
    {
        GameObject root = new GameObject("Prompt E");
        root.transform.SetParent(transform, false);
        promptStartPos = new Vector3(0f, 0.72f, 0f);
        root.transform.localPosition = promptStartPos;
        promptRoot = root.transform;

        // Cercle blanc
        GameObject disc = new GameObject("Cercle");
        disc.transform.SetParent(promptRoot, false);
        SpriteRenderer discSr = disc.AddComponent<SpriteRenderer>();
        discSr.sprite = PromptGraphics.Disc;
        discSr.color = Color.white;
        discSr.sortingLayerName = "PlayerName";
        discSr.sortingOrder = 5;

        // Lletra E en negre
        GameObject textGo = new GameObject("Lletra");
        textGo.transform.SetParent(promptRoot, false);
        TextMeshPro text = textGo.AddComponent<TextMeshPro>();
        PlayerNameTag nameTag = FindAnyObjectByType<PlayerNameTag>();
        if (nameTag != null && nameTag.playerNameText != null)
            text.font = nameTag.playerNameText.font;
        text.text = openKey.ToString();
        text.fontSize = 2.8f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.rectTransform.sizeDelta = new Vector2(1f, 1f);
        MeshRenderer textMr = textGo.GetComponent<MeshRenderer>();
        textMr.sortingLayerName = "PlayerName";
        textMr.sortingOrder = 7;

        // Anell exterior (es descompta mentre mantens la E)
        GameObject ringGo = new GameObject("Anell");
        ringGo.transform.SetParent(promptRoot, false);
        promptRing = ringGo.AddComponent<LineRenderer>();
        promptRing.useWorldSpace = false;
        promptRing.startWidth = promptRing.endWidth = 0.045f;
        promptRing.startColor = promptRing.endColor = Color.white;
        promptRing.material = PromptGraphics.LineMaterial;
        promptRing.numCapVertices = 2;
        promptRing.sortingLayerName = "PlayerName";
        promptRing.sortingOrder = 6;
        SetRing(1f);

        root.SetActive(false);
    }

    void SetPromptVisible(bool visible)
    {
        if (promptRoot != null && promptRoot.gameObject.activeSelf != visible)
            promptRoot.gameObject.SetActive(visible);
    }

    // fraction = 1 -> anell sencer, 0 -> buit. Es buida en sentit horari des de dalt.
    void SetRing(float fraction)
    {
        if (promptRing == null) return;
        fraction = Mathf.Clamp01(fraction);

        int points = Mathf.Max(2, Mathf.CeilToInt(RingSegments * fraction) + 1);
        promptRing.positionCount = points;
        promptRing.enabled = fraction > 0.001f;

        for (int i = 0; i < points; i++)
        {
            float a = (90f - 360f * fraction * i / (points - 1)) * Mathf.Deg2Rad;
            promptRing.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * RingRadius);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}

// Sons generats per codi (estil retro), per si no poses cap AudioClip
public static class ChestSounds
{
    const int SampleRate = 44100;
    private static AudioClip knock;
    private static AudioClip open;

    // Colp curt de fusta
    public static AudioClip Knock
    {
        get
        {
            if (knock == null)
            {
                int n = (int)(SampleRate * 0.05f);
                float[] data = new float[n];
                System.Random rnd = new System.Random(7);
                float lowPass = 0f;
                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / SampleRate;
                    float env = Mathf.Exp(-t * 90f);
                    float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                    lowPass += (noise - lowPass) * 0.25f;
                    float tone = Mathf.Sin(2f * Mathf.PI * 170f * t);
                    data[i] = (lowPass * 0.7f + tone * 0.5f) * env * 0.7f;
                }
                knock = AudioClip.Create("ChestKnock", n, 1, SampleRate, false);
                knock.SetData(data, 0);
            }
            return knock;
        }
    }

    // "Pop" + arpegi pujant (do-mi-sol-do) d'obrir el cofre
    public static AudioClip Open
    {
        get
        {
            if (open == null)
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                const float noteTime = 0.07f;
                const float lastNoteTime = 0.3f;
                const float popTime = 0.04f;
                float total = popTime + noteTime * (notes.Length - 1) + lastNoteTime;
                int n = (int)(SampleRate * total);
                float[] data = new float[n];
                System.Random rnd = new System.Random(3);

                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / SampleRate;
                    float v = 0f;

                    // Pop inicial (soroll curt)
                    if (t < popTime)
                        v += (float)(rnd.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 80f) * 0.5f;

                    // Arpegi
                    float nt = t - popTime;
                    if (nt >= 0f)
                    {
                        int idx = Mathf.Min((int)(nt / noteTime), notes.Length - 1);
                        float local = nt - idx * noteTime;
                        float decay = idx == notes.Length - 1 ? 6f : 18f;
                        float phase = 2f * Mathf.PI * notes[idx] * local;
                        float square = Mathf.Sign(Mathf.Sin(phase));
                        float wave = Mathf.Sin(phase) * 0.6f + square * 0.25f;
                        v += wave * Mathf.Exp(-local * decay) * 0.35f;
                    }

                    data[i] = Mathf.Clamp(v, -1f, 1f);
                }

                open = AudioClip.Create("ChestOpen", n, 1, SampleRate, false);
                open.SetData(data, 0);
            }
            return open;
        }
    }
}

// Gràfics generats per codi per a l'indicador de la E
public static class PromptGraphics
{
    private static Sprite disc;
    private static Material lineMaterial;

    // Cercle blanc suau de 0.42 unitats de diàmetre
    public static Sprite Disc
    {
        get
        {
            if (disc == null)
            {
                const int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                float r = size / 2f;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                        float a = Mathf.Clamp01(r - d); // Vora suavitzada d'1 píxel
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                }
                tex.Apply();
                disc = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 0.42f);
            }
            return disc;
        }
    }

    public static Material LineMaterial
    {
        get
        {
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                lineMaterial = new Material(shader);
            }
            return lineMaterial;
        }
    }
}