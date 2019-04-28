using System;
using System.IO;
using System.Reflection;

namespace EGUI
{
    public class FieldInfoPersistence : CustomPersistence
    {
        public FieldInfoPersistence(Persistence persistence) : base(persistence)
        {
        }

        public override Type persistentType
        {
            get { return typeof(FieldInfo); }
        }

        public override void Parse(object value, BinaryWriter writer)
        {
            PushCheckpoint(writer);
            var fieldInfo = value as FieldInfo;
            SerializeType(fieldInfo.ReflectedType, writer);
            writer.Write(fieldInfo.Name);
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