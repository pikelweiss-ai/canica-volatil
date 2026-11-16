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

    [Header("Quan s'obri (ací anirà el loot)")]
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
    private TextMeshPro prompt;
    private Vector3 promptStartPos;
    private bool opening;

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
            if (prompt != null) prompt.gameObject.SetActive(false);
            return;
        }

        bool canOpen = IsInRange() && IsClosestChest();
        prompt.gameObject.SetActive(canOpen);

        if (canOpen)
        {
            // La [E] puja i baixa suaument
            prompt.transform.localPosition = promptStartPos + Vector3.up * Mathf.Sin(Time.time * 4f) * 0.04f;

            if (Input.GetKeyDown(openKey))
                StartCoroutine(OpenRoutine());
        }
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

    IEnumerator OpenRoutine()
    {
        opening = true;

        // 1) Vibra cada vegada més fort, amb colps de fusta
        float t = 0f;
        float nextKnock = 0f;
        while (t < openTime)
        {
            float k = t / openTime;
            float amp = Mathf.Lerp(shakeStrength * 0.35f, shakeStrength, k);

            float x = Mathf.Sin(t * 60f) * amp;
            float angle = Mathf.Sin(t * 47f) * amp * 45f;
            visual.localPosition = visualStartPos + new Vector3(x, 0f, 0f);
            visual.localRotation = visualStartRot * Quaternion.Euler(0f, 0f, angle);

            if (t >= nextKnock)
            {
                audioSource.pitch = Random.Range(0.85f, 1.25f);
                audioSource.PlayOneShot(shakeSound, volume * Random.Range(0.6f, 1f));
                nextKnock = t + Mathf.Lerp(0.12f, 0.06f, k); // Colps més seguits al final
            }

            t += Time.deltaTime;
            yield return null;
        }

        visual.localPosition = visualStartPos;
        visual.localRotation = visualStartRot;

        // 2) S'obri: sprite obert + so
        if (spriteRenderer != null && openSprite != null)
            spriteRenderer.sprite = openSprite;

        IsOpen = true;
        opening = false;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(openSound, volume);

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
    }

    // Text "[E]" damunt del cofre
    void CreatePrompt()
    {
        GameObject go = new GameObject("Prompt E");
        go.transform.SetParent(transform, false);
        promptStartPos = new Vector3(0f, 0.65f, 0f);
        go.transform.localPosition = promptStartPos;

        prompt = go.AddComponent<TextMeshPro>();
        PlayerNameTag nameTag = FindAnyObjectByType<PlayerNameTag>();
        if (nameTag != null && nameTag.playerNameText != null)
            prompt.font = nameTag.playerNameText.font;

        prompt.text = "[" + openKey + "]";
        prompt.fontSize = 3f;
        prompt.alignment = TextAlignmentOptions.Center;
        prompt.color = Color.white;
        prompt.rectTransform.sizeDelta = new Vector2(2f, 1f);

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.sortingLayerName = "PlayerName";
        mr.sortingOrder = 5;

        go.SetActive(false);
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
