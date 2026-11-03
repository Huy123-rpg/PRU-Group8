import re

with open('Assets/Scenes/Physics/Game2/17. Type.unity', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = re.compile(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)
blocks = {fid: (cid, body) for cid, fid, body in pattern.findall(text)}

# Collect all transforms (class 4 and 224)
transforms = {}
for fid, (cid, body) in blocks.items():
    if cid in ['4', '224']:
        father_m = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', body)
        children = re.findall(r'- \{fileID:\s*(\d+)\}', re.search(r'm_Children:.*?(?=\n  m_Father:|\n  m_RootOrder:|\n  m_LocalEulerAnglesHint:|\n  m_AnchorMin:|\Z)', body, re.DOTALL).group(0)) if 'm_Children:' in body else []
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', body)
        go_id = go_m.group(1) if go_m else '?'
        go_name = '?'
        if go_id in blocks:
            name_m = re.search(r'm_Name:\s*(.*)', blocks[go_id][1])
            go_name = name_m.group(1).strip() if name_m else '?'
        father = father_m.group(1) if father_m else '0'
        transforms[fid] = {
            'cid': cid,
            'go_id': go_id,
            'go_name': go_name,
            'father': father,
            'children': children
        }

print(f"Total transforms: {len(transforms)}")

# Check parent-child bidirectional consistency:
errors = []
for fid, t in transforms.items():
    # 1. For each child in t['children'], verify that child exists and has father == fid
    for c in t['children']:
        if c not in transforms:
            errors.append(f"Transform &{fid} ('{t['go_name']}') lists child &{c} which DOES NOT EXIST!")
        else:
            if transforms[c]['father'] != fid:
                errors.append(f"Transform &{fid} ('{t['go_name']}') lists child &{c} ('{transforms[c]['go_name']}'), but child's father is &{transforms[c]['father']}!")
                
    # 2. For father, if father != '0', verify that father exists and lists fid in its children
    if t['father'] != '0':
        if t['father'] not in transforms:
            errors.append(f"Transform &{fid} ('{t['go_name']}') has father &{t['father']} which DOES NOT EXIST!")
        else:
            if fid not in transforms[t['father']]['children']:
                errors.append(f"Transform &{fid} ('{t['go_name']}') has father &{t['father']} ('{transforms[t['father']]['go_name']}'), but father DOES NOT list this child!")

print(f"\nTotal hierarchy consistency errors: {len(errors)}")
for e in errors:
    print("  ERROR:", e)
