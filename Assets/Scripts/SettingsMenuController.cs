using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public void OpenSettings() { SettingsUI.Open(); }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}