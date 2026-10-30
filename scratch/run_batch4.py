import sys
sys.stdout.reconfigure(encoding='utf-8')
from excel_helper import append_to_excel, MCQ_EXCEL_PATH, ESSAY_EXCEL_PATH, MCQ_COLUMNS, ESSAY_COLUMNS
from generate_batch4_mcq import mcq_batch4
from generate_batch4_essay_part1 import essay_batch4_p1
from generate_batch4_essay_part2 import essay_batch4_p2

essay_batch4 = essay_batch4_p1 + essay_batch4_p2

print(f"Executing Batch 4 append...")
total_mcq = append_to_excel(MCQ_EXCEL_PATH, mcq_batch4, MCQ_COLUMNS)
total_essay = append_to_excel(ESSAY_EXCEL_PATH, essay_batch4, ESSAY_COLUMNS)

print(f"BATCH 4 COMPLETED SUCCESSFULLY!")
print(f"MCQ saved in Batch 4: {len(mcq_batch4)} questions. Total MCQ now in Excel: {total_mcq}")
print(f"Essay saved in Batch 4: {len(essay_batch4)} questions. Total Essay now in Excel: {total_essay}")
