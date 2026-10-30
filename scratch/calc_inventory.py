import csv
import sys
from collections import defaultdict
sys.stdout.reconfigure(encoding='utf-8')

from test_fixed_normalize import normalize_difficulty_fixed

# 1. Check MCQ counts in scratch/live_sheet_nganhang.csv
mcq_counts = defaultdict(lambda: defaultdict(int))
bai_names = {}
chuong_map = {}

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    r = csv.DictReader(f)
    for row in r:
        b = row.get('BaiID', '').strip().lower()
        if not b or b.startswith('test_'): continue
        st = row.get('TrangThai', '').strip()
        if st in ['Đã ẩn', 'Bản nháp']: continue
        md = normalize_difficulty_fixed(row.get('MucDo', ''))
        mcq_counts[b][md] += 1
        bai_names[b] = row.get('TenBai', '').strip()
        chuong_map[b] = (row.get('ChuongId', '').strip(), row.get('TenChuong', '').strip())

# 2. Check Essay counts in scratch/live_sheet_cauhoituluan.csv
essay_counts = defaultdict(lambda: defaultdict(int))
with open('scratch/live_sheet_cauhoituluan.csv', 'r', encoding='utf-8') as f:
    r = csv.DictReader(f)
    for row in r:
        b = row.get('BaiID', '').strip()
        # map to canonical bai2, bai3...
        canon = b.lower()
        if canon.startswith('c') and '_l' in canon:
            part = canon.split('_l')[1]
            canon = f"bai{part}"
        st = row.get('TrangThai', '').strip()
        if st in ['Đã ẩn', 'Bản nháp']: continue
        md = normalize_difficulty_fixed(row.get('MucDo', ''))
        essay_counts[canon][md] += 1

print("=== INVENTORY OF 16 LESSONS ===")
print("Lesson | MCQ Existing (NB/TH/VD/VDC) | MCQ Missing | Essay Existing (NB/TH/VD/VDC) | Essay Missing")
print("-" * 90)

total_mcq_needed = 0
total_essay_needed = 0

all_bais = [f"bai{i}" for i in range(2, 18)]

mcq_missing_dict = {}
essay_missing_dict = {}

for b in all_bais:
    mcq_exist = [mcq_counts[b][md] for md in ['NB', 'TH', 'VD', 'VDC']]
    mcq_miss = [max(0, 10 - mcq_counts[b][md]) for md in ['NB', 'TH', 'VD', 'VDC']]
    mcq_missing_dict[b] = mcq_miss
    total_mcq_needed += sum(mcq_miss)

    essay_exist = [essay_counts[b][md] for md in ['NB', 'TH', 'VD', 'VDC']]
    essay_miss = [max(0, 5 - essay_counts[b][md]) for md in ['NB', 'TH', 'VD', 'VDC']]
    essay_missing_dict[b] = essay_miss
    total_essay_needed += sum(essay_miss)

    print(f"{b:6s} | {mcq_exist} -> miss: {mcq_miss} (sum={sum(mcq_miss)}) | {essay_exist} -> miss: {essay_miss} (sum={sum(essay_miss)})")

print("-" * 90)
print(f"TOTAL MCQ MISSING: {total_mcq_needed}")
print(f"TOTAL ESSAY MISSING: {total_essay_needed}")
