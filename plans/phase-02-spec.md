---
phase: 2
title: Spec
status: completed
priority: P1
dependencies:
  - 1
---

# Phase 2: Spec — Nội dung file DOCUMENTATION_GAME_SINH.md

## Overview

Viết `DOCUMENTATION_GAME_SINH.md` (tiếng Việt, root repo) đúng cấu trúc dưới. Đây là **file context cho agent** — ngôn ngữ tường minh, có "đã có / cần làm" rõ ràng, kèm bảng mapping đối tượng scene, để agent đọc repo + file này là code được ngay.

## Cấu trúc bắt buộc của file markdown

```
1. BỐI CẢNH GAME (Lore & Mục đích)
   - Game học Sinh học dạng endless runner 2D dọc (camera đi lên trục Y)
   - Nhân vật chạy trên "con đường kiến thức"; né/bù trừ chướng ngại vật
   - Mỗi mốc đường → Boss (thử thách) → màn hình quiz dừng game
2. QUY TẮC GAMEPLAY (spec chính)
   2.1 Nền: bg cuộn liên tục, loop vô tận, hiệu ứng chạy; đồng bộ tốc độ AutoScroll
   2.2 Chướng ngại vật: spawn random theo ObstacleSpawner; chạm → trừ máu theo % (ObstacleDamage); vật thể bị destroy sau khi chạm; bất tử 1s
   2.3 Boss: xuất hiện tại mốc quãng đường cố định (levels[] của ObstacleSpawner); boss về GIỮA màn hình (pos Y ≈ pos camera Y) → dừng (timeScale = 0) + mở quiz panel
   2.4 Quiz theo chế độ (PlayerPrefs.Sinh_QuizMode, chọn ở BiologyMenuManager TRƯỚC khi vào scene):
       - TracNghiem → paneltracno: 1 câu, GIỚI HẠN 30s/câu; hết giờ = sai
       - TuLuan    → paneltuluan: KHÔNG GIỚI HẠN thời gian; trả lời bằng (a) gõ text hoặc (b) upload ảnh bài viết
   2.5 Chấm tự luận: gọi AIGradingService (Gemini, key = PlayerPrefs "Gemini_ApiKey"); AI trả: isCorrect, score, feedback ngắn gọn (1-2 câu)
   2.6 Điểm & máu:
       - Trả lời ĐÚNG: + elo (điểm rank, hiện = +10 điểm) VÀ + máu (hpBonus%, mặc định ~20%)
       - Trả lời SAI: KHÔNG cộng elo, TRỪ máu (thanhmau)
       - Máu = 0 → Game Over popup (đã có trong QuizManager)
3. HIỆN TRẠNG CODEBASE (đã có — KHÔNG VIẾT TRÙNG)
   - Bảng mapping scene objects + scripts như Phase 1
4. NHIỆM VỤ CẦN TỰ HOÀN THIỆN (worklist cho agent)
   T1. Scroll bg: đưa 2 object "bg" (UI Image trong Canvas) sang 2 layers world-space (SpriteRenderer, parallax) hoặc làm texture scroll trong shader; phải loop vô hạn, không nháy, khớp mép theo chiều Y
   T2. QuizManager: tách panelCuonVo → gán paneltracno (TracNghiem) + paneltuluan (TuLuan); chọn panel theo PlayerPrefs.Sinh_QuizMode
   T3. Timer: TracNghiem = 30s/câu (đổi thoiGianMoiCau hoặc theo question.timeLimit); TuLuan tắt hẳn đếm ngược (không hiện timer, không trừ máu do hết giờ)
   T4. Upload ảnh bài tự luận: nút upload trong paneltuluan; lưu ảnh vào thư mục (Application.persistentDataPath); gửi kèm text khi chấm; nếu dùng model không hỗ trợ ảnh → OCR/parse fallback hoặc chỉ chấm phần text (ghi rõ trong doc)
   T5. AIGradingService: hỗ trợ thêm ảnh (hoặc note rõ giới hạn hiện tại là text-only)
   T6. Trigger boss ở giữa màn hình: thay OnBecameVisible + Invoke 3s bằng trigger theo khoảng cách Y tới camera (boss.position.y - camera.position.y ≈ 0)
   T7. Audio: thêm AudioSource nhạc nền (loop) + SFX (chạm chướng ngại vật, đúng/sai, boss xuất hiện, bấm nút); quản lý trong 1 Audio Manager (DontDestroyOnLoad không bắt buộc vì game chỉ 1 scene)
   T8. (Không làm) Don't touch: CSV parser, TeacherStore, ranking PlayerPrefs keys
5. CONSTRAINTS & QUY ƯỚC
   - Naming scene objects tiếng không dấu (paneltracno, paneltuluan, thanhmau...)
   - Không đổi PlayerPrefs keys; scene target: "SinhScene", menu về: "LessonList 1"
   - API key Gemini: lấy từ PlayerPrefs "Gemini_ApiKey" (không hardcode)
   - C#/Unity 2021+, TMP cho text, New Input System
   - Comment tiếng Việt, tone nhẹ nhàng theo style repo hiện có
6. QUY TRÌNH CHẠY LẠI (definition of done)
   - Play scene SinhScene: bg cuộn, chạm chướng ngại vật trừ máu + flash,
     tới mốc → boss giữa màn hình → đúng panel theo chế độ → quiz → kết quả cập nhật máu/điểm → tiếp tục chạy
```

## Yêu cầu chất lượng

- Mỗi task T1–T7 phải có: **Input/Output cụ thể**, **file cần tạo/sửa** (duyệt danh sách file), **hàm chính (pseudo code nếu cần)**
- Ghi rõ **file nào KHÔNG được sửa** (CSV parser, AIGradingService fallback logic) để tránh agent refactor thừa
- Bảng scene objects (tên → role → script gán) copy từ Phase 1

## Success Criteria

- [ ] File `DOCUMENTATION_GAME_SINH.md` tồn tại root repo, tiếng Việt, đủ 6 mục trên
- [ ] Agent lần đầu đọc repo + file này: code được T1–T7 không cần hỏi user
- [ ] Không mâu thuẫn với code hiện có (đã cross-check với Phase 1)
