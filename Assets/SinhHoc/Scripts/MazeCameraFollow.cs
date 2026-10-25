using UnityEngine;

namespace PRU.Biology
{
    [RequireComponent(typeof(Camera))]
    public class MazeCameraFollow : MonoBehaviour
    {
        public Transform target;
        [Min(0.01f)] public float smoothTime = 0.15f;
        public Vector2 mapMin = new Vector2(-10.5f, -10.5f);
        public Vector2 mapMax = new Vector2(10.5f, 10.5f);
        private Camera view;
        private Vector3 velocity;

        private void Awake() => view = GetComponent<Camera>();

        private void Start()
        {
            if (target != null) transform.position = DesiredPosition();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position,
                DesiredPosition(), ref velocity, smoothTime);
        }

        private Vector3 DesiredPosition()
        {
            float halfHeight = view.orthographicSize;
            float halfWidth = halfHeight * view.aspect;
            float x = ClampAxis(target.position.x, mapMin.x, mapMax.x, halfWidth);
            float y = ClampAxis(target.position.y, mapMin.y, mapMax.y, halfHeight);
            return new Vector3(x, y, -10f);
        }

        private static float ClampAxis(float value, float min, float max, float halfView)
        {
            return max - min <= halfView * 2f
                ? (min + max) * 0.5f
                : Mathf.Clamp(value, min + halfView, max - halfView);
        }
    }
}
