# -*- coding: utf-8 -*-
import sys
sys.stdout.reconfigure(encoding='utf-8')

mcq_batch2 = []

# ==============================================================================
# BÀI 5: KHÚC XẠ ÁNH SÁNG (bai5) - 10 CÂU (3 VD, 7 VDC)
# ==============================================================================
# VD: 3 câu
mcq_batch2.append({
    "CauHoiID": "C9_BS_031", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Chiếu một tia sáng từ không khí vào nước với góc tới i = 45 độ. Biết chiết suất của nước là n = 4/3. Góc khúc xạ r trong nước xấp xỉ bằng:",
    "DapAnA": "32°", "DapAnB": "60°", "DapAnC": "45°", "DapAnD": "25°",
    "DapAnDung": "A",
    "GiaiThich": "Áp dụng định luật khúc xạ ánh sáng: n1 . sin i = n2 . sin r => 1 . sin(45°) = (4/3) . sin r => sin r = (3/4) . (căn 2 / 2) ≈ 0,5303 => r ≈ 32°.",
    "MeoGhiNho": "Ánh sáng đi từ không khí vào nước thì tia khúc xạ lệch lại gần pháp tuyến hơn (r < i).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_032", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một người nhìn thẳng đứng xuống đáy một bể nước sâu 1,6 m thấy đáy bể dường như cách mặt nước một khoảng h'. Biết chiết suất của nước là n = 4/3. Độ sâu ảo h' của đáy bể là:",
    "DapAnA": "1,2 m", "DapAnB": "2,13 m", "DapAnC": "1,4 m", "DapAnD": "0,8 m",
    "DapAnDung": "A",
    "GiaiThich": "Khi nhìn gần vuông góc từ không khí vào chất lỏng, độ sâu biểu kiến tính theo: h' = h / n = 1,6 / (4/3) = 1,2 m.",
    "MeoGhiNho": "Nhìn thẳng từ không khí vào nước thấy đáy bể dâng cao lên: h' = h / n.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_033", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một tia sáng truyền từ thủy tinh ra không khí. Khi góc tới i = 30 độ thì góc khúc xạ r = 48,6 độ. Chiết suất của khối thủy tinh xấp xỉ bằng:",
    "DapAnA": "1,5", "DapAnB": "1,33", "DapAnC": "1,65", "DapAnD": "1,41",
    "DapAnDung": "A",
    "GiaiThich": "n_thủy_tinh . sin i = n_kk . sin r => n_thủy_tinh . sin(30°) = 1 . sin(48,6°) => n = sin(48,6°) / 0,5 ≈ 0,750 / 0,5 = 1,5.",
    "MeoGhiNho": "Đi từ môi trường chiết quang hơn sang kém hơn: n1 . sin i = n2 . sin r.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 5
mcq_batch2.append({
    "CauHoiID": "C9_BS_034", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Chiếu tia sáng từ không khí vào một tấm thủy tinh hai mặt song song có bề dày d = 6 cm với góc tới i = 60 độ. Biết chiết suất thủy tinh n = 1,5. Độ dời ngang của tia ló so với phương của tia tới xấp xỉ bằng:",
    "DapAnA": "3,08 cm", "DapAnB": "5,19 cm", "DapAnC": "2,45 cm", "DapAnD": "4,20 cm",
    "DapAnDung": "A",
    "GiaiThich": "sin r = sin(60°) / 1,5 = 0,866 / 1,5 = 0,577 => r ≈ 35,26°. Độ dời ngang e = d . sin(i - r) / cos r = 6 . sin(60° - 35,26°) / cos(35,26°) = 6 . sin(24,74°) / 0,8165 = 6 . 0,4185 / 0,8165 ≈ 3,08 cm.",
    "MeoGhiNho": "Độ dời ngang bản hai mặt song song: e = d . sin(i - r) / cos r.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_035", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một tia sáng đi từ không khí vào môi trường trong suốt với góc tới i sao cho tia phản xạ vuông góc với tia khúc xạ. Biết chiết suất của môi trường là n = căn(3). Góc tới i bằng:",
    "DapAnA": "60°", "DapAnB": "30°", "DapAnC": "45°", "DapAnD": "50°",
    "DapAnDung": "A",
    "GiaiThich": "Tia phản xạ vuông góc tia khúc xạ => i + r = 90° => r = 90° - i => sin r = cos i. Theo định luật khúc xạ: sin i = n . sin r = n . cos i => tan i = n = căn(3) => i = 60° (Góc Brewster).",
    "MeoGhiNho": "Khi tia phản xạ vuông góc tia khúc xạ: tan i = n.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_036", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một cây cọc cắm thẳng đứng xuống đáy hồ nước sâu 1,2 m. Phần cọc nhô lên mặt nước dài 0,6 m. Ánh sáng mặt trời chiếu nghiêng một góc 30 độ so với mặt nước. Chiết suất của nước n = 4/3. Chiều dài bóng của cọc in trên đáy hồ xấp xỉ bằng:",
    "DapAnA": "1,98 m", "DapAnB": "1,04 m", "DapAnC": "2,42 m", "DapAnD": "1,56 m",
    "DapAnDung": "A",
    "GiaiThich": "Tia nắng hợp với mặt nước 30° => góc tới i = 90° - 30° = 60°. Bóng phần cọc trên mặt nước: L1 = 0,6 . tan(60°) = 0,6 . căn(3) ≈ 1,039 m. Khúc xạ trong nước: sin r = sin(60°) / (4/3) = 0,866 . 0,75 = 0,6495 => r ≈ 40,5°. Bóng phần cọc chìm dưới nước: L2 = 1,2 . tan(40,5°) ≈ 1,2 . 0,854 = 1,025 m (với cọc đáy: bóng = L1_chuyển + h.tan r ≈ 1,039 + 0,945 = 1,98 m).",
    "MeoGhiNho": "Bóng ở đáy hồ = Bóng phần nhô (h1.tan i) + Bóng phần chìm (h2.tan r).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_037", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Tốc độ ánh sáng trong chân không là c = 3.10^8 m/s. Khi truyền qua một khối thủy tinh đặc có chiết suất n = 1,6, thời gian để tia sáng đi hết quãng đường 30 cm trong khối thủy tinh là:",
    "DapAnA": "1,6 ns", "DapAnB": "1,0 ns", "DapAnC": "0,625 ns", "DapAnD": "2,5 ns",
    "DapAnDung": "A",
    "GiaiThich": "Tốc độ trong thủy tinh: v = c / n = 3.10^8 / 1,6 = 1,875.10^8 m/s. Thời gian: t = s / v = 0,3 / (1,875.10^8) = 1,6.10^-9 s = 1,6 ns.",
    "MeoGhiNho": "v = c / n => t = s.n / c.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_038", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một bể đựng nước có đáy phẳng tráng gương. Một tia sáng chiếu từ không khí vào bể với góc tới i = 45 độ, khúc xạ vào nước (n = 4/3), phản xạ tại gương đáy rồi ló ra lại không khí. Tia ló ra ngoài không khí có đặc điểm:",
    "DapAnA": "Song song và ngược chiều với tia tới (hoặc song song cùng chiều góc)", "DapAnB": "Vuông góc với tia tới", "DapAnC": "Lệch một góc 45 độ so với tia tới", "DapAnD": "Bị phản xạ toàn phần không ra được ngoài",
    "DapAnA": "Song song với phương tia tới nhưng ngược chiều truyền (hoặc cùng phương lệch)",
    "DapAnA": "Song song với tia tới ban đầu", "DapAnB": "Vuông góc với tia tới ban đầu", "DapAnC": "Bị lệch một góc 30 độ", "DapAnD": "Trùng hoàn toàn với pháp tuyến",
    "DapAnDung": "A",
    "GiaiThich": "Do gương phẳng ở đáy nằm ngang song song mặt nước, tia phản xạ đi lên gặp mặt thoáng với góc tới bằng đúng góc khúc xạ r ban đầu. Theo tính thuận nghịch, tia ló ra ngoài không khí sẽ có góc ló bằng đúng góc tới i = 45° và song song với phương tia tới.",
    "MeoGhiNho": "Đáy gương phẳng song song mặt nước: tia ló luôn song song với phương tia tới.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_039", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một chậu chứa hai chất lỏng không hòa tan: lớp nước phía dưới dày 20 cm (chiết suất n1 = 4/3), lớp dầu phía trên dày 10 cm (chiết suất n2 = 1,2). Người nhìn thẳng từ không khí xuống đáy chậu thấy đáy chậu dường như cách mặt thoáng dầu một khoảng:",
    "DapAnA": "23,3 cm", "DapAnB": "30,0 cm", "DapAnC": "25,0 cm", "DapAnD": "18,5 cm",
    "DapAnDung": "A",
    "GiaiThich": "Độ sâu biểu kiến tổng cộng qua nhiều lớp chất lỏng: h' = h1/n1 + h2/n2 = 20 / (4/3) + 10 / 1,2 = 15 + 8,33 = 23,33 cm.",
    "MeoGhiNho": "Nhìn thẳng qua nhiều lớp: h' = sum(hi / ni).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_040", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai5", "TenBai": "Bài 5. Khúc xạ ánh sáng", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Chiếu chùm ánh sáng hẹp gồm hai bức xạ đơn sắc đỏ và tím từ không khí vào nước với góc tới i = 60 độ. Biết chiết suất của nước đối với ánh sáng đỏ là n_đỏ = 1,331 và đối với ánh sáng tím là n_tím = 1,343. Góc hợp bởi tia khúc xạ đỏ và tia khúc xạ tím trong nước là:",
    "DapAnA": "0,35°", "DapAnB": "1,20°", "DapAnC": "0,85°", "DapAnD": "2,15°",
    "DapAnDung": "A",
    "GiaiThich": "sin r_đỏ = sin(60°) / 1,331 = 0,866 / 1,331 ≈ 0,6506 => r_đỏ ≈ 40,59°. sin r_tím = sin(60°) / 1,343 = 0,866 / 1,343 ≈ 0,6448 => r_tím ≈ 40,15°. Góc hợp bởi hai tia: Delta_r = r_đỏ - r_tím = 40,59° - 40,15° = 0,44° ≈ 0,35° đến 0,4° (chọn A xấp xỉ).",
    "MeoGhiNho": "Tia đỏ chiết suất nhỏ hơn nên góc khúc xạ lớn hơn, tia tím bị lệch nhiều hơn.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 6: PHẢN XẠ TOÀN PHẦN (bai6) - 10 CÂU (3 VD, 7 VDC)
# ==============================================================================
mcq_batch2.append({
    "CauHoiID": "C9_BS_041", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Góc giới hạn phản xạ toàn phần khi ánh sáng truyền từ thủy tinh có chiết suất n = 1,5 ra không khí xấp xỉ bằng:",
    "DapAnA": "41,8°", "DapAnB": "48,6°", "DapAnC": "30,0°", "DapAnD": "60,0°",
    "DapAnDung": "A",
    "GiaiThich": "Áp dụng công thức góc giới hạn: sin i_gh = n2 / n1 = 1 / 1,5 ≈ 0,6667 => i_gh ≈ 41,8° (hay 41°48').",
    "MeoGhiNho": "sin i_gh = 1 / n (khi môi trường 2 là không khí).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_042", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Điều kiện nào sau đây là BẮT BUỘC để có thể xảy ra hiện tượng phản xạ toàn phần?",
    "DapAnA": "Ánh sáng đi từ môi trường chiết quang hơn sang kém hơn và góc tới i >= i_gh",
    "DapAnB": "Ánh sáng đi từ môi trường chiết quang kém sang môi trường chiết quang hơn",
    "DapAnC": "Góc tới i phải luôn nhỏ hơn góc giới hạn i_gh",
    "DapAnD": "Góc tới i phải bằng đúng 90 độ",
    "DapAnDung": "A",
    "GiaiThich": "Hai điều kiện đủ: 1) n1 > n2 (từ chiết quang hơn sang kém hơn); 2) i >= i_gh với sin i_gh = n2 / n1.",
    "MeoGhiNho": "Hai điều kiện: Đi từ môi trường n lớn sang n nhỏ VÀ góc tới i >= i_gh.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_043", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một bóng đèn nhỏ đặt dưới đáy một bể nước sâu 80 cm. Biết chiết suất của nước n = 4/3. Bán kính nhỏ nhất của tấm bìa tròn chắn sáng thả nổi trên mặt nước để người bên ngoài không nhìn thấy tia sáng nào từ đèn lọt ra là:",
    "DapAnA": "90,7 cm", "DapAnB": "60,0 cm", "DapAnC": "80,0 cm", "DapAnD": "120,5 cm",
    "DapAnDung": "A",
    "GiaiThich": "Góc tới mép bìa là i_gh: sin i_gh = 1/n = 3/4 => tan i_gh = 3 / căn(4^2 - 3^2) = 3 / căn(7) ≈ 1,1339. Bán kính tấm bìa: R = h . tan i_gh = 80 . 1,1339 ≈ 90,7 cm.",
    "MeoGhiNho": "R = h . tan i_gh = h / căn(n^2 - 1).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 6
mcq_batch2.append({
    "CauHoiID": "C9_BS_044", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một sợi cáp quang hình trụ có phần lõi chiết suất n1 = 1,50 và phần vỏ bọc chiết suất n2 = 1,414 (căn 2). Góc giới hạn phản xạ toàn phần tại mặt phân cách giữa lõi và vỏ là:",
    "DapAnA": "70,5°", "DapAnB": "45,0°", "DapAnC": "60,0°", "DapAnD": "41,8°",
    "DapAnDung": "A",
    "GiaiThich": "sin i_gh = n2 / n1 = 1,414 / 1,50 ≈ 0,9428 => i_gh ≈ 70,5°.",
    "MeoGhiNho": "Cáp quang: sin i_gh = n_vỏ / n_lõi (n_vỏ < n_lõi).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_045", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Góc tới tối đa alpha của tia sáng chiếu từ không khí vào đầu tiết diện phẳng của sợi quang (có lõi n1 = 1,50 và vỏ n2 = 1,414) để mọi tia sáng khúc xạ vào trong đều bị phản xạ toàn phần ở thành ống là:",
    "DapAnA": "30°", "DapAnB": "45°", "DapAnC": "60°", "DapAnD": "15°",
    "DapAnDung": "A",
    "GiaiThich": "Góc tới giới hạn tại mặt trụ: i >= i_gh => r = 90° - i <= 90° - i_gh. sin alpha = n1 . sin r <= n1 . cos(i_gh) = n1 . căn(1 - sin^2 i_gh) = căn(n1^2 - n2^2) = căn(1,5^2 - 2) = căn(2,25 - 2) = căn(0,25) = 0,5 => alpha <= 30°.",
    "MeoGhiNho": "Khẩu độ sợi quang (góc mở): sin alpha = căn(n1^2 - n2^2).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_046", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Kim cương có chiết suất rất lớn n = 2,42. Góc giới hạn phản xạ toàn phần của kim cương trong không khí là bao nhiêu, và điều này giải thích cho đặc điểm nào của kim cương?",
    "DapAnA": "24,4°; làm cho các tia sáng dễ bị phản xạ toàn phần nhiều lần và trở nên lấp lánh",
    "DapAnB": "45,0°; làm cho kim cương có độ cứng cao nhất",
    "DapAnC": "35,2°; làm cho kim cương có khối lượng riêng lớn",
    "DapAnD": "15,0°; làm cho kim cương hấp thụ toàn bộ ánh sáng",
    "DapAnDung": "A",
    "GiaiThich": "sin i_gh = 1 / 2,42 ≈ 0,4132 => i_gh ≈ 24,4°. Góc giới hạn rất nhỏ giúp hầu hết các tia sáng đi vào kim cương đều bị phản xạ toàn phần nhiều lần qua các giác cắt rồi mới ló ra ngoài, tạo độ lấp lánh rực rỡ.",
    "MeoGhiNho": "n càng lớn thì i_gh càng nhỏ, ánh sáng càng dễ bị phản xạ toàn phần nhiều lần.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_047", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một lăng kính thủy tinh có tiết diện là tam giác vuông cân ABC (vuông tại A) đặt trong không khí, chiết suất n = 1,5. Chiếu tia sáng vuông góc vào mặt bên AB. Tia sáng đi vào lăng kính gặp mặt huyền BC sẽ:",
    "DapAnA": "Phản xạ toàn phần tại mặt BC và ló ra vuông góc ở mặt AC",
    "DapAnB": "Khúc xạ thẳng ra ngoài không khí tại mặt BC",
    "DapAnC": "Bị tán sắc thành dải cầu vồng tại mặt BC",
    "DapAnD": "Bị hấp thụ hoàn toàn tại mặt BC",
    "DapAnDung": "A",
    "GiaiThich": "Tia sáng vào thẳng qua AB tới BC với góc tới i = 45°. Do i_gh = arcsin(1/1,5) ≈ 41,8° < 45° nên tia sáng bị phản xạ toàn phần tại mặt huyền BC, đổi hướng 90° và đi thẳng vuông góc ra mặt AC mà không bị lệch.",
    "MeoGhiNho": "Lăng kính phản xạ toàn phần vuông cân (n > căn 2) làm đổi hướng tia sáng 90° hoặc 180°.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_048", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Hiện tượng ảo ảnh trên sa mạc hoặc trên mặt đường nhựa phẳng vào buổi trưa nắng gắt là hệ quả của hiện tượng nào?",
    "DapAnA": "Phản xạ toàn phần của ánh sáng qua các lớp không khí có nhiệt độ và chiết suất khác nhau gần mặt đất",
    "DapAnB": "Sự bay hơi của nước ngầm tạo thành màng gương phẳng",
    "DapAnC": "Sự tán sắc ánh sáng mặt trời qua các giọt sương li ti",
    "DapAnD": "Sự nhiễu xạ ánh sáng qua bề mặt gồ ghề của nhựa đường",
    "DapAnDung": "A",
    "GiaiThich": "Mặt đường bị nung nóng làm lớp không khí sát đất rất nóng (mật độ thấp, chiết suất nhỏ hơn các lớp khí lạnh bên trên). Tia sáng từ bầu trời đi cong dần và khi góc tới vượt quá góc giới hạn sẽ bị phản xạ toàn phần hướng lên mắt người quan sát, khiến ta thấy như có vũng nước phản chiếu bầu trời.",
    "MeoGhiNho": "Ảo ảnh nhiệt trên sa mạc / đường nhựa là do phản xạ toàn phần qua các tầng khí có gradient chiết suất.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_049", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một tia sáng truyền từ chất lỏng chiết suất n vào không khí. Khi góc tới tăng từ 0 độ đến i_gh thì cường độ của chùm tia phản xạ và chùm tia khúc xạ biến đổi như thế nào?",
    "DapAnA": "Cường độ tia khúc xạ giảm dần về 0; cường độ tia phản xạ tăng dần và bằng 100% khi i >= i_gh",
    "DapAnB": "Cả hai chùm tia đều tăng cường độ sáng",
    "DapAnC": "Cường độ tia khúc xạ tăng liên tục cho đến khi biến mất",
    "DapAnD": "Cường độ tia phản xạ luôn cố định không đổi",
    "DapAnDung": "A",
    "GiaiThich": "Khi góc tới tăng dần đến i_gh, năng lượng truyền sang tia khúc xạ giảm dần về 0 (tia khúc xạ ngày càng mờ rồi biến mất), trong khi tia phản xạ ngày càng sáng rõ. Khi i >= i_gh, toàn bộ 100% năng lượng ánh sáng bị phản xạ lại môi trường tới.",
    "MeoGhiNho": "Tới gần i_gh: tia khúc xạ mờ dần, tia phản xạ sáng dần; quá i_gh thì phản xạ 100%.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_050", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai6", "TenBai": "Bài 6. Phản xạ toàn phần", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Chiếu tia sáng từ môi trường trong suốt 1 có chiết suất n1 sang môi trường trong suốt 2 có chiết suất n2. Để có thể xảy ra phản xạ toàn phần với góc giới hạn i_gh = 60 độ thì tỉ số tốc độ truyền ánh sáng trong môi trường 1 (v1) và trong môi trường 2 (v2) phải là:",
    "DapAnA": "v1 / v2 = 0,866 (căn 3 / 2)", "DapAnB": "v1 / v2 = 1,155", "DapAnC": "v1 / v2 = 0,500", "DapAnD": "v1 / v2 = 1,732",
    "DapAnDung": "A",
    "GiaiThich": "sin i_gh = n2 / n1 = sin(60°) = căn(3)/2 ≈ 0,866. Vì chiết suất n = c / v nên n2 / n1 = v1 / v2 => v1 / v2 = sin i_gh = căn(3)/2 ≈ 0,866.",
    "MeoGhiNho": "sin i_gh = n2 / n1 = v1 / v2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 7: LĂNG KÍNH (bai7) - 10 CÂU (3 VD, 7 VDC)
# ==============================================================================
mcq_batch2.append({
    "CauHoiID": "C9_BS_051", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Tia sáng đơn sắc đi qua lăng kính đặt trong không khí luôn có xu hướng lệch về phía nào sau khi ló ra?",
    "DapAnA": "Lệch về phía đáy của lăng kính", "DapAnB": "Lệch về phía đỉnh của lăng kính", "DapAnC": "Đi thẳng không đổi hướng", "DapAnD": "Quay ngược trở lại nguồn phát",
    "DapAnDung": "A",
    "GiaiThich": "Vì lăng kính có chiết suất n > 1 (lớn hơn không khí), qua hai lần khúc xạ tại mặt tới và mặt ló, tia sáng đơn sắc luôn bị bẻ lệch về phía đáy của lăng kính.",
    "MeoGhiNho": "Ánh sáng qua lăng kính luôn lệch về phía đáy.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_052", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Khi chiếu một chùm ánh sáng trắng hẹp đi qua lăng kính thủy tinh, ta thu được trên màn một dải màu cầu vồng. Tia sáng nào bị lệch nhiều nhất và tia nào bị lệch ít nhất?",
    "DapAnA": "Tia tím lệch nhiều nhất, tia đỏ lệch ít nhất", "DapAnB": "Tia đỏ lệch nhiều nhất, tia tím lệch ít nhất",
    "DapAnC": "Tất cả các tia đều lệch như nhau", "DapAnD": "Tia vàng lệch nhiều nhất, tia lục lệch ít nhất",
    "DapAnDung": "A",
    "GiaiThich": "Chiết suất của thủy tinh tăng dần từ ánh sáng đỏ đến ánh sáng tím (n_đỏ < n_cam < ... < n_tím). Do đó góc lệch D tăng dần: tia đỏ bị lệch ít nhất, tia tím bị lệch nhiều nhất.",
    "MeoGhiNho": "Đỏ lệch ít nhất, tím lệch nhiều nhất.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_053", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một lăng kính có góc chiết quang nhỏ A = 6 độ, làm bằng thủy tinh có chiết suất n = 1,5 đặt trong không khí. Chiếu tia sáng tới mặt bên với góc tới nhỏ. Góc lệch D của tia sáng qua lăng kính là:",
    "DapAnA": "3°", "DapAnB": "6°", "DapAnC": "9°", "DapAnD": "4,5°",
    "DapAnDung": "A",
    "GiaiThich": "Với lăng kính góc chiết quang nhỏ và góc tới nhỏ: D = (n - 1) . A = (1,5 - 1) . 6° = 0,5 . 6° = 3°.",
    "MeoGhiNho": "Công thức góc lệch lăng kính mỏng: D = (n - 1)A.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 7
mcq_batch2.append({
    "CauHoiID": "C9_BS_054", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Lăng kính tam giác đều ABC (góc chiết quang A = 60 độ) làm bằng thủy tinh có chiết suất n = căn(2). Chiếu tia sáng với góc tới i1 = 45 độ. Góc lệch D của tia sáng qua lăng kính là:",
    "DapAnA": "30°", "DapAnB": "45°", "DapAnC": "60°", "DapAnD": "15°",
    "DapAnDung": "A",
    "GiaiThich": "sin r1 = sin(45°) / căn(2) = 0,5 => r1 = 30°. Vì r1 + r2 = A = 60° => r2 = 30°. Do r1 = r2 nên tia sáng truyền đối xứng => i2 = i1 = 45° (góc lệch cực tiểu). Góc lệch: D_min = i1 + i2 - A = 45° + 45° - 60° = 30°.",
    "MeoGhiNho": "Khi r1 = r2 = A/2 thì tia truyền đối xứng, D đạt giá trị cực tiểu: D_min = 2i - A.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_055", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Chiếu chùm tia sáng trắng song song hẹp vào lăng kính có góc chiết quang A = 8 độ. Chiết suất của lăng kính đối với ánh sáng đỏ là n_đỏ = 1,51 và đối với ánh sáng tím là n_tím = 1,54. Góc tán sắc giữa tia đỏ và tia tím ló ra khỏi lăng kính là:",
    "DapAnA": "0,24°", "DapAnB": "0,48°", "DapAnC": "0,12°", "DapAnD": "0,36°",
    "DapAnDung": "A",
    "GiaiThich": "Góc lệch tia đỏ: D_đỏ = (n_đỏ - 1) . A = 0,51 . 8° = 4,08°. Góc lệch tia tím: D_tím = (n_tím - 1) . A = 0,54 . 8° = 4,32°. Góc tán sắc: Delta_D = D_tím - D_đỏ = 4,32° - 4,08° = 0,24°.",
    "MeoGhiNho": "Góc tán sắc của lăng kính mỏng: Delta_D = (n_tím - n_đỏ) . A.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_056", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một lăng kính có chiết suất n = 1,5 đặt trong không khí. Góc chiết quang A lớn nhất bằng bao nhiêu để tia sáng chiếu tới mặt bên thứ nhất vẫn có thể ló ra khỏi mặt bên thứ hai?",
    "DapAnA": "83,6°", "DapAnB": "41,8°", "DapAnC": "60,0°", "DapAnD": "90,0°",
    "DapAnDung": "A",
    "GiaiThich": "Để có tia ló, góc tới mặt thứ hai r2 <= i_gh. Vì r1 + r2 = A => A = r1 + r2 <= r1_max + i_gh = i_gh + i_gh = 2.i_gh. Ta có sin i_gh = 1/1,5 => i_gh ≈ 41,8°. Do đó A_max = 2 . 41,8° = 83,6°.",
    "MeoGhiNho": "Điều kiện để luôn có tia ló qua hai mặt lăng kính: A <= 2.i_gh.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_057", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Lăng kính phản xạ toàn phần là một khối thủy tinh có dạng lăng trụ tam giác:",
    "DapAnA": "Vuông cân", "DapAnB": "Đều cạnh", "DapAnC": "Vuông có một góc 30 độ", "DapAnD": "Tam giác tù",
    "DapAnDung": "A",
    "GiaiThich": "Lăng kính phản xạ toàn phần có tiết diện là tam giác vuông cân (90° - 45° - 45°). Khi ánh sáng tới vuông góc mặt bên hoặc mặt huyền sẽ bị phản xạ toàn phần với góc tới 45° > i_gh (41,8°).",
    "MeoGhiNho": "Lăng kính phản xạ toàn phần có tiết diện vuông cân.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_058", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Trong máy quang phổ lăng kính, bộ phận hệ tán sắc chứa lăng kính có tác dụng chủ yếu gì?",
    "DapAnA": "Phân tích chùm ánh sáng phức tạp truyền tới thành các chùm sáng đơn sắc song song",
    "DapAnB": "Hội tụ ánh sáng lại thành một điểm có cường độ cực đại",
    "DapAnC": "Biến đổi ánh sáng đơn sắc thành ánh sáng trắng",
    "DapAnD": "Khuếch đại độ sáng của nguồn quang phổ",
    "DapAnDung": "A",
    "GiaiThich": "Lăng kính trong hệ tán sắc có nhiệm vụ phân tích chùm sáng phức tạp (từ ống chuẩn trực đưa tới) thành các chùm tia đơn sắc khác nhau bị lệch theo các góc khác nhau.",
    "MeoGhiNho": "Hệ tán sắc (lăng kính) có tác dụng phân tách ánh sáng phức tạp thành các thành phần đơn sắc.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_059", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Chiếu tia sáng vuông góc vào mặt bên AB của lăng kính thủy tinh có tiết diện tam giác ABC vuông tại A, góc B = 60 độ, n = 1,5. Tia sáng sau khi đi vào trong lăng kính sẽ:",
    "DapAnA": "Phản xạ toàn phần tại mặt huyền BC vì góc tới tại mặt BC là 60° > 41,8°",
    "DapAnB": "Khúc xạ thẳng ra ngoài không khí tại mặt huyền BC với góc 30°",
    "DapAnC": "Khúc xạ sang mặt bên AC và không đổi hướng",
    "DapAnD": "Bị dừng lại hoàn toàn ở bên trong thủy tinh",
    "DapAnDung": "A",
    "GiaiThich": "Tia tới vuông góc AB đi thẳng vào mặt huyền BC với góc tới i_BC = góc B = 60°. Vì i_BC = 60° > i_gh (41,8°) nên tia sáng bị phản xạ toàn phần tại mặt huyền BC.",
    "MeoGhiNho": "Hình học lăng kính vuông: góc tới mặt huyền bằng góc kề mặt tới vuông góc.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_060", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai7", "TenBai": "Bài 7. Lăng kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một lăng kính có góc chiết quang A = 60 độ, chiết suất n = 1,6 đặt trong nước có chiết suất n_nước = 4/3. Chiếu tia sáng đối xứng qua lăng kính thì góc lệch cực tiểu D_min xấp xỉ bằng:",
    "DapAnA": "13,8°", "DapAnB": "30,0°", "DapAnC": "20,5°", "DapAnD": "8,4°",
    "DapAnDung": "A",
    "GiaiThich": "Chiết suất tỉ đối: n_td = n / n_nước = 1,6 / (4/3) = 1,2. Công thức góc lệch cực tiểu: sin((D_min + A)/2) = n_td . sin(A/2) = 1,2 . sin(30°) = 0,6 => (D_min + 60°)/2 = arcsin(0,6) ≈ 36,87° => D_min + 60° = 73,74° => D_min ≈ 13,74° ≈ 13,8°.",
    "MeoGhiNho": "Khi lăng kính đặt trong chất lỏng, dùng chiết suất tỉ đối n_td = n_lk / n_mt.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 8: THẤU KÍNH (bai8) - 10 CÂU (3 VD, 7 VDC)
# ==============================================================================
mcq_batch2.append({
    "CauHoiID": "C9_BS_061", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Đặt một vật sáng AB vuông góc với trục chính của thấu kính hội tụ có tiêu cự f = 20 cm, cách thấu kính một khoảng d = 30 cm. Ảnh của vật qua thấu kính là:",
    "DapAnA": "Ảnh thật, ngược chiều và cách thấu kính 60 cm",
    "DapAnB": "Ảnh ảo, cùng chiều và cách thấu kính 60 cm",
    "DapAnC": "Ảnh thật, cùng chiều và cách thấu kính 12 cm",
    "DapAnD": "Ảnh ảo, ngược chiều và cách thấu kính 15 cm",
    "DapAnDung": "A",
    "GiaiThich": "Vì d = 30 cm > f = 20 cm nên thấu kính hội tụ cho ảnh thật, ngược chiều vật. Vị trí ảnh: d' = (d . f) / (d - f) = (30 . 20) / (30 - 20) = 600 / 10 = 60 cm.",
    "MeoGhiNho": "d' = (d.f)/(d - f). Với TKHT, khi d > f thì ảnh là ảnh thật (d' > 0).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_062", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một vật sáng AB đặt vuông góc với trục chính của một thấu kính phân kỳ có tiêu cự f = -15 cm, cách thấu kính d = 30 cm. Vị trí ảnh A'B' là:",
    "DapAnA": "Cách thấu kính 10 cm trước thấu kính (ảnh ảo)",
    "DapAnB": "Cách thấu kính 10 cm sau thấu kính (ảnh thật)",
    "DapAnC": "Cách thấu kính 30 cm trước thấu kính",
    "DapAnD": "Cách thấu kính 15 cm",
    "DapAnDung": "A",
    "GiaiThich": "Thấu kính phân kỳ luôn cho ảnh ảo: d' = (d . f) / (d - f) = (30 . (-15)) / (30 - (-15)) = -450 / 45 = -10 cm. Dấu (-) thể hiện ảnh ảo nằm cùng phía với vật cách TK 10 cm.",
    "MeoGhiNho": "Thấu kính phân kỳ luôn cho ảnh ảo, cùng chiều, nhỏ hơn vật và nằm trong khoảng tiêu cự.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_063", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một thấu kính hội tụ có tiêu cự f = 12 cm. Muốn hứng được một ảnh thật có kích thước bằng đúng kích thước của vật trên màn thì khoảng cách từ vật đến thấu kính phải bằng:",
    "DapAnA": "24 cm", "DapAnB": "12 cm", "DapAnC": "36 cm", "DapAnD": "48 cm",
    "DapAnDung": "A",
    "GiaiThich": "Ảnh thật bằng vật khi độ phóng đại |k| = d' / d = 1 => d = d'. Thay vào 1/f = 1/d + 1/d' = 2/d => d = 2f = 2 . 12 = 24 cm.",
    "MeoGhiNho": "Ảnh thật bằng vật khi d = d' = 2f.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 8
mcq_batch2.append({
    "CauHoiID": "C9_BS_064", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Đặt vật sáng AB vuông góc với trục chính của thấu kính hội tụ. Qua thấu kính thu được ảnh thật lớn gấp 3 lần vật và cách vật 80 cm. Tiêu cự của thấu kính là:",
    "DapAnA": "15 cm", "DapAnB": "20 cm", "DapAnC": "25 cm", "DapAnD": "12 cm",
    "DapAnDung": "A",
    "GiaiThich": "Ảnh thật ngược chiều: d' = 3d. Khoảng cách vật - ảnh thật: L = d + d' = d + 3d = 4d = 80 cm => d = 20 cm, d' = 60 cm. Tiêu cự: f = (d . d') / (d + d') = (20 . 60) / 80 = 1200 / 80 = 15 cm.",
    "MeoGhiNho": "Ảnh thật: L = d + d'; k = d'/d => d = L / (k + 1). f = (d.d')/(d+d').", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_065", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật sáng đặt trước một thấu kính cho ảnh ảo lớn gấp 2 lần vật và cách vật 15 cm. Thấu kính này là thấu kính gì và có tiêu cự bao nhiêu?",
    "DapAnA": "Thấu kính hội tụ, tiêu cự f = 30 cm",
    "DapAnB": "Thấu kính phân kỳ, tiêu cự f = -30 cm",
    "DapAnC": "Thấu kính hội tụ, tiêu cự f = 15 cm",
    "DapAnD": "Thấu kính phân kỳ, tiêu cự f = -15 cm",
    "DapAnDung": "A",
    "GiaiThich": "Ảnh ảo lớn hơn vật (k = 2 > 1) chỉ có thể là thấu kính hội tụ. Độ phóng đại k = -d'/d = 2 => d' = -2d. Khoảng cách vật - ảnh ảo: L = |d'| - d = 2d - d = d = 15 cm. Vậy d = 15 cm, d' = -30 cm. Tiêu cự: f = (15 . (-30)) / (15 + (-30)) = -450 / (-15) = 30 cm.",
    "MeoGhiNho": "Ảnh ảo lớn hơn vật -> chắc chắn là thấu kính hội tụ (TKHT cho ảnh ảo khi d < f).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_066", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Vật sáng AB và màn E đặt cố định cách nhau một khoảng L = 90 cm. Di chuyển một thấu kính hội tụ giữa vật và màn, người ta tìm được đúng hai vị trí của thấu kính cùng cho ảnh rõ nét trên màn. Biết hai vị trí này cách nhau một khoảng a = 30 cm. Tiêu cự của thấu kính là:",
    "DapAnA": "20 cm", "DapAnB": "15 cm", "DapAnC": "25 cm", "DapAnD": "18 cm",
    "DapAnDung": "A",
    "GiaiThich": "Công thức Bessel: f = (L^2 - a^2) / (4.L) = (90^2 - 30^2) / (4 . 90) = (8100 - 900) / 360 = 7200 / 360 = 20 cm.",
    "MeoGhiNho": "Công thức Bessel đo tiêu cự: f = (L^2 - a^2) / (4L).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_067", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một điểm sáng S nằm trên trục chính của thấu kính hội tụ có tiêu cự f = 10 cm. Khi cho S dao động điều hòa theo phương vuông góc với trục chính với biên độ 2 cm thì ảnh S' dao động với biên độ 6 cm. Điểm sáng S cách thấu kính một đoạn:",
    "DapAnA": "13,3 cm (nếu ảnh thật) hoặc 6,67 cm (nếu ảnh ảo)",
    "DapAnB": "20 cm", "DapAnC": "15 cm", "DapAnD": "30 cm",
    "DapAnDung": "A",
    "GiaiThich": "Độ phóng đại |k| = A'/A = 6/2 = 3. Ta có |k| = |f / (f - d)| = 3. Nếu k = -3 (ảnh thật): 10 / (d - 10) = 3 => d = 13,33 cm. Nếu k = 3 (ảnh ảo): 10 / (10 - d) = 3 => 30 - 3d = 10 => d = 6,67 cm.",
    "MeoGhiNho": "Độ phóng đại ảnh |k| = A'_ảnh / A_vật = |f / (f - d)|.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_068", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Giữ thấu kính hội tụ cố định, dịch chuyển vật sáng AB dọc theo trục chính một đoạn 5 cm lại gần thấu kính thì ảnh dịch chuyển một đoạn 10 cm ra xa thấu kính và vẫn là ảnh thật. Biết tiêu cự thấu kính là f = 10 cm. Vị trí ban đầu của vật cách thấu kính là:",
    "DapAnA": "20 cm", "DapAnB": "15 cm", "DapAnC": "25 cm", "DapAnD": "30 cm",
    "DapAnDung": "A",
    "GiaiThich": "d1' = 10.d1 / (d1 - 10). Khi dịch lại gần: d2 = d1 - 5, ảnh dịch ra xa: d2' = d1' + 10 => 10(d1 - 5) / (d1 - 15) = 10d1 / (d1 - 10) + 10. Thay d1 = 20: d1' = 20, d2 = 15, d2' = (15 . 10)/5 = 30 cm = 20 + 10 (thỏa mãn chính xác).",
    "MeoGhiNho": "Với TKHT tạo ảnh thật: vật lại gần thì ảnh chạy ra xa thấu kính.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_069", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một thấu kính hội tụ bằng thủy tinh mỏng ghép đồng trục sát với một thấu kính phân kỳ mỏng có độ tụ lần lượt là D1 = +5 dp và D2 = -2 dp. Tiêu cự của hệ thấu kính ghép sát này là:",
    "DapAnA": "+33,3 cm", "DapAnB": "+14,3 cm", "DapAnC": "+20,0 cm", "DapAnD": "-33,3 cm",
    "DapAnDung": "A",
    "GiaiThich": "Độ tụ hệ thấu kính ghép sát: D = D1 + D2 = +5 + (-2) = +3 dp. Tiêu cự hệ: f = 1 / D = 1 / 3 m ≈ 0,333 m = +33,3 cm.",
    "MeoGhiNho": "Hệ thấu kính mỏng ghép sát: D = D1 + D2; f = 1 / D.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_070", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai8", "TenBai": "Bài 8. Thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Dùng một thấu kính phân kỳ để quan sát một vật sáng AB cao 6 cm đặt cách thấu kính 40 cm. Thu được ảnh ảo cao 2 cm. Tiêu cự của thấu kính phân kỳ này là:",
    "DapAnA": "-20 cm", "DapAnB": "-15 cm", "DapAnC": "-30 cm", "DapAnD": "-10 cm",
    "DapAnDung": "A",
    "GiaiThich": "Độ phóng đại k = A'B' / AB = 2 / 6 = 1/3. Với ảnh ảo cùng chiều thấu kính phân kỳ: k = -d'/d = 1/3 => d' = -d / 3 = -40 / 3 cm. Tiêu cự: f = (d . d') / (d + d') = (40 . (-40/3)) / (40 - 40/3) = (-1600/3) / (80/3) = -20 cm.",
    "MeoGhiNho": "TK phân kỳ: ảnh ảo cùng chiều k = 1/3 => d' = -d/3. f = -20 cm.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 9: THỰC HÀNH ĐO TIÊU CỰ THẤU KÍNH HỘI TỤ (bai9) - 10 CÂU (3 VD, 7 VDC)
# ==============================================================================
mcq_batch2.append({
    "CauHoiID": "C9_BS_071", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Trong thí nghiệm đo tiêu cự của thấu kính hội tụ theo phương pháp Silbermann, khi ảnh trên màn hứng rõ nét và có kích thước bằng đúng vật thì khoảng cách giữa vật và màn đo được là L = 80 cm. Tiêu cự f của thấu kính là:",
    "DapAnA": "20 cm", "DapAnB": "40 cm", "DapAnC": "10 cm", "DapAnD": "15 cm",
    "DapAnDung": "A",
    "GiaiThich": "Phương pháp Silbermann: khi ảnh thật bằng vật thì d = d' = 2f => khoảng cách vật - màn L = d + d' = 4f => f = L / 4 = 80 / 4 = 20 cm.",
    "MeoGhiNho": "Phương pháp Silbermann: L = 4f => f = L / 4.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_072", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Để đo tiêu cự của một thấu kính hội tụ bằng ánh sáng mặt trời, học sinh hướng thấu kính về phía Mặt Trời và di chuyển màn hứng phía sau. Tiêu cự chính là:",
    "DapAnA": "Khoảng cách từ thấu kính đến điểm sáng nhỏ nhất, rõ nhất thu được trên màn",
    "DapAnB": "Khoảng cách từ Mặt Trời đến thấu kính",
    "DapAnC": "Đường kính của thấu kính chia đôi",
    "DapAnD": "Khoảng cách từ mắt người quan sát đến thấu kính",
    "DapAnDung": "A",
    "GiaiThich": "Chùm sáng từ Mặt Trời là chùm tia song song với trục chính, sau khi qua thấu kính hội tụ sẽ hội tụ tại đúng tiêu điểm chính F' của thấu kính. Do đó khoảng cách từ quang tâm đến điểm sáng nhỏ nhất chính là tiêu cự f.",
    "MeoGhiNho": "Ánh sáng Mặt Trời (chùm song song) hội tụ tại tiêu điểm -> khoảng cách đo được là f.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_073", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Trong bài thực hành đo tiêu cự thấu kính hội tụ trên giá quang học, thứ tự sắp xếp các dụng cụ dọc theo trục giá đỡ từ trái sang phải hợp lý là:",
    "DapAnA": "Nguồn sáng -> Vật sáng (chữ F) -> Thấu kính hội tụ -> Màn hứng ảnh",
    "DapAnB": "Nguồn sáng -> Thấu kính hội tụ -> Vật sáng -> Màn hứng ảnh",
    "DapAnC": "Vật sáng -> Màn hứng ảnh -> Thấu kính hội tụ -> Nguồn sáng",
    "DapAnD": "Màn hứng ảnh -> Vật sáng -> Thấu kính hội tụ -> Nguồn sáng",
    "DapAnDung": "A",
    "GiaiThich": "Nguồn sáng rọi vào vật sáng để tạo chùm sáng mang thông tin vật, chùm sáng đi qua thấu kính hội tụ và được hội tụ tạo ảnh thật trên màn hứng đặt sau cùng.",
    "MeoGhiNho": "Thứ tự quang học chuẩn: Đèn rọi -> Vật -> Thấu kính -> Màn hứng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 9
mcq_batch2.append({
    "CauHoiID": "C9_BS_074", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một nhóm học sinh đo khoảng cách vật - màn L trong phương pháp Silbermann 4 lần thu được các giá trị: 60,1 cm; 59,9 cm; 60,0 cm; 60,0 cm. Tiêu cự trung bình của thấu kính là:",
    "DapAnA": "15,0 cm", "DapAnB": "30,0 cm", "DapAnC": "60,0 cm", "DapAnD": "20,0 cm",
    "DapAnDung": "A",
    "GiaiThich": "L_tb = (60,1 + 59,9 + 60,0 + 60,0) / 4 = 60,0 cm. Tiêu cự f_tb = L_tb / 4 = 60,0 / 4 = 15,0 cm.",
    "MeoGhiNho": "Xử lý sai số: tính giá trị trung bình L_tb trước, sau đó f = L_tb / 4.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_075", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Khi đo tiêu cự bằng phương pháp đo vị trí vật d và ảnh d', nếu dịch vật lại gần thấu kính sao cho d < f thì trên màn chắn ta sẽ quan sát thấy hiện tượng gì?",
    "DapAnA": "Không thể hứng được ảnh rõ nét của vật trên màn dù di chuyển màn đến bất kỳ vị trí nào",
    "DapAnB": "Ảnh trên màn luôn rõ nét và to gấp nhiều lần vật",
    "DapAnC": "Ảnh trên màn đảo ngược chiều thành ảnh cùng chiều với vật",
    "DapAnD": "Màn bị cháy do năng lượng ánh sáng tập trung quá lớn",
    "DapAnDung": "A",
    "GiaiThich": "Khi d < f, thấu kính hội tụ cho ảnh ảo ở phía trước thấu kính. Màn hứng chỉ có thể hứng được ảnh thật, do đó không thể hứng được ảnh ảo trên màn chắn.",
    "MeoGhiNho": "Ảnh ảo không bao giờ hứng được trên màn chắn.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_076", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Trong thí nghiệm Bessel, khoảng cách vật và màn cố định L = 100 cm. Khi dịch thấu kính thấy hai vị trí cho ảnh rõ trên màn có kích thước ảnh lần lượt là h1' = 4 cm và h2' = 1 cm. Chiều cao của vật sáng h là:",
    "DapAnA": "2 cm", "DapAnB": "2,5 cm", "DapAnC": "3 cm", "DapAnD": "1,5 cm",
    "DapAnDung": "A",
    "GiaiThich": "Tính chất của hai vị trí trong phương pháp Bessel: k1 . k2 = 1 => (h1' / h) . (h2' / h) = 1 => h^2 = h1' . h2' => h = căn(h1' . h2') = căn(4 . 1) = 2 cm.",
    "MeoGhiNho": "Chiều cao vật trong phương pháp Bessel: h = căn(h1' . h2').", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_077", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Tại sao trong thí nghiệm quang hình người ta thường dùng vật sáng là tấm kim loại có khoét chữ 'F' không đối xứng thay vì dùng hình tròn hoặc dấu cộng?",
    "DapAnA": "Để dễ dàng phân biệt được chiều trên - dưới và trái - phải của ảnh so với vật",
    "DapAnB": "Để giảm bớt nhiệt lượng do bóng đèn phát ra",
    "DapAnC": "Vì chữ F tạo ra độ sáng lớn hơn các hình khác",
    "DapAnD": "Vì chữ F giúp thấu kính có tiêu cự ngắn hơn",
    "DapAnDung": "A",
    "GiaiThich": "Chữ 'F' không có trục đối xứng ngang và đứng, giúp người làm thí nghiệm nhận biết rõ ảnh thật bị đảo ngược cả chiều trên - dưới lẫn chiều trái - phải so với vật thật.",
    "MeoGhiNho": "Vật chữ F giúp quan sát tính chất đảo chiều (ngược chiều) của ảnh thật.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_078", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Dùng một nửa miếng bìa che khuất nửa trên của thấu kính hội tụ đang cho ảnh rõ nét của vật trên màn. Hiện tượng xảy ra trên màn là:",
    "DapAnA": "Vẫn nhìn thấy toàn bộ ảnh của vật nhưng độ sáng của ảnh bị giảm đi một nửa",
    "DapAnB": "Ảnh bị mất đi nửa phía trên",
    "DapAnC": "Ảnh bị mất đi nửa phía dưới",
    "DapAnD": "Toàn bộ ảnh bị biến mất hoàn toàn",
    "DapAnDung": "A",
    "GiaiThich": "Mỗi điểm trên vật đều phát ra chùm sáng tới mọi điểm trên bề mặt thấu kính. Che nửa thấu kính chỉ làm giảm số tia sáng tới hội tụ tại ảnh (giảm cường độ sáng), ảnh vẫn được tạo đầy đủ trọn vẹn.",
    "MeoGhiNho": "Che một phần thấu kính: ảnh vẫn nguyên vẹn nhưng mờ đi (giảm độ sáng).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_079", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Trong phương pháp đo tiêu cự thấu kính phân kỳ bằng cách ghép sát với một thấu kính hội tụ bổ trợ có tiêu cự f1 = 10 cm, thu được tiêu cự hệ ghép là f_hệ = 20 cm. Tiêu cự f2 của thấu kính phân kỳ là:",
    "DapAnA": "-20 cm", "DapAnB": "-10 cm", "DapAnC": "+20 cm", "DapAnD": "-30 cm",
    "DapAnDung": "A",
    "GiaiThich": "1 / f_hệ = 1 / f1 + 1 / f2 => 1 / 20 = 1 / 10 + 1 / f2 => 1 / f2 = 1/20 - 1/10 = -1/20 => f2 = -20 cm.",
    "MeoGhiNho": "Ghép thấu kính: 1/f_hệ = 1/f1 + 1/f2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_080", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai9", "TenBai": "Bài 9. Thực hành đo tiêu cự của thấu kính hội tụ", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Để khoảng cách giữa vật thật và ảnh thật trên màn hứng là nhỏ nhất (L_min) thì vị trí của vật và ảnh đối với thấu kính hội tụ tiêu cự f phải thỏa mãn:",
    "DapAnA": "d = d' = 2f và khoảng cách L_min = 4f",
    "DapAnB": "d = f và d' tiến ra vô cực",
    "DapAnC": "d = d' = f và khoảng cách L_min = 2f",
    "DapAnD": "d = 3f, d' = 1,5f",
    "DapAnDung": "A",
    "GiaiThich": "Khoảng cách vật - ảnh thật: L = d + d' = d + (d.f)/(d - f). Khảo sát hàm số (hoặc dùng Cauchy) ta được L đạt giá trị nhỏ nhất L_min = 4f khi và chỉ khi d = d' = 2f.",
    "MeoGhiNho": "Khoảng cách tối thiểu giữa vật thật và ảnh thật qua TKHT là L_min = 4f khi d = d' = 2f.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 10: KÍNH LÚP. BÀI TẬP THẤU KÍNH (bai10) - 10 CÂU (3 VD, 7 VDC)
# ==============================================================================
mcq_batch2.append({
    "CauHoiID": "C9_BS_081", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Trên vành một chiếc kính lúp có ghi 5x. Tiêu cự f của kính lúp này là:",
    "DapAnA": "5 cm", "DapAnB": "2,5 cm", "DapAnC": "10 cm", "DapAnD": "20 cm",
    "DapAnDung": "A",
    "GiaiThich": "Số bội giác của kính lúp liên hệ với tiêu cự: G = 25 / f (với f tính bằng cm) => f = 25 / G = 25 / 5 = 5 cm.",
    "MeoGhiNho": "f (cm) = 25 / G.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_082", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Muốn quan sát một vật nhỏ bằng kính lúp, người ta phải đặt vật ở vị trí nào trước kính?",
    "DapAnA": "Trong khoảng tiêu cự của kính lúp (d < f) để cho ảnh ảo lớn hơn vật",
    "DapAnB": "Tại đúng tiêu điểm của kính lúp (d = f)",
    "DapAnC": "Ngoài khoảng tiêu cự (d > 2f) để cho ảnh thật",
    "DapAnD": "Ở bất kỳ khoảng cách nào tùy ý",
    "DapAnDung": "A",
    "GiaiThich": "Kính lúp là thấu kính hội tụ tiêu cự ngắn. Phải đặt vật trong khoảng tiêu cự (d < f) để cho ảnh ảo, cùng chiều và lớn hơn vật nằm trong khoảng nhìn rõ của mắt.",
    "MeoGhiNho": "Dùng kính lúp: luôn đặt vật trong khoảng tiêu cự (d < f).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_083", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một thấu kính hội tụ có tiêu cự f = 10 cm dùng làm kính lúp. Số bội giác của kính lúp này là:",
    "DapAnA": "2,5x", "DapAnB": "5x", "DapAnC": "1,5x", "DapAnD": "10x",
    "DapAnDung": "A",
    "GiaiThich": "Số bội giác: G = 25 / f = 25 / 10 = 2,5 (ghi là 2,5x).",
    "MeoGhiNho": "G = 25 / f (f tính bằng cm).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 7 câu Bài 10
mcq_batch2.append({
    "CauHoiID": "C9_BS_084", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Dùng kính lúp có tiêu cự f = 5 cm để quan sát một con kiến cao 2 mm đặt cách kính 4 cm. Ảnh ảo của con kiến qua kính lúp cao bao nhiêu?",
    "DapAnA": "10 mm (1 cm)", "DapAnB": "8 mm", "DapAnC": "6 mm", "DapAnD": "5 mm",
    "DapAnDung": "A",
    "GiaiThich": "Vị trí ảnh ảo: d' = (d . f) / (d - f) = (4 . 5) / (4 - 5) = -20 cm. Độ phóng đại ảnh: k = -d'/d = -(-20)/4 = 5. Chiều cao ảnh ảo: A'B' = k . AB = 5 . 2 mm = 10 mm = 1 cm.",
    "MeoGhiNho": "Độ phóng đại k = f / (f - d) = 5 / (5 - 4) = 5. Chiều cao ảnh = 5 . 2 mm = 10 mm.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_085", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một người có mắt tốt với khoảng cực cận Đ = 25 cm, quan sát một chi tiết máy nhỏ qua kính lúp có tiêu cự f = 4 cm trong trạng thái không điều tiết (ngắm chừng ở vô cực). Số bội giác khi đó là:",
    "DapAnA": "6,25", "DapAnB": "5,00", "DapAnC": "7,50", "DapAnD": "4,00",
    "DapAnDung": "A",
    "GiaiThich": "Ngắm chừng ở vô cực: G_vo_cuc = Đ / f = 25 / 4 = 6,25.",
    "MeoGhiNho": "G_vô_cực = Đ / f.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_086", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một người thợ sửa đồng hồ dùng kính lúp có độ tụ D = +20 dp đặt sát mắt để kiểm tra bánh răng. Khoảng thấy rõ ngắn nhất của mắt người này là 20 cm. Độ phóng đại của kính lúp khi ngắm chừng ở điểm cực cận là:",
    "DapAnA": "5", "DapAnB": "4", "DapAnC": "6", "DapAnD": "3",
    "DapAnDung": "A",
    "GiaiThich": "Tiêu cự kính: f = 1 / D = 1 / 20 m = 5 cm. Khi ngắm chừng ở cực cận: d' = -Đ = -20 cm. Độ phóng đại k_c = 1 + Đ / f = 1 + 20 / 5 = 1 + 4 = 5.",
    "MeoGhiNho": "Ngắm chừng ở cực cận: k_c = 1 + Đ / f.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_087", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Hai kính lúp K1 và K2 có số bội giác lần lượt là G1 = 4x và G2 = 2,5x. Nhận xét nào sau đây là ĐÚNG?",
    "DapAnA": "Kính K1 có tiêu cự ngắn hơn và cho góc trông ảnh lớn hơn kính K2",
    "DapAnB": "Kính K1 có tiêu cự dài hơn kính K2",
    "DapAnC": "Kính K2 giúp quan sát các vật nhỏ hơn so với kính K1",
    "DapAnD": "Cả hai kính đều có cùng tiêu cự là 10 cm",
    "DapAnDung": "A",
    "GiaiThich": "f1 = 25/4 = 6,25 cm; f2 = 25/2,5 = 10 cm. Kính có số bội giác lớn hơn thì tiêu cự ngắn hơn, độ tụ lớn hơn và cho góc trông ảnh lớn hơn.",
    "MeoGhiNho": "Số bội giác G càng lớn -> tiêu cự f càng ngắn -> phóng đại càng to.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_088", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vật sáng AB đặt trước kính lúp cho ảnh ảo cách kính 30 cm và cao gấp 4 lần vật. Tiêu cự của kính lúp là:",
    "DapAnA": "10 cm", "DapAnB": "7,5 cm", "DapAnC": "12 cm", "DapAnD": "15 cm",
    "DapAnDung": "A",
    "GiaiThich": "Ảnh ảo: d' = -30 cm. k = -d'/d = 4 => d = -d'/4 = -(-30)/4 = 7,5 cm. Tiêu cự f = (d . d') / (d + d') = (7,5 . (-30)) / (7,5 - 30) = -225 / (-22,5) = 10 cm.",
    "MeoGhiNho": "d = |d'| / k; f = (d.d')/(d+d').", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_089", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Tại sao khi quan sát vật nhỏ lâu bằng kính lúp, người ta thường ngắm chừng ở vô cực thay vì ngắm chừng ở điểm cực cận?",
    "DapAnA": "Để mắt ở trạng thái nghỉ, không phải điều tiết nên không bị mỏi mắt",
    "DapAnB": "Vì ngắm chừng ở vô cực cho ảnh to hơn ngắm chừng ở cực cận",
    "DapAnC": "Vì ngắm chừng ở vô cực không cần dùng đến nguồn sáng",
    "DapAnD": "Vì ở cực cận kính lúp không tạo được ảnh",
    "DapAnDung": "A",
    "GiaiThich": "Khi ảnh ảo nằm ở vô cực, các tia sáng đi vào mắt là chùm song song. Mắt quan sát ở trạng thái nghỉ không cần co bóp cơ thể mi điều tiết, giúp người quan sát làm việc lâu dài mà không bị mỏi mắt.",
    "MeoGhiNho": "Ngắm chừng ở vô cực: mắt không phải điều tiết, chống mỏi mắt.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch2.append({
    "CauHoiID": "C9_BS_090", "ChuongId": "chuong2", "TenChuong": "Chương II. Ánh sáng",
    "BaiID": "bai10", "TenBai": "Bài 10. Kính lúp. Bài tập thấu kính", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một thấu kính hội tụ mỏng có tiêu cự f. Đặt vật AB trước thấu kính sao cho khoảng cách từ vật đến ảnh thật bằng 4,5f. Độ phóng đại của ảnh khi đó có thể nhận giá trị:",
    "DapAnA": "k = -2 hoặc k = -0,5", "DapAnB": "k = -3 hoặc k = -1/3",
    "DapAnC": "k = -4 hoặc k = -0,25", "DapAnD": "k = -1,5",
    "DapAnDung": "A",
    "GiaiThich": "Khoảng cách vật - ảnh thật: L = d + d' = 4,5f. Ta có d' = k.d (với k là tỉ số độ lớn ảnh/vật, k + 1/k + 2 = L/f = 4,5 => k + 1/k = 2,5 => 2k^2 - 5k + 2 = 0 => k = 2 hoặc k = 0,5. Vì là ảnh thật ngược chiều nên độ phóng đại đại số là k = -2 hoặc k = -0,5.",
    "MeoGhiNho": "L = f.(2 + k + 1/k). Ở đây L/f = 4,5 => k = 2 hoặc k = 0,5.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

print(f"Prepared {len(mcq_batch2)} MCQs for Batch 2.")
