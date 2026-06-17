using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VertigoWheel.EditorTools
{
    /// <summary>
    /// Enforces correct, consistent import settings on every UI texture (TextureType = Sprite, no
    /// mipmaps, FullRect mesh) and assigns 9-slice borders to the frames/panels/buttons that the brief
    /// requires to be drawn as Sliced sprites. Runs automatically on (re)import so artists never have to
    /// touch the importer by hand.
    /// </summary>
    public sealed class UiSpriteImporter : AssetPostprocessor
    {
        private const string UiFolder = "Assets/Game/Art/UI/";

        // Per-file 9-slice borders (left, bottom, right, top). Anything not listed stays unsliced.
        private static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            { "UI_button_grey_standard", new Vector4(36, 30, 36, 36) },
            { "UI_button_orange_standard", new Vector4(36, 30, 36, 36) },
            { "ui_card_panel_zone_bg", new Vector4(20, 20, 20, 20) },
            { "ui_card_panel_zone_current", new Vector4(20, 20, 20, 20) },
            { "ui_card_panel_zone_super", new Vector4(20, 20, 20, 20) },
            { "ui_card_panel_zone_white", new Vector4(20, 20, 20, 20) },
            { "ui_card_frame_12px_neutral", new Vector4(24, 24, 24, 24) },
            { "ui_card_frame_4px_zone", new Vector4(8, 8, 8, 8) },
            { "ui_card_zone_map_frame", new Vector4(16, 16, 16, 16) },
        };

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UiFolder)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Compressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // recommended for UI
            settings.spriteGenerateFallbackPhysicsShape = false;

            string file = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            settings.spriteBorder = Borders.TryGetValue(file, out var border) ? border : Vector4.zero;

            importer.SetTextureSettings(settings);
        }
    }
}
