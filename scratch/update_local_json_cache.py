import csv
import json
import sys
sys.stdout.reconfigure(encoding='utf-8')

from test_fixed_normalize import normalize_difficulty_fixed

# 1. Update g2_mcq_bank.json
mcqs = []
with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for r in reader:
        qid = r.get('CauHoiID', '').strip()
        nd = r.get('NoiDung', '').strip()
        st = r.get('TrangThai', '').strip()
        if not qid or not nd: continue
        if st in ['Đã ẩn', 'Bản nháp']: continue
        
        dung = r.get('DapAnDung', '').strip().upper()
        cIdx = 0
        if dung in ['B', '1']: cIdx = 1
        elif dung in ['C', '2']: cIdx = 2
        elif dung in ['D', '3']: cIdx = 3
        
        rawMucDo = r.get('MucDo', '').strip()
        normMucDo = normalize_difficulty_fixed(rawMucDo)
        
        mcqs.append({
            'cauHoiID': qid,
            'chuongId': r.get('ChuongId', '').strip().lower(),
            'tenChuong': r.get('TenChuong', '').strip(),
            'baiID': r.get('BaiID', '').strip().lower(),
            'tenBai': r.get('TenBai', '').strip(),
            'loaiCauHoi': r.get('LoaiCauHoi', '').strip(),
            'mucDo': rawMucDo,
            'mucDoGame2': normMucDo,
            'noiDung': nd,
            'dapAnA': r.get('DapAnA', '').strip(),
            'dapAnB': r.get('DapAnB', '').strip(),
            'dapAnC': r.get('DapAnC', '').strip(),
            'dapAnD': r.get('DapAnD', '').strip(),
            'dapAnDung': dung[:1] if dung else 'A',
            'correctIndex': cIdx,
            'giaiThich': r.get('GiaiThich', '').strip(),
            'meoGhiNho': r.get('MeoGhiNho', '').strip(),
            'contentVersion': 'live_sheet'
        })

print(f"Prepared {len(mcqs)} MCQs from Google Sheet.")
with open('Assets/Resources/QuizData/g2_mcq_bank.json', 'w', encoding='utf-8') as f:
    json.dump({'questions': mcqs}, f, ensure_ascii=False, indent=2)
print("Updated Assets/Resources/QuizData/g2_mcq_bank.json")

# 2. Update g2_essay_bank.json
essays = []
with open('scratch/live_sheet_cauhoituluan.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for r in reader:
        qid = r.get('CauHoiID', '').strip()
        nd = r.get('NoiDung', '').strip()
        st = r.get('TrangThai', '').strip()
        if not qid or not nd: continue
        if st in ['Đã ẩn', 'Bản nháp']: continue
        
        rawMucDo = r.get('MucDo', '').strip()
        normMucDo = normalize_difficulty_fixed(rawMucDo)
        rawBaiId = r.get('BaiID', '').strip()
        
        # map canonical: C1_L2 -> bai2
        canon = rawBaiId.lower()
        if canon.startswith('c') and '_l' in canon:
            part = canon.split('_l')[1]
            canon = f"bai{part}"
            
        essays.append({
            'cauHoiID': qid,
            'chuongId': r.get('ChuongId', '').strip(),
            'tenChuong': r.get('TenChuong', '').strip(),
            'baiIDNguon': rawBaiId,
            'baiIDGame2': canon,
            'tenBai': r.get('TenBai', '').strip(),
            'mucDoNguon': rawMucDo,
            'mucDoGame2': normMucDo,
            'noiDung': nd,
            'tuKhoa': r.get('TuKhoaChamDiem', '').strip(),
            'dapAnMau': r.get('DapAnTuLuanMau', '').strip(),
            'giaiThich': r.get('GiaiThich_MeoGhiNho', '').strip(),
            'rubricId': f"RUB_{qid}",
            'diemToiDa': 20,
            'contentVersion': 'live_sheet'
        })

print(f"Prepared {len(essays)} Essays from Google Sheet.")
with open('Assets/Resources/QuizData/g2_essay_bank.json', 'w', encoding='utf-8') as f:
    json.dump({'questions': essays}, f, ensure_ascii=False, indent=2)
print("Updated Assets/Resources/QuizData/g2_essay_bank.json")
