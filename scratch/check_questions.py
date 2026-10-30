import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Resources/QuizData/physics_questions.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

if isinstance(data, dict):
    print('Dict keys:', list(data.keys()))
    for k, v in data.items():
        if isinstance(v, list):
            print(f'{k}: list of {len(v)}')
            if len(v) > 0:
                print('sample item:', list(v[0].keys()) if isinstance(v[0], dict) else type(v[0]))
elif isinstance(data, list):
    print(f'List of {len(data)}')
    print('sample:', type(data[0]))
