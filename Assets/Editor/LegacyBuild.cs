using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Compila la versió de 2025 per a Windows i Linux des del terminal.
public static class LegacyBuild
{
    static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/GameScene.unity",
    };

    static void Prepare()
    {
        PlayerSettings.productName = "Canica Volàtil 2025";
        PlayerSettings.companyName = "pikelweiss";
        PlayerSettings.bundleVersion = "2025.0";
    }

    static void Build(BuildTarget target, string path)
    {
        Prepare();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = Scenes,
            target = target,
            locationPathName = path,
            options = BuildOptions.None,
        });
        Debug.Log($"[LegacyBuild] {target}: {report.summary.result}, {report.summary.totalErrors} errors");
        if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }

    public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/CanicaVolatil2025.exe");
    public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Builds/Linux/CanicaVolatil2025.x86_64");
}
