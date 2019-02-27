﻿using System.Collections.Generic;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class Selectable : Leaf, ISelectable, IInteractive, IMouseDownHandler, IMouseUpHandler, IDragHandler
    {
        [PersistentField]
        private Graphic mTargetGraphic;

        public Graphic targetGraphic { get { return mTargetGraphic; } set { if (mTargetGraphic != value) { mTargetGraphic = value; SetStateTransitionDirty(); } } }

        public Image image { get { return mTargetGraphic as Image; } set { targetGraphic = value; } }

        [PersistentField]
        private bool mInteractive = true;

        public bool interactive { get { return mInteractive; } set { if (mInteractive != value) { mInteractive = value; FlushSelectionState(); } } }

        [PersistentField]
        private Transition mTransition = Transition.ColorTint;

        public Transition transition { get { return mTransition; } set { if (mTransition != value) { mTransition = value; SetStateTransitionDirty(); } } }

        [PersistentField]
        private SpriteState mSpriteState;

        public SpriteState spriteState { get { return mSpriteState; } set { if (!mSpriteState.Equals(value)) { mSpriteState = value; SetStateTransitionDirty(); } } }

        [PersistentField]
        private ColorState mColorState;

        public ColorState colorState { get { return mColorState; } set { if (!mColorState.Equals(value)) { mColorState = value; SetStateTransitionDirty(); } } }

        [PersistentField]
        private bool mFocused;

        public bool focused { get { return mFocused; } set { if (mFocused != value) { mFocused = value; FlushSelectionState(); } } }

        [PersistentField]
        private bool mPressed;

        protected bool pressed { get { return mPressed; } set { if (mPressed != value) { mPressed = value; FlushSelectionState(); } } }

        [PersistentField]
        private SelectionState mSelectionState = SelectionState.Normal;

        protected SelectionState selectionState { get { return mSelectionState; } set { if (mSelectionState != value) { mSelectionState = value; SetStateTransitionDirty(); } } }

        private bool mStateTransitionDirty;

        public Selectable() : base()
        {
            FlushSelectionState();
        }

        public override void Update()
        {
            if (mStateTransitionDirty)
            {
                UpdateStateTransition();
                mStateTransitionDirty = false;
            }
        }

        public void SetStateTransitionDirty()
        {
            mStateTransitionDirty = true;
        }

        public virtual void OnMouseDown(Event eventData)
        {
            FocusControl.currentSelectable = this;
            pressed = true;
        }

        public virtual void OnMouseUp(Event eventData)
        {
            pressed = false;
        }

        public virtual void OnDrag(Event eventData)
        {
            pressed = node.localRect.Contains(node.world2LocalMatrix.MultiplyPoint(eventData.mousePosition));
        }

        private void FlushSelectionState()
        {
            if (interactive)
            {
                if (pressed)
                {
                    selectionState = SelectionState.Pressed;
                }
                else if (focused)
                {
                    selectionState = SelectionState.Focused;
                }
                else
                {
                    selectionState = SelectionState.Normal;
                }
            }
            else
            {
                selectionState = SelectionState.Disabled;
            }
        }

        private void UpdateStateTransition()
        {
            Color targetColor;
            Sprite targetSprite;
            switch (selectionState)
            {
                case SelectionState.Disabled:
                    {
                        targetColor = colorState.disabledColor;
                        targetSprite = spriteState.disabledSprite;
                        break;
                    }
                case SelectionState.Focused:
                    {
                        targetColor = colorState.focusedColor;
                        targetSprite = spriteState.focusedSprite;
                        break;
                    }
                case SelectionState.Pressed:
                    {
                        targetColor = colorState.pressedColor;
                        targetSprite = spriteState.pressedSprite;
                        break;
                    }
                default:
                    {
                        targetColor = colorState.normalColor;
                        targetSprite = null;
                        break;
                    }
            }
            if (targetGraphic != null)
            {
                targetGraphic.tintColor = Color.white;
            }
            if (image != null)
            {
                image.overrideSprite = null;
            }
            switch (transition)
            {
                case Transition.ColorTint:
                    {
                        if (targetGraphic != null)
                        {
                            targetGraphic.tintColor = targetColor;
                        }
                        break;
                    }
                case Transition.SpriteSwap:
                    {
                        if (image != null)
                        {
                            image.overrideSprite = targetSprite;
                        }
                        break;
                    }
            }
        }

        public enum Transition
        {
            None = 0,
            ColorTint = 1,
            SpriteSwap = 2,
        }

        protected enum SelectionState
        {
            Normal = 0,
            Focused = 1,
            Pressed = 2,
            Disabled = 3
        }

        [Persistence]
        public struct SpriteState
        {
            [PersistentField]
            private Sprite mDisabledSprite;

            [PersistentField]
            private Sprite mFocusedSprite;

            [PersistentField]
            private Sprite mPressedSprite;

            public Sprite disabledSprite { get { return mDisabledSprite; } set { mDisabledSprite = value; } }

            public Sprite focusedSprite { get { return mFocusedSprite; } set { mFocusedSprite = value; } }

            public Sprite pressedSprite { get { return mPressedSprite; } set { mPressedSprite = value; } }

            public bool Equals(SpriteState state)
            {
                return disabledSprite == state.disabledSprite &&
                    focusedSprite == state.focusedSprite &&
                    pressedSprite == state.pressedSprite;
            }
        }

        [Persistence]
        public struct ColorState
        {
            [PersistentField]
            private Color mNormalColor;

            [PersistentField]
            private Color mDisabledColor;

            [PersistentField]
            private Color mFocusedColor;

            [PersistentField]
            private Color mPressedColor;

            public Color normalColor { get { return mNormalColor; } set { mNormalColor = value; } }

            public Color disabledColor { get { return mDisabledColor; } set { mDisabledColor = value; } }

            public Color focusedColor { get { return mFocusedColor; } set { mFocusedColor = value; } }

            public Color pressedColor { get { return mPressedColor; } set { mPressedColor = value; } }

            public bool Equals(ColorState state)
            {
                return normalColor == state.normalColor &&
                    disabledColor == state.disabledColor &&
                    focusedColor == state.focusedColor &&
                    pressedColor == state.pressedColor;
            }
        }
    }
}
