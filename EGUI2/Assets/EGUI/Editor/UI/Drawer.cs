using System;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    internal class Drawer : Leaf
    {
        public GUIStyle guiStyle = GUIStyle.none;

        public Matrix4x4 guiMatrix = Matrix4x4.identity;

        public Rect guiRect = Rect.zero;

        public GUIContent guiContent = GUIContent.none;

        public Color color = Color.white;

        public bool isOn = false;

        public bool isHover = false;

        public bool isActive = false;

        public bool hasKeyboardFocus = false;

        public Action drawProxy = null;

        public void Draw()
        {
            if (drawProxy != null)
            {
                drawProxy();
            }
            else
            {
                Debug.Assert(guiStyle != null, "style can not be null.");
                var rawMatrix = GUI.matrix;
                var rawColor = GUI.color;
                GUI.matrix = guiMatrix;
                GUI.color = color;
                guiStyle.Draw(guiRect, guiContent, isHover, isActive, isOn, hasKeyboardFocus);
                GUI.color = rawColor;
                GUI.matrix = rawMatrix;
            }
        }
    }
}
