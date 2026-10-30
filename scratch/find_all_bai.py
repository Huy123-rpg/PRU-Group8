import csv
import sys

sys.stdout.reconfigure(encoding='utf-8')

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    rows = list(reader)

print(f"Total rows in Sheet: {len(rows)}")

# Let's inspect unique BaiID and TenBai values
bai_tenbai = set()
for r in rows:
    bai_tenbai.add((r.get('ChuongId', ''), r.get('BaiID', ''), r.get('TenBai', '')))

print("\nAll (ChuongId, BaiID, TenBai) in Sheet:")
for c, b, t in sorted(bai_tenbai):
    print(f"  Chuong: {c:12s} | BaiID: {b:15s} | TenBai: {t}")
