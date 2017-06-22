using System;
using System.Linq.Expressions;
using AutoMapper.Collection.Internal;
using AutoMapper.Collection.Internal.Extensions;
using AutoMapper.Collection.Internal.Mapper;
using AutoMapper.Mappers;

namespace AutoMapper.EquivalencyExpression
{
    public static class EquivalentExpressions
    {
        public static void AddCollectionMappers(this IMapperConfigurationExpression cfg)
        {
            cfg.InsertBefore<ReadOnlyCollectionMapper>(
                new ObjectToEquivalencyExpressionByEquivalencyExistingMapper(),
                new EquivalentExpressionAddRemoveCollectionMapper());
        }

        /// <summary>
        ///     Make Comparison between <typeparamref name="TSource" /> and <typeparamref name="TDestination" />
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="equivalentExpression">
        ///     Equivalent Expression between <typeparamref name="TSource" /> and
        ///     <typeparamref name="TDestination" />
        /// </param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparison<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> equivalentExpression)
            where TSource : class
            where TDestination : class
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));

            var collectionExpression = new EqualityCollectionExpression<TSource, TDestination>(equivalentExpression);
            var expressionCollectionMapper = new ExpressionCollectionMapper<TSource, TDestination>(collectionExpression);

            CollectionMapperCache.AddOrUpdate(typePair, expressionCollectionMapper);

            return mappingExpression;
        }

        /// <summary>
        ///     Make Comparison between <typeparamref name="TSource" /> and <typeparamref name="TDestination" /> based on the
        ///     return object.
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="sourceProperty">
        ///     Source property that should be used for mapping. if property is object the property on the
        ///     object is used for mapping.
        /// </param>
        /// <param name="destinationProperty">
        ///     Destination property that should be used for mapping. if property is object the
        ///     property on the object is used for mapping.
        /// </param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparison<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, object>> sourceProperty, Expression<Func<TDestination, object>> destinationProperty)
            where TSource : class
            where TDestination : class
        {
            var typePair = new TypePair(typeof(TSource), typeof(TDestination));

            var collectionExpression = new ObjectCollectionExpression<TSource, TDestination>(sourceProperty, destinationProperty);
            var expressionCollectionMapper = new ExpressionCollectionMapper<TSource, TDestination>(collectionExpression);

            CollectionMapperCache.AddOrUpdate(typePair, expressionCollectionMapper);

            return mappingExpression;
        }

        public static void SetGeneratePropertyMaps<TGeneratePropertyMaps>(this IMapperConfigurationExpression cfg)
            where TGeneratePropertyMaps : IGeneratePropertyMaps, new()
        {
            cfg.SetGeneratePropertyMaps(new TGeneratePropertyMaps());
        }

        public static void SetGeneratePropertyMaps(this IMapperConfigurationExpression cfg, IGeneratePropertyMaps generatePropertyMaps)
        {
            GeneratePropertyMapsCache.AddGeneratePropertyMaps(generatePropertyMaps);
        }
    }
}
