using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudLayout : MonoBehaviour
{
    public static readonly string[] Positions={"Dalt a l'esquerra","Davall del mapa","Baix a l'esquerra","Damunt de la hotbar"};
    public static int HealthPosition { get=>Mathf.Clamp(PlayerPrefs.GetInt("HUD_Health",0),0,3);set{PlayerPrefs.SetInt("HUD_Health",Mathf.Clamp(value,0,3));PlayerPrefs.Save();} }
    public static int CoinPosition { get=>Mathf.Clamp(PlayerPrefs.GetInt("HUD_Coins",0),0,3);set{PlayerPrefs.SetInt("HUD_Coins",Mathf.Clamp(value,0,3));PlayerPrefs.Save();} }
    GameObject canvas;
    RectTransform safe,healthGroup,coinGroup;
    HotbarUI hotbar;
    RectTransform hotbarSlot;
    readonly Vector3[] corners=new Vector3[4];
    void Start()
    {
        canvas=new GameObject("HUD personalitzable",typeof(Canvas),typeof(CanvasScaler));
        var c=canvas.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=25;
        var sc=canvas.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        safe=Group(canvas.transform,"Zona segura",Vector2.zero);
        healthGroup=Group(safe,"Vida",new Vector2(220,44));coinGroup=Group(safe,"Monedes",new Vector2(160,44));
        var health=GetComponent<PlayerHealth>();
        if(health!=null && health.hearts!=null)
            for(int i=0;i<health.hearts.Length;i++) if(health.hearts[i]!=null) Place(health.hearts[i].rectTransform,healthGroup,new Vector2(i*42,0),new Vector2(36,36));
        var counter=CoinCounter.instance;
        if(counter!=null && counter.coinText!=null)
        {
            Place(counter.coinText.rectTransform,coinGroup,new Vector2(44,0),new Vector2(112,44));
            counter.coinText.alignment=TextAlignmentOptions.Left;counter.coinText.enableAutoSizing=true;counter.coinText.fontSizeMin=18;counter.coinText.fontSizeMax=32;
        }
        var icon=GameObject.Find("IconaMonedaUI");if(icon!=null && icon.transform is RectTransform rect) Place(rect,coinGroup,Vector2.zero,new Vector2(34,34));
        Layout();
    }
    static RectTransform Group(Transform parent,string name,Vector2 size)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.sizeDelta=size;return r;
    }
    static void Place(RectTransform item,Transform parent,Vector2 position,Vector2 size)
    {
        item.SetParent(parent,false);item.anchorMin=item.anchorMax=new Vector2(0,.5f);item.pivot=new Vector2(0,.5f);item.anchoredPosition=position;item.sizeDelta=size;item.localScale=Vector3.one;
        var g=item.GetComponent<Graphic>();if(g!=null)g.raycastTarget=false;
    }
    void LateUpdate(){Layout();}
    void Layout()
    {
        if(safe==null)return;var area=Screen.safeArea;
        safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;
        Anchor(healthGroup,HealthPosition,false);Anchor(coinGroup,CoinPosition,true);
        if(hotbar==null)hotbar=Object.FindFirstObjectByType<HotbarUI>();
        if(hotbar!=null)
        {
            hotbar.SetHudAbove(HealthPosition==3 || CoinPosition==3);
            if(hotbarSlot==null){var slot=hotbar.GetComponentInChildren<SlotInput>();if(slot!=null)hotbarSlot=(RectTransform)slot.transform;}
            if(hotbarSlot!=null)
            {
                hotbarSlot.GetWorldCorners(corners);float factor=canvas.GetComponent<Canvas>().scaleFactor;
                float center=hotbar.transform.position.x,y=corners[1].y+4*factor;
                if(MobileInput.Active)
                {
                    var heals=Object.FindFirstObjectByType<HealBarUI>();
                    var slot=heals!=null?heals.GetComponentInChildren<SlotInput>():null;
                    if(slot!=null){((RectTransform)slot.transform).GetWorldCorners(corners);y=corners[1].y+4*factor;}
                }
                if(HealthPosition==3)healthGroup.position=new Vector3(center,y,0);
                if(CoinPosition==3)coinGroup.position=new Vector3(center-(HealthPosition==3?202*factor:0),y,0);
            }
        }
    }
    static void Anchor(RectTransform r,int position,bool coins)
    {
        if(position==0){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(18,coins?-66:-18);}
        else if(position==1){r.anchorMin=r.anchorMax=r.pivot=Vector2.one;r.anchoredPosition=new Vector2(-20,-140-GameSettings.MapSize*190/240-(coins?48:0));}
        else if(position==2){r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(18,coins?20:68);}
        else {r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,96);}
    }
    void OnDestroy(){if(canvas!=null)Destroy(canvas);}
}
