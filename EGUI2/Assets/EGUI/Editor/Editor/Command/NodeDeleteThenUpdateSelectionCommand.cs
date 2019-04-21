using System.Linq;

namespace EGUI.Editor
{
    public class NodeDeleteThenUpdateSelectionCommand : Command
    {
        private Node[] mNodes;

        private Node mRoot;

        private Node[] mDups;

        private Node[] mParents;

        private Node[] mSelection;

        public NodeDeleteThenUpdateSelectionCommand() { }

        public NodeDeleteThenUpdateSelectionCommand(Node[] nodes, Node root)
        {
            mNodes = nodes;
            mRoot = root;
        }

        public override void Execute()
        {
            mDups = UserUtil.Duplicate(mNodes, mRoot);
            mParents = mNodes.Select(i => i.parent).ToArray();
            mSelection = UserSelection.nodes ?? new Node[0];
            UserSelection.nodes = mSelection.Where(i => !mNodes.Contains(i)).ToArray();
            foreach (var node in mNodes)
                node.Dispose();
            mNodes = null;
        }

        public override void Undo()
        {
            for (int i = 0; i < mDups.Length; i++)
                mDups[i].parent = mParents[i];
            mNodes = mDups;
            mDups = null;
            mParents = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
