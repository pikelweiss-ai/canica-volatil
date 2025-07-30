using UnityEngine;

public class Coin : MonoBehaviour
{
    public Animator animator; // Referència a l'Animator de la moneda
    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            CoinCounter.instance.AddCoin();
            animator.SetTrigger("Collect"); // El Trigger s'ha d'anomenar 'Collect'
            StartCoroutine(DestroyAfterAnimation());
        }
    }

    private System.Collections.IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1f); // Ajusta aquest temps segons la duració de l'animació CoinCollect
        Destroy(gameObject);
    }
}