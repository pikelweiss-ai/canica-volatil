using UnityEngine;
using TMPro;

public class CoinCounter : MonoBehaviour
{
    public static CoinCounter instance;
    public TextMeshProUGUI coinText;
    public int currentCoins = 0;

    public AudioSource coinAudioSource;

    void Awake()
    {
        instance = this;
    }

    public void AddCoin()
    {
        currentCoins++;
        coinText.text = currentCoins.ToString();
        coinAudioSource.Play();
    }
}