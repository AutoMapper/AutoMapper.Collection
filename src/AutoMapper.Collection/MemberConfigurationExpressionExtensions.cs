using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Collection;

public static class MemberConfigurationExpressionExtensions
{
    /// <summary>
    /// Maps children collection from source to destination with recycling of existing destination objects.
    /// </summary>
    /// <param name="expression">Member configuration expression</param>
    /// <param name="sourceMemberGetter">Function retrieving destination collection from destination object</param>
    /// <typeparam name="TSource">Type of source object</typeparam>
    /// <typeparam name="TSourceChild">Type of source object&apos;s child</typeparam>
    /// <typeparam name="TDestination">Type of destination object</typeparam>
    /// <typeparam name="TDestProp">Type of destination object</typeparam>
    /// <typeparam name="TDestChild">Type of destination object&apos;s child</typeparam>
    public static void MapWithOrdinalRecycling<TSource, TSourceChild, TDestination, TDestProp, TDestChild>(
        this IMemberConfigurationExpression<TSource, TDestination, IList<TDestChild>> expression,
        Func<TSource, IList<TSourceChild>> sourceMemberGetter)
        where TDestProp : IList<TDestChild>
    {
        expression.MapFrom((src, _, destinationList, ctx) =>
        {
            var mapper = ctx.Mapper;
            var source = sourceMemberGetter(src);
            
            var itemsToRecycle = Math.Min(destinationList.Count, source.Count);
            
            var destinationToKeep = destinationList
                .Take(itemsToRecycle)
                .Select((dstChild, i) => mapper.Map(source[i], dstChild));
            var destinationToAdd = source
                .Skip(itemsToRecycle)
                .Select(o => mapper.Map<TDestChild>(o))
                .ToList();
            return destinationToKeep.Concat(destinationToAdd).ToList();
        });
    }

    /// <summary>
    /// Maps children collection from source to destination with recycling of existing destination objects.
    /// </summary>
    /// <param name="expression">Member configuration expression</param>
    /// <param name="sourceMemberGetter">Function retrieving destination collection from destination object</param>
    /// <typeparam name="TSource">Type of source object</typeparam>
    /// <typeparam name="TSourceChild">Type of source object&apos;s child</typeparam>
    /// <typeparam name="TDestination">Type of destination object</typeparam>
    /// <typeparam name="TDestChild">Type of destination object&apos;s child</typeparam>
    public static void MapWithOrdinalRecycling<TSource, TSourceChild, TDestination, TDestChild>(
        this IMemberConfigurationExpression<TSource, TDestination, IList<TDestChild>> expression,
        Func<TSource, IList<TSourceChild>> sourceMemberGetter)
    {
        MapWithOrdinalRecycling<TSource, TSourceChild, TDestination, IList<TDestChild>, TDestChild>(expression, sourceMemberGetter);
    }
}