using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace EGUI.Editor
{
    public class SerializedProperty : Object
    {
        private Type mType;

        public Type type { get { return mType; } }

        private SerializedObject mSerializedObject;

        public SerializedObject serializedObject { get { return mSerializedObject; } }

        private string mPropertyPath;

        public string propertyPath { get { return mPropertyPath; } }

        private MemberInfo[] mMemberPath;

        public SerializedProperty(SerializedObject serializedObject, string propertyPath) : base()
        {
            Debug.Assert(serializedObject != null);
            Debug.Assert(!string.IsNullOrEmpty(propertyPath));
            mSerializedObject = serializedObject;
            mPropertyPath = propertyPath;
            mType = FindPropertyType(mSerializedObject, mPropertyPath);
        }

        public SerializedProperty Find(string relativePath)
        {
            return new SerializedProperty(serializedObject, Path.Combine(propertyPath, relativePath));
        }

        public T GetValue<T>()
        {
            Debug.Assert(typeof(T) == type);
            mMemberPath = mMemberPath ?? FindPropertyMemberPath(serializedObject, propertyPath);
            var value = serializedObject.values[0];
            foreach (var memberInfo in mMemberPath)
            {
                if (value == null)
                {
                    break;
                }
                value = EditorUtil.GetMemberValue(memberInfo, value);
            }
            return (T)value;
        }

        public T[] GetValues<T>()
        {
            Debug.Assert(typeof(T) == type);
            mMemberPath = mMemberPath ?? FindPropertyMemberPath(serializedObject, propertyPath);
            var values = serializedObject.values;
            foreach (var memberInfo in mMemberPath)
            {
                values = values.Select(i => i != null ? EditorUtil.GetMemberValue(memberInfo, i) : null).ToArray();
            }
            return values.Select(i => (T)i).ToArray();
        }

        public void SetValue<T>(T value)
        {
            Debug.Assert(typeof(T) == type);
            mMemberPath = mMemberPath ?? FindPropertyMemberPath(serializedObject, propertyPath);
            var list = new List<object[]>();
            list.Add(serializedObject.values);
            for (int i = 0; i < mMemberPath.Length - 1; i++)
            {
                list.Add(list[list.Count - 1].Select(v => v != null ? EditorUtil.GetMemberValue(mMemberPath[i], v) : null).ToArray());
            }
            ArraySetMemberValue(mMemberPath[mMemberPath.Length - 1], list[list.Count - 1], value);
            for (int i = list.Count - 1; i >= 1; i--)
            {
                if (list[i][0].GetType().IsValueType)
                {
                    ArraySetMemberValue(mMemberPath[i - 1], list[i - 1], list[i]);
                }
            }
        }

        private void ArraySetMemberValue(MemberInfo memberInfo, object[] targets, object value)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    EditorUtil.SetMemberValue(memberInfo, targets[i], value);
                }
            }
        }

        private void ArraySetMemberValue(MemberInfo memberInfo, object[] targets, object[] values)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    EditorUtil.SetMemberValue(memberInfo, targets[i], values[i]);
                }
            }
        }

        private Type FindPropertyType(SerializedObject serializedObject, string propertyPath)
        {
            var names = propertyPath.Split(new char[] { '\\', '/' });
            var currentType = serializedObject.type;
            int index = 0;
            while (currentType != null &&
                index < names.Length)
            {
                var memberInfo = EditorUtil.GetMemberInfo(
                    currentType,
                    names[index],
                    MemberTypes.Field | MemberTypes.Property,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty,
                    true);
                Debug.Assert(memberInfo != null, "Can not find member " + names[index] + " for type: " + currentType.FullName);
                currentType = EditorUtil.GetMemberType(memberInfo);
                index += 1;
            }
            Debug.Assert(currentType != null, "Can not find property: " + propertyPath);
            return currentType;
        }

        private MemberInfo[] FindPropertyMemberPath(SerializedObject serializedObject, string propertyPath)
        {
            var names = propertyPath.Split(new char[] { '\\', '/' });
            var path = new MemberInfo[names.Length];
            var currentType = serializedObject.type;
            int index = 0;
            while (currentType != null &&
                index < names.Length)
            {
                var memberInfo = EditorUtil.GetMemberInfo(
                    currentType,
                    names[index],
                    MemberTypes.Field | MemberTypes.Property,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty,
                    true);
                Debug.Assert(memberInfo != null, "Can not find member " + names[index] + " for type: " + currentType.FullName);
                path[index] = memberInfo;
                currentType = EditorUtil.GetMemberType(memberInfo);
                index += 1;
            }
            return path;
        }
    }
}
