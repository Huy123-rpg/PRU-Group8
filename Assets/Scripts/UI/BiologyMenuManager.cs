using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ScienceQuest.UI
{
    /// <summary>
    /// Quản lý menu chọn Chương và Bài học cho phân hệ Môn Sinh.
    /// Tự động tìm kiếm các nút trong scene và điều hướng:
    /// ChapterList 1 -> LessonList 1 -> Popup chọn chế độ -> SinhScene
    /// </summary>
    public class BiologyMenuManager : MonoBehaviour
    {
        public static BiologyMenuManager Instance { get; private set; }

        public static string SelectedChapterName = "";
        public static string SelectedLessonName = "";
        public static string SelectedMode = "TracNghiem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "ChapterList 1" || scene.name == "LessonList 1" || scene.name == "SinhScene")
            {
                if (FindFirstObjectByType<BiologyMenuManager>() == null)
                {
                    GameObject go = new GameObject("BiologyMenuManager_Auto");
                    go.AddComponent<BiologyMenuManager>();
                    Debug.Log($"[BiologyMenuManager] 🌿 Tự động khởi tạo cho scene {scene.name}");
                }
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "ChapterList 1")
            {
                StartCoroutine(SetupAfterProgressLoaded(SetupChapterListScene));
            }
            else if (sceneName == "LessonList 1")
            {
                StartCoroutine(SetupAfterProgressLoaded(SetupLessonListScene));
            }
            else if (sceneName == "SinhScene")
            {
                SetupSinhScene();
            }
        }

        #region ===== CHAPTER LIST 1 (MÔN SINH) =====

        private void SetupChapterListScene()
        {
            // 1. Cập nhật tiêu đề banner trên cùng thành: CHỦ ĐỀ SINH HỌC
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var tmp in allTexts)
            {
                string tName = tmp.gameObject.name;
                string tContent = tmp.text;
                if (tName == "Txt_Title" || tName == "Text_Tittle" || tContent.Contains("BÀI HỌC") || tContent.Contains("B\xC0I H\u1ECCC") || tContent.Contains("CHƯƠNG") || tContent.Contains("CHỦ ĐỀ"))
                {
                    tmp.transform.localScale = Vector3.one;
                    tmp.text = "CHỦ ĐỀ SINH HỌC";
                    tmp.enableAutoSizing = false;
                    tmp.fontSize = 48f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Center;
                }
            }

            // 2. Danh mục 5 Chủ đề Sinh học chuẩn chương trình KHTN 6, 7, 8, 9
            var biologyChapters = new (string key, string title, string subtitle)[]
            {
                ("1", "Chương I: Di truyền và biến dị", "KHTN 9 • ADN, Gen & Quy luật Menđen"),
                ("2", "Chương II: Sinh thái và môi trường", "KHTN 9 • Hệ sinh thái & Chuỗi thức ăn"),
                ("3", "Chương III: Cơ thể người và sức khỏe", "KHTN 8 • Tuần hoàn, Hô hấp & Dinh dưỡng"),
                ("4", "Chương IV: Trao đổi chất & năng lượng", "KHTN 7 • Quang hợp & Hô hấp tế bào"),
                ("5", "Chương V: Tế bào – Đơn vị sự sống", "KHTN 6 • Cấu tạo tế bào & Bào quan")
            };

            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (Button btn in allButtons)
            {
                btn.interactable = true;

                // Đồng bộ màu sắc nút
                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
                cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                cb.selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
                btn.colors = cb;

                string btnName = btn.gameObject.name;

                // Nút Back -> Quay về MainMenu
                if (btnName.ToLower().Contains("back"))
                {
                    TextMeshProUGUI backTmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (backTmp != null)
                    {
                        backTmp.text = "← Quay lại";
                        backTmp.fontStyle = FontStyles.Bold;
                        backTmp.color = new Color(0.24f, 0.14f, 0.08f, 1f); // Màu nâu gỗ sẫm
                        backTmp.enableAutoSizing = false;
                        backTmp.fontSize = 32f;
                        backTmp.alignment = TextAlignmentOptions.Center;
                    }

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[BiologyMenuManager] ← Quay về Menu từ ChapterList 1");
                        if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else { string returnScene = Application.CanStreamedLevelBeLoaded("SampleScene") ? "SampleScene" : "MainMenu"; SceneManager.LoadScene(returnScene); }
                    });
                    continue;
                }

                // Cấu hình các nút chương: Btn_Chuong1 -> Btn_Chuong5
                for (int i = 0; i < biologyChapters.Length; i++)
                {
                    var ch = biologyChapters[i];
                    if (btnName.EndsWith(ch.key) || btnName.Contains("Chuong" + ch.key) || btnName.Contains("chuong" + ch.key))
                    {
                        // Định dạng layout text cho nút chương (Nút lớn 900x153)
                        TextMeshProUGUI[] tmps = btn.GetComponentsInChildren<TextMeshProUGUI>();
                        if (tmps.Length >= 2)
                        {
                            TextMeshProUGUI titleTmp = tmps[0];
                            TextMeshProUGUI subTmp = tmps[1];

                            if (titleTmp.rectTransform.anchoredPosition.y < subTmp.rectTransform.anchoredPosition.y)
                            {
                                titleTmp = tmps[1];
                                subTmp = tmps[0];
                            }

                            // Định vị layout titleTmp nằm ở nửa trên và cách lề trái 55px
                            titleTmp.transform.localScale = Vector3.one;
                            titleTmp.rectTransform.anchorMin = new Vector2(0f, 0.42f);
                            titleTmp.rectTransform.anchorMax = new Vector2(1f, 0.95f);
                            titleTmp.rectTransform.offsetMin = new Vector2(55f, 0f);
                            titleTmp.rectTransform.offsetMax = new Vector2(-30f, 0f);
                            titleTmp.text = ch.title;
                            titleTmp.enableAutoSizing = false;
                            titleTmp.fontSize = 38f; // Tăng cỡ chữ to rõ tương xứng với nút 900x153
                            titleTmp.fontStyle = FontStyles.Bold;
                            titleTmp.color = Color.white;
                            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;

                            // Định vị layout subTmp nằm ở nửa dưới và cách lề trái 55px
                            subTmp.transform.localScale = Vector3.one;
                            subTmp.rectTransform.anchorMin = new Vector2(0f, 0.05f);
                            subTmp.rectTransform.anchorMax = new Vector2(1f, 0.45f);
                            subTmp.rectTransform.offsetMin = new Vector2(55f, 0f);
                            subTmp.rectTransform.offsetMax = new Vector2(-30f, 0f);
                            subTmp.text = ch.subtitle;
                            subTmp.enableAutoSizing = false;
                            subTmp.fontSize = 24f; // Tăng cỡ chữ phụ đề to dễ đọc
                            subTmp.fontStyle = FontStyles.Normal;
                            subTmp.color = new Color(1f, 0.92f, 0.65f, 1f); // Màu vàng kem sáng nổi bật trên nền gỗ
                            subTmp.alignment = TextAlignmentOptions.MidlineLeft;
                        }
                        else if (tmps.Length == 1)
                        {
                            tmps[0].transform.localScale = Vector3.one;
                            tmps[0].text = ch.title;
                            tmps[0].enableAutoSizing = false;
                            tmps[0].fontSize = 38f;
                            tmps[0].fontStyle = FontStyles.Bold;
                            tmps[0].color = Color.white;
                            tmps[0].alignment = TextAlignmentOptions.Center;
                        }

                        string targetChapter = ch.title;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => {
                            Debug.Log($"[BiologyMenuManager] 🌿 Chọn chương Sinh: {targetChapter} -> Mở LessonList 1");
                            SelectedChapterName = targetChapter;
                            PlayerPrefs.SetString("Sinh_SelectedChapter", targetChapter);
                            PlayerPrefs.SetString("SelectedChapter", targetChapter);
                            PlayerPrefs.Save();
                            SceneManager.LoadScene("LessonList 1");
                        });
                        break;
                    }
                }
            }

            // Tự động sinh nút Back nếu trong Scene không có
            bool hasBackButton = false;
            foreach (Button b in allButtons) { if (b.gameObject.name.ToLower().Contains("back")) { hasBackButton = true; break; } }
            if (!hasBackButton)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject btnObj = new GameObject("Btn_Back_Auto");
                    btnObj.transform.SetParent(canvas.transform, false);
                    btnObj.transform.SetAsLastSibling();
                    RectTransform rt = btnObj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = new Vector2(30f, -30f);
                    rt.sizeDelta = new Vector2(180f, 60f);
                    UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
                    img.color = new Color(0.9f, 0.8f, 0.6f, 1f);
                    Button btn = btnObj.AddComponent<Button>();
                    btn.onClick.AddListener(() => {
                        if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else { string returnScene = Application.CanStreamedLevelBeLoaded("SampleScene") ? "SampleScene" : "MainMenu"; SceneManager.LoadScene(returnScene); }
                    });
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(btnObj.transform, false);
                    RectTransform textRt = textObj.AddComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
                    textRt.sizeDelta = Vector2.zero;
                    TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
                    txt.text = "← Quay lại"; txt.fontSize = 28f; txt.fontStyle = FontStyles.Bold; txt.color = new Color(0.24f, 0.14f, 0.08f, 1f);
                    txt.alignment = TextAlignmentOptions.Center;
                }
            }

            Debug.Log("[BiologyMenuManager] ✅ Đã cấu hình xong toàn bộ Chủ đề Sinh học trong ChapterList 1");
        }

        #endregion

        #region ===== LESSON LIST 1 (MÔN SINH) =====

        private GameObject popupChonCheDo;

        private void SetupLessonListScene()
        {
            if (string.IsNullOrEmpty(SelectedChapterName))
            {
                SelectedChapterName = PlayerPrefs.GetString("Sinh_SelectedChapter", "Chương I: Di truyền và biến dị");
            }

            // 1. Cập nhật tiêu đề chương
            UpdateChapterTitle();

            // 2. Tìm popup Image_chon
            FindPopup();

            // 3. Cấu hình các nút bên trong popup (Trắc nghiệm / Tự luận / Back)
            AssignPopupButtons();

            // 4. Cấu hình các nút bài học Sinh học (btn_bai2, btn_bai3, btn_bai4, btn_ontapchuong, ...)
            SetupLessonButtons();

            // 5. Cấu hình nút Back chính trên scene LessonList 1 -> quay lại ChapterList 1
            SetupBackButton();

            Debug.Log("[BiologyMenuManager] ✅ Đã cấu hình xong scene LessonList 1");
        }

        private void UpdateChapterTitle()
        {
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var tmp in allTexts)
            {
                if (tmp.gameObject.name == "Text_Tittle" || tmp.gameObject.name == "Txt_Title")
                {
                    // Reset biến dạng scale 1.5x để chữ không bị kéo méo và tràn ra ngoài biển gỗ
                    tmp.transform.localScale = Vector3.one;

                    RectTransform rt = tmp.rectTransform;
                    rt.sizeDelta = new Vector2(750f, 90f);
                    rt.anchoredPosition = new Vector2(0f, 0f);

                    tmp.text = SelectedChapterName;
                    tmp.enableAutoSizing = false;
                    tmp.fontSize = 36f; // Cỡ chữ lớn 36f rõ nét trên biển gỗ 1920x1080
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = Color.white;
                    tmp.alignment = TextAlignmentOptions.Center;

                    Debug.Log($"[BiologyMenuManager] 📋 Cập nhật tiêu đề chương Sinh học: {SelectedChapterName}");
                }
            }
        }

        private void FindPopup()
        {
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            
            // 1. Ưu tiên tìm đối tượng gốc Popup_ChonCheDo (chứa cả background gỗ Image_bg và panel Image_chon)
            foreach (var t in allTransforms)
            {
                if (t.gameObject.scene.name != SceneManager.GetActiveScene().name) continue;

                if (t.name == "Popup_ChonCheDo")
                {
                    popupChonCheDo = t.gameObject;
                    break;
                }
            }

            // 2. Nếu không tìm thấy tên chính xác, tìm theo tên gần đúng
            if (popupChonCheDo == null)
            {
                foreach (var t in allTransforms)
                {
                    if (t.gameObject.scene.name != SceneManager.GetActiveScene().name) continue;

                    string lower = t.name.ToLower();
                    if (lower.Contains("popup_chon") || lower.Contains("popupchon"))
                    {
                        popupChonCheDo = t.gameObject;
                        break;
                    }
                }
            }

            // 3. Nếu chỉ tìm thấy Image_chon, lấy GameObject cha của nó (chính là Popup_ChonCheDo)
            if (popupChonCheDo == null)
            {
                foreach (var t in allTransforms)
                {
                    if (t.gameObject.scene.name != SceneManager.GetActiveScene().name) continue;

                    if (t.name == "Image_chon")
                    {
                        if (t.parent != null && (t.parent.name.Contains("Popup") || t.parent.name.Contains("popup")))
                            popupChonCheDo = t.parent.gameObject;
                        else
                            popupChonCheDo = t.gameObject;
                        break;
                    }
                }
            }

            if (popupChonCheDo != null)
            {
                Debug.Log($"[BiologyMenuManager] ✅ Đã tìm thấy root popup chọn chế độ: {popupChonCheDo.name}");

                // Đảm bảo tất cả các con (Image_bg, Image_chon, Image_tracnghiem, Image_tuluan, Image_back, Text...) được BẬT SẴN bên trong
                Transform[] allChildren = popupChonCheDo.GetComponentsInChildren<Transform>(true);
                foreach (var c in allChildren)
                {
                    c.gameObject.SetActive(true);
                }

                // ẨN TOÀN BỘ ROOT POPUP KHI BẮT ĐẦU SCENE (Tránh hiện khung gỗ rỗng che màn hình)
                popupChonCheDo.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[BiologyMenuManager] ⚠️ Không tìm thấy popup chọn chế độ trong Scene!");
            }
        }

        private void AssignPopupButtons()
        {
            if (popupChonCheDo == null) return;

            Transform[] children = popupChonCheDo.GetComponentsInChildren<Transform>(true);
            bool foundTracNghiem = false;
            bool foundTuLuan = false;

            Transform targetContainer = popupChonCheDo.transform;

            foreach (var child in children)
            {
                string name = child.name;
                string lower = name.ToLower();

                if (name == "Image_chon")
                {
                    targetContainer = child;
                }

                // Nút Trắc Nghiệm (Image_tracnghiem hoặc chứa chữ tracnghiem)
                if (name == "Image_tracnghiem" || lower.Contains("tracnghiem"))
                {
                    foundTracNghiem = true;
                    child.gameObject.SetActive(true);
                    Button btn = EnsureButtonComponent(child.gameObject);
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[BiologyMenuManager] 🎯 Chọn hình thức: TRẮC NGHIỆM -> Vào SinhScene");
                        SelectedMode = "TracNghiem";
                        PlayerPrefs.SetString("Sinh_QuizMode", "TracNghiem");
                        PlayerPrefs.SetString("QuizMode", "TracNghiem");
                        PlayerPrefs.Save();
                        SceneManager.LoadScene("SinhScene");
                    });

                    FormatPopupButtonAndText(child, "TRẮC NGHIỆM", 24f, new Vector2(270f, 62f));
                    Debug.Log($"[BiologyMenuManager] ✅ Đã căn chỉnh nút Trắc Nghiệm vào {name}");
                }
                // Nút Tự Luận (Image_tuluan hoặc chứa chữ tuluan)
                else if (name == "Image_tuluan" || lower.Contains("tuluan"))
                {
                    foundTuLuan = true;
                    child.gameObject.SetActive(true);
                    Button btn = EnsureButtonComponent(child.gameObject);
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[BiologyMenuManager] ✍️ Chọn hình thức: TỰ LUẬN -> Vào SinhScene");
                        SelectedMode = "TuLuan";
                        PlayerPrefs.SetString("Sinh_QuizMode", "TuLuan");
                        PlayerPrefs.SetString("QuizMode", "TuLuan");
                        PlayerPrefs.Save();
                        SceneManager.LoadScene("SinhScene");
                    });

                    FormatPopupButtonAndText(child, "TỰ LUẬN", 24f, new Vector2(270f, 62f));
                    Debug.Log($"[BiologyMenuManager] ✅ Đã căn chỉnh nút Tự Luận vào {name}");
                }
                // Nút Back trong popup (Image_back con của popupChonCheDo)
                else if ((name == "Image_back" || lower.Contains("back") || lower.Contains("dong") || lower.Contains("close")) && child != popupChonCheDo.transform)
                {
                    child.gameObject.SetActive(true);
                    Button btn = EnsureButtonComponent(child.gameObject);
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[BiologyMenuManager] ← Đóng popup chọn chế độ");
                        popupChonCheDo.SetActive(false);
                    });

                    FormatPopupButtonAndText(child, "QUAY LẠI", 22f, new Vector2(220f, 54f));
                    Debug.Log($"[BiologyMenuManager] ✅ Đã căn chỉnh nút Đóng popup vào {name}");
                }
            }

            // Dự phòng: Nếu thiếu nút nào trong prefab/scene thì tự động tạo thêm
            if (!foundTracNghiem)
            {
                CreateFallbackModeButton(targetContainer, "Btn_TracNghiem_Auto", "TRẮC NGHIỆM", new Vector2(0f, 30f), () => {
                    SelectedMode = "TracNghiem";
                    PlayerPrefs.SetString("Sinh_QuizMode", "TracNghiem");
                    PlayerPrefs.SetString("QuizMode", "TracNghiem");
                    PlayerPrefs.Save();
                    SceneManager.LoadScene("SinhScene");
                });
            }

            if (!foundTuLuan)
            {
                CreateFallbackModeButton(targetContainer, "Btn_TuLuan_Auto", "TỰ LUẬN", new Vector2(0f, -40f), () => {
                    SelectedMode = "TuLuan";
                    PlayerPrefs.SetString("Sinh_QuizMode", "TuLuan");
                    PlayerPrefs.SetString("QuizMode", "TuLuan");
                    PlayerPrefs.Save();
                    SceneManager.LoadScene("SinhScene");
                });
            }
        }

        private void FormatPopupButtonAndText(Transform buttonTransform, string text, float fontSize, Vector2 targetSize)
        {
            // 1. Chuẩn hóa kích thước nút bấm để cân đối trên bảng gỗ
            RectTransform btnRt = buttonTransform.GetComponent<RectTransform>();
            if (btnRt != null)
            {
                btnRt.sizeDelta = targetSize;
            }

            // 2. Căn chỉnh TextMeshPro con nằm chính giữa tâm nút bấm
            TextMeshProUGUI tmp = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.gameObject.SetActive(true);
                tmp.text = text;
                tmp.color = new Color(0.24f, 0.12f, 0.05f, 1f); // Nâu gỗ sẫm sang trọng
                tmp.fontStyle = FontStyles.Bold;
                tmp.enableAutoSizing = false;
                tmp.textWrappingMode = TextWrappingModes.NoWrap; // Tuyệt đối không rớt dòng (giữ nguyên 1 dòng cân đối)
                tmp.fontSize = fontSize;
                tmp.alignment = TextAlignmentOptions.Center; // Căn chính giữa cả ngang lẫn dọc
                tmp.raycastTarget = false;

                // Reset toàn bộ transform của Text để triệt tiêu độ lệch cũ (y = -10)
                RectTransform textRt = tmp.rectTransform;
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.pivot = new Vector2(0.5f, 0.5f);
                textRt.anchoredPosition = Vector2.zero; // X = 0, Y = 0 (Chính tâm nút!)
                textRt.offsetMin = new Vector2(10f, 0f);
                textRt.offsetMax = new Vector2(-10f, 0f);
                textRt.localScale = Vector3.one;
            }
        }

        private void CreateFallbackModeButton(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(240f, 55f);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.18f, 0.58f, 0.32f, 1f); // Xanh lá cây sinh học
            img.raycastTarget = true;

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
            tmp.raycastTarget = false;

            Debug.Log($"[BiologyMenuManager] 🛠️ Đã tạo nút dự phòng: {label}");
        }

        private (string b2, string b3, string b4, string ontap) GetLessonsForChapter(string chapter)
        {
            string c = chapter.ToLower();
            if (c.Contains("di truyền") || c.Contains("chuong i") || c.Contains("chương i") || c.Contains("chuong1"))
            {
                return (
                    "Bài 1: Menđen và Di truyền học",
                    "Bài 2: Nhiễm sắc thể và ADN",
                    "Bài 3: Đột biến gen và NST",
                    "ÔN TẬP CHƯƠNG I: DI TRUYỀN"
                );
            }
            else if (c.Contains("sinh thái") || c.Contains("chuong ii") || c.Contains("chương ii") || c.Contains("chuong2"))
            {
                return (
                    "Bài 1: Môi trường & Nhân tố sinh thái",
                    "Bài 2: Quần thể & Quần xã sinh vật",
                    "Bài 3: Hệ sinh thái & Chuỗi thức ăn",
                    "ÔN TẬP CHƯƠNG II: SINH THÁI"
                );
            }
            else if (c.Contains("cơ thể người") || c.Contains("chuong iii") || c.Contains("chương iii") || c.Contains("chuong3"))
            {
                return (
                    "Bài 1: Hệ vận động và Cơ quan",
                    "Bài 2: Hệ tuần hoàn và Máu",
                    "Bài 3: Hệ tiêu hóa và Dinh dưỡng",
                    "ÔN TẬP CHƯƠNG III: CƠ THỂ NGƯỜI"
                );
            }
            else if (c.Contains("trao đổi chất") || c.Contains("chuong iv") || c.Contains("chương iv") || c.Contains("chuong4"))
            {
                return (
                    "Bài 1: Quang hợp ở thực vật",
                    "Bài 2: Hô hấp tế bào",
                    "Bài 3: Trao đổi nước & Dinh dưỡng",
                    "ÔN TẬP CHƯƠNG IV: TRAO ĐỔI CHẤT"
                );
            }
            else // Chương V hoặc Tế bào
            {
                return (
                    "Bài 1: Khái quát về Tế bào",
                    "Bài 2: Tế bào nhân sơ & nhân thực",
                    "Bài 3: Cấu tạo & Chức năng bào quan",
                    "ÔN TẬP CHƯƠNG V: TẾ BÀO"
                );
            }
        }

        private void SetupLessonButtons()
        {
            var lessons = GetLessonsForChapter(SelectedChapterName);

            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button btn in allButtons)
            {
                // Bỏ qua các nút bên trong popup
                if (popupChonCheDo != null && btn.transform.IsChildOf(popupChonCheDo.transform))
                    continue;

                string btnName = btn.gameObject.name.ToLower();

                // Bỏ qua nút Back chính
                if (btnName.Contains("back"))
                    continue;

                string assignedLesson = "";

                // Gán đúng bài học môn Sinh học theo từng nút (thêm btn_bai1)
                if (btnName == "btn_bai1" || btnName.EndsWith("bai1"))
                {
                    assignedLesson = lessons.b2; // b2 = bài đầu của tuple vì tuple không có b1
                    // Thực ra GetLessonsForChapter trả (b2,b3,b4,ontap) - bai1 là bài đầu tiên trong chapter
                    // Gán tạm thành tên "Bài 1" theo chapter hiện tại
                    string chLower = SelectedChapterName.ToLower();
                    if (chLower.Contains("di truyền") || chLower.Contains("chuong i") || chLower.Contains("chương i"))
                        assignedLesson = "Bài 1: Menđen và Di truyền học";
                    else if (chLower.Contains("sinh thái") || chLower.Contains("chuong ii") || chLower.Contains("chương ii"))
                        assignedLesson = "Bài 1: Môi trường & Nhân tố sinh thái";
                    else if (chLower.Contains("cơ thể") || chLower.Contains("chuong iii") || chLower.Contains("chương iii"))
                        assignedLesson = "Bài 1: Hệ vận động và Cơ quan";
                    else if (chLower.Contains("trao đổi") || chLower.Contains("chuong iv") || chLower.Contains("chương iv"))
                        assignedLesson = "Bài 1: Quang hợp ở thực vật";
                    else
                        assignedLesson = "Bài 1: Khái quát về Tế bào";
                }
                else if (btnName == "btn_bai2" || btnName.EndsWith("bai2"))
                {
                    assignedLesson = lessons.b2;
                }
                else if (btnName == "btn_bai3" || btnName.EndsWith("bai3"))
                {
                    assignedLesson = lessons.b3;
                }
                else if (btnName == "btn_bai4" || btnName.EndsWith("bai4"))
                {
                    assignedLesson = lessons.b4;
                }
                else if (btnName.Contains("ontap") || btnName.Contains("review"))
                {
                    assignedLesson = lessons.ontap;
                }

                if (!string.IsNullOrEmpty(assignedLesson))
                {
                    TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        // Reset scale biến dạng và định dạng màu chữ chuẩn cho môn Sinh
                        tmp.transform.localScale = Vector3.one;
                        tmp.text = assignedLesson;
                        string username = GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.username) ? GameSession.Instance.username : "HS001";
                        string baiNum = "1";
                        if (btnName.Contains("bai2")) baiNum = "2";
                        else if (btnName.Contains("bai3")) baiNum = "3";
                        else if (btnName.Contains("bai4")) baiNum = "4";

                        // Fix: tính chapter index thực để baiID đúng (B1_001, B2_001...)
                        int chapIdx = 1;
                        string chName = SelectedChapterName.ToLower();
                        if (chName.Contains("chuong ii") || chName.Contains("chương ii") || chName.Contains("sinh thái")) chapIdx = 2;
                        else if (chName.Contains("chuong iii") || chName.Contains("chương iii") || chName.Contains("cơ thể")) chapIdx = 3;
                        else if (chName.Contains("chuong iv") || chName.Contains("chương iv") || chName.Contains("trao đổi")) chapIdx = 4;
                        else if (chName.Contains("chuong v") || chName.Contains("chương v") || chName.Contains("tế bào")) chapIdx = 5;

                        string baiID = $"B{chapIdx}_00{baiNum}";
                        string monID = "KHTN6";
                        int attempts = 0, highScore = 0;
                        if (ProgressSyncManager.Instance != null) {
                            var p = ProgressSyncManager.Instance.GetProgress(username, monID, baiID);
                            if (p != null) { attempts = p.soLanLam; highScore = p.diemCaoNhat; }
                        }
                        if (attempts > 0 || highScore > 0) tmp.text += $"\n<size=20><color=#FFD700>Số lượt làm: {attempts} | Điểm cao nhất: {highScore}</color></size>";
                        tmp.enableAutoSizing = false;
                        tmp.fontSize = 34f; // Tăng cỡ chữ to rõ trên màn hình 1920x1080
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.color = new Color(0.12f, 0.22f, 0.16f, 1f); // Màu xanh sẫm sinh học đậm nét trên nền thẻ sáng
                        tmp.alignment = TextAlignmentOptions.Center;

                        RectTransform rt = tmp.rectTransform;
                        rt.offsetMin = new Vector2(30f, 4f);
                        rt.offsetMax = new Vector2(-30f, -4f);
                    }

                    string lessonName = assignedLesson;
                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[BiologyMenuManager] 📖 Click chọn bài Sinh học: {lessonName} -> Mở popup chọn chế độ");
                        SelectedLessonName = lessonName;
                        PlayerPrefs.SetString("Sinh_SelectedLesson", lessonName);
                        PlayerPrefs.SetString("QuizLesson", lessonName);
                        PlayerPrefs.SetString("Sinh_QuizLesson", lessonName);
                        PlayerPrefs.Save();

                        if (popupChonCheDo != null)
                        {
                            popupChonCheDo.SetActive(true);
                            popupChonCheDo.transform.SetAsLastSibling(); // Đưa popup lên trên cùng hiển thị trước mặt người chơi
                        }
                        else
                        {
                            Debug.LogWarning("[BiologyMenuManager] ⚠️ Không có popup trong scene, tự động hiển thị chọn chế độ");
                            ShowDynamicFallbackPopup();
                        }
                    });
                }
            }
        }

        // Doi ProgressSyncManager tai xong data (toi da 5 giay) roi moi render nut
        private System.Collections.IEnumerator SetupAfterProgressLoaded(System.Action callback)
        {
            if (ProgressSyncManager.Instance != null && ProgressSyncManager.Instance.progressData.Count == 0)
            {
                bool done = false;
                StartCoroutine(ProgressSyncManager.Instance.FetchProgressFromSheet((_) => { done = true; }));
                float timeout = 5f;
                while (!done && timeout > 0f)
                {
                    timeout -= Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            callback?.Invoke();
        }

        private void ShowDynamicFallbackPopup()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                SceneManager.LoadScene("SinhScene");
                return;
            }

            GameObject popup = new GameObject("Popup_ChonCheDo_Fallback");
            popup.transform.SetParent(canvas.transform, false);
            popup.transform.SetAsLastSibling();

            RectTransform rt = popup.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image darkBg = popup.AddComponent<Image>();
            darkBg.color = new Color(0f, 0f, 0f, 0.7f);

            GameObject board = new GameObject("Board");
            board.transform.SetParent(popup.transform, false);
            RectTransform boardRt = board.AddComponent<RectTransform>();
            boardRt.sizeDelta = new Vector2(400f, 320f);
            Image boardImg = board.AddComponent<Image>();
            boardImg.color = new Color(0.22f, 0.16f, 0.12f, 0.95f); // Màu gỗ

            CreateFallbackModeButton(board.transform, "Btn_TracNghiem", "TRẮC NGHIỆM", new Vector2(0f, 40f), () => {
                SelectedMode = "TracNghiem";
                PlayerPrefs.SetString("Sinh_QuizMode", "TracNghiem");
                PlayerPrefs.SetString("QuizMode", "TracNghiem");
                PlayerPrefs.Save();
                SceneManager.LoadScene("SinhScene");
            });

            CreateFallbackModeButton(board.transform, "Btn_TuLuan", "TỰ LUẬN", new Vector2(0f, -30f), () => {
                SelectedMode = "TuLuan";
                PlayerPrefs.SetString("Sinh_QuizMode", "TuLuan");
                PlayerPrefs.SetString("QuizMode", "TuLuan");
                PlayerPrefs.Save();
                SceneManager.LoadScene("SinhScene");
            });

            CreateFallbackModeButton(board.transform, "Btn_Close", "QUAY LẠI", new Vector2(0f, -100f), () => {
                Destroy(popup);
            });
        }

        private void SetupBackButton()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            bool hasBack = false;
            foreach (Button btn in allButtons)
            {
                // Bỏ qua nút thuộc popup
                if (popupChonCheDo != null && btn.transform.IsChildOf(popupChonCheDo.transform))
                    continue;

                string btnName = btn.gameObject.name.ToLower();
                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                string text = tmp != null ? tmp.text.ToLower() : "";

                if (btnName == "btn_back" || btnName == "image_back" || text.Contains("quay lại") || text.Contains("back"))
                {
                    if (tmp != null)
                    {
                        tmp.enableAutoSizing = false;
                        tmp.fontSize = 32f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.color = new Color(0.24f, 0.14f, 0.08f, 1f);
                    }

                    btn.interactable = true;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("[BiologyMenuManager] ← Quay về ChapterList 1");
                        if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene("ChapterList 1");
                    });
                    Debug.Log($"[BiologyMenuManager] ✅ Đã gắn sự kiện nút Back chính vào {btn.gameObject.name}");
                    hasBack = true;
                }
            }

            if (!hasBack)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject btnObj = new GameObject("Btn_Back_Auto");
                    btnObj.transform.SetParent(canvas.transform, false);
                    btnObj.transform.SetAsLastSibling();
                    RectTransform rt = btnObj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = new Vector2(30f, -30f);
                    rt.sizeDelta = new Vector2(180f, 60f);
                    UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
                    img.color = new Color(0.9f, 0.8f, 0.6f, 1f);
                    Button btn = btnObj.AddComponent<Button>();
                    btn.onClick.AddListener(() => {
                        if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene("ChapterList 1");
                    });
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(btnObj.transform, false);
                    RectTransform textRt = textObj.AddComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
                    textRt.sizeDelta = Vector2.zero;
                    TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
                    txt.text = "← Quay lại"; txt.fontSize = 28f; txt.fontStyle = FontStyles.Bold; txt.color = new Color(0.24f, 0.14f, 0.08f, 1f);
                    txt.alignment = TextAlignmentOptions.Center;
                }
            }
        }

        #endregion

        #region ===== SINH SCENE =====

        private void SetupSinhScene()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Kiểm tra xem đã có nút Back chưa, nếu chưa có thì tạo 1 nút nhỏ ở góc trái để quay về LessonList 1
            Transform existingBtn = canvas.transform.Find("Btn_BackToLessonList");
            if (existingBtn == null)
            {
                GameObject btnObj = new GameObject("Btn_BackToLessonList");
                btnObj.transform.SetParent(canvas.transform, false);

                RectTransform rt = btnObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(20f, -20f);
                rt.sizeDelta = new Vector2(140f, 45f);

                Image img = btnObj.AddComponent<Image>();
                img.color = new Color(0.15f, 0.55f, 0.35f, 0.9f); // Xanh lá cây sinh học

                Button btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(() => {
                    Debug.Log("[BiologyMenuManager] ← Từ SinhScene quay lại LessonList 1");
                    Time.timeScale = 1f;
                    if (NavigationManager.Instance != null) NavigationManager.Instance.GoBack(); else SceneManager.LoadScene("LessonList 1");
                });

                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "← CHỌN BÀI";
                tmp.fontSize = 18f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;

                Debug.Log("[BiologyMenuManager] ✅ Đã tạo nút '← CHỌN BÀI' trên SinhScene");
            }
        }

        #endregion

        #region ===== HELPER METHODS =====

        private Button EnsureButtonComponent(GameObject go)
        {
            Button btn = go.GetComponent<Button>();
            if (btn == null)
            {
                btn = go.AddComponent<Button>();
            }

            Image img = go.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                btn.targetGraphic = img;
            }

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            cb.selectedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            btn.colors = cb;

            return btn;
        }

        #endregion
    }
}
