using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public class EquivlentExpressionAddRemoveCollectionMapper : IObjectMapper
    {
        private readonly ConcurrentDictionary<Type, MethodCacheItem> _methodCache = new ConcurrentDictionary<Type, MethodCacheItem>();
        private readonly CollectionMapper _collectionMapper = new CollectionMapper();

        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            if (context.IsSourceValueNull || context.DestinationValue == null)
                return _collectionMapper.Map(context, mapper);

            var sourceElementType = TypeHelper.GetElementType(context.SourceType);
            var destinationElementType = TypeHelper.GetElementType(context.DestinationType);
            var equivilencyExpression = GetEquivilentExpression(context);

            var destEnumerable = context.DestinationValue as IEnumerable;
            var sourceEnumerable = context.SourceValue as IEnumerable;

            var destItems = destEnumerable.Cast<object>().ToList();
            var sourceItems = sourceEnumerable.Cast<object>().ToList();
            var compareSourceToDestination = sourceItems.ToDictionary(s => s, s => destItems.FirstOrDefault(d => equivilencyExpression.IsEquivlent(s, d)));

            var actualDestType = destEnumerable.GetType();
            var methodItem = _methodCache.GetOrAdd(actualDestType, t =>
            {
                var addMethod = actualDestType.GetMethod("Add");
                var removeMethod = actualDestType.GetMethod("Remove");
                return new MethodCacheItem
                {
                    Add = (e, o) => addMethod.Invoke(e, new[] {o}),
                    Remove = (e, o) => removeMethod.Invoke(e, new[] {o})
                };
            });

            foreach (var keypair in compareSourceToDestination)
            {
                if (keypair.Value == null)
                {
                    methodItem.Add(destEnumerable, Mapper.Map(keypair.Key, sourceElementType, destinationElementType));
                }
                else
                {
                    Mapper.Map(keypair.Key, keypair.Value, sourceElementType, destinationElementType);
                }
            }

            foreach (var removedItem in destItems.Except(compareSourceToDestination.Values))
            {
                methodItem.Remove(destEnumerable, removedItem);
            }

            return destEnumerable;
        }

        public bool IsMatch(ResolutionContext context)
        {
            return context.SourceType.IsEnumerableType()
                   && context.DestinationType.IsCollectionType()
                   && GetEquivilentExpression(context) != null;
        }

        private static IEquivilentExpression GetEquivilentExpression(ResolutionContext context)
        {
            return EquivilentExpressions.GetEquivilentExpression(TypeHelper.GetElementType(context.SourceType), TypeHelper.GetElementType(context.DestinationType));
        }

        private class MethodCacheItem
        {
            public Action<IEnumerable, object> Add { get; set; }
            public Action<IEnumerable, object> Remove { get; set; }
        }
    }
}
