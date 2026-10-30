import os
import json
import urllib.request

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")

with open(ENV_FILE, "r", encoding="utf-8") as f:
    env = dict([l.strip().split("=", 1) for l in f if "=" in l and not l.strip().startswith("#")])

key = env["GEMINI_API_KEY"].strip().strip("'\"")
url = f"https://generativelanguage.googleapis.com/v1beta/models?key={key}"

req = urllib.request.Request(url)
with urllib.request.urlopen(req, timeout=10) as resp:
    data = json.loads(resp.read().decode("utf-8"))
    for m in data.get("models", []):
        name = m["name"].replace("models/", "")
        methods = m.get("supportedGenerationMethods", [])
        if "generateContent" in methods:
            print(f"- {name}")
