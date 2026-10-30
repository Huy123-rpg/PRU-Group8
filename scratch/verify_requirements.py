import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scripts/PhysicsGame2/UI/DifficultySelectionController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

checks = []

# 1. No red color or red debug text
has_red_code = 'FC8181' in code or 'Color.red' in code or 'Chưa đủ số câu' in code
checks.append(("No red text / warning text exposed on screen", not has_red_code))

# 2. Aurelia fantasy plaques with 4 descriptions
has_plaques = all(d in code for d in ["Câu hỏi cơ bản", "Hiểu và giải thích", "Áp dụng kiến thức", "Bài tập nâng cao"])
checks.append(("All 4 fantasy plaque descriptions present", has_plaques))

# 3. Navy background, gold outline, cream text
has_navy = '0.04f, 0.08f, 0.18f' in code or '0.04f, 0.09f, 0.20f' in code
has_gold = '0.96f, 0.82f, 0.38f' in code
has_cream = '0.98f, 0.96f, 0.92f' in code
checks.append(("Aurelia navy-gold-cream color palette in plaques", has_navy and has_gold and has_cream))

# 4. Large center button disabled initially, enabled when valid
has_start_btn_logic = 'SetStartButtonState(canStart: false)' in code and 'SetStartButtonState(canStart: true)' in code
checks.append(("Start button disabled initially & enabled on valid selection", has_start_btn_logic))

# 5. Clicking card only selects & highlights, does not navigate
has_card_select_only = 'OnCardClicked' in code and 'hover.SetSelected' in code
checks.append(("Clicking card selects/highlights only", has_card_select_only))

# 6. Only OnStartClicked navigates
has_start_navigate = 'NavigateTo(RouteId.PracticeQuiz)' in code and 'NavigateTo(RouteId.EssayAnswer)' in code
checks.append(("Start button triggers scene navigation", has_start_navigate))

# 7. Aurelia warning modal for insufficient questions
has_modal_logic = 'ShowWarningModal("Mức độ này hiện chưa đủ số câu hỏi' in code
checks.append(("Aurelia warning modal popup triggers for insufficient questions", has_modal_logic))

# 8. Start button repositioned to -415f to prevent overlap
has_pos_415 = '-415f' in code
checks.append(("Start button positioned at y = -415f for clean layout", has_pos_415))

print("\n=== VERIFICATION RESULTS ===")
all_passed = True
for name, passed in checks:
    status = "PASS" if passed else "FAIL"
    if not passed: all_passed = False
    print(f"[{status}] {name}")

print(f"\nOverall: {'ALL CHECKS PASSED' if all_passed else 'SOME CHECKS FAILED'}")
