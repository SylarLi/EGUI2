using System;

namespace EGUI.Editor
{
    public class UserPropertyDrawerAttribute : Attribute
    {
        private Type mType;

        public Type type { get { return mType; } }

        public UserPropertyDrawerAttribute(Type type)
        {
            mType = type;
        }
    }
}
