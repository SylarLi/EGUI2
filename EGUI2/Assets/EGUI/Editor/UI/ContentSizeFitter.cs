namespace EGUI.UI
{
    [Persistence]
    public class ContentSizeFitter : Leaf
    {
        public enum FitMode
        {
            Unconstrained = 0,
            PreferredSize = 1,
        }

        [PersistentField]
        private FitMode mHorizontalFit = FitMode.Unconstrained;

        public FitMode horizontalFit { get { return mHorizontalFit; } set { mHorizontalFit = value; } }

        [PersistentField]
        private FitMode mVerticalFit = FitMode.Unconstrained;

        public FitMode verticalFit { get { return mVerticalFit; } set { mVerticalFit = value; } }

        public override void Update()
        {
            if (horizontalFit == FitMode.PreferredSize ||
                verticalFit == FitMode.PreferredSize)
            {
                var layoutGroup = GetLeaf<LayoutGroup>(false);
                if (layoutGroup != null)
                {
                    var size = layoutGroup.node.size;
                    var contentSize = layoutGroup.GetContentSize();
                    if (horizontalFit == FitMode.PreferredSize)
                    {
                        size.x = contentSize.x;
                    }
                    if (verticalFit == FitMode.PreferredSize)
                    {
                        size.y = contentSize.y;
                    }
                    layoutGroup.node.size = size;
                }
                else
                {
                    var graphic = GetLeaf<Graphic>(false);
                    if (graphic != null)
                    {
                        var size = graphic.node.size;
                        var contentSize = graphic.GetContentSize();
                        if (horizontalFit == FitMode.PreferredSize)
                        {
                            size.x = contentSize.x;
                        }
                        if (verticalFit == FitMode.PreferredSize)
                        {
                            size.y = contentSize.y;
                        }
                        graphic.node.size = size;
                    }
                }
            }
        }
    }
}
