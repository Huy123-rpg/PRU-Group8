import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    content = f.read()

# find all GameObjects
gos = re.findall(r'--- !u!1 &(\d+)\nGameObject:.*?m_Name: ([^\n]+)', content, re.DOTALL)
print(f'Total GameObjects: {len(gos)}')
for gid, name in gos:
    rt_m = re.search(r'--- !u!224 &\d+\nRectTransform:.*?m_GameObject: \{fileID: ' + gid + r'\}.*?m_AnchoredPosition: \{x: ([^,]+), y: ([^\}]+)\}.*?m_SizeDelta: \{x: ([^,]+), y: ([^\}]+)\}', content, re.DOTALL)
    rt_str = f'pos=({rt_m.group(1)}, {rt_m.group(2)}) size=({rt_m.group(3)}, {rt_m.group(4)})' if rt_m else ''
    mb_m = re.search(r'--- !u!114 &\d+\nMonoBehaviour:.*?m_GameObject: \{fileID: ' + gid + r'\}.*?m_[Tt]ext: ([^\n]+)', content, re.DOTALL)
    text_str = f'text="{mb_m.group(1)}"' if mb_m else ''
    if text_str or 'Button' in name or 'Image' in name or gid in ['164711255']:
        print(f'GO {gid}: "{name}" {rt_str} {text_str}')
