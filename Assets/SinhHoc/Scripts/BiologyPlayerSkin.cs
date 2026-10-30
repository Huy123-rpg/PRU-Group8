using System.Collections;
using UnityEngine;

namespace PRU.Biology
{
    public class BiologyPlayerSkin : MonoBehaviour
    {
        public Sprite playerSprite;
        [Min(0.1f)] public float playerHeight = 1.4f;

        private Sprite runtimeSprite;

        private IEnumerator Start()
        {
            if (playerSprite == null)
            {
                Debug.LogWarning("BiologyPlayerSkin: chưa gán ảnh nhân vật, giữ hình cũ.", this);
                yield break;
            }

            float deadline = Time.realtimeSinceStartup + 10f;
            while (BiologyWhiteCell.Instance == null &&
                   Time.realtimeSinceStartup < deadline)
                yield return null;

            BiologyWhiteCell player = BiologyWhiteCell.Instance;
            if (player == null)
            {
                Debug.LogWarning("BiologyPlayerSkin: không tìm thấy người chơi.", this);
                yield break;
            }

            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            if (renderer == null) yield break;

            // Set the sprite's world height without scaling the player or collider.
            Rect rect = playerSprite.rect;
            runtimeSprite = Sprite.Create(playerSprite.texture, rect,
                new Vector2(0.5f, 0.5f), rect.height / Mathf.Max(0.1f, playerHeight),
                0, SpriteMeshType.FullRect);
            runtimeSprite.name = "FantasyPlayerRuntime";
            renderer.sprite = runtimeSprite;
        }

        private void OnDestroy()
        {
            if (runtimeSprite != null) Destroy(runtimeSprite);
        }
    }
}
