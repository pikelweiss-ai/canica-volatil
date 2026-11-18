using UnityEngine;

// Alguns Android filtren UDP Wi-Fi sense MulticastLock; IP directa i relay no el necessiten.
public class AndroidLanSupport : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject wifiLock;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var go = new GameObject("Suport LAN Android");
        DontDestroyOnLoad(go); go.AddComponent<AndroidLanSupport>();
    }
    void Start() => Acquire();
    void Acquire()
    {
        try
        {
            if(wifiLock == null)
            {
                using(var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using(var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using(var wifi = context.Call<AndroidJavaObject>("getSystemService", "wifi"))
                    wifiLock = wifi.Call<AndroidJavaObject>("createMulticastLock", "CanicaLAN");
                wifiLock.Call("setReferenceCounted", false);
            }
            if(!wifiLock.Call<bool>("isHeld")) wifiLock.Call("acquire");
        }
        catch(System.Exception e) { Debug.LogWarning("LAN Wi-Fi: " + e.Message + ". Usa IP directa o Internet si no apareixen sales."); }
    }
    void Release() { if(wifiLock != null && wifiLock.Call<bool>("isHeld")) wifiLock.Call("release"); }
    void OnApplicationPause(bool pause) { if(pause)Release();else Acquire(); }
    void OnDestroy() { Release(); wifiLock?.Dispose(); wifiLock=null; }
#endif
}
