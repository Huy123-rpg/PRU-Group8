import sys
sys.stdout.reconfigure(encoding='utf-8')
from excel_helper import append_to_excel, MCQ_EXCEL_PATH, ESSAY_EXCEL_PATH, MCQ_COLUMNS, ESSAY_COLUMNS
from generate_batch3_mcq import mcq_batch3
from generate_batch3_essay import essay_batch3

print(f"Executing Batch 3 append...")
total_mcq = append_to_excel(MCQ_EXCEL_PATH, mcq_batch3, MCQ_COLUMNS)
total_essay = append_to_excel(ESSAY_EXCEL_PATH, essay_batch3, ESSAY_COLUMNS)

print(f"BATCH 3 COMPLETED SUCCESSFULLY!")
print(f"MCQ saved in Batch 3: {len(mcq_batch3)} questions. Total MCQ now in Excel: {total_mcq}")
print(f"Essay saved in Batch 3: {len(essay_batch3)} questions. Total Essay now in Excel: {total_essay}")
