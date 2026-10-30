using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class PhysicsQuestionRaw
    {
        public string questionID;
        public string questionText;
        public string[] answers;
        public int correctAnswerIndex;
        public string chapter;
        public string difficulty;
        public string explanation;
        public string keyKnowledge;
    }

    [Serializable]
    public class PhysicsQuestionDataWrapper
    {
        public List<PhysicsQuestionRaw> questions;
    }

    public class LessonScoringRule
    {
        public string chapter;
        public string name;
        public string[] core;
        public string[] exclusive;
        public string[] secondary;
        public string[] negative;

        public LessonScoringRule(string ch, string name, string[] core, string[] exclusive, string[] secondary, string[] negative)
        {
            this.chapter = ch;
            this.name = name;
            this.core = core;
            this.exclusive = exclusive;
            this.secondary = secondary;
            this.negative = negative;
        }
    }

    public class ScoredQuestion
    {
        public PhysicsQuestionRaw raw;
        public float score;
        public string primaryLesson;
    }

    /// <summary>
    /// Repository quản lý ngân hàng 830 câu hỏi trắc nghiệm Vật lý 9 từ physics_questions.json.
    /// Tự động cache sau lần đọc đầu tiên.
    /// Phân loại câu hỏi theo Lesson Relevance Scoring (Core, Exclusive, Secondary, Negative).
    /// Ưu tiên độ phù hợp bài học (Lesson Relevance / Primary Lesson) trước, sau đó mới áp dụng độ khó Giáo viên.
    /// Tuyệt đối không bù câu bừa bãi giữa các bài học khác nhau trong cùng chương.
    /// </summary>
    public static class PhysicsQuestionRepository
    {
        public const string WHOLE_BOOK_REVIEW_ID = "ALL_REVIEW";
        private static List<PhysicsQuestionRaw> _cachedQuestions = null;
        private static bool _isLoaded = false;

        public static readonly Dictionary<string, LessonScoringRule> LessonRules = new Dictionary<string, LessonScoringRule>
        {
            // CHƯƠNG I: NĂNG LƯỢNG CƠ HỌC
            {
                "C1_L2", new LessonScoringRule(
                    "Chương I", "Động năng và Thế năng",
                    new[] { "động năng", "thế năng", "wđ", "wt" },
                    new[] { "tốc độ của vật tăng", "khối lượng và vận tốc", "khối lượng và độ cao", "m.v^2", "m.g.h", "đơn vị của động năng", "đơn vị của thế năng", "thế năng trọng trường", "thế năng đàn hồi", "tính động năng", "tính thế năng" },
                    new[] { "vận tốc", "tốc độ", "độ cao", "rơi tự do", "ném lên" },
                    new[] { "công cơ học", "công suất", "bảo toàn cơ năng", "định luật bảo toàn cơ năng", "cơ năng", "tổng động năng và thế năng", "w = wđ + wt" }
                )
            },
            {
                "C1_L3", new LessonScoringRule(
                    "Chương I", "Cơ năng",
                    new[] { "cơ năng", "bảo toàn cơ năng", "tổng động năng và thế năng", "w = wđ + wt", "chuyển hóa động năng và thế năng", "chuyển hóa cơ năng", "tổng động năng", "tổng thế năng" },
                    new[] { "định luật bảo toàn cơ năng", "đơn vị của cơ năng", "công thức tính cơ năng", "bảo toàn", "chuyển hóa hoàn toàn", "chuyển hóa qua lại" },
                    new[] { "chuyển hóa", "vị trí cân bằng", "vị trí biên", "con lắc", "đỉnh dốc", "chân dốc" },
                    new[] { "công cơ học", "công suất", "oát", "watt", "p = a/t" }
                )
            },
            {
                "C1_L4", new LessonScoringRule(
                    "Chương I", "Công và Công suất",
                    new[] { "công cơ học", "công suất", "sinh công", "thực hiện công" },
                    new[] { "jun", "oát", "watt", "a = f . s", "a = f.s", "p = a/t", "đơn vị của công suất", "đơn vị của công", "lực f kéo vật" },
                    new[] { "quãng đường", "lực tác dụng", "thời gian thực hiện công", "máy thực hiện" },
                    new[] { "cơ năng", "động năng", "thế năng" }
                )
            },

            // CHƯƠNG II: ÁNH SÁNG
            {
                "C2_L5", new LessonScoringRule(
                    "Chương II", "Khúc xạ ánh sáng",
                    new[] { "khúc xạ ánh sáng", "hiện tượng khúc xạ", "định luật khúc xạ", "góc khúc xạ", "chiết suất", "mặt phân cách" },
                    new[] { "tia khúc xạ", "chiết suất tuyệt đối", "sin i / sin r", "môi trường trong suốt", "gãy khúc tại mặt phân cách" },
                    new[] { "góc tới", "tia sáng", "pháp tuyến", "truyền từ nước sang không khí", "truyền từ không khí vào nước" },
                    new[] { "phản xạ toàn phần", "lăng kính", "thấu kính", "kính lúp", "tiêu cự" }
                )
            },
            {
                "C2_L6", new LessonScoringRule(
                    "Chương II", "Phản xạ toàn phần",
                    new[] { "phản xạ toàn phần", "góc giới hạn", "cáp quang", "sợi quang" },
                    new[] { "igh", "sin igh", "chiết quang hơn", "chiết quang kém", "không có tia khúc xạ", "tia phản xạ rất sáng", "toàn bộ tia sáng bị phản xạ" },
                    new[] { "góc tới lớn hơn", "mặt phân cách", "lăng kính phản xạ toàn phần" },
                    new[] { "thấu kính hội tụ", "thấu kính phân kì", "thấu kính phân kỳ", "kính lúp", "tiêu cự", "tiêu điểm" }
                )
            },
            {
                "C2_L7", new LessonScoringRule(
                    "Chương II", "Lăng kính",
                    new[] { "lăng kính", "tán sắc ánh sáng", "hiện tượng tán sắc", "góc lệch của tia sáng" },
                    new[] { "tiết diện lăng kính", "ánh sáng trắng qua lăng kính", "dải màu cầu vồng", "đỏ cam vàng lục lam chàm tím", "lệch về phía đáy lăng kính", "ánh sáng đơn sắc" },
                    new[] { "đơn sắc", "lệch về phía đáy", "tán sắc" },
                    new[] { "thấu kính", "tiêu cự", "kính lúp", "cáp quang", "đo tiêu cự" }
                )
            },
            {
                "C2_L8", new LessonScoringRule(
                    "Chương II", "Thấu kính",
                    new[] { "thấu kính hội tụ", "thấu kính phân kì", "thấu kính phân kỳ", "quang tâm", "trục chính", "tiêu điểm chính" },
                    new[] { "ảnh thật", "ảnh ảo của thấu kính", "hứng được trên màn", "đặc điểm ảnh qua thấu kính", "tia tới qua quang tâm", "tia tới song song trục chính", "khoảng tiêu cự" },
                    new[] { "tiêu cự", "tiêu điểm", "ảnh ảo" },
                    new[] { "lăng kính", "phản xạ toàn phần", "kính lúp", "đo tiêu cự bằng thực nghiệm", "dụng cụ thí nghiệm đo tiêu cự", "đo tiêu cự" }
                )
            },
            {
                "C2_L9", new LessonScoringRule(
                    "Chương II", "Thực hành đo tiêu cự",
                    new[] { "đo tiêu cự", "thực hành", "thí nghiệm đo tiêu cự", "màn chắn", "giá quang học", "đo khoảng cách" },
                    new[] { "phương pháp đo tiêu cự", "d = d' = 2f", "khoảng cách vật và ảnh", "đo khoảng cách từ vật đến màn", "ảnh rõ nét trên màn", "sai số phép đo", "d = 2f", "thực nghiệm đo tiêu cự" },
                    new[] { "đo tiêu cự thấu kính", "vật và ảnh đối xứng", "thực nghiệm", "d'" },
                    new[] { "lăng kính", "kính lúp", "cáp quang", "quang tâm o" }
                )
            },
            {
                "C2_L10", new LessonScoringRule(
                    "Chương II", "Kính lúp",
                    new[] { "kính lúp", "độ bội giác", "bội giác" },
                    new[] { "quan sát vật nhỏ", "g = 25/f", "số bội giác", "vật đặt trong khoảng tiêu cự của kính lúp", "kính lúp có số bội giác", "dùng kính lúp" },
                    new[] { "kính lúp có tiêu cự", "thấu kính hội tụ có tiêu cự ngắn", "quan sát" },
                    new[] { "lăng kính", "cáp quang", "thực hành đo tiêu cự", "thấu kính phân kì", "thấu kính phân kỳ" }
                )
            },

            // CHƯƠNG III: ĐIỆN
            {
                "C3_L11", new LessonScoringRule(
                    "Chương III", "Điện trở. Định luật Ohm",
                    new[] { "định luật ohm", "định luật ôm", "điện trở của dây dẫn", "điện trở suất", "i = u/r" },
                    new[] { "chiều dài và tiết diện", "r = p.l/s", "ôm (ω)", "phụ thuộc vào chiều dài", "phụ thuộc vào tiết diện", "vật liệu làm dây", "tỉ lệ nghịch với điện trở", "tỉ lệ thuận với hiệu điện thế" },
                    new[] { "điện trở", "hiệu điện thế", "cường độ dòng điện", "dây dẫn" },
                    new[] { "mắc nối tiếp", "mắc song song", "đoạn mạch nối tiếp", "đoạn mạch song song", "công suất điện", "jun-len-xơ", "nhiệt lượng tỏa ra" }
                )
            },
            {
                "C3_L12", new LessonScoringRule(
                    "Chương III", "Đoạn mạch nối tiếp và song song",
                    new[] { "nối tiếp", "song song", "đoạn mạch nối tiếp", "đoạn mạch song song", "mắc nối tiếp", "mắc song song" },
                    new[] { "điện trở tương đương", "rtd", "r1 nối tiếp r2", "r1 song song r2", "mạch phân nhánh", "u = u1 + u2", "i = i1 + i2", "1/rtd = 1/r1 + 1/r2", "rtd = r1 + r2" },
                    new[] { "cường độ dòng điện trong mạch", "hiệu điện thế hai đầu mạch", "hai điện trở" },
                    new[] { "điện trở suất", "chiều dài và tiết diện dây", "công suất điện", "jun-len-xơ", "điện năng tiêu thụ" }
                )
            },
            {
                "C3_L13", new LessonScoringRule(
                    "Chương III", "Năng lượng điện. Công suất điện",
                    new[] { "công suất điện", "điện năng", "năng lượng điện", "định luật jun-len-xơ", "jun-len-xơ" },
                    new[] { "p = u.i", "a = u.i.t", "q = i^2.r.t", "công tơ điện", "số đếm của công tơ", "kwh", "kilôoát giờ", "nhiệt lượng tỏa ra trên dây dẫn", "nhiệt lượng tỏa ra" },
                    new[] { "bóng đèn ghi 220v", "công suất định mức", "hiệu điện thế định mức", "đoạn mạch tiêu thụ" },
                    new[] { "điện trở suất", "chiều dài dây dẫn", "chiết suất" }
                )
            },

            // CHƯƠNG IV: ĐIỆN TỪ
            {
                "C4_L14", new LessonScoringRule(
                    "Chương IV", "Hiện tượng cảm ứng điện từ",
                    new[] { "cảm ứng điện từ", "dòng điện cảm ứng", "từ thông", "đường sức từ", "cuộn dây kín" },
                    new[] { "nam châm lại gần", "nam châm ra xa", "biến thiên số đường sức từ", "điều kiện xuất hiện dòng điện cảm ứng", "nam châm quay", "số đường sức từ xuyên qua" },
                    new[] { "cuộn dây dẫn", "nam châm vĩnh cửu", "nam châm điện", "dòng điện xoay chiều" },
                    new[] { "máy biến thế", "truyền tải điện năng", "hao phí trên đường dây tải điện", "công suất hao phí", "u1/u2 = n1/n2" }
                )
            },
            {
                "C4_L15", new LessonScoringRule(
                    "Chương IV", "Tác dụng của dòng điện xoay chiều",
                    new[] { "tác dụng của dòng điện xoay chiều", "máy biến thế", "truyền tải điện năng", "máy biến áp" },
                    new[] { "hao phí do tỏa nhiệt", "công suất hao phí", "giảm hao phí", "tăng hiệu điện thế", "u1/u2 = n1/n2", "cuộn sơ cấp", "cuộn thứ cấp", "tác dụng nhiệt của dòng điện xoay chiều", "tác dụng từ của dòng điện xoay chiều", "tác dụng quang", "loa điện" },
                    new[] { "dòng điện xoay chiều", "dây tải điện", "đường dây truyền tải", "tác dụng nhiệt", "tác dụng từ" },
                    new[] { "điều kiện xuất hiện dòng điện cảm ứng", "nam châm lại gần cuộn dây" }
                )
            },

            // CHƯƠNG V: NĂNG LƯỢNG VỚI CUỘC SỐNG
            {
                "C5_L16", new LessonScoringRule(
                    "Chương V", "Vòng năng lượng trên Trái Đất",
                    new[] { "vòng năng lượng", "nhiên liệu hóa thạch", "năng lượng hóa thạch", "than đá", "dầu mỏ", "khí tự nhiên" },
                    new[] { "không tái tạo", "hình thành từ xác sinh vật", "hàng triệu năm", "hiệu ứng nhà kính", "phát thải khí co2", "ô nhiễm không khí do đốt", "trữ lượng có hạn", "nhiên liệu không tái tạo" },
                    new[] { "nguồn năng lượng", "bảo vệ môi trường", "khí quyển" },
                    new[] { "pin mặt trời", "tua-bin gió", "thủy điện", "năng lượng sinh khối", "năng lượng địa nhiệt", "năng lượng thủy triều" }
                )
            },
            {
                "C5_L17", new LessonScoringRule(
                    "Chương V", "Năng lượng tái tạo",
                    new[] { "năng lượng tái tạo", "pin mặt trời", "tua-bin gió", "năng lượng gió", "thủy điện", "sinh khối", "địa nhiệt", "thủy triều" },
                    new[] { "liên tục được tái sinh", "không bị cạn kiệt", "quang điện", "quang năng thành điện năng", "năng lượng mặt trời", "biomass", "thân thiện với môi trường", "năng lượng sạch" },
                    new[] { "tái tạo", "năng lượng sạch", "tự nhiên", "mặt trời" },
                    new[] { "nhiên liệu hóa thạch", "than đá", "dầu mỏ", "khí tự nhiên" }
                )
            }
        };

        public static void EnsureLoaded()
        {
            if (_isLoaded && _cachedQuestions != null && _cachedQuestions.Count > 0 &&
                !string.IsNullOrEmpty(_cachedQuestions[0].keyKnowledge))
            {
                return;
            }

            _cachedQuestions = new List<PhysicsQuestionRaw>();

            try
            {
                string jsonText = null;
                string filePath = Path.Combine(Application.dataPath, "Resources", "QuizData", "physics_questions.json");
                if (File.Exists(filePath))
                {
                    jsonText = File.ReadAllText(filePath);
                }

                if (string.IsNullOrEmpty(jsonText))
                {
                    TextAsset jsonAsset = Resources.Load<TextAsset>("QuizData/physics_questions");
                    if (jsonAsset != null && !string.IsNullOrEmpty(jsonAsset.text))
                    {
                        jsonText = jsonAsset.text;
                    }
                }

                if (!string.IsNullOrEmpty(jsonText))
                {
                    var wrapper = JsonUtility.FromJson<PhysicsQuestionDataWrapper>(jsonText);
                    if (wrapper != null && wrapper.questions != null)
                    {
                        _cachedQuestions = wrapper.questions;
                        _isLoaded = true;
                        Debug.Log($"[PhysicsQuestionRepository] Đã tải thành công {_cachedQuestions.Count} câu hỏi từ physics_questions.json.");
                        return;
                    }
                }

                Debug.LogWarning("[PhysicsQuestionRepository] Không thể parse dữ liệu JSON câu hỏi!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhysicsQuestionRepository] Lỗi khi nạp physics_questions.json: {ex.Message}");
            }

            _isLoaded = true;
        }

        public static int TotalQuestionsCount
        {
            get
            {
                EnsureLoaded();
                return _cachedQuestions != null ? _cachedQuestions.Count : 0;
            }
        }

        public static float CalculateScore(PhysicsQuestionRaw q, LessonScoringRule rule)
        {
            if (q == null || rule == null) return 0f;

            string qOnly = (q.questionText ?? "").ToLower();
            string text = (qOnly + " " + (q.explanation ?? "")).ToLower();

            float score = 0f;

            // Core phrase: +4 nếu trong câu hỏi chính, +3 nếu trong phần giải thích
            if (rule.core != null)
            {
                foreach (var kw in rule.core)
                {
                    if (qOnly.Contains(kw)) score += 4f;
                    else if (text.Contains(kw)) score += 3f;
                }
            }

            // Exclusive phrase: +4 nếu trong câu hỏi chính, +3 nếu trong phần giải thích
            if (rule.exclusive != null)
            {
                foreach (var kw in rule.exclusive)
                {
                    if (qOnly.Contains(kw)) score += 4f;
                    else if (text.Contains(kw)) score += 3f;
                }
            }

            // Secondary phrase: +1.5 nếu trong câu hỏi chính, +1 nếu trong giải thích
            if (rule.secondary != null)
            {
                foreach (var kw in rule.secondary)
                {
                    if (qOnly.Contains(kw)) score += 1.5f;
                    else if (text.Contains(kw)) score += 1f;
                }
            }

            // Negative phrase: -2.5 nếu chứa từ khóa thuộc bài học khác trong cùng chương
            if (rule.negative != null)
            {
                foreach (var kw in rule.negative)
                {
                    if (text.Contains(kw)) score -= 2.5f;
                }
            }

            return Mathf.Max(0f, Mathf.Round(score * 10f) / 10f);
        }

        public static string GetPrimaryLesson(PhysicsQuestionRaw q, int chapterNumber, out float bestScore)
        {
            string bestLesson = "Unclassified";
            bestScore = 0f;

            string chapterKeyword = GetChapterKeyword(chapterNumber);
            foreach (var kvp in LessonRules)
            {
                if (kvp.Value.chapter.IndexOf(chapterKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    float s = CalculateScore(q, kvp.Value);
                    if (s > bestScore)
                    {
                        bestScore = s;
                        bestLesson = kvp.Key;
                    }
                }
            }

            if (bestScore < 2.0f)
            {
                bestLesson = "Unclassified";
            }

            return bestLesson;
        }

        /// <summary>
        /// Lấy danh sách câu hỏi trắc nghiệm cho phiên làm bài.
        /// - Nếu là bài TEST (EndsWith "_TEST"): Lấy toàn bộ câu hỏi trong chương.
        /// - Nếu là bài học cụ thể:
        ///   1. Phân loại theo Lesson Relevance Scoring (Core, Exclusive, Secondary, Negative).
        ///   2. Xác định Primary Lesson. Ưu tiên tuyệt đối câu hỏi có Primary = lessonId.
        ///   3. Áp dụng phong cách Giáo viên trên tập ứng viên đã đảm bảo Relevance.
        ///   4. Không bù câu bừa bãi giữa các bài học khác nhau trong cùng chương.
        /// </summary>
        public static List<PhysicsQuestionRaw> GetQuizQuestions(int chapterNumber, string lessonId, string teacherId = "Minh", int targetCount = 10)
        {
            EnsureLoaded();

            if (_cachedQuestions == null || _cachedQuestions.Count == 0)
            {
                return new List<PhysicsQuestionRaw>();
            }

            if (string.Equals(lessonId, WHOLE_BOOK_REVIEW_ID, StringComparison.OrdinalIgnoreCase))
            {
                List<PhysicsQuestionRaw> reviewPool = FilterByTeacherStrategyRaw(new List<PhysicsQuestionRaw>(_cachedQuestions), teacherId);
                int reviewCount = Mathf.Min(targetCount, reviewPool.Count);
                List<PhysicsQuestionRaw> review = reviewPool.GetRange(0, reviewCount);
                ShuffleList(review);
                return review;
            }

            // 1. Lọc theo Chương
            string chapterKeyword = GetChapterKeyword(chapterNumber);
            List<PhysicsQuestionRaw> chapterQuestions = _cachedQuestions.FindAll(q =>
                !string.IsNullOrEmpty(q.chapter) && q.chapter.IndexOf(chapterKeyword, StringComparison.OrdinalIgnoreCase) >= 0);

            if (chapterQuestions.Count == 0)
            {
                chapterQuestions = new List<PhysicsQuestionRaw>(_cachedQuestions);
            }

            bool isChapterTest = !string.IsNullOrEmpty(lessonId) && lessonId.EndsWith("_TEST", StringComparison.OrdinalIgnoreCase);

            if (isChapterTest)
            {
                // Bài TEST tổng hợp chương: Sử dụng toàn bộ ngân hàng câu hỏi của chương
                List<PhysicsQuestionRaw> testPrioritized = FilterByTeacherStrategyRaw(chapterQuestions, teacherId);
                int count = Mathf.Min(targetCount, testPrioritized.Count);
                List<PhysicsQuestionRaw> res = testPrioritized.GetRange(0, count);
                ShuffleList(res);
                return res;
            }

            // 2. Tính điểm liên quan (Relevance Scoring) và xác định Primary Lesson
            List<ScoredQuestion> scoredCandidates = new List<ScoredQuestion>();
            foreach (var q in chapterQuestions)
            {
                string primary = GetPrimaryLesson(q, chapterNumber, out float _);

                float lessonScore = 0f;
                if (LessonRules.TryGetValue(lessonId, out var rule))
                {
                    lessonScore = CalculateScore(q, rule);
                }

                if (lessonScore >= 2.0f)
                {
                    scoredCandidates.Add(new ScoredQuestion
                    {
                        raw = q,
                        score = lessonScore,
                        primaryLesson = primary
                    });
                }
            }

            if (scoredCandidates.Count == 0)
            {
                Debug.LogWarning($"[PhysicsQuestionRepository] Không tìm thấy câu hỏi khớp với bài '{lessonId}' (Chương {chapterNumber}).");
                return new List<PhysicsQuestionRaw>();
            }

            // 3. Phân tầng ưu tiên:
            // Tier 1: Primary Lesson chính xác là bài hiện tại (đặc trưng nhất)
            // Tier 2: Score >= 3.0f (liên quan mạnh nhưng có thể tie hoặc secondary)
            List<ScoredQuestion> tier1 = scoredCandidates.FindAll(s => s.primaryLesson == lessonId);
            tier1.Sort((a, b) => b.score.CompareTo(a.score));

            List<ScoredQuestion> tier2 = scoredCandidates.FindAll(s => s.primaryLesson != lessonId && s.score >= 3.0f);
            tier2.Sort((a, b) => b.score.CompareTo(a.score));

            List<ScoredQuestion> candidatePool = new List<ScoredQuestion>();
            candidatePool.AddRange(tier1);
            if (candidatePool.Count < targetCount)
            {
                candidatePool.AddRange(tier2);
            }

            // 4. Áp dụng phong cách Giáo viên TRÊN TẬP ỨNG VIÊN ĐÃ ĐẢM BẢO RELEVANCE
            // Nếu cùng độ khó thì ưu tiên câu có Relevance Score cao hơn
            List<PhysicsQuestionRaw> prioritized = FilterByTeacherStrategyWithRelevance(candidatePool, teacherId);

            int finalCount = Mathf.Min(targetCount, prioritized.Count);
            List<PhysicsQuestionRaw> selected = prioritized.GetRange(0, finalCount);

            ShuffleList(selected);

            Debug.Log($"[PhysicsQuestionRepository] Đã chọn {selected.Count} câu hỏi cho bài '{lessonId}' (Chương {chapterNumber}, GV: {teacherId}). Tổng Primary: {tier1.Count}.");
            return selected;
        }

        public static string GetChapterKeyword(int chapterNumber)
        {
            switch (chapterNumber)
            {
                case 1: return "Chương I";
                case 2: return "Chương II";
                case 3: return "Chương III";
                case 4: return "Chương IV";
                case 5: return "Chương V";
                default: return "Chương I";
            }
        }

        private static List<PhysicsQuestionRaw> FilterByTeacherStrategyWithRelevance(List<ScoredQuestion> pool, string teacherId)
        {
            List<ScoredQuestion> easyList = new List<ScoredQuestion>();
            List<ScoredQuestion> medList = new List<ScoredQuestion>();
            List<ScoredQuestion> hardList = new List<ScoredQuestion>();

            foreach (var item in pool)
            {
                if (string.Equals(item.raw.difficulty, "Easy", StringComparison.OrdinalIgnoreCase))
                    easyList.Add(item);
                else if (string.Equals(item.raw.difficulty, "Medium", StringComparison.OrdinalIgnoreCase))
                    medList.Add(item);
                else
                    hardList.Add(item);
            }

            // Trong từng nhóm độ khó, ưu tiên câu có điểm Relevance cao hơn
            easyList.Sort((a, b) => b.score.CompareTo(a.score));
            medList.Sort((a, b) => b.score.CompareTo(a.score));
            hardList.Sort((a, b) => b.score.CompareTo(a.score));

            List<PhysicsQuestionRaw> result = new List<PhysicsQuestionRaw>();

            switch (teacherId)
            {
                case "Minh": // Nền tảng: Easy -> Medium -> Hard
                    easyList.ForEach(x => result.Add(x.raw));
                    medList.ForEach(x => result.Add(x.raw));
                    hardList.ForEach(x => result.Add(x.raw));
                    break;

                case "Lan": // Ứng dụng: Medium -> Easy -> Hard
                    medList.ForEach(x => result.Add(x.raw));
                    easyList.ForEach(x => result.Add(x.raw));
                    hardList.ForEach(x => result.Add(x.raw));
                    break;

                case "Duy": // Nâng cao: Hard -> Medium -> Easy
                    hardList.ForEach(x => result.Add(x.raw));
                    medList.ForEach(x => result.Add(x.raw));
                    easyList.ForEach(x => result.Add(x.raw));
                    break;

                case "Ha": // Tổng hợp: Trộn Hard & Medium, sau đó đến Easy
                default:
                    int maxCount = Mathf.Max(hardList.Count, medList.Count);
                    for (int i = 0; i < maxCount; i++)
                    {
                        if (i < hardList.Count) result.Add(hardList[i].raw);
                        if (i < medList.Count) result.Add(medList[i].raw);
                    }
                    easyList.ForEach(x => result.Add(x.raw));
                    break;
            }

            return result;
        }

        private static List<PhysicsQuestionRaw> FilterByTeacherStrategyRaw(List<PhysicsQuestionRaw> pool, string teacherId)
        {
            List<PhysicsQuestionRaw> easyList = new List<PhysicsQuestionRaw>();
            List<PhysicsQuestionRaw> medList = new List<PhysicsQuestionRaw>();
            List<PhysicsQuestionRaw> hardList = new List<PhysicsQuestionRaw>();

            foreach (var q in pool)
            {
                if (string.Equals(q.difficulty, "Easy", StringComparison.OrdinalIgnoreCase))
                    easyList.Add(q);
                else if (string.Equals(q.difficulty, "Medium", StringComparison.OrdinalIgnoreCase))
                    medList.Add(q);
                else
                    hardList.Add(q);
            }

            ShuffleList(easyList);
            ShuffleList(medList);
            ShuffleList(hardList);

            List<PhysicsQuestionRaw> result = new List<PhysicsQuestionRaw>();

            switch (teacherId)
            {
                case "Minh":
                    result.AddRange(easyList);
                    result.AddRange(medList);
                    result.AddRange(hardList);
                    break;

                case "Lan":
                    result.AddRange(medList);
                    result.AddRange(easyList);
                    result.AddRange(hardList);
                    break;

                case "Duy":
                    result.AddRange(hardList);
                    result.AddRange(medList);
                    result.AddRange(easyList);
                    break;

                case "Ha":
                default:
                    int maxCount = Mathf.Max(hardList.Count, medList.Count);
                    for (int i = 0; i < maxCount; i++)
                    {
                        if (i < hardList.Count) result.Add(hardList[i]);
                        if (i < medList.Count) result.Add(medList[i]);
                    }
                    result.AddRange(easyList);
                    break;
            }

            return result;
        }

        private static void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Lấy nội dung "Kiến thức cần nhớ / Mẹo ghi nhớ" cho câu hỏi:
        /// Ưu tiên keyKnowledge riêng của câu hỏi (nếu khác explanation),
        /// sau đó kết hợp Công thức cần nhớ (CongThucCanNho / formulas) của bài học từ PhysicsTheoryRepository.
        /// </summary>
        public static string ResolveKeyKnowledge(PhysicsQuestionRaw q, string lessonId)
        {
            if (q != null && !string.IsNullOrEmpty(q.keyKnowledge) &&
                !string.Equals(q.keyKnowledge.Trim(), (q.explanation ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return q.keyKnowledge.Trim();
            }

            PhysicsTheoryItem theory = PhysicsTheoryRepository.GetTheoryForLesson(lessonId);
            if (theory != null)
            {
                if (!string.IsNullOrEmpty(theory.formulas))
                {
                    return theory.formulas.Trim();
                }
                if (!string.IsNullOrEmpty(theory.keyPoints))
                {
                    return theory.keyPoints.Trim();
                }
            }

            if (q != null && !string.IsNullOrEmpty(q.keyKnowledge))
            {
                return q.keyKnowledge.Trim();
            }

            if (q != null && !string.IsNullOrEmpty(q.explanation))
            {
                return q.explanation.Trim();
            }

            return "";
        }

        /// <summary>
        /// Tra cứu "Kiến thức cần nhớ" riêng của từng câu hỏi trong màn hình Xem lại bài làm (Scene 20),
        /// dựa trên questionId hoặc questionText trong ngân hàng 830 câu hỏi.
        /// </summary>
        public static string ResolveKeyKnowledgeForAnswer(QuizAttemptAnswer answer, string lessonId)
        {
            if (answer == null) return "";

            EnsureLoaded();

            if (_cachedQuestions != null && _cachedQuestions.Count > 0)
            {
                if (!string.IsNullOrEmpty(answer.questionId))
                {
                    for (int i = 0; i < _cachedQuestions.Count; i++)
                    {
                        var q = _cachedQuestions[i];
                        if (q != null && string.Equals(q.questionID, answer.questionId, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(q.keyKnowledge))
                                return q.keyKnowledge.Trim();
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(answer.questionText))
                {
                    string targetText = answer.questionText.Trim();
                    for (int i = 0; i < _cachedQuestions.Count; i++)
                    {
                        var q = _cachedQuestions[i];
                        if (q != null && !string.IsNullOrEmpty(q.questionText) &&
                            string.Equals(q.questionText.Trim(), targetText, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(q.keyKnowledge))
                                return q.keyKnowledge.Trim();
                            break;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(answer.keyKnowledge))
            {
                return answer.keyKnowledge.Trim();
            }

            return ResolveKeyKnowledge(null, lessonId);
        }
    }
}
