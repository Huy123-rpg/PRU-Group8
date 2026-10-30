using UnityEngine;

public class MonsterShowUp : MonoBehaviour
{
    private bool hasAppeared = false;

    // Hàm của Unity: Tự động chạy khi vật thể lọt vào ống kính Camera
    void OnBecameVisible()
    {
        if (!hasAppeared)
        {
            hasAppeared = true; // Đánh dấu đã xuất hiện để không đếm lại

            // Gọi hàm DungMapVaBatQuiz sau 1.5 giây để đợi Boss trôi ra giữa màn hình.
            // Nếu map cuộn nhanh hoặc chậm, bạn sửa số 1.5f này cho phù hợp.
            Invoke("DungMapVaBatQuiz", 3f);
        }
    }

    //void DungMapVaBatQuiz()
    //{
    //    // Kiểm tra xem QuizManager đã được khởi tạo trong Scene chưa
    //    if (QuizManager.Instance != null)
    //    {
    //        // Truyền chính bản thân con Boss này (gameObject) sang cho QuizManager xử lý
    //        QuizManager.Instance.ShowQuiz(this.gameObject);
    //    }
    //    else
    //    {
    //        Debug.LogError("Không tìm thấy QuizManager! Hãy kiểm tra lại file QuizManager.cs");
    //    }
    //}
    void DungMapVaBatQuiz()
    {
        Debug.Log("1. Boss đã đếm xong 3 giây và gọi DungMapVaBatQuiz!");

        if (QuizManager.Instance != null)
        {
            Debug.Log("2. Đã tìm thấy QuizManager, chuẩn bị bắn tín hiệu sang!");
            QuizManager.Instance.ShowQuiz(this.gameObject);
        }
        else
        {
            Debug.LogError("Không tìm thấy QuizManager!");
        }
    }
}