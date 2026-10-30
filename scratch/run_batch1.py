import sys
sys.stdout.reconfigure(encoding='utf-8')
from excel_helper import append_to_excel, MCQ_EXCEL_PATH, ESSAY_EXCEL_PATH, MCQ_COLUMNS, ESSAY_COLUMNS
from generate_batch1 import mcq_batch1
from generate_batch1_essay import essay_batch1

print(f"Executing Batch 1 append...")
total_mcq = append_to_excel(MCQ_EXCEL_PATH, mcq_batch1, MCQ_COLUMNS)
total_essay = append_to_excel(ESSAY_EXCEL_PATH, essay_batch1, ESSAY_COLUMNS)

print(f"BATCH 1 COMPLETED SUCCESSFULLY!")
print(f"MCQ saved: {len(mcq_batch1)} questions. Total MCQ now in Excel: {total_mcq}")
print(f"Essay saved: {len(essay_batch1)} questions. Total Essay now in Excel: {total_essay}")
