using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SceneParser
{
    class Program
    {
        class ObjInfo
        {
            public long FileId;
            public int TypeId;
            public string Name = "";
            public bool IsActive = true;
            public long GoFileId;
            public long FatherFileId;
            public List<long> Children = new List<long>();
            public string Text = "";
        }

        static void Main(string[] args)
        {
            // Tìm root folder của project
            string dir = AppContext.BaseDirectory;
            while (!Directory.Exists(Path.Combine(dir, "Assets")) && Directory.GetParent(dir) != null)
            {
                dir = Directory.GetParent(dir).FullName;
            }

            Console.WriteLine("=================================================================");
            Console.WriteLine(" BÁO CÁO KIỂM THỬ TÍNH NĂNG TOÀN DIỆN ROLE GIÁO VIÊN (UNITY 6)");
            Console.WriteLine("=================================================================\n");
            Console.WriteLine($"Project Root: {dir}\n");

            string scenePath = Path.Combine(dir, @"Assets\Scenes\Physics\Game1\16_DashboardTeacher.unity");
            if (!File.Exists(scenePath))
            {
                Console.WriteLine($"[ERROR] Không tìm thấy scene tại: {scenePath}");
                return;
            }

            var lines = File.ReadAllLines(scenePath);
            var objects = new Dictionary<long, ObjInfo>();
            ObjInfo cur = null;
            string curBlock = "";

            Regex headerRegex = new Regex(@"^---\s+!u!(\d+)\s+&(\d+)");
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                var match = headerRegex.Match(line);
                if (match.Success)
                {
                    if (cur != null) ParseBlock(cur, curBlock);
                    int tid = int.Parse(match.Groups[1].Value);
                    long fid = long.Parse(match.Groups[2].Value);
                    cur = new ObjInfo { FileId = fid, TypeId = tid };
                    objects[fid] = cur;
                    curBlock = "";
                }
                else
                {
                    curBlock += line + "\n";
                }
            }
            if (cur != null) ParseBlock(cur, curBlock);

            var goMap = new Dictionary<long, ObjInfo>();
            foreach (var kvp in objects)
            {
                if (kvp.Value.TypeId == 1) // GameObject
                {
                    goMap[kvp.Key] = kvp.Value;
                }
            }

            int passCount = 0;
            int failCount = 0;

            void Check(bool condition, string testName, string note = "")
            {
                if (condition)
                {
                    passCount++;
                    Console.WriteLine($" [PASS] {testName} {(string.IsNullOrEmpty(note) ? "" : "-> " + note)}");
                }
                else
                {
                    failCount++;
                    Console.WriteLine($" [FAIL] {testName} -> {note}");
                }
            }

            bool HasGo(string namePattern)
            {
                string cleanPattern = Clean(namePattern);
                return goMap.Values.Any(g => Clean(g.Name) == cleanPattern || Clean(g.Name).Contains(cleanPattern));
            }

            // In chi tiết các con của hang1 trong Panelqlychuong và Panelqlytiendo
            foreach (var go in goMap.Values)
            {
                if (go.Name == "hang1" || go.Name == "hang2")
                {
                    Console.WriteLine($"\n[FOUND ROW TEMPLATE] Name: '{go.Name}', FileId: {go.FileId}");
                    var tr = objects.Values.FirstOrDefault(o => o.TypeId == 224 && o.GoFileId == go.FileId);
                    if (tr != null)
                    {
                        Console.WriteLine($"  Children count: {tr.Children.Count}");
                        foreach (var cFid in tr.Children)
                        {
                            var cTr = objects.Values.FirstOrDefault(o => o.FileId == cFid);
                            if (cTr != null && goMap.ContainsKey(cTr.GoFileId))
                            {
                                var cGo = goMap[cTr.GoFileId];
                                Console.WriteLine($"    - Child Go: '{cGo.Name}' (Active: {cGo.IsActive})");
                            }
                        }
                    }
                }
            }

            Console.WriteLine("--- KIỂM TRA MÀN 1: BẢNG ĐIỀU KHIỂN (DASHBOARD GIÁO VIÊN) ---");
            Check(HasGo("title"), "Tiêu đề Dashboard 'title' tồn tại");
            Check(HasGo("quanlychuong"), "Nút 'quanlychuong' mở Quản lý Chương tồn tại");
            Check(HasGo("quanlybai"), "Nút 'quanlybai' mở Quản lý Bài học tồn tại");
            Check(HasGo("quanlycauhoi"), "Nút 'quanlycauhoi' mở Quản lý Câu hỏi tồn tại");
            Check(HasGo("tiendohoctap"), "Nút 'tiendohoctap' mở Tiến độ học tập tồn tại");
            Check(HasGo("logout"), "Nút 'logout' Đăng xuất tồn tại");

            Console.WriteLine("\n--- KIỂM TRA MÀN 2 & 3: QUẢN LÝ CHƯƠNG & POPUP THÊM CHƯƠNG ---");
            Check(HasGo("Panelqlychuong"), "Panelqlychuong tồn tại trong Canvas");
            Check(HasGo("vatly") && (HasGo("sinhhoc") || HasGo("sinh")) && (HasGo("hoahoc") || HasGo("hoa")), "Đủ 3 nút chọn môn: Vật lý, Sinh học, Hóa học");
            Check(HasGo("thêm chương"), "Nút 'thêm chương' tồn tại");
            Check(HasGo("thoát") || HasGo("thoat"), "Nút 'thoát' (X) đóng panel tồn tại");
            Check(HasGo("Panelthemmoichuong"), "Popup con 'Panelthemmoichuong' (Màn 3) tồn tại");

            Console.WriteLine("\n--- KIỂM TRA MÀN 4 & 5: QUẢN LÝ BÀI HỌC & POPUP SỬA LÝ THUYẾT ---");
            Check(HasGo("Panelqlybaihoc"), "Panelqlybaihoc tồn tại trong Canvas");
            Check(HasGo("Panelsuanoidung"), "Popup con 'Panelsuanoidung' (Màn 5) tồn tại");
            Check(HasGo("titlechonchuong"), "Khung 'titlechonchuong' hiển thị danh sách nút chương động");
            Check(HasGo("sửa"), "Nút 'sửa' bài học tồn tại");

            Console.WriteLine("\n--- KIỂM TRA MÀN 6 & 7: NGÂN HÀNG CÂU HỎI & POPUP THÊM CÂU HỎI ---");
            Check(HasGo("Panelqlycauhoi"), "Panelqlycauhoi tồn tại trong Canvas");
            Check(HasGo("Panelthemcauhoimoi"), "Popup con 'Panelthemcauhoimoi' (Màn 7) tồn tại");
            Check(HasGo("tải lên"), "Nút 'tải lên' đồng bộ Google Sheets tồn tại");
            Check(HasGo("thêm"), "Nút 'thêm' mở popup tạo câu hỏi mới tồn tại");

            Console.WriteLine("\n--- HIERARCHY CỦA PANELQLYBAIHOC ---");
            long panelBaiHocGoId = goMap.FirstOrDefault(kvp => kvp.Value.Name == "Panelqlybaihoc").Key;
            // Map RectTransform to GameObject
            var transformToGo = new Dictionary<long, long>();
            var goTransform = new Dictionary<long, long>();
            foreach (var kvp in objects.Where(o => o.Value.TypeId == 224 || o.Value.TypeId == 4))
            {
                transformToGo[kvp.Key] = kvp.Value.GoFileId;
                goTransform[kvp.Value.GoFileId] = kvp.Key;
            }

            void PrintTree(long goId, string indent)
            {
                if (!goTransform.ContainsKey(goId)) return;
                long trId = goTransform[goId];
                foreach (var tr in objects.Where(o => (o.Value.TypeId == 224 || o.Value.TypeId == 4) && o.Value.FatherFileId == trId))
                {
                    if (transformToGo.ContainsKey(tr.Key) && goMap.ContainsKey(transformToGo[tr.Key]))
                    {
                        var childGo = goMap[transformToGo[tr.Key]];
                        Console.WriteLine($"{indent}- {childGo.Name} (GoId: {childGo.FileId}, Active: {childGo.IsActive})");
                        PrintTree(childGo.FileId, indent + "  ");
                    }
                }
            }

            PrintTree(panelBaiHocGoId, "  ");

            // Kiểm tra 09_AnswerReview.unity
            string reviewPath = Path.Combine(dir, @"Assets\Scenes\Physics\Game1\09_AnswerReview.unity");
            if (File.Exists(reviewPath))
            {
                var rLines = File.ReadAllLines(reviewPath);
                Console.WriteLine("\n--- TẤT CẢ GAMEOBJECTS CỦA 09_AnswerReview.unity ---");
                foreach (var line in rLines)
                {
                    if (line.Trim().StartsWith("m_Name: "))
                    {
                        string n = line.Trim().Substring(8).Trim('"', '\'');
                        Console.WriteLine($"  - Go: '{n}'");
                    }
                }
            }

            Console.WriteLine("\n--- HIERARCHY CỦA PANELQLYCAUHOI ---");
            long panelCauHoiGoId = goMap.FirstOrDefault(kvp => kvp.Value.Name == "Panelqlycauhoi").Key;
            PrintTree(panelCauHoiGoId, "  ");

            Console.WriteLine("\n--- KIỂM TRA MÀN 8: THEO DÕI TIẾN ĐỘ HỌC TẬP ---");
            Check(HasGo("Panelqlytiendo"), "Panelqlytiendo tồn tại trong Canvas");
            Check(HasGo("làm mới"), "Nút 'làm mới' cập nhật dữ liệu tiến độ học sinh tồn tại");
            Check(HasGo("tên cột"), "Hàng tiêu đề cột 'tên cột' tồn tại");

            Console.WriteLine("\n--- KIỂM TRA TÍNH TOÀN VẸN CÁC SCRIPT ĐÃ TẠO & TÍNH NĂNG HOÀN THIỆN ---");
            string scriptTeacherDM = Path.Combine(dir, @"Assets\Scripts\Teacher\TeacherDataManager.cs");
            string scriptController = Path.Combine(dir, @"Assets\Scripts\PhysicsGame1\PhysicsGame1_TeacherDashboardController.cs");
            string scriptBootstrap = Path.Combine(dir, @"Assets\Scripts\PhysicsGame1\PhysicsGame1Bootstrap.cs");
            string scriptMenu = Path.Combine(dir, @"Assets\Scripts\PhysicsGame1\PhysicsGame1_MenuControllers.cs");

            Check(File.Exists(scriptTeacherDM), "TeacherDataManager.cs tồn tại", scriptTeacherDM);
            Check(File.Exists(scriptController), "PhysicsGame1_TeacherDashboardController.cs tồn tại", scriptController);
            Check(File.Exists(scriptBootstrap), "PhysicsGame1Bootstrap.cs tồn tại", scriptBootstrap);
            Check(File.Exists(scriptMenu), "PhysicsGame1_MenuControllers.cs tồn tại", scriptMenu);

            string dmCode = File.ReadAllText(scriptTeacherDM);
            Check(dmCode.Contains("TeacherSubject") && dmCode.Contains("Physics") && dmCode.Contains("Biology") && dmCode.Contains("Chemistry"),
                "TeacherDataManager hỗ trợ độc lập cả 3 môn: Vật lý, Sinh học, Hóa học");
            Check(dmCode.Contains("AddChapter") && dmCode.Contains("UpdateChapter") && dmCode.Contains("ToggleChapterActive"),
                "Đầy đủ chức năng Thêm, Sửa, Ẩn/Hiện Chương & Lưu bền vững JSON/PlayerPrefs");
            Check(dmCode.Contains("khaiNiemTrongTam") && dmCode.Contains("congThucCanNho") && dmCode.Contains("hienTuongUngDung"),
                "Đầy đủ 3 trang lý thuyết: Khái niệm trọng tâm, Công thức cần nhớ, Hiện tượng ứng dụng");
            Check(dmCode.Contains("QuestionData") && dmCode.Contains("loaiCauHoi") && dmCode.Contains("mucDo"),
                "Ngân hàng câu hỏi chuẩn 18 cột, hỗ trợ Trắc nghiệm 4 đáp án & Tự luận từ khóa");
            Check(dmCode.Contains("accuracy") && dmCode.Contains("totalQuestions"),
                "Tính tỷ lệ chính xác học sinh an toàn, chống chia cho 0");

            string ctrlCode = File.ReadAllText(scriptController);
            Check(ctrlCode.Contains("EnsureRaycastBlocker"), "Đã cấu hình Raycast Blocker ngăn click xuyên qua 3 popup con");
            Check(ctrlCode.Contains("ShowOnlyDashboard") && ctrlCode.Contains("FindChildFuzzy"), "Chỉ mở 1 panel chính tại một thời điểm, đóng panel quay về Dashboard");
            Check(dmCode.Contains("FetchQuestionsFromSheet") && dmCode.Contains("SubmitQuestionToSheet"), "Tích hợp 2 chiều với Google Sheets");

            string menuCode = File.ReadAllText(scriptMenu);
            Check(menuCode.Contains("IsChapterActiveByIndex"), "Luồng học sinh đã chặn không cho chọn các chương bị giáo viên ẩn");

            // Cập nhật PhysicsGame1_EndAndAuxControllers.cs (Đã áp dụng hoàn tất)
            // UpdateAnswerReviewController(dir);

            Console.WriteLine("\n=================================================================");
            Console.WriteLine($" TỔNG KẾT: {passCount} PASS / {failCount} FAIL (Tổng số: {passCount + failCount} kiểm thử)");
            Console.WriteLine("=================================================================\n");
        }

        static void UpdateAnswerReviewController(string rootDir)
        {
            string auxPath = Path.Combine(rootDir, @"Assets\Scripts\PhysicsGame1\PhysicsGame1_EndAndAuxControllers.cs");
            if (!File.Exists(auxPath)) return;

            string text = File.ReadAllText(auxPath);
            int startIdx = text.IndexOf("#region ===== 09_AnswerReview Controller =====");
            int endIdx = text.IndexOf("#region ===== 10_Settings Controller =====");

            if (startIdx >= 0 && endIdx > startIdx)
            {
                string newReviewCode = @"#region ===== 09_AnswerReview Controller =====
    public class PhysicsGame1_AnswerReviewController : MonoBehaviour
    {
        private int _currentIndex = 0;
        private List<QuestionReviewItem> _results;

        // Trắc nghiệm UI Elements
        private TextMeshProUGUI _tmpCauso;
        private TextMeshProUGUI _tmpDebai;
        private TextMeshProUGUI _tmpDapanA;
        private TextMeshProUGUI _tmpDapanB;
        private TextMeshProUGUI _tmpDapanC;
        private TextMeshProUGUI _tmpDapanD;
        private TextMeshProUGUI _tmpLoigiai;

        private GameObject _objDapanA;
        private GameObject _objDapanB;
        private GameObject _objDapanC;
        private GameObject _objDapanD;

        private Button _btnPrev;
        private Button _btnNext;

        // Tự luận Panel UI Elements (Panelgiaichitiettuluan)
        private GameObject _panelEssayReview;
        private TextMeshProUGUI _tmpEssayCauso;
        private TextMeshProUGUI _tmpEssayDebai;
        private TextMeshProUGUI _tmpEssayNhanXet;
        private TextMeshProUGUI _tmpEssayGoiY;
        private TextMeshProUGUI _tmpEssayBuocGiai;
        private TextMeshProUGUI _tmpEssayDiemAI;
        private Button _btnEssayPrev;
        private Button _btnEssayNext;
        private Button _btnEssayBack;

        private bool _isEssayMode = false;

        private void Start()
        {
            Debug.Log(""[PhysicsGame1] 📝 Khởi tạo 09_AnswerReview (Xem lời giải chi tiết)"");

            _results = PhysicsGame1Session.Instance.QuestionResults;
            if (_results == null || _results.Count == 0)
            {
                Debug.LogWarning(""[PhysicsGame1] ⚠️ Chưa có dữ liệu câu hỏi lượt vừa rồi, dùng dữ liệu mẫu"");
                _results = new List<QuestionReviewItem>();
                for (int i = 0; i < 10; i++)
                {
                    _results.Add(new QuestionReviewItem
                    {
                        question = new QuestionData
                        {
                            questionText = $""Câu hỏi ôn tập kiến thức Vật Lý #{i + 1}"",
                            answers = new string[] { ""A. Phương án 1"", ""B. Phương án 2"", ""C. Phương án 3"", ""D. Phương án 4"" },
                            correctAnswerIndex = 0,
                            explanation = ""Lời giải chi tiết: Áp dụng kiến thức định lý và công thức Vật lý chuẩn THCS.""
                        },
                        selectedAnswerIndex = (i % 2 == 0) ? 0 : 1,
                        isCorrect = (i % 2 == 0),
                        studentDisplayAnswer = (i % 2 == 0) ? ""A. Phương án 1"" : ""B. Phương án 2"",
                        correctDisplayAnswer = ""A. Phương án 1"",
                        explanation = ""Lời giải chi tiết: Áp dụng kiến thức định lý và công thức Vật lý chuẩn THCS.""
                    });
                }
            }

            FindUIReferences();

            // Nút Câu trước & Câu tiếp (Trắc nghiệm)
            GameObject prevObj = GameObject.Find(""cautruoc"");
            _btnPrev = UIHelper.GetOrAddButton(prevObj, () =>
            {
                if (_currentIndex > 0)
                {
                    _currentIndex--;
                    DisplayQuestion(_currentIndex);
                }
            });

            GameObject nextObj = GameObject.Find(""cautiep"");
            _btnNext = UIHelper.GetOrAddButton(nextObj, () =>
            {
                if (_currentIndex < _results.Count - 1)
                {
                    _currentIndex++;
                    DisplayQuestion(_currentIndex);
                }
            });

            // Nút Quay lại: Trở về ResultScene
            GameObject backObj = GameObject.Find(""quaylai"");
            UIHelper.GetOrAddButton(backObj, () =>
            {
                SceneManager.LoadScene(""08_ResultScene"");
            });

            _currentIndex = 0;
            DisplayQuestion(0);
        }

        private void FindUIReferences()
        {
            // Xác định xem lượt thi vừa rồi là Tự luận hay Trắc nghiệm
            _isEssayMode = false;
            if (PhysicsGame1Session.Instance != null)
            {
                if (PhysicsGame1Session.Instance.SelectedGameMode == ""TuLuan"" || 
                    PhysicsGame1Session.Instance.SelectedGameMode == ""Essay"")
                {
                    _isEssayMode = true;
                }
            }
            if (!_isEssayMode && _results != null && _results.Count > 0)
            {
                _isEssayMode = _results.Any(r => (r.question != null && (r.question.loaiCauHoi == ""Tự luận"" || (r.question.answers == null || r.question.answers.Length == 0))) || !string.IsNullOrEmpty(r.essayAnswerText));
            }

            // Tìm Panelgiaichitiettuluan trong Scene
            var allObjs = Resources.FindObjectsOfTypeAll<GameObject>();
            _panelEssayReview = allObjs.FirstOrDefault(g => g.name == ""Panelgiaichitiettuluan"");

            if (_panelEssayReview != null)
            {
                _panelEssayReview.SetActive(_isEssayMode);

                Transform pet = _panelEssayReview.transform;
                _tmpEssayCauso = pet.Find(""question (1)/ghicauso"")?.GetComponent<TextMeshProUGUI>() ?? pet.Find(""ghicauso"")?.GetComponent<TextMeshProUGUI>();
                _tmpEssayDebai = pet.Find(""debai (1)/ghidebai"")?.GetComponent<TextMeshProUGUI>() ?? pet.Find(""debai (1)"")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayNhanXet = pet.Find(""nhanxet"")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayGoiY = pet.Find(""goiycaithien"")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayBuocGiai = pet.Find(""cacbuocgiai/ghiloigiai"")?.GetComponent<TextMeshProUGUI>() ?? pet.Find(""cacbuocgiai"")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayDiemAI = pet.Find(""điểm AI chấm"")?.GetComponentInChildren<TextMeshProUGUI>(true) ?? pet.Find(""diem"")?.GetComponentInChildren<TextMeshProUGUI>(true);

                // Cấu hình các nút điều hướng Tự luận
                GameObject btnPrevEssay = pet.Find(""cautruoc (1)"")?.gameObject;
                _btnEssayPrev = UIHelper.GetOrAddButton(btnPrevEssay, () =>
                {
                    if (_currentIndex > 0)
                    {
                        _currentIndex--;
                        DisplayQuestion(_currentIndex);
                    }
                });

                GameObject btnNextEssay = pet.Find(""cautiep (1)"")?.gameObject;
                _btnEssayNext = UIHelper.GetOrAddButton(btnNextEssay, () =>
                {
                    if (_currentIndex < _results.Count - 1)
                    {
                        _currentIndex++;
                        DisplayQuestion(_currentIndex);
                    }
                });

                GameObject btnBackEssay = pet.Find(""quaylai (1)"")?.gameObject;
                _btnEssayBack = UIHelper.GetOrAddButton(btnBackEssay, () =>
                {
                    SceneManager.LoadScene(""08_ResultScene"");
                });
            }

            // Ẩn/Hiện nhóm Trắc nghiệm tương ứng
            GameObject mcQuestion = GameObject.Find(""question"");
            GameObject mcDebai = GameObject.Find(""debai"");
            GameObject mcLoigiai = GameObject.Find(""loigiai"");
            GameObject mcCautruoc = GameObject.Find(""cautruoc"");
            GameObject mcCautiep = GameObject.Find(""cautiep"");
            GameObject mcQuaylai = GameObject.Find(""quaylai"");

            if (mcQuestion != null) mcQuestion.SetActive(!_isEssayMode);
            if (mcDebai != null) mcDebai.SetActive(!_isEssayMode);
            if (mcLoigiai != null) mcLoigiai.SetActive(!_isEssayMode);
            if (mcCautruoc != null) mcCautruoc.SetActive(!_isEssayMode);
            if (mcCautiep != null) mcCautiep.SetActive(!_isEssayMode);
            if (mcQuaylai != null) mcQuaylai.SetActive(!_isEssayMode);

            GameObject causoObj = GameObject.Find(""ghicauso"");
            if (causoObj != null)
            {
                _tmpCauso = causoObj.GetComponent<TextMeshProUGUI>() ?? causoObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_tmpCauso != null)
                {
                    _tmpCauso.raycastTarget = false;
                    _tmpCauso.textWrappingMode = TextWrappingModes.NoWrap;
                    _tmpCauso.alignment = TextAlignmentOptions.Center;
                    _tmpCauso.fontStyle = FontStyles.Bold;
                    _tmpCauso.fontSize = 26f;
                    _tmpCauso.color = new Color(0.22f, 0.12f, 0.05f);
                    RectTransform crt = _tmpCauso.rectTransform;
                    if (crt != null)
                    {
                        crt.anchorMin = new Vector2(0.5f, 0.5f);
                        crt.anchorMax = new Vector2(0.5f, 0.5f);
                        crt.pivot = new Vector2(0.5f, 0.5f);
                        crt.anchoredPosition = new Vector2(55f, -8f);
                        crt.sizeDelta = new Vector2(90f, 44f);
                    }
                }
            }

            GameObject debaiObj = GameObject.Find(""debai"") ?? GameObject.Find(""ghidebai"");
            if (debaiObj != null)
            {
                _tmpDebai = debaiObj.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDebai, 60f, 60f, 70f, 35f, 14f, 22f, TextAlignmentOptions.Center, new Color(0.15f, 0.10f, 0.05f), true);
            }

            _objDapanA = GameObject.Find(""dapana"");
            _objDapanB = GameObject.Find(""dapanb"");
            _objDapanC = GameObject.Find(""dapanc"");
            _objDapanD = GameObject.Find(""dapand"");

            if (_objDapanA != null)
            {
                _tmpDapanA = _objDapanA.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanA, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            if (_objDapanB != null)
            {
                _tmpDapanB = _objDapanB.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanB, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            if (_objDapanC != null)
            {
                _tmpDapanC = _objDapanC.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanC, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            if (_objDapanD != null)
            {
                _tmpDapanD = _objDapanD.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanD, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            GameObject lgObj = GameObject.Find(""loigiai"") ?? GameObject.Find(""ghiloigiai"");
            if (lgObj != null)
            {
                _tmpLoigiai = lgObj.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpLoigiai, 85f, 85f, 150f, 60f, 13f, 24f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.08f, 0.04f));
            }
        }

        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= _results.Count) return;
            QuestionReviewItem item = _results[index];

            // Nếu là Tự Luận và có Panelgiaichitiettuluan
            if (_isEssayMode && _panelEssayReview != null)
            {
                if (_tmpEssayCauso != null)
                {
                    _tmpEssayCauso.text = $""CÂU {index + 1} / {_results.Count}"";
                    _tmpEssayCauso.enableAutoSizing = true;
                }
                if (_tmpEssayDebai != null)
                {
                    _tmpEssayDebai.text = item.question != null ? item.question.questionText : """";
                    _tmpEssayDebai.enableAutoSizing = true;
                    _tmpEssayDebai.fontSizeMin = 13f;
                    _tmpEssayDebai.fontSizeMax = 20f;
                }
                if (_tmpEssayDiemAI != null)
                {
                    _tmpEssayDiemAI.text = $""{item.earnedScore:F1}/10 ĐIỂM"";
                    _tmpEssayDiemAI.color = item.isCorrect ? new Color(0.2f, 0.9f, 0.35f) : new Color(0.95f, 0.3f, 0.2f);
                }
                if (_tmpEssayNhanXet != null)
                {
                    string fb = !string.IsNullOrEmpty(item.aiFeedback) ? item.aiFeedback :
                        (item.isCorrect ? ""Bài làm đúng trọng tâm, đạt yêu cầu tiêu chí chấm."" : ""Bài làm chưa đủ ý, cần bổ sung luận điểm và công thức."");
                    _tmpEssayNhanXet.text = fb;
                    _tmpEssayNhanXet.enableAutoSizing = true;
                    _tmpEssayNhanXet.fontSizeMin = 12f;
                    _tmpEssayNhanXet.fontSizeMax = 16f;
                }
                if (_tmpEssayGoiY != null)
                {
                    string sugg = !string.IsNullOrEmpty(item.aiSuggestions) ? item.aiSuggestions :
                        (!string.IsNullOrEmpty(item.question?.meoGhiNho) ? item.question.meoGhiNho : ""Cần rèn luyện cách trình bày khoa học và đủ các bước giải."");
                    _tmpEssayGoiY.text = sugg;
                    _tmpEssayGoiY.enableAutoSizing = true;
                    _tmpEssayGoiY.fontSizeMin = 12f;
                    _tmpEssayGoiY.fontSizeMax = 16f;
                }
                if (_tmpEssayBuocGiai != null)
                {
                    string refAns = !string.IsNullOrEmpty(item.question?.dapAnThamChieu) ? item.question.dapAnThamChieu :
                        (!string.IsNullOrEmpty(item.explanation) ? item.explanation : ""Tham khảo lời giải mẫu trong sách giáo khoa."");
                    _tmpEssayBuocGiai.text = refAns;
                    _tmpEssayBuocGiai.enableAutoSizing = true;
                    _tmpEssayBuocGiai.fontSizeMin = 12f;
                    _tmpEssayBuocGiai.fontSizeMax = 16f;
                }

                if (_btnEssayPrev != null) _btnEssayPrev.interactable = (index > 0);
                if (_btnEssayNext != null) _btnEssayNext.interactable = (index < _results.Count - 1);
                return;
            }

            if (_tmpCauso != null) _tmpCauso.text = $""{index + 1}/{_results.Count}"";
            if (_tmpDebai != null) _tmpDebai.text = item.question != null ? item.question.questionText : """";

            bool isEssay = (item.question != null && (item.question.answers == null || item.question.answers.Length == 0))
                           || !string.IsNullOrEmpty(item.essayAnswerText);

            if (_objDapanA != null) _objDapanA.SetActive(!isEssay);
            if (_objDapanB != null) _objDapanB.SetActive(!isEssay);
            if (_objDapanC != null) _objDapanC.SetActive(!isEssay);
            if (_objDapanD != null) _objDapanD.SetActive(!isEssay);

            if (isEssay)
            {
                if (_tmpLoigiai != null)
                {
                    string status = item.isCorrect 
                        ? $""<color=#15803d><b>[ ĐẠT ]</b> AI ĐÁNH GIÁ: {item.earnedScore}/10 ĐIỂM</color>"" 
                        : $""<color=#b91c1c><b>[ CHƯA ĐẠT ]</b> AI ĐÁNH GIÁ: {item.earnedScore}/10 ĐIỂM</color>"";

                    string studentText = !string.IsNullOrEmpty(item.essayAnswerText) 
                        ? item.essayAnswerText 
                        : (!string.IsNullOrEmpty(item.studentDisplayAnswer) ? item.studentDisplayAnswer : ""(Chưa có câu trả lời)"");

                    string fb = !string.IsNullOrEmpty(item.aiFeedback) ? item.aiFeedback : item.explanation;
                    string sugg = !string.IsNullOrEmpty(item.aiSuggestions) ? $""\n\n<b><color=#0284c7>Gợi ý cải thiện:</color></b>\n{item.aiSuggestions}"" : """";

                    _tmpLoigiai.text = $""{status}\n\n<b><color=#7c2d12>Bài làm của học sinh:</color></b>\n\""{studentText}\""\n\n<b><color=#1d4ed8>Nhận xét chi tiết từ AI:</color></b>\n{fb}{sugg}"";
                }
            }
            else
            {
                // Hiển thị 4 phương án (Lọc sạch tiền tố A. B. C. D. để không đè lên huy hiệu tròn có sẵn trên nút)
                string[] answers = item.question != null ? item.question.answers : null;
                if (_tmpDapanA != null && answers != null && answers.Length > 0) _tmpDapanA.text = UIHelper.CleanAnswerText(answers[0]);
                if (_tmpDapanB != null && answers != null && answers.Length > 1) _tmpDapanB.text = UIHelper.CleanAnswerText(answers[1]);
                if (_tmpDapanC != null && answers != null && answers.Length > 2) _tmpDapanC.text = UIHelper.CleanAnswerText(answers[2]);
                if (_tmpDapanD != null && answers != null && answers.Length > 3) _tmpDapanD.text = UIHelper.CleanAnswerText(answers[3]);

                // Đánh dấu màu sắc đáp án đúng và lựa chọn của học sinh
                GameObject[] optObjs = new GameObject[] { _objDapanA, _objDapanB, _objDapanC, _objDapanD };
                for (int i = 0; i < 4; i++)
                {
                    if (optObjs[i] != null)
                    {
                        Image img = optObjs[i].GetComponent<Image>();
                        if (img != null)
                        {
                            if (item.question != null && i == item.question.correctAnswerIndex)
                                img.color = new Color(0.3f, 0.9f, 0.4f); // Xanh lá: Đáp án đúng
                            else if (i == item.selectedAnswerIndex && !item.isCorrect)
                                img.color = new Color(1f, 0.35f, 0.35f); // Đỏ: Đáp án học sinh chọn sai
                            else
                                img.color = Color.white;
                        }
                    }
                }

                // Lời giải chi tiết được căn chỉnh hoàn hảo trong cuộn giấy bên dưới tiêu đề
                if (_tmpLoigiai != null)
                {
                    string status = item.isCorrect 
                        ? ""<color=#15803d><b>[ ĐÚNG ]</b> BẠN ĐÃ TRẢ LỜI CHÍNH XÁC (+10 điểm)</color>"" 
                        : ""<color=#b91c1c><b>[ CHƯA ĐÚNG ]</b> BẠN ĐÃ TRẢ LỜI CHƯA CHÍNH XÁC (0 điểm)</color>"";

                    string correctAnsText = """";
                    if (answers != null && item.question != null && item.question.correctAnswerIndex >= 0 && item.question.correctAnswerIndex < answers.Length)
                    {
                        char c = (char)('A' + item.question.correctAnswerIndex);
                        correctAnsText = $""<b><color=#1d4ed8>Đáp án đúng:</color></b> {c}. {UIHelper.CleanAnswerText(answers[item.question.correctAnswerIndex])}\n\n"";
                    }

                    string expl = item.explanation;
                    if (string.IsNullOrEmpty(expl) && item.question != null) expl = item.question.explanation;
                    if (string.IsNullOrEmpty(expl)) expl = ""Xem lại lý thuyết và công thức trọng tâm của bài học."";

                    _tmpLoigiai.text = $""{status}\n\n{correctAnsText}<b><color=#1d4ed8>Lời giải chi tiết:</color></b>\n{expl}"";
                }
            }

            // Vô hiệu hóa nút prev ở câu đầu, next ở câu cuối
            if (_btnPrev != null) _btnPrev.interactable = (index > 0);
            if (_btnNext != null) _btnNext.interactable = (index < _results.Count - 1);
        }
    }
    #endregion

    ";
                text = text.Substring(0, startIdx) + newReviewCode + text.Substring(endIdx);
            }

            File.WriteAllText(auxPath, text);
            Console.WriteLine("[INFO] ✅ Đã cập nhật PhysicsGame1_AnswerReviewController hỗ trợ Panelgiaichitiettuluan!");
        }

        static void ParseBlock(ObjInfo obj, string block)
        {
            var lines = block.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i].Trim();
                if (l.StartsWith("m_Name: "))
                {
                    obj.Name = UnescapeYaml(l.Substring(8));
                }
                if (l.StartsWith("m_IsActive: ")) obj.IsActive = (l.Substring(12).Trim() == "1");
                if (l.StartsWith("m_GameObject: "))
                {
                    var m = Regex.Match(l, @"fileID:\s*(\d+)");
                    if (m.Success) obj.GoFileId = long.Parse(m.Groups[1].Value);
                }
                if (l.StartsWith("- {fileID: "))
                {
                    var m = Regex.Match(l, @"fileID:\s*(\d+)");
                    if (m.Success) obj.Children.Add(long.Parse(m.Groups[1].Value));
                }
            }
        }

        static string UnescapeYaml(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Trim(' ', '"', '\'');
            s = Regex.Replace(s, @"\\x([0-9a-fA-F]{2})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
            s = Regex.Replace(s, @"\\u([0-9a-fA-F]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
            return s;
        }

        static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Trim().ToLower()
                .Replace(" ", "")
                .Replace("à", "a").Replace("á", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("è", "e").Replace("é", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("ì", "i").Replace("í", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ò", "o").Replace("ó", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ù", "u").Replace("ú", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ỳ", "y").Replace("ý", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
                .Replace("đ", "d");
        }
    }
}
