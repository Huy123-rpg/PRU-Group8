import re

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's find RectTransforms of Button, Button (1), Button (2)
# And set m_RaycastTarget to 0 for decorative images

# 1. Update Button pos:
# Left Button: Button &1364637793 -> x: -544
# Middle Button: Button (1) &714684263 -> x: 0
# Right Button: Button (2) &1123004799 -> x: 510.5

def replace_rect_pos(doc_str, target_id, new_x, new_y):
    # Find doc starting with --- !u!224 &target_id
    pattern = rf'(--- !u!224 &{target_id}\nRectTransform:[\s\S]*?m_AnchoredPosition:\s*\{{x:\s*)([^,]+)(,\s*y:\s*)([^}}]+)(\}})'
    m = re.search(pattern, doc_str)
    if m:
        repl = rf'\g<1>{new_x}\g<3>{new_y}\g<5>'
        return re.sub(pattern, repl, doc_str, count=1)
    return doc_str

# 2. Deactivate Image (8) & Image (9)
def set_go_active(doc_str, go_id, is_active):
    pattern = rf'(--- !u!1 &{go_id}\nGameObject:[\s\S]*?m_IsActive:\s*)(\d+)'
    m = re.search(pattern, doc_str)
    if m:
        repl = rf'\g<1>{is_active}'
        return re.sub(pattern, repl, doc_str, count=1)
    return doc_str

# Test replacements
new_text = text
new_text = replace_rect_pos(new_text, '1364637793', '-544', '-472.92')
new_text = replace_rect_pos(new_text, '714684263', '0', '-472.92')
new_text = replace_rect_pos(new_text, '1123004799', '510.5', '-472.92')

new_text = set_go_active(new_text, '840356712', '0') # Image (8)
new_text = set_go_active(new_text, '597985023', '0') # Image (9)

print("Differences found:", len(new_text) != len(text) or new_text != text)

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'w', encoding='utf-8') as f:
    f.write(new_text)

print("Successfully updated 19.1. Result_TL.unity")
