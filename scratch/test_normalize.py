import sys
sys.stdout.reconfigure(encoding='utf-8')

def normalize_difficulty_current(input_str):
    if not input_str: return "NB"
    inp = input_str.strip().upper()
    if "VDC" in inp or "CHUYÊN SÂU" in inp or "TỔNG HỢP" in inp or "VERY HARD" in inp:
        return "VDC"
    if "VD" in inp or "NÂNG CAO" in inp or "KHÓ" in inp or "HARD" in inp:
        return "VD"
    if "TH" in inp or "ỨNG DỤNG" in inp or "TRUNG BÌNH" in inp or "MEDIUM" in inp:
        return "TH"
    return "NB"

sheet_levels = ["Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao"]

for s in sheet_levels:
    res = normalize_difficulty_current(s)
    print(f"'{s}' -> '{res}'")
