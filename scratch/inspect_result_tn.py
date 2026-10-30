import re

with open('Assets/Scenes/Physics/Game2/19. Result_TN.unity', 'r', encoding='utf-8') as f:
    content = f.read()

# Let's inspect SceneRoots
roots_block = re.search(r'SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n(.*?)(?=\n---|\Z)', content, re.DOTALL)
if roots_block:
    print('SceneRoots:\n', roots_block.group(1))

# Let's list all Transform and RectTransform blocks
t_blocks = re.findall(r'--- !u!(4|224) &(\d+)\n(.*?)(?=\n---|\Z)', content, re.DOTALL)
for ttype, tid, body in t_blocks:
    go = re.search(r'm_GameObject: \{fileID: (\d+)\}', body)
    goid = go.group(1) if go else 'none'
    father = re.search(r'm_Father: \{fileID: (\d+)\}', body)
    fid = father.group(1) if father else 'none'
    children = re.search(r'm_Children:(.*?)(?=m_Father|\Z)', body, re.DOTALL)
    ch_ids = re.findall(r'fileID: (\d+)', children.group(1)) if children else []
    print(f'Type {ttype} ID {tid} GO {goid} Father {fid} Children: {ch_ids}')
