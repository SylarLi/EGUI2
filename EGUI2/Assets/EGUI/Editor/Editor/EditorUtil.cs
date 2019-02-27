using System;
using System.Reflection;

namespace EGUI.Editor
{
    public sealed class EditorUtil
    {
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
    }
}
