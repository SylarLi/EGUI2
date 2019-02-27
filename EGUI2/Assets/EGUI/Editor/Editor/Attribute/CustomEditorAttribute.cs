using System;

namespace EGUI.Editor
{
    public class CustomEditorAttribute : Attribute
    {
        private Type mType;

        public Type type { get { return mType; } }

        public CustomEditorAttribute(Type type)
        {
            mType = type;
        }
    }
}
