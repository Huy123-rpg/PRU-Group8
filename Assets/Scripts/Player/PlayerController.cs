using UnityEngine;

namespace ScienceQuest.Player
{
    /// <summary>
    /// PlayerController - Prototype di chuyển 2D 4 hướng bằng Rigidbody2D.
    /// Ngăn người chơi đi xuyên tường nhờ hệ thống Physics2D & Collider2D.
    /// Phân công: M2 - Gameplay Programmer
    /// </summary>

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D rb;
        private Vector2 movementInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            // Thiết lập gravityScale = 0 cho game Top-Down 2D để nhân vật không bị rơi
            rb.gravityScale = 0f;
            // Khóa xoay nhân vật khi va chạm với vật thể
            rb.freezeRotation = true;
        }

        private void Update()
        {
            // Lấy Input từ phím W/A/S/D hoặc các phím Mũi tên (Arrow Keys)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            // Chuẩn hóa vector di chuyển để di chuyển đường chéo không bị nhanh hơn
            movementInput = new Vector2(moveX, moveY).normalized;
        }

        private void FixedUpdate()
        {
            // Di chuyển nhân vật dựa trên Rigidbody2D để đảm bảo va chạm vật lý hoạt động chuẩn
            MovePlayer();
        }

        private void MovePlayer()
        {
            Vector2 targetPosition = rb.position + movementInput * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }
}
