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
    const string TestPath = "Assets/Scenes/MapaProva.unity";
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
        var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var testCamera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); testCamera.tag = "MainCamera"; testCamera.GetComponent<Camera>().orthographic = true;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        new GameObject("MapaProva").AddComponent<MapPlaytest>();
        EditorSceneManager.SaveScene(testScene, TestPath);
        var scenes = EditorBuildSettings.scenes.ToList();
        foreach (string path in new[] { Path, TestPath })
            if (!scenes.Any(s => s.path == path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            else for (int i = 0; i < scenes.Count; i++) if (scenes[i].path == path) scenes[i] = new EditorBuildSettingsScene(path, true);
        EditorBuildSettings.scenes = scenes.ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
    }
}
