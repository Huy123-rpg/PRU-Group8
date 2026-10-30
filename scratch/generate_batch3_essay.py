# -*- coding: utf-8 -*-
import sys
sys.stdout.reconfigure(encoding='utf-8')

essay_batch3 = []

# ==============================================================================
# BÀI 11: ĐIỆN TRỞ. ĐỊNH LUẬT OHM (C3_L11) - 16 CÂU (4 NB, 4 TH, 4 VD, 4 VDC)
# ==============================================================================

# --- NHẬN BIẾT (Dễ - Nền tảng) - Thầy Minh (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_145", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Phát biểu định luật Ohm đối với đoạn mạch. Viết hệ thức của định luật và nêu rõ tên gọi, đơn vị của từng đại lượng.",
    "TuKhoaChamDiem": "cường độ dòng điện; tỉ lệ thuận với hiệu điện thế; tỉ lệ nghịch với điện trở; I = U / R; Ampe; Vôn; Ôm",
    "DapAnTuLuanMau": "• B1 (Phát biểu định luật Ohm - 2.5đ): Cường độ dòng điện chạy qua một dây dẫn tỉ lệ thuận với hiệu điện thế đặt vào hai đầu dây và tỉ lệ nghịch với điện trở của dây dẫn đó.\n• B2 (Hệ thức định luật - 2.5đ): I = U / R.\n• B3 (Giải thích tên gọi các đại lượng - 2.5đ): U là hiệu điện thế giữa hai đầu đoạn mạch; I là cường độ dòng điện chạy qua đoạn mạch; R là điện trở của đoạn mạch.\n• B4 (Đơn vị đo chuẩn - 2.5đ): Trong hệ SI, hiệu điện thế U đo bằng Vôn (V); cường độ dòng điện I đo bằng Ampe (A); điện trở R đo bằng Ôm (Ω).",
    "GiaiThich_MeoGhiNho": "I = U / R. Dòng điện tỉ lệ thuận U, tỉ lệ nghịch R.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_146", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Điện trở của một dây dẫn phụ thuộc vào những yếu tố hình học và vật liệu nào? Viết công thức tính điện trở dây dẫn.",
    "TuKhoaChamDiem": "chiều dài l; tiết diện S; vật liệu làm dây; điện trở suất rho; R = rho . l / S",
    "DapAnTuLuanMau": "• B1 (Các yếu tố ảnh hưởng - 2.5đ): Điện trở của dây dẫn phụ thuộc vào ba yếu tố: chiều dài của dây (l), tiết diện của dây (S) và bản chất vật liệu làm dây (đặc trưng bởi điện trở suất rho).\n• B2 (Mối liên hệ cụ thể - 2.5đ): Điện trở tỉ lệ thuận với chiều dài dây, tỉ lệ nghịch với tiết diện dây và tỉ lệ thuận với điện trở suất của vật liệu.\n• B3 (Công thức định lượng - 2.5đ): R = rho · (l / S).\n• B4 (Đơn vị trong công thức - 2.5đ): l đo bằng mét (m); S đo bằng mét vuông (m^2); rho đo bằng Ôm mét (Ω·m); R đo bằng Ôm (Ω).",
    "GiaiThich_MeoGhiNho": "R = rho . l / S (dài tăng thì R tăng, béo tăng thì R giảm).",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_147", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Ý nghĩa vật lý của điện trở suất (rho) là gì? Trong các kim loại thông dụng (đồng, nhôm, sắt), kim loại nào dẫn điện tốt nhất và có điện trở suất nhỏ nhất?",
    "TuKhoaChamDiem": "điện trở suất đặc trưng cản trở dòng điện của vật liệu; hình trụ dài 1m tiết diện 1m^2; đồng dẫn điện tốt nhất; rho đồng nhỏ nhất",
    "DapAnTuLuanMau": "• B1 (Ý nghĩa vật lý của điện trở suất - 2.5đ): Điện trở suất đặc trưng cho khả năng cản trở dòng điện của vật liệu làm dây, có giá trị bằng điện trở của một đoạn dây hình trụ làm bằng vật liệu đó có chiều dài 1 m và tiết diện thẳng 1 m^2.\n• B2 (Đơn vị đo - 2.5đ): Ký hiệu là rho, đơn vị đo trong hệ SI là Ôm mét (Ω·m).\n• B3 (So sánh trong kim loại thông dụng - 2.5đ): Đồng (rho ≈ 1,7.10^-8 Ω·m) có điện trở suất nhỏ hơn nhôm (2,8.10^-8 Ω·m) và sắt (12.10^-8 Ω·m).\n• B4 (Kết luận khả năng dẫn điện - 2.5đ): Đồng là kim loại dẫn điện tốt nhất trong 3 loại trên nên được dùng phổ biến nhất làm lõi dây điện gia dụng.",
    "GiaiThich_MeoGhiNho": "Điện trở suất càng nhỏ thì vật liệu dẫn điện càng tốt (bạc < đồng < vàng < nhôm < sắt).",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_148", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Biến trở là gì? Nêu cấu tạo và công dụng của biến trở con chạy trong mạch điện thí nghiệm.",
    "TuKhoaChamDiem": "điện trở có thể thay đổi trị số; cuộn dây dẫn điện trở suất lớn; con chạy hoặc tay xoay; điều chỉnh cường độ dòng điện",
    "DapAnTuLuanMau": "• B1 (Định nghĩa biến trở - 2.5đ): Biến trở là một dụng cụ điện mà điện trở của nó có thể thay đổi trị số theo ý muốn của người sử dụng.\n• B2 (Cấu tạo biến trở con chạy - 2.5đ): Gồm một cuộn dây hợp kim có điện trở suất lớn quấn quanh một lõi sứ cách điện và một con chạy trượt tiếp xúc dọc cuộn dây.\n• B3 (Nguyên lý hoạt động - 2.5đ): Khi dịch chuyển con chạy, chiều dài phần cuộn dây có dòng điện chạy qua thay đổi, dẫn đến điện trở của biến trở thay đổi theo.\n• B4 (Công dụng chính - 2.5đ): Dùng để điều chỉnh cường độ dòng điện trong mạch hoặc điều chỉnh hiệu điện thế giữa hai đầu thiết bị điện.",
    "GiaiThich_MeoGhiNho": "Biến trở dùng để điều chỉnh cường độ dòng điện trong mạch bằng cách thay đổi chiều dài dây dẫn.",
    "TrangThai": "Xuất bản"
})

# --- THÔNG HIỂU (Trung bình - Ứng dụng thực tế) - Cô Lan (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_149", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Tại sao các dây dẫn điện cao thế đường dài thường dùng dây nhôm có lõi thép bên trong mà không dùng dây đồng nguyên chất?",
    "TuKhoaChamDiem": "nhôm nhẹ và giá thành rẻ; lõi thép tăng độ bền cơ học; đồng quá nặng và đắt đỏ; dây võng đứt",
    "DapAnTuLuanMau": "• B1 (Đặc tính khối lượng riêng - 2.5đ): Nhôm có khối lượng riêng rất nhỏ (D_nhôm ≈ 2700 kg/m^3) so với đồng (D_đồng ≈ 8900 kg/m^3), dây nhôm nhẹ hơn dây đồng hơn 3 lần.\n• B2 (Yếu tố chịu lực cơ học trên cao - 2.5đ): Đường dây cao thế trải dài hàng trăm kilômét treo trên các cột thép cao; nếu dùng đồng dây sẽ quá nặng làm võng gãy cột hoặc đứt dây. Nhôm nhẹ giúp giảm tải trọng lên cột điện.\n• B3 (Tác dụng của lõi thép gia cường - 2.5đ): Phần lõi thép ở giữa có độ bền kéo rất cao giúp chịu lực căng của đường dây, trong khi lớp nhôm bọc ngoài đóng vai trò dẫn điện.\n• B4 (Hiệu quả kinh tế - 2.5đ): Nhôm có trữ lượng lớn và giá thành rẻ hơn nhiều so với đồng, tiết kiệm hàng nghìn tỉ đồng chi phí đầu tư lưới điện quốc gia.",
    "GiaiThich_MeoGhiNho": "Dây cao thế dùng nhôm lõi thép: nhẹ, rẻ và lõi thép chịu lực căng cực tốt.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_150", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Giải thích nguyên lý làm việc của núm vặn điều chỉnh độ sáng bóng đèn (chiết áp / dimmer) hoặc núm chỉnh âm lượng loa dựa trên kiến thức về biến trở.",
    "TuKhoaChamDiem": "biến trở xoay; thay đổi chiều dài lớp điện trở; thay đổi điện trở mạch; thay đổi cường độ dòng điện; bóng đèn sáng tối",
    "DapAnTuLuanMau": "• B1 (Bản chất cấu tạo của chiết áp - 2.5đ): Núm vặn bên trong thực chất là một biến trở xoay có con trượt tiếp xúc quét trên một vành điện trở bằng than hoặc hợp kim.\n• B2 (Cơ chế thay đổi điện trở khi vặn núm - 2.5đ): Khi ta xoay núm vặn, vị trí con trượt dịch chuyển làm thay đổi chiều dài phần dây dẫn tham gia vào mạch điện, dẫn đến điện trở R của chiết áp tăng hoặc giảm.\n• B3 (Tác động lên cường độ dòng điện - 2.5đ): Theo định luật Ohm I = U / R, khi R tăng thì dòng điện I qua đèn giảm đi; khi R giảm thì dòng điện I tăng lên.\n• B4 (Kết quả điều chỉnh thực tế - 2.5đ): Dòng điện thay đổi làm công suất tiêu thụ của đèn thay đổi, giúp điều chỉnh đèn sáng rực rỡ hay mờ dịu theo ý muốn.",
    "GiaiThich_MeoGhiNho": "Vặn núm chiết áp = thay đổi điện trở biến trở -> đổi dòng điện I -> đổi độ sáng / âm lượng.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_151", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Tại sao các dây dẫn nối các thiết bị điện công suất lớn (như bình nóng lạnh, bếp từ, điều hòa) trong nhà luôn phải dùng loại dây có lõi đồng to (tiết diện lớn 2,5 - 4 mm^2) thay vì dùng dây mảnh?",
    "TuKhoaChamDiem": "công suất lớn dòng điện lớn; tiết diện S lớn điện trở R nhỏ; giảm nhiệt lượng tỏa ra Q = I^2.R.t; chống cháy nổ chập điện",
    "DapAnTuLuanMau": "• B1 (Dòng điện tiêu thụ của thiết bị công suất lớn - 2.5đ): Các thiết bị công suất lớn tiêu thụ dòng điện rất mạnh (từ 10 A đến 20 A).\n• B2 (Mối liên hệ giữa tiết diện và điện trở - 2.5đ): Theo công thức R = rho · l / S, dây có tiết diện S lớn sẽ có điện trở R rất nhỏ.\n• B3 (Nhiệt lượng tỏa ra trên đường dây - 2.5đ): Theo định luật Joule - Lenz, nhiệt lượng tỏa ra trên dây dẫn tính bằng Q = I^2 · R · t. Vì I rất lớn nên nếu R nhỏ thì dây chỉ ấm nhẹ.\n• B4 (Nguy cơ nếu dùng dây mảnh - 2.5đ): Nếu dùng dây lõi mảnh (S nhỏ => R lớn), dòng điện lớn sẽ làm dây nóng đỏ rực, làm chảy lớp vỏ bọc cách điện PVC gây chập cháy hỏa hoạn nghiêm trọng.",
    "GiaiThich_MeoGhiNho": "Dây to -> R nhỏ -> Q tỏa nhiệt ít -> an toàn, không bị nóng chảy vỏ nhựa.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_152", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Trên vỏ một biến trở con chạy có ghi 50 Ω – 2,5 A. Các con số này cho biết điều gì? Hiệu điện thế lớn nhất được phép đặt vào hai đầu biến trở này là bao nhiêu?",
    "TuKhoaChamDiem": "điện trở lớn nhất 50 Ôm; cường độ dòng điện định mức tối đa 2.5 Ampe; U_max = I_max . R_max = 125 V",
    "DapAnTuLuanMau": "• B1 (Ý nghĩa thông số 50 Ω - 2.5đ): 50 Ω là giá trị điện trở lớn nhất của biến trở (khi con chạy ở vị trí đưa toàn bộ chiều dài cuộn dây vào mạch).\n• B2 (Ý nghĩa thông số 2,5 A - 2.5đ): 2,5 A là cường độ dòng điện định mức lớn nhất được phép chạy qua biến trở mà không làm cuộn dây bị quá nhiệt hay cháy hỏng.\n• B3 (Tính hiệu điện thế tối đa cho phép - 2.5đ): Áp dụng định luật Ohm: U_max = I_max · R_max = 2,5 · 50 = 125 V.\n• B4 (Kết luận an toàn - 2.5đ): Không được đặt vào hai đầu biến trở hiệu điện thế vượt quá 125 V để đảm bảo an toàn thiết bị.",
    "GiaiThich_MeoGhiNho": "50Ω là R_max; 2,5A là I_max. Hiệu điện thế tối đa U_max = I_max . R_max = 125 V.",
    "TrangThai": "Xuất bản"
})

# --- VẬN DỤNG (Khó - Tính toán nâng cao) - Thầy Duy (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_153", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Một cuộn dây dẫn bằng đồng có điện trở R = 6,8 Ω. Dây có tiết diện tròn đường kính d = 0,4 mm. Cho điện trở suất của đồng rho = 1,7.10^-8 Ω.m (lấy pi = 3,14). a) Tính tiết diện S của sợi dây đồng ra đơn vị mét vuông. b) Tính chiều dài l của sợi dây trong cuộn.",
    "TuKhoaChamDiem": "d = 0.4 mm; r = 0.2 mm; S = 1.256.10^-7 m^2; R = 6.8 Ôm; l = 50.24 m",
    "DapAnTuLuanMau": "• B1 (Đổi đơn vị và tính bán kính - 2.5đ): Bán kính sợi dây: r = d / 2 = 0,4 / 2 = 0,2 mm = 0,2 · 10^-3 m = 2 · 10^-4 m.\n• B2 (Tính tiết diện S của sợi dây - 2.5đ): S = pi · r^2 = 3,14 · (2 · 10^-4)^2 = 3,14 · 4 · 10^-8 = 1,256 · 10^-7 m^2.\n• B3 (Công thức liên hệ chiều dài - 2.5đ): Từ công thức R = rho · (l / S) => l = (R · S) / rho.\n• B4 (Tính chiều dài l - 2.5đ): l = (6,8 · 1,256 · 10^-7) / (1,7 · 10^-8) = 4 · 1,256 · 10 = 50,24 m. Đáp số: a) S = 1,256.10^-7 m^2; b) Chiều dài l = 50,24 m.",
    "GiaiThich_MeoGhiNho": "S = pi . d^2 / 4; l = R . S / rho. Chú ý rút gọn 6,8 / 1,7 = 4.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_154", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Hai dây dẫn hình trụ được làm cùng một loại vật liệu có cùng khối lượng. Biết dây thứ nhất dài gấp 4 lần dây thứ hai (l1 = 4 l2). Hãy tính tỉ số điện trở R1 / R2 của hai dây dẫn này.",
    "TuKhoaChamDiem": "cùng khối lượng cùng thể tích; V1 = V2; l1 = 4 l2 => S2 = 4 S1; R1 / R2 = 16",
    "DapAnTuLuanMau": "• B1 (Liên hệ thể tích và chiều dài - 2.5đ): Vì hai dây làm cùng vật liệu và có cùng khối lượng nên thể tích của hai dây bằng nhau: V1 = V2.\n• B2 (Mối liên hệ giữa tiết diện và chiều dài - 2.5đ): Ta có V = S · l => S1 · l1 = S2 · l2 => S1 / S2 = l2 / l1 = 1 / 4 => S2 = 4 · S1.\n• B3 (Lập tỉ số điện trở hai dây - 2.5đ): R1 / R2 = (rho · l1 / S1) / (rho · l2 / S2) = (l1 / l2) · (S2 / S1).\n• B4 (Tính giá trị tỉ số - 2.5đ): Thay l1 / l2 = 4 và S2 / S1 = 4 vào: R1 / R2 = 4 · 4 = 16. Đáp số: R1 / R2 = 16 (Điện trở dây thứ nhất gấp 16 lần dây thứ hai).",
    "GiaiThich_MeoGhiNho": "Cùng khối lượng kéo dài gấp n lần thì tiết diện giảm n lần -> R tăng n^2 lần.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_155", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Một biến trở con chạy có cuộn dây làm bằng hợp kim nicrom (điện trở suất rho = 1,1.10^-6 Ω.m), đường kính tiết diện dây d = 0,8 mm được quấn đều trên một lõi sứ hình trụ tròn có đường kính D = 2,5 cm. Biết toàn bộ cuộn dây có 500 vòng quấn sát nhau. Tính điện trở lớn nhất của biến trở này (lấy pi = 3,14).",
    "TuKhoaChamDiem": "D = 0.025 m; chu vi C = pi.D = 0.0785 m; l = N.C = 39.25 m; S = 5.024.10^-7 m^2; R_max = 85.9 Ôm",
    "DapAnTuLuanMau": "• B1 (Tính chiều dài mỗi vòng dây và tổng chiều dài cuộn dây - 2.5đ): Chu vi một vòng quấn trên lõi sứ: C = pi · D = 3,14 · 0,025 = 0,0785 m. Tổng chiều dài sợi dây quấn 500 vòng: l = N · C = 500 · 0,0785 = 39,25 m.\n• B2 (Tính tiết diện sợi dây nicrom - 2.5đ): Bán kính r = d / 2 = 0,4 mm = 0,4 · 10^-3 m. Tiết diện: S = pi · r^2 = 3,14 · (0,4 · 10^-3)^2 = 3,14 · 0,16 · 10^-6 = 5,024 · 10^-7 m^2.\n• B3 (Áp dụng công thức tính điện trở - 2.5đ): R_max = rho · (l / S) = (1,1 · 10^-6 · 39,25) / (5,024 · 10^-7).\n• B4 (Tính kết quả - 2.5đ): R_max = (1,1 · 39,25 · 10) / 5,024 = 431,75 / 5,024 ≈ 85,94 Ω (xấp xỉ 86 Ω). Đáp số: R_max ≈ 85,9 Ω.",
    "GiaiThich_MeoGhiNho": "Chiều dài l = N . pi . D_lõi. Tiết diện S = pi . (d_dây / 2)^2. R = rho . l / S.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_156", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Đặt một hiệu điện thế U vào hai đầu một dây dẫn thì dòng điện qua dây là I. Nếu tăng hiệu điện thế thêm 5 V thì dòng điện qua dây tăng thêm 0,25 A. Nếu giảm hiệu điện thế đi 3 V thì cường độ dòng điện qua dây bằng bao nhiêu?",
    "TuKhoaChamDiem": "Delta_U1 = 5 V; Delta_I1 = 0.25 A; R = Delta_U / Delta_I = 20 Ôm; Delta_U2 = -3 V; I giảm 0.15 A",
    "DapAnTuLuanMau": "• B1 (Mối quan hệ biến thiên theo định luật Ohm - 2.5đ): Điện trở R của dây dẫn không đổi, theo định luật Ohm ta có: Delta_I = Delta_U / R => R = Delta_U / Delta_I.\n• B2 (Tính giá trị điện trở của dây dẫn - 2.5đ): R = Delta_U1 / Delta_I1 = 5 / 0,25 = 20 Ω.\n• B3 (Tính độ giảm của cường độ dòng điện khi giảm U đi 3 V - 2.5đ): Khi giảm hiệu điện thế một lượng Delta_U2 = 3 V, dòng điện giảm tương ứng một lượng: Delta_I2 = Delta_U2 / R = 3 / 20 = 0,15 A.\n• B4 (Kết luận cường độ dòng điện lúc sau - 2.5đ): Cường độ dòng điện mới qua dây là: I2 = I - 0,15 (A) (giảm đi 0,15 A so với ban đầu). Đáp số: Điện trở dây là 20 Ω; dòng điện giảm bớt 0,15 A.",
    "GiaiThich_MeoGhiNho": "R = ΔU / ΔI. Vì R = const nên ΔI tỉ lệ thuận với ΔU.",
    "TrangThai": "Xuất bản"
})

# --- VẬN DỤNG CAO (Tổng hợp - Chuyên sâu) - Cô Hà (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_157", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một đường dây tải điện đôi (gồm hai dây đi và về) bằng đồng dài L = 5 km nối từ trạm biến áp đến một xưởng sản xuất. Tiết diện mỗi sợi dây là S = 25 mm^2, điện trở suất của đồng rho = 1,7.10^-8 Ω.m. a) Tính tổng điện trở của cả đường dây đôi này. b) Nếu dòng điện tải qua dây là I = 50 A, hãy tính độ sụt áp (độ giảm thế) trên toàn bộ đường dây.",
    "TuKhoaChamDiem": "đường dây đôi tổng chiều dài 2L = 10 km = 10000 m; S = 25.10^-6 m^2; R_dây = 6.8 Ôm; I = 50 A; độ sụt áp Delta_U = 340 V",
    "DapAnTuLuanMau": "• B1 (Tính tổng chiều dài của đường dây đôi - 2.5đ): Đường dây đôi gồm hai sợi dây (dây pha và dây trung tính) nên tổng chiều dài dây dẫn điện là: l_tổng = 2 · L = 2 · 5 km = 10 km = 10000 m.\n• B2 (Đổi đơn vị tiết diện và tính điện trở dây - 2.5đ): S = 25 mm^2 = 25 · 10^-6 m^2. Tổng điện trở đường dây: R_dây = rho · (l_tổng / S) = (1,7 · 10^-8 · 10000) / (25 · 10^-6) = 1,7 · 10^-4 / (25 · 10^-6) = 170 / 25 = 6,8 Ω.\n• B3 (Tính độ sụt áp trên đường dây - 2.5đ): Áp dụng định luật Ohm: Delta_U = I · R_dây = 50 · 6,8 = 340 V.\n• B4 (Kết luận ý nghĩa thực tế - 2.5đ): Độ sụt áp lên tới 340 V là rất lớn, do đó trong kỹ thuật phải tăng hiệu điện thế truyền tải lên hàng chục kilôvôn để giảm dòng điện I, từ đó hạn chế tối đa sụt áp. Đáp số: a) R_dây = 6,8 Ω; b) Delta_U = 340 V.",
    "GiaiThich_MeoGhiNho": "Đường dây đôi nhớ nhân đôi chiều dài: l = 2L. Độ sụt áp ΔU = I . R_dây.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_158", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Dây xoắn của bếp điện được làm bằng hợp kim nicrom dài 12 m, tiết diện 0,2 mm^2 (rho = 1,1.10^-6 Ω.m). Người ta cắt bớt 2 m của sợi dây rồi mắc đoạn còn lại vào nguồn điện U = 220 V. a) Tính điện trở của bếp điện sau khi cắt bớt dây. b) Cường độ dòng điện qua bếp tăng hay giảm bao nhiêu Ampe so với trước khi cắt?",
    "TuKhoaChamDiem": "R1 = 66 Ôm; l2 = 10 m; R2 = 55 Ôm; I1 = 3.33 A; I2 = 4.0 A; tăng thêm 0.67 A",
    "DapAnTuLuanMau": "• B1 (Tính điện trở ban đầu của bếp - 2.5đ): S = 0,2 · 10^-6 m^2. Điện trở ban đầu: R1 = rho · l1 / S = (1,1 · 10^-6 · 12) / (0,2 · 10^-6) = 1,1 · 60 = 66 Ω.\n• B2 (Tính điện trở sau khi cắt bớt dây - 2.5đ): Chiều dài còn lại: l2 = 12 - 2 = 10 m. Điện trở mới: R2 = rho · l2 / S = (1,1 · 10^-6 · 10) / (0,2 · 10^-6) = 1,1 · 50 = 55 Ω.\n• B3 (Tính cường độ dòng điện trong hai trường hợp - 2.5đ): Ban đầu: I1 = U / R1 = 220 / 66 ≈ 3,33 A. Lúc sau: I2 = U / R2 = 220 / 55 = 4,00 A.\n• B4 (Đánh giá độ tăng của dòng điện - 2.5đ): Độ biến thiên dòng điện: Delta_I = I2 - I1 = 4,00 - 3,33 = 0,67 A. Dòng điện tăng thêm 0,67 A (làm bếp nóng nhanh hơn). Đáp số: a) R2 = 55 Ω; b) Dòng điện tăng thêm 0,67 A.",
    "GiaiThich_MeoGhiNho": "Cắt ngắn dây -> R giảm -> dòng điện I và công suất nhiệt P tăng lên.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_159", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một biến trở con chạy có điện trở toàn phần R_AB = 100 Ω mắc nối tiếp với bóng đèn Đ (12 V – 6 W) vào nguồn điện có hiệu điện thế không đổi U = 24 V. a) Tính điện trở của bóng đèn và dòng điện định mức của đèn. b) Phải điều chỉnh phần biến trở tham gia vào mạch R_x bằng bao nhiêu để đèn sáng đúng định mức? Khi đó con chạy đang ở vị trí nào so với toàn bộ chiều dài biến trở?",
    "TuKhoaChamDiem": "R_đèn = 24 Ôm; I_đm = 0.5 A; U_x = 12 V; R_x = 24 Ôm; con chạy ở vị trí 24% chiều dài",
    "DapAnTuLuanMau": "• B1 (Tính thông số định mức của đèn - 2.5đ): Điện trở bóng đèn: R_đèn = U_đm^2 / P_đm = 12^2 / 6 = 144 / 6 = 24 Ω. Cường độ định mức: I_đm = P_đm / U_đm = 6 / 12 = 0,5 A.\n• B2 (Xác định hiệu điện thế hai đầu biến trở - 2.5đ): Để đèn sáng bình thường, dòng điện qua mạch là I = I_đm = 0,5 A. Hiệu điện thế hai đầu biến trở: U_x = U - U_đm = 24 - 12 = 12 V.\n• B3 (Tính giá trị điện trở phần tham gia R_x - 2.5đ): R_x = U_x / I = 12 / 0,5 = 24 Ω.\n• B4 (Xác định vị trí con chạy - 2.5đ): Tỉ lệ phần biến trở đưa vào mạch: k = R_x / R_AB = 24 / 100 = 0,24 = 24%. Con chạy ở vị trí cách đầu mắc dây một khoảng bằng 24% tổng chiều dài cuộn dây. Đáp số: a) R_đèn = 24 Ω, I_đm = 0,5 A; b) R_x = 24 Ω, chiếm 24% chiều dài.",
    "GiaiThich_MeoGhiNho": "R_x = (U - U_đm) / I_đm. Vị trí con chạy tỉ lệ thuận với R_x / R_toàn_phần.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_160", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L11", "TenBai": "Bài 11. Điện trở. Định luật Ohm", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một nguồn điện không đổi duy trì hiệu điện thế U = 12 V. Khi mắc điện trở R1 vào nguồn thì dòng điện là 1,2 A. Khi mắc điện trở R2 vào nguồn thì dòng điện là 0,8 A. Hỏi nếu mắc hai điện trở này nối tiếp vào nguồn U thì dòng điện trong mạch là bao nhiêu? Nếu mắc song song thì dòng điện mạch chính là bao nhiêu?",
    "TuKhoaChamDiem": "R1 = 10 Ôm; R2 = 15 Ôm; R_nt = 25 Ôm => I_nt = 0.48 A; R_ss = 6 Ôm => I_ss = 2.0 A",
    "DapAnTuLuanMau": "• B1 (Tính giá trị từng điện trở R1 và R2 - 2.5đ): R1 = U / I1 = 12 / 1,2 = 10 Ω. R2 = U / I2 = 12 / 0,8 = 15 Ω.\n• B2 (Trường hợp mắc nối tiếp - 2.5đ): Điện trở tương đương nối tiếp: R_nt = R1 + R2 = 10 + 15 = 25 Ω. Cường độ dòng điện mạch nối tiếp: I_nt = U / R_nt = 12 / 25 = 0,48 A.\n• B3 (Trường hợp mắc song song - 2.5đ): Điện trở tương đương song song: R_ss = (R1 · R2) / (R1 + R2) = (10 · 15) / 25 = 150 / 25 = 6 Ω. Cường độ dòng điện mạch chính song song: I_ss = U / R_ss = 12 / 6 = 2,0 A (hoặc I_ss = I1 + I2 = 1,2 + 0,8 = 2,0 A).\n• B4 (Kết luận - 2.5đ): Đáp số: Khi mắc nối tiếp I_nt = 0,48 A; Khi mắc song song I_ss = 2,0 A.",
    "GiaiThich_MeoGhiNho": "Nối tiếp dòng điện giảm (I_nt = 0,48 A); Song song dòng điện tăng bằng tổng (I_ss = I1 + I2 = 2 A).",
    "TrangThai": "Xuất bản"
})

# ==============================================================================
# BÀI 12: ĐOẠN MẠCH NỐI TIẾP, SONG SONG (C3_L12) - 16 CÂU (4 NB, 4 TH, 4 VD, 4 VDC)
# ==============================================================================

# --- NHẬN BIẾT (Dễ - Nền tảng) - Thầy Minh (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_161", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Nêu các đặc điểm của cường độ dòng điện, hiệu điện thế và điện trở tương đương trong đoạn mạch gồm hai điện trở R1 và R2 mắc nối tiếp.",
    "TuKhoaChamDiem": "I = I1 = I2; U = U1 + U2; R_td = R1 + R2; U1 / U2 = R1 / R2",
    "DapAnTuLuanMau": "• B1 (Cường độ dòng điện trong mạch nối tiếp - 2.5đ): Cường độ dòng điện có giá trị như nhau tại mọi điểm: I = I1 = I2.\n• B2 (Hiệu điện thế trong mạch nối tiếp - 2.5đ): Hiệu điện thế giữa hai đầu đoạn mạch bằng tổng hiệu điện thế giữa hai đầu mỗi điện trở: U = U1 + U2.\n• B3 (Điện trở tương đương - 2.5đ): Điện trở tương đương của đoạn mạch bằng tổng các điện trở thành phần: R_td = R1 + R2 (R_td luôn lớn hơn từng điện trở thành phần).\n• B4 (Tỉ lệ hiệu điện thế - 2.5đ): Hiệu điện thế giữa hai đầu mỗi điện trở tỉ lệ thuận với điện trở đó: U1 / U2 = R1 / R2.",
    "GiaiThich_MeoGhiNho": "Mạch nối tiếp: I bằng nhau, U cộng lại, R cộng lại.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_162", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Nêu các đặc điểm của cường độ dòng điện, hiệu điện thế và điện trở tương đương trong đoạn mạch gồm hai điện trở R1 và R2 mắc song song.",
    "TuKhoaChamDiem": "U = U1 = U2; I = I1 + I2; 1/R_td = 1/R1 + 1/R2; R_td = (R1.R2)/(R1+R2); I1/I2 = R2/R1",
    "DapAnTuLuanMau": "• B1 (Hiệu điện thế trong mạch song song - 2.5đ): Hiệu điện thế giữa hai đầu đoạn mạch song song bằng hiệu điện thế giữa hai đầu mỗi đoạn mạch rẽ: U = U1 = U2.\n• B2 (Cường độ dòng điện trong mạch chính - 2.5đ): Cường độ dòng điện trong mạch chính bằng tổng cường độ dòng điện trong các mạch rẽ: I = I1 + I2.\n• B3 (Điện trở tương đương - 2.5đ): Nghịch đảo điện trở tương đương bằng tổng nghịch đảo các điện trở thành phần: 1 / R_td = 1 / R1 + 1 / R2 (suy ra R_td = (R1 · R2) / (R1 + R2), R_td luôn nhỏ hơn từng điện trở thành phần).\n• B4 (Tỉ lệ dòng điện - 2.5đ): Cường độ dòng điện trong mỗi nhánh tỉ lệ nghịch với điện trở nhánh đó: I1 / I2 = R2 / R1.",
    "GiaiThich_MeoGhiNho": "Mạch song song: U bằng nhau, I cộng lại, 1/R cộng lại.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_163", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Viết công thức tính điện trở tương đương khi mắc n điện trở giống nhau cùng có giá trị R: a) Khi mắc nối tiếp; b) Khi mắc song song.",
    "TuKhoaChamDiem": "n điện trở giống nhau; mắc nối tiếp R_nt = n . R; mắc song song R_ss = R / n",
    "DapAnTuLuanMau": "• B1 (Trường hợp mắc nối tiếp - 2.5đ): R_td = R + R + ... + R (n lần).\n• B2 (Công thức rút gọn mắc nối tiếp - 2.5đ): R_nt = n · R.\n• B3 (Trường hợp mắc song song - 2.5đ): 1 / R_td = 1/R + 1/R + ... + 1/R = n / R.\n• B4 (Công thức rút gọn mắc song song - 2.5đ): R_ss = R / n.",
    "GiaiThich_MeoGhiNho": "n chiếc giống nhau: Nối tiếp nhân n, Song song chia n.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_164", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Trong mạch điện thực tế, ampe kế và vôn kế được mắc như thế nào với đoạn mạch cần đo? Nêu yêu cầu về điện trở lý tưởng của từng dụng cụ.",
    "TuKhoaChamDiem": "ampe kế mắc nối tiếp có điện trở rất nhỏ RA xấp xỉ 0; vôn kế mắc song song có điện trở rất lớn RV vô cùng lớn",
    "DapAnTuLuanMau": "• B1 (Cách mắc và điện trở ampe kế - 2.5đ): Ampe kế mắc nối tiếp với đoạn mạch cần đo cường độ dòng điện. Điện trở của ampe kế lý tưởng rất nhỏ (R_A ≈ 0) để không làm thay đổi điện trở của mạch.\n• B2 (Hậu quả nếu mắc sai ampe kế - 2.5đ): Tuyệt đối không mắc ampe kế song song với nguồn vì sẽ gây đoản mạch làm cháy dụng cụ.\n• B3 (Cách mắc và điện trở vôn kế - 2.5đ): Vôn kế mắc song song với đoạn mạch cần đo hiệu điện thế. Điện trở của vôn kế lý tưởng rất lớn (R_V vô cùng lớn) để dòng điện qua vôn kế xấp xỉ bằng 0.\n• B4 (Kết luận nguyên tắc đo lường - 2.5đ): Nhờ vậy sự có mặt của thiết bị đo không làm ảnh hưởng đến hoạt động bình thường của mạch điện.",
    "GiaiThich_MeoGhiNho": "Ampe kế nối tiếp (R_A = 0); Vôn kế song song (R_V = vô cùng).",
    "TrangThai": "Xuất bản"
})

# --- THÔNG HIỂU (Trung bình - Ứng dụng thực tế) - Cô Lan (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_165", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Tại sao hệ thống điện chiếu sáng và các thiết bị điện trong các gia đình (đèn, quạt, tivi, tủ lạnh) luôn được mắc song song với nhau mà không mắc nối tiếp?",
    "TuKhoaChamDiem": "mắc song song cùng hiệu điện thế định mức 220V; hoạt động độc lập; tắt một thiết bị các thiết bị khác vẫn hoạt động",
    "DapAnTuLuanMau": "• B1 (Hiệu điện thế định mức độc lập - 2.5đ): Mạng điện dân dụng cung cấp hiệu điện thế chuẩn 220 V. Mắc song song đảm bảo mỗi thiết bị đều nhận đủ hiệu điện thế định mức 220 V để hoạt động đúng công suất thiết kế.\n• B2 (Tính độc lập trong vận hành - 2.5đ): Mắc song song tạo ra các nhánh mạch rẽ riêng biệt. Khi bật hoặc tắt một thiết bị, các thiết bị ở nhánh khác vẫn hoạt động bình thường.\n• B3 (Bảo toàn khi có sự cố - 2.5đ): Nếu một bóng đèn bị cháy đứt tóc, mạch của các thiết bị khác không bị hở, sinh hoạt trong nhà không bị gián đoạn.\n• B4 (Hạn chế nếu mắc nối tiếp - 2.5đ): Nếu mắc nối tiếp, hiệu điện thế 220 V sẽ bị chia nhỏ khiến các thiết bị yếu không chạy được, và hỏng một thiết bị thì cả nhà đều mất điện.",
    "GiaiThich_MeoGhiNho": "Mắc song song trong nhà: Đảm bảo đủ 220V cho từng máy và chạy độc lập không ảnh hưởng lẫn nhau.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_166", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Trong các dây đèn trang trí cây thông Noel kiểu truyền thống gồm 50 bóng đèn nhỏ mắc nối tiếp nhau. Nếu có một bóng đèn bị cháy đứt dây tóc thì toàn bộ dây đèn sẽ ra sao? Giải thích.",
    "TuKhoaChamDiem": "mạch nối tiếp; đứt một bóng mạch bị hở; dòng điện bằng 0; toàn bộ dây đèn đều tắt",
    "DapAnTuLuanMau": "• B1 (Tính chất mạch nối tiếp - 2.5đ): Trong đoạn mạch mắc nối tiếp, dòng điện chạy qua một đường dẫn liên tục khép kín từ bóng đèn này sang bóng đèn kế tiếp.\n• B2 (Hiện tượng khi một bóng đứt dây tóc - 2.5đ): Khi có một bóng đèn bị đứt dây tóc, dây tóc bị hở tạo ra một khe hở cách điện trong mạch.\n• B3 (Tình trạng toàn mạch - 2.5đ): Đoạn mạch bị ngắt (mạch hở), làm cường độ dòng điện trong toàn bộ mạch giảm về 0 (I = 0 A).\n• B4 (Kết luận hình ảnh - 2.5đ): Toàn bộ 49 bóng đèn còn lại đều đồng loạt bị tắt dù bản thân các bóng đó không hề bị hỏng.",
    "GiaiThich_MeoGhiNho": "Nối tiếp: Một bóng hỏng là hở mạch, cả dây cùng tắt.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_167", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Tại sao cầu chì hoặc aptomat (cầu dao tự động) bảo vệ mạch điện luôn bắt buộc phải mắc nối tiếp trên dây pha (dây nóng) của mạch điện?",
    "TuKhoaChamDiem": "mắc nối tiếp; chịu chung dòng điện toàn mạch; ngắt mạch kịp thời khi quá tải ngắn mạch; cô lập nguồn điện pha nóng an toàn",
    "DapAnTuLuanMau": "• B1 (Cơ chế cảm ứng dòng điện toàn phần - 2.5đ): Mắc nối tiếp giúp toàn bộ dòng điện chạy vào mạng điện gia đình đều phải đi qua cầu chì hoặc aptomat (I_bảo_vệ = I_mạch).\n• B2 (Tự động ngắt khi quá tải - 2.5đ): Khi xảy ra sự cố chập mạch (ngắn mạch) hoặc quá tải, dòng điện tăng vọt, dây chì nóng chảy đứt hoặc thanh lưỡng kim trong aptomat bật ngắt tức thì.\n• B3 (Tác dụng ngắt trên dây pha nóng - 2.5đ): Dây pha mang điện áp nguy hiểm 220 V so với đất. Khi ngắt trên dây nóng, toàn bộ hệ thống phía sau bị ngắt khỏi điện thế cao.\n• B4 (An toàn cho người sửa chữa - 2.5đ): Giúp con người chạm vào ổ cắm hay thiết bị không bị điện giật nguy hiểm đến tính mạng.",
    "GiaiThich_MeoGhiNho": "Cầu chì / Aptomat mắc nối tiếp trên dây nóng để cắt dòng điện ngay khi có sự cố.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_168", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Khi cắm đồng thời nhiều thiết bị điện công suất lớn (bếp từ, ấm siêu tốc, bàn là) vào cùng một ổ cắm kéo dài thì dây dẫn của ổ cắm thường bị nóng lên nhanh chóng. Giải thích nguyên nhân dựa vào tính chất của mạch mắc song song.",
    "TuKhoaChamDiem": "mắc song song; dòng điện mạch chính I = I1 + I2 + I3; dòng điện tăng rất lớn; nhiệt lượng Q = I^2.R.t tăng mạnh; nguy cơ quá tải",
    "DapAnTuLuanMau": "• B1 (Quy luật mạch song song tại ổ cắm - 2.5đ): Các ổ cắm trên thanh chia điện được nối song song với nhau vào nguồn điện 220 V.\n• B2 (Cộng dồn dòng điện mạch chính - 2.5đ): Dòng điện chạy qua dây nguồn chính của ổ cắm bằng tổng các dòng điện của từng thiết bị thành phần: I_tổng = I1 + I2 + I3.\n• B3 (Cường độ dòng điện vượt ngưỡng - 2.5đ): Mỗi thiết bị công suất lớn tiêu thụ từ 5 A đến 10 A, khiến dòng điện tổng trong dây cấp vượt quá 15 - 20 A.\n• B4 (Hiệu ứng tỏa nhiệt Joule - Lenz - 2.5đ): Nhiệt lượng tỏa ra trên dây tỉ lệ với bình phương cường độ dòng điện (Q = I^2 · R · t). Dòng điện tăng gấp 3 thì nhiệt lượng tăng gấp 9 lần, làm dây nóng ran và rất dễ bốc cháy.",
    "GiaiThich_MeoGhiNho": "Mắc song song nhiều thiết bị: I_tổng = sum(I) tăng vọt -> dây nguồn quá tải bốc nóng.",
    "TrangThai": "Xuất bản"
})

# --- VẬN DỤNG (Khó - Tính toán nâng cao) - Thầy Duy (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_169", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Cho mạch điện gồm R1 = 12 Ω mắc nối tiếp với R2 = 18 Ω vào nguồn điện có hiệu điện thế U = 15 V không đổi. a) Tính điện trở tương đương của mạch và cường độ dòng điện trong mạch. b) Mắc thêm điện trở R3 = 30 Ω song song với R2. Tính điện trở tương đương mới của toàn mạch và dòng điện chạy qua R1.",
    "TuKhoaChamDiem": "R_td1 = 30 Ôm; I1 = 0.5 A; R23 = 11.25 Ôm; R_td2 = 23.25 Ôm; I_mới = 0.645 A",
    "DapAnTuLuanMau": "• B1 (Tính câu a - Điện trở tương đương ban đầu - 2.5đ): R_td1 = R1 + R2 = 12 + 18 = 30 Ω.\n• B2 (Tính cường độ dòng điện ban đầu - 2.5đ): I = U / R_td1 = 15 / 30 = 0,5 A.\n• B3 (Tính câu b - Điện trở cụm song song R2 // R3 - 2.5đ): R23 = (R2 · R3) / (R2 + R3) = (18 · 30) / (18 + 30) = 540 / 48 = 11,25 Ω. Điện trở toàn mạch mới: R_td2 = R1 + R23 = 12 + 11,25 = 23,25 Ω.\n• B4 (Tính dòng điện qua R1 lúc sau - 2.5đ): R1 nằm trên mạch chính nên dòng điện qua R1 chính là dòng điện toàn mạch: I1' = U / R_td2 = 15 / 23,25 ≈ 0,645 A. Đáp số: a) R_td1 = 30 Ω, I = 0,5 A; b) R_td2 = 23,25 Ω, I1' ≈ 0,645 A.",
    "GiaiThich_MeoGhiNho": "R_td = R1 + (R2.R3)/(R2 + R3). Mắc thêm R song song làm R_td giảm -> dòng mạch chính tăng.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_170", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Hai điện trở R1 và R2 được mắc vào hiệu điện thế U = 24 V. Khi hai điện trở mắc nối tiếp, cường độ dòng điện trong mạch là I_nt = 0,6 A. Khi hai điện trở mắc song song, cường độ dòng điện mạch chính là I_ss = 2,5 A. Tính giá trị của mỗi điện trở R1 và R2.",
    "TuKhoaChamDiem": "R1 + R2 = 40 Ôm; (R1.R2)/(R1+R2) = 9.6 Ôm; R1.R2 = 384; R1 = 24 Ôm; R2 = 16 Ôm",
    "DapAnTuLuanMau": "• B1 (Phương trình khi mắc nối tiếp - 2.5đ): R_nt = R1 + R2 = U / I_nt = 24 / 0,6 = 40 Ω => R1 + R2 = 40 (1).\n• B2 (Phương trình khi mắc song song - 2.5đ): R_ss = (R1 · R2) / (R1 + R2) = U / I_ss = 24 / 2,5 = 9,6 Ω.\n• B3 (Tính tích hai điện trở - 2.5đ): Thay (1) vào biểu thức song song: (R1 · R2) / 40 = 9,6 => R1 · R2 = 40 · 9,6 = 384 (2).\n• B4 (Giải hệ phương trình theo định lý Viète - 2.5đ): R1 và R2 là hai nghiệm của phương trình: X^2 - 40X + 384 = 0. Delta' = (-20)^2 - 384 = 400 - 384 = 16 => căn(Delta') = 4. Nghiệm: X1 = 20 + 4 = 24 Ω; X2 = 20 - 4 = 16 Ω. Kết luận: Hai điện trở có giá trị là 24 Ω và 16 Ω (hoặc ngược lại).",
    "GiaiThich_MeoGhiNho": "Hệ phương trình tổng tích: Tổng S = 40, Tích P = 384 -> R1 = 24Ω, R2 = 16Ω.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_171", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Cho mạch điện gồm nguồn điện U = 18 V, điện trở R1 = 4 Ω mắc nối tiếp với đoạn mạch song song gồm R2 = 6 Ω và R3 = 12 Ω. a) Tính điện trở tương đương của toàn bộ đoạn mạch. b) Tính hiệu điện thế và cường độ dòng điện qua mỗi điện trở R1, R2, R3.",
    "TuKhoaChamDiem": "R23 = 4 Ôm; R_td = 8 Ôm; I1 = 2.25 A; U1 = 9 V; U2 = U3 = 9 V; I2 = 1.5 A; I3 = 0.75 A",
    "DapAnTuLuanMau": "• B1 (Tính điện trở tương đương - 2.5đ): R23 = (R2 · R3) / (R2 + R3) = (6 · 12) / (6 + 12) = 72 / 18 = 4 Ω. Điện trở toàn mạch: R_td = R1 + R23 = 4 + 4 = 8 Ω.\n• B2 (Tính thông số trên điện trở R1 - 2.5đ): Dòng điện qua mạch chính: I1 = I_toàn = U / R_td = 18 / 8 = 2,25 A. Hiệu điện thế hai đầu R1: U1 = I1 · R1 = 2,25 · 4 = 9 V.\n• B3 (Tính hiệu điện thế trên cụm song song - 2.5đ): U23 = U - U1 = 18 - 9 = 9 V. Vì R2 // R3 nên: U2 = U3 = U23 = 9 V.\n• B4 (Tính dòng điện qua R2 và R3 - 2.5đ): I2 = U2 / R2 = 9 / 6 = 1,5 A. I3 = U3 / R3 = 9 / 12 = 0,75 A. (Kiểm tra: I2 + I3 = 1,5 + 0,75 = 2,25 A = I1, chính xác). Đáp số: a) R_td = 8 Ω; b) R1 (9V, 2,25A); R2 (9V, 1,5A); R3 (9V, 0,75A).",
    "GiaiThich_MeoGhiNho": "R_td = R1 + (R2 // R3). U23 = U - U1. Dòng rẽ nhánh: I2 = U23 / R2, I3 = U23 / R3.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_172", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Hai bóng đèn Đ1 (6 V – 3 W) và Đ2 (6 V – 6 W) được mắc vào nguồn điện có hiệu điện thế U = 9 V. a) Nếu mắc Đ1 nối tiếp Đ2 vào nguồn U = 9 V thì hai đèn có sáng bình thường không? Tại sao? b) Để cả hai đèn cùng sáng bình thường thì phải mắc thêm một điện trở phụ R_x như thế nào và giá trị của R_x bằng bao nhiêu?",
    "TuKhoaChamDiem": "R1 = 12 Ôm; R2 = 6 Ôm; I1_đm = 0.5 A; I2_đm = 1.0 A; mắc Rx // Đ1 rồi nối tiếp Đ2; Rx = 12 Ôm",
    "DapAnTuLuanMau": "• B1 (Tính điện trở và dòng điện định mức mỗi đèn - 2.5đ): Đèn 1: R1 = U1^2 / P1 = 6^2 / 3 = 12 Ω; I1_đm = P1 / U1 = 3 / 6 = 0,5 A. Đèn 2: R2 = U2^2 / P2 = 6^2 / 6 = 6 Ω; I2_đm = P2 / U2 = 6 / 6 = 1,0 A.\n• B2 (Xét trường hợp mắc nối tiếp hai đèn - 2.5đ): Vì I1_đm = 0,5 A khác I2_đm = 1,0 A nên không thể mắc nối tiếp đơn thuần hai đèn. Nếu mắc nối tiếp, đèn 1 sẽ quá tải cháy trước hoặc đèn 2 quá tối.\n• B3 (Thiết kế mạch có điện trở phụ R_x - 2.5đ): Cả hai đèn đều có U_đm = 6 V, nguồn U = 9 V. Hiệu điện thế thừa là 9 - 6 = 3 V (không đủ chia đôi). Do đó mạch hợp lý: Mắc (Đ1 // R_x) nối tiếp với Đ2. Khi đó U_cụm = 6 V, U2 = 6 V => U_tổng = 6 + 6 = 12 V > 9 V (không được). Phương án 2: Đ2 (chịu 6V, 1A) mắc nối tiếp với (Đ1 // Rx) là không thỏa. Ta xét mạch: Đ1 // (Đ2 nt Rx) hoặc (Đ2 // Rx) nt Đ1: Để U1 = U2 = 6 V, hiệu điện thế dư U - 6 = 3 V. Ta mắc (Đ1 // Đ2) nối tiếp với R_p: U_cụm = 6 V, U_p = 9 - 6 = 3 V. Dòng mạch chính: I_chính = I1 + I2 = 0,5 + 1,0 = 1,5 A. Điện trở phụ: R_p = U_p / I_chính = 3 / 1,5 = 2 Ω.\n• B4 (Kết luận sơ đồ mạch chuẩn - 2.5đ): Mắc (Đ1 song song Đ2) rồi nối tiếp với điện trở phụ R_p = 2 Ω vào nguồn U = 9 V thì cả hai đèn đều sáng bình thường. Đáp số: a) Không sáng bình thường; b) R_p = 2 Ω mắc nối tiếp với cụm (Đ1 // Đ2).",
    "GiaiThich_MeoGhiNho": "Hai đèn cùng U_đm = 6V -> ghép song song hai đèn, phần U thừa (3V) cho R_p mắc nối tiếp gánh.",
    "TrangThai": "Xuất bản"
})

# --- VẬN DỤNG CAO (Tổng hợp - Chuyên sâu) - Cô Hà (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_173", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Cho mạch cầu điện trở gồm 5 điện trở: R1 = 1 Ω trên nhánh AB, R2 = 3 Ω trên nhánh BC, R3 = 2 Ω trên nhánh AD, R4 = 6 Ω trên nhánh DC, và R5 = 5 Ω nối giữa B và D. Đặt vào hai đầu A và C hiệu điện thế U_AC = 12 V. a) Chứng minh mạch cầu cân bằng và tính điện trở tương đương của toàn mạch giữa hai đầu A và C. b) Tính cường độ dòng điện qua mỗi điện trở.",
    "TuKhoaChamDiem": "R1.R4 = R2.R3 = 6; cầu cân bằng V_B = V_D; I5 = 0; R_td = 2.67 Ôm; I_toàn = 4.5 A",
    "DapAnTuLuanMau": "• B1 (Kiểm tra điều kiện cân bằng của mạch cầu - 2.5đ): Tích chéo các điện trở đối diện: R1 · R4 = 1 · 6 = 6; R2 · R3 = 3 · 2 = 6. Vì R1 · R4 = R2 · R3 nên mạch cầu cân bằng. Do đó điện thế tại B bằng điện thế tại D (V_B = V_D).\n• B2 (Xác định dòng điện qua nhánh giữa BD - 2.5đ): Vì V_B = V_D nên hiệu điện thế U_BD = 0 => cường độ dòng điện qua điện trở R5 bằng 0: I5 = 0 A. Có thể tháo bỏ điện trở R5 ra khỏi mạch mà không ảnh hưởng gì.\n• B3 (Tính điện trở tương đương toàn mạch - 2.5đ): Khi bỏ R5, mạch gồm hai nhánh song song: Nhánh trên R12 = R1 + R2 = 1 + 3 = 4 Ω; Nhánh dưới R34 = R3 + R4 = 2 + 6 = 8 Ω. Điện trở tương đương: R_AC = (R12 · R34) / (R12 + R34) = (4 · 8) / (4 + 8) = 32 / 12 = 8/3 Ω ≈ 2,67 Ω.\n• B4 (Tính dòng điện qua từng điện trở - 2.5đ): Nhánh ABC: I1 = I2 = U_AC / R12 = 12 / 4 = 3 A. Nhánh ADC: I3 = I4 = U_AC / R34 = 12 / 8 = 1,5 A. Nhánh giữa: I5 = 0 A. Dòng điện mạch chính: I_toàn = I1 + I3 = 3 + 1,5 = 4,5 A. Đáp số: a) Cầu cân bằng, R_td = 8/3 Ω; b) I1 = I2 = 3A, I3 = I4 = 1,5A, I5 = 0A.",
    "GiaiThich_MeoGhiNho": "Mạch cầu cân bằng: R1.R4 = R2.R3 -> dòng điện qua nhánh nối giữa I5 = 0.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_174", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một nguồn điện có hiệu điện thế không đổi U = 16 V mắc vào mạch gồm điện trở R0 = 4 Ω nối tiếp với một biến trở Rb. a) Xác định giá trị của Rb để công suất tỏa nhiệt trên biến trở đạt giá trị lớn nhất. b) Tính giá trị công suất cực đại đó và công suất toàn mạch lúc đó.",
    "TuKhoaChamDiem": "Pb = U^2 . Rb / (R0 + Rb)^2; cực đại khi Rb = R0 = 4 Ôm; Pb_max = 16 W; P_toàn = 32 W",
    "DapAnTuLuanMau": "• B1 (Thiết lập biểu thức công suất trên biến trở - 2.5đ): Cường độ dòng điện qua mạch: I = U / (R0 + Rb). Công suất tỏa nhiệt trên biến trở: Pb = I^2 · Rb = [U / (R0 + Rb)]^2 · Rb = U^2 · Rb / (R0 + Rb)^2.\n• B2 (Biến đổi mẫu số áp dụng bất đẳng thức Cauchy - 2.5đ): Chia cả tử và mẫu cho Rb: Pb = U^2 / [căn(Rb) + R0 / căn(Rb)]^2. Áp dụng BĐT Cauchy: căn(Rb) + R0 / căn(Rb) >= 2 · căn(R0). Dấu '=' xảy ra khi căn(Rb) = R0 / căn(Rb) => Rb = R0 = 4 Ω.\n• B3 (Tính công suất cực đại trên biến trở - 2.5đ): Pb_max = U^2 / (4 · R0) = 16^2 / (4 · 4) = 256 / 16 = 16 W.\n• B4 (Tính công suất toàn mạch khi đó - 2.5đ): Khi Rb = 4 Ω, R_toàn = 4 + 4 = 8 Ω. Dòng điện I = 16 / 8 = 2 A. Công suất toàn mạch: P_toàn = U · I = 16 · 2 = 32 W (Hiệu suất đạt 50%). Đáp số: a) Rb = 4 Ω; b) Pb_max = 16 W; P_toàn = 32 W.",
    "GiaiThich_MeoGhiNho": "Công suất trên biến trở cực đại khi Rb = R_còn_lại. Pb_max = U^2 / (4.R0).",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_175", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một đoạn dây dẫn đồng chất tiết diện đều được uốn thành một vòng tròn kín có điện trở toàn phần R = 24 Ω. Nối hai cực của nguồn điện vào hai điểm A và B trên vòng tròn. Góc ở tâm chắn cung nhỏ AB là alpha (tính bằng độ). a) Tìm biểu thức điện trở tương đương R_AB theo góc alpha. b) Góc alpha bằng bao nhiêu độ thì điện trở tương đương R_AB đạt giá trị lớn nhất? Tính giá trị lớn nhất đó.",
    "TuKhoaChamDiem": "R1 = R.alpha/360; R2 = R.(360 - alpha)/360; R_AB = R1//R2; cực đại khi alpha = 180 độ; R_AB_max = 6 Ôm",
    "DapAnTuLuanMau": "• B1 (Phân tích mạch điện hai cung tròn song song - 2.5đ): Hai điểm A và B chia vòng tròn thành hai cung tròn mắc song song với nhau. Cung nhỏ chắn góc alpha có điện trở: R1 = R · (alpha / 360°). Cung lớn chắn góc (360° - alpha) có điện trở: R2 = R · (1 - alpha / 360°).\n• B2 (Thiết lập biểu thức điện trở tương đương R_AB - 2.5đ): R_AB = (R1 · R2) / (R1 + R2) = (R1 · R2) / R (vì R1 + R2 = R). Thay R1, R2 vào: R_AB = R · (alpha / 360°) · (1 - alpha / 360°) = 24 · x · (1 - x), với x = alpha / 360°.\n• B3 (Tìm điều kiện cực đại bằng bất đẳng thức Cauchy - 2.5đ): Tích x · (1 - x) đạt giá trị lớn nhất khi x = 1 - x => 2x = 1 => x = 0,5 <=> alpha / 360° = 0,5 => alpha = 180° (A và B là hai đầu đường kính đối xứng).\n• B4 (Tính giá trị điện trở tương đương cực đại - 2.5đ): Khi alpha = 180°: R1 = R2 = 24 / 2 = 12 Ω. R_AB_max = 12 // 12 = 6 Ω. Đáp số: a) R_AB = 24 · (alpha / 360°) · (1 - alpha / 360°); b) alpha = 180° và R_AB_max = 6 Ω.",
    "GiaiThich_MeoGhiNho": "Vòng tròn chia đôi đối xứng (alpha = 180 độ) thì điện trở R_AB đạt cực đại = R / 4.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_176", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L12", "TenBai": "Bài 12. Đoạn mạch nối tiếp, song song", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Có một số điện trở giống nhau, mỗi chiếc có điện trở r = 3 Ω. Cần sử dụng tối thiểu bao nhiêu điện trở loại này và mắc theo sơ đồ như thế nào để được một đoạn mạch có điện trở tương đương R_td = 5 Ω?",
    "TuKhoaChamDiem": "R_td = 5 Ôm; phân tích 5 = 3 + 2; tạo 2 Ôm từ các điện trở 3 Ôm; số điện trở tối thiểu 5 chiếc",
    "DapAnTuLuanMau": "• B1 (Phân tích giá trị điện trở cần tạo - 2.5đ): Điện trở tương đương R_td = 5 Ω > r = 3 Ω, do đó mạch phải có cấu trúc gồm một điện trở r mắc nối tiếp với một cụm điện trở phụ rx: R_td = r + rx => 5 = 3 + rx => rx = 2 Ω.\n• B2 (Tạo điện trở phụ rx = 2 Ω từ các điện trở 3 Ω - 2.5đ): Vì rx = 2 Ω < r = 3 Ω nên rx phải được tạo từ một mạch song song. Xét nghịch đảo: 1 / rx = 1 / 2 = 0,5. Ta có 1 / 2 = 1/3 + 1/6.\n• B3 (Xác định cấu trúc nhánh 6 Ω - 2.5đ): Giá trị 6 Ω được tạo bởi hai điện trở 3 Ω mắc nối tiếp: 3 + 3 = 6 Ω. Như vậy cụm rx gồm: 1 điện trở r = 3 Ω mắc song song với nhánh gồm (2 điện trở r nối tiếp): rx = 3 // (3 + 3) = (3 · 6) / (3 + 6) = 18 / 9 = 2 Ω. Cụm rx dùng 1 + 2 = 3 điện trở.\n• B4 (Tổng hợp sơ đồ mạch và số lượng - 2.5đ): Sơ đồ mạch hoàn chỉnh: r nối tiếp với [r // (r nt r)]. Tổng số điện trở sử dụng là: 1 + 3 = 4 điện trở. Kiểm tra lại: R_td = 3 + (3 · 6) / (3 + 6) = 3 + 2 = 5 Ω (hoàn toàn chính xác). Đáp số: Số lượng tối thiểu là 4 điện trở r; Sơ đồ: r nt [r // (r nt r)].",
    "GiaiThich_MeoGhiNho": "Tối thiểu 4 điện trở: 3 + [3 // (3 + 3)] = 3 + 2 = 5 Ôm.",
    "TrangThai": "Xuất bản"
})

# ==============================================================================
# BÀI 13: NĂNG LƯỢNG DÒNG ĐIỆN VÀ CÔNG SUẤT (C3_L13) - 16 CÂU (4 NB, 4 TH, 4 VD, 4 VDC)
# ==============================================================================

# --- NHẬN BIẾT (Dễ - Nền tảng) - Thầy Minh (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_177", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Năng lượng của dòng điện (điện năng) là gì? Viết công thức tính điện năng tiêu thụ của một đoạn mạch theo hiệu điện thế, cường độ dòng điện và thời gian.",
    "TuKhoaChamDiem": "năng lượng của dòng điện; sinh công và cung cấp nhiệt lượng; A = U . I . t; P . t; Jun; J",
    "DapAnTuLuanMau": "• B1 (Khái niệm điện năng - 2.5đ): Năng lượng của dòng điện được gọi là điện năng. Dòng điện có mang năng lượng vì nó có khả năng thực hiện công cơ học và cung cấp nhiệt lượng để biến đổi thành các dạng năng lượng khác.\n• B2 (Công thức tính điện năng tiêu thụ - 2.5đ): A = U · I · t = P · t.\n• B3 (Giải thích các đại lượng - 2.5đ): U là hiệu điện thế giữa hai đầu đoạn mạch (V); I là cường độ dòng điện (A); t là thời gian dòng điện chạy qua (s); P là công suất điện (W).\n• B4 (Đơn vị đo chuẩn SI - 2.5đ): Đơn vị đo điện năng trong hệ SI là Jun (J), với 1 J = 1 W·s = 1 V·A·s.",
    "GiaiThich_MeoGhiNho": "A = U . I . t = P . t (Jun).",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_178", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Viết các công thức tính công suất điện P của một đoạn mạch có điện trở R theo U và I. Nêu tên và đơn vị đo của công suất.",
    "TuKhoaChamDiem": "P = U . I; P = I^2 . R; P = U^2 / R; Oát; W",
    "DapAnTuLuanMau": "• B1 (Công thức tổng quát - 2.5đ): P = U · I.\n• B2 (Các công thức suy biến theo điện trở thuần - 2.5đ): Thay U = I · R: P = I^2 · R. Thay I = U / R: P = U^2 / R.\n• B3 (Tên gọi và ký hiệu - 2.5đ): P là công suất điện của đoạn mạch.\n• B4 (Đơn vị đo chuẩn - 2.5đ): Đơn vị đo công suất trong hệ SI là Oát (Watt, ký hiệu là W). Các đơn vị bội số: 1 kW = 1000 W; 1 MW = 10^6 W.",
    "GiaiThich_MeoGhiNho": "Bộ ba công thức công suất: P = U.I = I^2.R = U^2 / R.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_179", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Phát biểu định luật Joule – Lenz về nhiệt lượng tỏa ra trên dây dẫn khi có dòng điện chạy qua. Viết hệ thức của định luật.",
    "TuKhoaChamDiem": "nhiệt lượng tỏa ra; tỉ lệ thuận với bình phương cường độ dòng điện; tỉ lệ với điện trở và thời gian; Q = I^2 . R . t",
    "DapAnTuLuanMau": "• B1 (Phát biểu định luật Joule – Lenz - 2.5đ): Nhiệt lượng tỏa ra trên một dây dẫn khi có dòng điện chạy qua tỉ lệ thuận với bình phương cường độ dòng điện, với điện trở của dây dẫn và thời gian dòng điện chạy qua.\n• B2 (Hệ thức định luật - 2.5đ): Q = I^2 · R · t.\n• B3 (Giải thích đại lượng và đơn vị - 2.5đ): I đo bằng Ampe (A); R đo bằng Ôm (Ω); t đo bằng giây (s); Q là nhiệt lượng đo bằng Jun (J).\n• B4 (Chuyển đổi sang đơn vị Calo - 2.5đ): Nếu tính theo đơn vị calo: Q = 0,24 · I^2 · R · t (với 1 cal ≈ 4,18 J).",
    "GiaiThich_MeoGhiNho": "Định luật Joule - Lenz: Q = I^2 . R . t (Jun).",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_180", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Minh", "TenGiaoVien": "Thầy Minh",
    "MucDo": "Dễ (Nền tảng)",
    "NoiDung": "Công tơ điện dùng để đo đại lượng nào? Cho biết 1 số điện đếm được trên công tơ điện tương ứng với bao nhiêu kilôoát giờ (kWh) và bằng bao nhiêu Jun (J)?",
    "TuKhoaChamDiem": "đo điện năng tiêu thụ; 1 số điện = 1 kWh; 1 kWh = 3600000 J = 3.6 MJ",
    "DapAnTuLuanMau": "• B1 (Công dụng của công tơ điện - 2.5đ): Công tơ điện là thiết bị chuyên dùng để đo lượng điện năng tiêu thụ của các hộ gia đình hoặc cơ sở sản xuất trong một khoảng thời gian.\n• B2 (Quy đổi số điện - 2.5đ): Mỗi 1 số đếm hiển thị trên mặt công tơ điện tương ứng với 1 kilôoát giờ điện năng: 1 số = 1 kWh.\n• B3 (Quy đổi sang đơn vị Jun - 2.5đ): 1 kWh = 1000 W · 3600 s = 3.600.000 J = 3,6 · 10^6 J (3,6 MJ).\n• B4 (Ý nghĩa quản lý - 2.5đ): Dựa vào số đếm công tơ điện hàng tháng, công ty điện lực tính toán chính xác hóa đơn tiền điện tiêu dùng.",
    "GiaiThich_MeoGhiNho": "1 số điện = 1 kWh = 3.600.000 J.",
    "TrangThai": "Xuất bản"
})

# --- THÔNG HIỂU (Trung bình - Ứng dụng thực tế) - Cô Lan (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_181", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Giải thích vì sao dây tóc của bóng đèn sợi đốt nóng sáng rực rỡ đến hơn 2000 độ C, trong khi hai đoạn dây đồng nối đèn với ổ điện hầu như không bị nóng lên?",
    "TuKhoaChamDiem": "mắc nối tiếp cùng cường độ dòng điện I; điện trở dây tóc R_tóc rất lớn; điện trở dây đồng R_đồng rất nhỏ; Q = I^2.R.t",
    "DapAnTuLuanMau": "• B1 (Cùng cường độ dòng điện trong mạch nối tiếp - 2.5đ): Dây nối bằng đồng và dây tóc bóng đèn được mắc nối tiếp với nhau nên có cùng cường độ dòng điện I chạy qua.\n• B2 (So sánh điện trở hai phần - 2.5đ): Dây tóc bóng đèn làm bằng vonfram rất mảnh và cuộn dài nên có điện trở R_tóc rất lớn (hàng trăm Ôm). Trong khi đó dây dẫn bằng đồng có tiết diện to và điện trở suất nhỏ nên điện trở R_đồng rất nhỏ (chưa tới 0,1 Ôm).\n• B3 (Áp dụng định luật Joule – Lenz - 2.5đ): Nhiệt lượng tỏa ra tính theo Q = I^2 · R · t. Với cùng I và t, nhiệt lượng tỉ lệ thuận với điện trở R.\n• B4 (Kết luận nhiệt độ đạt được - 2.5đ): Nhiệt lượng tỏa ra chủ yếu tập trung tại dây tóc làm nó phát nhiệt cực lớn tới 2000 - 2500 °C phát sáng trắng; còn dây đồng chỉ tỏa một lượng nhiệt rất nhỏ truyền ngay ra không khí nên hầu như không nóng.",
    "GiaiThich_MeoGhiNho": "Cùng I: R_tóc >> R_dây đồng -> Dây tóc nóng sáng, dây dẫn vẫn mát.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_182", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Tại sao các chuyên gia năng lượng luôn khuyến khích người tiêu dùng thay thế bóng đèn sợi đốt cũ bằng bóng đèn LED tiết kiệm điện?",
    "TuKhoaChamDiem": "hiệu suất phát quang cao; đèn sợi đốt 90-95% biến thành nhiệt hao phí; đèn LED tỏa nhiệt ít; tiết kiệm 80% điện năng",
    "DapAnTuLuanMau": "• B1 (Phân tích hiệu suất đèn sợi đốt - 2.5đ): Đèn sợi đốt hoạt động bằng cách nung nóng dây tóc, chỉ có khoảng 5 - 10% điện năng chuyển thành quang năng (ánh sáng), còn lại 90 - 95% biến thành nhiệt năng tỏa ra môi trường vô ích.\n• B2 (Phân tích hiệu suất đèn LED - 2.5đ): Đèn LED phát sáng bằng hiệu ứng bán dẫn quang điện, hiệu suất chuyển đổi điện năng thành ánh sáng đạt trên 50 - 60%, nhiệt lượng sinh ra rất ít.\n• B3 (So sánh công suất tiêu thụ - 2.5đ): Để tạo cùng độ sáng, đèn LED chỉ cần công suất 8 - 10 W, trong khi đèn sợi đốt cần tới 60 W (tiết kiệm hơn 80% điện năng).\n• B4 (Độ bền và môi trường - 2.5đ): Đèn LED có tuổi thọ gấp 15 - 25 lần đèn sợi đốt, không chứa thủy ngân độc hại, giúp giảm chi phí thay thế và giảm phát thải CO2.",
    "GiaiThich_MeoGhiNho": "Đèn LED: biến điện trực tiếp thành ánh sáng, tỏa nhiệt ít -> tiết kiệm 80% điện năng so với sợi đốt.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_183", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Nêu ít nhất 4 biện pháp thiết thực giúp các gia đình tiết kiệm điện năng hiệu quả trong mùa nắng nóng cao điểm.",
    "TuKhoaChamDiem": "cài đặt điều hòa 26-28 độ; tắt thiết bị khi không dùng; dùng thiết bị inverter nhãn năng lượng 5 sao; tận dụng ánh sáng thông gió tự nhiên",
    "DapAnTuLuanMau": "• B1 (Sử dụng điều hòa khoa học - 2.5đ): Cài đặt nhiệt độ điều hòa ở mức hợp lý (từ 26 °C đến 28 °C), kết hợp dùng quạt gió lưu thông không khí; vệ sinh lưới lọc bụi định kỳ.\n• B2 (Tắt các thiết bị khi không dùng - 2.5đ): Tắt hẳn nguồn các thiết bị điện và rút phích cắm khi ra khỏi phòng, tránh để chế độ chờ (standby) gây hao phí điện ngầm.\n• B3 (Lựa chọn thiết bị hiệu suất cao - 2.5đ): Ưu tiên mua các thiết bị công nghệ biến tần Inverter (tủ lạnh, máy giặt, điều hòa) có dán nhãn năng lượng 5 sao tiết kiệm điện.\n• B4 (Tận dụng điều kiện tự nhiên - 2.5đ): Mở cửa sổ đón ánh sáng mặt trời ban ngày và gió mát tự nhiên; dùng rèm cách nhiệt che bớt nắng gắt buổi trưa.",
    "GiaiThich_MeoGhiNho": "Tiết kiệm điện: Điều hòa >=26°C, rút phích cắm khi tắt, dùng máy Inverter 5 sao, đón ánh sáng tự nhiên.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_184", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Lan", "TenGiaoVien": "Cô Lan",
    "MucDo": "Trung bình (Ứng dụng thực tế)",
    "NoiDung": "Hiện tượng đoản mạch (chập mạch) là gì? Tại sao hiện tượng này lại rất nguy hiểm và thường là nguyên nhân hàng đầu gây ra các vụ cháy nhà?",
    "TuKhoaChamDiem": "điện trở mạch ngoài giảm xấp xỉ bằng 0; dòng điện tăng vọt cực đại; Q = I^2.R.t sinh nhiệt khổng lồ; bốc cháy lớp vỏ nhựa",
    "DapAnTuLuanMau": "• B1 (Khái niệm hiện tượng đoản mạch - 2.5đ): Đoản mạch (ngắn mạch) là hiện tượng dây pha (nóng) tiếp xúc trực tiếp với dây trung tính (nguội) mà không đi qua thiết bị tiêu thụ điện, làm điện trở mạch ngoài giảm xấp xỉ bằng 0 (R ≈ 0).\n• B2 (Dòng điện tăng vọt theo định luật Ohm - 2.5đ): Theo I = U / R, khi R tiến về 0 thì cường độ dòng điện tăng vọt lên hàng trăm Ampe trong tích tắc.\n• B3 (Nhiệt lượng sinh ra đột ngột - 2.5đ): Định luật Joule – Lenz Q = I^2 · R · t cho thấy dòng điện tăng hàng chục lần làm nhiệt lượng sinh ra tức thời tăng hàng trăm, hàng nghìn lần.\n• B4 (Hậu quả cháy nổ - 2.5đ): Sợi dây dẫn nóng đỏ bốc cháy trong vài giây, bắn tia lửa điện vào các vật liệu dễ cháy xung quanh (rèm cửa, gỗ, xốp) gây hỏa hoạn thiêu rụi nhà cửa.",
    "GiaiThich_MeoGhiNho": "Đoản mạch: R = 0 -> I tăng cực đại -> nhiệt lượng Q tăng vọt bốc cháy tức thì.",
    "TrangThai": "Xuất bản"
})

# --- VẬN DỤNG (Khó - Tính toán nâng cao) - Thầy Duy (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_185", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Một bóng đèn có ghi 220 V – 100 W được mắc vào mạng điện sinh hoạt có hiệu điện thế đúng 220 V. a) Tính điện trở của bóng đèn và cường độ dòng điện qua đèn khi sáng bình thường. b) Tính điện năng bóng đèn tiêu thụ trong 1 tháng (30 ngày), biết mỗi ngày thắp sáng trung bình 6 giờ.",
    "TuKhoaChamDiem": "R = 484 Ôm; I = 0.455 A; t = 180 h; A = 18 kWh (18 số điện)",
    "DapAnTuLuanMau": "• B1 (Tính điện trở của bóng đèn - 2.5đ): R = U_đm^2 / P_đm = 220^2 / 100 = 48400 / 100 = 484 Ω.\n• B2 (Tính cường độ dòng điện định mức - 2.5đ): I = P_đm / U_đm = 100 / 220 ≈ 0,455 A (hay 5/11 A).\n• B3 (Tính tổng thời gian thắp sáng trong 30 ngày - 2.5đ): Tổng thời gian: t = 6 h/ngày · 30 ngày = 180 giờ.\n• B4 (Tính điện năng tiêu thụ ra kWh - 2.5đ): Đổi P = 100 W = 0,1 kW. Điện năng tiêu thụ: A = P · t = 0,1 kW · 180 h = 18 kWh (tương đương 18 số điện hay 64,8 MJ). Đáp số: a) R = 484 Ω, I ≈ 0,455 A; b) A = 18 kWh.",
    "GiaiThich_MeoGhiNho": "R = U^2 / P; I = P / U; A = P (kW) . t (giờ).",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_186", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Một ấm điện có công suất 1000 W hoạt động ở hiệu điện thế 220 V dùng để đun sôi 1,5 lít nước từ nhiệt độ ban đầu 25 độ C. Biết nhiệt dung riêng của nước là c = 4200 J/(kg.K) và hiệu suất của ấm là 84%. Tính thời gian cần thiết để đun sôi ấm nước trên.",
    "TuKhoaChamDiem": "m = 1.5 kg; Q_ci = 472500 J; H = 84%; A_tp = 562500 J; P = 1000 W; t = 562.5 s (9 phút 22.5 giây)",
    "DapAnTuLuanMau": "• B1 (Tính nhiệt lượng có ích để đun sôi nước - 2.5đ): Khối lượng nước: m = 1,5 kg. Độ tăng nhiệt độ: Delta_t = 100 - 25 = 75 °C. Nhiệt lượng có ích: Q_ci = m · c · Delta_t = 1,5 · 4200 · 75 = 472500 J.\n• B2 (Tính điện năng toàn phần tiêu thụ - 2.5đ): Hiệu suất H = 84% => A_tp = Q_ci / H = 472500 / 0,84 = 562500 J.\n• B3 (Tính thời gian đun nước - 2.5đ): Áp dụng công thức A_tp = P · t => t = A_tp / P = 562500 / 1000 = 562,5 giây.\n• B4 (Đổi ra phút giây và kết luận - 2.5đ): 562,5 giây = 9 phút 22,5 giây (hay 9,375 phút). Đáp số: Thời gian đun sôi nước là 562,5 giây (khoảng 9 phút 23 giây).",
    "GiaiThich_MeoGhiNho": "Q_ci = m.c.Δt; A_tp = Q_ci / H; t = A_tp / P.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_187", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Một bếp điện có hai dây điện trở R1 và R2. Khi chỉ dùng dây R1 thì đun sôi một ấm nước trong thời gian t1 = 12 phút. Khi chỉ dùng dây R2 thì đun sôi cùng ấm nước đó trong thời gian t2 = 24 phút. Coi hiệu điện thế và hao phí không đổi. Hãy tính thời gian đun sôi ấm nước nếu: a) Mắc hai dây điện trở nối tiếp; b) Mắc hai dây điện trở song song.",
    "TuKhoaChamDiem": "t1 = 12 phút; t2 = 24 phút; nối tiếp t_nt = t1 + t2 = 36 phút; song song t_ss = (t1.t2)/(t1+t2) = 8 phút",
    "DapAnTuLuanMau": "• B1 (Mối liên hệ giữa điện trở và thời gian đun - 2.5đ): Nhiệt lượng cần đun sôi nước không đổi: Q = (U^2 / R) · t => R = (U^2 / Q) · t. Vì U và Q không đổi nên điện trở R tỉ lệ thuận với thời gian đun t.\n• B2 (Trường hợp mắc nối tiếp - 2.5đ): Mắc nối tiếp: R_nt = R1 + R2 => t_nt = t1 + t2 = 12 + 24 = 36 phút.\n• B3 (Trường hợp mắc song song - 2.5đ): Mắc song song: 1 / R_ss = 1 / R1 + 1 / R2 => 1 / t_ss = 1 / t1 + 1 / t2 = 1/12 + 1/24 = 3/24 = 1/8.\n• B4 (Tính thời gian song song - 2.5đ): t_ss = 8 phút. Đáp số: a) Nối tiếp: 36 phút; b) Song song: 8 phút.",
    "GiaiThich_MeoGhiNho": "Đun nước cùng U: Nối tiếp cộng thời gian (t_nt = t1 + t2); Song song tính tích trên tổng (t_ss = t1.t2 / (t1 + t2)).",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_188", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Duy", "TenGiaoVien": "Thầy Duy",
    "MucDo": "Khó (Tính toán nâng cao)",
    "NoiDung": "Mắc một bóng đèn Đ (12 V – 6 W) nối tiếp với một biến trở Rb vào nguồn điện U = 18 V. a) Tìm giá trị của biến trở Rb để đèn sáng bình thường. b) Tính công suất tiêu thụ của toàn mạch và hiệu suất thắp sáng của mạch (tỉ số giữa công suất đèn và công suất toàn mạch) khi đèn sáng bình thường.",
    "TuKhoaChamDiem": "U_đm = 12 V; P_đm = 6 W; I = 0.5 A; U_b = 6 V; Rb = 12 Ôm; P_toàn = 9 W; H = 66.7%",
    "DapAnTuLuanMau": "• B1 (Tính thông số định mức của đèn - 2.5đ): Cường độ dòng điện định mức của đèn: I_đm = P_đm / U_đm = 6 / 12 = 0,5 A. Điện trở của đèn: R_đèn = U_đm / I_đm = 12 / 0,5 = 24 Ω.\n• B2 (Tính giá trị biến trở để đèn sáng bình thường - 2.5đ): Đèn sáng bình thường khi I = 0,5 A và U_đèn = 12 V. Hiệu điện thế hai đầu biến trở: U_b = U - U_đèn = 18 - 12 = 6 V. Điện trở biến trở: Rb = U_b / I = 6 / 0,5 = 12 Ω.\n• B3 (Tính công suất toàn mạch - 2.5đ): P_toàn = U · I = 18 · 0,5 = 9 W (hoặc P_toàn = P_đèn + P_b = 6 + 0,5^2 · 12 = 6 + 3 = 9 W).\n• B4 (Tính hiệu suất của mạch - 2.5đ): Hiệu suất: H = (P_đèn / P_toàn) · 100% = (6 / 9) · 100% ≈ 66,67%. Đáp số: a) Rb = 12 Ω; b) P_toàn = 9 W, H ≈ 66,7%.",
    "GiaiThich_MeoGhiNho": "Rb = (U - U_đèn) / I_đm; Hiệu suất H = U_đèn / U_nguồn = 12 / 18 = 66,7%.",
    "TrangThai": "Xuất bản"
})

# --- VẬN DỤNG CAO (Tổng hợp - Chuyên sâu) - Cô Hà (4 câu) ---
essay_batch3.append({
    "CauHoiID": "TL_BS_189", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một gia đình sử dụng các thiết bị điện trong 1 tháng (30 ngày) gồm: 4 bóng đèn LED loại 15 W thắp sáng 5 giờ/ngày; 2 chiếc quạt bàn loại 60 W bật 8 giờ/ngày; 1 chiếc tủ lạnh công suất trung bình 120 W chạy liên tục 24 giờ/ngày; 1 nồi cơm điện 700 W nấu 1,5 giờ/ngày. a) Tính tổng lượng điện năng tiêu thụ của gia đình trong 30 ngày (ra kWh). b) Tính số tiền điện phải trả trong tháng biết giá điện tính theo bậc thang: 50 kWh đầu giá 1700 đ/kWh; từ 51 đến 100 kWh giá 1800 đ/kWh; trên 100 kWh giá 2100 đ/kWh (chưa bao gồm thuế VAT 10%).",
    "TuKhoaChamDiem": "A_ngày = 4.17 kWh; A_tháng = 125.1 kWh; tiền bậc 1 = 85000; tiền bậc 2 = 90000; tiền bậc 3 = 52710; tổng tiền = 227710 đồng",
    "DapAnTuLuanMau": "• B1 (Tính điện năng tiêu thụ mỗi ngày của từng thiết bị - 2.5đ): Đèn LED: A1 = 4 · 0,015 kW · 5 h = 0,30 kWh. Quạt bàn: A2 = 2 · 0,06 kW · 8 h = 0,96 kWh. Tủ lạnh: A3 = 0,12 kW · 24 h = 2,88 kWh. Nồi cơm điện: A4 = 0,70 kW · 1,5 h = 1,05 kWh. Tổng điện năng một ngày: A_ngày = 0,30 + 0,96 + 2,88 + 1,05 = 5,19 kWh.\n• B2 (Tính tổng điện năng trong 30 ngày - 2.5đ): A_tháng = 5,19 · 30 = 155,7 kWh (làm tròn 156 kWh).\n• B3 (Tính tiền điện theo 3 bậc thang - 2.5đ): Bậc 1 (50 kWh đầu): 50 · 1700 = 85.000 đ. Bậc 2 (50 kWh tiếp theo): 50 · 1800 = 90.000 đ. Bậc 3 (phần còn lại 55,7 kWh): 55,7 · 2100 = 116.970 đ. Tổng tiền chưa thuế: T = 85.000 + 90.000 + 116.970 = 291.970 đ.\n• B4 (Tính tổng tiền gồm thuế VAT 10% - 2.5đ): Thuế VAT: 291.970 · 10% = 29.197 đ. Tổng thanh toán: 291.970 + 29.197 = 321.167 đ. Đáp số: a) A_tháng = 155,7 kWh; b) Tiền điện chưa thuế: 291.970 đ (có thuế: 321.167 đ).",
    "GiaiThich_MeoGhiNho": "A = sum(P . t). Tính tiền điện nhớ chia đúng từng khoảng bậc thang lũy tiến.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_190", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một nhà máy thủy điện cần truyền tải công suất điện P = 2 MW đi xa 20 km bằng đường dây tải điện đôi có điện trở suất rho = 2,8.10^-8 Ω.m và tiết diện dây S = 56 mm^2. a) Tính điện trở của toàn bộ đường dây tải điện đôi. b) So sánh công suất hao phí do tỏa nhiệt trên đường dây trong hai trường hợp: truyền tải ở hiệu điện thế U1 = 10 kV và truyền tải ở hiệu điện thế U2 = 50 kV.",
    "TuKhoaChamDiem": "l_tổng = 40 km; R_dây = 20 Ôm; U1 = 10 kV hao phí Delta_P1 = 800 kW; U2 = 50 kV hao phí Delta_P2 = 32 kW; giảm 25 lần",
    "DapAnTuLuanMau": "• B1 (Tính điện trở toàn bộ đường dây tải điện đôi - 2.5đ): Tổng chiều dài dây: l_tổng = 2 · 20 km = 40 km = 40000 m. Tiết diện S = 56 · 10^-6 m^2. Điện trở đường dây: R = rho · (l_tổng / S) = (2,8 · 10^-8 · 40000) / (56 · 10^-6) = 1120 / 56 = 20 Ω.\n• B2 (Tính hao phí khi truyền tải ở U1 = 10 kV - 2.5đ): P = 2 MW = 2.10^6 W; U1 = 10 kV = 10^4 V. Dòng điện I1 = P / U1 = 2.10^6 / 10^4 = 200 A. Công suất hao phí: Delta_P1 = I1^2 · R = 200^2 · 20 = 40000 · 20 = 800000 W = 800 kW (chiếm tới 40% công suất nhà máy).\n• B3 (Tính hao phí khi truyền tải ở U2 = 50 kV - 2.5đ): U2 = 50 kV = 5.10^4 V. Dòng điện I2 = P / U2 = 2.10^6 / (5.10^4) = 40 A. Công suất hao phí: Delta_P2 = I2^2 · R = 40^2 · 20 = 1600 · 20 = 32000 W = 32 kW (chỉ chiếm 1,6%).\n• B4 (So sánh và kết luận biện pháp kỹ thuật - 2.5đ): Tỉ số hao phí: Delta_P1 / Delta_P2 = 800 / 32 = 25 lần. Vì hiệu điện thế tăng 5 lần (từ 10 lên 50 kV) nên công suất hao phí giảm 5^2 = 25 lần. Kết luận: Biện pháp hiệu quả nhất để giảm hao phí truyền tải là tăng hiệu điện thế bằng máy biến áp.",
    "GiaiThich_MeoGhiNho": "Hao phí ΔP tỉ lệ nghịch với U^2. Tăng U lên n lần thì hao phí giảm n^2 lần.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_191", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Một động cơ điện một chiều kéo một thùng hàng nặng khối lượng m = 200 kg chuyển động thẳng đều lên cao theo phương thẳng đứng với vận tốc v = 1,5 m/s (lấy g = 10 m/s^2). Động cơ hoạt động ở hiệu điện thế U = 220 V và có hiệu suất cơ - điện là H = 75%. a) Tính công suất cơ học có ích để nâng hàng và công suất điện toàn phần động cơ tiêu thụ. b) Tính cường độ dòng điện chạy qua động cơ và điện trở trong r của cuộn dây động cơ (coi hao phí hoàn toàn do tỏa nhiệt trên r).",
    "TuKhoaChamDiem": "P_ci = 3000 W; P_tp = 4000 W; I = 18.18 A; P_hp = 1000 W; r = 3.025 Ôm",
    "DapAnTuLuanMau": "• B1 (Tính công suất cơ học nâng hàng có ích - 2.5đ): Trọng lượng hàng: P_trọng = m · g = 200 · 10 = 2000 N. Công suất có ích nâng hàng đều: P_ci = F · v = 2000 · 1,5 = 3000 W = 3 kW.\n• B2 (Tính công suất điện toàn phần tiêu thụ - 2.5đ): Vì hiệu suất H = 75% = 0,75: P_tp = P_ci / H = 3000 / 0,75 = 4000 W = 4 kW.\n• B3 (Tính cường độ dòng điện chạy qua động cơ - 2.5đ): Ta có P_tp = U · I => I = P_tp / U = 4000 / 220 ≈ 18,18 A.\n• B4 (Tính điện trở trong r của động cơ - 2.5đ): Công suất hao phí tỏa nhiệt trên cuộn dây: P_hp = P_tp - P_ci = 4000 - 3000 = 1000 W. Vì hao phí do tỏa nhiệt trên r: P_hp = I^2 · r => r = P_hp / I^2 = 1000 / (18,18)^2 = 1000 / 330,5 ≈ 3,025 Ω. Đáp số: a) P_ci = 3 kW, P_tp = 4 kW; b) I ≈ 18,18 A, r ≈ 3,03 Ω.",
    "GiaiThich_MeoGhiNho": "P_ci = m.g.v; P_tp = P_ci / H; I = P_tp / U; r = (P_tp - P_ci) / I^2.",
    "TrangThai": "Xuất bản"
})

essay_batch3.append({
    "CauHoiID": "TL_BS_192", "ChuongId": "chuong3", "TenChuong": "Chương III. Điện",
    "BaiID": "C3_L13", "TenBai": "Bài 13. Năng lượng của dòng điện và công suất điện", "GiaoVienID": "Ha", "TenGiaoVien": "Cô Hà",
    "MucDo": "Tổng hợp (Chuyên sâu)",
    "NoiDung": "Cho mạch điện gồm nguồn điện có hiệu điện thế không đổi U = 12 V, điện trở R1 = 3 Ω mắc nối tiếp với một biến trở Rb. Khi điều chỉnh Rb, người ta thấy có hai giá trị khác nhau của biến trở là Rb1 = 1 Ω và Rb2 = 9 Ω mà công suất tiêu thụ trên biến trở đều bằng nhau và bằng P0. a) Chứng minh rằng tích Rb1 · Rb2 = R1^2 và tính giá trị công suất P0. b) Tìm giá trị của Rb để công suất trên toàn mạch đạt giá trị lớn nhất.",
    "TuKhoaChamDiem": "Pb1 = Pb2 = P0; tích Rb1.Rb2 = R1^2; P0 = 9 W; P_toàn max khi Rb = 0; P_toàn_max = 48 W",
    "DapAnTuLuanMau": "• B1 (Thiết lập phương trình công suất trên biến trở - 2.5đ): Pb = I^2 · Rb = [U / (R1 + Rb)]^2 · Rb = P0 => U^2 · Rb = P0 · (R1^2 + 2 · R1 · Rb + Rb^2) => P0 · Rb^2 + (2 · R1 · P0 - U^2) · Rb + P0 · R1^2 = 0.\n• B2 (Áp dụng định lý Viète chứng minh tích hai giá trị - 2.5đ): Phương trình bậc hai theo Rb có hai nghiệm Rb1 và Rb2. Theo định lý Viète: tích hai nghiệm Rb1 · Rb2 = (P0 · R1^2) / P0 = R1^2. Kiểm tra với số liệu: Rb1 · Rb2 = 1 · 9 = 9 = 3^2 = R1^2 (Đúng như lý thuyết).\n• B3 (Tính giá trị công suất P0 - 2.5đ): Thay Rb = 1 Ω vào: I1 = 12 / (3 + 1) = 3 A => P0 = I1^2 · Rb1 = 3^2 · 1 = 9 W. (Kiểm tra lại với Rb2 = 9 Ω: I2 = 12 / (3 + 9) = 1 A => P0 = 1^2 · 9 = 9 W, hoàn toàn trùng khớp).\n• B4 (Công suất toàn mạch cực đại - 2.5đ): Công suất toàn mạch: P_toàn = U^2 / (R1 + Rb). Vì U và R1 cố định nên P_toàn lớn nhất khi mẫu số (R1 + Rb) nhỏ nhất, tức là khi Rb = 0 Ω. Khi đó: P_toàn_max = U^2 / R1 = 12^2 / 3 = 144 / 3 = 48 W. Đáp số: a) Đã chứng minh, P0 = 9 W; b) Rb = 0 Ω thì P_toàn_max = 48 W.",
    "GiaiThich_MeoGhiNho": "Khi Pb1 = Pb2 thì Rb1 . Rb2 = R1^2. P_toàn lớn nhất khi Rb = 0.",
    "TrangThai": "Xuất bản"
})

print(f"Prepared {len(essay_batch3)} Essay questions for Batch 3.")
