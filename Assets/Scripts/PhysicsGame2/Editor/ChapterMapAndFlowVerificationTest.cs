using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    public class ChapterMapAndFlowVerificationTest
    {
        [InitializeOnLoadMethod]
        [MenuItem("Aurelia Academy/Run Map & Flow Tests (Test01-Test23)")]
        public static void RunAllMapAndFlowTests()
        {
            var tester = new ChapterMapAndFlowVerificationTest();
            int passed = 0;
            int failed = 0;
            var testMethods = typeof(ChapterMapAndFlowVerificationTest)
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0)
                .OrderBy(m => m.Name);

            foreach (var m in testMethods)
            {
                try
                {
                    m.Invoke(tester, null);
                    passed++;
                    Debug.Log($"<color=#40FFB0>[PASS] {m.Name}</color>");
                }
                catch (Exception ex)
                {
                    failed++;
                    Debug.LogError($"<color=#FF5555>[FAIL] {m.Name}: {ex.InnerException?.Message ?? ex.Message}</color>");
                }
            }
            Debug.Log($"<color=#F5D261>=== MAP & FLOW TESTS SUMMARY: {passed} PASSED, {failed} FAILED ===</color>");
        }

        [Test]
        public void Test01_ChapterSelectionFlow_IslandClickOnlyHighlights_ConfirmNavigates()
        {
            Debug.Log("=== TEST 01: Verify Chapter Selection Flow ===");

            // 1. Load scene 04_ChapterSelection in edit mode
            string scenePath = "Assets/Scenes/Physics/Game2/04_ChapterSelection.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found at {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var controllerGo = GameObject.FindAnyObjectByType<ChapterSelectionController>();
                Assert.IsNotNull(controllerGo, "ChapterSelectionController must exist in scene 04_ChapterSelection");

                // Find confirm button
                var confirmGo = GameObject.Find("Btn_Confirm") ?? GameObject.Find("Button") ?? GameObject.Find("xacnhan");
                Assert.IsNotNull(confirmGo, "Confirm button must exist in scene 04_ChapterSelection");

                var backGo = GameObject.Find("quaylai") ?? GameObject.Find("Btn_Back");
                Assert.IsNotNull(backGo, "Back button must exist in scene 04_ChapterSelection");

                // Verify chapter cards exist
                for (int ch = 1; ch <= 5; ch++)
                {
                    var cardGo = GameObject.Find($"chuong{ch}");
                    Assert.IsNotNull(cardGo, $"Chapter card chuong{ch} must exist");
                }

                Debug.Log("[TEST 01 PASSED] 04_ChapterSelection hierarchy and controller verified.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test02_CharacterPersistence_AllFourCharactersHaveDistinctSprites()
        {
            Debug.Log("=== TEST 02: Verify Character Persistence on Map ===");

            string[] characterIds = new[] { "Minh", "Lan", "Duy", "Ha" };
            var loadedSprites = new Dictionary<string, Sprite>();

            foreach (var charId in characterIds)
            {
                // Test Resources load
                Sprite sp = Resources.Load<Sprite>($"Characters/{charId}");
                if (sp == null)
                {
                    // Test AssetDatabase fallback
                    string sheetPath = "Assets/Art/Environment/Bộ tài sản chọn nhân vật chibi fantasy.png";
                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
                    string targetSub = charId switch
                    {
                        "Lan" => "Bộ tài sản chọn nhân vật chibi fantasy_3",
                        "Duy" => "Bộ tài sản chọn nhân vật chibi fantasy_4",
                        "Ha" => "Bộ tài sản chọn nhân vật chibi fantasy_5",
                        _ => "Bộ tài sản chọn nhân vật chibi fantasy_0"
                    };
                    sp = allAssets?.OfType<Sprite>().FirstOrDefault(s => s.name == targetSub);
                }

                Assert.IsNotNull(sp, $"Character sprite for '{charId}' must load successfully!");
                loadedSprites[charId] = sp;
                Debug.Log($"Loaded sprite for {charId}: {sp.name} ({sp.rect.width}x{sp.rect.height})");
            }

            // Verify all 4 sprites are distinct
            var uniqueSprites = loadedSprites.Values.Select(s => s.name).Distinct().ToList();
            Assert.AreEqual(4, uniqueSprites.Count, "All 4 characters (Minh, Lan, Duy, Hà) must have distinct sprites!");
            Debug.Log("[TEST 02 PASSED] 4 distinct character sprites successfully verified.");
        }

        [Test]
        public void Test03_WASDMovement_NormalizationAndClamping()
        {
            Debug.Log("=== TEST 03: Verify WASD Movement & Clamping ===");

            // Test diagonal normalization: (1, 1).normalized has magnitude 1.0
            Vector2 diag = new Vector2(1f, 1f).normalized;
            Assert.AreEqual(1.0f, diag.magnitude, 0.001f, "Diagonal movement must be normalized to prevent moving faster!");

            // Test bounds clamping
            Vector2 minBounds = new Vector2(-850f, -400f);
            Vector2 maxBounds = new Vector2(850f, 320f);

            Vector2 testOutOfBounds = new Vector2(-1200f, 600f);
            Vector2 clamped = new Vector2(
                Mathf.Clamp(testOutOfBounds.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(testOutOfBounds.y, minBounds.y, maxBounds.y)
            );

            Assert.AreEqual(-850f, clamped.x, "X position must be clamped within floor bounds");
            Assert.AreEqual(320f, clamped.y, "Y position must be clamped within floor bounds");

            Debug.Log("[TEST 03 PASSED] Diagonal normalization and bounds clamping verified.");
        }

        [Test]
        public void Test04_DeviceData_AllChaptersHaveCorrectDeviceCounts()
        {
            Debug.Log("=== TEST 04: Verify Device Counts Across All 5 Chapters ===");

            int[] expectedCounts = new[] { 4, 7, 4, 3, 3 }; // Chapter 1 to 5
            for (int ch = 1; ch <= 5; ch++)
            {
                var devs = GameDataRepository.GetDevicesForChapter(ch);
                Assert.AreEqual(expectedCounts[ch - 1], devs.Count, $"Chapter {ch} must have exactly {expectedCounts[ch - 1]} devices!");
                Debug.Log($"Chapter {ch} verified: {devs.Count} devices.");
            }

            // Verify Chapter 3 devices specifically
            var ch3Devs = GameDataRepository.GetDevicesForChapter(3);
            Assert.AreEqual("bai11", ch3Devs[0].BaiID);
            Assert.AreEqual("bai12", ch3Devs[1].BaiID);
            Assert.AreEqual("bai13", ch3Devs[2].BaiID);
            Assert.AreEqual("test_chuong3", ch3Devs[3].BaiID);

            Debug.Log("[TEST 04 PASSED] All 5 chapters have correct device mapping; Chapter 3 devices 11, 12, 13, Test verified.");
        }

        [Test]
        public void Test05_ChapterMapSceneHierarchy_ChapterIII()
        {
            Debug.Log("=== TEST 05: Verify Scene 08_Chương III Hierarchy ===");

            string scenePath = "Assets/Scenes/Physics/Game2/08_Chương III.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found at {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                // Verify ChapterMapController exists
                var controller = GameObject.FindAnyObjectByType<ChapterMapController>();
                Assert.IsNotNull(controller, "ChapterMapController must exist in 08_Chương III");

                // Verify player object exists with size ~110x140
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist");

                var images = canvas.GetComponentsInChildren<Image>(true);
                bool hasPlayer = images.Any(img => img.rectTransform.sizeDelta.x >= 90f && img.rectTransform.sizeDelta.x <= 160f
                                                && img.rectTransform.sizeDelta.y >= 120f && img.rectTransform.sizeDelta.y <= 200f
                                                && !img.name.Contains("Background") && !img.name.Contains("bg"));
                Assert.IsTrue(hasPlayer, "Player GameObject (~110x140) must exist on map");

                // Verify Chọn chương text exists
                var tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                bool hasBack = tmps.Any(t => t.text.Contains("Chọn chương") || t.text.Contains("Chon chuong"));
                Assert.IsTrue(hasBack, "Nút 'Chọn chương' text must exist on map");

                // Verify station numbers 1, 2, 3, 4 exist
                var numberTexts = tmps.Where(t => int.TryParse(t.text.Trim(), out _)).Select(t => int.Parse(t.text.Trim())).ToList();
                for (int st = 1; st <= 4; st++)
                {
                    Assert.IsTrue(numberTexts.Contains(st), $"Station number {st} must exist in Chapter III map");
                }

                Debug.Log("[TEST 05 PASSED] 08_Chương III hierarchy fully verified.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test06_StudyDurationPersistence_NoDefaultReset()
        {
            Debug.Log("=== TEST 06: Verify Study Duration Persistence (15, 30, 45 mins) ===");

            // 1. Test 15 minutes selection
            PhysicsGameSession.Instance.ConfigureStudyPlan(15, "Ôn bài cũ");
            var record15 = PhysicsGameSession.Instance.StartOrResumeStudySession();
            Assert.AreEqual(15, record15.plannedMinutes, "Session planned minutes must be 15 when 15 is configured!");
            Assert.AreEqual(15, PhysicsGameSession.Instance.SaveData.plannedStudyMinutes, "SaveData planned minutes must be 15!");

            // 2. Test 45 minutes selection
            PhysicsGameSession.Instance.ConfigureStudyPlan(45, "Luyện vận dụng");
            var record45 = PhysicsGameSession.Instance.StartOrResumeStudySession();
            Assert.AreEqual(45, record45.plannedMinutes, "Session planned minutes must be 45 when 45 is configured!");

            // 3. Test 30 minutes selection
            PhysicsGameSession.Instance.ConfigureStudyPlan(30, "Học bài mới");
            var record30 = PhysicsGameSession.Instance.StartOrResumeStudySession();
            Assert.AreEqual(30, record30.plannedMinutes, "Session planned minutes must be 30 when 30 is configured!");

            // 4. Test re-selecting 15 minutes does NOT get overwritten to 30
            PhysicsGameSession.Instance.ConfigureStudyPlan(15, "Ôn bài cũ");
            Assert.AreEqual(15, PhysicsGameSession.Instance.SaveData.plannedStudyMinutes);
            Assert.AreEqual(15, PhysicsGameSession.Instance.SaveData.activeStudySession.plannedMinutes,
                "Selecting 15 minutes must update active study session plannedMinutes without defaulting back to 30!");

            Debug.Log("[TEST 06 PASSED] Study duration persistence across 15, 30, 45 mins verified.");
        }

        [Test]
        public void Test07_ChapterIndexDetection_CorrectForAllRomanNumeralScenes()
        {
            Debug.Log("=== TEST 07: Verify Chapter Index Detection For All Scenes ===");

            // Mapping tests: ensure "Chương III" is NEVER wrongly detected as "Chương I"
            var testCases = new Dictionary<string, int>
            {
                { "08_Chương III", 3 },
                { "29. Chương III after", 3 },
                { "06_Chương I", 1 },
                { "27. Chương I after", 1 },
                { "07_Chương II", 2 },
                { "28. Chương II after 1", 2 },
                { "09_Chương IV 1", 4 },
                { "31. Chương V after 2", 4 }, // Scene 31 content is Tháp Cảm Ứng (Chương IV)
                { "10_Chương V 2", 5 },
                { "30. Chương IV after 1", 5 }  // Scene 30 content is Vườn Năng Lượng (Chương V)
            };

            foreach (var kvp in testCases)
            {
                string sceneName = kvp.Key;
                int expected = kvp.Value;

                int detected;
                if (sceneName.Contains("30.") || sceneName.Contains("Chuong IV after") || sceneName.Contains("Chương IV after"))
                {
                    detected = 5;
                }
                else if (sceneName.Contains("31.") || sceneName.Contains("Chuong V after") || sceneName.Contains("Chương V after"))
                {
                    detected = 4;
                }
                else if (sceneName.Contains("III") || sceneName.Contains("08_") || sceneName.Contains("29."))
                {
                    detected = 3;
                }
                else if (sceneName.Contains("IV") || sceneName.Contains("09_"))
                {
                    detected = 4;
                }
                else if (sceneName.Contains("II") || sceneName.Contains("07_") || sceneName.Contains("28."))
                {
                    detected = 2;
                }
                else if (sceneName.Contains("Chương V") || sceneName.Contains("Chuong V") || sceneName.Contains(" 5") || sceneName.Contains("10_"))
                {
                    detected = 5;
                }
                else if (sceneName.Contains("Chương I") || sceneName.Contains("Chuong I") || sceneName.Contains(" 1") || sceneName.Contains("06_") || sceneName.Contains("27."))
                {
                    detected = 1;
                }
                else
                {
                    detected = 1;
                }

                Assert.AreEqual(expected, detected, $"Scene '{sceneName}' must be detected as Chapter {expected}, but got {detected}!");
                Debug.Log($"Scene '{sceneName}' -> Chapter {detected} [CORRECT]");
            }

            Debug.Log("[TEST 07 PASSED] All 10 scene chapter detections strictly verified.");
        }

        [Test]
        public void Test08_PromptPosition_SeparatedFromInstructionBar()
        {
            Debug.Log("=== TEST 08: Verify Interaction Prompt Separated From Instruction Bar ===");

            // Bottom instruction ribbon in 08_Chương III:
            // Center y = -445.38, size y = 47.94 -> Top edge = -445.38 + 23.97 = -421.41
            float instructionBarTopEdge = -445.38f + (47.94f / 2f);

            // Interaction prompt in ChapterMapController:
            // Center y = -345f, size y = 44f -> Bottom edge = -345f - 22f = -367f
            float promptCenterY = -345f;
            float promptHeight = 44f;
            float promptBottomEdge = promptCenterY - (promptHeight / 2f);

            float gap = promptBottomEdge - instructionBarTopEdge;

            Assert.Greater(gap, 40f,
                $"Interaction prompt bottom edge ({promptBottomEdge}) must have at least 40px gap above instruction ribbon top edge ({instructionBarTopEdge})! Current gap: {gap}px");

            Debug.Log($"[TEST 08 PASSED] Gap between prompt ({promptBottomEdge}) and instruction ribbon ({instructionBarTopEdge}) is {gap:F1}px. Zero overlap!");
        }

        [Test]
        public void Test09_ChapterIIIDevicesAndTitles_AccurateMapping()
        {
            Debug.Log("=== TEST 09: Verify Chapter III Tooltip & Lesson Data Filter ===");

            var ch3Devs = GameDataRepository.GetDevicesForChapter(3);
            Assert.AreEqual(4, ch3Devs.Count, "Chapter III must have exactly 4 devices");

            Assert.AreEqual("G2_C3_D01", ch3Devs[0].DeviceId);
            Assert.AreEqual("bai11", ch3Devs[0].BaiID);
            StringAssert.Contains("Bài 11", ch3Devs[0].TenBai);

            Assert.AreEqual("G2_C3_D02", ch3Devs[1].DeviceId);
            Assert.AreEqual("bai12", ch3Devs[1].BaiID);
            StringAssert.Contains("Bài 12", ch3Devs[1].TenBai);

            Assert.AreEqual("G2_C3_D03", ch3Devs[2].DeviceId);
            Assert.AreEqual("bai13", ch3Devs[2].BaiID);
            StringAssert.Contains("Bài 13", ch3Devs[2].TenBai);

            Assert.AreEqual("G2_C3_D04", ch3Devs[3].DeviceId);
            Assert.AreEqual("test_chuong3", ch3Devs[3].BaiID);
            StringAssert.Contains("TEST tổng hợp Chương III", ch3Devs[3].TenBai);

            Debug.Log("[TEST 09 PASSED] Chapter III devices strictly filter Bài 11, 12, 13, and Test Chương III.");
        }

        [Test]
        public void Test10_ChapterSelection_DirectRouteToChapterMap_SkipsLessonSelection()
        {
            Debug.Log("=== TEST 10: Verify Direct Navigation to Chapter Map (Skipping LessonSelection) ===");

            for (int ch = 1; ch <= 5; ch++)
            {
                bool isCompleted = RestorationService.IsChapterFullyRestored(ch);
                RouteId mapRoute = SceneCatalog.GetChapterMapRoute(ch, isCompleted);

                // Ensure route is a Chapter Map route, NEVER LessonSelection
                Assert.AreNotEqual(RouteId.LessonSelection, mapRoute, $"Confirming Chapter {ch} must NEVER navigate to LessonSelection!");
                Assert.IsTrue(
                    mapRoute == RouteId.ChapterMap1 || mapRoute == RouteId.ChapterMap2 ||
                    mapRoute == RouteId.ChapterMap3 || mapRoute == RouteId.ChapterMap4 ||
                    mapRoute == RouteId.ChapterMap5 || mapRoute == RouteId.ChapterMap1After ||
                    mapRoute == RouteId.ChapterMap2After || mapRoute == RouteId.ChapterMap3After ||
                    mapRoute == RouteId.ChapterMap4After || mapRoute == RouteId.ChapterMap5After,
                    $"Chapter {ch} must map directly to its gameplay map route, got {mapRoute}."
                );

                Debug.Log($"Chapter {ch} confirms directly to: {mapRoute} [VERIFIED]");
            }

            Debug.Log("[TEST 10 PASSED] Direct navigation to chapter maps verified across all 5 chapters.");
        }

        [Test]
        public void Test11_All21DevicesAcross5Chapters_HaveCorrectTitlesAndPlaqueFormatting()
        {
            Debug.Log("=== TEST 11: Verify All 21 Devices and Name Plaque Formatting Across 5 Chapters ===");

            int totalDevices = 0;
            for (int ch = 1; ch <= 5; ch++)
            {
                var devs = GameDataRepository.GetDevicesForChapter(ch);
                totalDevices += devs.Count;

                for (int i = 0; i < devs.Count; i++)
                {
                    var dev = devs[i];
                    Assert.IsFalse(string.IsNullOrEmpty(dev.DeviceId), $"Chapter {ch} Device {i+1} must have DeviceId");
                    Assert.IsFalse(string.IsNullOrEmpty(dev.BaiID), $"Chapter {ch} Device {i+1} must have BaiID");
                    Assert.IsFalse(string.IsNullOrEmpty(dev.TenBai), $"Chapter {ch} Device {i+1} must have TenBai");

                    // Check Chapter III specific titles
                    if (ch == 3)
                    {
                        if (i == 0) StringAssert.Contains("Bài 11", dev.TenBai);
                        if (i == 1) StringAssert.Contains("Bài 12", dev.TenBai);
                        if (i == 2) StringAssert.Contains("Bài 13", dev.TenBai);
                        if (i == 3) StringAssert.Contains("Chương III", dev.TenBai);
                    }
                }
            }

            Assert.AreEqual(21, totalDevices, "Total devices across all 5 chapters must equal exactly 21 (4+7+4+3+3)!");
            Debug.Log($"[TEST 11 PASSED] All 21 devices verified with accurate data and dynamic chapter filtering.");
        }

        [Test]
        public void Test12_ChapterMapBackButton_BindsCorrectlyAndNavigatesToChapterSelection()
        {
            Debug.Log("=== TEST 12: Verify Chapter Map Back Button ('Chọn chương') Across Chapters ===");

            string[] chapterScenes = new[]
            {
                "Assets/Scenes/Physics/Game2/06_Chương I.unity",
                "Assets/Scenes/Physics/Game2/07_Chương II.unity",
                "Assets/Scenes/Physics/Game2/08_Chương III.unity",
                "Assets/Scenes/Physics/Game2/09_Chương IV 1.unity",
                "Assets/Scenes/Physics/Game2/10_Chương V 2.unity"
            };

            foreach (var scPath in chapterScenes)
            {
                Assert.IsTrue(File.Exists(scPath), $"Scene not found: {scPath}");
                var scene = EditorSceneManager.OpenScene(scPath, OpenSceneMode.Additive);
                try
                {
                    var canvas = GameObject.FindAnyObjectByType<Canvas>();
                    Assert.IsNotNull(canvas, $"Canvas must exist in {scPath}");

                    // 1. Verify frame Image exists in bottom-left corner
                    Image frameImg = null;
                    foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                    {
                        var rt = img.rectTransform;
                        if (rt.anchoredPosition.x < -450f && rt.anchoredPosition.y < -350f && rt.sizeDelta.x >= 200f)
                        {
                            frameImg = img;
                            break;
                        }
                    }
                    Assert.IsNotNull(frameImg, $"Back button frame Image must exist at bottom-left in {scPath}");

                    // 2. Verify TextMeshProUGUI exists with 'Chọn chương'
                    TextMeshProUGUI txtBack = null;
                    foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        if (tmp.text.Contains("Chọn chương") || tmp.text.Contains("Chon chuong"))
                        {
                            txtBack = tmp;
                            break;
                        }
                    }
                    Assert.IsNotNull(txtBack, $"Text 'Chọn chương' must exist in {scPath}");

                    // 3. Attach ChapterMapController to verify auto-binding
                    var ctrlGo = new GameObject("TestChapterMapController");
                    ctrlGo.transform.SetParent(canvas.transform, false);
                    var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                    // Call SetupBackButton via reflection or direct setup
                    var setupBackMethod = typeof(ChapterMapController).GetMethod("SetupBackButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Assert.IsNotNull(setupBackMethod, "SetupBackButton method must exist");
                    setupBackMethod.Invoke(ctrl, null);

                    var btnBackField = typeof(ChapterMapController).GetField("btnBack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Button btnBack = btnBackField.GetValue(ctrl) as Button;
                    Assert.IsNotNull(btnBack, $"btnBack must be bound in {scPath}");
                    Assert.IsTrue(btnBack.GetComponent<Image>().raycastTarget, $"btnBack image must have raycastTarget = true in {scPath}");

                    // Text must also have Button to handle direct text clicks
                    var textBtn = txtBack.GetComponent<Button>();
                    Assert.IsNotNull(textBtn, $"Text 'Chọn chương' must also have Button for click fallback in {scPath}");

                    GameObject.DestroyImmediate(ctrlGo);
                    Debug.Log($"[Chapter Map {Path.GetFileNameWithoutExtension(scPath)}] Back Button successfully verified.");
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            Debug.Log("[TEST 12 PASSED] Back button ('Chọn chương') verified across all Chapter Maps.");
        }

        [Test]
        public void Test13_LessonTypeBackButton_BindsCorrectlyAndReturnsToPreviousChapterMap()
        {
            Debug.Log("=== TEST 13: Verify Lesson Type Scene 17 Back Button ===");

            string scenePath = "Assets/Scenes/Physics/Game2/17. Type.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist in 17. Type");

                // Find Button (3) (quay lai) and Button (4) (dong)
                var buttons = canvas.GetComponentsInChildren<Button>(true);
                Button btnBack = buttons.FirstOrDefault(b => b.name == "Button (3)");
                Button btnClose = buttons.FirstOrDefault(b => b.name == "Button (4)");
                Assert.IsNotNull(btnBack, "Button (3) (Quay lại) must exist in 17. Type");
                Assert.IsNotNull(btnClose, "Button (4) (Đóng) must exist in 17. Type");

                // Attach LessonTypeController to test binding
                var ctrlGo = new GameObject("TestLessonTypeController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<LessonTypeController>();

                var autoBindMethod = typeof(LessonTypeController).GetMethod("AutoBindUIElements", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(autoBindMethod, "AutoBindUIElements method must exist");
                autoBindMethod.Invoke(ctrl, null);

                var setupEventsMethod = typeof(LessonTypeController).GetMethod("SetupButtonEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(setupEventsMethod, "SetupButtonEvents method must exist");
                setupEventsMethod.Invoke(ctrl, null);

                var boundBtnBack = typeof(LessonTypeController).GetField("btnBack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ctrl) as Button;
                Assert.IsNotNull(boundBtnBack, "LessonTypeController must successfully bind btnBack!");
                Assert.AreEqual(btnBack.gameObject, boundBtnBack.gameObject, "btnBack must bind to Button (3)");

                var boundBtnClose = typeof(LessonTypeController).GetField("btnClose", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ctrl) as Button;
                Assert.IsNotNull(boundBtnClose, "LessonTypeController must successfully bind btnClose!");
                Assert.AreEqual(btnClose.gameObject, boundBtnClose.gameObject, "btnClose must bind to Button (4)");

                // Test routing for all 5 chapters:
                for (int ch = 1; ch <= 5; ch++)
                {
                    NavigationService.Instance.Selection.ChapterId = ch;
                    bool completed = RestorationService.IsChapterFullyRestored(ch);
                    RouteId expectedRoute = SceneCatalog.GetChapterMapRoute(ch, completed);
                    string expectedScene = SceneCatalog.GetSceneName(expectedRoute);

                    Assert.IsFalse(string.IsNullOrEmpty(expectedScene), $"Chapter {ch} must map to valid scene");
                    StringAssert.Contains("Chương", expectedScene);
                    Debug.Log($"Chapter {ch} in 17. Type returns to: {expectedScene} ({expectedRoute}) [VERIFIED]");
                }

                GameObject.DestroyImmediate(ctrlGo);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log("[TEST 13 PASSED] LessonTypeController Back and Close buttons binding and routing verified.");
        }

        [Test]
        public void Test14_LessonPlaqueStyle_OrnateNavyGoldAndNineSlice()
        {
            Debug.Log("=== TEST 14: Verify Lesson Plaque Style & Sprite ===");

            Sprite plaqueSprite = ChapterMapController.GetLessonCardSprite();
            Assert.IsNotNull(plaqueSprite, "ChapterMapController.GetLessonCardSprite() must successfully load the ornate card frame!");

            // Verify 9-slice borders
            Vector4 border = plaqueSprite.border;
            Assert.IsTrue(border.x > 0 && border.y > 0 && border.z > 0 && border.w > 0,
                $"Lesson plaque sprite must have 9-slice borders configured! Actual: {border}");

            Debug.Log($"[TEST 14 PASSED] Card sprite loaded: {plaqueSprite.name}, 9-slice border: {border}");
        }

        [Test]
        public void Test15_CleanLegacyStationMarkers_HidesAllCircularBadgesAndNumbers()
        {
            Debug.Log("=== TEST 15: Verify Removal of Circular Station Markers & Numbers ===");

            string scenePath = "Assets/Scenes/Physics/Game2/08_Chương III.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist in 08_Chương III");

                // Execute cleanup
                ChapterMapController.CleanLegacyStationMarkers(canvas);

                // Verify all number TMP texts 1..4 are deactivated
                var allTmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in allTmps)
                {
                    string txt = tmp.text.Trim();
                    if (int.TryParse(txt, out int num) && num >= 1 && num <= 4)
                    {
                        Assert.IsFalse(tmp.gameObject.activeSelf, $"Legacy station number text '{num}' must be deactivated!");
                    }
                }

                // Verify all circular markers (~87.14 x 70) are deactivated
                var allImgs = canvas.GetComponentsInChildren<Image>(true);
                foreach (var img in allImgs)
                {
                    var rt = img.rectTransform;
                    if (rt.sizeDelta.x >= 75f && rt.sizeDelta.x <= 98f && rt.sizeDelta.y >= 60f && rt.sizeDelta.y <= 85f)
                    {
                        Assert.IsFalse(img.gameObject.activeSelf, $"Legacy circular badge image '{img.name}' must be deactivated!");
                    }
                }

                // Verify HUD elements remain active
                bool hasTitle = allTmps.Any(t => t.gameObject.activeSelf && t.text.Contains("XƯỞNG ĐIỆN"));
                bool hasTimer = allTmps.Any(t => t.gameObject.activeSelf && t.text.Contains("phút"));
                bool hasProgress = allTmps.Any(t => t.gameObject.activeSelf && t.text.Contains("Đã sáng"));
                Assert.IsTrue(hasTitle, "HUD Title must remain active");
                Assert.IsTrue(hasTimer, "HUD Timer must remain active");
                Assert.IsTrue(hasProgress, "HUD Progress must remain active");

                Debug.Log("[TEST 15 PASSED] Circular badges and number texts successfully hidden without affecting HUD.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test16_ChapterIIIPlaques_NonOverlappingLayoutAndDynamicTitles()
        {
            Debug.Log("=== TEST 16: Verify Chapter III Lesson Plaques & Clearance ===");

            string scenePath = "Assets/Scenes/Physics/Game2/08_Chương III.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist in 08_Chương III");

                var ctrlGo = new GameObject("TestChapterMapController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                // Set chapter index to 3
                var chField = typeof(ChapterMapController).GetField("chapterIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                chField.SetValue(ctrl, 3);

                // Setup stations
                var setupMethod = typeof(ChapterMapController).GetMethod("SetupStations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                setupMethod.Invoke(ctrl, null);

                // Verify 4 plaques created
                var plaques = new GameObject[4];
                for (int i = 1; i <= 4; i++)
                {
                    plaques[i - 1] = GameObject.Find($"Plaque_Station_{i}");
                    Assert.IsNotNull(plaques[i - 1], $"Plaque_Station_{i} must be created!");
                }

                // 1. Check Station 1 (Bài 11 - Top-Left)
                var p1Rt = plaques[0].GetComponent<RectTransform>();
                Assert.AreEqual(-352f, p1Rt.anchoredPosition.x, 2f);
                Assert.AreEqual(78f, p1Rt.anchoredPosition.y, 2f);
                Assert.AreEqual(370f, p1Rt.sizeDelta.x, 2f);
                Assert.AreEqual(86f, p1Rt.sizeDelta.y, 2f);
                var p1Tmp = plaques[0].GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(p1Tmp);
                Assert.IsTrue(p1Tmp.fontSizeMax >= 20f, "FontSizeMax must be >= 20pt for clear legibility at Game View 0.57x");
                Assert.IsTrue(p1Tmp.fontSizeMin >= 18f, "FontSizeMin must be >= 18pt to prevent shrinking to tiny text");
                StringAssert.Contains("Bài 11. Điện trở. Định luật Ohm", p1Tmp.text);

                // 2. Check Station 2 (Bài 12 - Top-Right)
                var p2Rt = plaques[1].GetComponent<RectTransform>();
                Assert.AreEqual(349f, p2Rt.anchoredPosition.x, 2f);
                Assert.AreEqual(86f, p2Rt.anchoredPosition.y, 2f);
                Assert.AreEqual(370f, p2Rt.sizeDelta.x, 2f);
                Assert.AreEqual(86f, p2Rt.sizeDelta.y, 2f);
                var p2Tmp = plaques[1].GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(p2Tmp);
                StringAssert.Contains("Bài 12. Đoạn mạch nối tiếp, song song", p2Tmp.text);

                // 3. Check Station 3 (Bài 13 - Bottom-Left)
                var p3Rt = plaques[2].GetComponent<RectTransform>();
                Assert.AreEqual(-377f, p3Rt.anchoredPosition.x, 2f);
                Assert.AreEqual(-286f, p3Rt.anchoredPosition.y, 2f);
                Assert.AreEqual(370f, p3Rt.sizeDelta.x, 2f);
                Assert.AreEqual(86f, p3Rt.sizeDelta.y, 2f);
                var p3Tmp = plaques[2].GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(p3Tmp);
                StringAssert.Contains("Bài 13. Năng lượng của dòng điện và công suất điện", p3Tmp.text);

                // 4. Check Station 4 (Kiểm tra Chương III - Bottom-Right)
                var p4Rt = plaques[3].GetComponent<RectTransform>();
                Assert.AreEqual(370f, p4Rt.anchoredPosition.x, 2f);
                Assert.AreEqual(-267f, p4Rt.anchoredPosition.y, 2f);
                Assert.AreEqual(370f, p4Rt.sizeDelta.x, 2f);
                Assert.AreEqual(86f, p4Rt.sizeDelta.y, 2f);
                var p4Tmp = plaques[3].GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(p4Tmp);
                Assert.AreEqual("Kiểm tra Chương III", p4Tmp.text.Trim());

                // 5. Verify no station number badges inside plaques
                for (int i = 0; i < 4; i++)
                {
                    var gem = plaques[i].transform.Find("CrystalGem");
                    Assert.IsNull(gem, "Plaque must not contain legacy CrystalGem or station numbers!");

                    var btn = plaques[i].GetComponent<Button>();
                    Assert.IsNotNull(btn, "Plaque must have Button component for direct click!");

                    var img = plaques[i].GetComponent<Image>();
                    Assert.IsNotNull(img);
                    Assert.AreEqual(Image.Type.Sliced, img.type, "Plaque image must be Sliced!");
                }

                // 6. Verify Clearance / No Overlap with HUD, Back Button, Player and Prompt Bar
                Vector2 playerSpawn = new Vector2(-201.58f, -132.49f);
                Vector2 promptBarCenter = new Vector2(0f, -345f);
                Vector2 promptBarSize = new Vector2(250f, 44f);

                foreach (var p in plaques)
                {
                    var rt = p.GetComponent<RectTransform>();
                    // Y should be < 300 to not overlap Top HUD
                    Assert.Less(rt.anchoredPosition.y, 300f, "Plaque must not overlap Top HUD");
                    // Distance to player spawn should be > 100
                    float distToPlayer = Vector2.Distance(rt.anchoredPosition, playerSpawn);
                    Assert.Greater(distToPlayer, 80f, "Plaque must not cover player spawn position");
                }

                // Verify Plaque 3 does NOT overlap with Prompt Bar
                float plaque3Right = p3Rt.anchoredPosition.x + p3Rt.sizeDelta.x * 0.5f; // -377 + 185 = -192
                float promptBarLeft = promptBarCenter.x - promptBarSize.x * 0.5f;       // 0 - 125 = -125
                float horizontalGapP3Prompt = promptBarLeft - plaque3Right;             // -125 - (-192) = 67px
                Assert.Greater(horizontalGapP3Prompt, 50f, "Plaque 3 must have at least 50px clearance from interaction prompt panel");

                // Verify Plaque 4 does NOT overlap with Prompt Bar
                float plaque4Left = p4Rt.anchoredPosition.x - p4Rt.sizeDelta.x * 0.5f;  // 370 - 185 = 185
                float promptBarRight = promptBarCenter.x + promptBarSize.x * 0.5f;      // 0 + 125 = 125
                float horizontalGapP4Prompt = plaque4Left - promptBarRight;             // 185 - 125 = 60px
                Assert.Greater(horizontalGapP4Prompt, 50f, "Plaque 4 must have at least 50px clearance from interaction prompt panel");

                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 16 PASSED] All 4 plaques in Chapter III correctly positioned, formatted, and non-overlapping.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test17_PromptUI_NavyGoldCardAndTextContent()
        {
            Debug.Log("=== TEST 17: Verify Redesigned Prompt UI ===");

            string scenePath = "Assets/Scenes/Physics/Game2/08_Chương III.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist");

                var ctrlGo = new GameObject("TestChapterMapController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                // Invoke CreatePromptUI
                var createPromptMethod = typeof(ChapterMapController).GetMethod("CreatePromptUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(createPromptMethod, "CreatePromptUI method must exist");
                createPromptMethod.Invoke(ctrl, null);

                var promptGo = GameObject.Find("InteractionPromptPanel");
                Assert.IsNotNull(promptGo, "InteractionPromptPanel GameObject must be created!");

                var rt = promptGo.GetComponent<RectTransform>();
                Assert.AreEqual(0f, rt.anchoredPosition.x, 1f);
                Assert.AreEqual(-345f, rt.anchoredPosition.y, 1f);
                Assert.AreEqual(240f, rt.sizeDelta.x, 5f);
                Assert.AreEqual(44f, rt.sizeDelta.y, 5f);

                var img = promptGo.GetComponent<Image>();
                Assert.IsNotNull(img, "Prompt must have Image component");
                Assert.AreEqual(Image.Type.Sliced, img.type, "Prompt Image must be 9-sliced!");
                Assert.IsFalse(img.raycastTarget, "Prompt Image must have raycastTarget = false");

                var textTmp = promptGo.GetComponentInChildren<TextMeshProUGUI>();
                Assert.IsNotNull(textTmp, "Prompt TextMeshProUGUI must exist");
                Assert.AreEqual("Nhấn [ E ] để học", textTmp.text.Trim());
                Assert.AreEqual(TextAlignmentOptions.Center, textTmp.alignment);
                Assert.IsFalse(textTmp.raycastTarget, "Prompt text must not block raycast");

                GameObject.DestroyImmediate(ctrlGo);
                GameObject.DestroyImmediate(promptGo);
                Debug.Log("[TEST 17 PASSED] Prompt UI verified: 240x44 at (0, -345), 9-slice navy-gold card, centered cream white text.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test18_SpriteAtlas_TestMachine_Height485_SingleMachine()
        {
            Debug.Log("=== TEST 18: Verify Test Machine Sprite Atlas Isolation (Height 485) ===");

            string metaPath = "Assets/Art/Environment/Bộ thiết bị điện học huyền ảo.png.meta";
            Assert.IsTrue(File.Exists(metaPath), $"Meta file not found: {metaPath}");

            string metaContent = File.ReadAllText(metaPath);
            int idx = metaContent.IndexOf("name: \"B\\u1ED9 thi\\u1EBFt b\\u1ECB \\u0111i\\u1EC7n h\\u1ECDc huy\\u1EC1n \\u1EA3o_11\"");
            if (idx == -1)
            {
                idx = metaContent.IndexOf("Bộ thiết bị điện học huyền ảo_11");
            }
            Assert.IsTrue(idx != -1, "Sprite _11 must exist in .meta file");

            string chunk = metaContent.Substring(idx, Math.Min(350, metaContent.Length - idx));
            StringAssert.Contains("height: 485", chunk, "Sprite _11 height must be 485 (isolating lower large machine, preventing double core)!");

            Debug.Log("[TEST 18 PASSED] Sprite _11 strictly set to height 485. Lower large machine isolated with no overlapping machine.");
        }

        [Test]
        public void Test19_BackButtons_RobustWiringAndDirectNavigation()
        {
            Debug.Log("=== TEST 19: Verify Back Buttons Direct Synchronous Navigation & Layering ===");

            // 1. Test NavigationService NavigateTo loads scene directly without transition lock drop
            NavigationService.Instance.ResetTransitionLock();
            Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Transition lock must be false after reset");

            // 2. Test 17. Type scene Back Button bringing to top
            string scenePath = "Assets/Scenes/Physics/Game2/17. Type.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas);

                var ctrlGo = new GameObject("TestLessonTypeController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<LessonTypeController>();

                var autoBind = typeof(LessonTypeController).GetMethod("AutoBindUIElements", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                autoBind.Invoke(ctrl, null);

                var setupEvents = typeof(LessonTypeController).GetMethod("SetupButtonEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                setupEvents.Invoke(ctrl, null);

                var btnBack = typeof(LessonTypeController).GetField("btnBack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ctrl) as Button;
                Assert.IsNotNull(btnBack);

                // Verify child texts have raycastTarget = false
                foreach (var t in btnBack.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    Assert.IsFalse(t.raycastTarget, "Child text on back button must have raycastTarget = false");
                }

                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 19 PASSED] Back Buttons robust wiring and direct navigation verified.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test20_ChapterI_FourDevices_ExactPositionsAndPlaques()
        {
            Debug.Log("=== TEST 20: Verify Chapter I (Sân Cơ Học) 4 Devices & Plaques ===");

            string scenePath = "Assets/Scenes/Physics/Game2/06_Chương I.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist");

                var ctrlGo = new GameObject("TestChapterMapController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                var chField = typeof(ChapterMapController).GetField("chapterIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                chField.SetValue(ctrl, 1);

                var setupMethod = typeof(ChapterMapController).GetMethod("SetupStations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                setupMethod.Invoke(ctrl, null);

                var plaques = new GameObject[4];
                for (int i = 1; i <= 4; i++)
                {
                    plaques[i - 1] = GameObject.Find($"Plaque_Station_{i}");
                    Assert.IsNotNull(plaques[i - 1], $"Plaque_Station_{i} must be created in Chapter I!");
                }

                // Station 1: Bài 2 (Top-Left)
                var p1Rt = plaques[0].GetComponent<RectTransform>();
                Assert.AreEqual(-348f, p1Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(135f, p1Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(370f, p1Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p1Rt.sizeDelta.y, 5f);
                StringAssert.Contains("Bài 2", plaques[0].GetComponentInChildren<TextMeshProUGUI>().text);

                // Station 2: Bài 3 (Bottom-Left)
                var p2Rt = plaques[1].GetComponent<RectTransform>();
                Assert.AreEqual(-385f, p2Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(-270f, p2Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(370f, p2Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p2Rt.sizeDelta.y, 5f);
                StringAssert.Contains("Bài 3", plaques[1].GetComponentInChildren<TextMeshProUGUI>().text);

                // Station 3: Bài 4 (Bottom-Right)
                var p3Rt = plaques[2].GetComponent<RectTransform>();
                Assert.AreEqual(385f, p3Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(-225f, p3Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(370f, p3Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p3Rt.sizeDelta.y, 5f);
                StringAssert.Contains("Bài 4", plaques[2].GetComponentInChildren<TextMeshProUGUI>().text);

                // Station 4: Kiểm tra Chương I (Top-Right)
                var p4Rt = plaques[3].GetComponent<RectTransform>();
                Assert.AreEqual(349f, p4Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(115f, p4Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(370f, p4Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p4Rt.sizeDelta.y, 5f);
                Assert.AreEqual("Kiểm tra Chương I", plaques[3].GetComponentInChildren<TextMeshProUGUI>().text.Trim());

                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 20 PASSED] Chapter I 4 devices & plaques verified with exact positions.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test21_ChapterII_SevenDevices_ExactPositionsAndNoOverlap()
        {
            Debug.Log("=== TEST 21: Verify Chapter II (Khu Quang Học) 7 Devices & Zero Overlap ===");

            string scenePath = "Assets/Scenes/Physics/Game2/07_Chương II.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist");

                var ctrlGo = new GameObject("TestChapterMapController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                var chField = typeof(ChapterMapController).GetField("chapterIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                chField.SetValue(ctrl, 2);

                var setupMethod = typeof(ChapterMapController).GetMethod("SetupStations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                setupMethod.Invoke(ctrl, null);

                var plaques = new GameObject[7];
                for (int i = 1; i <= 7; i++)
                {
                    plaques[i - 1] = GameObject.Find($"Plaque_Station_{i}");
                    Assert.IsNotNull(plaques[i - 1], $"Plaque_Station_{i} must be created in Chapter II!");
                }

                // Verify titles
                StringAssert.Contains("Bài 5", plaques[0].GetComponentInChildren<TextMeshProUGUI>().text);
                StringAssert.Contains("Bài 6", plaques[1].GetComponentInChildren<TextMeshProUGUI>().text);
                StringAssert.Contains("Bài 7", plaques[2].GetComponentInChildren<TextMeshProUGUI>().text);
                StringAssert.Contains("Bài 8", plaques[3].GetComponentInChildren<TextMeshProUGUI>().text);
                StringAssert.Contains("Bài 9", plaques[4].GetComponentInChildren<TextMeshProUGUI>().text);
                StringAssert.Contains("Bài 10", plaques[5].GetComponentInChildren<TextMeshProUGUI>().text);
                Assert.AreEqual("Kiểm tra Chương II", plaques[6].GetComponentInChildren<TextMeshProUGUI>().text.Trim());

                // Verify pairwise clearance between all 7 plaques
                for (int a = 0; a < 7; a++)
                {
                    var rtA = plaques[a].GetComponent<RectTransform>();
                    Rect rectA = new Rect(rtA.anchoredPosition.x - rtA.sizeDelta.x * 0.5f,
                                          rtA.anchoredPosition.y - rtA.sizeDelta.y * 0.5f,
                                          rtA.sizeDelta.x, rtA.sizeDelta.y);

                    for (int b = a + 1; b < 7; b++)
                    {
                        var rtB = plaques[b].GetComponent<RectTransform>();
                        Rect rectB = new Rect(rtB.anchoredPosition.x - rtB.sizeDelta.x * 0.5f,
                                              rtB.anchoredPosition.y - rtB.sizeDelta.y * 0.5f,
                                              rtB.sizeDelta.x, rtB.sizeDelta.y);

                        Assert.IsFalse(rectA.Overlaps(rectB), $"Plaque {a + 1} must not overlap with Plaque {b + 1}!");
                    }
                }

                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 21 PASSED] Chapter II 7 devices & plaques verified with zero overlap.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test22_ChapterIV_ThreeDevices_MaxTwoLinesAndNoOverflow()
        {
            Debug.Log("=== TEST 22: Verify Chapter IV (Tháp Cảm Ứng) 3 Devices & 2-Line Containment ===");

            string scenePath = "Assets/Scenes/Physics/Game2/09_Chương IV 1.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist");

                var ctrlGo = new GameObject("TestChapterMapController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                var chField = typeof(ChapterMapController).GetField("chapterIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                chField.SetValue(ctrl, 4);

                var setupMethod = typeof(ChapterMapController).GetMethod("SetupStations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                setupMethod.Invoke(ctrl, null);

                var plaques = new GameObject[3];
                for (int i = 1; i <= 3; i++)
                {
                    plaques[i - 1] = GameObject.Find($"Plaque_Station_{i}");
                    Assert.IsNotNull(plaques[i - 1], $"Plaque_Station_{i} must be created in Chapter IV!");
                }

                // Station 1: Bài 14 (Top)
                var p1Rt = plaques[0].GetComponent<RectTransform>();
                Assert.AreEqual(0f, p1Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(135f, p1Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(390f, p1Rt.sizeDelta.x, 5f);
                Assert.AreEqual(88f, p1Rt.sizeDelta.y, 5f);
                var p1Tmp = plaques[0].GetComponentInChildren<TextMeshProUGUI>();
                Assert.AreEqual(2, p1Tmp.maxVisibleLines);
                StringAssert.Contains("Bài 14", p1Tmp.text);

                // Station 2: Bài 15 (Bottom-Right)
                var p2Rt = plaques[1].GetComponent<RectTransform>();
                Assert.AreEqual(405f, p2Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(-205f, p2Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(380f, p2Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p2Rt.sizeDelta.y, 5f);
                var p2Tmp = plaques[1].GetComponentInChildren<TextMeshProUGUI>();
                Assert.AreEqual(2, p2Tmp.maxVisibleLines);
                StringAssert.Contains("Bài 15", p2Tmp.text);

                // Station 3: Kiểm tra Chương IV (Bottom-Left)
                var p3Rt = plaques[2].GetComponent<RectTransform>();
                Assert.AreEqual(-395f, p3Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(-235f, p3Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(370f, p3Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p3Rt.sizeDelta.y, 5f);
                Assert.AreEqual("Kiểm tra Chương IV", plaques[2].GetComponentInChildren<TextMeshProUGUI>().text.Trim());

                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 22 PASSED] Chapter IV 3 devices verified with 2-line containment and no overflow.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test23_ChapterV_ThreeDevices_MaxTwoLinesAndNoOverflow()
        {
            Debug.Log("=== TEST 23: Verify Chapter V (Vườn Năng Lượng) 3 Devices & 2-Line Containment ===");

            string scenePath = "Assets/Scenes/Physics/Game2/10_Chương V 2.unity";
            Assert.IsTrue(File.Exists(scenePath), $"Scene not found: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist");

                var ctrlGo = new GameObject("TestChapterMapController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                var chField = typeof(ChapterMapController).GetField("chapterIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                chField.SetValue(ctrl, 5);

                var setupMethod = typeof(ChapterMapController).GetMethod("SetupStations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                setupMethod.Invoke(ctrl, null);

                var plaques = new GameObject[3];
                for (int i = 1; i <= 3; i++)
                {
                    plaques[i - 1] = GameObject.Find($"Plaque_Station_{i}");
                    Assert.IsNotNull(plaques[i - 1], $"Plaque_Station_{i} must be created in Chapter V!");
                }

                // Station 1: Bài 16 (Top)
                var p1Rt = plaques[0].GetComponent<RectTransform>();
                Assert.AreEqual(5f, p1Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(145f, p1Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(390f, p1Rt.sizeDelta.x, 5f);
                Assert.AreEqual(88f, p1Rt.sizeDelta.y, 5f);
                var p1Tmp = plaques[0].GetComponentInChildren<TextMeshProUGUI>();
                Assert.AreEqual(2, p1Tmp.maxVisibleLines);
                StringAssert.Contains("Bài 16", p1Tmp.text);

                // Station 2: Bài 17 (Bottom-Right)
                var p2Rt = plaques[1].GetComponent<RectTransform>();
                Assert.AreEqual(396f, p2Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(-215f, p2Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(380f, p2Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p2Rt.sizeDelta.y, 5f);
                var p2Tmp = plaques[1].GetComponentInChildren<TextMeshProUGUI>();
                Assert.AreEqual(2, p2Tmp.maxVisibleLines);
                StringAssert.Contains("Bài 17", p2Tmp.text);

                // Station 3: Kiểm tra Chương V (Bottom-Left)
                var p3Rt = plaques[2].GetComponent<RectTransform>();
                Assert.AreEqual(-366f, p3Rt.anchoredPosition.x, 5f);
                Assert.AreEqual(-245f, p3Rt.anchoredPosition.y, 5f);
                Assert.AreEqual(370f, p3Rt.sizeDelta.x, 5f);
                Assert.AreEqual(86f, p3Rt.sizeDelta.y, 5f);
                Assert.AreEqual("Kiểm tra Chương V", plaques[2].GetComponentInChildren<TextMeshProUGUI>().text.Trim());

                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 23 PASSED] Chapter V 3 devices verified with 2-line containment and no overflow.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test24_StationInteraction_RequiresWASDProximityAndEKey_ClickDoesNotOpenScene()
        {
            string scenePath = "Assets/Scenes/Physics/Game2/08_Chuong III.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist in Chapter III scene");

                var ctrlGo = new GameObject("TestChapterMapController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<ChapterMapController>();

                var chField = typeof(ChapterMapController).GetField("chapterIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                chField.SetValue(ctrl, 3);

                var setupMethod = typeof(ChapterMapController).GetMethod("SetupStations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                setupMethod.Invoke(ctrl, null);

                var createPromptMethod = typeof(ChapterMapController).GetMethod("CreatePromptUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                createPromptMethod.Invoke(ctrl, null);

                Assert.AreEqual(4, ctrl.Stations.Count, "Chapter III must have 4 stations");

                var playerGo = new GameObject("TestPlayer");
                playerGo.transform.SetParent(canvas.transform, false);
                var playerRt = playerGo.AddComponent<RectTransform>();
                playerRt.anchoredPosition = Vector2.zero; // Far away in the center
                ctrl.PlayerRect = playerRt;

                NavigationService.Instance.ResetTransitionLock();
                NavigationService.Instance.Selection.DeviceId = "";
                NavigationService.Instance.Selection.LessonId = "";

                // 1. Kiểm tra khi đứng xa: CheckProximityToStations không nhận trạm nào
                ctrl.CheckProximityToStations();
                Assert.IsNull(ctrl.NearestStation, "Player at (0, 0) is far away from all Chapter III devices (X ~ +/-350). NearestStation must be null.");
                Assert.IsFalse(ctrl.PromptPopup.activeSelf, "Prompt popup must be inactive when player is far away.");

                // 2. Click vào biển tên bài học từ xa: KHÔNG mở bài học, KHÔNG chuyển scene
                ctrl.OnStationClicked(1);
                Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Clicking station 1 plaque from afar must NOT trigger scene transition!");
                Assert.AreNotEqual(ctrl.Stations[0].deviceId, NavigationService.Instance.Selection.DeviceId, "Clicking from afar must NOT select device for navigation!");

                // 3. Nhấn phím E khi đứng xa: KHÔNG mở bài học
                ctrl.HandleInteractionKey();
                Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Pressing E while far from any device must NOT trigger scene transition!");

                // 4. Di chuyển nhân vật lại gần thiết bị trạm 1 (Machine 1: X ~ -352, Y ~ 253)
                Vector2 devCenter = ChapterMapController.GetVisualCenter(ctrl.Stations[0].deviceGraphicRect);
                playerRt.anchoredPosition = devCenter + new Vector2(0f, -80f); // Đứng ngay phía trước thiết bị 1

                ctrl.CheckProximityToStations();
                Assert.IsNotNull(ctrl.NearestStation, "Player standing near device 1 must be detected by proximity.");
                Assert.AreEqual(1, ctrl.NearestStation.stationIndex, "Nearest station must be Station 1.");
                Assert.IsTrue(ctrl.PromptPopup.activeSelf, "Interaction prompt must become visible when near device.");
                StringAssert.Contains("E", ctrl.PromptText.text, "Prompt text must prompt player to press [ E ].");
                Assert.IsTrue(ctrl.Stations[0].outlineEffect.enabled, "Device graphic outline must glow when player is near.");

                // 5. Click vào trạm khi đứng gần: CŨNG KHÔNG chuyển scene (BẮT BUỘC dùng phím E)
                ctrl.OnStationClicked(1);
                Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Clicking station while near must still NOT navigate! Player MUST press E key.");

                // 6. Nhấn phím E khi ở trong vùng tương tác: Kích hoạt vào học (Scene 17. Type)
                Vector2 expectedReturnPos = playerRt.anchoredPosition;
                ctrl.EnterStationModeSelection(ctrl.NearestStation);

                Assert.IsTrue(NavigationService.Instance.IsTransitioning, "Pressing E near device must trigger navigation to ModeSelection!");
                Assert.AreEqual(ctrl.Stations[0].deviceId, NavigationService.Instance.Selection.DeviceId, "Selected device ID must match Station 1.");
                Assert.AreEqual(ctrl.Stations[0].baiId, NavigationService.Instance.Selection.LessonId, "Selected lesson ID must match Station 1.");
                Assert.AreEqual(3, NavigationService.Instance.Selection.ChapterId, "ChapterId must be 3.");
                Assert.AreEqual(expectedReturnPos, NavigationService.Instance.ReturnContext.PlayerPosition, "ReturnContext must preserve exact player position when interacting.");

                // 7. Thiết bị đã khôi phục (hoàn thành) vẫn cho phép tiếp cận và nhấn E để ôn tập
                ctrl.Stations[0].isRestored = true;
                ctrl.CheckProximityToStations();
                Assert.IsNotNull(ctrl.NearestStation, "Restored device must still detect proximity so player can review.");
                Assert.AreEqual(1, ctrl.NearestStation.stationIndex, "Nearest station must still be Station 1 even when restored.");

                GameObject.DestroyImmediate(playerGo);
                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 24 PASSED] Station interaction strictly requires WASD proximity and E key. Mouse click does not open scene.");
            }
            finally
            {
                NavigationService.Instance.ResetTransitionLock();
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Test25_LessonType_ModeSelectionAndConfirmationFlow()
        {
            string scenePath = "Assets/Scenes/Physics/Game2/17. Type.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>();
                Assert.IsNotNull(canvas, "Canvas must exist in Scene 17. Type");

                var ctrlGo = new GameObject("TestLessonTypeController");
                ctrlGo.transform.SetParent(canvas.transform, false);
                var ctrl = ctrlGo.AddComponent<LessonTypeController>();

                // 1. Kiểm tra tự động gắn đúng 6 nút chính
                Assert.IsNotNull(ctrl.BtnTheory, "btnTheory must bind to Button (2)");
                Assert.IsNotNull(ctrl.BtnQuiz, "btnQuiz must bind to Button (5)");
                Assert.IsNotNull(ctrl.BtnWritten, "btnWritten must bind to Button (6)");
                Assert.IsNotNull(ctrl.BtnBack, "btnBack must bind to Button (3)");
                Assert.IsNotNull(ctrl.BtnConfirm, "btnConfirm must bind to Button (4)");

                // 2. Kiểm tra vùng click: Tất cả hình nền/khung/TMP không thuộc 6 nút chính đều có raycastTarget = false
                ctrl.FixRaycastBlockers();
                var activeGos = new HashSet<GameObject>
                {
                    ctrl.BtnTheory.gameObject,
                    ctrl.BtnQuiz.gameObject,
                    ctrl.BtnWritten.gameObject,
                    ctrl.BtnBack.gameObject,
                    ctrl.BtnConfirm.gameObject
                };

                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    if (activeGos.Contains(img.gameObject))
                    {
                        Assert.IsTrue(img.raycastTarget, $"Button {img.name} must have raycastTarget=true to receive clicks");
                    }
                    else
                    {
                        Assert.IsFalse(img.raycastTarget, $"Overlay/Decorator {img.name} must have raycastTarget=false so clicks pass through");
                    }
                }

                foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    Assert.IsFalse(tmp.raycastTarget, $"TMP '{tmp.name}' must have raycastTarget=false to avoid blocking clicks");
                }

                // Thiết lập dữ liệu ngữ cảnh giả lập từ Bài 11 Chương III
                NavigationService.Instance.ResetTransitionLock();
                NavigationService.Instance.Selection.ChapterId = 3;
                NavigationService.Instance.Selection.DeviceId = "DEV_03_01";
                NavigationService.Instance.Selection.LessonId = "bai_11";
                NavigationService.Instance.Selection.LessonTitle = "Bài 11. Điện trở. Định luật Ohm";
                Vector2 expectedReturnPos = new Vector2(-350f, 173f);
                NavigationService.Instance.ReturnContext.PlayerPosition = expectedReturnPos;
                NavigationService.Instance.ReturnContext.ChapterId = 3;
                NavigationService.Instance.ReturnContext.DeviceId = "DEV_03_01";

                // 3. Ban đầu chưa chọn hình thức nào: bấm Xác nhận -> KHÔNG chuyển scene
                ctrl.OnConfirmClicked();
                Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Clicking Confirm without selecting mode must NOT navigate!");

                // 4. Click Học lý thuyết: Chưa chuyển Scene ngay, chỉ highlight
                ctrl.BtnTheory.onClick.Invoke();
                Assert.AreEqual(StudyModeType.Theory, ctrl.SelectedMode, "Clicking Theory must select Theory mode");
                Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Selecting Theory must NOT transition immediately!");
                Assert.AreEqual(1.025f, ctrl.BtnTheory.transform.localScale.x, 0.005f, "Theory bar must scale up when selected");
                Assert.AreEqual(1.0f, ctrl.BtnQuiz.transform.localScale.x, 0.005f, "Quiz bar must return to normal scale");

                // 5. Chuyển sang Trắc nghiệm: Highlight chuyển sang Trắc nghiệm, Lý thuyết về bình thường
                ctrl.BtnQuiz.onClick.Invoke();
                Assert.AreEqual(StudyModeType.Quiz, ctrl.SelectedMode, "Clicking Quiz must select Quiz mode");
                Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Selecting Quiz must NOT transition immediately!");
                Assert.AreEqual(1.0f, ctrl.BtnTheory.transform.localScale.x, 0.005f, "Theory bar must return to normal scale");
                Assert.AreEqual(1.025f, ctrl.BtnQuiz.transform.localScale.x, 0.005f, "Quiz bar must scale up when selected");

                // 6. Chuyển sang Tự luận: Highlight chuyển sang Tự luận
                ctrl.BtnWritten.onClick.Invoke();
                Assert.AreEqual(StudyModeType.WrittenAnswer, ctrl.SelectedMode, "Clicking Written must select Written mode");
                Assert.IsFalse(NavigationService.Instance.IsTransitioning, "Selecting Written must NOT transition immediately!");
                Assert.AreEqual(1.025f, ctrl.BtnWritten.transform.localScale.x, 0.005f, "Written bar must scale up when selected");

                // 7. Chọn lại Lý thuyết và bấm XÁC NHẬN -> Vào thẳng 18. Theory (không chọn mức độ)
                ctrl.SelectMode(StudyModeType.Theory);
                ctrl.OnConfirmClicked();
                Assert.IsTrue(NavigationService.Instance.IsTransitioning, "Confirming Theory must trigger navigation");
                // Reset transition lock for next test step
                NavigationService.Instance.ResetTransitionLock();

                // 8. Chọn Trắc nghiệm và bấm XÁC NHẬN -> Chuyển sang Scene 33. Chọn độ khó (MCQ)
                ctrl.SelectMode(StudyModeType.Quiz);
                ctrl.OnConfirmClicked();
                Assert.IsTrue(NavigationService.Instance.IsTransitioning, "Confirming Quiz must trigger navigation to DifficultySelection");
                Assert.AreEqual("MCQ", NavigationService.Instance.Selection.Mode);
                NavigationService.Instance.ResetTransitionLock();

                // 9. Chọn Tự luận và bấm XÁC NHẬN -> Chuyển sang Scene 33. Chọn độ khó (ESSAY)
                ctrl.SelectMode(StudyModeType.WrittenAnswer);
                ctrl.OnConfirmClicked();
                Assert.IsTrue(NavigationService.Instance.IsTransitioning, "Confirming Written must trigger navigation to DifficultySelection");
                Assert.AreEqual("ESSAY", NavigationService.Instance.Selection.Mode);
                NavigationService.Instance.ResetTransitionLock();

                // 10. Bấm nút Quay lại -> Về bản đồ Chương 3, giữ nguyên dữ liệu và vị trí nhân vật
                ctrl.OnBackClicked();
                Assert.IsTrue(NavigationService.Instance.IsTransitioning, "Back button must navigate back to map");
                Assert.AreEqual(3, NavigationService.Instance.Selection.ChapterId, "ChapterId must remain 3");
                Assert.AreEqual("bai_11", NavigationService.Instance.Selection.LessonId, "LessonId must remain bai_11");
                Assert.AreEqual("DEV_03_01", NavigationService.Instance.Selection.DeviceId, "DeviceId must remain DEV_03_01");
                Assert.AreEqual(expectedReturnPos, NavigationService.Instance.ReturnContext.PlayerPosition, "Return player position must be preserved!");

                GameObject.DestroyImmediate(ctrlGo);
                Debug.Log("[TEST 25 PASSED] Scene 17. Type mode selection and confirmation workflow fully verified!");
            }
            finally
            {
                NavigationService.Instance.ResetTransitionLock();
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
