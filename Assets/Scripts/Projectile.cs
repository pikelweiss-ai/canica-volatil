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

    public void Init(Vector2 dir, WeaponData weapon, Transform shooter)
    {
        direction = dir.normalized;
        speed = weapon.projectileSpeed;
        maxDistance = weapon.range;
        damage = weapon.damage;
        knockbackForce = weapon.knockbackForce;
        radius = weapon.projectileSize * 0.5f;
        owner = shooter;
        startPosition = transform.position;

        // Gira el projectil cap a on va (per si el prefab té forma)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        transform.position += (Vector3)(direction * speed * Time.fixedDeltaTime);

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, radius))
        {
            // No ens fem mal a nosaltres mateixos
            if (owner != null && hit.transform.IsChildOf(owner))
                continue;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeHit(damage, direction * knockbackForce);
                Destroy(gameObject);
                return;
            }

            // Collider sòlid (paret, arbre...) -> el projectil s'atura
            if (!hit.isTrigger)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (Vector2.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
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

    public static Projectile Spawn(Vector2 position, Vector2 dir, WeaponData weapon, Transform shooter, int sortingLayerID, int sortingOrder)
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
            go.transform.localScale = Vector3.one * weapon.projectileSize;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CircleSprite;
            sr.color = weapon.projectileColor;
            sr.sortingLayerID = sortingLayerID;
            sr.sortingOrder = sortingOrder;
        }

        Projectile p = go.GetComponent<Projectile>();
        if (p == null)
            p = go.AddComponent<Projectile>();

        p.Init(dir, weapon, shooter);
        return p;
    }
}
