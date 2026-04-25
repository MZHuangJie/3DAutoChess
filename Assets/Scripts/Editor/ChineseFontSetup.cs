#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

namespace AutoChess.Editor
{
    public class ChineseFontSetup : EditorWindow
    {
        [MenuItem("AutoChess/Setup Chinese Font")]
        public static void SetupChineseFont()
        {
            // Find system Chinese font
            string fontPath = FindChineseFont();
            if (string.IsNullOrEmpty(fontPath))
            {
                EditorUtility.DisplayDialog("Error", "Cannot find Chinese font on system.\nPlease manually import a Chinese .ttf/.otf font.", "OK");
                return;
            }

            // Copy font to project
            string destDir = "Assets/Fonts";
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            string destPath = destDir + "/ChineseFont.ttf";
            if (!File.Exists(destPath))
            {
                File.Copy(fontPath, destPath, true);
                AssetDatabase.Refresh();
            }

            // Load the font
            var font = AssetDatabase.LoadAssetAtPath<Font>(destPath);
            if (font == null)
            {
                Debug.LogError("Failed to load copied font.");
                return;
            }

            // Create TMP font asset
            string tmpFontPath = destDir + "/ChineseFont SDF.asset";
            var tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(tmpFontPath);
            if (tmpFont == null)
            {
                tmpFont = TMP_FontAsset.CreateFontAsset(font, 36, 4,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                    2048, 2048,
                    AtlasPopulationMode.Dynamic);

                if (tmpFont == null)
                {
                    Debug.LogError("Failed to create TMP font asset.");
                    return;
                }

                AssetDatabase.CreateAsset(tmpFont, tmpFontPath);

                // Save atlas texture
                if (tmpFont.atlasTexture != null)
                {
                    tmpFont.atlasTexture.name = "ChineseFont SDF Atlas";
                    AssetDatabase.AddObjectToAsset(tmpFont.atlasTexture, tmpFont);
                }
                if (tmpFont.material != null)
                {
                    tmpFont.material.name = "ChineseFont SDF Material";
                    AssetDatabase.AddObjectToAsset(tmpFont.material, tmpFont);
                }
            }

            // Set as fallback for default TMP font
            var defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                if (defaultFont.fallbackFontAssetTable == null)
                    defaultFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

                bool alreadyAdded = false;
                foreach (var f in defaultFont.fallbackFontAssetTable)
                {
                    if (f == tmpFont) { alreadyAdded = true; break; }
                }

                if (!alreadyAdded)
                {
                    defaultFont.fallbackFontAssetTable.Add(tmpFont);
                    EditorUtility.SetDirty(defaultFont);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success",
                $"Chinese font setup complete!\n\nFont: {Path.GetFileName(fontPath)}\nTMP Asset: {tmpFontPath}\n\nAdded as fallback to default TMP font.",
                "OK");
        }

        static string FindChineseFont()
        {
            string winFonts = @"C:\Windows\Fonts";
            // Priority: Microsoft YaHei > SimHei > SimSun > NSimSun
            string[] candidates = { "msyh.ttc", "msyhbd.ttc", "simhei.ttf", "simsun.ttc", "STZHONGS.TTF" };
            foreach (var name in candidates)
            {
                string path = Path.Combine(winFonts, name);
                if (File.Exists(path)) return path;
            }
            return null;
        }
    }
}
#endif
