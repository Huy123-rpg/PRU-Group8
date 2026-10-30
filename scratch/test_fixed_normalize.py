import sys
sys.stdout.reconfigure(encoding='utf-8')

def normalize_difficulty_fixed(input_str):
    if not input_str: return "NB"
    s = input_str.strip()
    upper = s.upper()

    # 1. Vận dụng cao
    if (upper == "VDC" or "VDC" in upper or 
        "VẬN DỤNG CAO" in upper or "VAN DUNG CAO" in upper or
        "CHUYÊN SÂU" in upper or "CHUYEN SAU" in upper or
        "TỔNG HỢP" in upper or "TONG HOP" in upper or
        "VERY HARD" in upper):
        return "VDC"

    # 2. Vận dụng
    if (upper == "VD" or "VD" in upper or
        "VẬN DỤNG" in upper or "VAN DUNG" in upper or
        "NÂNG CAO" in upper or "NANG CAO" in upper or
        "KHÓ" in upper or "KHO" in upper or
        "HARD" in upper):
        return "VD"

    # 3. Thông hiểu
    if (upper == "TH" or "TH" in upper or
        "THÔNG HIỂU" in upper or "THONG HIEU" in upper or
        "ỨNG DỤNG" in upper or "UNG DUNG" in upper or
        "TRUNG BÌNH" in upper or "TRUNG BINH" in upper or
        "MEDIUM" in upper):
        return "TH"

    # 4. Nhận biết
    return "NB"

sheet_levels = ["Nhận biết", "Thông hiểu", "Vận dụng", "Vận dụng cao", "NB", "TH", "VD", "VDC", "Dễ (Nền tảng)", "Trung bình (Ứng dụng thực tế)", "Khó (Tính toán nâng cao)", "Tổng hợp (Chuyên sâu)"]

for s in sheet_levels:
    res = normalize_difficulty_fixed(s)
    print(f"'{s:30s}' -> '{res}'")
