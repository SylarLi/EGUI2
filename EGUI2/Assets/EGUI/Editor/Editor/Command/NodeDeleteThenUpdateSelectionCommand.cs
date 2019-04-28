using System.Linq;

namespace EGUI.Editor
{
    public class NodeDeleteThenUpdateSelectionCommand : Command
    {
        private Node[] mNodes;

        private Node[] mParents;

        private int[] mIndexes;

        private Node[] mSelection;

        public NodeDeleteThenUpdateSelectionCommand()
        {
        }

        public NodeDeleteThenUpdateSelectionCommand(Node[] nodes)
        {
            mNodes = nodes;
        }

        public override void Execute()
        {
            mSelection = UserDatabase.selection.nodes ?? new Node[0];
            UserDatabase.selection.nodes = mSelection.Where(i => !mNodes.Contains(i)).ToArray();
            mParents = new Node[mNodes.Length];
            mIndexes = new int[mNodes.Length];
            for (var i = 0; i < mNodes.Length; i++)
            {
                mIndexes[i] = mNodes[i].GetSiblingIndex();
                mParents[i] = mNodes[i].parent;
                mNodes[i].parent = null;
                mNodes[i].MarkInternalDisposed(true);
            }
        }

        public override void Undo()
        {
            for (var i = 0; i < mNodes.Length; i++)
            {
                mNodes[i].MarkInternalDisposed(false);
                mNodes[i].parent = mParents[i];
                mNodes[i].SetSiblingIndex(mIndexes[i]);
            }

            mParents = null;
            mIndexes = null;
            UserDatabase.selection.nodes = mSelection;
            mSelection = null;
        }
    }
}