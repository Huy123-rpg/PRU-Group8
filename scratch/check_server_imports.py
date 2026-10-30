with open('Backend/PhysicsGeminiServer/physics_gemini_server.py', 'r', encoding='utf-8') as f:
    lines = f.readlines()

for i, l in enumerate(lines[:50]):
    print(f'{i+1}: {l.strip()}')
