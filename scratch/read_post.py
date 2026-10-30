with open('Backend/PhysicsGeminiServer/physics_gemini_server.py', 'r', encoding='utf-8') as f:
    lines = f.readlines()

for i, l in enumerate(lines):
    if 'def do_POST' in l:
        print(''.join(lines[i:i+60]))
        break
