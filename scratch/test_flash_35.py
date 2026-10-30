import os
import time
import json
import urllib.request
import urllib.error

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")

with open(ENV_FILE, "r", encoding="utf-8") as f:
    env = dict([l.strip().split("=", 1) for l in f if "=" in l and not l.strip().startswith("#")])

key = env["GEMINI_API_KEY"].strip().strip("'\"")

for m in ["gemini-3.5-flash", "gemini-3.5-flash-lite", "gemini-3.1-flash-lite"]:
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={key}"
    payload = {"contents": [{"role": "user", "parts": [{"text": "Hello, respond with 1 word: Ready"}]}]}
    req = urllib.request.Request(url, data=json.dumps(payload).encode("utf-8"), headers={"Content-Type": "application/json"}, method="POST")
    t0 = time.time()
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            ans = data["candidates"][0]["content"]["parts"][0]["text"].strip()
            print(f"Model {m}: SUCCESS in {time.time()-t0:.2f}s! Ans: {ans}", flush=True)
    except Exception as e:
        print(f"Model {m}: FAILED in {time.time()-t0:.2f}s! Error: {e}", flush=True)
