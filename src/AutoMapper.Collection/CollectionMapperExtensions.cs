using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Mappers;

namespace AutoMapper
{
    public static class CollectionMapperExtensions
    {
        public static void AddCollectionMappers(this IList<IObjectMapper> mappers)
        {
            mappers.InsertBefore<ReadOnlyCollectionMapper>(
                new ObjectToEquivalencyExpressionByEquivalencyExistingMapper(),
                new EquivlentExpressionAddRemoveCollectionMapper());
        }

        private static void InsertBefore<TObjectMapper>(this IList<IObjectMapper> mappers, params IObjectMapper[] adds)
            where TObjectMapper : IObjectMapper
        {
            var targetMapper = mappers.FirstOrDefault(om => om is TObjectMapper);
            var index = targetMapper == null ? 0 : mappers.IndexOf(targetMapper);
            foreach (var mapper in adds.Reverse())
                mappers.Insert(index, mapper);
        }

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