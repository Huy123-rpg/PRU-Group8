import sys
sys.stdout.reconfigure(encoding='utf-8')

path = 'Assets/Scripts/PhysicsGame2/Data/GameDataRepository.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

target = """                var item = new G2EssayItem
                {
                    cauHoiID = id,
                    chuongId = GetColValue(row, colChuongId).Trim(),
                    tenChuong = GetColValue(row, colTenChuong).Trim(),
                    baiID = rawBaiId,
                    baiIDGame2 = canonBaiId,
                    tenBai = GetColValue(row, colTenBai).Trim(),
                    giaoVienID = GetColValue(row, colGiaoVienId).Trim(),
                    tenGiaoVien = GetColValue(row, colTenGiaoVien).Trim(),
                    mucDo = rawMucDo,
                    mucDoGame2 = normMucDo,
                    noiDung = noiDung,
                    tuKhoaChamDiem = GetColValue(row, colTuKhoa).Trim(),
                    dapAnMau = GetColValue(row, colDapAnMau).Trim(),
                    giaiThich = GetColValue(row, colGiaiThich).Trim(),
                    contentVersion = "live_sheet"
                };"""

replacement = """                var item = new G2EssayItem
                {
                    cauHoiID = id,
                    chuongId = GetColValue(row, colChuongId).Trim(),
                    tenChuong = GetColValue(row, colTenChuong).Trim(),
                    baiIDNguon = rawBaiId,
                    baiIDGame2 = canonBaiId,
                    tenBai = GetColValue(row, colTenBai).Trim(),
                    mucDoNguon = rawMucDo,
                    mucDoGame2 = normMucDo,
                    noiDung = noiDung,
                    tuKhoa = GetColValue(row, colTuKhoa).Trim(),
                    dapAnMau = GetColValue(row, colDapAnMau).Trim(),
                    giaiThich = GetColValue(row, colGiaiThich).Trim(),
                    rubricId = $"RUB_{id}",
                    diemToiDa = 20,
                    contentVersion = "live_sheet"
                };"""

text_norm = text.replace('\r\n', '\n')
t_norm = target.replace('\r\n', '\n')
r_norm = replacement.replace('\r\n', '\n')

assert t_norm in text_norm, "Target not found in GameDataRepository.cs"
text_norm = text_norm.replace(t_norm, r_norm, 1)

with open(path, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(text_norm)

print("Updated G2EssayItem assignment in GameDataRepository.cs")
