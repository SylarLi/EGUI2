﻿using System;
using System.Linq;
using UnityEngine;

namespace EGUI.Editor
{
    public class PersistentObject : Object
    {
        private object[] mValues;

        public object[] values { get { return mValues; } }

        private Type mType;

        public Type type { get { return mType; } }

        public PersistentObject(object[] values) : base()
        {
            Debug.Assert(values.Length > 0 && values.All(i => i != null), "Object can not be null.");
            Debug.Assert(values.All(i => i.GetType() == values[0].GetType()), "All objects should be the same type.");
            Debug.Assert(values[0].GetType().IsClass, "Object must be class, value type is not supported.");
            mValues = values;
            mType = mValues[0].GetType();
        }

        public PersistentProperty Find(string propertyPath)
        {
            return new PersistentProperty(this, propertyPath);
        }

        public T GetValue<T>()
        {
            Debug.Assert(typeof(T).IsAssignableFrom(type), string.Format("Type missmatch, {0} : {1}.", typeof(T).Name, type.Name));
            return (T)values[0];
        }

        public T[] GetValues<T>()
        {
            Debug.Assert(typeof(T).IsAssignableFrom(type), string.Format("Type missmatch, {0} : {1}.", typeof(T).Name, type.Name));
            return values.Select(i => (T)i).ToArray();
        }
    }
}
