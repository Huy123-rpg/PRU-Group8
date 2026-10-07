using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PRU.Biology
{
    public class BiologyGameHistory : MonoBehaviour
    {
        public static BiologyGameHistory Instance { get; private set; }

        [Header("Google Apps Script Web App URL")]
        [TextArea(2, 4)]
        public string webAppUrl = "";

        private string phienID;
        private string thoiGianBatDau;

        private int soDung = 0;
private int soSai = 0;

private readonly List<string> questionIDs = new List<string>();
private readonly List<string> answers = new List<string>();
private readonly List<string> results = new List<string>();

// ================================
// THỐNG KÊ CHO MÀN HÌNH GAME OVER
// ================================

public int TotalQuestions
{
    get { return questionIDs.Count; }
}

public int CorrectAnswers
{
    get { return soDung; }
}

public int WrongAnswers
{
    get { return soSai; }
}

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            phienID = "PH-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            thoiGianBatDau = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Debug.Log("[GameHistory] Bắt đầu phiên: " + phienID);
        }

        // ============================================================
        // LƯU MỖI CÂU TRẢ LỜI
        // ============================================================
        public void RecordAnswer(
            string questionID,
            string selectedAnswer,
            bool isCorrect)
        {
            // Google Sheet hiện lưu tối đa 10 câu
            if (questionIDs.Count >= 10)
            {
                Debug.LogWarning(
                    "[GameHistory] Đã đủ 10 câu, không lưu thêm."
                );
                return;
            }

            questionIDs.Add(questionID);
            answers.Add(selectedAnswer);
            results.Add(isCorrect ? "Đúng" : "Sai");

            if (isCorrect)
            {
                soDung++;
            }
            else
            {
                soSai++;
            }

            Debug.Log(
                "[GameHistory] "
                + questionID
                + " | "
                + selectedAnswer
                + " | "
                + (isCorrect ? "Đúng" : "Sai")
            );
        }

        // ============================================================
        // KẾT THÚC GAME
        // ============================================================
        public void FinishGame(
            int level,
            float survivalTime,
            int gameScore)
        {
            StartCoroutine(
                UploadHistory(
                    level,
                    survivalTime,
                    gameScore
                )
            );
        }

        // ============================================================
        // GỬI DỮ LIỆU LÊN GOOGLE SHEET
        // ============================================================
        private IEnumerator UploadHistory(
            int level,
            float survivalTime,
            int gameScore)
        {
            if (string.IsNullOrWhiteSpace(webAppUrl))
            {
                Debug.LogError(
                    "[GameHistory] Chưa nhập Google Apps Script Web App URL!"
                );

                yield break;
            }

            int tongCau = questionIDs.Count;

            // Điểm học tập thang 100
            int diemSo = 0;

            if (tongCau > 0)
            {
                diemSo = Mathf.RoundToInt(
                    (float)soDung / tongCau * 100f
                );
            }

            GameHistoryData data = new GameHistoryData();

            // ========================================================
            // THÔNG TIN PHIÊN
            // ========================================================

            data.PhienID = phienID;

            // Tạm thời dùng HS001.
            // Sau này có login thì thay bằng UserID thật.
            data.UserID = "HS001";

            data.BaiID = GetLessonID();

            data.DiemSo = diemSo;

            data.ThoiGianBatDau = thoiGianBatDau;

            data.ThoiGianKetThuc =
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            data.TongCau = tongCau;
            data.SoDung = soDung;
            data.SoSai = soSai;

            data.TrangThai = "Hoàn thành";

            // ========================================================
            // QUESTION ID
            // ========================================================

            data.Q1ID = GetValue(questionIDs, 0);
            data.Q2ID = GetValue(questionIDs, 1);
            data.Q3ID = GetValue(questionIDs, 2);
            data.Q4ID = GetValue(questionIDs, 3);
            data.Q5ID = GetValue(questionIDs, 4);
            data.Q6ID = GetValue(questionIDs, 5);
            data.Q7ID = GetValue(questionIDs, 6);
            data.Q8ID = GetValue(questionIDs, 7);
            data.Q9ID = GetValue(questionIDs, 8);
            data.Q10ID = GetValue(questionIDs, 9);

            // ========================================================
            // ĐÁP ÁN NGƯỜI CHƠI
            // ========================================================

            data.DapAn1 = GetValue(answers, 0);
            data.DapAn2 = GetValue(answers, 1);
            data.DapAn3 = GetValue(answers, 2);
            data.DapAn4 = GetValue(answers, 3);
            data.DapAn5 = GetValue(answers, 4);
            data.DapAn6 = GetValue(answers, 5);
            data.DapAn7 = GetValue(answers, 6);
            data.DapAn8 = GetValue(answers, 7);
            data.DapAn9 = GetValue(answers, 8);
            data.DapAn10 = GetValue(answers, 9);

            // ========================================================
            // KẾT QUẢ TỪNG CÂU
            // ========================================================

            data.KetQua1 = GetValue(results, 0);
            data.KetQua2 = GetValue(results, 1);
            data.KetQua3 = GetValue(results, 2);
            data.KetQua4 = GetValue(results, 3);
            data.KetQua5 = GetValue(results, 4);
            data.KetQua6 = GetValue(results, 5);
            data.KetQua7 = GetValue(results, 6);
            data.KetQua8 = GetValue(results, 7);
            data.KetQua9 = GetValue(results, 8);
            data.KetQua10 = GetValue(results, 9);

            // ========================================================
            // THÔNG TIN GAME
            // ========================================================

            data.GameID = "GAME_SINHHOC";
            data.LoaiLuyenTap = "SINH_TON";

            data.LevelDatDuoc = level;
            data.ThoiGianSong = FormatTime(survivalTime);
            data.DiemGame = gameScore;

            // ========================================================
            // CHUYỂN THÀNH JSON
            // ========================================================

            string json = JsonUtility.ToJson(data);

            Debug.Log(
                "[GameHistory] Đang gửi lên Google Sheet:\n"
                + json
            );

            // ========================================================
            // HTTP POST
            // ========================================================

            using (UnityWebRequest request =
                   new UnityWebRequest(webAppUrl, "POST"))
            {
                byte[] body =
                    System.Text.Encoding.UTF8.GetBytes(json);

                request.uploadHandler =
                    new UploadHandlerRaw(body);

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json"
                );

                yield return request.SendWebRequest();

                // ====================================================
                // KẾT QUẢ
                // ====================================================

                if (request.result ==
                    UnityWebRequest.Result.Success)
                {
                    Debug.Log(
                        "[GameHistory] Google Sheet trả về: "
                        + request.downloadHandler.text
                    );
                }
                else
                {
                    Debug.LogError(
                        "[GameHistory] Gửi lịch sử thất bại: "
                        + request.error
                    );

                    Debug.LogError(
                        "[GameHistory] Response: "
                        + request.downloadHandler.text
                    );
                }
            }
        }

        // ============================================================
        // LẤY BÀI HỌC HIỆN TẠI
        // ============================================================
        private string GetLessonID()
        {
            if (!string.IsNullOrWhiteSpace(
                    GameSessionData.SelectedLessonID))
            {
                return GameSessionData.SelectedLessonID;
            }

            return "";
        }

        // ============================================================
        // LẤY GIÁ TRỊ TRONG LIST
        // ============================================================
        private string GetValue(
            List<string> list,
            int index)
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }

            return "";
        }

        // ============================================================
        // FORMAT THỜI GIAN
        // ============================================================
        private string FormatTime(float seconds)
        {
            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.FloorToInt(seconds)
                );

            int minutes =
                totalSeconds / 60;

            int secs =
                totalSeconds % 60;

            return $"{minutes:00}:{secs:00}";
        }
    }

    // ================================================================
    // DATA GỬI SANG GOOGLE APPS SCRIPT
    // ================================================================

    [Serializable]
    public class GameHistoryData
    {
        public string PhienID;
        public string UserID;
        public string BaiID;

        public int DiemSo;

        public string ThoiGianBatDau;
        public string ThoiGianKetThuc;

        public int TongCau;
        public int SoDung;

        public string TrangThai;

        // ============================================================
        // QUESTION ID
        // ============================================================

        public string Q1ID;
        public string Q2ID;
        public string Q3ID;
        public string Q4ID;
        public string Q5ID;
        public string Q6ID;
        public string Q7ID;
        public string Q8ID;
        public string Q9ID;
        public string Q10ID;

        // ============================================================
        // ĐÁP ÁN
        // ============================================================

        public string DapAn1;
        public string DapAn2;
        public string DapAn3;
        public string DapAn4;
        public string DapAn5;
        public string DapAn6;
        public string DapAn7;
        public string DapAn8;
        public string DapAn9;
        public string DapAn10;

        // ============================================================
        // KẾT QUẢ
        // ============================================================

        public string KetQua1;
        public string KetQua2;
        public string KetQua3;
        public string KetQua4;
        public string KetQua5;
        public string KetQua6;
        public string KetQua7;
        public string KetQua8;
        public string KetQua9;
        public string KetQua10;

        // ============================================================
        // GAME
        // ============================================================

        public string GameID;
        public string LoaiLuyenTap;

        public int SoSai;
        public int LevelDatDuoc;

        public string ThoiGianSong;

        public int DiemGame;
    }
}