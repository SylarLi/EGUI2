using System;
using System.Linq;
using UnityEngine;

namespace EGUI.Editor
{
    public class SerializedObject : Object
    {
        private object[] mValues;

        public object[] values { get { return mValues; } }

        private Type mType;

        public Type type { get { return mType; } }

        public SerializedObject(object[] values) : base()
        {
            Debug.Assert(values.Length > 0 && values.All(i => i != null));
            Debug.Assert(values.All(i => i.GetType() == values[0].GetType()));
            Debug.Assert(values[0].GetType().IsClass);
            mValues = values;
            mType = mValues[0].GetType();
        }

        public SerializedProperty Find(string propertyPath)
        {
            return new SerializedProperty(this, propertyPath);
        }

        public T GetValue<T>()
        {
            Debug.Assert(typeof(T) == type);
            return (T)values[0];
        }

        public T[] GetValues<T>()
        {
            Debug.Assert(typeof(T) == type);
            return values.Select(i => (T)i).ToArray();
        }
    }
}
