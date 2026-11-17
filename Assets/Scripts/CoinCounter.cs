using UnityEngine;
using TMPro;

// Comptador de monedes de la pantalla.
// - A la masmorra continua sumant entre pisos (no torna a 0 quan passes de pis).
// - Cada moneda també se suma al total guardat de totes les partides (CoinBank).
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

    void Start()
    {
        if (RunState.Active)
            currentCoins = RunState.Coins;
        UpdateText();
    }

    public void AddCoin()
    {
        currentCoins++;
        RunState.Coins = currentCoins;
        CoinBank.Add(1);
        UpdateText();
        if (coinAudioSource != null) coinAudioSource.Play();
    }

    void UpdateText()
    {
        if (coinText != null) coinText.text = currentCoins.ToString();
    }
}

// Monedes guardades de totes les partides (per a una futura botiga de skins)
public static class CoinBank
{
    const string Key = "MonedesTotals";
    public static int Total => PlayerPrefs.GetInt(Key, 0);
    public static void Add(int n)
    {
        PlayerPrefs.SetInt(Key, Total + n);
    }
}
