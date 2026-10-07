using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quản lý logic cho Scene 4 — Chọn Chương & Chọn Bài Học (Hóa Học Lớp 8 - 3 Chương)
/// Xử lý chuẩn xác sự kiện click cho cả 3 Chương:
///   - Chương 1: Chất - Nguyên tử - Phân tử
///   - Chương 2: Phản ứng hóa học
///   - Chương 3: Mol và tính toán hóa học
/// </summary>
public class ChapterSelectUIController : MonoBehaviour
{
    [Header("=== Title Header ===")]
    public TextMeshProUGUI titleText;
    private string originalTitle = "Chọn Chương Học";

    [Header("=== Chapter View (Chọn Chương) ===")]
    public GameObject chapterContainer;
    public List<SelectableCard> chapterCards = new List<SelectableCard>();
    public List<Button> chapterButtons = new List<Button>();
    public List<ChapterData> chapterDataList = new List<ChapterData>();

    [Header("=== Lesson View (Chọn Bài Học) ===")]
    public GameObject lessonContainer;
    public List<SelectableCard> lessonCards = new List<SelectableCard>();
    public List<Button> lessonButtons = new List<Button>();

    private bool isInLessonView = false;
    public bool IsInLessonView => isInLessonView;

    private ChapterData currentSelectedChapter;
    private int currentSelectedChapterIndex = 0;
    private bool isNavigating = false;
    private readonly List<GameObject> dynamicallyCreatedLessonObjects = new List<GameObject>();

    private void OnEnable()
    {
        isNavigating = false;
    }

    private void Awake()
    {
        AutoBindComponents();
    }

    private void Start()
    {
        AutoBindComponents();
        BindChapterEvents();
        UpdateChapterCardTexts();
        ShowChapterList();
    }

    private void AutoBindComponents()
    {
        Transform searchRoot = transform.parent != null ? transform.parent : transform;
        if (transform.root != null && searchRoot == transform) searchRoot = transform.root;

        // 1. Tìm Title Text nếu chưa gán
        if (titleText == null)
        {
            TextMeshProUGUI[] tmps = searchRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                if (tmp.name.ToLower().Contains("title") || tmp.text.Contains("Chương") || tmp.transform.parent.name.ToLower().Contains("title"))
                {
                    titleText = tmp;
                    originalTitle = tmp.text;
                    break;
                }
            }
            if (titleText == null && tmps.Length > 0)
            {
                titleText = tmps[0];
                originalTitle = tmps[0].text;
            }
        }
        else
        {
            originalTitle = titleText.text;
        }

        chapterCards.Clear();
        chapterButtons.Clear();

        // 2. Tìm chính xác 3 GameObjects: Chapter1Row, Chapter2Row, Chapter3Row
        string[] rowNames = new string[] { "Chapter1Row", "Chapter2Row", "Chapter3Row" };
        foreach (var rName in rowNames)
        {
            Transform rowTransform = null;
            Transform[] allTransforms = searchRoot.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t.name.Equals(rName, System.StringComparison.OrdinalIgnoreCase))
                {
                    rowTransform = t;
                    break;
                }
            }

            if (rowTransform != null)
            {
                // Dọn sạch SelectableCard rác trên child Text nếu có
                SelectableCard[] childCards = rowTransform.GetComponentsInChildren<SelectableCard>(true);
                foreach (var sc in childCards)
                {
                    if (sc.gameObject != rowTransform.gameObject)
                    {
                        Destroy(sc);
                    }
                }

                // Tắt raycastTarget trên các text con để không chặn sự kiện click của thẻ
                TextMeshProUGUI[] rowTexts = rowTransform.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in rowTexts)
                {
                    tmp.raycastTarget = false;
                }

                // Lấy hoặc gắn SelectableCard trên chính Row
                SelectableCard rowCard = rowTransform.GetComponent<SelectableCard>();
                if (rowCard != null)
                {
                    rowCard.isLocked = false;
                    chapterCards.Add(rowCard);
                }

                // Lấy hoặc gắn Button trên chính Row
                Button rowBtn = rowTransform.GetComponent<Button>();
                if (rowBtn == null) rowBtn = rowTransform.gameObject.AddComponent<Button>();
                chapterButtons.Add(rowBtn);
            }
        }

        // Fallback nếu không tìm thấy theo đúng tên 3 Row
        if (chapterCards.Count == 0)
        {
            SelectableCard[] foundCards = searchRoot.GetComponentsInChildren<SelectableCard>(true);
            foreach (var card in foundCards)
            {
                if (card != null && !card.name.ToLower().Contains("back") && !card.name.ToLower().Contains("quay") && !card.name.ToLower().Contains("text") && !card.name.ToLower().Contains("tmp"))
                {
                    card.isLocked = false;
                    chapterCards.Add(card);

                    Button btn = card.GetComponent<Button>();
                    if (btn == null) btn = card.gameObject.AddComponent<Button>();
                    chapterButtons.Add(btn);
                }
            }
            SortCardsByNumber(chapterCards);
            if (chapterCards.Count > 3) chapterCards = chapterCards.GetRange(0, 3);
            if (chapterButtons.Count > 3) chapterButtons = chapterButtons.GetRange(0, 3);
        }

        // 3. Khởi tạo 3 Chương Hóa 8
        EnsureChapterDataList();
    }

    private void SortCardsByNumber(List<SelectableCard> list)
    {
        if (list == null || list.Count <= 1) return;
        list.Sort((a, b) => GetOrderIndex(a != null ? a.gameObject : null).CompareTo(GetOrderIndex(b != null ? b.gameObject : null)));
    }

    private void SortButtonsByNumber(List<Button> list)
    {
        if (list == null || list.Count <= 1) return;
        list.Sort((a, b) => GetOrderIndex(a != null ? a.gameObject : null).CompareTo(GetOrderIndex(b != null ? b.gameObject : null)));
    }

    private int GetOrderIndex(GameObject go)
    {
        if (go == null) return 999;
        Match match = Regex.Match(go.name, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int num))
        {
            return num;
        }
        return go.transform.GetSiblingIndex() + 100;
    }

    private void BindChapterEvents()
    {
        for (int i = 0; i < chapterCards.Count; i++)
        {
            int index = i;
            SelectableCard card = chapterCards[index];
            if (card != null)
            {
                card.isLocked = false;
                card.OnCardSelected -= (id) => OnChapterSelected(index);
                card.OnCardSelected += (id) => OnChapterSelected(index);
            }
        }

        for (int i = 0; i < chapterButtons.Count; i++)
        {
            int index = i;
            Button btn = chapterButtons[index];
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnChapterSelected(index));
                Debug.Log($"[ChapterSelect] Đã gán sự kiện Click cho Chương #{index + 1}: {btn.gameObject.name}");
            }
        }
    }

    /// <summary>
    /// Cập nhật tên 3 chương lên giao diện với chữ màu đen
    /// </summary>
    private void UpdateChapterCardTexts()
    {
        for (int i = 0; i < chapterCards.Count && i < chapterDataList.Count; i++)
        {
            if (chapterCards[i] != null && chapterDataList[i] != null)
            {
                FormatAndSetText(chapterCards[i].gameObject, chapterDataList[i].chapterTitle);
            }
        }
    }

    /// <summary>
    /// Cố định đúng 3 Chương Hóa Học Lớp 8
    /// </summary>
    private void EnsureChapterDataList()
    {
        if (chapterDataList == null) chapterDataList = new List<ChapterData>();

        string[] chemistryGrade8Chapters = new string[]
        {
            "Chương 1: Chất - Nguyên tử - Phân tử",
            "Chương 2: Phản ứng hóa học",
            "Chương 3: Mol và tính toán hóa học"
        };

        const int totalCount = 3;

        if (chapterDataList.Count > 3)
        {
            chapterDataList.RemoveRange(3, chapterDataList.Count - 3);
        }

        for (int i = 0; i < totalCount; i++)
        {
            if (i >= chapterDataList.Count || chapterDataList[i] == null)
            {
                ChapterData data = ScriptableObject.CreateInstance<ChapterData>();
                data.chapterNumber = i + 1;
                data.chapterTitle = chemistryGrade8Chapters[i];
                data.isUnlocked = true;
                data.lessons = CreateChemistryGrade8LessonsForChapter(i + 1);

                if (i >= chapterDataList.Count)
                {
                    chapterDataList.Add(data);
                }
                else
                {
                    chapterDataList[i] = data;
                }
            }
            else
            {
                chapterDataList[i].chapterNumber = i + 1;
                chapterDataList[i].chapterTitle = chemistryGrade8Chapters[i];
                chapterDataList[i].lessons = CreateChemistryGrade8LessonsForChapter(i + 1);
            }
        }
    }

    /// <summary>
    /// Danh mục các bài học Hóa 8 cho 3 chương
    /// </summary>
    private List<LessonData> CreateChemistryGrade8LessonsForChapter(int chapterNum)
    {
        List<LessonData> list = new List<LessonData>();

        string[][] chemistryGrade8Lessons = new string[][]
        {
            // Chương 1: Chất - Nguyên tử - Phân tử
            new string[]
            {
                "Bài 1: Mở đầu môn Hóa học & Chất",
                "Bài 2: Nguyên tử & Cấu tạo nguyên tử",
                "Bài 3: Nguyên tố hóa học & Đơn chất - Hợp chất",
                "Bài 4: Công thức hóa học & Quy tắc hóa trị"
            },
            // Chương 2: Phản ứng hóa học
            new string[]
            {
                "Bài 1: Sự biến đổi chất (Vật lý & Hóa học)",
                "Bài 2: Phản ứng hóa học & Hiện tượng nhận biết",
                "Bài 3: Định luật bảo toàn khối lượng",
                "Bài 4: Lập phương trình hóa học & Ý nghĩa"
            },
            // Chương 3: Mol và tính toán hóa học
            new string[]
            {
                "Bài 1: Mol, Khối lượng mol & Thể tích mol",
                "Bài 2: Chuyển đổi khối lượng, thể tích & mol",
                "Bài 3: Tỉ khối của chất khí",
                "Bài 4: Tính theo công thức & phương trình hóa học"
            }
        };

        string[] names;
        if (chapterNum >= 1 && chapterNum <= chemistryGrade8Lessons.Length)
        {
            names = chemistryGrade8Lessons[chapterNum - 1];
        }
        else
        {
            names = chemistryGrade8Lessons[0];
        }

        for (int j = 0; j < names.Length; j++)
        {
            LessonData lesson = new LessonData();
            lesson.lessonNumber = j + 1;
            lesson.lessonTitle = names[j];
            lesson.isUnlocked = true;
            list.Add(lesson);
        }

        return list;
    }

    /// <summary>
    /// Bước 1: Người chơi click vào 1 trong 3 Chương
    /// </summary>
    public void OnChapterSelected(int chapterIndex)
    {
        EnsureChapterDataList();

        currentSelectedChapterIndex = Mathf.Clamp(chapterIndex, 0, 2);
        currentSelectedChapter = (currentSelectedChapterIndex < chapterDataList.Count) ? chapterDataList[currentSelectedChapterIndex] : null;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.selectedChapter = currentSelectedChapter;
        }

        string chapterName = currentSelectedChapter != null ? currentSelectedChapter.chapterTitle : $"Chương {currentSelectedChapterIndex + 1}";
        Debug.Log($"<color=green>[ChapterSelect] Đã click chọn Chương #{currentSelectedChapterIndex + 1}: '{chapterName}'. Đang mở danh sách Bài học...</color>");

        ShowLessonList(currentSelectedChapter, currentSelectedChapterIndex);
    }

    /// <summary>
    /// Bước 2: Hiển thị danh sách Bài học
    /// </summary>
    public void ShowLessonList(ChapterData chapter, int chapterIndex)
    {
        isInLessonView = true;

        // 1. Tiêu đề chỉ ghi đúng "Chọn Bài Học"
        if (titleText != null)
        {
            titleText.text = "Chọn Bài Học";
        }

        // 2. Ẩn danh sách Chương
        SetChapterRowsActive(false);

        // 3. Hiển thị danh sách Bài học
        if (lessonContainer != null)
        {
            lessonContainer.SetActive(true);
            BindExistingLessonCards(chapter);
        }
        else
        {
            CreateDynamicLessonRows(chapter, chapterIndex);
        }
    }

    /// <summary>
    /// Bước 3: Người chơi click chọn Bài học -> Lưu vào GameSession và chuyển sang Scene 5 (DogSelect)
    /// </summary>
    public void SelectLessonAndProceed(int lessonIndex, LessonData lesson)
    {
        if (isNavigating) return;
        isNavigating = true;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.selectedLesson = lesson;
            GameSession.Instance.selectedLessonNumber = lessonIndex + 1;
            GameSession.Instance.selectedLessonTitle = lesson != null ? lesson.lessonTitle : $"Bài {lessonIndex + 1}";

            if (lesson != null && lesson.questions != null && lesson.questions.Count > 0)
            {
                GameSession.Instance.loadedQuestions = new List<QuestionData>(lesson.questions);
            }
            else if (currentSelectedChapter != null && currentSelectedChapter.questions != null && currentSelectedChapter.questions.Count > 0)
            {
                GameSession.Instance.loadedQuestions = new List<QuestionData>(currentSelectedChapter.questions);
            }
        }

        string lTitle = lesson != null ? lesson.lessonTitle : $"Bài {lessonIndex + 1}";
        Debug.Log($"<color=cyan>[ChapterSelect] Đã chọn Bài học: '{lTitle}'. Đang chuyển sang Scene 5 (DogSelect)...</color>");

        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_DOG_SELECT);
        }
        else
        {
            SceneManager.LoadScene("Scene5_DogSelect");
        }
    }

    /// <summary>
    /// Quay lại danh sách 3 Chương khi bấm Back từ màn chọn Bài
    /// </summary>
    public void BackToChapterList()
    {
        isInLessonView = false;
        isNavigating = false;

        ClearDynamicLessonRows();
        if (lessonContainer != null) lessonContainer.SetActive(false);

        if (titleText != null)
        {
            titleText.text = originalTitle;
        }

        SetChapterRowsActive(true);
        UpdateChapterCardTexts();

        Debug.Log("[ChapterSelect] Quay lại danh sách Chọn Chương.");
    }

    private void SetChapterRowsActive(bool active)
    {
        if (chapterContainer != null)
        {
            chapterContainer.SetActive(active);
            return;
        }

        foreach (var card in chapterCards)
        {
            if (card != null) card.gameObject.SetActive(active);
        }

        foreach (var btn in chapterButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(active);
            }
        }
    }

    private void BindExistingLessonCards(ChapterData chapter)
    {
        List<LessonData> lessons = (chapter != null && chapter.lessons != null && chapter.lessons.Count > 0)
            ? chapter.lessons
            : CreateChemistryGrade8LessonsForChapter(currentSelectedChapterIndex + 1);

        for (int i = 0; i < lessonCards.Count; i++)
        {
            int index = i;
            SelectableCard card = lessonCards[index];
            LessonData lData = (index < lessons.Count) ? lessons[index] : null;

            if (card != null)
            {
                card.gameObject.SetActive(index < lessons.Count);
                if (index < lessons.Count)
                {
                    FormatAndSetText(card.gameObject, lData != null ? lData.lessonTitle : $"Bài {index + 1}");

                    card.OnCardSelected -= (id) => SelectLessonAndProceed(index, lData);
                    card.OnCardSelected += (id) => SelectLessonAndProceed(index, lData);

                    Button btn = card.GetComponent<Button>();
                    if (btn == null) btn = card.GetComponentInChildren<Button>(true);
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SelectLessonAndProceed(index, lData));
                    }
                }
            }
        }
    }

    private void CreateDynamicLessonRows(ChapterData chapter, int chapterIndex)
    {
        ClearDynamicLessonRows();

        List<LessonData> lessons = (chapter != null && chapter.lessons != null && chapter.lessons.Count > 0)
            ? chapter.lessons
            : CreateChemistryGrade8LessonsForChapter(chapterIndex + 1);

        GameObject templateObj = (chapterCards.Count > 0 && chapterCards[0] != null) ? chapterCards[0].gameObject : null;
        Transform parentTransform = (templateObj != null && templateObj.transform.parent != null) ? templateObj.transform.parent : transform;

        float startY = 220f;
        float spacing = 105f;

        for (int i = 0; i < lessons.Count; i++)
        {
            int index = i;
            LessonData lData = lessons[index];

            GameObject rowObj;
            if (templateObj != null)
            {
                bool wasActive = templateObj.activeSelf;
                templateObj.SetActive(true);

                rowObj = Instantiate(templateObj, parentTransform);
                rowObj.name = $"Lesson_{index + 1}_Row";
                rowObj.SetActive(true);

                templateObj.SetActive(wasActive);

                RectTransform rt = rowObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, startY - (index * spacing));
                    rt.sizeDelta = new Vector2(850, 92);
                    rt.localScale = Vector3.one;
                }

                // Xóa SelectableCard phụ trên child Text
                foreach (var sc in rowObj.GetComponentsInChildren<SelectableCard>(true))
                {
                    if (sc.gameObject != rowObj)
                    {
                        Destroy(sc);
                    }
                }

                // Định dạng và gán text màu đen chuẩn
                FormatAndSetText(rowObj, lData.lessonTitle);

                SelectableCard card = rowObj.GetComponent<SelectableCard>();
                if (card != null)
                {
                    card.cardId = $"Lesson_{index + 1}";
                    card.groupName = "LessonGroup";
                    card.isSelected = false;
                    card.isLocked = false;
                    card.OnCardSelected -= (id) => SelectLessonAndProceed(index, lData);
                    card.OnCardSelected += (id) => SelectLessonAndProceed(index, lData);
                }

                Button btn = rowObj.GetComponent<Button>();
                if (btn == null) btn = rowObj.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectLessonAndProceed(index, lData));
                }
            }
            else
            {
                rowObj = new GameObject($"Lesson_{index + 1}_Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                rowObj.transform.SetParent(parentTransform, false);

                RectTransform rt = rowObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, startY - (index * spacing));
                rt.sizeDelta = new Vector2(850, 92);

                Image img = rowObj.GetComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 1f);

                FormatAndSetText(rowObj, lData.lessonTitle);

                Button btn = rowObj.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectLessonAndProceed(index, lData));
            }

            dynamicallyCreatedLessonObjects.Add(rowObj);
        }
    }

    /// <summary>
    /// Đảm bảo TextMeshProUGUI hiển thị chữ màu ĐEN sắc nét, không bị khuất, căn giữa khung
    /// </summary>
    private void FormatAndSetText(GameObject rootObj, string textContent)
    {
        TextMeshProUGUI txt = rootObj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (txt != null)
        {
            txt.gameObject.SetActive(true);
            txt.enabled = true;
            txt.raycastTarget = false; // Không cản trở click button

            txt.margin = Vector4.zero;
            txt.text = textContent;

            txt.color = Color.black;
            txt.faceColor = new Color32(0, 0, 0, 255);
            txt.enableVertexGradient = false;

            txt.fontSize = 28;
            txt.fontSizeMin = 18;
            txt.fontSizeMax = 28;
            txt.enableAutoSizing = true;
            txt.alignment = TextAlignmentOptions.Midline;
            txt.overflowMode = TextOverflowModes.Ellipsis;

            RectTransform textRt = txt.rectTransform;
            if (textRt != null)
            {
                textRt.anchorMin = new Vector2(0f, 0f);
                textRt.anchorMax = new Vector2(1f, 1f);
                textRt.offsetMin = new Vector2(130f, 0f);
                textRt.offsetMax = new Vector2(-130f, 0f);
                textRt.pivot = new Vector2(0.5f, 0.5f);
                textRt.anchoredPosition = Vector2.zero;
                textRt.localScale = Vector3.one;
            }

            txt.ForceMeshUpdate();
        }
    }

    // Các hàm tiện ích dùng gán trực tiếp Inspector OnClick nếu cần
    public void SelectChapter1() => OnChapterSelected(0);
    public void SelectChapter2() => OnChapterSelected(1);
    public void SelectChapter3() => OnChapterSelected(2);

    private void ClearDynamicLessonRows()
    {
        foreach (var obj in dynamicallyCreatedLessonObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        dynamicallyCreatedLessonObjects.Clear();
    }

    public void ShowChapterList()
    {
        BackToChapterList();
    }
}
