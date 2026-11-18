using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlsUI : MonoBehaviour
{
    int page, capture = -1, captureSlot; float captureAfter, previousScale;
    Transform panel; TMP_FontAsset font;
    public static void Open()
    {
        if(MobileInput.Active) { MobileHelpUI.Open(); return; }
        if (GameControls.Editing) return;
        var go = new GameObject("Controls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=100;
        var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1600,900); scaler.matchWidthOrHeight=0.5f;
        var ui=go.AddComponent<ControlsUI>(); var any=Object.FindFirstObjectByType<TextMeshProUGUI>(); if(any!=null) ui.font=any.font;
        ui.previousScale=Time.timeScale; Time.timeScale=0; GameControls.Editing=true; ui.Build();
    }
    void Build()
    {
        if(panel!=null) { panel.gameObject.SetActive(false); Destroy(panel.gameObject); }
        var bg=new GameObject("Panell",typeof(RectTransform),typeof(Image)); bg.transform.SetParent(transform,false); panel=bg.transform;
        var rect=(RectTransform)panel; rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        bg.GetComponent<Image>().color=new Color(0.025f,0.04f,0.075f,1f);
        Text("CONTROLS · "+(page+1)+"/2",0,380,40,1100);
        Text(capture>=0 ? "Prem una tecla · Esc cancel·la · Retrocés lleva l'assignació" : "Clica una assignació. Els duplicats s'intercanvien. Dues tecles per acció.",0,320,22,1500);
        for(int row=0;row<10;row++)
        {
            int a=page*10+row; if(a>=GameControls.Actions.Length) break;
            float y=260-row*48;
            Text(GameControls.Actions[a],-410,y,24,500);
            for(int slot=0;slot<2;slot++) { int s=slot; Button((capture==a && captureSlot==slot)?"...":GameControls.Key(a,slot).ToString(),80+slot*420,y,390,()=>{capture=a;captureSlot=s;captureAfter=Time.unscaledTime+0.2f;Build();}); }
        }
        string[] modes={"Roda: armes","Roda: cures","Roda: armes / Maj + roda: cures","Roda desactivada"};
        Button(modes[GameControls.Wheel],0,-265,1300,()=>{GameControls.Wheel=(GameControls.Wheel+1)%4;Build();});
        Button("Anterior / següent",-470,-355,420,()=>{page=1-page;capture=-1;Build();});
        Button("Restablir",0,-355,380,()=>{GameControls.Reset();capture=-1;Build();});
        Button("Fet",470,-355,380,Close);
    }
    void Text(string text,float x,float y,float size,float width)
    {
        var go=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(panel,false);
        var t=go.GetComponent<TextMeshProUGUI>();if(font!=null)t.font=font;t.text=text;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;
        var r=t.rectTransform;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(width,44);
    }
    void Button(string title,float x,float y,float width,UnityEngine.Events.UnityAction action)
    {
        var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(panel,false);
        var r=(RectTransform)go.transform;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(width,42);
        go.GetComponent<Image>().color=new Color(0.12f,0.28f,0.35f);go.GetComponent<Button>().onClick.AddListener(action);
        var label=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(go.transform,false);
        var t=label.GetComponent<TextMeshProUGUI>();if(font!=null)t.font=font;t.text=title;t.fontSize=22;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;
        t.rectTransform.sizeDelta=new Vector2(width-12,40);
    }
    void Update()
    {
        if(capture<0) { if(Input.GetKeyDown(KeyCode.Escape))Close(); return; }
        if(Time.unscaledTime<captureAfter) return;
        if(Input.GetKeyDown(KeyCode.Escape)){capture=-1;Build();return;}
        foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode))) if(key!=KeyCode.None && Input.GetKeyDown(key))
        { GameControls.Bind(capture,captureSlot,key==KeyCode.Backspace?KeyCode.None:key); capture=-1;Build();break; }
    }
    void Close(){gameObject.SetActive(false);Destroy(gameObject);}
    void OnDisable(){GameControls.Editing=false;Time.timeScale=previousScale;}
}
