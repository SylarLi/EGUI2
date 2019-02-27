using System;
using System.IO;
using System.Reflection;

namespace EGUI
{
    public class MonoPropertyPersistence : CustomPersistence
    {
        private PropertyInfo mReflectedType;

        private PropertyInfo mName;

        public MonoPropertyPersistence(Persistence persistence) : base(persistence)
        {
            var type = persistentType;
            mReflectedType = type.GetProperty("ReflectedType");
            mName = type.GetProperty("Name");
        }

        public override Type persistentType { get { return FindType("System.Reflection.MonoProperty"); } }

        public override void Parse(object value, BinaryWriter writer)
        {
            SerializeType((Type)mReflectedType.GetValue(value, null), writer);
            Serialize(mName.GetValue(value, null), typeof(string), writer);
        }

        public override object Revert(BinaryReader reader)
        {
            var type = DeserializeType(reader);
            var name = "";
            Deserialize(reader, ret => name = (string)ret);
            return type.GetProperty(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}
