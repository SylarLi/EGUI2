using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class VerticalLayoutGroup : LayoutGroup
    {
        protected override Vector2 GetLayoutSize()
        {
            Vector2 size = Vector2.zero;
            foreach (var child in layoutChildren)
            {
                size.y += child.size.y + spacing.y;
            }
            return size;
        }

        protected override Vector2 GetChildOffset(Node child)
        {
            var offset = base.GetChildOffset(child);
            if (childAlignment == Alignment.UpperCenter ||
                childAlignment == Alignment.MiddleCenter ||
                childAlignment == Alignment.LowerCenter)
            {
                offset.x -= child.size.x * 0.5f;
            }
            if (childAlignment == Alignment.UpperRight ||
                childAlignment == Alignment.MiddleRight ||
                childAlignment == Alignment.LowerRight)
            {
                offset.x -= child.size.x;
            }
            if (childAlignment == Alignment.MiddleLeft ||
                childAlignment == Alignment.MiddleCenter ||
                childAlignment == Alignment.MiddleRight)
            {
                offset.y -= layoutSize.y * 0.5f;
            }
            if (childAlignment == Alignment.LowerLeft ||
                childAlignment == Alignment.LowerCenter ||
                childAlignment == Alignment.LowerRight)
            {
                offset.y -= layoutSize.y;
            }
            return offset;
        }

        public override void Update()
        {
            base.Update();
            Vector2 offset = Vector2.zero;
            for (int i = 0; i < layoutChildren.Count; i++)
            {
                var child = layoutChildren[i];
                child.localPosition = offset + GetChildOffset(child);
                offset.y += child.size.y + spacing.y;
            }
        }

        public override Vector2 GetContentSize()
        {
            Vector2 size = Vector2.zero;
            foreach (var child in layoutChildren)
            {
                size.x = Mathf.Max(size.x, child.size.x);
                size.y += child.size.y + spacing.y;
            }
            return size;
        }
    }
}
