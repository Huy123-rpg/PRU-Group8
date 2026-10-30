import os
import time
import json
import urllib.request
import urllib.error

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")

with open(ENV_FILE, "r", encoding="utf-8") as f:
    env = dict([l.strip().split("=", 1) for l in f if "=" in l and not l.strip().startswith("#")])

key = env["GEMINI_API_KEY"].strip().strip("'\"")

candidates = [
    "gemini-1.5-flash",
    "gemini-1.5-pro",
    "gemini-2.0-flash",
    "gemini-flash-latest",
    "gemini-pro-latest",
    "gemini-3.5-flash",
    "gemini-3.7-flash",
    "gemini-3.8-flash"
]

payload = {
    "contents": [{"role": "user", "parts": [{"text": "Hello, respond with 1 word: Ready"}]}]
}

working_models = []

for m in candidates:
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={key}"
    req = urllib.request.Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST"
    )
    t0 = time.time()
    try:
        with urllib.request.urlopen(req, timeout=8) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            elapsed = time.time() - t0
            ans = data["candidates"][0]["content"]["parts"][0]["text"].strip()
            print(f"Model {m}: SUCCESS ({elapsed:.2f}s) -> {ans}", flush=True)
            working_models.append((m, elapsed))
    except urllib.error.HTTPError as e:
        elapsed = time.time() - t0
        print(f"Model {m}: HTTP {e.code} ({elapsed:.2f}s)", flush=True)
    except Exception as e:
        elapsed = time.time() - t0
        print(f"Model {m}: ERROR {e} ({elapsed:.2f}s)", flush=True)

print("\nWORKING MODELS SUMMARY:", working_models, flush=True)
