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
