using UnityEngine;

public enum SoundCategory { Weapons, Enemies, Effects, Music }
public static class GameSettings
{
    public static float MapSize { get=>Mathf.Clamp(PlayerPrefs.GetFloat("Map_Size",200),140,360);set{PlayerPrefs.SetFloat("Map_Size",value);PlayerPrefs.Save();} }
    public static float MapRange { get=>Mathf.Clamp(PlayerPrefs.GetFloat("Map_Range",1.2f),1,2);set{PlayerPrefs.SetFloat("Map_Range",value);PlayerPrefs.Save();} }
    public static float Master { get=>Mathf.Clamp01(PlayerPrefs.GetFloat("Audio_Master",1));set{PlayerPrefs.SetFloat("Audio_Master",value);AudioListener.volume=value;PlayerPrefs.Save();} }
    public static float Volume(SoundCategory channel)=>Mathf.Clamp01(PlayerPrefs.GetFloat("Audio_"+channel,1));
    public static void Volume(SoundCategory channel,float value){PlayerPrefs.SetFloat("Audio_"+channel,Mathf.Clamp01(value));PlayerPrefs.Save();GameAudio.Refresh();}
}