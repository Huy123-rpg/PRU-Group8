using System;
using System.Collections.Generic;

namespace ScienceQuest.Data
{
    /// <summary>
    /// GameData - Struct/Class chứa dữ liệu lưu trữ thông tin màn chơi và tiến trình của người chơi.
    /// Phân công: M1 - Core Programmer / Team Lead
    /// </summary>
    [Serializable]
    public class GameData
    {
        public string playerName = "Học sinh KHTN 8";
        public int playerLevel = 1;
        public int playerEXP = 0;
        public int coins = 0;

        public string lastSavedScene = "Academy";

        // Danh sách các khu vực đã unlock (Academy, Physics, Chemistry, Biology)
        public List<string> unlockedAreas = new List<string>() { "Academy" };

        // Danh sách các câu hỏi / quiz đã hoàn thành (Dành cho Sprint tiếp theo)
        public List<string> completedQuizIDs = new List<string>();

        public GameData()
        {
            playerName = "Học sinh KHTN 8";
            playerLevel = 1;
            playerEXP = 0;
            coins = 0;
            lastSavedScene = "Academy";
            unlockedAreas = new List<string>() { "Academy" };
            completedQuizIDs = new List<string>();
        }
    }
}
