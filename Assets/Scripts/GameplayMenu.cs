using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Menu de partida comu a Dungeon i al camp de proves.
public class GameplayMenu : MonoBehaviour
{
    GameObject canvas,panel;TMP_FontAsset font;float previousScale;bool paused;RectTransform menuButton;
    void Start()
    {
        canvas=new GameObject("Menu de partida",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c=canvas.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=90;
        var sc=canvas.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.matchWidthOrHeight=0.5f;
        var any=Object.FindFirstObjectByType<TextMeshProUGUI>();if(any!=null)font=any.font;
        var button=Box("Menu",canvas.transform,MobileInput.Active ? new Vector2(85,70) : new Vector2(54,48),new Color(0.04f,0.09f,0.14f,0.8f));
        menuButton=button;
        button.anchorMin=button.anchorMax=button.pivot=Vector2.one;button.anchoredPosition=new Vector2(-20,-20);
        button.gameObject.AddComponent<Button>().onClick.AddListener(()=>Pause(!paused));
        for(int i=0;i<3;i++){var bar=Box("Barra",button,new Vector2(30,3),Color.white);bar.anchoredPosition=new Vector2(0,(i-1)*9);bar.GetComponent<Image>().raycastTarget=false;}
        var root=Box("Pausa",canvas.transform,Vector2.zero,new Color(0.02f,0.04f,0.07f,0.97f));panel=root.gameObject;
        root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
        Text("PARTIDA EN PAUSA",panel.transform,new Vector2(0,270),44);
        Button("Continuar",150,()=>Pause(false));
        Button("Ajustos",50,SettingsUI.Open);
        Button("Controls",-50,ControlsUI.Open);
        Button("Eixir al menú principal",-150,()=>{Pause(false);RunState.Reset();SceneManager.LoadScene("MainMenu");});
        panel.SetActive(false);
    }
    RectTransform Box(string name,Transform parent,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.sizeDelta=size;go.GetComponent<Image>().color=color;return r;
    }
    void Text(string label,Transform parent,Vector2 pos,float size)
    {
        var go=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
        var t=go.GetComponent<TextMeshProUGUI>();if(font!=null)t.font=font;t.text=label;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.rectTransform.sizeDelta=new Vector2(1200,80);t.rectTransform.anchoredPosition=pos;
    }
    void Button(string label,float y,UnityEngine.Events.UnityAction action)
    {
        var r=Box(label,panel.transform,new Vector2(850,72),new Color(0.12f,0.28f,0.36f));r.anchoredPosition=new Vector2(0,y);r.gameObject.AddComponent<Button>().onClick.AddListener(action);Text(label,r,Vector2.zero,28);
    }
    public void PauseGame(){if(!GameControls.Editing)Pause(true);}
    void Pause(bool value){if(panel==null)return;if(value&&!paused)previousScale=Time.timeScale;paused=value;panel.SetActive(value);Time.timeScale=value?0:previousScale;if(value)MobileInput.Clear();}
    void Update(){if(MobileInput.Active && menuButton!=null){float scale=canvas.GetComponent<Canvas>().scaleFactor;menuButton.anchoredPosition=new Vector2(-20-(Screen.width-Screen.safeArea.xMax)/scale,-20-(Screen.height-Screen.safeArea.yMax)/scale);}if(!GameControls.Editing && GameControls.Down(8))Pause(!paused);}
    void OnDisable(){if(canvas!=null)canvas.SetActive(false);if(paused)Time.timeScale=previousScale;paused=false;}
    void OnDestroy(){if(canvas!=null)Destroy(canvas);}
}
