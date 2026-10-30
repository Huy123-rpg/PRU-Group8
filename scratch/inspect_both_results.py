import os, re

guids = {'0cd44c1031e13a943bb63640046fad76', 'f4688fdb7df04437aeb418b961361dc5', 'a79441f348de89743a2939f4d699eac1', '76c392e42b5098c458856cdf6ecaaaa1', '4e29b1a8efbd4b44bb3f3716e73f07ff', 'fe87c0e1cc204ed48ad3b37840f39efc', '01614664b831546d2ae94a42149d80ac', 'dc42784cf147c0c48a680349fa168899'}

guid_map = {}
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.meta'):
            p = os.path.join(root, f)
            with open(p, 'r', encoding='utf-8', errors='ignore') as mf:
                content = mf.read()
                gm = re.search(r'guid: ([0-9a-fA-F]+)', content)
                if gm and gm.group(1) in guids:
                    guid_map[gm.group(1)] = p[:-5]

for g, path in guid_map.items():
    print(f'{g} -> {path}')
