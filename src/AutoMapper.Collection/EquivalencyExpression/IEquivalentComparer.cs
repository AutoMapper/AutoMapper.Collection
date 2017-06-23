using System;
using System.Linq.Expressions;

namespace AutoMapper.EquivalencyExpression
{
    public interface IEquivalentComparer
    {
        int GetHashCode(object obj);
    }

    public interface IEquivalentComparer<TSource, TDestination> : IEquivalentComparer
    {
        bool IsEquivalent(TSource source, TDestination destination);
        Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource destination);
    }
}