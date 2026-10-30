#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
=============================================================================
AURELIA ACADEMY - PHYSICS 9 GEMINI AI TUTOR BACKEND
=============================================================================
Server chuyển tiếp độc lập kết nối Google Gemini API và Unity Game 2.
- Bảo mật: Tuyệt đối không nhúng API Key vào Unity client hay git repo.
- Hỗ trợ 2 chế độ:
  1. Live Gemini: Khi có GEMINI_API_KEY trong file .env hoặc biến môi trường.
  2. Physics Tutor Engine (High-Fidelity): Chấm từng bước theo tiêu chuẩn
     sư phạm của Giảng viên ("đến bước nào dừng bước đó", không lộ đáp án bước sau).
=============================================================================
"""

import os
import sys
import json
import time
import urllib.request
import urllib.error
from http.server import HTTPServer, BaseHTTPRequestHandler

# Cấu hình kiểm tra nghiêm ngặt (Strict Verification Mode) theo yêu cầu của sinh viên:
# Khi bật: TẠM TẮT FALLBACK CHẤM ĐIỂM NỘI BỘ khi Gemini lỗi hoặc chưa có Key.
# Chỉ chấp nhận kết quả thực sự trả về từ Google Gemini API!
STRICT_GEMINI_MODE = True
PORT = int(os.environ.get("PORT", "5050"))
ENV_FILE = os.path.join(os.path.dirname(__file__), ".env")

def get_env_config():
    """Đọc lại .env linh hoạt trên mỗi request để nhận key và model ngay lập tức."""
    if os.path.exists(ENV_FILE):
        with open(ENV_FILE, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#") and "=" in line:
                    k, v = line.split("=", 1)
                    k = k.strip()
                    v = v.strip().strip('"').strip("'")
                    if k in ["GEMINI_API_KEY", "GEMINI_MODEL"]:
                        os.environ[k] = v
    key = os.environ.get("GEMINI_API_KEY", "").strip()
    model = os.environ.get("GEMINI_MODEL", "gemini-1.5-flash").strip()
    return key, model

# Nạp dữ liệu Rubric Vật lý 9 (64 câu cũ + 256 câu mới)
sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "..", "scratch"))
try:
    from load_all_320_rubrics import load_all_rubrics
    from physics_tutor_engine import PhysicsTutorEngine
    
    ALL_STEP_DATA = load_all_rubrics()
    tutor_engine = PhysicsTutorEngine(ALL_STEP_DATA)
    print(f"[PhysicsGeminiServer] Đã nạp thành công toàn bộ {len(ALL_STEP_DATA)} rubric câu hỏi Vật lý 9.")
except Exception as e:
    print(f"[PhysicsGeminiServer] Cảnh báo nạp rubric: {e}")
    ALL_STEP_DATA = {}
    tutor_engine = None

def call_live_gemini(cau_hoi_id, question_info, student_text, api_key, model="gemini-1.5-flash"):
    """Gọi trực tiếp Google Gemini API qua endpoint chính thức."""
    steps_desc = []
    for s in question_info.get("steps", []):
        steps_desc.append(f"- Bước {s['stepId']} ({s['diem']}đ): {s['tenBuoc']}. Yêu cầu: {s['yeuCau']}. Lỗi thường gặp: {s['loiThuongGap']}")
    
    system_instruction = (
        "Bạn là Gia sư AI Vật lý 9 chấm bài tự luận theo từng bước cho học sinh.\n"
        "QUY TẮC BẮT BUỘC:\n"
        "1. Chấm bài đến bước nào thì nhận xét và cho điểm đến bước đó.\n"
        "2. Nếu học sinh làm đúng bước 1 và 2 nhưng chưa làm bước 3: Đánh dấu bước 3 là 'unassessable', điểm 0đ, KHÔNG được đưa ra đáp số của bước 3, chỉ đưa ra lời gợi ý ngắn giúp học sinh tự làm bước 3.\n"
        "3. Nếu học sinh làm sai ở một bước: Đánh dấu 'incorrect', chỉ rõ chỗ sai và gợi ý sửa bước đó trước khi sang bước sau.\n"
        "4. Điểm tối đa cả bài là 10.0đ. Chỉ cho điểm các bước học sinh đã thực hiện đúng.\n"
        "5. Phải chấp nhận các phương pháp giải tương đương nếu đúng về mặt Vật lý.\n"
        "Trả về định dạng JSON thuần túy (không dùng markdown ```json) với cấu trúc:\n"
        "{\n"
        '  "totalScore": number (thang 10),\n'
        '  "summary": string,\n'
        '  "firstIncompleteStepIndex": number (chỉ số bước chưa làm hoặc sai đầu tiên),\n'
        '  "nextStepHint": string (lời gợi ý định hướng cho bước tiếp theo mà không lộ đáp án),\n'
        '  "steps": [\n'
        '    {\n'
        '      "stepId": string,\n'
        '      "status": "correct" | "partial" | "incorrect" | "unassessable",\n'
        '      "awardedPoints": number,\n'
        '      "maxPoints": number,\n'
        '      "explanation": string,\n'
        '      "suggestedCorrection": string\n'
        "    }\n"
        "  ]\n"
        "}"
    )
    
    user_prompt = (
        f"Mã câu hỏi: {cau_hoi_id}\n"
        f"Rubric từng bước:\n" + "\n".join(steps_desc) + "\n\n"
        f"Bài làm của học sinh:\n\"\"\"{student_text}\"\"\"\n\n"
        f"Hãy chấm và nhận xét chi tiết theo đúng quy tắc Gia sư AI trên."
    )
    
    models_to_try = [model]
    if "gemini-3.5-flash-lite" not in models_to_try:
        models_to_try.append("gemini-3.5-flash-lite")
    if "gemini-3.5-flash" not in models_to_try:
        models_to_try.append("gemini-3.5-flash")

    last_error = None
    for target_model in models_to_try:
        url = f"https://generativelanguage.googleapis.com/v1beta/models/{target_model}:generateContent?key={api_key}"
        payload = {
            "contents": [
                {
                    "role": "user",
                    "parts": [{"text": system_instruction + "\n\n" + user_prompt}]
                }
            ],
            "generationConfig": {
                "temperature": 0.2,
                "responseMimeType": "application/json"
            }
        }
        
        req = urllib.request.Request(
            url,
            data=json.dumps(payload).encode("utf-8"),
            headers={"Content-Type": "application/json"},
            method="POST"
        )
        
        try:
            with urllib.request.urlopen(req, timeout=15) as resp:
                res_data = json.loads(resp.read().decode("utf-8"))
                candidate = res_data["candidates"][0]["content"]["parts"][0]["text"].strip()
                if candidate.startswith("```json"):
                    candidate = candidate[7:]
                if candidate.startswith("```"):
                    candidate = candidate[3:]
                if candidate.endswith("```"):
                    candidate = candidate[:-3]
                parsed = json.loads(candidate.strip())
                parsed["modelUsed"] = target_model
                return parsed
        except Exception as e:
            last_error = e
            print(f" -> Cảnh báo: Model {target_model} lỗi ({e}), đang thử model Gemini kế tiếp...")

    raise last_error if last_error else RuntimeError("Không thể gọi được model Gemini nào.")

def call_gemini_ocr(image_b64, mime_type, api_key, model="gemini-3.5-flash-lite"):
    """Nhận dạng chữ viết tay và công thức môn Vật lý 9 từ ảnh qua Gemini API."""
    ocr_system_instruction = (
        "Bạn là chuyên gia nhận dạng chữ viết tay và công thức môn Vật lý 9 (OCR Engine).\n"
        "Nhiệm vụ của bạn là đọc toàn bộ nội dung chữ viết tay, ký hiệu và công thức trong ảnh bài làm của học sinh.\n"
        "QUY TẮC BẮT BUỘC:\n"
        "1. Giữ nguyên tiếng Việt có dấu, chữ hoa, chữ thường và cách xuống dòng của học sinh.\n"
        "2. Giữ nguyên ký hiệu vật lý (U, I, R, t, Q, W, P, A...), đơn vị đo (V, A, Ω, J, s, W, kW...), số mũ (I², m², s²), phân số hoặc dấu nhân (*, .).\n"
        "3. Nếu có chữ hoặc công thức bị nhòe/mờ/không đọc được chắc chắn, hãy ghi '[không đọc rõ]'.\n"
        "4. TUYỆT ĐỐI KHÔNG được giải bài, KHÔNG tự ý sửa lỗi sai của học sinh, KHÔNG nhận xét đúng sai, KHÔNG thêm lời chào hay kết bài.\n"
        "5. Chỉ trả về đúng văn bản thu được từ ảnh."
    )
    
    models_to_try = [model]
    if "gemini-3.5-flash-lite" not in models_to_try:
        models_to_try.append("gemini-3.5-flash-lite")
    if "gemini-3.5-flash" not in models_to_try:
        models_to_try.append("gemini-3.5-flash")
    if "gemini-1.5-flash" not in models_to_try:
        models_to_try.append("gemini-1.5-flash")
    
    last_error = None
    for target_model in models_to_try:
        url = f"https://generativelanguage.googleapis.com/v1beta/models/{target_model}:generateContent?key={api_key}"
        payload = {
            "contents": [
                {
                    "role": "user",
                    "parts": [
                        {"text": ocr_system_instruction + "\nHãy nhận dạng toàn bộ chữ và công thức vật lý trong ảnh sau:"},
                        {
                            "inlineData": {
                                "mimeType": mime_type if mime_type else "image/png",
                                "data": image_b64
                            }
                        }
                    ]
                }
            ],
            "generationConfig": {
                "temperature": 0.1
            }
        }
        
        req = urllib.request.Request(
            url,
            data=json.dumps(payload).encode("utf-8"),
            headers={"Content-Type": "application/json"},
            method="POST"
        )
        
        try:
            with urllib.request.urlopen(req, timeout=20) as resp:
                res_data = json.loads(resp.read().decode("utf-8"))
                candidate = res_data["candidates"][0]["content"]["parts"][0]["text"].strip()
                return {"transcriptionText": candidate, "modelUsed": target_model}
        except Exception as e:
            last_error = e
            print(f" -> Cảnh báo OCR: Model {target_model} lỗi ({e}), đang thử model Gemini kế tiếp...")

    raise last_error if last_error else RuntimeError("Không thể gọi được model Gemini nào cho OCR.")

class PhysicsGeminiHandler(BaseHTTPRequestHandler):
    def _send_cors(self):
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "POST, GET, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type, Authorization")

    def do_OPTIONS(self):
        self.send_response(200)
        self._send_cors()
        self.end_headers()

    def do_GET(self):
        self.send_response(200)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self._send_cors()
        self.end_headers()
        key, model = get_env_config()
        has_key = bool(key)
        resp = {
            "status": "online",
            "service": "Aurelia Physics 9 Gemini AI Tutor Backend",
            "endpoints": {
                "grade": "/api/grade_physics",
                "ocr": "/api/ocr_physics"
            },
            "model": model,
            "hasGeminiKey": has_key,
            "strictGeminiMode": STRICT_GEMINI_MODE,
            "geminiLiveMode": has_key,
            "questionsLoaded": len(ALL_STEP_DATA)
        }
        self.wfile.write(json.dumps(resp, ensure_ascii=False, indent=2).encode("utf-8"))

    def do_POST(self):
        if self.path in ["/api/ocr_physics", "/api/ocr", "/ocr"]:
            start_time = time.time()
            content_length = int(self.headers.get("Content-Length", 0))
            body = self.rfile.read(content_length).decode("utf-8")
            
            try:
                data = json.loads(body)
            except Exception as e:
                self.send_response(400)
                self.send_header("Content-Type", "application/json")
                self._send_cors()
                self.end_headers()
                self.wfile.write(json.dumps({"success": False, "error": "INVALID_JSON"}).encode("utf-8"))
                return

            image_b64 = data.get("imageBase64") or data.get("image") or data.get("data") or ""
            mime_type = data.get("mimeType") or data.get("mime") or "image/png"
            if not image_b64:
                self.send_response(400)
                self.send_header("Content-Type", "application/json; charset=utf-8")
                self._send_cors()
                self.end_headers()
                self.wfile.write(json.dumps({"success": False, "error": "MISSING_IMAGE_DATA", "message": "Dữ liệu ảnh trống."}).encode("utf-8"))
                return

            current_key, current_model = get_env_config()
            if data.get("apiKey"):
                current_key = data.get("apiKey")
            if data.get("model"):
                current_model = data.get("model")

            print(f"\n[REQUEST] OCR bài làm học sinh ({len(image_b64)} b64 chars, {mime_type})...")

            if STRICT_GEMINI_MODE and not current_key:
                elapsed = time.time() - start_time
                print(f" -> [LỖI] Chưa có GEMINI_API_KEY! Strict Mode đang BẬT: Từ chối OCR giả lập.")
                resp_payload = {
                    "success": False,
                    "error": "GEMINI_KEY_MISSING",
                    "message": "Không thể kết nối Gemini: Chưa cấu hình GEMINI_API_KEY trong file Backend/PhysicsGeminiServer/.env hoặc biến môi trường.",
                    "source": "GEMINI_NOT_CONFIGURED",
                    "durationSeconds": round(elapsed, 2)
                }
                self.send_response(200)
                self.send_header("Content-Type", "application/json; charset=utf-8")
                self._send_cors()
                self.end_headers()
                self.wfile.write(json.dumps(resp_payload, ensure_ascii=False).encode("utf-8"))
                return

            try:
                print(f" -> [GEMINI_OCR] Đang gửi ảnh tới Google Gemini ({current_model})...")
                ocr_res = call_gemini_ocr(image_b64, mime_type, current_key, current_model)
                elapsed = time.time() - start_time
                resp_payload = {
                    "success": True,
                    "transcriptionText": ocr_res["transcriptionText"],
                    "modelUsed": ocr_res.get("modelUsed", current_model),
                    "source": "GEMINI_API",
                    "durationSeconds": round(elapsed, 2)
                }
                print(f" -> [GEMINI_OCR] Nhận dạng thành công từ Google Gemini! Thời gian: {elapsed:.2f}s | Ký tự: {len(resp_payload['transcriptionText'])}")
                self.send_response(200)
                self.send_header("Content-Type", "application/json; charset=utf-8")
                self._send_cors()
                self.end_headers()
                self.wfile.write(json.dumps(resp_payload, ensure_ascii=False).encode("utf-8"))
                return
            except Exception as ex:
                elapsed = time.time() - start_time
                print(f" -> [GEMINI_OCR] LỖI khi gọi Google Gemini OCR: {ex} (Thời gian: {elapsed:.2f}s)")
                resp_payload = {
                    "success": False,
                    "error": "GEMINI_OCR_FAILED",
                    "message": f"Không thể nhận dạng chữ qua Gemini API ({str(ex)}).",
                    "source": "GEMINI_API_FAILED",
                    "durationSeconds": round(elapsed, 2)
                }
                self.send_response(200)
                self.send_header("Content-Type", "application/json; charset=utf-8")
                self._send_cors()
                self.end_headers()
                self.wfile.write(json.dumps(resp_payload, ensure_ascii=False).encode("utf-8"))
                return

        elif self.path in ["/api/grade_physics", "/grade", "/"]:
            start_time = time.time()
            content_length = int(self.headers.get("Content-Length", 0))
            body = self.rfile.read(content_length).decode("utf-8")
            
            try:
                data = json.loads(body)
            except Exception as e:
                self.send_response(400)
                self.send_header("Content-Type", "application/json")
                self._send_cors()
                self.end_headers()
                self.wfile.write(json.dumps({"success": False, "error": "INVALID_JSON"}).encode("utf-8"))
                return
                
            cau_hoi_id = data.get("cauHoiId") or data.get("questionID") or "TL_C1_L2_Duy"
            student_text = data.get("studentText") or data.get("answerText") or ""
            current_key, current_model = get_env_config()
            if data.get("apiKey"):
                current_key = data.get("apiKey")
            if data.get("model"):
                current_model = data.get("model")
            
            print(f"\n[REQUEST] Chấm bài câu '{cau_hoi_id}' ({len(student_text)} ký tự)...")
            
            # KIỂM TRA CHẾ ĐỘ NGHIÊM NGẶT (STRICT GEMINI VERIFICATION):
            if STRICT_GEMINI_MODE:
                if not current_key:
                    elapsed = time.time() - start_time
                    print(f" -> [LỖI] Chưa có GEMINI_API_KEY! Strict Mode đang BẬT: Từ chối dùng điểm Tutor Engine.")
                    resp_payload = {
                        "success": False,
                        "error": "GEMINI_KEY_MISSING",
                        "message": "Không thể kết nối Gemini: Chưa cấu hình GEMINI_API_KEY trong file Backend/PhysicsGeminiServer/.env hoặc biến môi trường.",
                        "source": "GEMINI_NOT_CONFIGURED",
                        "durationSeconds": round(elapsed, 2)
                    }
                    self.send_response(200)
                    self.send_header("Content-Type", "application/json; charset=utf-8")
                    self._send_cors()
                    self.end_headers()
                    self.wfile.write(json.dumps(resp_payload, ensure_ascii=False).encode("utf-8"))
                    return
                
                # Có Key -> Gọi trực tiếp Google Gemini API
                try:
                    print(f" -> [GEMINI_API] Đang gửi request tới Google Gemini ({current_model})...")
                    gemini_res = call_live_gemini(cau_hoi_id, ALL_STEP_DATA.get(cau_hoi_id, {}), student_text, current_key, current_model)
                    elapsed = time.time() - start_time
                    gemini_res["success"] = True
                    gemini_res["cauHoiId"] = cau_hoi_id
                    gemini_res["mode"] = "live_gemini"
                    gemini_res["source"] = "GEMINI_API"
                    gemini_res["durationSeconds"] = round(elapsed, 2)
                    print(f" -> [GEMINI_API] Chấm thành công từ Google Gemini! Thời gian: {elapsed:.2f}s | Điểm: {gemini_res.get('totalScore')}/10")
                    
                    self.send_response(200)
                    self.send_header("Content-Type", "application/json; charset=utf-8")
                    self._send_cors()
                    self.end_headers()
                    self.wfile.write(json.dumps(gemini_res, ensure_ascii=False).encode("utf-8"))
                    return
                except Exception as ex:
                    elapsed = time.time() - start_time
                    print(f" -> [GEMINI_API] LỖI khi gọi Google Gemini: {ex} (Thời gian: {elapsed:.2f}s)")
                    resp_payload = {
                        "success": False,
                        "error": "GEMINI_CALL_FAILED",
                        "message": f"Không thể kết nối Gemini API ({str(ex)}).",
                        "source": "GEMINI_API_FAILED",
                        "durationSeconds": round(elapsed, 2)
                    }
                    self.send_response(200)
                    self.send_header("Content-Type", "application/json; charset=utf-8")
                    self._send_cors()
                    self.end_headers()
                    self.wfile.write(json.dumps(resp_payload, ensure_ascii=False).encode("utf-8"))
                    return

            # Khi STRICT_GEMINI_MODE = False (Chế độ Fallback thông thường)
            result = None
            if current_key and cau_hoi_id in ALL_STEP_DATA:
                try:
                    gemini_res = call_live_gemini(cau_hoi_id, ALL_STEP_DATA[cau_hoi_id], student_text, current_key)
                    gemini_res["success"] = True
                    gemini_res["cauHoiId"] = cau_hoi_id
                    gemini_res["mode"] = "live_gemini"
                    gemini_res["source"] = "GEMINI_API"
                    result = gemini_res
                except Exception as ex:
                    print(f" -> Lỗi Gemini: {ex}. Chuyển sang Tutor Engine...")
                    
            if not result:
                if tutor_engine:
                    result = tutor_engine.evaluate(cau_hoi_id, student_text)
                    result["mode"] = "deterministic_tutor_engine"
                    result["source"] = "LOCAL_FALLBACK"
                else:
                    result = {"success": False, "error": "ENGINE_UNAVAILABLE"}

            self.send_response(200)
            self.send_header("Content-Type", "application/json; charset=utf-8")
            self._send_cors()
            self.end_headers()
            self.wfile.write(json.dumps(result, ensure_ascii=False).encode("utf-8"))
        else:
            self.send_response(404)
            self._send_cors()
            self.end_headers()

def run_server():
    server_address = ("127.0.0.1", PORT)
    httpd = HTTPServer(server_address, PhysicsGeminiHandler)
    print("=" * 65)
    print(f" Aurelia Physics 9 Gemini AI Tutor Server đang chạy tại:")
    print(f" - Chấm bài: http://127.0.0.1:{PORT}/api/grade_physics")
    print(f" - Nhận dạng OCR: http://127.0.0.1:{PORT}/api/ocr_physics")
    current_key, current_model = get_env_config()
    print(f" Model đang dùng: {current_model}")
    print(f" Trạng thái Gemini API Key: {'ĐÃ CÓ (Sẵn sàng gọi Gemini API)' if current_key else 'CHƯA CÓ (Cần cấu hình .env)'}")
    print(f" Chế độ Strict Gemini: {'BẬT (Không tự động dùng điểm giả lập)' if STRICT_GEMINI_MODE else 'TẮT'}")
    print("=" * 65)
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nĐã dừng server.")

if __name__ == "__main__":
    run_server()
