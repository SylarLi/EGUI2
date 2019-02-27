using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EGUI
{
    public class ListPersistence : CustomPersistence
    {
        public ListPersistence(Persistence persistence) : base(persistence) { }

        public override Type persistentType { get { return typeof(List<>); } }

        public override void Parse(object value, BinaryWriter writer)
        {
            var itemType = persistentType.GetGenericArguments()[0];
            var list = value as IList;
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var trueType = item != null ? item.GetType() : itemType;
                Serialize(item, trueType, writer);
            }
        }

        public override object Revert(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var list = Activator.CreateInstance(persistentType, new object[] { length }) as IList;
            for (int i = 0; i < length; i++)
            {
                var index = i;
                list.Add(null);
                Deserialize(reader, ret => list[index] = ret);
            }
            return list;
        }
    }
}
