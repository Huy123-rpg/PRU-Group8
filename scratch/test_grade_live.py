import os
import json
import time
import sys

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer"))
from physics_gemini_server import call_live_gemini, ALL_STEP_DATA

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")

with open(ENV_FILE, "r", encoding="utf-8") as f:
    env = dict([l.strip().split("=", 1) for l in f if "=" in l and not l.strip().startswith("#")])

key = env["GEMINI_API_KEY"].strip().strip("'\"")

cau_hoi_id = "TL_C1_L2_Duy"
q_info = ALL_STEP_DATA[cau_hoi_id]

# Test: Bài làm giải đúng đầy đủ
student_text = """Đổi m = 200g = 0,2 kg.
Động năng ban đầu của viên bi: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J.
Do động năng tỉ lệ thuận với bình phương vận tốc (v^2), nên khi vận tốc tăng gấp đôi (v' = 2v), động năng mới tăng lên 4 lần.
Kết luận: Động năng ban đầu là 10 J; khi vận tốc tăng gấp đôi thì động năng tăng 4 lần."""

for test_m in ["gemini-3.5-flash-lite", "gemini-3.5-flash"]:
    print(f"\n--- Testing grading with {test_m} ---", flush=True)
    t0 = time.time()
    try:
        res = call_live_gemini(cau_hoi_id, q_info, student_text, key, model=test_m)
        elapsed = time.time() - t0
        print(f"[GEMINI API THÀNH CÔNG] Model: {test_m} | Thời gian: {elapsed:.2f}s", flush=True)
        print(f"Tổng điểm: {res.get('totalScore')}/10", flush=True)
        print(f"Nhận xét: {res.get('summary')}", flush=True)
        print(f"Gợi ý tiếp theo: {res.get('nextStepHint')}", flush=True)
        print(f"Số bước chấm: {len(res.get('steps', []))}", flush=True)
        for s in res.get("steps", []):
            print(f"  * {s.get('stepId')}: [{s.get('status').upper()}] {s.get('awardedPoints')}/{s.get('maxPoints')}đ | {s.get('explanation')}", flush=True)
    except Exception as e:
        print(f"[GEMINI API LỖI với {test_m}]: {e}", flush=True)
