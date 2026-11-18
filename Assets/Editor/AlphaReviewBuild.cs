using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AlphaReviewBuild
{
    public static void BuildWindows() { AlphaVisualBuilder.BuildAssets(); AlphaSettingsChecks.Validate(); Build(BuildTarget.StandaloneWindows64,"Windows/CanicaAlphaReview.exe"); }
    public static void BuildLinux() { AlphaVisualBuilder.Validate(); AlphaSettingsChecks.Validate(); Build(BuildTarget.StandaloneLinux64,"Linux/CanicaAlphaReview.x86_64"); }
    static void Build(BuildTarget target,string output)
    {
        PlayerSettings.bundleVersion=GameVersion.Package;
        PlayerSettings.resizableWindow=true;
        PlayerSettings.allowFullscreenSwitch=true;
        var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        string path="Builds/AlphaReview/"+output;Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(scenes,path,target,BuildOptions.StrictMode);
        string result=target+": "+report.summary.result+"; errors="+report.summary.totalErrors;
        File.WriteAllText(Path.GetDirectoryName(path)+"/build-result.txt",result);Debug.Log("ALPHA_REVIEW: "+result);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception(result);
    }
}
