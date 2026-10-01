// ============================================================
// BiologyQuestionBank.cs
// Ngân hàng câu hỏi - CHỈ NẠP TỪ GOOGLE SHEET (CSV) + cache offline
//
// HỖ TRỢ 2 FORMAT CSV (tự nhận biết theo dòng tiêu đề):
//
// (A) FORM GIÁO VIÊN - đọc theo TÊN CỘT, thứ tự cột tùy ý, thừa/ thiếu cột không sao:
//     CauHoiID | BaiID | MucDo | NoiDung | HinhAnh | DapAnA | DapAnB | DapAnC | DapAnD |
//     DapAnDung | GiaiThich | MeoGhiNho | TrangThai | GiaoVienID | LoaiCauHoi |
//     DapAnTuLuanMau | NguonTao | NgayTao
//     → ChapterID/ChapterName/LessonName tự suy từ BaiID (B36..B51 → C11..C14)
//     → MucDo: De/Dễ, TrungBinh/Trung bình, Kho/Khó, Boss | TrangThai: 1/TRUE = bật
//     → LoaiCauHoi: TracNghiem hoặc TuLuan (EssayImage)
//
// (B) FORMAT CŨ 19 CỘT (vẫn tương thích):
//     0: QuestionID | 1: Subject | 2: ChapterID | 3: ChapterName | 4: LessonID | 5: LessonName |
//     6: QuestionType | 7: Difficulty | 8: QuestionText | 9: AnswerA | 10: AnswerB | 11: AnswerC |
//     12: AnswerD | 13: CorrectAnswer | 14: ModelAnswer | 15: Rubric | 16: MaxScore | 17: Explanation | 18: Enabled
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

            // CHỈ dùng câu hỏi từ Google Sheet: cache offline nếu từng tải, KHÔNG còn câu mẫu trong code
            if (!LoadCache())
            {
                totalQuestions = 0;
                Debug.LogWarning("[BiologyQuestionBank] Chưa có câu hỏi (chưa từng tải Google Sheet). Nhập link Sheet vào BiologyBootstrap rồi chơi online 1 lần để nạp.");
            }
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(googleSheetCsvUrl))
            {
                Debug.LogWarning("[BiologyQuestionBank] ⚠ CHƯA NHẬP GOOGLE SHEET CSV URL! Dán link vào Main Camera → BiologyBootstrap → Google Sheet Csv Url (scene SinhHoc).");
                return;
            }
            if (fetchOnStart)
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
                        Debug.Log($"[BiologyQuestionBank] ✓ Đã tải {count} câu hỏi từ Google Sheet (đã lưu cache offline).");
                    }
                    else
                    {
                        Debug.LogWarning("[BiologyQuestionBank] Tải được dữ liệu nhưng 0 câu hợp lệ! Kiểm tra Sheet: phải có dòng tiêu đề (CauHoiID, BaiID, MucDo, NoiDung, DapAnA...) và dữ liệu bên dưới.");
                    }
                    OnQuestionsChanged?.Invoke();
                }
            }
        }

        // ----------------- PHÂN TÍCH CSV -----------------
        public int ParseCsv(string csv)
        {
            string[] lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Tự nhận format: có dòng tiêu đề → đọc theo TÊN CỘT; không có → format cũ 19 cột
            int headerIndex = FindHeaderLine(lines);
            int skipped = 0;
            List<BiologyQuestion> parsed = headerIndex >= 0
                ? ParseWithHeader(lines, headerIndex, out skipped)
                : ParseLegacy19Cols(lines, out skipped);

            if (skipped > 0)
                Debug.LogWarning($"[BiologyQuestionBank] Bỏ qua {skipped} dòng không hợp lệ (thiếu BaiID/NoiDung/đáp án A).");

            if (parsed.Count > 0)
            {
                _allQuestions = parsed;
                totalQuestions = parsed.Count;
            }
            return parsed.Count;
        }

        /// <summary>Tìm dòng tiêu đề: dòng chứa "CauHoiID" hoặc "QuestionID" (đã chuẩn hóa).</summary>
        private static int FindHeaderLine(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string n = NormalizeKey(lines[i]);
                if (n.Contains("cauhoiid") || n.Contains("questionid")) return i;
            }
            return -1;
        }

        // Tên field -> các tên cột có thể có (đã chuẩn hóa: chữ thường, không dấu, không cách)
        private static readonly Dictionary<string, string[]> HeaderAliases = new Dictionary<string, string[]>
        {
            { "questionID",   new[] { "cauhoiid", "questionid", "macauhoi" } },
            { "subject",      new[] { "subject", "mon", "monhoc" } },
            { "chapterID",    new[] { "chapterid", "machuong", "chuongid" } },
            { "chapterName",  new[] { "chaptername", "tenchuong", "chuongten" } },
            { "lessonID",     new[] { "baiid", "lessonid", "mabai" } },
            { "lessonName",   new[] { "lessonname", "tenbai", "baiten" } },
            { "questionType", new[] { "loaicauhoi", "questiontype" } },
            { "difficulty",   new[] { "mucdo", "difficulty" } },
            { "questionText", new[] { "noidung", "questiontext", "cauhoi" } },
            { "answerA",      new[] { "dapana", "answera" } },
            { "answerB",      new[] { "dapanb", "answerb" } },
            { "answerC",      new[] { "dapanc", "answerc" } },
            { "answerD",      new[] { "dapand", "answerd" } },
            { "correct",      new[] { "dapandung", "correctanswer" } },
            { "modelAnswer",  new[] { "dapantuluanmau", "modelanswer" } },
            { "rubric",       new[] { "rubric", "tieuchi" } },
            { "maxScore",     new[] { "maxscore", "diemtoida" } },
            { "explanation",  new[] { "giaithich", "explanation" } },
            { "memoryTip",    new[] { "meoghinho" } },
            { "enabled",      new[] { "trangthai", "enabled", "kichhoat" } },
        };

        private static List<BiologyQuestion> ParseWithHeader(string[] lines, int headerIndex, out int skipped)
        {
            skipped = 0;
            List<BiologyQuestion> parsed = new List<BiologyQuestion>();
            List<string> headers = SplitCsvLine(lines[headerIndex]);

            // Map: index cột -> tên field
            var colField = new string[headers.Count];
            for (int c = 0; c < headers.Count; c++)
            {
                string h = NormalizeKey(headers[c]);
                if (string.IsNullOrEmpty(h)) continue;
                foreach (var kv in HeaderAliases)
                {
                    if (Array.IndexOf(kv.Value, h) >= 0) { colField[c] = kv.Key; break; }
                }
            }

            for (int i = headerIndex + 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                List<string> cols = SplitCsvLine(line);
                var row = new Dictionary<string, string>();
                for (int c = 0; c < cols.Count && c < colField.Length; c++)
                {
                    if (colField[c] == null) continue;
                    if (!row.ContainsKey(colField[c])) row[colField[c]] = cols[c].Trim();
                }
                if (row.Count == 0) { skipped++; continue; }

                BiologyQuestion q = BuildQuestionFromRow(row);
                if (q != null) parsed.Add(q);
                else skipped++;
            }
            return parsed;
        }

        /// <summary>Dựng câu hỏi từ 1 dòng dữ liệu đã map theo tên cột. Trả null nếu dòng vô dụng.</summary>
        private static BiologyQuestion BuildQuestionFromRow(Dictionary<string, string> row)
        {
            row.TryGetValue("questionText", out string text);
            if (string.IsNullOrEmpty(text)) return null; // bắt buộc có nội dung câu hỏi

            row.TryGetValue("lessonID", out string lessonID);
            if (string.IsNullOrEmpty(lessonID)) return null; // bắt buộc có BaiID để game lọc bài

            BiologyQuestion q = new BiologyQuestion();

            row.TryGetValue("questionID", out string qid);
            q.questionID = string.IsNullOrEmpty(qid) ? "Q_" + Guid.NewGuid().ToString("N").Substring(0, 8) : qid;

            // Môn: sheet không có cột Subject → mặc định Biology
            row.TryGetValue("subject", out string subject);
            q.subject = string.IsNullOrEmpty(subject) ? "Biology" : subject;

            q.lessonID = lessonID.Trim().ToUpperInvariant();

            row.TryGetValue("lessonName", out string lessonName);
            row.TryGetValue("chapterID", out string chapterID);
            row.TryGetValue("chapterName", out string chapterName);
            FillChapterInfo(q, chapterID, chapterName, lessonName);

            row.TryGetValue("questionType", out string qType);
            string qTypeNorm = NormalizeKey(qType);
            q.questionType = (qTypeNorm.Contains("tuluan") || qTypeNorm.Contains("essay"))
                ? QuestionType.EssayImage : QuestionType.MultipleChoice;

            row.TryGetValue("difficulty", out string diff);
            q.difficulty = ParseLevel(diff);
            q.questionText = text;

            row.TryGetValue("answerA", out string a);
            row.TryGetValue("answerB", out string b);
            row.TryGetValue("answerC", out string c);
            row.TryGetValue("answerD", out string d);
            q.answerA = a ?? ""; q.answerB = b ?? ""; q.answerC = c ?? ""; q.answerD = d ?? "";

            row.TryGetValue("correct", out string correct);
            q.correctIndex = ParseCorrect(correct);

            // Dữ liệu tự luận (nếu có)
            row.TryGetValue("modelAnswer", out string model);
            q.modelAnswer = model ?? "";
            row.TryGetValue("rubric", out string rubric);
            q.rubric = rubric ?? "";
            row.TryGetValue("maxScore", out string maxScore);
            float.TryParse(maxScore, out q.maxScore);
            if (q.maxScore <= 0f && q.questionType == QuestionType.EssayImage) q.maxScore = 10f;

            // Giải thích + mẹo ghi nhớ (gộp vào Explanation để hiện cùng chỗ)
            row.TryGetValue("explanation", out string explain);
            row.TryGetValue("memoryTip", out string tip);
            q.explanation = CombineExplanation(explain, tip);

            // TrangThai: trống/1/TRUE/HoatDong... = bật; 0/FALSE/An/Tat... = tắt
            row.TryGetValue("enabled", out string enabled);
            q.enabled = ParseEnabled(enabled);

            // Trắc nghiệm phải có ít nhất đáp án A
            if (q.questionType == QuestionType.MultipleChoice && string.IsNullOrEmpty(q.answerA)) return null;
            return q;
        }

        /// <summary>Suy ChapterID/ChapterName/LessonName từ BaiID (B36..B51) khi sheet không có các cột này.</summary>
        private static void FillChapterInfo(BiologyQuestion q, string chapterID, string chapterName, string lessonName)
        {
            q.chapterID = (chapterID ?? "").Trim();
            q.chapterName = chapterName ?? "";
            q.lessonName = lessonName ?? "";

            // Tra CurriculumCatalog trước (đúng nhất)
            foreach (var ch in CurriculumCatalog.GetChapters("Biology"))
            {
                var ls = ch.GetLesson(q.lessonID);
                if (ls == null) continue;
                if (string.IsNullOrEmpty(q.chapterID)) q.chapterID = ch.chapterID;
                if (string.IsNullOrEmpty(q.chapterName)) q.chapterName = ch.chapterName;
                if (string.IsNullOrEmpty(q.lessonName)) q.lessonName = ls.lessonName;
                return;
            }

            // Fallback: suy chương theo khoảng bài
            if (string.IsNullOrEmpty(q.chapterID)) q.chapterID = ChapterFromLesson(q.lessonID);
            if (string.IsNullOrEmpty(q.lessonName)) q.lessonName = q.lessonID;
        }

        private static string ChapterFromLesson(string lessonID)
        {
            if (string.IsNullOrEmpty(lessonID)) return "";
            if (!int.TryParse(lessonID.Trim().TrimStart('B', 'b'), out int n)) return "";
            if (n >= 36 && n <= 41) return "C11";
            if (n >= 42 && n <= 46) return "C12";
            if (n >= 47 && n <= 48) return "C13";
            if (n >= 49 && n <= 51) return "C14";
            return "";
        }

        private static string CombineExplanation(string explain, string tip)
        {
            string gt = (explain ?? "").Trim();
            string meo = (tip ?? "").Trim();
            if (string.IsNullOrEmpty(meo)) return gt;
            if (string.IsNullOrEmpty(gt)) return "💡 Mẹo ghi nhớ: " + meo;
            return gt + "\n💡 Mẹo ghi nhớ: " + meo;
        }

        /// <summary>Chữ thường + bỏ dấu tiếng Việt + bỏ khoảng trắng/underscore — dùng so khớp header & giá trị.</summary>
        private static string NormalizeKey(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            string formD = s.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(formD.Length);
            foreach (char c in formD)
            {
                if (c == ' ' || c == '_' || c == '\t') continue;
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat == System.Globalization.UnicodeCategory.NonSpacingMark || cat == System.Globalization.UnicodeCategory.Format) continue;
                sb.Append(c);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        /// <summary>Format cũ 19 cột cố định (QuestionID..Enabled) — giữ để tương thích cache/link sheet cũ.</summary>
        private static List<BiologyQuestion> ParseLegacy19Cols(string[] lines, out int skipped)
        {
            skipped = 0;
            List<BiologyQuestion> parsed = new List<BiologyQuestion>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                List<string> cols = SplitCsvLine(line);
                if (cols.Count < 19) { skipped++; continue; } // Phải đủ 19 cột
                if (cols[0].Trim().Equals("QuestionID", StringComparison.OrdinalIgnoreCase)) continue; // Header

                BiologyQuestion q = new BiologyQuestion();
                q.questionID = cols[0].Trim();
                q.subject = cols[1].Trim();
                q.chapterID = cols[2].Trim();
                q.chapterName = cols[3].Trim();
                q.lessonID = cols[4].Trim();
                q.lessonName = cols[5].Trim();
                q.questionType = cols[6].Trim().Equals("EssayImage", StringComparison.OrdinalIgnoreCase) ? QuestionType.EssayImage : QuestionType.MultipleChoice;
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
                q.enabled = ParseEnabled(cols[18]);

                if (!q.enabled) continue;
                parsed.Add(q);
            }
            return parsed;
        }

        private static BiologyDifficulty ParseLevel(string s)
        {
            string n = NormalizeKey(s);
            if (n.Contains("kho") || n.Contains("hard") || n == "3") return BiologyDifficulty.Kho;
            if (n.Contains("boss") || n == "4") return BiologyDifficulty.Boss;
            if (n.Contains("de") || n.Contains("easy") || n == "1") return BiologyDifficulty.De;
            return BiologyDifficulty.TrungBinh;
        }

        private static int ParseCorrect(string s)
        {
            s = (s ?? "").Trim();
            if (s.Length == 0) return 0;
            switch (char.ToUpperInvariant(s[0]))
            {
                case 'A': case '1': return 0;
                case 'B': case '2': return 1;
                case 'C': case '3': return 2;
                case 'D': case '4': return 3;
                default: return 0;
            }
        }

        /// <summary>TrangThai/Enabled: trống = bật; 0/FALSE/An/Tat/Off/No... = tắt.</summary>
        private static bool ParseEnabled(string s)
        {
            string n = NormalizeKey(s);
            if (string.IsNullOrEmpty(n)) return true;
            if (n == "0" || n == "false" || n == "an" || n == "tat" || n == "off" || n == "no" || n.Contains("ngung") || n.Contains("voanh")) return false;
            return true;
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
    }
}
