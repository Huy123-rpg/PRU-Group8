import sys
import re

sys.stdout.reconfigure(encoding='utf-8')

path = 'Assets/Scenes/Physics/Game2/33. Chọn độ khó.unity'

with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# Match RectTransform 1342882856 specifically
pattern = r'(--- !u!224 &1342882856\nRectTransform:.*?\n  m_AnchoredPosition: \{x: )[^,]+(, y: )-?\d+(\.[0-9]+)?(\})'

match = re.search(pattern, content, re.DOTALL)
if match:
    replacement = r'\g<1>0\g<2>-415\g<4>'
    new_content = re.sub(pattern, replacement, content, count=1, flags=re.DOTALL)
    with open(path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(new_content)
    print("Updated RectTransform 1342882856 y to -415")
else:
    print("Pattern not found!")
