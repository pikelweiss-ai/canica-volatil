using System;
using UnityEngine;
using TMPro;

public static class AlphaSettingsChecks
{
    public static void Validate()
    {
        if(GameControls.DefaultKey(10)!=KeyCode.Mouse1) throw new Exception("La cura ha de ser clic dret per defecte");
        var sizes=DisplaySettings.Supported(1920,1080,new Vector2Int(1280,720));
        if(!sizes.Contains(new Vector2Int(1920,1080)) || !sizes.Contains(new Vector2Int(1024,768))) throw new Exception("Falten resolucions natives o 4:3");
        if(sizes.Exists(s=>s.x>1920 || s.y>1080) || new System.Collections.Generic.HashSet<Vector2Int>(sizes).Count!=sizes.Count) throw new Exception("Resolucions invalides o duplicades");
        var fitted=DisplaySettings.FitWindow(new Vector2Int(1920,1080),new Vector2Int(1920,1040));
        if(fitted.x>1896 || fitted.y>968 || Mathf.Abs((float)fitted.x/fitted.y-16f/9)>0.004f) throw new Exception("La finestra tapa la barra o deforma la proporcio");
        if(DisplaySettings.FitWindow(new Vector2Int(1024,768),new Vector2Int(1920,1040))!=new Vector2Int(1024,768)) throw new Exception("Una finestra que ja cap no ha de canviar");
        if(System.Array.IndexOf(PresentationSettings.Limits,240)<0) throw new Exception("Falta opcio de 240 FPS");
        if(AlphaVisualStyle.Current.logo==null || AlphaVisualStyle.Current.menuArtwork==null) throw new Exception("Falten les imatges del menu");
        var root=new GameObject("Prova de tipografia",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
        root.GetComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        try
        {
            var ui=new GameObject("Text principal",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();ui.transform.SetParent(root.transform,false);
            GameTypography.Apply(ui);
            if(root.GetComponent<UnityEngine.UI.CanvasScaler>().screenMatchMode!=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand) throw new Exception("La UI pot retallar-se en 4:3");
            if(ui.font!=AlphaVisualStyle.Current.font || !ui.font.name.Contains("LiberationSans") || (ui.fontStyle & FontStyles.Bold)==0 || !ui.fontSharedMaterial.IsKeywordEnabled("UNDERLAY_ON")) throw new Exception("Font principal sense negreta o ombra");
            var material=ui.fontSharedMaterial;GameTypography.Apply(ui);if(ui.fontSharedMaterial!=material) throw new Exception("Material de text duplicat");
            var coin=new GameObject("CoinText",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();coin.transform.SetParent(root.transform,false);GameTypography.Apply(coin);
            if(coin.font!=AlphaVisualStyle.Current.coinFont || !coin.font.name.Contains("Minecraft")) throw new Exception("El comptador ha de conservar Minecraft");
            var world=new GameObject("Nom jugador",typeof(TextMeshPro)).GetComponent<TextMeshPro>();world.transform.SetParent(root.transform,false);GameTypography.Apply(world);
            if(world.font!=ui.font || (world.fontStyle & FontStyles.Bold)==0) throw new Exception("El text del mon usa una altra font");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        Debug.Log("ALPHA_SETTINGS_CHECKS: PASS (font UI/mon, negreta, ombra, monedes, cura i resolucions)");
    }
}
