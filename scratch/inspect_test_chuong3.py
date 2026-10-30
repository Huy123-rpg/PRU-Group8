import csv
import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    rows = list(reader)

t3_rows = [r for r in rows if r.get('BaiID', '').strip() == 'test_chuong3']

print(f"Total rows in test_chuong3: {len(t3_rows)}")
for r in t3_rows:
    q = r.get('NoiDung', '')
    print(f"ID={r.get('CauHoiID')}, MucDo={r.get('MucDo')}: {q[:80]}")
