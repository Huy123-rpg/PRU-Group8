using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển một trụ câu hỏi trong Đấu trường Đối kháng 1vs1.
    /// - Quản lý số thứ tự trụ (1 -> 10)
    /// - Hiển thị trạng thái chiếm:
    ///   + Chưa chiếm (Unclaimed): Sprite pha lê trung tính (Sprite 24), KHÔNG vòng sáng, Outline tắt.
    ///   + Bạn chiếm (ClaimedByPlayer): Sprite pha lê vòng sáng XANH (Sprite 26).
    ///   + Đối thủ chiếm (ClaimedByOpponent): Sprite pha lê vòng sáng VÀNG (Sprite 46).
    /// - Hiện badge tương tác "[E] Mở câu hỏi" khi người chơi ở gần
    /// - Khóa tương tác vĩnh viễn khi đã bị chiếm
    /// </summary>
    public class VersusTowerController : MonoBehaviour
    {
        [Header("=== PROGRESS STATE (INDEPENDENT) ===")]
        public int towerNumber = 1;
        public bool playerSolved = false;
        public bool opponentSolved = false;
        public float playerSolvedTime = 0f;
        public float opponentSolvedTime = 0f;
        public PhysicsQuestionRaw questionData;

        // Tương thích ngược với enum cũ
        public VersusTowerClaim claimState
        {
            get
            {
                if (playerSolved && opponentSolved) return VersusTowerClaim.Player;
                if (playerSolved) return VersusTowerClaim.Player;
                if (opponentSolved) return VersusTowerClaim.Opponent;
                return VersusTowerClaim.Unclaimed;
            }
            set
            {
                if (value == VersusTowerClaim.Player) playerSolved = true;
                else if (value == VersusTowerClaim.Opponent) opponentSolved = true;
                else if (value == VersusTowerClaim.Unclaimed) { playerSolved = false; opponentSolved = false; }
            }
        }

        [Header("=== SPRITE REFERENCES ===")]
        public Sprite neutralSprite;
        public Sprite playerSprite;
        public Sprite opponentSprite;

        [Header("=== VISUALS ===")]
        [SerializeField] private Image towerImage;
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private GameObject promptBadge;
        [SerializeField] private Image imgAuraPlayer;
        [SerializeField] private Image imgAuraOpponent;

        private Outline _outline;
        private Color _playerOutlineColor = new Color(0.2f, 0.8f, 1f, 1f);
        private Color _opponentOutlineColor = new Color(1f, 0.85f, 0.2f, 1f);

        public Vector2 Position
        {
            get
            {
                var rt = GetComponent<RectTransform>();
                return rt != null ? rt.anchoredPosition : (Vector2)transform.localPosition;
            }
        }

        public bool IsPlayerSolved => playerSolved;
        public bool IsOpponentSolved => opponentSolved;
        public bool CanPlayerInteract => !playerSolved;
        public bool IsClaimed => playerSolved && opponentSolved;

        private void Awake()
        {
            if (towerImage == null) towerImage = GetComponent<Image>();
            if (numberText == null) numberText = GetComponentInChildren<TextMeshProUGUI>(true);

            _outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
            _outline.enabled = false;

            var btn = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            btn.targetGraphic = towerImage;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnTowerClicked);

            CreatePromptBadge();
            EnsureAuraCircles();

            // Khởi tạo ban đầu: LUÔN ở trạng thái chưa chiếm, cả 2 hiệu ứng tắt
            ResetTower();
        }

        private void EnsureAuraCircles()
        {
            // Vòng sáng XANH (Tiến độ Bạn - Inner Ring)
            if (imgAuraPlayer == null)
            {
                var pTrans = transform.Find("Aura_Player");
                if (pTrans != null)
                {
                    imgAuraPlayer = pTrans.GetComponent<Image>();
                }
                else
                {
                    var pGo = new GameObject("Aura_Player");
                    pGo.transform.SetParent(transform, false);
                    pGo.transform.SetAsFirstSibling(); // Vẽ dưới pha lê
                    var pRt = pGo.AddComponent<RectTransform>();
                    pRt.anchorMin = new Vector2(0.5f, 0.5f);
                    pRt.anchorMax = new Vector2(0.5f, 0.5f);
                    pRt.pivot = new Vector2(0.5f, 0.5f);
                    pRt.anchoredPosition = Vector2.zero;
                    pRt.sizeDelta = new Vector2(105f, 105f); // Vòng sáng trong

                    imgAuraPlayer = pGo.AddComponent<Image>();
                    imgAuraPlayer.raycastTarget = false;
                    imgAuraPlayer.color = new Color(0.15f, 0.85f, 1f, 0.85f); // Xanh cyan sáng
                    var pOutline = pGo.AddComponent<Outline>();
                    pOutline.effectColor = new Color(0f, 0.9f, 1f, 0.95f);
                    pOutline.effectDistance = new Vector2(3f, -3f);
                }
            }

            // Vòng sáng VÀNG (Tiến độ Đối thủ - Outer Ring lớn hơn không che nhau)
            if (imgAuraOpponent == null)
            {
                var oTrans = transform.Find("Aura_Opponent");
                if (oTrans != null)
                {
                    imgAuraOpponent = oTrans.GetComponent<Image>();
                }
                else
                {
                    var oGo = new GameObject("Aura_Opponent");
                    oGo.transform.SetParent(transform, false);
                    oGo.transform.SetAsFirstSibling(); // Vẽ dưới pha lê
                    var oRt = oGo.AddComponent<RectTransform>();
                    oRt.anchorMin = new Vector2(0.5f, 0.5f);
                    oRt.anchorMax = new Vector2(0.5f, 0.5f);
                    oRt.pivot = new Vector2(0.5f, 0.5f);
                    oRt.anchoredPosition = Vector2.zero;
                    oRt.sizeDelta = new Vector2(135f, 135f); // Vòng sáng ngoài (đồng tâm, lớn hơn 30px)

                    imgAuraOpponent = oGo.AddComponent<Image>();
                    imgAuraOpponent.raycastTarget = false;
                    imgAuraOpponent.color = new Color(1f, 0.82f, 0.2f, 0.85f); // Vàng kim sáng
                    var oOutline = oGo.AddComponent<Outline>();
                    oOutline.effectColor = new Color(1f, 0.75f, 0.1f, 0.95f);
                    oOutline.effectDistance = new Vector2(4f, -4f);
                }
            }

            if (playerSprite != null && imgAuraPlayer != null && imgAuraPlayer.sprite == null)
            {
                imgAuraPlayer.sprite = playerSprite;
            }
            if (opponentSprite != null && imgAuraOpponent != null && imgAuraOpponent.sprite == null)
            {
                imgAuraOpponent.sprite = opponentSprite;
            }
        }

        public void ConfigureSprites(Sprite neutral, Sprite player, Sprite opponent)
        {
            if (neutral != null) neutralSprite = neutral;
            if (player != null)
            {
                playerSprite = player;
                if (imgAuraPlayer != null) imgAuraPlayer.sprite = player;
            }
            if (opponent != null)
            {
                opponentSprite = opponent;
                if (imgAuraOpponent != null) imgAuraOpponent.sprite = opponent;
            }

            UpdateVisuals();
        }

        public void ResetTower()
        {
            playerSolved = false;
            opponentSolved = false;
            playerSolvedTime = 0f;
            opponentSolvedTime = 0f;

            if (towerImage != null)
            {
                if (neutralSprite != null) towerImage.sprite = neutralSprite;
                towerImage.color = Color.white;
            }

            if (imgAuraPlayer != null) imgAuraPlayer.gameObject.SetActive(false);
            if (imgAuraOpponent != null) imgAuraOpponent.gameObject.SetActive(false);

            if (_outline != null) _outline.enabled = false;
            if (numberText != null)
            {
                numberText.text = $"{towerNumber}";
            }
            SetPromptVisible(false);
        }

        private void OnTowerClicked()
        {
            if (playerSolved || VersusArenaController.Instance == null || VersusArenaController.Instance.IsInputLocked) return;
            VersusArenaController.Instance.OpenQuestionModal(towerNumber);
        }

        public void SetPlayerSolved(bool solved)
        {
            playerSolved = solved;
            if (solved) playerSolvedTime = Time.realtimeSinceStartup;
            SetPromptVisible(false);
            UpdateVisuals();
        }

        public void SetOpponentSolved(bool solved)
        {
            opponentSolved = solved;
            if (solved) opponentSolvedTime = Time.realtimeSinceStartup;
            UpdateVisuals();
        }

        public void SetClaimed(VersusTowerClaim claim)
        {
            if (claim == VersusTowerClaim.Player) SetPlayerSolved(true);
            else if (claim == VersusTowerClaim.Opponent) SetOpponentSolved(true);
        }

        public void UpdateVisuals()
        {
            EnsureAuraCircles();

            // 1. Quản lý 2 vòng sáng độc lập
            if (imgAuraPlayer != null)
            {
                imgAuraPlayer.gameObject.SetActive(playerSolved);
            }
            if (imgAuraOpponent != null)
            {
                imgAuraOpponent.gameObject.SetActive(opponentSolved);
            }

            // 2. Sprite pha lê chính và Outline
            if (towerImage != null)
            {
                if (playerSolved && !opponentSolved && playerSprite != null)
                {
                    towerImage.sprite = playerSprite;
                }
                else if (!playerSolved && opponentSolved && opponentSprite != null)
                {
                    towerImage.sprite = opponentSprite;
                }
                else if (playerSolved && opponentSolved)
                {
                    // Cả 2 đều đúng: Pha lê ở giữa rực rỡ, cả 2 vòng xanh & vàng cùng tỏa sáng
                    towerImage.sprite = playerSprite != null ? playerSprite : neutralSprite;
                }
                else if (neutralSprite != null)
                {
                    towerImage.sprite = neutralSprite;
                }
                towerImage.color = Color.white;
            }

            if (_outline != null)
            {
                if (playerSolved && opponentSolved)
                {
                    _outline.enabled = true;
                    _outline.effectColor = new Color(0.6f, 0.9f, 0.6f, 1f); // Xanh pha vàng ánh kim
                    _outline.effectDistance = new Vector2(3f, -3f);
                }
                else if (playerSolved)
                {
                    _outline.enabled = true;
                    _outline.effectColor = _playerOutlineColor;
                    _outline.effectDistance = new Vector2(3f, -3f);
                }
                else if (opponentSolved)
                {
                    _outline.enabled = true;
                    _outline.effectColor = _opponentOutlineColor;
                    _outline.effectDistance = new Vector2(3f, -3f);
                }
                else
                {
                    _outline.enabled = false;
                }
            }

            // 3. Cập nhật nhãn số thứ tự trụ
            if (numberText != null)
            {
                if (playerSolved && opponentSolved)
                {
                    numberText.text = $"{towerNumber} <color=#38b6ff>(Bạn)</color> <color=#ffd700>(Đối thủ)</color>";
                }
                else if (playerSolved)
                {
                    numberText.text = $"{towerNumber} <color=#38b6ff>(Bạn)</color>";
                }
                else if (opponentSolved)
                {
                    numberText.text = $"{towerNumber} <color=#ffd700>(Đối thủ)</color>";
                }
                else
                {
                    numberText.text = $"{towerNumber}";
                }
            }
        }

        private void CreatePromptBadge()
        {
            if (promptBadge != null) return;

            promptBadge = new GameObject("PromptBadge");
            promptBadge.transform.SetParent(transform, false);

            var rt = promptBadge.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 95f);
            rt.sizeDelta = new Vector2(140f, 32f);

            var bg = promptBadge.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.12f, 0.25f, 0.95f);

            var outline = promptBadge.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.35f, 1f);
            outline.effectDistance = new Vector2(1f, -1f);

            var badgeBtn = promptBadge.AddComponent<Button>();
            badgeBtn.targetGraphic = bg;
            badgeBtn.onClick.AddListener(OnTowerClicked);

            var tGo = new GameObject("Text");
            tGo.transform.SetParent(promptBadge.transform, false);
            var tRt = tGo.AddComponent<RectTransform>();
            tRt.sizeDelta = rt.sizeDelta;
            tRt.anchoredPosition = Vector2.zero;

            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "[E] MỞ CÂU HỎI";
            tmp.fontSize = 13f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.95f, 0.6f, 1f);
            tmp.raycastTarget = false;

            promptBadge.SetActive(false);
        }

        public void SetPromptVisible(bool visible)
        {
            if (promptBadge != null)
            {
                // Chỉ hiển thị badge gợi ý khi Player chưa giải đúng trụ này
                promptBadge.SetActive(visible && !playerSolved);
            }
        }
    }
}
