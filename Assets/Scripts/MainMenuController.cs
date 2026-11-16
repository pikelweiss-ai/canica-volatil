using UnityEngine;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Mapes que es poden triar en prémer Jugar")]
    public MapInfo[] maps =
    {
        new MapInfo { displayName = "Mapa proves", sceneName = "GameScene" },
        new MapInfo { displayName = "Lobby", sceneName = "Lobby" },
    };

    public TMP_FontAsset menuFont; // Si està buit, agafa la font dels botons del menú

    private MapSelectUI mapSelect;

    // El crida el botó "Jugar": obri la pantalla per a triar mapa
    public void PlayGame()
    {
        if (mapSelect == null)
        {
            TMP_FontAsset font = menuFont;
            if (font == null)
            {
                TextMeshProUGUI anyText = FindAnyObjectByType<TextMeshProUGUI>();
                if (anyText != null) font = anyText.font;
            }
            mapSelect = MapSelectUI.Create(maps, font);
        }

        mapSelect.Show();
    }
}
