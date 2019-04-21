namespace EGUI.Editor
{
    public class NodeDuplicateThenSelectionCommand : Command
    {
        private Node[] mNodes;

        private Node[] mParents;

        private Node mRoot;

        private Node[] mDups;

        private Node[] mSelection;

        public NodeDuplicateThenSelectionCommand() { }

        public NodeDuplicateThenSelectionCommand(Node[] nodes, Node[] parents, Node root)
        {
            mNodes = nodes;
            mParents = parents;
            mRoot = root;
        }

        public override void Execute()
        {
            mDups = UserUtil.Duplicate(mNodes, mRoot);
            for (int i = 0 ; i < mDups.Length; i++)
            {
                mDups[i].parent = mParents[i];
            }
            mSelection = UserSelection.nodes;
            UserSelection.nodes = mDups;
        }

        public override void Undo()
        {
            foreach (var dup in mDups)
                dup.Dispose();
            mDups = null;
            UserSelection.nodes = mSelection;
            mSelection = null;
        }
    }
}
