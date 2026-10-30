import re

def check_button_texts(path):
    print(f'=== BUTTON TEXTS IN {path} ===')
    with open(path, 'r', encoding='utf-8', errors='ignore') as f:
        c = f.read()
    
    blocks = c.split('--- !u!')
    gameobjects = {}
    transforms = {}
    tmps = {}
    buttons = {}
    
    for b in blocks:
        if not b.strip(): continue
        lines = b.split('\n')
        parts = lines[0].split('&')
        if len(parts) < 2: continue
        type_id = parts[0].strip()
        fid = parts[1].strip()
        
        if type_id == '1': # GameObject
            m = re.search(r'm_Name:\s*([^\r\n]+)', b)
            gameobjects[fid] = m.group(1) if m else 'Unknown'
        elif type_id in ['4', '224']: # Transform
            go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
            parent_m = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', b)
            children_m = re.findall(r'-\s*\{fileID:\s*(\d+)\}', b)
            if go_m:
                transforms[fid] = {
                    'go': go_m.group(1),
                    'father': parent_m.group(1) if parent_m else None,
                    'children': children_m
                }
        elif type_id == '114':
            go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
            if 'm_Script: {fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff' in b:
                if go_m: buttons[go_m.group(1)] = True
            elif 'm_text:' in b:
                t_m = re.search(r'm_text:\s*([^\r\n]+)', b)
                if go_m and t_m:
                    tmps[go_m.group(1)] = t_m.group(1)

    for b_go in buttons:
        b_name = gameobjects.get(b_go, 'Unknown')
        # find if this go or any child has TMP
        txt = tmps.get(b_go, None)
        print(f"Button [{b_name}]: text='{txt}'")

check_button_texts('Assets/Scenes/Physics/Game2/19. Result_TN.unity')
check_button_texts('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity')
