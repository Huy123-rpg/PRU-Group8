with open('Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs', 'r', encoding='utf-8') as f:
    text = f.read()

import re
print("Length of GameDataRepository.cs:", len(text))
m = re.search(r'public static string NormalizeDifficulty.*?\{.*?\}', text, re.DOTALL)
if m:
    print("Found NormalizeDifficulty:")
    print(m.group(0))
