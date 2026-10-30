import os
import sys

sys.stdout.reconfigure(encoding='utf-8')

guids = ["b07d1587e3525de42b6112a9a8f73ccf", "de21c4c998367a44b9de263187d17142"]

for root, dirs, files in os.walk("Assets"):
    for file in files:
        if file.endswith(".meta"):
            p = os.path.join(root, file)
            with open(p, "r", encoding="utf-8", errors="ignore") as f:
                content = f.read()
                for g in guids:
                    if g in content:
                        print(f"GUID {g} -> {p[:-5]}")
