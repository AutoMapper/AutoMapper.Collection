using System;
using System.Linq.Expressions;

namespace AutoMapper.EquivalencyExpression
{
    public interface IEquivalentComparer
    {
        int GetHashCode(object obj);
        bool IsEquivalent(object source, object destination);
    }

    public interface IEquivalentComparer<TSource, TDestination> : IEquivalentComparer
    {
        Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource destination);
    }
}