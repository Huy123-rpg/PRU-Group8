import urllib.request, json

payload = {
    'cauHoiId': 'TL_C1_L2_Duy',
    'studentText': 'Đổi m = 200g = 0,2 kg. Động năng ban đầu: Wđ = 1/2 . m . v^2 = 1/2 . 0,2 . 10^2 = 10 J.'
}
data = json.dumps(payload).encode('utf-8')
req = urllib.request.Request('http://127.0.0.1:5050/api/grade_physics', data=data, headers={'Content-Type': 'application/json'})
with urllib.request.urlopen(req, timeout=5) as resp:
    res = json.loads(resp.read().decode('utf-8'))
    print("Mode:", res.get("mode"))
    print("Total score:", res['totalScore'], '/', res['maxScore'])
    print("Next step hint:", res['nextStepHint'])
    for s in res['steps']:
        print(f"  [{s['stepId']}] {s['status'].upper():12s} ({s['awardedPoints']}đ) - {s['explanation']}")
