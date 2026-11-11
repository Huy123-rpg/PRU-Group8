using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển nhân vật người chơi trong Đấu trường Đối kháng 1vs1:
    /// - Di chuyển bằng phím W - A - S - D hoặc phím mũi tên
    /// - Hỗ trợ đồng thời cả New Input System và Legacy Input
    /// - Giới hạn biên đấu trường (không ra ngoài màn chơi)
    /// - Nhận diện trụ gần nhất (bán kính ~135 units), hiển thị gợi ý bấm E
    /// - Bấm E, Space hoặc Enter để mở popup câu hỏi của trụ đó
    /// - Tự động khóa di chuyển khi đang mở popup câu hỏi hoặc kết thúc trận
    /// </summary>
    public class VersusPlayerController : MonoBehaviour
    {
        [Header("=== MOVEMENT CONFIG ===")]
        [SerializeField] private float moveSpeed = 380f;
        [SerializeField] private Vector2 xBounds = new Vector2(-750f, 750f);
        [SerializeField] private Vector2 yBounds = new Vector2(-360f, 220f);

        [Header("=== INTERACTION ===")]
        [SerializeField] private float interactRadius = 175f;

        private RectTransform _rectTransform;
        private VersusTowerController _nearestTower;

        public Vector2 Position => _rectTransform != null ? _rectTransform.anchoredPosition : Vector2.zero;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (VersusArenaController.Instance != null && VersusArenaController.Instance.IsInputLocked)
            {
                if (_nearestTower != null)
                {
                    _nearestTower.SetPromptVisible(false);
                }
                return;
            }

            HandleMovement();
            HandleInteraction();
        }

        private void HandleMovement()
        {
            if (_rectTransform == null) return;

            float moveX = 0f;
            float moveY = 0f;

            // 1. Kiểm tra New Input System (Unity 6 native)
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveY += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveY -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveX -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveX += 1f;
            }

            // 2. Dự phòng Legacy Input (nếu New Input System null)
            try
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveY += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveY -= 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveX -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveX += 1f;
            }
            catch {}

            moveX = Mathf.Clamp(moveX, -1f, 1f);
            moveY = Mathf.Clamp(moveY, -1f, 1f);

            if (moveX != 0f || moveY != 0f)
            {
                Vector2 dir = new Vector2(moveX, moveY).normalized;
                Vector2 newPos = _rectTransform.anchoredPosition + dir * (moveSpeed * Time.deltaTime);

                // Giới hạn biên đấu trường
                newPos.x = Mathf.Clamp(newPos.x, xBounds.x, xBounds.y);
                newPos.y = Mathf.Clamp(newPos.y, yBounds.x, yBounds.y);

                _rectTransform.anchoredPosition = newPos;
            }
        }

        private void HandleInteraction()
        {
            if (VersusArenaController.Instance == null) return;

            var towers = VersusArenaController.Instance.Towers;
            if (towers == null || towers.Length == 0) return;

            VersusTowerController closest = null;
            float minDst = interactRadius;

            Vector2 playerPos = Position;
            foreach (var tower in towers)
            {
                if (tower == null || tower.IsClaimed) continue;

                float dst = Vector2.Distance(playerPos, tower.Position);
                if (dst < minDst)
                {
                    minDst = dst;
                    closest = tower;
                }
            }

            if (_nearestTower != closest)
            {
                if (_nearestTower != null) _nearestTower.SetPromptVisible(false);
                _nearestTower = closest;
                if (_nearestTower != null) _nearestTower.SetPromptVisible(true);
            }

            bool interactPressed = false;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                {
                    interactPressed = true;
                }
            }
            try
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    interactPressed = true;
                }
            }
            catch {}

            if (_nearestTower != null && interactPressed)
            {
                VersusArenaController.Instance.OpenQuestionModal(_nearestTower.towerNumber);
            }
        }
    }
}
