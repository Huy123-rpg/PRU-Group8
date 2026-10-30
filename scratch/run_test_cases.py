import json
from old_64_data_part1 import OLD_64_STEP_DATA
from old_64_data_part2 import OLD_64_STEP_DATA_P2
from old_64_data_part3 import OLD_64_STEP_DATA_P3
from physics_tutor_engine import PhysicsTutorEngine

all_old_steps = {}
all_old_steps.update(OLD_64_STEP_DATA)
all_old_steps.update(OLD_64_STEP_DATA_P2)
all_old_steps.update(OLD_64_STEP_DATA_P3)

engine = PhysicsTutorEngine(all_old_steps)

# TEST A: Học sinh giải đúng đầy đủ
test_a_input = """Đổi m = 200g = 0,2 kg.
Động năng ban đầu của viên bi: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J.
Do động năng tỉ lệ thuận với bình phương vận tốc (v^2), nên khi vận tốc tăng gấp đôi (v' = 2v), động năng mới tăng lên 4 lần.
Kết luận: Động năng ban đầu là 10 J; khi vận tốc tăng gấp đôi thì động năng tăng 4 lần."""

# TEST B: Học sinh mới giải đúng các bước đầu, chưa hoàn thành
test_b_input = """Đổi m = 200g = 0,2 kg.
Động năng ban đầu: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J."""

# TEST C: Học sinh áp dụng sai công thức
test_c_input = """Đổi m = 200g = 0,2 kg.
Động năng ban đầu: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J.
Khi vận tốc tăng gấp đôi, vì động năng tỉ lệ thuận với vận tốc nên động năng chỉ tăng lên gấp 2 lần."""

print("=" * 60)
print("TEST A: Học sinh giải đúng đầy đủ")
res_a = engine.evaluate("TL_C1_L2_Duy", test_a_input)
print(f"Total Score: {res_a['totalScore']} / {res_a['maxScore']}")
print(f"Summary: {res_a['summary']}")
for s in res_a['steps']:
    print(f"  [{s['stepId']}] {s['status'].upper():12s} ({s['awardedPoints']}/{s['maxPoints']}đ) - {s['explanation']}")

print("\n" + "=" * 60)
print("TEST B: Học sinh mới giải đúng bước 1, dừng lại chưa làm bước 2")
res_b = engine.evaluate("TL_C1_L2_Duy", test_b_input)
print(f"Total Score: {res_b['totalScore']} / {res_b['maxScore']}")
print(f"Summary: {res_b['summary']}")
print(f"Gợi ý bước kế tiếp (Tutor Hint): {res_b['nextStepHint']}")
for s in res_b['steps']:
    print(f"  [{s['stepId']}] {s['status'].upper():12s} ({s['awardedPoints']}/{s['maxPoints']}đ) - {s['explanation']}")

print("\n" + "=" * 60)
print("TEST C: Học sinh áp dụng sai công thức ở bước 2")
res_c = engine.evaluate("TL_C1_L2_Duy", test_c_input)
print(f"Total Score: {res_c['totalScore']} / {res_c['maxScore']}")
print(f"Summary: {res_c['summary']}")
print(f"Gợi ý sửa lỗi (Tutor Hint): {res_c['nextStepHint']}")
for s in res_c['steps']:
    print(f"  [{s['stepId']}] {s['status'].upper():12s} ({s['awardedPoints']}/{s['maxPoints']}đ) - {s['explanation']}")
