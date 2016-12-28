using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    public static class EquivilentExpressions
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IEquivilentExpression>> _equivilentExpressionDictionary = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IEquivilentExpression>>();
        
        internal static IEquivilentExpression GetEquivilentExpression(Type sourceType, Type destinationType)
        {
            ConcurrentDictionary<Type, IEquivilentExpression> destMap;
            IEquivilentExpression srcExpression;
            if (_equivilentExpressionDictionary.TryGetValue(destinationType, out destMap) && destMap.TryGetValue(sourceType, out srcExpression))
                return srcExpression;
            return null;
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
            var destinationDictionary = _equivilentExpressionDictionary.GetOrAdd(typeof(TDestination), t => new ConcurrentDictionary<Type, IEquivilentExpression>());
            destinationDictionary.AddOrUpdate(typeof(TSource), new EquivilentExpression<TSource, TDestination>(equivilentExpression), (type, old) => new EquivilentExpression<TSource, TDestination>(equivilentExpression));
            return mappingExpression;
        }

        public static void SetGeneratePropertyMaps<TGeneratePropertyMaps>(this IMapperConfigurationExpression cfg)
            where TGeneratePropertyMaps : IGeneratePropertyMaps, new()
        {
            cfg.SetGeneratePropertyMaps(new TGeneratePropertyMaps());
        }

        public static void SetGeneratePropertyMaps(this IMapperConfigurationExpression cfg, IGeneratePropertyMaps generatePropertyMaps)
        {
            cfg.ForAllMaps((tm, expression) =>
            {
                var pms = generatePropertyMaps.GeneratePropertyMaps(tm);
                if (pms.Any())
                {
                    var equiv = new GenerateEquivilentExpressionOnPropertyMaps(pms).GeneratEquivilentExpression(tm.SourceType, tm.DestinationType);
                    var destinationDictionary = _equivilentExpressionDictionary.GetOrAdd(tm.DestinationType, t => new ConcurrentDictionary<Type, IEquivilentExpression>());
                    destinationDictionary.AddOrUpdate(tm.SourceType, equiv, (type, old) => equiv);
                }
            });
        }
    }
}