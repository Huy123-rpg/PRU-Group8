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

            int score = PhysicsGame1Session.Instance.CurrentScore;
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
                correctCount = Mathf.Clamp(score / 10, 0, totalQ);
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
        private void SetupDecoratedScoreBoard(int score, int correctCount, int totalQ, string timeStr, bool isHappy)
        {
            GameObject frameObj = GameObject.Find("framescore");
            if (frameObj == null) return;

            // Hiệu ứng rơi bảng điểm đàn hồi khi vào màn chơi
            StartCoroutine(AnimateFrameEntrance(frameObj.GetComponent<RectTransform>()));

            // 1. Text Tiêu đề Đánh Giá (Nằm gọn bên trong bảng, không chạm viền trên)
            GameObject headerObj = new GameObject("ResultHeaderTitle");
            headerObj.transform.SetParent(frameObj.transform, false);
            RectTransform hrt = headerObj.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.5f, 0.5f);
            hrt.anchorMax = new Vector2(0.5f, 0.5f);
            hrt.pivot = new Vector2(0.5f, 0.5f);
            hrt.anchoredPosition = new Vector2(0f, -25f);
            hrt.sizeDelta = new Vector2(620f, 36f);

            TextMeshProUGUI tmpHeader = headerObj.AddComponent<TextMeshProUGUI>();
            tmpHeader.alignment = TextAlignmentOptions.Center;
            tmpHeader.fontStyle = FontStyles.Bold;
            tmpHeader.raycastTarget = false;

            if (score >= 80)
            {
                tmpHeader.text = "<size=25><color=#E65100><b>XUẤT SẮC! BẠN LÀ THIÊN TÀI VẬT LÝ</b></color></size>";
            }
            else if (score >= 50)
            {
                tmpHeader.text = "<size=25><color=#2E7D32><b>HOÀN THÀNH TỐT! TIẾP TỤC PHÁT HUY</b></color></size>";
            }
            else
            {
                tmpHeader.text = "<size=25><color=#C62828><b>CẦN CỐ GẮNG HƠN! ĐỪNG NẢN LÒNG NHÉ</b></color></size>";
            }

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

        private IEnumerator AnimateRollUpScore(int targetScore, bool isHappy)
        {
            float duration = 1.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                int currentDisplay = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, easeOut));

                if (_tmpScore != null)
                {
                    _tmpScore.text = $"<size=92><b>{currentDisplay}</b></size> <size=38><color=#795548>ĐIỂM</color></size>";
                }
                yield return null;
            }

            if (_tmpScore != null)
            {
                _tmpScore.text = $"<size=96><b>{targetScore}</b></size> <size=40><color=#795548>ĐIỂM</color></size>";

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

        private void Start()
        {
            Debug.Log("[PhysicsGame1] 📝 Khởi tạo 09_AnswerReview (Xem lời giải chi tiết)");

            _results = PhysicsGame1Session.Instance.QuestionResults;
            if (_results == null || _results.Count == 0)
            {
                Debug.LogWarning("[PhysicsGame1] ⚠️ Chưa có dữ liệu câu hỏi lượt vừa rồi, dùng dữ liệu mẫu");
                _results = new List<QuestionReviewItem>();
                for (int i = 0; i < 10; i++)
                {
                    _results.Add(new QuestionReviewItem
                    {
                        question = new QuestionData
                        {
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

            FindUIReferences();

            // Nút Câu trước & Câu tiếp
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
                UIHelper.FitTextInContainer(_tmpLoigiai, 85f, 85f, 150f, 60f, 13f, 24f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.08f, 0.04f));
            }
        }

        private void DisplayQuestion(int index)
        {
            if (index < 0 || index >= _results.Count) return;
            QuestionReviewItem item = _results[index];

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
                    string status = item.isCorrect 
                        ? $"<color=#15803d><b>[ ĐẠT ]</b> AI ĐÁNH GIÁ: {item.earnedScore}/10 ĐIỂM</color>" 
                        : $"<color=#b91c1c><b>[ CHƯA ĐẠT ]</b> AI ĐÁNH GIÁ: {item.earnedScore}/10 ĐIỂM</color>";

                    string studentText = !string.IsNullOrEmpty(item.essayAnswerText) 
                        ? item.essayAnswerText 
                        : (!string.IsNullOrEmpty(item.studentDisplayAnswer) ? item.studentDisplayAnswer : "(Chưa có câu trả lời)");

                    string fb = !string.IsNullOrEmpty(item.aiFeedback) ? item.aiFeedback : item.explanation;
                    string sugg = !string.IsNullOrEmpty(item.aiSuggestions) ? $"\n\n<b><color=#0284c7>Gợi ý cải thiện:</color></b>\n{item.aiSuggestions}" : "";

                    _tmpLoigiai.text = $"{status}\n\n<b><color=#7c2d12>Bài làm của học sinh:</color></b>\n\"{studentText}\"\n\n<b><color=#1d4ed8>Nhận xét chi tiết từ AI:</color></b>\n{fb}{sugg}";
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

            // Khởi tạo ô nhập Google Gemini API Key
            SetupApiKeyInput();

            // Nút Lưu cài đặt: Lưu cấu hình, hiển thị thông báo thành công
            GameObject saveBtn = GameObject.Find("luucaidat");
            UIHelper.GetOrAddButton(saveBtn, () =>
            {
                PhysicsGame1Session.Instance.BGMEnabled = _tempBGM;
                PhysicsGame1Session.Instance.SFXEnabled = _tempSFX;
                PlayerPrefs.SetInt("PhysicsGame1_BGM", _tempBGM ? 1 : 0);
                PlayerPrefs.SetInt("PhysicsGame1_SFX", _tempSFX ? 1 : 0);

                if (_apiKeyInput != null)
                {
                    string key = _apiKeyInput.text.Trim();
                    PlayerPrefs.SetString("Gemini_API_Key", key);
                    AIGradingClient.EnsureInstance().SaveGeminiKey(key);
                }

                PlayerPrefs.Save();
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

        private TMP_InputField _apiKeyInput;

        private void SetupApiKeyInput()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject apiGroup = new GameObject("GeminiApiKeyGroup");
            apiGroup.transform.SetParent(canvas.transform, false);

            RectTransform grt = apiGroup.AddComponent<RectTransform>();
            grt.anchorMin = new Vector2(0.5f, 0.5f);
            grt.anchorMax = new Vector2(0.5f, 0.5f);
            grt.pivot = new Vector2(0.5f, 0.5f);
            grt.anchoredPosition = new Vector2(0f, -115f);
            grt.sizeDelta = new Vector2(560f, 65f);

            Image bg = apiGroup.AddComponent<Image>();
            bg.color = new Color(0.96f, 0.94f, 0.90f, 0.95f);

            GameObject labelObj = new GameObject("ApiLabel");
            labelObj.transform.SetParent(apiGroup.transform, false);
            RectTransform lrt = labelObj.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 1f);
            lrt.anchorMax = new Vector2(1f, 1f);
            lrt.pivot = new Vector2(0.5f, 0f);
            lrt.anchoredPosition = new Vector2(0f, 4f);
            lrt.sizeDelta = new Vector2(0f, 25f);
            TextMeshProUGUI lbl = labelObj.AddComponent<TextMeshProUGUI>();
            lbl.text = "🔑 GOOGLE GEMINI API KEY (CHẤM TỰ LUẬN & ĐỌC VIẾT TAY):";
            lbl.fontSize = 15;
            lbl.fontStyle = FontStyles.Bold;
            lbl.color = new Color(0.25f, 0.16f, 0.08f);

            GameObject textObj = new GameObject("KeyText");
            textObj.transform.SetParent(apiGroup.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(15f, 10f);
            trt.offsetMax = new Vector2(-15f, -10f);
            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 17;
            txt.color = new Color(0.12f, 0.12f, 0.12f);
            txt.alignment = TextAlignmentOptions.MidlineLeft;

            _apiKeyInput = apiGroup.AddComponent<TMP_InputField>();
            _apiKeyInput.textComponent = txt;
            string savedKey = PlayerPrefs.GetString("Gemini_API_Key", "");
            if (string.IsNullOrEmpty(savedKey) && AIGradingClient.EnsureInstance() != null)
            {
                savedKey = AIGradingClient.EnsureInstance().geminiApiKey;
            }
            _apiKeyInput.text = savedKey;
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
                Canvas canvas = FindFirstObjectByType<Canvas>();
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
        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🏅 Khởi tạo 11_Achievements (Bảng thành tích từ dữ liệu thật)");

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
            if (contentObj == null || entries == null || entries.Count <= 1) return;

            RectTransform contentRt = contentObj.GetComponent<RectTransform>();
            if (contentRt == null) return;

            // Đặt kích thước chiều dọc cho Content để cuộn vừa vặn toàn bộ học sinh
            float rowHeight = 115f;
            float totalHeight = 160f + (entries.Count - 1) * rowHeight;
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, Mathf.Max(320f, totalHeight));

            // Tìm đối tượng mẫu hàng 1 để nhân bản style
            GameObject sttTemplate = GameObject.Find("stt1");
            GameObject tenTemplate = GameObject.Find("tenngchoi1");
            GameObject diemTemplate = GameObject.Find("diemngchoi1");

            float startY = 100f; // Vị trí Y của hàng 1 trong Content

            for (int i = 1; i < entries.Count; i++)
            {
                var entry = entries[i];
                float currentY = startY - i * rowHeight;
                int rank = i + 1;

                // 1. Số thứ tự (STT)
                GameObject rowStt = new GameObject($"stt_{rank}");
                rowStt.transform.SetParent(contentObj.transform, false);
                RectTransform srt = rowStt.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.5f, 0.5f);
                srt.anchorMax = new Vector2(0.5f, 0.5f);
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
                trt.anchorMin = new Vector2(0.5f, 0.5f);
                trt.anchorMax = new Vector2(0.5f, 0.5f);
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
                drt.anchorMin = new Vector2(0.5f, 0.5f);
                drt.anchorMax = new Vector2(0.5f, 0.5f);
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
                SceneManager.LoadScene("06_GameModeSelect");
            });
        }

        private void CreatePageIndicator()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
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
        private GameObject _panelChoChapNhan;
        private GameObject _panelGhepNgauNhien;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] ⚔️ Khởi tạo 13_VersusSelection");

            _panelChoChapNhan = GameObject.Find("panelchochapnhan");
            _panelGhepNgauNhien = GameObject.Find("panelghepngaunhien");

            if (_panelChoChapNhan != null) _panelChoChapNhan.SetActive(false);
            if (_panelGhepNgauNhien != null) _panelGhepNgauNhien.SetActive(false);

            // Đấu với máy / Bắt đầu ngay
            GameObject btnBatDau = GameObject.Find("batdau");
            UIHelper.GetOrAddButton(btnBatDau, () =>
            {
                PhysicsGame1Session.Instance.StartNewVersusSession("Gia Sư AI");
                SceneManager.LoadScene("14_VersusGame");
            });

            // Mời người chơi
            GameObject btnMoi = GameObject.Find("moinguoichoi");
            UIHelper.GetOrAddButton(btnMoi, () =>
            {
                if (_panelChoChapNhan != null) _panelChoChapNhan.SetActive(true);
                StartCoroutine(SimulateMatching("Bạn Cùng Tiến (Online)"));
            });

            // Ghép ngẫu nhiên
            GameObject btnGhep = GameObject.Find("ghepngaunhien");
            UIHelper.GetOrAddButton(btnGhep, () =>
            {
                if (_panelGhepNgauNhien != null) _panelGhepNgauNhien.SetActive(true);
                StartCoroutine(SimulateMatching("Đối Thủ Ngẫu Nhiên"));
            });

            // Nút Quay lại: Trở về 06_GameModeSelect
            GameObject backBtn = GameObject.Find("quaylai");
            UIHelper.GetOrAddButton(backBtn, () =>
            {
                SceneManager.LoadScene("06_GameModeSelect");
            });
        }

        private IEnumerator SimulateMatching(string opponentName)
        {
            yield return new WaitForSeconds(2.0f);
            PhysicsGame1Session.Instance.StartNewVersusSession(opponentName);
            SceneManager.LoadScene("14_VersusGame");
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

        private float _versusTimer = 600f; // 10 phút
        private bool _isOver = false;

        private void Start()
        {
            Debug.Log("[PhysicsGame1] ⚔️ Khởi tạo 14_VersusGame");

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

            // Gán sự kiện bắn vịt trong Versus
            string[] duckNames = new string[] { "vitvang", "vitvang (1)", "vitnau", "vitnau (1)", "vitxanh", "vitxanh (1)" };
            foreach (var dn in duckNames)
            {
                GameObject d = GameObject.Find(dn);
                if (d != null)
                {
                    UIHelper.GetOrAddButton(d, () =>
                    {
                        if (_isOver) return;
                        PhysicsGame1Session.Instance.PlayerVersusScore += 10;
                        PhysicsGame1Session.Instance.PlayerAnsweredVersus++;
                        UpdateHUD();

                        if (PhysicsGame1Session.Instance.PlayerAnsweredVersus >= 10)
                        {
                            EndVersus();
                        }
                    });
                }
            }

            // Đối thủ máy tự động bắn định kỳ
            StartCoroutine(AIOpponentLoop());

            UpdateHUD();
        }

        private IEnumerator AIOpponentLoop()
        {
            while (!_isOver && PhysicsGame1Session.Instance.OpponentAnsweredVersus < 10)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 8f));
                if (_isOver) yield break;

                // 75% tỷ lệ trả lời đúng của AI
                if (UnityEngine.Random.value < 0.75f)
                {
                    PhysicsGame1Session.Instance.OpponentVersusScore += 10;
                }
                PhysicsGame1Session.Instance.OpponentAnsweredVersus++;
                UpdateHUD();

                if (PhysicsGame1Session.Instance.OpponentAnsweredVersus >= 10 &&
                    PhysicsGame1Session.Instance.PlayerAnsweredVersus >= 10)
                {
                    EndVersus();
                }
            }
        }

        private void Update()
        {
            if (_isOver) return;

            _versusTimer -= Time.deltaTime;
            if (_versusTimer <= 0f)
            {
                _versusTimer = 0f;
                EndVersus();
            }

            if (_tmpTime != null)
            {
                int m = (int)(_versusTimer / 60);
                int s = (int)(_versusTimer % 60);
                _tmpTime.text = $"{m:00}:{s:00}";
            }
        }

        private void UpdateHUD()
        {
            if (_tmpDiemBan != null) _tmpDiemBan.text = $"{PhysicsGame1Session.Instance.PlayerVersusScore}";
            if (_tmpDiemDoiThu != null) _tmpDiemDoiThu.text = $"{PhysicsGame1Session.Instance.OpponentVersusScore}";
            if (_tmpSoCauBan != null) _tmpSoCauBan.text = $"CÂU: {PhysicsGame1Session.Instance.PlayerAnsweredVersus} / 10";
            if (_tmpSoCauDoiThu != null) _tmpSoCauDoiThu.text = $"CÂU: {PhysicsGame1Session.Instance.OpponentAnsweredVersus} / 10";
        }

        private void EndVersus()
        {
            if (_isOver) return;
            _isOver = true;
            Debug.Log("[PhysicsGame1] ⚔️ Kết thúc trận đấu đối kháng!");
            SceneManager.LoadScene("15_VersusResult");
        }
    }
    #endregion

    #region ===== 15_VersusResult Controller =====
    public class PhysicsGame1_VersusResultController : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[PhysicsGame1] 🏆 Khởi tạo 15_VersusResult");

            int myScore = PhysicsGame1Session.Instance.PlayerVersusScore;
            int oppScore = PhysicsGame1Session.Instance.OpponentVersusScore;

            GameObject winObj = GameObject.Find("win");
            GameObject loseObj = GameObject.Find("lose");

            if (myScore > oppScore)
            {
                if (winObj != null) winObj.SetActive(true);
                if (loseObj != null) loseObj.SetActive(false);
            }
            else if (myScore < oppScore)
            {
                if (winObj != null) winObj.SetActive(false);
                if (loseObj != null) loseObj.SetActive(true);
            }
            else
            {
                if (winObj != null) winObj.SetActive(true);
                if (loseObj != null) loseObj.SetActive(true); // Hòa
            }

            GameObject db = GameObject.Find("diemban");
            if (db != null) UIHelper.SetTMPText(db, $"{myScore}");

            GameObject ddt = GameObject.Find("diemdoithu");
            if (ddt != null) UIHelper.SetTMPText(ddt, $"{oppScore}");

            // Nút Tái đấu: Tạo trận mới
            GameObject btnTaiDau = GameObject.Find("taidau");
            UIHelper.GetOrAddButton(btnTaiDau, () =>
            {
                PhysicsGame1Session.Instance.StartNewVersusSession(PhysicsGame1Session.Instance.OpponentName);
                SceneManager.LoadScene("14_VersusGame");
            });

            // Nút Quay về menu: Về 01_MainMenu
            GameObject btnMenu = GameObject.Find("quayvemenu");
            UIHelper.GetOrAddButton(btnMenu, () =>
            {
                SceneManager.LoadScene("01_MainMenu");
            });
        }
    }
    #endregion
}
