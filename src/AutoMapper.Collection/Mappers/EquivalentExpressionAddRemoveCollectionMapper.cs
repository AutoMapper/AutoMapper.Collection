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
        private readonly CollectionMapper _collectionMapper = new CollectionMapper();

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

        private static readonly MethodInfo _mapMethodInfo = typeof(EquivalentExpressionAddRemoveCollectionMapper).GetRuntimeMethods().Single(_ => _.IsStatic && _.Name == nameof(Map));

        public bool IsMatch(TypePair typePair)
        {
            return typePair.SourceType.IsEnumerableType()
                   && typePair.DestinationType.IsCollectionType();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap,
            Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            IObjectMapper nextMapper = _collectionMapper;
            var typePair = new TypePair(sourceExpression.Type, destExpression.Type);
            var mappers = new List<IObjectMapper>(configurationProvider.GetMappers());
            for (var i = mappers.IndexOf(this) + 1; i < mappers.Count; i++)
            {
                var mapper = mappers[i];
                if (mapper.IsMatch(typePair))
                {
                    nextMapper = mapper;
                    break;
                }
            }
            var nextMapperExpression = nextMapper.MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression, contextExpression);

            var sourceType = TypeHelper.GetElementType(sourceExpression.Type);
            var destType = TypeHelper.GetElementType(destExpression.Type);

            var equivalencyExpression = this.GetEquivalentExpression(sourceType, destType);
            if (equivalencyExpression == null)
            {
                return nextMapperExpression;
            }

            var method = _mapMethodInfo.MakeGenericMethod(sourceExpression.Type, sourceType, destExpression.Type, destType);
            var map = Call(null, method, sourceExpression, destExpression, contextExpression, Constant(equivalencyExpression));

            var notNull = NotEqual(destExpression, Constant(null));
            return Condition(notNull, map, Convert(nextMapperExpression, destExpression.Type));
        }
    }
}
