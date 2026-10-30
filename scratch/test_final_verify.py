import urllib.request
import json
import time

url = "http://127.0.0.1:5050/api/grade_physics"
payload = {
    "cau_hoi_id": "TL_C1_L2_Duy",
    "student_answer": "B1: Khúc xạ ánh sáng là hiện tượng tia sáng bị gãy khúc khi truyền qua mặt phân cách giữa hai môi trường trong suốt.\nB2: Khi truyền từ không khí vào nước, góc khúc xạ r nhỏ hơn góc tới i.\nB3: Khi tăng góc tới i thì góc khúc xạ r cũng tăng."
}

t0 = time.time()
req = urllib.request.Request(url, data=json.dumps(payload).encode('utf-8'), headers={'Content-Type': 'application/json'})
try:
    with urllib.request.urlopen(req, timeout=15) as res:
        dt = time.time() - t0
        body = res.read().decode('utf-8')
        data = json.loads(body)
        print(f"Status: {res.status} in {dt:.2f}s")
        print(f"Source: {data.get('source')}")
        print(f"TotalScore: {data.get('totalScore')}")
        print(f"Steps count: {len(data.get('steps', []))}")
        print(f"Next hint: {data.get('nextStepHint')}")
        print("All OK!")
except Exception as e:
    print(f"Request failed: {e}")
