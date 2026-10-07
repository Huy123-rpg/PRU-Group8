using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

[System.Serializable]
public class Question
{
    public string questionId = "";
    public string chapterId = "";
    public string lessonId = "";
    public string questionText = "";
    public string ansA = "", ansB = "", ansC = "", ansD = "";
    public string correctAnswer = "";
    public string mode = "TracNghiem"; // TracNghiem hoặc TuLuan
    public string keywordsTuLuan = "";
    public string explain = "";
    public string difficulty = "Easy";
    public string hinhAnh = "";
    public float timeLimit = 15f;
    public float hpBonus = 20f;
}

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Dữ liệu Excel (CSV / Google Sheet)")]
    [Tooltip("File CSV câu hỏi (hỗ trợ cả định dạng mới từ Game Hub và định dạng cũ)")]
    public TextAsset questionDataCSV;

    [Tooltip("Đường dẫn Google Sheet CSV trực tuyến (tùy chọn, cập nhật câu hỏi online)")]
    public string googleSheetCsvUrl = "";

    [Header("Cài đặt Lọc Câu Hỏi")]
    [Tooltip("Lọc câu hỏi theo Chương và Bài học đã chọn từ Menu Sinh học")]
    public bool filterByChapterAndLesson = true;

    [Header("Cài đặt Số lượng câu hỏi")]
    public int soCauHoiCanTraLoi = 5;
    private int soCauDaHoi = 0; // Đếm TỔNG số câu đã hỏi (Đúng/Sai/Hết giờ đều đếm)

    [Header("Giao diện UI")]
    public GameObject panelCuonVo;
    public TextMeshProUGUI questionTextUI;
    public TMP_InputField answerInputField;
    public TextMeshProUGUI textA;
    public TextMeshProUGUI textB;
    public TextMeshProUGUI textC;
    public TextMeshProUGUI textD;

    [Header("Tương tác Button & Màu sắc")]
    public Button[] nutDapAn;
    public Color mauChuBinhThuong = Color.black;
    public Color mauChuKhiChon = Color.blue;

    [Header("Cài đặt Thời gian")]
    public TextMeshProUGUI timerTextUI;
    public float thoiGianMoiCau = 15f;
    private float thoiGianHienTai;
    private bool dangDemNguoc = false;

    [Header("Cài đặt Máu (Health)")]
    public Slider thanhMauUI;
    public float maxMau = 100f;

    [Tooltip("Phần trăm máu ĐƯỢC CỘNG khi trả lời ĐÚNG")]
    public float phanTramCongMau = 20f; // Mặc định hồi 20% máu khi đúng

    private float mauHienTai;

    [Header("Hệ thống Điểm & Ranking")]
    public int currentScore = 0;
    public TextMeshProUGUI scoreTextUI;
    private int bossesDefeatedCount = 0;
    private GameObject gameOverPopup;

    // Ngân hàng toàn bộ câu hỏi đã parse
    private List<Question> masterQuestionList = new List<Question>();
    // Danh sách câu hỏi đang chơi trong màn hiện tại
    private List<Question> questionList = new List<Question>();
    private int currentQuestionIndex = 0;
    private GameObject currentBoss;

    private bool daChotDapAn = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        mauHienTai = maxMau;
        if (thanhMauUI != null)
        {
            thanhMauUI.maxValue = maxMau;
            thanhMauUI.value = mauHienTai;
        }

        if (panelCuonVo != null) panelCuonVo.SetActive(false);

        // Khởi tạo HUD hiển thị điểm tích lũy
        CreateScoreUIHUD();

        // Nạp câu hỏi từ CSV cục bộ trước
        LoadQuestionsFromCSV();

        // Nếu có link Google Sheet online thì nạp cập nhật từ Google Sheet
        if (!string.IsNullOrEmpty(googleSheetCsvUrl))
        {
            StartCoroutine(FetchQuestionsFromGoogleSheet(googleSheetCsvUrl));
        }
    }

    private void Update()
    {
        if (dangDemNguoc && !daChotDapAn)
        {
            thoiGianHienTai -= Time.unscaledDeltaTime;

            if (timerTextUI != null) timerTextUI.text = Mathf.CeilToInt(thoiGianHienTai).ToString() + "s";

            if (thoiGianHienTai <= 0)
            {
                dangDemNguoc = false;
                daChotDapAn = true;
                if (timerTextUI != null) timerTextUI.text = "0s";

                string loiGiaiThich = (currentQuestionIndex < questionList.Count) 
                    ? questionList[currentQuestionIndex].explain 
                    : "";
                if (answerInputField != null)
                {
                    answerInputField.text = "HẾT GIỜ! " + loiGiaiThich;
                }

                Debug.Log("[QuizManager] ⏰ Đã hết giờ! Trừ máu và chuyển câu...");
                TruMau();
                if (mauHienTai > 0) StartCoroutine(DoiChuyenCau(false));
            }
        }
    }

    #region ===== NẠP & XỬ LÝ DỮ LIỆU CÂU HỎI MỚI =====

    /// <summary>
    /// Nạp câu hỏi từ file CSV / TextAsset hỗ trợ cả cấu trúc mới từ file Excel Game Hub
    /// và định dạng câu hỏi cũ (7 cột).
    /// </summary>
    void LoadQuestionsFromCSV()
    {
        if (questionDataCSV == null)
        {
            // Dự phòng: Thử tìm trong thư mục Resources
            questionDataCSV = Resources.Load<TextAsset>("QuizData/question");
            if (questionDataCSV == null)
            {
                questionDataCSV = Resources.Load<TextAsset>("question");
            }
        }

        if (questionDataCSV == null)
        {
            Debug.LogWarning("[QuizManager] ⚠️ Chưa gán file CSV câu hỏi vào Inspector. Đang chờ dữ liệu hoặc Google Sheet.");
            return;
        }

        ParseRawData(questionDataCSV.text);
    }

    /// <summary>
    /// Đồng bộ câu hỏi online từ link Google Sheet CSV.
    /// </summary>
    private IEnumerator FetchQuestionsFromGoogleSheet(string url)
    {
        Debug.Log("[QuizManager] 🌐 Đang tải ngân hàng câu hỏi mới từ Google Sheet: " + url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string downloadedCsv = www.downloadHandler.text;
                Debug.Log("[QuizManager] ✅ Đã tải thành công câu hỏi từ Google Sheet!");
                ParseRawData(downloadedCsv);
            }
            else
            {
                Debug.LogWarning($"[QuizManager] ❌ Lỗi nạp Google Sheet: {www.error}. Giữ nguyên dữ liệu từ file local.");
            }
        }
    }

    /// <summary>
    /// Parse chuỗi CSV thành danh sách câu hỏi.
    /// Tự động nhận diện tên cột theo chuẩn mới (Question_ID, Chapter_ID, Question_Text, Option_A...)
    /// hoặc fallback theo vị trí cột truyền thống.
    /// </summary>
    public void ParseRawData(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return;

        masterQuestionList.Clear();

        string[] dataLines = rawText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (dataLines.Length == 0) return;

        // Phân tích dòng tiêu đề (Header Row)
        List<string> headers = ParseCsvLine(dataLines[0]);

        int colId = FindCol(headers, "cauhoid", "cauhoi_id", "question_id", "id", "mã câu", "macau");
        int colChapter = FindCol(headers, "chudeid", "chude_id", "chude", "chapter_id", "chapter", "chủ đề", "chương");
        int colLesson = FindCol(headers, "baiid", "bai_id", "lesson_id", "lesson", "bài", "mã bài");
        int colQuestion = FindCol(headers, "noidung", "noidungcauhoi", "cauhoi", "question_text", "question", "câu hỏi", "nội dung");
        int colA = FindCol(headers, "dapana", "dapan_a", "option_a", "ans_a", "ans a", "a", "đáp án a");
        int colB = FindCol(headers, "dapanb", "dapan_b", "option_b", "ans_b", "ans b", "b", "đáp án b");
        int colC = FindCol(headers, "dapanc", "dapan_c", "option_c", "ans_c", "ans c", "c", "đáp án c");
        int colD = FindCol(headers, "dapand", "dapan_d", "option_d", "ans_d", "ans d", "d", "đáp án d");
        int colCorrect = FindCol(headers, "dapandung", "dapan_dung", "correct_answer", "answer", "đáp án đúng", "đáp án");
        int colMode = FindCol(headers, "loai", "mode", "chế độ", "hình thức");
        int colKeywords = FindCol(headers, "dapanmau", "goiydapan", "tukhoa", "keywords_tuluan", "keywords", "từ khóa", "huongdancham");
        int colExplain = FindCol(headers, "giaithich", "giaithichai", "loigiai", "explanation", "explain", "giải thích", "lời giải");
        int colDiff = FindCol(headers, "dokho", "mucdo", "difficulty", "độ khó", "mức độ");
        int colImage = FindCol(headers, "hinhanh", "hinh_anh", "image", "ảnh");
        int colTime = FindCol(headers, "time_limit_sec", "time_limit", "thời gian");
        int colHpBonus = FindCol(headers, "hp_bonus_percent", "hp_bonus", "cộng máu");

        // Nếu file CSV cũ (không có header hoặc header chỉ có Question,A,B,C,D,Answer,Explain)
        bool hasNamedHeaders = (colQuestion >= 0 || (colA >= 0 && colCorrect >= 0));
        if (!hasNamedHeaders)
        {
            colQuestion = 0;
            colA = 1;
            colB = 2;
            colC = 3;
            colD = 4;
            colCorrect = 5;
            colExplain = 6;
        }

        int startIndex = 1;
        // Kiểm tra xem dòng 0 có phải là header thật hay là câu hỏi
        string line0 = dataLines[0].ToLower();
        if (!line0.Contains("question") && !line0.Contains("câu hỏi") && !line0.Contains("answer") && !line0.Contains("đáp án") && !line0.Contains("noidung"))
        {
            startIndex = 0; // File không có header
        }

        for (int i = startIndex; i < dataLines.Length; i++)
        {
            List<string> cols = ParseCsvLine(dataLines[i]);
            if (cols.Count == 0) continue;

            string qText = GetVal(cols, colQuestion);
            if (string.IsNullOrEmpty(qText)) continue;

            Question q = new Question();
            q.questionId = GetVal(cols, colId, $"BIO_Q_{i}");
            q.chapterId = GetVal(cols, colChapter, "");
            q.lessonId = GetVal(cols, colLesson, "");
            q.questionText = qText;
            q.ansA = GetVal(cols, colA);
            q.ansB = GetVal(cols, colB);
            q.ansC = GetVal(cols, colC);
            q.ansD = GetVal(cols, colD);
            q.correctAnswer = GetVal(cols, colCorrect);
            q.mode = GetVal(cols, colMode, "TracNghiem");
            q.keywordsTuLuan = GetVal(cols, colKeywords);
            q.explain = GetVal(cols, colExplain, "(Không có giải thích chi tiết)");
            q.difficulty = GetVal(cols, colDiff, "Nhận biết");
            q.hinhAnh = GetVal(cols, colImage, "");

            float tLimit;
            if (float.TryParse(GetVal(cols, colTime), out tLimit) && tLimit > 0) q.timeLimit = tLimit;
            else q.timeLimit = thoiGianMoiCau;

            float hp;
            if (float.TryParse(GetVal(cols, colHpBonus), out hp) && hp > 0) q.hpBonus = hp;
            else q.hpBonus = phanTramCongMau;

            masterQuestionList.Add(q);
        }

        // Bổ sung ngân hàng câu hỏi Tự luận sinh học nếu file nguồn chỉ có trắc nghiệm
        EnsureTuLuanQuestionBank();

        Debug.Log($"[QuizManager] 📚 Đã tải thành công {masterQuestionList.Count} câu hỏi vào ngân hàng tổng.");

        // Lọc câu hỏi theo Chương & Bài đang chọn
        FilterCurrentQuestions();
    }

    private void EnsureTuLuanQuestionBank()
    {
        bool hasTuLuan = masterQuestionList.Exists(q => q.mode.Equals("TuLuan", System.StringComparison.OrdinalIgnoreCase));
        if (hasTuLuan) return;

        Debug.Log("[QuizManager] 📝 Bổ sung ngân hàng câu hỏi Tự Luận sinh học từ NganHangTuLuan...");

        AddTuLuanQuestion("TL6_001", "B6_002", "CD6_SH_01",
            "Trình bày cấu tạo cơ bản của một tế bào nhân thực và nêu chức năng của màng sinh chất.",
            "Tế bào nhân thực gồm 3 thành phần chính: màng sinh chất, tế bào chất và nhân. Màng sinh chất bảo vệ và kiểm soát trao đổi chất giữa tế bào với môi trường.",
            "màng sinh chất;tế bào chất;nhân;trao đổi chất;bảo vệ;kiểm soát",
            "Màng sinh chất kiểm soát các chất ra vào tế bào một cách chọn lọc.");

        AddTuLuanQuestion("TL6_002", "B6_002", "CD6_SH_01",
            "Vì sao ty thể được ví như 'nhà máy năng lượng' của tế bào nhân thực?",
            "Vì ty thể là nơi diễn ra quá trình hô hấp tế bào, phân giải chất hữu cơ để tổng hợp phần lớn phân tử ATP cung cấp năng lượng cho mọi hoạt động sống.",
            "ty thể;năng lượng;atp;hô hấp tế bào;chuyển hóa",
            "Ty thể thực hiện chu trình chuyển hóa tạo phân tử ATP giàu năng lượng.");

        AddTuLuanQuestion("TL7_001", "B7_001", "CD7_SH_01",
            "Nêu vai trò của quá trình quang hợp ở thực vật đối với bầu khí quyển và sự sống trên Trái Đất.",
            "Quang hợp hấp thụ CO2 và giải phóng O2 giúp cân bằng khí quyển, đồng thời tổng hợp các chất hữu cơ làm thức ăn cho sinh vật.",
            "quang hợp;oxy;o2;co2;chất hữu cơ;khí quyển;năng lượng ánh sáng",
            "Quang hợp cung cấp khí oxy và là nguồn thức ăn sơ cấp cho sinh giới.");

        AddTuLuanQuestion("TL8_001", "B8_001", "CD8_SH_01",
            "Giải thích vì sao tâm thất trái của tim người lại có thành cơ dày nhất?",
            "Tâm thất trái phải co bóp tạo một áp lực rất lớn để tống máu vào động mạch chủ đi khắp vòng tuần hoàn lớn nuôi toàn bộ cơ thể.",
            "tâm thất trái;vòng tuần hoàn lớn;áp lực;co bóp;động mạch chủ;toàn cơ thể",
            "Thành cơ tâm thất trái dày nhất để chịu được lực co bóp lớn tống máu nuôi toàn thân.");

        AddTuLuanQuestion("TL9_001", "B9_002", "CD9_SH_01",
            "Định nghĩa gen là gì và phân tích vai trò của gen trong việc quy định tính trạng của sinh vật.",
            "Gen là một đoạn phân tử ADN mang thông tin di truyền mã hóa cho chuỗi polypeptide hoặc phân tử ARN, từ đó biểu hiện thành tính trạng cơ thể.",
            "gen;adn;thông tin di truyền;polypeptide;arn;tính trạng;protein",
            "Gen chứa thông tin di truyền mã hóa cho protein, quy định các tính trạng.");

        AddTuLuanQuestion("TL9_002", "B9_002", "CD9_SH_01",
            "Phân biệt điểm khác nhau cơ bản giữa đột biến gen và đột biến nhiễm sắc thể.",
            "Đột biến gen là những biến đổi trong cấu trúc phân tử của gen liên quan đến 1 hoặc một số cặp nucleotide. Đột biến NST là biến đổi về cấu trúc hoặc số lượng nhiễm sắc thể ở cấp độ tế bào.",
            "đột biến gen;nhiễm sắc thể;cấu trúc;số lượng;nucleotide;phân tử;tế bào",
            "Đột biến gen xảy ra ở cấp độ phân tử ADN, đột biến NST xảy ra ở cấp độ tế bào.");
    }

    private void AddTuLuanQuestion(string qId, string bId, string cdId, string text, string correctAns, string keywords, string explain)
    {
        Question q = new Question();
        q.questionId = qId;
        q.lessonId = bId;
        q.chapterId = cdId;
        q.questionText = text;
        q.ansA = "";
        q.ansB = "";
        q.ansC = "";
        q.ansD = "";
        q.correctAnswer = correctAns;
        q.mode = "TuLuan";
        q.keywordsTuLuan = keywords;
        q.explain = explain;
        q.difficulty = "Thông hiểu";
        q.timeLimit = 30f;
        q.hpBonus = 25f;
        masterQuestionList.Add(q);
    }

    /// <summary>
    /// Lọc câu hỏi từ masterQuestionList dựa trên chương và bài học được người chơi chọn từ Menu Sinh học.
    /// </summary>
    public void FilterCurrentQuestions()
    {
        questionList.Clear();

        string selectedChapter = PlayerPrefs.GetString("Sinh_SelectedChapter", "");
        string selectedLesson = PlayerPrefs.GetString("Sinh_SelectedLesson", "");
        string selectedMode = PlayerPrefs.GetString("Sinh_QuizMode", "TracNghiem");

        Debug.Log($"[QuizManager] 🎯 Lọc câu hỏi: Chương='{selectedChapter}', Bài='{selectedLesson}', Chế độ='{selectedMode}'");

        if (filterByChapterAndLesson && (!string.IsNullOrEmpty(selectedChapter) || !string.IsNullOrEmpty(selectedLesson)))
        {
            foreach (var q in masterQuestionList)
            {
                bool matchChapter = IsChapterMatch(q.chapterId, q.lessonId, q.questionText, selectedChapter);
                bool matchLesson = IsLessonMatch(q.lessonId, selectedLesson);

                bool matchMode = q.mode.Equals(selectedMode, System.StringComparison.OrdinalIgnoreCase);

                if (matchChapter && matchLesson && matchMode)
                {
                    questionList.Add(q);
                }
            }
        }

        // Nếu bộ lọc chi tiết chưa có câu, nạp các câu hỏi có cùng Chế độ (Trắc nghiệm hoặc Tự luận)
        if (questionList.Count == 0)
        {
            foreach (var q in masterQuestionList)
            {
                if (q.mode.Equals(selectedMode, System.StringComparison.OrdinalIgnoreCase))
                {
                    questionList.Add(q);
                }
            }
        }

        // Dự phòng an toàn: Nếu vẫn = 0 câu hỏi, nạp toàn bộ để người chơi không bị crash/đóng băng game
        if (questionList.Count == 0)
        {
            Debug.LogWarning("[QuizManager] ⚠️ Bộ lọc không tìm thấy câu hỏi tương thích cụ thể, nạp toàn bộ câu hỏi từ ngân hàng.");
            questionList.AddRange(masterQuestionList);
        }

        Debug.Log($"[QuizManager] ✅ Danh sách câu hỏi sẵn sàng cho màn chơi: {questionList.Count} câu (Chế độ: {selectedMode}).");
    }

    private bool IsChapterMatch(string chapterId, string lessonId, string questionText, string selectedChapter)
    {
        if (string.IsNullOrEmpty(selectedChapter)) return true;
        string normChap = NormalizeSearchKey(selectedChapter);

        if (!string.IsNullOrEmpty(chapterId))
        {
            string normCId = NormalizeSearchKey(chapterId);
            if (normCId.Contains(normChap) || normChap.Contains(normCId)) return true;
        }

        string lId = lessonId.ToUpper();
        if (normChap.Contains("ditruyen") || normChap.Contains("chuongi") || normChap.Contains("chuong1"))
        {
            if (lId.StartsWith("B9_") || lId.Contains("CD9") || questionText.ToLower().Contains("di truyền") || questionText.ToLower().Contains("adn")) return true;
        }
        else if (normChap.Contains("sinhthai") || normChap.Contains("traodoichat") || normChap.Contains("chuongii") || normChap.Contains("chuong2"))
        {
            if (lId.StartsWith("B7_") || lId.Contains("CD7") || questionText.ToLower().Contains("quang hợp") || questionText.ToLower().Contains("sinh thái")) return true;
        }
        else if (normChap.Contains("cothe") || normChap.Contains("chuongiii") || normChap.Contains("chuong3"))
        {
            if (lId.StartsWith("B8_") || lId.Contains("CD8") || questionText.ToLower().Contains("tim") || questionText.ToLower().Contains("máu") || questionText.ToLower().Contains("phổi")) return true;
        }
        else if (normChap.Contains("tebao") || normChap.Contains("chuongiv") || normChap.Contains("chuong4"))
        {
            if (lId.StartsWith("B6_") || lId.Contains("CD6") || questionText.ToLower().Contains("tế bào")) return true;
        }

        return false;
    }

    private bool IsLessonMatch(string lessonId, string selectedLesson)
    {
        if (string.IsNullOrEmpty(selectedLesson) || string.IsNullOrEmpty(lessonId)) return true;
        string normLId = NormalizeSearchKey(lessonId);
        string normSel = NormalizeSearchKey(selectedLesson);

        if (normLId.Contains(normSel) || normSel.Contains(normLId)) return true;

        for (int num = 1; num <= 9; num++)
        {
            bool selHasNum = normSel.Contains("bai" + num) || normSel.Contains("btnbai" + num) || normSel.EndsWith(num.ToString());
            bool idHasNum = normLId.EndsWith("00" + num) || normLId.EndsWith("0" + num) || normLId.EndsWith(num.ToString());
            if (selHasNum && idHasNum) return true;
        }

        if ((normSel.Contains("ontap") || normSel.Contains("review")) && (normLId.Contains("ontap") || normLId.Contains("rev") || normLId.Contains("005") || normLId.Contains("004")))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region ===== GAMEPLAY QUIZ & ĐẤU BOSS =====

    public void ShowQuiz(GameObject boss)
    {
        Debug.Log("[QuizManager] ⚔️ Mở bảng Quiz chiến đấu với Boss!");

        currentBoss = boss;
        soCauDaHoi = 0; // Reset số câu đã hỏi về 0 khi gặp Boss mới

        // Đảm bảo luôn có câu hỏi trước khi mở Quiz (Hỗ trợ vòng lặp vô tận Endless loop)
        if (questionList.Count < soCauHoiCanTraLoi)
        {
            FilterCurrentQuestions();
            if (questionList.Count < soCauHoiCanTraLoi)
            {
                ReplenishAnyAvailableQuestions();
            }
        }

        Time.timeScale = 0f;
        if (panelCuonVo != null) panelCuonVo.SetActive(true);

        HienThiCauHoiMoi();
    }

    private void HienThiCauHoiMoi()
    {
        if (questionList.Count == 0)
        {
            FilterCurrentQuestions();
            if (questionList.Count == 0)
            {
                ReplenishAnyAvailableQuestions();
            }
        }

        if (questionList.Count == 0)
        {
            ThangBoss();
            return;
        }

        daChotDapAn = false;
        currentQuestionIndex = Random.Range(0, questionList.Count);
        ResetHieuUngTatCaNut();
        DisplayQuestion(currentQuestionIndex);

        Question q = questionList[currentQuestionIndex];
        thoiGianHienTai = q.timeLimit > 0 ? q.timeLimit : thoiGianMoiCau;
        dangDemNguoc = true;
    }

    void DisplayQuestion(int index)
    {
        Question q = questionList[index];
        if (questionTextUI != null)
        {
            questionTextUI.text = q.questionText;
            questionTextUI.color = new Color(0.12f, 0.12f, 0.12f, 1f); // Màu đen than đậm nét trên giấy
            questionTextUI.fontStyle = FontStyles.Bold;
            questionTextUI.enableAutoSizing = false;
            questionTextUI.fontSize = 28f;
        }

        bool isTuLuan = q.mode.Equals("TuLuan", System.StringComparison.OrdinalIgnoreCase);

        // Đảm bảo các nút A, B, C, D luôn hiển thị đầy đủ
        if (nutDapAn != null)
        {
            foreach (var btn in nutDapAn)
            {
                if (btn != null) btn.gameObject.SetActive(true);
            }
        }
        if (textA != null && textA.transform.parent != null) textA.transform.parent.gameObject.SetActive(true);
        if (textB != null && textB.transform.parent != null) textB.transform.parent.gameObject.SetActive(true);
        if (textC != null && textC.transform.parent != null) textC.transform.parent.gameObject.SetActive(true);
        if (textD != null && textD.transform.parent != null) textD.transform.parent.gameObject.SetActive(true);

        if (textA != null) { textA.text = q.ansA; textA.color = mauChuBinhThuong; textA.enableAutoSizing = false; textA.fontSize = 24f; }
        if (textB != null) { textB.text = q.ansB; textB.color = mauChuBinhThuong; textB.enableAutoSizing = false; textB.fontSize = 24f; }
        if (textC != null) { textC.text = q.ansC; textC.color = mauChuBinhThuong; textC.enableAutoSizing = false; textC.fontSize = 24f; }
        if (textD != null) { textD.text = q.ansD; textD.color = mauChuBinhThuong; textD.enableAutoSizing = false; textD.fontSize = 24f; }

        if (answerInputField != null)
        {
            answerInputField.gameObject.SetActive(true);
            answerInputField.text = "";
            answerInputField.interactable = true;

            // Ẩn hoàn toàn chữ mờ placeholder phía dưới để tránh vướng mắt người chơi
            if (answerInputField.placeholder != null)
            {
                answerInputField.placeholder.gameObject.SetActive(false);
            }
        }
    }

    public void OnAnswerButtonClicked(string selectedAnswer)
    {
        Debug.Log("Bạn vừa click đáp án: [" + selectedAnswer + "]");

        if (daChotDapAn)
        {
            Debug.Log("Bảng đã khóa chốt đáp án, từ chối click!");
            return;
        }

        if (answerInputField != null)
        {
            answerInputField.text = selectedAnswer;
        }

        ResetHieuUngTatCaNut();

        if (selectedAnswer == "A" && textA != null) { textA.color = mauChuKhiChon; textA.fontStyle = FontStyles.Bold; }
        else if (selectedAnswer == "B" && textB != null) { textB.color = mauChuKhiChon; textB.fontStyle = FontStyles.Bold; }
        else if (selectedAnswer == "C" && textC != null) { textC.color = mauChuKhiChon; textC.fontStyle = FontStyles.Bold; }
        else if (selectedAnswer == "D" && textD != null) { textD.color = mauChuKhiChon; textD.fontStyle = FontStyles.Bold; }
    }

    private void ResetHieuUngTatCaNut()
    {
        if (textA != null) { textA.color = mauChuBinhThuong; textA.fontStyle = FontStyles.Normal; }
        if (textB != null) { textB.color = mauChuBinhThuong; textB.fontStyle = FontStyles.Normal; }
        if (textC != null) { textC.color = mauChuBinhThuong; textC.fontStyle = FontStyles.Normal; }
        if (textD != null) { textD.color = mauChuBinhThuong; textD.fontStyle = FontStyles.Normal; }
    }

    public void OnSubmitClicked()
    {
        Debug.Log("Bạn vừa bấm nút GỬI!");

        if (daChotDapAn) return;
        if (answerInputField == null || string.IsNullOrEmpty(answerInputField.text.Trim()))
        {
            Debug.LogWarning("Chưa chọn hoặc nhập đáp án nào, không cho gửi!");
            return;
        }

        daChotDapAn = true;
        dangDemNguoc = false;

        Question currentQ = questionList[currentQuestionIndex];
        string userAnswer = answerInputField.text.Trim();
        string correctAnswer = currentQ.correctAnswer.Trim();
        string loiGiaiThich = currentQ.explain;

        bool isCorrect = false;

        // Xử lý kiểm tra đáp án Trắc Nghiệm hoặc Tự Luận
        if (currentQ.mode.Equals("TuLuan", System.StringComparison.OrdinalIgnoreCase))
        {
            // Tự luận: So sánh với đáp án chuẩn hoặc kiểm tra từ khóa
            isCorrect = CheckTuLuanAnswer(userAnswer, correctAnswer, currentQ.keywordsTuLuan);
        }
        else
        {
            // Trắc nghiệm: So sánh ký tự A/B/C/D hoặc chuỗi đáp án (không phân biệt hoa thường)
            isCorrect = userAnswer.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase);
        }

        if (isCorrect)
        {
            // Yêu cầu 2: Mỗi câu đúng cộng 10 điểm tích lũy
            currentScore += 10;
            UpdateScoreUI();
            SaveScoreAndRanking();

            answerInputField.text = "ĐÚNG! " + loiGiaiThich;

            // Hồi máu theo cấu hình câu hỏi hoặc tỷ lệ mặc định 20%
            float bonusHp = currentQ.hpBonus > 0 ? currentQ.hpBonus : phanTramCongMau;
            CongMau(bonusHp);

            StartCoroutine(DoiChuyenCau(true));
        }
        else
        {
            answerInputField.text = "SAI! " + loiGiaiThich;
            TruMau();
            if (mauHienTai > 0) StartCoroutine(DoiChuyenCau(false));
        }
    }

    private bool CheckTuLuanAnswer(string userAns, string correctAns, string keywords)
    {
        string normUser = userAns.ToLower().Trim();
        string normCorrect = correctAns.ToLower().Trim();

        if (normUser.Contains(normCorrect) || normCorrect.Contains(normUser)) return true;

        if (!string.IsNullOrEmpty(keywords))
        {
            string[] kwList = keywords.Split(new[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            int matchCount = 0;
            foreach (var kw in kwList)
            {
                if (normUser.Contains(kw.Trim().ToLower())) matchCount++;
            }
            if (matchCount > 0) return true;
        }

        return false;
    }

    IEnumerator DoiChuyenCau(bool laCauDung)
    {
        yield return new WaitForSecondsRealtime(3.5f);

        if (currentQuestionIndex < questionList.Count)
        {
            questionList.RemoveAt(currentQuestionIndex);
        }

        soCauDaHoi++;

        // Yêu cầu 3: Hoàn thành số câu cho 1 Boss (mặc định 5 câu)
        if (soCauDaHoi >= soCauHoiCanTraLoi)
        {
            ThangBoss();
            yield break;
        }

        // Nếu danh sách câu hỏi tạm thời hết nhưng chưa đủ số câu cho Boss, bổ sung ngay từ ngân hàng
        if (questionList.Count == 0)
        {
            FilterCurrentQuestions();
            if (questionList.Count == 0)
            {
                ReplenishAnyAvailableQuestions();
            }
        }

        HienThiCauHoiMoi();
    }

    public void CongMau(float phanTram)
    {
        float luongMauCong = maxMau * (phanTram / 100f);
        mauHienTai += luongMauCong;

        if (mauHienTai > maxMau) mauHienTai = maxMau;

        if (thanhMauUI != null) thanhMauUI.value = mauHienTai;
        Debug.Log($"[QuizManager] 💚 Thưởng: Được cộng {phanTram}% máu! (Hiện tại: {mauHienTai}/{maxMau})");
    }

    public void TruMau(float phanTram = 10f)
    {
        float luongMauTru = maxMau * (phanTram / 100f);
        mauHienTai -= luongMauTru;

        if (thanhMauUI != null) thanhMauUI.value = mauHienTai;
        Debug.Log($"[QuizManager] 💔 Bị trừ {phanTram}% máu! (Còn lại: {mauHienTai}/{maxMau})");

        if (mauHienTai <= 0)
        {
            mauHienTai = 0;
            if (thanhMauUI != null) thanhMauUI.value = 0;

            dangDemNguoc = false;
            if (panelCuonVo != null) panelCuonVo.SetActive(false);
            Time.timeScale = 0f;

            Debug.Log($"[QuizManager] 💀 GAME OVER! Bạn đã hết máu. Tổng điểm đạt được: {currentScore}");

            // Lưu điểm kỷ lục và lưu lịch sử để xếp hạng ranking
            SaveScoreAndRanking();

            // Hiển thị Popup Hết Máu có 2 nút: Chơi lại & Thoát
            ShowGameOverPopup();
        }
    }

    private void ThangBoss()
    {
        bossesDefeatedCount++;
        Debug.Log($"[QuizManager] 🎉 CHIẾN THẮNG BOSS #{bossesDefeatedCount}! Đã vượt qua thử thách. Map tiếp tục chạy vô tận.");
        dangDemNguoc = false;
        if (currentBoss != null) Destroy(currentBoss);
        if (panelCuonVo != null) panelCuonVo.SetActive(false);
        Time.timeScale = 1f; // Tiếp tục chạy để vượt qua các boss tiếp theo

        // Chuẩn bị sẵn ngân hàng câu hỏi cho Boss tiếp theo trong chu kỳ vô tận
        if (questionList.Count < soCauHoiCanTraLoi)
        {
            FilterCurrentQuestions();
            if (questionList.Count < soCauHoiCanTraLoi)
            {
                ReplenishAnyAvailableQuestions();
            }
        }
    }

    private void ReplenishAnyAvailableQuestions()
    {
        string mode = PlayerPrefs.GetString("Sinh_QuizMode", "TracNghiem");
        var filtered = masterQuestionList.FindAll(q => q.mode.Equals(mode, System.StringComparison.OrdinalIgnoreCase));
        if (filtered.Count > 0)
        {
            questionList.AddRange(filtered);
        }
        else
        {
            questionList.AddRange(masterQuestionList);
        }
        Debug.Log($"[QuizManager] 🔄 Đã nạp lại {questionList.Count} câu hỏi cho vòng lặp kế tiếp (Chế độ: {mode})");
    }

    #region ===== HUD ĐIỂM, LỊCH SỬ XẾP HẠNG & GAME OVER POPUP =====

    private void CreateScoreUIHUD()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Transform existing = canvas.transform.Find("ScoreHUD_Sinh");
        if (existing != null)
        {
            scoreTextUI = existing.GetComponentInChildren<TextMeshProUGUI>();
            UpdateScoreUI();
            return;
        }

        GameObject scoreObj = new GameObject("ScoreHUD_Sinh");
        scoreObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = scoreObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20f, -20f);
        rt.sizeDelta = new Vector2(170f, 45f);

        Image bg = scoreObj.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.16f, 0.14f, 0.85f); // Nền tối bo nhẹ sang trọng

        GameObject textObj = new GameObject("Txt_Score");
        textObj.transform.SetParent(scoreObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(8f, 2f);
        textRt.offsetMax = new Vector2(-8f, -2f);

        scoreTextUI = textObj.AddComponent<TextMeshProUGUI>();
        scoreTextUI.alignment = TextAlignmentOptions.Center;
        scoreTextUI.fontSize = 20f;
        scoreTextUI.fontStyle = FontStyles.Bold;
        scoreTextUI.color = new Color(1f, 0.88f, 0.25f, 1f); // Màu vàng gold lấp lánh

        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreTextUI != null)
        {
            scoreTextUI.text = $"🏆 ĐIỂM: {currentScore}";
        }
    }

    public void SaveScoreAndRanking()
    {
        // 1. Lưu High Score
        int highScore = PlayerPrefs.GetInt("Sinh_HighScore", 0);
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("Sinh_HighScore", highScore);
        }
        PlayerPrefs.SetInt("Sinh_LastScore", currentScore);

        // 2. Lưu lịch sử điểm để ranking: "score|date|mode#..."
        string historyRaw = PlayerPrefs.GetString("Sinh_ScoreHistory", "");
        string mode = PlayerPrefs.GetString("Sinh_QuizMode", "TracNghiem");
        string lesson = PlayerPrefs.GetString("Sinh_SelectedLesson", "Sinh học");
        string newEntry = $"{currentScore}|{System.DateTime.Now:dd/MM/yyyy HH:mm}|{mode}|{lesson}";

        if (!string.IsNullOrEmpty(historyRaw))
        {
            historyRaw = newEntry + "#" + historyRaw;
        }
        else
        {
            historyRaw = newEntry;
        }

        // Giới hạn lưu 20 bản ghi gần nhất
        string[] entries = historyRaw.Split(new[] { '#' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (entries.Length > 20)
        {
            System.Array.Resize(ref entries, 20);
            historyRaw = string.Join("#", entries);
        }

        PlayerPrefs.SetString("Sinh_ScoreHistory", historyRaw);
        PlayerPrefs.Save();
        Debug.Log($"[QuizManager] 💾 Đã lưu lịch sử điểm Sinh học: {currentScore} điểm. Kỷ lục: {highScore} điểm!");
    }

    private void ShowGameOverPopup()
    {
        if (gameOverPopup != null)
        {
            gameOverPopup.SetActive(true);
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Root overlay
        gameOverPopup = new GameObject("GameOverPopup_Sinh");
        gameOverPopup.transform.SetParent(canvas.transform, false);
        gameOverPopup.transform.SetAsLastSibling();

        RectTransform overlayRt = gameOverPopup.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        Image overlayImg = gameOverPopup.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.85f); // Làm mờ nền phía sau

        // 2. Main Box (Bảng thông báo)
        GameObject boardObj = new GameObject("Board");
        boardObj.transform.SetParent(gameOverPopup.transform, false);
        RectTransform boardRt = boardObj.AddComponent<RectTransform>();
        boardRt.sizeDelta = new Vector2(480f, 380f);
        boardRt.anchoredPosition = Vector2.zero;

        Image boardImg = boardObj.AddComponent<Image>();
        boardImg.color = new Color(0.18f, 0.12f, 0.08f, 0.98f); // Màu nâu gỗ sẫm

        // 3. Tiêu đề HẾT MÁU
        GameObject headerObj = new GameObject("HeaderBanner");
        headerObj.transform.SetParent(boardObj.transform, false);
        RectTransform headerRt = headerObj.AddComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0f, 1f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0f, -15f);
        headerRt.sizeDelta = new Vector2(-30f, 60f);

        Image headerImg = headerObj.AddComponent<Image>();
        headerImg.color = new Color(0.72f, 0.12f, 0.12f, 1f); // Đỏ thẫm nổi bật

        GameObject headerTextObj = new GameObject("Txt_Header");
        headerTextObj.transform.SetParent(headerObj.transform, false);
        RectTransform headerTextRt = headerTextObj.AddComponent<RectTransform>();
        headerTextRt.anchorMin = Vector2.zero;
        headerTextRt.anchorMax = Vector2.one;
        headerTextRt.offsetMin = Vector2.zero;
        headerTextRt.offsetMax = Vector2.zero;

        TextMeshProUGUI headerTmp = headerTextObj.AddComponent<TextMeshProUGUI>();
        headerTmp.text = "💀 BẠN ĐÃ HẾT MÁU!";
        headerTmp.fontSize = 24f;
        headerTmp.fontStyle = FontStyles.Bold;
        headerTmp.color = Color.white;
        headerTmp.alignment = TextAlignmentOptions.Center;

        // 4. Khung nội dung điểm số & kỷ lục
        int highScore = PlayerPrefs.GetInt("Sinh_HighScore", currentScore);

        GameObject infoObj = new GameObject("Txt_Info");
        infoObj.transform.SetParent(boardObj.transform, false);
        RectTransform infoRt = infoObj.AddComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0f, 0.32f);
        infoRt.anchorMax = new Vector2(1f, 0.8f);
        infoRt.offsetMin = new Vector2(25f, 0f);
        infoRt.offsetMax = new Vector2(-25f, 0f);

        TextMeshProUGUI infoTmp = infoObj.AddComponent<TextMeshProUGUI>();
        infoTmp.text = $"<size=18><color=#E0E0E0>Cuộc hành trình Sinh học đã dừng lại!</color></size>\n\n" +
                       $"<size=22>Điểm tích lũy: <color=#FFE600><b>{currentScore}</b></color> điểm</size>\n" +
                       $"<size=18>Kỷ lục cao nhất: <color=#00FFAA><b>{highScore}</b></color> điểm</size>\n" +
                       $"<size=17>Số Boss đã vượt qua: <color=#FFA500><b>{bossesDefeatedCount}</b></color> Boss</size>";
        infoTmp.alignment = TextAlignmentOptions.Center;
        infoTmp.lineSpacing = 12f;

        // 5. Nút CHƠI LẠI (Xanh lá)
        CreatePopupButton(boardObj.transform, "Btn_Replay", "CHƠI LẠI", new Vector2(-110f, -135f), new Color(0.16f, 0.65f, 0.32f, 1f), () => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        // 6. Nút THOÁT (Nâu đỏ)
        CreatePopupButton(boardObj.transform, "Btn_Exit", "THOÁT", new Vector2(110f, -135f), new Color(0.65f, 0.22f, 0.16f, 1f), () => {
            Time.timeScale = 1f;
            SceneManager.LoadScene("LessonList 1");
        });
    }

    private void CreatePopupButton(Transform parent, string name, string label, Vector2 pos, Color btnColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(175f, 50f);

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    #endregion

    #endregion

    #region ===== HELPER PARSER & UTILS =====

    private List<string> ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    sb.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString().Trim());
        return result;
    }

    private int FindCol(List<string> headers, params string[] searchNames)
    {
        for (int i = 0; i < headers.Count; i++)
        {
            string h = headers[i].Trim().ToLower();
            foreach (var name in searchNames)
            {
                if (h.Equals(name.ToLower()) || h.Contains(name.ToLower()))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private string GetVal(List<string> cols, int colIndex, string defaultVal = "")
    {
        if (colIndex >= 0 && colIndex < cols.Count)
        {
            string val = cols[colIndex].Trim();
            if (!string.IsNullOrEmpty(val)) return val;
        }
        return defaultVal;
    }

    private string NormalizeSearchKey(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace(":", "").Replace(".", "").Replace(" ", "").Replace("_", "").Replace("-", "").ToLower();
    }

    #endregion

    #region ===== MINI GAME BẮN VỊT & LIÊN KẾT NHÁNH PHỤ =====

    [Header("Biến cho MiniGame của Thảo")]
    public static int SelectedChapter = 1;
    public static bool IsDuckShootingMode = false;

    public void SyncTimer(float time)
    {
        thoiGianHienTai = time;
    }

    public void ShowDuckShootingQuestion(int questionIndex)
    {
        Debug.Log("Đang gọi câu hỏi bắn vịt số: " + questionIndex);
    }

    public void ShowDuckShootingQuestion(string questionData)
    {
        Debug.Log("Đang gọi câu hỏi bắn vịt: " + questionData);
    }

    public void SetQuizVisible(bool isVisible)
    {
        if (panelCuonVo != null)
        {
            panelCuonVo.SetActive(isVisible);
        }
    }

    #endregion
}