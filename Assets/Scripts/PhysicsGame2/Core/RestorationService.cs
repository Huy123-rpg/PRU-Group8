using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class DeviceRestorationRecord
    {
        public string deviceId;
        public string chuongId;
        public string baiId;
        public bool isRestored;
        public float bestScore;
        public string restoredAtUtc;
        public string restoredByAttemptId;
    }

    /// <summary>
    /// Service quản lý tiến độ khôi phục 21 thiết bị theo G2_ThietBi.
    /// Quy tắc:
    /// - Đạt ScoreRaw >= 80 (chưa làm tròn, 79.96 KHÔNG được làm tròn lên 80) để khôi phục đúng một lần.
    /// - Học lại điểm thấp không làm mất tiến độ khôi phục (high-water mark).
    /// - Bản đồ hiển thị sáng từng trạm 1/7, 2/7... theo dữ liệu thật.
    /// </summary>
    public static class RestorationService
    {
        public const float PASS_THRESHOLD = 80.0f;

        public static bool IsDeviceRestored(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return false;
            var save = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load();
            if (save == null) return false;

            var dev = save.restoredDevices?.FirstOrDefault(d => d.deviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase));
            return dev != null && dev.isRestored;
        }

        public static float GetDeviceBestScore(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return 0f;
            var save = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load();
            if (save == null) return 0f;

            var dev = save.restoredDevices?.FirstOrDefault(d => d.deviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase));
            return dev != null ? dev.bestScore : 0f;
        }

        /// <summary>
        /// Ghi nhận kết quả của một lượt làm bài.
        /// Trả về true nếu thiết bị NÀY ĐƯỢC KHÔI PHỤC LẦN ĐẦU TIÊN trong lượt này.
        /// </summary>
        public static bool RecordAttemptResult(string deviceId, string baiId, float score, string attemptId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                var d = GameDataRepository.GetDeviceByLesson(baiId);
                if (d != null) deviceId = d.DeviceId;
            }

            if (string.IsNullOrEmpty(deviceId)) return false;

            var save = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load();
            if (save == null) return false;

            if (save.restoredDevices == null)
            {
                save.restoredDevices = new List<DeviceRestorationRecord>();
            }

            var record = save.restoredDevices.FirstOrDefault(d => d.deviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase));
            bool isFirstTimeRestored = false;

            if (record == null)
            {
                var devInfo = GameDataRepository.GetDeviceById(deviceId);
                record = new DeviceRestorationRecord
                {
                    deviceId = deviceId,
                    chuongId = devInfo != null ? devInfo.ChuongId : "chuong1",
                    baiId = baiId,
                    isRestored = false,
                    bestScore = score
                };
                save.restoredDevices.Add(record);
            }
            else
            {
                // Cập nhật best score theo MAX
                if (score > record.bestScore)
                {
                    record.bestScore = score;
                }
            }

            // Kiểm tra ngưỡng khôi phục (>= 80 nghiêm ngặt)
            if (score >= PASS_THRESHOLD && !record.isRestored)
            {
                record.isRestored = true;
                record.restoredAtUtc = DateTime.UtcNow.ToString("o");
                record.restoredByAttemptId = attemptId;
                isFirstTimeRestored = true;
                Debug.Log($"[RestorationService] Thiết bị {deviceId} vừa được KHÔI PHỤC với điểm {score:F1}/100!");

                // Đánh giá thành tích
                EvaluateAchievementsOnRestoration(save);
            }

            PhysicsSaveManager.Save(save);
            return isFirstTimeRestored;
        }

        public static int GetRestoredDeviceCount(int chapterIndex) => GetRestoredCountForChapter(chapterIndex);
        public static int GetTotalDeviceCount(int chapterIndex) => GetTotalDeviceCountForChapter(chapterIndex);

        public static string GetDeviceIdForLesson(string baiId)
        {
            var d = GameDataRepository.GetDeviceByLesson(baiId);
            return d != null ? d.DeviceId : "";
        }

        public static int GetRestoredCountForChapter(int chapterIndex)
        {
            var save = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load();
            if (save == null || save.restoredDevices == null) return 0;

            string chId = $"chuong{chapterIndex}";
            var chapterDevices = GameDataRepository.GetDevicesForChapter(chapterIndex);
            var devIds = new HashSet<string>(chapterDevices.Select(d => d.DeviceId), StringComparer.OrdinalIgnoreCase);

            return save.restoredDevices.Count(d => devIds.Contains(d.deviceId) && d.isRestored);
        }

        public static int GetTotalDeviceCountForChapter(int chapterIndex)
        {
            switch (chapterIndex)
            {
                case 1: return 4;
                case 2: return 7;
                case 3: return 4;
                case 4: return 3;
                case 5: return 3;
                default: return 4;
            }
        }

        public static bool IsChapterFullyRestored(int chapterIndex)
        {
            int restored = GetRestoredCountForChapter(chapterIndex);
            int total = GetTotalDeviceCountForChapter(chapterIndex);
            return restored >= total;
        }

        public static int GetTotalRestoredDevicesCount()
        {
            var save = PhysicsGameSession.Instance != null ? PhysicsGameSession.Instance.SaveData : PhysicsSaveManager.Load();
            if (save == null || save.restoredDevices == null) return 0;
            return save.restoredDevices.Count(d => d.isRestored);
        }

        private static void EvaluateAchievementsOnRestoration(PhysicsSaveData save)
        {
            int totalRestored = save.restoredDevices.Count(d => d.isRestored);

            // A01: Tia sáng đầu tiên (khôi phục >= 1 thiết bị)
            if (totalRestored >= 1) UnlockAchievement(save, "A01");

            // A02: Kỹ sư Cơ học (4/4 Chương I)
            if (GetRestoredCountForChapter(1) >= 4) UnlockAchievement(save, "A02");

            // A03: Người thắp sáng (7/7 Chương II)
            if (GetRestoredCountForChapter(2) >= 7) UnlockAchievement(save, "A03");

            // A04: Bậc thầy Điện học (4/4 Chương III)
            if (GetRestoredCountForChapter(3) >= 4) UnlockAchievement(save, "A04");

            // A05: Nhà phát minh Cảm ứng (3/3 Chương IV)
            if (GetRestoredCountForChapter(4) >= 3) UnlockAchievement(save, "A05");

            // A06: Vệ binh Năng lượng xanh (3/3 Chương V)
            if (GetRestoredCountForChapter(5) >= 3) UnlockAchievement(save, "A06");

            // A07: Người hồi sinh Aurelia (21/21 thiết bị)
            if (totalRestored >= 21) UnlockAchievement(save, "A07");
        }

        public static void UnlockAchievement(PhysicsSaveData save, string achievementId)
        {
            if (save == null) return;
            if (save.unlockedAchievementIds == null)
            {
                save.unlockedAchievementIds = new List<string>();
            }

            if (!save.unlockedAchievementIds.Contains(achievementId))
            {
                save.unlockedAchievementIds.Add(achievementId);
                Debug.Log($"[RestorationService] MỞ KHÓA THÀNH TÍCH: {achievementId}");
            }
        }
    }
}
