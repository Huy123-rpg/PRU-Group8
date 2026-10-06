using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ScienceQuest.Core;
using ScienceQuest.Quiz;
using System.Collections.Generic;

namespace ScienceQuest.UI
{
    /// <summary>
    /// Quản lý menu chọn Chương và Bài học phân hệ Vật Lý.
    /// Tự động dò tìm các nút trong giao diện và gắn sự kiện chuyển màn hình.
    /// 
    /// Flow: ChapterList → LessonList → (Popup chọn chế độ) → CharacterSelection → Physics (Bắn Vịt)
    /// </summary>
    public class PhysicsMenuManager : MonoBehaviour
    {
        // Lưu tên chương đã chọn giữa các scene
        public static string SelectedChapterName = "";
        public static string SelectedLessonName = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "ChapterList" || scene.name == "LessonList" || scene.name == "Physics")
            {
                if (FindFirstObjectByType<PhysicsMenuManager>() == null)
                {
                    GameObject go = new GameObject("PhysicsMenuManager_Auto");
                    go.AddComponent<PhysicsMenuManager>();
                    Debug.Log($"[PhysicsMenuManager] ⚙️ Tự động khởi tạo cho scene {scene.name}");
                }
            }
        }

        private void Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "Physics")
                SetupPhysicsScene();
            else if (sceneName == "ChapterList")
                SetupChapterListScene();
            else if (sceneName == "LessonList")
                SetupLessonListScene();
        }

        #region ===== PHYSICS SCENE (GAME BẮN VỊT) =====

        /// <summary>
        /// Tạo nút "Vào Học" trên scene Physics (game bắn vịt)
        /// </summary>
        private void SetupPhysicsScene()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                // Nếu chưa có Canvas (scene chỉ có game objects), tạo một overlay canvas
                GameObject canvasObj = new GameObject("UI_Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Tạo nút "📚 VÀO HỌC" ở góc trên phải
            GameObject btnObj = new GameObject("Btn_GoStudy");
            btnObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
            rt.sizeDelta = new Vector2(180f, 55f);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.9f, 0.9f); // Xanh dương

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => {
                Debug.Log("[PhysicsMenuManager] 📚 Quay về danh sách bài học / chương");
                if (!string.IsNullOrEmpty(SelectedChapterName))
                    SceneManager.LoadScene("LessonList");
                else
                    SceneManager.LoadScene("ChapterList");
            });

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "CHỌN BÀI";
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            Debug.Log("[PhysicsMenuManager] ✅ Đã tạo nút 'Vào Học' trên scene Physics");

            // Hiện đúng nhân vật đã chọn ở màn CharacterSelection
            ApplySelectedCharacter();
        }

        /// <summary>
        /// Đọc tên nhân vật đã chọn từ PlayerPrefs ("SelectedCharacterName"),
        /// tìm GameObject cùng tên trong scene Physics, bật lên và ẩn các nhân vật còn lại.
        /// Nhân vật trong Physics scene phải đặt tên trùng với CharacterSelection (cha1, cha2, cha3, cha4).
        /// </summary>
        private void ApplySelectedCharacter()
        {
            string selectedName = PlayerPrefs.GetString("SelectedCharacterName", "");
            if (string.IsNullOrEmpty(selectedName))
            {
                Debug.Log("[PhysicsMenuManager] ℹ️ Chưa chọn nhân vật — giữ nguyên mặc định");
                return;
            }

            // Tìm tất cả GameObject có tên dạng "cha" + số
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            bool foundSelected = false;

            foreach (GameObject obj in allObjects)
            {
                string n = obj.name.ToLower();
                if (n.Length >= 4 && n.StartsWith("cha") && char.IsDigit(n[3]))
                {
                    bool isSelected = obj.name.Equals(selectedName, System.StringComparison.OrdinalIgnoreCase);
                    obj.SetActive(isSelected);
                    if (isSelected)
                    {
                        foundSelected = true;
                        Debug.Log($"[PhysicsMenuManager] 🎭 Hiện nhân vật: {obj.name}");
                    }
                }
            }

            if (!foundSelected)
                Debug.LogWarning($"[PhysicsMenuManager] ⚠️ Không tìm thấy nhân vật '{selectedName}' trong Physics scene!");
        }

        #endregion

        #region ===== CHAPTER LIST SCENE =====

        private void SetupChapterListScene()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (Button btn in allButtons)
            {
                btn.interactable = true;

                // Đồng bộ màu sắc nút: đảm bảo tất cả các nút chương có cùng màu ở cả trạng thái thường và hover
                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
                cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
                cb.selectedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
                btn.colors = cb;

                Image btnImg = btn.GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = Color.white;
                }

                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                string text = tmp != null ? tmp.text.Trim() : "";

                // Nút Back → quay về Physics hoặc MainMenu
                if (text.Contains("Back") || text.Contains("BACK") || text.Contains("Quay lại") || btn.gameObject.name.ToLower().Contains("back"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[PhysicsMenuManager] ← Quay về màn chọn môn (SampleScene)");
                        SceneLoader.Instance.LoadScene("SampleScene");
                    });
                    continue;
                }

                // Nút chọn Chương → luôn mở khóa tự do, người chơi thích chọn chương nào cũng được
                string chapterName = "";
                if (text.StartsWith("Chương") || text.StartsWith("Ch\u01B0\u01A1ng") || text.Contains("Chương") || text.Contains("Ch\u01B0\u01A1ng"))
                {
                    chapterName = text;
                    if (chapterName.Contains("\n")) chapterName = chapterName.Split('\n')[0];
                    chapterName = chapterName.Trim();
                }
                else if (btn.gameObject.name.StartsWith("Btn_Chuong"))
                {
                    if (btn.gameObject.name.Contains("1")) chapterName = "Chương I: Năng lượng cơ học";
                    else if (btn.gameObject.name.Contains("2")) chapterName = "Chương II: Ánh sáng";
                    else if (btn.gameObject.name.Contains("3")) chapterName = "Chương III: Điện học";
                    else if (btn.gameObject.name.Contains("4")) chapterName = "Chương IV: Điện từ";
                    else if (btn.gameObject.name.Contains("5")) chapterName = "Chương V: Năng lượng với cuộc sống";
                }

                if (!string.IsNullOrEmpty(chapterName))
                {
                    string targetChapter = chapterName;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[PhysicsMenuManager] 📖 Chọn chương tự do: {targetChapter}");
                        SelectedChapterName = targetChapter;
                        QuizManager.SelectedChapter = targetChapter;
                        PlayerPrefs.SetString("SelectedChapter", targetChapter);
                        PlayerPrefs.Save();
                        SceneLoader.Instance.LoadScene("LessonList");
                    });
                    continue;
                }
            }

            Debug.Log("[PhysicsMenuManager] ✅ Đã mở khóa tự do toàn bộ các chương trong ChapterList");
        }

        #endregion

        #region ===== LESSON LIST SCENE =====

        // Danh sách bài học chuẩn cho từng chương Vật Lý (KHTN 8/9):
        // Chương I: Bài 2 -> 4
        // Chương II: Bài 5 -> 10
        // Chương III: Bài 11 -> 13
        // Chương IV: Bài 14 -> 15
        // Chương V: Bài 16 -> 17
        public static readonly Dictionary<int, string[]> ChapterLessons = new Dictionary<int, string[]>
        {
            { 1, new string[] { 
                "Bài 2: Động năng. Thế năng.", 
                "Bài 3: Cơ năng.", 
                "Bài 4: Công và công suất." 
            } },
            { 2, new string[] { 
                "Bài 5: Khúc xạ ánh sáng.", 
                "Bài 6: Phản xạ toàn phần.", 
                "Bài 7: Lăng kính.", 
                "Bài 8: Thấu kính.", 
                "Bài 9: Thực hành đo tiêu cự\nthấu kính hội tụ.", 
                "Bài 10: Kính lúp.\nBài tập thấu kính." 
            } },
            { 3, new string[] { 
                "Bài 11: Điện trở. Định luật Ohm.", 
                "Bài 12: Đoạn mạch nối tiếp, song song.", 
                "Bài 13: Năng lượng dòng điện\n- Công suất điện." 
            } },
            { 4, new string[] { 
                "Bài 14: Cảm ứng điện từ.\nDòng điện xoay chiều.", 
                "Bài 15: Tác dụng của\ndòng điện xoay chiều." 
            } },
            { 5, new string[] { 
                "Bài 16: Vòng năng lượng trên Trái Đất.\nNăng lượng hoá thạch.", 
                "Bài 17: Một số dạng năng lượng tái tạo." 
            } }
        };

        public static int GetCurrentChapterIndex()
        {
            string name = (SelectedChapterName ?? "").ToUpper().Trim();

            // 1. Kiểm tra Chương IV (4) TRƯỚC để không bị dính chữ "V" trong "IV"
            if (name.Contains("CHƯƠNG IV") || name.Contains("CHUONG IV") || name.Contains("CHƯƠNG 4") || name.Contains("CHUONG 4") || name.EndsWith(" 4") || name.Contains("IV:") || name.Contains("BTN_CHUONG4"))
                return 4;

            // 2. Kiểm tra Chương V (5)
            if (name.Contains("CHƯƠNG V") || name.Contains("CHUONG V") || name.Contains("CHƯƠNG 5") || name.Contains("CHUONG 5") || name.EndsWith(" 5") || name.Contains("BTN_CHUONG5"))
                return 5;

            // 3. Kiểm tra Chương III (3)
            if (name.Contains("CHƯƠNG III") || name.Contains("CHUONG III") || name.Contains("CHƯƠNG 3") || name.Contains("CHUONG 3") || name.EndsWith(" 3") || name.Contains("BTN_CHUONG3"))
                return 3;

            // 4. Kiểm tra Chương II (2)
            if (name.Contains("CHƯƠNG II") || name.Contains("CHUONG II") || name.Contains("CHƯƠNG 2") || name.Contains("CHUONG 2") || name.EndsWith(" 2") || name.Contains("BTN_CHUONG2"))
                return 2;

            // 5. Mặc định là Chương I (1)
            return 1;
        }

        public static string GetChapterRoman(int index)
        {
            switch (index)
            {
                case 1: return "I";
                case 2: return "II";
                case 3: return "III";
                case 4: return "IV";
                case 5: return "V";
                default: return "I";
            }
        }

        // Các reference cần tìm trong LessonList
        private GameObject popupChonCheDo;
        private Button btnTracNghiem;
        private Button btnTuLuan;

        private void SetupLessonListScene()
        {
            if (string.IsNullOrEmpty(SelectedChapterName))
            {
                SelectedChapterName = PlayerPrefs.GetString("SelectedChapter", "Chương I: Năng lượng cơ học");
            }
            QuizManager.SelectedChapter = SelectedChapterName;

            // Cập nhật tiêu đề chương
            UpdateChapterTitle();

            // Tìm popup "Chọn Hình Thức Luyện Tập"
            FindPopup();

            // Gắn sự kiện ngay cho các nút trong popup (kể cả khi popup đang ẩn)
            AssignPopupButtons();

            // Cập nhật text và sự kiện các nút bài học theo chương đã chọn
            UpdateLessonButtons();

            // Gắn sự kiện cho các nút điều hướng (Back, Ôn tập chương)
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (Button btn in allButtons)
            {
                if (popupChonCheDo != null && btn.transform.IsChildOf(popupChonCheDo.transform))
                    continue;

                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                string text = tmp != null ? tmp.text.Trim() : "";
                string textUpper = text.ToUpper();
                string btnName = btn.gameObject.name.ToLower();

                // Nút BACK ở góc trên màn hình
                if (textUpper == "BACK" || text.Contains("Quay lại") || btnName.Contains("back"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        if (popupChonCheDo != null && popupChonCheDo.activeSelf)
                        {
                            Debug.Log("[PhysicsMenuManager] ← Đang mở popup, nút Back góc trên sẽ đóng popup");
                            popupChonCheDo.SetActive(false);
                        }
                        else
                        {
                            Debug.Log("[PhysicsMenuManager] ← Quay về ChapterList");
                            SceneLoader.Instance.LoadScene("ChapterList");
                        }
                    });
                    continue;
                }

                // Nút "Ôn Tập Chương" → hiện popup chọn chế độ
                if (textUpper.Contains("ÔN T") || textUpper.Contains("\u00D4N T\u1EACP") || btnName.Contains("ontap"))
                {
                    int chIdx = GetCurrentChapterIndex();
                    string roman = GetChapterRoman(chIdx);
                    string onTapLesson = "Ôn tập Chương " + roman;

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[PhysicsMenuManager] 📝 Ôn tập chương {roman} → Hiện popup chọn chế độ");
                        SelectedLessonName = onTapLesson;
                        ShowPopup();
                    });
                    continue;
                }
            }

            // ===== BƯỚC 2: Tìm nút Trắc Nghiệm / Tự Luận / BACK popup bằng text (có thể chưa có Button component) =====
            List<GameObject> popupRelatedObjects = new List<GameObject>();
            TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

            foreach (TextMeshProUGUI tmp in allTexts)
            {
                if (tmp.gameObject.scene.name != SceneManager.GetActiveScene().name) continue;
                string text = tmp.text.Trim();
                string textLower = text.ToLower();

                // Nút "Trắc Nghiệm" → Vào thẳng màn Bắn Vịt (Physics)
                if (textLower.Contains("trắc") || textLower.Contains("tr\u1EAFc"))
                {
                    // Tìm object cha có thể click được (Image hoặc chính nó)
                    GameObject clickTarget = FindClickableParent(tmp.gameObject);
                    Button btn = EnsureButtonComponent(clickTarget);
                    btnTracNghiem = btn;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[PhysicsMenuManager] 🎯 Chế độ: Trắc Nghiệm → Physics (Bắn Vịt)");
                        QuizManager.IsDuckShootingMode = true;
                        QuizManager.SelectedChapter = SelectedChapterName;
                        PlayerPrefs.SetString("QuizChapter", SelectedChapterName);
                        PlayerPrefs.SetString("QuizLesson", SelectedLessonName);
                        PlayerPrefs.SetString("QuizMode", "TracNghiem");
                        PlayerPrefs.Save();
                        SceneLoader.Instance.LoadScene("CharacterSelection");
                    });
                    popupRelatedObjects.Add(clickTarget);
                    Debug.Log($"[PhysicsMenuManager] ✅ Gắn nút Trắc Nghiệm → {clickTarget.name}");
                    continue;
                }

                // Nút "Tự Luận" → Vào thẳng màn Bắn Vịt (Physics)
                if (textLower.Contains("tự lu") || textLower.Contains("t\u1EF1 lu"))
                {
                    GameObject clickTarget = FindClickableParent(tmp.gameObject);
                    Button btn = EnsureButtonComponent(clickTarget);
                    btnTuLuan = btn;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[PhysicsMenuManager] ✍️ Chế độ: Tự Luận → Physics (Bắn Vịt)");
                        QuizManager.IsDuckShootingMode = true;
                        QuizManager.SelectedChapter = SelectedChapterName;
                        PlayerPrefs.SetString("QuizChapter", SelectedChapterName);
                        PlayerPrefs.SetString("QuizLesson", SelectedLessonName);
                        PlayerPrefs.SetString("QuizMode", "TuLuan");
                        PlayerPrefs.Save();
                        SceneLoader.Instance.LoadScene("CharacterSelection");
                    });
                    popupRelatedObjects.Add(clickTarget);
                    Debug.Log($"[PhysicsMenuManager] ✅ Gắn nút Tự Luận → {clickTarget.name}");
                    continue;
                }

                // Text "CHỌN HÌNH THỨC LUYỆN TẬP" (label tiêu đề popup)
                if (textLower.Contains("hình thức") || textLower.Contains("h\u00ECnh th\u1EE9c"))
                {
                    popupRelatedObjects.Add(tmp.gameObject);
                    // Nếu có parent là Image, thêm luôn
                    if (tmp.transform.parent != null)
                        popupRelatedObjects.Add(tmp.transform.parent.gameObject);
                    continue;
                }
            }

            // ===== BƯỚC 3: Gom tất cả popup-related objects vào Popup_ChonCheDo =====
            if (popupChonCheDo != null)
            {
                foreach (var obj in popupRelatedObjects)
                {
                    if (obj != popupChonCheDo && obj.transform.parent != popupChonCheDo.transform)
                    {
                        obj.transform.SetParent(popupChonCheDo.transform, true);
                        Debug.Log($"[PhysicsMenuManager] 🛠️ Gom {obj.name} vào Popup_ChonCheDo");
                    }
                }

                // Ẩn popup ban đầu
                popupChonCheDo.SetActive(false);
                Debug.Log("[PhysicsMenuManager] 🔲 Đã ẩn popup chọn chế độ ban đầu");
            }

            Debug.Log("[PhysicsMenuManager] ✅ Đã gán xong sự kiện cho LessonList");
        }

        /// <summary>
        /// Tìm object cha có Image component (thường là cái nền nút) để gắn Button component vào
        /// </summary>
        private GameObject FindClickableParent(GameObject textObj)
        {
            // Nếu parent có Image → đó chính là nút
            Transform parent = textObj.transform.parent;
            if (parent != null && parent.GetComponent<Image>() != null)
            {
                return parent.gameObject;
            }
            // Nếu chính nó có Image → dùng luôn
            if (textObj.GetComponent<Image>() != null)
            {
                return textObj;
            }
            // Fallback: dùng parent hoặc chính nó
            return parent != null ? parent.gameObject : textObj;
        }

        /// <summary>
        /// Đảm bảo object có Button component, thêm mới nếu chưa có
        /// </summary>
        private Button EnsureButtonComponent(GameObject obj)
        {
            Button btn = obj.GetComponent<Button>();
            if (btn == null)
            {
                btn = obj.AddComponent<Button>();
                Debug.Log($"[PhysicsMenuManager] ➕ Đã tự động thêm Button component vào {obj.name}");
            }
            Image img = obj.GetComponent<Image>();
            if (img != null && btn.targetGraphic == null)
            {
                btn.targetGraphic = img;
            }
            btn.interactable = true;
            return btn;
        }

        /// <summary>
        /// Cập nhật text, sự kiện và số lượng nút bài học tương ứng với chương đã chọn.
        /// Tự động sinh thêm nút nếu chương có nhiều bài hơn (ví dụ Chương II có 6 bài),
        /// tự động căn chỉnh vị trí nút Ôn tập và co giãn chiều cao ScrollView để cuộn mượt mà.
        /// </summary>
        private void UpdateLessonButtons()
        {
            int chapterIndex = GetCurrentChapterIndex();
            string[] lessons = ChapterLessons.ContainsKey(chapterIndex) ? ChapterLessons[chapterIndex] : ChapterLessons[1];

            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            List<Button> lessonButtons = new List<Button>();
            Button ontapButton = null;

            foreach (Button btn in allButtons)
            {
                if (popupChonCheDo != null && btn.transform.IsChildOf(popupChonCheDo.transform))
                    continue;

                string btnName = btn.gameObject.name.ToLower();
                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                string text = tmp != null ? tmp.text.Trim() : "";
                string textUpper = text.ToUpper();

                if (textUpper == "BACK" || text.Contains("Quay lại") || btnName.Contains("back"))
                    continue;

                if (textUpper.Contains("ÔN T") || textUpper.Contains("\u00D4N T\u1EACP") || btnName.Contains("ontap"))
                {
                    ontapButton = btn;
                    continue;
                }

                // Nút bài học (bắt đầu bằng "Bài", hoặc con của LessonDetail)
                bool isLesson = text.StartsWith("Bài") || text.StartsWith("B\u00E0i") 
                                || btnName.Contains("lesson") || btnName.Contains("bai");
                if (!isLesson && btn.transform.parent != null && btn.transform.parent.name == "LessonDetail")
                {
                    isLesson = true;
                }

                if (isLesson)
                {
                    lessonButtons.Add(btn);
                }
            }

            // Sắp xếp các nút từ trên xuống dưới theo toạ độ Y
            lessonButtons.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

            if (lessonButtons.Count == 0) return;

            // Tìm container LessonDetail và Content của ScrollView
            RectTransform lessonDetailRt = lessonButtons[0].transform.parent as RectTransform;
            RectTransform contentRt = lessonDetailRt != null ? lessonDetailRt.parent as RectTransform : null;

            float itemHeight = 155f;
            float startY = -100f;
            float totalHeight = (lessons.Length + 1) * itemHeight + 160f;

            if (lessonDetailRt != null)
            {
                lessonDetailRt.anchorMin = new Vector2(0.5f, 1f);
                lessonDetailRt.anchorMax = new Vector2(0.5f, 1f);
                lessonDetailRt.pivot = new Vector2(0.5f, 1f);
                lessonDetailRt.anchoredPosition = Vector2.zero;
                lessonDetailRt.sizeDelta = new Vector2(lessonDetailRt.sizeDelta.x, totalHeight);
            }

            if (contentRt != null)
            {
                contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Max(totalHeight, 780f));
                contentRt.anchoredPosition = Vector2.zero;
            }

            // Cuộn ScrollView về đầu danh sách
            ScrollRect scrollRect = FindFirstObjectByType<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }

            // Nhân bản thêm nút nếu số bài trong chương lớn hơn số nút có sẵn (như Chương II có 6 bài)
            Button templateBtn = lessonButtons[0];
            Transform parentContainer = lessonDetailRt != null ? lessonDetailRt : templateBtn.transform.parent;

            while (lessonButtons.Count < lessons.Length)
            {
                GameObject newBtnObj = Instantiate(templateBtn.gameObject, parentContainer);
                newBtnObj.name = $"Btn_Lesson_Auto_{lessonButtons.Count + 1}";
                Button newBtn = newBtnObj.GetComponent<Button>();
                lessonButtons.Add(newBtn);
            }

            // Cập nhật vị trí, nội dung bài học và sự kiện click
            for (int i = 0; i < lessonButtons.Count; i++)
            {
                Button btn = lessonButtons[i];
                if (i < lessons.Length)
                {
                    btn.gameObject.SetActive(true);
                    string currentLesson = lessons[i];

                    RectTransform btnRt = btn.GetComponent<RectTransform>();
                    if (btnRt != null)
                    {
                        btnRt.anchorMin = new Vector2(0.5f, 1f);
                        btnRt.anchorMax = new Vector2(0.5f, 1f);
                        btnRt.pivot = new Vector2(0.5f, 1f);
                        btnRt.anchoredPosition = new Vector2(0f, startY - i * itemHeight);
                    }

                    TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmp != null)
                    {
                        tmp.text = currentLesson;
                        // Bật ngắt dòng và căn giữa chuẩn cả chiều ngang lẫn chiều dọc
                        tmp.textWrappingMode = TextWrappingModes.Normal;
                        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
                        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 20f;
                        tmp.fontSizeMax = 26f;
                        tmp.margin = new Vector4(35f, 4f, 35f, 4f);
                        tmp.lineSpacing = -8f; // Thu hẹp khoảng cách giữa 2 dòng để cân đối hoàn hảo trong nút
                    }

                    string cleanLessonName = currentLesson.Replace("\n", " ");
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[PhysicsMenuManager] 📝 Chọn bài: {cleanLessonName} → Hiện popup chọn chế độ");
                        SelectedLessonName = cleanLessonName;
                        ShowPopup();
                    });
                }
                else
                {
                    // Ẩn nút thừa nếu chương có ít bài hơn
                    btn.gameObject.SetActive(false);
                }
            }

            // Đặt nút Ôn tập chương nằm ngay bên dưới bài học cuối cùng
            if (ontapButton != null)
            {
                RectTransform ontapRt = ontapButton.GetComponent<RectTransform>();
                if (ontapRt != null)
                {
                    ontapRt.anchorMin = new Vector2(0.5f, 1f);
                    ontapRt.anchorMax = new Vector2(0.5f, 1f);
                    ontapRt.pivot = new Vector2(0.5f, 1f);
                    ontapRt.anchoredPosition = new Vector2(0f, startY - lessons.Length * itemHeight);
                }

                TextMeshProUGUI tmpOntap = ontapButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmpOntap != null)
                {
                    tmpOntap.textWrappingMode = TextWrappingModes.Normal;
                    tmpOntap.margin = new Vector4(55f, 6f, 55f, 6f);
                    tmpOntap.fontSize = 28f;
                    tmpOntap.alignment = TextAlignmentOptions.Center;
                }
            }

            Debug.Log($"[PhysicsMenuManager] 📚 Đã hiển thị đầy đủ {lessons.Length} bài học cho Chương {GetChapterRoman(chapterIndex)}");
        }

        /// <summary>
        /// Cập nhật tiêu đề chương trên scene LessonList
        /// </summary>
        private void UpdateChapterTitle()
        {
            if (string.IsNullOrEmpty(SelectedChapterName)) return;

            int chIdx = GetCurrentChapterIndex();
            string roman = GetChapterRoman(chIdx);
            string chapterHeader = "CHƯƠNG " + roman;

            // Tìm Text_Tittle hoặc text có chữ "CHƯƠNG"
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var tmp in allTexts)
            {
                string upper = tmp.text.Trim().ToUpper();
                if ((upper.StartsWith("CH\u01AF\u01A0NG") || upper.StartsWith("CHƯƠNG") || upper.StartsWith("CHUONG") || tmp.gameObject.name == "Text_Tittle") 
                    && !upper.Contains("ÔN T") && !upper.Contains("\u00D4N T\u1EACP"))
                {
                    tmp.text = chapterHeader;
                    Debug.Log($"[PhysicsMenuManager] 📋 Cập nhật tiêu đề chương: {chapterHeader}");
                }
                else if (upper.Contains("ÔN TẬP") || upper.Contains("\u00D4N T\u1EACP") || tmp.gameObject.name.ToLower().Contains("ontap"))
                {
                    tmp.text = "ÔN TẬP " + chapterHeader;
                    Debug.Log($"[PhysicsMenuManager] 📋 Cập nhật nút ôn tập: ÔN TẬP {chapterHeader}");
                }
            }
        }

        /// <summary>
        /// Tìm popup "Chọn Hình Thức Luyện Tập" bằng tên GameObject (kể cả inactive)
        /// </summary>
        private void FindPopup()
        {
            // Tìm tất cả Transform kể cả inactive
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in allTransforms)
            {
                if (t.gameObject.scene.name != SceneManager.GetActiveScene().name) continue;
                
                if (t.name == "Popup_ChonCheDo" || t.name.ToLower().Contains("popup_chon"))
                {
                    popupChonCheDo = t.gameObject;
                    Debug.Log($"[PhysicsMenuManager] ✅ Đã tìm thấy popup: {t.name}");
                    return;
                }
            }

            Debug.LogWarning("[PhysicsMenuManager] ⚠️ Không tìm thấy popup chọn chế độ!");
        }

        /// <summary>
        /// Hiển thị popup chọn chế độ và gắn sự kiện cho nút bên trong
        /// </summary>
        private void ShowPopup()
        {
            if (popupChonCheDo != null)
            {
                popupChonCheDo.SetActive(true);
                Debug.Log("[PhysicsMenuManager] 🔲 Hiện popup chọn chế độ");

                // Gắn sự kiện cho các nút BÊN TRONG popup ngay lúc hiện
                AssignPopupButtons();
            }
            else
            {
                Debug.LogWarning("[PhysicsMenuManager] ⚠️ Không có popup → Load thẳng Physics (Bắn Vịt)");
                QuizManager.SelectedChapter = SelectedChapterName;
                PlayerPrefs.SetString("QuizChapter", SelectedChapterName);
                PlayerPrefs.SetString("QuizLesson", SelectedLessonName);
                PlayerPrefs.SetString("QuizMode", "TracNghiem");
                PlayerPrefs.Save();
                SceneLoader.Instance.LoadScene("CharacterSelection");
            }
        }

        /// <summary>
        /// Gắn sự kiện click cho các nút trong popup (gọi SAU KHI popup đã SetActive(true))
        /// </summary>
        private void AssignPopupButtons()
        {
            if (popupChonCheDo == null) return;

            // 1. Quét tất cả TextMeshProUGUI trong popup để đảm bảo mọi nút (kể cả chưa có Button) đều được gắn Button
            TextMeshProUGUI[] allPopupTexts = popupChonCheDo.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allPopupTexts)
            {
                string text = tmp.text.Trim();
                string textLower = text.ToLower();

                // Nút "BACK" trong popup → đóng popup
                if (textLower == "back" || textLower.Contains("quay") || textLower.Contains("đóng") || textLower.Contains("dong"))
                {
                    GameObject clickTarget = FindClickableParent(tmp.gameObject);
                    Button btn = EnsureButtonComponent(clickTarget);
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[PhysicsMenuManager] ← Đóng popup chọn chế độ (từ nút BACK popup)");
                        popupChonCheDo.SetActive(false);
                    });
                    Debug.Log($"[PhysicsMenuManager] ✅ Đã gắn sự kiện nút BACK (đóng popup) vào {clickTarget.name}");
                }
                // Nút "Trắc Nghiệm"
                else if (textLower.Contains("trắc") || textLower.Contains("tr\u1EAFc") || textLower.Contains("trac"))
                {
                    GameObject clickTarget = FindClickableParent(tmp.gameObject);
                    Button btn = EnsureButtonComponent(clickTarget);
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[PhysicsMenuManager] 🎯 Chế độ: Trắc Nghiệm → Physics (Bắn Vịt)");
                        QuizManager.IsDuckShootingMode = true;
                        QuizManager.SelectedChapter = SelectedChapterName;
                        PlayerPrefs.SetString("QuizChapter", SelectedChapterName);
                        PlayerPrefs.SetString("QuizLesson", SelectedLessonName);
                        PlayerPrefs.SetString("QuizMode", "TracNghiem");
                        PlayerPrefs.Save();
                        SceneLoader.Instance.LoadScene("CharacterSelection");
                    });
                    Debug.Log($"[PhysicsMenuManager] ✅ Đã gắn sự kiện nút Trắc Nghiệm vào {clickTarget.name}");
                }
                // Nút "Tự Luận"
                else if (textLower.Contains("tự") || textLower.Contains("t\u1EF1") || textLower.Contains("tu lu"))
                {
                    GameObject clickTarget = FindClickableParent(tmp.gameObject);
                    Button btn = EnsureButtonComponent(clickTarget);
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[PhysicsMenuManager] ✍️ Chế độ: Tự Luận → Physics (Bắn Vịt)");
                        QuizManager.IsDuckShootingMode = true;
                        QuizManager.SelectedChapter = SelectedChapterName;
                        PlayerPrefs.SetString("QuizChapter", SelectedChapterName);
                        PlayerPrefs.SetString("QuizLesson", SelectedLessonName);
                        PlayerPrefs.SetString("QuizMode", "TuLuan");
                        PlayerPrefs.Save();
                        SceneLoader.Instance.LoadScene("CharacterSelection");
                    });
                    Debug.Log($"[PhysicsMenuManager] ✅ Đã gắn sự kiện nút Tự Luận vào {clickTarget.name}");
                }
            }

            // 2. Quét thêm bất kỳ Button nào có tên chứa 'back' hoặc 'close' trong popup
            Button[] popupButtons = popupChonCheDo.GetComponentsInChildren<Button>(true);
            foreach (Button btn in popupButtons)
            {
                string bName = btn.gameObject.name.ToLower();
                if (bName.Contains("back") || bName.Contains("close") || bName.Contains("quay") || bName.Contains("dong"))
                {
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[PhysicsMenuManager] ← Đóng popup qua nút {btn.gameObject.name}");
                        popupChonCheDo.SetActive(false);
                    });
                }
            }
        }

        #endregion
    }
}

