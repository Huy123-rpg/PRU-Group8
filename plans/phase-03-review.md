---
phase: 3
title: Review
status: completed
priority: P2
dependencies:
  - 2
---

# Phase 3: Review — Duyệt doc trước khi giao cho agent

## Overview

Pass rà soát cuối: đảm bảo `DOCUMENTATION_GAME_SINH.md` không mâu thuẫn với code hiện có và đủ thông tin để agent tự chạy.

## Checklist

- [ ] Đối chiếu từng task T1–T7 với file hiện có (không chỉ ra lệnh sửa file sai tên)
- [ ] Bối cảnh/lore nhất quán với UI hiện có (sprite chibi, phong cách "chạy trên đường")
- [ ] Quy tắc máu/điểm khớp với `QuizManager` (cộng hpBonus%, +10 điểm) và `ObstacleDamage` (default 15%)
- [ ] Constraint key PlayerPrefs, scene names, naming objects giữ nguyên như Phase 1
- [ ] Phần "không được sửa" rõ ràng (CSV parser, TeacherStore, ranking keys)
- [ ] Read lần cuối như "agent lạ đọc repo + doc": còn thiếu context gì không

## Success Criteria

- [ ] Doc pass 6 mục checklist
- [ ] Không có câu chỉ dẫn mơ hồ kiểu "xử lý hợp lý" (mỗi hành vi phải có giá trị/số cụ thể)
