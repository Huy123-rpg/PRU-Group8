// ============================================================
// BiologyHUD.cs
// Giao diện game SINH TỒN MIỄN DỊCH:
//   • Máu người chơi (thanh hồng)
//   • Thanh kinh nghiệm (XP) + cấp độ
//   • Thanh hồi chiêu DASH
//   • Chuỗi câu hỏi đúng liên tiếp (STREAK)
//   • Thanh VOID THREAT (0-100%) - đầy → Boss xuất hiện
//   • Đồng hồ đếm ngược 5 phút tới chiến thắng
//   • Điểm + số mầm bệnh đã tiêu diệt
// Toàn bộ UI dựng bằng code → không cần setup tay trong Editor.
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PRU.Biology
{
    public class BiologyHUD : MonoBehaviour
    {
        public static BiologyHUD Instance { get; private set; }

        private Image _hpFill;
        private TextMeshProUGUI _hpText;
        private Image _xpFill;
        private TextMeshProUGUI _levelText;
        private Image _dashFill;
        private TextMeshProUGUI _dashText;
        private TextMeshProUGUI _streakText;
        private TextMeshProUGUI _timerText;
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _messageText;
        private Image _threatFill;
        private TextMeshProUGUI _threatText;
        private TextMeshProUGUI _bossWarningText;
        private float _bossWarningTimer;

        private void Awake()
        {
            Instance = this;
            BuildUI();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildUI()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
                scaler.matchWidthOrHeight = 0.5f;
                gameObject.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            // ---------- Máu trên trái ----------
            TextMeshProUGUI hpLabel = CreateText("HpLabel", "🧬 TẾ BÀO TRẮNG", 22, TextAlignmentOptions.Left,
                new Vector2(25f, -20f), new Vector2(360f, 34f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            hpLabel.color = new Color(0.25f, 0.5f, 0.9f);

            CreateImage("HpBg", new Vector2(25f, -58f), new Vector2(340f, 26f),
                new Color(0f, 0f, 0f, 0.35f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _hpFill = CreateFill("HpFill", new Vector2(25f, -58f), new Vector2(340f, 26f),
                new Color(0.9f, 0.3f, 0.4f));
            _hpText = CreateText("HpText", "Máu: 8/8", 19, TextAlignmentOptions.Left,
                new Vector2(25f, -92f), new Vector2(300f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            // ---------- XP dưới máu ----------
            CreateImage("XpBg", new Vector2(25f, -124f), new Vector2(340f, 18f),
                new Color(0f, 0f, 0f, 0.35f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _xpFill = CreateFill("XpFill", new Vector2(25f, -124f), new Vector2(340f, 18f),
                new Color(0.3f, 0.85f, 1f));
            _levelText = CreateText("LevelText", "Lv.1", 20, TextAlignmentOptions.Left,
                new Vector2(25f, -148f), new Vector2(300f, 30f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _levelText.color = new Color(0.2f, 0.7f, 0.9f);

            // ---------- Dash dưới XP ----------
            CreateImage("DashBg", new Vector2(25f, -182f), new Vector2(340f, 14f),
                new Color(0f, 0f, 0f, 0.35f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _dashFill = CreateFill("DashFill", new Vector2(25f, -182f), new Vector2(340f, 14f),
                new Color(0.55f, 1f, 0.75f));
            _dashText = CreateText("DashText", "SPACE - Dash sẵn sàng", 17, TextAlignmentOptions.Left,
                new Vector2(25f, -206f), new Vector2(340f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _dashText.color = new Color(0.7f, 1f, 0.8f);

            // ---------- VOID THREAT dưới Dash ----------
            CreateImage("ThreatBg", new Vector2(25f, -234f), new Vector2(340f, 16f),
                new Color(0f, 0f, 0f, 0.35f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _threatFill = CreateFill("ThreatFill", new Vector2(25f, -234f), new Vector2(340f, 16f),
                new Color(0.45f, 0.9f, 0.5f));
            _threatText = CreateText("ThreatText", "VOID THREAT: 0%", 15, TextAlignmentOptions.Left,
                new Vector2(25f, -256f), new Vector2(340f, 22f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _threatText.color = new Color(0.8f, 0.95f, 0.8f);

            // ---------- Streak dưới Threat ----------
            _streakText = CreateText("StreakText", "", 24, TextAlignmentOptions.Left,
                new Vector2(25f, -288f), new Vector2(400f, 34f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            _streakText.color = new Color(1f, 0.85f, 0.3f);
            _streakText.outlineWidth = 0.25f;

            // ---------- Đồng hồ trên giữa ----------
            _timerText = CreateText("TimerText", "3:00", 44, TextAlignmentOptions.Center,
                new Vector2(0f, -24f), new Vector2(300f, 60f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            _timerText.color = Color.white;
            _timerText.outlineWidth = 0.3f;

            // ---------- Điểm trên phải ----------
            _scoreText = CreateText("ScoreText", "Điểm: 0", 24, TextAlignmentOptions.Right,
                new Vector2(-25f, -24f), new Vector2(420f, 36f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            // ---------- Thông báo giữa màn ----------
            _messageText = CreateText("MessageText", "", 36, TextAlignmentOptions.Center,
                new Vector2(0f, 110f), new Vector2(1000f, 100f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _messageText.color = new Color(1f, 0.95f, 0.4f);
            _messageText.outlineWidth = 0.3f;
            _messageText.gameObject.SetActive(false);

            // ---------- Boss Warning (đếm ngược trước khi boss xuất hiện) ----------
            _bossWarningText = CreateText("BossWarning", "", 40, TextAlignmentOptions.Center,
                new Vector2(0f, -70f), new Vector2(1000f, 70f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _bossWarningText.color = new Color(1f, 0.25f, 0.25f);
            _bossWarningText.outlineWidth = 0.35f;
            _bossWarningText.gameObject.SetActive(false);
        }

        private void Update()
        {
            // ----- VOID THREAT bar + Boss Warning (không phụ thuộc player) -----
            UpdateThreatUI();
            UpdateBossWarning();

            // Cập nhật thanh dash mỗi frame (đọc từ player)
            BiologyWhiteCell pc = BiologyWhiteCell.Instance;
            if (pc == null || _dashFill == null) return;

            float ratio = 1f - pc.DashCooldownRatio; // 1 = sẵn sàng
            _dashFill.fillAmount = ratio;
            if (_dashText != null)
                _dashText.text = ratio >= 1f ? "SPACE - Dash sẵn sàng" : $"SPACE - Đang hồi ({pc.DashCooldownRatio * pc.DashCooldown:0.0}s)";
            _dashFill.color = ratio >= 1f ? new Color(0.55f, 1f, 0.75f) : new Color(0.45f, 0.55f, 0.6f);
        }

        // ----------------- Tiện ích dựng UI -----------------
        private TextMeshProUGUI CreateText(string name, string text, int size, TextAlignmentOptions align,
            Vector2 pos, Vector2 sizeDelta, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private GameObject CreateImage(string name, Vector2 pos, Vector2 sizeDelta, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            Image img = go.AddComponent<Image>();
            img.sprite = BiologySpriteFactory.GetWhiteSprite();
            img.color = color;
            return go;
        }

        private Image CreateFill(string name, Vector2 pos, Vector2 sizeDelta, Color color)
        {
            GameObject go = CreateImage(name, pos, sizeDelta, color, new Vector2(0f, 1f), new Vector2(0f, 1f));
            Image img = go.GetComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            return img;
        }

        // ----------------- VOID THREAT UI -----------------
        private void UpdateThreatUI()
        {
            if (_threatFill == null) return;
            // Nhấp nháy cảnh báo khi threat cao
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 5f);
            if (_threatFill.fillAmount >= 0.75f)
                _threatFill.color = Color.Lerp(new Color(1f, 0.45f, 0.2f), new Color(1f, 0.15f, 0.15f), pulse);
            else if (_threatFill.fillAmount >= 0.4f)
                _threatFill.color = new Color(1f, 0.75f, 0.25f);
            else
                _threatFill.color = new Color(0.45f, 0.9f, 0.5f);
        }

        private void UpdateBossWarning()
        {
            if (_bossWarningTimer <= 0f) return;
            _bossWarningTimer -= Time.unscaledDeltaTime;
            if (_bossWarningText == null) { _bossWarningTimer = 0f; return; }
            if (_bossWarningTimer <= 0f)
            {
                _bossWarningText.gameObject.SetActive(false);
                return;
            }
            _bossWarningText.text = $"☠ BOSS INCOMING IN {_bossWarningTimer:0.0}s";
            bool blink = Mathf.FloorToInt(_bossWarningTimer * 6f) % 2 == 0;
            _bossWarningText.color = blink ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.85f, 0.3f);
        }

        // ----------------- API công khai -----------------
        public void SetThreat(float ratio)
        {
            if (_threatFill != null) _threatFill.fillAmount = Mathf.Clamp01(ratio);
            if (_threatText != null) _threatText.text = $"VOID THREAT: {Mathf.RoundToInt(ratio * 100f)}%";
        }

        public void ShowBossWarning(float duration)
        {
            _bossWarningTimer = duration;
            if (_bossWarningText != null) _bossWarningText.gameObject.SetActive(true);
        }

        public void SetHp(int current, int max)
        {
            if (_hpFill != null) _hpFill.fillAmount = max > 0 ? (float)current / max : 0f;
            if (_hpText != null) _hpText.text = $"Máu: {current}/{max}";
        }

        public void SetXp(float current, float needed, int level)
        {
            if (_xpFill != null) _xpFill.fillAmount = needed > 0 ? Mathf.Clamp01(current / needed) : 0f;
            if (_levelText != null) _levelText.text = $"Lv.{level}  ({current:0}/{needed:0} XP)";
        }

        public void SetStreak(int streak)
        {
            if (_streakText == null) return;
            if (streak >= 2)
            {
                _streakText.text = $"🔥 STREAK x{streak}";
                _streakText.color = streak >= BiologyGameConfig.STREAK_AWAKENING_AT - 2
                    ? new Color(1f, 0.5f, 0.2f) : new Color(1f, 0.85f, 0.3f);
            }
            else
            {
                _streakText.text = "";
            }
        }

        public void SetTimer(float elapsed, float total)
        {
            if (_timerText == null) return;
            float remain = Mathf.Max(0f, total - elapsed);
            int m = Mathf.FloorToInt(remain / 60f);
            int s = Mathf.FloorToInt(remain % 60f);
            _timerText.text = $"{m}:{s:00}";
            _timerText.color = remain <= 30f ? new Color(1f, 0.5f, 0.4f) : Color.white;
        }

        public void SetScore(int score, int kills)
        {
            if (_scoreText != null)
                _scoreText.text = $"Điểm: {score}   🦠 {kills}";
        }

        public void ShowMessage(string msg, float duration)
        {
            if (_messageText == null) return;
            _messageText.text = msg;
            _messageText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideMessage));
            if (duration > 0f) Invoke(nameof(HideMessage), duration);
        }

        private void HideMessage()
        {
            if (_messageText != null) _messageText.gameObject.SetActive(false);
        }
    }
}
