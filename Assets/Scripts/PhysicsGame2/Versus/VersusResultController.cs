using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển Màn hình Kết quả Đối kháng chính thức (Scene 35: "35. Kết quả đối kháng").
    /// Tuân thủ nghiêm ngặt các nguyên tắc:
    /// 1. SỬ DỤNG TRỰC TIẾP 2 Image avatar do người dùng đã đặt trong Scene (PlayerAvatar và OpponentAvatar).
    ///    CHỈ CẬP NHẬT .sprite theo nhân vật thực tế của trận đấu.
    ///    TUYỆT ĐỐI KHÔNG thay đổi RectTransform, Position, Anchor, Pivot, SizeDelta, Scale, hay Khung vàng.
    ///    KHÔNG tạo thêm bất kỳ UI hay Image hay Mask nào đè lên.
    /// 2. Nếu mở trực tiếp Scene kết quả mà chưa có dữ liệu trận (preview), giữ nguyên 100% hai chân dung mẫu hiện tại.
    /// 3. Bốn TMP (PlayerScoreText, PlayerCorrectText, OpponentScoreText, OpponentCorrectText):
    ///    CHỈ gán dữ liệu vào .text, giữ nguyên 100% font, font size, màu nâu #3E2305, anchors, pivots.
    /// 4. WIN / LOSE / HÒA cập nhật động chính xác theo MatchResult.
    /// 5. Nút Tái đấu và Quay về menu hoạt động 1 click mượt mà.
    /// </summary>
    public class VersusResultController : MonoBehaviour
    {
        [Header("=== OUTCOME BADGES ===")]
        [SerializeField] private Image imgPlayerBadge;     // Image (3) - Bạn (Bên trái)
        [SerializeField] private Image imgOpponentBadge;   // Image (4) - Đối thủ (Bên phải)

        [Header("=== AVATAR IMAGES (USER DESIGNED) ===")]
        [SerializeField] private Image imgPlayerAvatar;     // PlayerAvatar (Image 7) - Bên trái
        [SerializeField] private Image imgOpponentAvatar;   // OpponentAvatar (Image 8) - Bên phải

        [Header("=== SCORE & TOWERS TEXT (USER DESIGNED) ===")]
        [SerializeField] private TextMeshProUGUI txtPlayerScore;    // PlayerScoreText (cạnh Cúp vàng)
        [SerializeField] private TextMeshProUGUI txtPlayerTowers;   // PlayerCorrectText (cạnh Bia mục tiêu)
        [SerializeField] private TextMeshProUGUI txtOpponentScore;  // OpponentScoreText (cạnh Cúp vàng)
        [SerializeField] private TextMeshProUGUI txtOpponentTowers; // OpponentCorrectText (cạnh Bia mục tiêu)

        [Header("=== ACTION BUTTONS ===")]
        [SerializeField] private Button btnReplay;  // Image (5) - Nút Tái đấu
        [SerializeField] private Button btnBack;    // Image (6) - Nút Quay về menu

        [Header("=== BADGE SPRITES ===")]
        [SerializeField] private Sprite winSprite;
        [SerializeField] private Sprite loseSprite;

        [Header("=== DEFAULT AVATAR SPRITES ===")]
        [SerializeField] private Sprite defaultPlayerAvatarSprite;
        [SerializeField] private Sprite defaultOpponentAvatarSprite;

        public TextMeshProUGUI TxtPlayerScore => txtPlayerScore;
        public TextMeshProUGUI TxtPlayerTowers => txtPlayerTowers;
        public TextMeshProUGUI TxtOpponentScore => txtOpponentScore;
        public TextMeshProUGUI TxtOpponentTowers => txtOpponentTowers;
        public Image ImgPlayerBadge => imgPlayerBadge;
        public Image ImgOpponentBadge => imgOpponentBadge;
        public Image ImgPlayerAvatar => imgPlayerAvatar;
        public Image ImgOpponentAvatar => imgOpponentAvatar;
        public Button BtnReplay => btnReplay;
        public Button BtnBack => btnBack;

        private static bool s_isHookRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterSceneLoadHook()
        {
            if (s_isHookRegistered) return;
            s_isHookRegistered = true;
            SceneManager.sceneLoaded += OnSceneLoadedGlobal;
        }

        private static void OnSceneLoadedGlobal(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "35. Kết quả đối kháng" || scene.name == PhysicsSceneNames.VersusResult)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null && canvas.GetComponent<VersusResultController>() == null)
                {
                    Debug.Log("[VersusResult] Kích hoạt VersusResultController trên Canvas Scene 35.");
                    canvas.gameObject.AddComponent<VersusResultController>();
                }
            }
        }

        private void Awake()
        {
            AutoBindUI();
        }

        private void Start()
        {
            if (NavigationService.Instance != null)
            {
                NavigationService.Instance.EnsureDevContext();
            }
            RenderResultData();
            SetupButtonEvents();
        }

        /// <summary>
        /// Tự động tìm và liên kết các UI elements có sẵn do người dùng thiết kế trong Scene 35.
        /// </summary>
        public void AutoBindUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Tự động nhận diện Badge WIN / LOSE và Buttons có sẵn
            var allImages = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img == null) continue;
                var pos = GetCanvasPosition(canvas, img.transform);
                var size = img.rectTransform.sizeDelta;
                string n = img.gameObject.name;

                // Player Badge: WIN/LOSE bên trái (Pos x ~ -389, y ~ -53)
                if (imgPlayerBadge == null && (n == "Image (3)" || (pos.x < -300f && pos.x > -480f && pos.y > -100f && pos.y < 0f && size.x > 180f)))
                {
                    imgPlayerBadge = img;
                    if (winSprite == null && img.sprite != null && !img.sprite.name.Contains("_0"))
                    {
                        winSprite = img.sprite;
                    }
                }
                // Opponent Badge: WIN/LOSE bên phải (Pos x ~ 582, y ~ -62)
                else if (imgOpponentBadge == null && (n == "Image (4)" || (pos.x > 500f && pos.x < 680f && pos.y > -110f && pos.y < 0f && size.x > 180f)))
                {
                    imgOpponentBadge = img;
                    if (loseSprite == null && img.sprite != null && !img.sprite.name.Contains("_0"))
                    {
                        loseSprite = img.sprite;
                    }
                }
                // Nút Tái đấu (Pos x ~ -296, y ~ -432.9)
                else if (btnReplay == null && (n == "Image (5)" || (pos.x < -150f && pos.y < -350f && size.x > 250f)))
                {
                    btnReplay = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                    btnReplay.targetGraphic = img;
                    img.raycastTarget = true;
                }
                // Nút Quay về menu (Pos x ~ 95, y ~ -432.9)
                else if (btnBack == null && (n == "Image (6)" || (pos.x > 0f && pos.y < -350f && size.x > 250f)))
                {
                    btnBack = img.gameObject.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
                    btnBack.targetGraphic = img;
                    img.raycastTarget = true;
                }
                // PlayerAvatar: bên trái (Pos x ~ -650)
                else if (imgPlayerAvatar == null && (n == "PlayerAvatar" || n == "Image (7)" || (pos.x < -500f && pos.y > -100f && pos.y < 50f && size.x > 150f)))
                {
                    imgPlayerAvatar = img;
                    if (defaultPlayerAvatarSprite == null && img.sprite != null && IsValidAvatarSprite(img.sprite))
                    {
                        defaultPlayerAvatarSprite = img.sprite;
                    }
                }
                // OpponentAvatar: bên phải (Pos x ~ 298)
                else if (imgOpponentAvatar == null && (n == "OpponentAvatar" || n == "Image (8)" || (pos.x > 150f && pos.x < 450f && pos.y > -100f && pos.y < 50f && size.x > 150f)))
                {
                    imgOpponentAvatar = img;
                    if (defaultOpponentAvatarSprite == null && img.sprite != null && IsValidAvatarSprite(img.sprite))
                    {
                        defaultOpponentAvatarSprite = img.sprite;
                    }
                }
            }

            EnsureOutcomeSprites();

            // 2. Tìm đúng 4 TextMeshProUGUI người dùng đã thiết kế trong Scene
            BindUserDesignedTMPs(canvas);

            // 3. Chuẩn hóa vị trí và căn lề 4 Text theo đúng chuẩn bố cục bảng kết quả
            EnforceScoreTMPTransforms();
        }

        private Vector2 GetCanvasPosition(Canvas canvas, Transform target)
        {
            if (canvas == null || target == null) return Vector2.zero;
            Vector3 localPos = canvas.transform.InverseTransformPoint(target.position);
            return new Vector2(localPos.x, localPos.y);
        }

        private void BindUserDesignedTMPs(Canvas canvas)
        {
            var allTMPs = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);

            // 1. Tìm theo tên trước (PlayerScoreText, PlayerCorrectText, OpponentScoreText, OpponentCorrectText)
            foreach (var tmp in allTMPs)
            {
                if (tmp == null) continue;
                string n = tmp.gameObject.name.ToLower();
                if (txtPlayerScore == null && (n == "playerscoretext" || n.Contains("playerscore") || n.Contains("txt_playerscore") || n.Contains("diemban")))
                {
                    txtPlayerScore = tmp;
                }
                else if (txtPlayerTowers == null && (n == "playercorrecttext" || n.Contains("playercorrect") || n.Contains("playertowers") || n.Contains("txt_playercorrect") || n.Contains("socauban")))
                {
                    txtPlayerTowers = tmp;
                }
                else if (txtOpponentScore == null && (n == "opponentscoretext" || n.Contains("opponentscore") || n.Contains("txt_opponentscore") || n.Contains("diemdoithu")))
                {
                    txtOpponentScore = tmp;
                }
                else if (txtOpponentTowers == null && (n == "opponentcorrecttext" || n.Contains("opponentcorrect") || n.Contains("opponenttowers") || n.Contains("txt_opponentcorrect") || n.Contains("socaudoithu")))
                {
                    txtOpponentTowers = tmp;
                }
            }

            // 2. Nếu chưa đủ, tìm theo tọa độ dải trắng ngà (y khoảng -280 đến -130)
            var leftTMPs = new List<TextMeshProUGUI>();
            var rightTMPs = new List<TextMeshProUGUI>();

            foreach (var tmp in allTMPs)
            {
                if (tmp == null) continue;
                if (tmp == txtPlayerScore || tmp == txtPlayerTowers || tmp == txtOpponentScore || tmp == txtOpponentTowers) continue;

                Vector2 cPos = GetCanvasPosition(canvas, tmp.transform);
                if (cPos.y >= -280f && cPos.y <= -130f)
                {
                    if (cPos.x < 0f) leftTMPs.Add(tmp);
                    else rightTMPs.Add(tmp);
                }
            }

            if (txtPlayerScore == null && leftTMPs.Count > 0)
            {
                leftTMPs.Sort((a, b) => GetCanvasPosition(canvas, a.transform).x.CompareTo(GetCanvasPosition(canvas, b.transform).x));
                txtPlayerScore = leftTMPs[0];
                if (leftTMPs.Count > 1 && txtPlayerTowers == null) txtPlayerTowers = leftTMPs[1];
            }
            else if (txtPlayerTowers == null && leftTMPs.Count > 0)
            {
                txtPlayerTowers = leftTMPs[0];
            }

            if (txtOpponentScore == null && rightTMPs.Count > 0)
            {
                rightTMPs.Sort((a, b) => GetCanvasPosition(canvas, a.transform).x.CompareTo(GetCanvasPosition(canvas, b.transform).x));
                txtOpponentScore = rightTMPs[0];
                if (rightTMPs.Count > 1 && txtOpponentTowers == null) txtOpponentTowers = rightTMPs[1];
            }
            else if (txtOpponentTowers == null && rightTMPs.Count > 0)
            {
                txtOpponentTowers = rightTMPs[0];
            }

            // Kích hoạt và bật alpha 1 cho toàn bộ 4 TMP
            ActivateTMP(txtPlayerScore);
            ActivateTMP(txtPlayerTowers);
            ActivateTMP(txtOpponentScore);
            ActivateTMP(txtOpponentTowers);
        }

        private void ActivateTMP(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            tmp.gameObject.SetActive(true);
            tmp.enabled = true;
            Color c = tmp.color;
            if (c.a < 0.1f) c.a = 1f;
            tmp.color = c;
        }

        private void EnsureOutcomeSprites()
        {
            if (winSprite != null && loseSprite != null) return;

            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var sp in allSprites)
            {
                if (sp == null) continue;
                if (sp.texture != null && sp.texture.name.Contains("Bộ giao diện kết quả"))
                {
                    if (winSprite == null && sp.name.EndsWith("_8")) winSprite = sp;
                    else if (loseSprite == null && sp.name.EndsWith("_5")) loseSprite = sp;
                }
            }
        }

        /// <summary>
        /// Hiển thị dữ liệu trận đấu thực tế vào các thành phần có sẵn của người dùng.
        /// </summary>
        public void RenderResultData()
        {
            var res = VersusSessionState.LastResult;

            if (res != null)
            {
                int pCorrect = res.playerCorrectCount > 0 ? res.playerCorrectCount : res.playerTowers;
                int oCorrect = res.opponentCorrectCount > 0 ? res.opponentCorrectCount : res.opponentTowers;
                int pScore = pCorrect * 100;
                int oScore = oCorrect * 100;

                Debug.Log($"[VersusResult] Render Dữ Liệu Thật: Bạn={pScore}đ ({pCorrect}/10) | Đối thủ={oScore}đ ({oCorrect}/10) | Thắng={res.isWinner}, Hòa={res.isDraw}");

                // 1. Cập nhật Điểm và Số câu đúng vào đúng các TMP có sẵn
                // CHỈ thay đổi thuộc tính .text, tuyệt đối không chỉnh font hay rect
                if (txtPlayerScore != null)
                {
                    txtPlayerScore.text = $"{pScore}";
                    ActivateTMP(txtPlayerScore);
                }
                if (txtPlayerTowers != null)
                {
                    txtPlayerTowers.text = $"{pCorrect}/10";
                    ActivateTMP(txtPlayerTowers);
                }

                if (txtOpponentScore != null)
                {
                    txtOpponentScore.text = $"{oScore}";
                    ActivateTMP(txtOpponentScore);
                }
                if (txtOpponentTowers != null)
                {
                    txtOpponentTowers.text = $"{oCorrect}/10";
                    ActivateTMP(txtOpponentTowers);
                }

                // 2. Cập nhật WIN / LOSE / HÒA động
                EnsureOutcomeSprites();

                if (res.isDraw)
                {
                    if (imgPlayerBadge != null)
                    {
                        imgPlayerBadge.gameObject.SetActive(true);
                        imgPlayerBadge.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
                    }
                    if (imgOpponentBadge != null)
                    {
                        imgOpponentBadge.gameObject.SetActive(true);
                        imgOpponentBadge.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
                    }
                }
                else if (res.isWinner)
                {
                    if (imgPlayerBadge != null)
                    {
                        imgPlayerBadge.gameObject.SetActive(true);
                        if (winSprite != null) imgPlayerBadge.sprite = winSprite;
                        imgPlayerBadge.color = Color.white;
                    }
                    if (imgOpponentBadge != null)
                    {
                        imgOpponentBadge.gameObject.SetActive(true);
                        if (loseSprite != null) imgOpponentBadge.sprite = loseSprite;
                        imgOpponentBadge.color = Color.white;
                    }
                }
                else
                {
                    if (imgPlayerBadge != null)
                    {
                        imgPlayerBadge.gameObject.SetActive(true);
                        if (loseSprite != null) imgPlayerBadge.sprite = loseSprite;
                        imgPlayerBadge.color = Color.white;
                    }
                    if (imgOpponentBadge != null)
                    {
                        imgOpponentBadge.gameObject.SetActive(true);
                        if (winSprite != null) imgOpponentBadge.sprite = winSprite;
                        imgOpponentBadge.color = Color.white;
                    }
                }

                // 3. Cập nhật Sprite cho 2 Avatar thật theo nhân vật thực tế
                UpdateAvatarSprites(res);
            }
            else
            {
                // Mở trực tiếp Scene kết quả chưa có dữ liệu trận:
                // "Nếu mở trực tiếp Scene kết quả mà chưa có dữ liệu trận, giữ nguyên hai chân dung mẫu hiện tại, không thay bằng hình trắng hoặc null."
                Debug.Log("[VersusResult] Mở trực tiếp Scene 35 không có MatchResult -> Giữ nguyên 100% hai chân dung mẫu và thiết kế hiện tại.");
                
                // Đảm bảo các TMP có sẵn vẫn active và hiển thị rõ ràng
                ActivateTMP(txtPlayerScore);
                ActivateTMP(txtPlayerTowers);
                ActivateTMP(txtOpponentScore);
                ActivateTMP(txtOpponentTowers);
            }

            EnforceScoreTMPTransforms();
        }

        /// <summary>
        /// Chuẩn hóa vị trí 4 Text kết quả đúng chuẩn tỷ lệ thiết kế:
        /// [ CÚP ]  [ ĐIỂM SỐ ]     |     [ BIA MỤC TIÊU ]  [ SỐ CÂU ĐÚNG /10 ]
        /// Điểm số nằm giữa Cúp và Vạch chia. Số câu đúng nằm giữa Bia mục tiêu và mép phải bảng.
        /// </summary>
        public void EnforceScoreTMPTransforms()
        {
            if (txtPlayerScore != null)
            {
                txtPlayerScore.rectTransform.anchoredPosition = new Vector2(-550f, -283f);
                txtPlayerScore.rectTransform.sizeDelta = new Vector2(100f, 45f);
                txtPlayerScore.alignment = TextAlignmentOptions.Center;
            }
            if (txtPlayerTowers != null)
            {
                txtPlayerTowers.rectTransform.anchoredPosition = new Vector2(-330f, -283f);
                txtPlayerTowers.rectTransform.sizeDelta = new Vector2(100f, 45f);
                txtPlayerTowers.alignment = TextAlignmentOptions.Center;
            }
            if (txtOpponentScore != null)
            {
                txtOpponentScore.rectTransform.anchoredPosition = new Vector2(430f, -305f);
                txtOpponentScore.rectTransform.sizeDelta = new Vector2(100f, 45f);
                txtOpponentScore.alignment = TextAlignmentOptions.Center;
            }
            if (txtOpponentTowers != null)
            {
                txtOpponentTowers.rectTransform.anchoredPosition = new Vector2(665f, -305f);
                txtOpponentTowers.rectTransform.sizeDelta = new Vector2(100f, 45f);
                txtOpponentTowers.alignment = TextAlignmentOptions.Center;
            }
        }

        /// <summary>
        /// CẬP NHẬT SPRITE CHO HAI AVATAR:
        /// CHỈ cập nhật playerAvatar.sprite và opponentAvatar.sprite.
        /// TUYỆT ĐỐI KHÔNG thay đổi RectTransform, Position, Anchor, Pivot, SizeDelta, Scale hay Mask.
        /// </summary>
        private void UpdateAvatarSprites(VersusMatchResult res)
        {
            if (res == null) return;

            // 1. Avatar Bạn (PlayerAvatar)
            string pChar = res.playerCharacter;
            if (string.IsNullOrEmpty(pChar))
            {
                var save = PhysicsSaveManager.Load();
                pChar = save?.selectedCharacterId;
            }

            Sprite pSprite = LoadCharacterPortrait(true, pChar);
            if (pSprite == null && res.playerAvatarSprite != null && IsValidAvatarSprite(res.playerAvatarSprite))
            {
                pSprite = res.playerAvatarSprite;
            }
            if (pSprite == null) pSprite = defaultPlayerAvatarSprite;

            if (imgPlayerAvatar != null && pSprite != null && IsValidAvatarSprite(pSprite))
            {
                imgPlayerAvatar.sprite = pSprite;
                imgPlayerAvatar.color = Color.white;
            }

            // 2. Avatar Đối thủ (OpponentAvatar)
            string oChar = res.opponentCharacter;
            Sprite oSprite = LoadCharacterPortrait(false, oChar);
            if (oSprite == null && res.opponentAvatarSprite != null && IsValidAvatarSprite(res.opponentAvatarSprite))
            {
                oSprite = res.opponentAvatarSprite;
            }
            if (oSprite == null) oSprite = defaultOpponentAvatarSprite;

            if (imgOpponentAvatar != null && oSprite != null && IsValidAvatarSprite(oSprite))
            {
                imgOpponentAvatar.sprite = oSprite;
                imgOpponentAvatar.color = Color.white;
            }
        }

        public bool IsValidAvatarSprite(Sprite sp)
        {
            if (sp == null) return false;
            if (sp.texture != null)
            {
                string t = sp.texture.name.ToLower();
                if (t.Contains("bộ giao diện") || t.Contains("atlas") || t.Contains("khung") || t.Contains("button"))
                    return false;
            }
            string s = sp.name.ToLower();
            if (s.Contains("khung") || s.Contains("frame") || s.Contains("btn") || s.Contains("bg") || s.Contains("banner"))
                return false;

            if (sp.rect.width > 900f || sp.rect.height > 900f) return false;
            if (sp.rect.width < 10f || sp.rect.height < 10f) return false;
            float aspect = sp.rect.width / Mathf.Max(1f, sp.rect.height);
            if (aspect < 0.5f || aspect > 2.0f) return false;
            return true;
        }

        public Sprite LoadCharacterPortrait(bool isPlayer, string charId)
        {
            if (isPlayer)
            {
                if (string.IsNullOrEmpty(charId)) charId = "Minh";
            }
            else
            {
                if (string.IsNullOrEmpty(charId) || charId.Equals("Bot", StringComparison.OrdinalIgnoreCase) || charId.Contains("Đối thủ"))
                {
                    charId = "Duy";
                }
            }

            string cleanName = charId;
            if (cleanName.Equals("Hà", StringComparison.OrdinalIgnoreCase)) cleanName = "Ha";

            // 1. Ưu tiên load từ sprite sheet "Đôi Chân Dung Học Viện Aurelia" (Minh=_0, Duy=_2)
            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            if (cleanName.Equals("Minh", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var sp in allSprites)
                {
                    if (sp == null || !IsValidAvatarSprite(sp)) continue;
                    if (sp.name.Contains("Đôi Chân Dung") && sp.name.EndsWith("_0")) return sp;
                }
                if (defaultPlayerAvatarSprite != null && IsValidAvatarSprite(defaultPlayerAvatarSprite))
                    return defaultPlayerAvatarSprite;
            }
            else if (cleanName.Equals("Duy", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var sp in allSprites)
                {
                    if (sp == null || !IsValidAvatarSprite(sp)) continue;
                    if (sp.name.Contains("Đôi Chân Dung") && sp.name.EndsWith("_2")) return sp;
                }
                if (defaultOpponentAvatarSprite != null && IsValidAvatarSprite(defaultOpponentAvatarSprite))
                    return defaultOpponentAvatarSprite;
            }

            // 2. Load từ Resources/Characters/<charId> (Minh, Lan, Duy, Ha)
            var resSp = Resources.Load<Sprite>($"Characters/{cleanName}");
            if (resSp != null && IsValidAvatarSprite(resSp))
            {
                return resSp;
            }

            // 3. Tra cứu từ sprite sheet "Lưới sprite nhân vật và đèn hiệu" (Minh=_0, Lan=_3, Duy=_6, Ha=_5)
            string targetSub = "_0";
            if (cleanName.Equals("Lan", StringComparison.OrdinalIgnoreCase)) targetSub = "_3";
            else if (cleanName.Equals("Duy", StringComparison.OrdinalIgnoreCase)) targetSub = "_6";
            else if (cleanName.Equals("Ha", StringComparison.OrdinalIgnoreCase)) targetSub = "_5";
            else if (!isPlayer) targetSub = "_6";

            foreach (var sp in allSprites)
            {
                if (sp == null || !IsValidAvatarSprite(sp)) continue;
                if (sp.name.Contains("nhân vật và đèn hiệu") && sp.name.EndsWith(targetSub))
                {
                    return sp;
                }
            }

            // 4. Fallback trực tiếp
            return isPlayer ? (defaultPlayerAvatarSprite ?? Resources.Load<Sprite>("Characters/Minh"))
                            : (defaultOpponentAvatarSprite ?? Resources.Load<Sprite>("Characters/Duy"));
        }

        private void SetupButtonEvents()
        {
            if (btnReplay != null)
            {
                btnReplay.onClick.RemoveAllListeners();
                btnReplay.onClick.AddListener(OnReplayClicked);
            }

            if (btnBack != null)
            {
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(OnBackClicked);
            }
        }

        /// <summary>
        /// Nút Tái đấu: Reset trận mới (10 câu, timer 05:00, 0đ) và nạp lại Đấu trường (Scene 34.1).
        /// </summary>
        public void OnReplayClicked()
        {
            Debug.Log("[VersusResult] Người chơi bấm TÁI ĐẤU -> Bắt đầu trận mới.");
            VersusSessionState.CreateFreshMatchSession("Bot");
            Time.timeScale = 1f;
            NavigationService.Instance.ResetTransitionLock();
            NavigationService.Instance.NavigateTo(RouteId.VersusArena);
        }

        /// <summary>
        /// Nút Quay về menu: Xóa trạng thái thi đấu và load Scene 01_MainMenu.
        /// </summary>
        public void OnBackClicked()
        {
            Debug.Log("[VersusResult] Người chơi bấm QUAY VỀ MENU.");
            VersusSessionState.ClearSession();
            Time.timeScale = 1f;
            if (NavigationService.Instance != null)
            {
                if (NavigationService.Instance.Selection != null)
                {
                    NavigationService.Instance.Selection.Mode = "STUDY";
                }
                if (NavigationService.Instance.GameContext != null)
                {
                    NavigationService.Instance.GameContext.ReturnAction = "";
                }
                NavigationService.Instance.ResetTransitionLock();
                NavigationService.Instance.NavigateTo(RouteId.MainMenu);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(PhysicsSceneNames.MainMenu);
            }
        }
    }
}
