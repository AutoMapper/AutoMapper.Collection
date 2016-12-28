using System;
using System.Reflection;

namespace AutoMapper.Collection
{
    internal static class ReflectionHelper
    {
        public static object GetMemberValue(this MemberInfo propertyOrField, object target)
        {
            var property = propertyOrField as PropertyInfo;
            if (property != null)
                return property.GetValue(target, null);
            var field = propertyOrField as FieldInfo;
            if (field != null)
                return field.GetValue(target);
            throw Expected(propertyOrField);
        }

        private static ArgumentOutOfRangeException Expected(MemberInfo propertyOrField)
            => new ArgumentOutOfRangeException("propertyOrField",
                "Expected a property or field, not " + propertyOrField);

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).ReturnType;
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).PropertyType;
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).FieldType;
            return null;
        }
    }
}