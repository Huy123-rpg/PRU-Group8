using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PRU.Biology
{
    public class MazeEssayQuestions : MonoBehaviour
    {
        [TextArea(2, 3)] public string googleSheetCsvUrl;
        [Header("Boss1 đến Boss5, không kéo Gate vào đây")]
        public MazeBossGate[] bosses = new MazeBossGate[5];
        [Header("Chữ hướng dẫn trong MazeCanvas")]
        public TMP_Text statusText;
        public bool Ready { get; private set; }

        private IEnumerator Start()
        {
            if (bosses == null || bosses.Length != 5)
            {
                Report("Cần gán đúng 5 boss trong MazeEssayQuestions.");
                yield break;
            }
            HashSet<MazeBossGate> assigned = new HashSet<MazeBossGate>();
            for (int i = 0; i < bosses.Length; i++)
            {
                if (bosses[i] == null || !assigned.Add(bosses[i]) || bosses[i].Passed)
                {
                    Report("Danh sách boss còn trống, trùng hoặc đã được sử dụng.");
                    yield break;
                }
                bosses[i].bossNumber = i + 1;
                bosses[i].questionID = "";
                bosses[i].questionText = "";
            }

            if (string.IsNullOrWhiteSpace(GameSessionData.SelectedChapterID)
                || string.IsNullOrWhiteSpace(GameSessionData.SelectedLessonID))
            {
                Report("Hãy vào mê cung từ MainMenu → chọn chương → chọn bài → Tự luận.");
                yield break;
            }
            // Avoid the original bank's null subject comparison when loading cache.
            if (string.IsNullOrWhiteSpace(GameSessionData.SelectedSubject))
                GameSessionData.SelectedSubject = "Biology";
            GameSessionData.SelectedQuestionType = QuestionType.EssayImage;

            Report("Đang chuẩn bị câu tự luận cho " + GameSessionData.SelectedLessonID + "…");
            BiologyQuestionBank bank = BiologyQuestionBank.Instance;
            if (bank == null)
            {
                GameObject bankObject = new GameObject("BiologyQuestionBank");
                bank = bankObject.AddComponent<BiologyQuestionBank>();
                bank.fetchOnStart = false;
            }

            string url = string.IsNullOrWhiteSpace(googleSheetCsvUrl)
                ? bank.googleSheetCsvUrl : googleSheetCsvUrl.Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                bank.googleSheetCsvUrl = url;
                // Timeout belongs to this request only; cached questions remain usable.
                yield return StartCoroutine(FetchWithTimeout(bank, url));
            }

            bank.BuildPoolForCurrentSession();
            BiologyDifficulty[] preferred = {
                BiologyDifficulty.De, BiologyDifficulty.TrungBinh,
                BiologyDifficulty.TrungBinh, BiologyDifficulty.Kho, BiologyDifficulty.Boss
            };
            List<BiologyQuestion> selected = new List<BiologyQuestion>();
            for (int i = 0; i < 5; i++)
            {
                BiologyQuestion question = bank.GetRandomQuestionWithFallback(preferred[i]);
                if (question == null || question.questionType != QuestionType.EssayImage
                    || string.IsNullOrWhiteSpace(question.questionID)
                    || string.IsNullOrWhiteSpace(question.questionText))
                {
                    Report("Bài " + GameSessionData.SelectedLessonID +
                        " chưa có câu tự luận hợp lệ. Thêm câu LoaiCauHoi = TuLuan vào Sheet.");
                    yield break;
                }
                selected.Add(question);
            }

            HashSet<string> unique = new HashSet<string>();
            for (int i = 0; i < 5; i++)
            {
                bosses[i].questionID = selected[i].questionID;
                bosses[i].questionText = selected[i].questionText;
                unique.Add(selected[i].questionID);
            }
            Ready = true;
            string message = "Đến gần người gác cổng, nhấn E để trả lời.";
            if (unique.Count < 5)
                message += " Bài hiện có ít hơn 5 câu khác nhau nên một số boss lặp câu.";
            Report(message);
        }

        private IEnumerator FetchWithTimeout(BiologyQuestionBank bank, string url)
        {
            bool completed = false;
            Coroutine fetch = StartCoroutine(Fetch(bank, url, () => completed = true));
            float deadline = Time.realtimeSinceStartup + 30f;
            while (!completed && Time.realtimeSinceStartup < deadline) yield return null;
            if (!completed)
            {
                StopCoroutine(fetch);
                Debug.LogWarning("Tải câu hỏi quá lâu; dùng cache nếu có.");
            }
        }

        private IEnumerator Fetch(BiologyQuestionBank bank, string url, System.Action finished)
        {
            yield return bank.FetchFromGoogleSheet(url);
            finished();
        }

        private void Report(string message)
        {
            Debug.Log("[EssayMaze] " + message);
            if (statusText != null)
            {
                FantasyUIFont.Apply(statusText);
                statusText.text = message;
            }
        }
    }
}
