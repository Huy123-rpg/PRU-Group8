using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển toàn bộ Đấu trường Đối kháng 1vs1 (Scene 34.1 & 34.2):
    /// - Quản lý 10 trụ câu hỏi dùng chung
    /// - Quản lý đồng hồ 5:00 (300 giây) đếm lùi
    /// - Bảng điểm thời gian thực: Bạn vs Đối thủ
    /// - Tải bộ 10 câu hỏi chuẩn Vật lý 9 từ PhysicsQuestionRepository theo đúng bài/chương
    /// - Khóa/mở di chuyển khi bật popup câu hỏi
    /// - Xử lý chiếm trụ nguyên tử (atomic claim) và tính điểm
    /// - Kết thúc trận: hết giờ hoặc đủ 10 trụ -> hiển thị màn kết quả Thắng/Thua/Hòa
    /// </summary>
    public class VersusArenaController : MonoBehaviour
    {
        public static VersusArenaController Instance { get; private set; }

        [Header("=== TOP BAR UI ===")]
        [SerializeField] private TextMeshProUGUI txtTimer;
        [SerializeField] private TextMeshProUGUI txtPlayerScore;
        [SerializeField] private TextMeshProUGUI txtOpponentScore;
        [SerializeField] private Image imgPlayerAvatar;
        [SerializeField] private Image imgOpponentAvatar;
        [SerializeField] private TextMeshProUGUI txtPlayerName;
        [SerializeField] private TextMeshProUGUI txtOpponentName;

        [Header("=== ARENA CHARACTERS ===")]
        [SerializeField] private VersusPlayerController playerController;
        [SerializeField] private VersusBotController botController;

        [Header("=== 10 TOWERS ===")]
        [SerializeField] private VersusTowerController[] towers = new VersusTowerController[10];

        [Header("=== QUESTION MODAL ===")]
        [SerializeField] private VersusQuestionModalController questionModal;

        [Header("=== NAVIGATION ===")]
        [SerializeField] private Button btnBack;

        private float _matchTimeRemaining = 300f; // 5 phút
        private bool _isMatchEnded = false;
        private bool _isInputLocked = false;
        private int _playerScore = 0;
        private int _opponentScore = 0;
        private int _playerSolvedCount = 0;
        private int _opponentSolvedCount = 0;
        private float _lastPlayerClaimTime = 0f;
        private float _lastOpponentClaimTime = 0f;

        public VersusTowerController[] Towers { get => towers; set => towers = value; }
        public bool IsMatchEnded => _isMatchEnded;
        public bool IsInputLocked => _isInputLocked || _isMatchEnded;
        public int PlayerScore => _playerScore;
        public int OpponentScore => _opponentScore;
        public int PlayerSolvedCount => _playerSolvedCount;
        public int OpponentSolvedCount => _opponentSolvedCount;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            if (towers == null || towers.Length < 10)
            {
                towers = new VersusTowerController[10];
            }
            EnsureEventSystem();
            try
            {
                AutoBindArenaElements();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersusArenaController] Lỗi trong AutoBindArenaElements: {ex}");
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;
            try
            {
                NavigationService.Instance.EnsureDevContext();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersusArenaController] EnsureDevContext warning: {ex.Message}");
            }

            _playerScore = 0;
            _opponentScore = 0;
            _playerSolvedCount = 0;
            _opponentSolvedCount = 0;
            _lastPlayerClaimTime = 0f;
            _lastOpponentClaimTime = 0f;
            _matchTimeRemaining = 300f; // 5 phút = 05:00
            _isMatchEnded = false;
            _isInputLocked = false;

            if (txtTimer != null) txtTimer.text = "05:00";
            UpdateScoreboardUI();

            try
            {
                LoadMatchQuestions();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersusArenaController] Lỗi trong LoadMatchQuestions: {ex}");
            }

            try
            {
                InitCharacters();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersusArenaController] Lỗi trong InitCharacters: {ex}");
            }

            // Đảm bảo ẩn toàn bộ 15 GameObjects của Popup câu hỏi ngay đầu trận
            if (questionModal != null)
            {
                try
                {
                    questionModal.SetPopupVisible(false);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[VersusArenaController] SetPopupVisible warning: {ex.Message}");
                }
            }

            StartCoroutine(MatchTimerRoutine());
            Debug.Log("[VersusArenaController] Trận đấu đối kháng 1vs1 đã khởi động! MatchTimerRoutine đang chạy.");

            if (VersusSessionState.ActiveConfig.isOnline && VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.OnRemoteSolveTower += HandleRemoteSolveTower;
                VersusNetworkManager.Instance.OnRemoteMatchFinish += HandleRemoteMatchFinish;
                VersusNetworkManager.Instance.OnOpponentDisconnected += HandleOpponentDisconnected;
            }
        }

        private void OnDestroy()
        {
            if (VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.OnRemoteSolveTower -= HandleRemoteSolveTower;
                VersusNetworkManager.Instance.OnRemoteMatchFinish -= HandleRemoteMatchFinish;
                VersusNetworkManager.Instance.OnOpponentDisconnected -= HandleOpponentDisconnected;
            }
        }

        private void Update()
        {
            if (VersusSessionState.ActiveConfig.isOnline && VersusNetworkManager.Instance != null)
            {
                if (playerController != null && !_isMatchEnded)
                {
                    VersusNetworkManager.Instance.SendPlayerPosition(playerController.Position);
                }

                if (botController != null && VersusNetworkManager.Instance.HasRemotePosition)
                {
                    var bRt = botController.GetComponent<RectTransform>();
                    if (bRt != null)
                    {
                        bRt.anchoredPosition = Vector2.Lerp(bRt.anchoredPosition, VersusNetworkManager.Instance.RemotePlayerPosition, Time.unscaledDeltaTime * 15f);
                    }
                }
            }
        }

        private void EnsureEventSystem()
        {
            var es = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
            else
            {
                // Trong Unity 6 New Input System, nếu EventSystem chưa có InputSystemUIInputModule thì thêm vào
                if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }
            }
        }

        private void AutoBindArenaElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 0. Tìm nút Quay lại ở góc dưới bên trái (Image 26 tại pos (-773, -396.5))
            if (btnBack == null)
            {
                foreach (var b in canvas.GetComponentsInChildren<Button>(true))
                {
                    var pos = b.GetComponent<RectTransform>().anchoredPosition;
                    if (pos.x < -600f && pos.y < -300f)
                    {
                        btnBack = b;
                        break;
                    }
                }
            }
            if (btnBack == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    var pos = img.rectTransform.anchoredPosition;
                    if (pos.x < -600f && pos.y < -300f && img.rectTransform.sizeDelta.x < 350f)
                    {
                        btnBack = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                        btnBack.targetGraphic = img;
                        break;
                    }
                }
            }

            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackButtonClicked);
            }

            // Tắt raycastTarget cho text "Quay lại" ở góc dưới trái và thêm listener trực tiếp
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                var pos = tmp.rectTransform.anchoredPosition;
                if (pos.x < -500f && pos.y < -300f && (tmp.text.Contains("Quay") || tmp.text.Contains("Back")))
                {
                    tmp.raycastTarget = false;
                    var tBtn = tmp.gameObject.GetComponent<Button>() ?? tmp.gameObject.AddComponent<Button>();
                    tBtn.onClick.RemoveAllListeners();
                    tBtn.onClick.AddListener(OnBackButtonClicked);
                }
            }

            // 1. Tìm Text Timer (chứa "03:42" hoặc font to ở giữa trên)
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string txt = tmp.text.Trim();
                var pos = tmp.rectTransform.anchoredPosition;
                if (txt.Contains(":") && (txt.Length == 5 || txt.Length == 4) && Mathf.Abs(pos.x) < 200f && pos.y > 200f)
                {
                    txtTimer = tmp;
                    break;
                }
            }

            // 2. Tìm Text Điểm Bạn & Đối Thủ
            foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string txt = tmp.text;
                var pos = tmp.rectTransform.anchoredPosition;
                if (txt.Contains("điểm") || txt.Contains("diem") || txt.Contains("/10"))
                {
                    if (pos.x < 0f && txtPlayerScore == null)
                    {
                        txtPlayerScore = tmp;
                    }
                    else if (pos.x > 0f && txtOpponentScore == null)
                    {
                        txtOpponentScore = tmp;
                    }
                }
            }

            // 2.1. Tìm Avatar Bạn và Đối thủ ở 2 bảng điểm trên cùng
            if (imgPlayerAvatar == null || imgOpponentAvatar == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    if (img == null || img.sprite == null || !IsCharacterAvatarSprite(img.sprite)) continue;
                    var pos = img.rectTransform.anchoredPosition;
                    var sz = img.rectTransform.sizeDelta;
                    if (sz.x > 300f || sz.y > 300f) continue;

                    if (imgPlayerAvatar == null && pos.x < -600f && pos.y > 350f)
                    {
                        imgPlayerAvatar = img;
                    }
                    else if (imgOpponentAvatar == null && pos.x > 600f && pos.y > 350f)
                    {
                        imgOpponentAvatar = img;
                    }
                }
            }

            // 3. Nhận diện 2 nhân vật TRƯỚC TIÊN (để không bị gán nhầm thành Trụ)
            AutoBindCharacters(canvas);

            // 4. Tự động nhận diện 10 trụ (loại trừ tuyệt đối Player & Bot)
            AutoBindTowers(canvas);

            // 5. Nhận diện Question Modal do người dùng tự thiết kế
            AutoBindQuestionModal(canvas);
        }

        private void AutoBindCharacters(Canvas canvas)
        {
            // Trong thiết kế Scene 34.1 & 34.2:
            // Bạn ở pos (-430, -196), kích thước ~171.46x156.01, ID 861865521
            // Đối thủ ở pos (414, -188), kích thước ~171.46x156.01, ID 1048938327
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
            {
                var pos = img.rectTransform.anchoredPosition;
                var size = img.rectTransform.sizeDelta;
                string n = img.gameObject.name;

                // Player: vị trí bên trái (~ -430, -196)
                bool isPlayerMatch = (n.Contains("Player") || n.Contains("Ban"))
                    || (pos.x < -350f && pos.y < -150f && size.x < 180f);

                if (isPlayerMatch && playerController == null)
                {
                    // Xóa VersusTowerController nếu vô tình bị gán trước đó
                    var rogueTc = img.GetComponent<VersusTowerController>();
                    if (rogueTc != null) DestroyImmediate(rogueTc);

                    // Xóa Button cướp click trên Player
                    var rogueBtn = img.GetComponent<Button>();
                    if (rogueBtn != null && rogueBtn.onClick.GetPersistentEventCount() == 0) DestroyImmediate(rogueBtn);

                    playerController = img.gameObject.GetComponent<VersusPlayerController>() ?? img.gameObject.AddComponent<VersusPlayerController>();
                }

                // Bot: vị trí bên phải (~ 414, -188)
                bool isBotMatch = (n.Contains("Bot") || n.Contains("Opponent") || n.Contains("DoiThu"))
                    || (pos.x > 350f && pos.y < -150f && size.x < 180f);

                if (isBotMatch && botController == null)
                {
                    var rogueTc = img.GetComponent<VersusTowerController>();
                    if (rogueTc != null) DestroyImmediate(rogueTc);

                    var rogueBtn = img.GetComponent<Button>();
                    if (rogueBtn != null && rogueBtn.onClick.GetPersistentEventCount() == 0) DestroyImmediate(rogueBtn);

                    botController = img.gameObject.GetComponent<VersusBotController>() ?? img.gameObject.AddComponent<VersusBotController>();
                }
            }
        }

        [Header("=== TOWER SPRITES ===")]
        [SerializeField] private Sprite neutralTowerSprite;
        [SerializeField] private Sprite playerTowerSprite;
        [SerializeField] private Sprite opponentTowerSprite;

        private void LoadTowerSprites()
        {
            if (neutralTowerSprite == null || playerTowerSprite == null || opponentTowerSprite == null)
            {
                var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var sp in allSprites)
                {
                    if (sp == null) continue;
                    if (neutralTowerSprite == null && sp.name.EndsWith("_24")) neutralTowerSprite = sp;
                    else if (playerTowerSprite == null && sp.name.EndsWith("_26")) playerTowerSprite = sp;
                    else if (opponentTowerSprite == null && sp.name.EndsWith("_46")) opponentTowerSprite = sp;
                }
            }
        }

        private void AutoBindTowers(Canvas canvas)
        {
            // Tìm chính xác 10 trụ:
            // Kích thước chuẩn thiết kế của 10 trụ là 181.12 x 167.60
            // Hàng trên Y ~ 235-245 (Trụ 1-5), Hàng dưới Y ~ -157 đến -165 (Trụ 6-10)
            // LOẠI TRỪ TUYỆT ĐỐI Player (-430, -196) và Bot (414, -188)
            var allImages = canvas.GetComponentsInChildren<Image>(true);
            var towerCandidates = new List<Image>();

            foreach (var img in allImages)
            {
                // Bỏ qua Player và Bot
                if (playerController != null && img.gameObject == playerController.gameObject) continue;
                if (botController != null && img.gameObject == botController.gameObject) continue;

                var pos = img.rectTransform.anchoredPosition;
                var size = img.rectTransform.sizeDelta;
                string n = img.gameObject.name;

                // Bỏ qua Player/Bot theo tọa độ
                if (pos.x < -380f && pos.y < -170f) continue;
                if (pos.x > 380f && pos.y < -170f) continue;

                // Bỏ qua popup, banner, avatar
                if (n.Contains("Popup") || n.Contains("Banner") || n.Contains("Btn_") || n.Contains("Txt_") || n.Contains("Avatar")) continue;

                // Kích thước chính xác của trụ
                if (size.x > 170f && size.x < 195f && size.y > 155f && size.y < 180f)
                {
                    if ((pos.y > 180f && pos.y < 300f) || (pos.y < -120f && pos.y > -200f))
                    {
                        towerCandidates.Add(img);
                    }
                }
            }

            // Sắp xếp: hàng trên từ trái sang phải (1->5), hàng dưới từ trái sang phải (6->10)
            var topRow = new List<Image>();
            var bottomRow = new List<Image>();
            foreach (var img in towerCandidates)
            {
                if (img.rectTransform.anchoredPosition.y > 0f) topRow.Add(img);
                else bottomRow.Add(img);
            }

            topRow.Sort((a, b) => a.rectTransform.anchoredPosition.x.CompareTo(b.rectTransform.anchoredPosition.x));
            bottomRow.Sort((a, b) => a.rectTransform.anchoredPosition.x.CompareTo(b.rectTransform.anchoredPosition.x));

            var orderedTowers = new List<Image>();
            orderedTowers.AddRange(topRow);
            orderedTowers.AddRange(bottomRow);

            if (towers == null || towers.Length < 10)
            {
                towers = new VersusTowerController[10];
            }

            LoadTowerSprites();

            for (int i = 0; i < 10; i++)
            {
                if (i < orderedTowers.Count)
                {
                    var img = orderedTowers[i];
                    towers[i] = img.gameObject.GetComponent<VersusTowerController>() ?? img.gameObject.AddComponent<VersusTowerController>();
                    towers[i].towerNumber = (i + 1);
                    towers[i].ConfigureSprites(neutralTowerSprite, playerTowerSprite, opponentTowerSprite);
                    towers[i].ResetTower();
                }
            }
        }

        private void AutoBindQuestionModal(Canvas canvas)
        {
            // Tìm trong scene xem đã có QuestionModal chưa (có sẵn trong Scene 34.1 & 34.2)
            questionModal = canvas.GetComponentInChildren<VersusQuestionModalController>(true);
            if (questionModal == null)
            {
                foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                {
                    if (img.rectTransform.sizeDelta.x > 1200f && img.rectTransform.sizeDelta.y > 700f)
                    {
                        questionModal = img.gameObject.GetComponent<VersusQuestionModalController>() ?? img.gameObject.AddComponent<VersusQuestionModalController>();
                        break;
                    }
                }
            }

            if (questionModal != null)
            {
                questionModal.AutoBindUI();
                questionModal.SetPopupVisible(false);
            }
        }

        private void LoadMatchQuestions()
        {
            List<PhysicsQuestionRaw> qList = null;

            // Ưu tiên cao nhất: Nạp đúng danh sách ID câu hỏi thực tế mà Host đã đồng bộ
            var cfg = VersusSessionState.ActiveConfig;
            if (cfg.isOnline && cfg.matchQuestionIDs != null && cfg.matchQuestionIDs.Count > 0)
            {
                qList = new List<PhysicsQuestionRaw>();
                foreach (var qId in cfg.matchQuestionIDs)
                {
                    var q = PhysicsQuestionRepository.GetQuestionById(qId);
                    if (q != null)
                    {
                        qList.Add(q);
                    }
                }
                Debug.Log($"[VersusArenaController] Đã nạp thành công {qList.Count}/10 câu hỏi theo danh sách ID Host chỉ định!");
            }

            // Nếu chưa đủ hoặc là chế độ Bot: Lấy theo seed
            if (qList == null || qList.Count < 10)
            {
                int? seed = cfg.isOnline ? cfg.questionSeed : (int?)null;
                qList = PhysicsQuestionRepository.GetRandomVersusQuestions(10, seed);
            }

            if (qList == null || qList.Count == 0)
            {
                qList = new List<PhysicsQuestionRaw>();
                for (int i = 0; i < 10; i++)
                {
                    var fallback = new PhysicsQuestionRaw();
                    fallback.questionID = $"VS_Q{i + 1}";
                    fallback.questionText = $"Câu hỏi đối kháng số {i + 1}: Vật lý 9 - Học viện Aurelia";
                    fallback.answers = new string[] { "Phương án A", "Phương án B", "Phương án C", "Phương án D" };
                    fallback.correctAnswerIndex = 0;
                    qList.Add(fallback);
                }
            }

            // Gán cố định 10 câu hỏi cho 10 trụ (Trụ 1 -> Câu 1, Trụ 2 -> Câu 2, ..., Trụ 10 -> Câu 10)
            for (int i = 0; i < 10; i++)
            {
                if (towers[i] != null)
                {
                    towers[i].questionData = i < qList.Count ? qList[i] : qList[0];
                }
            }
        }

        private void OnBackButtonClicked()
        {
            Debug.Log("[VersusArenaController] Người chơi bấm Quay lại -> Thoát trận đối kháng");
            _isMatchEnded = true;
            _isInputLocked = true;
            StopAllCoroutines();

            if (VersusSessionState.ActiveConfig.isOnline && VersusNetworkManager.Instance != null)
            {
                VersusNetworkManager.Instance.LeaveRoom();
            }

            if (questionModal != null && questionModal.IsOpen)
            {
                questionModal.Close();
            }

            Time.timeScale = 1f;
            NavigationService.Instance.ResetTransitionLock();
            NavigationService.Instance.NavigateTo(RouteId.VersusLobby);
        }

        private void InitCharacters()
        {
            var cfg = VersusSessionState.ActiveConfig;
            if (txtPlayerName != null) txtPlayerName.text = cfg.playerName;
            if (txtOpponentName != null) txtOpponentName.text = cfg.opponentName;

            // Nếu là trận Online: TẮT BOT CONTROLLER để đối thủ người thật điều khiển
            if (cfg.isOnline && botController != null)
            {
                botController.enabled = false;
                Debug.Log("[VersusArenaController] Chế độ ĐỐI KHÁNG ONLINE: Đã tắt BotController. Đối thủ người thật điều khiển.");
            }
        }

        private IEnumerator MatchTimerRoutine()
        {
            while (_matchTimeRemaining > 0f && !_isMatchEnded)
            {
                // Dùng realtime để đồng hồ không bao giờ bị đứng im
                yield return new WaitForSecondsRealtime(1f);
                _matchTimeRemaining -= 1f;

                int minutes = Mathf.FloorToInt(_matchTimeRemaining / 60f);
                int seconds = Mathf.FloorToInt(_matchTimeRemaining % 60f);

                if (txtTimer != null)
                {
                    txtTimer.text = $"{minutes:00}:{seconds:00}";
                }
            }

            if (!_isMatchEnded)
            {
                HandleTimeOut();
            }
        }

        private void HandleTimeOut()
        {
            bool isWin = false;
            bool isDraw = false;
            string reason;

            if (_playerSolvedCount > _opponentSolvedCount)
            {
                isWin = true;
                reason = "HẾT GIỜ - BẠN ĐÚNG NHIỀU CÂU HƠN!";
            }
            else if (_opponentSolvedCount > _playerSolvedCount)
            {
                isWin = false;
                reason = "HẾT GIỜ - ĐỐI THỦ ĐÚNG NHIỀU CÂU HƠN!";
            }
            else
            {
                isDraw = true;
                reason = "HẾT GIỜ - HAI BÊN HÒA NHAU!";
            }
            EndMatch(isWin, isDraw, reason);
        }

        public bool IsTowerClaimed(int towerNumber)
        {
            int idx = towerNumber - 1;
            if (idx >= 0 && idx < towers.Length && towers[idx] != null)
            {
                return towers[idx].IsClaimed;
            }
            return false;
        }

        public bool CanPlayerInteractWithTower(int towerNumber)
        {
            int idx = towerNumber - 1;
            if (idx >= 0 && idx < towers.Length && towers[idx] != null)
            {
                return !towers[idx].playerSolved;
            }
            return false;
        }

        public void OpenQuestionModal(int towerNumber)
        {
            if (_isMatchEnded) return;

            int idx = towerNumber - 1;
            if (idx < 0 || idx >= towers.Length || towers[idx] == null) return;

            var tower = towers[idx];
            // Chỉ chặn nếu chính Player đã giải đúng trụ này
            if (tower.playerSolved) return;

            _isInputLocked = true;
            if (questionModal != null)
            {
                questionModal.Open(towerNumber, tower.questionData);
            }
        }

        public void OnModalClosed()
        {
            _isInputLocked = false;
        }

        /// <summary>
        /// Xử lý giải đúng trụ theo luật đối kháng mới:
        /// - Tiến độ độc lập 100%: Cả Player và Bot đều có thể giải đúng cùng một trụ.
        /// - Điểm: +100 cho mỗi trụ giải đúng lần đầu, tối đa 1000 điểm (10/10).
        /// - Trụ hiển thị cả 2 vòng sáng Xanh và Vàng đồng tâm khi cả hai cùng giải đúng.
        /// - Điều kiện thắng mới: Ai giải đủ 10/10 trụ trước -> Thắng ngay lập tức!
        /// </summary>
        public void SolveTower(int towerNumber, bool isPlayer)
        {
            if (_isMatchEnded) return;

            int idx = towerNumber - 1;
            if (idx < 0 || idx >= towers.Length || towers[idx] == null) return;

            var tower = towers[idx];

            if (isPlayer)
            {
                if (tower.playerSolved) return; // Không cộng điểm lặp lại nếu chính mình đã giải đúng
                tower.SetPlayerSolved(true);
                _playerScore += 100;
                _playerSolvedCount++;
                _lastPlayerClaimTime = Time.realtimeSinceStartup;
                Debug.Log($"[VersusArena] BẠN ĐÃ GIẢI ĐÚNG TRỤ {towerNumber}! Điểm={_playerScore}, Đúng={_playerSolvedCount}/10");

                // Đồng bộ qua mạng cho đối thủ
                if (VersusSessionState.ActiveConfig.isOnline && VersusNetworkManager.Instance != null)
                {
                    VersusNetworkManager.Instance.SendSolveTower(towerNumber);
                }
            }
            else
            {
                if (tower.opponentSolved) return; // Không cộng điểm lặp lại nếu bot đã giải đúng
                tower.SetOpponentSolved(true);
                _opponentScore += 100;
                _opponentSolvedCount++;
                _lastOpponentClaimTime = Time.realtimeSinceStartup;
                Debug.Log($"[VersusArena] ĐỐI THỦ ĐÃ GIẢI ĐÚNG TRỤ {towerNumber}! Điểm={_opponentScore}, Đúng={_opponentSolvedCount}/10");
            }

            UpdateScoreboardUI();

            // LUẬT MỚI: AI HOÀN THÀNH 10/10 TRƯỚC SẼ THẮNG NGAY LẬP TỨC!
            if (_playerSolvedCount >= 10)
            {
                EndMatch(isPlayerWin: true, isDraw: false, reason: "BẠN ĐÃ HOÀN THÀNH 10/10 TRỤ TRƯỚC - CHIẾN THẮNG!");
                return;
            }

            if (_opponentSolvedCount >= 10)
            {
                EndMatch(isPlayerWin: false, isDraw: false, reason: "ĐỐI THỦ ĐÃ HOÀN THÀNH 10/10 TRỤ TRƯỚC!");
                return;
            }
        }

        // Tương thích ngược với các hàm gọi cũ
        public void ClaimTower(int towerNumber, VersusTowerClaim claimer)
        {
            SolveTower(towerNumber, claimer == VersusTowerClaim.Player);
        }

        private void UpdateScoreboardUI()
        {
            if (txtPlayerScore != null)
            {
                txtPlayerScore.text = $"{_playerScore} điểm    Đúng: {_playerSolvedCount}/10";
            }
            if (txtOpponentScore != null)
            {
                txtOpponentScore.text = $"{_opponentScore} điểm    Đúng: {_opponentSolvedCount}/10";
            }
        }

        private void EndMatch(bool isPlayerWin, bool isDraw, string reason)
        {
            if (_isMatchEnded) return;
            _isMatchEnded = true;
            _isInputLocked = true;

            Debug.Log($"[VersusArena] KẾT THÚC TRẬN ĐẤU: {reason} | Bạn={_playerSolvedCount}/10 ({_playerScore}đ) vs Bot={_opponentSolvedCount}/10 ({_opponentScore}đ)");

            if (questionModal != null && questionModal.IsOpen)
            {
                questionModal.Close();
            }

            var res = new VersusMatchResult();
            res.matchId = VersusSessionState.ActiveConfig.matchId;
            res.mode = VersusSessionState.ActiveConfig.mode;
            res.playerScore = _playerScore;
            res.opponentScore = _opponentScore;
            res.playerTowers = _playerSolvedCount;
            res.opponentTowers = _opponentSolvedCount;
            res.playerCorrectCount = _playerSolvedCount;
            res.opponentCorrectCount = _opponentSolvedCount;
            res.playerCharacter = VersusSessionState.ActiveConfig.playerCharacter;
            res.opponentCharacter = VersusSessionState.ActiveConfig.opponentCharacter;
            res.opponentName = VersusSessionState.ActiveConfig.opponentName;

            // Chụp snapshot Avatar thực tế đang hiển thị trong Đấu trường (loại trừ tuyệt đối các banner, frame, atlas UI)
            Sprite pSprite = (imgPlayerAvatar != null && imgPlayerAvatar.sprite != null && IsCharacterAvatarSprite(imgPlayerAvatar.sprite)) ? imgPlayerAvatar.sprite : null;
            Sprite oSprite = (imgOpponentAvatar != null && imgOpponentAvatar.sprite != null && IsCharacterAvatarSprite(imgOpponentAvatar.sprite)) ? imgOpponentAvatar.sprite : null;

            if (pSprite == null || oSprite == null)
            {
                var canvasImages = GetComponentsInChildren<Image>(true);
                foreach (var img in canvasImages)
                {
                    if (img == null || img.sprite == null || !IsCharacterAvatarSprite(img.sprite)) continue;
                    var pos = img.rectTransform.anchoredPosition;
                    var sz = img.rectTransform.sizeDelta;
                    if (sz.x > 300f || sz.y > 300f) continue;

                    // Bảng điểm Bạn góc trên bên trái
                    if (pSprite == null && pos.x < -600f && pos.y > 350f)
                    {
                        pSprite = img.sprite;
                    }
                    // Bảng điểm Đối thủ góc trên bên phải
                    else if (oSprite == null && pos.x > 600f && pos.y > 350f)
                    {
                        oSprite = img.sprite;
                    }
                }
            }

            if (pSprite == null) pSprite = Resources.Load<Sprite>("Characters/Minh");
            if (oSprite == null) oSprite = Resources.Load<Sprite>("Characters/Duy");

            res.playerAvatarSprite = pSprite;
            res.opponentAvatarSprite = oSprite;

            res.isWinner = isPlayerWin && !isDraw;
            res.isOpponentWinner = !isPlayerWin && !isDraw;
            res.isDraw = isDraw;
            res.durationSeconds = 300f - _matchTimeRemaining;
            res.finishedTimestamp = DateTime.UtcNow.ToString("o");

            VersusSessionState.LastResult = res;

            if (VersusSessionState.ActiveConfig.isOnline && VersusNetworkManager.Instance != null && isPlayerWin && !isDraw)
            {
                VersusNetworkManager.Instance.BroadcastMatchFinish(VersusNetworkManager.Instance.IsHost, reason);
            }

            // XÓA BỎ POPUP XÁM CŨ: Chuyển thẳng sang Scene 35 do người dùng tự thiết kế
            StartCoroutine(TransitionToResultSceneRoutine());
        }

        private void HandleRemoteSolveTower(int towerNum, bool isHostClaimer)
        {
            Debug.Log($"[VersusArenaController] Nhận thông báo đối thủ giải đúng trụ {towerNum}");
            SolveTower(towerNum, isPlayer: false);
        }

        private void HandleRemoteMatchFinish(bool isPlayerWin, string reason)
        {
            Debug.Log($"[VersusArenaController] Nhận thông báo kết thúc trận từ đối thủ: {reason}");
            EndMatch(isPlayerWin, isDraw: false, reason);
        }

        private void HandleOpponentDisconnected()
        {
            Debug.LogWarning("[VersusArenaController] Đối thủ đã mất kết nối!");
            if (!_isMatchEnded)
            {
                EndMatch(isPlayerWin: true, isDraw: false, "ĐỐI THỦ ĐÃ MẤT KẾT NỐI - BẠN CHIẾN THẮNG!");
            }
        }

        private bool IsCharacterAvatarSprite(Sprite sp)
        {
            if (sp == null) return false;
            if (sp.texture != null)
            {
                string t = sp.texture.name.ToLower();
                if (t.Contains("atlas") || t.Contains("khung") || t.Contains("giao diện") || t.Contains("bộ giao diện") || t.Contains("button"))
                    return false;
                if (t.Contains("nhân vật") || t.Contains("chân dung") || t.Contains("characters"))
                    return true;
            }
            string s = sp.name.ToLower();
            if (s.Contains("khung") || s.Contains("frame") || s.Contains("btn") || s.Contains("bg") || s.Contains("banner"))
                return false;
            if (sp.rect.width > 900f || sp.rect.height > 900f || sp.rect.width < 10f || sp.rect.height < 10f)
                return false;
            float aspect = sp.rect.width / Mathf.Max(1f, sp.rect.height);
            if (aspect < 0.5f || aspect > 2.0f)
                return false;
            return true;
        }

        private IEnumerator TransitionToResultSceneRoutine()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            Time.timeScale = 1f;
            NavigationService.Instance.ResetTransitionLock();
            NavigationService.Instance.NavigateTo(RouteId.VersusResult);
        }
    }
}
