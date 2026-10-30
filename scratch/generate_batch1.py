import sys
sys.stdout.reconfigure(encoding='utf-8')
from excel_helper import append_to_excel, MCQ_EXCEL_PATH, ESSAY_EXCEL_PATH, MCQ_COLUMNS, ESSAY_COLUMNS

# ==========================================
# BATCH 1: CHƯƠNG I - NĂNG LƯỢNG CƠ HỌC
# Bài 2, Bài 3, Bài 4
# ==========================================

mcq_batch1 = []
essay_batch1 = []

# ----------------------------------------------------
# BÀI 2: ĐỘNG NĂNG. THẾ NĂNG (Thiếu 3 VD, 7 VDC)
# ----------------------------------------------------
# VD: 3 câu
mcq_batch1.append({
    "CauHoiID": "C9_BS_001", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một xe máy có khối lượng cả người và xe là 150 kg đang chuyển động với tốc độ 36 km/h. Động năng của hệ là:",
    "DapAnA": "7500 J", "DapAnB": "97200 J", "DapAnC": "2700 J", "DapAnD": "1500 J",
    "DapAnDung": "A",
    "GiaiThich": "Đổi v = 36 km/h = 10 m/s. Động năng Wđ = 1/2 . m . v^2 = 1/2 . 150 . 10^2 = 7500 J.",
    "MeoGhiNho": "Nhớ đổi km/h sang m/s bằng cách chia 3,6 trước khi tính động năng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_002", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một vật có khối lượng 2 kg được đặt ở vị trí có thế năng trọng trường là 100 J so với mặt đất (lấy g = 10 m/s^2). Độ cao của vật so với mặt đất là:",
    "DapAnA": "50 m", "DapAnB": "5 m", "DapAnC": "10 m", "DapAnD": "20 m",
    "DapAnDung": "B",
    "GiaiThich": "Áp dụng công thức thế năng trọng trường: Wt = m.g.h => h = Wt / (m.g) = 100 / (2 . 10) = 5 m.",
    "MeoGhiNho": "h = Wt / (m.g), chú ý đơn vị khối lượng phải là kg.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_003", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Khi một ô tô giảm tốc độ từ 20 m/s xuống còn 10 m/s thì động năng của ô tô đã thay đổi như thế nào?",
    "DapAnA": "Giảm đi 2 lần", "DapAnB": "Giảm đi 4 lần", "DapAnC": "Giảm đi một nửa", "DapAnD": "Tăng lên 2 lần",
    "DapAnDung": "B",
    "GiaiThich": "Động năng Wđ tỉ lệ với bình phương tốc độ v^2. Tốc độ giảm 2 lần (từ 20 xuống 10 m/s) nên động năng giảm 2^2 = 4 lần.",
    "MeoGhiNho": "Wđ tỉ lệ với v^2, v giảm n lần thì Wđ giảm n^2 lần.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 2
mcq_batch1.append({
    "CauHoiID": "C9_BS_004", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Hai vật A và B có khối lượng mA = 2.mB. Để động năng của vật B bằng động năng của vật A thì tỉ số tốc độ vB / vA phải bằng:",
    "DapAnA": "2", "DapAnB": "4", "DapAnC": "căn bậc hai của 2", "DapAnD": "1/2",
    "DapAnDung": "C",
    "GiaiThich": "WđA = 1/2 mA vA^2; WđB = 1/2 mB vB^2. Cho WđA = WđB => mA vA^2 = mB vB^2 => (vB / vA)^2 = mA / mB = 2 => vB / vA = căn(2).",
    "MeoGhiNho": "Khi động năng bằng nhau, tốc độ tỉ lệ nghịch với căn bậc hai của khối lượng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_005", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật khối lượng 500g được ném thẳng đứng lên cao từ mặt đất với tốc độ ban đầu 14 m/s. Khi vật đạt độ cao 5 m thì động năng của vật còn lại bao nhiêu? (Lấy g = 10 m/s^2, bỏ qua lực cản không khí)",
    "DapAnA": "24 J", "DapAnB": "49 J", "DapAnC": "25 J", "DapAnD": "19 J",
    "DapAnDung": "A",
    "GiaiThich": "Đổi m = 0,5 kg. Động năng ban đầu Wđ0 = 1/2 . 0,5 . 14^2 = 49 J. Ở độ cao h = 5m, thế năng Wt = m.g.h = 0,5 . 10 . 5 = 25 J. Theo định luật bảo toàn cơ năng, Wđ = Wđ0 - Wt = 49 - 25 = 24 J.",
    "MeoGhiNho": "Cơ năng bảo toàn: Wđ + Wt = Hằng số => Wđ = W - Wt.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_006", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một máy bay khối lượng 20 tấn đang bay đều ở độ cao 8 km với tốc độ 720 km/h. Tỉ số giữa thế năng trọng trường (so với mặt đất, g = 10 m/s^2) và động năng của máy bay là:",
    "DapAnA": "4", "DapAnB": "2", "DapAnC": "0,5", "DapAnD": "8",
    "DapAnDung": "A",
    "GiaiThich": "Đổi v = 720 km/h = 200 m/s; h = 8 km = 8000 m; m = 20000 kg. Wt = m.g.h = 20000 . 10 . 8000 = 1,6.10^9 J. Wđ = 1/2 . 20000 . 200^2 = 4.10^8 J. Tỉ số Wt / Wđ = 1,6.10^9 / 4.10^8 = 4.",
    "MeoGhiNho": "Có thể rút gọn khối lượng m: Wt/Wđ = (g.h) / (0,5 . v^2).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_007", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật rơi tự do không vận tốc đầu từ độ cao h. Tại điểm mà động năng bằng 3 lần thế năng trọng trường thì vật đang ở độ cao nào so với mặt đất?",
    "DapAnA": "h / 4", "DapAnB": "3h / 4", "DapAnC": "h / 3", "DapAnD": "h / 2",
    "DapAnDung": "A",
    "GiaiThich": "Cơ năng W = Wt_max = m.g.h. Khi Wđ = 3.Wt thì cơ năng W = Wđ + Wt = 4.Wt => m.g.h = 4.m.g.h' => h' = h / 4.",
    "MeoGhiNho": "Khi Wđ = n.Wt thì h' = h / (n + 1). Ở đây n = 3 => h' = h/4.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_008", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một lò xo có độ cứng k = 100 N/m. Thế năng đàn hồi của lò xo khi bị nén một đoạn 4 cm là:",
    "DapAnA": "0,08 J", "DapAnB": "0,16 J", "DapAnC": "80 J", "DapAnD": "8 J",
    "DapAnDung": "A",
    "GiaiThich": "Đổi độ biến dạng x = 4 cm = 0,04 m. Thế năng đàn hồi: Wt_đh = 1/2 . k . x^2 = 1/2 . 100 . (0,04)^2 = 0,08 J.",
    "MeoGhiNho": "Nhớ đổi cm ra mét trước khi bình phương độ biến dạng: Wt = 1/2.k.x^2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_009", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một ô tô có khối lượng 1,2 tấn tăng tốc từ 36 km/h lên 72 km/h. Độ biến thiên động năng của ô tô trong quá trình này là:",
    "DapAnA": "180 kJ", "DapAnB": "120 kJ", "DapAnC": "240 kJ", "DapAnD": "300 kJ",
    "DapAnDung": "A",
    "GiaiThich": "Đổi m = 1200 kg; v1 = 10 m/s; v2 = 20 m/s. Delta_Wđ = 1/2 . m . (v2^2 - v1^2) = 1/2 . 1200 . (400 - 100) = 180000 J = 180 kJ.",
    "MeoGhiNho": "Độ tăng động năng = 1/2.m.(v2^2 - v1^2), không lấy bình phương của hiệu vận tốc.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_010", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai2", "TenBai": "Bài 2. Động năng. Thế năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một quả bóng được thả rơi tự do từ độ cao 20 m xuống sân xi măng (g = 10 m/s^2). Khi chạm đất bóng nảy lên được độ cao 15 m. Phần trăm cơ năng bị mất mát do va chạm và ma sát là:",
    "DapAnA": "25%", "DapAnB": "75%", "DapAnC": "20%", "DapAnD": "50%",
    "DapAnDung": "A",
    "GiaiThich": "Cơ năng ban đầu W1 = m.g.h1; Cơ năng lúc nảy lên W2 = m.g.h2. Tỉ lệ cơ năng mất: (W1 - W2) / W1 = (h1 - h2) / h1 = (20 - 15) / 20 = 5/20 = 25%.",
    "MeoGhiNho": "Khi khối lượng không đổi, độ hao hụt cơ năng tỉ lệ thuận với độ hao hụt độ cao nảy lên.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ----------------------------------------------------
# BÀI 3: CƠ NĂNG (Thiếu 3 VD, 7 VDC)
# ----------------------------------------------------
mcq_batch1.append({
    "CauHoiID": "C9_BS_011", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một con lắc đơn chuyển động dao động qua lại quanh vị trí cân bằng. Khi vật nặng đi từ vị trí biên về vị trí cân bằng thì:",
    "DapAnA": "Thế năng chuyển hóa thành động năng", "DapAnB": "Động năng chuyển hóa thành thế năng", "DapAnC": "Cơ năng tăng dần", "DapAnD": "Cơ năng giảm dần",
    "DapAnDung": "A",
    "GiaiThich": "Ở biên, độ cao cực đại nên thế năng cực đại. Khi về vị trí cân bằng, độ cao giảm (thế năng giảm) và tốc độ tăng (động năng tăng). Vậy thế năng đã chuyển hóa thành động năng.",
    "MeoGhiNho": "Độ cao giảm, vận tốc tăng => Thế năng chuyển hóa thành Động năng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_012", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một vật có khối lượng 1 kg được thả rơi từ độ cao 45 m so với mặt đất. Lấy g = 10 m/s^2 và bỏ qua sức cản không khí. Tốc độ của vật ngay trước khi chạm đất là:",
    "DapAnA": "30 m/s", "DapAnB": "45 m/s", "DapAnC": "15 m/s", "DapAnD": "20 m/s",
    "DapAnDung": "A",
    "GiaiThich": "Bảo toàn cơ năng: W = m.g.h = 1/2 . m . v^2 => v = căn(2.g.h) = căn(2 . 10 . 45) = căn(900) = 30 m/s.",
    "MeoGhiNho": "Vận tốc chạm đất khi rơi tự do: v = căn(2.g.h).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_013", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một viên sỏi được ném thẳng đứng lên cao từ mặt đất với cơ năng 50 J. Khi động năng của viên sỏi bằng 20 J thì thế năng của nó là:",
    "DapAnA": "30 J", "DapAnB": "70 J", "DapAnC": "25 J", "DapAnD": "10 J",
    "DapAnDung": "A",
    "GiaiThich": "Cơ năng bảo toàn: W = Wđ + Wt => Wt = W - Wđ = 50 - 20 = 30 J.",
    "MeoGhiNho": "Tổng Wđ + Wt luôn không đổi và bằng cơ năng W.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 3
mcq_batch1.append({
    "CauHoiID": "C9_BS_014", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật trượt không ma sát từ đỉnh dốc nghiêng có chiều dài l = 10 m, góc nghiêng 30 độ so với phương ngang. Lấy g = 10 m/s^2. Vận tốc của vật tại chân dốc là:",
    "DapAnA": "10 m/s", "DapAnB": "14,1 m/s", "DapAnC": "7,07 m/s", "DapAnD": "5 m/s",
    "DapAnDung": "A",
    "GiaiThich": "Độ cao của đỉnh dốc: h = l . sin(30°) = 10 . 0,5 = 5 m. Bảo toàn cơ năng: v = căn(2.g.h) = căn(2 . 10 . 5) = 10 m/s.",
    "MeoGhiNho": "Với mặt phẳng nghiêng không ma sát, v chân dốc chỉ phụ thuộc độ cao h: v = căn(2gh).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_015", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Ném một vật từ mặt đất lên cao theo phương thẳng đứng với vận tốc v0. Tại độ cao nào thì thế năng bằng động năng?",
    "DapAnA": "v0^2 / (4g)", "DapAnB": "v0^2 / (2g)", "DapAnC": "v0^2 / g", "DapAnD": "v0^2 / (8g)",
    "DapAnDung": "A",
    "GiaiThich": "Cơ năng ban đầu W = 1/2 m v0^2. Khi Wt = Wđ thì W = 2.Wt => 1/2 m v0^2 = 2.m.g.h => h = v0^2 / (4g).",
    "MeoGhiNho": "Khi Wt = Wđ => h = h_max / 2 = v0^2 / (4g).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_016", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một hòn bi nhỏ có khối lượng m được thả từ điểm A có độ cao h trên một đường ray lượn vòng tròn bán kính R trong mặt phẳng thẳng đứng. Bỏ qua ma sát. Độ cao tối thiểu h để hòn bi không bị rơi khỏi đỉnh cao nhất của vòng tròn là:",
    "DapAnA": "2,5 R", "DapAnB": "2 R", "DapAnC": "3 R", "DapAnD": "1,5 R",
    "DapAnDung": "A",
    "GiaiThich": "Tại đỉnh vòng tròn: v^2 >= g.R. Bảo toàn cơ năng: m.g.h = m.g.(2R) + 1/2.m.v^2 >= 2.m.g.R + 0,5.m.g.R = 2,5.m.g.R => h >= 2,5 R.",
    "MeoGhiNho": "Độ cao tối thiểu thả vật lượn vòng tròn không rơi ở đỉnh là h = 2,5 R.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_017", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật khối lượng 200g rơi từ độ cao 10 m xuống bãi cát mềm và lún sâu vào cát một đoạn 10 cm (g = 10 m/s^2). Lực cản trung bình của cát tác dụng lên vật là:",
    "DapAnA": "202 N", "DapAnB": "200 N", "DapAnC": "20 N", "DapAnD": "100 N",
    "DapAnDung": "A",
    "GiaiThich": "Độ cao tổng cộng vật rơi tới khi dừng: s = h + d = 10 + 0,1 = 10,1 m. Trọng lực sinh công A_P = m.g.(h + d) = 0,2 . 10 . 10,1 = 20,2 J. Công của lực cản triệt tiêu cơ năng: Fc . d = 20,2 => Fc = 20,2 / 0,1 = 202 N.",
    "MeoGhiNho": "Định lý biến thiên cơ năng: A_cản = m.g.(h + d). Chú ý tính cả đoạn lún d.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_018", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một con lắc đơn có chiều dài dây treo l = 1 m, kéo vật lệch góc 60 độ so với phương thẳng đứng rồi buông nhẹ (lấy g = 10 m/s^2). Tốc độ của vật khi đi qua vị trí cân bằng là:",
    "DapAnA": "3,16 m/s", "DapAnB": "4,47 m/s", "DapAnC": "2,24 m/s", "DapAnD": "5,00 m/s",
    "DapAnDung": "A",
    "GiaiThich": "Độ cao cực đại h = l.(1 - cos 60°) = 1 . (1 - 0,5) = 0,5 m. Tốc độ ở vị trí cân bằng: v = căn(2.g.h) = căn(2 . 10 . 0,5) = căn(10) ≈ 3,16 m/s.",
    "MeoGhiNho": "Con lắc đơn: h = l(1 - cosα). Tại VTCB: v = căn(2gl(1 - cosα)).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_019", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một viên đạn khối lượng 10g bay ngang với tốc độ 400 m/s xuyên qua một tấm gỗ dày 5 cm và bay ra với tốc độ 200 m/s. Lực cản trung bình của tấm gỗ tác dụng lên viên đạn là:",
    "DapAnA": "12000 N", "DapAnB": "6000 N", "DapAnC": "16000 N", "DapAnD": "24000 N",
    "DapAnDung": "A",
    "GiaiThich": "Độ biến thiên động năng: Delta_Wđ = 1/2 . m . (v2^2 - v1^2) = 1/2 . 0,01 . (200^2 - 400^2) = 0,005 . (-120000) = -600 J. Công lực cản: A_cản = -Fc . d => -Fc . 0,05 = -600 => Fc = 12000 N.",
    "MeoGhiNho": "Định lý động năng: A_ngoại lực = Wđ2 - Wđ1. Fc = |Delta_Wđ| / d.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_020", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai3", "TenBai": "Bài 3. Cơ năng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật được thả rơi tự do không vận tốc đầu từ độ cao 80 m (g = 10 m/s^2). Thời gian kể từ lúc thả đến khi động năng của vật bằng đúng thế năng là:",
    "DapAnA": "2,83 s", "DapAnB": "2,00 s", "DapAnC": "4,00 s", "DapAnD": "1,41 s",
    "DapAnDung": "A",
    "GiaiThich": "Khi Wđ = Wt => vật đã đi được nửa quãng đường h_rơi = 80 / 2 = 40 m. Thời gian rơi: t = căn(2.h_rơi / g) = căn(2 . 40 / 10) = căn(8) ≈ 2,83 s.",
    "MeoGhiNho": "Khi Wđ = Wt, vật ở vị trí một nửa độ cao rơi => quãng đường rơi là h/2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ----------------------------------------------------
# BÀI 4: CÔNG VÀ CÔNG SUẤT (Thiếu 3 VD, 7 VDC)
# ----------------------------------------------------
mcq_batch1.append({
    "CauHoiID": "C9_BS_021", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một con ngựa kéo xe chuyển động đều với lực kéo 500 N trên đoạn đường nằm ngang dài 2 km trong thời gian 20 phút. Công suất của con ngựa là:",
    "DapAnA": "833,3 W", "DapAnB": "1000 W", "DapAnC": "50000 W", "DapAnD": "250 W",
    "DapAnDung": "A",
    "GiaiThich": "Công thực hiện: A = F . s = 500 . 2000 = 1000000 J. Thời gian t = 20 . 60 = 1200 s. Công suất P = A / t = 1000000 / 1200 ≈ 833,3 W.",
    "MeoGhiNho": "Đổi km sang m (2 km = 2000 m), phút sang giây (20 phút = 1200 s). P = A/t.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_022", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một cần cẩu nâng một thùng hàng nặng 2 tấn lên cao 6 m theo phương thẳng đứng trong thời gian 30 giây (g = 10 m/s^2). Công suất của cần cẩu là:",
    "DapAnA": "4000 W", "DapAnB": "2400 W", "DapAnC": "1200 W", "DapAnD": "400 W",
    "DapAnDung": "A",
    "GiaiThich": "Trọng lượng thùng hàng P_trọng = m . g = 2000 . 10 = 20000 N. Công nâng: A = P_trọng . h = 20000 . 6 = 120000 J. Công suất: P = A / t = 120000 / 30 = 4000 W (4 kW).",
    "MeoGhiNho": "Công nâng đều thẳng đứng A = m.g.h. P = mgh/t.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_023", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một người dùng lực kéo 200 N để kéo một vật dịch chuyển một đoạn 10 m cùng hướng với lực kéo. Công cơ học mà người đó thực hiện là:",
    "DapAnA": "2000 J", "DapAnB": "200 J", "DapAnC": "20 J", "DapAnD": "1000 J",
    "DapAnDung": "A",
    "GiaiThich": "Công cơ học khi lực cùng hướng chuyển dời: A = F . s = 200 . 10 = 2000 J.",
    "MeoGhiNho": "A = F . s (lực cùng chiều dịch chuyển).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 4
mcq_batch1.append({
    "CauHoiID": "C9_BS_024", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một máy bơm nước có công suất 1,5 kW và hiệu suất 80%. Máy bơm này có thể bơm được bao nhiêu mét khối nước lên độ cao 12 m trong 1 giờ? (Cho khối lượng riêng của nước D = 1000 kg/m^3, g = 10 m/s^2)",
    "DapAnA": "36 m^3", "DapAnB": "45 m^3", "DapAnC": "28,8 m^3", "DapAnD": "50 m^3",
    "DapAnDung": "A",
    "GiaiThich": "Công toàn phần trong 1 giờ: A_tp = P . t = 1500 . 3600 = 5,4.10^6 J. Công có ích: A_ci = H . A_tp = 0,8 . 5,4.10^6 = 4,32.10^6 J. Khối lượng nước: m = A_ci / (g.h) = 4,32.10^6 / (10 . 12) = 36000 kg. Thể tích V = m / D = 36 m^3.",
    "MeoGhiNho": "Công có ích = H . P . t. Khối lượng nước = A_ci / (g.h).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_025", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một đầu máy xe lửa kéo đoàn tàu chuyển động đều trên đường ray nằm ngang với tốc độ 54 km/h. Biết công suất của đầu máy là 1500 kW. Lực kéo của đầu máy là:",
    "DapAnA": "100 kN", "DapAnB": "27,7 kN", "DapAnC": "150 kN", "DapAnD": "81 kN",
    "DapAnDung": "A",
    "GiaiThich": "Đổi v = 54 km/h = 15 m/s. Công thức liên hệ công suất, lực và vận tốc: P = F . v => F = P / v = 1500000 / 15 = 100000 N = 100 kN.",
    "MeoGhiNho": "P = F . v (vận tốc tính theo m/s, công suất tính theo W).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_026", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Kéo một vật nặng 100 kg lên cao 2 m bằng mặt phẳng nghiêng dài 8 m. Lực kéo song song với mặt phẳng nghiêng đo được là 300 N (g = 10 m/s^2). Hiệu suất của mặt phẳng nghiêng là:",
    "DapAnA": "83,3%", "DapAnB": "75,0%", "DapAnC": "90,0%", "DapAnD": "66,7%",
    "DapAnDung": "A",
    "GiaiThich": "Công có ích nâng trực tiếp: A_ci = m.g.h = 100 . 10 . 2 = 2000 J. Công toàn phần qua mặt phẳng nghiêng: A_tp = F . l = 300 . 8 = 2400 J. Hiệu suất: H = A_ci / A_tp = 2000 / 2400 = 83,33%.",
    "MeoGhiNho": "Hiệu suất H = A_ci / A_tp = (mgh) / (F.l).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_027", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một ô tô có khối lượng 1,5 tấn leo lên dốc nghiêng 10% (cứ đi 100 m thì lên cao 10 m) với vận tốc không đổi 36 km/h. Lực ma sát cản trở chuyển động bằng 4% trọng lượng xe (g = 10 m/s^2). Công suất của động cơ ô tô là:",
    "DapAnA": "21 kW", "DapAnB": "15 kW", "DapAnC": "30 kW", "DapAnD": "18 kW",
    "DapAnDung": "A",
    "GiaiThich": "Trọng lượng xe P_trọng = 15000 N. Lực kéo cần thiết để lên dốc đều: F = P_song_song + F_ms = 15000 . 0,1 + 15000 . 0,04 = 1500 + 600 = 2100 N. Đổi v = 36 km/h = 10 m/s. Công suất: P = F . v = 2100 . 10 = 21000 W = 21 kW.",
    "MeoGhiNho": "Lực kéo động cơ khi lên dốc đều: F = F_thành_phần_trọng_lực + F_ma_sát.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_028", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một thác nước có độ cao 25 m, lưu lượng nước chảy qua thác là 120 m^3/phút. Biết khối lượng riêng của nước là 1000 kg/m^3 và lấy g = 10 m/s^2. Nếu tận dụng năng lượng dòng nước để xây dựng nhà máy thủy điện có hiệu suất 85% thì công suất phát điện của nhà máy là:",
    "DapAnA": "425 kW", "DapAnB": "500 kW", "DapAnC": "350 kW", "DapAnD": "600 kW",
    "DapAnDung": "A",
    "GiaiThich": "Khối lượng nước trong 1 giây: m = (120 . 1000) / 60 = 2000 kg/s. Công suất dòng nước: P_nước = m . g . h = 2000 . 10 . 25 = 500000 W = 500 kW. Công suất phát điện: P_điện = H . P_nước = 0,85 . 500 = 425 kW.",
    "MeoGhiNho": "Lưu lượng nước mỗi giây: Q_s = V / 60. Công suất nước P = Q_s . D . g . h.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_029", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vận động viên cử tạ nâng đòn tạ nặng 160 kg lên độ cao 2,1 m trong thời gian 1,4 giây rồi giữ yên tạ trên cao trong 3 giây (g = 10 m/s^2). Công cơ học vận động viên thực hiện trong toàn bộ quá trình là:",
    "DapAnA": "3360 J", "DapAnB": "10560 J", "DapAnC": "7200 J", "DapAnD": "0 J",
    "DapAnDung": "A",
    "GiaiThich": "Khi nâng tạ lên: A = m.g.h = 160 . 10 . 2,1 = 3360 J. Khi giữ yên tạ trên cao, quãng đường dịch chuyển s = 0 nên công thực hiện bằng 0. Vậy tổng công thực hiện là 3360 J.",
    "MeoGhiNho": "Khi giữ yên vật không có sự dịch chuyển (s = 0) thì công cơ học bằng 0.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch1.append({
    "CauHoiID": "C9_BS_030", "ChuongId": "chuong1", "TenChuong": "Chương I. Năng lượng cơ học",
    "BaiID": "bai4", "TenBai": "Bài 4. Công và công suất", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật khối lượng 5 kg chịu tác dụng của lực kéo không đổi F = 20 N làm vật chuyển động từ trạng thái nghỉ trên mặt sàn nằm ngang không ma sát. Công suất tức thời của lực kéo tại thời điểm t = 4 giây là:",
    "DapAnA": "320 W", "DapAnB": "160 W", "DapAnC": "80 W", "DapAnD": "240 W",
    "DapAnDung": "A",
    "GiaiThich": "Gia tốc: a = F / m = 20 / 5 = 4 m/s^2. Tốc độ tại thời điểm t = 4s: v = a . t = 4 . 4 = 16 m/s. Công suất tức thời: P = F . v = 20 . 16 = 320 W.",
    "MeoGhiNho": "Công suất tức thời P = F . v(t). Vận tốc chuyển động thẳng biến đổi đều v = a.t.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

print(f"Prepared {len(mcq_batch1)} MCQs for Batch 1.")
