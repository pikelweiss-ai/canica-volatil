using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Android es compila des del MATEIX projecte i cataleg que Windows/Linux.
public static class AlphaMobileBuild
{
    // El canvi d'Input necessita un reinici de l'Editor abans de compilar URP.
    public static void PrepareAndroid() => SetInput(0);
    public static void PreparePC() => SetInput(2);
    static void SetInput(int value)
    {
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue=value;
        settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets();
    }
    public static void BuildAndroid()
    {
        AlphaVisualBuilder.Validate(); AlphaContentBuilder.Validate(); NetContentBuilder.Build(); MobileInputChecks.Validate();
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input = settings.FindProperty("activeInputHandler");
        int originalInput = input.intValue;
        if(originalInput != 0) throw new Exception("Executa AlphaMobileBuild.PrepareAndroid i reinicia Unity abans de BuildAndroid (Tools/Build/build-alpha.ps1).");
        try
        {
            input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); // Input Manager; UI substituida per MobileBootstrap.
            PlayerSettings.bundleVersion = GameVersion.Package;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.pikelweiss.canicavolatil.mobile");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = 9;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.development = false;
            string output = $"Builds/AlphaReview/Android/CanicaVolatil-{GameVersion.Label.Replace(' ', '-')}-Android.apk";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.Android, BuildOptions.StrictMode);
            string result = "Android: " + report.summary.result + "; errors=" + report.summary.totalErrors;
            File.WriteAllText(Path.GetDirectoryName(output) + "/build-result.txt", result);
            Debug.Log("ALPHA_MOBILE: " + result);
            if (report.summary.result != BuildResult.Succeeded) throw new Exception(result);
            BuildCompatibility.Write(Path.GetDirectoryName(output), "Android");
        }
        finally
        {
            settings.Update(); settings.FindProperty("activeInputHandler").intValue = originalInput;
            settings.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets();
        }
    }
}
