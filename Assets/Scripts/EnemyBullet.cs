using UnityEngine;

// Bala dels enemics: va recta, fa mal al jugador i es trenca contra parets i objectes sòlids.
public class EnemyBullet : MonoBehaviour
{
    public enum Appearance : byte { Default, Rat, Pan }
    public static Sprite sprite; // L'assigna la masmorra (BalaEnemiga.png); si no, cercle roig

    public static void ClearAll()
    {
        foreach (var bullet in FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None))
            if (bullet != null) Destroy(bullet.gameObject);
    }

    private Vector2 velocity;
    private int damage;
    bool slows; // v1.4.1: boles de gel (color blau pàl·lid): en tocar-te et ralentixen ~2 s
    bool overHazardousLiquid;
    private float life;
    private Transform owner;

    public static EnemyBullet Fire(Vector2 pos, Vector2 dir, float speed, int damage, Transform owner, float scale = 1f, Color? tint = null, bool overHazardousLiquid = false, float flightTime = 6f, Appearance appearance = Appearance.Default)
    {
        Sprite s = sprite != null ? sprite : Fx.PixelCircle(6);
        // Amb color: cercle d'aquest color (bales màgiques, pedres...); sense: la bala roja
        if (tint.HasValue) s = Fx.PixelCircle(6);
        Color c = tint ?? (sprite != null ? Color.white : new Color(1f, 0.3f, 0.4f));
        if (appearance == Appearance.Pan)
            s = Resources.Load<Sprite>("Pixel/Bosses/Paella_centre") ?? s;
        else if (appearance == Appearance.Rat)
            s = RatSprite;
        SpriteRenderer sr = Fx.MakeSprite("Bala enemiga", pos, s, c, "Player", 8);
        sr.transform.localScale = Vector3.one * scale;
        EnemyBullet b = sr.gameObject.AddComponent<EnemyBullet>();
        b.velocity = dir.normalized * speed;
        b.damage = damage;
        b.slows = tint.HasValue && Mathf.Abs(tint.Value.r - .65f) < .03f && Mathf.Abs(tint.Value.g - .9f) < .03f && Mathf.Abs(tint.Value.b - 1f) < .03f;
        b.owner = owner;
        b.overHazardousLiquid = overHazardousLiquid;
        b.life = flightTime;
        b.appearance = appearance;
        if (appearance == Appearance.Rat) sr.flipX = dir.x < 0f;
        NetCoop.EnemyBulletFired(pos, b.velocity, damage, scale, tint, overHazardousLiquid, flightTime, appearance);
        return b;
    }

    Appearance appearance;
    static Sprite ratSprite;
    static Sprite RatSprite
    {
        get
        {
            if (ratSprite != null) return ratSprite;
            string[] pixels =
            {
                "......#..#..",
                ".....#o##o#.",
                "....#oooooo#",
                "..##ooooo#o#",
                "##ooooooo##.",
                "..##oooo##..",
                "...##..##..."
            };
            var tex = new Texture2D(pixels[0].Length, pixels.Length) { filterMode = FilterMode.Point };
            for (int y = 0; y < pixels.Length; y++)
                for (int x = 0; x < pixels[y].Length; x++)
                    tex.SetPixel(x, pixels.Length - 1 - y, pixels[y][x] == '#' ? new Color(.14f, .1f, .12f) :
                        pixels[y][x] == 'o' ? new Color(.57f, .43f, .43f) : Color.clear);
            tex.Apply();
            ratSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * .5f, 16);
            return ratSprite;
        }
    }

    void FixedUpdate()
    {
        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
        if (appearance == Appearance.Pan) transform.Rotate(0f, 0f, 700f * Time.fixedDeltaTime);
        life -= Time.fixedDeltaTime;
        if (life <= 0f) { Destroy(gameObject); return; }

        if (TileNav.Ground != null && TileNav.IsBlocked(TileNav.Cell(transform.position), ignoreWater: true, overHazardousLiquid: overHazardousLiquid)) { Pop(); return; }

        if (NetCoop.AllyShieldBlocksLocal(transform.position, velocity)) { Pop(); return; }

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, 0.12f * transform.localScale.x))
        {
            if (owner != null && hit.transform.IsChildOf(owner)) continue;

            PlayerHealth p = hit.GetComponentInParent<PlayerHealth>();
            if (p != null)
            {
                p.TakeDamage(damage);
                if (slows) p.GetComponent<PlayerMovement>()?.BoostSpeed(0.6f, 2f);
                Pop();
                return;
            }
            if (hit.GetComponentInParent<EnemyHealth>() != null) continue;     // Travessa els altres enemics
            var tilemap = hit.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap != null && TileNav.CanShootOverLiquid(tilemap, tilemap.WorldToCell(transform.position), overHazardousLiquid)) continue;
            if (!hit.isTrigger) { Pop(); return; }                             // Paret, cofre, caixa...
        }
    }

    void Pop()
    {
        Fx.Burst(transform.position, new Color(1f, 0.4f, 0.5f), 4, 1.5f);
        Destroy(gameObject);
    }
}
