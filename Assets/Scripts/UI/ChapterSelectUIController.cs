using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý logic cho Scene 4 — Chapter Select (Chọn Chương / Bài Học)
/// Khi người chơi click chọn 1 Chương -> Lưu vào GameSession và chuyển sang Scene 5 (DogSelect)
/// </summary>
public class ChapterSelectUIController : MonoBehaviour
{
    [Header("Chapter Cards (Thẻ chọn Chương)")]
    public List<SelectableCard> chapterCards = new List<SelectableCard>();

    [Header("Alternative Chapter Buttons (Nút bấm Chương dự phòng)")]
    public List<Button> chapterButtons = new List<Button>();

    [Header("Chapter Data List (Tùy chọn - Tự động tạo nếu trống)")]
    public List<ChapterData> chapterDataList = new List<ChapterData>();

    private bool isNavigating = false;

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
        BindEvents();
    }

    private void AutoBindComponents()
    {
        // 1. Làm sạch danh sách khỏi các phần tử null
        if (chapterCards != null) chapterCards.RemoveAll(c => c == null);
        if (chapterButtons != null) chapterButtons.RemoveAll(b => b == null);

        Transform searchRoot = transform.parent != null ? transform.parent : transform;

        // 2. Tìm kiếm SelectableCard nếu danh sách chapterCards đang rỗng
        if (chapterCards == null || chapterCards.Count == 0)
        {
            if (chapterCards == null) chapterCards = new List<SelectableCard>();

            SelectableCard[] foundCards = searchRoot.GetComponentsInChildren<SelectableCard>(true);
            if (foundCards.Length == 0 && transform.root != null)
            {
                foundCards = transform.root.GetComponentsInChildren<SelectableCard>(true);
            }

            foreach (var card in foundCards)
            {
                if (card != null && !card.name.ToLower().Contains("back") && !card.name.ToLower().Contains("quay"))
                {
                    chapterCards.Add(card);
                }
            }
        }

        // Sắp xếp danh sách Card chuẩn theo thứ tự số Chương (1, 2, 3)
        SortCardsByChapterNumber(chapterCards);

        // Mở khóa toàn bộ Thẻ Chương (tránh bị kẹt isLocked = true trong Inspector)
        foreach (var card in chapterCards)
        {
            if (card != null)
            {
                card.isLocked = false;
            }
        }

        // 3. Tìm kiếm Button tương ứng
        if (chapterButtons == null || chapterButtons.Count == 0)
        {
            if (chapterButtons == null) chapterButtons = new List<Button>();

            // Lấy từ chapterCards trước
            foreach (var card in chapterCards)
            {
                if (card != null)
                {
                    Button btn = card.GetComponent<Button>();
                    if (btn == null) btn = card.GetComponentInChildren<Button>(true);
                    if (btn != null && !chapterButtons.Contains(btn))
                    {
                        chapterButtons.Add(btn);
                    }
                }
            }

            // Nếu không có card, quét toàn bộ Button trong Scene (bỏ nút Back/Quay lại)
            if (chapterButtons.Count == 0)
            {
                Button[] foundButtons = searchRoot.GetComponentsInChildren<Button>(true);
                if (foundButtons.Length == 0 && transform.root != null)
                {
                    foundButtons = transform.root.GetComponentsInChildren<Button>(true);
                }

                foreach (var btn in foundButtons)
                {
                    if (btn != null && btn.GetComponent<BackButton>() == null)
                    {
                        string lower = btn.name.ToLower();
                        if (!lower.Contains("back") && !lower.Contains("quay") && !lower.Contains("return") && !lower.Contains("arrow"))
                        {
                            chapterButtons.Add(btn);
                        }
                    }
                }
                SortButtonsByChapterNumber(chapterButtons);
            }
        }

        // 4. Sinh dữ liệu chương tương ứng
        EnsureChapterDataList();
    }

    private void SortCardsByChapterNumber(List<SelectableCard> list)
    {
        if (list == null || list.Count <= 1) return;
        list.Sort((a, b) => GetOrderIndex(a != null ? a.gameObject : null).CompareTo(GetOrderIndex(b != null ? b.gameObject : null)));
    }

    private void SortButtonsByChapterNumber(List<Button> list)
    {
        if (list == null || list.Count <= 1) return;
        list.Sort((a, b) => GetOrderIndex(a != null ? a.gameObject : null).CompareTo(GetOrderIndex(b != null ? b.gameObject : null)));
    }

    private int GetOrderIndex(GameObject go)
    {
        if (go == null) return 999;

        // Ưu tiên tìm số thứ tự trong tên GameObject (VD: "Chương 1", "Chapter1", "Row_2")
        Match match = Regex.Match(go.name, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int num))
        {
            return num;
        }

        return go.transform.GetSiblingIndex() + 100;
    }

    private void BindEvents()
    {
        // 1. Đăng ký sự kiện cho SelectableCards
        for (int i = 0; i < chapterCards.Count; i++)
        {
            int index = i;
            SelectableCard card = chapterCards[index];
            if (card != null)
            {
                card.isLocked = false;

                // Xóa bớt listener cũ bằng cách bỏ đăng ký (đảm bảo không bị lặp event)
                card.OnCardSelected -= (id) => SelectChapterAndProceed(index);
                card.OnCardSelected += (id) => SelectChapterAndProceed(index);

                Button btn = card.GetComponent<Button>();
                if (btn == null) btn = card.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectChapterAndProceed(index));
                }

                Debug.Log($"[ChapterSelect] Đã kết nối Card Chương #{index + 1}: {card.gameObject.name}");
            }
        }

        // 2. Đăng ký sự kiện cho Buttons độc lập (chưa có trong SelectableCards)
        for (int i = 0; i < chapterButtons.Count; i++)
        {
            int index = i;
            Button btn = chapterButtons[index];
            if (btn != null)
            {
                SelectableCard card = btn.GetComponent<SelectableCard>();
                if (card == null || !chapterCards.Contains(card))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectChapterAndProceed(index));
                    Debug.Log($"[ChapterSelect] Đã kết nối Button độc lập Chương #{index + 1}: {btn.gameObject.name}");
                }
            }
        }
    }

    private void EnsureChapterDataList()
    {
        if (chapterDataList == null) chapterDataList = new List<ChapterData>();

        string[] defaultTitles = new string[]
        {
            "Chương 1: Khái niệm cơ bản",
            "Chương 2: Các dạng bài tập và quy luật",
            "Chương 3: Bài tập tổng hợp & nâng cao"
        };

        int itemCount = Mathf.Max(chapterCards.Count, chapterButtons.Count);
        int totalCount = Mathf.Max(itemCount, 3);

        for (int i = 0; i < totalCount; i++)
        {
            if (i >= chapterDataList.Count || chapterDataList[i] == null)
            {
                ChapterData data = ScriptableObject.CreateInstance<ChapterData>();
                data.chapterNumber = i + 1;
                data.chapterTitle = i < defaultTitles.Length ? defaultTitles[i] : $"Chương {i + 1}";
                data.isUnlocked = true;

                if (i >= chapterDataList.Count)
                {
                    chapterDataList.Add(data);
                }
                else
                {
                    chapterDataList[i] = data;
                }
            }
        }
    }

    /// <summary>
    /// Lưu Chương đã chọn và chuyển sang Scene 5 (Scene5_DogSelect)
    /// </summary>
    public void SelectChapterAndProceed(int chapterIndex)
    {
        if (isNavigating)
        {
            Debug.LogWarning($"[ChapterSelect] Đã đang chuyển cảnh, bỏ qua thao tác click lại.");
            return;
        }
        isNavigating = true;

        EnsureChapterDataList();

        ChapterData selectedChapter = null;
        if (chapterIndex >= 0 && chapterIndex < chapterDataList.Count)
        {
            selectedChapter = chapterDataList[chapterIndex];
        }

        if (GameSession.Instance != null)
        {
            GameSession.Instance.selectedChapter = selectedChapter;
        }

        string chapterName = selectedChapter != null ? selectedChapter.chapterTitle : $"Chương {chapterIndex + 1}";
        Debug.Log($"[ChapterSelect] CLICKED Chương #{chapterIndex + 1} ({chapterName}). Đang chuyển sang Scene 5 (DogSelect)...");

        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.LoadScene(NavigationManager.SCENE_DOG_SELECT);
        }
        else
        {
            Debug.LogWarning("[ChapterSelect] NavigationManager chưa sẵn sàng, nạp trực tiếp Scene5_DogSelect");
            SceneManager.LoadScene("Scene5_DogSelect");
        }
    }

    // Các hàm tiện ích dùng để gán trực tiếp vào Unity Inspector Event (OnClick)
    public void SelectChapter1() => SelectChapterAndProceed(0);
    public void SelectChapter2() => SelectChapterAndProceed(1);
    public void SelectChapter3() => SelectChapterAndProceed(2);
}
