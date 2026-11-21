using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MobAudioBuilder
{
    [MenuItem("Canica/Audio/Importa sorolls v1.4")]
    public static void Build()
    {
        const string folder = "Assets/Audio/Mobs";
        Directory.CreateDirectory(folder);
        foreach (var source in Directory.GetFiles("ArtNou_v1.4/Sorolls", "*.wav"))
        {
            var dest = folder + "/" + Path.GetFileName(source);
            if (!File.Exists(dest)) File.Copy(source, dest);
        }
        AssetDatabase.Refresh();
        foreach (var audioPath in Directory.GetFiles(folder, "*.wav"))
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(audioPath.Replace('\\', '/'));
            var settings = importer.defaultSampleSettings;
            if (settings.compressionFormat == AudioCompressionFormat.PCM) continue;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        }
        var clips = Directory.GetFiles(folder, "*.wav").OrderBy(p => p).Select(p => AssetDatabase.LoadAssetAtPath<AudioClip>(p.Replace('\\', '/'))).ToArray();
        if (clips.Length != 23 || clips.Any(c => c == null)) throw new Exception("Falten sorolls v1.4");
        const string path = "Assets/Resources/MobSounds.asset";
        var library = AssetDatabase.LoadAssetAtPath<MobSounds>(path);
        if (library == null) { library = ScriptableObject.CreateInstance<MobSounds>(); AssetDatabase.CreateAsset(library, path); }
        library.clips = clips; EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
        Debug.Log("MOB_AUDIO: 23 clips importats");
    }
}
