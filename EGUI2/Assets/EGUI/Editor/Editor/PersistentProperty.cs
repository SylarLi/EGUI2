using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EGUI.Editor
{
    public class PersistentProperty
    {
        private Type mType;

        public Type type { get { return mType; } }

        private PersistentObject mPersistentObject;

        public PersistentObject persistentObject { get { return mPersistentObject; } }

        private string mPropertyPath;

        public string propertyPath { get { return mPropertyPath; } }

        private MemberInfo[] mMemberPath;

        public MemberInfo memberInfo { get { return mMemberPath[mMemberPath.Length - 1]; } }

        public string displayName { get { return UserUtil.GetNiceDisplayName(memberInfo.Name); } }

        public bool hasMultipleDifferentValues
        {
            get
            {
                var values = GetValues<object>();
                return values.Any(i => !Equals(i, values[0]));
            }
        }

        public int length
        {
            get
            {
                return persistentObject.values.Length;
            }
        }

        public bool exist
        {
            get
            {
                return GetValues<object>().All(i => i != null);
            }
        }

        public PersistentProperty(PersistentObject persistentObject, string propertyPath)
        {
            Debug.Assert(persistentObject != null, "PersistentObject can not be null.");
            Debug.Assert(!string.IsNullOrEmpty(propertyPath), "Property path is not specified.");
            mPersistentObject = persistentObject;
            mPropertyPath = propertyPath;
            mMemberPath = CoreUtil.FindMemberPath(persistentObject.type, propertyPath);
            foreach (var memberInfo in mMemberPath)
            {
                if (memberInfo.MemberType == MemberTypes.Property)
                    Debug.Assert(((PropertyInfo)memberInfo).CanRead);
            }
            mType = CoreUtil.GetMemberType(memberInfo);
        }

        public PersistentProperty Find(string relativePath)
        {
            return new PersistentProperty(persistentObject, propertyPath + "." + relativePath);
        }

        public PersistentProperty[] Find(string[] relativePaths)
        {
            return (from path in relativePaths
                    select Find(path))
                    .ToArray();
        }

        public PersistentProperty[] ListChildren()
        {
            var members = new List<MemberInfo>();
            UserUtil.GetDisplayedMembersInType(members, type);
            return (from m in members
                   select new PersistentProperty(persistentObject, propertyPath + "." + m.Name)).ToArray();
        }

        public T GetValue<T>()
        {
            Debug.Assert(typeof(T).IsAssignableFrom(type), string.Format("Type missmatch, {0} : {1}.", typeof(T).Name, type.Name));
            return (T)CoreUtil.GetMemberValue(mMemberPath, persistentObject.values[0]);
        }

        public T[] GetValues<T>()
        {
            Debug.Assert(typeof(T).IsAssignableFrom(type), string.Format("Type missmatch, {0} : {1}.", typeof(T).Name, type.Name));
            var a = persistentObject.values.Select(i => (T)CoreUtil.GetMemberValue(mMemberPath, i)).ToArray();
            return a;
        }

        public void SetValue<T>(T value)
        {
            Debug.Assert(typeof(T).IsAssignableFrom(type), string.Format("Type missmatch, {0} : {1}.", typeof(T).Name, type.Name));
            foreach (var memberInfo in mMemberPath)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                    Debug.Assert(!((FieldInfo)memberInfo).IsLiteral && !((FieldInfo)memberInfo).IsInitOnly);
                else if (memberInfo.MemberType == MemberTypes.Property)
                    Debug.Assert(((PropertyInfo)memberInfo).CanWrite);
            }
            var objects = persistentObject.GetValues<object>();
            var commands = objects.Select(obj => new UpdateMemberCommand(obj, propertyPath, value)).ToArray();
            Command.Execute(new CombinedCommand(commands));
        }

        public void SetValues<T>(T[] values)
        {
            Debug.Assert(typeof(T).IsAssignableFrom(type), string.Format("Type missmatch, {0} : {1}.", typeof(T).Name, type.Name));
            foreach (var memberInfo in mMemberPath)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                    Debug.Assert(!((FieldInfo)memberInfo).IsLiteral && !((FieldInfo)memberInfo).IsInitOnly);
                else if (memberInfo.MemberType == MemberTypes.Property)
                    Debug.Assert(((PropertyInfo)memberInfo).CanWrite);
            }
            var objects = persistentObject.GetValues<object>();
            Debug.Assert(values.Length == objects.Length);
            var commands = new UpdateMemberCommand[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                commands[i] = new UpdateMemberCommand(objects[i], propertyPath, values[i]);
            }
            Command.Execute(new CombinedCommand(commands));
        }

        public void ResizeArray(int length)
        {
            foreach (var memberInfo in mMemberPath)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                    Debug.Assert(!((FieldInfo)memberInfo).IsInitOnly);
                else if (memberInfo.MemberType == MemberTypes.Property)
                    Debug.Assert(((PropertyInfo)memberInfo).CanWrite);
            }
            var objects = persistentObject.GetValues<object>();
            var commands = new List<UpdateMemberCommand>();
            var arrays = GetValues<Array>();
            for (int i = 0; i < arrays.Length; i++)
            {
                var array = arrays[i];
                if (array.Length != length)
                {
                    var newArray = Array.CreateInstance(type.GetElementType(), length);
                    Array.ConstrainedCopy(array, 0, newArray, 0, Mathf.Min(array.Length, length));
                    commands.Add(new UpdateMemberCommand(objects[i], propertyPath, newArray));
                }
            }
            if (commands.Count > 0)
            {
                Command.Execute(new CombinedCommand(commands.ToArray()));
            }
        }
    }
}
