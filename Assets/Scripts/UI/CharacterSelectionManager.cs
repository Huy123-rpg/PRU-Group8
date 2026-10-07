using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ScienceQuest.Core;
using ScienceQuest.Quiz;
using System.Collections.Generic;

namespace ScienceQuest.UI
{
    /// <summary>
    /// Quản lý màn chọn nhân vật (CharacterSelection).
    /// 
    /// Các chức năng:
    ///   1. Carousel nhân vật: Tự động tìm cha1, cha2, cha3, cha4 và ẩn/hiện theo lượt.
    ///   2. Nút Mũi tên: Chuyển qua lại giữa các nhân vật (vòng tròn).
    ///   3. Nút CONFIRM: Lưu nhân vật đã chọn và chuyển sang màn Physics (Bắn Vịt).
    ///   4. Nút QUAY LẠI (Back): Quay về màn danh sách bài học (LessonList).
    /// </summary>
    public class CharacterSelectionManager : MonoBehaviour
    {
        public static int SelectedCharacterIndex = 0;
        public static string SelectedCharacter = "";

        private List<GameObject> characterObjects = new List<GameObject>();
        private int currentIndex = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "CharacterSelection")
            {
                if (FindFirstObjectByType<CharacterSelectionManager>() == null)
                {
                    GameObject go = new GameObject("CharacterSelectionManager_Auto");
                    go.AddComponent<CharacterSelectionManager>();
                    Debug.Log("[CharacterSelectionManager] ⚙️ Tự động khởi tạo quản lý chọn nhân vật");
                }
            }
        }

        private void Start()
        {
            currentIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

            FindCharacters();
            SetupButtons();
            ShowCharacter(currentIndex);
        }

        /// <summary>
        /// Tìm các GameObject tên dạng "cha" + số → sắp xếp theo thứ tự (cha1, cha2, cha3, cha4).
        /// </summary>
        private void FindCharacters()
        {
            characterObjects.Clear();

            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            List<GameObject> found = new List<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                string n = obj.name.ToLower();
                // Khớp "cha" theo sau là chữ số (cha1, cha2, cha3, cha4...)
                if (n.Length >= 4 && n.StartsWith("cha") && char.IsDigit(n[3]))
                    found.Add(obj);
            }

            found.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
            characterObjects = found;

            if (characterObjects.Count > 0)
                currentIndex = Mathf.Clamp(currentIndex, 0, characterObjects.Count - 1);

            Debug.Log($"[CharacterSelectionManager] ✅ Đã tìm thấy {characterObjects.Count} nhân vật: "
                      + string.Join(", ", characterObjects.ConvertAll(g => g.name)));
        }

        /// <summary>
        /// Hiển thị nhân vật tại index được chọn, ẩn các nhân vật còn lại.
        /// </summary>
        private void ShowCharacter(int index)
        {
            for (int i = 0; i < characterObjects.Count; i++)
            {
                characterObjects[i].SetActive(i == index);
            }

            SelectedCharacterIndex = index;
            SelectedCharacter = characterObjects.Count > 0 ? characterObjects[index].name : "";
            Debug.Log($"[CharacterSelectionManager] 👤 Đang chọn: [{index}] {SelectedCharacter}");
        }

        /// <summary>
        /// Gắn sự kiện thông minh cho tất cả các nút trong scene:
        ///   - Nút CONFIRM: Xác nhận chọn nhân vật → vào màn Physics.
        ///   - Nút QUAY LẠI (Back): Nhận diện theo tên/text/vị trí → về LessonList.
        ///   - Nút MŨI TÊN: Nhận diện icon mũi tên hoặc nút điều hướng → chuyển nhân vật.
        /// </summary>
        private void SetupButtons()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            Debug.Log($"[CharacterSelectionManager] 🔍 Đang gắn sự kiện cho {allButtons.Length} nút trong scene");

            foreach (Button btn in allButtons)
            {
                string btnName = btn.gameObject.name.ToLower();
                TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                string btnText = tmp != null ? tmp.text.Trim().ToLower() : "";
                Image btnImg = btn.GetComponent<Image>();
                string spriteName = (btnImg != null && btnImg.sprite != null) ? btnImg.sprite.name.ToLower() : "";

                // ── 1. NÚT CONFIRM (Xác nhận) ──────────────────────────────────
                if (btnName.Contains("confirm") || btnName.Contains("ok")
                    || btnText.Contains("confirm") || btnText.Contains("xác nhận")
                    || btnText.Contains("vào") || btnText.Contains("chọn"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(ConfirmSelection);
                    Debug.Log($"[CharacterSelectionManager] ✅ Gắn CONFIRM: {btn.gameObject.name}");
                    continue;
                }

                // ── 2. NÚT QUAY LẠI (BACK) ────────────────────────────────────
                // Nhận diện qua tên (back, quay, return, exit, trove)
                // hoặc nội dung text (quay lại, back, trở về, button mặc định vừa tạo)
                bool isBackBtn = btnName.Contains("back") || btnName.Contains("quay") || btnName.Contains("return")
                                 || btnName.Contains("exit") || btnName.Contains("trove") || btnName.Contains("tro_ve")
                                 || btnText.Contains("quay") || btnText.Contains("back") || btnText.Contains("trở")
                                 || btnText.Contains("<") || btnText.Contains("←")
                                 || spriteName.Contains("back") || spriteName.Contains("return")
                                 // Nếu người dùng vừa bấm thêm nút "Button" mặc định trong Unity
                                 || (btnName == "button" && (btnText == "button" || string.IsNullOrEmpty(btnText)));

                if (isBackBtn)
                {
                    // Tự động cập nhật chữ hiển thị thành "Quay Lại" nếu còn để chữ "Button" mặc định
                    if (tmp != null && (tmp.text.Trim() == "Button" || string.IsNullOrEmpty(tmp.text.Trim())))
                    {
                        tmp.text = "Quay Lại";
                    }

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnBackClicked);
                    Debug.Log($"[CharacterSelectionManager] 🔙 Gắn nút QUAY LẠI: {btn.gameObject.name} (Text='{btnText}')");
                    continue;
                }

                // ── 3. NÚT MŨI TÊN ĐIỀU HƯỚNG CAROUSEL ───────────────────────
                bool isLeftArrow = btnName.Contains("left") || btnName.Contains("prev") || btnName.Contains("_l")
                                   || spriteName.Contains("left") || spriteName.Contains("prev") || spriteName.Contains("_w");
                bool isRightArrow = btnName.Contains("right") || btnName.Contains("next") || btnName.Contains("_r") || btnName.Contains("_e")
                                    || btnName.Contains("arrow") || btnName == "t" || btnName.Contains("image (3)")
                                    || spriteName.Contains("arrow") || spriteName.Contains("right") || spriteName.Contains("_e");

                if (isLeftArrow)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => Navigate(-1));
                    Debug.Log($"[CharacterSelectionManager] ⬅️ Gắn MŨI TÊN TRÁI: {btn.gameObject.name}");
                    continue;
                }

                if (isRightArrow)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => Navigate(1));
                    Debug.Log($"[CharacterSelectionManager] ➡️ Gắn MŨI TÊN PHẢI: {btn.gameObject.name}");
                    continue;
                }

                // ── 4. CÁC NÚT KHÁC ───────────────────────────────────────────
                // Nút phụ còn lại mặc định chuyển tiếp nhân vật
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Navigate(1));
                Debug.Log($"[CharacterSelectionManager] ➡️ Gắn NAVIGATE mặc định: {btn.gameObject.name}");
            }
        }

        /// <summary>
        /// Chuyển nhân vật theo hướng dir (+1 = kế tiếp, -1 = trước đó).
        /// </summary>
        public void Navigate(int dir)
        {
            if (characterObjects.Count == 0) return;
            currentIndex = (currentIndex + dir + characterObjects.Count) % characterObjects.Count;
            ShowCharacter(currentIndex);
        }

        /// <summary>
        /// Xử lý khi bấm nút QUAY LẠI: Trở về màn LessonList.
        /// </summary>
        public void OnBackClicked()
        {
            Debug.Log("[CharacterSelectionManager] 🔙 Bấm nút Quay Lại → Chuyển về màn LessonList");
            SceneLoader.Instance.LoadScene("LessonList");
        }

        /// <summary>
        /// Xử lý khi bấm nút CONFIRM: Lưu lựa chọn và vào game Physics.
        /// </summary>
        public void ConfirmSelection()
        {
            PlayerPrefs.SetInt("SelectedCharacter", currentIndex);
            PlayerPrefs.SetString("SelectedCharacterName", SelectedCharacter);
            PlayerPrefs.Save();

            Debug.Log($"[CharacterSelectionManager] 🎭 Đã xác nhận chọn nhân vật [{currentIndex}] {SelectedCharacter} → Nạp Scene Physics");

            ScienceQuest.Quiz.QuizManager.SelectedChapter = PlayerPrefs.GetString("QuizChapter", "");
            ScienceQuest.Quiz.QuizManager.IsDuckShootingMode = true;
            SceneLoader.Instance.LoadScene("Physics");
        }
    }
}
