import shutil
import os
import sys

sys.stdout.reconfigure(encoding='utf-8')

src = 'Assets/Scenes/Physics/Game2/33. Chọn độ khó.unity'
dst = 'Assets/Scenes/Physics/Game2/33. Chọn độ khó.unity.bak'
shutil.copyfile(src, dst)
print(f"Successfully backed up {src} to {dst}")
