using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class SubjectCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Subject Info")]
    public string subjectName = "Physics";

    [Header("Sprites")]
    public Image cardImage;
    public Sprite normalSprite;
    public Sprite selectedSprite;

    [Header("Animation Settings")]
    public float hoverScale = 1.05f;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }

        // Đảm bảo Image trên card nhận click (RaycastTarget = true)
        if (cardImage != null)
        {
            cardImage.raycastTarget = true;
        }

        // Tự động sửa subjectName nếu Inspector bị gán nhầm giá trị mặc định "Biology"
        AutoCorrectSubjectName();

        // Tắt raycastTarget trên các text con để click vào chữ vẫn ăn vào card
        TextMeshProUGUI[] childTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in childTexts)
        {
            txt.raycastTarget = false;
        }

        SetNormalState();
    }

    /// <summary>
    /// Nhận diện đúng môn học dựa theo tên GameObject hoặc chữ hiển thị trên Card
    /// để tránh lỗi copy-paste component trong Inspector.
    /// </summary>
    private void AutoCorrectSubjectName()
    {
        string objName = gameObject.name.ToLower();

        if (objName.Contains("phy"))
        {
            subjectName = "Physics";
            return;
        }
        if (objName.Contains("chem"))
        {
            subjectName = "Chemistry";
            return;
        }
        if (objName.Contains("bio"))
        {
            subjectName = "Biology";
            return;
        }

        // Kiểm tra text con
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null && !string.IsNullOrEmpty(tmp.text))
        {
            string t = tmp.text.ToLower();
            if (t.Contains("vật") || t.Contains("lý") || t.Contains("lí") || t.Contains("phy"))
                subjectName = "Physics";
            else if (t.Contains("hóa") || t.Contains("hoá") || t.Contains("chem"))
                subjectName = "Chemistry";
            else if (t.Contains("sinh") || t.Contains("bio"))
                subjectName = "Biology";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectedSprite != null && cardImage != null)
        {
            cardImage.sprite = selectedSprite;
        }
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNormalState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerSelect();
    }

    /// <summary>
    /// Xử lý chọn môn học với cơ chế an toàn 3 lớp
    /// </summary>
    public void TriggerSelect()
    {
        AutoCorrectSubjectName();
        Debug.Log($"[SubjectCard] 🎯 Đã bấm chọn môn học: '{subjectName}' (Object={gameObject.name})");

        // Lớp 1: Gọi qua MainMenuManager.Instance
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.SelectSubject(subjectName);
            return;
        }

        // Lớp 2: Tìm MainMenuManager trong scene (kể cả inactive)
        MainMenuManager menu = FindFirstObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        if (menu != null)
        {
            menu.SelectSubject(subjectName);
            return;
        }

        // Lớp 3: Fallback trực tiếp nếu MainMenuManager không tìm thấy
        string lower = subjectName.ToLower();
        if (lower.Contains("physic") || lower.Contains("vật lý") || lower.Contains("vật lí") || lower.Contains("phy"))
        {
            Debug.Log("[SubjectCard] 🚀 Fallback trực tiếp → Nạp Scene ChapterList");
            SceneManager.LoadScene("ChapterList");
        }
        else if (lower.Contains("chem") || lower.Contains("hóa") || lower.Contains("hoá") || lower.Contains("hoa"))
        {
            SceneManager.LoadScene("Scene3_LessonType");
        }
        else if (lower.Contains("bio") || lower.Contains("sinh"))
        {
            SceneManager.LoadScene("SinhHoc");
        }
    }

    public void SetNormalState()
    {
        if (normalSprite != null && cardImage != null)
        {
            cardImage.sprite = normalSprite;
        }
        transform.localScale = originalScale;
    }
}
