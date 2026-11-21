using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NetBot
{
    IEnumerator Audio140()
    {
        yield return null;
        var library = Resources.Load<MobSounds>("MobSounds");
        if (library == null || library.clips.Length != 23) { Fail("falten clips de mobs"); yield break; }
        var signatures = new HashSet<string>();
        foreach (var clip in library.clips)
        {
            clip.LoadAudioData();
            yield return WaitFor(() => clip.loadState == AudioDataLoadState.Loaded, 5, "audio carregat: " + clip.name);
            if (failed) yield break;
            var data = new float[clip.samples * clip.channels];
            if (!clip.GetData(data, 0)) { Fail("no es pot llegir " + clip.name); yield break; }
            double energy = 0; float peak = 0;
            foreach (float sample in data) { energy += sample * sample; peak = Mathf.Max(peak, Mathf.Abs(sample)); }
            if (peak < .001f || energy < .001) { Fail("clip en silenci: " + clip.name); yield break; }
            if (peak >= .99f) { Fail("clip saturat en importar: " + clip.name); yield break; }
            signatures.Add(data.Length + ":" + energy.ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
            Ok("so no buit: " + clip.name + ", pic=" + peak.ToString("F3"));
        }
        if (signatures.Count != 23) { Fail("clips duplicats"); yield break; }
        bool hadVolume = PlayerPrefs.HasKey("Audio_Enemies"); float saved = GameSettings.Volume(SoundCategory.Enemies);
        var go = new GameObject("Prova volum Enemics"); var source = go.AddComponent<AudioSource>(); source.volume = .8f;
        try
        {
            GameAudio.Track(source, SoundCategory.Enemies);
            GameSettings.Volume(SoundCategory.Enemies, 0f);
            if (!Mathf.Approximately(source.volume, 0f)) { Fail("el volum Enemics no silencia"); yield break; }
            GameSettings.Volume(SoundCategory.Enemies, .5f);
            if (!Mathf.Approximately(source.volume, .4f)) { Fail("el volum Enemics no escala"); yield break; }
            Ok("categoria Enemics: silenci i volum al 50 %");
        }
        finally
        {
            GameSettings.Volume(SoundCategory.Enemies, saved);
            if (!hadVolume) { PlayerPrefs.DeleteKey("Audio_Enemies"); PlayerPrefs.Save(); }
            Destroy(go);
        }
        Pass();
    }
}
