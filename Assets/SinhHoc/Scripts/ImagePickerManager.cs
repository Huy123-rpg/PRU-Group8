// ============================================================
// ImagePickerManager.cs
// Lấy ảnh bài làm TỰ LUẬN (#12, #13):
//   • "CHỤP ẢNH"  : Editor = chụp GameView (test dễ); Mobile = camera (tích hợp sau)
//   • "TẢI ẢNH"   : Editor/PC = hộp thoại chọn file; Mobile = gallery (tích hợp sau)
//
// Ảnh sau khi chọn được:
//   1. Kiểm tra dung lượng (IMAGE_TOO_LARGE nếu > maxRawBytes)
//   2. Downscale về tối đa maxImageSize px cạnh dài
//   3. Nén JPEG quality 75 → base64 (gửi backend được ngay)
//
// Trả về qua callback: success, base64, Sprite preview (hiện Image Preview #13).
// Không phụ thuộc plugin ngoài → build không lỗi; chổ tích hợp NativeCamera/
// NativeGallery trên mobile được đánh dấu TODO rõ ràng.
// ============================================================
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace PRU.Biology
{
    public class ImagePickerManager : MonoBehaviour
    {
        public static ImagePickerManager Instance { get; private set; }

        [Header("Cấu hình ảnh gửi AI")]
        [Tooltip("Cạnh dài tối đa (px) sau khi downscale")]
        public int maxImageSize = 1280;
        [Tooltip("Chất lượng JPEG 1-100")]
        [Range(1, 100)] public int jpegQuality = 75;
        [Tooltip("Dung lượng file gốc tối đa (MB) - vượt quá báo lỗi ảnh quá lớn")]
        public float maxRawBytesMB = 10f;

        public static ImagePickerManager EnsureInstance()
        {
            if (Instance != null) return Instance;
            GameObject go = new GameObject("ImagePickerManager");
            return go.AddComponent<ImagePickerManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Kết quả chọn ảnh: success=false khi hủy hoặc có lỗi kỹ thuật.</summary>
        public void PickImage(bool captureFromCamera, Action<bool, string, Sprite> onResult)
        {
            StartCoroutine(PickImageCo(captureFromCamera, onResult));
        }

        private IEnumerator PickImageCo(bool capture, Action<bool, string, Sprite> onResult)
        {
#if UNITY_EDITOR
            string path = capture ? null : EditorFilePicker();
            if (!capture && string.IsNullOrEmpty(path))
            {
                onResult?.Invoke(false, null, null); // user hủy hộp thoại
                yield break;
            }

            string screenshotPath = null;
            if (capture)
            {
                // CHỤP ẢNH trong Editor: chụp GameView để test luồng nhanh không cần ảnh thật
                screenshotPath = Path.Combine(Application.temporaryCachePath, "essay_capture.png");
                ScreenCapture.CaptureScreenshot(screenshotPath);
                // CaptureScreenshot là bất đồng bộ - chờ vài frame cho file ghi xong
                for (int i = 0; i < 10 && !File.Exists(screenshotPath); i++) yield return null;
                yield return null;
                path = File.Exists(screenshotPath) ? screenshotPath : null;
                if (path == null)
                {
                    Debug.LogWarning("[ImagePicker] Không chụp được GameView - thử lại.");
                    onResult?.Invoke(false, null, null);
                    yield break;
                }
            }

            byte[] raw = File.ReadAllBytes(path);
            if (capture) try { File.Delete(screenshotPath); } catch { }
            FinishWithRawImage(raw, onResult);
#else
            // TODO MOBILE (bước tích hợp sau, không ảnh hưởng build hiện tại):
            //  - capture = true  → NativeCamera.TakePicture(...)
            //  - capture = false → NativeGallery.GetImageFromGallery(...)
            // Cài plugin NativeCamera/NativeGallery rồi thay khối dưới.
            Debug.LogWarning("[ImagePicker] Platform này chưa hỗ trợ chọn ảnh. " +
                             "Chạy trong Unity Editor để test, hoặc tích hợp NativeCamera/NativeGallery.");
            onResult?.Invoke(false, null, null);
            yield break;
#endif
        }

#if UNITY_EDITOR
        private string EditorFilePicker()
        {
            return UnityEditor.EditorUtility.OpenFilePanel(
                "Chọn ảnh bài làm", "", "png,jpg,jpeg");
        }
#endif

        /// <summary>Chuyển bytes ảnh gốc → downscale + nén JPEG + base64.</summary>
        private void FinishWithRawImage(byte[] raw, Action<bool, string, Sprite> onResult)
        {
            float maxMb = maxRawBytesMB;
            if (raw == null || raw.Length == 0)
            {
                Debug.LogWarning("[ImagePicker] Ảnh rỗng.");
                onResult?.Invoke(false, null, null);
                return;
            }
            if (raw.Length > maxMb * 1024f * 1024f)
            {
                Debug.LogWarning($"[ImagePicker] Ảnh quá lớn ({raw.Length / (1024f * 1024f):0.0} MB > {maxMb:0} MB).");
                onResult?.Invoke(false, "TOO_LARGE", null); // false + mã TOO_LARGE để UI hiện đúng lỗi ảnh quá lớn
                return;
            }

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(raw))
            {
                Debug.LogWarning("[ImagePicker] Không đọc được file ảnh (không phải PNG/JPG).");
                UnityEngine.Object.Destroy(tex);
                onResult?.Invoke(false, null, null);
                return;
            }

            // ----- Downscale giữ tỷ lệ -----
            int w = tex.width, h = tex.height;
            float scale = Mathf.Min(1f, (float)maxImageSize / Mathf.Max(w, h));
            int nw = Mathf.Max(1, Mathf.RoundToInt(w * scale));
            int nh = Mathf.Max(1, Mathf.RoundToInt(h * scale));

            Texture2D resized = tex;
            if (scale < 1f)
            {
                RenderTexture rt = RenderTexture.GetTemporary(nw, nh, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(tex, rt);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                resized = new Texture2D(nw, nh, TextureFormat.RGBA32, false);
                resized.ReadPixels(new Rect(0, 0, nw, nh), 0, 0);
                resized.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.Destroy(tex);
            }

            // ----- Nén JPEG → base64 -----
            byte[] jpg = resized.EncodeToJPG(jpegQuality);
            string base64 = Convert.ToBase64String(jpg);
            Debug.Log($"[ImagePicker] Ảnh {w}x{h} → {nw}x{nh}, JPEG {jpg.Length / 1024f:0} KB, base64 {base64.Length} ký tự.");

            // Sprite preview (Pivot giữa để dễ hiển thị trong Image)
            Sprite sp = Sprite.Create(resized, new Rect(0, 0, resized.width, resized.height),
                                      new Vector2(0.5f, 0.5f), 100f);
            sp.name = "EssayPreview";
            onResult?.Invoke(true, base64, sp);
        }
    }
}
