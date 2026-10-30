from PIL import Image

img = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_14 27 thg 10, 2026.png')
w, h = img.size

# Slices:
# 6: x=62, y=147, w=344, h=436 (NB)
# 11: x=471, y=151, w=344, h=426 (TH)
# 12: x=870, y=154, w=326, h=422 (VD)
# 8: x=1267, y=152, w=350, h=432 (VDC)

crops = {
    'nb': (62, h - (147 + 436), 62 + 344, h - 147),
    'th': (471, h - (151 + 426), 471 + 344, h - 151),
    'vd': (870, h - (154 + 422), 870 + 326, h - 154),
    'vdc': (1267, h - (152 + 432), 1267 + 350, h - 152),
}

for name, box in crops.items():
    cropped = img.crop(box)
    cropped.save(f'scratch/card_{name}.png')
print("Cards cropped!")
