using System.Collections;
using UnityEngine;

namespace PRU.Biology
{
    public class BiologyArenaSkin : MonoBehaviour
    {
        [Header("Ảnh nền sân đấu fantasy")]
        public Sprite arenaBackground;

        [Header("Kích thước sân nền, khớp nền cũ")]
        public Vector2 arenaSize = new Vector2(60f, 40f);

        public bool hideOldPaperDecoration = true;

        private IEnumerator Start()
        {
            if (arenaBackground == null)
            {
                Debug.LogWarning("BiologyArenaSkin: chưa gán ảnh nền, giữ nền cũ.", this);
                yield break;
            }

            // Bootstrap creates the manager, whose Start builds ArenaBg later.
            GameObject arena = null;
            float deadline = Time.realtimeSinceStartup + 10f;
            while (arena == null && Time.realtimeSinceStartup < deadline)
            {
                arena = GameObject.Find("ArenaBg");
                if (arena == null) yield return null;
            }

            if (arena == null)
            {
                Debug.LogWarning("BiologyArenaSkin: không tìm thấy ArenaBg.", this);
                yield break;
            }

            SpriteRenderer renderer = arena.GetComponent<SpriteRenderer>();
            if (renderer == null) yield break;

            renderer.sprite = arenaBackground;
            renderer.color = Color.white;
            renderer.sortingOrder = -100;

            Vector2 spriteSize = arenaBackground.bounds.size;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                arena.transform.localScale = new Vector3(
                    arenaSize.x / spriteSize.x,
                    arenaSize.y / spriteSize.y,
                    1f);
            }

            if (hideOldPaperDecoration)
            {
                GameObject fxRoot = GameObject.Find("FxRoot");
                if (fxRoot != null)
                {
                    foreach (SpriteRenderer decoration in
                             fxRoot.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (decoration.name.StartsWith("Paper_"))
                            decoration.enabled = false;
                    }
                }
            }
        }
    }
}
