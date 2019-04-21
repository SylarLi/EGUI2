using UnityEngine;

namespace EGUI.Editor
{
    internal sealed class UserSetting
    {
        public static Color FrameBackgroundColor = new Color32(56, 56, 56, 255);

        public static Color FrameContainerBackgroundColor = new Color32(41, 41, 41, 255);

        public static Color HierarchySelectedFocusedColor = new Color32(62, 95, 150, 255);

        public static Color HierarchySelectedColor = new Color32(72, 72, 72, 255);

        public static Color HierarchyDragTipsColor = Color.white;

        public static int FrameMenuBarHeight = 18;

        public const int FrameIntervalSpace = 5;

        public static Color SceneNodeSelectionColor = new Color(1, 0.4f, 0);

        public const float SceneNodeSelectionLineWidth = 2;
    }
}
