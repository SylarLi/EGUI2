namespace EGUI.Editor
{
    public sealed class UserSelection
    {
        public delegate void OnChange();

        public static OnChange onChange = () => { };

        private static Node[] mNodes;

        public static Node[] nodes { get { return mNodes; } set { mNodes = value; onChange(); } }

        public static Node node { get { return nodes != null && nodes.Length > 0 ? nodes[nodes.Length - 1] : null; } set { nodes = value != null ? new Node[] { value } : null; } }
    }
}
