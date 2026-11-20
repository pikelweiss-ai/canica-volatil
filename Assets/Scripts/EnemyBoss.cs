using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Boss configurable (Rei Llim, Golem Gegant, Mag Fosc...). Entre atac i atac camina cap a tu;
// cada atac s'avisa abans (parpelleja o marca el terra en roig).
//  - Ring: bales en totes direccions        - Jump: bot amb ona de bales en caure
//  - Summon: crida esbirros                 - Spiral: bales girant
//  - Charge: envestides seguides            - RockRain: pluja de roques on estàs
//  - Teleport: desapareix i reapareix       - Fan: ventall de bales apuntat
// Els primers 'calmAttacks' atacs els fa sempre; a mitja vida es posa furiós i els fa tots.
[RequireComponent(typeof(EnemyPathfindingAI), typeof(EnemyHealth))]
public class EnemyBoss : MonoBehaviour
{
    public enum Attack { Ring, Jump, Summon, Spiral, Charge, RockRain, Teleport, Fan }

    public string bossName = "Rei Llim";
    public GameObject minionPrefab;
    public float timeBetweenAttacks = 1.4f;
    public float bulletSpeed = 5f;
    [Tooltip("Atacs del boss. Els 'calmAttacks' primers els fa des del principi; la resta, quan està furiós (mitja vida).")]
    public Attack[] attacks = { Attack.Ring, Attack.Jump, Attack.Summon, Attack.Spiral };
    public int calmAttacks = 2;
    public bool useBulletColor = false;
    public Color bulletColor = Color.white;
    public Color dustColor = new Color(0.6f, 0.9f, 0.35f);

    private EnemyPathfindingAI ai;
    private EnemyHealth health;
    private SpriteRenderer sr;
    private bool attacking;
    private float timer = 1.5f;
    private bool enraged;
    private int lastAttack = -1;
    private readonly System.Collections.Generic.List<SpriteRenderer> attackMarks = new();

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
        if (timer <= 0f) StartCoroutine(DoAttack());
    }

    IEnumerator DoAttack()
    {
        attacking = true;
        ai.mode = EnemyPathfindingAI.Mode.Hold;

        int count = Mathf.Clamp(enraged ? attacks.Length : calmAttacks, 1, attacks.Length);
        int a;
        int guard = 0;
        do { a = Random.Range(0, count); } while (a == lastAttack && count > 1 && guard++ < 10);
        lastAttack = a;

        switch (attacks[a])
        {
            case Attack.Ring: yield return Ring(); break;
            case Attack.Jump: yield return Jump(); break;
            case Attack.Summon: yield return Summon(); break;
            case Attack.Spiral: yield return Spiral(); break;
            case Attack.Charge: yield return Charge(); break;
            case Attack.RockRain: yield return RockRain(); break;
            case Attack.Teleport: yield return Teleport(); break;
            case Attack.Fan: yield return Fan(); break;
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
            GameAudio.Enemy(Fx.Pew, 0.5f);
            yield return new WaitForSeconds(0.35f);
        }
    }

    IEnumerator Jump()
    {
        if (ai.Player == null) yield break;
        Vector2 target = ai.Player.position;
        if (TileNav.Ground != null) target = TileNav.Center(TileNav.NearestFree(TileNav.Cell(target)));

        // Ombra roja on caurà
        SpriteRenderer mark = MarkAttack("Avis bot", target, 2.4f, 1.3f);
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
        RemoveMark(mark);

        // Aterratge: colp a prop + ona de bales
        Fx.Shake(0.35f, 0.35f);
        GameAudio.Enemy(Fx.Boom, 0.6f);
        Fx.Burst(target, dustColor, 16, 5f);
        if (ai.Player != null && Vector2.Distance(ai.Player.position, target) < 1.3f)
            NetCoop.DamagePlayer(ai.Player, 1);

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
        int count = 3 + Mathf.Clamp((RunState.Floor - 1) / 2, 0, 5);
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle.normalized * 1.6f;
            var dungeon = DungeonGenerator.Instance;
            if (dungeon != null)
            {
                if (!dungeon.TryChildSpawn(GetComponent<EnemyHealth>(), pos, out pos)) continue;
            }
            else if (TileNav.Ground != null) pos = TileNav.Center(TileNav.NearestFree(TileNav.Cell(pos)));
            GameObject m = Instantiate(minionPrefab, pos, Quaternion.identity);
            m.transform.localScale *= 0.7f;
            EnemySplitter split = m.GetComponent<EnemySplitter>();
            if (split != null) split.generation = split.maxGenerations; // Els fills del boss no es divideixen
            EnemyHealth h = m.GetComponent<EnemyHealth>();
            if (DungeonGenerator.Instance != null && h != null) DungeonGenerator.Instance.RegisterSpawned(h);
            m.GetComponent<EnemyPathfindingAI>()?.Alert();
            Fx.Burst(pos, dustColor, 6, 2f);
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
            if (i % 3 == 0) GameAudio.Enemy(Fx.Pew, 0.25f);
            ang += 13f;
            yield return new WaitForSeconds(0.07f);
        }
    }

    // Carrega: 3 envestides seguides cap a on estàs
    IEnumerator Charge()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        int dashes = enraged ? 4 : 3;
        for (int d = 0; d < dashes && ai.Player != null; d++)
        {
            yield return Telegraph(0.45f);
            Vector2 dir = ((Vector2)ai.Player.position - rb.position).normalized;
            ai.enabled = false;
            for (float moved = 0f; moved < 7f;)
            {
                yield return new WaitForFixedUpdate();
                float step = 13f * Time.fixedDeltaTime;
                Vector2 next = rb.position + dir * step;
                if (!TileNav.CircleFree(next, ai.bodyRadius)) { Fx.Shake(0.2f, 0.2f); break; }
                rb.MovePosition(next);
                moved += step;
                if (ai.Player != null && Vector2.Distance(next, ai.Player.position) < ai.bodyRadius + 0.4f)
                    NetCoop.DamagePlayer(ai.Player, 1);
            }
            ai.enabled = true;
            Fx.Burst(transform.position, dustColor, 8, 3f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    // Pluja de roques: marques roges al voltant teu i, després, cauen
    IEnumerator RockRain()
    {
        if (ai.Player == null) yield break;
        yield return Telegraph(0.4f);
        int n = enraged ? 10 : 7;
        var marks = new System.Collections.Generic.List<SpriteRenderer>();
        for (int i = 0; i < n; i++)
        {
            Vector2 p = (Vector2)ai.Player.position + (i == 0 ? Vector2.zero : Random.insideUnitCircle * 3f);
            marks.Add(MarkAttack("Roca avis", p, 1.2f, 0.9f));
        }
        yield return new WaitForSeconds(0.9f);
        foreach (SpriteRenderer m in marks)
        {
            Vector2 p = m.transform.position;
            RemoveMark(m);
            Fx.Burst(p, new Color(0.55f, 0.55f, 0.6f), 6, 2.5f);
            if (ai.Player != null && Vector2.Distance(ai.Player.position, p) < 0.65f)
                NetCoop.DamagePlayer(ai.Player, 1);
        }
        GameAudio.Enemy(Fx.Boom, 0.5f);
        Fx.Shake(0.3f, 0.25f);
        yield return new WaitForSeconds(0.3f);
    }

    // Desapareix, reapareix lluny de tu i dispara un anell
    IEnumerator Teleport()
    {
        for (float t = 0f; t < 0.35f; t += Time.deltaTime)
        {
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f - t / 0.35f);
            yield return null;
        }
        Fx.Burst(transform.position, dustColor, 12, 3f);
        if (ai.Player != null && TileNav.Ground != null)
            for (int i = 0; i < 30; i++)
            {
                Vector2 p = (Vector2)ai.Player.position + Random.insideUnitCircle.normalized * Random.Range(4f, 6f);
                if (!TileNav.CircleFree(p, ai.bodyRadius)) continue;
                Vector3Int c = TileNav.Cell(p);
                DungeonGenerator.Room room = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.CurrentRoom : null;
                if (room != null && !room.rect.Contains(new Vector2Int(c.x, c.y))) continue;
                transform.position = p;
                break;
            }
        for (float t = 0f; t < 0.35f; t += Time.deltaTime)
        {
            if (sr != null) sr.color = new Color(1f, 1f, 1f, t / 0.35f);
            yield return null;
        }
        if (sr != null) sr.color = Color.white;
        yield return Ring();
    }

    // Ventall de bales apuntat, tres vegades
    IEnumerator Fan()
    {
        yield return Telegraph(0.4f);
        for (int w = 0; w < 3 && ai.Player != null; w++)
        {
            Vector2 dir = (ai.Player.position - transform.position).normalized;
            int n = enraged ? 7 : 5;
            for (int i = 0; i < n; i++)
                Fire(EnemyShooter.Rotate(dir, Mathf.Lerp(-35f, 35f, i / (float)(n - 1))), 1.2f);
            GameAudio.Enemy(Fx.Pew, 0.5f);
            yield return new WaitForSeconds(0.4f);
        }
    }

    void Fire(Vector2 dir, float speedMul = 1f)
    {
        EnemyBullet.Fire((Vector2)transform.position + dir * 0.9f, dir, bulletSpeed * speedMul, 1, transform, 1.3f,
                         useBulletColor ? bulletColor : (Color?)null);
    }

    SpriteRenderer MarkAttack(string label, Vector2 position, float size, float seconds)
    {
        var mark = Fx.MakeSprite(label, position, Fx.Disc, new Color(1f, 0.2f, 0.2f, 0.35f), "Shadows", 5);
        mark.transform.localScale = Vector3.one * size;
        attackMarks.Add(mark);
        NetShared.Telegraph(position, size, seconds, health.netId);
        return mark;
    }

    void RemoveMark(SpriteRenderer mark)
    {
        attackMarks.Remove(mark);
        if (mark != null) Destroy(mark.gameObject);
    }

    void ClearAttackMarks()
    {
        foreach (var mark in attackMarks)
            if (mark != null) Destroy(mark.gameObject);
        attackMarks.Clear();
    }

    void OnDied(EnemyHealth h)
    {
        StopAllCoroutines();
        ClearAttackMarks();
        if (barRoot != null) Destroy(barRoot);
        Fx.Shake(0.6f, 0.4f);
        for (int i = 0; i < 12; i++) CoinDrop.Spawn(transform.position);
    }

    void OnDestroy()
    {
        ClearAttackMarks();
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
        barRoot.SetActive((health != null && health.netProxy) || (ai != null && ai.HasAggro)); // Online: als clients sempre es veu
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
