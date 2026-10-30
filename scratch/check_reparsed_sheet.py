import csv
import sys
from collections import defaultdict
sys.stdout.reconfigure(encoding='utf-8')

from test_fixed_normalize import normalize_difficulty_fixed

counts = defaultdict(lambda: defaultdict(int))
with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for r in reader:
        b = r.get('BaiID', '').strip().lower()
        if not b: continue
        st = r.get('TrangThai', '').strip()
        if st in ['Đã ẩn', 'Bản nháp']: continue
        md = normalize_difficulty_fixed(r.get('MucDo', ''))
        counts[b][md] += 1

print("Parsed Google Sheets with FIXED normalization:")
for b in sorted(counts.keys()):
    c_str = ", ".join(f"{md}: {counts[b][md]}" for md in ['NB', 'TH', 'VD', 'VDC'])
    print(f"  {b:15s} -> {c_str}")
