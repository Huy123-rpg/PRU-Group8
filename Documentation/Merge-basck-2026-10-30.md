# Ghép nhánh basck vào feature/hoa

## Quy tắc xử lý

- Nền trước pull: `180cf06`; nhánh Bách: `6583f86554f0956f50569a2887871391f7e7ca02`.
- Các thay đổi chưa commit trước khi ghép được sao lưu trong stash `6d21dfe575f6d63199a06ce2bdc5f4ffd12e33d8` (`codex-before-basck-merge`) và đã khôi phục vào kết quả ghép. Không cần apply/pop lại stash này.
- Giữ sửa đổi Hóa/Lý, DogRacer LAN, tài khoản học sinh, nguồn Sheet Hóa, font tiếng Việt và sửa QuizManager của game bắn vịt từ phiên bản trước pull.
- Lấy gameplay, câu hỏi, chọn chương/bài và TeacherCenter môn Sinh từ Bách.
- Tách menu môn Sinh thành `BiologyGameMenu`, nối từ menu chọn môn hiện có; giữ cả game vượt chướng ngại vật của Bách và game Sinh tồn (`Assets/SinhHoc`).
- Đưa AudioManager mới của Sinh vào namespace `ScienceQuest.Biology` để không che AudioManager của DogRacer.
- Bổ sung API thông tin đăng nhập cho luồng Sinh; giữ cách đăng nhập và phân quyền trước pull.
- Giữ Build Settings cũ, bổ sung TeacherCenter qua AutoSyncBuildScenes để không mất các scene của Hóa/Lý/Sinh tồn.
- Giữ sửa lỗi vòng đời GameManager/UIManager tương thích của hai nhánh, thêm reset static cho Editor khi tắt Domain Reload.

## Kiểm tra

Chạy `node Tools/test_biology_merge.mjs` khi Unity mở project, UnitySkills bật bypass và Editor đang dừng Play. Script gọi callback thật của các nút trong Play Mode, kiểm tra chuyển scene, nội dung bài theo chương và panel câu hỏi. Script dừng Play và mở lại scene ban đầu khi kết thúc.

Script không kiểm tra toàn bộ một trận, chấm điểm AI qua mạng, lưu Excel hay kết nối hai máy LAN. Các phần đó cần kiểm tra riêng nếu thay đổi cấu hình dịch vụ hoặc môi trường mạng.

Các kiểm tra đã thực hiện trong Unity 6000.6.0f1:

- Biên dịch C#: thành công, 0 lỗi (vẫn có cảnh báo API Unity cũ).
- Menu môn Sinh mở bộ chọn hai game; game vượt chướng ngại vật vào đúng ChapterList.
- Cả 5 chương có tên bài học riêng, không bị dùng chung danh sách của một chương.
- Vượt chướng ngại vật: vào SinhScene, có RunnerController, mở đúng panel trắc nghiệm/tự luận ở độ khó dễ.
- Sinh tồn: chọn chương C11, bài B39, vào SinhHoc cho trắc nghiệm và EssayMaze cho tự luận.
- Session giáo viên: vào TeacherCenter và quay lại Scene2_MainMenu.
- Menu Hóa vẫn vào Scene3_LessonType; menu Lý vẫn vào GameSelection.
- Không còn file unmerged hoặc dấu conflict trong mã nguồn, scene, prefab và cấu hình được quét.

## Trạng thái Git

Các conflict được giải quyết và stage để hoàn tất merge. Chưa tạo merge commit hoặc push. Bản sao lưu stash vẫn được giữ để đối chiếu.
