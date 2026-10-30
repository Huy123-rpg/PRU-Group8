import pandas as pd

df = pd.read_excel('BoSung_TuLuan_ChoDuyet.xlsx')
for i in [0, 4, 8, 12]:
    r = df.iloc[i]
    print(f"=== {r['CauHoiID']} | {r['MucDo']} ===")
    print("NoiDung:", r['NoiDung'])
    print("DapAn:", r['DapAnTuLuanMau'])
    print("GiaiThich:", r['GiaiThich_MeoGhiNho'])
    print("-" * 50)
