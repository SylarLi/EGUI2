using System;
using EGUI.UI;

namespace EGUI.Editor
{
    public class DefaultControlCreateThenUpdateSelectionCommand : Command
    {
        private Node mParent;

        private Type mUIType;

        private Node mNode;

        private Node[] mSelection;

        public DefaultControlCreateThenUpdateSelectionCommand() { }

        public DefaultControlCreateThenUpdateSelectionCommand(Node parent, Type uiType)
        {
            mParent = parent;
            mUIType = uiType;
        }

        public override void Execute()
        {
            if (mUIType == typeof(Node))
                mNode = DefaultControl.CreateNode(mParent);
            else if (mUIType == typeof(Image))
                mNode = DefaultControl.CreateImage(mParent).node;
            else if (mUIType == typeof(Text))
                mNode = DefaultControl.CreateText(mParent).node;
            else if (mUIType == typeof(Button))
                mNode = DefaultControl.CreateButton(mParent).node;
            else if (mUIType == typeof(Toggle))
                mNode = DefaultControl.CreateToggle(mParent).node;
            else if (mUIType == typeof(TextField))
                mNode = DefaultControl.CreateTextField(mParent).node;
            else if (mUIType == typeof(Scrollbar))
                mNode = DefaultControl.CreateScrollbar(mParent).node;
            else
                throw new NotImplementedException();
            mSelection = UserDatabase.selection.nodes;
            UserDatabase.selection.nodes = new Node[] { mNode };
        }

        public override void Undo()
        {
            mNode.Dispose();
            mNode = null;
            UserDatabase.selection.nodes = mSelection;
            mSelection = null;
        }
    }
}
