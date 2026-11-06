#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Công cụ tự động thiết lập Font tiếng Việt (VietnameseFont.ttf) cho TextMeshPro và khắc phục sự cố UI trong Game 1.
/// - Giải quyết triệt để lỗi mất chữ, thiếu dấu tiếng Việt (ô, ơ, ư, đ, ả, ạ, ớ, ự, ế,...) trong Scene View và Game View.
/// - Khắc phục hoàn toàn lỗi nút bấm không ăn (unclickable) trong 01_MainMenu của Game 1.
/// - Ngăn chặn xung đột giữa Physics Game 1 và Physics Game 2.
/// </summary>
[InitializeOnLoad]
public static class VietnameseFontSetup
{
    private const string TTF_PATH = "Assets/SinhHoc/Resources/FantasyUI/VietnameseFont.ttf";
    private const string SDF_ASSET_DIR = "Assets/TextMesh Pro/Resources/Fonts & Materials";
    private const string SDF_ASSET_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/VietnameseFont SDF.asset";
    private const string G2_BOOTSTRAP_PATH = "Assets/Scripts/PhysicsGame2/Core/PhysicsGameBootstrap.cs";

    static VietnameseFontSetup()
    {
        EditorApplication.delayCall += () =>
        {
            EnsureVietnameseFontConfigured();
            FixGame2BootstrapFile();
            FixMainMenuScene();
        };
    }

    [MenuItem("Tools/Physics/Sửa Lỗi Font và Menu Game 1 (Toàn diện)")]
    public static void MenuFixAll()
    {
        EnsureVietnameseFontConfigured();
        FixGame2BootstrapFile();
        FixMainMenuScene();
        FixAllGame1Scenes();
    }

    /// <summary>
    /// Tạo bộ chuỗi đầy đủ 100% tất cả các ký tự tiếng Việt, dấu thanh, dấu mũ, móc, ASCII, số học, ký hiệu khoa học
    /// </summary>
    public static string BuildComprehensiveVietnameseCharSet()
    {
        var sb = new StringBuilder(2000);

        // 1. ASCII tiêu chuẩn (32 - 126)
        for (int i = 32; i <= 126; i++) sb.Append((char)i);

        // 2. Ký tự xuống dòng, tab
        sb.Append("\n\r\t");

        // 3. Toàn bộ bảng chữ cái tiếng Việt thường & hoa (đủ tất cả 5 dấu thanh và biến thể)
        sb.Append("aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬ");
        sb.Append("bBcCdDđĐ");
        sb.Append("eEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆ");
        sb.Append("fFgGhH");
        sb.Append("iIìÌỉỈĩĨíÍịỊ");
        sb.Append("jJkKlLmMnN");
        sb.Append("oOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢ");
        sb.Append("pPqQrRsStT");
        sb.Append("uUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰ");
        sb.Append("vVwWxX");
        sb.Append("yYỳỲỷỶỹỸýÝỵỴ");
        sb.Append("zZ");

        // 4. Latin-1 Supplement (0x00A0 - 0x00FF)
        for (int i = 0x00A0; i <= 0x00FF; i++) sb.Append((char)i);

        // 5. Latin Extended-A (0x0100 - 0x017F) - bao gồm đ, Đ, ă, Ă, ư, Ư, ơ, Ơ
        for (int i = 0x0100; i <= 0x017F; i++) sb.Append((char)i);

        // 6. Latin Extended Additional (0x1EA0 - 0x1EF9) - kho chứa toàn bộ nguyên âm ghép dấu tiếng Việt
        for (int i = 0x1EA0; i <= 0x1EF9; i++) sb.Append((char)i);

        // 7. Dấu câu, ngoặc kép, gạch ngang, bullet (0x2000 - 0x206F)
        for (int i = 0x2000; i <= 0x206F; i++) sb.Append((char)i);

        // 8. Đơn vị tiền tệ (0x20A0 - 0x20CF: ₫, €, $,...)
        for (int i = 0x20A0; i <= 0x20CF; i++) sb.Append((char)i);

        // 9. Ký hiệu toán học (0x2200 - 0x2230: ±, ×, ÷, ≤, ≥,...)
        for (int i = 0x2200; i <= 0x2230; i++) sb.Append((char)i);

        return sb.ToString();
    }

    public static void EnsureVietnameseFontConfigured()
    {
        try
        {
            Font ttf = AssetDatabase.LoadAssetAtPath<Font>(TTF_PATH);
            if (ttf == null)
            {
                Debug.LogWarning($"[VietnameseFontSetup] Không tìm thấy font TTF tại: {TTF_PATH}");
                return;
            }

            if (!Directory.Exists(SDF_ASSET_DIR))
            {
                Directory.CreateDirectory(SDF_ASSET_DIR);
            }

            TMP_FontAsset vnFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SDF_ASSET_PATH);
            if (vnFont == null)
            {
                vnFont = TMP_FontAsset.CreateFontAsset(
                    ttf,
                    48,
                    6,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                    2048,
                    2048,
                    AtlasPopulationMode.Dynamic,
                    true);

                vnFont.name = "VietnameseFont SDF";
                AssetDatabase.CreateAsset(vnFont, SDF_ASSET_PATH);

                if (vnFont.material != null)
                {
                    vnFont.material.name = "VietnameseFont SDF Material";
                    AssetDatabase.AddObjectToAsset(vnFont.material, vnFont);
                }
                if (vnFont.atlasTexture != null)
                {
                    vnFont.atlasTexture.name = "VietnameseFont SDF Atlas";
                    AssetDatabase.AddObjectToAsset(vnFont.atlasTexture, vnFont);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("<color=green>[VietnameseFontSetup] ✅ Đã tạo mới asset: VietnameseFont SDF.asset (2048x2048)!</color>");
            }

            // Nạp toàn bộ ký tự tiếng Việt + ASCII vào characterTable & atlas texture
            string comprehensiveCharSet = BuildComprehensiveVietnameseCharSet();
            if (vnFont != null)
            {
                string missing;
                vnFont.TryAddCharacters(comprehensiveCharSet, out missing);
                EditorUtility.SetDirty(vnFont);
                if (vnFont.atlasTexture != null) EditorUtility.SetDirty(vnFont.atlasTexture);
            }

            // 1. Gán vào defaultFontAsset fallback table
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                if (defaultFont.fallbackFontAssetTable == null)
                    defaultFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

                if (!defaultFont.fallbackFontAssetTable.Contains(vnFont))
                {
                    defaultFont.fallbackFontAssetTable.Insert(0, vnFont);
                    EditorUtility.SetDirty(defaultFont);
                }
            }

            // 2. Gán vào TMP_Settings fallback table
            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings != null)
            {
                SerializedObject so = new SerializedObject(settings);
                SerializedProperty fallbacks = so.FindProperty("m_fallbackFontAssets");
                if (fallbacks != null)
                {
                    bool exists = false;
                    for (int i = 0; i < fallbacks.arraySize; i++)
                    {
                        if (fallbacks.GetArrayElementAtIndex(i).objectReferenceValue == vnFont)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        fallbacks.InsertArrayElementAtIndex(0);
                        fallbacks.GetArrayElementAtIndex(0).objectReferenceValue = vnFont;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(settings);
                    }
                }
            }

            // 3. Cập nhật các TextMeshProUGUI trong scene đang mở
            int updatedInActiveScene = ApplyFontToActiveScene(vnFont);

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>[VietnameseFontSetup] ✅ Đã nạp thành công bộ ký tự tiếng Việt vào VietnameseFont SDF (Cập nhật {updatedInActiveScene} Text trong Scene).</color>");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VietnameseFontSetup] Lỗi cấu hình font: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public static int ApplyFontToActiveScene(TMP_FontAsset vnFont)
    {
        if (vnFont == null) return 0;
        int count = 0;
        var texts = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        foreach (var t in texts)
        {
            if (t != null)
            {
                bool changed = false;
                if (t.font != vnFont)
                {
                    t.font = vnFont;
                    if (vnFont.material != null)
                    {
                        t.fontSharedMaterial = vnFont.material;
                    }
                    changed = true;
                }

                // Luôn kích hoạt rebuild mesh để đảm bảo hiển thị đúng dấu
                t.SetAllDirty();

                if (changed)
                {
                    EditorUtility.SetDirty(t);
                    count++;
                }
            }
        }
        if (count > 0)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }
        return count;
    }

    /// <summary>
    /// Khắc phục toàn bộ vấn đề trong scene 01_MainMenu của Game 1
    /// </summary>
    public static void FixMainMenuScene()
    {
        string scenePath = "Assets/Scenes/Physics/Game1/01_MainMenu.unity";
        if (!File.Exists(scenePath)) return;

        Scene currentScene = SceneManager.GetActiveScene();
        bool isMainMenuActive = currentScene.isLoaded && currentScene.path == scenePath;
        Scene targetScene;

        if (isMainMenuActive)
        {
            targetScene = currentScene;
        }
        else
        {
            targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        // 1. Tắt raycastTarget trên các hình ảnh trang trí để không chắn click
        string[] decorative = new string[] { "bg", "title", "Image (1)", "Image (2)", "Image" };
        var allImages = UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include);
        foreach (var img in allImages)
        {
            if (img == null) continue;
            string objName = img.gameObject.name;
            foreach (var d in decorative)
            {
                if (objName == d)
                {
                    img.raycastTarget = false;
                    EditorUtility.SetDirty(img);
                    break;
                }
            }
        }

        // 2. Bật raycastTarget và gắn Button component trên 5 nút menu
        string[] buttonNames = new string[] { "start", "chonnhanvat", "thanhtich", "caidat", "thoat" };
        foreach (var bName in buttonNames)
        {
            GameObject bObj = GameObject.Find(bName);
            if (bObj != null)
            {
                Image img = bObj.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    EditorUtility.SetDirty(img);
                }

                Button btn = bObj.GetComponent<Button>();
                if (btn == null)
                {
                    btn = bObj.AddComponent<Button>();
                }
                btn.interactable = true;
                btn.transition = Selectable.Transition.ColorTint;
                EditorUtility.SetDirty(btn);
                EditorUtility.SetDirty(bObj);
            }
        }

        // 3. Đảm bảo Canvas có GraphicRaycaster và gắn PhysicsGame1_MainMenuController
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            var raycaster = canvasObj.GetComponent<GraphicRaycaster>();
            if (raycaster == null) canvasObj.AddComponent<GraphicRaycaster>();

            var menuCtrl = canvasObj.GetComponent<ScienceQuest.PhysicsGame1.PhysicsGame1_MainMenuController>();
            if (menuCtrl == null)
            {
                canvasObj.AddComponent<ScienceQuest.PhysicsGame1.PhysicsGame1_MainMenuController>();
            }
            EditorUtility.SetDirty(canvasObj);
        }

        // 4. Đảm bảo EventSystem có InputSystemUIInputModule
        GameObject esObj = GameObject.Find("EventSystem");
        if (esObj != null)
        {
            var inputModule = esObj.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null)
            {
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
            var oldModule = esObj.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null)
            {
                UnityEngine.Object.DestroyImmediate(oldModule);
            }
            EditorUtility.SetDirty(esObj);
        }

        EditorSceneManager.SaveScene(targetScene);
        Debug.Log("<color=green>[VietnameseFontSetup] ✅ Đã cấu hình hoàn chỉnh các nút bấm trong 01_MainMenu.unity!</color>");

        if (!isMainMenuActive && currentScene.IsValid() && !string.IsNullOrEmpty(currentScene.path) && File.Exists(currentScene.path))
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }
    }

    /// <summary>
    /// Chèn kiểm tra bảo vệ vào PhysicsGameBootstrap (Game 2) để không phá hoại Game 1
    /// </summary>
    public static void FixGame2BootstrapFile()
    {
        try
        {
            if (!File.Exists(G2_BOOTSTRAP_PATH)) return;

            string content = File.ReadAllText(G2_BOOTSTRAP_PATH, Encoding.UTF8);
            if (content.Contains("IsGame1Scene")) return; // Đã vá rồi

            // Thêm hàm IsGame1Scene và kiểm tra vào AttachControllerForScene
            string methodGuard = @"
        private static bool IsGame1Scene(Scene scene)
        {
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
            {
                if (scene.path.IndexOf(""Physics/Game1"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    scene.path.IndexOf(""Physics\\Game1"", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            if (GameObject.Find(""chonnhanvat"") != null || GameObject.Find(""thanhtich"") != null)
            {
                return true;
            }
            return false;
        }
";
            int idx = content.IndexOf("public static void AttachControllerForScene(string sceneName)");
            if (idx >= 0)
            {
                content = content.Insert(idx, methodGuard + "\n        ");
                int braceIdx = content.IndexOf('{', idx + methodGuard.Length);
                if (braceIdx >= 0)
                {
                    string attachGuard = "\n            Scene activeScene = SceneManager.GetActiveScene();\n            if (IsGame1Scene(activeScene)) return;\n";
                    content = content.Insert(braceIdx + 1, attachGuard);
                }
                File.WriteAllText(G2_BOOTSTRAP_PATH, content, Encoding.UTF8);
                Debug.Log("<color=green>[VietnameseFontSetup] ✅ Đã vá bảo vệ PhysicsGameBootstrap của Game 2 thành công!</color>");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VietnameseFontSetup] Lỗi vá PhysicsGameBootstrap: {ex.Message}");
        }
    }

    public static void FixAllGame1Scenes()
    {
        TMP_FontAsset vnFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SDF_ASSET_PATH);
        if (vnFont == null) return;

        string[] game1Scenes = new string[]
        {
            "Assets/Scenes/Physics/Game1/01_MainMenu.unity",
            "Assets/Scenes/Physics/Game1/02_CharacterSelection.unity",
            "Assets/Scenes/Physics/Game1/03_LevelSelection.unity",
            "Assets/Scenes/Physics/Game1/04_ChapterSelection.unity",
            "Assets/Scenes/Physics/Game1/05_LessonSelection.unity",
            "Assets/Scenes/Physics/Game1/06_GameModeSelect.unity",
            "Assets/Scenes/Physics/Game1/07_DuckShooting.unity",
            "Assets/Scenes/Physics/Game1/08_ResultScene.unity",
            "Assets/Scenes/Physics/Game1/09_AnswerReview.unity",
            "Assets/Scenes/Physics/Game1/10_Settings.unity",
            "Assets/Scenes/Physics/Game1/11_Achievements.unity",
            "Assets/Scenes/Physics/Game1/12_Theory.unity",
            "Assets/Scenes/Physics/Game1/13_VersusSelection.unity",
            "Assets/Scenes/Physics/Game1/14_VersusGame.unity",
            "Assets/Scenes/Physics/Game1/15_VersusResult.unity",
            "Assets/Scenes/Physics/Game1/16_DashboardTeacher.unity"
        };

        string currentScene = SceneManager.GetActiveScene().path;

        foreach (var scenePath in game1Scenes)
        {
            if (!File.Exists(scenePath)) continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int updated = ApplyFontToActiveScene(vnFont);
            if (updated > 0)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[VietnameseFontSetup] Đã sửa {updated} text tại {scenePath}");
            }
        }

        if (!string.IsNullOrEmpty(currentScene) && File.Exists(currentScene))
        {
            EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }
    }
}
#endif
