namespace EGUI.Editor
{
    public sealed class UserClipBoard
    {
        private static object mData;

        public static object data { get { return mData; } }

        private static bool mIsCut = false;

        public static bool isCut { get { return mIsCut; } }

        public static void Copy(object data)
        {
            mData = data;
            mIsCut = false;
        }

        public static void Cut(object data)
        {
            mData = data;
            mIsCut = true;
        }

        public static object Paste()
        {
            var ret = mData;
            if (mIsCut) mData = null;
            return ret;
        }
    }
}
