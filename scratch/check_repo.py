import re

with open('Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's see some lessons and their counts
print("Repository file loaded, length:", len(text))
