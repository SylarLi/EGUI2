using EGUI.UI;

namespace EGUI.Editor
{
    public class DefaultImageCreateThenUpdateSelectionCommand : Command
    {
        private Node mParent;

        private Image mImage;

        private Node[] mSelection;

        public DefaultImageCreateThenUpdateSelectionCommand() { }

        public DefaultImageCreateThenUpdateSelectionCommand(Node parent)
        {
            mParent = parent;
        }

        public override void Execute()
        {
            mImage = DefaultControl.CreateImage(mParent);
            mSelection = UserSelection.nodes;
            UserSelection.nodes = new Node[] { mImage.node };
        }

        public override void Undo()
        {
            mImage.node.Dispose();
            mImage = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
