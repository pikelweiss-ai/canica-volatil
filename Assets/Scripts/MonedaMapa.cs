using UnityEngine;

// Moneda animada col·locada al mapa (lobby, mapa lliure).
// Online: només la pot agafar UN jugador (l'amfitrió decideix qui ha arribat primer).
public class Coin : MonoBehaviour
{
    public Animator animator; // Referència a l'Animator de la moneda
    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true; // mentre esperem la resposta de l'amfitrió no es torna a demanar
            if (NetShared.ClaimMapCoin(this, Collect)) return;
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true;
        if (CoinCounter.instance != null) CoinCounter.instance.AddCoin();
        PlayVanish();
    }

    // Online: l'ha agafada un altre jugador
    public void NetTakenByOther()
    {
        isCollected = true;
        PlayVanish();
    }

    // Online: l'amfitrió no ha respost; es pot tornar a provar
    public void NetRelease() { isCollected = false; }

    void PlayVanish()
    {
        if (animator != null) animator.SetTrigger("Collect"); // El Trigger s'ha d'anomenar 'Collect'
        StartCoroutine(DestroyAfterAnimation());
    }

    private System.Collections.IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1f); // Ajusta aquest temps segons la duració de l'animació CoinCollect
        Destroy(gameObject);
    }
}
