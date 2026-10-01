// ============================================================

// BiologySurvivalManager.cs

// "Bộ não" game SINH TỒN TRƯỜNG HỌC - môn Sinh Học

//

// LUỒNG CHƠI:

//   • DIFFICULTY PROGRESSION 5 phase (tất cả chỉnh được trong Inspector):

//       P1 (0-1'): ít quái, chậm, interval ~2.25s, max 7 alive

//       P2 (1-2'): mở khóa Shooter, max 12 alive

//       P3 (2-3'): mở khóa Charger, max 18 alive

//       P4 (3-4'): có ELITE, max 25 alive

//       P5 (4-5'): FINAL WAVE, max 30 alive

//     → KHÔNG BAO GIỜ spawn nếu số enemy sống >= maxAliveEnemies của phase.

//   • VOID THREAT SYSTEM: threat 0..100 tích từ thời gian sống, kill, elite kill,

//       level up, trả lời câu hỏi. Đạt 100% →

//       1) dừng spawn thường → 2) Boss Warning đếm ngược → 3) player chuẩn bị

//       → 4) spawn Boss → 5) Boss Fight. Sau khi boss chết threat reset một phần.

//   • Question difficulty theo THỜI GIAN game: sớm → Dễ, giữa → Trung bình,

//       muộn → Khó; Boss Knowledge Clash luôn dùng câu Khó (Boss).

//   • Đủ XP → LÊN CẤP: trả lời câu hỏi → chọn nâng cấp (giữ nguyên hệ thống cũ)

//   • Đúng liên tiếp → STREAK thưởng → 10 câu đúng = AWAKENING MODE

//   • Boss còn 70% HP → KNOWLEDGE CLASH; còn 30% → ENRAGED

//   • Sống sót 5 phút → CHIẾN THẮNG

// ============================================================

using System;

using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.EventSystems;

using Random = UnityEngine.Random; // tránh ambiguity khi có using System



namespace PRU.Biology

{

    // ----------------- Cấu hình 1 phase độ khó (Inspector) -----------------

    [Serializable]

    public class DifficultyPhase

    {

        [Tooltip("Tên phase (chỉ để hiển thị)")]

        public string label;

        [Tooltip("Thời điểm phase bắt đầu (giây)")]

        public float startTime;

        [Tooltip("Giây giữa 2 lần spawn")]

        public float spawnInterval;

        [Tooltip("TRẦN số enemy sống cùng lúc - không spawn nếu vượt")]

        public int maxAliveEnemies;

        [Tooltip("Hệ số nhân HP enemy trong phase này")]

        public float hpMultiplier;

        [Tooltip("Cho phép enemy loại Bắn xa (Shooter)")]

        public bool allowShooter;

        [Tooltip("Cho phép enemy loại Lao vào (Charger)")]

        public bool allowCharger;

        [Range(0f, 1f), Tooltip("Tỷ lệ enemy thành ELITE")]

        public float eliteChance;

        [HideInInspector] public int Index; // gán tự động

    }



    public class BiologySurvivalManager : MonoBehaviour

    {

        public static BiologySurvivalManager Instance { get; private set; }



        [Header("Tham chiếu (để trống sẽ tự tìm)")]

        public BiologyQuizUI quizUI;

        public EssayQuestionUI essayUI;

        public BiologyHUD hud;



        // ----------------- DIFFICULTY PROGRESSION (Inspector) -----------------

        [Header("=== DIFFICULTY PROGRESSION - 5 PHASE ===")]

        public DifficultyPhase[] Phases =

        {

            new DifficultyPhase { label = "P1 · 0-1' Nhe nhang",      startTime = BiologyGameConfig.PHASE1_TIME, spawnInterval = BiologyGameConfig.PHASE1_SPAWN_INTERVAL, maxAliveEnemies = BiologyGameConfig.PHASE1_MAX_ALIVE, hpMultiplier = BiologyGameConfig.PHASE1_HP_MULT, allowShooter = false, allowCharger = false, eliteChance = 0f },

            new DifficultyPhase { label = "P2 · 1-2' Shooter",         startTime = BiologyGameConfig.PHASE2_TIME, spawnInterval = BiologyGameConfig.PHASE2_SPAWN_INTERVAL, maxAliveEnemies = BiologyGameConfig.PHASE2_MAX_ALIVE, hpMultiplier = BiologyGameConfig.PHASE2_HP_MULT, allowShooter = true,  allowCharger = false, eliteChance = 0f },

            new DifficultyPhase { label = "P3 · 2-3' Day du loai",     startTime = BiologyGameConfig.PHASE3_TIME, spawnInterval = BiologyGameConfig.PHASE3_SPAWN_INTERVAL, maxAliveEnemies = BiologyGameConfig.PHASE3_MAX_ALIVE, hpMultiplier = BiologyGameConfig.PHASE3_HP_MULT, allowShooter = true,  allowCharger = true,  eliteChance = 0f },

            new DifficultyPhase { label = "P4 · 3-4' Elite",           startTime = BiologyGameConfig.PHASE4_TIME, spawnInterval = BiologyGameConfig.PHASE4_SPAWN_INTERVAL, maxAliveEnemies = BiologyGameConfig.PHASE4_MAX_ALIVE, hpMultiplier = BiologyGameConfig.PHASE4_HP_MULT, allowShooter = true,  allowCharger = true,  eliteChance = BiologyGameConfig.PHASE4_ELITE_CHANCE },

            new DifficultyPhase { label = "P5 · 4-5' FINAL WAVE",      startTime = BiologyGameConfig.PHASE5_TIME, spawnInterval = BiologyGameConfig.PHASE5_SPAWN_INTERVAL, maxAliveEnemies = BiologyGameConfig.PHASE5_MAX_ALIVE, hpMultiplier = BiologyGameConfig.PHASE5_HP_MULT, allowShooter = true,  allowCharger = true,  eliteChance = BiologyGameConfig.PHASE5_ELITE_CHANCE },

        };



        // ----------------- VOID THREAT SYSTEM (Inspector) -----------------

        [Header("=== VOID THREAT SYSTEM ===")]

        [Tooltip("Threat cộng mỗi giây sống")]

        public float threatPerSecond = BiologyGameConfig.THREAT_PER_SECOND;

        [Tooltip("Threat mỗi lần giết enemy thường")]

        public float threatPerKill = BiologyGameConfig.THREAT_PER_KILL;

        [Tooltip("Threat mỗi lần giết ELITE")]

        public float threatPerEliteKill = BiologyGameConfig.THREAT_PER_ELITE_KILL;

        [Tooltip("Threat mỗi lần lên cấp")]

        public float threatPerLevelUp = BiologyGameConfig.THREAT_PER_LEVELUP;

        [Tooltip("Threat mỗi câu trả lời ĐÚNG")]

        public float threatPerCorrectAnswer = BiologyGameConfig.THREAT_PER_CORRECT_ANSWER;

        [Tooltip("Threat mỗi câu trả lời SAI")]

        public float threatPerWrongAnswer = BiologyGameConfig.THREAT_PER_WRONG_ANSWER;

        [Tooltip("Số giây Boss Warning trước khi boss xuất hiện")]

        public float bossWarningDuration = BiologyGameConfig.BOSS_WARNING_DURATION;

        [Tooltip("Threat reset về mức này sau khi boss chết")]

        public float threatAfterBossReset = BiologyGameConfig.THREAT_AFTER_BOSS_RESET;



        // ----------------- QUESTION DIFFICULTY (Inspector) -----------------

        [Header("=== ĐỘ KHÓ CÂU HỎI THEO THỜI GIAN ===")]

        [Tooltip("Từ giây này trở đi → câu TRUNG BÌNH")]

        public float midGameQuestionTime = 120f;

        [Tooltip("Từ giây này trở đi → câu KHÓ")]

        public float lateGameQuestionTime = 240f;



        // ---------- Trạng thái ----------

        public int Score { get; private set; }

        public int Level { get; private set; } = 1;

        public float Xp { get; private set; }

        public float XpToNext { get; private set; }

        public float Elapsed { get; private set; }

        public int Kills { get; private set; }

        public bool IsPaused { get; private set; }

        public bool IsGameOver { get; private set; }

        public bool IsVictory { get; private set; }



        // ---------- VOID THREAT ----------

        [Header("Trạng thái Threat (đọc để debug)")]

        public float Threat { get; private set; }

        public float ThreatRatio => Mathf.Clamp01(Threat / BiologyGameConfig.THREAT_MAX);

        public bool BossIncoming => _bossIncoming;   // true khi warning + đang đánh boss



        // ---------- Knowledge Streak ----------

        public int Streak { get; private set; }

        public int BestStreak { get; private set; }

        public bool IsAwakened { get; private set; }

        public float AwakeningTimer { get; private set; }

        public float StreakXpBonus => Streak >= BiologyGameConfig.STREAK_XP_BONUS_AT

            ? BiologyGameConfig.STREAK_XP_BONUS : 0f;



        // ---------- Nội bộ ----------

        private GameObject _playerGo;

        private Transform _fxRoot;

        private float _spawnTimer;

        private int _lastPhaseIndex = -1;

        private bool _bossIncoming;          // threat 100% → chặn spawn thường

        private bool _warningRunning;

        private int _bossCount;              // số boss đã xuất hiện (HP tăng dần)

        private BiologySurvBoss _boss;

        private bool _clashRunning;



        private static readonly string[] WeaponNames =

            { "Pencil Blaster", "Ruler Blade", "Book Orbit", "Calculator Lightning", "Chemical Flask" };



        private void Awake()

        {

            if (Instance != null && Instance != this) { Destroy(gameObject); return; }

            Instance = this;



            // Gán Index cho phase + đảm bảo sắp xếp theo startTime

            Array.Sort(Phases, (a, b) => a.startTime.CompareTo(b.startTime));

            for (int i = 0; i < Phases.Length; i++) Phases[i].Index = i;

        }



        private void Start()

        {

            if (quizUI == null) quizUI = FindAnyObjectByType<BiologyQuizUI>();

            if (essayUI == null) essayUI = FindAnyObjectByType<EssayQuestionUI>();

            if (hud == null) hud = FindAnyObjectByType<BiologyHUD>();



            BuildArena();

            XpToNext = BiologyGameConfig.XP_TO_LEVEL_2;

            hud?.SetHp(BiologyWhiteCell.Instance.CurrentHp, BiologyWhiteCell.Instance.MaxHp);

            hud?.SetXp(0f, 1f, Level);

            hud?.SetTimer(0f, BiologyGameConfig.WIN_TIME_SECONDS);

            hud?.SetScore(0, 0);

            hud?.SetStreak(0);

            hud?.SetThreat(0f);

            hud?.ShowMessage("🎒 Học sinh ơi! Bắn bằng CHUỘT TRÁI - Dash bằng SPACE! Sống sót 5 phút!", 4f);



            // Vũ khí khởi đầu: Pencil Blaster

            ApplyWeaponSprite(0);

            _spawnTimer = Phases[0].spawnInterval;

        }



        private void OnDestroy()

        {

            if (Instance == this) Instance = null;

        }



        // ================================================================

        // DỰNG SÂN - THEME: CORRUPTED CLASSROOM

        // ================================================================

        private void BuildArena()

        {

            // Sàn lớp học bị "nhiễm": bảng đen xám-xanh tối

            GameObject bg = new GameObject("ArenaBg");

            SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();

            bgSr.sprite = BiologySpriteFactory.GetWhiteSprite();

            bgSr.sortingOrder = -100;

            bgSr.color = new Color(0.30f, 0.38f, 0.36f); // bảng 클래스 tối

            bg.transform.localScale = new Vector3(60f, 40f, 1f);



            // Mảnh giấy/bụi phấn nhấp nhô cho có chiều sâu

            _fxRoot = new GameObject("FxRoot").transform;

            for (int i = 0; i < 40; i++)

            {

                GameObject dot = new GameObject($"Paper_{i}");

                dot.transform.SetParent(_fxRoot);

                dot.transform.position = new Vector3(Random.Range(-16f, 16f), Random.Range(-9f, 9f), 5f);

                dot.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 90f));

                SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();

                sr.sprite = BiologySpriteFactory.GetWhiteSprite();

                sr.sortingOrder = -90;

                sr.color = new Color(1f, 1f, 1f, 0.10f);

                float s = Random.Range(0.2f, 0.8f);

                dot.transform.localScale = new Vector3(s, s * 0.7f, 1f);

            }



            // Người chơi: HỌC SINH (sprite học sinh áo trắng quần xanh đã có sẵn)

            _playerGo = new GameObject("Student");

            _playerGo.transform.position = Vector3.zero;

            SpriteRenderer psr = _playerGo.AddComponent<SpriteRenderer>();

            psr.sprite = BiologySpriteFactory.GetPlayerSprite();

            psr.sortingOrder = 5;

            CircleCollider2D col = _playerGo.AddComponent<CircleCollider2D>();

            col.radius = 0.55f;

            _playerGo.AddComponent<BiologyWhiteCell>();



            // Camera cần Physics2DRaycaster (an toàn cho tương lai)

            Camera cam = Camera.main;

            if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)

                cam.gameObject.AddComponent<Physics2DRaycaster>();



            // Nối sự kiện của người chơi vào manager + HUD

            BiologyWhiteCell.Instance.OnPlayerDied += HandlePlayerDied;

            BiologyWhiteCell.Instance.OnHpChanged += (hp, max) => hud?.SetHp(hp, max);

        }



        // ================================================================

        // VÒNG LẶP GAME

        // ================================================================

        private void Update()

        {

            if (IsPaused || IsGameOver || IsVictory) return;



            Elapsed += Time.deltaTime;

            hud?.SetTimer(Elapsed, BiologyGameConfig.WIN_TIME_SECONDS);



            DifficultyPhase phase = CurrentPhase();



            // ----- Đổi vũ khí khi vào phase mới -----

            if (phase.Index != _lastPhaseIndex)

            {

                bool first = _lastPhaseIndex < 0;

                _lastPhaseIndex = phase.Index;

                ApplyWeaponSprite(phase.Index);

                if (!first)

                    hud?.ShowMessage($"✏️ Vũ khí mới: {WeaponNames[phase.Index]}!", 2.2f);

            }



            // ----- VOID THREAT: tích điểm theo thời gian sống -----

            if (!_bossIncoming && _boss == null)

            {

                Threat = Mathf.Min(BiologyGameConfig.THREAT_MAX, Threat + threatPerSecond * Time.deltaTime);

                if (Threat >= BiologyGameConfig.THREAT_MAX && !_warningRunning)

                {

                    _warningRunning = true;

                    StartCoroutine(BossWarningFlow());

                }

            }

            hud?.SetThreat(ThreatRatio);



            // ----- SPAWN: theo phase + trần maxAliveEnemies -----

            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer <= 0f)

            {

                TrySpawnWave(phase);

                _spawnTimer = phase.spawnInterval;

            }



            // ----- Awakening đếm ngược -----

            if (IsAwakened)

            {

                AwakeningTimer -= Time.deltaTime;

                if (AwakeningTimer <= 0f)

                {

                    IsAwakened = false;

                    hud?.ShowMessage("Hết THỨC TỈNH!", 1.5f);

                }

            }



            // ----- Sống sót 5 phút → thắng -----

            if (Elapsed >= BiologyGameConfig.WIN_TIME_SECONDS) Victory();

        }



        /// <summary>Phase hiện tại theo thời gian đã sống.</summary>

        private DifficultyPhase CurrentPhase()

        {

            DifficultyPhase cur = Phases[0];

            for (int i = 0; i < Phases.Length; i++)

                if (Elapsed >= Phases[i].startTime) cur = Phases[i];

            return cur;

        }



        private void ApplyWeaponSprite(int index)

        {

            BiologyWhiteCell pc = BiologyWhiteCell.Instance;

            if (pc != null)

                pc.CurrentWeaponSprite = BiologySpriteFactory.GetWeaponSprite(

                    Mathf.Clamp(index, 0, WeaponNames.Length - 1));

        }



        // ================================================================

        // SPAWN QUÁI - theo phase, có trần maxAliveEnemies

        // ================================================================

        private void TrySpawnWave(DifficultyPhase phase)

        {

            // VOID THREAT 100% hoặc đang có boss → dừng spawn enemy thường

            if (_bossIncoming || _boss != null) return;



            // TRẦN SỐ LƯỢNG: không spawn nếu đã đủ tối đa

            if (BiologySurvEnemy.All.Count >= phase.maxAliveEnemies) return;



            // Chọn loại enemy theo phase

            BiologyEnemyKind kind;

            float roll = Random.value;

            if (phase.allowCharger && roll >= 0.8f) kind = BiologyEnemyKind.Charger;

            else if (phase.allowShooter && roll >= 0.45f) kind = BiologyEnemyKind.Shooter;

            else kind = BiologyEnemyKind.Chaser;



            // ELITE chỉ xuất hiện từ phase có eliteChance > 0

            BiologyEliteKind elite = Random.value < phase.eliteChance

                ? (BiologyEliteKind)Random.Range(1, 5) // 1..4

                : BiologyEliteKind.None;



            SpawnEnemy(kind, elite, phase.hpMultiplier);

        }



        private void SpawnEnemy(BiologyEnemyKind kind, BiologyEliteKind elite, float hpMult)

        {

            Vector2 spawnPos = RandomOnScreenEdge(1.2f);

            GameObject go = new GameObject($"Enemy_{kind}");

            go.transform.position = spawnPos;



            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

            switch (kind)

            {

                case BiologyEnemyKind.Shooter: sr.sprite = BiologySpriteFactory.GetShooterSprite(); break;

                case BiologyEnemyKind.Charger: sr.sprite = BiologySpriteFactory.GetChargerSprite(); break;

                default: sr.sprite = BiologySpriteFactory.GetChaserSprite(); break;

            }

            sr.sortingOrder = 4;



            CircleCollider2D col = go.AddComponent<CircleCollider2D>();

            col.radius = 0.5f;



            // Rigidbody2D (kinematic) bắt buộc: đạn player là trigger, mà trigger

            // giữa 2 collider trigger cần ít nhất 1 Rigidbody2D để fire callback.

            BiologyPhysics2DUtil.AddKinematicBody(go);



            int hp; float speed; int dmg; float xp; int points;

            switch (kind)

            {

                case BiologyEnemyKind.Shooter:

                    hp = BiologyGameConfig.SHOOTER_HP; speed = BiologyGameConfig.SHOOTER_SPEED;

                    dmg = BiologyGameConfig.SHOOTER_DMG; xp = BiologyGameConfig.XP_PER_KILL_BASE + 1f; break;

                case BiologyEnemyKind.Charger:

                    hp = BiologyGameConfig.CHARGER_HP; speed = BiologyGameConfig.CHARGER_WALK_SPEED;

                    dmg = BiologyGameConfig.CHARGER_DMG; xp = BiologyGameConfig.XP_PER_KILL_BASE + 1f; break;

                default:

                    hp = BiologyGameConfig.CHASER_HP;

                    speed = Random.Range(BiologyGameConfig.CHASER_SPEED_MIN, BiologyGameConfig.CHASER_SPEED_MAX);

                    dmg = BiologyGameConfig.CHASER_DMG; xp = BiologyGameConfig.XP_PER_KILL_BASE; break;

            }

            points = BiologyGameConfig.POINTS_PER_KILL;



            // HP nhân theo PHASE hiện tại (thay cho cơ chế tăng vô hạn trước đây)

            hp = Mathf.Max(1, Mathf.RoundToInt(hp * hpMult));



            if (elite != BiologyEliteKind.None)

            {

                hp *= BiologyGameConfig.ELITE_HP_MULT;

                dmg += BiologyGameConfig.ELITE_DMG_BONUS;

                xp *= BiologyGameConfig.ELITE_XP_MULT;

                points += BiologyGameConfig.ELITE_POINTS;

            }



            BiologySurvEnemy e;

            switch (kind)

            {

                case BiologyEnemyKind.Shooter: e = go.AddComponent<BiologyShooter>(); break;

                case BiologyEnemyKind.Charger: e = go.AddComponent<BiologyCharger>(); break;

                default: e = go.AddComponent<BiologyChaser>(); break;

            }

            e.Setup(kind, hp, speed, dmg, xp, points, elite);

        }



        // ================================================================

        // BOSS FIGHT

        // ================================================================

        private IEnumerator BossWarningFlow()

        {

            // 1) Dừng spawn enemy thường + dừng tích threat

            _bossIncoming = true;



            // 2+3) Boss Warning: đếm ngược cho player chuẩn bị

            hud?.ShowBossWarning(bossWarningDuration);

            hud?.ShowMessage("☠ VOID THREAT 100%! Boss đang tiến tới...", 2.5f);

            yield return new WaitForSecondsRealtime(bossWarningDuration);

            _warningRunning = false;



            if (IsGameOver || IsVictory) { _bossIncoming = false; yield break; }



            // 4+5) Spawn Boss → Boss Fight

            SpawnBoss();

        }



        private void SpawnBoss()

        {

            _bossCount++;



            GameObject go = new GameObject("VoidBoss");

            go.transform.position = RandomOnScreenEdge(1.5f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();

            sr.sprite = BiologySpriteFactory.GetExamBossSprite(); // Bài Kiểm Tra bị nguyền

            sr.sortingOrder = 4;

            go.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();

            col.radius = 0.9f;

            BiologyPhysics2DUtil.AddKinematicBody(go); // cần cho trigger với đạn player



            // HP boss tăng theo số lần xuất hiện (endless threat → boss mạnh dần)

            int hp = Mathf.RoundToInt(BiologyGameConfig.BOSS_HP * (1f + 0.5f * (_bossCount - 1)));

            _boss = go.AddComponent<BiologySurvBoss>();

            _boss.Setup(hp, BiologyGameConfig.BOSS_SPEED);



            Threat = 0f; // threat đã "tiêu hao" vào boss

            hud?.ShowMessage("⚠ BÀI KIỂM TRA MA XUẤT HIỆN! Né đạn và hạ nó!", 2.5f);

        }



        private static Vector2 RandomOnScreenEdge(float margin)

        {

            float halfH = Camera.main.orthographicSize + margin;

            float halfW = halfH * Camera.main.aspect + margin;

            int side = Random.Range(0, 4);

            switch (side)

            {

                case 0: return new Vector2(Random.Range(-halfW, halfW), halfH);

                case 1: return new Vector2(Random.Range(-halfW, halfW), -halfH);

                case 2: return new Vector2(halfW, Random.Range(-halfH, halfH));

                default: return new Vector2(-halfW, Random.Range(-halfH, halfH));

            }

        }



        // ================================================================

        // XP & LÊN CẤP → CÂU HỎI

        // ================================================================

        public void GainXp(float amount)

        {

            if (IsGameOver || IsVictory) return;

            Xp += amount;

            if (Xp >= XpToNext && !_levelUpRunning)

            {

                _levelUpRunning = true;

                StartCoroutine(LevelUpFlow());

            }

            else hud?.SetXp(Xp, XpToNext, Level);

        }



        private bool _levelUpRunning;



        private IEnumerator LevelUpFlow()

        {

            Xp -= XpToNext;

            Level++;

            Score += BiologyGameConfig.POINTS_PER_LEVEL;

            XpToNext = BiologyGameConfig.XP_TO_LEVEL_2 * Mathf.Pow(Level - 1, BiologyGameConfig.XP_GROWTH);

            hud?.SetXp(Xp, XpToNext, Level);

            hud?.SetScore(Score, Kills);



            // VOID THREAT: level up làm threat tăng (kết hợp survival + knowledge progress)

            Threat = Mathf.Min(BiologyGameConfig.THREAT_MAX, Threat + threatPerLevelUp);



            IsPaused = true;

            Time.timeScale = 0f;



            BiologyQuestionBank bank = BiologyQuestionBank.Instance;

            // ======================================================
            // CHỜ GOOGLE SHEET / QUESTION BANK SẴN SÀNG
            // ======================================================
            if (bank == null)
            {
                Debug.LogError("[SinhHoc][Survival] Không tìm thấy BiologyQuestionBank!");
            }
            else
            {
                float waitTime = 0f;
                const float maxWaitTime = 10f;

                Debug.Log(
                    $"[SinhHoc][Survival] QuestionBank hiện có {bank.totalQuestions} câu. " +
                    "Đang kiểm tra dữ liệu..."
                );

                // Time.timeScale đang = 0, nên phải dùng unscaledDeltaTime.
                while (bank.totalQuestions <= 0 && waitTime < maxWaitTime)
                {
                    waitTime += Time.unscaledDeltaTime;
                    yield return null;
                }

                // Build lại pool theo môn/chương/bài/loại câu hỏi hiện tại.
                bank.BuildPoolForCurrentSession();

                Debug.Log(
                    $"[SinhHoc][Survival] QuestionBank sẵn sàng. " +
                    $"Tổng câu đã tải = {bank.totalQuestions}"
                );
            }

            // Độ khó câu hỏi theo THỜI GIAN game (không theo level):
            // sớm → Dễ, giữa → Trung bình, muộn → Khó
            BiologyDifficulty diff = Elapsed < midGameQuestionTime ? BiologyDifficulty.De
                                     : Elapsed < lateGameQuestionTime ? BiologyDifficulty.TrungBinh
                                     : BiologyDifficulty.Kho;

            BiologyQuestion q = bank != null ? bank.GetRandomQuestionWithFallback(diff) : null;



            if (q == null)

            {

                Debug.LogError("[SinhHoc][Survival] Không có câu hỏi nào trong ngân hàng!");

                List<BioUpgrade> free = RollUpgradePool(BiologyGameConfig.UPGRADES_ON_CORRECT, false);

                quizUI.ShowUpgradeChoice(free, choice =>

                {

                    ApplyUpgrade(free[choice]);

                    ClosePause();

                    _levelUpRunning = false;

                });

                yield break;

            }



            bool answered = false;

            bool correct = false;

            bool skipped = false;

            float essayPercentage = 0f;



            if (q.questionType == QuestionType.EssayImage)

            {

                if (essayUI != null)

                {

                    essayUI.ShowQuestion(q, result => {

                        skipped = result.skipped;

                        if (!skipped)

                        {

                            essayPercentage = result.percentage;

                            correct = (result.percentage >= 50f);

                        }

                        answered = true;

                    });

                }

                else

                {

                    // Thiếu EssayQuestionUI trong scene → bỏ qua an toàn (không crash, không tính sai)

                    Debug.LogWarning("[SinhHoc][Survival] Thiếu EssayQuestionUI - bỏ qua câu tự luận.");

                    skipped = true;

                    answered = true;

                }

            }

            else

            {

                quizUI.ShowQuestion(q, isCorrect => { correct = isCorrect; answered = true; });

            }



            yield return new WaitUntil(() => answered);



            // Progress kiến thức (#26): MC ghi tại đây, tự luận đã tự ghi trong EssayQuestionUI

            if (!skipped && q.questionType == QuestionType.MultipleChoice)

            {

                KnowledgeProgressManager.RecordMultipleChoice(

                    GameSessionData.SelectedSubject, GameSessionData.SelectedChapterID,

                    GameSessionData.SelectedLessonID, correct);

            }

            if (!skipped) bank.RecordAnswer(q.difficulty, correct);



            if (skipped)

            {

                // Bỏ qua do lỗi kỹ thuật / ảnh không đọc được (#17, #21):

                // KHÔNG tính sai, KHÔNG mất streak, KHÔNG phạt - vẫn cho nâng cấp thường.

                List<BioUpgrade> skipPool = RollUpgradePool(BiologyGameConfig.UPGRADES_ON_WRONG, false);

                quizUI.ShowUpgradeChoice(skipPool, choice =>

                {

                    ApplyUpgrade(skipPool[choice]);

                    ClosePause();

                    _levelUpRunning = false;

                });

                yield break;

            }



            Threat = Mathf.Min(BiologyGameConfig.THREAT_MAX,

                Threat + (correct ? threatPerCorrectAnswer : threatPerWrongAnswer));



            if (correct)

            {

                Streak++;

                if (Streak > BestStreak) BestStreak = Streak;

                hud?.SetStreak(Streak);

                KnowledgeProgressManager.RecordStreak(GameSessionData.SelectedSubject,

                    GameSessionData.SelectedChapterID, GameSessionData.SelectedLessonID, Streak);



                if (Streak == BiologyGameConfig.STREAK_XP_BONUS_AT) hud?.ShowMessage("STREAK x2 - EXP +10%!", 1.6f);

                else if (Streak == BiologyGameConfig.STREAK_DMG_AT) { BiologyWhiteCell.Instance.ProjectileDmg += 1; hud?.ShowMessage("STREAK x3 - DAMAGE +1!", 1.6f); }

                else if (Streak == BiologyGameConfig.STREAK_CRIT_AT) { BiologyWhiteCell.Instance.CritChance += BiologyGameConfig.STREAK_CRIT_CHANCE; hud?.ShowMessage("STREAK x5 - CRIT +15%!", 1.6f); }

                else if (Streak >= BiologyGameConfig.STREAK_AWAKENING_AT && !IsAwakened) StartAwakening();



                // Phần thưởng theo băng điểm (#11 trắc nghiệm, #18 tự luận)

                List<BioUpgrade> pool = q.questionType == QuestionType.EssayImage

                    ? RollUpgradePoolForEssay(essayPercentage)

                    : RollUpgradePool(BiologyGameConfig.UPGRADES_ON_CORRECT, true);

                quizUI.ShowUpgradeChoice(pool, choice =>

                {

                    ApplyUpgrade(pool[choice]);

                    ClosePause();

                    _levelUpRunning = false;

                });

            }

            else

            {

                Streak = 0;

                hud?.SetStreak(0);



                List<BioUpgrade> pool = RollUpgradePool(BiologyGameConfig.UPGRADES_ON_WRONG, false);

                quizUI.ShowUpgradeChoice(pool, choice =>

                {

                    ApplyUpgrade(pool[choice]);

                    ClosePause();

                    _levelUpRunning = false;

                });

            }

        }



        private void ClosePause()

        {

            IsPaused = false;

            Time.timeScale = 1f;

        }



        private void StartAwakening()

        {

            IsAwakened = true;

            AwakeningTimer = BiologyGameConfig.AWAKENING_DURATION;

            quizUI.ShowAwakening(BiologyGameConfig.AWAKENING_DURATION);

            hud?.ShowMessage("⚡ AWAKENING! DAMAGE x2 - XP x2!", 2.5f);

        }



        // ================================================================

        // NÂNG CẤP (có rarity) - giữ nguyên từ phiên bản cũ

        // ================================================================

        public enum BioRarity { Common = 0, Rare = 1, Epic = 2, Legendary = 3 }



        public struct BioUpgrade

        {

            public int Id;

            public BioRarity Rarity;

        }



        /// <summary>

        /// Xáo pool nâng cấp. Đúng câu → có cơ hội Rare/Epic/Legendary.

        /// Sai câu → chỉ Common (nhưng vẫn được nâng cấp để không bị kẹt).

        /// </summary>

        private List<BioUpgrade> RollUpgradePool(int count, bool allowLegendary)

        {

            List<BioUpgrade> pool = new List<BioUpgrade>();

            while (pool.Count < count)

            {

                int id = Random.Range(0, 10); // 0..9

                BioRarity rar = BioRarity.Common;



                if (allowLegendary) // chỉ trả lời ĐÚNG mới có cơ hội rarity cao

                {

                    float roll = Random.value;

                    if (roll < BiologyGameConfig.LEGENDARY_CHANCE_ON_CORRECT)

                    {

                        rar = BioRarity.Legendary;

                        id = Random.Range(6, 10); // Legendary là các nâng cấp đổi gameplay

                    }

                    else if (roll < BiologyGameConfig.RARE_CHANCE * 0.5f)

                    {

                        rar = BioRarity.Epic;

                        id = Random.Range(3, 10);

                    }

                    else if (roll < BiologyGameConfig.RARE_CHANCE)

                    {

                        rar = BioRarity.Rare;

                        id = Random.Range(1, 10);

                    }

                }



                BioUpgrade up = new BioUpgrade { Id = id, Rarity = rar };

                if (!pool.Contains(up)) pool.Add(up);

            }

            return pool;

        }



        /// <summary>

        /// Pool nâng cấp cho TỰ LUẬN theo băng điểm (#18):

        ///   90-100%  PERFECT → Epic (kèm cơ hội Legendary)

        ///   70-89%   GOOD    → Rare

        ///   50-69%   PASS    → Common

        ///   <50%     NEEDS IMPROVEMENT → pool nhỏ như trả lời sai (không kẹt game)

        /// </summary>

        private List<BioUpgrade> RollUpgradePoolForEssay(float percentage)

        {

            if (percentage < 50f)

                return RollUpgradePool(BiologyGameConfig.UPGRADES_ON_WRONG, false);



            List<BioUpgrade> pool = RollUpgradePool(BiologyGameConfig.UPGRADES_ON_CORRECT, true);

            BioRarity minRarity = percentage >= 90f ? BioRarity.Epic

                                : percentage >= 70f ? BioRarity.Rare

                                : BioRarity.Common;

            if (pool.Count > 0 && (int)pool[0].Rarity < (int)minRarity)

            {

                BioUpgrade up = pool[0];

                up.Rarity = minRarity;

                pool[0] = up;

            }

            return pool;

        }



        public static string UpgradeName(BioUpgrade up)

        {

            bool boost = up.Rarity != BioRarity.Common; // rarity cao → số liệu mạnh hơn

            switch (up.Id)

            {

                case 0: return boost ? "BẮN SIÊU NHANH\nBắn nhanh hơn 35%" : "BẮN NHANH\nBắn nhanh hơn 20%";

                case 1: return boost ? "ĐẠN XUYÊN PHÁ x2\nXuyên qua thêm 2 mầm bệnh" : "ĐẠN XUYÊN PHÁ\nXuyên qua thêm 1 mầm bệnh";

                case 2: return boost ? "PHÁT 3 VIÊN\n+1 viên mỗi lượt bắn" : "THÊM ĐẠN\n+1 viên mỗi lượt bắn";

                case 3: return boost ? "HỒI PHỤC ĐẦY ĐỦ\nHồi toàn bộ máu" : "HỒI MÁU\nPhục hồi 3 HP";

                case 4: return boost ? "NAM CHÂM XỈU\nHút XP xa hơn + XP x2" : "NAM CHÂM XP\nHút XP xa hơn";

                case 5: return boost ? "KHÁNG THỂ CỰC MẠNH\n+2 sát thương mỗi viên" : "KHÁNG THỂ MẠNH\n+1 sát thương mỗi viên";

                case 6: return "ĐẠN THIÊN THẦN\n+1 pierce và +1 đạn";

                case 7: return "MÁU VAMPIRE\nGiết quái hồi 1 HP";

                case 8: return "TỐC ĐỘ BÓNG ĐÊM\nDi chuyển +30%, hồi chiêu dash nhanh hơn";

                case 9: return "CHÍ MẠNG THIÊN THẦN\nCrit +20% (đạn crit x2 sát thương)";

                default: return "Nâng cấp";

            }

        }



        private void ApplyUpgrade(BioUpgrade up)

        {

            BiologyWhiteCell pc = BiologyWhiteCell.Instance;

            if (pc == null) return;

            bool boost = up.Rarity != BioRarity.Common;



            switch (up.Id)

            {

                case 0: pc.ShootInterval = Mathf.Max(0.12f, pc.ShootInterval * (boost ? 0.65f : 0.8f)); break;

                case 1: pc.Pierce += boost ? 2f : 1f; break;

                case 2: pc.ProjectileCount = Mathf.Min(6, pc.ProjectileCount + 1); break;

                case 3:

                    if (boost) pc.Heal(pc.MaxHp); else pc.Heal(3);

                    break;

                case 4:

                    pc.MagnetRadius += boost ? 3f : 1.5f;

                    if (boost) pc.OrbBonus += 1f; else pc.OrbBonus += 0.5f;

                    break;

                case 5: pc.ProjectileDmg += boost ? 2 : 1; break;

                case 6: pc.Pierce += 1f; pc.ProjectileCount = Mathf.Min(6, pc.ProjectileCount + 1); break;

                case 7: pc.LifestealPerKill += 1; break;

                case 8:

                    pc.MoveSpeed *= 1.3f;

                    pc.DashCooldown = Mathf.Max(0.8f, pc.DashCooldown * 0.7f);

                    break;

                case 9: pc.CritChance = Mathf.Min(0.8f, pc.CritChance + 0.2f); break;

            }

            hud?.ShowMessage($"Đã nâng cấp: {UpgradeName(up).Split('\n')[0]}!", 1.8f);

        }



        // ================================================================

        // KILL / THẮNG / THUA

        // ================================================================

        public void OnEnemyKilled(BiologySurvEnemy enemy)

        {

            if (enemy == null) return;



            Kills++;

            Score += enemy.IsElite ? BiologyGameConfig.ELITE_POINTS : BiologyGameConfig.POINTS_PER_KILL;



            // VOID THREAT: giết quái (nhất là ELITE) làm threat tăng

            Threat = Mathf.Min(BiologyGameConfig.THREAT_MAX,

                Threat + (enemy.IsElite ? threatPerEliteKill : threatPerKill));



            BiologyWhiteCell pc = BiologyWhiteCell.Instance;



            // Lifesteal: elite hồi gấp đôi, quái thường hồi 35% cơ hội

            if (pc != null && pc.LifestealPerKill > 0)

            {

                if (enemy.IsElite) pc.Heal(pc.LifestealPerKill * 2);

                else if (Random.value < 0.35f) pc.Heal(pc.LifestealPerKill);

            }



            hud?.SetScore(Score, Kills);



            // Rơi bong bóng XP (bong bóng kiến thức)

            GameObject orb = new GameObject("XpOrb");

            orb.transform.position = enemy.transform.position;

            SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();

            sr.sprite = BiologySpriteFactory.GetXpBubbleSprite();

            sr.sortingOrder = 3;

            float s = enemy.IsElite ? 0.9f : Random.Range(0.5f, 0.7f);

            orb.transform.localScale = new Vector3(s, s, 1f);

            orb.AddComponent<BiologyXpOrb>().Setup(enemy.XpValue);

        }



        public void OnBossKilled(float xp)

        {

            Score += 500;

            BiologyWhiteCell pc = BiologyWhiteCell.Instance;

            Vector2 center = pc != null ? (Vector2)pc.transform.position : Vector2.zero;

            for (int i = 0; i < 6; i++)

            {

                GameObject orb = new GameObject("BossXpOrb");

                orb.transform.position = center + (Vector2)(Random.insideUnitCircle * 1.5f);

                SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();

                sr.sprite = BiologySpriteFactory.GetXpBubbleSprite();

                sr.sortingOrder = 3;

                orb.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                orb.AddComponent<BiologyXpOrb>().Setup(xp / 2f);

            }



            // VOID THREAT: boss chết → reset một phần, game tiếp tục bình thường

            _boss = null;

            _bossIncoming = false;

            Threat = threatAfterBossReset;

            hud?.SetThreat(ThreatRatio);

            hud?.ShowMessage("✅ Đã vượt qua Bài Kiểm Tra! Nhặt kiến thức!", 2.5f);

        }



        // ================================================================

        // KNOWLEDGE CLASH - Boss chuẩn bị Ultimate, trả lời đúng để phản đòn

        // (luôn dùng câu hỏi mức BOSS = KHÓ)

        // ================================================================

        public void StartKnowledgeClash(BiologySurvBoss boss)

        {

            if (_clashRunning || IsGameOver || IsVictory) return;

            StartCoroutine(KnowledgeClashFlow(boss));

        }



        private IEnumerator KnowledgeClashFlow(BiologySurvBoss boss)

        {

            _clashRunning = true;



            // Slow-motion tạo kịch tính: mọi thứ chạy chậm lại nhưng chưa dừng

            Time.timeScale = BiologyGameConfig.BOSS_CLASH_SLOWMO_SCALE;

            hud?.ShowMessage("⚡ KNOWLEDGE CLASH! Boss đang gom sức!", 1.5f);

            yield return new WaitForSecondsRealtime(BiologyGameConfig.BOSS_CLASH_SLOWMO_TIME);



            // Dừng hẳn để trả lời câu hỏi

            IsPaused = true;

            Time.timeScale = 0f;



            BiologyQuestionBank bank = BiologyQuestionBank.Instance;

            // Boss Knowledge Clash LUÔN ưu tiên câu KHÓ (mức Boss) - tự fallback khi hết (#9)

            BiologyQuestion q = bank != null ? bank.GetRandomQuestionWithFallback(BiologyDifficulty.Boss) : null;



            if (q == null)

            {

                // Không có câu hỏi boss → coi như player vượt qua

                Debug.LogWarning("[SinhHoc][Survival] Không có câu hỏi boss - bỏ qua Knowledge Clash.");

                boss.Stun(BiologyGameConfig.BOSS_STUN_TIME);

                IsPaused = false;

                Time.timeScale = 1f;

                _clashRunning = false;

                yield break;

            }



            bool answered = false;

            bool correct = false;

            bool skipped = false;

            float essayPercentage = 0f;



            if (q.questionType == QuestionType.EssayImage)

            {

                if (essayUI != null)

                {

                    essayUI.ShowQuestion(q, result => {

                        skipped = result.skipped;

                        if (!skipped)

                        {

                            essayPercentage = result.percentage;

                            correct = (result.percentage >= 50f);

                        }

                        answered = true;

                    });

                }

                else

                {

                    Debug.LogWarning("[SinhHoc][Survival] Thiếu EssayQuestionUI - bỏ qua câu tự luận.");

                    skipped = true;

                    answered = true;

                }

            }

            else

            {

                quizUI.ShowQuestion(q, isCorrect => { correct = isCorrect; answered = true; });

            }



            yield return new WaitUntil(() => answered);



            // Boss có thể đã chết trong lúc chậm/nhả đạn cuối → không đụng vào nó nữa

            if (boss == null)

            {

                if (!IsGameOver && !IsVictory)

                {

                    IsPaused = false;

                    Time.timeScale = 1f;

                }

                _clashRunning = false;

                yield break;

            }



            // Progress kiến thức (#26) + bỏ qua an toàn

            if (!skipped && q.questionType == QuestionType.MultipleChoice)

            {

                KnowledgeProgressManager.RecordMultipleChoice(

                    GameSessionData.SelectedSubject, GameSessionData.SelectedChapterID,

                    GameSessionData.SelectedLessonID, correct);

            }

            if (!skipped) bank.RecordAnswer(q.difficulty, correct);



            if (skipped)

            {

                // Bỏ qua do lỗi kỹ thuật → không stun, không ultimate, không phạt (#21)

                if (!IsGameOver && !IsVictory)

                {

                    IsPaused = false;

                    Time.timeScale = 1f;

                }

                _clashRunning = false;

                yield break;

            }



            // VOID THREAT: trả lời clash đúng/sai cũng ảnh hưởng threat

            Threat = Mathf.Min(BiologyGameConfig.THREAT_MAX,

                Threat + (correct ? threatPerCorrectAnswer : threatPerWrongAnswer));



            if (correct)

            {

                Streak++;

                if (Streak > BestStreak) BestStreak = Streak;

                hud?.SetStreak(Streak);

                KnowledgeProgressManager.RecordStreak(GameSessionData.SelectedSubject,

                    GameSessionData.SelectedChapterID, GameSessionData.SelectedLessonID, Streak);



                quizUI.ShowPerfectCounter(BiologyGameConfig.BOSS_STUN_TIME);

                boss.Stun(BiologyGameConfig.BOSS_STUN_TIME);

                Score += 300;

                hud?.SetScore(Score, Kills);

            }

            else

            {

                Streak = 0;

                hud?.SetStreak(0);

                quizUI.ShowUltimateIncoming();

                boss.CastUltimate(); // player phải né

            }



            yield return new WaitForSecondsRealtime(1.2f);



            // Nếu trong lúc đó game đã kết thúc thì không đụng vào timeScale nữa

            if (!IsGameOver && !IsVictory)

            {

                IsPaused = false;

                Time.timeScale = 1f;

            }

            _clashRunning = false;

        }



        public void NotifyBossEnraged()

        {

            hud?.ShowMessage("☠ BOSS NỔI ĐIÊN! Tránh xa!", 2f);

        }


// ================================================================
// KẾT THÚC
// ================================================================

private void Victory()
{
    if (IsGameOver || IsVictory)
        return;

    IsVictory = true;

    Score += BiologyGameConfig.POINTS_WIN_BONUS;

    // Lưu lịch sử trước khi dừng game
    SaveGameHistory();

    Time.timeScale = 0f;

    if (quizUI != null)
    {
        quizUI.ShowResults(victory: true);
    }
}


public void HandlePlayerDied()
{
    if (IsGameOver || IsVictory)
        return;

    IsGameOver = true;

    // Lưu lịch sử trước khi dừng game
    SaveGameHistory();

    Time.timeScale = 0f;

    if (quizUI != null)
    {
        quizUI.ShowResults(victory: false);
    }
}


// ================================================================
// LƯU LỊCH SỬ CHƠI
// ================================================================

private void SaveGameHistory()
{
    if (BiologyGameHistory.Instance == null)
    {
        Debug.LogError(
            "[SinhHoc][Survival] Không tìm thấy BiologyGameHistory!"
        );

        return;
    }

    Debug.Log(
        "[SinhHoc][Survival] Đang lưu lịch sử game..." +
        " | Level=" + Level +
        " | Time=" + Elapsed +
        " | Score=" + Score
    );

    BiologyGameHistory.Instance.FinishGame(
        Level,
        Elapsed,
        Score
    );
}


// ================================================================
// CHƠI LẠI
// ================================================================

public void RestartGame()
{
    Time.timeScale = 1f;

    UnityEngine.SceneManagement.SceneManager.LoadScene(
        BiologyGameConfig.SCENE_NAME
    );
}


// ================================================================
// VỀ MENU
// ================================================================

public void BackToMenu()
{
    Time.timeScale = 1f;

    UnityEngine.SceneManagement.SceneManager.LoadScene(
        "SampleScene"
    );
}

    }
}

