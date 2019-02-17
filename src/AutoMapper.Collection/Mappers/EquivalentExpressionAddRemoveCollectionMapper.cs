using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;
using AutoMapper.EquivalencyExpression;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    public class EquivalentExpressionAddRemoveCollectionMapper : IConfigurationObjectMapper
    {
        private readonly CollectionMapper CollectionMapper = new CollectionMapper();

        public IConfigurationProvider ConfigurationProvider { get; set; }

        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, IEquivalentComparer equivalentComparer)
            where TSource : IEnumerable<TSourceItem>
            where TDestination : ICollection<TDestinationItem>
        {
            if (source == null || destination == null)
            {
                return destination;
            }

            var destList = destination.ToLookup(x => equivalentComparer.GetHashCode(x)).ToDictionary(x => x.Key, x => x.ToList());

            var items = source.Select(x =>
            {
                var sourceHash = equivalentComparer.GetHashCode(x);

                var item = default(TDestinationItem);
                List<TDestinationItem> itemList;
                if (destList.TryGetValue(sourceHash, out itemList))
                {
                    item = itemList.FirstOrDefault(dest => equivalentComparer.IsEquivalent(x, dest));
                    if (item != null)
                    {
                        itemList.Remove(item);
                    }
                }
                return new { SourceItem = x, DestinationItem = item };
            });

            foreach (var keypair in items)
            {
                if (keypair.DestinationItem == null)
                {
                    destination.Add((TDestinationItem)context.Mapper.Map(keypair.SourceItem, null, typeof(TSourceItem), typeof(TDestinationItem), context));
                }
                else
                {
                    context.Mapper.Map(keypair.SourceItem, keypair.DestinationItem, context);
                }
            }

            foreach (var removedItem in destList.SelectMany(x => x.Value))
            {
                destination.Remove(removedItem);
            }

            return destination;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EquivalentExpressionAddRemoveCollectionMapper).GetRuntimeMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair typePair)
        {
            if (typePair.SourceType.IsEnumerableType()
                   && typePair.DestinationType.IsCollectionType())
            {
                var realType = new TypePair(TypeHelper.GetElementType(typePair.SourceType), TypeHelper.GetElementType(typePair.DestinationType));

                return realType != typePair
                    && this.GetEquivalentExpression(realType.SourceType, realType.DestinationType) != null;
            }

            return false;
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap,
            Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceType = TypeHelper.GetElementType(sourceExpression.Type);
            var destType = TypeHelper.GetElementType(destExpression.Type);

            var method = MapMethodInfo.MakeGenericMethod(sourceExpression.Type, sourceType, destExpression.Type, destType);
            var equivalencyExpression = this.GetEquivalentExpression(sourceType, destType);

            var equivalencyExpressionConst = Constant(equivalencyExpression);
            var map = Call(null, method, sourceExpression, destExpression, contextExpression, equivalencyExpressionConst);

            var notNull = NotEqual(destExpression, Constant(null));
            var collectionMap = CollectionMapper.MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression, contextExpression);
            return Condition(notNull, map, Convert(collectionMap, destExpression.Type));
        }
    }
}
