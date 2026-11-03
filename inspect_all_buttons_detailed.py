import re

with open('Assets/Scenes/Physics/Game2/17. Type.unity', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = re.compile(r'--- !u!114 &(\d+)\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)
for fid, body in pattern.findall(text):
    if 'guid: 4e29b1a8efbd4b44bb3f3716e73f07ff' in body:
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        go_id = go_m.group(1) if go_m else '?'
        print(f"=== Button fid &{fid} on GO &{go_id} ===")
        # print first 15 lines of body
        print('\n'.join(body.splitlines()[:20]))
