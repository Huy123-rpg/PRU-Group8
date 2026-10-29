using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using ScienceQuest.Biology;

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
                SetupChapterListScene();
            }
            else if (sceneName == "LessonList 1")
            {
                SetupLessonListScene();
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
                        if (LoginManager.IsCurrentTeacher)
                        {
                            Debug.Log("[BiologyMenuManager] ← Giảng viên quay về Trung tâm giảng viên");
                            TeacherNavigation.GoToTeacherCenter();
                        }
                        else
                        {
                            Debug.Log("[BiologyMenuManager] ← Quay về màn chọn môn từ ChapterList 1");
                            TeacherNavigation.GoToSubjectSelect();
                        }
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

            // Các bài do giảng viên tạo cho chủ đề hiện tại (nếu có)
            List<string> teacherLessons = GetTeacherLessonsForChapter(SelectedChapterName);

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

                // Gán đúng bài học môn Sinh học theo từng nút
                if (btnName == "btn_bai2" || btnName.EndsWith("bai2"))
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

            // Thêm các nút bài do giảng viên tạo (nếu có) vào cuối danh sách
            CreateTeacherLessonButtons(teacherLessons);
        }

        /// <summary>
        /// Lấy danh sách tiêu đề bài do giảng viên tạo cho chủ đề hiện tại.
        /// </summary>
        private List<string> GetTeacherLessonsForChapter(string chapter)
        {
            List<string> result = new List<string>();
            try
            {
                string selected = string.IsNullOrEmpty(chapter) ? SelectedChapterName : chapter;
                foreach (var lesson in TeacherStore.GetLessons())
                {
                    if (lesson == null) continue;
                    if (!ChapterTitleMatches(lesson.chapterId, selected)) continue;
                    result.Add(lesson.title);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BiologyMenuManager] Không đọc được danh sách bài của giảng viên: {e.Message}");
            }
            return result;
        }

        /// <summary>
        /// So khớp tiêu đề chủ đề giữa dữ liệu giảng viên và chủ đề đang chọn.
        /// </summary>
        private bool ChapterTitleMatches(string teacherChapter, string selectedChapter)
        {
            if (string.IsNullOrEmpty(teacherChapter)) return false;
            if (string.IsNullOrEmpty(selectedChapter)) return true;

            string a = NormalizeChapter(teacherChapter);
            string b = NormalizeChapter(selectedChapter);
            return a.Contains(b) || b.Contains(a);
        }

        private static string NormalizeChapter(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace(":", "").Replace(".", "").Replace(" ", "").Replace("-", "").ToLower();
        }

        /// <summary>
        /// Tạo nút cho từng bài của giảng viên, đặt dưới các bài có sẵn trong scene.
        /// </summary>
        private void CreateTeacherLessonButtons(List<string> teacherLessons)
        {
            if (teacherLessons == null || teacherLessons.Count == 0) return;

            // Tìm Content chứa các nút bài đang có để clone theo mẫu
            Transform content = FindLessonButtonsContainer();
            if (content == null)
            {
                Debug.LogWarning("[BiologyMenuManager] Không tìm thấy container chứa nút bài để thêm bài của giảng viên.");
                return;
            }

            // Lấy nút mẫu (btn_bai4) để clone hình dạng
            Transform template = content.Find("btn_bai4") ?? content.Find("btn_bai3") ?? content.Find("btn_bai2");

            // Tính vị trí bắt đầu: dưới nút cuối cùng hiện có
            float y = -178f - 152f; // btn_bai4 nằm ở y = -178
            foreach (Transform child in content)
            {
                RectTransform rt = child as RectTransform;
                if (rt == null) continue;
                float childBottom = rt.anchoredPosition.y - rt.sizeDelta.y * 0.5f;
                if (childBottom < y) y = childBottom;
            }
            y -= 12f;

            for (int i = 0; i < teacherLessons.Count; i++)
            {
                string lessonTitle = teacherLessons[i];

                GameObject btnObj = template != null
                    ? Instantiate(template.gameObject, content)
                    : CreateFallbackLessonButton(content);

                btnObj.name = $"btn_giaovien_{i + 1}";

                RectTransform btnRt = btnObj.GetComponent<RectTransform>();
                btnRt.anchoredPosition = new Vector2(0f, y);
                btnRt.sizeDelta = new Vector2(790.741f, 113.475f);

                // Gỡ sprite/icon mặc định của bài có sẵn nếu có
                Image img = btnObj.GetComponent<Image>();
                if (img != null && teacherLessons.Count > 0)
                {
                    img.color = Color.white;
                }

                // Đặt lại chữ trên nút (chỉ giữ 1 TMP chính, ẩn các TMP phụ của nút mẫu)
                TextMeshProUGUI[] tmps = btnObj.GetComponentsInChildren<TextMeshProUGUI>(true);
                bool first = true;
                foreach (var tmp in tmps)
                {
                    if (!first)
                    {
                        tmp.gameObject.SetActive(false);
                        continue;
                    }
                    first = false;

                    tmp.text = lessonTitle;
                    tmp.transform.localScale = Vector3.one;
                    tmp.enableAutoSizing = false;
                    tmp.fontSize = 34f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = new Color(0.12f, 0.22f, 0.16f, 1f);
                    tmp.alignment = TextAlignmentOptions.Center;

                    RectTransform rt = tmp.rectTransform;
                    rt.offsetMin = new Vector2(30f, 4f);
                    rt.offsetMax = new Vector2(-30f, -4f);
                }

                // Ẩn icon/hình phụ đi kèm của nút mẫu (tránh hiện icon của bài khác)
                foreach (var child in btnObj.GetComponentsInChildren<Transform>(true))
                {
                    if (child == btnObj.transform) continue;
                    if (child.GetComponent<TextMeshProUGUI>() != null) continue;
                    if (child.GetComponent<RectTransform>() == null) continue;

                    // Giữ lại Image nền của chính nút, ẩn các ảnh con (icon)
                    if (child.GetComponent<Image>() != null && child != btnObj.transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                Button btn = btnObj.GetComponent<Button>();
                if (btn == null) btn = btnObj.AddComponent<Button>();
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[BiologyMenuManager] 👨‍🏫 Click chọn bài của giảng viên: {lessonTitle}");
                    SelectedLessonName = lessonTitle;
                    PlayerPrefs.SetString("Sinh_SelectedLesson", lessonTitle);
                    PlayerPrefs.SetString("QuizLesson", lessonTitle);
                    PlayerPrefs.Save();

                    if (popupChonCheDo != null)
                    {
                        popupChonCheDo.SetActive(true);
                        popupChonCheDo.transform.SetAsLastSibling();
                    }
                    else
                    {
                        ShowDynamicFallbackPopup();
                    }
                });

                y -= 152f;
            }

            // Nới rộng vùng cuộn để các nút mới không bị chìm dưới nút cũ
            RectTransform contentRt = content as RectTransform;
            if (contentRt != null)
            {
                float needed = Mathf.Abs(y) + 40f;
                if (contentRt.sizeDelta.y < needed)
                {
                    contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, needed);
                }
            }

            Debug.Log($"[BiologyMenuManager] 👨‍🏫 Đã thêm {teacherLessons.Count} bài do giảng viên tạo vào danh sách.");
        }

        /// <summary>
        /// Tìm container chứa các nút bài (Content trong Scroll View).
        /// </summary>
        private Transform FindLessonButtonsContainer()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button btn in allButtons)
            {
                string name = btn.gameObject.name.ToLower();
                if (name == "btn_bai2" || name == "btn_bai3" || name == "btn_bai4")
                {
                    return btn.transform.parent;
                }
            }

            // Dự phòng: tìm "Content" trong scene
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var t in allTransforms)
            {
                if (t.gameObject.scene.name != SceneManager.GetActiveScene().name) continue;
                if (t.name == "Content") return t;
            }
            return null;
        }

        private GameObject CreateFallbackLessonButton(Transform parent)
        {
            GameObject btnObj = new GameObject("btn_giaovien");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(790.741f, 113.475f);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.94f, 0.90f, 0.78f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(30f, 4f);
            textRt.offsetMax = new Vector2(-30f, -4f);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 34f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.12f, 0.22f, 0.16f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            return btnObj;
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
                        SceneManager.LoadScene("ChapterList 1");
                    });
                    Debug.Log($"[BiologyMenuManager] ✅ Đã gắn sự kiện nút Back chính vào {btn.gameObject.name}");
                }
            }
        }

        #endregion

        #region ===== SINH SCENE =====

        private void SetupSinhScene()
        {
            // Spec mục 3: Loại bỏ hoàn toàn nút "← CHỌN BÀI" thừa tự sinh ở góc trên bên trái
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform existingBtn = canvas.transform.Find("Btn_BackToLessonList");
            if (existingBtn != null)
            {
                Destroy(existingBtn.gameObject);
                Debug.Log("[BiologyMenuManager] 🗑️ Đã xóa nút 'Btn_BackToLessonList' ('← CHỌN BÀI') theo spec.");
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
