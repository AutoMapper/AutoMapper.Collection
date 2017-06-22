using System;
using System.Linq.Expressions;

namespace AutoMapper.Collection.Internal
{
    public interface ICollectionExpression<TSource, TDestination>
    {
        Expression<Func<TDestination, int>> DestinationHashCodeExpression { get; }
        Expression<Func<TSource, TDestination, bool>> EqualExpression { get; }
        Expression<Func<TSource, int>> SourceHashCodeExpression { get; }
    }
}
