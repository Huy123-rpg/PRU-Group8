using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Author the mode scene through Unity's scene serializer, preserving the open scene.</summary>
[InitializeOnLoad]
public static class DogRaceLanSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene5_PlayMode.unity";
    static DogRaceLanSceneSetup() { EditorApplication.delayCall += EnsureScene; }
    [MenuItem("Tools/PRU/Setup DogRacer LAN Scene")]
    public static void EnsureScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!File.Exists(ScenePath))
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.06f, .12f, .16f);
            camera.gameObject.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        }
        AutoSyncBuildScenes.Sync();
    }
}
