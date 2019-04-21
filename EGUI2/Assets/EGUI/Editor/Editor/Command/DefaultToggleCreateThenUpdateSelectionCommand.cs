using EGUI.UI;

namespace EGUI.Editor
{
    public class DefaultToggleCreateThenUpdateSelectionCommand : Command
    {
        private Node mParent;

        private Toggle mToggle;

        private Node[] mSelection;

        public DefaultToggleCreateThenUpdateSelectionCommand() { }

        public DefaultToggleCreateThenUpdateSelectionCommand(Node parent)
        {
            mParent = parent;
        }

        public override void Execute()
        {
            mToggle = DefaultControl.CreateToggle(mParent);
            mSelection = UserSelection.nodes;
            UserSelection.nodes = new Node[] { mToggle.node };
        }

        public override void Undo()
        {
            mToggle.node.Dispose();
            mToggle = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
