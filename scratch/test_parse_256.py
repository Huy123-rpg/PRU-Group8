import pandas as pd
import re

df = pd.read_excel('BoSung_TuLuan_ChoDuyet.xlsx')
print("Loaded 256 questions.")

def parse_steps(dapan_text):
    # Split by • B1, • B2, etc.
    lines = [line.strip() for line in dapan_text.split('\n') if line.strip()]
    steps = []
    for line in lines:
        m = re.match(r'•\s*(B\d+)\s*\((.*?)\s*-\s*([0-9.,]+)đ\):\s*(.*)', line)
        if m:
            b_id, title, pts, content = m.groups()
            steps.append({
                "b_id": b_id,
                "title": title.strip(),
                "pts": float(pts.replace(',', '.')),
                "content": content.strip()
            })
    return steps

parsed_count = 0
for i, r in df.iterrows():
    st = parse_steps(r['DapAnTuLuanMau'])
    if len(st) == 4:
        parsed_count += 1

print(f"Questions matching 4-step regex: {parsed_count} / {len(df)}")
