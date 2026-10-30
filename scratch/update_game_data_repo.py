import sys
import re

sys.stdout.reconfigure(encoding='utf-8')

path = 'Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Add static sync tracking fields
field_target = """        private static bool _isInitialized = false;"""

field_replacement = """        private static bool _isInitialized = false;

        public static string LastSyncDataSource { get; set; } = "LocalCache";
        public static string LastSyncStatus { get; set; } = "NotSynced";
        public static string LastSyncError { get; set; } = null;
        public static DateTime LastSyncTime { get; set; } = DateTime.MinValue;
        public static int LiveSheetMCQCount { get; set; } = 0;
        public static int LiveSheetEssayCount { get; set; } = 0;"""

# 2. Fix NormalizeDifficulty
norm_target = """        public static string NormalizeDifficulty(string input)
        {
            if (string.IsNullOrEmpty(input)) return "NB";
            input = input.Trim().ToUpper();
            if (input.Contains("VDC") || input.Contains("CHUYÊN SÂU") || input.Contains("TỔNG HỢP") || input.Contains("VERY HARD")) return "VDC";
            if (input.Contains("VD") || input.Contains("NÂNG CAO") || input.Contains("KHÓ") || input.Contains("HARD")) return "VD";
            if (input.Contains("TH") || input.Contains("ỨNG DỤNG") || input.Contains("TRUNG BÌNH") || input.Contains("MEDIUM")) return "TH";
            return "NB"; // Mặc định là Nhận biết (Dễ / Nền tảng)
        }"""

norm_replacement = """        public static string NormalizeDifficulty(string input)
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
        }"""

# 3. Update LoadOrUpdateMCQsFromCsv to update sync metadata
mcq_sync_target = """            Debug.Log($"[GameDataRepository] Đã nạp Live Questions từ Sheet. Tổng số câu: {_mcqs.Count} (Thêm mới: {addedCount}).");
            return _mcqs.Count;
        }"""

mcq_sync_replacement = """            Debug.Log($"[GameDataRepository] Đã nạp Live Questions từ Sheet. Tổng số câu: {_mcqs.Count} (Thêm mới: {addedCount}).");
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
                    baiID = rawBaiId,
                    baiIDGame2 = canonBaiId,
                    tenBai = GetColValue(row, colTenBai).Trim(),
                    giaoVienID = GetColValue(row, colGiaoVienId).Trim(),
                    tenGiaoVien = GetColValue(row, colTenGiaoVien).Trim(),
                    mucDo = rawMucDo,
                    mucDoGame2 = normMucDo,
                    noiDung = noiDung,
                    tuKhoaChamDiem = GetColValue(row, colTuKhoa).Trim(),
                    dapAnMau = GetColValue(row, colDapAnMau).Trim(),
                    giaiThich = GetColValue(row, colGiaiThich).Trim(),
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
            return _essays.Count;
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
                DataSource = LastSyncDataSource,
                SyncStatus = LastSyncStatus,
                SheetValidCount = (LastSyncDataSource == "GoogleSheets") ? validCount : 0,
                LocalValidCount = validCount,
                FilteredCount = validCount,
                ExcludedReasons = excludedReasons,
                RequiredCount = required,
                CanStart = (validCount >= required)
            };

            Debug.Log($"[DiagnosticLog]\\n" +
                      $"ChapterId = {info.ChapterId}\\n" +
                      $"LessonId = {info.LessonId}\\n" +
                      $"Mode = {info.Mode}\\n" +
                      $"Difficulty = {info.Difficulty}\\n" +
                      $"DataSource = {info.DataSource}\\n" +
                      $"SyncStatus = {info.SyncStatus}\\n" +
                      $"SheetValidCount = {info.SheetValidCount}\\n" +
                      $"LocalValidCount = {info.LocalValidCount}\\n" +
                      $"FilteredCount = {info.FilteredCount}\\n" +
                      $"ExcludedReasons = {info.ExcludedReasons}\\n" +
                      $"RequiredCount = {info.RequiredCount}\\n" +
                      $"CanStart = {info.CanStart}");

            return info;
        }"""

# Normalize \r\n to \n
text_norm = text.replace('\r\n', '\n')
f_t = field_target.replace('\r\n', '\n')
f_r = field_replacement.replace('\r\n', '\n')
n_t = norm_target.replace('\r\n', '\n')
n_r = norm_replacement.replace('\r\n', '\n')
m_t = mcq_sync_target.replace('\r\n', '\n')
m_r = mcq_sync_replacement.replace('\r\n', '\n')

assert f_t in text_norm, "Field target not found"
assert n_t in text_norm, "Norm target not found"
assert m_t in text_norm, "MCQ sync target not found"

text_norm = text_norm.replace(f_t, f_r, 1)
text_norm = text_norm.replace(n_t, n_r, 1)
text_norm = text_norm.replace(m_t, m_r, 1)

with open(path, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(text_norm)

print("Successfully updated GameDataRepository.cs")
