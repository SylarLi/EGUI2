using System;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    [RequireLeaf(typeof(Drawer))]
    public abstract class Graphic : Leaf, IContentSizeFitable
    {
        private Canvas mCanvas;

        protected Canvas canvas { get { if (mCanvas == null) mCanvas = GetLeafInAncestors<Canvas>(); return mCanvas; } }

        private Drawer mDrawer;

        internal Drawer drawer { get { mDrawer = mDrawer ?? GetLeaf<Drawer>(); return mDrawer; } }

        [PersistentField]
        private Color mColor = Color.white;

        public Color color { get { return mColor; } set { mColor = value; } }

        [PersistentField]
        private Color mTintColor = Color.white;

        public Color tintColor { get { return mTintColor; } set { mTintColor = value; } }

        [PersistentField]
        private bool mRaycastTarget = true;

        public bool raycastTarget { get { return mRaycastTarget; } set { mRaycastTarget = value; } }

        protected GUIStyle mStyle;

        /// <summary>
        /// GUI style for drawing.
        /// </summary>
        public virtual GUIStyle style { get { return mStyle; } set { throw new NotImplementedException(); } }

        private bool mStyleDirty = true;

        public override void Update()
        {
            if (mStyleDirty)
            {
                RebuildStyle();
                mStyleDirty = false;
            }
            drawer.guiMatrix = node.local2WorldMatrix;
            drawer.guiRect = node.localRect;
            drawer.guiStyle = style;
            drawer.color = color * tintColor;
            OnDraw();
        }

        public override void OnEnable()
        {
            drawer.enabled = true;
        }

        public override void OnDisable()
        {
            drawer.enabled = false;
        }

        public override void OnNodeParentChanged()
        {
            canvas.MarkRebuildDrawingList();
        }

        public override void OnNodeSiblingIndexChanged()
        {
            canvas.MarkRebuildDrawingList();
        }

        public override void OnDestroy()
        {
            canvas.MarkRebuildDrawingList();
        }

        public void SetStyleDirty()
        {
            mStyleDirty = true;
        }

        public virtual void RebuildStyle()
        {

        }

        protected virtual void OnDraw()
        {
            DrawContent(GUIContent.none);
        }

        protected void DrawContent(GUIContent content, bool isOn = false, bool isHover = false, bool isActive = false, bool hasKeyboardFocus = false)
        {
            drawer.guiContent = content;
            drawer.isOn = isOn;
            drawer.isHover = isHover;
            drawer.isActive = isActive;
            drawer.hasKeyboardFocus = hasKeyboardFocus;
            drawer.drawProxy = null;
        }

        protected void DrawProcess(Action process)
        {
            drawer.drawProxy = process;
        }

        public virtual Vector2 GetContentSize()
        {
            return node.size;
        }
    }
}
