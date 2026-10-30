using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Reusable Story Modal duy nhất trên mỗi Scene cho Aurelia Academy.
    /// Tự động dựng giao diện Navy/Gold chuẩn của học viện dưới Canvas hiện tại,
    /// đảm bảo chống duplicate tuyệt đối và an toàn raycast.
    /// </summary>
    public class StoryModalController : MonoBehaviour
    {
        private GameObject _modalRoot;
        private Image _blockerImage;
        private RectTransform _cardRect;
        private TextMeshProUGUI _titleTmp;
        private GameObject _speakerBadgeObj;
        private TextMeshProUGUI _speakerTmp;
        private GameObject _portraitObj;
        private Image _portraitImg;
        private RectTransform _bodyRect;
        private TextMeshProUGUI _bodyTmp;
        private GameObject _missionPanelObj;
        private TextMeshProUGUI _missionTmp;
        private GameObject _rewardPanelObj;
        private TextMeshProUGUI _rewardTmp;
        private Button _btnContinue;
        private TextMeshProUGUI _btnContinueTmp;
        private Button _btnClose;

        private TMP_FontAsset _defaultFont;
        private StoryContent _currentContent;
        private Action _onModalClosedCallback;

        private RectTransform _parchmentRect;

        public bool IsShowing => _modalRoot != null && _modalRoot.activeSelf;
        public StoryContent CurrentContent => _currentContent;
        public Image BlockerImage => _blockerImage;
        public RectTransform CardRect => _cardRect;
        public Button ContinueButton => _btnContinue;
        public Button CloseButton => _btnClose;
        public GameObject MissionPanel => _missionPanelObj;
        public GameObject RewardPanel => _rewardPanelObj;
        public TextMeshProUGUI TitleText => _titleTmp;
        public TextMeshProUGUI SpeakerText => _speakerTmp;
        public TextMeshProUGUI BodyText => _bodyTmp;
        public TextMeshProUGUI MissionText => _missionTmp;
        public TextMeshProUGUI RewardText => _rewardTmp;

        /// <summary>
        /// Tìm và tái sử dụng StoryModalController sẵn có trong scene, hoặc khởi tạo mới dưới Canvas hiện tại.
        /// Đảm bảo chỉ có tối đa 1 StoryModalController tồn tại trong mỗi scene.
        /// </summary>
        public static StoryModalController GetOrCreate(Transform preferredCanvasTransform = null)
        {
            StoryModalController existing = FindAnyObjectByType<StoryModalController>(FindObjectsInactive.Include);
            if (existing != null)
            {
                existing.EnsureUIInitialized();
                existing.transform.SetAsLastSibling();
                return existing;
            }

            EnsureSingleEventSystem();

            Canvas targetCanvas = null;
            if (preferredCanvasTransform != null)
            {
                targetCanvas = preferredCanvasTransform.GetComponentInParent<Canvas>() ?? preferredCanvasTransform.GetComponent<Canvas>();
            }
            if (targetCanvas == null)
            {
                targetCanvas = FindAnyObjectByType<Canvas>();
            }
            if (targetCanvas == null)
            {
                targetCanvas = CreateFallbackCanvas();
            }

            GameObject modalGo = new GameObject("AureliaStoryModal");
            modalGo.transform.SetParent(targetCanvas.transform, false);
            modalGo.transform.SetAsLastSibling();

            StoryModalController controller = modalGo.AddComponent<StoryModalController>();
            controller.EnsureUIInitialized();
            return controller;
        }

        private void Awake()
        {
            StoryModalController[] all = FindObjectsByType<StoryModalController>(FindObjectsInactive.Include);
            if (all != null && all.Length > 1)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i] != this)
                    {
                        DestroyImmediate(gameObject);
                        return;
                    }
                }
            }

            EnsureSingleEventSystem();
            EnsureUIInitialized();
        }

        public static void EnsureSingleEventSystem()
        {
            EventSystem es = FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                if (!es.gameObject.activeSelf) es.gameObject.SetActive(true);
                if (es.GetComponent<InputSystemUIInputModule>() == null && es.GetComponent<BaseInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
        }

        private static Canvas CreateFallbackCanvas()
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public void EnsureUIInitialized()
        {
            if (_modalRoot != null) return;

            // Tìm font TMP đang dùng trong scene để đồng bộ hiển thị tiếng Việt
            TextMeshProUGUI anyTmp = FindAnyObjectByType<TextMeshProUGUI>(FindObjectsInactive.Include);
            if (anyTmp != null && anyTmp.font != null)
            {
                _defaultFont = anyTmp.font;
            }

            _modalRoot = gameObject;
            RectTransform rootRt = _modalRoot.GetComponent<RectTransform>();
            if (rootRt == null) rootRt = _modalRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            // 1. Background Blocker (alpha = 0.52f để nhìn rõ môi trường Gameplay Aurelia phía sau, raycastTarget = true)
            _blockerImage = _modalRoot.GetComponent<Image>();
            if (_blockerImage == null) _blockerImage = _modalRoot.AddComponent<Image>();
            _blockerImage.color = new Color(0.02f, 0.05f, 0.12f, 0.52f);
            _blockerImage.raycastTarget = true;

            // 2. Main Aurelia Navy/Gold Card (~58.3% width, ~59.3% height trên 1920x1080)
            GameObject cardGo = new GameObject("StoryCard");
            cardGo.transform.SetParent(_modalRoot.transform, false);
            _cardRect = cardGo.AddComponent<RectTransform>();
            _cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            _cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            _cardRect.pivot = new Vector2(0.5f, 0.5f);
            _cardRect.anchoredPosition = Vector2.zero;
            _cardRect.sizeDelta = new Vector2(1120f, 640f);

            Image cardImg = cardGo.AddComponent<Image>();
            cardImg.color = new Color(0.06f, 0.11f, 0.22f, 0.96f);
            cardImg.raycastTarget = false;

            Outline outerOutline = cardGo.AddComponent<Outline>();
            outerOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 1f);
            outerOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Inner Decorative Gold Frame
            GameObject innerBorderGo = new GameObject("InnerBorder");
            innerBorderGo.transform.SetParent(cardGo.transform, false);
            RectTransform innerRt = innerBorderGo.AddComponent<RectTransform>();
            innerRt.anchorMin = Vector2.zero;
            innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = new Vector2(12f, 12f);
            innerRt.offsetMax = new Vector2(-12f, -12f);

            Image innerImg = innerBorderGo.AddComponent<Image>();
            innerImg.color = new Color(0.08f, 0.14f, 0.27f, 0.70f);
            innerImg.raycastTarget = false;

            Outline innerOutline = innerBorderGo.AddComponent<Outline>();
            innerOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.55f);
            innerOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Warm Parchment / Light Center Reading Area
            GameObject parchmentGo = new GameObject("ParchmentCenter");
            parchmentGo.transform.SetParent(cardGo.transform, false);
            _parchmentRect = parchmentGo.AddComponent<RectTransform>();
            _parchmentRect.anchorMin = new Vector2(0.5f, 0.5f);
            _parchmentRect.anchorMax = new Vector2(0.5f, 0.5f);
            _parchmentRect.pivot = new Vector2(0.5f, 0.5f);
            _parchmentRect.anchoredPosition = new Vector2(0f, 45f);
            _parchmentRect.sizeDelta = new Vector2(1030f, 300f);

            Image parchmentImg = parchmentGo.AddComponent<Image>();
            parchmentImg.color = new Color(0.97f, 0.94f, 0.86f, 0.97f);
            parchmentImg.raycastTarget = false;

            Outline parchmentOutline = parchmentGo.AddComponent<Outline>();
            parchmentOutline.effectColor = new Color(0.82f, 0.66f, 0.28f, 0.85f);
            parchmentOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // 3. Header Title
            GameObject titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(cardGo.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, 256f);
            titleRt.sizeDelta = new Vector2(980f, 54f);

            _titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _titleTmp.font = _defaultFont;
            _titleTmp.fontSize = 28f;
            _titleTmp.enableAutoSizing = true;
            _titleTmp.fontSizeMin = 20f;
            _titleTmp.fontSizeMax = 30f;
            _titleTmp.fontStyle = FontStyles.Bold;
            _titleTmp.alignment = TextAlignmentOptions.Center;
            _titleTmp.textWrappingMode = TextWrappingModes.Normal;
            _titleTmp.color = new Color(0.98f, 0.85f, 0.40f, 1f);
            _titleTmp.raycastTarget = false;

            // 4. Optional Speaker Badge
            _speakerBadgeObj = new GameObject("SpeakerBadge");
            _speakerBadgeObj.transform.SetParent(cardGo.transform, false);
            RectTransform spkRt = _speakerBadgeObj.AddComponent<RectTransform>();
            spkRt.anchorMin = new Vector2(0.5f, 0.5f);
            spkRt.anchorMax = new Vector2(0.5f, 0.5f);
            spkRt.pivot = new Vector2(0.5f, 0.5f);
            spkRt.anchoredPosition = new Vector2(0f, 198f);
            spkRt.sizeDelta = new Vector2(380f, 40f);

            Image spkBg = _speakerBadgeObj.AddComponent<Image>();
            spkBg.color = new Color(0.10f, 0.20f, 0.38f, 0.98f);
            spkBg.raycastTarget = false;

            Outline spkOutline = _speakerBadgeObj.AddComponent<Outline>();
            spkOutline.effectColor = new Color(0.96f, 0.82f, 0.38f, 0.90f);
            spkOutline.effectDistance = new Vector2(1f, -1f);

            GameObject spkTxtGo = new GameObject("SpeakerText");
            spkTxtGo.transform.SetParent(_speakerBadgeObj.transform, false);
            RectTransform spkTxtRt = spkTxtGo.AddComponent<RectTransform>();
            spkTxtRt.anchorMin = Vector2.zero;
            spkTxtRt.anchorMax = Vector2.one;
            spkTxtRt.offsetMin = new Vector2(10f, 2f);
            spkTxtRt.offsetMax = new Vector2(-10f, -2f);

            _speakerTmp = spkTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _speakerTmp.font = _defaultFont;
            _speakerTmp.fontSize = 21f;
            _speakerTmp.enableAutoSizing = true;
            _speakerTmp.fontSizeMin = 16f;
            _speakerTmp.fontSizeMax = 22f;
            _speakerTmp.fontStyle = FontStyles.Bold;
            _speakerTmp.alignment = TextAlignmentOptions.Center;
            _speakerTmp.color = new Color(0.98f, 0.86f, 0.42f, 1f);
            _speakerTmp.raycastTarget = false;

            // 5. Optional Speaker Portrait (bên trái nếu có)
            _portraitObj = new GameObject("SpeakerPortrait");
            _portraitObj.transform.SetParent(cardGo.transform, false);
            RectTransform portRt = _portraitObj.AddComponent<RectTransform>();
            portRt.anchorMin = new Vector2(0.5f, 0.5f);
            portRt.anchorMax = new Vector2(0.5f, 0.5f);
            portRt.pivot = new Vector2(0.5f, 0.5f);
            portRt.anchoredPosition = new Vector2(-390f, 35f);
            portRt.sizeDelta = new Vector2(150f, 190f);

            _portraitImg = _portraitObj.AddComponent<Image>();
            _portraitImg.preserveAspect = true;
            _portraitImg.raycastTarget = false;
            _portraitObj.SetActive(false);

            // 6. Body Text (High-contrast Deep Navy trên nền Parchment sáng ấm)
            GameObject bodyGo = new GameObject("BodyText");
            bodyGo.transform.SetParent(cardGo.transform, false);
            _bodyRect = bodyGo.AddComponent<RectTransform>();
            _bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
            _bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
            _bodyRect.pivot = new Vector2(0.5f, 0.5f);
            _bodyRect.anchoredPosition = new Vector2(0f, 45f);
            _bodyRect.sizeDelta = new Vector2(960f, 250f);

            _bodyTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _bodyTmp.font = _defaultFont;
            _bodyTmp.fontSize = 21f;
            _bodyTmp.enableAutoSizing = true;
            _bodyTmp.fontSizeMin = 16f;
            _bodyTmp.fontSizeMax = 22f;
            _bodyTmp.lineSpacing = 8f;
            _bodyTmp.alignment = TextAlignmentOptions.Center;
            _bodyTmp.textWrappingMode = TextWrappingModes.Normal;
            _bodyTmp.overflowMode = TextOverflowModes.Overflow;
            _bodyTmp.color = new Color(0.10f, 0.15f, 0.26f, 1f);
            _bodyTmp.raycastTarget = false;

            // 7. Optional Mission Panel
            _missionPanelObj = new GameObject("MissionPanel");
            _missionPanelObj.transform.SetParent(cardGo.transform, false);
            RectTransform misRt = _missionPanelObj.AddComponent<RectTransform>();
            misRt.anchorMin = new Vector2(0.5f, 0.5f);
            misRt.anchorMax = new Vector2(0.5f, 0.5f);
            misRt.pivot = new Vector2(0.5f, 0.5f);
            misRt.anchoredPosition = new Vector2(0f, -132f);
            misRt.sizeDelta = new Vector2(960f, 90f);

            Image misBg = _missionPanelObj.AddComponent<Image>();
            misBg.color = new Color(0.10f, 0.19f, 0.36f, 0.96f);
            misBg.raycastTarget = false;

            Outline misOutline = _missionPanelObj.AddComponent<Outline>();
            misOutline.effectColor = new Color(0.98f, 0.82f, 0.35f, 0.85f);
            misOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject misTxtGo = new GameObject("MissionText");
            misTxtGo.transform.SetParent(_missionPanelObj.transform, false);
            RectTransform misTxtRt = misTxtGo.AddComponent<RectTransform>();
            misTxtRt.anchorMin = Vector2.zero;
            misTxtRt.anchorMax = Vector2.one;
            misTxtRt.offsetMin = new Vector2(18f, 8f);
            misTxtRt.offsetMax = new Vector2(-18f, -8f);

            _missionTmp = misTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _missionTmp.font = _defaultFont;
            _missionTmp.fontSize = 18f;
            _missionTmp.enableAutoSizing = true;
            _missionTmp.fontSizeMin = 14f;
            _missionTmp.fontSizeMax = 20f;
            _missionTmp.alignment = TextAlignmentOptions.Center;
            _missionTmp.textWrappingMode = TextWrappingModes.Normal;
            _missionTmp.color = Color.white;
            _missionTmp.raycastTarget = false;

            // 8. Optional Reward Panel
            _rewardPanelObj = new GameObject("RewardPanel");
            _rewardPanelObj.transform.SetParent(cardGo.transform, false);
            RectTransform rewRt = _rewardPanelObj.AddComponent<RectTransform>();
            rewRt.anchorMin = new Vector2(0.5f, 0.5f);
            rewRt.anchorMax = new Vector2(0.5f, 0.5f);
            rewRt.pivot = new Vector2(0.5f, 0.5f);
            rewRt.anchoredPosition = new Vector2(0f, -132f);
            rewRt.sizeDelta = new Vector2(960f, 96f);

            Image rewBg = _rewardPanelObj.AddComponent<Image>();
            rewBg.color = new Color(0.09f, 0.20f, 0.34f, 0.96f);
            rewBg.raycastTarget = false;

            Outline rewOutline = _rewardPanelObj.AddComponent<Outline>();
            rewOutline.effectColor = new Color(0.00f, 0.88f, 1.00f, 0.85f);
            rewOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject rewTxtGo = new GameObject("RewardText");
            rewTxtGo.transform.SetParent(_rewardPanelObj.transform, false);
            RectTransform rewTxtRt = rewTxtGo.AddComponent<RectTransform>();
            rewTxtRt.anchorMin = Vector2.zero;
            rewTxtRt.anchorMax = Vector2.one;
            rewTxtRt.offsetMin = new Vector2(18f, 8f);
            rewTxtRt.offsetMax = new Vector2(-18f, -8f);

            _rewardTmp = rewTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _rewardTmp.font = _defaultFont;
            _rewardTmp.fontSize = 18f;
            _rewardTmp.enableAutoSizing = true;
            _rewardTmp.fontSizeMin = 14f;
            _rewardTmp.fontSizeMax = 20f;
            _rewardTmp.alignment = TextAlignmentOptions.Center;
            _rewardTmp.textWrappingMode = TextWrappingModes.Normal;
            _rewardTmp.color = Color.white;
            _rewardTmp.raycastTarget = false;

            // 9. Continue Button (Actual interactive button -> raycastTarget = true)
            GameObject btnContGo = new GameObject("Btn_StoryContinue");
            btnContGo.transform.SetParent(cardGo.transform, false);
            RectTransform btnContRt = btnContGo.AddComponent<RectTransform>();
            btnContRt.anchorMin = new Vector2(0.5f, 0.5f);
            btnContRt.anchorMax = new Vector2(0.5f, 0.5f);
            btnContRt.pivot = new Vector2(0.5f, 0.5f);
            btnContRt.anchoredPosition = new Vector2(0f, -248f);
            btnContRt.sizeDelta = new Vector2(340f, 54f);

            Image btnContImg = btnContGo.AddComponent<Image>();
            btnContImg.color = new Color(0.96f, 0.82f, 0.38f, 1f);
            btnContImg.raycastTarget = true;

            _btnContinue = btnContGo.AddComponent<Button>();
            _btnContinue.targetGraphic = btnContImg;

            Outline btnContOutline = btnContGo.AddComponent<Outline>();
            btnContOutline.effectColor = new Color(1f, 0.95f, 0.72f, 0.9f);
            btnContOutline.effectDistance = new Vector2(1f, -1f);

            GameObject btnContTxtGo = new GameObject("Text");
            btnContTxtGo.transform.SetParent(btnContGo.transform, false);
            RectTransform btnContTxtRt = btnContTxtGo.AddComponent<RectTransform>();
            btnContTxtRt.anchorMin = Vector2.zero;
            btnContTxtRt.anchorMax = Vector2.one;
            btnContTxtRt.offsetMin = new Vector2(10f, 4f);
            btnContTxtRt.offsetMax = new Vector2(-10f, -4f);

            _btnContinueTmp = btnContTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) _btnContinueTmp.font = _defaultFont;
            _btnContinueTmp.text = "TIẾP TỤC";
            _btnContinueTmp.fontSize = 20f;
            _btnContinueTmp.enableAutoSizing = true;
            _btnContinueTmp.fontSizeMin = 15f;
            _btnContinueTmp.fontSizeMax = 21f;
            _btnContinueTmp.fontStyle = FontStyles.Bold;
            _btnContinueTmp.alignment = TextAlignmentOptions.Center;
            _btnContinueTmp.color = new Color(0.07f, 0.11f, 0.22f, 1f);
            _btnContinueTmp.raycastTarget = false;

            _btnContinue.onClick.RemoveAllListeners();
            _btnContinue.onClick.AddListener(OnContinueClicked);

            // 10. Optional Close Button (chỉ bật khi allowCloseButton = true)
            GameObject btnCloseGo = new GameObject("Btn_StoryClose");
            btnCloseGo.transform.SetParent(cardGo.transform, false);
            RectTransform btnCloseRt = btnCloseGo.AddComponent<RectTransform>();
            btnCloseRt.anchorMin = new Vector2(1f, 1f);
            btnCloseRt.anchorMax = new Vector2(1f, 1f);
            btnCloseRt.pivot = new Vector2(1f, 1f);
            btnCloseRt.anchoredPosition = new Vector2(-22f, -22f);
            btnCloseRt.sizeDelta = new Vector2(42f, 42f);

            Image btnCloseImg = btnCloseGo.AddComponent<Image>();
            btnCloseImg.color = new Color(0.18f, 0.24f, 0.38f, 0.92f);
            btnCloseImg.raycastTarget = true;

            _btnClose = btnCloseGo.AddComponent<Button>();
            _btnClose.targetGraphic = btnCloseImg;

            GameObject btnCloseTxtGo = new GameObject("Text");
            btnCloseTxtGo.transform.SetParent(btnCloseGo.transform, false);
            RectTransform btnCloseTxtRt = btnCloseTxtGo.AddComponent<RectTransform>();
            btnCloseTxtRt.anchorMin = Vector2.zero;
            btnCloseTxtRt.anchorMax = Vector2.one;
            btnCloseTxtRt.offsetMin = Vector2.zero;
            btnCloseTxtRt.offsetMax = Vector2.zero;

            TextMeshProUGUI closeTmp = btnCloseTxtGo.AddComponent<TextMeshProUGUI>();
            if (_defaultFont != null) closeTmp.font = _defaultFont;
            closeTmp.text = "✕";
            closeTmp.fontSize = 20f;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.color = Color.white;
            closeTmp.raycastTarget = false;

            _btnClose.onClick.RemoveAllListeners();
            _btnClose.onClick.AddListener(OnCloseClicked);
            _btnClose.gameObject.SetActive(false);

            _modalRoot.SetActive(false);
        }

        /// <summary>
        /// Hiển thị nội dung StoryContent lên modal.
        /// </summary>
        public void ShowStory(StoryContent content, Action onClosed = null)
        {
            if (content == null) return;

            EnsureUIInitialized();
            transform.SetAsLastSibling();

            _currentContent = content;
            _onModalClosedCallback = onClosed;

            // 1. Title
            if (_titleTmp != null)
            {
                _titleTmp.text = !string.IsNullOrEmpty(content.title) ? content.title : "AURELIA ACADEMY";
            }

            // 2. Speaker
            bool hasSpeaker = !string.IsNullOrEmpty(content.speaker);
            if (_speakerBadgeObj != null)
            {
                _speakerBadgeObj.SetActive(hasSpeaker);
                if (hasSpeaker && _speakerTmp != null)
                {
                    _speakerTmp.text = content.speaker;
                }
            }

            // 3. Optional Portrait
            bool hasPortrait = content.speakerPortrait != null;
            if (_portraitObj != null && _portraitImg != null)
            {
                _portraitObj.SetActive(hasPortrait);
                _portraitImg.sprite = content.speakerPortrait;
            }

            // 4. Optional Mission
            bool hasMission = !string.IsNullOrEmpty(content.mission);
            if (_missionPanelObj != null)
            {
                _missionPanelObj.SetActive(hasMission);
                if (hasMission && _missionTmp != null)
                {
                    _missionTmp.text = $"<b><color=#FFD54F>NHIỆM VỤ</color></b>\n{content.mission}";
                }
            }

            // 5. Optional Reward (chỉ hiển thị nếu không bật Mission hoặc xếp hợp lý)
            bool hasReward = !string.IsNullOrEmpty(content.rewardTitle);
            if (_rewardPanelObj != null)
            {
                _rewardPanelObj.SetActive(hasReward);
                if (hasReward && _rewardTmp != null)
                {
                    if (!string.IsNullOrEmpty(content.rewardDescription))
                    {
                        _rewardTmp.text = $"<size=15><color=#FFD54F>VẬT PHẨM ĐÃ KHÔI PHỤC:</color></size> <b><color=#00E5FF>{content.rewardTitle}</color></b>\n" +
                                          $"<i>{content.rewardDescription}</i>";
                    }
                    else
                    {
                        _rewardTmp.text = $"<size=15><color=#FFD54F>VẬT PHẨM ĐÃ KHÔI PHỤC:</color></size>\n<b><color=#00E5FF>{content.rewardTitle}</color></b>";
                    }
                }
            }

            // 6. Căn chỉnh vùng Parchment & Body Text linh hoạt để không bao giờ đè lên Speaker / Mission / Reward
            if (_bodyRect != null)
            {
                bool hasBottomBox = hasMission || hasReward;
                float topY = hasSpeaker ? 162f : 204f;
                float bottomY = hasBottomBox ? -72f : -186f;
                float centerY = (topY + bottomY) * 0.5f;
                float height = Mathf.Max(140f, topY - bottomY);

                float posX = hasPortrait ? 75f : 0f;
                float width = hasPortrait ? 780f : 960f;

                _bodyRect.anchoredPosition = new Vector2(posX, centerY);
                _bodyRect.sizeDelta = new Vector2(width, height);

                if (_parchmentRect != null)
                {
                    _parchmentRect.anchoredPosition = new Vector2(0f, centerY);
                    _parchmentRect.sizeDelta = new Vector2(1030f, height + 24f);
                }
            }

            if (_bodyTmp != null)
            {
                _bodyTmp.text = content.body ?? "";
            }

            // 7. Button Text & Close Button
            if (_btnContinueTmp != null)
            {
                _btnContinueTmp.text = !string.IsNullOrEmpty(content.buttonText) ? content.buttonText : "TIẾP TỤC";
            }

            if (_btnClose != null)
            {
                _btnClose.gameObject.SetActive(content.allowCloseButton);
            }

            _modalRoot.SetActive(true);
        }

        public void HideModal()
        {
            if (_modalRoot != null)
            {
                _modalRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Xác nhận/đóng story hiện tại (tương đương người chơi nhấn nút Continue).
        /// </summary>
        public void AcknowledgeCurrentStory()
        {
            OnContinueClicked();
        }

        private void OnContinueClicked()
        {
            StoryContent finishedContent = _currentContent;
            Action closedCallback = _onModalClosedCallback;

            _currentContent = null;
            _onModalClosedCallback = null;

            if (_modalRoot != null)
            {
                _modalRoot.SetActive(false);
            }

            finishedContent?.onContinue?.Invoke();
            closedCallback?.Invoke();
        }

        private void OnCloseClicked()
        {
            OnContinueClicked();
        }
    }
}
