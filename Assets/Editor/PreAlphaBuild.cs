using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

// La publicacio es manual; este menu nomes crea una build local ignorada per Git.
public static class PreAlphaBuild
{
    [MenuItem("Canica/Pre-Alpha/Provar armes cos a cos en Play")]
    public static void CheckMelee()
    {
        if (!EditorApplication.isPlaying) { Debug.LogWarning("Entra en una masmorra amb Play primer."); return; }
        PlayerAttack player = Object.FindFirstObjectByType<PlayerAttack>();
        if (player == null) { Debug.LogError("No hi ha jugador."); return; }
        var front = new GameObject("Prova frontal temporal");
        var back = new GameObject("Prova darrere temporal");
        try
        {
            Vector3 dir = player.AimDirection;
            front.transform.position = player.transform.position + dir;
            back.transform.position = player.transform.position - dir;
            front.AddComponent<CircleCollider2D>().isTrigger = true;
            back.AddComponent<CircleCollider2D>().isTrigger = true;
            var a = front.AddComponent<EnemyHealth>(); var b = back.AddComponent<EnemyHealth>();
            Physics2D.SyncTransforms();
            foreach (string file in new[] { "EspasaVidre", "SabreCorall", "MartellOrbital" })
            {
                WeaponData w = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Weapons/" + file + ".asset");
                if (w == null || !w.melee || w.icon == null) throw new System.Exception("Arma incompleta: " + file);
                a.currentHealth = b.currentHealth = 100;
                player.SendMessage("Shoot", w, SendMessageOptions.RequireReceiver);
                if (a.currentHealth != 100 - w.damage || b.currentHealth != (w.meleeRadial ? 100 - w.damage : 100))
                    throw new System.Exception("Arc o dany incorrecte: " + file + ". Prova en el centre d'una sala lliure.");
            }
            Debug.Log("PRE-ALPHA: 3 armes comprovades. Dany frontal i arcs comprovats, inclos martell circular.");
        }
        catch (System.Exception e) { Debug.LogError("PRE-ALPHA: " + e.Message); }
        finally { front.SetActive(false); back.SetActive(false); Object.Destroy(front); Object.Destroy(back); }
    }
    [MenuItem("Canica/Pre-Alpha/Crear build Windows v1.0.0")]
    public static void BuildWindows()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Ix de Play abans de crear la build."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0 || !scenes[0].EndsWith("MainMenu.unity"))
        { Debug.LogError("MainMenu ha de ser la primera escena activa del Build Settings."); return; }
        string previous = PlayerSettings.bundleVersion;
        try
        {
            PlayerSettings.bundleVersion = "1.0.0-prealpha";
            var report = BuildPipeline.BuildPlayer(scenes, "Builds/Pre-Alpha-v1.0.0/CanicaVolatil.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (report.summary.result != BuildResult.Succeeded) Debug.LogError("Build fallida: " + report.summary.result);
            else Debug.Log("Pre-Alpha creada en Builds/Pre-Alpha-v1.0.0. Cal provar l'executable abans de publicar.");
        }
        finally { PlayerSettings.bundleVersion = previous; }
    }
}