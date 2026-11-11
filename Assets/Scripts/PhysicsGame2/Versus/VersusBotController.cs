using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    /// <summary>
    /// Điều khiển Bot AI trong Đấu trường Đối kháng 1vs1:
    /// - Tự động định vị trụ gần nhất chưa bị chiếm trong 10 trụ thật
    /// - Di chuyển mượt mà tới vị trí trụ
    /// - Mô phỏng thời gian suy nghĩ hợp lý (6s - 10s)
    /// - Tỷ lệ trả lời đúng kiểm soát (~65%)
    /// - Tuân thủ 100% quy tắc chiếm trụ và tính điểm như người thật
    /// </summary>
    public class VersusBotController : MonoBehaviour
    {
        [Header("=== BOT CONFIG ===")]
        [SerializeField] private float moveSpeed = 310f;
        [SerializeField] private float arrivalDistance = 60f;
        [SerializeField] private float minThinkTime = 6.0f;
        [SerializeField] private float maxThinkTime = 10.0f;
        [SerializeField] private float accuracy = 0.65f; // 65% tỉ lệ đúng

        private RectTransform _rectTransform;
        private VersusTowerController _targetTower;
        private bool _isThinking = false;
        private Coroutine _thinkCoroutine;
        private readonly Dictionary<int, float> _cooldownTowers = new Dictionary<int, float>();

        public Vector2 Position => _rectTransform != null ? _rectTransform.anchoredPosition : Vector2.zero;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (VersusArenaController.Instance == null || VersusArenaController.Instance.IsMatchEnded)
            {
                if (_thinkCoroutine != null) StopCoroutine(_thinkCoroutine);
                return;
            }

            if (_isThinking) return;

            // Cập nhật cooldown các trụ bot đã trả lời sai
            UpdateCooldowns();

            // Nếu chưa có mục tiêu hoặc bot đã giải đúng mục tiêu này rồi, tìm trụ mới mà bot chưa giải
            if (_targetTower == null || _targetTower.opponentSolved)
            {
                _targetTower = FindBestTargetTower();
            }

            if (_targetTower != null)
            {
                MoveTowardsTarget();
            }
        }

        private void UpdateCooldowns()
        {
            var keys = new List<int>(_cooldownTowers.Keys);
            foreach (var k in keys)
            {
                _cooldownTowers[k] -= Time.deltaTime;
                if (_cooldownTowers[k] <= 0f)
                {
                    _cooldownTowers.Remove(k);
                }
            }
        }

        private VersusTowerController FindBestTargetTower()
        {
            var towers = VersusArenaController.Instance.Towers;
            if (towers == null || towers.Length == 0) return null;

            VersusTowerController best = null;
            float minDst = float.MaxValue;
            Vector2 botPos = Position;

            foreach (var t in towers)
            {
                if (t == null || t.opponentSolved) continue;
                if (_cooldownTowers.ContainsKey(t.towerNumber)) continue;

                float dst = Vector2.Distance(botPos, t.Position);
                if (dst < minDst)
                {
                    minDst = dst;
                    best = t;
                }
            }

            // Nếu mọi trụ bot chưa giải đều trong cooldown, chọn trụ bot chưa giải gần nhất
            if (best == null)
            {
                foreach (var t in towers)
                {
                    if (t == null || t.opponentSolved) continue;
                    float dst = Vector2.Distance(botPos, t.Position);
                    if (dst < minDst)
                    {
                        minDst = dst;
                        best = t;
                    }
                }
            }

            return best;
        }

        private void MoveTowardsTarget()
        {
            if (_rectTransform == null || _targetTower == null) return;

            Vector2 targetPos = _targetTower.Position;
            Vector2 currentPos = _rectTransform.anchoredPosition;

            float dst = Vector2.Distance(currentPos, targetPos);
            if (dst <= arrivalDistance)
            {
                // Đã đến trụ -> Bắt đầu suy nghĩ trả lời
                _thinkCoroutine = StartCoroutine(ThinkAndAnswerRoutine(_targetTower));
            }
            else
            {
                Vector2 dir = (targetPos - currentPos).normalized;
                _rectTransform.anchoredPosition = currentPos + dir * (moveSpeed * Time.deltaTime);
            }
        }

        private IEnumerator ThinkAndAnswerRoutine(VersusTowerController tower)
        {
            _isThinking = true;
            float thinkTime = UnityEngine.Random.Range(minThinkTime, maxThinkTime);
            yield return new WaitForSecondsRealtime(thinkTime);

            // Kiểm tra xem bot đã hoàn thành trụ này chưa và trận đấu còn tiếp diễn không
            if (tower != null && !tower.opponentSolved && VersusArenaController.Instance != null && !VersusArenaController.Instance.IsMatchEnded)
            {
                bool isCorrect = UnityEngine.Random.value < accuracy;
                if (isCorrect)
                {
                    // Bot trả lời đúng -> Cộng điểm và bật vòng sáng vàng của bot
                    VersusArenaController.Instance.SolveTower(tower.towerNumber, isPlayer: false);
                    _targetTower = null;
                }
                else
                {
                    // Bot trả lời sai -> Trụ vẫn mở, Bot tạm thời rời đi tìm trụ khác trong 5 giây rồi thử lại
                    _cooldownTowers[tower.towerNumber] = 5f;
                    _targetTower = null;
                }
            }

            _isThinking = false;
        }
    }
}
