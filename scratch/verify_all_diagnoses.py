import json
import sys

sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Resources/QuizData/g2_mcq_bank.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

qs = data['questions']
print(f"Total MCQs loaded: {len(qs)}")

def get_diagnosis(lesson_id, difficulty):
    norm_diff = difficulty.upper()
    matches = [q for q in qs if q['baiID'].lower() == lesson_id.lower() and q['mucDoGame2'] == norm_diff]
    count = len(matches)
    can_start = count >= 10
    return {
        'LessonId': lesson_id,
        'Difficulty': norm_diff,
        'Count': count,
        'Required': 10,
        'CanStart': can_start
    }

print("\n=== SAMPLE LESSON DIAGNOSES ===")
for bai in ['bai2', 'bai3', 'bai11', 'bai12', 'bai13']:
    print(f"\nLesson: {bai}")
    for md in ['NB', 'TH', 'VD', 'VDC']:
        diag = get_diagnosis(bai, md)
        print(f"  [{diag['Difficulty']:3s}] Count = {diag['Count']:2d}/10 | CanStart = {str(diag['CanStart']):5s}")
