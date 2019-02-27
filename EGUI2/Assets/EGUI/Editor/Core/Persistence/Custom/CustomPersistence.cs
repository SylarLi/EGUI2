using System;
using System.IO;

namespace EGUI
{
    public abstract class CustomPersistence
    {
        private Persistence persistence;

        public CustomPersistence(Persistence persistence)
        {
            this.persistence = persistence;
        }

        public virtual Type persistentType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual void Parse(object value, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public virtual object Revert(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        protected void Serialize(object obj, Type type, BinaryWriter writer)
        {
            persistence.Serialize(obj, type, writer);
        }

        protected void Deserialize(BinaryReader reader, Persistence.InverseLookup.LookupCallback callback)
        {
            persistence.Deserialize(reader, callback);
        }

        protected void SerializeConstruction(object obj, bool instance, Type type, BinaryWriter writer)
        {
            persistence.SerializeConstruction(obj, instance, type, writer);
        }

        protected object DeserializeConstruction(Type type, bool instance, BinaryReader reader)
        {
            return persistence.DeserializeConstruction(type, instance, reader);
        }

        protected void SerializeUnityAsset(object obj, Type type, BinaryWriter writer)
        {
            persistence.SerializeUnityAsset(obj, type, writer);
        }

        protected object DeserializeUnityAsset(Type type, BinaryReader reader)
        {
            return persistence.DeserializeUnityAsset(type, reader);
        }

        protected void SerializeValue(object value, Type type, BinaryWriter writer)
        {
            persistence.SerializeValue(value, type, writer);
        }

        protected object DeserializeValue(Type type, BinaryReader reader)
        {
            return persistence.DeserializeValue(type, reader);
        }

        protected void SerializeReference(object reference, Type type, BinaryWriter writer)
        {
            persistence.SerializeReference(reference, type, writer);
        }

        protected void DeserializeReference(Type type, BinaryReader reader, Persistence.InverseLookup.LookupCallback callback)
        {
            persistence.DeserializeReference(type, reader, callback);
        }

        protected void SerializeType(Type type, BinaryWriter writer)
        {
            persistence.SerializeType(type, writer);
        }

        protected Type DeserializeType(BinaryReader reader)
        {
            return persistence.DeserializeType(reader);
        }

        protected void SerializeArray(object value, Type type, BinaryWriter writer)
        {
            persistence.SerializeArray(value, type, writer);
        }

        protected object DeserializeArray(Type type, BinaryReader reader)
        {
            return persistence.DeserializeArray(type, reader);
        }

        protected Type FindType(string name)
        {
            return persistence.FindType(name);
        }
    }
}
