using UnityEngine;

public class AlwaysMenu : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject SettingsPanel;

    private void Start()
    {
        // Quan comença, assegura que només el Main Menu està actiu.
        MainMenuPanel.SetActive(true);
        SettingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        SettingsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
}