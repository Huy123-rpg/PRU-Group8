import csv
import sys
from collections import defaultdict
sys.stdout.reconfigure(encoding='utf-8')

with open('scratch/live_sheet_cauhoituluan.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    rows = list(reader)

print(f"Total rows in Cauhoituluan Sheet: {len(rows)}")

essay_counts = defaultdict(lambda: defaultdict(list))
for r in rows:
    b = r.get('BaiID', '').strip()
    md = r.get('MucDo', '').strip()
    st = r.get('TrangThai', '').strip()
    qid = r.get('CauHoiID', '').strip()
    essay_counts[b][md].append((qid, st, r.get('NoiDung', '')[:40]))

print("\nEssay Counts by BaiID and MucDo in Sheet:")
for b in sorted(essay_counts.keys()):
    print(f"  BaiID: {b:15s}")
    for md, items in essay_counts[b].items():
        print(f"    - {md:20s}: {len(items)} câu")
