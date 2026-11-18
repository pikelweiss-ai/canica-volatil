using UnityEngine;

public static class PresentationSettings
{
    public static readonly int[] Limits={0,60,120,144,165,240,360};
    public static bool VSync { get=>PlayerPrefs.GetInt("Video_VSync",1)!=0;set{PlayerPrefs.SetInt("Video_VSync",value?1:0);Apply();PlayerPrefs.Save();} }
    public static int Limit { get=>PlayerPrefs.GetInt("Video_FPS",0);set{PlayerPrefs.SetInt("Video_FPS",value);Apply();PlayerPrefs.Save();} }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Apply() { QualitySettings.vSyncCount=MobileInput.Active?0:VSync?1:0;Application.targetFrameRate=MobileInput.Active?(Limit==30?30:60):VSync || Limit==0 ? -1 : Limit; }
}
