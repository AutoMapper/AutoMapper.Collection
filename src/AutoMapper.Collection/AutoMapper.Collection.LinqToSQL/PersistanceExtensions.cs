using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Reflection;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.Collection.LinqToSQL
{
    public static class PersistanceExtensions
    {
        public static IPersistance Persist<TSource>(this Table<TSource> source)
            where TSource : class
        {
            return new Persistance<TSource>(source, Mapper.Engine);
        }

        public static IEnumerable For<TSource>(this IQueryDataSourceInjection<TSource> source, Type destType)
        {
            var forMethod = source.GetType().GetMethod("For").MakeGenericMethod(destType);
            var listType = typeof(List<>).MakeGenericType(destType);
            var forResult = forMethod.Invoke(source, new object[] { null });
            var enumeratedResult = Activator.CreateInstance(listType, forResult);
            return enumeratedResult as IEnumerable;
        }
    }
}