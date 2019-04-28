using System;
using System.IO;
using System.Reflection;

namespace EGUI
{
    public class PropertyInfoPersistence : CustomPersistence
    {
        public PropertyInfoPersistence(Persistence persistence) : base(persistence)
        {
        }

        public override Type persistentType
        {
            get { return typeof(PropertyInfo); }
        }

        public override void Parse(object value, BinaryWriter writer)
        {
            PushCheckpoint(writer);
            var propertyInfo = value as PropertyInfo;
            SerializeType(propertyInfo.ReflectedType, writer);
            writer.Write(propertyInfo.Name);
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