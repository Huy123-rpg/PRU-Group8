from PIL import Image

# Center y is 540
# uy of TH = -54.967, uh = 481 -> bottom y of TH on screen: 540 - (-54.967) + 481/2 = 540 + 55 + 240.5 = 835.5
# uy of Start = -379, h = 134 -> top y of Start on screen: 540 - (-379) - 134/2 = 540 + 379 - 67 = 852
# Gap = 852 - 835.5 = 16.5 pixels!
print(f"Bottom of TH card on screen: {540 + 54.967 + 481.1/2:.1f}")
print(f"Top of Start button on screen: {540 + 379 - 133.98/2:.1f}")
print(f"Gap: {(540 + 379 - 133.98/2) - (540 + 54.967 + 481.1/2):.1f}")
