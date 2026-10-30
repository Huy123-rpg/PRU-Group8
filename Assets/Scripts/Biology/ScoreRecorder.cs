using System;

using System.Collections;

using System.Collections.Generic;

using System.Globalization;

using System.IO;

using UnityEngine;

using UnityEngine.Networking;



namespace ScienceQuest.Biology

{

    /// <summary>

    /// Ghi kết quả chơi của người học ra bảng tính.

    ///

    /// Cơ chế:

    ///   1. Ghi nối vào file .xlsx cục bộ (nằm trong persistentDataPath/TeacherScores).

    ///      File này mở được bằng Excel / Google Sheets / LibreOffice.

    ///   2. Nếu có cấu hình Google Sheet (Apps Script webhook) thì đồng bộ thêm 1 dòng lên sheet.

    ///

    /// Lịch sử cũng được giữ trong PlayerPrefs để vẽ lại file .xlsx khi mở game (tránh mất dữ liệu nếu file bị xoá).

    /// </summary>

    public static class ScoreRecorder

    {

        // ----- Khai báo điểm -----



        public const string HistoryKey = "Sinh_ScoreRecords";

        public const string GoogleSheetUrlKey = "Sinh_ScoreSheetUrl";

        /// <summary>
        /// DÁN TRỰC TIẾP LINK WEBHOOK URL GOOGLE APPS SCRIPT CỦA BẠN VÀO GIỮA CẶP NGOẶC KÉP DƯỚI ĐÂY:
        /// Ví dụ: "https://script.google.com/macros/s/AKfycb.../exec"
        /// </summary>
        public const string DefaultGoogleSheetUrl = "https://script.google.com/macros/s/AKfycbxk2EsE-iTyOlsJMSuvN1RDuQ9H95bsRghUv2159C8Yzw9O5XXzg4SfydrrTNQiPLhuCA/exec";



        private const int MaxHistoryRecords = 500;



        /// <summary>

        /// Đường dẫn file Excel kết quả.

        /// </summary>

        public static string ExcelFilePath => Path.Combine(TeacherStore.DataFolder, "KetQua_SinhHoc.xlsx");



        /// <summary>

        /// Đường dẫn file CSV kết quả (bản sao dạng text, tiện để mở/import).

        /// </summary>

        public static string CsvFilePath => Path.Combine(TeacherStore.DataFolder, "KetQua_SinhHoc.csv");



        /// <summary>

        /// Ghi 1 kết quả lên bảng tính: ghi nối vào file .xlsx cục bộ và đồng bộ Google Sheet nếu có.

        /// </summary>

        public static void Record(ScoreRecord record)

        {

            if (record == null) return;



            // 1. Lưu vào lịch sử trong RAM/PlayerPrefs (cho UI xem nhanh trong game)

            List<ScoreRecord> history = LoadHistory();

            history.Insert(0, record);

            if (history.Count > MaxHistoryRecords)

            {

                history.RemoveRange(MaxHistoryRecords, history.Count - MaxHistoryRecords);

            }

            SaveHistory(history);



            // 2. THEO YÊU CẦU: TUYỆT ĐỐI KHÔNG LƯU VÀO LOCAL Ổ ĐĨA TRÊN MÁY (.xlsx / .csv)

            // Quản trị viên sẽ xem kết quả trực tiếp qua link Google Sheet / Excel Online.

            // (Đã vô hiệu hóa WriteExcelFromHistory và WriteCsvFromHistory)



            Debug.Log($"[ScoreRecorder] 🌐 Đã ghi nhận phiên chơi: {record.phienId} ({record.userId} - Bài {record.baiId} - Điểm {record.diemSo}%) " +

                      $"- Chế độ đồng bộ online lên Google Sheet (Không lưu local disk).");

        }



        /// <summary>

        /// Dựng lại file .xlsx từ lịch sử đã lưu.

        /// </summary>

        public static void WriteExcelFromHistory(List<ScoreRecord> history)

        {

            List<string> headers = ScoreExcelWriter.BuildHeaders();

            List<string[]> rows = new List<string[]>();

            if (history != null)

            {

                foreach (var rec in history)

                {

                    rows.Add(ScoreExcelWriter.RecordToRow(rec));

                }

            }



            ScoreExcelWriter.Write(ExcelFilePath, headers, rows);

        }



        private static void WriteCsvFromHistory(List<ScoreRecord> history)

        {

            try

            {

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendLine("Thời gian,Tài khoản,Chủ đề,Bài học,Hình thức,Tổng số câu,Số câu đúng,Số câu sai,Điểm,Kỷ lục,Số Boss vượt qua,Tỉ lệ đối (%),Chi tiết");

                if (history != null)

                {

                    foreach (var rec in history)

                    {

                        string[] row = ScoreExcelWriter.RecordToRow(rec);

                        for (int i = 0; i < row.Length; i++)

                        {

                            if (i > 0) sb.Append(",");

                            sb.Append(EscapeCsv(row[i]));

                        }

                        sb.AppendLine();

                    }

                }



                if (!Directory.Exists(TeacherStore.DataFolder)) Directory.CreateDirectory(TeacherStore.DataFolder);

                File.WriteAllText(CsvFilePath, sb.ToString(), new System.Text.UTF8Encoding(true));

            }

            catch (Exception e)

            {

                Debug.LogError($"[ScoreRecorder] Lỗi ghi CSV kết quả: {e.Message}");

            }

        }



        private static string EscapeCsv(string value)

        {

            string v = value ?? "";

            if (v.Contains(",") || v.Contains("\"") || v.Contains("\n"))

            {

                return "\"" + v.Replace("\"", "\"\"") + "\"";

            }

            return v;

        }



        // ----- Lịch sử (PlayerPrefs) -----



        /// <summary>

        /// Đọc lịch sử kết quả đã lưu.

        /// </summary>

        public static List<ScoreRecord> LoadHistory()

        {

            List<ScoreRecord> list = new List<ScoreRecord>();

            string raw = PlayerPrefs.GetString(HistoryKey, "");

            if (string.IsNullOrEmpty(raw)) return list;



            // Lưu dạng JSON để không vỡ khi nội dung có dấu phẩy hoặc xuống dòng

            try

            {

                ScoreRecordList wrapper = JsonUtility.FromJson<ScoreRecordList>(raw);

                if (wrapper != null && wrapper.records != null)

                {

                    list.AddRange(wrapper.records);

                }

            }

            catch

            {

                Debug.LogWarning("[ScoreRecorder] Không đọc được lịch sử điểm, sẽ bắt đầu lại từ đầu.");

            }



            return list;

        }



        private static void SaveHistory(List<ScoreRecord> history)

        {

            ScoreRecordList wrapper = new ScoreRecordList { records = history };

            PlayerPrefs.SetString(HistoryKey, JsonUtility.ToJson(wrapper));

            PlayerPrefs.Save();

        }



        /// <summary>

        /// Xoá toàn bộ lịch sử điểm (dùng khi giảng viên muốn reset bảng điểm).

        /// </summary>

        public static void ClearHistory()

        {

            PlayerPrefs.DeleteKey(HistoryKey);

            PlayerPrefs.Save();

            WriteExcelFromHistory(new List<ScoreRecord>());

            WriteCsvFromHistory(new List<ScoreRecord>());

        }



        // ----- Google Sheet -----



        /// <summary>

        /// Lấy URL webhook Google Sheet đã cấu hình (rỗng nếu chưa cấu hình).

        /// </summary>

        public static string GetSheetUrl()
        {
            string url = PlayerPrefs.GetString(GoogleSheetUrlKey, "");
            if (!string.IsNullOrEmpty(url)) return url;
            return DefaultGoogleSheetUrl;
        }



        /// <summary>

        /// Lưu URL webhook Google Sheet.

        /// </summary>

        public static void SetSheetUrl(string url)

        {

            PlayerPrefs.SetString(GoogleSheetUrlKey, url ?? "");

            PlayerPrefs.Save();

        }



        /// <summary>

        /// Đồng bộ 1 kết quả lên Google Sheet qua Apps Script webhook.

        /// Hàm chạy bằng coroutine nên cần một MonoBehaviour để chạy.

        /// </summary>

        public static IEnumerator PushToGoogleSheet(MonoBehaviour runner, ScoreRecord record)

        {

            string url = GetSheetUrl();

            if (string.IsNullOrEmpty(url) || runner == null || record == null)

            {

                yield break;

            }



            string payload = BuildSheetPayload(record);



            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))

            {

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);

                www.uploadHandler = new UploadHandlerRaw(bodyRaw);

                www.downloadHandler = new DownloadHandlerBuffer();

                www.SetRequestHeader("Content-Type", "application/json");



                yield return www.SendWebRequest();



                if (www.result == UnityWebRequest.Result.Success)

                {

                    Debug.Log($"[ScoreRecorder] 🌐 Đã đồng bộ điểm lên Google Sheet: {record.username} - {record.score} điểm");

                }

                else

                {

                    Debug.LogWarning($"[ScoreRecorder] ⚠️ Không đồng bộ được lên Google Sheet: {www.error}. " +

                                     $"Điểm vẫn đã lưu tại file: {ExcelFilePath}");

                }

            }

        }



        /// <summary>

        /// Dựng payload JSON gửi webhook Apps Script.

        /// </summary>

        private static string BuildSheetPayload(ScoreRecord rec)

        {

            return "{" +

                   // ===== 44 TRƯỜNG CHUẨN FORM BẢNG PHIÊN CHƠI (GOOGLE SHEET) =====

                   $"\"PhienID\":{Quote(rec.phienId)}," +

                   $"\"UserID\":{Quote(rec.userId)}," +

                   $"\"BaiID\":{Quote(rec.baiId)}," +

                   $"\"DiemSo\":{rec.diemSo}," +

                   $"\"ThoiGianBatDau\":{Quote(rec.thoiGianBatDau)}," +

                   $"\"ThoiGianKetThuc\":{Quote(rec.thoiGianKetThuc)}," +

                   $"\"TongCau\":{rec.tongCau}," +

                   $"\"SoDung\":{rec.soDung}," +

                   $"\"TrangThai\":{Quote(rec.trangThai)}," +



                   $"\"Q1ID\":{Quote(rec.q1Id)},\"Q2ID\":{Quote(rec.q2Id)},\"Q3ID\":{Quote(rec.q3Id)},\"Q4ID\":{Quote(rec.q4Id)},\"Q5ID\":{Quote(rec.q5Id)}," +

                   $"\"Q6ID\":{Quote(rec.q6Id)},\"Q7ID\":{Quote(rec.q7Id)},\"Q8ID\":{Quote(rec.q8Id)},\"Q9ID\":{Quote(rec.q9Id)},\"Q10ID\":{Quote(rec.q10Id)}," +



                   $"\"DapAn1\":{Quote(rec.dapAn1)},\"DapAn2\":{Quote(rec.dapAn2)},\"DapAn3\":{Quote(rec.dapAn3)},\"DapAn4\":{Quote(rec.dapAn4)},\"DapAn5\":{Quote(rec.dapAn5)}," +

                   $"\"DapAn6\":{Quote(rec.dapAn6)},\"DapAn7\":{Quote(rec.dapAn7)},\"DapAn8\":{Quote(rec.dapAn8)},\"DapAn9\":{Quote(rec.dapAn9)},\"DapAn10\":{Quote(rec.dapAn10)}," +



                   $"\"KetQua1\":{Quote(rec.ketQua1)},\"KetQua2\":{Quote(rec.ketQua2)},\"KetQua3\":{Quote(rec.ketQua3)},\"KetQua4\":{Quote(rec.ketQua4)},\"KetQua5\":{Quote(rec.ketQua5)}," +

                   $"\"KetQua6\":{Quote(rec.ketQua6)},\"KetQua7\":{Quote(rec.ketQua7)},\"KetQua8\":{Quote(rec.ketQua8)},\"KetQua9\":{Quote(rec.ketQua9)},\"KetQua10\":{Quote(rec.ketQua10)}," +



                   $"\"GameID\":{Quote(rec.gameId)}," +

                   $"\"LoaiLuyenTap\":{Quote(rec.loaiLuyenTap)}," +

                   $"\"SoSai\":{rec.soSai}," +

                   $"\"LevelDatDuoc\":{rec.levelDatDuoc}," +

                   $"\"ThoiGianSong\":{Quote(rec.thoiGianSong)}," +

                   $"\"DiemGame\":{rec.diemGame}," +



                   // ===== TRƯỜNG PHỤ TƯƠNG THÍCH =====

                   $"\"timestamp\":{Quote(rec.timestamp)}," +

                   $"\"username\":{Quote(rec.username)}," +

                   $"\"chapter\":{Quote(rec.chapter)}," +

                   $"\"lesson\":{Quote(rec.lesson)}," +

                   $"\"mode\":{Quote(rec.mode)}," +

                   $"\"totalQuestions\":{rec.totalQuestions}," +

                   $"\"correctCount\":{rec.correctCount}," +

                   $"\"wrongCount\":{rec.wrongCount}," +

                   $"\"score\":{rec.score}," +

                   $"\"highScore\":{rec.highScore}," +

                   $"\"bossesDefeated\":{rec.bossesDefeated}," +

                   $"\"accuracyPercent\":{rec.accuracyPercent.ToString("0.0", CultureInfo.InvariantCulture)}," +

                   $"\"details\":{Quote(rec.details)}" +

                   "}";

        }



        private static string Quote(string value)

        {

            string v = value ?? "";

            v = v.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");

            return "\"" + v + "\"";

        }

    }



    /// <summary>

    /// Wrapper để JsonUtility đọc/ghi danh sách kết quả.

    /// </summary>

    [Serializable]

    public class ScoreRecordList

    {

        public List<ScoreRecord> records = new List<ScoreRecord>();

    }

}

