using System;
using System.Net;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class Scrollbar : Selectable, IBeginDragHandler, IEndDragHandler
    {
        [PersistentField] private Node mHandleRect;

        public Node handleRect
        {
            get { return mHandleRect; }
            set
            {
                if (mHandleRect != value)
                {
                    mHandleRect = value;
                    UpdateVisuals();
                }
            }
        }

        [PersistentField] private float mValue = 0f;

        public float value
        {
            get { return mValue; }
            set { SetValue(value, true); }
        }

        [PersistentField] private float mSize = 0f;

        public float size
        {
            get { return mSize; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (mSize != value)
                {
                    mSize = value;
                    UpdateVisuals();
                }
            }
        }

        [PersistentField] private Direction mDirection = Direction.LeftToRight;

        public Direction direction
        {
            get { return mDirection; }
            set { SetDirection(value); }
        }

        public delegate void OnValueChanged(float value);

        public OnValueChanged onValueChanged = value => { };

        private bool mDragging = false;

        public override bool OnMouseDown(Event eventData)
        {
            base.OnMouseDown(eventData);
            var localPos = node.InverseTransformPoint(eventData.mousePosition);
            DragMouseTo(localPos);
            return true;
        }

        public bool OnBeginDrag(Event eventData)
        {
            mDragging = true;
            return true;
        }

        public override bool OnDrag(Event eventData)
        {
            base.OnDrag(eventData);
            if (mDragging)
            {
                DragMouseTo(node.InverseTransformPoint(eventData.mousePosition));
                return true;
            }

            return false;
        }

        public bool OnEndDrag(Event eventData)
        {
            if (mDragging)
            {
                mDragging = false;
                return true;
            }

            return false;
        }

        private void DragMouseTo(Vector2 localPos)
        {
            if (handleRect == null) return;
            switch (direction)
            {
                case Direction.LeftToRight:
                    if (node.size.x == 0) return;
                    value = (localPos.x - handleRect.size.x * 0.5f) / (node.size.x - handleRect.size.x);
                    break;
                case Direction.RightToLeft:
                    if (node.size.x == 0) return;
                    value = (node.size.x - localPos.x - handleRect.size.x * 0.5f) / (node.size.x - handleRect.size.x);
                    break;
                case Direction.TopToBottom:
                    if (node.size.y == 0) return;
                    value = (localPos.y - handleRect.size.y * 0.5f) / (node.size.y - handleRect.size.y);
                    break;
                case Direction.BottomToTop:
                    if (node.size.y == 0) return;
                    value = (node.size.y - localPos.y - handleRect.size.y * 0.5f) / (node.size.y - handleRect.size.y);
                    break;
            }
        }

        private void SetValue(float value, bool callback)
        {
            value = Mathf.Clamp(value, 0, 1);
            if (Math.Abs(mValue - value) < float.Epsilon) return;
            mValue = value;
            UpdateVisuals();
            if (callback)
                onValueChanged(this.value);
        }

        private void UpdateVisuals()
        {
            if (handleRect == null) return;
            var axis = GetAxis(direction);
            if ((axis == Axis.Horizontal && node.size.x == 0) ||
                (axis == Axis.Vertical && node.size.y == 0)) return;
            var anchorMin = Vector2.zero;
            var anchorMax = Vector2.zero;
            var slidingLen = axis == Axis.Horizontal
                ? (node.size.x - handleRect.size.x) / node.size.x
                : (node.size.y - handleRect.size.y) / node.size.y;
            switch (direction)
            {
                case Direction.LeftToRight:
                    anchorMin.Set(Mathf.Max(0, value * slidingLen), 0);
                    anchorMax.Set(Mathf.Min(1, anchorMin.x + size), 1);
                    break;
                case Direction.RightToLeft:
                    anchorMin.Set(Mathf.Max(0, 1 - value * slidingLen - size), 0);
                    anchorMax.Set(Mathf.Min(1, anchorMin.x + size), 1);
                    break;
                case Direction.TopToBottom:
                    anchorMin.Set(0, Mathf.Max(0, value * slidingLen));
                    anchorMax.Set(1, Mathf.Min(1, anchorMin.y + size));
                    break;
                case Direction.BottomToTop:
                    anchorMin.Set(0, Mathf.Max(0, 1 - value * slidingLen - size));
                    anchorMax.Set(1, Mathf.Min(1, anchorMin.y + size));
                    break;
            }

            handleRect.anchorMin = anchorMin;
            handleRect.anchorMax = anchorMax;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
        }

        private void SetDirection(Direction value)
        {
            if (mDirection == value) return;
            if (GetAxis(mDirection) != GetAxis(value))
            {
                var nodeSize = node.size;
                node.size = new Vector2(nodeSize.y, nodeSize.x);
            }

            mDirection = value;
            UpdateVisuals();
        }

        private Axis GetAxis(Direction dir)
        {
            return dir == Direction.LeftToRight || dir == Direction.RightToLeft
                ? Axis.Horizontal
                : Axis.Vertical;
        }

        public enum Direction
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        private enum Axis
        {
            Horizontal,
            Vertical,
        }
    }
}