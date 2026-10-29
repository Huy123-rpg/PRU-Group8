using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using ScienceQuest.Biology;

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
    public bool isTeacherQuestion = false; // True nếu câu hỏi do giảng viên tạo trong Trung tâm quản lý
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

    [Header("Panel Quiz theo chế độ (T2)")]
    [Tooltip("Panel TRẮC NGHIỆM (object 'paneltracno' trong scene). Để trống = tự tìm theo tên.")]
    public GameObject panelTracNo;
    [Tooltip("Panel TỰ LUẬN (object 'paneltuluan' trong scene). Để trống = tự tìm theo tên.")]
    public GameObject panelTuLuan;

    [Header("Ảnh trả lời Tự luận (T4)")]
    [Tooltip("Nút 'upload ảnh bài viết' trong panel tự luận. Gắn OnClick -> QuizManager.UploadAnswerImage().")]
    public Button uploadAnswerButton;
    [Tooltip("Image hiển thị xem trước ảnh bài viết đã nạp. Để trống nếu panel không có ô xem trước.")]
    public Image answerImagePreview;
    [SerializeField] private byte[] uploadedAnswerImageBytes;

    [Header("UI riêng cho panel TỰ LUẬN & Popup AI")]
    [Tooltip("Nếu panel 'paneltuluan' có khung hỏi/đáp RIÊNG, gán ở đây để hiển thị đúng panel đó. Để trống = dùng chung bộ chính.")]
    public TextMeshProUGUI questionTextTuLuanUI;
    public TMP_InputField answerInputTuLuanField;
    [Tooltip("Nút 'xóa ảnh' trong panel tự luận (object 'xoaanh').")]
    public Button deleteAnswerImageButton;
    [Tooltip("Popup AI giải thích tự luận (object 'AI' con của paneltuluan).")]
    public GameObject panelAI;
    public TextMeshProUGUI aiFeedbackText;
    public Button aiCloseButton;
    private Sprite originalTaiAnhSprite;
    private Image taiAnhImageComponent;
    private ScienceQuest.Biology.AIGradingService.GradeResult pendingTuLuanResult;

    [Header("Tương tác Button & Màu sắc")]
    public Button[] nutDapAn;
    public Color mauChuBinhThuong = Color.black;
    public Color mauChuKhiChon = Color.blue;

    [Header("Cài đặt Thời gian")]
    public TextMeshProUGUI timerTextUI;
    [Tooltip("Thời gian mỗi câu TRẮC NGHIỆM (mặc định 30s theo spec Sinh). Câu TỰ LUẬN không giới hạn thời gian.")]
    public float thoiGianMoiCau = 30f;
    private float thoiGianHienTai;
    private bool dangDemNguoc = false;

    [Header("Cài đặt Máu (Health)")]
    public Slider thanhMauUI;
    [Tooltip("Thanh máu dạng Image Filled (nếu scene dùng Image thay vì Slider)")]
    public Image thanhMauImage;
    public float maxMau = 100f;

    [Tooltip("Phần trăm máu ĐƯỢC CỘNG khi trả lời ĐÚNG")]
    public float phanTramCongMau = 20f; // Mặc định hồi 20% máu khi đúng

    private float mauHienTai;
    public float MauHienTai => mauHienTai;
    public bool IsGameOver => gameHasEnded || mauHienTai <= 0f;

    [Header("Hệ thống Điểm & Ranking")]
    public int currentScore = 0;
    public TextMeshProUGUI scoreTextUI;
    private int bossesDefeatedCount = 0;
    private GameObject gameOverPopup;

    [Header("Ghi điểm ra bảng tính (môn Sinh học)")]
    [Tooltip("Ghi kết quả ra file Excel khi người học chơi xong.")]
    public bool ghiDiemRaBangTinh = true;
    private int soCauDung = 0;
    private int soCauSai = 0;
    private bool daGhiBangTinh = false;

    // Ngân hàng toàn bộ câu hỏi đã parse
    private List<Question> masterQuestionList = new List<Question>();
    // Danh sách câu hỏi đang chơi trong màn hiện tại
    private List<Question> questionList = new List<Question>();
    private int currentQuestionIndex = 0;
    private GameObject currentBoss;

    private bool daChotDapAn = false;
    private string selectedAnswerChoice = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        mauHienTai = maxMau; // Khởi tạo máu đầy 100% ngay khi khởi động
        AutoAssignUIReferences();
        CapNhatThanhMauUI();
    }

    private void CapNhatThanhMauUI()
    {
        if (thanhMauUI != null)
        {
            thanhMauUI.maxValue = maxMau;
            thanhMauUI.value = mauHienTai;
        }
        if (thanhMauImage != null)
        {
            thanhMauImage.type = Image.Type.Filled;
            thanhMauImage.fillMethod = Image.FillMethod.Horizontal;
            thanhMauImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            thanhMauImage.fillAmount = Mathf.Clamp01(mauHienTai / Mathf.Max(maxMau, 1f));
        }
    }

    private void HienThiPanelTheoCheDo()
    {
        string cheDo = PlayerPrefs.GetString("Sinh_QuizMode", "TracNghiem");
        bool laTuLuan = cheDo.Equals("TuLuan", System.StringComparison.OrdinalIgnoreCase);

        // Nếu chưa gán trong Inspector, tự tìm panel theo tên trong scene (T2)
        if (panelTracNo == null) panelTracNo = FindPanelByVisibleName("paneltracno");
        if (panelTuLuan == null) panelTuLuan = FindPanelByVisibleName("paneltuluan");

        GameObject panelDenChon = laTuLuan ? panelTuLuan : panelTracNo;
        GameObject panelConLai  = laTuLuan ? panelTracNo : panelTuLuan;

        if (panelDenChon != null) panelDenChon.SetActive(true);
        if (panelConLai  != null) panelConLai.SetActive(false);

        // Đảm bảo popup AI luôn được ẩn khi mới mở bảng câu hỏi
        if (panelAI != null) panelAI.SetActive(false);

        // Ẩn/hiện đồng hồ đếm ngược theo chế độ (T3)
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform timeT = canvas.transform.Find("time");
            if (timeT != null) timeT.gameObject.SetActive(!laTuLuan);
        }
        if (timerTextUI != null) timerTextUI.gameObject.SetActive(!laTuLuan);

        // Dự phòng: nếu cả 2 panel đều không gán (cửa sổ chạy bằng panelCuonVo cũ), dùng panelCuonVo
        if (panelDenChon == null && panelCuonVo != null) panelCuonVo.SetActive(true);
    }

    // Tìm object con trực tiếp của Canvas có tên đúng (chấp nhận cả tên có kèm đuôi số như 'paneltracno')
    private GameObject FindPanelByVisibleName(string ten)
    {
        if (string.IsNullOrEmpty(ten)) return null;
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return null;

        // 1. Tìm theo tên chính xác trong cây canvas
        Transform found = canvas.transform.Find(ten);
        if (found != null) return found.gameObject;

        // 2. Quét tất cả object có tên chứa chuỗi (dự phòng tên có đuôi số / cách mạng khác nhau)
        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Equals(ten, System.StringComparison.OrdinalIgnoreCase))
                return t.gameObject;
        }
        return null;
    }

    // Tắt toàn bộ panel quiz (dùng khi đóng / game over)
    private void TatPanelQuiz()
    {
        if (panelCuonVo != null) panelCuonVo.SetActive(false);
        if (panelTracNo == null) panelTracNo = FindPanelByVisibleName("paneltracno");
        if (panelTuLuan == null) panelTuLuan = FindPanelByVisibleName("paneltuluan");
        if (panelTracNo != null) panelTracNo.SetActive(false);
        if (panelTuLuan != null) panelTuLuan.SetActive(false);

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform timeT = canvas.transform.Find("time");
            if (timeT != null) timeT.gameObject.SetActive(false);
        }
        if (timerTextUI != null) timerTextUI.gameObject.SetActive(false);
    }

    // Tự động tìm và nối tất cả UI references khi chưa được kéo thả trong Inspector
    private void AutoAssignUIReferences()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Panels
        if (panelTracNo == null) panelTracNo = FindPanelByVisibleName("paneltracno");
        if (panelTuLuan == null) panelTuLuan = FindPanelByVisibleName("paneltuluan");

        // 2. Wire paneltracno
        if (panelTracNo != null)
        {
            if (questionTextUI == null)
            {
                Transform debai = panelTracNo.transform.Find("debai");
                if (debai != null) questionTextUI = debai.GetComponentInChildren<TextMeshProUGUI>();
            }

            string[] ansNames = { "dapana", "dapanb", "dapanc", "dapand" };
            if (nutDapAn == null || nutDapAn.Length < 4) nutDapAn = new Button[4];

            for (int i = 0; i < 4; i++)
            {
                Transform ansT = panelTracNo.transform.Find(ansNames[i]);
                if (ansT == null)
                {
                    foreach (Transform t in panelTracNo.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.Equals(ansNames[i], System.StringComparison.OrdinalIgnoreCase)) { ansT = t; break; }
                    }
                }

                if (ansT != null)
                {
                    Button btn = ansT.GetComponent<Button>();
                    if (btn == null) btn = ansT.gameObject.AddComponent<Button>();
                    if (btn.targetGraphic == null) btn.targetGraphic = ansT.GetComponent<Image>();
                    nutDapAn[i] = btn;

                    string letter = ((char)('A' + i)).ToString();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnAnswerButtonClicked(letter));

                    TextMeshProUGUI tmp = ansT.GetComponentInChildren<TextMeshProUGUI>();
                    if (i == 0 && textA == null) textA = tmp;
                    if (i == 1 && textB == null) textB = tmp;
                    if (i == 2 && textC == null) textC = tmp;
                    if (i == 3 && textD == null) textD = tmp;
                }
            }

            Transform nopbaiTracNo = panelTracNo.transform.Find("nopbai");
            if (nopbaiTracNo != null)
            {
                Button btn = nopbaiTracNo.GetComponent<Button>() ?? nopbaiTracNo.gameObject.AddComponent<Button>();
                if (btn.targetGraphic == null) btn.targetGraphic = nopbaiTracNo.GetComponent<Image>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnSubmitClicked);
            }

            if (timerTextUI == null)
            {
                Transform timeT = panelTracNo.transform.Find("time") ?? canvas.transform.Find("time");
                if (timeT != null) timerTextUI = timeT.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // 3. Wire paneltuluan
        if (panelTuLuan != null)
        {
            if (questionTextTuLuanUI == null)
            {
                Transform debai = panelTuLuan.transform.Find("debai");
                if (debai != null) questionTextTuLuanUI = debai.GetComponentInChildren<TextMeshProUGUI>();
            }

            Transform nhapT = panelTuLuan.transform.Find("nhapdapan");
            if (nhapT != null)
            {
                TMP_InputField input = nhapT.GetComponent<TMP_InputField>();
                if (input == null)
                {
                    input = nhapT.gameObject.AddComponent<TMP_InputField>();
                    TextMeshProUGUI txt = nhapT.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        input.textComponent = txt;
                        txt.textWrappingMode = TextWrappingModes.Normal;
                    }
                }
                answerInputTuLuanField = input;
                if (answerInputField == null) answerInputField = input;
            }

            Transform nopbaiTuLuan = panelTuLuan.transform.Find("nopbai");
            if (nopbaiTuLuan != null)
            {
                Button btn = nopbaiTuLuan.GetComponent<Button>() ?? nopbaiTuLuan.gameObject.AddComponent<Button>();
                if (btn.targetGraphic == null) btn.targetGraphic = nopbaiTuLuan.GetComponent<Image>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnSubmitClicked);
            }

            Transform taiAnhT = panelTuLuan.transform.Find("taianh");
            if (taiAnhT != null)
            {
                Button btn = taiAnhT.GetComponent<Button>() ?? taiAnhT.gameObject.AddComponent<Button>();
                if (btn.targetGraphic == null) btn.targetGraphic = taiAnhT.GetComponent<Image>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(UploadAnswerImage);
                uploadAnswerButton = btn;

                taiAnhImageComponent = taiAnhT.GetComponent<Image>();
                if (taiAnhImageComponent != null) originalTaiAnhSprite = taiAnhImageComponent.sprite;
            }

            Transform xoaAnhT = panelTuLuan.transform.Find("xoaanh");
            if (xoaAnhT != null)
            {
                deleteAnswerImageButton = xoaAnhT.GetComponent<Button>() ?? xoaAnhT.gameObject.AddComponent<Button>();
                if (deleteAnswerImageButton.targetGraphic == null) deleteAnswerImageButton.targetGraphic = xoaAnhT.GetComponent<Image>();
                deleteAnswerImageButton.onClick.RemoveAllListeners();
                deleteAnswerImageButton.onClick.AddListener(ClearAnswerImage);
            }

            // Wire Popup AI (object 'AI' con của paneltuluan)
            Transform aiT = panelTuLuan.transform.Find("AI");
            if (aiT != null)
            {
                panelAI = aiT.gameObject;
                aiFeedbackText = aiT.GetComponentInChildren<TextMeshProUGUI>();

                Transform thoatT = aiT.Find("thoat");
                if (thoatT != null)
                {
                    aiCloseButton = thoatT.GetComponent<Button>() ?? thoatT.gameObject.AddComponent<Button>();
                    if (aiCloseButton.targetGraphic == null) aiCloseButton.targetGraphic = thoatT.GetComponent<Image>();
                    aiCloseButton.onClick.RemoveAllListeners();
                    aiCloseButton.onClick.AddListener(OnAICloseButtonClicked);
                }

                // Ban đầu luôn ẩn popup AI, chỉ hiện khi người dùng gửi bài
                panelAI.SetActive(false);
            }
        }

        // 4. Score & Health UI
        Transform redundantScoreHUD = canvas.transform.Find("ScoreHUD_Sinh");
        if (redundantScoreHUD != null) Destroy(redundantScoreHUD.gameObject);

        if (scoreTextUI == null)
        {
            Transform scoreT = canvas.transform.Find("score");
            if (scoreT != null) scoreTextUI = scoreT.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (thanhMauUI == null && thanhMauImage == null)
        {
            // thanhmau (1) là thanh fill máu thực tế, thanhmau là khung viền/nền
            Transform tmT = canvas.transform.Find("thanhmau (1)") ?? canvas.transform.Find("thanhmau");
            if (tmT != null)
            {
                Slider s = tmT.GetComponent<Slider>();
                if (s != null) thanhMauUI = s;
                else
                {
                    Image img = tmT.GetComponent<Image>();
                    if (img != null)
                    {
                        thanhMauImage = img;
                        thanhMauImage.type = Image.Type.Filled;
                        thanhMauImage.fillMethod = Image.FillMethod.Horizontal;
                        thanhMauImage.fillOrigin = (int)Image.OriginHorizontal.Left;

                        // Đảm bảo kích thước thanh máu khớp 100% rãnh khung thanhmau
                        RectTransform rt = img.rectTransform;
                        rt.anchoredPosition = new Vector2(676f, 437.2f);
                        rt.sizeDelta = new Vector2(307f, 28f);
                    }
                }
            }
        }

        // 5. Panel Pause & Buttons
        GameObject pausePanel = FindPanelByVisibleName("panelpause");
        if (pausePanel != null)
        {
            Transform choiTiepT = pausePanel.transform.Find("choitiep");
            if (choiTiepT != null)
            {
                Button btn = choiTiepT.GetComponent<Button>() ?? choiTiepT.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    pausePanel.SetActive(false);
                    Time.timeScale = 1f;
                });
            }

            Transform choiLaiT = pausePanel.transform.Find("choilai");
            if (choiLaiT != null)
            {
                Button btn = choiLaiT.GetComponent<Button>() ?? choiLaiT.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }

            Transform thoatT = pausePanel.transform.Find("thoat");
            if (thoatT != null)
            {
                Button btn = thoatT.GetComponent<Button>() ?? thoatT.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("LessonList 1");
                });
            }

            Transform pauseBtnT = canvas.transform.Find("pause");
            if (pauseBtnT != null)
            {
                Button btn = pauseBtnT.GetComponent<Button>() ?? pauseBtnT.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    pausePanel.SetActive(true);
                    Time.timeScale = 0f;
                });
            }

            pausePanel.SetActive(false);
        }
    }

    // T1 & T7: Đảm bảo nền cuộn + âm thanh có trong màn (tự tạo nếu chưa gán trong scene)
    private void BootstrapSystemObjects()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            // Tự động đảm bảo RunnerWiring được gắn lên Canvas và kích hoạt toàn bộ hệ thống gameplay mới
            RunnerWiring wiring = canvas.GetComponent<RunnerWiring>();
            if (wiring == null)
            {
                wiring = canvas.gameObject.AddComponent<RunnerWiring>();
                Debug.Log("[QuizManager] 🏃 Đã tự động gắn RunnerWiring lên Canvas.");
            }
        }

        // Loại bỏ AutoScroll trên Main Camera (tránh camera bay mất)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            AutoScroll auto = mainCam.GetComponent<AutoScroll>();
            if (auto != null) Destroy(auto);
        }

        // Âm thanh (T7)
        if (AudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("AudioManager_Sinh");
            AudioManager audio = audioObj.AddComponent<AudioManager>();
            audio.bgmVolume = 0.4f;

            // Gắn sự kiện vào 2 nút âm thanh sẵn có trong UI (nhac = BGM, amthanh (1) = SFX)
            WireAudioToggles(audio);
            Debug.Log("[QuizManager] 🔊 Đã tự tạo AudioManager (T7). Gán clip trong Inspector để bật tiếng.");
        }
    }

    // Gắn nút UI 'nhac' (BGM) và 'amthanh (1)' (SFX) nếu chúng chưa có Button + sự kiện
    private void WireAudioToggles(AudioManager audio)
    {
        TryToggleAudioButton("nhac", () => audio.ToggleBGM());
        TryToggleAudioButton("amthanh (1)", () => audio.ToggleSFX());
    }

    private void TryToggleAudioButton(string tenNut, UnityEngine.Events.UnityAction action)
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.Log($"[QuizManager] ℹ️ Không thấy Canvas, bỏ qua nút '{tenNut}'.");
            return;
        }

        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.Equals(tenNut, System.StringComparison.OrdinalIgnoreCase)) continue;
            Button btn = t.GetComponent<Button>();
            if (btn == null) btn = t.gameObject.AddComponent<Button>();
            if (btn.targetGraphic == null) btn.targetGraphic = t.GetComponent<Image>();
            btn.onClick.AddListener(action);
            Debug.Log($"[QuizManager] 🔗 Nút '{tenNut}' đã gắn sự kiện âm thanh.");
            return;
        }
        Debug.Log($"[QuizManager] ℹ️ Không thấy nút '{tenNut}', bỏ qua (vẫn có AudioManager hoạt động).");
    }

    private void Start()
    {
        // Tự động tìm và nối các thành phần UI trong scene nếu chưa kéo thả
        AutoAssignUIReferences();

        mauHienTai = maxMau;
        soCauDung = 0;
        soCauSai = 0;
        daGhiBangTinh = false;
        if (thanhMauUI != null)
        {
            thanhMauUI.maxValue = maxMau;
            thanhMauUI.value = mauHienTai;
        }
        CapNhatThanhMauUI();

        if (panelCuonVo != null) panelCuonVo.SetActive(false);
        TatPanelQuiz(); // T2: tắt cả panel trắc nghiệm / tự luận

        // Khởi tạo HUD hiển thị điểm tích lũy
        CreateScoreUIHUD();

        // T1 & T7: Đảm bảo nền cuộn + âm thanh chạy (tự tạo object nếu scene chưa gán)
        BootstrapSystemObjects();

        // Nạp câu hỏi từ CSV cục bộ trước
        LoadQuestionsFromCSV();

        // Nếu có link Google Sheet online thì nạp cập nhật từ Google Sheet
        if (!string.IsNullOrEmpty(googleSheetCsvUrl))
        {
            StartCoroutine(FetchQuestionsFromGoogleSheet(googleSheetCsvUrl));
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Đảm bảo ghi điểm ra file cục bộ khi người học rời màn (thoát giữa chừng hoặc đóng game)
        // Không gọi coroutine đồng bộ Google Sheet trong OnDestroy vì MonoBehaviour đang bị hủy
        GhiKetQuaRaBangTinh(false);
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
                soCauSai++; // Hết giờ = trả lời sai
                if (timerTextUI != null) timerTextUI.text = "0s";

                string loiGiaiThich = (currentQuestionIndex < questionList.Count) 
                    ? questionList[currentQuestionIndex].explain 
                    : "";
                if (answerInputField != null)
                {
                    answerInputField.text = "HẾT GIỜ! " + loiGiaiThich;
                }

                Debug.Log("[QuizManager] ⏰ Đã hết giờ! Xử lý như trả lời sai (-10 HP)...");
                OnQuizWrong();
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

        // Nạp thêm các câu hỏi do giảng viên tạo trong Trung tâm quản lý
        LoadTeacherQuestions();

        Debug.Log($"[QuizManager] 📚 Đã tải thành công {masterQuestionList.Count} câu hỏi vào ngân hàng tổng.");

        // Lọc câu hỏi theo Chương & Bài đang chọn
        FilterCurrentQuestions();
    }

    /// <summary>
    /// Nạp các câu hỏi do giảng viên tạo (lưu ở Trung tâm quản lý) vào ngân hàng tổng.
    /// Câu hỏi của giảng viên được ưu tiên: nếu trùng bài/chế độ thì dùng bộ của giảng viên.
    /// </summary>
    private void LoadTeacherQuestions()
    {
        // Xoá các câu hỏi giảng viên đã nạp trước đó để không bị trùng khi Google Sheet tải lại dữ liệu
        masterQuestionList.RemoveAll(q => q.isTeacherQuestion);

        List<TeacherLesson> lessons = TeacherStore.GetLessons();
        if (lessons == null || lessons.Count == 0) return;

        int added = 0;
        foreach (var lesson in lessons)
        {
            if (lesson.questions == null) continue;

            foreach (var tq in lesson.questions)
            {
                if (tq == null || string.IsNullOrEmpty(tq.questionText)) continue;

                Question q = new Question();
                q.questionId = tq.questionId;
                q.chapterId = tq.chapterId;
                q.lessonId = tq.lessonId;
                q.questionText = tq.questionText;
                q.ansA = tq.ansA;
                q.ansB = tq.ansB;
                q.ansC = tq.ansC;
                q.ansD = tq.ansD;
                q.correctAnswer = tq.correctAnswer;
                q.mode = string.IsNullOrEmpty(tq.mode) ? "TracNghiem" : tq.mode;
                q.keywordsTuLuan = tq.keywordsTuLuan;
                q.explain = tq.explain;
                q.difficulty = tq.difficulty;
                q.hinhAnh = tq.hinhAnh;
                q.timeLimit = tq.timeLimit > 0 ? tq.timeLimit : thoiGianMoiCau;
                q.hpBonus = tq.hpBonus > 0 ? tq.hpBonus : phanTramCongMau;
                q.isTeacherQuestion = true;

                masterQuestionList.Add(q);
                added++;
            }
        }

        if (added > 0)
        {
            Debug.Log($"[QuizManager] 👨‍🏫 Đã nạp {added} câu hỏi do giảng viên tạo.");
        }
    }

    private void EnsureTuLuanQuestionBank()
    {
        // Xoá bộ tự luận mặc định của lần nạp trước để không nhân bản khi Google Sheet tải lại
        masterQuestionList.RemoveAll(q => q.questionId.StartsWith("TL"));

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

        // Nếu học sinh chọn một bài do giảng viên tạo, chỉ lấy câu hỏi của đúng bài đó
        // (bài của giảng viên dùng chính tiêu đề làm khoá, không trộn với câu hỏi có sẵn).
        if (filterByChapterAndLesson && !string.IsNullOrEmpty(selectedLesson))
        {
            List<Question> teacherOnly = new List<Question>();
            foreach (var q in masterQuestionList)
            {
                if (!q.isTeacherQuestion) continue;
                if (!q.mode.Equals(selectedMode, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (!IsLessonMatch(q.lessonId, selectedLesson)) continue;
                teacherOnly.Add(q);
            }

            if (teacherOnly.Count > 0)
            {
                questionList.AddRange(teacherOnly);
                Debug.Log($"[QuizManager] 👨‍🏫 Dùng {questionList.Count} câu hỏi của bài giảng viên: {selectedLesson}");
                return;
            }
        }

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

        // Câu hỏi do giảng viên tạo: chapterId lưu tiêu đề chủ đề, lessonId lưu tiêu đề bài.
        // So khớp trực tiếp theo tiêu đề trước khi áp dụng quy tắc nhóm môn học.
        if (!string.IsNullOrEmpty(chapterId))
        {
            string normCId = NormalizeSearchKey(chapterId);
            if (!normCId.StartsWith("cd") && (normCId.Contains(normChap) || normChap.Contains(normCId)))
            {
                return true;
            }
        }

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

        // Bài của giảng viên dùng chính tiêu đề làm khoá, nên so khớp ở trên đã chính xác.
        // Trường hợp chỉ khác nhau ở tiền tố "Bài N" được xử lý bằng vòng lặp số bài bên dưới.

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
        // Dừng hệ thống runner (spawn + nền cuộn + vật thể) khi mở quiz
        if (RunnerController.Instance != null) RunnerController.Instance.SetRunning(false);
        HienThiPanelTheoCheDo(); // T2: hiện đúng panel (paneltracno / paneltuluan) theo chế độ đã chọn
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBoss(); // T7: SFX boss hiện / mở quiz

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
        selectedAnswerChoice = "";
        currentQuestionIndex = Random.Range(0, questionList.Count);
        ResetHieuUngTatCaNut();
        DisplayQuestion(currentQuestionIndex);

        Question q = questionList[currentQuestionIndex];

        // T3: Tự luận KHÔNG giới hạn thời gian => tắt đồng hồ ngược; trắc nghiệm thì đếm ngược
        bool isTuLuanQ = q.mode.Equals("TuLuan", System.StringComparison.OrdinalIgnoreCase);
        if (isTuLuanQ)
        {
            dangDemNguoc = false;
            thoiGianHienTai = 0f;
            if (timerTextUI != null) timerTextUI.text = "∞";
        }
        else
        {
            thoiGianHienTai = q.timeLimit > 0 ? q.timeLimit : thoiGianMoiCau;
            dangDemNguoc = true;
            if (timerTextUI != null) timerTextUI.text = Mathf.CeilToInt(thoiGianHienTai).ToString() + "s";
        }

        // T4: Tắt ảnh bài làm cũ giữa các câu
        uploadedAnswerImageBytes = null;
        if (answerImagePreview != null) answerImagePreview.enabled = false;
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

        // T2/T4: Chế độ TỰ LUẬN thì ẩn bộ đáp án trắc nghiệm (A/B/C/D)
        if (nutDapAn != null && nutDapAn.Length > 0)
        {
            foreach (var btn in nutDapAn)
            {
                if (btn != null) btn.gameObject.SetActive(!isTuLuan);
            }
        }
        if (textA != null && textA.transform.parent != null) textA.transform.parent.gameObject.SetActive(!isTuLuan);
        if (textB != null && textB.transform.parent != null) textB.transform.parent.gameObject.SetActive(!isTuLuan);
        if (textC != null && textC.transform.parent != null) textC.transform.parent.gameObject.SetActive(!isTuLuan);
        if (textD != null && textD.transform.parent != null) textD.transform.parent.gameObject.SetActive(!isTuLuan);

        if (!isTuLuan)
        {
            if (textA != null) { textA.text = q.ansA; textA.color = mauChuBinhThuong; textA.enableAutoSizing = false; textA.fontSize = 24f; }
            if (textB != null) { textB.text = q.ansB; textB.color = mauChuBinhThuong; textB.enableAutoSizing = false; textB.fontSize = 24f; }
            if (textC != null) { textC.text = q.ansC; textC.color = mauChuBinhThuong; textC.enableAutoSizing = false; textC.fontSize = 24f; }
            if (textD != null) { textD.text = q.ansD; textD.color = mauChuBinhThuong; textD.enableAutoSizing = false; textD.fontSize = 24f; }
        }

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

        // T4: Đồng bộ UI riêng cho panel TỰ LUẬN (nếu gán) + cho phép nút upload khi tự luận
        if (questionTextTuLuanUI != null) questionTextTuLuanUI.text = q.questionText;
        if (answerInputTuLuanField != null)
        {
            answerInputTuLuanField.text = "";
            answerInputTuLuanField.interactable = true;
        }
        ClearAnswerImage();
        if (panelAI != null) panelAI.SetActive(false);

        if (uploadAnswerButton != null)
        {
            uploadAnswerButton.gameObject.SetActive(isTuLuan);
            uploadAnswerButton.interactable = isTuLuan;
        }
        if (deleteAnswerImageButton != null)
        {
            deleteAnswerImageButton.gameObject.SetActive(isTuLuan);
            deleteAnswerImageButton.interactable = isTuLuan;
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

        selectedAnswerChoice = selectedAnswer;
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
        if (currentQuestionIndex >= questionList.Count) return;

        Question currentQ = questionList[currentQuestionIndex];
        bool isTuLuanQ = currentQ.mode.Equals("TuLuan", System.StringComparison.OrdinalIgnoreCase);

        // Với tự luận: lấy từ input tự luận hoặc text; Với trắc nghiệm: lấy đáp án vừa chọn
        string rawAnswer = "";
        if (isTuLuanQ)
        {
            if (answerInputTuLuanField != null) rawAnswer = answerInputTuLuanField.text;
            else if (answerInputField != null) rawAnswer = answerInputField.text;
        }
        else
        {
            if (!string.IsNullOrEmpty(selectedAnswerChoice)) rawAnswer = selectedAnswerChoice;
            else if (answerInputField != null) rawAnswer = answerInputField.text;
        }

        string userAnswer = rawAnswer != null ? rawAnswer.Trim() : "";
        bool hasText = !string.IsNullOrEmpty(userAnswer);
        bool hasImage = uploadedAnswerImageBytes != null && uploadedAnswerImageBytes.Length > 0;

        // Kiểm tra hợp lệ theo chế độ:
        if (isTuLuanQ)
        {
            // Tự luận: Người dùng có 2 cách (nhập bảng nhapdapan HOẶC upload ảnh trả lời)
            if (!hasText && !hasImage)
            {
                Debug.LogWarning("[QuizManager] ⚠️ Vui lòng nhập câu trả lời vào ô hoặc tải ảnh bài làm trước khi gửi!");
                return;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(userAnswer))
            {
                Debug.LogWarning("[QuizManager] ⚠️ Chưa chọn đáp án trắc nghiệm!");
                return;
            }
        }

        daChotDapAn = true;
        dangDemNguoc = false;

        // Xử lý gửi bài theo chế độ
        if (isTuLuanQ)
        {
            // 1. Mở popup AI hiển thị trạng thái đang chấm
            if (panelAI != null)
            {
                panelAI.SetActive(true);
                panelAI.transform.SetAsLastSibling(); // luôn hiển thị trên cùng
            }

            if (aiFeedbackText != null)
            {
                aiFeedbackText.text = "<b><color=#d35400>🤖 AI đang chấm bài và tạo giải thích ngắn gọn, vui lòng đợi trong giây lát...</color></b>";
            }

            if (aiCloseButton != null)
            {
                aiCloseButton.interactable = false; // tạm khóa nút dấu X đỏ trong lúc AI đang chấm
            }

            // 2. Gửi cho AI chấm bài (gồm văn bản và/hoặc ảnh bài viết)
            byte[] anhBanLam = uploadedAnswerImageBytes;

            StartCoroutine(AIGradingService.GradeAnswer(
                currentQ.questionText,
                userAnswer,
                currentQ.correctAnswer,
                currentQ.keywordsTuLuan,
                anhBanLam,
                (result) =>
                {
                    XuLyKetQuaTuLuanAI(result, currentQ);
                }
            ));
        }
        else
        {
            // Trắc nghiệm: So sánh ký tự A/B/C/D hoặc chuỗi đáp án (không phân biệt hoa thường)
            bool isCorrect = userAnswer.Equals(currentQ.correctAnswer.Trim(), System.StringComparison.OrdinalIgnoreCase);
            XuLyKetQuaTraLoi(isCorrect, currentQ, currentQ.explain);
        }
    }

    /// <summary>
    /// Xử lý khi AI chấm xong bài Tự luận: hiển thị nhận xét và giải thích ngắn gọn trong popup AI.
    /// Giữ nguyên popup để người dùng đọc, KHÔNG tự động chuyển câu.
    /// </summary>
    private void XuLyKetQuaTuLuanAI(AIGradingService.GradeResult result, Question currentQ)
    {
        pendingTuLuanResult = result;

        if (result.isCorrect)
        {
            soCauDung++;
            OnQuizCorrect();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrect(); // SFX đúng
        }
        else
        {
            soCauSai++;
            OnQuizWrong();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWrong(); // SFX sai
        }

        // Hiển thị kết quả & giải thích ngắn gọn trong popup AI
        if (aiFeedbackText != null)
        {
            // Chỉ hiển thị DUY NHẤT "ĐÚNG" hoặc "CHƯA ĐÚNG", không hiển thị điểm số hay chữ đạt/chưa đạt
            string danhGia = result.isCorrect
                ? "<color=#27ae60><b>ĐÚNG</b></color>"
                : "<color=#c0392b><b>CHƯA ĐÚNG</b></color>";

            string feedback = !string.IsNullOrEmpty(result.feedback) ? result.feedback : currentQ.explain;
            aiFeedbackText.text = $"{danhGia}\n\n<b>Giải thích:</b>\n{feedback}";
        }

        // Mở lại nút dấu X đỏ để người dùng đọc giải thích xong thì chủ động bấm tắt
        if (aiCloseButton != null)
        {
            aiCloseButton.interactable = true;
        }

        Debug.Log($"[QuizManager] 🤖 AI đã giải thích xong. Chờ người dùng click nút X đỏ để chuyển câu.");
    }

    /// <summary>
    /// Nút dấu X màu đỏ trên popup AI: khi người dùng đọc xong giải thích và click vào nút này,
    /// popup AI sẽ đóng lại và hệ thống mới chuyển sang câu hỏi tiếp theo (câu 2).
    /// </summary>
    public void OnAICloseButtonClicked()
    {
        Debug.Log("[QuizManager] ❌ Người dùng đã click nút dấu X đỏ tắt AI. Đang chuyển sang câu tiếp theo...");

        if (panelAI != null) panelAI.SetActive(false);

        // Dọn dẹp câu trả lời vừa làm
        if (answerInputTuLuanField != null) answerInputTuLuanField.text = "";
        ClearAnswerImage();

        // Nếu người chơi hết máu -> Game Over
        if (mauHienTai <= 0)
        {
            TriggerGameOver();
            return;
        }

        // Chuyển sang câu tiếp theo ngay lập tức
        ChuyenSangCauTiepTheo();
    }

    /// <summary>
    /// T4: Người chơi nạp ảnh bài viết tự luận (gọi từ ô 'taianh' trong panelTuluan).
    /// </summary>
    public void UploadAnswerImage()
    {
        byte[] data = null;

#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(
            "Chọn ảnh bài viết tự luận", "", new string[] { "Image files", "png,jpg,jpeg", "All files", "*" });
        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
        {
            data = System.IO.File.ReadAllBytes(path);
        }
#endif

        if (data != null)
        {
            uploadedAnswerImageBytes = data;

            if (taiAnhImageComponent == null && panelTuLuan != null)
            {
                Transform taiAnhT = panelTuLuan.transform.Find("taianh");
                if (taiAnhT != null) taiAnhImageComponent = taiAnhT.GetComponent<Image>();
            }

            if (taiAnhImageComponent != null)
            {
                if (originalTaiAnhSprite == null) originalTaiAnhSprite = taiAnhImageComponent.sprite;
                Texture2D tex = CreatePreviewTexture(data);
                if (tex != null)
                {
                    taiAnhImageComponent.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }

            if (answerImagePreview != null)
            {
                answerImagePreview.enabled = true;
                Texture2D tex = CreatePreviewTexture(data);
                if (tex != null)
                    answerImagePreview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            if (deleteAnswerImageButton != null) deleteAnswerImageButton.gameObject.SetActive(true);

            Debug.Log($"[QuizManager] 📷 Đã nạp ảnh bài viết ({data.Length} bytes).");
            return;
        }

        Debug.LogWarning("[QuizManager] Chưa chọn ảnh. Bạn có thể nhập nội dung vào bảng nhapdapan.");
    }

    /// <summary>
    /// Xóa ảnh đã tải (khi người dùng click nút 'xoaanh' / Xóa ảnh).
    /// </summary>
    public void ClearAnswerImage()
    {
        uploadedAnswerImageBytes = null;

        if (taiAnhImageComponent == null && panelTuLuan != null)
        {
            Transform taiAnhT = panelTuLuan.transform.Find("taianh");
            if (taiAnhT != null) taiAnhImageComponent = taiAnhT.GetComponent<Image>();
        }

        // Hoàn nguyên ô taianh về icon tải ảnh ban đầu
        if (taiAnhImageComponent != null && originalTaiAnhSprite != null)
        {
            taiAnhImageComponent.sprite = originalTaiAnhSprite;
        }

        if (answerImagePreview != null)
        {
            answerImagePreview.enabled = false;
            answerImagePreview.sprite = null;
        }

        Debug.Log("[QuizManager] 🗑️ Đã xóa ảnh bài viết vừa tải.");
    }

    // Tạo Texture xem trước từ byte ảnh (an toàn, tự destroy texture cũ)
    private Texture2D CreatePreviewTexture(byte[] bytes)
    {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        if (!ImageConversion.LoadImage(tex, bytes))
        {
            Destroy(tex);
            return null;
        }
        return tex;
    }

    /// <summary>
    /// Xử lý điểm, hồi máu / trừ máu sau khi có kết quả cho Trắc Nghiệm.
    /// </summary>
    private void XuLyKetQuaTraLoi(bool isCorrect, Question currentQ, string loiNhanXet)
    {
        if (isCorrect)
        {
            soCauDung++;
            OnQuizCorrect();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrect(); // SFX đúng

            if (answerInputField != null)
            {
                answerInputField.text = "ĐÚNG! " + loiNhanXet;
            }
            if (answerInputTuLuanField != null)
            {
                answerInputTuLuanField.text = "ĐÚNG! " + loiNhanXet;
            }

            StartCoroutine(DoiChuyenCau(true));
        }
        else
        {
            soCauSai++;
            OnQuizWrong();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWrong(); // SFX sai
            if (answerInputField != null)
            {
                answerInputField.text = "CHƯA ĐÚNG! " + loiNhanXet;
            }
            if (answerInputTuLuanField != null)
            {
                answerInputTuLuanField.text = "CHƯA ĐÚNG! " + loiNhanXet;
            }
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

    /// <summary>
    /// Chuyển câu hỏi kế tiếp cho cả Trắc Nghiệm và Tự Luận.
    /// </summary>
    public void ChuyenSangCauTiepTheo()
    {
        if (currentQuestionIndex < questionList.Count)
        {
            questionList.RemoveAt(currentQuestionIndex);
        }

        soCauDaHoi++;

        // Hoàn thành số câu cho 1 Boss (mặc định 5 câu)
        if (soCauDaHoi >= soCauHoiCanTraLoi)
        {
            ThangBoss();
            return;
        }

        // Bổ sung câu hỏi nếu danh sách tạm hết
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

    IEnumerator DoiChuyenCau(bool laCauDung)
    {
        yield return new WaitForSecondsRealtime(2f);
        ChuyenSangCauTiepTheo();
    }

    public void CongMau(float phanTram)
    {
        float luongMauCong = maxMau * (phanTram / 100f);
        mauHienTai += luongMauCong;

        if (mauHienTai > maxMau) mauHienTai = maxMau;

        CapNhatThanhMauUI();
        Debug.Log($"[QuizManager] 💚 Thưởng: Được cộng {phanTram}% máu! (Hiện tại: {mauHienTai}/{maxMau})");
    }

    // =====================================================================
    // HP / Elo TUYỆT ĐỐI (spec mới) — nguồn trạng thái duy nhất (single source of truth)
    // Thay thế hoàn toàn cơ chế theo phần trăm cũ cho các sự kiện gameplay.
    // =====================================================================

    /// <summary>
    /// Cộng hoặc trừ HP TUYỆT ĐỐI (không phải phần trăm), clamp trong [0, maxMau].
    /// Gọi với 20 (đúng quiz / vật phẩm), -20 (chạm chướng ngại vật), -10 (sai quiz), v.v.
    /// Tự kích hoạt Game Over khi HP về 0.
    /// </summary>
    public void ChangeHP(int absoluteAmount)
    {
        mauHienTai += absoluteAmount;
        if (mauHienTai < 0f) mauHienTai = 0f;
        if (mauHienTai > maxMau) mauHienTai = maxMau;

        CapNhatThanhMauUI();
        Debug.Log($"[QuizManager] HP {(absoluteAmount > 0 ? "+" : "")}{absoluteAmount} (Hiện tại: {mauHienTai}/{maxMau})");

        if (mauHienTai <= 0f) TriggerGameOver();
    }

    /// <summary>Cộng Elo/điểm tích lũy TUYỆT ĐỐI (không trừ, không tự đặt 0).</summary>
    public void AddElo(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
        SaveScoreAndRanking();
        Debug.Log($"[QuizManager] 🏆 Elo {(amount > 0 ? "+" : "")}{amount} (Tổng: {currentScore})");
    }

    /// <summary>Đúng quiz: +20 HP, +10 Elo (theo spec).</summary>
    public void OnQuizCorrect()
    {
        ChangeHP(+20);
        AddElo(+10);
    }

    /// <summary>Sai quiz / hết giờ: -10 HP, Elo không đổi (theo spec).</summary>
    public void OnQuizWrong()
    {
        ChangeHP(-10);
    }

    /// <summary>
    /// Luồng Game Over thống nhất (HP về 0): dừng runner, hiện popup, lưu điểm, ghi bảng tính.
    /// Chỉ gọi 1 lần (dùng cờ chống lặp).
    /// </summary>
    private bool gameHasEnded = false;
    private void TriggerGameOver()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;

        dangDemNguoc = false;
        if (panelCuonVo != null) panelCuonVo.SetActive(false);
        TatPanelQuiz();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
        Time.timeScale = 0f;

        if (RunnerController.Instance != null) RunnerController.Instance.SetRunning(false); // dừng spawn/scroll

        Debug.Log($"[QuizManager] 💀 GAME OVER! Bạn đã hết máu. Tổng điểm đạt được: {currentScore}");

        // Lưu điểm kỷ lục và lưu lịch sử để xếp hạng ranking
        SaveScoreAndRanking();

        // Ghi kết quả ra bảng tính (Excel + Google Sheet) cho giảng viên
        GhiKetQuaRaBangTinh();

        // Hiển thị Popup Hết Máu có 2 nút: Chơi lại & Thoát
        ShowGameOverPopup();
    }

    public void TruMau(float phanTram = 10f)
    {
        // Cơ chế CŨ (theo %) - giữ cho tương thích, nhưng chuyển hướng sang ChangeHP tuyệt đối
        // để có MỘT luồng Game Over thống nhất. Giá trị % được quy đổi sang số HP tuyệt đối.
        int absolute = Mathf.RoundToInt(maxMau * (phanTram / 100f));
        ChangeHP(-absolute);
    }

    private void ThangBoss()
    {
        bossesDefeatedCount++;
        Debug.Log($"[QuizManager] 🎉 CHIẾN THẮNG BOSS #{bossesDefeatedCount}! Đã vượt qua thử thách. Map tiếp tục chạy vô tận.");
        dangDemNguoc = false;
        if (currentBoss != null) Destroy(currentBoss);
        if (panelCuonVo != null) panelCuonVo.SetActive(false);
        TatPanelQuiz();
        Time.timeScale = 1f; // Tiếp tục chạy để vượt qua các boss tiếp theo
        // Chạy lại hệ thống runner (spawn + nền + vật thể) và sẵn sàng cho Boss tiếp theo
        if (RunnerController.Instance != null)
        {
            RunnerController.Instance.NotifyBossDefeated();
        }

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
        string selectedLesson = PlayerPrefs.GetString("Sinh_SelectedLesson", "");

        // Nếu đang chơi bài của giảng viên thì chỉ nạp lại câu hỏi của đúng bài đó,
        // tránh trộn câu hỏi có sẵn vào bài mới.
        List<Question> filtered = masterQuestionList.FindAll(q =>
            q.mode.Equals(mode, System.StringComparison.OrdinalIgnoreCase) &&
            (!q.isTeacherQuestion || IsLessonMatch(q.lessonId, selectedLesson)));

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
        // Spec mục 3: Loại bỏ hoàn toàn khung điểm thừa "🏆 ĐIỂM: 0" (ScoreHUD_Sinh)
        // Gắn điểm trực tiếp vào đối tượng 'score' chính thức có sẵn trên Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Transform existing = canvas.transform.Find("ScoreHUD_Sinh");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        if (scoreTextUI == null)
        {
            Transform scoreT = canvas.transform.Find("score");
            if (scoreT != null)
            {
                scoreTextUI = scoreT.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (scoreTextUI != null)
        {
            scoreTextUI.text = currentScore.ToString();
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

    /// <summary>
    /// Ghi kết quả lượt chơi ra bảng tính cho giảng viên.
    /// Ghi 1 lần duy nhất mỗi lượt chơi (dù hết giờ hay chơi lại).
    /// </summary>
    private void GhiKetQuaRaBangTinh(bool syncOnline = true)
    {
        if (!ghiDiemRaBangTinh || daGhiBangTinh) return;
        daGhiBangTinh = true;

        int tongSoCau = soCauDung + soCauSai;
        float tiLeDung = tongSoCau > 0 ? (float)soCauDung / tongSoCau * 100f : 0f;

        string tenTaiKhoan = LoginManager.CurrentUsername;
        if (string.IsNullOrEmpty(tenTaiKhoan))
        {
            tenTaiKhoan = PlayerPrefs.GetString("Sinh_CurrentUsername", "");
        }
        if (string.IsNullOrEmpty(tenTaiKhoan))
        {
            tenTaiKhoan = "Không rõ";
        }

        ScoreRecord record = new ScoreRecord();
        record.timestamp = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        record.username = tenTaiKhoan;
        record.chapter = PlayerPrefs.GetString("Sinh_SelectedChapter", "");
        record.lesson = PlayerPrefs.GetString("Sinh_SelectedLesson", "");
        record.mode = PlayerPrefs.GetString("Sinh_QuizMode", "TracNghiem") == "TuLuan" ? "Tự luận" : "Trắc nghiệm";
        record.totalQuestions = tongSoCau;
        record.correctCount = soCauDung;
        record.wrongCount = soCauSai;
        record.score = currentScore;
        record.highScore = PlayerPrefs.GetInt("Sinh_HighScore", currentScore);
        record.bossesDefeated = bossesDefeatedCount;
        record.accuracyPercent = tiLeDung;
        record.details = $"{record.chapter} | {record.lesson} | Boss vượt qua: {bossesDefeatedCount}";

        ScoreRecorder.Record(record);

        // Đồng bộ lên Google Sheet nếu giảng viên đã cấu hình link và MonoBehaviour còn hoạt động
        if (syncOnline && gameObject.activeInHierarchy && !string.IsNullOrEmpty(ScoreRecorder.GetSheetUrl()))
        {
            StartCoroutine(ScoreRecorder.PushToGoogleSheet(this, record));
        }
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
            // Ghi kết quả lượt chơi ra bảng tính trước khi thoát
            GhiKetQuaRaBangTinh();
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