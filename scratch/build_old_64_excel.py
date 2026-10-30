import pandas as pd
from old_64_data_part1 import OLD_64_STEP_DATA
from old_64_data_part2 import OLD_64_STEP_DATA_P2
from old_64_data_part3 import OLD_64_STEP_DATA_P3

# Merge all parts
all_old_steps = {}
all_old_steps.update(OLD_64_STEP_DATA)
all_old_steps.update(OLD_64_STEP_DATA_P2)
all_old_steps.update(OLD_64_STEP_DATA_P3)

print("Total merged questions in old step data:", len(all_old_steps))

df_live = pd.read_csv('scratch/live_sheet_cauhoituluan.csv')
print("Total questions in live_sheet_cauhoituluan.csv:", len(df_live))

# Verify all IDs exist and point sum = 10.0
missing_ids = []
invalid_sums = []
step_counts = {}

for i, r in df_live.iterrows():
    cid = r['CauHoiID']
    if cid not in all_old_steps:
        missing_ids.append(cid)
    else:
        q_data = all_old_steps[cid]
        num_steps = q_data['soBuoc']
        step_counts[num_steps] = step_counts.get(num_steps, 0) + 1
        steps = q_data['steps']
        pt_sum = sum(s['diem'] for s in steps)
        if abs(pt_sum - 10.0) > 1e-4:
            invalid_sums.append((cid, pt_sum))

print("Missing IDs:", missing_ids)
print("Invalid point sums:", invalid_sums)
print("Step count distribution:", step_counts)
