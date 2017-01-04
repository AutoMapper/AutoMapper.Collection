using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Mappers;

namespace AutoMapper.EquivilencyExpression
{
    public static class EquivilentExpressions
    {
        private static readonly ConcurrentDictionary<TypePair, IEquivilentExpression> _equivilentExpressionDictionary = new ConcurrentDictionary<TypePair, IEquivilentExpression>();
        private static readonly IList<IGeneratePropertyMaps> GeneratePropertyMaps = new List<IGeneratePropertyMaps>();

        private static IConfigurationProvider ConfigurationProvider { get; set; }

        public static void AddCollectionMappers(this IMapperConfigurationExpression cfg)
        {
            cfg.Advanced.BeforeSeal = c => ConfigurationProvider = c;
            cfg.Mappers.InsertBefore<ReadOnlyCollectionMapper>(
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

        internal static IEquivilentExpression GetEquivilentExpression(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);
            var typeMap = ConfigurationProvider.ResolveTypeMap(typePair);
            return _equivilentExpressionDictionary.GetOrAdd(typePair,
                tp =>
                    GeneratePropertyMaps.Select(_ =>_.GeneratePropertyMaps(typeMap).CreateEquivilentExpression()).FirstOrDefault(_ => _ != null));
        }

        /// <summary>
        /// Make Comparison between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="equivilentExpression">Equivilent Expression between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparision<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> equivilentExpression) 
            where TSource : class 
            where TDestination : class
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            _equivilentExpressionDictionary.AddOrUpdate(typePair,
                new EquivilentExpression<TSource, TDestination>(equivilentExpression),
                (type, old) => new EquivilentExpression<TSource, TDestination>(equivilentExpression));
            return mappingExpression;
        }

        public static void SetGeneratePropertyMaps<TGeneratePropertyMaps>(this IMapperConfigurationExpression cfg)
            where TGeneratePropertyMaps : IGeneratePropertyMaps, new()
        {
            cfg.SetGeneratePropertyMaps(new TGeneratePropertyMaps());
        }

        public static void SetGeneratePropertyMaps(this IMapperConfigurationExpression cfg, IGeneratePropertyMaps generatePropertyMaps)
        {
            GeneratePropertyMaps.Add(generatePropertyMaps);
        }
        
        private static IEquivilentExpression CreateEquivilentExpression(this IEnumerable<PropertyMap> propertyMaps)
        {
            if (!propertyMaps.Any())
                return null;
            var typeMap = propertyMaps.First().TypeMap;
            var srcType = typeMap.SourceType;
            var destType = typeMap.DestinationType;
            var srcExpr = Expression.Parameter(srcType, "src");
            var destExpr = Expression.Parameter(destType, "dest");

            var equalExpr = propertyMaps.Select(pm => SourceEqualsDestinationExpression(pm, srcExpr, destExpr)).ToList();
            if (!equalExpr.Any())
                return EquivilentExpression.BadValue;
            var finalExpression = equalExpr.Skip(1).Aggregate(equalExpr.First(), Expression.And);

            var expr = Expression.Lambda(finalExpression, srcExpr, destExpr);
            var genericExpressionType = typeof(EquivilentExpression<,>).MakeGenericType(srcType, destType);
            var equivilientExpression = Activator.CreateInstance(genericExpressionType, expr) as IEquivilentExpression;
            return equivilientExpression;
        }

        private static BinaryExpression SourceEqualsDestinationExpression(PropertyMap propertyMap, Expression srcExpr, Expression destExpr)
        {
            var srcPropExpr = Expression.Property(srcExpr, propertyMap.SourceMember as PropertyInfo);
            var destPropExpr = Expression.Property(destExpr, propertyMap.DestinationProperty as PropertyInfo);
            return Expression.Equal(srcPropExpr, destPropExpr);
        }
    }
}