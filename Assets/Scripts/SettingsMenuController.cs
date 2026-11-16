using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}