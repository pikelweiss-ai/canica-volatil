using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Canvis de pantalla amb confirmacio temporal; nomes es persisteixen en acceptar.
public class DisplaySettings : MonoBehaviour
{
    static DisplaySettings instance;
    public static bool Pending => instance != null && instance.confirmation != null;
    public static bool ConsumedEscape => escapeFrame==Time.frameCount;
    static int escapeFrame=-1;
    GameObject confirmation;
    TextMeshProUGUI countdown;
    int previousWidth,previousHeight,requestedWidth,requestedHeight;
    FullScreenMode previousMode,requestedMode;
    float deadline,previousScale;
    bool previousEditing;
    const string WidthKey="Display_Width",HeightKey="Display_Height",ModeKey="Display_Mode";
    Vector2Int lastWindow;
    bool lastFullscreen;
    bool switching;
    FullScreenMode targetMode;
    public static bool IsFullscreen => instance!=null && instance.switching ? instance.targetMode!=FullScreenMode.Windowed : Screen.fullScreen;
    public static Vector2Int NativeSize()
    {
        var d=Screen.mainWindowDisplayInfo;
        int w=d.width>0?d.width:Screen.currentResolution.width,h=d.height>0?d.height:Screen.currentResolution.height;
        return new Vector2Int(Mathf.Max(640,w),Mathf.Max(480,h));
    }
    void ApplyResolution(Vector2Int size,FullScreenMode mode)
    {
        StopAllCoroutines();switching=true;targetMode=mode;
        Screen.SetResolution(size.x,size.y,mode);
        StartCoroutine(SettleResolution(size,mode));
    }
    IEnumerator SettleResolution(Vector2Int size,FullScreenMode mode)
    {
        yield return null;yield return null;
        float until=Time.realtimeSinceStartup+5;
        while(Time.realtimeSinceStartup<until && ((mode==FullScreenMode.Windowed?Screen.fullScreen:!Screen.fullScreen) || Screen.width!=size.x || Screen.height!=size.y))yield return null;
        if((mode==FullScreenMode.Windowed && Screen.fullScreen)||(mode!=FullScreenMode.Windowed && !Screen.fullScreen))
        {
            Debug.LogWarning("No s'ha pogut aplicar el mode de pantalla; restaurem l'anterior.");
            switching=false;if(Pending)Revert();yield break;
        }
        if(mode==FullScreenMode.Windowed)yield return FitAndCenter();
        switching=false;
        if(Pending)deadline=Time.realtimeSinceStartup+15;
    }
    public static Vector2Int FitWindow(Vector2Int requested,Vector2Int workArea)
    {
        float maxW=Mathf.Max(320,workArea.x-24),maxH=Mathf.Max(240,workArea.y-72);
        float scale=Mathf.Min(1,maxW/Mathf.Max(1,requested.x),maxH/Mathf.Max(1,requested.y));
        return new Vector2Int(Mathf.Max(1,Mathf.FloorToInt(requested.x*scale)),Mathf.Max(1,Mathf.FloorToInt(requested.y*scale)));
    }
    static Vector2Int WindowSize(Vector2Int requested)
    {
        var area=Screen.mainWindowDisplayInfo.workArea;
        var available=area.width>0 && area.height>0 ? new Vector2Int(area.width,area.height) : new Vector2Int(Screen.currentResolution.width,Screen.currentResolution.height);
        return FitWindow(requested,available);
    }
    IEnumerator FitAndCenter()
    {
        yield return null;yield return null;
        if(targetMode!=FullScreenMode.Windowed || Screen.fullScreen)yield break;
        var size=WindowSize(new Vector2Int(Screen.width,Screen.height));
        if(size.x!=Screen.width || size.y!=Screen.height)Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
        yield return null;
        var display=Screen.mainWindowDisplayInfo;var area=display.workArea;
        if(targetMode==FullScreenMode.Windowed && !Screen.fullScreen && area.width>0 && area.height>0)Screen.MoveMainWindowTo(display,new Vector2Int(area.x+Mathf.Max(0,(area.width-size.x)/2),area.y+Mathf.Max(0,(area.height-size.y)/2)));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if(MobileInput.Active) return; // Android controla la pantalla i no te finestres redimensionables.
        instance=new GameObject("Configuracio de pantalla",typeof(DisplaySettings)).GetComponent<DisplaySettings>();
        DontDestroyOnLoad(instance.gameObject);
        var args=System.Environment.GetCommandLineArgs();
        bool commandLine=System.Array.IndexOf(args,"-screen-width")>=0 || System.Array.IndexOf(args,"-screen-fullscreen")>=0;
        if(PlayerPrefs.HasKey(WidthKey) && !commandLine)
        {
            bool full=PlayerPrefs.GetInt(ModeKey,0)==1;
            var size=full ? NativeSize() : WindowSize(Closest(PlayerPrefs.GetInt(WidthKey),PlayerPrefs.GetInt(HeightKey)));
            instance.ApplyResolution(size,full ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
        else { instance.targetMode=Screen.fullScreenMode;instance.ApplyResolution(new Vector2Int(Screen.width,Screen.height),Screen.fullScreenMode); }
    }
    public static List<Vector2Int> Options()
    {
        var native=Screen.currentResolution;
        int maxW=Mathf.Max(native.width,Screen.width),maxH=Mathf.Max(native.height,Screen.height);
        return Supported(maxW,maxH,new Vector2Int(Screen.width,Screen.height));
    }
    public static List<Vector2Int> Supported(int maxW,int maxH,Vector2Int current)
    {
        var sizes=new List<Vector2Int>();
        foreach(var s in new[] { new Vector2Int(800,600),new Vector2Int(960,540),new Vector2Int(1024,768),new Vector2Int(1280,720),new Vector2Int(1280,800),new Vector2Int(1280,960),new Vector2Int(1366,768),new Vector2Int(1600,900),new Vector2Int(1920,1080),new Vector2Int(2560,1440),new Vector2Int(3440,1440),new Vector2Int(3840,2160),current,new Vector2Int(maxW,maxH) })
            if(s.x>=640 && s.y>=480 && s.x<=maxW && s.y<=maxH && !sizes.Contains(s)) sizes.Add(s);
        if(sizes.Count==0) sizes.Add(new Vector2Int(Mathf.Max(1,maxW),Mathf.Max(1,maxH)));
        sizes.Sort((a,b)=>((long)a.x*a.y).CompareTo((long)b.x*b.y));return sizes;
    }
    static Vector2Int Closest(int w,int h)
    {
        var sizes=Options();var best=sizes[0];long score=long.MaxValue;
        foreach(var s in sizes) { long distance=System.Math.Abs((long)s.x-w)+System.Math.Abs((long)s.y-h);if(distance<score){score=distance;best=s;} }
        return best;
    }
    public static void Request(Vector2Int size,bool fullscreen)
    {
        if(instance == null || Pending) return;
        instance.previousWidth=Screen.width;instance.previousHeight=Screen.height;instance.previousMode=Screen.fullScreenMode;
        var safe=fullscreen ? NativeSize() : WindowSize(Closest(size.x,size.y));instance.requestedWidth=safe.x;instance.requestedHeight=safe.y;
        instance.requestedMode=fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        instance.previousScale=Time.timeScale;instance.previousEditing=GameControls.Editing;
        Time.timeScale=0;GameControls.Editing=true;
        instance.OpenConfirmation();instance.deadline=float.PositiveInfinity;
        instance.ApplyResolution(safe,instance.requestedMode);
    }
    public static void Confirm()
    {
        if(!Pending || instance.switching) return;
        PlayerPrefs.SetInt(WidthKey,Screen.width);PlayerPrefs.SetInt(HeightKey,Screen.height);
        PlayerPrefs.SetInt(ModeKey,instance.requestedMode==FullScreenMode.Windowed ? 0 : 1);PlayerPrefs.Save();instance.CloseConfirmation();
    }
    public static void Revert()
    {
        if(!Pending) return;
        instance.ApplyResolution(new Vector2Int(instance.previousWidth,instance.previousHeight),instance.previousMode);instance.CloseConfirmation();
    }
    void Update()
    {
        var current=new Vector2Int(Screen.width,Screen.height);
        if(!switching && !Pending && (current!=lastWindow || Screen.fullScreen!=lastFullscreen))
        {
            lastWindow=current;lastFullscreen=Screen.fullScreen;
            targetMode=Screen.fullScreenMode;
            // Les mides triades al menu es limiten en Request/FitAndCenter.
            // Els canvis natius (maximitzar/arrossegar) els gestiona el sistema operatiu:
            // tornar a reservar marges aci desfaria immediatament la maximitzacio.
        }
        if(Pending)
        {
            countdown.text=switching ? "Aplicant la configuració..." : "Tornarem als ajustos anteriors en "+Mathf.Max(0,Mathf.CeilToInt(deadline-Time.realtimeSinceStartup))+" s";
            if(Input.GetKeyDown(KeyCode.Escape)) { escapeFrame=Time.frameCount;Revert(); }
            else if(Time.realtimeSinceStartup>=deadline) Revert();
            else if(Input.GetKeyDown(KeyCode.Return)) Confirm();
        }
        else if(Input.GetKeyDown(KeyCode.F11) && !GameControls.Blocked && !switching) Request(new Vector2Int(Screen.width,Screen.height),!IsFullscreen); // Durant una transicio, Screen.fullScreen encara no es fiable
    }
    void CloseConfirmation()
    {
        confirmation.SetActive(false);Destroy(confirmation);confirmation=null;
        GameControls.Editing=previousEditing;Time.timeScale=previousScale;
    }
    void OpenConfirmation()
    {
        confirmation=new GameObject("Confirmar pantalla",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c=confirmation.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=200;
        var sc=confirmation.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var bg=Box(confirmation.transform,Vector2.zero,Vector2.zero);bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;bg.GetComponent<Image>().color=new Color(.02f,.035f,.055f,.98f);
        Text(confirmation.transform,"MANTINDRE ESTOS AJUSTOS?",new Vector2(0,150),38);
        countdown=Text(confirmation.transform,"",new Vector2(0,40),25);
        Button("MANTINDRE",new Vector2(-260,-110),Confirm);Button("TORNAR ARRERE",new Vector2(260,-110),Revert);
        Text(confirmation.transform,"Intro: mantindre · Esc: tornar arrere",new Vector2(0,-240),22);
    }
    RectTransform Box(Transform parent,Vector2 pos,Vector2 size)
    {
        var r=new GameObject("Panell",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=Vector2.one*.5f;r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    TextMeshProUGUI Text(Transform parent,string label,Vector2 pos,float size)
    {
        var t=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();t.transform.SetParent(parent,false);t.font=GameTypography.Font;t.text=label;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.rectTransform.anchorMin=t.rectTransform.anchorMax=Vector2.one*.5f;t.rectTransform.anchoredPosition=pos;t.rectTransform.sizeDelta=new Vector2(1300,90);return t;
    }
    void Button(string label,Vector2 pos,UnityEngine.Events.UnityAction action)
    {
        var r=Box(confirmation.transform,pos,new Vector2(450,80));r.GetComponent<Image>().color=new Color(.13f,.32f,.4f);r.gameObject.AddComponent<Button>().onClick.AddListener(action);Text(r,label,Vector2.zero,27);
    }
}
