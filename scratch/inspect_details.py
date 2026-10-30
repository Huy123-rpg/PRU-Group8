import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

scene_path = r"Assets/Scenes/Physics/Game2/33. Chọn độ khó.unity"

with open(scene_path, "r", encoding="utf-8") as f:
    lines = f.readlines()

# Let's inspect objects in detail
cur_obj = None
objects = {}

for line in lines:
    m = re.match(r"^--- !u!(\d+) &(\d+)", line)
    if m:
        cur_type = m.group(1)
        cur_id = m.group(2)
        cur_obj = {"type": cur_type, "id": cur_id, "lines": []}
        objects[cur_id] = cur_obj
    elif cur_obj:
        cur_obj["lines"].append(line)

for fid, obj in objects.items():
    text = "".join(obj["lines"])
    if obj["type"] == "1": # GameObject
        name_m = re.search(r"m_Name:\s*(.+)", text)
        name = name_m.group(1) if name_m else "Unknown"
        print(f"\n=== GameObject {fid}: '{name}' ===")
    elif obj["type"] == "224": # RectTransform
        go_m = re.search(r"m_GameObject:\s*\{fileID:\s*(\d+)\}", text)
        pos_m = re.search(r"m_AnchoredPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}", text)
        size_m = re.search(r"m_SizeDelta:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}", text)
        parent_m = re.search(r"m_Father:\s*\{fileID:\s*(\d+)\}", text)
        print(f"RectTransform {fid} (GO {go_m.group(1) if go_m else '?'}) Parent: {parent_m.group(1) if parent_m else 'none'} Pos: ({pos_m.group(1) if pos_m else '?'}, {pos_m.group(2) if pos_m else '?'}) Size: ({size_m.group(1) if size_m else '?'}, {size_m.group(2) if size_m else '?'})")
    elif obj["type"] == "114": # MonoBehaviour
        go_m = re.search(r"m_GameObject:\s*\{fileID:\s*(\d+)\}", text)
        script_m = re.search(r"m_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([a-f0-9]+)", text)
        print(f"MonoBehaviour {fid} on GO {go_m.group(1) if go_m else '?'}, script guid: {script_m.group(2) if script_m else '?'}")
    elif obj["type"] == "115": # MonoScript or Image
        pass
    elif obj["type"] == "222": # CanvasRenderer
        pass
