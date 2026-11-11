#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    /// <summary>
    /// Tự động chuẩn hóa và đồng bộ hóa Popup câu hỏi Đối kháng (VersusQuestionFantasyPopup)
    /// cho cả Prefab (Resources/UI) và Scene (34.1. Đối kháng 1.unity).
    /// Khắc phục toàn bộ 7 lỗi được báo cáo từ người dùng:
    /// - Câu hỏi to rõ (24pt), cân đối khung.
    /// - 4 Option A/B/C/D phân tách riêng chữ cái trong oval vàng và nội dung đáp án (21pt), không đè viền.
    /// - Nút Xác nhận và Đóng hoạt động 100%.
    /// - Duy nhất 1 nút Đóng ở dưới, chữ ĐÓNG căn giữa hoàn hảo, xóa sạch nút đỏ góc trên.
    /// - Highlight rõ nét khi chọn đáp án (đổi màu nền + viền vàng rực 4px).
    /// - Popup luôn là Last Sibling trong Canvas để nhận toàn bộ raycast.
    /// </summary>
    [InitializeOnLoad]
    public static class VersusPopupSyncBuilder
    {
        public const string PREFAB_PATH = "Assets/Resources/UI/VersusQuestionFantasyPopup.prefab";
        public const string SCENE_ARENA_PATH = "Assets/Scenes/Physics/Game2/34.1. Đối kháng 1.unity";

        static VersusPopupSyncBuilder()
        {
            // Chỉ chạy khi người dùng chọn MenuItem, không tự động ghi đè thiết kế của người dùng
        }

        [MenuItem("Aurelia/Sync Versus Question Popup (Prefab & Scene)")]
        public static void SyncPrefabAndScene()
        {
            SyncPrefab();
            SyncScene();
        }

        public static void SyncPrefab()
        {
            if (!File.Exists(PREFAB_PATH))
            {
                Debug.LogWarning($"[VersusPopupSyncBuilder] Prefab not found at {PREFAB_PATH}");
                return;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
            if (prefabRoot == null) return;

            try
            {
                ConfigurePopupHierarchy(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_PATH);
                Debug.Log($"[VersusPopupSyncBuilder] Successfully synchronized {PREFAB_PATH}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        public static void SyncScene()
        {
            if (!File.Exists(SCENE_ARENA_PATH)) return;

            bool isCurrentScene = (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path == SCENE_ARENA_PATH);
            var scene = isCurrentScene 
                ? UnityEngine.SceneManagement.SceneManager.GetActiveScene() 
                : EditorSceneManager.OpenScene(SCENE_ARENA_PATH, OpenSceneMode.Additive);

            try
            {
                var rootObjects = scene.GetRootGameObjects();
                GameObject popupGo = null;
                Canvas targetCanvas = null;

                foreach (var root in rootObjects)
                {
                    var c = root.GetComponentInChildren<Canvas>(true);
                    if (c != null && targetCanvas == null) targetCanvas = c;

                    var p = root.transform.Find("VersusQuestionFantasyPopup");
                    if (p != null) { popupGo = p.gameObject; break; }

                    if (root.name == "VersusQuestionFantasyPopup") { popupGo = root; break; }

                    var allPopups = root.GetComponentsInChildren<VersusQuestionModalController>(true);
                    if (allPopups.Length > 0)
                    {
                        popupGo = allPopups[0].gameObject;
                        break;
                    }
                }

                if (popupGo != null)
                {
                    ConfigurePopupHierarchy(popupGo);
                    var modal = popupGo.GetComponent<VersusQuestionModalController>();
                    if (modal != null)
                    {
                        modal.EnforcePopupHierarchyOrder();
                    }
                    else
                    {
                        popupGo.transform.SetAsLastSibling();
                    }

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[VersusPopupSyncBuilder] Successfully synchronized popup in Scene {SCENE_ARENA_PATH}");
                }
            }
            finally
            {
                if (!isCurrentScene && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        public static void ConfigurePopupHierarchy(GameObject root)
        {
            // 0. Xóa triệt để nút đỏ CloseX
            var redundantCloseX = root.transform.Find("Btn_CloseX");
            if (redundantCloseX != null)
            {
                Object.DestroyImmediate(redundantCloseX.gameObject);
            }

            // Đảm bảo Root có GraphicRaycaster hoặc RectTransform chuẩn
            var rootRt = root.GetComponent<RectTransform>();
            if (rootRt != null)
            {
                rootRt.anchorMin = Vector2.zero;
                rootRt.anchorMax = Vector2.one;
                rootRt.sizeDelta = Vector2.zero;
                rootRt.anchoredPosition = Vector2.zero;
            }

            var rootImg = root.GetComponent<Image>();
            if (rootImg != null)
            {
                rootImg.raycastTarget = true;
                rootImg.color = new Color(0.02f, 0.05f, 0.12f, 0.85f);
            }

            // 1. PopupFrame
            var frameTrans = root.transform.Find("PopupFrame");
            if (frameTrans != null)
            {
                var frameImg = frameTrans.GetComponent<Image>();
                if (frameImg != null)
                {
                    frameImg.raycastTarget = false; // Không nuốt raycast của các nút con
                }
            }

            // 2. Banner Header & Title
            var titleTrans = root.transform.Find("BannerHeader/TitleText") ?? root.transform.Find("TitleText");
            TextMeshProUGUI txtTitle = null;
            if (titleTrans != null)
            {
                txtTitle = titleTrans.GetComponent<TextMeshProUGUI>();
                if (txtTitle != null)
                {
                    txtTitle.fontSize = 26f;
                    txtTitle.fontStyle = FontStyles.Bold;
                    txtTitle.alignment = TextAlignmentOptions.Center;
                    txtTitle.color = new Color(1f, 0.88f, 0.4f, 1f);
                    txtTitle.raycastTarget = false;
                }
            }

            // 3. QuestionText
            var qTrans = root.transform.Find("QuestionText");
            TextMeshProUGUI txtQuestion = null;
            if (qTrans != null)
            {
                var qRt = qTrans.GetComponent<RectTransform>();
                qRt.anchoredPosition = new Vector2(0f, 160f);
                qRt.sizeDelta = new Vector2(1040f, 110f);

                txtQuestion = qTrans.GetComponent<TextMeshProUGUI>();
                if (txtQuestion != null)
                {
                    txtQuestion.fontSize = 24f;
                    txtQuestion.fontStyle = FontStyles.Bold;
                    txtQuestion.alignment = TextAlignmentOptions.Center;
                    txtQuestion.color = Color.white;
                    txtQuestion.raycastTarget = false;
                    txtQuestion.textWrappingMode = TextWrappingModes.Normal;
                }
            }

            // 4. Bốn thanh Option A, B, C, D
            char[] prefixes = new char[] { 'A', 'B', 'C', 'D' };
            float[] yOffsets = new float[] { 92f, 0f, -88f, -175f };

            Button[] optionButtons = new Button[4];
            Image[] optionImages = new Image[4];
            TextMeshProUGUI[] badgeTexts = new TextMeshProUGUI[4];
            TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];
            Outline[] optionOutlines = new Outline[4];

            for (int i = 0; i < 4; i++)
            {
                var optTrans = root.transform.Find($"Option_{prefixes[i]}");
                if (optTrans == null) continue;

                var oRt = optTrans.GetComponent<RectTransform>();
                oRt.anchoredPosition = new Vector2(0f, yOffsets[i]);
                oRt.sizeDelta = new Vector2(885.07f, 99.09f);

                var oImg = optTrans.GetComponent<Image>();
                if (oImg != null)
                {
                    oImg.raycastTarget = true;
                    optionImages[i] = oImg;
                }

                var oBtn = optTrans.GetComponent<Button>() ?? optTrans.gameObject.AddComponent<Button>();
                oBtn.targetGraphic = oImg;
                oBtn.interactable = true;
                optionButtons[i] = oBtn;

                var oOutline = optTrans.GetComponent<Outline>() ?? optTrans.gameObject.AddComponent<Outline>();
                oOutline.enabled = false;
                oOutline.effectColor = new Color(1f, 0.92f, 0.35f, 1f);
                oOutline.effectDistance = new Vector2(4f, -4f);
                optionOutlines[i] = oOutline;

                // Badge (chữ cái trong oval vàng)
                var bTrans = optTrans.Find($"Badge_{prefixes[i]}");
                if (bTrans != null)
                {
                    var bRt = bTrans.GetComponent<RectTransform>();
                    bRt.anchoredPosition = new Vector2(-382.5f, 0f);
                    bRt.sizeDelta = new Vector2(50f, 50f);

                    var bTmp = bTrans.GetComponent<TextMeshProUGUI>();
                    if (bTmp != null)
                    {
                        bTmp.text = prefixes[i].ToString();
                        bTmp.fontSize = 26f;
                        bTmp.fontStyle = FontStyles.Bold;
                        bTmp.alignment = TextAlignmentOptions.Center;
                        bTmp.color = new Color(1f, 0.85f, 0.35f, 1f);
                        bTmp.raycastTarget = false;
                        badgeTexts[i] = bTmp;
                    }
                }

                // Content (nội dung câu hỏi)
                var cTrans = optTrans.Find($"Content_{prefixes[i]}");
                if (cTrans != null)
                {
                    var cRt = cTrans.GetComponent<RectTransform>();
                    // Dịch sang phải (+60, size 740 -> mép trái -310) để tuyệt đối không chạm viền oval vàng
                    cRt.anchoredPosition = new Vector2(60f, 0f);
                    cRt.sizeDelta = new Vector2(740f, 75f);

                    var cTmp = cTrans.GetComponent<TextMeshProUGUI>();
                    if (cTmp != null)
                    {
                        cTmp.fontSize = 21f;
                        cTmp.fontStyle = FontStyles.Normal;
                        cTmp.alignment = TextAlignmentOptions.MidlineLeft;
                        cTmp.color = Color.white;
                        cTmp.raycastTarget = false;
                        cTmp.textWrappingMode = TextWrappingModes.Normal;
                        optionTexts[i] = cTmp;
                    }
                }
            }

            // 5. FeedbackText
            var fbTrans = root.transform.Find("FeedbackText");
            TextMeshProUGUI txtFeedback = null;
            if (fbTrans != null)
            {
                var fbRt = fbTrans.GetComponent<RectTransform>();
                fbRt.anchoredPosition = new Vector2(0f, -235f);
                fbRt.sizeDelta = new Vector2(700f, 40f);

                txtFeedback = fbTrans.GetComponent<TextMeshProUGUI>();
                if (txtFeedback != null)
                {
                    txtFeedback.fontSize = 18f;
                    txtFeedback.fontStyle = FontStyles.Bold;
                    txtFeedback.alignment = TextAlignmentOptions.Center;
                    txtFeedback.color = new Color(1f, 0.9f, 0.3f, 1f);
                    txtFeedback.raycastTarget = false;
                }
            }

            // 6. Nút Đóng (Btn_Close)
            var closeTrans = root.transform.Find("Btn_Close");
            Button btnClose = null;
            TextMeshProUGUI txtClose = null;
            if (closeTrans != null)
            {
                var cRt = closeTrans.GetComponent<RectTransform>();
                cRt.anchoredPosition = new Vector2(-255.71f, -308.02f);
                cRt.sizeDelta = new Vector2(260f, 118.44f);

                var cImg = closeTrans.GetComponent<Image>();
                if (cImg != null) cImg.raycastTarget = true;

                btnClose = closeTrans.GetComponent<Button>() ?? closeTrans.gameObject.AddComponent<Button>();
                btnClose.targetGraphic = cImg;
                btnClose.interactable = true;

                var ctTrans = closeTrans.Find("Text");
                if (ctTrans != null)
                {
                    var ctRt = ctTrans.GetComponent<RectTransform>();
                    ctRt.anchorMin = new Vector2(0.5f, 0.5f);
                    ctRt.anchorMax = new Vector2(0.5f, 0.5f);
                    ctRt.pivot = new Vector2(0.5f, 0.5f);
                    ctRt.anchoredPosition = new Vector2(0f, 6f); // Căn chính giữa lòng khung
                    ctRt.sizeDelta = new Vector2(200f, 44f);

                    txtClose = ctTrans.GetComponent<TextMeshProUGUI>();
                    if (txtClose != null)
                    {
                        txtClose.text = "ĐÓNG";
                        txtClose.fontSize = 20f;
                        txtClose.fontStyle = FontStyles.Bold;
                        txtClose.alignment = TextAlignmentOptions.Center;
                        txtClose.color = Color.white;
                        txtClose.raycastTarget = false;
                    }
                }
            }

            // 7. Nút Xác nhận (Btn_Confirm)
            var confirmTrans = root.transform.Find("Btn_Confirm");
            Button btnConfirm = null;
            if (confirmTrans != null)
            {
                var cfRt = confirmTrans.GetComponent<RectTransform>();
                cfRt.anchoredPosition = new Vector2(255.71f, -308.02f);
                cfRt.sizeDelta = new Vector2(281.14f, 118.44f);

                var cfImg = confirmTrans.GetComponent<Image>();
                if (cfImg != null) cfImg.raycastTarget = true;

                btnConfirm = confirmTrans.GetComponent<Button>() ?? confirmTrans.gameObject.AddComponent<Button>();
                btnConfirm.targetGraphic = cfImg;
                btnConfirm.interactable = true;
            }

            // 8. Đồng bộ component Controller
            var controller = root.GetComponent<VersusQuestionModalController>() ?? root.AddComponent<VersusQuestionModalController>();
            var so = new SerializedObject(controller);
            so.Update();

            if (txtTitle != null) so.FindProperty("txtTitle").objectReferenceValue = txtTitle;
            if (txtQuestion != null) so.FindProperty("txtQuestion").objectReferenceValue = txtQuestion;
            if (btnConfirm != null) so.FindProperty("btnConfirm").objectReferenceValue = btnConfirm;
            if (btnClose != null) so.FindProperty("btnClose").objectReferenceValue = btnClose;
            if (txtClose != null) so.FindProperty("txtCloseButton").objectReferenceValue = txtClose;
            if (txtFeedback != null) so.FindProperty("txtFeedback").objectReferenceValue = txtFeedback;

            var optBtnsProp = so.FindProperty("optionButtons");
            var optImgsProp = so.FindProperty("optionImages");
            var badgeTextsProp = so.FindProperty("badgeTexts");
            var optTextsProp = so.FindProperty("optionTexts");
            var optOutlinesProp = so.FindProperty("optionOutlines");

            optBtnsProp.arraySize = 4;
            optImgsProp.arraySize = 4;
            badgeTextsProp.arraySize = 4;
            optTextsProp.arraySize = 4;
            optOutlinesProp.arraySize = 4;

            for (int i = 0; i < 4; i++)
            {
                if (optionButtons[i] != null) optBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionButtons[i];
                if (optionImages[i] != null) optImgsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionImages[i];
                if (badgeTexts[i] != null) badgeTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = badgeTexts[i];
                if (optionTexts[i] != null) optTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionTexts[i];
                if (optionOutlines[i] != null) optOutlinesProp.GetArrayElementAtIndex(i).objectReferenceValue = optionOutlines[i];
            }

            so.ApplyModifiedProperties();
        }
    }
}
#endif
