using System;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class Drawer : Leaf
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

        public bool clipping = false;

        public Rect clipRect = Rect.zero;

        public void Draw()
        {
            var rawMatrix = GUI.matrix;
            var rawColor = GUI.color;
            GUI.matrix *= guiMatrix;
            GUI.color *= color;
            if (clipping)
                GUI.BeginClip(clipRect);

            if (drawProxy != null)
            {
                drawProxy();
            }
            else
            {
                Debug.Assert(guiStyle != null, "GUI style can not be null.");
                guiStyle.Draw(guiRect, guiContent, isHover, isActive, isOn,
                    hasKeyboardFocus);
            }

            if (clipping)
                GUI.EndClip();
            GUI.color = rawColor;
            GUI.matrix = rawMatrix;
        }
    }
}