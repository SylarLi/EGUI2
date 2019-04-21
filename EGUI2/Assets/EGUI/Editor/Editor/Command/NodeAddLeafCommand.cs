namespace EGUI
{
    public class NodeAddLeafCommand : Command
    {
        private Node mNode;

        private Leaf mLeaf;

        public NodeAddLeafCommand() { }

        public NodeAddLeafCommand(Node node, Leaf leaf)
        {
            mNode = node;
            mLeaf = leaf;
        }

        public override void Execute()
        {
            mNode.AddLeaf(mLeaf);
        }

        public override void Undo()
        {
            mNode.RemoveLeaf(mLeaf);
        }
    }
}
