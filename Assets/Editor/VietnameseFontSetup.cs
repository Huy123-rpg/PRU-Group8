using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>Build a portable Vietnamese TMP fallback from the font already in the project.</summary>
[InitializeOnLoad]
public static class VietnameseFontSetup
{
    private const string FontPath = "Assets/SinhHoc/Resources/FantasyUI/VietnameseFont.ttf";
    private const string AssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Vietnamese SDF.asset";
    private const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    static VietnameseFontSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            if (font == null || TMP_Settings.fallbackFontAssets == null || !TMP_Settings.fallbackFontAssets.Contains(font))
                Configure();
        };
    }

    [MenuItem("Tools/PRU/Fix Vietnamese TMP Fonts")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Thoát Play Mode trước khi tạo font.");
        Font source = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (source == null) throw new InvalidOperationException("Không tìm thấy font tiếng Việt: " + FontPath);
        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (fallback == null)
        {
            fallback = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA,
                1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (fallback == null) throw new InvalidOperationException("Không tạo được font SDF tiếng Việt.");
            fallback.name = "Vietnamese SDF";
            AssetDatabase.CreateAsset(fallback, AssetPath);
        }
        fallback.isMultiAtlasTexturesEnabled = true;
        string characters = VietnameseCharacters();
        if (!fallback.HasCharacters(characters))
        {
            fallback.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            if (!fallback.TryAddCharacters(characters, out string missing))
                throw new InvalidOperationException("Font nguồn thiếu ký tự tiếng Việt: " + missing);
        }
        // All Vietnamese glyphs are baked: no runtime font-face loading is needed.
        fallback.atlasPopulationMode = AtlasPopulationMode.Static;

        if (!AssetDatabase.Contains(fallback.material)) AssetDatabase.AddObjectToAsset(fallback.material, fallback);
        foreach (Texture2D atlas in fallback.atlasTextures)
        {
            if (atlas == null) continue;
            if (!AssetDatabase.Contains(atlas)) AssetDatabase.AddObjectToAsset(atlas, fallback);
            EditorUtility.SetDirty(atlas);
        }
        // Keep the verified atlas in builds as well as its dynamic source font.
        var serialized = new SerializedObject(fallback);
        serialized.FindProperty("m_ClearDynamicDataOnBuild").boolValue = false;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        string[] baseFonts = {
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset",
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset"
        };
        foreach (string path in baseFonts)
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null || font == fallback) continue;
            if (font.fallbackFontAssetTable == null) font.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (!font.fallbackFontAssetTable.Contains(fallback)) font.fallbackFontAssetTable.Add(fallback);
            EditorUtility.SetDirty(font);
        }
        if (TMP_Settings.fallbackFontAssets == null) TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>();
        if (!TMP_Settings.fallbackFontAssets.Contains(fallback)) TMP_Settings.fallbackFontAssets.Add(fallback);
        EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<TMP_Settings>(SettingsPath));
        EditorUtility.SetDirty(fallback);
        AssetDatabase.SaveAssets();
        Debug.Log("[VietnameseFontSetup] Đã tạo và kiểm tra đầy đủ ký tự tiếng Việt, đăng ký fallback cho LiberationSans và TMP Settings.");
    }

    private static string VietnameseCharacters()
    {
        var text = new StringBuilder("ĂăÂâĐđÊêÔôƠơƯưÀÁÈÉÌÍÒÓÙÚÝàáèéìíòóùúý");
        for (int code = 0x20; code <= 0x7e; code++) text.Append((char)code);
        for (int code = 0x1ea0; code <= 0x1ef9; code++) text.Append((char)code);
        return text.ToString();
    }

    public static string Verify()
    {
        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (fallback == null || !fallback.HasCharacters(VietnameseCharacters()))
            throw new InvalidOperationException("Vietnamese SDF chưa đủ ký tự.");
        TMP_FontAsset primary = TMP_Settings.defaultFontAsset;
        if (primary == null || !primary.HasCharacters("Chọn Môn Học · Sinh Học · Hóa Học · Vật Lý", out uint[] missing, true, true))
            throw new InvalidOperationException("Font mặc định chưa tìm thấy đủ glyph qua fallback.");
        return "PASS: Vietnamese glyph coverage and default TMP fallback";
    }
}
