using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/Core/GoogleSheetDataManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        string regex = @"private void Awake\(\)\s*\{\s*if \(Instance == null\)\s*\{\s*Instance = this;\s*gameObject\.AddComponent<ProgressSyncManager>\(\);\s*DontDestroyOnLoad\(gameObject\);\s*\}\s*else\s*\{\s*Destroy\(gameObject\);\s*\}\s*\}";
        string replacement = @"private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (UnityEngine.Object.FindFirstObjectByType<ProgressSyncManager>() == null)
            {
                UnityEngine.GameObject psmGO = new UnityEngine.GameObject(""ProgressSyncManager_Auto"");
                psmGO.AddComponent<ProgressSyncManager>();
                DontDestroyOnLoad(psmGO);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }";
        content = Regex.Replace(content, regex, replacement);

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
