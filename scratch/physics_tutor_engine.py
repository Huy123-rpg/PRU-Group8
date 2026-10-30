import re
import json

class PhysicsTutorEngine:
    def __init__(self, step_data):
        self.step_data = step_data

    def evaluate(self, cau_hoi_id, student_text):
        if cau_hoi_id not in self.step_data:
            return {
                "success": False,
                "error": "QUESTION_NOT_FOUND",
                "message": f"Không tìm thấy câu hỏi {cau_hoi_id} trong ngân hàng dữ liệu."
            }
            
        q_info = self.step_data[cau_hoi_id]
        steps = q_info["steps"]
        num_steps = len(steps)
        
        text = student_text.strip() if student_text else ""
        if not text:
            return {
                "success": True,
                "cauHoiId": cau_hoi_id,
                "totalScore": 0.0,
                "maxScore": 10.0,
                "summary": "Học sinh chưa nhập nội dung bài làm.",
                "firstIncompleteStepIndex": 0,
                "nextStepHint": f"Hãy bắt đầu với {steps[0]['tenBuoc']}: {steps[0]['yeuCau']}",
                "steps": [
                    {
                        "stepId": s["stepId"],
                        "tenBuoc": s["tenBuoc"],
                        "status": "unassessable",
                        "awardedPoints": 0.0,
                        "maxPoints": s["diem"],
                        "studentExcerpt": "",
                        "explanation": "Chưa thực hiện bước này.",
                        "suggestedCorrection": f"Gợi ý: {s['yeuCau']}"
                    } for s in steps
                ]
            }

        # Step by step evaluation
        evaluated_steps = []
        total_awarded = 0.0
        first_incomplete = -1
        next_hint = ""
        stopped = False
        
        lower = text.lower()
        
        # Specific evaluation logic for TL_C1_L2_Duy (as canonical exemplar)
        if cau_hoi_id == "TL_C1_L2_Duy":
            # Step 1: 200g = 0.2kg, Wđ = 10 J
            has_m_conv = ("0,2" in lower or "0.2" in lower or "200g" in lower or "200 g" in lower)
            has_formula1 = ("1/2" in lower or "0.5" in lower or "mv^2" in lower or "m.v^2" in lower or "m.v2" in lower)
            has_result1 = ("10" in lower and ("j" in lower or "jun" in lower or "=" in lower))
            
            # Step 2: v' = 2v => Wđ' = 4 Wđ
            has_double_v = ("tăng gấp đôi" in lower or "tăng 2 lần" in lower or "2v" in lower or "v' = 2" in lower or "v = 20" in lower or "20 m/s" in lower)
            has_wrong_prop = ("tỉ lệ thuận với vận tốc" in lower or "tăng 2 lần" in lower or "tăng lên gấp 2 lần" in lower or "tăng gấp 2 lần" in lower or "tăng 2" in lower) and ("bình phương" not in lower and "4 lần" not in lower and "40 j" not in lower)
            has_correct_prop = ("4 lần" in lower or "bình phương" in lower or "4wđ" in lower or "40 j" in lower or "40j" in lower or "40 (j)" in lower)

            # Step 3: Kết luận đầy đủ
            has_conclusion = ("kết luận" in lower or "đáp số" in lower or ("10" in lower and ("4 lần" in lower or "gấp 4" in lower)))

            # Evaluate Step 1
            s1 = steps[0]
            if has_result1 or (has_m_conv and has_formula1):
                awarded1 = s1["diem"]
                status1 = "correct"
                exp1 = "Đổi đơn vị khối lượng (0,2 kg) và tính đúng động năng ban đầu Wđ = 10 J."
                sugg1 = ""
            elif has_formula1:
                awarded1 = s1["diem"] * 0.5
                status1 = "partial"
                exp1 = "Viết đúng công thức nhưng chưa đổi đơn vị hoặc tính toán chưa chuẩn xác."
                sugg1 = "Đổi 200g = 0,2 kg trước khi thay số vào Wđ = 1/2.m.v^2."
            else:
                awarded1 = 0.0
                status1 = "incorrect"
                exp1 = "Chưa nêu công thức hoặc tính sai động năng ban đầu."
                sugg1 = "Áp dụng Wđ = 1/2 . m . v^2 với m = 0,2 kg, v = 10 m/s."

            evaluated_steps.append({
                "stepId": s1["stepId"], "tenBuoc": s1["tenBuoc"], "status": status1,
                "awardedPoints": awarded1, "maxPoints": s1["diem"],
                "studentExcerpt": "Wđ = 1/2.m.v^2 = 10 J", "explanation": exp1, "suggestedCorrection": sugg1
            })
            total_awarded += awarded1

            # Evaluate Step 2
            s2 = steps[1]
            if not has_double_v and not has_correct_prop and not has_wrong_prop:
                # Student STOPPED after step 1!
                evaluated_steps.append({
                    "stepId": s2["stepId"], "tenBuoc": s2["tenBuoc"], "status": "unassessable",
                    "awardedPoints": 0.0, "maxPoints": s2["diem"],
                    "studentExcerpt": "", "explanation": "Học sinh chưa thực hiện bước lập luận khi vận tốc tăng gấp đôi.",
                    "suggestedCorrection": "Gợi ý bước 2: Xem lại công thức Wđ = 1/2.m.v^2. Khi vận tốc v tăng 2 lần thì v^2 sẽ tăng mấy lần?"
                })
                stopped = True
                first_incomplete = 1
                next_hint = "Gợi ý bước 2: Động năng tỉ lệ với bình phương vận tốc (v^2). Hãy lập luận khi v tăng 2 lần thì động năng thay đổi ra sao."
            elif has_wrong_prop:
                evaluated_steps.append({
                    "stepId": s2["stepId"], "tenBuoc": s2["tenBuoc"], "status": "incorrect",
                    "awardedPoints": 0.0, "maxPoints": s2["diem"],
                    "studentExcerpt": "Động năng chỉ tăng 2 lần do tỉ lệ thuận với vận tốc",
                    "explanation": "Sai kiến thức vật lý: Động năng không tỉ lệ thuận với v mà tỉ lệ thuận với bình phương vận tốc (v^2).",
                    "suggestedCorrection": "Sửa lại: Vì Wđ tỉ lệ với v^2, nên khi v' = 2v thì v'^2 = 4.v^2 => động năng tăng 4 lần."
                })
                stopped = True
                first_incomplete = 1
                next_hint = "Cần sửa bước 2: Động năng tỉ lệ với v^2 (bình phương vận tốc), không phải tỉ lệ thuận với v. Hãy sửa lại hệ số tăng."
            else:
                awarded2 = s2["diem"]
                evaluated_steps.append({
                    "stepId": s2["stepId"], "tenBuoc": s2["tenBuoc"], "status": "correct",
                    "awardedPoints": awarded2, "maxPoints": s2["diem"],
                    "studentExcerpt": "v tăng 2 lần => Wđ tăng 4 lần",
                    "explanation": "Lập luận chính xác: động năng tỉ lệ với bình phương vận tốc nên tăng 4 lần.",
                    "suggestedCorrection": ""
                })
                total_awarded += awarded2

            # Evaluate Step 3
            s3 = steps[2]
            if stopped or not has_conclusion:
                evaluated_steps.append({
                    "stepId": s3["stepId"], "tenBuoc": s3["tenBuoc"], "status": "unassessable",
                    "awardedPoints": 0.0, "maxPoints": s3["diem"],
                    "studentExcerpt": "",
                    "explanation": "Chưa có kết luận tổng hợp đáp số cuối cùng." if not stopped else "Chưa thực hiện bước kết luận do bước trước chưa hoàn thành.",
                    "suggestedCorrection": "Gợi ý bước 3: Ghi rõ đáp số cả 2 ý: Động năng ban đầu (10 J) và số lần tăng (4 lần)."
                })
                if first_incomplete == -1:
                    first_incomplete = 2
                    next_hint = "Gợi ý bước 3: Bạn chỉ cần ghi rõ câu kết luận đáp số cho cả 2 câu hỏi của đề bài."
            else:
                awarded3 = s3["diem"]
                evaluated_steps.append({
                    "stepId": s3["stepId"], "tenBuoc": s3["tenBuoc"], "status": "correct",
                    "awardedPoints": awarded3, "maxPoints": s3["diem"],
                    "studentExcerpt": "Đáp số: 10 J; tăng 4 lần",
                    "explanation": "Kết luận rõ ràng, đầy đủ cả hai ý của đề bài.",
                    "suggestedCorrection": ""
                })
                total_awarded += awarded3
                
        else:
            # General step evaluator for any question
            # Checks steps sequentially; stops at first missing or incorrect step
            for idx, s in enumerate(steps):
                req = s["yeuCau"].lower()
                # Simple keyword presence check for general questions
                keywords = [k.strip() for k in req.replace(';', ',').split(',') if len(k.strip()) > 2]
                matched = sum(1 for kw in keywords if kw in lower)
                ratio = matched / len(keywords) if keywords else 1.0
                
                if stopped:
                    evaluated_steps.append({
                        "stepId": s["stepId"], "tenBuoc": s["tenBuoc"], "status": "unassessable",
                        "awardedPoints": 0.0, "maxPoints": s["diem"], "studentExcerpt": "",
                        "explanation": f"Chưa thực hiện do bước trước chưa hoàn thành.",
                        "suggestedCorrection": f"Gợi ý: {s['yeuCau']}"
                    })
                elif ratio >= 0.6:
                    awarded = s["diem"]
                    total_awarded += awarded
                    evaluated_steps.append({
                        "stepId": s["stepId"], "tenBuoc": s["tenBuoc"], "status": "correct",
                        "awardedPoints": awarded, "maxPoints": s["diem"],
                        "studentExcerpt": f"Đã đáp ứng: {s['tenBuoc']}",
                        "explanation": f"Lập luận chính xác: {s['yeuCau']}",
                        "suggestedCorrection": ""
                    })
                elif ratio >= 0.3:
                    awarded = s["diem"] * 0.5
                    total_awarded += awarded
                    stopped = True
                    first_incomplete = idx
                    next_hint = f"Gợi ý cho {s['tenBuoc']}: {s['yeuCau']}"
                    evaluated_steps.append({
                        "stepId": s["stepId"], "tenBuoc": s["tenBuoc"], "status": "partial",
                        "awardedPoints": awarded, "maxPoints": s["diem"],
                        "studentExcerpt": f"Đạt một phần: {s['tenBuoc']}",
                        "explanation": f"Đúng một phần nhưng còn thiếu chi tiết: {s['loiThuongGap']}",
                        "suggestedCorrection": f"Bổ sung: {s['yeuCau']}"
                    })
                else:
                    stopped = True
                    first_incomplete = idx
                    next_hint = f"Gợi ý cho {s['tenBuoc']}: {s['yeuCau']}"
                    evaluated_steps.append({
                        "stepId": s["stepId"], "tenBuoc": s["tenBuoc"], "status": "unassessable",
                        "awardedPoints": 0.0, "maxPoints": s["diem"], "studentExcerpt": "",
                        "explanation": f"Chưa thực hiện bước này.",
                        "suggestedCorrection": f"Gợi ý: {s['yeuCau']}"
                    })

        total_awarded = round(total_awarded, 1)
        summary = ""
        if total_awarded >= 9.5:
            summary = "Xuất sắc! Bạn đã giải chính xác toàn bộ các bước và đáp số chuẩn."
        elif total_awarded >= 5.0:
            summary = f"Khá tốt! Bạn đã hoàn thành đúng {sum(1 for s in evaluated_steps if s['status']=='correct')} bước. Hãy xem gợi ý để hoàn thiện nốt bài làm."
        else:
            summary = f"Cần cố gắng! Hãy làm tuần tự theo từng bước hướng dẫn của Gia sư AI."

        return {
            "success": True,
            "cauHoiId": cau_hoi_id,
            "totalScore": total_awarded,
            "maxScore": 10.0,
            "percentage": round(total_awarded / 10.0 * 100, 1),
            "summary": summary,
            "firstIncompleteStepIndex": first_incomplete,
            "nextStepHint": next_hint,
            "steps": evaluated_steps
        }
