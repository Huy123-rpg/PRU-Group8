using UnityEngine;
using TMPro;
using System;

namespace ScienceQuest.Quiz
{
    /// <summary>
    /// Quản lý thời gian cho các bài kiểm tra/trắc nghiệm (Quiz)
    /// </summary>
    public class QuizTimer : MonoBehaviour
    {
        [SerializeField] private float totalTime = 900f; // 15 phút mặc định
        private float remainingTime;
        private bool isRunning = false;
        
        [SerializeField] private TextMeshProUGUI timerText;

        // Sự kiện khi hết thời gian
        public event Action OnTimeUp;

        private Color normalColor = Color.white;
        private Color orangeColor = new Color(1f, 0.5f, 0f); // Màu cam
        private Color redColor = Color.red;

        // Scale ban đầu của text để làm hiệu ứng nhấp nháy
        private Vector3 originalScale = Vector3.one;

        private void Start()
        {
            if (timerText != null)
            {
                originalScale = timerText.transform.localScale;
                normalColor = timerText.color;
            }
        }

        private void Update()
        {
            if (!isRunning) return;

            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isRunning = false;
                UpdateTimerUI();
                OnTimeUp?.Invoke(); // Gọi sự kiện hết giờ
                return;
            }

            UpdateTimerUI();
        }

        /// <summary>
        /// Bắt đầu đếm ngược thời gian
        /// </summary>
        public void StartTimer(float duration)
        {
            totalTime = duration;
            remainingTime = duration;
            isRunning = true;
            UpdateTimerUI();
        }

        /// <summary>
        /// Dừng/Tạm dừng thời gian
        /// </summary>
        public void StopTimer()
        {
            isRunning = false;
        }

        /// <summary>
        /// Tiếp tục đếm thời gian
        /// </summary>
        public void ResumeTimer()
        {
            if (remainingTime > 0)
            {
                isRunning = true;
            }
        }

        /// <summary>
        /// Đặt lại thời gian
        /// </summary>
        public void ResetTimer(float duration)
        {
            totalTime = duration;
            remainingTime = duration;
            isRunning = false;
            
            // Đặt lại màu sắc và kích thước
            if (timerText != null)
            {
                timerText.color = normalColor;
                timerText.transform.localScale = originalScale;
            }
            
            UpdateTimerUI();
        }

        /// <summary>
        /// Lấy thời gian còn lại (giây)
        /// </summary>
        public float GetRemainingTime()
        {
            return remainingTime;
        }

        /// <summary>
        /// Lấy chuỗi định dạng thời gian MM:SS
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        /// <summary>
        /// Gán UI text cho timer
        /// </summary>
        public void SetTimerText(TextMeshProUGUI text)
        {
            timerText = text;
            if (timerText != null)
            {
                originalScale = timerText.transform.localScale;
                // Nếu muốn giữ màu text ban đầu: normalColor = timerText.color;
            }
        }

        private void UpdateTimerUI()
        {
            if (timerText == null) return;

            // Cập nhật text MM:SS
            timerText.text = GetFormattedTime();

            // Cập nhật màu sắc và hiệu ứng dựa trên thời gian còn lại
            if (remainingTime <= 10f)
            {
                timerText.color = redColor;
                // Hiệu ứng phóng to thu nhỏ (pulse) khi còn dưới 10 giây
                float scale = 1f + Mathf.PingPong(Time.time * 2f, 0.2f); // Phóng to thêm tối đa 20%
                timerText.transform.localScale = originalScale * scale;
            }
            else if (remainingTime <= 30f)
            {
                timerText.color = orangeColor; // Còn 30 giây -> đổi sang cam
                timerText.transform.localScale = originalScale;
            }
            else
            {
                timerText.color = normalColor;
                timerText.transform.localScale = originalScale;
            }
        }
    }
}
