import os
import json
import urllib.request
import urllib.error

ENV_FILE = os.path.join(os.path.dirname(__file__), "..", "Backend", "PhysicsGeminiServer", ".env")

def test_direct():
    env = {}
    with open(ENV_FILE, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                k, v = line.split("=", 1)
                env[k.strip()] = v.strip().strip("'\"")

    key = env.get("GEMINI_API_KEY", "").strip()
    model = env.get("GEMINI_MODEL", "gemini-3.8-flash").strip()

    print(f"Testing direct call to model: {model} ...")
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={key}"
    
    payload = {
        "contents": [
            {
                "role": "user",
                "parts": [{"text": "Xin chao, hay tra loi bang 1 tu: OK"}]
            }
        ]
    }
    
    req = urllib.request.Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST"
    )
    
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            text = data["candidates"][0]["content"]["parts"][0]["text"]
            print("Direct Call: SUCCESS!")
            print("Response text:", text.strip())
    except urllib.error.HTTPError as e:
        print(f"HTTP Error {e.code}: {e.reason}")
        try:
            err_json = json.loads(e.read().decode("utf-8"))
            print("Google Error Message:", err_json.get("error", {}).get("message"))
            print("Google Error Status:", err_json.get("error", {}).get("status"))
        except Exception as read_err:
            print("Could not read error body:", read_err)
    except Exception as e:
        print("General Error:", e)

if __name__ == "__main__":
    test_direct()
