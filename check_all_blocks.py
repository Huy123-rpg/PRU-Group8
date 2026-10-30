import re

with open('Assets/Scenes/Physics/Game2/17. Type.unity', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = re.compile(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)
blocks = pattern.findall(text)

print(f"Total blocks in 17. Type.unity: {len(blocks)}")
class_counts = {}
for cid, fid, body in blocks:
    class_counts[cid] = class_counts.get(cid, 0) + 1

print("Class counts:", class_counts)

# List all GameObjects (cid == 1)
print("\nAll GameObjects:")
for cid, fid, body in blocks:
    if cid == '1':
        name = re.search(r'm_Name:\s*(.*)', body).group(1).strip()
        print(f"  GO &{fid}: '{name}'")
