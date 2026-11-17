using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Efectes compartits generats per codi: sprites rodons de píxel, sons retro,
// números de dany, explosions i tremolor de càmera.
public static class Fx
{
    const int SampleRate = 44100;

    // ---------- Sprites ----------

    static readonly Dictionary<int, Sprite> circles = new Dictionary<int, Sprite>();

    // Cercle de píxel art (blanc amb vora fosca d'1 píxel) de 'size' píxels, a 16 PPU
    public static Sprite PixelCircle(int size = 6)
    {
        if (circles.TryGetValue(size, out Sprite s) && s != null) return s;
        Texture2D tex = new Texture2D(size, size) { filterMode = FilterMode.Point };
        float r = size / 2f;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                Color c = d <= r - 1f ? Color.white : d <= r ? new Color(0.12f, 0.12f, 0.14f) : Color.clear;
                tex.SetPixel(x, y, c);
            }
        tex.Apply();
        s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        circles[size] = s;
        return s;
    }

    // Disc pla (sense vora) gran, per a explosions i zones
    static Sprite disc;
    public static Sprite Disc
    {
        get
        {
            if (disc == null)
            {
                const int size = 32;
                Texture2D tex = new Texture2D(size, size) { filterMode = FilterMode.Point };
                float r = size / 2f;
                for (int x = 0; x < size; x++)
                    for (int y = 0; y < size; y++)
                        tex.SetPixel(x, y, Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) <= r ? Color.white : Color.clear);
                tex.Apply();
                disc = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            }
            return disc;
        }
    }

    public static SpriteRenderer MakeSprite(string name, Vector3 pos, Sprite sprite, Color color, string layer = "Player", int order = 5)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        go.layer = LayerMask.NameToLayer("Ignore Raycast");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
        return sr;
    }

    // ---------- Càmera ----------

    static CameraFollow cam;
    public static void Shake(float duration, float magnitude)
    {
        if (cam == null) cam = Object.FindAnyObjectByType<CameraFollow>();
        if (cam != null) cam.ShakeCamera(duration, magnitude);
    }

    // ---------- Números de dany ----------

    public static void DamageNumber(Vector3 pos, int amount, Color color)
    {
        GameObject go = new GameObject("Dany");
        go.transform.position = pos + new Vector3(Random.Range(-0.15f, 0.15f), 0.4f, 0f);
        TextMeshPro t = go.AddComponent<TextMeshPro>();
        t.text = amount.ToString();
        t.fontSize = 4f;
        t.alignment = TextAlignmentOptions.Center;
        t.color = color;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(30, 30, 36, 255);
        t.sortingLayerID = SortingLayer.NameToID("PlayerName");
        t.sortingOrder = 20;
        Runner.Run(FloatUp(go.transform, t));
    }

    static IEnumerator FloatUp(Transform tr, TextMeshPro t)
    {
        float time = 0f;
        Vector3 start = tr.position;
        while (time < 0.6f && tr != null)
        {
            float k = time / 0.6f;
            tr.position = start + Vector3.up * (0.5f * Mathf.Sin(k * Mathf.PI * 0.5f));
            tr.localScale = Vector3.one * (k < 0.15f ? Mathf.Lerp(1.6f, 1f, k / 0.15f) : 1f);
            Color c = t.color; c.a = 1f - Mathf.Clamp01((k - 0.6f) / 0.4f); t.color = c;
            time += Time.deltaTime;
            yield return null;
        }
        if (tr != null) Object.Destroy(tr.gameObject);
    }

    // ---------- Explosions ----------

    // Fa mal en un radi: al jugador, als enemics i als objectes trencables
    public static void Explode(Vector2 pos, float radius, int playerDamage, int enemyDamage, Transform ignore = null)
    {
        SpriteRenderer sr = MakeSprite("Explosio", pos, Disc, new Color(1f, 0.85f, 0.3f, 0.9f), "Player", 10);
        Runner.Run(ExplosionAnim(sr, radius));
        Play(Boom, 0.8f);
        Shake(0.25f, 0.25f);

        var done = new HashSet<Object>();
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(pos, radius))
        {
            if (ignore != null && hit.transform.IsChildOf(ignore)) continue;
            Vector2 push = ((Vector2)hit.transform.position - pos).normalized * 6f;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null && done.Add(enemy)) { enemy.TakeHit(enemyDamage, push); continue; }

            PlayerHealth player = hit.GetComponentInParent<PlayerHealth>();
            if (player != null && done.Add(player)) { if (playerDamage > 0) player.TakeDamage(playerDamage); continue; }

            Breakable b = hit.GetComponentInParent<Breakable>();
            if (b != null && done.Add(b)) b.Hit(enemyDamage);
        }
    }

    static IEnumerator ExplosionAnim(SpriteRenderer sr, float radius)
    {
        float t = 0f;
        const float dur = 0.3f;
        while (t < dur && sr != null)
        {
            float k = t / dur;
            sr.transform.localScale = Vector3.one * radius * 2f * Mathf.Lerp(0.4f, 1.1f, Mathf.Sqrt(k));
            sr.color = Color.Lerp(new Color(1f, 0.95f, 0.6f, 0.95f), new Color(1f, 0.35f, 0.1f, 0f), k);
            t += Time.deltaTime;
            yield return null;
        }
        if (sr != null) Object.Destroy(sr.gameObject);
    }

    // Esclat de partícules quadrades (en morir, trencar caixes...)
    public static void Burst(Vector2 pos, Color color, int count = 8, float speed = 3f)
    {
        for (int i = 0; i < count; i++)
        {
            SpriteRenderer sr = MakeSprite("Particula", pos, PixelCircle(3), color, "Player", 6);
            Vector2 v = Random.insideUnitCircle.normalized * Random.Range(speed * 0.5f, speed);
            Runner.Run(Particle(sr, v));
        }
    }

    static IEnumerator Particle(SpriteRenderer sr, Vector2 v)
    {
        float t = 0f, dur = Random.Range(0.25f, 0.45f);
        while (t < dur && sr != null)
        {
            sr.transform.position += (Vector3)(v * Time.deltaTime);
            v *= 0.9f;
            Color c = sr.color; c.a = 1f - t / dur; sr.color = c;
            t += Time.deltaTime;
            yield return null;
        }
        if (sr != null) Object.Destroy(sr.gameObject);
    }

    // ---------- Sons ----------

    public static void Play(AudioClip clip, float volume = 0.6f) => WeaponSounds.Play(clip, volume);

    static AudioClip boom, pew, heal, door, coin;

    public static AudioClip Boom => boom != null ? boom : boom = Make("Boom", 0.5f, (t, n) => (n * Mathf.Exp(-t * 7f) * 0.9f + Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(90f, 30f, t * 2f) * t) * Mathf.Exp(-t * 6f)) * 0.8f, true);
    public static AudioClip Pew => pew != null ? pew : pew = Make("Pew", 0.15f, (t, n) => Square(Mathf.Lerp(900f, 300f, t / 0.15f) * t) * Mathf.Exp(-t * 18f) * 0.35f);
    public static AudioClip Heal => heal != null ? heal : heal = Make("Cura", 0.35f, (t, n) => Mathf.Sin(2f * Mathf.PI * (t < 0.12f ? 660f : 990f) * t) * Mathf.Exp(-((t < 0.12f ? t : t - 0.12f)) * 9f) * 0.4f);
    public static AudioClip Door => door != null ? door : door = Make("Porta", 0.25f, (t, n) => (Square(70f * t) * 0.5f + n * 0.3f) * Mathf.Exp(-t * 14f) * 0.6f, true);
    public static AudioClip Coin => coin != null ? coin : coin = Make("Moneda", 0.18f, (t, n) => Square((t < 0.06f ? 1320f : 1760f) * t) * Mathf.Exp(-t * 12f) * 0.25f);

    static float Square(float phase) => (phase % 1f) < 0.5f ? 1f : -1f;

    static AudioClip Make(string name, float dur, System.Func<float, float, float> f, bool lowpass = false)
    {
        int n = (int)(SampleRate * dur);
        float[] d = new float[n];
        System.Random rnd = new System.Random(name.GetHashCode());
        float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
            if (lowpass) { lp += (noise - lp) * 0.15f; noise = lp * 2f; }
            d[i] = Mathf.Clamp(f((float)i / SampleRate, noise), -1f, 1f);
        }
        AudioClip c = AudioClip.Create(name, n, 1, SampleRate, false);
        c.SetData(d, 0);
        return c;
    }
}

// Objecte invisible per a executar corrutines des de codi estàtic
public class Runner : MonoBehaviour
{
    static Runner instance;

    public static Coroutine Run(IEnumerator routine)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("Fx Runner");
            instance = go.AddComponent<Runner>();
        }
        return instance.StartCoroutine(routine);
    }
}
