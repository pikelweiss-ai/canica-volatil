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
    private WeaponData weapon;

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

    void FixedUpdate()
    {
        // Avança, però sense passar-se del final (l'últim pas es retalla just al final)
        float travelled = Vector2.Distance(startPosition, transform.position);
        float step = speed * Time.fixedDeltaTime;
        bool reachesEnd = travelled + step >= maxDistance;

        if (reachesEnd)
            transform.position = startPosition + direction * maxDistance;
        else
            transform.position += (Vector3)(direction * step);

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, radius))
        {
            // No ens fem mal a nosaltres mateixos
            if (owner != null && hit.transform.IsChildOf(owner))
                continue;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeHit(damage, direction * knockbackForce);
                End();
                return;
            }

            // Collider sòlid (paret, arbre...) -> el projectil s'atura
            if (!hit.isTrigger)
            {
                End();
                return;
            }
        }

        // Arriba al final de l'aim (la pedra cau a terra)
        if (reachesEnd)
            End();
    }

    // El projectil s'acaba: so d'impacte (si l'arma en té) i desapareix
    void End()
    {
        if (weapon != null)
            WeaponSounds.Play(WeaponSounds.ImpactClip(weapon), 0.6f);
        Destroy(gameObject);
    }

    // ---- Boleta generada per codi, per a armes sense prefab ----

    private static Sprite circleSprite;

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
                go.transform.localScale = Vector3.one * weapon.projectileSpriteScale;
            }
            else
            {
                // Sense sprite: boleta de color
                sr.sprite = CircleSprite;
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
