using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Collection.Configuration;
using AutoMapper.Collection.Runtime;
using AutoMapper.Internal;
using AutoMapper.Internal.Mappers;
using AutoMapper.Mappers;

namespace AutoMapper.EquivalencyExpression
{
    public static class EquivalentExpressions
    {
        public static void AddCollectionMappers(this IMapperConfigurationExpression cfg)
        {
            var mapper = new ObjectToEquivalencyExpressionByEquivalencyExistingMapper();
            cfg.Internal().Features.Set(new GeneratePropertyMapsExpressionFeature(mapper));
            cfg.Internal().InsertBefore<CollectionMapper>(
                mapper,
                new EquivalentExpressionAddRemoveCollectionMapper());
        }

        private static void InsertBefore<TObjectMapper>(this IMapperConfigurationExpression cfg, params IObjectMapper[] adds)
            where TObjectMapper : IObjectMapper
        {
            var mappers = cfg.Internal().Mappers;
            var targetMapper = mappers.FirstOrDefault(om => om is TObjectMapper);
            var index = targetMapper == null ? 0 : mappers.IndexOf(targetMapper);
            foreach (var mapper in adds.Reverse())
            {
                mappers.Insert(index, mapper);
            }
        }

        internal static IEquivalentComparer GetEquivalentExpression(this IObjectMapper mapper, Type sourceType,
            Type destinationType, IConfigurationProvider configuration)
        {
            var typeMap = configuration.Internal().ResolveTypeMap(sourceType, destinationType);
            if (typeMap == null)
            {
                return null;
            }

            var comparer = GetEquivalentExpression(configuration, typeMap);
            if (comparer == null)
            {
                foreach (var item in typeMap.IncludedBaseTypes)
                {
                    var baseTypeMap = configuration.Internal().ResolveTypeMap(item.SourceType, item.DestinationType);
                    if (baseTypeMap == null)
                    {
                        continue;
                    }

                    comparer = GetEquivalentExpression(configuration, baseTypeMap);
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
            return typeMap.Features.Get<CollectionMappingFeature>()?.EquivalentComparer
                ?? configurationProvider.Internal().Features.Get<GeneratePropertyMapsFeature>().Get(typeMap);
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
            mappingExpression.Features.Set(new CollectionMappingExpressionFeature<TSource, TDestination>(EquivalentExpression));
            return mappingExpression;
        }

        public static void SetGeneratePropertyMaps<TGeneratePropertyMaps>(this IMapperConfigurationExpression cfg)
            where TGeneratePropertyMaps : IGeneratePropertyMaps
        {
            (cfg.Internal().Features.Get<GeneratePropertyMapsExpressionFeature>()
                ?? throw new ArgumentException("Invoke the IMapperConfigurationExpression.AddCollectionMappers() before adding IGeneratePropertyMaps."))
                .Add(serviceCtor => (IGeneratePropertyMaps)serviceCtor(typeof(TGeneratePropertyMaps)));
        }

        public static void SetGeneratePropertyMaps(this IMapperConfigurationExpression cfg, IGeneratePropertyMaps generatePropertyMaps)
        {
            (cfg.Internal().Features.Get<GeneratePropertyMapsExpressionFeature>()
                ?? throw new ArgumentException("Invoke the IMapperConfigurationExpression.AddCollectionMappers() before adding IGeneratePropertyMaps."))
                .Add(_ => generatePropertyMaps);
        }
    }
}