using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/LoginManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);

        // Fix typo LOginScene
        content = content.Replace("\"LOginScene\"", "\"LoginScene\"");

        // Fix free login bypass
        string bypassRegex = @"if \(!string\.IsNullOrEmpty\(username\) && !string\.IsNullOrEmpty\(password\) && password\.Length >= 4\)\s*\{\s*Debug\.Log\(\$""\[LoginManager\] Đăng nhập tự do tài khoản: \{username\}""\);\s*return true;\s*\}";
        content = Regex.Replace(content, bypassRegex, "");

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
