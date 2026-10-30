with open('Backend/PhysicsGeminiServer/physics_gemini_server.py', 'r', encoding='utf-8') as f:
    lines = f.readlines()

for i, l in enumerate(lines):
    if 'KNOWN_QUESTIONS' in l:
        print(''.join(lines[i:i+40]))
        break
