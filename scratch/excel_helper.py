import os
import sys
import pandas as pd
from openpyxl import load_workbook

sys.stdout.reconfigure(encoding='utf-8')

MCQ_EXCEL_PATH = r"d:\PRU_213\Project_Team\PRU-Group8\BoSung_TracNghiem_ChoDuyet.xlsx"
ESSAY_EXCEL_PATH = r"d:\PRU_213\Project_Team\PRU-Group8\BoSung_TuLuan_ChoDuyet.xlsx"

MCQ_COLUMNS = [
    "STT", "CauHoiID", "ChuongId", "TenChuong", "BaiID", "TenBai",
    "LoaiCauHoi", "MucDo", "NoiDung", "DapAnA", "DapAnB", "DapAnC", "DapAnD",
    "DapAnDung", "GiaiThich", "MeoGhiNho", "TrangThai", "GiaoVienID"
]

ESSAY_COLUMNS = [
    "STT", "CauHoiID", "ChuongId", "TenChuong", "BaiID", "TenBai",
    "GiaoVienID", "TenGiaoVien", "MucDo", "NoiDung",
    "TuKhoaChamDiem", "DapAnTuLuanMau", "GiaiThich_MeoGhiNho", "TrangThai"
]

def append_to_excel(file_path, new_rows, columns):
    if os.path.exists(file_path):
        try:
            df_existing = pd.read_excel(file_path)
            start_stt = len(df_existing) + 1
        except Exception:
            df_existing = pd.DataFrame(columns=columns)
            start_stt = 1
    else:
        df_existing = pd.DataFrame(columns=columns)
        start_stt = 1

    for i, r in enumerate(new_rows):
        r["STT"] = start_stt + i

    df_new = pd.DataFrame(new_rows, columns=columns)
    df_combined = pd.concat([df_existing, df_new], ignore_index=True)
    df_combined.to_excel(file_path, index=False)
    return len(df_combined)

print("Excel module initialized.")
