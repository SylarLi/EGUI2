using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace EGUI
{
    public class MonoFieldPersistence : CustomPersistence
    {
        private PropertyInfo mReflectedType;

        private PropertyInfo mName;

        public MonoFieldPersistence(Persistence persistence) : base(persistence)
        {
            var type = persistentType;
            mReflectedType = type.GetProperty("ReflectedType");
            mName = type.GetProperty("Name");
        }

        public override Type persistentType
        {
            get { return CoreUtil.FindType("System.Reflection.MonoField"); }
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
            return type.GetField(name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}