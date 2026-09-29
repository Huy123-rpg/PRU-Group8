// ============================================================
// BiologySpriteFactory.cs
// Sinh toàn bộ hình ảnh 2D bằng code (không cần asset ngoài).
// Mỗi sprite được vẽ bằng "pixel art ASCII" rồi quy đổi PPU
// theo CHIỀU CAO thế giới mong muốn → kích thước trong game
// luôn đúng thiết kế, collider khớp hình.
// Muốn thay art thật: kéo Sprite của bạn vào tham chiếu tương
// ứng trong Inspector (nếu có), script sẽ ưu tiên dùng.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

namespace PRU.Biology
{
    public static class BiologySpriteFactory
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        // ------------------- API công khai -------------------

        public static Sprite GetPlayerSprite()       => Get("player", CreatePlayer);
        public static Sprite GetObstacleSprite()     => Get("obst",   CreateObstacle);
        public static Sprite GetBossSprite()         => Get("boss",   CreateBoss);
        public static Sprite GetProjectileSprite()   => Get("proj",   CreateProjectile);
        public static Sprite GetHeartSprite()        => Get("heart",  CreateHeart);
        public static Sprite GetGroundSprite()       => Get("ground", CreateGround);
        public static Sprite GetCheckpointSprite()   => Get("cp",     CreateCheckpoint);
        public static Sprite GetPatrolVirusSprite()  => Get("patrol", CreatePatrolVirus);
        public static Sprite GetSpitterSprite()      => Get("spit",   CreateSpitter);
        public static Sprite GetPlatformSprite()     => Get("plat",   CreatePlatform);
        public static Sprite GetGateSprite()         => Get("gate",   CreateGate);
        public static Sprite GetQuestionMarkSprite() => Get("qmark",  CreateQuestionMark);
        public static Sprite GetWhiteSprite()        => Get("white",  CreateWhite);

        // ---- Game phòng thủ miễn dịch ----
        public static Sprite GetHeartOrganSprite()   => Get("organ",  CreateHeartOrgan);
        public static Sprite GetPathogenSprite()     => Get("germ",   CreatePathogen);
        public static Sprite GetBossPathogenSprite() => Get("germB",  CreateBossPathogen);
        public static Sprite GetAntibodySprite()     => Get("ab",      CreateAntibody);
        public static Sprite GetSlotSprite()         => Get("slot",    CreateSlot);
        public static Sprite GetXpBubbleSprite()     => Get("xpb",     CreateXpBubble);

        // ---- Game sinh tồn: 3 loại quái + đạn địch + tâm ngắm ----
        public static Sprite GetChaserSprite()       => Get("chaser",  CreateChaser);
        public static Sprite GetShooterSprite()      => Get("shooter", CreateShooter);
        public static Sprite GetChargerSprite()      => Get("charger", CreateCharger);
        public static Sprite GetEnemyBulletSprite()  => Get("ebullet", CreateEnemyBullet);
        public static Sprite GetCrosshairSprite()    => Get("xhair",   CreateCrosshair);

        // ---- Theme trường học: vũ khí học tập + boss bài kiểm tra ----
        // index: 0 Pencil Blaster / 1 Ruler Blade / 2 Book Orbit /
        //        3 Calculator Lightning / 4 Chemical Flask
        public static Sprite GetWeaponSprite(int index)
        {
            switch (Mathf.Clamp(index, 0, 4))
            {
                case 1: return Get("weaponRuler", CreateWeaponRuler);
                case 2: return Get("weaponBook", CreateWeaponBook);
                case 3: return Get("weaponCalc", CreateWeaponCalculator);
                case 4: return Get("weaponFlask", CreateWeaponFlask);
                default: return Get("weaponPencil", CreateWeaponPencil);
            }
        }
        public static Sprite GetExamBossSprite()     => Get("examBoss", CreateExamBoss);
        public static Sprite GetBossWarningSprite()  => Get("bossWarn", CreateBossWarning);

        public static Sprite GetTowerSprite(BiologyTowerType t)
        {
            switch (t)
            {
                case BiologyTowerType.LymphoB: return Get("towerB", CreateLymphoB);
                case BiologyTowerType.LymphoT: return Get("towerT", CreateLymphoT);
                default: return Get("towerM", CreateMacrophage);
            }
        }

        // ------------------- Nội bộ -------------------

        private static Sprite Get(string key, System.Func<Sprite> factory)
        {
            if (_cache.TryGetValue(key, out Sprite s) && s != null) return s;
            s = factory();
            _cache[key] = s;
            return s;
        }

        private static Texture2D NewTex(int w, int h)
        {
            Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Point;
            Color[] px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
            t.SetPixels(px);
            return t;
        }

        /// <summary>
        /// Vẽ mẫu ASCII lên toàn bộ texture (góc dưới-trái là gốc).
        /// '.' hoặc ' ' = trong suốt. Kích thước texture PHẢI bằng cỡ art.
        /// </summary>
        private static void Stamp(Texture2D tex, string[] rows, Dictionary<char, Color> palette)
        {
            int h = rows.Length;
            for (int y = 0; y < h; y++)
            {
                string row = rows[h - 1 - y]; // phần tử đầu mảng = đỉnh
                for (int x = 0; x < row.Length; x++)
                {
                    char ch = row[x];
                    if (ch == '.' || ch == ' ') continue;
                    if (!palette.TryGetValue(ch, out Color col)) continue;
                    tex.SetPixel(x, y, col);
                }
            }
        }

        /// <summary>Hoàn tất texture & tạo sprite với chiều cao thế giới = worldHeight (đơn vị Unity).</summary>
        private static Sprite Finalize(Texture2D t, float worldHeight)
        {
            t.Apply();
            float ppu = t.height / Mathf.Max(0.01f, worldHeight);
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu, 0,
                                 SpriteMeshType.FullRect);
        }

        // ------------------- NHÂN VẬT (sinh viên) - cao 1.4 đơn vị -------------------

        private static Sprite CreatePlayer()
        {
            Texture2D t = NewTex(8, 14);
            var pal = new Dictionary<char, Color>
            {
                ['K'] = new Color(0.15f, 0.15f, 0.18f), // tóc đen
                ['S'] = new Color(0.98f, 0.85f, 0.72f), // da
                ['W'] = new Color(1f, 1f, 1f),          // áo trắng
                ['B'] = new Color(0.24f, 0.46f, 0.85f), // xanh quần
                ['G'] = new Color(0.30f, 0.30f, 0.34f), // kính
                ['C'] = new Color(0.18f, 0.62f, 0.55f), // thắt lưng mint
            };
            Stamp(t, new[]
            {
                "..KKKK..",
                ".KKKKKK.",
                ".KSSSSK.",
                ".GSSSSG.",
                ".GKKKKG.",
                "..SSSS..",
                ".WWWWWW.",
                "WWBWWBWW",
                "WWBWWBWW",
                ".WWWWWW.",
                ".CCCCCC.",
                "..C..C..",
                ".BB..BB.",
                ".BB..BB.",
            }, pal);
            return Finalize(t, 1.4f);
        }

        // ------------------- VẬT CẢN (virus) - cao 1.1 đơn vị -------------------

        private static Sprite CreateObstacle()
        {
            Texture2D t = NewTex(13, 12);
            var pal = new Dictionary<char, Color>
            {
                ['V'] = new Color(0.62f, 0.16f, 0.38f), // thân virus hồng đậm
                ['D'] = new Color(0.42f, 0.08f, 0.26f), // gai + viền
                ['E'] = new Color(1f, 0.95f, 0.4f),     // mắt vàng
                ['M'] = new Color(0.35f, 0.05f, 0.20f), // miệng
            };
            Stamp(t, new[]
            {
                "D....D....D..",
                ".D..D.D..D...",
                ".DVVVVVVVVD..",
                "DVVEVVVEVVVD.",
                ".VVVVVVVVVV..",
                "DVVVMMMVVVVD.",
                ".VVVMMMVVVV..",
                "DVVVVVVVVVVD.",
                ".VVVVVVVVVV..",
                ".D.VVVVVV.D..",
                "D...DDDD...D.",
                "..D......D...",
            }, pal);
            return Finalize(t, 1.1f);
        }

        // ------------------- BOSS (vi khuẩn to) - cao 3.2 đơn vị -------------------

        private static Sprite CreateBoss()
        {
            Texture2D t = NewTex(24, 14);
            var pal = new Dictionary<char, Color>
            {
                ['P'] = new Color(0.55f, 0.20f, 0.72f), // tím thân
                ['D'] = new Color(0.35f, 0.10f, 0.48f), // viền đậm
                ['E'] = new Color(1f, 0.9f, 0.2f),      // mắt
                ['K'] = new Color(0.1f, 0.05f, 0.15f),  // con ngươi
                ['M'] = new Color(0.9f, 0.3f, 0.35f),   // miệng
                ['F'] = new Color(0.75f, 0.45f, 0.9f),  // lông rung
            };
            Stamp(t, new[]
            {
                "...F..F..F..F..F..F...",
                "..F................F..",
                "..DPPPPPPPPPPPPPPPPD..",
                ".DPPPPPPPPPPPPPPPPPPD.",
                ".DPPDDEEPPPPPPDDEEPPD.",
                "DPPPDEKEEPPPDPDEKEEPPD",
                "DPPPPDDEPPPPPPPDDEPPPD",
                "DPPPPPPPPPPPPPPPPPPPPD",
                "DPPPPMMMMMMMMMMMMPPPPD",
                ".DPPPMEMEMEMEMEMMPPPD.",
                ".DPPPMMMMMMMMMMPPPPPD.",
                "..DPPPPPPPPPPPPPPPPD..",
                "...D.PP.PP.PP.PP.D....",
                "..F..D..D..D..D...F...",
            }, pal);
            return Finalize(t, 3.2f);
        }

        // ------------------- ĐẠN BOSS - cao 0.35 đơn vị -------------------

        private static Sprite CreateProjectile()
        {
            Texture2D t = NewTex(8, 6);
            var pal = new Dictionary<char, Color>
            {
                ['A'] = new Color(0.95f, 0.4f, 0.85f), // hồng neon
                ['B'] = new Color(1f, 0.85f, 0.4f),    // lõi vàng
            };
            Stamp(t, new[]
            {
                "..AAAA..",
                ".AABBAA.",
                "AABBBBAA",
                "AABBBBAA",
                ".AABBAA.",
                "..AAAA..",
            }, pal);
            return Finalize(t, 0.35f);
        }

        // ------------------- TRÁI TIM (UI) -------------------

        private static Sprite CreateHeart()
        {
            Texture2D t = NewTex(8, 7);
            var pal = new Dictionary<char, Color>
            {
                ['R'] = new Color(0.92f, 0.25f, 0.35f),
                ['W'] = new Color(1f, 1f, 1f),
            };
            Stamp(t, new[]
            {
                ".RR..RR.",
                "RRRRRRRR",
                "RWRRRRRR",
                "RRRRRRRR",
                ".RRRRRR.",
                "..RRRR..",
                "...RR...",
            }, pal);
            return Finalize(t, 0.5f);
        }

        // ------------------- MẶT ĐẤT - viên 2 x 1 đơn vị -------------------

        private static Sprite CreateGround()
        {
            Texture2D t = NewTex(32, 16);
            Color grass = new Color(0.42f, 0.68f, 0.32f);
            Color grassDark = new Color(0.30f, 0.52f, 0.24f);
            Color dirt = new Color(0.55f, 0.38f, 0.24f);
            Color dirtLight = new Color(0.63f, 0.46f, 0.30f);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Color c;
                    if (y >= 12) // 4 hàng trên: cỏ
                    {
                        c = grass;
                        if ((x + y) % 5 == 0) c = grassDark;
                        if (y == 15) c = grassDark; // viền trên đậm
                    }
                    else // đất đá
                    {
                        c = dirt;
                        if ((x * 7 + y * 3) % 11 == 0) c = dirtLight;
                    }
                    t.SetPixel(x, y, c);
                }
            }
            return Finalize(t, 1f); // 32x16 px → 2 x 1 đơn vị
        }

        // ------------------- CỘT MỐC - cao 1.5 đơn vị -------------------

        private static Sprite CreateCheckpoint()
        {
            Texture2D t = NewTex(8, 12);
            var pal = new Dictionary<char, Color>
            {
                ['W'] = new Color(0.9f, 0.9f, 0.9f),
                ['G'] = new Color(0.25f, 0.7f, 0.45f),
            };
            Stamp(t, new[]
            {
                "..GGGG..",
                ".GGGGGG.",
                "GGGGGGGG",
                "GGGGGGGG",
                "..GGGG..",
                "...WW...",
                "...WW...",
                "...WW...",
                "...WW...",
                "...WW...",
                "...WW...",
                "...WW...",
            }, pal);
            return Finalize(t, 1.5f);
        }

        // ------------------- DẤU HỎI TRANG TRÍ - cao 1 đơn vị -------------------

        private static Sprite CreateQuestionMark()
        {
            Texture2D t = NewTex(8, 10);
            var pal = new Dictionary<char, Color>
            {
                ['Y'] = new Color(1f, 0.85f, 0.3f),
            };
            Stamp(t, new[]
            {
                "..YYYY..",
                ".YY..YY.",
                ".YY..YY.",
                ".....YY.",
                "....YY..",
                "...YY...",
                "...YY...",
                "........",
                "...YY...",
                "...YY...",
            }, pal);
            return Finalize(t, 1f);
        }

        // ------------------- VIRUS ĐI TUẦN - cao 1.0 -------------------

        private static Sprite CreatePatrolVirus()
        {
            Texture2D t = NewTex(11, 10);
            var pal = new Dictionary<char, Color>
            {
                ['V'] = new Color(0.85f, 0.30f, 0.25f), // đỏ tươi
                ['D'] = new Color(0.55f, 0.12f, 0.12f),
                ['E'] = new Color(1f, 1f, 1f),
                ['K'] = new Color(0.1f, 0.1f, 0.1f),
            };
            Stamp(t, new[]
            {
                "..D..D..D..",
                ".D.DD.DD.D.",
                ".DVVVVVVVD.",
                "DVVEVVEVVVD",
                ".VVVKVVKVV.",
                "DVVVVVVVVVD",
                ".VVDVVVDVV.",
                "DVVVVVVVVVD",
                ".D.VVVVV.D.",
                "..D.DD.D...",
            }, pal);
            return Finalize(t, 1.0f);
        }

        // ------------------- NẤM ĐỘC BẮN ĐẠN - cao 1.3 -------------------

        private static Sprite CreateSpitter()
        {
            Texture2D t = NewTex(12, 13);
            var pal = new Dictionary<char, Color>
            {
                ['C'] = new Color(0.45f, 0.75f, 0.30f), // mũ nấm
                ['D'] = new Color(0.25f, 0.45f, 0.15f),
                ['S'] = new Color(0.95f, 0.92f, 0.75f), // thân
                ['E'] = new Color(0.9f, 0.2f, 0.3f),    // đốm độc
                ['K'] = new Color(0.1f, 0.1f, 0.1f),
                ['M'] = new Color(0.5f, 0.1f, 0.15f),
            };
            Stamp(t, new[]
            {
                "...CCCCC....",
                "..CCECCECC..",
                ".CCCCCCCCC..",
                ".CCECCCCECC.",
                "CCCCCCCCCCCC",
                ".DDDDDDDDDD.",
                "..SSSSSSSS..",
                "..SKSSSSKS..",
                "..SSSMMSSS..",
                "..SSSMMSSS..",
                "..SSSSSSSS..",
                ".SSSSSSSSSS.",
                ".DD......DD.",
            }, pal);
            return Finalize(t, 1.3f);
        }

        // ------------------- BỤC NHẢY (nền tế bào) - rộng 3 x cao 0.5 -------------------

        private static Sprite CreatePlatform()
        {
            Texture2D t = NewTex(24, 4);
            Color top = new Color(0.55f, 0.80f, 0.95f);
            Color body = new Color(0.30f, 0.55f, 0.80f);
            Color dark = new Color(0.20f, 0.38f, 0.60f);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 24; x++)
                {
                    Color c = y == 3 ? top : body;
                    if (y == 0) c = dark;
                    if (y == 2 && (x % 6) < 2) c = dark;
                    t.SetPixel(x, y, c);
                }
            return Finalize(t, 0.5f);
        }

        // ------------------- CỔNG BOSS - cao 3.4 -------------------

        private static Sprite CreateGate()
        {
            Texture2D t = NewTex(10, 17);
            var pal = new Dictionary<char, Color>
            {
                ['F'] = new Color(1f, 0.95f, 0.55f), // khung vàng
                ['G'] = new Color(0.4f, 0.85f, 0.5f), // lõi xanh (cổng đóng)
            };
            Stamp(t, new[]
            {
                "..FFFFFF..",
                ".FFGGGGFF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FGGGGGGF.",
                ".FFGGGGFF.",
                "..FFFFFF..",
            }, pal);
            return Finalize(t, 3.4f);
        }

        // =================== GAME PHÒNG THỦ MIỄN DỊCH ===================

        // ------------------- TRÁI TIM (cơ quan cần bảo vệ) - cao 2.0 -------------------

        private static Sprite CreateHeartOrgan()
        {
            Texture2D t = NewTex(16, 14);
            var pal = new Dictionary<char, Color>
            {
                ['R'] = new Color(0.85f, 0.18f, 0.28f),
                ['D'] = new Color(0.60f, 0.08f, 0.16f),
                ['W'] = new Color(1f, 0.85f, 0.85f),
            };
            Stamp(t, new[]
            {
                "..RRR....RRR....",
                ".RRRRR..RRRRR...",
                "RRWRRRRRRRRRRRD.",
                "RRWRRRRRRRRRRRD.",
                "RRRRRRRRRRRRRRD.",
                "RRRRRRRRRRRRRD..",
                ".RRRRRRRRRRRD...",
                ".RRRRRRRRRRD....",
                "..RRRRRRRRD.....",
                "...RRRRRRD......",
                "....RRRRD.......",
                ".....RRD........",
                ".....RD.........",
                ".....D..........",
            }, pal);
            return Finalize(t, 2.0f);
        }

        // ------------------- MẦM BỆNH (vi khuẩn) - cao 1.0 -------------------

        private static Sprite CreatePathogen()
        {
            Texture2D t = NewTex(12, 11);
            var pal = new Dictionary<char, Color>
            {
                ['G'] = new Color(0.45f, 0.75f, 0.35f),
                ['D'] = new Color(0.25f, 0.45f, 0.18f),
                ['E'] = new Color(1f, 1f, 1f),
                ['K'] = new Color(0.1f, 0.12f, 0.1f),
                ['M'] = new Color(0.3f, 0.12f, 0.2f),
            };
            Stamp(t, new[]
            {
                "..D..D..D...",
                ".D.GGGGGG.D.",
                ".GGEGGGGEGG.",
                "DGGKGGGGKGGD",
                ".GGGGGGGGGG.",
                "DGMMMGGMMMGD",
                ".GGMGGGGMGG.",
                "DGGGGGGGGGGD",
                ".GGDGGGGDGG.",
                ".D..DD.D..D.",
                "....D..D....",
            }, pal);
            return Finalize(t, 1.0f);
        }

        // ------------------- BOSS MẦM BỆNH - cao 1.0 (scale x2.2 khi spawn) -------------------

        private static Sprite CreateBossPathogen()
        {
            Texture2D t = NewTex(16, 14);
            var pal = new Dictionary<char, Color>
            {
                ['P'] = new Color(0.62f, 0.30f, 0.75f),
                ['D'] = new Color(0.38f, 0.12f, 0.50f),
                ['E'] = new Color(1f, 0.9f, 0.25f),
                ['K'] = new Color(0.1f, 0.05f, 0.1f),
                ['M'] = new Color(0.85f, 0.25f, 0.35f),
            };
            Stamp(t, new[]
            {
                ".D....D..D....D.",
                "..D.PPPPPPPP.D..",
                ".DPPPPPPPPPPPPD.",
                "DPPPEEPPPPEEPPPD",
                "DPPPKKPPPPKKPPPD",
                "DPPPPPPPPPPPPPPD",
                "DPPPMPPPPPPMPPPD",
                ".DPPMMMMMMMMPPD.",
                ".DPPPMEMEMMPPPD.",
                "..DPPPPPPPPPPD..",
                "...DPPPPPPPPD...",
                ".D..D.DD.D..D...",
                ".D............D.",
                "................",
            }, pal);
            return Finalize(t, 1.0f);
        }

        // ------------------- KHÁNG THỂ (đạn) - cao 0.45 -------------------

        private static Sprite CreateAntibody()
        {
            Texture2D t = NewTex(7, 9);
            var pal = new Dictionary<char, Color>
            {
                ['Y'] = new Color(1f, 0.95f, 0.4f),
            };
            Stamp(t, new[]
            {
                "YY...YY",
                "YY...YY",
                ".YY.YY.",
                "..YYY..",
                "...Y...",
                "...Y...",
                "...Y...",
                "...Y...",
                "..YYY..",
            }, pal);
            return Finalize(t, 0.45f);
        }

        // ------------------- Ô ĐẶT TẾ BÀO - cao 1.4 -------------------

        private static Sprite CreateSlot()
        {
            Texture2D t = NewTex(14, 14);
            Color ring = new Color(0.4f, 0.7f, 0.5f);
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 14; x++)
                {
                    float dx = x - 6.5f, dy = y - 6.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist >= 5.5f && dist <= 6.5f)
                    {
                        // vòng tròn nét đứt
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg + 180f;
                        if ((int)(angle / 22.5f) % 2 == 0) t.SetPixel(x, y, ring);
                    }
                }
            return Finalize(t, 1.4f);
        }

        // ------------------- ĐẠI THỰC BÀO - cao 1.6 -------------------

        private static Sprite CreateMacrophage()
        {
            Texture2D t = NewTex(16, 13);
            var pal = new Dictionary<char, Color>
            {
                ['W'] = new Color(0.95f, 0.90f, 0.80f), // thân trắng ngà
                ['D'] = new Color(0.65f, 0.55f, 0.45f), // viền
                ['E'] = new Color(0.15f, 0.15f, 0.25f), // mắt
                ['M'] = new Color(0.55f, 0.20f, 0.25f), // miệng
            };
            Stamp(t, new[]
            {
                "....DDDDDD......",
                "..DDWWWWWWDD....",
                ".DWWWWWWWWWWD...",
                "DWWWEEWWWEEWWD..",
                "DWWWEEWWWEEWWWD.",
                "DWWWWWWWWWWWWWD.",
                "DWWWWMMWWWWMWWD.",
                ".DWWWMMMMMWWWD..",
                ".DWWWWMMWWWWWD..",
                "..DWWWWWWWWD.D..",
                "...DDWWWDDD.D...",
                ".D....DDD....D..",
                "................",
            }, pal);
            return Finalize(t, 1.6f);
        }

        // ------------------- LYMPHO B - cao 1.4 -------------------

        private static Sprite CreateLymphoB()
        {
            Texture2D t = NewTex(13, 12);
            var pal = new Dictionary<char, Color>
            {
                ['B'] = new Color(0.40f, 0.60f, 0.95f),
                ['D'] = new Color(0.20f, 0.35f, 0.70f),
                ['Y'] = new Color(1f, 0.95f, 0.45f), // thụ thể Y
                ['W'] = new Color(0.85f, 0.92f, 1f),
            };
            Stamp(t, new[]
            {
                "..Y..Y..Y....",
                ".YYY.YY.YYY..",
                ".B.YYYYYY.B..",
                ".BBBBBBBBBB..",
                "BBBBBBBBBBBB.",
                "BWWBBBBBBBWBD",
                "BWWBBBBBBBWBD",
                "BBBBBBBBBBBB.",
                ".BBBBBBBBBB..",
                ".BBBBBBBBBD..",
                "..DBBBBBBD...",
                "....DDDD.....",
            }, pal);
            return Finalize(t, 1.4f);
        }

        // ------------------- LYMPHO T (sát thủ) - cao 1.4 -------------------

        private static Sprite CreateLymphoT()
        {
            Texture2D t = NewTex(14, 13);
            var pal = new Dictionary<char, Color>
            {
                ['O'] = new Color(0.95f, 0.45f, 0.30f),
                ['D'] = new Color(0.65f, 0.20f, 0.15f),
                ['E'] = new Color(0.95f, 0.85f, 0.2f),
                ['K'] = new Color(0.15f, 0.1f, 0.1f),
            };
            Stamp(t, new[]
            {
                "D..D......D.D.",
                ".D.OOOOOO..D..",
                ".DOOOOOOOOD...",
                "DOOEOOOOEOOOD.",
                "DOOKKOOOKKOOD.",
                "DOOOOOOOOOOOD.",
                ".DOODDDDOOOD..",
                ".DOOOOOOOOD...",
                "..DOOOOOOD....",
                "...DOOOOD.....",
                "....DDDD......",
                "..............",
                "..............",
            }, pal);
            return Finalize(t, 1.4f);
        }

        // ------------------- BONG BÓNG XP (rơi ra khi mầm bệnh chết) - cao 0.5 -------------------

        private static Sprite CreateXpBubble()
        {
            Texture2D t = NewTex(10, 10);
            var pal = new Dictionary<char, Color>
            {
                ['C'] = new Color(0.35f, 0.85f, 1f, 0.9f),  // viền xanh ngọc
                ['W'] = new Color(1f, 1f, 1f, 0.95f),        // lõi sáng
            };
            Stamp(t, new[]
            {
                "...CCCC...",
                "..C....C..",
                ".C..WW..C.",
                "C..W..W..C",
                "C..W..W..C",
                "C..WWWW..C",
                "C...WW...C",
                ".C......C.",
                "..C....C..",
                "...CCCC...",
            }, pal);
            return Finalize(t, 0.5f);
        }

        // ------------------- MẦM BỆNH ĐUỔI THEO (dùng lại germ xanh) -------------------
        private static Sprite CreateChaser() => CreatePathogen();

        // ------------------- MẦM BỆNH BẮN XA - cao 1.1 (gai tím nhả độc) -------------------

        private static Sprite CreateShooter()
        {
            Texture2D t = NewTex(13, 12);
            var pal = new Dictionary<char, Color>
            {
                ['P'] = new Color(0.68f, 0.35f, 0.85f), // thân tím
                ['D'] = new Color(0.42f, 0.15f, 0.58f), // gai + viền
                ['E'] = new Color(1f, 0.95f, 0.35f),    // mắt vàng
                ['K'] = new Color(0.12f, 0.05f, 0.15f), // ngươi
                ['M'] = new Color(0.35f, 0.08f, 0.30f), // miệng
            };
            Stamp(t, new[]
            {
                "D....D....D.",
                ".D..DDD..D..",
                ".DPPPPPPPPD.",
                "DPPPEPPEPPPD",
                ".PPPKPPKPPP.",
                "DPPPPPPPPPPD",
                ".PPMPPPMPPP.",
                "DPPMMMMMMPPD",
                ".DPPPPPPPPD.",
                ".D.PPPPPP.D..",
                "D..DD..DD..D.",
                "..D..D...D...",
            }, pal);
            return Finalize(t, 1.1f);
        }

        // ------------------- MẦM BỆNH LAO VÀO - cao 1.2 (sừng đỏ, hình mũi tên) -------------------

        private static Sprite CreateCharger()
        {
            Texture2D t = NewTex(13, 13);
            var pal = new Dictionary<char, Color>
            {
                ['R'] = new Color(0.88f, 0.35f, 0.25f), // thân đỏ cam
                ['D'] = new Color(0.55f, 0.15f, 0.10f), // viền + sừng
                ['E'] = new Color(1f, 0.9f, 0.3f),      // mắt
                ['K'] = new Color(0.1f, 0.05f, 0.05f),  // ngươi
            };
            Stamp(t, new[]
            {
                "......DD.....",
                ".....DRRD....",
                ".D...RRRR..D.",
                ".DRRRRRRRRRD.",
                ".DRRRRRRRRRD.",
                "DRREERREERRRD",
                ".DRKRRRRKRRD.",
                ".DRRRRRRRRRD.",
                "..DRRRRRRRD..",
                "...DRRRRRD...",
                "....DRRRD....",
                ".....DRD.....",
                "......D......",
            }, pal);
            return Finalize(t, 1.2f);
        }

        // ------------------- ĐẠN ĐỊCH (gai độc) - cao 0.35 -------------------

        private static Sprite CreateEnemyBullet()
        {
            Texture2D t = NewTex(7, 7);
            var pal = new Dictionary<char, Color>
            {
                ['P'] = new Color(0.85f, 0.45f, 1f),   // tím sáng
                ['W'] = new Color(1f, 0.9f, 1f),       // lõi
            };
            Stamp(t, new[]
            {
                ".PPPPP.",
                "PPPWPPP",
                "PPWWWPP",
                "PPWWWPP",
                "PPPWPPP",
                "PPPWPPP",
                ".PPPPP.",
            }, pal);
            return Finalize(t, 0.35f);
        }

        // ------------------- TÂM NGẮM (crosshair cho chuột) - cao 0.7 -------------------

        private static Sprite CreateCrosshair()
        {
            Texture2D t = NewTex(11, 11);
            var pal = new Dictionary<char, Color>
            {
                ['C'] = new Color(0.2f, 1f, 0.6f, 0.95f), // xanh lá neon
                ['W'] = new Color(1f, 1f, 1f, 0.9f),
            };
            Stamp(t, new[]
            {
                "....CCC....",
                "....CCC....",
                "...........",
                "...........",
                "CC.......CC",
                "CCC..W..CCC",
                "CCC..W..CCC",
                "CC.......CC",
                "...........",
                "....CCC....",
                "....CCC....",
            }, pal);
            return Finalize(t, 0.7f);
        }

        // ------------------- VŨ KHÍ HỌC TẬP -------------------
        // 0: BÚT CHÌ BLASTER - bút chì vàng, mũi đen (cao 0.9)
        private static Sprite CreateWeaponPencil()
        {
            Texture2D t = NewTex(5, 14);
            var pal = new Dictionary<char, Color>
            {
                ['Y'] = new Color(0.95f, 0.78f, 0.25f), // thân gỗ vàng
                ['D'] = new Color(0.72f, 0.55f, 0.15f), // viền
                ['K'] = new Color(0.15f, 0.15f, 0.18f), // chì đen
                ['W'] = new Color(0.94f, 0.90f, 0.78f), // gỗ sharpened
                ['P'] = new Color(0.85f, 0.30f, 0.35f), // tẩy hồng
                ['M'] = new Color(0.55f, 0.55f, 0.60f), // ferrule kim loại
            };
            Stamp(t, new[]
            {
                "..P..",
                ".PPP.",
                ".MMM.",
                ".YYY.",
                ".YYY.",
                ".YYY.",
                ".YYY.",
                ".YYY.",
                ".YYY.",
                ".YYY.",
                ".YYY.",
                ".YYY.",
                ".WWW.",
                "..K..",
            }, pal);
            return Finalize(t, 0.9f);
        }

        // 1: THƯỚC KẺ BLADE - thước gỗ có khoảng đo (cao 1.0, nằm ngang)
        private static Sprite CreateWeaponRuler()
        {
            Texture2D t = NewTex(20, 6);
            var pal = new Dictionary<char, Color>
            {
                ['W'] = new Color(0.93f, 0.86f, 0.66f), // gỗ nhạt
                ['D'] = new Color(0.70f, 0.58f, 0.35f), // viền
                ['K'] = new Color(0.25f, 0.22f, 0.18f), // vạch đo
                ['R'] = new Color(0.85f, 0.30f, 0.30f), // lưỡi dao sắc
            };
            Stamp(t, new[]
            {
                "DDDDDDDDDDDDDDDDDDDD",
                "KWKWKWKWKWKWKWKWKWRR",
                "KWKWKWKWKWKWKWKWKWRR",
                "KWKWKWKWKWKWKWKWKWRR",
                "WWWWWWWWWWWWWWWWWWRR",
                "DDDDDDDDDDDDDDDDDDDD",
            }, pal);
            return Finalize(t, 0.8f);
        }

        // 2: SÁCH BAY (book orbit) - quyển sách xanh mở trang (cao 0.9)
        private static Sprite CreateWeaponBook()
        {
            Texture2D t = NewTex(14, 11);
            var pal = new Dictionary<char, Color>
            {
                ['B'] = new Color(0.25f, 0.45f, 0.75f), // bìa xanh
                ['D'] = new Color(0.15f, 0.28f, 0.52f), // viền
                ['W'] = new Color(0.98f, 0.96f, 0.88f), // trang giấy
                ['Y'] = new Color(0.85f, 0.68f, 0.30f), // chữ trên bìa
            };
            Stamp(t, new[]
            {
                ".DDDDDDDDDD..",
                "DBBBBBBBBBDW.",
                "DBYBBYBBYBDWW.",
                "DBBBBBBBBDWWW.",
                "DBYBBYBBYBDWWW",
                "DBBBBBBBBDWWW.",
                "DBYBBYBBYBDWW.",
                "DBBBBBBBBBDW..",
                ".DDDDDDDDDD...",
                "..WWWWWWWW....",
                "...WWWWWW.....",
            }, pal);
            return Finalize(t, 0.9f);
        }

        // 3: MÁY TÍNH SÉT (calculator lightning) - máy tính vàng có sét (cao 0.9)
        private static Sprite CreateWeaponCalculator()
        {
            Texture2D t = NewTex(10, 13);
            var pal = new Dictionary<char, Color>
            {
                ['C'] = new Color(0.35f, 0.36f, 0.42f), // thân máy
                ['D'] = new Color(0.20f, 0.21f, 0.26f), // viền
                ['G'] = new Color(0.55f, 0.95f, 0.65f), // màn hình
                ['K'] = new Color(0.12f, 0.12f, 0.15f), // phím
                ['Y'] = new Color(1f, 0.9f, 0.30f),     // tia sét
            };
            Stamp(t, new[]
            {
                ".DDDDDDDD.",
                "DCCCCCCCCD",
                "DCGGGGGGYD",
                "DCGGGGGGYD",
                "DCCCCCCCCD",
                "DCKCKCKCKD",
                "DCKCKCKCKD",
                "DCKCKCKCKD",
                "DCKCKCKCKD",
                "DCCCCCCCCD",
                ".DDDDDDDD.",
                "....Y.....",
                "...Y......",
            }, pal);
            return Finalize(t, 0.9f);
        }

        // 4: BÌNH HOÁ CHẤT (chemical flask) - bình tam giác xanh sôi (cao 0.9)
        private static Sprite CreateWeaponFlask()
        {
            Texture2D t = NewTex(12, 13);
            var pal = new Dictionary<char, Color>
            {
                ['G'] = new Color(0.45f, 0.85f, 0.55f), // hoá chất xanh
                ['W'] = new Color(0.88f, 0.95f, 0.95f), // thuỷ tinh
                ['D'] = new Color(0.55f, 0.68f, 0.70f), // viền thuỷ tinh
                ['T'] = new Color(0.35f, 0.70f, 0.45f), // bọt
            };
            Stamp(t, new[]
            {
                "..DDDDDD....",
                "..WWWWWW....",
                "..DWWWWWD...",
                "...DWWWWD...",
                "...WWWWWW...",
                "..WWGGGGWW..",
                ".WWGGTGGGWW.",
                ".WWGGGGGGGW.",
                ".WWGTGGGGGW.",
                ".DWWGGGGGWD.",
                "..DDDDDDDD..",
                "....D..D....",
                "....G..G....",
            }, pal);
            return Finalize(t, 0.9f);
        }

        // ------------------- BOSS: BÀI KIỂM TRA MA (exam paper) - cao 1.0, scale x2.2 khi spawn -------------------

        private static Sprite CreateExamBoss()
        {
            Texture2D t = NewTex(16, 14);
            var pal = new Dictionary<char, Color>
            {
                ['W'] = new Color(0.96f, 0.94f, 0.85f), // giấy
                ['D'] = new Color(0.72f, 0.66f, 0.52f), // mép giấy
                ['R'] = new Color(0.85f, 0.20f, 0.20f), // mực đỏ: điểm + gương dữ
                ['K'] = new Color(0.15f, 0.10f, 0.10f), // chữ đen
                ['G'] = new Color(0.35f, 0.55f, 0.35f), // con dấu xanh
            };
            Stamp(t, new[]
            {
                ".DDDDDDDDDDDDD.",
                "DWWWWWWWWWWWWWD",
                "DWKKKKKKKKKWWD.",
                "DWKKKKKKKKKWWD.",
                "DWWWWWWWWWWWWWD",
                "DWRWWRRRWWWRWD.",
                "DWRRWRRRRWRRWD.",
                "DWWRRRRRRRRRWD.",
                "DWWRWKWKWKWRWD.",
                "DWWRKKKKKKKRWD.",
                "DWWRRRRRRRRRWD.",
                "DWWGGWWWWWWWD..",
                "DWWGGWWWWWWWD..",
                ".DDDDDDDDDDD...",
            }, pal);
            return Finalize(t, 1.0f);
        }

        // ------------------- BOSS WARNING: chấm than đỏ - cao 1.2 -------------------

        private static Sprite CreateBossWarning()
        {
            Texture2D t = NewTex(8, 12);
            var pal = new Dictionary<char, Color>
            {
                ['R'] = new Color(0.95f, 0.25f, 0.25f),
                ['D'] = new Color(0.55f, 0.08f, 0.08f),
            };
            Stamp(t, new[]
            {
                "..RRRR..",
                ".RRRRRR.",
                ".RRRRRR.",
                "..RRRR..",
                "...RR...",
                "...RR...",
                "...RR...",
                "...RR...",
                "...RR...",
                "........",
                "...RR...",
                "...RR...",
            }, pal);
            return Finalize(t, 1.2f);
        }

        // ------------------- Ô TRẮNG 1x1 (cho UI Image) -------------------

        private static Sprite CreateWhite()
        {
            Texture2D t = NewTex(4, 4);
            Color[] px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            t.SetPixels(px);
            t.filterMode = FilterMode.Bilinear;
            return Finalize(t, 1f);
        }
    }
}
