using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Quản lý tiến trình hoàn thành năm bài kiểm tra chương và trao thưởng Tinh thể năng lượng.
    /// </summary>
    public static class PhysicsFinalProgression
    {
        public const string FINAL_ITEM_ID = "energy_crystal";

        // Required Explore lessons for each chapter
        public static readonly string[] REQUIRED_EXPLORE_CH2 = { "C2_L5", "C2_L6", "C2_L7", "C2_L8", "C2_L9", "C2_L10" };
        public static readonly string[] REQUIRED_EXPLORE_CH3 = { "C3_L11", "C3_L12", "C3_L13" };
        public static readonly string[] REQUIRED_EXPLORE_CH4 = { "C4_L14", "C4_L15" };
        public static readonly string[] REQUIRED_EXPLORE_CH5 = { "C5_L16", "C5_L17" };

        public static event Action OnFinalJourneyCompleted;

        public static bool IsChapterQuizPassed(int chapterNumber, PhysicsSaveData save)
        {
            if (save == null || chapterNumber < 1 || chapterNumber > 5 || save.lessonScores == null) return false;
            var requiredTests = PhysicsCurriculumData.GetLessons(chapterNumber)
                .FindAll(x => x != null && x.isTest && x.isRequiredForChapterCompletion);
            if (requiredTests.Count == 0) return false;
            foreach (var test in requiredTests)
            {
                string scoreId = $"{chapterNumber}_{test.lessonId}";
                var score = save.lessonScores.Find(x => x != null && x.lessonId == scoreId);
                if (score == null || !score.isCompleted) return false;
            }
            return true;
        }

        /// <summary>
        /// Cảnh Lễ hội Ánh Sao chỉ mở sau khi người chơi đạt bài kiểm tra bắt buộc của cả năm chương.
        /// </summary>
        public static bool IsFinalJourneyComplete(PhysicsSaveData save)
        {
            if (save == null) return false;

            for (int chapter = 1; chapter <= 5; chapter++)
                if (!IsChapterQuizPassed(chapter, save)) return false;
            return true;
        }

        /// <summary>
        /// Đánh giá và cấp phần thưởng Final Journey:
        /// COMPLETE JOURNEY -> UNLOCK CRYSTAL -> EVALUATE ACHIEVEMENTS -> SAVE
        /// Chống duplicate, hỗ trợ save cũ và load an toàn.
        /// </summary>
        public static bool EvaluateFinalProgression(PhysicsSaveData customSave = null)
        {
            var save = customSave ?? PhysicsSaveManager.Load();
            if (save == null) return false;

            if (IsFinalJourneyComplete(save))
            {
                // Nếu chưa mở khóa energy_crystal
                if (!save.IsItemUnlocked(FINAL_ITEM_ID))
                {
                    if (save.unlockedItemIds == null) save.unlockedItemIds = new List<string>();
                    save.unlockedItemIds.Add(FINAL_ITEM_ID);

                    // Đánh giá toàn bộ thành tích ngay sau khi có crystal (A3: collect_all_items, A7: aurelia_master)
                    AchievementManager.EvaluateAchievements(save);

                    // Lưu dữ liệu vào đĩa
                    PhysicsSaveManager.Save(save);
                    if (PhysicsGameSession.Instance != null)
                    {
                        PhysicsGameSession.Instance.ReloadSaveData();
                    }

                    OnFinalJourneyCompleted?.Invoke();
                    Debug.Log("[PhysicsFinalProgression] Chúc mừng! Đã mở khóa Tinh Thể Năng Lượng (Energy Crystal) thành công!");
                    return true;
                }
            }

            return false;
        }

        public static bool DidNavigateFromFinalModal { get; private set; }

        /// <summary>
        /// Tạo và hiển thị Modal vinh danh hoàn thành hành trình Aurelia Academy (One-time modal).
        /// </summary>
        public static GameObject ShowFinalCompletionModal(
            Transform parent,
            Action onDismiss = null,
            PhysicsSaveData customSave = null,
            bool isCustomSave = false)
        {
            DidNavigateFromFinalModal = false;
            if (parent == null) return null;

            // Chống duplicate nếu FinalCompletionModal đã tồn tại trên parent
            Transform existing = parent.Find("FinalCompletionModal");
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                existing.SetAsLastSibling();
                return existing.gameObject;
            }

            GameObject modalGo = new GameObject("FinalCompletionModal");
            modalGo.transform.SetParent(parent, false);
            modalGo.transform.SetAsLastSibling();

            RectTransform rt = modalGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Nền tối mờ bán trong suốt để vẫn thấy cảnh nền Gameplay
            Image bg = modalGo.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.05f, 0.12f, 0.55f);
            bg.raycastTarget = true;

            // Card vinh danh trung tâm
            GameObject cardGo = new GameObject("Card");
            cardGo.transform.SetParent(modalGo.transform, false);
            RectTransform cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(1080, 630);
            cardRt.anchoredPosition = Vector2.zero;

            Image cardImg = cardGo.AddComponent<Image>();
            cardImg.color = new Color(0.06f, 0.11f, 0.22f, 0.96f);
            cardImg.raycastTarget = false;

            Outline cardOutline = cardGo.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f); // Aurelia Gold
            cardOutline.effectDistance = new Vector2(2, -2);

            // Tiêu đề chính
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(cardGo.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 245);
            titleRt.sizeDelta = new Vector2(920, 50);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "CẢ NĂM KHU VỰC ĐÃ ĐƯỢC KHÔI PHỤC!";
            titleTmp.fontSize = 28;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            titleTmp.raycastTarget = false;

            // Nội dung kết thúc cốt truyện chính thức (Đêm hội Ánh Sao)
            GameObject subGo = new GameObject("Subtitle");
            subGo.transform.SetParent(cardGo.transform, false);
            RectTransform subRt = subGo.AddComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0, 118);
            subRt.sizeDelta = new Vector2(920, 185);
            var subTmp = subGo.AddComponent<TextMeshProUGUI>();
            subTmp.text =
                "Cây cầu đã mở. Những ngọn đèn đã sáng. Tiếng chuông vang lên khắp sân trường.\n\n" +
                "Các mô hình mà em từng giúp sửa cùng hoạt động trong Lễ hội Ánh Sao.\n\n" +
                "Giáo viên: “Những điều em học đã giúp mọi người làm nên đêm hội này.”\n\n" +
                "<color=#00E5FF><b>Bạn đã thắp sáng học viện Aurelia!</b></color>";
            subTmp.fontSize = 18;
            subTmp.enableAutoSizing = true;
            subTmp.fontSizeMin = 15;
            subTmp.fontSizeMax = 19;
            subTmp.fontStyle = FontStyles.Normal;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.92f, 0.96f, 1f, 0.98f);
            subTmp.textWrappingMode = TextWrappingModes.Normal;
            subTmp.raycastTarget = false;

            // Phần thưởng trung tâm
            GameObject rewGo = new GameObject("RewardBadge");
            rewGo.transform.SetParent(cardGo.transform, false);
            RectTransform rewRt = rewGo.AddComponent<RectTransform>();
            rewRt.anchoredPosition = new Vector2(0, -40);
            rewRt.sizeDelta = new Vector2(720, 106);
            var rewImg = rewGo.AddComponent<Image>();
            rewImg.color = new Color(0.10f, 0.18f, 0.35f, 0.95f);
            rewImg.raycastTarget = false;
            var rewOutline = rewGo.AddComponent<Outline>();
            rewOutline.effectColor = new Color(0.00f, 0.88f, 1.00f, 0.8f); // Cyan
            rewOutline.effectDistance = new Vector2(1, -1);

            GameObject rewTxtGo = new GameObject("RewardText");
            rewTxtGo.transform.SetParent(rewGo.transform, false);
            RectTransform rewTxtRt = rewTxtGo.AddComponent<RectTransform>();
            rewTxtRt.sizeDelta = rewRt.sizeDelta;
            var rewTmp = rewTxtGo.AddComponent<TextMeshProUGUI>();
            rewTmp.text = "<size=14>DANH HIỆU CAO QUÝ TOÀN BỘ HỌC VIỆN:</size>\n" +
                          "<size=24><b><color=#00E5FF>NGƯỜI THẮP SÁNG AURELIA</color></b></size>\n" +
                          "<size=14><color=#FFD54F>ENERGY CRYSTAL · HOÀN THÀNH 5 KHU VỰC</color></size>";
            rewTmp.alignment = TextAlignmentOptions.Center;
            rewTmp.raycastTarget = false;

            // Buttons: Xem kho vật phẩm, Xem thành tích, Trở về học viện
            CreateModalButton(cardGo.transform, "Btn_ViewInventory", "XEM KHO VẬT PHẨM", new Vector2(-230, -168), () =>
            {
                DidNavigateFromFinalModal = true;
                DismissModal(modalGo, onDismiss, customSave, isCustomSave);
                if (PhysicsGameSession.Instance != null)
                    PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.Inventory);
            });

            CreateModalButton(cardGo.transform, "Btn_ViewAchievements", "XEM THÀNH TÍCH", new Vector2(0, -168), () =>
            {
                DidNavigateFromFinalModal = true;
                DismissModal(modalGo, onDismiss, customSave, isCustomSave);
                if (PhysicsGameSession.Instance != null)
                    PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.Achievements);
            });

            CreateModalButton(cardGo.transform, "Btn_ReturnAcademy", "TRỞ VỀ HỌC VIỆN", new Vector2(230, -168), () =>
            {
                DidNavigateFromFinalModal = true;
                DismissModal(modalGo, onDismiss, customSave, isCustomSave);
                if (PhysicsGameSession.Instance != null)
                    PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.MainMenu);
            }, true);

            // Nút đóng ở dưới cùng
            CreateModalButton(cardGo.transform, "Btn_Close", "ĐÓNG", new Vector2(0, -238), () =>
            {
                DismissModal(modalGo, onDismiss, customSave, isCustomSave);
            });

            return modalGo;
        }

        private static void DismissModal(GameObject modalGo, Action onDismiss, PhysicsSaveData customSave = null, bool isCustomSave = false)
        {
            var save = customSave ?? (PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load());
            if (save != null)
            {
                save.hasSeenFinalJourneyCompletion = true;
                if (!isCustomSave)
                {
                    PhysicsSaveManager.Save(save);
                }
            }
            if (modalGo != null)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(modalGo);
                else
                    UnityEngine.Object.DestroyImmediate(modalGo);
            }
            onDismiss?.Invoke();
        }

        private static Button CreateModalButton(Transform parent, string name, string label, Vector2 pos, Action onClick, bool isGold = false)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);

            RectTransform rt = btnGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(190, 44);

            Image img = btnGo.AddComponent<Image>();
            img.color = isGold ? new Color(0.96f, 0.82f, 0.38f, 1f) : new Color(0.12f, 0.20f, 0.38f, 0.95f);
            img.raycastTarget = true;

            Button btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;

            var outline = btnGo.AddComponent<Outline>();
            outline.effectColor = isGold ? new Color(1f, 0.95f, 0.7f, 0.8f) : new Color(0.85f, 0.70f, 0.28f, 0.85f);
            outline.effectDistance = new Vector2(1, -1);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform tRt = textGo.AddComponent<RectTransform>();
            tRt.sizeDelta = rt.sizeDelta;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = isGold ? new Color(0.08f, 0.12f, 0.24f, 1f) : Color.white;
            tmp.raycastTarget = false;

            btn.onClick.AddListener(() => onClick?.Invoke());
            btnGo.AddComponent<UIButtonFeedback>();

            return btn;
        }
    }
}
