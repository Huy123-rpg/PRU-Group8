using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ScienceQuest.Biology;

namespace ScienceQuest.BiologyEditor
{
    /// <summary>
    /// Tạo scene Trung tâm giảng viên cho môn Sinh học.
    ///
    /// Scene được tạo tự động vì toàn bộ giao diện của TeacherManager được dựng bằng code,
    /// nên scene chỉ cần 1 Camera + 1 EventSystem là đủ.
    ///
    /// Chạy: menu "PRU/Sinh học/Tạo scene Trung tâm giảng viên"
    /// </summary>
    public static class TeacherCenterSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Sinh/TeacherCenter.unity";

        [MenuItem("PRU/Sinh học/Tạo scene Trung tâm giảng viên")]
        public static void BuildScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            // --- Camera ---
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.14f, 0.12f, 1f);
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            // --- EventSystem ---
            // Dự án dùng Input System nên không thêm InputModule ở đây;
            // InputModule sẽ được Unity thêm tự động khi mở scene trong Editor.
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));

            // --- TeacherManager ---
            GameObject teacherGo = new GameObject("TeacherManager");
            teacherGo.AddComponent<TeacherManager>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[TeacherCenterSceneBuilder] ✅ Đã tạo scene: {ScenePath}");
        }
    }
}
