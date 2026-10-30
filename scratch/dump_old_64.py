import pandas as pd

df = pd.read_csv('scratch/live_sheet_cauhoituluan.csv')
print("Total rows:", len(df))
for i, r in df.iterrows():
    stt = r['STT']
    cid = r['CauHoiID']
    bid = r['BaiID']
    md = r['MucDo']
    nd = str(r['NoiDung']).replace('\n', ' ')[:60]
    print(f"{stt:2d} | {cid:18s} | {bid:6s} | {md:28s} | {nd}")
