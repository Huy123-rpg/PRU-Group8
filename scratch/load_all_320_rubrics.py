import pandas as pd
import os
from old_64_data_part1 import OLD_64_STEP_DATA
from old_64_data_part2 import OLD_64_STEP_DATA_P2
from old_64_data_part3 import OLD_64_STEP_DATA_P3

def load_all_rubrics():
    all_data = {}
    all_data.update(OLD_64_STEP_DATA)
    all_data.update(OLD_64_STEP_DATA_P2)
    all_data.update(OLD_64_STEP_DATA_P3)
    
    excel_path = os.path.join(os.path.dirname(__file__), "..", "BoSung_TuLuan_ChoDuyet.xlsx")
    if not os.path.exists(excel_path):
        excel_path = "BoSung_TuLuan_ChoDuyet.xlsx"
    if os.path.exists(excel_path):
        df_gemini = pd.read_excel(excel_path, sheet_name="Rubric_TungBuoc_Gemini")
        grouped = df_gemini.groupby("CauHoiID")
        for cid, grp in grouped:
            steps = []
            for _, r in grp.iterrows():
                steps.append({
                    "stepId": str(r["StepId"]),
                    "tenBuoc": str(r["TenBuoc"]),
                    "noiDung": str(r["NoiDungBuoc"]),
                    "diem": float(r["DiemToiDa"]),
                    "diemMotPhan": str(r["DiemMotPhan"]),
                    "yeuCau": str(r["YeuCauKiemTra"]),
                    "loiThuongGap": str(r["LoiThuongGap"]),
                    "tuongDuong": str(r["PhuongPhapTuongDuong"])
                })
            all_data[cid] = {
                "soBuoc": len(steps),
                "steps": steps
            }
    return all_data

if __name__ == '__main__':
    data = load_all_rubrics()
    print("Total questions loaded in all_rubrics:", len(data))
