using UnityEngine;
using TMPro;

public class UsernameManager : MonoBehaviour
{
    public TMP_InputField nameInput;

    public void SavePlayerName()
    {
        string playerName = nameInput.text;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Usuario";
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
    }
}