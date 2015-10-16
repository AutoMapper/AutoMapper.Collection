using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using AutoMapper;
using AutoMapper.EquivilencyExpression;
using AutoMapper.Mappers;

namespace AutoMapperTest.EF7
{
    public class CollectionTestUpdated : CollectionTestBase
    {
        public class EquivlentExpressionAddRemoveCollectionMapper : IObjectMapper
        {
            private readonly ConcurrentDictionary<Type, MethodCacheItem> _methodCache = new ConcurrentDictionary<Type, MethodCacheItem>();

            public object Map(ResolutionContext context, IMappingEngineRunner mapper)
            {
                if (context.IsSourceValueNull && mapper.ShouldMapSourceCollectionAsNull(context))
                {
                    return null;
                }

                var sourceEnumerable = ((IEnumerable)context.SourceValue ?? new object[0])
                    .Cast<object>()
                    .ToList();

                var sourceElementType = TypeHelper.GetElementType(context.SourceType);
                var destinationElementType = TypeHelper.GetElementType(context.DestinationType);
                var equivilencyExpression = GetEquivilentExpression(context);

                var destEnumerable = (IEnumerable)(context.DestinationValue as IEnumerable ?? ObjectCreator.CreateList(destinationElementType));

                var destItems = destEnumerable.Cast<object>().ToList();
                var compareSourceToDestination = sourceEnumerable.ToDictionary(s => s, s => destItems.FirstOrDefault(d => equivilencyExpression.IsEquivlent(s, d)));

                var actualDestType = destEnumerable.GetType();
                var methodItem = _methodCache.GetOrAdd(actualDestType, t =>
                {
                    var addMethod = actualDestType.GetMethod("Add");
                    var removeMethod = actualDestType.GetMethod("Remove");
                    return new MethodCacheItem
                    {
                        Add = (e, o) => addMethod.Invoke(e, new[] { o }),
                        Remove = (e, o) => removeMethod.Invoke(e, new[] { o })
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
}