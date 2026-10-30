with open('Assets/Scripts/PhysicsGame2/UI/EssayResultController.cs', 'r', encoding='utf-8') as f:
    c = f.read()

# Replace AutoBindUIReferences and SetupButtonListeners and RenderAttemptResult
old_block = '''        private void AutoBindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                string text = t.text != null ? t.text.Trim() : "";

                // Xóa số 8 rời hoặc text mẫu
                if (text == "8" || text == "80" || text == "80 điểm" || text == "Điểm: 80")
                {
                    scoreText = t;
                }
                else if (text.Contains("Chưa đủ năng lượng") || text.Contains("Đủ năng lượng") || text.Contains("Hoàn thành") || text.Contains("Chúc mừng"))
                {
                    statusText = t;
                }
                else if (text.Contains("câu đúng") || text.Contains("câu đạt") || text.Contains("thời gian"))
                {
                    detailSummaryText = t;
                }
            }

            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string bText = "";
                var tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null && tmp.text != null) bText = tmp.text.ToLowerInvariant();

                if (bText.Contains("khu vực") || bText.Contains("bài học") || bText.Contains("quay lại") || bText.Contains("tiếp tục"))
                {
                    btnReturnToMap = b;
                }
                else if (bText.Contains("chi tiết") || bText.Contains("lời giải") || bText.Contains("đối chiếu"))
                {
                    btnViewDetail = b;
                }
                else if (bText.Contains("làm lại") || bText.Contains("thử lại"))
                {
                    btnRetry = b;
                }
            }

            // Tạo rubricNoticeText nếu chưa có
            if (rubricNoticeText == null)
            {
                GameObject noticeGo = new GameObject("RubricNoticeText");
                noticeGo.transform.SetParent(canvas.transform, false);
                rubricNoticeText = noticeGo.AddComponent<TextMeshProUGUI>();
                rubricNoticeText.fontSize = 20;
                rubricNoticeText.fontStyle = FontStyles.Bold;
                rubricNoticeText.alignment = TextAlignmentOptions.Center;
                rubricNoticeText.raycastTarget = false;

                RectTransform rt = noticeGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, 160);
                rt.sizeDelta = new Vector2(700, 40);
            }
        }

        private void SetupButtonListeners()
        {
            if (btnReturnToMap != null)
            {
                UIHoverPulseEffect.Attach(btnReturnToMap.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnReturnToMap.onClick.RemoveAllListeners();
                btnReturnToMap.onClick.AddListener(OnReturnToMapClicked);
            }

            if (btnViewDetail != null)
            {
                UIHoverPulseEffect.Attach(btnViewDetail.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnViewDetail.onClick.RemoveAllListeners();
                btnViewDetail.onClick.AddListener(OnViewDetailClicked);
            }

            if (btnRetry != null)
            {
                UIHoverPulseEffect.Attach(btnRetry.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnRetry.onClick.RemoveAllListeners();
                btnRetry.onClick.AddListener(OnRetryClicked);
            }
        }

        private void RenderAttemptResult()
        {
            var gradingResult = NavigationService.Instance.AttemptContext.GradingResult;
            float score = NavigationService.Instance.AttemptContext.Score;

            if (gradingResult != null)
            {
                score = gradingResult.totalScore;
            }
            else if (score <= 0f)
            {
                // Fallback nếu chạy test trực tiếp Scene 19
                score = 85.0f;
            }

            if (scoreText != null)
            {
                if (score <= 10f)
                {
                    scoreText.text = $"{score:F1}/10";
                }
                else
                {
                    scoreText.text = $"{score / 10f:F1}/10 ({score:F1}/100)";
                }
            }

            bool passed = (score <= 10f) ? (score >= 8.0f) : (score >= RestorationService.PASS_THRESHOLD);
            bool isDraft = (gradingResult != null && !gradingResult.rubricApproved);

            if (statusText != null)
            {
                if (passed)
                {
                    statusText.text = isDraft
                        ? "ĐẠT YÊU CẦU (CHẾ ĐỘ THỬ)"
                        : "ĐỦ NĂNG LƯỢNG KHÔI PHỤC!";
                    statusText.color = new Color(0.2f, 0.9f, 0.4f, 1f);
                }
                else
                {
                    statusText.text = "CHƯA ĐỦ NĂNG LƯỢNG — HÃY LUYỆN THÊM";
                    statusText.color = new Color(0.95f, 0.4f, 0.35f, 1f);
                }
            }

            if (detailSummaryText != null)
            {
                detailSummaryText.text = passed
                    ? "1/1 câu tự luận đạt yêu cầu (từ 8/10 điểm)"
                    : "0/1 câu tự luận đạt yêu cầu. Cần bổ sung các bước lập luận.";
            }

            if (rubricNoticeText != null)
            {
                if (isDraft)
                {
                    rubricNoticeText.text = "<color=#f59e0b>⚠ Chấm thử — rubric chưa duyệt chính thức</color>";
                    rubricNoticeText.gameObject.SetActive(true);
                }
                else
                {
                    rubricNoticeText.text = "";
                    rubricNoticeText.gameObject.SetActive(false);
                }
            }
        }'''

new_block = '''        private void AutoBindUIReferences()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tự động liên kết các nhãn văn bản theo nội dung và tọa độ
            TextMeshProUGUI[] tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                string text = t.text != null ? t.text.Trim() : "";

                // Xóa số 8 rời bên phải bảng điểm nếu có
                if (pos.x > 400f && pos.x < 650f && Mathf.Abs(pos.y - -10f) < 30f && text == "8")
                {
                    t.text = "";
                    t.gameObject.SetActive(false);
                    continue;
                }

                // Điểm số: Ô số nằm trên khung điểm (y trong khoảng [150, 280], x trong khoảng [20, 160])
                if (scoreText == null && pos.y > 150f && pos.y < 280f && pos.x > 20f && pos.x < 160f)
                {
                    scoreText = t;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 54f;
                    t.fontStyle = FontStyles.Bold;
                    t.color = new Color(1f, 0.95f, 0.70f, 1f);
                    t.raycastTarget = false;
                }
                // Trạng thái khôi phục: ở giữa khung bảng điểm (pos y ~ 0)
                else if (statusText == null && Mathf.Abs(pos.y) < 40f && Mathf.Abs(pos.x - 45f) < 60f)
                {
                    statusText = t;
                    t.alignment = TextAlignmentOptions.Center;
                    t.fontSize = 22f;
                    t.fontStyle = FontStyles.Bold;
                    t.raycastTarget = false;
                }
                // Khung tóm tắt (hoặc câu đúng/đạt): bên dưới
                else if (detailSummaryText == null && (text.Contains("câu đúng") || text.Contains("câu đạt") || text.Contains("thời gian") || (pos.y < -150f && pos.y > -250f && pos.x < -200f)))
                {
                    detailSummaryText = t;
                    t.raycastTarget = false;
                }
            }

            // 2. Tự động liên kết 3 nút theo tọa độ anchoredPosition (hỗ trợ hoàn hảo sprite-button không có child TMP)
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string bText = "";
                var tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null && tmp.text != null) bText = tmp.text.ToLowerInvariant();

                if (bText.Contains("khu vực") || bText.Contains("bài học") || bText.Contains("quay lại") || bText.Contains("tiếp tục"))
                {
                    btnReturnToMap = b;
                    continue;
                }
                else if (bText.Contains("chi tiết") || bText.Contains("lời giải") || bText.Contains("đối chiếu"))
                {
                    btnViewDetail = b;
                    continue;
                }
                else if (bText.Contains("làm lại") || bText.Contains("thử lại"))
                {
                    btnRetry = b;
                    continue;
                }

                // Nhận diện theo tọa độ X của hàng nút dưới cùng (pos.y < -350f)
                Vector2 pos = b.GetComponent<RectTransform>().anchoredPosition;
                if (pos.y < -350f)
                {
                    if (pos.x < -300f && btnReturnToMap == null)
                    {
                        btnReturnToMap = b; // Nút bên trái: Về khu vực
                    }
                    else if (pos.x >= -200f && pos.x <= 250f && btnViewDetail == null)
                    {
                        btnViewDetail = b; // Nút ở giữa: Xem lời giải chi tiết
                    }
                    else if (pos.x > 250f && btnRetry == null)
                    {
                        btnRetry = b; // Nút bên phải: Làm lại
                    }
                }
            }

            // Tạo rubricNoticeText nếu chưa có
            if (rubricNoticeText == null)
            {
                GameObject noticeGo = new GameObject("RubricNoticeText");
                noticeGo.transform.SetParent(canvas.transform, false);
                rubricNoticeText = noticeGo.AddComponent<TextMeshProUGUI>();
                rubricNoticeText.fontSize = 20;
                rubricNoticeText.fontStyle = FontStyles.Bold;
                rubricNoticeText.alignment = TextAlignmentOptions.Center;
                rubricNoticeText.raycastTarget = false;

                RectTransform rt = noticeGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, 160);
                rt.sizeDelta = new Vector2(700, 40);
            }
        }

        private void SetupButtonListeners()
        {
            if (btnReturnToMap != null)
            {
                UIHoverPulseEffect.Attach(btnReturnToMap.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnReturnToMap.onClick.RemoveAllListeners();
                btnReturnToMap.onClick.AddListener(OnReturnToMapClicked);
            }

            if (btnViewDetail != null)
            {
                UIHoverPulseEffect.Attach(btnViewDetail.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnViewDetail.onClick.RemoveAllListeners();
                btnViewDetail.onClick.AddListener(OnViewDetailClicked);
            }

            if (btnRetry != null)
            {
                UIHoverPulseEffect.Attach(btnRetry.gameObject, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.08f, outline: true);
                btnRetry.onClick.RemoveAllListeners();
                btnRetry.onClick.AddListener(OnRetryClicked);
            }
        }

        private void RenderAttemptResult()
        {
            var gradingResult = NavigationService.Instance.AttemptContext.GradingResult;
            float score10 = NavigationService.Instance.AttemptContext.Score;
            float score100 = NavigationService.Instance.AttemptContext.ScoreRaw;

            if (gradingResult != null)
            {
                score10 = (gradingResult.totalScore <= 10f) ? (float)Math.Round(gradingResult.totalScore, 2) : (float)Math.Round(gradingResult.totalScore / 10f, 2);
                score100 = (float)Math.Round(score10 * 10.0, 1);
            }
            else if (score10 <= 0f && score100 <= 0f)
            {
                // Fallback nếu chạy test trực tiếp Scene 19.1
                score10 = 8.5f;
                score100 = 85.0f;
            }

            if (scoreText != null)
            {
                scoreText.text = $"{score10:F1}/10";
            }

            // Chuẩn hóa quy đổi điểm: 8.0/10 tương đương chính xác 80/100
            bool passed = (score10 >= 8.0f - 1e-4f) || (score100 >= RestorationService.PASS_THRESHOLD - 1e-4f);
            bool isDraft = (gradingResult != null && !gradingResult.rubricApproved);

            if (statusText != null)
            {
                if (passed)
                {
                    statusText.text = isDraft
                        ? "ĐẠT YÊU CẦU (CHẾ ĐỘ THỬ)"
                        : "ĐỦ NĂNG LƯỢNG KHÔI PHỤC!";
                    statusText.color = new Color(0.2f, 0.9f, 0.4f, 1f);
                }
                else
                {
                    statusText.text = "CHƯA ĐỦ NĂNG LƯỢNG — HÃY LUYỆN THÊM";
                    statusText.color = new Color(0.95f, 0.4f, 0.35f, 1f);
                }
            }

            if (detailSummaryText != null)
            {
                detailSummaryText.text = passed
                    ? $"1/1 câu tự luận đạt ({score10:F1}/10 đ ~ {score100:F0}/100 đ)"
                    : $"0/1 câu tự luận đạt ({score10:F1}/10 đ, cần ≥ 8.0/10 để khôi phục)";
            }

            if (rubricNoticeText != null)
            {
                if (isDraft)
                {
                    rubricNoticeText.text = "<color=#f59e0b>⚠ Chấm thử — rubric chưa duyệt chính thức</color>";
                    rubricNoticeText.gameObject.SetActive(true);
                }
                else
                {
                    rubricNoticeText.text = "";
                    rubricNoticeText.gameObject.SetActive(false);
                }
            }
        }'''

c_norm = c.replace('\r\n', '\n')
old_norm = old_block.replace('\r\n', '\n')
new_norm = new_block.replace('\r\n', '\n')

if old_norm in c_norm:
    res = c_norm.replace(old_norm, new_norm)
    with open('Assets/Scripts/PhysicsGame2/UI/EssayResultController.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(res)
    print('SUCCESS')
else:
    print('OLD BLOCK NOT FOUND')
