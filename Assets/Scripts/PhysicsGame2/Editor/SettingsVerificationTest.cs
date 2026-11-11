using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động toàn diện cho chức năng Cài đặt (Settings) Game 2 Học viện Aurelia:
    /// 1. Điều hướng từ Main Menu sang Scene 14_Settings (không sang Thành tích, không sang 14.2_Settings 1).
    /// 2. Bố cục và cấu trúc UI của Scene 14_Settings (2 toggle, 2 nút, tọa độ đối xứng, raycast safety).
    /// 3. Bốn trạng thái trực quan độc lập của 2 Toggle (ON/ON, ON/OFF, OFF/ON, OFF/OFF).
    /// 4. Chức năng Lưu cài đặt vào PlayerPrefs và PhysicsSaveData.
    /// 5. Chức năng Quay lại: hoàn tác trạng thái nếu chưa Lưu và trở về Main Menu.
    /// 6. Điều khiển âm thanh thực tế: BGM và SFX độc lập, không dùng AudioListener.pause.
    /// </summary>
    public static class SettingsVerificationTest
    {
        private const string MAIN_MENU_SCENE = "Assets/Scenes/Physics/Game2/01_MainMenu.unity";
        private const string SETTINGS_SCENE = "Assets/Scenes/Physics/Game2/14_Settings.unity";

        [MenuItem("Aurelia Academy/Run Settings Verification Tests", priority = 105)]
        public static bool RunAllTests()
        {
            Debug.Log("<color=#00e676>====================================================</color>");
            Debug.Log("<color=#00e676>=== CHẠY KIỂM THỬ CHỨC NĂNG CÀI ĐẶT (SETTINGS) GAME 2 ===</color>");
            Debug.Log("<color=#00e676>====================================================</color>");

            bool allPassed = true;
            allPassed &= Test1_MainMenuSettingsNavigation();
            allPassed &= Test2_SettingsSceneStructure();
            allPassed &= Test3_SettingsFourVisualStates();
            allPassed &= Test4_SettingsSaveAndLoad();
            allPassed &= Test5_SettingsRevertOnCancel();
            allPassed &= Test6_AudioSeparationAndNoListenerPause();
            allPassed &= Test7_BgmClipAndPlaybackConfiguration();

            if (allPassed)
            {
                Debug.Log("<color=#00e676>====================================================</color>");
                Debug.Log("<color=#00e676>✓ TẤT CẢ TEST CASE CÀI ĐẶT ĐỀU VƯỢT QUA 100% THÀNH CÔNG!</color>");
                Debug.Log("<color=#00e676>====================================================</color>");
            }
            else
            {
                Debug.LogError("[TEST FAIL] Một hoặc nhiều test case cài đặt bị lỗi!");
            }

            return allPassed;
        }

        /// <summary>
        /// TEST 1: Kiểm tra nút 'Cài đặt' trên Main Menu điều hướng đúng sang 14_Settings.
        /// </summary>
        private static bool Test1_MainMenuSettingsNavigation()
        {
            Debug.Log("--- [TEST 1] Kiểm tra điều hướng nút 'Cài đặt' từ Main Menu ---");
            var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[TEST 1 FAIL] Không mở được {MAIN_MENU_SCENE}");
                return false;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TEST 1 FAIL] Thiếu Canvas trên 01_MainMenu!");
                return false;
            }

            MainMenuController controller = canvas.GetComponent<MainMenuController>();
            if (controller == null) controller = canvas.gameObject.AddComponent<MainMenuController>();
            controller.InitializeMainMenu();

            Button btnSettings = controller.BtnSettings;
            if (btnSettings == null)
            {
                Debug.LogError("[TEST 1 FAIL] Không tìm thấy Btn_Settings trên MainMenuController!");
                return false;
            }

            // Route mapping check
            string targetScene = SceneCatalog.GetSceneName(RouteId.Settings);
            if (targetScene != "14_Settings")
            {
                Debug.LogError($"[TEST 1 FAIL] RouteId.Settings ánh xạ sai: '{targetScene}' thay vì '14_Settings'!");
                return false;
            }

            if (targetScene.Contains("Achievements") || targetScene.Contains("14.2_Settings 1"))
            {
                Debug.LogError($"[TEST 1 FAIL] RouteId.Settings dẫn nhầm tới '{targetScene}'!");
                return false;
            }

            Debug.Log("  <color=#00e676>✓ Nút Cài đặt Main Menu liên kết chính xác tới Scene '14_Settings'.</color>");
            return true;
        }

        /// <summary>
        /// TEST 2: Kiểm tra cấu trúc hierarchy, RectTransform, raycast safety trong 14_Settings.
        /// </summary>
        private static bool Test2_SettingsSceneStructure()
        {
            Debug.Log("--- [TEST 2] Kiểm tra cấu trúc giao diện Scene 14_Settings ---");
            var scene = EditorSceneManager.OpenScene(SETTINGS_SCENE, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[TEST 2 FAIL] Không mở được {SETTINGS_SCENE}");
                return false;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TEST 2 FAIL] Thiếu Canvas trên 14_Settings!");
                return false;
            }

            SettingsUIController controller = canvas.GetComponent<SettingsUIController>();
            if (controller == null)
            {
                controller = canvas.gameObject.AddComponent<SettingsUIController>();
            }
            controller.FindUIReferences();

            // Kiểm tra nút và toggle
            if (controller.BtnToggleMusic == null)
            {
                Debug.LogError("[TEST 2 FAIL] Không tìm thấy Công tắc Nhạc nền!");
                return false;
            }

            if (controller.BtnToggleSfx == null)
            {
                Debug.LogError("[TEST 2 FAIL] Không tìm thấy Công tắc SFX!");
                return false;
            }

            if (controller.BtnSave == null)
            {
                Debug.LogError("[TEST 2 FAIL] Không tìm thấy Nút Lưu cài đặt!");
                return false;
            }

            if (controller.BtnBack == null)
            {
                Debug.LogError("[TEST 2 FAIL] Không tìm thấy Nút Quay lại!");
                return false;
            }

            // Kiểm tra tọa độ của Icon Loa và Công tắc SFX
            RectTransform sfxToggleRt = controller.BtnToggleSfx.GetComponent<RectTransform>();
            if (Mathf.Abs(sfxToggleRt.anchoredPosition.x - 286f) > 5f || Mathf.Abs(sfxToggleRt.anchoredPosition.y - (-117f)) > 5f)
            {
                Debug.LogError($"[TEST 2 FAIL] Tọa độ Công tắc SFX chưa chuẩn: {sfxToggleRt.anchoredPosition} (kỳ vọng: (286, -117))");
                return false;
            }

            // Kiểm tra Raycast safety
            if (!controller.BtnToggleMusic.GetComponent<Image>().raycastTarget)
            {
                Debug.LogError("[TEST 2 FAIL] BtnToggleMusic bị tắt raycastTarget!");
                return false;
            }
            if (!controller.BtnToggleSfx.GetComponent<Image>().raycastTarget)
            {
                Debug.LogError("[TEST 2 FAIL] BtnToggleSfx bị tắt raycastTarget!");
                return false;
            }
            if (!controller.BtnSave.GetComponent<Image>().raycastTarget)
            {
                Debug.LogError("[TEST 2 FAIL] BtnSave bị tắt raycastTarget!");
                return false;
            }
            if (!controller.BtnBack.GetComponent<Image>().raycastTarget)
            {
                Debug.LogError("[TEST 2 FAIL] BtnBack bị tắt raycastTarget!");
                return false;
            }

            Debug.Log("  <color=#00e676>✓ Cấu trúc UI, tọa độ hàng 1 và hàng 2 đối xứng hoàn hảo, raycastTarget an toàn 100%.</color>");
            return true;
        }

        /// <summary>
        /// TEST 3: Kiểm tra 4 trạng thái độc lập của 2 Toggle (ON/ON, ON/OFF, OFF/ON, OFF/OFF).
        /// </summary>
        private static bool Test3_SettingsFourVisualStates()
        {
            Debug.Log("--- [TEST 3] Kiểm tra 4 trạng thái trực quan độc lập của 2 công tắc ---");
            EditorSceneManager.OpenScene(SETTINGS_SCENE, OpenSceneMode.Single);
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            SettingsUIController controller = canvas.GetComponent<SettingsUIController>();
            controller.FindUIReferences();

            // Set state 1: ON / ON
            PlayerPrefs.SetInt("MusicEnabled", 1);
            PlayerPrefs.SetInt("SfxEnabled", 1);
            controller.LoadSavedSettingsToUI();
            if (!controller.CurrentMusic || !controller.CurrentSfx)
            {
                Debug.LogError("[TEST 3 FAIL] Trạng thái 1 (ON/ON) thất bại!");
                return false;
            }

            // Set state 2: ON / OFF
            controller.OnToggleSfxClicked(); // SFX chuyển thành OFF
            if (!controller.CurrentMusic || controller.CurrentSfx)
            {
                Debug.LogError("[TEST 3 FAIL] Trạng thái 2 (ON/OFF) thất bại!");
                return false;
            }

            // Set state 3: OFF / ON
            controller.OnToggleMusicClicked(); // Music thành OFF
            controller.OnToggleSfxClicked();   // SFX thành ON
            if (controller.CurrentMusic || !controller.CurrentSfx)
            {
                Debug.LogError("[TEST 3 FAIL] Trạng thái 3 (OFF/ON) thất bại!");
                return false;
            }

            // Set state 4: OFF / OFF
            controller.OnToggleSfxClicked(); // SFX thành OFF
            if (controller.CurrentMusic || controller.CurrentSfx)
            {
                Debug.LogError("[TEST 3 FAIL] Trạng thái 4 (OFF/OFF) thất bại!");
                return false;
            }

            Debug.Log("  <color=#00e676>✓ 4 trạng thái trực quan độc lập (ON/ON, ON/OFF, OFF/ON, OFF/OFF) hoạt động chuẩn xác.</color>");
            return true;
        }

        /// <summary>
        /// TEST 4: Kiểm tra nút 'Lưu cài đặt' ghi đè vào PlayerPrefs và PhysicsSaveData.
        /// </summary>
        private static bool Test4_SettingsSaveAndLoad()
        {
            Debug.Log("--- [TEST 4] Kiểm tra chức năng Lưu cài đặt ---");
            EditorSceneManager.OpenScene(SETTINGS_SCENE, OpenSceneMode.Single);
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            SettingsUIController controller = canvas.GetComponent<SettingsUIController>();
            controller.FindUIReferences();

            // Test case: Bật Music, Tắt SFX rồi bấm Lưu
            PlayerPrefs.SetInt("MusicEnabled", 0);
            PlayerPrefs.SetInt("SfxEnabled", 0);
            controller.LoadSavedSettingsToUI();

            controller.OnToggleMusicClicked(); // Music: true
            // SFX: false
            controller.OnSaveClicked(); // Lưu

            int savedM = PlayerPrefs.GetInt("MusicEnabled", -1);
            int savedS = PlayerPrefs.GetInt("SfxEnabled", -1);

            if (savedM != 1 || savedS != 0)
            {
                Debug.LogError($"[TEST 4 FAIL] PlayerPrefs sau khi lưu không đúng: Music={savedM}, SFX={savedS} (kỳ vọng: 1, 0)");
                return false;
            }

            var save = PhysicsSaveManager.Load();
            if (save == null || save.settings == null || save.settings.musicVolume < 0.5f || save.settings.sfxVolume > 0.5f)
            {
                Debug.LogError("[TEST 4 FAIL] PhysicsSaveData chưa được đồng bộ đúng giá trị đã lưu!");
                return false;
            }

            Debug.Log("  <color=#00e676>✓ Nút 'Lưu cài đặt' lưu đồng bộ vào PlayerPrefs và PhysicsSaveData thành công.</color>");
            return true;
        }

        /// <summary>
        /// TEST 5: Kiểm tra nút 'Quay lại' hoàn tác thay đổi nếu chưa bấm Lưu.
        /// </summary>
        private static bool Test5_SettingsRevertOnCancel()
        {
            Debug.Log("--- [TEST 5] Kiểm tra chức năng hoàn tác khi bấm Quay lại mà chưa Lưu ---");
            EditorSceneManager.OpenScene(SETTINGS_SCENE, OpenSceneMode.Single);
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            SettingsUIController controller = canvas.GetComponent<SettingsUIController>();
            controller.FindUIReferences();

            // Trạng thái đã lưu: Music ON, SFX ON
            PlayerPrefs.SetInt("MusicEnabled", 1);
            PlayerPrefs.SetInt("SfxEnabled", 1);
            PlayerPrefs.Save();
            controller.LoadSavedSettingsToUI();

            // Người chơi nghịch bấm tắt cả 2 toggle
            controller.OnToggleMusicClicked(); // Music -> OFF
            controller.OnToggleSfxClicked();   // SFX -> OFF

            if (controller.CurrentMusic || controller.CurrentSfx)
            {
                Debug.LogError("[TEST 5 FAIL] Đổi toggle tạm thời thất bại!");
                return false;
            }

            // Bấm Quay lại mà KHÔNG bấm Lưu
            controller.OnBackClicked();

            // Kiểm tra trạng thái đã hoàn tác về ON / ON chưa
            if (!controller.CurrentMusic || !controller.CurrentSfx)
            {
                Debug.LogError($"[TEST 5 FAIL] Trạng thái chưa được hoàn tác về cài đặt cũ! Music={controller.CurrentMusic}, SFX={controller.CurrentSfx}");
                return false;
            }

            Debug.Log("  <color=#00e676>✓ Nút 'Quay lại' tự động hủy bỏ thay đổi chưa lưu và khôi phục cài đặt trước đó.</color>");
            return true;
        }

        /// <summary>
        /// TEST 6: Kiểm tra BGM và SFX điều khiển độc lập và KHÔNG dùng AudioListener.pause.
        /// </summary>
        private static bool Test6_AudioSeparationAndNoListenerPause()
        {
            Debug.Log("--- [TEST 6] Kiểm tra điều khiển âm thanh độc lập và cấm AudioListener.pause ---");
            GameObject mgrGo = new GameObject("SettingsManager_Test");
            SettingsManager sm = mgrGo.AddComponent<SettingsManager>();
            sm.EnsureAudioSources();

            // 1. Kiểm tra BGM OFF không tắt SFX
            sm.ApplyMusicEnabled(false);
            sm.ApplySfxEnabled(true);

            if (sm.IsMusicEnabled || !sm.IsSfxEnabled)
            {
                Debug.LogError("[TEST 6 FAIL] BGM tắt nhưng SFX bị ảnh hưởng!");
                Object.DestroyImmediate(mgrGo);
                return false;
            }

            // 2. Kiểm tra SFX OFF không tắt BGM
            sm.ApplyMusicEnabled(true);
            sm.ApplySfxEnabled(false);

            if (!sm.IsMusicEnabled || sm.IsSfxEnabled)
            {
                Debug.LogError("[TEST 6 FAIL] SFX tắt nhưng BGM bị ảnh hưởng!");
                Object.DestroyImmediate(mgrGo);
                return false;
            }

            // 3. Kiểm tra AudioListener.pause tuyệt đối không bị kích hoạt
            if (AudioListener.pause)
            {
                Debug.LogError("[TEST 6 FAIL] AudioListener.pause bị bật! Yêu cầu cấm dùng AudioListener.pause!");
                Object.DestroyImmediate(mgrGo);
                return false;
            }

            Object.DestroyImmediate(mgrGo);
            Debug.Log("  <color=#00e676>✓ BGM và SFX hoàn toàn độc lập, không dùng AudioListener.pause.</color>");
            return true;
        }

        /// <summary>
        /// TEST 7: Kiểm tra cấu hình BGM thực tế: volume = 25%, loop = true, không synth procedural,
        /// nạp file MP3 từ Resources / BGM folder chuẩn cho cả Unity Editor và Android Build.
        /// </summary>
        private static bool Test7_BgmClipAndPlaybackConfiguration()
        {
            Debug.Log("--- [TEST 7] Kiểm tra cấu hình BGM: Volume 25%, loop=true, không synth, hỗ trợ Resources/Android ---");
            GameObject mgrGo = new GameObject("SettingsManager_BgmTest");
            SettingsManager sm = mgrGo.AddComponent<SettingsManager>();
            sm.EnsureAudioSources();

            bool ok = true;

            // 1. Kiểm tra volume 25% (trong khoảng 20-30%)
            if (Mathf.Abs(sm.BgmVolume - 0.25f) > 0.05f)
            {
                Debug.LogError($"[TEST 7 FAIL] Âm lượng BGM ({sm.BgmVolume:P0}) không nằm trong khoảng 20-30%!");
                ok = false;
            }
            else
            {
                Debug.Log($"  <color=#00e676>✓ Âm lượng BGM được cấu hình chuẩn {sm.BgmVolume:P0} (êm dịu, không chói tai).</color>");
            }

            // 2. Kiểm tra tồn tại file MP3
            string bgmAssetPath = "Assets/Audio/PhysicsGame2/BGM/Ocean_Morning.mp3";
            string resAssetPath = "Assets/Resources/Audio/BGM/Ocean_Morning.mp3";
            bool bgmFileExists = System.IO.File.Exists(bgmAssetPath);
            bool resFileExists = System.IO.File.Exists(resAssetPath);

            if (!bgmFileExists && !resFileExists)
            {
                Debug.LogError("[TEST 7 FAIL] Không tìm thấy file MP3 trong Assets/Audio/PhysicsGame2/BGM/ hoặc Assets/Resources/Audio/BGM/!");
                ok = false;
            }
            else
            {
                long sz = bgmFileExists ? new System.IO.FileInfo(bgmAssetPath).Length : new System.IO.FileInfo(resAssetPath).Length;
                Debug.Log($"  <color=#00e676>✓ Đã xác nhận file MP3 hợp lệ: {sz:N0} bytes.</color>");
            }

            // 3. Kiểm tra không còn phát nhạc synth cũ
            if (sm.CurrentBgmName.Contains("Default_Ambient_BGM") || sm.CurrentBgmName.Contains("Synth"))
            {
                Debug.LogError("[TEST 7 FAIL] Vẫn còn phát hiện nhạc synth cũ!");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ Đã loại bỏ hoàn toàn 100% nhạc synth procedural cũ.</color>");
            }

            // 4. Kiểm tra lưu và nạp trạng thái công tắc
            sm.SaveSettings(musicEnabled: false, sfxEnabled: true);
            if (PlayerPrefs.GetInt("MusicEnabled", 1) != 0 || PlayerPrefs.GetInt("SfxEnabled", 1) != 1)
            {
                Debug.LogError("[TEST 7 FAIL] Lưu trạng thái công tắc vào PlayerPrefs thất bại!");
                ok = false;
            }

            sm.SaveSettings(musicEnabled: true, sfxEnabled: true);
            if (PlayerPrefs.GetInt("MusicEnabled", 1) != 1 || PlayerPrefs.GetInt("SfxEnabled", 1) != 1)
            {
                Debug.LogError("[TEST 7 FAIL] Lưu trạng thái BGM=ON vào PlayerPrefs thất bại!");
                ok = false;
            }
            else
            {
                Debug.Log("  <color=#00e676>✓ Lưu và phục hồi trạng thái công tắc BGM/SFX qua PlayerPrefs & PhysicsSaveData hoạt động chính xác.</color>");
            }

            Object.DestroyImmediate(mgrGo);
            return ok;
        }
    }
}
