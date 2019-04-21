namespace EGUI.Editor
{
    public sealed class UserDragDrop
    {
        private static bool mDragging;

        public static bool dragging { get { return mDragging; } }

        private static object mData;

        public static object data { get { return mData; } }

        public static void StartDrag(object data)
        {
            mDragging = true;
            mData = data;
        }

        public static void StopDrag()
        {
            mDragging = false;
            mData = null;
        }
    }
}
