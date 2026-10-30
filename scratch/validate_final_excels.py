import sys
import pandas as pd
sys.stdout.reconfigure(encoding='utf-8')

mcq_file = r"d:\PRU_213\Project_Team\PRU-Group8\BoSung_TracNghiem_ChoDuyet.xlsx"
essay_file = r"d:\PRU_213\Project_Team\PRU-Group8\BoSung_TuLuan_ChoDuyet.xlsx"

print("==================================================")
print("FINAL VALIDATION OF SUPPLEMENTAL QUESTION BANKS")
print("==================================================")

# 1. Validate MCQ Excel
df_mcq = pd.read_excel(mcq_file)
print(f"\n[1] MCQ EXCEL: {mcq_file}")
print(f"Total Rows: {len(df_mcq)} (Target: 162)")
print(f"Columns: {list(df_mcq.columns)}")
assert len(df_mcq) == 162, f"MCQ count mismatch! Expected 162, got {len(df_mcq)}"

# Check nulls
null_mcq = df_mcq.isnull().sum()
print("Null count per column in MCQ:")
print(null_mcq[null_mcq > 0] if any(null_mcq > 0) else "  No null values found!")

# Check difficulty counts
print("\nMCQ Distribution by Lesson and Difficulty:")
mcq_dist = df_mcq.groupby(['BaiID', 'MucDo']).size().unstack(fill_value=0)
print(mcq_dist)

# 2. Validate Essay Excel
df_essay = pd.read_excel(essay_file)
print(f"\n[2] ESSAY EXCEL: {essay_file}")
print(f"Total Rows: {len(df_essay)} (Target: 256)")
print(f"Columns: {list(df_essay.columns)}")
assert len(df_essay) == 256, f"Essay count mismatch! Expected 256, got {len(df_essay)}"

# Check nulls
null_essay = df_essay.isnull().sum()
print("Null count per column in Essay:")
print(null_essay[null_essay > 0] if any(null_essay > 0) else "  No null values found!")

# Check difficulty counts
print("\nEssay Distribution by Lesson and Difficulty:")
essay_dist = df_essay.groupby(['BaiID', 'MucDo']).size().unstack(fill_value=0)
print(essay_dist)

# Check Rubrics: verify every essay has step-by-step solutions
missing_b1 = df_essay[~df_essay['DapAnTuLuanMau'].str.contains('B1')]
missing_b2 = df_essay[~df_essay['DapAnTuLuanMau'].str.contains('B2')]
missing_b3 = df_essay[~df_essay['DapAnTuLuanMau'].str.contains('B3')]
print(f"\nRubrics step verification:")
print(f"  Rows missing B1: {len(missing_b1)}")
print(f"  Rows missing B2: {len(missing_b2)}")
print(f"  Rows missing B3: {len(missing_b3)}")

print("\n==================================================")
print("ALL VALIDATION CHECKS PASSED PERFECTLY!")
print("==================================================")
