import os
import time
import json
import urllib.request
import urllib.error

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")

with open(ENV_FILE, "r", encoding="utf-8") as f:
    env = dict([l.strip().split("=", 1) for l in f if "=" in l and not l.strip().startswith("#")])

key = env["GEMINI_API_KEY"].strip().strip("'\"")

def test_model(model_name):
    print(f"\n--- Testing model: {model_name} ---")
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{model_name}:generateContent?key={key}"
    payload = {
        "contents": [{"role": "user", "parts": [{"text": "Hello, answer in 1 word: Ready"}]}]
    }
    req = urllib.request.Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST"
    )
    t0 = time.time()
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            elapsed = time.time() - t0
            ans = data["candidates"][0]["content"]["parts"][0]["text"].strip()
            print(f"Result: SUCCESS in {elapsed:.2f}s! Response: {ans}")
            return True, elapsed
    except Exception as e:
        elapsed = time.time() - t0
        print(f"Result: FAILED in {elapsed:.2f}s! Error: {e}")
        return False, elapsed

test_model("gemini-2.5-flash")
test_model("gemini-3.8-flash")
