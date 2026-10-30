#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Starts only the DogRacer local grading service; no UI is generated here.
[InitializeOnLoad]
internal static class DogRacerAIBackend
{
    private static bool startedThisPlay;

    static DogRacerAIBackend()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode) startedThisPlay = false;
        };
        EditorApplication.update += () =>
        {
            if (startedThisPlay || !EditorApplication.isPlaying || SceneManager.GetActiveScene().name != "Scene6_Race") return;
            startedThisPlay = true;
            if (Application.platform != RuntimePlatform.WindowsEditor) return;
            string script = Path.GetFullPath(Path.Combine(Application.dataPath, "../Backend/DogRacerAI/Start-DogRacerAI.ps1"));
            if (!File.Exists(script)) return;
            try
            {
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + script + "\" -NonInteractive",
                    WorkingDirectory = Path.GetDirectoryName(script),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                })) { }
            }
            catch (Exception)
            {
                UnityEngine.Debug.LogWarning("[DogRacer] Could not start AI backend. Run Backend/DogRacerAI/Start-DogRacerAI.ps1.");
            }
        };
    }
}
#endif
