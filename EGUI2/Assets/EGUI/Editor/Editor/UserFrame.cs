using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    internal abstract class UserFrame
    {
        private Rect mRect;

        public Rect rect { get { return mRect; } set { if (mRect != value) { mRect = value; OnResize(); } } }

        private bool mFocused;

        public bool focused { get { return mFocused; } set { if (mFocused != value) { mFocused = value; if (mFocused) OnFocus(); else OnLostFocus(); } } }

        private Vector2 mScrollPos = Vector2.zero;

        public virtual void OnDraw()
        {
            EditorGUI.DrawRect(rect, backgroundColor);
            GUILayout.BeginArea(rect);
            mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
            PersistentGUI.BeginWideMode(rect.width > 330f);
            var eventType = Event.current.type;
            if (focused || 
                eventType == EventType.Layout ||
                eventType == EventType.Repaint ||
                eventType == EventType.ScrollWheel)
                OnGUI();
            PersistentGUI.EndWideMode();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
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
            get
            {
                return UserSetting.FrameBackgroundColor;
            }
        }
    }
}
