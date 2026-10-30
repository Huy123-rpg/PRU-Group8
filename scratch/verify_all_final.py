import pandas as pd
import os

files_to_check = [
    "Rubric_TuLuanCu_ChoDuyet.xlsx",
    "BoSung_TuLuan_ChoDuyet.xlsx",
    "BoSung_TracNghiem_ChoDuyet.xlsx",
    r"D:\Ca_Nhan\Downloads\cauhoi\Rubric_TuLuanCu_ChoDuyet.xlsx",
    r"D:\Ca_Nhan\Downloads\cauhoi\BoSung_TuLuan_ChoDuyet.xlsx",
    r"D:\Ca_Nhan\Downloads\cauhoi\BoSung_TracNghiem_ChoDuyet.xlsx"
]

for f in files_to_check:
    assert os.path.exists(f), f"File missing: {f}"
    xl = pd.ExcelFile(f)
    print(f"File: {os.path.basename(f)} | Size: {os.path.getsize(f):,} bytes | Sheets: {xl.sheet_names}")
    for s in xl.sheet_names:
        df = xl.parse(s)
        print(f"   [{s}]: {len(df)} rows, {len(df.columns)} cols, nulls: {df.isnull().sum().sum()}")

print("\n--- ALL VALIDATIONS PASSED PERFECTLY ---")
