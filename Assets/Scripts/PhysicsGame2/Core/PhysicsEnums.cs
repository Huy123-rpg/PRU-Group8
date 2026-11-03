namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Các enum định nghĩa chế độ chơi và định danh cốt lõi cho Physics Game 2.
    /// </summary>
    public enum GameMode
    {
        None = 0,
        Explore = 1,   // Khám phá (mini-game mô phỏng)
        Quiz = 2,      // Trắc nghiệm (10 câu hỏi)
        Written = 3,   // Tự luận (Điền câu trả lời ngắn)
        Theory = 4     // Học lý thuyết bài học
    }

    public enum PhysicsCharacterId
    {
        Lyra,
        Kai,
        Nova,
        Ethan
    }

    public enum PhysicsTeacherId
    {
        Minh, // Thầy Minh: Cơ bản, nền tảng
        Lan,  // Cô Lan: Ứng dụng thực tế, tình huống
        Duy,  // Thầy Duy: Tính toán, nâng cao
        Ha    // Cô Hà: Tổng hợp, vận dụng
    }
}
