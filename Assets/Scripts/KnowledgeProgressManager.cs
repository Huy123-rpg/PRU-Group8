// ============================================================
// KnowledgeProgressManager.cs
// Lưu KNOWLEDGE PROGRESS theo Subject / Chapter / Lesson / QuestionType (#26).
//
//   Trắc nghiệm : Accuracy = đúng/tổng (%)  + số câu đã làm
//   Tự luận     : Average Score (quy đổi thang /10)
//   Chung       : Best Streak của lesson
//
// - Là STATIC class → không cần gắn GameObject nào, sống qua mọi scene.
// - Dữ liệu lưu JSON tại Application.persistentDataPath/knowledge_progress.json
//   và tự nạp khi game khởi động.
// - Sau này màn chọn bài (MainMenuManager) hiển thị:
//       BÀI 43  Nguyên phân và giảm phân
//       Progress: 72%  |  Trắc nghiệm: 78%  |  Tự luận: 7.6/10  |  Best Streak: 8
// ============================================================
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class KnowledgeProgressManager
{
    [Serializable]
    public class LessonStats
    {
        public string lessonID;

        // ---- Trắc nghiệm ----
        public int mcTotal;
        public int mcCorrect;

        // ---- Tự luận ----
        public int essayCount;
        public float essayScoreSum;   // tổng điểm quy đổi thang 10

        // ---- Chung ----
        public int bestStreak;

        public float McAccuracy => mcTotal > 0 ? (float)mcCorrect / mcTotal * 100f : 0f;
        public float EssayAverage => essayCount > 0 ? essayScoreSum / essayCount : 0f;

        /// <summary>Progress tổng hợp 0-100: trắc nghiệm ưu tiên accuracy, tự luận ưu tiên điểm TB, lấy max hai phần đã làm.</summary>
        public float OverallProgress
        {
            get
            {
                float p = 0f;
                if (mcTotal > 0) p = McAccuracy;
                if (essayCount > 0) p = Mathf.Max(p, EssayAverage * 10f);
                return Mathf.Clamp(p, 0f, 100f);
            }
        }
    }

    [Serializable]
    private class ProgressFile
    {
        public List<LessonStats> lessons = new List<LessonStats>();
    }

    private static readonly Dictionary<string, LessonStats> _stats = new Dictionary<string, LessonStats>();
    private static bool _loaded;

    private static string SavePath => Path.Combine(Application.persistentDataPath, "knowledge_progress.json");

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            if (!File.Exists(SavePath)) return;
            ProgressFile f = JsonUtility.FromJson<ProgressFile>(File.ReadAllText(SavePath));
            if (f == null || f.lessons == null) return;
            foreach (LessonStats s in f.lessons)
                _stats[s.lessonID] = s;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[KnowledgeProgress] Không đọc được file progress: " + e.Message);
        }
    }

    private static string Key(string subject, string chapterID, string lessonID)
        => $"{subject}|{chapterID}|{lessonID}";

    private static LessonStats Get(string subject, string chapterID, string lessonID, bool createIfMissing)
    {
        EnsureLoaded();
        string k = Key(subject, chapterID, lessonID);
        if (_stats.TryGetValue(k, out LessonStats s)) return s;
        if (!createIfMissing) return null;
        s = new LessonStats { lessonID = lessonID };
        _stats[k] = s;
        return s;
    }

    // ---------------- API GHI ----------------

    /// <summary>Ghi kết quả 1 câu TRẮC NGHIỆM.</summary>
    public static void RecordMultipleChoice(string subject, string chapterID, string lessonID, bool correct)
    {
        LessonStats s = Get(subject, chapterID, lessonID, true);
        s.mcTotal++;
        if (correct) s.mcCorrect++;
        Save();
    }

    /// <summary>Ghi kết quả 1 bài TỰ LUẬN (score theo thang của câu hỏi, tự quy đổi /10).</summary>
    public static void RecordEssay(string subject, string chapterID, string lessonID, float score, float maxScore)
    {
        LessonStats s = Get(subject, chapterID, lessonID, true);
        float normalized = maxScore > 0f ? score / maxScore * 10f : 0f;
        s.essayCount++;
        s.essayScoreSum += normalized;
        Save();
    }

    /// <summary>Cập nhật Best Streak nếu streak mới cao hơn.</summary>
    public static void RecordStreak(string subject, string chapterID, string lessonID, int streak)
    {
        LessonStats s = Get(subject, chapterID, lessonID, true);
        if (streak > s.bestStreak) { s.bestStreak = streak; Save(); }
    }

    // ---------------- API ĐỌC ----------------

    public static LessonStats GetStats(string subject, string chapterID, string lessonID)
        => Get(subject, chapterID, lessonID, false);

    /// <summary>"Trắc nghiệm: 78%" hoặc "" nếu chưa có dữ liệu.</summary>
    public static string McSummaryText(string subject, string chapterID, string lessonID)
    {
        LessonStats s = GetStats(subject, chapterID, lessonID);
        return s != null && s.mcTotal > 0 ? $"Trắc nghiệm: {s.McAccuracy:0}%" : "";
    }

    /// <summary>"Tự luận: 7.6/10" hoặc "" nếu chưa có dữ liệu.</summary>
    public static string EssaySummaryText(string subject, string chapterID, string lessonID)
    {
        LessonStats s = GetStats(subject, chapterID, lessonID);
        return s != null && s.essayCount > 0 ? $"Tự luận: {s.EssayAverage:0.0}/10" : "";
    }

    // ---------------- LƯU FILE ----------------

    private static void Save()
    {
        try
        {
            ProgressFile f = new ProgressFile { lessons = new List<LessonStats>(_stats.Values) };
            File.WriteAllText(SavePath, JsonUtility.ToJson(f, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[KnowledgeProgress] Không lưu được file progress: " + e.Message);
        }
    }
}
