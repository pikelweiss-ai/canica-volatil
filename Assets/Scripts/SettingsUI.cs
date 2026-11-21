using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    float previousScale;
    bool previousEditing;
    GameObject panel;
    int page;
    int resolutionIndex;
    bool fullscreen;
    System.Collections.Generic.List<Vector2Int> resolutions;
    public static void Open()
    {
        if(Object.FindFirstObjectByType<SettingsUI>() != null) return;
        var go=new GameObject("Ajustos",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=110;
        var sc=go.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var ui=go.AddComponent<SettingsUI>();ui.previousScale=Time.timeScale;ui.previousEditing=GameControls.Editing;
        ui.resolutions=DisplaySettings.Options();ui.resolutionIndex=Mathf.Max(0,ui.resolutions.IndexOf(new Vector2Int(Screen.width,Screen.height)));ui.fullscreen=DisplaySettings.IsFullscreen;
        Time.timeScale=0;GameControls.Editing=true;ui.Build();
    }
    RectTransform Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;go.GetComponent<Image>().color=color;return r;
    }
    TextMeshProUGUI Text(string value,Transform parent,Vector2 pos,Vector2 size,float fontSize=26)
    {
        var go=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
        var t=go.GetComponent<TextMeshProUGUI>();t.font=GameTypography.Font;t.text=value;t.fontSize=fontSize;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.rectTransform.anchorMin=t.rectTransform.anchorMax=Vector2.one*.5f;t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=size;return t;
    }
    void Button(string label,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action,bool active=false)
    {
        var r=Box(label,panel.transform,pos,size,active ? new Color(.2f,.4f,.43f) : new Color(.10f,.20f,.27f));
        r.gameObject.AddComponent<Button>().onClick.AddListener(action);Text(label,r,Vector2.zero,size,25);
    }
    void Build()
    {
        if(panel!=null) { panel.SetActive(false); Destroy(panel); }
        var bg=Box("Fons",transform,Vector2.zero,Vector2.zero,new Color(.025f,.045f,.07f,.99f));panel=bg.gameObject;bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;
        Text("AJUSTOS",panel.transform,new Vector2(0,350),new Vector2(1200,70),42);
        Button("PARTIDA",new Vector2(-560,245),new Vector2(250,65),()=>{page=0;Build();},page==0);
        Button("SO",new Vector2(-280,245),new Vector2(250,65),()=>{page=1;Build();},page==1);
        Button("PANTALLA",new Vector2(0,245),new Vector2(250,65),()=>{page=2;Build();},page==2);
        Button("INTERFÍCIE",new Vector2(280,245),new Vector2(250,65),()=>{page=3;Build();},page==3);
        Button("OPCIONS",new Vector2(560,245),new Vector2(250,65),()=>{page=4;Build();},page==4);
        if(page==0)
        {
            Row("Mida del mapa",100,140,360,GameSettings.MapSize,v=>GameSettings.MapSize=v,0);
            Row("Abast del mapa",0,1,2,GameSettings.MapRange,v=>GameSettings.MapRange=v,1);
            Text(MobileInput.Active?"Consulta els controls tàctils i els dos joysticks en Controls.":"Les tecles i la roda del ratolí es configuren en Controls.",panel.transform,new Vector2(0,-125),new Vector2(1350,60),22);
            Button("OBRIR CONTROLS",new Vector2(0,-220),new Vector2(600,65),()=>{Close();ControlsUI.Open();});
        }
        else if(page==1)
        {
            Row("Volum total",115,0,1,GameSettings.Master,v=>GameSettings.Master=v,2);
            Row("Armes",30,0,1,GameSettings.Volume(SoundCategory.Weapons),v=>GameSettings.Volume(SoundCategory.Weapons,v),2);
            Row("Enemics",-55,0,1,GameSettings.Volume(SoundCategory.Enemies),v=>GameSettings.Volume(SoundCategory.Enemies,v),2);
            Row("Efectes i objectes",-140,0,1,GameSettings.Volume(SoundCategory.Effects),v=>GameSettings.Volume(SoundCategory.Effects,v),2);
            Row("Música i ambient",-225,0,1,GameSettings.Volume(SoundCategory.Music),v=>GameSettings.Volume(SoundCategory.Music,v),2);
        }
        else if(page==4)
        {
            // v1.3.1: opcions de joc
            Button("CURA INTEL·LIGENT (CLIC DRET): "+(GameOptions.SmartHeal?"SÍ":"NO"),new Vector2(0,160),new Vector2(1200,70),()=>{GameOptions.SmartHeal=!GameOptions.SmartHeal;Build();});
            Button("NÚMEROS DE DANY: "+(GameOptions.DamageNumbers?"SÍ":"NO"),new Vector2(0,80),new Vector2(1200,70),()=>{GameOptions.DamageNumbers=!GameOptions.DamageNumbers;Build();});
            Button("AIM AMB REBOTS (DISC): "+(GameOptions.BounceAim?"SÍ":"NO"),new Vector2(0,0),new Vector2(1200,70),()=>{GameOptions.BounceAim=!GameOptions.BounceAim;Build();});
            Row("Tremolor de càmera",-90,0,1,GameOptions.Shake,v=>GameOptions.Shake=v,2);
            Row("Seguiment suau de càmera",-175,0,1,GameOptions.CameraSmooth,v=>GameOptions.CameraSmooth=v,2); // v1.4.1
            Text("Clic dret: la cura intel·ligent tria la que encaixa amb la vida que et falta.\nSeguiment suau a 0: la càmera va enganxada a la canica (millor per a apuntar).",panel.transform,new Vector2(0,-262),new Vector2(1350,70),20);
        }
        else if(page==2 && MobileInput.Active)
        {
            Text("La pantalla s'adapta al dispositiu en horitzontal.",panel.transform,new Vector2(0,110),new Vector2(1300,80),26);
            Button("LÍMIT FPS: "+(PresentationSettings.Limit==0?"60":PresentationSettings.Limit.ToString()),new Vector2(0,-20),new Vector2(900,75),()=>{PresentationSettings.VSync=false;PresentationSettings.Limit=PresentationSettings.Limit==30?60:30;Build();});
            Text("30 FPS estalvia bateria; 60 FPS ofereix més fluïdesa.",panel.transform,new Vector2(0,-140),new Vector2(1300,80),23);
        }
        else if(page==2)
        {
            Text("Resolució",panel.transform,new Vector2(-430,135),new Vector2(400,60),28);
            var size=resolutions[resolutionIndex];
            Button("<",new Vector2(-100,135),new Vector2(80,65),()=>{resolutionIndex=(resolutionIndex+resolutions.Count-1)%resolutions.Count;Build();});
            var shown=fullscreen ? DisplaySettings.NativeSize() : size;
            Text(shown.x+" × "+shown.y,panel.transform,new Vector2(170,135),new Vector2(420,60),30);
            Button(">",new Vector2(450,135),new Vector2(80,65),()=>{resolutionIndex=(resolutionIndex+1)%resolutions.Count;Build();});
            // El mode s'aplica en clicar (amb confirmacio de 15 s), igual que F11
            Button(fullscreen ? "MODE: PANTALLA COMPLETA  ·  clica per a passar a finestra" : "MODE: FINESTRA  ·  clica per a passar a pantalla completa",new Vector2(0,45),new Vector2(1100,65),()=>DisplaySettings.Request(resolutions[resolutionIndex],!DisplaySettings.IsFullscreen));
            Button(PresentationSettings.VSync ? "V-SYNC: ACTIVAT (REFRESC DEL MONITOR)" : "V-SYNC: DESACTIVAT",new Vector2(0,-45),new Vector2(1100,60),()=>{PresentationSettings.VSync=!PresentationSettings.VSync;Build();});
            Button("LÍMIT FPS: "+(PresentationSettings.VSync ? "MONITOR" : PresentationSettings.Limit==0 ? "SENSE LÍMIT" : PresentationSettings.Limit.ToString()),new Vector2(0,-120),new Vector2(1100,60),()=>{int i=System.Array.IndexOf(PresentationSettings.Limits,PresentationSettings.Limit);PresentationSettings.VSync=false;PresentationSettings.Limit=PresentationSettings.Limits[(i+1)%PresentationSettings.Limits.Length];Build();});
            Button(fullscreen ? "APLICAR (EN PANTALLA COMPLETA S'USA LA NATIVA)" : "APLICAR RESOLUCIÓ",new Vector2(0,-215),new Vector2(fullscreen?900:550,65),()=>DisplaySettings.Request(resolutions[resolutionIndex],DisplaySettings.IsFullscreen));
            Text("F11 alterna entre finestra i pantalla completa.\nConfirma el canvi en 15 segons o es desfarà automàticament.",panel.transform,new Vector2(0,-285),new Vector2(1400,65),20);
        }
        else
        {
            Button("VIDA: "+HudLayout.Positions[HudLayout.HealthPosition],new Vector2(0,105),new Vector2(1200,75),()=>{HudLayout.HealthPosition=(HudLayout.HealthPosition+1)%4;Build();});
            Button("MONEDES: "+HudLayout.Positions[HudLayout.CoinPosition],new Vector2(0,0),new Vector2(1200,75),()=>{HudLayout.CoinPosition=(HudLayout.CoinPosition+1)%4;Build();});
            Text("Tria una posició per a cada indicador.\nEls canvis es veuen en la partida i es conserven en reiniciar.",panel.transform,new Vector2(0,-120),new Vector2(1350,90),23);
            Button("POSICIONS ORIGINALS",new Vector2(0,-230),new Vector2(700,65),()=>{HudLayout.HealthPosition=0;HudLayout.CoinPosition=0;Build();});
        }
        Button("FET",new Vector2(0,-355),new Vector2(350,65),Close);
        Text(page==2 && !MobileInput.Active ? "La configuració de pantalla es guarda en confirmar-la." : "Els canvis es guarden automàticament.",panel.transform,new Vector2(0,-415),new Vector2(1300,35),17);
    }
    void Row(string label,float y,float min,float max,float initial,UnityEngine.Events.UnityAction<float> change,int format)
    {
        Text(label,panel.transform,new Vector2(-440,y),new Vector2(480,60),25);
        var value=Text("",panel.transform,new Vector2(615,y),new Vector2(180,55),24);
        var track=Box(label,panel.transform,new Vector2(140,y),new Vector2(650,38),new Color(.08f,.14f,.19f));
        var fillArea=Box("Ompliment",track,Vector2.zero,Vector2.zero,Color.clear);fillArea.anchorMin=Vector2.zero;fillArea.anchorMax=Vector2.one;fillArea.offsetMin=new Vector2(12,12);fillArea.offsetMax=new Vector2(-12,-12);fillArea.GetComponent<Image>().raycastTarget=false;
        var fill=Box("Valor",fillArea,Vector2.zero,Vector2.zero,new Color(.2f,.55f,.58f));fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;fill.GetComponent<Image>().raycastTarget=false;
        var area=new GameObject("Area del cursor",typeof(RectTransform)).GetComponent<RectTransform>();area.SetParent(track,false);area.anchorMin=Vector2.zero;area.anchorMax=Vector2.one;area.offsetMin=new Vector2(12,0);area.offsetMax=new Vector2(-12,0);
        var handle=Box("Cursor",area,Vector2.zero,new Vector2(24,48),new Color(.85f,.88f,.72f));
        var slider=track.gameObject.AddComponent<Slider>();slider.targetGraphic=handle.GetComponent<Image>();slider.handleRect=handle;slider.fillRect=fill;slider.minValue=min;slider.maxValue=max;slider.wholeNumbers=format==0;slider.SetValueWithoutNotify(initial);
        System.Action<float> display=v=>value.text=format==0?Mathf.RoundToInt(v)+" px":format==1?v.ToString("0.0")+"x":Mathf.RoundToInt(v*100)+"%";
        display(initial);slider.onValueChanged.AddListener(v=>{change(v);display(v);});
    }
    void Update()
    {
        // Quan es confirma o es desfa un canvi de pantalla, la pestanya mostra el mode real
        if(!DisplaySettings.Pending && DisplaySettings.IsFullscreen!=fullscreen){fullscreen=DisplaySettings.IsFullscreen;if(page==2)Build();}
        if(Input.GetKeyDown(KeyCode.Escape) && !DisplaySettings.Pending && !DisplaySettings.ConsumedEscape)Close();
    }
    void Close(){gameObject.SetActive(false);Destroy(gameObject);}
    void OnDisable(){GameControls.Editing=previousEditing;Time.timeScale=previousScale;}
}
