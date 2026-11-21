using UnityEngine;

// v1.4.1: zona de perill temporal dels enemics nous (cercle de foc, toll d'àcid, bomba).
// Es crea a TOTS els equips (missatge ZoneFx) i cada equip aplica l'efecte al SEU jugador;
// els altres enemics (explosió de la bomba) els toca només l'amfitrió, que és qui mana al combat.
public class HazardZone : MonoBehaviour
{
    public enum Style : byte { Fire = 0, Acid = 1, Bomb = 2 }

    Style style;
    float radius, delay, life, age, nextTick;
    SpriteRenderer disc, ring;
    bool exploded;
    PlayerMovement local;

    // Dades de cada tipus (taula del disseny §1: Salamandra, Llimac àcid, Bombardier)
    static void Stats(Style s, out float radius, out float delay, out float life)
    {
        switch (s)
        {
            case Style.Fire: radius = 1.5f; delay = 0f; life = 3f; break;   // 1 de mal/s durant 3 s
            case Style.Acid: radius = 0.9f; delay = 0f; life = 6f; break;   // -30 % de velocitat
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
        Color c = s == Style.Fire ? new Color(1f, .45f, .1f) : s == Style.Acid ? new Color(.45f, .9f, .2f) : new Color(1f, .15f, .15f);
        disc = Fx.MakeSprite("Zona terra", transform.position, Fx.Disc, new Color(c.r, c.g, c.b, s == Style.Bomb ? .18f : .38f), "Shadows", 3);
        disc.transform.SetParent(transform, true); disc.transform.localScale = Vector3.one * radius * 2f;
        ring = Fx.MakeSprite("Zona vora", transform.position, Fx.Disc, new Color(c.r, c.g, c.b, .55f), "Shadows", 4);
        ring.transform.SetParent(transform, true); ring.transform.localScale = Vector3.one * radius * 2f * (s == Style.Bomb ? .2f : .9f);
        if (s == Style.Fire) MobSounds.PlayLocal("salamandra_flama", transform.position, .35f);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= delay + life) { Destroy(gameObject); return; }

        if (style == Style.Bomb)
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
        if (style == Style.Acid) local.BoostSpeed(0.7f, 0.2f);
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
        if (NetCoop.IsClient) return;
        foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
            if (enemy != null && !enemy.IsDead && Vector2.Distance(enemy.transform.position, transform.position) <= radius)
                enemy.TakeDamage(3);
    }
}
