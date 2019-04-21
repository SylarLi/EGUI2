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
        }

        public override void Undo()
        {
            mNode.AddLeaf(mLeaf);
        }
    }
}
