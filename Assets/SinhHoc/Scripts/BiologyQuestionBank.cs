// ============================================================
// BiologyQuestionBank.cs
// Ngân hàng câu hỏi SINH HỌC - tải từ Google Sheet (CSV)
//
// FORMAT MỚI (19 CỘT):
// 0: QuestionID | 1: Subject | 2: ChapterID | 3: ChapterName | 4: LessonID | 5: LessonName | 
// 6: QuestionType | 7: Difficulty | 8: QuestionText | 9: AnswerA | 10: AnswerB | 11: AnswerC | 
// 12: AnswerD | 13: CorrectAnswer | 14: ModelAnswer | 15: Rubric | 16: MaxScore | 17: Explanation | 18: Enabled
// ============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace PRU.Biology
{
    public enum BiologyDifficulty
    {
        De = 0,
        TrungBinh = 1,
        Kho = 2,
        Boss = 3
    }

    [Serializable]
    public class BiologyQuestion
    {
        public string questionID;
        public string subject;
        public string chapterID;
        public string chapterName;
        public string lessonID;
        public string lessonName;
        public QuestionType questionType;
        public BiologyDifficulty difficulty;
        [TextArea(2, 4)] public string questionText;
        
        // Trắc nghiệm
        public string answerA;
        public string answerB;
        public string answerC;
        public string answerD;
        public int correctIndex; // 0=A, 1=B, 2=C, 3=D
        
        // Tự luận
        [TextArea(2, 4)] public string modelAnswer;
        [TextArea(2, 4)] public string rubric;
        public float maxScore;
        
        public string explanation;
        public bool enabled;
    }

    public class BiologyQuestionBank : MonoBehaviour
    {
        public static BiologyQuestionBank Instance { get; private set; }

        [Header("Google Sheet CSV URL")]
        [TextArea(2, 3)] public string googleSheetCsvUrl = "";
        public bool fetchOnStart = true;

        [Header("Trạng thái")]
        public int totalQuestions;
        public bool isLoadedFromSheet;

        // Lưu toàn bộ câu hỏi tải về
        private List<BiologyQuestion> _allQuestions = new List<BiologyQuestion>();
        
        // Pool đã lọc cho Session hiện tại
        private readonly Dictionary<BiologyDifficulty, List<BiologyQuestion>> _sessionPool = new Dictionary<BiologyDifficulty, List<BiologyQuestion>>();
        private readonly Dictionary<BiologyDifficulty, List<int>> _unusedIndices = new Dictionary<BiologyDifficulty, List<int>>();
        private readonly Dictionary<BiologyDifficulty, int[]> _stats = new Dictionary<BiologyDifficulty, int[]>();

        private static string CachePath => Path.Combine(Application.persistentDataPath, "sinhhoc_questions_v2_cache.csv");

        public event Action OnQuestionsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!LoadCache()) AddDefaultQuestions();
        }

        private void Start()
        {
            if (fetchOnStart && !string.IsNullOrEmpty(googleSheetCsvUrl))
            {
                StartCoroutine(FetchFromGoogleSheet(googleSheetCsvUrl));
            }
        }

        // ----------------- LỌC THEO GAME SESSION -----------------
        public void BuildPoolForCurrentSession()
        {
            _sessionPool.Clear();
            _unusedIndices.Clear();
            _stats.Clear();

            string reqSubject = GameSessionData.SelectedSubject;
            string reqChapter = GameSessionData.SelectedChapterID;
            string reqLesson = GameSessionData.SelectedLessonID;
            QuestionType reqType = GameSessionData.SelectedQuestionType;

            int count = 0;
            foreach (var q in _allQuestions)
            {
                if (!q.enabled) continue;
                if (!string.IsNullOrEmpty(reqSubject) && !q.subject.Equals(reqSubject, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(reqChapter) && !q.chapterID.Equals(reqChapter, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(reqLesson) && !q.lessonID.Equals(reqLesson, StringComparison.OrdinalIgnoreCase)) continue;
                if (q.questionType != reqType) continue;

                if (!_sessionPool.ContainsKey(q.difficulty))
                {
                    _sessionPool[q.difficulty] = new List<BiologyQuestion>();
                }
                _sessionPool[q.difficulty].Add(q);
                count++;
            }

            RebuildUnused();
            Debug.Log($"[BiologyQuestionBank] Đã tạo pool cho session: {reqSubject} - {reqChapter} - {reqLesson} - {reqType}. Tổng cộng {count} câu.");
        }

        // ----------------- API -----------------
        public bool HasQuestions(BiologyDifficulty level)
        {
            return _sessionPool.TryGetValue(level, out var list) && list.Count > 0;
        }

        public BiologyQuestion GetRandomQuestion(BiologyDifficulty level)
        {
            if (!HasQuestions(level)) return null;

            List<int> pool = _unusedIndices[level];
            if (pool.Count == 0) RebuildUnused(level);

            int idx = pool[UnityEngine.Random.Range(0, pool.Count)];
            pool.Remove(idx);
            return _sessionPool[level][idx];
        }

        /// <summary>
        /// Lấy câu hỏi theo mức ưu tiên, TỰ FALLBACK sang mức khó gần nhất khi hết (#9).
        /// VD: đầu trận xin Easy nhưng bài chỉ có Medium → trả Medium thay vì bỏ trống.
        /// Chỉ trả null khi toàn bộ pool session rỗng (bài này không có câu nào).
        /// </summary>
        public BiologyQuestion GetRandomQuestionWithFallback(BiologyDifficulty preferred)
        {
            if (_sessionPool.Count == 0) return null;
            if (HasQuestions(preferred)) return GetRandomQuestion(preferred);

            // Thử các mức còn lại theo khoảng cách gần mức ưu tiên trước
            List<BiologyDifficulty> order = new List<BiologyDifficulty>();
            foreach (BiologyDifficulty lv in Enum.GetValues(typeof(BiologyDifficulty)))
                if (lv != preferred) order.Add(lv);
            order.Sort((a, b) =>
                Mathf.Abs((int)a - (int)preferred).CompareTo(Mathf.Abs((int)b - (int)preferred)));

            foreach (BiologyDifficulty lv in order)
            {
                if (HasQuestions(lv))
                {
                    Debug.Log($"[BiologyQuestionBank] Hết câu mức {preferred} → fallback sang {lv} (#9).");
                    return GetRandomQuestion(lv);
                }
            }
            return null;
        }

        public void RecordAnswer(BiologyDifficulty level, bool correct)
        {
            if (!_stats.TryGetValue(level, out var arr)) _stats[level] = new int[2];
            _stats[level][correct ? 0 : 1]++;
        }

        public int TotalCorrect()
        {
            int sum = 0;
            foreach (var arr in _stats.Values) sum += arr[0];
            return sum;
        }

        public int TotalWrong()
        {
            int sum = 0;
            foreach (var arr in _stats.Values) sum += arr[1];
            return sum;
        }

        // ----------------- TẢI GOOGLE SHEET -----------------
        public IEnumerator FetchFromGoogleSheet(string csvUrl)
        {
            Debug.Log("[BiologyQuestionBank] Đang tải...");
            using (UnityWebRequest www = UnityWebRequest.Get(csvUrl))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    int count = ParseCsv(www.downloadHandler.text);
                    if (count > 0)
                    {
                        isLoadedFromSheet = true;
                        SaveCache(www.downloadHandler.text);
                        BuildPoolForCurrentSession();
                    }
                    OnQuestionsChanged?.Invoke();
                }
            }
        }

        public int ParseCsv(string csv)
        {
            List<BiologyQuestion> parsed = new List<BiologyQuestion>();
            string[] lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                List<string> cols = SplitCsvLine(line);
                if (cols.Count < 19) continue; // Phải đủ 19 cột

                if (cols[0].Trim().Equals("QuestionID", StringComparison.OrdinalIgnoreCase)) continue; // Header

                BiologyQuestion q = new BiologyQuestion();
                q.questionID = cols[0].Trim();
                q.subject = cols[1].Trim();
                q.chapterID = cols[2].Trim();
                q.chapterName = cols[3].Trim();
                q.lessonID = cols[4].Trim();
                q.lessonName = cols[5].Trim();
                
                string qTypeStr = cols[6].Trim();
                q.questionType = qTypeStr.Equals("EssayImage", StringComparison.OrdinalIgnoreCase) ? QuestionType.EssayImage : QuestionType.MultipleChoice;
                
                q.difficulty = ParseLevel(cols[7]);
                q.questionText = cols[8].Trim();
                
                q.answerA = cols[9].Trim();
                q.answerB = cols[10].Trim();
                q.answerC = cols[11].Trim();
                q.answerD = cols[12].Trim();
                q.correctIndex = ParseCorrect(cols[13]);
                
                q.modelAnswer = cols[14].Trim();
                q.rubric = cols[15].Trim();
                float.TryParse(cols[16].Trim(), out q.maxScore);
                
                q.explanation = cols[17].Trim();
                q.enabled = cols[18].Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);

                if (!q.enabled) continue;
                parsed.Add(q);
            }

            if (parsed.Count > 0)
            {
                _allQuestions = parsed;
                totalQuestions = parsed.Count;
            }
            return parsed.Count;
        }

        private BiologyDifficulty ParseLevel(string s)
        {
            s = s.Trim().ToLowerInvariant();
            if (s.Contains("de") || s.Contains("easy")) return BiologyDifficulty.De;
            if (s.Contains("kho") || s.Contains("hard")) return BiologyDifficulty.Kho;
            if (s.Contains("boss")) return BiologyDifficulty.Boss;
            return BiologyDifficulty.TrungBinh;
        }

        private int ParseCorrect(string s)
        {
            s = s.Trim().ToUpperInvariant();
            if (s == "A") return 0;
            if (s == "B") return 1;
            if (s == "C") return 2;
            if (s == "D") return 3;
            return 0;
        }

        private static List<string> SplitCsvLine(string line)
        {
            List<string> result = new List<string>();
            System.Text.StringBuilder cur = new System.Text.StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cur.Append('"');
                        i++;
                    }
                    else inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(cur.ToString());
                    cur.Length = 0;
                }
                else cur.Append(c);
            }
            result.Add(cur.ToString());
            return result;
        }

        private void SaveCache(string csv) { try { File.WriteAllText(CachePath, csv); } catch { } }
        private bool LoadCache()
        {
            try
            {
                if (!File.Exists(CachePath)) return false;
                int count = ParseCsv(File.ReadAllText(CachePath));
                if (count > 0)
                {
                    isLoadedFromSheet = true;
                    BuildPoolForCurrentSession();
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void RebuildUnused(BiologyDifficulty level)
        {
            List<int> all = new List<int>();
            for (int i = 0; i < _sessionPool[level].Count; i++) all.Add(i);
            _unusedIndices[level] = all;
        }

        private void RebuildUnused()
        {
            foreach (BiologyDifficulty lv in Enum.GetValues(typeof(BiologyDifficulty)))
            {
                if (_sessionPool.TryGetValue(lv, out var list) && list.Count > 0)
                    RebuildUnused(lv);
                else
                    _unusedIndices[lv] = new List<int>();
            }
        }

        // DEFAULT CÂU HỎI MẪU CHO TEST CHUẨN MỚI
        private void AddDefaultQuestions()
        {
            _allQuestions.Clear();
            // Thêm 1 câu trắc nghiệm mẫu
            _allQuestions.Add(new BiologyQuestion
            {
                questionID = "Q_TEST_1",
                subject = "Biology",
                chapterID = "C11",
                chapterName = "Di truyền học",
                lessonID = "B39",
                lessonName = "Tái bản DNA",
                questionType = QuestionType.MultipleChoice,
                difficulty = BiologyDifficulty.De,
                questionText = "Cơ thể sống được cấu tạo từ đơn vị nào?",
                answerA = "Tế bào", answerB = "Nguyên tử", answerC = "Nước", answerD = "Quả đá",
                correctIndex = 0,
                explanation = "Tế bào là đơn vị cơ bản.",
                enabled = true
            });
            // Thêm 1 câu tự luận mẫu
            _allQuestions.Add(new BiologyQuestion
            {
                questionID = "Q_TEST_2",
                subject = "Biology",
                chapterID = "C12",
                chapterName = "Di truyền NST",
                lessonID = "B43",
                lessonName = "Nguyên phân",
                questionType = QuestionType.EssayImage,
                difficulty = BiologyDifficulty.TrungBinh,
                questionText = "Trình bày các kỳ của nguyên phân.",
                modelAnswer = "Kỳ đầu, kỳ giữa, kỳ sau, kỳ cuối.",
                rubric = "Nêu đúng tên 4 kỳ = 10 điểm",
                maxScore = 10f,
                explanation = "Có 4 kỳ chính.",
                enabled = true
            });

            totalQuestions = _allQuestions.Count;
            BuildPoolForCurrentSession();
        }
    }
}
