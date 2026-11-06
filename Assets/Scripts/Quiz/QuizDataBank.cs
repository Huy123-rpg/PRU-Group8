using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.Quiz
{
    /// <summary>
    /// Lớp quản lý ngân hàng câu hỏi, tải dữ liệu từ file JSON hoặc bản cập nhật của Giáo viên.
    /// </summary>
    public class QuizDataBank
    {
        // Danh sách lưu trữ toàn bộ câu hỏi
        private List<QuestionData> _questions = new List<QuestionData>();

        // Tổng số câu hỏi trong ngân hàng
        public int TotalQuestionCount => _questions.Count;

        /// <summary>
        /// Khởi tạo và tự động tải dữ liệu JSON.
        /// </summary>
        public QuizDataBank()
        {
            LoadData();
        }

        /// <summary>
        /// Tải dữ liệu JSON từ thư mục Resources hoặc từ bản cập nhật mới nhất của Giáo viên.
        /// </summary>
        private void LoadData()
        {
            string jsonContent = "";

            // 1. Ưu tiên nạp bản câu hỏi mới nhất vừa được Giáo viên thêm/chỉnh sửa
            if (PlayerPrefs.HasKey("Teacher_Custom_Questions_Json"))
            {
                jsonContent = PlayerPrefs.GetString("Teacher_Custom_Questions_Json", "");
            }

            // 2. Nếu chưa có bản cập nhật thì nạp từ file Resources gốc
            if (string.IsNullOrEmpty(jsonContent))
            {
                TextAsset jsonFile = Resources.Load<TextAsset>("QuizData/physics_questions");
                if (jsonFile != null)
                {
                    jsonContent = jsonFile.text;
                }
            }
            
            if (!string.IsNullOrEmpty(jsonContent))
            {
                try
                {
                    // Phân tích cú pháp chuỗi JSON thành đối tượng QuestionDataList
                    QuestionDataList dataList = JsonUtility.FromJson<QuestionDataList>(jsonContent);
                    
                    if (dataList != null && dataList.questions != null)
                    {
                        _questions = dataList.questions;
                        Debug.Log($"Đã tải thành công {_questions.Count} câu hỏi từ ngân hàng dữ liệu.");
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy danh sách câu hỏi trong dữ liệu JSON, hoặc cấu trúc JSON không khớp.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Lỗi khi phân tích dữ liệu JSON: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("Không tìm thấy ngân hàng câu hỏi QuizData/physics_questions trong thư mục Resources.");
            }
        }

        /// <summary>
        /// Lấy toàn bộ danh sách câu hỏi.
        /// </summary>
        public List<QuestionData> GetAllQuestions()
        {
            return new List<QuestionData>(_questions);
        }

        /// <summary>
        /// Lọc danh sách câu hỏi theo chương.
        /// </summary>
        public List<QuestionData> GetQuestionsByChapter(string chapter)
        {
            List<QuestionData> filteredQuestions = new List<QuestionData>();
            
            // Chuẩn hóa tên chương tìm kiếm
            string searchChapter = chapter.Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();

            foreach (var q in _questions)
            {
                string qChapter = q.chapter.Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();
                if (qChapter.Contains(searchChapter) || searchChapter.Contains(qChapter) || string.IsNullOrEmpty(searchChapter))
                {
                    filteredQuestions.Add(q);
                }
            }
            
            return filteredQuestions;
        }

        /// <summary>
        /// Lấy danh sách câu hỏi ngẫu nhiên từ toàn bộ ngân hàng.
        /// </summary>
        public List<QuestionData> GetRandomQuestions(int count)
        {
            return GetRandomQuestionsFromList(_questions, count);
        }

        /// <summary>
        /// Lấy danh sách câu hỏi ngẫu nhiên theo một chương cụ thể.
        /// </summary>
        public List<QuestionData> GetRandomQuestionsByChapter(string chapter, int count)
        {
            List<QuestionData> filteredQuestions = GetQuestionsByChapter(chapter);
            return GetRandomQuestionsFromList(filteredQuestions, count);
        }

        /// <summary>
        /// Hàm hỗ trợ: Trộn danh sách và lấy ra 'count' câu hỏi ngẫu nhiên sử dụng thuật toán Fisher-Yates.
        /// </summary>
        private List<QuestionData> GetRandomQuestionsFromList(List<QuestionData> sourceList, int count)
        {
            if (sourceList == null || sourceList.Count == 0)
            {
                Debug.LogWarning("Danh sách câu hỏi nguồn trống, không thể lấy câu hỏi ngẫu nhiên.");
                return new List<QuestionData>();
            }

            // Tạo bản sao để tránh thay đổi danh sách gốc
            List<QuestionData> shuffledList = new List<QuestionData>(sourceList);

            // Thuật toán xáo trộn Fisher-Yates
            for (int i = shuffledList.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                // Hoán đổi vị trí i và j
                QuestionData temp = shuffledList[i];
                shuffledList[i] = shuffledList[j];
                shuffledList[j] = temp;
            }

            // Trả về số lượng câu hỏi mong muốn, không vượt quá số lượng hiện có
            int returnCount = Mathf.Min(count, shuffledList.Count);
            return shuffledList.GetRange(0, returnCount);
        }
    
        #region ===== ĐỒNG BỘ CÂU HỎI TRẮC NGHIỆM & TỰ LUẬN TỪ GOOGLE SHEETS =====
        public const string TRAC_NGHIEM_CSV_URL = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=286121556";
        public const string TU_LUAN_CSV_URL = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=2085901900";

        /// <summary>
        /// Coroutine tải trực tiếp và đồng bộ ngân hàng câu hỏi từ Google Sheets (Live Sync)
        /// </summary>
        public static System.Collections.IEnumerator FetchAndSyncQuestionsFromGoogleSheets(System.Action<bool, int> onComplete = null)
        {
            int addedCount = 0;
            List<QuestionData> liveList = new List<QuestionData>();

            // 1. Tải câu hỏi trắc nghiệm
            using (UnityEngine.Networking.UnityWebRequest reqTN = UnityEngine.Networking.UnityWebRequest.Get(TRAC_NGHIEM_CSV_URL))
            {
                reqTN.timeout = 10;
                yield return reqTN.SendWebRequest();
                if (reqTN.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var tnQuestions = ParseTracNghiemCsv(reqTN.downloadHandler.text);
                    if (tnQuestions != null && tnQuestions.Count > 0)
                    {
                        liveList.AddRange(tnQuestions);
                        addedCount += tnQuestions.Count;
                        Debug.Log($"[QuizDataBank] 📥 Đã tải thành công {tnQuestions.Count} câu trắc nghiệm từ Google Sheets!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[QuizDataBank] ⚠️ Không tải được sheet trắc nghiệm: {reqTN.error}");
                }
            }

            // 2. Tải câu hỏi tự luận
            using (UnityEngine.Networking.UnityWebRequest reqTL = UnityEngine.Networking.UnityWebRequest.Get(TU_LUAN_CSV_URL))
            {
                reqTL.timeout = 10;
                yield return reqTL.SendWebRequest();
                if (reqTL.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var tlQuestions = ParseTuLuanCsv(reqTL.downloadHandler.text);
                    if (tlQuestions != null && tlQuestions.Count > 0)
                    {
                        liveList.AddRange(tlQuestions);
                        addedCount += tlQuestions.Count;
                        Debug.Log($"[QuizDataBank] 📥 Đã tải thành công {tlQuestions.Count} câu tự luận từ Google Sheets!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[QuizDataBank] ⚠️ Không tải được sheet tự luận: {reqTL.error}");
                }
            }

            if (liveList.Count > 0)
            {
                // Lưu vào cache PlayerPrefs để dùng xuyên suốt ứng dụng
                QuestionDataList dList = new QuestionDataList { questions = liveList };
                string json = JsonUtility.ToJson(dList, true);
                PlayerPrefs.SetString("Teacher_Custom_Questions_Json", json);
                PlayerPrefs.Save();
                Debug.Log($"[QuizDataBank] ✅ Đã lưu cache {liveList.Count} câu hỏi Live vào PlayerPrefs thành công!");
                onComplete?.Invoke(true, liveList.Count);
            }
            else
            {
                onComplete?.Invoke(false, 0);
            }
        }

        private static List<QuestionData> ParseTracNghiemCsv(string csv)
        {
            List<QuestionData> list = new List<QuestionData>();
            if (string.IsNullOrEmpty(csv)) return list;

            var rows = ParseCsvRows(csv);
            if (rows.Count <= 1) return list;

            // STT(0), CauHoiID(1), ChuongId(2), TenChuong(3), BaiID(4), TenBai(5), LoaiCauHoi(6), MucDo(7), NoiDung(8), DapAnA(9), DapAnB(10), DapAnC(11), DapAnD(12), DapAnDung(13), GiaiThich(14)
            for (int i = 1; i < rows.Count; i++)
            {
                var r = rows[i];
                if (r.Count < 14) continue;
                string qText = r[8].Trim();
                if (string.IsNullOrEmpty(qText)) continue;

                string a = r[9].Trim();
                string b = r[10].Trim();
                string c = r[11].Trim();
                string d = r[12].Trim();
                string correctKey = r[13].Trim().ToUpperInvariant();

                int cIdx = 0;
                if (correctKey.Contains("B") || correctKey == "1") cIdx = 1;
                else if (correctKey.Contains("C") || correctKey == "2") cIdx = 2;
                else if (correctKey.Contains("D") || correctKey == "3") cIdx = 3;

                string[] ansArr = new string[] { a, b, c, d };
                string expl = r.Count > 14 ? r[14].Trim() : "";

                QuestionData q = new QuestionData
                {
                    questionID = r[1].Trim(),
                    chuongId = r[2].Trim(),
                    chapter = r[3].Trim(),
                    tenChuong = r[3].Trim(),
                    baiID = r[4].Trim(),
                    tenBai = r[5].Trim(),
                    loaiCauHoi = "Trắc nghiệm",
                    mucDo = r[7].Trim(),
                    difficulty = MapMucDoToDifficulty(r[7].Trim()),
                    questionText = qText,
                    answers = ansArr,
                    correctAnswerIndex = cIdx,
                    correctAnswer = ansArr[cIdx],
                    explanation = expl
                };
                list.Add(q);
            }
            return list;
        }

        private static List<QuestionData> ParseTuLuanCsv(string csv)
        {
            List<QuestionData> list = new List<QuestionData>();
            if (string.IsNullOrEmpty(csv)) return list;

            var rows = ParseCsvRows(csv);
            if (rows.Count <= 1) return list;

            // STT(0), CauHoiID(1), ChuongId(2), TenChuong(3), BaiID(4), TenBai(5), GiaoVienID(6), TenGiaoVien(7), MucDo(8), NoiDung(9), TuKhoaChamDiem(10), DapAnTuLuanMau(11), GiaiThich_MeoGhiNho(12)
            for (int i = 1; i < rows.Count; i++)
            {
                var r = rows[i];
                if (r.Count < 12) continue;
                string qText = r[9].Trim();
                if (string.IsNullOrEmpty(qText)) continue;

                string tuKhoa = r[10].Trim();
                string dapAnMau = r[11].Trim();
                string expl = r.Count > 12 ? r[12].Trim() : "";

                QuestionData q = new QuestionData
                {
                    questionID = r[1].Trim(),
                    chuongId = r[2].Trim(),
                    chapter = r[3].Trim(),
                    tenChuong = r[3].Trim(),
                    baiID = r[4].Trim(),
                    tenBai = r[5].Trim(),
                    loaiCauHoi = "Tự luận",
                    mucDo = r[8].Trim(),
                    difficulty = MapMucDoToDifficulty(r[8].Trim()),
                    questionText = qText,
                    answers = new string[0],
                    correctAnswerIndex = -1,
                    correctAnswer = dapAnMau,
                    dapAnThamChieu = dapAnMau,
                    tuKhoa = tuKhoa,
                    explanation = !string.IsNullOrEmpty(expl) ? expl : dapAnMau
                };
                list.Add(q);
            }
            return list;
        }

        private static string MapMucDoToDifficulty(string md)
        {
            if (string.IsNullOrEmpty(md)) return "Easy";
            string l = md.ToLower();
            if (l.Contains("thông hiểu") || l.Contains("trung bình")) return "Medium";
            if (l.Contains("vận dụng cao") || l.Contains("khó")) return "VeryHard";
            if (l.Contains("vận dụng")) return "Hard";
            return "Easy";
        }

        private static List<List<string>> ParseCsvRows(string csv)
        {
            List<List<string>> result = new List<List<string>>();
            List<string> curRow = new List<string>();
            System.Text.StringBuilder curField = new System.Text.StringBuilder();
            bool insideQuotes = false;

            for (int i = 0; i < csv.Length; i++)
            {
                char c = csv[i];
                if (insideQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csv.Length && csv[i + 1] == '"')
                        {
                            curField.Append('"');
                            i++;
                        }
                        else
                        {
                            insideQuotes = false;
                        }
                    }
                    else
                    {
                        curField.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        insideQuotes = true;
                    }
                    else if (c == ',')
                    {
                        curRow.Add(curField.ToString());
                        curField.Length = 0;
                    }
                    else if (c == '\r')
                    {
                        // Bỏ qua
                    }
                    else if (c == '\n')
                    {
                        curRow.Add(curField.ToString());
                        curField.Length = 0;
                        result.Add(curRow);
                        curRow = new List<string>();
                    }
                    else
                    {
                        curField.Append(c);
                    }
                }
            }

            if (curField.Length > 0 || curRow.Count > 0)
            {
                curRow.Add(curField.ToString());
                result.Add(curRow);
            }

            return result;
        }
        #endregion
}
}
