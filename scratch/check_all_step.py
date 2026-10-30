with open('Backend/PhysicsGeminiServer/physics_gemini_server.py', 'r', encoding='utf-8') as f:
    lines = f.readlines()

for i, l in enumerate(lines):
    if 'ALL_STEP_DATA' in l:
        print(''.join(lines[i:i+40]))
        break
