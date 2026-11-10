#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    /// <summary>
    /// Builder tự động kiến tạo và đồng bộ Scene "24.1. Tổng quan tiến độ.unity"
    /// tuân thủ 100% thiết kế Fantasy Aurelia (khung xanh vàng, font TMP, 1920x1080).
    /// </summary>
    [InitializeOnLoad]
    public static class ProgressOverviewSceneBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/Physics/Game2/24.1. Tổng quan tiến độ.unity";
        private const string BG_PATH = "Assets/Art/Environment/Đại sảnh thư viện huyền thuật.png";
        private const string ATLAS_PROG1_PATH = "Assets/Art/Environment/Bộ sprite giao diện tiến độ xanh vàng 1.png";
        private const string ATLAS_PROG_PATH = "Assets/Art/Environment/Bộ sprite giao diện tiến độ xanh vàng.png";
        private const string ATLAS_RANK_PATH = "Assets/Art/Environment/Atlas UI bảng xếp hạng xanh vàng.png";
        private const string FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";

        static ProgressOverviewSceneBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(SCENE_PATH))
                {
                    BuildOverviewScene();
                }
            };
        }

        [MenuItem("Aurelia/Build Progress Overview Scene (24.1)")]
        public static void BuildOverviewScene()
        {
            Debug.Log("[ProgressOverviewSceneBuilder] Bắt đầu tạo Scene 24.1. Tổng quan tiến độ...");

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH) ?? TMP_Settings.defaultFontAsset;
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_PATH);
            var prog1Sprites = AssetDatabase.LoadAllAssetsAtPath(ATLAS_PROG1_PATH).OfType<Sprite>().ToArray();
            var progSprites = AssetDatabase.LoadAllAssetsAtPath(ATLAS_PROG_PATH).OfType<Sprite>().ToArray();
            var rankSprites = AssetDatabase.LoadAllAssetsAtPath(ATLAS_RANK_PATH).OfType<Sprite>().ToArray();

            // Lấy các sprite Fantasy phù hợp
            var titleBannerSprite = prog1Sprites.FirstOrDefault(s => s.name.Contains("_0")) ?? prog1Sprites.FirstOrDefault();
            var cardFrameSprite = prog1Sprites.FirstOrDefault(s => s.name.Contains("_14")) ?? prog1Sprites.FirstOrDefault();
            var mainBoardSprite = progSprites.FirstOrDefault(s => s.name.Contains("_0")) ?? progSprites.FirstOrDefault();
            var actionBtnSprite = prog1Sprites.FirstOrDefault(s => s.name.Contains("_16")) ?? prog1Sprites.FirstOrDefault();
            var backBtnSprite = rankSprites.FirstOrDefault(s => s.name.Contains("1") || s.name.Contains("back")) ?? rankSprites.FirstOrDefault();
            var gemSprite = prog1Sprites.FirstOrDefault(s => s.name.Contains("_22") || s.name.Contains("_21")) ?? prog1Sprites.FirstOrDefault();

            var currentScene = EditorSceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            // 1. Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.08f, 0.16f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.AddComponent<AudioListener>();

            // 2. EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();

            // 3. Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var controller = canvasGo.AddComponent<ProgressOverviewController>();

            // 4. Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            // 5. Top Bar (Header + Back Button)
            var topBarGo = new GameObject("TopBar");
            topBarGo.transform.SetParent(canvasGo.transform, false);
            var topBarRt = topBarGo.AddComponent<RectTransform>();
            topBarRt.anchorMin = new Vector2(0.5f, 1f);
            topBarRt.anchorMax = new Vector2(0.5f, 1f);
            topBarRt.anchoredPosition = new Vector2(0, -75f);
            topBarRt.sizeDelta = new Vector2(1800, 120);

            // Back button
            var backBtnGo = new GameObject("Btn_Back");
            backBtnGo.transform.SetParent(topBarGo.transform, false);
            var backBtnRt = backBtnGo.AddComponent<RectTransform>();
            backBtnRt.anchoredPosition = new Vector2(-780f, 0);
            backBtnRt.sizeDelta = new Vector2(200f, 58f);
            var backBtnImg = backBtnGo.AddComponent<Image>();
            backBtnImg.sprite = backBtnSprite;
            backBtnImg.type = Image.Type.Sliced;
            backBtnImg.raycastTarget = true;
            backBtnGo.AddComponent<Button>();

            var backText = CreateTmp(backBtnGo, "Txt_Back", "< Quay lại", 22f, Color.white, font, TextAlignmentOptions.Center);
            StretchFull(backText.rectTransform);

            // Title Banner
            var bannerGo = new GameObject("HeaderBanner");
            bannerGo.transform.SetParent(topBarGo.transform, false);
            var bannerRt = bannerGo.AddComponent<RectTransform>();
            bannerRt.anchoredPosition = new Vector2(0, 0);
            bannerRt.sizeDelta = new Vector2(850f, 140f);
            var bannerImg = bannerGo.AddComponent<Image>();
            bannerImg.sprite = titleBannerSprite;
            bannerImg.type = Image.Type.Simple;
            bannerImg.raycastTarget = false;

            var titleTmp = CreateTmp(bannerGo, "Txt_Title", "TỔNG QUAN TIẾN ĐỘ HỌC TẬP", 32f, new Color(1f, 0.92f, 0.65f), font, TextAlignmentOptions.Center, true);
            titleTmp.rectTransform.anchoredPosition = new Vector2(0, 10f);
            titleTmp.rectTransform.sizeDelta = new Vector2(750f, 50f);

            // 6. Cột Trái - THỐNG KÊ TỔNG QUAN
            var leftPanelGo = new GameObject("LeftPanel_Stats");
            leftPanelGo.transform.SetParent(canvasGo.transform, false);
            var leftPanelRt = leftPanelGo.AddComponent<RectTransform>();
            leftPanelRt.anchoredPosition = new Vector2(-540f, -60f);
            leftPanelRt.sizeDelta = new Vector2(560f, 740f);
            var leftPanelImg = leftPanelGo.AddComponent<Image>();
            leftPanelImg.sprite = mainBoardSprite;
            leftPanelImg.type = Image.Type.Sliced;
            leftPanelImg.color = new Color(0.9f, 0.95f, 1f, 0.95f);

            var statsHeaderTmp = CreateTmp(leftPanelGo, "Txt_StatsHeader", "THỐNG KÊ TỔNG QUAN", 24f, new Color(1f, 0.84f, 0.2f), font, TextAlignmentOptions.Center, true);
            statsHeaderTmp.rectTransform.anchoredPosition = new Vector2(0, 310f);
            statsHeaderTmp.rectTransform.sizeDelta = new Vector2(480f, 40f);

            // 6 Card thống kê
            string[] cardLabels = {
                "Tổng số bài đã luyện",
                "Tổng lượt hoàn thành",
                "Điểm trung bình",
                "Điểm cao nhất",
                "Lượt học gần nhất",
                "Tỷ lệ hoàn thành"
            };
            string[] cardValNames = {
                "Txt_TotalLessons",
                "Txt_TotalAttempts",
                "Txt_AvgScore",
                "Txt_MaxScore",
                "Txt_LastDate",
                "Txt_CompletionRate"
            };
            string[] defaultValues = { "0 bài", "0 lượt", "—", "—", "Chưa có", "0%" };
            Color[] valueColors = {
                new Color(1f, 0.92f, 0.65f),
                new Color(1f, 0.92f, 0.65f),
                new Color(1f, 0.85f, 0.35f),
                new Color(1f, 0.85f, 0.35f),
                new Color(0.6f, 0.9f, 1f),
                new Color(0.45f, 0.95f, 0.5f)
            };

            float startY = 225f;
            float stepY = 88f;
            for (int i = 0; i < 6; i++)
            {
                var cardGo = new GameObject($"Card_{i+1}");
                cardGo.transform.SetParent(leftPanelGo.transform, false);
                var cardRt = cardGo.AddComponent<RectTransform>();
                cardRt.anchoredPosition = new Vector2(0, startY - i * stepY);
                cardRt.sizeDelta = new Vector2(490f, 74f);
                var cardImg = cardGo.AddComponent<Image>();
                cardImg.sprite = cardFrameSprite;
                cardImg.type = Image.Type.Sliced;
                cardImg.color = new Color(0.95f, 0.95f, 1f, 0.95f);

                var lblTmp = CreateTmp(cardGo, "Label", cardLabels[i], 18f, new Color(0.88f, 0.85f, 0.78f), font, TextAlignmentOptions.Left);
                lblTmp.rectTransform.anchoredPosition = new Vector2(-65f, 0);
                lblTmp.rectTransform.sizeDelta = new Vector2(300f, 36f);

                var valTmp = CreateTmp(cardGo, cardValNames[i], defaultValues[i], i == 4 ? 17f : 22f, valueColors[i], font, TextAlignmentOptions.Right, true);
                valTmp.rectTransform.anchoredPosition = new Vector2(155f, 0);
                valTmp.rectTransform.sizeDelta = new Vector2(150f, 36f);
            }

            // 7. Cột Phải - DANH SÁCH BÀI HỌC & BỘ LỌC
            var rightPanelGo = new GameObject("RightPanel_Lessons");
            rightPanelGo.transform.SetParent(canvasGo.transform, false);
            var rightPanelRt = rightPanelGo.AddComponent<RectTransform>();
            rightPanelRt.anchoredPosition = new Vector2(300f, -60f);
            rightPanelRt.sizeDelta = new Vector2(1080f, 740f);
            var rightPanelImg = rightPanelGo.AddComponent<Image>();
            rightPanelImg.sprite = mainBoardSprite;
            rightPanelImg.type = Image.Type.Sliced;
            rightPanelImg.color = new Color(0.9f, 0.95f, 1f, 0.95f);

            // Filter Bar
            var filterBarGo = new GameObject("FilterBar");
            filterBarGo.transform.SetParent(rightPanelGo.transform, false);
            var filterBarRt = filterBarGo.AddComponent<RectTransform>();
            filterBarRt.anchoredPosition = new Vector2(0, 310f);
            filterBarRt.sizeDelta = new Vector2(1020f, 54f);

            // Chapter filter
            CreateButton(filterBarGo, "Btn_PrevChapter", new Vector2(-440f, 0), new Vector2(42f, 42f), backBtnSprite, "◀", font, 18f);
            CreateFrameWithText(filterBarGo, "Frame_Chapter", "Txt_ChapterTitle", new Vector2(-265f, 0), new Vector2(300f, 46f), cardFrameSprite, "Chương I: Năng lượng cơ học", font, 17f, new Color(1f, 0.92f, 0.65f));
            CreateButton(filterBarGo, "Btn_NextChapter", new Vector2(-90f, 0), new Vector2(42f, 42f), backBtnSprite, "▶", font, 18f);

            // Difficulty filter
            CreateButton(filterBarGo, "Btn_PrevDifficulty", new Vector2(-15f, 0), new Vector2(42f, 42f), backBtnSprite, "◀", font, 18f);
            CreateFrameWithText(filterBarGo, "Frame_Difficulty", "Txt_DifficultyTitle", new Vector2(120f, 0), new Vector2(215f, 46f), cardFrameSprite, "Nhận biết", font, 17f, new Color(1f, 0.92f, 0.65f));
            CreateButton(filterBarGo, "Btn_NextDifficulty", new Vector2(250f, 0), new Vector2(42f, 42f), backBtnSprite, "▶", font, 18f);

            // Mode filter (Trắc nghiệm)
            CreateFrameWithText(filterBarGo, "Frame_Mode", "Txt_ModeTitle", new Vector2(385f, 0), new Vector2(190f, 46f), cardFrameSprite, "Trắc nghiệm", font, 17f, new Color(0.6f, 0.9f, 1f));

            // Lesson List Container
            var listContainerGo = new GameObject("LessonContainer");
            listContainerGo.transform.SetParent(rightPanelGo.transform, false);
            var listContainerRt = listContainerGo.AddComponent<RectTransform>();
            listContainerRt.anchoredPosition = new Vector2(0, 20f);
            listContainerRt.sizeDelta = new Vector2(1020f, 480f);

            // 7 Lesson rows
            float rowStartY = 195f;
            float rowStepY = 65f;
            for (int r = 0; r < 7; r++)
            {
                var rowGo = new GameObject($"LessonItem_{r}");
                rowGo.transform.SetParent(listContainerGo.transform, false);
                var rowRt = rowGo.AddComponent<RectTransform>();
                rowRt.anchoredPosition = new Vector2(0, rowStartY - r * rowStepY);
                rowRt.sizeDelta = new Vector2(1000f, 56f);

                var rowImg = rowGo.AddComponent<Image>();
                rowImg.sprite = cardFrameSprite;
                rowImg.type = Image.Type.Sliced;
                rowImg.raycastTarget = true;
                rowGo.AddComponent<Button>();

                var rowUI = rowGo.AddComponent<LessonOverviewRowUI>();

                // Highlight border
                var hlGo = new GameObject("Img_HighlightBorder");
                hlGo.transform.SetParent(rowGo.transform, false);
                var hlRt = hlGo.AddComponent<RectTransform>();
                StretchFull(hlRt);
                var hlImg = hlGo.AddComponent<Image>();
                hlImg.sprite = cardFrameSprite;
                hlImg.type = Image.Type.Sliced;
                hlImg.color = new Color(1f, 0.84f, 0f, 0.95f);
                hlImg.raycastTarget = false;
                hlGo.SetActive(false);

                // Gem icon
                var gemGo = new GameObject("Img_Gem");
                gemGo.transform.SetParent(rowGo.transform, false);
                var gemRt = gemGo.AddComponent<RectTransform>();
                gemRt.anchoredPosition = new Vector2(-465f, 0);
                gemRt.sizeDelta = new Vector2(28f, 28f);
                var gemImg = gemGo.AddComponent<Image>();
                gemImg.sprite = gemSprite;
                gemImg.raycastTarget = false;

                // Lesson Name
                var nameTmp = CreateTmp(rowGo, "Txt_LessonName", $"Bài {r+2}. Tên bài học mẫu", 18f, Color.white, font, TextAlignmentOptions.Left, true);
                nameTmp.rectTransform.anchoredPosition = new Vector2(-190f, 0);
                nameTmp.rectTransform.sizeDelta = new Vector2(490f, 36f);

                // Attempts Count
                var attTmp = CreateTmp(rowGo, "Txt_AttemptsCount", "0 lượt", 16f, new Color(0.6f, 0.9f, 1f), font, TextAlignmentOptions.Center);
                attTmp.rectTransform.anchoredPosition = new Vector2(120f, 0);
                attTmp.rectTransform.sizeDelta = new Vector2(90f, 36f);

                // Latest Score
                var latestTmp = CreateTmp(rowGo, "Txt_LatestScore", "Gần nhất: —", 16f, new Color(1f, 0.92f, 0.65f), font, TextAlignmentOptions.Center);
                latestTmp.rectTransform.anchoredPosition = new Vector2(235f, 0);
                latestTmp.rectTransform.sizeDelta = new Vector2(130f, 36f);

                // Best Score
                var bestTmp = CreateTmp(rowGo, "Txt_BestScore", "Cao nhất: —", 16f, new Color(1f, 0.85f, 0.35f), font, TextAlignmentOptions.Center, true);
                bestTmp.rectTransform.anchoredPosition = new Vector2(360f, 0);
                bestTmp.rectTransform.sizeDelta = new Vector2(130f, 36f);

                // Badge Bg + Text
                var badgeBgGo = new GameObject("Img_StatusBadgeBg");
                badgeBgGo.transform.SetParent(rowGo.transform, false);
                var badgeBgRt = badgeBgGo.AddComponent<RectTransform>();
                badgeBgRt.anchoredPosition = new Vector2(440f, 0);
                badgeBgRt.sizeDelta = new Vector2(86f, 32f);
                var badgeBgImg = badgeBgGo.AddComponent<Image>();
                badgeBgImg.sprite = cardFrameSprite;
                badgeBgImg.type = Image.Type.Sliced;
                badgeBgImg.color = new Color(0.18f, 0.22f, 0.28f, 0.7f);
                badgeBgImg.raycastTarget = false;

                var badgeTmp = CreateTmp(badgeBgGo, "Txt_StatusBadge", "CHƯA HỌC", 14f, new Color(0.7f, 0.7f, 0.75f), font, TextAlignmentOptions.Center, true);
                StretchFull(badgeTmp.rectTransform);

                rowUI.AutoWireRowReferences();
            }

            // Action Button (Xem chi tiết)
            var actionBtnGo = new GameObject("Btn_ViewDetail");
            actionBtnGo.transform.SetParent(rightPanelGo.transform, false);
            var actionBtnRt = actionBtnGo.AddComponent<RectTransform>();
            actionBtnRt.anchoredPosition = new Vector2(0, -280f);
            actionBtnRt.sizeDelta = new Vector2(380f, 72f);
            var actionBtnImg = actionBtnGo.AddComponent<Image>();
            actionBtnImg.sprite = actionBtnSprite;
            actionBtnImg.type = Image.Type.Sliced;
            actionBtnImg.raycastTarget = true;
            actionBtnGo.AddComponent<Button>();

            var actionTmp = CreateTmp(actionBtnGo, "Txt_Action", "Xem chi tiết  ➔", 24f, new Color(1f, 0.95f, 0.8f), font, TextAlignmentOptions.Center, true);
            StretchFull(actionTmp.rectTransform);

            // Tự động kết nối references vào ProgressOverviewController
            controller.AutoWireUIReferences();

            // Lưu scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            EditorSceneManager.CloseScene(scene, true);

            // Khôi phục active scene cũ nếu có
            if (currentScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(currentScene);
            }

            // Đồng bộ vào Build Settings
            PhysicsGame2BuildSettingsAutoSync.SyncScenes();

            Debug.Log($"[ProgressOverviewSceneBuilder] THÀNH CÔNG: Đã tạo và lưu Scene '{SCENE_PATH}'!");
        }

        private static TextMeshProUGUI CreateTmp(GameObject parent, string name, string text, float size, Color color, TMP_FontAsset font, TextAlignmentOptions align, bool bold = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.font = font;
            tmp.alignment = align;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(GameObject parent, string name, Vector2 pos, Vector2 size, Sprite sprite, string text, TMP_FontAsset font, float textSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();

            var tmp = CreateTmp(go, "Text", text, textSize, Color.white, font, TextAlignmentOptions.Center, true);
            StretchFull(tmp.rectTransform);
            return btn;
        }

        private static void CreateFrameWithText(GameObject parent, string frameName, string textName, Vector2 pos, Vector2 size, Sprite sprite, string text, TMP_FontAsset font, float textSize, Color textColor)
        {
            var frameGo = new GameObject(frameName);
            frameGo.transform.SetParent(parent.transform, false);
            var rt = frameGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = frameGo.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;

            var tmp = CreateTmp(frameGo, textName, text, textSize, textColor, font, TextAlignmentOptions.Center, true);
            StretchFull(tmp.rectTransform);
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
#endif
