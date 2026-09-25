namespace ScienceQuest.Interaction
{
    /// <summary>
    /// IInteractable - Interface dành cho tất cả vật thể có thể tương tác (NPC, Cửa, Thiết bị phòng lab, Bảng câu hỏi).
    /// Phân công: M2 - Gameplay Programmer
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Được gọi khi người chơi bấm phím tương tác (Ví dụ: Phím E)
        /// </summary>
        void Interact();

        /// <summary>
        /// Trả về dòng chữ nhắc nhở hiển thị trên UI (Ví dụ: "Nhấn E để mở cửa", "Nhấn E để nói chuyện")
        /// </summary>
        string GetInteractionPrompt();
    }
}
