# 🌿 GAME SINH HỌC — TÀI LIỆU BỐI CẢNH & SPEC CHO AI AGENT

> **Đọc file này trước khi code.** Đây là hướng dẫn để bạn (AI agent) tự hoàn thiện module "Sinh" của game.
> Game dạng **vượt chướng ngại vật (endless runner 2D dọc)** + **trắc nghiệm / tự luận** khi gặp Boss.
> Người chơi đã **đập đi làm lại phần giao diện Unity** — nền, chướng ngại vật, Boss, panel quiz, thanh máu, nút âm thanh. Nhiệm vụ của agent là **viết code hoàn thiện các phần còn thiếu**, KHÔNG cần làm lại giao diện.

---

## 1. BỐI CẢNH GAME (Lore & Mục đích)

- **Thể loại:** Game học tập (edutainment) môn **Sinh học phổ thông**, hình thức endless runner 2D.
- **Thế giới:** Nhân vật (sprite chibi, tên object `nv` trong scene) chạy trên "con đường kiến thức" — một dải đường thẳng dọc, camera tự động đi **lên trục Y** (world chạy xuống phía dưới người chơi). Người chơi chỉ điều khiển **trục X** (né sang trái/phải, phạm vi đường vàng).
- **Vòng lặp gameplay:**
  1. Nền đường (`bg`) cuộn liên tục tạo cảm giác đang chạy.
  2. **Chướng ngại vật** (object `chuongngaivat`) xuất hiện **random** dọc đường. Player chạm vào → bị **trừ máu** (theo % của từng vật thể), vật thể bị phá hủy, player bất tử tạm thời 1 giây.
  3. **Boss** (object `boss`) xuất hiện **tại các mốc quãng đường cố định**, không random. Khi Boss trôi tới **giữa màn hình** → game dừng (`Time.timeScale = 0`) và mở popup quiz.
  4. Quiz hiển thị đúng **panel theo chế độ** người chơi đã chọn trước đó:
     - **Trắc nghiệm** → panel `paneltracno`
     - **Tự luận** → panel `paneltuluan`
  5. Trả lời **đúng** → cộng **điểm elo** và **cộng máu** (thanh máu). Trả lời **sai** → **không cộng điểm**, **trừ máu** (thanh máu).
  6. Hết máu → popup **Game Over** (Chơi lại / Thoát về menu).
  7. Vượt qua đủ số câu của Boss → thắng Boss, hủy Boss, tiếp tục chạy tới mốc Boss tiếp theo (vô tận).
- **Âm thanh:** Game phải có **nhạc nền (BGM)** chạy liên tục trong màn chơi và **hiệu ứng âm thanh (SFX)** khi: chạm chướng ngại vật, Boss xuất hiện, chọn đáp án, trả lời đúng / sai, Game Over.

> Người chơi **chọn chế độ chơi (trắc nghiệm / tự luận) ở menu Sinh học TRƯỚC KHI vào game** (scene `SinhScene`), thông tin lưu trong PlayerPrefs (xem §5).

---

## 2. QUY TẮC GAMEPLAY CHI TIẾT

### 2.1 Nền (background)
- Nền đường phải **cuộn liên tục theo trục Y**, đồng bộ với tốc độ camera (script `AutoScroll`), tạo hiệu ứng người chơi đang chạy trên đường.
- Cuộn phải **vô hạn, không nháy (seamless loop)**, các mảnh nền nối liền mép.

### 2.2 Chướng ngại vật
- Xuất hiện **random** (vị trí X/Y trong đường vàng, số lượng mỗi đoạn map từ 1–3) — đã có logic trong `ObstacleSpawner`.
- Khi player **chạm vào**:
  - Trừ máu theo **phần trăm của từng loại vật thể** (field `phanTramTruMau` trong `ObstacleDamage`, mặc định 15%).
  - Vật thể bị `Destroy` ngay lập tức (phá bẫy, không va chạm lại).
  - Player **bất tử 1 giây** (không mất máu lần chạm kế tiếp) kèm hiệu ứng **chớp đỏ**.
  - Phát **SFX chạm**.

### 2.3 Boss
- Boss xuất hiện **tại các mốc quãng đường cố định** (list `MonsterLevel[]` trong `ObstacleSpawner`: mỗi mốc có `spawnAtY`). Sau mốc cuối, Boss lặp lại theo chu kỳ 40m (endless).
- Khi Boss **đến giữa màn hình** (tọa độ Y của Boss ≈ tọa độ Y của camera) → dừng game, mở quiz. **KHÔNG** dùng timer đếm ngược cố định để đợi Boss tới (cách cũ bằng `OnBecameVisible` + `Invoke 3s` là không ổn định khi tốc độ map thay đổi — xem Task T6).
- Khi dừng ở quiz: `Time.timeScale = 0`. Khi thắng Boss: hủy Boss, `Time.timeScale = 1`, tiếp tục chạy.

### 2.4 Quiz theo chế độ (do người chơi chọn TRƯỚC khi vào game)

| Chế độ | Panel hiển thị | Thời gian / câu | Cách trả lời |
|---|---|---|---|
| **Trắc nghiệm** | `paneltracno` | **30 giây** | Chọn 1 trong các đáp án A/B/C/D |
| **Tự luận** | `paneltuluan` | **KHÔNG GIỚI HẠN thời gian** (tắt đồng hồ ngược) | Gõ text vào khung trả lời **HOẶC upload ảnh** bài viết |

- Hết giờ câu trắc nghiệm = **trả lời sai** (trừ máu, không cộng điểm).
- Số câu hỏi cho mỗi Boss: 5 (field `soCauHoiCanTraLoi` của `QuizManager`).

### 2.5 Chấm bài tự luận bằng AI
- Gọi `AIGradingService` (Google Gemini) với API key lấy từ **PlayerPrefs `"Gemini_ApiKey"`** (KHÔNG hardcode trong code).
- AI trả về: `isCorrect`, `score` (thang 10), `feedback` — **giải đáp ngắn gọn 1–2 câu**, kèm lời giải / đáp án mẫu để học sinh hiểu.
- **Input cho AI:** câu hỏi + đáp án mẫu + từ khóa + **bài làm của học sinh (text hoặc ảnh)**. Nếu học sinh **upload ảnh bài viết**, cần gửi ảnh kèm cho model (xem Task T4).
- Khi **mất mạng / chưa có API key** → dùng **chế độ chấm từ khóa dự phòng** (đã có sẵn trong `AIGradingService.FallbackGrade`) — không được xóa phần này.

### 2.6 Điểm (elo) & Máu (HP)
- **Trả lời ĐÚNG:**
  - **Cộng điểm elo** (điểm tích lũy, hiện lưu trong `QuizManager.currentScore` — mỗi câu đúng +10 điểm).
  - **Cộng máu** theo % (`hpBonus` của câu hỏi hoặc field `phanTramCongMau`, mặc định +20% máu), không vượt max máu (100).
- **Trả lời SAI:**
  - **KHÔNG cộng điểm elo.**
  - **Trừ máu** trên thanh máu (`thanhmau`), mức trừ mặc định 10% (field của `QuizManager.TruMau`).
- **Máu về 0 → Game Over:** hiện popup, lưu điểm kỷ lục + lịch sử xếp hạng (PlayerPrefs), ghi kết quả ra bảng tính cho giáo viên (đã có).

### 2.7 Âm thanh & nhạc nền
- Nhạc nền: 1 bản loop, chạy liên tục trong màn chơi (nút `nhac` / `amthanh` trong UI để bật/tắt).
- SFX theo §1. Quản lý tập trung trong **1 Audio Manager** (xem Task T7) — hiện scene **chưa có bất kỳ AudioSource nào**.

---

## 3. HIỆN TRẠNG CODEBASE (ĐÃ CÓ — KHÔNG VIẾT TRÙNG)

### 3.1 Scripts (thư mục `Assets/Scripts/`)

| File | Vai trò | Trạng thái |
|---|---|---|
| `MapManager.cs` | Sinh chunk map vô tận trục Y (`chunkLength = 20`, `chunksOnScreen = 3`), theo `cameraTransform`; chunk thừa phía sau bị destroy | ✅ Có |
| `AutoScroll.cs` | Đẩy camera lên `Vector3.up * speed * Time.deltaTime` (`speed` chỉnh Inspector) | ✅ Có |
| `ObstacleSpawner.cs` | Mỗi chunk map spawn **1–3 chướng ngại vật random** (khoảng cách tối thiểu 2.5u, chống đè nhau) trong vùng đường vàng `minX..maxX / minY..maxY`; spawn **Boss tại các mốc** `MonsterLevel[]` cố định; sau mốc cuối → lặp Boss mỗi 40m | ✅ Có |
| `PlayerMovement.cs` | A/D hoặc phím mũi tên dịch chuyển trục X, clamp `[minX, maxX]` (New Input System) | ✅ Có |
| `PlayerCollision.cs` | `OnCollisionEnter2D` / `OnTriggerEnter2D` với tag `Obstacle` → destroy vật thể, trừ máu theo `ObstacleDamage`, bất tử 1s + chớp đỏ | ✅ Có |
| `ObstacleDamage.cs` | 1 field `phanTramTruMau` (mặc định 15%) — mỗi loại chướng ngại vật gán script này với % riêng | ✅ Có |
| `MonsterShowUp.cs` | Boss `OnBecameVisible` → Invoke 3s → `QuizManager.ShowQuiz()` (cơ chế cũ, **cần thay** — T6) | ⚠️ Cần sửa |
| `QuizManager.cs` | Nạp câu hỏi (CSV / Google Sheet / câu do GV tạo trong `TeacherStore`), lọc theo chương–bài–chế độ, điều khiển quiz 5 câu/Boss, máu/điểm/elo, popup Game Over, ghi kết quả Excel + Google Sheet | ✅ Có (sửa thêm theo T2, T3) |
| `Biology/AIGradingService.cs` | Chấm tự luận bằng Gemini (`gemini-1.5-flash`), key từ PlayerPrefs `Gemini_ApiKey`, timeout 10s, fallback từ khóa | ✅ Có (rộng mở theo T4, T5) |
| `UI/BiologyMenuManager.cs` | Menu chọn chương → bài → **chế độ** (trắc nghiệm / tự luận), lưu PlayerPrefs rồi mở `SinhScene` | ✅ Có |
| `LoginManager.cs` | Đăng nhập, `LoginManager.CurrentUsername` (dùng để ghi điểm) | ✅ Có |

### 3.2 Đối tượng chính trong scene `SinhScene.unity`

| Tên object | Layer | Loại | Vai trò |
|---|---|---|---|
| `bg` (×2) | 5 (UI) | **UI Image trong Canvas** | Nền hiện tại — **TĨNH, chưa cuộn (Task T1)** |
| `Player`, `nv` | — | Sprite + collider | Nhân vật người chơi |
| `chuongngaivat` (×7) | — | Prefab chướng ngại | Chướng ngại vật, tag `Obstacle`, script `ObstacleDamage` |
| `boss` (×12) | — | Prefab Boss | Boss theo mốc, script `MonsterShowUp` |
| `MapManager` | — | GameObject | Chạy `MapManager` (sinh chunk) |
| `paneltracno` | 5 (UI) | Panel quiz | **Panel trắc nghiệm** |
| `paneltuluan` | 5 (UI) | Panel quiz | **Panel tự luận** |
| `thanhmau`, `thanhmau (1)`, `hoimau` | 5 (UI) | Slider / Icon | Thanh máu, icon hồi máu |
| `dapana / dapanb / dapanc` | 5 (UI) | Nút + Text | Các đáp án A/B/C |
| `nopbai` | 5 (UI) | Nút | Nộp bài |
| `nhac`, `nhac (1)`, `amthanh (1)` | 5 (UI) | **Nút Image (chưa có AudioSource)** | Nút âm thanh — T7 |
| `debai` | 5 (UI) | Panel | Màn kết thúc |
| `panelpause`, `choitiep`, `choilai`, `thoat` | 5 (UI) | Panel / Nút | Pause, chơi tiếp, chơi lại, thoát |
| `time` | 5 (UI) | Text | Đồng hồ ngược (trắc nghiệm) |
| `AI` | — | GameObject | Placeholder cho module AI |
| `title`, `score` | 5 (UI) | Text | Tiêu đề, điểm |

### 3.3 Dữ liệu & khóa PlayerPrefs (KHÔNG ĐƯỢC ĐỔI TÊN)

| Key | Giá trị | Nghĩa |
|---|---|---|
| `Sinh_SelectedChapter` | string | Chương Sinh học đã chọn |
| `Sinh_SelectedLesson` | string | Bài học đã chọn |
| `Sinh_QuizMode` | `TracNghiem` / `TuLuan` | **Chế độ chơi** (quyết định panel + thời gian + cách chấm) |
| `Sinh_HighScore`, `Sinh_LastScore`, `Sinh_ScoreHistory` | int / string | Kỷ lục & lịch sử xếp hạng |
| `Sinh_CurrentUsername` | string | Tên người chơi hiện tại |
| `Gemini_ApiKey` | string | **API key chấm tự luận** (quy định dùng PlayerPrefs, không hardcode) |

Scene: màn chơi = **`"SinhScene"`**, menu Sinh học trước đó = **`"LessonList 1"`**.

---

## 4. NHIỆM VỤ CẦN TỰ HOÀN THIỆN (Worklist)

> Quy tắc chung: **chỉ viết thêm / sửa tối thiểu**, giữ style code + comment tiếng Việt như các file hiện có. Mỗi task có: **Input / Output**, **File cần tạo–sửa**, **Chi tiết**.

### T1. Cuộn nền đường (background)
- **Input:** 2 object `bg` (UI Image trong Canvas, Layer 5) + tốc độ của `AutoScroll`.
- **Output:** hiệu ứng nền **cuộn liên tục theo trục Y**, khớp mép, không nháy, đồng bộ tốc độ camera → cảm giác "đang chạy trên đường".
- **File cần tạo/sửa:**
  - Tạo `Assets/Scripts/UI/BackgroundScroller.cs` (hoặc `Assets/Scripts/BackgroundScroller.cs`).
- **Chi tiết:**
  - Chuyển nền về **world-space** (sprite + `SpriteRenderer`, đặt trước camera theo Z, dùng parallax nếu muốn) HOẶC giữ nguyên UI Image nhưng **lẻ texture/sprite nền bằng shader hoặc offset texture** theo thời gian. Chọn phương án nào thì phải đảm bảo **loop vô hạn, mép nối liền**, không "nhảy" khi nối.
  - Tốc độ scroll phải **tỷ lệ với `AutoScroll.speed`** (không viết cứng) để khi tăng/giảm tốc map, nền vẫn khớp.
  - Khi quiz dừng (`Time.timeScale = 0`) nền cũng phải **dừng** (dùng `Time.deltaTime`), khi thắng Boss tiếp tục chạy.

### T2. Tách panel quiz theo chế độ
- **Input:** `PlayerPrefs "Sinh_QuizMode"` (`TracNghiem` / `TuLuan`), 2 panel trong scene: `paneltracno`, `paneltuluan`.
- **Output:** khi Boss mở quiz, chỉ hiện **đúng panel của chế độ**.
- **File cần sửa:** `Assets/Scripts/QuizManager.cs`.
- **Chi tiết:**
  - Thay 1 trường `panelCuonVo` chung bằng **2 trường**: `panelTracNo` (→ object `paneltracno`) và `panelTuLuan` (→ object `paneltuluan`).
  - Gán lại 2 object đó trong Inspector của `QuizManager` (hoặc `BiologyMenuManager` khi build UI).
  - Trong `ShowQuiz()` / `HienThiCauHoiMoi()`: đọc chế độ → chỉ bật panel đúng chế độ, panel còn lại tắt.

### T3. Đồng hồ quiz theo chế độ
- **Input:** `Question.timeLimit` (field hiện có, mặc định 15s), chế độ chơi.
- **Output:**
  - **Trắc nghiệm: 30s/câu** (thay giá trị mặc định của môn Sinh).
  - **Tự luận: KHÔNG hiện đồng hồ ngược, không trừ máu do hết giờ** (ngắt bộ đếm ngược).
- **File cần sửa:** `Assets/Scripts/QuizManager.cs` (khối `dangDemNguoc` + `thoiGianMoiCau`).
- **Chi tiết:**
  - Đổi `thoiGianMoiCau = 30f` (hoặc set `q.timeLimit = 30` cho câu trắc nghiệm Sinh).
  - Nếu `Sinh_QuizMode == "TuLuan"`: `dangDemNguoc = false` (không khởi tạo đếm ngược), ẩn object `time` trong panel tự luận.
  - Hết giờ (chỉ trắc nghiệm): xử lý như **trả lời sai** (hiện tại đã `TruMau()` + đổi câu) — giữ nguyên hành vi.

### T4. Trả lời tự luận bằng **upload ảnh** + AI chấm kèm ảnh
- **Input:** nút upload trong panel `paneltuluan`; ảnh bài viết (PNG/JPG); text câu hỏi + đáp án mẫu + từ khóa.
- **Output:** người chơi có thể **gõ text HOẶC tải ảnh** bài làm; ảnh được gửi kèm cho AI khi chấm; nhận lại kết quả `isCorrect`, `score`, `feedback` ngắn gọn.
- **File cần tạo/sửa:**
  - Sửa `Assets/Scripts/QuizManager.cs` (thêm nút upload + lưu path ảnh).
  - Sửa `Assets/Scripts/Biology/AIGradingService.cs` (rộng mở `GradeAnswer` thêm tham số ảnh).
- **Chi tiết:**
  - Upload: lưu ảnh vào `Application.persistentDataPath` (hoặc dùng `EditorUtility`/dialog nếu chỉ chạy Editor), lưu đường dẫn vào 1 field tạm trong `QuizManager`.
  - Khi chấm: nếu có path ảnh → đổi `GradeAnswer` nhận thêm `byte[] imageBytes` + `string mimeType`, nhúng vào payload Gemini (field `inline_data` base64). **Lưu ý:** model `gemini-1.5-flash` hỗ trợ đa phương tiện; nếu chưa chắc, có thể đổi sang `gemini-1.5-flash` (đã đúng) hoặc giữ text-only khi chưa có ảnh.
  - Nếu AI lỗi / không hỗ trợ ảnh → fallback về chấm từ khóa phần text (đảm bảo `AIGradingService.FallbackGrade` vẫn chạy, không crash).

### T5. Rà soát `AIGradingService` (khổ hẹp, YAGNI)
- **Input:** kết quả từ T4.
- **Output:** hàm `GradeAnswer` có thêm nhánh xử lý **ảnh**; không sửa lại logic fallback từ khóa, không đổi tên hàm / class hiện có.
- **File cần sửa:** `Assets/Scripts/Biology/AIGradingService.cs`.
- **Chi tiết:**
  - Chỉ mở rộng: (a) thêm tham số ảnh (tuỳ chọn), (b) prompt bổ sung câu "nếu có ảnh bài làm, chấm theo nội dung ảnh". **KHÔNG** refactor regex parse JSON (rủi ro cao, YAGNI).

### T6. Trigger Boss "ở giữa màn hình"
- **Input:** vị trí Boss + vị trí camera mỗi frame.
- **Output:** Boss mở quiz **chính xác khi trôi đến giữa màn hình** (không phụ thuộc tốc độ map, không dùng timer chờ cứng 3s).
- **File cần sửa:** `Assets/Scripts/MonsterShowUp.cs` (thay `OnBecameVisible` + `Invoke`).
- **Chi tiết (gợi ý, không bắt buộc):**
  ```csharp
  void Update() {
      if (hasAppeared) return;
      float camY = Camera.main != null ? Camera.main.transform.position.y : 0f;
      if (transform.position.y <= camY + marginFront) { hasAppeared = true; TriggerQuiz(); }
  }
  ```
  - `marginFront` chỉnh Inspector (ví dụ 1–2u trước camera) để quiz bật đúng lúc Boss đến giữa. Hoặc trigger khi `Mathf.Abs(bossY - camY) < threshold`.
  - Giữ nguyên gọi `QuizManager.Instance.ShowQuiz(this.gameObject)` (không đổi interface).

### T7. Quản lý âm thanh & nhạc nền
- **Input:** nút UI `nhac`, `amthanh (1)` (hiện chỉ là Image, **chưa có AudioSource**); các sự kiện trong gameplay.
- **Output:** nhạc nền loop + SFX (chạm chướng ngại, Boss hiện, chọn đáp án, đúng/sai, Game Over), bật/tắt được bằng nút UI.
- **File cần tạo:** `Assets/Scripts/UI/AudioManager.cs` (hoặc `Assets/Scripts/AudioManager.cs`).
- **Chi tiết:**
  - 1 object `AudioManager` (singleton hoặc static) chứa **AudioSource BGM (loop)** + **AudioSource SFX (nhiều, pool hoặc coroutine)**.
  - Nút `nhac` (BGM) và `amthanh` (SFX) trong UI gọi `AudioManager.ToggleBGM()` / `ToggleSFX()`.
  - SFX cần (tên asset, để người chơi tự tải): `sfx_chuongngai.mp3`, `sfx_boss.mp3`, `sfx_dung.mp3`, `sfx_sai.mp3`, `bgm_chay.mp3` (hoặc format Unity hỗ trợ).
  - **KHÔNG** tự tạo file audio nếu chưa có asset — chỉ tạo code + placeholder, để người chơi thêm file thật sau.

---

## 5. CONSTRAINTS & QUY ƯỚC

- **Naming object scene:** tiếng không dấu (`paneltracno`, `paneltuluan`, `thanhmau`, `boss`, `nv`, ...). Khi tham chiếu trong code, dùng `FindFirstObjectByType` hoặc gán Inspector — **không** đổi tên object đã có.
- **KHÔNG đổi tên / delete khóa PlayerPrefs** (§3.3). Scene đích: vào `"SinhScene"`, thoát về `"LessonList 1"`.
- **API key Gemini:** chỉ đọc từ `PlayerPrefs "Gemini_ApiKey"`, **KHÔNG hardcode** trong source (nên để ở file cấu hình hoặc PlayerPrefs để người chơi tự nhập).
- **Công nghệ:** Unity 2021+, C#, TextMeshPro (TMP) cho mọi text UI, New Input System (đã có trong `PlayerMovement`).
- **Style code:** comment tiếng Việt, header/tooltip Inspector, tone nhẹ nhàng như các file hiện có.
- **KHÔNG sửa** (vùng cấm): bộ parse CSV trong `QuizManager`, `TeacherStore`, `ScoreRecorder`, khóa ranking, `AIGradingService.FallbackGrade` (logic dự phòng).

---

## 6. QUY TRÌNH CHẠY LẠI (Definition of Done)

Agent coi là **xong** khi chạy scene `SinhScene` và thấy:

1. ✅ Nền đường **cuộn liên tục** theo chiều chạy, khớp mép, không nháy.
2. ✅ Chướng ngại vật random; chạm vào → **trừ máu theo %** + vật thể biến + chớp đỏ + bất tử 1s + SFX.
3. ✅ Tới mốc → **Boss xuất hiện**, Boss trôi về **giữa màn hình** → game dừng + hiện đúng panel (trắc nghiệm `paneltracno` / tự luận `paneltuluan` theo chế độ đã chọn).
4. ✅ Trắc nghiệm: 30s/câu; hết giờ = sai (trừ máu, không cộng điểm).
5. ✅ Tự luận: **không đồng hồ**, gõ text HOẶC upload ảnh → AI chấm + giải đáp ngắn gọn (có API key) / chấm từ khóa (không API key).
6. ✅ Đúng → **+ điểm elo + máu** (thanh máu đầy dần); Sai → **không cộng elo, trừ máu**.
7. ✅ Máu về 0 → popup Game Over (Chơi lại / Thoát), điểm + xếp hạng + ghi Excel được lưu.
8. ✅ Nhạc nền chạy loop, SFX đúng các sự kiện, bật/tắt bằng nút UI.

---

## 📌 GHI CHÚ CHO AGENT

- Làm **theo thứ tự T1 → T7**. T2 & T3 phụ thuộc panel đã có trong scene; T4 phụ thuộc T5 (interface hàm) nhưng có thể làm song song.
- Trước khi code, **cần đọc các file thực tế** trong §3.1 để khớp interface (đặc biệt `QuizManager.cs`, `ObstacleSpawner.cs`, `AIGradingService.cs`).
- Nếu 1 task có nhiều cách giải, **chọn cách tối thiểu thay đổi** (YAGNI), đừng refactor phần đang chạy.
- Sau mỗi task, chạy Play Mode kiểm tra theo §6 trước khi sang task kế.

---

*Tài liệu do group 8 PRU biên soạn. Khi cần cập nhật, giữ nguyên cấu trúc 6 mục này để các agent sau đọc liền mạch.*
