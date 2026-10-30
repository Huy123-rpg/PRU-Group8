using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI selectedSubjectText;

    [Header("Subject Selection Root (Màn Chọn Môn Học)")]
    public GameObject titlePlate;
    public GameObject bioCard;
    public GameObject chemCard;
    public GameObject phyCard;

    [Header("Biology Game Selection (Màn Chọn Game Môn Sinh)")]
    public GameObject sinhGameSelectPanel;
    public Button obstacleRunnerBtn;
    public Button survivalGameBtn;
    public Button backToSubjectBtn;
    public GameObject survivalNoticeDialog;

    [Header("Scenes Phân hệ Sinh học (Folder Sinh)")]
    [Tooltip("Scene dành cho Học sinh (Player)")]
    public string bioStudentSceneName = "ChapterList 1";
    [Tooltip("Scene dành cho Giảng viên (Teacher)")]
    public string bioTeacherSceneName = "TeacherCenter";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindSubjectUI();
        EnsureSinhGameSelectPanel();
    }

    /// <summary>
    /// Tự động tìm kiếm các thành phần UI của màn chọn môn nếu chưa gán trong Inspector.
    /// </summary>
    private void FindSubjectUI()
    {
        if (titlePlate == null) titlePlate = FindChildByName("TitlePlate");
        if (bioCard == null) bioCard = FindChildByName("Bio Card");
        if (chemCard == null) chemCard = FindChildByName("ChemCard");
        if (phyCard == null) phyCard = FindChildByName("PhyCard");
    }

    private GameObject FindChildByName(string childName)
    {
        Transform t = transform.Find(childName);
        if (t != null) return t.gameObject;

        // Tìm kiếm đệ quy
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child != null && child.name == childName)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Đảm bảo panel chọn chế độ game (Vượt chướng ngại vật & Sinh tồn) đã sẵn sàng.
    /// Nếu chưa có trong scene, tự động dựng UI đồng bộ chuẩn template.
    /// </summary>
    private void EnsureSinhGameSelectPanel()
    {
        if (sinhGameSelectPanel == null)
        {
            Transform existing = transform.Find("SinhGameSelectPanel");
            if (existing != null)
            {
                sinhGameSelectPanel = existing.gameObject;
            }
        }

        if (sinhGameSelectPanel == null)
        {
            BuildSinhGameSelectPanel();
        }

        // Mặc định ban đầu ẩn màn chọn game, chỉ hiện khi click vào Sinh học
        if (sinhGameSelectPanel != null)
        {
            sinhGameSelectPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Tạo dynamically panel chọn game môn Sinh khớp 100% với template phòng học & thẻ bài.
    /// </summary>
    private void BuildSinhGameSelectPanel()
    {
        // 1. Root Panel
        GameObject panelObj = new GameObject("SinhGameSelectPanel", typeof(RectTransform));
        panelObj.transform.SetParent(transform, false);
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        sinhGameSelectPanel = panelObj;

        // Lấy tài nguyên mẫu từ TitlePlate & Cards
        Sprite titleSprite = null;
        TMP_FontAsset fontAsset = null;
        if (titlePlate != null)
        {
            Image tpImg = titlePlate.GetComponent<Image>();
            if (tpImg != null) titleSprite = tpImg.sprite;
            TextMeshProUGUI tpTmp = titlePlate.GetComponentInChildren<TextMeshProUGUI>();
            if (tpTmp != null) fontAsset = tpTmp.font;
        }

        Sprite bioNormal = null, bioSelected = null;
        if (bioCard != null)
        {
            SubjectCard sc = bioCard.GetComponent<SubjectCard>();
            if (sc != null)
            {
                bioNormal = sc.normalSprite;
                bioSelected = sc.selectedSprite;
            }
            if (bioNormal == null)
            {
                Image img = bioCard.GetComponent<Image>();
                if (img != null) bioNormal = img.sprite;
            }
        }

        Sprite phyNormal = null, phySelected = null;
        if (phyCard != null)
        {
            SubjectCard sc = phyCard.GetComponent<SubjectCard>();
            if (sc != null)
            {
                phyNormal = sc.normalSprite;
                phySelected = sc.selectedSprite;
            }
            if (phyNormal == null)
            {
                Image img = phyCard.GetComponent<Image>();
                if (img != null) phyNormal = img.sprite;
            }
        }

        // 2. Banner Tiêu đề: CHỌN CHẾ ĐỘ CHƠI
        GameObject titleObj = new GameObject("TitlePlate_Game", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.5f, 0.5f);
        tRt.anchorMax = new Vector2(0.5f, 0.5f);
        tRt.anchoredPosition = new Vector2(0, 170);
        tRt.sizeDelta = new Vector2(580, 80);
        Image tImg = titleObj.GetComponent<Image>();
        if (titleSprite != null) tImg.sprite = titleSprite;

        GameObject titleTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleTextObj.transform.SetParent(titleObj.transform, false);
        RectTransform ttRt = titleTextObj.GetComponent<RectTransform>();
        ttRt.anchorMin = new Vector2(0.5f, 0.5f);
        ttRt.anchorMax = new Vector2(0.5f, 0.5f);
        ttRt.anchoredPosition = new Vector2(30, -3);
        ttRt.sizeDelta = new Vector2(380, 35);
        TextMeshProUGUI ttTmp = titleTextObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) ttTmp.font = fontAsset;
        ttTmp.text = "CHỌN CHẾ ĐỘ CHƠI";
        ttTmp.fontSize = 23f;
        ttTmp.fontStyle = FontStyles.Bold;
        ttTmp.color = new Color(0.24f, 0.15f, 0.08f, 1f); // Nâu sẫm
        ttTmp.alignment = TextAlignmentOptions.Center;

        // 3. Thẻ Game 1: VƯỢT CHƯỚNG NGẠI VẬT (Game của tôi)
        GameObject card1 = new GameObject("ObstacleRunnerCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(GameModeCard));
        card1.transform.SetParent(panelObj.transform, false);
        RectTransform c1Rt = card1.GetComponent<RectTransform>();
        c1Rt.anchorMin = new Vector2(0.5f, 0.5f);
        c1Rt.anchorMax = new Vector2(0.5f, 0.5f);
        c1Rt.anchoredPosition = new Vector2(-140, 11);
        c1Rt.sizeDelta = new Vector2(185, 235);

        Image c1Img = card1.GetComponent<Image>();
        if (bioNormal != null) c1Img.sprite = bioNormal;

        GameModeCard gmc1 = card1.GetComponent<GameModeCard>();
        gmc1.modeType = GameModeCard.GameModeType.ObstacleRunner;
        gmc1.cardImage = c1Img;
        gmc1.normalSprite = bioNormal;
        gmc1.selectedSprite = bioSelected;
        gmc1.hoverScale = 1.05f;

        obstacleRunnerBtn = card1.GetComponent<Button>();
        obstacleRunnerBtn.onClick.AddListener(OnObstacleRunnerSelected);

        // Text tên game 1
        GameObject c1TextObj = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        c1TextObj.transform.SetParent(card1.transform, false);
        RectTransform c1tRt = c1TextObj.GetComponent<RectTransform>();
        c1tRt.anchorMin = new Vector2(0.5f, 0.5f);
        c1tRt.anchorMax = new Vector2(0.5f, 0.5f);
        c1tRt.anchoredPosition = new Vector2(0, -78);
        c1tRt.sizeDelta = new Vector2(170, 36);
        TextMeshProUGUI c1Tmp = c1TextObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) c1Tmp.font = fontAsset;
        c1Tmp.text = "VƯỢT CHƯỚNG NGẠI VẬT";
        c1Tmp.fontSize = 13.5f;
        c1Tmp.fontStyle = FontStyles.Bold;
        c1Tmp.color = new Color(0.14f, 0.26f, 0.10f, 1f); // Xanh sinh học sẫm
        c1Tmp.alignment = TextAlignmentOptions.Center;

        // Subtext game 1
        GameObject c1SubObj = new GameObject("SubText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        c1SubObj.transform.SetParent(card1.transform, false);
        RectTransform c1sRt = c1SubObj.GetComponent<RectTransform>();
        c1sRt.anchorMin = new Vector2(0.5f, 0.5f);
        c1sRt.anchorMax = new Vector2(0.5f, 0.5f);
        c1sRt.anchoredPosition = new Vector2(0, -97);
        c1sRt.sizeDelta = new Vector2(170, 20);
        TextMeshProUGUI c1sTmp = c1SubObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) c1sTmp.font = fontAsset;
        c1sTmp.text = "Né vật cản & Đấu Boss";
        c1sTmp.fontSize = 10f;
        c1sTmp.fontStyle = FontStyles.Italic;
        c1sTmp.color = new Color(0.30f, 0.42f, 0.22f, 1f);
        c1sTmp.alignment = TextAlignmentOptions.Center;

        // 4. Thẻ Game 2: GAME SINH TỒN
        GameObject card2 = new GameObject("SurvivalCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(GameModeCard));
        card2.transform.SetParent(panelObj.transform, false);
        RectTransform c2Rt = card2.GetComponent<RectTransform>();
        c2Rt.anchorMin = new Vector2(0.5f, 0.5f);
        c2Rt.anchorMax = new Vector2(0.5f, 0.5f);
        c2Rt.anchoredPosition = new Vector2(140, 11);
        c2Rt.sizeDelta = new Vector2(185, 235);

        Image c2Img = card2.GetComponent<Image>();
        if (phyNormal != null) c2Img.sprite = phyNormal;

        GameModeCard gmc2 = card2.GetComponent<GameModeCard>();
        gmc2.modeType = GameModeCard.GameModeType.Survival;
        gmc2.cardImage = c2Img;
        gmc2.normalSprite = phyNormal;
        gmc2.selectedSprite = phySelected;
        gmc2.hoverScale = 1.05f;

        survivalGameBtn = card2.GetComponent<Button>();
        survivalGameBtn.onClick.AddListener(OnSurvivalGameSelected);

        // Text tên game 2
        GameObject c2TextObj = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        c2TextObj.transform.SetParent(card2.transform, false);
        RectTransform c2tRt = c2TextObj.GetComponent<RectTransform>();
        c2tRt.anchorMin = new Vector2(0.5f, 0.5f);
        c2tRt.anchorMax = new Vector2(0.5f, 0.5f);
        c2tRt.anchoredPosition = new Vector2(0, -78);
        c2tRt.sizeDelta = new Vector2(170, 36);
        TextMeshProUGUI c2Tmp = c2TextObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) c2Tmp.font = fontAsset;
        c2Tmp.text = "GAME SINH TỒN";
        c2Tmp.fontSize = 15f;
        c2Tmp.fontStyle = FontStyles.Bold;
        c2Tmp.color = new Color(0.48f, 0.22f, 0.08f, 1f); // Nâu cam ấm
        c2Tmp.alignment = TextAlignmentOptions.Center;

        // Subtext game 2
        GameObject c2SubObj = new GameObject("SubText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        c2SubObj.transform.SetParent(card2.transform, false);
        RectTransform c2sRt = c2SubObj.GetComponent<RectTransform>();
        c2sRt.anchorMin = new Vector2(0.5f, 0.5f);
        c2sRt.anchorMax = new Vector2(0.5f, 0.5f);
        c2sRt.anchoredPosition = new Vector2(0, -97);
        c2sRt.sizeDelta = new Vector2(170, 20);
        TextMeshProUGUI c2sTmp = c2SubObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) c2sTmp.font = fontAsset;
        c2sTmp.text = "Khám phá & Sinh tồn";
        c2sTmp.fontSize = 10f;
        c2sTmp.fontStyle = FontStyles.Italic;
        c2sTmp.color = new Color(0.55f, 0.35f, 0.22f, 1f);
        c2sTmp.alignment = TextAlignmentOptions.Center;

        // 5. Nút Quay Lại: ← Quay lại
        GameObject backBtnObj = new GameObject("Btn_BackToSubject", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        backBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform bRt = backBtnObj.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0.5f, 0.5f);
        bRt.anchorMax = new Vector2(0.5f, 0.5f);
        bRt.anchoredPosition = new Vector2(-330, 200);
        bRt.sizeDelta = new Vector2(130, 44);

        Image bImg = backBtnObj.GetComponent<Image>();
        bImg.color = new Color(0.85f, 0.76f, 0.62f, 1f); // Nền gỗ sáng
        backToSubjectBtn = backBtnObj.GetComponent<Button>();
        backToSubjectBtn.onClick.AddListener(OnBackToSubjectSelect);

        GameObject bTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        bTextObj.transform.SetParent(backBtnObj.transform, false);
        RectTransform btRt = bTextObj.GetComponent<RectTransform>();
        btRt.anchorMin = Vector2.zero;
        btRt.anchorMax = Vector2.one;
        btRt.offsetMin = Vector2.zero;
        btRt.offsetMax = Vector2.zero;
        TextMeshProUGUI btTmp = bTextObj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) btTmp.font = fontAsset;
        btTmp.text = "← Quay lại";
        btTmp.fontSize = 16f;
        btTmp.fontStyle = FontStyles.Bold;
        btTmp.color = new Color(0.25f, 0.15f, 0.08f, 1f);
        btTmp.alignment = TextAlignmentOptions.Center;

        // 6. Popup Thông Báo Game Sinh Tồn Đang Phát Triển
        BuildSurvivalNoticePopup(panelObj, fontAsset, titleSprite);
    }

    /// <summary>
    /// Dựng popup thông báo khi click vào Game Sinh Tồn.
    /// </summary>
    private void BuildSurvivalNoticePopup(GameObject parent, TMP_FontAsset fontAsset, Sprite titleSprite)
    {
        GameObject popupObj = new GameObject("SurvivalNoticeDialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        popupObj.transform.SetParent(parent.transform, false);
        RectTransform pRt = popupObj.GetComponent<RectTransform>();
        pRt.anchorMin = Vector2.zero;
        pRt.anchorMax = Vector2.one;
        pRt.offsetMin = Vector2.zero;
        pRt.offsetMax = Vector2.zero;

        Image pImg = popupObj.GetComponent<Image>();
        pImg.color = new Color(0f, 0f, 0f, 0.55f); // Làm mờ nền

        // Khung nội dung thông báo
        GameObject box = new GameObject("DialogBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        box.transform.SetParent(popupObj.transform, false);
        RectTransform boxRt = box.GetComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(460, 240);

        Image boxImg = box.GetComponent<Image>();
        if (titleSprite != null)
        {
            boxImg.sprite = titleSprite;
            boxImg.type = Image.Type.Simple;
        }
        else
        {
            boxImg.color = new Color(0.96f, 0.93f, 0.86f, 1f);
        }

        // Tiêu đề popup
        GameObject pTitle = new GameObject("PopupTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        pTitle.transform.SetParent(box.transform, false);
        RectTransform ptRt = pTitle.GetComponent<RectTransform>();
        ptRt.anchorMin = new Vector2(0.5f, 0.5f);
        ptRt.anchorMax = new Vector2(0.5f, 0.5f);
        ptRt.anchoredPosition = new Vector2(25, 45);
        ptRt.sizeDelta = new Vector2(380, 35);
        TextMeshProUGUI ptTmp = pTitle.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) ptTmp.font = fontAsset;
        ptTmp.text = "THÔNG BÁO";
        ptTmp.fontSize = 20f;
        ptTmp.fontStyle = FontStyles.Bold;
        ptTmp.color = new Color(0.25f, 0.15f, 0.08f, 1f);
        ptTmp.alignment = TextAlignmentOptions.Center;

        // Nội dung thông báo
        GameObject pMsg = new GameObject("PopupMessage", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        pMsg.transform.SetParent(box.transform, false);
        RectTransform pmRt = pMsg.GetComponent<RectTransform>();
        pmRt.anchorMin = new Vector2(0.5f, 0.5f);
        pmRt.anchorMax = new Vector2(0.5f, 0.5f);
        pmRt.anchoredPosition = new Vector2(25, 2);
        pmRt.sizeDelta = new Vector2(380, 50);
        TextMeshProUGUI pmTmp = pMsg.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) pmTmp.font = fontAsset;
        pmTmp.text = "Chế độ Game Sinh Tồn đang được phát triển!\nVui lòng trải nghiệm Game Vượt Chướng Ngại Vật nhé.";
        pmTmp.fontSize = 14f;
        pmTmp.color = new Color(0.35f, 0.22f, 0.12f, 1f);
        pmTmp.alignment = TextAlignmentOptions.Center;

        // Nút Đóng / Đã hiểu
        GameObject closeBtn = new GameObject("Btn_CloseNotice", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtn.transform.SetParent(box.transform, false);
        RectTransform cbRt = closeBtn.GetComponent<RectTransform>();
        cbRt.anchorMin = new Vector2(0.5f, 0.5f);
        cbRt.anchorMax = new Vector2(0.5f, 0.5f);
        cbRt.anchoredPosition = new Vector2(25, -45);
        cbRt.sizeDelta = new Vector2(120, 36);

        Image cbImg = closeBtn.GetComponent<Image>();
        cbImg.color = new Color(0.85f, 0.72f, 0.55f, 1f);

        Button btn = closeBtn.GetComponent<Button>();
        btn.onClick.AddListener(CloseSurvivalNotice);

        GameObject cbText = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        cbText.transform.SetParent(closeBtn.transform, false);
        RectTransform cbtRt = cbText.GetComponent<RectTransform>();
        cbtRt.anchorMin = Vector2.zero;
        cbtRt.anchorMax = Vector2.one;
        cbtRt.offsetMin = Vector2.zero;
        cbtRt.offsetMax = Vector2.zero;
        TextMeshProUGUI cbtTmp = cbText.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) cbtTmp.font = fontAsset;
        cbtTmp.text = "Đã hiểu";
        cbtTmp.fontSize = 15f;
        cbtTmp.fontStyle = FontStyles.Bold;
        cbtTmp.color = new Color(0.25f, 0.15f, 0.08f, 1f);
        cbtTmp.alignment = TextAlignmentOptions.Center;

        survivalNoticeDialog = popupObj;
        survivalNoticeDialog.SetActive(false);
    }

    /// <summary>
    /// Xử lý chọn môn học từ SubjectCard.
    /// CHÚ Ý QUAN TRỌNG: Chỉ mở màn chọn game khi chọn môn Sinh học!
    /// </summary>
    public void SelectSubject(string subjectName)
    {
        if (selectedSubjectText != null)
        {
            selectedSubjectText.text = "Môn đã chọn: " + subjectName;
        }

        Debug.Log("Chuẩn bị vào màn chơi cho môn: " + subjectName);

        string lower = subjectName.ToLower();
        if (lower.Contains("physic") || lower.Contains("vật lý") || lower.Contains("vật lí"))
        {
            SceneManager.LoadScene("Physics");
        }
        else if (lower.Contains("sinh") || lower.Contains("bio"))
        {
            // Chỉ thêm màn chọn game sau khi chọn môn Sinh học theo đúng yêu cầu
            OpenSinhGameSelectPanel();
        }
    }

    /// <summary>
    /// Ẩn 3 thẻ môn học và mở màn hình chọn 2 game của môn Sinh.
    /// </summary>
    public void OpenSinhGameSelectPanel()
    {
        Debug.Log("[MainMenuManager] 🧬 Mở màn hình chọn game môn Sinh Học (Vượt chướng ngại vật vs Sinh tồn)");

        // Ẩn bảng chọn môn ban đầu
        SetSubjectCardsActive(false);

        // Đảm bảo panel chọn game sẵn sàng và hiển thị
        if (sinhGameSelectPanel == null)
        {
            EnsureSinhGameSelectPanel();
        }

        if (sinhGameSelectPanel != null)
        {
            sinhGameSelectPanel.SetActive(true);
        }

        if (survivalNoticeDialog != null)
        {
            survivalNoticeDialog.SetActive(false);
        }
    }

    /// <summary>
    /// Quay lại màn chọn môn học ban đầu.
    /// </summary>
    public void OnBackToSubjectSelect()
    {
        Debug.Log("[MainMenuManager] ← Quay lại màn chọn 3 môn học");

        if (sinhGameSelectPanel != null)
        {
            sinhGameSelectPanel.SetActive(false);
        }

        SetSubjectCardsActive(true);
    }

    private void SetSubjectCardsActive(bool active)
    {
        if (titlePlate != null) titlePlate.SetActive(active);
        if (bioCard != null) bioCard.SetActive(active);
        if (chemCard != null) chemCard.SetActive(active);
        if (phyCard != null) phyCard.SetActive(active);
    }

    /// <summary>
    /// Khi click vào Game Vượt Chướng Ngại Vật (game của tôi):
    /// Tiếp tục flow scene như cũ!
    /// </summary>
    public void OnObstacleRunnerSelected()
    {
        Debug.Log("[MainMenuManager] 🏃 Đã chọn: Game Vượt Chướng Ngại Vật -> Tiếp tục luồng scene như cũ");
        EnterSinhSubject();
    }

    /// <summary>
    /// Khi click vào Game Sinh Tồn:
    /// Hiển thị thông báo đang phát triển.
    /// </summary>
    public void OnSurvivalGameSelected()
    {
        Debug.Log("[MainMenuManager] ⛺ Đã chọn: Game Sinh Tồn -> Hiện thông báo");
        if (survivalNoticeDialog != null)
        {
            survivalNoticeDialog.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Chế độ Game Sinh Tồn đang được phát triển!");
        }
    }

    public void CloseSurvivalNotice()
    {
        if (survivalNoticeDialog != null)
        {
            survivalNoticeDialog.SetActive(false);
        }
    }

    /// <summary>
    /// Vào môn Sinh học (Folder "Sinh") và tự động chuyển scene theo role:
    /// - Học sinh (Player): Vào ChapterList 1 để chơi như bình thường.
    /// - Giảng viên (Teacher): Vào TeacherCenter để xem thống kê điểm số và thêm/quản lý câu hỏi.
    /// </summary>
    public void EnterSinhSubject()
    {
        bool isTeacher = LoginManager.IsCurrentTeacher;
        string currentUser = LoginManager.CurrentUsername;

        Debug.Log($"[MainMenuManager] Chọn Game Vượt chướng ngại vật môn Sinh. User: '{currentUser}', IsTeacher: {isTeacher}");

        if (isTeacher)
        {
            Debug.Log($"[MainMenuManager] 🧑‍🏫 Giảng viên '{currentUser}' -> Mở Trung tâm Quản lý & Thống kê ({bioTeacherSceneName})");
            if (Application.CanStreamedLevelBeLoaded(bioTeacherSceneName))
            {
                SceneManager.LoadScene(bioTeacherSceneName);
            }
            else
            {
                Debug.LogError($"[MainMenuManager] Không tìm thấy scene '{bioTeacherSceneName}'. Hãy thêm scene vào Build Settings!");
                SceneManager.LoadScene(bioStudentSceneName);
            }
        }
        else
        {
            Debug.Log($"[MainMenuManager] 🎮 Học sinh '{currentUser}' (Player) -> Vào màn chơi như bình thường ({bioStudentSceneName})");
            SceneManager.LoadScene(bioStudentSceneName);
        }
    }

    public void OnBackToLoginClicked()
    {
        // Handle returning to login screen
        LoginManager loginManager = FindAnyObjectByType<LoginManager>();
        if (loginManager != null && loginManager.usePanelSwitching)
        {
            if (loginManager.menuGamePanel != null) loginManager.menuGamePanel.SetActive(false);
            if (loginManager.loginPanel != null) loginManager.loginPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("SampleScene"); // Or Login scene name
        }
    }
}
