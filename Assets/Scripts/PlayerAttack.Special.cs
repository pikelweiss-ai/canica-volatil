using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Alpha v1.3.1: armes amb comportament propi.
//  - Bàcul de torreta: deixa una torreta que dispara sola.      - Llançamines: deixa mines que exploten.
//  - Arc de llum: mantín per carregar, solta per disparar.        - Escut de mirall: mantín el clic per parar i reflectir bales.
//  - Vareta curativa: cura el company que apuntes.                - Ganxo: estira un enemic o t'estira cap a una paret.
// Els efectes (torreta, mines, ganxo) són visibles per a qui les usa; el mal passa per EnemyHealth i, per tant, també online.
public partial class PlayerAttack
{
    float chargeTime;
    TurretUnit turret;
    readonly List<MineUnit> mines = new List<MineUnit>();
    static readonly Dictionary<int, WeaponData> chargedShots = new Dictionary<int, WeaponData>();
    GameObject shieldVisual;
    WeaponData reflectShot;

    // ------------------------------------------------------------ Armes que mantenen el botó
    // Torna true si el tick d'atac ja l'ha gestionat (Arc de llum).
    bool TickSpecialHeld(bool attackPressed)
    {
        WeaponData w = CurrentWeapon;
        if (w == null || w.special == SpecialKind.None) { chargeTime = 0f; HideShield(); return false; }

        if (w.special == SpecialKind.Charge)
        {
            HideShield();
            float full = Mathf.Max(0.3f, w.specialValue);
            if (attackPressed && slotCooldown[selectedSlot] <= 0f)
                chargeTime = Mathf.Min(full, chargeTime + Time.deltaTime);
            else if (chargeTime > 0f)
            {
                float level = chargeTime / full; chargeTime = 0f;
                if (level >= 0.15f) FireCharged(w, level);
            }
            return true;
        }

        chargeTime = 0f;
        if (w.special == SpecialKind.Shield) { UpdateShield(w, attackPressed); return true; } // l escut no colpeja: v1.4.1, DPS a l àrea
        HideShield();
        return false;
    }

    // De 2 a 8 de dany segons la càrrega (la flecha travessa enemics, com diu l'arma)
    void FireCharged(WeaponData w, float level)
    {
        int damage = Mathf.RoundToInt(Mathf.Lerp(2f, 8f, Mathf.Clamp01(level)));
        if (!chargedShots.TryGetValue(damage, out var shot) || shot == null)
        {
            shot = Instantiate(w); shot.name = w.name; shot.damage = damage; shot.special = SpecialKind.None;
            chargedShots[damage] = shot;
        }
        Shoot(shot);
        slotCooldown[selectedSlot] = w.cooldown;
        if (level >= 0.95f) Fx.Text(transform.position, "Càrrega total", new Color(1f, 0.9f, 0.4f));
    }

    // L'aim ensenya la càrrega mentre la mantens
    bool ChargeProgress(WeaponData w, out float progress)
    {
        progress = 0f;
        if (w == null || w.special != SpecialKind.Charge || chargeTime <= 0f) return false;
        progress = chargeTime / Mathf.Max(0.3f, w.specialValue);
        return true;
    }

    // ------------------------------------------------------------ Armes d'un sol ús
    void UseInstant(WeaponData w)
    {
        switch (w.special)
        {
            case SpecialKind.Turret: PlaceTurret(w); break;
            case SpecialKind.Mine: PlaceMine(w); break;
            case SpecialKind.Grapple: FireGrapple(w); break;
        }
        WeaponSounds.Play(WeaponSounds.ShootClip(w), 0.4f);
        recoil = 0.08f;
    }

    Vector3 SpotInFront(float distance) => TileNav.DropDestination(transform.position, transform.position + (Vector3)(aimDirection * distance));

    // v1.4.1: a quina distància cau l'objecte de les armes de col·locar (l'aim l'usa per a acabar ahí); 0 = no és de col·locar
    const float TurretDistance = 1.2f, MineDistance = 0.9f;
    static float PlaceDistance(WeaponData w) =>
        w == null ? 0f : w.special == SpecialKind.Turret ? TurretDistance : w.special == SpecialKind.Mine ? MineDistance : 0f;

    void PlaceTurret(WeaponData w)
    {
        if (turret != null) turret.Expire();
        var go = new GameObject("Torreta");
        go.transform.position = SpotInFront(TurretDistance);
        turret = go.AddComponent<TurretUnit>();
        turret.Setup(w, transform, Mathf.Max(3f, w.specialValue));
    }

    void PlaceMine(WeaponData w)
    {
        mines.RemoveAll(m => m == null);
        while (mines.Count >= 3) { mines[0].Expire(); mines.RemoveAt(0); }
        var go = new GameObject("Mina");
        go.transform.position = SpotInFront(MineDistance);
        var mine = go.AddComponent<MineUnit>();
        mine.Setup(w, transform, w.specialValue > 0f ? w.specialValue : 1.5f);
        mines.Add(mine);
    }

    void FireGrapple(WeaponData w)
    {
        Vector2 origin = transform.position;
        Vector2 end = origin + aimDirection * w.range;
        foreach (var hit in Physics2D.CircleCastAll(origin, 0.25f, aimDirection, w.range))
        {
            Collider2D c = hit.collider;
            if (c == null || c.transform.IsChildOf(transform)) continue;
            EnemyHealth enemy = c.GetComponentInParent<EnemyHealth>();
            if (enemy != null && !enemy.IsDead)
            {
                end = hit.point;
                RunStats.HitBy = w.weaponName; enemy.TakeHit(w.damage, aimDirection * 2f); RunStats.HitBy = null;
                // El gegant (boss) no es mou; els enemics de l'amfitrió es mouen sols: només se'n va el mal
                if (enemy.GetComponentInParent<EnemyBoss>() == null && !enemy.IsNetProxy)
                    StartCoroutine(PullEnemy(enemy, Mathf.Max(0.5f, w.specialValue)));
                break;
            }
            if (c.isTrigger) continue;
            end = hit.point;
            float distance = Mathf.Max(0f, hit.distance - 0.6f);
            if (distance > 0.5f) GetComponent<PlayerMovement>()?.Dash(aimDirection, distance); // et llances cap a la paret
            break;
        }
        StartCoroutine(RopeFlash(origin, end, w.specialSprite));
    }

    IEnumerator PullEnemy(EnemyHealth enemy, float stopDistance)
    {
        var body = enemy != null ? enemy.GetComponent<Rigidbody2D>() : null;
        for (float t = 0f; t < 0.25f && enemy != null && !enemy.IsDead; t += Time.deltaTime)
        {
            Vector2 target = (Vector2)transform.position + aimDirection * stopDistance;
            Vector2 now = enemy.transform.position;
            Vector2 next = Vector2.MoveTowards(now, target, 22f * Time.deltaTime);
            if (TileNav.Ready() && !TileNav.LineOfSight(now, next, 0.2f)) break;
            if (body != null) body.position = next; else enemy.transform.position = next;
            yield return null;
        }
    }

    // Corda (i harpó, si l'arma en té) que s'allarga fins al punt tocat
    IEnumerator RopeFlash(Vector2 from, Vector2 to, Sprite hook = null)
    {
        Vector2 delta = to - from; float length = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        var rope = Fx.MakeSprite("Corda del ganxo", from, Fx.Disc, new Color(0.85f, 0.75f, 0.5f), "Player", 9);
        rope.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        SpriteRenderer head = null;
        if (hook != null)
        {
            head = Fx.MakeSprite("Harpo", from, hook, Color.white, "Player", 10);
            head.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        const float flight = 0.1f;
        for (float t = 0f; t < flight; t += Time.deltaTime)
        {
            float k = t / flight; Vector2 tip = from + delta * k;
            if (rope != null) { rope.transform.position = (from + tip) * 0.5f; rope.transform.localScale = new Vector3(delta.magnitude * k, 0.08f, 1f); }
            if (head != null) head.transform.position = tip;
            yield return null;
        }
        if (rope != null) { rope.transform.position = (from + to) * 0.5f; rope.transform.localScale = new Vector3(length, 0.08f, 1f); }
        if (head != null) head.transform.position = to;
        yield return new WaitForSeconds(0.12f);
        if (rope != null) Destroy(rope.gameObject);
        if (head != null) Destroy(head.gameObject);
    }

    // Vareta curativa: si apuntes a un company ferit, el cures 1 cor (només online)
    void TryHealAlly(WeaponData w)
    {
        var session = NetSession.Instance;
        if (session == null || !NetCoop.Online) return;
        float range = w.specialValue > 0f ? w.specialValue : 7f;
        NetPlayer best = null; float bestDistance = float.MaxValue;
        foreach (var p in session.Players.Values)
        {
            if (p.id == session.LocalId || p.maxHealth <= 0 || p.health >= p.maxHealth || NetCoop.IsDown(p.id)) continue;
            if (p.scene != NetSession.SceneToken() || Time.realtimeSinceStartupAsDouble - p.lastPos > 2) continue;
            Vector2 delta = new Vector2(p.x, p.y) - (Vector2)transform.position;
            if (delta.magnitude > range || Vector2.Angle(aimDirection, delta) > 35f) continue;
            if (TileNav.Ready() && !TileNav.LineOfSight(transform.position, new Vector2(p.x, p.y), 0f, true)) continue;
            if (delta.magnitude < bestDistance) { bestDistance = delta.magnitude; best = p; }
        }
        if (best == null) return;
        NetCoop.HealAlly(best.id);
        StartCoroutine(RopeFlash(transform.position, new Vector2(best.x, best.y)));
    }

    // ------------------------------------------------------------ Escut de mirall
    // v1.4.1: l'escut té una barra d'energia (5 s seguits). Alçat, gasta; quan no, es recarrega al mateix ritme (1:1) i no
    // es recarrega mentre està alçat. Si s'esgota, no es pot tornar a alçar fins que torna al 20 %.
    // Qui entra a l'àrea de l'escut rep 3 de mal per segon (1 cada 0,33 s). Només es veu UN escut, més gran.
    public const float ShieldMaxEnergy = 5f;
    float shieldEnergy = ShieldMaxEnergy, shieldTick;
    bool shieldLocked, shieldRaised;
    public float ShieldEnergyFraction => shieldEnergy / ShieldMaxEnergy;
    bool shieldShown;
    public bool ShieldIsUp => shieldShown;

    void TickShieldEnergy()
    {
        if (!shieldRaised) shieldEnergy = Mathf.Min(ShieldMaxEnergy, shieldEnergy + Time.deltaTime);
        if (shieldLocked && shieldEnergy >= ShieldMaxEnergy * 0.2f) shieldLocked = false;
        shieldRaised = false; // UpdateShield el torna a marcar si continua alçat
    }

    void UpdateShield(WeaponData w, bool pressed)
    {
        if (pressed && !shieldLocked && shieldEnergy > 0f)
        {
            shieldEnergy = Mathf.Max(0f, shieldEnergy - Time.deltaTime);
            if (shieldEnergy <= 0f) { shieldLocked = true; Fx.Text(transform.position, "Escut esgotat", new Color(1f, .5f, .4f)); HideShield(); return; }
        }
        else { if (pressed && shieldEnergy <= 0f) shieldLocked = true; HideShield(); return; }
        shieldRaised = true; shieldShown = true;

        if (shieldVisual == null)
        {
            var sr = Fx.MakeSprite("Escut de mirall", transform.position, w.icon != null ? w.icon : Fx.Disc, Color.white, "Player", 9);
            sr.transform.localScale = Vector3.one * 1.1f; // un sol escut, més gran
            shieldVisual = sr.gameObject;
        }
        shieldVisual.SetActive(true);
        Vector2 centre = (Vector2)transform.position + aimDirection * 0.85f;
        shieldVisual.transform.position = centre;
        var visual = shieldVisual.GetComponent<SpriteRenderer>();
        visual.flipY = aimDirection.x < 0f;
        shieldVisual.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg);
        // Capes: darrere del jugador si apuntes cap amunt, davant en la resta (igual que l'arma a la mà)
        if (playerSprite != null)
        {
            visual.sortingLayerID = playerSprite.sortingLayerID;
            visual.sortingOrder = aimDirection.y > 0.35f ? playerSprite.sortingOrder - 1 : playerSprite.sortingOrder + 1;
        }
        GetComponent<PlayerMovement>()?.BoostSpeed(0.7f, 0.1f); // amb l'escut alçat vas més a poc a poc

        float arc = w.specialValue > 0f ? w.specialValue : 120f;
        foreach (var bullet in FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None))
        {
            Vector2 delta = (Vector2)bullet.transform.position - (Vector2)transform.position;
            if (delta.magnitude > 1.5f || Vector2.Angle(aimDirection, delta) > arc * 0.5f) continue;
            ReflectBullet(bullet);
        }

        // 3 de mal per segon als enemics de dins l'àrea (també als bosses)
        shieldTick -= Time.deltaTime;
        if (shieldTick > 0f) return;
        shieldTick = 1f / 3f;
        var seen = new HashSet<EnemyHealth>();
        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, 1.5f))
        {
            var enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null || enemy.IsDead || !seen.Add(enemy)) continue;
            Vector2 delta = (Vector2)enemy.transform.position - (Vector2)transform.position;
            if (Vector2.Angle(aimDirection, delta) > arc * 0.5f) continue;
            enemy.TakeHit(1, aimDirection * 1.5f);
        }
    }

    void ReflectBullet(EnemyBullet bullet)
    {
        Vector2 position = bullet.transform.position;
        Vector2 direction = (position - (Vector2)transform.position).normalized;
        Destroy(bullet.gameObject);
        if (reflectShot == null) reflectShot = NetCoop.Weapon("Pistola");
        if (reflectShot != null)
        {
            int layer = playerSprite != null ? playerSprite.sortingLayerID : 0;
            int order = playerSprite != null ? playerSprite.sortingOrder + 1 : 0;
            Projectile.Spawn(position, direction, reflectShot, transform, layer, order, position + direction * 8f);
        }
        Fx.Burst(position, new Color(0.7f, 0.9f, 1f), 6, 2f);
    }

    void HideShield() { shieldShown = false; if (shieldVisual != null && shieldVisual.activeSelf) shieldVisual.SetActive(false); }
}

// ---------------------------------------------------------------- Torreta
public class TurretUnit : MonoBehaviour
{
    WeaponData weapon; Transform owner;
    float life, nextShot;
    SpriteRenderer body, light;

    SpriteRenderer barrel;

    public void Setup(WeaponData w, Transform ownerTransform, float seconds)
    {
        weapon = w; owner = ownerTransform; life = seconds;
        if (w.specialSprite != null)
        {
            // Sprite propi: cúpula fixa i canó que gira cap a l'enemic
            body = Fx.MakeSprite("Torreta cos", transform.position, w.specialSprite, Color.white, "Player", 6);
            body.transform.SetParent(transform, true);
            if (w.specialSprite2 != null)
            {
                barrel = Fx.MakeSprite("Torreta cano", transform.position + Vector3.up * 0.05f, w.specialSprite2, Color.white, "Player", 7);
                barrel.transform.SetParent(transform, true);
            }
        }
        else
        {
            body = Fx.MakeSprite("Torreta cos", transform.position, Fx.Disc, new Color(0.55f, 0.6f, 0.68f), "Player", 6);
            body.transform.SetParent(transform, true); body.transform.localScale = Vector3.one * 0.55f;
            light = Fx.MakeSprite("Torreta llum", transform.position, Fx.Disc, new Color(1f, 0.3f, 0.3f), "Player", 7);
            light.transform.SetParent(transform, true); light.transform.localScale = Vector3.one * 0.2f;
        }
        Fx.Burst(transform.position, new Color(0.7f, 0.75f, 0.85f), 8, 2f);
    }

    public void Expire()
    {
        Fx.Burst(transform.position, new Color(0.7f, 0.75f, 0.85f), 6, 2f);
        Destroy(gameObject);
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) { Expire(); return; }
        if (Time.time < nextShot) return;

        EnemyHealth target = null; float best = 7f * 7f;
        foreach (var c in Physics2D.OverlapCircleAll(transform.position, 7f))
        {
            var enemy = c.GetComponentInParent<EnemyHealth>();
            if (enemy == null || enemy.IsDead) continue;
            float d = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d >= best) continue;
            if (TileNav.Ready() && !TileNav.LineOfSight(transform.position, enemy.transform.position, 0f, true)) continue;
            best = d; target = enemy;
        }
        if (target == null) return;

        nextShot = Time.time + 0.4f;
        Vector2 from = transform.position, to = target.transform.position;
        RunStats.HitBy = weapon.weaponName;
        target.TakeHit(Mathf.Max(1, weapon.damage), (to - from).normalized * 1.5f);
        RunStats.HitBy = null;
        StartCoroutine(Tracer(from, to));
        Vector2 aim = (to - from).normalized;
        if (light != null) light.transform.position = from + aim * 0.18f;
        if (barrel != null)
        {
            barrel.transform.position = (Vector3)(from + aim * 0.32f) + Vector3.up * 0.05f;
            barrel.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
        }
    }

    IEnumerator Tracer(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        var beam = Fx.MakeSprite("Tret de torreta", (from + to) * 0.5f, Fx.Disc, new Color(1f, 0.85f, 0.4f), "Player", 8);
        beam.transform.localScale = new Vector3(delta.magnitude, 0.07f, 1f);
        beam.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        yield return new WaitForSeconds(0.08f);
        if (beam != null) Destroy(beam.gameObject);
    }
}

// ---------------------------------------------------------------- Mina
public class MineUnit : MonoBehaviour
{
    WeaponData weapon; Transform owner;
    float radius, armedAt, life = 25f;
    SpriteRenderer body, lamp;

    public void Setup(WeaponData w, Transform ownerTransform, float blastRadius)
    {
        weapon = w; owner = ownerTransform; radius = blastRadius; armedAt = Time.time + 0.7f;
        if (w.specialSprite != null)
        {
            body = Fx.MakeSprite("Mina cos", transform.position, w.specialSprite, Color.white, "Player", 5);
            body.transform.SetParent(transform, true);
            if (w.specialSprite2 != null)
            {
                lamp = Fx.MakeSprite("Mina llum", transform.position + Vector3.up * 0.02f, w.specialSprite2, Color.white, "Player", 6);
                lamp.transform.SetParent(transform, true);
                lamp.enabled = false; // només parpelleja quan la mina està armada
            }
        }
        else
        {
            body = Fx.MakeSprite("Mina cos", transform.position, Fx.Disc, new Color(0.35f, 0.25f, 0.25f), "Player", 5);
            body.transform.SetParent(transform, true); body.transform.localScale = Vector3.one * 0.38f;
        }
    }

    public void Expire() { if (this != null) Destroy(gameObject); }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) { Expire(); return; }
        bool armed = Time.time >= armedAt;
        bool blink = armed && ((int)(Time.time * 4f) % 2 == 0);
        if (lamp != null) lamp.enabled = blink;
        else if (body != null) body.color = blink ? new Color(1f, 0.2f, 0.2f) : new Color(0.35f, 0.25f, 0.25f);
        if (!armed) return;

        foreach (var c in Physics2D.OverlapCircleAll(transform.position, 0.9f))
        {
            var enemy = c.GetComponentInParent<EnemyHealth>();
            var secretWall = c.GetComponentInParent<Breakable>();
            if ((enemy == null || enemy.IsDead) && (secretWall == null || !secretWall.meleeOrMineOnly)) continue;
            RunStats.HitBy = weapon.weaponName;
            Fx.Explode(transform.position, radius, 0, Mathf.Max(1, weapon.damage), owner, mine: true);
            RunStats.HitBy = null;
            Destroy(gameObject);
            return;
        }
    }
}
