using EGUI.UI;

namespace EGUI.Editor
{
    public class DefaultTextFieldCreateThenUpdateSelectionCommand : Command
    {
        private Node mParent;

        private TextField mTextField;

        private Node[] mSelection;

        public DefaultTextFieldCreateThenUpdateSelectionCommand() { }

        public DefaultTextFieldCreateThenUpdateSelectionCommand(Node parent)
        {
            mParent = parent;
        }

        public override void Execute()
        {
            mTextField = DefaultControl.CreateTextField(mParent);
            mSelection = UserSelection.nodes;
            UserSelection.nodes = new Node[] { mTextField.node };
        }

        public override void Undo()
        {
            mTextField.node.Dispose();
            mTextField = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
