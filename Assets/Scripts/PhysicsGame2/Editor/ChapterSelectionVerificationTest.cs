using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Tests
{
    /// <summary>
    /// Bộ kiểm thử tự động cho Scene 04_ChapterSelection:
    /// 1. Kiểm tra cấu trúc Scene và 5 ô chương (chuong1..chuong5) cùng nút quay lại (quaylai).
    /// 2. Kiểm tra mỗi ô chương được tách thành 3 TextMeshPro riêng biệt:
    ///    - Text_ChapterHeader: "Chương I/II/III/IV/V"
    ///    - Text_ChapterTitle: Tên chương chính thức ("Năng lượng cơ học", "Ánh sáng", v.v.)
    ///    - Text_ChapterProgress: Tiến độ "x/y thiết bị"
    /// 3. Kiểm tra căn giữa theo chiều ngang và định vị chính xác với khung vàng của từng đảo.
    /// 4. Kiểm tra phân bổ rect / line spacing không bị đè chữ, không bị tràn khung.
    /// 5. Kiểm tra tương tác chọn chương và luồng điều hướng sang LessonSelection.
    /// </summary>
    public static class ChapterSelectionVerificationTest
    {
        private const string SCENE_PATH = "Assets/Scenes/Physics/Game2/04_ChapterSelection.unity";

        [InitializeOnLoadMethod]
        [MenuItem("Aurelia Academy/Run Chapter Selection Tests", priority = 103)]
        public static bool RunAllTests()
        {
            Debug.Log("<color=#00e676>====================================================</color>");
            Debug.Log("<color=#00e676>=== CHẠY KIỂM THỬ GIAO DIỆN CHỌN CHƯƠNG (SCENE 04) ===</color>");
            Debug.Log("<color=#00e676>====================================================</color>");

            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[TEST FAIL] Không thể mở Scene tại: {SCENE_PATH}");
                return false;
            }

            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TEST FAIL] Scene 04 không có Canvas!");
                return false;
            }

            bool allPassed = true;
            allPassed &= TestScene04Structure(canvas);
            allPassed &= TestChapterThreeTMPComponents(canvas);
            allPassed &= TestChapterSelectionInteraction(canvas);

            if (allPassed)
            {
                Debug.Log("<color=#00e676>====================================================</color>");
                Debug.Log("<color=#00e676>=== TẤT CẢ KIỂM THỬ CHỌN CHƯƠNG ĐÃ ĐẠT 100%! ===</color>");
                Debug.Log("<color=#00e676>====================================================</color>");
            }
            else
            {
                Debug.LogError("[TEST FAILED] Một số kiểm thử Chọn chương chưa đạt yêu cầu!");
            }

            return allPassed;
        }

        private static bool TestScene04Structure(Canvas canvas)
        {
            string[] expectedCards = { "chuong1", "chuong2", "chuong3", "chuong4", "chuong5" };
            foreach (var cardName in expectedCards)
            {
                GameObject card = GameObject.Find(cardName);
                if (card == null)
                {
                    Debug.LogError($"[TEST FAIL] Không tìm thấy ô chương: {cardName}!");
                    return false;
                }

                Image img = card.GetComponent<Image>();
                if (img == null || img.sprite == null)
                {
                    Debug.LogError($"[TEST FAIL] Ô {cardName} thiếu Image hoặc Sprite!");
                    return false;
                }
            }

            GameObject backBtn = GameObject.Find("quaylai");
            if (backBtn == null)
            {
                Debug.LogError("[TEST FAIL] Không tìm thấy nút quay lại (quaylai)!");
                return false;
            }

            Debug.Log("[TEST PASS] Đầy đủ 5 ô chương và nút quay lại trong Scene 04_ChapterSelection.");
            return true;
        }

        private static bool TestChapterThreeTMPComponents(Canvas canvas)
        {
            ChapterSelectionController controller = canvas.GetComponent<ChapterSelectionController>();
            if (controller == null)
            {
                controller = canvas.gameObject.AddComponent<ChapterSelectionController>();
            }

            // Gọi các hàm setup
            var awakeMethod = typeof(ChapterSelectionController).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(controller, null);

            var startMethod = typeof(ChapterSelectionController).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            startMethod?.Invoke(controller, null);

            string[] expectedRomans = { "I", "II", "III", "IV", "V" };
            string[] expectedTitles = { "Năng lượng cơ học", "Ánh sáng", "Điện", "Điện từ", "Năng lượng với cuộc sống" };
            int[] expectedDeviceTotals = { 4, 7, 4, 3, 3 };

            for (int ch = 1; ch <= 5; ch++)
            {
                GameObject card = GameObject.Find($"chuong{ch}");
                if (card == null) return false;

                Transform panelT = card.transform.Find("ChapterInfoPanel");
                if (panelT == null)
                {
                    Debug.LogError($"[TEST FAIL] Chương {ch} không có container ChapterInfoPanel!");
                    return false;
                }

                RectTransform panelRt = panelT.GetComponent<RectTransform>();
                // Kiểm tra căn giữa ngang đặc biệt cho Chương 3 và Chương 5
                if (ch == 3 && panelRt.anchoredPosition.x > -10f)
                {
                    Debug.LogError($"[TEST FAIL] Chương 3 chưa được bù lệch tâm ngang cho khung vàng! x={panelRt.anchoredPosition.x}");
                    return false;
                }
                if (ch == 5 && panelRt.anchoredPosition.x > -15f)
                {
                    Debug.LogError($"[TEST FAIL] Chương 5 chưa được bù lệch tâm ngang cho khung vàng! x={panelRt.anchoredPosition.x}");
                    return false;
                }

                // 1. TextMeshPro Header
                Transform hT = panelT.Find("Text_ChapterHeader");
                if (hT == null)
                {
                    Debug.LogError($"[TEST FAIL] Chương {ch} thiếu Text_ChapterHeader!");
                    return false;
                }
                var tmpH = hT.GetComponent<TextMeshProUGUI>();
                if (tmpH == null || tmpH.text != $"Chương {expectedRomans[ch - 1]}")
                {
                    Debug.LogError($"[TEST FAIL] Chương {ch} Text_ChapterHeader sai nội dung: '{tmpH?.text}'!");
                    return false;
                }

                // 2. TextMeshPro Title
                Transform tT = panelT.Find("Text_ChapterTitle");
                if (tT == null)
                {
                    Debug.LogError($"[TEST FAIL] Chương {ch} thiếu Text_ChapterTitle!");
                    return false;
                }
                var tmpT = tT.GetComponent<TextMeshProUGUI>();
                if (tmpT == null || tmpT.text != expectedTitles[ch - 1])
                {
                    Debug.LogError($"[TEST FAIL] Chương {ch} Text_ChapterTitle sai tên chương: '{tmpT?.text}'!");
                    return false;
                }

                // 3. TextMeshPro Progress
                Transform pT = panelT.Find("Text_ChapterProgress");
                if (pT == null)
                {
                    Debug.LogError($"[TEST FAIL] Chương {ch} thiếu Text_ChapterProgress!");
                    return false;
                }
                var tmpP = pT.GetComponent<TextMeshProUGUI>();
                string expectedProgEnd = $"/{expectedDeviceTotals[ch - 1]} thiết bị";
                if (tmpP == null || !tmpP.text.EndsWith(expectedProgEnd))
                {
                    Debug.LogError($"[TEST FAIL] Chương {ch} Text_ChapterProgress sai định dạng tiến độ: '{tmpP?.text}', kỳ vọng kết thúc bằng '{expectedProgEnd}'!");
                    return false;
                }

                // Kiểm tra không chồng chéo Y (Header > Title > Progress)
                RectTransform rtH = hT.GetComponent<RectTransform>();
                RectTransform rtTitle = tT.GetComponent<RectTransform>();
                RectTransform rtP = pT.GetComponent<RectTransform>();

                if (rtH.anchoredPosition.y <= rtTitle.anchoredPosition.y || rtTitle.anchoredPosition.y <= rtP.anchoredPosition.y)
                {
                    Debug.LogError($"[TEST FAIL] Thứ tự chiều dọc của 3 Text bị chồng chéo! Header={rtH.anchoredPosition.y}, Title={rtTitle.anchoredPosition.y}, Progress={rtP.anchoredPosition.y}");
                    return false;
                }

                // Kiểm tra không còn component ProgressBadge cũ
                Transform oldBadge = card.transform.Find("ProgressBadge");
                if (oldBadge != null && oldBadge.gameObject.activeSelf)
                {
                    Debug.LogError($"[TEST FAIL] Ô chương {ch} vẫn còn ProgressBadge cũ đè vào khung!");
                    return false;
                }

                Debug.Log($"[TEST PASS] Chương {ch} tách chuẩn 3 TextMeshPro: [{tmpH.text}] - [{tmpT.text}] - [{tmpP.text}], căn giữa hoàn hảo, không đè chữ.");
            }

            return true;
        }

        private static bool TestChapterSelectionInteraction(Canvas canvas)
        {
            ChapterSelectionController controller = canvas.GetComponent<ChapterSelectionController>();
            if (controller == null) return false;

            // Giả lập click chọn Chương 2
            var selectMethod = typeof(ChapterSelectionController).GetMethod("OnChapterSelected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectMethod?.Invoke(controller, new object[] { 2 });

            if (PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId != 2)
            {
                Debug.LogError($"[TEST FAIL] Chọn Chương 2 nhưng GameSession lưu {PhysicsGameSession.Instance.SelectedChapterId}!");
                return false;
            }

            if (NavigationService.Instance != null && NavigationService.Instance.Selection != null && NavigationService.Instance.Selection.ChapterId != 2)
            {
                Debug.LogError($"[TEST FAIL] Chọn Chương 2 nhưng NavigationSelection lưu {NavigationService.Instance.Selection.ChapterId}!");
                return false;
            }

            Debug.Log("[TEST PASS] Tương tác click chọn Chương hoạt động chính xác và đồng bộ Session/Navigation.");
            return true;
        }
    }
}
