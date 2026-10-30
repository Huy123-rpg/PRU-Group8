with open('Backend/PhysicsGeminiServer/physics_gemini_server.py', 'r', encoding='utf-8') as f:
    c = f.read()

import re
matches = re.findall(r'def [^\(]+\([^\)]*\):', c)
print('Functions in server:', matches)
