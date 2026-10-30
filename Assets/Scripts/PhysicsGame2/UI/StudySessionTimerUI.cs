using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Hiển thị thanh đếm ngược thời gian buổi học toàn cục (Session Timer) phong cách fantasy hoàng gia Aurelia:
    /// - Quản lý thời gian buổi học thống nhất xuyên suốt mọi Scene (Bản đồ, Chọn hình thức, Lý thuyết, Trắc nghiệm, Tự luận, Kết quả, Xem lại).
    /// - Không bao giờ bị reset khi chuyển Scene hoặc chuyển chương.
    /// - Đồng bộ trực tiếp với PhysicsGameSession.Instance.RemainingStudySeconds.
    /// - Khi hết giờ: đổi màu cảnh báo, thông báo rõ ràng cho người học.
    /// - RaycastTarget = false để tuyệt đối không chặn click các nút điều hướng bên dưới.
    /// </summary>
    public class StudySessionTimerUI : MonoBehaviour
    {
        [SerializeField] private RectTransform badgeRect;
        [SerializeField] private Image badgeBg;
        [SerializeField] private Outline badgeOutline;
        [SerializeField] private TextMeshProUGUI timerText;

        private static StudySessionTimerUI _instance;
        public static StudySessionTimerUI Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            EnsureVisualElements();
        }

        private void Start()
        {
            // Đảm bảo session luôn có phiên học hoạt động (mặc định 30 phút nếu chưa chọn)
            if (PhysicsGameSession.Instance != null && !PhysicsGameSession.Instance.IsStudySessionActive)
            {
                PhysicsGameSession.Instance.StartOrResumeStudySession();
            }
            UpdateDisplay();
        }

        private void Update()
        {
            UpdateDisplay();
        }

        private void EnsureVisualElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Nếu chưa có badgeRect, tự động tạo thanh hiển thị tinh tế
            if (badgeRect == null)
            {
                GameObject badgeGo = new GameObject("Aurelia_SessionTimerBadge");
                badgeGo.transform.SetParent(canvas.transform, false);

                badgeRect = badgeGo.AddComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
                badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
                badgeRect.pivot = new Vector2(0.5f, 0.5f);
                badgeRect.anchoredPosition = new Vector2(0f, 492f);
                badgeRect.sizeDelta = new Vector2(170f, 36f);

                badgeBg = badgeGo.AddComponent<Image>();
                // Nền xanh navy hoàng gia Aurelia sẫm bán trong suốt
                badgeBg.color = new Color(0.04f, 0.08f, 0.18f, 0.90f);
                badgeBg.raycastTarget = false;

                badgeOutline = badgeGo.AddComponent<Outline>();
                // Viền vàng fantasy Aurelia
                badgeOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.85f);
                badgeOutline.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject txtGo = new GameObject("Text");
                txtGo.transform.SetParent(badgeGo.transform, false);
                RectTransform tRt = txtGo.AddComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.offsetMin = Vector2.zero;
                tRt.offsetMax = Vector2.zero;

                timerText = txtGo.AddComponent<TextMeshProUGUI>();
                timerText.font = canvas.GetComponentInChildren<TextMeshProUGUI>(true)?.font;
                timerText.fontSize = 18f;
                timerText.fontStyle = FontStyles.Bold;
                timerText.alignment = TextAlignmentOptions.Center;
                timerText.color = new Color(1f, 0.96f, 0.88f, 1f); // Trắng kem
                timerText.raycastTarget = false;
            }
        }

        private void UpdateDisplay()
        {
            if (timerText == null) return;

            var session = PhysicsGameSession.Instance;
            if (session == null)
            {
                timerText.text = "⏱ 30:00";
                return;
            }

            float rem = session.RemainingStudySeconds;
            int mins = Mathf.FloorToInt(rem / 60f);
            int secs = Mathf.FloorToInt(rem % 60f);

            if (session.StudyTimeLimitReached || rem <= 0f)
            {
                timerText.text = "⏱ 00:00 (Hết giờ)";
                timerText.color = new Color(1f, 0.40f, 0.40f, 1f); // Coral red
                if (badgeOutline != null) badgeOutline.effectColor = new Color(1f, 0.35f, 0.35f, 1f);
            }
            else if (rem <= 60f)
            {
                timerText.text = $"⏱ {mins:00}:{secs:00}";
                timerText.color = new Color(1f, 0.75f, 0.30f, 1f); // Amber warning
                if (badgeOutline != null) badgeOutline.effectColor = new Color(1f, 0.70f, 0.25f, 1f);
            }
            else
            {
                timerText.text = $"⏱ {mins:00}:{secs:00}";
                timerText.color = new Color(1f, 0.96f, 0.88f, 1f); // Cream
                if (badgeOutline != null) badgeOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.85f);
            }
        }
    }
}
