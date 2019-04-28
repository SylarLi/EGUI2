namespace EGUI.Editor
{
    public class NodeDuplicateThenSelectionCommand : Command
    {
        private Node[] mNodes;

        private Node[] mParents;

        private Node mRoot;

        private Node[] mDups;

        private Node[] mSelection;

        public NodeDuplicateThenSelectionCommand()
        {
        }

        public NodeDuplicateThenSelectionCommand(Node[] nodes, Node[] parents, Node root)
        {
            mNodes = nodes;
            mParents = parents;
            mRoot = root;
        }

        public override void Execute()
        {
            if (mDups == null)
                mDups = UserUtil.Duplicate(mNodes, mRoot);
            for (var i = 0; i < mDups.Length; i++)
            {
                mDups[i].MarkInternalDisposed(false);
                mDups[i].SetParent(mParents[i]);
            }

            mSelection = UserDatabase.selection.nodes;
            UserDatabase.selection.nodes = mDups;
        }

        public override void Undo()
        {
            foreach (var dup in mDups)
                dup.MarkInternalDisposed(true);
            UserDatabase.selection.nodes = mSelection;
            mSelection = null;
        }
    }
}