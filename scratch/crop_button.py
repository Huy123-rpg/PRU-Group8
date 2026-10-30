from PIL import Image
import sys

img = Image.open('Assets/Art/Environment/Ảnh ChatGPT 00_21_14 27 thg 10, 2026.png')
w, h = img.size
print(f"Image size: {w}x{h}")

# Unity texture coordinates: y is from bottom
# rect: x=560, y=9, width=545, height=151
# PIL top-left y: h - (y + height)
crop_box = (560, h - (9 + 151), 560 + 545, h - 9)
cropped = img.crop(crop_box)
cropped.save('scratch/start_btn_sprite.png')
print("Saved scratch/start_btn_sprite.png")
