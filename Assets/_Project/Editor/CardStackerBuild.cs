using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Hatdon.CardStacker.Editor
{
    public static class CardStackerBuild
    {
        public static void WindowsPreview()
        {
            var args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-buildOutput");
            if (option < 0 || option + 1 >= args.Length) throw new ArgumentException("Supply -buildOutput EXE path.");
            string output = Path.GetFullPath(args[option + 1]);
            bool capture = Array.IndexOf(args, "-captureBuild") >= 0;
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/CardStacker.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = capture ? BuildOptions.None : BuildOptions.Development,
                extraScriptingDefines = capture ? new[] { "HATDON_CAPTURE" } : Array.Empty<string>()
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows preview build failed: " + report.summary.result);
            Debug.Log("HATDON_BUILD_OK: " + output);
        }
    }
}
