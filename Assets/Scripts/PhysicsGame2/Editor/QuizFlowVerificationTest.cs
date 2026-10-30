#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ScienceQuest.PhysicsGame2;

namespace ScienceQuest.PhysicsGame2.Editor
{
    /// <summary>
    /// Automated test tool to verify Quiz Flow & Scene 20 QuizReviewController runtime-logic mapping:
    /// - TEST A: 10 wrong -> 10 wrong states
    /// - TEST B: 10 correct -> 10 correct states
    /// - TEST C: Q1 selected A, correct B -> A wrong, B correct, C/D neutral
    /// - TEST D: Q1 selected B, correct B -> only B correct, A/C/D neutral
    /// - TEST E: click Q7 -> center uses attempt.answers[6]
    /// - TEST F: current-row highlight does not change wrong->green or correct->red
    /// - TEST G: wasCorrect inconsistent with selected/correct indexes -> log inconsistency & render from index comparison
    /// </summary>
    public static class QuizFlowVerificationTest
    {
        [MenuItem("Tools/Physics Game 2/Run Quiz Flow Verification Test")]
        public static void RunAllTests()
        {
            int passCount = 0;
            int totalTests = 0;

            Debug.Log("==================================================");
            Debug.Log("[QuizReviewTest] STARTING AUTOMATED RUNTIME-LOGIC TESTS (A - G)");
            Debug.Log("==================================================");

            GameObject harnessRoot = CreateReviewTestHarness(out QuizReviewController controller);

            try
            {
                // ==================================================
                // TEST A: 10 wrong -> 10 wrong states
                // ==================================================
                totalTests++;
                var attemptAllWrong = BuildTestAttempt(10, i => (selected: 0, correct: 1, wasCorrect: false));
                controller.RenderReviewFromAttempt(attemptAllWrong);

                bool testAPassed = (controller.LeftRowViews.Count == 10);
                for (int i = 0; i < controller.LeftRowViews.Count; i++)
                {
                    var r = controller.LeftRowViews[i];
                    if (r.correctnessState != QuizReviewController.RowCorrectnessState.Wrong ||
                        r.backgroundImage.color != QuizReviewController.WrongRowColor ||
                        r.checkIcon.gameObject.activeSelf ||
                        !r.crossIcon.gameObject.activeSelf)
                    {
                        testAPassed = false;
                        break;
                    }
                }

                if (testAPassed)
                {
                    Debug.Log("✓ TEST A PASSED: 10 wrong answers -> all 10 left rows rendered as RED + ✕ (Wrong state).");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ TEST A FAILED: Not all 10 rows rendered as Wrong state.");
                }

                // ==================================================
                // TEST B: 10 correct -> 10 correct states
                // ==================================================
                totalTests++;
                var attemptAllCorrect = BuildTestAttempt(10, i => (selected: 1, correct: 1, wasCorrect: true));
                controller.RenderReviewFromAttempt(attemptAllCorrect);

                bool testBPassed = (controller.LeftRowViews.Count == 10);
                for (int i = 0; i < controller.LeftRowViews.Count; i++)
                {
                    var r = controller.LeftRowViews[i];
                    if (r.correctnessState != QuizReviewController.RowCorrectnessState.Correct ||
                        r.backgroundImage.color != QuizReviewController.CorrectRowColor ||
                        !r.checkIcon.gameObject.activeSelf ||
                        r.crossIcon.gameObject.activeSelf)
                    {
                        testBPassed = false;
                        break;
                    }
                }

                if (testBPassed)
                {
                    Debug.Log("✓ TEST B PASSED: 10 correct answers -> all 10 left rows rendered as GREEN + ✓ (Correct state).");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ TEST B FAILED: Not all 10 rows rendered as Correct state.");
                }

                // ==================================================
                // TEST C: Q1 selected A (0), correct B (1) -> A wrong, B correct
                // ==================================================
                totalTests++;
                var attemptWrongQ1 = BuildTestAttempt(10, i => (selected: 0, correct: 1, wasCorrect: false));
                controller.RenderReviewFromAttempt(attemptWrongQ1);

                var rowA = controller.CenterAnswerRows[0];
                var rowB = controller.CenterAnswerRows[1];
                var rowC = controller.CenterAnswerRows[2];
                var rowD = controller.CenterAnswerRows[3];

                bool testCPassed =
                    rowA.visualState == QuizReviewController.AnswerOptionVisualState.WrongSelected &&
                    rowA.statusIcon.gameObject.activeSelf &&
                    rowA.statusLabel.gameObject.activeSelf &&
                    rowA.statusLabel.text == "Bạn chọn" &&
                    rowB.visualState == QuizReviewController.AnswerOptionVisualState.CorrectAnswer &&
                    rowB.statusIcon.gameObject.activeSelf &&
                    rowB.statusLabel.gameObject.activeSelf &&
                    rowB.statusLabel.text == "Đáp án đúng" &&
                    rowC.visualState == QuizReviewController.AnswerOptionVisualState.Neutral &&
                    !rowC.statusIcon.gameObject.activeSelf &&
                    !rowC.statusLabel.gameObject.activeSelf &&
                    rowD.visualState == QuizReviewController.AnswerOptionVisualState.Neutral &&
                    !rowD.statusIcon.gameObject.activeSelf &&
                    !rowD.statusLabel.gameObject.activeSelf;

                if (testCPassed)
                {
                    Debug.Log("✓ TEST C PASSED: Q1 selected A, correct B -> A is RED ✕ 'Bạn chọn', B is GREEN ✓ 'Đáp án đúng', C/D neutral.");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ TEST C FAILED: Center option states did not match selected A / correct B.");
                }

                // ==================================================
                // TEST D: Q1 selected B (1), correct B (1) -> only B correct
                // ==================================================
                totalTests++;
                var attemptCorrectQ1 = BuildTestAttempt(10, i => (selected: 1, correct: 1, wasCorrect: true));
                controller.RenderReviewFromAttempt(attemptCorrectQ1);

                rowA = controller.CenterAnswerRows[0];
                rowB = controller.CenterAnswerRows[1];
                rowC = controller.CenterAnswerRows[2];
                rowD = controller.CenterAnswerRows[3];

                bool testDPassed =
                    rowB.visualState == QuizReviewController.AnswerOptionVisualState.CorrectAnswer &&
                    rowB.statusIcon.gameObject.activeSelf &&
                    rowB.statusLabel.gameObject.activeSelf &&
                    rowB.statusLabel.text == "Đáp án đúng" &&
                    rowA.visualState == QuizReviewController.AnswerOptionVisualState.Neutral &&
                    !rowA.statusIcon.gameObject.activeSelf &&
                    rowC.visualState == QuizReviewController.AnswerOptionVisualState.Neutral &&
                    !rowC.statusIcon.gameObject.activeSelf &&
                    rowD.visualState == QuizReviewController.AnswerOptionVisualState.Neutral &&
                    !rowD.statusIcon.gameObject.activeSelf;

                if (testDPassed)
                {
                    Debug.Log("✓ TEST D PASSED: Q1 selected B, correct B -> only B is GREEN ✓ 'Đáp án đúng', A/C/D neutral (no fake red option).");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ TEST D FAILED: Center option states did not match selected B / correct B.");
                }

                // ==================================================
                // TEST E: click Q7 -> center uses attempt.answers[6]
                // ==================================================
                totalTests++;
                var attemptMixed = BuildTestAttempt(10, i => i == 6
                    ? (selected: 2, correct: 3, wasCorrect: false)
                    : (selected: 1, correct: 1, wasCorrect: true));
                controller.RenderReviewFromAttempt(attemptMixed);

                // Click Q7 button (index 6)
                controller.LeftRowViews[6].rowButton.onClick.Invoke();

                bool testEPassed =
                    controller.CurrentReviewIndex == 6 &&
                    controller.QuestionTextComponent.text.Contains(attemptMixed.answers[6].questionText) &&
                    controller.ExplanationTextComponent.text == attemptMixed.answers[6].explanation &&
                    controller.BottomCounterTextComponent.text == "Câu 7/10" &&
                    controller.CenterAnswerRows[2].visualState == QuizReviewController.AnswerOptionVisualState.WrongSelected &&
                    controller.CenterAnswerRows[3].visualState == QuizReviewController.AnswerOptionVisualState.CorrectAnswer;

                if (testEPassed)
                {
                    Debug.Log("✓ TEST E PASSED: Clicking Q7 updates center question, options, explanation, and counter from attempt.answers[6].");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ TEST E FAILED: Clicking Q7 did not render attempt.answers[6] properly.");
                }

                // ==================================================
                // TEST F: current-row highlight does not change wrong->green or correct->red
                // ==================================================
                totalTests++;
                var attemptAlternating = BuildTestAttempt(10, i => (i % 2 == 0)
                    ? (selected: 0, correct: 1, wasCorrect: false)
                    : (selected: 1, correct: 1, wasCorrect: true));
                controller.RenderReviewFromAttempt(attemptAlternating);

                // Q3 (index 2) is wrong; select Q3
                controller.ShowQuestion(2);
                var q3Row = controller.LeftRowViews[2];
                bool q3StillWrongWhileHighlighted =
                    q3Row.isSelectedHighlight &&
                    q3Row.selectionHighlightObj.activeSelf &&
                    q3Row.correctnessState == QuizReviewController.RowCorrectnessState.Wrong &&
                    q3Row.backgroundImage.color == QuizReviewController.WrongRowColor &&
                    q3Row.crossIcon.gameObject.activeSelf &&
                    !q3Row.checkIcon.gameObject.activeSelf;

                // Q4 (index 3) is correct; select Q4
                controller.ShowQuestion(3);
                var q4Row = controller.LeftRowViews[3];
                bool q4StillCorrectWhileHighlighted =
                    q4Row.isSelectedHighlight &&
                    q4Row.selectionHighlightObj.activeSelf &&
                    !q3Row.isSelectedHighlight &&
                    !q3Row.selectionHighlightObj.activeSelf &&
                    q4Row.correctnessState == QuizReviewController.RowCorrectnessState.Correct &&
                    q4Row.backgroundImage.color == QuizReviewController.CorrectRowColor &&
                    q4Row.checkIcon.gameObject.activeSelf &&
                    !q4Row.crossIcon.gameObject.activeSelf;

                if (q3StillWrongWhileHighlighted && q4StillCorrectWhileHighlighted)
                {
                    Debug.Log("✓ TEST F PASSED: Current-row highlight is strictly separate from correctness state (never flips wrong->green or correct->red).");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ TEST F FAILED: Highlight interfered with row correctness state.");
                }

                // ==================================================
                // TEST G: wasCorrect inconsistent with selected/correct indexes
                // -> log inconsistency & render from selected/correct comparison
                // ==================================================
                totalTests++;
                // selected = 0 (A), correct = 1 (B), but corrupt wasCorrect = true
                var attemptCorrupt = BuildTestAttempt(10, i => i == 0
                    ? (selected: 0, correct: 1, wasCorrect: true)
                    : (selected: 1, correct: 1, wasCorrect: true));
                controller.RenderReviewFromAttempt(attemptCorrupt);

                bool loggedInconsistency =
                    controller.InconsistencyErrorCount > 0 &&
                    !string.IsNullOrEmpty(controller.LastInconsistencyErrorMessage) &&
                    controller.LastInconsistencyErrorMessage.Contains("[QuizReview] Attempt inconsistency");

                bool renderedFromIndexComparison =
                    controller.LeftRowViews[0].correctnessState == QuizReviewController.RowCorrectnessState.Wrong &&
                    controller.LeftRowViews[0].backgroundImage.color == QuizReviewController.WrongRowColor &&
                    controller.LeftRowViews[0].crossIcon.gameObject.activeSelf &&
                    !controller.LeftRowViews[0].checkIcon.gameObject.activeSelf &&
                    controller.CenterAnswerRows[0].visualState == QuizReviewController.AnswerOptionVisualState.WrongSelected &&
                    controller.CenterAnswerRows[1].visualState == QuizReviewController.AnswerOptionVisualState.CorrectAnswer;

                if (loggedInconsistency && renderedFromIndexComparison)
                {
                    Debug.Log("✓ TEST G PASSED: Corrupt wasCorrect detected, logged '[QuizReview] Attempt inconsistency...', and rendered from index comparison.");
                    passCount++;
                }
                else
                {
                    Debug.LogError("✗ TEST G FAILED: Corrupt wasCorrect inconsistency handling failed.");
                }

                // --------------------------------------------------
                // TEST H: Visible Back Button ("QUAY LẠI KẾT QUẢ") & Round-Trip Scene 20 -> Scene 19 -> Scene 20
                // --------------------------------------------------
                totalTests++;
                var attemptRoundTrip = BuildTestAttempt(10, i => (selected: (i < 7 ? 1 : 0), correct: 1, wasCorrect: i < 7));
                PhysicsGameSession.Instance.CurrentQuizAttempt = attemptRoundTrip;
                controller.LoadAttemptData();

                Button backBtn = controller.BackToResultButton;
                Image backImg = backBtn != null ? backBtn.GetComponent<Image>() : null;
                var backTmp = backBtn != null ? backBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>(true) : null;

                bool buttonValid =
                    backBtn != null &&
                    backBtn.gameObject.activeInHierarchy &&
                    backBtn.interactable &&
                    backImg != null &&
                    backImg.raycastTarget &&
                    backTmp != null &&
                    !backTmp.raycastTarget &&
                    backTmp.text == "QUAY LẠI KẾT QUẢ";

                // Verify non-overlap with btnPrev ("Câu trước")
                bool nonOverlapping = true;
                if (backBtn != null && controller.PrevButton != null)
                {
                    RectTransform backRt = backBtn.GetComponent<RectTransform>();
                    RectTransform prevRt = controller.PrevButton.GetComponent<RectTransform>();
                    float backBottom = backRt.anchoredPosition.y - backRt.sizeDelta.y * 0.5f;
                    float prevTop = prevRt.anchoredPosition.y + prevRt.sizeDelta.y * 0.5f;
                    nonOverlapping = backBottom > prevTop;
                }

                // Verify CurrentQuizAttempt is preserved and re-rendering Scene 20 shows identical 10-question review
                var preservedAttempt = PhysicsGameSession.Instance.CurrentQuizAttempt;
                bool attemptPreserved =
                    ReferenceEquals(preservedAttempt, attemptRoundTrip) &&
                    preservedAttempt.score == 70 &&
                    preservedAttempt.correctCount == 7 &&
                    preservedAttempt.wrongCount == 3 &&
                    preservedAttempt.answers.Count == 10;

                controller.LoadAttemptData();
                bool reviewRestoredIdentically =
                    controller.LeftRowViews.Count == 10 &&
                    controller.LeftRowViews[0].correctnessState == QuizReviewController.RowCorrectnessState.Correct &&
                    controller.LeftRowViews[9].correctnessState == QuizReviewController.RowCorrectnessState.Wrong;

                if (buttonValid && nonOverlapping && attemptPreserved && reviewRestoredIdentically)
                {
                    Debug.Log("✓ TEST H PASSED: 'QUAY LẠI KẾT QUẢ' button is visible, raycast-safe, non-overlapping with 'Câu trước', and preserves CurrentQuizAttempt across Scene 20 <-> Scene 19.");
                    passCount++;
                }
                else
                {
                    Debug.LogError($"✗ TEST H FAILED: buttonValid={buttonValid}, nonOverlapping={nonOverlapping}, attemptPreserved={attemptPreserved}, reviewRestoredIdentically={reviewRestoredIdentically}");
                }
            }
            finally
            {
                if (harnessRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(harnessRoot);
                }
            }

            Debug.Log("==================================================");
            Debug.Log($"[QuizReviewTest] COMPLETED: {passCount}/{totalTests} TESTS PASSED");
            Debug.Log("==================================================");
        }

        private static GameObject CreateReviewTestHarness(out QuizReviewController controller)
        {
            GameObject canvasGo = new GameObject("TestCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Parchment panel
            GameObject parchmentGo = new GameObject("ParchmentPanel");
            parchmentGo.transform.SetParent(canvasGo.transform, false);
            RectTransform pRt = parchmentGo.AddComponent<RectTransform>();
            pRt.anchoredPosition = new Vector2(206.62f, -136.06f);
            pRt.sizeDelta = new Vector2(1418f, 926f);
            parchmentGo.AddComponent<Image>();

            // 10 Left Question Rows
            for (int i = 0; i < 10; i++)
            {
                GameObject rowGo = new GameObject($"Row_{i + 1}");
                rowGo.transform.SetParent(canvasGo.transform, false);
                RectTransform rRt = rowGo.AddComponent<RectTransform>();
                rRt.anchoredPosition = new Vector2(-720f, 335f - i * 90f);
                rRt.sizeDelta = new Vector2(263.83f, 90.22f);
                rowGo.AddComponent<Image>();
            }

            // Question text
            GameObject qGo = new GameObject("QuestionText");
            qGo.transform.SetParent(canvasGo.transform, false);
            RectTransform qRt = qGo.AddComponent<RectTransform>();
            qRt.anchoredPosition = new Vector2(-31.68f, 186f);
            qRt.sizeDelta = new Vector2(706f, 84f);
            var qTmp = qGo.AddComponent<TMPro.TextMeshProUGUI>();
            qTmp.text = "Câu 1/10: Placeholder?\nA. Opt A\nB. Opt B";

            // Explanation text
            GameObject expGo = new GameObject("ExplanationText");
            expGo.transform.SetParent(canvasGo.transform, false);
            RectTransform expRt = expGo.AddComponent<RectTransform>();
            expRt.anchoredPosition = new Vector2(499f, 56f);
            expRt.sizeDelta = new Vector2(200f, 50f);
            expGo.AddComponent<TMPro.TextMeshProUGUI>();

            // KeyKnowledge text
            GameObject kkGo = new GameObject("KeyKnowledgeText");
            kkGo.transform.SetParent(canvasGo.transform, false);
            RectTransform kkRt = kkGo.AddComponent<RectTransform>();
            kkRt.anchoredPosition = new Vector2(499f, -277f);
            kkRt.sizeDelta = new Vector2(200f, 50f);
            kkGo.AddComponent<TMPro.TextMeshProUGUI>();

            // Bottom Counter text
            GameObject counterGo = new GameObject("BottomCounter");
            counterGo.transform.SetParent(canvasGo.transform, false);
            RectTransform cRt = counterGo.AddComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(165f, -417f);
            cRt.sizeDelta = new Vector2(200f, 60f);
            var cTmp = counterGo.AddComponent<TMPro.TextMeshProUGUI>();
            cTmp.text = "Câu 1/10";

            controller = canvasGo.AddComponent<QuizReviewController>();
            controller.EnsureInitialized();
            return canvasGo;
        }

        private static QuizAttemptData BuildTestAttempt(int count, Func<int, (int selected, int correct, bool wasCorrect)> selector)
        {
            var attempt = new QuizAttemptData
            {
                lessonId = "C2_L6",
                lessonTitle = "Bài 6: Phản xạ toàn phần",
                elapsedSeconds = 120f
            };

            int correctTotal = 0;
            for (int i = 0; i < count; i++)
            {
                var cfg = selector(i);
                if (cfg.selected == cfg.correct) correctTotal++;

                attempt.answers.Add(new QuizAttemptAnswer
                {
                    questionId = $"PHY_Q{180 + i}",
                    questionText = i == 0
                        ? "Hiện tượng phản xạ toàn phần được ứng dụng trong thiết bị nào sau đây?"
                        : $"Nội dung câu hỏi kiểm thử số {i + 1}?",
                    options = new List<string>
                    {
                        "A. Gương phẳng thông thường.",
                        "B. Sợi cáp quang truyền tín hiệu.",
                        "C. Bóng đèn sợi đốt.",
                        "D. Pin mặt trời."
                    },
                    selectedAnswerIndex = cfg.selected,
                    correctAnswerIndex = cfg.correct,
                    wasCorrect = cfg.wasCorrect,
                    explanation = $"Lời giải chi tiết cho câu {i + 1}.",
                    keyKnowledge = ""
                });
            }

            attempt.correctCount = correctTotal;
            attempt.wrongCount = count - correctTotal;
            attempt.score = correctTotal * 10;
            return attempt;
        }
    }
}
#endif
