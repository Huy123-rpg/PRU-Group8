# 🚀 HƯỚNG DẪN CẤU HÌNH & LÀM VIỆC DÀNH CHO TEAM SINH VIÊN
## PROJECT: SCIENCE QUEST – KHTN 8

Chào mừng bạn tham gia đội ngũ phát triển game **Science Quest – KHTN 8**! 
Tài liệu này được biên soạn ngắn gọn, dễ hiểu nhất để giúp các bạn thành viên (M1 -> M5) nhanh chóng thiết lập môi trường và làm việc nhóm hiệu quả bằng Unity & Git.

---

## 🛠️ BƯỚC 1: CÀI ĐẶT MÔI TRƯỜNG (CHỈ LÀM 1 LẦN)

1. **Cài đặt Unity Hub & Unity Editor**:
   - Tải và cài đặt [Unity Hub](https://unity.com/download).
   - Mở Unity Hub -> Chọn mục **Installs** -> Cài đặt phiên bản **Unity 2022.3.20f1 LTS** (hoặc phiên bản Unity 2022.3+ LTS bất kỳ được chỉ định).
   - Chọn thêm Module: `Documentation` và `Visual Studio Community` (nếu chưa có IDE).

2. **Cài đặt Git**:
   - Tải và cài đặt [Git cho Windows](https://git-scm.com/download/win).
   - (Khuyên dùng) Cài đặt **Git Kraken**, **GitHub Desktop** hoặc **VS Code** để dễ thao tác Git giao diện trực quan.

---

## 📥 BƯỚC 2: CLONE DỰ ÁN VỀ MÁY CÁ NHÂN

1. Mở Terminal (CMD / PowerShell / Git Bash) tại thư mục muốn lưu game trên máy tính.
2. Chạy lệnh clone repository:
   ```bash
   git clone <URL_REPOSITORY_GIHUB_CỦA_TEAM>
   cd "PROJECT PRU"
   ```
3. Kiểm tra các nhánh hiện tại:
   ```bash
   git branch -a
   ```

---

## 🖥️ BƯỚC 3: MỞ PROJECT TRONG UNITY EDITOR

1. Mở **Unity Hub**.
2. Bấm nút **Add** (hoặc *Open*) -> Tìm đến thư mục chứa project `PROJECT PRU`.
3. Chọn phiên bản Unity `2022.3.20f1` và nhấn mở.
4. Lần đầu tiên mở, Unity sẽ mất khoảng 1-3 phút để nạp gói `Packages/`. Vui lòng kiên nhẫn chờ đợi.

---

## 🔀 BƯỚC 4: QUY TRÌNH LÀM VIỆC VỚI GIT BRANCH (QUAN TRỌNG)

> [!CAUTION]
> **QUY TẮC VÀNG:**
> 1. **KHÔNG BAO GIỜ** làm việc trực tiếp trên nhánh `main`.
> 2. Nhánh `main` chỉ chứa bản game chạy ổn định đã qua nghiệm thu.
> 3. Mọi công việc mới phải được thực hiện trên **Feature Branch** tách ra từ `develop`.

```
main  -------------------------------------> [RELEASES]
        \
develop  -----------------------------------> [INTEGRATION TEST]
           \                 /
feature     ---> [FEATURE] --
```

### 1. Chuyển sang nhánh `develop` và cập nhật code mới nhất:
```bash
git checkout develop
git pull origin develop
```

### 2. Tạo nhánh tính năng mới (Feature Branch) cho công việc của bạn:
Tên nhánh đặt theo phân công hoặc task cụ thể:
- `feature/m1-core` (M1 - Core Programmer)
- `feature/m2-gameplay` (M2 - Gameplay)
- `feature/m3-ui` (M3 - UI/UX)
- `feature/m4-content` (M4 - Education Content)
- `feature/m5-level` (M5 - Level & Assets)
- Hoặc theo task: `feature/player-movement`, `feature/quiz-system`

**Lệnh tạo branch:**
```bash
git checkout -b feature/m2-gameplay
```

---

## ✏️ BƯỚC 5: THỰC HIỆN CÔNG VIỆC, COMMIT & PUSH

### 1. Thực hiện công việc trong Unity
- Tạo script, chỉnh sửa UI, thêm Asset, dựng Scene...
- **Test kỹ trên Unity Editor**: Đảm bảo không có dòng đỏ (Console Error) trước khi commit.

### 2. Xem các file đã thay đổi:
```bash
git status
```

### 3. Thêm các thay đổi vào Staging:
```bash
git add Assets/
git add ProjectSettings/
```

### 4. Commit với quy chuẩn rõ ràng (Commit Convention):
Sử dụng các tiền tố chuẩn:
- `feat:` Thêm tính năng mới (Ví dụ: `git commit -m "feat: add player movement script"`)
- `fix:` Sửa lỗi bug (Ví dụ: `git commit -m "fix: prevent player walking through walls"`)
- `docs:` Viết tài liệu (Ví dụ: `git commit -m "docs: update setup guide"`)
- `refactor:` Cải thiện/tối ưu code cũ mà không đổi tính năng.

### 5. Push nhánh của bạn lên GitHub:
```bash
git push -u origin feature/m2-gameplay
```

---

## 🔀 BƯỚC 6: TẠO PULL REQUEST (PR) ĐỂ MERGE VÀO DEVELOP

1. Truy cập vào GitHub Repository của team.
2. Bạn sẽ thấy thông báo vừa push nhánh mới -> Nhấn nút **Compare & Pull Request**.
3. Chọn Base branch là **`develop`** (KHÔNG chọn `main`).
4. Viết mô tả ngắn gọn về những gì bạn đã làm.
5. Gửi PR cho **Team Lead (M1)** review và duyệt merge vào `develop`.

---

## 🛡️ GIT SAFETY - NHỮNG ĐIỀU NÊN LÀM & TUYỆT ĐỐI KHÔNG ĐƯỢC LÀM

| ❌ TUYỆT ĐỐI KHÔNG ĐƯỢC LÀM | ✅ NÊN LÀM |
| :--- | :--- |
| ❌ `git add Library/` hoặc `git add .` khi chưa kiểm tra `.gitignore`. | ✅ Để `.gitignore` tự động bỏ qua thư mục `Library/`, `Temp/`, `Build/`. |
| ❌ Commit thư mục xuất game `Build/` lên Git. | ✅ Chỉ lưu file nguồn (`Assets/`, `ProjectSettings/`, `Packages/`). |
| ❌ Dùng `git push --force` lên `main` hoặc `develop`. | ✅ Luôn tạo Pull Request và nhờ đồng đội review code. |
| ❌ Tự ý sửa code của thành viên khác trên nhánh của họ mà không báo trước. | ✅ Mỗi người làm việc trên feature branch riêng của mình. |
| ❌ Gộp toàn bộ công việc của 1 tuần vào 1 commit duy nhất khổng lồ. | ✅ Commit nhỏ, thường xuyên sau mỗi tính năng hoàn thành. |

---

## 🚨 XỬ LÝ CÁC LỖI THƯỜNG GẶP (TROUBLESHOOTING)

### 1. Unity báo lỗi "Missing Script" (Unassigned Script Component):
- **Nguyên nhân**: File `.meta` bị mất hoặc trùng lặp GUID khi copy file ngoài Unity.
- **Khắc phục**: Khi thêm hay xóa file trong Unity, **luôn thực hiện bên trong giao diện Unity Editor** để Unity tự động tạo và quản lý file `.meta`.

### 2. Xung đột Git Conflict (Merge Conflict):
- **Nguyên nhân**: Hai người cùng sửa 1 file `.cs` hoặc 1 file `.unity` scene.
- **Khắc phục**:
  - Hạn chế 2 người cùng sửa chung 1 file `.unity` scene. Hãy dùng **Prefabs**!
  - Nếu bị conflict file `.cs`: Mở file lên bằng Visual Studio Code, tìm đoạn `<<<<<<< HEAD` và thảo luận với đồng đội để chọn giữ lại code đúng.

### 3. Unity không chạy được sau khi pull code mới:
- **Khắc phục**: Tắt Unity Editor -> Xóa thư mục `Library/` trong folder project -> Mở lại Unity Hub để Unity build lại cache Library từ đầu.

---
**CHÚC TEAM LÀM VIỆC NĂNG SUẤT VÀ TẠO RA GAME SCIENCE QUEST KHTN 8 THẬT XUẤT SẮC! 🎉**
