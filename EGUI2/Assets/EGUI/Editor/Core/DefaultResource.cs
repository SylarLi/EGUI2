using System.IO;
using UnityEngine;
using UnityEditor;

namespace EGUI
{
    public sealed class DefaultResource
    {
        public const string ResourcePath = "Assets/EGUI/Editor/Resource";
        public static string DefaultImageSprite = Path.Combine(ResourcePath, "blank.png");
        public static string DefaultToggleCheckmarkSprite = Path.Combine(ResourcePath, "toggle_checkmark.png");
        public static string LinkCursorTex = Path.Combine(ResourcePath, "cursor_aero_link.png");
        public static string HelpCursorTex = Path.Combine(ResourcePath, "cursor_aero_help.png");
        public static string VerticalCursorTex = Path.Combine(ResourcePath, "cursor_aero_ns.png");
        public static string HorizontalCursorTex = Path.Combine(ResourcePath, "cursor_aero_ew.png");
        public static string Diagonal1CursorTex = Path.Combine(ResourcePath, "cursor_aero_nesw.png");
        public static string Diagonal2CursorTex = Path.Combine(ResourcePath, "cursor_aero_nwse.png");

        private static Sprite mBlankSprite;
        public static Sprite GetBlankSprite()
        {
            mBlankSprite = mBlankSprite ?? AssetDatabase.LoadAssetAtPath<Sprite>(DefaultImageSprite);
            return mBlankSprite;
        }

        private static Sprite mToggleCheckmarkSprite;
        public static Sprite GetToggleCheckmarkSprite()
        {
            mToggleCheckmarkSprite = mToggleCheckmarkSprite ?? AssetDatabase.LoadAssetAtPath<Sprite>(DefaultToggleCheckmarkSprite);
            return mToggleCheckmarkSprite;
        }
    }
}
