import pandas as pd
from old_64_data_part1 import OLD_64_STEP_DATA
from old_64_data_part2 import OLD_64_STEP_DATA_P2
from old_64_data_part3 import OLD_64_STEP_DATA_P3

all_old_steps = {}
all_old_steps.update(OLD_64_STEP_DATA)
all_old_steps.update(OLD_64_STEP_DATA_P2)
all_old_steps.update(OLD_64_STEP_DATA_P3)

df_live = pd.read_csv('scratch/live_sheet_cauhoituluan.csv')

# Sheet 1 data
sheet1_rows = []
# Sheet 2 data
sheet2_rows = []

for i, r in df_live.iterrows():
    cid = r['CauHoiID']
    q_data = all_old_steps[cid]
    so_buoc = q_data['soBuoc']
    steps = q_data['steps']
    
    # Build LoiGiaiTungBuoc
    loi_giai_parts = []
    rubric_summary_parts = []
    
    for s_idx, s in enumerate(steps, 1):
        step_id = s['stepId']
        ten_buoc = s['tenBuoc']
        diem = s['diem']
        noi_dung = s['noiDung']
        diem_str = f"{diem:g}đ"
        loi_giai_parts.append(f"• B{s_idx} ({ten_buoc} - {diem_str}): {noi_dung}")
        rubric_summary_parts.append(f"[B{s_idx}] ({diem_str}) {s['yeuCau']}")
        
        # Append to Sheet 2 (Gemini step breakdown)
        sheet2_rows.append({
            "CauHoiID": cid,
            "StepId": step_id,
            "TenBuoc": ten_buoc,
            "NoiDungBuoc": noi_dung,
            "DiemToiDa": diem,
            "DiemMotPhan": s['diemMotPhan'],
            "YeuCauKiemTra": s['yeuCau'],
            "LoiThuongGap": s['loiThuongGap'],
            "PhuongPhapTuongDuong": s['tuongDuong'],
            "TrangThaiDuyet": "CHO_DUYET"
        })
        
    loi_giai_str = "\n".join(loi_giai_parts)
    rubric_summary_str = " | ".join(rubric_summary_parts)
    
    sheet1_rows.append({
        "STT": r['STT'],
        "CauHoiID": cid,
        "ChuongId": r['ChuongId'],
        "TenChuong": r['TenChuong'],
        "BaiID": r['BaiID'],
        "TenBai": r['TenBai'],
        "GiaoVienID": r['GiaoVienID'],
        "TenGiaoVien": r['TenGiaoVien'],
        "MucDo": r['MucDo'],
        "NoiDung": r['NoiDung'],
        "DapAnGoc": r['DapAnTuLuanMau'],
        "LoiGiaiTungBuoc": loi_giai_str,
        "SoBuoc": so_buoc,
        "TongDiem": 10.0,
        "TieuChiRubricTomTat": rubric_summary_str,
        "TrangThaiDuyet": "CHO_DUYET"
    })

df_sheet1 = pd.DataFrame(sheet1_rows)
df_sheet2 = pd.DataFrame(sheet2_rows)

out_path = "Rubric_TuLuanCu_ChoDuyet.xlsx"
with pd.ExcelWriter(out_path, engine='openpyxl') as writer:
    df_sheet1.to_excel(writer, sheet_name="DanhSach_TuLuanCu_LoiGiai", index=False)
    df_sheet2.to_excel(writer, sheet_name="Rubric_TungBuoc_Gemini", index=False)

print(f"Successfully generated {out_path}")
print(f"Sheet 1 rows: {len(df_sheet1)}, Sheet 2 rows: {len(df_sheet2)}")
