using UnityEngine;
using TMPro;

public class PlayerNameTag : MonoBehaviour
{
    public TextMeshPro playerNameText;

    void Start()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Usuario");
        playerNameText.text = playerName;
    }
}