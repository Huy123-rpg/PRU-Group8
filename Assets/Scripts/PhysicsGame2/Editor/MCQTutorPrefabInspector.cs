#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2.Editor
{
    public static class MCQTutorPrefabInspector
    {
        [MenuItem("Aurelia/Dump MCQ Tutor Prefab")]
        public static void Dump()
        {
            string prefabPath = "Assets/Resources/UI/MCQTutorPopup.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Prefab not found: " + prefabPath);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== MCQ TUTOR PREFAB DUMP ===");
            DumpTransform(prefab.transform, 0, sb);

            File.WriteAllText("scratch/prefab_dump.txt", sb.ToString());
            Debug.Log("Dumped to scratch/prefab_dump.txt");
        }

        private static void DumpTransform(Transform t, int depth, StringBuilder sb)
        {
            string indent = new string(' ', depth * 2);
            RectTransform rt = t as RectTransform;
            string rtInfo = rt != null
                ? $"anchoredPos={rt.anchoredPosition}, sizeDelta={rt.sizeDelta}, anchorMin={rt.anchorMin}, anchorMax={rt.anchorMax}, pivot={rt.pivot}, offsetMin={rt.offsetMin}, offsetMax={rt.offsetMax}"
                : "No RectTransform";

            sb.AppendLine($"{indent}- {t.name} (Active: {t.gameObject.activeSelf}) | {rtInfo}");

            // Components
            var img = t.GetComponent<Image>();
            if (img != null)
            {
                sb.AppendLine($"{indent}  [Image] sprite={img.sprite?.name}, color={img.color}, raycastTarget={img.raycastTarget}, type={img.type}");
            }
            var btn = t.GetComponent<Button>();
            if (btn != null)
            {
                sb.AppendLine($"{indent}  [Button] interactable={btn.interactable}, targetGraphic={btn.targetGraphic?.name}");
            }
            var tmp = t.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                sb.AppendLine($"{indent}  [TextMeshProUGUI] text='{tmp.text}', fontSize={tmp.fontSize}, align={tmp.alignment}, raycastTarget={tmp.raycastTarget}");
            }
            var inp = t.GetComponent<TMP_InputField>();
            if (inp != null)
            {
                sb.AppendLine($"{indent}  [TMP_InputField] targetGraphic={inp.targetGraphic?.name}, textViewport={inp.textViewport?.name}, textComponent={inp.textComponent?.name}, placeholder={inp.placeholder?.name}");
            }
            var cg = t.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                sb.AppendLine($"{indent}  [CanvasGroup] alpha={cg.alpha}, interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}");
            }

            for (int i = 0; i < t.childCount; i++)
            {
                DumpTransform(t.GetChild(i), depth + 1, sb);
            }
        }
    }
}
#endif
