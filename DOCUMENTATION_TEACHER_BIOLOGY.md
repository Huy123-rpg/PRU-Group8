# 🧑‍🏫 TRUNG TÂM GIẢNG VIÊN & GHI ĐIỂM EXCEL — MÔN SINH HỌC
> **Phân hệ:** Biology (Sinh học) · **Nhánh:** `basck`
> **Bổ sung cho:** `DOCUMENTATION_BIOLOGY.md`

---

## 📌 1. Tổng quan

Bổ sung vào phân hệ Sinh học 2 chức năng mới:

| # | Chức năng | Mô tả |
|:--|:---|:---|
| 1 | **Trung tâm giảng viên** | Giảng viên tự tạo bài học + câu hỏi (Trắc nghiệm A/B/C/D hoặc Tự luận chấm theo từ khóa). Học sinh thấy các bài này ngay trong menu môn Sinh. |
| 2 | **Ghi điểm ra bảng tính** | Khi học sinh chơi xong, kết quả được ghi ra file `.xlsx` thật (mở được bằng Excel / Google Sheets / LibreOffice) và đồng bộ lên Google Sheet nếu giảng viên cấu hình link webhook. |

---

## 📂 2. File mới & file được sửa

### 2.1. File mới (`Assets/Scripts/Biology/`)

| File | Chức năng |
|:---|:---|
| `TeacherModels.cs` | Model dữ liệu: `TeacherLesson` (bài học), `TeacherQuestion` (câu hỏi), `TeacherData` (gói dữ liệu). |
| `TeacherStore.cs` | Đọc/ghi dữ liệu giảng viên (JSON), CRUD bài/câu hỏi, xuất CSV. |
| `TeacherManager.cs` | Toàn bộ giao diện Trung tâm giảng viên (dựng 100% bằng code). |
| `ScoreExcelWriter.cs` | Ghi file `.xlsx` thật (tự dựng cấu trúc OOXML, **không cần thư viện ngoài**). |
| `ScoreRecorder.cs` | Ghi kết quả lượt chơi vào bảng tính + đồng bộ Google Sheet. |
| `TeacherEntryPoint.cs` | Tự thêm nút "👨‍🏫 GIẢNG VIÊN" vào các scene menu (MainMenu, MenuGame, ChapterList 1, LessonList 1). |

### 2.2. File mới khác

| File | Chức năng |
|:---|:---|
| `Assets/Scenes/Sinh/TeacherCenter.unity` | Scene Trung tâm giảng viên (Camera + EventSystem + `TeacherManager`). |
| `Assets/Editor/Biology/TeacherCenterSceneBuilder.cs` | Menu `PRU/Sinh học/Tạo scene Trung tâm giảng viên` để tạo lại scene bằng Editor. |

### 2.3. File được sửa

| File | Thay đổi |
|:---|:---|
| `Assets/Scripts/LoginManager.cs` | Thêm role giảng viên (`gv`), chuyển tới `TeacherCenter` khi đăng nhập bằng tài khoản đó, thêm nút vào nhanh trên màn login. |
| `Assets/Scripts/QuizManager.cs` (môn Sinh) | Nạp câu hỏi của giảng viên vào ngân hàng, đếm đúng/sai, **ghi kết quả ra bảng tính** khi chơi xong. |
| `Assets/Scripts/UI/BiologyMenuManager.cs` | Tự thêm nút bài của giảng viên vào danh sách bài của `LessonList 1`; thêm nút "👨‍🏫 GIẢNG VIÊN" ở `ChapterList 1`. |
| `ProjectSettings/EditorBuildSettings.asset` | Đăng ký scene `TeacherCenter` vào Build Settings. |

---

## 🚀 3. Hướng dẫn sử dụng

### 3.1. Phân quyền Người chơi (Player) & Giảng viên (Teacher)

1. **Đăng nhập (`LoginManager`):**
   - Học sinh (`student`, `pru`, `user`, ...) hoặc Giảng viên (`gv` / `123456`) đăng nhập vào như ban đầu.
   - Hệ thống lưu lại Role và **vào màn hình chọn Game (`MenuGame`) như ban đầu**, không chuyển thẳng vào TeacherCenter hay chèn nút đè lên màn login.

2. **Chọn môn Sinh học (`MainMenuManager`):**
   - Người dùng click vào thẻ **Sinh học** (Bio Card):
     + **Nếu là Học sinh (Player):** Hệ thống nạp scene `ChapterList 1` $\rightarrow$ Chơi như bình thường (chọn chương, bài, né bẫy, đấu Boss).
     + **Nếu là Giảng viên:** Hệ thống check role và nạp scene `TeacherCenter` $\rightarrow$ Mở màn hình xem thống kê điểm số và thêm/quản lý câu hỏi.
     + Tại `TeacherCenter`, giảng viên có nút **🎮 CHƠI THỬ** để vào thẳng `ChapterList 1` trải nghiệm màn chơi của học sinh bất cứ lúc nào.

### 3.1b. Thoát khỏi Trung tâm giảng viên

Thanh điều hướng nằm **một hàng riêng** dưới tiêu đề (trải đều 4 nút, không bị scene khác che):

| Nút | Chức năng |
|:---|:---|
| CHƠI THỬ | Vào màn chọn chương môn Sinh học (`ChapterList 1`) để thử bài vừa tạo |
| KẾT QUẢ | Xem bảng điểm học sinh (tab thay cho form thêm câu hỏi) |
| XUẤT CÂU HỎI | Xuất toàn bộ câu hỏi giảng viên ra CSV |
| **← CHỌN MÔN** | **Quay về màn chọn môn — KHÔNG phải logout, KHÔNG thoát game** |

Nút **"← CHỌN MÔN"** gọi `TeacherNavigation.GoToSubjectSelect()`:
* Nạp scene `SampleScene` với cờ `LoginManager.SkipLoginOnStart = true`
* Ẩn panel đăng nhập, hiện panel `MenuGame` (các thẻ môn).

Trên `ChapterList 1` / `LessonList 1`, giảng viên còn 2 nút overlay góc phải:
* **← TRUNG TÂM GV** → về `TeacherCenter`
* **← CHỌN MÔN** → về màn chọn môn

> Nếu muốn thoát hẳn ra màn đăng nhập (logout), hãy dùng nút quay lại của màn chọn môn
> (`MainMenuManager.OnBackToLoginClicked`).

### 3.2. Tạo bài học

1. Bấm **➕ TẠO BÀI MỚI** (góc trên cột trái).
2. Nhập **tiêu đề bài** (VD: *"Bài 1: Menđen và Di truyền học"*).
3. Chọn **chủ đề** (1 trong 5 chủ đề Sinh học).
4. Chọn **hình thức mặc định** (Trắc nghiệm / Tự luận).
5. Bấm **💾 TẠO BÀI**.

### 3.3. Thêm câu hỏi

1. Chọn bài ở cột trái.
2. Ở cột phải, chọn **TRẮC NGHIỆM** hoặc **TỰ LUẬN**.
3. Điền nội dung + các trường tương ứng:

| Hình thức | Trường bắt buộc | Trường tuỳ chọn |
|:---|:---|:---|
| **Trắc nghiệm** | Nội dung, 4 phương án A/B/C/D, chọn đáp án đúng | Giải thích, độ khó, thời gian (giây), cộng máu (%) |
| **Tự luận** | Nội dung, **đáp án mẫu** hoặc **từ khóa** | Giải thích, độ khó, thời gian, cộng máu |

4. Bấm **💾 LƯU CÂU HỎI**.

> **Tự luận được chấm thế nào?** Hệ thống chấm đúng nếu câu trả lời chứa chuỗi đáp án mẫu, **hoặc** chứa ít nhất 1 từ khóa (các từ khóa cách nhau bằng dấu `;`).

### 3.4. Học sinh chơi bài của giảng viên

1. Vào môn Sinh học → chọn chủ đề → **danh sách bài sẽ có thêm các bài của giảng viên** ở cuối (đánh dấu bằng tên bài do GV đặt).
2. Bấm bài → chọn Trắc nghiệm/Tự luận → vào `SinhScene`.
3. Game chỉ lấy câu hỏi **đúng bài đó** (không trộn với câu hỏi có sẵn).

### 3.5. Xem kết quả & xuất Excel

1. Bấm **📊 KẾT QUẢ** trên thanh tiêu đề.
2. Xem danh sách lượt thi (thời gian, tài khoản, bài, đúng/sai, điểm, kỷ lục, số Boss, tỉ lệ đúng).
3. Bấm **📁 XEM FILE EXCEL** để mở thư mục chứa file kết quả.
4. Bấm **⬇ XUẤT CÂU HỎI** để xuất toàn bộ câu hỏi giảng viên ra CSV (import được vào Google Sheets).

### 3.6. Đồng bộ Google Sheet (tuỳ chọn)

1. Trên Google Sheets: **Extensions → Apps Script**, dán đoạn mã nhận dữ liệu, **Deploy → New deployment → Web app** (`Anyone`).
2. Lấy URL dạng `https://script.google.com/macros/s/XXXX/exec`.
3. Dán vào ô **LINK GOOGLE SHEET** trong Trung tâm → bấm **💾 LƯU**.
4. Từ đó, mỗi lượt thi sẽ tự gửi 1 dòng lên sheet.

<details>
<summary>Apps Script mẫu (tuỳ chọn)</summary>

```javascript
function doPost(e) {
  var d = JSON.parse(e.postData.contents);
  var sheet = SpreadsheetApp.getActive().getActiveSheet();
  sheet.appendRow([
    d.timestamp, d.username, d.chapter, d.lesson, d.mode,
    d.totalQuestions, d.correctCount, d.wrongCount, d.score,
    d.highScore, d.bossesDefeated, d.accuracyPercent, d.details
  ]);
  return ContentService.createTextOutput("OK");
}
```

</details>

---

## 📂 4. Nơi lưu dữ liệu

| Loại | Đường dẫn |
|:---|:---|
| Dữ liệu giảng viên (JSON) | `%AppData%/../LocalLow/<Company>/<Product>/BiologyTeacher/teacher_lessons.json` — tức `Application.persistentDataPath/BiologyTeacher/` |
| Câu hỏi giảng viên (CSV) | `.../BiologyTeacher/teacher_lessons.csv` |
| **Bảng điểm (Excel)** | `.../BiologyTeacher/KetQua_SinhHoc.xlsx` |
| Bảng điểm (CSV kèm theo) | `.../BiologyTeacher/KetQua_SinhHoc.csv` |
| Lịch sử điểm (dự phòng) | `PlayerPrefs` key `Sinh_ScoreRecords` |

> Bảng điểm được ghi lại **toàn bộ lịch sử** mỗi lần có lượt thi mới, nên nếu mất file `.xlsx` thì vào game lần sau sẽ được dựng lại từ `PlayerPrefs`.

### Cấu trúc bảng điểm

| Thời gian | Tài khoản | Chủ đề | Bài học | Hình thức | Tổng số câu | Số câu đúng | Số câu sai | Điểm | Kỷ lục | Số Boss vượt qua | Tỉ lệ đúng (%) | Chi tiết |

Cột số được ghi dạng **cell số** (không phải chữ) để Excel/Google Sheets tính toán và sắp xếp được; hàng tiêu đề được **đóng băng** và có **bộ lọc**.

---

## 🔑 5. PlayerPrefs Keys mới

| Key | Kiểu | Mô tả |
|:---|:---:|:---|
| `Sinh_CurrentUsername` | `string` | Tài khoản người học hiện tại (ghi vào bảng điểm) |
| `Sinh_ScoreRecords` | `string` | Lịch sử kết quả dạng JSON (dự phòng để dựng lại file Excel) |
| `Sinh_ScoreSheetUrl` | `string` | URL webhook Google Sheet do giảng viên cấu hình |

---

## 🧪 6. Checklist test nhanh

1. **Vào Trung tâm:** bấm nút "👨‍🏫 GIẢNG VIÊN" ở `MainMenu` → scene `TeacherCenter` mở ra, thấy cột trái (danh sách bài) + cột phải (form thêm câu hỏi).
2. **Tạo bài:** ➕ TẠO BÀI MỚI → nhập tiêu đề → chọn chủ đề "Chương I" → 💾 TẠO BÀI → bài xuất hiện trong danh sách bên trái.
3. **Thêm câu hỏi trắc nghiệm:** điền nội dung + 4 phương án → chọn đáp án đúng (VD "B") → 💾 LƯU → câu hỏi hiện trong danh sách dưới form.
4. **Thêm câu hỏi tự luận:** bấm "TỰ LUẬN" → điền nội dung + đáp án mẫu hoặc từ khóa → 💼 LƯU.
5. **Học sinh thấy bài:** vào môn Sinh → chủ đề tương ứng → `LessonList 1` có thêm bài của GV ở cuối danh sách.
6. **Chơi & ghi điểm:** vào game, trả lời vài câu, để hết máu → popup Game Over hiện ra → bấm "📊 KẾT QUẢ" ở GV, thấy lượt thi vừa rồi.
7. **File Excel:** bấm "📁 XEM FILE EXCEL" → mở `KetQua_SinhHoc.xlsx` → có dòng vừa chơi, cột số là số thật.
8. **Google Sheet:** dán link webhook vào ô link → 💾 LƯU → chơi 1 lượt → mở sheet thấy dòng mới.

---

## ⚠️ 7. Lưu ý cho Leader khi Merge

* Toàn bộ UI giảng viên được **dựng bằng code**, không sửa file `.unity` hiện có → **không phát sinh merge conflict** với các nhánh khác.
* Scene `TeacherCenter.unity` là file mới, có `.meta` với GUID cố định.
* `Assets/Editor/Biology/TeacherCenterSceneBuilder.cs` chỉ dùng trong Editor, không ảnh hưởng build.
* File `Assembly-CSharp.csproj` được Unity tự sinh lại — nếu IDE báo thiếu file trong `Assets/Scripts/Biology/`, chỉ cần reload project trong Unity.
* Không ảnh hưởng tới môn Vật lý / Hóa học: tất cả lớp mới nằm trong namespace `ScienceQuest.Biology`, riêng biệt với `ScienceQuest.Quiz` (Vật lý) và `QuizManager` (Sinh, global namespace) đã tồn tại.

---

## 📐 8. Quy ước layout (bắt buộc khi sửa UI giảng viên)

### 8.1. Quy tắc vàng: không trộn `sizeDelta` với `offsetMin/offsetMax`

Unity tính kích thước một ô UI như sau:

```
width  = (anchorMax.x - anchorMin.x) * chaWidth  + (offsetMax.x - offsetMin.x)
height = (anchorMax.y - anchorMin.y) * chaHeight + (offsetMax.y - offsetMin.y)
```

Nếu dùng **neo Y bằng nhau** (`0,1 → 0,1`) mà vẫn set `offsetMin.y = offsetMax.y = 0`
thì `height = 0` → **ô đó vô hình và không bấm được** (đây chính là lỗi khiến
form "Tạo bài mới" hiện ra mà trống trơn).

Quy tắc:

| Loại đặt | Khi nào dùng | Cách làm |
|:---|:---|:---|
| Neo cạnh trên, giãn ngang | Hàng / ô nhập / nút xếp dọc từ trên xuống | Dùng `SetTopStretch()` — chỉ `anchor` + `pivot` + `anchoredPosition` + `sizeDelta` |
| Neo 4 cạnh (full stretch) | Panel lớn, vùng nền | Dùng `SetStretchInset()` — `anchorMin=0`, `anchorMax=1` + `offsetMin/offsetMax` |
| Neo điểm | Nút cố định, nút tròn | `anchorMin = anchorMax` + `anchoredPosition` + `sizeDelta` (đủ cả 2 chiều) |

**Không bao giờ** set `offsetMin.y` / `offsetMax.y` cho phần tử có neo Y bằng nhau.

### 8.2. Thứ tự vẽ (z-order)

Unity vẽ theo thứ tự sibling, **sau thì đè trước**. Vì vậy:

* Thanh tiêu đề & thanh trạng thái phải `SetAsLastSibling()` **sau khi** dựng xong nội dung.
* Form câu hỏi phải `SetAsLastSibling()` để nằm trên tab kết quả.
* Nút "Giảng viên" trong các scene khác cũng `SetAsLastSibling()`.

### 8.3. Canvas riêng

Trung tâm giảng viên **luôn tạo Canvas riêng** (`TeacherCanvas`, `sortingOrder = 200`),
không dùng lại Canvas của scene trước — nếu dùng lại sẽ bị panel của scene đó che,
và `CanvasScaler` khác nhau làm bố cục lệch tùy scene vào.

### 8.4. Tự kiểm tra layout

`TeacherManager.LogLayoutWarnings()` chạy mỗi lúc vào Trung tâm, in ra Console:

```
[TeacherManager] ✅ Kiểm tra layout: mọi phần tử UI đều có kích thước hợp lệ.
```

Nếu thấy cảnh báo "có kích thước 0" → còn ô nào đang bị lỗi layout, sẽ không hiển thị.

