// ============================================================
// BiologyBootstrap.cs
// Gắn script này vào Main Camera của scene "SinhHoc".
// Khi scene chạy, nó tự động tạo đầy đủ:
//   BiologyQuestionBank (tải Google Sheet) → BiologySurvivalManager
//   → BiologyHUD → BiologyQuizUI → EventSystem
// Nhờ vậy KHÔNG cần setup tay gì trong Editor, không thể quên kéo tham chiếu.
// ============================================================
using UnityEngine;

namespace PRU.Biology
{
    public class BiologyBootstrap : MonoBehaviour
    {
        [Header("Google Sheet CSV URL (câu hỏi sinh học)")]
        [TextArea(2, 3)]
        public string googleSheetCsvUrl = "";

        [Header("Nâng cao (thường để mặc định)")]
        public bool createQuizUI = true;
        public bool createHud = true;
        public bool createEssayUI = true;

        [Header("AI Grading (tự luận - để Mock khi chưa có backend)")]
        public AIGradingClient.Mode aiGradingMode = AIGradingClient.Mode.Mock;
        [Tooltip("URL backend chấm bài (chỉ dùng khi mode = RealBackend)")]
        public string aiGradingBackendUrl = "https://your-backend.example.com/api/grade";

        private void Start()
        {
            BuildAll();
        }

        private void BuildAll()
        {
            // 1) Ngân hàng câu hỏi (tự DontDestroyOnLoad bên trong nó)
            BiologyQuestionBank bank = FindAnyObjectByType<BiologyQuestionBank>();
            if (bank == null)
            {
                GameObject bankGo = new GameObject("BiologyQuestionBank");
                bank = bankGo.AddComponent<BiologyQuestionBank>();
                bank.googleSheetCsvUrl = googleSheetCsvUrl;
            }

            // 2) Manager của game sinh tồn (Start() của nó tự dựng sân đấu + người chơi)
            GameObject lmGo = new GameObject(BiologyGameConfig.LEVEL_MANAGER_GO_NAME);
            lmGo.AddComponent<BiologySurvivalManager>();

            // 3) EventSystem cho UI (bắt buộc để bấm được nút)
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 4) UI (HUD + Quiz)
            if (createHud)
            {
                GameObject hudGo = new GameObject(BiologyGameConfig.HUD_GO_NAME);
                hudGo.AddComponent<BiologyHUD>();
            }
            if (createQuizUI)
            {
                GameObject quizGo = new GameObject(BiologyGameConfig.QUIZ_UI_GO_NAME);
                quizGo.AddComponent<BiologyQuizUI>();
            }

            // 5) UI TỰ LUẬN (chụp/tải ảnh → AI chấm) + ImagePicker + AIGradingClient
            if (createEssayUI)
            {
                GameObject essayGo = new GameObject("EssayQuestionUI");
                essayGo.AddComponent<EssayQuestionUI>();

                GameObject pickerGo = new GameObject("ImagePickerManager");
                pickerGo.AddComponent<ImagePickerManager>();

                GameObject aiGo = new GameObject("AIGradingClient");
                AIGradingClient ai = aiGo.AddComponent<AIGradingClient>();
                ai.mode = aiGradingMode;
                ai.backendUrl = aiGradingBackendUrl;
            }

            Debug.Log("[SinhHoc][Bootstrap] Đã dựng xong game SINH TỒN MIỄN DỊCH - Sinh Học. Chúc chơi game vui vẻ!");
        }
    }
}
