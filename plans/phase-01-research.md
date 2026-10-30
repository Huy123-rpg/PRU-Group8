---
phase: 1
title: Research
status: completed
priority: P1
dependencies: []
---

# Phase 1: Research — Khảo sát hiện trạng module Sinh

## Overview

Xác định chính xác những gì ĐÃ có trong `SinhScene` và các script liên quan, để spec trong Phase 2 chỉ yêu cầu agent làm phần THẬT SỰ thiếu, tránh sai hướng hoặc viết trùng.

## Hiện trạng đã xác minh (2026-10-29)

### File script hiện có (Assets/Scripts/)

| File | Vai trò | Trạng thái |
|------|---------|-----------|
| `MapManager.cs` | Sinh chunk map vô tận theo trục Y (`chunkLength = 20`, `chunksOnScreen = 3`), theo `cameraTransform` | ✅ Có, gán Inspector |
| `AutoScroll.cs` | Đẩy camera lên theo `Vector3.up * speed * dt` | ✅ Có |
| `ObstacleSpawner.cs` | Random chướng ngại vật thường (1–3/chunk, `khoangCachAnToan = 2.5`); Boss theo mốc cố định `MonsterLevel[]`; sau mốc cuối → lặp Boss cycle 40m | ✅ Có |
| `PlayerCollision.cs` | Chạm tag `Obstacle` → `Destroy`, trừ máu theo `ObstacleDamage.phanTramTruMau`, bất tử 1s + chớp đỏ | ✅ Có |
| `ObstacleDamage.cs` | 1 float `phanTramTruMau` (mặc định 15%) | ✅ Có |
| `PlayerMovement.cs` | A/D hoặc ←/→ di chuyển X, clamp `[minX, maxX]` (New Input System) | ✅ Có |
| `MonsterShowUp.cs` | `OnBecameVisible` + Invoke 3s → `QuizManager.Instance.ShowQuiz(this.gameObject)` | ✅ Có |
| `QuizManager.cs` | Core: nạp CSV/GoogleSheet/TeacherStore, filter theo chapter/lesson/mode, quiz 5 câu/Boss, `panelCuonVo`, máu (cộng `hpBonus`% / trừ), score +10/câu đúng, ranking PlayerPrefs, Game Over popup (chơi lại/thoát) | ✅ Có |
| `Biology/AIGradingService.cs` | Chấm tự luận bằng Gemini `gemini-1.5-flash`, key từ PlayerPrefs `Gemini_ApiKey`, timeout 10s, fallback từ khóa | ✅ Có |
| `UI/BiologyMenuManager.cs` | Chọn chương → bài → chế độ; set `Sinh_SelectedChapter/Lesson/QuizMode` rồi `LoadScene("SinhScene")` | ✅ Có |
| `Quiz/QuizManager.cs`, `QuizTimer.cs`, `QuestionData.cs` | Module quiz riêng (dùng cho môn khác?) | Kèm |

### Scene `SinhScene.unity`

- Có 2 object `bg` (Layer 5, RectTransform = UI Image trong Canvas) — **tĩnh, không cuộn**
- Có các object: `paneltracno` (line 384), `paneltuluan` (line 6359), `boss`, `boss (1)..(11)`, `chuongngaivat` + (1)-(6), `nv`, `nv (1)-(3)`, `thanhmau`, `thanhmau (1)`, `hoimau`, `dapana/b/c`, `nopbai`, `nhac`, `nhac (1)`, `amthanh (1)`, `debai`, `panelpause`, `time`, `choitiep`, `choilai`, `thoat`
- `nhac`, `nhac (1)`, `amthanh (1)` là **object UI (Image + nút)** ở Layer 5, **KHÔNG có AudioSource nào trong scene** → âm thanh/nhạc nền là feature mới hoàn toàn (T7)
- `paneltracno` có sẵn nhưng QuizManager hiện chỉ gán 1 trường `panelCuonVo` (chưa tách 2 panel theo chế độ)

### PlayerPrefs keys đang dùng (constraint không được đổi)

`Sinh_SelectedChapter`, `Sinh_SelectedLesson`, `Sinh_QuizMode` (TracNghiem|TuLuan), `Sinh_HighScore`, `Sinh_LastScore`, `Sinh_ScoreHistory`, `Sinh_CurrentUsername`, `Gemini_ApiKey`. Scene names: `SinhScene`, `LessonList 1`.

## Chênh lệch giữa yêu cầu user và hiện trạng

1. **bg scroll** — chưa có. `bg` là UI Image tĩnh trong Canvas; cần đưa sang world-space (SpriteRenderer/Tilemap) + hiệu ứng cuộn liên tục đồng bộ với `AutoScroll`
2. **Trắc nghiệm 30s/câu** — `QuizManager.thoiGianMoiCau = 15f`, cần 30s (đặc thù Sinh)
3. **Panel theo chế độ** — chưa tách: TracNghiem → `paneltracno` (hiện `panelCuonVo` chung), TuLuan → `paneltuluan` (hiện chưa gán)
4. **Tự luận không giới hạn thời gian** — chưa có (timer đang chạy cho mọi câu)
5. **Tự luận upload ảnh bài làm** — chưa có; `AIGradingService` chỉ nhận text
6. **AI chấm + giải đáp ngắn gọn cho tự luận** — có (Gemini) nhưng cần truyền ảnh nếu mode upload; key lấy từ đâu chưa thống nhất
7. **Đúng → cộng elo + máu; sai → chỉ trừ máu, không cộng elo** — hiện `currentScore += 10` mọi câu đúng, cần chuyển sang "elo" (rank/điểm); sai: không cộng điểm (đã đúng hành vi), chỉ trừ máu ✅
8. **Âm thanh + nhạc nền** — chưa có AudioSource
9. **Boss ở giữa màn hình** — `MonsterShowUp` dùng `OnBecameVisible` + 3s Invoke (sờ gồ, phụ thuộc tốc độ); nên trigger theo khoảng cách Y tới camera

## Risk Assessment

- Rủi ro cao nhất: agent hiểu nhầm "scroll bg" thành kéo sprite trong Canvas → spec phải chỉ rõ đưa bg ra world-space 2D (đồng trục với `AutoScroll`)
- Boss trigger bằng `OnBecameVisible` + Invoke 3s không ổn định khi speed thay đổi → spec nên quy định trigger theo vị trí (giữa màn hình = khoảng cách Y tới camera ≈ 0)
- `AIGradingService` parse JSON bằng regex (nặng, dễ vỡ) — chấp nhận hiện trạng, chỉ mở rộng thêm phần image, không refactor sâu (YAGNI)
