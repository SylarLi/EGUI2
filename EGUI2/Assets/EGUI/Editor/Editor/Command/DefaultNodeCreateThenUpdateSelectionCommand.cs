using EGUI.UI;

namespace EGUI.Editor
{
    public class DefaultNodeCreateThenUpdateSelectionCommand : Command
    {
        private Node mParent;

        private Node mNode;

        private Node[] mSelection;

        public DefaultNodeCreateThenUpdateSelectionCommand() { }

        public DefaultNodeCreateThenUpdateSelectionCommand(Node parent)
        {
            mParent = parent;
        }

        public override void Execute()
        {
            mNode = DefaultControl.CreateNode(mParent);
            mSelection = UserSelection.nodes;
            UserSelection.nodes = new Node[] { mNode };
        }

        public override void Undo()
        {
            mNode.Dispose();
            mNode = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
