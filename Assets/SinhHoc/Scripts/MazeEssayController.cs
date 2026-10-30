using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PRU.Biology
{
    public class MazeEssayController : MonoBehaviour
    {
        [Header("Đối tượng Scene đã dựng")]
        public MazePlayerController player;
        public AIGradingClient gradingClient;
        public GameObject essayPanel;
        public TMP_Text bossTitle;
        public TMP_Text questionText;
        public TMP_InputField answerInput;
        public GameObject uploadArea;
        public RawImage imagePreview;
        [Header("Nút đã dựng")]
        public Button btnTyping;
        public Button btnUpload;
        public Button btnChooseImage;
        public Button btnSubmit;
        public Button btnClose;
        [Header("Chữ nhắc trong MazeCanvas (có thể thêm sau)")]
        public TMP_Text interactionHint;

        public int NextBossNumber { get; private set; } = 1;
        public bool IsOpen => essayPanel != null && essayPanel.activeSelf;
        private enum State { Editing, PickingImage, Grading, Failed, Passed, Error }
        private State state;
        private bool imageMode;
        private bool ready;
        private MazeBossGate currentBoss;
        private MazeBossGate nearbyBoss;
        private int requestVersion;

        private class Draft
        {
            public string text = "";
            public string imageBase64;
            public Sprite preview;
        }
        private readonly Dictionary<MazeBossGate, Draft> drafts =
            new Dictionary<MazeBossGate, Draft>();

        private void Awake()
        {
            ready = player != null && essayPanel != null && bossTitle != null
                && questionText != null && answerInput != null && uploadArea != null
                && imagePreview != null && btnTyping != null && btnUpload != null
                && btnChooseImage != null && btnSubmit != null && btnClose != null;
            if (!ready)
            {
                Debug.LogError("MazeEssayController: còn ô tham chiếu chưa gán trong Inspector.", this);
                return;
            }

            foreach (TMP_Text text in essayPanel.GetComponentsInChildren<TMP_Text>(true))
                FantasyUIFont.Apply(text);
            // Student text and AI feedback must not be interpreted as TMP markup.
            answerInput.richText = false;
            questionText.richText = false;
            bossTitle.richText = false;
            answerInput.lineType = TMP_InputField.LineType.MultiLineNewline;
            answerInput.characterLimit = 5000;
            ReplaceClick(btnTyping, UseTyping);
            ReplaceClick(btnUpload, UseImage);
            ReplaceClick(btnChooseImage, ChooseImage);
            ReplaceClick(btnSubmit, Submit);
            ReplaceClick(btnClose, Close);
            essayPanel.SetActive(false);
            if (interactionHint != null)
            {
                FantasyUIFont.Apply(interactionHint);
                interactionHint.text = "Đến gần người gác cổng, nhấn E để trò chuyện.";
            }
        }

        private static void ReplaceClick(Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
        }

        private Draft CurrentDraft()
        {
            if (!drafts.TryGetValue(currentBoss, out Draft draft))
            {
                draft = new Draft();
                drafts.Add(currentBoss, draft);
            }
            return draft;
        }

        private void SaveTypedDraft()
        {
            if (currentBoss != null && state == State.Editing && !imageMode)
                CurrentDraft().text = answerInput.text;
        }

        public void SetNearbyBoss(MazeBossGate boss)
        {
            nearbyBoss = boss;
            UpdateHint();
        }

        public void ClearNearbyBoss(MazeBossGate boss)
        {
            if (nearbyBoss == boss) nearbyBoss = null;
            UpdateHint();
        }

        private void UpdateHint()
        {
            if (interactionHint == null) return;
            if (NextBossNumber > 5)
                interactionHint.text = "Đã vượt 5 người gác cổng. Hãy tìm cửa thoát!";
            else if (nearbyBoss != null && !nearbyBoss.Passed)
                interactionHint.text = nearbyBoss.bossNumber == NextBossNumber
                    ? "Nhấn E: " + nearbyBoss.bossName
                    : "Hãy vượt cổng " + NextBossNumber + " trước.";
            else interactionHint.text = "Đến gần người gác cổng, nhấn E để trò chuyện.";
        }

        public void OpenBoss(MazeBossGate boss)
        {
            if (!ready || IsOpen || boss == null || boss.Passed
                || boss.bossNumber != NextBossNumber) return;
            requestVersion++;
            currentBoss = boss;
            imageMode = false;
            state = State.Editing;
            bossTitle.text = boss.bossName + " — CỔNG " + boss.bossNumber + "/5";
            essayPanel.SetActive(true);
            player.SetInputLocked(true);
            ShowEditor();
        }

        private bool HasQuestion => currentBoss != null
            && !string.IsNullOrWhiteSpace(currentBoss.questionID)
            && !string.IsNullOrWhiteSpace(currentBoss.questionText);

        private void ShowEditor()
        {
            state = State.Editing;
            answerInput.characterLimit = 5000;
            questionText.text = HasQuestion ? currentBoss.questionText
                : "Chưa gán câu hỏi cho boss. Cần nối ngân hàng câu hỏi trước khi nộp bài.";
            answerInput.readOnly = false;
            answerInput.text = CurrentDraft().text;
            answerInput.gameObject.SetActive(!imageMode);
            uploadArea.SetActive(imageMode);
            imagePreview.texture = CurrentDraft().preview != null
                ? CurrentDraft().preview.texture : null;
            FitPreview();
            SetModeButtons(true);
            btnChooseImage.interactable = true;
            btnSubmit.interactable = HasQuestion;
            SetButtonText(btnSubmit, "NỘP BÀI");
        }

        public void UseTyping()
        {
            if (state != State.Editing) return;
            SaveTypedDraft();
            imageMode = false;
            ShowEditor();
        }

        public void UseImage()
        {
            if (state != State.Editing) return;
            SaveTypedDraft();
            imageMode = true;
            ShowEditor();
        }

        public void ChooseImage()
        {
            if (state != State.Editing || !imageMode) return;
            int version = ++requestVersion;
            state = State.PickingImage;
            btnChooseImage.interactable = false;
            btnSubmit.interactable = false;
            SetModeButtons(false);
            ImagePickerManager.EnsureInstance().PickImage(false, (success, data, sprite) =>
            {
                if (this == null || !isActiveAndEnabled || version != requestVersion || !IsOpen)
                {
                    ReleasePreview(sprite);
                    return;
                }
                if (success && sprite != null && !string.IsNullOrEmpty(data))
                {
                    Draft draft = CurrentDraft();
                    ReleasePreview(draft.preview);
                    draft.preview = sprite;
                    draft.imageBase64 = data;
                }
                else ReleasePreview(sprite);
                ShowEditor();
                if (!success && data == "TOO_LARGE")
                    questionText.text = "Ảnh vượt giới hạn dung lượng. Hãy chọn ảnh nhỏ hơn.";
            });
        }

        private void FitPreview()
        {
            if (imagePreview.texture == null) return;
            // Fit inside the 480 x 220 preview area without stretching the photo.
            float ratio = (float)imagePreview.texture.width / imagePreview.texture.height;
            Vector2 size = ratio >= 480f / 220f
                ? new Vector2(480f, 480f / ratio) : new Vector2(220f * ratio, 220f);
            imagePreview.rectTransform.sizeDelta = size;
        }

        public void Submit()
        {
            if (state == State.Passed) { Close(); return; }
            if (state == State.Failed || state == State.Error) { ShowEditor(); return; }
            if (state != State.Editing || !HasQuestion) return;
            SaveTypedDraft();
            Draft draft = CurrentDraft();
            if (imageMode ? string.IsNullOrEmpty(draft.imageBase64)
                          : string.IsNullOrWhiteSpace(draft.text))
            {
                questionText.text = imageMode ? "Hãy chọn ảnh bài làm trước khi nộp."
                                             : "Hãy nhập câu trả lời trước khi nộp.";
                return;
            }
            if (gradingClient == null) gradingClient = AIGradingClient.Instance;
            if (gradingClient == null)
            {
                ShowTechnicalError("Chưa gán AIGradingClient trên MazeManager.");
                return;
            }

            state = State.Grading;
            SetModeButtons(false);
            btnChooseImage.interactable = false;
            btnSubmit.interactable = false;
            answerInput.readOnly = true;
            bool mock = gradingClient.mode == AIGradingClient.Mode.Mock;
            questionText.text = mock
                ? "[GIẢ LẬP — CHƯA CHẤM AI THẬT] Đang thử luồng nộp bài…"
                : currentBoss.bossName + " đang xem bài của bạn…";
            int version = ++requestVersion;
            System.Action<AIGradingResponse> done = response =>
            {
                if (this == null || !isActiveAndEnabled || version != requestVersion || !IsOpen) return;
                ShowResponse(response, mock);
            };
            System.Action<string> error = message =>
            {
                if (this != null && isActiveAndEnabled && version == requestVersion && IsOpen)
                    ShowTechnicalError(message);
            };
            if (imageMode)
                gradingClient.SubmitEssay(currentBoss.questionID, draft.imageBase64, done, error);
            else
                gradingClient.SubmitTextEssay(currentBoss.questionID, draft.text, done, error);
        }

        private void ShowResponse(AIGradingResponse response, bool mock)
        {
            if (response == null || !response.readable)
            {
                ShowTechnicalError(response != null ? response.feedback : "Phản hồi rỗng.");
                return;
            }
            if (float.IsNaN(response.score) || float.IsInfinity(response.score)
                || float.IsNaN(response.maxScore) || float.IsInfinity(response.maxScore)
                || response.maxScore <= 0f || response.score < 0f || response.score > response.maxScore)
            {
                ShowTechnicalError("Kết quả chấm không hợp lệ; chưa mở cổng.");
                return;
            }
            float percentage = response.score / response.maxScore * 100f;
            bool passed = percentage >= currentBoss.passPercentage;
            state = passed ? State.Passed : State.Failed;
            if (passed && currentBoss.MarkPassed(this)) NextBossNumber++;

            StringBuilder result = new StringBuilder();
            if (mock) result.AppendLine("GIẢ LẬP: ĐIỂM THỬ, KHÔNG ĐÁNH GIÁ NỘI DUNG BÀI LÀM.");
            result.AppendLine($"Điểm: {response.score:0.#}/{response.maxScore:0.#} ({percentage:0}%)");
            result.AppendLine(passed ? "ĐẠT — Cổng đã mở." : "CHƯA ĐẠT — Bạn có thể sửa và nộp lại.");
            AppendPoints(result, "Làm tốt", response.correctPoints);
            AppendPoints(result, "Cần sửa/bổ sung", response.missingPoints);
            result.AppendLine("Nhận xét: " + response.feedback);
            if (!string.IsNullOrWhiteSpace(response.suggestedAnswer))
                result.AppendLine("Gợi ý giải thích: " + response.suggestedAnswer);
            DisplayResult(result.ToString());
            questionText.text = mock ? "KẾT QUẢ GIẢ LẬP — CHƯA CÓ AI CHẤM THẬT"
                : (passed ? "ĐÃ VƯỢT THỬ THÁCH" : "HÃY XEM NHẬN XÉT VÀ THỬ LẠI");
            SetButtonText(btnSubmit, passed ? "ĐI TIẾP" : "SỬA BÀI");
            UpdateHint();
        }

        private static void AppendPoints(StringBuilder output, string heading, List<string> points)
        {
            if (points == null || points.Count == 0) return;
            output.AppendLine(heading + ":");
            foreach (string point in points) output.AppendLine("• " + point);
        }

        private void DisplayResult(string message)
        {
            uploadArea.SetActive(false);
            answerInput.gameObject.SetActive(true);
            answerInput.readOnly = true;
            answerInput.characterLimit = 0;
            answerInput.text = message;
            answerInput.caretPosition = 0;
            answerInput.ForceLabelUpdate();
            SetModeButtons(false);
            btnSubmit.interactable = true;
        }

        private void ShowTechnicalError(string error)
        {
            state = State.Error;
            questionText.text = "CHƯA CHẤM ĐƯỢC — KHÔNG TÍNH LÀ TRẢ LỜI SAI";
            DisplayResult("Không thể chấm bài: " + error +
                "\nBài làm được giữ lại. Chọn THỬ LẠI để quay về sửa/gửi bài.");
            SetButtonText(btnSubmit, "THỬ LẠI");
        }

        private void SetModeButtons(bool enabled)
        {
            btnTyping.interactable = enabled;
            btnUpload.interactable = enabled;
        }

        private static void SetButtonText(Button button, string text)
        {
            TMP_Text caption = button.GetComponentInChildren<TMP_Text>(true);
            if (caption != null) caption.text = text;
        }

        public void Close()
        {
            SaveTypedDraft();
            requestVersion++; // Ignore responses from abandoned requests.
            essayPanel.SetActive(false);
            player.SetInputLocked(false);
            currentBoss = null;
            UpdateHint();
        }

        private static void ReleasePreview(Sprite sprite)
        {
            if (sprite == null) return;
            Texture2D texture = sprite.texture;
            Destroy(sprite);
            if (texture != null) Destroy(texture);
        }

        private void OnDestroy()
        {
            requestVersion++;
            if (player != null) player.SetInputLocked(false);
            foreach (Draft draft in drafts.Values) ReleasePreview(draft.preview);
        }
    }
}
