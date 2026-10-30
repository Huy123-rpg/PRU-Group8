import csv
import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    rows = list(reader)

print(f"Total rows in Sheet: {len(rows)}")

# Find all rows related to Chuong 3 or Bai 11 or Dien tro or Dinh luat Ohm
c3_rows = []
for i, r in enumerate(rows):
    line_no = i + 2
    c_id = r.get('ChuongId', '').lower()
    b_id = r.get('BaiID', '').lower()
    ten_b = r.get('TenBai', '').lower()
    noi_dung = r.get('NoiDung', '')
    
    if 'chuong3' in c_id or 'bai11' in b_id or 'điện trở' in noi_dung.lower() or 'định luật ohm' in noi_dung.lower() or 'đoạn mạch' in noi_dung.lower() or 'ohm' in noi_dung.lower():
        c3_rows.append((line_no, r))

print(f"Found {len(c3_rows)} rows matching Chuong 3 / Bai 11 / Ohm.")

# Group by BaiID and MucDo
bai11_rows = [r for l, r in c3_rows if r.get('BaiID', '').strip().lower() == 'bai11']
print(f"\nExact BaiID == 'bai11': {len(bai11_rows)} rows")
for r in bai11_rows:
    print(f"  Line {r.get('STT')}: ID={r.get('CauHoiID')}, MucDo={r.get('MucDo')}, TrangThai={r.get('TrangThai')}, Q={r.get('NoiDung')[:60]}...")

# Check rows where BaiID is NOT bai11 but content mentions Dien tro or Dinh luat Ohm
other_ohm = [(l, r) for l, r in c3_rows if r.get('BaiID', '').strip().lower() != 'bai11']
print(f"\nOther rows matching Ohm / Chuong 3 ({len(other_ohm)} rows):")
for l, r in other_ohm:
    print(f"  Line {l}: BaiID={r.get('BaiID')}, MucDo={r.get('MucDo')}, TenBai={r.get('TenBai')}, Q={r.get('NoiDung')[:60]}...")
