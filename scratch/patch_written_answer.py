with open('Assets/Scripts/PhysicsGame2/UI/WrittenAnswerController.cs', 'r', encoding='utf-8') as f:
    c = f.read()

# 1. Update modal buttons: center the Continue button, do not reveal model solution in hint modal
old_modal_buttons = '''            // 2 Nút hành động
            // 1. Nút "ĐÃ HIỂU — LÀM TIẾP"
            GameObject btnContinueGo = new GameObject("Btn_TutorContinue");
            btnContinueGo.transform.SetParent(card.transform, false);
            RectTransform bcRt = btnContinueGo.AddComponent<RectTransform>();
            bcRt.anchoredPosition = new Vector2(-150, -220);
            bcRt.sizeDelta = new Vector2(250, 44);
            Image bcImg = btnContinueGo.AddComponent<Image>();
            bcImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);
            btnTutorDismiss = btnContinueGo.AddComponent<Button>();
            btnTutorDismiss.targetGraphic = bcImg;
            GameObject bcTxtGo = new GameObject("Text");
            bcTxtGo.transform.SetParent(btnContinueGo.transform, false);
            var bcTmp = bcTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) bcTmp.font = _defaultFont;
            bcTmp.text = "✓ ĐÃ HIỂU — LÀM TIẾP";
            bcTmp.fontSize = 16;
            bcTmp.fontStyle = FontStyles.Bold;
            bcTmp.alignment = TextAlignmentOptions.Center;
            bcTmp.color = Color.white;
            bcTmp.raycastTarget = false;
            btnTutorDismiss.onClick.AddListener(() => tutorModalRoot.SetActive(false));
            UIHoverPulseEffect.Attach(btnContinueGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);

            // 2. Nút "XEM ĐÁP ÁN MẪU" (Chỉ hiện khi học sinh yêu cầu)
            GameObject btnSolGo = new GameObject("Btn_TutorModelSol");
            btnSolGo.transform.SetParent(card.transform, false);
            RectTransform bsRt = btnSolGo.AddComponent<RectTransform>();
            bsRt.anchoredPosition = new Vector2(150, -220);
            bsRt.sizeDelta = new Vector2(250, 44);
            Image bsImg = btnSolGo.AddComponent<Image>();
            bsImg.color = new Color(0.20f, 0.35f, 0.55f, 1f);
            btnTutorShowModelSol = btnSolGo.AddComponent<Button>();
            btnTutorShowModelSol.targetGraphic = bsImg;
            GameObject bsTxtGo = new GameObject("Text");
            bsTxtGo.transform.SetParent(btnSolGo.transform, false);
            var bsTmp = bsTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) bsTmp.font = _defaultFont;
            bsTmp.text = "📖 XEM ĐÁP ÁN MẪU";
            bsTmp.fontSize = 16;
            bsTmp.fontStyle = FontStyles.Bold;
            bsTmp.alignment = TextAlignmentOptions.Center;
            bsTmp.color = new Color(1f, 0.92f, 0.70f, 1f);
            bsTmp.raycastTarget = false;
            UIHoverPulseEffect.Attach(btnSolGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);'''

new_modal_buttons = '''            // Nút "ĐÃ HIỂU GỢI Ý — TIẾP TỤC LÀM" (Không tiết lộ toàn bộ lời giải theo yêu cầu sư phạm)
            GameObject btnContinueGo = new GameObject("Btn_TutorContinue");
            btnContinueGo.transform.SetParent(card.transform, false);
            RectTransform bcRt = btnContinueGo.AddComponent<RectTransform>();
            bcRt.anchoredPosition = new Vector2(0, -220);
            bcRt.sizeDelta = new Vector2(300, 46);
            Image bcImg = btnContinueGo.AddComponent<Image>();
            bcImg.color = new Color(0.15f, 0.65f, 0.35f, 1f);
            btnTutorDismiss = btnContinueGo.AddComponent<Button>();
            btnTutorDismiss.targetGraphic = bcImg;
            GameObject bcTxtGo = new GameObject("Text");
            bcTxtGo.transform.SetParent(btnContinueGo.transform, false);
            var bcTmp = bcTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) bcTmp.font = _defaultFont;
            bcTmp.text = "✓ ĐÃ HIỂU GỢI Ý — TIẾP TỤC LÀM";
            bcTmp.fontSize = 16;
            bcTmp.fontStyle = FontStyles.Bold;
            bcTmp.alignment = TextAlignmentOptions.Center;
            bcTmp.color = Color.white;
            bcTmp.raycastTarget = false;
            btnTutorDismiss.onClick.AddListener(() => tutorModalRoot.SetActive(false));
            UIHoverPulseEffect.Attach(btnContinueGo, hover: 1.06f, minPulse: 1.03f, maxPulse: 1.07f, outline: true);'''

# 2. Update OnSubmitClicked to standardize score rounding (8.0/10 <-> 80/100)
old_submit = '''                // Điểm theo thang 10 chuẩn sư phạm giảng viên
                float scoreScale10 = (result.totalScore <= 10f) ? result.totalScore : (result.totalScore / 10f);
                float scoreScale100 = scoreScale10 * 10f;

                // Lưu kết quả vào AttemptContext
                NavigationService.Instance.AttemptContext.AttemptId = attemptId;
                NavigationService.Instance.AttemptContext.GradingResult = result;
                NavigationService.Instance.AttemptContext.Score = scoreScale10; // 0..10
                NavigationService.Instance.AttemptContext.ScoreRaw = scoreScale100; // 0..100
                NavigationService.Instance.AttemptContext.SubmittedAnswer = currentAnswer;

                if (scoreScale100 >= RestorationService.PASS_THRESHOLD)
                {
                    if (result.rubricApproved)
                    {
                        RestorationService.RecordAttemptResult(deviceId, lessonId, scoreScale100, attemptId);
                    }
                    else
                    {
                        Debug.Log("[WrittenAnswerController] Điểm >= 80 nhưng Rubric đang DRAFT -> Chấm thử, chưa cấp khôi phục chính thức.");
                    }
                }

                // Chuyển sang màn kết quả Tự luận: Scene 19. Result_TN (tiêu đề TỰ LUẬN)
                NavigationService.Instance.NavigateTo(RouteId.EssayResult);'''

new_submit = '''                // Chuẩn hóa quy đổi điểm: 8.0/10 tương đương chính xác 80/100
                float scoreScale10 = (result.totalScore <= 10f) ? (float)Math.Round(result.totalScore, 2) : (float)Math.Round(result.totalScore / 10f, 2);
                float scoreScale100 = (float)Math.Round(scoreScale10 * 10.0, 1);

                // Lưu kết quả vào AttemptContext
                NavigationService.Instance.AttemptContext.AttemptId = attemptId;
                NavigationService.Instance.AttemptContext.GradingResult = result;
                NavigationService.Instance.AttemptContext.Score = scoreScale10; // 0..10
                NavigationService.Instance.AttemptContext.ScoreRaw = scoreScale100; // 0..100
                NavigationService.Instance.AttemptContext.SubmittedAnswer = currentAnswer;

                bool isPassed = (scoreScale10 >= 8.0f - 1e-4f) || (scoreScale100 >= RestorationService.PASS_THRESHOLD - 1e-4f);
                if (isPassed)
                {
                    if (result.rubricApproved)
                    {
                        RestorationService.RecordAttemptResult(deviceId, lessonId, scoreScale100, attemptId);
                        Debug.Log($"[WrittenAnswerController] Đạt điểm khôi phục thiết bị {deviceId}: {scoreScale10:F1}/10 ({scoreScale100:F0}/100)");
                    }
                    else
                    {
                        Debug.Log("[WrittenAnswerController] Điểm >= 8.0/10 nhưng Rubric đang DRAFT -> Chấm thử, chưa cấp khôi phục chính thức.");
                    }
                }

                // Chuyển sang màn kết quả Tự luận: Scene 19.1. Result_TL (RouteId.EssayResult)
                NavigationService.Instance.NavigateTo(RouteId.EssayResult);'''

c_norm = c.replace('\r\n', '\n')
m1_norm = old_modal_buttons.replace('\r\n', '\n')
m2_norm = new_modal_buttons.replace('\r\n', '\n')
s1_norm = old_submit.replace('\r\n', '\n')
s2_norm = new_submit.replace('\r\n', '\n')

ok1 = m1_norm in c_norm
ok2 = s1_norm in c_norm
print(f'Match modal: {ok1}, Match submit: {ok2}')

if ok1 and ok2:
    res = c_norm.replace(m1_norm, m2_norm).replace(s1_norm, s2_norm)
    with open('Assets/Scripts/PhysicsGame2/UI/WrittenAnswerController.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(res)
    print('SUCCESS WrittenAnswerController')
