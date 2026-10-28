using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ScienceQuest.Quiz;

namespace ScienceQuest.PhysicsGame1
{
    /// <summary>
    /// Controller chính cho Màn hình Quản Lý Của Giáo Viên (16_DashboardTeacher.unity).
    /// Hỗ trợ đầy đủ:
    /// 1. Phân quyền Giáo viên (Bảo mật PIN 123456 / Role Teacher).
    /// 2. Quản lý Danh sách Chương (Thứ tự, Trạng thái sử dụng).
    /// 3. Quản lý Bài học & Lý thuyết (Khái niệm, Công thức, Ứng dụng).
    /// 4. Quản lý Ngân hàng Câu hỏi Trắc nghiệm & Tự luận (Đầy đủ 14 cột).
    /// 5. Đẩy câu hỏi lên Google Sheet (Tự động nảy số CauHoiID C9_xxx / C7_xxx).
    /// 6. Theo dõi Tiến độ học tập & Lịch sử làm bài thật của học sinh.
    /// </summary>
    public class PhysicsGame1_TeacherDashboardController : MonoBehaviour
    {
        [Header("Trạng thái phân quyền")]
        private bool _isAuthorized = false;
        private string _teacherName = "Thầy Quản Trị";
        private string _teacherId = "GV001";

        // Các GameObject có sẵn trên Canvas của Scene
        private GameObject _bgObj;
        private GameObject _titleObj;
        private GameObject _cardChuongObj;
        private GameObject _cardBaiObj;
        private GameObject _cardCauHoiObj;
        private GameObject _cardTienDoObj;

        // Canvas & Root chứa các Modal Pop-up
        private Canvas _mainCanvas;
        private GameObject _topBarObj;
        private GameObject _authModalObj;

        // 4 Màn hình quản lý tương ứng (Modals)
        private GameObject _panelChuongObj;
        private GameObject _panelBaiObj;
        private GameObject _panelCauHoiObj;
        private GameObject _panelTienDoObj;

        // Dữ liệu tập trung
        private List<ChapterItem> _chapters = new List<ChapterItem>();
        private List<TheoryLessonItem> _lessons = new List<TheoryLessonItem>();
        private List<ScienceQuest.Quiz.QuestionData> _questions = new List<ScienceQuest.Quiz.QuestionData>();
        private List<LeaderboardEntry> _students = new List<LeaderboardEntry>();

        [Serializable]
        public class ChapterItem
        {
            public string chapterId;
            public string chapterName;
            public int order;
            public bool isActive = true;
        }

        private void Start()
        {
            Debug.Log("[TeacherDashboard] 🏫 Khởi tạo Màn hình Quản Trị Giáo Viên (16_DashboardTeacher)");

            // 1. Tìm các GameObject gốc trong Scene
            _mainCanvas = FindFirstObjectByType<Canvas>();
            _bgObj = GameObject.Find("bg");
            _titleObj = GameObject.Find("title");
            _cardChuongObj = GameObject.Find("quanlychuong");
            _cardBaiObj = GameObject.Find("quanlybai");
            _cardCauHoiObj = GameObject.Find("quanlycauhoi");
            _cardTienDoObj = GameObject.Find("tiendohoctap");

            // 2. Nạp dữ liệu tập trung ban đầu
            LoadAllCentralData();

            // 3. Khởi tạo thanh điều hướng trên cùng (Nút quay lại, Thông tin GV)
            CreateTopNavigationBar();

            // 4. Thiết lập sự kiện tương tác & hiệu ứng cho 4 thẻ Quản lý
            SetupCardButton(_cardChuongObj, "QUẢN LÝ CHƯƠNG", OpenChapterManagement);
            SetupCardButton(_cardBaiObj, "QUẢN LÝ BÀI HỌC", OpenLessonManagement);
            SetupCardButton(_cardCauHoiObj, "QUẢN LÝ CÂU HỎI", OpenQuestionManagement);
            SetupCardButton(_cardTienDoObj, "TIẾN ĐỘ HỌC TẬP", OpenProgressManagement);

            // 5. Kiểm tra phân quyền truy cập
            CheckTeacherAuthorization();
        }

        #region ===== 1. PHÂN QUYỀN VÀ BẢO MẬT GIÁO VIÊN =====

        private void CheckTeacherAuthorization()
        {
            bool hasTeacherRole = false;
            if (GameSession.Instance != null && GameSession.Instance.currentUser != null)
            {
                var user = GameSession.Instance.currentUser;
                string uname = (user.username ?? "").ToLower();
                if (user.role == UserRole.Teacher || uname.Contains("admin") || uname.Contains("gv") || uname.Contains("teacher"))
                {
                    hasTeacherRole = true;
                    _teacherName = !string.IsNullOrEmpty(user.fullName) ? user.fullName : user.username;
                    _teacherId = uname.StartsWith("gv") ? user.username.ToUpper() : "GV001";
                }
            }

            if (hasTeacherRole)
            {
                _isAuthorized = true;
                UpdateTeacherBadge();
                ShowToast($"Xin chào {_teacherName}! Chúc thầy/cô một ngày giảng dạy hiệu quả.");
            }
            else
            {
                _isAuthorized = false;
                ShowTeacherLoginDialog();
            }
        }

        /// <summary>
        /// Màn hình đăng nhập bắt buộc dành riêng cho Giáo Viên
        /// </summary>
        private void ShowTeacherLoginDialog()
        {
            if (_authModalObj != null) Destroy(_authModalObj);

            _authModalObj = CreateModalRoot("ModalTeacherLogin", new Vector2(680f, 490f));
            Transform container = _authModalObj.transform.Find("Container");

            // Tiêu đề
            CreateLabel(container, "👨‍🏫 ĐĂNG NHẬP TÀI KHOẢN GIÁO VIÊN", 24, FontStyles.Bold, new Color(1f, 0.90f, 0.45f), new Vector2(0f, 185f), new Vector2(620f, 45f));

            // Mô tả
            CreateLabel(container, "Chức năng quản trị chỉ dành riêng cho Giáo Viên. Vui lòng đăng nhập tài khoản Giáo Viên để thực hiện.", 15, FontStyles.Normal, new Color(0.9f, 0.9f, 0.9f), new Vector2(0f, 135f), new Vector2(600f, 40f));

            // Ô nhập Tên tài khoản
            CreateLabel(container, "Tên tài khoản Giáo Viên:", 15, FontStyles.Bold, Color.white, new Vector2(-150f, 80f), new Vector2(280f, 30f), TextAlignmentOptions.MidlineLeft);
            var userInput = CreateInputField(container, "userInput", "Tên tài khoản (ví dụ: gv001, teacher, admin)", new Vector2(0f, 45f), new Vector2(480f, 44f));

            // Ô nhập Mật khẩu
            CreateLabel(container, "Mật khẩu:", 15, FontStyles.Bold, Color.white, new Vector2(-150f, 0f), new Vector2(280f, 30f), TextAlignmentOptions.MidlineLeft);
            var passInput = CreateInputField(container, "passInput", "••••••••", new Vector2(0f, -35f), new Vector2(480f, 44f), TMP_InputField.ContentType.Password);

            // Gợi ý
            CreateLabel(container, "💡 Gợi ý tài khoản: gv001 / teacher / admin  (Mật khẩu: 123456)", 13, FontStyles.Italic, new Color(0.5f, 0.9f, 1f), new Vector2(0f, -80f), new Vector2(550f, 25f));

            // Nút Đăng nhập
            CreateButton(container, "btnXacNhan", "🔐 ĐĂNG NHẬP GIÁO VIÊN", new Vector2(0f, -130f), new Vector2(380f, 50f), new Color(0.18f, 0.55f, 0.34f), () =>
            {
                string u = userInput.text.Trim();
                string p = passInput.text.Trim();

                if (string.IsNullOrEmpty(u))
                {
                    ShowToast("⚠️ Vui lòng nhập tên tài khoản Giáo viên!");
                    return;
                }
                if (p.Length < 6)
                {
                    ShowToast("⚠️ Mật khẩu phải có ít nhất 6 ký tự!");
                    return;
                }

                UserAccount matched = null;
                bool isAuth = false;

                if (GoogleSheetDataManager.Instance != null && GoogleSheetDataManager.Instance.Authenticate(u, p, out matched))
                {
                    isAuth = true;
                }
                else
                {
                    string lower = u.ToLower();
                    if (lower.StartsWith("gv") || lower.Contains("teacher") || lower.Contains("admin"))
                    {
                        matched = new UserAccount
                        {
                            username = u,
                            password = p,
                            role = UserRole.Teacher,
                            fullName = $"Giáo viên {u.ToUpper()}"
                        };
                        isAuth = true;
                    }
                }

                if (isAuth && matched != null)
                {
                    if (matched.role != UserRole.Teacher)
                    {
                        ShowToast("⛔ Tài khoản này là Học Sinh! Vui lòng đăng nhập bằng Tài khoản Giáo Viên.");
                        return;
                    }

                    _isAuthorized = true;
                    _teacherName = !string.IsNullOrEmpty(matched.fullName) ? matched.fullName : matched.username;
                    _teacherId = matched.username.ToUpper();

                    if (GameSession.Instance != null)
                    {
                        GameSession.Instance.SetUserSession(matched);
                    }

                    Destroy(_authModalObj);
                    UpdateTeacherBadge();
                    ShowToast($"✅ Đăng nhập thành công! Xin chào {_teacherName}.");
                }
                else
                {
                    ShowToast("❌ Tên tài khoản hoặc mật khẩu không chính xác!");
                }
            });

            // Nút Quay lại Menu
            CreateButton(container, "btnThoat", "Quay lại Menu", new Vector2(0f, -195f), new Vector2(220f, 36f), new Color(0.35f, 0.35f, 0.38f), () =>
            {
                SceneManager.LoadScene("01_MainMenu");
            });
        }

        #endregion

        #region ===== 2. NẠP DỮ LIỆU TẬP TRUNG (CHAPTERS, LESSONS, QUESTIONS, STUDENTS) =====

        private void LoadAllCentralData()
        {
            // 1. Nạp danh sách Bài học & Chương từ physics_theory.json
            TextAsset theoryAsset = Resources.Load<TextAsset>("QuizData/physics_theory");
            if (theoryAsset != null && !string.IsNullOrEmpty(theoryAsset.text))
            {
                try
                {
                    var data = JsonUtility.FromJson<TheoryLessonList>(theoryAsset.text);
                    if (data != null && data.lessons != null)
                    {
                        _lessons = new List<TheoryLessonItem>(data.lessons);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[TeacherDashboard] Lỗi parse physics_theory: {ex.Message}");
                }
            }

            // Tự động phân tách danh sách Chương từ các bài học
            _chapters.Clear();
            var grouped = _lessons.GroupBy(l => l.chuongId).ToList();
            int cOrder = 1;
            foreach (var g in grouped)
            {
                var first = g.First();
                _chapters.Add(new ChapterItem
                {
                    chapterId = g.Key,
                    chapterName = first.tenChuong,
                    order = cOrder++,
                    isActive = true
                });
            }

            if (_chapters.Count == 0)
            {
                _chapters.Add(new ChapterItem { chapterId = "chuong1", chapterName = "Chương I. Năng lượng cơ học", order = 1, isActive = true });
                _chapters.Add(new ChapterItem { chapterId = "chuong2", chapterName = "Chương II. Ánh sáng", order = 2, isActive = true });
                _chapters.Add(new ChapterItem { chapterId = "chuong3", chapterName = "Chương III. Điện từ học", order = 3, isActive = true });
            }

            // 2. Nạp ngân hàng câu hỏi từ physics_questions.json
            TextAsset qAsset = Resources.Load<TextAsset>("QuizData/physics_questions");
            if (qAsset != null && !string.IsNullOrEmpty(qAsset.text))
            {
                try
                {
                    var qList = JsonUtility.FromJson<QuestionDataList>(qAsset.text);
                    if (qList != null && qList.questions != null)
                    {
                        _questions = new List<ScienceQuest.Quiz.QuestionData>(qList.questions);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[TeacherDashboard] Lỗi parse physics_questions: {ex.Message}");
                }
            }

            // 3. Nạp danh sách tiến độ học sinh từ real_achievements_data.json
            TextAsset achAsset = Resources.Load<TextAsset>("QuizData/real_achievements_data");
            if (achAsset != null && !string.IsNullOrEmpty(achAsset.text))
            {
                try
                {
                    var ds = JsonUtility.FromJson<RealAchievementDataset>(achAsset.text);
                    if (ds != null && ds.students != null)
                    {
                        _students.Clear();
                        foreach (var s in ds.students)
                        {
                            _students.Add(new LeaderboardEntry
                            {
                                playerName = s.playerName,
                                className = s.className,
                                characterId = s.avatarId,
                                score = s.score,
                                accuracy = s.accuracy,
                                timeUsed = s.timeUsed,
                                lessonName = s.lesson,
                                date = s.date
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[TeacherDashboard] Lỗi parse real_achievements_data: {ex.Message}");
                }
            }

            Debug.Log($"[TeacherDashboard] 📦 Đã nạp: {_chapters.Count} chương, {_lessons.Count} bài học, {_questions.Count} câu hỏi, {_students.Count} học sinh.");
        }

        #endregion

        #region ===== 3. THANH ĐIỀU HƯỚNG & NÚT THẺ DASHBOARD =====

        private void CreateTopNavigationBar()
        {
            if (_mainCanvas == null) return;

            _topBarObj = new GameObject("TeacherTopBar");
            _topBarObj.transform.SetParent(_mainCanvas.transform, false);
            RectTransform rt = _topBarObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -15f);
            rt.sizeDelta = new Vector2(-40f, 70f);

            // Nút Quay lại Menu
            CreateButton(_topBarObj.transform, "btnQuayLai", "◀ QUAY LẠI MENU", new Vector2(-750f, 0f), new Vector2(210f, 48f), new Color(0.24f, 0.20f, 0.16f, 0.95f), () =>
            {
                if (Application.CanStreamedLevelBeLoaded("01_MainMenu"))
                    SceneManager.LoadScene("01_MainMenu");
                else if (Application.CanStreamedLevelBeLoaded("GameSelection"))
                    SceneManager.LoadScene("GameSelection");
                else
                    SceneManager.LoadScene(0);
            });

            // Huy hiệu Giáo viên góc phải
            GameObject badgeObj = new GameObject("TeacherBadge");
            badgeObj.transform.SetParent(_topBarObj.transform, false);
            RectTransform brt = badgeObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(1f, 0.5f);
            brt.anchorMax = new Vector2(1f, 0.5f);
            brt.pivot = new Vector2(1f, 0.5f);
            brt.anchoredPosition = new Vector2(-20f, 0f);
            brt.sizeDelta = new Vector2(380f, 48f);

            Image bimg = badgeObj.AddComponent<Image>();
            bimg.color = new Color(0.12f, 0.18f, 0.24f, 0.92f);

            GameObject textObj = new GameObject("BadgeText");
            textObj.transform.SetParent(badgeObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(15f, 5f);
            trt.offsetMax = new Vector2(-15f, -5f);
            TextMeshProUGUI btmp = textObj.AddComponent<TextMeshProUGUI>();
            btmp.text = $"👨‍🏫 Giáo Viên: {_teacherName}  🟢 Online";
            btmp.fontSize = 15;
            btmp.fontStyle = FontStyles.Bold;
            btmp.color = new Color(0.95f, 0.95f, 1f);
            btmp.alignment = TextAlignmentOptions.MidlineRight;

            // Nút Đăng Xuất tài khoản Giáo viên
            var btnDx = CreateButton(_topBarObj.transform, "btnDangXuat", "ĐĂNG XUẤT", new Vector2(-415f, 0f), new Vector2(130f, 44f), new Color(0.65f, 0.22f, 0.22f, 0.95f), () =>
            {
                if (GameSession.Instance != null) GameSession.Instance.ResetSession();
                PlayerPrefs.DeleteKey("Current_Username");
                PlayerPrefs.DeleteKey("Current_UserRole");
                PlayerPrefs.Save();
                _isAuthorized = false;
                CloseAllModals();
                ShowTeacherLoginDialog();
                ShowToast("Đã đăng xuất tài khoản Giáo viên!");
            });
            RectTransform dxRt = btnDx.GetComponent<RectTransform>();
            dxRt.anchorMin = new Vector2(1f, 0.5f);
            dxRt.anchorMax = new Vector2(1f, 0.5f);
            dxRt.pivot = new Vector2(1f, 0.5f);
            dxRt.anchoredPosition = new Vector2(-415f, 0f);
        }

        private void UpdateTeacherBadge()
        {
            if (_topBarObj != null)
            {
                var tmp = _topBarObj.transform.Find("TeacherBadge/BadgeText")?.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = $"👨‍🏫 Giáo Viên: {_teacherName} ({_teacherId})  🟢";
                }
            }
        }

        private void SetupCardButton(GameObject cardObj, string label, Action onClick)
        {
            if (cardObj == null) return;

            UIHelper.GetOrAddButton(cardObj, () =>
            {
                if (!_isAuthorized)
                {
                    ShowTeacherLoginDialog();
                    return;
                }

                StartCoroutine(AnimateCardPunch(cardObj.GetComponent<RectTransform>(), onClick));
            });

            // Thêm hiệu ứng hover phóng to nhẹ
            var trigger = cardObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null) trigger = cardObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((data) => { cardObj.transform.localScale = Vector3.one * 1.04f; });
            trigger.triggers.Add(entryEnter);

            var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
            entryExit.callback.AddListener((data) => { cardObj.transform.localScale = Vector3.one; });
            trigger.triggers.Add(entryExit);
        }

        private IEnumerator AnimateCardPunch(RectTransform rt, Action onComplete)
        {
            if (rt != null)
            {
                float duration = 0.18f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float scale = (t < 0.5f) ? Mathf.Lerp(1.04f, 0.95f, t / 0.5f) : Mathf.Lerp(0.95f, 1f, (t - 0.5f) / 0.5f);
                    rt.localScale = Vector3.one * scale;
                    yield return null;
                }
                rt.localScale = Vector3.one;
            }
            onComplete?.Invoke();
        }

        #endregion

        #region ===== 4. MÀN HÌNH QUẢN LÝ CHƯƠNG (CHAPTER MANAGEMENT) =====

        private void OpenChapterManagement()
        {
            CloseAllModals();

            _panelChuongObj = CreateModalRoot("ModalQuanLyChuong", new Vector2(1050f, 650f));
            Transform container = _panelChuongObj.transform.Find("Container");

            // Header
            CreateModalHeader(container, "📚 QUẢN LÝ DANH SÁCH CHƯƠNG VẬT LÝ 9", CloseAllModals);

            // Nút Thêm chương mới
            CreateButton(container, "btnAddChuong", "+ THÊM CHƯƠNG MỚI", new Vector2(-360f, 225f), new Vector2(220f, 42f), new Color(0.18f, 0.52f, 0.35f), () =>
            {
                ShowAddChapterSubDialog();
            });

            // Tổng số chương
            CreateLabel(container, $"Tổng cộng: {_chapters.Count} chương học", 16, FontStyles.Italic, new Color(1f, 0.88f, 0.55f), new Vector2(360f, 225f), new Vector2(250f, 40f));

            // Danh sách cuộn chứa các chương
            var scrollObj = CreateScrollView(container, "ScrollChapters", new Vector2(0f, -40f), new Vector2(980f, 460f));
            Transform content = scrollObj.transform.Find("Viewport/Content");

            float itemHeight = 70f;
            for (int i = 0; i < _chapters.Count; i++)
            {
                var ch = _chapters[i];
                int idx = i;

                GameObject row = new GameObject($"ChapterRow_{i}");
                row.transform.SetParent(content, false);
                RectTransform rrt = row.AddComponent<RectTransform>();
                rrt.sizeDelta = new Vector2(950f, itemHeight - 8f);

                Image rbg = row.AddComponent<Image>();
                rbg.color = (i % 2 == 0) ? new Color(0.16f, 0.16f, 0.22f, 0.85f) : new Color(0.20f, 0.20f, 0.28f, 0.85f);

                // Số thứ tự
                CreateLabel(row.transform, $"{ch.order}.", 20, FontStyles.Bold, Color.white, new Vector2(-430f, 0f), new Vector2(50f, 40f));

                // Tên chương
                CreateLabel(row.transform, ch.chapterName, 18, FontStyles.Bold, new Color(1f, 0.95f, 0.75f), new Vector2(-120f, 0f), new Vector2(520f, 45f), TextAlignmentOptions.MidlineLeft);

                // Trạng thái badge
                string statusText = ch.isActive ? "🟢 Đang áp dụng" : "⚪ Tạm dừng";
                Color statusColor = ch.isActive ? new Color(0.3f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f);
                CreateLabel(row.transform, statusText, 14, FontStyles.Bold, statusColor, new Vector2(210f, 0f), new Vector2(140f, 35f));

                // Nút Bật / Tắt trạng thái
                CreateButton(row.transform, $"btnToggle_{i}", ch.isActive ? "Ẩn" : "Bật", new Vector2(320f, 0f), new Vector2(70f, 36f), ch.isActive ? new Color(0.6f, 0.3f, 0.2f) : new Color(0.2f, 0.55f, 0.3f), () =>
                {
                    ch.isActive = !ch.isActive;
                    OpenChapterManagement(); // Refresh
                    ShowToast($"Đã {(ch.isActive ? "kích hoạt" : "tạm dừng")} {ch.chapterName}");
                });

                // Nút Chỉnh sửa tên
                CreateButton(row.transform, $"btnEdit_{i}", "Sửa", new Vector2(405f, 0f), new Vector2(70f, 36f), new Color(0.25f, 0.45f, 0.7f), () =>
                {
                    ShowEditChapterSubDialog(ch);
                });
            }
        }

        private void ShowAddChapterSubDialog()
        {
            var dialog = CreateModalRoot("DialogAddChapter", new Vector2(650f, 350f));
            Transform cont = dialog.transform.Find("Container");

            CreateModalHeader(cont, "➕ THÊM CHƯƠNG VẬT LÝ MỚI", () => Destroy(dialog));

            CreateLabel(cont, "Tên chương mới:", 16, FontStyles.Bold, Color.white, new Vector2(0f, 75f), new Vector2(550f, 30f));
            var nameInput = CreateInputField(cont, "inputChName", "Ví dụ: Chương V. Năng lượng tái tạo...", new Vector2(0f, 30f), new Vector2(550f, 45f));

            CreateLabel(cont, "Thứ tự hiển thị:", 16, FontStyles.Bold, Color.white, new Vector2(-150f, -30f), new Vector2(250f, 30f));
            var orderInput = CreateInputField(cont, "inputOrder", (_chapters.Count + 1).ToString(), new Vector2(150f, -30f), new Vector2(150f, 45f));

            CreateButton(cont, "btnSaveCh", "💾 LƯU CHƯƠNG", new Vector2(0f, -95f), new Vector2(240f, 48f), new Color(0.18f, 0.55f, 0.34f), () =>
            {
                string cName = nameInput.text.Trim();
                if (string.IsNullOrEmpty(cName))
                {
                    ShowToast("⚠️ Tên chương không được để trống!");
                    return;
                }
                int.TryParse(orderInput.text, out int ord);
                if (ord <= 0) ord = _chapters.Count + 1;

                string newId = $"chuong{_chapters.Count + 1}";
                _chapters.Add(new ChapterItem { chapterId = newId, chapterName = cName, order = ord, isActive = true });

                Destroy(dialog);
                OpenChapterManagement();
                ShowToast($"✅ Đã thêm thành công: {cName}");
            });
        }

        private void ShowEditChapterSubDialog(ChapterItem ch)
        {
            var dialog = CreateModalRoot("DialogEditChapter", new Vector2(650f, 350f));
            Transform cont = dialog.transform.Find("Container");

            CreateModalHeader(cont, "✏️ CHỈNH SỬA TÊN CHƯƠNG", () => Destroy(dialog));

            CreateLabel(cont, "Tên chương:", 16, FontStyles.Bold, Color.white, new Vector2(0f, 75f), new Vector2(550f, 30f));
            var nameInput = CreateInputField(cont, "inputChName", ch.chapterName, new Vector2(0f, 30f), new Vector2(550f, 45f));
            nameInput.text = ch.chapterName;

            CreateButton(cont, "btnSaveCh", "💾 CẬP NHẬT", new Vector2(0f, -60f), new Vector2(240f, 48f), new Color(0.18f, 0.55f, 0.34f), () =>
            {
                string cName = nameInput.text.Trim();
                if (string.IsNullOrEmpty(cName))
                {
                    ShowToast("⚠️ Tên chương không được để trống!");
                    return;
                }
                ch.chapterName = cName;
                Destroy(dialog);
                OpenChapterManagement();
                ShowToast("✅ Đã cập nhật tên chương thành công!");
            });
        }

        #endregion

        #region ===== 5. MÀN HÌNH QUẢN LÝ BÀI HỌC & LÝ THUYẾT (LESSON MANAGEMENT) =====

        private string _selectedChapterFilter = "";

        private void OpenLessonManagement()
        {
            CloseAllModals();

            _panelBaiObj = CreateModalRoot("ModalQuanLyBai", new Vector2(1150f, 700f));
            Transform container = _panelBaiObj.transform.Find("Container");

            CreateModalHeader(container, "📖 QUẢN LÝ BÀI HỌC & NỘI DUNG LÝ THUYẾT", CloseAllModals);

            if (string.IsNullOrEmpty(_selectedChapterFilter) && _chapters.Count > 0)
                _selectedChapterFilter = _chapters[0].chapterId;

            // Thanh chọn chương (Filter Tabs) - Hiển thị ĐẦY ĐỦ tất cả các chương
            CreateLabel(container, "Chọn Chương:", 15, FontStyles.Bold, new Color(1f, 0.88f, 0.55f), new Vector2(-475f, 250f), new Vector2(120f, 35f));
            string[] romanNumbers = { "I", "II", "III", "IV", "V", "VI", "VII" };
            float tabX = -345f;
            for (int c = 0; c < _chapters.Count; c++)
            {
                var ch = _chapters[c];
                bool isCur = ch.chapterId == _selectedChapterFilter;
                Color tCol = isCur ? new Color(0.2f, 0.6f, 0.85f) : new Color(0.25f, 0.25f, 0.32f);

                string tabName = ch.chapterId == "tonghop" ? "Tổng ôn" : (c < romanNumbers.Length ? $"Chương {romanNumbers[c]}" : ch.chapterName);

                CreateButton(container, $"tabCh_{c}", tabName, new Vector2(tabX, 250f), new Vector2(105f, 36f), tCol, () =>
                {
                    _selectedChapterFilter = ch.chapterId;
                    OpenLessonManagement();
                });
                tabX += 114f;
            }

            // Danh sách bài học của chương đang chọn
            var filteredLessons = _lessons.Where(l => string.IsNullOrEmpty(_selectedChapterFilter) || l.chuongId == _selectedChapterFilter).ToList();

            var scrollObj = CreateScrollView(container, "ScrollLessons", new Vector2(0f, -25f), new Vector2(1080f, 510f));
            Transform content = scrollObj.transform.Find("Viewport/Content");

            float lHeight = 110f;
            for (int i = 0; i < filteredLessons.Count; i++)
            {
                var les = filteredLessons[i];
                GameObject lRow = new GameObject($"LessonRow_{i}");
                lRow.transform.SetParent(content, false);
                RectTransform lrt = lRow.AddComponent<RectTransform>();
                lrt.sizeDelta = new Vector2(1050f, lHeight - 10f);

                Image lbg = lRow.AddComponent<Image>();
                lbg.color = new Color(0.15f, 0.16f, 0.22f, 0.92f);

                // Badge Mã bài
                CreateLabel(lRow.transform, les.baiID.ToUpper(), 14, FontStyles.Bold, new Color(0.3f, 0.8f, 1f), new Vector2(-460f, 25f), new Vector2(90f, 30f));

                // Tên bài
                CreateLabel(lRow.transform, les.tenBai, 18, FontStyles.Bold, new Color(1f, 0.95f, 0.65f), new Vector2(-140f, 25f), new Vector2(530f, 35f), TextAlignmentOptions.MidlineLeft);

                // Trích dẫn khái niệm
                string summary = les.khaiNiemTrongTam;
                if (!string.IsNullOrEmpty(summary) && summary.Length > 80) summary = summary.Substring(0, 80) + "...";
                CreateLabel(lRow.transform, summary, 13, FontStyles.Italic, new Color(0.85f, 0.85f, 0.85f), new Vector2(-110f, -15f), new Vector2(600f, 35f), TextAlignmentOptions.MidlineLeft);

                // Nút Chỉnh sửa lý thuyết
                CreateButton(lRow.transform, $"btnEditLes_{i}", "✏️ SỬA LÝ THUYẾT", new Vector2(420f, 0f), new Vector2(160f, 42f), new Color(0.2f, 0.5f, 0.75f), () =>
                {
                    ShowEditLessonDialog(les);
                });
            }
        }

        private void ShowEditLessonDialog(TheoryLessonItem les)
        {
            var dialog = CreateModalRoot("DialogEditLesson", new Vector2(980f, 650f));
            Transform cont = dialog.transform.Find("Container");

            CreateModalHeader(cont, $"✏️ CHỈNH SỬA BÀI HỌC: {les.tenBai}", () => Destroy(dialog));

            // Tên bài
            CreateLabel(cont, "Tên bài học:", 15, FontStyles.Bold, Color.white, new Vector2(-220f, 230f), new Vector2(450f, 30f), TextAlignmentOptions.MidlineLeft);
            var nameInp = CreateInputField(cont, "inpName", les.tenBai, new Vector2(0f, 195f), new Vector2(900f, 40f));
            nameInp.text = les.tenBai;

            // Khái niệm trọng tâm
            CreateLabel(cont, "Khái niệm trọng tâm (Trang 1):", 15, FontStyles.Bold, new Color(0.4f, 0.85f, 1f), new Vector2(-220f, 145f), new Vector2(450f, 30f), TextAlignmentOptions.MidlineLeft);
            var knInp = CreateInputField(cont, "inpKn", les.khaiNiemTrongTam, new Vector2(0f, 85f), new Vector2(900f, 80f), TMP_InputField.ContentType.Standard, true);
            knInp.text = les.khaiNiemTrongTam;

            // Công thức cần nhớ
            CreateLabel(cont, "Công thức cần nhớ (Trang 2):", 15, FontStyles.Bold, new Color(1f, 0.85f, 0.4f), new Vector2(-220f, 15f), new Vector2(450f, 30f), TextAlignmentOptions.MidlineLeft);
            var ctInp = CreateInputField(cont, "inpCt", les.congThucCanNho, new Vector2(0f, -45f), new Vector2(900f, 80f), TMP_InputField.ContentType.Standard, true);
            ctInp.text = les.congThucCanNho;

            // Hiện tượng & Ứng dụng
            CreateLabel(cont, "Hiện tượng & Ứng dụng thực tế (Trang 3):", 15, FontStyles.Bold, new Color(0.4f, 1f, 0.6f), new Vector2(-220f, -115f), new Vector2(450f, 30f), TextAlignmentOptions.MidlineLeft);
            var htInp = CreateInputField(cont, "inpHt", les.hienTuongUngDung, new Vector2(0f, -175f), new Vector2(900f, 80f), TMP_InputField.ContentType.Standard, true);
            htInp.text = les.hienTuongUngDung;

            // Nút Lưu
            CreateButton(cont, "btnSaveLesson", "💾 LƯU BÀI HỌC VÀO HỆ THỐNG", new Vector2(0f, -255f), new Vector2(320f, 48f), new Color(0.18f, 0.55f, 0.34f), () =>
            {
                les.tenBai = nameInp.text.Trim();
                les.khaiNiemTrongTam = knInp.text.Trim();
                les.congThucCanNho = ctInp.text.Trim();
                les.hienTuongUngDung = htInp.text.Trim();

                Destroy(dialog);
                OpenLessonManagement();
                ShowToast("✅ Đã cập nhật lý thuyết bài học thành công! Game sẽ áp dụng ngay.");
            });
        }

        #endregion

        #region ===== 6. MÀN HÌNH QUẢN LÝ CÂU HỎI & ĐẨY LÊN GOOGLE SHEET (QUESTION MANAGEMENT) =====

        private string _filterMucDo = "Tất cả";
        private string _filterLoai = "Tất cả";

        private void OpenQuestionManagement()
        {
            CloseAllModals();

            _panelCauHoiObj = CreateModalRoot("ModalQuanLyCauHoi", new Vector2(1200f, 720f));
            Transform container = _panelCauHoiObj.transform.Find("Container");

            CreateModalHeader(container, "❓ QUẢN LÝ NGÂN HÀNG CÂU HỎI & ĐỒNG BỘ GOOGLE SHEET", CloseAllModals);

            // Nút Thêm câu hỏi mới (Nổi bật rực rỡ)
            CreateButton(container, "btnAddQ", "➕ THÊM CÂU HỎI MỚI", new Vector2(-420f, 260f), new Vector2(230f, 44f), new Color(0.15f, 0.60f, 0.40f), () =>
            {
                ShowAddOrEditQuestionForm(null);
            });

            // Nút Tải lại / đồng bộ từ Sheet
            CreateButton(container, "btnSyncQ", "🔄 Tải Từ Sheet", new Vector2(-220f, 260f), new Vector2(140f, 44f), new Color(0.25f, 0.45f, 0.70f), () =>
            {
                ShowToast("🔄 Đang đồng bộ danh sách câu hỏi từ Google Sheet...");
                if (GoogleSheetDataManager.Instance != null)
                {
                    StartCoroutine(GoogleSheetDataManager.Instance.FetchQuestionsFromSheet((success) =>
                    {
                        if (success)
                        {
                            LoadAllCentralData();
                            OpenQuestionManagement();
                            ShowToast("✅ Đã cập nhật ngân hàng câu hỏi mới nhất từ Google Sheet!");
                        }
                        else
                        {
                            ShowToast("⚠️ Không thể tải từ Sheet. Kiểm tra lại kết nối mạng!");
                        }
                    }));
                }
            });

            // Bộ lọc Mức độ
            CreateLabel(container, "Mức độ:", 15, FontStyles.Bold, Color.white, new Vector2(-50f, 260f), new Vector2(80f, 35f));
            string[] mucDoList = { "Tất cả", "Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao" };
            float mx = 40f;
            foreach (var md in mucDoList)
            {
                bool isCur = md == _filterMucDo;
                CreateButton(container, $"btnMd_{md}", md, new Vector2(mx, 260f), new Vector2(85f, 34f), isCur ? new Color(0.85f, 0.55f, 0.15f) : new Color(0.25f, 0.25f, 0.32f), () =>
                {
                    _filterMucDo = md;
                    OpenQuestionManagement();
                });
                mx += 92f;
            }

            // Lọc danh sách
            var displayList = _questions.Where(q =>
            {
                if (_filterMucDo != "Tất cả" && !string.IsNullOrEmpty(q.mucDo) && !q.mucDo.Equals(_filterMucDo, StringComparison.OrdinalIgnoreCase))
                    return false;
                return true;
            }).ToList();

            CreateLabel(container, $"Hiển thị: {displayList.Count} câu hỏi", 15, FontStyles.Italic, new Color(1f, 0.88f, 0.5f), new Vector2(490f, 260f), new Vector2(170f, 35f));

            // Danh sách cuộn câu hỏi
            var scrollObj = CreateScrollView(container, "ScrollQuestions", new Vector2(0f, -25f), new Vector2(1140f, 530f));
            Transform content = scrollObj.transform.Find("Viewport/Content");

            float qHeight = 125f;
            for (int i = 0; i < displayList.Count; i++)
            {
                var q = displayList[i];
                int idx = i;

                GameObject qRow = new GameObject($"QRow_{i}");
                qRow.transform.SetParent(content, false);
                RectTransform qrt = qRow.AddComponent<RectTransform>();
                qrt.sizeDelta = new Vector2(1110f, qHeight - 10f);

                Image qbg = qRow.AddComponent<Image>();
                qbg.color = new Color(0.14f, 0.15f, 0.22f, 0.92f);

                // Mã câu hỏi & Mã bài
                string idStr = !string.IsNullOrEmpty(q.questionID) ? q.questionID : $"C9_{(i + 1):D3}";
                string baiStr = !string.IsNullOrEmpty(q.baiID) ? q.baiID : "B9_001";
                CreateLabel(qRow.transform, $"{idStr} | {baiStr}", 15, FontStyles.Bold, new Color(0.35f, 0.85f, 1f), new Vector2(-460f, 30f), new Vector2(150f, 30f), TextAlignmentOptions.MidlineLeft);

                // Badge Mức độ
                string mdStr = !string.IsNullOrEmpty(q.mucDo) ? q.mucDo : "Nhận biết";
                Color mdColor = (mdStr == "Nhận biết") ? new Color(0.2f, 0.8f, 0.4f) :
                                (mdStr == "Thông hiểu") ? new Color(0.2f, 0.6f, 1f) :
                                (mdStr == "Vận dụng") ? new Color(0.95f, 0.6f, 0.1f) : new Color(0.95f, 0.3f, 0.3f);
                CreateLabel(qRow.transform, $"[{mdStr}]", 14, FontStyles.Bold, mdColor, new Vector2(-320f, 30f), new Vector2(110f, 30f), TextAlignmentOptions.MidlineLeft);

                // Nội dung đề bài
                string qText = q.questionText;
                if (!string.IsNullOrEmpty(qText) && qText.Length > 90) qText = qText.Substring(0, 90) + "...";
                CreateLabel(qRow.transform, qText, 16, FontStyles.Bold, Color.white, new Vector2(-80f, 2f), new Vector2(580f, 40f), TextAlignmentOptions.MidlineLeft);

                // Preview Đáp án đúng / Tiêu chí
                string correctPrev = "";
                if (q.answers != null && q.answers.Length > q.correctAnswerIndex && q.correctAnswerIndex >= 0)
                {
                    char optChar = (char)('A' + q.correctAnswerIndex);
                    correctPrev = $"Đáp án đúng: {optChar}. {q.answers[q.correctAnswerIndex]}";
                    if (correctPrev.Length > 70) correctPrev = correctPrev.Substring(0, 70) + "...";
                }
                else if (!string.IsNullOrEmpty(q.dapAnThamChieu))
                {
                    correctPrev = $"Tự luận: {q.dapAnThamChieu}";
                    if (correctPrev.Length > 70) correctPrev = correctPrev.Substring(0, 70) + "...";
                }
                CreateLabel(qRow.transform, correctPrev, 13, FontStyles.Italic, new Color(0.4f, 1f, 0.6f), new Vector2(-80f, -30f), new Vector2(580f, 30f), TextAlignmentOptions.MidlineLeft);

                // Nút Sửa
                CreateButton(qRow.transform, $"btnEditQ_{i}", "Sửa", new Vector2(340f, 0f), new Vector2(75f, 38f), new Color(0.2f, 0.5f, 0.75f), () =>
                {
                    ShowAddOrEditQuestionForm(q);
                });

                // Nút Đẩy lên Google Sheet riêng lẻ
                CreateButton(qRow.transform, $"btnPushQ_{i}", "🚀 ĐẨY SHEET", new Vector2(445f, 0f), new Vector2(120f, 38f), new Color(0.18f, 0.58f, 0.35f), () =>
                {
                    PushQuestionToGoogleSheet(q);
                });
            }
        }

        /// <summary>
        /// Form Thêm / Chỉnh sửa câu hỏi theo đúng 14 cột chuẩn Google Sheets
        /// </summary>
        #region ===== DANH MỤC CHƯƠNG & BÀI HỌC VẬT LÝ 9 (PHỤC VỤ THÊM CÂU HỎI) =====

        private class LessonChoice
        {
            public string baiId;
            public string shortName;
            public string fullName;
            public LessonChoice(string id, string shortN, string fullN)
            {
                baiId = id;
                shortName = shortN;
                fullName = fullN;
            }
        }

        private static readonly Dictionary<string, (string title, string shortCode, List<LessonChoice> lessons)> ChapterLessonMap = new Dictionary<string, (string, string, List<LessonChoice>)>
        {
            { "chuong1", ("Chương I. Năng lượng cơ học", "Chương I", new List<LessonChoice>
                {
                    new LessonChoice("bai2", "Bài 2", "Bài 2. Động năng. Thế năng"),
                    new LessonChoice("bai3", "Bài 3", "Bài 3. Cơ năng"),
                    new LessonChoice("bai4", "Bài 4", "Bài 4. Công và Công suất"),
                    new LessonChoice("test_chuong1", "TEST C1", "Đề TEST Chương 1 - Cơ học")
                })
            },
            { "chuong2", ("Chương II. Ánh sáng", "Chương II", new List<LessonChoice>
                {
                    new LessonChoice("bai5", "Bài 5", "Bài 5. Khúc xạ ánh sáng"),
                    new LessonChoice("bai6", "Bài 6", "Bài 6. Phản xạ toàn phần"),
                    new LessonChoice("bai7", "Bài 7", "Bài 7. Thấu kính hội tụ"),
                    new LessonChoice("bai8", "Bài 8", "Bài 8. Thấu kính phân kì"),
                    new LessonChoice("bai9", "Bài 9", "Bài 9. Sự tạo ảnh qua thấu kính"),
                    new LessonChoice("bai10", "Bài 10", "Bài 10. Kính lúp"),
                    new LessonChoice("test_chuong2", "TEST C2", "Đề TEST Chương 2 - Ánh sáng")
                })
            },
            { "chuong3", ("Chương III. Điện từ học", "Chương III", new List<LessonChoice>
                {
                    new LessonChoice("bai11", "Bài 11", "Bài 11. Từ trường"),
                    new LessonChoice("bai12", "Bài 12", "Bài 12. Từ phổ - Đường sức từ"),
                    new LessonChoice("bai13", "Bài 13", "Bài 13. Lực điện từ"),
                    new LessonChoice("test_chuong3", "TEST C3", "Đề TEST Chương 3 - Điện từ học")
                })
            },
            { "chuong4", ("Chương IV. Điện thế & Dòng điện xoay chiều", "Chương IV", new List<LessonChoice>
                {
                    new LessonChoice("bai14", "Bài 14", "Bài 14. Hiện tượng cảm ứng điện từ"),
                    new LessonChoice("bai15", "Bài 15", "Bài 15. Dòng điện xoay chiều"),
                    new LessonChoice("test_chuong4", "TEST C4", "Đề TEST Chương 4 - Dòng điện xoay chiều")
                })
            },
            { "chuong5", ("Chương V. Năng lượng & Sự bảo toàn năng lượng", "Chương V", new List<LessonChoice>
                {
                    new LessonChoice("bai16", "Bài 16", "Bài 16. Năng lượng và sự chuyển hóa năng lượng"),
                    new LessonChoice("bai17", "Bài 17", "Bài 17. Định luật bảo toàn và chuyển hóa năng lượng"),
                    new LessonChoice("test_chuong5", "TEST C5", "Đề TEST Chương 5 - Bảo toàn năng lượng")
                })
            },
            { "tonghop", ("TỔNG ÔN CẢ QUYỂN (5 chương)", "Tổng ôn", new List<LessonChoice>
                {
                    new LessonChoice("test_tonghop", "TEST Tổng Hợp", "Đề TEST tổng hợp toàn bộ Vật lý 9")
                })
            }
        };

        #endregion

        /// <summary>
        /// Form Thêm / Chỉnh sửa câu hỏi theo đúng chuẩn 18 CỘT Google Sheets
        /// Hỗ trợ đầy đủ: Chọn Chương, Chọn Bài học, Chọn Trắc nghiệm / Tự luận, Chọn 4 Mức độ, ID tự tăng
        /// </summary>
        private void ShowAddOrEditQuestionForm(ScienceQuest.Quiz.QuestionData editTarget)
        {
            bool isNew = editTarget == null;
            var q = isNew ? new ScienceQuest.Quiz.QuestionData() : editTarget;

            var form = CreateModalRoot("FormQuestion", new Vector2(1180f, 760f));
            Transform cont = form.transform.Find("Container");

            string title = isNew ? "➕ THÊM CÂU HỎI MỚI" : "✏️ CHỈNH SỬA CÂU HỎI";
            CreateModalHeader(cont, title, () => Destroy(form));

            // Tự động sinh ID nảy số tiếp theo nếu là tạo mới (dò số lớn nhất trong danh sách câu hỏi)
            string autoID = q.questionID;
            if (string.IsNullOrEmpty(autoID))
            {
                int maxIdNum = 0;
                foreach (var item in _questions)
                {
                    if (!string.IsNullOrEmpty(item.questionID))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(item.questionID, @"\d+");
                        if (match.Success && int.TryParse(match.Value, out int n) && n > maxIdNum)
                        {
                            maxIdNum = n;
                        }
                    }
                }
                int nextNum = Mathf.Max(_questions.Count + 1, maxIdNum + 1);
                autoID = $"C9_{nextNum:D3}";
            }

            // =========================================================================
            // HÀNG 1: MÃ ID (TỰ TĂNG) | LOẠI CÂU HỎI (TRẮC NGHIỆM / TỰ LUẬN) | GIÁO VIÊN ID
            // =========================================================================
            float yRow1 = 285f;
            CreateLabel(cont, "Mã câu hỏi:", 14, FontStyles.Bold, Color.white, new Vector2(-480f, yRow1), new Vector2(120f, 25f), TextAlignmentOptions.MidlineLeft);
            var idInp = CreateInputField(cont, "inpQId", autoID, new Vector2(-350f, yRow1), new Vector2(110f, 34f));
            idInp.text = autoID;

            // Loại câu hỏi (Trắc nghiệm vs Tự luận)
            CreateLabel(cont, "Hình thức (*):", 14, FontStyles.Bold, new Color(1f, 0.9f, 0.45f), new Vector2(-190f, yRow1), new Vector2(100f, 25f), TextAlignmentOptions.MidlineLeft);
            bool isEssay = !string.IsNullOrEmpty(q.loaiCauHoi) && (q.loaiCauHoi.Contains("Tự luận") || q.loaiCauHoi.Contains("TuLuan"));

            Button btnMcq = null;
            Button btnEssay = null;
            GameObject mcqContainer = new GameObject("McqContainer");
            mcqContainer.transform.SetParent(cont, false);
            GameObject essayContainer = new GameObject("EssayContainer");
            essayContainer.transform.SetParent(cont, false);

            Action updateQuestionTypeVisual = () =>
            {
                if (btnMcq != null)
                {
                    var img = btnMcq.GetComponent<Image>();
                    if (img != null) img.color = !isEssay ? new Color(0.18f, 0.55f, 0.88f) : new Color(0.20f, 0.23f, 0.30f);
                    var txt = btnMcq.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        txt.text = !isEssay ? "✓ 📝 TRẮC NGHIỆM" : "📝 TRẮC NGHIỆM";
                        txt.fontStyle = !isEssay ? FontStyles.Bold : FontStyles.Normal;
                    }
                }
                if (btnEssay != null)
                {
                    var img = btnEssay.GetComponent<Image>();
                    if (img != null) img.color = isEssay ? new Color(0.85f, 0.45f, 0.20f) : new Color(0.20f, 0.23f, 0.30f);
                    var txt = btnEssay.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        txt.text = isEssay ? "✓ ✍️ TỰ LUẬN" : "✍️ TỰ LUẬN";
                        txt.fontStyle = isEssay ? FontStyles.Bold : FontStyles.Normal;
                    }
                }
                mcqContainer.SetActive(!isEssay);
                essayContainer.SetActive(isEssay);
            };

            btnMcq = CreateButton(cont, "btnTypeMcq", "📝 TRẮC NGHIỆM", new Vector2(-45f, yRow1), new Vector2(145f, 34f), new Color(0.18f, 0.55f, 0.88f), () =>
            {
                isEssay = false;
                updateQuestionTypeVisual();
            });

            btnEssay = CreateButton(cont, "btnTypeEssay", "✍️ TỰ LUẬN", new Vector2(110f, yRow1), new Vector2(135f, 34f), new Color(0.20f, 0.23f, 0.30f), () =>
            {
                isEssay = true;
                updateQuestionTypeVisual();
            });

            CreateLabel(cont, "GV ID:", 14, FontStyles.Bold, Color.white, new Vector2(255f, yRow1), new Vector2(60f, 25f), TextAlignmentOptions.MidlineLeft);
            var gvInp = CreateInputField(cont, "inpGvId", _teacherId, new Vector2(350f, yRow1), new Vector2(100f, 34f));
            gvInp.text = !string.IsNullOrEmpty(q.giaoVienID) ? q.giaoVienID : _teacherId;

            // =========================================================================
            // HÀNG 2: CHỌN CHƯƠNG (6 NÚT CHỌN NHANH)
            // =========================================================================
            float yRow2 = 242f;
            CreateLabel(cont, "Chọn Chương (*):", 14, FontStyles.Bold, new Color(1f, 0.9f, 0.45f), new Vector2(-480f, yRow2), new Vector2(125f, 25f), TextAlignmentOptions.MidlineLeft);

            string selectedChuongId = !string.IsNullOrEmpty(q.chuongId) ? q.chuongId : GetChuongIdFromBaiId(q.baiID);
            string selectedTenChuong = !string.IsNullOrEmpty(q.tenChuong) ? q.tenChuong : GetTenChuongFromId(selectedChuongId);
            string selectedBaiID = !string.IsNullOrEmpty(q.baiID) ? q.baiID : "bai2";
            string selectedTenBai = !string.IsNullOrEmpty(q.tenBai) ? q.tenBai : GetTenBaiFromId(selectedBaiID);

            string[] chapterKeys = { "chuong1", "chuong2", "chuong3", "chuong4", "chuong5", "tonghop" };
            Button[] chapterBtns = new Button[chapterKeys.Length];

            // Vùng chứa các nút bài học động
            float yRow3 = 200f;
            CreateLabel(cont, "Chọn Bài học (*):", 14, FontStyles.Bold, new Color(1f, 0.9f, 0.45f), new Vector2(-480f, yRow3), new Vector2(125f, 25f), TextAlignmentOptions.MidlineLeft);
            GameObject lessonBtnContainer = new GameObject("LessonBtnContainer");
            lessonBtnContainer.transform.SetParent(cont, false);

            TextMeshProUGUI lessonBadge = CreateLabel(cont, "", 13, FontStyles.Normal, Color.white, new Vector2(280f, yRow3), new Vector2(380f, 25f), TextAlignmentOptions.MidlineLeft);

            Action rebuildLessonButtons = () =>
            {
                // Xóa các nút bài cũ
                for (int i = lessonBtnContainer.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(lessonBtnContainer.transform.GetChild(i).gameObject);
                }

                if (!ChapterLessonMap.ContainsKey(selectedChuongId)) selectedChuongId = "chuong1";
                var entry = ChapterLessonMap[selectedChuongId];
                var lessons = entry.lessons;

                // Nếu bài hiện tại không thuộc chương vừa chọn, chọn bài đầu tiên của chương
                if (!lessons.Any(l => l.baiId.Equals(selectedBaiID, StringComparison.OrdinalIgnoreCase)))
                {
                    selectedBaiID = lessons[0].baiId;
                    selectedTenBai = lessons[0].fullName;
                }

                float bX = -345f;
                foreach (var les in lessons)
                {
                    var thisLes = les;
                    bool isPick = thisLes.baiId.Equals(selectedBaiID, StringComparison.OrdinalIgnoreCase);
                    Color bCol = isPick ? new Color(0.18f, 0.65f, 0.38f) : new Color(0.20f, 0.23f, 0.30f);

                    float bWidth = Mathf.Max(70f, thisLes.shortName.Length * 11f + 16f);
                    Button bBtn = CreateButton(lessonBtnContainer.transform, $"btnLes_{thisLes.baiId}", isPick ? $"✓ {thisLes.shortName}" : thisLes.shortName, new Vector2(bX + bWidth * 0.5f, yRow3), new Vector2(bWidth, 30f), bCol, () =>
                    {
                        selectedBaiID = thisLes.baiId;
                        selectedTenBai = thisLes.fullName;
                        if (lessonBadge != null)
                        {
                            lessonBadge.text = $"Đang chọn: <b><color=#5DADE2>{selectedTenBai}</color></b> ({selectedBaiID})";
                        }
                        // Cập nhật lại màu các nút bài
                        for (int k = 0; k < lessonBtnContainer.transform.childCount; k++)
                        {
                            var childBtn = lessonBtnContainer.transform.GetChild(k).GetComponent<Button>();
                            if (childBtn != null)
                            {
                                bool isThis = childBtn.gameObject.name == $"btnLes_{thisLes.baiId}";
                                var img = childBtn.GetComponent<Image>();
                                if (img != null) img.color = isThis ? new Color(0.18f, 0.65f, 0.38f) : new Color(0.20f, 0.23f, 0.30f);
                                var txt = childBtn.GetComponentInChildren<TextMeshProUGUI>();
                                if (txt != null) txt.color = isThis ? Color.white : new Color(0.75f, 0.78f, 0.85f);
                            }
                        }
                    });
                    bX += bWidth + 8f;
                }

                if (lessonBadge != null)
                {
                    lessonBadge.text = $"Đang chọn: <b><color=#5DADE2>{selectedTenBai}</color></b> ({selectedBaiID})";
                }
            };

            Action updateChapterButtonsVisual = () =>
            {
                for (int c = 0; c < chapterKeys.Length; c++)
                {
                    string k = chapterKeys[c];
                    bool isCurrent = string.Equals(selectedChuongId, k, StringComparison.OrdinalIgnoreCase);
                    if (chapterBtns[c] != null)
                    {
                        var btnImg = chapterBtns[c].GetComponent<Image>();
                        if (btnImg != null) btnImg.color = isCurrent ? new Color(0.20f, 0.58f, 0.90f) : new Color(0.20f, 0.23f, 0.30f);

                        var txt = chapterBtns[c].GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null)
                        {
                            txt.text = isCurrent ? $"✓ {ChapterLessonMap[k].shortCode}" : ChapterLessonMap[k].shortCode;
                            txt.color = isCurrent ? Color.white : new Color(0.75f, 0.78f, 0.85f);
                            txt.fontStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;
                        }
                    }
                }
                rebuildLessonButtons();
            };

            float cStartX = -300f;
            float cSpacing = 100f;
            for (int c = 0; c < chapterKeys.Length; c++)
            {
                string cKey = chapterKeys[c];
                Vector2 btnPos = new Vector2(cStartX + c * cSpacing, yRow2);
                chapterBtns[c] = CreateButton(cont, $"btnCh_{c}", ChapterLessonMap[cKey].shortCode, btnPos, new Vector2(92f, 32f), new Color(0.20f, 0.23f, 0.30f), () =>
                {
                    selectedChuongId = cKey;
                    selectedTenChuong = ChapterLessonMap[cKey].title;
                    updateChapterButtonsVisual();
                });
            }
            updateChapterButtonsVisual();

            // =========================================================================
            // HÀNG 4: MỨC ĐỘ NHẬN THỨC (4 NÚT CHỌN NHANH)
            // =========================================================================
            float yRow4 = 158f;
            CreateLabel(cont, "Mức độ (*):", 14, FontStyles.Bold, new Color(1f, 0.9f, 0.45f), new Vector2(-480f, yRow4), new Vector2(125f, 25f), TextAlignmentOptions.MidlineLeft);

            string selectedMucDo = !string.IsNullOrEmpty(q.mucDo) ? q.mucDo : "Nhận biết";
            string[] levels = { "Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao" };
            Color[] activeColors = {
                new Color(0.16f, 0.65f, 0.35f), // Xanh lá
                new Color(0.18f, 0.52f, 0.88f), // Xanh dương
                new Color(0.90f, 0.58f, 0.15f), // Cam vàng
                new Color(0.90f, 0.25f, 0.25f)  // Đỏ tươi
            };
            Color inactiveColor = new Color(0.20f, 0.23f, 0.30f, 0.95f);

            Button[] levelBtns = new Button[levels.Length];
            TextMeshProUGUI badgeText = null;

            Action updateLevelButtonsVisual = () =>
            {
                for (int l = 0; l < levels.Length; l++)
                {
                    bool isCurrent = string.Equals(selectedMucDo, levels[l], StringComparison.OrdinalIgnoreCase);
                    if (levelBtns[l] != null)
                    {
                        var btnImg = levelBtns[l].GetComponent<Image>();
                        if (btnImg != null) btnImg.color = isCurrent ? activeColors[l] : inactiveColor;

                        var txt = levelBtns[l].GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null)
                        {
                            txt.text = isCurrent ? $"✓ {levels[l]}" : levels[l];
                            txt.color = isCurrent ? Color.white : new Color(0.75f, 0.78f, 0.85f);
                            txt.fontStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;
                        }
                    }
                }
                if (badgeText != null)
                {
                    badgeText.text = $"Mức độ: <b><color=#5DADE2>{selectedMucDo}</color></b>";
                }
            };

            float btnStartX = -285f;
            float btnSpacing = 135f;
            for (int l = 0; l < levels.Length; l++)
            {
                string lvl = levels[l];
                Vector2 btnPos = new Vector2(btnStartX + l * btnSpacing, yRow4);
                levelBtns[l] = CreateButton(cont, $"btnLevel_{l}", lvl, btnPos, new Vector2(125f, 32f), inactiveColor, () =>
                {
                    selectedMucDo = lvl;
                    updateLevelButtonsVisual();
                });
            }

            badgeText = CreateLabel(cont, "", 14, FontStyles.Normal, Color.white, new Vector2(340f, yRow4), new Vector2(250f, 25f), TextAlignmentOptions.MidlineLeft);
            updateLevelButtonsVisual();

            // =========================================================================
            // HÀNG 5: NỘI DUNG ĐỀ BÀI (NOIDUNG)
            // =========================================================================
            CreateLabel(cont, "Nội dung đề bài (*):", 15, FontStyles.Bold, new Color(1f, 0.9f, 0.45f), new Vector2(-480f, 118f), new Vector2(160f, 25f), TextAlignmentOptions.MidlineLeft);
            var noiDungInp = CreateInputField(cont, "inpNoiDung", "Nhập đề bài câu hỏi...", new Vector2(0f, 78f), new Vector2(1060f, 55f), TMP_InputField.ContentType.Standard, true);
            noiDungInp.text = q.questionText ?? "";

            // =========================================================================
            // HÀNG 6A: KHU VỰC TRẮC NGHIỆM (MCQ CONTAINER)
            // =========================================================================
            string aA = (q.answers != null && q.answers.Length > 0) ? q.answers[0] : "";
            string aB = (q.answers != null && q.answers.Length > 1) ? q.answers[1] : "";
            string aC = (q.answers != null && q.answers.Length > 2) ? q.answers[2] : "";
            string aD = (q.answers != null && q.answers.Length > 3) ? q.answers[3] : "";

            CreateLabel(mcqContainer.transform, "Đáp án A:", 14, FontStyles.Bold, Color.white, new Vector2(-480f, 25f), new Vector2(80f, 25f), TextAlignmentOptions.MidlineLeft);
            var inpA = CreateInputField(mcqContainer.transform, "inpA", aA, new Vector2(-240f, 25f), new Vector2(400f, 32f));
            inpA.text = aA;

            CreateLabel(mcqContainer.transform, "Đáp án B:", 14, FontStyles.Bold, Color.white, new Vector2(70f, 25f), new Vector2(80f, 25f), TextAlignmentOptions.MidlineLeft);
            var inpB = CreateInputField(mcqContainer.transform, "inpB", aB, new Vector2(310f, 25f), new Vector2(400f, 32f));
            inpB.text = aB;

            CreateLabel(mcqContainer.transform, "Đáp án C:", 14, FontStyles.Bold, Color.white, new Vector2(-480f, -15f), new Vector2(80f, 25f), TextAlignmentOptions.MidlineLeft);
            var inpC = CreateInputField(mcqContainer.transform, "inpC", aC, new Vector2(-240f, -15f), new Vector2(400f, 32f));
            inpC.text = aC;

            CreateLabel(mcqContainer.transform, "Đáp án D:", 14, FontStyles.Bold, Color.white, new Vector2(70f, -15f), new Vector2(80f, 25f), TextAlignmentOptions.MidlineLeft);
            var inpD = CreateInputField(mcqContainer.transform, "inpD", aD, new Vector2(310f, -15f), new Vector2(400f, 32f));
            inpD.text = aD;

            // Chọn đáp án đúng trắc nghiệm (A, B, C, D)
            CreateLabel(mcqContainer.transform, "Đáp án đúng (*):", 15, FontStyles.Bold, new Color(0.4f, 1f, 0.5f), new Vector2(-480f, -55f), new Vector2(140f, 25f), TextAlignmentOptions.MidlineLeft);
            int selectedCorrectOpt = Mathf.Clamp(q.correctAnswerIndex, 0, 3);
            string[] opts = { "A", "B", "C", "D" };
            Button[] optBtns = new Button[4];

            Action updateCorrectOptVisual = () =>
            {
                for (int o = 0; o < 4; o++)
                {
                    bool isPick = selectedCorrectOpt == o;
                    if (optBtns[o] != null)
                    {
                        var bImg = optBtns[o].GetComponent<Image>();
                        if (bImg != null) bImg.color = isPick ? new Color(0.18f, 0.72f, 0.38f) : new Color(0.24f, 0.27f, 0.35f);

                        var bTxt = optBtns[o].GetComponentInChildren<TextMeshProUGUI>();
                        if (bTxt != null)
                        {
                            bTxt.text = isPick ? $"✓ {opts[o]}" : opts[o];
                            bTxt.color = isPick ? Color.white : new Color(0.75f, 0.78f, 0.85f);
                            bTxt.fontStyle = isPick ? FontStyles.Bold : FontStyles.Normal;
                        }
                    }
                }
            };

            float optX = -320f;
            for (int o = 0; o < 4; o++)
            {
                int optIdx = o;
                optBtns[o] = CreateButton(mcqContainer.transform, $"btnOpt_{opts[o]}", opts[o], new Vector2(optX, -55f), new Vector2(65f, 32f), new Color(0.24f, 0.27f, 0.35f), () =>
                {
                    selectedCorrectOpt = optIdx;
                    updateCorrectOptVisual();
                });
                optX += 75f;
            }
            updateCorrectOptVisual();

            CreateLabel(mcqContainer.transform, "Lời giải thích:", 14, FontStyles.Bold, Color.white, new Vector2(-480f, -92f), new Vector2(200f, 20f), TextAlignmentOptions.MidlineLeft);
            var gtInp = CreateInputField(mcqContainer.transform, "inpGt", q.explanation ?? "", new Vector2(0f, -118f), new Vector2(1060f, 36f), TMP_InputField.ContentType.Standard, true);
            gtInp.text = q.explanation ?? "";

            CreateLabel(mcqContainer.transform, "Mẹo ghi nhớ nhanh:", 14, FontStyles.Bold, new Color(1f, 0.85f, 0.4f), new Vector2(-480f, -145f), new Vector2(240f, 20f), TextAlignmentOptions.MidlineLeft);
            var meoInp = CreateInputField(mcqContainer.transform, "inpMeo", q.meoGhiNho ?? "", new Vector2(0f, -170f), new Vector2(1060f, 32f));
            meoInp.text = q.meoGhiNho ?? "";

            // =========================================================================
            // HÀNG 6B: KHU VỰC TỰ LUẬN (ESSAY CONTAINER)
            // =========================================================================
            CreateLabel(essayContainer.transform, "📘 Đáp án tham chiếu / Lời giải mẫu (*):", 15, FontStyles.Bold, new Color(0.4f, 1f, 0.6f), new Vector2(-480f, 25f), new Vector2(400f, 25f), TextAlignmentOptions.MidlineLeft);
            var refAnsInp = CreateInputField(essayContainer.transform, "inpEssayRef", "Nhập lời giải mẫu từng bước để AI và giáo viên đối chiếu...", new Vector2(0f, -15f), new Vector2(1060f, 60f), TMP_InputField.ContentType.Standard, true);
            refAnsInp.text = !string.IsNullOrEmpty(q.dapAnThamChieu) ? q.dapAnThamChieu : (q.explanation ?? "");

            CreateLabel(essayContainer.transform, "📋 Tiêu chí chấm điểm từng bước (*):", 14, FontStyles.Bold, new Color(1f, 0.88f, 0.45f), new Vector2(-480f, -62f), new Vector2(450f, 22f), TextAlignmentOptions.MidlineLeft);
            var rubricInp = CreateInputField(essayContainer.transform, "inpEssayRubric", "Ví dụ: Bước 1: Công thức (0.5đ). Bước 2: Thay số (0.25đ). Bước 3: Đáp số & Đơn vị (0.25đ)...", new Vector2(0f, -98f), new Vector2(1060f, 50f), TMP_InputField.ContentType.Standard, true);
            rubricInp.text = q.tieuChiCham ?? "";

            CreateLabel(essayContainer.transform, "💡 Mẹo & Gợi ý làm bài:", 14, FontStyles.Bold, new Color(0.6f, 0.9f, 1f), new Vector2(-480f, -138f), new Vector2(300f, 20f), TextAlignmentOptions.MidlineLeft);
            var essayMeoInp = CreateInputField(essayContainer.transform, "inpEssayTip", q.meoGhiNho ?? "", new Vector2(0f, -165f), new Vector2(1060f, 32f));
            essayMeoInp.text = q.meoGhiNho ?? "";

            // Khởi tạo trạng thái hiển thị theo loại câu hỏi ban đầu
            updateQuestionTypeVisual();

            // =========================================================================
            // HÀNG 7: CÁC NÚT HÀNH ĐỘNG
            // =========================================================================
            // Nút 1: Lưu nội bộ Game
            CreateButton(cont, "btnSaveLocal", "💾 LƯU VÀO GAME", new Vector2(-190f, -240f), new Vector2(260f, 46f), new Color(0.2f, 0.45f, 0.7f), () =>
            {
                if (ExtractAndValidateForm(q, isEssay, idInp.text, selectedChuongId, selectedTenChuong, selectedBaiID, selectedTenBai, selectedMucDo, noiDungInp.text, inpA.text, inpB.text, inpC.text, inpD.text, selectedCorrectOpt, gtInp.text, meoInp.text, refAnsInp.text, rubricInp.text, essayMeoInp.text, gvInp.text))
                {
                    if (isNew) _questions.Add(q);
                    SaveQuestionsToLocalJson();
                    Destroy(form);
                    OpenQuestionManagement();
                    ShowToast($"✅ Đã lưu câu hỏi {q.questionID} ({q.loaiCauHoi} - {q.mucDo}) vào game thành công!");
                }
            });

            // Nút 2: Đẩy lên Google Sheet
            CreateButton(cont, "btnPushSheet", "🚀 ĐẨY LÊN GOOGLE SHEET", new Vector2(190f, -240f), new Vector2(300f, 46f), new Color(0.18f, 0.60f, 0.35f), () =>
            {
                if (ExtractAndValidateForm(q, isEssay, idInp.text, selectedChuongId, selectedTenChuong, selectedBaiID, selectedTenBai, selectedMucDo, noiDungInp.text, inpA.text, inpB.text, inpC.text, inpD.text, selectedCorrectOpt, gtInp.text, meoInp.text, refAnsInp.text, rubricInp.text, essayMeoInp.text, gvInp.text))
                {
                    if (isNew) _questions.Add(q);
                    SaveQuestionsToLocalJson();
                    Destroy(form);
                    OpenQuestionManagement();
                    PushQuestionToGoogleSheet(q);
                }
            });
        }

        private bool ExtractAndValidateForm(
            ScienceQuest.Quiz.QuestionData q,
            bool isEssay,
            string id,
            string chuongId,
            string tenChuong,
            string baiId,
            string tenBai,
            string mucDo,
            string noiDung,
            string a, string b, string c, string d,
            int correctOpt,
            string gt, string meo,
            string refAns, string rubric, string essayTip,
            string gv)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
            {
                ShowToast("⚠️ Đề bài câu hỏi không được để trống!");
                return false;
            }

            if (isEssay)
            {
                if (string.IsNullOrWhiteSpace(refAns))
                {
                    ShowToast("⚠️ Vui lòng nhập Đáp án tham chiếu / Lời giải mẫu cho câu hỏi Tự Luận!");
                    return false;
                }

                q.loaiCauHoi = "Tự luận";
                q.dapAnThamChieu = refAns.Trim();
                q.tieuChiCham = rubric.Trim();
                q.explanation = refAns.Trim();
                q.meoGhiNho = !string.IsNullOrWhiteSpace(essayTip) ? essayTip.Trim() : rubric.Trim();
                q.answers = new string[] { "Tự luận", "-", "-", "-" };
                q.correctAnswerIndex = 0;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
                {
                    ShowToast("⚠️ Câu hỏi trắc nghiệm cần nhập ít nhất đáp án A và B!");
                    return false;
                }

                q.loaiCauHoi = "Trắc nghiệm";
                q.answers = new string[] { a.Trim(), b.Trim(), c.Trim(), d.Trim() };
                q.correctAnswerIndex = correctOpt;
                q.explanation = gt.Trim();
                q.meoGhiNho = meo.Trim();
                q.dapAnThamChieu = "";
                q.tieuChiCham = "";
            }

            q.stt = q.stt > 0 ? q.stt : (_questions.Count + 1);
            q.questionID = !string.IsNullOrEmpty(id) ? id.Trim() : $"C9_{_questions.Count + 1:D3}";
            q.chuongId = chuongId;
            q.tenChuong = tenChuong;
            q.baiID = baiId;
            q.tenBai = tenBai;
            q.mucDo = !string.IsNullOrEmpty(mucDo) ? mucDo.Trim() : "Nhận biết";
            q.questionText = noiDung.Trim();
            q.giaoVienID = !string.IsNullOrEmpty(gv) ? gv.Trim() : _teacherId;
            q.trangThai = "Xuất bản";

            return true;
        }

        private string GetChuongIdFromBaiId(string baiID)
        {
            if (string.IsNullOrEmpty(baiID)) return "chuong1";
            string b = baiID.ToLower().Trim();
            if (b == "bai2" || b == "bai3" || b == "bai4" || b == "test_chuong1" || b.StartsWith("b9_001") || b.StartsWith("b9_002") || b.StartsWith("b9_003")) return "chuong1";
            if (b == "bai5" || b == "bai6" || b == "bai7" || b == "bai8" || b == "bai9" || b == "bai10" || b == "test_chuong2") return "chuong2";
            if (b == "bai11" || b == "bai12" || b == "bai13" || b == "test_chuong3") return "chuong3";
            if (b == "bai14" || b == "bai15" || b == "test_chuong4") return "chuong4";
            if (b == "bai16" || b == "bai17" || b == "test_chuong5") return "chuong5";
            return "tonghop";
        }

        private string GetTenChuongFromId(string chuongId)
        {
            if (string.IsNullOrEmpty(chuongId)) return "Chương I. Năng lượng cơ học";
            string c = chuongId.ToLower().Trim();
            if (c == "chuong1") return "Chương I. Năng lượng cơ học";
            if (c == "chuong2") return "Chương II. Ánh sáng";
            if (c == "chuong3") return "Chương III. Điện từ học";
            if (c == "chuong4") return "Chương IV. Điện thế & Dòng điện xoay chiều";
            if (c == "chuong5") return "Chương V. Năng lượng & Sự bảo toàn năng lượng";
            return "TỔNG ÔN CẢ QUYỂN (5 chương)";
        }

        private string GetTenBaiFromId(string baiID)
        {
            if (string.IsNullOrEmpty(baiID)) return "Bài 2. Động năng. Thế năng";
            string b = baiID.ToLower().Trim();
            switch (b)
            {
                case "bai2": return "Bài 2. Động năng. Thế năng";
                case "bai3": return "Bài 3. Cơ năng";
                case "bai4": return "Bài 4. Công và Công suất";
                case "test_chuong1": return "Đề TEST Chương 1 - Cơ học";
                case "bai5": return "Bài 5. Khúc xạ ánh sáng";
                case "bai6": return "Bài 6. Phản xạ toàn phần";
                case "bai7": return "Bài 7. Thấu kính hội tụ";
                case "bai8": return "Bài 8. Thấu kính phân kì";
                case "bai9": return "Bài 9. Sự tạo ảnh qua thấu kính";
                case "bai10": return "Bài 10. Kính lúp";
                case "test_chuong2": return "Đề TEST Chương 2 - Ánh sáng";
                case "bai11": return "Bài 11. Từ trường";
                case "bai12": return "Bài 12. Từ phổ - Đường sức từ";
                case "bai13": return "Bài 13. Lực điện từ";
                case "test_chuong3": return "Đề TEST Chương 3 - Điện từ học";
                case "bai14": return "Bài 14. Hiện tượng cảm ứng điện từ";
                case "bai15": return "Bài 15. Dòng điện xoay chiều";
                case "test_chuong4": return "Đề TEST Chương 4 - Dòng điện xoay chiều";
                case "bai16": return "Bài 16. Năng lượng và sự chuyển hóa năng lượng";
                case "bai17": return "Bài 17. Định luật bảo toàn và chuyển hóa năng lượng";
                case "test_chuong5": return "Đề TEST Chương 5 - Bảo toàn năng lượng";
                case "test_tonghop": return "Đề TEST tổng hợp toàn bộ Vật lý 9";
                default: return $"Bài học ({baiID})";
            }
        }

        private void SaveQuestionsToLocalJson()
        {
            try
            {
                var wrapper = new QuestionDataList { questions = _questions };
                string json = JsonUtility.ToJson(wrapper, true);

                // Lưu trực tiếp vào file json
                string path = Path.Combine(Application.dataPath, "Resources/QuizData/physics_questions.json");
                if (File.Exists(path))
                {
                    File.WriteAllText(path, json);
                }

                // Lưu vào PlayerPrefs để đảm bảo runtime luôn nạp được
                PlayerPrefs.SetString("Teacher_Custom_Questions_Json", json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeacherDashboard] Lỗi ghi json câu hỏi: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi yêu cầu HTTP POST đẩy câu hỏi lên Google Sheet (tab NganHangCauHoi)
        /// </summary>
        private void PushQuestionToGoogleSheet(ScienceQuest.Quiz.QuestionData q)
        {
            ShowToast($"🚀 Đang đẩy câu hỏi {q.questionID} lên Google Sheet...");

            if (GoogleSheetDataManager.Instance != null)
            {
                StartCoroutine(GoogleSheetDataManager.Instance.SubmitQuestionToSheet(q, (success, resp) =>
                {
                    if (success)
                    {
                        ShowToast($"✅ Đẩy thành công! Đã ghi nhận câu hỏi {q.questionID} trên Google Sheet!");
                    }
                    else
                    {
                        ShowToast($"⚠️ Lỗi mạng: {resp}. Câu hỏi đã được lưu an toàn trong game.");
                    }
                }));
            }
            else
            {
                ShowToast("⚠️ GoogleSheetDataManager chưa sẵn sàng!");
            }
        }

        #endregion

        #region ===== 7. MÀN HÌNH TIẾN ĐỘ HỌC TẬP & KẾT QUẢ HỌC SINH (PROGRESS MANAGEMENT) =====

        private void OpenProgressManagement()
        {
            CloseAllModals();

            _panelTienDoObj = CreateModalRoot("ModalTienDo", new Vector2(1200f, 720f));
            Transform container = _panelTienDoObj.transform.Find("Container");

            CreateModalHeader(container, "📊 TIẾN ĐỘ HỌC TẬP & KẾT QUẢ HỌC SINH TOÀN TRƯỜNG", CloseAllModals);

            // Nút đồng bộ từ Google Sheet
            CreateButton(container, "btnSyncProgress", "🔄 LÀM MỚI TỪ GOOGLE SHEET", new Vector2(-380f, 260f), new Vector2(260f, 42f), new Color(0.2f, 0.45f, 0.7f), () =>
            {
                ShowToast("🔄 Đang tải tiến độ học tập mới nhất từ Google Sheet...");
                if (ProgressSyncManager.Instance != null)
                {
                    StartCoroutine(ProgressSyncManager.Instance.FetchProgressFromSheet((success) =>
                    {
                        if (success)
                        {
                            LoadAllCentralData();
                            OpenProgressManagement();
                            ShowToast("✅ Đã cập nhật kết quả học tập mới nhất từ Google Sheet!");
                        }
                        else
                        {
                            ShowToast("⚠️ Không thể tải dữ liệu tiến độ. Kiểm tra kết nối mạng!");
                        }
                    }));
                }
            });

            // Tóm tắt thống kê lớp học
            int totalHs = _students.Count;
            float avgScore = totalHs > 0 ? (float)_students.Average(s => s.score) : 0f;
            float avgAcc = totalHs > 0 ? (float)_students.Average(s => s.accuracy) : 0f;

            CreateLabel(container, $"Sĩ số: {totalHs} học sinh  |  Điểm TB: {avgScore:F1}  |  Độ chính xác TB: {avgAcc:F1}%", 16, FontStyles.Bold, new Color(1f, 0.9f, 0.5f), new Vector2(200f, 260f), new Vector2(650f, 35f));

            // Bảng danh sách học sinh
            // Header cột
            GameObject thRow = new GameObject("TableHeader");
            thRow.transform.SetParent(container, false);
            RectTransform thrt = thRow.AddComponent<RectTransform>();
            thrt.anchoredPosition = new Vector2(0f, 215f);
            thrt.sizeDelta = new Vector2(1140f, 35f);
            Image thbg = thRow.AddComponent<Image>();
            thbg.color = new Color(0.12f, 0.14f, 0.20f, 0.95f);

            CreateLabel(thRow.transform, "HẠNG", 14, FontStyles.Bold, Color.white, new Vector2(-510f, 0f), new Vector2(60f, 30f));
            CreateLabel(thRow.transform, "HỌ TÊN HỌC SINH", 14, FontStyles.Bold, Color.white, new Vector2(-330f, 0f), new Vector2(240f, 30f), TextAlignmentOptions.MidlineLeft);
            CreateLabel(thRow.transform, "LỚP", 14, FontStyles.Bold, Color.white, new Vector2(-150f, 0f), new Vector2(90f, 30f));
            CreateLabel(thRow.transform, "BÀI ĐÃ LÀM", 14, FontStyles.Bold, Color.white, new Vector2(40f, 0f), new Vector2(240f, 30f), TextAlignmentOptions.MidlineLeft);
            CreateLabel(thRow.transform, "ĐIỂM", 14, FontStyles.Bold, Color.white, new Vector2(230f, 0f), new Vector2(80f, 30f));
            CreateLabel(thRow.transform, "TỶ LỆ", 14, FontStyles.Bold, Color.white, new Vector2(330f, 0f), new Vector2(90f, 30f));
            CreateLabel(thRow.transform, "THỜI GIAN", 14, FontStyles.Bold, Color.white, new Vector2(430f, 0f), new Vector2(90f, 30f));
            CreateLabel(thRow.transform, "NGÀY LÀM", 14, FontStyles.Bold, Color.white, new Vector2(515f, 0f), new Vector2(90f, 30f));

            // Danh sách cuộn học sinh
            var scrollObj = CreateScrollView(container, "ScrollStudents", new Vector2(0f, -40f), new Vector2(1140f, 460f));
            Transform content = scrollObj.transform.Find("Viewport/Content");

            float sHeight = 55f;
            for (int i = 0; i < _students.Count; i++)
            {
                var st = _students[i];
                GameObject sRow = new GameObject($"StudentRow_{i}");
                sRow.transform.SetParent(content, false);
                RectTransform srt = sRow.AddComponent<RectTransform>();
                srt.sizeDelta = new Vector2(1110f, sHeight - 6f);

                Image sbg = sRow.AddComponent<Image>();
                sbg.color = (i % 2 == 0) ? new Color(0.16f, 0.17f, 0.25f, 0.90f) : new Color(0.20f, 0.21f, 0.30f, 0.90f);

                // Hạng
                string rankStr = (i + 1).ToString();
                Color rkCol = Color.white;
                if (i == 0) { rankStr = "🥇 1"; rkCol = new Color(1f, 0.85f, 0.2f); }
                else if (i == 1) { rankStr = "🥈 2"; rkCol = new Color(0.85f, 0.9f, 0.95f); }
                else if (i == 2) { rankStr = "🥉 3"; rkCol = new Color(0.9f, 0.65f, 0.4f); }
                CreateLabel(sRow.transform, rankStr, 15, FontStyles.Bold, rkCol, new Vector2(-495f, 0f), new Vector2(70f, 30f));

                // Tên học sinh
                CreateLabel(sRow.transform, st.playerName, 15, FontStyles.Bold, Color.white, new Vector2(-315f, 0f), new Vector2(240f, 30f), TextAlignmentOptions.MidlineLeft);

                // Lớp
                CreateLabel(sRow.transform, st.className, 14, FontStyles.Normal, new Color(0.85f, 0.85f, 0.9f), new Vector2(-135f, 0f), new Vector2(90f, 30f));

                // Bài đã làm
                string lesName = st.lessonName;
                if (!string.IsNullOrEmpty(lesName) && lesName.Length > 24) lesName = lesName.Substring(0, 24) + "...";
                CreateLabel(sRow.transform, lesName, 14, FontStyles.Italic, new Color(1f, 0.92f, 0.7f), new Vector2(55f, 0f), new Vector2(240f, 30f), TextAlignmentOptions.MidlineLeft);

                // Điểm
                Color scoreCol = st.score >= 90 ? new Color(0.3f, 1f, 0.5f) : (st.score >= 70 ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.5f, 0.5f));
                CreateLabel(sRow.transform, $"{st.score}đ", 16, FontStyles.Bold, scoreCol, new Vector2(245f, 0f), new Vector2(70f, 30f));

                // Tỷ lệ
                CreateLabel(sRow.transform, $"{st.accuracy:F0}%", 14, FontStyles.Bold, Color.white, new Vector2(345f, 0f), new Vector2(80f, 30f));

                // Thời gian
                int m = (int)(st.timeUsed / 60);
                int s = (int)(st.timeUsed % 60);
                CreateLabel(sRow.transform, $"{m}p {s}s", 14, FontStyles.Normal, new Color(0.8f, 0.9f, 1f), new Vector2(445f, 0f), new Vector2(80f, 30f));

                // Ngày làm
                CreateLabel(sRow.transform, st.date, 13, FontStyles.Normal, new Color(0.7f, 0.7f, 0.8f), new Vector2(530f, 0f), new Vector2(90f, 30f));
            }
        }

        #endregion

        #region ===== 8. TIỆN ÍCH DỰNG GIAO DIỆN RUNTIME (UI FACTORY) =====

        private void CloseAllModals()
        {
            if (_panelChuongObj != null) Destroy(_panelChuongObj);
            if (_panelBaiObj != null) Destroy(_panelBaiObj);
            if (_panelCauHoiObj != null) Destroy(_panelCauHoiObj);
            if (_panelTienDoObj != null) Destroy(_panelTienDoObj);
        }

        private GameObject CreateModalRoot(string name, Vector2 size)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(_mainCanvas.transform, false);

            RectTransform rrt = root.AddComponent<RectTransform>();
            rrt.anchorMin = Vector2.zero;
            rrt.anchorMax = Vector2.one;
            rrt.offsetMin = Vector2.zero;
            rrt.offsetMax = Vector2.zero;

            // Nền mờ tối phía sau (Backdrop)
            Image backdrop = root.AddComponent<Image>();
            backdrop.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);
            backdrop.raycastTarget = true;

            // Khung chứa nội dung chính
            GameObject container = new GameObject("Container");
            container.transform.SetParent(root.transform, false);
            RectTransform crt = container.AddComponent<RectTransform>();
            crt.sizeDelta = size;

            Image cbg = container.AddComponent<Image>();
            cbg.color = new Color(0.09f, 0.10f, 0.15f, 0.98f);

            // Viền vàng kim mỏng
            Outline outline = container.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.72f, 0.35f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            return root;
        }

        private void CreateModalHeader(Transform parent, string titleText, Action onClose)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            RectTransform hrt = headerObj.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.sizeDelta = new Vector2(0f, 60f);

            Image hbg = headerObj.AddComponent<Image>();
            hbg.color = new Color(0.14f, 0.16f, 0.24f, 0.95f);

            // Tiêu đề
            CreateLabel(headerObj.transform, titleText, 20, FontStyles.Bold, new Color(1f, 0.92f, 0.5f), new Vector2(-30f, 0f), new Vector2(750f, 40f), TextAlignmentOptions.MidlineLeft);

            // Nút Đóng [X]
            CreateButton(headerObj.transform, "btnClose", "✕", new Vector2(parent.GetComponent<RectTransform>().rect.width * 0.5f - 40f, 0f), new Vector2(44f, 44f), new Color(0.7f, 0.2f, 0.2f), onClose);
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, FontStyles style, Color color, Vector2 pos, Vector2 size, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            GameObject go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color bgColor, Action onClick)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;

            Button btn = go.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            RectTransform trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        private TMP_InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 pos, Vector2 size, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, bool multiLine = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.20f, 0.28f, 0.95f);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            RectTransform trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(12f, 6f);
            trt.offsetMax = new Vector2(-12f, -6f);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 15;
            tmp.color = Color.white;
            tmp.alignment = multiLine ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;

            TMP_InputField input = go.AddComponent<TMP_InputField>();
            input.textComponent = tmp;
            input.contentType = contentType;
            if (multiLine) input.lineType = TMP_InputField.LineType.MultiLineNewline;

            return input;
        }

        private GameObject CreateScrollView(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject scrollRoot = new GameObject(name);
            scrollRoot.transform.SetParent(parent, false);
            RectTransform srt = scrollRoot.AddComponent<RectTransform>();
            srt.anchoredPosition = pos;
            srt.sizeDelta = size;

            ScrollRect sr = scrollRoot.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollRoot.transform, false);
            RectTransform vrt = viewport.AddComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = vrt;
            sr.content = crt;

            return scrollRoot;
        }

        private void ShowToast(string message)
        {
            if (_mainCanvas == null) return;

            GameObject toast = new GameObject("ToastTeacher");
            toast.transform.SetParent(_mainCanvas.transform, false);
            RectTransform trt = toast.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(0f, -220f);
            trt.sizeDelta = new Vector2(850f, 65f);

            Image tbg = toast.AddComponent<Image>();
            tbg.color = new Color(0.10f, 0.12f, 0.16f, 0.96f);

            Outline to = toast.AddComponent<Outline>();
            to.effectColor = new Color(1f, 0.85f, 0.35f, 0.8f);
            to.effectDistance = new Vector2(2f, -2f);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(toast.transform, false);
            RectTransform textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20f, 10f);
            textRt.offsetMax = new Vector2(-20f, -10f);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.fontSize = 17;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.95f, 0.7f);
            tmp.alignment = TextAlignmentOptions.Center;

            StartCoroutine(AnimateToastFade(toast));
        }

        private IEnumerator AnimateToastFade(GameObject toast)
        {
            yield return new WaitForSeconds(3.5f);
            if (toast != null) Destroy(toast);
        }

        #endregion
    }
}
