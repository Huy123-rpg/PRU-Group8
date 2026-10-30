import sys, os, shutil, re
sys.stdout.reconfigure(encoding='utf-8')

scenes = ['Assets/Scenes/Physics/Game2/19. Result_TN.unity', 'Assets/Scenes/Physics/Game2/19.1. Result_TL.unity']

for sc in scenes:
    bak = sc + '.bak'
    shutil.copy2(sc, bak)
    print(f'Backed up {sc} -> {bak}')

    with open(sc, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. In RectTransform 164711256, change m_LocalScale: {x: 0, y: 0, z: 0} to {x: 1, y: 1, z: 1} and m_Father: {fileID: 983221601} to {fileID: 0}
    pattern_rt = r'(--- !u!224 &164711256\nRectTransform:.*?\n  m_LocalScale: )\{x: 0, y: 0, z: 0\}(.*?\n  m_Father: )\{fileID: 983221601\}'
    match_rt = re.search(pattern_rt, content, re.DOTALL)
    if match_rt:
        content = content[:match_rt.start()] + match_rt.group(1) + '{x: 1, y: 1, z: 1}' + match_rt.group(2) + '{fileID: 0}' + content[match_rt.end():]
        print(f'[{sc}] Updated RectTransform 164711256.')
    else:
        print(f'[{sc}] WARNING: Pattern for RT 164711256 not matched.')

    # 2. In Transform 983221601, clear m_Children: - {fileID: 164711256} to m_Children: []
    pattern_cam = r'(--- !u!4 &983221601\nTransform:.*?\n  m_Children:)\n  - \{fileID: 164711256\}'
    match_cam = re.search(pattern_cam, content, re.DOTALL)
    if match_cam:
        content = content[:match_cam.start()] + match_cam.group(1) + ' []' + content[match_cam.end():]
        print(f'[{sc}] Updated Transform 983221601.')
    else:
        print(f'[{sc}] WARNING: Pattern for Transform 983221601 not matched.')

    # 3. In SceneRoots, add - {fileID: 164711256}
    pattern_roots = r'(SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n  - \{fileID: 983221601\})\n(  - \{fileID: 1326528420\})'
    match_roots = re.search(pattern_roots, content)
    if match_roots:
        content = content[:match_roots.start()] + match_roots.group(1) + '\n  - {fileID: 164711256}\n' + match_roots.group(2) + content[match_roots.end():]
        print(f'[{sc}] Updated SceneRoots.')
    else:
        print(f'[{sc}] WARNING: Pattern for SceneRoots not matched.')

    with open(sc, 'w', encoding='utf-8') as f:
        f.write(content)

print('Canvas roots adjustment completed.')
