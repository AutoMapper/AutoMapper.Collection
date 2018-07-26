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
        private static readonly ConcurrentDictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, IEquivalentComparer>>
            _equivalentExpressionDictionary = new ConcurrentDictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, IEquivalentComparer>>();

        private static readonly ConcurrentDictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>>
            _generatePropertyMapsDictionary = new ConcurrentDictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>>();

        private static ConcurrentDictionary<TypePair, IEquivalentComparer>
            _equalityComparisonCache = new ConcurrentDictionary<TypePair, IEquivalentComparer>();

        private static IList<Func<Func<Type, object>, IGeneratePropertyMaps>>
            _generatePropertyMapsCache = new List<Func<Func<Type, object>, IGeneratePropertyMaps>>();

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
            {
                mappers.Insert(index, mapper);
            }

            cfg.Advanced.BeforeSeal(c =>
            {
                foreach (var configurationObjectMapper in adds)
                {
                    configurationObjectMapper.ConfigurationProvider = c;
                }

                var propertyMapsGenerators = _generatePropertyMapsCache.Select(x => x?.Invoke(c.ServiceCtor)).ToList();

                _equivalentExpressionDictionary.AddOrUpdate(c, _equalityComparisonCache, (_, __) => _equalityComparisonCache);
                _equalityComparisonCache = new ConcurrentDictionary<TypePair, IEquivalentComparer>();

                _generatePropertyMapsDictionary.AddOrUpdate(c, propertyMapsGenerators, (_, __) => propertyMapsGenerators);
                _generatePropertyMapsCache = new List<Func<Func<Type, object>, IGeneratePropertyMaps>>();
            });
        }

        internal static IEquivalentComparer GetEquivalentExpression(this IConfigurationObjectMapper mapper, Type sourceType, Type destinationType)
        {
            var typeMap = mapper.ConfigurationProvider.ResolveTypeMap(sourceType, destinationType);
            if (typeMap == null)
            {
                return null;
            }

            var comparer = GetEquivalentExpression(mapper.ConfigurationProvider, typeMap);
            if (comparer == null)
            {
                foreach (var item in typeMap.IncludedBaseTypes)
                {
                    var baseTypeMap = mapper.ConfigurationProvider.ResolveTypeMap(item.SourceType, item.DestinationType);
                    if (baseTypeMap == null)
                    {
                        continue;
                    }

                    comparer = GetEquivalentExpression(mapper.ConfigurationProvider, baseTypeMap);
                    if (comparer != null)
                    {
                        break;
                    }
                }
            }
            return comparer;
        }

        internal static IEquivalentComparer GetEquivalentExpression(IConfigurationProvider configurationProvider, TypeMap typeMap)
        {
            return _equivalentExpressionDictionary[configurationProvider].GetOrAdd(typeMap.Types, _
                => _generatePropertyMapsDictionary[configurationProvider]
                        .Select(x => x.GeneratePropertyMaps(typeMap).CreateEquivalentExpression())
                        .FirstOrDefault(x => x != null));
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
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));

            _equalityComparisonCache.AddOrUpdate(typePair,
                new EquivalentExpression<TSource, TDestination>(EquivalentExpression),
                (_, __) => new EquivalentExpression<TSource, TDestination>(EquivalentExpression));

            return mappingExpression;
        }

        public static void SetGeneratePropertyMaps<TGeneratePropertyMaps>(this IMapperConfigurationExpression cfg)
            where TGeneratePropertyMaps : IGeneratePropertyMaps
        {
            _generatePropertyMapsCache.Add(serviceCtor => (IGeneratePropertyMaps)serviceCtor(typeof(TGeneratePropertyMaps)));
        }

        public static void SetGeneratePropertyMaps(this IMapperConfigurationExpression cfg, IGeneratePropertyMaps generatePropertyMaps)
        {
            _generatePropertyMapsCache.Add(_ => generatePropertyMaps);
        }

        private static IEquivalentComparer CreateEquivalentExpression(this IEnumerable<PropertyMap> propertyMaps)
        {
            if (!propertyMaps.Any() || propertyMaps.Any(pm => pm.DestinationProperty.GetMemberType() != pm.SourceMember.GetMemberType()))
            {
                return null;
            }

            var typeMap = propertyMaps.First().TypeMap;
            var srcType = typeMap.SourceType;
            var destType = typeMap.DestinationType;
            var srcExpr = Expression.Parameter(srcType, "src");
            var destExpr = Expression.Parameter(destType, "dest");

            var equalExpr = propertyMaps.Select(pm => SourceEqualsDestinationExpression(pm, srcExpr, destExpr)).ToList();
            if (equalExpr.Count == 0)
            {
                return EquivalentExpression.BadValue;
            }

            var finalExpression = equalExpr.Skip(1).Aggregate(equalExpr[0], Expression.And);

            var expr = Expression.Lambda(finalExpression, srcExpr, destExpr);
            var genericExpressionType = typeof(EquivalentExpression<,>).MakeGenericType(srcType, destType);
            return Activator.CreateInstance(genericExpressionType, expr) as IEquivalentComparer;
        }

        private static BinaryExpression SourceEqualsDestinationExpression(PropertyMap propertyMap, Expression srcExpr, Expression destExpr)
        {
            var srcPropExpr = Expression.Property(srcExpr, propertyMap.SourceMember as PropertyInfo);
            var destPropExpr = Expression.Property(destExpr, propertyMap.DestinationProperty as PropertyInfo);
            return Expression.Equal(srcPropExpr, destPropExpr);
        }
    }
}