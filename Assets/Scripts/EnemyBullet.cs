using UnityEngine;

// Bala dels enemics: va recta, fa mal al jugador i es trenca contra parets i objectes sòlids.
public class EnemyBullet : MonoBehaviour
{
    public static Sprite sprite; // L'assigna la masmorra (BalaEnemiga.png); si no, cercle roig

    private Vector2 velocity;
    private int damage;
    private float life;
    private Transform owner;

    public static EnemyBullet Fire(Vector2 pos, Vector2 dir, float speed, int damage, Transform owner, float scale = 1f)
    {
        Sprite s = sprite != null ? sprite : Fx.PixelCircle(6);
        SpriteRenderer sr = Fx.MakeSprite("Bala enemiga", pos, s, sprite != null ? Color.white : new Color(1f, 0.3f, 0.4f), "Player", 8);
        sr.transform.localScale = Vector3.one * scale;
        EnemyBullet b = sr.gameObject.AddComponent<EnemyBullet>();
        b.velocity = dir.normalized * speed;
        b.damage = damage;
        b.owner = owner;
        b.life = 6f;
        return b;
    }

    void FixedUpdate()
    {
        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
        life -= Time.fixedDeltaTime;
        if (life <= 0f) { Destroy(gameObject); return; }

        if (TileNav.Ground != null && TileNav.IsBlockedWorld(transform.position)) { Pop(); return; }

        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, 0.12f * transform.localScale.x))
        {
            if (owner != null && hit.transform.IsChildOf(owner)) continue;

            PlayerHealth p = hit.GetComponentInParent<PlayerHealth>();
            if (p != null)
            {
                p.TakeDamage(damage);
                Pop();
                return;
            }
            if (hit.GetComponentInParent<EnemyHealth>() != null) continue;     // Travessa els altres enemics
            if (!hit.isTrigger) { Pop(); return; }                             // Paret, cofre, caixa...
        }
    }

    void Pop()
    {
        Fx.Burst(transform.position, new Color(1f, 0.4f, 0.5f), 4, 1.5f);
        Destroy(gameObject);
    }
}
