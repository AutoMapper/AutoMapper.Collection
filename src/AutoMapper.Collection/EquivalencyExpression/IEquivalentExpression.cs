using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.EquivalencyExpression
{
    public interface IEquivalentExpression
    {
    }

    public interface IEquivalentExpression<TSource, TDestination> : IEquivalentExpression
    {
        bool IsEquivalent(TSource source, TDestination destination);

        TDestinationItem Map<TSourceItem, TDestinationItem>(TSourceItem source, TDestinationItem destination, ResolutionContext context)
            where TSourceItem : IEnumerable<TSource>
            where TDestinationItem : class, ICollection<TDestination>;

        Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource destination);

    }
}
