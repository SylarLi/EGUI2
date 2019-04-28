//using UnityEngine;
//
//namespace EGUI.UI
//{
//    [Persistence]
//    public class Slider : Selectable, IMouseClickHandler, IBeginDragHandler, IScrollWheelHandler, IEndDragHandler
//    {
//        [PersistentField] private Node mFillRect;
//
//        public Node fillRect
//        {
//            get { return mFillRect; }
//            set
//            {
//                if (mFillRect != value)
//                {
//                    mFillRect = value;
//                    UpdateVisuals();
//                }
//            }
//        }
//
//        [PersistentField] private Node mHandleRect;
//
//        public Node handleRect
//        {
//            get { return mHandleRect; }
//            set
//            {
//                if (mHandleRect != value)
//                {
//                    mHandleRect = value;
//                    UpdateVisuals();
//                }
//            }
//        }
//
//        [PersistentField] private Direction mDirection = Direction.LeftToRight;
//
//        public Direction direction
//        {
//            get { return mDirection; }
//            set
//            {
//                if (mDirection != value)
//                {
//                    mDirection = value;
//                    OnUpdateDirection();
//                }
//            }
//        }
//
//        [PersistentField] private float mMinValue = 0f;
//
//        public float minValue
//        {
//            get { return mMinValue; }
//            set
//            {
//                if (mMinValue != value)
//                {
//                    mMinValue = value;
//                    UpdateVisuals();
//                }
//            }
//        }
//
//        [PersistentField] private float mMaxValue = 1f;
//
//        public float maxValue
//        {
//            get { return mMaxValue; }
//            set
//            {
//                if (mMaxValue != value)
//                {
//                    mMaxValue = value;
//                    UpdateVisuals();
//                }
//            }
//        }
//
//        [PersistentField] private bool mWholeNumbers = false;
//
//        public bool wholeNumbers
//        {
//            get { return mWholeNumbers; }
//            set
//            {
//                if (mWholeNumbers != value)
//                {
//                    mWholeNumbers = value;
//                    UpdateVisuals();
//                }
//            }
//        }
//
//        [PersistentField] private float mValue = 0f;
//
//        public float value
//        {
//            get { return wholeNumbers ? Mathf.Round(mValue) : mValue; }
//            set { SetValue(value, true); }
//        }
//
//        public delegate void OnValueChanged(float value);
//
//        public OnValueChanged onValueChanged = value => { };
//
//        public override void Update()
//        {
//            base.Update();
//        }
//
//        public void OnMouseClick(Event eventData)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnScrollWheel(Event eventData)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnBeginDrag(Event eventData)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnEndDrag(Event eventData)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        private void SetValue(float value, bool callback)
//        {
//            value = ClampValue(value);
//            if (mValue != value)
//            {
//                mValue = value;
//                UpdateVisuals();
//                if (callback)
//                    onValueChanged(this.value);
//            }
//        }
//
//        private void UpdateVisuals()
//        {
//            if (fillRect == null || handleRect == null) return;
//            UnityEngine.UI.Slider s;
//            s
//        }
//
//        private void OnUpdateDirection()
//        {
//            
//        }
//
//        private float ClampValue(float value)
//        {
//            return Mathf.Clamp(value, minValue, maxValue);
//        }
//
//        public enum Direction
//        {
//            LeftToRight,
//            RightToLeft,
//            BottomToTop,
//            TopToBottom,
//        }
//    }
//}