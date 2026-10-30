import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Physics/Game2/33. Chọn độ khó.unity', 'r', encoding='utf-8') as f:
    content = f.read()

# Let's inspect GO 1342882855
block = re.search(r'--- !u!1 &1342882855\nGameObject:(.*?)(?=\n--- !u|\Z)', content, re.DOTALL)
if block:
    print('GO 1342882855:')
    print(block.group(0))

# find all components of this GO
comps = re.findall(r'fileID: (\d+)', block.group(1))
for cid in comps:
    cblock = re.search(r'--- !u!\d+ &' + cid + r'\n(.*?)(?=\n--- !u|\Z)', content, re.DOTALL)
    if cblock:
        print(f'Component {cid}:\n', cblock.group(0)[:400])
