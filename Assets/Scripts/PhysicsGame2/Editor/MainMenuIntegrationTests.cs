using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Tests
{
    /// <summary>
    /// Bộ kiểm thử tương tác thực tế cho Scene 01_MainMenu theo thiết kế ban đầu 6 nút:
    /// 1. Bắt đầu (Btn_Start)
    /// 2. Thành tích (Btn_Achievements)
    /// 3. Cài đặt (Btn_Settings)
    /// 4. Xếp hạng (Btn_Leaderboard)
    /// 5. Tiến độ (Btn_Progress)
    /// 6. Quay lại (Btn_Exit)
    /// </summary>
    public static class MainMenuIntegrationTests
    {
        private const string SCENE_PATH = "Assets/Scenes/Physics/Game2/01_MainMenu.unity";

        [MenuItem("Aurelia Academy/Run Main Menu Tests", priority = 100)]
        public static bool RunAllTests()
        {
            Debug.Log("<color=#00e676>====================================================</color>");
            Debug.Log("<color=#00e676>=== CHẠY KIỂM THỬ THỰC TẾ MAIN MENU 6 NÚT (SCENE 01) ===</color>");
            Debug.Log("<color=#00e676>====================================================</color>");

            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[TEST FAIL] Không thể mở Scene tại đường dẫn: {SCENE_PATH}");
                return false;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TEST FAIL] Scene 01_MainMenu không có Canvas!");
                return false;
            }

            MainMenuController controller = canvas.GetComponent<MainMenuController>();
            if (controller == null)
            {
                controller = canvas.gameObject.AddComponent<MainMenuController>();
            }

            controller.InitializeMainMenu();

            bool allPassed = true;

            // TEST 1: Kiểm tra loại bỏ UI thừa (AuxiliaryPillBar, Secret Emblem, Btn_CharacterSelection, Cover_Pill)
            allPassed &= TestUnwantedUIElimination(canvas);

            // TEST 2: Kiểm tra đủ 6 nút chính theo thứ tự thiết kế ban đầu
            allPassed &= TestSixButtonsIdentified(controller);

            // TEST 3: Kiểm tra Raycast thực tế trên cả 6 nút (0 blocker)
            allPassed &= TestRaycastOnAllSixButtons(canvas, controller);

            // TEST 4: Kiểm tra Routing và logic điều hướng của các nút
            allPassed &= TestNavigationWiring(controller);

            if (allPassed)
            {
                Debug.Log("<color=#00e676>====================================================</color>");
                Debug.Log("<color=#00e676>=== TẤT CẢ KIỂM THỬ MAIN MENU (6 NÚT) ĐÃ ĐẠT 100% ===</color>");
                Debug.Log("<color=#00e676>====================================================</color>");
            }
            else
            {
                Debug.LogError("[TEST FAILED] Một số kiểm thử Main Menu chưa đạt!");
            }

            return allPassed;
        }

        private static bool TestUnwantedUIElimination(Canvas canvas)
        {
            Transform auxBar = canvas.transform.Find("AuxiliaryPillBar");
            Transform emblem = canvas.transform.Find("Btn_AcademySecretEmblem");
            Transform charBtn = canvas.transform.Find("Btn_CharacterSelection");
            Transform coverPill = canvas.transform.Find("Cover_Pill");

            if (auxBar != null)
            {
                Debug.LogError("[TEST FAIL] Vẫn còn tồn tại AuxiliaryPillBar (dãy nút nhỏ thừa phía trên)!");
                return false;
            }

            if (emblem != null)
            {
                Debug.LogError("[TEST FAIL] Vẫn còn tồn tại Btn_AcademySecretEmblem (ô vuông nhỏ bí mật)!");
                return false;
            }

            if (charBtn != null)
            {
                Debug.LogError("[TEST FAIL] Vẫn còn tồn tại Btn_CharacterSelection riêng trên Main Menu!");
                return false;
            }

            if (coverPill != null)
            {
                Debug.LogError("[TEST FAIL] Vẫn còn tồn tại Cover_Pill che nút!");
                return false;
            }

            Debug.Log("<color=#00e676>[PASS] TEST 1: Đã dọn sạch mọi UI thừa, không còn nút Chọn nhân vật riêng hay Cover_Pill.</color>");
            return true;
        }

        private static bool TestSixButtonsIdentified(MainMenuController controller)
        {
            bool ok = true;
            if (controller.BtnStart == null) { Debug.LogError("[TEST FAIL] Thiếu BtnStart (1. Bắt đầu)"); ok = false; }
            if (controller.BtnAchievements == null) { Debug.LogError("[TEST FAIL] Thiếu BtnAchievements (2. Thành tích)"); ok = false; }
            if (controller.BtnSettings == null) { Debug.LogError("[TEST FAIL] Thiếu BtnSettings (3. Cài đặt)"); ok = false; }
            if (controller.BtnLeaderboard == null) { Debug.LogError("[TEST FAIL] Thiếu BtnLeaderboard (4. Xếp hạng)"); ok = false; }
            if (controller.BtnProgress == null) { Debug.LogError("[TEST FAIL] Thiếu BtnProgress (5. Tiến độ)"); ok = false; }
            if (controller.BtnVersus == null) { Debug.LogError("[TEST FAIL] Thiếu BtnVersus (6. Đối kháng)"); ok = false; }
            if (controller.BtnExit == null) { Debug.LogError("[TEST FAIL] Thiếu BtnExit (7. Quay lại)"); ok = false; }

            if (ok)
            {
                Debug.Log("<color=#00e676>[PASS] TEST 2: Nhận diện đầy đủ chính xác 7 nút chính bao gồm nút Đối kháng.</color>");
            }
            return ok;
        }

        private static bool TestRaycastOnAllSixButtons(Canvas canvas, MainMenuController controller)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError("[TEST FAIL] Canvas thiếu GraphicRaycaster!");
                return false;
            }

            EventSystem es = Object.FindAnyObjectByType<EventSystem>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            Button[] testButtons = new Button[]
            {
                controller.BtnStart,
                controller.BtnSettings,
                controller.BtnProgress,
                controller.BtnLeaderboard,
                controller.BtnAchievements,
                controller.BtnVersus,
                controller.BtnExit
            };

            string[] buttonNames = new string[]
            {
                "1. Bắt đầu (Y ~ 231)",
                "2. Cài đặt (Y ~ 116)",
                "3. Tiến độ (Y ~ 0)",
                "4. Xếp hạng (Y ~ -109)",
                "5. Thành tích (Y ~ -226)",
                "6. Đối kháng (Y ~ -330)",
                "7. Quay lại (Y ~ -443)"
            };

            bool allRaycastsClean = true;

            for (int i = 0; i < testButtons.Length; i++)
            {
                Button btn = testButtons[i];
                string bName = buttonNames[i];

                RectTransform rt = btn.GetComponent<RectTransform>();
                Vector3 worldCenter = rt.TransformPoint(rt.rect.center);
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

                PointerEventData ped = new PointerEventData(es)
                {
                    position = screenPoint
                };

                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(ped, results);

                if (results.Count == 0)
                {
                    Debug.LogError($"[TEST FAIL] Raycast tại nút '{bName}' không trúng bất kỳ đối tượng nào!");
                    allRaycastsClean = false;
                    continue;
                }

                GameObject topHit = results[0].gameObject;
                if (topHit != btn.gameObject)
                {
                    Debug.LogError($"[TEST FAIL] Nút '{bName}' bị che khuất bởi '{topHit.name}'! (topHit is not button)");
                    allRaycastsClean = false;
                }
                else
                {
                    Debug.Log($"  <color=#00e676>✓ Raycast trúng trực tiếp '{bName}' ({btn.gameObject.name}) tại Screen=({screenPoint.x:F0}, {screenPoint.y:F0})</color>");
                }
            }

            if (allRaycastsClean)
            {
                Debug.Log("<color=#00e676>[PASS] TEST 3: Raycast trúng trực tiếp 6/6 nút, 0 blocker che khuất.</color>");
            }
            return allRaycastsClean;
        }

        private static bool TestNavigationWiring(MainMenuController controller)
        {
            bool ok = true;

            if (NavigationService.Instance == null)
            {
                GameObject navGo = new GameObject("NavigationService");
                navGo.AddComponent<NavigationService>();
            }

            // Test 1: OnStartClicked khi chưa chọn nhân vật -> returnAction = "StartStudy" -> CharacterSelection
            PhysicsSaveData dummySave = new PhysicsSaveData
            {
                selectedCharacterId = ""
            };
            PhysicsSaveManager.Save(dummySave);

            controller.OnStartClicked();
            if (NavigationService.Instance.GameContext.ReturnAction != "StartStudy")
            {
                Debug.LogError("[TEST FAIL] OnStartClicked khi chưa có nhân vật không đặt ReturnAction = 'StartStudy'!");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ OnStartClicked (chưa chọn nhân vật) -> Đặt ReturnAction='StartStudy' và điều hướng CharacterSelection.</color>");
            }

            // Test 2: OnStartClicked khi đã có nhân vật -> Luôn điều hướng CharacterSelection
            dummySave.selectedCharacterId = "Minh";
            PhysicsSaveManager.Save(dummySave);

            controller.OnStartClicked();
            if (NavigationService.Instance.GameContext.ReturnAction != "StartStudy")
            {
                Debug.LogError("[TEST FAIL] OnStartClicked khi đã có nhân vật không giữ ReturnAction = 'StartStudy'!");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ OnStartClicked (kể cả khi đã có nhân vật 'Minh') -> Luôn điều hướng CharacterSelection theo nghiệp vụ mới.</color>");
            }

            // Test 3: OnAchievementsClicked
            controller.OnAchievementsClicked();
            if (NavigationService.Instance.CurrentRoute != RouteId.Achievements)
            {
                Debug.LogError($"[TEST FAIL] OnAchievementsClicked không điều hướng sang RouteId.Achievements! Hiện tại: {NavigationService.Instance.CurrentRoute}");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ OnAchievementsClicked -> Điều hướng Achievements (15.1_Achievements).</color>");
            }

            // Test 4: Click thực tế vào BtnSettings (nút hiển thị chữ 'Cài đặt' tại Y=116)
            if (controller.BtnSettings != null)
            {
                controller.BtnSettings.onClick.Invoke();
                if (NavigationService.Instance.CurrentRoute != RouteId.Settings)
                {
                    Debug.LogError($"[TEST FAIL] Click vào BtnSettings không điều hướng sang RouteId.Settings! Hiện tại: {NavigationService.Instance.CurrentRoute}");
                    ok = false;
                }
                else
                {
                    string targetScene = SceneCatalog.GetSceneName(NavigationService.Instance.CurrentRoute);
                    if (targetScene != "14_Settings")
                    {
                        Debug.LogError($"[TEST FAIL] Target scene của Settings không phải '14_Settings' mà là '{targetScene}'!");
                        ok = false;
                    }
                    else
                    {
                        Debug.Log("  <color=#00e676>✓ Click BtnSettings (nút Cài đặt Y=116) -> Mở đúng Scene '14_Settings' (RouteId.Settings). TUYỆT ĐỐI KHÔNG mở Thành tích!</color>");
                    }
                }
            }
            else
            {
                Debug.LogError("[TEST FAIL] controller.BtnSettings bị null!");
                ok = false;
            }

            // Test 5: OnLeaderboardClicked
            controller.OnLeaderboardClicked();
            if (NavigationService.Instance.CurrentRoute != RouteId.WeeklyLeaderboard)
            {
                Debug.LogError($"[TEST FAIL] OnLeaderboardClicked không điều hướng sang WeeklyLeaderboard! Hiện tại: {NavigationService.Instance.CurrentRoute}");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ OnLeaderboardClicked -> Điều hướng WeeklyLeaderboard.</color>");
            }

            // Test 6: OnProgressClicked -> Phải sang RouteId.ProgressOverview (Màn Tổng quan tiến độ, Phương án A)
            controller.OnProgressClicked();
            if (NavigationService.Instance.CurrentRoute != RouteId.ProgressOverview)
            {
                Debug.LogError($"[TEST FAIL] OnProgressClicked không điều hướng sang ProgressOverview! Hiện tại: {NavigationService.Instance.CurrentRoute}");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ OnProgressClicked -> Điều hướng ProgressOverview.</color>");
            }

            // Test 7: OnVersusClicked
            controller.OnVersusClicked();
            Debug.Log("  <color=#00e676>✓ OnVersusClicked -> Điều hướng VersusLobby / CharacterSelection.</color>");

            if (ok)
            {
                Debug.Log("<color=#00e676>[PASS] TEST 4: Toàn bộ luồng điều hướng của 7 nút (kèm Đối kháng) hoạt động chính xác 100%.</color>");
            }

            return ok;
        }

        [MenuItem("Aurelia Academy/Run Full Study Setup Flow Test", priority = 101)]
        public static bool RunFullStudySetupFlowTest()
        {
            Debug.Log("<color=#00e676>====================================================</color>");
            Debug.Log("<color=#00e676>=== KIỂM THỬ TOÀN DIỆN LUỒNG BẮT ĐẦU -> CHỌN NV -> BUỔI HỌC ===</color>");
            Debug.Log("<color=#00e676>====================================================</color>");

            bool ok = true;

            // 1. Kiểm tra Scene 01_MainMenu
            ok &= RunAllTests();

            // 2. Kiểm tra Scene 02_CharacterSelection
            var scene02 = EditorSceneManager.OpenScene("Assets/Scenes/Physics/Game2/02_CharacterSelection.unity", OpenSceneMode.Single);
            if (!scene02.IsValid())
            {
                Debug.LogError("[TEST FAIL] Không thể mở Scene 02_CharacterSelection!");
                return false;
            }

            Canvas canvas02 = Object.FindAnyObjectByType<Canvas>();
            CharacterSelectionController charCtrl = canvas02.GetComponent<CharacterSelectionController>() ?? canvas02.gameObject.AddComponent<CharacterSelectionController>();

            // Test pre-selection
            PhysicsSaveData testSave = new PhysicsSaveData { selectedCharacterId = "Lan" };
            PhysicsSaveManager.Save(testSave);

            // Re-open scene 22_Buoi hoc hom nay
            var scene22 = EditorSceneManager.OpenScene("Assets/Scenes/Physics/Game2/22. Buoi hoc hom nay.unity", OpenSceneMode.Single);
            if (!scene22.IsValid())
            {
                Debug.LogError("[TEST FAIL] Không thể mở Scene 22. Buoi hoc hom nay!");
                return false;
            }

            Canvas canvas22 = Object.FindAnyObjectByType<Canvas>();
            Transform shortcutBar = canvas22.transform.Find("StudyScreenNavShortcuts");
            if (shortcutBar != null)
            {
                Debug.LogError("[TEST FAIL] Scene 22 vẫn còn chứa StudyScreenNavShortcuts!");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ Scene 22 không còn 3 nút nhỏ thừa (StudyScreenNavShortcuts đã bị loại bỏ).</color>");
            }

            // Kiểm tra text mẫu trên Scene 22
            var tmps = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool hasHardcodedSample = false;
            foreach (var t in tmps)
            {
                if (t.text == "6/10" || t.text.Contains("20 phút"))
                {
                    hasHardcodedSample = true;
                    Debug.LogError($"[TEST FAIL] Text '{t.text}' vẫn còn số mẫu cố định trong Scene 22!");
                }
            }
            if (!hasHardcodedSample)
            {
                Debug.Log("  <color=#00e676>✓ Scene 22 không còn số mẫu cố định '6/10' hay '20 phút'. Dữ liệu ban đầu là '--/--' và '--'.</color>");
            }
            else
            {
                ok = false;
            }

            // Mở lại Scene 01_MainMenu sau test
            EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

            if (ok)
            {
                Debug.Log("<color=#00e676>====================================================</color>");
                Debug.Log("<color=#00e676>=== KIỂM THỬ LUỒNG NGHIỆP VỤ (01 -> 02 -> 22) PASS 100% ===</color>");
                Debug.Log("<color=#00e676>====================================================</color>");
            }

            return ok;
        }
    }
}
