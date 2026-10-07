# Tài liệu Đặc tả Cấu trúc Dữ liệu: General_Game_Hub_Shared_Sheet_V1_1
**Hệ thống:** Game Hub & AppSheet KHTN THCS (Khoa học tự nhiên lớp 6, 7, 8, 9)
**Tập tin dữ liệu:** `General_Game_Hub_Shared_Sheet_V1_1` (Google Sheets)
**Trọng tâm:** Đặc tả toàn diện các bảng (Sheets), cột (Columns), kiểu dữ liệu và **Cấu trúc Dữ liệu Phân hệ Game Chủ đề Sinh học**.

---

## 1. Tổng quan Kiến trúc Dữ liệu (System Overview)

Theo tài liệu gốc tại tab `README_AppSheet`, hệ thống được thiết kế dạng bảng quan hệ hỗ trợ cả AppSheet quản lý học tập và Game Hub tương tác:

### 1.1. Nguyên tắc Khóa & Quan hệ Tham chiếu (Keys & References):
* **Khóa chính (Primary Key):** Mỗi bảng đều có một cột ID duy nhất (sử dụng `UNIQUEID()` khi sinh mới).
* **Mối quan hệ liên kết (Foreign Keys):**
  * `Users.LopID` ➔ `LopHoc.LopID`
  * `PhanCongGiaoVien.GiaoVienID` ➔ `Users.UserID`
  * `PhanCongGiaoVien.LopID` ➔ `LopHoc.LopID`
  * `PhanCongGiaoVien.MonKhoiID` ➔ `MonHoc_KhoiLop.MonKhoiID`
  * `ChuDeKHTN.MonKhoiID` ➔ `MonHoc_KhoiLop.MonKhoiID`
  * `BaiHoc.MonKhoiID` ➔ `MonHoc_KhoiLop.MonKhoiID`
  * `BaiHoc.ChuDeID` ➔ `ChuDeKHTN.ChuDeID`
  * `BaiHoc.GiaoVienID` ➔ `Users.UserID`
  * `TaiLieuBaiHoc.BaiID` ➔ `BaiHoc.BaiID`
  * `NganHangCauHoi.BaiID` ➔ `BaiHoc.BaiID` *(Dành riêng cho câu hỏi Trắc nghiệm)*
  * `NganHangTuLuan.BaiID` ➔ `BaiHoc.BaiID` *(Dành riêng cho câu hỏi Tự luận - AI chấm)*
  * `AI_GiaiThich.CauHoiID` ➔ `NganHangCauHoi.CauHoiID` hoặc `NganHangTuLuan.CauHoiTL_ID`
  * `AI_GiaiThich.UserID` ➔ `Users.UserID`

### 1.2. Phân tách Dữ liệu Trắc nghiệm & Tự luận (Game Hub V1.1):
* **Trắc nghiệm (`NganHangCauHoi`):** Cung cấp 4 phương án cố định `DapAnA`, `DapAnB`, `DapAnC`, `DapAnD`, đáp án đúng `DapAnDung` và `GiaiThich`.
* **Tự luận (`NganHangTuLuan`):** Tách riêng hoàn toàn, **không có cột A/B/C/D**, sử dụng `DapAnMau`, `TuKhoaChamDiem` và kết quả đánh giá từ AI Gemini (`DiemAI`, `GiaiThichAI`).

---

## 2. Chi tiết Cấu trúc các Sheets trong Hệ thống

### 2.1. Sheet: `MonHoc_KhoiLop` (Môn học theo Khối)
| Cột | Kiểu | Bắt buộc | Khóa | Dữ liệu mẫu | Mô tả |
| :--- | :--- | :---: | :---: | :--- | :--- |
| `MonKhoiID` | Text | Có | PK | `KHTN6`, `KHTN7`, `KHTN8`, `KHTN9` | Mã môn theo khối lớp |
| `TenMon` | Text | Có | | `Khoa học tự nhiên` | Tên môn hiển thị |
| `Khoi` | Number | Có | | `6`, `7`, `8`, `9` | Khối học |
| `TrangThai` | Text | Có | | `Hoạt động` | Trạng thái môn |

### 2.2. Sheet: `LopHoc` (Danh sách Lớp)
| Cột | Kiểu | Bắt buộc | Khóa | Dữ liệu mẫu | Mô tả |
| :--- | :--- | :---: | :---: | :--- | :--- |
| `LopID` | Text | Có | PK | `6A1`, `6A2`, `7A1`, `7A2`, `8A1`, `8A2`, `9A1`, `9A2` | Mã lớp học |
| `TenLop` | Text | Có | | `6A1`, `8A1` | Tên lớp |
| `Khoi` | Number | Có | | `6`, `7`, `8`, `9` | Khối lớp |
| `GVCN_UserID` | Text | Có | FK | `GV001`, `GV002` | Giáo viên chủ nhiệm (ref `Users.UserID`) |
| `NamHoc` | Text | Có | | `2026-2027` | Năm học |
| `TrangThai` | Text | Có | | `Hoạt động` | Trạng thái lớp |

### 2.3. Sheet: `Users` (Người dùng Hệ thống)
| Cột | Kiểu | Bắt buộc | Khóa | Dữ liệu mẫu | Mô tả |
| :--- | :--- | :---: | :---: | :--- | :--- |
| `UserID` | Text | Có | PK | `GV001`, `GV002`, `HS001` .. `HS004` | Mã tài khoản |
| `HoTen` | Text | Có | | `Nguyễn Văn An`, `Nguyễn Minh Anh` | Họ tên đầy đủ |
| `Email` | Email | Có | | `hs.anh@fpt.edu.vn` | Email đăng nhập / USEREMAIL() |
| `VaiTro` | Enum | Có | | `Giáo viên`, `Học sinh` | Phân quyền tài khoản |
| `LopID` | Text | Không | FK | `6A1`, `7A1`, `8A1`, `9A1` | Lớp học (đối với học sinh) |
| `TrangThai` | Text | Có | | `Hoạt động` | Trạng thái tài khoản |
| `Alias` | Text | Không | | `MinhAn`, `GiaBao7`, `DucDuy9` | Tên hiển thị trong Game Hub / Bảng xếp hạng |

### 2.4. Sheet: `PhanCongGiaoVien` (Phân công Giảng dạy)
| Cột | Kiểu | Bắt buộc | Khóa | Dữ liệu mẫu | Mô tả |
| :--- | :--- | :---: | :---: | :--- | :--- |
| `PhanCongID` | Text | Có | PK | `PC001` .. `PC008` | Mã phân công |
| `GiaoVienID` | Text | Có | FK | `GV001`, `GV002` | Ref `Users.UserID` |
| `LopID` | Text | Có | FK | `6A1`, `6A2`, `7A1`, `8A1`, `9A1` | Ref `LopHoc.LopID` |
| `MonKhoiID` | Text | Có | FK | `KHTN6`, `KHTN7`, `KHTN8`, `KHTN9` | Ref `MonHoc_KhoiLop.MonKhoiID` |
| `VaiTro` | Text | Có | | `Giáo viên bộ môn` | Nhiệm vụ |
| `TrangThai` | Text | Có | | `Hoạt động` | Trạng thái hiệu lực |

---

## 3. Đặc tả Chuyên biệt: Phân hệ Game Chủ đề Sinh học (Biology Subsystem)

### 3.1. Sheet: `ChuDeKHTN` (Các Chủ đề Môn Sinh học)
Trong danh mục chủ đề KHTN toàn khóa, các chủ đề Sinh học (`Loai = 'Sinh học'`) gồm:

| ChuDeID | MonKhoiID | TenChuDe | Loai | ThuTu | TrangThai | Nội dung trọng tâm |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| `CD6_SH_01` | `KHTN6` | **Tế bào và cơ thể sống** | `Sinh học` | 3 | `Hoạt động` | Cấu tạo tế bào nhân sơ/nhân thực, bào quan, cơ thể đơn bào & đa bào |
| `CD7_SH_01` | `KHTN7` | **Trao đổi chất và chuyển hóa năng lượng** | `Sinh học` | 4 | `Hoạt động` | Quang hợp, hô hấp tế bào, trao đổi nước và chất dinh dưỡng |
| `CD8_SH_01` | `KHTN8` | **Cơ thể người và sức khỏe** | `Sinh học` | 4 | `Hoạt động` | Giải phẫu & sinh lý hệ vận động, tuần hoàn, hô hấp, tiêu hóa, bài tiết, thần kinh |
| `CD9_SH_01` | `KHTN9` | **Di truyền và biến dị** | `Sinh học` | 4 | `Hoạt động` | Quy luật di truyền Menđen, phân tử ADN/ARN, nhiễm sắc thể, đột biến gen & NST |

---

### 3.2. Sheet: `BaiHoc` (Danh mục Bài học Sinh học)
Quản lý các bài học cụ thể, video bài giảng và liên kết game:

| Cột | Kiểu | Khóa | Mô tả & Quy tắc xử lý Unity |
| :--- | :--- | :---: | :--- |
| `BaiID` | Text | PK | Mã bài học (VD: `B6_002`, `B9_002`). Khóa ngoại tham chiếu của câu hỏi. |
| `MonKhoiID` | Text | FK | Thuộc khối nào (`KHTN6`, `KHTN7`, `KHTN8`, `KHTN9`). |
| `TenBai` | Text | | Tên bài học (VD: *Tế bào - đơn vị cơ bản của sự sống*, *Di truyền và biến dị*). |
| `ChuDeID` | Text | FK | Ref `ChuDeKHTN.ChuDeID` (VD: `CD6_SH_01`, `CD8_SH_01`, `CD9_SH_01`). |
| `GiaoVienID`| Text | FK | Giáo viên phụ trách (ref `Users.UserID`). |
| `NoiDung` | LongText | | Tóm tắt lý thuyết bài giảng. |
| `VideoURL` | URL | | Link video YouTube bài học. |
| `HinhAnh` | Image/URL | | Ảnh thumbnail bài học. |
| `FileDinhKem`| File/URL| | Tài liệu đính kèm. |
| `ThuTu` | Number | | Thứ tự xuất hiện bài học trong chủ đề. |
| `TrangThai` | Text | | `Xuất bản` / `Bản nháp`. |
| `GameID` | Text | FK | Mã Game Hub liên kết (VD: `GAME_KHTN_CD`, `GAME_BIO_RUNNER`). |

*Dữ liệu thực tế các bài học Sinh học trong sheet `BaiHoc`:*
* `B6_002` | `KHTN6` | **Tế bào - đơn vị cơ bản của sự sống** | `CD6_SH_01` | `GV001` | *Tìm hiểu cấu tạo cơ bản của tế bào và chức năng các thành phần.* | `Xuất bản` | `GAME_KHTN_CD`
* `B9_002` | `KHTN9` | **Di truyền và biến dị** | `CD9_SH_01` | `GV002` | *Khái quát cơ sở của di truyền và biến dị ở sinh vật.* | `Xuất bản` | `GAME_KHTN_CD`

---

### 3.3. Sheet: `TaiLieuBaiHoc` (Tài liệu học tập Môn Sinh)
| TaiLieuID | BaiID | TenTaiLieu | Loai | FileURL | MoTa |
| :--- | :--- | :--- | :---: | :--- | :--- |
| `TL002` | `B6_002` | Hình cấu tạo tế bào | `Image` | `https://...` | Hình minh họa bào quan tế bào |
| `TL008` | `B9_002` | Sơ đồ di truyền | `Image` | `https://...` | Sơ đồ cơ chế di truyền Menđen và NST |

---

### 3.4. Sheet: `NganHangCauHoi` (Ngân hàng Câu hỏi Trắc Nghiệm Môn Sinh - Trọng tâm Game)

Đây là bảng dữ liệu nạp trực tiếp vào [QuizManager.cs](file:///d:/FU-learning/FALL-2026_ky7/PRU/PROJECT/PRU-Group8/Assets/Scripts/QuizManager.cs) để điều khiển màn chơi Runner và Đấu Boss trong `SinhScene`.

#### Cấu trúc 11 cột chuẩn xác:
| Thứ tự | Tên Cột | Kiểu | Bắt buộc | Khóa | Mô tả & Ánh xạ Unity |
| :---: | :--- | :--- | :---: | :---: | :--- |
| 1 | `CauHoiID` | Text | Có | PK | Mã câu hỏi (VD: `C6_003`, `C9_003`). |
| 2 | `BaiID` | Text | Có | FK | Thuộc bài học nào (`B6_002`, `B9_002`...). Dùng để lọc câu hỏi theo bài. |
| 3 | `MucDo` | Enum | Có | | Mức độ tư duy: `Nhận biết`, `Thông hiểu`, `Vận dụng`, `Vận dụng cao`. |
| 4 | `NoiDung` | LongText | Có | | Nội dung câu hỏi khoa học rõ ràng. |
| 5 | `HinhAnh` | Image/URL | Không | | Hình ảnh minh họa (nếu có). |
| 6 | `DapAnA` | Text | Có | | Nội dung phương án A. |
| 7 | `DapAnB` | Text | Có | | Nội dung phương án B. |
| 8 | `DapAnC` | Text | Có | | Nội dung phương án C. |
| 9 | `DapAnD` | Text | Có | | Nội dung phương án D. |
| 10 | `DapAnDung` | Enum | Có | | Đáp án chính xác (`A`, `B`, `C`, hoặc `D`). |
| 11 | `GiaiThich` | LongText | Có | | Lời giải thích khoa học hiển thị khi trả lời đúng/sai/hết giờ. |

#### Dữ liệu mẫu thực tế trong Sheet:
| CauHoiID | BaiID | MucDo | NoiDung | HinhAnh | DapAnA | DapAnB | DapAnC | DapAnD | DapAnDung | GiaiThich |
| :--- | :--- | :--- | :--- | :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| `C6_003` | `B6_002` | Nhận biết | Đơn vị cấu tạo cơ bản của cơ thể sống là gì? | | Mô | Tế bào | Cơ quan | Hệ cơ quan | **B** | Tế bào là đơn vị cấu tạo và chức năng cơ bản của cơ thể sống. |
| `C9_003` | `B9_002` | Nhận biết | Đơn vị mang thông tin di truyền trong tế bào là gì? | | Gen | Mô | Cơ quan | Enzyme | **A** | Gen là một đoạn phân tử ADN mang thông tin di truyền mã hóa cho chuỗi polypeptide hoặc ARN. |
| `C8_SH01`| `B8_001` | Thông hiểu | Ngăn tim nào có thành cơ dày nhất để tống máu vào vòng tuần hoàn lớn? | | Tâm nhĩ phải | Tâm nhĩ trái | Tâm thất phải | Tâm thất trái | **D** | Tâm thất trái co bóp tạo áp lực tống máu đi nuôi toàn cơ thể nên thành cơ dày nhất. |
| `C8_SH02`| `B8_001` | Nhận biết | Loại tế bào máu nào chịu trách nhiệm vận chuyển oxy trong cơ thể? | | Hồng cầu | Bạch cầu | Tiểu cầu | Huyết tương | **A** | Hồng cầu chứa hemoglobin kết hợp thuận nghịch với O2 để vận chuyển oxy. |

---

### 3.5. Sheet: `NganHangTuLuan` (Ngân hàng Câu hỏi Tự Luận - AI Chấm)

Tách biệt hoàn toàn khỏi câu hỏi trắc nghiệm, dùng cho chế độ Tự Luận (`Image_tuluan`):

| Cột | Kiểu | Bắt buộc | Khóa | Mô tả & Chức năng |
| :--- | :--- | :---: | :---: | :--- |
| `CauHoiTL_ID` | Text | Có | PK | Mã định danh câu tự luận (VD: `TL6_001`, `TL8_001`, `TL9_001`). |
| `BaiID` | Text | Có | FK | Thuộc bài học (ref `BaiHoc.BaiID`). |
| `MucDo` | Enum | Có | | `Nhận biết`, `Thông hiểu`, `Vận dụng`. |
| `NoiDung` | LongText | Có | | Nội dung câu hỏi tự luận mở. |
| `DapAnMau` | LongText | Có | | Đáp án mẫu chuẩn xác của giáo viên. |
| `TuKhoaChamDiem`| Text | Có | | Danh sách từ khóa bắt buộc phân cách dấu `;` để chấm tự động / AI. |
| `ThangDiem` | Number | Có | | Thang điểm tối đa (mặc định: 10.0). |
| `GiaiThichAI` | LongText | Có | | Nhận xét chi tiết hoặc định hướng tư duy học sinh. |

---

### 3.6. Sheet: `AI_GiaiThich` (Nhật ký AI Chấm & Hướng dẫn)
| Cột | Kiểu | Khóa | Dữ liệu mẫu | Chức năng |
| :--- | :--- | :---: | :--- | :--- |
| `AIGiaiThichID` | Text | PK | `AI0001`, `AI0002` | Mã nhật ký AI |
| `ChiTietID` | Text | FK | `CT004`, `CT025` | Mã chi tiết phiên luyện tập |
| `CauHoiID` | Text | FK | `C7_002`, `TL8_001` | Ref `CauHoiID` hoặc `CauHoiTL_ID` |
| `UserID` | Text | FK | `HS002`, `HS003` | Học sinh làm bài |
| `GiaiThichAI` | LongText | | *Bài làm xác định đúng công thức...* | Nội dung nhận xét chi tiết của AI |
| `LoaiAI` | Enum | | `CHAM_TU_LUAN`, `HUONG_DAN_TUNG_BUOC` | Phân loại tác vụ AI |
| `DiemAI` | Number | | `8.0`, `10.0` | Điểm số AI chấm |
| `NhanXetNgan` | Text | | *Đúng công thức, đúng đơn vị.* | Tóm tắt nhanh |
| `TrangThaiAI` | Text | | `HOAN_THANH`, `CAN_HUONG_DAN` | Trạng thái xử lý |

---

## 4. Đặc tả Quy chuẩn Tích hợp vào Game Unity & Cơ Chế Chọn Hình Thức Chơi

### 4.1. Kiến trúc Scene và Luồng Điều Hướng (Game Flow):
1. **Màn hình chọn Chương ([ChapterList 1.unity](file:///d:/FU-learning/FALL-2026_ky7/PRU/PROJECT/PRU-Group8/Assets/Scenes/Sinh/ChapterList%201.unity)):**
   * Quản lý bởi [BiologyMenuManager.cs](file:///d:/FU-learning/FALL-2026_ky7/PRU/PROJECT/PRU-Group8/Assets/Scripts/UI/BiologyMenuManager.cs).
   * Lấy danh mục chương từ sheet `ChuDeKHTN` (lọc `Loai = 'Sinh học'`):
     * Chương I: `CD9_SH_01` (*Di truyền và biến dị*)
     * Chương II: `CD7_SH_01` (*Trao đổi chất và chuyển hóa năng lượng*)
     * Chương III: `CD8_SH_01` (*Cơ thể người và sức khỏe*)
     * Chương IV: `CD6_SH_01` (*Tế bào và cơ thể sống*)
   * Khi người chơi chọn 1 chương (VD: Chương I), lưu `Sinh_SelectedChapter` vào `PlayerPrefs` và chuyển sang `LessonList 1.unity`.

2. **Màn hình chọn Bài ([LessonList 1.unity](file:///d:/FU-learning/FALL-2026_ky7/PRU/PROJECT/PRU-Group8/Assets/Scenes/Sinh/LessonList%201.unity)):**
   * Hiển thị danh mục các bài học thuộc chương đã chọn: `btn_bai2`, `btn_bai3`, `btn_bai4`, `btn_ontapchuong`...
   * **Cơ chế chọn hình thức làm bài:** Khi bấm vào bất kỳ bài học nào, hệ thống không vào màn chơi ngay mà kích hoạt Modal Popup `Popup_ChonCheDo` để người chơi lựa chọn hình thức: **Trắc nghiệm** hoặc **Tự luận**.

---

### 4.2. Quy trình & Giao diện Chọn Hình Thức Luyện Tập (Trắc Nghiệm or Tự Luận)

#### Cấu trúc Popup Lựa chọn (`Popup_ChonCheDo` trong [LessonList 1.unity](file:///d:/FU-learning/FALL-2026_ky7/PRU/PROJECT/PRU-Group8/Assets/Scenes/Sinh/LessonList%201.unity)):
* **Root Popup (`Popup_ChonCheDo`):** Mặc định ẩn (`SetActive(false)`) khi màn hình `LessonList 1` vừa tải. Chỉ xuất hiện khi người chơi nhấp chọn một bài học cụ thể.
* **Khung nền (`Image_bg`):** Bảng khung gỗ cổ điển đóng vai trò làm khung chứa cho giao diện lựa chọn.
* **Khối nội dung chọn (`Image_chon`):**
  1. **Tiêu đề (`Text (TMP)`):** *"CHỌN HÌNH THỨC LUYỆN TẬP"*
  2. **Nút Trắc Nghiệm (`Image_tracnghiem`):**
     * Label: **TRẮC NGHIỆM** (Font chữ trắng, đậm, căn giữa).
     * Action: Đặt `Sinh_QuizMode = "TracNghiem"`, lưu `PlayerPrefs`, chuyển sang màn chơi `SinhScene.unity`.
     * Nguồn dữ liệu: Nạp từ Sheet **`NganHangCauHoi`** (câu hỏi trắc nghiệm 4 phương án A, B, C, D).
  3. **Nút Tự Luận (`Image_tuluan`):**
     * Label: **TỰ LUẬN** (Font chữ trắng, đậm, căn giữa).
     * Action: Đặt `Sinh_QuizMode = "TuLuan"`, lưu `PlayerPrefs`, chuyển sang màn chơi `SinhScene.unity`.
     * Nguồn dữ liệu: Nạp từ Sheet **`NganHangTuLuan`** (câu hỏi mở có từ khóa chấm điểm và lời giải mẫu).
  4. **Nút Quay lại (`Image_back`):**
     * Label: **QUAY LẠI**
     * Action: Đóng popup (`popupChonCheDo.SetActive(false)`), giữ nguyên màn hình danh sách bài học.

---

### 4.3. Đặc tả Chi tiết Cơ Chế Chơi Theo Từng Hình Thức trong `SinhScene`

| Tiêu chí | Chế độ TRẮC NGHIỆM (`TracNghiem`) | Chế độ TỰ LUẬN (`TuLuan`) |
| :--- | :--- | :--- |
| **Nguồn dữ liệu Sheet** | `NganHangCauHoi` (11 cột) | `NganHangTuLuan` (8 cột) |
| **Giao diện đáp án UI** | **4 nút lựa chọn cố định A, B, C, D** (`nutDapAn[0..3]`) bật hiển thị; `answerInputField` hiển thị ký tự đáp án được chọn. | **Ẩn toàn bộ 4 nút A, B, C, D**; mở rộng ô nhập liệu `answerInputField` cho phép học sinh tự gõ câu trả lời dạng văn bản. |
| **Placeholder hướng dẫn**| *"Chọn đáp án A, B, C hoặc D..."* | *"Nhập câu trả lời tự luận tại đây rồi bấm GỬI..."* |
| **Thời gian đếm ngược** | **15 giây / câu** (`timeLimit = 15s`). | **30 giây / câu** (`timeLimit = 30s` để đủ thời gian gõ nội dung). |
| **Quy tắc chấm điểm** | So khớp chính xác ký tự người chơi chọn (`A`, `B`, `C`, `D`) với cột `DapAnDung`. | So khớp từ khóa với `TuKhoaChamDiem` (phân tách dấu `;`) hoặc kiểm tra độ tương đồng với `DapAnMau`. |
| **Cơ chế Hồi / Trừ Máu** | • Đúng: **Hồi 20% máu** (`CongMau(20)`).<br>• Sai hoặc Hết 15s: **Trừ 10 máu** (`TruMau(10)`). | • Đạt từ khóa: **Hồi 25% máu** (`CongMau(25)`).<br>• Sai hoặc Hết 30s: **Trừ 10 máu** (`TruMau(10)`). |
| **Phản hồi sau khi trả lời**| Hiển thị giải thích khoa học từ cột `GiaiThich` trong 3.5 giây trước khi chuyển câu. | Hiển thị lời nhận xét / đáp án chuẩn từ `GiaiThichAI` hoặc `DapAnMau`. |
| **Điều kiện Chiến Thắng Boss**| Vượt qua đủ 5 câu hỏi của bài học đã chọn ➔ Boss võ sĩ bị hạ gục, hoàn thành màn chơi Runner Sinh học. | Vượt qua đủ 5 câu hỏi của bài học đã chọn ➔ Boss võ sĩ bị hạ gục, hoàn thành màn chơi Runner Sinh học. |

---

### 4.4. Quy Tắc Đồng Bộ và Phục Hồi Dữ Liệu Dự Phòng (Fail-Safe Architecture)
1. **Đồng bộ Online & Offline:**
   * Hệ thống ưu tiên nạp dữ liệu cục bộ từ TextAsset CSV (`question.csv`).
   * Nếu cấu hình biến `googleSheetCsvUrl`, `QuizManager` sẽ tự động tải phiên bản mới nhất từ link xuất bản Google Sheet qua `UnityWebRequest`.
2. **Cơ chế Dự phòng (Fallback):**
   * Nếu file CSV đầu vào chỉ có câu hỏi trắc nghiệm mà người chơi chọn chế độ Tự Luận, hàm `EnsureTuLuanQuestionBank()` trong [QuizManager.cs](file:///d:/FU-learning/FALL-2026_ky7/PRU/PROJECT/PRU-Group8/Assets/Scripts/QuizManager.cs) sẽ tự động nạp ngân hàng câu hỏi tự luận mẫu chuẩn KHTN Sinh học (về tế bào, ty thể, quang hợp, tim người, gen và đột biến).
   * Đảm bảo người chơi luôn trải nghiệm mượt mà cả 2 hình thức: **Trắc nghiệm** và **Tự luận** mà không bao giờ gặp lỗi màn hình trống hay đứng game.