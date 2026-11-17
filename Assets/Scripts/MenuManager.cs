using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject SettingsPanel;

    public void OpenSettings() { SettingsUI.Open(); }

    public void BackToMenu()
    {
        SettingsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
}