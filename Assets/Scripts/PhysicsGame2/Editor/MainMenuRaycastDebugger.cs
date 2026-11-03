using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScienceQuest.PhysicsGame2.Tests
{
    public static class MainMenuRaycastDebugger
    {
        [MenuItem("Aurelia Academy/Debug Main Menu Raycasts", priority = 200)]
        public static void DebugRaycasts()
        {
            if (EditorApplication.isPlaying)
            {
                RunRaycastCheck();
            }
            else
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Physics/Game2/01_MainMenu.unity");
                EditorApplication.EnterPlaymode();
            }
        }

        public static void RunRaycastCheck()
        {
            Debug.Log("<color=#00e676>=== MAIN MENU RAYCAST DEBUGGER ===</color>");

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in scene!");
                return;
            }

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError("No GraphicRaycaster on Canvas!");
                return;
            }

            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            Debug.Log($"EventSystem: {(eventSystem != null ? eventSystem.name : "NULL")}, Enabled={eventSystem?.enabled}, CurrentInputModule={eventSystem?.currentInputModule?.GetType().Name}");

            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            // Find all buttons or candidate button images
            var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            Debug.Log($"Total Button components in scene: {buttons.Length}");

            foreach (var btn in buttons)
            {
                RectTransform rt = btn.GetComponent<RectTransform>();
                Vector3 worldCenter = rt.TransformPoint(rt.rect.center);
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

                PointerEventData pointerData = new PointerEventData(eventSystem)
                {
                    position = screenPos
                };

                var results = new List<RaycastResult>();
                raycaster.Raycast(pointerData, results);

                Debug.Log($"--- Testing Button '{btn.gameObject.name}' at ScreenPos={screenPos} ---");
                if (results.Count == 0)
                {
                    Debug.LogWarning($"[NO HIT] Raycast hit NOTHING at {btn.gameObject.name}!");
                }
                else
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        var r = results[i];
                        string tag = (r.gameObject == btn.gameObject) ? "<color=#00e676>[TARGET HIT]</color>" : $"<color=#ff1744>[BLOCKER #{i}]</color>";
                        Debug.Log($"  {tag} Hit #{i}: '{r.gameObject.name}' (depth={r.depth}, sortingOrder={r.sortingOrder}, raycastTarget={r.gameObject.GetComponent<Graphic>()?.raycastTarget})");
                    }
                }
            }
        }
    }
}
