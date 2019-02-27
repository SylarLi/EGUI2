using System;
using System.IO;
using System.Reflection;

namespace EGUI
{
    public class MonoFieldPersistence : CustomPersistence
    {
        private PropertyInfo mReflectedType;

        private PropertyInfo mName;

        private PropertyInfo mIsStatic;

        private PropertyInfo mIsPublic;

        public MonoFieldPersistence(Persistence persistence) : base(persistence)
        {
            var type = persistentType;
            mReflectedType = type.GetProperty("ReflectedType");
            mName = type.GetProperty("Name");
            mIsStatic = type.GetProperty("IsStatic");
            mIsPublic = type.GetProperty("IsPublic");
        }

        public override Type persistentType { get { return FindType("System.Reflection.MonoField"); } }

        public override void Parse(object value, BinaryWriter writer)
        {
            SerializeType((Type)mReflectedType.GetValue(value, null), writer);
            Serialize(mName.GetValue(value, null), typeof(string), writer);
            SerializeValue(mIsStatic.GetValue(value, null), typeof(bool), writer);
            SerializeValue(mIsPublic.GetValue(value, null), typeof(bool), writer);
        }

        public override object Revert(BinaryReader reader)
        {
            var type = DeserializeType(reader);
            var name = "";
            Deserialize(reader, ret => name = (string)ret);
            var isStatic = (bool)DeserializeValue(typeof(bool), reader);
            var isPublic = (bool)DeserializeValue(typeof(bool), reader);
            return type.GetField(name, (isStatic ? BindingFlags.Static : BindingFlags.Instance) | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic));
        }
    }
}
