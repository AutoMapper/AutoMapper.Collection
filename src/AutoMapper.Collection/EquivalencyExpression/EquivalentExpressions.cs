using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;
using AutoMapper.Mappers;

namespace AutoMapper.EquivalencyExpression
{
    public static class EquivalentExpressions
    {
        private static readonly
            ConcurrentDictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, IEquivalentComparer>>
            EquivalentExpressionDictionary =
                new ConcurrentDictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, IEquivalentComparer>>();

        private static ConcurrentDictionary<TypePair, IEquivalentComparer> _equalityComparisonCache = new ConcurrentDictionary<TypePair, IEquivalentComparer>();

        private static readonly ConcurrentDictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>> GeneratePropertyMapsDictionary = new ConcurrentDictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>>();
        private static IList<IGeneratePropertyMaps> _generatePropertyMapsCache = new List<IGeneratePropertyMaps>();

        public static void AddCollectionMappers(this IMapperConfigurationExpression cfg)
        {
            cfg.InsertBefore<ReadOnlyCollectionMapper>(
                new ObjectToEquivalencyExpressionByEquivalencyExistingMapper(),
                new EquivalentExpressionAddRemoveCollectionMapper());
        }

        private static void InsertBefore<TObjectMapper>(this IMapperConfigurationExpression cfg, params IConfigurationObjectMapper[] adds)
            where TObjectMapper : IObjectMapper
        {
            var mappers = cfg.Mappers;
            var targetMapper = mappers.FirstOrDefault(om => om is TObjectMapper);
            var index = targetMapper == null ? 0 : mappers.IndexOf(targetMapper);
            foreach (var mapper in adds.Reverse())
                mappers.Insert(index, mapper);
            cfg.Advanced.BeforeSeal(c =>
            {
                foreach (var configurationObjectMapper in adds)
                    configurationObjectMapper.ConfigurationProvider = c;

                EquivalentExpressionDictionary.AddOrUpdate(c, _equalityComparisonCache, (type, old) => _equalityComparisonCache);
                _equalityComparisonCache = new ConcurrentDictionary<TypePair, IEquivalentComparer>();

                GeneratePropertyMapsDictionary.AddOrUpdate(c, _generatePropertyMapsCache, (type, old) => _generatePropertyMapsCache);
                _generatePropertyMapsCache = new List<IGeneratePropertyMaps>();
            });
        }

        internal static IEquivalentComparer GetEquivalentExpression(this IConfigurationObjectMapper mapper, Type sourceType, Type destinationType)
        {
            var typeMap = mapper.ConfigurationProvider.ResolveTypeMap(sourceType, destinationType);
            return typeMap == null ? null : GetEquivalentExpression(mapper.ConfigurationProvider, typeMap);
        }

        internal static IEquivalentComparer GetEquivalentExpression(IConfigurationProvider configurationProvider, TypeMap typeMap)
        {
            return EquivalentExpressionDictionary[configurationProvider].GetOrAdd(typeMap.Types,
                tp =>
                    GeneratePropertyMapsDictionary[configurationProvider].Select(_ =>_.GeneratePropertyMaps(typeMap).CreateEquivalentExpression()).FirstOrDefault(_ => _ != null));
        }

        /// <summary>
        /// Make Comparison between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="EquivalentExpression">Equivalent Expression between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparison<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> EquivalentExpression)
            where TSource : class
            where TDestination : class
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));
            _equalityComparisonCache.AddOrUpdate(typePair,
                new EquivalentExpression<TSource, TDestination>(EquivalentExpression),
                (type, old) => new EquivalentExpression<TSource, TDestination>(EquivalentExpression));
            return mappingExpression;
        }

        public static void SetGeneratePropertyMaps<TGeneratePropertyMaps>(this IMapperConfigurationExpression cfg)
            where TGeneratePropertyMaps : IGeneratePropertyMaps, new()
        {
            cfg.SetGeneratePropertyMaps(new TGeneratePropertyMaps());
        }

        public static void SetGeneratePropertyMaps(this IMapperConfigurationExpression cfg, IGeneratePropertyMaps generatePropertyMaps)
        {
            _generatePropertyMapsCache.Add(generatePropertyMaps);
        }

        private static IEquivalentComparer CreateEquivalentExpression(this IEnumerable<PropertyMap> propertyMaps)
        {
            if (!propertyMaps.Any() || propertyMaps.Any(pm => pm.DestinationProperty.GetMemberType() != pm.SourceMember.GetMemberType()))
                return null;
            var typeMap = propertyMaps.First().TypeMap;
            var srcType = typeMap.SourceType;
            var destType = typeMap.DestinationType;
            var srcExpr = Expression.Parameter(srcType, "src");
            var destExpr = Expression.Parameter(destType, "dest");

            var equalExpr = propertyMaps.Select(pm => SourceEqualsDestinationExpression(pm, srcExpr, destExpr)).ToList();
            if (!equalExpr.Any())
                return EquivalentExpression.BadValue;
            var finalExpression = equalExpr.Skip(1).Aggregate(equalExpr.First(), Expression.And);

            var expr = Expression.Lambda(finalExpression, srcExpr, destExpr);
            var genericExpressionType = typeof(EquivalentExpression<,>).MakeGenericType(srcType, destType);
            var equivilientExpression = Activator.CreateInstance(genericExpressionType, expr) as IEquivalentComparer;
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