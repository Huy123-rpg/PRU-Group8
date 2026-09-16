# SCIENCE QUEST – KHTN 8 🧪🔬🧬

![Unity Version](https://img.shields.io/badge/Unity-2022.3.20f1%20LTS-blue.svg)
![Target Platform](https://img.shields.io/badge/Platform-PC%20%2F%20Windows-green.svg)
![Genre](https://img.shields.io/badge/Genre-2D%20Science%20Adventure%20%2B%20Quiz-orange.svg)

## 📌 1. TỔNG QUAN DỰ ÁN

**SCIENCE QUEST – KHTN 8** là dự án game giáo dục 2D dành cho học sinh lớp 8, giúp các em học tập và ôn tập môn Khoa học Tự nhiên 8 (Vật lý, Hóa học, Sinh học) thông qua lối chơi nhập vai phiêu lưu kết hợp Mini-games, Giải đố (Puzzle) và Trắc nghiệm kiến thức (Quiz).

### 🔄 Vòng lặp Gameplay (Game Loop):
`Explore (Khám phá)` ➔ `Learn (Học tập)` ➔ `Solve (Giải đố)` ➔ `Mini-game` ➔ `Quiz (Trắc nghiệm)` ➔ `Reward (Phần thưởng)` ➔ `Unlock (Mở khóa khu vực)`

### 🗺️ Các Khu vực chính trong Game:
- 🏛️ **Academy (Học viện trung tâm)**: Nơi tiếp nhận nhiệm vụ và hướng dẫn ban đầu.
- ⚡ **Physics (Khu vực Vật lý)**: Các câu hỏi và bài tập thực hành về Vật lý lớp 8.
- 🧪 **Chemistry (Khu vực Hóa học)**: Thí nghiệm và kiến thức Hóa học 8.
- 🌿 **Biology (Khu vực Sinh học)**: Khám phá hệ sinh thái và cơ thể người.

---

## 👥 2. PHÂN CÔNG NHÂN SỰ TEAM 5 SINH VIÊN

| Vai trò | Thành viên | Trách nhiệm chính |
| :--- | :--- | :--- |
| **M1** | **Core Programmer / Team Lead** | Unity Architecture, `GameManager`, `SceneLoader`, Save/Load, Integration, Review Code Git. |
| **M2** | **Gameplay Programmer** | `PlayerController`, `InteractionController`, `IInteractable`, Quiz Logic, Mini-games. |
| **M3** | **UI/UX Designer & Dev** | Main Menu UI, HUD (Coins/Level/EXP), Dialogue UI, Quiz UI, Map UI, Settings. |
| **M4** | **Education & Game Designer** | Game Design Document (GDD), Ngân hàng câu hỏi KHTN 8, Puzzle & Mission design. |
| **M5** | **Level / Asset / QA** | Thiết kế Màn chơi (Level design), Asset Integration, Tilemap, Animation, SFX/BGM, QA Testing. |

---

## ⚙️ 3. YÊU CẦU NỀN TẢNG & CÔNG NGHỆ

- **Unity Editor**: `2022.3.20f1 LTS` (hoặc Unity 2022.3+ LTS).
- **Target Platform**: Standalone PC (Windows).
- **Phân giải mục tiêu**: 1920x1080 (16:9).
- **Vật lý**: Physics2D standard (`Rigidbody2D`, `BoxCollider2D`).

---

## 📁 4. CẤU TRÚC DỰ ÁN (PROJECT STRUCTURE)

```
Assets/
├── Scenes/           # Các màn chơi (MainMenu, Academy, Physics, Chemistry, Biology)
│   ├── MainMenu/
│   ├── Academy/
│   ├── Physics/
│   ├── Chemistry/
│   └── Biology/
│
├── Scripts/          # Toàn bộ mã nguồn C# của dự án
│   ├── Core/         # GameManager, SceneLoader (M1)
│   ├── Player/       # PlayerController (M2)
│   ├── Interaction/  # IInteractable, InteractionController (M2)
│   ├── Quiz/         # Quiz System logic (M2/M4)
│   ├── MiniGames/    # Mini-game mechanics (M2)
│   ├── Mission/      # Task & Mission tracking (M4)
│   ├── Progression/  # Level & Reward progression (M1)
│   ├── UI/           # UIManager & Canvas scripts (M3)
│   ├── Data/         # GameData structures (M1)
│   └── Utilities/    # Helper functions & extension methods
│
├── Prefabs/          # Các vật thể mẫu có thể tái sử dụng
│   ├── Player/
│   ├── NPC/
│   ├── Environment/
│   ├── Interactables/
│   ├── UI/
│   └── MiniGames/
│
├── Art/              # Tài nguyên hình ảnh, sprite
│   ├── Characters/
│   ├── Environment/
│   ├── Objects/
│   └── Icons/
│
├── Audio/            # Âm thanh game
│   ├── BGM/
│   └── SFX/
│
├── UI/               # Font chữ, Sprite giao diện, Material UI
│   ├── Fonts/
│   ├── Sprites/
│   └── Materials/
│
├── Resources/        # Assets nạp động runtime
└── Documentation/    # Hướng dẫn kỹ thuật & Team Setup Guide
```

---

## 🌿 5. CHIẾN LƯỢC QUẢN LÝ NHÁNH GIT (GIT BRANCH STRATEGY)

Dự án áp dụng mô hình Git Flow rút gọn phù hợp với nhóm sinh viên:

- **`main`**: Nhánh chứa phiên bản game chạy ổn định nhất đã qua test và duyệt bài. **KHÔNG COMMIT TRỰC TIẾP**.
- **`develop`**: Nhánh tích hợp code chính của team. Tất cả tính năng mới sẽ được hợp nhất vào đây sau khi review.
- **`feature/*`**: Các nhánh làm việc cá nhân cho từng tính năng.

### 🏷️ Quy tắc đặt tên nhánh (Branch Naming):
- **Theo phân công**: `feature/m1-core`, `feature/m2-gameplay`, `feature/m3-ui`, `feature/m4-content`, `feature/m5-level`
- **Theo tính năng cụ thể**: `feature/player-movement`, `feature/quiz-system`, `feature/main-menu`, `feature/academy-level`

---

## 📝 6. QUY CHUẨN COMMIT MESSAGE (COMMIT CONVENTION)

Tất cả commit message phải tuân theo cấu trúc:
`type: môt tả ngắn gọn công việc bằng tiếng Việt hoặc tiếng Anh`

### Các loại `type` cho phép:
- `feat:` Thêm một tính năng mới (Ví dụ: `feat: add player 2d movement controller`)
- `fix:` Sửa một lỗi/bug trong code (Ví dụ: `fix: resolve scene transition null reference error`)
- `refactor:` Tối ưu hóa code cũ mà không làm thay đổi tính năng
- `docs:` Thêm hoặc sửa đổi tài liệu (Ví dụ: `docs: update README setup guide`)
- `style:` Chỉnh sửa format code, đặt tên biến (không ảnh hưởng logic)
- `chore:` Thay đổi cấu hình dự án, file `.gitignore`, package

---

## 🚀 7. HƯỚNG DẪN THAO TÁC GIT CƠ BẢN DÀNH CHO THÀNH VIÊN

Chi tiết từng bước cấu hình máy tính và workflow có tại file: [`Documentation/TEAM_SETUP.md`](file:///c:/Users/littl/OneDrive/Desktop/PROJECT%20PRU/Documentation/TEAM_SETUP.md)

### Cách lấy dự án về máy (Clone):
```bash
git clone <URL_REPOSITORY_GIHUB>
cd "PROJECT PRU"
```

### Cách tạo nhánh mới để làm việc:
```bash
git checkout develop
git pull origin develop
git checkout -b feature/m2-gameplay
```

### Cách Commit & Push code:
```bash
git status
git add Assets/ ProjectSettings/
git commit -m "feat: add interaction controller script"
git push -u origin feature/m2-gameplay
```

### Cách hợp nhất (Merge) vào `develop`:
Vào GitHub -> Tạo **Pull Request (PR)** từ `feature/m2-gameplay` vào `develop` -> Gửi cho Team Lead (M1) review và duyệt merge.

---

## ✅ 8. DEFINITION OF DONE (WEEK 1 CHECKLIST)

Project chỉ được xem là hoàn thành Sprint Week 1 khi đạt đủ các tiêu chí:

- [x] Unity project mở thành công trên Unity Hub (Không báo lỗi Version/Package).
- [x] Git repository hoạt động ổn định.
- [x] `.gitignore` hoạt động chuẩn (Không push rác `Library/`, `Temp/`, `Build/`).
- [x] Nhánh `main` và `develop` đã được khởi tạo.
- [x] Cấu hình cây thư mục chuẩn (`Assets/Scenes`, `Assets/Scripts`, `Assets/Prefabs`, v.v.).
- [x] Scene `MainMenu` tồn tại và nạp thành công.
- [x] Scene `Academy` tồn tại và nạp thành công.
- [x] Scene `Physics` tồn tại và nạp thành công.
- [x] Scene `Chemistry` tồn tại và nạp thành công.
- [x] Scene `Biology` tồn tại và nạp thành công.
- [x] Hệ thống chuyển đổi Scene (`SceneLoader.cs`) hoạt động qua phím hoặc nút UI.
- [x] Prototype nhân vật (`PlayerController.cs`) di chuyển 4 hướng WASD / Arrow Keys mượt mà.
- [x] Hệ thống va chạm 2D (`Rigidbody2D` + `Collider2D`) hoạt động, ngăn nhân vật đi xuyên tường.
- [x] Prototype tương tác (`IInteractable.cs` + `InteractionController.cs`) hoạt động khi ấn phím `E`.
- [x] Skeleton `GameManager.cs` hoạt động (Singleton quản lý Level, EXP, Coins, CurrentScene).
- [x] Tài liệu `README.md` hoàn thiện.
- [x] Tài liệu `Documentation/TEAM_SETUP.md` hoàn thiện.
- [x] Không có lỗi đỏ (Console Error) nghiêm trọng nào xuất hiện trong Unity Editor.

---

## 🛠️ 9. CÁC LỖI THƯỜNG GẶP VÀ CÁCH XỬ LÝ (FAQ)

1. **Unity báo missing script khi mở project**:
   - Tắt Unity -> Xóa thư mục `Library/` -> Mở lại Unity Hub để Rebuild Library cache.
2. **Xung đột file Scene (.unity conflict)**:
   - Quy tắc: Không nên cho 2 thành viên sửa cùng 1 file Scene cùng lúc. Hãy tách công việc thành các **Prefabs** và kéo thả vào Scene.
3. **Lỡ add nhầm thư mục `Library/` vào Git**:
   - Chạy lệnh gỡ khỏi cache Git: `git rm -r --cached Library/` sau đó commit lại `.gitignore`.

---
*Dự án được xây dựng bởi Team 5 sinh viên - Học phần Đồ án Game / Dự án Phần mềm.*
