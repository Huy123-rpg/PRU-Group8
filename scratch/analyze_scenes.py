import re

def analyze_scene(path):
    print(f'=== ANALYZE {path} ===')
    with open(path, 'r', encoding='utf-8', errors='ignore') as f:
        c = f.read()
    
    # Split by GameObject or Component
    blocks = c.split('--- !u!')
    gameobjects = {}
    components = {}
    for b in blocks:
        if not b.strip(): continue
        lines = b.split('\n')
        header = lines[0]
        # header e.g. "1 &123456"
        parts = header.split('&')
        if len(parts) < 2: continue
        type_id = parts[0].strip()
        fid = parts[1].strip()
        
        if type_id == '1': # GameObject
            name_m = re.search(r'm_Name:\s*([^\r\n]+)', b)
            name = name_m.group(1) if name_m else 'Unknown'
            gameobjects[fid] = {'name': name, 'block': b}
        elif type_id == '114': # MonoBehaviour (e.g. TMP)
            # check script
            m_text = re.search(r'm_text:\s*([^\r\n]+)', b)
            go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
            if go_m and m_text:
                components[fid] = {'go': go_m.group(1), 'text': m_text.group(1)}

    for fid, comp in components.items():
        go_id = comp['go']
        go_name = gameobjects.get(go_id, {}).get('name', 'Unknown')
        print(f"  TMP on GameObject [{go_name}] (fid={go_id}): text={comp['text']}")

analyze_scene('Assets/Scenes/Physics/Game2/19. Result_TN.unity')
analyze_scene('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity')
