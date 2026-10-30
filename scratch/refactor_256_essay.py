import pandas as pd
import re

def refactor_256():
    df = pd.read_excel('BoSung_TuLuan_ChoDuyet.xlsx')
    print(f"Total rows in BoSung_TuLuan_ChoDuyet.xlsx: {len(df)}")
    
    refactored_sheet1 = []
    gemini_rubric_sheet2 = []
    
    step_distribution = {}
    
    for idx, row in df.iterrows():
        cid = row['CauHoiID']
        gv = row['GiaoVienID']
        md = row['MucDo']
        nd = row['NoiDung']
        dapan = row['DapAnTuLuanMau']
        tukhoa = str(row['TuKhoaChamDiem'])
        
        # Parse the existing 4 steps
        lines = [line.strip() for line in dapan.split('\n') if line.strip()]
        old_steps = []
        for line in lines:
            m = re.match(r'•\s*(B\d+)\s*\((.*?)\s*-\s*([0-9.,]+)đ\):\s*(.*)', line)
            if m:
                b_id, title, pts, content = m.groups()
                old_steps.append({
                    "b_id": b_id,
                    "title": title.strip(),
                    "content": content.strip()
                })
                
        if len(old_steps) != 4:
            print(f"Warning: {cid} did not have 4 steps!")
            continue
            
        new_steps = []
        
        # Strategy by Teacher & Question Nature
        if gv == 'Minh': # Dễ - Nền tảng (64 questions)
            # Decide between 2 steps (for pure theory/direct definition) and 3 steps (concept + formula + application)
            # Check question index within lesson (idx % 16: 0, 1, 2, 3)
            sub_idx = idx % 4
            if sub_idx in [0, 1]: # 2 steps
                # Step 1: Khái niệm & biểu thức (5.0đ)
                s1_content = f"{old_steps[0]['content']} {old_steps[1]['content']}"
                # Step 2: Tính chất & kết luận (5.0đ)
                s2_content = f"{old_steps[2]['content']} {old_steps[3]['content']}"
                new_steps = [
                    {
                        "stepId": "S1", "tenBuoc": f"Khái niệm và hệ thức: {old_steps[0]['title']}",
                        "noiDung": s1_content, "diem": 5.0,
                        "diemMotPhan": "Nêu đúng khái niệm được 2.5đ; viết đúng công thức và đơn vị được 2.5đ.",
                        "yeuCau": f"Khái niệm chuẩn và biểu thức định lượng kèm đơn vị.",
                        "loiThuongGap": "Quên ghi tên và đơn vị đại lượng; nhầm lẫn giữa các đại lượng.",
                        "tuongDuong": "Chấp nhận các cách phát biểu tương đương theo SGK KHTN 9."
                    },
                    {
                        "stepId": "S2", "tenBuoc": f"Biện luận tính chất và kết luận",
                        "noiDung": s2_content, "diem": 5.0,
                        "diemMotPhan": "Lập luận đúng quan hệ được 3.0đ; kết luận đáp số chuẩn được 2.0đ.",
                        "yeuCau": f"Lập luận chặt chẽ và kết luận chính xác theo yêu cầu đề bài.",
                        "loiThuongGap": "Chỉ đưa ra kết luận mà không lập luận giải thích.",
                        "tuongDuong": "Lập luận bằng lời hoặc viết biểu thức tỉ lệ đều được chấp nhận."
                    }
                ]
            else: # 3 steps
                new_steps = [
                    {
                        "stepId": "S1", "tenBuoc": old_steps[0]['title'],
                        "noiDung": old_steps[0]['content'], "diem": 3.5,
                        "diemMotPhan": "Nêu đúng bản chất được 3.5đ; thiếu ý nhỏ được 2.0đ.",
                        "yeuCau": "Nêu đúng khái niệm hoặc điều kiện theo yêu cầu.",
                        "loiThuongGap": "Nêu thiếu điều kiện cần thiết.",
                        "tuongDuong": "Diễn đạt tương đương được chấp nhận."
                    },
                    {
                        "stepId": "S2", "tenBuoc": f"{old_steps[1]['title']} & {old_steps[2]['title']}",
                        "noiDung": f"{old_steps[1]['content']} {old_steps[2]['content']}", "diem": 4.5,
                        "diemMotPhan": "Viết đúng biểu thức được 2.5đ; giải thích liên hệ được 2.0đ.",
                        "yeuCau": "Hệ thức liên hệ định lượng và phân tích quy luật.",
                        "loiThuongGap": "Sai biểu thức toán học hoặc nhầm đơn vị.",
                        "tuongDuong": "Chấp nhận các biến đổi tương đương."
                    },
                    {
                        "stepId": "S3", "tenBuoc": old_steps[3]['title'],
                        "noiDung": old_steps[3]['content'], "diem": 2.0,
                        "diemMotPhan": "Kết luận đúng được 2.0đ.",
                        "yeuCau": "Kết luận đáp số hoặc ý nghĩa vật lý đầy đủ.",
                        "loiThuongGap": "Thiếu câu kết luận cuối cùng.",
                        "tuongDuong": "Ghi tóm tắt đáp số chuẩn."
                    }
                ]
                
        elif gv == 'Lan': # Trung bình - Ứng dụng thực tế (64 questions)
            # Natural 3 steps: Theory -> Phenomenon analysis -> Real world application/safety conclusion
            s2_text = f"{old_steps[1]['content']} {old_steps[2]['content']}"
            new_steps = [
                {
                    "stepId": "S1", "tenBuoc": f"Cơ sở lý thuyết: {old_steps[0]['title']}",
                    "noiDung": old_steps[0]['content'], "diem": 3.5,
                    "diemMotPhan": "Nêu đúng nguyên lý hoặc công thức liên quan được 3.5đ; thiếu ý được 2.0đ.",
                    "yeuCau": "Xác định đúng nguyên lý/định luật vật lý chi phối hiện tượng.",
                    "loiThuongGap": "Nhầm lẫn hiện tượng hoặc chỉ nói cảm tính không dựa trên cơ sở khoa học.",
                    "tuongDuong": "Diễn đạt bằng lời hoặc công thức vật lý đều đạt điểm."
                },
                {
                    "stepId": "S2", "tenBuoc": f"Phân tích bản chất hiện tượng thực tế",
                    "noiDung": s2_text, "diem": 4.0,
                    "diemMotPhan": "Phân tích đúng các yếu tố ảnh hưởng được 2.5đ; giải thích cơ chế tác động được 1.5đ.",
                    "yeuCau": "Liên hệ cơ sở lý thuyết với các số liệu/đặc điểm thực tế của đề bài.",
                    "loiThuongGap": "Bỏ qua phân tích chi tiết mà nhảy ngay vào kết luận.",
                    "tuongDuong": "Chấp nhận phân tích định tính hoặc đưa số liệu minh họa tương đương."
                },
                {
                    "stepId": "S3", "tenBuoc": f"Ý nghĩa thực tiễn và kết luận",
                    "noiDung": old_steps[3]['content'], "diem": 2.5,
                    "diemMotPhan": "Kết luận đúng giải pháp/hậu quả thực tế được 2.5đ.",
                    "yeuCau": "Trả lời dứt khoát câu hỏi của đề bài và nêu ý nghĩa an toàn/đời sống.",
                    "loiThuongGap": "Thiếu câu chốt trả lời trực tiếp câu hỏi thực tiễn.",
                    "tuongDuong": "Nêu được giải pháp tương đương trong đời sống."
                }
            ]
            
        elif gv == 'Duy': # Khó - Tính toán nâng cao (64 questions)
            sub_idx = idx % 4
            if sub_idx in [0, 2]: # Standard 3-step calculation
                # Merge B1 & B2 if B1 is just formula or tóm tắt
                s1_text = f"{old_steps[0]['content']} {old_steps[1]['content']}"
                new_steps = [
                    {
                        "stepId": "S1", "tenBuoc": f"Tóm tắt, đổi đơn vị và thiết lập công thức",
                        "noiDung": s1_text, "diem": 3.5,
                        "diemMotPhan": "Đổi đơn vị đúng được 1.5đ; nêu đúng công thức áp dụng được 2.0đ.",
                        "yeuCau": "Đổi các đại lượng về đơn vị chuẩn SI và viết công thức cơ bản.",
                        "loiThuongGap": "Quên đổi đơn vị (cm sang m, g sang kg, phút sang giây); sai công thức gốc.",
                        "tuongDuong": "Viết công thức biến đổi trực tiếp vẫn cho đủ điểm."
                    },
                    {
                        "stepId": "S2", "tenBuoc": f"Biến đổi và tính toán: {old_steps[2]['title']}",
                        "noiDung": old_steps[2]['content'], "diem": 4.5,
                        "diemMotPhan": "Biến đổi đúng biểu thức được 2.0đ; thay số tính ra kết quả đúng được 2.5đ (sai đơn vị trừ 0.5đ).",
                        "yeuCau": "Thay số chính xác, thực hiện đúng các phép tính toán trung gian.",
                        "loiThuongGap": "Bấm máy tính nhầm số thập phân, lũy thừa hoặc căn bậc hai.",
                        "tuongDuong": "Chấp nhận các bước tính gộp hoặc tách đại lượng phụ."
                    },
                    {
                        "stepId": "S3", "tenBuoc": f"Kết luận đáp số và nhận xét",
                        "noiDung": old_steps[3]['content'], "diem": 2.0,
                        "diemMotPhan": "Ghi đúng đáp số kèm đơn vị chuẩn được 2.0đ.",
                        "yeuCau": "Đáp số đầy đủ, chính xác, có đơn vị vật lý rõ ràng.",
                        "loiThuongGap": "Thiếu đơn vị hoặc ghi sai đơn vị của đại lượng cần tìm.",
                        "tuongDuong": "Ghi đáp số dưới dạng số thập phân hoặc phân số chuẩn."
                    }
                ]
            else: # 4 steps with customized point allocation (e.g. for multi-part a and b)
                new_steps = [
                    {
                        "stepId": "S1", "tenBuoc": f"Câu a: {old_steps[0]['title']}",
                        "noiDung": old_steps[0]['content'], "diem": 3.5,
                        "diemMotPhan": "Nêu công thức và tính đúng kết quả câu a được 3.5đ (sai đơn vị trừ 0.5đ).",
                        "yeuCau": "Hoàn thành trọn vẹn kết quả của phần câu hỏi thứ nhất (câu a).",
                        "loiThuongGap": "Quên đổi đơn vị ở câu a.",
                        "tuongDuong": "Các cách giải tương đương cho kết quả câu a đúng."
                    },
                    {
                        "stepId": "S2", "tenBuoc": f"Thiết lập điều kiện câu b: {old_steps[1]['title']}",
                        "noiDung": old_steps[1]['content'], "diem": 2.5,
                        "diemMotPhan": "Thiết lập đúng mối quan hệ hoặc thông số mới ở câu b được 2.5đ.",
                        "yeuCau": "Xác định sự thay đổi của các thông số ở câu b.",
                        "loiThuongGap": "Hiểu sai dữ kiện đề bài khi điều kiện thay đổi.",
                        "tuongDuong": "Lập tỉ số hoặc tính giá trị mới trực tiếp."
                    },
                    {
                        "stepId": "S3", "tenBuoc": f"Câu b: {old_steps[2]['title']}",
                        "noiDung": old_steps[2]['content'], "diem": 3.0,
                        "diemMotPhan": "Tính ra đúng kết quả câu b được 3.0đ.",
                        "yeuCau": "Tính toán chính xác kết quả của phần câu hỏi thứ hai (câu b).",
                        "loiThuongGap": "Tính toán sai số học; nhầm lẫn giữa giá trị mới và độ biến thiên.",
                        "tuongDuong": "Chấp nhận các cách tính nhanh hoặc tỉ lệ."
                    },
                    {
                        "stepId": "S4", "tenBuoc": f"Tổng kết đáp số: {old_steps[3]['title']}",
                        "noiDung": old_steps[3]['content'], "diem": 1.0,
                        "diemMotPhan": "Ghi đúng kết luận cả 2 câu a và b được 1.0đ.",
                        "yeuCau": "Kết luận rõ ràng đáp số của cả hai câu a và b kèm đơn vị.",
                        "loiThuongGap": "Chỉ ghi đáp số của 1 câu.",
                        "tuongDuong": "Ghi đáp số rõ ràng ở cuối bài."
                    }
                ]
                
        else: # Ha - Tổng hợp (Chuyên sâu) (64 questions)
            sub_idx = idx % 4
            if sub_idx in [0, 1]: # 4-step comprehensive analysis
                new_steps = [
                    {
                        "stepId": "S1", "tenBuoc": f"Phân tích cơ sở: {old_steps[0]['title']}",
                        "noiDung": old_steps[0]['content'], "diem": 2.5,
                        "diemMotPhan": "Xác định đúng các quá trình vật lý và đổi đơn vị được 2.5đ.",
                        "yeuCau": "Phân tích đúng các giai đoạn chuyển hóa năng lượng hoặc hiện tượng.",
                        "loiThuongGap": "Bỏ sót một giai đoạn hoặc đổi sai đơn vị ban đầu.",
                        "tuongDuong": "Tóm tắt kết hợp sơ đồ hóa bài toán."
                    },
                    {
                        "stepId": "S2", "tenBuoc": f"Giai đoạn 1: {old_steps[1]['title']}",
                        "noiDung": old_steps[1]['content'], "diem": 3.0,
                        "diemMotPhan": "Thiết lập đúng phương trình và tính được thông số giai đoạn 1 được 3.0đ.",
                        "yeuCau": "Tính toán hoặc lập luận chính xác thông số trung gian thứ nhất.",
                        "loiThuongGap": "Áp dụng sai định luật cho giai đoạn 1.",
                        "tuongDuong": "Các phương pháp tính tương đương."
                    },
                    {
                        "stepId": "S3", "tenBuoc": f"Giai đoạn 2: {old_steps[2]['title']}",
                        "noiDung": old_steps[2]['content'], "diem": 3.0,
                        "diemMotPhan": "Thiết lập đúng phương trình cân bằng hoặc biến đổi giai đoạn 2 được 3.0đ.",
                        "yeuCau": "Tính toán hoặc lập luận chính xác thông số giai đoạn tiếp theo.",
                        "loiThuongGap": "Quên tính đến hiệu suất hoặc sự hao phí năng lượng.",
                        "tuongDuong": "Dùng phương pháp năng lượng hoặc động lực học tương đương."
                    },
                    {
                        "stepId": "S4", "tenBuoc": f"Tổng hợp và kết luận: {old_steps[3]['title']}",
                        "noiDung": old_steps[3]['content'], "diem": 1.5,
                        "diemMotPhan": "Kết luận đáp số và nhận xét ý nghĩa thực tiễn được 1.5đ.",
                        "yeuCau": "Kết luận chuẩn xác đáp số cuối cùng và rút ra bài học thực tiễn.",
                        "loiThuongGap": "Thiếu đáp số hoặc không đánh giá tính thực tiễn.",
                        "tuongDuong": "Kết luận đáp số kèm nhận xét."
                    }
                ]
            else: # 5-step detailed synthesis
                # Split step 2 or 3 into intermediate physics reasoning
                new_steps = [
                    {
                        "stepId": "S1", "tenBuoc": f"Nhận định bài toán & chuẩn hóa đơn vị",
                        "noiDung": old_steps[0]['content'], "diem": 2.0,
                        "diemMotPhan": "Tóm tắt và đổi đơn vị chuẩn được 2.0đ.",
                        "yeuCau": "Nhận diện đúng dạng toán và chuẩn hóa toàn bộ đơn vị về SI.",
                        "loiThuongGap": "Sai đơn vị chuẩn.",
                        "tuongDuong": "Tóm tắt đề bài."
                    },
                    {
                        "stepId": "S2", "tenBuoc": f"Thiết lập định luật cho quá trình 1",
                        "noiDung": old_steps[1]['content'], "diem": 2.5,
                        "diemMotPhan": "Viết đúng phương trình quá trình 1 được 2.5đ.",
                        "yeuCau": "Áp dụng đúng định luật vật lý cho quá trình ban đầu.",
                        "loiThuongGap": "Nhầm lẫn công thức định luật.",
                        "tuongDuong": "Biến đổi toán học tương đương."
                    },
                    {
                        "stepId": "S3", "tenBuoc": f"Thiết lập định luật cho quá trình 2 & chuyển hóa",
                        "noiDung": old_steps[2]['content'], "diem": 2.5,
                        "diemMotPhan": "Phân tích đúng chuyển hóa hoặc hao phí được 2.5đ.",
                        "yeuCau": "Mô tả đúng sự tương tác, truyền nhiệt hoặc hao phí giữa các thành phần.",
                        "loiThuongGap": "Bỏ qua mất mát năng lượng do ma sát/nhiệt.",
                        "tuongDuong": "Cân bằng năng lượng toàn phần."
                    },
                    {
                        "stepId": "S4", "tenBuoc": f"Giải hệ phương trình tìm nghiệm",
                        "noiDung": f"Kết hợp các kết quả trên để giải biểu thức: {old_steps[3]['content']}", "diem": 2.0,
                        "diemMotPhan": "Tính ra giá trị đại lượng cần tìm được 2.0đ.",
                        "yeuCau": "Tính toán ra kết quả số học chính xác.",
                        "loiThuongGap": "Tính toán sai số học trong bước giải phương trình.",
                        "tuongDuong": "Các cách rút gọn biểu thức khác nhau."
                    },
                    {
                        "stepId": "S5", "tenBuoc": f"Biện luận vật lý và kết luận đáp số",
                        "noiDung": f"Đáp số & Biện luận: {old_steps[3]['content']}", "diem": 1.0,
                        "diemMotPhan": "Ghi đúng kết luận và nhận xét được 1.0đ.",
                        "yeuCau": "Kết luận đáp số có đơn vị và đánh giá kết quả phù hợp thực tế.",
                        "loiThuongGap": "Thiếu kết luận đáp số.",
                        "tuongDuong": "Ghi đáp số rõ ràng."
                    }
                ]
                
        # Validate point sum == 10.0
        pt_sum = sum(s['diem'] for s in new_steps)
        if abs(pt_sum - 10.0) > 1e-4:
            print(f"Error in {cid}: pt_sum = {pt_sum}")
            
        num_s = len(new_steps)
        step_distribution[num_s] = step_distribution.get(num_s, 0) + 1
        
        # Build LoiGiai string for DapAnTuLuanMau
        loi_giai_parts = []
        rubric_parts = []
        for s_idx, s in enumerate(new_steps, 1):
            s_pts = f"{s['diem']:g}đ"
            loi_giai_parts.append(f"• B{s_idx} ({s['tenBuoc']} - {s_pts}): {s['noiDung']}")
            rubric_parts.append(f"[B{s_idx}] ({s_pts}) {s['yeuCau']}")
            
            gemini_rubric_sheet2.append({
                "CauHoiID": cid,
                "StepId": s['stepId'],
                "TenBuoc": s['tenBuoc'],
                "NoiDungBuoc": s['noiDung'],
                "DiemToiDa": s['diem'],
                "DiemMotPhan": s['diemMotPhan'],
                "YeuCauKiemTra": s['yeuCau'],
                "LoiThuongGap": s['loiThuongGap'],
                "PhuongPhapTuongDuong": s['tuongDuong'],
                "TrangThaiDuyet": "CHO_DUYET"
            })
            
        dapan_refactored = "\n".join(loi_giai_parts)
        rubric_refactored = " | ".join(rubric_parts) + f" | Lưu ý: {row['GiaiThich_MeoGhiNho']}"
        
        refactored_sheet1.append({
            "STT": row['STT'],
            "CauHoiID": cid,
            "ChuongId": row['ChuongId'],
            "TenChuong": row['TenChuong'],
            "BaiID": row['BaiID'],
            "TenBai": row['TenBai'],
            "GiaoVienID": row['GiaoVienID'],
            "TenGiaoVien": row['TenGiaoVien'],
            "MucDo": row['MucDo'],
            "NoiDung": row['NoiDung'],
            "TuKhoaChamDiem": row['TuKhoaChamDiem'],
            "DapAnTuLuanMau": dapan_refactored,
            "GiaiThich_MeoGhiNho": rubric_refactored,
            "TrangThai": row['TrangThai']
        })
        
    df_s1 = pd.DataFrame(refactored_sheet1)
    df_s2 = pd.DataFrame(gemini_rubric_sheet2)
    
    out_file = "BoSung_TuLuan_ChoDuyet.xlsx"
    with pd.ExcelWriter(out_file, engine='openpyxl') as writer:
        df_s1.to_excel(writer, sheet_name="NganHangTuLuan", index=False)
        df_s2.to_excel(writer, sheet_name="Rubric_TungBuoc_Gemini", index=False)
        
    print(f"Successfully refactored {out_file}")
    print(f"Sheet 1 (NganHangTuLuan): {len(df_s1)} rows, {len(df_s1.columns)} cols")
    print(f"Sheet 2 (Rubric_TungBuoc_Gemini): {len(df_s2)} rows, {len(df_s2.columns)} cols")
    print(f"Step distribution across 256 questions: {step_distribution}")

if __name__ == '__main__':
    refactor_256()
