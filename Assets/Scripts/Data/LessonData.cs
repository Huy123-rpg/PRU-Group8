using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject hoặc Data Model chứa thông tin của một Bài học trong Chương
/// </summary>
[System.Serializable]
public class LessonData
{
    public int lessonNumber = 1;
    public string lessonTitle = "Bài 1: Lý thuyết cơ bản";
    public string lessonDescription = "";
    public bool isUnlocked = true;
    public int starsEarned = 0; // Từ 0 đến 3 sao

    [Header("Danh sách câu hỏi trong bài học (nếu có)")]
    public List<QuestionData> questions = new List<QuestionData>();
}
