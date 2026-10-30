using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace ScienceQuest.PhysicsGame1
{
    public static class UIHelper
    {
        public static Button GetOrAddButton(GameObject go, UnityEngine.Events.UnityAction action)
        {
            if (go == null) return null;
            Button btn = go.GetComponent<Button>();
            if (btn == null)
            {
                btn = go.AddComponent<Button>();
            }

            // Đảm bảo Image có raycastTarget
            Image img = go.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;

            btn.onClick.RemoveAllListeners();
            if (action != null) btn.onClick.AddListener(action);
            return btn;
        }

        public static void SetTMPText(GameObject go, string text)
        {
            if (go == null) return;
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = text;
        }

        /// <summary>
        /// Lọc bỏ các tiền tố đáp án thừa như "A. ", "B. ", "C: ", "D: " để không bị hiển thị đè lên huy hiệu tròn chữ cái có sẵn trên nút.
        /// </summary>
        public static string CleanAnswerText(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            string s = raw.Trim();
            if (s.Length >= 2)
            {
                char c0 = char.ToUpper(s[0]);
                if (c0 >= 'A' && c0 <= 'D')
                {
                    if (s[1] == '.' || s[1] == ':')
                    {
                        return s.Substring(2).TrimStart();
                    }
                    if (s.Length >= 3 && s[1] == ' ' && (s[2] == '.' || s[2] == ':'))
                    {
                        return s.Substring(3).TrimStart();
                    }
                }
            }
            return s;
        }

        /// <summary>
        /// Căn chỉnh RectTransform và TextMeshProUGUI vừa vặn bên trong khung cha với lề padding, tự động xuống dòng và co giãn chữ.
        /// Giải quyết triệt để lỗi chữ bị lệch hoặc tràn ra ngoài ô đề bài, đáp án, lời giải, lý thuyết, mẹo...
        /// Nâng cấp thẩm mỹ typography font chữ: giãn dòng thoáng mắt, màu sắc tự nhiên, đường viền nét chữ rõ ràng.
        /// </summary>
        public static void FitTextInContainer(
            TextMeshProUGUI tmp, 
            float paddingLeft = 30f, 
            float paddingRight = 30f, 
            float paddingTop = 20f, 
            float paddingBottom = 20f,
            float minFontSize = 10f,
            float maxFontSize = 30f,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            Color? textColor = null,
            bool makeBold = false)
        {
            if (tmp == null) return;
            RectTransform rt = tmp.rectTransform;
            if (rt == null) return;

            // Neo co dãn phủ toàn bộ diện tích khung cha
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(paddingLeft, paddingBottom);
            rt.offsetMax = new Vector2(-paddingRight, -paddingTop);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            // Đảm bảo không bị margin phụ bên trong TMP đệm thêm
            tmp.margin = Vector4.zero;

            // Cấu hình TextMeshPro
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minFontSize;
            tmp.fontSizeMax = maxFontSize;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            // Thẩm mỹ typography: giãn dòng cho tiếng Việt, độ dày nét và màu chữ
            tmp.lineSpacing = -2f;
            tmp.paragraphSpacing = 0f;
            tmp.characterSpacing = 0f;

            if (makeBold)
            {
                tmp.fontStyle |= FontStyles.Bold;
            }

            if (textColor.HasValue)
            {
                tmp.color = textColor.Value;
            }
            else
            {
                // Màu chữ nâu gỗ sẫm tự nhiên (Espresso Dark Brown), tăng độ tương phản và thẩm mỹ trên nền gỗ/giấy
                tmp.color = new Color(0.15f, 0.10f, 0.05f, 1f);
            }
        }
    }

    #region ===== 01_MainMenu Controller =====
    public class PhysicsGame1_MainMenuController : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🌟 Khởi tạo 01_MainMenu");

            // Nút Bắt đầu: Vào học với nhân vật mặc định (nv1 nếu chưa chọn) -> 04_ChapterSelection
            GameObject startObj = GameObject.Find("start");
            UIHelper.GetOrAddButton(startObj, () =>
            {
                if (string.IsNullOrEmpty(PhysicsGame1Session.Instance.SelectedCharacterId))
                    PhysicsGame1Session.Instance.SelectedCharacterId = "nv1";

                PhysicsGame1Session.Instance.PreviousSceneName = "01_MainMenu";
                SceneManager.LoadScene("04_ChapterSelection");
            });

            // Nút Chọn nhân vật -> 02_CharacterSelection
            GameObject charObj = GameObject.Find("chonnhanvat");
            UIHelper.GetOrAddButton(charObj, () =>
            {
                PhysicsGame1Session.Instance.PreviousSceneName = "01_MainMenu";
                SceneManager.LoadScene("02_CharacterSelection");
            });

            // Nút Thành tích -> 11_Achievements
            GameObject achObj = GameObject.Find("thanhtich");
            UIHelper.GetOrAddButton(achObj, () =>
            {
                PhysicsGame1Session.Instance.PreviousSceneName = "01_MainMenu";
                SceneManager.LoadScene("11_Achievements");
            });

            // Nút Cài đặt -> 10_Settings
            GameObject setObj = GameObject.Find("caidat");
            UIHelper.GetOrAddButton(setObj, () =>
            {
                PhysicsGame1Session.Instance.PreviousSceneName = "01_MainMenu";
                SceneManager.LoadScene("10_Settings");
            });

            // Nút Thoát/Quay lại: Trở về màn chọn game (SampleScene hoặc Scene2_MainMenu)
            GameObject exitObj = GameObject.Find("thoat");
            UIHelper.GetOrAddButton(exitObj, () =>
            {
                Debug.Log("[PhysicsGame1] 🚪 Quay về màn chọn Game 1 / Game 2");
                if (Application.CanStreamedLevelBeLoaded("GameSelection"))
                    SceneManager.LoadScene("GameSelection");
                else if (Application.CanStreamedLevelBeLoaded("Scene2_MainMenu"))
                    SceneManager.LoadScene("Scene2_MainMenu");
                else if (Application.CanStreamedLevelBeLoaded("SampleScene"))
                    SceneManager.LoadScene("SampleScene");
                else
                    SceneManager.LoadScene(0);
            });

            // Nút Quản Trị Giáo Viên -> 16_DashboardTeacher
            CreateTeacherAccessButton();
        }

        private void CreateTeacherAccessButton()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject btnGo = new GameObject("BtnTeacherDashboard");
            btnGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-25f, -25f);
            rt.sizeDelta = new Vector2(250f, 52f);

            Image img = btnGo.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.30f, 0.95f);

            Outline outline = btnGo.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.35f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "👨‍🏫 QUẢN TRỊ VIÊN";
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.95f, 0.75f);
            tmp.alignment = TextAlignmentOptions.Center;

            UIHelper.GetOrAddButton(btnGo, () =>
            {
                Debug.Log("[MainMenu] 👨‍🏫 Chuyển tới Màn hình Quản trị Giáo viên (16_DashboardTeacher)");
                PhysicsGame1Session.Instance.PreviousSceneName = "01_MainMenu";
                if (Application.CanStreamedLevelBeLoaded("16_DashboardTeacher"))
                {
                    SceneManager.LoadScene("16_DashboardTeacher");
                }
                else
                {
                    Debug.LogWarning("[MainMenu] Scene 16_DashboardTeacher chưa có trong Build Settings!");
                }
            });
        }
    }
    #endregion

    #region ===== 02_CharacterSelection Controller =====
    public class PhysicsGame1_CharacterSelectController : MonoBehaviour
    {
        private int _selectedCharIndex = 0; // 0=nv1, 1=nv2, 2=nv3, 3=nv4
        private GameObject[] _characters = new GameObject[4];
        private GameObject _duocChonObj;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🎭 Khởi tạo 02_CharacterSelection");

            _duocChonObj = GameObject.Find("duocchon");
            _characters[0] = GameObject.Find("nv1");
            _characters[1] = GameObject.Find("nv2");
            _characters[2] = GameObject.Find("nv3");
            _characters[3] = GameObject.Find("nv4");

            // Đọc nhân vật đang chọn từ Session
            string curId = PhysicsGame1Session.Instance.SelectedCharacterId;
            if (curId == "nv2") _selectedCharIndex = 1;
            else if (curId == "nv3") _selectedCharIndex = 2;
            else if (curId == "nv4") _selectedCharIndex = 3;
            else _selectedCharIndex = 0;

            // Click trực tiếp vào từng nhân vật để chọn
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                if (_characters[i] != null)
                {
                    UIHelper.GetOrAddButton(_characters[i], () =>
                    {
                        _selectedCharIndex = idx;
                        UpdateCharacterVisuals();
                    });
                }
            }

            // Mũi tên trái: cuộn sang trái
            GameObject btnTrai = GameObject.Find("trai");
            UIHelper.GetOrAddButton(btnTrai, () =>
            {
                _selectedCharIndex = (_selectedCharIndex - 1 + 4) % 4;
                UpdateCharacterVisuals();
            });

            // Mũi tên phải: cuộn sang phải
            GameObject btnPhai = GameObject.Find("phai");
            UIHelper.GetOrAddButton(btnPhai, () =>
            {
                _selectedCharIndex = (_selectedCharIndex + 1) % 4;
                UpdateCharacterVisuals();
            });

            // Nút Chọn: Xác nhận nhân vật đang chọn và chuyển sang 04_ChapterSelection
            GameObject btnChon = GameObject.Find("chon");
            UIHelper.GetOrAddButton(btnChon, () =>
            {
                string id = "nv" + (_selectedCharIndex + 1);
                PhysicsGame1Session.Instance.SelectedCharacterId = id;
                PlayerPrefs.SetString("PhysicsGame1_SelectedCharacter", id);
                PlayerPrefs.Save();
                Debug.Log($"[PhysicsGame1] ✅ Đã chọn nhân vật: {id}");

                PhysicsGame1Session.Instance.PreviousSceneName = "02_CharacterSelection";
                SceneManager.LoadScene("04_ChapterSelection");
            });

            // Nút Quay lại: Về 01_MainMenu
            GameObject btnQuayLai = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(btnQuayLai, () =>
            {
                SceneManager.LoadScene("01_MainMenu");
            });

            UpdateCharacterVisuals();
        }

        private void UpdateCharacterVisuals()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_characters[i] != null)
                {
                    Image img = _characters[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = (i == _selectedCharIndex) ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.85f);
                    }
                    _characters[i].transform.localScale = (i == _selectedCharIndex) ? Vector3.one * 1.12f : Vector3.one * 0.95f;
                }
            }

            // Di chuyển khung duocchon tới nhân vật được chọn
            if (_duocChonObj != null && _characters[_selectedCharIndex] != null)
            {
                _duocChonObj.SetActive(true);
                _duocChonObj.transform.position = _characters[_selectedCharIndex].transform.position;
            }
        }
    }
    #endregion

    #region ===== 04_ChapterSelection Controller =====
    public class PhysicsGame1_ChapterSelectController : MonoBehaviour
    {
        private int _selectedChapter = 1;
        private GameObject[] _chapterBtns = new GameObject[5];

        private readonly string[] ChapterNames = new string[]
        {
            "Chương I: Năng lượng cơ học",
            "Chương II: Ánh sáng",
            "Chương III: Điện học",
            "Chương IV: Điện từ",
            "Chương V: Năng lượng với cuộc sống"
        };

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 📖 Khởi tạo 04_ChapterSelection");
            _selectedChapter = PhysicsGame1Session.Instance.SelectedChapterIndex;

            // Kiểm tra chương mặc định có bị giáo viên ẩn không; nếu bị ẩn thì chọn chương active đầu tiên
            if (!ScienceQuest.Teacher.TeacherDataManager.Instance.IsChapterActiveByIndex("Physics", _selectedChapter))
            {
                for (int c = 1; c <= 5; c++)
                {
                    if (ScienceQuest.Teacher.TeacherDataManager.Instance.IsChapterActiveByIndex("Physics", c))
                    {
                        _selectedChapter = c;
                        PhysicsGame1Session.Instance.SelectedChapterIndex = c;
                        if (c - 1 < ChapterNames.Length)
                            PhysicsGame1Session.Instance.SelectedChapterName = ChapterNames[c - 1];
                        break;
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                int chapNum = i + 1;
                GameObject btnObj = GameObject.Find("Btn_Chuong" + chapNum);
                _chapterBtns[i] = btnObj;

                if (btnObj != null)
                {
                    // Kiểm tra chương có bị giáo viên ẩn trong Dashboard không
                    bool isChapActive = ScienceQuest.Teacher.TeacherDataManager.Instance.IsChapterActiveByIndex("Physics", chapNum);
                    if (!isChapActive)
                    {
                        btnObj.SetActive(false);
                        continue;
                    }

                    UIHelper.GetOrAddButton(btnObj, () =>
                    {
                        _selectedChapter = chapNum;
                        PhysicsGame1Session.Instance.SelectedChapterIndex = chapNum;
                        PhysicsGame1Session.Instance.SelectedChapterName = ChapterNames[chapNum - 1];
                        HighlightChapter();
                    });
                }
            }

            // Nút Chọn: Xác nhận chương và chuyển sang 05_LessonSelection
            GameObject btnChon = GameObject.Find("chon");
            UIHelper.GetOrAddButton(btnChon, () =>
            {
                if (!ScienceQuest.Teacher.TeacherDataManager.Instance.IsChapterActiveByIndex("Physics", _selectedChapter))
                {
                    Debug.LogWarning("[PhysicsGame1] Chương này đang bị giáo viên tạm ẩn!");
                    return;
                }

                PhysicsGame1Session.Instance.SelectedChapterIndex = _selectedChapter;
                PhysicsGame1Session.Instance.SelectedChapterName = ChapterNames[_selectedChapter - 1];
                Debug.Log($"[PhysicsGame1] ✅ Đã chọn {ChapterNames[_selectedChapter - 1]}");

                SceneManager.LoadScene("05_LessonSelection");
            });

            // Nút Quay lại: Trở về màn trước theo nguồn điều hướng (01_MainMenu hoặc 02_CharacterSelection)
            GameObject btnQuayLai = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(btnQuayLai, () =>
            {
                string prev = PhysicsGame1Session.Instance.PreviousSceneName;
                if (string.IsNullOrEmpty(prev) || prev == "04_ChapterSelection") prev = "01_MainMenu";
                SceneManager.LoadScene(prev);
            });

            HighlightChapter();
        }

        private void HighlightChapter()
        {
            for (int i = 0; i < 5; i++)
            {
                if (_chapterBtns[i] != null)
                {
                    bool isSel = (i + 1 == _selectedChapter);
                    _chapterBtns[i].transform.localScale = isSel ? Vector3.one * 1.08f : Vector3.one;
                    Image img = _chapterBtns[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = isSel ? new Color(1f, 1f, 0.85f, 1f) : Color.white;
                    }
                }
            }
        }
    }
    #endregion

    #region ===== 05_LessonSelection Controller =====
    public class PhysicsGame1_LessonSelectController : MonoBehaviour
    {
        private int _selectedLesson = 2;
        private GameObject _btnBai2;
        private GameObject _btnBai3;
        private GameObject _btnBai4;
        private GameObject _btnOntap;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 📚 Khởi tạo 05_LessonSelection");

            int chapIdx = PhysicsGame1Session.Instance.SelectedChapterIndex;
            if (chapIdx < 1 || chapIdx > 5) chapIdx = 1;

            // 1. Cập nhật Banner Tiêu đề: Bật đúng Image_tittle tương ứng với Chương đã chọn (1..5), ẩn các title khác
            UpdateChapterTitle(chapIdx);

            _btnBai2 = GameObject.Find("btn_bai2");
            _btnBai3 = GameObject.Find("btn_bai3");
            _btnBai4 = GameObject.Find("btn_bai4");
            _btnOntap = GameObject.Find("btn_ontapchuong");

            // 2. Cập nhật tên các bài học trên bảng gỗ theo chương
            UpdateLessonTitles(chapIdx);

            UIHelper.GetOrAddButton(_btnBai2, () => { _selectedLesson = 2; HighlightLesson(); });
            UIHelper.GetOrAddButton(_btnBai3, () => { _selectedLesson = 3; HighlightLesson(); });
            UIHelper.GetOrAddButton(_btnBai4, () => { _selectedLesson = 4; HighlightLesson(); });
            UIHelper.GetOrAddButton(_btnOntap, () => { _selectedLesson = 5; HighlightLesson(); });

            // Nút Chọn: Mở màn chọn chế độ học tập (06_GameModeSelect)
            GameObject btnChon = GameObject.Find("chon");
            UIHelper.GetOrAddButton(btnChon, () =>
            {
                PhysicsGame1Session.Instance.SelectedLessonIndex = _selectedLesson;
                PhysicsGame1Session.Instance.SelectedLessonName = GetLessonName(chapIdx, _selectedLesson);
                Debug.Log($"[PhysicsGame1] ✅ Đã chọn bài: {PhysicsGame1Session.Instance.SelectedLessonName}");

                SceneManager.LoadScene("06_GameModeSelect");
            });

            // Nút Quay lại: Về 04_ChapterSelection và giữ nguyên chương đã chọn
            GameObject btnQuayLai = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(btnQuayLai, () =>
            {
                SceneManager.LoadScene("04_ChapterSelection");
            });

            HighlightLesson();
        }

        /// <summary>
        /// Bật duy nhất banner tiêu đề (Image_tittleX) của chương được chọn, ẩn các chương khác.
        /// Tìm cả các GameObject đang inactive trong Canvas.
        /// </summary>
        private void UpdateChapterTitle(int chapIdx)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            for (int i = 1; i <= 5; i++)
            {
                string name1 = "Image_tittle" + i;
                string name2 = "Image_title" + i;
                foreach (var t in allChildren)
                {
                    if (t.name.Equals(name1, StringComparison.OrdinalIgnoreCase) ||
                        t.name.Equals(name2, StringComparison.OrdinalIgnoreCase))
                    {
                        bool shouldActive = (i == chapIdx);
                        t.gameObject.SetActive(shouldActive);
                        Debug.Log($"[PhysicsGame1] Banner {t.name} -> Active: {shouldActive}");
                    }
                }
            }
        }

        private string GetLessonName(int chapIdx, int lessonIdx)
        {
            switch (chapIdx)
            {
                case 1:
                    if (lessonIdx == 2) return "Bài 2: Động năng. Thế năng";
                    if (lessonIdx == 3) return "Bài 3: Cơ năng";
                    if (lessonIdx == 4) return "Bài 4: Công và công suất";
                    return "ÔN TẬP CHƯƠNG I";
                case 2:
                    if (lessonIdx == 2) return "Bài 5: Khúc xạ ánh sáng";
                    if (lessonIdx == 3) return "Bài 6: Phản xạ toàn phần";
                    if (lessonIdx == 4) return "Bài 7: Thấu kính và sự tạo ảnh";
                    return "ÔN TẬP CHƯƠNG II";
                case 3:
                    if (lessonIdx == 2) return "Bài 8: Định luật Ohm";
                    if (lessonIdx == 3) return "Bài 9: Đoạn mạch nối tiếp & song song";
                    if (lessonIdx == 4) return "Bài 10: Công và công suất điện";
                    return "ÔN TẬP CHƯƠNG III";
                case 4:
                    if (lessonIdx == 2) return "Bài 11: Từ trường và từ phổ";
                    if (lessonIdx == 3) return "Bài 12: Tác dụng từ của dòng điện";
                    if (lessonIdx == 4) return "Bài 13: Hiện tượng cảm ứng điện từ";
                    return "ÔN TẬP CHƯƠNG IV";
                case 5:
                default:
                    if (lessonIdx == 2) return "Bài 14: Năng lượng tái tạo";
                    if (lessonIdx == 3) return "Bài 15: Sử dụng năng lượng tiết kiệm";
                    if (lessonIdx == 4) return "Bài 16: Năng lượng với cuộc sống";
                    return "ÔN TẬP CHƯƠNG V";
            }
        }

        private void UpdateLessonTitles(int chapIdx)
        {
            SetButtonText(_btnBai2, GetLessonName(chapIdx, 2));
            SetButtonText(_btnBai3, GetLessonName(chapIdx, 3));
            SetButtonText(_btnBai4, GetLessonName(chapIdx, 4));
            SetButtonText(_btnOntap, GetLessonName(chapIdx, 5));
        }

        private void SetButtonText(GameObject btnObj, string text)
        {
            if (btnObj == null) return;
            TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = text;
                tmp.raycastTarget = false;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 14f;
                tmp.fontSizeMax = 22f;
                tmp.lineSpacing = 12f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(0.18f, 0.12f, 0.06f);
            }
        }

        private void HighlightLesson()
        {
            GameObject[] btns = new GameObject[] { _btnBai2, _btnBai3, _btnBai4, _btnOntap };
            int[] indices = new int[] { 2, 3, 4, 5 };

            for (int i = 0; i < btns.Length; i++)
            {
                if (btns[i] != null)
                {
                    bool isSel = (indices[i] == _selectedLesson);
                    btns[i].transform.localScale = isSel ? Vector3.one * 1.05f : Vector3.one;
                    Image img = btns[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = isSel ? new Color(1f, 1f, 0.8f, 1f) : Color.white;
                    }
                }
            }
        }
    }
    #endregion

    #region ===== 06_GameModeSelect Controller =====
    public class PhysicsGame1_GameModeController : MonoBehaviour
    {
        private string _selectedMode = "TracNghiem";
        private GameObject _btnTracno;
        private GameObject _btnTuluan;
        private GameObject _btnLythuyet;
        private GameObject _btnDoikhang;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🎮 Khởi tạo 06_GameModeSelect");

            _btnTracno = GameObject.Find("tracno");
            _btnTuluan = GameObject.Find("tuluan");
            _btnLythuyet = GameObject.Find("lythuyet");
            _btnDoikhang = GameObject.Find("doikhang");

            // Đồng bộ chuẩn hóa kích thước 4 nút chế độ học tập cân đối, đẹp mắt
            StandardizeModeButton(_btnTracno);
            StandardizeModeButton(_btnTuluan);
            StandardizeModeButton(_btnLythuyet);
            StandardizeModeButton(_btnDoikhang);

            // Cả 4 nút đều có hành vi giống hệt nhau: Nhấn để chọn và highlight, chờ nhấn nút "CHỌN"
            UIHelper.GetOrAddButton(_btnTracno, () =>
            {
                _selectedMode = "TracNghiem";
                HighlightMode();
            });

            UIHelper.GetOrAddButton(_btnTuluan, () =>
            {
                _selectedMode = "TuLuan";
                HighlightMode();
            });

            UIHelper.GetOrAddButton(_btnLythuyet, () =>
            {
                _selectedMode = "LyThuyet";
                HighlightMode();
            });

            UIHelper.GetOrAddButton(_btnDoikhang, () =>
            {
                _selectedMode = "DoiKhang";
                HighlightMode();
            });

            // Nút Chọn: Xác nhận chế độ đã chọn và chuyển màn hình tương ứng
            GameObject btnChon = GameObject.Find("chon");
            UIHelper.GetOrAddButton(btnChon, () =>
            {
                PhysicsGame1Session.Instance.SelectedGameMode = _selectedMode;
                if (_selectedMode == "TracNghiem" || _selectedMode == "TuLuan")
                {
                    SceneManager.LoadScene("03_LevelSelection");
                }
                else if (_selectedMode == "LyThuyet")
                {
                    SceneManager.LoadScene("12_Theory");
                }
                else if (_selectedMode == "DoiKhang")
                {
                    SceneManager.LoadScene("13_VersusSelection");
                }
            });

            // Nút Quay lại: Về 05_LessonSelection
            GameObject btnBack = GameObject.Find("back") ?? GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(btnBack, () =>
            {
                SceneManager.LoadScene("05_LessonSelection");
            });

            HighlightMode();
        }

        private void StandardizeModeButton(GameObject go)
        {
            if (go == null) return;
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(325f, 280f);
            }
        }

        private void HighlightMode()
        {
            HighlightButton(_btnTracno, _selectedMode == "TracNghiem");
            HighlightButton(_btnTuluan, _selectedMode == "TuLuan");
            HighlightButton(_btnLythuyet, _selectedMode == "LyThuyet");
            HighlightButton(_btnDoikhang, _selectedMode == "DoiKhang");
        }

        private void HighlightButton(GameObject go, bool active)
        {
            if (go == null) return;
            go.transform.localScale = active ? Vector3.one * 1.08f : Vector3.one;
            Image img = go.GetComponent<Image>();
            if (img != null)
            {
                img.color = active ? new Color(1f, 1f, 0.82f, 1f) : Color.white;
            }
        }
    }
    #endregion

    #region ===== 03_LevelSelection Controller =====
    public class PhysicsGame1_LevelSelectController : MonoBehaviour
    {
        private string _selectedLevel = "NhanBiet";
        private GameObject _btnNhanBiet;
        private GameObject _btnThongHieu;
        private GameObject _btnVanDung;
        private GameObject _btnVanDungCao;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🎯 Khởi tạo 03_LevelSelection (Chọn mức độ)");

            _btnNhanBiet = FindLevelCard("nhanbiet");
            _btnThongHieu = FindLevelCard("thonghieu");
            _btnVanDung = FindLevelCard("vandung");
            _btnVanDungCao = FindLevelCard("vandungcao");

            SetupCardButton(_btnNhanBiet, "NhanBiet");
            SetupCardButton(_btnThongHieu, "ThongHieu");
            SetupCardButton(_btnVanDung, "VanDung");
            SetupCardButton(_btnVanDungCao, "VanDungCao");

            // Nút Chọn: Xác nhận mức độ -> chuẩn bị 10 câu hỏi -> nạp 07_DuckShooting
            GameObject btnChon = GameObject.Find("chon");
            UIHelper.GetOrAddButton(btnChon, () =>
            {
                PhysicsGame1Session.Instance.SelectedLevel = _selectedLevel;
                PhysicsGame1Session.Instance.StartNewGameSession();
                Debug.Log($"[PhysicsGame1] 🦆 Bắt đầu Bắn Vịt với mức độ: {_selectedLevel}");

                SceneManager.LoadScene("07_DuckShooting");
            });

            // Nút Quay lại: Trở về 06_GameModeSelect
            GameObject btnQuayLai = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(btnQuayLai, () =>
            {
                SceneManager.LoadScene("06_GameModeSelect");
            });

            HighlightLevel();
        }

        private GameObject FindLevelCard(string levelName)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform t = canvas.transform.Find(levelName);
                if (t != null) return t.gameObject;
            }

            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
            {
                if (go.name.Equals(levelName, StringComparison.OrdinalIgnoreCase) &&
                    go.transform.parent != null &&
                    go.transform.parent.GetComponent<Canvas>() != null)
                {
                    return go;
                }
            }

            return GameObject.Find(levelName);
        }

        private void SetupCardButton(GameObject cardObj, string levelKey)
        {
            if (cardObj == null) return;
            UIHelper.GetOrAddButton(cardObj, () => { _selectedLevel = levelKey; HighlightLevel(); });

            // Đảm bảo chữ bên trong thẻ không cản trở click và được tự động xuống dòng/co giãn vừa vặn
            TextMeshProUGUI txt = cardObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
            {
                txt.raycastTarget = false;
                txt.textWrappingMode = TextWrappingModes.Normal;
                txt.enableAutoSizing = true;
                txt.fontSizeMin = 14f;
                txt.fontSizeMax = 22f;
                txt.lineSpacing = 14f;
                txt.fontStyle = FontStyles.Bold;
                txt.color = new Color(0.18f, 0.12f, 0.06f);
            }
        }

        private void HighlightLevel()
        {
            HighlightBtn(_btnNhanBiet, _selectedLevel == "NhanBiet");
            HighlightBtn(_btnThongHieu, _selectedLevel == "ThongHieu");
            HighlightBtn(_btnVanDung, _selectedLevel == "VanDung");
            HighlightBtn(_btnVanDungCao, _selectedLevel == "VanDungCao");
        }

        private void HighlightBtn(GameObject go, bool active)
        {
            if (go == null) return;
            go.transform.localScale = active ? Vector3.one * 1.08f : Vector3.one;
            Image img = go.GetComponent<Image>();
            if (img != null)
            {
                img.color = active ? new Color(1f, 1f, 0.75f, 1f) : Color.white;
            }
        }
    }
    #endregion
}
