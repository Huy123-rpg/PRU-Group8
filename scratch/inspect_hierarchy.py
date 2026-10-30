import re

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    text = f.read()

docs = text.split('--- !u!')

game_objects = {}
rect_transforms = {}
components = {}

for doc in docs:
    lines = doc.strip().split('\n')
    if not lines or not lines[0]:
        continue
    m = re.match(r'(\d+)\s+&(\d+)', lines[0])
    if not m:
        continue
    type_id, file_id = m.groups()
    
    if type_id == '1': # GameObject
        name_m = re.search(r'm_Name:\s*(.+)', doc)
        comps = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', doc)
        game_objects[file_id] = {
            'name': name_m.group(1).strip() if name_m else "Unknown",
            'comps': comps
        }
    elif type_id == '224': # RectTransform
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
        pos_m = re.search(r'm_AnchoredPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', doc)
        size_m = re.search(r'm_SizeDelta:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', doc)
        father_m = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', doc)
        children = re.findall(r'-\s*\{fileID:\s*(\d+)\}', doc)
        if go_m:
            rect_transforms[file_id] = {
                'go_id': go_m.group(1),
                'x': float(pos_m.group(1)) if pos_m else 0,
                'y': float(pos_m.group(2)) if pos_m else 0,
                'w': float(size_m.group(1)) if size_m else 0,
                'h': float(size_m.group(2)) if size_m else 0,
                'father': father_m.group(1) if father_m else "0",
                'children': children
            }
    elif type_id == '114': # MonoBehaviour
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
        script_m = re.search(r'm_Script:\s*\{fileID:\s*([^,]+),\s*guid:\s*([^,]+)', doc)
        class_m = re.search(r'm_EditorClassIdentifier:\s*(.+)', doc)
        raycast_m = re.search(r'm_RaycastTarget:\s*(\d+)', doc)
        text_m = re.search(r'm_text:\s*(.*)', doc)
        if go_m:
            components[file_id] = {
                'go_id': go_m.group(1),
                'class': class_m.group(1).strip() if class_m else "MonoBehaviour",
                'raycast': raycast_m.group(1) if raycast_m else None,
                'text': text_m.group(1) if text_m else None
            }

# Find canvas
canvas_rt_id = None
for rt_id, rt in rect_transforms.items():
    go_id = rt['go_id']
    if game_objects.get(go_id, {}).get('name') == 'Canvas':
        canvas_rt_id = rt_id
        break

print(f"Canvas RT ID: {canvas_rt_id}")

def print_tree(rt_id, indent=0):
    rt = rect_transforms.get(rt_id)
    if not rt:
        return
    go_id = rt['go_id']
    go = game_objects.get(go_id, {})
    name = go.get('name', 'Unknown')
    
    # Check components
    comp_desc = []
    for c_id in go.get('comps', []):
        if c_id in components:
            c = components[c_id]
            rc = f"raycast={c['raycast']}" if c['raycast'] is not None else ""
            tx = f"text='{c['text']}'" if c['text'] is not None else ""
            comp_desc.append(f"{c['class']} ({rc} {tx})".strip())
    
    pos_str = f"pos=({rt['x']:.1f}, {rt['y']:.1f}) size=({rt['w']:.1f}x{rt['h']:.1f})"
    print("  " * indent + f"- [{go_id}] '{name}' {pos_str} -> {', '.join(comp_desc)}")
    for child_rt_id in rt['children']:
        print_tree(child_rt_id, indent + 1)

if canvas_rt_id:
    print_tree(canvas_rt_id)
