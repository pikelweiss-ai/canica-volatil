using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MobileBuild
{
    public static void BuildAndroid()
    {
        PlayerSettings.companyName = "Pikelweiss";
        PlayerSettings.productName = "Canica Volatil Mobile";
        PlayerSettings.bundleVersion = "1.0.0-mobile-prealpha";
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/pixil-frame-0.png");
        if(icon != null) PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[] { icon });
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.pikelweiss.canicavolatil.mobile");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.development = false;
        Build(BuildTarget.Android,"Builds/Android/CanicaVolatil-Mobile-v1.0.0.apk");
    }
    public static void BuildPreview() => Build(BuildTarget.StandaloneWindows64,"Builds/MobilePreview/CanicaMobilePreview.exe");
    static void Build(BuildTarget target,string path)
    {
        MobileInputChecks.Validate();
        var scenes = EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        if(scenes.Length == 0 || !scenes[0].EndsWith("MainMenu.unity")) throw new Exception("MainMenu ha de ser la primera escena.");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report = BuildPipeline.BuildPlayer(scenes,path,target,BuildOptions.StrictMode);
        string summary = target + ": " + report.summary.result + "; errors=" + report.summary.totalErrors + "; bytes=" + report.summary.totalSize;
        Debug.Log(summary); File.WriteAllText(Path.GetDirectoryName(path)+"/build-result.txt",summary);
        if(report.summary.result != BuildResult.Succeeded) throw new Exception(summary);
    }
}
