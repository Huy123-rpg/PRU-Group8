using UnityEngine;

namespace PRU.Biology
{
    public class MazeBossGate : MonoBehaviour
    {
        [Range(1, 5)] public int bossNumber = 1;
        public string bossName = "Người gác cổng";
        public GameObject gate;
        public MazeEssayController essayController;
        [Header("Câu hỏi thật; để trống đến khi nối ngân hàng câu hỏi")]
        public string questionID;
        [TextArea(3, 8)] public string questionText;
        [Range(0f, 100f)] public float passPercentage = 70f;

        public bool Passed { get; private set; }
        private MazePlayerController nearbyPlayer;
        private int contactCount;

        private void OnTriggerEnter2D(Collider2D other)
        {
            MazePlayerController player = other.GetComponentInParent<MazePlayerController>();
            if (player == null) return;
            nearbyPlayer = player;
            contactCount++;
            if (essayController != null) essayController.SetNearbyBoss(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<MazePlayerController>() != nearbyPlayer) return;
            contactCount = Mathf.Max(0, contactCount - 1);
            if (contactCount > 0) return;
            nearbyPlayer = null;
            if (essayController != null) essayController.ClearNearbyBoss(this);
        }

        private void Update()
        {
            if (nearbyPlayer == null || Passed || essayController == null) return;
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            pressed = keyboard != null && keyboard.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            pressed = Input.GetKeyDown(KeyCode.E);
#endif
            if (pressed) essayController.OpenBoss(this);
        }

        public bool MarkPassed(MazeEssayController owner)
        {
            if (owner == null || owner != essayController || Passed) return false;
            Passed = true;
            if (gate != null) gate.SetActive(false);
            return true;
        }
    }
}
