using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

public class GitSubmoduleTools
{
    [MenuItem("Tools/Git/Update Submodules")]
    public static void UpdateSubmodules()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        ProcessStartInfo info = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "submodule update --remote --merge --recursive",
            WorkingDirectory = projectRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Process process = Process.Start(info);

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        Debug.Log(output);
        if (!string.IsNullOrEmpty(error))
            Debug.LogError(error);

        AssetDatabase.Refresh();
    }
}