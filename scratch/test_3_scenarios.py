import json
import base64
import time
import urllib.request
import os

SERVER_URL = "http://127.0.0.1:5050"
IMAGE_PATH = r"d:\Ca_Nhan\Downloads\Bài tập định luật Joule–Lenz.png"
CAU_HOI_ID = "TL_C3_L13_Ha"

def post_json(endpoint, payload):
    url = f"{SERVER_URL}{endpoint}"
    data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
    req = urllib.request.Request(url, data=data, headers={"Content-Type": "application/json; charset=utf-8"}, method="POST")
    t0 = time.time()
    with urllib.request.urlopen(req, timeout=30) as resp:
        elapsed = time.time() - t0
        body = json.loads(resp.read().decode("utf-8"))
        return body, elapsed

print("=" * 70)
print("KIỂM THỬ THỰC TẾ 3 TRƯỜNG HỢP NỘP BÀI TỰ LUẬN QUA GEMINI AI THẬT")
print("=" * 70)

# TEST 1: CHỈ GÕ VĂN BẢN TRỰC TIẾP
print("\n[TEST 1] HỌC SINH CHỈ GÕ VĂN BẢN TRỰC TIẾP BẰNG BÀN PHÍM")
typed_text = (
    "Định luật Joule-Lenz: Nhiệt lượng tỏa ra ở dây dẫn khi có dòng điện chạy qua "
    "tỉ lệ thuận với bình phương cường độ dòng điện, với điện trở của dây dẫn và thời gian dòng điện chạy qua.\n"
    "Hệ thức của định luật: Q = I^2 * R * t\n"
    "Trong đó:\n"
    "+ I là cường độ dòng điện (A)\n"
    "+ R là điện trở của dây dẫn (Ohm)\n"
    "+ t là thời gian dòng điện chạy qua (s)\n"
    "+ Q là nhiệt lượng tỏa ra (J)"
)
print(f" -> Văn bản đã gõ: {len(typed_text)} ký tự")
res1, dur1 = post_json("/api/grade_physics", {
    "cauHoiId": CAU_HOI_ID,
    "studentText": typed_text
})
print(f" -> Nguồn chấm: {res1.get('source')} | Model: {res1.get('modelUsed', 'gemini')} | Thời gian: {dur1:.2f}s")
print(f" -> Tổng điểm: {res1.get('totalScore')}/10 đ ({res1.get('percentage')}%)")
print(f" -> Nhận xét chung: {res1.get('summary')}")
print(f" -> Số bước chấm: {len(res1.get('steps', []))}")
for s in res1.get("steps", []):
    print(f"    + [{s.get('status').upper()}] Bước {s.get('stepId')}: {s.get('awardedPoints')}/{s.get('maxPoints')}đ - {s.get('explanation')}")

# TEST 2: TẢI ẢNH -> OCR -> DÙNG BẢN CHỮ NÀY -> NỘP BÀI
print("\n[TEST 2] TẢI ẢNH THẬT -> OCR GEMINI -> DÙNG BẢN CHỮ NÀY -> NỘP BÀI")
if not os.path.exists(IMAGE_PATH):
    print(f" -> [LỖI] Không tìm thấy ảnh tại: {IMAGE_PATH}")
else:
    with open(IMAGE_PATH, "rb") as f:
        img_bytes = f.read()
    b64_str = base64.b64encode(img_bytes).decode("utf-8")
    print(f" -> Ảnh đọc thành công: {len(img_bytes)} bytes | Base64: {len(b64_str)} chars")
    
    ocr_res, ocr_dur = post_json("/api/ocr_physics", {
        "imageBase64": b64_str,
        "mimeType": "image/png"
    })
    print(f" -> OCR thành công từ: {ocr_res.get('source')} | Model: {ocr_res.get('modelUsed')} | Thời gian: {ocr_dur:.2f}s")
    ocr_text = ocr_res.get("transcriptionText", "").strip()
    print(f" -> Bản chữ nhận dạng:\n{ocr_text}")
    
    print("\n -> Học sinh bấm 'DÙNG BẢN CHỮ NÀY' -> Bấm NỘP BÀI:")
    res2, dur2 = post_json("/api/grade_physics", {
        "cauHoiId": CAU_HOI_ID,
        "studentText": ocr_text
    })
    print(f" -> Nguồn chấm: {res2.get('source')} | Thời gian: {dur2:.2f}s")
    print(f" -> Tổng điểm: {res2.get('totalScore')}/10 đ ({res2.get('percentage')}%)")
    print(f" -> Nhận xét chung: {res2.get('summary')}")
    for s in res2.get("steps", []):
        print(f"    + [{s.get('status').upper()}] Bước {s.get('stepId')}: {s.get('awardedPoints')}/{s.get('maxPoints')}đ - {s.get('explanation')}")

# TEST 3: GÕ MỘT PHẦN -> TẢI ẢNH -> OCR -> GHÉP CUỐI -> SỬA THÊM -> NỘP BÀI
print("\n[TEST 3] GÕ TRƯỚC MỘT PHẦN -> OCR -> GHÉP CUỐI -> SỬA BỔ SUNG -> NỘP BÀI")
partial_intro = "Phần 1: Khái niệm về tác dụng nhiệt của dòng điện trong đời sống và kỹ thuật."
merged_text = partial_intro + "\n\n" + ocr_text + "\n\nPhần bổ sung: Mối liên hệ giữa Jun và Calo: 1 cal = 4.186 J."
print(f" -> Toàn bộ nội dung ghép và sửa bổ sung:\n{merged_text}")

res3, dur3 = post_json("/api/grade_physics", {
    "cauHoiId": CAU_HOI_ID,
    "studentText": merged_text
})
print(f" -> Nguồn chấm: {res3.get('source')} | Thời gian: {dur3:.2f}s")
print(f" -> Tổng điểm: {res3.get('totalScore')}/10 đ ({res3.get('percentage')}%)")
print(f" -> Nhận xét chung: {res3.get('summary')}")
for s in res3.get("steps", []):
    print(f"    + [{s.get('status').upper()}] Bước {s.get('stepId')}: {s.get('awardedPoints')}/{s.get('maxPoints')}đ - {s.get('explanation')}")

print("\n" + "=" * 70)
print("HOÀN TẤT KIỂM THỬ THỰC TẾ 3 KỊCH BẢN NỘP BÀI TỰ LUẬN QUA GEMINI AI")
print("=" * 70)
