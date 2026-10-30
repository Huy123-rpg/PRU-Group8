# Data definitions for the 64 old essay questions
# Chapters I & II (Questions 1 to 36)

OLD_64_STEP_DATA = {
    # Bài 2: Động năng. Thế năng
    "TL_C1_L2_Minh": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Khái niệm và biểu thức Động năng",
                "noiDung": "Động năng là dạng năng lượng vật có được do đang chuyển động: Wđ = 1/2 . m . v^2. Đơn vị: Wđ (J), m (kg), v (m/s).",
                "diem": 5.0, "diemMotPhan": "Nêu đúng định nghĩa được 2.5đ; viết đúng công thức và đơn vị được 2.5đ.",
                "yeuCau": "Định nghĩa gắn với chuyển động; công thức Wđ = 1/2 m v^2; đơn vị Jun (J).",
                "loiThuongGap": "Quên bình phương vận tốc; nhầm đơn vị khối lượng sang gam; nhầm với thế năng.",
                "tuongDuong": "Chấp nhận kí hiệu Ed hoặc Wd."
            },
            {
                "stepId": "S2", "tenBuoc": "Khái niệm và biểu thức Thế năng trọng trường",
                "noiDung": "Thế năng trọng trường là dạng năng lượng vật có được do vị trí ở một độ cao so với mốc thế năng: Wt = m . g . h. Đơn vị: Wt (J), m (kg), h (m), g (m/s^2).",
                "diem": 5.0, "diemMotPhan": "Nêu đúng định nghĩa được 2.5đ; viết đúng công thức và đơn vị được 2.5đ.",
                "yeuCau": "Định nghĩa gắn với độ cao/vị trí so với mốc; công thức Wt = mgh; đơn vị Jun (J).",
                "loiThuongGap": "Quên nhắc đến mốc tính thế năng (mặt đất); nhầm h là quãng đường.",
                "tuongDuong": "Chấp nhận kí hiệu Ep hoặc Wt."
            }
        ]
    },
    "TL_C1_L2_Lan": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Cơ sở lý thuyết động năng",
                "noiDung": "Động năng của một vật chuyển động tỉ lệ thuận với khối lượng và bình phương vận tốc: Wđ = 1/2 . m . v^2.",
                "diem": 3.0, "diemMotPhan": "Nêu đúng sự phụ thuộc vào m và v nhưng thiếu bình phương vận tốc được 1.5đ.",
                "yeuCau": "Chỉ ra động năng phụ thuộc vào khối lượng m và vận tốc v^2.",
                "loiThuongGap": "Quên bình phương của vận tốc; chỉ nói xe to hơn xe nhỏ mà không gắn với động năng.",
                "tuongDuong": "Diễn đạt bằng lời hoặc viết công thức đều đạt điểm tối đa."
            },
            {
                "stepId": "S2", "tenBuoc": "Phân tích đặc điểm xe tải so với xe máy",
                "noiDung": "Xe tải chở nặng có khối lượng m rất lớn và khi chạy ở tốc độ cao, bình phương vận tốc v^2 làm cho động năng của xe tải đạt giá trị khổng lồ, gấp hàng chục đến hàng trăm lần xe máy chạy chậm.",
                "diem": 4.0, "diemMotPhan": "Chỉ so sánh khối lượng mà bỏ qua tốc độ được 2.0đ; so sánh cả hai nhưng chưa định lượng được 3.0đ.",
                "yeuCau": "So sánh khối lượng xe tải >> xe máy và tốc độ cao -> Động năng xe tải cực lớn.",
                "loiThuongGap": "Chỉ nói xe tải nặng hơn mà không phân tích yếu tố tốc độ cao.",
                "tuongDuong": "Có thể dùng ví dụ số liệu giả định để so sánh."
            },
            {
                "stepId": "S3", "tenBuoc": "Giải thích tác hại va chạm và kết luận",
                "noiDung": "Khi va chạm xảy ra, toàn bộ động năng khổng lồ này chuyển hóa thành công phá hủy, làm biến dạng mạnh khung xe, hư hỏng công trình và gây thương vong cực kỳ nghiêm trọng.",
                "diem": 3.0, "diemMotPhan": "Nêu được va chạm sinh công phá hủy nhưng thiếu liên hệ thực tế được 1.5đ.",
                "yeuCau": "Liên hệ động năng chuyển hóa thành công phá hủy/biến dạng khi va chạm.",
                "loiThuongGap": "Chỉ kết luận xe tải nguy hiểm mà không gắn với sự chuyển hóa năng lượng.",
                "tuongDuong": "Chấp nhận giải thích theo xung lượng hoặc quán tính kết hợp năng lượng."
            }
        ]
    },
    "TL_C1_L2_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Đổi đơn vị và tính động năng ban đầu",
                "noiDung": "Đổi m = 200g = 0,2 kg. Áp dụng công thức: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 (J).",
                "diem": 4.0, "diemMotPhan": "Đổi đơn vị đúng được 1.0đ; viết công thức đúng được 1.0đ; tính ra kết quả 10 J đúng được 2.0đ (sai đơn vị trừ 0.5đ).",
                "yeuCau": "Đổi m = 0.2 kg; Wđ = 10 J.",
                "loiThuongGap": "Để nguyên 200g tính ra 10000 J; quên bình phương 10^2.",
                "tuongDuong": "Tính gộp Wđ = 0.5 * 0.2 * 100 = 10 J."
            },
            {
                "stepId": "S2", "tenBuoc": "Lập luận tỉ lệ khi vận tốc tăng gấp đôi",
                "noiDung": "Khi vận tốc tăng gấp đôi (v' = 2v), động năng mới là Wđ' = 1/2 . m . (2v)^2 = 1/2 . m . 4v^2 = 4 . Wđ.",
                "diem": 4.0, "diemMotPhan": "Nêu v tăng 2 lần nên v^2 tăng 4 lần được 2.0đ; lập luận đầy đủ biểu thức được 4.0đ.",
                "yeuCau": "Lập luận v' = 2v => Wđ' = 4 Wđ (hoặc tính trực tiếp Wđ' = 1/2 * 0.2 * 20^2 = 40 J).",
                "loiThuongGap": "Nhầm tưởng v tăng 2 lần thì động năng tăng 2 lần.",
                "tuongDuong": "Tính số trực tiếp: v' = 20 m/s => Wđ' = 40 J = 4 * 10 J."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận và đáp số",
                "noiDung": "Đáp số: Động năng ban đầu là 10 J; khi vận tốc tăng gấp đôi thì động năng tăng lên 4 lần.",
                "diem": 2.0, "diemMotPhan": "Trả lời đúng 1 trong 2 ý được 1.0đ.",
                "yeuCau": "Kết luận rõ ràng cả 2 ý của đề bài.",
                "loiThuongGap": "Quên không ghi đáp số kết luận.",
                "tuongDuong": "Ghi Wđ = 10 J và tăng 4 lần."
            }
        ]
    },
    "TL_C1_L2_Ha": {
        "soBuoc": 4,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Trường hợp a: Phân tích thế năng khi không có sức cản",
                "noiDung": "Khi bỏ qua sức cản: Giọt mưa rơi từ đám mây cao xuống đất, độ cao h giảm liên tục nên thế năng trọng trường Wt = m.g.h giảm dần.",
                "diem": 2.5, "diemMotPhan": "Nêu độ cao giảm nên thế năng giảm được 2.5đ.",
                "yeuCau": "Xác định thế năng giảm do độ cao giảm.",
                "loiThuongGap": "Nhầm thế năng tăng.",
                "tuongDuong": "Giải thích Wt = mgh với h giảm."
            },
            {
                "stepId": "S2", "tenBuoc": "Trường hợp a: Phân tích động năng và bảo toàn cơ năng",
                "noiDung": "Do cơ năng bảo toàn (W = Wđ + Wt = const), toàn bộ thế năng giảm đi chuyển hóa hoàn toàn thành động năng, làm vận tốc giọt mưa tăng liên tục (rơi tự do nhanh dần).",
                "diem": 2.5, "diemMotPhan": "Nêu động năng tăng được 1.5đ; nêu cơ năng bảo toàn được 1.0đ.",
                "yeuCau": "Động năng tăng liên tục do thế năng chuyển hóa thành động năng.",
                "loiThuongGap": "Quên nhắc đến định luật bảo toàn cơ năng.",
                "tuongDuong": "Viết hệ thức bảo toàn Wđ1 + Wt1 = Wđ2 + Wt2."
            },
            {
                "stepId": "S3", "tenBuoc": "Trường hợp b: Chuyển hóa thế năng khi có sức cản",
                "noiDung": "Khi có sức cản không khí: Lực cản sinh công âm, do đó thế năng giảm đi không chỉ chuyển thành động năng mà một phần đáng kể chuyển hóa thành nhiệt năng làm nóng giọt nước và không khí xung quanh.",
                "diem": 2.5, "diemMotPhan": "Nêu thế năng giảm biến thành nhiệt năng được 2.5đ.",
                "yeuCau": "Cơ năng hao phí thành nhiệt năng do lực ma sát cản trở.",
                "loiThuongGap": "Vẫn cho rằng cơ năng bảo toàn.",
                "tuongDuong": "Nêu W giảm = công lực cản = nhiệt năng Q."
            },
            {
                "stepId": "S4", "tenBuoc": "Trường hợp b: Trạng thái rơi đều ổn định",
                "noiDung": "Khi lực cản tăng bằng trọng lực (Fc = P), lực tổng hợp bằng 0, giọt mưa rơi với vận tốc không đổi (rơi đều). Lúc này động năng không đổi, thế năng tiếp tục giảm để chuyển hóa hoàn toàn thành nhiệt năng.",
                "diem": 2.5, "diemMotPhan": "Nêu được giai đoạn rơi đều được 1.5đ; giải thích động năng không đổi được 1.0đ.",
                "yeuCau": "Phân tích giai đoạn cân bằng lực Fc = P => rơi đều, động năng không đổi.",
                "loiThuongGap": "Cho rằng giọt mưa tăng tốc mãi mãi.",
                "tuongDuong": "Mô tả vận tốc giới hạn (terminal velocity)."
            }
        ]
    },

    # Bài 3: Cơ năng
    "TL_C1_L3_Minh": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Phát biểu định luật bảo toàn cơ năng",
                "noiDung": "Phát biểu: Khi một vật chuyển động trong trọng trường chỉ chịu tác dụng của trọng lực (bỏ qua ma sát và lực cản), cơ năng của vật là một đại lượng bảo toàn (không đổi theo thời gian).",
                "diem": 5.0, "diemMotPhan": "Thiếu điều kiện 'chỉ chịu tác dụng của trọng lực' hoặc 'bỏ qua ma sát' trừ 1.5đ.",
                "yeuCau": "Đầy đủ điều kiện bỏ qua ma sát/lực cản và tính không đổi của cơ năng.",
                "loiThuongGap": "Quên điều kiện không có ma sát; chỉ nói năng lượng bảo toàn chung chung.",
                "tuongDuong": "Phát biểu theo SGK KHTN 9 chuẩn."
            },
            {
                "stepId": "S2", "tenBuoc": "Viết hệ thức cơ năng toàn phần",
                "noiDung": "Hệ thức: W = Wđ + Wt = 1/2 . m . v^2 + m . g . h = hằng số. Trong đó Wđ là động năng, Wt là thế năng trọng trường, W là cơ năng (J).",
                "diem": 5.0, "diemMotPhan": "Viết đúng W = Wđ + Wt = hằng số được 2.5đ; khai triển 1/2mv^2 + mgh và đơn vị được 2.5đ.",
                "yeuCau": "Hệ thức W = Wđ + Wt = const, nêu rõ các đại lượng và đơn vị Jun.",
                "loiThuongGap": "Viết W = Wđ - Wt; quên ghi đơn vị.",
                "tuongDuong": "W1 = W2 <=> Wđ1 + Wt1 = Wđ2 + Wt2."
            }
        ]
    },
    "TL_C1_L3_Lan": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Phân tích cơ năng tại vị trí biên (cao nhất)",
                "noiDung": "Tại hai vị trí biên (cao nhất): Vận tốc v = 0 nên động năng bằng 0 (Wđ = 0). Độ cao h đạt cực đại nên thế năng đạt giá trị cực đại (Wt max). Toàn bộ cơ năng bằng thế năng cực đại: W = Wt max.",
                "diem": 3.5, "diemMotPhan": "Nêu v=0 nên Wđ=0 được 1.5đ; nêu h max nên Wt max được 2.0đ.",
                "yeuCau": "Xác định tại biên: v=0, Wđ=0, Wt cực đại.",
                "loiThuongGap": "Nghĩ rằng tại biên vận tốc đạt cực đại.",
                "tuongDuong": "Mô tả điểm dừng tạm thời khi đổi chiều chuyển động."
            },
            {
                "stepId": "S2", "tenBuoc": "Phân tích cơ năng tại vị trí cân bằng (thấp nhất)",
                "noiDung": "Khi chuyển động từ biên về vị trí cân bằng: Độ cao giảm nên thế năng giảm, chuyển hóa thành động năng làm tốc độ tăng dần. Tại vị trí cân bằng (thấp nhất, h = 0): Thế năng bằng 0, tốc độ cực đại nên động năng cực đại: W = Wđ max.",
                "diem": 3.5, "diemMotPhan": "Nêu thế năng chuyển thành động năng được 1.5đ; xác định tại VTCB Wt=0 và Wđ max được 2.0đ.",
                "yeuCau": "Xác định tại VTCB: h=0 (hoặc thấp nhất), Wt=0, Wđ cực đại.",
                "loiThuongGap": "Nhầm vị trí cân bằng là thế năng cực đại.",
                "tuongDuong": "Giải thích theo sự giảm độ cao tăng vận tốc."
            },
            {
                "stepId": "S3", "tenBuoc": "Khái quát quá trình chuyển hóa tuần hoàn",
                "noiDung": "Sau đó, khi vật đi từ vị trí cân bằng lên biên đối diện, động năng lại chuyển hóa ngược lại thành thế năng. Quá trình chuyển hóa qua lại giữa động năng và thế năng lặp đi lặp lại tuần hoàn.",
                "diem": 3.0, "diemMotPhan": "Nêu được sự chuyển hóa ngược lại và tính tuần hoàn được 3.0đ.",
                "yeuCau": "Khẳng định sự chuyển hóa qua lại liên tục Wđ <-> Wt.",
                "loiThuongGap": "Chỉ mô tả nửa chu kỳ đi xuống mà không mô tả nửa chu kỳ đi lên.",
                "tuongDuong": "Nếu có nhắc đến ma sát làm tắt dần trong thực tế thì khuyến khích cộng điểm."
            }
        ]
    },
    "TL_C1_L3_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tính cơ năng của vật tại vị trí thả",
                "noiDung": "Chọn mốc thế năng tại mặt đất. Tại vị trí thả rơi (h = 20 m), vật thả rơi tự do nên v = 0 => Wđ = 0. Cơ năng của vật: W = Wt = m . g . h = 1 . 10 . 20 = 200 (J).",
                "diem": 4.0, "diemMotPhan": "Chọn mốc thế năng và viết công thức W = mgh được 2.0đ; thay số tính ra 200 J được 2.0đ.",
                "yeuCau": "W = 200 J.",
                "loiThuongGap": "Quên mốc thế năng; sai đơn vị.",
                "tuongDuong": "Tính Wt = 200 J, suy ra W = 200 J."
            },
            {
                "stepId": "S2", "tenBuoc": "Áp dụng định luật bảo toàn cơ năng",
                "noiDung": "Bỏ qua ma sát và lực cản, cơ năng được bảo toàn. Ngay trước khi chạm đất (h = 0), toàn bộ thế năng chuyển hóa thành động năng: W = Wđ = 1/2 . m . v^2 = 200 (J).",
                "diem": 3.0, "diemMotPhan": "Nêu điều kiện bảo toàn và thiết lập 1/2 m v^2 = W được 3.0đ.",
                "yeuCau": "Thiết lập Wđ chạm đất = W = 200 J.",
                "loiThuongGap": "Dùng công thức rơi tự do lớp 10 v = sqrt(2gh) mà không giải thích qua cơ năng.",
                "tuongDuong": "Chấp nhận cả hai cách: bảo toàn cơ năng hoặc v = sqrt(2gh) nhưng ưu tiên cơ năng."
            },
            {
                "stepId": "S3", "tenBuoc": "Tính vận tốc chạm đất và kết luận",
                "noiDung": "Suy ra v^2 = (2 . W) / m = (2 . 200) / 1 = 400 => v = √400 = 20 (m/s). Đáp số: Cơ năng W = 200 J; vận tốc chạm đất v = 20 m/s.",
                "diem": 3.0, "diemMotPhan": "Tính ra v = 20 m/s đúng được 2.0đ; kết luận đáp số đầy đủ được 1.0đ.",
                "yeuCau": "v = 20 m/s, đáp số đầy đủ cả 2 yêu cầu.",
                "loiThuongGap": "Quên lấy căn bậc hai (để 400 m/s); thiếu đơn vị m/s.",
                "tuongDuong": "Ghi v = 20 m/s."
            }
        ]
    },
    "TL_C1_L3_Ha": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tích lũy thế năng ở đỉnh dốc đầu tiên",
                "noiDung": "Động cơ chỉ kéo đoàn tàu lên đỉnh dốc cao nhất ban đầu nhằm cung cấp công cơ học, tích lũy cho đoàn tàu một lượng thế năng trọng trường cực đại (Wt max = m.g.h_max).",
                "diem": 3.5, "diemMotPhan": "Nêu động cơ kéo lên để tích lũy thế năng lớn nhất được 3.5đ.",
                "yeuCau": "Đỉnh dốc cao nhất là nơi tích lũy toàn bộ cơ năng/thế năng cực đại cho cả hành trình.",
                "loiThuongGap": "Chỉ nói động cơ kéo lên để thả rơi mà không gắn với thế năng.",
                "tuongDuong": "Nêu W = Wt_max."
            },
            {
                "stepId": "S2", "tenBuoc": "Cơ chế chuyển hóa qua lại giữa thế năng và động năng",
                "noiDung": "Khi thả trôi từ đỉnh dốc, tàu chạy hoàn toàn nhờ trọng lực: khi xuống dốc, thế năng chuyển thành động năng làm tàu tăng tốc; khi lên dốc nhỏ kế tiếp, động năng chuyển ngược thành thế năng.",
                "diem": 3.5, "diemMotPhan": "Phân tích xuống dốc tăng tốc và lên dốc chuyển hóa ngược lại được 3.5đ.",
                "yeuCau": "Mô tả quá trình chuyển hóa liên tục Wt <-> Wđ dọc hành trình.",
                "loiThuongGap": "Nghĩ rằng tàu chạy do quán tính thuần túy mà không nhắc đến thế năng.",
                "tuongDuong": "Giải thích theo định luật bảo toàn cơ năng."
            },
            {
                "stepId": "S3", "tenBuoc": "Quy luật thiết kế các dốc kế tiếp và kết luận",
                "noiDung": "Do một phần cơ năng bị tiêu hao vì ma sát và lực cản không khí, các đỉnh dốc hoặc vòng lượn kế tiếp luôn phải được thiết kế thấp dần so với đỉnh ban đầu. Nhờ đó cơ năng tích lũy ban đầu đủ để tàu hoàn thành hành trình mà không cần động cơ.",
                "diem": 3.0, "diemMotPhan": "Chỉ ra các dốc sau phải thấp hơn dốc đầu do ma sát được 3.0đ.",
                "yeuCau": "Giải thích vì sao các đỉnh dốc sau luôn thấp hơn đỉnh đầu tiên.",
                "loiThuongGap": "Quên không nhắc đến lực cản ma sát thực tế.",
                "tuongDuong": "Nêu W sau < W trước do hao phí nhiệt."
            }
        ]
    },

    # Bài 4: Công và công suất
    "TL_C1_L4_Minh": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Nêu điều kiện để có công cơ học",
                "noiDung": "Điều kiện có công cơ học: Phải có lực tác dụng vào vật và làm vật dịch chuyển một quãng đường theo phương không vuông góc với lực (hoặc có thành phần theo phương dịch chuyển).",
                "diem": 3.0, "diemMotPhan": "Nêu có lực và có dịch chuyển được 3.0đ; thiếu 1 trong 2 ý được 1.5đ.",
                "yeuCau": "Đủ 2 điều kiện: Có lực F tác dụng và có độ dịch chuyển s.",
                "loiThuongGap": "Chỉ nêu có lực tác dụng mà không nêu vật dịch chuyển.",
                "tuongDuong": "Diễn đạt đúng theo SGK KHTN 9."
            },
            {
                "stepId": "S2", "tenBuoc": "Viết công thức tính công cơ học và đơn vị",
                "noiDung": "Công thức công cơ học: A = F . s. Trong đó: A là công cơ học (Jun - J), F là lực tác dụng theo phương chuyển động (Niutơn - N), s là quãng đường dịch chuyển (mét - m).",
                "diem": 3.5, "diemMotPhan": "Viết đúng A = F.s được 2.0đ; nêu đúng tên và đơn vị các đại lượng được 1.5đ.",
                "yeuCau": "A = F.s; Jun (J), N, m.",
                "loiThuongGap": "Nhầm đơn vị công sang Oát; ghi thiếu đơn vị.",
                "tuongDuong": "A = F * s * cos(alpha) nếu viết công thức tổng quát vẫn cho điểm tối đa."
            },
            {
                "stepId": "S3", "tenBuoc": "Viết công thức tính công suất và đơn vị",
                "noiDung": "Công thức tính công suất: P = A / t. Trong đó: P là công suất (Oát - W), A là công thực hiện (J), t là thời gian thực hiện công (giây - s).",
                "diem": 3.5, "diemMotPhan": "Viết đúng P = A/t được 2.0đ; nêu đúng tên và đơn vị các đại lượng được 1.5đ.",
                "yeuCau": "P = A/t; Oát (W), J, s.",
                "loiThuongGap": "Nhầm công suất P với trọng lượng P; nhầm đơn vị W sang J.",
                "tuongDuong": "P = F * v."
            }
        ]
    },
    "TL_C1_L4_Lan": {
        "soBuoc": 2,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Phân tích trường hợp a: Lực sĩ giữ tạ đứng yên",
                "noiDung": "Trường hợp a: KHÔNG có công cơ học. Giải thích: Mặc dù lực sĩ tác dụng một lực nâng rất lớn lên quả tạ, nhưng quả tạ đứng yên, không có sự dịch chuyển (quãng đường s = 0), do đó công cơ học A = F . s = 0.",
                "diem": 5.0, "diemMotPhan": "Khẳng định KHÔNG có công được 2.0đ; giải thích do s = 0 được 3.0đ.",
                "yeuCau": "Khẳng định không có công và giải thích do quãng đường dịch chuyển s = 0.",
                "loiThuongGap": "Nghĩ rằng người mệt mỏi tức là có công cơ học.",
                "tuongDuong": "Giải thích A = F * s = F * 0 = 0."
            },
            {
                "stepId": "S2", "tenBuoc": "Phân tích trường hợp b: Cần cẩu nâng khối bê tông",
                "noiDung": "Trường hợp b: CÓ công cơ học. Giải thích: Cần cẩu tác dụng lực nâng F hướng lên và làm cho khối bê tông dịch chuyển một quãng đường h đi lên theo phương của lực (s > 0), do đó có công cơ học A = F . h > 0.",
                "diem": 5.0, "diemMotPhan": "Khẳng định CÓ công được 2.0đ; giải thích có lực và có dịch chuyển theo phương lực được 3.0đ.",
                "yeuCau": "Khẳng định có công và chỉ ra vừa có lực nâng vừa có dịch chuyển hướng lên.",
                "loiThuongGap": "Chỉ trả lời Có mà không giải thích.",
                "tuongDuong": "A = P * h > 0."
            }
        ]
    },
    "TL_C1_L4_Duy": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "Tính công cơ học thực hiện để nâng hàng",
                "noiDung": "Để kéo đều thùng hàng lên cao, lực kéo của động cơ cân bằng với trọng lượng: F = P = 1200 N. Công cơ học thực hiện: A = F . h = 1200 . 6 = 7200 (J).",
                "diem": 4.5, "diemMotPhan": "Viết công thức A = F.s = P.h được 2.0đ; thay số tính ra 7200 J đúng được 2.5đ.",
                "yeuCau": "A = 7200 J.",
                "loiThuongGap": "Quên nhân chiều cao; nhầm đơn vị thành kJ mà không đổi.",
                "tuongDuong": "A = 7,2 kJ."
            },
            {
                "stepId": "S2", "tenBuoc": "Tính công suất của động cơ",
                "noiDung": "Áp dụng công thức tính công suất: P_cs = A / t = 7200 / 30 = 240 (W).",
                "diem": 4.0, "diemMotPhan": "Viết công thức P = A/t được 2.0đ; thay số tính ra 240 W đúng được 2.0đ.",
                "yeuCau": "P = 240 W.",
                "loiThuongGap": "Lấy A nhân với t; nhầm đơn vị W sang J.",
                "tuongDuong": "P = 0,24 kW."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận đáp số",
                "noiDung": "Đáp số: Công cơ học A = 7200 J; Công suất của động cơ P = 240 W.",
                "diem": 1.5, "diemMotPhan": "Ghi đúng cả 2 đáp số kèm đơn vị chuẩn được 1.5đ.",
                "yeuCau": "Đáp số A = 7200 J; P = 240 W.",
                "loiThuongGap": "Thiếu đáp số tổng kết hoặc sai đơn vị.",
                "tuongDuong": "Ghi A = 7200 J, P = 240 W."
            }
        ]
    },
    "TL_C1_L4_Ha": {
        "soBuoc": 3,
        "steps": [
            {
                "stepId": "S1", "tenBuoc": "So sánh công cơ học của hai máy nâng",
                "noiDung": "Hai máy nâng cùng nâng đều một vật nặng (cùng trọng lượng P) lên cùng một độ cao h, do đó lực nâng F = P và quãng đường s = h là như nhau. Công cơ học hai máy thực hiện bằng nhau: A_A = A_B = P . h.",
                "diem": 3.5, "diemMotPhan": "Khẳng định A_A = A_B được 2.0đ; lập luận do cùng lực và cùng độ cao được 1.5đ.",
                "yeuCau": "A_A = A_B vì cùng F và cùng h.",
                "loiThuongGap": "Nghĩ rằng máy nhanh hơn thì tốn nhiều công hơn.",
                "tuongDuong": "A1 = A2."
            },
            {
                "stepId": "S2", "tenBuoc": "So sánh công suất của hai máy nâng",
                "noiDung": "Công suất được xác định bằng công thực hiện trong 1 đơn vị thời gian: P = A / t. Do A_A = A_B nên công suất tỉ lệ nghịch với thời gian hoàn thành. Ta có: P_A / P_B = t_B / t_A = 25 / 10 = 2,5 lần.",
                "diem": 4.5, "diemMotPhan": "Chỉ ra P tỉ lệ nghịch với t được 2.0đ; lập tỉ số P_A / P_B = 25/10 = 2.5 được 2.5đ.",
                "yeuCau": "P_A / P_B = 2.5 lần (hoặc máy A có công suất gấp 2.5 lần máy B).",
                "loiThuongGap": "Lấy 10/25 = 0.4 kết luận máy A nhỏ hơn máy B.",
                "tuongDuong": "P_A = 2,5 P_B."
            },
            {
                "stepId": "S3", "tenBuoc": "Kết luận và ý nghĩa vật lý",
                "noiDung": "Kết luận: Hai máy thực hiện công như nhau, nhưng máy A có công suất gấp 2,5 lần máy B (máy A làm việc nhanh hơn và khỏe hơn).",
                "diem": 2.0, "diemMotPhan": "Nêu kết luận rõ ràng được 2.0đ.",
                "yeuCau": "Kết luận đúng cả 2 ý: Công bằng nhau, công suất máy A gấp 2.5 lần máy B.",
                "loiThuongGap": "Thiếu phần kết luận tổng hợp.",
                "tuongDuong": "Tóm tắt: A_A = A_B; P_A = 2.5 P_B."
            }
        ]
    }
}
print("Loaded Part 1 template definitions:", len(OLD_64_STEP_DATA))
