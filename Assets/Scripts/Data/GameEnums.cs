using System;

/// <summary>
/// Vai trò của tài khoản người dùng
/// </summary>
public enum UserRole
{
    Student,    // Học sinh
    Teacher     // Giáo viên
}

/// <summary>
/// Lớp lưu trữ thông tin tài khoản người dùng
/// </summary>
[Serializable]
public class UserAccount
{
    public string username;
    public string password;
    public UserRole role = UserRole.Student;
    public string fullName = "";
}

/// <summary>
/// Định nghĩa các loại Môn học
/// </summary>
public enum SubjectType
{
    Biology,    // Sinh Học
    Chemistry,  // Hóa Học
    Physics     // Vật Lý
}

/// <summary>
/// Định nghĩa 2 hình thức bài học trong Game Đua Chó:
/// 1. Trắc Nghiệm (Multiple Choice)
/// 2. Tự Luận (Essay / Short Answer)
/// </summary>
public enum LessonType
{
    MultipleChoice, // Trắc Nghiệm
    Essay           // Tự Luận
}
