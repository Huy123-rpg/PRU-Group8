# Nối tự luận Unity với Google Apps Script hiện có

Backend là Web App Apps Script cùng dự án đang lưu LichSuChoi. Không chạy dịch vụ CMD/Python riêng.

## 1. Cập nhật Apps Script

Mở Google Sheet gắn với Apps Script lưu lịch sử, chọn Tiện ích mở rộng > Apps Script. Lưu lại mã cũ để có thể khôi phục. Thay toàn bộ nội dung Code.gs bằng nội dung Code.gs trong thư mục này. Chỉ giữ một hàm doPost và một hàm doGet trong toàn dự án Apps Script; không dán thêm bản thứ hai bên cạnh hàm cũ. Nhấn Lưu.

Luồng lịch sử vẫn dùng 45 cột theo đúng thứ tự bản cũ. Yêu cầu chấm không được ghi thành dòng lịch sử. Script mới hỗ trợ cả Unity client cũ chỉ gửi questionID và client mới gửi action = gradeEssay.

## 2. Đặt Script properties

Trong Apps Script, mở biểu tượng bánh răng Cài đặt dự án > Thuộc tính tập lệnh (Script properties) > Thêm thuộc tính:

| Tên | Giá trị |
| --- | --- |
| OPENAI_API_KEY | Khóa mới, chưa từng gửi vào chat |
| AI_GRADING_ENABLED | true |
| OPENAI_MODEL | gpt-4.1-mini |
| SPREADSHEET_ID | ID của file Google Sheet CHỨA tab TuLuan |
| ESSAY_SHEET_NAME | TuLuan |
| DAILY_GRADE_LIMIT | 20 |

Lấy SPREADSHEET_ID từ địa chỉ khi đang sửa Sheet: https://docs.google.com/spreadsheets/d/ID_CUA_FILE/edit . Chỉ lấy đoạn ID giữa /d/ và /edit, không dùng link xuất bản /d/e/2PACX.../pub hoặc gid.

Tab TuLuan phải có: CauHoiID, NoiDung, LoaiCauHoi, DapAnTuLuanMau, Rubric, MaxScore. Mã duy nhất, LoaiCauHoi = TuLuan, TrangThai = 1. BaiID/MucDo vẫn dùng cho bộ lọc trong Unity. Không cần dán link CSV vào Apps Script: nó đọc Sheet trực tiếp bằng ID, theo quyền của người triển khai.

Lịch sử mặc định vẫn ghi vào file Sheet đang gắn với Apps Script như mã cũ. Nếu Apps Script độc lập hoặc bạn muốn dùng một file lịch sử khác, thêm HISTORY_SPREADSHEET_ID là ID file chứa LichSuChoi. SPREADSHEET_ID của câu hỏi không đổi nơi lưu lịch sử mặc định.

Khóa chỉ đặt trong Script properties. Không điền khóa trong code, Inspector Unity hoặc Git. Người có quyền sửa dự án Apps Script có thể truy cập Script properties, nên chỉ chia sẻ quyền sửa với người tin cậy.

## 3. Cấp quyền và kiểm tra cấu hình

Trở lại Trình chỉnh sửa. Trong danh sách hàm phía trên, chọn kiemTraCauHinh > Chạy. Cấp quyền đọc Sheet và kết nối ngoài theo yêu cầu Google cho dự án của chính bạn.

Hàm này chỉ kiểm tra cấu hình, đọc câu đầu và gửi yêu cầu kết nối đến Google. Nó KHÔNG gọi OpenAI tính phí, không ghi lịch sử, không in khóa. Thông báo cấu hình hợp lệ chưa xác nhận hiệu lực khóa hoặc số dư API.

## 4. Cập nhật bản Web App đang triển khai

Triển khai > Quản lý các bản triển khai > chọn Web App đang dùng cho lịch sử > biểu tượng bút chì > Phiên bản mới > Triển khai.

- Thực thi dưới quyền: Tôi/người triển khai (để backend đọc Sheet và gọi AI).
- Ai có quyền truy cập: Bất kỳ ai, nếu tài khoản/tổ chức cho phép. Unity Editor hiện không gửi đăng nhập Google. Nếu tổ chức không cho phép truy cập này thì cần phương án xác thực khác, không thể chỉ dán URL rồi dùng.
- Sao chép URL Web App kết thúc bằng /exec. Không dùng /dev hoặc URL script.googleusercontent.com tạm thời.
- Mở /exec trong trình duyệt: bản mới phải trả version = biology-essay-v1. Nếu vẫn chỉ có thông báo History API cũ thì chưa cập nhật đúng deployment.

Nếu tạo bản triển khai mới thay vì cập nhật bản cũ, URL thay đổi. Khi đó cập nhật cả trường lưu lịch sử trong scene SinhHoc để dùng cùng bản mới nếu muốn tiếp tục chung một endpoint.

## 5. Unity

AIGradingClient.cs mới đã được chuẩn bị để đọc lỗi JSON từ Apps Script và gửi action cho nhánh chấm. Giữ file .meta cũ khi thay code, không xóa component đang có trong scene.

Ngoài Play Mode, mở EssayMaze > MazeManager > AI Grading Client:

- Mode: RealBackend
- Backend Url: URL /exec vừa cập nhật ở bước 4
- Timeout Seconds: 180

Maze Essay Questions > Google Sheet Csv Url vẫn dùng link CSV của tab TuLuan như hiện tại. Đường dẫn CSV lấy câu hỏi và đường dẫn /exec chấm bài là hai ô khác nhau. Ctrl+S lưu scene. Không cần thêm /api/grade sau URL /exec.

Vào game từ SampleScene > Sinh học > chương > bài > Tự luận. Thử bài đúng, bài sai và ảnh tương ứng đề đang hiện. Chỉ khi nhận điểm hợp lệ và đạt ngưỡng ở MazeBossGate thì game mới mở cổng. Backend không trực tiếp mở cổng.

## Lỗi thường gặp

- MISSING_API_KEY: thiếu khóa trong Script properties.
- GRADING_DISABLED: AI_GRADING_ENABLED chưa phải chuỗi true.
- AI_AUTH: khóa sai/bị thu hồi.
- AI_QUOTA: hết số dư/hạn mức API hoặc gửi quá nhanh. Thanh toán ChatGPT và thanh toán API là hai phần riêng.
- DAILY_LIMIT: đạt giới hạn số lần gọi OpenAI trong ngày của toàn backend. Bộ đếm tự sang ngày mới theo giờ Việt Nam. Lần gọi bị OpenAI từ chối cũng tính vào bộ đếm để tránh lặp vô hạn.
- ESSAY_SHEET_MISSING / SHEET_HEADERS: sai file Sheet, tên tab hoặc tiêu đề.
- QUESTION_NOT_FOUND: ID trong game và tab TuLuan không khớp.
- INVALID_AI_RESPONSE: AI trả dữ liệu không đầy đủ hoặc điểm không hợp lệ. Không tính bài sai.
- Backend không trả JSON: kiểm tra /exec, quyền truy cập và bản triển khai.

## Phạm vi và kiểm chứng

Giới hạn ảnh sau nén 4 MB, bài gõ 5000 ký tự, số lần gọi AI mặc định 20/ngày cho toàn bộ backend. Mỗi lượt gọi có thể mất phí, phụ thuộc model và độ dài/ảnh. Không lưu bài hoặc ảnh lên Sheet trong nhánh chấm; chúng được gửi sang OpenAI. Hạn mức này là giới hạn số lượt, không phải giới hạn chi phí tiền tệ hay cơ chế xác thực người chơi.

Web App công khai chưa có xác thực người chơi đáng tin cậy theo mã đăng nhập hiện tại. Bản này phù hợp thử nghiệm có giới hạn; trước khi mở rộng nhiều người dùng cần tích hợp xác thực và phân bổ hạn mức theo tài khoản tại backend. Không coi một token ghi cứng trong Unity là bí mật.

Kiểm thử offline giả lập Google/OpenAI đã kiểm tra hợp đồng lịch sử 45 cột, hai kiểu bài làm, lỗi API, dữ liệu không hợp lệ và giới hạn lượt. Client được kiểm tra biên dịch với thư viện Unity 6000.6.0f1. Chưa triển khai vào tài khoản Google, chưa gọi API tính phí và chưa chạy lại trong Unity Editor thực tế.

Tài liệu: [Google Web Apps](https://developers.google.com/apps-script/guides/web), [Content Service](https://developers.google.com/apps-script/guides/content), [Script properties](https://developers.google.com/apps-script/guides/properties), [OpenAI Structured Outputs](https://developers.openai.com/api/docs/guides/structured-outputs).
