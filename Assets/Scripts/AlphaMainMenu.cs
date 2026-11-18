using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Composicio amb els sprites originals del joc, creada per codi i adaptable.
public class AlphaMainMenu : MonoBehaviour
{
    static readonly Color Ink = new Color(.035f,.065f,.10f);
    public static void Create(MainMenuController controller)
    {
        var oldPlay = GameObject.Find("Jugar");
        if(oldPlay != null)
        {
            var old = oldPlay.GetComponentInParent<Canvas>();
            if(old != null) { old.enabled = false; var ray = old.GetComponent<GraphicRaycaster>(); if(ray != null) ray.enabled = false; foreach(var selectable in old.GetComponentsInChildren<Selectable>(true)) selectable.interactable=false; }
        }
        var go = new GameObject("Menu Alpha",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=5;
        var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1600,900); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var ui=go.AddComponent<AlphaMainMenu>();
        var bg=ui.Box(go.transform,Vector2.zero,Vector2.zero,Ink); bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;
        ui.Box(go.transform,new Vector2(395,0),new Vector2(750,900),new Color(.06f,.12f,.16f));
        if(AlphaVisualStyle.Current != null && AlphaVisualStyle.Current.logo != null) ui.Picture(go.transform,AlphaVisualStyle.Current.logo,new Vector2(-455,270),new Vector2(630,230));
        else ui.Text(go.transform,"CANICA\nVOLÀTIL",new Vector2(-440,270),new Vector2(630,200),76,TextAlignmentOptions.Left,new Color(1,.87f,.45f));
        ui.Text(go.transform,"EXPEDICIONS · TRESORS · CARAMBOLES",new Vector2(-440,120),new Vector2(630,65),20,TextAlignmentOptions.Left,new Color(.55f,.75f,.75f));
        ui.Button(go.transform,"JUGAR",new Vector2(-455,20),controller.PlayGame,true);
        ui.Button(go.transform,"PERSONALITZAR",new Vector2(-455,-75),controller.OpenCustomize);
        ui.Button(go.transform,"AJUSTOS",new Vector2(-455,-170),SettingsUI.Open);
        ui.Button(go.transform,"CONTROLS",new Vector2(-455,-265),ControlsUI.Open);
        ui.Text(go.transform,GameVersion.Label,new Vector2(-440,-385),new Vector2(630,50),22,TextAlignmentOptions.Left,new Color(.4f,.58f,.62f));
        ui.Diorama(go.transform);
    }
    void Diorama(Transform parent)
    {
        var style=AlphaVisualStyle.Current; if(style == null) return;
        if(style.menuArtwork != null)
        {
            Picture(parent,style.menuArtwork,new Vector2(395,-20),new Vector2(620,620));
            Text(parent,"CADA PARTIDA,\nUNA NOVA HISTÒRIA.",new Vector2(395,350),new Vector2(650,100),30,TextAlignmentOptions.Center,new Color(.85f,.93f,.9f));
            Text(parent,"EXPLORA · COMBINA · SUPERA'T",new Vector2(395,-365),new Vector2(650,60),20,TextAlignmentOptions.Center,new Color(.5f,.7f,.7f));
            return;
        }
        // Illa de rajola, gespa i fusta: peces del mapa, sense il·lustracio generada.
        for(int y=0;y<6;y++) for(int x=0;x<7;x++)
        {
            if((x==0 || x==6) && (y==0 || y==5)) continue;
            var sprite=y==5 ? style.brick : y<2 ? style.wood : style.grass;
            Picture(parent,sprite,new Vector2(185+x*64,-220+y*64),new Vector2(64,64));
        }
        Picture(parent,style.tree,new Vector2(220,150),new Vector2(140,190));
        Picture(parent,style.tree,new Vector2(560,115),new Vector2(125,170));
        Box(parent,new Vector2(370,-80),new Vector2(100,24),new Color(0,0,0,.3f));
        Picture(parent,style.player,new Vector2(370,-35),new Vector2(108,108));
        Text(parent,"CADA PARTIDA,\nUNA NOVA HISTÒRIA.",new Vector2(395,320),new Vector2(650,130),30,TextAlignmentOptions.Center,new Color(.85f,.93f,.9f));
        Text(parent,"EXPLORA · COMBINA · SUPERA'T",new Vector2(395,-355),new Vector2(650,60),20,TextAlignmentOptions.Center,new Color(.5f,.7f,.7f));
    }
    void Picture(Transform parent,Sprite sprite,Vector2 pos,Vector2 size)
    {
        if(sprite == null) return; var r=Box(parent,pos,size,Color.white); var im=r.GetComponent<Image>(); im.sprite=sprite; im.preserveAspect=true; im.raycastTarget=false;
    }
    RectTransform Box(Transform parent,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject("Panell",typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=Vector2.one*.5f;r.anchoredPosition=pos;r.sizeDelta=size;go.GetComponent<Image>().color=color;return r;
    }
    void Text(Transform parent,string value,Vector2 pos,Vector2 size,float fontSize,TextAlignmentOptions alignment,Color color)
    {
        var go=new GameObject(value,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var t=go.GetComponent<TextMeshProUGUI>();t.font=GameTypography.Font;t.text=value;t.fontSize=fontSize;t.alignment=alignment;t.color=color;t.raycastTarget=false;t.rectTransform.anchorMin=t.rectTransform.anchorMax=Vector2.one*.5f;t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;
    }
    void Button(Transform parent,string label,Vector2 pos,UnityEngine.Events.UnityAction action,bool primary=false)
    {
        Box(parent,pos+new Vector2(0,-5),new Vector2(560,75),new Color(.015f,.025f,.04f));
        var r=Box(parent,pos,new Vector2(560,75),primary ? new Color(.92f,.70f,.28f) : new Color(.11f,.21f,.27f));
        var b=r.gameObject.AddComponent<Button>();var c=b.colors;c.highlightedColor=new Color(1,1,.85f);c.pressedColor=new Color(.65f,.8f,.8f);c.selectedColor=c.highlightedColor;b.colors=c;b.onClick.AddListener(action);
        Text(r,label,Vector2.zero,new Vector2(530,65),30,TextAlignmentOptions.Center,primary ? Ink : new Color(.88f,.94f,.95f));
    }
}
