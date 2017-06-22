using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Collection.Internal.Mapper
{
    public class ExpressionCollectionMapper<TSource, TDestination> : ICollectionMapper<TSource, TDestination>
    {
        private readonly Func<TDestination, int> _destinationPropertyGetHashCodeFunc;
        private readonly Func<TSource, TDestination, bool> _propertyEqualFunc;
        private readonly Func<TSource, int> _sourcePropertyGetHashCodeFunc;

        public ExpressionCollectionMapper(ICollectionExpression<TSource, TDestination> collectionExpression)
        {
            _sourcePropertyGetHashCodeFunc = collectionExpression.SourceHashCodeExpression.Compile();
            _destinationPropertyGetHashCodeFunc = collectionExpression.DestinationHashCodeExpression.Compile();

            EquivalentExpression = collectionExpression.EqualExpression;
            _propertyEqualFunc = EquivalentExpression.Compile();
        }

        public Expression<Func<TSource, TDestination, bool>> EquivalentExpression { get; }

        public TDestinationItem Map<TSourceItem, TDestinationItem>(TSourceItem source, TDestinationItem destination, ResolutionContext context)
            where TSourceItem : IEnumerable<TSource>
            where TDestinationItem : class, ICollection<TDestination>
        {
            if (source == null || destination == null)
            {
                return destination;
            }

            var destList = destination.ToLookup(x => _destinationPropertyGetHashCodeFunc(x)).ToDictionary(x => x.Key, x => x.ToList());

            var items = source.Select(x =>
            {
                var sourceHash = _sourcePropertyGetHashCodeFunc(x);

                var item = default(TDestination);
                List<TDestination> itemList;
                if (destList.TryGetValue(sourceHash, out itemList))
                {
                    item = itemList.FirstOrDefault(dest => _propertyEqualFunc(x, dest));
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
                    destination.Add((TDestination)context.Mapper.Map(keypair.SourceItem, null, typeof(TSource), typeof(TDestination), context));
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
    }
}
