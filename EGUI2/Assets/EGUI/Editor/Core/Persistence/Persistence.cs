using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EGUI.Editor;
using UnityEditor;
using UnityEngine;

namespace EGUI
{
    /// <summary>
    /// 持久化
    /// 拥有Persistent标记的复合数据类型将会启用正向过滤: 只有拥有PersistentField的可读写 字段/public属性 才会被持久化，范围覆盖该类与其基类的所有字段
    /// 未拥有Persistent标记的复合数据类型，所有可读写 字段/public属性 都将会被持久化
    /// 支持: 原始数据类型/System.String/System.Type/Unity持久化资源(内存资源不支持)/以及其他自定义复合类型数据结构，如果是类，必须拥有默认构造器，否则反序列化会失败
    /// 不支持: 指针/IntPtr/UIntPtr/delegate/index属性(例如public int this[int index])
    /// Auto-implemented属性会生成一个名为{propertyName}k__BackingField字段，带来各种意想不到的情况，请尽量避免使用
    /// 可继承CustomPersistence进行自定义扩展
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

        private Dictionary<Type, CustomPersistence> mCustomGenericPersistenceMap;

        private Stack<long> mCheckpoints;

        private Cache mCache;

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
            mCheckpoints = new Stack<long>();
            mCache = new Cache();
            InitCustomPersistence();
        }

        private void InitCustomPersistence()
        {
            mCustomPersistenceMap = new Dictionary<Type, CustomPersistence>();
            mCustomGenericPersistenceMap = new Dictionary<Type, CustomPersistence>();
            var customTypes = CoreUtil.FindSubTypes(typeof(CustomPersistence));
            foreach (var type in customTypes)
            {
                var instance = (CustomPersistence) CoreUtil.CreateInstance(type, new object[] {this});
                if (instance.persistentType.IsGenericTypeDefinition)
                    mCustomGenericPersistenceMap.Add(instance.persistentType, instance);
                else
                    mCustomPersistenceMap.Add(instance.persistentType, instance);
            }
        }

        private CustomPersistence GetCustomPersistence(Type type)
        {
            if (mCustomPersistenceMap.ContainsKey(type))
                return mCustomPersistenceMap[type];
            if (type.IsGenericType)
            {
                if (type.IsGenericTypeDefinition && mCustomGenericPersistenceMap.ContainsKey(type))
                    return mCustomGenericPersistenceMap[type];
                var typeGenericDef = type.GetGenericTypeDefinition();
                foreach (var pair in mCustomGenericPersistenceMap)
                {
                    if (typeGenericDef == pair.Key)
                        return pair.Value;
                }
            }

            return null;
        }

        public void Register(object reference)
        {
            var id = ++mRegisterLookupIdSeed;
            mRegisterLookupIdMap[reference] = id;
            mRegisterInverseLookupMap[id] = new InverseLookup
            {
                value = reference,
                settled = true
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
            if (obj == null)
                throw new NullReferenceException();
            var objType = obj.GetType();
            if (objType.IsInterface)
                throw new NotSupportedException();
            mLookupIdSeed = 0;
            mLookupIdMap.Clear();
            mCheckpoints.Clear();
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                Serialize(obj, objType, writer);
                bytes = stream.ToArray();
            }

            return bytes;
        }

        public void Serialize(object obj, Type type, BinaryWriter writer)
        {
            PushCheckpoint(writer);
            SerializeType(type, writer);
            var typeObj = obj as Type;
            if (typeObj != null)
                SerializeType(typeObj, writer);
            else if (type.IsClass)
                SerializeReference(obj, type, writer);
            else if (type.IsValueType)
                SerializeValue(obj, type, writer);
            else
                throw new NotSupportedException("Invalid type: " + type.FullName);
            PopCheckpoint(writer);
        }

        public void SerializeStructure(object obj, Type type, BinaryWriter writer)
        {
            var customPersistence = GetCustomPersistence(type);
            if (customPersistence != null)
                customPersistence.Parse(obj, writer);
            else if (type.IsArray)
                SerializeArray(obj, type, writer);
            else if (type == typeof(string))
                writer.Write((string) obj);
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                SerializeUnityAsset(obj, type, writer);
            else
            {
                if (type.IsClass)
                {
                    var constructor = type.GetConstructor(new Type[0]);
                    if (constructor == null || !constructor.IsPublic)
                    {
                        Debug.LogError(type.FullName +
                                       " does not have a public default constructor, it will be ignored and serialized as null.");
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
                writer.Write((byte) 1);
                writer.Write(mLookupIdMap[type]);
            }
            else if (mRegisterLookupIdMap.ContainsKey(type))
            {
                writer.Write((byte) 2);
                writer.Write(mRegisterLookupIdMap[type]);
            }
            else
            {
                var id = ++mLookupIdSeed;
                mLookupIdMap[type] = id;
                writer.Write((byte) 8);
                writer.Write(id);
                SerializeTypeRecur(type, writer);
                SerializeConstruction(null, false, type, writer);
            }
        }

        public void SerializeTypeRecur(Type type, BinaryWriter writer)
        {
            writer.Write(type.IsGenericType);
            if (type.IsGenericType)
            {
                writer.Write(type.GetGenericTypeDefinition().FullName);
                var genericArgs = type.GetGenericArguments();
                writer.Write(genericArgs.Length);
                foreach (var genericArg in genericArgs)
                {
                    SerializeTypeRecur(genericArg, writer);
                }
            }
            else
            {
                writer.Write(type.FullName);
            }
        }

        public void SerializeArray(object value, Type type, BinaryWriter writer)
        {
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
            var fields = mCache.GetFields(type, instance);
            writer.Write(fields.Length);
            foreach (var field in fields)
            {
                var val = field.GetValue(obj);
                var trueType = val != null ? val.GetType() : field.FieldType;
                writer.Write(field.Name);
                Serialize(val, trueType, writer);
            }

            var props = mCache.GetProps(type, instance);
            writer.Write(props.Length);
            foreach (var property in props)
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
                writer.Write((int) value);
            else if (value is bool)
                writer.Write((bool) value);
            else if (value is byte)
                writer.Write((byte) value);
            else if (value is char)
                writer.Write((char) value);
            else if (value is short)
                writer.Write((short) value);
            else if (value is ushort)
                writer.Write((ushort) value);
            else if (value is int)
                writer.Write((int) value);
            else if (value is uint)
                writer.Write((uint) value);
            else if (value is long)
                writer.Write((long) value);
            else if (value is ulong)
                writer.Write((ulong) value);
            else if (value is float)
                writer.Write((float) value);
            else if (value is double)
                writer.Write((double) value);
            else if (value is decimal)
                writer.Write((decimal) value);
            else
                SerializeStructure(value, type, writer);
        }

        public void SerializeReference(object reference, Type type, BinaryWriter writer)
        {
            Debug.Assert(type.IsClass);
            if (reference == null)
            {
                writer.Write((byte) 0);
            }
            else if (mLookupIdMap.ContainsKey(reference))
            {
                writer.Write((byte) 1);
                writer.Write(mLookupIdMap[reference]);
            }
            else if (mRegisterLookupIdMap.ContainsKey(reference))
            {
                writer.Write((byte) 2);
                writer.Write(mRegisterLookupIdMap[reference]);
            }
            else
            {
                var id = ++mLookupIdSeed;
                mLookupIdMap[reference] = id;
                writer.Write((byte) 8);
                writer.Write(id);
                SerializeStructure(reference, type, writer);
            }
        }

        public void SerializeUnityAsset(object obj, Type type, BinaryWriter writer)
        {
            var asset = obj as UnityEngine.Object;
            var assetPath = AssetDatabase.GetAssetPath(asset);
            Debug.Assert(!string.IsNullOrEmpty(assetPath), "Can not find asset: " + asset.name);
            if (assetPath.Equals(UnityEditorResPath))
            {
                writer.Write((byte) 0);
                writer.Write(asset.name);
            }
            else if (assetPath.Equals(UnityDefaultResPath))
            {
                writer.Write((byte) 1);
                writer.Write(asset.name);
            }
            else if (assetPath.Equals(UnityBuiltinExtraResPath))
            {
                writer.Write((byte) 2);
                writer.Write(asset.name);
            }
            else
            {
                writer.Write((byte) 8);
                writer.Write(AssetDatabase.AssetPathToGUID(assetPath));
                writer.Write(assetPath);
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return (T) Deserialize(bytes);
        }

        public object Deserialize(byte[] bytes)
        {
            mCheckpoints.Clear();
            mInverseLookupMap.Clear();
            object obj = null;
            using (var stream = new MemoryStream(bytes))
            {
                stream.Seek(0, SeekOrigin.Current);
                var reader = new BinaryReader(stream);
                Deserialize(reader, ret => obj = ret);
            }

            return obj;
        }

        public void Deserialize(BinaryReader reader, InverseLookup.LookupCallback callback)
        {
            SaveCheckpoint(reader);
            var type = DeserializeType(reader);
            if (type == null)
            {
                LoadCheckpoint(reader);
                callback(null);
            }
            else if (type.IsSubclassOf(typeof(Type)))
                callback(DeserializeType(reader));
            else if (type.IsClass)
                DeserializeReference(type, reader, callback);
            else if (type.IsValueType)
                callback(DeserializeValue(type, reader));
            else
                throw new NotSupportedException();
        }

        public object DeserializeStructure(Type type, BinaryReader reader)
        {
            var customPersistence = GetCustomPersistence(type);
            if (customPersistence != null)
                return customPersistence.Revert(reader);
            if (type == typeof(string))
                return reader.ReadString();
            if (type.IsArray)
                return DeserializeArray(type, reader);
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                return DeserializeUnityAsset(type, reader);
            if (type.IsClass)
            {
                var constructor = type.GetConstructor(new Type[0]);
                if (constructor == null || !constructor.IsPublic)
                {
                    Debug.Log(type.FullName +
                              " does not have a default public constructor, it will be ignored and deserialized as null.");
                    return null;
                }
            }

            return DeserializeConstruction(type, true, reader);
        }

        public object DeserializeConstruction(Type type, bool instance, BinaryReader reader)
        {
            var obj = instance ? CoreUtil.CreateInstance(type) : null;
            var fieldCount = reader.ReadInt32();
            for (var i = 0; i < fieldCount; i++)
            {
                var fieldName = reader.ReadString();
                var field = mCache.GetField(type, fieldName, instance);
                Debug.Assert(field != null,
                    type.FullName + "." + fieldName + " is not exist or writable.");
                Deserialize(reader, ret =>
                {
                    if (field != null)
                        field.SetValue(obj, ret);
                });
            }

            var propertyCount = reader.ReadInt32();
            for (var i = 0; i < propertyCount; i++)
            {
                var propertyName = reader.ReadString();
                var property = mCache.GetProp(type, propertyName, instance);
                Debug.Assert(property != null,
                    type.FullName + "." + propertyName + " is not exist or writable.");
                Deserialize(reader, ret =>
                {
                    if (property != null)
                        property.SetValue(obj, ret, null);
                });
            }

            return obj;
        }

        public object DeserializeArray(Type type, BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var obj = CoreUtil.CreateInstance(type, new object[] {length}) as Array;
            for (var i = 0; i < obj.Length; i++)
            {
                var index = i;
                Deserialize(reader, ret => obj.SetValue(ret, index));
            }

            return obj;
        }

        public Type DeserializeType(BinaryReader reader)
        {
            int refType = reader.ReadByte();
            switch (refType)
            {
                case 1:
                {
                    var id = reader.ReadInt32();
                    return mInverseLookupMap.ContainsKey(id) && mInverseLookupMap[id] != null
                        ? mInverseLookupMap[id].value as Type
                        : null;
                }
                case 2:
                {
                    var id = reader.ReadInt32();
                    return mRegisterInverseLookupMap.ContainsKey(id) && mRegisterInverseLookupMap[id] != null
                        ? mRegisterInverseLookupMap[id].value as Type
                        : null;
                }
                case 8:
                {
                    var id = reader.ReadInt32();
                    var type = DeserializeTypePure(reader);
                    if (type != null)
                    {
                        mInverseLookupMap.Add(id, new InverseLookup {value = type});
                        DeserializeConstruction(type, false, reader);
                    }

                    return type;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public Type DeserializeTypePure(BinaryReader reader)
        {
            var IsGenericType = reader.ReadBoolean();
            if (IsGenericType)
            {
                var genericTypeDefName = reader.ReadString();
                var type = mCache.FindType(genericTypeDefName);
                if (type == null)
                {
                    Debug.LogError("Type missing: " + genericTypeDefName);
                    return null;
                }

                var genericArgsLen = reader.ReadInt32();
                var genericArgs = new Type[genericArgsLen];
                for (var i = 0; i < genericArgsLen; i++)
                {
                    genericArgs[i] = DeserializeTypePure(reader);
                    if (genericArgs[i] == null)
                        return null;
                }

                type = type.MakeGenericType(genericArgs);
                return type;
            }
            else
            {
                var fullName = reader.ReadString();
                var type = mCache.FindType(fullName);
                if (type == null)
                    Debug.Log("Type missing: " + fullName);
                return type;
            }
        }

        public object DeserializeValue(Type type, BinaryReader reader)
        {
            if (type.IsEnum)
                return Enum.ToObject(type, reader.ReadInt32());
            if (type == typeof(bool))
                return reader.ReadBoolean();
            if (type == typeof(byte))
                return reader.ReadByte();
            if (type == typeof(char))
                return reader.ReadChar();
            if (type == typeof(short))
                return reader.ReadInt16();
            if (type == typeof(ushort))
                return reader.ReadUInt16();
            if (type == typeof(int))
                return reader.ReadInt32();
            if (type == typeof(uint))
                return reader.ReadUInt32();
            if (type == typeof(long))
                return reader.ReadInt64();
            if (type == typeof(ulong))
                return reader.ReadUInt64();
            if (type == typeof(float))
                return reader.ReadSingle();
            if (type == typeof(double))
                return reader.ReadDouble();
            if (type == typeof(decimal))
                return reader.ReadDecimal();
            return DeserializeStructure(type, reader);
        }

        public void DeserializeReference(Type type, BinaryReader reader, InverseLookup.LookupCallback callback)
        {
            int refType = reader.ReadByte();
            switch (refType)
            {
                case 0:
                    callback(null);
                    break;
                case 1:
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

                    break;
                }
                case 2:
                {
                    var id = reader.ReadInt32();
                    callback(mRegisterInverseLookupMap[id].value);
                    break;
                }
                case 8:
                {
                    var id = reader.ReadInt32();
                    var reference = DeserializeStructure(type, reader);
                    if (!mInverseLookupMap.ContainsKey(id))
                        mInverseLookupMap.Add(id, new InverseLookup());
                    mInverseLookupMap[id].settled = true;
                    mInverseLookupMap[id].value = reference;
                    mInverseLookupMap[id].callback(reference);
                    callback(reference);
                    break;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public object DeserializeUnityAsset(Type type, BinaryReader reader)
        {
            UnityEngine.Object asset;
            var assetType = reader.ReadByte();
            if (assetType == 0)
            {
                var assetPath = reader.ReadString();
                asset = EditorGUIUtility.Load(assetPath);
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
                asset = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/" + assetPath + ".psd");
                Debug.Assert(asset, "Can not find asset: " + assetPath);
            }
            else if (assetType == 8)
            {
                var assetGUID = reader.ReadString();
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                var backupPath = reader.ReadString();
                if (string.IsNullOrEmpty(assetPath))
                    assetPath = backupPath;
                asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                Debug.Assert(asset, "Can not find asset: " + assetPath);
            }
            else
            {
                throw new NotSupportedException("Invalid asset type: " + assetType);
            }

            return asset;
        }

        public void PushCheckpoint(BinaryWriter writer)
        {
            mCheckpoints.Push(writer.BaseStream.Position);
            writer.Write(0L);
        }

        public void PopCheckpoint(BinaryWriter writer)
        {
            var point = mCheckpoints.Pop();
            var current = writer.BaseStream.Position;
            writer.BaseStream.Position = point;
            writer.Write(current);
            writer.BaseStream.Position = current;
        }

        public void SaveCheckpoint(BinaryReader reader)
        {
            mCheckpoints.Push(reader.ReadInt64());
        }

        public void LoadCheckpoint(BinaryReader reader)
        {
            reader.BaseStream.Position = mCheckpoints.Pop();
        }

        private class Cache
        {
            private Dictionary<string, Type> typeMap = new Dictionary<string, Type>();

            private Dictionary<string, FieldInfo[]> fieldsMap = new Dictionary<string, FieldInfo[]>();

            private Dictionary<string, PropertyInfo[]> propsMap = new Dictionary<string, PropertyInfo[]>();

            private Dictionary<string, FieldInfo> fieldMap = new Dictionary<string, FieldInfo>();

            private Dictionary<string, PropertyInfo> propMap = new Dictionary<string, PropertyInfo>();

            public Type FindType(string fullName)
            {
                if (!typeMap.ContainsKey(fullName))
                    typeMap.Add(fullName, CoreUtil.FindType(fullName));
                return typeMap[fullName];
            }

            public FieldInfo[] GetFields(Type type, bool instance)
            {
                var fullName = type.FullName;
                if (!instance) fullName += "$static$";
                if (!fieldsMap.ContainsKey(fullName))
                {
                    var list = new List<FieldInfo>();
                    var flag = BindingFlags.Public | BindingFlags.NonPublic;
                    flag |= instance ? BindingFlags.Instance : BindingFlags.Static;
                    CoreUtil.GetFields(list, type, flag, true);
                    var fields = (IEnumerable<FieldInfo>) list;
                    var typeAttrs = type.GetCustomAttributes(typeof(PersistenceAttribute), false);
                    if (typeAttrs.Length > 0)
                    {
                        fields = fields.Where(f =>
                        {
                            var attributes = f.GetCustomAttributes(typeof(PersistentFieldAttribute), false);
                            return attributes.Length > 0;
                        });
                    }

                    fields = fields.Where(f =>
                        !f.IsLiteral && !f.IsInitOnly && CoreUtil.IsSafetyReflectionType(f.FieldType));
                    fieldsMap.Add(fullName, fields.ToArray());
                }

                return fieldsMap[fullName];
            }

            public PropertyInfo[] GetProps(Type type, bool instance)
            {
                var fullName = type.FullName;
                if (!instance) fullName += "$static$";
                if (!propsMap.ContainsKey(fullName))
                {
                    var list = new List<PropertyInfo>();
                    var flag = BindingFlags.Public;
                    flag |= (instance ? BindingFlags.Instance : BindingFlags.Static);
                    CoreUtil.GetProperties(list, type, flag, true);
                    var props = (IEnumerable<PropertyInfo>) list;
                    var typeAttrs = type.GetCustomAttributes(typeof(PersistenceAttribute), false);
                    if (typeAttrs.Length > 0)
                    {
                        props = props.Where(p =>
                        {
                            var attributes = p.GetCustomAttributes(typeof(PersistentFieldAttribute), false);
                            return attributes.Length > 0;
                        });
                    }

                    props = props.Where(p => p.CanRead && p.CanWrite &&
                                             p.GetIndexParameters().Length == 0 &&
                                             CoreUtil.IsSafetyReflectionType(p.PropertyType));
                    propsMap.Add(fullName, props.ToArray());
                }

                return propsMap[fullName];
            }

            public FieldInfo GetField(Type type, string name, bool instance)
            {
                var fullName = type.FullName + "." + name;
                if (!instance) fullName += "$static$";
                if (!fieldMap.ContainsKey(fullName))
                {
                    var flag = BindingFlags.Public | BindingFlags.NonPublic;
                    flag |= (instance ? BindingFlags.Instance : BindingFlags.Static);
                    var field = CoreUtil.GetField(type, name, flag, true);
                    if (field != null && !field.IsLiteral && !field.IsInitOnly)
                        fieldMap.Add(fullName, field);
                    else
                        fieldMap.Add(fullName, null);
                }

                return fieldMap[fullName];
            }

            public PropertyInfo GetProp(Type type, string name, bool instance)
            {
                var fullName = type.FullName + "." + name;
                if (!instance) fullName += "$static$";
                if (!propMap.ContainsKey(fullName))
                {
                    var flag = BindingFlags.Public | BindingFlags.NonPublic;
                    flag |= (instance ? BindingFlags.Instance : BindingFlags.Static);
                    var prop = CoreUtil.GetProperty(type, name, flag, true);
                    if (prop != null && prop.CanWrite)
                        propMap.Add(fullName, prop);
                    else
                        propMap.Add(fullName, null);
                }

                return propMap[fullName];
            }
        }
    }
}