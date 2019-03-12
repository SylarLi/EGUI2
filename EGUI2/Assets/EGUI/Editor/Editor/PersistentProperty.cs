using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EGUI.Editor
{
    public class PersistentProperty : Object
    {
        private Type mType;

        public Type type { get { return mType; } }

        private PersistentObject mPersistentObject;

        public PersistentObject persistentObject { get { return mPersistentObject; } }

        private string mPropertyPath;

        public string propertyPath { get { return mPropertyPath; } }

        private MemberInfo[] mMemberPath;

        public MemberInfo memberInfo { get { return mMemberPath[mMemberPath.Length - 1]; } }

        public string displayName { get { return EditorUtil.GetNiceDisplayName(memberInfo.Name); } }

        public bool hasMultipleDifferentValues
        {
            get
            {
                var values = GetValues<object>();
                return values.Any(i => !Equals(i, values[0]));
            }
        }

        public PersistentProperty(PersistentObject persistentObject, string propertyPath) : base()
        {
            Debug.Assert(persistentObject != null, "PersistentObject can not be null.");
            Debug.Assert(!string.IsNullOrEmpty(propertyPath), "Property path is not specified.");
            mPersistentObject = persistentObject;
            mPropertyPath = propertyPath;
            mMemberPath = CoreUtil.FindMemberPath(persistentObject.type, propertyPath);
            mType = CoreUtil.GetMemberType(memberInfo);
        }

        public PersistentProperty Find(string relativePath)
        {
            return new PersistentProperty(persistentObject, Path.Combine(propertyPath, relativePath));
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
    }
}
