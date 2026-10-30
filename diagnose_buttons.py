import re

with open('Assets/Scenes/Physics/Game2/17. Type.unity', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = re.compile(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)
blocks = pattern.findall(text)

data = {}
for class_id, file_id, body in blocks:
    data[file_id] = {'class_id': class_id, 'body': body}

# Find all GameObjects and their components
gos = {}
for fid, item in data.items():
    if item['class_id'] == '1':
        name_m = re.search(r'm_Name:\s*(.*)', item['body'])
        name = name_m.group(1).strip() if name_m else '?'
        comps = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', item['body'])
        gos[fid] = {'name': name, 'components': comps}

print(f"Total GameObjects: {len(gos)}")

# Find all Buttons
buttons = []
for fid, item in data.items():
    if item['class_id'] == '114' and 'guid: 4e29b1a8efbd4b44bb3f3716e73f07ff' in item['body']:
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', item['body'])
        go_id = go_m.group(1) if go_m else None
        inter_m = re.search(r'm_Interactable:\s*(\d+)', item['body'])
        tg_m = re.search(r'm_TargetGraphic:\s*\{fileID:\s*(\d+)\}', item['body'])
        
        # Check if this Button is in its GameObject's component list!
        in_go = False
        go_name = "UNKNOWN_GO"
        if go_id and go_id in gos:
            go_name = gos[go_id]['name']
            in_go = fid in gos[go_id]['components']
            
        buttons.append({
            'fid': fid,
            'go_id': go_id,
            'go_name': go_name,
            'in_go_components': in_go,
            'interactable': inter_m.group(1) if inter_m else '?',
            'target_graphic': tg_m.group(1) if tg_m else '?'
        })

print(f"\nTotal Button components: {len(buttons)}")
for b in buttons:
    status = "OK" if b['in_go_components'] else "DANGLING/ORPHANED!!!"
    print(f"Button {b['fid']} on GO {b['go_id']} ('{b['go_name']}'): in_go={b['in_go_components']} [{status}], inter={b['interactable']}, tg={b['target_graphic']}")
