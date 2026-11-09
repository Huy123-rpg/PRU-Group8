using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ScienceQuest.Quiz;

namespace ScienceQuest.PhysicsGame1
{
    using QuestionData = ScienceQuest.Quiz.QuestionData;

    #region ===== 08_ResultScene Controller =====
    public class PhysicsGame1_ResultController : MonoBehaviour
    {
        private TextMeshProUGUI _tmpScore;
        private RectTransform _scoreRt;
        private GameObject _activeCharacterObj;
        private RectTransform _characterRt;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🏆 Khởi tạo 08_ResultScene (Bảng vàng thành tích)");

            float score = PhysicsGame1Session.Instance.CurrentScore;
            string charId = PhysicsGame1Session.Instance.SelectedCharacterId;
            if (string.IsNullOrEmpty(charId)) charId = "nv1";

            // 1. Thống kê số câu đúng & thời gian làm bài
            int totalQ = PhysicsGame1Session.Instance.MaxQuestionsForCurrentSession;
            int correctCount = 0;
            if (PhysicsGame1Session.Instance.QuestionResults != null && PhysicsGame1Session.Instance.QuestionResults.Count > 0)
            {
                for (int i = 0; i < PhysicsGame1Session.Instance.QuestionResults.Count; i++)
                {
                    var r = PhysicsGame1Session.Instance.QuestionResults[i];
                    if (r != null && r.isCorrect) correctCount++;
                }
            }
            else
            {
                correctCount = Mathf.Clamp(Mathf.RoundToInt(score / 10f), 0, totalQ);
            }

            float timeUsed = Mathf.Max(0f, 900f - PhysicsGame1Session.Instance.RemainingTime);
            if (timeUsed < 1f) timeUsed = 155f; // Mặc định thời gian mẫu nếu nạp trực tiếp scene
            int m = (int)(timeUsed / 60);
            int s = (int)(timeUsed % 60);
            string timeStr = $"{m:00}:{s:00}";

            // 2. Quy tắc hiển thị nhân vật & hiệu ứng sống động
            bool isHappy = (score >= (totalQ * 10 / 2));
            string activeCharName = isHappy ? (charId + "vui") : (charId + "buon");

            // 3. Trang trí ô điểm số lộng lẫy, in to và cân đối trong khung biển gỗ
            SetupDecoratedScoreBoard(score, correctCount, totalQ, timeStr, isHappy);

            if (isHappy)
            {
                PhysicsGame1_AudioManager.Instance.PlayCorrect();
            }
            else
            {
                PhysicsGame1_AudioManager.Instance.PlayWrong();
            }

            string[] allCharacterStateNames = new string[]
            {
                "nv1vui", "nv1buon",
                "nv2vui", "nv2buon",
                "nv3vui", "nv3buon",
                "nv4vui", "nv4buon"
            };

            foreach (var cname in allCharacterStateNames)
            {
                GameObject obj = GameObject.Find(cname);
                if (obj != null)
                {
                    bool isActive = cname.Equals(activeCharName, StringComparison.OrdinalIgnoreCase);
                    obj.SetActive(isActive);
                    if (isActive)
                    {
                        _activeCharacterObj = obj;
                        _characterRt = obj.GetComponent<RectTransform>();
                    }
                }
            }

            // Gắn hiệu ứng chuyển động, tương tác và hiệu ứng khóc procedural nếu thua điểm
            if (_characterRt != null)
            {
                StartCoroutine(CharacterIdleBreathingLoop(_characterRt, isHappy));

                if (!isHappy)
                {
                    AttachProceduralCryingEffect(_characterRt);
                }

                UIHelper.GetOrAddButton(_activeCharacterObj, () =>
                {
                    StartCoroutine(AnimateCharacterHop(_characterRt));
                });
            }

            // 4. Các nút điều hướng với hiệu ứng nảy nút & nhịp thở hấp dẫn
            SetupNavigationButtons();
        }

        #region ===== A. TRANG TRÍ Ô ĐIỂM SỐ (SCORE BOARD DECORATION) =====
        private void SetupDecoratedScoreBoard(float score, int correctCount, int totalQ, string timeStr, bool isHappy)
        {
            GameObject frameObj = GameObject.Find("framescore");
            if (frameObj == null) return;

            // Dọn dẹp các GameObject tiêu đề / thống kê cũ nếu Start gọi lại để tránh trùng lặp
            string[] cleanupNames = new string[] { "ResultHeaderTitle", "StarsRatingContainer", "StatsRowContainer", "AdviceText", "CloudSyncStatus" };
            foreach (var cName in cleanupNames)
            {
                Transform old = frameObj.transform.Find(cName);
                if (old != null) Destroy(old.gameObject);
            }

            // Hiệu ứng rơi bảng điểm đàn hồi khi vào màn chơi
            StartCoroutine(AnimateFrameEntrance(frameObj.GetComponent<RectTransform>()));

                        // 1. Text Tiêu đề Đánh Giá
            GameObject headerObj = new GameObject("ResultHeaderTitle");
            headerObj.transform.SetParent(frameObj.transform, false);
            RectTransform hrt = headerObj.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.5f, 0.5f);
            hrt.anchorMax = new Vector2(0.5f, 0.5f);
            hrt.pivot = new Vector2(0.5f, 0.5f);
            hrt.anchoredPosition = new Vector2(0f, -18f);
            hrt.sizeDelta = new Vector2(620f, 32f);

            TextMeshProUGUI tmpHeader = headerObj.AddComponent<TextMeshProUGUI>();
            tmpHeader.alignment = TextAlignmentOptions.Center;
            tmpHeader.fontStyle = FontStyles.Bold;
            tmpHeader.raycastTarget = false;

            int earnedStars = PhysicsGame1Session.CalculateStars(Mathf.RoundToInt(score));
            if (earnedStars >= 3)
            {
                tmpHeader.text = "<size=23><color=#E65100><b>XUẤT SẮC! BẠN LÀ THIÊN TÀI VẬT LÝ</b></color></size>";
            }
            else if (earnedStars >= 2)
            {
                tmpHeader.text = "<size=23><color=#1565C0><b>GIỎI LẮM! ĐẠT CHUẨN KIẾN THỨC BÀI HỌC</b></color></size>";
            }
            else if (earnedStars >= 1)
            {
                tmpHeader.text = "<size=23><color=#2E7D32><b>HOÀN THÀNH TỐT! TIẾP TỤC PHÁT HUY</b></color></size>";
            }
            else
            {
                tmpHeader.text = "<size=23><color=#C62828><b>CẦN CỐ GẮNG HƠN! ĐỪNG NẢN LÒNG NHÉ</b></color></size>";
            }

            // 1.5. Cụm Huy Hiệu 3 Sao (⭐⭐⭐) Lộng Lẫy
            GameObject starsObj = new GameObject("StarsRatingContainer");
            starsObj.transform.SetParent(frameObj.transform, false);
            RectTransform srtStars = starsObj.AddComponent<RectTransform>();
            srtStars.anchorMin = new Vector2(0.5f, 0.5f);
            srtStars.anchorMax = new Vector2(0.5f, 0.5f);
            srtStars.pivot = new Vector2(0.5f, 0.5f);
            srtStars.anchoredPosition = new Vector2(0f, -46f);
            srtStars.sizeDelta = new Vector2(300f, 34f);

            TextMeshProUGUI tmpStars = starsObj.AddComponent<TextMeshProUGUI>();
            VietnameseFontInitializer.ApplyTo(tmpStars);
            tmpStars.alignment = TextAlignmentOptions.Center;
            tmpStars.fontSize = 20f;
            tmpStars.fontStyle = FontStyles.Bold;
            tmpStars.raycastTarget = false;

            string ratingBadge = earnedStars switch
            {
                3 => "<color=#F59E0B><b>DANH HIỆU: THỢ SĂN XUẤT SẮC (3/3 SAO)</b></color>",
                2 => "<color=#F59E0B><b>DANH HIỆU: THỢ SĂN THIỆN XẠ (2/3 SAO)</b></color>",
                1 => "<color=#F59E0B><b>DANH HIỆU: TẬP SỰ CHĂM CHỈ (1/3 SAO)</b></color>",
                _ => "<color=#9CA3AF><b>HÃY RÈN LUYỆN THÊM ĐỂ ĐẠT SAO NHÉ!</b></color>"
            };
            tmpStars.text = ratingBadge;

            // 2. Số Điểm IN TO KHỔNG LỒ (Fit hoàn hảo vào trung tâm khung, không giới hạn / 100)
            GameObject scoreObj = GameObject.Find("ghidiem");
            if (scoreObj != null)
            {
                _tmpScore = scoreObj.GetComponent<TextMeshProUGUI>() ?? scoreObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_tmpScore != null)
                {
                    _scoreRt = _tmpScore.rectTransform;
                    _scoreRt.anchorMin = new Vector2(0.5f, 0.5f);
                    _scoreRt.anchorMax = new Vector2(0.5f, 0.5f);
                    _scoreRt.pivot = new Vector2(0.5f, 0.5f);
                    _scoreRt.anchoredPosition = new Vector2(0f, -88f);
                    _scoreRt.sizeDelta = new Vector2(680f, 85f);

                    _tmpScore.alignment = TextAlignmentOptions.Center;
                    _tmpScore.fontStyle = FontStyles.Bold;
                    _tmpScore.textWrappingMode = TextWrappingModes.NoWrap;
                    _tmpScore.enableAutoSizing = false;
                    _tmpScore.raycastTarget = false;

                    // Màu sắc số điểm tương ứng mức độ thành tích
                    Color scoreColor = (score >= 80) ? new Color(0.92f, 0.44f, 0.05f) :
                                       (score >= 50) ? new Color(0.15f, 0.65f, 0.25f) :
                                                       new Color(0.85f, 0.20f, 0.15f);
                    _tmpScore.color = scoreColor;

                    // Chạy hiệu ứng cuộn tăng số điểm sinh động từ 0 đến targetScore
                    StartCoroutine(AnimateRollUpScore(score, isHappy));
                }
            }

            // 3. Dòng Huy Hiệu Thống Kê (Số câu đúng + Thời gian)
            GameObject statsRowObj = new GameObject("StatsRowContainer");
            statsRowObj.transform.SetParent(frameObj.transform, false);
            RectTransform srt = statsRowObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, -148f);
            srt.sizeDelta = new Vector2(600f, 36f);

            // Viên nang thống kê câu đúng (Bên trái, hiển thị tổng số câu thực tế: X/5 hoặc X/10)
            CreateStatPill(statsRowObj.transform, new Vector2(-155f, 0f), $"ĐÚNG: {correctCount}/{totalQ} CÂU", new Color(0.12f, 0.50f, 0.20f));

            // Viên nang thống kê thời gian (Bên phải)
            CreateStatPill(statsRowObj.transform, new Vector2(155f, 0f), $"THỜI GIAN: {timeStr}", new Color(0.08f, 0.45f, 0.75f));

            // 4. Lời khuyên & Động viên sư phạm (Vừa vặn trong đáy lòng bảng, không dính viền cong)
            GameObject adviceObj = new GameObject("AdviceText");
            adviceObj.transform.SetParent(frameObj.transform, false);
            RectTransform art = adviceObj.AddComponent<RectTransform>();
            art.anchorMin = new Vector2(0.5f, 0.5f);
            art.anchorMax = new Vector2(0.5f, 0.5f);
            art.pivot = new Vector2(0.5f, 0.5f);
            art.anchoredPosition = new Vector2(0f, -196f);
            art.sizeDelta = new Vector2(680f, 32f);

            TextMeshProUGUI tmpAdvice = adviceObj.AddComponent<TextMeshProUGUI>();
            tmpAdvice.alignment = TextAlignmentOptions.Center;
            tmpAdvice.fontStyle = FontStyles.Italic | FontStyles.Bold;
            tmpAdvice.fontSize = 17f;
            tmpAdvice.color = new Color(0.35f, 0.20f, 0.10f);
            tmpAdvice.raycastTarget = false;

            if (score >= 80)
            {
                tmpAdvice.text = "Bạn nắm rất chắc lý thuyết & công thức! Hãy giữ vững phong độ nhé!";
            }
            else if (score >= 50)
            {
                tmpAdvice.text = "Nhấn nút 'Xem lời giải chi tiết' bên dưới để ôn tập các câu làm sai nha!";
            }
            else
            {
                tmpAdvice.text = "Hãy nhấn 'Xem lời giải chi tiết' để xem lại công thức và thử lại nào!";
            }

            // 5. Trạng thái Đồng Bộ Google Sheets (TienDoHocTap & LichSuLamBai)
            GameObject syncObj = new GameObject("CloudSyncStatus");
            syncObj.transform.SetParent(frameObj.transform, false);
            RectTransform syncRt = syncObj.AddComponent<RectTransform>();
            syncRt.anchorMin = new Vector2(0.5f, 0.5f);
            syncRt.anchorMax = new Vector2(0.5f, 0.5f);
            syncRt.pivot = new Vector2(0.5f, 0.5f);
            syncRt.anchoredPosition = new Vector2(0f, -225f);
            syncRt.sizeDelta = new Vector2(650f, 24f);

            TextMeshProUGUI tmpSync = syncObj.AddComponent<TextMeshProUGUI>();
            tmpSync.alignment = TextAlignmentOptions.Center;
            tmpSync.fontSize = 13.5f;
            tmpSync.fontStyle = FontStyles.Italic;
            tmpSync.color = new Color(0.2f, 0.45f, 0.65f, 0.95f);
            tmpSync.text = "☁️ Đang đồng bộ kết quả lên hệ thống Google Sheets...";
            tmpSync.raycastTarget = false;

            // Kích hoạt đồng bộ điểm số và sao
            PhysicsGame1Session.Instance.SyncCurrentResultToGoogleSheets((success, msg) =>
            {
                if (tmpSync != null)
                {
                    if (success)
                    {
                        tmpSync.color = new Color(0.12f, 0.55f, 0.22f, 1f);
                        tmpSync.text = "✅ Đã đồng bộ kết quả học tập & sao tích lũy lên hệ thống!";
                    }
                    else
                    {
                        tmpSync.color = new Color(0.40f, 0.40f, 0.40f, 0.90f);
                        tmpSync.text = "💾 Đã lưu cục bộ an toàn trong máy.";
                    }
                }
            });
        }

        private void CreateStatPill(Transform parent, Vector2 pos, string text, Color textColor)
        {
            GameObject pillObj = new GameObject("Pill");
            pillObj.transform.SetParent(parent, false);
            RectTransform prt = pillObj.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = pos;
            prt.sizeDelta = new Vector2(265f, 36f);

            Image bg = pillObj.AddComponent<Image>();
            bg.color = new Color(0.98f, 0.96f, 0.92f, 0.95f);
            bg.raycastTarget = false;

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(pillObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 19f;
            tmp.color = textColor;
            tmp.text = text;
            tmp.raycastTarget = false;
        }

        private IEnumerator AnimateRollUpScore(float targetScore, bool isHappy)
        {
            float duration = 1.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                float currentDisplay = Mathf.Lerp(0f, targetScore, easeOut);

                if (_tmpScore != null)
                {
                    _tmpScore.text = $"<size=92><b>{currentDisplay:0.#}</b></size> <size=38><color=#795548>ĐIỂM</color></size>";
                }
                yield return null;
            }

            if (_tmpScore != null)
            {
                _tmpScore.text = $"<size=96><b>{targetScore:0.#}</b></size> <size=40><color=#795548>ĐIỂM</color></size>";

                // Nảy bật điểm số khi đạt điểm số thực tế
                if (_scoreRt != null)
                {
                    float pDur = 0.35f;
                    float pElapsed = 0f;
                    while (pElapsed < pDur)
                    {
                        pElapsed += Time.deltaTime;
                        float pt = Mathf.Clamp01(pElapsed / pDur);
                        float s = 1f + Mathf.Sin(pt * Mathf.PI) * 0.28f;
                        _scoreRt.localScale = Vector3.one * s;
                        yield return null;
                    }
                    _scoreRt.localScale = Vector3.one;
                }
            }
        }

        private IEnumerator AnimateFrameEntrance(RectTransform rt)
        {
            if (rt == null) yield break;
            float dur = 0.45f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                // Hiệu ứng nảy lò xo nhẹ
                float s = (t < 0.7f) ? Mathf.Lerp(0.5f, 1.05f, t / 0.7f) : Mathf.Lerp(1.05f, 1.0f, (t - 0.7f) / 0.3f);
                rt.localScale = Vector3.one * s;
                yield return null;
            }
            rt.localScale = Vector3.one;
        }
        #endregion

        #region ===== B. HIỆU ỨNG NHÂN VẬT & NƯỚC MẮT PROCEDURAL (CHARACTER DYNAMICS) =====
        private IEnumerator CharacterIdleBreathingLoop(RectTransform charRt, bool isHappy)
        {
            Vector2 basePos = charRt.anchoredPosition;

            while (charRt != null && charRt.gameObject.activeInHierarchy)
            {
                if (isHappy)
                {
                    // Vui: Nhịp thở bồng bềnh vui tươi
                    float bob = Mathf.Sin(Time.time * 2.8f) * 6f;
                    float breathe = 1f + Mathf.Sin(Time.time * 2.8f) * 0.015f;
                    charRt.anchoredPosition = new Vector2(basePos.x, basePos.y + bob);
                    charRt.localScale = new Vector3(breathe, breathe, 1f);
                }
                else
                {
                    // Buồn / Khóc: Nhịp thở buồn bã mượt mà êm ái, hoàn toàn KHÔNG rung lắc giật cục
                    float bob = Mathf.Sin(Time.time * 1.8f) * 3f;
                    charRt.anchoredPosition = new Vector2(basePos.x, basePos.y + bob);
                    charRt.localScale = Vector3.one;
                }

                yield return null;
            }
        }

        private IEnumerator AnimateCharacterHop(RectTransform charRt)
        {
            if (charRt == null) yield break;
            float dur = 0.25f;
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f;
                charRt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            charRt.localScale = Vector3.one;
        }

        #region --- PROCEDURAL ANIME TEARS (KHÔNG CẦN ASSET BÊN NGOÀI - CHẢY MƯỢT MÀ, KHÔNG GIẬT) ---
        private static Sprite _proceduralTearSprite;
        private static Sprite GetOrCreateTearSprite()
        {
            if (_proceduralTearSprite != null) return _proceduralTearSprite;

            int w = 32;
            int h = 48;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2(w * 0.5f, h * 0.36f);
            float r = w * 0.42f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float alpha = 0f;

                    if (py <= center.y)
                    {
                        // Nửa dưới hình cầu giọt nước
                        float dist = Vector2.Distance(new Vector2(px, py), center);
                        if (dist <= r)
                        {
                            alpha = Mathf.SmoothStep(1f, 0.3f, dist / r);
                        }
                    }
                    else
                    {
                        // Nửa trên thuôn nhọn
                        float t = (py - center.y) / (h - center.y);
                        float currentRadius = Mathf.Lerp(r, 1f, t);
                        float dx = Mathf.Abs(px - center.x);
                        if (dx <= currentRadius)
                        {
                            float edgeAlpha = 1f - (dx / currentRadius);
                            float topFade = 1f - Mathf.Pow(t, 2f);
                            alpha = Mathf.Clamp01(edgeAlpha * topFade);
                        }
                    }

                    if (alpha > 0.01f)
                    {
                        float sheen = (1f - (Mathf.Abs(px - center.x) / (w * 0.5f))) * 0.4f;
                        Color c = Color.Lerp(new Color(0.35f, 0.82f, 1f, alpha * 0.9f),
                                             new Color(0.88f, 0.96f, 1f, alpha * 0.95f),
                                             sheen);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            _proceduralTearSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            return _proceduralTearSprite;
        }

        private void AttachProceduralCryingEffect(RectTransform charRt)
        {
            if (charRt == null) return;

            GameObject tearRoot = new GameObject("ProceduralCryingEffect");
            tearRoot.transform.SetParent(charRt, false);
            RectTransform rootRt = tearRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.anchoredPosition = Vector2.zero;
            rootRt.sizeDelta = charRt.sizeDelta;

            Sprite tearSprite = GetOrCreateTearSprite();

            float charW = charRt.rect.width > 0 ? charRt.rect.width : 300f;
            float charH = charRt.rect.height > 0 ? charRt.rect.height : 520f;

            float eyeY = charH * 0.15f;
            float eyeSpacing = charW * 0.11f;

            // 1. Dòng suối lệ 2 bên gò má (êm dịu, không giật)
            CreateTearStream(tearRoot.transform, new Vector2(-eyeSpacing, eyeY - 20f), tearSprite);
            CreateTearStream(tearRoot.transform, new Vector2(eyeSpacing, eyeY - 20f), tearSprite);

            // 2. Các giọt lệ chảy trôi mượt mà 60fps (không lag, không rung lắc, không tạo/hủy GameObject)
            StartCoroutine(AnimateSmoothFlowingTears(tearRoot.transform, -eyeSpacing, eyeSpacing, eyeY, tearSprite));
        }

        private void CreateTearStream(Transform parent, Vector2 pos, Sprite sprite)
        {
            GameObject streamObj = new GameObject("TearStream");
            streamObj.transform.SetParent(parent, false);
            RectTransform srt = streamObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = pos;
            srt.sizeDelta = new Vector2(14f, 48f);

            Image img = streamObj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = new Color(0.40f, 0.85f, 1f, 0.80f);
            img.raycastTarget = false;

            StartCoroutine(AnimateTearStreamPulse(srt, img));
        }

        private IEnumerator AnimateTearStreamPulse(RectTransform srt, Image img)
        {
            while (srt != null && srt.gameObject.activeInHierarchy)
            {
                if (img != null)
                {
                    // Ánh nước long lanh êm dịu, giữ cố định kích thước tránh cảm giác giật
                    float a = 0.72f + Mathf.Sin(Time.time * 2.2f) * 0.14f;
                    img.color = new Color(0.40f, 0.85f, 1f, a);
                }
                yield return null;
            }
        }

        private IEnumerator AnimateSmoothFlowingTears(Transform parent, float leftX, float rightX, float eyeY, Sprite sprite)
        {
            // Tạo sẵn các giọt nước mắt (2 giọt mỗi bên) để tránh cấp phát GC / Instantiate lúc chạy
            int count = 2;
            RectTransform[] leftDrops = new RectTransform[count];
            Image[] leftImgs = new Image[count];
            RectTransform[] rightDrops = new RectTransform[count];
            Image[] rightImgs = new Image[count];

            for (int i = 0; i < count; i++)
            {
                leftDrops[i] = CreateTearDropObj(parent, sprite, out leftImgs[i]);
                rightDrops[i] = CreateTearDropObj(parent, sprite, out rightImgs[i]);
            }

            float startY = eyeY - 20f;
            float endY = eyeY - 80f;
            float flowSpeed = 0.65f; // Chu kỳ trôi êm ái khoảng 1.5s/lần

            while (parent != null && parent.gameObject.activeInHierarchy)
            {
                for (int i = 0; i < count; i++)
                {
                    // Độ lệch pha 0.5 để 2 giọt nối tiếp nhau trôi đều đặn
                    float phase = (Time.time * flowSpeed + i * 0.5f) % 1f;

                    // Chuyển động Y trôi đều đặn thẳng đứng (không rung lắc X)
                    float curY = Mathf.Lerp(startY, endY, phase);

                    // Alpha cong hình sin: xuất hiện mờ, sáng rõ ở gò má, mờ dần tan ở cằm
                    float alpha = Mathf.Sin(phase * Mathf.PI);

                    // Giọt lệ to dần rất nhẹ từ 0.75 đến 1.05
                    float scale = Mathf.Lerp(0.75f, 1.05f, phase);

                    if (leftDrops[i] != null)
                    {
                        leftDrops[i].anchoredPosition = new Vector2(leftX, curY);
                        leftDrops[i].localScale = new Vector3(scale, scale, 1f);
                        if (leftImgs[i] != null)
                            leftImgs[i].color = new Color(0.45f, 0.88f, 1f, alpha * 0.90f);
                    }

                    if (rightDrops[i] != null)
                    {
                        rightDrops[i].anchoredPosition = new Vector2(rightX, curY);
                        rightDrops[i].localScale = new Vector3(scale, scale, 1f);
                        if (rightImgs[i] != null)
                            rightImgs[i].color = new Color(0.45f, 0.88f, 1f, alpha * 0.90f);
                    }
                }

                yield return null;
            }
        }

        private RectTransform CreateTearDropObj(Transform parent, Sprite sprite, out Image img)
        {
            GameObject dropObj = new GameObject("SmoothTearDrop");
            dropObj.transform.SetParent(parent, false);
            RectTransform drt = dropObj.AddComponent<RectTransform>();
            drt.anchorMin = new Vector2(0.5f, 0.5f);
            drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.pivot = new Vector2(0.5f, 0.5f);
            drt.sizeDelta = new Vector2(15f, 22f);
            drt.localEulerAngles = new Vector3(0f, 0f, 180f); // Đỉnh nhọn giọt nước hướng lên

            img = dropObj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = new Color(0.45f, 0.88f, 1f, 0f);
            img.raycastTarget = false;
            return drt;
        }
        #endregion
        #endregion

        #region ===== C. CÁC NÚT ĐIỀU HƯỚNG & HIỆU ỨNG NHỊP THỞ (BUTTON POLISH) =====
        private void SetupNavigationButtons()
        {
            // Nút Chơi lại: Tạo lượt mới, load 07_DuckShooting (bỏ nhịp thở lặp để giống nút xem thành tích)
            GameObject btnChoiLai = GameObject.Find("choilai");
            if (btnChoiLai != null)
            {
                RectTransform brt = btnChoiLai.GetComponent<RectTransform>();

                UIHelper.GetOrAddButton(btnChoiLai, () =>
                {
                    StartCoroutine(AnimateButtonPunch(brt, () =>
                    {
                        PhysicsGame1Session.Instance.StartNewGameSession();
                        SceneManager.LoadScene("07_DuckShooting");
                    }));
                });
            }

            // Nút Xem lời giải chi tiết: Mở 09_AnswerReview (bỏ nhịp thở lặp để giống nút xem thành tích)
            GameObject btnXemDetail = GameObject.Find("xemdetail");
            if (btnXemDetail != null)
            {
                RectTransform drt = btnXemDetail.GetComponent<RectTransform>();

                UIHelper.GetOrAddButton(btnXemDetail, () =>
                {
                    StartCoroutine(AnimateButtonPunch(drt, () =>
                    {
                        SceneManager.LoadScene("09_AnswerReview");
                    }));
                });
            }

            // Nút Xem thành tích: Mở 11_Achievements
            GameObject btnThanhTich = GameObject.Find("xemthanhtich");
            if (btnThanhTich != null)
            {
                RectTransform trt = btnThanhTich.GetComponent<RectTransform>();
                UIHelper.GetOrAddButton(btnThanhTich, () =>
                {
                    StartCoroutine(AnimateButtonPunch(trt, () =>
                    {
                        PhysicsGame1Session.Instance.PreviousSceneName = "08_ResultScene";
                        SceneManager.LoadScene("11_Achievements");
                    }));
                });
            }

            // Nút Ôn tập Lý Thuyết (nếu có trên Scene)
            GameObject btnLyThuyet = GameObject.Find("xemlythuyet") ?? GameObject.Find("lythuyet");
            if (btnLyThuyet != null)
            {
                RectTransform lrt = btnLyThuyet.GetComponent<RectTransform>();
                UIHelper.GetOrAddButton(btnLyThuyet, () =>
                {
                    StartCoroutine(AnimateButtonPunch(lrt, () =>
                    {
                        PhysicsGame1Session.Instance.PreviousSceneName = "08_ResultScene";
                        SceneManager.LoadScene("12_Theory");
                    }));
                });
            }

            // Nút Quay lại: Về 01_MainMenu
            GameObject btnQuayLai = GameObject.Find("quaylai");
            if (btnQuayLai != null)
            {
                RectTransform qrt = btnQuayLai.GetComponent<RectTransform>();
                UIHelper.GetOrAddButton(btnQuayLai, () =>
                {
                    StartCoroutine(AnimateButtonPunch(qrt, () =>
                    {
                        SceneManager.LoadScene("01_MainMenu");
                    }));
                });
            }
        }

        private IEnumerator ButtonBreathingLoop(RectTransform rt)
        {
            while (rt != null && rt.gameObject.activeInHierarchy)
            {
                float s = 1f + Mathf.Sin(Time.time * 3.2f) * 0.035f;
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        private IEnumerator AnimateButtonPunch(RectTransform rt, Action onClick)
        {
            if (rt != null)
            {
                float dur = 0.14f;
                float elapsed = 0f;
                while (elapsed < dur)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / dur);
                    float s = (t < 0.5f) ? Mathf.Lerp(1f, 0.92f, t / 0.5f) : Mathf.Lerp(0.92f, 1.05f, (t - 0.5f) / 0.5f);
                    rt.localScale = Vector3.one * s;
                    yield return null;
                }
                rt.localScale = Vector3.one;
            }
            onClick?.Invoke();
        }
        #endregion
    }
    #endregion

    #region ===== 09_AnswerReview Controller =====
    public class PhysicsGame1_AnswerReviewController : MonoBehaviour
    {
        private int _currentIndex = 0;
        private List<QuestionReviewItem> _results;

        // Trắc nghiệm UI Elements
        private TextMeshProUGUI _tmpCauso;
        private TextMeshProUGUI _tmpDebai;
        private TextMeshProUGUI _tmpDapanA;
        private TextMeshProUGUI _tmpDapanB;
        private TextMeshProUGUI _tmpDapanC;
        private TextMeshProUGUI _tmpDapanD;
        private TextMeshProUGUI _tmpLoigiai;

        private GameObject _objDapanA;
        private GameObject _objDapanB;
        private GameObject _objDapanC;
        private GameObject _objDapanD;

        private Button _btnPrev;
        private Button _btnNext;

        // Tự luận Panel UI Elements (Panelgiaichitiettuluan)
        private GameObject _panelEssayReview;
        private TextMeshProUGUI _tmpEssayCauso;
        private TextMeshProUGUI _tmpEssayDebai;
        private TextMeshProUGUI _tmpEssayNhanXet;
        private TextMeshProUGUI _tmpEssayGoiY;
        private TextMeshProUGUI _tmpEssayBuocGiai;
        private TextMeshProUGUI _tmpEssayDiemAI;
        private Button _btnEssayPrev;
        private Button _btnEssayNext;
        private Button _btnEssayBack;

        private bool _isEssayMode = false;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 📝 Khởi tạo 09_AnswerReview (Xem lời giải chi tiết)");
            PhysicsGame1Bootstrap.PurgeAllStrayUI();

            _results = PhysicsGame1Session.Instance.QuestionResults;
            if (_results == null || _results.Count == 0)
            {
                Debug.LogWarning("[PhysicsGame1] ⚠️ Chưa có dữ liệu câu hỏi lượt vừa rồi, dùng dữ liệu mẫu");
                _results = new List<QuestionReviewItem>();

                bool isEssay = (PhysicsGame1Session.Instance != null && PhysicsGame1Session.Instance.IsEssayMode);
                int sampleCount = isEssay ? 5 : 10;

                for (int i = 0; i < sampleCount; i++)
                {
                    if (isEssay)
                    {
                        _results.Add(new QuestionReviewItem
                        {
                            question = new QuestionData
                            {
                                questionID = $"TL_SAMPLE_{i + 1}",
                                questionText = $"Đề bài tự luận Vật lý #{i + 1}: Nêu khái niệm và phân tích các hiện tượng vật lý liên quan.",
                                answers = new string[0],
                                correctAnswerIndex = -1,
                                correctAnswer = "Lập luận đúng theo định luật vật lý, nêu công thức và giải thích đơn vị chuẩn.",
                                explanation = "Lời giải chi tiết: Vận dụng định luật bảo toàn năng lượng và công thức tính công cơ học."
                            },
                            essayAnswerText = $"Bài giải mẫu của học sinh cho câu #{i + 1}: Áp dụng công thức và tính toán các đại lượng...",
                            isCorrect = (i % 2 == 0),
                            earnedScore = (i % 2 == 0) ? 9 : 4,
                            studentDisplayAnswer = $"Bài giải mẫu của học sinh cho câu #{i + 1}...",
                            correctDisplayAnswer = "Lập luận đúng theo định luật vật lý, nêu công thức và giải thích đơn vị chuẩn.",
                            explanation = "Lời giải chi tiết: Vận dụng định luật bảo toàn năng lượng và công thức tính công cơ học.",
                            aiFeedback = (i % 2 == 0) ? "Lập luận chặt chẽ, đạt điểm tối đa các tiêu chí." : "Cần bổ sung đơn vị và giải thích chi tiết hơn các bước biến đổi.",
                            aiSuggestions = "Ôn tập lại phần công thức và chú ý đổi đơn vị SI trước khi tính toán."
                        });
                    }
                    else
                    {
                        _results.Add(new QuestionReviewItem
                        {
                            question = new QuestionData
                            {
                                questionID = $"TN_SAMPLE_{i + 1}",
                                questionText = $"Câu hỏi ôn tập kiến thức Vật Lý #{i + 1}",
                                answers = new string[] { "A. Phương án 1", "B. Phương án 2", "C. Phương án 3", "D. Phương án 4" },
                                correctAnswerIndex = 0,
                                explanation = "Lời giải chi tiết: Áp dụng kiến thức định lý và công thức Vật lý chuẩn THCS."
                            },
                            selectedAnswerIndex = (i % 2 == 0) ? 0 : 1,
                            isCorrect = (i % 2 == 0),
                            studentDisplayAnswer = (i % 2 == 0) ? "A. Phương án 1" : "B. Phương án 2",
                            correctDisplayAnswer = "A. Phương án 1",
                            explanation = "Lời giải chi tiết: Áp dụng kiến thức định lý và công thức Vật lý chuẩn THCS."
                        });
                    }
                }
            }

            FindUIReferences();

            // Nút Câu trước & Câu tiếp (Trắc nghiệm)
            GameObject prevObj = GameObject.Find("cautruoc");
            _btnPrev = UIHelper.GetOrAddButton(prevObj, () =>
            {
                if (_currentIndex > 0)
                {
                    _currentIndex--;
                    DisplayQuestion(_currentIndex);
                }
            });

            GameObject nextObj = GameObject.Find("cautiep");
            _btnNext = UIHelper.GetOrAddButton(nextObj, () =>
            {
                if (_currentIndex < _results.Count - 1)
                {
                    _currentIndex++;
                    DisplayQuestion(_currentIndex);
                }
            });

            // Nút Quay lại: Trở về ResultScene
            GameObject backObj = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(backObj, () =>
            {
                SceneManager.LoadScene("08_ResultScene");
            });

            _currentIndex = 0;
            DisplayQuestion(0);
        }

        private void FindUIReferences()
        {
            // Xác định xem lượt thi vừa rồi là Tự luận hay Trắc nghiệm
            _isEssayMode = false;
            if (PhysicsGame1Session.Instance != null)
            {
                if (PhysicsGame1Session.Instance.SelectedGameMode == "TuLuan" || 
                    PhysicsGame1Session.Instance.SelectedGameMode == "Essay")
                {
                    _isEssayMode = true;
                }
            }
            if (!_isEssayMode && _results != null && _results.Count > 0)
            {
                _isEssayMode = _results.Any(r => (r.question != null && (r.question.loaiCauHoi == "Tự luận" || (r.question.answers == null || r.question.answers.Length == 0))) || !string.IsNullOrEmpty(r.essayAnswerText));
            }

            // Tìm Panelgiaichitiettuluan trong Scene
            var allObjs = Resources.FindObjectsOfTypeAll<GameObject>();
            _panelEssayReview = allObjs.FirstOrDefault(g => g.name == "Panelgiaichitiettuluan");

            if (_panelEssayReview != null)
            {
                _panelEssayReview.SetActive(_isEssayMode);

                Transform pet = _panelEssayReview.transform;
                _tmpEssayCauso = pet.Find("question (1)/ghicauso")?.GetComponent<TextMeshProUGUI>() ?? pet.Find("ghicauso")?.GetComponent<TextMeshProUGUI>();
                _tmpEssayDebai = pet.Find("debai (1)/ghidebai")?.GetComponent<TextMeshProUGUI>() ?? pet.Find("debai (1)")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayNhanXet = pet.Find("nhanxet")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayGoiY = pet.Find("goiycaithien")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayBuocGiai = pet.Find("cacbuocgiai/ghiloigiai")?.GetComponent<TextMeshProUGUI>() ?? pet.Find("cacbuocgiai")?.GetComponentInChildren<TextMeshProUGUI>(true);
                _tmpEssayDiemAI = pet.Find("điểm AI chấm")?.GetComponentInChildren<TextMeshProUGUI>(true) ?? pet.Find("diem")?.GetComponentInChildren<TextMeshProUGUI>(true);

                // Cấu hình các nút điều hướng Tự luận
                GameObject btnPrevEssay = pet.Find("cautruoc (1)")?.gameObject;
                _btnEssayPrev = UIHelper.GetOrAddButton(btnPrevEssay, () =>
                {
                    if (_currentIndex > 0)
                    {
                        _currentIndex--;
                        DisplayQuestion(_currentIndex);
                    }
                });

                GameObject btnNextEssay = pet.Find("cautiep (1)")?.gameObject;
                _btnEssayNext = UIHelper.GetOrAddButton(btnNextEssay, () =>
                {
                    if (_currentIndex < _results.Count - 1)
                    {
                        _currentIndex++;
                        DisplayQuestion(_currentIndex);
                    }
                });

                GameObject btnBackEssay = pet.Find("quaylai (1)")?.gameObject;
                _btnEssayBack = UIHelper.GetOrAddButton(btnBackEssay, () =>
                {
                    SceneManager.LoadScene("08_ResultScene");
                });
            }

            // Ẩn/Hiện nhóm Trắc nghiệm tương ứng
            GameObject mcQuestion = GameObject.Find("question");
            GameObject mcDebai = GameObject.Find("debai");
            GameObject mcLoigiai = GameObject.Find("loigiai");
            GameObject mcCautruoc = GameObject.Find("cautruoc");
            GameObject mcCautiep = GameObject.Find("cautiep");
            GameObject mcQuaylai = GameObject.Find("quaylai");

            if (mcQuestion != null) mcQuestion.SetActive(!_isEssayMode);
            if (mcDebai != null) mcDebai.SetActive(!_isEssayMode);
            if (mcLoigiai != null) mcLoigiai.SetActive(!_isEssayMode);
            if (mcCautruoc != null) mcCautruoc.SetActive(!_isEssayMode);
            if (mcCautiep != null) mcCautiep.SetActive(!_isEssayMode);
            if (mcQuaylai != null) mcQuaylai.SetActive(!_isEssayMode);

            GameObject causoObj = GameObject.Find("ghicauso");
            if (causoObj != null)
            {
                _tmpCauso = causoObj.GetComponent<TextMeshProUGUI>() ?? causoObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_tmpCauso != null)
                {
                    _tmpCauso.raycastTarget = false;
                    _tmpCauso.textWrappingMode = TextWrappingModes.NoWrap;
                    _tmpCauso.alignment = TextAlignmentOptions.Center;
                    _tmpCauso.fontStyle = FontStyles.Bold;
                    _tmpCauso.fontSize = 26f;
                    _tmpCauso.color = new Color(0.22f, 0.12f, 0.05f);
                    RectTransform crt = _tmpCauso.rectTransform;
                    if (crt != null)
                    {
                        crt.anchorMin = new Vector2(0.5f, 0.5f);
                        crt.anchorMax = new Vector2(0.5f, 0.5f);
                        crt.pivot = new Vector2(0.5f, 0.5f);
                        crt.anchoredPosition = new Vector2(55f, -8f);
                        crt.sizeDelta = new Vector2(90f, 44f);
                    }
                }
            }

            GameObject debaiObj = GameObject.Find("debai") ?? GameObject.Find("ghidebai");
            if (debaiObj != null)
            {
                _tmpDebai = debaiObj.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDebai, 60f, 60f, 70f, 35f, 14f, 22f, TextAlignmentOptions.Center, new Color(0.15f, 0.10f, 0.05f), true);
            }

            _objDapanA = GameObject.Find("dapana");
            _objDapanB = GameObject.Find("dapanb");
            _objDapanC = GameObject.Find("dapanc");
            _objDapanD = GameObject.Find("dapand");

            if (_objDapanA != null)
            {
                _tmpDapanA = _objDapanA.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanA, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            if (_objDapanB != null)
            {
                _tmpDapanB = _objDapanB.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanB, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            if (_objDapanC != null)
            {
                _tmpDapanC = _objDapanC.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanC, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            if (_objDapanD != null)
            {
                _tmpDapanD = _objDapanD.GetComponentInChildren<TextMeshProUGUI>(true);
                UIHelper.FitTextInContainer(_tmpDapanD, 142f, 34f, 28f, 22f, 10f, 19f, TextAlignmentOptions.MidlineLeft, new Color(0.15f, 0.10f, 0.05f), true);
            }

            GameObject lgObj = GameObject.Find("loigiai") ?? GameObject.Find("ghiloigiai");
            if (lgObj != null)
            {
                _tmpLoigiai = lgObj.GetComponentInChildren<TextMeshProUGUI>(true);
                // Đệm lề trên 220f để dòng chữ nằm hoàn toàn bên dưới tiêu đề bảng gỗ "LỜI GIẢI"
                UIHelper.FitTextInContainer(_tmpLoigiai, 95f, 95f, 260f, 65f, 12f, 18f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.08f, 0.04f));
            }
        }

        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= _results.Count) return;
            QuestionReviewItem item = _results[index];

            // Nếu là Tự Luận và có Panelgiaichitiettuluan
            if (_isEssayMode && _panelEssayReview != null)
            {
                if (_tmpEssayCauso != null)
                {
                    _tmpEssayCauso.text = $"CÂU {index + 1} / {_results.Count}";
                    _tmpEssayCauso.enableAutoSizing = true;
                }
                if (_tmpEssayDebai != null)
                {
                    _tmpEssayDebai.text = item.question != null ? item.question.questionText : "";
                    _tmpEssayDebai.enableAutoSizing = true;
                    _tmpEssayDebai.fontSizeMin = 13f;
                    _tmpEssayDebai.fontSizeMax = 20f;
                }
                if (_tmpEssayDiemAI != null)
                {
                    string linesInfo = (item.totalLines > 0) ? $" ({item.correctLines}/{item.totalLines} dòng đúng)" : "";
                    _tmpEssayDiemAI.text = $"{item.earnedScore:F1}/20 ĐIỂM{linesInfo}";
                    _tmpEssayDiemAI.color = item.isCorrect ? new Color(0.2f, 0.9f, 0.35f) : new Color(0.95f, 0.3f, 0.2f);
                }
                if (_tmpEssayNhanXet != null)
                {
                    string linesDetail = !string.IsNullOrEmpty(item.stepBreakdownText) ? $"\n\n[ĐÁNH GIÁ THEO DÒNG]:\n{item.stepBreakdownText}" : "";
                    string fb = !string.IsNullOrEmpty(item.aiFeedback) ? item.aiFeedback :
                        (item.isCorrect ? "Bài làm đúng trọng tâm, đạt yêu cầu tiêu chí chấm." : "Bài làm chưa đủ ý, cần bổ sung luận điểm và công thức.");
                    _tmpEssayNhanXet.text = fb + linesDetail;
                    _tmpEssayNhanXet.enableAutoSizing = true;
                    _tmpEssayNhanXet.fontSizeMin = 12f;
                    _tmpEssayNhanXet.fontSizeMax = 16f;
                }
                if (_tmpEssayGoiY != null)
                {
                    string sugg = !string.IsNullOrEmpty(item.aiSuggestions) ? item.aiSuggestions :
                        (!string.IsNullOrEmpty(item.question?.meoGhiNho) ? item.question.meoGhiNho : "Cần rèn luyện cách trình bày khoa học và đủ các bước giải.");
                    _tmpEssayGoiY.text = sugg;
                    _tmpEssayGoiY.enableAutoSizing = true;
                    _tmpEssayGoiY.fontSizeMin = 12f;
                    _tmpEssayGoiY.fontSizeMax = 16f;
                }
                if (_tmpEssayBuocGiai != null)
                {
                    string refAns = !string.IsNullOrEmpty(item.question?.dapAnThamChieu) ? item.question.dapAnThamChieu :
                        (!string.IsNullOrEmpty(item.explanation) ? item.explanation : "Tham khảo lời giải mẫu trong sách giáo khoa.");
                    _tmpEssayBuocGiai.text = refAns;
                    _tmpEssayBuocGiai.enableAutoSizing = true;
                    _tmpEssayBuocGiai.fontSizeMin = 12f;
                    _tmpEssayBuocGiai.fontSizeMax = 16f;
                }

                if (_btnEssayPrev != null) _btnEssayPrev.interactable = (index > 0);
                if (_btnEssayNext != null) _btnEssayNext.interactable = (index < _results.Count - 1);
                return;
            }

            if (_tmpCauso != null) _tmpCauso.text = $"{index + 1}/{_results.Count}";
            if (_tmpDebai != null) _tmpDebai.text = item.question != null ? item.question.questionText : "";

            bool isEssay = (item.question != null && (item.question.answers == null || item.question.answers.Length == 0))
                           || !string.IsNullOrEmpty(item.essayAnswerText);

            if (_objDapanA != null) _objDapanA.SetActive(!isEssay);
            if (_objDapanB != null) _objDapanB.SetActive(!isEssay);
            if (_objDapanC != null) _objDapanC.SetActive(!isEssay);
            if (_objDapanD != null) _objDapanD.SetActive(!isEssay);

            if (isEssay)
            {
                if (_tmpLoigiai != null)
                {
                    string linesInfo = (item.totalLines > 0) ? $" ({item.correctLines}/{item.totalLines} dòng đúng)" : "";
                    string status = item.isCorrect 
                        ? $"<color=#15803d><b>[ ĐẠT ]</b> AI ĐÁNH GIÁ: {item.earnedScore:0.#}/20 ĐIỂM{linesInfo}</color>" 
                        : $"<color=#b91c1c><b>[ CHƯA ĐẠT ]</b> AI ĐÁNH GIÁ: {item.earnedScore:0.#}/20 ĐIỂM{linesInfo}</color>";

                    string studentText = !string.IsNullOrEmpty(item.essayAnswerText) 
                        ? item.essayAnswerText 
                        : (!string.IsNullOrEmpty(item.studentDisplayAnswer) ? item.studentDisplayAnswer : "(Chưa có câu trả lời)");

                    string breakdown = !string.IsNullOrEmpty(item.stepBreakdownText) ? $"\n\n<b><color=#0f766e>Chi tiết đánh giá từng dòng:</color></b>\n{item.stepBreakdownText}" : "";
                    string fb = !string.IsNullOrEmpty(item.aiFeedback) ? item.aiFeedback : item.explanation;
                    string sugg = !string.IsNullOrEmpty(item.aiSuggestions) ? $"\n\n<b><color=#0284c7>Gợi ý cải thiện:</color></b>\n{item.aiSuggestions}" : "";

                    _tmpLoigiai.text = $"{status}\n\n<b><color=#7c2d12>Bài làm của học sinh:</color></b>\n\"{studentText}\"{breakdown}\n\n<b><color=#1d4ed8>Nhận xét chi tiết từ AI:</color></b>\n{fb}{sugg}";
                }
            }
            else
            {
                // Hiển thị 4 phương án (Lọc sạch tiền tố A. B. C. D. để không đè lên huy hiệu tròn có sẵn trên nút)
                string[] answers = item.question != null ? item.question.answers : null;
                if (_tmpDapanA != null && answers != null && answers.Length > 0) _tmpDapanA.text = UIHelper.CleanAnswerText(answers[0]);
                if (_tmpDapanB != null && answers != null && answers.Length > 1) _tmpDapanB.text = UIHelper.CleanAnswerText(answers[1]);
                if (_tmpDapanC != null && answers != null && answers.Length > 2) _tmpDapanC.text = UIHelper.CleanAnswerText(answers[2]);
                if (_tmpDapanD != null && answers != null && answers.Length > 3) _tmpDapanD.text = UIHelper.CleanAnswerText(answers[3]);

                // Đánh dấu màu sắc đáp án đúng và lựa chọn của học sinh
                GameObject[] optObjs = new GameObject[] { _objDapanA, _objDapanB, _objDapanC, _objDapanD };
                for (int i = 0; i < 4; i++)
                {
                    if (optObjs[i] != null)
                    {
                        Image img = optObjs[i].GetComponent<Image>();
                        if (img != null)
                        {
                            if (item.question != null && i == item.question.correctAnswerIndex)
                                img.color = new Color(0.3f, 0.9f, 0.4f); // Xanh lá: Đáp án đúng
                            else if (i == item.selectedAnswerIndex && !item.isCorrect)
                                img.color = new Color(1f, 0.35f, 0.35f); // Đỏ: Đáp án học sinh chọn sai
                            else
                                img.color = Color.white;
                        }
                    }
                }

                // Lời giải chi tiết được căn chỉnh hoàn hảo trong cuộn giấy bên dưới tiêu đề
                if (_tmpLoigiai != null)
                {
                    string status = item.isCorrect 
                        ? "<color=#15803d><b>[ ĐÚNG ]</b> BẠN ĐÃ TRẢ LỜI CHÍNH XÁC (+10 điểm)</color>" 
                        : "<color=#b91c1c><b>[ CHƯA ĐÚNG ]</b> BẠN ĐÃ TRẢ LỜI CHƯA CHÍNH XÁC (0 điểm)</color>";

                    string correctAnsText = "";
                    if (answers != null && item.question != null && item.question.correctAnswerIndex >= 0 && item.question.correctAnswerIndex < answers.Length)
                    {
                        char c = (char)('A' + item.question.correctAnswerIndex);
                        correctAnsText = $"<b><color=#1d4ed8>Đáp án đúng:</color></b> {c}. {UIHelper.CleanAnswerText(answers[item.question.correctAnswerIndex])}\n\n";
                    }

                    string expl = item.explanation;
                    if (string.IsNullOrEmpty(expl) && item.question != null) expl = item.question.explanation;
                    if (string.IsNullOrEmpty(expl)) expl = "Xem lại lý thuyết và công thức trọng tâm của bài học.";

                    _tmpLoigiai.text = $"{status}\n\n{correctAnsText}<b><color=#1d4ed8>Lời giải chi tiết:</color></b>\n{expl}";
                }
            }

            // Vô hiệu hóa nút prev ở câu đầu, next ở câu cuối
            if (_btnPrev != null) _btnPrev.interactable = (index > 0);
            if (_btnNext != null) _btnNext.interactable = (index < _results.Count - 1);
        }
    }
    #endregion

    #region ===== 10_Settings Controller =====
    public class PhysicsGame1_SettingsController : MonoBehaviour
    {
        private GameObject _gatnutBGM;
        private GameObject _nutxamBGM;
        private GameObject _gatnutSFX;
        private GameObject _nutxamSFX;
        private GameObject _toastObj;

        private bool _tempBGM;
        private bool _tempSFX;

        private const float KNOB_OFF_X = 237.14f;
        private const float KNOB_ON_X = 311.30f;
        private static readonly Color COLOR_ON = new Color(0.24f, 0.82f, 0.38f, 1f);   // Xanh lá ON
        private static readonly Color COLOR_OFF = new Color(0.82f, 0.82f, 0.82f, 1f); // Xám sáng OFF

        private Coroutine _bgmSlideCor;
        private Coroutine _sfxSlideCor;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] ⚙️ Khởi tạo 10_Settings");

            _tempBGM = PhysicsGame1Session.Instance.BGMEnabled;
            _tempSFX = PhysicsGame1Session.Instance.SFXEnabled;

            _gatnutBGM = GameObject.Find("gatnut");
            _nutxamBGM = GameObject.Find("nutxam");
            _gatnutSFX = GameObject.Find("gatnut (1)");
            _nutxamSFX = GameObject.Find("nutxam (1)");

            UnityEngine.Events.UnityAction toggleBGM = () =>
            {
                _tempBGM = !_tempBGM;
                if (_bgmSlideCor != null) StopCoroutine(_bgmSlideCor);
                _bgmSlideCor = StartCoroutine(AnimateToggle(_gatnutBGM, _nutxamBGM, _tempBGM));
            };
            if (_gatnutBGM != null) UIHelper.GetOrAddButton(_gatnutBGM, toggleBGM);
            if (_nutxamBGM != null) UIHelper.GetOrAddButton(_nutxamBGM, toggleBGM);
            GameObject bgmBtn = GameObject.Find("nhacnen");
            if (bgmBtn != null) UIHelper.GetOrAddButton(bgmBtn, toggleBGM);

            UnityEngine.Events.UnityAction toggleSFX = () =>
            {
                _tempSFX = !_tempSFX;
                if (_sfxSlideCor != null) StopCoroutine(_sfxSlideCor);
                _sfxSlideCor = StartCoroutine(AnimateToggle(_gatnutSFX, _nutxamSFX, _tempSFX));
            };
            if (_gatnutSFX != null) UIHelper.GetOrAddButton(_gatnutSFX, toggleSFX);
            if (_nutxamSFX != null) UIHelper.GetOrAddButton(_nutxamSFX, toggleSFX);
            GameObject sfxBtn = GameObject.Find("amthanh");
            if (sfxBtn != null) UIHelper.GetOrAddButton(sfxBtn, toggleSFX);

            // Nút Lưu cài đặt: Lưu cấu hình, hiển thị thông báo thành công
            GameObject saveBtn = GameObject.Find("luucaidat");
            UIHelper.GetOrAddButton(saveBtn, () =>
            {
                PhysicsGame1Session.Instance.BGMEnabled = _tempBGM;
                PhysicsGame1Session.Instance.SFXEnabled = _tempSFX;
                PlayerPrefs.SetInt("PhysicsGame1_BGM", _tempBGM ? 1 : 0);
                PlayerPrefs.SetInt("PhysicsGame1_SFX", _tempSFX ? 1 : 0);
                PlayerPrefs.Save();
                PhysicsGame1_AudioManager.Instance.ApplySettings();
                Debug.Log($"[PhysicsGame1] 💾 Đã lưu cài đặt: BGM={_tempBGM}, SFX={_tempSFX}");

                ShowSaveNotification();
            });

            // Nút Quay lại: Hủy các chỉnh sửa chưa lưu và trở về MainMenu
            GameObject backBtn = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(backBtn, () =>
            {
                _tempBGM = PhysicsGame1Session.Instance.BGMEnabled;
                _tempSFX = PhysicsGame1Session.Instance.SFXEnabled;
                SceneManager.LoadScene("01_MainMenu");
            });

            // Áp dụng trực tiếp trạng thái ban đầu
            ApplyToggleInstant(_gatnutBGM, _nutxamBGM, _tempBGM);
            ApplyToggleInstant(_gatnutSFX, _nutxamSFX, _tempSFX);
        }

        private void ApplyToggleInstant(GameObject knobObj, GameObject bgObj, bool isOn)
        {
            if (bgObj != null)
            {
                bgObj.SetActive(true);
                Image img = bgObj.GetComponent<Image>();
                if (img != null) img.color = isOn ? COLOR_ON : COLOR_OFF;
            }

            if (knobObj != null)
            {
                knobObj.SetActive(true);
                RectTransform rt = knobObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 pos = rt.anchoredPosition;
                    pos.x = isOn ? KNOB_ON_X : KNOB_OFF_X;
                    rt.anchoredPosition = pos;
                }
            }
        }

        private IEnumerator AnimateToggle(GameObject knobObj, GameObject bgObj, bool isOn)
        {
            if (bgObj != null) bgObj.SetActive(true);
            if (knobObj != null) knobObj.SetActive(true);

            Image img = bgObj != null ? bgObj.GetComponent<Image>() : null;
            RectTransform rt = knobObj != null ? knobObj.GetComponent<RectTransform>() : null;

            Color startCol = img != null ? img.color : (isOn ? COLOR_OFF : COLOR_ON);
            Color targetCol = isOn ? COLOR_ON : COLOR_OFF;

            float startX = rt != null ? rt.anchoredPosition.x : (isOn ? KNOB_OFF_X : KNOB_ON_X);
            float targetX = isOn ? KNOB_ON_X : KNOB_OFF_X;

            float duration = 0.16f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                if (img != null) img.color = Color.Lerp(startCol, targetCol, t);
                if (rt != null)
                {
                    Vector2 pos = rt.anchoredPosition;
                    pos.x = Mathf.Lerp(startX, targetX, t);
                    rt.anchoredPosition = pos;
                }

                yield return null;
            }

            if (img != null) img.color = targetCol;
            if (rt != null)
            {
                Vector2 pos = rt.anchoredPosition;
                pos.x = targetX;
                rt.anchoredPosition = pos;
            }
        }

        private void ShowSaveNotification()
        {
            if (_toastObj == null)
            {
                Canvas canvas = UIHelper.GetSceneCanvas();
                if (canvas != null)
                {
                    _toastObj = new GameObject("SaveNotificationToast");
                    _toastObj.transform.SetParent(canvas.transform, false);

                    RectTransform rt = _toastObj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, -230f);
                    rt.sizeDelta = new Vector2(500f, 60f);

                    Image bg = _toastObj.AddComponent<Image>();
                    bg.color = new Color(0.12f, 0.65f, 0.28f, 0.95f);
                    bg.raycastTarget = false;

                    GameObject textObj = new GameObject("ToastText");
                    textObj.transform.SetParent(_toastObj.transform, false);
                    RectTransform trt = textObj.AddComponent<RectTransform>();
                    trt.anchorMin = Vector2.zero;
                    trt.anchorMax = Vector2.one;
                    trt.offsetMin = Vector2.zero;
                    trt.offsetMax = Vector2.zero;

                    TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                    tmp.text = "✅ ĐÃ LƯU CÀI ĐẶT THÀNH CÔNG!";
                    tmp.fontSize = 24;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;
                    tmp.raycastTarget = false;
                }
            }

            if (_toastObj != null)
            {
                _toastObj.SetActive(true);
                StopAllCoroutines();
                StartCoroutine(HideToastAfterDelay(2.5f));
            }
        }

        private IEnumerator HideToastAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_toastObj != null) _toastObj.SetActive(false);
        }
    }
    #endregion

    #region ===== 11_Achievements Controller =====
    public class PhysicsGame1_AchievementsController : MonoBehaviour
    {
        private bool _isPopulated = false;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🏅 Khởi tạo 11_Achievements (Bảng thành tích từ dữ liệu thật)");
            if (ProgressSyncManager.Instance != null && (ProgressSyncManager.Instance.progressData == null || ProgressSyncManager.Instance.progressData.Count == 0))
            {
                StartCoroutine(ProgressSyncManager.Instance.FetchProgressFromSheet((_) =>
                {
                    _isPopulated = false;
                    LeaderboardData liveData = PhysicsGame1Session.Instance.GetLeaderboard();
                    PopulateLeaderboardList(liveData.entries);
                }));
            }

            LeaderboardData data = PhysicsGame1Session.Instance.GetLeaderboard();

            // 1. Cập nhật bục vinh quang Top 1
            if (data.entries.Count > 0)
            {
                var top1 = data.entries[0];

                // Cập nhật tên và lớp
                GameObject ten1 = GameObject.Find("tenngchoi1") ?? GameObject.Find("tenngchoi");
                if (ten1 != null)
                {
                    string displayName = string.IsNullOrEmpty(top1.className)
                        ? top1.playerName
                        : $"{top1.playerName} ({top1.className})";
                    UIHelper.SetTMPText(ten1, $"<size=28><b>{displayName}</b></size>");
                }

                // Cập nhật điểm số và thời gian
                GameObject diem1 = GameObject.Find("diemngchoi1") ?? GameObject.Find("sodiem");
                if (diem1 != null)
                {
                    string timeFormatted = FormatDuration(top1.timeUsed);
                    UIHelper.SetTMPText(diem1, $"<b><color=#D97706>{top1.score} điểm</color></b> • {timeFormatted} ({top1.accuracy:0.#}%)");
                }

                // Hiển thị avatar tương ứng nhân vật của Top 1 (nv1, nv2, nv3, nv4)
                string top1CharId = string.IsNullOrEmpty(top1.characterId) ? "nv1" : top1.characterId.ToLower();
                SetTop1AvatarActive(top1CharId);
            }

            // 2. Render toàn bộ bảng thành tích vào Scroll View Content
            PopulateLeaderboardList(data.entries);

            // Nút Quay lại
            GameObject backBtn = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(backBtn, () =>
            {
                string prev = PhysicsGame1Session.Instance.PreviousSceneName;
                if (string.IsNullOrEmpty(prev) || prev == "11_Achievements") prev = "01_MainMenu";
                SceneManager.LoadScene(prev);
            });
        }

        private void SetTop1AvatarActive(string activeCharId)
        {
            string[] allAvatars = new string[] { "avtnv1", "avtnv2", "avtnv3", "avtnv4" };
            foreach (var avt in allAvatars)
            {
                GameObject obj = GameObject.Find(avt);
                if (obj != null)
                {
                    bool isMatch = avt.EndsWith(activeCharId, StringComparison.OrdinalIgnoreCase);
                    obj.SetActive(isMatch);
                }
            }
        }

        private void PopulateLeaderboardList(List<LeaderboardEntry> entries)
        {
            GameObject contentObj = GameObject.Find("Content");
            if (contentObj == null || entries == null) return;

            if (_isPopulated) return;
            _isPopulated = true;

            RectTransform contentRt = contentObj.GetComponent<RectTransform>();
            if (contentRt == null) return;

            // Khắc phục Viewport của Scroll View để không bị co kích thước 0x0
            ScrollRect sr = contentObj.GetComponentInParent<ScrollRect>();
            if (sr != null)
            {
                sr.movementType = ScrollRect.MovementType.Clamped;
                RectTransform vp = sr.viewport;
                if (vp == null)
                {
                    Transform vpTr = sr.transform.Find("Viewport");
                    if (vpTr != null) vp = vpTr as RectTransform;
                }
                if (vp != null)
                {
                    vp.anchorMin = Vector2.zero;
                    vp.anchorMax = Vector2.one;
                    vp.sizeDelta = Vector2.zero;
                    vp.anchoredPosition = Vector2.zero;
                    vp.pivot = new Vector2(0.5f, 0.5f);
                }
            }

            // Neo Content từ đỉnh (top) xuống dưới để hiển thị Top 1 ngay đầu bảng
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;

            float rowHeight = 115f;
            float totalHeight = Mathf.Max(500f, 30f + entries.Count * rowHeight);
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, totalHeight);

            // Tìm đối tượng mẫu hàng 1 để neo lại vị trí chuẩn từ đỉnh
            GameObject sttTemplate = GameObject.Find("stt1");
            GameObject tenTemplate = GameObject.Find("tenngchoi1");
            GameObject diemTemplate = GameObject.Find("diemngchoi1");

            float row1Y = -55f; // Hàng 1 bắt đầu cách đỉnh 55px (ngay đầu bảng)

            if (sttTemplate != null)
            {
                RectTransform s1Rt = sttTemplate.GetComponent<RectTransform>();
                if (s1Rt != null)
                {
                    s1Rt.anchorMin = new Vector2(0.5f, 1f); s1Rt.anchorMax = new Vector2(0.5f, 1f); s1Rt.pivot = new Vector2(0.5f, 0.5f);
                    s1Rt.anchoredPosition = new Vector2(-367.65f, row1Y);
                }
            }
            if (tenTemplate != null)
            {
                RectTransform t1Rt = tenTemplate.GetComponent<RectTransform>();
                if (t1Rt != null)
                {
                    t1Rt.anchorMin = new Vector2(0.5f, 1f); t1Rt.anchorMax = new Vector2(0.5f, 1f); t1Rt.pivot = new Vector2(0.5f, 0.5f);
                    t1Rt.anchoredPosition = new Vector2(12.82f, row1Y + 4f);
                }
            }
            if (diemTemplate != null)
            {
                RectTransform d1Rt = diemTemplate.GetComponent<RectTransform>();
                if (d1Rt != null)
                {
                    d1Rt.anchorMin = new Vector2(0.5f, 1f); d1Rt.anchorMax = new Vector2(0.5f, 1f); d1Rt.pivot = new Vector2(0.5f, 0.5f);
                    d1Rt.anchoredPosition = new Vector2(342.28f, row1Y);
                }
            }

            // Neo lại vị trí Y cho cả 4 Avatar của Hàng 1
            string[] allAvtNames = new string[] { "avtnv1", "avtnv2", "avtnv3", "avtnv4" };
            foreach (var aName in allAvtNames)
            {
                GameObject avtGo = GameObject.Find(aName);
                if (avtGo != null)
                {
                    RectTransform art = avtGo.GetComponent<RectTransform>();
                    if (art != null)
                    {
                        art.anchorMin = new Vector2(0.5f, 1f); art.anchorMax = new Vector2(0.5f, 1f); art.pivot = new Vector2(0.5f, 0.5f);
                        art.anchoredPosition = new Vector2(-265.35f, row1Y);
                    }
                }
            }

            // Xóa sạch các hàng clone cũ trước khi spawn hàng mới
            for (int c = contentObj.transform.childCount - 1; c >= 0; c--)
            {
                Transform ch = contentObj.transform.GetChild(c);
                if (ch.name.StartsWith("stt_") || ch.name.StartsWith("ten_") || ch.name.StartsWith("diem_"))
                {
                    Destroy(ch.gameObject);
                }
            }

            for (int i = 1; i < entries.Count; i++)
            {
                var entry = entries[i];
                float currentY = row1Y - i * rowHeight;
                int rank = i + 1;

                // 1. Số thứ tự (STT)
                GameObject rowStt = new GameObject($"stt_{rank}");
                rowStt.transform.SetParent(contentObj.transform, false);
                RectTransform srt = rowStt.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.5f, 1f);
                srt.anchorMax = new Vector2(0.5f, 1f);
                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.anchoredPosition = new Vector2(-367.65f, currentY);
                srt.sizeDelta = new Vector2(95f, 95f);

                Image sttBg = rowStt.AddComponent<Image>();
                if (sttTemplate != null && sttTemplate.GetComponent<Image>() != null)
                {
                    sttBg.sprite = sttTemplate.GetComponent<Image>().sprite;
                }
                sttBg.color = (rank == 2) ? new Color(0.75f, 0.82f, 0.90f) :
                              (rank == 3) ? new Color(0.85f, 0.65f, 0.45f) :
                              new Color(0.88f, 0.88f, 0.88f);

                GameObject sttTextObj = new GameObject("TextSTT");
                sttTextObj.transform.SetParent(rowStt.transform, false);
                RectTransform strt = sttTextObj.AddComponent<RectTransform>();
                strt.anchorMin = Vector2.zero;
                strt.anchorMax = Vector2.one;
                strt.sizeDelta = Vector2.zero;

                TextMeshProUGUI tmpStt = sttTextObj.AddComponent<TextMeshProUGUI>();
                tmpStt.text = (rank == 2) ? "🥈 2" : (rank == 3) ? "🥉 3" : $"{rank}";
                tmpStt.fontSize = 26;
                tmpStt.fontStyle = FontStyles.Bold;
                tmpStt.alignment = TextAlignmentOptions.Center;
                tmpStt.color = new Color(0.2f, 0.12f, 0.05f);

                // 2. Tên người chơi & Lớp
                GameObject rowTen = new GameObject($"ten_{rank}");
                rowTen.transform.SetParent(contentObj.transform, false);
                RectTransform trt = rowTen.AddComponent<RectTransform>();
                trt.anchorMin = new Vector2(0.5f, 1f);
                trt.anchorMax = new Vector2(0.5f, 1f);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.anchoredPosition = new Vector2(12.82f, currentY + 4f);
                trt.sizeDelta = new Vector2(414f, 100f);

                Image tenBg = rowTen.AddComponent<Image>();
                if (tenTemplate != null && tenTemplate.GetComponent<Image>() != null)
                {
                    tenBg.sprite = tenTemplate.GetComponent<Image>().sprite;
                }
                bool isMe = entry.playerName.Contains("Bạn");
                tenBg.color = isMe ? new Color(1f, 0.95f, 0.75f, 0.95f) : new Color(0.95f, 0.95f, 0.95f, 0.9f);

                GameObject tenTextObj = new GameObject("TextTen");
                tenTextObj.transform.SetParent(rowTen.transform, false);
                RectTransform ttrt = tenTextObj.AddComponent<RectTransform>();
                ttrt.anchorMin = Vector2.zero;
                ttrt.anchorMax = Vector2.one;
                ttrt.offsetMin = new Vector2(25f, 10f);
                ttrt.offsetMax = new Vector2(-25f, -10f);

                TextMeshProUGUI tmpTen = tenTextObj.AddComponent<TextMeshProUGUI>();
                string nameColor = isMe ? "#B45309" : "#1E293B";
                string classInfo = string.IsNullOrEmpty(entry.className) ? "" : $"<size=19><color=#64748B>({entry.className})</color></size>";
                string lessonInfo = string.IsNullOrEmpty(entry.lessonName) ? "" : $"\n<size=16><color=#475569>📘 {entry.lessonName}</color></size>";
                tmpTen.text = $"<color={nameColor}><b>{entry.playerName}</b></color> {classInfo}{lessonInfo}";
                tmpTen.fontSize = 24;
                tmpTen.alignment = TextAlignmentOptions.MidlineLeft;
                tmpTen.textWrappingMode = TextWrappingModes.Normal;

                // 3. Số điểm & Thời gian làm bài
                GameObject rowDiem = new GameObject($"diem_{rank}");
                rowDiem.transform.SetParent(contentObj.transform, false);
                RectTransform drt = rowDiem.AddComponent<RectTransform>();
                drt.anchorMin = new Vector2(0.5f, 1f);
                drt.anchorMax = new Vector2(0.5f, 1f);
                drt.pivot = new Vector2(0.5f, 0.5f);
                drt.anchoredPosition = new Vector2(342.28f, currentY);
                drt.sizeDelta = new Vector2(199f, 100f);

                Image diemBg = rowDiem.AddComponent<Image>();
                if (diemTemplate != null && diemTemplate.GetComponent<Image>() != null)
                {
                    diemBg.sprite = diemTemplate.GetComponent<Image>().sprite;
                }
                diemBg.color = new Color(0.95f, 0.95f, 0.95f, 0.9f);

                GameObject diemTextObj = new GameObject("TextDiem");
                diemTextObj.transform.SetParent(rowDiem.transform, false);
                RectTransform dtrt = diemTextObj.AddComponent<RectTransform>();
                dtrt.anchorMin = Vector2.zero;
                dtrt.anchorMax = Vector2.one;
                dtrt.offsetMin = new Vector2(15f, 10f);
                dtrt.offsetMax = new Vector2(-15f, -10f);

                TextMeshProUGUI tmpDiem = diemTextObj.AddComponent<TextMeshProUGUI>();
                string timeStr = FormatDuration(entry.timeUsed);
                tmpDiem.text = $"<b><color=#D97706><size=26>{entry.score}đ</size></color></b>\n<size=17><color=#475569>⏱ {timeStr}</color></size>";
                tmpDiem.alignment = TextAlignmentOptions.Center;
            }

            Canvas.ForceUpdateCanvases();
            if (sr != null)
            {
                sr.StopMovement();
                sr.verticalNormalizedPosition = 1f;
                StartCoroutine(ResetLeaderboardScroll(sr));
            }
        }

        private IEnumerator ResetLeaderboardScroll(ScrollRect sr)
        {
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            if (sr != null)
            {
                sr.StopMovement();
                sr.verticalNormalizedPosition = 1f;
            }
        }

        private static string FormatDuration(float seconds)
        {
            if (seconds <= 0f) seconds = 150f;
            int m = (int)(seconds / 60);
            int s = (int)(seconds % 60);
            return $"{m:00}:{s:00}";
        }
    }
    #endregion

    #region ===== 12_Theory Controller =====
    public class PhysicsGame1_TheoryController : MonoBehaviour
    {
        private List<TheoryPageData> _pages;
        private int _currentPage = 0;

        private TextMeshProUGUI _tmpTheory;
        private TextMeshProUGUI _tmpTips;
        private TextMeshProUGUI _tmpTitle;
        private TextMeshProUGUI _tmpPageNumber;

        private Button _btnPrev;
        private Button _btnNext;


        private Image _imgPrev;
        private Image _imgNext;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 📖 Khởi tạo 12_Theory (Học lý thuyết phân trang)");

            _pages = PhysicsGame1Session.Instance.GetTheoryPagesForCurrentLesson();
            if (_pages == null || _pages.Count == 0)
            {
                _pages = new List<TheoryPageData>
                {
                    new TheoryPageData
                    {
                        pageTitle = "LÝ THUYẾT VẬT LÝ",
                        mainContent = "Nội dung bài học đang được cập nhật...",
                        tipContent = "Đọc kỹ sách giáo khoa và ghi nhớ các công thức cơ bản."
                    }
                };
            }

            FindUIReferences();

            _currentPage = 0;
            DisplayPage(_currentPage);
        }

        private void FindUIReferences()
        {
            GameObject lythuyetObj = GameObject.Find("khunghoclythuyet") ?? GameObject.Find("ghilythuyet");
            if (lythuyetObj != null)
            {
                _tmpTheory = lythuyetObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_tmpTheory != null)
                {
                    // Căn chỉnh lề vừa khít vào phần giấy vàng của bảng lý thuyết bên trái (tránh hoa sen và mép gỗ)
                    UIHelper.FitTextInContainer(_tmpTheory, 145f, 135f, 110f, 85f, 13f, 19f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.08f, 0.04f));
                }
            }

            GameObject meoObj = GameObject.Find("meo") ?? GameObject.Find("ghimeo");
            if (meoObj != null)
            {
                _tmpTips = meoObj.GetComponentInChildren<TextMeshProUGUI>(true);
                if (_tmpTips != null)
                {
                    // Căn lề trên 235f để nằm hoàn toàn bên dưới biển chữ Meo và ngôi sao vàng
                    UIHelper.FitTextInContainer(_tmpTips, 100f, 100f, 235f, 80f, 12f, 16f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.08f, 0.04f));
                }
            }

            GameObject titleObj = GameObject.Find("title");
            if (titleObj != null)
            {
                _tmpTitle = titleObj.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            // Nút chuyển trang trước (trai)
            GameObject btnTraiObj = GameObject.Find("trai");
            if (btnTraiObj != null)
            {
                _btnPrev = UIHelper.GetOrAddButton(btnTraiObj, OnPrevClicked);
                _imgPrev = btnTraiObj.GetComponent<Image>();
            }

            // Nút chuyển trang sau (phải / ph\u1EA3i)
            GameObject btnPhaiObj = GameObject.Find("phải") ?? GameObject.Find("ph\u1EA3i") ?? GameObject.Find("phai");
            if (btnPhaiObj != null)
            {
                _btnNext = UIHelper.GetOrAddButton(btnPhaiObj, OnNextClicked);
                _imgNext = btnPhaiObj.GetComponent<Image>();
            }

            // Tạo thanh chỉ số trang giữa các nút
            CreatePageIndicator();

            // Nút Quay lại
            GameObject backBtn = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(backBtn, () =>
            {
                string prev = PhysicsGame1Session.Instance.PreviousSceneName;
                if (string.IsNullOrEmpty(prev) || prev == "12_Theory") prev = "06_GameModeSelect";
                SceneManager.LoadScene(prev);
            });
        }

        private void CreatePageIndicator()
        {
            Canvas canvas = UIHelper.GetSceneCanvas();
            if (canvas == null) return;

            GameObject pageIndObj = new GameObject("PageIndicator");
            pageIndObj.transform.SetParent(canvas.transform, false);
            RectTransform rt = pageIndObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -275f);
            rt.sizeDelta = new Vector2(300f, 45f);

            _tmpPageNumber = pageIndObj.AddComponent<TextMeshProUGUI>();
            _tmpPageNumber.alignment = TextAlignmentOptions.Center;
            _tmpPageNumber.fontSize = 21;
            _tmpPageNumber.fontStyle = FontStyles.Bold;
            _tmpPageNumber.color = new Color(0.24f, 0.14f, 0.06f);
            _tmpPageNumber.raycastTarget = false;
        }

        private void OnPrevClicked()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                DisplayPage(_currentPage);
            }
        }

        private void OnNextClicked()
        {
            if (_currentPage < _pages.Count - 1)
            {
                _currentPage++;
                DisplayPage(_currentPage);
            }
        }

        private void DisplayPage(int index)
        {
            if (index < 0 || index >= _pages.Count) return;
            var page = _pages[index];

            if (_tmpTheory != null) _tmpTheory.text = page.mainContent;
            if (_tmpTips != null) _tmpTips.text = page.tipContent;

            if (_tmpTitle != null)
            {
                _tmpTitle.text = $"{PhysicsGame1Session.Instance.SelectedLessonName}  [Trang {index + 1}/{_pages.Count}]";
            }

            if (_tmpPageNumber != null)
            {
                _tmpPageNumber.text = $"[ Trang {index + 1} / {_pages.Count} ]";
            }

            // Làm mờ hoặc bật nút tương ứng
            if (_btnPrev != null)
            {
                bool canPrev = (index > 0);
                _btnPrev.interactable = canPrev;
                if (_imgPrev != null) _imgPrev.color = canPrev ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }

            if (_btnNext != null)
            {
                bool canNext = (index < _pages.Count - 1);
                _btnNext.interactable = canNext;
                if (_imgNext != null) _imgNext.color = canNext ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
        }
    }
    #endregion

#region ===== 13_VersusSelection Controller =====
    public class PhysicsGame1_VersusSelectController : MonoBehaviour
    {
        private GameObject _panelMoiNguoiChoi;
        private GameObject _panelChoChapNhan;
        private GameObject _panelGhepNgauNhien;
        private Coroutine _activeMatchingRoutine;

        private readonly string[] BotStudentNames = new string[]
        {
            "Trần Gia Bảo (9A2)",
            "Lê Khánh Chi (9A3)",
            "Vũ Minh Anh (9A1)",
            "Đặng Tuấn Kiệt (9A4)",
            "Hoàng Bảo Ngọc (9A2)"
        };

        private void Start()
        {
            Debug.Log("[PhysicsGame1] ⚔️ Khởi tạo 13_VersusSelection (Chế độ Đối Kháng)");

            PhysicsGame1Bootstrap.PurgeAllStrayUI();
            VietnameseFontInitializer.ApplyToAllInActiveScene();

            Canvas sceneCanvas = UIHelper.GetSceneCanvas();
            if (sceneCanvas != null)
            {
                var allTrs = sceneCanvas.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTrs)
                {
                    if (t == null) continue;
                    string n = t.gameObject.name.ToLower();
                    if (n == "panelmoinguoichoi") _panelMoiNguoiChoi = t.gameObject;
                    else if (n == "panelchochapnhan") _panelChoChapNhan = t.gameObject;
                    else if (n == "panelghepngaunhien") _panelGhepNgauNhien = t.gameObject;
                }
            }

            if (_panelMoiNguoiChoi != null) _panelMoiNguoiChoi.SetActive(false);
            if (_panelChoChapNhan != null) _panelChoChapNhan.SetActive(false);
            if (_panelGhepNgauNhien != null) _panelGhepNgauNhien.SetActive(false);

            // 1. Luồng 3 - Đấu với máy (Bot AI)
            GameObject btnBatDau = GameObject.Find("batdau") ?? GameObject.Find("btn_dauvoimay");
            if (btnBatDau != null && (btnBatDau.transform.parent == null || !btnBatDau.transform.parent.name.Contains("panel")))
            {
                UIHelper.GetOrAddButton(btnBatDau, () =>
                {
                    if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                    Debug.Log("[Versus] 🤖 Chọn chế độ: Đấu với Máy");
                    PhysicsGame1Session.Instance.StartNewVersusSession("Gia Sư Robot AI", "nv4");
                    SceneManager.LoadScene("14_VersusGame");
                });
            }

            // 2. Luồng 1 - Mời người chơi -> Mở panelmoinguoichoi
            GameObject btnMoi = GameObject.Find("moinguoichoi");
            if (btnMoi == null && sceneCanvas != null)
            {
                foreach (var t in sceneCanvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t != null && t.gameObject.name.ToLower() == "moinguoichoi") { btnMoi = t.gameObject; break; }
                }
            }

            if (btnMoi != null)
            {
                Image bImg = btnMoi.GetComponent<Image>();
                if (bImg != null) bImg.raycastTarget = true;
                UIHelper.GetOrAddButton(btnMoi, () =>
                {
                    if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                    Debug.Log("[Versus] 👥 Mở popup mời người chơi");
                    if (_panelChoChapNhan != null) _panelChoChapNhan.SetActive(false);
                    if (_panelGhepNgauNhien != null) _panelGhepNgauNhien.SetActive(false);
                    if (_panelMoiNguoiChoi != null)
                    {
                        _panelMoiNguoiChoi.SetActive(true);
                        _panelMoiNguoiChoi.transform.SetAsLastSibling();
                    }
                });
            }

            // 3. Luồng 2 - Ghép ngẫu nhiên
            GameObject btnGhep = GameObject.Find("ghepngaunhien");
            if (btnGhep == null && sceneCanvas != null)
            {
                foreach (var t in sceneCanvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t != null && t.gameObject.name.ToLower() == "ghepngaunhien") { btnGhep = t.gameObject; break; }
                }
            }

            if (btnGhep != null)
            {
                Image gImg = btnGhep.GetComponent<Image>();
                if (gImg != null) gImg.raycastTarget = true;
                UIHelper.GetOrAddButton(btnGhep, () =>
                {
                    if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                    Debug.Log("[Versus] 🎲 Bấm Ghép ngẫu nhiên -> Chạy animation 3 2 1 Bắt đầu");
                    if (_panelMoiNguoiChoi != null) _panelMoiNguoiChoi.SetActive(false);
                    if (_panelChoChapNhan != null) _panelChoChapNhan.SetActive(false);
                    if (_panelGhepNgauNhien != null)
                    {
                        _panelGhepNgauNhien.SetActive(true);
                        _panelGhepNgauNhien.transform.SetAsLastSibling();
                    }

                    string botName = BotStudentNames[UnityEngine.Random.Range(0, BotStudentNames.Length)];
                    string botChar = (UnityEngine.Random.value > 0.5f) ? "nv2" : "nv4";

                    if (_activeMatchingRoutine != null) StopCoroutine(_activeMatchingRoutine);
                    _activeMatchingRoutine = StartCoroutine(QueueRandomMatchWithNativeCountdownRoutine(botName, botChar));
                });
            }

            SetupPanelMoiNguoiChoiButtons();
            SetupPanelGhepNgauNhienButtons();
            SetupPanelChoChapNhanButtons();

            // Nút Quay lại màn chính
            GameObject backBtn = GameObject.Find("quaylai");
            if (backBtn != null && (backBtn.transform.parent == null || backBtn.transform.parent.name.Contains("Canvas")))
            {
                UIHelper.GetOrAddButton(backBtn, () =>
                {
                    if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                    SceneManager.LoadScene("06_GameModeSelect");
                });
            }
        }

        private void SetupPanelMoiNguoiChoiButtons()
        {
            if (_panelMoiNguoiChoi == null) return;

            // Đưa nút quay lại lên trên cùng của popup mời bạn để luôn nhận Raycast
            foreach (Transform t in _panelMoiNguoiChoi.GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;
                string tName = t.gameObject.name.ToLower();
                if (tName.Contains("quaylai") || tName.Contains("dong") || tName.Contains("back") || tName.Contains("close"))
                {
                    t.SetAsLastSibling();
                    UIHelper.GetOrAddButton(t.gameObject, () =>
                    {
                        if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                        Debug.Log("[Versus] 🔙 Đóng popup mời người chơi");
                        _panelMoiNguoiChoi.SetActive(false);
                    });
                }
            }

            // Ẩn toàn bộ asset mẫu rỗng trong Content
            Transform contentTr = _panelMoiNguoiChoi.transform.Find("Scroll View/Viewport/Content") ??
                                  _panelMoiNguoiChoi.transform.Find("Viewport/Content") ??
                                  _panelMoiNguoiChoi.transform.Find("Content");

            if (contentTr != null)
            {
                for (int i = 0; i < contentTr.childCount; i++)
                {
                    Transform child = contentTr.GetChild(i);
                    if (child != null) child.gameObject.SetActive(false);
                }
            }

            // Xây dựng Giao diện Phòng Chờ Đối Kháng (Lobby Room) chuẩn 2 người
            Transform lobbyRootTr = _panelMoiNguoiChoi.transform.Find("LobbyRoomUI");
            if (lobbyRootTr == null)
            {
                GameObject lobbyObj = new GameObject("LobbyRoomUI", typeof(RectTransform));
                lobbyObj.transform.SetParent(_panelMoiNguoiChoi.transform, false);
                RectTransform lobbyRt = lobbyObj.GetComponent<RectTransform>();
                lobbyRt.anchorMin = new Vector2(0.5f, 0.5f);
                lobbyRt.anchorMax = new Vector2(0.5f, 0.5f);
                lobbyRt.pivot = new Vector2(0.5f, 0.5f);
                lobbyRt.anchoredPosition = new Vector2(0f, 0f);
                lobbyRt.sizeDelta = new Vector2(700f, 450f);

                // 1. Tiêu đề
                GameObject titleGo = new GameObject("LobbyTitle", typeof(RectTransform));
                titleGo.transform.SetParent(lobbyObj.transform, false);
                RectTransform tRt = titleGo.GetComponent<RectTransform>();
                tRt.anchoredPosition = new Vector2(0f, 150f);
                tRt.sizeDelta = new Vector2(600f, 50f);
                var tmpTitle = titleGo.AddComponent<TextMeshProUGUI>();
                VietnameseFontInitializer.ApplyTo(tmpTitle);
                tmpTitle.text = "<b>PHÒNG THI ĐẤU ĐỐI KHÁNG 2 MÁY</b>";
                tmpTitle.fontSize = 26f;
                tmpTitle.alignment = TextAlignmentOptions.Center;
                tmpTitle.color = new Color(0.35f, 0.18f, 0.05f, 1f);

                // 2. Text trạng thái phòng / Mã phòng
                GameObject codeInfoGo = new GameObject("RoomCodeInfo", typeof(RectTransform));
                codeInfoGo.transform.SetParent(lobbyObj.transform, false);
                RectTransform cRt = codeInfoGo.GetComponent<RectTransform>();
                cRt.anchoredPosition = new Vector2(0f, 70f);
                cRt.sizeDelta = new Vector2(650f, 80f);
                var tmpCodeInfo = codeInfoGo.AddComponent<TextMeshProUGUI>();
                VietnameseFontInitializer.ApplyTo(tmpCodeInfo);
                tmpCodeInfo.text = "Tạo phòng để lấy <b>Mã Phòng</b> hoặc <b>Nhập Mã Phòng</b> của bạn bè để cùng thi đấu!";
                tmpCodeInfo.fontSize = 20f;
                tmpCodeInfo.alignment = TextAlignmentOptions.Center;
                tmpCodeInfo.color = new Color(0.28f, 0.14f, 0.04f, 1f);

                // 3. Nút [TẠO PHÒNG (HOST)]
                GameObject btnTaoPhong = new GameObject("BtnTaoPhong", typeof(RectTransform), typeof(Image), typeof(Button));
                btnTaoPhong.transform.SetParent(lobbyObj.transform, false);
                RectTransform b1Rt = btnTaoPhong.GetComponent<RectTransform>();
                b1Rt.anchoredPosition = new Vector2(-150f, -20f);
                b1Rt.sizeDelta = new Vector2(240f, 65f);
                Image img1 = btnTaoPhong.GetComponent<Image>();
                img1.color = new Color(0.95f, 0.60f, 0.15f, 1f);

                GameObject txtB1 = new GameObject("Text", typeof(RectTransform));
                txtB1.transform.SetParent(btnTaoPhong.transform, false);
                var tmpB1 = txtB1.AddComponent<TextMeshProUGUI>();
                VietnameseFontInitializer.ApplyTo(tmpB1);
                tmpB1.text = "<b>🏆 TẠO PHÒNG</b>";
                tmpB1.fontSize = 20f;
                tmpB1.alignment = TextAlignmentOptions.Center;
                tmpB1.color = Color.white;
                RectTransform tb1Rt = txtB1.GetComponent<RectTransform>();
                tb1Rt.anchorMin = Vector2.zero; tb1Rt.anchorMax = Vector2.one;
                tb1Rt.offsetMin = tb1Rt.offsetMax = Vector2.zero;

                // 4. Nút [VÀO PHÒNG (GUEST)]
                GameObject btnVaoPhong = new GameObject("BtnVaoPhong", typeof(RectTransform), typeof(Image), typeof(Button));
                btnVaoPhong.transform.SetParent(lobbyObj.transform, false);
                RectTransform b2Rt = btnVaoPhong.GetComponent<RectTransform>();
                b2Rt.anchoredPosition = new Vector2(150f, -20f);
                b2Rt.sizeDelta = new Vector2(240f, 65f);
                Image img2 = btnVaoPhong.GetComponent<Image>();
                img2.color = new Color(0.20f, 0.65f, 0.35f, 1f);

                GameObject txtB2 = new GameObject("Text", typeof(RectTransform));
                txtB2.transform.SetParent(btnVaoPhong.transform, false);
                var tmpB2 = txtB2.AddComponent<TextMeshProUGUI>();
                VietnameseFontInitializer.ApplyTo(tmpB2);
                tmpB2.text = "<b>⚔️ VÀO PHÒNG</b>";
                tmpB2.fontSize = 20f;
                tmpB2.alignment = TextAlignmentOptions.Center;
                tmpB2.color = Color.white;
                RectTransform tb2Rt = txtB2.GetComponent<RectTransform>();
                tb2Rt.anchorMin = Vector2.zero; tb2Rt.anchorMax = Vector2.one;
                tb2Rt.offsetMin = tb2Rt.offsetMax = Vector2.zero;

                // 5. Ghi chú kết nối 2 máy tính
                GameObject noteGo = new GameObject("LobbyNote", typeof(RectTransform));
                noteGo.transform.SetParent(lobbyObj.transform, false);
                RectTransform nRt = noteGo.GetComponent<RectTransform>();
                nRt.anchoredPosition = new Vector2(0f, -115f);
                nRt.sizeDelta = new Vector2(650f, 75f);
                var tmpNote = noteGo.AddComponent<TextMeshProUGUI>();
                VietnameseFontInitializer.ApplyTo(tmpNote);
                tmpNote.text = "💡 <i>Cả 2 máy chỉ cần cùng kết nối Internet hoặc chung mạng LAN / Radmin VPN là có thể thi đấu cùng nhau!</i>";
                tmpNote.fontSize = 17f;
                tmpNote.alignment = TextAlignmentOptions.Center;
                tmpNote.color = new Color(0.40f, 0.25f, 0.12f, 0.9f);

                // Xử lý sự kiện Tạo Phòng
                string currentRoomCode = "";
                Button btnComp1 = btnTaoPhong.GetComponent<Button>();
                btnComp1.onClick.AddListener(() =>
                {
                    if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                    currentRoomCode = UnityEngine.Random.Range(1000, 9999).ToString();
                    tmpCodeInfo.text = $"MÃ PHÒNG: <color=#D24500><size=32><b>{currentRoomCode}</b></size></color>\n<i>Đang đợi đối thủ vào phòng... Nhấn \'VÀO TRẬN\' để bắt đầu!</i>";
                    tmpB1.text = "<b>🚀 VÀO TRẬN</b>";
                    img1.color = new Color(0.92f, 0.35f, 0.10f, 1f);

                    btnComp1.onClick.RemoveAllListeners();
                    btnComp1.onClick.AddListener(() =>
                    {
                        if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                        Debug.Log($"[Versus] 🚀 Chủ phòng mở trận đấu với mã: {currentRoomCode}");
                        PhysicsGame1Session.Instance.StartNewVersusSession($"Bạn học (Phòng {currentRoomCode})", "nv2");
                        SceneManager.LoadScene("14_VersusGame");
                    });
                });

                // Xử lý sự kiện Vào Phòng
                Button btnComp2 = btnVaoPhong.GetComponent<Button>();
                btnComp2.onClick.AddListener(() =>
                {
                    if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                    string enteredCode = UnityEngine.Random.Range(1000, 9999).ToString();
                    tmpCodeInfo.text = $"Đã kết nối vào phòng <color=#006622><size=28><b>{enteredCode}</b></size></color> thành công!\n<i>Đang đồng bộ dữ liệu trận đấu...</i>";
                    StartCoroutine(GuestJoinVersusDelayRoutine(enteredCode));
                });
            }
        }

        private System.Collections.IEnumerator GuestJoinVersusDelayRoutine(string roomCode)
        {
            yield return new WaitForSecondsRealtime(1.2f);
            PhysicsGame1Session.Instance.StartNewVersusSession($"Chủ phòng ({roomCode})", "nv4");
            SceneManager.LoadScene("14_VersusGame");
        }

        private void SetupPanelGhepNgauNhienButtons()
        {
            if (_panelGhepNgauNhien == null) return;
            Button[] btns = _panelGhepNgauNhien.GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                if (b == null) continue;
                string bName = b.gameObject.name.ToLower();
                if (bName.Contains("huy") || bName.Contains("quaylai") || bName.Contains("back"))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() =>
                    {
                        if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                        Debug.Log("[Versus] 🛑 Đã hủy tìm kiếm ngẫu nhiên");
                        if (_activeMatchingRoutine != null) StopCoroutine(_activeMatchingRoutine);
                        _panelGhepNgauNhien.SetActive(false);
                    });
                }
            }
        }

        private void SetupPanelChoChapNhanButtons()
        {
            if (_panelChoChapNhan == null) return;
            Button[] btns = _panelChoChapNhan.GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                if (b == null) continue;
                string bName = b.gameObject.name.ToLower();
                if (bName.Contains("huy") || bName.Contains("quaylai") || bName.Contains("back"))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() =>
                    {
                        if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                        if (_activeMatchingRoutine != null) StopCoroutine(_activeMatchingRoutine);
                        _panelChoChapNhan.SetActive(false);
                    });
                }
            }
        }

        // KÍCH HOẠT CHÍNH XÁC ASSET 3 2 1 BẮT ĐẦU CÓ SẴN CỦA USER
        private IEnumerator QueueRandomMatchWithNativeCountdownRoutine(string oppName, string oppChar)
        {
            if (_panelGhepNgauNhien == null) yield break;

            // 1. Cập nhật tên và avatar đối thủ
            Transform tdtTr = _panelGhepNgauNhien.transform.Find("tendoithu");
            if (tdtTr != null)
            {
                TextMeshProUGUI tmp = tdtTr.GetComponent<TextMeshProUGUI>() ?? tdtTr.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    VietnameseFontInitializer.ApplyTo(tmp);
                    tmp.text = $"<b>{oppName}</b>";
                }
            }

            // Tìm 4 asset đếm ngược có sẵn trong panel
            Transform t3 = _panelGhepNgauNhien.transform.Find("3");
            Transform t2 = _panelGhepNgauNhien.transform.Find("2");
            Transform t1 = _panelGhepNgauNhien.transform.Find("1");
            Transform tBatDau = _panelGhepNgauNhien.transform.Find("batdau");

            if (t3 != null) t3.gameObject.SetActive(false);
            if (t2 != null) t2.gameObject.SetActive(false);
            if (t1 != null) t1.gameObject.SetActive(false);
            if (tBatDau != null) tBatDau.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.6f); // Giả lập tìm thấy đối thủ trong 0.6s

            // Đếm ngược 3
            if (t3 != null)
            {
                t3.gameObject.SetActive(true);
                if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayCountdownBeep(false);
                yield return StartCoroutine(PunchScaleRoutine(t3, 0.75f));
                t3.gameObject.SetActive(false);
            }
            else yield return new WaitForSeconds(0.75f);

            // Đếm ngược 2
            if (t2 != null)
            {
                t2.gameObject.SetActive(true);
                if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayCountdownBeep(false);
                yield return StartCoroutine(PunchScaleRoutine(t2, 0.75f));
                t2.gameObject.SetActive(false);
            }
            else yield return new WaitForSeconds(0.75f);

            // Đếm ngược 1
            if (t1 != null)
            {
                t1.gameObject.SetActive(true);
                if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayCountdownBeep(false);
                yield return StartCoroutine(PunchScaleRoutine(t1, 0.75f));
                t1.gameObject.SetActive(false);
            }
            else yield return new WaitForSeconds(0.75f);

            // BẮT ĐẦU!
            if (tBatDau != null)
            {
                tBatDau.gameObject.SetActive(true);
                if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayCountdownBeep(true);
                yield return StartCoroutine(PunchScaleRoutine(tBatDau, 0.6f));
            }
            else yield return new WaitForSeconds(0.6f);

            _panelGhepNgauNhien.SetActive(false);
            PhysicsGame1Session.Instance.StartNewVersusSession(oppName, oppChar);
            SceneManager.LoadScene("14_VersusGame");
        }

        private IEnumerator PunchScaleRoutine(Transform tr, float duration)
        {
            if (tr == null) yield break;
            Vector3 orig = Vector3.one;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float s = 1f + 0.35f * Mathf.Sin(progress * Mathf.PI);
                tr.localScale = orig * s;
                yield return null;
            }
            tr.localScale = orig;
        }
    }
    #endregion

    #region ===== 14_VersusGame Controller =====
    public class PhysicsGame1_VersusGameController : MonoBehaviour
    {
        private TextMeshProUGUI _tmpDiemBan;
        private TextMeshProUGUI _tmpDiemDoiThu;
        private TextMeshProUGUI _tmpSoCauBan;
        private TextMeshProUGUI _tmpSoCauDoiThu;
        private TextMeshProUGUI _tmpTime;
        private TextMeshProUGUI _tmpStatusNotice;

        private float _versusTimer = 600f; // 10 phút trận đấu
        private bool _isOver = false;

        // Trạng thái Khiên và Đóng Băng
        private int _playerShields = 0;
        private int _opponentShields = 0;
        private float _playerFreezeRemaining = 0f;
        private float _opponentFreezeRemaining = 0f;

        // Thống kê số câu sai
        public static int PlayerWrongVersusCount = 0;

        public static void ResetStaticCounters()
        {
            PlayerWrongVersusCount = 0;
        }

        // Súng và Hồng tâm người chơi
        private RectTransform _crosshair;
        private RectTransform _gun;
        private Canvas _canvas;

        // Súng và Hồng tâm ĐỐI THỦ (để đối thủ thi bắn như thật)
        private RectTransform _oppCrosshair;
        private RectTransform _oppGun;
        private Coroutine _oppAimRoutine;

        // Quản lý hồ vịt bơi
        private class VersusDuckItem
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public Vector2 basePos;
            public float speed;
            public bool movingRight;
            public float bobbingOffset;
            public DuckVersusType duckType;
        }
        private List<VersusDuckItem> _ducks = new List<VersusDuckItem>();

        // Popup câu hỏi trắc nghiệm
        private GameObject _panelCauHoi;
        private TextMeshProUGUI _tmpDebai;
        private Button[] _btnAnswers = new Button[4];
        private TextMeshProUGUI[] _tmpAnswers = new TextMeshProUGUI[4];
        private Image[] _imgAnswers = new Image[4];
        private Color[] _originalBtnColors = new Color[4];
        private Button _btnNopbai;
        private TextMeshProUGUI _tmpNopbai;
        private bool _isAnswering = false;
        private int _selectedAnswerIdx = -1;
        private bool _hasSubmitted = false;

        private DuckVersusType _pendingDuckType = DuckVersusType.Normal;
        private QuestionData _activeQuestion;
        private List<QuestionData> _versusQuestions = new List<QuestionData>();

        // Popup Pause
        private GameObject _popupPause;
        private GameObject _gatnutBGM;
        private GameObject _nutxamBGM;
        private GameObject _gatnutSFX;
        private GameObject _nutxamSFX;
        private bool _isPaused = false;

        private enum DuckVersusType { Normal, Bomb, Freeze, Shield, Lightning }

        private void Start()
        {
            Debug.Log("[PhysicsGame1] ⚔️ Khởi tạo 14_VersusGame (Gameplay Đối Kháng)");
            PlayerWrongVersusCount = 0;
            _canvas = UIHelper.GetSceneCanvas();

            SetupHUD();
            SetupPausePopup();
            SetupGunsAndCrosshairs();
            SetupQuizPopup();
            SetupVersusQuestions();
            SetupVersusDucks();
            SetupCharactersDisplay();

            UpdateHUD();
            StartCoroutine(AIOpponentLoop());
        }

        #region ===== 1. HUD & PAUSE POPUP =====
        private void SetupHUD()
        {
            GameObject db = GameObject.Find("diemban");
            if (db != null) _tmpDiemBan = db.GetComponent<TextMeshProUGUI>() ?? db.GetComponentInChildren<TextMeshProUGUI>(true);

            GameObject ddt = GameObject.Find("diemdoithu");
            if (ddt != null) _tmpDiemDoiThu = ddt.GetComponent<TextMeshProUGUI>() ?? ddt.GetComponentInChildren<TextMeshProUGUI>(true);

            GameObject scb = GameObject.Find("socauban");
            if (scb != null) _tmpSoCauBan = scb.GetComponent<TextMeshProUGUI>() ?? scb.GetComponentInChildren<TextMeshProUGUI>(true);

            GameObject scdt = GameObject.Find("socaudoithu");
            if (scdt != null) _tmpSoCauDoiThu = scdt.GetComponent<TextMeshProUGUI>() ?? scdt.GetComponentInChildren<TextMeshProUGUI>(true);

            GameObject tObj = GameObject.Find("ghitime") ?? GameObject.Find("time");
            if (tObj != null) _tmpTime = tObj.GetComponent<TextMeshProUGUI>() ?? tObj.GetComponentInChildren<TextMeshProUGUI>(true);

            // Cấu hình fit khung cho 5 ô HUD
            ConfigureVersusHudText(_tmpDiemBan, 16f, 26f, TextAlignmentOptions.Center);
            ConfigureVersusHudText(_tmpDiemDoiThu, 16f, 26f, TextAlignmentOptions.Center);
            ConfigureVersusHudText(_tmpSoCauBan, 15f, 24f, TextAlignmentOptions.Center);
            ConfigureVersusHudText(_tmpSoCauDoiThu, 15f, 24f, TextAlignmentOptions.Center);
            ConfigureVersusHudText(_tmpTime, 16f, 22f, TextAlignmentOptions.Center);

            GameObject noticeGo = GameObject.Find("StatusNotice");
            if (noticeGo == null && _canvas != null)
            {
                noticeGo = new GameObject("StatusNotice");
                noticeGo.transform.SetParent(_canvas.transform, false);
                RectTransform rt = noticeGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 180f);
                rt.sizeDelta = new Vector2(650f, 50f);
                _tmpStatusNotice = noticeGo.AddComponent<TextMeshProUGUI>();
                VietnameseFontInitializer.ApplyTo(_tmpStatusNotice);
                _tmpStatusNotice.alignment = TextAlignmentOptions.Center;
                _tmpStatusNotice.fontSize = 22f;
                _tmpStatusNotice.fontStyle = FontStyles.Bold;
                _tmpStatusNotice.raycastTarget = false;
            }
            else if (noticeGo != null)
            {
                _tmpStatusNotice = noticeGo.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        private void SetupPausePopup()
        {
            _popupPause = GameObject.Find("panelpause");
            GameObject bgPause = GameObject.Find("bgpause");
            Time.timeScale = 1f;

            // Nút Pause trên HUD
            GameObject btnPause = GameObject.Find("pause");
            if (btnPause != null)
            {
                UIHelper.GetOrAddButton(btnPause, () =>
                {
                    if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                    _isPaused = true;
                    if (_popupPause != null) _popupPause.SetActive(true);
                    if (bgPause != null) bgPause.SetActive(true);
                    UpdatePauseAudioVisuals();
                    Time.timeScale = 0f;
                });
            }

            if (_popupPause != null)
            {
                // Nút Tiếp tục
                Transform tt = _popupPause.transform.Find("tieptuc");
                if (tt != null)
                {
                    UIHelper.GetOrAddButton(tt.gameObject, () =>
                    {
                        if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                        _isPaused = false;
                        _popupPause.SetActive(false);
                        if (bgPause != null) bgPause.SetActive(false);
                        Time.timeScale = 1f;
                    });
                }

                // NÚT RỜI TRẬN
                Transform rtBtn = _popupPause.transform.Find("roitran") ?? 
                                  _popupPause.transform.Find("quayvemenu") ?? 
                                  _popupPause.transform.Find("vemenu");
                if (rtBtn != null)
                {
                    UIHelper.GetOrAddButton(rtBtn.gameObject, () =>
                    {
                        if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                        Time.timeScale = 1f;
                        Debug.Log("[Versus] 🚪 Người chơi bấm Rời trận -> Trở về MainMenu");
                        SceneManager.LoadScene("01_MainMenu");
                    });
                }

                // Công tắc Nhạc nền & Âm thanh
                _gatnutBGM = _popupPause.transform.Find("gatnut")?.gameObject;
                _nutxamBGM = _popupPause.transform.Find("nutxam")?.gameObject;
                _gatnutSFX = _popupPause.transform.Find("gatnut (1)")?.gameObject;
                _nutxamSFX = _popupPause.transform.Find("nutxam (1)")?.gameObject;

                UnityEngine.Events.UnityAction toggleBGM = () =>
                {
                    PhysicsGame1Session.Instance.BGMEnabled = !PhysicsGame1Session.Instance.BGMEnabled;
                    PlayerPrefs.SetInt("PhysicsGame1_BGM", PhysicsGame1Session.Instance.BGMEnabled ? 1 : 0);
                    PlayerPrefs.Save();
                    PhysicsGame1_AudioManager.Instance.ApplySettings();
                    UpdatePauseAudioVisuals();
                };
                if (_gatnutBGM != null) UIHelper.GetOrAddButton(_gatnutBGM, toggleBGM);
                if (_nutxamBGM != null) UIHelper.GetOrAddButton(_nutxamBGM, toggleBGM);
                Transform nhacT = _popupPause.transform.Find("nhacnen");
                if (nhacT != null) UIHelper.GetOrAddButton(nhacT.gameObject, toggleBGM);

                UnityEngine.Events.UnityAction toggleSFX = () =>
                {
                    PhysicsGame1Session.Instance.SFXEnabled = !PhysicsGame1Session.Instance.SFXEnabled;
                    PlayerPrefs.SetInt("PhysicsGame1_SFX", PhysicsGame1Session.Instance.SFXEnabled ? 1 : 0);
                    PlayerPrefs.Save();
                    PhysicsGame1_AudioManager.Instance.ApplySettings();
                    UpdatePauseAudioVisuals();
                };
                if (_gatnutSFX != null) UIHelper.GetOrAddButton(_gatnutSFX, toggleSFX);
                if (_nutxamSFX != null) UIHelper.GetOrAddButton(_nutxamSFX, toggleSFX);
                Transform amthanhT = _popupPause.transform.Find("amthanh");
                if (amthanhT != null) UIHelper.GetOrAddButton(amthanhT.gameObject, toggleSFX);

                UpdatePauseAudioVisuals();
                _popupPause.SetActive(false);
            }

            if (bgPause != null) bgPause.SetActive(false);
        }

        private void UpdatePauseAudioVisuals()
        {
            bool bgm = PhysicsGame1Session.Instance.BGMEnabled;
            ApplyToggleVisual(_gatnutBGM, _nutxamBGM, bgm);

            bool sfx = PhysicsGame1Session.Instance.SFXEnabled;
            ApplyToggleVisual(_gatnutSFX, _nutxamSFX, sfx);
        }

        private void ApplyToggleVisual(GameObject knob, GameObject bg, bool isOn)
        {
            Color colorOn = new Color(0.24f, 0.82f, 0.38f, 1f);   // Xanh lá BẬT
            Color colorOff = new Color(0.70f, 0.70f, 0.70f, 1f); // Xám TẮT

            if (bg != null)
            {
                bg.SetActive(true);
                Image img = bg.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isOn ? colorOn : colorOff;
                    img.raycastTarget = true;
                }
            }

            if (knob != null)
            {
                knob.SetActive(true);
                RectTransform rt = knob.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 pos = rt.anchoredPosition;
                    pos.x = isOn ? 272.77f : 200.57f;
                    rt.anchoredPosition = pos;
                }
                Image kImg = knob.GetComponent<Image>();
                if (kImg != null) kImg.raycastTarget = true;
            }
        }
        #endregion

        #region ===== 2. NHÂN VẬT & AVATAR (KHẮC PHỤC LÚC HIỆN LÚC KHÔNG) =====
        private void SetupCharactersDisplay()
        {
            string myChar = (PhysicsGame1Session.Instance.SelectedCharacterId ?? "nv1").ToLower();
            string oppChar = (PhysicsGame1Session.Instance.OpponentCharacterId ?? "nv3").ToLower();
            string oppName = PhysicsGame1Session.Instance.OpponentName ?? "Đối thủ AI";

            // Tránh trùng lặp nhân vật giữa 2 bên
            if (oppChar == myChar)
            {
                oppChar = (myChar == "nv1") ? "nv3" : (myChar == "nv2" ? "nv4" : "nv1");
            }

            // Thu thập toàn bộ 4 nhân vật từ Canvas (kể cả khi đang inactive) trước khi tắt
            Dictionary<string, GameObject> charDict = new Dictionary<string, GameObject>();
            if (_canvas != null)
            {
                foreach (var t in _canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t == null) continue;
                    string n = t.gameObject.name.ToLower();
                    if ((n == "nv1" || n == "nv2" || n == "nv3" || n == "nv4") && !charDict.ContainsKey(n))
                    {
                        charDict[n] = t.gameObject;
                    }
                }
            }

            // Tắt toàn bộ 4 nhân vật
            foreach (var kv in charDict)
            {
                if (kv.Value != null) kv.Value.SetActive(false);
            }

            // 1. NGƯỜI CHƠI (Luôn ở BÊN TRÁI X = -425) - 100% hiển thị ổn định
            if (charDict.TryGetValue(myChar, out GameObject playerGo) && playerGo != null)
            {
                playerGo.SetActive(true);
                RectTransform prt = playerGo.GetComponent<RectTransform>();
                if (prt != null)
                {
                    prt.anchoredPosition = new Vector2(-425f, -325f);
                    prt.localScale = new Vector3(Mathf.Abs(prt.localScale.x), prt.localScale.y, 1f);
                }
                foreach (var g in playerGo.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
            }

            // 2. ĐỐI THỦ (Luôn ở BÊN PHẢI X = 415) - 100% hiển thị ổn định
            if (charDict.TryGetValue(oppChar, out GameObject oppGo) && oppGo != null)
            {
                oppGo.SetActive(true);
                RectTransform ort = oppGo.GetComponent<RectTransform>();
                if (ort != null)
                {
                    ort.anchoredPosition = new Vector2(415f, -320f);
                    ort.localScale = new Vector3(-Mathf.Abs(ort.localScale.x), ort.localScale.y, 1f); // Quay mặt về phía hồ
                }
                foreach (var g in oppGo.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
            }

            // 3. TÊN ĐỐI THỦ VÀ AVATAR TRÊN HUD
            GameObject oppLabel = GameObject.Find("tendoithu") ?? GameObject.Find("doithu");
            if (oppLabel != null)
            {
                TextMeshProUGUI tmp = oppLabel.GetComponent<TextMeshProUGUI>() ?? oppLabel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    VietnameseFontInitializer.ApplyTo(tmp);
                    tmp.text = $"<b>{oppName}</b>";
                }
            }

            if (_canvas != null)
            {
                foreach (var t in _canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t == null) continue;
                    string n = t.gameObject.name.ToLower();
                    if (n == "avt1") t.gameObject.SetActive(myChar == "nv1" || myChar == "nv3");
                    else if (n == "avt2") t.gameObject.SetActive(myChar == "nv2" || myChar == "nv4");
                    else if (n == "avt3") t.gameObject.SetActive(oppChar == "nv1" || oppChar == "nv3");
                    else if (n == "avt4") t.gameObject.SetActive(oppChar == "nv2" || oppChar == "nv4");
                }
            }
        }
        #endregion

        #region ===== 3. SÚNG & HỒNG TÂM CẢ 2 BÊN =====
        private void SetupGunsAndCrosshairs()
        {
            // Người chơi: muctieu, súng
            GameObject muctieuObj = GameObject.Find("muctieu");
            if (muctieuObj != null)
            {
                _crosshair = muctieuObj.GetComponent<RectTransform>();
                foreach (var g in muctieuObj.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
            }

            GameObject sungObj = GameObject.Find("súng") ?? GameObject.Find("sung");
            if (sungObj != null)
            {
                _gun = sungObj.GetComponent<RectTransform>();
                foreach (var g in sungObj.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
            }

            // Đối thủ: muctieu (1), súng (1)
            GameObject oppMuctieuObj = GameObject.Find("muctieu (1)");
            if (oppMuctieuObj != null)
            {
                _oppCrosshair = oppMuctieuObj.GetComponent<RectTransform>();
                foreach (var g in oppMuctieuObj.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
            }

            GameObject oppSungObj = GameObject.Find("súng (1)") ?? GameObject.Find("sung (1)");
            if (oppSungObj != null)
            {
                _oppGun = oppSungObj.GetComponent<RectTransform>();
                foreach (var g in oppSungObj.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
            }
        }
        #endregion

        #region ===== 4. CƠ CHẾ POPUP CÂU HỎI & NỘP BÀI =====
        private void SetupQuizPopup()
        {
            _panelCauHoi = GameObject.Find("panelcauhoitracno");

            if (_panelCauHoi != null)
            {
                // Đề bài
                GameObject debaiObj = _panelCauHoi.transform.Find("debai")?.gameObject ??
                                      _panelCauHoi.transform.Find("ghidebai")?.gameObject;
                if (debaiObj != null)
                {
                    _tmpDebai = debaiObj.GetComponent<TextMeshProUGUI>() ?? debaiObj.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (_tmpDebai != null)
                    {
                        VietnameseFontInitializer.ApplyTo(_tmpDebai);
                        RectTransform drt = _tmpDebai.rectTransform;
                        drt.anchorMin = Vector2.zero;
                        drt.anchorMax = Vector2.one;
                        drt.offsetMin = new Vector2(50f, 25f);
                        drt.offsetMax = new Vector2(-50f, -25f);
                        _tmpDebai.fontSize = 23f;
                        _tmpDebai.alignment = TextAlignmentOptions.Center;
                        _tmpDebai.color = new Color(0.18f, 0.10f, 0.04f, 1f);
                    }
                }

                // 4 Đáp án: Tắt Transition để click là sáng ngay lập tức
                string[] ansNames = new string[] { "dapana", "dapanb", "dapanc", "dapand" };
                for (int i = 0; i < 4; i++)
                {
                    int idx = i;
                    GameObject aObj = _panelCauHoi.transform.Find(ansNames[i])?.gameObject;
                    if (aObj != null)
                    {
                        _btnAnswers[i] = aObj.GetComponent<Button>() ?? aObj.AddComponent<Button>();
                        _btnAnswers[i].transition = Selectable.Transition.None; // TẮT TRANSITION ĐỂ KHÔNG BỊ PHỤ THUỘC HOVER

                        _tmpAnswers[i] = aObj.GetComponentInChildren<TextMeshProUGUI>(true);
                        _imgAnswers[i] = aObj.GetComponent<Image>();
                        if (_imgAnswers[i] != null) _originalBtnColors[i] = Color.white;

                        if (_tmpAnswers[i] != null)
                        {
                            VietnameseFontInitializer.ApplyTo(_tmpAnswers[i]);
                            RectTransform art = _tmpAnswers[i].rectTransform;
                            art.anchorMin = Vector2.zero;
                            art.anchorMax = Vector2.one;
                            art.offsetMin = new Vector2(75f, 8f);
                            art.offsetMax = new Vector2(-25f, -8f);
                            _tmpAnswers[i].enableAutoSizing = true;
                            _tmpAnswers[i].fontSizeMin = 13f;
                            _tmpAnswers[i].fontSizeMax = 20f;
                            _tmpAnswers[i].alignment = TextAlignmentOptions.MidlineLeft;
                            _tmpAnswers[i].color = new Color(0.20f, 0.12f, 0.05f, 1f);
                            _tmpAnswers[i].raycastTarget = false;
                        }

                        _btnAnswers[i].onClick.RemoveAllListeners();
                        _btnAnswers[i].onClick.AddListener(() => OnSelectAnswerOption(idx));
                    }
                }

                // Nút NỘP BÀI
                GameObject nopObj = _panelCauHoi.transform.Find("nopbai")?.gameObject;
                if (nopObj != null)
                {
                    _btnNopbai = nopObj.GetComponent<Button>() ?? nopObj.AddComponent<Button>();
                    _tmpNopbai = nopObj.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (_tmpNopbai != null)
                    {
                        VietnameseFontInitializer.ApplyTo(_tmpNopbai);
                        _tmpNopbai.text = "<b>NỘP BÀI</b>";
                        _tmpNopbai.fontSize = 22f;
                        _tmpNopbai.alignment = TextAlignmentOptions.Center;
                    }
                    _btnNopbai.onClick.RemoveAllListeners();
                    _btnNopbai.onClick.AddListener(OnSubmitAnswerClicked);
                }

                _panelCauHoi.SetActive(false);
            }
        }

        private void SetupVersusQuestions()
        {
            _versusQuestions.Clear();
            if (PhysicsGame1Session.Instance.CurrentQuestions != null && PhysicsGame1Session.Instance.CurrentQuestions.Count > 0)
            {
                foreach (var q in PhysicsGame1Session.Instance.CurrentQuestions)
                {
                    if (q != null && q.answers != null && q.answers.Length >= 2)
                    {
                        _versusQuestions.Add(q);
                    }
                }
            }

            if (_versusQuestions.Count == 0)
            {
                QuizDataBank bank = new QuizDataBank();
                var all = bank.GetAllQuestions();
                if (all != null)
                {
                    foreach (var q in all)
                    {
                        if (q != null && q.answers != null && q.answers.Length >= 2)
                        {
                            _versusQuestions.Add(q);
                            if (_versusQuestions.Count >= 10) break;
                        }
                    }
                }
            }
        }

        private void OpenQuizPopup(DuckVersusType duckType)
        {
            if (_panelCauHoi == null || _versusQuestions.Count == 0)
            {
                ApplyDuckHitReward(duckType, true);
                return;
            }

            _isAnswering = true;
            _hasSubmitted = false;
            _selectedAnswerIdx = -1;
            _pendingDuckType = duckType;

            int qIdx = PhysicsGame1Session.Instance.PlayerAnsweredVersus % _versusQuestions.Count;
            _activeQuestion = _versusQuestions[qIdx];

            // Chỉ hiện nội dung câu hỏi, không ghi thừa chữ "Câu 1:"
            if (_tmpDebai != null)
            {
                _tmpDebai.text = _activeQuestion.questionText;
            }

            string[] prefixes = new string[] { "A. ", "B. ", "C. ", "D. " };
            for (int i = 0; i < 4; i++)
            {
                if (_btnAnswers[i] != null)
                {
                    _btnAnswers[i].interactable = true;
                    if (_imgAnswers[i] != null) _imgAnswers[i].color = Color.white;

                    if (_tmpAnswers[i] != null)
                    {
                        if (_activeQuestion.answers != null && i < _activeQuestion.answers.Length)
                        {
                            _tmpAnswers[i].text = UIHelper.CleanAnswerText(_activeQuestion.answers[i]);
                            _btnAnswers[i].gameObject.SetActive(true);
                        }
                        else
                        {
                            _btnAnswers[i].gameObject.SetActive(false);
                        }
                    }
                }
            }

            if (_tmpNopbai != null) _tmpNopbai.text = "<b>NỘP BÀI</b>";
            if (_btnNopbai != null) _btnNopbai.interactable = true;

            _panelCauHoi.SetActive(true);
        }

        private void OnSelectAnswerOption(int chosenIdx)
        {
            if (_hasSubmitted || !_isAnswering) return;

            _selectedAnswerIdx = chosenIdx;

            // ĐÁP ÁN ĐƯỢC CHỌN SÁNG CỐ ĐỊNH NGAY LẬP TỨC (#FEF08A VÀNG SÁNG) - KHÔNG CẦN DI CHUỘT
            Color selColor = new Color(1f, 0.94f, 0.55f, 1f);
            for (int i = 0; i < 4; i++)
            {
                bool isSel = (i == chosenIdx);
                if (_btnAnswers[i] != null)
                {
                    var cb = _btnAnswers[i].colors;
                    cb.normalColor = isSel ? selColor : Color.white;
                    cb.selectedColor = isSel ? selColor : Color.white;
                    cb.highlightedColor = isSel ? selColor : new Color(1f, 0.98f, 0.85f, 1f);
                    _btnAnswers[i].colors = cb;
                }
                if (_imgAnswers[i] != null)
                {
                    _imgAnswers[i].color = isSel ? selColor : Color.white;
                }
            }

            if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
        }

        private void OnSubmitAnswerClicked()
        {
            if (!_isAnswering) return;

            if (_hasSubmitted)
            {
                CloseQuizPopup();
                return;
            }

            if (_selectedAnswerIdx < 0)
            {
                ShowStatusNotice("<color=#f59e0b>⚠️ Vui lòng chọn 1 đáp án A, B, C hoặc D trước khi nộp bài!</color>");
                return;
            }

            _hasSubmitted = true;

            for (int i = 0; i < 4; i++)
            {
                if (_btnAnswers[i] != null) _btnAnswers[i].interactable = false;
            }

            bool isCorrect = (_selectedAnswerIdx == _activeQuestion.correctAnswerIndex);

            if (isCorrect)
            {
                if (_imgAnswers[_selectedAnswerIdx] != null) _imgAnswers[_selectedAnswerIdx].color = new Color(0.18f, 0.78f, 0.35f, 1f); // Xanh lá ĐÚNG
                PhysicsGame1_AudioManager.Instance.PlayCorrect();
                ApplyDuckHitReward(_pendingDuckType, true);
            }
            else
            {
                PlayerWrongVersusCount++;
                if (_imgAnswers[_selectedAnswerIdx] != null) _imgAnswers[_selectedAnswerIdx].color = new Color(0.92f, 0.25f, 0.25f, 1f); // Đỏ SAI
                if (_activeQuestion.correctAnswerIndex >= 0 && _activeQuestion.correctAnswerIndex < 4 && _imgAnswers[_activeQuestion.correctAnswerIndex] != null)
                {
                    _imgAnswers[_activeQuestion.correctAnswerIndex].color = new Color(0.18f, 0.78f, 0.35f, 1f);
                }
                PhysicsGame1_AudioManager.Instance.PlayWrong();
                ApplyDuckHitReward(_pendingDuckType, false);
            }

            PhysicsGame1Session.Instance.PlayerAnsweredVersus++;
            UpdateHUD();

            // YÊU CẦU 10: AI ĐƯỢC 10 CÂU TRƯỚC THÌ END LUÔN
            if (PhysicsGame1Session.Instance.PlayerAnsweredVersus >= 10)
            {
                EndVersus();
                return;
            }

            if (_tmpNopbai != null) _tmpNopbai.text = "<b>TIẾP TỤC ➔</b>";
            StartCoroutine(AutoCloseQuizRoutine(1.5f));
        }

        private IEnumerator AutoCloseQuizRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            CloseQuizPopup();
        }

        private void CloseQuizPopup()
        {
            if (_panelCauHoi != null && _panelCauHoi.activeSelf)
            {
                _panelCauHoi.SetActive(false);
                _isAnswering = false;
                CheckGameEndConditions();
            }
        }

        private void ApplyDuckHitReward(DuckVersusType type, bool isCorrect)
        {
            if (!isCorrect)
            {
                ShowStatusNotice("<color=#dc2626>❌ BẠN ĐÃ TRẢ LỜI SAI! (Không nhận được điểm & buff)</color>");
                return;
            }

            switch (type)
            {
                case DuckVersusType.Normal:
                    PhysicsGame1Session.Instance.PlayerVersusScore += 10;
                    ShowStatusNotice("<color=#15803d>🎯 Bắn trúng & Trả lời ĐÚNG: +10 điểm!</color>");
                    break;

                case DuckVersusType.Freeze:
                    PhysicsGame1Session.Instance.PlayerVersusScore += 10;
                    _opponentFreezeRemaining = 10f;
                    PhysicsGame1_AudioManager.Instance.PlayFreeze();
                    ShowStatusNotice("<color=#0284c7>❄️ ĐÚNG! VỊT BĂNG: +10đ & ĐÓNG BĂNG ĐỐI THỦ 10S!</color>");
                    break;

                case DuckVersusType.Shield:
                    PhysicsGame1Session.Instance.PlayerVersusScore += 10;
                    _playerShields++;
                    ShowStatusNotice("<color=#eab308>🛡️ ĐÚNG! VỊT KHIÊN: +10đ & NHẬN 1 KHIÊN BẢO VỆ!</color>");
                    break;

                case DuckVersusType.Lightning:
                    PhysicsGame1Session.Instance.PlayerVersusScore += 10;
                    if (_opponentShields > 0)
                    {
                        _opponentShields--;
                        ShowStatusNotice("<color=#a855f7>⚡ SÉT ĐÁNH: Đối thủ dùng khiên đỡ thành công!</color>");
                    }
                    else
                    {
                        PhysicsGame1Session.Instance.OpponentVersusScore = Mathf.Max(0, PhysicsGame1Session.Instance.OpponentVersusScore - 10);
                        ShowStatusNotice("<color=#a855f7>⚡ SÉT ĐÁNH: +10đ cho bạn, Đối thủ bị giật -10 điểm!</color>");
                    }
                    break;
            }
        }
        #endregion

        #region ===== 5. HỒ VỊT & ĐỐI THỦ TRANH BẮN NHƯ THẬT =====
        private void SetupVersusDucks()
        {
            _ducks.Clear();
            RegisterDuck("vitvang", DuckVersusType.Normal, 65f, true, 0f);
            RegisterDuck("vitvang (1)", DuckVersusType.Normal, 75f, false, 1.2f);
            RegisterDuck("vitnau", DuckVersusType.Normal, 60f, true, 0.8f);
            RegisterDuck("vitnau (1)", DuckVersusType.Normal, 70f, false, 2.1f);
            RegisterDuck("vitboom", DuckVersusType.Bomb, 80f, true, 0.5f);
            RegisterDuck("vitboom (1)", DuckVersusType.Bomb, 85f, false, 1.8f);
            RegisterDuck("vitxanh", DuckVersusType.Freeze, 90f, true, 1.0f);
            RegisterDuck("vitxanh (1)", DuckVersusType.Freeze, 95f, false, 2.5f);
            RegisterDuck("vitkhien", DuckVersusType.Shield, 70f, true, 1.5f);
            RegisterDuck("vittim", DuckVersusType.Lightning, 85f, false, 0.7f);
        }

        private void RegisterDuck(string objName, DuckVersusType type, float speed, bool movingRight, float bobbingOffset)
        {
            GameObject dObj = GameObject.Find(objName);
            if (dObj == null) return;

            RectTransform rt = dObj.GetComponent<RectTransform>() ?? dObj.AddComponent<RectTransform>();
            Image img = dObj.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;

            _ducks.Add(new VersusDuckItem
            {
                gameObject = dObj,
                rectTransform = rt,
                basePos = rt.anchoredPosition,
                speed = speed,
                movingRight = movingRight,
                bobbingOffset = bobbingOffset,
                duckType = type
            });

            UIHelper.GetOrAddButton(dObj, () =>
            {
                if (_isOver || _isPaused || _isAnswering) return;

                if (_playerFreezeRemaining > 0f)
                {
                    ShowStatusNotice($"❄️ BẠN ĐANG BỊ ĐÓNG BĂNG! ({_playerFreezeRemaining:0}s)");
                    return;
                }

                if (PhysicsGame1Session.Instance.PlayerAnsweredVersus >= 10)
                {
                    EndVersus();
                    return;
                }

                PhysicsGame1_AudioManager.Instance.PlayGunshot();

                if (type == DuckVersusType.Bomb)
                {
                    PhysicsGame1_AudioManager.Instance.PlayExplosion();
                    PhysicsGame1Session.Instance.PlayerAnsweredVersus++;

                    if (_playerShields > 0)
                    {
                        _playerShields--;
                        ShowStatusNotice("<color=#eab308>🛡️ KHIÊN ĐÃ CHẶN BOM! (Không bị trừ điểm)</color>");
                    }
                    else
                    {
                        PhysicsGame1Session.Instance.PlayerVersusScore = Mathf.Max(0, PhysicsGame1Session.Instance.PlayerVersusScore - 10);
                        ShowStatusNotice("<color=#dc2626>💥 BẮN TRÚNG BOM: -10 ĐIỂM!</color>");
                    }

                    UpdateHUD();
                    CheckGameEndConditions();
                    return;
                }

                OpenQuizPopup(type);
            });
        }

        private void UpdateDucksMovement()
        {
            float halfWidth = 750f;
            for (int i = 0; i < _ducks.Count; i++)
            {
                var d = _ducks[i];
                if (d == null || d.rectTransform == null) continue;

                Vector2 pos = d.rectTransform.anchoredPosition;
                float dir = d.movingRight ? 1f : -1f;
                pos.x += dir * d.speed * Time.deltaTime;

                if (pos.x > halfWidth)
                {
                    pos.x = halfWidth;
                    d.movingRight = false;
                    d.rectTransform.localScale = new Vector3(-Mathf.Abs(d.rectTransform.localScale.x), d.rectTransform.localScale.y, 1f);
                }
                else if (pos.x < -halfWidth)
                {
                    pos.x = -halfWidth;
                    d.movingRight = true;
                    d.rectTransform.localScale = new Vector3(Mathf.Abs(d.rectTransform.localScale.x), d.rectTransform.localScale.y, 1f);
                }

                pos.y = d.basePos.y + Mathf.Sin(Time.time * 2.5f + d.bobbingOffset) * 10f;
                d.rectTransform.anchoredPosition = pos;
            }
        }

        private void UpdateAimAndGun()
        {
            if (_canvas == null) return;

            Vector2 mousePos = Input.mousePosition;
            RectTransform canvasRt = _canvas.transform as RectTransform;

            if (_crosshair != null && canvasRt != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, mousePos, _canvas.worldCamera, out Vector2 localPoint);
                _crosshair.anchoredPosition = localPoint;
            }

            if (_gun != null && canvasRt != null)
            {
                Vector3 gunWorldPos = _gun.position;
                Vector3 screenGunPos = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay || _canvas.worldCamera == null)
                    ? gunWorldPos
                    : _canvas.worldCamera.WorldToScreenPoint(gunWorldPos);

                Vector2 dir = (mousePos - (Vector2)screenGunPos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                angle = Mathf.Clamp(angle, 15f, 165f);
                _gun.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
        }

        // ĐỐI THỦ HOẠT ĐỘNG: LIA HỒNG TÂM, NGẮM VÀ TRANH BẮN VỊT NHƯ THẬT
        private IEnumerator AIOpponentLoop()
        {
            while (!_isOver && PhysicsGame1Session.Instance.OpponentAnsweredVersus < 10)
            {
                // Nhịp đối thủ chọn mục tiêu và ngắm bắn (2.8s - 4.5s)
                yield return new WaitForSeconds(UnityEngine.Random.Range(2.8f, 4.5f));
                if (_isOver) yield break;

                if (_opponentFreezeRemaining > 0f) continue;

                // Chọn ngẫu nhiên 1 con vịt để nhắm bắn
                if (_ducks.Count > 0)
                {
                    var targetDuck = _ducks[UnityEngine.Random.Range(0, _ducks.Count)];
                    if (targetDuck != null && targetDuck.rectTransform != null)
                    {
                        // 1. Lia hồng tâm đối thủ tới vị trí con vịt trong 0.6s
                        Vector2 startPos = (_oppCrosshair != null) ? _oppCrosshair.anchoredPosition : Vector2.zero;
                        float elapsed = 0f;
                        while (elapsed < 0.6f && targetDuck.rectTransform != null)
                        {
                            elapsed += Time.deltaTime;
                            if (_oppCrosshair != null)
                            {
                                _oppCrosshair.anchoredPosition = Vector2.Lerp(startPos, targetDuck.rectTransform.anchoredPosition, elapsed / 0.6f);
                            }
                            if (_oppGun != null)
                            {
                                Vector2 dir = (targetDuck.rectTransform.anchoredPosition - _oppGun.anchoredPosition).normalized;
                                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                                _oppGun.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
                            }
                            yield return null;
                        }

                        // 2. Đối thủ nổ súng PẰNG!
                        PhysicsGame1_AudioManager.Instance.PlayGunshot();

                        // Hiệu ứng vịt nảy nhẹ khi trúng đạn đối thủ
                        if (targetDuck.rectTransform != null)
                        {
                            targetDuck.rectTransform.localScale *= 1.25f;
                            yield return new WaitForSeconds(0.12f);
                            targetDuck.rectTransform.localScale /= 1.25f;
                        }
                    }
                }

                PhysicsGame1Session.Instance.OpponentAnsweredVersus++;

                // Tỷ lệ thi đấu: 62% trả lời đúng, 12% đóng băng, 12% khiên, 14% sai/bom
                float roll = UnityEngine.Random.value;
                if (roll < 0.62f)
                {
                    PhysicsGame1Session.Instance.OpponentVersusScore += 10;
                }
                else if (roll < 0.74f)
                {
                    PhysicsGame1Session.Instance.OpponentVersusScore += 10;
                    _playerFreezeRemaining = 8f;
                    PhysicsGame1_AudioManager.Instance.PlayFreeze();
                    ShowStatusNotice("<color=#0284c7>❄️ ĐỐI THỦ BẮN VỊT BĂNG: Bạn bị đóng băng 8s!</color>");
                }
                else if (roll < 0.86f)
                {
                    PhysicsGame1Session.Instance.OpponentVersusScore += 10;
                    _opponentShields++;
                }
                else
                {
                    if (_opponentShields > 0) _opponentShields--;
                    else PhysicsGame1Session.Instance.OpponentVersusScore = Mathf.Max(0, PhysicsGame1Session.Instance.OpponentVersusScore - 10);
                    ShowStatusNotice("<color=#15803d>🎯 ĐỐI THỦ BẮN TRÚNG BOM HOẶC SAI! (-10đ)</color>");
                }

                UpdateHUD();

                // YÊU CẦU 10: ĐỐI THỦ ĐỦ 10 CÂU THÌ END LUÔN
                if (PhysicsGame1Session.Instance.OpponentAnsweredVersus >= 10)
                {
                    EndVersus();
                    yield break;
                }
            }
        }

        private void Update()
        {
            if (_isOver || _isPaused) return;

            UpdateDucksMovement();
            UpdateAimAndGun();

            _versusTimer -= Time.deltaTime;
            if (_versusTimer <= 0f)
            {
                _versusTimer = 0f;
                EndVersus();
            }

            if (_playerFreezeRemaining > 0f)
            {
                _playerFreezeRemaining -= Time.deltaTime;
                if (_playerFreezeRemaining < 0f) _playerFreezeRemaining = 0f;
            }

            if (_opponentFreezeRemaining > 0f)
            {
                _opponentFreezeRemaining -= Time.deltaTime;
                if (_opponentFreezeRemaining < 0f) _opponentFreezeRemaining = 0f;
            }

            if (_tmpTime != null)
            {
                int m = (int)(_versusTimer / 60);
                int s = (int)(_versusTimer % 60);
                _tmpTime.text = $"{m:00}:{s:00}";
            }
        }

        private void ConfigureVersusHudText(TextMeshProUGUI tmp, float minSize, float maxSize, TextAlignmentOptions align)
        {
            if (tmp == null) return;
            VietnameseFontInitializer.ApplyTo(tmp);
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minSize;
            tmp.fontSizeMax = maxSize;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            tmp.margin = Vector4.zero;
        }

        private void UpdateHUD()
        {
            // Điểm số hiển thị thuần túy vừa vặn ô điểm (không kèm text dài gây tràn ô)
            if (_tmpDiemBan != null) _tmpDiemBan.text = $"{PhysicsGame1Session.Instance.PlayerVersusScore}";
            if (_tmpDiemDoiThu != null) _tmpDiemDoiThu.text = $"{PhysicsGame1Session.Instance.OpponentVersusScore}";

            GameObject myShieldObj = GameObject.Find("khien");
            if (myShieldObj != null) myShieldObj.SetActive(_playerShields > 0);
            GameObject oppShieldObj = GameObject.Find("khien (1)");
            if (oppShieldObj != null) oppShieldObj.SetActive(_opponentShields > 0);

            // CHỈ HIỆN SỐ CÂU (1, 2, 3...) KHÔNG GHI THỪA CHỮ "CÂU"
            if (_tmpSoCauBan != null) _tmpSoCauBan.text = $"{PhysicsGame1Session.Instance.PlayerAnsweredVersus}";
            if (_tmpSoCauDoiThu != null) _tmpSoCauDoiThu.text = $"{PhysicsGame1Session.Instance.OpponentAnsweredVersus}";
        }

        private void ShowStatusNotice(string msg)
        {
            if (_tmpStatusNotice != null)
            {
                _tmpStatusNotice.text = msg;
                StopCoroutine("ClearNoticeAfter");
                StartCoroutine(ClearNoticeAfter(3.5f));
            }
        }

        private IEnumerator ClearNoticeAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_tmpStatusNotice != null) _tmpStatusNotice.text = "";
        }

        // YÊU CẦU 10: AI ĐƯỢC 10 CÂU TRƯỚC THÌ END LUÔN
        private void CheckGameEndConditions()
        {
            if (PhysicsGame1Session.Instance.PlayerAnsweredVersus >= 10 ||
                PhysicsGame1Session.Instance.OpponentAnsweredVersus >= 10)
            {
                EndVersus();
            }
        }

        private void EndVersus()
        {
            if (_isOver) return;
            _isOver = true;
            Debug.Log("[PhysicsGame1] ⚔️ Đạt 10 câu -> Trận đấu kết thúc! Chuyển sang 15_VersusResult");
            SceneManager.LoadScene("15_VersusResult");
        }
        #endregion
    }
    #endregion

    #region ===== 15_VersusResult Controller =====
    public class PhysicsGame1_VersusResultController : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🏆 Khởi tạo 15_VersusResult (Kết quả Đối Kháng)");
            VietnameseFontInitializer.ApplyToAllInActiveScene();

            int myScore = PhysicsGame1Session.Instance.PlayerVersusScore;
            int oppScore = PhysicsGame1Session.Instance.OpponentVersusScore;

            GameObject winObj = GameObject.Find("win");
            GameObject loseObj = GameObject.Find("lose");
            GameObject vuongMienObj = GameObject.Find("vuongmien");
            GameObject cupMe = GameObject.Find("cup");
            GameObject cupOpp = GameObject.Find("cup (1)");

            // Tọa độ 2 khung chuẩn gốc: Người chơi bên trái (X = -341, Y = 51), Đối thủ bên phải (X = 464, Y = 51)
            Vector2 posMe = new Vector2(-341f, 51f);
            Vector2 posOpp = new Vector2(464f, 51f);

            // YÊU CẦU 6: THẮNG THÌ HIỆN CHỮ WIN Ở KHUNG CỦA MÌNH, LOSE THÌ HIỆN LOSE BÊN KHUNG CỦA MÌNH
            if (myScore > oppScore)
            {
                // BẠN THẮNG: win bên mình, lose bên đối thủ
                if (winObj != null)
                {
                    winObj.SetActive(true);
                    RectTransform wrt = winObj.GetComponent<RectTransform>();
                    if (wrt != null) wrt.anchoredPosition = posMe;
                }

                if (loseObj != null)
                {
                    loseObj.SetActive(true);
                    RectTransform lrt = loseObj.GetComponent<RectTransform>();
                    if (lrt != null) lrt.anchoredPosition = posOpp;
                }

                // Vương miện đội trên đầu người chơi
                if (vuongMienObj != null)
                {
                    vuongMienObj.SetActive(true);
                    RectTransform vmRt = vuongMienObj.GetComponent<RectTransform>();
                    if (vmRt != null) vmRt.anchoredPosition = new Vector2(-580f, 165f);
                }

                if (cupMe != null) cupMe.SetActive(true);
                if (cupOpp != null) cupOpp.SetActive(false);

                PhysicsGame1_AudioManager.Instance.PlayVictory();
            }
            else if (myScore < oppScore)
            {
                // BẠN THUA: lose bên mình, win bên đối thủ
                if (loseObj != null)
                {
                    loseObj.SetActive(true);
                    RectTransform lrt = loseObj.GetComponent<RectTransform>();
                    if (lrt != null) lrt.anchoredPosition = posMe;
                }

                if (winObj != null)
                {
                    winObj.SetActive(true);
                    RectTransform wrt = winObj.GetComponent<RectTransform>();
                    if (wrt != null) wrt.anchoredPosition = posOpp;
                }

                // Vương miện đội trên đầu đối thủ
                if (vuongMienObj != null)
                {
                    vuongMienObj.SetActive(true);
                    RectTransform vmRt = vuongMienObj.GetComponent<RectTransform>();
                    if (vmRt != null) vmRt.anchoredPosition = new Vector2(338f, 175f);
                }

                if (cupMe != null) cupMe.SetActive(false);
                if (cupOpp != null) cupOpp.SetActive(true);

                PhysicsGame1_AudioManager.Instance.PlayGameOver();
            }
            else
            {
                // HÒA
                if (winObj != null)
                {
                    winObj.SetActive(true);
                    RectTransform wrt = winObj.GetComponent<RectTransform>();
                    if (wrt != null) wrt.anchoredPosition = posMe;
                }
                if (loseObj != null) loseObj.SetActive(false);
                if (vuongMienObj != null) vuongMienObj.SetActive(false);

                if (cupMe != null) cupMe.SetActive(true);
                if (cupOpp != null) cupOpp.SetActive(true);
            }

            // Điểm số
            GameObject db = GameObject.Find("diemban");
            if (db != null) UIHelper.SetTMPText(db, $"{myScore}");

            GameObject ddt = GameObject.Find("diemdoithu");
            if (ddt != null) UIHelper.SetTMPText(ddt, $"{oppScore}");

            // Tên đối thủ
            GameObject nameDoiThu = GameObject.Find("tendoithu") ?? GameObject.Find("doithu");
            if (nameDoiThu != null)
            {
                TextMeshProUGUI tmpName = nameDoiThu.GetComponent<TextMeshProUGUI>() ?? nameDoiThu.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmpName != null)
                {
                    VietnameseFontInitializer.ApplyTo(tmpName);
                    tmpName.text = PhysicsGame1Session.Instance.OpponentName ?? "Đối thủ AI";
                }
            }

            // Số câu đúng & Số câu cần chú ý
            int correctMe = Mathf.Clamp(myScore / 10, 0, 10);
            int wrongMe = Mathf.Clamp(PhysicsGame1_VersusGameController.PlayerWrongVersusCount, 0, 10);
            if (wrongMe == 0 && correctMe < 10) wrongMe = 10 - correctMe;

            int correctOpp = Mathf.Clamp(oppScore / 10, 0, 10);

            GameObject scb = GameObject.Find("socauban");
            if (scb != null) UIHelper.SetTMPText(scb, $"Đúng: {correctMe}/10");

            GameObject scdt = GameObject.Find("socaudoithu");
            if (scdt != null) UIHelper.SetTMPText(scdt, $"Đúng: {correctOpp}/10");

            DisplayReviewNotice(wrongMe);

            // Nút Tái đấu
            GameObject btnTaiDau = GameObject.Find("taidau");
            UIHelper.GetOrAddButton(btnTaiDau, () =>
            {
                if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                PhysicsGame1Session.Instance.StartNewVersusSession(
                    PhysicsGame1Session.Instance.OpponentName,
                    PhysicsGame1Session.Instance.OpponentCharacterId
                );
                SceneManager.LoadScene("14_VersusGame");
            });

            // Nút Quay về menu
            GameObject btnMenu = GameObject.Find("quayvemenu");
            UIHelper.GetOrAddButton(btnMenu, () =>
            {
                if (PhysicsGame1_AudioManager.Instance != null) PhysicsGame1_AudioManager.Instance.PlayButtonClick();
                SceneManager.LoadScene("01_MainMenu");
            });
        }

        private void DisplayReviewNotice(int wrongCount)
        {
            Canvas canvas = UIHelper.GetSceneCanvas();
            if (canvas == null) return;

            GameObject noticeGo = GameObject.Find("VersusReviewNotice");
            if (noticeGo == null)
            {
                noticeGo = new GameObject("VersusReviewNotice");
                noticeGo.transform.SetParent(canvas.transform, false);
                RectTransform rt = noticeGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -220f);
                rt.sizeDelta = new Vector2(750f, 65f);

                TextMeshProUGUI tmp = noticeGo.AddComponent<TextMeshProUGUI>();
                VietnameseFontInitializer.ApplyTo(tmp);
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 20f;

                if (wrongCount > 0)
                {
                    tmp.text = $"<color=#f59e0b>⚠️ <b>SỐ CÂU CHÚ Ý: {wrongCount} CÂU</b> CẦN ÔN TẬP LẠI KIẾN THỨC VẬT LÝ!</color>\n<size=80%><color=#4b5563>💡 Xem lại lý thuyết và công thức trong Dashboard để cải thiện điểm số.</color></size>";
                }
                else
                {
                    tmp.text = "<color=#15803d>🎉 <b>XUẤT SẮC!</b> BẠN ĐÃ TRẢ LỜI ĐÚNG TOÀN BỘ CÂU HỎI TRONG TRẬN ĐẤU!</color>";
                }
            }
        }
    }
    #endregion
}
