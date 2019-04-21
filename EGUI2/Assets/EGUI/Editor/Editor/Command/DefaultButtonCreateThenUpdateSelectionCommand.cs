using EGUI.UI;

namespace EGUI.Editor
{
    public class DefaultButtonCreateThenUpdateSelectionCommand : Command
    {
        private Node mParent;

        private Button mButton;

        private Node[] mSelection;

        public DefaultButtonCreateThenUpdateSelectionCommand() { }

        public DefaultButtonCreateThenUpdateSelectionCommand(Node parent)
        {
            mParent = parent;
        }

        public override void Execute()
        {
            mButton = DefaultControl.CreateButton(mParent);
            mSelection = UserSelection.nodes;
            UserSelection.nodes = new Node[] { mButton.node };
        }

        public override void Undo()
        {
            mButton.node.Dispose();
            mButton = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
