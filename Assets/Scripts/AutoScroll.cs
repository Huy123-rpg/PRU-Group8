using UnityEngine;

public class AutoScroll : MonoBehaviour
{
    [Header("Tốc độ cuộn của Map")]
    public float speed = 5f;

    void Update()
    {
        // Liên tục đẩy Camera đi lên phía trên (trục Y) theo thời gian thực
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}