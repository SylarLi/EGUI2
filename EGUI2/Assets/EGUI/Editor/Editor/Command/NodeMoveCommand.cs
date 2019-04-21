namespace EGUI
{
    public class NodeMoveCommand : Command
    {
        private Node mNode;

        private int mRawIndex;

        private int mNewIndex;

        private Node mRawParent;

        private Node mNewParent;

        public NodeMoveCommand() { }

        public NodeMoveCommand(Node node, Node parent, int index)
        {
            mNode = node;
            mNewParent = parent;
            mNewIndex = index;
            mRawIndex = mNode.GetSiblingIndex();
            mRawParent = mNode.parent;
        }

        public override void Execute()
        {
            mNode.parent = mNewParent;
            mNode.SetSiblingIndex(mNewIndex);
        }

        public override void Undo()
        {
            mNode.parent = mRawParent;
            mNode.SetSiblingIndex(mRawIndex);
        }
    }
}
