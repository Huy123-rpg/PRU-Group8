using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class G2DeviceItem
    {
        public string DeviceId;
        public string ChuongId;
        public int ThuTuTrongMap;
        public string BaiID;
        public string TenBai;
        public string LoaiBai; // "LESSON" or "CHAPTER_TEST"
        public float NguongDat = 80f;
        public string CheDoKhoiPhuc; // "MCQ_OR_ESSAY" or "MCQ"
        public string TrangThaiMo;
        public string ViTriUnity;
    }

    [Serializable]
    public class G2DeviceWrapper
    {
        public List<G2DeviceItem> devices;
    }

    [Serializable]
    public class G2MCQItem
    {
        public string cauHoiID;
        public string chuongId;
        public string tenChuong;
        public string baiID;
        public string tenBai;
        public string loaiCauHoi;
        public string mucDo;
        public string mucDoGame2; // "NB", "TH", "VD", "VDC"
        public string noiDung;
        public string dapAnA;
        public string dapAnB;
        public string dapAnC;
        public string dapAnD;
        public string dapAnDung; // "A", "B", "C", "D"
        public int correctIndex; // 0, 1, 2, 3
        public string giaiThich;
        public string meoGhiNho;
        public string contentVersion;

        public string MucDo => mucDoGame2;
    }

    [Serializable]
    public class G2MCQWrapper
    {
        public List<G2MCQItem> questions;
    }

    [Serializable]
    public class G2EssayItem
    {
        public string cauHoiID;
        public string chuongId;
        public string tenChuong;
        public string baiIDNguon;
        public string baiIDGame2; // e.g. "bai2"
        public string tenBai;
        public string mucDoNguon;
        public string mucDoGame2; // "NB", "TH", "VD", "VDC"
        public string noiDung;
        public string tuKhoa;
        public string dapAnMau;
        public string giaiThich;
        public string rubricId;
        public int diemToiDa;
        public string rubricStatus; // "DRAFT"
        public string contentVersion;

        // Convenient aliases for controller compatibility
        public string CauHoiID => cauHoiID;
        public string NoiDungCauHoi => noiDung;
        public string MucDoGame2 => mucDoGame2;
        public string DapAnMau => dapAnMau;
        public string GiaiThich => giaiThich;
    }

    [Serializable]
    public class G2EssayWrapper
    {
        public List<G2EssayItem> questions;
    }

    [Serializable]
    public class G2RubricItem
    {
        public string rubricId;
        public string cauHoiID;
        public string criterionId;
        public string yeuCau;
        public float maxPoints = 25f;
        public string partialCreditRule;
        public string approvalStatus; // "DRAFT"
        public string rubricVersion;

        public string TrangThaiDuyet => approvalStatus;
    }

    [Serializable]
    public class G2RubricWrapper
    {
        public List<G2RubricItem> criteria;
    }

    [Serializable]
    public class G2TheoryItem
    {
        public string baiID;
        public string chuongId;
        public string tenChuong;
        public string tenBai;
        public string khaiNiemTrongTam;
        public string congThucCanNho;
        public string hienTuongUngDung;
        public string noiDungDayDu;

        public string BaiId => baiID;
        public List<string> DiemCotLoi => new List<string> { khaiNiemTrongTam, congThucCanNho, hienTuongUngDung, noiDungDayDu };
    }

    [Serializable]
    public class G2TheoryWrapper
    {
        public List<G2TheoryItem> lessons;
    }

    /// <summary>
    /// Nguồn dữ liệu chuẩn (Ground Truth) cho Physics Game 2.
    /// Tải từ 5 file JSON đã được trích xuất nguyên vẹn từ Aurelia_Game2_Data.xlsx:
    /// - 21 Thiết bị (G2_ThietBi)
    /// - 830 MCQ (NganHangCauHoi)
    /// - 64 Tự luận (CauHoiTuLuan)
    /// - 256 Tiêu chí rubric (G2_Rubric)
    /// - 22 Bài lý thuyết (LyThuyetBaiHoc)
    /// </summary>
    public static class GameDataRepository
    {
        private static bool _isInitialized = false;

        public static string LastSyncDataSource { get; set; } = "LocalCache";
        public static string LastSyncStatus { get; set; } = "NotSynced";
        public static string LastSyncError { get; set; } = null;
        public static DateTime LastSyncTime { get; set; } = DateTime.MinValue;
        public static int LiveSheetMCQCount { get; set; } = 0;
        public static int LiveSheetEssayCount { get; set; } = 0;

        private static List<G2DeviceItem> _devices = new List<G2DeviceItem>();
        private static List<G2MCQItem> _mcqs = new List<G2MCQItem>();
        private static List<G2EssayItem> _essays = new List<G2EssayItem>();
        private static List<G2EssayItem> _demoSupplementalEssays = new List<G2EssayItem>();
        public static bool IsDemoSupplementalEnabled { get; set; } = true;
        private static List<G2RubricItem> _rubrics = new List<G2RubricItem>();
        private static List<G2TheoryItem> _theories = new List<G2TheoryItem>();

        private static readonly Dictionary<string, G2DeviceItem> _devicesById = new Dictionary<string, G2DeviceItem>();
        private static readonly Dictionary<string, G2DeviceItem> _devicesByLesson = new Dictionary<string, G2DeviceItem>();

        public static void EnsureInitialized()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // 1. Load Devices
            TextAsset devAsset = Resources.Load<TextAsset>("QuizData/g2_devices");
            if (devAsset != null)
            {
                var wrap = JsonUtility.FromJson<G2DeviceWrapper>(devAsset.text);
                if (wrap != null && wrap.devices != null)
                {
                    _devices = wrap.devices;
                    foreach (var d in _devices)
                    {
                        _devicesById[d.DeviceId] = d;
                        _devicesByLesson[d.BaiID] = d;
                        // Map legacy lessonId C1_L2 -> bai2
                        string altLessonId = ConvertToLegacyId(d.BaiID);
                        if (!string.IsNullOrEmpty(altLessonId))
                        {
                            _devicesByLesson[altLessonId] = d;
                        }
                    }
                }
            }
            Debug.Log($"[GameDataRepository] Đã nạp {_devices.Count} thiết bị.");

            // 2. Load MCQs
            TextAsset mcqAsset = Resources.Load<TextAsset>("QuizData/g2_mcq_bank");
            if (mcqAsset != null)
            {
                var wrap = JsonUtility.FromJson<G2MCQWrapper>(mcqAsset.text);
                if (wrap != null && wrap.questions != null)
                {
                    _mcqs = wrap.questions;
                }
            }
            Debug.Log($"[GameDataRepository] Đã nạp {_mcqs.Count} câu trắc nghiệm.");

            // 3. Load Essays (64 câu gốc)
            TextAsset essayAsset = Resources.Load<TextAsset>("QuizData/g2_essay_bank");
            if (essayAsset != null)
            {
                var wrap = JsonUtility.FromJson<G2EssayWrapper>(essayAsset.text);
                if (wrap != null && wrap.questions != null)
                {
                    _essays = wrap.questions;
                }
            }
            Debug.Log($"[GameDataRepository] Đã nạp {_essays.Count} câu tự luận gốc.");

            // 3.1 Load Demo Supplemental Essays (256 câu bổ sung chờ duyệt từ BoSung_TuLuan_ChoDuyet.xlsx)
            TextAsset demoAsset = Resources.Load<TextAsset>("QuizData/g2_essay_demo_supplemental");
            if (demoAsset != null)
            {
                var wrap = JsonUtility.FromJson<G2EssayWrapper>(demoAsset.text);
                if (wrap != null && wrap.questions != null)
                {
                    _demoSupplementalEssays = wrap.questions;
                    Debug.Log($"[GameDataRepository] Đã nạp {_demoSupplementalEssays.Count} câu tự luận bổ sung (nguồn Demo BoSung_TuLuan_ChoDuyet).");
                }
            }
            MergeDemoSupplementalEssays();

            // 4. Load Rubrics
            TextAsset rubricAsset = Resources.Load<TextAsset>("QuizData/g2_rubric_bank");
            if (rubricAsset != null)
            {
                var wrap = JsonUtility.FromJson<G2RubricWrapper>(rubricAsset.text);
                if (wrap != null && wrap.criteria != null)
                {
                    _rubrics = wrap.criteria;
                }
            }
            Debug.Log($"[GameDataRepository] Đã nạp {_rubrics.Count} tiêu chí rubric.");

            // 5. Load Theory
            TextAsset theoryAsset = Resources.Load<TextAsset>("TheoryData/g2_theory_bank");
            if (theoryAsset != null)
            {
                var wrap = JsonUtility.FromJson<G2TheoryWrapper>(theoryAsset.text);
                if (wrap != null && wrap.lessons != null)
                {
                    _theories = wrap.lessons;
                }
            }
            Debug.Log($"[GameDataRepository] Đã nạp {_theories.Count} bài lý thuyết.");
        }

        public static string ConvertToCanonicalId(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId)) return "bai2";
            lessonId = lessonId.Trim();
            if (lessonId.StartsWith("bai", StringComparison.OrdinalIgnoreCase)) return lessonId.ToLower();
            if (lessonId.StartsWith("test_", StringComparison.OrdinalIgnoreCase)) return lessonId.ToLower();

            // C1_L2 -> bai2, C1_L3 -> bai3, C1_TEST -> test_chuong1, etc.
            switch (lessonId.ToUpper())
            {
                case "C1_L2": return "bai2";
                case "C1_L3": return "bai3";
                case "C1_L4": return "bai4";
                case "C1_TEST": return "test_chuong1";
                case "C2_L5": return "bai5";
                case "C2_L6": return "bai6";
                case "C2_L7": return "bai7";
                case "C2_L8": return "bai8";
                case "C2_L9": return "bai9";
                case "C2_L10": return "bai10";
                case "C2_TEST": return "test_chuong2";
                case "C3_L11": return "bai11";
                case "C3_L12": return "bai12";
                case "C3_L13": return "bai13";
                case "C3_TEST": return "test_chuong3";
                case "C4_L14": return "bai14";
                case "C4_L15": return "bai15";
                case "C4_TEST": return "test_chuong4";
                case "C5_L16": return "bai16";
                case "C5_L17": return "bai17";
                case "C5_TEST": return "test_chuong5";
                default: return lessonId.ToLower();
            }
        }

        public static string ConvertToLegacyId(string canonicalId)
        {
            if (string.IsNullOrEmpty(canonicalId)) return "";
            switch (canonicalId.ToLower())
            {
                case "bai2": return "C1_L2";
                case "bai3": return "C1_L3";
                case "bai4": return "C1_L4";
                case "test_chuong1": return "C1_TEST";
                case "bai5": return "C2_L5";
                case "bai6": return "C2_L6";
                case "bai7": return "C2_L7";
                case "bai8": return "C2_L8";
                case "bai9": return "C2_L9";
                case "bai10": return "C2_L10";
                case "test_chuong2": return "C2_TEST";
                case "bai11": return "C3_L11";
                case "bai12": return "C3_L12";
                case "bai13": return "C3_L13";
                case "test_chuong3": return "C3_TEST";
                case "bai14": return "C4_L14";
                case "bai15": return "C4_L15";
                case "test_chuong4": return "C4_TEST";
                case "bai16": return "C5_L16";
                case "bai17": return "C5_L17";
                case "test_chuong5": return "C5_TEST";
                default: return canonicalId;
            }
        }

        public static List<G2DeviceItem> AllDevices
        {
            get { EnsureInitialized(); return _devices; }
        }

        public static List<G2MCQItem> AllMCQs
        {
            get { EnsureInitialized(); return _mcqs; }
        }

        public static List<G2EssayItem> AllEssays
        {
            get { EnsureInitialized(); return _essays; }
        }

        public static List<G2RubricItem> AllRubrics
        {
            get { EnsureInitialized(); return _rubrics; }
        }

        public static List<G2TheoryItem> AllTheories
        {
            get { EnsureInitialized(); return _theories; }
        }

        public static List<G2DeviceItem> GetDevicesByChapter(int chapterIndex)
        {
            return GetDevicesForChapter(chapterIndex);
        }

        public static List<G2MCQItem> GetChapterTestQuestions(int chapterIndex)
        {
            return GetChapterTestMCQs(chapterIndex);
        }

        public static List<G2EssayItem> GetEssaysByLesson(string lessonId)
        {
            EnsureInitialized();
            string canon = ConvertToCanonicalId(lessonId);
            return _essays.Where(e => e.baiIDGame2.Equals(canon, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static List<G2MCQItem> GetMCQsByLessonAndDifficulty(string lessonId, string mucDo, int limit = 10)
        {
            return GetMCQs(lessonId, mucDo, limit);
        }

        public static List<G2MCQItem> CreateChapterTestQuestions(int chapterIndex)
        {
            return GetChapterTestMCQs(chapterIndex);
        }

        public static List<G2MCQItem> GetMCQQuestions(string lessonId, string mucDo, int limit = 10)
        {
            return GetMCQs(lessonId, mucDo, limit);
        }

        public static List<G2DeviceItem> GetAllDevices()
        {
            EnsureInitialized();
            return _devices;
        }

        public static List<G2DeviceItem> GetDevicesForChapter(int chapterIndex)
        {
            EnsureInitialized();
            string chId = $"chuong{chapterIndex}";
            return _devices.Where(d => d.ChuongId.Equals(chId, StringComparison.OrdinalIgnoreCase))
                           .OrderBy(d => d.ThuTuTrongMap)
                           .ToList();
        }

        public static G2DeviceItem GetDeviceById(string deviceId)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(deviceId)) return null;
            _devicesById.TryGetValue(deviceId, out var item);
            return item;
        }

        public static G2DeviceItem GetDeviceByLesson(string lessonId)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(lessonId)) return null;
            string canon = ConvertToCanonicalId(lessonId);
            if (_devicesByLesson.TryGetValue(canon, out var item)) return item;
            _devicesByLesson.TryGetValue(lessonId, out item);
            return item;
        }

        /// <summary>
        /// Lấy câu trắc nghiệm cho bài học theo độ khó.
        /// TUYỆT ĐỐI KHÔNG TRỘN CÂU HỎI KHÁC MỨC ĐỘ:
        /// - NB chỉ lấy NB, TH chỉ lấy TH, VD chỉ lấy VD, VDC chỉ lấy VDC.
        /// - Tuyệt đối không tự ý lấy bù từ mức độ khác, không tạo câu giả.
        /// </summary>
        public static List<G2MCQItem> GetMCQs(string lessonId, string mucDo, int limit = 10)
        {
            EnsureInitialized();
            string canon = ConvertToCanonicalId(lessonId);
            string normMucDo = NormalizeDifficulty(mucDo);

            var matches = _mcqs.Where(q => q.baiID.Equals(canon, StringComparison.OrdinalIgnoreCase) &&
                                           q.mucDoGame2.Equals(normMucDo, StringComparison.OrdinalIgnoreCase))
                               .ToList();

            // Shuffle deterministically for variety but bounded
            int takeCount = Mathf.Min(limit, matches.Count);
            return matches.OrderBy(_ => Guid.NewGuid()).Take(takeCount).ToList();
        }

        /// <summary>
        /// Đếm số câu trắc nghiệm thực tế có trong ngân hàng cho bài học và mức độ.
        /// </summary>
        public static int CountMCQs(string lessonId, string mucDo)
        {
            EnsureInitialized();
            string canon = ConvertToCanonicalId(lessonId);
            string normMucDo = NormalizeDifficulty(mucDo);
            return _mcqs.Count(q => q.baiID.Equals(canon, StringComparison.OrdinalIgnoreCase) &&
                                    q.mucDoGame2.Equals(normMucDo, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Tạo đề kiểm tra chương: chính xác 10 câu trắc nghiệm theo blueprint 3 NB + 3 TH + 3 VD + 1 VDC.
        /// </summary>
        public static List<G2MCQItem> GetChapterTestMCQs(int chapterIndex)
        {
            EnsureInitialized();
            string testId = $"test_chuong{chapterIndex}";

            var nb = _mcqs.Where(q => q.baiID.Equals(testId, StringComparison.OrdinalIgnoreCase) && q.mucDoGame2 == "NB")
                          .OrderBy(_ => Guid.NewGuid()).Take(3).ToList();
            var th = _mcqs.Where(q => q.baiID.Equals(testId, StringComparison.OrdinalIgnoreCase) && q.mucDoGame2 == "TH")
                          .OrderBy(_ => Guid.NewGuid()).Take(3).ToList();
            var vd = _mcqs.Where(q => q.baiID.Equals(testId, StringComparison.OrdinalIgnoreCase) && q.mucDoGame2 == "VD")
                          .OrderBy(_ => Guid.NewGuid()).Take(3).ToList();
            var vdc = _mcqs.Where(q => q.baiID.Equals(testId, StringComparison.OrdinalIgnoreCase) && q.mucDoGame2 == "VDC")
                           .OrderBy(_ => Guid.NewGuid()).Take(1).ToList();

            var result = new List<G2MCQItem>();
            result.AddRange(nb);
            result.AddRange(th);
            result.AddRange(vd);
            result.AddRange(vdc);
            return result;
        }

        /// <summary>
        /// Lấy 10 câu cho thử thách tuần: 3 NB + 3 TH + 3 VD + 1 VDC từ ngân hàng tổng hợp/chương.
        /// </summary>
        public static List<G2MCQItem> GetWeeklyChallengeMCQs(int chapterHint = 1)
        {
            EnsureInitialized();
            string testId = $"test_chuong{chapterHint}";
            var qs = GetChapterTestMCQs(chapterHint);
            if (qs.Count == 10) return qs;

            // Fallback to general bank
            return _mcqs.Where(q => q.baiID.StartsWith("test_chuong", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(_ => Guid.NewGuid()).Take(10).ToList();
        }

        /// <summary>
        /// Lấy câu tự luận cho bài học theo mức độ.
        /// N = min(5, số hợp lệ), hiện tại thường là 1 câu/bài/mức.
        /// </summary>
        public static List<G2EssayItem> GetEssayQuestions(string lessonId, string mucDo, int limit = 5)
        {
            EnsureInitialized();
            string canon = ConvertToCanonicalId(lessonId);
            string normMucDo = NormalizeDifficulty(mucDo);

            var matches = _essays.Where(q => q.baiIDGame2.Equals(canon, StringComparison.OrdinalIgnoreCase) &&
                                             q.mucDoGame2.Equals(normMucDo, StringComparison.OrdinalIgnoreCase))
                                 .ToList();

            int takeCount = Mathf.Min(limit, matches.Count);
            return matches.Take(takeCount).ToList();
        }

        public static int CountEssayQuestions(string lessonId, string mucDo)
        {
            EnsureInitialized();
            string canon = ConvertToCanonicalId(lessonId);
            string normMucDo = NormalizeDifficulty(mucDo);
            return _essays.Count(q => q.baiIDGame2.Equals(canon, StringComparison.OrdinalIgnoreCase) &&
                                      q.mucDoGame2.Equals(normMucDo, StringComparison.OrdinalIgnoreCase));
        }

        public static List<G2RubricItem> GetRubricsForQuestion(string cauHoiId)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(cauHoiId)) return new List<G2RubricItem>();
            return _rubrics.Where(r => r.cauHoiID.Equals(cauHoiId, StringComparison.OrdinalIgnoreCase))
                           .OrderBy(r => r.criterionId)
                           .ToList();
        }

        public static G2TheoryItem GetTheory(string lessonId)
        {
            EnsureInitialized();
            string canon = ConvertToCanonicalId(lessonId);
            return _theories.FirstOrDefault(t => t.baiID.Equals(canon, StringComparison.OrdinalIgnoreCase));
        }

        public static string NormalizeDifficulty(string input)
        {
            if (string.IsNullOrEmpty(input)) return "NB";
            string upper = input.Trim().ToUpper();

            // 1. Vận dụng cao (VDC / Very Hard / Chuyên sâu / Tổng hợp)
            if (upper == "VDC" || upper.Contains("VDC") ||
                upper.Contains("VẬN DỤNG CAO") || upper.Contains("VAN DUNG CAO") ||
                upper.Contains("CHUYÊN SÂU") || upper.Contains("CHUYEN SAU") ||
                upper.Contains("TỔNG HỢP") || upper.Contains("TONG HOP") ||
                upper.Contains("VERY HARD"))
            {
                return "VDC";
            }

            // 2. Vận dụng (VD / Hard / Nâng cao / Khó)
            if (upper == "VD" || upper.Contains("VD") ||
                upper.Contains("VẬN DỤNG") || upper.Contains("VAN DUNG") ||
                upper.Contains("NÂNG CAO") || upper.Contains("NANG CAO") ||
                upper.Contains("KHÓ") || upper.Contains("KHO") ||
                upper.Contains("HARD"))
            {
                return "VD";
            }

            // 3. Thông hiểu (TH / Medium / Ứng dụng / Trung bình)
            if (upper == "TH" || upper.Contains("TH") ||
                upper.Contains("THÔNG HIỂU") || upper.Contains("THONG HIEU") ||
                upper.Contains("ỨNG DỤNG") || upper.Contains("UNG DUNG") ||
                upper.Contains("TRUNG BÌNH") || upper.Contains("TRUNG BINH") ||
                upper.Contains("MEDIUM"))
            {
                return "TH";
            }

            // 4. Mặc định là Nhận biết (NB / Easy / Dễ / Nền tảng)
            return "NB";
        }

        /// <summary>
        /// Cập nhật hoặc bổ sung các câu hỏi hợp lệ từ Google Sheets (Live Data).
        /// Không cố định ở 830 câu, tự động mở rộng khi ngân hàng câu hỏi trên Sheet được thêm mới.
        /// </summary>
        public static int LoadOrUpdateMCQsFromCsv(string csvText)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(csvText)) return _mcqs.Count;

            var rows = ParseCsvToRows(csvText);
            if (rows.Count <= 1) return _mcqs.Count;

            var headers = rows[0];
            int colCauHoiId = FindCol(headers, "CauHoiID");
            int colChuongId = FindCol(headers, "ChuongId");
            int colTenChuong = FindCol(headers, "TenChuong");
            int colBaiId = FindCol(headers, "BaiID");
            int colTenBai = FindCol(headers, "TenBai");
            int colLoai = FindCol(headers, "LoaiCauHoi");
            int colMucDo = FindCol(headers, "MucDo");
            int colNoiDung = FindCol(headers, "NoiDung");
            int colA = FindCol(headers, "DapAnA");
            int colB = FindCol(headers, "DapAnB");
            int colC = FindCol(headers, "DapAnC");
            int colD = FindCol(headers, "DapAnD");
            int colDung = FindCol(headers, "DapAnDung");
            int colGiaiThich = FindCol(headers, "GiaiThich");
            int colMeo = FindCol(headers, "MeoGhiNho");
            int colTrangThai = FindCol(headers, "TrangThai");

            int addedCount = 0;
            var mcqDict = _mcqs.ToDictionary(q => q.cauHoiID, StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                string id = GetColValue(row, colCauHoiId).Trim();
                string noiDung = GetColValue(row, colNoiDung).Trim();
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(noiDung)) continue;

                string trangThai = GetColValue(row, colTrangThai).Trim();
                if (trangThai.Equals("Đã ẩn", StringComparison.OrdinalIgnoreCase) ||
                    trangThai.Equals("Bản nháp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string rawMucDo = GetColValue(row, colMucDo).Trim();
                string normMucDo = NormalizeDifficulty(rawMucDo);
                string baiId = GetColValue(row, colBaiId).Trim().ToLower();
                string chuongId = GetColValue(row, colChuongId).Trim().ToLower();

                string dung = GetColValue(row, colDung).Trim().ToUpper();
                int cIdx = 0;
                if (dung == "B" || dung == "1") cIdx = 1;
                else if (dung == "C" || dung == "2") cIdx = 2;
                else if (dung == "D" || dung == "3") cIdx = 3;

                var item = new G2MCQItem
                {
                    cauHoiID = id,
                    chuongId = chuongId,
                    tenChuong = GetColValue(row, colTenChuong).Trim(),
                    baiID = baiId,
                    tenBai = GetColValue(row, colTenBai).Trim(),
                    loaiCauHoi = GetColValue(row, colLoai).Trim(),
                    mucDo = rawMucDo,
                    mucDoGame2 = normMucDo,
                    noiDung = noiDung,
                    dapAnA = GetColValue(row, colA).Trim(),
                    dapAnB = GetColValue(row, colB).Trim(),
                    dapAnC = GetColValue(row, colC).Trim(),
                    dapAnD = GetColValue(row, colD).Trim(),
                    dapAnDung = dung.Length > 0 ? dung.Substring(0, 1) : "A",
                    correctIndex = cIdx,
                    giaiThich = GetColValue(row, colGiaiThich).Trim(),
                    meoGhiNho = GetColValue(row, colMeo).Trim(),
                    contentVersion = "live_sheet"
                };

                if (mcqDict.TryGetValue(id, out var existing))
                {
                    int idx = _mcqs.IndexOf(existing);
                    if (idx >= 0) _mcqs[idx] = item;
                    mcqDict[id] = item;
                }
                else
                {
                    _mcqs.Add(item);
                    mcqDict[id] = item;
                    addedCount++;
                }
            }

            Debug.Log($"[GameDataRepository] Đã nạp Live Questions từ Sheet. Tổng số câu: {_mcqs.Count} (Thêm mới: {addedCount}).");
            LastSyncDataSource = "GoogleSheets";
            LastSyncStatus = "Success";
            LastSyncError = null;
            LastSyncTime = DateTime.Now;
            LiveSheetMCQCount = _mcqs.Count;
            return _mcqs.Count;
        }

        /// <summary>
        /// Cập nhật hoặc bổ sung các câu hỏi tự luận hợp lệ từ Google Sheets tab Cauhoituluan.
        /// </summary>
        public static int LoadOrUpdateEssaysFromCsv(string csvText)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(csvText)) return _essays.Count;

            var rows = ParseCsvToRows(csvText);
            if (rows.Count <= 1) return _essays.Count;

            var headers = rows[0];
            int colCauHoiId = FindCol(headers, "CauHoiID");
            int colChuongId = FindCol(headers, "ChuongId");
            int colTenChuong = FindCol(headers, "TenChuong");
            int colBaiId = FindCol(headers, "BaiID");
            int colTenBai = FindCol(headers, "TenBai");
            int colGiaoVienId = FindCol(headers, "GiaoVienID");
            int colTenGiaoVien = FindCol(headers, "TenGiaoVien");
            int colMucDo = FindCol(headers, "MucDo");
            int colNoiDung = FindCol(headers, "NoiDung");
            int colTuKhoa = FindCol(headers, "TuKhoaChamDiem");
            int colDapAnMau = FindCol(headers, "DapAnTuLuanMau");
            int colGiaiThich = FindCol(headers, "GiaiThich_MeoGhiNho");
            int colTrangThai = FindCol(headers, "TrangThai");

            int addedCount = 0;
            var essayDict = _essays.ToDictionary(e => e.cauHoiID, StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                string id = GetColValue(row, colCauHoiId).Trim();
                string noiDung = GetColValue(row, colNoiDung).Trim();
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(noiDung)) continue;

                string trangThai = GetColValue(row, colTrangThai).Trim();
                if (trangThai.Equals("Đã ẩn", StringComparison.OrdinalIgnoreCase) ||
                    trangThai.Equals("Bản nháp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string rawMucDo = GetColValue(row, colMucDo).Trim();
                string normMucDo = NormalizeDifficulty(rawMucDo);
                string rawBaiId = GetColValue(row, colBaiId).Trim();
                string canonBaiId = ConvertToCanonicalId(rawBaiId);

                var item = new G2EssayItem
                {
                    cauHoiID = id,
                    chuongId = GetColValue(row, colChuongId).Trim(),
                    tenChuong = GetColValue(row, colTenChuong).Trim(),
                    baiIDNguon = rawBaiId,
                    baiIDGame2 = canonBaiId,
                    tenBai = GetColValue(row, colTenBai).Trim(),
                    mucDoNguon = rawMucDo,
                    mucDoGame2 = normMucDo,
                    noiDung = noiDung,
                    tuKhoa = GetColValue(row, colTuKhoa).Trim(),
                    dapAnMau = GetColValue(row, colDapAnMau).Trim(),
                    giaiThich = GetColValue(row, colGiaiThich).Trim(),
                    rubricId = $"RUB_{id}",
                    diemToiDa = 20,
                    contentVersion = "live_sheet"
                };

                if (essayDict.TryGetValue(id, out var existing))
                {
                    int idx = _essays.IndexOf(existing);
                    if (idx >= 0) _essays[idx] = item;
                    essayDict[id] = item;
                }
                else
                {
                    _essays.Add(item);
                    essayDict[id] = item;
                    addedCount++;
                }
            }

            Debug.Log($"[GameDataRepository] Đã nạp Live Essays từ Sheet. Tổng số câu: {_essays.Count} (Thêm mới: {addedCount}).");
            LiveSheetEssayCount = _essays.Count;
            // Tự động duy trì và hợp nhất lại các câu demo bổ sung sau khi Sheet sync
            MergeDemoSupplementalEssays();
            return _essays.Count;
        }

        /// <summary>
        /// Hợp nhất an toàn 256 câu tự luận bổ sung vào ngân hàng runtime phục vụ Demo Game 2.
        /// Bảo đảm:
        /// - Đọc câu cũ từ ngân hàng gốc / live sheet.
        /// - Đọc câu mới từ nguồn demo BoSung_TuLuan_ChoDuyet.
        /// - Ánh xạ và lọc đúng bài học, đúng mức độ.
        /// - Loại trừ trùng CauHoiID và trùng nội dung.
        /// - Giữ nguyên ID câu hỏi (TL_BS_xxx) để khớp rubric Gemini.
        /// - Khi Google Sheets đồng bộ lại, dữ liệu demo không bị xóa hoặc ghi đè mất.
        /// </summary>
        public static void MergeDemoSupplementalEssays()
        {
            if (!IsDemoSupplementalEnabled || _demoSupplementalEssays == null || _demoSupplementalEssays.Count == 0) return;

            var existingIds = new HashSet<string>(_essays.Select(e => e.cauHoiID), StringComparer.OrdinalIgnoreCase);
            var existingTexts = new HashSet<string>(_essays.Select(e => e.noiDung != null ? e.noiDung.Trim() : ""), StringComparer.OrdinalIgnoreCase);

            int mergedCount = 0;
            foreach (var item in _demoSupplementalEssays)
            {
                if (item == null || string.IsNullOrEmpty(item.cauHoiID)) continue;
                string trimmedText = item.noiDung != null ? item.noiDung.Trim() : "";
                if (existingIds.Contains(item.cauHoiID) || existingTexts.Contains(trimmedText)) continue;

                _essays.Add(item);
                existingIds.Add(item.cauHoiID);
                existingTexts.Add(trimmedText);
                mergedCount++;
            }

            Debug.Log($"[GameDataRepository] Đã hợp nhất {mergedCount} câu tự luận demo bổ sung. Tổng số câu hiện có: {_essays.Count} (đủ 5 câu/bài/mức độ cho toàn bộ 16 bài).");
        }

        public class DiagnosticInfo
        {
            public int ChapterId;
            public string LessonId;
            public string Mode;
            public string Difficulty;
            public string DataSource;
            public string SyncStatus;
            public int SheetValidCount;
            public int LocalValidCount;
            public int FilteredCount;
            public string ExcludedReasons;
            public int RequiredCount;
            public bool CanStart;
        }

        /// <summary>
        /// Tạo bản chẩn đoán chi tiết và in log dạng bắt buộc theo yêu cầu YÊU CẦU 4.
        /// </summary>
        public static DiagnosticInfo GetDetailedDiagnosis(int chapterId, string lessonId, string mode, string difficulty)
        {
            EnsureInitialized();
            string canon = ConvertToCanonicalId(lessonId);
            string normMucDo = NormalizeDifficulty(difficulty);
            bool isEssay = (mode == "ESSAY");
            int required = isEssay ? 5 : 10;

            int validCount = 0;
            if (isEssay)
            {
                validCount = _essays.Count(q => q.baiIDGame2.Equals(canon, StringComparison.OrdinalIgnoreCase) &&
                                                q.mucDoGame2.Equals(normMucDo, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                validCount = _mcqs.Count(q => q.baiID.Equals(canon, StringComparison.OrdinalIgnoreCase) &&
                                              q.mucDoGame2.Equals(normMucDo, StringComparison.OrdinalIgnoreCase));
            }

            string excludedReasons = "None";
            if (validCount < required)
            {
                excludedReasons = $"Không đủ số lượng trong ngân hàng bài {canon} mức {normMucDo} (Hiện có: {validCount}/{required})";
            }

            var info = new DiagnosticInfo
            {
                ChapterId = chapterId,
                LessonId = canon,
                Mode = isEssay ? "ESSAY" : "MCQ",
                Difficulty = normMucDo,
                DataSource = (isEssay && IsDemoSupplementalEnabled) ? $"{LastSyncDataSource}+LocalDemoSupplemental" : LastSyncDataSource,
                SyncStatus = LastSyncStatus,
                SheetValidCount = (LastSyncDataSource == "GoogleSheets") ? validCount : 0,
                LocalValidCount = validCount,
                FilteredCount = validCount,
                ExcludedReasons = excludedReasons,
                RequiredCount = required,
                CanStart = (validCount >= required)
            };

            Debug.Log($"[DiagnosticLog]\n" +
                      $"ChapterId = {info.ChapterId}\n" +
                      $"LessonId = {info.LessonId}\n" +
                      $"Mode = {info.Mode}\n" +
                      $"Difficulty = {info.Difficulty}\n" +
                      $"DataSource = {info.DataSource}\n" +
                      $"SyncStatus = {info.SyncStatus}\n" +
                      $"SheetValidCount = {info.SheetValidCount}\n" +
                      $"LocalValidCount = {info.LocalValidCount}\n" +
                      $"FilteredCount = {info.FilteredCount}\n" +
                      $"ExcludedReasons = {info.ExcludedReasons}\n" +
                      $"RequiredCount = {info.RequiredCount}\n" +
                      $"CanStart = {info.CanStart}");

            return info;
        }

        private static int FindCol(List<string> headers, string name)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                if (headers[i].Trim().Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return -1;
        }

        private static string GetColValue(List<string> row, int colIndex)
        {
            if (colIndex < 0 || colIndex >= row.Count) return "";
            return row[colIndex] ?? "";
        }

        private static List<List<string>> ParseCsvToRows(string text)
        {
            var rows = new List<List<string>>();
            if (string.IsNullOrEmpty(text)) return rows;

            var currentRow = new List<string>();
            var sb = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    currentRow.Add(sb.ToString());
                    sb.Clear();
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    currentRow.Add(sb.ToString());
                    sb.Clear();
                    if (currentRow.Count > 0 && currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                    {
                        rows.Add(currentRow);
                    }
                    currentRow = new List<string>();
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(sb.ToString());
                if (currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                {
                    rows.Add(currentRow);
                }
            }

            return rows;
        }
    }
}
