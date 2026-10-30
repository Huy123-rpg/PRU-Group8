import sys
sys.stdout.reconfigure(encoding='utf-8')
from excel_helper import append_to_excel, MCQ_EXCEL_PATH, ESSAY_EXCEL_PATH, MCQ_COLUMNS, ESSAY_COLUMNS
from generate_batch2_mcq import mcq_batch2
from generate_batch2_essay_part1 import essay_batch2_p1
from generate_batch2_essay_part2 import essay_batch2_p2

essay_batch2 = essay_batch2_p1 + essay_batch2_p2

print(f"Executing Batch 2 append...")
total_mcq = append_to_excel(MCQ_EXCEL_PATH, mcq_batch2, MCQ_COLUMNS)
total_essay = append_to_excel(ESSAY_EXCEL_PATH, essay_batch2, ESSAY_COLUMNS)

print(f"BATCH 2 COMPLETED SUCCESSFULLY!")
print(f"MCQ saved in Batch 2: {len(mcq_batch2)} questions. Total MCQ now in Excel: {total_mcq}")
print(f"Essay saved in Batch 2: {len(essay_batch2)} questions. Total Essay now in Excel: {total_essay}")
