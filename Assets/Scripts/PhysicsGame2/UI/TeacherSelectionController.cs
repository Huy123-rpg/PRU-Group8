using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ logic giao diện Scene 03_TeacherSelection.
    /// Tự động liên kết các thẻ giáo viên: Thầy Minh, Cô Lan, Thầy Duy, Cô Hà và nút Quay lại (quay lai).
    /// Đảm bảo tự động bổ sung EventSystem nếu scene thiếu để click hoạt động 100%.
    /// </summary>
    public class TeacherSelectionController : MonoBehaviour
    {
        [Header("=== TEACHER CARDS ===")]
        [SerializeField] private GameObject cardThayMinh; // "chon giao vien"
        [SerializeField] private GameObject cardCoLan;     // "co Lan"
        [SerializeField] private GameObject cardThayDuy;   // "thay Duy"
        [SerializeField] private GameObject cardCoHa;      // "co Ha"

        [Header("=== BACK BUTTON ===")]
        [SerializeField] private GameObject btnBack;       // "quay lai"

        private readonly Dictionary<string, GameObject> _teacherCards = new Dictionary<string, GameObject>();

        private void Awake()
        {
            EnsureEventSystem();
            FindUIReferences();
            SetupTeacherButtons();
            SetupBackButton();
        }

        private void Start()
        {
            HighlightCurrentTeacher();
        }

        /// <summary>
        /// Khắc phục lỗi thiếu EventSystem trong Scene 03 đã được phát hiện trong audit.
        /// </summary>
        private void EnsureEventSystem()
        {
            EventSystem es = FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                Debug.Log("[TeacherSelectionController] Phát hiện thiếu EventSystem. Tự động khởi tạo EventSystem + InputSystemUIInputModule.");
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void FindUIReferences()
        {
            // Trong scene gốc, thẻ Thầy Minh nằm ở object 'header' (card 1 bên trái)
            if (cardThayMinh == null) cardThayMinh = GameObject.Find("header") ?? GameObject.Find("chon giao vien");
            if (cardCoLan == null) cardCoLan = GameObject.Find("co Lan");
            if (cardThayDuy == null) cardThayDuy = GameObject.Find("thay Duy");
            if (cardCoHa == null) cardCoHa = GameObject.Find("co Ha");
            if (btnBack == null) btnBack = GameObject.Find("quay lai");

            if (cardThayMinh != null) _teacherCards["Minh"] = cardThayMinh;
            if (cardCoLan != null) _teacherCards["Lan"] = cardCoLan;
            if (cardThayDuy != null) _teacherCards["Duy"] = cardThayDuy;
            if (cardCoHa != null) _teacherCards["Ha"] = cardCoHa;
        }

        private void SetupTeacherButtons()
        {
            foreach (var kvp in _teacherCards)
            {
                string teacherId = kvp.Key;
                GameObject go = kvp.Value;
                if (go == null) continue;

                Button btn = go.GetComponent<Button>();
                if (btn == null)
                {
                    btn = go.AddComponent<Button>();
                    Image img = go.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                        btn.targetGraphic = img;
                    }
                }

                // Gỡ bỏ UIButtonFeedback để tránh xung đột pointer down
                var feedback = go.GetComponent<UIButtonFeedback>();
                if (feedback != null) Destroy(feedback);
                UIHoverPulseEffect.Attach(go, hover: 1.08f, minPulse: 1.05f, maxPulse: 1.10f, outline: true);

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnTeacherSelected(teacherId));
            }
        }

        private void SetupBackButton()
        {
            if (btnBack != null)
            {
                Button btn = btnBack.GetComponent<Button>();
                if (btn == null)
                {
                    btn = btnBack.AddComponent<Button>();
                    Image img = btnBack.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                        btn.targetGraphic = img;
                    }
                }

                var feedback = btnBack.GetComponent<UIButtonFeedback>();
                if (feedback != null) Destroy(feedback);
                UIHoverPulseEffect.Attach(btnBack, hover: 1.08f, minPulse: 1.04f, maxPulse: 1.09f, outline: true);

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnBackClicked);
            }
        }

        private void HighlightCurrentTeacher()
        {
            string selectedId = PhysicsGameSession.Instance.SelectedTeacherId;
            foreach (var kvp in _teacherCards)
            {
                string teacherId = kvp.Key;
                GameObject go = kvp.Value;
                if (go == null) continue;

                Image img = go.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white; // Luôn giữ màu gốc bản thiết kế
                }

                var hover = go.GetComponent<UIHoverPulseEffect>();
                if (hover != null)
                {
                    hover.SetSelected(teacherId == selectedId);
                }
            }
        }

        private void OnTeacherSelected(string teacherId)
        {
            Debug.Log($"[TeacherSelectionController] Đã chọn giáo viên: {teacherId}");
            PhysicsGameSession.Instance.SelectTeacher(teacherId);
            HighlightCurrentTeacher();

            Sprite portrait = null;
            if (_teacherCards.TryGetValue(teacherId, out GameObject cardGo) && cardGo != null)
            {
                Image cardImg = cardGo.GetComponent<Image>();
                if (cardImg != null) portrait = cardImg.sprite;
            }

            Canvas c = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            StoryFlowController.HandleTeacherSelection(
                teacherId: teacherId,
                customSave: null,
                navigateToChapterSelection: () =>
                {
                    if (PhysicsGameSession.Instance != null)
                        PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.TodayStudy);
                },
                canvasTransform: c != null ? c.transform : null,
                portrait: portrait);
        }

        private void OnBackClicked()
        {
            Debug.Log("[TeacherSelectionController] Quay lại 02_CharacterSelection.");
            PhysicsGameSession.Instance.LoadScene(PhysicsSceneNames.CharacterSelection);
        }
    }
}
