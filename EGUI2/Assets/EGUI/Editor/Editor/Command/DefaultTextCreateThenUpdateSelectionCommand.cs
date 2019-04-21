using EGUI.UI;

namespace EGUI.Editor
{
    public class DefaultTextCreateThenUpdateSelectionCommand : Command
    {
        private Node mParent;

        private Text mText;

        private Node[] mSelection;

        public DefaultTextCreateThenUpdateSelectionCommand() { }

        public DefaultTextCreateThenUpdateSelectionCommand(Node parent)
        {
            mParent = parent;
        }

        public override void Execute()
        {
            mText = DefaultControl.CreateText(mParent);
            mSelection = UserSelection.nodes;
            UserSelection.nodes = new Node[] { mText.node };
        }

        public override void Undo()
        {
            mText.node.Dispose();
            mText = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
