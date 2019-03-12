using System;
using UnityEngine;

namespace EGUI.Editor
{
    public class CustomDrawerAttribute : Attribute
    {
        private Type mType;

        public Type type { get { return mType; } }

        public CustomDrawerAttribute(Type type)
        {
            Debug.Assert(typeof(PropertyDrawer).IsAssignableFrom(type), "Type should be derived from PropertyDrawer.");
            mType = type;
        }
    }
}
