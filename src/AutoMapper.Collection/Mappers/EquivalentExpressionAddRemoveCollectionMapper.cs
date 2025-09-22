using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Collection;
using AutoMapper.EquivalencyExpression;
using AutoMapper.Internal;
using AutoMapper.Internal.Mappers;
using static System.Linq.Expressions.Expression;
using static AutoMapper.Execution.ExpressionBuilder;

namespace AutoMapper.Mappers
{
    public class EquivalentExpressionAddRemoveCollectionMapper : IObjectMapper
    {
        private readonly CollectionMapper _collectionMapper = new CollectionMapper();

        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, IEquivalentComparer equivalentComparer)
            where TSource : IEnumerable<TSourceItem>
            where TDestination : ICollection<TDestinationItem>
        {
            if (source == null || destination == null)
            {
                return destination;
            }

            // Build a lookup of existing destination items by the equivalency hash
            var destLookup = destination.ToLookup(x => equivalentComparer.GetHashCode(x)).ToDictionary(x => x.Key, x => x.ToList());

            // We'll collect the items in the exact order of the source, preserving existing instances
            var ordered = new List<TDestinationItem>();
            var toAdd = new List<TDestinationItem>();

            foreach (var src in source)
            {
                var sourceHash = equivalentComparer.GetHashCode(src);

                TDestinationItem match = default;
                if (destLookup.TryGetValue(sourceHash, out var candidates))
                {
                    match = candidates.FirstOrDefault(dest => equivalentComparer.IsEquivalent(src, dest));
                    if (match != null)
                    {
                        // Reserve this destination instance and update it
                        candidates.Remove(match);
                        context.Mapper.Map(src, match);
                        ordered.Add(match);
                        continue;
                    }
                }

                // No match found: create a new destination item
                var newItem = (TDestinationItem)context.Mapper.Map(src, null, typeof(TSourceItem), typeof(TDestinationItem));
                toAdd.Add(newItem);
                ordered.Add(newItem);
            }

            // Remove any remaining destination items that were not matched
            foreach (var removedItem in destLookup.SelectMany(x => x.Value))
            {
                destination.Remove(removedItem);
            }

            // Ensure all new items are part of the destination collection before reordering
            foreach (var add in toAdd)
            {
                if (!destination.Contains(add))
                {
                    destination.Add(add);
                }
            }

            // Reorder destination to match the 'ordered' sequence while preserving the collection instance
            if (destination is IList<TDestinationItem> list)
            {
                var oc = list as System.Collections.ObjectModel.ObservableCollection<TDestinationItem>;
                for (int i = 0; i < ordered.Count; i++)
                {
                    var target = ordered[i];
                    if (i < list.Count && ReferenceEquals(list[i], target))
                    {
                        continue;
                    }

                    // Find the current index of the target item, if it exists
                    int currentIndex = -1;
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        if (ReferenceEquals(list[j], target))
                        {
                            currentIndex = j;
                            break;
                        }
                    }

                    if (currentIndex >= 0)
                    {
                        if (oc != null)
                        {
                            // Use Move to raise a single Move event
                            oc.Move(currentIndex, i);
                        }
                        else
                        {
                            // Move existing item to the desired index
                            var item = list[currentIndex];
                            list.RemoveAt(currentIndex);
                            list.Insert(i, item);
                        }
                    }
                    else
                    {
                        // Insert the new item at the correct position
                        list.Insert(i, target);
                    }
                }

            }
            else
            {
                // Fallback for non-IList collections: remove and re-add in order
                // Note: This may lose ordering guarantees for certain collection types that don't preserve insertion order
                // but provides best-effort behavior.
                var existing = destination.ToList();
                foreach (var item in existing)
                {
                    destination.Remove(item);
                }
                foreach (var item in ordered)
                {
                    destination.Add(item);
                }
            }

            return destination;
        }

        private static readonly MethodInfo _mapMethodInfo = typeof(EquivalentExpressionAddRemoveCollectionMapper).GetRuntimeMethods().Single(x => x.IsStatic && x.Name == nameof(Map));
        private static readonly ConcurrentDictionary<TypePair, IObjectMapper> _objectMapperCache = new ConcurrentDictionary<TypePair, IObjectMapper>();

        public bool IsMatch(TypePair typePair)
        {
            return typePair.SourceType.IsEnumerableType()
                   && typePair.DestinationType.IsCollectionType();
        }

        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap,
            Expression sourceExpression, Expression destExpression)
        {
            var sourceType = TypeHelper.GetElementType(sourceExpression.Type);
            var destType = TypeHelper.GetElementType(destExpression.Type);

            var equivalencyExpression = this.GetEquivalentExpression(sourceType, destType, configurationProvider);
            if (equivalencyExpression == null)
            {
                var typePair = new TypePair(sourceExpression.Type, destExpression.Type);
                return _objectMapperCache.GetOrAdd(typePair, _ =>
                {
                    var mappers = new List<IObjectMapper>(configurationProvider.GetMappers());
                    for (var i = mappers.IndexOf(this) + 1; i < mappers.Count; i++)
                    {
                        var mapper = mappers[i];
                        if (mapper.IsMatch(typePair))
                        {
                            return mapper;
                        }
                    }
                    return _collectionMapper;
                })
                .MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression);
            }

            var method = _mapMethodInfo.MakeGenericMethod(sourceExpression.Type, sourceType, destExpression.Type, destType);
            var map = Call(null, method, sourceExpression, destExpression, ContextParameter, Constant(equivalencyExpression));

            var notNull = NotEqual(destExpression, Constant(null));
            var collectionMapperExpression = _collectionMapper.MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression);
            return Condition(notNull, map, Convert(collectionMapperExpression, destExpression.Type));
        }
    }
}
