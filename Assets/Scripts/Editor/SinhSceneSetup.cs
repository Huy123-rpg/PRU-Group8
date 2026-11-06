#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

/// <summary>
/// SinhSceneSetup - Editor helper "Sinh > Setup Runner Scene (1-click)".
///
/// Chạy 1 lần trong Unity Editor để tự động cấu hình scene SinhScene:
///   1. Gắn RunnerWiring lên object "Canvas" (script runtime tự wire lần 2).
///   2. Xóa object "MapManager" (hệ thống chunk cũ) + xóa component "AutoScroll" trên Main Camera.
///   3. Xóa 7 object "chuongngaivat", 1 object "hoimau", 6 object "boss*" đặt sẵn.
///   4. Gắn PlayerMovementUI + PlayerCollision lên "nv" + bật trigger collider.
///   5. Tạo object "RunnerLayer" (RectTransform con Canvas) + "Runner" (con Canvas, gắn RunnerController).
///   6. Tạo object "BgScroll" (RectTransform full canvas) + 2 UI Image "bgA"/"bgB" (dùng sprite "bg" hiện có).
///   7. Lưu scene (không auto, để user kiểm tra trước).
///
/// GỌI TỪ MENU: "Sinh" -> "Setup Runner Scene (1-click)"
/// </summary>
public static class SinhSceneSetup
{
    [MenuItem("Sinh/Setup Runner Scene (1-click)")]
    public static void RunSetup()
    {
        // Chỉ chạy trong scene SinhScene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "SinhScene")
        {
            EditorUtility.DisplayDialog("SinhSceneSetup",
                "Scene hiện tại là '" + currentScene + "'.\nVui lòng mở scene 'SinhScene' rồi chạy lại menu.",
                "OK");
            return;
        }

        // 1. Gắn RunnerWiring lên Canvas
        Transform canvasT = FindCanvas();
        if (canvasT == null)
        {
            EditorUtility.DisplayDialog("SinhSceneSetup", "Không tìm thấy object 'Canvas'.", "OK");
            return;
        }
        if (canvasT.GetComponent<RunnerWiring>() == null)
        {
            canvasT.gameObject.AddComponent<RunnerWiring>();
            Debug.Log("[SinhSceneSetup] Đã gắn RunnerWiring lên Canvas.");
        }

        // 2. Xóa MapManager
        GameObject mapMgr = FindObjectByName("MapManager");
        if (mapMgr != null)
        {
            DestroyImmediate(mapMgr);
            Debug.Log("[SinhSceneSetup] Đã xóa MapManager (cũ).");
        }

        // 3. Xóa AutoScroll trên Main Camera
        Camera mainCam = FindMainCamera();
        if (mainCam != null)
        {
            var autoScroll = mainCam.GetComponent<AutoScroll>();
            if (autoScroll != null)
            {
                DestroyImmediate(autoScroll);
                Debug.Log("[SinhSceneSetup] Đã xóa AutoScroll khỏi Main Camera.");
            }
        }

        // 4. Xóa instance cũ đặt sẵn và UI thừa tự sinh
        string[] toRemove =
        {
            "chuongngaivat", "chuongngaivat (1)", "chuongngaivat (2)", "chuongngaivat (3)",
            "chuongngaivat (4)", "chuongngaivat (5)", "chuongngaivat (6)",
            "hoimau",
            "boss", "boss (2)", "boss (4)", "boss (6)", "boss (8)", "boss (10)",
            "Btn_BackToLessonList", "ScoreHUD_Sinh"
        };
        int removed = 0;
        foreach (string n in toRemove)
        {
            GameObject found = FindObjectByName(n);
            if (found != null && found.transform.parent == canvasT) // chỉ xóa con trực tiếp Canvas (tránh nhầm)
            {
                DestroyImmediate(found);
                removed++;
            }
        }
        Debug.Log($"[SinhSceneSetup] Đã xóa {removed} object instance cũ (chuongngaivat/hoimau/boss).");

        // 5. "nv": gắn PlayerMovementUI + PlayerCollision + bật trigger collider
        Transform nv = FindChildRecursive(canvasT, "nv");
        if (nv != null)
        {
            if (nv.GetComponent<PlayerMovementUI>() == null) nv.gameObject.AddComponent<PlayerMovementUI>();
            if (nv.GetComponent<PlayerCollision>() == null) nv.gameObject.AddComponent<PlayerCollision>();
            var bc = nv.GetComponent<UnityEngine.BoxCollider2D>();
            if (bc != null) bc.isTrigger = true;
            Debug.Log("[SinhSceneSetup] Đã gắn PlayerMovementUI + PlayerCollision lên 'nv'.");
        }

        // 6. Tạo RunnerLayer + Runner (con Canvas)
        if (FindChildRecursive(canvasT, "RunnerLayer") == null)
        {
            GameObject rl = new GameObject("RunnerLayer");
            RectTransform rlt = rl.AddComponent<RectTransform>();
            rlt.SetParent(canvasT, false);
            rlt.anchorMin = rlt.anchorMax = new Vector2(0.5f, 0.5f);
            rlt.anchoredPosition = Vector2.zero;
            rlt.sizeDelta = Vector2.zero;
        }
        if (FindChildRecursive(canvasT, "Runner") == null)
        {
            GameObject runner = new GameObject("Runner");
            runner.transform.SetParent(canvasT, false);
            runner.AddComponent<RunnerController>();
        }
        Debug.Log("[SinhSceneSetup] Đã tạo RunnerLayer + Runner (RunnerController).");

        // 7. Tạo BgScroll + 2 image (dùng sprite "bg" hiện có)
        CreateBgScrollStructure(canvasT);

        // Lưu scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SinhSceneSetup] ✅ Hoàn tất setup. Hãy lưu scene rồi vào Play Mode kiểm tra.");
    }

    // ============ Helpers ============

    static Transform FindCanvas()
    {
        GameObject go = GameObject.Find("Canvas");
        return go != null ? go.transform : null;
    }

    static Camera FindMainCamera()
    {
        GameObject go = GameObject.Find("Main Camera");
        return go != null ? go.GetComponent<Camera>() : null;
    }

    static Transform FindChildRecursive(Transform root, string name)
    {
        if (root == null) return null;
        foreach (Transform c in root)
        {
            if (c.name == name) return c;
            Transform r = FindChildRecursive(c, name);
            if (r != null) return r;
        }
        return null;
    }

    static GameObject FindObjectByName(string name)
    {
        // Tìm object active + inactive trong scene hiện tại
        UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (GameObject go in s.GetRootGameObjects())
        {
            if (go.name == name) return go;
            // cũng tìm trong hierarchy
            var found = FindInHierarchy(go.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    static GameObject FindInHierarchy(Transform root, string name)
    {
        if (root.name == name) return root.gameObject;
        foreach (Transform c in root)
        {
            var r = FindInHierarchy(c, name);
            if (r != null) return r;
        }
        return null;
    }

    static void CreateBgScrollStructure(Transform canvasT)
    {
        if (FindChildRecursive(canvasT, "BgScroll") != null) return; // đã có

        // Lấy sprite "bg" hiện có (1 object "bg" con Canvas đầu tiên)
        Sprite bgSprite = null;
        GameObject existingBg = FindObjectByName("bg");
        if (existingBg != null)
        {
            Image img = existingBg.GetComponent<Image>();
            if (img != null) bgSprite = img.sprite;
        }

        // BgScroll (RectTransform, full canvas)
        GameObject bgScroll = new GameObject("BgScroll");
        RectTransform brt = bgScroll.AddComponent<RectTransform>();
        brt.SetParent(canvasT, false);
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.anchoredPosition = Vector2.zero;
        brt.sizeDelta = Vector2.zero;
        brt.SetAsFirstSibling(); // đứng sau UI khác

        // bgA + bgB (2 image nối tiếp, dùng cùng sprite)
        CreateBgImage(bgScroll.transform, "bgA", bgSprite);
        CreateBgImage(bgScroll.transform, "bgB", bgSprite);

        // Gắn BackgroundScroller (UI-based)
        BackgroundScroller scroller = bgScroll.AddComponent<BackgroundScroller>();
        scroller.bgImageA = bgScroll.transform.Find("bgA") as RectTransform;
        scroller.bgImageB = bgScroll.transform.Find("bgB") as RectTransform;
        scroller.scrollSpeed = 120f;

        // Xóa object "bg" cũ (đơn lẻ, con Canvas) - nền mới là 2 image
        if (existingBg != null && existingBg.transform.parent == canvasT)
        {
            DestroyImmediate(existingBg);
        }

        Debug.Log("[SinhSceneSetup] Đã tạo BgScroll + bgA + bgB (BackgroundScroller UI).");
    }

    static void CreateBgImage(Transform parent, string name, Sprite sprite)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Canvas cv = parent.GetComponentInParent<Canvas>();
        float h = (cv != null ? cv.GetComponent<RectTransform>().rect.height : 1080f);
        rt.sizeDelta = new Vector2(0f, h);
        rt.localScale = Vector3.one;

        // A ở 0 (chiếm trọn màn hình), B ở h (nối tiếp ngay trên đỉnh A)
        rt.anchoredPosition = name == "bgA" ? new Vector2(0, 0f) : new Vector2(0, h);

        Image img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        else img.color = new Color(0.55f, 0.45f, 0.3f, 1f); // dự phòng
        img.preserveAspect = false;
        img.raycastTarget = false;
    }
}
#endif
