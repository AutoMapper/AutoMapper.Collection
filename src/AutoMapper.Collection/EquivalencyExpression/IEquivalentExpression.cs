using System;
using System.Linq.Expressions;

namespace AutoMapper.EquivalencyExpression
{
    public interface IEquivalentExpression
    {
        
    }
    public interface IEquivalentExpression<TSource, TDestination> : IEquivalentExpression
    {
        bool IsEquivalent(TSource source, TDestination destination);
        Expression<Func<TDestination, bool>> ToSingleSourceExpression(TSource destination);
    }
}