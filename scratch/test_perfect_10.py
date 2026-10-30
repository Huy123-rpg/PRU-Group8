import urllib.request
import json
import time

url = "http://127.0.0.1:5050/api/grade_physics"
payload = {
    "cauHoiId": "TL_C1_L2_Duy",
    "studentText": "B1: Đổi đơn vị: m = 200g = 0.2kg; v = 36km/h = 10m/s.\nĐộng năng ban đầu của vật là: Wđ = 1/2 * m * v^2 = 1/2 * 0.2 * 10^2 = 10 (J).\nB2: Khi vận tốc tăng lên gấp đôi v' = 2v = 20m/s, động năng mới là: Wđ' = 1/2 * m * (v')^2 = 1/2 * 0.2 * 20^2 = 40 (J). Hoặc động năng tỉ lệ thuận với bình phương vận tốc nên khi v tăng 2 lần thì động năng tăng 4 lần: Wđ' = 4 * 10 = 40 (J).\nB3: So sánh động năng mới với động năng ban đầu: Wđ' / Wđ = 40 / 10 = 4 lần. Vậy động năng tăng 4 lần."
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
        print(f"TotalScore: {data.get('totalScore')}/10")
        for s in data.get('steps', []):
            print(f"  [{s.get('status')}] {s.get('stepId')}: {s.get('score')}/{s.get('maxPoints')} - {s.get('feedback')}")
        print(f"Next hint: {data.get('nextStepHint')}")
except Exception as e:
    print(f"Request failed: {e}")
