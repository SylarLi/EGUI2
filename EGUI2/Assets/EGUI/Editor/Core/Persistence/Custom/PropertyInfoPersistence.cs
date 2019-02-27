using System;
using System.IO;
using System.Reflection;

namespace EGUI
{
    public class PropertyInfoPersistence : CustomPersistence
    {
        public PropertyInfoPersistence(Persistence persistence) : base(persistence) { }

        public override Type persistentType { get { return typeof(PropertyInfo); } }

        public override void Parse(object value, BinaryWriter writer)
        {
            var propertyInfo = value as PropertyInfo;
            SerializeType(propertyInfo.ReflectedType, writer);
            Serialize(propertyInfo.Name, typeof(string), writer);
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
