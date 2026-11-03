using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Quản lý việc Đọc / Ghi dữ liệu tiến trình game ra file JSON tại Application.persistentDataPath.
    /// </summary>
    public static class PhysicsSaveManager
    {
        public const string SAVE_FILE_NAME = "physics_game2_save.json";
        private static PhysicsSaveData _cachedSaveData;

        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        public static event Action<PhysicsSaveData> OnSaveDataLoaded;
        public static event Action<PhysicsSaveData> OnSaveDataSaved;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _cachedSaveData = null;
            OnSaveDataLoaded = null;
            OnSaveDataSaved = null;
        }

        /// <summary>
        /// Kiểm tra xem file save đã tồn tại trên thiết bị chưa.
        /// </summary>
        public static bool HasSave()
        {
            try
            {
                return File.Exists(SaveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PhysicsSaveManager] Lỗi kiểm tra HasSave: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tải dữ liệu từ file JSON. Nếu chưa có, tạo dữ liệu mới mặc định.
        /// </summary>
        public static PhysicsSaveData Load()
        {
            if (_cachedSaveData != null)
            {
                return _cachedSaveData;
            }

            if (!HasSave())
            {
                _cachedSaveData = new PhysicsSaveData();
                Debug.Log("[PhysicsSaveManager] Chưa có file save, khởi tạo dữ liệu mặc định.");
                OnSaveDataLoaded?.Invoke(_cachedSaveData);
                return _cachedSaveData;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidDataException("Save JSON rỗng.");
                else
                {
                    _cachedSaveData = JsonUtility.FromJson<PhysicsSaveData>(json);
                    if (_cachedSaveData == null)
                    {
                        Debug.LogWarning("[PhysicsSaveManager] Dữ liệu JSON bị hỏng, khởi tạo mới.");
                        _cachedSaveData = new PhysicsSaveData();
                    }
                    else
                    {
                        // Đảm bảo tương thích ngược tuyệt đối với các bản save cũ
                        if (_cachedSaveData.completedLessons == null) _cachedSaveData.completedLessons = new List<string>();
                        if (_cachedSaveData.lessonScores == null) _cachedSaveData.lessonScores = new List<LessonScoreEntry>();
                        if (_cachedSaveData.unlockedItemIds == null) _cachedSaveData.unlockedItemIds = new List<string>();
                        if (_cachedSaveData.unlockedAchievementIds == null) _cachedSaveData.unlockedAchievementIds = new List<string>();
                        if (_cachedSaveData.completedWrittenLessons == null) _cachedSaveData.completedWrittenLessons = new List<string>();
                        if (_cachedSaveData.completedGameplayChapters == null) _cachedSaveData.completedGameplayChapters = new List<string>();
                        if (_cachedSaveData.completedExploreLessons == null) _cachedSaveData.completedExploreLessons = new List<string>();
                        if (_cachedSaveData.studySessions == null) _cachedSaveData.studySessions = new List<PhysicsStudySessionRecord>();
                        if (_cachedSaveData.quizHistory == null) _cachedSaveData.quizHistory = new List<PhysicsQuizHistoryEntry>();
                        if (_cachedSaveData.joinedChallengeWeekIds == null) _cachedSaveData.joinedChallengeWeekIds = new List<string>();
                        if (_cachedSaveData.activeStudySession != null && _cachedSaveData.activeStudySession.lessonIds == null)
                            _cachedSaveData.activeStudySession.lessonIds = new List<string>();
                        foreach (var study in _cachedSaveData.studySessions)
                            if (study != null && study.lessonIds == null) study.lessonIds = new List<string>();
                        if (_cachedSaveData.settings == null) _cachedSaveData.settings = new GameSettingsData();

                        bool isLegacySaveFile = !json.Contains("\"storyMigrationApplied\"");
                        StoryFlowController.ApplyStoryMigrationIfNeeded(_cachedSaveData, isLegacySaveFile);
                    }
                }

                Debug.Log($"[PhysicsSaveManager] Đã tải save thành công từ: {SaveFilePath}");
            }
            catch (Exception ex)
            {
                string backupPath = SaveFilePath + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                try
                {
                    if (File.Exists(SaveFilePath)) File.Copy(SaveFilePath, backupPath, false);
                }
                catch (Exception backupException)
                {
                    Debug.LogError($"[PhysicsSaveManager] Không thể sao lưu file save bị lỗi: {backupException.Message}");
                }
                Debug.LogError($"[PhysicsSaveManager] Lỗi khi đọc file save: {ex.Message}. Bản gốc được giữ tại {backupPath}; khởi tạo dữ liệu mới trong bộ nhớ.");
                _cachedSaveData = new PhysicsSaveData();
            }

            OnSaveDataLoaded?.Invoke(_cachedSaveData);
            return _cachedSaveData;
        }

        /// <summary>
        /// Ghi dữ liệu PhysicsSaveData ra file JSON.
        /// </summary>
        public static bool Save(PhysicsSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[PhysicsSaveManager] Dữ liệu save truyền vào bị null!");
                return false;
            }

            data.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _cachedSaveData = data;

            try
            {
                string dir = Path.GetDirectoryName(SaveFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonUtility.ToJson(data, true);
                string tempPath = SaveFilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(SaveFilePath))
                    File.Replace(tempPath, SaveFilePath, SaveFilePath + ".bak", true);
                else
                    File.Move(tempPath, SaveFilePath);

                Debug.Log($"[PhysicsSaveManager] Đã lưu tiến trình thành công: {SaveFilePath}");
                OnSaveDataSaved?.Invoke(_cachedSaveData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhysicsSaveManager] Lỗi khi ghi file save: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa file save và reset dữ liệu bộ nhớ về ban đầu.
        /// </summary>
        public static void ResetSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("[PhysicsSaveManager] Đã xóa file save thành công.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhysicsSaveManager] Lỗi khi xóa file save: {ex.Message}");
            }

            _cachedSaveData = new PhysicsSaveData();
            OnSaveDataSaved?.Invoke(_cachedSaveData);
        }
    }
}
