import json

# Load old 64 questions rubric and new 256 questions rubric
from old_64_data_part1 import OLD_64_STEP_DATA

sample_q = OLD_64_STEP_DATA["TL_C1_L2_Duy"]
# TL_C1_L2_Duy:
# Đề: Một viên bi có khối lượng 200g chuyển động với vận tốc 10 m/s. Hãy tính động năng của viên bi. Nếu vận tốc của viên bi tăng gấp đôi thì động năng tăng lên gấp mấy lần? Giải thích.
# B1 (4.0đ): Đổi m = 200g = 0.2 kg. Wđ = 1/2 m v^2 = 1/2 * 0.2 * 10^2 = 10 J.
# B2 (4.0đ): Khi v' = 2v, Wđ' = 1/2 m (2v)^2 = 4 Wđ.
# B3 (2.0đ): Kết luận: Wđ = 10 J; khi vận tốc tăng gấp đôi thì động năng tăng 4 lần.

print("Question TL_C1_L2_Duy steps:")
for s in sample_q['steps']:
    print(f" - {s['stepId']} ({s['diem']}đ): {s['tenBuoc']} | Yêu cầu: {s['yeuCau']}")
