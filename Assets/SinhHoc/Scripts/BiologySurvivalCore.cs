// ============================================================
// BiologySurvivalCore.cs
// Các thực thể của game SINH TỒN MIỄN DỊCH:
//   • BiologyWhiteCell      - người chơi: WASD + chuột ngắm + chuột trái bắn
//                             + SPACE dash (né đòn, bất tử lúc dash)
//   • BiologySurvProjectile - kháng thể người chơi bắn ra (có chí mạng)
//   • BiologyEnemyBullet    - gai độc mầm bệnh/boss bắn vào người chơi
//   • BiologySurvEnemy      - lớp cơ sở của mầm bệnh, 3 loại AI:
//                             Chaser (đuổi) / Shooter (bắn xa) / Charger (lao vào)
//                             + modifier ELITE: Fast / Armored / Vampiric / Explosive
//   • BiologySurvBoss       - boss 3 phase: thường → Knowledge Clash (70% HP)
//                             → ENRAGED (30% HP)
//   • BiologyXpOrb          - bong bóng XP rơi ra khi mầm bệnh chết
//
// ⚠ QUY TẮC PHYSICS2D (quan trọng để OnTriggerEnter2D hoạt động):
//   Để 2 Collider2D (đều là Trigger) tạo ra callback OnTriggerEnter2D,
//   ÍT NHẤT MỘT trong 2 object phải có Rigidbody2D.
//   Vì vậy: đạn (cả đạn player & đạn địch) luôn được gắn Rigidbody2D
//   (kinematic, không chịu gravity), và di chuyển bằng linearVelocity
//   thay vì transform.position để physics engine luôn biết vị trí mới nhất.
//   Enemy/Boss cũng được gắn Rigidbody2D khi spawn (xem BiologySurvivalManager).
// ============================================================
using System.Collections.Generic;
using UnityEngine;

namespace PRU.Biology
{
    // Loại hình dạng tế bào (dùng chung cho sprite factory)
    public enum BiologyTowerType { Macrophage = 0, LymphoB = 1, LymphoT = 2 }

    // Loại mầm bệnh
    public enum BiologyEnemyKind { Chaser = 0, Shooter = 1, Charger = 2 }

    // Modifier elite
    public enum BiologyEliteKind { None = 0, Fast = 1, Armored = 2, Vampiric = 3, Explosive = 4 }

    // ---------------- TIỆN ÍCH PHYSICS2D DÙNG CHUNG ----------------
    public static class BiologyPhysics2DUtil
    {
        /// <summary>
        /// Gắn Rigidbody2D kiểu kinematic (không gravity) - bắt buộc để trigger
        /// giữa 2 collider trigger hoạt động, và để đạn di chuyển bằng velocity.
        /// </summary>
        public static Rigidbody2D AddKinematicBody(GameObject go)
        {
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.freezeRotation = true;
            return rb;
        }
    }

    // ---------------- NGƯỜI CHƠI: TẾ BÀO TRẮNG ----------------
    public class BiologyWhiteCell : MonoBehaviour
    {
        public static BiologyWhiteCell Instance { get; private set; }

        public int CurrentHp { get; private set; }
        public int MaxHp => BiologyGameConfig.PLAYER_MAX_HP;
        public bool IsDead => CurrentHp <= 0;

        // ----- Nâng cấp (SurvivalManager cộng dồn khi chọn nâng cấp) -----
        public float ShootInterval = BiologyGameConfig.SHOOT_INTERVAL_BASE;
        public int   ProjectileDmg = BiologyGameConfig.PROJECTILE_DMG_BASE;
        public float ProjectileSpeed = BiologyGameConfig.PROJECTILE_SPEED_BASE;
        public int   ProjectileCount = 1;      // số viên bắn mỗi lượt
        public float Pierce = 0f;              // số mầm bệnh xuyên qua thêm
        public float MagnetRadius = 2.2f;      // bán kính hút XP
        public float MoveSpeed = BiologyGameConfig.PLAYER_SPEED;
        public float OrbBonus = 0f;            // XP cộng thêm khi ăn
        public float CritChance = BiologyGameConfig.CRIT_CHANCE_BASE; // 0..1
        public int   LifestealPerKill = 0;     // hồi máu mỗi lần giết
        public float DashCooldown = BiologyGameConfig.DASH_COOLDOWN;

        // ----- Vũ khí học tập (theme mới) -----
        // Manager đổi sprite theo phase: Pencil Blaster → Ruler Blade → Book Orbit
        // → Calculator Lightning → Chemical Flask. Đạn giữ nguyên physics/logic cũ.
        public Sprite CurrentWeaponSprite { get; set; }

        // ----- Dash -----
        private float _dashTimer;      // còn bao lâu nữa hết dash
        private float _dashCdTimer;    // hồi chiêu
        private Vector2 _lastMoveDir = Vector2.right;
        public float DashCooldownRatio => Mathf.Clamp01(_dashCdTimer / DashCooldown);

        private float _shootTimer;
        private float _ifTimer;
        private SpriteRenderer _sr;
        private float _blinkTimer;
        private Transform _crosshair;

        public event System.Action<int, int> OnHpChanged;
        public event System.Action OnPlayerDied;

        private void Awake()
        {
            Instance = this;
            CurrentHp = MaxHp; // khởi tạo ngay để HUD hiện đúng từ frame đầu
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();

            // Người chơi có collider (tạo trong manager) → cần Rigidbody2D
            // để va chạm trigger với đạn địch hoạt động ổn định.
            BiologyPhysics2DUtil.AddKinematicBody(gameObject);

            // Tâm ngắm theo chuột
            GameObject xh = new GameObject("Crosshair");
            SpriteRenderer xsr = xh.AddComponent<SpriteRenderer>();
            xsr.sprite = BiologySpriteFactory.GetCrosshairSprite();
            xsr.sortingOrder = 50;
            xsr.color = new Color(1f, 1f, 1f, 0.85f);
            _crosshair = xh.transform;
        }

        private void Update()
        {
            BiologySurvivalManager mgr = BiologySurvivalManager.Instance;
            if (mgr != null && mgr.IsPaused) return;
            if (IsDead) { if (_crosshair != null) _crosshair.gameObject.SetActive(false); return; }

            // ----- Ngắm -----
            Vector3 mouseWorld = GetMouseWorld();

            // ----- Di chuyển WASD / mũi tên -----
            float h = 0f, v = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
            bool dashPressed = Input.GetKeyDown(KeyCode.Space);
            bool shooting = Input.GetMouseButton(0);
#elif ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
            }
            bool dashPressed = kb != null && kb.spaceKey.wasPressedThisFrame;
            var mouse = UnityEngine.InputSystem.Mouse.current;
            bool shooting = mouse != null && mouse.leftButton.isPressed;
#endif
            Vector3 dir = new Vector3(h, v, 0f).normalized;
            if (dir.sqrMagnitude > 0.01f) _lastMoveDir = dir;

            float speed = MoveSpeed;
            // AWAKENING MODE
            if (mgr != null && mgr.IsAwakened) speed *= BiologyGameConfig.AWAKENING_SPEED_MULT;

            // ----- Dash (SPACE) -----
            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                transform.position += (Vector3)(_dashDir * (BiologyGameConfig.DASH_SPEED * Time.deltaTime));
            }
            else
            {
                transform.position += dir * (speed * Time.deltaTime);
                if (dashPressed && _dashCdTimer <= 0f)
                {
                    _dashDir = dir.sqrMagnitude > 0.01f ? (Vector2)dir : _lastMoveDir;
                    _dashTimer = BiologyGameConfig.DASH_TIME;
                    _dashCdTimer = DashCooldown;
                    _ifTimer = Mathf.Max(_ifTimer, BiologyGameConfig.DASH_TIME + 0.12f); // bất tử khi dash
                }
            }
            if (_dashCdTimer > 0f) _dashCdTimer -= Time.deltaTime;

            // ----- Bắn theo chuột -----
            float interval = ShootInterval;
            if (mgr != null && mgr.IsAwakened) interval /= BiologyGameConfig.AWAKENING_ATKSPD_MULT;

            _shootTimer -= Time.deltaTime;
            if (shooting && _shootTimer <= 0f)
            {
                ShootToward((Vector2)mouseWorld);
                _shootTimer = interval;
            }

            // ----- Nhấp nháy khi bất tử -----
            if (_ifTimer > 0f)
            {
                _ifTimer -= Time.deltaTime;
                _blinkTimer += Time.deltaTime;
                if (_sr != null) _sr.enabled = Mathf.FloorToInt(_blinkTimer * 12f) % 2 == 0;
            }
            else if (_sr != null && !_sr.enabled)
            {
                _sr.enabled = true;
            }

            // ----- Tâm ngắm -----
            if (_crosshair != null)
            {
                _crosshair.position = new Vector3(mouseWorld.x, mouseWorld.y, 0f);
            }
        }

        private Vector2 _dashDir = Vector2.right;

        private static Vector3 GetMouseWorld()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            Vector3 mp = Input.mousePosition;
#else
            Vector3 mp = UnityEngine.InputSystem.Mouse.current != null
                ? (Vector3)UnityEngine.InputSystem.Mouse.current.position.ReadValue()
                : new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
#endif
            Camera cam = Camera.main;
            if (cam == null) return Vector3.zero;
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, -cam.transform.position.z));
            return new Vector3(w.x, w.y, 0f);
        }

        private void ShootToward(Vector2 target)
        {
            Vector2 origin = transform.position;
            Vector2 baseDir = (target - origin).normalized;
            float spreadStep = 12f; // độ lệch giữa các viên khi bắn nhiều viên

            for (int i = 0; i < ProjectileCount; i++)
            {
                float angleOffset = (i - (ProjectileCount - 1) / 2f) * spreadStep;
                Vector2 dir = Rotate(baseDir, angleOffset);

                GameObject go = new GameObject($"Antibody_{i}");
                go.transform.position = origin;
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = CurrentWeaponSprite != null
                    ? CurrentWeaponSprite
                    : BiologySpriteFactory.GetWeaponSprite(0); // mặc định: Pencil Blaster
                sr.sortingOrder = 6;

                CircleCollider2D col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.22f;

                bool crit = Random.value < CritChance;
                int dmg = Mathf.Max(1, Mathf.RoundToInt(ProjectileDmg * (crit ? BiologyGameConfig.CRIT_DMG_MULT : 1f)));

                // AWAKENING: sát thương x2
                BiologySurvivalManager mgr = BiologySurvivalManager.Instance;
                if (mgr != null && mgr.IsAwakened) dmg = Mathf.RoundToInt(dmg * BiologyGameConfig.AWAKENING_DMG_MULT);

                BiologySurvProjectile p = go.AddComponent<BiologySurvProjectile>();
                p.Init(dir, ProjectileSpeed, dmg, Mathf.FloorToInt(Pierce), crit);
            }
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(r), sin = Mathf.Sin(r);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        public void TakeDamage(int dmg)
        {
            if (IsDead || _ifTimer > 0f) return;
            CurrentHp = Mathf.Max(0, CurrentHp - dmg);
            _ifTimer = BiologyGameConfig.IFRAME_TIME;
            _blinkTimer = 0f;
            OnHpChanged?.Invoke(CurrentHp, MaxHp);
            if (CurrentHp <= 0) OnPlayerDied?.Invoke();
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
            OnHpChanged?.Invoke(CurrentHp, MaxHp);
        }
    }

    // ---------------- KHÁNG THỂ (đạn người chơi) ----------------
    public class BiologySurvProjectile : MonoBehaviour
    {
        private Vector2 _dir;
        private float _speed;
        private int _dmg;
        private int _pierce;
        private bool _crit;

        private readonly HashSet<BiologySurvEnemy> _hitEnemies = new HashSet<BiologySurvEnemy>();
        private readonly HashSet<BiologySurvBoss> _hitBosses = new HashSet<BiologySurvBoss>();

        public void Init(Vector2 dir, float speed, int dmg, int pierce, bool crit)
        {
            _dir = dir;
            _speed = speed;
            _dmg = dmg;
            _pierce = pierce;
            _crit = crit;
            Destroy(gameObject, 1.6f);

            // BẮT BUỘC: Rigidbody2D để OnTriggerEnter2D hoạt động giữa 2 trigger,
            // đồng thời di chuyển bằng linearVelocity để physics theo kịp vị trí.
            Rigidbody2D rb = BiologyPhysics2DUtil.AddKinematicBody(gameObject);
            rb.linearVelocity = _dir.normalized * _speed;

            if (crit) GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0.2f); // đạn crit màu cam

            Debug.Log($"[Bullet] Spawn tại {transform.position}, dir={_dir}, speed={_speed}, dmg={_dmg}, pierce={_pierce}, crit={_crit}");
        }

        private void Update()
        {
            // Giữ hướng & tốc độ không đổi. Nếu vì lý do nào đó velocity bị reset
            // (ví dụ spawn trùng frame) thì áp lại.
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.sqrMagnitude < 0.01f)
                rb.linearVelocity = _dir.normalized * _speed;

            // Xoay sprite theo hướng bay (chỉ hình ảnh, không ảnh hưởng physics)
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg + 90f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[Bullet] Trigger với '{other.name}' (layer={LayerMask.LayerToName(other.gameObject.layer)}, tag='{other.tag}')");

            BiologySurvEnemy enemy = other.GetComponentInParent<BiologySurvEnemy>();
            if (enemy != null && !enemy.IsDead && !_hitEnemies.Contains(enemy))
            {
                _hitEnemies.Add(enemy);
                Debug.Log($"[Bullet] GỌI TakeDamage({_dmg}) lên enemy '{enemy.name}' (HP trước: {enemy.Hp})");
                enemy.TakeDamage(_dmg);
                HandlePierce("enemy");
                return;
            }

            BiologySurvBoss boss = other.GetComponentInParent<BiologySurvBoss>();
            if (boss != null && !boss.IsDead && !_hitBosses.Contains(boss))
            {
                _hitBosses.Add(boss);
                Debug.Log($"[Bullet] GỌI TakeDamage({_dmg}) lên BOSS (HP trước: {boss.HpLeft})");
                boss.TakeDamage(_dmg);
                HandlePierce("boss");
            }
        }

        private void HandlePierce(string targetName)
        {
            if (_pierce > 0)
            {
                _pierce--;
                Debug.Log($"[Bullet] Xuyên qua {targetName}, còn pierce = {_pierce}");
            }
            else
            {
                Debug.Log($"[Bullet] Hết pierce → tự hủy sau khi trúng {targetName}.");
                Destroy(gameObject);
            }
        }
    }

    // ---------------- ĐẠN ĐỊCH (gai độc bắn vào người chơi) ----------------
    public class BiologyEnemyBullet : MonoBehaviour
    {
        private Vector2 _dir;
        private float _speed;
        private int _dmg;

        public void Init(Vector2 dir, float speed, int dmg)
        {
            _dir = dir;
            _speed = speed;
            _dmg = dmg;
            Destroy(gameObject, 4f);

            // BẮT BUỘC: Rigidbody2D để trigger với collider người chơi hoạt động.
            Rigidbody2D rb = BiologyPhysics2DUtil.AddKinematicBody(gameObject);
            rb.linearVelocity = _dir.normalized * _speed;
        }

        private void Update()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.sqrMagnitude < 0.01f)
                rb.linearVelocity = _dir.normalized * _speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BiologyWhiteCell pc = other.GetComponent<BiologyWhiteCell>();
            if (pc != null && !pc.IsDead)
            {
                pc.TakeDamage(_dmg);
                Destroy(gameObject);
            }
        }
    }

    // ---------------- MẦM BỆNH (lớp cơ sở 3 loại AI) ----------------
    public class BiologySurvEnemy : MonoBehaviour
    {
        public static readonly List<BiologySurvEnemy> All = new List<BiologySurvEnemy>();

        public BiologyEnemyKind Kind { get; private set; }
        public bool IsDead => Hp <= 0;
        public bool IsElite => EliteKind != BiologyEliteKind.None;
        public BiologyEliteKind EliteKind { get; private set; }
        public int Hp { get; private set; }

        protected float Speed;
        protected int Dmg;
        public float XpValue { get; private set; }      // XP rơi ra khi chết (manager đọc)
        public int PointsValue { get; private set; }    // điểm khi giết (manager đọc)
        protected float _touchCooldown;
        private SpriteRenderer _sr;
        private Color _baseColor = Color.white;
        private static readonly Color EliteTint = new Color(1f, 0.82f, 0.3f);

        protected BiologyWhiteCell Pc => BiologyWhiteCell.Instance;

        private void Awake() => All.Add(this);
        private void OnDestroy() => All.Remove(this);

        public void Setup(BiologyEnemyKind kind, int hp, float speed, int dmg, float xp, int points,
                          BiologyEliteKind elite = BiologyEliteKind.None)
        {
            Kind = kind;
            Hp = hp;
            Dmg = dmg;
            XpValue = xp;
            PointsValue = points;
            EliteKind = elite;

            switch (elite)
            {
                case BiologyEliteKind.Fast: Speed = speed * 1.5f; break;
                case BiologyEliteKind.Armored: Speed = speed * 0.8f; break;
                default: Speed = speed; break;
            }

            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;

            if (IsElite)
            {
                transform.localScale *= BiologyGameConfig.ELITE_SCALE;
                if (_sr != null) _sr.color = Color.Lerp(_baseColor, EliteTint, 0.65f);
                _baseColor = _sr != null ? _sr.color : _baseColor;
            }
        }

        protected virtual void Behave() { }

        // (OnTouchedPlayer được định nghĩa phía dưới - Vampiric hút máu)

        private void Update()
        {
            if (BiologySurvivalManager.Instance != null && BiologySurvivalManager.Instance.IsPaused) return;
            if (Pc == null || Pc.IsDead) return;

            Behave();

            // Chạm người chơi
            _touchCooldown -= Time.deltaTime;
            if (_touchCooldown <= 0f &&
                Vector2.Distance(transform.position, Pc.transform.position) < 0.75f)
            {
                Pc.TakeDamage(Dmg);
                OnTouchedPlayer();
                _touchCooldown = BiologyGameConfig.PATHOGEN_TOUCH_COOLDOWN;
            }
        }

        public void TakeDamage(int dmg)
        {
            if (IsDead) return;
            if (EliteKind == BiologyEliteKind.Armored) dmg = Mathf.Max(1, dmg - 1); // Giáp: giảm 1 sát thương

            Hp -= dmg;
            Debug.Log($"[Enemy] '{name}' nhận {dmg} damage → HP còn {Hp}");
            StartCoroutine(Flash());
            if (Hp <= 0)
            {
                Debug.Log($"[Enemy] '{name}' CHẾT → rơi XP + cộng điểm.");
                if (EliteKind == BiologyEliteKind.Explosive) TriggerExplosion();
                BiologySurvivalManager.Instance?.OnEnemyKilled(this);
                Destroy(gameObject);
            }
        }

        /// <summary>Gọi khi chạm người chơi - Vampiric hút máu để hồi chính nó.</summary>
        private void OnTouchedPlayer()
        {
            if (EliteKind == BiologyEliteKind.Vampiric)
            {
                Hp += 2; // hút máu người chơi để tự hồi
                StartCoroutine(Flash());
            }
        }

        /// <summary>Elite Explosive: nổ khi chết, gây sát thương nếu người chơi đứng gần.</summary>
        private void TriggerExplosion()
        {
            const float radius = 2.2f;
            const int explosionDmg = 2;

            GameObject fx = new GameObject("ExplosionFx");
            fx.transform.position = transform.position;
            SpriteRenderer sr = fx.AddComponent<SpriteRenderer>();
            sr.sprite = BiologySpriteFactory.GetWhiteSprite();
            sr.color = new Color(1f, 0.45f, 0.15f, 0.75f);
            sr.sortingOrder = 7;
            fx.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            fx.AddComponent<BiologyExplosionFx>();

            BiologyWhiteCell pc = BiologyWhiteCell.Instance;
            if (pc != null && !pc.IsDead &&
                Vector2.Distance(transform.position, pc.transform.position) < radius)
            {
                pc.TakeDamage(explosionDmg);
            }
        }

        private System.Collections.IEnumerator Flash()
        {
            if (_sr == null) yield break;
            _sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (_sr != null) _sr.color = _baseColor;
        }

        /// <summary>Đạn địch dùng chung (Shooter & Boss).</summary>
        public static void FireBullet(Vector2 from, Vector2 dir, float speed, int dmg)
        {
            GameObject go = new GameObject("EnemyBullet");
            go.transform.position = from;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BiologySpriteFactory.GetEnemyBulletSprite();
            sr.sortingOrder = 6;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.18f;
            go.AddComponent<BiologyEnemyBullet>().Init(dir.normalized, speed, dmg);
        }
    }

    // ---------------- LOẠI 1: ĐUỔI THEO ----------------
    public class BiologyChaser : BiologySurvEnemy
    {
        protected override void Behave()
        {
            Vector3 dir = (Pc.transform.position - transform.position).normalized;
            transform.position += dir * (Speed * Time.deltaTime);
        }
    }

    // ---------------- LOẠI 2: BẮN XA (giữ khoảng cách, nhả gai độc) ----------------
    public class BiologyShooter : BiologySurvEnemy
    {
        private float _fireTimer;

        protected override void Behave()
        {
            Vector3 toPlayer = Pc.transform.position - transform.position;
            float dist = toPlayer.magnitude;
            Vector3 dir = toPlayer.normalized;

            float keep = BiologyGameConfig.SHOOTER_KEEP_DIST;
            if (dist > keep) transform.position += dir * (Speed * Time.deltaTime);
            else if (dist < keep * 0.65f) transform.position -= dir * (Speed * Time.deltaTime);

            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f && dist < keep + 3f)
            {
                FireBullet(transform.position, dir, BiologyGameConfig.ENEMY_BULLET_SPEED,
                           BiologyGameConfig.ENEMY_BULLET_DMG + (IsElite ? BiologyGameConfig.ELITE_DMG_BONUS : 0));
                _fireTimer = BiologyGameConfig.SHOOTER_FIRE_INTERVAL;
            }
        }
    }

    // ---------------- LOẠI 3: LAO VÀO (cảnh báo rồi lao nhanh) ----------------
    public class BiologyCharger : BiologySurvEnemy
    {
        private enum State { Walk, Warn, Charge, Rest }
        private State _state = State.Walk;
        private float _stateTimer;
        private Vector2 _chargeDir;
        private SpriteRenderer _sr;
        private Color _myColor = Color.white;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _myColor = _sr.color; // giữ màu gốc (có thể là màu elite)
        }

        protected override void Behave()
        {
            switch (_state)
            {
                case State.Walk:
                {
                    Vector3 dir = (Pc.transform.position - transform.position).normalized;
                    transform.position += dir * (Speed * Time.deltaTime);
                    if (Vector2.Distance(transform.position, Pc.transform.position) < BiologyGameConfig.CHARGER_TRIGGER_DIST)
                    {
                        _state = State.Warn;
                        _stateTimer = BiologyGameConfig.CHARGER_WARN_TIME;
                        _chargeDir = dir;
                    }
                    break;
                }
                case State.Warn: // nhấp nháy cảnh báo
                {
                    _stateTimer -= Time.deltaTime;
                    if (_sr != null)
                        _sr.color = Mathf.FloorToInt(_stateTimer * 14f) % 2 == 0
                            ? new Color(1f, 0.25f, 0.2f) : _myColor;
                    if (_stateTimer <= 0f)
                    {
                        _state = State.Charge;
                        _stateTimer = BiologyGameConfig.CHARGER_CHARGE_TIME;
                    }
                    break;
                }
                case State.Charge:
                {
                    _stateTimer -= Time.deltaTime;
                    transform.position += (Vector3)(_chargeDir * (BiologyGameConfig.CHARGER_DASH_SPEED * Time.deltaTime));
                    if (_stateTimer <= 0f)
                    {
                        _state = State.Rest;
                        _stateTimer = BiologyGameConfig.CHARGER_REST_TIME;
                    }
                    break;
                }
                case State.Rest:
                {
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        _state = State.Walk;
                        if (_sr != null) _sr.color = _myColor;
                    }
                    break;
                }
            }
        }
    }

    // ---------------- BOSS: THE CORRUPTED PATHOGEN ----------------
    public class BiologySurvBoss : MonoBehaviour
    {
        public bool IsDead { get; private set; }
        public bool IsStunned { get; private set; }
        public bool IsEnraged { get; private set; }
        public bool ClashDone { get; private set; }

        private int _hp;
        private int _hpMax;
        private float _speed;
        private float _attackTimer;
        private float _stunTimer;
        private SpriteRenderer _sr;

        /// <summary>Máu còn lại của boss (để debug/logging).</summary>
        public int HpLeft => _hp;

        public void Setup(int hp, float speed)
        {
            _hp = hp;
            _hpMax = hp;
            _speed = speed;
            _attackTimer = 2.5f;
        }

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        private void Update()
        {
            if (BiologySurvivalManager.Instance != null && BiologySurvivalManager.Instance.IsPaused) return;
            if (IsDead) return;

            // Choáng sau khi player trả lời đúng Knowledge Clash
            if (IsStunned)
            {
                _stunTimer -= Time.deltaTime;
                if (_stunTimer <= 0f)
                {
                    IsStunned = false;
                    if (_sr != null) _sr.color = Color.white;
                }
                return;
            }

            BiologyWhiteCell pc = BiologyWhiteCell.Instance;
            if (pc == null || pc.IsDead) return;

            // Đuổi theo
            float speed = _speed * (IsEnraged ? BiologyGameConfig.BOSS_ENRAGE_SPEED_MULT : 1f);
            Vector3 dir = (pc.transform.position - transform.position).normalized;
            transform.position += dir * (speed * Time.deltaTime);

            // Chạm người chơi
            if (Vector2.Distance(transform.position, pc.transform.position) < 1.1f)
                pc.TakeDamage(BiologyGameConfig.BOSS_DMG);

            // Bắn vòng tròn đạn
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                int bullets = IsEnraged ? 8 + BiologyGameConfig.BOSS_ENRAGE_EXTRA_BULLET * 2 : 8;
                float dmgMult = IsEnraged ? BiologyGameConfig.BOSS_ENRAGE_ATK_MULT : 1f;
                RadialBurst(bullets, BiologyGameConfig.ENEMY_BULLET_SPEED, dmgMult);
                _attackTimer = IsEnraged ? 1.6f : 2.6f;
            }
        }

        public void TakeDamage(int dmg)
        {
            if (IsDead) return;
            _hp -= dmg;
            Debug.Log($"[Boss] Nhận {dmg} damage → HP còn {_hp}/{_hpMax}");
            StartCoroutine(Flash());

            float frac = _hp / (float)_hpMax;

            // 70% HP → KNOWLEDGE CLASH (chỉ 1 lần)
            if (!ClashDone && frac <= BiologyGameConfig.BOSS_CLASH_HP_THRESHOLD)
            {
                ClashDone = true;
                BiologySurvivalManager.Instance?.StartKnowledgeClash(this);
            }

            // 30% HP → ENRAGED
            if (!IsEnraged && frac <= BiologyGameConfig.BOSS_ENRAGE_THRESHOLD)
            {
                IsEnraged = true;
                if (_sr != null) _sr.color = new Color(1f, 0.4f, 0.4f); // đỏ dữ tợn
                BiologySurvivalManager.Instance?.NotifyBossEnraged();
            }

            if (_hp <= 0)
            {
                IsDead = true;
                Debug.Log("[Boss] CHẾT!");
                BiologySurvivalManager.Instance?.OnBossKilled(BiologyGameConfig.BOSS_XP);
                Destroy(gameObject);
            }
        }

        /// <summary>Player trả lời đúng → boss bị choáng, không tấn công.</summary>
        public void Stun(float duration)
        {
            IsStunned = true;
            _stunTimer = duration;
            if (_sr != null) _sr.color = new Color(0.6f, 0.6f, 0.8f); // xám choáng
        }

        /// <summary>Player trả lời sai → boss tung Ultimate (vòng đạn lớn).</summary>
        public void CastUltimate()
        {
            if (IsDead) return;
            RadialBurst(16, BiologyGameConfig.ENEMY_BULLET_SPEED * 1.15f, BiologyGameConfig.BOSS_ULT_DAMAGE);
        }

        private void RadialBurst(int count, float speed, float dmg)
        {
            for (int i = 0; i < count; i++)
            {
                float a = i * (360f / count) + Random.Range(0f, 18f);
                Vector2 dir = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
                BiologySurvEnemy.FireBullet(transform.position, dir, speed, Mathf.Max(1, Mathf.RoundToInt(dmg)));
            }
        }

        private System.Collections.IEnumerator Flash()
        {
            Color baseCol = IsEnraged ? new Color(1f, 0.4f, 0.4f)
                          : IsStunned ? new Color(0.6f, 0.6f, 0.8f) : Color.white;
            if (_sr != null) _sr.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            if (_sr != null) _sr.color = baseCol;
        }
    }

    // ---------------- HIỆU ỨNG NỔ (elite Explosive chết) ----------------
    public class BiologyExplosionFx : MonoBehaviour
    {
        private void Update()
        {
            float s = transform.localScale.x + Time.deltaTime * 7f;
            transform.localScale = new Vector3(s, s, 1f);
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a -= Time.deltaTime * 2.2f;
                sr.color = c;
                if (c.a <= 0f) Destroy(gameObject);
            }
            else Destroy(gameObject);
        }
    }

    // ---------------- BONG BÓNG XP ----------------
    public class BiologyXpOrb : MonoBehaviour
    {
        private float _value;
        private bool _magnetized;

        public void Setup(float value) => _value = value;

        private void Update()
        {
            if (BiologySurvivalManager.Instance != null && BiologySurvivalManager.Instance.IsPaused) return;

            BiologyWhiteCell pc = BiologyWhiteCell.Instance;
            if (pc == null) return;

            float dist = Vector2.Distance(transform.position, pc.transform.position);

            if (!_magnetized && dist < pc.MagnetRadius) _magnetized = true;
            if (_magnetized)
            {
                Vector3 dir = (pc.transform.position - transform.position).normalized;
                transform.position += dir * (8f * Time.deltaTime);
            }

            if (dist < 0.4f)
            {
                float xp = _value + pc.OrbBonus;
                BiologySurvivalManager mgr = BiologySurvivalManager.Instance;
                if (mgr != null)
                {
                    if (mgr.StreakXpBonus > 0f) xp *= 1f + mgr.StreakXpBonus;
                    if (mgr.IsAwakened) xp *= BiologyGameConfig.AWAKENING_XP_MULT;
                    mgr.GainXp(xp);
                }
                Destroy(gameObject);
            }
        }
    }
}
