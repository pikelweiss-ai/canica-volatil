using UnityEngine;

// Objecte que es trenca a colps o trets: caixes (poden deixar monedes o cura)
// i barrils explosius (exploten i fan mal a tot el que hi ha prop, també a tu!).
public class Breakable : MonoBehaviour
{
    public int health = 2;
    public Color debrisColor = new Color(0.62f, 0.35f, 0.16f);

    [Header("Barril explosiu")]
    public bool explodes = false;
    public float explosionRadius = 1.7f;
    public int explosionPlayerDamage = 2;
    public int explosionEnemyDamage = 6;

    [Header("Botí")]
    [Range(0f, 1f)] public float coinChance = 0.4f;
    [Range(0f, 1f)] public float healChance = 0.08f;

    private SpriteRenderer sr;
    private bool broken;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        TileNav.AddBlocker(transform.position);
    }

    public void Hit(int damage)
    {
        if (broken) return;
        health -= damage;
        if (sr != null) { StopAllCoroutines(); StartCoroutine(Flash()); }
        Fx.Play(WeaponSounds.Impact, 0.4f);
        if (health <= 0) Break();
    }

    System.Collections.IEnumerator Flash()
    {
        Vector3 basePos = sr.transform.localPosition;
        sr.color = new Color(1f, 0.7f, 0.7f);
        for (int i = 0; i < 4; i++)
        {
            sr.transform.localPosition = basePos + (Vector3)Random.insideUnitCircle * 0.05f;
            yield return new WaitForSeconds(0.025f);
        }
        sr.transform.localPosition = basePos;
        sr.color = Color.white;
    }

    void Break()
    {
        broken = true;
        TileNav.RemoveBlocker(transform.position);
        Fx.Burst(transform.position, debrisColor, 10, 3f);

        if (explodes)
            Fx.Explode(transform.position, explosionRadius, explosionPlayerDamage, explosionEnemyDamage, transform);
        else
        {
            if (Random.value < coinChance) CoinDrop.Spawn(transform.position);
            if (Random.value < healChance)
            {
                HealItem item = HealItem.RandomCommon();
                if (item != null) HealPickup.Spawn(item, transform.position, transform.position);
            }
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!broken) TileNav.RemoveBlocker(transform.position);
    }
}
