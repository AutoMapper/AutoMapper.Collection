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
        private static readonly
            IDictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, IEquivilentExpression>>
            EquivilentExpressionDictionary =
                new Dictionary<IConfigurationProvider, ConcurrentDictionary<TypePair, IEquivilentExpression>>();

        private static ConcurrentDictionary<TypePair, IEquivilentExpression> _equalityComparisionCache = new ConcurrentDictionary<TypePair, IEquivilentExpression>();

        private static readonly IDictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>> GeneratePropertyMapsDictionary = new Dictionary<IConfigurationProvider, IList<IGeneratePropertyMaps>>();
        private static IList<IGeneratePropertyMaps> _generatePropertyMapsCache = new List<IGeneratePropertyMaps>();

        public static void AddCollectionMappers(this IMapperConfigurationExpression cfg)
        {
            cfg.InsertBefore<ReadOnlyCollectionMapper>(
                new ObjectToEquivalencyExpressionByEquivalencyExistingMapper(),
                new EquivlentExpressionAddRemoveCollectionMapper());
        }

        private static void InsertBefore<TObjectMapper>(this IMapperConfigurationExpression cfg, params IConfigurationObjectMapper[] adds)
            where TObjectMapper : IObjectMapper
        {
            var mappers = cfg.Mappers;
            var targetMapper = mappers.FirstOrDefault(om => om is TObjectMapper);
            var index = targetMapper == null ? 0 : mappers.IndexOf(targetMapper);
            foreach (var mapper in adds.Reverse())
                mappers.Insert(index, mapper);
            cfg.Advanced.BeforeSeal = c =>
            {
                foreach (var configurationObjectMapper in adds)
                    configurationObjectMapper.ConfigurationProvider = c;

                EquivilentExpressionDictionary.Add(c, _equalityComparisionCache);
                _equalityComparisionCache = new ConcurrentDictionary<TypePair, IEquivilentExpression>();

                GeneratePropertyMapsDictionary.Add(c, _generatePropertyMapsCache);
                _generatePropertyMapsCache = new List<IGeneratePropertyMaps>();
            };
        }

        internal static IEquivilentExpression GetEquivilentExpression(this IConfigurationObjectMapper mapper, Type sourceType, Type destinationType)
        {
            var typeMap = mapper.ConfigurationProvider.ResolveTypeMap(sourceType, destinationType);
            return typeMap == null ? null : GetEquivilentExpression(mapper.ConfigurationProvider, typeMap);
        }
        
        internal static IEquivilentExpression GetEquivilentExpression(IConfigurationProvider configurationProvider, TypeMap typeMap)
        {
            return EquivilentExpressionDictionary[configurationProvider].GetOrAdd(typeMap.Types,
                tp =>
                    GeneratePropertyMapsDictionary[configurationProvider].Select(_ =>_.GeneratePropertyMaps(typeMap).CreateEquivilentExpression()).FirstOrDefault(_ => _ != null));
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
            _equalityComparisionCache.AddOrUpdate(typePair,
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
            _generatePropertyMapsCache.Add(generatePropertyMaps);
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