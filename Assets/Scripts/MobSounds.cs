using UnityEngine;

public class MobSounds : ScriptableObject
{
    public AudioClip[] clips;
    static MobSounds library;
    public static AudioClip Get(string id)
    {
        if (library == null) library = Resources.Load<MobSounds>("MobSounds");
        if (library == null || library.clips == null) return null;
        foreach (var clip in library.clips) if (clip != null && clip.name == id) return clip;
        return null;
    }
    public static void Play(string id, Vector2 position, float volume = .5f)
    {
        if (NetCoop.IsClient) return;
        PlayLocal(id, position, volume);
        NetCoop.EnemySound(id, position, volume);
    }
    public static void PlayLocal(string id, Vector2 position, float volume)
    {
        var camera = Camera.main;
        float distance = camera == null ? 0 : Vector2.Distance(camera.transform.position, position);
        GameAudio.Enemy(Get(id), Mathf.Clamp01(volume) * Mathf.Clamp01(1f - distance / 22f));
    }
}
