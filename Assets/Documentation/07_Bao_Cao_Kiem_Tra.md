# Báo cáo kiểm tra gói dữ liệu Aurelia

Ngày: 28/10/2026.

## Đã thực hiện

|Kiểm tra|Kết quả|
|---|---|
|Giữ đủ 18 tab nguồn|PASS|
|830 câu trắc nghiệm|PASS|
|64 câu tự luận|PASS|
|Không trùng mã câu trắc nghiệm|PASS|
|Không trùng mã câu tự luận|PASS|
|Khóa đáp án MCQ hợp lệ A/B/C/D|PASS|
|Mã bài MCQ tồn tại|PASS|
|Mã bài tự luận chuẩn hóa tồn tại|PASS|
|21 thiết bị đúng phân bố 4/7/4/3/3|PASS|
|64 rubric, mỗi rubric tối đa 100 điểm|PASS|
|Rubric giữ trạng thái DRAFT, không giả mạo duyệt|PASS|
|Bảng runtime mới không chứa kết quả giả|PASS|
|Giữ dữ liệu nguồn ngoài đúng 44 ô sửa lỗi|PASS|
|Bản Google có 39 tab|PASS|
|Giá trị Google xuất lại khớp bản Excel|PASS|
|Không có lỗi công thức trong bản Excel|PASS|
|90 ca nghiệm thu được ghi rõ NOT_RUN|PASS|
|JSON đọc được, schema có đủ trường và enum bước|PASS|

Đối chiếu toàn bộ ô nguồn: không có thay đổi dữ liệu ngoài 44 ô lý thuyết được chuyển từ công thức giả thành text. Các cột/tab mới được thêm riêng. Bản Google Sheets đã xuất lại và đối chiếu dữ liệu với bản Excel giao; không có khác biệt giá trị. Đã xem các vùng đại diện của 39 tab qua bản xuất và kiểm tra các vùng mới; giữ bố cục tab nguồn. Một số cột nguồn vốn hẹp, có thể cần mở ô để đọc đầy đủ văn bản.

## Chưa thực hiện, không được coi là PASS

- Chạy Play Mode/build Unity, kiểm click trên 34 màn trong project thật.
- Kết nối auth của nhóm, deploy backend, gọi Gemini OCR/chấm bằng key thật.
- Chạy 90 ca nghiệm thu: tất cả vẫn NOT_RUN.
- Duyệt chuyên môn toàn bộ 894 câu và 64 rubric. Rubric hiện là đề xuất, không giả mạo người duyệt.
- Ghép vào Sheet chung đích và chia sẻ cho thành viên nhóm, vì chưa xác định URL và người nhận.

Báo cáo này xác nhận kiểm tra cấu trúc và tính toàn vẹn của dữ liệu, không xác nhận AI chấm chính xác hoặc game đã chạy hoàn chỉnh.
