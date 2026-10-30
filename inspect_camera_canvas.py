import re

with open('Assets/Scenes/Physics/Game2/17. Type.unity', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = re.compile(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)
blocks = {fid: (cid, body) for cid, fid, body in pattern.findall(text)}

# Check Main Camera GO 592744254 components
go_cam = blocks.get('592744254')
if go_cam:
    print("=== Main Camera GO ===")
    print(go_cam[1])
    cam_comps = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', go_cam[1])
    for c in cam_comps:
        print(f"Cam Comp &{c}: Class {blocks.get(c, ('?', '?'))[0]}")
        print(blocks.get(c, ('?', '?'))[1][:300])

# Check Canvas GO 875648885 components
go_canvas = blocks.get('875648885')
if go_canvas:
    print("\n=== Canvas GO ===")
    print(go_canvas[1])
    cv_comps = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', go_canvas[1])
    for c in cv_comps:
        print(f"Canvas Comp &{c}: Class {blocks.get(c, ('?', '?'))[0]}")
        print(blocks.get(c, ('?', '?'))[1][:300])
