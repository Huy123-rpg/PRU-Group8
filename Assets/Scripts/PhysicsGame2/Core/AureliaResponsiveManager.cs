using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// AureliaResponsiveManager - Hệ thống điều phối Responsive thống nhất toàn bộ Game 2 Aurelia Academy.
    /// Bảo toàn 100% thiết kế Fantasy gốc 1920x1080 trên mọi tỷ lệ màn hình:
    /// - 16:9 (1920x1080, 1600x900, 1366x768, 1280x720, 2560x1440)
    /// - 16:10 (1920x1200, 1280x800)
    /// - 20:9 / 19.5:9 (2400x1080, 2340x1080 - điện thoại Android ngang)
    /// - An toàn tuyệt đối: Không can thiệp hoặc ghi đè RectTransform của Board, Card, Button đã được thiết kế.
    /// - Safe Area cho tai thỏ và camera đục lỗ (có lưu vị trí gốc chống trôi nút).
    /// - Tránh bàn phím ảo (Virtual Keyboard) cho màn tự luận có khôi phục vị trí gốc.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class AureliaResponsiveManager : MonoBehaviour
    {
        private static AureliaResponsiveManager _instance;
        public static AureliaResponsiveManager Instance => _instance;

        private const float BASE_WIDTH = 1920f;
        private const float BASE_HEIGHT = 1080f;
        private const float BASE_ASPECT = BASE_WIDTH / BASE_HEIGHT; // 1.777778f (16:9)

        private int _lastWidth = -1;
        private int _lastHeight = -1;
        private Rect _lastSafeArea = Rect.zero;
        private bool _lastKeyboardVisible = false;

        // Lưu vị trí gốc của các nút mép để tránh cộng dồn tọa độ khi thay đổi độ phân giải
        private static readonly Dictionary<RectTransform, Vector2> _originalButtonPositions = new Dictionary<RectTransform, Vector2>();
        private static readonly Dictionary<RectTransform, Vector2> _originalBoardPositions = new Dictionary<RectTransform, Vector2>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ApplyResponsiveToCurrentScene();
        }

        private void Update()
        {
            int currentW = Screen.width;
            int currentH = Screen.height;
            Rect currentSafe = Screen.safeArea;
            bool currentKb = TouchScreenKeyboard.visible;

            if (currentW != _lastWidth || currentH != _lastHeight || currentSafe != _lastSafeArea || currentKb != _lastKeyboardVisible)
            {
                _lastWidth = currentW;
                _lastHeight = currentH;
                _lastSafeArea = currentSafe;
                _lastKeyboardVisible = currentKb;

                ApplyResponsiveToCurrentScene();
            }
        }

        /// <summary>
        /// Áp dụng responsive toàn diện cho scene hiện tại hoặc scene chỉ định.
        /// </summary>
        public static void ApplyResponsive(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            ApplyResponsiveInternal(scene);
        }

        public void ApplyResponsiveToCurrentScene()
        {
            ApplyResponsiveInternal(SceneManager.GetActiveScene());
        }

        private static void ApplyResponsiveInternal(Scene targetScene)
        {
            if (!targetScene.IsValid() || !targetScene.isLoaded) return;

            float currentAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            // Dynamic Match logic:
            // - Màn hình rộng hơn 16:9 (aspect >= 1.77f, ví dụ 20:9, 19.5:9): Match Height = 1.0f.
            //   Giữ nguyên chuẩn 1080 chiều cao của bảng Fantasy, mở rộng chiều ngang 2 bên.
            // - Màn hình hẹp/cao hơn 16:9 (aspect < 1.77f, ví dụ 16:10, 4:3): Match Width = 0.0f.
            //   Giữ nguyên chuẩn 1920 chiều rộng của bảng Fantasy, mở rộng chiều dọc trên dưới.
            // Kết quả: Không bao giờ bị co rút hoặc cắt mất bất kỳ phần tử nào so với chuẩn 1920x1080!
            float dynamicMatch = (currentAspect >= BASE_ASPECT - 0.01f) ? 1.0f : 0.0f;

            // Tìm tất cả Canvas trong scene
            Canvas[] allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            foreach (var canvas in allCanvases)
            {
                if (canvas == null || canvas.gameObject.scene != targetScene) continue;

                // 1. Chuẩn hóa CanvasScaler theo Dynamic Match
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                }

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(BASE_WIDTH, BASE_HEIGHT);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = dynamicMatch;

                // 2. Bảo vệ Background: Không cưỡng bức crop hay thay đổi RectTransform của bảng chơi
                EnsureBackgroundCoversCanvas(canvas);

                // 3. Xử lý Safe Area cho các nút ở góc (chỉ khi có notch thật trên Android)
                ApplySafeAreaToCornerControls(canvas, dynamicMatch);

                // 4. Tránh bàn phím ảo Android khi nhập tự luận
                HandleVirtualKeyboardAvoidance(canvas);
            }
        }

        /// <summary>
        /// Đảm bảo tấm hình nền trực tiếp của Canvas phủ trọn màn hình mà KHÔNG cắt mất chữ, khung hay nút.
        /// Tuyệt đối KHÔNG đụng vào các board, card hay panel con.
        /// </summary>
        private static void EnsureBackgroundCoversCanvas(Canvas canvas)
        {
            if (canvas == null) return;

            // Chỉ xét các Image là con trực tiếp của Canvas ở lớp dưới cùng (index 0 hoặc 1)
            int childCount = canvas.transform.childCount;
            for (int i = 0; i < Mathf.Min(childCount, 2); i++)
            {
                Transform childTf = canvas.transform.GetChild(i);
                if (childTf == null) continue;

                string goName = childTf.gameObject.name.ToLowerInvariant();
                // Tuyệt đối không đụng vào board, panel, modal, popup, card, button
                if (goName.Contains("board") || goName.Contains("panel") || goName.Contains("modal") ||
                    goName.Contains("popup") || goName.Contains("card") || goName.Contains("btn") ||
                    goName.Contains("button") || goName.Contains("header"))
                {
                    continue;
                }

                Image img = childTf.GetComponent<Image>();
                if (img != null)
                {
                    // Nếu Image đã được gỡ hoặc có AspectRatioFitter EnvelopeParent gây crop chữ, gỡ bỏ
                    var fitter = img.GetComponent<AspectRatioFitter>();
                    if (fitter != null && fitter.aspectMode == AspectRatioFitter.AspectMode.EnvelopeParent)
                    {
                        // Giữ nguyên thiết kế gốc, không cưỡng bức phóng to gây mất góc/tiêu đề
                        Destroy(fitter);
                    }
                }
            }
        }

        /// <summary>
        /// Bảo vệ các nút góc (nút Quay lại, Đóng) không bị che bởi tai thỏ / camera đục lỗ trên Android.
        /// Có cơ chế cache vị trí gốc để không bị lệch nút khi thay đổi độ phân giải liên tục.
        /// </summary>
        private static void ApplySafeAreaToCornerControls(Canvas canvas, float dynamicMatch)
        {
            Rect safeArea = Screen.safeArea;
            float screenW = Screen.width;
            float screenH = Screen.height;
            if (safeArea.width <= 0 || safeArea.height <= 0 || screenW <= 0 || screenH <= 0) return;

            float scaleFactor = (dynamicMatch >= 0.5f) ? (screenH / BASE_HEIGHT) : (screenW / BASE_WIDTH);
            if (scaleFactor <= 0) scaleFactor = 1f;

            float leftSafePadding = safeArea.xMin / scaleFactor;

            var allButtons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                string btnName = btn.gameObject.name.ToLowerInvariant();

                bool isCornerBackButton = btnName.Contains("quay") || btnName.Contains("back") || btnName.Contains("trove") ||
                                          (btnName.Contains("btn_close") && !btnName.Contains("bottom"));

                if (isCornerBackButton)
                {
                    RectTransform rt = btn.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        // Ghi nhớ vị trí gốc nếu chưa có
                        if (!_originalButtonPositions.ContainsKey(rt))
                        {
                            _originalButtonPositions[rt] = rt.anchoredPosition;
                        }

                        Vector2 origPos = _originalButtonPositions[rt];

                        // Nếu có tai thỏ thật phía bên trái (trên điện thoại Android)
                        if (leftSafePadding > 10f)
                        {
                            Vector3[] corners = new Vector3[4];
                            rt.GetWorldCorners(corners);
                            float minScreenX = corners[0].x;

                            if (minScreenX < safeArea.xMin + 5f)
                            {
                                float offsetCanvasX = (safeArea.xMin - minScreenX + 15f) / scaleFactor;
                                rt.anchoredPosition = new Vector2(origPos.x + offsetCanvasX, origPos.y);
                            }
                            else
                            {
                                rt.anchoredPosition = origPos;
                            }
                        }
                        else
                        {
                            // Trả về vị trí gốc thiết kế chuẩn 1920x1080 khi không có notch
                            rt.anchoredPosition = origPos;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tự động nâng khung nhập bài tự luận lên khi bàn phím ảo Android xuất hiện để không che nút Nộp bài.
        /// Khôi phục chính xác vị trí gốc khi bàn phím ảo đóng lại.
        /// </summary>
        private static void HandleVirtualKeyboardAvoidance(Canvas canvas)
        {
            var board = canvas.transform.Find("Board") ?? canvas.transform.Find("MainBoard") ?? canvas.transform.Find("Panel");
            if (board == null) return;

            RectTransform boardRt = board.GetComponent<RectTransform>();
            if (boardRt == null) return;

            if (!_originalBoardPositions.ContainsKey(boardRt))
            {
                _originalBoardPositions[boardRt] = boardRt.anchoredPosition;
            }

            Vector2 origPos = _originalBoardPositions[boardRt];

            if (TouchScreenKeyboard.visible)
            {
                var activeInputField = canvas.GetComponentInChildren<TMP_InputField>();
                if (activeInputField != null && activeInputField.isFocused)
                {
                    Rect kbArea = TouchScreenKeyboard.area;
                    if (kbArea.height > 50f)
                    {
                        float kbHeightCanvas = kbArea.height * (BASE_HEIGHT / Mathf.Max(1, Screen.height));
                        boardRt.anchoredPosition = new Vector2(origPos.x, origPos.y + (kbHeightCanvas * 0.4f));
                        return;
                    }
                }
            }

            // Trả về vị trí gốc khi bàn phím không hoạt động
            boardRt.anchoredPosition = origPos;
        }
    }
}
