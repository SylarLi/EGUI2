namespace EGUI
{
    public class NodeRemoveLeafCommand : Command
    {
        private Node mNode;

        private Leaf mLeaf;

        public NodeRemoveLeafCommand() { }

        public NodeRemoveLeafCommand(Node node, Leaf leaf)
        {
            mNode = node;
            mLeaf = leaf;
        }

        public override void Execute()
        {
            mNode.RemoveLeaf(mLeaf);
            mLeaf.MarkInternalDisposed(true);
        }

        public override void Undo()
        {
            mLeaf.MarkInternalDisposed(false);
            mNode.AddLeaf(mLeaf);
        }
    }
}
