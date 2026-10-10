using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/UI/BiologyMenuManager.cs";
        string content = File.ReadAllText(path, Encoding.UTF8);
        
        string orig = @"if (btnName == ""btn_bai2"") baiID = $""B{chapID}_002"";
                        else if (btnName == ""btn_bai3"") baiID = $""B{chapID}_003"";
                        else if (btnName == ""btn_bai4"") baiID = $""B{chapID}_004"";
                        else if (btnName == ""btn_ontapchuong"") baiID = $""B{chapID}_005"";";
                        
        string rep = @"if (btnName == ""btn_bai1"") baiID = $""B{chapID}_001"";
                        else if (btnName == ""btn_bai2"") baiID = $""B{chapID}_002"";
                        else if (btnName == ""btn_bai3"") baiID = $""B{chapID}_003"";
                        else if (btnName == ""btn_bai4"") baiID = $""B{chapID}_004"";
                        else if (btnName == ""btn_ontapchuong"") baiID = $""B{chapID}_005"";";

        content = content.Replace(orig, rep);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}
