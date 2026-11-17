using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Boss "Rei Llim". Entre atac i atac camina cap a tu; cada atac s'avisa abans.
//  - Anell: dispara bales en totes direccions
//  - Bot: marca on caurà (ombra roja), salta i en caure fa una ona de bales
//  - Invocació (a mitja vida): crida llims xicotets
//  - Espiral (a mitja vida): bales girant
// A mitja vida es posa furiós: més ràpid i més bales.
[RequireComponent(typeof(EnemyPathfindingAI), typeof(EnemyHealth))]
public class EnemyBoss : MonoBehaviour
{
    public string bossName = "Rei Llim";
    public GameObject minionPrefab;
    public float timeBetweenAttacks = 1.4f;
    public float bulletSpeed = 5f;

    private EnemyPathfindingAI ai;
    private EnemyHealth health;
    private SpriteRenderer sr;
    private bool attacking;
    private float timer = 1.5f;
    private bool enraged;
    private int lastAttack = -1;

    // Barra de vida
    private GameObject barRoot;
    private Image barFill;

    void Start()
    {
        ai = GetComponent<EnemyPathfindingAI>();
        health = GetComponent<EnemyHealth>();
        sr = GetComponent<SpriteRenderer>();
        ai.mode = EnemyPathfindingAI.Mode.Chase;
        ai.detectionRange = 30f;
        health.Died += OnDied;
        CreateBar();
    }

    void Update()
    {
        UpdateBar();
        if (attacking || ai.Player == null || !ai.HasAggro) return;

        if (!enraged && health.currentHealth <= health.maxHealth / 2)
        {
            enraged = true;
            ai.moveSpeed *= 1.3f;
            timeBetweenAttacks *= 0.7f;
            Fx.Shake(0.4f, 0.3f);
        }

        timer -= Time.deltaTime;
        if (timer <= 0f) StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        attacking = true;
        ai.mode = EnemyPathfindingAI.Mode.Hold;

        int count = enraged ? 4 : 2;
        int a;
        do { a = Random.Range(0, count); } while (a == lastAttack && count > 1);
        lastAttack = a;

        switch (a)
        {
            case 0: yield return Ring(); break;
            case 1: yield return Jump(); break;
            case 2: yield return Summon(); break;
            case 3: yield return Spiral(); break;
        }

        ai.mode = EnemyPathfindingAI.Mode.Chase;
        timer = timeBetweenAttacks;
        attacking = false;
    }

    // Parpelleig d'avís abans de l'atac
    IEnumerator Telegraph(float time)
    {
        float t = 0f;
        while (t < time)
        {
            if (sr != null) sr.color = Mathf.FloorToInt(t * 14f) % 2 == 0 ? Color.white : new Color(1f, 0.9f, 0.5f);
            t += Time.deltaTime;
            yield return null;
        }
        if (sr != null) sr.color = Color.white;
    }

    IEnumerator Ring()
    {
        yield return Telegraph(0.5f);
        int waves = enraged ? 3 : 2;
        int n = enraged ? 16 : 12;
        for (int w = 0; w < waves; w++)
        {
            float off = w * 180f / n;
            for (int i = 0; i < n; i++)
            {
                float ang = (off + i * 360f / n) * Mathf.Deg2Rad;
                Fire(new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)));
            }
            Fx.Play(Fx.Pew, 0.5f);
            yield return new WaitForSeconds(0.35f);
        }
    }

    IEnumerator Jump()
    {
        if (ai.Player == null) yield break;
        Vector2 target = ai.Player.position;
        if (TileNav.Ground != null) target = TileNav.Center(TileNav.NearestFree(TileNav.Cell(target)));

        // Ombra roja on caurà
        SpriteRenderer mark = Fx.MakeSprite("Avis bot", target, Fx.Disc, new Color(1f, 0.2f, 0.2f, 0.35f), "Shadows", 5);
        mark.transform.localScale = Vector3.one * 2.4f;
        yield return Telegraph(0.6f);

        Vector2 from = transform.position;
        Vector3 baseScale = transform.localScale;
        float t = 0f;
        const float dur = 0.7f;
        while (t < dur)
        {
            float k = t / dur;
            transform.position = Vector2.Lerp(from, target, k) + Vector2.up * Mathf.Sin(k * Mathf.PI) * 2f;
            transform.localScale = baseScale * (1f + 0.2f * Mathf.Sin(k * Mathf.PI));
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        transform.localScale = baseScale;
        Destroy(mark.gameObject);

        // Aterratge: colp a prop + ona de bales
        Fx.Shake(0.35f, 0.35f);
        Fx.Play(Fx.Boom, 0.6f);
        Fx.Burst(target, new Color(0.6f, 0.9f, 0.35f), 16, 5f);
        if (ai.Player != null && Vector2.Distance(ai.Player.position, target) < 1.3f)
            ai.Player.GetComponent<PlayerHealth>()?.TakeDamage(1);

        int n = enraged ? 20 : 14;
        for (int i = 0; i < n; i++)
        {
            float ang = i * 360f / n * Mathf.Deg2Rad;
            Fire(new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)), 0.8f);
        }
        yield return new WaitForSeconds(0.4f);
    }

    IEnumerator Summon()
    {
        if (minionPrefab == null) { yield return Ring(); yield break; }
        yield return Telegraph(0.6f);
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle.normalized * 1.6f;
            if (TileNav.Ground != null) pos = TileNav.Center(TileNav.NearestFree(TileNav.Cell(pos)));
            GameObject m = Instantiate(minionPrefab, pos, Quaternion.identity);
            m.transform.localScale *= 0.7f;
            EnemySplitter split = m.GetComponent<EnemySplitter>();
            if (split != null) split.generation = split.maxGenerations; // Els fills del boss no es divideixen
            EnemyHealth h = m.GetComponent<EnemyHealth>();
            if (DungeonGenerator.Instance != null && h != null) DungeonGenerator.Instance.RegisterSpawned(h);
            m.GetComponent<EnemyPathfindingAI>()?.Alert();
            Fx.Burst(pos, new Color(0.6f, 0.9f, 0.35f), 6, 2f);
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator Spiral()
    {
        yield return Telegraph(0.4f);
        float ang = 0f;
        for (int i = 0; i < 36; i++)
        {
            for (int arm = 0; arm < 3; arm++)
            {
                float a = (ang + arm * 120f) * Mathf.Deg2Rad;
                Fire(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), 0.9f);
            }
            if (i % 3 == 0) Fx.Play(Fx.Pew, 0.25f);
            ang += 13f;
            yield return new WaitForSeconds(0.07f);
        }
    }

    void Fire(Vector2 dir, float speedMul = 1f)
    {
        EnemyBullet.Fire((Vector2)transform.position + dir * 0.9f, dir, bulletSpeed * speedMul, 1, transform, 1.3f);
    }

    void OnDied(EnemyHealth h)
    {
        if (barRoot != null) Destroy(barRoot);
        Fx.Shake(0.6f, 0.4f);
        for (int i = 0; i < 12; i++) CoinDrop.Spawn(transform.position);
    }

    void OnDestroy()
    {
        if (barRoot != null) Destroy(barRoot);
    }

    // ---------- Barra de vida a dalt de la pantalla ----------

    void CreateBar()
    {
        barRoot = new GameObject("Barra Boss", typeof(Canvas), typeof(CanvasScaler));
        Canvas c = barRoot.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 40;
        CanvasScaler sc = barRoot.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1600, 900);

        Image back = NewImage("Fons", barRoot.transform, new Color(0.12f, 0.12f, 0.14f, 0.9f));
        RectTransform br = back.rectTransform;
        br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
        br.pivot = new Vector2(0.5f, 1f);
        br.anchoredPosition = new Vector2(0f, -60f);
        br.sizeDelta = new Vector2(620f, 30f);

        barFill = NewImage("Vida", back.transform, new Color(0.55f, 0.85f, 0.3f));
        RectTransform fr = barFill.rectTransform;
        fr.anchorMin = new Vector2(0f, 0f);
        fr.anchorMax = new Vector2(1f, 1f);
        fr.offsetMin = new Vector2(4f, 4f);
        fr.offsetMax = new Vector2(-4f, -4f);
        fr.pivot = new Vector2(0f, 0.5f);

        GameObject tgo = new GameObject("Nom", typeof(RectTransform), typeof(TextMeshProUGUI));
        tgo.transform.SetParent(back.transform, false);
        TextMeshProUGUI t = tgo.GetComponent<TextMeshProUGUI>();
        t.text = bossName;
        t.fontSize = 26;
        t.alignment = TextAlignmentOptions.Center;
        t.outlineWidth = 0.25f;
        RectTransform tr = t.rectTransform;
        tr.anchorMin = new Vector2(0f, 1f);
        tr.anchorMax = new Vector2(1f, 1f);
        tr.pivot = new Vector2(0.5f, 0f);
        tr.anchoredPosition = new Vector2(0f, 4f);
        tr.sizeDelta = new Vector2(0f, 36f);
    }

    void UpdateBar()
    {
        if (barFill == null || health == null) return;
        float k = Mathf.Clamp01((float)health.currentHealth / health.maxHealth);
        barFill.rectTransform.localScale = new Vector3(k, 1f, 1f);
        barFill.color = enraged ? new Color(0.95f, 0.35f, 0.3f) : new Color(0.55f, 0.85f, 0.3f);
        barRoot.SetActive(ai != null && ai.HasAggro);
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image i = go.GetComponent<Image>();
        i.color = color;
        return i;
    }
}
