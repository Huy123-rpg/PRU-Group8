using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2
{
    public enum VersusGameMode
    {
        Bot,           // Đấu với máy (100% offline)
        Invite,        // Mời người chơi (10s countdown)
        RandomMatch,   // Ghép ngẫu nhiên
        OnlineHost,    // Chủ phòng Online (Máy 1)
        OnlineClient   // Khách tham gia phòng Online (Máy 2)
    }

    public enum VersusTowerClaim
    {
        Unclaimed = 0,
        Player = 1,    // Xanh
        Opponent = 2   // Vàng
    }

    [Serializable]
    public class VersusTowerProgress
    {
        public int towerNumber; // 1 -> 10
        public PhysicsQuestionRaw question;
        public bool playerSolved = false;
        public bool opponentSolved = false;
        public float playerSolvedTime = 0f;
        public float opponentSolvedTime = 0f;
        public Vector2 arenaPosition;
    }

    [Serializable]
    public class VersusMatchConfig
    {
        public string matchId = "";
        public VersusGameMode mode = VersusGameMode.Bot;
        public int chapterId = 1;
        public string lessonId = "bai2";
        public string lessonTitle = "Bài 2. Động năng. Thế năng";
        public string playerCharacter = "Minh";
        public string playerName = "Bạn";
        public string opponentCharacter = "Duy";
        public string opponentName = "Đối thủ";
        public bool isHost = true;
        public int questionSeed = 12345;
        public string roomCode = "";
        public List<string> matchQuestionIDs = new List<string>();
        public bool isOnline => mode == VersusGameMode.OnlineHost || mode == VersusGameMode.OnlineClient;
    }

    [Serializable]
    public class VersusMatchResult
    {
        public string matchId = "";
        public VersusGameMode mode = VersusGameMode.Bot;
        public int playerScore = 0;
        public int opponentScore = 0;
        public int playerTowers = 0; // Số câu đúng của Player (0 -> 10)
        public int opponentTowers = 0; // Số câu đúng của Đối thủ (0 -> 10)
        public int playerCorrectCount = 0;
        public int opponentCorrectCount = 0;
        public string playerCharacter = "Minh";
        public string opponentCharacter = "Duy";
        public string opponentName = "Đối thủ";
        public Sprite playerAvatarSprite = null; // Snapshot sprite avatar Bạn từ đấu trường
        public Sprite opponentAvatarSprite = null; // Snapshot sprite avatar Đối thủ từ đấu trường
        public bool isWinner = false; // Player thắng
        public bool isOpponentWinner = false; // Bot/Đối thủ thắng
        public bool isDraw = false; // Hòa
        public float durationSeconds = 0f;
        public string finishedTimestamp = "";
    }

    /// <summary>
    /// Lưu trữ phiên trạng thái đối kháng giữa các màn hình (Lobby -> Arena -> Result).
    /// </summary>
    public static class VersusSessionState
    {
        private static VersusMatchConfig _activeConfig;
        private static VersusMatchResult _lastResult;

        public static VersusMatchConfig ActiveConfig
        {
            get
            {
                if (_activeConfig == null)
                {
                    _activeConfig = CreateDefaultConfig();
                }
                return _activeConfig;
            }
            set => _activeConfig = value;
        }

        public static VersusMatchResult LastResult
        {
            get => _lastResult;
            set => _lastResult = value;
        }

        public static VersusMatchConfig CreateDefaultConfig()
        {
            var cfg = new VersusMatchConfig();
            cfg.matchId = "VS_" + DateTime.UtcNow.Ticks.ToString().Substring(10);
            cfg.mode = VersusGameMode.Bot;

            if (NavigationService.Instance != null && NavigationService.Instance.Selection != null)
            {
                cfg.chapterId = NavigationService.Instance.Selection.ChapterId > 0 ? NavigationService.Instance.Selection.ChapterId : 1;
                cfg.lessonId = !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonId) ? NavigationService.Instance.Selection.LessonId : "bai2";
                cfg.lessonTitle = !string.IsNullOrEmpty(NavigationService.Instance.Selection.LessonTitle) ? NavigationService.Instance.Selection.LessonTitle : "Bài 2. Động năng. Thế năng";
            }

            var save = PhysicsSaveManager.Load();
            if (save != null && !string.IsNullOrEmpty(save.selectedCharacterId))
            {
                cfg.playerCharacter = save.selectedCharacterId;
            }
            else
            {
                cfg.playerCharacter = "Minh";
            }

            // Chọn nhân vật đối thủ khác với người chơi
            string[] opponents = new string[] { "Minh", "Lan", "Duy", "Hà" };
            foreach (var op in opponents)
            {
                if (!string.Equals(op, cfg.playerCharacter, StringComparison.OrdinalIgnoreCase))
                {
                    cfg.opponentCharacter = op;
                    cfg.opponentName = op;
                    break;
                }
            }

            return cfg;
        }

        public static void CreateFreshMatchSession(string modeStr = "Bot")
        {
            _activeConfig = CreateDefaultConfig();
            if (Enum.TryParse<VersusGameMode>(modeStr, true, out var m))
            {
                _activeConfig.mode = m;
            }
            _activeConfig.matchId = "VS_" + DateTime.UtcNow.Ticks.ToString();
            _activeConfig.questionSeed = UnityEngine.Random.Range(1000, 99999);
        }

        public static void ClearSession()
        {
            _activeConfig = null;
            _lastResult = null;
        }
    }
}
