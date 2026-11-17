using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Cada categoria conserva el volum propi de la font; el master s'aplica una sola vegada.
public static class GameAudio
{
    static readonly Dictionary<SoundCategory,AudioSource> players=new Dictionary<SoundCategory,AudioSource>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        players.Clear();AudioListener.volume=GameSettings.Master;
        SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;
    }
    static void Loaded(Scene scene,LoadSceneMode mode)
    {
        foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            TrackUnassigned(source);
    }
    static void TrackUnassigned(AudioSource source)
    {
        foreach(var gain in source.GetComponents<AudioCategoryGain>())if(gain.source==source)return;
        Track(source,source.loop?SoundCategory.Music:SoundCategory.Effects);
    }    public static void Track(AudioSource source,SoundCategory category)
    {
        if(source==null)return;
        AudioCategoryGain gain=null;
        foreach(var candidate in source.GetComponents<AudioCategoryGain>()) if(candidate.source==source){gain=candidate;break;}
        if(gain==null){gain=source.gameObject.AddComponent<AudioCategoryGain>();gain.source=source;gain.baseVolume=source.volume;}
        gain.category=category;gain.Apply();
    }
    public static void Refresh(){foreach(var gain in Object.FindObjectsByType<AudioCategoryGain>(FindObjectsInactive.Include,FindObjectsSortMode.None))gain.Apply();}
    public static void Play(AudioClip clip,float volume=0.6f,SoundCategory category=SoundCategory.Effects,float pitchVariation=0f)
    {
        if(clip==null)return;
        if(!players.TryGetValue(category,out var source)||source==null)
        {
            var go=new GameObject("Audio "+category);Object.DontDestroyOnLoad(go);
            source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;players[category]=source;Track(source,category);
        }
        source.pitch=1+Random.Range(-pitchVariation,pitchVariation);source.PlayOneShot(clip,volume);
    }
    public static void Enemy(AudioClip clip,float volume=0.6f)=>Play(clip,volume,SoundCategory.Enemies);
}

public class AudioCategoryGain : MonoBehaviour
{
    public AudioSource source;public SoundCategory category;public float baseVolume=1;
    public void Apply(){if(source!=null)source.volume=baseVolume*GameSettings.Volume(category);}
}