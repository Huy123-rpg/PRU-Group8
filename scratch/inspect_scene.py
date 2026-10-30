import re

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Split into YAML documents
docs = text.split('--- !u!')

game_objects = {}
rect_transforms = {}
tmp_texts = {}
images = {}
canvas_children = []

for doc in docs:
    lines = doc.strip().split('\n')
    if not lines or not lines[0]:
        continue
    header = lines[0] # e.g. "1 &12345"
    m = re.match(r'(\d+)\s+&(\d+)', header)
    if not m:
        continue
    type_id, file_id = m.groups()
    
    if type_id == '1': # GameObject
        name_m = re.search(r'm_Name:\s*(.+)', doc)
        name = name_m.group(1).strip() if name_m else "Unknown"
        game_objects[file_id] = {'name': name, 'doc': doc}
    elif type_id == '224': # RectTransform
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
        pos_m = re.search(r'm_AnchoredPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', doc)
        size_m = re.search(r'm_SizeDelta:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', doc)
        parent_m = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', doc)
        if go_m:
            rect_transforms[file_id] = {
                'go_id': go_m.group(1),
                'x': float(pos_m.group(1)) if pos_m else 0,
                'y': float(pos_m.group(2)) if pos_m else 0,
                'w': float(size_m.group(1)) if size_m else 0,
                'h': float(size_m.group(2)) if size_m else 0,
                'parent': parent_m.group(1) if parent_m else "0"
            }
    elif type_id == '114': # MonoBehaviour (TMP or Image or Button)
        if 'TMPro.TextMeshProUGUI' in doc:
            go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
            text_m = re.search(r'm_text:\s*(.*)', doc)
            if go_m:
                tmp_texts[go_m.group(1)] = text_m.group(1) if text_m else ""
        elif 'UnityEngine.UI.Image' in doc:
            go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
            raycast_m = re.search(r'm_RaycastTarget:\s*(\d+)', doc)
            sprite_m = re.search(r'm_Sprite:\s*\{fileID:\s*([^,]+),\s*guid:\s*([^,]+)', doc)
            if go_m:
                images[go_m.group(1)] = {
                    'raycast': raycast_m.group(1) if raycast_m else '?',
                    'sprite_guid': sprite_m.group(2) if sprite_m else ''
                }

print(f"Total GameObjects: {len(game_objects)}")
print("\n--- BUTTONS & UI OBJECTS ---")
for rt_id, rt in rect_transforms.items():
    go_id = rt['go_id']
    go = game_objects.get(go_id, {})
    go_name = go.get('name', 'Unknown')
    txt = tmp_texts.get(go_id, '')
    img = images.get(go_id, {})
    
    # Check if this GO has button
    has_button = 'UnityEngine.UI.Button' in go.get('doc', '')
    if 'Button' in go_name or has_button or 'Text' in go_name or abs(rt['y']) > 300:
        print(f"GO [{go_id}] '{go_name}' | pos=({rt['x']}, {rt['y']}) size=({rt['w']}x{rt['h']}) button={has_button} txt='{txt}' img_raycast={img.get('raycast')}")
