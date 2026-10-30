from PIL import Image, ImageDraw, ImageFont

bg = Image.open('scratch/scene33_rendered.png')
# Re-render with Start button at y = -420 and plaques under the 4 cards!

# Load base bg again
bg = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_07 27 thg 10, 2026.png').resize((1920, 1080))
atlas = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_14 27 thg 10, 2026.png')
atlas_w, atlas_h = atlas.size

def unity_crop(x, y, w, h):
    return atlas.crop((x, atlas_h - (y + h), x + w, atlas_h - y))

center_x, center_y = 960, 540

# Banner: pos=(0, 369.84), size=(1119.5, 340.3)
banner_img = unity_crop(180, 571, 1313, 366).resize((int(1119.5), int(340.3)))
bg.paste(banner_img, (int(center_x - 1119.5/2), int(center_y - 369.84 - 340.3/2)), banner_img.convert('RGBA'))

# Back: pos=(-747.26, 448.93), size=(333, 110)
back_img = unity_crop(57, 27, 317, 104).resize((333, 110))
bg.paste(back_img, (int(center_x - 747.26 - 333/2), int(center_y - 448.93 - 110/2)), back_img.convert('RGBA'))

# Start button at y = -420, size=(564, 134)
start_img = unity_crop(560, 9, 545, 151).resize((564, 134))
bg.paste(start_img, (int(center_x - 564/2), int(center_y - (-420) - 134/2)), start_img.convert('RGBA'))

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

# Now draw plaques under each card!
draw = ImageDraw.Draw(bg)

# Try font
try:
    font = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 17)
    font_bold = ImageFont.truetype("C:/Windows/Fonts/segouib.ttf", 17)
except:
    font = ImageFont.load_default()
    font_bold = font

for name, sx, sy, sw, sh, ux, uy, uw, uh, desc in cards_data:
    # Plaque center x is center_x + ux
    # Plaque top is bottom of card + 6px
    card_bottom_screen_y = center_y - uy + uh/2
    plaque_w = 230
    plaque_h = 36
    px1 = int(center_x + ux - plaque_w / 2)
    py1 = int(card_bottom_screen_y + 4)
    px2 = px1 + plaque_w
    py2 = py1 + plaque_h

    # Draw navy plaque with gold border
    draw.rounded_rectangle([px1, py1, px2, py2], radius=8, fill=(8, 20, 48, 235), outline=(236, 201, 75, 255), width=2)
    # Inner subtle glow or border
    draw.rounded_rectangle([px1+1, py1+1, px2-1, py2-1], radius=7, outline=(212, 175, 55, 180), width=1)
    
    # Draw text
    bbox = font_bold.getbbox(desc)
    tw = bbox[2] - bbox[0]
    th = bbox[3] - bbox[1]
    tx = px1 + (plaque_w - tw) / 2
    ty = py1 + (plaque_h - th) / 2 - 2
    draw.text((tx, ty), desc, fill=(250, 245, 255), font=font_bold)

bg.save('scratch/scene33_with_plaques.png')
print("Saved scratch/scene33_with_plaques.png")
