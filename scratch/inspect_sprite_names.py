import re

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's read meta file for 71c7be75 and 5e5c3162 to find sprite names
def get_sprite_names(meta_path):
    with open(meta_path, 'r', encoding='utf-8') as mf:
        content = mf.read()
    # match fileID to name
    # e.g. - first: {213: 21300000} \n second: name
    sprites = {}
    matches = re.findall(r'fileID:\s*(-?\d+)\s*\}\s*\n\s*name:\s*(.+)', content)
    for fid, name in matches:
        sprites[fid] = name
    # Also check internalIDToNameTable
    matches2 = re.findall(r'first:\s*\{\s*213:\s*(-?\d+)\s*\}\s*\n\s*second:\s*(.+)', content)
    for fid, name in matches2:
        sprites[fid] = name
    return sprites

atlas_meta = 'Assets/Art/Environment/Atlas chữ kết quả Aurelia Academy.png.meta'
sprites_meta = 'Assets/Art/Environment/Bộ sprite giao diện Unity xanh vàng.png.meta'

atlas_sprites = get_sprite_names(atlas_meta)
ui_sprites = get_sprite_names(sprites_meta)

docs = text.split('--- !u!')
for doc in docs:
    m = re.search(r'm_Name:\s*(.+)', doc)
    if not m: continue
    name = m.group(1).strip()
    sp_m = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([^,]+)', doc)
    if sp_m:
        fid, guid = sp_m.group(1), sp_m.group(2)
        sp_name = atlas_sprites.get(fid, ui_sprites.get(fid, f"Unknown_{fid}"))
        print(f"'{name}' -> Sprite: '{sp_name}' (guid={guid[:8]})")
