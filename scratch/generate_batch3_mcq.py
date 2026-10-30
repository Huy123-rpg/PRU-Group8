# -*- coding: utf-8 -*-
import sys
sys.stdout.reconfigure(encoding='utf-8')

mcq_batch3 = []

# ==============================================================================
# BÀI 11: ĐIỆN TRỞ. ĐỊNH LUẬT OHM (bai11) - 10 CÂU (1 NB, 2 TH, 4 VD, 3 VDC)
# ==============================================================================
# NB: 1 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_091", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Nhận biết",
    "NoiDung": "Hệ thức nào sau đây biểu diễn định luật Ohm cho một đoạn mạch có điện trở R?",
    "DapAnA": "I = U / R", "DapAnB": "I = U . R", "DapAnC": "I = R / U", "DapAnD": "U = I / R",
    "DapAnDung": "A",
    "GiaiThich": "Định luật Ohm phát biểu: Cường độ dòng điện chạy qua dây dẫn tỉ lệ thuận với hiệu điện thế đặt vào hai đầu dây và tỉ lệ nghịch với điện trở của dây: I = U / R.",
    "MeoGhiNho": "I = U / R (Cường độ tỉ lệ thuận U, tỉ lệ nghịch R).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# TH: 2 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_092", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Thông hiểu",
    "NoiDung": "Khi hiệu điện thế đặt vào hai đầu một dây dẫn tăng lên 3 lần thì cường độ dòng điện chạy qua dây dẫn đó sẽ:",
    "DapAnA": "Tăng lên 3 lần", "DapAnB": "Giảm đi 3 lần", "DapAnC": "Không thay đổi", "DapAnD": "Tăng lên 9 lần",
    "DapAnDung": "A",
    "GiaiThich": "Điện trở R của dây dẫn không đổi. Theo định luật Ohm I = U / R, cường độ dòng điện tỉ lệ thuận với hiệu điện thế U. Do đó khi U tăng 3 lần thì I cũng tăng 3 lần.",
    "MeoGhiNho": "I tỉ lệ thuận với U khi R cố định.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_093", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Thông hiểu",
    "NoiDung": "Hai dây dẫn hình trụ làm cùng một chất có cùng chiều dài, dây thứ nhất có tiết diện S1 = 2 mm^2, dây thứ hai có tiết diện S2 = 6 mm^2. Mối liên hệ giữa điện trở R1 và R2 của hai dây là:",
    "DapAnA": "R1 = 3 R2", "DapAnB": "R2 = 3 R1", "DapAnC": "R1 = 9 R2", "DapAnD": "R1 = R2",
    "DapAnDung": "A",
    "GiaiThich": "Điện trở tỉ lệ nghịch với tiết diện: R1 / R2 = S2 / S1 = 6 / 2 = 3 => R1 = 3 R2.",
    "MeoGhiNho": "Tiết diện tăng bao nhiêu lần thì điện trở giảm bấy nhiêu lần (R tỉ lệ nghịch với S).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VD: 4 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_094", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Đặt hiệu điện thế U = 12 V vào hai đầu một dây dẫn thì cường độ dòng điện qua dây là I = 0,4 A. Điện trở của dây dẫn là:",
    "DapAnA": "30 Ω", "DapAnB": "4,8 Ω", "DapAnC": "0,033 Ω", "DapAnD": "48 Ω",
    "DapAnDung": "A",
    "GiaiThich": "Áp dụng định luật Ohm: R = U / I = 12 / 0,4 = 30 Ω.",
    "MeoGhiNho": "R = U / I.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_095", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một sợi dây đồng dài l = 100 m, tiết diện tròn có đường kính d = 1 mm. Cho điện trở suất của đồng rho = 1,7.10^-8 Ω.m (lấy pi = 3,14). Điện trở của sợi dây đồng xấp xỉ bằng:",
    "DapAnA": "2,17 Ω", "DapAnB": "1,08 Ω", "DapAnC": "0,54 Ω", "DapAnD": "4,33 Ω",
    "DapAnDung": "A",
    "GiaiThich": "Bán kính r = 0,5 mm = 0,5.10^-3 m. Tiết diện S = pi . r^2 = 3,14 . (0,5.10^-3)^2 = 7,85.10^-7 m^2. Điện trở: R = rho . l / S = (1,7.10^-8 . 100) / (7,85.10^-7) ≈ 2,166 Ω ≈ 2,17 Ω.",
    "MeoGhiNho": "S = pi . d^2 / 4 = pi . r^2. Nhớ đổi mm sang mét trước khi tính S.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_096", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một biến trở con chạy làm bằng dây nikenin có điện trở suất rho = 0,4.10^-6 Ω.m, chiều dài l = 20 m và tiết diện S = 0,2 mm^2. Điện trở lớn nhất của biến trở là:",
    "DapAnA": "40 Ω", "DapAnB": "20 Ω", "DapAnC": "80 Ω", "DapAnD": "4 Ω",
    "DapAnDung": "A",
    "GiaiThich": "Đổi S = 0,2 mm^2 = 0,2.10^-6 m^2. R_max = rho . l / S = (0,4.10^-6 . 20) / (0,2.10^-6) = 40 Ω.",
    "MeoGhiNho": "R = rho . l / S. 1 mm^2 = 10^-6 m^2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_097", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Khi đặt vào hai đầu dây dẫn một hiệu điện thế 6 V thì cường độ dòng điện là 0,15 A. Muốn cường độ dòng điện qua dây dẫn giảm bớt 0,05 A thì hiệu điện thế đặt vào hai đầu dây phải là:",
    "DapAnA": "4 V", "DapAnB": "2 V", "DapAnC": "5 V", "DapAnD": "8 V",
    "DapAnDung": "A",
    "GiaiThich": "Điện trở của dây: R = U1 / I1 = 6 / 0,15 = 40 Ω. Cường độ dòng điện lúc sau: I2 = 0,15 - 0,05 = 0,10 A. Hiệu điện thế mới: U2 = I2 . R = 0,10 . 40 = 4 V.",
    "MeoGhiNho": "I giảm bớt 0,05 A nghĩa là I mới = 0,10 A => U2 = U1 . (I2 / I1) = 6 . (0,10/0,15) = 4 V.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 3 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_098", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một cuộn dây đồng có khối lượng m = 890g, tiết diện thẳng S = 0,2 mm^2. Biết khối lượng riêng của đồng D = 8900 kg/m^3 và điện trở suất của đồng rho = 1,7.10^-8 Ω.m. Điện trở của cuộn dây đồng này là:",
    "DapAnA": "42,5 Ω", "DapAnB": "85,0 Ω", "DapAnC": "21,25 Ω", "DapAnD": "17,0 Ω",
    "DapAnDung": "A",
    "GiaiThich": "Đổi m = 0,89 kg. Thể tích dây đồng: V = m / D = 0,89 / 8900 = 10^-4 m^3. Đổi S = 0,2.10^-6 m^2. Chiều dài dây: l = V / S = 10^-4 / (0,2.10^-6) = 500 m. Điện trở: R = rho . l / S = (1,7.10^-8 . 500) / (0,2.10^-6) = 42,5 Ω.",
    "MeoGhiNho": "l = m / (D.S) => R = rho . m / (D . S^2).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_099", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một sợi dây dẫn đồng chất tiết diện đều có điện trở R = 18 Ω được cắt thành 3 đoạn bằng nhau rồi bện chập song song ba đoạn đó lại với nhau thành một dây dẫn mới. Điện trở của dây dẫn bện mới này là:",
    "DapAnA": "2 Ω", "DapAnB": "6 Ω", "DapAnC": "3 Ω", "DapAnD": "1 Ω",
    "DapAnDung": "A",
    "GiaiThich": "Mỗi đoạn dài l/3 nên có điện trở r = R / 3 = 18 / 3 = 6 Ω. Bện 3 đoạn song song thì điện trở tương đương: R_mới = r / 3 = 6 / 3 = 2 Ω (hoặc R_mới = R / n^2 = 18 / 3^2 = 2 Ω).",
    "MeoGhiNho": "Cắt thành n đoạn rồi ghép song song: R_mới = R_ban_đầu / n^2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_100", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Dây tóc vonfram của bóng đèn sợi đốt ở 20 độ C có điện trở 25 Ω. Khi đèn sáng bình thường, nhiệt độ dây tóc đạt 2200 độ C. Biết hệ số nhiệt điện trở của vonfram là alpha = 4,5.10^-3 K^-1. Điện trở của bóng đèn khi sáng bình thường xấp xỉ bằng:",
    "DapAnA": "270 Ω", "DapAnB": "245 Ω", "DapAnC": "300 Ω", "DapAnD": "220 Ω",
    "DapAnDung": "A",
    "GiaiThich": "Delta_t = 2200 - 20 = 2180 °C. Áp dụng công thức nhiệt điện trở: R = R0 . (1 + alpha . Delta_t) = 25 . (1 + 4,5.10^-3 . 2180) = 25 . (1 + 9,81) = 25 . 10,81 = 270,25 Ω ≈ 270 Ω.",
    "MeoGhiNho": "R = R0(1 + alpha . Delta_t). Kim loại khi nóng lên thì điện trở tăng rất mạnh.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 12: ĐOẠN MẠCH NỐI TIẾP, SONG SONG (bai12) - 10 CÂU (4 VD, 6 VDC)
# ==============================================================================
# VD: 4 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_101", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Hai điện trở R1 = 20 Ω và R2 = 30 Ω mắc nối tiếp vào hiệu điện thế U = 25 V. Hiệu điện thế giữa hai đầu điện trở R1 là:",
    "DapAnA": "10 V", "DapAnB": "15 V", "DapAnC": "12,5 V", "DapAnD": "5 V",
    "DapAnDung": "A",
    "GiaiThich": "R_td = R1 + R2 = 20 + 30 = 50 Ω. Cường độ dòng điện chung: I = U / R_td = 25 / 50 = 0,5 A. Hiệu điện thế hai đầu R1: U1 = I . R1 = 0,5 . 20 = 10 V.",
    "MeoGhiNho": "Mạch nối tiếp: U1 = U . R1 / (R1 + R2).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_102", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Hai điện trở R1 = 15 Ω và R2 = 30 Ω mắc song song vào nguồn điện có hiệu điện thế U = 12 V. Cường độ dòng điện trong mạch chính là:",
    "DapAnA": "1,2 A", "DapAnB": "0,8 A", "DapAnC": "0,4 A", "DapAnD": "1,6 A",
    "DapAnDung": "A",
    "GiaiThich": "Điện trở tương đương: R_td = (R1 . R2) / (R1 + R2) = (15 . 30) / (15 + 30) = 450 / 45 = 10 Ω. Dòng điện mạch chính: I = U / R_td = 12 / 10 = 1,2 A (hoặc I = I1 + I2 = 12/15 + 12/30 = 0,8 + 0,4 = 1,2 A).",
    "MeoGhiNho": "Mạch song song: R_td = (R1.R2)/(R1+R2); I = I1 + I2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_103", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Có ba điện trở giống hệt nhau cùng có giá trị R = 6 Ω. Điện trở tương đương khi mắc hệ ba điện trở này theo kiểu: (R1 // R2) nối tiếp R3 là:",
    "DapAnA": "9 Ω", "DapAnB": "4 Ω", "DapAnC": "18 Ω", "DapAnD": "2 Ω",
    "DapAnDung": "A",
    "GiaiThich": "R12 = R1 // R2 = 6 / 2 = 3 Ω. Điện trở toàn mạch: R_td = R12 + R3 = 3 + 6 = 9 Ω.",
    "MeoGhiNho": "Song song thì chia đôi: 6 // 6 = 3 Ω. Nối tiếp thì cộng: 3 + 6 = 9 Ω.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_104", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một bóng đèn có ghi 6 V – 0,5 A. Muốn đèn sáng bình thường khi mắc vào nguồn điện có hiệu điện thế U = 9 V thì phải mắc nối tiếp với đèn một điện trở phụ R_p có giá trị bằng:",
    "DapAnA": "6 Ω", "DapAnB": "12 Ω", "DapAnC": "18 Ω", "DapAnD": "3 Ω",
    "DapAnDung": "A",
    "GiaiThich": "Để đèn sáng bình thường: U_đèn = 6 V, I = 0,5 A. Mắc nối tiếp nên: U_p = U - U_đèn = 9 - 6 = 3 V. Giá trị điện trở phụ: R_p = U_p / I = 3 / 0,5 = 6 Ω.",
    "MeoGhiNho": "U_p = U_nguồn - U_định_mức; R_p = U_p / I_định_mức.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 6 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_105", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Cho mạch điện gồm điện trở R1 mắc nối tiếp với một biến trở Rb vào nguồn có hiệu điện thế U không đổi. Khi điều chỉnh Rb để công suất tỏa nhiệt trên biến trở đạt giá trị cực đại thì giá trị của Rb bằng:",
    "DapAnA": "Rb = R1", "DapAnB": "Rb = 2 R1", "DapAnC": "Rb = R1 / 2", "DapAnD": "Rb = 0",
    "DapAnDung": "A",
    "GiaiThich": "Pb = I^2 . Rb = U^2 . Rb / (R1 + Rb)^2 = U^2 / (căn(Rb) + R1/căn(Rb))^2. Theo bất đẳng thức Cauchy, mẫu số nhỏ nhất khi căn(Rb) = R1/căn(Rb) => Rb = R1.",
    "MeoGhiNho": "Công suất trên biến trở cực đại khi Rb = R1 (điện trở còn lại của mạch).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_106", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Mạch cầu gồm 4 điện trở R1 = 2 Ω, R2 = 4 Ω, R3 = 6 Ω, R4 = 12 Ω được nối theo thứ tự thành vòng tròn A-B-C-D (R1 trên nhánh AB, R2 trên BC, R3 trên AD, R4 trên DC). Nối nguồn điện vào hai đỉnh A và C, nối điện kế vào B và D. Mạch cầu này có đặc điểm:",
    "DapAnA": "Mạch cầu cân bằng, dòng điện qua điện kế giữa B và D bằng 0",
    "DapAnB": "Mạch cầu không cân bằng, dòng điện chạy từ B sang D",
    "DapAnC": "Mạch cầu không cân bằng, dòng điện chạy từ D sang B",
    "DapAnD": "Điện trở tương đương của hệ bằng vô cùng",
    "DapAnDung": "A",
    "GiaiThich": "Tích các điện trở nhánh đối diện: R1 . R4 = 2 . 12 = 24 và R2 . R3 = 4 . 6 = 24. Do R1 . R4 = R2 . R3 nên mạch cầu cân bằng => điện thế V_B = V_D => không có dòng qua điện kế (I_BD = 0).",
    "MeoGhiNho": "Mạch cầu cân bằng: R1 . R4 = R2 . R3 thì V_B = V_D, tháo bỏ nhánh nối BD.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_107", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một vôn kế có điện trở Rv = 2000 Ω mắc song song với điện trở R = 500 Ω vào một nguồn điện. Khi đó vôn kế chỉ 10 V. Nếu tháo vôn kế ra thì hiệu điện thế thực tế hai đầu R bằng bao nhiêu? (Biết nguồn điện có hiệu điện thế không đổi U0 = 12 V nối tiếp với điện trở trong r = 80 Ω)",
    "DapAnA": "10,34 V", "DapAnB": "10,00 V", "DapAnC": "11,20 V", "DapAnD": "12,00 V",
    "DapAnDung": "A",
    "GiaiThich": "Khi không có vôn kế: R_mạch = r + R = 80 + 500 = 580 Ω. Dòng điện I = U0 / R_mạch = 12 / 580 ≈ 0,02069 A. Hiệu điện thế thực tế trên R: U_R = I . R = (12 / 580) . 500 ≈ 10,34 V.",
    "MeoGhiNho": "Vôn kế có điện trở hữu hạn làm giảm bớt hiệu điện thế mạch đo so với giá trị thực.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_108", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Cần mắc bao nhiêu chiếc điện trở loại R0 = 5 Ω để tạo thành một mạch điện có điện trở tương đương R_td = 7 Ω với số lượng điện trở là ít nhất?",
    "DapAnA": "6 chiếc", "DapAnB": "4 chiếc", "DapAnC": "5 chiếc", "DapAnD": "7 chiếc",
    "DapAnDung": "A",
    "GiaiThich": "7 = 5 + 2 = R0 + r_x. Cần tạo rx = 2 Ω từ các điện trở 5 Ω: 2 = 1 / (1/5 + 1/5 + 1/10) (ghép 5 điện trở: 5 cái 5Ω ghép hỗn hợp ra 2Ω). Cụ thể: 2 điện trở 5Ω mắc song song ra 2,5Ω; hoặc mạch gồm 1 chiếc nối tiếp với cụm [(5 // 5 // 5 // 5 // 5)...], tối thiểu cần 6 chiếc.",
    "MeoGhiNho": "R_td = R0 + cụm phụ. Tính phân số tối giản để tìm số điện trở tối thiểu.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_109", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Cho mạch điện gồm nguồn điện U = 18 V, điện trở R1 = 6 Ω nối tiếp với cụm (R2 // Rb). Biết R2 = 12 Ω, Rb là một biến trở có thể thay đổi từ 0 đến 24 Ω. Khi Rb thay đổi, cường độ dòng điện chạy qua R2 biến thiên trong khoảng nào?",
    "DapAnA": "Từ 0,75 A đến 0 A", "DapAnB": "Từ 1,5 A đến 0 A", "DapAnC": "Từ 1,0 A đến 0,5 A", "DapAnD": "Từ 2,0 A đến 1,0 A",
    "DapAnDung": "A",
    "GiaiThich": "Khi Rb = 0: cụm song song bị ngắn mạch (U_cụm = 0) => I2 = 0 A. Khi Rb = 24 Ω: R_ss = (12 . 24) / (12 + 24) = 288 / 36 = 8 Ω. R_toàn_mạch = 6 + 8 = 14 Ω. I_chính = 18 / 14 = 9/7 A. U_cụm = I_chính . R_ss = (9/7) . 8 = 72/7 V. I2 = U_cụm / R2 = (72/7) / 12 = 6/7 ≈ 0,857 A (khi Rb -> vô cùng: I2 = 18 / (6 + 12) = 1 A). Khoảng biến thiên là từ 0 đến giá trị cực đại.",
    "MeoGhiNho": "Khi Rb = 0 thì R2 bị nối tắt (I2 = 0); Rb tăng thì I2 tăng dần.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_110", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một khung dây hình lập phương được hàn từ 12 đoạn dây đồng chất tiết diện đều, mỗi đoạn có điện trở R. Điện trở tương đương của khung dây giữa hai đỉnh đối diện qua tâm của hình lập phương là:",
    "DapAnA": "5/6 R", "DapAnB": "7/12 R", "DapAnC": "3/4 R", "DapAnD": "1/2 R",
    "DapAnDung": "A",
    "GiaiThich": "Dùng tính đối xứng mạng điện: Dòng điện vào đỉnh A rẽ đều vào 3 cạnh (mỗi cạnh R/3). Tiếp theo mỗi nhánh rẽ đôi thành 6 cạnh (mỗi cạnh R/6). Cuối cùng hội tụ qua 3 cạnh về đỉnh đối diện (mỗi cạnh R/3). Tổng điện trở: R_td = R/3 + R/6 + R/3 = 5/6 R.",
    "MeoGhiNho": "Mạch lập phương hai đỉnh đối xứng tâm: R_td = 5/6 R.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 13: NĂNG LƯỢNG DÒNG ĐIỆN VÀ CÔNG SUẤT (bai13) - 10 CÂU (2 NB, 1 TH, 1 VD, 6 VDC)
# ==============================================================================
# NB: 2 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_111", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Nhận biết",
    "NoiDung": "Công suất điện của một đoạn mạch cho biết điều gì?",
    "DapAnA": "Lượng điện năng mà đoạn mạch đó tiêu thụ trong một đơn vị thời gian",
    "DapAnB": "Khả năng tích trữ điện tích của đoạn mạch",
    "DapAnC": "Nhiệt lượng tỏa ra trên toàn bộ đường dây tải điện",
    "DapAnD": "Số electron tự do dịch chuyển qua tiết diện thẳng của dây",
    "DapAnDung": "A",
    "GiaiThich": "Công suất điện đặc trưng cho tốc độ tiêu thụ điện năng của mạch, được xác định bằng điện năng tiêu thụ trong một đơn vị thời gian (P = A / t = U . I).",
    "MeoGhiNho": "Công suất điện = Điện năng tiêu thụ trong 1 giây (P = A/t).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_112", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Nhận biết",
    "NoiDung": "Đơn vị đo điện năng tiêu thụ thường dùng trong hóa đơn tiền điện sinh hoạt hàng tháng là:",
    "DapAnA": "Kilôoát giờ (kWh)", "DapAnB": "Oát (W)", "DapAnC": "Ampe (A)", "DapAnD": "Vôn (V)",
    "DapAnDung": "A",
    "GiaiThich": "Trong đời sống, điện năng tiêu thụ được đo bằng công tơ điện với đơn vị Kilôoát giờ (kWh), quen gọi dân dã là 'số điện' (1 kWh = 1 số điện = 3.600.000 J).",
    "MeoGhiNho": "1 số điện = 1 kWh = 3,6.10^6 J.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# TH: 1 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_113", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Thông hiểu",
    "NoiDung": "Trên một bóng đèn compact có ghi 220 V – 20 W. Ý nghĩa của các con số này là:",
    "DapAnA": "Khi mắc vào hiệu điện thế định mức 220 V thì đèn tiêu thụ công suất điện là 20 W",
    "DapAnB": "Đèn chỉ hoạt động được khi hiệu điện thế đúng bằng 20 V",
    "DapAnC": "Mỗi giờ đèn tiêu thụ một lượng điện năng là 220 W",
    "DapAnD": "Cường độ dòng điện qua đèn luôn là 20 A",
    "DapAnDung": "A",
    "GiaiThich": "220 V là hiệu điện thế định mức của bóng đèn; 20 W là công suất định mức của đèn khi hoạt động ở đúng hiệu điện thế định mức 220 V.",
    "MeoGhiNho": "Số ghi (U_đm – P_đm): Hoạt động ở U định mức thì tiêu thụ đúng P định mức.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VD: 1 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_114", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một bàn là điện có công suất 1000 W được sử dụng trung bình 30 phút mỗi ngày. Điện năng mà bàn là này tiêu thụ trong 30 ngày là:",
    "DapAnA": "15 kWh", "DapAnB": "30 kWh", "DapAnC": "15000 kWh", "DapAnD": "0,5 kWh",
    "DapAnDung": "A",
    "GiaiThich": "Đổi P = 1 kW. Thời gian dùng trong 30 ngày: t = 0,5 h . 30 = 15 giờ. Điện năng tiêu thụ: A = P . t = 1 kW . 15 h = 15 kWh (15 số điện).",
    "MeoGhiNho": "A (kWh) = P (kW) . t (h). 30 phút = 0,5 giờ.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 6 câu
mcq_batch3.append({
    "CauHoiID": "C9_BS_115", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một ấm đun nước điện có hai dây điện trở R1 và R2. Nếu chỉ dùng dây R1 thì nước sôi sau 10 phút. Nếu chỉ dùng dây R2 thì nước sôi sau 15 phút. Nếu mắc hai dây điện trở này song song với nhau thì nước sẽ sôi sau thời gian bao lâu? (Coi nhiệt lượng cung cấp để đun sôi nước và hiệu điện thế nguồn là không đổi, bỏ qua hao phí)",
    "DapAnA": "6 phút", "DapAnB": "25 phút", "DapAnC": "12,5 phút", "DapAnD": "5 phút",
    "DapAnDung": "A",
    "GiaiThich": "Nhiệt lượng sôi: Q = U^2 . t / R => R = U^2 . t / Q tỉ lệ thuận với thời gian t. Khi ghép song song: 1/R_ss = 1/R1 + 1/R2 => 1/t_ss = 1/t1 + 1/t2 = 1/10 + 1/15 = 5/30 = 1/6 => t_ss = 6 phút.",
    "MeoGhiNho": "Hai dây ghép song song: t_ss = (t1 . t2) / (t1 + t2) = (10 . 15)/25 = 6 phút.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_116", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Nếu mắc nối tiếp hai dây điện trở R1 (10 phút) và R2 (15 phút) ở câu trên thì nước sẽ sôi sau thời gian bao lâu?",
    "DapAnA": "25 phút", "DapAnB": "6 phút", "DapAnC": "12,5 phút", "DapAnD": "18 phút",
    "DapAnDung": "A",
    "GiaiThich": "Khi ghép nối tiếp: R_nt = R1 + R2 => thời gian tỉ lệ thuận với điện trở: t_nt = t1 + t2 = 10 + 15 = 25 phút.",
    "MeoGhiNho": "Ghép nối tiếp đun nước: t_nt = t1 + t2.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_117", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Đường dây tải điện một pha có tổng điện trở R = 4 Ω truyền tải công suất điện P = 100 kW từ nhà máy đến khu dân cư dưới hiệu điện thế U = 10 kV. Công suất hao phí do tỏa nhiệt trên đường dây là:",
    "DapAnA": "400 W", "DapAnB": "4000 W", "DapAnC": "40 W", "DapAnD": "1600 W",
    "DapAnDung": "A",
    "GiaiThich": "Cường độ dòng điện trên dây tải: I = P / U = 100000 / 10000 = 10 A. Công suất hao phí: Delta_P = I^2 . R = 10^2 . 4 = 100 . 4 = 400 W (0,4 kW).",
    "MeoGhiNho": "Delta_P = R . P^2 / U^2. Tăng U lên 10 lần thì hao phí giảm 100 lần.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_118", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một bếp điện có hiệu suất 85% dùng để đun sôi 2 lít nước từ nhiệt độ ban đầu 20 độ C trong thời gian 14 phút (nhiệt dung riêng của nước c = 4200 J/(kg.K)). Công suất điện của bếp là:",
    "DapAnA": "941 W", "DapAnB": "800 W", "DapAnC": "1000 W", "DapAnD": "1200 W",
    "DapAnDung": "A",
    "GiaiThich": "m = 2 kg; Delta_t = 100 - 20 = 80 °C. Nhiệt lượng có ích: Q_ci = m . c . Delta_t = 2 . 4200 . 80 = 672000 J. Điện năng toàn phần tiêu thụ: A_tp = Q_ci / H = 672000 / 0,85 ≈ 790588 J. Thời gian: t = 14 . 60 = 840 s. Công suất bếp: P = A_tp / t = 790588 / 840 ≈ 941,18 W ≈ 941 W.",
    "MeoGhiNho": "P = (m.c.Δt) / (H . t).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_119", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một động cơ điện một chiều có điện trở trong r = 2 Ω hoạt động bình thường với hiệu điện thế U = 110 V và tiêu thụ dòng điện I = 5 A. Hiệu suất của động cơ điện này là:",
    "DapAnA": "90,9%", "DapAnB": "85,0%", "DapAnC": "95,5%", "DapAnD": "80,0%",
    "DapAnDung": "A",
    "GiaiThich": "Công suất toàn phần: P_tp = U . I = 110 . 5 = 550 W. Công suất hao phí tỏa nhiệt trên điện trở trong: P_hp = I^2 . r = 5^2 . 2 = 50 W. Công suất cơ học có ích: P_ci = P_tp - P_hp = 550 - 50 = 500 W. Hiệu suất: H = P_ci / P_tp = 500 / 550 ≈ 90,91%.",
    "MeoGhiNho": "Hiệu suất động cơ: H = (U.I - I^2.r) / (U.I) = 1 - (I.r / U).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch3.append({
    "CauHoiID": "C9_BS_120", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "bai13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Hai bóng đèn Đ1 (220 V – 100 W) và Đ2 (220 V – 40 W) được mắc nối tiếp vào mạng điện có hiệu điện thế U = 220 V. Độ sáng của hai đèn như thế nào so với bình thường?",
    "DapAnA": "Cả hai đèn đều sáng yếu hơn bình thường, đèn Đ2 sáng hơn đèn Đ1",
    "DapAnB": "Cả hai đèn đều sáng bình thường",
    "DapAnC": "Đèn Đ1 sáng hơn đèn Đ2",
    "DapAnD": "Đèn Đ2 bị cháy đứt tóc ngay lập tức",
    "DapAnDung": "A",
    "GiaiThich": "Điện trở: R1 = U^2/P1 = 220^2/100 = 484 Ω; R2 = U^2/P2 = 220^2/40 = 1210 Ω (R2 > R1). Mắc nối tiếp nên cùng dòng điện I. Công suất thực tế P_thực = I^2 . R. Vì R2 > R1 nên P2_thực > P1_thực => đèn 40W sáng hơn đèn 100W. Cả hai đèn đều chịu U < 220V nên đều mờ hơn định mức.",
    "MeoGhiNho": "Mắc nối tiếp: Đèn công suất định mức nhỏ hơn sẽ có điện trở lớn hơn và sáng hơn.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

print(f"Prepared {len(mcq_batch3)} MCQs for Batch 3.")
