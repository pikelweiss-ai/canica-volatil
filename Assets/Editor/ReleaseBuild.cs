using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Reproduible amb Unity -batchmode -executeMethod ReleaseBuild.BuildWindows / BuildLinux.
public static class ReleaseBuild
{
    public static void BuildWindows()=>Build(BuildTarget.StandaloneWindows64,"Windows/CanicaVolatil.exe");
    public static void BuildLinux()=>Build(BuildTarget.StandaloneLinux64,"Linux/CanicaVolatil.x86_64");
    static void Build(BuildTarget target,string output)
    {
        var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        if(scenes.Length==0 || !scenes[0].EndsWith("MainMenu.unity"))throw new Exception("MainMenu ha de ser la primera escena.");
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,target))throw new Exception("Falta el modul de compilacio: "+target);
        PlayerSettings.bundleVersion="1.0.0-prealpha";
        // Conservar companyName/productName: canviar-los mouria les preferencies dels jugadors.
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        string path="Builds/Pre-Alpha-v1.0.0/"+output;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        BuildReport report=BuildPipeline.BuildPlayer(scenes,path,target,BuildOptions.StrictMode);
        string summary=target+": "+report.summary.result+"; errors="+report.summary.totalErrors+"; bytes="+report.summary.totalSize;
        File.WriteAllText(Path.GetDirectoryName(path)+"/build-result.txt",summary);
        Debug.Log(summary);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception(summary);
    }
}