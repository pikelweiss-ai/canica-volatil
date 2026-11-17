using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Mapes que es poden triar en prémer Jugar")]
    public MapInfo[] maps =
    {
        new MapInfo { displayName = "Masmorra", sceneName = "Dungeon" },
        new MapInfo { displayName = "Mapa proves", sceneName = "GameScene" },
        new MapInfo { displayName = "Lobby", sceneName = "Lobby" },
    };

    public TMP_FontAsset menuFont; // Si està buit, agafa la font dels botons del menú

    private MapSelectUI mapSelect;
    private CustomizeMenuUI customize;

    void Start()
    {
        // Tornar al menú acaba la partida de masmorra que hi haguera
        RunState.Reset();
        EnsureDungeonMap();
        CreateCustomizeButton();
    }

    // L'escena guardada pot tindre la llista vella: afegim la Masmorra al principi si no hi és
    void EnsureDungeonMap()
    {
        if (maps == null) maps = new MapInfo[0];
        foreach (MapInfo m in maps) if (m != null && m.sceneName == "Dungeon") return;
        var list = new System.Collections.Generic.List<MapInfo>(maps);
        list.Insert(0, new MapInfo { displayName = "Masmorra", sceneName = "Dungeon" });
        maps = list.ToArray();
    }

    // Copia el botó "Jugar" i el posa davall de "Ajustes", amb el text "Personalitzar"
    void CreateCustomizeButton()
    {
        GameObject play = GameObject.Find("Jugar");
        if (play == null) return;

        GameObject copy = Instantiate(play, play.transform.parent);
        copy.name = "Personalitzar";
        RectTransform rt = copy.GetComponent<RectTransform>();
        rt.anchoredPosition = play.GetComponent<RectTransform>().anchoredPosition + new Vector2(0f, -120f);

        TextMeshProUGUI label = copy.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = "Personalitzar";

        Button b = copy.GetComponent<Button>();
        if (b != null)
        {
            b.onClick = new Button.ButtonClickedEvent(); // Lleva el "PlayGame" copiat
            b.onClick.AddListener(OpenCustomize);
        GameObject controls=Instantiate(copy,copy.transform.parent);
        controls.name="Controls";
        controls.GetComponent<RectTransform>().anchoredPosition=rt.anchoredPosition+new Vector2(0,-60);
        controls.GetComponentInChildren<TextMeshProUGUI>().text="Controls";
        var cb=controls.GetComponent<Button>();cb.onClick=new Button.ButtonClickedEvent();cb.onClick.AddListener(ControlsUI.Open);
        }
    }

    TMP_FontAsset Font()
    {
        if (menuFont != null) return menuFont;
        TextMeshProUGUI anyText = FindAnyObjectByType<TextMeshProUGUI>();
        return anyText != null ? anyText.font : null;
    }

    // El crida el botó "Jugar": obri la pantalla per a triar mapa
    public void PlayGame()
    {
        if (mapSelect == null)
            mapSelect = MapSelectUI.Create(maps, Font());

        mapSelect.Show();
    }

    public void OpenCustomize()
    {
        if (customize == null)
            customize = CustomizeMenuUI.Create(Font());
        customize.Show();
    }
}
