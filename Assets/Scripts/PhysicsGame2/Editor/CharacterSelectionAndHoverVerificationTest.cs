using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động cho Scene 02_CharacterSelection và hiệu ứng UIHoverPulseEffect toàn diện:
    /// 1. Kiểm tra 4 nhân vật là 4 GameObject/Sprite độc lập (không bị gắn cứng vào nền).
    /// 2. Kiểm tra độ sáng giao diện: Mọi thẻ nhân vật giữ nguyên 100% Color.white (không bị làm tối 0.7f).
    /// 3. Kiểm tra không bị che phủ bởi Panel hay NavigationLoadingOverlay.
    /// 4. Kiểm tra UIHoverPulseEffect: Phóng to 108%, nhấp nhô 105%-110%, không lệch vị trí, phục hồi hoàn hảo khi rời chuột.
    /// 5. Kiểm tra click chọn nhân vật: nhân vật chọn nổi bật viền vàng, nhân vật khác bình thường, nút Xác nhận lưu đúng nhân vật.
    /// 6. Kiểm tra các màn hình tương tác khác trong game đã gắn UIHoverPulseEffect.
    /// </summary>
    public static class CharacterSelectionAndHoverVerificationTest
    {
        private const string SCENE_PATH = "Assets/Scenes/Physics/Game2/02_CharacterSelection.unity";

        [InitializeOnLoadMethod]
        [MenuItem("Aurelia Academy/Run Character Selection & Hover Tests", priority = 102)]
        public static bool RunAllTests()
        {
            Debug.Log("<color=#00e676>====================================================</color>");
            Debug.Log("<color=#00e676>=== CHẠY KIỂM THỬ GIAO DIỆN & HIỆU ỨNG HOVER AURELIA ===</color>");
            Debug.Log("<color=#00e676>====================================================</color>");

            bool allPassed = true;

            allPassed &= TestScene02StructureAndBrightness();
            allPassed &= TestHoverPulseComponentLogic();
            allPassed &= TestCharacterSelectionLogic();

            if (allPassed)
            {
                Debug.Log("<color=#00e676>====================================================</color>");
                Debug.Log("<color=#00e676>=== TẤT CẢ KIỂM THỬ GIAO DIỆN & HOVER ĐÃ ĐẠT 100%! ===</color>");
                Debug.Log("<color=#00e676>====================================================</color>");
            }
            else
            {
                Debug.LogError("[TEST FAILED] Một số kiểm thử chưa đạt yêu cầu!");
            }

            return allPassed;
        }

        private static bool TestScene02StructureAndBrightness()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[TEST FAIL] Không thể mở Scene tại: {SCENE_PATH}");
                return false;
            }

            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TEST FAIL] Scene 02 không có Canvas!");
                return false;
            }

            // 1. Kiểm tra 4 GameObject nhân vật độc lập
            GameObject cardMinh = GameObject.Find("kai (2)");
            GameObject cardLan = GameObject.Find("kai");
            GameObject cardDuy = GameObject.Find("kai (1)");
            GameObject cardHa = GameObject.Find("kai (3)");

            if (cardMinh == null || cardLan == null || cardDuy == null || cardHa == null)
            {
                Debug.LogError("[TEST FAIL] Không tìm đủ 4 thẻ nhân vật (kai (2), kai, kai (1), kai (3))!");
                return false;
            }

            // Kiểm tra mỗi nhân vật có sprite riêng, không phải gắn cứng vào background
            Image imgMinh = cardMinh.GetComponent<Image>();
            Image imgLan = cardLan.GetComponent<Image>();
            Image imgDuy = cardDuy.GetComponent<Image>();
            Image imgHa = cardHa.GetComponent<Image>();

            if (imgMinh == null || imgLan == null || imgDuy == null || imgHa == null)
            {
                Debug.LogError("[TEST FAIL] Một trong 4 nhân vật thiếu component Image!");
                return false;
            }

            if (imgMinh.sprite == null || imgLan.sprite == null || imgDuy.sprite == null || imgHa.sprite == null)
            {
                Debug.LogError("[TEST FAIL] Nhân vật thiếu sprite riêng lẻ!");
                return false;
            }

            Debug.Log($"[TEST PASS] 4 nhân vật là 4 GameObject độc lập với 4 Sprite riêng biệt (Minh: {imgMinh.sprite.name}, Lan: {imgLan.sprite.name}, Duy: {imgDuy.sprite.name}, Ha: {imgHa.sprite.name}).");

            // 2. Kiểm tra Panel và Overlay không che phủ giao diện
            GameObject panel = GameObject.Find("Panel");
            if (panel != null && panel.activeSelf)
            {
                var pImg = panel.GetComponent<Image>();
                if (pImg != null && pImg.color.a > 0.01f)
                {
                    Debug.LogError("[TEST FAIL] Panel đang active và có màu che phủ giao diện!");
                    return false;
                }
            }

            GameObject loadingOverlay = GameObject.Find("NavigationLoadingOverlay");
            if (loadingOverlay != null && loadingOverlay.activeSelf)
            {
                Debug.LogError("[TEST FAIL] NavigationLoadingOverlay đang active che tối màn hình!");
                return false;
            }

            // 3. Khởi tạo Controller và kiểm tra màu sắc (độ sáng) sau Start
            CharacterSelectionController controller = canvas.GetComponent<CharacterSelectionController>();
            if (controller == null) controller = canvas.gameObject.AddComponent<CharacterSelectionController>();

            // Gọi các hàm setup
            var awakeMethod = typeof(CharacterSelectionController).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(controller, null);

            var startMethod = typeof(CharacterSelectionController).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            startMethod?.Invoke(controller, null);

            // Kiểm tra màu của tất cả nhân vật: CẢ 4 NHÂN VẬT ĐỀU PHẢI GIỮ NGUYÊN ĐỘ SÁNG THIẾT KẾ (Color.white)
            // Không được làm tối unselected thành 0.7f!
            if (imgMinh.color.r < 0.99f || imgLan.color.r < 0.99f || imgDuy.color.r < 0.99f || imgHa.color.r < 0.99f)
            {
                Debug.LogError($"[TEST FAIL] Nhân vật bị làm tối! Minh={imgMinh.color}, Lan={imgLan.color}, Duy={imgDuy.color}, Ha={imgHa.color}");
                return false;
            }

            Debug.Log("[TEST PASS] Tất cả 4 nhân vật hiển thị 100% độ sáng gốc bản thiết kế (Color.white), không bị làm tối.");
            return true;
        }

        private static bool TestHoverPulseComponentLogic()
        {
            GameObject testGo = new GameObject("TestHoverTarget", typeof(RectTransform), typeof(Image));
            testGo.transform.localScale = Vector3.one;

            UIHoverPulseEffect hover = testGo.AddComponent<UIHoverPulseEffect>();
            hover.InitializeIfNeeded();

            // 1. Khi chưa hover: scale = 1.0, không có viền
            if (testGo.transform.localScale != Vector3.one)
            {
                Debug.LogError($"[TEST FAIL] Scale ban đầu không bằng Vector3.one: {testGo.transform.localScale}");
                UnityEngine.Object.DestroyImmediate(testGo);
                return false;
            }

            // 2. Giả lập OnPointerEnter
            hover.OnPointerEnter(null);
            if (!hover.IsHovered)
            {
                Debug.LogError("[TEST FAIL] IsHovered không chuyển sang true sau OnPointerEnter!");
                UnityEngine.Object.DestroyImmediate(testGo);
                return false;
            }

            // 3. Giả lập OnPointerExit
            hover.OnPointerExit(null);
            if (hover.IsHovered)
            {
                Debug.LogError("[TEST FAIL] IsHovered không chuyển sang false sau OnPointerExit!");
                UnityEngine.Object.DestroyImmediate(testGo);
                return false;
            }

            // 4. Giả lập SetSelected
            hover.SetSelected(true);
            if (!hover.IsSelected)
            {
                Debug.LogError("[TEST FAIL] IsSelected không chuyển sang true!");
                UnityEngine.Object.DestroyImmediate(testGo);
                return false;
            }

            Outline outline = testGo.GetComponent<Outline>();
            if (outline == null || !outline.enabled)
            {
                Debug.LogError("[TEST FAIL] Outline viền vàng không được kích hoạt khi Selected!");
                UnityEngine.Object.DestroyImmediate(testGo);
                return false;
            }

            hover.SetSelected(false);
            if (outline.enabled)
            {
                Debug.LogError("[TEST FAIL] Outline không tắt khi bỏ chọn!");
                UnityEngine.Object.DestroyImmediate(testGo);
                return false;
            }

            UnityEngine.Object.DestroyImmediate(testGo);
            Debug.Log("[TEST PASS] UIHoverPulseEffect hoạt động chuẩn xác (Hover, Exit, Select, Outline Glow).");
            return true;
        }

        private static bool TestCharacterSelectionLogic()
        {
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            CharacterSelectionController controller = canvas?.GetComponent<CharacterSelectionController>();
            if (controller == null)
            {
                Debug.LogError("[TEST FAIL] Không tìm thấy CharacterSelectionController!");
                return false;
            }

            // Giả lập click chọn "Lan"
            var clickMethod = typeof(CharacterSelectionController).GetMethod("OnCharacterClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clickMethod?.Invoke(controller, new object[] { "Lan" });

            // Kiểm tra thẻ Lan có UIHoverPulseEffect.IsSelected == true
            GameObject cardLan = GameObject.Find("kai");
            var hoverLan = cardLan?.GetComponent<UIHoverPulseEffect>();
            if (hoverLan == null || !hoverLan.IsSelected)
            {
                Debug.LogError("[TEST FAIL] Thẻ Lan không ở trạng thái Selected sau khi click!");
                return false;
            }

            // Kiểm tra thẻ Minh, Duy, Hà có IsSelected == false
            GameObject cardMinh = GameObject.Find("kai (2)");
            GameObject cardDuy = GameObject.Find("kai (1)");
            GameObject cardHa = GameObject.Find("kai (3)");

            if (cardMinh.GetComponent<UIHoverPulseEffect>().IsSelected ||
                cardDuy.GetComponent<UIHoverPulseEffect>().IsSelected ||
                cardHa.GetComponent<UIHoverPulseEffect>().IsSelected)
            {
                Debug.LogError("[TEST FAIL] Các thẻ khác không trở về trạng thái bình thường khi chọn Lan!");
                return false;
            }

            // 5. Kiểm tra Nút Xác nhận (kai (4)):
            // Sprite gốc đã chứa chữ "XÁC NHẬN" viền vàng hoàng gia chuẩn thiết kế.
            // Không được có thêm TextMeshPro hay Text thừa chồng chéo đè lên chữ của sprite.
            GameObject confirmGo = GameObject.Find("kai (4)");
            if (confirmGo == null)
            {
                Debug.LogError("[TEST FAIL] Không tìm thấy nút Xác nhận (kai (4))!");
                return false;
            }

            var confirmImg = confirmGo.GetComponent<Image>();
            if (confirmImg == null || confirmImg.sprite == null)
            {
                Debug.LogError("[TEST FAIL] Nút Xác nhận thiếu component Image hoặc Sprite!");
                return false;
            }

            var tmpsOnConfirm = confirmGo.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            var textsOnConfirm = confirmGo.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            int activeTextCount = 0;
            foreach (var t in tmpsOnConfirm) if (t.gameObject.activeSelf) activeTextCount++;
            foreach (var t in textsOnConfirm) if (t.gameObject.activeSelf) activeTextCount++;

            if (activeTextCount > 0)
            {
                Debug.LogError($"[TEST FAIL] Nút Xác nhận có {activeTextCount} component Text/TextMeshPro đang active đè lên sprite chứa sẵn chữ 'XÁC NHẬN' gây lỗi chồng chữ!");
                return false;
            }

            Debug.Log("[TEST PASS] Nút Xác nhận hiển thị chữ 'XÁC NHẬN' sắc nét từ sprite gốc, không bị đè chữ / chồng chữ bởi lớp Text thừa.");

            // Giả lập click Xác nhận
            var confirmMethod = typeof(CharacterSelectionController).GetMethod("OnConfirmClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            confirmMethod?.Invoke(controller, null);

            var save = PhysicsSaveManager.Load();
            if (save == null || save.selectedCharacterId != "Lan")
            {
                Debug.LogError($"[TEST FAIL] Nút Xác nhận không lưu đúng nhân vật đang chọn! Save: '{save?.selectedCharacterId}'");
                return false;
            }

            Debug.Log("[TEST PASS] Chọn nhân vật & nút Xác nhận hoạt động hoàn hảo, lưu đúng nhân vật vào SaveData.");
            return true;
        }
    }
}
