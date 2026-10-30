import os
import json
import urllib.request
import urllib.error

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")

def check_gemini():
    env = {}
    if os.path.exists(ENV_FILE):
        with open(ENV_FILE, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#") and "=" in line:
                    k, v = line.split("=", 1)
                    env[k.strip()] = v.strip().strip("'\"")

    key = env.get("GEMINI_API_KEY", "").strip()
    configured_model = env.get("GEMINI_MODEL", "gemini-1.5-flash").strip()

    print(f"File .env tồn tại: {os.path.exists(ENV_FILE)}")
    print(f"Cấu hình GEMINI_MODEL: {configured_model}")
    print(f"Độ dài GEMINI_API_KEY: {len(key)} ký tự (Khóa được bảo mật)")

    if not key:
        print("LỖI: Chưa tìm thấy GEMINI_API_KEY!")
        return

    # 1. Liệt kê các model khả dụng cho API Key này
    list_url = f"https://generativelanguage.googleapis.com/v1beta/models?key={key}"
    try:
        req = urllib.request.Request(list_url)
        with urllib.request.urlopen(req, timeout=10) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            all_models = [m["name"].replace("models/", "") for m in data.get("models", [])]
            supported_methods = {m["name"].replace("models/", ""): m.get("supportedGenerationMethods", []) for m in data.get("models", [])}
            print(f"\n[GOOGLE API] Kết nối thành công! Tổng số models: {len(all_models)}")
            
            # Kiểm tra xem configured_model có tồn tại không
            if configured_model in all_models:
                print(f"-> Model '{configured_model}' CÓ KHẢ DỤNG và hợp lệ với key này!")
            else:
                print(f"-> CẢNH BÁO: Model '{configured_model}' KHÔNG CÓ trong danh sách models của Google API!")
                # Tìm các model gemini tương tự
                gemini_models = [m for m in all_models if "generateContent" in supported_methods.get(m, []) and "gemini" in m]
                print(f"-> Các model Gemini hỗ trợ generateContent hợp lệ: {gemini_models[:8]}")

    except urllib.error.HTTPError as e:
        print(f"[GOOGLE API] LỖI HTTP: {e.code} - {e.reason}")
        try:
            err_body = e.read().decode("utf-8")
            err_json = json.loads(err_body)
            print(f"Chi tiết lỗi từ Google: {err_json.get('error', {}).get('message')}")
        except:
            pass
    except Exception as e:
        print(f"[GOOGLE API] Lỗi kết nối: {e}")

if __name__ == "__main__":
    check_gemini()
