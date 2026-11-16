using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Menú: Canica > Capturar miniatures dels mapes
// Obri cada escena del Build Settings (menys MainMenu), fa una foto amb la càmera
// centrada al jugador i la guarda a Assets/Resources/MapPreviews/<escena>.png.
// El selector de mapes del menú les carrega soles.
public static class MapPreviewCapture
{
    const int Width = 800;
    const int Height = 450; // 16:9
    const string Folder = "Assets/Resources/MapPreviews";

    [MenuItem("Canica/Capturar miniatures dels mapes")]
    public static void CaptureAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string originalScene = EditorSceneManager.GetActiveScene().path;
        Directory.CreateDirectory(Folder);
        int captured = 0;

        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;

            string sceneName = Path.GetFileNameWithoutExtension(buildScene.path);
            if (sceneName == "MainMenu") continue;

            EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
            if (cam == null)
            {
                Debug.LogWarning("[Miniatures] " + sceneName + ": no hi ha càmera, me la salte.");
                continue;
            }

            // Centrem la càmera en el jugador, com quan comença la partida
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector3 p = player.transform.position;
                cam.transform.position = new Vector3(p.x, p.y, cam.transform.position.z);
            }

            Texture2D image = Render(cam);
            string path = Folder + "/" + sceneName + ".png";
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            Debug.Log("[Miniatures] Guardada " + path);
            captured++;
        }

        // Tornem a l'escena on estàvem (sense guardar els moviments de càmera)
        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("Miniatures dels mapes", captured + " miniatures guardades a " + Folder, "OK");
    }

    static Texture2D Render(Camera cam)
    {
        RenderTexture rt = new RenderTexture(Width, Height, 24);
        float oldAspect = cam.aspect;
        cam.aspect = (float)Width / Height;

        RenderPipeline.StandardRequest request = new RenderPipeline.StandardRequest();
        if (RenderPipeline.SupportsRenderRequest(cam, request))
        {
            request.destination = rt;
            RenderPipeline.SubmitRenderRequest(cam, request);
        }
        else
        {
            RenderTexture oldTarget = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = oldTarget;
        }

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        tex.Apply();
        RenderTexture.active = previous;

        cam.aspect = oldAspect;
        rt.Release();
        Object.DestroyImmediate(rt);
        return tex;
    }
}
