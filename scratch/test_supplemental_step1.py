import urllib.request
import json
import time

url = "http://127.0.0.1:5050/api/grade_physics"
payload = {
    "cauHoiId": "TL_BS_001",
    "studentText": "B1: Động năng của một vật phụ thuộc vào khối lượng m và tốc độ v. Biểu thức: Wđ = 1/2 * m * v^2."
}

t0 = time.time()
req = urllib.request.Request(url, data=json.dumps(payload).encode('utf-8'), headers={'Content-Type': 'application/json'})
try:
    with urllib.request.urlopen(req, timeout=30) as res:
        dt = time.time() - t0
        body = res.read().decode('utf-8')
        data = json.loads(body)
        print(f"Status: {res.status} in {dt:.2f}s")
        print(f"Source: {data.get('source')}")
        print(f"TotalScore: {data.get('totalScore')}/10")
        print(f"Next hint: {data.get('nextStepHint')}")
        for s in data.get('steps', []):
            print(f"  Step {s.get('stepId')}: {s.get('status')} - {s.get('awardedPoints')}/{s.get('maxPoints')} - {s.get('explanation')}")
except Exception as e:
    print(f"Request failed: {e}")
