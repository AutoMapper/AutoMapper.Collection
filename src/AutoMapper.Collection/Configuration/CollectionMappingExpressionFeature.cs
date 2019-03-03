using System;
using System.Linq.Expressions;
using AutoMapper.Collection.Execution;
using AutoMapper.EquivalencyExpression;

namespace AutoMapper.Collection.Configuration
{
    public class CollectionMappingExpressionFeature<TSource, TDestination> : IMappingExpressionFeature
    {
        private readonly Expression<Func<TSource, TDestination, bool>> _expression;

        public CollectionMappingExpressionFeature(Expression<Func<TSource, TDestination, bool>> expression)
        {
            _expression = expression;
        }

        public void Configure(TypeMap typeMap)
        {
            var equivalentExpression = new EquivalentExpression<TSource, TDestination>(_expression);
            typeMap.Features.Add(new CollectionMappingFeature(equivalentExpression));
        }

        public IMappingExpressionFeature Reverse()
        {
            var reverseExpression = Expression.Lambda<Func<TDestination, TSource, bool>>(_expression.Body, _expression.Parameters[1], _expression.Parameters[0]);
            return new CollectionMappingExpressionFeature<TDestination, TSource>(reverseExpression);
        }
    }
}
