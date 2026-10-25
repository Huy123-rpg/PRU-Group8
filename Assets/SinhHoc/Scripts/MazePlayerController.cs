using UnityEngine;

namespace PRU.Biology
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MazePlayerController : MonoBehaviour
    {
        [Min(0f)] public float moveSpeed = 3.5f;
        private Rigidbody2D body;
        private Vector2 input;
        private bool inputLocked;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            input = Vector2.zero;
            if (inputLocked) return;
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
            input = Vector2.ClampMagnitude(input, 1f);
        }

        private void FixedUpdate()
        {
            body.linearVelocity = inputLocked ? Vector2.zero : input * moveSpeed;
        }

        // Question UI can lock movement without pausing the entire game.
        public void SetInputLocked(bool locked)
        {
            inputLocked = locked;
            input = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void OnDisable()
        {
            input = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }
    }
}
