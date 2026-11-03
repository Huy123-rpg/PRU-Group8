using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Quản lý đánh giá và mở khóa 7 thành tích chính thức của Aurelia Academy.
    /// Tự động đánh giá theo tiến trình thực tế từ PhysicsSaveData.
    /// </summary>
    public static class AchievementManager
    {
        public static event Action<string> OnAchievementUnlocked;

        public const string ACH_COMPLETE_CH1 = "complete_chapter_1";
        public const string ACH_QUIZ_50 = "quiz_50";
        public const string ACH_COLLECT_ALL_ITEMS = "collect_all_items";
        public const string ACH_COMPLETE_ALL_CHAPTERS = "complete_all_chapters";
        public const string ACH_DISCOVER_SECRET = "discover_secret";
        public const string ACH_CORRECT_100 = "correct_100";
        public const string ACH_AURELIA_MASTER = "aurelia_master";

        /// <summary>
        /// Kiểm tra xem một thành tích đã mở khóa chưa.
        /// </summary>
        public static bool IsUnlocked(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId)) return false;
            var save = PhysicsSaveManager.Load();
            return save != null && save.unlockedAchievementIds != null && save.unlockedAchievementIds.Contains(achievementId);
        }

        private static void SafeLog(string msg)
        {
            try { Debug.Log(msg); } catch { }
        }

        private static void SafeLogWarning(string msg)
        {
            try { Debug.LogWarning(msg); } catch { }
        }

        /// <summary>
        /// Mở khóa thủ công một thành tích (nếu chưa mở).
        /// </summary>
        public static bool Unlock(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId)) return false;

            var ach = PhysicsAchievementData.Get(achievementId);
            if (ach == null)
            {
                SafeLogWarning($"[AchievementManager] Không tìm thấy thành tích có ID '{achievementId}'!");
                return false;
            }

            var save = PhysicsSaveManager.Load();
            if (save == null) return false;

            if (save.unlockedAchievementIds == null)
            {
                save.unlockedAchievementIds = new List<string>();
            }

            if (!save.unlockedAchievementIds.Contains(achievementId))
            {
                save.unlockedAchievementIds.Add(achievementId);
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }

                SafeLog($"[AchievementManager] Chúc mừng! Mở khóa thành tích: '{ach.title}' ({achievementId}).");
                OnAchievementUnlocked?.Invoke(achievementId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Đánh giá lại toàn bộ 7 thành tích dựa trên dữ liệu tiến trình thực tế của SaveData.
        /// Chỉ lưu dữ liệu khi có thành tích mới được mở khóa (không lưu thừa).
        /// </summary>
        public static int EvaluateAchievements(PhysicsSaveData targetSave = null)
        {
            bool isExplicitSave = targetSave != null;
            var save = targetSave ?? PhysicsSaveManager.Load();
            if (save == null) return 0;

            if (save.unlockedAchievementIds == null)
            {
                save.unlockedAchievementIds = new List<string>();
            }

            bool hasChanges = false;

            // -------------------------------------------------------------
            // A1: Hoàn thành Chương I
            // Điều kiện: Toàn bộ bài học của Chương I đã hoàn thành
            // -------------------------------------------------------------
            if (!save.unlockedAchievementIds.Contains(ACH_COMPLETE_CH1))
            {
                var ch1Lessons = PhysicsCurriculumData.GetLessons(1);
                bool ch1AllCompleted = ch1Lessons != null && ch1Lessons.Count > 0;
                if (ch1AllCompleted)
                {
                    foreach (var l in ch1Lessons)
                    {
                        if (save.completedLessons == null || !save.completedLessons.Contains(l.lessonId))
                        {
                            ch1AllCompleted = false;
                            break;
                        }
                    }
                }

                if (ch1AllCompleted)
                {
                    save.unlockedAchievementIds.Add(ACH_COMPLETE_CH1);
                    hasChanges = true;
                    OnAchievementUnlocked?.Invoke(ACH_COMPLETE_CH1);
                    SafeLog("[AchievementManager] Unlock A1: Hoàn thành Chương I.");
                }
            }

            // -------------------------------------------------------------
            // A2: Giải 50 câu trắc nghiệm
            // Điều kiện: totalQuestionsAnswered >= 50
            // -------------------------------------------------------------
            if (!save.unlockedAchievementIds.Contains(ACH_QUIZ_50))
            {
                if (save.totalQuestionsAnswered >= 50)
                {
                    save.unlockedAchievementIds.Add(ACH_QUIZ_50);
                    hasChanges = true;
                    OnAchievementUnlocked?.Invoke(ACH_QUIZ_50);
                    SafeLog("[AchievementManager] Unlock A2: Giải 50 câu trắc nghiệm.");
                }
            }

            // -------------------------------------------------------------
            // A3: Thu thập tất cả vật phẩm
            // Điều kiện: unlockedItemIds chứa ĐỦ chính xác cả 12 ID chuẩn từ PhysicsItemData.AllItems
            // -------------------------------------------------------------
            if (!save.unlockedAchievementIds.Contains(ACH_COLLECT_ALL_ITEMS))
            {
                if (save.unlockedItemIds != null)
                {
                    bool hasAllOfficialItems = true;
                    foreach (var item in PhysicsItemData.AllItems)
                    {
                        if (!save.unlockedItemIds.Contains(item.id))
                        {
                            hasAllOfficialItems = false;
                            break;
                        }
                    }

                    if (hasAllOfficialItems)
                    {
                        save.unlockedAchievementIds.Add(ACH_COLLECT_ALL_ITEMS);
                        hasChanges = true;
                        OnAchievementUnlocked?.Invoke(ACH_COLLECT_ALL_ITEMS);
                        SafeLog("[AchievementManager] Unlock A3: Thu thập tất cả vật phẩm.");
                    }
                }
            }

            // -------------------------------------------------------------
            // A4: Hoàn thành toàn bộ chương
            // Điều kiện: Toàn bộ bài học của cả 5 chương trong PhysicsCurriculumData đã hoàn thành
            // -------------------------------------------------------------
            if (!save.unlockedAchievementIds.Contains(ACH_COMPLETE_ALL_CHAPTERS))
            {
                bool allChaptersCompleted = true;
                for (int ch = 1; ch <= 5; ch++)
                {
                    var lessons = PhysicsCurriculumData.GetLessons(ch);
                    if (lessons == null || lessons.Count == 0) continue;

                    foreach (var l in lessons)
                    {
                        if (save.completedLessons == null || !save.completedLessons.Contains(l.lessonId))
                        {
                            allChaptersCompleted = false;
                            break;
                        }
                    }
                    if (!allChaptersCompleted) break;
                }

                if (allChaptersCompleted && save.completedLessons != null && save.completedLessons.Count >= 16)
                {
                    save.unlockedAchievementIds.Add(ACH_COMPLETE_ALL_CHAPTERS);
                    hasChanges = true;
                    OnAchievementUnlocked?.Invoke(ACH_COMPLETE_ALL_CHAPTERS);
                    SafeLog("[AchievementManager] Unlock A4: Hoàn thành toàn bộ chương.");
                }
            }

            // -------------------------------------------------------------
            // A5: Khám phá bí mật học viện
            // Điều kiện: hasDiscoveredAcademySecret == true (dành cho Phase 5 gameplay)
            // -------------------------------------------------------------
            if (!save.unlockedAchievementIds.Contains(ACH_DISCOVER_SECRET))
            {
                if (save.hasDiscoveredAcademySecret)
                {
                    save.unlockedAchievementIds.Add(ACH_DISCOVER_SECRET);
                    hasChanges = true;
                    OnAchievementUnlocked?.Invoke(ACH_DISCOVER_SECRET);
                    SafeLog("[AchievementManager] Unlock A5: Khám phá bí mật học viện.");
                }
            }

            // -------------------------------------------------------------
            // A6: 100 câu trả lời chính xác
            // Điều kiện: totalCorrectAnswers >= 100
            // -------------------------------------------------------------
            if (!save.unlockedAchievementIds.Contains(ACH_CORRECT_100))
            {
                if (save.totalCorrectAnswers >= 100)
                {
                    save.unlockedAchievementIds.Add(ACH_CORRECT_100);
                    hasChanges = true;
                    OnAchievementUnlocked?.Invoke(ACH_CORRECT_100);
                    SafeLog("[AchievementManager] Unlock A6: 100 câu trả lời chính xác.");
                }
            }

            // -------------------------------------------------------------
            // A7: Bậc thầy Aurelia
            // Điều kiện: Cả 6 thành tích A1-A6 đều đã được mở khóa
            // -------------------------------------------------------------
            if (!save.unlockedAchievementIds.Contains(ACH_AURELIA_MASTER))
            {
                bool a1ToA6Unlocked =
                    save.unlockedAchievementIds.Contains(ACH_COMPLETE_CH1) &&
                    save.unlockedAchievementIds.Contains(ACH_QUIZ_50) &&
                    save.unlockedAchievementIds.Contains(ACH_COLLECT_ALL_ITEMS) &&
                    save.unlockedAchievementIds.Contains(ACH_COMPLETE_ALL_CHAPTERS) &&
                    save.unlockedAchievementIds.Contains(ACH_DISCOVER_SECRET) &&
                    save.unlockedAchievementIds.Contains(ACH_CORRECT_100);

                if (a1ToA6Unlocked)
                {
                    save.unlockedAchievementIds.Add(ACH_AURELIA_MASTER);
                    hasChanges = true;
                    OnAchievementUnlocked?.Invoke(ACH_AURELIA_MASTER);
                    SafeLog("[AchievementManager] Unlock A7: Bậc thầy Aurelia!");
                }
            }

            if (hasChanges && !isExplicitSave)
            {
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }

            return save.unlockedAchievementIds.Count;
        }

        public static int GetUnlockedCount()
        {
            var save = PhysicsSaveManager.Load();
            return save != null && save.unlockedAchievementIds != null ? save.unlockedAchievementIds.Count : 0;
        }
    }
}
