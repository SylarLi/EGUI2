using System;
using UnityEngine;

namespace EGUI.Editor
{
    public class CustomEditorAttribute : Attribute
    {
        private Type mType;

        public Type type { get { return mType; } }

        public CustomEditorAttribute(Type type)
        {
            Debug.Assert(typeof(EditorDrawer).IsAssignableFrom(type), "Type should be derived from Editor.");
            mType = type;
        }
    }
}
