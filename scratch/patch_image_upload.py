import os

with open('Assets/Scripts/PhysicsGame2/UI/WrittenAnswerController.cs', 'r', encoding='utf-8') as f:
    c = f.read()

# 1. Ensure using System.IO;
if 'using System.IO;' not in c:
    c = 'using System.IO;\n' + c

# 2. Add _activeTabSprite and _inactiveTabSprite fields
old_fields = '''        [Header("=== SIDEBAR QUESTION TABS (1-5) ===")]
        [SerializeField] private List<Button> sidebarQuestionButtons = new List<Button>();
        [SerializeField] private List<TextMeshProUGUI> sidebarQuestionLabels = new List<TextMeshProUGUI>();'''

new_fields = '''        [Header("=== SIDEBAR QUESTION TABS (1-5) ===")]
        [SerializeField] private List<Button> sidebarQuestionButtons = new List<Button>();
        [SerializeField] private List<TextMeshProUGUI> sidebarQuestionLabels = new List<TextMeshProUGUI>();
        private Sprite _activeTabSprite;
        private Sprite _inactiveTabSprite;'''

# 3. Update FindUIReferences sidebar binding
old_sidebar_bind = '''            // 3. Tìm 5 Tab câu hỏi bên sidebar trái: x ~ -671 đến -678, y xếp từ 189 xuống -189
            List<Image> sidebarImgs = new List<Image>();
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.x < -640f && pos.y < 350f && pos.y > -250f && img.rectTransform.sizeDelta.x > 180f && img.rectTransform.sizeDelta.x < 260f)
                {
                    sidebarImgs.Add(img);
                }
            }
            // Sắp xếp theo thứ tự Y từ trên xuống dưới (Câu 1 đến Câu 5)
            sidebarImgs = sidebarImgs.OrderByDescending(img => img.rectTransform.anchoredPosition.y).ToList();

            sidebarQuestionButtons.Clear();
            sidebarQuestionLabels.Clear();
            for (int i = 0; i < sidebarImgs.Count; i++)
            {
                var sImg = sidebarImgs[i];
                var sBtn = sImg.GetComponent<Button>() ?? sImg.gameObject.AddComponent<Button>();
                sImg.raycastTarget = true;
                sBtn.targetGraphic = sImg;
                UIHoverPulseEffect.Attach(sBtn.gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);

                int qIdx = i;
                sBtn.onClick.RemoveAllListeners();
                sBtn.onClick.AddListener(() => OnSidebarQuestionClicked(qIdx));

                var label = sImg.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null) label.raycastTarget = false;

                sidebarQuestionButtons.Add(sBtn);
                sidebarQuestionLabels.Add(label);
            }'''

new_sidebar_bind = '''            // 3. Tìm 5 Tab câu hỏi bên sidebar trái: x ~ -671 đến -678, y xếp từ 189 xuống -189
            List<Image> sidebarImgs = new List<Image>();
            foreach (var img in allImages)
            {
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.x < -640f && pos.y < 350f && pos.y > -250f && img.rectTransform.sizeDelta.x > 180f && img.rectTransform.sizeDelta.x < 260f)
                {
                    sidebarImgs.Add(img);
                }
            }
            // Sắp xếp theo thứ tự Y từ trên xuống dưới (Câu 1 đến Câu 5)
            sidebarImgs = sidebarImgs.OrderByDescending(img => img.rectTransform.anchoredPosition.y).ToList();

            // Ghi nhớ sprite active (xanh lá ban đầu ở câu 1) và inactive (xanh lam ở các câu 2..5)
            if (sidebarImgs.Count > 0 && sidebarImgs[0] != null)
            {
                _activeTabSprite = sidebarImgs[0].sprite;
            }
            if (sidebarImgs.Count > 1 && sidebarImgs[1] != null)
            {
                _inactiveTabSprite = sidebarImgs[1].sprite;
            }

            // Tìm 5 nhãn TextMeshProUGUI độc lập ở sidebar trái (x ~ -620, y từ 182 xuống -197)
            List<TextMeshProUGUI> sidebarLabels = new List<TextMeshProUGUI>();
            foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                Vector2 pos = t.rectTransform.anchoredPosition;
                if (pos.x < -580f && pos.x > -650f && pos.y < 250f && pos.y > -250f)
                {
                    sidebarLabels.Add(t);
                }
            }
            sidebarLabels = sidebarLabels.OrderByDescending(t => t.rectTransform.anchoredPosition.y).ToList();

            sidebarQuestionButtons.Clear();
            sidebarQuestionLabels.Clear();
            for (int i = 0; i < sidebarImgs.Count; i++)
            {
                var sImg = sidebarImgs[i];
                var sBtn = sImg.GetComponent<Button>() ?? sImg.gameObject.AddComponent<Button>();
                sImg.raycastTarget = true;
                sBtn.targetGraphic = sImg;
                UIHoverPulseEffect.Attach(sBtn.gameObject, hover: 1.05f, minPulse: 1.02f, maxPulse: 1.06f, outline: true);

                int qIdx = i;
                sBtn.onClick.RemoveAllListeners();
                sBtn.onClick.AddListener(() => OnSidebarQuestionClicked(qIdx));

                TextMeshProUGUI label = (i < sidebarLabels.Count) ? sidebarLabels[i] : sImg.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    label.raycastTarget = false;
                    label.text = $"Câu {i + 1}";
                }

                sidebarQuestionButtons.Add(sBtn);
                sidebarQuestionLabels.Add(label);
            }'''

# 4. Update DisplayQuestion sidebar highlight
old_display_sidebar = '''            // 4. Highlight Tab bên sidebar trái
            for (int i = 0; i < sidebarQuestionButtons.Count; i++)
            {
                if (sidebarQuestionButtons[i] != null)
                {
                    bool exists = (i < _essayQuestions.Count);
                    sidebarQuestionButtons[i].gameObject.SetActive(exists);
                    if (!exists) continue;

                    bool isSelected = (i == _currentIndex);
                    var img = sidebarQuestionButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = isSelected ? new Color(1f, 0.95f, 0.60f, 1f) : Color.white;
                    }
                    if (i < sidebarQuestionLabels.Count && sidebarQuestionLabels[i] != null)
                    {
                        sidebarQuestionLabels[i].color = isSelected ? new Color(0.1f, 0.15f, 0.3f, 1f) : Color.white;
                    }
                }
            }'''

new_display_sidebar = '''            // 4. Highlight Tab bên sidebar trái: di chuyển nút xanh lá theo đúng câu đang chọn
            for (int i = 0; i < sidebarQuestionButtons.Count; i++)
            {
                if (sidebarQuestionButtons[i] != null)
                {
                    bool exists = (i < _essayQuestions.Count);
                    sidebarQuestionButtons[i].gameObject.SetActive(exists);
                    if (!exists) continue;

                    bool isSelected = (i == _currentIndex);
                    var img = sidebarQuestionButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = Color.white;
                        if (_activeTabSprite != null && _inactiveTabSprite != null)
                        {
                            img.sprite = isSelected ? _activeTabSprite : _inactiveTabSprite;
                        }
                    }

                    if (i < sidebarQuestionLabels.Count && sidebarQuestionLabels[i] != null)
                    {
                        sidebarQuestionLabels[i].gameObject.SetActive(exists);
                        sidebarQuestionLabels[i].text = $"Câu {i + 1}";
                        sidebarQuestionLabels[i].color = isSelected ? Color.white : new Color(0.85f, 0.90f, 1f, 0.85f);
                    }
                }
            }'''

# 5. Update UpdateImagePreview to preserve aspect ratio and disable OCR when no image
old_preview = '''        private void UpdateImagePreview()
        {
            bool hasImage = _userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null;
            if (uploadedImagePreview != null)
            {
                if (hasImage)
                {
                    uploadedImagePreview.texture = _userImages[_currentIndex];
                    uploadedImagePreview.gameObject.SetActive(true);
                }
                else
                {
                    uploadedImagePreview.gameObject.SetActive(false);
                }
            }

            if (noImageNoticeText != null)
            {
                noImageNoticeText.gameObject.SetActive(!hasImage);
            }
        }'''

new_preview = '''        private void UpdateImagePreview()
        {
            bool hasImage = _userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null;
            if (uploadedImagePreview != null)
            {
                if (hasImage)
                {
                    Texture2D tex = _userImages[_currentIndex];
                    uploadedImagePreview.texture = tex;

                    // Giữ đúng tỷ lệ ảnh (Aspect Ratio), nằm gọn trong khung không bị méo và không tràn ra ngoài
                    float maxW = 260f;
                    float maxH = 170f;
                    float aspect = (tex != null && tex.width > 0 && tex.height > 0) ? (float)tex.width / tex.height : 1.33f;

                    float targetW = maxW;
                    float targetH = targetW / aspect;
                    if (targetH > maxH)
                    {
                        targetH = maxH;
                        targetW = targetH * aspect;
                    }

                    RectTransform pRt = uploadedImagePreview.rectTransform;
                    pRt.sizeDelta = new Vector2(targetW, targetH);
                    pRt.anchoredPosition = new Vector2(536.34f, -120f);

                    uploadedImagePreview.gameObject.SetActive(true);
                }
                else
                {
                    uploadedImagePreview.texture = null;
                    uploadedImagePreview.gameObject.SetActive(false);
                }
            }

            if (noImageNoticeText != null)
            {
                noImageNoticeText.gameObject.SetActive(!hasImage);
            }

            // Nút OCR: chỉ bật khi đã có ảnh thật hợp lệ
            if (btnOcr != null)
            {
                btnOcr.interactable = hasImage;
                var ocrImg = btnOcr.GetComponent<Image>();
                if (ocrImg != null)
                {
                    ocrImg.color = hasImage ? new Color(0.08f, 0.20f, 0.42f, 0.95f) : new Color(0.12f, 0.16f, 0.22f, 0.40f);
                }
            }
        }'''

# 6. Replace OnUploadClicked, CreateSampleHandwritingTexture, and OnOcrButtonClicked
old_upload_block = '''        /// <summary>
        /// Nút "TẢI ẢNH": Nạp ảnh bài giải viết tay và hiển thị ngay trên khung xem trước.
        /// Tự động chuẩn bị ảnh mẫu vật lý nếu chạy trên thiết bị chưa có camera hoặc file picker.
        /// </summary>
        private void OnUploadClicked()
        {
            Debug.Log($"[WrittenAnswerController] Tải ảnh bài làm cho Câu {_currentIndex + 1}.");

            // Tạo texture mô phỏng bài làm học sinh rõ nét để kiểm tra trực quan
            Texture2D sampleTexture = CreateSampleHandwritingTexture();
            _userImages[_currentIndex] = sampleTexture;
            UpdateImagePreview();

            ShowNotice("<color=#38d368>Đã tải ảnh bài làm lên thành công! Bấm 'ĐỌC CHỮ OCR' để trích xuất.</color>");
        }

        /// <summary>
        /// Nút "XÓA ẢNH": Xóa ảnh đã tải của câu hiện tại.
        /// </summary>
        private void OnDeleteImgClicked()
        {
            if (_userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null)
            {
                _userImages[_currentIndex] = null;
                UpdateImagePreview();
                ShowNotice("<color=#d97706>Đã xóa ảnh của câu này.</color>");
            }
            else
            {
                ShowNotice("Chưa có ảnh nào được tải lên ở câu này.");
            }
        }

        /// <summary>
        /// Tạo texture mẫu bài làm viết tay vật lý để test hiển thị và OCR ngay trong Unity.
        /// </summary>
        private Texture2D CreateSampleHandwritingTexture()
        {
            int w = 256;
            int h = 180;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color paperBg = new Color(0.96f, 0.95f, 0.92f, 1f);
            Color ink = new Color(0.08f, 0.12f, 0.28f, 1f);

            Color[] cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = paperBg;
            tex.SetPixels(cols);

            // Vẽ vài nét tượng trưng bài làm
            for (int x = 20; x < w - 20; x++)
            {
                for (int y = 40; y < h - 20; y += 30)
                {
                    tex.SetPixel(x, y, ink);
                    tex.SetPixel(x, y + 1, ink);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Nút "📷 ĐỌC CHỮ OCR": Trích xuất văn bản viết tay từ ảnh qua EssayGradingService.
        /// </summary>
        private void OnOcrButtonClicked()
        {
            Debug.Log($"[WrittenAnswerController] Chạy OCR cho Câu {_currentIndex + 1}.");
            ShowNotice("<color=#d97706>Đang đọc chữ từ bài viết tay qua OCR...</color>");

            if (btnOcr != null) btnOcr.interactable = false;

            Texture2D tex = _userImages.ContainsKey(_currentIndex) ? _userImages[_currentIndex] : null;

            EssayGradingService.PerformOCRAsync(tex, (result) =>
            {
                if (btnOcr != null) btnOcr.interactable = true;
                if (noticeText != null) noticeText.gameObject.SetActive(false);

                if (result.success && !string.IsNullOrEmpty(result.transcriptionText))
                {
                    _pendingOcrText = result.transcriptionText;
                    ShowOcrTranscriptModal(_pendingOcrText);
                }
                else
                {
                    ShowNotice("<color=#f56565>Không thể nhận dạng chữ hoặc ảnh mờ. Vui lòng thử lại.</color>");
                }
            });
        }'''

new_upload_block = '''        /// <summary>
        /// Nút "TẢI ẢNH": Mở hộp thoại chọn file ảnh thật từ máy tính (PNG, JPG, JPEG).
        /// Nếu người dùng bấm Cancel hoặc đóng hộp thoại: Tuyệt đối không tạo ảnh giả, không đổi trạng thái.
        /// </summary>
        private void OnUploadClicked()
        {
            Debug.Log($"[WrittenAnswerController] Mở hộp thoại chọn ảnh cho Câu {_currentIndex + 1}.");

            string filePath = SelectImageFile();

            // Nếu người dùng hủy hoặc không chọn file
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("[WrittenAnswerController] Người dùng đã hủy chọn ảnh (Cancel). Giữ nguyên trạng thái.");
                return;
            }

            if (!File.Exists(filePath))
            {
                ShowNotice("<color=#ef4444>Tệp ảnh không tồn tại trên hệ thống.</color>");
                return;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D loadedTex = new Texture2D(2, 2);
                if (loadedTex.LoadImage(bytes))
                {
                    // Giải phóng ảnh cũ của câu này nếu có
                    if (_userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null)
                    {
                        Destroy(_userImages[_currentIndex]);
                    }

                    _userImages[_currentIndex] = loadedTex;
                    UpdateImagePreview();
                    ShowNotice("<color=#38d368>Đã tải ảnh bài làm thành công! Bấm 'ĐỌC CHỮ OCR' nếu muốn trích xuất văn bản.</color>");
                }
                else
                {
                    Destroy(loadedTex);
                    ShowNotice("<color=#ef4444>Không thể giải mã định dạng ảnh. Vui lòng chọn tệp PNG hoặc JPG hợp lệ.</color>");
                }
            }
            catch (Exception ex)
            {
                ShowNotice($"<color=#ef4444>Lỗi nạp tệp ảnh: {ex.Message}</color>");
            }
        }

        private string SelectImageFile()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanelWithFilters(
                "Chọn ảnh bài làm học sinh (PNG, JPG)",
                "",
                new string[] { "Image files", "png,jpg,jpeg", "All files", "*" });
#else
            return OpenFilePanelWindows("Chọn ảnh bài làm học sinh", "Image Files (*.png;*.jpg;*.jpeg)\0*.png;*.jpg;*.jpeg\0All Files (*.*)\0*.*\0\0");
#endif
        }

#if !UNITY_EDITOR
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public struct OpenFileName
        {
            public int structSize;
            public IntPtr dlgOwner;
            public IntPtr instance;
            public string filter;
            public string customFilter;
            public int maxCustFilter;
            public int filterIndex;
            public string file;
            public int maxFile;
            public string fileTitle;
            public int maxFileTitle;
            public string initialDir;
            public string title;
            public int flags;
            public short fileOffset;
            public short fileExtension;
            public string defExt;
            public IntPtr custData;
            public IntPtr hook;
            public string templateName;
            public IntPtr reservedPtr;
            public int reservedInt;
            public int flagsEx;
        }

        [System.Runtime.InteropServices.DllImport("comdlg32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool GetOpenFileName([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] ref OpenFileName ofn);

        private static string OpenFilePanelWindows(string title, string filter)
        {
            var ofn = new OpenFileName();
            ofn.structSize = System.Runtime.InteropServices.Marshal.SizeOf(ofn);
            ofn.filter = filter;
            ofn.file = new string(new char[260]);
            ofn.maxFile = ofn.file.Length;
            ofn.title = title;
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            if (GetOpenFileName(ref ofn))
            {
                return ofn.file;
            }
            return null;
        }
#endif

        /// <summary>
        /// Nút "XÓA ẢNH": Xóa ảnh đã tải của câu hiện tại.
        /// </summary>
        private void OnDeleteImgClicked()
        {
            if (_userImages.ContainsKey(_currentIndex) && _userImages[_currentIndex] != null)
            {
                Destroy(_userImages[_currentIndex]);
                _userImages[_currentIndex] = null;
                UpdateImagePreview();
                ShowNotice("<color=#d97706>Đã xóa ảnh của câu này.</color>");
            }
            else
            {
                ShowNotice("Chưa có ảnh nào được tải lên ở câu này.");
            }
        }

        /// <summary>
        /// Nút "📷 ĐỌC CHỮ OCR": Trích xuất văn bản viết tay từ ảnh.
        /// Minh bạch: Nếu chưa có engine OCR trực tiếp, thông báo rõ ràng, không giả lập đáp án mẫu.
        /// Cho phép học sinh xem và sửa văn bản trước khi gửi Gemini chấm.
        /// </summary>
        private void OnOcrButtonClicked()
        {
            Texture2D tex = _userImages.ContainsKey(_currentIndex) ? _userImages[_currentIndex] : null;
            if (tex == null)
            {
                ShowNotice("<color=#d97706>Vui lòng tải ảnh bài làm lên trước khi nhận dạng OCR.</color>");
                return;
            }

            Debug.Log($"[WrittenAnswerController] Yêu cầu OCR cho Câu {_currentIndex + 1}.");
            ShowNotice("<color=#f59e0b>Chức năng OCR tự động đang trong quá trình tích hợp. Mở khung xem và nhập văn bản bài làm từ ảnh...</color>");

            // Mở modal kiểm tra & chỉnh sửa văn bản từ ảnh cho học sinh
            string currentText = _userAnswers.ContainsKey(_currentIndex) ? _userAnswers[_currentIndex] : "";
            ShowOcrTranscriptModal(currentText);
        }'''

c_norm = c.replace('\r\n', '\n')
f_ok = old_fields.replace('\r\n', '\n') in c_norm
sb_ok = old_sidebar_bind.replace('\r\n', '\n') in c_norm
ds_ok = old_display_sidebar.replace('\r\n', '\n') in c_norm
pr_ok = old_preview.replace('\r\n', '\n') in c_norm
up_ok = old_upload_block.replace('\r\n', '\n') in c_norm

print(f'fields: {f_ok}, sidebar_bind: {sb_ok}, display_sb: {ds_ok}, preview: {pr_ok}, upload_block: {up_ok}')

if f_ok and sb_ok and ds_ok and pr_ok and up_ok:
    res = c_norm.replace(old_fields.replace('\r\n', '\n'), new_fields.replace('\r\n', '\n'))
    res = res.replace(old_sidebar_bind.replace('\r\n', '\n'), new_sidebar_bind.replace('\r\n', '\n'))
    res = res.replace(old_display_sidebar.replace('\r\n', '\n'), new_display_sidebar.replace('\r\n', '\n'))
    res = res.replace(old_preview.replace('\r\n', '\n'), new_preview.replace('\r\n', '\n'))
    res = res.replace(old_upload_block.replace('\r\n', '\n'), new_upload_block.replace('\r\n', '\n'))

    with open('Assets/Scripts/PhysicsGame2/UI/WrittenAnswerController.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(res)
    print('SUCCESS WrittenAnswerController.cs patched')
else:
    print('FAILED TO MATCH')
