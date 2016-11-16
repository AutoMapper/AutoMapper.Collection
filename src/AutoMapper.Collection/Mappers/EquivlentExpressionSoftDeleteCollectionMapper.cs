using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;
using AutoMapper.EquivilencyExpression;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class EquivlentExpressionSoftDeleteCollectionMapper : IObjectMapper
    {
        private readonly CollectionMapper CollectionMapper = new CollectionMapper();

        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context)
            where TSource : IEnumerable<TSourceItem>
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null || destination == null)
                return destination;

            var equivilencyExpression = GetEquivilentExpression(new TypePair(typeof(TSource), typeof(TDestination))) as IEquivilentSoftDeleteExpression<TSourceItem,TDestinationItem>;
            var compareSourceToDestination = source.ToDictionary(s => s, s => destination.FirstOrDefault(d => equivilencyExpression.IsEquivlent(s, d)));

            foreach (var removedItem in destination.Except(compareSourceToDestination.Values).ToList())
                equivilencyExpression.SetSoftDeleteValue(removedItem);

            foreach (var keypair in compareSourceToDestination)
            {
                if (keypair.Value == null)
                    destination.Add(context.Mapper.Map<TDestinationItem>(keypair.Key));
                else
                    context.Mapper.Map(keypair.Key, keypair.Value);
            }

            return destination;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EquivlentExpressionSoftDeleteCollectionMapper).GetRuntimeMethods().First(_ => _.Name.Equals(nameof(Map)) && _.IsStatic);

        public bool IsMatch(TypePair typePair)
        {
            return typePair.SourceType.IsEnumerableType()
                   && typePair.DestinationType.IsCollectionType()
                   && GetEquivilentExpression(typePair) is IEquivilentSoftDeleteExpression;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var notNull = NotEqual(destExpression, Constant(null));
            var map = Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type), destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
                    sourceExpression, destExpression, contextExpression);
            var collectionMap = CollectionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression);

            return Condition(notNull, map, collectionMap);
        }

        private static IEquivilentExpression GetEquivilentExpression(TypePair typePair)
        {
            return EquivilentExpressions.GetEquivilentExpression(TypeHelper.GetElementType(typePair.SourceType), TypeHelper.GetElementType(typePair.DestinationType));
        }
    }
}
