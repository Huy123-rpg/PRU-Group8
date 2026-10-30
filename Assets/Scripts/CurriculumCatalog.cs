// ============================================================
// CurriculumCatalog.cs
// Dữ liệu CHƯƠNG TRÌNH HỌC (danh mục Chương - Bài) tách khỏi UI.
//
// - Menu dùng catalog này để sinh Chapter Card / Lesson Card
//   (KHÔNG hard-code trong MainMenuManager nữa).
// - IDs khớp với cột ChapterID / LessonID trong Google Sheet
//   (C11..C14, B36..B51) để QuestionManager lọc đúng.
// - Mở rộng sau này: thêm môn khác bằng 1 CatalogEntry mới
//   (VD: ChemistryGrade9), kiến trúc không đổi.
// ============================================================
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CurriculumLesson
{
    public string lessonID;    // "B39"
    public int lessonNumber;   // 39
    public string lessonName;  // "Tái bản DNA và phiên mã tạo ra RNA"

    public CurriculumLesson(string id, string name)
    {
        lessonID = id;
        lessonName = name;
        int.TryParse(id.TrimStart('B'), out lessonNumber);
    }
}

[System.Serializable]
public class CurriculumChapter
{
    public string chapterID;     // "C11"
    public int chapterNumber;    // 11
    public string chapterName;   // Tên đầy đủ (hiện trong HUD / chi tiết)
    public string cardTitle;     // Dòng ngắn hiện trên Chapter Card
    public List<CurriculumLesson> lessons = new List<CurriculumLesson>();

    public CurriculumChapter(string id, string name, string cardTitle)
    {
        chapterID = id;
        chapterName = name;
        this.cardTitle = cardTitle;
        int.TryParse(id.TrimStart('C'), out chapterNumber);
    }

    public CurriculumLesson GetLesson(string lessonID)
    {
        return lessons.FirstOrDefault(l => l.lessonID == lessonID);
    }
}

public static class CurriculumCatalog
{
    private static readonly List<CurriculumChapter> _biologyGrade9 = new List<CurriculumChapter>
    {
        // ---------------- CHƯƠNG 11 ----------------
        new CurriculumChapter("C11",
            "DI TRUYỀN HỌC MENDEL. CƠ SỞ PHÂN TỬ CỦA HIỆN TƯỢNG DI TRUYỀN",
            "Di truyền học Mendel\n& Cơ sở phân tử")
        {
            lessons =
            {
                new CurriculumLesson("B36", "Khái quát về di truyền học"),
                new CurriculumLesson("B37", "Các quy luật di truyền của Mendel"),
                new CurriculumLesson("B38", "Nucleic acid và gene"),
                new CurriculumLesson("B39", "Tái bản DNA và phiên mã tạo ra RNA"),
                new CurriculumLesson("B40", "Dịch mã và mối quan hệ từ gene đến tính trạng"),
                new CurriculumLesson("B41", "Đột biến gene"),
            }
        },
        // ---------------- CHƯƠNG 12 ----------------
        new CurriculumChapter("C12",
            "DI TRUYỀN NHIỄM SẮC THỂ",
            "Di truyền\nnhiễm sắc thể")
        {
            lessons =
            {
                new CurriculumLesson("B42", "Di truyền nhiễm sắc thể"),
                new CurriculumLesson("B43", "Nguyên phân và giảm phân"),
                new CurriculumLesson("B44", "Nhiễm sắc thể giới tính và cơ chế xác định giới tính"),
                new CurriculumLesson("B45", "Di truyền liên kết"),
                new CurriculumLesson("B46", "Đột biến nhiễm sắc thể"),
            }
        },
        // ---------------- CHƯƠNG 13 ----------------
        new CurriculumChapter("C13",
            "DI TRUYỀN HỌC VỚI CON NGƯỜI VÀ ĐỜI SỐNG",
            "Di truyền học với con người\nvà đời sống")
        {
            lessons =
            {
                new CurriculumLesson("B47", "Di truyền học với con người"),
                new CurriculumLesson("B48", "Ứng dụng công nghệ di truyền vào đời sống"),
            }
        },
        // ---------------- CHƯƠNG 14 ----------------
        new CurriculumChapter("C14",
            "TIẾN HÓA",
            "Tiến hóa")
        {
            lessons =
            {
                new CurriculumLesson("B49", "Khái niệm tiến hóa và các hình thức chọn lọc"),
                new CurriculumLesson("B50", "Cơ chế tiến hóa"),
                new CurriculumLesson("B51", "Sự phát sinh và phát triển của sự sống trên Trái Đất"),
            }
        },
    };

    /// <summary>Lấy danh sách chương của 1 môn. Môn khác → thêm entry mới.</summary>
    public static List<CurriculumChapter> GetChapters(string subject)
    {
        // Hiện tại chỉ có Sinh học 9; mọi môn chưa khai báo trả về Sinh học 9
        return _biologyGrade9;
    }

    /// <summary>Lấy danh sách bài của 1 chương.</summary>
    public static List<CurriculumLesson> GetLessons(string subject, string chapterID)
    {
        CurriculumChapter ch = GetChapter(subject, chapterID);
        return ch != null ? ch.lessons : new List<CurriculumLesson>();
    }

    public static CurriculumChapter GetChapter(string subject, string chapterID)
    {
        return _biologyGrade9.FirstOrDefault(c => c.chapterID == chapterID);
    }

    public static CurriculumLesson GetLesson(string subject, string chapterID, string lessonID)
    {
        return GetChapter(subject, chapterID)?.GetLesson(lessonID);
    }

    /// <summary>"Bài 39" — hiển thị trên Lesson Card / HUD.</summary>
    public static string LessonShortLabel(CurriculumLesson l)
        => l == null ? "" : $"Bài {l.lessonNumber}";

    /// <summary>"Bài 39 - Tái bản DNA và phiên mã tạo ra RNA" — hiển thị trên nút.</summary>
    public static string LessonFullLabel(CurriculumLesson l)
        => l == null ? "" : $"Bài {l.lessonNumber}\n{l.lessonName}";

    /// <summary>"CHƯƠNG 11" — hiển thị trên Chapter Card.</summary>
    public static string ChapterShortLabel(CurriculumChapter c)
        => c == null ? "" : $"CHƯƠNG {c.chapterNumber}";
}
