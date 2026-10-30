using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Quản lý dữ liệu mở khóa và truy vấn 12 vật phẩm trong Inventory.
    /// Liên kết trực tiếp với PhysicsSaveData.unlockedItemIds.
    /// </summary>
    public static class InventoryManager
    {
        public static event Action<string> OnItemUnlocked;

        private static void SafeLog(string msg)
        {
            try { Debug.Log(msg); } catch { }
        }

        private static void SafeLogWarning(string msg)
        {
            try { Debug.LogWarning(msg); } catch { }
        }

        /// <summary>
        /// Kiểm tra xem một vật phẩm đã được mở khóa chưa.
        /// </summary>
        public static bool IsUnlocked(string itemId, PhysicsSaveData customSave = null)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            var save = customSave ?? PhysicsSaveManager.Load();
            return save != null && save.unlockedItemIds != null && save.unlockedItemIds.Contains(itemId);
        }

        /// <summary>
        /// Mở khóa một vật phẩm mới.
        /// Tự động tránh trùng lặp, từ chối ID không hợp lệ, lưu dữ liệu và kích hoạt sự kiện kiểm tra thành tích.
        /// </summary>
        public static bool UnlockItem(string itemId, PhysicsSaveData customSave = null)
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            var item = PhysicsItemData.Get(itemId);
            if (item == null)
            {
                SafeLogWarning($"[InventoryManager] Từ chối mở khóa: Không tìm thấy vật phẩm có ID '{itemId}' trong danh mục chuẩn!");
                return false;
            }

            bool isCustomSave = customSave != null;
            var save = customSave ?? PhysicsSaveManager.Load();
            if (save == null) return false;

            if (save.unlockedItemIds == null)
            {
                save.unlockedItemIds = new List<string>();
            }

            // Nếu vật phẩm đã tồn tại trong danh sách: Không add lại, không save vô ích
            if (save.unlockedItemIds.Contains(itemId))
            {
                return false;
            }

            // Thêm vật phẩm hợp lệ vào danh sách
            save.unlockedItemIds.Add(itemId);

            if (!isCustomSave)
            {
                PhysicsSaveManager.Save(save);
                if (PhysicsGameSession.Instance != null)
                {
                    PhysicsGameSession.Instance.ReloadSaveData();
                }
            }

            SafeLog($"[InventoryManager] Đã mở khóa thành công vật phẩm: '{item.displayName}' ({itemId}).");
            OnItemUnlocked?.Invoke(itemId);

            // Tự động đánh giá lại thành tích (ví dụ thành tích 'collect_all_items')
            AchievementManager.EvaluateAchievements(isCustomSave ? save : null);
            return true;
        }

        /// <summary>
        /// Lấy tổng số lượng vật phẩm người chơi đã mở khóa.
        /// </summary>
        public static int GetUnlockedCount(PhysicsSaveData customSave = null)
        {
            var save = customSave ?? PhysicsSaveManager.Load();
            return save != null && save.unlockedItemIds != null ? save.unlockedItemIds.Count : 0;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ ID vật phẩm đã mở khóa.
        /// </summary>
        public static List<string> GetUnlockedItemIds(PhysicsSaveData customSave = null)
        {
            var save = customSave ?? PhysicsSaveManager.Load();
            return save != null && save.unlockedItemIds != null
                ? new List<string>(save.unlockedItemIds)
                : new List<string>();
        }
    }
}
