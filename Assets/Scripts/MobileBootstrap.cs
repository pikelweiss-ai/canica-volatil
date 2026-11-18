using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Les escenes de PC guarden InputSystemUIInputModule. Android usa Input Manager:
// substituir el modul a l'arrancada manté els menús i el multitàctil operatius.
public static class MobileBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if(!MobileInput.Active) return;
        SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded;
    }
    static void Loaded(Scene scene,LoadSceneMode mode)
    {
        MobileInput.Clear();
        var system = Object.FindFirstObjectByType<EventSystem>();
        if(system == null) system = new GameObject("EventSystem tactil",typeof(EventSystem)).GetComponent<EventSystem>();
        foreach(var module in system.GetComponents<BaseInputModule>())
            if(!(module is StandaloneInputModule)) { module.enabled = false; Object.Destroy(module); }
        var legacy = system.GetComponent<StandaloneInputModule>();
        if(legacy == null) legacy = system.gameObject.AddComponent<StandaloneInputModule>();
        legacy.enabled = true; EventSystem.current = system;
    }
}
