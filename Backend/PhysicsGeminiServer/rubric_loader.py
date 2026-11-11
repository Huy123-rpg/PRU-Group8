import json
import os

def load_all_rubrics_from_json():
    essay_path = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "QuizData", "g2_essay_bank.json")
    rubric_path = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "QuizData", "g2_rubric_bank.json")
    
    questions_map = {}
    if os.path.exists(essay_path):
        with open(essay_path, "r", encoding="utf-8") as f:
            data = json.load(f)
            q_list = data.get("questions", []) if isinstance(data, dict) else data
            for q in q_list:
                qid = q.get("cauHoiID")
                if qid:
                    questions_map[qid] = {
                        "cauHoiId": qid,
                        "noiDung": q.get("noiDung", ""),
                        "dapAnMau": q.get("dapAnMau", ""),
                        "steps": []
                    }
                    
    if os.path.exists(rubric_path):
        with open(rubric_path, "r", encoding="utf-8") as f:
            rdata = json.load(f)
            c_list = rdata.get("criteria", [])
            for c in c_list:
                qid = c.get("cauHoiID")
                if not qid:
                    continue
                if qid not in questions_map:
                    questions_map[qid] = {
                        "cauHoiId": qid,
                        "noiDung": "",
                        "dapAnMau": "",
                        "steps": []
                    }
                
                yeu_cau = c.get("yeuCau", "")
                ten_buoc = yeu_cau.split(":")[0].strip() if ":" in yeu_cau else f"Bước {c.get('criterionId')}"
                questions_map[qid]["steps"].append({
                    "stepId": c.get("criterionId", "S1"),
                    "diem": float(c.get("maxPoints", 0)),
                    "tenBuoc": ten_buoc,
                    "yeuCau": yeu_cau,
                    "loiThuongGap": c.get("partialCreditRule", ""),
                    "noiDung": yeu_cau
                })
    return questions_map

if __name__ == "__main__":
    all_data = load_all_rubrics_from_json()
    print("Total questions mapped:", len(all_data))
    sample = all_data.get("TL_C2_L10_Minh")
    if sample:
        print("Sample TL_C2_L10_Minh steps:", len(sample["steps"]))
        for s in sample["steps"]:
            print(" -", s["stepId"], f"({s['diem']}d):", s["tenBuoc"])
