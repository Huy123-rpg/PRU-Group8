#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tự động đăng ký toàn bộ 7 Scene vào Unity Editor Build Settings theo đúng thứ tự luồng game.
/// </summary>
[InitializeOnLoad]
public static class SceneBuildSettingsSetup
{
    static SceneBuildSettingsSetup()
    {
        EditorApplication.delayCall += EnsureScenesInBuildSettings;
    }

    [MenuItem("Tools/PRU/Auto Setup Build Settings Scenes")]
    public static void EnsureScenesInBuildSettings()
    {
        AutoSyncBuildScenes.Sync();
    }
}
#endif
