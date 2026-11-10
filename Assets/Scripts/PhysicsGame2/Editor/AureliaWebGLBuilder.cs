using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ScienceQuest.PhysicsGame2.Editor
{
    public static class AureliaWebGLBuilder
    {
        public const string BUILD_OUTPUT_DIR = "Build_WebGL/Aurelia_Game2_WebGL";
        public const string ZIP_OUTPUT_PATH = "Build_WebGL/Aurelia_Game2_itch_io.zip";

        [MenuItem("Aurelia/Build WebGL (For itch.io / Web)")]
        public static void BuildWebGLMenu()
        {
            BuildWebGLExport();
        }

        public static bool BuildWebGLExport()
        {
            Debug.Log("[AureliaWebGLBuilder] === BẮT ĐẦU TIẾN TRÌNH BUILD WEBGL AURELIA GAME 2 ===");

            // 1. Chuyển đổi Build Target sang WebGL
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                Debug.Log("[AureliaWebGLBuilder] Đang chuyển Active Build Target sang WebGL...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            // 2. Cấu hình tối ưu tương thích cho WebGL & itch.io
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled; // Tránh lỗi MIME type Gzip/Brotli trên máy chủ web
            PlayerSettings.runInBackground = true;

            // 3. Lấy và chuẩn hóa danh sách Scene hợp lệ thực tế trên ổ đĩa
            string projectRoot = Directory.GetCurrentDirectory();
            var validScenesList = new System.Collections.Generic.List<string>();

            // Luôn đặt Main Menu của Aurelia Game 2 làm Scene số 0 để khi mở WebGL vào thẳng game
            string game2MainMenu = "Assets/Scenes/Physics/Game2/01_MainMenu.unity";
            if (File.Exists(Path.Combine(projectRoot, game2MainMenu)))
            {
                validScenesList.Add(game2MainMenu);
            }

            // Thêm toàn bộ các scene thuộc Game 2 thực tế có trên ổ đĩa
            string game2Dir = Path.Combine(projectRoot, "Assets/Scenes/Physics/Game2");
            if (Directory.Exists(game2Dir))
            {
                foreach (string file in Directory.GetFiles(game2Dir, "*.unity", SearchOption.TopDirectoryOnly))
                {
                    string relPath = file.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
                    if (!validScenesList.Contains(relPath))
                    {
                        validScenesList.Add(relPath);
                    }
                }
            }

            // Bổ sung các Scene hợp lệ khác từ EditorBuildSettings (Game 1, Sinh học, Hóa học, Login...)
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s == null || !s.enabled || string.IsNullOrEmpty(s.path)) continue;
                string cleanPath = s.path.Replace('\\', '/');
                if (File.Exists(Path.Combine(projectRoot, cleanPath)) && !validScenesList.Contains(cleanPath))
                {
                    validScenesList.Add(cleanPath);
                }
            }

            string[] scenes = validScenesList.ToArray();

            // Đồng bộ lại EditorBuildSettings với danh sách scene sạch thực tế
            EditorBuildSettings.scenes = scenes.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            Debug.Log($"[AureliaWebGLBuilder] Tổng số scene đưa vào bản build: {scenes.Length}. Scene khởi đầu: {scenes.FirstOrDefault()}");

            // Đảm bảo thư mục đích sạch sẽ
            string outputFullPath = Path.Combine(projectRoot, BUILD_OUTPUT_DIR);

            if (Directory.Exists(outputFullPath))
            {
                try
                {
                    Directory.Delete(outputFullPath, true);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AureliaWebGLBuilder] Không thể xóa thư mục cũ: {ex.Message}");
                }
            }

            Directory.CreateDirectory(outputFullPath);

            // 4. Thực thi Build Player
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputFullPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AureliaWebGLBuilder] BUILD WEBGL THÀNH CÔNG! Dung lượng: {summary.totalSize / (1024 * 1024):F2} MB. Thời gian: {summary.totalTime.TotalSeconds:F1}s.");

                // 5. Đóng gói file ZIP sẵn sàng upload lên itch.io
                PackageItchIoZip(outputFullPath, Path.Combine(projectRoot, ZIP_OUTPUT_PATH));
                return true;
            }
            else
            {
                Debug.LogError($"[AureliaWebGLBuilder] BUILD WEBGL THẤT BẠI! Lỗi ({summary.totalErrors}):");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($" - {msg.content}");
                        }
                    }
                }
                return false;
            }
        }

        public static void PackageItchIoZip(string sourceDir, string zipPath)
        {
            try
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                string zipDir = Path.GetDirectoryName(zipPath);
                if (!Directory.Exists(zipDir)) Directory.CreateDirectory(zipDir);

                // Nén toàn bộ nội dung thư mục build sao cho index.html nằm ngay gốc file zip
                ZipFile.CreateFromDirectory(sourceDir, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);
                Debug.Log($"[AureliaWebGLBuilder] ĐÃ TẠO FILE ZIP ITCH.IO: {zipPath} ({new FileInfo(zipPath).Length / (1024 * 1024):F2} MB)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AureliaWebGLBuilder] Lỗi khi tạo file zip: {ex.Message}");
            }
        }
    }
}
