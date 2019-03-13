using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace EGUI
{
    public sealed class CoreUtil
    {
        public static object CreateInstance(Type type, object[] args = null)
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

        public static Type FindType(string name, bool throwOnError = false)
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

        public static void GetFields(ICollection<FieldInfo> fields, Type type, BindingFlags flag, bool inherit)
        {
            var typeFields = type.GetFields(flag);
            foreach (var typeField in typeFields)
                fields.Add(typeField);
            if (inherit && type.BaseType != null)
                GetFields(fields, type.BaseType, flag, inherit);
        }

        public static FieldInfo GetField(Type type, string name, BindingFlags flag, bool inherit)
        {
            var field = type.GetField(name, flag);
            if (field == null && inherit && type.BaseType != null)
                field = GetField(type.BaseType, name, flag, inherit);
            return field;
        }

        public static MemberInfo GetMemberInfo(Type type, string name, MemberTypes memberTypes, BindingFlags flag, bool inherit)
        {
            MemberInfo member = null;
            var members = type.GetMember(name, memberTypes, flag);
            if (members.Length > 0)
                member = members[0];
            else if (inherit && type.BaseType != null)
                member = GetMemberInfo(type.BaseType, name, memberTypes, flag, inherit);
            return member;
        }

        public static Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
                return ((FieldInfo)memberInfo).FieldType;
            else if (memberInfo.MemberType == MemberTypes.Property)
                return ((PropertyInfo)memberInfo).PropertyType;
            else
                throw new NotSupportedException();
        }

        public static object GetMemberValue(MemberInfo memberInfo, object obj)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
                return ((FieldInfo)memberInfo).GetValue(obj);
            else if (memberInfo.MemberType == MemberTypes.Property)
                return ((PropertyInfo)memberInfo).GetValue(obj, null);
            else
                throw new NotSupportedException();
        }

        public static void SetMemberValue(MemberInfo memberInfo, object obj, object value)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
                ((FieldInfo)memberInfo).SetValue(obj, value);
            else if (memberInfo.MemberType == MemberTypes.Property)
                ((PropertyInfo)memberInfo).SetValue(obj, value, null);
            else
                throw new NotSupportedException();
        }

        public static MemberInfo[] FindMemberPath(Type type, string memberPath)
        {
            Debug.Assert(type != null, "Type can not be null.");
            Debug.Assert(!string.IsNullOrEmpty(memberPath), "MemberPath can not be empty.");
            var names = memberPath.Split(new char[] { '.' });
            var path = new MemberInfo[names.Length];
            int index = 0;
            while (type != null &&
                index < names.Length)
            {
                var memberInfo = GetMemberInfo(
                    type,
                    names[index],
                    MemberTypes.Field | MemberTypes.Property,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty,
                    true);
                Debug.Assert(memberInfo != null, "Can not find member " + names[index] + " for type: " + type.FullName);
                path[index] = memberInfo;
                type = GetMemberType(memberInfo);
                index += 1;
            }
            return path;
        }

        public static object GetMemberValue(MemberInfo[] memberPath, object obj)
        {
            foreach (var memberInfo in memberPath)
            {
                if (obj == null) break;
                obj = GetMemberValue(memberInfo, obj);
            }
            return obj;
        }

        public static void SetMemberValue(MemberInfo[] memberPath, object obj, object value)
        {
            var list = new List<object>() { obj };
            for (int i = 0; i < memberPath.Length - 1; i++)
            {
                var item = list[list.Count - 1] != null ? 
                    GetMemberValue(memberPath[i], list[list.Count - 1]) : null;
                list.Add(item);
            }
            SetMemberValue(memberPath[memberPath.Length - 1], list[list.Count - 1], value);
            for (int i = list.Count - 1; i >= 1; i--)
            {
                if (list[i].GetType().IsValueType)
                {
                    SetMemberValue(memberPath[i - 1], list[i - 1], list[i]);
                }
            }
        }
    }
}
