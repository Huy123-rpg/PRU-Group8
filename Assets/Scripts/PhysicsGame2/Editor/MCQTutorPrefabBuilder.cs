#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    [InitializeOnLoad]
    public static class MCQTutorPrefabBuilder
    {
        private const string ATLAS_PATH = "Assets/Art/Environment/Bộ sprite giao diện Aurelia fantasy.png";
        private const string FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
        public const string PREFAB_PATH = "Assets/Resources/UI/MCQTutorPopup.prefab";

        static MCQTutorPrefabBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                // Always rebuild prefab when scripts compile
                if (true)
                {
                    BuildPrefab();
                }
            };
        }

        [MenuItem("Aurelia/Generate MCQ Tutor Prefab")]
        public static void BuildPrefab()
        {
            // Ensure directory
            string dir = Path.GetDirectoryName(PREFAB_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Load atlas sprites
            Sprite sprBoard = null;
            Sprite sprTitle = null;
            Sprite sprContext = null;
            Sprite sprInput = null;
            Sprite sprSend = null;
            Sprite sprClose = null;
            Sprite sprCloseX = null;
            Sprite sprAiBubble = null;
            Sprite sprStudentBubble = null;

            var allSprites = AssetDatabase.LoadAllAssetsAtPath(ATLAS_PATH);
            foreach (var asset in allSprites)
            {
                if (asset is Sprite s)
                {
                    if (s.name.EndsWith("_0")) sprBoard = s;
                    else if (s.name.EndsWith("_4")) sprTitle = s;
                    else if (s.name.EndsWith("_5")) sprContext = s;
                    else if (s.name.EndsWith("_9")) sprSend = s;
                    else if (s.name.EndsWith("_10")) sprClose = s;
                    else if (s.name.EndsWith("_11")) sprCloseX = s;
                    else if (s.name.EndsWith("_12")) sprInput = s;
                    else if (s.name.EndsWith("_13")) sprAiBubble = s;
                    else if (s.name.EndsWith("_14")) sprStudentBubble = s;
                }
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);

            // 1. Root: MCQTutorPopup
            GameObject rootGo = new GameObject("MCQTutorPopup");
            RectTransform rootRt = rootGo.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.pivot = new Vector2(0.5f, 0.5f);

            Image backdrop = rootGo.AddComponent<Image>();
            backdrop.color = new Color(0.01f, 0.03f, 0.08f, 0.82f);
            backdrop.raycastTarget = true;

            CanvasGroup cg = rootGo.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            // 2. Board Background (1560 x 940)
            GameObject boardGo = new GameObject("Board_Background");
            boardGo.transform.SetParent(rootGo.transform, false);
            RectTransform boardRt = boardGo.AddComponent<RectTransform>();
            boardRt.anchorMin = new Vector2(0.5f, 0.5f);
            boardRt.anchorMax = new Vector2(0.5f, 0.5f);
            boardRt.pivot = new Vector2(0.5f, 0.5f);
            boardRt.anchoredPosition = Vector2.zero;
            boardRt.sizeDelta = new Vector2(1560f, 940f);

            Image boardImg = boardGo.AddComponent<Image>();
            if (sprBoard != null) { boardImg.sprite = sprBoard; boardImg.type = Image.Type.Simple; }
            boardImg.color = Color.white;
            boardImg.raycastTarget = true;

            // Template_AiBubble (chứa sprite _13)
            GameObject tplAi = new GameObject("Template_AiBubble");
            tplAi.transform.SetParent(boardGo.transform, false);
            Image aiImg = tplAi.AddComponent<Image>();
            if (sprAiBubble != null) { aiImg.sprite = sprAiBubble; aiImg.type = Image.Type.Sliced; }
            tplAi.SetActive(false);

            // Template_StudentBubble (chứa sprite _14)
            GameObject tplStu = new GameObject("Template_StudentBubble");
            tplStu.transform.SetParent(boardGo.transform, false);
            Image stuImg = tplStu.AddComponent<Image>();
            if (sprStudentBubble != null) { stuImg.sprite = sprStudentBubble; stuImg.type = Image.Type.Sliced; }
            tplStu.SetActive(false);

            // 3. Header: Banner_Title (Pos Y = 395)
            GameObject titleGo = new GameObject("Banner_Title");
            titleGo.transform.SetParent(boardGo.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, 395f);
            titleRt.sizeDelta = new Vector2(980f, 160f);

            Image titleImg = titleGo.AddComponent<Image>();
            if (sprTitle != null) { titleImg.sprite = sprTitle; titleImg.type = Image.Type.Simple; }
            titleImg.color = Color.white;
            titleImg.raycastTarget = false;

            // Text_Title
            GameObject tTextGo = new GameObject("Text_Title");
            tTextGo.transform.SetParent(titleGo.transform, false);
            RectTransform tTextRt = tTextGo.AddComponent<RectTransform>();
            tTextRt.anchorMin = new Vector2(0f, 0.5f);
            tTextRt.anchorMax = new Vector2(1f, 1f);
            tTextRt.anchoredPosition = new Vector2(0f, -14f);
            tTextRt.sizeDelta = new Vector2(-100f, 0f);
            TextMeshProUGUI tTmp = tTextGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) tTmp.font = fontAsset;
            tTmp.text = "GIA SƯ AI AURELIA";
            tTmp.fontSize = 36f;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.color = new Color(1f, 0.94f, 0.70f, 1f);
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.raycastTarget = false;

            // Text_Subtitle
            GameObject sTextGo = new GameObject("Text_Subtitle");
            sTextGo.transform.SetParent(titleGo.transform, false);
            RectTransform sTextRt = sTextGo.AddComponent<RectTransform>();
            sTextRt.anchorMin = new Vector2(0f, 0f);
            sTextRt.anchorMax = new Vector2(1f, 0.5f);
            sTextRt.anchoredPosition = new Vector2(0f, 12f);
            sTextRt.sizeDelta = new Vector2(-100f, 0f);
            TextMeshProUGUI sTmp = sTextGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) sTmp.font = fontAsset;
            sTmp.text = "Người đồng hành Vật lý 9";
            sTmp.fontSize = 19f;
            sTmp.fontStyle = FontStyles.Italic;
            sTmp.color = new Color(0.78f, 0.90f, 1f, 1f);
            sTmp.alignment = TextAlignmentOptions.Center;
            sTmp.raycastTarget = false;

            // Btn_CloseTopRight
            GameObject closeXGo = new GameObject("Btn_CloseTopRight");
            closeXGo.transform.SetParent(boardGo.transform, false);
            RectTransform xRt = closeXGo.AddComponent<RectTransform>();
            xRt.anchorMin = new Vector2(0.5f, 0.5f);
            xRt.anchorMax = new Vector2(0.5f, 0.5f);
            xRt.pivot = new Vector2(0.5f, 0.5f);
            xRt.anchoredPosition = new Vector2(705f, 395f);
            xRt.sizeDelta = new Vector2(64f, 64f);
            Image xImg = closeXGo.AddComponent<Image>();
            if (sprCloseX != null) { xImg.sprite = sprCloseX; xImg.type = Image.Type.Simple; }
            else xImg.color = new Color(0.8f, 0.15f, 0.15f, 1f);
            xImg.raycastTarget = true;
            closeXGo.AddComponent<Button>();

            // 4. Banner_Context (Pos Y = 285, Size 1340 x 95)
            GameObject ctxGo = new GameObject("Banner_Context");
            ctxGo.transform.SetParent(boardGo.transform, false);
            RectTransform ctxRt = ctxGo.AddComponent<RectTransform>();
            ctxRt.anchorMin = new Vector2(0.5f, 0.5f);
            ctxRt.anchorMax = new Vector2(0.5f, 0.5f);
            ctxRt.pivot = new Vector2(0.5f, 0.5f);
            ctxRt.anchoredPosition = new Vector2(0f, 285f);
            ctxRt.sizeDelta = new Vector2(1340f, 95f);
            Image ctxImg = ctxGo.AddComponent<Image>();
            if (sprContext != null) { ctxImg.sprite = sprContext; ctxImg.type = Image.Type.Simple; }
            else ctxImg.color = new Color(0.08f, 0.16f, 0.32f, 0.95f);
            ctxImg.raycastTarget = false;

            // Text_ContextInfo (con của Banner_Context)
            GameObject ctxTextGo = new GameObject("Text_ContextInfo");
            ctxTextGo.transform.SetParent(ctxGo.transform, false);
            RectTransform ctRt = ctxTextGo.AddComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.offsetMin = new Vector2(250f, 6f);
            ctRt.offsetMax = new Vector2(-40f, -6f);
            TextMeshProUGUI cTmp = ctxTextGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) cTmp.font = fontAsset;
            cTmp.text = "<b><color=#FFE57F>CÂU 1/10</color></b> • <b><color=#80D8FF>[Nhận biết]</color></b>: Năng lượng của dòng điện được gọi là gì?";
            cTmp.fontSize = 20f;
            cTmp.color = Color.white;
            cTmp.alignment = TextAlignmentOptions.MidlineLeft;
            cTmp.textWrappingMode = TextWrappingModes.Normal;
            cTmp.lineSpacing = 4f;
            cTmp.raycastTarget = false;

            // 5. Chat_ScrollView (Pos Y = 30, Size 1340 x 400)
            GameObject scrollGo = new GameObject("Chat_ScrollView");
            scrollGo.transform.SetParent(boardGo.transform, false);
            RectTransform sRt = scrollGo.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.pivot = new Vector2(0.5f, 0.5f);
            sRt.anchoredPosition = new Vector2(0f, 30f);
            sRt.sizeDelta = new Vector2(1340f, 400f);

            Image scrollBg = scrollGo.AddComponent<Image>();
            scrollBg.color = new Color(0.02f, 0.05f, 0.12f, 0.70f);
            scrollBg.raycastTarget = false;

            ScrollRect sr = scrollGo.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40f;

            // Viewport
            GameObject vpGo = new GameObject("Viewport");
            vpGo.transform.SetParent(scrollGo.transform, false);
            RectTransform vpRt = vpGo.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.sizeDelta = Vector2.zero;
            Image vpImg = vpGo.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            vpGo.AddComponent<RectMask2D>();
            sr.viewport = vpRt;

            // Content
            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(vpGo.transform, false);
            RectTransform contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 16f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = contentRt;

            // 6. QuickPromptsBar (Pos Y = -215, Size 1340 x 42)
            GameObject qpGo = new GameObject("QuickPromptsBar");
            qpGo.transform.SetParent(boardGo.transform, false);
            RectTransform qpRt = qpGo.AddComponent<RectTransform>();
            qpRt.anchorMin = new Vector2(0.5f, 0.5f);
            qpRt.anchorMax = new Vector2(0.5f, 0.5f);
            qpRt.pivot = new Vector2(0.5f, 0.5f);
            qpRt.anchoredPosition = new Vector2(0f, -215f);
            qpRt.sizeDelta = new Vector2(1340f, 42f);

            HorizontalLayoutGroup qpHlg = qpGo.AddComponent<HorizontalLayoutGroup>();
            qpHlg.spacing = 14f;
            qpHlg.childControlWidth = false;
            qpHlg.childControlHeight = true;
            qpHlg.childForceExpandWidth = false;
            qpHlg.childForceExpandHeight = false;
            qpHlg.childAlignment = TextAnchor.MiddleLeft;

            // 7. BottomInputBar (Pos Y = -290, Size 1340 x 85)
            GameObject botGo = new GameObject("BottomInputBar");
            botGo.transform.SetParent(boardGo.transform, false);
            RectTransform botRt = botGo.AddComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0.5f, 0.5f);
            botRt.anchorMax = new Vector2(0.5f, 0.5f);
            botRt.pivot = new Vector2(0.5f, 0.5f);
            botRt.anchoredPosition = new Vector2(0f, -290f);
            botRt.sizeDelta = new Vector2(1340f, 85f);

            // Box_InputField (Pos X = 0, Size 940 x 85)
            GameObject inBoxGo = new GameObject("Box_InputField");
            inBoxGo.transform.SetParent(botGo.transform, false);
            RectTransform inBoxRt = inBoxGo.AddComponent<RectTransform>();
            inBoxRt.anchorMin = new Vector2(0f, 0.5f);
            inBoxRt.anchorMax = new Vector2(0f, 0.5f);
            inBoxRt.pivot = new Vector2(0f, 0.5f);
            inBoxRt.anchoredPosition = new Vector2(0f, 0f);
            inBoxRt.sizeDelta = new Vector2(940f, 85f);

            Image inImg = inBoxGo.AddComponent<Image>();
            if (sprInput != null) { inImg.sprite = sprInput; inImg.type = Image.Type.Simple; }
            else inImg.color = new Color(0.08f, 0.14f, 0.28f, 0.95f);
            inImg.raycastTarget = true;

            // TextArea (OffsetMin x = 145 để vượt qua hoàn toàn icon cây bút của sprite)
            GameObject taGo = new GameObject("TextArea");
            taGo.transform.SetParent(inBoxGo.transform, false);
            RectTransform taRt = taGo.AddComponent<RectTransform>();
            taRt.anchorMin = Vector2.zero;
            taRt.anchorMax = Vector2.one;
            taRt.offsetMin = new Vector2(180f, 12f);
            taRt.offsetMax = new Vector2(-30f, -12f);
            taGo.AddComponent<RectMask2D>();

            // Placeholder
            GameObject phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(taGo.transform, false);
            RectTransform phRt = phGo.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero;
            phRt.offsetMax = Vector2.zero;
            TextMeshProUGUI phTmp = phGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) phTmp.font = fontAsset;
            phTmp.text = "Nhập câu hỏi của em...";
            phTmp.fontSize = 20f;
            phTmp.fontStyle = FontStyles.Italic;
            phTmp.color = new Color(0.55f, 0.70f, 0.90f, 0.85f);
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;
            phTmp.raycastTarget = false;

            // Text
            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(taGo.transform, false);
            RectTransform txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            TextMeshProUGUI txtTmp = txtGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) txtTmp.font = fontAsset;
            txtTmp.fontSize = 20f;
            txtTmp.color = Color.white; // Crisp white on dark navy input bar
            txtTmp.alignment = TextAlignmentOptions.MidlineLeft;
            txtTmp.textWrappingMode = TextWrappingModes.Normal;
            txtTmp.raycastTarget = false;

            TMP_InputField inputField = inBoxGo.AddComponent<TMP_InputField>();
            inputField.targetGraphic = inImg;
            inputField.textViewport = taRt;
            inputField.textComponent = txtTmp;
            inputField.placeholder = phTmp;
            inputField.lineType = TMP_InputField.LineType.MultiLineSubmit;
            inputField.characterLimit = 1000;
            inputField.interactable = true;

            // Btn_Send (Pos X = 960, Size 180 x 85)
            GameObject sendGo = new GameObject("Btn_Send");
            sendGo.transform.SetParent(botGo.transform, false);
            RectTransform sendRt = sendGo.AddComponent<RectTransform>();
            sendRt.anchorMin = new Vector2(0f, 0.5f);
            sendRt.anchorMax = new Vector2(0f, 0.5f);
            sendRt.pivot = new Vector2(0f, 0.5f);
            sendRt.anchoredPosition = new Vector2(960f, 0f);
            sendRt.sizeDelta = new Vector2(180f, 85f);
            Image sendImg = sendGo.AddComponent<Image>();
            if (sprSend != null) { sendImg.sprite = sprSend; sendImg.type = Image.Type.Simple; }
            else sendImg.color = new Color(0.12f, 0.55f, 0.28f, 1f);
            sendImg.raycastTarget = true;
            sendGo.AddComponent<Button>();

            // Text_SendLabel: Đặt trong vùng [38%, 96%] của nút để không đè lên chiếc mũ bên trái
            GameObject sendTxtGo = new GameObject("Text_SendLabel");
            sendTxtGo.transform.SetParent(sendGo.transform, false);
            RectTransform stRt = sendTxtGo.AddComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.44f, 0f);
            stRt.anchorMax = new Vector2(0.96f, 1f);
            stRt.offsetMin = Vector2.zero;
            stRt.offsetMax = Vector2.zero;
            TextMeshProUGUI sendTmp = sendTxtGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) sendTmp.font = fontAsset;
            sendTmp.text = "<b>GỬI</b>";
            sendTmp.fontSize = 24f;
            sendTmp.color = Color.white;
            sendTmp.alignment = TextAlignmentOptions.Center;
            sendTmp.raycastTarget = false;

            // Btn_CloseBottom (Pos X = 1160, Size 180 x 85)
            GameObject closeGo = new GameObject("Btn_CloseBottom");
            closeGo.transform.SetParent(botGo.transform, false);
            RectTransform closeRt = closeGo.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0f, 0.5f);
            closeRt.anchorMax = new Vector2(0f, 0.5f);
            closeRt.pivot = new Vector2(0f, 0.5f);
            closeRt.anchoredPosition = new Vector2(1160f, 0f);
            closeRt.sizeDelta = new Vector2(180f, 85f);
            Image closeImg = closeGo.AddComponent<Image>();
            if (sprClose != null) { closeImg.sprite = sprClose; closeImg.type = Image.Type.Simple; }
            else closeImg.color = new Color(0.18f, 0.24f, 0.38f, 1f);
            closeImg.raycastTarget = true;
            closeGo.AddComponent<Button>();

            // Text_CloseLabel: Đặt trong vùng [38%, 96%] của nút để không đè lên khiên chữ X bên trái
            GameObject closeTxtGo = new GameObject("Text_CloseLabel");
            closeTxtGo.transform.SetParent(closeGo.transform, false);
            RectTransform crt = closeTxtGo.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.46f, 0f);
            crt.anchorMax = new Vector2(0.96f, 1f);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            TextMeshProUGUI closeTmp = closeTxtGo.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) closeTmp.font = fontAsset;
            closeTmp.text = "<b>ĐÓNG</b>";
            closeTmp.fontSize = 22f;
            closeTmp.color = Color.white;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.raycastTarget = false;

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(rootGo, PREFAB_PATH);
            Object.DestroyImmediate(rootGo);
            AssetDatabase.Refresh();
            Debug.Log($"[MCQTutorPrefabBuilder] Đã xuất thành công Prefab tại '{PREFAB_PATH}'!");
        }
    }
}
#endif
