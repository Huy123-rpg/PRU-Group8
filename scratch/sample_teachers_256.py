import pandas as pd
from test_parse_256 import parse_steps

df = pd.read_excel('BoSung_TuLuan_ChoDuyet.xlsx')

teachers = df['GiaoVienID'].unique()
for gv in teachers:
    sub = df[df['GiaoVienID'] == gv].head(2)
    for i, r in sub.iterrows():
        print(f"=== {r['CauHoiID']} | {r['GiaoVienID']} | {r['MucDo']} ===")
        print("ND:", r['NoiDung'])
        st = parse_steps(r['DapAnTuLuanMau'])
        for s in st:
            print(f"  {s['b_id']} ({s['title']}): {s['content'][:70]}...")
        print("-" * 50)
