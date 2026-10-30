from PIL import Image, ImageDraw, ImageFont
import os
import sys

sys.stdout.reconfigure(encoding='utf-8')

# Render Screen 1: Default state (no selection, Start button dimmed)
canvas_w, canvas_h = 1920, 1080
center_x, center_y = 960, 540

bg = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_07 27 thg 10, 2026.png').resize((1920, 1080))
atlas = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_14 27 thg 10, 2026.png')
atlas_w, atlas_h = atlas.size

def unity_crop(x, y, w, h):
    return atlas.crop((x, atlas_h - (y + h), x + w, atlas_h - y))

# Banner
banner_img = unity_crop(180, 571, 1313, 366).resize((1120, 340))
bg.paste(banner_img, (int(center_x - 1120/2), int(center_y - 369.84 - 340/2)), banner_img.convert('RGBA'))

# Back
back_img = unity_crop(57, 27, 317, 104).resize((333, 110))
bg.paste(back_img, (int(center_x - 747.26 - 333/2), int(center_y - 448.93 - 110/2)), back_img.convert('RGBA'))

# Start button (dimmed to 45% alpha)
start_img = unity_crop(560, 9, 545, 151).resize((564, 134))
# Apply 45% alpha
start_rgba = start_img.convert('RGBA')
r, g, b, a = start_rgba.split()
a = a.point(lambda p: int(p * 0.45))
start_rgba.putalpha(a)
bg.paste(start_rgba, (int(center_x - 564/2), int(center_y - (-415) - 134/2)), start_rgba)

cards_data = [
    ("NB", 62, 147, 344, 436, -557, -54.967, 345, 481, "Câu hỏi cơ bản"),
    ("TH", 471, 151, 344, 426, -161, -54.967, 345, 481, "Hiểu và giải thích"),
    ("VD", 870, 154, 326, 422, 223, -48.899, 345, 469, "Áp dụng kiến thức"),
    ("VDC", 1267, 152, 350, 432, 655, -41.849, 345, 483, "Bài tập nâng cao"),
]

# Paste Cards
for name, sx, sy, sw, sh, ux, uy, uw, uh, desc in cards_data:
    c_img = unity_crop(sx, sy, sw, sh).resize((int(uw), int(uh)))
    px = int(center_x + ux - uw/2)
    py = int(center_y - uy - uh/2)
    bg.paste(c_img, (px, py), c_img.convert('RGBA'))

# Plaque fonts
try:
    font_bold = ImageFont.truetype("C:/Windows/Fonts/segouib.ttf", 17)
    font_header = ImageFont.truetype("C:/Windows/Fonts/segouib.ttf", 22)
    font_msg = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 18)
except:
    font_bold = ImageFont.load_default()
    font_header = font_bold
    font_msg = font_bold

# Draw plaques
draw = ImageDraw.Draw(bg)
for name, sx, sy, sw, sh, ux, uy, uw, uh, desc in cards_data:
    card_bottom_screen_y = center_y - uy + uh/2
    plaque_w = 240
    plaque_h = 38
    px1 = int(center_x + ux - plaque_w / 2)
    py1 = int(card_bottom_screen_y + 4)
    px2 = px1 + plaque_w
    py2 = py1 + plaque_h

    # Deep Aurelia Navy with Gold Outline
    draw.rounded_rectangle([px1, py1, px2, py2], radius=6, fill=(10, 22, 48, 245), outline=(245, 209, 97, 255), width=2)
    draw.rounded_rectangle([px1+1, py1+1, px2-1, py2-1], radius=5, outline=(212, 175, 55, 120), width=1)
    
    bbox = font_bold.getbbox(desc)
    tw = bbox[2] - bbox[0]
    th = bbox[3] - bbox[1]
    tx = px1 + (plaque_w - tw) / 2
    ty = py1 + (plaque_h - th) / 2 - 2
    draw.text((tx, ty), desc, fill=(250, 245, 255), font=font_bold)

out1 = 'C:/Users/Admin/.gemini/antigravity/brain/da837182-bf0d-45c9-8d4e-5b2000f30d53/difficulty_screen_refined.png'
bg.save(out1)
print(f"Saved refined screen to {out1}")

# Render Screen 2: Warning Modal State
modal_bg = bg.copy()
overlay = Image.new('RGBA', (canvas_w, canvas_h), (5, 12, 30, 210))
modal_bg.paste(overlay, (0, 0), overlay)

m_draw = ImageDraw.Draw(modal_bg)
panel_w, panel_h = 580, 260
m_x1 = int(center_x - panel_w / 2)
m_y1 = int(center_y - panel_h / 2)
m_x2 = m_x1 + panel_w
m_y2 = m_y1 + panel_h

# Draw Modal Dialog Panel
m_draw.rounded_rectangle([m_x1, m_y1, m_x2, m_y2], radius=10, fill=(10, 23, 51, 250), outline=(245, 209, 97, 255), width=2)
m_draw.rounded_rectangle([m_x1+2, m_y1+2, m_x2-2, m_y2-2], radius=8, outline=(180, 140, 45, 120), width=1)

# Header "THÔNG BÁO"
h_text = "THÔNG BÁO"
h_bbox = font_header.getbbox(h_text)
m_draw.text((center_x - (h_bbox[2]-h_bbox[0])/2, m_y1 + 25), h_text, fill=(246, 224, 94), font=font_header)

# Body message
msg_lines = [
    "Mức độ này hiện chưa đủ số câu hỏi.",
    "Vui lòng chọn mức khác hoặc quay lại."
]
curr_y = m_y1 + 80
for line in msg_lines:
    l_bbox = font_msg.getbbox(line)
    m_draw.text((center_x - (l_bbox[2]-l_bbox[0])/2, curr_y), line, fill=(250, 245, 255), font=font_msg)
    curr_y += 28

# Button "ĐÃ HIỂU"
btn_w, btn_h = 180, 46
bx1 = int(center_x - btn_w / 2)
by1 = int(m_y2 - 68)
bx2 = bx1 + btn_w
by2 = by1 + btn_h

m_draw.rounded_rectangle([bx1, by1, bx2, by2], radius=6, fill=(20, 41, 82, 255), outline=(245, 209, 97, 255), width=2)
b_text = "ĐÃ HIỂU"
b_bbox = font_bold.getbbox(b_text)
m_draw.text((center_x - (b_bbox[2]-b_bbox[0])/2, by1 + (btn_h - (b_bbox[3]-b_bbox[1]))/2 - 2), b_text, fill=(250, 235, 166), font=font_bold)

out2 = 'C:/Users/Admin/.gemini/antigravity/brain/da837182-bf0d-45c9-8d4e-5b2000f30d53/difficulty_warning_modal.png'
modal_bg.save(out2)
print(f"Saved warning modal state to {out2}")
