using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject chứa thông tin của một Môn học
/// </summary>
[CreateAssetMenu(fileName = "NewSubjectData", menuName = "Game Data/Subject Data")]
public class SubjectData : ScriptableObject
{
    public SubjectType subjectType;
    public string subjectName = "Sinh học";
    public Sprite icon;

    [Header("Danh sách chương học của môn này")]
    public List<ChapterData> chapters = new List<ChapterData>();
}
