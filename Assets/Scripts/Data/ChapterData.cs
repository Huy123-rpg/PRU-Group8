using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject chứa thông tin của một chương học
/// </summary>
[CreateAssetMenu(fileName = "NewChapterData", menuName = "Game Data/Chapter Data")]
public class ChapterData : ScriptableObject
{
    public int chapterNumber = 1;
    public string chapterTitle = "Chương 1: Khái niệm cơ bản";
    public bool isUnlocked = true;
    public int starsEarned = 0; // Từ 0 đến 3 sao

    [Header("Danh sách câu hỏi trong chương")]
    public List<QuestionData> questions = new List<QuestionData>();
}
