using System.Collections.Generic;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class LayoutGroup : Leaf, IContentSizeFitable
    {
        public enum Alignment
        {
            UpperLeft = 1,
            UpperCenter = 2,
            UpperRight = 3,
            MiddleLeft = 4,
            MiddleCenter = 5,
            MiddleRight = 6,
            LowerLeft = 7,
            LowerCenter = 8,
            LowerRight = 9
        }

        [PersistentField]
        private RectOffset mPadding = new RectOffset(0, 0, 0, 0);

        public RectOffset padding { get { return mPadding; } set { mPadding = value; } }

        [PersistentField]
        private Vector2 mSpacing = Vector2.zero;

        public Vector2 spacing { get { return mSpacing; } set { mSpacing = value; } }

        [PersistentField]
        private Alignment mChildAlignment = Alignment.UpperLeft;

        public Alignment childAlignment { get { return mChildAlignment; } set { mChildAlignment = value; } }

        private List<Node> mLayoutChildren = new List<Node>();

        protected List<Node> layoutChildren { get { return mLayoutChildren; } }

        private Vector2 mLayoutSize = Vector2.zero;

        protected Vector2 layoutSize { get { return mLayoutSize; } }

        public override void Update()
        {
            PrepareLayoutChildren();
            PrepareLayoutSize();
        }

        protected void PrepareLayoutChildren()
        {
            layoutChildren.Clear();
            for (int i = 0; i < node.childCount; i++)
            {
                var child = node.GetChild(i);
                if (child.active)
                {
                    var layoutEl = child.GetLeaf<LayoutElement>();
                    if (layoutEl == null ||
                        !layoutEl.active ||
                        !layoutEl.ignoreLayout)
                    {
                        layoutChildren.Add(child);
                    }
                }
            }
        }

        protected void PrepareLayoutSize()
        {
            mLayoutSize = GetLayoutSize();
        }

        protected virtual Vector2 GetLayoutSize()
        {
            return Vector2.zero;
        }

        protected virtual Vector2 GetChildOffset(Node child)
        {
            switch (childAlignment)
            {
                case Alignment.UpperLeft:
                    {
                        return new Vector2(
                            child.pivot.x * child.size.x + padding.left,
                            child.pivot.y * child.size.y + padding.top);
                    }
                case Alignment.UpperCenter:
                    {
                        return new Vector2(
                            node.size.x * 0.5f + child.pivot.x * child.size.x + padding.left + padding.right,
                            child.pivot.y * child.size.y + padding.top);
                    }
                case Alignment.UpperRight:
                    {
                        return new Vector2(
                            node.size.x + child.pivot.x * child.size.x + padding.right,
                            child.pivot.y * child.size.y + padding.top);
                    }
                case Alignment.MiddleLeft:
                    {
                        return new Vector2(
                            child.pivot.x * child.size.x + padding.left,
                            node.size.y * 0.5f + child.pivot.y * child.size.y + padding.top + padding.bottom);
                    }
                case Alignment.MiddleCenter:
                    {
                        return new Vector2(
                            node.size.x * 0.5f + child.pivot.x * child.size.x + padding.left + padding.right,
                            node.size.y * 0.5f + child.pivot.y * child.size.y + padding.top + padding.bottom);
                    }
                case Alignment.MiddleRight:
                    {
                        return new Vector2(
                            node.size.x + child.pivot.x * child.size.x + padding.right,
                            node.size.y * 0.5f + child.pivot.y * child.size.y + padding.top + padding.bottom);
                    }
                case Alignment.LowerLeft:
                    {
                        return new Vector2(
                            child.pivot.x * child.size.x + padding.left,
                            node.size.y + child.pivot.y * child.size.y + padding.bottom);
                    }
                case Alignment.LowerCenter:
                    {
                        return new Vector2(
                            node.size.x * 0.5f + child.pivot.x * child.size.x + padding.left + padding.right,
                            node.size.y + child.pivot.y * child.size.y + padding.bottom);
                    }
                case Alignment.LowerRight:
                    {
                        return new Vector2(
                            node.size.x + child.pivot.x * child.size.x + padding.right,
                            node.size.y + child.pivot.y * child.size.y + padding.bottom);
                    }
            }
            return Vector2.zero;
        }

        public virtual Vector2 GetContentSize()
        {
            return node.size;
        }
    }
}