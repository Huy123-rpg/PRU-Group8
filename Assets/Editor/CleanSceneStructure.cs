#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tự động dọn dẹp cấu trúc tất cả các Scene trong dự án:
/// Trong mỗi file Scene, giữ lại DUY NHẤT Panel UI tương ứng và tự động XÓA tất cả các Panel UI thừa khác.
/// </summary>
[InitializeOnLoad]
public static class CleanSceneStructure
{
    static CleanSceneStructure()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("PRU_ScenesCleaned_V2", false))
            {
                SessionState.SetBool("PRU_ScenesCleaned_V2", true);
                CleanAllScenes();
            }
        };
    }

    [MenuItem("Tools/PRU/Clean Up All Scenes (Keep Only Target Panel)")]
    public static void CleanAllScenes()
    {
        string currentActiveScene = EditorSceneManager.GetActiveScene().path;

        CleanScene("Assets/Scenes/Scene1_Login.unity", "LoginScene", "LoginPanel");
        CleanScene("Assets/Scenes/Scene2_MainMenu.unity", "MenuGame", "MainMenuPanel");
        CleanScene("Assets/Scenes/Scene3_LessonType.unity", "LessonType", "LessonTypePanel");
        CleanScene("Assets/Scenes/Scene4_ChapterSelect.unity", "ChapterSelect", "ChapterSelectPanel");
        CleanScene("Assets/Scenes/Scene5_DogSelect.unity", "DogSelected", "DogSelect", "DogSelectPanel");
        CleanScene("Assets/Scenes/Scene7_Results.unity", "Results", "ResultsPanel");

        if (!string.IsNullOrEmpty(currentActiveScene) && System.IO.File.Exists(currentActiveScene))
        {
            EditorSceneManager.OpenScene(currentActiveScene, OpenSceneMode.Single);
        }

        Debug.Log("[CleanSceneStructure] 🎉 Đã dọn dẹp sạch sẽ tất cả các Scene! Mỗi Scene hiện chỉ giữ lại 1 Panel UI duy nhất.");
    }

    private static void CleanScene(string scenePath, params string[] keepPanelNames)
    {
        if (!System.IO.File.Exists(scenePath)) return;

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) return;

        string[] allPanelNames = new string[]
        {
            "LoginScene", "LoginPanel",
            "MenuGame", "MainMenuPanel",
            "LessonType", "LessonTypePanel",
            "ChapterSelect", "ChapterSelectPanel",
            "DogSelected", "DogSelect", "DogSelectPanel",
            "RacePanel", "DogRacer",
            "Results", "ResultsPanel"
        };

        GameObject[] rootObjects = scene.GetRootGameObjects();
        List<GameObject> objectsToDestroy = new List<GameObject>();

        foreach (GameObject root in rootObjects)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in children)
            {
                if (t == null) continue;

                string objName = t.name;
                bool isPanel = false;
                foreach (string pName in allPanelNames)
                {
                    if (objName.Equals(pName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        isPanel = true;
                        break;
                    }
                }

                if (isPanel)
                {
                    bool shouldKeep = false;
                    foreach (string keepName in keepPanelNames)
                    {
                        if (objName.Equals(keepName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            shouldKeep = true;
                            break;
                        }
                    }

                    if (shouldKeep)
                    {
                        t.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (!objectsToDestroy.Contains(t.gameObject))
                        {
                            objectsToDestroy.Add(t.gameObject);
                        }
                    }
                }
            }
        }

        // Tiến hành xóa các GameObject panel thừa
        foreach (GameObject go in objectsToDestroy)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        EditorSceneManager.SaveScene(scene);
    }
}
#endif
