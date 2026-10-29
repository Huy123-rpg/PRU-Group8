using UnityEngine;

/// <summary>
/// MonsterShowUp - Boss gọi mở Quiz khi trôi tới GIỮA MÀN HÌNH.
///
/// Bản mới (thay cơ chế OnBecameVisible + Invoke 3s cũ):
///   - Camera tự đi LÊN trục Y (AutoScroll), Boss đứng yên => mỗi frame "boss"
///     lùi dần về phía nửa dưới màn hình.
///   - "Giữa màn hình" = đúng tọa độ Y của camera (camera là điểm giữa màn hình).
///   - Kích hoạt khi boss VỪA CẮT đường giữa (từ phía trước camera trôi xuống):
///     frame trước boss còn trước đường giữa, frame này boss đã tới/quá đường giữa.
///     => Chính xác, không phụ thuộc tốc độ map, không dùng timer chờ cứng.
///   - Boss nào bị đặt SẴN ở/behind vị trí bắt đầu của camera (không trôi tới)
///     sẽ KHÔNG kích hoạt (chỉ kích hoạt với boss thật sự trôi vào giữa màn hình).
/// </summary>
public class MonsterShowUp : MonoBehaviour
{
    [Header("Ngưỡng kích hoạt")]
    [Tooltip("Cách sớm bao nhiêu đơn vị trước 'đường giữa màn hình' (tọa độ Y camera) để kích hoạt quiz. " +
             "0 = đúng giữa màn hình. Tăng lên (ví dụ 1-2) nếu muốn quiz bật sớm hơn một chút.")]
    public float marginFront = 0f;

    private bool hasAppeared = false;
    private bool hasLastBehind = false;
    private float lastBehind = 0f;
    private Transform cam;

    void Awake()
    {
        CamMoi();
    }

    void CamMoi()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        if (cam == null)
            cam = FindFirstObjectByType<Camera>() != null ? FindFirstObjectByType<Camera>().transform : null;
    }

    void Update()
    {
        if (hasAppeared) return;
        if (cam == null) { CamMoi(); if (cam == null) return; }

        // "behind" = camY - bossY.
        //   behind càng ÂM  => boss càng NHIỀU phía TRƯỚC camera (chưa tới giữa).
        //   behind >= -marginFront => boss đã tới/quá đường giữa màn hình.
        float behind = cam.position.y - transform.position.y;

        if (hasLastBehind
            && lastBehind < -marginFront          // frame trước: boss còn ở TRƯỚC đường giữa
            && behind >= -marginFront)            // frame này:   boss đã tới/quá đường giữa
        {
            hasAppeared = true;
            Debug.Log($"[MonsterShowUp] Boss vừa tới giữa màn hình (behind={behind:F1}) -> mở quiz!");
            DungMapVaBatQuiz();
        }

        lastBehind = behind;
        hasLastBehind = true;
    }

    void DungMapVaBatQuiz()
    {
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.ShowQuiz(this.gameObject);
        }
        else
        {
            Debug.LogError("Không tìm thấy QuizManager! Hãy kiểm tra lại file QuizManager.cs");
        }
    }
}
