using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace PRU.Biology
{
    public static class FantasyUIFont
    {
        private static TMP_FontAsset cachedFont;
        private static bool warned;

        public static void Apply(TMP_Text text)
        {
            if (cachedFont == null)
            {
                Font source = Resources.Load<Font>("FantasyUI/VietnameseFont");
                if (source != null)
                {
                    cachedFont = TMP_FontAsset.CreateFontAsset(source, 64, 8,
                        GlyphRenderMode.SDFAA, 1024, 1024,
                        AtlasPopulationMode.Dynamic, true);
                    cachedFont.name = "FantasyVietnameseDynamic";
                }
                else if (!warned)
                {
                    warned = true;
                    Debug.LogWarning("Thiếu Resources/FantasyUI/VietnameseFont.ttf; đang dùng font cũ.");
                }
            }

            if (cachedFont != null)
            {
                text.font = cachedFont;
                text.fontSharedMaterial = cachedFont.material;
            }
            text.fontStyle = FontStyles.Normal;
            text.raycastTarget = false;
        }
    }
}
