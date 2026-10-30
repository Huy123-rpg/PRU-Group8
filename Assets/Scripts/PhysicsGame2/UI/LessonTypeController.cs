using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Các hình thức học trong Scene 17. Type
    /// </summary>
    public enum StudyModeType
    {
        None,
        Theory,
        Quiz,
        WrittenAnswer
    }

    /// <summary>
    /// Điều khiển logic cho Scene 17. Type (CHỌN HÌNH THỨC HỌC).
    /// Tuân thủ quy trình chọn và xác nhận:
    /// 1. Click vào thanh "Học lý thuyết": lưu Theory, highlight nổi bật, chưa chuyển Scene.
    /// 2. Click vào thanh "Trắc nghiệm": lưu Quiz, highlight nổi bật, chưa chuyển Scene.
    /// 3. Click vào thanh "Tự luận": lưu WrittenAnswer, highlight nổi bật, chưa chuyển Scene.
    /// 4. Click hình thức khác: chuyển trạng thái selected ngay lập tức, thanh cũ về bình thường.
    /// 5. Bấm nút XÁC NHẬN:
    ///    - Theory -> vào thẳng màn Học lý thuyết (18. Theory), không cần chọn mức độ.
    ///    - Quiz -> vào Scene 33. Chọn độ khó (MCQ). (Nếu là đề kiểm tra chương -> vào thẳng 11_Quiz).
    ///    - WrittenAnswer -> vào Scene 33. Chọn độ khó (ESSAY).
    ///    - Chưa chọn hình thức -> hiện thông báo yêu cầu chọn hình thức trước.
    /// 6. Bấm nút QUAY LẠI: về đúng bản đồ chương vừa chơi, giữ nguyên vị trí nhân vật gần thiết bị.
    /// 7. Toàn bộ thanh của từng hình thức đều click được, không bị bất kỳ blocker/TMP nào chặn raycast.
    /// </summary>
    public class LessonTypeController : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [SerializeField] private TextMeshProUGUI lessonTitleText;
        [SerializeField] private Button btnTheory;
        [SerializeField] private Button btnQuiz;
        [SerializeField] private Button btnWritten;
        [SerializeField] private Button btnPvp;
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnConfirm;
        [SerializeField] private Button btnClose; // Alias tương thích ngược cho Button (4)

        private StudyModeType _selectedMode = StudyModeType.None;
        public StudyModeType SelectedMode => _selectedMode;

        public Button BtnTheory => btnTheory;
        public Button BtnQuiz => btnQuiz;
        public Button BtnWritten => btnWritten;
        public Button BtnConfirm => btnConfirm;
        public Button BtnBack => btnBack;

        private GameObject _alertModal;
        private TextMeshProUGUI _alertText;

        // Outlines phục vụ visual feedback
        private Outline _outlineTheory;
        private Outline _outlineQuiz;
        private Outline _outlineWritten;

        private void Awake()
        {
            EnsureEventSystem();
            AutoBindUIElements();
            FixRaycastBlockers();
            CreateAlertModal();
        }

        private void Start()
        {
            NavigationService.Instance.EnsureDevContext();
            if (PhysicsGameSession.Instance != null && !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId))
            {
                if (NavigationService.Instance.Selection.ChapterId > 0)
                {
                    PhysicsGameSession.Instance.SelectChapter(NavigationService.Instance.Selection.ChapterId);
                }
                PhysicsGameSession.Instance.SelectLesson(NavigationService.Instance.Selection.LessonId, NavigationService.Instance.Selection.LessonTitle);
            }
            RenderLessonTitle();
            SetupButtonEvents();
            UpdateSelectionVisuals();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null)
                {
                    var pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = Input.mousePosition
                    };
                    var results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerData, results);
                    if (results.Count > 0)
                    {
                        Debug.Log($"[LessonTypeController Raycast Probe] Mouse click tại {Input.mousePosition} -> Top Hit: '{results[0].gameObject.name}', Total Hits: {results.Count}");
                    }
                    else
                    {
                        Debug.Log($"[LessonTypeController Raycast Probe] Mouse click tại {Input.mousePosition} -> Không trúng UI element nào");
                    }
                }
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void AutoBindUIElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);

            foreach (var b in allButtons)
            {
                if (b == null) continue;
                RectTransform rt = b.GetComponent<RectTransform>();
                if (rt == null) continue;

                Vector2 pos = rt.anchoredPosition;
                float w = rt.sizeDelta.x;
                float h = rt.sizeDelta.y;
                string bName = b.name.Trim();

                // 1. Tiêu đề bài học (Frame Header ở trên cùng: y ~ 308)
                if (pos.y > 200f && pos.y < 350f && Mathf.Abs(pos.x) < 200f && lessonTitleText == null)
                {
                    var tmp = b.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null) lessonTitleText = tmp;
                }

                // 2. Nút Quay lại: Nút ở góc dưới bên trái (x < -500, y < -300)
                else if (btnBack == null && (bName == "Button (3)" || (pos.x < -500f && pos.y < -300f)))
                {
                    btnBack = b;
                }

                // 3. Nút Xác nhận: Nút ở góc dưới bên phải (x > 500, y < -300)
                else if (btnConfirm == null && (bName == "Button (4)" || (pos.x > 500f && pos.y < -300f)))
                {
                    btnConfirm = b;
                    btnClose = b; // Đảm bảo test cũ tra cứu btnClose vẫn trỏ đúng Button (4)
                }

                // 4. Thanh Học lý thuyết: Thanh lớn bề rộng > 600, vị trí Y trong khoảng [20, 120]
                else if (btnTheory == null && (bName == "Button (2)" || (pos.y > 20f && pos.y < 120f && Mathf.Abs(pos.x) < 100f && w > 500f)))
                {
                    btnTheory = b;
                }

                // 5. Thanh Trắc nghiệm: Thanh lớn bề rộng > 600, vị trí Y trong khoảng [-110, 0]
                else if (btnQuiz == null && (bName == "Button (5)" || (pos.y > -110f && pos.y < 0f && Mathf.Abs(pos.x) < 100f && w > 500f)))
                {
                    btnQuiz = b;
                }

                // 6. Thanh Tự luận: Thanh lớn bề rộng > 600, vị trí Y trong khoảng [-210, -110]
                else if (btnWritten == null && (bName == "Button (6)" && w > 500f || (pos.y > -210f && pos.y < -110f && Mathf.Abs(pos.x) < 100f && w > 500f)))
                {
                    btnWritten = b;
                }

                // 7. Thanh Đối kháng: Thanh lớn bề rộng > 600, vị trí Y trong khoảng [-330, -210]
                else if (btnPvp == null && (bName == "Button (7)" && w > 500f || (pos.y > -330f && pos.y < -210f && Mathf.Abs(pos.x) < 100f && w > 500f)))
                {
                    btnPvp = b;
                }
            }

            // Đảm bảo btnWritten và btnPvp là con trực tiếp của Canvas, không bị lồng trong btnTheory
            if (btnWritten != null && btnWritten.transform.parent != canvas.transform)
            {
                btnWritten.transform.SetParent(canvas.transform, true);
            }
            if (btnPvp != null && btnPvp.transform.parent != canvas.transform)
            {
                btnPvp.transform.SetParent(canvas.transform, true);
            }

            // Fallback nếu chưa tìm thấy lessonTitleText
            if (lessonTitleText == null)
            {
                foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (tmp.text.Contains("Bài") || tmp.text.Contains("Bai") || tmp.rectTransform.anchoredPosition.y > 250f)
                    {
                        lessonTitleText = tmp;
                        break;
                    }
                }
            }

            Debug.Log($"[LessonTypeController] AutoBind hoàn tất: Theory={btnTheory?.name}, Quiz={btnQuiz?.name}, Written={btnWritten?.name}, Pvp={btnPvp?.name}, Back={btnBack?.name}, Confirm={btnConfirm?.name}");
        }

        public void FixRaycastBlockers()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 5 nút chính nhận click (Đối kháng vô hiệu hóa hoàn toàn)
            var activeGos = new HashSet<GameObject>
            {
                btnTheory?.gameObject,
                btnQuiz?.gameObject,
                btnWritten?.gameObject,
                btnBack?.gameObject,
                btnConfirm?.gameObject
            };

            // 1. Chỉ 5 nút chính mới có raycastTarget = true
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                if (img == null) continue;
                if (activeGos.Contains(img.gameObject))
                {
                    img.raycastTarget = true;
                    var btn = img.GetComponent<Button>();
                    if (btn != null) btn.targetGraphic = img;
                }
                else
                {
                    img.raycastTarget = false;
                }
            }

            // 2. Tắt raycastTarget trên tất cả TextMeshPro để không chặn click của nút bên dưới
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp != null) tmp.raycastTarget = false;
            }

            // 3. Vô hiệu hóa Đối kháng
            if (btnPvp != null)
            {
                btnPvp.interactable = false;
                var pvpImg = btnPvp.GetComponent<Image>();
                if (pvpImg != null) pvpImg.raycastTarget = false;
            }

            // 4. Vô hiệu hóa bất kỳ Button phụ nào nằm trên các child objects (icons, texts, header)
            foreach (var b in canvas.GetComponentsInChildren<Button>(true))
            {
                if (b != null && !activeGos.Contains(b.gameObject))
                {
                    b.enabled = false;
                    b.interactable = false;
                }
            }
        }

        private void RenderLessonTitle()
        {
            string title = NavigationService.Instance.Selection.LessonTitle;
            string lessonId = NavigationService.Instance.Selection.LessonId;

            if (lessonTitleText != null)
            {
                lessonTitleText.text = !string.IsNullOrEmpty(title) ? title : lessonId;
                lessonTitleText.enableAutoSizing = true;
                lessonTitleText.fontSizeMin = 18f;
                lessonTitleText.fontSizeMax = 24f;
                lessonTitleText.textWrappingMode = TextWrappingModes.Normal;
                lessonTitleText.overflowMode = TextOverflowModes.Ellipsis;
                lessonTitleText.alignment = TextAlignmentOptions.Center;
                lessonTitleText.color = new Color(1f, 0.96f, 0.88f, 1f); // Trắng kem sáng rõ
                lessonTitleText.margin = new Vector4(45f, 10f, 45f, 15f); // Cách đều hoa văn viền vàng của khung
            }

            // Đảm bảo cả ba hình thức học luôn sáng rõ 100%, không bị mờ hay chặn raycast
            if (btnWritten != null)
            {
                var cg = btnWritten.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
                var img = btnWritten.GetComponent<Image>();
                if (img != null) img.color = Color.white;
                btnWritten.interactable = true;
            }
        }

        private bool IsChapterTest(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return false;
            string lower = lessonId.ToLower();
            return lower.StartsWith("test_") || lower.Contains("_test");
        }

        private void SetupButtonEvents()
        {
            // 1. Gắn sự kiện và Outline cho Học lý thuyết
            if (btnTheory != null)
            {
                _outlineTheory = GetOrAddOutline(btnTheory.gameObject);
                AttachBarHover(btnTheory, StudyModeType.Theory, _outlineTheory);
                btnTheory.onClick.RemoveAllListeners();
                btnTheory.onClick.AddListener(() => SelectMode(StudyModeType.Theory));
            }

            // 2. Gắn sự kiện và Outline cho Trắc nghiệm
            if (btnQuiz != null)
            {
                _outlineQuiz = GetOrAddOutline(btnQuiz.gameObject);
                AttachBarHover(btnQuiz, StudyModeType.Quiz, _outlineQuiz);
                btnQuiz.onClick.RemoveAllListeners();
                btnQuiz.onClick.AddListener(() => SelectMode(StudyModeType.Quiz));
            }

            // 3. Gắn sự kiện và Outline cho Tự luận
            if (btnWritten != null)
            {
                _outlineWritten = GetOrAddOutline(btnWritten.gameObject);
                AttachBarHover(btnWritten, StudyModeType.WrittenAnswer, _outlineWritten);
                btnWritten.onClick.RemoveAllListeners();
                btnWritten.onClick.AddListener(() => SelectMode(StudyModeType.WrittenAnswer));
            }

            // 4. Đối kháng: VÔ HIỆU HÓA HOÀN TOÀN (Không click, không popup, không chuyển scene)
            if (btnPvp != null)
            {
                btnPvp.onClick.RemoveAllListeners();
                btnPvp.interactable = false;
            }

            // 5. Nút Quay lại
            if (btnBack != null)
            {
                btnBack.transform.SetAsLastSibling();
                var img = btnBack.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    btnBack.targetGraphic = img;
                }
                UIHoverPulseEffect.Attach(btnBack.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }

            // 6. Nút Xác nhận
            if (btnConfirm != null)
            {
                btnConfirm.transform.SetAsLastSibling();
                var img = btnConfirm.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    btnConfirm.targetGraphic = img;
                }
                UIHoverPulseEffect.Attach(btnConfirm.gameObject, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);
                btnConfirm.onClick.RemoveAllListeners();
                btnConfirm.onClick.AddListener(OnConfirmClicked);
            }
        }

        private Outline GetOrAddOutline(GameObject go)
        {
            var outline = go.GetComponent<Outline>() ?? go.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.5f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.enabled = false;
            return outline;
        }

        private void AttachBarHover(Button barBtn, StudyModeType mode, Outline outline)
        {
            if (barBtn == null) return;
            var hover = barBtn.GetComponent<BarHoverTrigger>() ?? barBtn.gameObject.AddComponent<BarHoverTrigger>();
            hover.onHoverEnter = () =>
            {
                if (_selectedMode != mode && outline != null)
                {
                    outline.enabled = true;
                    outline.effectColor = new Color(1f, 0.88f, 0.4f, 0.85f);
                    outline.effectDistance = new Vector2(2.5f, -2.5f);
                }
            };
            hover.onHoverExit = () =>
            {
                if (_selectedMode != mode && outline != null)
                {
                    outline.enabled = false;
                }
            };
        }

        /// <summary>
        /// Chọn hình thức học. Chỉ highlight lựa chọn, cập nhật trạng thái, không chuyển Scene ngay.
        /// </summary>
        public void SelectMode(StudyModeType mode)
        {
            Debug.Log($"[LessonTypeController] Click chọn hình thức: {mode} (Click nhận thành công từ EventSystem).");
            _selectedMode = mode;
            UpdateSelectionVisuals();
        }

        private void UpdateSelectionVisuals()
        {
            // Màu viền vàng hoàng gia Aurelia khi được chọn
            Color selectedOutlineColor = new Color(1f, 0.92f, 0.38f, 1f);
            Vector2 selectedDistance = new Vector2(4.5f, -4.5f);
            Vector3 selectedScale = new Vector3(1.025f, 1.025f, 1f);

            // 1. Học lý thuyết
            bool isTheory = (_selectedMode == StudyModeType.Theory);
            if (_outlineTheory != null)
            {
                _outlineTheory.enabled = isTheory;
                if (isTheory)
                {
                    _outlineTheory.effectColor = selectedOutlineColor;
                    _outlineTheory.effectDistance = selectedDistance;
                }
            }
            if (btnTheory != null)
            {
                btnTheory.transform.localScale = isTheory ? selectedScale : Vector3.one;
                var img = btnTheory.GetComponent<Image>();
                if (img != null) img.color = isTheory ? new Color(1f, 1f, 0.9f, 1f) : Color.white;
            }

            // 2. Trắc nghiệm
            bool isQuiz = (_selectedMode == StudyModeType.Quiz);
            if (_outlineQuiz != null)
            {
                _outlineQuiz.enabled = isQuiz;
                if (isQuiz)
                {
                    _outlineQuiz.effectColor = selectedOutlineColor;
                    _outlineQuiz.effectDistance = selectedDistance;
                }
            }
            if (btnQuiz != null)
            {
                btnQuiz.transform.localScale = isQuiz ? selectedScale : Vector3.one;
                var img = btnQuiz.GetComponent<Image>();
                if (img != null) img.color = isQuiz ? new Color(1f, 1f, 0.9f, 1f) : Color.white;
            }

            // 3. Tự luận
            bool isWritten = (_selectedMode == StudyModeType.WrittenAnswer);
            if (_outlineWritten != null)
            {
                _outlineWritten.enabled = isWritten;
                if (isWritten)
                {
                    _outlineWritten.effectColor = selectedOutlineColor;
                    _outlineWritten.effectDistance = selectedDistance;
                }
            }
            if (btnWritten != null)
            {
                btnWritten.transform.localScale = isWritten ? selectedScale : Vector3.one;
                var img = btnWritten.GetComponent<Image>();
                if (img != null) img.color = isWritten ? new Color(1f, 1f, 0.9f, 1f) : Color.white;
            }

            // 4. Đối kháng (luôn vô hiệu hóa, không highlight)
            if (btnPvp != null)
            {
                btnPvp.transform.localScale = Vector3.one;
                var img = btnPvp.GetComponent<Image>();
                if (img != null) img.color = new Color(0.7f, 0.7f, 0.75f, 0.6f);
            }
        }

        /// <summary>
        /// Bấm nút XÁC NHẬN để chuyển sang màn hình tương ứng với hình thức đã chọn.
        /// </summary>
        public void OnConfirmClicked()
        {
            Debug.Log($"[LessonTypeController] Bấm XÁC NHẬN. Hình thức hiện tại: {_selectedMode}");

            if (_selectedMode == StudyModeType.None)
            {
                Debug.LogWarning("[LessonTypeController] Chưa chọn hình thức học -> Nhắc nhở người chơi.");
                ShowAlert("Vui lòng chọn một hình thức học trước khi bấm Xác nhận!");
                return;
            }

            if (PhysicsGameSession.Instance != null)
            {
                if (PhysicsGameSession.Instance.StudyTimeLimitReached)
                {
                    ShowAlert("Đã hết thời gian buổi học hôm nay. Vui lòng vào 'Buổi học hôm nay' để bắt đầu buổi mới.");
                    return;
                }
                if (NavigationService.Instance.Selection.ChapterId > 0)
                {
                    PhysicsGameSession.Instance.SelectChapter(NavigationService.Instance.Selection.ChapterId);
                }
                if (!string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId))
                {
                    PhysicsGameSession.Instance.SelectLesson(NavigationService.Instance.Selection.LessonId, NavigationService.Instance.Selection.LessonTitle);
                }
            }

            string lessonId = NavigationService.Instance.Selection.LessonId;
            int chapterIndex = NavigationService.Instance.Selection.ChapterId;

            switch (_selectedMode)
            {
                case StudyModeType.Theory:
                    Debug.Log($"[LessonTypeController] Xác nhận Học lý thuyết -> Chuyển trực tiếp sang Scene 18. Theory (RouteId.Theory).");
                    NavigationService.Instance.NavigateTo(RouteId.Theory);
                    break;

                case StudyModeType.Quiz:
                    NavigationService.Instance.Selection.Mode = "MCQ";
                    if (IsChapterTest(lessonId))
                    {
                        Debug.Log($"[LessonTypeController] Đề kiểm tra chương: tạo đề 10 câu hỗn hợp -> Vào thẳng Scene 11_Quiz (RouteId.PracticeQuiz).");
                        var mcqs = GameDataRepository.GetChapterTestMCQs(chapterIndex);
                        NavigationService.Instance.Attempt = new AttemptContextData
                        {
                            AttemptId = Guid.NewGuid().ToString("N"),
                            Source = AttemptSource.RESTORATION,
                            BankVersion = "1.0",
                            TotalQuestions = mcqs.Count,
                            QuestionCount = mcqs.Count,
                            QuestionIds = mcqs.Select(q => q.cauHoiID).ToList(),
                            ScoreRaw = 0f,
                            CorrectCount = 0,
                            IsFinished = false
                        };

                        NavigationService.Instance.Attempt.Answers.Clear();
                        foreach (var q in mcqs)
                        {
                            NavigationService.Instance.Attempt.Answers.Add(new QuizAnswerRecord
                            {
                                CauHoiID = q.cauHoiID,
                                QuestionText = q.noiDung,
                                Options = new[] { q.dapAnA, q.dapAnB, q.dapAnC, q.dapAnD },
                                CorrectOptionIndex = q.correctIndex,
                                Explanation = q.giaiThich,
                                KeyKnowledge = q.meoGhiNho,
                                SelectedOptionIndex = -1
                            });
                        }

                        NavigationService.Instance.NavigateTo(RouteId.PracticeQuiz);
                    }
                    else
                    {
                        Debug.Log($"[LessonTypeController] Xác nhận Trắc nghiệm -> Chuyển sang Scene 33. Chọn độ khó (RouteId.DifficultySelection).");
                        NavigationService.Instance.NavigateTo(RouteId.DifficultySelection);
                    }
                    break;

                case StudyModeType.WrittenAnswer:
                    if (IsChapterTest(lessonId))
                    {
                        ShowAlert("Bài kiểm tra chương chỉ có hình thức trắc nghiệm.");
                        return;
                    }
                    Debug.Log($"[LessonTypeController] Xác nhận Tự luận -> Chuyển sang Scene 33. Chọn độ khó (RouteId.DifficultySelection).");
                    NavigationService.Instance.Selection.Mode = "ESSAY";
                    NavigationService.Instance.NavigateTo(RouteId.DifficultySelection);
                    break;
            }
        }

        public void OnPvpClicked()
        {
            // Chế độ Đối kháng chưa triển khai -> Bỏ qua hoàn toàn, KHÔNG hiện popup, KHÔNG chuyển scene.
            Debug.Log("[LessonTypeController] Chế độ Đối kháng chưa triển khai (đã vô hiệu hóa tương tác).");
        }

        public void OnBackClicked()
        {
            int ch = NavigationService.Instance.Selection.ChapterId;
            if (ch < 1 || ch > 5)
            {
                ch = NavigationService.Instance.ReturnContext.ChapterId;
            }
            if ((ch < 1 || ch > 5) && PhysicsGameSession.Instance != null && PhysicsGameSession.Instance.SelectedChapterId > 0)
            {
                ch = PhysicsGameSession.Instance.SelectedChapterId;
            }
            if (ch < 1 || ch > 5) ch = 1;

            bool completed = RestorationService.IsChapterFullyRestored(ch);
            RouteId targetRoute = SceneCatalog.GetChapterMapRoute(ch, completed);
            string sceneName = SceneCatalog.GetSceneName(targetRoute);

            Debug.Log($"[LessonTypeController] Bấm Quay lại -> Trở về bản đồ Chương {ch} (Route: {targetRoute}, Scene: {sceneName}), vị trí nhân vật bảo toàn: {NavigationService.Instance.ReturnContext.PlayerPosition}");

            Time.timeScale = 1f;
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.ResetTransitionLock();
                NavigationService.Instance.NavigateTo(targetRoute);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }

        private void CreateAlertModal()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _alertModal = new GameObject("TypeAlertModal");
            _alertModal.transform.SetParent(canvas.transform, false);
            _alertModal.transform.SetAsLastSibling();

            var rt = _alertModal.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var bg = _alertModal.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.08f, 0.18f, 0.85f);

            var panel = new GameObject("Panel");
            panel.transform.SetParent(_alertModal.transform, false);
            var pRt = panel.AddComponent<RectTransform>();
            pRt.sizeDelta = new Vector2(500f, 260f);
            pRt.anchoredPosition = Vector2.zero;

            var pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0.08f, 0.14f, 0.28f, 0.98f);

            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            var tGo = new GameObject("Text");
            tGo.transform.SetParent(panel.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0f, 30f);
            tRt.sizeDelta = new Vector2(440f, 120f);
            _alertText = tGo.AddComponent<TextMeshProUGUI>();
            _alertText.fontSize = 20f;
            _alertText.alignment = TextAlignmentOptions.Center;
            _alertText.color = Color.white;
            _alertText.raycastTarget = false;

            var btnGo = new GameObject("Btn_OK");
            btnGo.transform.SetParent(panel.transform, false);
            var bRt = btnGo.AddComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(0f, -70f);
            bRt.sizeDelta = new Vector2(160f, 44f);
            var bImg = btnGo.AddComponent<Image>();
            bImg.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = bImg;
            btn.onClick.AddListener(() => _alertModal.SetActive(false));

            var btGo = new GameObject("Text");
            btGo.transform.SetParent(btnGo.transform, false);
            var btRt = btGo.AddComponent<RectTransform>();
            btRt.sizeDelta = bRt.sizeDelta;
            var btTmp = btGo.AddComponent<TextMeshProUGUI>();
            btTmp.text = "ĐÃ HIỂU";
            btTmp.fontSize = 17f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = new Color(0.08f, 0.12f, 0.24f, 1f);
            btTmp.raycastTarget = false;

            _alertModal.SetActive(false);
        }

        private void ShowAlert(string msg)
        {
            if (_alertModal != null)
            {
                if (_alertText != null) _alertText.text = msg;
                _alertModal.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Component bắt hover cho các thanh hình thức học.
    /// </summary>
    public class BarHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action onHoverEnter;
        public Action onHoverExit;

        public void OnPointerEnter(PointerEventData eventData) => onHoverEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => onHoverExit?.Invoke();
    }
}
