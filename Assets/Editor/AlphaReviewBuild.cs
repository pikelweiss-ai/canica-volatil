using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AlphaReviewBuild
{
    internal static string OutputRoot
    {
        get
        {
            string root = Environment.GetEnvironmentVariable("CANICA_ALPHA_BUILD_ROOT");
            return string.IsNullOrEmpty(root) ? "Builds/AlphaReview" : root;
        }
    }

    public static void BuildWindows() { AlphaVisualBuilder.BuildAssets(); AlphaContentBuilder.Build(); V141ContentBuilder.Build(); AlphaSettingsChecks.Validate(); MobileInputChecks.Validate(); Build(BuildTarget.StandaloneWindows64,"Windows/CanicaAlphaReview.exe"); }
    public static void BuildLinux() { AlphaVisualBuilder.Validate(); AlphaContentBuilder.Validate(); V141ContentBuilder.Build(); AlphaSettingsChecks.Validate(); Build(BuildTarget.StandaloneLinux64,"Linux/CanicaAlphaReview.x86_64"); }
    static void Build(BuildTarget target,string output)
    {
        PlayerSettings.bundleVersion=GameVersion.Package;
        PlayerSettings.resizableWindow=true;
        PlayerSettings.allowFullscreenSwitch=true;
        var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        string path=Path.Combine(OutputRoot,output);Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(scenes,path,target,BuildOptions.StrictMode);
        string result=target+": "+report.summary.result+"; errors="+report.summary.totalErrors;
        File.WriteAllText(Path.GetDirectoryName(path)+"/build-result.txt",result);Debug.Log("ALPHA_REVIEW: "+result);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception(result);
        BuildCompatibility.Write(Path.GetDirectoryName(path), target.ToString());
        if(target==BuildTarget.StandaloneLinux64)
            File.WriteAllText(Path.GetDirectoryName(path)+"/Jugar.sh","#!/usr/bin/env bash\ncd -- \"$(dirname -- \"$0\")\" || exit 1\nchmod u+x ./CanicaAlphaReview.x86_64\nexec ./CanicaAlphaReview.x86_64 \"$@\"\n");
    }
}
