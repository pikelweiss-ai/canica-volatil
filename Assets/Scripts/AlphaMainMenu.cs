using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AlphaMainMenu : MonoBehaviour
{
    const string Art = "Pixel/Menu140/";
    RectTransform safeContent;
    Rect lastSafeArea;
    Vector2Int lastScreenSize;

    public static void Create(MainMenuController controller)
    {
        var oldPlay = GameObject.Find("Jugar");
        var oldCanvas = oldPlay != null ? oldPlay.GetComponentInParent<Canvas>() : null;
        if (oldCanvas != null)
        {
            oldCanvas.enabled = false;
            var raycaster = oldCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null) raycaster.enabled = false;
            foreach (var selectable in oldCanvas.GetComponentsInChildren<Selectable>(true))
                selectable.interactable = false;
        }

        var menu = UIKit.Canvas("Menu Alpha", 5);
        menu.AddComponent<AlphaMainMenu>().Build(controller);
    }

    void Build(MainMenuController controller)
    {
        var backdrop = UIKit.Box(transform, Vector2.zero, Vector2.zero, UIKit.Ink, "Fons del menú");
        UIKit.Stretch(backdrop);
        backdrop.GetComponent<Image>().raycastTarget = false;
        var marble = CosmeticsDatabase.Instance?.playerSprite ?? AlphaVisualStyle.Current?.player;
        var art = Resources.Load<Sprite>(Art + "Fons");
        if (art != null)
        {
            var background = UIKit.Picture(backdrop, art, Vector2.zero, Vector2.zero);
            var fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectRatio = art.rect.width / art.rect.height;
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            // Coordenades sobre la imatge, perquè la canica conserve el lloc quan el fons es retalla.
            PlaceMarble(background.rectTransform, marble, "Canica del fons", new Vector2(.665f, .398f), .066f, art);
        }

        safeContent = UIKit.Empty(transform, "Àrea segura del menú");
        UpdateSafeArea();
        var logo = Resources.Load<Sprite>(Art + "Logo");
        if (logo != null)
        {
            var image = UIKit.Picture(safeContent, logo, Vector2.zero, new Vector2(500, 375));
            PlaceFromLeft(image.rectTransform, 325, 163);
            PlaceMarble(image.rectTransform, marble, "Canica del logo", new Vector2(.773f, .568f), .116f, logo);
        }
        else
        {
            var title = UIKit.Text(safeContent, "CANICA VOLÀTIL", Vector2.zero, new Vector2(470, 200), 62,
                color: UIKit.Gold);
            PlaceFromLeft(title.rectTransform, 325, 163);
        }

        AddButton("JUGAR", 337, controller.PlayGame, true);
        AddButton("PERSONALITZAR", 443, controller.OpenCustomize);
        AddButton("AJUSTOS", 548, SettingsUI.Open);
        AddButton("CONTROLS", 653, ControlsUI.Open);
        var news = AddButton("NOVETATS", 758, Changelog.Open);
        if (Changelog.HasNew)
        {
            var unread = UIKit.Text(news.rect, "NOU", new Vector2(122, 0), new Vector2(55, 32), 17,
                color: UIKit.Gold);
            news.button.onClick.AddListener(() => unread.gameObject.SetActive(false));
        }

        var version = UIKit.Text(safeContent, GameVersion.Label, Vector2.zero, new Vector2(330, 35), 22,
            TextAlignmentOptions.MidlineLeft);
        version.rectTransform.anchorMin = version.rectTransform.anchorMax = Vector2.zero;
        version.rectTransform.pivot = Vector2.zero;
        version.rectTransform.anchoredPosition = new Vector2(28, 16);
    }

    static void PlaceMarble(RectTransform parent, Sprite marble, string name, Vector2 position, float width, Sprite artwork)
    {
        if (marble == null) return;
        var image = UIKit.Picture(parent, marble, Vector2.zero, Vector2.zero);
        image.name = name;
        float height = width * artwork.rect.width / artwork.rect.height;
        var half = new Vector2(width, height) * .5f;
        var rect = image.rectTransform;
        rect.anchorMin = position - half;
        rect.anchorMax = position + half;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    UIKit.UIButton AddButton(string label, float y, UnityAction action, bool primary = false)
    {
        var style = primary ? UIKit.Style.Primary : UIKit.Style.Normal;
        float width = primary ? 490 : 424;
        var button = UIKit.Button(safeContent, label, Vector2.zero, new Vector2(width - 40, 93),
            action, style, primary ? 36 : 26, false);
        button.label.name = GameTypography.MainMenuButtonText;
        button.label.characterSpacing = 2f;
        var labelRect = button.label.rectTransform;
        labelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, primary ? 360 : 306);
        button.label.enableAutoSizing = true;
        button.label.fontSizeMax = button.label.fontSize;
        button.label.fontSizeMin = button.label.fontSize * .65f;
        GameTypography.Apply(button.label);
        PlaceFromLeft(button.rect, 325, y);

        string art = Art + (primary ? "primari_" : "secundari_");
        var normal = Resources.Load<Sprite>(art + "normal");
        if (normal == null) return button;

        button.image.color = Color.clear;
        button.image.canvasRenderer.cullTransparentMesh = false;
        var sprite = UIKit.Picture(button.rect, normal, Vector2.zero, new Vector2(width, width / 3f));
        sprite.transform.SetAsFirstSibling();
        button.button.targetGraphic = sprite;
        button.button.transition = Selectable.Transition.SpriteSwap;
        var hover = Resources.Load<Sprite>(art + "hover");
        button.button.spriteState = new SpriteState
        {
            highlightedSprite = hover,
            selectedSprite = hover,
            pressedSprite = Resources.Load<Sprite>(art + "premut")
        };
        if (primary) button.label.color = new Color(.15f, .065f, .025f);
        labelRect.anchoredPosition = new Vector2(0, primary ? 6 : 0);
        return button;
    }

    static void PlaceFromLeft(RectTransform rect, float x, float y)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
    }

    void LateUpdate() => UpdateSafeArea();

    void UpdateSafeArea()
    {
        var size = new Vector2Int(Screen.width, Screen.height);
        var safeArea = Screen.safeArea;
        if (size == lastScreenSize && safeArea == lastSafeArea) return;
        lastScreenSize = size;
        lastSafeArea = safeArea;
        safeContent.anchorMin = new Vector2(safeArea.xMin / size.x, safeArea.yMin / size.y);
        safeContent.anchorMax = new Vector2(safeArea.xMax / size.x, safeArea.yMax / size.y);
    }
}
