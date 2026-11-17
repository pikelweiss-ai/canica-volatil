using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    float previousScale;bool previousEditing;TMP_FontAsset font;
    public static void Open()
    {
        if(Object.FindFirstObjectByType<SettingsUI>()!=null)return;
        var go=new GameObject("Ajustos",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=110;
        var sc=go.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.matchWidthOrHeight=0.5f;
        var ui=go.AddComponent<SettingsUI>();ui.previousScale=Time.timeScale;ui.previousEditing=GameControls.Editing;
        Time.timeScale=0;GameControls.Editing=true;
        var any=Object.FindFirstObjectByType<TextMeshProUGUI>();if(any!=null)ui.font=any.font;
        ui.Build();
    }
    RectTransform Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.anchoredPosition=pos;r.sizeDelta=size;go.GetComponent<Image>().color=color;return r;
    }
    TextMeshProUGUI Text(string value,Transform parent,Vector2 pos,Vector2 size,float fontSize=26)
    {
        var go=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
        var t=go.GetComponent<TextMeshProUGUI>();if(font!=null)t.font=font;t.text=value;t.fontSize=fontSize;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;return t;
    }
    void Build()
    {
        var bg=Box("Fons",transform,Vector2.zero,Vector2.zero,new Color(0.025f,0.04f,0.075f,1));bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;
        Text("AJUSTOS",transform,new Vector2(0,365),new Vector2(1200,70),44);
        Row("Mida del mapa",270,140,360,GameSettings.MapSize,v=>GameSettings.MapSize=v,false);
        Row("Abast del mapa",195,1,2,GameSettings.MapRange,v=>GameSettings.MapRange=v,false);
        Row("Volum total",80,0,1,GameSettings.Master,v=>GameSettings.Master=v);
        Row("Armes",5,0,1,GameSettings.Volume(SoundCategory.Weapons),v=>GameSettings.Volume(SoundCategory.Weapons,v));
        Row("Enemics",-70,0,1,GameSettings.Volume(SoundCategory.Enemies),v=>GameSettings.Volume(SoundCategory.Enemies,v));
        Row("Efectes i objectes",-145,0,1,GameSettings.Volume(SoundCategory.Effects),v=>GameSettings.Volume(SoundCategory.Effects,v));
        Row("Música i ambient",-220,0,1,GameSettings.Volume(SoundCategory.Music),v=>GameSettings.Volume(SoundCategory.Music,v));
        var back=Box("Fet",transform,new Vector2(0,-345),new Vector2(320,65),new Color(0.12f,0.3f,0.38f));back.gameObject.AddComponent<Button>().onClick.AddListener(Close);
        Text("Fet",back,Vector2.zero,new Vector2(300,60),30);
    }
    void Row(string label,float y,float min,float max,float initial,UnityEngine.Events.UnityAction<float> change,bool percent=true)
    {
        Text(label,transform,new Vector2(-420,y),new Vector2(550,50),25);
        var value=Text("",transform,new Vector2(610,y),new Vector2(170,50),24);
        var track=Box(label,transform,new Vector2(150,y),new Vector2(650,32),new Color(0.1f,0.18f,0.24f));
        var handle=Box("Cursor",track,Vector2.zero,new Vector2(22,42),new Color(0.3f,0.9f,0.9f));
        var slider=track.gameObject.AddComponent<Slider>();slider.targetGraphic=handle.GetComponent<Image>();slider.handleRect=handle;slider.minValue=min;slider.maxValue=max;slider.value=initial;
        System.Action<float> display=v=>value.text=percent?Mathf.RoundToInt(v*100)+"%":max>2?Mathf.RoundToInt(v)+" px":v.ToString("0.0")+"x";
        display(initial);slider.onValueChanged.AddListener(v=>{change(v);display(v);});
    }
    void Update(){if(Input.GetKeyDown(KeyCode.Escape))Close();}
    void Close(){gameObject.SetActive(false);Destroy(gameObject);}
    void OnDisable(){GameControls.Editing=previousEditing;Time.timeScale=previousScale;}
}