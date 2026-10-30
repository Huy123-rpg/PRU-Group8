# Kiểm tra chuyển scene các môn — 30/10/2026

## Màn chọn môn đang dùng

`Assets/Scenes/Scene2_MainMenu.unity` dùng `SelectableCard` và
`MainMenuUIController`. Các component `SubjectCard` trên ba thẻ đã bị disable.
Không nên bật đồng thời hai cơ chế xử lý click.

| Môn | Scene đích từ MainMenuUIController | Cách chuyển |
| --- | --- | --- |
| Hóa | Scene3_LessonType | NavigationManager.LoadScene → LoadSceneAsync, fade và lưu lịch sử Back |
| Lý | GameSelection; thiếu scene này thì 01_MainMenu | SceneManager.LoadScene đồng bộ |
| Sinh | ChapterList 1; thiếu scene này thì SinhHoc | SceneManager.LoadScene đồng bộ |

## Lỗi Hóa đã xác định

`MenuGame/Manager` chứa cả `GoogleSheetDataManager` và
`MainMenuUIController`. Khi đã có singleton dữ liệu từ màn trước,
`GoogleSheetDataManager.Awake` gọi `Destroy(gameObject)` trên bản trùng.
Cả `MainMenuUIController` bị xóa, nên click chỉ đổi hình thẻ mà không chuyển
scene. Việc chạy riêng `SubjectCard.TriggerSelect()` vẫn chuyển được scene
không chứng minh click UI đang hoạt động.

Bản sửa chỉ hủy component dữ liệu bị trùng, tạo service dữ liệu riêng ở root
nếu component ban đầu gắn chung UI, và reset singleton khi bắt đầu Play.
`NavigationManager` cũng được khởi tạo ở root trước khi scene chạy; bản trùng
chỉ hủy component để giữ các controller gắn chung. Khóa chuyển scene được đặt
trước fade và giữ đến hết fade; scene đích được kiểm tra trước khi che màn hình.

`MainMenuUIController.AutoBindComponents` trước đây lấy Button theo thứ tự
trong toàn Canvas, có thể gán BackButton thành bioButton. Bản sửa chỉ lấy Button
trên đúng thẻ môn.

## Luồng Hóa

Scene2_MainMenu → Scene3_LessonType (trắc nghiệm/tự luận)
→ Scene4_ChapterSelect (chương/bài) → Scene5_DogSelect
→ Scene6_Race → Scene7_Results.

Các controller tương ứng gọi NavigationManager; một số có fallback LoadScene
trực tiếp. Results cho chơi lại về DogSelect hoặc về MainMenu. Back dùng lịch
sử NavigationManager, nếu trống thì dùng mapping scene trước đó.

## Luồng Lý

GameSelection → Game 1: Assets/Scenes/Physics/Game1/01_MainMenu.unity.
Luồng học chính: 04_ChapterSelection → 05_LessonSelection
→ 06_GameModeSelect → 03_LevelSelection → 07_DuckShooting
→ 08_ResultScene/09_AnswerReview. Có các nhánh chọn nhân vật,
lý thuyết, đối kháng, thành tích, cài đặt và dashboard giáo viên.

GameSelection → Game 2: Assets/Scenes/Physics/Game2/02_CharacterSelection.unity
(fallback về Game2/01_MainMenu hoặc 06_Gameplay_Mechanics).

Rủi ro chưa sửa: PhysicsGame1Bootstrap phân loại controller chỉ bằng scene.name.
Game 2 có các scene trùng tên 01_MainMenu, 02_CharacterSelection,
04_ChapterSelection, 05_LessonSelection; chúng có thể bị gắn controller Game 1.
Các lệnh LoadScene bằng tên ngắn 01_MainMenu/02_CharacterSelection/
04_ChapterSelection/05_LessonSelection cũng không phân biệt hai thư mục.
Nên phân loại bootstrap và gọi load bằng đường dẫn đầy đủ khi hoàn thiện Game 2.

## Luồng Sinh

Nhánh đang được MainMenuUIController chọn:
ChapterList 1 → LessonList 1 → SinhScene, do BiologyMenuManager điều khiển.

Nhánh khác do MainMenuManager/SubjectCard dùng:
Assets/SinhHoc/Scenes/MainMenu.unity → ChapterSelect → LessonSelect
→ ModeSelect → SinhHoc (trắc nghiệm) hoặc EssayMaze (tự luận).
MainMenuController dùng đường dẫn Assets/SinhHoc/Scenes/<scene>.unity.
MainMenuManager còn có lựa chọn game sinh tồn và vượt chướng ngại vật;
nhánh vượt chướng ngại vật tải ChapterList 1.

Hai bộ menu hiện chưa thống nhất đích đến cho Sinh. SceneLoader.LoadBiology
cũng tải ChapterList 1, còn LoadPhysics tải thẳng 07_DuckShooting.
Các scene Chemistry/Chemistry.unity và Biology/Biology.unity có trong build
nhưng không phải đích chọn môn của MainMenuUIController.

## Build Settings

AutoSyncBuildScenes chạy khi reload script và thay toàn bộ danh sách build
bằng danh sách cố định. Trước bản sửa, danh sách này thiếu GameSelection và
MainMenu/ChapterSelect/LessonSelect/ModeSelect/EssayMaze của SinhHoc, dù các
scene tồn tại. Đã bổ sung các đường dẫn đó vào nguồn đồng bộ.

## Kiểm tra lại trong Unity

1. Play từ Scene1_Login, vào Scene2_MainMenu.
2. Bấm Hóa: phải vào Scene3_LessonType; chọn phương thức để vào ChapterSelect.
3. Back về chọn môn, chọn Hóa lần nữa để kiểm tra instance dữ liệu đã tồn tại.
4. Chọn Lý: phải vào GameSelection; kiểm tra hai lựa chọn Game 1/Game 2.
5. Chọn Sinh: theo menu hiện tại phải vào ChapterList 1.
6. Chạy Play lần hai với Domain Reload tắt và lặp lại các thao tác.

Kiểm tra này tập trung điều hướng; không thay thế kiểm thử gameplay, chấm điểm
hay gửi kết quả lên Google Sheets.

## Kết quả đã kiểm chứng

- Unity biên dịch hai assembly thành công, 0 lỗi C#; còn 84 warning của project.
- Đã tái hiện click Hóa trước sửa: thẻ đổi hình nhưng vẫn ở Scene2_MainMenu.
- Sau sửa, click chuột thật: Scene2_MainMenu → Hóa → Scene3_LessonType
  → Trắc nghiệm → Scene4_ChapterSelect, rồi Back hai lần về Scene2_MainMenu.
- Sau khi quay lại menu: click Lý mở GameSelection, Quay lại về Scene2_MainMenu,
  click Sinh mở ChapterList 1.
- AutoSyncBuildScenes.Sync đã chạy và đăng ký 54 scene.
- Chưa kiểm thử toàn bộ gameplay, các nhánh Game 2 Lý, nhánh SinhHoc riêng,
  đăng nhập thực tế hoặc build executable. Đã thoát Play Mode sau kiểm tra.
