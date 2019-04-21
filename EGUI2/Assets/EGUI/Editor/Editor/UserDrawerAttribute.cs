using System;

namespace EGUI.Editor
{
    public class UserDrawerAttribute : Attribute
    {
        private Type mType;

        public Type type { get { return mType; } }

        public UserDrawerAttribute(Type type)
        {
            mType = type;
        }
    }
}
