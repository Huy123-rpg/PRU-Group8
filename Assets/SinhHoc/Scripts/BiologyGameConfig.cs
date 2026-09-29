// ============================================================
// BiologyGameConfig.cs
// Cấu hình game môn SINH HỌC - thể loại SINH TỒN MIỄN DỊCH
// (kiểu Vampire Survivors):
//   • WASD di chuyển + chuột ngắm + chuột trái bắn kháng thể
//   • SPACE dash để né đòn (có cooldown)
//   • 3 loại mầm bệnh: Đuổi theo / Bắn xa / Lao vào
//   • Mầm bệnh có thể thành ELITE (modifier)
//   • Lên cấp → trả lời câu hỏi Sinh học → chọn nâng cấp (có rarity)
//   • Chuỗi đúng → thưởng + AWAKENING MODE
//   • Boss có 3 phase + KNOWLEDGE CLASH
// Mọi thông số cân bằng game nằm ở đây.
// ============================================================
namespace PRU.Biology
{
    public static class BiologyGameConfig
    {
        // ---------- Scene ----------
        public const string SCENE_NAME = "SinhHoc";

        // ---------- Người chơi (tế bào trắng) ----------
        public const float PLAYER_SPEED = 5.2f;
        public const int   PLAYER_MAX_HP = 8;             // 8 nửa-tim (4 tim)
        public const float IFRAME_TIME = 0.8f;            // bất tử sau khi trúng đạn

        // ---------- Dash ----------
        public const float DASH_SPEED = 14f;              // tốc độ khi dash
        public const float DASH_TIME = 0.18f;             // thời gian dash (giây)
        public const float DASH_COOLDOWN = 2.0f;          // hồi chiêu dash

        // ---------- Vũ khí (kháng thể) ----------
        public const float SHOOT_INTERVAL_BASE = 0.55f;   // giây giữa 2 lần bắn (bắn bằng chuột)
        public const int   PROJECTILE_DMG_BASE = 1;
        public const float PROJECTILE_SPEED_BASE = 10f;

        // ---------- Mầm bệnh - loại ĐUỶ THEO (Slime) ----------
        public const int   CHASER_HP = 2;
        public const float CHASER_SPEED_MIN = 1.6f;
        public const float CHASER_SPEED_MAX = 2.4f;
        public const int   CHASER_DMG = 1;

        // ---------- Mầm bệnh - loại BẮN XA (Archer) ----------
        public const int   SHOOTER_HP = 3;
        public const float SHOOTER_SPEED = 1.2f;
        public const int   SHOOTER_DMG = 1;
        public const float SHOOTER_KEEP_DIST = 5.0f;      // giữ cách người chơi
        public const float SHOOTER_FIRE_INTERVAL = 2.2f;  // giây giữa 2 phát
        public const float ENEMY_BULLET_SPEED = 4.5f;
        public const int   ENEMY_BULLET_DMG = 1;

        // ---------- Mầm bệnh - loại LAO VÀO (Charger) ----------
        public const int   CHARGER_HP = 4;
        public const float CHARGER_WALK_SPEED = 1.0f;
        public const float CHARGER_DASH_SPEED = 7.5f;
        public const int   CHARGER_DMG = 1;
        public const float CHARGER_WARN_TIME = 0.55f;     // nhấp nháy cảnh báo trước khi lao
        public const float CHARGER_CHARGE_TIME = 0.5f;    // thời gian lao
        public const float CHARGER_REST_TIME = 1.2f;      // nghỉ sau khi lao
        public const float CHARGER_TRIGGER_DIST = 5.5f;   // khoảng cách bắt đầu lao

        // ---------- DIFFICULTY PROGRESSION (5 giai đoạn, cấu hình trong Inspector) ----------
        // Mỗi phase khai báo: thời điểm bắt đầu (giây), spawn interval, số enemy tối đa sống,
        // hệ số nhân HP enemy và các loại enemy được mở khóa.
        public const int   PHASE_COUNT = 5;

        // Thời điểm chuyển phase (giây, mốc đầu mỗi phase)
        public const float PHASE1_TIME = 0f;              // 0–1 phút: nhẹ nhàng
        public const float PHASE2_TIME = 60f;             // 1–2 phút: thêm Shooter
        public const float PHASE3_TIME = 120f;            // 2–3 phút: thêm Charger
        public const float PHASE4_TIME = 180f;            // 3–4 phút: bắt đầu có ELITE
        public const float PHASE5_TIME = 240f;            // 4–5 phút: FINAL WAVE

        // Spawn interval (giây) từng phase - bạn yêu cầu: ~2–2.5s đầu, ~1–1.5s cuối
        public const float PHASE1_SPAWN_INTERVAL = 2.25f;
        public const float PHASE2_SPAWN_INTERVAL = 1.8f;
        public const float PHASE3_SPAWN_INTERVAL = 1.25f;
        public const float PHASE4_SPAWN_INTERVAL = 1.0f;
        public const float PHASE5_SPAWN_INTERVAL = 0.9f;

        // maxAliveEnemies - trần số enemy sống cùng lúc, không bao giờ spawn vượt quá
        public const int   PHASE1_MAX_ALIVE = 7;
        public const int   PHASE2_MAX_ALIVE = 12;
        public const int   PHASE3_MAX_ALIVE = 18;
        public const int   PHASE4_MAX_ALIVE = 25;
        public const int   PHASE5_MAX_ALIVE = 30;

        // Hệ số nhân HP enemy theo phase (giữ cảm giác "quái không tăng vô hạn")
        public const float PHASE1_HP_MULT = 1f;
        public const float PHASE2_HP_MULT = 1.15f;
        public const float PHASE3_HP_MULT = 1.3f;
        public const float PHASE4_HP_MULT = 1.5f;
        public const float PHASE5_HP_MULT = 1.7f;

        // Tỷ lệ ELITE (0..1) bắt đầu từ phase 4
        public const float PHASE4_ELITE_CHANCE = 0.08f;
        public const float PHASE5_ELITE_CHANCE = 0.15f;

        // ---------- ELITE modifier ----------
        public const float ELITE_CHANCE_START = 0.03f;    // 3% lúc đầu
        public const float ELITE_CHANCE_END = 0.12f;      // 12% ở phút 3
        public const int   ELITE_HP_MULT = 2;             // x2 máu
        public const int   ELITE_DMG_BONUS = 1;           // +1 sát thương
        public const float ELITE_SCALE = 1.4f;            // to hơn
        public const float ELITE_XP_MULT = 3f;            // x3 XP
        public const int   ELITE_POINTS = 50;

        // ---------- Máu quái cơ bản (trước hệ số phase) ----------
        public const int   PATHOGEN_HP_MAX = 8;
        public const float PATHOGEN_TOUCH_COOLDOWN = 0.8f;
        public const float XP_PER_KILL_BASE = 1;

        // ---------- Kinh nghiệm & lên cấp ----------
        public const float XP_TO_LEVEL_2 = 5f;
        public const float XP_GROWTH = 1.35f;

        // ---------- THỜI GIAN VÒNG ĐỜI (5 phút) ----------
        public const float WIN_TIME_SECONDS = 300f;       // sống sót 5 phút = thắng

        // ---------- Câu hỏi ----------
        public const float QUESTION_TIMER_SECONDS = 20f;  // thời gian trả lời (0 = không giới hạn)
        public const float ANSWER_FEEDBACK_CORRECT_TIME = 0.9f;
        public const float ANSWER_FEEDBACK_WRONG_TIME = 2.6f; // sai → xem giải thích lâu hơn

        // ---------- KNOWLEDGE STREAK (đúng liên tiếp) ----------
        public const int STREAK_XP_BONUS_AT = 2;          // 2 câu đúng → XP +10%
        public const float STREAK_XP_BONUS = 0.10f;
        public const int STREAK_DMG_AT = 3;               // 3 câu đúng → Damage +1
        public const int STREAK_CRIT_AT = 5;              // 5 câu đúng → Crit 15%
        public const float STREAK_CRIT_CHANCE = 0.15f;
        public const int STREAK_AWAKENING_AT = 10;        // 10 câu đúng → AWAKENING MODE

        // ---------- AWAKENING MODE ----------
        public const float AWAKENING_DURATION = 15f;
        public const float AWAKENING_DMG_MULT = 2f;       // Damage x2
        public const float AWAKENING_ATKSPD_MULT = 1.5f;  // Bắn nhanh hơn 50%
        public const float AWAKENING_SPEED_MULT = 1.2f;   // Di chuyển nhanh hơn 20%
        public const float AWAKENING_XP_MULT = 2f;        // XP x2

        // ---------- CRITICAL ----------
        public const float CRIT_CHANCE_BASE = 0.05f;      // 5% chí mạng cơ bản
        public const float CRIT_DMG_MULT = 2f;            // chí mạng x2 sát thương

        // ---------- Nâng cấp ----------
        public const int   UPGRADES_ON_CORRECT = 3;       // đúng → chọn 1 trong 3
        public const int   UPGRADES_ON_WRONG = 2;         // sai → vẫn được 2 nâng cấp thường
        public const float RARE_CHANCE = 0.20f;           // tỷ lệ ra Rare trong pool
        public const float EPIC_CHANCE = 0.10f;
        public const float LEGENDARY_CHANCE_ON_CORRECT = 0.05f; // Legendary chỉ khi đúng

        // ---------- VOID THREAT SYSTEM ----------
        // Threat (0..100) tích từ hành vi chơi; đạt 100 → Boss xuất hiện.
        // Kết hợp cả SURVIVAL progress (thời gian, kills) và KNOWLEDGE progress (câu hỏi, level up).
        public const float THREAT_MAX = 100f;

        // Điểm threat cộng mỗi lần (tùy chỉnh được - xem ThreatConfig trong Manager)
        public const float THREAT_PER_SECOND = 0.10f;     // 0.1 điểm/giây → 1 phút sống ≈ +6
        public const float THREAT_PER_KILL = 1.5f;        // mỗi enemy thường giết được
        public const float THREAT_PER_ELITE_KILL = 5f;    // mỗi ELITE giết được
        public const float THREAT_PER_LEVELUP = 8f;       // mỗi lần lên cấp (level up)
        public const float THREAT_PER_CORRECT_ANSWER = 3f;// mỗi câu trả lời ĐÚNG
        public const float THREAT_PER_WRONG_ANSWER = 1f;  // trả lời SAI vẫn tích nhẹ (vẫn tiến triển)

        // Boss Warning: đếm ngược cho player chuẩn bị trước khi boss xuất hiện
        public const float BOSS_WARNING_DURATION = 5f;    // 5 giây chuẩn bị

        // Sau khi boss chết, threat reset về mức này để có thể tích lại (endless mode)
        public const float THREAT_AFTER_BOSS_RESET = 25f;

        // ---------- Boss ----------
        public const int   BOSS_HP = 60;                  // nhân theo số lần boss xuất hiện
        public const float BOSS_SPEED = 1.2f;
        public const int   BOSS_DMG = 2;
        public const int   BOSS_XP = 15;
        public const float BOSS_CLASH_HP_THRESHOLD = 0.7f; // còn 70% HP → Knowledge Clash
        public const float BOSS_CLASH_SLOWMO_SCALE = 0.15f; // slow-motion khi Clash
        public const float BOSS_CLASH_SLOWMO_TIME = 1.0f;   // thời gian slow-mo trước khi hiện câu hỏi
        public const float BOSS_ULT_DAMAGE = 3;             // sát thương Ultimate nếu trả lời sai
        public const float BOSS_STUN_TIME = 5f;             // boss bị choáng khi player trả lời đúng
        public const float BOSS_ENRAGE_THRESHOLD = 0.3f;    // còn 30% HP → ENRAGED
        public const float BOSS_ENRAGE_SPEED_MULT = 1.5f;
        public const float BOSS_ENRAGE_ATK_MULT = 1.5f;
        public const int   BOSS_ENRAGE_EXTRA_BULLET = 2;    // bắn thêm đạn khi enraged

        // ---------- Điểm ----------
        public const int POINTS_PER_KILL = 10;
        public const int POINTS_PER_LEVEL = 100;
        public const int POINTS_WIN_BONUS = 2000;

        // ---------- Tên GameObject / Canvas ----------
        public const string LEVEL_MANAGER_GO_NAME = "BiologySurvivalManager";
        public const string QUIZ_UI_GO_NAME = "BiologyQuizUI";
        public const string HUD_GO_NAME = "BiologyHUD";
    }
}
