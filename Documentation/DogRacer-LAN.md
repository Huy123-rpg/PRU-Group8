# DogRacer PvP LAN 1v1

## Luồng chơi

Trắc nghiệm: chọn chương/bài → **Scene5_PlayMode** → Chơi với máy hoặc
PvP LAN → chọn chó → Sẵn sàng → đếm ngược 3 giây → đua → kết quả.
Tự luận tiếp tục vào chọn chó và chơi với máy, không có PvP.

Hai máy chọn cùng chương/bài rồi bấm PvP LAN. Không cần nhập IP hoặc chọn Host.
Các máy quảng bá phòng chờ qua UDP; ID phòng quyết định máy nào kết nối vào máy
nào, tránh cả hai cùng kết nối chéo. Chỉ ghép phòng trống cùng phiên bản giao
thức, cùng chương/bài. Đây là tìm tự động trong LAN, không đo khoảng cách vật lý.

Host giữ quyền chọn chó, đáp án, điểm, tăng tốc, vị trí và kết quả. Mỗi chó chỉ
thuộc một người. Khi hai người chọn cùng lúc, yêu cầu Host xử lý trước được nhận.
Bấm Sẵn sàng sẽ giữ lựa chọn; khi cả hai sẵn sàng, Host tải câu hỏi và yêu cầu
cả hai máy vào scene đua. Chỉ bắt đầu khi cả hai báo đã tải xong.

Ngân hàng dùng tab trắc nghiệm Hóa `gid=0` đã cấu hình. Host chia ngẫu nhiên
ngân hàng thành hai bộ rời nhau; khi hết bộ sẽ lặp lại trong bộ của người đó.
Hiện parser dùng ngân hàng Hóa chung, chưa ánh xạ ID chương/bài của Sheet vào
danh mục bài trong UI; lựa chọn chương/bài dùng để ghép phòng tương thích.
Không dùng câu hỏi mẫu để bắt đầu PvP nếu tải Sheet thất bại hoặc thiếu câu.

Host gửi riêng nội dung và bốn lựa chọn cho từng máy, không gửi đáp án đúng
cho client. Mỗi đáp án gửi kèm số thứ tự; Host loại yêu cầu trùng và quá nhanh.
Đúng: +100 điểm và tăng tốc; sai: ghi nhận câu sai. PvP không kết thúc khi hết
tim; điều kiện kết thúc là chó đầu tiên về đích. Chỉ hai chó của hai người đua.
Tốc độ lấy từ RaceUIController trong Scene6_Race, giống chó người chơi khi đấu
máy: nền 0.0055, đúng cộng 0.015 trong 10 giây theo cấu hình scene hiện tại.
Trả lời đúng tiếp sẽ làm mới thời gian boost, không cộng dồn tốc độ. Host phải
nhận cấu hình này trước khi bắt đầu đếm ngược.
Nếu cùng vượt đích, so sánh thời điểm nội suy trong bước mô phỏng Host; bằng
nhau trong sai số nhỏ thì hòa. Cả hai nhận cùng người thắng và thời gian trận.

Chơi lại ở màn kết quả trở về chọn chế độ để tìm trận mới. Nếu đối thủ thoát
giữa trận hoặc kết nối im lặng quá 12 giây, trận bị hủy và trở về chọn chế độ.
Thoát sau khi đã có kết quả không làm mất màn kết quả của người còn lại.

## Chạy trên hai máy Windows

1. Mở Build Profiles, chọn Windows. Danh sách scene đã có Scene5_PlayMode;
   nếu thiếu, chạy **Tools → PRU → Setup DogRacer LAN Scene**.
2. Build game và chép cùng bản build sang máy thứ hai. Có thể dùng Editor làm
   một người và bản build làm người còn lại.
3. Hai máy dùng cùng Wi-Fi/LAN. Cho phép Unity Editor hoặc file game `.exe`
   truy cập **Private networks** khi Windows Firewall hỏi. Không cần tắt Firewall.
4. Cả hai vào Hóa → Trắc nghiệm → chọn cùng chương/bài → PvP LAN.
5. Chọn hai chó khác nhau và cùng bấm Sẵn sàng. Host cần Internet để tải Sheet;
   dữ liệu trận đấu sau đó chạy trực tiếp trong LAN.
6. Kiểm tra hai câu hỏi khác nhau, tăng tốc khi đúng, và hai máy cùng dừng khi
   một chó về đích. Thử rút mạng hoặc đóng một bản game giữa trận.

Discovery dùng UDP **47778**; kết nối trận dùng một cổng TCP động do hệ điều
hành cấp. Nếu Firewall quản lý theo ứng dụng, cho phép đúng executable của game.
Wi-Fi khách hoặc mạng trường có AP/client isolation có thể chặn hai máy liên lạc
dù cùng SSID; dùng mạng riêng cho phép thiết bị giao tiếp trực tiếp.

## Cấu trúc

- `DogRaceLanSession`: bootstrap xuyên scene, discovery, TCP có phân khung JSON,
  khóa chó, bắt tay, heartbeat, luật đua phía Host và đồng bộ kết quả.
- `DogRaceLanSceneSetup`: tạo scene thật qua Unity Editor API; giữ nguyên scene
  đang mở. Scene có camera riêng; giao diện chọn chế độ được service vẽ runtime.
- `ChapterSelectUIController`: chèn màn chọn chế độ cho trắc nghiệm.
- `DogSelectUIController`: gửi yêu cầu chọn chó/sẵn sàng và hiển thị chó bị khóa.
- `RaceUIController`: hiển thị trạng thái Host, không chạy AI hoặc tự chấm PvP.
- `ResultsUIController`: kết quả 1v1, chơi lại và rời phòng.

Không thêm package mạng. TCP đảm bảo thứ tự và phân khung JSON xử lý cả gói bị
chia nhỏ/gộp; giới hạn bộ đệm để tránh tăng bộ nhớ không giới hạn. Socket được
poll trên main thread, không gọi Unity API từ background thread. Đây là chế độ
Windows/desktop LAN tin cậy, chưa hỗ trợ WebGL, Internet matchmaking, mã hóa,
chống Host gian lận, khôi phục Host hoặc tự nối lại trận bị ngắt.

## Kiểm thử tự động

Với UnitySkills đang chạy bypass và Editor đã thoát Play, dùng Node 18+:

```powershell
node Tools/test_dogracer_lan.mjs
node Tools/test_dogracer_lan.mjs --client
node Tools/test_dogracer_lan.mjs --disconnect
```

Bài thử dùng socket TCP/UDP thật với đối thủ mô phỏng trên cùng máy. Nó giữ
scene đang mở, vào Play từ scene chế độ, tăng tốc trận tạm thời khi kiểm thử và
thoát Play/khôi phục scene sau kiểm tra. Bài thử Host cần truy cập Sheet thật.
Đây không thay thế kiểm tra hai máy vật lý, Firewall và chất lượng Wi-Fi.

### Kết quả kiểm tra trong môi trường phát triển

- Unity biên dịch thành công, 0 lỗi C# (project còn các warning API cũ).
- Host với peer TCP thật: UDP discovery, chọn chó độc quyền, đếm ngược/start,
  câu hỏi khác nhau, chấm tại Host, từ chối đáp án gửi lặp; đã kiểm tra cả kết
  quả hòa và người chơi thứ hai thắng.
- Unity làm client với Host mô phỏng: tự tìm Host, gửi chọn chó/sẵn sàng,
  xác nhận scene đã tải, gửi đáp án và nhận kết quả thua từ Host.
- Đóng kết nối giữa trận: hủy phiên và quay về scene chọn chế độ.
- Đối thủ rời sau kết thúc: màn kết quả được giữ lại.
- Chưa kiểm tra hai máy vật lý hoặc bản Windows build.
