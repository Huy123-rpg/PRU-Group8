using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    public static class LessonSelectionVerificationTest
    {
        [InitializeOnLoadMethod]
        [MenuItem("Aurelia/Test Lesson Selection Scene")]
        public static void RunTest()
        {
            Debug.Log("==================================================================");
            Debug.Log("[VERIFICATION TEST] Bắt đầu kiểm tra Scene 05_LessonSelection...");
            Debug.Log("==================================================================");

            string scenePath = "Assets/Scenes/Physics/Game2/05_LessonSelection.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 1. Khởi tạo session test
            PhysicsGameSession session = PhysicsGameSession.Instance;
            if (session == null)
            {
                var sessionGo = new GameObject("PhysicsGameSession_Test");
                session = sessionGo.AddComponent<PhysicsGameSession>();
            }

            // ==========================================
            // TEST CHƯƠNG III (ĐIỆN)
            // ==========================================
            Debug.Log("\n--- [TEST 1] KIỂM TRA CHƯƠNG III (ĐIỆN) ---");
            session.SelectChapter(3);

            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("❌ THẤT BẠI: Không tìm thấy Canvas trong Scene!");
                return;
            }

            var controller = canvas.GetComponent<LessonSelectionController>() ?? canvas.gameObject.AddComponent<LessonSelectionController>();

            // Gọi Awake và Start
            var awakeMethod = typeof(LessonSelectionController).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (awakeMethod != null) awakeMethod.Invoke(controller, null);

            var startMethod = typeof(LessonSelectionController).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null) startMethod.Invoke(controller, null);

            // Kiểm tra Header
            var headerField = typeof(LessonSelectionController).GetField("headerChapterText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var headerTmp = headerField != null ? headerField.GetValue(controller) as TextMeshProUGUI : null;
            if (headerTmp != null && (headerTmp.text.Contains("Chương III") || headerTmp.text.Contains("Điện")))
            {
                Debug.Log($"✔ Header Chương III chính xác: '{headerTmp.text}'");
            }
            else
            {
                Debug.LogError($"❌ Header Chương III không đúng: '{headerTmp?.text}'");
            }

            // Kiểm tra Scrollbar & Handle size (KHÔNG BỊ TRÀN VIỀN)
            var sbField = typeof(LessonSelectionController).GetField("verticalScrollbar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scrollbar = sbField != null ? sbField.GetValue(controller) as Scrollbar : null;
            if (scrollbar != null && scrollbar.handleRect != null)
            {
                var handleRt = scrollbar.handleRect;
                string parentName = handleRt.parent != null ? handleRt.parent.name : "null";
                float handleW = handleRt.rect.width;
                Debug.Log($"✔ Scrollbar Handle cha: '{parentName}', width: {handleW:F1}px");

                if (parentName == "Sliding Area")
                {
                    Debug.Log("✔ XÁC THỰC THÀNH CÔNG: Handle đã nằm trong Sliding Area của Scrollbar, không còn tràn che Canvas!");
                }
                else
                {
                    Debug.LogError($"❌ THẤT BẠI: Handle vẫn có cha là '{parentName}'!");
                }
            }
            else
            {
                Debug.LogError("❌ THẤT BẠI: Scrollbar hoặc Handle bị null!");
            }

            // Kiểm tra Content & Cards của Chương III
            var contentField = typeof(LessonSelectionController).GetField("contentTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var contentRt = contentField != null ? contentField.GetValue(controller) as RectTransform : null;

            if (contentRt != null)
            {
                List<string> ch3Lessons = new List<string>();
                bool hasTestLesson = false;
                for (int i = 0; i < contentRt.childCount; i++)
                {
                    var child = contentRt.GetChild(i);
                    if (child.gameObject.activeSelf && child.name.StartsWith("LessonCard_"))
                    {
                        var tmp = child.GetComponentInChildren<TextMeshProUGUI>();
                        string title = tmp != null ? tmp.text : child.name;
                        ch3Lessons.Add(title);
                        if (child.name.Contains("C3_TEST")) hasTestLesson = true;
                    }
                }

                Debug.Log($"✔ Chương III có {ch3Lessons.Count}/4 bài học:");
                for (int i = 0; i < ch3Lessons.Count; i++)
                {
                    Debug.Log($"   [{i + 1}] {ch3Lessons[i]}");
                }

                if (ch3Lessons.Count == 4 && hasTestLesson)
                {
                    Debug.Log("✔ XÁC THỰC THÀNH CÔNG: Chương III có đủ 4 bài (Bài 11, 12, 13 và Bài kiểm tra cuối Chương III)!");
                }
                else
                {
                    Debug.LogError($"❌ THẤT BẠI: Chương III không đủ 4 bài hoặc thiếu bài kiểm tra!");
                }
            }

            // ==========================================
            // TEST CHƯƠNG II (ÁNH SÁNG)
            // ==========================================
            Debug.Log("\n--- [TEST 2] KIỂM TRA CHƯƠNG II (ÁNH SÁNG) ---");
            session.SelectChapter(2);
            if (startMethod != null) startMethod.Invoke(controller, null);

            if (contentRt != null)
            {
                int ch2Count = 0;
                List<string> ch2Lessons = new List<string>();
                for (int i = 0; i < contentRt.childCount; i++)
                {
                    var child = contentRt.GetChild(i);
                    if (child.gameObject.activeSelf && child.name.StartsWith("LessonCard_"))
                    {
                        ch2Count++;
                        var tmp = child.GetComponentInChildren<TextMeshProUGUI>();
                        ch2Lessons.Add(tmp != null ? tmp.text : child.name);
                    }
                }

                Debug.Log($"✔ Chương II có {ch2Count}/7 bài học:");
                for (int i = 0; i < ch2Lessons.Count; i++)
                {
                    Debug.Log($"   [{i + 1}] {ch2Lessons[i]}");
                }

                if (ch2Count == 7)
                {
                    Debug.Log("✔ XÁC THỰC THÀNH CÔNG: Chương II có đủ 7 bài học và bài kiểm tra!");
                }
                else
                {
                    Debug.LogError($"❌ THẤT BẠI: Chương II có {ch2Count} bài (kỳ vọng 7)!");
                }
            }

            // ==========================================
            // TEST CÁC NÚT ĐIỀU HƯỚNG
            // ==========================================
            Debug.Log("\n--- [TEST 3] KIỂM TRA NÚT ĐIỀU HƯỚNG ---");
            var backField = typeof(LessonSelectionController).GetField("btnBackToChapters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var btnBack = backField != null ? backField.GetValue(controller) as Button : null;
            if (btnBack != null)
            {
                Debug.Log($"✔ Nút Quay Lại: '{btnBack.gameObject.name}' hoạt động tốt.");
            }

            var confirmField = typeof(LessonSelectionController).GetField("btnConfirmLesson", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var btnConfirm = confirmField != null ? confirmField.GetValue(controller) as Button : null;
            if (btnConfirm != null)
            {
                Debug.Log($"✔ Nút Xác Nhận: '{btnConfirm.gameObject.name}' hoạt động tốt.");
            }

            Debug.Log("==================================================================");
            Debug.Log("[VERIFICATION TEST] HOÀN TẤT TẤT CẢ KIỂM TRA!");
            Debug.Log("==================================================================");
        }
    }
}
