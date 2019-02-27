using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EGUI
{
    /// <summary>
    /// 持久化
    /// 拥有Persistent标记的复合数据类型将会启用正向过滤: 只有拥有PersistentField的字段或public可读写属性才会被持久化，范围覆盖该类与其基类的所有字段
    /// 未拥有Persistent标记的复合数据类型，所有字段和public可读写属性都将会被持久化
    /// 支持: 原始数据类型/System.String/System.Type/UnityEngine.Object/以及其他复合类型数据结构，如果是类，必须拥有默认构造器
    /// 不支持: IntPtr/UIntPtr/delegate/index属性(例如public int this[int index])
    /// Auto-implemented属性会生成一个名为<propertyName>k__BackingField字段，带来各种意想不到的情况，请尽量避免使用
    /// </summary>
    public sealed class Persistence
    {
        private static readonly string UnityEditorResPath = "Library/unity editor resources";
        private static readonly string UnityDefaultResPath = "Library/unity default resources";
        private static readonly string UnityBuiltinExtraResPath = "Resources/unity_builtin_extra";

        private int mLookupIdSeed;

        private Dictionary<object, int> mLookupIdMap;

        private Dictionary<int, InverseLookup> mInverseLookupMap;

        private int mRegisterLookupIdSeed;

        private Dictionary<object, int> mRegisterLookupIdMap;

        private Dictionary<int, InverseLookup> mRegisterInverseLookupMap;

        private Dictionary<Type, CustomPersistence> mCustomPersistenceMap;

        public class InverseLookup
        {
            public delegate void LookupCallback(object reference);

            public LookupCallback callback = v => { };

            public object value;

            public bool settled;
        }

        public Persistence()
        {
            mLookupIdMap = new Dictionary<object, int>();
            mInverseLookupMap = new Dictionary<int, InverseLookup>();
            mRegisterLookupIdMap = new Dictionary<object, int>();
            mRegisterInverseLookupMap = new Dictionary<int, InverseLookup>();
            InitCustomPersistence();
        }

        private void InitCustomPersistence()
        {
            mCustomPersistenceMap = new Dictionary<Type, CustomPersistence>();
            var baseType = typeof(CustomPersistence);
            var customTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in customTypes)
            {
                var instance = (CustomPersistence)CreateInstance(type, new object[] { this });
                mCustomPersistenceMap.Add(instance.persistentType, instance);
            }
        }

        public void Register(object reference)
        {
            var id = ++mRegisterLookupIdSeed;
            mRegisterLookupIdMap[reference] = id;
            mRegisterInverseLookupMap[id] = new InverseLookup()
            {
                value = reference,
                settled = true,
            };
        }

        public void ClearRegister()
        {
            mRegisterLookupIdSeed = 0;
            mRegisterLookupIdMap.Clear();
            mRegisterInverseLookupMap.Clear();
        }

        public byte[] Serialize(object obj)
        {
            Debug.Assert(obj != null && !obj.GetType().IsInterface);
            mLookupIdSeed = 0;
            mLookupIdMap.Clear();
            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                Serialize(obj, obj.GetType(), writer);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        public void Serialize(object obj, Type type, BinaryWriter writer)
        {
            SerializeType(type, writer);
            if (obj is Type)
                SerializeType(obj as Type, writer);
            else if (type.IsClass)
                SerializeReference(obj, type, writer);
            else if (type.IsInterface)
                SerializeReference(null, type, writer);
            else if (type.IsValueType)
                SerializeValue(obj, type, writer);
            else
                throw new NotSupportedException("Invalid type: " + type.FullName);
        }

        public void SerializeStructure(object obj, Type type, BinaryWriter writer)
        {
            if (mCustomPersistenceMap.ContainsKey(type))
                mCustomPersistenceMap[type].Parse(obj, writer);
            else if (type.IsArray)
                SerializeArray(obj, type, writer);
            else if (type == typeof(string))
                writer.Write(obj as string);
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)) &&
                !type.IsSubclassOf(typeof(ScriptableObject)))
                SerializeUnityAsset(obj, type, writer);
            else
            {
                if (type.IsClass)
                {
                    var constuctor = type.GetConstructor(new Type[0]);
                    if (constuctor == null || !constuctor.IsPublic)
                    {
                        Debug.Log(type.FullName + " does not have a public default consturctor, it will be ignored and serialized as null.");
                        return;
                    }
                }
                SerializeConstruction(obj, true, type, writer);
            }
        }

        public void SerializeType(Type type, BinaryWriter writer)
        {
            if (mLookupIdMap.ContainsKey(type))
            {
                writer.Write((byte)1);
                writer.Write(mLookupIdMap[type]);
            }
            else if (mRegisterLookupIdMap.ContainsKey(type))
            {
                writer.Write((byte)2);
                writer.Write(mRegisterLookupIdMap[type]);
            }
            else
            {
                var id = ++mLookupIdSeed;
                mLookupIdMap[type] = id;
                writer.Write((byte)8);
                writer.Write(id);
                SerializeTypeRecur(type, writer);
                SerializeConstruction(null, false, type, writer);
            }
        }

        public void SerializeTypeRecur(Type type, BinaryWriter writer)
        {
            writer.Write(type.IsGenericType);
            if (type.IsArray)
            {
                writer.Write(type.FullName);
            }
            else if (type.IsGenericType)
            {
                writer.Write(type.GetGenericTypeDefinition().FullName);
                var genericArgs = type.GetGenericArguments();
                writer.Write(genericArgs.Length);
                for (int i = 0; i < genericArgs.Length; i++)
                {
                    SerializeTypeRecur(genericArgs[i], writer);
                }
            }
            else
            {
                writer.Write(type.FullName);
            }
        }

        public void SerializeArray(object value, Type type, BinaryWriter writer)
        {
            Debug.Assert(type.IsArray);
            var array = value as Array;
            writer.Write(array.Length);
            var elType = type.GetElementType();
            for (int i = 0; i < array.Length; i++)
            {
                var el = array.GetValue(i);
                var trueType = el != null ? el.GetType() : elType;
                Serialize(el, trueType, writer);
            }
        }

        public void SerializeConstruction(object obj, bool instance, Type type, BinaryWriter writer)
        {
            var fields = new List<FieldInfo>();
            var fieldFlags = BindingFlags.Public | BindingFlags.NonPublic;
            fieldFlags |= (instance ? BindingFlags.Instance : BindingFlags.Static);
            GetFields(fields, type, fieldFlags, true);
            var typeAttrs = type.GetCustomAttributes(typeof(PersistenceAttribute), false);
            if (typeAttrs != null && typeAttrs.Length > 0)
            {
                fields = fields.Where(f => {
                    var attributes = f.GetCustomAttributes(typeof(PersistentFieldAttribute), false);
                    return attributes != null && attributes.Length > 0;
                }).ToList();
            }
            fields = fields.Where(f => {
                return !f.IsLiteral && !f.IsInitOnly &&
                    !f.FieldType.IsSubclassOf(typeof(MulticastDelegate)) &&
                    f.FieldType != typeof(IntPtr) &&
                    f.FieldType != typeof(UIntPtr);
            }).ToList();
            writer.Write(fields.Count);
            foreach (var field in fields)
            {
                var val = field.GetValue(obj);
                var trueType = val != null ? val.GetType() : field.FieldType;
                writer.Write(field.Name);
                Serialize(val, trueType, writer);
            }

            var propFlags = BindingFlags.Public;
            propFlags |= (instance ? BindingFlags.Instance : BindingFlags.Static);
            var properties = type.GetProperties(propFlags);
            if (typeAttrs != null && typeAttrs.Length > 0)
            {
                properties = properties.Where(p => {
                    var attributes = p.GetCustomAttributes(typeof(PersistentFieldAttribute), false);
                    return attributes != null && attributes.Length > 0;
                }).ToArray();
            }
            properties = properties.Where(p => {
                return p.CanRead && p.CanWrite && 
                    p.GetIndexParameters().Length == 0 &&
                    !p.PropertyType.IsSubclassOf(typeof(MulticastDelegate)) &&
                    p.PropertyType != typeof(IntPtr) &&
                    p.PropertyType != typeof(UIntPtr);
            }).ToArray();
            writer.Write(properties.Length);
            foreach (var property in properties)
            {
                var val = property.GetValue(obj, null);
                var trueType = val != null ? val.GetType() : property.PropertyType;
                writer.Write(property.Name);
                Serialize(val, trueType, writer);
            }
        }

        public void SerializeValue(object value, Type type, BinaryWriter writer)
        {
            Debug.Assert(type.IsValueType);
            if (type.IsEnum)
                writer.Write((int)value);
            else if (value is bool)
                writer.Write((bool)value);
            else if (value is byte)
                writer.Write((byte)value);
            else if (value is char)
                writer.Write((char)value);
            else if (value is short)
                writer.Write((short)value);
            else if (value is ushort)
                writer.Write((ushort)value);
            else if (value is int)
                writer.Write((int)value);
            else if (value is uint)
                writer.Write((uint)value);
            else if (value is long)
                writer.Write((long)value);
            else if (value is ulong)
                writer.Write((ulong)value);
            else if (value is float)
                writer.Write((float)value);
            else if (value is double)
                writer.Write((double)value);
            else if (value is decimal)
                writer.Write((decimal)value);
            else
                SerializeStructure(value, type, writer);
        }

        public void SerializeReference(object reference, Type type, BinaryWriter writer)
        {
            Debug.Assert(type.IsClass);
            if (reference == null)
            {
                writer.Write((byte)0);
            }
            else if (mLookupIdMap.ContainsKey(reference))
            {
                writer.Write((byte)1);
                writer.Write(mLookupIdMap[reference]);
            }
            else if (mRegisterLookupIdMap.ContainsKey(reference))
            {
                writer.Write((byte)2);
                writer.Write(mRegisterLookupIdMap[reference]);
            }
            else
            {
                var id = ++mLookupIdSeed;
                mLookupIdMap[reference] = id;
                writer.Write((byte)8);
                writer.Write(id);
                SerializeStructure(reference, type, writer);
            }
        }

        public void SerializeUnityAsset(object obj, Type type, BinaryWriter writer)
        {
            var asset = obj as UnityEngine.Object; 
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
            Debug.Assert(!string.IsNullOrEmpty(assetPath), "Can not find asset: " + asset.name);
            if (assetPath.Equals(UnityEditorResPath))
            {
                writer.Write((byte)0);
                writer.Write(asset.name);
            }
            else if (assetPath.Equals(UnityDefaultResPath))
            {
                writer.Write((byte)1);
                writer.Write(asset.name);
            }
            else if (assetPath.Equals(UnityBuiltinExtraResPath))
            {
                writer.Write((byte)2);
                writer.Write(asset.name);
            }
            else
            {
                writer.Write((byte)8);
                writer.Write(UnityEditor.AssetDatabase.AssetPathToGUID(assetPath));
                writer.Write(assetPath);
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            mInverseLookupMap.Clear();
            var obj = default(T);
            using (var stream = new MemoryStream(bytes))
            {
                stream.Seek(0, SeekOrigin.Current);
                var reader = new BinaryReader(stream);
                Deserialize(reader, (ret) => obj = (T)ret);
            }
            return obj;
        }

        public void Deserialize(BinaryReader reader, InverseLookup.LookupCallback callback)
        {
            var type = DeserializeType(reader);
            if (type.IsSubclassOf(typeof(Type)))
                DeserializeType(reader);
            else if (type.IsClass)
                DeserializeReference(type, reader, callback);
            else if (type.IsValueType)
                callback(DeserializeValue(type, reader));
            else
                Debug.Assert(false, type.FullName);
        }

        public object DeserializeStructure(Type type, BinaryReader reader)
        {
            if (mCustomPersistenceMap.ContainsKey(type))
                return mCustomPersistenceMap[type].Revert(reader);
            else if (type.IsArray)
                return DeserializeArray(type, reader);
            else if (type == typeof(string))
                return reader.ReadString();
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)) &&
                !type.IsSubclassOf(typeof(ScriptableObject)))
                return DeserializeUnityAsset(type, reader);
            else
            {
                if (type.IsClass)
                {
                    var constuctor = type.GetConstructor(new Type[0]);
                    if (constuctor == null || !constuctor.IsPublic)
                    {
                        Debug.Log(type.FullName + " does not have a default public consturctor, it will be ignored and deserialized as null.");
                        return null;
                    }
                }
                return DeserializeConstruction(type, true, reader);
            }
        }

        public object DeserializeConstruction(Type type, bool instance, BinaryReader reader)
        {
            var obj = instance ? CreateInstance(type) : null;
            var fieldCount = reader.ReadInt32();
            for (int i = 0; i < fieldCount; i++)
            {
                var fieldName = reader.ReadString();
                var fieldFlags = BindingFlags.Public | BindingFlags.NonPublic;
                fieldFlags |= (instance ? BindingFlags.Instance : BindingFlags.Static);
                var field = GetField(type, fieldName, fieldFlags, true);
                Debug.Assert(field != null, type.FullName + "." + fieldName + " not exist.");
                Deserialize(reader, ret => {
                    if (field != null)
                        field.SetValue(obj, ret);
                });
            }
            var propertyCount = reader.ReadInt32();
            for (int i = 0; i < propertyCount; i++)
            {
                var propertyName = reader.ReadString();
                var propFlags = BindingFlags.Public;
                propFlags |= (instance ? BindingFlags.Instance : BindingFlags.Static);
                var property = type.GetProperty(propertyName, propFlags);
                Debug.Assert(property != null, type.FullName + "." + propertyName + " not exist.");
                Debug.Assert(property.CanWrite, type.FullName + "." + propertyName + " can not be written in.");
                Deserialize(reader, ret => {
                    if (property != null)
                        property.SetValue(obj, ret, null);
                });
            }
            return obj;
        }

        public object DeserializeArray(Type type, BinaryReader reader)
        {
            Debug.Assert(type.IsArray);
            var length = reader.ReadInt32();
            var obj = CreateInstance(type, new object[] { length }) as Array;
            for (int i = 0; i < obj.Length; i++)
            {
                var index = i;
                Deserialize(reader, ret => obj.SetValue(ret, index));
            }
            return obj;
        }

        public Type DeserializeType(BinaryReader reader)
        {
            int refType = reader.ReadByte();
            if (refType == 1)
            {
                var id = reader.ReadInt32();
                Debug.Assert(mInverseLookupMap.ContainsKey(id) && mInverseLookupMap[id].value != null);
                var type = mInverseLookupMap[id].value as Type;
                Debug.Assert(type != null);
                return type;
            }
            else if (refType == 2)
            {
                var id = reader.ReadInt32();
                Debug.Assert(mRegisterInverseLookupMap.ContainsKey(id) && mRegisterInverseLookupMap[id].value != null);
                var type = mRegisterInverseLookupMap[id].value as Type;
                Debug.Assert(type != null);
                return type;
            }
            else if (refType == 8)
            {
                var id = reader.ReadInt32();
                var type = DeserializeTypePure(reader);
                mInverseLookupMap.Add(id, new InverseLookup() { value = type });
                DeserializeConstruction(type, false, reader);
                return type;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public Type DeserializeTypePure(BinaryReader reader)
        {
            bool IsGenericType = reader.ReadBoolean();
            if (IsGenericType)
            {
                var genericTypeDefName = reader.ReadString();
                var genericArgsLen = reader.ReadInt32();
                var genericArgs = new Type[genericArgsLen];
                for (int i = 0; i < genericArgsLen; i++)
                {
                    genericArgs[i] = DeserializeTypePure(reader);
                } 
                var type = FindType(genericTypeDefName);
                type = type.MakeGenericType(genericArgs);
                return type;
            }
            else
            {
                var fullName = reader.ReadString();
                var type = FindType(fullName);
                return type;
            }
        }

        public object DeserializeValue(Type type, BinaryReader reader)
        {
            Debug.Assert(type.IsValueType);
            if (type.IsEnum)
                return Enum.ToObject(type, reader.ReadInt32());
            else if (type == typeof(bool))
                return reader.ReadBoolean();
            else if (type == typeof(byte))
                return reader.ReadByte();
            else if (type == typeof(char))
                return reader.ReadChar();
            else if (type == typeof(short))
                return reader.ReadInt16();
            else if (type == typeof(ushort))
                return reader.ReadUInt16();
            else if (type == typeof(int))
                return reader.ReadInt32();
            else if (type == typeof(uint))
                return reader.ReadUInt32();
            else if (type == typeof(long))
                return reader.ReadInt64();
            else if (type == typeof(ulong))
                return reader.ReadUInt64();
            else if (type == typeof(float))
                return reader.ReadSingle();
            else if (type == typeof(double))
                return reader.ReadDouble();
            else if (type == typeof(decimal))
                return reader.ReadDecimal();
            else
                return DeserializeStructure(type, reader);
        }

        public void DeserializeReference(Type type, BinaryReader reader, InverseLookup.LookupCallback callback)
        {
            Debug.Assert(type.IsClass);
            int refType = reader.ReadByte();
            if (refType == 0)
            {
                callback(null);
            }
            else if (refType == 1)
            {
                var id = reader.ReadInt32();
                if (mInverseLookupMap.ContainsKey(id) &&
                    mInverseLookupMap[id].settled)
                {
                    callback(mInverseLookupMap[id].value);
                }
                else
                {
                    if (!mInverseLookupMap.ContainsKey(id))
                        mInverseLookupMap.Add(id, new InverseLookup());
                    mInverseLookupMap[id].callback += callback;
                }
            }
            else if (refType == 2)
            {
                var id = reader.ReadInt32();
                Debug.Assert(mRegisterInverseLookupMap.ContainsKey(id) && mRegisterInverseLookupMap[id].value != null);
                callback(mRegisterInverseLookupMap[id].value);
            }
            else if (refType == 8)
            {
                var id = reader.ReadInt32();
                var reference = DeserializeStructure(type, reader);
                if (!mInverseLookupMap.ContainsKey(id))
                    mInverseLookupMap.Add(id, new InverseLookup());
                Debug.Assert(!mInverseLookupMap[id].settled);
                mInverseLookupMap[id].settled = true;
                mInverseLookupMap[id].value = reference;
                mInverseLookupMap[id].callback(reference);
                callback(reference);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public object DeserializeUnityAsset(Type type, BinaryReader reader)
        {
            var assetType = reader.ReadByte();
            UnityEngine.Object asset = null;
            if (assetType == 0)
            {
                var assetPath = reader.ReadString();
                asset = UnityEditor.EditorGUIUtility.Load(assetPath);
                Debug.Assert(asset, "Can not find asset: " + assetPath);
            }
            else if (assetType == 1)
            {
                var assetPath = reader.ReadString();
                asset = Resources.GetBuiltinResource(typeof(UnityEngine.Object), assetPath);
                Debug.Assert(asset, "Can not find asset: " + assetPath);
            }
            else if (assetType == 2)
            {
                var assetPath = reader.ReadString();
                asset = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/" + assetPath + ".psd");
                Debug.Assert(asset, "Can not find asset: " + assetPath);
            }
            else if (assetType == 8)
            {
                var assetGUID = reader.ReadString();
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUID);
                var backupPath = reader.ReadString();
                if (string.IsNullOrEmpty(assetPath))
                    assetPath = backupPath;
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, type);
                Debug.Assert(asset, "Can not find asset: " + assetPath);
            }
            else
            {
                throw new NotSupportedException("Invalid asset type: " + assetType);
            }
            return asset;
        }

        public object CreateInstance(Type type, object[] args = null)
        {
            if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                return ScriptableObject.CreateInstance(type);
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new NotSupportedException();
            }
            return Activator.CreateInstance(type, args);
        }

        public Type FindType(string name, bool throwOnError = false)
        {
            Type type = null;
            if (!string.IsNullOrEmpty(name))
            {
                type = Type.GetType(name);
                if (type == null)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        type = assembly.GetType(name);
                        if (type != null)
                            break;
                    }
                }
            }
            if (throwOnError && type == null)
                throw new NullReferenceException("Can not find type: " + name);
            return type;
        }

        public void GetFields(ICollection<FieldInfo> fields, Type type, BindingFlags flag, bool inherit)
        {
            var typeFields = type.GetFields(flag);
            foreach (var typeField in typeFields)
                fields.Add(typeField);
            if (inherit && type.BaseType != null)
                GetFields(fields, type.BaseType, flag, inherit);
        }

        public FieldInfo GetField(Type type, string name, BindingFlags flag, bool inherit)
        {
            var field = type.GetField(name, flag);
            if (field == null && inherit && type.BaseType != null)
                field = GetField(type.BaseType, name, flag, inherit);
            return field;
        }
    }
}