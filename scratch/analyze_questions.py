import csv
import json
import sys
from collections import defaultdict

sys.stdout.reconfigure(encoding='utf-8')

# Read live sheet CSV
sheet_counts = defaultdict(lambda: defaultdict(list))
sheet_status_counts = defaultdict(lambda: defaultdict(int))

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        bai = row.get('BaiID', '').strip().lower()
        mucdo = row.get('MucDo', '').strip()
        status = row.get('TrangThai', '').strip()
        qid = row.get('CauHoiID', '').strip()
        
        # map mucdo
        norm_mucdo = mucdo
        if 'nhận biết' in mucdo.lower() or mucdo == 'NB': norm_mucdo = 'NB'
        elif 'thông hiểu' in mucdo.lower() or mucdo == 'TH': norm_mucdo = 'TH'
        elif 'vận dụng cao' in mucdo.lower() or mucdo == 'VDC': norm_mucdo = 'VDC'
        elif 'vận dụng' in mucdo.lower() or mucdo == 'VD': norm_mucdo = 'VD'
        
        sheet_status_counts[status][norm_mucdo] += 1
        
        # Check validity
        is_valid = True
        reason = []
        if status in ['Đã ẩn', 'Bản nháp']:
            is_valid = False
            reason.append(f"Status: {status}")
        if not row.get('NoiDung', '').strip():
            is_valid = False
            reason.append("Empty content")
        
        sheet_counts[bai][norm_mucdo].append({
            'qid': qid,
            'status': status,
            'valid': is_valid,
            'reason': reason,
            'loai': row.get('LoaiCauHoi', ''),
            'dung': row.get('DapAnDung', '')
        })

print("=== GOOGLE SHEETS OVERVIEW ===")
print("Status breakdown:")
for status, mcounts in sheet_status_counts.items():
    print(f"  {status}: {dict(mcounts)}")

print("\nCounts per BaiID and Difficulty in Google Sheets (Valid / Total):")
for bai in sorted(sheet_counts.keys()):
    counts_str = []
    for md in ['NB', 'TH', 'VD', 'VDC']:
        items = sheet_counts[bai][md]
        val = sum(1 for x in items if x['valid'])
        tot = len(items)
        counts_str.append(f"{md}: {val}/{tot}")
    print(f"  {bai:15s}: {', '.join(counts_str)}")

# Now read local json
print("\n=== LOCAL JSON (g2_mcq_bank.json) OVERVIEW ===")
with open('Assets/Resources/QuizData/g2_mcq_bank.json', 'r', encoding='utf-8') as f:
    local_data = json.load(f)

local_qs = local_data.get('questions', [])
print(f"Total MCQs in local g2_mcq_bank.json: {len(local_qs)}")

local_counts = defaultdict(lambda: defaultdict(int))
for q in local_qs:
    bai = q.get('baiID', '').strip().lower()
    md = q.get('mucDoGame2', q.get('mucDo', '')).strip()
    if 'nhận biết' in md.lower() or md == 'NB': md = 'NB'
    elif 'thông hiểu' in md.lower() or md == 'TH': md = 'TH'
    elif 'vận dụng cao' in md.lower() or md == 'VDC': md = 'VDC'
    elif 'vận dụng' in md.lower() or md == 'VD': md = 'VD'
    local_counts[bai][md] += 1

for bai in sorted(local_counts.keys()):
    counts_str = [f"{md}: {local_counts[bai][md]}" for md in ['NB', 'TH', 'VD', 'VDC']]
    print(f"  {bai:15s}: {', '.join(counts_str)}")
