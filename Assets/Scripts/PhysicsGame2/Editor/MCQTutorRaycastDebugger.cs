#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    [InitializeOnLoad]
    public static class MCQTutorRaycastDebugger
    {
        private const string OUT_PATH = "scratch/raycast_debug_output.txt";

        static MCQTutorRaycastDebugger()
        {
            EditorApplication.delayCall += () =>
            {
                RunRaycastDebug();
            };
        }

        [MenuItem("Aurelia/Debug MCQ Tutor Raycast")]
        public static void RunRaycastDebug()
        {
            var logLines = new List<string>();
            void Log(string msg)
            {
                Debug.Log(msg);
                logLines.Add(msg);
            }
            void LogError(string msg)
            {
                Debug.LogError(msg);
                logLines.Add("[ERROR] " + msg);
            }

            Log("==================================================");
            Log("[MCQTutorRaycastDebugger] BẮT ĐẦU CHẨN ĐOÁN RAYCAST");
            Log("==================================================");

            // 1. Mở Scene 11_Quiz nếu chưa mở
            string scenePath = "Assets/Scenes/Physics/Game2/11_Quiz.unity";
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != scenePath)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            // 2. Tìm Canvas và EventSystem
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                LogError("[MCQTutorRaycastDebugger] KHÔNG TÌM THẤY CANVAS!");
                File.WriteAllLines(OUT_PATH, logLines);
                return;
            }
            Log($"[MCQTutorRaycastDebugger] Canvas: {canvas.name}, RenderMode: {canvas.renderMode}, SortingOrder: {canvas.sortingOrder}");

            EventSystem eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                LogError("[MCQTutorRaycastDebugger] KHÔNG TÌM THẤY EVENTSYSTEM!");
            }
            else
            {
                Log($"[MCQTutorRaycastDebugger] EventSystem: {eventSystem.name}, Active: {eventSystem.gameObject.activeInHierarchy}, Module: {eventSystem.currentInputModule?.GetType().Name}");
            }

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                LogError("[MCQTutorRaycastDebugger] Canvas THIẾU GRAPHICRAYCASTER!");
                File.WriteAllLines(OUT_PATH, logLines);
                return;
            }

            // 3. Khởi tạo / Mở MCQ Tutor Popup
            var ctx = new MCQQuestionContext
            {
                AttemptId = "debug_attempt",
                QuestionId = "TN_DEBUG_01",
                QuestionNumber = 1,
                TotalQuestions = 10,
                QuestionText = "Công suất điện của một đoạn mạch cho biết điều gì?",
                Options = new List<string> { "A. Năng lượng điện", "B. Tốc độ sinh công", "C. Nhiệt lượng tỏa ra", "D. Khả năng tích điện" },
                SelectedAnswerIndex = -1,
                Difficulty = "NB"
            };

            var controller = MCQTutorPopupController.GetOrCreate();
            controller.OpenTutor(MCQTutorMode.Hint, ctx);

            // 4. Tìm các đối tượng tương tác trong Popup
            Transform popupRoot = canvas.transform.Find("Aurelia_MCQTutorPopup");
            if (popupRoot == null)
            {
                LogError("[MCQTutorRaycastDebugger] Không tìm thấy Aurelia_MCQTutorPopup dưới Canvas!");
                File.WriteAllLines(OUT_PATH, logLines);
                return;
            }

            Log($"[MCQTutorRaycastDebugger] PopupRoot: {popupRoot.name}, SiblingIndex: {popupRoot.GetSiblingIndex()}/{canvas.transform.childCount - 1}");

            CanvasGroup cg = popupRoot.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                Log($"[MCQTutorRaycastDebugger] CanvasGroup: alpha={cg.alpha}, interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}");
            }

            // Danh sách các điểm cần test
            var targetsToTest = new Dictionary<string, Transform>
            {
                { "Btn_Send", FindDeepChild(popupRoot, "Btn_Send") },
                { "Btn_CloseBottom", FindDeepChild(popupRoot, "Btn_CloseBottom") },
                { "Btn_CloseTopRight", FindDeepChild(popupRoot, "Btn_CloseTopRight") },
                { "Box_InputField", FindDeepChild(popupRoot, "Box_InputField") },
                { "QuickPromptsBar", FindDeepChild(popupRoot, "QuickPromptsBar") }
            };

            Camera eventCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera ?? Camera.main;

            foreach (var kvp in targetsToTest)
            {
                string targetName = kvp.Key;
                Transform targetTf = kvp.Value;

                if (targetTf == null)
                {
                    Log($"[MCQTutorRaycastDebugger] Target '{targetName}' KHÔNG TỒN TẠI trong popup!");
                    continue;
                }

                RectTransform targetRt = targetTf as RectTransform;
                Vector3 worldCenter = targetRt.TransformPoint(targetRt.rect.center);
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCenter);

                Log($"\n--- TEST TARGET: '{targetName}' tại ScreenPos {screenPos} (World: {worldCenter}) ---");
                Log($"Target ActiveInHierarchy: {targetTf.gameObject.activeInHierarchy}, sizeDelta: {targetRt.sizeDelta}, anchoredPos: {targetRt.anchoredPosition}");

                var targetImg = targetTf.GetComponent<Image>();
                if (targetImg != null)
                {
                    Log($"Target Image: raycastTarget={targetImg.raycastTarget}, color={targetImg.color}");
                }
                var targetBtn = targetTf.GetComponent<Button>();
                if (targetBtn != null)
                {
                    Log($"Target Button: interactable={targetBtn.interactable}, targetGraphic={targetBtn.targetGraphic?.name}");
                }

                PointerEventData pointerData = new PointerEventData(eventSystem != null ? eventSystem : EventSystem.current)
                {
                    position = screenPos
                };

                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(pointerData, results);

                Log($"Số lượng kết quả Raycast: {results.Count}");
                if (results.Count == 0)
                {
                    LogError($"[RAYCAST THẤT BẠI] Không có GameObject nào nhận Raycast tại vị trí của '{targetName}'!");
                }
                else
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        var res = results[i];
                        string isTarget = (res.gameObject == targetTf.gameObject || res.gameObject.transform.IsChildOf(targetTf)) ? "[TARGET/CHILD]" : "[KHÁC/CHẶN]";
                        Log($"  #{i + 1} {isTarget} GameObject: '{GetFullPath(res.gameObject)}' | Depth: {res.depth} | Module: {res.module.name}");
                    }

                    var topHit = results[0];
                    if (topHit.gameObject == targetTf.gameObject || topHit.gameObject.transform.IsChildOf(targetTf))
                    {
                        Log($"[OK] Target '{targetName}' nhận được Raycast ĐỨNG ĐẦU!");
                    }
                    else
                    {
                        LogError($"[BỊ CHẶN RAYCAST] Target '{targetName}' BỊ ĐỐI TƯỢNG '{GetFullPath(topHit.gameObject)}' NẰM ĐÈ LÊN VÀ CHẶN CLICK!");
                    }
                }
            }

            // 5. Test Click Simulation
            Log("\n--- TEST CLICK SIMULATION ---");
            Transform sendTf = targetsToTest["Btn_Send"];
            if (sendTf != null)
            {
                var btn = sendTf.GetComponent<Button>();
                if (btn != null && btn.interactable)
                {
                    Log("[TEST] Gọi btnSend.onClick.Invoke()...");
                    btn.onClick.Invoke();
                    Log("[OK] btnSend.onClick.Invoke() thành công không crash!");
                }
            }

            Transform closeTf = targetsToTest["Btn_CloseBottom"];
            if (closeTf != null)
            {
                var btn = closeTf.GetComponent<Button>();
                if (btn != null && btn.interactable)
                {
                    Log("[TEST] Gọi btnCloseBottom.onClick.Invoke()...");
                    btn.onClick.Invoke();
                    Log($"[OK] btnCloseBottom thành công! Popup activeSelf = {popupRoot.gameObject.activeSelf}");
                }
            }

            Log("\n==================================================");
            Log("[MCQTutorRaycastDebugger] KẾT THÚC CHẨN ĐOÁN RAYCAST");
            Log("==================================================");

            File.WriteAllLines(OUT_PATH, logLines);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static string GetFullPath(GameObject go)
        {
            string path = go.name;
            Transform t = go.transform.parent;
            while (t != null)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }
            return path;
        }
    }
}
#endif
