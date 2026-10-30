using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/MainMenuManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        string regex = @"string baiID = \$\""B\{chapterID\}_00\{lessonID\}\"";";
        string replacement = @"
        int chapIdx = 1;
        System.Text.RegularExpressions.Match mc = System.Text.RegularExpressions.Regex.Match(chapterID, @""\d+"");
        if (mc.Success) int.TryParse(mc.Value, out chapIdx);

        int lessonIdx = 1;
        System.Text.RegularExpressions.Match ml = System.Text.RegularExpressions.Regex.Match(lessonID, @""\d+"");
        if (ml.Success) int.TryParse(ml.Value, out lessonIdx);

        string baiID = string.Format(""B{0}_00{1}"", chapIdx, lessonIdx);";

        content = Regex.Replace(content, regex, replacement);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
