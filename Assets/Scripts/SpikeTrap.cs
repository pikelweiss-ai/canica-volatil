using UnityEngine;

// Punxes del terra: estan amagades, tremolen un moment (avís) i ixen.
// Si estàs damunt quan ixen, et fan mal. Es poden trepitjar quan estan baixades.
public class SpikeTrap : MonoBehaviour
{
    public Sprite downSprite;
    public Sprite upSprite;
    public float downTime = 1.6f;
    public float warnTime = 0.35f;
    public float upTime = 0.9f;
    public int damage = 1;

    private SpriteRenderer sr;
    private Transform player;
    private PlayerHealth playerHealth;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; playerHealth = p.GetComponent<PlayerHealth>(); }
        // Cada trampa comença en un moment diferent
        StartCoroutine(Cycle(Random.Range(0f, downTime)));
    }

    System.Collections.IEnumerator Cycle(float firstDelay)
    {
        yield return new WaitForSeconds(firstDelay);
        Vector3 basePos = sr.transform.localPosition;
        while (true)
        {
            sr.sprite = downSprite;
            yield return new WaitForSeconds(downTime);

            // Avís: tremola
            float t = 0f;
            while (t < warnTime)
            {
                sr.transform.localPosition = basePos + new Vector3(Random.Range(-0.03f, 0.03f), 0f, 0f);
                t += Time.deltaTime;
                yield return null;
            }
            sr.transform.localPosition = basePos;

            sr.sprite = upSprite;
            t = 0f;
            while (t < upTime)
            {
                if (player != null && playerHealth != null &&
                    Mathf.Abs(player.position.x - transform.position.x) < 0.45f &&
                    Mathf.Abs(player.position.y - transform.position.y) < 0.45f)
                    playerHealth.TakeDamage(damage); // Els frames d'invulnerabilitat eviten el dany repetit
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}
