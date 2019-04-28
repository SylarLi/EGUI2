using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EGUI
{
    public class DictionaryPersistence : CustomPersistence
    {
        public DictionaryPersistence(Persistence persistence) : base(persistence)
        {
        }

        public override Type persistentType
        {
            get { return typeof(Dictionary<,>); }
        }

        public override void Parse(object value, BinaryWriter writer)
        {
            PushCheckpoint(writer);
            var type = value.GetType();
            SerializeType(value.GetType(), writer);
            var itemTypes = type.GetGenericArguments();
            var keyType = itemTypes[0];
            var valType = itemTypes[1];
            var dictionary = value as IDictionary;
            writer.Write(dictionary.Keys.Count);
            foreach (var key in dictionary.Keys)
            {
                var trueType = key != null ? key.GetType() : keyType;
                Serialize(key, trueType, writer);
            }

            foreach (var val in dictionary.Values)
            {
                var trueType = val != null ? val.GetType() : valType;
                Serialize(val, trueType, writer);
            }
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
            var length = reader.ReadInt32();
            var dictionary = Activator.CreateInstance(type, length) as IDictionary;
            var keys = new object[length];
            var vals = new object[length];
            var count = 0;
            Action callback = () =>
            {
                if (++count == length * 2)
                {
                    for (var i = 0; i < keys.Length; i++)
                    {
                        dictionary.Add(keys[i], vals[i]);
                    }
                }
            };
            for (var i = 0; i < keys.Length; i++)
            {
                var index = i;
                Deserialize(reader, ret =>
                {
                    keys.SetValue(ret, index);
                    callback();
                });
            }

            for (var i = 0; i < vals.Length; i++)
            {
                var index = i;
                Deserialize(reader, ret =>
                {
                    vals.SetValue(ret, index);
                    callback();
                });
            }

            return dictionary;
        }
    }
}