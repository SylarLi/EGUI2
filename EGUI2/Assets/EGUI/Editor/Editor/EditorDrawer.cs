using UnityEngine;

namespace EGUI.Editor
{
    public abstract class EditorDrawer
    {
        private Rect mPosition;

        public Rect position { get { return mPosition; } }

        public EditorDrawer(Rect position)
        {
            mPosition = position;
        }
    }
}
