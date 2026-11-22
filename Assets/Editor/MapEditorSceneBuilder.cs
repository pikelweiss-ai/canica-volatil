using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MapEditorSceneBuilder
{
    const string Path = "Assets/Scenes/Creador.unity";
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camera.tag = "MainCamera";
        camera.GetComponent<Camera>().orthographic = true;
        camera.GetComponent<Camera>().backgroundColor = new Color(.02f, .04f, .07f);
        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        new GameObject("Creador").AddComponent<MapEditor>();
        EditorSceneManager.SaveScene(scene, Path);
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == Path)) scenes.Add(new EditorBuildSettingsScene(Path, true));
        else for (int i = 0; i < scenes.Count; i++) if (scenes[i].path == Path) scenes[i] = new EditorBuildSettingsScene(Path, true);
        EditorBuildSettings.scenes = scenes.ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
    }
}
