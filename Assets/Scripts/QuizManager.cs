using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Question
{
    public string questionText;
    public string ansA, ansB, ansC, ansD;
    public string correctAnswer;
    public string explain;
}

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Dữ liệu Excel (CSV)")]
    public TextAsset questionDataCSV;

    [Header("Cài đặt Số lượng câu hỏi")]
    public int soCauHoiCanTraLoi = 5;
    private int soCauDaHoi = 0; // Đổi thành đếm TỔNG số câu đã hỏi (Đúng/Sai/Hết giờ đều đếm)

    [Header("Giao diện UI")]
    public GameObject panelCuonVo;
    public TextMeshProUGUI questionTextUI;
    public TMP_InputField answerInputField;
    public TextMeshProUGUI textA;
    public TextMeshProUGUI textB;
    public TextMeshProUGUI textC;
    public TextMeshProUGUI textD;

    [Header("Tương tác Button & Màu sắc")]
    public Button[] nutDapAn;
    public Color mauChuBinhThuong = Color.black;
    public Color mauChuKhiChon = Color.blue;

    [Header("Cài đặt Thời gian")]
    public TextMeshProUGUI timerTextUI;
    public float thoiGianMoiCau = 15f;
    private float thoiGianHienTai;
    private bool dangDemNguoc = false;

    [Header("Cài đặt Máu (Health)")]
    public Slider thanhMauUI;
    public float maxMau = 100f;

    [Tooltip("Phần trăm máu ĐƯỢC CỘNG khi trả lời ĐÚNG")]
    public float phanTramCongMau = 20f; // <-- BIẾN MỚI THÊM ĐỂ CỘNG 20% MÁU

    private float mauHienTai;

    private List<Question> questionList = new List<Question>();
    private int currentQuestionIndex = 0;
    private GameObject currentBoss;

    private bool daChotDapAn = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        LoadQuestionsFromCSV();
        if (panelCuonVo != null) panelCuonVo.SetActive(false);

        mauHienTai = maxMau;
        if (thanhMauUI != null)
        {
            thanhMauUI.maxValue = maxMau;
            thanhMauUI.value = mauHienTai;
        }
    }

    private void Update()
    {
        if (dangDemNguoc && !daChotDapAn)
        {
            thoiGianHienTai -= Time.unscaledDeltaTime;

            if (timerTextUI != null) timerTextUI.text = Mathf.CeilToInt(thoiGianHienTai).ToString() + "s";

            if (thoiGianHienTai <= 0)
            {
                dangDemNguoc = false;
                daChotDapAn = true;
                if (timerTextUI != null) timerTextUI.text = "0s";

                string loiGiaiThich = questionList[currentQuestionIndex].explain;
                answerInputField.text = "HẾT GIỜ! " + loiGiaiThich;

                Debug.Log("Đã hết giờ! Trừ máu và chuyển câu...");
                TruMau();
                if (mauHienTai > 0) StartCoroutine(DoiChuyenCau(false));
            }
        }
    }

    void LoadQuestionsFromCSV()
    {
        if (questionDataCSV == null)
        {
            Debug.LogError("LỖI: Chưa gắn file CSV vào ô Question Data CSV!");
            return;
        }

        string[] dataLines = questionDataCSV.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        Debug.Log("File CSV có tổng cộng " + dataLines.Length + " dòng.");

        for (int i = 1; i < dataLines.Length; i++)
        {
            string[] columns = dataLines[i].Split(',');

            if (columns.Length >= 6)
            {
                Question q = new Question();
                q.questionText = columns[0].Trim();
                q.ansA = columns[1].Trim();
                q.ansB = columns[2].Trim();
                q.ansC = columns[3].Trim();
                q.ansD = columns[4].Trim();
                q.correctAnswer = columns[5].Trim();

                if (columns.Length >= 7) q.explain = columns[6].Trim();
                else q.explain = "(Không có giải thích chi tiết)";

                questionList.Add(q);
            }
            else
            {
                Debug.LogWarning("Dòng " + i + " bị thiếu cột, đã bỏ qua.");
            }
        }

        Debug.Log("Đã load THÀNH CÔNG " + questionList.Count + " câu hỏi!");
    }

    public void ShowQuiz(GameObject boss)
    {
        Debug.Log("Lệnh mở bảng Quiz đã được gọi!");

        if (questionList.Count == 0)
        {
            Debug.LogError("LỖI NẶNG: Danh sách câu hỏi đang = 0. Bảng Quiz sẽ bị đóng băng!");
            return;
        }

        currentBoss = boss;
        soCauDaHoi = 0; // Reset số câu đã hỏi về 0 khi gặp Boss

        Time.timeScale = 0f;
        panelCuonVo.SetActive(true);

        HienThiCauHoiMoi();
    }

    private void HienThiCauHoiMoi()
    {
        if (questionList.Count == 0)
        {
            ThangBoss();
            return;
        }

        daChotDapAn = false;
        currentQuestionIndex = Random.Range(0, questionList.Count);
        ResetHieuUngTatCaNut();
        DisplayQuestion(currentQuestionIndex);

        thoiGianHienTai = thoiGianMoiCau;
        dangDemNguoc = true;
    }

    void DisplayQuestion(int index)
    {
        Question q = questionList[index];
        questionTextUI.text = q.questionText;
        textA.text = q.ansA;
        textB.text = q.ansB;
        textC.text = q.ansC;
        textD.text = q.ansD;
        answerInputField.text = "";
    }

    public void OnAnswerButtonClicked(string selectedAnswer)
    {
        Debug.Log("Bạn vừa click đáp án: [" + selectedAnswer + "]");

        if (daChotDapAn)
        {
            Debug.Log("Bảng đã khóa chốt đáp án, từ chối click!");
            return;
        }

        answerInputField.text = selectedAnswer;
        ResetHieuUngTatCaNut();

        if (selectedAnswer == "A" && textA != null) { textA.color = mauChuKhiChon; textA.fontStyle = FontStyles.Bold; }
        else if (selectedAnswer == "B" && textB != null) { textB.color = mauChuKhiChon; textB.fontStyle = FontStyles.Bold; }
        else if (selectedAnswer == "C" && textC != null) { textC.color = mauChuKhiChon; textC.fontStyle = FontStyles.Bold; }
        else if (selectedAnswer == "D" && textD != null) { textD.color = mauChuKhiChon; textD.fontStyle = FontStyles.Bold; }
    }

    private void ResetHieuUngTatCaNut()
    {
        if (textA != null) { textA.color = mauChuBinhThuong; textA.fontStyle = FontStyles.Normal; }
        if (textB != null) { textB.color = mauChuBinhThuong; textB.fontStyle = FontStyles.Normal; }
        if (textC != null) { textC.color = mauChuBinhThuong; textC.fontStyle = FontStyles.Normal; }
        if (textD != null) { textD.color = mauChuBinhThuong; textD.fontStyle = FontStyles.Normal; }
    }

    public void OnSubmitClicked()
    {
        Debug.Log("Bạn vừa bấm nút GỬI!");

        if (daChotDapAn) return;
        if (string.IsNullOrEmpty(answerInputField.text))
        {
            Debug.LogWarning("Chưa chọn đáp án nào, không cho gửi!");
            return;
        }

        daChotDapAn = true;
        dangDemNguoc = false;

        string userAnswer = answerInputField.text.Trim().ToUpper();
        string correctAnswer = questionList[currentQuestionIndex].correctAnswer.ToUpper();
        string loiGiaiThich = questionList[currentQuestionIndex].explain;

        if (userAnswer == correctAnswer)
        {
            answerInputField.text = "ĐÚNG! " + loiGiaiThich;

            // GỌI HÀM CỘNG MÁU TẠI ĐÂY
            CongMau(phanTramCongMau);

            StartCoroutine(DoiChuyenCau(true));
        }
        else
        {
            answerInputField.text = "SAI! " + loiGiaiThich;
            TruMau();
            if (mauHienTai > 0) StartCoroutine(DoiChuyenCau(false));
        }
    }

    IEnumerator DoiChuyenCau(bool laCauDung)
    {
        yield return new WaitForSecondsRealtime(4f);
        questionList.RemoveAt(currentQuestionIndex);

        // Đếm tổng số câu đã hỏi bất chấp đúng hay sai
        soCauDaHoi++;

        if (soCauDaHoi >= soCauHoiCanTraLoi)
        {
            ThangBoss();
            yield break;
        }

        HienThiCauHoiMoi();
    }

    // ĐÃ THÊM: Hàm cộng máu
    public void CongMau(float phanTram)
    {
        float luongMauCong = maxMau * (phanTram / 100f);
        mauHienTai += luongMauCong;

        // Chốt chặn an toàn: Nếu cộng lố thì trả về đúng 100
        if (mauHienTai > maxMau)
        {
            mauHienTai = maxMau;
        }

        if (thanhMauUI != null) thanhMauUI.value = mauHienTai;
        Debug.Log("Thưởng: Được cộng " + phanTram + "% máu!");
    }

    // Thêm tham số phanTram để nhận sát thương. Nếu gọi bình thường (trả lời sai) sẽ tự lấy mặc định là 10.
    public void TruMau(float phanTram = 10f)
    {
        float luongMauTru = maxMau * (phanTram / 100f);
        mauHienTai -= luongMauTru;

        if (thanhMauUI != null) thanhMauUI.value = mauHienTai;

        if (mauHienTai <= 0)
        {
            Debug.Log("GAME OVER! Bạn đã hết máu.");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void ThangBoss()
    {
        dangDemNguoc = false;
        if (currentBoss != null) Destroy(currentBoss);
        panelCuonVo.SetActive(false);
        Time.timeScale = 1f;
    }
}