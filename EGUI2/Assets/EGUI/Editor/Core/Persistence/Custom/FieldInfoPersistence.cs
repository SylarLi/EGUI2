using System;
using System.IO;
using System.Reflection;

namespace EGUI
{
    public class FieldInfoPersistence : CustomPersistence
    {
        public FieldInfoPersistence(Persistence persistence) : base(persistence) { }

        public override Type persistentType { get { return typeof(FieldInfo); } }

        public override void Parse(object value, BinaryWriter writer)
        {
            var fieldInfo = value as FieldInfo;
            SerializeType(fieldInfo.ReflectedType, writer);
            Serialize(fieldInfo.Name, typeof(string), writer);
            SerializeValue(fieldInfo.IsStatic, typeof(bool), writer);
            SerializeValue(fieldInfo.IsPublic, typeof(bool), writer);
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
