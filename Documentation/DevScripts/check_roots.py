import re

with open('Assets/Scenes/Physics/Game2/17. Type.unity', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = re.compile(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)
blocks = {fid: (cid, body) for cid, fid, body in pattern.findall(text)}

roots = []
for fid, (cid, body) in blocks.items():
    if cid in ['4', '224']: # Transform or RectTransform
        father_m = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', body)
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        if father_m and father_m.group(1) == '0':
            go_id = go_m.group(1) if go_m else '?'
            go_name = '?'
            if go_id in blocks:
                name_m = re.search(r'm_Name:\s*(.*)', blocks[go_id][1])
                go_name = name_m.group(1).strip() if name_m else '?'
            roots.append((fid, go_id, go_name))

print(f"Total root GameObjects: {len(roots)}")
for r in roots:
    print(f"  Root Transform &{r[0]} on GO &{r[1]} '{r[2]}'")
