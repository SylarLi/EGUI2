using EGUI.Editor;

namespace EGUI
{
    public class NodeSelectionCommand : Command
    {
        private Node[] mNodes;

        public NodeSelectionCommand() { }

        public NodeSelectionCommand(Node[] nodes)
        {
            mNodes = nodes;
        }

        public override void Execute()
        {
            var nodes = UserDatabase.selection.nodes; 
            UserDatabase.selection.nodes = mNodes;
            mNodes = nodes;
        }

        public override void Undo()
        {
            Execute();
        }
    }
}
