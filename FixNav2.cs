using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/Core/NavigationManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        string regex = @"case ""ChapterList"":\s*return ""Physics"";";
        string replacement = @"case ""ChapterList"":
                return ""Physics"";
            
            case ""ChapterList 1"":
                return ""SampleScene"";

            case ""SinhScene"":
                return ""LessonList 1"";
            
            case ""LessonList 1"":
                return ""ChapterList 1"";";

        content = Regex.Replace(content, regex, replacement);

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
