using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    internal abstract class UserFrame
    {
        private Rect mRect;

        public Rect rect
        {
            get { return mRect; }
            set
            {
                if (mRect != value)
                {
                    mRect = value;
                    OnResize();
                }
            }
        }

        public Rect nativeRect
        {
            get { return new Rect(0, 0, rect.width, rect.height); }
        }

        private bool mFocused;

        public bool focused
        {
            get { return mFocused; }
            set
            {
                if (mFocused != value)
                {
                    mFocused = value;
                    if (mFocused) OnFocus();
                    else OnLostFocus();
                }
            }
        }

        protected virtual bool scrollEnabled
        {
            get { return true; }
        }

        private Vector2 mScrollPos = Vector2.zero;

        public Vector2 scrollPos
        {
            get { return mScrollPos; }
            set { mScrollPos = value; }
        }

        public virtual void OnDraw()
        {
            EditorGUI.DrawRect(rect, backgroundColor);
            if (focused)
                PersistentGUI.DrawAAPolyLine(rect, UserSetting.FrameFocusedLineWidth,
                    UserSetting.FrameFocusedLineColor);
            GUILayout.BeginArea(rect);
            if (scrollEnabled)
                mScrollPos = GUILayout.BeginScrollView(mScrollPos);
            PersistentGUI.BeginWideMode(rect.width > 330f);
            OnGUI();
            PersistentGUI.EndWideMode();
            if (scrollEnabled)
                GUILayout.EndScrollView();
            GUILayout.EndArea();

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.MouseDrag:
                    if (UserDragDrop.dragging)
                    {
                        if (new Rect(rect.x, rect.yMax - 20, rect.width, 20).Contains(Event.current.mousePosition))
                        {
                            mScrollPos += new Vector2(0, 2);
                            Event.current.Use();    
                        }
                        else if (new Rect(rect.x, rect.y, rect.width, 20).Contains(Event.current.mousePosition))
                        {
                            mScrollPos -= new Vector2(0, 2);
                            Event.current.Use();
                        }
                    }

                    break;
            }
        }

        protected virtual void OnGUI()
        {
        }

        protected virtual void OnResize()
        {
        }

        protected virtual void OnFocus()
        {
        }

        protected virtual void OnLostFocus()
        {
        }

        protected virtual Color backgroundColor
        {
            get { return UserSetting.FrameBackgroundColor; }
        }
    }
}