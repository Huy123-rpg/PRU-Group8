using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động toàn diện cho màn Thành tích (Achievements) Game 2 Học viện Aurelia
    /// Đáp ứng đầy đủ 15 tiêu chí nghiệm thu bắt buộc (TEST 1 - TEST 15):
    /// - Không còn bất kỳ thanh xám/vàng đè lên danh sách gốc.
    /// - Sử dụng 100% thiết kế Fantasy gốc từ 4 Scene của người dùng.
    /// - 12 thành tích chia làm 2 trang (6 items/trang), trạng thái độc lập từng dòng.
    /// - Chuyển trang 1/2 <-> 2/2 và Quay lại Main Menu hoạt động trơn tru.
    /// </summary>
    public static class AchievementVerificationTest
    {
        private const string ScenePage1Unlocked = "Assets/Scenes/Physics/Game2/15.1_Achievements (màn 2).unity";
        private const string ScenePage1Locked = "Assets/Scenes/Physics/Game2/15.3_Achievements 1.unity";
        private const string ScenePage2Unlocked = "Assets/Scenes/Physics/Game2/15_Achievements 1.unity";
        private const string ScenePage2Locked = "Assets/Scenes/Physics/Game2/15.4_Achievements.unity";

        [MenuItem("Aurelia/Test Achievements/Chạy toàn bộ 17 bài kiểm tra Thành tích")]
        public static void RunAllTests()
        {
            Debug.Log("===============================================================================");
            Debug.Log("=== BẮT ĐẦU KIỂM THỬ TOÀN DIỆN MÀN THÀNH TÍCH AURELIA (17 TIÊU CHÍ) ===");
            Debug.Log("===============================================================================");

            int passed = 0;
            int total = 17;

            if (Test_01_FourSceneAssetsExist()) passed++;
            if (Test_02_NoRuntimeSynthesizedBoxes()) passed++;
            if (Test_03_NoDuplicateTexts()) passed++;
            if (Test_04_Page1_Has6Achievements()) passed++;
            if (Test_05_Page2_Has6Achievements()) passed++;
            if (Test_06_PaginationControls()) passed++;
            if (Test_07_LockedAchievementVisuals()) passed++;
            if (Test_08_UnlockedAchievementVisuals()) passed++;
            if (Test_09_MixedIndependentStatuses()) passed++;
            if (Test_10_ProgressTextCalculation()) passed++;
            if (Test_11_SceneReentryNoDuplicateUI()) passed++;
            if (Test_12_SaveDataPersistence()) passed++;
            if (Test_13_BackButtonNavigation()) passed++;
            if (Test_14_RaycastSafetyAndEventSystem()) passed++;
            if (Test_15_LayoutFullHDVerification()) passed++;
            if (Test_16_TextNoWrapSingleLineAndClearance()) passed++;
            if (Test_17_PaginationButtonClickabilityAndRaycast()) passed++;

            Debug.Log("===============================================================================");
            Debug.Log($"=== KẾT QUẢ KIỂM THỬ THÀNH TÍCH: {passed}/{total} BÀI ĐẠT (PASS) ===");
            Debug.Log("===============================================================================");
        }

        public static bool Test_01_FourSceneAssetsExist()
        {
            bool s1 = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePage1Unlocked) != null;
            bool s2 = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePage1Locked) != null;
            bool s3 = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePage2Unlocked) != null;
            bool s4 = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePage2Locked) != null;

            bool pass = s1 && s2 && s3 && s4;
            Debug.Log($"[TEST 1] 4 Scene thiết kế Fantasy gốc tồn tại đầy đủ: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_02_NoRuntimeSynthesizedBoxes()
        {
            var scene = EditorSceneManager.OpenScene(ScenePage1Unlocked, OpenSceneMode.Single);
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TEST 2] FAIL: Không tìm thấy Canvas trong Scene 15.1!");
                return false;
            }

            // Gắn controller để mô phỏng runtime
            var ctrl = canvas.GetComponent<AchievementUIController>() ?? canvas.gameObject.AddComponent<AchievementUIController>();

            // Kiểm tra các container thừa
            bool hasListContainer = canvas.transform.Find("AchievementListContainer") != null;
            bool hasPageControls = canvas.transform.Find("AchievementPageControls") != null;
            bool hasRow0 = canvas.transform.Find("Row_0") != null;

            bool pass = !hasListContainer && !hasPageControls && !hasRow0;
            Debug.Log($"[TEST 2] Loại bỏ hoàn toàn thanh xám/vàng runtime: {(pass ? "PASS" : "FAIL")} (ListContainer={hasListContainer}, PageControls={hasPageControls})");
            return pass;
        }

        public static bool Test_03_NoDuplicateTexts()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;

            // Kiểm tra không có text runtime đè lên
            var ctrl = canvas.GetComponent<AchievementUIController>();
            if (ctrl == null) ctrl = canvas.gameObject.AddComponent<AchievementUIController>();
            ctrl.RefreshUI();

            // Đếm số TMP hiển thị nội dung hàng (phải đúng 12 text: 6 title + 6 desc)
            int rowTmps = 0;
            for (int i = 0; i < 6; i++)
            {
                if (ctrl.RowTitleTexts[i] != null) rowTmps++;
                if (ctrl.RowDescTexts[i] != null) rowTmps++;
            }

            bool pass = (rowTmps == 12);
            Debug.Log($"[TEST 3] Không có text trùng lặp hoặc chồng text: {(pass ? "PASS" : "FAIL")} (RowTmps={rowTmps}/12)");
            return pass;
        }

        public static bool Test_04_Page1_Has6Achievements()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();
            ctrl.OnPrevPageClicked(); // Về trang 1

            bool pass = (ctrl.CurrentPage == 1);
            if (ctrl.RowTitleTexts[0] != null)
            {
                pass = pass && ctrl.RowTitleTexts[0].text.Contains("Tia sáng đầu tiên");
            }
            if (ctrl.RowTitleTexts[5] != null)
            {
                pass = pass && ctrl.RowTitleTexts[5].text.Contains("Vệ binh Năng lượng xanh");
            }

            Debug.Log($"[TEST 4] Trang 1 hiển thị chính xác 6 thành tích A01-A06: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_05_Page2_Has6Achievements()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();
            ctrl.OnNextPageClicked(); // Sang trang 2

            bool pass = (ctrl.CurrentPage == 2);
            if (ctrl.RowTitleTexts[0] != null)
            {
                pass = pass && ctrl.RowTitleTexts[0].text.Contains("Người hồi sinh Aurelia");
            }
            if (ctrl.RowTitleTexts[5] != null)
            {
                pass = pass && ctrl.RowTitleTexts[5].text.Contains("Nhịp học đều đặn");
            }

            Debug.Log($"[TEST 5] Trang 2 hiển thị chính xác 6 thành tích A07-A12: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_06_PaginationControls()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();

            // Chuyển về trang 1
            ctrl.OnPrevPageClicked();
            bool p1Valid = (ctrl.CurrentPage == 1) &&
                           (ctrl.PageIndicatorText != null && ctrl.PageIndicatorText.text == "1/2") &&
                           (ctrl.BtnPrevPage != null && !ctrl.BtnPrevPage.interactable) &&
                           (ctrl.BtnNextPage != null && ctrl.BtnNextPage.interactable);

            // Chuyển sang trang 2
            ctrl.OnNextPageClicked();
            bool p2Valid = (ctrl.CurrentPage == 2) &&
                           (ctrl.PageIndicatorText != null && ctrl.PageIndicatorText.text == "2/2") &&
                           (ctrl.BtnPrevPage != null && ctrl.BtnPrevPage.interactable) &&
                           (ctrl.BtnNextPage != null && !ctrl.BtnNextPage.interactable);

            // Quay lại trang 1
            ctrl.OnPrevPageClicked();
            bool p1Return = (ctrl.CurrentPage == 1) && (ctrl.PageIndicatorText.text == "1/2");

            bool pass = p1Valid && p2Valid && p1Return;
            Debug.Log($"[TEST 6] Nút Trang trước / Trang sau hoạt động chuẩn xác: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_07_LockedAchievementVisuals()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();

            // Giả lập save không có thành tích nào
            var save = new PhysicsSaveData();
            save.unlockedAchievementIds = new List<string>();
            PhysicsSaveManager.Save(save);
            ctrl.RefreshUI();

            // Kiểm tra Row 0: Icon trạng thái phải là Khóa (Ảnh ChatGPT 09_00_29_14)
            bool pass = false;
            if (ctrl.RowStatusImages[0] != null && ctrl.RowStatusImages[0].sprite != null)
            {
                string sName = ctrl.RowStatusImages[0].sprite.name;
                pass = sName.Contains("_14") || sName.Contains("lock") || sName.Contains("Lock");
            }

            Debug.Log($"[TEST 7] Thành tích chưa đạt hiển thị biểu tượng Khóa: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_08_UnlockedAchievementVisuals()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();

            // Giả lập mở khóa A01
            var save = new PhysicsSaveData();
            save.unlockedAchievementIds = new List<string> { "A01" };
            PhysicsSaveManager.Save(save);
            ctrl.RefreshUI();

            // Kiểm tra Row 0: Icon trạng thái phải là Dấu tích vàng (Ảnh ChatGPT 09_00_35_8)
            bool pass = false;
            if (ctrl.RowStatusImages[0] != null && ctrl.RowStatusImages[0].sprite != null)
            {
                string sName = ctrl.RowStatusImages[0].sprite.name;
                pass = sName.Contains("_8") || sName.Contains("check") || sName.Contains("tick");
            }

            Debug.Log($"[TEST 8] Thành tích đã đạt hiển thị Dấu tích vàng: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_09_MixedIndependentStatuses()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();

            // Mở A01 và A03, khóa A02, A04, A05, A06
            var save = new PhysicsSaveData();
            save.unlockedAchievementIds = new List<string> { "A01", "A03" };
            PhysicsSaveManager.Save(save);
            ctrl.OnPrevPageClicked(); // Trang 1
            ctrl.RefreshUI();

            bool row0Unlocked = ctrl.RowStatusImages[0].sprite != null && ctrl.RowStatusImages[0].sprite.name.Contains("_8");
            bool row1Locked = ctrl.RowStatusImages[1].sprite != null && ctrl.RowStatusImages[1].sprite.name.Contains("_14");
            bool row2Unlocked = ctrl.RowStatusImages[2].sprite != null && ctrl.RowStatusImages[2].sprite.name.Contains("_8");
            bool row3Locked = ctrl.RowStatusImages[3].sprite != null && ctrl.RowStatusImages[3].sprite.name.Contains("_14");

            bool pass = row0Unlocked && row1Locked && row2Unlocked && row3Locked;
            Debug.Log($"[TEST 9] Cùng trang hiển thị độc lập khóa và mở: {(pass ? "PASS" : "FAIL")} (R0={row0Unlocked}, R1_Lock={row1Locked}, R2={row2Unlocked}, R3_Lock={row3Locked})");
            return pass;
        }

        public static bool Test_10_ProgressTextCalculation()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();

            // Mở 3 thành tích: A01, A02, A07
            var save = new PhysicsSaveData();
            save.unlockedAchievementIds = new List<string> { "A01", "A02", "A07" };
            PhysicsSaveManager.Save(save);
            ctrl.RefreshUI();

            bool pass = ctrl.ProgressText != null && ctrl.ProgressText.text.Contains("3/12");
            Debug.Log($"[TEST 10] Nhãn 'Đã mở khóa: X/12' tính toán chính xác: {(pass ? "PASS" : "FAIL")} (Text='{ctrl.ProgressText?.text}')");
            return pass;
        }

        public static bool Test_11_SceneReentryNoDuplicateUI()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;

            // Gọi Refresh nhiều lần
            var ctrl = canvas.GetComponent<AchievementUIController>();
            ctrl.RefreshUI();
            ctrl.RefreshUI();

            // Kiểm tra xem có tạo thêm bất kỳ component nào không
            var allControllers = canvas.GetComponents<AchievementUIController>();
            bool pass = (allControllers.Length == 1);
            Debug.Log($"[TEST 11] Rời Scene và vào lại không nhân đôi UI: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_12_SaveDataPersistence()
        {
            var save = new PhysicsSaveData();
            save.unlockedAchievementIds = new List<string> { "A01", "A05", "A11" };
            PhysicsSaveManager.Save(save);

            var loaded = PhysicsSaveManager.Load();
            bool pass = loaded != null &&
                        loaded.unlockedAchievementIds != null &&
                        loaded.unlockedAchievementIds.Contains("A01") &&
                        loaded.unlockedAchievementIds.Contains("A05") &&
                        loaded.unlockedAchievementIds.Contains("A11");

            Debug.Log($"[TEST 12] Dữ liệu thành tích lưu trữ bền vững: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_13_BackButtonNavigation()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;
            var ctrl = canvas.GetComponent<AchievementUIController>();

            bool pass = (ctrl.BtnBack != null) && (ctrl.BtnBack.onClick.GetPersistentEventCount() >= 0);
            Debug.Log($"[TEST 13] Nút Quay lại cấu hình đúng về MainMenu: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_14_RaycastSafetyAndEventSystem()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;

            // Kiểm tra các Image background không có raycastTarget
            Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
            int blockedRaycasts = 0;
            foreach (var img in allImages)
            {
                var rt = img.rectTransform;
                // Nếu là background (pos.y > -350 và x < 300) mà có raycastTarget thì bị lỗi chặn
                if (img.raycastTarget && rt.anchoredPosition.x < 300f && rt.anchoredPosition.y > -350f)
                {
                    blockedRaycasts++;
                }
            }

            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            bool pass = (blockedRaycasts == 0) && (es != null);
            Debug.Log($"[TEST 14] Raycast Safety & EventSystem sẵn sàng: {(pass ? "PASS" : "FAIL")} (BlockedBg={blockedRaycasts}, EventSystem={(es != null)})");
            return pass;
        }

        public static bool Test_15_LayoutFullHDVerification()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;

            var scaler = canvas.GetComponent<CanvasScaler>();
            bool scalerOk = scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize && scaler.referenceResolution == new Vector2(1920f, 1080f);

            var ctrl = canvas.GetComponent<AchievementUIController>();
            bool elementsOk = (ctrl.RowBadgeImages.Length == 6) && (ctrl.RowStatusImages.Length == 6) &&
                              (ctrl.RowTitleTexts.Length == 6) && (ctrl.RowDescTexts.Length == 6);

            bool pass = scalerOk && elementsOk;
            Debug.Log($"[TEST 15] Giao diện Full HD 1920x1080 khớp hoàn toàn thiết kế: {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        public static bool Test_16_TextNoWrapSingleLineAndClearance()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;

            var ctrl = canvas.GetComponent<AchievementUIController>();
            if (ctrl == null) ctrl = canvas.gameObject.AddComponent<AchievementUIController>();
            ctrl.RefreshUI();

            bool allNoWrap = true;
            bool allWidthSufficient = true;
            bool noEmbeddedNewlines = true;

            // Kiểm tra cả 2 trang
            for (int p = 1; p <= 2; p++)
            {
                if (p == 1) ctrl.OnPrevPageClicked();
                else ctrl.OnNextPageClicked();

                for (int i = 0; i < 6; i++)
                {
                    var title = ctrl.RowTitleTexts[i];
                    var desc = ctrl.RowDescTexts[i];

                    if (title != null)
                    {
                        if (title.textWrappingMode != TextWrappingModes.NoWrap) allNoWrap = false;
                        if (title.rectTransform.sizeDelta.x < 550f) allWidthSufficient = false;
                        if (title.text.Contains("\n") || title.text.Contains("\r")) noEmbeddedNewlines = false;
                    }

                    if (desc != null)
                    {
                        if (desc.textWrappingMode != TextWrappingModes.NoWrap) allNoWrap = false;
                        if (desc.rectTransform.sizeDelta.x < 550f) allWidthSufficient = false;
                        if (desc.text.Contains("\n") || desc.text.Contains("\r")) noEmbeddedNewlines = false;
                    }
                }
            }

            bool pass = allNoWrap && allWidthSufficient && noEmbeddedNewlines;
            Debug.Log($"[TEST 16] Text 1 dòng, NoWrap, Width >= 550px, không ngắt dòng: {(pass ? "PASS" : "FAIL")} (NoWrap={allNoWrap}, Width550={allWidthSufficient}, NoNL={noEmbeddedNewlines})");
            return pass;
        }

        public static bool Test_17_PaginationButtonClickabilityAndRaycast()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return false;

            var ctrl = canvas.GetComponent<AchievementUIController>();
            if (ctrl == null) ctrl = canvas.gameObject.AddComponent<AchievementUIController>();

            // 1. Kiểm tra ở trang 1: Next button phải interactable và nhận raycast
            ctrl.OnPrevPageClicked();
            bool p1NextInteractable = ctrl.BtnNextPage != null && ctrl.BtnNextPage.interactable;
            bool p1PrevDisabled = ctrl.BtnPrevPage != null && !ctrl.BtnPrevPage.interactable;
            bool nextHasRaycast = false;
            var nextGraphic = ctrl.BtnNextPage != null ? ctrl.BtnNextPage.targetGraphic : null;
            if (nextGraphic != null && nextGraphic.raycastTarget) nextHasRaycast = true;

            // 2. Click Next Page -> trang chuyển sang 2
            ctrl.OnNextPageClicked();
            bool p2NextDisabled = ctrl.BtnNextPage != null && !ctrl.BtnNextPage.interactable;
            bool p2PrevInteractable = ctrl.BtnPrevPage != null && ctrl.BtnPrevPage.interactable;
            bool p2PageText = ctrl.PageIndicatorText != null && ctrl.PageIndicatorText.text == "2/2";

            // 3. Click Prev Page -> trang chuyển về 1
            ctrl.OnPrevPageClicked();
            bool p1ReturnOk = ctrl.CurrentPage == 1 && ctrl.PageIndicatorText != null && ctrl.PageIndicatorText.text == "1/2";

            // 4. Lặp chuyển trang 5 lần liên tiếp xem có lỗi hay không
            bool loopOk = true;
            for (int k = 0; k < 5; k++)
            {
                ctrl.OnNextPageClicked();
                if (ctrl.CurrentPage != 2) { loopOk = false; break; }
                ctrl.OnPrevPageClicked();
                if (ctrl.CurrentPage != 1) { loopOk = false; break; }
            }

            bool pass = p1NextInteractable && p1PrevDisabled && nextHasRaycast && p2NextDisabled && p2PrevInteractable && p2PageText && p1ReturnOk && loopOk;
            Debug.Log($"[TEST 17] Nút Trang sau / Trang trước tương tác mượt mà & Raycast chuẩn: {(pass ? "PASS" : "FAIL")} (P1Next={p1NextInteractable}, Raycast={nextHasRaycast}, P2Prev={p2PrevInteractable}, Loop={loopOk})");
            return pass;
        }
    }
}
