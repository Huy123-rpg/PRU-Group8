import csv
import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for i, r in enumerate(reader):
        if r.get('ChuongId') == 'Nhận biết' or not r.get('BaiID'):
            print(f'Row {i+2}:', dict(r))
