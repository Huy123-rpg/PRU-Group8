using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class LessonScoreEntry
    {
        public string lessonId;
        public int highestScore;
        public int correctCount;
        public int totalCount;
        public bool isCompleted;

        public LessonScoreEntry() { }

        public LessonScoreEntry(string id, int score, int correct, int total, bool completed)
        {
            lessonId = id;
            highestScore = score;
            correctCount = correct;
            totalCount = total;
            isCompleted = completed;
        }
    }

    [Serializable]
    public class GameSettingsData
    {
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public float brightness = 1.0f; // 0.2 (tối) đến 1.0 (sáng tối đa)
    }

    [Serializable]
    public class PhysicsQuizHistoryEntry
    {
        public string attemptId;
        public string lessonId;
        public string lessonTitle;
        public int chapterId;
        public string activityType;
        public int scorePercent;
        public int correctCount;
        public int wrongCount;
        public int unansweredCount;
        public int totalCount;
        public string assessmentKey;
        public bool isComplete;
        public float elapsedSeconds;
        public string sessionId;
        public string completedAtUtc;
    }

    [Serializable]
    public class PhysicsStudySessionRecord
    {
        public string sessionId;
        public string startedAtUtc;
        public string endedAtUtc;
        public string goal;
        public int plannedMinutes = 30;
        public float activeSeconds;
        public bool isActive;
        public bool endedByTime;
        public int answeredCount;
        public int quizAnsweredCount;
        public int ungradedAnsweredCount;
        public int correctCount;
        public List<string> lessonIds = new List<string>();
    }

    /// <summary>
    /// Cấu trúc dữ liệu lưu tiến trình chơi Physics Game 2.
    /// Chuẩn hóa cho Unity JsonUtility (không lưu tham chiếu GameObject/Sprite trực tiếp).
    /// </summary>
    [Serializable]
    public class PhysicsSaveData
    {
        // 1. Nhân vật & Giáo viên
        public string selectedCharacterId = "Lyra";
        public string selectedTeacherId = "Minh";

        // 2. Tiến trình Chương & Bài học
        public int unlockedChapter = 1; // 1 đến 5
        public List<string> completedLessons = new List<string>();
        public List<LessonScoreEntry> lessonScores = new List<LessonScoreEntry>();
        public int plannedStudyMinutes = 30;
        public string studyGoal = "Ôn tập";
        public bool endStudyWhenTimeReached;
        public PhysicsStudySessionRecord activeStudySession;
        public List<PhysicsStudySessionRecord> studySessions = new List<PhysicsStudySessionRecord>();
        public List<PhysicsQuizHistoryEntry> quizHistory = new List<PhysicsQuizHistoryEntry>();
        public int selectedChallengeWeekOffset;
        public List<string> joinedChallengeWeekIds = new List<string>();

        // 3. Thống kê câu hỏi & Lượt thi
        public int totalQuestionsAnswered = 0;
        public int totalCorrectAnswers = 0;
        public int totalQuizzesCompleted = 0;
        public int totalWrittenCompleted = 0;
        public List<string> completedWrittenLessons = new List<string>();
        /// <summary>
        /// Danh sách các chương mà người chơi đã hoàn thành trải nghiệm mini-game thực hành (ví dụ: "chapter_1", "chapter_2").
        /// Lưu ý ngữ nghĩa: Chỉ biểu thị đã hoàn thành gameplay mô phỏng của chương, không đại diện cho việc hoàn thành toàn bộ bài học lý thuyết.
        /// </summary>
        public List<string> completedGameplayChapters = new List<string>();
        public bool hasSeenMechanicsTutorial = false;
        public List<string> completedExploreLessons = new List<string>();
        public bool hasSeenOpticsTutorial = false;
        public bool hasSeenElectricityTutorial = false;
        public bool hasSeenElectromagnetismTutorial = false;
        public bool hasSeenEnergyTutorial = false;

        // 4. Vật phẩm & Thành tích (Chỉ lưu ID)
        public List<string> unlockedItemIds = new List<string>();
        public List<string> unlockedAchievementIds = new List<string>();
        public List<DeviceRestorationRecord> restoredDevices = new List<DeviceRestorationRecord>();

        // 5. Cài đặt hệ thống
        public GameSettingsData settings = new GameSettingsData();

        // 6. Điểm lưu để nút "Tiếp tục" hoạt động
        public string lastSceneName = PhysicsSceneNames.CharacterSelection;
        public int lastChapterId = 1;
        public string lastLessonId = "";
        public bool hasViewedGuide = false;
        public bool hasDiscoveredAcademySecret = false;
        public bool hasSeenFinalJourneyCompletion = false;
        public string lastSaveTime = "";

        // 7. Cốt truyện Aurelia Academy (Story System Flags)
        public bool storyMigrationApplied = false;
        public bool hasSeenAureliaIntro = false;

        public bool hasSeenTeacherIntroMinh = false;
        public bool hasSeenTeacherIntroLan = false;
        public bool hasSeenTeacherIntroDuy = false;
        public bool hasSeenTeacherIntroHa = false;

        public bool hasSeenCore1Intro = false;
        public bool hasSeenCore2Intro = false;
        public bool hasSeenCore3Intro = false;
        public bool hasSeenCore4Intro = false;
        public bool hasSeenCore5Intro = false;

        public bool hasSeenCore1RestoredStory = false;
        public bool hasSeenCore2RestoredStory = false;
        public bool hasSeenCore3RestoredStory = false;
        public bool hasSeenCore4RestoredStory = false;
        public bool hasSeenCore5RestoredStory = false;

        public bool hasSeenEnergyCrystalStory = false;

        public PhysicsSaveData()
        {
            // Thiết lập giá trị ban đầu hợp lệ
            unlockedChapter = 1;
            selectedCharacterId = "Lyra";
            selectedTeacherId = "Minh";
            lastSceneName = PhysicsSceneNames.CharacterSelection;
            lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ TRUY VẤN & CẬP NHẬT DỮ LIỆU
        // ==========================================

        public bool IsChapterUnlocked(int chapterNumber)
        {
            // Chapter selection is intentionally non-linear; progress never gates course content.
            return chapterNumber >= 1 && chapterNumber <= 5;
        }

        public void UnlockChapter(int chapterNumber)
        {
            if (chapterNumber > unlockedChapter && chapterNumber <= 5)
            {
                unlockedChapter = chapterNumber;
            }
        }

        public bool IsLessonCompleted(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return false;
            return completedLessons.Contains(lessonId);
        }

        public void MarkLessonCompleted(string lessonId, int score = 10, int correct = 10, int total = 10)
        {
            if (string.IsNullOrEmpty(lessonId)) return;

            if (!completedLessons.Contains(lessonId))
            {
                completedLessons.Add(lessonId);
            }

            LessonScoreEntry entry = lessonScores.Find(e => e.lessonId == lessonId);
            if (entry == null)
            {
                lessonScores.Add(new LessonScoreEntry(lessonId, score, correct, total, true));
            }
            else
            {
                if (score > entry.highestScore)
                {
                    entry.highestScore = score;
                }
                entry.correctCount = Mathf.Max(entry.correctCount, correct);
                entry.totalCount = total;
                entry.isCompleted = true;
            }
        }

        /// <summary>
        /// Ghi nhận kết quả sau một lượt thi Trắc nghiệm (Quiz).
        /// Cập nhật: totalQuestionsAnswered, totalCorrectAnswers, totalQuizzesCompleted, best score, completedLessons.
        /// </summary>
        public void RecordQuizCompletion(string lessonId, int correct, int total)
        {
            totalQuestionsAnswered += total;
            totalCorrectAnswers += correct;
            totalQuizzesCompleted += 1;

            int accuracyPercent = total > 0 ? (int)((float)correct / total * 100f) : 0;
            bool isPassed = accuracyPercent >= 70;

            if (string.IsNullOrEmpty(lessonId)) return;

            LessonScoreEntry entry = lessonScores.Find(e => e.lessonId == lessonId);
            if (entry == null)
            {
                lessonScores.Add(new LessonScoreEntry(lessonId, correct, correct, total, isPassed));
            }
            else
            {
                if (correct > entry.highestScore)
                {
                    entry.highestScore = correct;
                }
                entry.correctCount = Mathf.Max(entry.correctCount, correct);
                entry.totalCount = total;
                if (isPassed) entry.isCompleted = true;
            }

            if (isPassed && !completedLessons.Contains(lessonId))
            {
                completedLessons.Add(lessonId);
            }
        }

        public void RecordQuizAttempt(PhysicsQuizHistoryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.attemptId)) return;
            if (quizHistory == null) quizHistory = new List<PhysicsQuizHistoryEntry>();
            if (quizHistory.Exists(x => x != null && x.attemptId == entry.attemptId)) return;
            quizHistory.Add(entry);
            if (activeStudySession != null && activeStudySession.isActive && entry.sessionId == activeStudySession.sessionId)
            {
                activeStudySession.answeredCount += entry.correctCount + entry.wrongCount;
                activeStudySession.quizAnsweredCount += entry.correctCount + entry.wrongCount;
                activeStudySession.correctCount += entry.correctCount;
                if (!string.IsNullOrEmpty(entry.lessonId) && !activeStudySession.lessonIds.Contains(entry.lessonId))
                    activeStudySession.lessonIds.Add(entry.lessonId);
            }
        }

        public void RecordWrittenStudyActivity(string lessonId)
        {
            var study = activeStudySession;
            if (study == null || !study.isActive) return;
            study.answeredCount++;
            study.ungradedAnsweredCount++;
            if (!string.IsNullOrEmpty(lessonId) && !study.lessonIds.Contains(lessonId)) study.lessonIds.Add(lessonId);
        }

        /// <summary>
        /// Ghi nhận kết quả sau khi nộp bài Tự luận (Written).
        /// KHÔNG làm sai lệch thống kê trắc nghiệm (totalQuizzesCompleted, totalCorrectAnswers).
        /// </summary>
        public void RecordWrittenCompletion(string lessonId, bool isQuality)
        {
            totalWrittenCompleted += 1;
            if (isQuality && !string.IsNullOrEmpty(lessonId) && !completedWrittenLessons.Contains(lessonId))
            {
                completedWrittenLessons.Add(lessonId);
            }
        }

        public int GetLessonHighScore(string lessonId)
        {
            LessonScoreEntry entry = lessonScores.Find(e => e.lessonId == lessonId);
            return entry != null ? entry.highestScore : 0;
        }

        public bool IsItemUnlocked(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            return unlockedItemIds.Contains(itemId);
        }

        public bool UnlockItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            if (!unlockedItemIds.Contains(itemId))
            {
                unlockedItemIds.Add(itemId);
                return true;
            }
            return false;
        }

        public bool IsAchievementUnlocked(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId)) return false;
            return unlockedAchievementIds.Contains(achievementId);
        }

        public bool UnlockAchievement(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId)) return false;
            if (!unlockedAchievementIds.Contains(achievementId))
            {
                unlockedAchievementIds.Add(achievementId);
                return true;
            }
            return false;
        }

        public bool IsGameplayChapterCompleted(string chapterId)
        {
            if (string.IsNullOrEmpty(chapterId) || completedGameplayChapters == null) return false;
            return completedGameplayChapters.Contains(chapterId);
        }

        public void MarkGameplayChapterCompleted(string chapterId)
        {
            if (string.IsNullOrEmpty(chapterId)) return;
            if (completedGameplayChapters == null) completedGameplayChapters = new List<string>();
            if (!completedGameplayChapters.Contains(chapterId))
            {
                completedGameplayChapters.Add(chapterId);
            }
        }

        public bool IsExploreLessonCompleted(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId) || completedExploreLessons == null) return false;
            return completedExploreLessons.Contains(lessonId);
        }

        public void MarkExploreLessonCompleted(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return;
            if (completedExploreLessons == null) completedExploreLessons = new List<string>();
            if (!completedExploreLessons.Contains(lessonId))
            {
                completedExploreLessons.Add(lessonId);
            }
        }
    }
}
