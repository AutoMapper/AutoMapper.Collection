using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.Collection
{
    public interface ICollectionMapper
    {
    }

    public interface ICollectionMapper<TSource, TDestination> : ICollectionMapper
    {
        Expression<Func<TSource, TDestination, bool>> EquivalentExpression { get; }

        TDestinationItem Map<TSourceItem, TDestinationItem>(TSourceItem source, TDestinationItem destination, ResolutionContext context)
            where TSourceItem : IEnumerable<TSource>
            where TDestinationItem : class, ICollection<TDestination>;
    }
}
