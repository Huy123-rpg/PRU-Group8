# Gói bàn giao Aurelia cho Antigravity

**Bạn làm theo ba bước:**

1. Giải nén toàn bộ gói vào một thư mục tài liệu trong project Unity mà Anti đang mở.
2. Gửi cho Anti nội dung của `02_Prompt_Antigravity.txt`, kèm các file trong gói và 34 ảnh thiết kế.
3. Yêu cầu Anti triển khai và ghi kết quả thực vào `05_Acceptance_Tests.csv`. Các ô NOT_RUN hiện là chưa chạy, không phải đã vượt qua kiểm thử.

|File|Dùng để làm gì|
|---|---|
|01_Luong_34_Man_Aurelia.md|Bản chính: ánh xạ đủ 34 ảnh, từng click, Back, làm lại, hết giờ, bản đồ sáng từng thiết bị, điểm và thành tích|
|02_Prompt_Antigravity.txt|Lệnh gửi Anti để triển khai trên project thật|
|03_GoogleSheets_AI_Backend.md|Nhập Google Sheets, backend, xác thực, OCR, chấm theo rubric, tô đỏ bước sai, retry và lưu dữ liệu|
|Aurelia_Game2_Data.xlsx|Bản sao bổ sung: giữ 18 tab nguồn; thêm cột và 21 tab G2_; sửa 44 ô lý thuyết|
|04_Data_Contracts.json|Mapping 21 thiết bị, mã bài tự luận và header bảng runtime|
|05_Acceptance_Tests.csv|90 ca nghiệm thu UI, dữ liệu, AI và online; đang NOT_RUN để Anti chạy|
|06_Grading_Output.schema.json|Schema output AI, backend còn phải kiểm ID và điểm tối đa|
|07_Bao_Cao_Kiem_Tra.md|Những kiểm tra dữ liệu đã thực hiện và phần chưa kiểm được|

## Google Sheets đã tạo

[Mở bản Google Sheets Aurelia](https://docs.google.com/spreadsheets/d/12ZM84_Sn3KB20R865KhTAgTRSCDh_Xg4yNhW1myV_zw/edit)

SpreadsheetId: `12ZM84_Sn3KB20R865KhTAgTRSCDh_Xg4yNhW1myV_zw`.

Đây là **bản sao mới** của workbook bổ sung, có 39 tab và múi giờ Asia/Ho_Chi_Minh. Chưa ghép vào Sheet chung của nhóm, chưa chia sẻ thêm cho thành viên, chưa có backend ghi dữ liệu game. Muốn dùng đúng Sheet nhóm đã nhắc tới, đưa URL đích cho Anti rồi làm theo phần merge an toàn trong file03.

## Các điểm cần biết trước khi chạy

- Ngân hàng có 830 câu trắc nghiệm và 64 câu tự luận; mỗi bài/mức tự luận hiện chỉ có một câu. Số câu trên UI phải động.
- 64 rubric là **bản đề xuất**, chưa phải đáp án đã được giáo viên phê duyệt. Có một số nội dung nguồn cần rà soát, đã ghi trong G2_VanDeDuLieu. Chấm thử được tách khỏi điểm chính thức.
- Key Gemini nhập ở backend/Script Properties, không nhập vào ô Sheet hoặc gửi vào chat. Model cấu hình được và phải thử bằng account thực.
- Không có project Unity hoặc key Gemini trong phiên chuẩn bị này, nên chưa chạy game, chưa gọi OCR/chấm thật và chưa thể cam kết các nút trong project hiện đã hoạt động. Bộ tài liệu yêu cầu Anti kiểm click thực và cung cấp bằng chứng.

Nếu Anti hỏi nên bắt đầu từ đâu: cho làm SceneCatalog, EventSystem/raycast và luồng local trước; sau đó map/gameplay, ngân hàng, kết quả; cuối cùng nối auth/Sheets/AI và weekly. Không cần chờ key để sửa các nút và điều hướng.
