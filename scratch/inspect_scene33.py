import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

scene_path = r"Assets/Scenes/Physics/Game2/33. Chọn độ khó.unity"

with open(scene_path, "r", encoding="utf-8") as f:
    content = f.read()

# Find GameObjects
go_pattern = re.compile(r"--- !u!1 &(\d+)\nGameObject:\n  m_ObjectHideFlags: [^\n]+\n  m_CorrespondingSourceObject: [^\n]+\n  m_PrefabInstance: [^\n]+\n  m_PrefabAsset: [^\n]+\n  serializedVersion: \d+\n  m_Component:\n(.*?)\n  m_Layer: [^\n]+\n  m_Name: ([^\n]+)", re.DOTALL)

for m in go_pattern.finditer(content):
    fid = m.group(1)
    name = m.group(3)
    print(f"GO {fid}: {name}")
