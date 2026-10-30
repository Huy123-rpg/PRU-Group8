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
using ScienceQuest.Teacher;

namespace ScienceQuest.PhysicsGame1
{
    using QuestionData = ScienceQuest.Quiz.QuestionData;

    /// <summary>
    /// Controller chính cho Màn hình Quản Lý Của Giáo Viên (16_DashboardTeacher.unity).
    /// Đáp ứng 100% các yêu cầu từ Màn 1 đến Màn 8:
    /// - Màn 1: Dashboard Giáo viên (4 nút chức năng, Đăng xuất, Phân quyền)
    /// - Màn 2: Quản lý danh sách chương (3 môn, Ẩn/Hiện, Sửa, Thêm)
    /// - Màn 3: Popup Thêm/Sửa chương (Chọn môn, Tên, Mô tả, Lưu, Chặn click xuyên)
    /// - Màn 4: Quản lý bài học & nội dung lý thuyết (3 môn, Chọn chương động, Sửa, Thêm bài)
    /// - Màn 5: Chỉnh sửa bài học (3 trang lý thuyết: Khái niệm, Công thức, Ứng dụng thực tế)
    /// - Màn 6: Quản lý ngân hàng câu hỏi & đồng bộ Google Sheets (3 môn, Sửa, Đẩy Sheet, Tải Sheet)
    /// - Màn 7: Popup Thêm/Sửa câu hỏi mới (Trắc nghiệm 4 đáp án vs Tự luận rubric, 4 mức độ, Lưu Game, Đẩy Sheet)
    /// - Màn 8: Tiến độ học tập học sinh (3 môn, Hạng, Điểm, Tỷ lệ, Thời gian, Làm mới, Lịch sử chi tiết)
    /// TẬN DỤNG TOÀN BỘ CÁC GAMEOBJECT CÓ SẴN TRÊN CANVAS, KHÔNG ĐÈ UI MỚI, LƯU DỮ LIỆU PERSISTENT.
    /// </summary>
    public class PhysicsGame1_TeacherDashboardController : MonoBehaviour
    {
        [Header("Trạng thái phân quyền")]
        private bool _isAuthorized = false;
        private string _teacherName = "Thầy Quản Trị";
        private string _teacherId = "GV001";

        // Môn học & Chương đang được chọn trong toàn bộ Dashboard
        private TeacherSubject _currentSubject = TeacherSubject.Physics;
        private string _selectedChapterId = "chuong1";

        // Canvas & Các GameObject gốc trên Canvas của Scene 16_DashboardTeacher
        private Canvas _canvas;
        private GameObject _bgObj;
        private GameObject _titleObj;
        private GameObject _cardChuongObj;
        private GameObject _cardBaiObj;
        private GameObject _cardCauHoiObj;
        private GameObject _cardTienDoObj;
        private GameObject _logoutBtnObj;

        // 4 Panel chức năng chính
        private GameObject _panelChuong;
        private GameObject _panelBaiHoc;
        private GameObject _panelCauHoi;
        private GameObject _panelTienDo;

        // 3 Popup con
        private GameObject _popupThemChuong;
        private GameObject _popupSuaBaiHoc;
        private GameObject _popupThemCauHoi;

        // Templates hàng và nút có sẵn từ scene
        private GameObject _chapterRowTemplate;
        private GameObject _studentRowTemplate;
        private Sprite _crownSprite;
        private Sprite _normalRowSprite;
        private GameObject _chapterTabTemplate;
        private GameObject _btnSuaBaiHocTemplate;
        private GameObject _btnSuaCauHoiTemplate;
        private GameObject _btnPushCauHoiTemplate;
        private GameObject _chapterTabsScrollContainer;

        // Toast thông báo
        private GameObject _toastObj;
        private Coroutine _toastCoroutine;

        private void Start()
        {
            Debug.Log("[TeacherDashboard] 🏫 Khởi tạo Màn hình Quản Trị Giáo Viên (16_DashboardTeacher)");

            // Kiểm tra phân quyền đăng nhập
            bool isLoggedIn = LoginManager.IsLoggedIn || (GameSession.Instance != null && GameSession.Instance.activeUser != null && GameSession.Instance.activeUser.role == UserRole.Teacher);
            if (!isLoggedIn)
            {
#if !UNITY_EDITOR
                Debug.LogWarning("[TeacherDashboard] ⚠️ Chưa đăng nhập! Chuyển hướng về Scene1_Login...");
                SceneManager.LoadScene("Scene1_Login");
                return;
#endif
            }

            // 1. Khởi tạo & nạp dữ liệu tập trung (Chapters, Lessons, Questions, Student Progress)
            if (TeacherDataManager.Instance != null)
            {
                TeacherDataManager.Instance.InitializeData();
            }

            // 2. Tìm kiếm và liên kết các GameObject có sẵn trên Canvas
            FindSceneGameObjects();

            // 3. Thiết lập sự kiện Dashboard (Màn 1)
            SetupDashboardEvents();

            // 4. Thiết lập sự kiện cho 4 Panel chính & các Popup con
            SetupChapterManagementEvents();
            SetupLessonManagementEvents();
            SetupQuestionManagementEvents();
            SetupProgressManagementEvents();

            // 5. Kiểm tra phân quyền truy cập
            CheckTeacherAuthorization();

            // 6. Ban đầu chỉ hiển thị Dashboard, ẩn tất cả panel con
            ShowOnlyDashboard();
        }

        #region ===== TÌM KIẾM GAMEOBJECTS TRONG SCENE =====

        private void FindSceneGameObjects()
        {
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("[TeacherDashboard] ❌ Không tìm thấy Canvas trong Scene!");
                return;
            }

            // Dashboard Màn 1
            _bgObj = _canvas.transform.Find("Image")?.gameObject ?? GameObject.Find("bg");
            _titleObj = _canvas.transform.Find("title")?.gameObject;
            _cardChuongObj = _canvas.transform.Find("quanlychuong")?.gameObject;
            _cardBaiObj = _canvas.transform.Find("quanlybai")?.gameObject;
            _cardCauHoiObj = _canvas.transform.Find("quanlycauhoi")?.gameObject;
            _cardTienDoObj = _canvas.transform.Find("tiendohoctap")?.gameObject;
            _logoutBtnObj = _canvas.transform.Find("logout")?.gameObject;

            // 4 Panel chính
            _panelChuong = _canvas.transform.Find("Panelqlychuong")?.gameObject;
            _panelBaiHoc = _canvas.transform.Find("Panelqlybaihoc")?.gameObject;
            _panelCauHoi = _canvas.transform.Find("Panelqlycauhoi")?.gameObject;
            _panelTienDo = _canvas.transform.Find("Panelqlytiendo")?.gameObject;

            // 3 Popup con
            if (_panelChuong != null)
            {
                _popupThemChuong = _panelChuong.transform.Find("Panelthemmoichuong")?.gameObject;
                EnsureRaycastBlocker(_popupThemChuong);
            }

            if (_panelBaiHoc != null)
            {
                _popupSuaBaiHoc = _panelBaiHoc.transform.Find("Panelsuanoidung")?.gameObject;
                EnsureRaycastBlocker(_popupSuaBaiHoc);
            }

            if (_panelCauHoi != null)
            {
                _popupThemCauHoi = _panelCauHoi.transform.Find("Panelthemcauhoimoi")?.gameObject;
                EnsureRaycastBlocker(_popupThemCauHoi);
            }

            // Hàng mẫu danh sách chương
            if (_panelChuong != null)
            {
                _chapterRowTemplate = _panelChuong.transform.Find("Scroll View/Viewport/Content/hang1")?.gameObject;
            }

            // Hàng mẫu danh sách học sinh
            if (_panelTienDo != null)
            {
                var h1 = _panelTienDo.transform.Find("Scroll View/Viewport/Content/hang1")?.gameObject;
                var h2 = _panelTienDo.transform.Find("Scroll View/Viewport/Content/hang2")?.gameObject;
                if (h1 != null)
                {
                    var img1 = h1.GetComponent<Image>();
                    if (img1 != null) _crownSprite = img1.sprite;
                }
                if (h2 != null)
                {
                    var img2 = h2.GetComponent<Image>();
                    if (img2 != null) _normalRowSprite = img2.sprite;
                }
                _studentRowTemplate = h2 ?? h1;
            }

            // Nút chọn chương mẫu trong bài học
            if (_panelBaiHoc != null)
            {
                _chapterTabTemplate = _panelBaiHoc.transform.Find("titlechonchuong/chương")?.gameObject;
                _btnSuaBaiHocTemplate = FindTemplateButton(_panelBaiHoc.transform, "sửa");
                if (_btnSuaBaiHocTemplate != null) _btnSuaBaiHocTemplate.SetActive(false);
            }

            if (_panelCauHoi != null)
            {
                _btnSuaCauHoiTemplate = FindTemplateButton(_panelCauHoi.transform, "sửa");
                _btnPushCauHoiTemplate = FindTemplateButton(_panelCauHoi.transform, "tải lên");
                if (_btnSuaCauHoiTemplate != null) _btnSuaCauHoiTemplate.SetActive(false);
                if (_btnPushCauHoiTemplate != null) _btnPushCauHoiTemplate.SetActive(false);
            }

            HideTemplateButtons();
        }

        private GameObject FindTemplateButton(Transform parent, string targetName)
        {
            if (parent == null) return null;
            string cleanTarget = CleanString(targetName);
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith("Panel", StringComparison.OrdinalIgnoreCase)) continue;
                string cClean = CleanString(child.name);
                if (cClean == cleanTarget || child.name.Trim().Equals(targetName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        private void HideTemplateButtons()
        {
            if (_btnSuaBaiHocTemplate != null) _btnSuaBaiHocTemplate.SetActive(false);
            if (_btnSuaCauHoiTemplate != null) _btnSuaCauHoiTemplate.SetActive(false);
            if (_btnPushCauHoiTemplate != null) _btnPushCauHoiTemplate.SetActive(false);

            if (_panelBaiHoc != null)
            {
                for (int i = 0; i < _panelBaiHoc.transform.childCount; i++)
                {
                    Transform c = _panelBaiHoc.transform.GetChild(i);
                    if (c.name.StartsWith("Panel", StringComparison.OrdinalIgnoreCase)) continue;
                    string cClean = CleanString(c.name);
                    if (cClean == "sua" || c.name.Trim().Equals("sửa", StringComparison.OrdinalIgnoreCase))
                    {
                        c.gameObject.SetActive(false);
                    }
                }
            }

            if (_panelCauHoi != null)
            {
                for (int i = 0; i < _panelCauHoi.transform.childCount; i++)
                {
                    Transform c = _panelCauHoi.transform.GetChild(i);
                    if (c.name.StartsWith("Panel", StringComparison.OrdinalIgnoreCase)) continue;
                    string cClean = CleanString(c.name);
                    if (cClean == "sua" || cClean == "tailen" || cClean == "daysheet" ||
                        c.name.Trim().Equals("sửa", StringComparison.OrdinalIgnoreCase) ||
                        c.name.Trim().Equals("tải lên", StringComparison.OrdinalIgnoreCase))
                    {
                        c.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Đảm bảo popup có lớp chắn raycast ngăn click xuyên qua panel phía sau
        /// </summary>
        private void EnsureRaycastBlocker(GameObject popup)
        {
            if (popup == null) return;
            Image img = popup.GetComponent<Image>();
            if (img == null) img = popup.AddComponent<Image>();
            img.color = new Color(0.02f, 0.04f, 0.08f, 0.85f);
            img.raycastTarget = true;

            // Đưa popup phủ toàn màn hình Canvas để chặn click toàn diện
            RectTransform rt = popup.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            ConfigureCaretForPopupInputs(popup);
        }

        private void ConfigureCaretForPopupInputs(GameObject popup)
        {
            if (popup == null) return;
            var inputs = popup.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var inp in inputs)
            {
                if (inp.textViewport == null)
                {
                    if (inp.textComponent != null) inp.textViewport = inp.textComponent.rectTransform;
                    else inp.textViewport = inp.GetComponent<RectTransform>();
                }
                inp.caretBlinkRate = 0.85f;
                inp.caretWidth = 3;
                inp.customCaretColor = true;
                inp.caretColor = new Color(1f, 0.88f, 0.25f, 1f);
                inp.selectionColor = new Color(0.2f, 0.5f, 0.9f, 0.45f);
            }
        }

        #endregion

        #region ===== MÀN 1: BẢNG ĐIỀU KHIỂN (DASHBOARD GIÁO VIÊN) =====

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
            else
            {
                string prefRole = PlayerPrefs.GetString("Current_UserRole", "");
                string prefUser = PlayerPrefs.GetString("Current_Username", "");
                if (prefRole == "Teacher" || prefUser.ToLower().StartsWith("gv") || prefUser.ToLower().Contains("admin"))
                {
                    hasTeacherRole = true;
                    _teacherName = !string.IsNullOrEmpty(prefUser) ? prefUser : "Thầy Quản Trị";
                    _teacherId = _teacherName.ToUpper();
                }
            }

            _isAuthorized = true; // Cho phép giáo viên thao tác quản trị
            UpdateDashboardTeacherBadge();
            ShowToast($"👨‍🏫 Xin chào {_teacherName}! Chúc Thầy/Cô một ngày giảng dạy hiệu quả.");
        }

        private void UpdateDashboardTeacherBadge()
        {
            // Cập nhật text tiêu đề hoặc huy hiệu giáo viên góc phải nếu có
            if (_titleObj != null)
            {
                var tmp = _titleObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "BẢNG ĐIỀU KHIỂN GIÁO VIÊN";
                }
            }
        }

        private void SetupDashboardEvents()
        {
            // 1. Quản lý Chương
            SetupButton(_cardChuongObj, () =>
            {
                StartCoroutine(PunchAnimation(_cardChuongObj, () => OpenChapterManagement(TeacherSubject.Physics)));
            });

            // 2. Quản lý Bài học
            SetupButton(_cardBaiObj, () =>
            {
                StartCoroutine(PunchAnimation(_cardBaiObj, () => OpenLessonManagement(TeacherSubject.Physics)));
            });

            // 3. Quản lý Câu hỏi
            SetupButton(_cardCauHoiObj, () =>
            {
                StartCoroutine(PunchAnimation(_cardCauHoiObj, () => OpenQuestionManagement(TeacherSubject.Physics)));
            });

            // 4. Tiến độ học tập
            SetupButton(_cardTienDoObj, () =>
            {
                StartCoroutine(PunchAnimation(_cardTienDoObj, () => OpenProgressManagement(TeacherSubject.Physics)));
            });

            // 5. Đăng xuất
            SetupButton(_logoutBtnObj, OnLogoutClicked);
        }

        private void ShowOnlyDashboard()
        {
            SetDashboardCardsActive(true);

            if (_panelChuong != null) _panelChuong.SetActive(false);
            if (_panelBaiHoc != null) _panelBaiHoc.SetActive(false);
            if (_panelCauHoi != null) _panelCauHoi.SetActive(false);
            if (_panelTienDo != null) _panelTienDo.SetActive(false);

            if (_popupThemChuong != null) _popupThemChuong.SetActive(false);
            if (_popupSuaBaiHoc != null) _popupSuaBaiHoc.SetActive(false);
            if (_popupThemCauHoi != null) _popupThemCauHoi.SetActive(false);

            HideTemplateButtons();
        }

        private void SetDashboardCardsActive(bool active)
        {
            if (_cardChuongObj != null) _cardChuongObj.SetActive(active);
            if (_cardBaiObj != null) _cardBaiObj.SetActive(active);
            if (_cardCauHoiObj != null) _cardCauHoiObj.SetActive(active);
            if (_cardTienDoObj != null) _cardTienDoObj.SetActive(active);
            if (_logoutBtnObj != null) _logoutBtnObj.SetActive(active);
            if (_titleObj != null) _titleObj.SetActive(active);
        }

        private void OnLogoutClicked()
        {
            Debug.Log("[TeacherDashboard] 🚪 Giáo viên nhấn Đăng xuất");
            LoginManager.IsLoggedIn = false;
            if (GameSession.Instance != null)
            {
                GameSession.Instance.ResetSession();
            }

            PlayerPrefs.DeleteKey("Current_Username");
            PlayerPrefs.DeleteKey("Current_UserRole");
            PlayerPrefs.Save();

            ShowToast("Đã kết thúc phiên đăng nhập Giáo viên. Đang trở về màn đăng nhập...");

            // Chuyển về Scene1_Login hoặc 01_MainMenu
            if (Application.CanStreamedLevelBeLoaded("Scene1_Login"))
            {
                SceneManager.LoadScene("Scene1_Login");
            }
            else if (Application.CanStreamedLevelBeLoaded("01_MainMenu"))
            {
                SceneManager.LoadScene("01_MainMenu");
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        #endregion

        #region ===== MÀN 2 & MÀN 3: QUẢN LÝ DANH SÁCH CHƯƠNG & POPUP THÊM/SỬA CHƯƠNG =====

        private ChapterItem _editingChapter = null;

        private void SetupChapterManagementEvents()
        {
            if (_panelChuong == null) return;

            // 3 nút chọn môn bên trái
            SetupButton(FindChildFuzzy(_panelChuong.transform, "vatly", "vat ly"), () => SwitchChapterSubject(TeacherSubject.Physics));
            SetupButton(FindChildFuzzy(_panelChuong.transform, "sinhhoc", "sinh"), () => SwitchChapterSubject(TeacherSubject.Biology));
            SetupButton(FindChildFuzzy(_panelChuong.transform, "hoahoc", "hoa"), () => SwitchChapterSubject(TeacherSubject.Chemistry));

            // Nút Thêm mới chương
            SetupButton(FindChildFuzzy(_panelChuong.transform, "thêm chương", "them chuong", "them"), () => OpenAddChapterPopup());

            // Nút X đóng panel về Dashboard
            SetupButton(FindChildFuzzy(_panelChuong.transform, "thoát", "thoat"), () => ShowOnlyDashboard());

            // Setup sự kiện cho Popup Thêm/Sửa chương (Màn 3)
            if (_popupThemChuong != null)
            {
                SetupButton(FindChildFuzzy(_popupThemChuong.transform, "vatly", "vat ly"), () => SelectPopupChapterSubject(TeacherSubject.Physics));
                SetupButton(FindChildFuzzy(_popupThemChuong.transform, "sinh", "sinhhoc"), () => SelectPopupChapterSubject(TeacherSubject.Biology));
                SetupButton(FindChildFuzzy(_popupThemChuong.transform, "hóa", "hoahoc", "hoa"), () => SelectPopupChapterSubject(TeacherSubject.Chemistry));

                // Nút Lưu chương
                SetupButton(FindChildFuzzy(_popupThemChuong.transform, "thêm", "them", "luu"), OnSaveChapterClicked);

                // Nút X đóng popup
                SetupButton(FindChildFuzzy(_popupThemChuong.transform, "thoat", "thoát"), () =>
                {
                    _popupThemChuong.SetActive(false);
                });
            }
        }

        public void OpenChapterManagement(TeacherSubject subject)
        {
            _currentSubject = subject;
            SetDashboardCardsActive(false);

            if (_panelBaiHoc != null) _panelBaiHoc.SetActive(false);
            if (_panelCauHoi != null) _panelCauHoi.SetActive(false);
            if (_panelTienDo != null) _panelTienDo.SetActive(false);

            if (_panelChuong != null)
            {
                _panelChuong.SetActive(true);
                if (_popupThemChuong != null) _popupThemChuong.SetActive(false);
                RefreshChapterList();
            }
        }

        private void SwitchChapterSubject(TeacherSubject sub)
        {
            _currentSubject = sub;
            HighlightSubjectButtons(_panelChuong, sub);
            RefreshChapterList();
            ShowToast($"Đang xem danh sách chương môn {TeacherDataManager.SubjectToVietnamese(sub)}");
        }

        private void RefreshChapterList()
        {
            if (_panelChuong == null) return;
            HighlightSubjectButtons(_panelChuong, _currentSubject);

            Transform content = _panelChuong.transform.Find("Scroll View/Viewport/Content");
            if (content == null || _chapterRowTemplate == null) return;

            // Ẩn các hàng thừa ban đầu
            for (int i = 0; i < content.childCount; i++)
            {
                content.GetChild(i).gameObject.SetActive(false);
            }

            var chapters = TeacherDataManager.Instance.GetChapters(_currentSubject);

            for (int i = 0; i < chapters.Count; i++)
            {
                var ch = chapters[i];
                int index = i;

                GameObject rowGo;
                if (index < content.childCount)
                {
                    rowGo = content.GetChild(index).gameObject;
                }
                else
                {
                    rowGo = Instantiate(_chapterRowTemplate, content);
                }

                rowGo.name = $"ChapterRow_{index + 1}";
                rowGo.SetActive(true);

                // Tìm kiếm tất cả các con linh hoạt chống lệch tên/dấu cách trong scene
                GameObject applyObj = null;
                GameObject khongApplyObj = null;
                GameObject btnAn = null;
                GameObject btnHien = null;
                GameObject btnSua = null;
                TextMeshProUGUI sttTmp = null;
                TextMeshProUGUI nameTmp = null;

                for (int c = 0; c < rowGo.transform.childCount; c++)
                {
                    Transform child = rowGo.transform.GetChild(c);
                    string cClean = CleanString(child.name);
                    if (cClean == "apply") applyObj = child.gameObject;
                    else if (cClean.Contains("khongapply")) khongApplyObj = child.gameObject;
                    else if (cClean == "an" || child.name.Trim().Equals("ẩn", StringComparison.OrdinalIgnoreCase)) btnAn = child.gameObject;
                    else if (cClean == "hien" || child.name.Trim().Equals("hiện", StringComparison.OrdinalIgnoreCase)) btnHien = child.gameObject;
                    else if (cClean == "sua" || child.name.Trim().Equals("sửa", StringComparison.OrdinalIgnoreCase)) btnSua = child.gameObject;
                    else if (cClean == "stt") sttTmp = child.GetComponent<TextMeshProUGUI>() ?? child.GetComponentInChildren<TextMeshProUGUI>(true);
                    else if (cClean.Contains("tenchuong")) nameTmp = child.GetComponent<TextMeshProUGUI>() ?? child.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                // Điền STT
                if (sttTmp != null) sttTmp.text = (index + 1).ToString();

                // Điền Tên chương & Tự động co chữ vừa vặn khung
                if (nameTmp != null)
                {
                    nameTmp.text = ch.name;
                    nameTmp.enableAutoSizing = true;
                    nameTmp.fontSizeMin = 12f;
                    nameTmp.fontSizeMax = 17f;
                    nameTmp.lineSpacing = -8f;
                    nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
                }

                // Trạng thái badge: apply vs khongapply
                if (applyObj != null)
                {
                    applyObj.SetActive(ch.isActive);
                    SetupButton(applyObj, () =>
                    {
                        TeacherDataManager.Instance.ToggleChapterActive(ch.id);
                        RefreshChapterList();
                        ShowToast($"Đã chuyển '{ch.name}' thành: ⚪ Tạm dừng áp dụng");
                    });
                }

                if (khongApplyObj != null)
                {
                    khongApplyObj.SetActive(!ch.isActive);
                    SetupButton(khongApplyObj, () =>
                    {
                        TeacherDataManager.Instance.ToggleChapterActive(ch.id);
                        RefreshChapterList();
                        ShowToast($"Đã chuyển '{ch.name}' thành: 🟢 Đang áp dụng");
                    });
                }

                // Nút Ẩn / Hiện
                if (btnAn != null)
                {
                    btnAn.SetActive(ch.isActive);
                    SetupButton(btnAn, () =>
                    {
                        TeacherDataManager.Instance.ToggleChapterActive(ch.id);
                        RefreshChapterList();
                        ShowToast($"Đã chuyển '{ch.name}' thành: ⚪ Tạm dừng áp dụng");
                    });
                }

                if (btnHien != null)
                {
                    btnHien.SetActive(!ch.isActive);
                    SetupButton(btnHien, () =>
                    {
                        TeacherDataManager.Instance.ToggleChapterActive(ch.id);
                        RefreshChapterList();
                        ShowToast($"Đã chuyển '{ch.name}' thành: 🟢 Đang áp dụng");
                    });
                }

                // Nút Sửa
                if (btnSua != null)
                {
                    SetupButton(btnSua, () => OpenEditChapterPopup(ch));
                }
            }

            // Kích hoạt Layout Group tự động co dãn
            EnsureVerticalLayout(content.gameObject, 8f);
        }

        private void OpenAddChapterPopup()
        {
            if (_popupThemChuong == null) return;
            _editingChapter = null;

            _popupThemChuong.SetActive(true);
            _popupThemChuong.transform.SetAsLastSibling();

            // Cập nhật title
            var titleTmp = _popupThemChuong.transform.Find("title")?.GetComponentInChildren<TextMeshProUGUI>();
            if (titleTmp != null) titleTmp.text = "THÊM CHƯƠNG MỚI";

            // Xóa input
            SetInputText(_popupThemChuong, "Image (2)", "tên chương", "");
            SetInputText(_popupThemChuong, "Image (1)", "mô tả", "");

            SelectPopupChapterSubject(_currentSubject);
        }

        private void OpenEditChapterPopup(ChapterItem ch)
        {
            if (_popupThemChuong == null || ch == null) return;
            _editingChapter = ch;

            _popupThemChuong.SetActive(true);
            _popupThemChuong.transform.SetAsLastSibling();

            var titleTmp = _popupThemChuong.transform.Find("title")?.GetComponentInChildren<TextMeshProUGUI>();
            if (titleTmp != null) titleTmp.text = "CHỈNH SỬA CHƯƠNG";

            SetInputText(_popupThemChuong, "Image (2)", "tên chương", ch.name);
            SetInputText(_popupThemChuong, "Image (1)", "mô tả", ch.description);

            SelectPopupChapterSubject(TeacherDataManager.StringToSubject(ch.subject));
        }

        private TeacherSubject _popupSelectedSubject = TeacherSubject.Physics;

        private void SelectPopupChapterSubject(TeacherSubject sub)
        {
            _popupSelectedSubject = sub;
            if (_popupThemChuong == null) return;

            HighlightButtonSelection(_popupThemChuong.transform.Find("vatly")?.gameObject, sub == TeacherSubject.Physics);
            HighlightButtonSelection(_popupThemChuong.transform.Find("sinh")?.gameObject, sub == TeacherSubject.Biology);
            HighlightButtonSelection(_popupThemChuong.transform.Find("hóa")?.gameObject, sub == TeacherSubject.Chemistry);
        }

        private void OnSaveChapterClicked()
        {
            string name = GetInputText(_popupThemChuong, "Image (2)", "tên chương");
            string desc = GetInputText(_popupThemChuong, "Image (1)", "mô tả");

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowToast("⚠️ Tên chương là bắt buộc, không được để trống!");
                return;
            }

            if (_editingChapter == null)
            {
                // Thêm mới
                if (TeacherDataManager.Instance.AddChapter(_popupSelectedSubject, name, desc, out string err))
                {
                    _currentSubject = _popupSelectedSubject;
                    _popupThemChuong.SetActive(false);
                    RefreshChapterList();
                    ShowToast($"✅ Đã tạo thành công chương mới: '{name}'!");
                }
                else
                {
                    ShowToast($"⚠️ {err}");
                }
            }
            else
            {
                // Cập nhật
                if (TeacherDataManager.Instance.UpdateChapter(_editingChapter.id, name, desc, out string err))
                {
                    _popupThemChuong.SetActive(false);
                    RefreshChapterList();
                    ShowToast($"✅ Đã cập nhật thành công chương '{name}'!");
                }
                else
                {
                    ShowToast($"⚠️ {err}");
                }
            }
        }

        #endregion

        #region ===== MÀN 4 & MÀN 5: QUẢN LÝ BÀI HỌC & CHỈNH SỬA NỘI DUNG LÝ THUYẾT =====

        private LessonItem _editingLesson = null;
        private GameObject _lessonScrollView;

        private void SetupLessonManagementEvents()
        {
            if (_panelBaiHoc == null) return;

            // 3 nút môn bên trái
            SetupButton(FindChildFuzzy(_panelBaiHoc.transform, "vatly", "vat ly"), () => SwitchLessonSubject(TeacherSubject.Physics));
            SetupButton(FindChildFuzzy(_panelBaiHoc.transform, "sinh", "sinhhoc"), () => SwitchLessonSubject(TeacherSubject.Biology));
            SetupButton(FindChildFuzzy(_panelBaiHoc.transform, "hoa", "hoahoc", "hóa"), () => SwitchLessonSubject(TeacherSubject.Chemistry));

            // Nút X đóng panel
            SetupButton(FindChildFuzzy(_panelBaiHoc.transform, "thoát", "thoat"), () => ShowOnlyDashboard());

            // Tạo nút Thêm bài học mới trên board nếu chưa có
            CreateAddLessonButtonIfNeeded();

            // Khởi tạo ScrollView danh sách bài học bên trong board
            CreateLessonScrollViewIfNeeded();

            // Setup sự kiện cho Popup Chỉnh sửa bài học (Màn 5)
            if (_popupSuaBaiHoc != null)
            {
                SetupButton(FindChildFuzzy(_popupSuaBaiHoc.transform, "lưu", "luu"), OnSaveLessonClicked);
                SetupButton(FindChildFuzzy(_popupSuaBaiHoc.transform, "thoat", "thoát"), () =>
                {
                    _popupSuaBaiHoc.SetActive(false);
                });
            }
        }

        private void CreateAddLessonButtonIfNeeded()
        {
            if (_panelBaiHoc == null) return;
            Transform board = _panelBaiHoc.transform.Find("board");
            if (board == null) return;

            Transform existing = board.Find("BtnThemBaiHoc");
            if (existing != null)
            {
                RectTransform art = existing.GetComponent<RectTransform>();
                if (art != null)
                {
                    art.anchoredPosition = new Vector2(420f, 185f);
                    art.sizeDelta = new Vector2(210f, 48f);
                }
                return;
            }

            GameObject btnObj = new GameObject("BtnThemBaiHoc");
            btnObj.transform.SetParent(board, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(420f, 185f);
            rt.sizeDelta = new Vector2(210f, 48f);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.12f, 0.48f, 0.28f, 0.95f);

            Outline ol = btnObj.AddComponent<Outline>();
            ol.effectColor = new Color(1f, 0.85f, 0.35f, 0.95f);
            ol.effectDistance = new Vector2(2f, -2f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "+ THÊM BÀI HỌC";
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            SetupButton(btnObj, OpenAddLessonPopup);
        }

        private void CreateLessonScrollViewIfNeeded()
        {
            if (_panelBaiHoc == null) return;
            Transform board = _panelBaiHoc.transform.Find("board");
            if (board == null) return;

            Transform existing = board.Find("ScrollLessons");
            if (existing != null)
            {
                _lessonScrollView = existing.gameObject;
                return;
            }

            _lessonScrollView = CreateDynamicScrollView(board, "ScrollLessons", new Vector2(0f, -60f), new Vector2(1380f, 520f));
        }

        public void OpenLessonManagement(TeacherSubject subject)
        {
            _currentSubject = subject;
            SetDashboardCardsActive(false);

            if (_panelChuong != null) _panelChuong.SetActive(false);
            if (_panelCauHoi != null) _panelCauHoi.SetActive(false);
            if (_panelTienDo != null) _panelTienDo.SetActive(false);

            if (_panelBaiHoc != null)
            {
                _panelBaiHoc.SetActive(true);
                if (_popupSuaBaiHoc != null) _popupSuaBaiHoc.SetActive(false);
                HideTemplateButtons();

                // Lấy chương đầu tiên của môn
                var chapters = TeacherDataManager.Instance.GetChapters(_currentSubject);
                if (chapters.Count > 0)
                {
                    _selectedChapterId = chapters[0].id;
                }

                BuildChapterTabs();
                RefreshLessonList();
            }
        }

        private void SwitchLessonSubject(TeacherSubject sub)
        {
            _currentSubject = sub;
            HighlightSubjectButtons(_panelBaiHoc, sub);

            var chapters = TeacherDataManager.Instance.GetChapters(_currentSubject);
            if (chapters.Count > 0)
            {
                _selectedChapterId = chapters[0].id;
            }

            BuildChapterTabs();
            RefreshLessonList();
            ShowToast($"Đang xem bài học môn {TeacherDataManager.SubjectToVietnamese(sub)}");
        }

        private void BuildChapterTabs()
        {
            if (_panelBaiHoc == null) return;

            // Đảm bảo các con ban đầu của titlechonchuong bị ẩn để không tràn UI
            Transform origTitle = _panelBaiHoc.transform.Find("titlechonchuong");
            if (origTitle != null)
            {
                for (int i = 0; i < origTitle.childCount; i++)
                {
                    origTitle.GetChild(i).gameObject.SetActive(false);
                }
            }

            // Tạo Horizontal ScrollView cho các tab chương nếu chưa có
            if (_chapterTabsScrollContainer == null)
            {
                Transform existing = _panelBaiHoc.transform.Find("ScrollChapterTabs");
                if (existing != null)
                {
                    _chapterTabsScrollContainer = existing.gameObject;
                }
                else
                {
                    _chapterTabsScrollContainer = CreateHorizontalScrollView(_panelBaiHoc.transform, "ScrollChapterTabs", new Vector2(40f, 185f), new Vector2(720f, 65f));
                }
            }

            Transform content = _chapterTabsScrollContainer.transform.Find("Viewport/Content");
            if (content == null) return;

            // Xóa tab cũ trong content
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            var chapters = TeacherDataManager.Instance.GetChapters(_currentSubject);
            string[] roman = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };

            for (int i = 0; i < chapters.Count; i++)
            {
                var ch = chapters[i];
                string tabLabel = ch.id.Contains("tonghop") ? "Tổng ôn" : (i < roman.Length ? $"Chương {roman[i]}" : ch.name);
                bool isSelected = ch.id == _selectedChapterId;

                GameObject tabGo = new GameObject($"Tab_{ch.id}");
                tabGo.transform.SetParent(content, false);
                RectTransform trt = tabGo.AddComponent<RectTransform>();
                trt.sizeDelta = new Vector2(160f, 48f);

                Image tabImg = tabGo.AddComponent<Image>();
                tabImg.color = isSelected ? new Color(0.22f, 0.62f, 0.95f) : new Color(0.16f, 0.18f, 0.26f, 0.95f);

                Outline ol = tabGo.AddComponent<Outline>();
                ol.effectColor = isSelected ? new Color(1f, 0.88f, 0.25f, 0.95f) : new Color(0.4f, 0.45f, 0.55f, 0.5f);
                ol.effectDistance = isSelected ? new Vector2(2f, -2f) : new Vector2(1f, -1f);

                GameObject textGo = new GameObject("Text");
                textGo.transform.SetParent(tabGo.transform, false);
                RectTransform lrt = textGo.AddComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;

                TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
                tmp.text = tabLabel;
                tmp.fontSize = 15;
                tmp.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                tmp.color = isSelected ? Color.white : new Color(0.8f, 0.85f, 0.9f);
                tmp.alignment = TextAlignmentOptions.Center;

                SetupButton(tabGo, () =>
                {
                    _selectedChapterId = ch.id;
                    BuildChapterTabs();
                    RefreshLessonList();
                });
            }
        }

        private GameObject CreateHorizontalScrollView(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject scrollGo = new GameObject(name);
            scrollGo.transform.SetParent(parent, false);
            RectTransform srt = scrollGo.AddComponent<RectTransform>();
            srt.anchoredPosition = pos;
            srt.sizeDelta = size;

            ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGo.transform, false);
            RectTransform vrt = viewport.AddComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = vrt.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 0.5f);
            crt.anchorMax = new Vector2(0f, 0.5f);
            crt.pivot = new Vector2(0f, 0.5f);
            crt.anchoredPosition = Vector2.zero;

            var hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Tạo thanh cuộn ngang (Horizontal Scrollbar) trực quan dễ kéo vuốt
            GameObject scrollbarGo = new GameObject("Scrollbar Horizontal");
            scrollbarGo.transform.SetParent(scrollGo.transform, false);
            RectTransform sbRt = scrollbarGo.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0f, 0f);
            sbRt.anchorMax = new Vector2(1f, 0f);
            sbRt.pivot = new Vector2(0.5f, 0f);
            sbRt.anchoredPosition = new Vector2(0f, -12f);
            sbRt.sizeDelta = new Vector2(0f, 10f);

            Image sbBg = scrollbarGo.AddComponent<Image>();
            sbBg.color = new Color(0.1f, 0.12f, 0.18f, 0.75f);

            GameObject slidingArea = new GameObject("Sliding Area");
            slidingArea.transform.SetParent(scrollbarGo.transform, false);
            RectTransform saRt = slidingArea.AddComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform, false);
            RectTransform handleRt = handle.AddComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0f, 0f);
            handleRt.anchorMax = new Vector2(0f, 1f);
            handleRt.sizeDelta = Vector2.zero;

            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = new Color(0.25f, 0.65f, 0.95f, 0.9f);

            Scrollbar scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRt;
            scrollbar.targetGraphic = handleImg;
            scrollbar.direction = Scrollbar.Direction.LeftToRight;

            scroll.viewport = vrt;
            scroll.content = crt;
            scroll.horizontalScrollbar = scrollbar;
            scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            return scrollGo;
        }

        private void RefreshLessonList()
        {
            if (_lessonScrollView == null) return;
            Transform content = _lessonScrollView.transform.Find("Viewport/Content");
            if (content == null) return;

            // Xóa các hàng cũ
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            var lessons = TeacherDataManager.Instance.GetLessons(_currentSubject, _selectedChapterId);

            if (lessons.Count == 0)
            {
                CreateEmptyStateMessage(content, "Chưa có bài học nào trong chương này. Nhấn nút '+ THÊM BÀI HỌC' để tạo mới.");
                return;
            }

            for (int i = 0; i < lessons.Count; i++)
            {
                var les = lessons[i];
                GameObject row = new GameObject($"LessonItem_{i}");
                row.transform.SetParent(content, false);
                RectTransform rrt = row.AddComponent<RectTransform>();
                rrt.sizeDelta = new Vector2(1180f, 88f);

                Image bg = row.AddComponent<Image>();
                bg.color = (i % 2 == 0) ? new Color(0.12f, 0.15f, 0.22f, 0.92f) : new Color(0.16f, 0.19f, 0.28f, 0.92f);

                Outline ol = row.AddComponent<Outline>();
                ol.effectColor = new Color(0.85f, 0.75f, 0.4f, 0.45f);
                ol.effectDistance = new Vector2(1f, -1f);

                // Badge Mã bài (pill có nền và viền đẹp mắt, căn lề an toàn cách mép 60px)
                GameObject badgeObj = new GameObject("Badge");
                badgeObj.transform.SetParent(row.transform, false);
                RectTransform brt = badgeObj.AddComponent<RectTransform>();
                brt.anchoredPosition = new Vector2(-475f, 0f);
                brt.sizeDelta = new Vector2(95f, 36f);
                Image bImg = badgeObj.AddComponent<Image>();
                bImg.color = new Color(0.15f, 0.22f, 0.35f, 0.95f);
                Outline bOl = badgeObj.AddComponent<Outline>();
                bOl.effectColor = new Color(0.35f, 0.85f, 1f, 0.85f);
                bOl.effectDistance = new Vector2(1f, -1f);

                var bTmp = CreateLabelDirect(badgeObj.transform, les.baiID.ToUpper(), 14, FontStyles.Bold, new Color(0.4f, 0.9f, 1f), Vector2.zero, new Vector2(95f, 36f), TextAlignmentOptions.Center);
                bTmp.enableAutoSizing = true;
                bTmp.fontSizeMin = 11f;
                bTmp.fontSizeMax = 15f;

                // Tên bài học (co chữ tự động fit khung)
                var titleTmp = CreateLabelDirect(row.transform, les.tenBai, 17, FontStyles.Bold, new Color(1f, 0.95f, 0.75f), new Vector2(-110f, 15f), new Vector2(620f, 32f), TextAlignmentOptions.MidlineLeft);
                titleTmp.enableAutoSizing = true;
                titleTmp.fontSizeMin = 13f;
                titleTmp.fontSizeMax = 17f;

                // Tóm tắt lý thuyết (co chữ tự động fit khung)
                string summary = les.khaiNiemTrongTam;
                if (!string.IsNullOrEmpty(summary) && summary.Length > 90) summary = summary.Substring(0, 90) + "...";
                var sumTmp = CreateLabelDirect(row.transform, summary, 13, FontStyles.Italic, new Color(0.85f, 0.85f, 0.85f), new Vector2(-110f, -16f), new Vector2(620f, 28f), TextAlignmentOptions.MidlineLeft);
                sumTmp.enableAutoSizing = true;
                sumTmp.fontSizeMin = 11f;
                sumTmp.fontSizeMax = 13f;

                // Dùng nút sửa có sẵn của User nếu có
                GameObject btnSuaGo;
                if (_btnSuaBaiHocTemplate != null)
                {
                    btnSuaGo = Instantiate(_btnSuaBaiHocTemplate, row.transform);
                    btnSuaGo.name = "btnSua";
                    btnSuaGo.SetActive(true);
                    RectTransform srt = btnSuaGo.GetComponent<RectTransform>();
                    srt.anchorMin = new Vector2(1f, 0.5f);
                    srt.anchorMax = new Vector2(1f, 0.5f);
                    srt.pivot = new Vector2(1f, 0.5f);
                    srt.anchoredPosition = new Vector2(-25f, 0f);
                    srt.sizeDelta = new Vector2(140f, 50f);
                }
                else
                {
                    btnSuaGo = CreateDynamicButton(row.transform, "btnSua", "SỬA", new Vector2(460f, 0f), new Vector2(130f, 44f), new Color(0.2f, 0.52f, 0.82f), null).gameObject;
                }

                var btnTmp = btnSuaGo.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTmp != null)
                {
                    btnTmp.text = "SỬA";
                    btnTmp.fontSize = 15;
                    btnTmp.fontStyle = FontStyles.Bold;
                }

                SetupButton(btnSuaGo, () =>
                {
                    OpenEditLessonPopup(les);
                });
            }

            EnsureVerticalLayout(content.gameObject, 10f);
        }

        private void OpenAddLessonPopup()
        {
            if (_popupSuaBaiHoc == null) return;
            _editingLesson = null;

            _popupSuaBaiHoc.SetActive(true);
            _popupSuaBaiHoc.transform.SetAsLastSibling();

            var titleTmp = _popupSuaBaiHoc.transform.Find("title")?.GetComponentInChildren<TextMeshProUGUI>();
            if (titleTmp != null) titleTmp.text = "THÊM BÀI HỌC MỚI";

            SetInputText(_popupSuaBaiHoc, "Image", "Text (TMP)", "", false);
            SetInputText(_popupSuaBaiHoc, "Image (1)", "Text (TMP)", "", true);
            SetInputText(_popupSuaBaiHoc, "Image (2)", "Text (TMP)", "", true);
            SetInputText(_popupSuaBaiHoc, "Image (3)", "Text (TMP)", "", true);

            ConfigureCaretForPopupInputs(_popupSuaBaiHoc);

            SetupButton(FindChildFuzzy(_popupSuaBaiHoc.transform, "thoat", "thoát"), () =>
            {
                _popupSuaBaiHoc.SetActive(false);
            });
            SetupButton(FindChildFuzzy(_popupSuaBaiHoc.transform, "lưu", "luu"), OnSaveLessonClicked);
        }

        private void OpenEditLessonPopup(LessonItem les)
        {
            if (_popupSuaBaiHoc == null || les == null) return;
            _editingLesson = les;

            _popupSuaBaiHoc.SetActive(true);
            _popupSuaBaiHoc.transform.SetAsLastSibling();

            var titleTmp = _popupSuaBaiHoc.transform.Find("title")?.GetComponentInChildren<TextMeshProUGUI>();
            if (titleTmp != null) titleTmp.text = $"CHỈNH SỬA: {les.tenBai}";

            SetInputText(_popupSuaBaiHoc, "Image", "Text (TMP)", les.tenBai, false);
            SetInputText(_popupSuaBaiHoc, "Image (1)", "Text (TMP)", les.khaiNiemTrongTam, true);
            SetInputText(_popupSuaBaiHoc, "Image (2)", "Text (TMP)", les.congThucCanNho, true);
            SetInputText(_popupSuaBaiHoc, "Image (3)", "Text (TMP)", les.hienTuongUngDung, true);

            ConfigureCaretForPopupInputs(_popupSuaBaiHoc);

            SetupButton(FindChildFuzzy(_popupSuaBaiHoc.transform, "thoat", "thoát"), () =>
            {
                _popupSuaBaiHoc.SetActive(false);
            });
            SetupButton(FindChildFuzzy(_popupSuaBaiHoc.transform, "lưu", "luu"), OnSaveLessonClicked);
        }

        private void OnSaveLessonClicked()
        {
            string name = GetInputText(_popupSuaBaiHoc, "Image", "Text (TMP)");
            string kn = GetInputText(_popupSuaBaiHoc, "Image (1)", "Text (TMP)");
            string ct = GetInputText(_popupSuaBaiHoc, "Image (2)", "Text (TMP)");
            string ht = GetInputText(_popupSuaBaiHoc, "Image (3)", "Text (TMP)");

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowToast("⚠️ Tên bài học không được để trống!");
                return;
            }

            if (_editingLesson == null)
            {
                // Thêm mới
                if (TeacherDataManager.Instance.AddLesson(_currentSubject, _selectedChapterId, name, kn, ct, ht, out string err))
                {
                    _popupSuaBaiHoc.SetActive(false);
                    RefreshLessonList();
                    ShowToast($"✅ Đã tạo bài học mới: '{name}'!");
                }
                else
                {
                    ShowToast($"⚠️ {err}");
                }
            }
            else
            {
                // Cập nhật
                if (TeacherDataManager.Instance.UpdateLesson(_editingLesson.baiID, name, kn, ct, ht, out string err))
                {
                    _popupSuaBaiHoc.SetActive(false);
                    RefreshLessonList();
                    ShowToast($"✅ Đã lưu lý thuyết bài học '{name}' vào hệ thống! Học sinh sẽ đọc được nội dung mới ngay.");
                }
                else
                {
                    ShowToast($"⚠️ {err}");
                }
            }
        }

        #endregion

        #region ===== MÀN 6 & MÀN 7: QUẢN LÝ NGÂN HÀNG CÂU HỎI & ĐỒNG BỘ GOOGLE SHEETS =====

        private QuestionData _editingQuestion = null;
        private GameObject _questionScrollView;
        private string _filterMucDo = "Tất cả";
        private string _filterLoai = "Tất cả";

        private void SetupQuestionManagementEvents()
        {
            if (_panelCauHoi == null) return;

            // 3 nút môn bên trái
            SetupButton(FindChildFuzzy(_panelCauHoi.transform, "vatly", "vat ly"), () => SwitchQuestionSubject(TeacherSubject.Physics));
            SetupButton(FindChildFuzzy(_panelCauHoi.transform, "sinh", "sinhhoc"), () => SwitchQuestionSubject(TeacherSubject.Biology));
            SetupButton(FindChildFuzzy(_panelCauHoi.transform, "hoa", "hoahoc", "hóa"), () => SwitchQuestionSubject(TeacherSubject.Chemistry));

            // Nút Thêm câu hỏi mới
            SetupButton(FindChildFuzzy(_panelCauHoi.transform, "thêm", "them"), () => OpenAddQuestionPopup());

            // Nút Tải lên / Đồng bộ từ Google Sheet
            SetupButton(FindChildFuzzy(_panelCauHoi.transform, "tải lên", "tai len", "dong bo"), OnSyncQuestionsFromSheetClicked);

            // Nút X đóng panel
            SetupButton(FindChildFuzzy(_panelCauHoi.transform, "thoat", "thoát"), () => ShowOnlyDashboard());

            // Tạo ScrollView hiển thị danh sách câu hỏi trong board
            CreateQuestionScrollViewIfNeeded();

            // Setup sự kiện cho Popup Thêm/Sửa câu hỏi (Màn 7)
            if (_popupThemCauHoi != null)
            {
                SetupButton(FindChildFuzzy(_popupThemCauHoi.transform, "lưu", "luu"), () => OnSaveQuestionClicked(false));
                SetupButton(FindChildFuzzy(_popupThemCauHoi.transform, "tải lên", "tai len"), () => OnSaveQuestionClicked(true));
                SetupButton(FindChildFuzzy(_popupThemCauHoi.transform, "thoát", "thoat"), () =>
                {
                    _popupThemCauHoi.SetActive(false);
                });
            }
        }

        private void CreateQuestionScrollViewIfNeeded()
        {
            if (_panelCauHoi == null) return;
            Transform board = _panelCauHoi.transform.Find("board");
            if (board == null) return;

            Transform existing = board.Find("ScrollQuestions");
            if (existing != null)
            {
                _questionScrollView = existing.gameObject;
                RectTransform existingRt = _questionScrollView.GetComponent<RectTransform>();
                if (existingRt != null)
                {
                    existingRt.anchoredPosition = new Vector2(0f, -80f);
                    existingRt.sizeDelta = new Vector2(1280f, 470f);
                }
            }
            else
            {
                _questionScrollView = CreateDynamicScrollView(board, "ScrollQuestions", new Vector2(0f, -80f), new Vector2(1280f, 470f));
            }

            if (board.Find("BtnSyncSheet") == null)
            {
                CreateDynamicButton(board, "BtnSyncSheet", "🔄 ĐỒNG BỘ GOOGLE SHEET", new Vector2(460f, 250f), new Vector2(240f, 48f), new Color(0.18f, 0.58f, 0.35f), OnSyncQuestionsFromSheetClicked);
            }
        }

        public void OpenQuestionManagement(TeacherSubject subject)
        {
            _currentSubject = subject;
            SetDashboardCardsActive(false);

            if (_panelChuong != null) _panelChuong.SetActive(false);
            if (_panelBaiHoc != null) _panelBaiHoc.SetActive(false);
            if (_panelTienDo != null) _panelTienDo.SetActive(false);

            if (_panelCauHoi != null)
            {
                _panelCauHoi.SetActive(true);
                if (_popupThemCauHoi != null) _popupThemCauHoi.SetActive(false);
                HideTemplateButtons();
                RefreshQuestionList();
            }
        }

        private void SwitchQuestionSubject(TeacherSubject sub)
        {
            _currentSubject = sub;
            HighlightSubjectButtons(_panelCauHoi, sub);
            RefreshQuestionList();
            ShowToast($"Đang xem ngân hàng câu hỏi môn {TeacherDataManager.SubjectToVietnamese(sub)}");
        }

        private void RefreshQuestionList()
        {
            if (_questionScrollView == null) return;
            HighlightSubjectButtons(_panelCauHoi, _currentSubject);

            Transform content = _questionScrollView.transform.Find("Viewport/Content");
            if (content == null) return;

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            var questions = TeacherDataManager.Instance.GetQuestions(_currentSubject, _filterMucDo, _filterLoai);

            if (questions.Count == 0)
            {
                CreateEmptyStateMessage(content, "Chưa có câu hỏi nào phù hợp với bộ lọc. Nhấn nút '+ THÊM' để soạn câu hỏi mới!");
                return;
            }

            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                GameObject row = new GameObject($"QItem_{i}");
                row.transform.SetParent(content, false);
                RectTransform rrt = row.AddComponent<RectTransform>();
                rrt.sizeDelta = new Vector2(1200f, 95f);

                Image bg = row.AddComponent<Image>();
                bg.color = (i % 2 == 0) ? new Color(0.12f, 0.15f, 0.22f, 0.92f) : new Color(0.16f, 0.19f, 0.28f, 0.92f);

                Outline ol = row.AddComponent<Outline>();
                ol.effectColor = new Color(0.85f, 0.75f, 0.4f, 0.45f);
                ol.effectDistance = new Vector2(1f, -1f);

                // Mã ID & Mã bài
                string idStr = !string.IsNullOrEmpty(q.questionID) ? q.questionID : $"Q_{(i + 1):D3}";
                string baiStr = !string.IsNullOrEmpty(q.baiID) ? q.baiID : "Bài 2";
                var idTmp = CreateLabelDirect(row.transform, $"{idStr} | {baiStr}", 13, FontStyles.Bold, new Color(0.35f, 0.85f, 1f), new Vector2(-480f, 20f), new Vector2(160f, 28f), TextAlignmentOptions.MidlineLeft);
                idTmp.enableAutoSizing = true; idTmp.fontSizeMin = 10f; idTmp.fontSizeMax = 14f;

                // Mức độ
                string mdStr = !string.IsNullOrEmpty(q.mucDo) ? q.mucDo : "Nhận biết";
                Color mdCol = mdStr.Contains("Nhận") ? new Color(0.2f, 0.8f, 0.4f) :
                              (mdStr.Contains("Thông") ? new Color(0.2f, 0.65f, 1f) :
                              (mdStr.Contains("cao") ? new Color(0.95f, 0.3f, 0.3f) : new Color(0.95f, 0.65f, 0.1f)));
                var mdTmp = CreateLabelDirect(row.transform, $"[{mdStr}]", 13, FontStyles.Bold, mdCol, new Vector2(-350f, 20f), new Vector2(95f, 28f), TextAlignmentOptions.MidlineLeft);
                mdTmp.enableAutoSizing = true; mdTmp.fontSizeMin = 10f; mdTmp.fontSizeMax = 13f;

                // Hình thức
                bool isEssay = !string.IsNullOrEmpty(q.loaiCauHoi) && (q.loaiCauHoi.Contains("Tự luận") || q.loaiCauHoi.Contains("TuLuan"));
                string typeStr = isEssay ? "[Tự luận]" : "[Trắc nghiệm]";
                var typeTmp = CreateLabelDirect(row.transform, typeStr, 12, FontStyles.Bold, isEssay ? new Color(1f, 0.7f, 0.3f) : new Color(0.4f, 0.85f, 1f), new Vector2(-255f, 20f), new Vector2(90f, 28f), TextAlignmentOptions.MidlineLeft);
                typeTmp.enableAutoSizing = true; typeTmp.fontSizeMin = 10f; typeTmp.fontSizeMax = 12f;

                // Nội dung câu hỏi (rút gọn & tự động fit cỡ chữ)
                string qText = q.questionText ?? "";
                if (qText.Length > 65) qText = qText.Substring(0, 65) + "...";
                var qTextTmp = CreateLabelDirect(row.transform, qText, 14, FontStyles.Bold, Color.white, new Vector2(50f, 20f), new Vector2(500f, 30f), TextAlignmentOptions.MidlineLeft);
                qTextTmp.enableAutoSizing = true; qTextTmp.fontSizeMin = 11f; qTextTmp.fontSizeMax = 15f;

                // Preview đáp án đúng / tham chiếu (tự động fit cỡ chữ)
                string ansPreview = "";
                if (!isEssay && q.answers != null && q.correctAnswerIndex >= 0 && q.correctAnswerIndex < q.answers.Length)
                {
                    char optChar = (char)('A' + q.correctAnswerIndex);
                    ansPreview = $"Đáp án đúng: {optChar}. {q.answers[q.correctAnswerIndex]}";
                }
                else
                {
                    ansPreview = $"Lời giải mẫu: {q.dapAnThamChieu ?? q.explanation ?? ""}";
                }
                if (ansPreview.Length > 85) ansPreview = ansPreview.Substring(0, 85) + "...";
                var ansTmp = CreateLabelDirect(row.transform, ansPreview, 12, FontStyles.Italic, new Color(0.4f, 1f, 0.65f), new Vector2(-70f, -20f), new Vector2(680f, 28f), TextAlignmentOptions.MidlineLeft);
                ansTmp.enableAutoSizing = true; ansTmp.fontSizeMin = 10f; ansTmp.fontSizeMax = 13f;

                // Nút Sửa của User nếu có
                GameObject btnSuaGo;
                if (_btnSuaCauHoiTemplate != null)
                {
                    btnSuaGo = Instantiate(_btnSuaCauHoiTemplate, row.transform);
                    btnSuaGo.name = "btnEdit";
                    btnSuaGo.SetActive(true);
                    RectTransform srt = btnSuaGo.GetComponent<RectTransform>();
                    srt.anchorMin = new Vector2(1f, 0.5f);
                    srt.anchorMax = new Vector2(1f, 0.5f);
                    srt.pivot = new Vector2(1f, 0.5f);
                    srt.anchoredPosition = new Vector2(-140f, 0f);
                    srt.sizeDelta = new Vector2(110f, 44f);
                }
                else
                {
                    btnSuaGo = CreateDynamicButton(row.transform, "btnEdit", "SỬA", new Vector2(390f, 0f), new Vector2(105f, 42f), new Color(0.2f, 0.5f, 0.75f), null).gameObject;
                }
                var sTmp = btnSuaGo.GetComponentInChildren<TextMeshProUGUI>();
                if (sTmp != null) { sTmp.text = "SỬA"; sTmp.fontSize = 14; sTmp.fontStyle = FontStyles.Bold; }
                SetupButton(btnSuaGo, () => OpenEditQuestionPopup(q));

                // Nút Đẩy Sheet của User nếu có
                GameObject btnPushGo;
                if (_btnPushCauHoiTemplate != null)
                {
                    btnPushGo = Instantiate(_btnPushCauHoiTemplate, row.transform);
                    btnPushGo.name = "btnPush";
                    btnPushGo.SetActive(true);
                    RectTransform prt = btnPushGo.GetComponent<RectTransform>();
                    prt.anchorMin = new Vector2(1f, 0.5f);
                    prt.anchorMax = new Vector2(1f, 0.5f);
                    prt.pivot = new Vector2(1f, 0.5f);
                    prt.anchoredPosition = new Vector2(-15f, 0f);
                    prt.sizeDelta = new Vector2(115f, 44f);
                }
                else
                {
                    btnPushGo = CreateDynamicButton(row.transform, "btnPush", "ĐẨY SHEET", new Vector2(510f, 0f), new Vector2(120f, 42f), new Color(0.18f, 0.58f, 0.35f), null).gameObject;
                }
                var pTmp = btnPushGo.GetComponentInChildren<TextMeshProUGUI>();
                if (pTmp != null) { pTmp.text = "ĐẨY SHEET"; pTmp.fontSize = 14; pTmp.fontStyle = FontStyles.Bold; }
                SetupButton(btnPushGo, () => PushSingleQuestionToSheet(q));
            }

            EnsureVerticalLayout(content.gameObject, 10f);
        }

        private void OnSyncQuestionsFromSheetClicked()
        {
            ShowToast($"🔄 Đang đồng bộ ngân hàng câu hỏi môn {TeacherDataManager.SubjectToVietnamese(_currentSubject)} từ Google Sheets...");
            TeacherDataManager.Instance.SyncQuestionsFromGoogleSheet(_currentSubject, (success) =>
            {
                if (success)
                {
                    RefreshQuestionList();
                    ShowToast($"✅ Đã cập nhật ngân hàng câu hỏi môn {TeacherDataManager.SubjectToVietnamese(_currentSubject)} mới nhất từ Google Sheets!");
                }
                else
                {
                    ShowToast("⚠️ Không thể kết nối Google Sheets. Dữ liệu trong game vẫn an toàn.");
                }
            });
        }

        private void PushSingleQuestionToSheet(QuestionData q)
        {
            ShowToast($"🚀 Đang đẩy câu hỏi {q.questionID} lên Google Sheets...");
            TeacherDataManager.Instance.PushQuestionToGoogleSheet(q, (success, resp) =>
            {
                if (success)
                {
                    ShowToast($"✅ Đẩy thành công! Đã ghi nhận {q.questionID} trên Google Sheets!");
                }
                else
                {
                    ShowToast($"⚠️ Lỗi: {resp}. Câu hỏi đã được lưu an toàn trong game.");
                }
            });
        }

        // ======================= POPUP THÊM / SỬA CÂU HỎI (MÀN 7) =======================

        private GameObject _qFormContainer;
        private TMP_InputField _inputQId;
        private TMP_InputField _inputQText;
        private TMP_InputField _inputAnsA, _inputAnsB, _inputAnsC, _inputAnsD;
        private TMP_InputField _inputExplanation, _inputTip;
        private TMP_InputField _inputRefAns, _inputRubric;
        private TMP_InputField _inputTeacherId;

        private GameObject _mcqContainer;
        private GameObject _essayContainer;
        private bool _formIsEssay = false;
        private int _formCorrectOpt = 0;
        private string _formMucDo = "Nhận biết";
        private string _formChuongId = "";
        private string _formBaiId = "";

        private void OpenAddQuestionPopup()
        {
            _editingQuestion = null;
            BuildQuestionForm(null);
        }

        private void OpenEditQuestionPopup(QuestionData q)
        {
            _editingQuestion = q;
            BuildQuestionForm(q);
        }

        private void BuildQuestionForm(QuestionData target)
        {
            if (_popupThemCauHoi == null) return;
            _popupThemCauHoi.SetActive(true);
            _popupThemCauHoi.transform.SetAsLastSibling();

            bool isNew = target == null;
            var titleTmp = _popupThemCauHoi.transform.Find("title")?.GetComponentInChildren<TextMeshProUGUI>();
            if (titleTmp != null) titleTmp.text = isNew ? "THÊM CÂU HỎI MỚI" : "CHỈNH SỬA CÂU HỎI";

            Transform board = _popupThemCauHoi.transform.Find("board");
            if (board == null) return;

            // Xóa container form cũ nếu có
            Transform oldForm = board.Find("FormScrollContainer");
            if (oldForm != null) Destroy(oldForm.gameObject);

            // Tạo Scroll View chứa Form hoàn chỉnh
            var formScroll = CreateDynamicScrollView(board, "FormScrollContainer", new Vector2(0f, 0f), new Vector2(1420f, 620f));
            Transform cont = formScroll.transform.Find("Viewport/Content");

            // Nảy số ID tự động nếu là tạo mới
            string autoId = target != null ? target.questionID : GenerateNextQuestionId(_currentSubject);

            _formIsEssay = target != null && !string.IsNullOrEmpty(target.loaiCauHoi) && (target.loaiCauHoi.Contains("Tự luận") || target.loaiCauHoi.Contains("TuLuan"));
            _formMucDo = target != null && !string.IsNullOrEmpty(target.mucDo) ? target.mucDo : "Nhận biết";
            _formChuongId = target != null && !string.IsNullOrEmpty(target.chuongId) ? target.chuongId : (_selectedChapterId ?? "chuong1");
            _formBaiId = target != null && !string.IsNullOrEmpty(target.baiID) ? target.baiID : "bai2";
            _formCorrectOpt = target != null ? Mathf.Clamp(target.correctAnswerIndex, 0, 3) : 0;

            // HÀNG 1: MÔN HỌC & HÌNH THỨC (TRẮC NGHIỆM / TỰ LUẬN) & MÃ ID & GV
            GameObject row1 = CreateFormRow(cont, "Row1", 50f);
            CreateLabelDirect(row1.transform, "Môn học (*):", 15, FontStyles.Bold, new Color(1f, 0.9f, 0.5f), new Vector2(-600f, 0f), new Vector2(110f, 35f), TextAlignmentOptions.MidlineLeft);

            var subjects = new TeacherSubject[] { TeacherSubject.Physics, TeacherSubject.Biology, TeacherSubject.Chemistry };
            float subX = -460f;
            foreach (var s in subjects)
            {
                var curS = s;
                bool isPick = _currentSubject == curS;
                CreateDynamicButton(row1.transform, $"btnSub_{s}", TeacherDataManager.SubjectToVietnamese(s), new Vector2(subX, 0f), new Vector2(100f, 36f), isPick ? new Color(0.2f, 0.6f, 0.9f) : new Color(0.2f, 0.22f, 0.3f), () =>
                {
                    _currentSubject = curS;
                    BuildQuestionForm(target);
                });
                subX += 110f;
            }

            CreateLabelDirect(row1.transform, "Hình thức (*):", 15, FontStyles.Bold, new Color(1f, 0.9f, 0.5f), new Vector2(-100f, 0f), new Vector2(120f, 35f), TextAlignmentOptions.MidlineLeft);
            Button btnMcq = null, btnEssay = null;

            Action updateTypeVisual = () =>
            {
                if (btnMcq != null) btnMcq.GetComponent<Image>().color = !_formIsEssay ? new Color(0.18f, 0.65f, 0.38f) : new Color(0.2f, 0.22f, 0.3f);
                if (btnEssay != null) btnEssay.GetComponent<Image>().color = _formIsEssay ? new Color(0.85f, 0.45f, 0.2f) : new Color(0.2f, 0.22f, 0.3f);
                if (_mcqContainer != null) _mcqContainer.SetActive(!_formIsEssay);
                if (_essayContainer != null) _essayContainer.SetActive(_formIsEssay);
            };

            btnMcq = CreateDynamicButton(row1.transform, "btnMcq", "Trắc nghiệm", new Vector2(50f, 0f), new Vector2(130f, 36f), new Color(0.18f, 0.65f, 0.38f), () =>
            {
                _formIsEssay = false;
                updateTypeVisual();
            });

            btnEssay = CreateDynamicButton(row1.transform, "btnEssay", "Tự luận", new Vector2(195f, 0f), new Vector2(120f, 36f), new Color(0.2f, 0.22f, 0.3f), () =>
            {
                _formIsEssay = true;
                updateTypeVisual();
            });

            CreateLabelDirect(row1.transform, "Mã ID:", 15, FontStyles.Bold, Color.white, new Vector2(330f, 0f), new Vector2(60f, 35f), TextAlignmentOptions.MidlineLeft);
            _inputQId = CreateDynamicInputField(row1.transform, "inpQId", autoId, new Vector2(430f, 0f), new Vector2(120f, 36f));

            CreateLabelDirect(row1.transform, "GV:", 15, FontStyles.Bold, Color.white, new Vector2(520f, 0f), new Vector2(40f, 35f), TextAlignmentOptions.MidlineLeft);
            _inputTeacherId = CreateDynamicInputField(row1.transform, "inpGv", _teacherId, new Vector2(600f, 0f), new Vector2(95f, 36f));

            // HÀNG 2: CHỌN CHƯƠNG & CHỌN BÀI HỌC
            GameObject row2 = CreateFormRow(cont, "Row2", 50f);
            CreateLabelDirect(row2.transform, "Chọn Chương (*):", 15, FontStyles.Bold, new Color(1f, 0.9f, 0.5f), new Vector2(-580f, 0f), new Vector2(150f, 35f), TextAlignmentOptions.MidlineLeft);

            var chapters = TeacherDataManager.Instance.GetChapters(_currentSubject);
            if (!chapters.Any(c => c.id == _formChuongId) && chapters.Count > 0) _formChuongId = chapters[0].id;

            var chBtns = new List<Button>();
            var chIds = new List<string>();

            Action updateChapterVisual = () =>
            {
                for (int c = 0; c < chBtns.Count; c++)
                {
                    if (chBtns[c] != null)
                    {
                        bool isSel = chIds[c] == _formChuongId;
                        chBtns[c].GetComponent<Image>().color = isSel ? new Color(0.2f, 0.6f, 0.9f) : new Color(0.18f, 0.22f, 0.30f);
                        var ol = chBtns[c].GetComponent<Outline>();
                        if (isSel)
                        {
                            if (ol == null) ol = chBtns[c].gameObject.AddComponent<Outline>();
                            ol.effectColor = new Color(1f, 0.85f, 0.25f, 0.95f);
                            ol.effectDistance = new Vector2(2f, -2f);
                            chBtns[c].transform.localScale = Vector3.one * 1.05f;
                        }
                        else
                        {
                            if (ol != null) Destroy(ol);
                            chBtns[c].transform.localScale = Vector3.one;
                        }
                    }
                }
            };

            float chX = -450f;
            foreach (var ch in chapters)
            {
                var curCh = ch;
                string cLabel = curCh.id.Contains("tonghop") ? "Tổng ôn" : (curCh.name.Length > 12 ? curCh.name.Substring(0, 10) + ".." : curCh.name);
                var btn = CreateDynamicButton(row2.transform, $"btnCh_{curCh.id}", cLabel, new Vector2(chX, 0f), new Vector2(105f, 34f), curCh.id == _formChuongId ? new Color(0.2f, 0.6f, 0.9f) : new Color(0.18f, 0.22f, 0.30f), () =>
                {
                    _formChuongId = curCh.id;
                    updateChapterVisual();
                });
                chBtns.Add(btn);
                chIds.Add(curCh.id);
                chX += 114f;
            }
            updateChapterVisual();

            // HÀNG 3: MỨC ĐỘ NHẬN THỨC (4 MỨC)
            GameObject row3 = CreateFormRow(cont, "Row3", 50f);
            CreateLabelDirect(row3.transform, "Mức độ (*):", 15, FontStyles.Bold, new Color(1f, 0.9f, 0.5f), new Vector2(-580f, 0f), new Vector2(150f, 35f), TextAlignmentOptions.MidlineLeft);

            string[] levels = { "Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao" };
            Color[] lColors = { new Color(0.18f, 0.65f, 0.35f), new Color(0.18f, 0.55f, 0.88f), new Color(0.9f, 0.58f, 0.15f), new Color(0.9f, 0.25f, 0.25f) };
            Button[] lvlBtns = new Button[4];

            Action updateLvlVisual = () =>
            {
                for (int l = 0; l < 4; l++)
                {
                    if (lvlBtns[l] != null)
                    {
                        bool isSel = levels[l] == _formMucDo;
                        lvlBtns[l].GetComponent<Image>().color = isSel ? lColors[l] : new Color(0.2f, 0.22f, 0.3f);
                    }
                }
            };

            float lx = -420f;
            for (int l = 0; l < 4; l++)
            {
                int lIdx = l;
                lvlBtns[l] = CreateDynamicButton(row3.transform, $"btnLvl_{l}", levels[l], new Vector2(lx, 0f), new Vector2(130f, 36f), levels[l] == _formMucDo ? lColors[l] : new Color(0.2f, 0.22f, 0.3f), () =>
                {
                    _formMucDo = levels[lIdx];
                    updateLvlVisual();
                });
                lx += 140f;
            }

            // HÀNG 4: NỘI DUNG ĐỀ BÀI
            GameObject row4 = CreateFormRow(cont, "Row4", 85f);
            CreateLabelDirect(row4.transform, "Nội dung đề bài (*):", 15, FontStyles.Bold, new Color(1f, 0.9f, 0.5f), new Vector2(-560f, 25f), new Vector2(180f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputQText = CreateDynamicInputField(row4.transform, "inpNoiDung", target?.questionText ?? "", new Vector2(60f, 0f), new Vector2(1050f, 65f), true);

            // ================== VÙNG TRẮC NGHIỆM ==================
            _mcqContainer = CreateFormRow(cont, "McqContainer", 230f);

            string aA = (target?.answers != null && target.answers.Length > 0) ? target.answers[0] : "";
            string aB = (target?.answers != null && target.answers.Length > 1) ? target.answers[1] : "";
            string aC = (target?.answers != null && target.answers.Length > 2) ? target.answers[2] : "";
            string aD = (target?.answers != null && target.answers.Length > 3) ? target.answers[3] : "";

            CreateLabelDirect(_mcqContainer.transform, "Đáp án A:", 14, FontStyles.Bold, Color.white, new Vector2(-580f, 85f), new Vector2(90f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputAnsA = CreateDynamicInputField(_mcqContainer.transform, "inpA", aA, new Vector2(-280f, 85f), new Vector2(480f, 34f));

            CreateLabelDirect(_mcqContainer.transform, "Đáp án B:", 14, FontStyles.Bold, Color.white, new Vector2(60f, 85f), new Vector2(90f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputAnsB = CreateDynamicInputField(_mcqContainer.transform, "inpB", aB, new Vector2(360f, 85f), new Vector2(480f, 34f));

            CreateLabelDirect(_mcqContainer.transform, "Đáp án C:", 14, FontStyles.Bold, Color.white, new Vector2(-580f, 40f), new Vector2(90f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputAnsC = CreateDynamicInputField(_mcqContainer.transform, "inpC", aC, new Vector2(-280f, 40f), new Vector2(480f, 34f));

            CreateLabelDirect(_mcqContainer.transform, "Đáp án D:", 14, FontStyles.Bold, Color.white, new Vector2(60f, 40f), new Vector2(90f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputAnsD = CreateDynamicInputField(_mcqContainer.transform, "inpD", aD, new Vector2(360f, 40f), new Vector2(480f, 34f));

            // Chọn đáp án đúng
            CreateLabelDirect(_mcqContainer.transform, "Đáp án đúng (*):", 15, FontStyles.Bold, new Color(0.4f, 1f, 0.6f), new Vector2(-560f, -5f), new Vector2(160f, 25f), TextAlignmentOptions.MidlineLeft);
            string[] opts = { "A", "B", "C", "D" };
            Button[] optBtns = new Button[4];

            Action updateOptVisual = () =>
            {
                for (int o = 0; o < 4; o++)
                {
                    if (optBtns[o] != null)
                    {
                        bool isPick = o == _formCorrectOpt;
                        optBtns[o].GetComponent<Image>().color = isPick ? new Color(0.18f, 0.72f, 0.38f) : new Color(0.24f, 0.27f, 0.35f);
                    }
                }
            };

            float ox = -380f;
            for (int o = 0; o < 4; o++)
            {
                int oIdx = o;
                optBtns[o] = CreateDynamicButton(_mcqContainer.transform, $"btnOpt_{opts[o]}", opts[o], new Vector2(ox, -5f), new Vector2(65f, 32f), o == _formCorrectOpt ? new Color(0.18f, 0.72f, 0.38f) : new Color(0.24f, 0.27f, 0.35f), () =>
                {
                    _formCorrectOpt = oIdx;
                    updateOptVisual();
                });
                ox += 75f;
            }

            CreateLabelDirect(_mcqContainer.transform, "Lời giải thích:", 14, FontStyles.Bold, Color.white, new Vector2(-580f, -48f), new Vector2(150f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputExplanation = CreateDynamicInputField(_mcqContainer.transform, "inpExp", target?.explanation ?? "", new Vector2(40f, -48f), new Vector2(1100f, 34f));

            CreateLabelDirect(_mcqContainer.transform, "Mẹo ghi nhớ nhanh:", 14, FontStyles.Bold, new Color(1f, 0.85f, 0.4f), new Vector2(-560f, -90f), new Vector2(180f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputTip = CreateDynamicInputField(_mcqContainer.transform, "inpTip", target?.meoGhiNho ?? "", new Vector2(40f, -90f), new Vector2(1100f, 32f));

            // ================== VÙNG TỰ LUẬN ==================
            _essayContainer = CreateFormRow(cont, "EssayContainer", 210f);

            CreateLabelDirect(_essayContainer.transform, "📘 Lời giải mẫu / Đáp án tham chiếu (*):", 15, FontStyles.Bold, new Color(0.4f, 1f, 0.6f), new Vector2(-460f, 75f), new Vector2(400f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputRefAns = CreateDynamicInputField(_essayContainer.transform, "inpRefAns", target?.dapAnThamChieu ?? target?.explanation ?? "", new Vector2(0f, 30f), new Vector2(1250f, 55f), true);

            CreateLabelDirect(_essayContainer.transform, "📋 Tiêu chí chấm điểm từng bước (Rubric) (*):", 14, FontStyles.Bold, new Color(1f, 0.88f, 0.45f), new Vector2(-440f, -15f), new Vector2(450f, 25f), TextAlignmentOptions.MidlineLeft);
            _inputRubric = CreateDynamicInputField(_essayContainer.transform, "inpRubric", target?.tieuChiCham ?? "", new Vector2(0f, -55f), new Vector2(1250f, 48f), true);

            // Cập nhật hiển thị theo loại câu hỏi ban đầu
            updateTypeVisual();
            updateLvlVisual();

            EnsureVerticalLayout(cont.gameObject, 12f);
        }

        private string GenerateNextQuestionId(TeacherSubject sub)
        {
            string prefix = sub == TeacherSubject.Physics ? "C9_" : (sub == TeacherSubject.Biology ? "BIO_" : "CHEM_");
            var inSub = TeacherDataManager.Instance.GetQuestions(sub);
            int maxNum = 0;
            foreach (var q in inSub)
            {
                if (!string.IsNullOrEmpty(q.questionID))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(q.questionID, @"\d+");
                    if (match.Success && int.TryParse(match.Value, out int n) && n > maxNum)
                    {
                        maxNum = n;
                    }
                }
            }
            int next = Mathf.Max(inSub.Count + 1, maxNum + 1);
            return $"{prefix}{next:D3}";
        }

        private void OnSaveQuestionClicked(bool pushToSheet)
        {
            string qText = _inputQText != null ? _inputQText.text.Trim() : "";
            if (string.IsNullOrWhiteSpace(qText))
            {
                ShowToast("⚠️ Nội dung đề bài câu hỏi không được để trống!");
                return;
            }

            var q = _editingQuestion ?? new QuestionData();
            q.questionText = qText;
            q.questionID = _inputQId != null && !string.IsNullOrWhiteSpace(_inputQId.text) ? _inputQId.text.Trim() : GenerateNextQuestionId(_currentSubject);
            q.mucDo = _formMucDo;
            q.chuongId = _formChuongId;
            q.baiID = _formBaiId;
            q.chapter = TeacherDataManager.SubjectToString(_currentSubject);
            q.giaoVienID = _inputTeacherId != null && !string.IsNullOrWhiteSpace(_inputTeacherId.text) ? _inputTeacherId.text.Trim() : _teacherId;
            q.trangThai = "Xuất bản";

            var ch = TeacherDataManager.Instance.GetChapterById(_formChuongId);
            q.tenChuong = ch != null ? ch.name : "Chương học";

            if (_formIsEssay)
            {
                string refAns = _inputRefAns != null ? _inputRefAns.text.Trim() : "";
                if (string.IsNullOrWhiteSpace(refAns))
                {
                    ShowToast("⚠️ Câu hỏi Tự Luận bắt buộc phải nhập Đáp án tham chiếu / Lời giải mẫu!");
                    return;
                }

                q.loaiCauHoi = "Tự luận";
                q.dapAnThamChieu = refAns;
                q.explanation = refAns;
                q.tieuChiCham = _inputRubric != null ? _inputRubric.text.Trim() : "";
                q.answers = new string[] { "Tự luận", "-", "-", "-" };
                q.correctAnswerIndex = 0;
            }
            else
            {
                string a = _inputAnsA != null ? _inputAnsA.text.Trim() : "";
                string b = _inputAnsB != null ? _inputAnsB.text.Trim() : "";
                string c = _inputAnsC != null ? _inputAnsC.text.Trim() : "";
                string d = _inputAnsD != null ? _inputAnsD.text.Trim() : "";

                if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
                {
                    ShowToast("⚠️ Câu hỏi trắc nghiệm cần nhập ít nhất 2 đáp án A và B!");
                    return;
                }

                q.loaiCauHoi = "Trắc nghiệm";
                q.answers = new string[] { a, b, string.IsNullOrWhiteSpace(c) ? "-" : c, string.IsNullOrWhiteSpace(d) ? "-" : d };
                q.correctAnswerIndex = _formCorrectOpt;
                q.explanation = _inputExplanation != null ? _inputExplanation.text.Trim() : "";
                q.meoGhiNho = _inputTip != null ? _inputTip.text.Trim() : "";
                q.dapAnThamChieu = "";
                q.tieuChiCham = "";
            }

            bool isNew = _editingQuestion == null;
            TeacherDataManager.Instance.SaveOrUpdateQuestion(q, isNew);

            if (_popupThemCauHoi != null) _popupThemCauHoi.SetActive(false);
            RefreshQuestionList();

            if (pushToSheet)
            {
                PushSingleQuestionToSheet(q);
            }
            else
            {
                ShowToast($"✅ Đã lưu câu hỏi {q.questionID} ({q.loaiCauHoi} - {q.mucDo}) vào game thành công!");
            }
        }

        #endregion

        #region ===== MÀN 8: TIẾN ĐỘ HỌC TẬP HỌC SINH =====

        private void SetupProgressManagementEvents()
        {
            if (_panelTienDo == null) return;

            // 3 nút chọn môn bên trái
            SetupButton(FindChildFuzzy(_panelTienDo.transform, "vatly", "vat ly"), () => SwitchProgressSubject(TeacherSubject.Physics));
            SetupButton(FindChildFuzzy(_panelTienDo.transform, "sinh", "sinhhoc"), () => SwitchProgressSubject(TeacherSubject.Biology));
            SetupButton(FindChildFuzzy(_panelTienDo.transform, "hoa", "hoahoc", "hóa"), () => SwitchProgressSubject(TeacherSubject.Chemistry));

            // Nút Làm mới
            SetupButton(FindChildFuzzy(_panelTienDo.transform, "làm mới", "lam moi", "refresh"), OnRefreshProgressClicked);

            // Nút X đóng panel về Dashboard
            SetupButton(FindChildFuzzy(_panelTienDo.transform, "thoát", "thoat"), () => ShowOnlyDashboard());
        }

        public void OpenProgressManagement(TeacherSubject subject)
        {
            _currentSubject = subject;
            SetDashboardCardsActive(false);

            if (_panelChuong != null) _panelChuong.SetActive(false);
            if (_panelBaiHoc != null) _panelBaiHoc.SetActive(false);
            if (_panelCauHoi != null) _panelCauHoi.SetActive(false);

            if (_panelTienDo != null)
            {
                _panelTienDo.SetActive(true);
                RefreshProgressTable();
            }
        }

        private void SwitchProgressSubject(TeacherSubject sub)
        {
            _currentSubject = sub;
            HighlightSubjectButtons(_panelTienDo, sub);
            RefreshProgressTable();
            ShowToast($"Đang xem kết quả học tập môn {TeacherDataManager.SubjectToVietnamese(sub)}");
        }

        private void OnRefreshProgressClicked()
        {
            ShowToast("🔄 Đang làm mới dữ liệu tiến độ từ Google Sheets...");
            TeacherDataManager.Instance.SyncProgressFromGoogleSheet((success) =>
            {
                RefreshProgressTable();
                if (success)
                {
                    ShowToast("✅ Đã cập nhật kết quả học tập mới nhất từ Google Sheets!");
                }
                else
                {
                    ShowToast("ℹ️ Đã nạp lại toàn bộ lịch sử học tập lưu trong game.");
                }
            });
        }

        private void RefreshProgressTable()
        {
            if (_panelTienDo == null) return;
            HighlightSubjectButtons(_panelTienDo, _currentSubject);

            Transform content = _panelTienDo.transform.Find("Scroll View/Viewport/Content");
            if (content == null || _studentRowTemplate == null) return;

            // Ẩn các hàng cũ
            for (int i = 0; i < content.childCount; i++)
            {
                content.GetChild(i).gameObject.SetActive(false);
            }

            var students = TeacherDataManager.Instance.GetStudentProgress(_currentSubject);

            if (students.Count == 0)
            {
                CreateEmptyStateMessage(content, $"Chưa có học sinh nào hoàn thành bài thi môn {TeacherDataManager.SubjectToVietnamese(_currentSubject)}.");
                return;
            }

            for (int i = 0; i < students.Count; i++)
            {
                var st = students[i];
                int index = i;

                GameObject rowGo;
                if (index < content.childCount)
                {
                    rowGo = content.GetChild(index).gameObject;
                }
                else
                {
                    rowGo = Instantiate(_studentRowTemplate, content);
                }

                rowGo.name = $"StudentRow_{index + 1}";
                rowGo.SetActive(true);

                // Xử lý background row: Chỉ riêng hạng 1 mới có vương miện và viền vàng
                Image rowImg = rowGo.GetComponent<Image>();
                if (rowImg != null)
                {
                    if (index == 0 && _crownSprite != null)
                    {
                        rowImg.sprite = _crownSprite;
                    }
                    else if (_normalRowSprite != null)
                    {
                        rowImg.sprite = _normalRowSprite;
                    }
                }

                // Điền Hạng (Chỉ người hạng 1 mới có vương miện vàng)
                var sttTmp = rowGo.transform.Find("stt")?.GetComponent<TextMeshProUGUI>();
                if (sttTmp == null) sttTmp = CreateLabelDirect(rowGo.transform, "", 16, FontStyles.Bold, Color.white, new Vector2(-612f, 0f), new Vector2(90f, 40f));
                if (sttTmp != null)
                {
                    if (index == 0)
                    {
                        // Hạng 1: Dòng vương miện vàng
                        sttTmp.text = (_crownSprite != null) ? "" : "🥇 1";
                        sttTmp.color = new Color(1f, 0.85f, 0.2f);
                    }
                    else
                    {
                        // Hạng 2 trở đi: Hàng bình thường, không vương miện
                        sttTmp.text = (index + 1).ToString();
                        sttTmp.color = Color.white;
                        sttTmp.fontSize = 15;
                        sttTmp.fontStyle = FontStyles.Bold;
                    }
                }

                // Họ tên học sinh
                SetRowText(rowGo, "Text (TMP)", st.studentName);

                // Lớp
                SetRowText(rowGo, "Text (TMP) (1)", st.className);

                // Bài đã làm
                string lesName = st.lessonName;
                if (lesName.Length > 20) lesName = lesName.Substring(0, 20) + "...";
                SetRowText(rowGo, "Text (TMP) (2)", lesName);

                // Điểm
                var scoreTmp = rowGo.transform.Find("Text (TMP) (3)")?.GetComponent<TextMeshProUGUI>();
                if (scoreTmp != null)
                {
                    scoreTmp.text = $"{st.score}đ";
                    scoreTmp.enableAutoSizing = true;
                    scoreTmp.fontSizeMin = 11f;
                    scoreTmp.fontSizeMax = 16f;
                    scoreTmp.color = st.score >= 90 ? new Color(0.3f, 1f, 0.5f) : (st.score >= 70 ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.4f, 0.4f));
                }

                // Tỷ lệ (%)
                float acc = st.accuracy;
                if (acc <= 0 && st.totalQuestions > 0) acc = (float)st.correctAnswers / st.totalQuestions * 100f;
                SetRowText(rowGo, "Text (TMP) (4)", $"{acc:F0}%");

                // Thời gian (phút giây)
                int m = (int)(st.timeUsed / 60);
                int s = (int)(st.timeUsed % 60);
                SetRowText(rowGo, "Text (TMP) (5)", $"{m}p {s}s");

                // Ngày làm
                SetRowText(rowGo, "Text (TMP) (6)", st.date);

                // Cho phép bấm vào học sinh xem chi tiết
                SetupButton(rowGo, () =>
                {
                    ShowToast($"👤 {st.studentName} ({st.className}): Điểm {st.score}đ | Tỷ lệ {acc:F0}% | Bài: {st.lessonName} | Ngày: {st.date}");
                });
            }

            EnsureVerticalLayout(content.gameObject, 8f);
        }

        private void SetRowText(GameObject row, string childName, string text)
        {
            var tmp = row.transform.Find(childName)?.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 11f;
                tmp.fontSizeMax = 15f;
            }
        }

        #endregion

        #region ===== TIỆN ÍCH DỰNG GIAO DIỆN & TƯƠNG TÁC =====

        private void SetupButton(GameObject go, UnityEngine.Events.UnityAction action)
        {
            if (go == null) return;
            UIHelper.GetOrAddButton(go, action);

            // Thêm hiệu ứng hover nếu chưa có
            var trigger = go.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = go.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
                entryEnter.callback.AddListener((data) => { go.transform.localScale = Vector3.one * 1.04f; });
                trigger.triggers.Add(entryEnter);

                var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
                entryExit.callback.AddListener((data) => { go.transform.localScale = Vector3.one; });
                trigger.triggers.Add(entryExit);
            }
        }

        private IEnumerator PunchAnimation(GameObject go, Action onComplete)
        {
            if (go != null)
            {
                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float dur = 0.16f;
                    float elapsed = 0f;
                    while (elapsed < dur)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / dur;
                        float scale = (t < 0.5f) ? Mathf.Lerp(1.04f, 0.94f, t / 0.5f) : Mathf.Lerp(0.94f, 1f, (t - 0.5f) / 0.5f);
                        rt.localScale = Vector3.one * scale;
                        yield return null;
                    }
                    rt.localScale = Vector3.one;
                }
            }
            onComplete?.Invoke();
        }

        private void HighlightSubjectButtons(GameObject panel, TeacherSubject sub)
        {
            if (panel == null) return;

            GameObject vObj = FindChildFuzzy(panel.transform, "vatly", "vat ly");
            GameObject sObj = FindChildFuzzy(panel.transform, "sinhhoc", "sinh");
            GameObject hObj = FindChildFuzzy(panel.transform, "hoahoc", "hoa", "hóa");

            HighlightButtonSelection(vObj, sub == TeacherSubject.Physics);
            HighlightButtonSelection(sObj, sub == TeacherSubject.Biology);
            HighlightButtonSelection(hObj, sub == TeacherSubject.Chemistry);
        }

        private GameObject FindChildFuzzy(Transform parent, params string[] names)
        {
            if (parent == null) return null;
            foreach (var n in names)
            {
                var t = parent.Find(n);
                if (t != null) return t.gameObject;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                string cName = CleanString(child.name);
                foreach (var n in names)
                {
                    string target = CleanString(n);
                    if (cName == target || cName.Contains(target) || target.Contains(cName))
                    {
                        return child.gameObject;
                    }
                }
            }
            return null;
        }

        private static string CleanString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Trim().ToLower()
                .Replace(" ", "")
                .Replace("à", "a").Replace("á", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("è", "e").Replace("é", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("ì", "i").Replace("í", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ò", "o").Replace("ó", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ù", "u").Replace("ú", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ỳ", "y").Replace("ý", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
                .Replace("đ", "d");
        }

        private void HighlightButtonSelection(GameObject btnObj, bool isSelected)
        {
            if (btnObj == null) return;

            var img = btnObj.GetComponent<Image>();
            var outline = btnObj.GetComponent<Outline>();
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (isSelected)
            {
                btnObj.transform.localScale = Vector3.one * 1.15f;
                if (img != null) img.color = Color.white;

                if (outline == null) outline = btnObj.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.88f, 0.2f, 1f);
                outline.effectDistance = new Vector2(4f, -4f);

                if (tmp != null)
                {
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = new Color(1f, 0.95f, 0.6f, 1f);
                }
            }
            else
            {
                btnObj.transform.localScale = Vector3.one * 0.92f;
                if (img != null) img.color = new Color(0.55f, 0.55f, 0.62f, 0.72f);

                if (outline != null) Destroy(outline);

                if (tmp != null)
                {
                    tmp.fontStyle = FontStyles.Normal;
                    tmp.color = new Color(0.75f, 0.75f, 0.78f, 0.8f);
                }
            }
        }

        private void SetInputText(GameObject parent, string containerName, string childTextName, string text, bool isMultiLine = false)
        {
            if (parent == null) return;
            Transform cont = parent.transform.Find(containerName);
            if (cont == null) cont = parent.transform;

            Transform child = cont.Find(childTextName);
            if (child == null)
            {
                var tmpInCont = cont.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmpInCont != null) child = tmpInCont.transform;
            }
            if (child == null) return;

            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            RectTransform crt = child.GetComponent<RectTransform>();

            // Đảm bảo container không nuốt raycast để child input nhận click toàn diện
            Image contImg = cont.GetComponent<Image>();
            if (contImg != null && cont != child)
            {
                contImg.raycastTarget = false;
            }

            // Mở rộng child phủ vừa vặn container
            if (crt != null && cont != child)
            {
                crt.anchorMin = Vector2.zero;
                crt.anchorMax = Vector2.one;
                crt.offsetMin = new Vector2(20f, 15f);
                crt.offsetMax = new Vector2(-20f, -15f);
            }

            if (tmp != null)
            {
                tmp.color = Color.white;
                tmp.fontSize = isMultiLine ? 15f : 17f;
                tmp.alignment = isMultiLine ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;
                tmp.raycastTarget = true;
            }

            // Nếu container có TMP_InputField cũ thì dọn dẹp tránh xung đột
            TMP_InputField contInput = cont.GetComponent<TMP_InputField>();
            if (contInput != null && cont != child)
            {
                Destroy(contInput);
            }

            TMP_InputField input = child.GetComponent<TMP_InputField>();
            if (input == null)
            {
                input = child.gameObject.AddComponent<TMP_InputField>();
            }

            input.textComponent = tmp;
            input.textViewport = crt ?? child.GetComponent<RectTransform>();
            if (tmp != null) input.fontAsset = tmp.font;
            input.lineType = isMultiLine ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;

            // Thiết lập con trỏ nhấp nháy rõ nét | khi click vào ô nhập
            input.caretBlinkRate = 0.85f;
            input.caretWidth = 3;
            input.customCaretColor = true;
            input.caretColor = new Color(1f, 0.88f, 0.25f, 1f);
            input.selectionColor = new Color(0.2f, 0.5f, 0.9f, 0.45f);

            input.text = text;
        }

        private string GetInputText(GameObject parent, string containerName, string childTextName)
        {
            if (parent == null) return "";
            Transform cont = parent.transform.Find(containerName);
            if (cont == null) cont = parent.transform;

            Transform child = cont.Find(childTextName);
            if (child != null)
            {
                TMP_InputField input = child.GetComponent<TMP_InputField>();
                if (input != null) return input.text.Trim();

                TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null) return tmp.text.Trim();
            }

            var anyInput = cont.GetComponentInChildren<TMP_InputField>(true);
            if (anyInput != null) return anyInput.text.Trim();

            var anyTmp = cont.GetComponentInChildren<TextMeshProUGUI>(true);
            if (anyTmp != null) return anyTmp.text.Trim();

            return "";
        }

        private GameObject CreateDynamicScrollView(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject scrollRoot = new GameObject(name);
            scrollRoot.transform.SetParent(parent, false);
            RectTransform srt = scrollRoot.AddComponent<RectTransform>();
            srt.anchoredPosition = pos;
            srt.sizeDelta = size;

            ScrollRect sr = scrollRoot.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 25f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollRoot.transform, false);
            RectTransform vrt = viewport.AddComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform crt = content.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
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

        private GameObject CreateFormRow(Transform parent, string name, float height)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1360f, height);
            return go;
        }

        private Button CreateDynamicButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color bgColor, Action onClick)
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
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        private TMP_InputField CreateDynamicInputField(Transform parent, string name, string initialText, Vector2 pos, Vector2 size, bool multiLine = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.14f, 0.16f, 0.24f, 0.95f);

            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0.85f, 0.72f, 0.35f, 0.75f);
            ol.effectDistance = new Vector2(1f, -1f);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            RectTransform trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10f, 5f);
            trt.offsetMax = new Vector2(-10f, -5f);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = initialText;
            tmp.fontSize = 15;
            tmp.color = Color.white;
            tmp.alignment = multiLine ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.MidlineLeft;

            TMP_InputField input = go.AddComponent<TMP_InputField>();
            input.textComponent = tmp;
            input.textViewport = trt;
            if (tmp != null) input.fontAsset = tmp.font;
            input.targetGraphic = img;
            if (multiLine) input.lineType = TMP_InputField.LineType.MultiLineNewline;
            input.text = initialText;

            // Thiết lập con trỏ nhấp nháy rõ nét | khi click vào ô nhập
            input.caretBlinkRate = 0.85f;
            input.caretWidth = 3;
            input.customCaretColor = true;
            input.caretColor = new Color(1f, 0.88f, 0.25f, 1f);
            input.selectionColor = new Color(0.2f, 0.5f, 0.9f, 0.45f);

            input.onSelect.AddListener((val) =>
            {
                if (ol != null)
                {
                    ol.effectColor = new Color(0.3f, 0.85f, 1f, 0.95f);
                    ol.effectDistance = new Vector2(2f, -2f);
                }
            });
            input.onDeselect.AddListener((val) =>
            {
                if (ol != null)
                {
                    ol.effectColor = new Color(0.85f, 0.72f, 0.35f, 0.75f);
                    ol.effectDistance = new Vector2(1f, -1f);
                }
            });

            return input;
        }

        private TextMeshProUGUI CreateLabelDirect(Transform parent, string text, float fontSize, FontStyles style, Color color, Vector2 pos, Vector2 size, TextAlignmentOptions align = TextAlignmentOptions.Center)
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

        private void CreateEmptyStateMessage(Transform parent, string message)
        {
            GameObject go = new GameObject("EmptyMessage");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1000f, 120f);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = $"ℹ️ {message}";
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Italic;
            tmp.color = new Color(0.9f, 0.85f, 0.6f);
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void EnsureVerticalLayout(GameObject go, float spacing)
        {
            VerticalLayoutGroup vlg = go.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = go.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void ShowToast(string message)
        {
            if (_canvas == null) return;

            if (_toastObj == null)
            {
                _toastObj = new GameObject("ToastTeacher");
                _toastObj.transform.SetParent(_canvas.transform, false);
                RectTransform trt = _toastObj.AddComponent<RectTransform>();
                trt.anchorMin = new Vector2(0.5f, 0.5f);
                trt.anchorMax = new Vector2(0.5f, 0.5f);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.anchoredPosition = new Vector2(0f, -220f);
                trt.sizeDelta = new Vector2(850f, 65f);

                Image tbg = _toastObj.AddComponent<Image>();
                tbg.color = new Color(0.08f, 0.10f, 0.16f, 0.96f);

                Outline to = _toastObj.AddComponent<Outline>();
                to.effectColor = new Color(1f, 0.85f, 0.35f, 0.85f);
                to.effectDistance = new Vector2(2f, -2f);

                GameObject textGo = new GameObject("Text");
                textGo.transform.SetParent(_toastObj.transform, false);
                RectTransform textRt = textGo.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(20f, 10f);
                textRt.offsetMax = new Vector2(-20f, -10f);

                TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 17;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = new Color(1f, 0.95f, 0.75f);
                tmp.alignment = TextAlignmentOptions.Center;
            }

            _toastObj.SetActive(true);
            _toastObj.transform.SetAsLastSibling();

            var textComponent = _toastObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null) textComponent.text = message;

            if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
            _toastCoroutine = StartCoroutine(HideToastRoutine());
        }

        private IEnumerator HideToastRoutine()
        {
            yield return new WaitForSeconds(3.5f);
            if (_toastObj != null) _toastObj.SetActive(false);
        }

        #endregion
    }
}
