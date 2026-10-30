import sys
sys.stdout.reconfigure(encoding='utf-8')

# Cards:
# NB: pos=(-557, -54.967), size=(345.168, 481.103)
# TH: pos=(-161, -54.967), size=(345.168, 481.1)
# VD: pos=(223, -48.899), size=(345.168, 468.966)
# VDC: pos=(655, -41.849), size=(345.168, 483.07)
# Start btn: pos=(0, -379), size=(563.635, 133.984)

cards = [
    ("NB", -557, -54.967, 345.168, 481.103),
    ("TH", -161, -54.967, 345.168, 481.1),
    ("VD", 223, -48.899, 345.168, 468.966),
    ("VDC", 655, -41.849, 345.168, 483.07),
]

btn = ("StartBtn", 0, -379, 563.635, 133.984)

print("Card bounds (x_min, x_max, y_bottom, y_top):")
for name, x, y, w, h in cards:
    print(f"{name:4s}: x=[{x - w/2:.1f}, {x + w/2:.1f}], y=[{y - h/2:.1f}, {y + h/2:.1f}]")

btn_name, bx, by, bw, bh = btn
print(f"{btn_name:8s}: x=[{bx - bw/2:.1f}, {bx + bw/2:.1f}], y=[{by - bh/2:.1f}, {by + bh/2:.1f}]")
