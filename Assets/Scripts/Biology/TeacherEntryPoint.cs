using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.Biology
{
    /// <summary>
    /// Nút quay lại Trung tâm giảng viên trên ChapterList / LessonList.
    /// Dùng Canvas overlay riêng để không bị banner/popup của scene che.
    /// </summary>
    public class TeacherEntryPoint : MonoBehaviour
    {
        public const string TeacherSceneName = TeacherNavigation.TeacherScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeOnLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != TeacherNavigation.ChapterListScene &&
                scene.name != TeacherNavigation.LessonListScene)
            {
                return;
            }

            if (!LoginManager.IsCurrentTeacher) return;
            if (FindFirstObjectByType<TeacherEntryPoint>() != null) return;

            GameObject go = new GameObject("TeacherEntryPoint_Auto");
            go.AddComponent<TeacherEntryPoint>();
        }

        private void Start()
        {
            if (!LoginManager.IsCurrentTeacher ||
                !Application.CanStreamedLevelBeLoaded(TeacherSceneName))
            {
                Destroy(gameObject);
                return;
            }

            if (GameObject.Find("Btn_TeacherEntry") != null) return;

            Canvas canvas = CreateOverlayCanvas();
            CreateNavButton(canvas.transform, "Btn_TeacherEntry", "← TRUNG TÂM GV",
                new Vector2(1f, 1f), new Vector2(-20f, -18f),
                () => TeacherNavigation.GoToTeacherCenter());

            CreateNavButton(canvas.transform, "Btn_TeacherToSubjects", "← CHỌN MÔN",
                new Vector2(1f, 1f), new Vector2(-20f, -74f),
                () => TeacherNavigation.GoToSubjectSelect());
        }

        private static Canvas CreateOverlayCanvas()
        {
            GameObject existing = GameObject.Find(TeacherNavigation.OverlayCanvasName);
            if (existing != null)
            {
                Canvas found = existing.GetComponent<Canvas>();
                if (found != null) return found;
            }

            GameObject canvasObj = new GameObject(TeacherNavigation.OverlayCanvasName, typeof(RectTransform), typeof(Canvas));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 250;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateNavButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(210f, 46f);

            Image img = btnObj.AddComponent<Image>();
            img.color = name.Contains("Subjects")
                ? new Color(0.70f, 0.24f, 0.20f, 0.95f)
                : new Color(0.16f, 0.62f, 0.35f, 0.95f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8f, 0f);
            textRt.offsetMax = new Vector2(-8f, 0f);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 12f;
            tmp.fontSizeMax = 16f;
        }
    }
}
