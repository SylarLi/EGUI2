using System;
using UnityEngine;

namespace EGUI
{
    public class RequireLeafAttribute : Attribute
    {
        private Type mType;

        public Type type { get { return mType; } }

        public RequireLeafAttribute(Type type)
        {
            Debug.Assert(type.IsSubclassOf(typeof(Leaf)));
            mType = type;
        }
    }
}
