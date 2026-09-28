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
    /// Flow: ChapterList → LessonList → (Popup chọn chế độ) → QuizScene / QuizScene_TuLuan
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
                        Debug.Log("[PhysicsMenuManager] ← Quay về từ ChapterList");
                        SceneLoader.Instance.LoadScene("Physics");
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
                        SceneLoader.Instance.LoadScene("LessonList");
                    });
                    continue;
                }
            }

            Debug.Log("[PhysicsMenuManager] ✅ Đã mở khóa tự do toàn bộ các chương trong ChapterList");
        }

        #endregion

        #region ===== LESSON LIST SCENE =====

        // Các reference cần tìm trong LessonList
        private GameObject popupChonCheDo;
        private Button btnTracNghiem;
        private Button btnTuLuan;

        private void SetupLessonListScene()
        {
            // Cập nhật tiêu đề chương nếu có
            UpdateChapterTitle();

            // Tìm popup "Chọn Hình Thức Luyện Tập"
            FindPopup();

            // Gắn sự kiện ngay cho các nút trong popup (kể cả khi popup đang ẩn)
            AssignPopupButtons();

            // ===== BƯỚC 1: Gắn sự kiện cho các Button đã có sẵn =====
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (Button btn in allButtons)
            {
                // Bỏ qua các nút bên trong popup để AssignPopupButtons xử lý riêng
                if (popupChonCheDo != null && btn.transform.IsChildOf(popupChonCheDo.transform))
                    continue;

                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                string text = tmp != null ? tmp.text.Trim() : "";
                string textUpper = text.ToUpper();
                string btnName = btn.gameObject.name.ToLower();

                // Nút BACK ở góc trên màn hình → nếu đang mở popup thì đóng popup, nếu không thì quay về ChapterList
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
                if (textUpper.Contains("ÔN T") || textUpper.Contains("\u00D4N T\u1EACP"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[PhysicsMenuManager] 📝 Ôn tập chương → Hiện popup chọn chế độ");
                        ShowPopup();
                    });
                    continue;
                }

                // Nút "Bài X: ..." → hiện popup chọn chế độ
                if (text.StartsWith("Bài") || text.StartsWith("B\u00E0i"))
                {
                    string lessonName = text;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[PhysicsMenuManager] 📝 Chọn bài: {lessonName} → Hiện popup chọn chế độ");
                        SelectedLessonName = lessonName;
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
                        SceneLoader.Instance.LoadScene("Physics");
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
                        SceneLoader.Instance.LoadScene("Physics");
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
        /// Cập nhật tiêu đề chương trên scene LessonList
        /// </summary>
        private void UpdateChapterTitle()
        {
            if (string.IsNullOrEmpty(SelectedChapterName)) return;

            string chapterNumber = SelectedChapterName;
            if (chapterNumber.Contains(":"))
                chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf(":")).Trim().ToUpper();
            else if (chapterNumber.Contains("."))
                chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf(".")).Trim().ToUpper();

            // Tìm Text_Tittle hoặc text có chữ "CHƯƠNG"
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var tmp in allTexts)
            {
                string upper = tmp.text.Trim().ToUpper();
                if ((upper.StartsWith("CH\u01AF\u01A0NG") || upper.StartsWith("CHƯƠNG") || tmp.gameObject.name == "Text_Tittle") 
                    && !upper.Contains("ÔN T") && !upper.Contains("\u00D4N T\u1EACP"))
                {
                    tmp.text = chapterNumber;
                    Debug.Log($"[PhysicsMenuManager] 📋 Cập nhật tiêu đề chương: {chapterNumber}");
                }
                else if (upper.Contains("ÔN TẬP CHƯƠNG") || upper.Contains("\u00D4N T\u1EACP CH\u01AF\u01A0NG") || tmp.gameObject.name.ToLower().Contains("ontap"))
                {
                    tmp.text = "ÔN TẬP " + chapterNumber;
                    Debug.Log($"[PhysicsMenuManager] 📋 Cập nhật nút ôn tập: ÔN TẬP {chapterNumber}");
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
                SceneLoader.Instance.LoadScene("Physics");
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
                        SceneLoader.Instance.LoadScene("Physics");
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
                        SceneLoader.Instance.LoadScene("Physics");
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

