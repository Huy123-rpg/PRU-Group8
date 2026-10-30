using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ScienceQuest.Quiz;

namespace ScienceQuest.Teacher
{
    using QuestionData = ScienceQuest.Quiz.QuestionData;

    public enum TeacherSubject
    {
        Physics,
        Biology,
        Chemistry
    }

    [Serializable]
    public class ChapterItem
    {
        public string id;
        public string name;
        public string subject; // "Physics", "Biology", "Chemistry"
        public int order;
        public bool isActive = true;
        public string description = "";
    }

    [Serializable]
    public class ChapterItemList
    {
        public List<ChapterItem> chapters = new List<ChapterItem>();
    }

    [Serializable]
    public class LessonItem
    {
        public string baiID;
        public string chuongId;
        public string tenChuong;
        public string tenBai;
        public string subject; // "Physics", "Biology", "Chemistry"
        public string khaiNiemTrongTam = "";
        public string congThucCanNho = "";
        public string hienTuongUngDung = "";
        public int order = 1;
    }

    [Serializable]
    public class LessonItemList
    {
        public List<LessonItem> lessons = new List<LessonItem>();
    }

    [Serializable]
    public class StudentProgressItem
    {
        public string studentId = "";
        public string studentName = "";
        public string className = "";
        public string subject = "Physics";
        public string lessonName = "";
        public int score = 0;
        public float accuracy = 0f;
        public int correctAnswers = 0;
        public int totalQuestions = 0;
        public float timeUsed = 0f;
        public string date = "";
    }

    [Serializable]
    public class StudentProgressItemList
    {
        public List<StudentProgressItem> students = new List<StudentProgressItem>();
    }

    /// <summary>
    /// Quản lý dữ liệu trung tâm dành cho Role Giáo Viên:
    /// - Quản lý Chương (Physics, Biology, Chemistry)
    /// - Quản lý Bài học & Lý thuyết 3 trang
    /// - Quản lý Ngân hàng câu hỏi (Trắc nghiệm + Tự luận 18 cột)
    /// - Quản lý Tiến độ học sinh & Đồng bộ Google Sheets
    /// - Dữ liệu được lưu trữ Persistent (JSON + PlayerPrefs), không mất sau khi thoát game
    /// </summary>
    public class TeacherDataManager : MonoBehaviour
    {
        private static TeacherDataManager _instance;
        public static TeacherDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TeacherDataManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TeacherDataManager_Auto");
                        _instance = go.AddComponent<TeacherDataManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private List<ChapterItem> _chapters = new List<ChapterItem>();
        private List<LessonItem> _lessons = new List<LessonItem>();
        private List<QuestionData> _questions = new List<QuestionData>();
        private List<StudentProgressItem> _studentProgress = new List<StudentProgressItem>();

        private bool _isInitialized = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void InitializeData()
        {
            if (_isInitialized) return;
            LoadChapters();
            LoadLessons();
            LoadQuestions();
            LoadStudentProgress();
            _isInitialized = true;
            Debug.Log($"[TeacherDataManager] 📦 Khởi tạo hoàn tất: {_chapters.Count} chương, {_lessons.Count} bài học, {_questions.Count} câu hỏi, {_studentProgress.Count} tiến độ HS.");
        }

        #region ===== HELPER CHUYỂN ĐỔI TÊN MÔN HỌC =====

        public static string SubjectToString(TeacherSubject sub)
        {
            switch (sub)
            {
                case TeacherSubject.Physics: return "Physics";
                case TeacherSubject.Biology: return "Biology";
                case TeacherSubject.Chemistry: return "Chemistry";
                default: return "Physics";
            }
        }

        public static string SubjectToVietnamese(TeacherSubject sub)
        {
            switch (sub)
            {
                case TeacherSubject.Physics: return "Vật lý";
                case TeacherSubject.Biology: return "Sinh học";
                case TeacherSubject.Chemistry: return "Hóa học";
                default: return "Vật lý";
            }
        }

        public static TeacherSubject StringToSubject(string str)
        {
            if (string.IsNullOrEmpty(str)) return TeacherSubject.Physics;
            string s = str.Trim().ToLowerInvariant();
            if (s.Contains("bio") || s.Contains("sinh")) return TeacherSubject.Biology;
            if (s.Contains("chem") || s.Contains("hóa") || s.Contains("hoa")) return TeacherSubject.Chemistry;
            return TeacherSubject.Physics;
        }

        #endregion

        #region ===== 1. QUẢN LÝ CHƯƠNG (CHAPTERS) =====

        private string ChaptersFilePath => Path.Combine(Application.persistentDataPath, "teacher_chapters.json");

        private void LoadChapters()
        {
            _chapters.Clear();
            bool loaded = false;

            // 1. Đọc từ file persistentDataPath nếu có
            try
            {
                if (File.Exists(ChaptersFilePath))
                {
                    string json = File.ReadAllText(ChaptersFilePath);
                    var data = JsonUtility.FromJson<ChapterItemList>(json);
                    if (data != null && data.chapters != null && data.chapters.Count > 0)
                    {
                        _chapters = data.chapters;
                        loaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeacherDataManager] Lỗi đọc {ChaptersFilePath}: {ex.Message}");
            }

            // 2. Fallback PlayerPrefs
            if (!loaded && PlayerPrefs.HasKey("Teacher_Custom_Chapters_Json"))
            {
                try
                {
                    string json = PlayerPrefs.GetString("Teacher_Custom_Chapters_Json");
                    var data = JsonUtility.FromJson<ChapterItemList>(json);
                    if (data != null && data.chapters != null && data.chapters.Count > 0)
                    {
                        _chapters = data.chapters;
                        loaded = true;
                    }
                }
                catch { }
            }

            // 3. Fallback khởi tạo mặc định cho cả 3 môn nếu chưa có dữ liệu
            if (!loaded || _chapters.Count == 0)
            {
                CreateDefaultChapters();
                SaveChapters();
            }
            else
            {
                // Sanitize & Deduplicate loaded chapters
                var cleanChapters = new List<ChapterItem>();
                var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                bool neededCleaning = false;

                foreach (var ch in _chapters)
                {
                    if (ch == null) continue;
                    string cName = (ch.name ?? "").Trim();
                    string cId = (ch.id ?? "").Trim();
                    if (string.IsNullOrEmpty(cId) || string.IsNullOrEmpty(cName)) { neededCleaning = true; continue; }
                    if (cName.Equals("xin chao", StringComparison.OrdinalIgnoreCase) || cId.Equals("Nhận biết", StringComparison.OrdinalIgnoreCase))
                    {
                        neededCleaning = true;
                        continue;
                    }
                    if (ch.subject == "Physics")
                    {
                        if (cId == "chuong3" && !cName.Contains("Điện từ")) { neededCleaning = true; continue; }
                        if (cId == "chuong4" && !cName.Contains("Điện thế")) { neededCleaning = true; continue; }
                        if (cId == "chuong5" && !cName.Contains("Sự bảo toàn")) { neededCleaning = true; continue; }
                    }

                    string key = $"{ch.subject}_{cId}";
                    if (!seenKeys.Contains(key))
                    {
                        seenKeys.Add(key);
                        cleanChapters.Add(ch);
                    }
                    else
                    {
                        neededCleaning = true;
                    }
                }

                _chapters = cleanChapters;
                if (neededCleaning)
                {
                    SaveChapters();
                }
            }
        }

        private void CreateDefaultChapters()
        {
            // --- VẬT LÝ 9 ---
            _chapters.Add(new ChapterItem { id = "chuong1", name = "Chương I. Năng lượng cơ học", subject = "Physics", order = 1, isActive = true, description = "Động năng, thế năng, cơ năng, công và công suất." });
            _chapters.Add(new ChapterItem { id = "chuong2", name = "Chương II. Ánh sáng", subject = "Physics", order = 2, isActive = true, description = "Khúc xạ ánh sáng, phản xạ toàn phần, thấu kính hội tụ, thấu kính phân kì, kính lúp." });
            _chapters.Add(new ChapterItem { id = "chuong3", name = "Chương III. Điện từ học", subject = "Physics", order = 3, isActive = true, description = "Từ trường, từ phổ, đường sức từ, lực điện từ." });
            _chapters.Add(new ChapterItem { id = "chuong4", name = "Chương IV. Điện thế & Dòng điện xoay chiều", subject = "Physics", order = 4, isActive = true, description = "Hiện tượng cảm ứng điện từ, máy phát điện, dòng điện xoay chiều." });
            _chapters.Add(new ChapterItem { id = "chuong5", name = "Chương V. Năng lượng & Sự bảo toàn", subject = "Physics", order = 5, isActive = true, description = "Năng lượng tái tạo, định luật bảo toàn và chuyển hóa năng lượng." });
            _chapters.Add(new ChapterItem { id = "tonghop", name = "TỔNG ÔN CẢ QUYỂN (5 chương)", subject = "Physics", order = 6, isActive = true, description = "Đề ôn tập tổng hợp toàn bộ chương trình Vật lý 9." });

            // --- SINH HỌC 9 ---
            _chapters.Add(new ChapterItem { id = "C11", name = "Chương XI. Di truyền học Mendel & Cơ sở phân tử", subject = "Biology", order = 1, isActive = true, description = "Khái quát di truyền, các quy luật Mendel, ADN và ARN." });
            _chapters.Add(new ChapterItem { id = "C12", name = "Chương XII. Di truyền nhiễm sắc thể", subject = "Biology", order = 2, isActive = true, description = "Nguyên phân, giảm phân, nhiễm sắc thể giới tính và di truyền liên kết." });
            _chapters.Add(new ChapterItem { id = "C13", name = "Chương XIII. Di truyền học với đời sống", subject = "Biology", order = 3, isActive = true, description = "Ứng dụng công nghệ di truyền vào đời sống và y học." });
            _chapters.Add(new ChapterItem { id = "C14", name = "Chương XIV. Tiến hóa", subject = "Biology", order = 4, isActive = true, description = "Các học thuyết tiến hóa và sự phát sinh phát triển của sinh giới." });

            // --- HÓA HỌC 9 ---
            _chapters.Add(new ChapterItem { id = "chem_c1", name = "Chương I. Kim loại & Dãy hoạt động hóa học", subject = "Chemistry", order = 1, isActive = true, description = "Tính chất của kim loại, nhôm, sắt, hợp kim và sự ăn mòn kim loại." });
            _chapters.Add(new ChapterItem { id = "chem_c2", name = "Chương II. Phi kim & Bảng tuần hoàn", subject = "Chemistry", order = 2, isActive = true, description = "Tính chất phi kim, Clo, Cacbon, Silic và bảng tuần hoàn các nguyên tố hóa học." });
            _chapters.Add(new ChapterItem { id = "chem_c3", name = "Chương III. Hydrocarbon & Nhiên liệu", subject = "Chemistry", order = 3, isActive = true, description = "Metan, Etilen, Axetilen, Benzen và dầu mỏ." });
            _chapters.Add(new ChapterItem { id = "chem_c4", name = "Chương IV. Dẫn xuất Hydrocarbon & Polymer", subject = "Chemistry", order = 4, isActive = true, description = "Rượu etylic, axit axetic, chất béo, glucozơ và chất dẻo polymer." });
        }

        public void SaveChapters()
        {
            try
            {
                var list = new ChapterItemList { chapters = _chapters };
                string json = JsonUtility.ToJson(list, true);

                File.WriteAllText(ChaptersFilePath, json);
                PlayerPrefs.SetString("Teacher_Custom_Chapters_Json", json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeacherDataManager] Lỗi lưu chapters: {ex.Message}");
            }
        }

        public List<ChapterItem> GetChapters(string subject)
        {
            return GetChapters(StringToSubject(subject));
        }

        public List<ChapterItem> GetChapters(TeacherSubject sub)
        {
            string subStr = SubjectToString(sub);
            return _chapters.Where(c => c.subject.Equals(subStr, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(c => c.order)
                            .ToList();
        }

        public ChapterItem GetChapterById(string id)
        {
            return _chapters.FirstOrDefault(c => c.id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public bool AddChapter(TeacherSubject sub, string name, string description, out string error)
        {
            error = "";
            string cleanName = (name ?? "").Trim();
            if (string.IsNullOrEmpty(cleanName))
            {
                error = "Tên chương không được để trống hoặc chỉ có dấu cách!";
                return false;
            }

            string subStr = SubjectToString(sub);
            bool isDuplicate = _chapters.Any(c => c.subject.Equals(subStr, StringComparison.OrdinalIgnoreCase) &&
                                                  c.name.Trim().Equals(cleanName, StringComparison.OrdinalIgnoreCase));
            if (isDuplicate)
            {
                error = $"Môn {SubjectToVietnamese(sub)} đã có chương '{cleanName}'!";
                return false;
            }

            var inSub = GetChapters(sub);
            int nextOrder = inSub.Count + 1;
            string prefix = sub == TeacherSubject.Physics ? "chuong" : (sub == TeacherSubject.Biology ? "C" : "chem_c");
            string newId = $"{prefix}{nextOrder}_{DateTime.Now.Ticks % 10000}";

            _chapters.Add(new ChapterItem
            {
                id = newId,
                name = cleanName,
                subject = subStr,
                order = nextOrder,
                isActive = true,
                description = (description ?? "").Trim()
            });

            SaveChapters();
            return true;
        }

        public bool UpdateChapter(string id, string newName, string newDescription, out string error)
        {
            error = "";
            var ch = GetChapterById(id);
            if (ch == null)
            {
                error = "Không tìm thấy chương cần sửa!";
                return false;
            }

            string cleanName = (newName ?? "").Trim();
            if (string.IsNullOrEmpty(cleanName))
            {
                error = "Tên chương không được để trống!";
                return false;
            }

            // Check duplicate in same subject
            bool isDuplicate = _chapters.Any(c => c != ch &&
                                                  c.subject.Equals(ch.subject, StringComparison.OrdinalIgnoreCase) &&
                                                  c.name.Trim().Equals(cleanName, StringComparison.OrdinalIgnoreCase));
            if (isDuplicate)
            {
                error = "Tên chương này đã tồn tại trong môn học!";
                return false;
            }

            ch.name = cleanName;
            ch.description = (newDescription ?? "").Trim();
            SaveChapters();

            // Cập nhật tên chương ở các bài học liên kết
            foreach (var l in _lessons.Where(l => l.chuongId == ch.id))
            {
                l.tenChuong = cleanName;
            }
            SaveLessons();

            return true;
        }

        public void ToggleChapterActive(string id)
        {
            var ch = GetChapterById(id);
            if (ch != null)
            {
                ch.isActive = !ch.isActive;
                SaveChapters();
            }
        }

        public static bool IsChapterActive(string subject, string chapterId)
        {
            if (Instance == null) return true;
            var ch = Instance._chapters.FirstOrDefault(c =>
                c.subject.Equals(subject, StringComparison.OrdinalIgnoreCase) &&
                c.id.Equals(chapterId, StringComparison.OrdinalIgnoreCase));
            return ch == null || ch.isActive;
        }

        public bool IsChapterActiveByIndex(string subject, int index1Based)
        {
            TeacherSubject s = StringToSubject(subject);
            var list = GetChapters(s);
            if (index1Based >= 1 && index1Based <= list.Count)
            {
                return list[index1Based - 1].isActive;
            }
            return true;
        }

        public static bool CheckChapterActiveByIndex(string subject, int index1Based)
        {
            if (Instance == null) return true;
            return Instance.IsChapterActiveByIndex(subject, index1Based);
        }

        #endregion

        #region ===== 2. QUẢN LÝ BÀI HỌC & LÝ THUYẾT (LESSONS) =====

        private string LessonsFilePath => Path.Combine(Application.persistentDataPath, "teacher_lessons.json");

        private void LoadLessons()
        {
            _lessons.Clear();
            bool loaded = false;

            // 1. Persistent file
            try
            {
                if (File.Exists(LessonsFilePath))
                {
                    string json = File.ReadAllText(LessonsFilePath);
                    var data = JsonUtility.FromJson<LessonItemList>(json);
                    if (data != null && data.lessons != null && data.lessons.Count > 0)
                    {
                        _lessons = data.lessons;
                        loaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeacherDataManager] Lỗi đọc {LessonsFilePath}: {ex.Message}");
            }

            // 2. PlayerPrefs
            if (!loaded && PlayerPrefs.HasKey("Teacher_Custom_Lessons_Json"))
            {
                try
                {
                    string json = PlayerPrefs.GetString("Teacher_Custom_Lessons_Json");
                    var data = JsonUtility.FromJson<LessonItemList>(json);
                    if (data != null && data.lessons != null && data.lessons.Count > 0)
                    {
                        _lessons = data.lessons;
                        loaded = true;
                    }
                }
                catch { }
            }

            // 3. Nạp từ physics_theory.json và CurriculumCatalog
            if (!loaded || _lessons.Count == 0)
            {
                CreateDefaultLessons();
                SaveLessons();
            }
        }

        private void CreateDefaultLessons()
        {
            // Nạp Vật lý từ Resources physics_theory.json
            TextAsset theoryAsset = Resources.Load<TextAsset>("QuizData/physics_theory");
            if (theoryAsset != null && !string.IsNullOrEmpty(theoryAsset.text))
            {
                try
                {
                    var data = JsonUtility.FromJson<PhysicsGame1.TheoryLessonList>(theoryAsset.text);
                    if (data != null && data.lessons != null)
                    {
                        int ord = 1;
                        foreach (var l in data.lessons)
                        {
                            _lessons.Add(new LessonItem
                            {
                                baiID = l.baiID,
                                chuongId = l.chuongId,
                                tenChuong = l.tenChuong,
                                tenBai = l.tenBai,
                                subject = "Physics",
                                khaiNiemTrongTam = l.khaiNiemTrongTam,
                                congThucCanNho = l.congThucCanNho,
                                hienTuongUngDung = l.hienTuongUngDung,
                                order = ord++
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[TeacherDataManager] Lỗi parse physics_theory: {ex.Message}");
                }
            }

            // Nạp Sinh học mặc định từ CurriculumCatalog
            var bioChapters = CurriculumCatalog.GetChapters("Biology");
            if (bioChapters != null)
            {
                int bOrd = 1;
                foreach (var ch in bioChapters)
                {
                    foreach (var l in ch.lessons)
                    {
                        _lessons.Add(new LessonItem
                        {
                            baiID = l.lessonID,
                            chuongId = ch.chapterID,
                            tenChuong = ch.chapterName,
                            tenBai = $"Bài {l.lessonNumber}. {l.lessonName}",
                            subject = "Biology",
                            khaiNiemTrongTam = $"Kiến thức trọng tâm về {l.lessonName} trong chương trình Sinh học 9.",
                            congThucCanNho = "Quy luật di truyền, cấu trúc phân tử ADN/ARN và các nguyên tắc bổ sung A-T, G-X.",
                            hienTuongUngDung = "Giải thích các tính trạng di truyền ở người, chọn giống vật nuôi cây trồng và bảo vệ môi trường.",
                            order = bOrd++
                        });
                    }
                }
            }

            // Nạp Hóa học mặc định
            AddDefaultChemistryLessons();
        }

        private void AddDefaultChemistryLessons()
        {
            var chemList = new (string cId, string cName, string bId, string bName, string kn, string ct, string ht)[]
            {
                ("chem_c1", "Chương I. Kim loại & Dãy hoạt động hóa học", "H14", "Bài 14. Tính chất chung của kim loại",
                    "Kim loại có tính dẻo, dẫn điện, dẫn nhiệt tốt và có ánh kim. Tác dụng với phi kim tạo muối hoặc oxide; tác dụng với acid loãng tạo muối và khí H2.",
                    "1. 2Fe + 3Cl2 -> 2FeCl3 (t°)\n2. Fe + 2HCl -> FeCl2 + H2 ↑\n3. Cu + 2AgNO3 -> Cu(NO3)2 + 2Ag ↓",
                    "Đồng dẫn điện làm dây cáp điện; nhôm nhẹ và bền làm khung cửa, vỏ máy bay; sắt làm khung cầu, bê tông cốt thép."),

                ("chem_c1", "Chương I. Kim loại & Dãy hoạt động hóa học", "H15", "Bài 15. Dãy hoạt động hóa học của kim loại",
                    "K > Na > Mg > Al > Zn > Fe > Pb > (H) > Cu > Ag > Au.\nÝ nghĩa: Kim loại đứng trước Mg phản ứng với nước ở nhiệt độ thường; đứng trước H đẩy được H ra khỏi acid loãng; kim loại đứng trước đẩy kim loại đứng sau ra khỏi muối.",
                    "Dãy nhớ: Khi Nào May Áo Giáp Sắt Nhớ Sang Phố Hỏi Cửa Hàng Á Phi Âu.",
                    "Mạ điện, chống ăn mòn kim loại trong môi trường biển, điện phân luyện nhôm."),

                ("chem_c2", "Chương II. Phi kim & Bảng tuần hoàn", "H17", "Bài 17. Tính chất của phi kim",
                    "Phi kim không dẫn điện (trừ than chì), dẫn nhiệt kém. Tác dụng với kim loại tạo muối, tác dụng với H2 tạo hợp chất khí, tác dụng với O2 tạo oxide acid.",
                    "1. C + O2 -> CO2 (t°)\n2. S + O2 -> SO2 (t°)\n3. H2 + Cl2 -> 2HCl (ánh sáng)",
                    "Than chì làm ruột bút chì và điện cực; lưu huỳnh sản xuất acid sulfuric; clo khử trùng nước sinh hoạt."),

                ("chem_c3", "Chương III. Hydrocarbon & Nhiên liệu", "H20", "Bài 20. Khái niệm hợp chất hữu cơ & Methane",
                    "Hợp chất hữu cơ là hợp chất của carbon (trừ CO, CO2, H2CO3 và các muối cacbonat). Metan (CH4) là hydrocarbon no, phân tử chỉ có liên kết đơn C-H.",
                    "1. Phản ứng cháy: CH4 + 2O2 -> CO2 + 2H2O (tỏa nhiệt lớn)\n2. Phản ứng thế: CH4 + Cl2 -> CH3Cl + HCl (ánh sáng)",
                    "Metan là thành phần chính của khí thiên nhiên, khí mỏ dầu và khí biogas, dùng làm nhiên liệu sinh hoạt."),

                ("chem_c4", "Chương IV. Dẫn xuất Hydrocarbon & Polymer", "H22", "Bài 22. Rượu Ethylic & Axit Acetic",
                    "Rượu etylic (C2H5OH) có nhóm -OH, phản ứng với Na giải phóng H2. Axit axetic (CH3COOH) có nhóm -COOH mang đầy đủ tính chất của một acid hữu cơ.",
                    "1. 2C2H5OH + 2Na -> 2C2H5ONa + H2 ↑\n2. 2CH3COOH + Na2CO3 -> 2CH3COONa + H2O + CO2 ↑\n3. Este hóa: C2H5OH + CH3COOH <=> CH3COOC2H5 + H2O (H2SO4 đặc, t°)",
                    "Cồn sát trùng trong y tế; giấm ăn gia đình; sản xuất tơ nhân tạo, dung môi pha sơn.")
            };

            int hOrd = 1;
            foreach (var item in chemList)
            {
                _lessons.Add(new LessonItem
                {
                    baiID = item.bId,
                    chuongId = item.cId,
                    tenChuong = item.cName,
                    tenBai = item.bName,
                    subject = "Chemistry",
                    khaiNiemTrongTam = item.kn,
                    congThucCanNho = item.ct,
                    hienTuongUngDung = item.ht,
                    order = hOrd++
                });
            }
        }

        public void SaveLessons()
        {
            try
            {
                var list = new LessonItemList { lessons = _lessons };
                string json = JsonUtility.ToJson(list, true);

                File.WriteAllText(LessonsFilePath, json);
                PlayerPrefs.SetString("Teacher_Custom_Lessons_Json", json);
                PlayerPrefs.Save();

                // Đồng bộ cập nhật vào file physics_theory.json nếu ở Editor
#if UNITY_EDITOR
                string resPath = Path.Combine(Application.dataPath, "Resources/QuizData/physics_theory.json");
                var phyLessons = _lessons.Where(l => l.subject == "Physics").Select(l => new PhysicsGame1.TheoryLessonItem
                {
                    baiID = l.baiID,
                    chuongId = l.chuongId,
                    tenChuong = l.tenChuong,
                    tenBai = l.tenBai,
                    khaiNiemTrongTam = l.khaiNiemTrongTam,
                    congThucCanNho = l.congThucCanNho,
                    hienTuongUngDung = l.hienTuongUngDung
                }).ToList();

                var wrapper = new PhysicsGame1.TheoryLessonList { lessons = phyLessons };
                File.WriteAllText(resPath, JsonUtility.ToJson(wrapper, true));
#endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeacherDataManager] Lỗi lưu lessons: {ex.Message}");
            }
        }

        public List<LessonItem> GetLessons(string subject, string chapterId = null)
        {
            return GetLessons(StringToSubject(subject), chapterId);
        }

        public List<LessonItem> GetLessons(TeacherSubject sub, string chapterId = null)
        {
            string subStr = SubjectToString(sub);
            var query = _lessons.Where(l => l.subject.Equals(subStr, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(chapterId))
            {
                query = query.Where(l => l.chuongId.Equals(chapterId, StringComparison.OrdinalIgnoreCase));
            }
            return query.OrderBy(l => l.order).ToList();
        }

        public LessonItem GetLessonById(string baiId)
        {
            return _lessons.FirstOrDefault(l => l.baiID.Equals(baiId, StringComparison.OrdinalIgnoreCase));
        }

        public bool AddLesson(TeacherSubject sub, string chapterId, string tenBai, string kn, string ct, string ht, out string error)
        {
            error = "";
            string cleanName = (tenBai ?? "").Trim();
            if (string.IsNullOrEmpty(cleanName))
            {
                error = "Tên bài học không được để trống!";
                return false;
            }

            var ch = GetChapterById(chapterId);
            string tenChuong = ch != null ? ch.name : "Chương học";

            string subStr = SubjectToString(sub);
            var lessonsInChap = GetLessons(sub, chapterId);
            string newBaiId = $"B_{subStr.Substring(0, 3)}_{lessonsInChap.Count + 1:D2}_{DateTime.Now.Ticks % 1000}";

            _lessons.Add(new LessonItem
            {
                baiID = newBaiId,
                chuongId = chapterId,
                tenChuong = tenChuong,
                tenBai = cleanName,
                subject = subStr,
                khaiNiemTrongTam = (kn ?? "").Trim(),
                congThucCanNho = (ct ?? "").Trim(),
                hienTuongUngDung = (ht ?? "").Trim(),
                order = _lessons.Count + 1
            });

            SaveLessons();
            return true;
        }

        public bool UpdateLesson(string baiId, string newTenBai, string kn, string ct, string ht, out string error)
        {
            error = "";
            var les = GetLessonById(baiId);
            if (les == null)
            {
                error = "Không tìm thấy bài học cần cập nhật!";
                return false;
            }

            string cleanName = (newTenBai ?? "").Trim();
            if (string.IsNullOrEmpty(cleanName))
            {
                error = "Tên bài học không được để trống!";
                return false;
            }

            les.tenBai = cleanName;
            les.khaiNiemTrongTam = (kn ?? "").Trim();
            les.congThucCanNho = (ct ?? "").Trim();
            les.hienTuongUngDung = (ht ?? "").Trim();

            SaveLessons();
            return true;
        }

        #endregion

        #region ===== 3. QUẢN LÝ NGÂN HÀNG CÂU HỎI (QUESTIONS) =====

        private string QuestionsFilePath => Path.Combine(Application.persistentDataPath, "teacher_questions.json");

        private void LoadQuestions()
        {
            _questions.Clear();
            bool loaded = false;

            // 1. Persistent file
            try
            {
                if (File.Exists(QuestionsFilePath))
                {
                    string json = File.ReadAllText(QuestionsFilePath);
                    var data = JsonUtility.FromJson<QuestionDataList>(json);
                    if (data != null && data.questions != null && data.questions.Count > 0)
                    {
                        _questions = data.questions;
                        loaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeacherDataManager] Lỗi đọc {QuestionsFilePath}: {ex.Message}");
            }

            // 2. PlayerPrefs
            if (!loaded && PlayerPrefs.HasKey("Teacher_Custom_Questions_Json"))
            {
                try
                {
                    string json = PlayerPrefs.GetString("Teacher_Custom_Questions_Json");
                    var data = JsonUtility.FromJson<QuestionDataList>(json);
                    if (data != null && data.questions != null && data.questions.Count > 0)
                    {
                        _questions = data.questions;
                        loaded = true;
                    }
                }
                catch { }
            }

            // 3. Fallback từ physics_questions.json
            if (!loaded || _questions.Count == 0)
            {
                TextAsset qAsset = Resources.Load<TextAsset>("QuizData/physics_questions");
                if (qAsset != null && !string.IsNullOrEmpty(qAsset.text))
                {
                    try
                    {
                        var data = JsonUtility.FromJson<QuestionDataList>(qAsset.text);
                        if (data != null && data.questions != null)
                        {
                            foreach (var q in data.questions)
                            {
                                if (string.IsNullOrEmpty(q.chapter)) q.chapter = "Physics";
                                _questions.Add(q);
                            }
                        }
                    }
                    catch { }
                }

                // Bổ sung câu hỏi mẫu cho Sinh học & Hóa học nếu chưa có
                AddSampleBioAndChemQuestions();
                SaveQuestions();
            }
        }

        private void AddSampleBioAndChemQuestions()
        {
            // Sinh học mẫu
            _questions.Add(new QuestionData
            {
                questionID = "BIO_001",
                stt = _questions.Count + 1,
                chuongId = "C11",
                tenChuong = "Chương XI. Di truyền học Mendel",
                baiID = "B38",
                tenBai = "Bài 38. Nucleic acid và gene",
                mucDo = "Nhận biết",
                loaiCauHoi = "Trắc nghiệm",
                questionText = "Phân tử ADN được cấu tạo từ bao nhiêu loại đơn phân nucleotit chính?",
                answers = new string[] { "4 loại (A, T, G, X)", "3 loại (A, U, G)", "2 loại (Purin, Pyrimidin)", "5 loại (A, T, U, G, X)" },
                correctAnswerIndex = 0,
                explanation = "ADN được cấu tạo từ 4 loại nucleotit là Adenin (A), Timin (T), Guanin (G) và Xitozin (X).",
                meoGhiNho = "ADN nhớ 4 chữ cái: A - T - G - X.",
                chapter = "Biology"
            });

            _questions.Add(new QuestionData
            {
                questionID = "BIO_002",
                stt = _questions.Count + 1,
                chuongId = "C11",
                tenChuong = "Chương XI. Di truyền học Mendel",
                baiID = "B39",
                tenBai = "Bài 39. Tái bản DNA",
                mucDo = "Thông hiểu",
                loaiCauHoi = "Tự luận",
                questionText = "Nêu nguyên tắc bán bảo tồn trong quá trình nhân đôi ADN và ý nghĩa sinh học của nó.",
                answers = new string[] { "Tự luận", "-", "-", "-" },
                correctAnswerIndex = 0,
                dapAnThamChieu = "Trong mỗi phân tử ADN con tạo thành, có một mạch là của ADN mẹ ban đầu, còn mạch kia được tổng hợp mới từ môi trường nội bào theo nguyên tắc bổ sung (A-T, G-X). Ý nghĩa: Đảm bảo thông tin di truyền được truyền đạt chính xác qua các thế hệ tế bào.",
                tieuChiCham = "1. Nêu đúng định nghĩa mạch cũ - mạch mới (0.5đ). 2. Nêu nguyên tắc bổ sung (0.25đ). 3. Nêu ý nghĩa truyền đạt nguyên vẹn (0.25đ).",
                meoGhiNho = "Bán bảo tồn = giữ lại một nửa.",
                chapter = "Biology"
            });

            // Hóa học mẫu
            _questions.Add(new QuestionData
            {
                questionID = "CHEM_001",
                stt = _questions.Count + 1,
                chuongId = "chem_c1",
                tenChuong = "Chương I. Kim loại",
                baiID = "H15",
                tenBai = "Bài 15. Dãy hoạt động hóa học",
                mucDo = "Nhận biết",
                loaiCauHoi = "Trắc nghiệm",
                questionText = "Kim loại nào sau đây tác dụng mãnh liệt với nước ở nhiệt độ thường?",
                answers = new string[] { "Natri (Na)", "Sắt (Fe)", "Đồng (Cu)", "Kẽm (Zn)" },
                correctAnswerIndex = 0,
                explanation = "Natri là kim loại kiềm, đứng trước Mg trong dãy hoạt động nên tác dụng mãnh liệt với nước tạo dung dịch kiềm NaOH và khí H2.",
                meoGhiNho = "K, Na tác dụng nước ngay ở nhiệt độ thường.",
                chapter = "Chemistry"
            });

            _questions.Add(new QuestionData
            {
                questionID = "CHEM_002",
                stt = _questions.Count + 1,
                chuongId = "chem_c3",
                tenChuong = "Chương III. Hydrocarbon",
                baiID = "H20",
                tenBai = "Bài 20. Khái niệm hợp chất hữu cơ & Methane",
                mucDo = "Vận dụng",
                loaiCauHoi = "Tự luận",
                questionText = "Đốt cháy hoàn toàn 2.24 lít khí metan (đktc). Tính thể tích khí oxy cần dùng và khối lượng nước tạo thành.",
                answers = new string[] { "Tự luận", "-", "-", "-" },
                correctAnswerIndex = 0,
                dapAnThamChieu = "Phương trình: CH4 + 2O2 -> CO2 + 2H2O (t°).\nSố mol CH4 = 2.24 / 22.4 = 0.1 mol.\nTheo pt: n(O2) = 2 * n(CH4) = 0.2 mol => V(O2) = 0.2 * 22.4 = 4.48 lít.\nn(H2O) = 2 * n(CH4) = 0.2 mol => m(H2O) = 0.2 * 18 = 3.6 gam.",
                tieuChiCham = "1. Viết và cân bằng đúng phương trình (0.25đ). 2. Tính đúng mol CH4 (0.25đ). 3. Tính đúng V(O2) = 4.48L (0.25đ). 4. Tính đúng m(H2O) = 3.6g (0.25đ).",
                meoGhiNho = "Đốt 1 mol CH4 cần 2 mol O2 sinh ra 1 mol CO2 và 2 mol H2O.",
                chapter = "Chemistry"
            });
        }

        public void SaveQuestions()
        {
            try
            {
                var list = new QuestionDataList { questions = _questions };
                string json = JsonUtility.ToJson(list, true);

                File.WriteAllText(QuestionsFilePath, json);
                PlayerPrefs.SetString("Teacher_Custom_Questions_Json", json);
                PlayerPrefs.Save();

#if UNITY_EDITOR
                string resPath = Path.Combine(Application.dataPath, "Resources/QuizData/physics_questions.json");
                File.WriteAllText(resPath, json);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TeacherDataManager] Lỗi lưu questions: {ex.Message}");
            }
        }

        public List<QuestionData> GetQuestions(string subject, string mucDo = "Tất cả", string loai = "Tất cả")
        {
            return GetQuestions(StringToSubject(subject), mucDo, loai);
        }

        public List<QuestionData> GetQuestions(TeacherSubject sub, string mucDo = "Tất cả", string loai = "Tất cả")
        {
            string subStr = SubjectToString(sub);
            return _questions.Where(q =>
            {
                // Nhận diện môn học
                bool matchSub = false;
                if (!string.IsNullOrEmpty(q.chapter) && q.chapter.Equals(subStr, StringComparison.OrdinalIgnoreCase)) matchSub = true;
                else if (q.questionID != null && q.questionID.StartsWith("BIO", StringComparison.OrdinalIgnoreCase) && sub == TeacherSubject.Biology) matchSub = true;
                else if (q.questionID != null && (q.questionID.StartsWith("CHEM", StringComparison.OrdinalIgnoreCase) || q.questionID.StartsWith("H8_") || q.questionID.StartsWith("TL_H8")) && sub == TeacherSubject.Chemistry) matchSub = true;
                else if (sub == TeacherSubject.Physics && (string.IsNullOrEmpty(q.chapter) || q.chapter.Equals("Physics", StringComparison.OrdinalIgnoreCase) || q.questionID.StartsWith("C9_") || q.questionID.StartsWith("PHY"))) matchSub = true;

                if (!matchSub) return false;

                // Lọc mức độ
                if (mucDo != "Tất cả" && !string.IsNullOrEmpty(q.mucDo) && !q.mucDo.Equals(mucDo, StringComparison.OrdinalIgnoreCase)) return false;

                // Lọc loại câu hỏi
                if (loai != "Tất cả")
                {
                    bool isEssay = !string.IsNullOrEmpty(q.loaiCauHoi) && (q.loaiCauHoi.Contains("Tự luận") || q.loaiCauHoi.Contains("TuLuan"));
                    if (loai == "Trắc nghiệm" && isEssay) return false;
                    if (loai == "Tự luận" && !isEssay) return false;
                }

                return true;
            }).ToList();
        }

        public void SaveOrUpdateQuestion(QuestionData q, bool isNew)
        {
            if (isNew)
            {
                q.stt = _questions.Count + 1;
                _questions.Add(q);
            }
            else
            {
                var existing = _questions.FirstOrDefault(item => item.questionID == q.questionID);
                if (existing != null)
                {
                    int idx = _questions.IndexOf(existing);
                    _questions[idx] = q;
                }
                else
                {
                    _questions.Add(q);
                }
            }

            SaveQuestions();
        }

        public void PushQuestionToGoogleSheet(QuestionData q, Action<bool, string> onComplete)
        {
            if (GoogleSheetDataManager.Instance != null)
            {
                StartCoroutine(GoogleSheetDataManager.Instance.SubmitQuestionToSheet(q, onComplete));
            }
            else
            {
                onComplete?.Invoke(false, "GoogleSheetDataManager chưa sẵn sàng!");
            }
        }

        public void SyncQuestionsFromGoogleSheet(Action<bool> onComplete)
        {
            StartCoroutine(FetchAndImportAllQuestionsFromSheet(TeacherSubject.Physics, onComplete));
        }

        public void SyncQuestionsFromGoogleSheet(TeacherSubject subject, Action<bool> onComplete)
        {
            StartCoroutine(FetchAndImportAllQuestionsFromSheet(subject, onComplete));
        }

        private IEnumerator FetchAndImportAllQuestionsFromSheet(TeacherSubject subject, Action<bool> onComplete)
        {
            string mcUrl = "";
            string essayUrl = "";

            if (subject == TeacherSubject.Biology)
            {
                mcUrl = "https://docs.google.com/spreadsheets/d/1rDuaisuA9P8kDRcPCwrnLFQnt4DLbdPX6Zksftf9LfQ/export?format=csv&gid=0";
                essayUrl = "https://docs.google.com/spreadsheets/d/1rDuaisuA9P8kDRcPCwrnLFQnt4DLbdPX6Zksftf9LfQ/export?format=csv&gid=460607521";
            }
            else if (subject == TeacherSubject.Chemistry)
            {
                mcUrl = "https://docs.google.com/spreadsheets/d/19dOXUGG0ygoGNwCvOtgI_TVUcSg8bUGdS3tgqzMxmRI/export?format=csv&gid=0";
                essayUrl = "https://docs.google.com/spreadsheets/d/19dOXUGG0ygoGNwCvOtgI_TVUcSg8bUGdS3tgqzMxmRI/export?format=csv&gid=205802123";
            }
            else
            {
                mcUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=286121556";
                essayUrl = "https://docs.google.com/spreadsheets/d/1X8xjI3qn9XubGlkRyLD-hDw1HKL9s9JDSizkxSCrCIY/export?format=csv&gid=2085901900";
            }

            int importedMC = 0;
            int importedEssay = 0;

            // 1. Tải câu hỏi trắc nghiệm
            using (var wwwMC = UnityEngine.Networking.UnityWebRequest.Get(mcUrl))
            {
                yield return wwwMC.SendWebRequest();
                if (wwwMC.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    if (subject == TeacherSubject.Biology)
                        importedMC = ParseAndMergeBioMCQuestions(wwwMC.downloadHandler.text);
                    else
                        importedMC = ParseAndMergeMCQuestions(wwwMC.downloadHandler.text, subject);
                }
                else
                {
                    Debug.LogWarning($"[TeacherDataManager] Lỗi tải Trắc nghiệm từ Sheet: {wwwMC.error}");
                }
            }

            // 2. Tải câu hỏi tự luận
            using (var wwwEssay = UnityEngine.Networking.UnityWebRequest.Get(essayUrl))
            {
                yield return wwwEssay.SendWebRequest();
                if (wwwEssay.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    if (subject == TeacherSubject.Biology)
                        importedEssay = ParseAndMergeBioEssayQuestions(wwwEssay.downloadHandler.text);
                    else
                        importedEssay = ParseAndMergeEssayQuestions(wwwEssay.downloadHandler.text, subject);
                }
                else
                {
                    Debug.LogWarning($"[TeacherDataManager] Lỗi tải Tự luận từ Sheet: {wwwEssay.error}");
                }
            }

            if (importedMC > 0 || importedEssay > 0)
            {
                SaveQuestions();
                SaveChapters();
                Debug.Log($"[TeacherDataManager] ✅ Đã đồng bộ thành công {importedMC} câu trắc nghiệm và {importedEssay} câu tự luận từ Google Sheet!");
                onComplete?.Invoke(true);
            }
            else
            {
                // Fallback nếu không kết nối được trực tiếp
                if (GoogleSheetDataManager.Instance != null)
                {
                    yield return GoogleSheetDataManager.Instance.FetchQuestionsFromSheet((success) =>
                    {
                        if (success) LoadQuestions();
                        onComplete?.Invoke(success);
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            }
        }

        private int ParseAndMergeMCQuestions(string csvText, TeacherSubject subject = TeacherSubject.Physics)
        {
            var rows = ParseCsvLines(csvText);
            if (rows.Count <= 1) return 0;
            int count = 0;
            string subStr = SubjectToString(subject);
            string subVn = SubjectToVietnamese(subject);

            for (int r = 1; r < rows.Count; r++)
            {
                var cols = rows[r];
                if (cols.Count < 14) continue;

                string qId = cols[1].Trim();
                string cId = cols[2].Trim();
                string cName = cols[3].Trim();
                string bId = cols[4].Trim();
                string bName = cols[5].Trim();
                string mucDo = cols[7].Trim();
                string qText = cols[8].Trim();
                string a = cols[9].Trim();
                string b = cols[10].Trim();
                string c = cols[11].Trim();
                string d = cols[12].Trim();
                string correct = cols[13].Trim().ToUpperInvariant();
                string explain = cols.Count > 14 ? cols[14].Trim() : "";
                string tip = cols.Count > 15 ? cols[15].Trim() : "";

                if (string.IsNullOrEmpty(qId) || string.IsNullOrEmpty(qText)) continue;

                int correctIdx = 0;
                if (correct == "B" || correct == "1") correctIdx = 1;
                else if (correct == "C" || correct == "2") correctIdx = 2;
                else if (correct == "D" || correct == "3") correctIdx = 3;

                // Sync chapter nếu là chương hợp lệ và chưa từng có
                if (!string.IsNullOrEmpty(cName) && cName.StartsWith("Chương", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_chapters.Any(ch => ch.subject == subStr && (ch.id.Equals(cId, StringComparison.OrdinalIgnoreCase) || ch.name.Equals(cName, StringComparison.OrdinalIgnoreCase))))
                    {
                        _chapters.Add(new ChapterItem
                        {
                            id = !string.IsNullOrEmpty(cId) ? cId : $"chuong_{_chapters.Count + 1}",
                            name = cName,
                            subject = subStr,
                            order = _chapters.Count + 1,
                            isActive = true,
                            description = $"Chương {subVn} đồng bộ từ Google Sheets ({cName})"
                        });
                    }
                }

                var existing = _questions.FirstOrDefault(q => q.questionID == qId);
                if (existing != null)
                {
                    existing.chuongId = cId;
                    existing.tenChuong = cName;
                    existing.baiID = bId;
                    existing.tenBai = bName;
                    existing.loaiCauHoi = "Trắc nghiệm";
                    existing.mucDo = mucDo;
                    existing.questionText = qText;
                    existing.answers = new string[] { a, b, c, d };
                    existing.correctAnswerIndex = correctIdx;
                    existing.explanation = explain;
                    existing.meoGhiNho = tip;
                    existing.chapter = subStr;
                }
                else
                {
                    _questions.Add(new QuestionData
                    {
                        questionID = qId,
                        stt = _questions.Count + 1,
                        chuongId = cId,
                        tenChuong = cName,
                        baiID = bId,
                        tenBai = bName,
                        loaiCauHoi = "Trắc nghiệm",
                        mucDo = mucDo,
                        questionText = qText,
                        answers = new string[] { a, b, c, d },
                        correctAnswerIndex = correctIdx,
                        explanation = explain,
                        meoGhiNho = tip,
                        chapter = subStr
                    });
                }
                count++;
            }
            return count;
        }

        private int ParseAndMergeEssayQuestions(string csvText, TeacherSubject subject = TeacherSubject.Physics)
        {
            var rows = ParseCsvLines(csvText);
            if (rows.Count <= 1) return 0;
            int count = 0;
            string subStr = SubjectToString(subject);

            for (int r = 1; r < rows.Count; r++)
            {
                var cols = rows[r];
                if (cols.Count < 11) continue;

                string qId = cols[1].Trim();
                string cId = cols[2].Trim();
                string cName = cols[3].Trim();
                string bId = cols[4].Trim();
                string bName = cols[5].Trim();
                string mucDo = cols[8].Trim();
                string qText = cols[9].Trim();
                string keywords = cols[10].Trim();
                string sampleAns = cols.Count > 11 ? cols[11].Trim() : "";
                string explain = cols.Count > 12 ? cols[12].Trim() : "";

                if (string.IsNullOrEmpty(qId) || string.IsNullOrEmpty(qText)) continue;

                var existing = _questions.FirstOrDefault(q => q.questionID == qId);
                if (existing != null)
                {
                    existing.chuongId = cId;
                    existing.tenChuong = cName;
                    existing.baiID = bId;
                    existing.tenBai = bName;
                    existing.loaiCauHoi = "Tự luận";
                    existing.mucDo = mucDo;
                    existing.questionText = qText;
                    existing.tieuChiCham = keywords;
                    existing.dapAnThamChieu = sampleAns;
                    existing.explanation = explain;
                    existing.answers = new string[] { "Tự luận", "-", "-", "-" };
                    existing.correctAnswerIndex = 0;
                    existing.chapter = subStr;
                }
                else
                {
                    _questions.Add(new QuestionData
                    {
                        questionID = qId,
                        stt = _questions.Count + 1,
                        chuongId = cId,
                        tenChuong = cName,
                        baiID = bId,
                        tenBai = bName,
                        loaiCauHoi = "Tự luận",
                        mucDo = mucDo,
                        questionText = qText,
                        tieuChiCham = keywords,
                        dapAnThamChieu = sampleAns,
                        explanation = explain,
                        answers = new string[] { "Tự luận", "-", "-", "-" },
                        correctAnswerIndex = 0,
                        chapter = subStr
                    });
                }
                count++;
            }
            return count;
        }

        private int ParseAndMergeBioMCQuestions(string csvText)
        {
            var rows = ParseCsvLines(csvText);
            if (rows.Count <= 1) return 0;
            int count = 0;

            for (int r = 1; r < rows.Count; r++)
            {
                var cols = rows[r];
                if (cols.Count < 10) continue;

                string qId = cols[0].Trim();
                string bId = cols[1].Trim();
                string rawMucDo = cols[2].Trim();
                string qText = cols[3].Trim();
                string a = cols.Count > 5 ? cols[5].Trim() : "";
                string b = cols.Count > 6 ? cols[6].Trim() : "";
                string c = cols.Count > 7 ? cols[7].Trim() : "";
                string d = cols.Count > 8 ? cols[8].Trim() : "";
                string correct = cols.Count > 9 ? cols[9].Trim().ToUpperInvariant() : "A";
                string explain = cols.Count > 10 ? cols[10].Trim() : "";
                string tip = cols.Count > 11 ? cols[11].Trim() : "";

                if (string.IsNullOrEmpty(qId) || string.IsNullOrEmpty(qText)) continue;

                int correctIdx = 0;
                if (correct == "B" || correct == "1") correctIdx = 1;
                else if (correct == "C" || correct == "2") correctIdx = 2;
                else if (correct == "D" || correct == "3") correctIdx = 3;

                string mucDo = "Nhận biết";
                string mLower = rawMucDo.ToLower();
                if (mLower.Contains("trung") || mLower.Contains("hieu")) mucDo = "Thông hiểu";
                else if (mLower.Contains("kho") || mLower.Contains("van dung")) mucDo = "Vận dụng";
                else if (mLower.Contains("tong hop") || mLower.Contains("cao")) mucDo = "Vận dụng cao";

                string cId = "C11";
                string cName = "Chương XI. Di truyền học Mendel & Cơ sở phân tử";
                int bNum = 36;
                var match = System.Text.RegularExpressions.Regex.Match(bId, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int bn)) bNum = bn;

                if (bNum >= 41 && bNum <= 45) { cId = "C12"; cName = "Chương XII. Di truyền nhiễm sắc thể"; }
                else if (bNum >= 46 && bNum <= 48) { cId = "C13"; cName = "Chương XIII. Di truyền học với đời sống"; }
                else if (bNum >= 49) { cId = "C14"; cName = "Chương XIV. Tiến hóa"; }

                var existing = _questions.FirstOrDefault(q => q.questionID == qId);
                if (existing != null)
                {
                    existing.chuongId = cId;
                    existing.tenChuong = cName;
                    existing.baiID = bId;
                    existing.mucDo = mucDo;
                    existing.questionText = qText;
                    existing.answers = new string[] { a, b, c, d };
                    existing.correctAnswerIndex = correctIdx;
                    existing.explanation = explain;
                    existing.meoGhiNho = tip;
                    existing.loaiCauHoi = "Trắc nghiệm";
                    existing.chapter = "Biology";
                }
                else
                {
                    _questions.Add(new QuestionData
                    {
                        questionID = qId,
                        chuongId = cId,
                        tenChuong = cName,
                        baiID = bId,
                        tenBai = $"Bài {bNum}",
                        mucDo = mucDo,
                        questionText = qText,
                        answers = new string[] { a, b, c, d },
                        correctAnswerIndex = correctIdx,
                        explanation = explain,
                        meoGhiNho = tip,
                        loaiCauHoi = "Trắc nghiệm",
                        maxScore = 10,
                        trangThai = "Đang áp dụng",
                        giaoVienID = "GV_BIO",
                        nguonTao = "GoogleSheet",
                        ngayTao = DateTime.Now.ToString("yyyy-MM-dd"),
                        chapter = "Biology"
                    });
                }
                count++;
            }
            return count;
        }

        private int ParseAndMergeBioEssayQuestions(string csvText)
        {
            var rows = ParseCsvLines(csvText);
            if (rows.Count <= 1) return 0;
            int count = 0;

            for (int r = 1; r < rows.Count; r++)
            {
                var cols = rows[r];
                if (cols.Count < 4) continue;

                string qId = cols[0].Trim();
                string bId = cols[1].Trim();
                string rawMucDo = cols[2].Trim();
                string qText = cols[3].Trim();
                string sampleAns = cols.Count > 5 ? cols[5].Trim() : "";
                string rubric = cols.Count > 6 ? cols[6].Trim() : "";

                if (string.IsNullOrEmpty(qId) || string.IsNullOrEmpty(qText)) continue;

                string mucDo = "Nhận biết";
                string mLower = rawMucDo.ToLower();
                if (mLower.Contains("trung") || mLower.Contains("hieu")) mucDo = "Thông hiểu";
                else if (mLower.Contains("kho") || mLower.Contains("van dung")) mucDo = "Vận dụng";
                else if (mLower.Contains("tong hop") || mLower.Contains("cao")) mucDo = "Vận dụng cao";

                string cId = "C11";
                string cName = "Chương XI. Di truyền học Mendel & Cơ sở phân tử";
                int bNum = 36;
                var match = System.Text.RegularExpressions.Regex.Match(bId, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int bn)) bNum = bn;

                if (bNum >= 41 && bNum <= 45) { cId = "C12"; cName = "Chương XII. Di truyền nhiễm sắc thể"; }
                else if (bNum >= 46 && bNum <= 48) { cId = "C13"; cName = "Chương XIII. Di truyền học với đời sống"; }
                else if (bNum >= 49) { cId = "C14"; cName = "Chương XIV. Tiến hóa"; }

                var existing = _questions.FirstOrDefault(q => q.questionID == qId);
                if (existing != null)
                {
                    existing.chuongId = cId;
                    existing.tenChuong = cName;
                    existing.baiID = bId;
                    existing.mucDo = mucDo;
                    existing.questionText = qText;
                    existing.loaiCauHoi = "Tự luận";
                    existing.dapAnThamChieu = sampleAns;
                    existing.tieuChiCham = rubric;
                    existing.chapter = "Biology";
                }
                else
                {
                    _questions.Add(new QuestionData
                    {
                        questionID = qId,
                        chuongId = cId,
                        tenChuong = cName,
                        baiID = bId,
                        tenBai = $"Bài {bNum}",
                        mucDo = mucDo,
                        questionText = qText,
                        loaiCauHoi = "Tự luận",
                        dapAnThamChieu = sampleAns,
                        tieuChiCham = rubric,
                        answers = new string[] { "Tự luận", "-", "-", "-" },
                        correctAnswerIndex = 0,
                        maxScore = 10,
                        trangThai = "Đang áp dụng",
                        giaoVienID = "GV_BIO",
                        nguonTao = "GoogleSheet",
                        ngayTao = DateTime.Now.ToString("yyyy-MM-dd"),
                        chapter = "Biology"
                    });
                }
                count++;
            }
            return count;
        }

        private static List<List<string>> ParseCsvLines(string csvText)
        {
            List<List<string>> result = new List<List<string>>();
            if (string.IsNullOrEmpty(csvText)) return result;

            List<string> currentRow = new List<string>();
            System.Text.StringBuilder currentCell = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                        {
                            currentCell.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentCell.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        currentRow.Add(currentCell.ToString().Trim());
                        currentCell.Clear();
                    }
                    else if (c == '\r')
                    {
                        // ignore
                    }
                    else if (c == '\n')
                    {
                        currentRow.Add(currentCell.ToString().Trim());
                        currentCell.Clear();
                        result.Add(currentRow);
                        currentRow = new List<string>();
                    }
                    else
                    {
                        currentCell.Append(c);
                    }
                }
            }
            if (currentCell.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentCell.ToString().Trim());
                result.Add(currentRow);
            }
            return result;
        }

        #endregion

        #region ===== 4. QUẢN LÝ TIẾN ĐỘ HỌC SINH (PROGRESS) =====

        private void LoadStudentProgress()
        {
            _studentProgress.Clear();

            // 1. Nạp từ real_achievements_data.json
            TextAsset achAsset = Resources.Load<TextAsset>("QuizData/real_achievements_data");
            if (achAsset != null && !string.IsNullOrEmpty(achAsset.text))
            {
                try
                {
                    var ds = JsonUtility.FromJson<PhysicsGame1.RealAchievementDataset>(achAsset.text);
                    if (ds != null && ds.students != null)
                    {
                        foreach (var s in ds.students)
                        {
                            _studentProgress.Add(new StudentProgressItem
                            {
                                studentId = s.avatarId,
                                studentName = s.playerName,
                                className = s.className,
                                subject = "Physics",
                                lessonName = s.lesson,
                                score = s.score,
                                accuracy = s.accuracy,
                                correctAnswers = (int)(s.score / 10),
                                totalQuestions = 10,
                                timeUsed = s.timeUsed,
                                date = s.date
                            });
                        }
                    }
                }
                catch { }
            }

            // 2. Nạp thêm từ ProgressSyncManager nếu đã có cache
            if (ProgressSyncManager.Instance != null && ProgressSyncManager.Instance.progressData.Count > 0)
            {
                AppendProgressSyncData();
            }

            // 3. Bổ sung dữ liệu mẫu thực tế cho Sinh học và Hóa học nếu chưa có
            AddDefaultBioAndChemProgress();
        }

        private void AppendProgressSyncData()
        {
            foreach (var kvp in ProgressSyncManager.Instance.progressData)
            {
                var p = kvp.Value;
                string subName = StringToSubject(p.monID).ToString();

                var existing = _studentProgress.FirstOrDefault(s => s.studentName == p.userID && s.subject == subName && s.lessonName == p.baiID);
                if (existing != null)
                {
                    existing.score = p.diemCaoNhat;
                    existing.accuracy = p.tyLeDung;
                    existing.date = p.thoiGian;
                }
                else
                {
                    _studentProgress.Add(new StudentProgressItem
                    {
                        studentId = p.userID,
                        studentName = p.userID,
                        className = "9A1",
                        subject = subName,
                        lessonName = p.baiID,
                        score = p.diemCaoNhat,
                        accuracy = p.tyLeDung,
                        timeUsed = 120f,
                        date = p.thoiGian
                    });
                }
            }
        }

        private void AddDefaultBioAndChemProgress()
        {
            // Sinh học
            if (!_studentProgress.Any(s => s.subject == "Biology"))
            {
                _studentProgress.Add(new StudentProgressItem { studentName = "Nguyễn Minh Khang", className = "9A2", subject = "Biology", lessonName = "Bài 38. ADN & Gene", score = 95, accuracy = 95f, timeUsed = 94f, date = "2026-10-28 09:15" });
                _studentProgress.Add(new StudentProgressItem { studentName = "Trần Bảo Ngọc", className = "9A1", subject = "Biology", lessonName = "Bài 38. ADN & Gene", score = 90, accuracy = 90f, timeUsed = 112f, date = "2026-10-28 09:20" });
                _studentProgress.Add(new StudentProgressItem { studentName = "Lê Hoàng Long", className = "9A3", subject = "Biology", lessonName = "Bài 39. Tái bản DNA", score = 80, accuracy = 80f, timeUsed = 145f, date = "2026-10-28 10:05" });
                _studentProgress.Add(new StudentProgressItem { studentName = "Phạm Quỳnh Anh", className = "9A2", subject = "Biology", lessonName = "Bài 37. Quy luật Mendel", score = 70, accuracy = 70f, timeUsed = 180f, date = "2026-10-27 15:30" });
            }

            // Hóa học
            if (!_studentProgress.Any(s => s.subject == "Chemistry"))
            {
                _studentProgress.Add(new StudentProgressItem { studentName = "Vũ Đình Trọng", className = "9A1", subject = "Chemistry", lessonName = "Bài 15. Dãy hoạt động", score = 100, accuracy = 100f, timeUsed = 85f, date = "2026-10-29 14:10" });
                _studentProgress.Add(new StudentProgressItem { studentName = "Hoàng Thị Mai", className = "9A2", subject = "Chemistry", lessonName = "Bài 14. Tính chất kim loại", score = 85, accuracy = 85f, timeUsed = 105f, date = "2026-10-29 14:22" });
                _studentProgress.Add(new StudentProgressItem { studentName = "Đỗ Gia Huy", className = "9A1", subject = "Chemistry", lessonName = "Bài 20. Metan & Hữu cơ", score = 75, accuracy = 75f, timeUsed = 130f, date = "2026-10-28 16:45" });
            }
        }

        public List<StudentProgressItem> GetStudentProgress(string subject)
        {
            return GetStudentProgress(StringToSubject(subject));
        }

        public List<StudentProgressItem> GetStudentProgress(TeacherSubject sub)
        {
            string subStr = SubjectToString(sub);
            // Xếp hạng: Ưu tiên điểm cao hơn, nếu bằng điểm thì thời gian hoàn thành ngắn hơn
            return _studentProgress.Where(s => s.subject.Equals(subStr, StringComparison.OrdinalIgnoreCase))
                                   .OrderByDescending(s => s.score)
                                   .ThenBy(s => s.timeUsed)
                                   .ToList();
        }

        public void SyncProgressFromGoogleSheet(Action<bool> onComplete)
        {
            if (ProgressSyncManager.Instance != null)
            {
                StartCoroutine(ProgressSyncManager.Instance.FetchProgressFromSheet((success) =>
                {
                    if (success)
                    {
                        AppendProgressSyncData();
                    }
                    onComplete?.Invoke(success);
                }));
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        #endregion
    }
}
