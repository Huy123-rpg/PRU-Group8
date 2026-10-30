# -*- coding: utf-8 -*-
import sys
sys.stdout.reconfigure(encoding='utf-8')

mcq_batch4 = []

# ==============================================================================
# BÀI 14: CẢM ỨNG ĐIỆN TỪ. NGUYÊN TẮC TẠO RA DÒNG ĐIỆN XOAY CHIỀU (bai14) - 10 CÂU (5 VD, 5 VDC)
# ==============================================================================
# VD: 5 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_121", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Dòng điện cảm ứng xuất hiện trong cuộn dây dẫn kín trong trường hợp nào dưới đây?",
    "DapAnA": "Cho nam châm chuyển động lại gần hoặc ra xa đầu cuộn dây",
    "DapAnB": "Đặt nam châm đứng yên bất động bên trong lòng cuộn dây",
    "DapAnC": "Đặt cuộn dây đứng yên trong một từ trường đều không đổi",
    "DapAnD": "Nối hai đầu cuộn dây với hai cực của một pin điện hóa",
    "DapAnDung": "A",
    "GiaiThich": "Dòng điện cảm ứng chỉ xuất hiện khi số đường sức từ xuyên qua tiết diện cuộn dây dẫn kín biến thiên (tăng lên khi lại gần hoặc giảm đi khi ra xa).",
    "MeoGhiNho": "Có sự chuyển động tương đối làm số đường sức từ biến thiên -> Có dòng điện cảm ứng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_122", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Khi quay đều một khung dây dẫn kín trong từ trường đều của nam châm sao cho các đường sức từ vuông góc với trục quay của khung thì trong khung dây xuất hiện:",
    "DapAnA": "Dòng điện xoay chiều", "DapAnB": "Dòng điện một chiều không đổi",
    "DapAnC": "Dòng điện lúc có lúc không nhưng cùng chiều", "DapAnD": "Không có dòng điện nào xuất hiện",
    "DapAnDung": "A",
    "GiaiThich": "Khi khung dây quay đều, số đường sức từ xuyên qua tiết diện khung dây luân phiên tăng rồi giảm, tạo ra dòng điện cảm ứng luân phiên đổi chiều gọi là dòng điện xoay chiều.",
    "MeoGhiNho": "Quay khung dây trong từ trường -> Dòng điện xoay chiều (AC).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_123", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một máy phát điện xoay chiều một pha có rôto gồm 4 cặp cực quay với tốc độ n = 750 vòng/phút. Tần số f của dòng điện xoay chiều do máy phát ra là:",
    "DapAnA": "50 Hz", "DapAnB": "60 Hz", "DapAnC": "100 Hz", "DapAnD": "25 Hz",
    "DapAnDung": "A",
    "GiaiThich": "Tần số dòng điện: f = (p . n) / 60 = (4 . 750) / 60 = 3000 / 60 = 50 Hz.",
    "MeoGhiNho": "f = p . n / 60 (khi n tính bằng vòng/phút, p là số cặp cực).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_124", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Hai bộ phận chính của một máy phát điện xoay chiều kỹ thuật là:",
    "DapAnA": "Nam châm tạo ra từ trường và cuộn dây dẫn tạo ra dòng điện cảm ứng",
    "DapAnB": "Bộ góp điện và chổi quét",
    "DapAnC": "Bình ắc quy và công tắc đảo chiều",
    "DapAnD": "Lõi sắt non và vỏ nhôm bảo vệ",
    "DapAnDung": "A",
    "GiaiThich": "Máy phát điện xoay chiều gồm phần cảm là nam châm tạo từ trường và phần ứng là cuộn dây dẫn tạo dòng điện cảm ứng (một phần đứng yên gọi là stato, một phần quay gọi là rôto).",
    "MeoGhiNho": "Máy phát điện AC gồm 2 bộ phận chính: Nam châm (phần cảm) và Cuộn dây (phần ứng).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_125", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Dòng điện xoay chiều trong lưới điện quốc gia Việt Nam có tần số chuẩn là 50 Hz. Điều này có nghĩa là trong mỗi giây dòng điện này đổi chiều:",
    "DapAnA": "100 lần", "DapAnB": "50 lần", "DapAnC": "25 lần", "DapAnD": "200 lần",
    "DapAnDung": "A",
    "GiaiThich": "Mỗi chu kỳ dao động toàn phần (1 Hz) dòng điện xoay chiều đổi chiều đúng 2 lần. Do đó với tần số f = 50 Hz, trong mỗi 1 giây dòng điện đổi chiều: 50 . 2 = 100 lần.",
    "MeoGhiNho": "1 chu kỳ đổi chiều 2 lần -> 50 Hz đổi chiều 100 lần/giây.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 5 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_126", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một khung dây phẳng có diện tích S = 50 cm^2 gồm 100 vòng dây quay đều quanh một trục nằm trong mặt phẳng khung với tốc độ góc omega = 100 rad/s trong từ trường đều có cảm ứng từ B = 0,2 T vuông góc với trục quay. Suất điện động cảm ứng cực đại xuất hiện trong khung dây là:",
    "DapAnA": "10 V", "DapAnB": "1 V", "DapAnC": "100 V", "DapAnD": "0,1 V",
    "DapAnDung": "A",
    "GiaiThich": "Đổi S = 50.10^-4 m^2 = 5.10^-3 m^2. Từ thông cực đại: Phi0 = B . S = 0,2 . 5.10^-3 = 10^-3 Wb. Suất điện động cực đại: E0 = N . omega . Phi0 = 100 . 100 . 10^-3 = 10 V.",
    "MeoGhiNho": "E0 = N . B . S . omega.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_127", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một máy phát điện xoay chiều có rôto quay với tốc độ 600 vòng/phút phát ra dòng điện có tần số 50 Hz. Nếu thay bằng rôto khác có số cặp cực tăng thêm 1 cặp cực thì để tần số vẫn giữ nguyên 50 Hz, tốc độ quay mới của rôto phải là:",
    "DapAnA": "500 vòng/phút", "DapAnB": "450 vòng/phút", "DapAnC": "400 vòng/phút", "DapAnD": "750 vòng/phút",
    "DapAnDung": "A",
    "GiaiThich": "Ban đầu: p1 = (60 . f) / n1 = (60 . 50) / 600 = 5 cặp cực. Khi tăng 1 cặp cực: p2 = 5 + 1 = 6 cặp cực. Tốc độ quay mới: n2 = (60 . f) / p2 = (60 . 50) / 6 = 3000 / 6 = 500 vòng/phút.",
    "MeoGhiNho": "p . n = const => p tăng thì n giảm tương ứng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_128", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Thả một thanh nam châm thẳng rơi tự do theo phương thẳng đứng đi xuyên qua tâm một vòng nhôm tròn kín đặt nằm ngang. Gia tốc rơi của thanh nam châm khi đang tiến vào vòng nhôm có đặc điểm:",
    "DapAnA": "Nhỏ hơn gia tốc trọng trường g (a < g) do lực từ cản trở chuyển động",
    "DapAnB": "Lớn hơn gia tốc trọng trường g (a > g) do vòng nhôm hút nam châm xuống",
    "DapAnC": "Luôn bằng đúng gia tốc trọng trường g (a = g)",
    "DapAnD": "Bằng 0 vì nam châm bị lơ lửng ngay lập tức",
    "DapAnDung": "A",
    "GiaiThich": "Theo định luật Lenz, dòng điện cảm ứng xuất hiện trong vòng nhôm sinh ra từ trường có chiều chống lại sự tăng của từ thông, tác dụng lực từ đẩy hướng lên cản trở sự rơi của nam châm. Hợp lực F = P - F_từ < P => gia tốc a = F / m < g.",
    "MeoGhiNho": "Định luật Lenz: chống lại nguyên nhân sinh ra nó -> nam châm rơi qua vòng dây luôn có a < g.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_129", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Trong các nhà máy phát điện công suất lớn (thủy điện, nhiệt điện), người ta thường thiết kế: cuộn dây đứng yên (stato) và nam châm điện quay (rôto). Ưu điểm chính của thiết kế này là:",
    "DapAnA": "Dòng điện cảm ứng công suất cực lớn được lấy trực tiếp từ stato ra ngoài mà không cần qua chổi quét và vành khuyên trượt",
    "DapAnB": "Làm cho máy phát điện có khối lượng nhẹ hơn",
    "DapAnC": "Không cần làm mát cuộn dây máy phát",
    "DapAnD": "Giúp nam châm không bị nóng",
    "DapAnDung": "A",
    "GiaiThich": "Dòng điện phát ra có điện áp hàng nghìn vôn và dòng điện hàng nghìn ampe. Nếu cuộn dây quay sẽ phải dùng vành khuyên và chổi quét tiếp xúc trượt, gây phát sinh tia lửa điện dữ dội, mòn tiếp điểm và nguy cơ cháy nổ. Để cuộn dây đứng yên giúp nối cố định đường dây cáp tải ra ngoài.",
    "MeoGhiNho": "Stato là cuộn dây giúp lấy dòng điện lớn trực tiếp mà không sinh tia lửa điện ở vành trượt.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_130", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai14", "TenBai": "Bài 14. Cảm ứng điện từ. Nguyên tắc tạo ra dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một thanh kim loại dài l = 40 cm chuyển động tịnh tiến đều với tốc độ v = 5 m/s trên hai thanh ray nằm ngang trong từ trường đều thẳng đứng có cảm ứng từ B = 0,5 T. Hai đầu thanh ray nối với một bóng đèn nhỏ. Hiệu điện thế cảm ứng xuất hiện giữa hai đầu thanh kim loại là:",
    "DapAnA": "1 V", "DapAnB": "2 V", "DapAnC": "0,5 V", "DapAnD": "4 V",
    "DapAnDung": "A",
    "GiaiThich": "Suất điện động cảm ứng do thanh kim loại cắt đường sức từ: e_c = B . l . v = 0,5 . 0,4 . 5 = 1 V.",
    "MeoGhiNho": "Thanh chuyển động cắt từ trường: e_c = B . l . v.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 15: TÁC DỤNG CỦA DÒNG ĐIỆN XOAY CHIỀU (bai15) - 10 CÂU (1 NB, 1 TH, 3 VD, 5 VDC)
# ==============================================================================
# NB: 1 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_131", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Nhận biết",
    "NoiDung": "Dòng điện xoay chiều có những tác dụng nào sau đây?",
    "DapAnA": "Tác dụng nhiệt, tác dụng phát quang và tác dụng từ",
    "DapAnB": "Chỉ có tác dụng nhiệt duy nhất",
    "DapAnC": "Chỉ có tác dụng từ duy nhất",
    "DapAnD": "Chỉ có tác dụng hóa học mạ điện",
    "DapAnDung": "A",
    "GiaiThich": "Dòng điện xoay chiều có đầy đủ tác dụng nhiệt (bàn là, nồi cơm), tác dụng quang (đèn thắp sáng) và tác dụng từ (quạt điện, máy biến áp). Lưu ý tác dụng hóa học (như mạ điện) chỉ thực hiện được bằng dòng điện một chiều.",
    "MeoGhiNho": "Dòng điện xoay chiều có 3 tác dụng chính: Nhiệt, Quang, Từ.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# TH: 1 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_132", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Thông hiểu",
    "NoiDung": "Tác dụng nào của dòng điện xoay chiều KHÔNG phụ thuộc vào chiều của dòng điện?",
    "DapAnA": "Tác dụng nhiệt và tác dụng phát quang",
    "DapAnB": "Tác dụng từ",
    "DapAnC": "Lực điện từ tác dụng lên nam châm",
    "DapAnD": "Chiều quay của kim nam châm thử",
    "DapAnDung": "A",
    "GiaiThich": "Tác dụng nhiệt (tỏa nhiệt Q = I^2.R.t tỉ lệ với bình phương dòng điện) và tác dụng phát quang không phụ thuộc vào chiều dòng điện. Còn tác dụng từ đổi chiều theo chiều dòng điện.",
    "MeoGhiNho": "Tác dụng nhiệt và quang không phụ thuộc chiều dòng điện.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VD: 3 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_133", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một máy biến áp lý tưởng có cuộn sơ cấp gồm N1 = 1000 vòng và cuộn thứ cấp gồm N2 = 200 vòng. Đặt vào hai đầu cuộn sơ cấp hiệu điện thế xoay chiều U1 = 220 V. Hiệu điện thế giữa hai đầu cuộn thứ cấp U2 là:",
    "DapAnA": "44 V", "DapAnB": "1100 V", "DapAnC": "110 V", "DapAnD": "88 V",
    "DapAnDung": "A",
    "GiaiThich": "Áp dụng công thức máy biến áp: U1 / U2 = N1 / N2 => U2 = U1 . (N2 / N1) = 220 . (200 / 1000) = 44 V.",
    "MeoGhiNho": "U2 = U1 . (N2 / N1). Máy hạ thế có N2 < N1 nên U2 < U1.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_134", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Để truyền tải một công suất điện nhất định đi xa, nếu tăng hiệu điện thế ở hai đầu đường dây truyền tải lên 4 lần bằng máy biến áp thì công suất hao phí do tỏa nhiệt trên đường dây sẽ:",
    "DapAnA": "Giảm đi 16 lần", "DapAnB": "Giảm đi 4 lần", "DapAnC": "Tăng lên 16 lần", "DapAnD": "Không đổi",
    "DapAnDung": "A",
    "GiaiThich": "Công suất hao phí tỏa nhiệt: Delta_P = R . P^2 / U^2. Công suất hao phí tỉ lệ nghịch với bình phương hiệu điện thế U^2. Khi U tăng 4 lần thì Delta_P giảm 4^2 = 16 lần.",
    "MeoGhiNho": "U tăng n lần thì công suất hao phí giảm n^2 lần.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_135", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Một máy biến áp lý tưởng có cuộn sơ cấp 500 vòng và cuộn thứ cấp 2500 vòng. Máy này là loại máy gì và có tác dụng làm thay đổi hiệu điện thế như thế nào?",
    "DapAnA": "Máy tăng áp, làm tăng hiệu điện thế lên 5 lần",
    "DapAnB": "Máy hạ áp, làm giảm hiệu điện thế đi 5 lần",
    "DapAnC": "Máy tăng áp, làm tăng hiệu điện thế lên 25 lần",
    "DapAnD": "Máy biến áp một chiều không làm đổi điện áp",
    "DapAnDung": "A",
    "GiaiThich": "Vì N2 = 2500 > N1 = 500 nên đây là máy tăng áp. Tỉ số tăng áp: N2 / N1 = 2500 / 500 = 5 lần.",
    "MeoGhiNho": "N2 > N1 là máy tăng áp (U2 > U1).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 5 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_136", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một máy biến áp lý tưởng có tỉ số vòng dây N1 / N2 = 10. Khi mắc cuộn sơ cấp vào nguồn xoay chiều U1 = 220 V thì dòng điện ở mạch thứ cấp đo được I2 = 5 A. Cường độ dòng điện ở cuộn sơ cấp I1 là:",
    "DapAnA": "0,5 A", "DapAnB": "50 A", "DapAnC": "2,5 A", "DapAnD": "1,0 A",
    "DapAnDung": "A",
    "GiaiThich": "Máy biến áp lý tưởng bảo toàn công suất P1 = P2 => U1 . I1 = U2 . I2 => I1 / I2 = U2 / U1 = N2 / N1 => I1 = I2 . (N2 / N1) = 5 . (1/10) = 0,5 A.",
    "MeoGhiNho": "Máy hạ áp (U giảm 10 lần) thì dòng điện tăng 10 lần: I1 = I2 / 10 = 0,5 A.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_137", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Tại sao lõi thép của máy biến áp không được đúc bằng một khối sắt thép đặc liền khối mà phải ghép từ nhiều lá thép silic mỏng có sơn lớp cách điện?",
    "DapAnA": "Để giảm thiểu dòng điện xoáy Foucault chạy trong lõi thép, giảm hao phí tỏa nhiệt",
    "DapAnB": "Để giảm bớt khối lượng và chi phí kim loại",
    "DapAnC": "Để tăng từ trường của nam châm vĩnh cửu",
    "DapAnD": "Để cách điện giữa cuộn sơ cấp và cuộn thứ cấp",
    "DapAnDung": "A",
    "GiaiThich": "Từ thông biến thiên sinh ra dòng điện xoáy Foucault chạy vòng khép kín trong lõi kim loại làm nóng máy dữ dội. Ghép các lá thép mỏng cách điện chia nhỏ đường đi dòng Foucault, triệt tiêu hao phí nhiệt.",
    "MeoGhiNho": "Lá thép mỏng cách điện giúp giảm dòng điện xoáy Foucault và giảm nhiệt hao phí.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_138", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Nếu vô tình cắm cuộn sơ cấp của máy biến áp vào một nguồn điện một chiều không đổi (như ắc quy 12 V) thì hiện tượng gì sẽ xảy ra?",
    "DapAnA": "Ở cuộn thứ cấp không có dòng điện và cuộn sơ cấp có nguy cơ bị cháy do quá nhiệt",
    "DapAnB": "Ở cuộn thứ cấp có dòng điện xoay chiều tần số cao",
    "DapAnC": "Máy biến áp hoạt động bình thường như nguồn xoay chiều",
    "DapAnD": "Hiệu điện thế ở thứ cấp tăng lên gấp 10 lần",
    "DapAnDung": "A",
    "GiaiThich": "Dòng một chiều không đổi tạo ra từ thông không đổi, không có hiện tượng cảm ứng điện từ nên U2 = 0. Ngoài ra không có hiện tượng tự cảm cản trở, cuộn sơ cấp chỉ có điện trở thuần rất nhỏ làm dòng I tăng cực lớn gây cháy cuộn dây.",
    "MeoGhiNho": "Máy biến áp CHỈ hoạt động với dòng xoay chiều, cắm điện 1 chiều sẽ bị cháy.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_139", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Điện năng được truyền tải từ trạm phát điện có công suất P không đổi đến nơi tiêu thụ. Khi hiệu điện thế truyền tải là U thì độ sụt áp trên đường dây là Delta_U = 0,1 U. Hiệu suất truyền tải điện năng lúc này là:",
    "DapAnA": "90%", "DapAnB": "80%", "DapAnC": "95%", "DapAnD": "81%",
    "DapAnDung": "A",
    "GiaiThich": "Hiệu suất truyền tải: H = P_tiêu_thụ / P_phát = (U - Delta_U) / U = 1 - Delta_U / U = 1 - 0,1 = 0,9 = 90%.",
    "MeoGhiNho": "H = 1 - Delta_U / U.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_140", "ChuongId": "chuong4", "TenChuong": "Chương IV. Điện từ",
    "BaiID": "bai15", "TenBai": "Bài 15. Tác dụng của dòng điện xoay chiều", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một máy biến áp có cuộn sơ cấp N1 = 2000 vòng được cấp điện áp xoay chiều U1 = 220 V. Để thắp sáng bình thường một bóng đèn loại 12 V thì số vòng dây N2 của cuộn thứ cấp phải bằng xấp xỉ:",
    "DapAnA": "109 vòng", "DapAnB": "183 vòng", "DapAnC": "240 vòng", "DapAnD": "85 vòng",
    "DapAnDung": "A",
    "GiaiThich": "N2 = N1 . (U2 / U1) = 2000 . (12 / 220) = 24000 / 220 ≈ 109,09 vòng ≈ 109 vòng.",
    "MeoGhiNho": "N2 = N1 . (U2 / U1).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 16: VÒNG NĂNG LƯỢNG TRÊN TRÁI ĐẤT. NĂNG LƯỢNG HOÁ THẠCH (bai16) - 11 CÂU (3 VD, 8 VDC)
# ==============================================================================
# VD: 3 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_141", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Nguồn gốc sâu xa của hầu hết các dạng năng lượng trên Trái Đất (như sinh khối, gió, thủy năng, nhiên liệu hóa thạch) bắt nguồn từ đâu?",
    "DapAnA": "Năng lượng bức xạ từ Mặt Trời", "DapAnB": "Nhiệt lượng trong lòng đất",
    "DapAnC": "Lực hấp dẫn của Mặt Trăng", "DapAnD": "Năng lượng phân rã hạt nhân phóng xạ",
    "DapAnDung": "A",
    "GiaiThich": "Mặt Trời cung cấp ánh sáng cho cây quang hợp tạo sinh khối và hóa thạch, đốt nóng không khí tạo gió, bốc hơi nước tạo mưa và dòng chảy thủy điện. Vậy nguồn gốc ban đầu là năng lượng Mặt Trời.",
    "MeoGhiNho": "Hầu hết mọi dạng năng lượng trên Trái Đất đều bắt nguồn từ năng lượng bức xạ Mặt Trời.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_142", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Nhiên liệu nào sau đây KHÔNG PHẢI là nhiên liệu hóa thạch?",
    "DapAnA": "Gỗ củi", "DapAnB": "Than đá", "DapAnC": "Dầu mỏ", "DapAnD": "Khí thiên nhiên",
    "DapAnDung": "A",
    "GiaiThich": "Gỗ củi là năng lượng sinh khối (biomass) có thể tái tạo trong thời gian ngắn. Còn than đá, dầu mỏ và khí thiên nhiên là nhiên liệu hóa thạch hình thành qua hàng triệu năm và không tái tạo.",
    "MeoGhiNho": "Nhiên liệu hóa thạch gồm 3 loại chính: Than đá, Dầu mỏ, Khí tự nhiên.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_143", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Chất khí nào sinh ra chủ yếu khi đốt cháy nhiên liệu hóa thạch là thủ phạm chính gây ra hiệu ứng nhà kính và biến đổi khí hậu?",
    "DapAnA": "Khí cacbonic (CO2)", "DapAnB": "Khí oxi (O2)", "DapAnC": "Khí nitơ (N2)", "DapAnD": "Khí heli (He)",
    "DapAnDung": "A",
    "GiaiThich": "Đốt cháy than, dầu khí thải ra một lượng khổng lồ CO2. Khí CO2 hấp thụ bức xạ nhiệt hồng ngoại từ mặt đất phát ra không cho thoát ra ngoài vũ trụ, làm Trái Đất nóng lên.",
    "MeoGhiNho": "Khí nhà kính chủ yếu là CO2 từ đốt nhiên liệu hóa thạch.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 8 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_144", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Hiện tượng mưa axit gây hại nghiêm trọng cho rừng cây, mùa màng và các công trình xây dựng chủ yếu do khí thải nào từ các nhà máy nhiệt điện đốt than phát tán vào khí quyển?",
    "DapAnA": "Lưu huỳnh đioxit (SO2) và các oxit của nitơ (NOx)",
    "DapAnB": "Khí mêtan (CH4) và hơi nước",
    "DapAnC": "Khí cacbon monoxit (CO) và ozon",
    "DapAnD": "Khí clo (Cl2) và hiđro",
    "DapAnDung": "A",
    "GiaiThich": "Trong than đá chứa nhiều tạp chất lưu huỳnh (S) và nitơ (N). Khi đốt cháy sinh ra SO2 và NOx, các khí này hòa tan trong nước mưa tạo thành axit sunfuric (H2SO4) và axit nitric (HNO3) gây mưa axit.",
    "MeoGhiNho": "Mưa axit do SO2 và NOx từ đốt than dầu tạo thành.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_145", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Năng lượng hóa thạch được phân loại là dạng năng lượng không thể tái tạo vì lí do cốt lõi nào?",
    "DapAnA": "Tốc độ hình thành mất hàng trăm triệu năm trong khi tốc độ khai thác và tiêu thụ của con người diễn ra quá nhanh",
    "DapAnB": "Năng lượng hóa thạch không thể chuyển hóa thành điện năng",
    "DapAnC": "Trữ lượng của than đá trên Trái Đất đã cạn kiệt hoàn toàn",
    "DapAnD": "Không thể vận chuyển than dầu qua đường biển",
    "DapAnDung": "A",
    "GiaiThich": "Nhiên liệu hóa thạch được hình thành từ xác thực vật, động vật bị vùi lấp dưới áp suất và nhiệt độ cao trong lòng đất qua hàng trăm triệu năm. Với tốc độ khai thác khổng lồ hiện nay, chúng sẽ cạn kiệt trong vài thập kỷ tới.",
    "MeoGhiNho": "Không thể tái tạo vì hình thành mất hàng trăm triệu năm, dùng nhanh hơn sinh ra.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_146", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một nhà máy nhiệt điện đốt than có công suất phát điện P = 600 MW và hiệu suất chuyển đổi nhiệt - điện là 36%. Năng suất tỏa nhiệt của than đá là q = 30 MJ/kg. Khối lượng than đá mà nhà máy này tiêu thụ trong một ngày đêm (24 giờ) xấp xỉ bằng:",
    "DapAnA": "4800 tấn", "DapAnB": "1728 tấn", "DapAnC": "3600 tấn", "DapAnD": "5400 tấn",
    "DapAnDung": "A",
    "GiaiThich": "Điện năng phát ra trong 24h: E = P . t = 600.10^6 . (24 . 3600) = 5,184.10^13 J. Nhiệt lượng đốt than cần cung cấp: Q_tp = E / H = 5,184.10^13 / 0,36 = 1,44.10^14 J. Khối lượng than tiêu thụ: m = Q_tp / q = 1,44.10^14 / (30.10^6) = 4,8.10^6 kg = 4800 tấn.",
    "MeoGhiNho": "m = (P . t) / (H . q). Nhà máy điện 600MW tiêu thụ gần 5000 tấn than mỗi ngày.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_147", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Sự hình thành gió trong khí quyển Trái Đất là kết quả của quá trình chuyển hóa năng lượng nào từ Mặt Trời?",
    "DapAnA": "Quang năng Mặt Trời đốt nóng không khí không đều -> Nhiệt năng tạo chênh lệch áp suất -> Động năng của các luồng khí",
    "DapAnB": "Mặt Trời tác dụng lực hấp dẫn hút trực tiếp các phân tử khí",
    "DapAnC": "Điện tích từ gió Mặt Trời đẩy không khí Trái Đất chuyển động",
    "DapAnD": "Ánh sáng phân rã các phân tử khí quyển",
    "DapAnDung": "A",
    "GiaiThich": "Bức xạ nhiệt Mặt Trời đốt nóng xích đạo nhiều hơn hai cực và lục địa nóng nhanh hơn đại dương. Không khí nóng nở ra bay lên, khí lạnh tràn vào bù chỗ, tạo ra luồng chuyển động không khí gọi là gió.",
    "MeoGhiNho": "Quang năng -> Nhiệt năng (chênh lệch áp suất) -> Động năng luồng gió.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_148", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Trong chuỗi mắt xích 'Cỏ -> Bò -> Con người', năng lượng được chuyển hóa từ dạng nào sang dạng nào?",
    "DapAnA": "Quang năng (Mặt Trời) -> Hóa năng (trong cỏ) -> Hóa năng (thịt bò) -> Cơ năng và nhiệt lượng (con người)",
    "DapAnB": "Điện năng -> Nhiệt năng -> Cơ năng",
    "DapAnC": "Cơ năng -> Hóa năng -> Quang năng",
    "DapAnD": "Hạt nhân -> Quang năng -> Điện năng",
    "DapAnDung": "A",
    "GiaiThich": "Cỏ quang hợp biến quang năng thành hóa năng dự trữ trong chất hữu cơ. Bò ăn cỏ biến thành hóa năng cơ bắp. Con người tiêu thụ thịt bò chuyển hóa thành nhiệt duy trì thân nhiệt và cơ năng vận động.",
    "MeoGhiNho": "Quang năng -> Hóa năng thực vật -> Hóa năng động vật -> Cơ năng & Nhiệt năng con người.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_149", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Hiệp định Khí hậu Paris và cam kết Net Zero (phát thải ròng bằng 0 vào năm 2050) đặt mục tiêu cấp bách gì cho ngành năng lượng toàn cầu?",
    "DapAnA": "Giảm dần và tiến tới loại bỏ nhiệt điện than, đẩy mạnh phát triển năng lượng tái tạo",
    "DapAnB": "Ngừng sử dụng hoàn toàn điện năng trên toàn cầu",
    "DapAnC": "Chuyển toàn bộ các nhà máy phát điện sang đốt dầu mỏ",
    "DapAnD": "Giữ nguyên tỉ trọng điện than như hiện nay",
    "DapAnDung": "A",
    "GiaiThich": "Mục tiêu Net Zero đòi hỏi cắt giảm triệt để phát thải khí nhà kính bằng cách đóng cửa dần các nhà máy nhiệt điện than, chuyển dịch sang năng lượng gió, mặt trời, thủy điện và tích trữ năng lượng xanh.",
    "MeoGhiNho": "Net Zero: Thay thế nhiệt điện than bằng năng lượng sạch tái tạo.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_150", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Khi đốt cháy hoàn toàn 1 lít xăng (khối lượng m ≈ 0,75 kg, năng suất tỏa nhiệt q = 46 MJ/kg) sẽ tỏa ra một nhiệt lượng là:",
    "DapAnA": "34,5 MJ", "DapAnB": "46,0 MJ", "DapAnC": "61,3 MJ", "DapAnD": "23,0 MJ",
    "DapAnDung": "A",
    "GiaiThich": "Áp dụng công thức tính nhiệt lượng tỏa ra khi đốt cháy nhiên liệu: Q = m . q = 0,75 . 46 = 34,5 MJ (34.500.000 J).",
    "MeoGhiNho": "Q = m . q (m tính bằng kg, q tính bằng J/kg).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_151", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai16", "TenBai": "Bài 16. Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Vòng tuần hoàn nước trong tự nhiên (bốc hơi -> mây -> mưa -> sông suối -> biển) duy trì liên tục chủ yếu nhờ nguồn năng lượng nào?",
    "DapAnA": "Năng lượng bức xạ nhiệt của Mặt Trời và thế năng trọng trường của Trái Đất",
    "DapAnB": "Năng lượng dòng hải lưu dưới biển",
    "DapAnC": "Năng lượng địa nhiệt từ lòng đất",
    "DapAnD": "Năng lượng hóa thạch dưới đáy đại dương",
    "DapAnDung": "A",
    "GiaiThich": "Nhiệt Mặt Trời làm nước biển bốc hơi lên cao thành mây; sau đó trọng lực Trái Đất kéo hạt mưa rơi xuống, thế năng trọng trường đưa nước từ núi cao chảy qua sông suối về biển.",
    "MeoGhiNho": "Vòng tuần hoàn nước vận hành nhờ: Nhiệt Mặt Trời (bốc hơi) + Trọng lực Trái Đất (chảy về biển).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# ==============================================================================
# BÀI 17: MỘT SỐ DẠNG NĂNG LƯỢNG TÁI TẠO (bai17) - 11 CÂU (2 NB, 3 VD, 6 VDC)
# ==============================================================================
# NB: 2 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_152", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Nhận biết",
    "NoiDung": "Năng lượng tái tạo là gì?",
    "DapAnA": "Nguồn năng lượng có sẵn trong tự nhiên, liên tục được tái sinh hoặc hầu như vô tận",
    "DapAnB": "Nguồn năng lượng khai thác từ các mỏ khoáng sản trong lòng đất",
    "DapAnC": "Năng lượng chỉ dùng được đúng một lần duy nhất rồi mất hẳn",
    "DapAnD": "Năng lượng sinh ra do phản ứng cháy của than đá",
    "DapAnDung": "A",
    "GiaiThich": "Năng lượng tái tạo là năng lượng từ các nguồn tự nhiên liên tục được phục hồi nhanh chóng và không bị cạn kiệt theo thời gian như mặt trời, gió, nước, địa nhiệt.",
    "MeoGhiNho": "Tái tạo: liên tục tự sinh ra, vô tận, sạch và thân thiện với môi trường.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_153", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Nhận biết",
    "NoiDung": "Thiết bị pin quang điện (pin mặt trời) có chức năng chính là gì?",
    "DapAnA": "Biến đổi trực tiếp quang năng của ánh sáng mặt trời thành điện năng",
    "DapAnB": "Biến đổi quang năng thành nhiệt năng đun sôi nước",
    "DapAnC": "Tích trữ năng lượng gió để tạo ra cơ năng",
    "DapAnD": "Biến đổi điện năng thành hóa năng trong bình ắc quy",
    "DapAnDung": "A",
    "GiaiThich": "Pin quang điện hoạt động dựa trên hiện tượng quang điện trong của chất bán dẫn, biến đổi trực tiếp quang năng ánh sáng thành điện năng một chiều.",
    "MeoGhiNho": "Pin mặt trời: Biến trực tiếp Quang năng thành Điện năng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VD: 3 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_154", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Tua bin gió biến đổi dạng năng lượng nào thành điện năng?",
    "DapAnA": "Động năng của luồng gió", "DapAnB": "Thế năng của không khí",
    "DapAnC": "Hóa năng của khí quyển", "DapAnD": "Nhiệt năng của gió",
    "DapAnDung": "A",
    "GiaiThich": "Gió thổi làm quay cánh quạt tua bin, biến động năng của không khí thành cơ năng quay trục máy phát, máy phát điện biến cơ năng thành điện năng.",
    "MeoGhiNho": "Điện gió: Động năng gió -> Cơ năng cánh quạt -> Điện năng.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_155", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Năng lượng sinh học (biogas - khí sinh học) được tạo ra chủ yếu từ quá trình nào?",
    "DapAnA": "Sự lên men kị khí của chất thải hữu cơ và phân gia súc gia cầm",
    "DapAnB": "Quá trình chưng cất dầu mỏ ở nhiệt độ cao",
    "DapAnC": "Đốt cháy than đá trong lò kín thiếu oxy",
    "DapAnD": "Sự bay hơi của cồn công nghiệp",
    "DapAnDung": "A",
    "GiaiThich": "Biogas chứa chủ yếu khí mêtan (CH4), được sinh ra khi vi sinh vật kị khí phân hủy chất thải hữu cơ, rơm rạ hoặc phân vật nuôi trong hầm ủ kín không có oxy.",
    "MeoGhiNho": "Biogas hình thành do vi khuẩn kị khí lên men chất thải chăn nuôi.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_156", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng",
    "NoiDung": "Dạng năng lượng tái tạo nào khai thác sự chênh lệch mực nước biển giữa lúc nước dâng và nước rút dưới tác dụng của lực hấp dẫn từ Mặt Trăng?",
    "DapAnA": "Năng lượng thủy triều", "DapAnB": "Năng lượng địa nhiệt",
    "DapAnC": "Năng lượng sóng biển", "DapAnD": "Năng lượng sinh khối",
    "DapAnDung": "A",
    "GiaiThich": "Thủy triều lên xuống tạo ra thế năng chênh lệch cột nước khổng lồ tại các vịnh và cửa sông, được dùng quay tua bin phát điện trong các nhà máy điện thủy triều.",
    "MeoGhiNho": "Nước biển dâng rút do Mặt Trăng là năng lượng thủy triều.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

# VDC: 6 câu
mcq_batch4.append({
    "CauHoiID": "C9_BS_157", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một tấm pin mặt trời có diện tích bề mặt S = 2 m^2 và hiệu suất chuyển đổi quang - điện là 20%. Cường độ bức xạ mặt trời chiếu vuông góc vào mặt pin là 1000 W/m^2. Công suất điện mà tấm pin này phát ra là:",
    "DapAnA": "400 W", "DapAnB": "200 W", "DapAnC": "500 W", "DapAnD": "1000 W",
    "DapAnDung": "A",
    "GiaiThich": "Tổng công suất bức xạ mặt trời nhận được: P_quang = I_bx . S = 1000 . 2 = 2000 W. Công suất điện phát ra: P_điện = H . P_quang = 20% . 2000 = 400 W.",
    "MeoGhiNho": "P_điện = H . S . I_bức xạ.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_158", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Nhược điểm kỹ thuật lớn nhất của năng lượng điện mặt trời và năng lượng điện gió trong việc vận hành ổn định hệ thống điện quốc gia là:",
    "DapAnA": "Tính bất định và phụ thuộc mạnh vào điều kiện thời tiết (không phát điện vào ban đêm, khi lặng gió hoặc mây mù)",
    "DapAnB": "Tạo ra quá nhiều khí thải CO2 trong quá trình vận hành",
    "DapAnC": "Tiêu tốn nguồn nước ngọt khổng lồ để làm mát",
    "DapAnD": "Không thể truyền tải điện đi xa",
    "DapAnDung": "A",
    "GiaiThich": "Mặt trời chỉ chiếu sáng ban ngày và bị gián đoạn khi mây mù; gió lúc mạnh lúc yếu thất thường. Tính bất định này gây nguy cơ mất ổn định tần số lưới điện nếu không có hệ thống pin lưu trữ hoặc nguồn điện linh hoạt hỗ trợ.",
    "MeoGhiNho": "Điện mặt trời & gió có tính gián đoạn, bất định cao, phụ thuộc thời tiết.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_159", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một nhà máy điện gió có 10 tua bin. Mỗi tua bin gió có đường kính cánh quạt D = 80 m, vận hành ở tốc độ gió trung bình v = 10 m/s với hiệu suất phát điện 40%. Biết khối lượng riêng không khí rho_kk = 1,2 kg/m^3. Tổng công suất phát điện của nhà máy xấp xỉ bằng:",
    "DapAnA": "12,0 MW", "DapAnB": "3,0 MW", "DapAnC": "6,0 MW", "DapAnD": "24,0 MW",
    "DapAnDung": "A",
    "GiaiThich": "Bán kính cánh quạt R = 40 m. Diện tích quét: S = pi . R^2 = 3,1416 . 1600 ≈ 5026,5 m^2. Công suất gió qua 1 tua bin: P_gió = 1/2 . rho . S . v^3 = 0,5 . 1,2 . 5026,5 . 1000 ≈ 3,016.10^6 W = 3,016 MW. Công suất điện 1 tua bin: P_1 = 0,40 . 3,016 ≈ 1,206 MW. Tổng 10 tua bin: P_tổng ≈ 10 . 1,206 ≈ 12,06 MW ≈ 12 MW.",
    "MeoGhiNho": "Công suất gió: P = 1/2 . rho . S . v^3. Tỉ lệ thuận với lập phương vận tốc gió v^3.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_160", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Nhà máy thủy điện tích năng đóng vai trò quan trọng như thế nào đối với sự phát triển của năng lượng tái tạo?",
    "DapAnA": "Đóng vai trò như một cục pin khổng lồ lưu trữ điện: bơm nước lên hồ cao khi thừa điện (ban ngày nắng gắt) và xả nước phát điện vào giờ cao điểm khi thiếu điện",
    "DapAnB": "Tự động sinh ra điện năng vĩnh cửu mà không cần nguồn nước",
    "DapAnC": "Lọc sạch toàn bộ bùn cát lòng sông",
    "DapAnD": "Làm mát các tấm pin năng lượng mặt trời",
    "DapAnDung": "A",
    "GiaiThich": "Thủy điện tích năng có 2 hồ chứa trên và dưới. Lúc thừa điện gió và mặt trời thì dùng điện bơm nước lên hồ trên tích lũy thế năng; lúc trời tối lặng gió thiếu điện thì xả nước xuống quay tua bin phát điện cứu lưới.",
    "MeoGhiNho": "Thủy điện tích năng là bể chứa năng lượng lưu trữ điện tái tạo quy mô lưới điện.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_161", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Một hệ thống bình nước nóng năng lượng mặt trời thu nhiệt với diện tích hấp thụ S = 2,5 m^2 và hiệu suất chuyển đổi nhiệt là 60%. Nếu mỗi ngày có 5 giờ nắng với cường độ bức xạ trung bình 800 W/m^2 thì lượng nhiệt thu được mỗi ngày có thể đun nóng bao nhiêu lít nước từ 20 độ C lên 60 độ C? (c_nước = 4200 J/(kg.K))",
    "TuKhoaChamDiem": "Q = 21.6 MJ; Delta_t = 40 độ; m = 128.6 kg; khoảng 129 lít nước",
    "DapAnA": "129 lít", "DapAnB": "200 lít", "DapAnC": "85 lít", "DapAnD": "250 lít",
    "DapAnDung": "A",
    "GiaiThich": "Thời gian t = 5 . 3600 = 18000 s. Tổng bức xạ nhận được: E = S . I . t = 2,5 . 800 . 18000 = 36.10^6 J = 36 MJ. Nhiệt lượng nước nhận được: Q = H . E = 0,60 . 36.10^6 = 21,6.10^6 J. Khối lượng nước: m = Q / (c . Delta_t) = 21,6.10^6 / (4200 . (60 - 20)) = 21,6.10^6 / 168000 ≈ 128,57 kg ≈ 129 lít nước.",
    "MeoGhiNho": "m = (H . S . I . t) / (c . Delta_t).", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})
mcq_batch4.append({
    "CauHoiID": "C9_BS_162", "ChuongId": "chuong5", "TenChuong": "Chương V. Năng lượng và sự biến đổi năng lượng",
    "BaiID": "bai17", "TenBai": "Bài 17. Một số dạng năng lượng tái tạo", "LoaiCauHoi": "BaiHoc", "MucDo": "Vận dụng cao",
    "NoiDung": "Năng lượng địa nhiệt khai thác nguồn nhiệt năng từ đâu?",
    "DapAnA": "Nhiệt lượng nội tại tích tụ trong lòng đất sâu (từ các tầng đá nóng và túi magma)",
    "DapAnB": "Nhiệt lượng từ ánh nắng mặt trời chiếu xuống mặt đất mùa hè",
    "DapAnC": "Nhiệt tỏa ra từ sự phân hủy của rác thải chôn lấp",
    "DapAnD": "Nhiệt lượng sinh ra do ma sát của các dòng sông",
    "DapAnDung": "A",
    "GiaiThich": "Địa nhiệt là năng lượng nhiệt dự trữ sâu trong lòng đất, sinh ra từ sự hình thành ban đầu của hành tinh và sự phân rã phóng xạ của các khoáng chất. Nước ngầm tiếp xúc đá nóng hóa hơi làm quay tua bin phát điện.",
    "MeoGhiNho": "Địa nhiệt là nhiệt năng tự nhiên từ sâu trong lòng đất.", "TrangThai": "Xuất bản", "GiaoVienID": "GV001"
})

print(f"Prepared {len(mcq_batch4)} MCQs for Batch 4.")
