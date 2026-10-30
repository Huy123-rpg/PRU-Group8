import sys
sys.stdout.reconfigure(encoding='utf-8')

for p in ['Assets/Scripts/PhysicsGame2/Editor/AureliaAcceptanceTestRunner.cs', 'Assets/Scripts/PhysicsGame2/Editor/AureliaPlayModeVerificationTest.cs']:
    with open(p, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    print(f"=== {p} ===")
    for i, line in enumerate(lines):
        if 'DifficultySelection' in line or 'SelectDifficulty' in line:
            print(f"{i+1}: {line.strip()}")
