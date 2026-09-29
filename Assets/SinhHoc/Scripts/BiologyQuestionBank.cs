// ============================================================
// BiologyQuestionBank.cs
// Ngân hàng câu hỏi SINH HỌC - tải từ Google Sheet (CSV),
// dùng CACHE offline khi mất mạng, hoặc câu hỏi mẫu có sẵn.
//
// ĐỊNH DẠNG GOOGLE SHEET (mỗi dòng 1 câu, dòng đầu là tiêu đề):
//   A: Level     -> de | trungbinh | kho | boss
//   B: Question  -> Nội dung câu hỏi
//   C: A         -> Đáp án ô A
//   D: B         -> Đáp án ô B
//   E: C         -> Đáp án ô C
//   F: D         -> Đáp án ô D
//   G: Correct   -> Ký tự đáp án đúng: A / B / C / D
//   H: Explain   -> (TUỲ CHỌN) Giải thích đáp án - hiện khi trả lời sai
// HƯỚNG DẪN LẤY LINK CSV: xem file README trong Assets/SinhHoc/Docs
// ============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace PRU.Biology
{
    // Độ khó của câu hỏi, trùng với cột "Level" trong Sheet
    public enum BiologyDifficulty
    {
        De = 0,          // Dễ
        TrungBinh = 1,   // Trung bình
        Kho = 2,         // Khó
        Boss = 3         // Dành riêng cho boss (Knowledge Clash)
    }

    [Serializable]
    public class BiologyQuestion
    {
        public BiologyDifficulty level;
        [TextArea(2, 4)] public string question;
        public string answerA;
        public string answerB;
        public string answerC;
        public string answerD;
        public int correctIndex;   // 0=A, 1=B, 2=C, 3=D
        public string explanation; // giải thích hiện khi trả lời sai
    }

    public class BiologyQuestionBank : MonoBehaviour
    {
        public static BiologyQuestionBank Instance { get; private set; }

        [Header("Google Sheet CSV URL")]
        [Tooltip("Dán link Google Sheet đã xuất CSV vào đây (xem README).")]
        [TextArea(2, 3)] public string googleSheetCsvUrl = "";

        [Header("Cài đặt tải")]
        [Tooltip("Tự tải Sheet khi vào game. Bỏ tick nếu chỉ muốn dùng câu hỏi mẫu.")]
        public bool fetchOnStart = true;

        [Header("Trạng thái (đọc để debug)")]
        public int totalQuestions;
        public bool isLoadedFromSheet;

        private readonly Dictionary<BiologyDifficulty, List<BiologyQuestion>> _questions =
            new Dictionary<BiologyDifficulty, List<BiologyQuestion>>();
        private readonly Dictionary<BiologyDifficulty, List<int>> _unused =
            new Dictionary<BiologyDifficulty, List<int>>();

        // Thống kê đúng/sai theo độ khó (cho màn kết quả)
        private readonly Dictionary<BiologyDifficulty, int[]> _stats = new Dictionary<BiologyDifficulty, int[]>();

        private static string CachePath =>
            Path.Combine(Application.persistentDataPath, "sinhhoc_questions_cache.csv");

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

            // Ưu tiên cache offline (kết quả lần tải Sheet gần nhất); không có thì dùng câu mẫu
            if (!LoadCache()) AddDefaultQuestions();

            RebuildUnused();
            totalQuestions = CountAll();
        }

        private void Start()
        {
            if (fetchOnStart && !string.IsNullOrEmpty(googleSheetCsvUrl))
            {
                StartCoroutine(FetchFromGoogleSheet(googleSheetCsvUrl));
            }
            else
            {
                Debug.Log("[SinhHoc][QuestionBank] Chưa gán link Google Sheet - dùng bộ câu hỏi " +
                          (isLoadedFromSheet ? "cache offline." : "mẫu trong code."));
            }
        }

        // ----------------- API dùng bởi các script khác -----------------

        public bool HasQuestions(BiologyDifficulty level)
        {
            return _questions.TryGetValue(level, out var list) && list.Count > 0;
        }

        /// <summary>Lấy 1 câu hỏi ngẫu nhiên chưa lặp của mức cho trước.</summary>
        public BiologyQuestion GetRandomQuestion(BiologyDifficulty level)
        {
            if (!HasQuestions(level)) return null;

            List<int> pool = _unused[level];
            if (pool.Count == 0) RebuildUnused(level); // Hết câu thì xáo lại từ đầu

            int idx = pool[UnityEngine.Random.Range(0, pool.Count)];
            pool.Remove(idx);
            return _questions[level][idx];
        }

        public int Count(BiologyDifficulty level)
        {
            return _questions.TryGetValue(level, out var list) ? list.Count : 0;
        }

        public void SetCsvUrlAndFetch(string url)
        {
            googleSheetCsvUrl = url;
            if (!string.IsNullOrEmpty(url))
                StartCoroutine(FetchFromGoogleSheet(url));
        }

        // ----------------- Thống kê (màn kết quả) -----------------

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

        public int CorrectOf(BiologyDifficulty level)
        {
            return _stats.TryGetValue(level, out var arr) ? arr[0] : 0;
        }

        public int WrongOf(BiologyDifficulty level)
        {
            return _stats.TryGetValue(level, out var arr) ? arr[1] : 0;
        }

        // ----------------- Tải Google Sheet -----------------

        public IEnumerator FetchFromGoogleSheet(string csvUrl)
        {
            Debug.Log("[SinhHoc][QuestionBank] Đang tải câu hỏi từ Google Sheet...");
            using (UnityWebRequest www = UnityWebRequest.Get(csvUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    int count = ParseCsv(www.downloadHandler.text);
                    if (count > 0)
                    {
                        isLoadedFromSheet = true;
                        SaveCache(www.downloadHandler.text); // cache lại để chơi offline
                        Debug.Log($"[SinhHoc][QuestionBank] ✓ Đã tải {count} câu hỏi từ Sheet! " +
                                  $"(Tổng: {CountAll()}) - đã lưu cache offline.");
                    }
                    else
                    {
                        Debug.LogWarning("[SinhHoc][QuestionBank] ⚠ Sheet tải về không có dòng hợp lệ - giữ nguyên dữ liệu hiện tại.");
                    }
                    OnQuestionsChanged?.Invoke();
                }
                else
                {
                    Debug.LogWarning($"[SinhHoc][QuestionBank] ⚠ Lỗi tải Sheet: {www.error}. " +
                                     "Dùng cache offline / câu hỏi mẫu. Game vẫn chơi được bình thường.");
                }
            }
        }

        /// <summary>
        /// Đọc CSV: Level,Question,A,B,C,D,Correct[,Explain]
        /// Hỗ trợ chuỗi có dấu ngoặc kép và dấu phẩy bên trong.
        /// </summary>
        public int ParseCsv(string csv)
        {
            Dictionary<BiologyDifficulty, List<BiologyQuestion>> parsed = new Dictionary<BiologyDifficulty, List<BiologyQuestion>>();
            int added = 0;
            string[] lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                List<string> cols = SplitCsvLine(line);
                if (cols.Count < 7)
                {
                    Debug.LogWarning($"[SinhHoc][QuestionBank] Bỏ qua dòng {i + 1}: thiếu cột ({cols.Count}/7).");
                    continue;
                }

                // Bỏ qua dòng tiêu đề
                if (cols[0].Trim().Equals("Level", StringComparison.OrdinalIgnoreCase)) continue;

                BiologyQuestion q = new BiologyQuestion
                {
                    level = ParseLevel(cols[0]),
                    question = cols[1].Trim(),
                    answerA = cols[2].Trim(),
                    answerB = cols[3].Trim(),
                    answerC = cols[4].Trim(),
                    answerD = cols[5].Trim(),
                    correctIndex = ParseCorrect(cols[6]),
                    explanation = cols.Count >= 8 ? cols[7].Trim() : ""
                };

                // VALIDATE: bỏ qua row lỗi + cảnh báo để developer biết
                if (string.IsNullOrEmpty(q.question))
                {
                    Debug.LogWarning($"[SinhHoc][QuestionBank] Dòng {i + 1}: câu hỏi trống → bỏ qua.");
                    continue;
                }
                if (string.IsNullOrEmpty(q.answerA) || string.IsNullOrEmpty(q.answerB) ||
                    string.IsNullOrEmpty(q.answerC) || string.IsNullOrEmpty(q.answerD))
                {
                    Debug.LogWarning($"[SinhHoc][QuestionBank] Dòng {i + 1}: thiếu đáp án A/B/C/D → bỏ qua.");
                    continue;
                }
                if (q.correctIndex < 0)
                {
                    Debug.LogWarning($"[SinhHoc][QuestionBank] Dòng {i + 1}: Correct='{cols[6]}' không hợp lệ (phải là A/B/C/D) → bỏ qua.");
                    continue;
                }

                if (!parsed.ContainsKey(q.level)) parsed[q.level] = new List<BiologyQuestion>();
                parsed[q.level].Add(q);
                added++;
            }

            if (added > 0)
            {
                _questions.Clear();
                foreach (var kv in parsed) _questions[kv.Key] = kv.Value;
                RebuildUnused();
                totalQuestions = CountAll();
            }
            return added;
        }

        private static BiologyDifficulty ParseLevel(string s)
        {
            s = s.Trim().ToLowerInvariant();
            switch (s)
            {
                case "de":
                case "dễ":
                case "easy":
                    return BiologyDifficulty.De;
                case "trungbinh":
                case "trung bình":
                case "trung binh":
                case "medium":
                case "normal":
                    return BiologyDifficulty.TrungBinh;
                case "kho":
                case "khó":
                case "hard":
                    return BiologyDifficulty.Kho;
                default:
                    return BiologyDifficulty.Boss;
            }
        }

        private static int ParseCorrect(string s)
        {
            s = s.Trim().ToUpperInvariant();
            switch (s)
            {
                case "A": return 0;
                case "B": return 1;
                case "C": return 2;
                case "D": return 3;
                default:
                    if (int.TryParse(s, out int n) && n >= 1 && n <= 4) return n - 1;
                    return -1;
            }
        }

        /// <summary>Tách 1 dòng CSV hỗ trợ dấu "..." bao quanh dấu phẩy.</summary>
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

        // ----------------- CACHE OFFLINE -----------------

        private void SaveCache(string csv)
        {
            try
            {
                File.WriteAllText(CachePath, csv);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SinhHoc][QuestionBank] Không lưu được cache: {e.Message}");
            }
        }

        private bool LoadCache()
        {
            try
            {
                if (!File.Exists(CachePath)) return false;
                string csv = File.ReadAllText(CachePath);
                int count = ParseCsv(csv);
                if (count > 0)
                {
                    isLoadedFromSheet = true; // dữ liệu từng đến từ Sheet
                    Debug.Log($"[SinhHoc][QuestionBank] Đã nạp {count} câu hỏi từ cache offline ({CachePath}).");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SinhHoc][QuestionBank] Lỗi đọc cache: {e.Message}");
                return false;
            }
        }

        private void RebuildUnused(BiologyDifficulty level)
        {
            List<int> all = new List<int>();
            for (int i = 0; i < _questions[level].Count; i++) all.Add(i);
            _unused[level] = all;
        }

        private void RebuildUnused()
        {
            foreach (BiologyDifficulty lv in Enum.GetValues(typeof(BiologyDifficulty)))
            {
                if (_questions.TryGetValue(lv, out var list) && list.Count > 0)
                    RebuildUnused(lv);
                else
                    _unused[lv] = new List<int>();
            }
        }

        private int CountAll()
        {
            int sum = 0;
            foreach (var kv in _questions) sum += kv.Value.Count;
            return sum;
        }

        // ----------------- CÂU HỎI MẪU (offline fallback) -----------------
        // Dùng khi chưa từng tải được Sheet. Bạn có thể sửa/thêm trực tiếp tại đây.

        private void AddDefaultQuestions()
        {
            _questions.Clear();

            // ---- DỄ ----
            Add(BiologyDifficulty.De,
                "Cơ thể sống được cấu tạo từ đơn vị nào?",
                "Tế bào", "Nguyên tử", "Phân tử nước", "Quả đá", 0,
                "Tế bào là đơn vị cấu tạo và chức năng cơ bản của cơ thể sống.");
            Add(BiologyDifficulty.De,
                "Quá trình cây xanh hấp thụ khí gì để quang hợp?",
                "Oxi (O2)", "Cacbonic (CO2)", "Nitơ (N2)", "Hydrogen (H2)", 1,
                "Lá hút CO2 từ không khí, cộng nước và ánh sáng để tạo glucozo và thải O2.");
            Add(BiologyDifficulty.De,
                "Tim người có mấy ngăn?",
                "2 ngăn", "3 ngăn", "4 ngăn", "5 ngăn", 2,
                "Tim gồm 2 tâm nhĩ và 2 tâm thất, tổng cộng 4 ngăn.");
            Add(BiologyDifficulty.De,
                "Bộ nhiễm sắc thể của người bình thường có bao nhiêu NST?",
                "23", "46", "48", "64", 1,
                "Người có 23 cặp = 46 NST (22 cặp thường + 1 cặp giới tính).");
            Add(BiologyDifficulty.De,
                "Cây xanh thải ra khí gì sau khi quang hợp?",
                "CO2", "Metan", "Oxi", "Nitơ", 2,
                "Quang hợp thải khí Oxi (O2) ra môi trường.");
            Add(BiologyDifficulty.De,
                "Cây hút nước và muối khoáng từ đất bằng bộ phận nào?",
                "Lá", "Rễ", "Hoa", "Quả", 1,
                "Rễ (đặc biệt là lông rễ) hút nước và muối khoáng.");
            Add(BiologyDifficulty.De,
                "Cơ thể người cần khí nào để sống sót?",
                "Oxi", "Nitơ", "CO2", "Hiđrô", 0,
                "Oxi cần cho hô hấp tế bào, tạo năng lượng ATP.");
            Add(BiologyDifficulty.De,
                "Phổi là cơ quan của hệ nào?",
                "Tuần hoàn", "Tiêu hóa", "Hô hấp", "Thần kinh", 2,
                "Phổi là nơi trao đổi khí O2 - CO2, thuộc hệ hô hấp.");
            Add(BiologyDifficulty.De,
                "Dạ dày thuộc hệ cơ quan nào?",
                "Hô hấp", "Tiêu hóa", "Tuần hoàn", "Bài tiết", 1,
                "Dạ dày nghiền và tiết dịch vị tiêu hóa thức ăn.");
            Add(BiologyDifficulty.De,
                "Con cá thở dưới nước bằng cơ quan nào?",
                "Phổi", "Da", "Mang", "Vảy", 2,
                "Mang cá trích oxy hòa tan trong nước.");
            Add(BiologyDifficulty.De,
                "Sâu bọ (côn trùng) trưởng thành có mấy chân?",
                "4 chân", "6 chân", "8 chân", "10 chân", 1,
                "Côn trùng có 3 đôi chân = 6 chân (nhện là 8 chân, không phải côn trùng).");
            Add(BiologyDifficulty.De,
                "Nhị của hoa có chức năng gì?",
                "Tạo phấn hoa", "Đón phấn hoa", "Bảo vệ hoa", "Tạo mật", 0,
                "Nhị là cơ quan sinh dục đực, tạo hạt phấn chứa tinh.");
            Add(BiologyDifficulty.De,
                "Hạt nảy mầm cần điều kiện nào?",
                "Chỉ cần đất", "Nước, không khí và nhiệt độ thích hợp", "Chỉ cần ánh sáng mạnh", "Chỉ cần phân bón", 1,
                "Ba yếu tố cần: đủ nước, đủ không khí (oxy), nhiệt độ thích hợp.");
            Add(BiologyDifficulty.De,
                "Muỗi là côn trùng truyền bệnh gì phổ biến?",
                "Sốt xuất huyết", "Gãy xương", "Sâu răng", "Viêm da", 0,
                "Muỗi Aedes truyền virus Dengue gây sốt xuất huyết.");
            Add(BiologyDifficulty.De,
                "Vỏ cây có tác dụng gì?",
                "Bảo vệ thân cây", "Tạo hạt", "Hút nước", "Quang hợp", 0,
                "Vỏ bảo vệ cây khỏi côn trùng, nấm bệnh và mất nước.");
            Add(BiologyDifficulty.De,
                "Tim người bơm máu nhờ cấu trúc nào?",
                "Cơ tim", "Xương sườn", "Sụn", "Dây chằng", 0,
                "Cơ tim co bóp để đẩy máu đi khắp cơ thể.");

            // ---- TRUNG BÌNH ----
            Add(BiologyDifficulty.TrungBinh,
                "ADN có cấu tạo không gian dạng gì?",
                "Chữ nhật", "Chuỗi xoắn kép", "Hình cầu", "Hình thang", 1,
                "Mô hình Watson-Crick: ADN gồm 2 mạch xoắn kép bổ sung.");
            Add(BiologyDifficulty.TrungBinh,
                "Quang hợp xảy ra ở bào quan nào của tế bào thực vật?",
                "Ti thể", "Lục lạp", "Ribosome", "Nhân", 1,
                "Lục lạp chứa diệp lục, nơi diễn ra quang hợp.");
            Add(BiologyDifficulty.TrungBinh,
                "Hô hấp tế bào diễn ra chủ yếu ở bào quan nào?",
                "Lục lạp", "Ribosome", "Ti thể", "Lysosome", 2,
                "Ti thể là 'nhà máy năng lượng', tổng hợp ATP qua hô hấp tế bào.");
            Add(BiologyDifficulty.TrungBinh,
                "Sự tổng hợp protein diễn ra tại bào quan nào?",
                "Ribosome", "Nhân tế bào", "Màng tế bào", "Ti thể", 0,
                "Ribosome 'dịch mã' mARN thành chuỗi amino acid (protein).");
            Add(BiologyDifficulty.TrungBinh,
                "Nhóm máu nào được coi là người cho phổ quát (cho được cho mọi nhóm máu)?",
                "Nhóm A", "Nhóm B", "Nhóm AB", "Nhóm O", 3,
                "Nhóm O không có kháng nguyên A hay B nên cho được cho mọi nhóm máu.");
            Add(BiologyDifficulty.TrungBinh,
                "Hóc-môn nào điều hòa đường huyết (làm giảm đường trong máu)?",
                "Adrenalin", "Insulin", "Testosterone", "Estrogen", 1,
                "Insulin (tụy) giúp tế bào hấp thụ glucose, giảm đường huyết.");
            Add(BiologyDifficulty.TrungBinh,
                "Cây ăn thịt bẫy côn trùng bằng bẫy gì?",
                "Lá biến đổi", "Rễ", "Hoa", "Thân gỗ", 0,
                "Lá biến đổi thành bẫy (VD: bẫy quỳ, bẫy dạng bình) để bắt côn trùng.");
            Add(BiologyDifficulty.TrungBinh,
                "Máu đông lại nhờ tế bào nào?",
                "Hồng cầu", "Tiểu cầu", "Bạch cầu", "Tế bào mầm", 1,
                "Tiểu cầu kết cụ tại vết thương và khởi động quá trình đông máu.");
            Add(BiologyDifficulty.TrungBinh,
                "Người trưởng thành trung bình có khoảng bao nhiêu lít máu?",
                "2–3 lít", "3–4 lít", "4–5 lít", "7–8 lít", 2,
                "Cơ thể người trưởng thành có khoảng 4–5 lít máu (~7-8% thể trọng).");
            Add(BiologyDifficulty.TrungBinh,
                "Vitamin nào cơ thể tổng hợp được nhờ ánh nắng mặt trời?",
                "Vitamin A", "Vitamin C", "Vitamin D", "Vitamin K", 2,
                "Tia UV giúp da tổng hợp vitamin D - cần cho hấp thụ canxi.");
            Add(BiologyDifficulty.TrungBinh,
                "Nấm men (men bánh mì) được dùng để làm gì?",
                "Lên men làm bánh mì, rượu, bia", "Bào mòn đá", "Nhuộm vải", "Diệt vi khuẩn", 0,
                "Nấm men lên men đường thành CO2 và cồn - làm bánh mì nở và rượu bia.");
            Add(BiologyDifficulty.TrungBinh,
                "Vi khuẩn có lợi trong ruột người giúp gì?",
                "Tổng hợp vitamin và hỗ trợ tiêu hóa", "Gây táo bón", "Làm thủng ruột", "Không có tác dụng", 0,
                "Hệ vi sinh đường ruột tổng hợp vitamin K, B12 và hỗ trợ tiêu hóa.");
            Add(BiologyDifficulty.TrungBinh,
                "Hệ tuần hoàn gồm tim và gì?",
                "Phổi", "Mạch máu", "Dạ dày", "Gan", 1,
                "Hệ tuần hoàn = tim (bơm) + mạch máu (dẫn máu đi và về).");
            Add(BiologyDifficulty.TrungBinh,
                "Phân bón NPK chứa 3 nguyên tố nào?",
                "Natri, Photpho, Kali", "Nitơ, Photpho, Kali", "Nitơ, Photpho, Canxi", "Nitơ, Sắt, Kali", 1,
                "N = Nitơ, P = Photpho, K = Kali - 3 nguyên tố đa lượng chính.");

            // ---- KHÓ ----
            Add(BiologyDifficulty.Kho,
                "Giai đoạn của nguyên phân: NST xếp thành hàng ở xích đạo tế bào là kỳ nào?",
                "Kỳ trước", "Kỳ giữa", "Kỳ sau", "Kỳ cuối", 1,
                "Kỳ giữa (metaphase): NST tập trung xếp thành 1 hàng ở xích đạo.");
            Add(BiologyDifficulty.Kho,
                "Enzim nào 'cắt' ADN tại vị trí đặc hiệu?",
                "ADN polymerase", "ADN ligase", "Enzim giới hạn", "Helicase", 2,
                "Enzim giới hạn (restriction enzyme) cắt ADN tại trình tự đặc hiệu - công cụ của công nghệ gen.");
            Add(BiologyDifficulty.Kho,
                "Codon (bộ ba mã di truyền) nằm ở loại ADN hay ARN nào?",
                "tARN", "rARN", "mARN", "ADN", 2,
                "Codon là bộ ba nucleotit trên mARN; bộ ba đối ứng trên tARN gọi là anticodon.");
            Add(BiologyDifficulty.Kho,
                "Hệ số con lai F2 theo định luật Mendel với 1 cặp tính trạng đối lập là bao nhiêu?",
                "1:1", "3:1", "9:3:3:1", "1:2:1", 1,
                "Lai 1 cặp tính trạng: F2 phân ly theo tỉ lệ 3 trội : 1 lặn.");
            Add(BiologyDifficulty.Kho,
                "Bệnh nào do đột biến NST số 21 (trisomy 21)?",
                "Hội chứng Down", "Bạch tạng", "Lão hội", "Huyết áp cao", 0,
                "Trisomy 21 (3 bản sao NST 21) gây hội chứng Down.");
            Add(BiologyDifficulty.Kho,
                "Quá trình 'tổng hợp ADN từ bản mẫu ADN' được gọi là gì?",
                "Phiên mã", "Dịch mã", "Sao chép (tái bản)", "Biến dị", 2,
                "ADN → ADN là sao chép; ADN → ARN là phiên mã; ARN → protein là dịch mã.");
            Add(BiologyDifficulty.Kho,
                "Ở người, giới tính nam được xác định bởi cặp NST nào?",
                "XX", "XY", "XO", "YY", 1,
                "Nam = XY, nữ = XX. NST Y mang gen SRY quyết định giới tính nam.");
            Add(BiologyDifficulty.Kho,
                "Quy luật phân ly độc lập của Mendel áp dụng cho trường hợp nào?",
                "1 cặp tính trạng", "2 cặp trở lên nằm trên các cặp NST khác nhau", "Gen liên kết hoàn toàn", "Gen trên NST giới tính", 1,
                "Phân ly độc lập đúng cho các cặp gen nằm trên các cặp NST khác nhau.");
            Add(BiologyDifficulty.Kho,
                "Đột biến gen là thay đổi gì?",
                "Số lượng NST", "Cấu trúc gen (mất, thêm, thay cặp nucleotit)", "Số lượng tế bào", "Tỉ lệ trao đổi chất", 1,
                "Đột biến gen = thay đổi trình tự nucleotit trong 1 gen (mất, thêm, thay thế).");
            Add(BiologyDifficulty.Kho,
                "Quang hợp gồm 2 giai đoạn nào?",
                "Pha sáng và pha tối", "Pha nóng và pha lạnh", "Pha hút và pha đẩy", "Pha xanh và pha vàng", 0,
                "Pha sáng cần ánh sáng (phân ly nước, thải O2); pha tối (Chu trình Calvin) cố định CO2.");
            Add(BiologyDifficulty.Kho,
                "Trong nguyên phân, NST di chuyển về 2 cực tế bào ở kỳ nào?",
                "Kỳ giữa", "Kỳ sau", "Kỳ trước", "Kỳ cuối", 1,
                "Kỳ sau (anaphase): các chromatid con tách ra và di chuyển về 2 cực.");
            Add(BiologyDifficulty.Kho,
                "Kháng nguyên là gì?",
                "Chất lạ kích thích cơ thể tạo kháng thể", "Tế bào bảo vệ cơ thể", "Chất đông máu", "Mô liên kết", 0,
                "Kháng nguyên (antigen) là chất lạ (vi khuẩn, virus...) kích hoạt phản ứng miễn dịch.");
            Add(BiologyDifficulty.Kho,
                "ARN khác ADN ở điểm nào?",
                "Có ribose và uraxil thay timin", "Có deoxyribose và timin", "Gồm 2 mạch xoắn kép", "Không có nucleotit", 0,
                "ARN dùng đường ribose và bazơ uraxil (U) thay cho timin (T), thường 1 mạch.");
            Add(BiologyDifficulty.Kho,
                "Tế bào thực vật đặt vào dung dịch đậm đặc hơn sẽ hiện tượng gì?",
                "Co nguyên sinh", "Phình to nứt ra", "Phân chia", "Quang hợp mạnh", 0,
                "Nước rời khỏi tế bào (thẩm thấu) làm màng nguyên sinh chất tách khỏi vỏ → co nguyên sinh.");

            // ---- BOSS (Knowledge Clash) ----
            Add(BiologyDifficulty.Boss,
                "Môi trường trong nhân tế bào được gọi là gì?",
                "Chất tế bào", "Nhân tương", "Chất nền tế bào", "Dịch màng", 1,
                "Nhân tương (nucleoplasm) là dịch bên trong nhân tế bào.");
            Add(BiologyDifficulty.Boss,
                "Bào quan nào có hệ thống màng xếp lớp liên thông với nhân?",
                "Ribosome", "Lục lạp", "Mạng lưới nội chất (ER)", "Thể Golgi", 2,
                "Mạng lưới nội chất (ER) nối liền với màng nhân, có 2 loại: hạt và trơn.");
            Add(BiologyDifficulty.Boss,
                "Trong dịch mã, bộ ba đối ứng trên tARN được gọi là gì?",
                "Codon", "Anticodon", "Gen", "Nucleosome", 1,
                "Anticodon trên tARN bắt cặp bổ sung với codon trên mARN.");
            Add(BiologyDifficulty.Boss,
                "Di truyền liên kết với giới tính: gen nằm trên NST nào thường gây bệnh ở nam?",
                "NST thường", "NST X", "NST Y", "Ti thể ADN", 1,
                "Nam chỉ có 1 NST X nên gen lặn trên X (VD mù màu, huyết khối) dễ biểu hiện.");
            Add(BiologyDifficulty.Boss,
                "Nước được vận chuyển trong cơ thể qua cơ chế nào giữa tế bào?",
                "Khuếch tán thẩm thấu", "Tiêu giọt", "Đảo ngược thẩm phân", "Pha loãng", 0,
                "Nước khuếch tán qua màng bán thấm từ nơi ít chất tan → nhiều chất tan (thẩm thấu).");
            Add(BiologyDifficulty.Boss,
                "Cặp NST giới tính của con gái là:",
                "XY", "XX", "YY", "XO", 1,
                "Con gái mang 2 NST X (XX), con trai là XY.");
            Add(BiologyDifficulty.Boss,
                "Ti thể là bào quan chuyên làm gì?",
                "Tạo ATP (hô hấp tế bào)", "Quang hợp", "Lưu trữ nước", "Tạo tia năng lượng mặt trời", 0,
                "Ti thể thực hiện hô hấp tế bào, chuyển hóa glucose thành ATP.");
            Add(BiologyDifficulty.Boss,
                "Lục lạp chứa chất gì để thực hiện quang hợp?",
                "Diệp lục (chlorophyll)", "Hemoglobin", "Keratin", "Melanin", 0,
                "Diệp lục hấp thụ ánh sáng cho pha sáng của quang hợp.");
            Add(BiologyDifficulty.Boss,
                "Mã di truyền trên mARN được đọc như thế nào?",
                "Từng nucleotit rời", "Bộ ba nucleotit (codon)", "Toàn mạch một lúc", "Theo thứ tự ngược", 1,
                "Mã di truyền được đọc theo bộ ba (codon), mỗi codon mã hóa 1 amino acid.");
            Add(BiologyDifficulty.Boss,
                "Hội chứng Turner ở người do dạng NST nào?",
                "XO", "XXY", "XX", "XXX", 0,
                "Turner = thiếu 1 NST giới tính (XO), chỉ gặp ở nữ.");
            Add(BiologyDifficulty.Boss,
                "Enzim 'băng keo' nối các đoạn ADN lại với nhau là:",
                "ADN ligase", "ADN polymerase", "Helicase", "Amylase", 0,
                "ADN ligase tạo liên kết phosphodiester nối các đoạn ADN.");
            Add(BiologyDifficulty.Boss,
                "Quá trình tạo protein từ mARN tại ribosome gọi là:",
                "Phiên mã", "Dịch mã", "Sao chép", "Tái tổ hợp", 1,
                "Dịch mã (translation): ribosome đọc mARN để ghép amino acid thành protein.");
        }

        private void Add(BiologyDifficulty lv, string q, string a, string b, string c, string d,
                         int correct, string explain = "")
        {
            if (!_questions.ContainsKey(lv)) _questions[lv] = new List<BiologyQuestion>();
            _questions[lv].Add(new BiologyQuestion
            {
                level = lv,
                question = q,
                answerA = a,
                answerB = b,
                answerC = c,
                answerD = d,
                correctIndex = correct,
                explanation = explain
            });
        }
    }
}
