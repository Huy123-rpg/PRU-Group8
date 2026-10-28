using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PRU.Biology
{
    // Added by enemy/boss Setup; no manual scene references are needed.
    public class BiologyEnemyHealthBars : MonoBehaviour
    {
        private static Canvas sharedCanvas;
        private static RectTransform canvasRect;
        private BiologySurvEnemy enemy;
        private BiologySurvBoss boss;
        private SpriteRenderer body;
        private RectTransform bar;
        private RectTransform fill;
        private TMP_Text label;
        private int maximum;
        private int previous;
        private float visibleUntil;
        private bool isBoss;
        private const float EnemyWidth = 48f;

        public void Initialize(BiologySurvEnemy target, int initialHp)
        {
            enemy = target;
            InitializeBar(initialHp, false);
        }

        public void Initialize(BiologySurvBoss target, int initialHp)
        {
            boss = target;
            InitializeBar(initialHp, true);
        }

        private void InitializeBar(int initialHp, bool bossBar)
        {
            isBoss = bossBar;
            maximum = Mathf.Max(1, initialHp);
            previous = initialHp;
            body = GetComponent<SpriteRenderer>();
            EnsureCanvas();

            GameObject root = new GameObject(isBoss ? "BossHealthBar" : "EnemyHealthBar");
            root.transform.SetParent(sharedCanvas.transform, false);
            bar = root.AddComponent<RectTransform>();
            bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 0.5f);
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.sizeDelta = isBoss ? new Vector2(360f, 36f) : new Vector2(EnemyWidth, 7f);
            Image background = root.AddComponent<Image>();
            background.color = new Color(0.025f, 0.04f, 0.035f, 0.92f);
            background.raycastTarget = false;
            Outline outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(0.70f, 0.52f, 0.22f, 0.95f);
            outline.effectDistance = Vector2.one;

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(root.transform, false);
            fill = fillObject.AddComponent<RectTransform>();
            fill.anchorMin = new Vector2(0f, 0.5f);
            fill.anchorMax = new Vector2(0f, 0.5f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.anchoredPosition = new Vector2(2f, 0f);
            fill.sizeDelta = new Vector2((isBoss ? 360f : EnemyWidth) - 4f,
                isBoss ? 30f : 3f);
            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.color = isBoss
                ? new Color(0.65f, 0.12f, 0.22f) : new Color(0.90f, 0.22f, 0.23f);
            fillImage.raycastTarget = false;

            if (isBoss)
            {
                GameObject textObject = new GameObject("BossHPText");
                textObject.transform.SetParent(root.transform, false);
                RectTransform textRect = textObject.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = textRect.offsetMax = Vector2.zero;
                label = textObject.AddComponent<TextMeshProUGUI>();
                FantasyUIFont.Apply(label);
                label.fontSize = 17;
                label.alignment = TextAlignmentOptions.Center;
                label.color = new Color(1f, 0.95f, 0.80f);
            }
            root.SetActive(isBoss);
        }

        private static void EnsureCanvas()
        {
            if (sharedCanvas != null) return;
            GameObject root = new GameObject("EnemyHealthBarsCanvas");
            sharedCanvas = root.AddComponent<Canvas>();
            sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            sharedCanvas.sortingOrder = 1;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasRect = root.GetComponent<RectTransform>();
            // No GraphicRaycaster: bars must never intercept gameplay clicks.
        }

        private void LateUpdate()
        {
            if (bar == null) return;
            int hp = isBoss ? (boss != null ? boss.HpLeft : 0)
                            : (enemy != null ? enemy.Hp : 0);
            bool dead = isBoss ? boss == null || boss.IsDead
                               : enemy == null || enemy.IsDead;
            if (dead || hp <= 0)
            {
                bar.gameObject.SetActive(false);
                return;
            }

            if (hp < previous) visibleUntil = Time.time + 2.5f;
            previous = hp;
            // Vampiric enemies can heal above their initial HP in existing code.
            // Expand the displayed capacity without changing their gameplay HP.
            maximum = Mathf.Max(maximum, hp);
            float ratio = Mathf.Clamp01((float)hp / maximum);
            fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                ((isBoss ? 360f : EnemyWidth) - 4f) * ratio);

            if (isBoss)
            {
                bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 1f);
                bar.anchoredPosition = new Vector2(0f, -84f);
                label.text = "BOSS  " + hp + " / " + maximum;
                return;
            }

            Camera camera = Camera.main;
            if (camera == null || Time.time >= visibleUntil)
            {
                bar.gameObject.SetActive(false);
                return;
            }
            Vector3 head = body != null
                ? new Vector3(body.bounds.center.x, body.bounds.max.y + 0.12f,
                    transform.position.z)
                : transform.position + Vector3.up;
            Vector3 viewport = camera.WorldToViewportPoint(head);
            bool onscreen = viewport.z > 0f && viewport.x > 0f && viewport.x < 1f
                && viewport.y > 0f && viewport.y < 1f;
            bar.gameObject.SetActive(onscreen);
            if (onscreen)
                bar.anchoredPosition = new Vector2(
                    (viewport.x - 0.5f) * canvasRect.rect.width,
                    (viewport.y - 0.5f) * canvasRect.rect.height);
        }

        private void OnDestroy()
        {
            if (bar != null) Destroy(bar.gameObject);
        }
    }
}
