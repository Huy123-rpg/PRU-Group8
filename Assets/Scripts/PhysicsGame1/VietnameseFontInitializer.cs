using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Tự động nạp font tiếng Việt (VietnameseFont.ttf) từ Resources và bổ sung vào bảng Fallback của TextMeshPro
/// Đảm bảo tất cả chữ tiếng Việt có dấu (ơ, ư, đ, ả, ẽ, ĩ, õ, ũ,...) hiển thị hoàn hảo, không bị lỗi font ô vuông.
/// </summary>
public static class VietnameseFontInitializer
{
    private static TMP_FontAsset _cachedVnFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        EnsureFontLoaded();
    }

    public static TMP_FontAsset EnsureFontLoaded()
    {
        if (_cachedVnFont != null) return _cachedVnFont;

        // 1. Thử nạp font asset tạo sẵn nếu có trong Resources
        _cachedVnFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/VietnameseFont SDF");

        // 2. Nếu chưa có, nạp font TTF gốc từ FantasyUI và khởi tạo dynamic TMP_FontAsset
        if (_cachedVnFont == null)
        {
            Font source = Resources.Load<Font>("FantasyUI/VietnameseFont");
            if (source != null)
            {
                try
                {
                    _cachedVnFont = TMP_FontAsset.CreateFontAsset(
                        source, 
                        64, 
                        8,
                        UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 
                        1024, 
                        1024, 
                        AtlasPopulationMode.Dynamic, 
                        true);
                    _cachedVnFont.name = "VietnameseFontDynamic";
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[VietnameseFontInitializer] Lỗi tạo TMP_FontAsset: {ex.Message}");
                }
            }
        }

        // 3. Fallback cuối cùng nếu vẫn null: LiberationSans SDF - Fallback
        if (_cachedVnFont == null)
        {
            _cachedVnFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF - Fallback");
        }

        if (_cachedVnFont != null)
        {
            // Thêm vào fallback list của defaultFontAsset
            if (TMP_Settings.defaultFontAsset != null)
            {
                if (TMP_Settings.defaultFontAsset.fallbackFontAssetTable == null)
                {
                    TMP_Settings.defaultFontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
                }
                if (!TMP_Settings.defaultFontAsset.fallbackFontAssetTable.Contains(_cachedVnFont))
                {
                    TMP_Settings.defaultFontAsset.fallbackFontAssetTable.Insert(0, _cachedVnFont);
                }
            }

            // Thêm vào fallback list chung của TMP Settings
            if (TMP_Settings.fallbackFontAssets == null)
            {
                TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>();
            }
            if (!TMP_Settings.fallbackFontAssets.Contains(_cachedVnFont))
            {
                TMP_Settings.fallbackFontAssets.Insert(0, _cachedVnFont);
            }

            Debug.Log("<color=green>[VietnameseFontInitializer] ✅ Đã nạp thành công font tiếng Việt và gán vào Fallback của TextMeshPro!</color>");
        }

        return _cachedVnFont;
    }

    /// <summary>
    /// Gán trực tiếp font tiếng Việt cho một TextMeshProUGUI cụ thể
    /// </summary>
    public static void ApplyTo(TMP_Text text)
    {
        if (text == null) return;
        TMP_FontAsset font = EnsureFontLoaded();
        if (font != null)
        {
            text.font = font;
            if (font.material != null)
            {
                text.fontSharedMaterial = font.material;
            }
            text.SetVerticesDirty();
            text.SetLayoutDirty();
        }
    }

    /// <summary>
    /// Quét và gán font tiếng Việt cho toàn bộ TextMeshPro trong scene hiện tại
    /// </summary>
    public static void ApplyToAllInActiveScene()
    {
        TMP_FontAsset font = EnsureFontLoaded();
        if (font == null) return;

        var allTexts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        foreach (var t in allTexts)
        {
            if (t != null)
            {
                t.font = font;
                if (font.material != null)
                {
                    t.fontSharedMaterial = font.material;
                }
                t.SetVerticesDirty();
                t.SetLayoutDirty();
            }
        }
    }
}
