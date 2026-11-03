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
from http.server import HTTPServer, ThreadingHTTPServer, BaseHTTPRequestHandler

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
    
    fast_models = ["gemini-3-flash-preview", "gemini-3.5-flash", "gemini-3.1-flash-lite", "gemini-3.5-flash-lite"]
    models_to_try = [model] if model in fast_models else ["gemini-3-flash-preview"]
    for m in fast_models:
        if m not in models_to_try:
            models_to_try.append(m)

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

def call_live_gemini_tutor_chat(cau_hoi_id, question_info, current_essay, student_message, active_step_id, chat_history, api_key, model="gemini-3.5-flash-lite"):
    """
    Gia sư AI Aurelia tương tác hai chiều sửa bài từng bước qua Google Gemini API.
    """
    steps_desc = []
    for s in question_info.get("steps", []):
        sid = s.get("stepId", "")
        diem = s.get("diem", 0)
        ten = s.get("tenBuoc", "")
        yc = s.get("yeuCau", "")
        loi = s.get("loiThuongGap", "")
        dap_an = s.get("noiDung", "")
        steps_desc.append(f"- Bước {sid} ({diem}đ): {ten}. Yêu cầu: {yc}. Lỗi thường gặp: {loi}. Đáp án chuẩn: {dap_an}")

    system_instruction = (
        "Bạn là Gia sư AI Vật lý 9 (Aurelia Academy) tương tác hai chiều trực tiếp với học sinh để hướng dẫn giải bài tự luận.\n"
        "Nhiệm vụ: Hướng dẫn học sinh sửa bài và hoàn thiện bài tự luận TỪNG BƯỚC theo đúng Rubric chuẩn.\n\n"
        "QUY TẮC SƯ PHẠM VÀ HƯỚNG DẪN BẮT BUỘC:\n"
        "1. ĐẾN BƯỚC NÀO HƯỚNG DẪN BƯỚC ĐÓ. Tuyệt đối KHÔNG đưa ra đáp án trực tiếp của bước tiếp theo.\n"
        "2. Trường hợp MỞ ĐẦU (khi studentMessage rỗng hoặc mang tính bắt đầu/chào hỏi):\n"
        "   - Đánh giá bài làm hiện tại của học sinh theo từng bước trong Rubric.\n"
        "   - Nhận xét ngắn gọn các bước học sinh đã làm đúng (nếu có).\n"
        "   - Xác định bước đầu tiên bị sai hoặc chưa làm (activeStepId).\n"
        "   - Đưa ra lời chào và câu hỏi/gợi ý hướng dẫn học sinh làm/sửa bước đó.\n"
        "3. Trường hợp HỌC SINH GỬI CÂU TRẢ LỜI / SỬA BÀI (studentMessage có nội dung thực tế):\n"
        "   - Đánh giá câu trả lời của học sinh cho bước đang làm (activeStepId).\n"
        "   - NẾU ĐÚNG:\n"
        "     + Đánh dấu bước đó là 'correct'.\n"
        "     + Khen ngợi ngắn gọn, giải thích vì sao đúng.\n"
        "     + Kiểm tra xem còn bước nào chưa hoàn thành không:\n"
        "       * Nếu còn bước tiếp theo: chuyển activeStepId sang bước đó và gợi ý định hướng cho bước tiếp theo (không lộ đáp án).\n"
        "       * Nếu TẤT CẢ các bước đều đã đúng: đặt isAllCompleted = true và chúc mừng học sinh đã hoàn thành toàn bộ bài giải.\n"
        "     + Cập nhật bài làm (updatedFullEssay): Tạo ra văn bản bài làm tổng hợp hoàn chỉnh từ bài làm ban đầu kết hợp với phần sửa đúng vừa được chấp nhận. Giữ nguyên các phần học sinh đã làm đúng trước đó.\n"
        "   - NẾU SAI HOẶC CHƯA ĐỦ:\n"
        "     + Đánh dấu bước đó là 'incorrect' hoặc 'partial'.\n"
        "     + Giữ nguyên activeStepId ở bước hiện tại.\n"
        "     + Giải thích rõ ràng lỗi sai về mặt Vật lý, gợi ý định hướng để học sinh tự suy nghĩ và sửa lại. KHÔNG tự động coi là đúng!\n"
        "     + updatedFullEssay giữ nguyên bài làm hiện tại (không ghép nội dung sai vào).\n"
        "     + isAllCompleted = false.\n"
        "4. TUYỆT ĐỐI không ép số bước cố định (phải theo đúng số bước trong Rubric của câu hỏi này).\n"
        "5. Chấp nhận các cách viết, cách lập luận và công thức tương đương đúng về mặt Vật lý.\n\n"
        "Trả về DUY NHẤT định dạng JSON thuần túy (không markdown):\n"
        "{\n"
        '  "isCurrentStepCorrect": bool,\n'
        '  "assessedStepId": string,\n'
        '  "tutorReply": string,\n'
        '  "isAllCompleted": bool,\n'
        '  "activeStepId": string,\n'
        '  "updatedFullEssay": string,\n'
        '  "steps": [\n'
        "    {\n"
        '      "stepId": string,\n'
        '      "status": "correct" | "incorrect" | "partial" | "unassessable",\n'
        '      "tenBuoc": string,\n'
        '      "explanation": string\n'
        "    }\n"
        "  ]\n"
        "}"
    )

    history_str = ""
    if chat_history and isinstance(chat_history, list):
        h_lines = []
        for msg in chat_history[-6:]:
            role = "Gia sư AI" if msg.get("role") in ["assistant", "model", "ai", "tutor"] else "Học sinh"
            text = msg.get("text", "").strip()
            if text:
                h_lines.append(f"{role}: {text}")
        if h_lines:
            history_str = "\n".join(h_lines)

    is_init = not student_message or student_message.strip().lower() in ["init", "bắt đầu", "bat dau", "start", ""]
    msg_prompt = "Lượt bắt đầu tương tác (học sinh chưa gửi tin nhắn sửa mới). Hãy đánh giá bài làm hiện tại, xác định bước cần sửa và đưa ra lời chào cùng gợi ý bắt đầu cho học sinh." if is_init else f'Tin nhắn mới nhất của học sinh:\n"""{student_message}"""'

    user_prompt = (
        f"Mã câu hỏi: {cau_hoi_id}\n"
        f"Rubric từng bước:\n" + "\n".join(steps_desc) + "\n\n"
        f"Bài làm hiện tại của học sinh:\n\"\"\"{current_essay}\"\"\"\n\n"
        f"Bước đang được hướng dẫn (activeStepId): {active_step_id or 'Chưa xác định'}\n"
        f"Lịch sử trao đổi gần nhất:\n{history_str or '(Chưa có trao đổi trước đó)'}\n\n"
        f"{msg_prompt}\n\n"
        f"Hãy đánh giá và trả về JSON theo đúng quy định."
    )

    fast_models = ["gemini-3-flash-preview", "gemini-3.5-flash", "gemini-3.1-flash-lite", "gemini-3.5-flash-lite"]
    models_to_try = [model] if model in fast_models else ["gemini-3-flash-preview"]
    for m in fast_models:
        if m not in models_to_try:
            models_to_try.append(m)

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
            print(f" -> Cảnh báo Tutor Chat: Model {target_model} lỗi ({e}), đang thử model Gemini kế tiếp...")

    raise last_error if last_error else RuntimeError("Không thể gọi được model Gemini nào cho Tutor Chat.")

def call_gemini_ocr(image_b64, mime_type, api_key, model="gemini-3.1-flash-lite"):
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
    
    fast_models = ["gemini-3-flash-preview", "gemini-3.5-flash", "gemini-3.1-flash-lite", "gemini-3.5-flash-lite"]
    models_to_try = [model] if model in fast_models else ["gemini-3-flash-preview"]
    for m in fast_models:
        if m not in models_to_try:
            models_to_try.append(m)
    
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
            with urllib.request.urlopen(req, timeout=15) as resp:
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

    def _safe_send_json(self, payload, code=200):
        try:
            body = json.dumps(payload, ensure_ascii=False).encode("utf-8")
            self.send_response(code)
            self.send_header("Content-Type", "application/json; charset=utf-8")
            self.send_header("Content-Length", str(len(body)))
            self._send_cors()
            self.end_headers()
            self.wfile.write(body)
        except (ConnectionResetError, ConnectionAbortedError, BrokenPipeError):
            print(" -> [SOCKET] Client disconnected before response could be sent.")
        except Exception as e:
            print(f" -> [SOCKET] Lỗi socket: {e}")

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
                "ocr": "/api/ocr_physics",
                "tutor_chat": "/api/tutor_chat"
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
                self._safe_send_json(resp_payload)
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
                self._safe_send_json(resp_payload)
                return

        elif self.path in ["/api/tutor_chat", "/tutor_chat", "/api/chat"]:
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

            cau_hoi_id = data.get("cauHoiId") or "TL_C1_L2_Duy"
            current_essay = data.get("currentEssay") or ""
            student_message = data.get("studentMessage") or ""
            active_step_id = data.get("activeStepId") or ""
            chat_history = data.get("chatHistory") or []

            current_key, current_model = get_env_config()
            if data.get("apiKey"):
                current_key = data.get("apiKey")
            if data.get("model"):
                current_model = data.get("model")

            print(f"\n[REQUEST] Tutor Chat câu '{cau_hoi_id}' (Step: {active_step_id or 'auto'}, Msg: {len(student_message)} ký tự)...")

            if not current_key:
                elapsed = time.time() - start_time
                print(" -> [LỖI] Chưa có GEMINI_API_KEY cho Tutor Chat!")
                resp_payload = {
                    "success": False,
                    "error": "GEMINI_KEY_MISSING",
                    "message": "Chưa cấu hình GEMINI_API_KEY trong file Backend/PhysicsGeminiServer/.env hoặc biến môi trường.",
                    "source": "GEMINI_NOT_CONFIGURED",
                    "durationSeconds": round(elapsed, 2)
                }
                self._safe_send_json(resp_payload)
                return

            try:
                print(f" -> [TUTOR_CHAT] Đang gửi tương tác tới Google Gemini ({current_model})...")
                q_info = ALL_STEP_DATA.get(cau_hoi_id, {})
                chat_res = call_live_gemini_tutor_chat(
                    cau_hoi_id=cau_hoi_id,
                    question_info=q_info,
                    current_essay=current_essay,
                    student_message=student_message,
                    active_step_id=active_step_id,
                    chat_history=chat_history,
                    api_key=current_key,
                    model=current_model
                )
                elapsed = time.time() - start_time
                chat_res["success"] = True
                chat_res["cauHoiId"] = cau_hoi_id
                chat_res["source"] = "GEMINI_API"
                chat_res["durationSeconds"] = round(elapsed, 2)
                print(f" -> [TUTOR_CHAT] Phản hồi thành công từ Google Gemini! Thời gian: {elapsed:.2f}s | Step: {chat_res.get('assessedStepId')} (Correct: {chat_res.get('isCurrentStepCorrect')})")
                self._safe_send_json(chat_res)
                return
            except Exception as ex:
                elapsed = time.time() - start_time
                print(f" -> [TUTOR_CHAT] LỖI khi gọi Google Gemini: {ex} (Thời gian: {elapsed:.2f}s)")
                resp_payload = {
                    "success": False,
                    "error": "GEMINI_CALL_FAILED",
                    "message": f"Không thể kết nối Gemini API ({str(ex)}).",
                    "source": "GEMINI_API_FAILED",
                    "durationSeconds": round(elapsed, 2)
                }
                self._safe_send_json(resp_payload)
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
                    self._safe_send_json(resp_payload)
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
                    self._safe_send_json(gemini_res)
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
                    self._safe_send_json(resp_payload)
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

class ThreadedPhysicsServer(ThreadingHTTPServer):
    allow_reuse_address = True
    daemon_threads = True

def run_server():
    server_address = ("127.0.0.1", PORT)
    httpd = ThreadedPhysicsServer(server_address, PhysicsGeminiHandler)
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
