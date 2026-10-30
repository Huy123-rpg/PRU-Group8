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
        public int MaxQuestionsForCurrentSession => (SelectedGameMode == "TuLuan") ? 5 : 10;
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

            // Lấy danh sách câu hỏi theo chương
            List<QuestionData> chapterQuestions = _dataBank.GetQuestionsByChapter(SelectedChapterName);
            if (chapterQuestions == null || chapterQuestions.Count == 0)
            {
                chapterQuestions = _dataBank.GetAllQuestions();
            }

            // Lọc theo chế độ chơi: Tự luận hoặc Trắc nghiệm
            bool isTuLuanMode = (SelectedGameMode == "TuLuan");
            List<QuestionData> modeQuestions = new List<QuestionData>();
            foreach (var q in chapterQuestions)
            {
                bool isQTuLuan = !string.IsNullOrEmpty(q.loaiCauHoi) &&
                                 (q.loaiCauHoi.IndexOf("tự luận", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  q.loaiCauHoi.IndexOf("tuluan", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  q.questionID.StartsWith("TL_", StringComparison.OrdinalIgnoreCase));

                if (isTuLuanMode && isQTuLuan) modeQuestions.Add(q);
                else if (!isTuLuanMode && !isQTuLuan) modeQuestions.Add(q);
            }
            if (modeQuestions.Count > 0)
            {
                chapterQuestions = modeQuestions;
            }

            // Lọc theo độ khó (Hỗ trợ cả mã tiếng Anh và nhãn tiếng Việt từ ngân hàng đề)
            List<QuestionData> filtered = new List<QuestionData>();
            foreach (var q in chapterQuestions)
            {
                string diff = (q.difficulty ?? "").ToLower();
                string mucDo = (q.mucDo ?? "").ToLower();
                bool match = false;

                if (SelectedLevel == "NhanBiet")
                {
                    match = (diff == "easy" || mucDo.Contains("nhận biết") || mucDo.Contains("dễ") || string.IsNullOrEmpty(diff));
                }
                else if (SelectedLevel == "ThongHieu")
                {
                    match = (diff == "medium" || mucDo.Contains("thông hiểu") || mucDo.Contains("trung bình"));
                }
                else if (SelectedLevel == "VanDung")
                {
                    match = (diff == "hard" || (mucDo.Contains("vận dụng") && !mucDo.Contains("cao")) || mucDo.Contains("khó"));
                }
                else if (SelectedLevel == "VanDungCao")
                {
                    match = (diff == "veryhard" || mucDo.Contains("cao") || mucDo.Contains("tổng hợp"));
                }

                if (match) filtered.Add(q);
            }
            int targetCount = MaxQuestionsForCurrentSession;

            // Nếu lọc ra ít hơn targetCount câu, bổ sung thêm từ chương để đủ số câu
            if (filtered.Count < targetCount)
            {
                foreach (var q in chapterQuestions)
                {
                    if (!filtered.Contains(q))
                    {
                        filtered.Add(q);
                        if (filtered.Count >= targetCount) break;
                    }
                }
            }

            // Xáo trộn ngẫu nhiên
            System.Random rnd = new System.Random();
            for (int i = filtered.Count - 1; i > 0; i--)
            {
                int k = rnd.Next(i + 1);
                var temp = filtered[i];
                filtered[i] = filtered[k];
                filtered[k] = temp;
            }

            int count = Mathf.Min(filtered.Count, targetCount);
            for (int i = 0; i < count; i++)
            {
                CurrentQuestions.Add(filtered[i]);
            }

            // Fallback nếu ngân hàng rỗng: tạo các câu mẫu chuẩn KHTN 8 / Lý 9
            if (CurrentQuestions.Count == 0)
            {
                CreateFallbackQuestions();
            }
        }

        private void CreateFallbackQuestions()
        {
            string[] samples = new string[]
            {
                "Động năng của một vật chuyển động phụ thuộc vào yếu tố nào?|Khối lượng và vận tốc của vật|Khối lượng và độ cao|Vận tốc và thể tích|Trọng lượng và màu sắc|0|Động năng Wđ = 1/2*m*v^2 phụ thuộc vào khối lượng và vận tốc.",
                "Đơn vị đo của công và năng lượng trong hệ SI là gì?|Jun (J)|Oát (W)|Niutơn (N)|Mã lực (HP)|0|Trong hệ đơn vị quốc tế SI, đơn vị của công và năng lượng là Jun (J).",
                "Hiện tượng khúc xạ ánh sáng là hiện tượng gì?|Tia sáng bị gãy khúc khi truyền qua mặt phân cách giữa hai môi trường trong suốt|Tia sáng bị hắt trở lại môi trường cũ|Tia sáng bị hấp thụ hoàn toàn|Tia sáng đổi màu|0|Khúc xạ ánh sáng là hiện tượng tia sáng bị gãy khúc khi truyền xiên góc qua mặt phân cách giữa 2 môi trường trong suốt.",
                "Công thức định luật Ohm cho đoạn mạch là gì?|I = U / R|U = I / R|R = U * I|I = U * R|0|Định luật Ohm: Cường độ dòng điện chạy qua dây dẫn tỉ lệ thuận với hiệu điện thế giữa hai đầu dây và tỉ lệ nghịch với điện trở của dây: I = U/R.",
                "Khi đưa thanh nam châm lại gần một cuộn dây dẫn kín thì trong cuộn dây xuất hiện:?|Dòng điện cảm ứng|Lực đẩy vĩnh cửu|Nhiệt độ giảm mạnh|Hiện tượng nhiễm xạ|0|Sự biến thiên số đường sức từ qua tiết diện cuộn dây làm sinh ra dòng điện cảm ứng."
            };

            int targetCount = MaxQuestionsForCurrentSession;
            for (int i = 0; i < targetCount; i++)
            {
                string raw = samples[i % samples.Length];
                string[] parts = raw.Split('|');
                QuestionData q = new QuestionData
                {
                    questionID = "FB_PHY_" + (i + 1),
                    questionText = parts[0],
                    answers = new string[] { parts[1], parts[2], parts[3], parts[4] },
                    correctAnswerIndex = int.Parse(parts[5]),
                    chapter = SelectedChapterName,
                    difficulty = "Easy",
                    explanation = parts[6]
                };
                CurrentQuestions.Add(q);
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
