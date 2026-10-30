import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Resources/QuizData/g2_mcq_bank.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

questions = data.get('questions', [])
print(f'Total questions in g2_mcq_bank: {len(questions)}')
from collections import defaultdict
lesson_diffs = defaultdict(lambda: defaultdict(int))
for q in questions:
    bai = q.get('baiID', '')
    muc = q.get('mucDoGame2', '')
    lesson_diffs[bai][muc] += 1

for bai in sorted(lesson_diffs.keys()):
    diff_str = ', '.join([f'{m}: {c}' for m, c in sorted(lesson_diffs[bai].items())])
    print(f'{bai}: {diff_str}')
