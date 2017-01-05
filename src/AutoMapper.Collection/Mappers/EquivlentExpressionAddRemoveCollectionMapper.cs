using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;
using AutoMapper.EquivilencyExpression;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class EquivlentExpressionAddRemoveCollectionMapper : IConfigurationObjectMapper
    {
        private readonly CollectionMapper CollectionMapper = new CollectionMapper();

        public IConfigurationProvider ConfigurationProvider { get; set; }

        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, IEquivilentExpression<TSourceItem, TDestinationItem> equivilencyExpression)
            where TSource : IEnumerable<TSourceItem>
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null || destination == null)
                return destination;

            var compareSourceToDestination = source.ToDictionary(s => s, s => destination.FirstOrDefault(d => equivilencyExpression.IsEquivlent(s, d)));

            foreach (var removedItem in destination.Except(compareSourceToDestination.Values).ToList())
                destination.Remove(removedItem);

            foreach (var keypair in compareSourceToDestination)
            {
                if (keypair.Value == null)
                    destination.Add(context.Mapper.Map<TDestinationItem>(keypair.Key));
                else
                    context.Mapper.Map(keypair.Key, keypair.Value);
            }

            return destination;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EquivlentExpressionAddRemoveCollectionMapper).GetRuntimeMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair typePair)
        {
            return typePair.SourceType.IsEnumerableType()
                   && typePair.DestinationType.IsCollectionType()
                   && this.GetEquivilentExpression(TypeHelper.GetElementType(typePair.SourceType), TypeHelper.GetElementType(typePair.DestinationType)) != null;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var notNull = NotEqual(destExpression, Constant(null));
            var equivilencyExpression = this.GetEquivilentExpression(TypeHelper.GetElementType(sourceExpression.Type), TypeHelper.GetElementType(destExpression.Type));
            var map = Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type), destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
                    sourceExpression, destExpression, contextExpression, Constant(equivilencyExpression));
            var collectionMap = CollectionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression);

            return Condition(notNull, map, collectionMap);
        }
    }
}
