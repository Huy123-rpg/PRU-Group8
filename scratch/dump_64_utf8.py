import pandas as pd

df = pd.read_csv('scratch/live_sheet_cauhoituluan.csv')
with open('scratch/all_64_dump_utf8.txt', 'w', encoding='utf-8') as f:
    for i, r in df.iterrows():
        f.write(f"[{i+1}] {r['CauHoiID']} | {r['TenBai']} | {r['MucDo']}\n")
        f.write(f"ND: {r['NoiDung']}\n")
        f.write(f"DA: {r['DapAnTuLuanMau']}\n")
        f.write("-" * 60 + "\n")
print("Done writing 64 items to scratch/all_64_dump_utf8.txt")
