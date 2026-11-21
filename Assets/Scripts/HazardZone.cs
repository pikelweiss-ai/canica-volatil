using UnityEngine;

// v1.4.1: zona de perill temporal dels enemics nous (cercle de foc, toll d'àcid, bomba).
// Es crea a TOTS els equips (missatge ZoneFx) i cada equip aplica l'efecte al SEU jugador;
// els altres enemics (explosió de la bomba) els toca només l'amfitrió, que és qui mana al combat.
public class HazardZone : MonoBehaviour
{
    public enum Style : byte { Fire = 0, Acid = 1, Bomb = 2, Web = 3, DirtyWater = 4, RisingLava = 5, Stove = 6, BossBomb = 7 }

    Style style;
    float radius, delay, life, age, nextTick;
    SpriteRenderer disc, ring;
    bool IsBomb => style == Style.Bomb || style == Style.BossBomb;
    bool exploded;
    PlayerMovement local;

    // Teranyina circular (radis i anells) dibuixada per codi, 32x32 píxels
    static Sprite web;
    public static Sprite WebSprite
    {
        get
        {
            if (web != null) return web;
            const int size = 32; var tex = new Texture2D(size, size) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var clear = new Color[size * size]; tex.SetPixels(clear);
            float c = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c, dy = y - c, d = Mathf.Sqrt(dx * dx + dy * dy); if (d > 15.5f || d < 1f) continue;
                    float angle = Mathf.Atan2(dy, dx) * 8f / (2f * Mathf.PI);              // 8 radis
                    bool spoke = Mathf.Abs(angle - Mathf.Round(angle)) * d < .55f;
                    bool ring = Mathf.Abs(d % 5f - 2.5f) > 2.1f && d > 4f;                  // anells cada 5 píxels
                    if (spoke || ring) tex.SetPixel(x, y, Color.white);
                }
            tex.Apply(); web = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size); return web;
        }
    }

    // Dades de cada tipus (taula del disseny §1: Salamandra, Llimac àcid, Bombardier)
    static void Stats(Style s, out float radius, out float delay, out float life)
    {
        switch (s)
        {
            case Style.Stove:
            case Style.Fire: radius = 1.5f; delay = 0f; life = 3f; break;   // 1 de mal/s durant 3 s
            case Style.Acid: radius = 0.9f; delay = 0f; life = 6f; break;   // -30 % de velocitat
            case Style.Web: radius = 1.7f; delay = 0f; life = 6f; break;    // teranyina de la Reina Aranya: velocitat x0,4
            case Style.DirtyWater: radius = 7f; delay = 0f; life = 1.18f; break;
            case Style.RisingLava: radius = 2f; delay = .7f; life = 5f; break;
            default: radius = 1.5f; delay = 1.2f; life = 1.4f; break;       // cercle roig 1,2 s i després 3 de mal
        }
    }

    // L'amfitrió (o la partida en solitari) crea la zona i la fa arribar als clients.
    public static void Spawn(Style s, Vector2 pos)
    {
        Create(s, pos);
        NetCoop.ZoneSpawned((byte)s, pos);
    }

    // Comú per a l'amfitrió i per als clients (NetCoop ho crida en rebre ZoneFx)
    public static void Create(Style s, Vector2 pos)
    {
        var go = new GameObject("Zona " + s);
        go.transform.position = pos;
        go.AddComponent<HazardZone>().Setup(s);
    }

    void Setup(Style s)
    {
        style = s; Stats(s, out radius, out delay, out life);
        Color c = (s == Style.Fire || s == Style.Stove) ? new Color(1f, .45f, .1f) : s == Style.Acid ? new Color(.45f, .9f, .2f) : s == Style.DirtyWater ? new Color(.29f, .37f, .17f) : s == Style.RisingLava ? new Color(1f, .25f, .04f) : s == Style.Web ? new Color(.92f, .92f, .98f) : new Color(1f, .15f, .15f);
        disc = Fx.MakeSprite("Zona terra", transform.position, s == Style.Web ? WebSprite : Fx.Disc, new Color(c.r, c.g, c.b, IsBomb ? .18f : s == Style.Web ? .9f : .38f), "Shadows", 3);
        disc.transform.SetParent(transform, true); disc.transform.localScale = Vector3.one * radius * 2f;
        ring = Fx.MakeSprite("Zona vora", transform.position, Fx.Disc, new Color(c.r, c.g, c.b, .55f), "Shadows", 4);
        ring.transform.SetParent(transform, true); ring.transform.localScale = Vector3.one * radius * 2f * (IsBomb ? .2f : .9f);
        if (s == Style.Stove)
        {
            var stove = Resources.Load<Sprite>("Pixel/Bosses/Fogons_encesos_centre");
            if (stove != null)
            {
                var glowing = Fx.MakeSprite("Fogons encesos", transform.position, stove, Color.white, "Shadows", 6);
                glowing.transform.SetParent(transform, true);
            }
        }
        if (s == Style.Fire || s == Style.Stove) MobSounds.PlayLocal("salamandra_flama", transform.position, .35f);
        if (s == Style.Web)
        {   // la teranyina es pot trencar a colps o amb trets (3 impactes); en trencar-se desapareix per a tots
            var hit = gameObject.AddComponent<CircleCollider2D>(); hit.isTrigger = true; hit.radius = 1.1f;
            var breakable = gameObject.AddComponent<Breakable>(); breakable.health = 3; breakable.coinChance = 0f; breakable.healChance = 0f; breakable.debrisColor = new Color(.92f, .92f, .98f);
        }
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= delay + life) { Destroy(gameObject); return; }

        if (IsBomb)
        {
            if (age < delay)
            {   // el cercle roig es va omplint fins al moment de l'explosió
                ring.transform.localScale = Vector3.one * radius * 2f * Mathf.Lerp(.2f, 1f, age / delay);
                return;
            }
            if (!exploded) Explode();
            var cc = ring.color; cc.a = Mathf.Lerp(.7f, 0f, (age - delay) / life); ring.color = cc;
            return;
        }

        // Foc i àcid: efecte continu sobre el jugador d'este equip
        if (local == null) local = FindFirstObjectByType<PlayerMovement>();
        if (local == null) return;
        if (Vector2.Distance(local.transform.position, transform.position) > radius) return;
        if (age < delay) return;
        if (style == Style.DirtyWater) local.BoostSpeed(.8f, .2f);
        else if (style == Style.Acid) local.BoostSpeed(0.7f, 0.2f);
        else if (style == Style.Web) local.BoostSpeed(0.3f, 0.2f);                 // xafar-la frena molt (x0,3)
        else if (Time.time >= nextTick)
        {
            nextTick = Time.time + 1f;
            var health = local.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1);
        }
    }

    void Explode()
    {
        exploded = true;
        Fx.ExplodeVisual(transform.position, radius);
        MobSounds.PlayLocal("bombardier_explosio", transform.position, .5f);
        if (local == null) local = FindFirstObjectByType<PlayerMovement>();
        if (local != null && Vector2.Distance(local.transform.position, transform.position) <= radius)
        {
            var health = local.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(3);
        }
        // Les bombes també fan mal als altres enemics (decideix l'amfitrió)
        if (NetCoop.IsClient || style == Style.BossBomb) return;
        foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
            if (enemy != null && !enemy.IsDead && Vector2.Distance(enemy.transform.position, transform.position) <= radius)
                enemy.TakeDamage(3);
    }
}
