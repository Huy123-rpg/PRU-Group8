# 🌿 GAME SINH HỌC — SPECIFICATION MỚI CHO AI AGENT

## 1. MỤC TIÊU VÀ NGUYÊN TẮC THỰC HIỆN

Đọc tài liệu này và kiểm tra codebase Unity thực tế trước khi sửa code.

Nhiệm vụ: **loại bỏ hệ thống gameplay cũ liên quan đến Player, MapManager và chướng ngại vật; xây dựng lại cơ chế chạy vô tận, sinh chướng ngại vật, sinh vật phẩm hồi máu và xuất hiện quái vật theo từng quãng đường.**

Không chỉ sửa tên script hoặc vô hiệu hóa tạm thời các component cũ. Phải loại bỏ các logic cũ gây xung đột và bảo đảm hệ thống mới hoạt động độc lập, nhất quán.

### Nguyên tắc bắt buộc

- Kiểm tra scene `SinhScene.unity`, prefab, script, Inspector references và các component liên quan trước khi chỉnh sửa.
- Xác định chính xác các object và script cũ cần loại bỏ, bao gồm các tham chiếu đến chúng trong scene và prefab.
- Không để script cũ và script mới cùng sinh chướng ngại vật, di chuyển map hoặc xử lý va chạm.
- Không xóa các module không thuộc phạm vi thay đổi nếu chúng vẫn cần thiết.
- Giữ lại hệ thống câu hỏi, chế độ trắc nghiệm/tự luận, AI chấm bài, dữ liệu câu hỏi, lưu lịch sử và xếp hạng nếu không có xung đột.
- Giữ nguyên các khóa `PlayerPrefs`, tên scene và các interface đang được những module khác sử dụng, trừ khi thực sự bắt buộc phải thay đổi.
- Không chỉ tạo code: phải cập nhật cấu hình scene, prefab, component và Inspector references để hệ thống chạy được.

## 2. LOẠI BỎ HỆ THỐNG CŨ

### 2.1. Player cũ

Loại bỏ toàn bộ hệ thống Player cũ đang điều khiển gameplay, bao gồm:

- Object Player cũ và các component gameplay gắn với nó.
- `PlayerMovement.cs` và `PlayerCollision.cs` nếu chúng thuộc hệ thống cũ cần thay thế.
- Các cơ chế va chạm, trừ máu, bất tử, chớp đỏ và xử lý vị trí được triển khai riêng trong hệ thống cũ.
- Những tham chiếu đến Player cũ trong các script quản lý map, spawn hoặc gameplay.

Nếu gameplay mới vẫn cần nhân vật để điều khiển hoặc nhận va chạm, hãy tạo hoặc cấu hình một Player mới với các component phù hợp. Không được để Player mới sử dụng đồng thời logic va chạm cũ.

**Lưu ý:** Không xóa asset hình ảnh, animation hoặc sprite dùng chung nếu chúng vẫn cần thiết cho nhân vật mới.

### 2.2. MapManager cũ

Loại bỏ hoàn toàn cơ chế quản lý chunk map cũ.

- Xóa object `MapManager` khỏi scene nếu không còn sử dụng.
- Loại bỏ script `MapManager.cs` và các logic tạo, quản lý, hủy chunk thuộc hệ thống cũ.
- Xóa các tham chiếu đến `MapManager` trong những script liên quan.
- Không giữ song song `MapManager` cũ và hệ thống cuộn nền mới.

Nền mới phải hoạt động độc lập với hệ thống chunk cũ.

### 2.3. Chướng ngại vật cũ

Loại bỏ các object chướng ngại vật cũ được đặt sẵn trong scene và logic sinh chướng ngại vật cũ.

- Xóa các instance chướng ngại vật được đặt thủ công trong `SinhScene`.
- Loại bỏ `ObstacleSpawner.cs` cũ nếu cần thay thế toàn bộ logic spawn.
- Thay thế `ObstacleDamage.cs` và `PlayerCollision.cs` cũ nếu chúng thực hiện cơ chế sát thương không còn phù hợp.
- Kiểm tra và xóa các prefab hoặc tham chiếu runtime cũ không còn được sử dụng.
- Không để các object cũ tiếp tục xuất hiện sau khi bắt đầu game.

Các asset hình ảnh chướng ngại vật có thể được tái sử dụng làm prefab mới nếu phù hợp. Yêu cầu loại bỏ là **hệ thống gameplay và instance cũ**, không phải xóa asset một cách máy móc.

## 3. HỆ THỐNG NỀN MỚI — CUỘN LIÊN TỤC TRONG CANVAS

### 3.1. Yêu cầu

Object `bg` là UI Image nằm trong Canvas. Giữ nền trong Canvas, không chuyển sang world-space và không sử dụng `MapManager` để tạo chunk.

Nền phải:

- Cuộn liên tục theo chiều dọc, tạo cảm giác nhân vật đang chạy.
- Nối liền các mép nền, không xuất hiện khoảng trống, đường nối hoặc hiện tượng nhấp nháy.
- Chạy vô hạn trong suốt quá trình chơi.
- Dừng khi game pause, mở quiz hoặc Game Over.
- Tiếp tục đúng trạng thái khi game được resume.
- Đồng bộ với tốc độ gameplay; không đặt tốc độ cuộn cứng độc lập với tốc độ game.

### 3.2. Cách triển khai

Tạo script mới, ví dụ `BackgroundScroller.cs`.

Có thể sử dụng hai UI Image nền giống nhau, đặt nối tiếp nhau theo trục dọc. Khi một ảnh đi hết vùng hiển thị, đưa nó về phía trước ảnh còn lại để tạo vòng lặp.

Có thể dùng phương pháp offset texture nếu sprite, material và cấu hình UI hỗ trợ phù hợp.

Ưu tiên phương án ổn định với UI Image hiện có. Không thay đổi toàn bộ giao diện chỉ để triển khai hiệu ứng cuộn.

Nếu sử dụng hai ảnh nền, phải tính toán đúng kích thước RectTransform, vị trí nối và khoảng dịch chuyển để không bị hở hoặc chồng mép.

## 4. SINH CHƯỚNG NGẠI VẬT NGẪU NHIÊN

### 4.1. Cơ chế spawn

Tạo hệ thống spawn mới, ví dụ `ObstacleSpawner.cs`.

- Chướng ngại vật được sinh ngẫu nhiên trong vùng đường chơi hợp lệ.
- Vị trí sinh phải nằm trong phạm vi hiển thị hoặc vùng gameplay được cấu hình.
- Khoảng cách giữa các chướng ngại vật phải đủ để người chơi có thể phản ứng và né tránh.
- Không sinh chướng ngại vật đè lên Player, quái vật hoặc vật phẩm hồi máu.
- Không để chướng ngại vật xuất hiện dồn dập khiến người chơi không thể vượt qua.
- Không đặt sẵn các instance chướng ngại vật trong scene để tạo cảm giác ngẫu nhiên.
- Có thể cấu hình danh sách prefab, tốc độ spawn và khoảng cách spawn thông qua Inspector.

Hệ thống phải hoạt động liên tục và có thể mở rộng về số lượng loại chướng ngại vật.

### 4.2. Sát thương khi va chạm

Khi Player chạm vào chướng ngại vật hợp lệ:

1. Trừ chính xác **20 HP**.
2. Cập nhật thanh máu ngay lập tức.
3. Hủy hoặc vô hiệu hóa chướng ngại vật đã va chạm để không bị tính sát thương lặp.
4. Không trừ máu liên tục trong nhiều frame do cùng một lần va chạm.
5. Nếu HP về 0, kích hoạt Game Over.
6. Không cho phép HP âm hoặc vượt quá giới hạn quy định.

Có thể giữ cơ chế bất tử tạm thời hoặc hiệu ứng chớp đỏ nếu vẫn phù hợp, nhưng đây là cơ chế bảo vệ bổ sung; nó không được làm sai mức sát thương 20 HP của mỗi lần va chạm hợp lệ.

## 5. SINH VẬT PHẨM HỒI MÁU `hoimau`

### 5.1. Cơ chế xuất hiện

Tạo hệ thống sinh vật phẩm hồi máu ngẫu nhiên, sử dụng object hoặc prefab `hoimau`.

- `hoimau` là vật phẩm trong gameplay, được sinh ra ngẫu nhiên trong vùng đường chơi.
- Không để icon hồi máu trên UI bị hiểu nhầm thành vật phẩm có thể nhặt.
- Vật phẩm xuất hiện độc lập với chướng ngại vật nhưng phải tuân thủ quy tắc tỷ lệ.
- Không sinh vật phẩm ở vị trí không thể tiếp cận hoặc nằm ngoài đường chạy.
- Không để vật phẩm xuất hiện liên tục hoặc quá dày đặc.
- Mỗi vật phẩm chỉ được nhặt một lần.

### 5.2. Tỷ lệ sinh

Quy định mục tiêu:

**Trung bình 10 chướng ngại vật được sinh ra thì có 1 vật phẩm hồi máu.**

Tương đương tỷ lệ mục tiêu khoảng 1 vật phẩm hồi máu trên 10 chướng ngại vật.

Không được sinh một vật phẩm hồi máu sau mỗi chướng ngại vật. Không sử dụng cơ chế spawn hồi máu độc lập với số lượng chướng ngại vật nếu điều đó làm sai tỷ lệ mục tiêu.

Ưu tiên đếm số chướng ngại vật đã spawn và thực hiện lượt kiểm tra sinh hồi máu theo từng nhóm 10 chướng ngại vật. Có thể dùng xác suất ngẫu nhiên trong từng nhóm để tránh vị trí xuất hiện luôn cố định, nhưng phải bảo đảm tỷ lệ dài hạn gần mức 1:10.

Nếu không có vị trí hợp lệ, hệ thống có thể trì hoãn spawn đến khi tìm được vị trí phù hợp.

### 5.3. Hiệu ứng nhặt hồi máu

Khi Player chạm vào `hoimau`:

1. Cộng **30 HP**.
2. Cập nhật thanh máu ngay lập tức.
3. Hủy vật phẩm sau khi nhặt.
4. Không cho phép nhặt cùng một vật phẩm nhiều lần.
5. Không cho HP vượt quá Max HP.

Ví dụ:

- HP = 40 → nhặt hồi máu → HP = 70.
- HP = 80 → nhặt hồi máu → HP = 100 nếu Max HP là 100.
- HP = 95 → nhặt hồi máu → HP vẫn bằng 100.
- HP = 100 → nhặt hồi máu → HP vẫn bằng 100.

## 6. QUẢN LÝ HP, ELO VÀ UI

### 6.1. Quy tắc chỉ số

Sử dụng một nguồn dữ liệu trạng thái gameplay thống nhất để quản lý HP, Max HP và Elo/điểm tích lũy. Các script không được tự quản lý những bản sao chỉ số độc lập.

| Sự kiện                 | Thay đổi HP | Thay đổi Elo |
| ----------------------- | ----------: | -----------: |
| Trả lời đúng            |         +20 |          +10 |
| Trả lời sai             |         -10 |            0 |
| Va chạm chướng ngại vật |         -20 |            0 |
| Nhặt `hoimau`           |         +30 |            0 |

Các thay đổi HP được giới hạn trong khoảng từ 0 đến Max HP.

Elo không bị trừ khi trả lời sai hoặc va chạm chướng ngại vật. Không tự ý đặt Elo về 0 khi HP giảm.

### 6.2. Tích hợp với QuizManager

Cập nhật `QuizManager.cs` để áp dụng đúng quy tắc mới.

- Trả lời đúng: cộng 20 HP và 10 Elo.
- Trả lời sai: trừ 10 HP, không cộng hoặc trừ Elo.
- Hết giờ câu trắc nghiệm: xử lý như trả lời sai.
- Câu trả lời tự luận chỉ được tính đúng hoặc sai sau khi có kết quả chấm hợp lệ.
- Không cộng thưởng nhiều lần khi người chơi nhấn nút nhiều lần hoặc nhận nhiều callback cho cùng một câu hỏi.
- Khi HP về 0, kích hoạt Game Over theo một luồng thống nhất.

Không được giữ các giá trị thưởng cũ như +10 Elo, +20% HP hoặc sát thương theo phần trăm.

### 6.3. Thanh máu và điểm số

Các object UI hiện có có thể bao gồm `thanhmau`, `thanhmau (1)` và `score`. Agent phải kiểm tra vai trò thực tế của từng object trước khi gán.

Yêu cầu:

- Thanh máu phản ánh đúng HP hiện tại.
- Max HP được cấu hình rõ ràng, mặc định 100 nếu không có yêu cầu khác.
- Nếu có nhiều thanh máu cùng biểu diễn HP, tất cả phải hiển thị cùng một giá trị.
- Elo/điểm số trên UI phải khớp với giá trị thực tế của hệ thống quản lý điểm.
- Cập nhật ngay sau mỗi sự kiện cộng hoặc trừ điểm, không chờ đến khi mở quiz tiếp theo.
- Khi bắt đầu lượt chơi mới, khởi tạo trạng thái theo đúng quy tắc reset game.
- Không cập nhật UI từ những giá trị tạm hoặc biến cục bộ không phản ánh trạng thái thực tế.

Nếu giao diện hiện có có cả Elo và điểm câu hỏi, phải xác định rõ ý nghĩa từng trường, tránh hiển thị nhầm hoặc cộng điểm hai lần.

## 7. QUÁI VẬT THEO TỪNG QUÃNG ĐƯỜNG

### 7.1. Quy tắc xuất hiện

Loại bỏ cơ chế spawn Boss cũ nếu đang sử dụng các instance đặt sẵn trong scene hoặc các mốc không còn phù hợp.

Tạo hệ thống quản lý quái vật theo quãng đường, ví dụ `MonsterSpawner.cs` hoặc tích hợp vào hệ thống gameplay mới nếu kiến trúc hiện tại phù hợp.

- Chia đường chạy thành các quãng liên tiếp.
- Mỗi quãng chỉ được gặp **một con quái vật**.
- Quái vật xuất hiện theo tiến độ đường chạy, không spawn ngẫu nhiên không kiểm soát.
- Không sinh hai quái vật cùng thuộc một quãng.
- Không để quái vật cũ tiếp tục kích hoạt quiz sau khi đã bị xử lý.
- Không để một quái vật mở nhiều quiz.
- Sau khi hoàn thành quãng hiện tại, gameplay tiếp tục tới quãng tiếp theo theo đúng trạng thái.

Có thể cấu hình độ dài quãng đường và danh sách prefab quái vật trong Inspector.

### 7.2. Khi gặp quái vật

Giữ lại hệ thống quiz hiện có nếu phù hợp:

- Khi quái vật đến đúng vị trí kích hoạt đã cấu hình, mở quiz.
- Dừng gameplay và nền khi quiz được mở.
- Hiển thị đúng panel theo chế độ đã chọn: `paneltracno` hoặc `paneltuluan`.
- Khi hoàn thành quiz và xử lý quái vật thành công, đóng quiz, xử lý quái vật hiện tại và tiếp tục chạy.
- Không được tạo thêm quái vật cho quãng hiện tại sau khi quiz đóng.
- Không dùng timer chờ cứng để xác định thời điểm mở quiz nếu tốc độ gameplay có thể thay đổi.

Sử dụng một trạng thái hoặc mã quãng đường để đánh dấu quãng đã sinh quái vật và quãng đã hoàn thành. Việc kiểm tra này phải ngăn spawn trùng ngay cả khi scene bị pause/resume.

Không thay đổi quy tắc số câu hỏi mỗi quái vật nếu chưa có yêu cầu khác; mặc định giữ cấu hình quiz hiện tại là 5 câu/quái vật nếu hệ thống đang dùng giá trị đó.

## 8. TÍCH HỢP VỚI HỆ THỐNG CŨ ĐƯỢC GIỮ LẠI

Các chức năng sau vẫn cần hoạt động sau khi thay thế gameplay:

- Chọn chương, bài học và chế độ chơi từ menu Sinh học.
- Trắc nghiệm và tự luận.
- Đồng hồ 30 giây/câu cho trắc nghiệm.
- Tự luận không giới hạn thời gian.
- AI chấm bài bằng Gemini và cơ chế fallback từ khóa.
- Upload ảnh bài làm nếu chức năng này đã được triển khai.
- Popup Game Over, chơi lại và thoát về menu.
- Lưu điểm cao, lịch sử điểm và kết quả.
- Nhạc nền và hiệu ứng âm thanh nếu đã triển khai.

Không viết lại các module này nếu chỉ cần sửa interface để tích hợp với gameplay mới.

Giữ nguyên các khóa `PlayerPrefs` hiện có, bao gồm `Sinh_SelectedChapter`, `Sinh_SelectedLesson`, `Sinh_QuizMode`, `Sinh_HighScore`, `Sinh_LastScore`, `Sinh_ScoreHistory`, `Sinh_CurrentUsername` và `Gemini_ApiKey`.

Scene gameplay vẫn là `SinhScene`; menu Sinh học vẫn là `LessonList 1`.

## 9. KIẾN TRÚC VÀ QUY ƯỚC CODE

Ưu tiên các thành phần có trách nhiệm rõ ràng:

- `BackgroundScroller.cs`: cuộn nền UI vô hạn.
- `ObstacleSpawner.cs`: sinh chướng ngại vật mới.
- `HealthPickupSpawner.cs`: quản lý tỷ lệ sinh `hoimau`.
- `MonsterSpawner.cs`: quản lý quái vật theo quãng.
- `GameStateManager.cs` hoặc một module quản lý trạng thái tương đương: HP, Max HP, Elo, cập nhật chỉ số và Game Over.
- `QuizManager.cs`: giữ logic câu hỏi và gọi hệ thống trạng thái khi có kết quả đúng/sai.
- Các component va chạm mới: xử lý sát thương và nhặt vật phẩm, bảo đảm mỗi sự kiện chỉ được áp dụng một lần.

Đây là kiến trúc tham khảo, không bắt buộc tạo đủ các file nếu codebase đã có module phù hợp. Không tạo nhiều singleton quản lý cùng một trạng thái.

Yêu cầu kỹ thuật:

- Unity 2021+ và C# tương thích với phiên bản dự án.
- Sử dụng đúng Canvas, RectTransform, UI Image và các component UI hiện có.
- Cấu hình prefab, collider, tag/layer và Inspector references đầy đủ.
- Không để `NullReferenceException` khi khởi động scene.
- Không để các script cũ và mới cùng xử lý một sự kiện.
- Có cơ chế chống spawn trùng và chống xử lý va chạm nhiều lần.
- Có thể dọn các object gameplay đã đi qua để hạn chế tăng bộ nhớ vô hạn.
- Không hardcode API key.
- Giữ comment tiếng Việt và code dễ bảo trì.

## 10. QUY TRÌNH THỰC HIỆN

Agent phải làm theo thứ tự sau:

### Bước 1 — Kiểm tra codebase

Đọc các script, prefab và scene thực tế. Xác định dependencies, các object được tham chiếu trong Inspector và những module phải giữ lại.

### Bước 2 — Loại bỏ hệ thống cũ

Gỡ các object và component gameplay cũ thuộc Player, MapManager và chướng ngại vật. Dọn các tham chiếu bị hỏng. Không xóa những tài nguyên UI hoặc asset vẫn cần thiết.

### Bước 3 — Xây dựng nền cuộn

Hoàn thiện `BackgroundScroller` với nền UI trong Canvas. Kiểm tra loop vô hạn, điểm nối và trạng thái pause/resume.

### Bước 4 — Xây dựng spawn mới

Triển khai sinh chướng ngại vật, vật phẩm hồi máu và quái vật theo từng quãng. Bảo đảm không sinh trùng và không tạo ra các vị trí không thể tiếp cận.

### Bước 5 — Tích hợp gameplay và quiz

Đồng bộ HP, Elo, va chạm, hồi máu và kết quả quiz. Bảo đảm các giá trị mới thay thế hoàn toàn các giá trị cũ.

### Bước 6 — Cập nhật UI và cấu hình scene

Gán các tham chiếu mới, kiểm tra thanh máu, điểm số, panel quiz và Game Over. Không để UI hiển thị các giá trị cũ.

### Bước 7 — Kiểm thử

Chạy Play Mode, kiểm tra Console, test các trường hợp biên và sửa lỗi phát sinh. Không kết luận hoàn thành chỉ dựa vào việc code biên dịch thành công.

## 11. DEFINITION OF DONE — TIÊU CHÍ HOÀN THÀNH

Chỉ được coi là hoàn thành khi đáp ứng toàn bộ các tiêu chí sau:

- [ ] Hệ thống Player cũ, MapManager cũ và logic chướng ngại vật cũ đã được loại bỏ đúng phạm vi.
- [ ] Không có script cũ tiếp tục spawn hoặc xử lý va chạm song song với hệ thống mới.
- [ ] Nền `bg` trong Canvas cuộn liên tục, không hở mép, không nhấp nháy và dừng đúng lúc pause.
- [ ] Chướng ngại vật được sinh ngẫu nhiên trong vùng hợp lệ.
- [ ] Va chạm chướng ngại vật trừ chính xác 20 HP mỗi lần va chạm hợp lệ.
- [ ] `hoimau` được sinh ngẫu nhiên với tỷ lệ mục tiêu 1 vật phẩm trên 10 chướng ngại vật.
- [ ] Nhặt `hoimau` cộng chính xác 30 HP, không vượt Max HP và không thể nhặt nhiều lần.
- [ ] Trả lời đúng cộng 20 HP và 10 Elo.
- [ ] Trả lời sai trừ 10 HP và không trừ Elo.
- [ ] Hết giờ trắc nghiệm xử lý như trả lời sai.
- [ ] Thanh máu và Elo trên UI luôn đồng bộ với trạng thái gameplay.
- [ ] Quái vật xuất hiện theo từng quãng, mỗi quãng chỉ có một con.
- [ ] Mỗi quái vật chỉ mở một phiên quiz; sau khi xử lý thành công, game tiếp tục đúng tiến độ.
- [ ] Game Over hoạt động khi HP bằng 0; chơi lại không giữ các object hoặc trạng thái gameplay cũ ngoài ý muốn.
- [ ] Các chức năng quiz, AI, lưu điểm và chuyển scene được giữ lại vẫn hoạt động.
- [ ] Không có lỗi compile hoặc lỗi runtime nghiêm trọng trong các luồng gameplay chính.

## 12. YÊU CẦU BÁO CÁO SAU KHI THỰC HIỆN

Sau khi hoàn thành, agent phải báo cáo:

1. Những script và object cũ đã xóa hoặc thay thế.
2. Những script mới đã tạo và nhiệm vụ của từng script.
3. Các thay đổi trong `SinhScene`, prefab, collider, tag/layer và Inspector references.
4. Cách xác minh công thức HP/Elo và tỷ lệ spawn hồi máu.
5. Kết quả kiểm thử Play Mode, các lỗi còn tồn tại và những phần chưa thể kiểm chứng.

**Không được tự tuyên bố đã kiểm thử thành công nếu chưa thực sự chạy được Play Mode.** Nếu môi trường không thể mở Unity hoặc chạy test, hãy nêu rõ phần đã kiểm tra bằng code và phần cần người dùng xác minh thủ công.

Ưu tiên cuối cùng: gameplay mới phải chạy ổn định, đúng các con số đã quy định, UI chính xác và không còn xung đột với hệ thống cũ.

# SPEC BỔ SUNG — FIX PLAYER MOVEMENT, UI VÀ BACKGROUND TRONG UNITY

## 1. MỤC TIÊU

Kiểm tra và sửa trực tiếp scene `SinhScene` cùng các script, prefab và component liên quan.

Các lỗi hiện tại:

1. Nhân vật `nv` nằm trong Canvas nhưng chưa thể di chuyển sang trái/phải.
2. Xuất hiện các button/UI thừa do agent tự tạo từ module cũ, ví dụ `CHỌN BÀI` và `ĐIỂM: 0` như trong ảnh.
3. Hai phần nền `bg` đang ghép lệch, không cân đối và xuất hiện đường nối ngang rất rõ giữa hai ảnh.

**Ưu tiên sửa đúng cấu trúc scene hiện tại, không tạo thêm hệ thống UI hoặc movement mới nếu có thể tận dụng những thành phần đang tồn tại.**

## 2. FIX DI CHUYỂN NHÂN VẬT `nv`

### 2.1. Yêu cầu chức năng

Nhân vật `nv` phải di chuyển được sang trái và phải trong vùng đường chạy.

- Nhấn phím `A` hoặc `←`: di chuyển sang trái.
- Nhấn phím `D` hoặc `→`: di chuyển sang phải.
- Chỉ di chuyển theo trục ngang; không cho nhân vật tự chạy ra ngoài vùng đường.
- Có thể giữ phím để tiếp tục di chuyển.
- Không di chuyển khi game đang pause, Game Over hoặc đang mở quiz.
- Khi resume game, điều khiển hoạt động lại bình thường.
- Không làm nhân vật di chuyển theo camera hoặc bị nền kéo đi ngoài ý muốn.

Nếu dự án hỗ trợ điều khiển bằng cảm ứng hoặc joystick thì giữ lại chức năng đó nếu đang được sử dụng.

### 2.2. Kiểm tra nguyên nhân trước khi sửa

Không được chỉ thêm script mới rồi giả định sẽ hoạt động. Agent phải kiểm tra lần lượt:

1. `nv` là UI object trong Canvas hay GameObject trong world-space.
2. `nv` có đúng component điều khiển và component đó đã được gắn vào đúng object chưa.
3. Script movement có đang tham chiếu đến Player cũ hoặc object đã bị xóa không.
4. Dự án đang dùng Input System mới, Input Manager cũ hay cấu hình Both.
5. `Active Input Handling` trong Player Settings có tương thích với cách đọc input trong code không.
6. Canvas đang dùng `Screen Space - Overlay`, `Screen Space - Camera` hay `World Space`.
7. Có script khác liên tục ghi đè `RectTransform.anchoredPosition`, `Transform.position` hoặc vị trí của `nv` không.
8. Có component Animator, Rigidbody hoặc script tự động di chuyển đang xung đột không.

### 2.3. Cách triển khai

Vì `nv` đang nằm trong Canvas, nếu đây là UI object thì ưu tiên điều khiển bằng `RectTransform.anchoredPosition` trong hệ tọa độ của parent phù hợp.

- Tạo hoặc sửa `PlayerMovementUI.cs` nếu cần.
- Không sử dụng tọa độ world-space trực tiếp để điều khiển một UI object nếu việc đó gây lệch vị trí.
- Nếu sử dụng Input System mới, đọc input đúng theo cấu hình hiện tại của dự án.
- Tốc độ di chuyển phải cấu hình được trong Inspector.
- Dùng `Time.unscaledDeltaTime` chỉ khi có chủ đích cho phép UI di chuyển lúc `Time.timeScale = 0`; mặc định nhân vật phải dừng khi gameplay pause.
- Xác định giới hạn trái/phải dựa trên vùng đường chạy thực tế, không dùng các giá trị cố định khiến nhân vật chạy ra ngoài đường khi độ phân giải thay đổi.
- Đảm bảo nhân vật không bị di chuyển bởi hai script cùng lúc.

Nếu `nv` không phải UI object thực sự mà chỉ là object khác nằm dưới một Canvas, hãy sử dụng phương pháp điều khiển phù hợp với hệ tọa độ thực tế sau khi kiểm tra.

### 2.4. Tiêu chí hoàn thành

- [ ] Nhấn `A` hoặc `←`, nhân vật di chuyển sang trái.
- [ ] Nhấn `D` hoặc `→`, nhân vật di chuyển sang phải.
- [ ] Giữ phím, nhân vật tiếp tục di chuyển mượt.
- [ ] Nhân vật không vượt khỏi giới hạn đường chạy.
- [ ] Không bị script khác kéo về vị trí cũ.
- [ ] Pause hoặc mở quiz thì nhân vật không di chuyển.
- [ ] Resume thì điều khiển hoạt động trở lại.
- [ ] Hoạt động ở độ phân giải và tỷ lệ hiển thị khác nhau.

## 3. XÓA CÁC BUTTON/UI THỪA DO AGENT TỰ SINH

### 3.1. Những thành phần cần xử lý

Trong ảnh hiện tại có các thành phần UI thừa, bao gồm:

- Button `CHỌN BÀI` ở góc trên bên trái.
- Thành phần `ĐIỂM: 0` ở góc trên bên phải.
- Các button, panel, label hoặc HUD khác do agent tự sinh từ module cũ mà không thuộc thiết kế gameplay đã được duyệt.

Đây là yêu cầu loại bỏ các UI thừa được tự động tạo thêm, không phải xóa toàn bộ UI của game.

### 3.2. Quy tắc xử lý

- Kiểm tra toàn bộ hierarchy của Canvas trong `SinhScene`.
- Xác định object nào thuộc giao diện cũ, object nào thuộc giao diện gốc.
- Xóa các instance UI thừa khỏi scene và prefab nếu chúng thực sự được tạo ra cho module cũ và không còn sử dụng.
- Xóa script hoặc listener chỉ phục vụ những UI đã bị loại bỏ, nếu không còn dependency nào khác.
- Kiểm tra các script `Start()`, `Awake()` hoặc `OnEnable()` có tự tạo button/HUD mỗi khi scene chạy không.
- Loại bỏ logic tự sinh UI trùng lặp, không chỉ ẩn object bằng `SetActive(false)`.
- Không tạo thêm button thay thế để bù vào những thành phần đã xóa.

**Không được xóa nhầm** thanh máu, thanh điểm Elo chính thức, panel quiz, panel Game Over, pause, resume, chơi lại, thoát hoặc các nút âm thanh đang được thiết kế sử dụng.

Nếu `CHỌN BÀI` là nút điều hướng không thuộc gameplay và không được yêu cầu giữ lại trên màn hình chơi, hãy loại bỏ khỏi `SinhScene`. Không xóa chức năng chọn bài trong scene menu.

Nếu `ĐIỂM: 0` là HUD tự sinh từ module cũ, hãy loại bỏ HUD đó. Nếu điểm số vẫn cần hiển thị, chỉ giữ thành phần điểm số chính thức đã có trong thiết kế, không tạo thêm một HUD mới.

### 3.3. Tiêu chí hoàn thành

- [ ] Không còn button `CHỌN BÀI` thừa trong màn chơi.
- [ ] Không còn label `ĐIỂM: 0` thừa do agent tự sinh.
- [ ] Không còn UI trùng lặp sau khi restart hoặc load lại scene.
- [ ] Các chức năng quiz, thanh máu, pause, chơi lại và thoát vẫn hoạt động.
- [ ] Không có lỗi tham chiếu do xóa nhầm UI object.

## 4. FIX NỀN `bg` BỊ GHÉP LỆCH

### 4.1. Hiện trạng

Trong ảnh, đường nối ngang giữa hai phần nền rất rõ. Các ô gạch không tạo thành một mặt đường liên tục, khiến hai ảnh nền trông lệch nhau.

Agent phải sửa trực tiếp cách bố trí và cuộn của các UI Image `bg` trong Canvas.

### 4.2. Yêu cầu bắt buộc

- Giữ nền là UI Image trong Canvas.
- Hai ảnh nền phải có kích thước hiển thị tương thích và tỷ lệ đồng nhất.
- Các mép nối phải khớp chính xác, không xuất hiện đường cắt ngang bất thường.
- Các hàng và cột gạch phải tiếp tục tự nhiên qua điểm nối.
- Không kéo giãn một ảnh theo cách làm ô gạch to nhỏ khác nhau giữa hai ảnh.
- Không để ảnh thứ hai bị lệch trái/phải hoặc lệch tỷ lệ so với ảnh thứ nhất.
- Không có khoảng trống hoặc vùng nền chồng lấn gây ra đường nối rõ rệt.
- Nền tiếp tục cuộn vô hạn mà không bị giật khi đổi vị trí hai ảnh.

### 4.3. Các bước sửa

1. Kiểm tra `RectTransform` của từng object `bg`, gồm Anchor, Pivot, Width, Height, Scale và vị trí.
2. Kiểm tra hai object có thực sự sử dụng cùng sprite, cùng tỷ lệ và cùng cách hiển thị hay không.
3. Kiểm tra Sprite Import Settings, `Image Type`, `Preserve Aspect` và các thiết lập liên quan.
4. Xác định chiều cao thực tế của vùng nền hiển thị và vị trí nối của hai ảnh.
5. Nếu sử dụng hai ảnh cuộn luân phiên, đặt chúng nối tiếp nhau theo đúng chiều dọc và đưa ảnh đã đi hết màn hình về đúng vị trí tiếp theo.
6. Nếu đường nối vẫn tồn tại do nội dung sprite, xác định đường nối thuộc asset hay do cách bố trí. Không che lỗi bằng cách tăng kích thước ảnh tùy tiện.
7. Chỉ chỉnh sửa asset hoặc cắt lại sprite nếu nguyên nhân thực sự nằm trong hình ảnh nguồn.
8. Kiểm tra lại ở tỷ lệ Game View hiện tại và ít nhất một độ phân giải khác.

### 4.4. Không được làm

- Không chuyển toàn bộ nền sang world-space để né lỗi.
- Không xóa nền hiện có rồi thay bằng một hình khác không được yêu cầu.
- Không chỉ di chuyển một ảnh bằng mắt mà không tính toán kích thước và điểm nối.
- Không tăng Scale tùy tiện làm sai tỷ lệ ô gạch.
- Không để script cuộn nền cũ và mới chạy đồng thời.

### 4.5. Tiêu chí hoàn thành

- [ ] Hai ảnh `bg` được nối chính xác.
- [ ] Không có đường nối ngang gây lệch ô gạch.
- [ ] Nền không bị méo hoặc sai tỷ lệ.
- [ ] Cuộn liên tục, không nhấp nháy hoặc xuất hiện khoảng trống.
- [ ] Dừng và tiếp tục đúng khi pause/resume gameplay.

## 5. THỨ TỰ THỰC HIỆN

1. Kiểm tra hierarchy và các script thực tế trong `SinhScene`.
2. Sửa movement của `nv`, xác nhận điều khiển hoạt động trước khi tiếp tục.
3. Xác định và xóa UI thừa, đồng thời loại bỏ logic tự sinh UI cũ.
4. Sửa bố trí, kích thước và cơ chế cuộn của `bg`.
5. Kiểm tra tương tác với hệ thống gameplay mới: spawn chướng ngại vật, `hoimau`, quái vật, quiz và HP/Elo.
6. Chạy Play Mode và sửa lỗi phát sinh.

Không triển khai thêm các tính năng ngoài phạm vi chỉ để thay thế những thành phần đã xóa.

## 6. BÁO CÁO SAU KHI SỬA

Agent phải nêu rõ:

- Script nào đã sửa hoặc tạo mới để điều khiển `nv`.
- Nguyên nhân thực tế khiến `nv` không di chuyển được.
- Những UI object nào đã bị xóa và tại sao.
- Nguyên nhân nền `bg` bị lệch và các giá trị RectTransform đã điều chỉnh.
- Kết quả kiểm thử movement, UI và background.
- Các lỗi còn tồn tại hoặc phần chưa kiểm tra được.

Không được báo cáo hoàn thành chỉ vì code compile thành công. Phải xác minh trong Play Mode; nếu không thể chạy Unity, cần ghi rõ những phần chưa được xác minh.
