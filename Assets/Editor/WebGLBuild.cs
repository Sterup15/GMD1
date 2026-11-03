using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Batch mode entry point, Unity has no command line flag for WebGL builds:
// Unity.exe -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod WebGLBuild.Build -logFile -
public static class WebGLBuild
{
    private const string OutputPath = "Builds/WebGL";

    [MenuItem("Tools/Build WebGL")]
    public static void Build()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes,
            locationPathName = OutputPath,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None
        });

        var summary = report.summary;
        Debug.Log($"WebGLBuild: {summary.result} in {summary.totalTime}, {summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors");

        if (Application.isBatchMode && summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
