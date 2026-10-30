import csv
import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    rows = list(reader)

print("Checking for typos or mislabeled rows...")

# 1. Check all unique BaiID in the entire sheet
all_bai_ids = set(r.get('BaiID', '').strip() for r in rows)
print("All unique BaiIDs in sheet:", sorted(all_bai_ids))

# 2. Check all unique TrangThai
all_trang_thai = set(r.get('TrangThai', '').strip() for r in rows)
print("All unique TrangThai:", all_trang_thai)

# 3. Check rows in Chuong 3:
chuong3_rows = [r for r in rows if 'chuong3' in r.get('ChuongId', '').lower()]
print(f"\nTotal rows in chuong3: {len(chuong3_rows)}")

# Count by BaiID in Chuong 3
c3_by_bai = {}
for r in chuong3_rows:
    b = r.get('BaiID', '').strip()
    c3_by_bai[b] = c3_by_bai.get(b, 0) + 1
print("Chuong 3 breakdown by BaiID:", c3_by_bai)

# Breakdown of each Bai in Chuong 3 by MucDo
for b in c3_by_bai:
    b_rows = [r for r in chuong3_rows if r.get('BaiID', '').strip() == b]
    md_counts = {}
    for r in b_rows:
        md = r.get('MucDo', '').strip()
        md_counts[md] = md_counts.get(md, 0) + 1
    print(f"  {b}: {md_counts}")

# 4. Check if any row has 'bai11' anywhere in any column
for i, r in enumerate(rows):
    line_no = i + 2
    row_str = " ".join(r.values())
    if 'bai11' in row_str.lower() and r.get('BaiID', '').strip().lower() != 'bai11':
        print(f"Row {line_no} mentions bai11 but BaiID={r.get('BaiID')}: {r.get('NoiDung')[:50]}")
