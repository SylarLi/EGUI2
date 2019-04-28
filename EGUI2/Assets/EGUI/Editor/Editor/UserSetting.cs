using UnityEditor;
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

        public const float DistanceComparisionTolerance = 0.01f;
        
        public static Color FrameFocusedLineColor = new Color(0f, 0.08f, 1f);

        public static float FrameFocusedLineWidth = 1f;
        
        public const int NetGridLineWidth = 1;

        public const int NetGridLineSpace = 20;

        public static Color NetGridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        
        private static GUIStyle mFrameTipsLabelStyle;
        public static GUIStyle FrameTipsLabelStyle
        {
            get
            {
                if (mFrameTipsLabelStyle == null)
                {
                    mFrameTipsLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                    mFrameTipsLabelStyle.fontSize = 30;
                    mFrameTipsLabelStyle.alignment = TextAnchor.MiddleCenter;
                    mFrameTipsLabelStyle.normal.textColor = Color.gray;
                    mFrameTipsLabelStyle.wordWrap = true;
                }

                return mFrameTipsLabelStyle;
            }
        }

        public readonly static int AnchorHash = "AnchorHash".GetHashCode();
    }
}
