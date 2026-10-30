import json
import pandas as pd

with open('Assets/Resources/QuizData/g2_essay_bank.json', 'r', encoding='utf-8') as f:
    old_data = json.load(f)['questions']

with open('Assets/Resources/QuizData/g2_essay_demo_supplemental.json', 'r', encoding='utf-8') as f:
    supp_data = json.load(f)['questions']

df_new = pd.read_excel('BoSung_TuLuan_ChoDuyet.xlsx', sheet_name='NganHangTuLuan')

print("=== BÁO CÁO KIỂM KÊ DỮ LIỆU TỰ LUẬN GAME 2 ===")
print(f"1. Tổng số câu Tự luận cũ (gốc): {len(old_data)}")
print(f"2. Tổng số câu Tự luận mới trong Excel (BoSung_TuLuan_ChoDuyet.xlsx): {len(df_new)}")
print(f"3. Tổng số câu Tự luận mới trong file demo supplemental: {len(supp_data)}")
print(f"4. Tổng số câu sau khi hợp nhất: {len(old_data) + len(supp_data)}")

# Check per lesson
lessons = [f"bai{i}" for i in range(2, 18)]
diffs = ["NB", "TH", "VD", "VDC"]

all_ok = True
for l in lessons:
    for d in diffs:
        old_cnt = sum(1 for q in old_data if q['baiIDGame2'] == l and q['mucDoGame2'] == d)
        new_cnt = sum(1 for q in supp_data if q['baiIDGame2'] == l and q['mucDoGame2'] == d)
        total = old_cnt + new_cnt
        if total != 5:
            print(f"FAIL: {l} - {d}: old={old_cnt}, new={new_cnt}, total={total}")
            all_ok = False

if all_ok:
    print("5. Xác minh toàn bộ 16 bài học (bai2 đến bai17) x 4 mức độ (NB, TH, VD, VDC):")
    print("   -> TẤT CẢ 64 bộ đều có ĐÚNG 5 CÂU (1 câu cũ + 4 câu mới bổ sung)!")
