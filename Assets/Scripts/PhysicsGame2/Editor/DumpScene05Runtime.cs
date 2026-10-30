using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    public static class DumpScene05Runtime
    {
        [InitializeOnLoadMethod]
        [MenuItem("Aurelia/Dump Scene 05 Hierarchy")]
        public static void Dump()
        {
            string scenePath = "Assets/Scenes/Physics/Game2/05_LessonSelection.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Pre-warm session
            if (PhysicsGameSession.Instance == null)
            {
                var sGo = new GameObject("Session_Test");
                sGo.AddComponent<PhysicsGameSession>();
            }
            PhysicsGameSession.Instance.SelectChapter(3);

            // Ensure controller
            var canvas = Object.FindAnyObjectByType<Canvas>();
            var controller = canvas.GetComponent<LessonSelectionController>() ?? canvas.gameObject.AddComponent<LessonSelectionController>();

            // Invoke Awake and Start
            var awake = typeof(LessonSelectionController).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (awake != null) awake.Invoke(controller, null);

            var start = typeof(LessonSelectionController).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (start != null) start.Invoke(controller, null);

            Debug.Log("====================== DUMP SCENE 05 OBJECTS ======================");
            var allTransforms = canvas.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                var rt = t as RectTransform;
                var img = t.GetComponent<Image>();
                var tmp = t.GetComponent<TextMeshProUGUI>();
                string imgInfo = "";
                if (img != null)
                {
                    string spName = img.sprite != null ? img.sprite.name : "null_sprite";
                    imgInfo = $" | Image(sprite={spName}, color={img.color}, type={img.type})";
                }
                string tmpInfo = tmp != null ? $" | TMP(text='{tmp.text}', color={tmp.color})" : "";
                string rtInfo = rt != null ? $" pos=({rt.anchoredPosition.x:F1}, {rt.anchoredPosition.y:F1}), size=({rt.rect.width:F1}, {rt.rect.height:F1}), scale=({rt.localScale.x:F2}, {rt.localScale.y:F2})" : "";
                Debug.Log($"[{t.gameObject.name}] active={t.gameObject.activeSelf}{rtInfo}{imgInfo}{tmpInfo}");
            }
            Debug.Log("===================================================================");
        }
    }
}
