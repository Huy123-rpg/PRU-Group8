import csv
import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('scratch/live_sheet_nganhang.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for r in reader:
        if r.get('CauHoiID') in ['C9_469', 'C9_476', 'C9_461', 'C9_462', 'C9_506', 'C9_507']:
            print(f"--- {r.get('CauHoiID')} ({r.get('MucDo')}) ---")
            print("TenBai:", r.get('TenBai'))
            print("NoiDung:", r.get('NoiDung'))
            print("A:", r.get('DapAnA'))
            print("B:", r.get('DapAnB'))
            print("C:", r.get('DapAnC'))
            print("D:", r.get('DapAnD'))
            print("Dung:", r.get('DapAnDung'))
