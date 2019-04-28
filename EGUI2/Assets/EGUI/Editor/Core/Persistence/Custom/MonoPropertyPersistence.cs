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

        public override Type persistentType
        {
            get { return CoreUtil.FindType("System.Reflection.MonoProperty"); }
        }

        public override void Parse(object value, BinaryWriter writer)
        {
            PushCheckpoint(writer);
            SerializeType((Type) mReflectedType.GetValue(value, null), writer);
            writer.Write((string) mName.GetValue(value, null));
            PopCheckpoint(writer);
        }

        public override object Revert(BinaryReader reader)
        {
            SaveCheckpoint(reader);
            var type = DeserializeType(reader);
            if (type == null)
            {
                LoadCheckpoint(reader);
                return null;
            }
            var name = reader.ReadString();
            return type.GetProperty(name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}