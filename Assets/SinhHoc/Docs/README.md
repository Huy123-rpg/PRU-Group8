# MÔN SINH HỌC — Game "SINH TỒN MIỄN DỊCH" (kiểu Vampire Survivors)

Game môn Sinh Học — bạn hóa thân thành **tế bào trắng** chiến đấu solo cả đội quân
mầm bệnh: **bắn kháng thể bằng chuột**, **dash né đòn**, di chuyển giữa 3 loại mầm
bệnh, thu bong bóng XP để **lên cấp → trả lời câu hỏi Sinh học → chọn nâng cấp**
(có độ hiếm Common / Rare / Epic / Legendary).

---

## 1. Luật chơi

- **Di chuyển:** WASD / mũi tên — **Bắn:** CHUỘT TRÁI (ngắm bằng tâm ngắm xanh)
- **Dash:** SPACE — lướt nhanh + bất tử ngắn để né đạn/né lao (có thanh hồi chiêu)
- **Máu:** 8 HP, chạm mầm bệnh/bị bắn trúng = mất máu, có 0.8s bất tử sau khi trúng
- **3 loại mầm bệnh** (mở khóa dần theo thời gian):
  - 🟢 **Đuổi theo** — lao vào người chơi từ đầu game
  - 🟣 **Bắn xa** — giữ khoảng cách và nhả gai độc (sau 35s)
  - 🔴 **Lao vào** — nhấp nháy đỏ cảnh báo rồi **lao** rất nhanh (sau 70s)
- **ELITE:** một số con có viền vàng, to hơn: **Nhanh / Giáp / Hút máu / Nổ khi chết**
  — nhiều XP và điểm hơn
- **XP:** mầm bệnh chết rơi **bong bóng XP** — lại gần để hút về, đủ XP → LÊN CẤP
- **Lên cấp → câu hỏi Sinh học (20 giây):**
  - **ĐÚNG** → chọn 1 trong 3 nâng cấp (có thể ra Rare/Epic/Legendary)
  - **SAI / hết giờ** → hiện **đáp án đúng + giải thích**, vẫn được 2 nâng cấp thường
    (không bị phạt chết — cứ chiến đấu tiếp!)
- **🔥 STREAK đúng liên tiếp:** 2 câu = EXP +10% · 3 câu = Damage +1 · 5 câu = Crit +15%
  · **10 câu = ⚡ AWAKENING (15s: Damage ×2, bắn nhanh hơn, chạy nhanh hơn, EXP ×2)**
- **Phút thứ 1: BOSS** (60 HP, bắn vòng đạn):
  - Boss còn **70% HP → ⚡ KNOWLEDGE CLASH**: slow-motion + câu hỏi boss
    - Đúng → **PERFECT COUNTER**: boss bị choáng 5s, đánh sập nó!
    - Sai → boss tung **Ultimate** (vòng đạn lớn) — phải né
  - Boss còn **30% HP → NỔI ĐIÊN**: nhanh hơn, đạn dày hơn
- **CHIẾN THẮNG:** sống sót 3 phút. **THUA:** hết máu

## 2. Nâng cấp & độ hiếm

| Độ hiếm | Màu | Ghi chú |
|---|---|---|
| **Common** | xám | 10 loại cơ bản |
| **Rare** ★ | xanh | hiệu ứng mạnh hơn |
| **Epic** ★★ | tím | mạnh hơn nữa |
| **Legendary** ✦✦✦ | cam | chỉ xuất hiện khi trả lời **ĐÚNG** — nâng cấp đổi lối chơi |

Legendary: Đạn Thiên Thần · Máu Vampire (giết quái hồi HP) · Tốc Độ Bóng Đêm ·
Chí Mạng Thiên Thần.

## 3. Câu hỏi theo cấp (độ khó tăng dần)

| Cấp | Mức câu hỏi |
|---|---|
| Lv.2–3 | Dễ (tế bào, cơ quan, vi sinh) |
| Lv.4–5 | Trung bình (tuần hoàn, hô hấp, miễn dịch) |
| Lv.6+ | Khó (di truyền, nguyên phân, miễn dịch) |
| Knowledge Clash | Câu hỏi BOSS chuyên sâu |

## 4. Nạp câu hỏi từ Google Sheet

1. Tạo Google Sheet, dòng đầu là tiêu đề:
   `Level,Question,A,B,C,D,Correct,Explain`
   - `Level`: `de` | `trungbinh` | `kho` | `boss`
   - `Correct`: `A/B/C/D`
   - `Explain`: **tùy chọn** — giải thích hiện khi trả lời sai
   - Ô có dấu phẩy → bọc trong `"..."`
2. **File → Share → Publish to web** → định dạng **Comma-separated values (.csv)** → Publish
3. Copy link (dạng `...pub?output=csv`), dán vào Main Camera → **Biology Bootstrap** →
   *Google Sheet Csv Url* trong scene `SinhHoc`
4. Play → Console hiện `✓ Đã tải N câu hỏi từ Sheet! (đã lưu cache offline)`

**Chơi offline:** lần tải Sheet thành công đầu tiên được lưu vào máy
(`sinhhoc_questions_cache.csv`). Sau đó mất mạng game vẫn chơi với bộ câu hỏi đã
cache; không từng có mạng thì dùng bộ câu hỏi mẫu trong `BiologyQuestionBank.cs`.
Row sai (thiếu đáp án, Correct không hợp lệ...) bị bỏ qua và ghi cảnh báo ra Console.

## 5. Cấu trúc code (Assets/SinhHoc/Scripts)

| File | Vai trò |
|---|---|
| `BiologyGameConfig.cs` | **Toàn bộ thông số cân bằng game** (máu, dash, 3 loại quái, elite, streak, boss...) |
| `BiologySurvivalManager.cs` | "Bộ não": spawn + elite, XP, lên cấp → hỏi bài → nâng cấp rarity, streak/awakening, Knowledge Clash, kết quả |
| `BiologySurvivalCore.cs` | Người chơi (bắn chuột + dash), đạn, 3 loại quái AI, elite modifier, boss 3 phase, XP orb |
| `BiologyQuestionBank.cs` | Ngân hàng câu hỏi (CSV + cache offline + câu mẫu) |
| `BiologyQuizUI.cs` | Bảng câu hỏi có timer + giải thích, nâng cấp rarity, banner Clash, Awakening, màn kết quả |
| `BiologyHUD.cs` | Máu, XP, thanh hồi chiêu dash, streak, đồng hồ, điểm |
| `BiologySpriteFactory.cs` | Vẽ toàn bộ hình ảnh bằng code (không cần asset ngoài) |
| `BiologyBootstrap.cs` | Tự dựng scene khi Play, không cần setup tay |

Cân bằng độ khó: chỉnh số trong `BiologyGameConfig.cs` — mọi thứ nằm ở một chỗ.

## 6. Sửa lỗi nhanh

| Triệu chứng | Xử lý |
|---|---|
| Bắn không trúng / đạn xuyên không biến mất | Đảm bảo đạn có Collider2D *Is Trigger* (script tự thêm khi tạo đạn) |
| Không click được nút UI | Đảm bảo EventSystem tồn tại (Bootstrap tự tạo) |
| `⚠ Lỗi tải Sheet` | Link chưa Publish to web, hoặc thiếu `output=csv` — game vẫn chơi bằng cache |
| Console `Không có câu hỏi` | Sheet thiếu dòng cho mức `de/trungbinh/kho/boss` |
| Boss không hiện Knowledge Clash | Câu hỏi boss chỉ hỏi 1 lần mỗi boss, khi boss xuống dưới 70% HP |
| Scene menu không vào được game | Build Settings phải có cả `SampleScene` và `SinhHoc` |
