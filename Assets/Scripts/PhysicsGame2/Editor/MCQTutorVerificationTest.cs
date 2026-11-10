#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    [InitializeOnLoad]
    public static class MCQTutorVerificationTest
    {
        private const string REPORT_PATH = "scratch/mcq_tutor_verification_report.txt";

        static MCQTutorVerificationTest()
        {
            EditorApplication.delayCall += () =>
            {
                RunAllTests();
            };
        }

        [MenuItem("Tools/Physics Game 2/Run MCQ Tutor Verification Test")]
        public static void RunAllTests()
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
                logLines.Add("[FAIL] " + msg);
            }

            Log("==================================================");
            Log("[MCQTutorVerificationTest] BẮT ĐẦU KIỂM THỬ GIA SƯ AI TRẮC NGHIỆM");
            Log("==================================================");

            int passCount = 0;
            int totalTests = 12;

            // --------------------------------------------------
            // TEST 1: EnsureSelectablesHealthy
            // --------------------------------------------------
            try
            {
                MCQTutorPopupController.EnsureSelectablesHealthy();
                Log("[PASS] TEST 1: EnsureSelectablesHealthy() thực thi thành công, phục hồi mảng UGUI.");
                passCount++;
            }
            catch (Exception ex)
            {
                LogError($"TEST 1 THẤT BẠI: {ex.Message}");
            }

            // --------------------------------------------------
            // TEST 2: Prefab UI Hierarchy Integrity
            // --------------------------------------------------
            GameObject prefab = Resources.Load<GameObject>("UI/MCQTutorPopup");
            if (prefab == null)
            {
                LogError("TEST 2 THẤT BẠI: Không tìm thấy Prefab Resources/UI/MCQTutorPopup!");
            }
            else
            {
                var requiredObjects = new string[]
                {
                    "Board_Background", "Banner_Title", "Text_Title", "Text_Subtitle",
                    "Btn_CloseTopRight", "Banner_Context", "Text_ContextInfo",
                    "Chat_ScrollView", "QuickPromptsBar", "BottomInputBar",
                    "Box_InputField", "TextArea", "Placeholder", "Text",
                    "Btn_Send", "Text_SendLabel", "Btn_CloseBottom", "Text_CloseLabel"
                };

                bool allFound = true;
                foreach (var req in requiredObjects)
                {
                    if (FindDeep(prefab.transform, req) == null)
                    {
                        LogError($"TEST 2 THẤT BẠI: Thiếu object '{req}' trong Prefab!");
                        allFound = false;
                    }
                }

                if (allFound)
                {
                    Log("[PASS] TEST 2: Prefab chứa đầy đủ 100% các thành phần UI tiêu chuẩn.");
                    passCount++;
                }
            }

            if (prefab != null)
            {
                // --------------------------------------------------
                // TEST 3: Non-interactive Elements RaycastTarget = False
                // --------------------------------------------------
                bool test3Passed = true;
                var nonInteractiveTexts = new string[]
                {
                    "Text_Title", "Text_Subtitle", "Text_ContextInfo",
                    "Placeholder", "Text", "Text_SendLabel", "Text_CloseLabel"
                };

                foreach (var tName in nonInteractiveTexts)
                {
                    var tf = FindDeep(prefab.transform, tName);
                    var tmp = tf?.GetComponent<TextMeshProUGUI>();
                    if (tmp != null && tmp.raycastTarget)
                    {
                        LogError($"TEST 3 THẤT BẠI: {tName} đang có raycastTarget = true (sẽ chặn click nút)!");
                        test3Passed = false;
                    }
                }

                var bannerTitleImg = FindDeep(prefab.transform, "Banner_Title")?.GetComponent<Image>();
                if (bannerTitleImg != null && bannerTitleImg.raycastTarget)
                {
                    LogError("TEST 3 THẤT BẠI: Banner_Title đang có raycastTarget = true!");
                    test3Passed = false;
                }

                var bannerCtxImg = FindDeep(prefab.transform, "Banner_Context")?.GetComponent<Image>();
                if (bannerCtxImg != null && bannerCtxImg.raycastTarget)
                {
                    LogError("TEST 3 THẤT BẠI: Banner_Context đang có raycastTarget = true!");
                    test3Passed = false;
                }

                if (test3Passed)
                {
                    Log("[PASS] TEST 3: Toàn bộ Text và Banner trang trí đều có raycastTarget = false.");
                    passCount++;
                }

                // --------------------------------------------------
                // TEST 4: Interactive Elements RaycastTarget = True
                // --------------------------------------------------
                bool test4Passed = true;
                var interactiveObjects = new string[] { "Btn_Send", "Btn_CloseBottom", "Btn_CloseTopRight", "Box_InputField" };
                foreach (var iName in interactiveObjects)
                {
                    var tf = FindDeep(prefab.transform, iName);
                    var img = tf?.GetComponent<Image>();
                    if (img == null || !img.raycastTarget)
                    {
                        LogError($"TEST 4 THẤT BẠI: {iName} thiếu Image hoặc raycastTarget != true!");
                        test4Passed = false;
                    }
                }

                if (test4Passed)
                {
                    Log("[PASS] TEST 4: Các nút tương tác và ô nhập đều có raycastTarget = true.");
                    passCount++;
                }

                // --------------------------------------------------
                // TEST 5: Send Button Label Position Offset
                // --------------------------------------------------
                var sendLblRt = FindDeep(prefab.transform, "Text_SendLabel") as RectTransform;
                if (sendLblRt != null && sendLblRt.anchorMin.x >= 0.40f && sendLblRt.anchorMax.x <= 1.0f)
                {
                    Log($"[PASS] TEST 5: Nút GỬI có Text_SendLabel căn chuẩn vùng [{sendLblRt.anchorMin.x:F2} .. {sendLblRt.anchorMax.x:F2}], không đè lên icon chiếc mũ.");
                    passCount++;
                }
                else
                {
                    LogError($"TEST 5 THẤT BẠI: Text_SendLabel anchorMin.x = {sendLblRt?.anchorMin.x:F2} < 0.35f!");
                }

                // --------------------------------------------------
                // TEST 6: Close Button Label Position Offset
                // --------------------------------------------------
                var closeLblRt = FindDeep(prefab.transform, "Text_CloseLabel") as RectTransform;
                if (closeLblRt != null && closeLblRt.anchorMin.x >= 0.44f && closeLblRt.anchorMax.x <= 1.0f)
                {
                    Log($"[PASS] TEST 6: Nút ĐÓNG có Text_CloseLabel căn chuẩn vùng [{closeLblRt.anchorMin.x:F2} .. {closeLblRt.anchorMax.x:F2}], không đè lên icon khiên X.");
                    passCount++;
                }
                else
                {
                    LogError($"TEST 6 THẤT BẠI: Text_CloseLabel anchorMin.x = {closeLblRt?.anchorMin.x:F2} < 0.35f!");
                }

                // --------------------------------------------------
                // TEST 7: InputField TextArea Offset
                // --------------------------------------------------
                var taRt = FindDeep(prefab.transform, "TextArea") as RectTransform;
                if (taRt != null && taRt.offsetMin.x >= 140f)
                {
                    Log($"[PASS] TEST 7: TextArea có offsetMin.x = {taRt.offsetMin.x}px >= 140px, vượt qua hoàn toàn icon cây bút.");
                    passCount++;
                }
                else
                {
                    LogError($"TEST 7 THẤT BẠI: TextArea offsetMin.x = {taRt?.offsetMin.x}px < 140px!");
                }

                // --------------------------------------------------
                // TEST 8: Context Banner Offset
                // --------------------------------------------------
                var ctxRt = FindDeep(prefab.transform, "Text_ContextInfo") as RectTransform;
                if (ctxRt != null && ctxRt.offsetMin.x >= 110f)
                {
                    Log($"[PASS] TEST 8: Text_ContextInfo có offsetMin.x = {ctxRt.offsetMin.x}px >= 110px, không đè lên hoa văn trang trí.");
                    passCount++;
                }
                else
                {
                    LogError($"TEST 8 THẤT BẠI: Text_ContextInfo offsetMin.x = {ctxRt?.offsetMin.x}px < 110px!");
                }

                // --------------------------------------------------
                // TEST 9: Click Simulation & Flow
                // --------------------------------------------------
                try
                {
                    GameObject canvasGo = new GameObject("Test_Canvas");
                    Canvas cv = canvasGo.AddComponent<Canvas>();
                    cv.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGo.AddComponent<CanvasScaler>();
                    canvasGo.AddComponent<GraphicRaycaster>();

                    var controller = MCQTutorPopupController.GetOrCreate();
                    var testCtx = new MCQQuestionContext
                    {
                        AttemptId = "test_att",
                        QuestionId = "TN_01",
                        QuestionNumber = 1,
                        TotalQuestions = 10,
                        QuestionText = "Năng lượng dòng điện gọi là gì?",
                        Options = new List<string> { "A. Cơ năng", "B. Điện năng" }
                    };

                    controller.OpenTutor(MCQTutorMode.Hint, testCtx);

                    Transform pRoot = canvasGo.transform.Find("Aurelia_MCQTutorPopup");
                    if (pRoot == null) pRoot = GameObject.Find("Aurelia_MCQTutorPopup")?.transform;

                    bool test9Success = false;
                    if (pRoot != null)
                    {
                        var btnSend = FindDeep(pRoot, "Btn_Send")?.GetComponent<Button>();
                        var btnClose = FindDeep(pRoot, "Btn_CloseBottom")?.GetComponent<Button>();

                        if (btnSend != null && btnClose != null)
                        {
                            btnSend.onClick.Invoke();
                            btnClose.onClick.Invoke();
                            test9Success = true;
                        }
                    }

                    UnityEngine.Object.DestroyImmediate(canvasGo);
                    if (controller != null && controller.gameObject != null)
                    {
                        UnityEngine.Object.DestroyImmediate(controller.gameObject);
                    }

                    if (test9Success)
                    {
                        Log("[PASS] TEST 9: Giả lập gọi Send và Close Click thành công, không gặp bất kỳ Exception nào!");
                        passCount++;
                    }
                    else
                    {
                        LogError("TEST 9 THẤT BẠI: Không thể tìm thấy popup hoặc button để giả lập click!");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"TEST 9 THẤT BẠI: {ex.Message}\n{ex.StackTrace}");
                }
            }

                // --------------------------------------------------
                // TEST 10: ScrollRect Viewport & RectMask2D Verification
                // --------------------------------------------------
                var scrollTf = FindDeep(prefab.transform, "Chat_ScrollView");
                var sr = scrollTf?.GetComponent<ScrollRect>();
                var vpTf = FindDeep(scrollTf, "Viewport");
                var rm = vpTf?.GetComponent<RectMask2D>();
                var vpImg = vpTf?.GetComponent<Image>();

                if (sr != null && vpTf != null && sr.viewport == vpTf as RectTransform &&
                    rm != null && rm.enabled && vpImg != null && vpImg.raycastTarget &&
                    sr.movementType == ScrollRect.MovementType.Clamped)
                {
                    Log("[PASS] TEST 10: Chat_ScrollView có Viewport chuẩn, RectMask2D kích hoạt chuẩn mực, Image raycastTarget = true, movementType = Clamped.");
                    passCount++;
                }
                else
                {
                    LogError($"TEST 10 THẤT BẠI: sr={sr != null}, vp={vpTf != null}, sr.viewport={sr?.viewport == vpTf as RectTransform}, rm={rm != null}, movementType={sr?.movementType}");
                }

                // --------------------------------------------------
                // TEST 11: Content Layout & ContentSizeFitter
                // --------------------------------------------------
                var contentTf = FindDeep(scrollTf, "Content");
                var vlg = contentTf?.GetComponent<VerticalLayoutGroup>();
                var csf = contentTf?.GetComponent<ContentSizeFitter>();

                if (contentTf != null && sr.content == contentTf as RectTransform &&
                    vlg != null && vlg.childControlHeight && !vlg.childForceExpandHeight &&
                    csf != null && csf.verticalFit == ContentSizeFitter.FitMode.PreferredSize)
                {
                    Log("[PASS] TEST 11: Content có VerticalLayoutGroup (childControlHeight=true) và ContentSizeFitter (verticalFit=PreferredSize) chuẩn.");
                    passCount++;
                }
                else
                {
                    LogError($"TEST 11 THẤT BẠI: Content Layout hoặc SizeFitter không đúng chuẩn!");
                }

                // --------------------------------------------------
                // TEST 12: Scene 11_Quiz.unity Scroll & Hardware Stencil Mask Verification
                // --------------------------------------------------
                try
                {
                    string scenePath = "Assets/Scenes/Physics/Game2/11_Quiz.unity";
                    string sceneContent = File.ReadAllText(scenePath);

                    bool sceneHasMask = sceneContent.Contains("guid: 31a19414c41e5ae4aae2af33fee712f6");
                    bool sceneHasClamped = sceneContent.Contains("m_MovementType: 2");

                    if (sceneHasMask && sceneHasClamped)
                    {
                        Log("[PASS] TEST 12: Scene 11_Quiz.unity đã được cấu hình Mask Stencil GUID chuẩn và Scroll MovementType Clamped.");
                        passCount++;
                    }
                    else
                    {
                        LogError($"TEST 12 THẤT BẠI: Scene 11_Quiz.unity thiếu Mask Stencil hoặc MovementType Clamped! (hasMask={sceneHasMask}, hasClamped={sceneHasClamped})");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"TEST 12 THẤT BẠI: {ex.Message}");
                }

                        Log("==================================================");
            Log($"[MCQTutorVerificationTest] KẾT QUẢ: {passCount}/{totalTests} TESTS ĐẠT YÊU CẦU!");
            Log("==================================================");

            File.WriteAllLines(REPORT_PATH, logLines);
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent == null) return null;
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                var res = FindDeep(child, name);
                if (res != null) return res;
            }
            return null;
        }
    }
}
#endif
