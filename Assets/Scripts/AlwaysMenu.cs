using UnityEngine;

public class AlwaysMenu : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject SettingsPanel;

    private void Start()
    {
        // En iniciar, mostrar el menu principal.
        MainMenuPanel.SetActive(true);
        SettingsPanel.SetActive(false);
    }

    public void OpenSettings() { SettingsUI.Open(); }

    public void BackToMenu()
    {
        SettingsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
}