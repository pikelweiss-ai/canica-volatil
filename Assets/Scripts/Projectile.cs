using UnityEngine;

// Projectil que va en línia recta, fa mal al primer enemic que toca
// i desapareix en xocar amb una paret o en arribar a la distància màxima.
public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private int damage;
    private float knockbackForce;
    private float radius;
    private Transform owner;
    private Vector2 startPosition;
    private Vector2 firingPosition;
    private WeaponData weapon;
    // Online: bala d'un company. Es veu igual pero no fa mal (el dany el compta el PC de qui dispara)
    [System.NonSerialized] public bool cosmetic;

    // endPoint: on acaba la línia d'apuntat. El projectil s'atura quan la seua PUNTA hi arriba.
    public void Init(Vector2 dir, WeaponData weapon, Transform shooter, Vector2? endPoint = null)
    {
        this.weapon = weapon;
        direction = dir.normalized;
        speed = weapon.projectileSpeed;
        damage = weapon.damage;
        knockbackForce = weapon.knockbackForce;
        radius = weapon.projectileSize * 0.5f;
        owner = shooter;
        startPosition = transform.position;
        firingPosition = shooter != null ? (Vector2)shooter.position : startPosition;
        pierceLeft = weapon.pierce;
        wallBouncesLeft = weapon.wallBounces;

        // Gira el projectil cap a on va (els sprites apunten cap a la dreta)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (endPoint.HasValue)
        {
            // Distància del centre del projectil a la seua punta
            float frontOffset = 0f;
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                frontOffset = sr.sprite.bounds.extents.x * Mathf.Abs(transform.lossyScale.x);

            maxDistance = Mathf.Max(0f, Vector2.Distance(startPosition, endPoint.Value) - frontOffset);
        }
        else
        {
            maxDistance = weapon.range;
        }
    }

    static readonly System.Collections.Generic.List<RaycastHit2D> hits = new();
    static readonly ContactFilter2D AllContacts = new ContactFilter2D().NoFilter();

    static bool CanHit(Collider2D hit, Transform shooter)
    {
        if (shooter != null && hit.transform.IsChildOf(shooter)) return false;
        if (NetCoop.Online && (hit.GetComponentInParent<PlayerHealth>() != null || hit.GetComponentInParent<RemoteAvatar>() != null))
            return false;
        var enemy = hit.GetComponentInParent<EnemyHealth>();
        if (enemy != null) return enemy.CanBeTargeted;
        return !hit.isTrigger || hit.GetComponentInParent<Breakable>() != null;
    }

    public static Vector2 SafeMuzzle(Vector2 center, Vector2 muzzle, float radius, Transform shooter)
    {
        Vector2 offset = muzzle - center;
        Physics2D.CircleCast(center, radius, offset.normalized, AllContacts, hits, offset.magnitude);
        foreach (var hit in hits)
            if (CanHit(hit.collider, shooter)) return center;
        return muzzle;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Gira el dibuix (shuriken, bumerang...)
        if (weapon.spinSpeed != 0f)
            transform.Rotate(0f, 0f, weapon.spinSpeed * dt);

        // Bumerang tornant: vola cap al jugador travessant-ho tot i pegant de nou
        if (returning)
        {
            if (owner == null) { End(); return; }
            Vector2 to = (Vector2)owner.position - (Vector2)transform.position;
            if (to.magnitude < 0.4f) { Destroy(gameObject); return; }
            direction = to.normalized;
            transform.position += (Vector3)(direction * speed * 1.2f * dt);
            HitEnemiesOnly();
            return;
        }

        // Teledirigit: gira poc a poc cap a l'enemic més pròxim de davant
        if (weapon.homing > 0f)
        {
            EnemyHealth target = NearestEnemy();
            if (target != null)
            {
                Vector2 want = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                direction = Vector3.RotateTowards(direction, want, weapon.homing * dt, 0f).normalized;
                if (weapon.spinSpeed == 0f)
                    transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }

        // Avança, però sense passar-se del final (l'últim pas es retalla just al final)
        float step = speed * dt;
        bool reachesEnd = travelled + step >= maxDistance;
        if (reachesEnd) step = Mathf.Max(0f, maxDistance - travelled);
        Vector2 start = transform.position;
        Physics2D.CircleCast(start, radius, direction, AllContacts, hits, step);
        foreach (var contact in hits)
        {
            Collider2D hit = contact.collider;
            if (!CanHit(hit, owner)) continue;
            transform.position = start + direction * contact.distance;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                if (alreadyHit.Contains(enemy)) continue; // Fletxa que travessa: no pega dues vegades
                alreadyHit.Add(enemy);
                if (!cosmetic) HitEnemy(enemy);
                if (pierceLeft-- > 0 || weapon.returns) continue;
                End();
                return;
            }

            // Caixes i barrils
            Breakable breakable = hit.GetComponentInParent<Breakable>();
            if (breakable != null)
            {
                if (!cosmetic) breakable.Hit(ImpactDamage());
                if (weapon.returns) { StartReturn(); return; }
                End();
                return;
            }

            // Collider sòlid (paret, arbre...) -> el projectil s'atura (el bumerang torna)
            if (!hit.isTrigger)
            {
                if (weapon.returns) { StartReturn(); return; }
                if (wallBouncesLeft > 0 && Vector2.Dot(direction, contact.normal) < 0f)
                {
                    wallBouncesLeft--; bouncesDone++;
                    maxDistance += weapon.range * weapon.bounceRangeBonus; // Disc: cada rebot allarga el recorregut un 15 %
                    travelled += contact.distance;
                    transform.position += (Vector3)(contact.normal * 0.01f);
                    direction = Vector2.Reflect(direction, contact.normal).normalized;
                    return;
                }
                End();
                return;
            }
        }

        transform.position = start + direction * step;
        travelled += step;

        // Arriba al final de l'aim (la pedra cau a terra; el bumerang torna)
        if (reachesEnd)
        {
            if (weapon.returns) StartReturn();
            else End();
        }
    }

    int ImpactDamage()
    {
        float range = weapon.closeDamageRange;
        int bounceBonus = bouncesDone * weapon.bounceDamage; // Disc: +1 a cada rebot
        return bounceBonus + (range > 0f && ((Vector2)transform.position - firingPosition).sqrMagnitude < range * range
            ? damage + 1 : damage);
    }

    void StartReturn()
    {
        returning = true;
        alreadyHit.Clear(); // En tornar pot pegar una altra vegada als mateixos
    }

    void HitEnemiesOnly()
    {
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, radius))
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null || !enemy.CanBeTargeted || alreadyHit.Contains(enemy)) continue;
            alreadyHit.Add(enemy);
            if (!cosmetic) HitEnemy(enemy);
        }
    }

    // Colp d'un projectil d'aquest jugador: el resum de partida apunta l'arma
    void HitEnemy(EnemyHealth enemy)
    {
        RunStats.HitBy = weapon.weaponName;
        enemy.TakeProjectileHit(ImpactDamage(), direction * knockbackForce, weapon.projectileEffect);
        RunStats.HitBy = null;
    }

    EnemyHealth NearestEnemy()
    {
        EnemyHealth best = null;
        float bestD = 6f;
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, 6f))
        {
            EnemyHealth e = hit.GetComponentInParent<EnemyHealth>();
            if (e == null || e.IsDead || !e.CanBeTargeted || alreadyHit.Contains(e)) continue;
            Vector2 to = (Vector2)e.transform.position - (Vector2)transform.position;
            if (Vector2.Dot(to.normalized, direction) < 0.2f) continue; // Només els de davant
            if (to.magnitude < bestD) { bestD = to.magnitude; best = e; }
        }
        return best;
    }

    private bool returning;
    private float travelled;

    // El projectil s'acaba: so d'impacte (si l'arma en té) i desapareix
    void End()
    {
        if (ended) return;
        ended = true;
        if (weapon != null)
        {
            WeaponSounds.Play(WeaponSounds.ImpactClip(weapon), 0.6f);
            if (weapon.explosionRadius > 0f)
                { if (cosmetic) Fx.ExplodeVisual(transform.position, weapon.explosionRadius); else { RunStats.HitBy = weapon.weaponName; Fx.Explode(transform.position, weapon.explosionRadius, 0, weapon.explosionDamage, owner); RunStats.HitBy = null; } } // No et fa mal a tu
        }
        Destroy(gameObject);
    }

    private bool ended;
    private int pierceLeft;
    private int wallBouncesLeft, bouncesDone;
    private readonly System.Collections.Generic.List<EnemyHealth> alreadyHit = new System.Collections.Generic.List<EnemyHealth>();

    // ---- Boleta generada per codi, per a armes sense prefab ----

    private static Sprite circleSprite, flameSprite, boltSprite;

    static Sprite PixelShape(params string[] rows)
    {
        int width = rows[0].Length, height = rows.Length;
        var texture = new Texture2D(width, height) { filterMode = FilterMode.Point };
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                texture.SetPixel(x, height - y - 1, rows[y][x] == '#' ? Color.white : Color.clear);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), width);
    }

    public static Sprite CircleSprite
    {
        get
        {
            if (circleSprite == null)
            {
                const int size = 32;
                Texture2D tex = new Texture2D(size, size);
                tex.filterMode = FilterMode.Point;
                float r = size / 2f;
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                        tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
                    }
                }
                tex.Apply();
                circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            }
            return circleSprite;
        }
    }

    public static Projectile Spawn(Vector2 position, Vector2 dir, WeaponData weapon, Transform shooter, int sortingLayerID, int sortingOrder, Vector2? endPoint = null)
    {
        GameObject go;

        if (weapon.projectilePrefab != null)
        {
            go = Instantiate(weapon.projectilePrefab, position, Quaternion.identity);
        }
        else
        {
            go = new GameObject("Projectil " + weapon.weaponName);
            go.transform.position = position;
            go.layer = LayerMask.NameToLayer("Ignore Raycast");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            if (weapon.projectileSprite != null)
            {
                // Sprite propi (bala, pedra...): Init el gira cap a on va
                sr.sprite = weapon.projectileSprite;
                if (weapon.tintProjectileSprite) sr.color = weapon.projectileColor;
                go.transform.localScale = Vector3.one * weapon.projectileSpriteScale;
            }
            else
            {
                // Sense sprite: boleta de color
                sr.sprite = weapon.maxShots > 0
                    ? (flameSprite ??= PixelShape("...##...", "..####..", "..######", ".#######", "########", "..#####.", "...###.."))
                    : weapon.projectileEffect == ProjectileEffect.Chain
                        ? (boltSprite ??= PixelShape("...####.", "..####..", ".####...", "######..", "..###...", ".##.....", "##......"))
                        : CircleSprite;
                sr.color = weapon.projectileColor;
                go.transform.localScale = Vector3.one * weapon.projectileSize;
            }
            sr.sortingLayerID = sortingLayerID;
            sr.sortingOrder = sortingOrder;
        }

        Projectile p = go.GetComponent<Projectile>();
        if (p == null)
            p = go.AddComponent<Projectile>();

        p.Init(dir, weapon, shooter, endPoint);
        return p;
    }
}
