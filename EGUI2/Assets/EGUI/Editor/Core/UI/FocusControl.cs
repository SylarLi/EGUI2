namespace EGUI.UI
{
    [Persistence]
    public sealed class FocusControl
    {
        [PersistentField]
        private static ISelectable mCurrentSelectable;

        public static ISelectable currentSelectable
        {
            get
            {
                return mCurrentSelectable;
            }
            set
            {
                if (mCurrentSelectable != value)
                {
                    if (mCurrentSelectable != null)
                    {
                        mCurrentSelectable.focused = false;
                    }
                    mCurrentSelectable = value;
                    if (mCurrentSelectable != null)
                    {
                        mCurrentSelectable.focused = true;
                    }
                }
            }
        }
    }
}
