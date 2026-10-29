---
title: Context Doc Sinh - Spec cho agent tự hoàn thiện module Sinh
description: >-
  Viết file markdown mô tả bối cảnh + spec gameplay module Sinh (runner vượt
  chướng ngại vật + quiz) để agent đọc và tự động hoàn thiện các phần còn thiếu
  trong codebase Unity.
status: completed
priority: P2
branch: basck
tags:
  - sinh
  - gameplay-doc
  - agent-context
blockedBy: []
blocks: []
created: '2026-10-29T05:59:19.045Z'
createdBy: 'ck:plan'
source: skill
---

# Context Doc Sinh - Spec cho agent tự hoàn thiện module Sinh

## Overview

Module "Sinh" vừa được đập đi xây lại phần giao diện Unity (scene `SinhScene.unity`). Mục tiêu của plan này: **viết 1 file markdown duy nhất** (tên `DOCUMENTATION_GAME_SINH.md`, đặt ở root repo, theo đúng quy ước đã có của `DOCUMENTATION_TEACHER_BIOLOGY.md`) mô tả **bối cảnh game + spec chi tiết gameplay** để một AI agent đọc là hiểu ngay:

1. Game là gì, thế giới/văn tự (lore) ra sao
2. Cơ chế hoạt động chính xác (scroll nền, chướng ngại vật, boss theo mốc, quiz theo chế độ, máu/điểm, âm thanh)
3. Hiện trạng codebase: file nào đã có, file nào thiếu, cần tạo/ sửa gì
4. Các constraint kỹ thuật để agent không sai hướng (naming, PlayerPrefs keys, scene names, AI API)

**Gameplay theo yêu cầu của user (2026-10-29):**

- Game **vượt chướng ngại vật** (endless runner dọc theo trục Y, camera tự đi lên)
- Nền `bg` hiện là **image tĩnh** → phải làm **scroll liên tục** tạo hiệu ứng chạy trên đường (vòng lặp vô tận, không nháy, khớp mép)
- Chướng ngại vật **xuất hiện random**, player chạm → **trừ máu theo % của từng vật thể**
- **Boss xuất hiện sau mỗi mốc quãng đường** (không random)
- Boss về **giữa màn hình** → mở popup:
  - `paneltracno` nếu chế độ chơi là **trắc nghiệm** (1 câu, **30s**/câu)
  - `paneltuluan` nếu chế độ **tự luận** (**không giới hạn thời gian**)
- Chế độ chơi do người dùng **chọn trước khi vào game** (menu Sinh học)
- **Tự luận**: trả lời bằng **viết vào khung** HOẶC **upload ảnh** bài viết → gọi **API key AI** chấm điểm + **giải đáp ngắn gọn**
- Mỗi câu **đúng**: cộng điểm **elo + máu** (thanh máu); **sai**: không cộng elo, **trừ máu**
- Game phải có **âm thanh (SFX) và nhạc nền (BGM)**

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Research](./phase-01-research.md) | Completed |
| 2 | [Spec](./phase-02-spec.md) | Completed |
| 3 | [Review](./phase-03-review.md) | Completed |

## Deliverable

- File: `DOCUMENTATION_GAME_SINH.md` (root repo, tiếng Việt)
- Tiêu chuẩn: agent nhận file này + repo là có thể tự viết code hoàn thiện các phần thiếu mà không cần hỏi lại user

## Dependencies

Không có plan khác. (Plan cũ của Trung tâm Giáo viên là `DOCUMENTATION_TEACHER_BIOLOGY.md` — độc lập, không chặn.)
