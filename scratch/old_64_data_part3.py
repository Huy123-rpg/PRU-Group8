# Data definitions for the 64 old essay questions
# Chapters III, IV, V: Electricity, Electromagnetism, Energy (Questions 37 to 64)

OLD_64_STEP_DATA_P3 = {
    # Bài 11: Điện trở. Định luật Ohm
    "TL_C3_L11_Minh": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Phát biểu định luật Ohm",
                "noiDung": "Định luật Ohm: Cường độ dòng điện chạy qua dây dẫn tỉ lệ thuận với hiệu điện thế đặt vào hai đầu dây và tỉ lệ nghịch với điện trở của dây.",
                "diem": 5.0, "diemMotPhan": "Nêu đúng tỉ lệ thuận với hiệu điện thế và tỉ lệ nghịch với điện trở được 5.0đ; thiếu 1 vế được 2.5đ.",
                "yeuCau": "Tỉ lệ thuận với U, tỉ lệ nghịch với R.",
                "loiThuongGap": "Nói tỉ lệ thuận với điện trở; nhầm lẫn giữa U và I.",
                "tuongDuong": "Phát biểu theo chuẩn SGK KHTN 9."
            },
            {
                "stepId": "S2", "tenBuoc": "Viết hệ thức và nêu đơn vị các đại lượng",
                "noiDung": "Hệ thức: I = U / R. Trong đó: I là cường độ dòng điện (Ampe - A), U là hiệu điện thế (Vôn - V), R là điện trở (Ôm - Ω).",
                "diem": 5.0, "diemMotPhan": "Viết đúng I = U/R được 2.5đ; nêu đủ tên và đơn vị 3 đại lượng (A, V, Ω) được 2.5đ.",
                "yeuCau": "I = U/R; Ampe (A), Vôn (V), Ôm (Ω).",
                "loiThuongGap": "Ghi I = U.R; nhầm đơn vị điện trở sang Vôn.",
                "tuongDuong": "U = I * R hoặc R = U / I."
            }
        ]
    },
    "TL_C3_L11_Lan": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Nêu các yếu tố ảnh hưởng đến điện trở",
                "noiDung": "Điện trở của một đoạn dây dẫn phụ thuộc vào 3 yếu tố: 1) Chiều dài của dây dẫn l (tỉ lệ thuận); 2) Tiết diện của dây dẫn S (tỉ lệ nghịch); 3) Vật liệu làm dây dẫn đặc trưng bởi điện trở suất ρ.",
                "diem": 5.0, "diemMotPhan": "Kể đủ 3 yếu tố kèm tính chất tỉ lệ được 5.0đ; kể thiếu 1 yếu tố được 3.0đ.",
                "yeuCau": "Chiều dài (tỉ lệ thuận), tiết diện (tỉ lệ nghịch), điện trở suất vật liệu.",
                "loiThuongGap": "Kể thêm hiệu điện thế và cường độ dòng điện (sai vì R không phụ thuộc U và I).",
                "tuongDuong": "Phụ thuộc l, S và bản chất vật liệu ρ."
            },
            {
                "stepId": "S2", "tenBuoc": "Viết công thức tính điện trở hình trụ và đơn vị",
                "noiDung": "Công thức tính điện trở: R = ρ . (l / S). Trong đó: R là điện trở (Ω), ρ là điện trở suất (Ω.m), l là chiều dài dây (m), S là tiết diện dây (m²).",
                "diem": 5.0, "diemMotPhan": "Viết đúng công thức R = ρ.l/S được 3.0đ; nêu đúng tên và đơn vị các đại lượng được 2.0đ.",
                "yeuCau": "R = ρ . l / S; Ω, Ω.m, m, m².",
                "loiThuongGap": "Viết R = ρ.S/l; ghi đơn vị ρ là Ω thay vì Ω.m; nhầm đơn vị S là mm² chưa đổi m².",
                "tuongDuong": "R = (ρ * l) / S."
            }
        ]
    },
    "TL_C3_L11_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tính cường độ dòng điện ban đầu",
                "noiDung": "Áp dụng định luật Ohm: I = U / R = 12 / 24 = 0,5 (A).",
                "diem": 3.5, "diemMotPhan": "Viết công thức I = U/R được 1.5đ; tính ra I = 0.5 A đúng được 2.0đ.",
                "yeuCau": "I = 0.5 A.",
                "loiThuongGap": "Lấy 24/12 = 2 A; sai đơn vị.",
                "tuongDuong": "I = 0,5 A."
            },
            {
                "stepId": "S2", "tenBuoc": "Tính hiệu điện thế mới cần thiết",
                "noiDung": "Để cường độ dòng điện là I' = 1 A, vì điện trở của dây dẫn không đổi (R = 24 Ω), hiệu điện thế mới cần đặt vào là: U' = I' . R = 1 . 24 = 24 (V).",
                "diem": 3.5, "diemMotPhan": "Viết U' = I'.R được 1.5đ; tính ra U' = 24 V đúng được 2.0đ.",
                "yeuCau": "U' = 24 V.",
                "loiThuongGap": "Lấy 1/24 V.",
                "tuongDuong": "Dùng tỉ lệ I tăng gấp đôi (1/0.5 = 2) nên U' cũng tăng gấp đôi: U' = 12 * 2 = 24 V."
            },
            {
                "stepId": "S3", "tenBuoc": "Tính độ tăng hiệu điện thế và kết luận",
                "noiDung": "Lượng hiệu điện thế cần phải tăng thêm: ΔU = U' - U = 24 - 12 = 12 (V). Đáp số: I = 0,5 A; cần tăng hiệu điện thế thêm 12 V.",
                "diem": 3.0, "diemMotPhan": "Tính ra ΔU = 12 V được 2.0đ; đáp số đầy đủ cả 2 ý được 1.0đ.",
                "yeuCau": "ΔU = 12 V.",
                "loiThuongGap": "Chỉ tính ra U' = 24 V mà quên trừ U ban đầu để tính độ tăng thêm ΔU.",
                "tuongDuong": "Cần tăng thêm 12 V."
            }
        ]
    },
    "TL_C3_L11_Ha": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Dạng hình học của đồ thị I theo U",
                "noiDung": "Đồ thị biểu diễn sự phụ thuộc của cường độ dòng điện I vào hiệu điện thế U là một ĐƯỜNG THẲNG ĐI QUA GỐC TỌA ĐỘ O(0,0).",
                "diem": 3.5, "diemMotPhan": "Khẳng định là đường thẳng đi qua gốc tọa độ O được 3.5đ; chỉ nói đường thẳng được 2.0đ.",
                "yeuCau": "Đường thẳng đi qua gốc tọa độ O(0,0).",
                "loiThuongGap": "Nói là đường cong parabol hoặc hyperbol.",
                "tuongDuong": "Đồ thị hàm số bậc nhất đồng biến đi qua gốc tọa độ (y = ax)."
            },
            {
                "stepId": "S2", "tenBuoc": "Ý nghĩa vật lý của độ dốc đồ thị",
                "noiDung": "Độ dốc (hệ số góc k) của đường thẳng này bằng k = I / U = 1 / R (được gọi là độ dẫn điện của vật dẫn).",
                "diem": 3.5, "diemMotPhan": "Chỉ ra hệ số góc k = I/U = 1/R (nghịch đảo của điện trở) được 3.5đ.",
                "yeuCau": "Độ dốc tỉ lệ nghịch với điện trở R (hệ số góc = 1/R).",
                "loiThuongGap": "Nói độ dốc bằng chính điện trở R.",
                "tuongDuong": "Nếu đồ thị vẽ U theo trục tung, I theo trục hoành thì hệ số góc là R."
            },
            {
                "stepId": "S3", "tenBuoc": "Quy luật so sánh điện trở qua độ dốc",
                "noiDung": "Nếu đồ thị I theo U càng dốc (nghiêng nhiều về trục I) thì độ dẫn điện càng lớn, điện trở R của dây dẫn càng nhỏ. Ngược lại, đường càng ít dốc (thoai thoải) thì điện trở R càng lớn.",
                "diem": 3.0, "diemMotPhan": "Giải thích đúng đường càng dốc thì điện trở càng nhỏ được 3.0đ.",
                "yeuCau": "Đường càng dốc thì R càng nhỏ (với đồ thị I theo U).",
                "loiThuongGap": "Nhầm lẫn chiều dốc với độ lớn của R.",
                "tuongDuong": "So sánh giá trị I tại cùng một hiệu điện thế U."
            }
        ]
    },

    # Bài 12: Đoạn mạch nối tiếp, song song
    "TL_C3_L12_Minh": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Đặc điểm cường độ dòng điện trong đoạn mạch nối tiếp",
                "noiDung": "Cường độ dòng điện có giá trị như nhau tại mọi điểm trong mạch: I = I1 = I2.",
                "diem": 3.0, "diemMotPhan": "Viết đúng I = I1 = I2 được 3.0đ.",
                "yeuCau": "I = I1 = I2.",
                "loiThuongGap": "Viết I = I1 + I2.",
                "tuongDuong": "Dòng điện bằng nhau qua các điện trở."
            },
            {
                "stepId": "S2", "tenBuoc": "Đặc điểm hiệu điện thế trong đoạn mạch nối tiếp",
                "noiDung": "Hiệu điện thế giữa hai đầu đoạn mạch bằng tổng các hiệu điện thế giữa hai đầu mỗi điện trở thành phần: U = U1 + U2.",
                "diem": 3.5, "diemMotPhan": "Viết đúng U = U1 + U2 được 3.5đ.",
                "yeuCau": "U = U1 + U2.",
                "loiThuongGap": "Viết U = U1 = U2.",
                "tuongDuong": "Hiệu điện thế toàn mạch bằng tổng hiệu điện thế thành phần."
            },
            {
                "stepId": "S3", "tenBuoc": "Đặc điểm điện trở tương đương trong đoạn mạch nối tiếp",
                "noiDung": "Điện trở tương đương của đoạn mạch bằng tổng các điện trở thành phần: Rtd = R1 + R2.",
                "diem": 3.5, "diemMotPhan": "Viết đúng Rtd = R1 + R2 được 3.5đ.",
                "yeuCau": "Rtd = R1 + R2.",
                "loiThuongGap": "Áp dụng nhầm công thức nghịch đảo của mạch song song.",
                "tuongDuong": "Rtd = R1 + R2."
            }
        ]
    },
    "TL_C3_L12_Lan": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Lý do 1: Đảm bảo đúng hiệu điện thế định mức",
                "noiDung": "Mắc song song giúp mỗi thiết bị điện đều nhận được đúng hiệu điện thế định mức của lưới điện gia đình (220 V), vì trong mạch song song U = U1 = U2 = ... = 220 V. Nhờ đó các thiết bị hoạt động đúng công suất thiết kế.",
                "diem": 5.0, "diemMotPhan": "Nêu mỗi thiết bị đều chịu đúng hiệu điện thế định mức 220V được 5.0đ; giải thích U = U định mức.",
                "yeuCau": "Mọi thiết bị hoạt động ở đúng hiệu điện thế định mức 220 V.",
                "loiThuongGap": "Không nhắc đến hiệu điện thế 220V hay hiệu điện thế định mức.",
                "tuongDuong": "U giữa 2 đầu mỗi thiết bị bằng U nguồn."
            },
            {
                "stepId": "S2", "tenBuoc": "Lý do 2: Đảm bảo tính độc lập khi vận hành",
                "noiDung": "Mắc song song giúp các thiết bị hoạt động hoàn toàn độc lập với nhau. Khi ta bật/tắt hoặc có một thiết bị bị sự cố đứt dây, cháy hỏng thì các thiết bị khác trong nhà vẫn có dòng điện chạy qua và hoạt động bình thường (không bị ngắt toàn bộ mạch như mắc nối tiếp).",
                "diem": 5.0, "diemMotPhan": "Nêu các thiết bị hoạt động độc lập, một cái hỏng các cái khác vẫn chạy được 5.0đ.",
                "yeuCau": "Hoạt động độc lập, ngắt/hỏng một thiết bị không ảnh hưởng thiết bị khác.",
                "loiThuongGap": "Chỉ nói cho tiện mà không giải thích cơ chế ngắt mạch nối tiếp.",
                "tuongDuong": "Đóng mở riêng rẽ từng nhánh mạch."
            }
        ]
    },
    "TL_C3_L12_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tính điện trở tương đương của đoạn mạch song song",
                "noiDung": "Công thức điện trở tương đương của hai điện trở mắc song song: Rtd = (R1 . R2) / (R1 + R2) = (20 . 30) / (20 + 30) = 600 / 50 = 12 (Ω).",
                "diem": 4.5, "diemMotPhan": "Viết đúng công thức 1/Rtd = 1/R1 + 1/R2 hoặc Rtd = (R1.R2)/(R1+R2) được 2.0đ; thay số tính ra 12 Ω được 2.5đ.",
                "yeuCau": "Rtd = 12 Ω.",
                "loiThuongGap": "Tính cộng Rtd = 20 + 30 = 50 Ω (nhầm sang nối tiếp); tính 1/Rtd = 50/600 rồi quên nghịch đảo.",
                "tuongDuong": "1/Rtd = 1/20 + 1/30 = 5/60 = 1/12 => Rtd = 12 Ω."
            },
            {
                "stepId": "S2", "tenBuoc": "Tính cường độ dòng điện mạch chính",
                "noiDung": "Áp dụng định luật Ohm cho toàn mạch: I = U / Rtd = 12 / 12 = 1 (A).",
                "diem": 4.0, "diemMotPhan": "Viết I = U/Rtd được 2.0đ; tính ra I = 1 A đúng được 2.0đ.",
                "yeuCau": "I = 1 A.",
                "loiThuongGap": "Lấy U chia cho từng R mà không cộng lại; quên đơn vị Ampe.",
                "tuongDuong": "Cách khác: I1 = 12/20 = 0.6A; I2 = 12/30 = 0.4A => I = I1 + I2 = 1A."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận đáp số",
                "noiDung": "Đáp số: Điện trở tương đương Rtd = 12 Ω; Cường độ dòng điện mạch chính I = 1 A.",
                "diem": 1.5, "diemMotPhan": "Ghi đầy đủ cả 2 đáp số kèm đơn vị chuẩn được 1.5đ.",
                "yeuCau": "Rtd = 12 Ω; I = 1 A.",
                "loiThuongGap": "Thiếu đáp số tổng kết.",
                "tuongDuong": "Ghi Rtd = 12 Ω, I = 1 A."
            }
        ]
    },
    "TL_C3_L12_Ha": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Thiết lập điện trở tương đương khi mắc nối tiếp",
                "noiDung": "Khi hai điện trở R1 = R2 = R mắc nối tiếp, điện trở tương đương là: R_nt = R1 + R2 = R + R = 2R.",
                "diem": 3.0, "diemMotPhan": "Tính ra R_nt = 2R được 3.0đ.",
                "yeuCau": "R_nt = 2R.",
                "loiThuongGap": "Nhầm công thức nối tiếp.",
                "tuongDuong": "R_nt = 2 * R."
            },
            {
                "stepId": "S2", "tenBuoc": "Thiết lập điện trở tương đương khi mắc song song",
                "noiDung": "Khi hai điện trở R1 = R2 = R mắc song song, điện trở tương đương là: 1/R_ss = 1/R + 1/R = 2/R => R_ss = R / 2.",
                "diem": 3.5, "diemMotPhan": "Tính ra R_ss = R/2 được 3.5đ.",
                "yeuCau": "R_ss = R / 2.",
                "loiThuongGap": "Quên nghịch đảo để nguyên 2/R.",
                "tuongDuong": "R_ss = (R*R)/(R+R) = R/2."
            },
            {
                "stepId": "S3", "tenBuoc": "So sánh tỉ số và rút ra kết luận",
                "noiDung": "Lập tỉ số: R_nt / R_ss = (2R) / (R / 2) = 4 lần. Kết luận: Khi mắc nối tiếp, điện trở tương đương lớn gấp 4 lần so với khi mắc song song hai điện trở giống nhau.",
                "diem": 3.5, "diemMotPhan": "Lập tỉ số ra 4 lần được 2.5đ; kết luận rõ ràng được 1.0đ.",
                "yeuCau": "R_nt / R_ss = 4 lần.",
                "loiThuongGap": "Tính 2 / 0.5 ra 1 hoặc 2 lần.",
                "tuongDuong": "R_nt = 4 R_ss."
            }
        ]
    },

    # Bài 13: Năng lượng của dòng điện và công suất điện
    "TL_C3_L13_Minh": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Viết công thức công suất điện và đơn vị",
                "noiDung": "Công thức công suất điện của đoạn mạch: P = U . I. Trong đó: P là công suất điện (Oát - W), U là hiệu điện thế (Vôn - V), I là cường độ dòng điện (Ampe - A).",
                "diem": 3.5, "diemMotPhan": "Viết đúng P = U.I được 2.0đ; nêu đủ tên và đơn vị các đại lượng được 1.5đ.",
                "yeuCau": "P = U.I; Oát (W), V, A.",
                "loiThuongGap": "Nhầm P = U/I; viết đơn vị W thành Wh.",
                "tuongDuong": "P = I^2 * R hoặc P = U^2 / R."
            },
            {
                "stepId": "S2", "tenBuoc": "Viết công thức điện năng tiêu thụ và đơn vị",
                "noiDung": "Công thức điện năng tiêu thụ: A = P . t = U . I . t. Trong đó: A là điện năng tiêu thụ (Jun - J hoặc kWh), t là thời gian dòng điện chạy qua (giây - s hoặc giờ - h).",
                "diem": 3.5, "diemMotPhan": "Viết đúng A = P.t = U.I.t được 2.0đ; nêu tên và đơn vị (J, kWh) được 1.5đ.",
                "yeuCau": "A = P.t = U.I.t; Jun (J) hoặc kilôoát giờ (kWh).",
                "loiThuongGap": "Quên đại lượng thời gian t.",
                "tuongDuong": "W = P * t."
            },
            {
                "stepId": "S3", "tenBuoc": "Nêu ý nghĩa của số đếm trên công tơ điện",
                "noiDung": "Ý nghĩa: Mỗi số đếm (1 đơn vị số) trên công tơ điện gia đình cho biết lượng điện năng mà gia đình đã tiêu thụ là 1 kilôoát giờ (1 kWh = 1000 W . 3600 s = 3 600 000 J).",
                "diem": 3.0, "diemMotPhan": "Nêu được mỗi số đếm là 1 kWh (hoặc 3.600.000 J) điện năng tiêu thụ được 3.0đ.",
                "yeuCau": "Mỗi số đếm = 1 kWh = 3 600 000 J điện năng tiêu thụ.",
                "loiThuongGap": "Nói số đếm đo công suất điện (sai vì đo điện năng/công).",
                "tuongDuong": "1 số điện = 1 kWh."
            }
        ]
    },
    "TL_C3_L13_Lan": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Giải thích ý nghĩa các thông số định mức",
                "noiDung": "Ý nghĩa các số ghi trên bóng đèn: 220V là hiệu điện thế định mức cần đặt vào hai đầu đèn để đèn hoạt động bình thường; 100W là công suất định mức của đèn khi hoạt động ở đúng hiệu điện thế 220V.",
                "diem": 4.0, "diemMotPhan": "Nêu đúng ý nghĩa 220V định mức được 2.0đ; nêu đúng 100W công suất định mức được 2.0đ.",
                "yeuCau": "220V: hiệu điện thế định mức; 100W: công suất định mức.",
                "loiThuongGap": "Chỉ đọc lại tên đơn vị mà không dùng từ 'định mức'.",
                "tuongDuong": "U định mức = 220V, P định mức = 100W."
            },
            {
                "stepId": "S2", "tenBuoc": "Phân tích công suất khi sử dụng ở hiệu điện thế 110V",
                "noiDung": "Điện trở của đèn không đổi: R = U_dm^2 / P_dm. Công suất thực tế của đèn khi dùng ở hiệu điện thế U: P = U^2 / R. Do U giảm từ 220V xuống 110V (giảm 2 lần), vì P tỉ lệ thuận với bình phương hiệu điện thế (U^2) nên công suất của đèn sẽ giảm đi 2^2 = 4 lần.",
                "diem": 4.0, "diemMotPhan": "Chỉ ra P tỉ lệ với U^2 được 2.0đ; tính ra P giảm 4 lần (còn 25W) được 2.0đ.",
                "yeuCau": "P tỉ lệ thuận U^2 => U giảm 2 lần thì P giảm 4 lần, P' = 25 W.",
                "loiThuongGap": "Nhầm tưởng U giảm 2 lần thì P chỉ giảm 2 lần (thành 50W).",
                "tuongDuong": "P' = 100 / 4 = 25 W."
            },
            {
                "stepId": "S3", "tenBuoc": "Nhận xét độ sáng thực tế và kết luận",
                "noiDung": "Kết luận: Khi dùng ở 110V, công suất của đèn chỉ còn 25W (giảm 4 lần) và bóng đèn sẽ sáng rất mờ (mờ hơn rất nhiều so với bình thường).",
                "diem": 2.0, "diemMotPhan": "Kết luận P' = 25W và đèn sáng rất mờ được 2.0đ.",
                "yeuCau": "P = 25 W, đèn sáng rất mờ.",
                "loiThuongGap": "Quên nhận xét độ sáng của bóng đèn.",
                "tuongDuong": "Đèn sáng yếu/mờ."
            }
        ]
    },
    "TL_C3_L13_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Đổi đơn vị và tính tổng thời gian sử dụng",
                "noiDung": "Đổi công suất ấm điện: P = 1000 W = 1 kW. Thời gian sử dụng mỗi ngày: t = 30 phút = 0,5 giờ. Tổng thời gian ấm hoạt động trong 30 ngày: T = 0,5 . 30 = 15 (giờ).",
                "diem": 4.0, "diemMotPhan": "Đổi P = 1 kW được 1.5đ; tính tổng thời gian T = 15 h được 2.5đ.",
                "yeuCau": "P = 1 kW, T = 15 giờ.",
                "loiThuongGap": "Để nguyên 30 phút nhân 30 = 900 rồi nhân 1000 ra 900.000 mà không đổi giờ.",
                "tuongDuong": "T = 15 h."
            },
            {
                "stepId": "S2", "tenBuoc": "Tính điện năng tiêu thụ theo đơn vị kWh",
                "noiDung": "Vì ấm hoạt động ở đúng hiệu điện thế định mức 220V nên công suất thực tế bằng công suất định mức (1 kW). Điện năng ấm tiêu thụ trong 30 ngày: A = P . T = 1 . 15 = 15 (kWh).",
                "diem": 4.5, "diemMotPhan": "Viết công thức A = P.T được 2.0đ; thay số tính ra đúng 15 kWh được 2.5đ.",
                "yeuCau": "A = 15 kWh.",
                "loiThuongGap": "Tính ra Jun (54 000 000 J) nhưng không đổi sang kWh theo yêu cầu đề bài (trừ 1.5đ).",
                "tuongDuong": "A = 15 số điện."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận đáp số",
                "noiDung": "Đáp số: Điện năng tiêu thụ trong 30 ngày là A = 15 kWh (tương đương 15 số điện).",
                "diem": 1.5, "diemMotPhan": "Ghi đúng đáp số 15 kWh kèm đơn vị được 1.5đ.",
                "yeuCau": "A = 15 kWh.",
                "loiThuongGap": "Thiếu đơn vị kWh.",
                "tuongDuong": "A = 15 kWh."
            }
        ]
    },
    "TL_C3_L13_Ha": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Phát biểu định luật Joule - Lenz",
                "noiDung": "Định luật Joule - Lenz: Nhiệt lượng tỏa ra ở một dây dẫn khi có dòng điện chạy qua tỉ lệ thuận với bình phương cường độ dòng điện, với điện trở của dây dẫn và với thời gian dòng điện chạy qua.",
                "diem": 4.0, "diemMotPhan": "Phát biểu đúng tỉ lệ với I^2, R, t được 4.0đ; thiếu yếu tố 'bình phương' của I được 2.0đ.",
                "yeuCau": "Tỉ lệ thuận với bình phương cường độ dòng điện, điện trở và thời gian.",
                "loiThuongGap": "Quên từ 'bình phương' của cường độ dòng điện.",
                "tuongDuong": "Phát biểu theo SGK Vật lý/KHTN 9 chuẩn."
            },
            {
                "stepId": "S2", "tenBuoc": "Viết hệ thức của định luật và đơn vị",
                "noiDung": "Hệ thức: Q = I^2 . R . t. Trong đó: Q là nhiệt lượng tỏa ra (Jun - J), I là cường độ dòng điện (Ampe - A), R là điện trở (Ôm - Ω), t là thời gian dòng điện chạy qua (giây - s).",
                "diem": 4.0, "diemMotPhan": "Viết đúng Q = I^2.R.t được 2.0đ; nêu đủ tên và đơn vị các đại lượng được 2.0đ.",
                "yeuCau": "Q = I^2 . R . t; Jun (J), A, Ω, s.",
                "loiThuongGap": "Viết Q = I.R.t (quên mũ 2); nhầm đơn vị thời gian là phút.",
                "tuongDuong": "Nếu tính theo calo: Q = 0.24 * I^2 * R * t."
            },
            {
                "stepId": "S3", "tenBuoc": "Ý nghĩa thực tiễn và ứng dụng/phòng tránh",
                "noiDung": "Ý nghĩa thực tiễn: 1) Ứng dụng có ích trong các thiết bị biến điện năng thành nhiệt năng (bàn là, nồi cơm điện, bình nóng lạnh, mỏ hàn); 2) Tác hại cần phòng tránh: gây nóng đường dây truyền tải điện, làm hao phí điện năng và có nguy cơ gây chập cháy hỏa hoạn.",
                "diem": 2.0, "diemMotPhan": "Nêu được ứng dụng làm nóng và biện pháp phòng tránh hao phí nhiệt được 2.0đ.",
                "yeuCau": "Nêu ứng dụng tạo nhiệt và tác hại hao phí nhiệt trên dây dẫn.",
                "loiThuongGap": "Không liên hệ thực tế.",
                "tuongDuong": "Thiết bị đốt nóng và hiện tượng đoản mạch gây cháy."
            }
        ]
    },

    # Bài 14: Cảm ứng điện từ
    "TL_C4_L14_Minh": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Định nghĩa hiện tượng cảm ứng điện từ",
                "noiDung": "Hiện tượng cảm ứng điện từ là hiện tượng xuất hiện dòng điện cảm ứng trong một cuộn dây dẫn kín khi có sự biến thiên của từ thông qua tiết diện của cuộn dây đó (hoặc khi số đường sức từ xuyên qua tiết diện cuộn dây biến thiên).",
                "diem": 5.0, "diemMotPhan": "Nêu được xuất hiện dòng điện cảm ứng trong mạch kín khi từ thông biến thiên được 5.0đ; chỉ nói có nam châm chuyển động được 3.0đ.",
                "yeuCau": "Xuất hiện dòng điện cảm ứng trong cuộn dây kín khi số đường sức từ biến thiên.",
                "loiThuongGap": "Nói dòng điện sinh ra từ trường (đó là tác dụng từ, không phải cảm ứng).",
                "tuongDuong": "Dòng điện cảm ứng xuất hiện khi từ trường qua mạch kín thay đổi."
            },
            {
                "stepId": "S2", "tenBuoc": "Điều kiện xuất hiện dòng điện cảm ứng",
                "noiDung": "Điều kiện cần và đủ: Mạch dẫn phải là MẠCH KÍN và SỐ ĐƯỜNG SỨC TỪ xuyên qua tiết diện của cuộn dây phải BIẾN THIÊN (tăng lên hoặc giảm đi theo thời gian).",
                "diem": 5.0, "diemMotPhan": "Nêu đúng cuộn dây kín được 2.5đ; số đường sức từ biến thiên (tăng hoặc giảm) được 2.5đ.",
                "yeuCau": "Cuộn dây kín và số đường sức từ xuyên qua tiết diện biến thiên.",
                "loiThuongGap": "Chỉ nói có đường sức từ xuyên qua (nếu xuyên qua nhưng không đổi thì không có dòng điện).",
                "tuongDuong": "Từ thông biến thiên qua mạch kín."
            }
        ]
    },
    "TL_C4_L14_Lan": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Cấu tạo chính của đinamô xe đạp",
                "noiDung": "Đinamô xe đạp gồm hai bộ phận chính: 1) Một nam châm vĩnh cửu gắn liền với trục quay và núm xoay; 2) Một cuộn dây dẫn kín quấn quanh lõi sắt non đặt cố định bên cạnh nam châm.",
                "diem": 3.0, "diemMotPhan": "Kể đúng nam châm gắn trục quay và cuộn dây dẫn kín quấn quanh lõi sắt được 3.0đ.",
                "yeuCau": "Nam châm quay gắn núm xoay và cuộn dây dẫn kín cố định.",
                "loiThuongGap": "Quên cuộn dây hoặc nhầm nam châm đứng yên.",
                "tuongDuong": "Rotor là nam châm, stator là cuộn dây."
            },
            {
                "stepId": "S2", "tenBuoc": "Cơ chế biến thiên từ trường khi bánh xe quay",
                "noiDung": "Khi bánh xe đạp quay, núm xoay cọ xát với vành bánh xe làm nam châm quay tròn liên tục. Khi các cực Bắc và Nam của nam châm lần lượt luân phiên lại gần rồi ra xa đầu cuộn dây, số đường sức từ xuyên qua tiết diện cuộn dây liên tục tăng rồi giảm (biến thiên tuần hoàn).",
                "diem": 4.0, "diemMotPhan": "Giải thích nam châm quay làm số đường sức từ qua cuộn dây biến thiên liên tục được 4.0đ.",
                "yeuCau": "Nam châm quay làm số đường sức từ xuyên qua cuộn dây biến thiên liên tục.",
                "loiThuongGap": "Chỉ nói bánh xe quay làm đèn sáng mà không giải thích từ trường.",
                "tuongDuong": "Từ thông qua cuộn dây biến thiên tuần hoàn."
            },
            {
                "stepId": "S3", "tenBuoc": "Xuất hiện dòng điện cảm ứng làm sáng bóng đèn",
                "noiDung": "Theo định luật cảm ứng điện từ, sự biến thiên liên tục của từ trường sinh ra một dòng điện cảm ứng xoay chiều chạy trong cuộn dây dẫn kín và truyền tới làm sáng bóng đèn xe đạp. Bánh xe quay càng nhanh thì dòng điện càng mạnh và đèn càng sáng.",
                "diem": 3.0, "diemMotPhan": "Kết luận sinh ra dòng điện cảm ứng làm sáng đèn được 3.0đ.",
                "yeuCau": "Sinh ra dòng điện cảm ứng xoay chiều thắp sáng bóng đèn.",
                "loiThuongGap": "Không kết luận được dòng điện sinh ra làm sáng đèn.",
                "tuongDuong": "Chuyển hóa cơ năng quay thành điện năng thắp sáng."
            }
        ]
    },
    "TL_C4_L14_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Hệ thức máy biến thế",
                "noiDung": "Áp dụng hệ thức định luật của máy biến thế lý tưởng: U1 / U2 = N1 / N2. Trong đó: U1, U2 lần lượt là hiệu điện thế ở cuộn sơ cấp và thứ cấp; N1, N2 là số vòng dây tương ứng.",
                "diem": 3.5, "diemMotPhan": "Viết đúng U1/U2 = N1/N2 được 3.5đ.",
                "yeuCau": "U1 / U2 = N1 / N2.",
                "loiThuongGap": "Viết lộn U1/U2 = N2/N1.",
                "tuongDuong": "U2 = U1 * (N2 / N1)."
            },
            {
                "stepId": "S2", "tenBuoc": "Thay số và tính hiệu điện thế U2",
                "noiDung": "Suy ra U2 = U1 . (N2 / N1). Với N1 = 4400 vòng, N2 = 240 vòng và U1 = 220 V: U2 = 220 . (240 / 4400) = 220 . (6 / 110) = 2 . 6 = 12 (V).",
                "diem": 4.5, "diemMotPhan": "Biến đổi công thức đúng được 2.0đ; thay số tính ra U2 = 12 V đúng được 2.5đ.",
                "yeuCau": "U2 = 12 V.",
                "loiThuongGap": "Tính nhầm phép chia; sai đơn vị Vôn.",
                "tuongDuong": "U2 = 12 V."
            },
            {
                "stepId": "S3", "tenBuoc": "Xác định loại máy biến thế và kết luận",
                "noiDung": "Vì U2 = 12 V < U1 = 220 V (hoặc N2 < N1), đây là máy BIẾN THẾ HẠ ÁP (máy hạ thế). Đáp số: U2 = 12 V; là máy hạ thế.",
                "diem": 2.0, "diemMotPhan": "Kết luận đúng là máy hạ thế được 2.0đ.",
                "yeuCau": "Kết luận máy hạ áp / hạ thế.",
                "loiThuongGap": "Nhầm thành máy tăng thế.",
                "tuongDuong": "Máy hạ áp."
            }
        ]
    },
    "TL_C4_L14_Ha": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Cách 1: Cho nam châm quay trước cuộn dây cố định",
                "noiDung": "Cách 1 (Rotor là nam châm, Stator là cuộn dây): Đặt cuộn dây dẫn kín cố định, cho nam châm (nam châm vĩnh cửu hoặc nam châm điện) quay tròn xung quanh một trục trước đầu cuộn dây. Khi các cực Bắc - Nam lần lượt tiến lại gần và ra xa, số đường sức từ qua cuộn dây luân phiên tăng giảm, tạo ra dòng điện cảm ứng xoay chiều.",
                "diem": 5.0, "diemMotPhan": "Nêu cuộn dây đứng yên, nam châm quay được 3.0đ; giải thích luân phiên đổi cực sinh dòng xoay chiều được 2.0đ.",
                "yeuCau": "Nam châm quay trước cuộn dây cố định.",
                "loiThuongGap": "Chỉ nói cho nam châm chuyển động mà không nói quay liên tục.",
                "tuongDuong": "Mô hình đinamô hoặc máy phát điện công nghiệp lớn."
            },
            {
                "stepId": "S2", "tenBuoc": "Cách 2: Cho cuộn dây quay trong từ trường của nam châm cố định",
                "noiDung": "Cách 2 (Rotor là cuộn dây, Stator là nam châm): Đặt nam châm cố định để tạo ra từ trường đều giữa hai cực. Cho cuộn dây dẫn kín quay tròn đều quanh một trục vuông góc với các đường sức từ. Trong mỗi nửa vòng quay, số đường sức từ qua cuộn dây luân phiên tăng rồi giảm, tạo ra dòng điện xoay chiều được dẫn ra ngoài qua vành khuyên và chổi quét.",
                "diem": 5.0, "diemMotPhan": "Nêu nam châm cố định, cuộn dây quay được 3.0đ; giải thích biến thiên từ thông tạo dòng xoay chiều được 2.0đ.",
                "yeuCau": "Cuộn dây quay trong từ trường của nam châm đứng yên.",
                "loiThuongGap": "Quên nhắc đến việc cuộn dây phải quay tròn.",
                "tuongDuong": "Mô hình khung dây quay trong từ trường."
            }
        ]
    },

    # Bài 15: Dòng điện xoay chiều
    "TL_C4_L15_Minh": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tác dụng nhiệt và tác dụng quang",
                "noiDung": "1) Tác dụng nhiệt: Dòng điện xoay chiều chạy qua vật dẫn làm vật nóng lên (Ví dụ: Ấm điện, bàn là điện, bình nóng lạnh hoạt động). 2) Tác dụng quang (phát sáng): Dòng điện xoay chiều làm phát sáng các nguồn sáng điện (Ví dụ: Đèn sợi đốt phát sáng, đèn huỳnh quang, đèn LED).",
                "diem": 5.0, "diemMotPhan": "Nêu tác dụng nhiệt + ví dụ được 2.5đ; nêu tác dụng quang + ví dụ được 2.5đ.",
                "yeuCau": "Tác dụng nhiệt (bàn là/bếp điện); Tác dụng phát quang (đèn điện).",
                "loiThuongGap": "Kể tên tác dụng nhưng không cho ví dụ cụ thể.",
                "tuongDuong": "Phát nhiệt và phát sáng."
            },
            {
                "stepId": "S2", "tenBuoc": "Tác dụng từ và tác dụng sinh lý",
                "noiDung": "3) Tác dụng từ: Dòng điện xoay chiều sinh ra từ trường biến thiên (Ví dụ: Cuộn dây hút/đẩy các lá thép, làm quay động cơ quạt điện, máy bơm). 4) Tác dụng sinh lý: Dòng điện xoay chiều đi qua cơ thể sinh vật gây co giật cơ, tim ngừng đập (Ví dụ: Tai nạn điện giật; ứng dụng máy sốc tim trong cấp cứu y tế).",
                "diem": 5.0, "diemMotPhan": "Nêu tác dụng từ + ví dụ được 2.5đ; nêu tác dụng sinh lý + ví dụ được 2.5đ.",
                "yeuCau": "Tác dụng từ (động cơ điện); Tác dụng sinh lý (điện giật, sốc tim).",
                "loiThuongGap": "Thiếu tác dụng sinh lý hoặc tác dụng từ.",
                "tuongDuong": "Từ trường và tác động lên cơ thể sống."
            }
        ]
    },
    "TL_C4_L15_Lan": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Công thức công suất hao phí do tỏa nhiệt trên đường dây",
                "noiDung": "Khi truyền tải một công suất điện P trên đường dây có điện trở R dưới hiệu điện thế U, công suất hao phí do tỏa nhiệt trên đường dây được tính theo công thức: P_hp = R . (P / U)^2 = (R . P^2) / U^2.",
                "diem": 4.0, "diemMotPhan": "Viết đúng công thức P_hp = R . P^2 / U^2 được 4.0đ; viết P_hp = I^2.R được 2.0đ.",
                "yeuCau": "P_hp = (R . P^2) / U^2.",
                "loiThuongGap": "Viết P_hp = U^2 / R (nhầm với công suất tiêu thụ định mức của tải).",
                "tuongDuong": "P_hp = R * I^2 với I = P / U."
            },
            {
                "stepId": "S2", "tenBuoc": "Phân tích mối quan hệ tỉ lệ nghịch với bình phương hiệu điện thế",
                "noiDung": "Từ công thức trên, ta thấy công suất hao phí P_hp tỉ lệ nghịch với bình phương hiệu điện thế U (P_hp ~ 1 / U^2). Do đó, nếu tăng hiệu điện thế U lên n lần thì công suất hao phí do tỏa nhiệt sẽ giảm đi n^2 lần.",
                "diem": 3.5, "diemMotPhan": "Chỉ ra P_hp tỉ lệ nghịch với U^2 (tăng U lên n lần thì hao phí giảm n^2 lần) được 3.5đ.",
                "yeuCau": "P_hp tỉ lệ nghịch với U^2.",
                "loiThuongGap": "Chỉ nói tăng U thì hao phí giảm mà quên nhấn mạnh 'bình phương' (n^2).",
                "tuongDuong": "Hiệu quả giảm hao phí vượt trội khi tăng thế."
            },
            {
                "stepId": "S3", "tenBuoc": "So sánh với biện pháp giảm điện trở R và kết luận",
                "noiDung": "Biện pháp giảm R bằng cách tăng tiết diện dây hoặc dùng bạc/đồng dẫn tới dây quá nặng, tốn kém cột điện và chi phí kim loại màu cực kỳ lớn. Do đó, dùng máy biến thế để tăng hiệu điện thế lên hàng trăm nghìn vôn (đường dây cao thế 110kV, 220kV, 500kV) là phương án kinh tế, an toàn và tối ưu nhất.",
                "diem": 2.5, "diemMotPhan": "Giải thích vì sao tăng U kinh tế hơn giảm R được 2.5đ.",
                "yeuCau": "Tăng U là giải pháp kinh tế và tối ưu nhất so với giảm R.",
                "loiThuongGap": "Không so sánh tính khả thi giữa tăng U và giảm R.",
                "tuongDuong": "Giải pháp kinh tế và kỹ thuật tối ưu."
            }
        ]
    },
    "TL_C4_L15_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Áp dụng công thức tính công suất hao phí",
                "noiDung": "Công thức công suất hao phí do tỏa nhiệt trên đường dây truyền tải: P_hp = (R . P^2) / U^2.",
                "diem": 3.5, "diemMotPhan": "Viết đúng công thức P_hp = R.P^2 / U^2 được 3.5đ.",
                "yeuCau": "P_hp = (R . P^2) / U^2.",
                "loiThuongGap": "Viết lộn U lên tử số.",
                "tuongDuong": "I = P / U => P_hp = I^2 * R."
            },
            {
                "stepId": "S2", "tenBuoc": "Thay số và tính giá trị công suất hao phí",
                "noiDung": "Thay số: R = 10 Ω, P = 500 000 W, U = 25 000 V: P_hp = [10 . (500 000)^2] / (25 000)^2 = 10 . (500 000 / 25 000)^2 = 10 . (20)^2 = 10 . 400 = 4000 (W) = 4 (kW).",
                "diem": 4.5, "diemMotPhan": "Rút gọn tỉ số (500000/25000) = 20 được 2.0đ; tính ra P_hp = 4000 W (4 kW) đúng được 2.5đ.",
                "yeuCau": "P_hp = 4000 W (hoặc 4 kW).",
                "loiThuongGap": "Bấm máy sai số 0 dẫn đến kết quả sai lệch 10 lần hoặc 100 lần.",
                "tuongDuong": "I = 500000 / 25000 = 20 A => P_hp = 20^2 * 10 = 4000 W."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận đáp số",
                "noiDung": "Đáp số: Công suất hao phí do tỏa nhiệt trên đường dây là P_hp = 4000 W (hay 4 kW).",
                "diem": 2.0, "diemMotPhan": "Ghi đúng kết luận 4000 W hoặc 4 kW kèm đơn vị được 2.0đ.",
                "yeuCau": "P_hp = 4000 W hoặc 4 kW.",
                "loiThuongGap": "Thiếu đơn vị W hoặc kW.",
                "tuongDuong": "P_hp = 4 kW."
            }
        ]
    },
    "TL_C4_L15_Ha": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tác dụng từ của dòng điện một chiều",
                "noiDung": "Dòng điện một chiều (DC) có chiều không đổi theo thời gian, do đó từ trường do nó sinh ra có các cực từ (Bắc - Nam) cố định và không đổi chiều. Khi đưa một cực của thanh nam châm lại gần một đầu cuộn dây DC, lực tương tác từ giữa chúng luôn là lực hút hoặc lực đẩy cố định.",
                "diem": 3.5, "diemMotPhan": "Nêu dòng một chiều sinh từ trường cố định, cực từ không đổi được 3.5đ.",
                "yeuCau": "Dòng một chiều: từ trường không đổi chiều, cực từ cố định.",
                "loiThuongGap": "Không nêu tính cố định của cực từ.",
                "tuongDuong": "Từ trường tĩnh/cố định."
            },
            {
                "stepId": "S2", "tenBuoc": "Tác dụng từ của dòng điện xoay chiều",
                "noiDung": "Dòng điện xoay chiều (AC) có chiều và cường độ luân phiên đổi chiều tuần hoàn theo thời gian, do đó từ trường do nó sinh ra cũng liên tục đổi cực từ (Bắc trở thành Nam và ngược lại) theo tần số của dòng điện. Lực từ tác dụng lên cực thanh nam châm luân phiên là lực hút và lực đẩy liên tiếp.",
                "diem": 4.0, "diemMotPhan": "Nêu dòng xoay chiều sinh từ trường biến thiên đổi cực liên tục được 4.0đ.",
                "yeuCau": "Dòng xoay chiều: từ trường biến thiên, các cực từ luân phiên đổi dấu liên tục.",
                "loiThuongGap": "Nghĩ rằng dòng xoay chiều không có tác dụng từ.",
                "tuongDuong": "Từ trường biến thiên tuần hoàn."
            },
            {
                "stepId": "S3", "tenBuoc": "Ứng dụng khác biệt trong thực tế",
                "noiDung": "Hệ quả ứng dụng: Nhờ từ trường biến thiên liên tục, dòng xoay chiều có thể tạo ra từ trường quay làm quay roto động cơ không đồng bộ và dùng được trong máy biến thế, điều mà dòng điện một chiều không làm được.",
                "diem": 2.5, "diemMotPhan": "Nêu được ứng dụng tạo từ trường quay hoặc dùng trong máy biến thế được 2.5đ.",
                "yeuCau": "Ứng dụng trong máy biến thế và động cơ điện xoay chiều.",
                "loiThuongGap": "Thiếu phần liên hệ ứng dụng phân biệt.",
                "tuongDuong": "Hoạt động của máy biến áp."
            }
        ]
    },

    # Bài 16: Vòng năng lượng trên Trái Đất. Năng lượng hoá thạch
    "TL_C5_L16_Minh": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Định nghĩa năng lượng hóa thạch",
                "noiDung": "Năng lượng hóa thạch là dạng năng lượng chứa trong các loại nhiên liệu hóa thạch, được hình thành từ xác của các sinh vật (thực vật, động vật, phiêu sinh vật) bị chôn vùi dưới các tầng địa chất qua hàng trăm triệu năm dưới tác dụng của nhiệt độ và áp suất cao.",
                "diem": 5.0, "diemMotPhan": "Nêu được hình thành từ xác sinh vật cổ đại chôn vùi hàng trăm triệu năm được 5.0đ; chỉ nói khai thác dưới đất được 2.5đ.",
                "yeuCau": "Hình thành từ xác sinh vật cổ đại bị phân hủy và chôn vùi hàng triệu năm.",
                "loiThuongGap": "Nói là khoáng sản vô cơ tự sinh ra.",
                "tuongDuong": "Nhiên liệu hình thành từ trầm tích hữu cơ cổ sinh vật."
            },
            {
                "stepId": "S2", "tenBuoc": "Kể tên 3 loại nhiên liệu hóa thạch phổ biến",
                "noiDung": "Ba loại nhiên liệu hóa thạch phổ biến nhất: 1) Than đá; 2) Dầu mỏ (dầu thô); 3) Khí tự nhiên (khí thiên nhiên).",
                "diem": 5.0, "diemMotPhan": "Kể đúng mỗi loại được 1.67đ (đủ 3 loại than đá, dầu mỏ, khí thiên nhiên được 5.0đ).",
                "yeuCau": "Than đá, dầu mỏ, khí tự nhiên.",
                "loiThuongGap": "Kể củi gỗ, cồn, khí biogas (đây là sinh khối, không phải hóa thạch).",
                "tuongDuong": "Than mỏ, dầu thô, khí đốt tự nhiên."
            }
        ]
    },
    "TL_C5_L16_Lan": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tác hại 1: Gây hiệu ứng nhà kính và biến đổi khí hậu toàn cầu",
                "noiDung": "Khi đốt cháy nhiên liệu hóa thạch (than đá, xăng dầu), một lượng khổng lồ khí CO2 (carbon dioxide) và các khí nhà kính khác được thải vào khí quyển. Các khí này hấp thụ và giữ lại bức xạ nhiệt hồng ngoại từ mặt đất, làm Trái Đất nóng lên toàn cầu, gây băng tan ở hai cực, nước biển dâng và các hiện tượng thời tiết cực đoan (bão lũ, hạn hán khốc liệt).",
                "diem": 5.0, "diemMotPhan": "Nêu phát thải khí CO2 gây hiệu ứng nhà kính và biến đổi khí hậu được 5.0đ; chỉ nói làm nóng Trái Đất được 3.0đ.",
                "yeuCau": "Phát thải CO2 -> Hiệu ứng nhà kính -> Trái Đất nóng lên, biến đổi khí hậu.",
                "loiThuongGap": "Nhầm lẫn hiệu ứng nhà kính với thủng tầng ozon.",
                "tuongDuong": "Làm tăng nồng độ khí nhà kính và biến đổi khí hậu."
            },
            {
                "stepId": "S2", "tenBuoc": "Tác hại 2: Gây ô nhiễm không khí và hiện tượng mưa axit",
                "noiDung": "Khói thải đốt nhiên liệu hóa thạch chứa nhiều hạt bụi mịn nguy hại (PM2.5) và các khí độc hại như SO2 (lưu huỳnh dioxit), NOx (oxit nito). Các khí này phản ứng với hơi nước trong khí quyển tạo thành axit sulfuric (H2SO4) và axit nitric (HNO3) rơi xuống thành mưa axit, hủy hoại rừng cây, axit hóa ao hồ giết chết sinh vật thủy sinh và ăn mòn các công trình xây dựng.",
                "diem": 5.0, "diemMotPhan": "Nêu bụi mịn và SO2, NOx gây ô nhiễm không khí và mưa axit hủy hoại môi trường được 5.0đ.",
                "yeuCau": "Bụi mịn, khí độc SO2/NOx -> Ô nhiễm không khí, mưa axit hủy hoại sinh thái.",
                "loiThuongGap": "Không giải thích được cơ chế sinh ra mưa axit.",
                "tuongDuong": "Gây bệnh đường hô hấp và ăn mòn sinh thái qua mưa axit."
            }
        ]
    },
    "TL_C5_L16_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Đổi đơn vị và tính nhiệt lượng tỏa ra khi đốt than",
                "noiDung": "Đổi khối lượng than đá sang kilôgam: m = 2 tấn = 2000 kg. Nhiệt lượng toàn phần tỏa ra khi đốt cháy hoàn toàn lượng than này: Q = q . m = 27.10^6 . 2000 = 54.10^9 (J) (tức 54 tỷ Jun).",
                "diem": 4.5, "diemMotPhan": "Đổi 2 tấn = 2000 kg được 1.5đ; tính ra Q = 54.10^9 J (hoặc 54 GJ) đúng được 3.0đ.",
                "yeuCau": "Q = 54 . 10^9 J (hoặc 54 GJ).",
                "loiThuongGap": "Để nguyên m = 2 tấn tính ra 54.10^6 J (sai đơn vị 1000 lần).",
                "tuongDuong": "Q = 54 GJ."
            },
            {
                "stepId": "S2", "tenBuoc": "Tính lượng điện năng hữu ích thu được theo hiệu suất",
                "noiDung": "Hiệu suất chuyển hóa của nhà máy nhiệt điện là H = 35% = 0,35. Lượng điện năng hữu ích thu được: A = H . Q = 0,35 . 54.10^9 = 18,9.10^9 (J). Đổi ra đơn vị điện thương phẩm: A = 18,9.10^9 / 3 600 000 = 5250 (kWh).",
                "diem": 4.0, "diemMotPhan": "Viết công thức A = H.Q được 1.5đ; tính ra A = 18.9.10^9 J (hoặc 5250 kWh) đúng được 2.5đ.",
                "yeuCau": "A = 18.9 . 10^9 J (hoặc 5250 kWh).",
                "loiThuongGap": "Lấy Q chia H thay vì nhân H; bấm nhầm số 0.",
                "tuongDuong": "A = 18,9 GJ hoặc 5250 kWh."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận đáp số",
                "noiDung": "Đáp số: Nhiệt lượng tỏa ra Q = 54.10^9 J; Lượng điện năng hữu ích thu được A = 18,9.10^9 J (tương đương 5250 kWh).",
                "diem": 1.5, "diemMotPhan": "Ghi đúng cả hai đáp số kèm đơn vị chuẩn được 1.5đ.",
                "yeuCau": "Q = 54.10^9 J; A = 18.9.10^9 J (5250 kWh).",
                "loiThuongGap": "Thiếu đáp số tổng kết.",
                "tuongDuong": "Q = 54.10^9 J; A = 18,9.10^9 J."
            }
        ]
    },
    "TL_C5_L16_Ha": {
        "soBuoc": 4,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Xác nhận nguồn năng lượng gốc tối thượng",
                "noiDung": "Nguồn gốc sâu xa của hầu hết các dạng năng lượng trên Trái Đất (than đá, dầu mỏ, khí đốt, gió, thủy điện, sinh khối...) chính là NĂNG LƯỢNG MẶT TRỜI (năng lượng bức xạ phát ra từ Mặt Trời).",
                "diem": 2.5, "diemMotPhan": "Khẳng định nguồn gốc là Năng lượng Mặt Trời được 2.5đ.",
                "yeuCau": "Khẳng định nguồn năng lượng gốc là Mặt Trời.",
                "loiThuongGap": "Nói nguồn gốc là lõi Trái Đất hoặc tự nhiên sinh ra.",
                "tuongDuong": "Bức xạ nhiệt và quang từ Mặt Trời."
            },
            {
                "stepId": "S2", "tenBuoc": "Phân tích nguồn gốc năng lượng hóa thạch (than, dầu)",
                "noiDung": "Năng lượng hóa thạch (than đá, dầu mỏ): Hàng trăm triệu năm trước, thực vật và phiêu sinh vật quang hợp hấp thụ năng lượng ánh sáng Mặt Trời để tổng hợp chất hữu cơ (chuyển hóa thành hóa năng). Khi chết đi, xác của chúng bị chôn vùi và biến đổi thành than đá, dầu khí. Bản chất là năng lượng Mặt Trời cổ đại được tích lũy.",
                "diem": 2.5, "diemMotPhan": "Chỉ ra quang hợp tích lũy năng lượng mặt trời cổ đại thành hóa thạch được 2.5đ.",
                "yeuCau": "Sinh vật quang hợp hấp thụ ánh sáng mặt trời -> hóa năng lưu trữ.",
                "loiThuongGap": "Không liên hệ với quá trình quang hợp sinh vật.",
                "tuongDuong": "Năng lượng hóa thạch là năng lượng mặt trời tích lũy trong quá khứ."
            },
            {
                "stepId": "S3", "tenBuoc": "Phân tích nguồn gốc năng lượng gió",
                "noiDung": "Năng lượng gió: Bức xạ nhiệt của Mặt Trời chiếu xuống các vùng bề mặt Trái Đất không đều (xích đạo nhận nhiều nhiệt hơn hai cực, biển nhận nhiệt khác đất liền), tạo ra sự chênh lệch nhiệt độ và áp suất khí quyển. Sự chênh lệch áp suất này làm các khối không khí chuyển động đối lưu sinh ra gió.",
                "diem": 2.5, "diemMotPhan": "Giải thích bức xạ mặt trời hun nóng không đều tạo đối lưu khí quyển sinh ra gió được 2.5đ.",
                "yeuCau": "Bức xạ mặt trời đốt nóng không đều -> chênh lệch áp suất -> gió thổi.",
                "loiThuongGap": "Chỉ nói gió do Trái Đất tự quay sinh ra.",
                "tuongDuong": "Chênh lệch nhiệt độ bề mặt do ánh sáng mặt trời sinh ra dòng đối lưu gió."
            },
            {
                "stepId": "S4", "tenBuoc": "Phân tích nguồn gốc năng lượng thủy điện",
                "noiDung": "Thủy điện: Nhiệt lượng Mặt Trời làm bốc hơi nước từ đại dương và sông ngòi bay lên cao tạo thành mây. Gió thổi mây vào đất liền và ngưng tụ thành mưa rơi xuống vùng núi cao, tạo ra thế năng trọng trường cho dòng nước. Khi nước chảy từ trên đập cao xuống làm quay tuabin máy phát sinh ra điện năng.",
                "diem": 2.5, "diemMotPhan": "Giải thích vòng tuần hoàn nước (bốc hơi nhờ mặt trời, tạo mưa trên núi sinh thế năng) được 2.5đ.",
                "yeuCau": "Vòng tuần hoàn nước nhờ nhiệt mặt trời -> mưa trên cao tạo thế năng dòng chảy.",
                "loiThuongGap": "Nghĩ rằng nước tự nhiên chảy từ đồi núi mà không liên quan Mặt Trời.",
                "tuongDuong": "Chu trình tuần hoàn nước được vận hành bởi nhiệt năng Mặt Trời."
            }
        ]
    },

    # Bài 17: Một số dạng năng lượng tái tạo
    "TL_C5_L17_Minh": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Định nghĩa năng lượng tái tạo",
                "noiDung": "Năng lượng tái tạo là dạng năng lượng có nguồn gốc từ thiên nhiên, liên tục được tái tạo và bổ sung bởi các quá trình tự nhiên, tồn tại vô tận và không bị cạn kiệt theo thời gian sử dụng của con người.",
                "diem": 5.0, "diemMotPhan": "Nêu được nguồn gốc tự nhiên, liên tục được bổ sung và không bị cạn kiệt được 5.0đ; thiếu ý 'không cạn kiệt' được 3.0đ.",
                "yeuCau": "Nguồn gốc tự nhiên, liên tục tái tạo, không bị cạn kiệt.",
                "loiThuongGap": "Nói năng lượng tái sinh là rác tái chế.",
                "tuongDuong": "Năng lượng sạch vô tận từ thiên nhiên."
            },
            {
                "stepId": "S2", "tenBuoc": "Kể tên ít nhất 4 nguồn năng lượng tái tạo",
                "noiDung": "Bốn nguồn năng lượng tái tạo tiêu biểu: 1) Năng lượng mặt trời (quang điện, nhiệt mặt trời); 2) Năng lượng gió; 3) Thủy điện (năng lượng dòng nước); 4) Năng lượng sinh khối (hoặc năng lượng địa nhiệt, năng lượng sóng/thủy triều).",
                "diem": 5.0, "diemMotPhan": "Kể đúng mỗi nguồn được 1.25đ (đủ 4 nguồn được 5.0đ).",
                "yeuCau": "Mặt trời, gió, nước/thủy điện, sinh khối/địa nhiệt/thủy triều.",
                "loiThuongGap": "Kể nhầm than đá, dầu mỏ hoặc hạt nhân urani.",
                "tuongDuong": "Điện mặt trời, điện gió, thủy điện, sinh khối."
            }
        ]
    },
    "TL_C5_L17_Lan": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Ưu điểm 1: Thân thiện với môi trường và không phát thải khí nhà kính",
                "noiDung": "Điện mặt trời và điện gió là nguồn năng lượng sạch tuyệt đối trong quá trình phát điện: Không phát thải khí carbon dioxide (CO2) gây hiệu ứng nhà kính, không phát sinh bụi mịn và các khí độc hại (SO2, NOx), góp phần bảo vệ chất lượng không khí và ứng phó biến đổi khí hậu.",
                "diem": 3.5, "diemMotPhan": "Nêu không phát thải CO2 và khí độc hại, không gây ô nhiễm môi trường được 3.5đ.",
                "yeuCau": "Năng lượng sạch, không phát thải CO2 và khí ô nhiễm.",
                "loiThuongGap": "Chỉ nói 'sạch hơn' chung chung mà không nêu khí phát thải.",
                "tuongDuong": "Giảm phát thải khí nhà kính về 0."
            },
            {
                "stepId": "S2", "tenBuoc": "Ưu điểm 2: Nguồn tài nguyên vô tận và chi phí nhiên liệu bằng không",
                "noiDung": "Năng lượng mặt trời và gió là nguồn tài nguyên thiên nhiên vô tận và hoàn toàn miễn phí, không bao giờ bị cạn kiệt như than đá. Các nhà máy này không tốn chi phí mua nhiên liệu vận hành hàng ngày, giúp tự chủ an ninh năng lượng.",
                "diem": 3.5, "diemMotPhan": "Nêu tài nguyên vô tận và chi phí nhiên liệu bằng không được 3.5đ.",
                "yeuCau": "Tài nguyên vô tận, không tốn chi phí mua nhiên liệu đốt.",
                "loiThuongGap": "Nghĩ rằng vận hành vẫn tốn tiền mua than.",
                "tuongDuong": "Không cạn kiệt, chi phí nhiên liệu bằng không."
            },
            {
                "stepId": "S3", "tenBuoc": "Ưu điểm 3: Linh hoạt vị trí lắp đặt và tận dụng không gian",
                "noiDung": "Điện mặt trời áp mái tận dụng được diện tích mái nhà dân dụng và nhà xưởng mà không chiếm thêm diện tích đất canh tác; điện gió có thể lắp đặt ngoài khơi hoặc trên vùng đất bán khô hạn, giảm thiểu việc chiếm dụng đất đai quý giá.",
                "diem": 3.0, "diemMotPhan": "Nêu tận dụng được diện tích mái nhà và đất cằn cỗi/ngoài khơi được 3.0đ.",
                "yeuCau": "Tận dụng diện tích mái nhà, không tranh chấp đất nông nghiệp.",
                "loiThuongGap": "Không liên hệ đến hình thức điện áp mái.",
                "tuongDuong": "Lắp đặt linh hoạt, phân tán tại chỗ."
            }
        ]
    },
    "TL_C5_L17_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tính tổng công suất bức xạ mặt trời chiếu vào tấm pin",
                "noiDung": "Tổng công suất bức xạ mặt trời chiếu lên bề mặt tấm pin: P_bx = I . S = 1000 . 2 = 2000 (W).",
                "diem": 4.0, "diemMotPhan": "Viết công thức P_bx = I.S được 2.0đ; thay số tính ra 2000 W đúng được 2.0đ.",
                "yeuCau": "P_bx = 2000 W.",
                "loiThuongGap": "Quên nhân diện tích S; sai đơn vị W.",
                "tuongDuong": "Công suất quang chiếu vào = 2 kW."
            },
            {
                "stepId": "S2", "tenBuoc": "Tính công suất điện hữu ích tạo ra",
                "noiDung": "Áp dụng hiệu suất chuyển hóa quang điện H = 20% = 0,2: Công suất điện hữu ích tấm pin sinh ra: P = H . P_bx = 0,20 . 2000 = 400 (W).",
                "diem": 4.5, "diemMotPhan": "Viết P = H.P_bx được 2.0đ; thay số tính ra P = 400 W đúng được 2.5đ.",
                "yeuCau": "P = 400 W.",
                "loiThuongGap": "Lấy 2000 chia 0.2 ra 10000 W; quên đổi % thành số thập phân.",
                "tuongDuong": "P = 0,4 kW."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận đáp số",
                "noiDung": "Đáp số: Công suất điện mà tấm pin tạo ra là P = 400 W (tương đương 0,4 kW).",
                "diem": 1.5, "diemMotPhan": "Ghi đúng kết luận 400 W kèm đơn vị được 1.5đ.",
                "yeuCau": "P = 400 W.",
                "loiThuongGap": "Thiếu đơn vị W.",
                "tuongDuong": "P = 400 W (0,4 kW)."
            }
        ]
    },
    "TL_C5_L17_Ha": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Thách thức 1: Tính chập chờn và phụ thuộc thời tiết",
                "noiDung": "Thách thức 1: Tính gián đoạn và không ổn định cao. Điện mặt trời hoàn toàn không phát điện vào ban đêm và suy giảm mạnh vào ngày mưa bão; điện gió phụ thuộc vào tốc độ gió tự nhiên lúc mạnh lúc lặng. Do đó khó duy trì công suất phát ổn định liên tục theo nhu cầu phụ tải 24/7.",
                "diem": 3.5, "diemMotPhan": "Nêu tính chập chờn, gián đoạn phụ thuộc thời tiết ngày đêm được 3.5đ.",
                "yeuCau": "Tính gián đoạn, phụ thuộc thời tiết, không có nắng ban đêm.",
                "loiThuongGap": "Chỉ nói trời nắng mới có điện mà không gắn với tính ổn định lưới điện.",
                "tuongDuong": "Không đảm bảo công suất nền liên tục."
            },
            {
                "stepId": "S2", "tenBuoc": "Thách thức 2: Chi phí công nghệ tích trữ điện năng quy mô lớn",
                "noiDung": "Thách thức 2: Công nghệ lưu trữ điện quy mô lưới (hệ thống pin tích điện BESS, thủy điện tích năng) hiện nay có chi phí đầu tư vô cùng đắt đỏ và tuổi thọ pin hữu hạn. Chưa có giải pháp lưu trữ điện khổng lồ giá rẻ để dự trữ nguồn điện dư thừa dùng khi thiếu nắng, gió.",
                "diem": 3.5, "diemMotPhan": "Nêu khó khăn và chi phí cực lớn của hệ thống lưu trữ điện năng quy mô lớn được 3.5đ.",
                "yeuCau": "Công nghệ tích trữ điện năng quy mô lớn quá đắt đỏ.",
                "loiThuongGap": "Nghĩ rằng điện phát ra cất vào ắc quy xe máy là xong.",
                "tuongDuong": "Hệ thống lưu trữ năng lượng (BESS) chi phí cao."
            },
            {
                "stepId": "S3", "tenBuoc": "Thách thức 3: Chi phí hạ tầng truyền tải và nhu cầu đất đai",
                "noiDung": "Thách thức 3: Các cánh đồng điện gió, điện mặt trời thường nằm xa trung tâm tiêu thụ, đòi hỏi vốn đầu tư khổng lồ để nâng cấp lưới điện truyền tải thông minh và cần diện tích quỹ đất/mặt biển rất lớn để bố trí tuabin và tấm pin.",
                "diem": 3.0, "diemMotPhan": "Nêu chi phí đầu tư lưới điện truyền tải và diện tích mặt bằng lớn được 3.0đ.",
                "yeuCau": "Chi phí đầu tư hạ tầng lưới điện truyền tải và diện tích đất đai lớn.",
                "loiThuongGap": "Không nhận ra khoảng cách giữa nguồn phát và nơi tiêu thụ.",
                "tuongDuong": "Hạ tầng truyền tải điện và mật độ năng lượng thấp."
            }
        ]
    }
}
print("Loaded Part 3 template definitions:", len(OLD_64_STEP_DATA_P3))
