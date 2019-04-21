using UnityEngine;
using System.Linq;

namespace EGUI.Editor
{
    internal class UserFrameContainer : UserFrame
    {
        private UserFrame[] mFrames;

        public UserFrame[] frames { get { return mFrames; } set { mFrames = value; } }

        public override void OnDraw()
        {
            OnFocusControl();
            base.OnDraw();
        }

        protected void OnFocusControl()
        {
            if (focused && frames != null && frames.Length > 0)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    var contains = frames.Where(f => f.rect.Contains(Event.current.mousePosition));
                    foreach (var f in frames)
                        f.focused = contains.Contains(f);
                }
            }
        }

        protected override void OnGUI()
        {
            if (frames != null && frames.Length > 0)
            {
                foreach (var f in frames)
                    f.OnDraw();
            }
        }

        protected override void OnLostFocus()
        {
            if (frames != null && frames.Length > 0)
            {
                foreach (var f in frames)
                    f.focused = false;
            }
        }

        protected override Color backgroundColor
        {
            get
            {
                return UserSetting.FrameContainerBackgroundColor;
            }
        }
    }
}
