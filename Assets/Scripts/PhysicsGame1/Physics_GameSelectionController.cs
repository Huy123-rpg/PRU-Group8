using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace ScienceQuest.PhysicsGame1
{
    /// <summary>
    /// Controller cho màn hình Chọn Game (GameSelection.unity)
    /// Cho phép người chơi lựa chọn giữa:
    /// - Game 1: Bắn Vịt & Đấu Boss Vật Lý (Chế độ học tập, Lý thuyết, Đối kháng, Tự luận AI)
    /// - Game 2: Phiêu Lưu Khám Phá Kiến Thức Vật Lý
    /// </summary>
    public class Physics_GameSelectionController : MonoBehaviour
    {
        private int _selectedGame = 1; // 1 = Game 1, 2 = Game 2

        private GameObject _game1Obj;
        private GameObject _game2Obj;
        private GameObject _khungChonObj;
        private GameObject _btnChonObj;
        private GameObject _btnQuayLaiObj;

        private RectTransform _game1Rt;
        private RectTransform _game2Rt;
        private RectTransform _khungRt;

        private Image _game1Img;
        private Image _game2Img;

        // Tọa độ chuẩn của khung được chọn khi bao quanh từng game
        // Game 1 tại x = -472, Game 2 tại x = 482
        // Khung có offset khoảng (+9.8f, +42.5f)
        private readonly Vector2 POS_KHUNG_GAME1 = new Vector2(-462.2f, 42.5f);
        private readonly Vector2 POS_KHUNG_GAME2 = new Vector2(491.8f, 42.5f);

        private Coroutine _moveFrameCoroutine;
        private Coroutine _breathingCoroutine;

        private void Start()
        {
            Debug.Log("[GameSelection] 🎮 Khởi tạo màn hình Chọn Game Vật Lý (Game 1 / Game 2)");

            // 1. Tìm các GameObject UI trong Scene
            _game1Obj = GameObject.Find("game1");
            _game2Obj = GameObject.Find("game2");
            _khungChonObj = GameObject.Find("khungdcchon");
            _btnChonObj = GameObject.Find("chon");
            _btnQuayLaiObj = GameObject.Find("quaylai");

            if (_game1Obj != null)
            {
                _game1Rt = _game1Obj.GetComponent<RectTransform>();
                _game1Img = _game1Obj.GetComponent<Image>();
                UIHelper.GetOrAddButton(_game1Obj, () => SelectGame(1, true));
            }

            if (_game2Obj != null)
            {
                _game2Rt = _game2Obj.GetComponent<RectTransform>();
                _game2Img = _game2Obj.GetComponent<Image>();
                UIHelper.GetOrAddButton(_game2Obj, () => SelectGame(2, true));
            }

            // Đảm bảo khung chọn không cản trở tương tác chuột vào card
            if (_khungChonObj != null)
            {
                _khungRt = _khungChonObj.GetComponent<RectTransform>();
                Image frameImg = _khungChonObj.GetComponent<Image>();
                if (frameImg != null) frameImg.raycastTarget = false;

                // Bắt đầu nhịp thở lung linh nhẹ cho khung chọn
                _breathingCoroutine = StartCoroutine(FrameBreathingLoop());
            }

            // 2. Gán sự kiện cho Nút Chọn
            if (_btnChonObj != null)
            {
                RectTransform crt = _btnChonObj.GetComponent<RectTransform>();
                UIHelper.GetOrAddButton(_btnChonObj, () =>
                {
                    StartCoroutine(AnimateButtonPunch(crt, OnConfirmSelection));
                });
            }

            // 3. Gán sự kiện cho Nút Quay Lại
            if (_btnQuayLaiObj != null)
            {
                RectTransform qrt = _btnQuayLaiObj.GetComponent<RectTransform>();
                UIHelper.GetOrAddButton(_btnQuayLaiObj, () =>
                {
                    StartCoroutine(AnimateButtonPunch(qrt, OnBackClicked));
                });
            }

            // 4. Đọc lựa chọn đã lưu trước đó (mặc định Game 1)
            _selectedGame = PlayerPrefs.GetInt("Physics_SelectedGame", 1);
            if (_selectedGame < 1 || _selectedGame > 2) _selectedGame = 1;

            // Cập nhật trạng thái hiển thị ban đầu
            SelectGame(_selectedGame, false);
        }

        /// <summary>
        /// Chọn game (1 hoặc 2) và cập nhật hiệu ứng thị giác
        /// </summary>
        private void SelectGame(int gameIndex, bool playBounce = true)
        {
            _selectedGame = gameIndex;
            PlayerPrefs.SetInt("Physics_SelectedGame", _selectedGame);
            PlayerPrefs.Save();

            Debug.Log($"[GameSelection] 👉 Đã chọn Game {_selectedGame}");

            Vector2 targetPos = (_selectedGame == 1) ? POS_KHUNG_GAME1 : POS_KHUNG_GAME2;

            // Di chuyển mượt mà khung viền được chọn
            if (_khungRt != null)
            {
                if (_moveFrameCoroutine != null) StopCoroutine(_moveFrameCoroutine);
                _moveFrameCoroutine = StartCoroutine(AnimateMoveFrame(targetPos));
            }

            // Highlight thẻ game được chọn và làm mờ nhẹ thẻ còn lại
            UpdateCardVisuals(_game1Rt, _game1Img, _selectedGame == 1, playBounce);
            UpdateCardVisuals(_game2Rt, _game2Img, _selectedGame == 2, playBounce);
        }

        private void UpdateCardVisuals(RectTransform rt, Image img, bool isSelected, bool playBounce)
        {
            if (rt == null) return;

            if (isSelected)
            {
                rt.localScale = Vector3.one * 1.05f;
                if (img != null) img.color = new Color(1f, 1f, 0.95f, 1f); // Tông vàng sáng nổi bật

                if (playBounce)
                {
                    StartCoroutine(AnimateCardPunch(rt));
                }
            }
            else
            {
                rt.localScale = Vector3.one;
                if (img != null) img.color = new Color(0.78f, 0.78f, 0.82f, 0.92f); // Giảm sáng nhẹ để card được chọn tỏa sáng
            }
        }

        /// <summary>
        /// Xác nhận lựa chọn và chuyển sang scene tương ứng của Game 1 hoặc Game 2
        /// </summary>
        private void OnConfirmSelection()
        {
            Debug.Log($"[GameSelection] ✅ Xác nhận nạp Game {_selectedGame}");

            if (_selectedGame == 1)
            {
                // Game 1: Bắn Vịt & Đấu Boss Vật Lý
                PhysicsGame1Session.Instance.PreviousSceneName = "GameSelection";

                if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/Physics/Game1/01_MainMenu.unity"))
                {
                    SceneManager.LoadScene("Assets/Scenes/Physics/Game1/01_MainMenu.unity");
                }
                else if (Application.CanStreamedLevelBeLoaded("01_MainMenu"))
                {
                    SceneManager.LoadScene("01_MainMenu");
                }
                else
                {
                    Debug.LogWarning("[GameSelection] ⚠️ Không tìm thấy scene Game 1, đang nạp fallback");
                    SceneManager.LoadScene(0);
                }
            }
            else
            {
                // Game 2: Phiêu Lưu Kiến Thức Vật Lý
                if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/Physics/Game2/01_MainMenu.unity"))
                {
                    SceneManager.LoadScene("Assets/Scenes/Physics/Game2/01_MainMenu.unity");
                }
                else if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/Physics/Game2/02_CharacterSelection.unity"))
                {
                    SceneManager.LoadScene("Assets/Scenes/Physics/Game2/01_MainMenu.unity");
                }
                else if (Application.CanStreamedLevelBeLoaded("06_Gameplay_Mechanics"))
                {
                    SceneManager.LoadScene("06_Gameplay_Mechanics");
                }
                else
                {
                    ShowToast("Game 2 đang được hoàn thiện! Vui lòng trải nghiệm Game 1 (Bắn Vịt & Đấu Boss).");
                }
            }
        }

        /// <summary>
        /// Nút Quay lại màn chọn môn học (Scene2_MainMenu hoặc SampleScene)
        /// </summary>
        private void OnBackClicked()
        {
            Debug.Log("[GameSelection] 🚪 Quay về màn hình trước đó");

            if (Application.CanStreamedLevelBeLoaded("Scene2_MainMenu"))
            {
                SceneManager.LoadScene("Scene2_MainMenu");
            }
            else if (Application.CanStreamedLevelBeLoaded("SampleScene"))
            {
                SceneManager.LoadScene("SampleScene");
            }
            else if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        #region ===== HIỆU ỨNG THỊ GIÁC (ANIMATIONS) =====
        private IEnumerator AnimateMoveFrame(Vector2 targetPos)
        {
            if (_khungRt == null) yield break;

            Vector2 startPos = _khungRt.anchoredPosition;
            float duration = 0.28f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Dùng hàm SmoothStep đàn hồi nhẹ
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                _khungRt.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);

                yield return null;
            }

            _khungRt.anchoredPosition = targetPos;
        }

        private IEnumerator AnimateCardPunch(RectTransform rt)
        {
            if (rt == null) yield break;

            float duration = 0.22f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = 1.05f + Mathf.Sin(t * Mathf.PI) * 0.08f;
                rt.localScale = Vector3.one * scale;
                yield return null;
            }

            rt.localScale = Vector3.one * 1.05f;
        }

        private IEnumerator AnimateButtonPunch(RectTransform rt, Action onComplete)
        {
            if (rt != null)
            {
                float duration = 0.15f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float scale = (t < 0.5f) ? Mathf.Lerp(1f, 0.92f, t / 0.5f) : Mathf.Lerp(0.92f, 1.04f, (t - 0.5f) / 0.5f);
                    rt.localScale = Vector3.one * scale;
                    yield return null;
                }

                rt.localScale = Vector3.one;
            }

            onComplete?.Invoke();
        }

        private IEnumerator FrameBreathingLoop()
        {
            while (_khungRt != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 2.5f) * 0.018f;
                _khungRt.localScale = new Vector3(pulse, pulse, 1f);
                yield return null;
            }
        }

        private void ShowToast(string message)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject toastObj = new GameObject("ToastGame2Notice");
            toastObj.transform.SetParent(canvas.transform, false);

            RectTransform trt = toastObj.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(0f, -120f);
            trt.sizeDelta = new Vector2(750f, 65f);

            Image bg = toastObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.10f, 0.08f, 0.95f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(toastObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20f, 10f);
            textRt.offsetMax = new Vector2(-20f, -10f);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.88f, 0.35f);

            StartCoroutine(AnimateToastFade(toastObj));
        }

        private IEnumerator AnimateToastFade(GameObject toast)
        {
            yield return new WaitForSeconds(2.5f);
            if (toast != null) Destroy(toast);
        }
        #endregion
    }
}
