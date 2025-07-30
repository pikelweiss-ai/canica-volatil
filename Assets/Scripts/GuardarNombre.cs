using UnityEngine;
using TMPro;

public class GuardarNombre : MonoBehaviour
{
    public TMP_InputField nameInput;           // Camp per escriure el nom
    public TextMeshPro nameDisplay;            // Text 3D damunt del jugador

    void Start()
    {
        string savedName = PlayerPrefs.GetString("PlayerName", "Usuario");
        nameDisplay.text = savedName;
    }

    public void SavePlayerName()
    {
        string playerName = nameInput.text;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Usuario";
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        nameDisplay.text = playerName;
    }
}