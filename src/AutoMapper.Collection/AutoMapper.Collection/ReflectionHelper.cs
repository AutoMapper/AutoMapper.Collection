using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.EquivilencyExpression
{
    internal static class ReflectionHelper
    {
        public static IMemberGetter ToMemberGetter(this MemberInfo accessorCandidate)
        {
            if (accessorCandidate == null)
                return null;

            if (accessorCandidate is PropertyInfo)
                return new PropertyGetter((PropertyInfo)accessorCandidate);

            if (accessorCandidate is FieldInfo)
                return new FieldGetter((FieldInfo)accessorCandidate);

            if (accessorCandidate is MethodInfo)
                return new MethodGetter((MethodInfo)accessorCandidate);

            return null;
        }
    }
}