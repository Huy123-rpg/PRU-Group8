import os
import json
import time
import urllib.request
import urllib.error

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")
with open(ENV_FILE, "r", encoding="utf-8") as f:
    env = dict([l.strip().split("=", 1) for l in f if "=" in l and not l.strip().startswith("#")])
key = env["GEMINI_API_KEY"].strip().strip("'\"")

def test_prompt(model, use_json_mime=True):
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={key}"
    prompt = "Chấm bài: Học sinh làm đúng bước 1: Wđ = 10 J. Trả về JSON: {\"score\": 10, \"comment\": \"Tot\"}"
    payload = {
        "contents": [{"role": "user", "parts": [{"text": prompt}]}]
    }
    if use_json_mime:
        payload["generationConfig"] = {"responseMimeType": "application/json"}
    
    req = urllib.request.Request(url, data=json.dumps(payload).encode("utf-8"), headers={"Content-Type": "application/json"}, method="POST")
    t0 = time.time()
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            ans = data["candidates"][0]["content"]["parts"][0]["text"]
            print(f"Model {model} (json_mime={use_json_mime}): SUCCESS in {time.time()-t0:.2f}s -> {ans[:80]}", flush=True)
            return True
    except urllib.error.HTTPError as e:
        print(f"Model {model} (json_mime={use_json_mime}): HTTP {e.code} ({e.reason})", flush=True)
        try:
            print("  Body:", e.read().decode("utf-8")[:200], flush=True)
        except: pass
        return False
    except Exception as e:
        print(f"Model {model} (json_mime={use_json_mime}): ERROR {e}", flush=True)
        return False

for m in ["gemini-3.5-flash-lite", "gemini-3.1-flash-lite", "gemini-3.5-flash"]:
    test_prompt(m, use_json_mime=True)
    test_prompt(m, use_json_mime=False)
