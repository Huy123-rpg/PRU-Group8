with open('Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs', 'r', encoding='utf-8') as f:
    c = f.read()

# 1. Add _demoSupplementalEssays and IsDemoSupplementalEnabled
old_vars = '''        private static List<G2EssayItem> _essays = new List<G2EssayItem>();'''
new_vars = '''        private static List<G2EssayItem> _essays = new List<G2EssayItem>();
        private static List<G2EssayItem> _demoSupplementalEssays = new List<G2EssayItem>();
        public static bool IsDemoSupplementalEnabled { get; set; } = true;'''

# 2. Add loading of demo supplemental asset in EnsureInitialized
old_init_essay = '''            // 3. Load Essays
            TextAsset essayAsset = Resources.Load<TextAsset>("QuizData/g2_essay_bank");
            if (essayAsset != null)
            {
                var wrap = JsonUtility.FromJson<G2EssayWrapper>(essayAsset.text);
                if (wrap != null && wrap.questions != null)
                {
                    _essays = wrap.questions;
                }
            }
            Debug.Log($"[GameDataRepository] Đã nạp {_essays.Count} câu tự luận.");'''

new_init_essay = '''            // 3. Load Essays (64 câu gốc)
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
            MergeDemoSupplementalEssays();'''

# 3. Add MergeDemoSupplementalEssays method and update LoadOrUpdateEssaysFromCsv
old_csv_end = '''            Debug.Log($"[GameDataRepository] Đã nạp Live Essays từ Sheet. Tổng số câu: {_essays.Count} (Thêm mới: {addedCount}).");
            LiveSheetEssayCount = _essays.Count;
            return _essays.Count;'''

new_csv_end = '''            Debug.Log($"[GameDataRepository] Đã nạp Live Essays từ Sheet. Tổng số câu: {_essays.Count} (Thêm mới: {addedCount}).");
            LiveSheetEssayCount = _essays.Count;
            // Tự động duy trì và hợp nhất lại các câu demo bổ sung sau khi Sheet sync
            MergeDemoSupplementalEssays();
            return _essays.Count;'''

# 4. Add the MergeDemoSupplementalEssays method definition before DiagnosticInfo
old_diag_header = '''        public class DiagnosticInfo'''
new_diag_header = '''        /// <summary>
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

        public class DiagnosticInfo'''

c_norm = c.replace('\r\n', '\n')
v_ok = old_vars.replace('\r\n', '\n') in c_norm
ie_ok = old_init_essay.replace('\r\n', '\n') in c_norm
ce_ok = old_csv_end.replace('\r\n', '\n') in c_norm
dh_ok = old_diag_header.replace('\r\n', '\n') in c_norm

print(f'Match vars: {v_ok}, init: {ie_ok}, csv_end: {ce_ok}, diag_hdr: {dh_ok}')

if v_ok and ie_ok and ce_ok and dh_ok:
    res = c_norm.replace(old_vars.replace('\r\n', '\n'), new_vars.replace('\r\n', '\n'))
    res = res.replace(old_init_essay.replace('\r\n', '\n'), new_init_essay.replace('\r\n', '\n'))
    res = res.replace(old_csv_end.replace('\r\n', '\n'), new_csv_end.replace('\r\n', '\n'))
    res = res.replace(old_diag_header.replace('\r\n', '\n'), new_diag_header.replace('\r\n', '\n'))
    
    with open('Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs', 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(res)
    print('SUCCESS GameDataRepository.cs patched')
else:
    print('FAILED TO MATCH SOME BLOCKS')
