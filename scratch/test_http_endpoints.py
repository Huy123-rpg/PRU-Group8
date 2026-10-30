import urllib.request
import json

SERVER_URL = "http://127.0.0.1:5050/api/grade_physics"

def post_grade(cau_hoi_id, student_text):
    payload = {
        "cauHoiId": cau_hoi_id,
        "studentText": student_text
    }
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        SERVER_URL,
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST"
    )
    with urllib.request.urlopen(req, timeout=10) as resp:
        return json.loads(resp.read().decode("utf-8"))

print("================================================================")
print("KIỂM THỬ TÍCH HỢP HTTP POST: http://127.0.0.1:5050/api/grade_physics")
print("================================================================")

# 1. TEST A: Giải đúng đầy đủ
test_a = """Đổi m = 200g = 0,2 kg.
Động năng ban đầu của viên bi: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J.
Do động năng tỉ lệ thuận với bình phương vận tốc (v^2), nên khi vận tốc tăng gấp đôi (v' = 2v), động năng mới tăng lên 4 lần.
Kết luận: Động năng ban đầu là 10 J; khi vận tốc tăng gấp đôi thì động năng tăng 4 lần."""

res_a = post_grade("TL_C1_L2_Duy", test_a)
print("\n[TEST A - Giải đúng toàn bộ]")
print(f"CauHoiId: {res_a.get('cauHoiId')} | Mode: {res_a.get('mode')}")
print(f"Tổng điểm: {res_a.get('totalScore')}/{res_a.get('maxScore')} ({res_a.get('percentage')}%)")
print(f"Nhận xét: {res_a.get('summary')}")
for s in res_a.get("steps", []):
    print(f"  * {s['stepId']} [{s['status'].upper()}]: {s['awardedPoints']}/{s['maxPoints']}đ | {s['explanation']}")

# 2. TEST B: Giải dở dang (dừng sau B1)
test_b = """Đổi m = 200g = 0,2 kg.
Động năng ban đầu: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J."""

res_b = post_grade("TL_C1_L2_Duy", test_b)
print("\n[TEST B - Giải dở dang, dừng sau B1]")
print(f"CauHoiId: {res_b.get('cauHoiId')} | Mode: {res_b.get('mode')}")
print(f"Tổng điểm: {res_b.get('totalScore')}/{res_b.get('maxScore')} ({res_b.get('percentage')}%)")
print(f"Nhận xét: {res_b.get('summary')}")
print(f"Gợi ý bước kế tiếp (TUTOR HINT): {res_b.get('nextStepHint')}")
for s in res_b.get("steps", []):
    print(f"  * {s['stepId']} [{s['status'].upper()}]: {s['awardedPoints']}/{s['maxPoints']}đ | {s['explanation']}")

# 3. TEST C: Áp dụng sai công thức ở B2
test_c = """Đổi m = 200g = 0,2 kg.
Động năng ban đầu: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J.
Khi vận tốc tăng gấp đôi, vì động năng tỉ lệ thuận với vận tốc nên động năng chỉ tăng lên gấp 2 lần."""

res_c = post_grade("TL_C1_L2_Duy", test_c)
print("\n[TEST C - Áp dụng sai công thức ở B2]")
print(f"CauHoiId: {res_c.get('cauHoiId')} | Mode: {res_c.get('mode')}")
print(f"Tổng điểm: {res_c.get('totalScore')}/{res_c.get('maxScore')} ({res_c.get('percentage')}%)")
print(f"Nhận xét: {res_c.get('summary')}")
print(f"Gợi ý sửa lỗi (TUTOR HINT): {res_c.get('nextStepHint')}")
for s in res_c.get("steps", []):
    print(f"  * {s['stepId']} [{s['status'].upper()}]: {s['awardedPoints']}/{s['maxPoints']}đ | {s['explanation']}")

# 4. TEST D: Câu hỏi mới bổ sung TL_BS_001
test_d = """Đổi đơn vị và áp dụng công thức tính cơ năng: W = Wđ + Wt."""
res_d = post_grade("TL_BS_001", test_d)
print("\n[TEST D - Kiểm tra câu hỏi mới bổ sung TL_BS_001]")
print(f"CauHoiId: {res_d.get('cauHoiId')} | Số bước rubric: {len(res_d.get('steps', []))}")
print(f"Tổng điểm: {res_d.get('totalScore')}/{res_d.get('maxScore')}")
print(f"Gợi ý: {res_d.get('nextStepHint')}")
print("================================================================")
