import pandas as pd

df = pd.read_csv('scratch/live_sheet_cauhoituluan.csv')
for i, r in df.iterrows():
    print(f"[{i+1}] {r['CauHoiID']} | {r['TenBai']} | {r['MucDo']}")
    print("ND:", r['NoiDung'])
    print("DA:", r['DapAnTuLuanMau'])
    print("-" * 60)
