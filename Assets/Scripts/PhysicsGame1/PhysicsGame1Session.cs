using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ScienceQuest.Quiz;

namespace ScienceQuest.PhysicsGame1
{
    using QuestionData = ScienceQuest.Quiz.QuestionData;

    [Serializable]
    public class TheoryLessonItem
    {
        public string baiID;
        public string chuongId;
        public string tenChuong;
        public string tenBai;
        public string khaiNiemTrongTam;
        public string congThucCanNho;
        public string hienTuongUngDung;
    }

    [Serializable]
    public class TheoryLessonList
    {
        public List<TheoryLessonItem> lessons;
    }

    [Serializable]
    public class QuestionReviewItem
    {
        public QuestionData question;
        public int selectedAnswerIndex = -1; // 0=A, 1=B, 2=C, 3=D, -1=none
        public string essayAnswerText = "";
        public bool isCorrect = false;
        public int earnedScore = 0;
        public string studentDisplayAnswer = "";
        public string correctDisplayAnswer = "";
        public string explanation = "";
        public string aiFeedback = "";
        public string aiSuggestions = "";
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public string className = "";
        public string characterId = "nv1";
        public int score;
        public float accuracy = 100f;
        public float timeUsed;
        public string lessonName = "";
        public string date;
    }

    [Serializable]
    public class RealStudentAchievement
    {
        public int rank;
        public string userId;
        public string playerName;
        public string className;
        public int score;
        public float accuracy;
        public float timeUsed;
        public string lesson;
        public string date;
        public string avatarId;
    }

    [Serializable]
    public class RealAchievementDataset
    {
        public List<RealStudentAchievement> students = new List<RealStudentAchievement>();
    }

    [Serializable]
    public class TheoryPageData
    {
        public string pageTitle;
        public string mainContent;
        public string tipContent;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    /// <summary>
    /// PhysicsGame1Session: Lưu trữ và đồng bộ toàn bộ dữ liệu xuyên suốt các màn của Game 1 (Vật lý - Bắn Vịt)
    /// Đáp ứng chính xác Phần I mục 2 và các yêu cầu nghiệp vụ của tài liệu đặc tả.
    /// </summary>
    public class PhysicsGame1Session : MonoBehaviour
    {
        public static PhysicsGame1Session Instance { get; private set; }

        [Header("=== THÔNG TIN HỌC SINH & LỰA CHỌN ===")]
        public string SelectedCharacterId = "nv1"; // Mặc định nv1 nếu chưa chọn
        public int SelectedChapterIndex = 1;
        public string SelectedChapterName = "Chương I: Năng lượng cơ học";
        public int SelectedLessonIndex = 2;
        public string SelectedLessonName = "Bài 2: Động năng. Thế năng";
        public string SelectedGameMode = "TracNghiem"; // TracNghiem, TuLuan, LyThuyet, DoiKhang
        public string SelectedLevel = "NhanBiet"; // NhanBiet, ThongHieu, VanDung, VanDungCao

        [Header("=== ĐIỀU HƯỚNG QUAY LẠI ===")]
        public string PreviousSceneName = "01_MainMenu";

        [Header("=== DỮ LIỆU LƯỢT CHƠI HIỆN TẠI ===")]
        public int CurrentScore = 0;
        public int AnsweredCount = 0;
        public const int MaxQuestionsPerSession = 10;
        public bool IsEssayMode
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedGameMode)) return false;
                string m = SelectedGameMode.Trim().ToLower();
                return m == "tuluan" || m == "essay" || m.Contains("tự luận") || m.Contains("tu luan");
            }
        }
        public int MaxQuestionsForCurrentSession => IsEssayMode ? 5 : 10;
        public float RemainingTime = 900f; // 15 phút = 900 giây
        public float FreezeTimeRemaining = 0f; // Đóng băng đồng hồ khi bắn trúng vịt xanh & đúng
        public List<QuestionData> CurrentQuestions = new List<QuestionData>();
        public List<QuestionReviewItem> QuestionResults = new List<QuestionReviewItem>();

        [Header("=== CHẾ ĐỘ ĐỐI KHÁNG ===")]
        public string OpponentName = "Robot Gia Sư";
        public int PlayerVersusScore = 0;
        public int OpponentVersusScore = 0;
        public int PlayerAnsweredVersus = 0;
        public int OpponentAnsweredVersus = 0;

        [Header("=== CÀI ĐẶT ÂM THANH ===")]
        public bool BGMEnabled = true;
        public bool SFXEnabled = true;

        private QuizDataBank _dataBank;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("PhysicsGame1Session_Persistent");
                Instance = go.AddComponent<PhysicsGame1Session>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitSession();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitSession()
        {
            // Tải cài đặt âm thanh
            BGMEnabled = PlayerPrefs.GetInt("PhysicsGame1_BGM", 1) == 1;
            SFXEnabled = PlayerPrefs.GetInt("PhysicsGame1_SFX", 1) == 1;

            // Nhân vật đã lưu gần nhất (nếu có)
            SelectedCharacterId = PlayerPrefs.GetString("PhysicsGame1_SelectedCharacter", "nv1");
            if (string.IsNullOrEmpty(SelectedCharacterId)) SelectedCharacterId = "nv1";

            // Khởi tạo ngân hàng câu hỏi
            _dataBank = new QuizDataBank();
        }

        /// <summary>
        /// Bắt đầu lượt chơi mới (10 câu, 15 phút, điểm 0).
        /// </summary>
        public void StartNewGameSession()
        {
            CurrentScore = 0;
            AnsweredCount = 0;
            RemainingTime = 900f; // 15:00
            FreezeTimeRemaining = 0f;
            QuestionResults.Clear();

            // Lọc 10 câu hỏi theo Chương và Mức độ đã chọn
            LoadQuestionsForSession();
        }

        /// <summary>
        /// Bắt đầu lượt đấu đối kháng mới
        /// </summary>
        public void StartNewVersusSession(string opponent = "Robot Gia Sư")
        {
            OpponentName = opponent;
            PlayerVersusScore = 0;
            OpponentVersusScore = 0;
            PlayerAnsweredVersus = 0;
            OpponentAnsweredVersus = 0;
            RemainingTime = 600f; // 10 phút cho đối kháng
            StartNewGameSession();
        }

        private void LoadQuestionsForSession()
        {
            CurrentQuestions.Clear();
            if (_dataBank == null) _dataBank = new QuizDataBank();

            int targetCount = MaxQuestionsForCurrentSession; // BẮT BUỘC: 10 câu cho Trắc nghiệm, 5 câu cho Tự luận
            bool isTuLuan = IsEssayMode;

            // 1. Lấy toàn bộ câu hỏi từ ngân hàng
            List<QuestionData> allQuestions = _dataBank.GetAllQuestions();
            if (allQuestions == null) allQuestions = new List<QuestionData>();

            // Phân loại câu hỏi Tự luận vs Trắc nghiệm
            Func<QuestionData, bool> matchesMode = (q) =>
            {
                if (q == null) return false;
                bool isQTuLuan = false;
                if (!string.IsNullOrEmpty(q.loaiCauHoi))
                {
                    string lch = q.loaiCauHoi.ToLower();
                    if (lch.Contains("tự luận") || lch.Contains("tuluan") || lch.Contains("essay")) isQTuLuan = true;
                }
                if (!string.IsNullOrEmpty(q.questionID))
                {
                    if (q.questionID.StartsWith("TL_", StringComparison.OrdinalIgnoreCase) ||
                        q.questionID.StartsWith("TL", StringComparison.OrdinalIgnoreCase) ||
                        q.questionID.IndexOf("_TL_", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        isQTuLuan = true;
                    }
                }
                if (q.answers == null || q.answers.Length < 2)
                {
                    isQTuLuan = true;
                }
                return isTuLuan ? isQTuLuan : !isQTuLuan;
            };

            // Hàm kiểm tra độ khó
            Func<QuestionData, bool> matchesDifficulty = (q) =>
            {
                if (q == null) return false;
                string diff = (q.difficulty ?? "").ToLower();
                string mucDo = (q.mucDo ?? "").ToLower();

                if (SelectedLevel == "NhanBiet")
                {
                    return diff == "easy" || mucDo.Contains("nhận biết") || mucDo.Contains("dễ") || (string.IsNullOrEmpty(diff) && string.IsNullOrEmpty(mucDo));
                }
                else if (SelectedLevel == "ThongHieu")
                {
                    return diff == "medium" || mucDo.Contains("thông hiểu") || mucDo.Contains("trung bình");
                }
                else if (SelectedLevel == "VanDung")
                {
                    return diff == "hard" || (mucDo.Contains("vận dụng") && !mucDo.Contains("cao")) || mucDo.Contains("khó");
                }
                else if (SelectedLevel == "VanDungCao")
                {
                    return diff == "veryhard" || mucDo.Contains("cao") || mucDo.Contains("tổng hợp");
                }
                return true;
            };

            // Hàm kiểm tra chương
            string searchChapter = (SelectedChapterName ?? "").Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();
            Func<QuestionData, bool> matchesChapter = (q) =>
            {
                if (q == null) return false;
                if (string.IsNullOrEmpty(searchChapter)) return true;
                string qChapter = (q.chapter ?? "").Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();
                return qChapter.Contains(searchChapter) || searchChapter.Contains(qChapter);
            };

            // Bước 1: Lấy các câu hỏi cùng chương, đúng chế độ, đúng độ khó
            List<QuestionData> pool = new List<QuestionData>();
            foreach (var q in allQuestions)
            {
                if (matchesMode(q) && matchesChapter(q) && matchesDifficulty(q))
                {
                    if (!pool.Contains(q)) pool.Add(q);
                }
            }

            // Bước 2: Nếu chưa đủ targetCount, bổ sung câu hỏi cùng chương, đúng chế độ (bất kể độ khó)
            if (pool.Count < targetCount)
            {
                foreach (var q in allQuestions)
                {
                    if (matchesMode(q) && matchesChapter(q))
                    {
                        if (!pool.Contains(q))
                        {
                            pool.Add(q);
                            if (pool.Count >= targetCount) break;
                        }
                    }
                }
            }

            // Bước 3: Nếu vẫn chưa đủ targetCount, bổ sung câu hỏi từ CÁC CHƯƠNG KHÁC (đúng chế độ, đúng độ khó)
            if (pool.Count < targetCount)
            {
                foreach (var q in allQuestions)
                {
                    if (matchesMode(q) && matchesDifficulty(q))
                    {
                        if (!pool.Contains(q))
                        {
                            pool.Add(q);
                            if (pool.Count >= targetCount) break;
                        }
                    }
                }
            }

            // Bước 4: Nếu vẫn chưa đủ targetCount, bổ sung bất kỳ câu hỏi nào trong ngân hàng đúng chế độ
            if (pool.Count < targetCount)
            {
                foreach (var q in allQuestions)
                {
                    if (matchesMode(q))
                    {
                        if (!pool.Contains(q))
                        {
                            pool.Add(q);
                            if (pool.Count >= targetCount) break;
                        }
                    }
                }
            }

            // Bước 5: Nếu ngân hàng hoàn toàn không có câu hỏi nào đúng chế độ, tạo câu hỏi fallback chuẩn
            if (pool.Count == 0)
            {
                CreateFallbackQuestions(isTuLuan, targetCount);
                return;
            }

            // Bước 6: Xáo trộn ngẫu nhiên danh sách pool tìm được
            System.Random rnd = new System.Random();
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int k = rnd.Next(i + 1);
                var temp = pool[i];
                pool[i] = pool[k];
                pool[k] = temp;
            }

            // Lấy tối đa targetCount câu từ pool
            int takeCount = Mathf.Min(pool.Count, targetCount);
            for (int i = 0; i < takeCount; i++)
            {
                CurrentQuestions.Add(pool[i]);
            }

            // Bước 7: NẾU pool có ít hơn targetCount (ví dụ giáo viên mới chỉ tạo 1-2 câu tự luận cho chương này),
            // lặp lại các câu hỏi này với ID bản sao để đảm bảo BẮT BUỘC ĐỦ targetCount (10 câu TN / 5 câu TL)!
            int repeatIndex = 0;
            while (CurrentQuestions.Count < targetCount)
            {
                var srcQ = pool[repeatIndex % pool.Count];
                QuestionData dupQ = new QuestionData
                {
                    questionID = $"{srcQ.questionID}_rpt{CurrentQuestions.Count + 1}",
                    questionText = srcQ.questionText,
                    answers = srcQ.answers != null ? (string[])srcQ.answers.Clone() : null,
                    correctAnswerIndex = srcQ.correctAnswerIndex,
                    correctAnswer = srcQ.correctAnswer,
                    explanation = srcQ.explanation,
                    chapter = srcQ.chapter,
                    difficulty = srcQ.difficulty,
                    mucDo = srcQ.mucDo,
                    loaiCauHoi = srcQ.loaiCauHoi,
                    maxScore = srcQ.maxScore,
                    nguonTao = srcQ.nguonTao,
                    ngayTao = srcQ.ngayTao
                };
                CurrentQuestions.Add(dupQ);
                repeatIndex++;
            }

            Debug.Log($"[PhysicsGame1Session] 🎯 Đã chuẩn bị chính xác {CurrentQuestions.Count}/{targetCount} câu hỏi cho lượt chơi (Chế độ: {(isTuLuan ? "Tự luận (5 câu)" : "Trắc nghiệm (10 câu)")}, Chương: '{SelectedChapterName}').");
        }

        private void CreateFallbackQuestions(bool isTuLuan, int targetCount)
        {
            if (isTuLuan)
            {
                string[] essaySamples = new string[]
                {
                    "Nêu định nghĩa và viết công thức tính động năng của một vật khối lượng m đang chuyển động với vận tốc v. Nêu rõ đơn vị của từng đại lượng.|Động năng là dạng năng lượng mà một vật có được do nó đang chuyển động. Công thức: Wđ = 1/2 * m * v^2. Trong đó: m là khối lượng (kg), v là vận tốc (m/s), Wđ là động năng (J).|Định nghĩa động năng đúng; công thức Wđ = 1/2 m v^2; chú thích đầy đủ đơn vị kg, m/s, J.",
                    "Thế nào là hiện tượng khúc xạ ánh sáng? Nêu mối quan hệ giữa góc tới và góc khúc xạ khi tia sáng truyền xiên góc từ không khí vào nước.|Khúc xạ ánh sáng là hiện tượng tia sáng bị gãy khúc khi truyền xiên góc qua mặt phân cách giữa hai môi trường trong suốt. Khi truyền từ không khí vào nước thì góc khúc xạ r nhỏ hơn góc tới i (r < i).|Định nghĩa hiện tượng gãy khúc tại mặt phân cách; nêu đúng điều kiện r < i khi từ không khí vào nước.",
                    "Phát biểu nội dung định luật Ohm đối với đoạn mạch có điện trở R. Viết hệ thức của định luật và giải thích các đại lượng trong hệ thức.|Cường độ dòng điện chạy qua dây dẫn tỉ lệ thuận với hiệu điện thế giữa hai đầu dây và tỉ lệ nghịch với điện trở của dây: I = U / R. Trong đó: I là cường độ dòng điện (A), U là hiệu điện thế (V), R là điện trở (Ω).|Phát biểu tỉ lệ thuận U, tỉ lệ nghịch R; công thức I = U/R; chú thích đơn vị A, V, Ω.",
                    "Giải thích tại sao khi xe buýt phanh gấp, hành khách trên xe lại bị ngả người chúi về phía trước? Dựa vào định luật vật lý nào để giải thích?|Do hiện tượng quán tính. Khi xe buýt phanh gấp, phần thân dưới của hành khách dừng lại cùng với xe, nhưng phần thân trên do quán tính vẫn giữ nguyên vận tốc cũ và tiếp tục chuyển động về phía trước, làm hành khách bị chúi ngả người.|Nêu rõ hiện tượng quán tính; giải thích sự chênh lệch trạng thái chuyển động giữa phần thân dưới và thân trên.",
                    "Nêu các cách làm tăng từ trường của một ống dây có dòng điện chạy qua. Kể tên ít nhất hai ứng dụng của nam châm điện trong đời sống và kỹ thuật.|Các cách tăng từ trường: tăng cường độ dòng điện I, tăng số vòng dây quấn N, hoặc đặt thêm lõi sắt non vào lòng ống dây. Ứng dụng: rơ-le điện từ, chuông điện, cần cẩu điện từ phế liệu.|Nêu đủ 3 cách tăng từ trường (I, N, lõi sắt non); kể tên chính xác từ 2 ứng dụng thực tế trở lên."
                };

                for (int i = 0; i < targetCount; i++)
                {
                    string raw = essaySamples[i % essaySamples.Length];
                    string[] parts = raw.Split('|');
                    QuestionData q = new QuestionData
                    {
                        questionID = "FB_TL_" + (i + 1),
                        questionText = parts[0],
                        answers = new string[0],
                        correctAnswerIndex = -1,
                        correctAnswer = parts[1],
                        chapter = SelectedChapterName,
                        difficulty = "Medium",
                        mucDo = "Thông hiểu",
                        loaiCauHoi = "Tự luận",
                        explanation = parts[2]
                    };
                    CurrentQuestions.Add(q);
                }
            }
            else
            {
                string[] samples = new string[]
                {
                    "Động năng của một vật chuyển động phụ thuộc vào những yếu tố nào sau đây?|Khối lượng và vận tốc của vật|Khối lượng và độ cao của vật|Vận tốc và thể tích của vật|Trọng lượng và màu sắc của vật|0|Động năng Wđ = 1/2*m*v^2 phụ thuộc vào khối lượng m và vận tốc v của vật.",
                    "Đơn vị đo của công cơ học và cơ năng trong hệ đơn vị quốc tế SI là gì?|Jun (J)|Oát (W)|Niutơn (N)|Mã lực (HP)|0|Trong hệ đơn vị quốc tế SI, đơn vị đo của công và năng lượng là Jun (J).",
                    "Hiện tượng khúc xạ ánh sáng là hiện tượng gì?|Tia sáng bị gãy khúc khi truyền qua mặt phân cách giữa hai môi trường trong suốt|Tia sáng bị hắt trở lại môi trường trong suốt cũ|Tia sáng bị hấp thụ hoàn toàn bởi môi trường|Tia sáng đổi màu khi đi vào môi trường mới|0|Khúc xạ ánh sáng là hiện tượng tia sáng bị gãy khúc khi truyền xiên góc qua mặt phân cách giữa 2 môi trường trong suốt.",
                    "Hệ thức của định luật Ohm cho đoạn mạch có điện trở R là gì?|I = U / R|U = I / R|R = U * I|I = U * R|0|Định luật Ohm: Cường độ dòng điện chạy qua dây dẫn tỉ lệ thuận với hiệu điện thế U và tỉ lệ nghịch với điện trở R: I = U/R.",
                    "Khi đưa thanh nam châm lại gần một cuộn dây dẫn kín thì trong cuộn dây xuất hiện:?|Dòng điện cảm ứng|Lực đẩy vĩnh cửu|Nhiệt độ giảm sâu|Hiện tượng phóng điện hồ quang|0|Sự biến thiên số đường sức từ qua tiết diện cuộn dây kín làm sinh ra dòng điện cảm ứng.",
                    "Một vật có khối lượng 2kg đang chuyển động với vận tốc 3m/s thì động năng của vật là bao nhiêu?|9 Jun|6 Jun|18 Jun|12 Jun|0|Động năng Wđ = 1/2 * m * v^2 = 0.5 * 2 * 3^2 = 9 J.",
                    "Thế năng trọng trường của một vật phụ thuộc vào những đại lượng nào?|Khối lượng và độ cao của vật so với mốc chọn thế năng|Vận tốc và thể tích của vật|Khối lượng và nhiệt độ của vật|Hình dạng và màu sắc của vật|0|Thế năng trọng trường Wt = m*g*z phụ thuộc vào khối lượng m và độ cao z so với mốc.",
                    "Khi tia sáng truyền từ nước sang không khí với góc tới i > 0, so sánh giữa góc khúc xạ r và góc tới i?|Góc khúc xạ r lớn hơn góc tới i (r > i)|Góc khúc xạ r nhỏ hơn góc tới i (r < i)|Góc khúc xạ r bằng góc tới i (r = i)|Góc khúc xạ r luôn bằng 90 độ|0|Khi truyền từ môi trường chiết quang hơn sang môi trường kém chiết quang hơn, góc khúc xạ r lớn hơn góc tới i.",
                    "Khi hai điện trở R1 và R2 mắc nối tiếp thì điện trở tương đương của đoạn mạch là:?|Rtđ = R1 + R2|Rtđ = R1 * R2|Rtđ = (R1 * R2) / (R1 + R2)|Rtđ = 1/R1 + 1/R2|0|Đoạn mạch gồm 2 điện trở mắc nối tiếp có điện trở tương đương Rtđ = R1 + R2.",
                    "Dòng điện xoay chiều có thể tạo ra tác dụng nào sau đây?|Tác dụng nhiệt, tác dụng phát sáng và tác dụng từ|Chỉ có tác dụng nhiệt|Chỉ có tác dụng từ|Chỉ có tác dụng hóa học mạ điện|0|Dòng điện xoay chiều có đầy đủ tác dụng nhiệt, phát sáng, cơ học và tác dụng từ biến thiên."
                };

                for (int i = 0; i < targetCount; i++)
                {
                    string raw = samples[i % samples.Length];
                    string[] parts = raw.Split('|');
                    QuestionData q = new QuestionData
                    {
                        questionID = "FB_TN_" + (i + 1),
                        questionText = parts[0],
                        answers = new string[] { parts[1], parts[2], parts[3], parts[4] },
                        correctAnswerIndex = int.Parse(parts[5]),
                        correctAnswer = parts[int.Parse(parts[5]) + 1],
                        chapter = SelectedChapterName,
                        difficulty = "Easy",
                        mucDo = "Nhận biết",
                        loaiCauHoi = "Trắc nghiệm",
                        explanation = parts[6]
                    };
                    CurrentQuestions.Add(q);
                }
            }
        }

        /// <summary>
        /// Ghi nhận kết quả trả lời 1 câu trắc nghiệm
        /// </summary>
        public void RecordMultipleChoiceAnswer(int qIndex, int chosenIndex, bool isBlueDuck)
        {
            if (qIndex < 0 || qIndex >= CurrentQuestions.Count) return;
            QuestionData q = CurrentQuestions[qIndex];
            bool correct = (chosenIndex == q.correctAnswerIndex);

            if (correct)
            {
                CurrentScore += 10;
                if (isBlueDuck)
                {
                    // Vịt xanh trả lời đúng: đóng băng đồng hồ 10 giây
                    FreezeTimeRemaining = 10f;
                    Debug.Log("[PhysicsGame1Session] ❄️ Vịt xanh đúng! Đóng băng đồng hồ 10 giây!");
                }
            }

            string studentAns = (chosenIndex >= 0 && chosenIndex < q.answers.Length) ? q.answers[chosenIndex] : "Chưa chọn";
            string correctAns = (q.correctAnswerIndex >= 0 && q.correctAnswerIndex < q.answers.Length) ? q.answers[q.correctAnswerIndex] : "";

            QuestionReviewItem item = new QuestionReviewItem
            {
                question = q,
                selectedAnswerIndex = chosenIndex,
                isCorrect = correct,
                studentDisplayAnswer = studentAns,
                correctDisplayAnswer = correctAns,
                explanation = q.explanation
            };
            QuestionResults.Add(item);
            AnsweredCount++;
        }

        /// <summary>
        /// Ghi nhận kết quả câu tự luận từ AI chấm điểm
        /// </summary>
        public void RecordEssayResult(int qIndex, string essayText, bool isCorrect, int earnedScore, AIGradingResponse aiResp)
        {
            if (qIndex < 0 || qIndex >= CurrentQuestions.Count) return;
            QuestionData q = CurrentQuestions[qIndex];

            if (isCorrect && earnedScore > 0)
            {
                CurrentScore += earnedScore;
            }

            string correctAns = (q.correctAnswerIndex >= 0 && q.correctAnswerIndex < q.answers.Length) ? q.answers[q.correctAnswerIndex] : "Đạt yêu cầu";
            string fb = aiResp != null ? aiResp.feedback : "";
            string sugg = aiResp != null ? aiResp.suggestions : "";

            QuestionReviewItem item = new QuestionReviewItem
            {
                question = q,
                essayAnswerText = essayText,
                isCorrect = isCorrect,
                earnedScore = earnedScore,
                studentDisplayAnswer = essayText,
                correctDisplayAnswer = !string.IsNullOrEmpty(aiResp?.suggestedAnswer) ? aiResp.suggestedAnswer : correctAns,
                explanation = !string.IsNullOrEmpty(fb) ? fb : q.explanation,
                aiFeedback = fb,
                aiSuggestions = sugg
            };
            QuestionResults.Add(item);
            AnsweredCount++;
        }

        /// <summary>
        /// Ghi nhận kết quả câu tự luận cơ bản (tự động phân loại linh tinh -> 0 điểm)
        /// </summary>
        public void RecordEssayAnswer(int qIndex, string essayText, bool isBlueDuck)
        {
            if (qIndex < 0 || qIndex >= CurrentQuestions.Count) return;
            QuestionData q = CurrentQuestions[qIndex];

            bool isGibberish = string.IsNullOrWhiteSpace(essayText) || essayText.Trim().Length < 12;
            if (!isGibberish)
            {
                string lower = essayText.ToLower();
                string[] badKeywords = new string[] { "linh tinh", "asdf", "qwer", "12345", "test" };
                foreach (var bad in badKeywords)
                {
                    if (lower.Contains(bad)) { isGibberish = true; break; }
                }
            }

            bool correct = !isGibberish;
            int score = correct ? 10 : 0;
            if (correct)
            {
                CurrentScore += score;
                if (isBlueDuck) FreezeTimeRemaining = 10f;
            }

            string correctAns = (q.correctAnswerIndex >= 0 && q.correctAnswerIndex < q.answers.Length) ? q.answers[q.correctAnswerIndex] : "Đạt yêu cầu";

            QuestionReviewItem item = new QuestionReviewItem
            {
                question = q,
                essayAnswerText = essayText,
                isCorrect = correct,
                earnedScore = score,
                studentDisplayAnswer = essayText,
                correctDisplayAnswer = correctAns,
                explanation = correct ? "Đạt yêu cầu tự luận." : "Bài làm viết linh tinh hoặc chưa đủ độ dài (0 điểm)."
            };
            QuestionResults.Add(item);
            AnsweredCount++;
        }

        /// <summary>
        /// Phạt trúng vịt bom: trừ 10 điểm (không âm)
        /// </summary>
        public void HitBombDuck()
        {
            CurrentScore = Mathf.Max(0, CurrentScore - 10);
            Debug.Log($"[PhysicsGame1Session] 💣 Bắn trúng vịt bom! Trừ 10 điểm. Điểm còn: {CurrentScore}");
        }

        /// <summary>
        /// Lưu kỷ lục điểm vào bảng xếp hạng
        /// </summary>
        public void SaveScoreRecord()
        {
            string username = "Học sinh";
            if (GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username))
                username = GameSession.Instance.username;

            float timeUsed = 900f - RemainingTime;
            AddLeaderboardEntry(username, SelectedCharacterId, CurrentScore, timeUsed);

            // Cập nhật kỷ lục cá nhân
            int high = PlayerPrefs.GetInt("PhysicsGame1_HighScore", 0);
            if (CurrentScore > high)
            {
                PlayerPrefs.SetInt("PhysicsGame1_HighScore", CurrentScore);
            }
            PlayerPrefs.SetInt("PhysicsGame1_LastScore", CurrentScore);
            PlayerPrefs.Save();
        }

        public void AddLeaderboardEntry(string name, string charId, int score, float duration)
        {
            LeaderboardData data = GetLeaderboard();
            LeaderboardEntry entry = new LeaderboardEntry
            {
                playerName = name,
                characterId = charId,
                score = score,
                timeUsed = duration,
                date = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            };
            data.entries.Add(entry);

            // Sắp xếp: Điểm cao xếp trên, nếu bằng điểm thì thời gian ngắn hơn xếp trên
            data.entries.Sort((a, b) =>
            {
                int cmp = b.score.CompareTo(a.score);
                if (cmp != 0) return cmp;
                return a.timeUsed.CompareTo(b.timeUsed);
            });

            // Giới hạn 20 kỷ lục cao nhất
            if (data.entries.Count > 20)
            {
                data.entries.RemoveRange(20, data.entries.Count - 20);
            }

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("PhysicsGame1_Leaderboard", json);
            PlayerPrefs.Save();
        }

        public LeaderboardData GetLeaderboard()
        {
            LeaderboardData result = new LeaderboardData();

            // 1. Nạp danh sách học sinh thật từ bảng dữ liệu thật Excel / JSON
            TextAsset realDataAsset = Resources.Load<TextAsset>("QuizData/real_achievements_data");
            if (realDataAsset != null && !string.IsNullOrEmpty(realDataAsset.text))
            {
                try
                {
                    RealAchievementDataset dataset = JsonUtility.FromJson<RealAchievementDataset>(realDataAsset.text);
                    if (dataset != null && dataset.students != null)
                    {
                        foreach (var s in dataset.students)
                        {
                            result.entries.Add(new LeaderboardEntry
                            {
                                playerName = s.playerName,
                                className = s.className,
                                characterId = string.IsNullOrEmpty(s.avatarId) ? "nv1" : s.avatarId,
                                score = s.score,
                                accuracy = s.accuracy > 0 ? s.accuracy : (s.score >= 100 ? 100f : (s.score >= 90 ? 90f : 80f)),
                                timeUsed = s.timeUsed > 0 ? s.timeUsed : 180f,
                                lessonName = s.lesson,
                                date = s.date
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PhysicsGame1Session] Lỗi parse real_achievements_data.json: {ex.Message}");
                }
            }

            // Fallback nếu không nạp được file
            if (result.entries.Count == 0)
            {
                result.entries.Add(new LeaderboardEntry { playerName = "Lê Khánh Chi", className = "Lớp 9A3", characterId = "nv3", score = 100, accuracy = 100f, timeUsed = 155f, lessonName = "Bài 11. Đoạn mạch song song", date = "27/09/2026" });
                result.entries.Add(new LeaderboardEntry { playerName = "Nguyễn Minh Anh", className = "Lớp 9A1", characterId = "nv1", score = 90, accuracy = 90f, timeUsed = 190f, lessonName = "Bài 2. Động năng. Thế năng", date = "27/09/2026" });
                result.entries.Add(new LeaderboardEntry { playerName = "Phạm Đức Duy", className = "Lớp 9A4", characterId = "nv4", score = 90, accuracy = 90f, timeUsed = 240f, lessonName = "Đề TEST tổng hợp Chương I", date = "27/09/2026" });
                result.entries.Add(new LeaderboardEntry { playerName = "Trần Gia Bảo", className = "Lớp 9A2", characterId = "nv2", score = 80, accuracy = 80f, timeUsed = 210f, lessonName = "Bài 3. Cơ năng", date = "27/09/2026" });
                result.entries.Add(new LeaderboardEntry { playerName = "Hoàng Ngọc Hà", className = "Lớp 9A5", characterId = "nv1", score = 75, accuracy = 75f, timeUsed = 265f, lessonName = "Bài 4. Công và công suất", date = "27/09/2026" });
            }

            // 2. Nạp thêm các kỷ lục do người chơi vừa hoàn thành trong game (từ PlayerPrefs)
            string savedJson = PlayerPrefs.GetString("PhysicsGame1_Leaderboard", "");
            if (!string.IsNullOrEmpty(savedJson))
            {
                try
                {
                    LeaderboardData savedData = JsonUtility.FromJson<LeaderboardData>(savedJson);
                    if (savedData != null && savedData.entries != null)
                    {
                        foreach (var e in savedData.entries)
                        {
                            if (!result.entries.Any(r => r.playerName == e.playerName && r.score == e.score))
                            {
                                result.entries.Add(e);
                            }
                        }
                    }
                }
                catch { }
            }

            // 3. Nếu người chơi vừa chơi xong có điểm > 0 trong session hiện tại và chưa lưu
            if (CurrentScore > 0)
            {
                string myName = $"Bạn ({SelectedCharacterId.ToUpper()})";
                if (!result.entries.Any(e => e.playerName == myName && e.score == CurrentScore))
                {
                    float timeUsed = Mathf.Max(30f, 900f - RemainingTime);
                    result.entries.Add(new LeaderboardEntry
                    {
                        playerName = myName,
                        className = "Học sinh",
                        characterId = SelectedCharacterId,
                        score = CurrentScore,
                        accuracy = Mathf.Clamp(CurrentScore / 10f * 10f, 0f, 100f),
                        timeUsed = timeUsed,
                        lessonName = SelectedLessonName,
                        date = DateTime.Now.ToString("dd/MM/yyyy")
                    });
                }
            }

            // 4. Sắp xếp thứ hạng chuẩn: Điểm cao lên trước, nếu hòa điểm thì thời gian ngắn hơn lên trước
            result.entries.Sort((a, b) =>
            {
                int cmp = b.score.CompareTo(a.score);
                if (cmp != 0) return cmp;
                return a.timeUsed.CompareTo(b.timeUsed);
            });

            return result;
        }

        private List<TheoryLessonItem> _theoryLessons;

        private void LoadTheoryData()
        {
            if (_theoryLessons != null && _theoryLessons.Count > 0) return;

            // Ưu tiên nạp từ TeacherDataManager nếu có dữ liệu bài học/lý thuyết được cập nhật từ giáo viên
            if (ScienceQuest.Teacher.TeacherDataManager.Instance != null)
            {
                var tLessons = ScienceQuest.Teacher.TeacherDataManager.Instance.GetLessons("Physics");
                if (tLessons != null && tLessons.Count > 0)
                {
                    _theoryLessons = tLessons.Select(l => new TheoryLessonItem
                    {
                        baiID = l.baiID,
                        chuongId = l.chuongId,
                        tenChuong = l.tenChuong,
                        tenBai = l.tenBai,
                        khaiNiemTrongTam = l.khaiNiemTrongTam,
                        congThucCanNho = l.congThucCanNho,
                        hienTuongUngDung = l.hienTuongUngDung
                    }).ToList();
                    return;
                }
            }

            TextAsset json = Resources.Load<TextAsset>("QuizData/physics_theory");
            if (json != null)
            {
                try
                {
                    var data = JsonUtility.FromJson<TheoryLessonList>(json.text);
                    if (data != null && data.lessons != null)
                    {
                        _theoryLessons = data.lessons;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PhysicsGame1] Error parsing physics_theory.json: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Lấy 3 trang lý thuyết phân chia rõ ràng để không bao giờ bị tràn khung hình
        /// </summary>
        public List<TheoryPageData> GetTheoryPagesForCurrentLesson()
        {
            LoadTheoryData();

            TheoryLessonItem matchedItem = null;
            if (_theoryLessons != null && _theoryLessons.Count > 0)
            {
                if (!string.IsNullOrEmpty(SelectedLessonName))
                {
                    string normSelected = SelectedLessonName.Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();
                    matchedItem = _theoryLessons.FirstOrDefault(l =>
                    {
                        string normBai = (l.tenBai ?? "").Replace(":", "").Replace(".", "").Replace(" ", "").ToLower();
                        return normBai.Contains(normSelected) || normSelected.Contains(normBai);
                    });
                }

                if (matchedItem == null)
                {
                    string chId = $"chuong{SelectedChapterIndex}";
                    var inChap = _theoryLessons.Where(l => l.chuongId == chId).ToList();
                    if (inChap.Count > 0)
                    {
                        int subIdx = Mathf.Clamp(SelectedLessonIndex - 2, 0, inChap.Count - 1);
                        matchedItem = inChap[subIdx];
                    }
                }
            }

            List<TheoryPageData> pages = new List<TheoryPageData>();

            if (matchedItem != null)
            {
                // Trang 1: Khái niệm trọng tâm
                pages.Add(new TheoryPageData
                {
                    pageTitle = matchedItem.tenBai.ToUpper(),
                    mainContent = $"<b><color=#1E3A8A><size=20>{matchedItem.tenBai.ToUpper()}</size></color></b>\n\n" +
                                  $"<b><color=#B45309>I. KHÁI NIỆM & ĐỊNH NGHĨA TRỌNG TÂM:</color></b>\n\n" +
                                  $"{matchedItem.khaiNiemTrongTam}",
                    tipContent = "<b><color=#B45309>[ MẸO GHI NHỚ KHÁI NIỆM ]</color></b>\n\n" +
                                 "- Nắm vững từ khóa bản chất: chuyển động, vị trí, lực tác dụng, sự biến thiên năng lượng.\n\n" +
                                 "- Luôn chọn mốc quy chiếu rõ ràng khi xác định độ cao và thế năng."
                });

                // Trang 2: Công thức cần nhớ
                pages.Add(new TheoryPageData
                {
                    pageTitle = $"CÔNG THỨC - {matchedItem.tenBai.ToUpper()}",
                    mainContent = "<b><color=#1E3A8A><size=20>CÔNG THỨC & ĐƠN VỊ ĐO (HỆ SI)</size></color></b>\n\n" +
                                  "<b><color=#B45309>II. CÔNG THỨC TRỌNG TÂM CẦN NHỚ:</color></b>\n\n" +
                                  $"{matchedItem.congThucCanNho}",
                    tipContent = "<b><color=#B45309>[ MẸO TÍNH TOÁN & ĐỔI ĐƠN VỊ ]</color></b>\n\n" +
                                 "- Đổi về đơn vị chuẩn SI trước khi tính: m [kg], v [m/s], h [m], t [s], A [J], P [W].\n\n" +
                                 "- Chú ý: 1 kJ = 1000 J; 1 kW = 1000 W; 1 km/h = 1/3.6 m/s."
                });

                // Trang 3: Hiện tượng & Ứng dụng
                pages.Add(new TheoryPageData
                {
                    pageTitle = $"ỨNG DỤNG - {matchedItem.tenBai.ToUpper()}",
                    mainContent = "<b><color=#1E3A8A><size=20>HIỆN TƯỢNG VẬT LÝ & ĐỜI SỐNG</size></color></b>\n\n" +
                                  "<b><color=#B45309>III. HIỆN TƯỢNG & ỨNG DỤNG THỰC TẾ:</color></b>\n\n" +
                                  $"{matchedItem.hienTuongUngDung}",
                    tipContent = "<b><color=#B45309>[ LIÊN HỆ ĐỀ THI & GIẢI TOÁN ]</color></b>\n\n" +
                                 "- Các câu hỏi vận dụng thường lồng ghép hiện tượng đời sống (thủy điện, phanh xe, quang học).\n\n" +
                                 "- Hãy áp dụng công thức ở Trang 2 để giải thích hiện tượng ở Trang 3!"
                });

                return pages;
            }

            // Fallback 3 trang theo chương
            pages.Add(new TheoryPageData
            {
                pageTitle = SelectedChapterName.ToUpper(),
                mainContent = $"<b><color=#1E3A8A><size=20>{SelectedChapterName.ToUpper()}</size></color></b>\n\n" +
                              "<b><color=#B45309>I. KHÁI NIỆM TRỌNG TÂM CHƯƠNG:</color></b>\n\n" +
                              "1. Nắm chắc bản chất vật lý của các đại lượng và hiện tượng.\n" +
                              "2. Hiểu rõ mối liên hệ giữa các khái niệm cơ bản trong chương trình THCS.",
                tipContent = "<b><color=#B45309>[ MẸO HỌC NHANH ]</color></b>\n\n" +
                             "- Đọc kỹ định nghĩa từng bài học.\n- Vẽ sơ đồ tư duy để kết nối các kiến thức trọng tâm."
            });

            pages.Add(new TheoryPageData
            {
                pageTitle = $"CÔNG THỨC - {SelectedChapterName.ToUpper()}",
                mainContent = "<b><color=#1E3A8A><size=20>HỆ THỐNG CÔNG THỨC BÀI HỌC</size></color></b>\n\n" +
                              "<b><color=#B45309>II. CÁC CÔNG THỨC VÀ ĐƠN VỊ ĐO:</color></b>\n\n" +
                              "- Động năng: Wđ = 1/2 * m * v^2 [J]\n" +
                              "- Thế năng: Wt = m * g * h [J]\n" +
                              "- Định luật Ohm: I = U / R [A]\n" +
                              "- Công suất điện: P = U * I [W]",
                tipContent = "<b><color=#B45309>[ MẸO TÍNH TOÁN ]</color></b>\n\n" +
                             "- Lập bảng so sánh các công thức tương đương và đơn vị đo chuẩn trong hệ SI."
            });

            pages.Add(new TheoryPageData
            {
                pageTitle = $"ỨNG DỤNG - {SelectedChapterName.ToUpper()}",
                mainContent = "<b><color=#1E3A8A><size=20>HIỆN TƯỢNG VẬT LÝ TRONG ĐỜI SỐNG</size></color></b>\n\n" +
                              "<b><color=#B45309>III. ỨNG DỤNG THỰC TẾ:</color></b>\n\n" +
                              "- Ứng dụng trong các nhà máy nhiệt điện, thủy điện và năng lượng tái tạo.\n" +
                              "- Ứng dụng quang học trong thấu kính, máy ảnh, mắt và kính thiên văn.\n" +
                              "- Ứng dụng an toàn điện trong đời sống sinh hoạt gia đình.",
                tipContent = "<b><color=#B45309>[ LIÊN HỆ THỰC TIỄN ]</color></b>\n\n" +
                             "- Quan sát các hiện tượng thực tế xung quanh để hiểu sâu hơn lý thuyết trong sách giáo khoa."
            });

            return pages;
        }

        /// <summary>
        /// Lấy nội dung lý thuyết và mẹo ôn tập theo chương và bài học
        /// </summary>
        public (string theory, string tips) GetTheoryAndTipsForCurrentLesson()
        {
            var pages = GetTheoryPagesForCurrentLesson();
            if (pages != null && pages.Count > 0)
            {
                return (pages[0].mainContent, pages[0].tipContent);
            }
            return ("Lý thuyết bài học", "Mẹo ôn tập");
        }
    }
}
