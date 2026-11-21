using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class GameTypography
{
    public const string MainMenuButtonText = "Text dels botons del menú principal";
    static readonly System.Collections.Generic.HashSet<TMP_Text> pending = new System.Collections.Generic.HashSet<TMP_Text>();
    static Material shadowMaterial;
    static TMP_FontAsset mainMenuButtonFont;
    public static TMP_FontAsset Font => AlphaVisualStyle.Current != null ? AlphaVisualStyle.Current.font : TMP_Settings.defaultFontAsset;
    public static TMP_FontAsset MainMenuButtonFont
    {
        get
        {
            if (mainMenuButtonFont != null) return mainMenuButtonFont;
            var source = Resources.Load<UnityEngine.Font>("Fonts/ChakraPetch-Bold");
            if (source != null) mainMenuButtonFont = TMP_FontAsset.CreateFontAsset(source);
            return mainMenuButtonFont;
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(Changed);
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(Changed);
        SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded;
        pending.Clear();
        if(shadowMaterial != null) Object.Destroy(shadowMaterial);
        shadowMaterial=null;
        if (mainMenuButtonFont != null) Object.Destroy(mainMenuButtonFont);
        mainMenuButtonFont = null;
        var go = new GameObject("Tipografia del joc",typeof(TypographyPump)); Object.DontDestroyOnLoad(go);
    }
    static void Loaded(Scene s,LoadSceneMode m) { foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Apply(text); }
    static void Changed(Object obj) { if(obj is TMP_Text text) pending.Add(text); }
    public static void Flush() { var texts = new TMP_Text[pending.Count]; pending.CopyTo(texts); pending.Clear(); foreach(var text in texts) Apply(text); }
    public static void Apply(TMP_Text text)
    {
        if(text == null || Font == null) return;
        if(text is TextMeshProUGUI)
        {
            var scaler=text.GetComponentInParent<UnityEngine.UI.CanvasScaler>();
            if(scaler != null && scaler.uiScaleMode==UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
        }
        bool coins=false;
        for(Transform t=text.transform;t!=null;t=t.parent)
            if(t.name == "CoinText") { coins=true; break; }
        var menuFont = text.name == MainMenuButtonText ? MainMenuButtonFont : null;
        var target = menuFont != null ? menuFont : coins && AlphaVisualStyle.Current != null ? AlphaVisualStyle.Current.coinFont : Font;
        if(target == null) return;
        if(text.font != target) text.font=target;
        if(coins || menuFont != null) return;
        if((text.fontStyle & FontStyles.Bold)==0) text.fontStyle |= FontStyles.Bold;
        if(shadowMaterial == null)
        {
            shadowMaterial=new Material(Font.material) { name="Text principal · negreta i ombra" };
            shadowMaterial.EnableKeyword("UNDERLAY_ON");
            shadowMaterial.SetColor("_UnderlayColor",new Color(0,0,0,.7f));
            shadowMaterial.SetFloat("_UnderlayOffsetX",.8f);shadowMaterial.SetFloat("_UnderlayOffsetY",-.8f);
            shadowMaterial.SetFloat("_UnderlaySoftness",.12f);
        }
        if(text.fontSharedMaterial != shadowMaterial) text.fontSharedMaterial=shadowMaterial;
    }
}
public class TypographyPump : MonoBehaviour { void LateUpdate() { GameTypography.Flush(); } }
