using UnityEngine;

namespace EGUI.UI
{
    public class LayoutElement : Leaf
    {
        [PersistentField]
        private bool mIgnoreLayout;

        public bool ignoreLayout { get { return mIgnoreLayout; } set { mIgnoreLayout = value; } }
    }
}
