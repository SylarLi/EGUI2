using System;
using UnityEngine;

namespace EGUI.Editor
{
    public class CustomDrawerAttribute : Attribute
    {
        private Type mType;

        public Type type { get { return mType; } }

        private bool mFoldEnabled;

        public bool foldEnabled { get { return mFoldEnabled; } }

        public CustomDrawerAttribute(Type type, bool foldEnabled = true)
        {
            Debug.Assert(typeof(PropertyDrawer).IsAssignableFrom(type), "Type should be derived from PropertyDrawer.");
            mType = type;
            mFoldEnabled = foldEnabled;
        }
    }
}
