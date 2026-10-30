from PIL import Image, ImageDraw, ImageFont
import sys

# Canvas is 1920x1080 (standard 16:9 Full HD)
canvas_w, canvas_h = 1920, 1080
center_x = canvas_w / 2 # 960
center_y = canvas_h / 2 # 540

# In Unity UI: (0, 0) is screen center.
# Screen X = center_x + ui_x
# Screen Y = center_y - ui_y (since in screen/image coords, Y increases downwards)

# Load atlas
atlas = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_14 27 thg 10, 2026.png')
atlas_w, atlas_h = atlas.size

def unity_crop(x, y, w, h):
    return atlas.crop((x, atlas_h - (y + h), x + w, atlas_h - y))

bg = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_07 27 thg 10, 2026.png').resize((1920, 1080))

# 4 cards
# 6: x=62, y=147, w=344, h=436 (NB)
# 11: x=471, y=151, w=344, h=426 (TH)
# 12: x=870, y=154, w=326, h=422 (VD)
# 8: x=1267, y=152, w=350, h=432 (VDC)
# Banner: 1: x=180, y=571, w=1313, h=366
# Back: 9: x=57, y=27, w=317, h=104
# Start: 13: x=560, y=9, w=545, h=151

cards_data = [
    ("NB", 62, 147, 344, 436, -557, -54.967, 345, 481),
    ("TH", 471, 151, 344, 426, -161, -54.967, 345, 481),
    ("VD", 870, 154, 326, 422, 223, -48.899, 345, 469),
    ("VDC", 1267, 152, 350, 432, 655, -41.849, 345, 483),
]

# Paste Banner: pos=(0, 369.84), size=(1119.5, 340.3)
banner_img = unity_crop(180, 571, 1313, 366).resize((int(1119.5), int(340.3)))
bg.paste(banner_img, (int(center_x - 1119.5/2), int(center_y - 369.84 - 340.3/2)), banner_img.convert('RGBA'))

# Paste Back: pos=(-747.26, 448.93), size=(333, 110)
back_img = unity_crop(57, 27, 317, 104).resize((333, 110))
bg.paste(back_img, (int(center_x - 747.26 - 333/2), int(center_y - 448.93 - 110/2)), back_img.convert('RGBA'))

# Paste Start: pos=(0, -379), size=(564, 134)
start_img = unity_crop(560, 9, 545, 151).resize((564, 134))
bg.paste(start_img, (int(center_x - 564/2), int(center_y - (-379) - 134/2)), start_img.convert('RGBA'))

# Paste Cards
for name, sx, sy, sw, sh, ux, uy, uw, uh in cards_data:
    c_img = unity_crop(sx, sy, sw, sh).resize((int(uw), int(uh)))
    px = int(center_x + ux - uw/2)
    py = int(center_y - uy - uh/2)
    bg.paste(c_img, (px, py), c_img.convert('RGBA'))

bg.save('scratch/scene33_rendered.png')
print("Rendered scene 33 to scratch/scene33_rendered.png")
