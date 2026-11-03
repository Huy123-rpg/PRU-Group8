using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    [Serializable]
    public class PhysicsTheoryItem
    {
        public string lessonId;
        public string chapterId;
        public string chapterName;
        public string lessonTitle;
        public string keyPoints;
        public string formulas;
        public string applications;
        public string fullText;
    }

    [Serializable]
    public class PhysicsTheoryContainer
    {
        public List<PhysicsTheoryItem> lessons;
    }

    /// <summary>
    /// Repository cung cấp dữ liệu Tóm tắt Lý thuyết chuẩn SGK Vật lý 9 cho các bài học.
    /// Tự động nạp từ Resources/TheoryData/physics_theory.json hoặc fallback code.
    /// </summary>
    public static class PhysicsTheoryRepository
    {
        private static readonly Dictionary<string, PhysicsTheoryItem> _theoryMap = new Dictionary<string, PhysicsTheoryItem>(StringComparer.OrdinalIgnoreCase);
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            // 1. Thử load từ Resources
            TextAsset jsonAsset = Resources.Load<TextAsset>("TheoryData/physics_theory");
            if (jsonAsset != null && !string.IsNullOrEmpty(jsonAsset.text))
            {
                try
                {
                    PhysicsTheoryContainer container = JsonUtility.FromJson<PhysicsTheoryContainer>(jsonAsset.text);
                    if (container != null && container.lessons != null && container.lessons.Count > 0)
                    {
                        foreach (var item in container.lessons)
                        {
                            if (!string.IsNullOrEmpty(item.lessonId))
                            {
                                _theoryMap[item.lessonId] = item;
                            }
                        }
                        Debug.Log($"[PhysicsTheoryRepository] Đã tải thành công {_theoryMap.Count} bài lý thuyết từ JSON.");
                        _initialized = true;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PhysicsTheoryRepository] Lỗi parse JSON, chuyển sang fallback: {ex.Message}");
                }
            }

            // 2. Fallback cơ bản
            AddFallbackData();
            _initialized = true;
            Debug.Log($"[PhysicsTheoryRepository] Đã khởi tạo fallback với {_theoryMap.Count} bài lý thuyết.");
        }

        public static PhysicsTheoryItem GetTheoryForLesson(string lessonId)
        {
            if (!_initialized) Initialize();

            if (string.IsNullOrEmpty(lessonId)) return null;

            // Thử map trực tiếp
            if (_theoryMap.TryGetValue(lessonId, out var item))
            {
                return item;
            }

            // Thử map chuẩn hóa (vd C1_L2 -> bai2)
            string normalized = NormalizeLessonId(lessonId);
            if (_theoryMap.TryGetValue(normalized, out item))
            {
                return item;
            }

            return null;
        }

        private static string NormalizeLessonId(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            string lower = id.ToLower();
            if (lower.Contains("l2")) return "bai2";
            if (lower.Contains("l3")) return "bai3";
            if (lower.Contains("l4")) return "bai4";
            if (lower.Contains("l5")) return "bai5";
            if (lower.Contains("l6")) return "bai6";
            if (lower.Contains("l7")) return "bai7";
            if (lower.Contains("l8")) return "bai8";
            if (lower.Contains("l9")) return "bai9";
            if (lower.Contains("l10")) return "bai10";
            if (lower.Contains("l11")) return "bai11";
            if (lower.Contains("l12")) return "bai12";
            if (lower.Contains("l13")) return "bai13";
            if (lower.Contains("l14")) return "bai14";
            if (lower.Contains("l15")) return "bai15";
            if (lower.Contains("l16")) return "bai16";
            if (lower.Contains("l17")) return "bai17";
            if (lower.Contains("c1_test")) return "test_chuong1";
            if (lower.Contains("c2_test")) return "test_chuong2";
            if (lower.Contains("c3_test")) return "test_chuong3";
            if (lower.Contains("c4_test")) return "test_chuong4";
            if (lower.Contains("c5_test")) return "test_chuong5";
            return lower;
        }

        private static void AddFallbackData()
        {
            _theoryMap["bai6"] = new PhysicsTheoryItem
            {
                lessonId = "bai6",
                chapterId = "chuong2",
                chapterName = "Chương II. Ánh sáng",
                lessonTitle = "Bài 6. Phản xạ toàn phần",
                keyPoints = "- Hiện tượng toàn bộ tia sáng tới bị phản xạ trở lại môi trường trong suốt ban đầu.\n- Điều kiện: 1. Truyền từ môi trường chiết quang hơn sang kém chiết quang (n1 > n2). 2. Góc tới i >= igh.",
                formulas = "sin(igh) = n2 / n1 (với n1 > n2).\nVí dụ từ nước sang không khí: sin(igh) = 1/(4/3) = 0.75 -> igh ~ 48°30'.",
                applications = "Ứng dụng trong cáp quang viễn thông internet, nội soi y tế, hiện tượng ảo ảnh và giác cắt kim cương.",
                fullText = "=== BÀI 6. PHẢN XẠ TOÀN PHẦN ===\n\n[1. KIẾN THỨC TRỌNG TÂM]\n- Hiện tượng toàn bộ tia sáng tới bị phản xạ trở lại môi trường trong suốt ban đầu.\n- Điều kiện:\n  1. Ánh sáng truyền từ môi trường chiết quang hơn sang kém chiết quang hơn (n1 > n2).\n  2. Góc tới i lớn hơn hoặc bằng góc giới hạn phản xạ toàn phần (i >= igh).\n\n[2. CÔNG THỨC]\nsin(igh) = n2 / n1\n\n[3. ỨNG DỤNG]\nCáp quang internet tốc độ cao, ống nội soi y học, giác cắt kim cương."
            };
        }
    }
}
