using UnityEngine;
using UnityEngine.InputSystem; // Thêm thư viện hệ thống mới

public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt tốc độ")]
    public float speed = 7f;

    [Header("Giới hạn mép đường")]
    public float minX = -2f;
    public float maxX = 2f;

    void Update()
    {
        float moveInput = 0f;

        // Bắt sự kiện bàn phím bằng New Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput = -1f; // Sang trái
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput = 1f;  // Sang phải
            }
        }

        Vector3 newPosition = transform.position;
        newPosition.x += moveInput * speed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        transform.position = newPosition;
    }
}