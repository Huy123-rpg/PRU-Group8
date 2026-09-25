# 📖 Tài Liệu Cấu Trúc Assets & Scripts - PRU Game Project

Tài liệu này tổng hợp toàn bộ các Scripts, Assets và hệ thống tính năng đã được xây dựng và kết nối trong dự án **PRU (Game Trắc Nghiệm Môn Học)**.

---

## 📁 1. Thư Mục & Cấu Trúc Dự Án (Project Structure)

```text
Assets/
├── Scenes/
│   ├── Login.unity           # Scene Đăng nhập độc lập
│   ├── MenuGame.unity        # Scene Chọn Môn Học độc lập
│   └── SampleScene.unity     # Scene tổng (chứa cả LoginPanel & MenuGame Panel)
├── Scripts/
│   ├── LoginManager.cs       # Quản lý Đăng Nhập & Chuyển Màn
│   ├── MainMenuManager.cs    # Quản lý Menu Game & Chọn Môn
│   └── SubjectCard.cs        # Xử lý hiệu ứng & click cho các thẻ môn học
└── TextMesh Pro/             # Tài nguyên Font & UI TextMeshPro
```

---

## 📜 2. Bảng Mô Tả Các Scripts Đã Xây Dựng

### 1️⃣ `LoginManager.cs` ([Assets/Scripts/LoginManager.cs](file:///d:/GameProject/PRU/Assets/Scripts/LoginManager.cs))
Quản lý toàn bộ luồng đăng nhập của người dùng, xác thực tài khoản offline/online và điều hướng chuyển màn.

* **Tính năng chính**:
  * **Xác thực offline/online**: Kiểm tra username/password mặc định hoặc tải đồng bộ tự động từ **Google Sheet CSV**.
  * **Hỗ trợ Input System**: Tự động chuyển focus giữa ô Username và Password bằng phím `TAB`, ấn `ENTER` để đăng nhập.
  * **Tự động tìm kiếm UI (`AutoAssignUIReferences`)**: Tự động kết nối với `UserName`, `Password`, `LoginButton`, `StatusTex` nếu quên kéo thả.
  * **Hỗ trợ 2 chế độ chuyển màn (`TransitionToMenu`)**:
    1. **Panel Switching** (`usePanelSwitching = true`): Animate / Bật tắt UI `loginPanel` và `menuGamePanel` ngay trong cùng 1 Scene.
    2. **Scene Loading** (`usePanelSwitching = false`): Tải Scene mới thông qua `SceneManager.LoadScene()`.

* **Các tham chiếu trong Inspector**:
  * `usernameInput` (TMP_InputField)
  * `passwordInput` (TMP_InputField)
  * `statusText` (TextMeshProUGUI)
  * `loginButton` (Button)
  * `googleSheetCsvUrl` (string - Đường dẫn CSV Google Sheet)
  * `usePanelSwitching` (bool)
  * `loginPanel` (GameObject)
  * `menuGamePanel` (GameObject)

---

### 2️⃣ `MainMenuManager.cs` ([Assets/Scripts/MainMenuManager.cs](file:///d:/GameProject/PRU/Assets/Scripts/MainMenuManager.cs))
Quản lý trạng thái màn hình chính (Menu Game) và xử lý sự kiện khi chọn từng môn học.

* **Tính năng chính**:
  * **Pattern Singleton (`MainMenuManager.Instance`)**: Dễ dàng truy cập từ bất kỳ script nào.
  * **`SelectSubject(string subjectName)`**: Xử lý logic khi người chơi click chọn môn học (Sinh Học, Hóa Học, Vật Lý).
  * **`OnBackToLoginClicked()`**: Đăng xuất / Quay trở lại màn hình Đăng Nhập.

* **Các tham chiếu trong Inspector**:
  * `selectedSubjectText` (TextMeshProUGUI - Hiển thị tên môn đã chọn)

---

### 3️⃣ `SubjectCard.cs` ([Assets/Scripts/SubjectCard.cs](file:///d:/GameProject/PRU/Assets/Scripts/SubjectCard.cs))
Gán vào từng thẻ môn học (Sinh Học, Hóa Học, Vật Lý) để tạo hiệu ứng tương tác trực quan.

* **Tính năng chính**:
  * **Event Interfaces (`IPointerEnterHandler`, `IPointerExitHandler`, `IPointerClickHandler`)**:
    * **Phóng to nhẹ (Hover Scale)** khi rê chuột vào thẻ (mặc định x1.05 lần).
    * **Đổi Sprite (`selectedSprite`)** khi hover để tạo hiệu ứng nổi bật.
    * **Click (`OnPointerClick`)**: Thông báo tới `MainMenuManager` môn học được chọn.

* **Các tham chiếu trong Inspector**:
  * `subjectName` (string: "Sinh Học", "Hóa Học", "Vật Lý")
  * `cardImage` (Image)
  * `normalSprite` (Sprite trạng thái bình thường)
  * `selectedSprite` (Sprite trạng thái hover/chọn)
  * `hoverScale` (float: 1.05)

---

## ⚙️ 3. Quy Trình Cấu Hình & Liên Kết Trực Quan Trong Unity

1. **Gán Script LoginManager**:
   * Tạo GameObject trống tên `LoginManager`.
   * Gán `LoginManager.cs` vào GameObject này.
   * Gán sự kiện `OnClick()` của nút Login tới `LoginManager.OnLoginButtonClicked()`.

2. **Gán Script MainMenuManager**:
   * Gán `MainMenuManager.cs` vào GameObject `MenuGame`.

3. **Cấu hình các thẻ môn học**:
   * Gán `SubjectCard.cs` vào `Bio Card`, `ChemCard`, `PhyCard`.
   * Điền tên môn tương ứng và kéo Sprite tương ứng vào.

---

## 🌐 4. Quản Lý Phiên Bản Git (Version Control)

* **Repository**: [https://github.com/Huy123-rpg/PRU-Group8](https://github.com/Huy123-rpg/PRU-Group8)
* **Branch chính**: `main`
* **Cấu hình `.gitignore`**: Đã loại bỏ hoàn toàn rác build (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`) để tối ưu hóa dung lượng repository.
