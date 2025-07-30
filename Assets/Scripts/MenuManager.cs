using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject SettingsPanel;

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