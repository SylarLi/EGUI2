using System.IO;
using UnityEngine;
using UnityEditor;

namespace EGUI
{
    public sealed class DefaultResource
    {
        public const string ResourcePath = "Assets/EGUI/Editor/Core/Resource";
        public static readonly string DefaultImageSprite = Path.Combine(ResourcePath, "blank.png");
        public static readonly string DefaultButtonSprite = Path.Combine(ResourcePath, "button.png");
        public static readonly string DefaultButtonActiveSprite = Path.Combine(ResourcePath, "button_active.png");
        public static readonly string DefaultToggleSprite = Path.Combine(ResourcePath, "toggle.png");
        public static readonly string DefaultToggleActiveSprite = Path.Combine(ResourcePath, "toggle_active.png");
        public static readonly string DefaultToggleCheckmarkSprite = Path.Combine(ResourcePath, "toggle_checkmark.png");
        public static readonly string DefaultScrollbarBGSprite = Path.Combine(ResourcePath, "scrollbar_bg.png");
        public static readonly string DefaultScrollbarThumbSprite = Path.Combine(ResourcePath, "scrollbar_thumb.png");
        public static readonly string LinkCursorTex = Path.Combine(ResourcePath, "cursor_aero_link.png");
        public static readonly string HelpCursorTex = Path.Combine(ResourcePath, "cursor_aero_help.png");
        public static readonly string VerticalCursorTex = Path.Combine(ResourcePath, "cursor_aero_ns.png");
        public static readonly string HorizontalCursorTex = Path.Combine(ResourcePath, "cursor_aero_ew.png");
        public static readonly string Diagonal1CursorTex = Path.Combine(ResourcePath, "cursor_aero_nesw.png");
        public static readonly string Diagonal2CursorTex = Path.Combine(ResourcePath, "cursor_aero_nwse.png");

        private static Sprite mBlankSprite;
        public static Sprite GetBlankSprite()
        {
            mBlankSprite = mBlankSprite ? mBlankSprite : AssetDatabase.LoadAssetAtPath<Sprite>(DefaultImageSprite);
            return mBlankSprite;
        }
    }
}
