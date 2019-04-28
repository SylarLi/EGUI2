using System;
using UnityEngine;

namespace EGUI.UI
{
    /// <summary>
    /// 由于Clip只支持矩形裁剪，Mask下的节点一旦发生旋转将会导致显示错误
    /// 精确到像素点的裁剪暂不支持
    /// </summary>
    public class MaskableGraphic : Graphic
    {
        private RectMask2D mMask;

        private RectMask2D mask
        {
            get
            {
                if (mMask == null) mMask = GetLeaf<RectMask2D>();
                if (mMask == null) mMask = GetLeafInAncestors<RectMask2D>();
                return mMask;
            }
        }

        protected override void DrawContent(GUIContent content, bool isOn = false, bool isHover = false, bool isActive = false,
            bool hasKeyboardFocus = false)
        {
            base.DrawContent(content, isOn, isHover, isActive, hasKeyboardFocus);
            var m = mask;
            drawer.clipping = m != null;
            if (m != null)
            {
                drawer.clipRect = BuildOverlappedLocalRect(m.node);
                drawer.guiRect = new Rect(drawer.guiRect.min - drawer.clipRect.min, drawer.guiRect.size);
            }
        }

        protected override void DrawProcess(Action process)
        {
            base.DrawProcess(process);
            var m = mask;
            drawer.clipping = m != null;
            if (m != null)
            {
                drawer.clipRect = BuildOverlappedLocalRect(m.node);
                drawer.guiRect = new Rect(drawer.guiRect.min - drawer.clipRect.min, drawer.guiRect.size);
            }
        }
        
        private Rect BuildOverlappedLocalRect(Node other)
        {
            var matrix = node.world2LocalMatrix * other.local2WorldMatrix;
            var min = matrix.MultiplyPoint(other.localRect.min);
            var max = matrix.MultiplyPoint(other.localRect.max);
            var local = new Rect(min, max - min);
            Rect overlaps;
            node.localRect.Intersects(local, out overlaps);
            return overlaps;
        }
    }
}